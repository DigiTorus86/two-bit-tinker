`timescale 1ns / 1ps
//////////////////////////////////////////////////////////////////////////////////
// Company: 
// Engineer: 
// 
// Create Date: 06/14/2019 06:14:22 PM
// Design Name: 
// Module Name: clk44100
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


module tick_generator 
    #(parameter CYCLES_PER_TICK = 2267) // 44100 Hz default
    (    
    input clk,
    input reset,
    output tick
    );
    
    reg  [11:0] counter = 0; 
    reg  [11:0] counter_next; 
    
    always_ff @(posedge clk, posedge reset)
    if (reset)
        counter <= 0;
    else
        counter <= counter_next;
    
    assign counter_next = (counter < CYCLES_PER_TICK-1) ? counter + 1 : 8'd0;
    assign tick = (counter == CYCLES_PER_TICK-1) ? 1'b1 : 1'b0;
endmodule
