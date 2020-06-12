`timescale 1ns / 1ps
//////////////////////////////////////////////////////////////////////////////////
// Company:  Two Bit Tinker
// Engineer: Paul Pagel
// 
// Create Date: 06/15/2019 08:23:47 PM
// Design Name: 
// Module Name: wav_player_kick
// Description: Emits Pulse Code Modulation audio data at the configured sample rate.
//              Open high hat audio data is pre-loaded via a .coe file into the block memory.  
//              
// Dependencies: 
// 
// Revision:
// Revision 0.01 - File Created
// Additional Comments:
// 
//////////////////////////////////////////////////////////////////////////////////


module wav_player_kick(
    input clk,
    input reset,
    input en,               // enable playback
    output [15:0] pcm_out,  // the audio sample data
    output playing          // high while the WAV is playing
    );
    
    // # of 16bit audio samples in the .coe file
    localparam [13:0] WAV_LENGTH = 14'd12011;
    
    wire sample_tick;
    reg  [13:0] wav_addr;
    reg  [13:0] wav_addr_next = 14'd0;
    reg  addr_changed = 0;
    logic [15:0] wav_douta;
    
    // 44100 Hz tick generator for audio sample timing
    tick_generator #( .CYCLES_PER_TICK(2267)) u_tick_gen_44100 (
        .clk(clk),
        .reset(reset),
        .tick(sample_tick)  // output high every 1/44100 of a second
    );
    
    blk_mem_gen_kick u_blk_mem_kick (
      .clka(clk),       // input wire clka
      .ena(1'b1),       // input wire enable
      .wea(1'b0),       // input wire [0 : 0] write enable (not used)
      .addra(wav_addr), // input wire [13 : 0] addra
      .dina(16'b0),     // data in (not used)
      .douta(wav_douta)   // audio pcm data out
    );
    
    // playback read address register
    always_ff @(posedge clk, posedge reset)  begin
        if (reset) begin
            wav_addr <= 14'd0;
        end else begin
            wav_addr <= wav_addr_next;
        end
    end
    
    // next state logic
    always_comb begin
        if (sample_tick && en) begin
            wav_addr_next = (wav_addr < WAV_LENGTH) ?  wav_addr + 1 : WAV_LENGTH;
        end else begin
            wav_addr_next = wav_addr;
        end
    end
    
    // output logic 
    assign pcm_out = (en & playing) ? wav_douta : 16'd0;
    assign playing = ((wav_addr < WAV_LENGTH) && en);
endmodule
