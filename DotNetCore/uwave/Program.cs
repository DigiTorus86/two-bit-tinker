/*
uwave (microwave) WAV file generator and analyzer console app
Requires .NET Core 3.1

Testing from within VS Code:
dotnet run --analyze Samples/ding.wav --verbose
dotnet run -a samples/10kHz_44100Hz_16bit_05sec.wav
dotnet run -a samples/440Hz_44100Hz_16bit_05sec.wav --numframes 2048
dotnet run -a samples/100Hz_44100Hz_16bit_05sec.wav --numframes 512
dotnet run -a samples/output.wav
dotnet run -g samples/outsine.wav --frequency 880 --duration 500 --rate 22050
dotnet run -g samples/outsquare.wav --waveform square --frequency 220 
dotnet run -g samples/outsaw.wav --waveform saw --frequency 660 
dotnet run -g samples/outnoise.wav --waveform noise 
dotnet run --generate samples/output.wav --frequency 880 --duration 500 --rate 22050 --analyze samples/output.wav 

Copyright (c) 2020 Paul Pagel
This is free software; see the license.txt file for more information.
There is no warranty; not even for merchantability or fitness for a particular purpose.
*/

using System;
using System.IO;
using System.Numerics;

namespace uwave
{
    class Program
    {
        private static string[] _waveforms = {"sine", "square", "saw", "noise"};

        static void Main(string[] args)
        {
            // Parameter variables and default values
            bool   generate = false;
            bool   analyze = false;
            string srcFilename = "";
            string destFilename = "";
            UInt32 frameSize = 1024;
            UInt32 duration = 1000;    // 1 second
            UInt32 sampleRate = 44100; // CD audio is 44.1KHz
            WAVETYPES waveType = WAVETYPES.SINE;
            double frequency = 440.0;  // A4
            bool   verbose = false;

            // Parse and validate the command line arguments

            if (args.Length == 0)
            {
                Console.WriteLine("Please specify an action and filename.");
                ShowHelp();
                return;
            }

            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == "-h" || args[i] == "--help")
                {
                    ShowHelp();
                    return;    
                }

                if (args[i] == "-a" || args[i] == "--analyze")
                {
                    if (args.Length <= i + 1)
                    {
                        Console.WriteLine("Please specify an input wav filename to analyze.");
                        return;
                    }
                    analyze = true; 
                    srcFilename = args[i + 1];
                    if (!File.Exists(srcFilename))  
                    {
                        Console.WriteLine("Input filename does not exist.");
                        return; 
                    }
                }

                if (args[i] == "-g" || args[i] == "--generate")
                {
                    if (args.Length <= i + 1)
                    {
                        Console.WriteLine("Please specify an output wav filename to generate.");
                        return;
                    }
                    generate = true; 
                    destFilename = args[i + 1];
                }

                if (args[i] == "-f" || args[i] == "--frequency")
                {
                    if (args.Length <= i + 1)
                    {
                        Console.WriteLine("Please specify an output frequency.");
                        return;
                    }
                    if (!double.TryParse(args[i + 1], out frequency))
                    {
                        Console.WriteLine("Invalid frequency specified.");
                        return;
                    }
                }

                if (args[i] == "-d" || args[i] == "--duration")
                {
                    if (args.Length <= i + 1)
                    {
                        Console.WriteLine("Please specify an output duration in milliseconds.");
                        return;
                    }
                    if (!UInt32.TryParse(args[i + 1], out duration))
                    {
                        Console.WriteLine("Invalid duration specified.");
                        return;
                    }
                }

                if (args[i] == "-r" || args[i] == "--rate")
                {
                    if (args.Length <= i + 1)
                    {
                        Console.WriteLine("Please specify an output sample rate in samples per second.");
                        return;
                    }
                    if (!UInt32.TryParse(args[i + 1], out sampleRate))
                    {
                        Console.WriteLine("Invalid sample rate specified.");
                        return;
                    }
                }

                if (args[i] == "-w" || args[i] == "--waveform")
                {
                    if (args.Length <= i + 1)
                    {
                        Console.WriteLine("Please specify the output waveform (sine, square, saw, noise).");
                        return;
                    }
                    for(int k = 0; k < _waveforms.Length; k++)
                    {
                        if (_waveforms[k].Equals(args[i + 1]))
                        {
                            waveType = (WAVETYPES)k;
                            Console.WriteLine("Waveform: {0}", waveType);
                        }
                    }
                }

                if (args[i] == "-n" || args[i] == "--numframes")
                {
                    if (args.Length <= i + 1)
                    {
                        Console.WriteLine("Please specify the frame count for analysis.");
                        return;
                    }
                    if (!UInt32.TryParse(args[i + 1], out frameSize) || frameSize < 4)
                    {
                        Console.WriteLine("Invalid frame count specified.");
                        return;
                    }
                }

                if (args[i] == "-v" || args[i] == "--verbose")
                {
                    verbose = true;
                }
            } 

            // Perform the requested action(s)

            if (generate)
            {
                WavAudio wav = WavForm.GenerateAudio(waveType, sampleRate, frequency, duration);
                if (WavFile.SaveFile(destFilename, wav))
                {
                    Console.WriteLine("Created {0}", destFilename);
                }
                else
                {
                    Console.WriteLine("ERROR: Unable to create {0}", destFilename);
                }

            }

            if (analyze)
            {
                if (verbose)
                {
                    System.Console.WriteLine("Analyzing {0}", srcFilename);
                }
                AnalyzeWav(srcFilename,  frameSize, verbose); 
                return;
            }

        }

        private static void ShowHelp()
        {
            Console.WriteLine("Usage:  uwave [options]");
            Console.WriteLine("");
            Console.WriteLine("Options");
            Console.WriteLine("  -h, --help     \t Displays help for this command.");
            Console.WriteLine("  -a, --analyze  \t Analyze the following wav filename.");
            Console.WriteLine("  -g, --generate \t Generate the following wav filename.");
            Console.WriteLine("  -f  --frequency\t Frequency to generate.");
            Console.WriteLine("  -d  --duration \t Duration in milliseconds to generate. Default is 1000 (ms).");
            Console.WriteLine("  -r  --rate     \t Sample rate per second to use for generation. Default is 44100");
            Console.WriteLine("  -w  --waveform \t Waveform to generation. (sine, square, saw)  Default is sine.");
            Console.WriteLine("  -n  --numframes\t DFT frame count to use for analysis. Default is 1024.");
            Console.WriteLine("  -v  --verbose  \t Verbose results output.");
            return;
        }

        private static void AnalyzeWav(string wavFilename, UInt32 frameSize, bool verbose)
        {
            WavAudio wav = WavFile.LoadFile(wavFilename);
            Complex[] dftBins = Dft.CalculateDFT(wav, frameSize);  
            double deltaF = wav.SampleFrequency / frameSize;
            int maxPowerIdx = 1;

            for (int i = 0; i < dftBins.Length / 2; i++)
            {
                if (verbose)
                {
                    Console.Write(dftBins[i].Real.ToString());
                    Console.Write(", ");
                }
                if (dftBins[i].Real > dftBins[maxPowerIdx].Real)
                {
                    maxPowerIdx = i; // new max value
                }
            }

            if (verbose)
            {
                Console.WriteLine("");
                Console.WriteLine("Delta Freq:   {0:F1} Hz", deltaF);
            }
            Console.WriteLine("Primary Freq: {0} - {1} Hz", maxPowerIdx * deltaF, (maxPowerIdx + 1) * deltaF);
            return;
        }
    }
}
