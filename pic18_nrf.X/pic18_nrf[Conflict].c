/* 
 * File:   newmain.h
 * Author: Anand
 *
 * Created on August 23, 2015, 1:07 AM
 */
 
/*
 * 
 */ 

 #pragma config FOSC = INTIO67, MCLRE = OFF, WDTEN = OFF, BOREN = OFF, IESO = OFF

#include <pic18f24k20.h> 

#define SYS_FREQ 	(1000000L)

#include "hardware.h"
#include "nrf24.h"
void init_hardware();

    unsigned char txdat[nRF_PACKET_SIZE];
   unsigned char rxdat[nRF_PACKET_SIZE];
int main() {


    int x = 0;
    
    init_hardware();
    
    init_spi();
    init_nrf();///////////////
    while(1)
    { 
        
        
        x++;
        if(x==10000)
        {        
            x = 0;
            LED2 = !LED2;
            LED1 = !LED1;
            
            tx_mode();
            txdat[0] = 'A';
            txdat[1] = '1';
            TXpayload (txdat);
            
        }
    }
    
    
}



/////////hardware init task
void init_hardware()
{
    ADCON0bits.ADON = 0;
    CM1CON0bits.C1ON = 0;
    CM2CON0bits.C2ON = 0;
    ///Direction init LED
    LED1Tris = 0;
    LED2Tris = 0;
    ///Direction init nRF 
    MOSI_TRIS     = 0;
    MISO_TRIS     = 1;
    SCK_TRIS      = 0;
    CSN_TRIS      = 0;
    CE_TRIS       = 0;
  
    //SPI Reg Init
    SPI_EN = 0;
    //SSPCON1bits.
    
//    SSPSTAT = 0x00;
}

/////////////Init nRF Module

void init_nRF()
{
        CE = 0;
        CSN = 0;
        SSPBUF = 0x00;
}