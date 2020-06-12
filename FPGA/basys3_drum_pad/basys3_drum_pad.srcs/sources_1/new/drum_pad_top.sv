`timescale 1ns / 1ps
//////////////////////////////////////////////////////////////////////////////////
// Company:  Two Bit Tinker :: https://twobittinker.com  
// Engineer: Paul Pagel
// 
// Create Date: 06/15/2019 08:07:39 PM
// Module Name: drum_pad_top
// Project Name:   4 Instrument Drum Pad
// Target Devices: Basys3
// Tool Versions:  Vivado 2019.1
// Description: Drum pad for playing percussion audio samples
//      Center button:  reset
//      Bottom button:  kick drum
//      Right button:   snare drum
//      Left button:    closed hat
//      Top button:     open hat
//      Switch 0:       enable wav playback (place high play)
//      Switch 1:       PmodAMP2 shutdown (high to play)
//      Switch 2:       PmodAMP2 gain (low = louder, high = softer
// 
// Dependencies:  PmodAMP2 in JC bottom row
// 
// Revision:
// Revision 0.01 - File Created
// Additional Comments:
// 
//////////////////////////////////////////////////////////////////////////////////


module drum_pad_top(
    input clk,
    input reset,
    input [15:0]  sw, 
    input btnU, btnD, btnL, btnR,
    
    output AMP_ain,
    output AMP_gain,
    output AMP_shutdown_n,
    
    output [15:0] led,
    output  [6:0] seg,
    output  [3:0] an
    );
    
    // signals
  
    wire [15:0] wav_kick_dout;
    wire [15:0] wav_snar_dout;
    wire [15:0] wav_chat_dout;
    wire [15:0] wav_ohat_dout;
    
    wire kick_playing;
    wire snar_playing;
    wire chat_playing;
    wire ohat_playing;
    
    reg signed [31:0] pcm_mixed;
 
    // structural
    
    wav_player_kick u_wav_player_kick (
        .clk(clk),
        .reset(btnD | reset),
        .en(sw[0]),
        .pcm_out(wav_kick_dout),
        .playing(kick_playing)
    );
    
    wav_player_snare u_wav_player_snare (
        .clk(clk),
        .reset(btnR | reset),
        .en(sw[0]),
        .pcm_out(wav_snar_dout),
        .playing(snar_playing)
    );
    
    wav_player_ohat u_wav_player_ohat (
        .clk(clk),
        .reset(btnU | reset),
        .en(sw[0]),
        .pcm_out(wav_ohat_dout),
        .playing(ohat_playing)
    );
    
    wav_player_chat u_wav_player_chat (
        .clk(clk),
        .reset(btnL | reset),
        .en(sw[0]),
        .pcm_out(wav_chat_dout),
        .playing(chat_playing)
    );
    
    // instantiate 1-bit dac for driving PModAMP2 output
   ds_1bit_dac #(.W(16)) u_1bit_dac( 
        .clk(clk), 
        .reset(reset),
        .pcm_in(pcm_mixed[15:0]),
        .pdm_out(AMP_ain)
    );
      
    assign AMP_shutdown_n = sw[1];
    assign AMP_gain = sw[2]; // 0 = 12 dB high gain
    
    assign pcm_mixed = ($signed(wav_kick_dout) + $signed(wav_snar_dout) + $signed(wav_chat_dout) + $signed(wav_ohat_dout)) / 4; 
       
    assign led[0] = kick_playing;
    assign led[1] = snar_playing;
    assign led[2] = ohat_playing;
    assign led[3] = chat_playing;
endmodule
