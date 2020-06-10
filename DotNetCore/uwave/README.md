# uwave (microwave)
Simple command-line tool for analyzing or generating WAV files.  

Testing from within VS Code:
dotnet run --analyze samples/outsine.wav --verbose
dotnet run -a samples/outsquare.wav --numframes 2048
dotnet run -a samples/outsaw.wav --numframes 512
dotnet run -a samples/output.wav
dotnet run -g samples/outsine.wav --frequency 880 --duration 500 --rate 22050
dotnet run -g samples/outsquare.wav --waveform square --frequency 220 
dotnet run -g samples/outsaw.wav --waveform saw --frequency 660 
dotnet run -g samples/outnoise.wav --waveform noise 
dotnet run --generate samples/output.wav --frequency 880 --duration 500 --rate 22050 --analyze samples/output.wav 

After building, replace dotnet run with: 
Linux:   ./uwave  
Windows: uwave.exe 
