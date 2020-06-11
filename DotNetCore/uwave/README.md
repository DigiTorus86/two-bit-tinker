# uwave (microwave)
<p>
Simple command-line tool for analyzing or generating WAV files.  Useful for determining the fundamental frequency of a WAV file or generating WAV files with a specific waveform, length, and frequency.
</p>
<p>
Testing from within VS Code:<br>
dotnet run --analyze samples/outsine.wav --verbose<br>
dotnet run -a samples/outsquare.wav --numframes 2048<br>
dotnet run -a samples/outsaw.wav --numframes 512<br>
dotnet run -a samples/output.wav<br>
dotnet run -g samples/outsine.wav --frequency 880 --duration 500 --rate 22050<br>
dotnet run -g samples/outsquare.wav --waveform square --frequency 220 <br>
dotnet run -g samples/outsaw.wav --waveform saw --frequency 660<br>
dotnet run -g samples/outnoise.wav --waveform noise<br>
dotnet run --generate samples/output.wav --frequency 880 --duration 500 --rate 22050 --analyze samples/output.wav<br> 
</p>
<p>
After building, replace dotnet run with:<br>
Linux:   ./uwave<br>
Windows: uwave.exe<br> 
</p>
