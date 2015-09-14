/* 
 * File:   hardware.c
 * Author: Anand
 *
 * Created on August 23, 2015, 12:57 AM
 */
 
#include "hardware.h"

   void delay_ms(int del)
   {
       while(del)
       {
           del--;
          __delay_ms(1);
       }
   }
////////////////////////////////////////////////

 