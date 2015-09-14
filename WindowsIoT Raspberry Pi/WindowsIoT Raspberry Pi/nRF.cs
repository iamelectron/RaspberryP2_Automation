using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Gpio;
using Windows.Devices.Spi; 
using Windows.Devices.Enumeration;

/* nRF24 Minimal Library Developed by Anand (Windows IoT challenge)
   I have omitted some complex operations from this library for simplicity.
   This library uses hardware SPI of Raspberry Pi 
   Don't operate the SPI above 8Mhz, as it is the maximum possible SPI speed of nRF module
   This lib tested with some chinese made nRF24 and nRF24 clone modules. If you find this lib is not
   suitable for your nRF24 module, make necessary changes after reading respective datasheet of the module.
   */
    

namespace Rasp_Final
{
    class nRF
    {
        //Private const
        private const byte CONFIG = 0x00;
        private const byte EN_AA = 0x01;
        private const byte EN_RXADDR = 0x02;

        //CONFIG register bitwise definitions
        private const byte CONFIG_RESERVED = 0x80;
        private const byte CONFIG_MASK_RX_DR = 0x40;
        private const byte CONFIG_MASK_TX_DS = 0x20;
        private const byte CONFIG_MASK_MAX_RT = 0x10;
        private const byte CONFIG_EN_CRC = 0x08;
        private const byte CONFIG_CRCO = 0x04;
        private const byte CONFIG_PWR_UP = 0x02;
        private const byte CONFIG_PRIM_RX = 0x01;


        private const byte STATUS = 0x07;
        private const byte STATUS_DEFAULT_VAL = 0x0E;
        private const byte STATUS_RX_DR = 0x40;

        //Command Name Mnemonics (Instructions)
        private const byte R_REGISTER = 0x00;
        private const byte W_REGISTER = 0x20;
        private const byte REGISTER_MASK = 0x1F;
        private const byte R_RX_PAYLOAD = 0x61;
        private const byte W_TX_PAYLOAD = 0xA0;
        private const byte FLUSH_TX = 0xE1;
        private const byte FLUSH_RX = 0xE2;
        private const byte REUSE_TX_PL = 0xE3;


        //PLUS DEVICES
        private const byte W_TX_PAYLOAD_NOACK = 0xB0;
        private const byte W_ACK_PAYLOAD = 0xA9;       //1bit with ack
        private const byte R_RX_PL_WD = 0x60;
        ///
        private const byte NOOP = 0xFF;


        //private const byte CD          0x09     //Mnemonic from nRF24L01, new is RPD
        private const byte RX_ADDR_P0 = 0x0A;
        private const byte RX_ADDR_P1 = 0x0B;
        private const byte RX_ADDR_P2 = 0x0C;
        private const byte RX_ADDR_P3 = 0x0D;
        private const byte RX_ADDR_P4 = 0x0E;
        private const byte RX_ADDR_P5 = 0x0F;
        private const byte TX_ADDR = 0x10;
        private const byte RX_PW_P0 = 0x11;
        private const byte RX_PW_P1 = 0x12;
        private const byte RX_PW_P2 = 0x13;
        private const byte RX_PW_P3 = 0x14;
        private const byte RX_PW_P4 = 0x15;
        private const byte RX_PW_P5 = 0x16;
        private const byte FIFO_STATUS = 0x17;

        private const byte nRF_PACKET_SIZE = 10;  


        GpioPin nRF_CE;          //CE Pin
        SpiDevice SPI_DEVICE;   //SPI Device Object

        /// <summary>
        /// nRF Hardware Init Function
        /// </summary>
        /// <param name="nrf_ce">nRF CE PIN</param>
        /// <param name="SPI_DEV">SPI Device object of Raspberry Pi</param>
        public nRF(GpioPin nrf_ce, SpiDevice SPI_DEV)
        {
            nRF_CE = nrf_ce;
            SPI_DEVICE = SPI_DEV;
            nRF_CE.SetDriveMode(GpioPinDriveMode.Output);
         
        }
    
 
        //nRF CE pin high low functions
        private void high_reset()
        {
            nRF_CE.Write(GpioPinValue.High);
        } 
        private void low_reset()
        {
            nRF_CE.Write(GpioPinValue.Low);
        }


        /// <summary>
        /// Write Byte stream to nRF device
        /// </summary>
        /// <param name="data">Byte data array</param>
        public void SPI_WRITE(byte[] data)
        {
            SPI_DEVICE.Write(data);
        }

        /// <summary>
        /// Read Byte array from nRF device (Don't call this function unless you know what you are really doing)
        /// You need to write RX_PAYLOAD before bulk reading.
        /// </summary>
        /// <returns></returns>
        public byte[] SPI_READ()
        { 
            byte[] bb = {0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF };
            SPI_DEVICE.Read(bb);
            return bb;
        }



/// <summary>
/// Initialize nRF module
/// Pipe 0 and Pipe 1 with no ack
/// This init can be replaced with user required initializations for complex wireless operations
/// </summary>
        public async void init_nRF()
        {
            low_reset();
            await Task.Delay(50); //Starting delay 
            byte[] tmp = { W_REGISTER + RX_PW_P0, nRF_PACKET_SIZE };
            SPI_WRITE(tmp); 
            byte[] tmp2 = { W_REGISTER + RX_PW_P1, nRF_PACKET_SIZE };
            SPI_WRITE(tmp2);

            byte[] tmp3 = { W_REGISTER + EN_AA, 0x00 };
            SPI_WRITE(tmp3);
            Flush_TX(); 
            Flush_RX();

            RX_MODE();

        }

      /// <summary>
      /// Clear TX Buffer
      /// </summary>
       public  void Flush_TX()
        {
            byte[] tmp4 = { FLUSH_TX };
            SPI_WRITE(tmp4);
        }

        /// <summary>
        /// Clear RX Buffer
        /// </summary>
        public void Flush_RX()
        {
            byte[] tmp4 = { FLUSH_RX };
            SPI_WRITE(tmp4);
        }


        /// <summary>
        /// Switch nRF mode to Receiver (Full duplex communication is not possible) 
        /// [Half duplex mode only]
        /// </summary>
        public async void RX_MODE()
        {
            low_reset();
            byte[] tmp = { W_REGISTER + CONFIG, CONFIG_PWR_UP | CONFIG_PRIM_RX };
            SPI_WRITE(tmp);
            high_reset(); 
            await Task.Delay(1);
        }

        /// <summary>
        /// Switch nRF mode to Transmitter (Full duplex communication is not possible) 
        /// [Half duplex mode only]
        /// </summary>
        public async void TX_MODE()
        {
            low_reset();
            byte[] tmp = { W_REGISTER + CONFIG, CONFIG_PWR_UP };
            SPI_WRITE(tmp); 
            await Task.Delay(4);
        }

/// <summary>
///     Transmit payload
///     before transmission, switch to TX_MODE()
/// </summary>
/// <param name="data">byte data array</param>
        public async void TX_PAYLOAD(byte[] data)
        {
            TX_MODE();
            //tmp array element can be altered so better use below format for debugging
            byte[] tmp = { W_TX_PAYLOAD, data[0],data[1], data[2], data[3], data[4], data[5], data[6],
                                      data[8],data[9]};

            SPI_WRITE(tmp); 
            high_reset();
            await Task.Delay(1);
            low_reset();
            await Task.Delay(10);
            RX_MODE();

        }

        /// <summary>
        ///     Transmit payload 
        /// </summary>
        /// <returns>byte array</returns>
        public byte[]  RX_PAYLOAD()
        { 
            low_reset();
            byte[] tmp = { R_RX_PAYLOAD, NOOP, NOOP, NOOP, NOOP, NOOP, NOOP, NOOP, NOOP, NOOP, NOOP };
            byte[] read_byte = new Byte[11]; 
            SPI_DEVICE.TransferFullDuplex(tmp, read_byte);  
            low_reset();
            byte[] tmp2 = { W_REGISTER + STATUS, STATUS_DEFAULT_VAL | STATUS_RX_DR };
            SPI_WRITE(tmp2); 
            return read_byte;
            
        }

        /// <summary>
        /// I used another cus
        /// </summary>
        /// <returns></returns>
       public bool is_data_available()
        {
            byte[] tmp = {NOOP};
            byte[] read_byte = new Byte[1];
            SPI_DEVICE.TransferFullDuplex(tmp, read_byte);

            if((read_byte[0] & (1 << 6)) >0)
            {
                return true;
            }
            else
            {
                return false;
            }
          
        }

    }

}
