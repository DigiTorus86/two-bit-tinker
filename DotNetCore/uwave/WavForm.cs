using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;

namespace uwave
{
    public enum WAVETYPES { SINE, SQUARE, SAW, NOISE};

    public static class WavForm
    {
        public static WavAudio GenerateAudio(WAVETYPES waveType, UInt32 sampleRateHz, double frequency, UInt32 durationMS) 
        {
            UInt16 sampleCnt = (UInt16)(sampleRateHz * durationMS / 1000);

            WavAudio wav = new WavAudio();
            wav.FmtSize = WavAudio.WAV_PCM_SIZE;
            wav.AudioFormat = WavAudio.WAV_FMT_PCM;
            wav.NumChannels = 1;
            wav.SampleFrequency = sampleRateHz;
            wav.BitsPerSample = 16;
            wav.ByteRate = (UInt32)(wav.SampleFrequency * wav.NumChannels * (wav.BitsPerSample >> 3));
            wav.SampleAlign = (UInt16)(wav.NumChannels * (wav.BitsPerSample >> 3));
            wav.SetSampleCount(sampleCnt, 1);

            switch(waveType)
            {
                case WAVETYPES.SINE:
                    wav.GenerateSine(frequency);
                    break;
                case WAVETYPES.SQUARE:
                    wav.GenerateSquare(frequency);
                    break;
                case WAVETYPES.SAW:
                    wav.GenerateSaw(frequency);
                    break;
                case WAVETYPES.NOISE:
                    wav.GenerateNoise();
                    break;

                default:  // Silence
                    break;
            }

            return wav;
        }

    }
}