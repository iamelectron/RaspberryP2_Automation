/* 
 * File:   newmain.h
 * Author: Anand
 *
 * Created on August 23, 2015, 1:07 AM
 */
 
/*
 * 
 */ 
#define bit_test(Var, Pos)  ((Var) & (1 << (Pos)))

 
#include <pic18f24k20.h> 

// PIC18F24K20 Configuration Bit Settings

// 'C' source line config statements

#include <xc.h>

// #pragma config statements should precede project file includes.
// Use project enums instead of #define for ON and OFF.

// CONFIG1H
#pragma config FOSC = INTIO67   // Oscillator Selection bits (Internal oscillator block, port function on RA6 and RA7)
#pragma config FCMEN = OFF      // Fail-Safe Clock Monitor Enable bit (Fail-Safe Clock Monitor disabled)
#pragma config IESO = OFF       // Internal/External Oscillator Switchover bit (Oscillator Switchover mode disabled)

// CONFIG2L
#pragma config PWRT = OFF       // Power-up Timer Enable bit (PWRT disabled)
#pragma config BOREN = OFF      // Brown-out Reset Enable bits (Brown-out Reset disabled in hardware and software)
#pragma config BORV = 18        // Brown Out Reset Voltage bits (VBOR set to 1.8 V nominal)

// CONFIG2H
#pragma config WDTEN = OFF      // Watchdog Timer Enable bit (WDT is controlled by SWDTEN bit of the WDTCON register)
#pragma config WDTPS = 32768    // Watchdog Timer Postscale Select bits (1:32768)

// CONFIG3H
#pragma config CCP2MX = PORTC   // CCP2 MUX bit (CCP2 input/output is multiplexed with RC1)
#pragma config PBADEN = OFF     // PORTB A/D Enable bit (PORTB<4:0> pins are configured as digital I/O on Reset)
#pragma config LPT1OSC = OFF    // Low-Power Timer1 Oscillator Enable bit (Timer1 configured for higher power operation)
#pragma config HFOFST = ON      // HFINTOSC Fast Start-up (HFINTOSC starts clocking the CPU without waiting for the oscillator to stablize.)
#pragma config MCLRE = OFF      // MCLR Pin Enable bit (RE3 input pin enabled; MCLR disabled)

// CONFIG4L
#pragma config STVREN = ON      // Stack Full/Underflow Reset Enable bit (Stack full/underflow will cause Reset)
#pragma config LVP = OFF        // Single-Supply ICSP Enable bit (Single-Supply ICSP enabled)
#pragma config XINST = OFF      // Extended Instruction Set Enable bit (Instruction set extension and Indexed Addressing mode disabled (Legacy mode))

// CONFIG5L
#pragma config CP0 = OFF        // Code Protection Block 0 (Block 0 (000800-001FFFh) not code-protected)
#pragma config CP1 = OFF        // Code Protection Block 1 (Block 1 (002000-003FFFh) not code-protected)

// CONFIG5H
#pragma config CPB = OFF        // Boot Block Code Protection bit (Boot block (000000-0007FFh) not code-protected)
#pragma config CPD = OFF        // Data EEPROM Code Protection bit (Data EEPROM not code-protected)

// CONFIG6L
#pragma config WRT0 = OFF       // Write Protection Block 0 (Block 0 (000800-001FFFh) not write-protected)
#pragma config WRT1 = OFF       // Write Protection Block 1 (Block 1 (002000-003FFFh) not write-protected)

// CONFIG6H
#pragma config WRTC = OFF       // Configuration Register Write Protection bit (Configuration registers (300000-3000FFh) not write-protected)
#pragma config WRTB = OFF       // Boot Block Write Protection bit (Boot Block (000000-0007FFh) not write-protected)
#pragma config WRTD = OFF       // Data EEPROM Write Protection bit (Data EEPROM not write-protected)

// CONFIG7L
#pragma config EBTR0 = OFF      // Table Read Protection Block 0 (Block 0 (000800-001FFFh) not protected from table reads executed in other blocks)
#pragma config EBTR1 = OFF      // Table Read Protection Block 1 (Block 1 (002000-003FFFh) not protected from table reads executed in other blocks)

// CONFIG7H
#pragma config EBTRB = OFF      // Boot Block Table Read Protection bit (Boot Block (000000-0007FFh) not protected from table reads executed in other blocks)

 ////////////////////////////define your device type and id
#define triac
//#define relay                    //////need to change according to your device
#define device_id 21           //////need to change according to your device
///////////////////////////////////////////////////////////
  
#define device_type 1         //////need to change according to your device
///Device type declaration for program 
/*   c# device_type (declare device type accordingly
         public enum device_type : byte
        {
            Test_Device = 0,
            Triac_Device = 1,
            Relay_Device = 2,
            Buzzer_Device = 3,
            Motor_Device = 4
        };
 */

///possible packet types
#define add_me 1 
#define update_packet 3
#define remove_packet 4
#define data_packet 2

#include "hardware.h"
#include "nrf24.h" 
//////////Definitions and Reg Initializations//////////////
void interrupt Trigger_Triac();  
void init_hardware(void);
int rxdt(void);
void send_init(void);
void send_data(char* bytes,  unsigned char pack_type); 

/////Registers and Inits//////
#define GLOBAL_INTERRUPT INTCONbits.GIE  
#define ENABLE_INT0 INTCONbits.INT0E 
#define ZERO_CROSS INTCONbits.INT0IF 

#define TIMER0_INTERRUPT INTCONbits.TMR0IE
#define TIMER_OVER_FLOW INTCONbits.TMR0IF
#define TIMER_ON_OFF T0CONbits.TMR0ON
#define TIMER_MODE  T0CONbits.T08BIT        // =1 8bit otherwise 16bit
//#define TIMER_PRESCALE_256 T0CON = T0CON | 0b00000111
#define TIMER_CONFIG T0CON = T0CON & 0b11010111;
 
//#define TMR1IF PIR1bits.TMR1IF
//#define TIMER1_ON_OFF T1CONbits.TMR1ON
#define INIT_INTERRUPT { GLOBAL_INTERRUPT = 1; ENABLE_INT0 = 1; TIMER0_INTERRUPT = 1; TIMER_MODE = 1; TIMER_CONFIG;}
//#define CONFIG_TIMER1 {T1CON = 0b00110000; PIE1bits.TMR1IE = 1;}

//Need to make RA3 as input, this is due to an error in the circuit drawing
//ADC channel is not present on pin RA4 so make RA4 as input and short RA4 and RA3 will do the job
#define INIT_ADC {ADCON2 = 0b01111010; ADCON1 = 0x00; TRISAbits.TRISA0 =1; TRISAbits.TRISA4 = 1; TRISAbits.TRISA3 = 1; ADCON0bits.ADON = 1;}
#define SELECT_LDR {ADCON0bits.CHS3 =0; ADCON0bits.CHS2 =0; ADCON0bits.CHS1 =0; ADCON0bits.CHS0 =0;}
#define SELECT_TEMP  {ADCON0bits.CHS3 =0; ADCON0bits.CHS2 =0; ADCON0bits.CHS1 =1; ADCON0bits.CHS0 =1;}
#define ADC_START  ADCON0bits.GO_nDONE = 1
#define ADC_CHECK  ADCON0bits.GO_nDONE
///////////////////////////////////////////////////////////


//////////Global Variables and Constants//////////////////
   unsigned char txdat[nRF_PACKET_SIZE];
   unsigned char rxdat[nRF_PACKET_SIZE];
   unsigned char TRIGGER_DELAY = 0;
   unsigned char device_1=0, device_2=0, device_3=0, device_4=0;
////////////////////////////////////////////
    
///////////////Interrupt Service Routines///////////
   void interrupt Trigger_Triac()
   { 
       if(ZERO_CROSS)
       { 
           ZERO_CROSS = 0;
           TMR0L = 255-TRIGGER_DELAY; 
           TIMER_ON_OFF = 1;  
       }
       
       if(TIMER_OVER_FLOW)
       { 
           R3 = 1; 
           TIMER_ON_OFF = 0;
           TIMER_OVER_FLOW = 0;
           __delay_us(15);
           R3 = 0; 
       }
       
    //   if(TMR1IF)
    //   {
    //       R1 = 1; 
    //      TIMER1_ON_OFF = 0;
    //      TMR1IF = 0;
    //       __delay_us(15); 
    //       R1 = 0;
           
     //  }
   } 
///////////////////////////////////////////////////// 
 void delay_us(long del)
   {
       del = del*10;
       while(del)
       {
           __delay_us(1);
           del--;
       }
   }
   ///////////////////////////////////////////////// Main /////////////////////////////////////
int main() {
    //Select 16Mhz Freq
    OSCCONbits.IRCF0 = 1;
    OSCCONbits.IRCF1 = 1;   
    OSCCONbits.IRCF2 = 1;    
    /////////
    delay_ms(1000); //2 Seconds Delay before starting
    init_hardware();
    TRIGGER_DELAY = 139;
    init_spi();///////////////  
    init_nrf();   
    LED1 = 1; 
    LED2 = 1;  
#ifdef relay
    R1 = 0;
    R2 = 0;
    R3 = 0;
    R4 = 0; 
#endif
    delay_ms(1000);   
  //  while(1)
  //  {
 //       TRIGGER_DELAY++;
  //      if(TRIGGER_DELAY>150)
 //           TRIGGER_DELAY = 0;
 //       __delay_ms(10);
 //   }  
    
    while(1)
    {        
            if(rxdt())
            {
                // R1 = !R1;
                ///Data received    
               // rx_mode();   
            }
            
            //if sw1 pressed send addme packet
             if(!SW1)
             {
                 send_init();
             }
             
            ///Send remove me packet
             if(!SW2)
             { 
                char ch[] = {0x55, 0x55, 0x55, 0x55, 0x55, 0x55}; 
                send_data(ch,remove_packet); 
                delay_ms(500);
             }       
 
        }
    
    
}
/////////////////////////////////////////////////////End of Main/////////////////////////////
void send_init(void)
{           
             LED1 = 0;            
             tx_mode();
             //packet header
             txdat[0] = add_me; txdat[1] = device_id; txdat[2] = device_type;
             //Since its an 'add me' packet rest is useless
             txdat[3] = 0x00; 
#ifdef triac
             txdat[4] = TRIGGER_DELAY; txdat[5] = device_2; txdat[6] = device_3; txdat[7] = device_4; txdat[8] = 9; txdat[9] = 10;      
#endif
#ifdef relay
             txdat[4] = device_1; txdat[5] = device_2; txdat[6] = device_3; txdat[7] = device_4; txdat[8] = 9; txdat[9] = 10;      
#endif
              TXpayload (txdat);
            // LED2 = 1;  //very fast blinking (1 = led off, 0 = on)) 
            delay_ms(500);
            rx_mode();  
            LED1 = 1; 
}

///Send Bytes function
void send_data(char* bytes, unsigned char pack_type)
{
             LED2 = 0; 
             tx_mode();
             //packet header
             txdat[0] = pack_type; txdat[1] = device_id; txdat[2] = device_type; 
             txdat[3] =  0x00;              //future usez
             ////Sensor data or other information from slave
             txdat[4] =  bytes[0]; txdat[5] =  bytes[1]; txdat[6] =  bytes[2]; 
             txdat[7] =  bytes[3]; txdat[8] =  bytes[4];  txdat[9] =  bytes[5];  
             TXpayload (txdat); 
             rx_mode();  
             LED2 = 1;  //very fast blinking (1 = led off, 0 = on)) 
}



/////////hardware init task
void init_hardware()
{      
    //Disable ADC and Comparators 
    CCP1CON = 0x00; 
    ///Disable Comparators
    CM1CON0  = 0b00000111;
    CM2CON0  = 0b00000111;
    ANSEL = 0x09;
    ANSELH = 0x00;    
    INIT_ADC;
    
    ///This will ENABLE INT0 (Triac Zero Cross Detection)
    
#ifdef triac
    INIT_INTERRUPT;
    //CONFIG_TIMER1;
#endif

#ifdef relay
   // GLOBAL_INTERRUPT = 0;
#endif
    //INIT ALL IOs
    INIT_IO;  
    init_nrf();
}

int count = 0;

/////////////RX nRF Module
int rxdt()
{ 
   unsigned char  tmp ;
   unsigned char adc1 = 0, adc2=0;
   char ch1[] = {0x55, 0x55, 0x55, 0x55, 0x55, 0x55}; 
   CSN = 0;
            tmp = xmitbyte(NOOP);
   CSN = 1;  

        if(bit_test(tmp,6))
        { 
            
            LED1 = 0;
            RXpayload(rxdat); 
            CSN = 0;
            xmitbyte(W_REGISTER + STATUS);
            xmitbyte(STATUS_DEFAULT_VAL | STATUS_RX_DR);
            CSN = 1;   
             
            nrf_FLUSH_RX();        
            if(rxdat[1]==device_id)
            { 
                switch((rxdat[0]))
                { 
                    case data_packet:
#ifdef triac
                        TRIGGER_DELAY = rxdat[4];   //Trigger angle variable
                        BUZZER = rxdat[5]; 
                        LED1 =  rxdat[5]; 
                        R1 = rxdat[6];              ///Triac 2 on/off
                         
                        
                        device_1 = rxdat[4];
                        device_2 = rxdat[5];
                        device_3 = rxdat[6];
                        device_4 = rxdat[7];
#endif
#ifdef relay      
                    R1 = rxdat[4];
                    R2 = rxdat[5]; 
                    R3 = rxdat[6];
                    R4 = rxdat[7];
                    
                    device_1 = rxdat[4];
                    device_2 = rxdat[5];
                    device_3 = rxdat[6];
                    device_4 = rxdat[7];
                    
#endif 
                    break;
                    
                    case update_packet:
                        if(rxdat[4] == 0xFF)            //Respond to scan request of raspberry Pi
                        {
                            delay_ms(200);
                            send_init();
                            return;
                        }
                            
                        SELECT_LDR;  
                        ADC_START; 
                        while(ADC_CHECK);  
                            adc1 = ADRESH; 
                        delay_ms(100);
                        
                        SELECT_TEMP; 
                        ADC_START; 
                        while(ADC_CHECK);  
                            adc2 = ADRESH;  
                        delay_ms(100); 
                        
                        count++;
                        ch1[0] = count;
                        ch1[1] = adc1;
                        ch1[2] = adc2;
                        ch1[3] = PIR;
                        send_data(ch1,update_packet);  
                    break;
                }

           }
            LED1 = 1;
          return 1;
        
        }
    return 0;
}