using System;

namespace uwave
{
    
    public class WavAudio
    {
        public const UInt32 WAV_HDR_RIFF      = 0x46464952;  // "RIFF" big endian
        public const UInt32 WAV_HDR_WAVE      = 0x45564157;  // "WAVE" big endian
        public const UInt32 WAV_HDR_FMT       = 0x20746D66;  // "fmt " big endian (with trailing space)
        public const UInt32 WAV_HDR_DATA      = 0x61746164;  // "data" big endian
        public const UInt32 WAV_PCM_SIZE      = 16;          // PCM format chunk size 
        public const UInt16 WAV_FMT_PCM       = 1;
        public const Int16  WAV_SILENCE_16BIT = 0;
        public const Int16  WAV_SILENCE_8BIT  = 128;  
        public const Int16  WAV_CHNL_LEFT     = 0;
        public const Int16  WAV_CHNL_RIGHT    = 1;

        public const UInt32 WAV_HDR_SIZE      = 36;  // header size not including the first 8 bytes 


        protected Int16[,] _samples;
        protected UInt32  _sampleCount;
        protected UInt16  _bitsPerSample;
        protected UInt16  _bytesPerSample;
        protected Int16   _silence = 128;  // 8-bit silence = 128, 16 bit silence = 0;
        protected Int16   _minValue = 0;   // 8-bit min volume value
        protected Int16   _maxValue = 255; // 8-bit max volume value

        // "RIFF" header
        public UInt32 ChunkID { get; set; }  

        // Size of the wav portion of the file, which follows the first 8 bytes. File size - 8
        public UInt32 WaveSize { get; set; }

        // "WAVE" big endian
        public UInt32 WaveId { get; set; }  

        // "fmt " (with trailing space) big endian
        public UInt32 FmtId { get; set; }

        // Should be 16 for PCM
        public UInt32 FmtSize { get; set; }

        // Should be 1 for PCM. 3 for IEEE Float 
        public UInt16 AudioFormat { get; set; }

        public UInt16 NumChannels { get; set; }

        public UInt32 SampleFrequency { get; set; }

        public UInt32 ByteRate { get; set; }

        public UInt16 SampleAlign { get; set; }

        public Int16 Silence { get => _silence; }

        public UInt16 BitsPerSample {
            get => _bitsPerSample;
            
            set
            {
                _bitsPerSample = value;
                _bytesPerSample = (UInt16)(_bitsPerSample / 8);
                _silence = (value == 8) ? WAV_SILENCE_8BIT : WAV_SILENCE_16BIT;
                _minValue= (value == 8) ? (short)0 : (short)-16000;
                _maxValue= (value == 8) ? (short)255 : (short)16000;
            }
        }

        public UInt32 DataId { get; set; }

        public UInt32 DataBytes { get; set; }

        public UInt32 SampleCount 
        { 
            get
            {
                return _sampleCount;
            }
        }


        public WavAudio()
        {
            // Set default header values
            ChunkID = WAV_HDR_RIFF;
            WaveSize = 0;
            WaveId = WAV_HDR_WAVE;
            FmtId = WAV_HDR_FMT;
            FmtSize = WAV_PCM_SIZE;
            AudioFormat = WAV_FMT_PCM;
            NumChannels = 1;  // MONO
            SampleFrequency = 22050;
            BitsPerSample = 16;
            ByteRate = SampleFrequency * NumChannels * BitsPerSample / 8;
            SampleAlign = (UInt16)(NumChannels * (BitsPerSample >> 3));

            DataId = WAV_HDR_DATA;
            DataBytes = 0;
        }

  
        public Int16 GetSample(UInt32 index, UInt16 channel)
        {
            if (index < _sampleCount && channel < this.NumChannels)
                return _samples[index, channel];
            else
                return _silence;
        }

        public void SetSample(UInt32 index, UInt16 channel, Int16 sample)
        {
            if (index < _sampleCount && channel < this.NumChannels)
            {
                _samples[index, channel] = sample;
            }
        }

        public void SetSampleCount(UInt32 count, UInt16 channels)
        {
            _sampleCount = count;
            _samples = new Int16[count, channels];
            DataBytes = count * channels * _bytesPerSample;
            WaveSize = DataBytes + WAV_HDR_SIZE;  
        }

        public bool GenerateSine(double frequency)
        {
            if (frequency <= 0) return false;

            double two_pi_scaled = 2 * Math.PI * frequency / (double)this.SampleFrequency;
            for(int i = 0; i < SampleCount; i++) 
            {
                _samples[i, 0] = (Int16)((double)_maxValue * Math.Sin((double)i * two_pi_scaled));
            }
            return true;
        }

        public bool GenerateSquare(double frequency)
        {
            if (frequency <= 0) return false;

            int samplesPerHalfCycle = (int)(SampleFrequency / (frequency * 2));
            int cycleSamples = 0;
            Int16 waveValue = Silence;

            for(int i = 0; i < SampleCount; i++) 
            {
                _samples[i, 0] = waveValue;
                cycleSamples += 1;

                if (cycleSamples >= samplesPerHalfCycle)
                {
                    // Toggle value from min to max or max to min
                    waveValue = (waveValue == _maxValue) ? _minValue : _maxValue;
                    cycleSamples = 0;
                }
            }
            return true;
        }

        public bool GenerateSaw( double frequency)
        {
            if (frequency <= 0) return false;

            int samplesPerCycle = (int)(SampleFrequency /frequency);
            double deltaFreq = (_maxValue - _minValue) / (double)samplesPerCycle;
            int cycleSamples = 0;

            Console.WriteLine("Saw wave form");
            Console.WriteLine("  Frequency: {0}", frequency);
            Console.WriteLine("  Cycle:     {0}", samplesPerCycle);
            Console.WriteLine("  Delta:     {0}", deltaFreq);


            for(int i = 0; i < SampleCount; i++) 
            {
                _samples[i, 0] = (Int16)(_maxValue - (deltaFreq * cycleSamples));

                cycleSamples += 1;
                if (cycleSamples >= samplesPerCycle)
                {
                    cycleSamples = 0;
                }
            }
            return true;
        }

        public bool GenerateNoise()
        {
            Console.WriteLine("Noise wave form");

            Random rand = new Random(); 
            double mean = 0;
            double stdDev = _maxValue;
            double u1, u2;
            double randStdNormal, randNormal;
            Int16 n = 0;

            for(int i = 0; i < SampleCount; i++) 
            {
                u1 = 1.0 - rand.NextDouble(); 
                u2 = 1.0 - rand.NextDouble();
                randStdNormal = Math.Sqrt(-2.0 * Math.Log(u1)) *
                        Math.Sin(2.0 * Math.PI * u2); //random normal(0,1)
                randNormal = mean + stdDev * randStdNormal; //random normal(mean, stdDev^2)
                n = (Int16)(randNormal);
                //Console.Write("{0} : {1}, ", randStdNormal, n);
                _samples[i, 0] = n;
            }
            //Console.WriteLine("");
            return true;
        }

        
    }
}
