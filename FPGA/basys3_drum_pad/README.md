# 4 Instrument Drum Pad for the Basys3 FPGA
<p>
Tool Versions:  Vivado 2019.1<br>
Description: Drum pad for playing percussion audio samples<br>
      Center button:  reset<br>
      Bottom button:  kick drum<br>
      Right button:   snare drum<br>
      Left button:    closed hat<br>
      Top button:     open hat<br>
      Switch 0:       enable wav playback (place high play)<br>
      Switch 1:       PmodAMP2 shutdown (high to play)<br>
      Switch 2:       PmodAMP2 gain (low = louder, high = softer<br>
</p>
<p>
Dependencies:  PmodAMP2 in JC bottom row<br>
Speaker hooked up to PmodAMP2 output jack<br>
</p>
<p>
Should work in other versions of Vivado or other similar PMod-equipped FPGA boards with minimal modifications.
</p>
<p>
Currently does not contain all project folders (cache, hw, runs, sims) as this blows the full project up to 70mb.  If you really want them, post a comment at https://twobittinker.com or create an issue on the repo and I'll respond. 
</p>
