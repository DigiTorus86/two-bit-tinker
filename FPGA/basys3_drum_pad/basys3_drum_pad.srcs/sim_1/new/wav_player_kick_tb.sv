`timescale 1ns / 1ps
//////////////////////////////////////////////////////////////////////////////////
// Company: 
// Engineer: 
// 
// Create Date: 06/15/2019 10:21:24 PM
// Design Name: 
// Module Name: wav_player_kick_tb
// Project Name: 
// Target Devices: 
// Tool Versions: 
// Description: 
// 
// Dependencies: 
// 
// Revision:
// Revision 0.01 - File Created
// Additional Comments:
// 
//////////////////////////////////////////////////////////////////////////////////


module wav_player_kick_tb();

logic clk;
    logic reset;
    logic en;
    logic [15:0] pcm_dout;
    logic playing;

    wav_player_kick u_wav_player_kick (
        .clk(clk),
        .reset(reset),
        .en(en),
        .pcm_out(pcm_dout),
        .playing(playing)
    );
    
    initial 
    begin
        reset = 1'b1;
        en = 1'b0;
        #60;
        reset = 1'b0;
        en = 1'b1;
    end
    
    always
    begin
        clk = 1'b1;
        #10;
        clk = 1'b0;
        #10;
    end
    
endmodule
