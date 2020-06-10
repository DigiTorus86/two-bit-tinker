using System;
using System.Collections.Generic;
using System.Numerics;

namespace uwave
{
    public class MelFreq
    {
        public UInt32 SampleFrequency { get; set; }

        public UInt32 WindowLengthMs { get; set; }

        public UInt32 WindowLength { get; set; }

        public UInt32 FrameStrideMs { get; set; }

        public UInt32 FrameStride { get; set; }

        public UInt32 Cepstra { get; set; }

        public UInt32 Nfft { get; set; }

        public UInt32 FftBins { get; set; }

        public UInt32 Filters { get; set; }

        public double PreEmphasisCoef { get; set; }

        public double LowFreq { get; set; }

        public double HighFreq { get; set; }

        public double[] Frame { get; set; }

        public Complex[] Fourier { get; set; }

        public double[] PowerSpectralCoef { get; set; }

        public double[] LmfbCoef { get; set; }

        public double[] Hamming { get; set; }

        public double[] Mfcc { get; set; }

        public double[][] Filterbank { get; set; }

        public double[][] Dct { get; set; }

        public List<double[]> SampleMFCC { get; set; }

        public MelFreq(UInt32 sampleFrequency)
        {
            // Set defaults
            SampleFrequency = sampleFrequency;
            WindowLengthMs = 25;
            WindowLength = WindowLengthMs * SampleFrequency / 1000;
            FrameStrideMs = 10;
            FrameStride = FrameStrideMs * SampleFrequency / 1000;
            Cepstra = 12;
            Filters = 40;
            PreEmphasisCoef = 0.97;
            LowFreq = 50;
            HighFreq = 5500;
            Nfft = (UInt32)(SampleFrequency > 20000 ? 512 : 2048);
            FftBins = Nfft / 2 + 1;

            PowerSpectralCoef = new double[WindowLength - FrameStride];

            SampleMFCC = new List<double[]>();

            InitFilterbank();
            InitHammingDct();
            //CalculateTwiddle();
        }

        public void CalculateMFCC(WavAudio wav)
        {
            // TODO:  validation checks on WAV audio sample

            SampleMFCC.Clear();
            UInt32 frameStart = 0;      

            while (frameStart < wav.SampleCount)
            {
                ProcessFrame(wav, frameStart);
                frameStart += FrameStride;
            }

        }

        void ProcessFrame(WavAudio wav, UInt32 frameStart)
        {
            // Initialize frame with Pre-Emphasis and Hamming Window

            Frame = new double[Math.Max(WindowLength, Nfft)];
            Frame[0] = Hamming[0] * wav.GetSample(frameStart, 0);

            for (UInt32 i = 0; i < WindowLength; i++)
                Frame[i] = Hamming[1] * (wav.GetSample(frameStart + i, 0) - PreEmphasisCoef * wav.GetSample(frameStart + i - 1, 0));

            CalculatePowerSpectrum();
            ApplyLMFB();
            ApplyDCT();

            SampleMFCC.Add(Mfcc);
        }


        /// <summary>
        /// Computes the Periodogram estimate of the power spectrum.
        /// </summary>
        private void CalculatePowerSpectrum()
        {
            PowerSpectralCoef = new double[Nfft];

            // Convert frame to Complex
            Complex[] frameComp = new Complex[Frame.Length];
            for (int i = 0; i < Frame.Length; i++)
                frameComp[i] = new Complex(Frame[i], 0);

            Complex[] fft = Dft.CalculateDFT(frameComp);

            for (int i = 0; i < FftBins; i++)
                PowerSpectralCoef[i] = Math.Pow(fft[i].Magnitude, 2) / WindowLength; // Magnitude is the same as Abs
        }

        /// <summary>
        /// Apply the Log Mel FilterBank
        /// </summary>
        private void ApplyLMFB()
        {
            LmfbCoef = new double[Filters];

            for (int i = 0; i < Filters; i++)
            {
                // Multiply the filterbank matrix
                for (int j = 0; j < Filterbank[i].Length; j++)
                    LmfbCoef[i] += Filterbank[i][j] * PowerSpectralCoef[j];
                
                // Apply Mel-flooring
                if (LmfbCoef[i] < 1.0)
                    LmfbCoef[i] = 1.0;
            }

            // Applying log on amplitude
            for (int i = 0; i < Filters; i++)
                LmfbCoef[i] = Math.Log(LmfbCoef[i]);
        }

        
        /// <summary>
        /// Apply the Discrete Cosine Transform
        /// </summary>
        void ApplyDCT()
        {
            Mfcc = new double[Cepstra + 1]; 

            for (int i = 0; i <= Cepstra; i++)
            {
                for (int j = 0; j < Filters; j++)
                    Mfcc[i] += Dct[i][j] * LmfbCoef[j];
            }
        }


        /// <summary>
        /// Converts a frequency in Hertz to Mels
        /// </summary>
        /// <param name="freq"></param>
        /// <returns></returns>
        public static double HertzToMel(double freq)
        {
            return 2595 * Math.Log10(1 + freq / 700);
        }

        /// <summary>
        /// Converts a a frequency in Mels to Hertz
        /// </summary>
        /// <param name="mel"></param>
        /// <returns></returns>
        public static double MelToHertz(double mel)
        {
            return 700 * (Math.Pow(10, mel / 2595) - 1);
        }


        private void InitFilterbank()
        {
            // Convert low and high frequencies to Mel scale
            double lowFreqMel = HertzToMel(LowFreq);
            double highFreqMel = HertzToMel(HighFreq);

            // Calculate filter centre-frequencies
            double[] filterCentreFreq = new double[Filters * 2];
           
            for (int i = 0; i < Filters + 2; i++)
                filterCentreFreq[i] = MelToHertz(lowFreqMel + (highFreqMel - lowFreqMel) / (Filters + 1) * i);

            // Calculate FFT bin frequencies
            double[] fftBinFreq = new double[FftBins];
            
            for (int i = 0; i < FftBins; i++)
                fftBinFreq[i] = SampleFrequency / 2.0 / (FftBins - 1) * i;

            // Filterbank: Allocate memory
            Filterbank = new double[Filters][];

            // Populate the Filterbank matrix
            for (int filt = 1; filt <= Filters; filt++)
            {
                double[] ftemp = new double[FftBins];
                for (int bin = 0; bin < FftBins; bin++)
                {
                    double weight;
                    if (fftBinFreq[bin] < filterCentreFreq[filt - 1])
                        weight = 0;
                    else if (fftBinFreq[bin] <= filterCentreFreq[filt])
                        weight = (fftBinFreq[bin] - filterCentreFreq[filt - 1]) / (filterCentreFreq[filt] - filterCentreFreq[filt - 1]);
                    else if (fftBinFreq[bin] <= filterCentreFreq[filt + 1])
                        weight = (filterCentreFreq[filt + 1] - fftBinFreq[bin]) / (filterCentreFreq[filt + 1] - filterCentreFreq[filt]);
                    else
                        weight = 0;
                    ftemp[bin] = weight;
                }
                Filterbank[filt - 1] = ftemp;
            }
        }

        private void InitHammingDct()
        {
            int i, j;

            Hamming = new double[WindowLength];

            for (i = 0; i < WindowLength; i++)
                Hamming[i] = 0.54 - 0.46 * Math.Cos(2 * Math.PI * i / (WindowLength - 1));

            double[] v1 = new double[Cepstra + 1];
            double[] v2 = new double[Filters];

            for (i = 0; i <= Cepstra; i++)
                v1[i] = i;

            for (i = 0; i < Filters; i++)
                v2[i] = i + 0.5;

            Dct = new double[Cepstra + 1][];
            double c = Math.Sqrt(2.0 / Filters);

            for (i = 0; i <= Cepstra; i++)
            {
                double[] dtemp = new double[Filters];
                for (j = 0; j < Filters; j++)
                    dtemp[j] = (c * Math.Cos(Math.PI / Filters * v1[i] * v2[j]));
                Dct[i] = dtemp;
            }
        }

       
    }
}