/* 
 * File:   hardware.h
 * Author: Anand
 *
 * Created on August 22, 2015, 1:27 PM
 */

#ifndef HARDWARE_H
#define	HARDWARE_H

#ifdef	__cplusplus
extern "C" {
#endif

     
#define _XTAL_FREQ	(16000000L)
#include <xc.h>
    
     
    ///////LED1 Direction & State
#define LED1        LATCbits.LC7
#define LED1Tris    TRISCbits.RC7
    ///////LED2 Direction & State
#define LED2        LATCbits.LC6
#define LED2Tris    TRISCbits.RC6
    
    //SW1 Direction and Read
#define SW1_Tris    TRISBbits.TRISB5
#define SW1         PORTBbits.RB5
    
    //SW1 Direction and Read    
#define SW2_Tris    TRISBbits.TRISB4
#define SW2         PORTBbits.RB4
     
///RELAY IO
#define R1_TRIS    TRISBbits.RB3
#define R2_TRIS    TRISBbits.RB2 
#define R3_TRIS    TRISBbits.RB1
#define R4_TRIS    TRISBbits.RB0    

#define R1    LATBbits.LATB3
#define R2    LATBbits.LATB2 
#define R3    LATBbits.LATB1
#define R4   LATBbits.LATB0    
#define BUZZER   LATBbits.LATB2         
    
#define ZEROCROSS  PORTBbits.RB0 
    
///spi enable bit 
//////////////////////////
#define CSN_TRIS    TRISCbits.RC4
#define CE_TRIS     TRISCbits.RC0  
 #define MOSItris    TRISCbits.RC5
#define MISOtris     TRISCbits.RC2    
 #define SCKtris    TRISCbits.RC1
      
#define CSN         LATCbits.LATC4
#define CE          LATCbits.LATC0
    
#define MOSI         LATCbits.LATC5
#define MISO         PORTCbits.RC2
#define SCK         LATCbits.LATC1 
 
////PIR Sensor TRIS
#define PIR_TRIS TRISAbits.TRISA2
#define PIR PORTAbits.RA2
 //Digital IO In
#define INIT_SW_IO    {PIR_TRIS = 1; INTCON2bits.nRBPU = 0;   WPUBbits.WPUB5 = 1; WPUBbits.WPUB4 = 1; SW1_Tris =1; SW2_Tris = 1;}  
 //Init LEDSs
#define INIT_LED_IO   {LED1Tris = 0; LED2Tris = 0;}       
 //Init SPI IO 
#define INIT_SPI_IO   {CSN_TRIS = 0; CE_TRIS = 0; MOSItris = 0; MISOtris = 1; SCKtris = 0;}      
//Init RELAY IO
#ifdef triac
    #define INIT_RELAY    {R1_TRIS = 0; R2_TRIS = 0; R3_TRIS = 0; R4_TRIS = 1; }
#endif

#ifdef relay
    #define INIT_RELAY    {R1_TRIS = 0; R2_TRIS = 0; R3_TRIS = 0; R4_TRIS = 0; }  
#endif
#define INIT_IO {INIT_SW_IO; INIT_LED_IO; INIT_SPI_IO; INIT_RELAY;}
    void CSNtoggle();       //Toggles CSN Pin
    void delay_ms(int);
#ifdef	__cplusplus
}
#endif

#endif	/* HARDWARE_H */

