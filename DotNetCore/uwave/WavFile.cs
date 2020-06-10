using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;

namespace uwave
{
    public static class WavFile
    {
        public static WavAudio LoadFile(string filepath)
        {
            WavAudio wav = new WavAudio();

            using (BinaryReader reader = new BinaryReader(new FileStream(filepath, FileMode.Open)))
            {
                wav.ChunkID = reader.ReadUInt32();
                wav.WaveSize = reader.ReadUInt32();
                wav.WaveId = reader.ReadUInt32();
                wav.FmtId = reader.ReadUInt32();
                wav.FmtSize = reader.ReadUInt32();
                wav.AudioFormat = reader.ReadUInt16();
                wav.NumChannels = reader.ReadUInt16();
                wav.SampleFrequency = reader.ReadUInt32();
                wav.ByteRate = reader.ReadUInt32();
                wav.SampleAlign = reader.ReadUInt16();
                wav.BitsPerSample = reader.ReadUInt16();
                wav.DataId = reader.ReadUInt32();
                wav.DataBytes = reader.ReadUInt32();

                // TODO: validate values

                UInt32 sample_cnt = wav.DataBytes / (UInt32)(wav.NumChannels * wav.BitsPerSample / 8);

                wav.SetSampleCount(sample_cnt, wav.NumChannels);

                switch(wav.BitsPerSample)
                {
                    case 8:
                        Load8bitData(reader, wav, sample_cnt);
                        break;
                    case 16:
                        Load16bitData(reader, wav, sample_cnt);
                        break;
                    default:
                        // unrecognized sample size
                        break;
                }
            }
         
            return wav;
        }

        public static bool SaveFile(string filename, WavAudio wav)
        {
            using (BinaryWriter writer = new BinaryWriter(File.Open(filename, FileMode.OpenOrCreate)))
            {
                writer.Write(wav.ChunkID);
                writer.Write(wav.WaveSize);
                writer.Write(wav.WaveId);
                writer.Write(wav.FmtId);
                writer.Write(wav.FmtSize);
                writer.Write(wav.AudioFormat);
                writer.Write(wav.NumChannels);
                writer.Write(wav.SampleFrequency);
                writer.Write(wav.ByteRate);
                writer.Write(wav.SampleAlign);
                writer.Write(wav.BitsPerSample);
                writer.Write(wav.DataId);
                writer.Write(wav.DataBytes);

                for (UInt32 i = 0; i < wav.SampleCount; i++)
                {  
                    if (wav.BitsPerSample == 16)
                    {
                        writer.Write(wav.GetSample(i, 0));  // TODO: stereo
                    }     
                    else
                    {
                        writer.Write((byte)(wav.GetSample(i, 0)));  // TODO: stereo
                    }  
                }
                writer.Close();
            } 

            return true;
        }


        private static void Load8bitData(BinaryReader reader, WavAudio wav, UInt32 samples)
        {
            byte sample8;

            for (UInt32 i = 0; i < samples; i++)
            {
                for (UInt16 channel = 0; channel < wav.NumChannels; channel++)
                {
                    sample8 = reader.ReadByte();
                    wav.SetSample(i, channel, sample8);
                }
            }
        }

        private static void Load16bitData(BinaryReader reader, WavAudio wav, UInt32 samples)
        {
            Int16 sample16;

            for (UInt32 i = 0; i < samples; i++)
            {
                for (UInt16 channel = 0; channel < wav.NumChannels; channel++)
                {
                    sample16 = reader.ReadInt16();
                    wav.SetSample(i, channel, sample16);
                }
            }
        }

        public static string BigEndianToString(UInt32 value)
        {
            string str = "";
            str += Convert.ToChar(value & 0x000000FF);
            str += Convert.ToChar((value & 0x0000FF00) >> 8);
            str += Convert.ToChar((value & 0x00FF0000) >> 16);
            str += Convert.ToChar((value & 0xFF000000) >> 24);

            return str;
        }


        public static string FormatName(UInt16 audioFormat)
        {
            string formatName = String.Format("Unknown ()", audioFormat);
            switch (audioFormat)
            {
                case 0:
                    formatName = "Unknown"; break;
                case 1:
                    formatName = "PCM"; break;
                case 2:
                    formatName = "ADPCM"; break;
                case 3:
                    formatName = "IEE Float"; break;
                case 6:
                    formatName = "ALAW"; break;
                case 7:
                    formatName = "MULAW"; break;
                default:
                    formatName =  String.Format("Compressed ()", audioFormat); ; break;
            }
            return formatName;
        }

        public static bool SaveDFT(string filepath, int binCount, double deltaFreq, Complex[] binData)
        {
            using (StreamWriter writer = new StreamWriter( new FileStream(filepath, FileMode.Create)))
            {
                writer.WriteLine(String.Format("{0}, {1}", binCount, deltaFreq));

                for (int i = 0; i < binCount; i++)
                {
                    writer.WriteLine(binData[i].Real);
                }
                writer.Flush();
            }
            return true;
        }

        public static bool SaveMFCC(string filepath, UInt32 frameCount, UInt32 frameSize, UInt32 frameStride, UInt32 cepstra, List<double[]> mfccList)
        {
            using (StreamWriter writer = new StreamWriter(new FileStream(filepath, FileMode.Create)))
            {
                writer.WriteLine(String.Format("{0}, {1}, {2}, {3}", frameCount, frameSize, frameStride, cepstra));

                for (int i = 0; i < mfccList.Count; i++)
                {
                    writer.WriteLine(mfccList[i][0].ToString());
                    for (int c = 1; c <= cepstra; c++)
                    {
                        writer.Write(",");
                        writer.WriteLine(mfccList[i][c].ToString());
                    }
                    writer.WriteLine("");
                }
                writer.Flush();
            }
            return true;
        }

        private static UInt16 ReverseBytes(UInt16 value)
        {
            return (UInt16)((value & 0xFFU) << 8 | (value & 0xFF00U) >> 8);
        }


    }
}