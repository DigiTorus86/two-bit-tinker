using System;

namespace uwave
{
    public struct WavHeader 
    {
        // RIFF Header
        public UInt32 RiffHeader;  // "RIFF" big endian
        public UInt32 WavSize; // Size of the wav portion of the file, which follows the first 8 bytes. File size - 8
        public UInt32 WaveHeader;  // "WAVE" big endian
        // Format Header subchunk
        public UInt32 FmtHeader;  // "fmt " (with trailing space) big endian
        public UInt32 FmtChunkSize; // Should be 16 for PCM
        public UInt16 AudioFormat;  // Should be 1 for PCM. 3 for IEEE Float
        public UInt16 NumChannels;  // default to Mono
        public UInt32 SampleRate;    // Require 11025 (0x2B11)
        public UInt32 ByteRate; // Bytes per second = sample_rate * num_channels * Bytes Per Sample
        public UInt16 SampleAlignment; // = num_channels * Bytes Per Sample
        public UInt16 BitsPerSample;
        
        // Data sub-chunk
        public UInt32 DataHeader;  // "data" big endian
        public UInt32 DataBytes; //  = sample count * num_channels * sample byte size
        // uint8_t bytes[]; // Remainder of wav file is sample data bytes
    }
}