#include "nrf24.h"
#include "Hardware.h" 


/////////////////Control defenitions////
#define nRF_SIZE_P0 1
#define nRF_SIZE_P1 1
#define nRF_SIZE_P2 0
#define nRF_SIZE_P3 0
#define nRF_SIZE_P4 0
#define nRF_SIZE_P5 0

#define nRF_TX
//#define nRF_TX

//#define nRF_CHANNEL         0
//#define nRF_TX_POWER        0
//#define nRF_AUTOACK         1           //
//#define nRF_RETRIES         5    //
//#define nRF_PIPE_ENABLE     0x03    //0x03 will enable Pipe0 and Pipe1


////////define required pipes


////////////////////initialize pins////////////////////
void init_spi()
{
    MOSItris = 0;
    MISOtris = 1;
    SCKtris = 0;
    CE_TRIS = 0;
    CSN_TRIS = 0;
}
  
/////////////////////////8bit transmission////////////
unsigned char xmitbyte(unsigned char dat_a)
{
    int tempc;
    unsigned char rxdat=0;
   ///Software SPI
    for(tempc=0; tempc<8; tempc++)
    {  
            NOP(); NOP();
        if(dat_a & 0x80)
            MOSI = 1;
        else
            MOSI = 0;

        dat_a = dat_a<<1;

        if(MISO == 1)
            SETBIT(rxdat,7-tempc);
        else
            CLEARBIT(rxdat,7-tempc);

        SCK = 1;
            NOP(); NOP();
        SCK = 0;
            NOP(); NOP();
    }

    return rxdat;
    
}

//////////////////////init tx/////////////////
void init_nrf()
{  
    CE = 0;
    CSN = 0;
    delay_ms(50); 
            // No need to change channel or address of nRF
            // Unless another nRF using the same channel

    ///Init nRF
            CSN = 0;
            //write pipe 0 payload length
            xmitbyte(W_REGISTER + RX_PW_P0 );
            xmitbyte(nRF_PACKET_SIZE);
            CSN = 1; 
            
            CSN = 0;
            //write pipe 1 payload length 
            xmitbyte(W_REGISTER + RX_PW_P1 );
            xmitbyte(nRF_PACKET_SIZE);
            CSN = 1; 
            
            CSN = 0;
            //Disable auto ACK
            xmitbyte(W_REGISTER + EN_AA );
            xmitbyte(0x00);                         //0x00
            CSN = 1;     
            
///////////////////////////////////////
                                   //retransmit
    //        xmitbyte(W_REGISTER + SETUP_RETR);
      //      xmitbyte(0x23);                         //0x00
        //    CSNtoggle();

       //     //Dynamic payload
         //   xmitbyte(W_REGISTER + FEATURE);
           // xmitbyte(EN_DYN_ACK | EN_ACK_PAY | EN_DPL);                         //0x00
            //CSNtoggle();      
    nrf_FLUSH_RX();
    nrf_FLUSH_TX();
//////////////////////////////////////////
 rx_mode(); 

}

//Flush TX Buffer
void nrf_FLUSH_TX()
{
    CSN = 0;
     xmitbyte(FLUSH_TX);
     CSN = 1;
}

//Flush RX Buffer
void nrf_FLUSH_RX()
{
     CSN = 0;
     xmitbyte(FLUSH_RX);
     CSN = 1;
}
///////////////////////rx mode//////
void rx_mode()
{ 
    CE = 0;
    CSN = 0;
               //write config reg
            xmitbyte(W_REGISTER + CONFIG );
            xmitbyte(CONFIG_PWR_UP | CONFIG_PRIM_RX);
      CSN = 1;    
            CE=1; 
            __delay_ms(1);
}

/////////////tx mode//////////
void tx_mode()
{ 
            CE=0;
            CSN = 0;
                //write config reg
            xmitbyte(W_REGISTER + CONFIG );
            xmitbyte(CONFIG_PWR_UP);
              CSN = 1;  
            __delay_ms(4);
}
 
/////////////////////////xmit data////////////////////
void TXpayload(char* pload)
{
    //Clears TX buffer
    nrf_FLUSH_TX();
    int tcount; 
    CSN = 0;
    //Txmit payload command
    xmitbyte(W_TX_PAYLOAD);
    for(tcount = 0 ; tcount<nRF_PACKET_SIZE; tcount++)
    {
            xmitbyte(pload[tcount]); 
    }
 
    CSN = 1;
    CE =1;
    __delay_us(50);         //Need minimum 10us delay for transmitting the packet
    CE=0;
    __delay_ms(10);
}

///Don't use this function it is in testing stage
/////////////////////////xmit data no ack////////////////////
void TXpayload_NOACK(char pload)
{
    CSN = 0;
    xmitbyte(W_TX_PAYLOAD_NOACK);
    xmitbyte(pload);
      CSN = 1; 
    CE =1;
    __delay_us(100);
    CE=0;
    __delay_ms(10);
}

/////////////////////////RX data////////////////////
void RXpayload(char* pload)
{
    // char tmp array[nRF_PACKET_SIZE];
    CSN = 0;   
    int tcount;
    xmitbyte(R_RX_PAYLOAD);

    for(tcount = 0 ; tcount<nRF_PACKET_SIZE; tcount++)
    {
            pload[tcount]= xmitbyte(NOOP);

    }
 CSN = 1;
      
}