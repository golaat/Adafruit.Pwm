using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adafruit.Pwm
{
    public enum Registers
    {
        MODE1 = 0x00,
        MODE2 = 0x01,
        SUBADR1 = 0x02,
        SUBADR2 = 0x03,
        SUBADR3 = 0x04,
        PRESCALE = 0xFE,
        LED0_ON_L = 0x06,
        LED0_ON_H = 0x07,
        LED0_OFF_L = 0x08,
        LED0_OFF_H = 0x09,
        ALL_LED_ON_L = 0xFA,
        ALL_LED_ON_H = 0xFB,
        ALL_LED_OFF_L = 0xFC,
        ALL_LED_OFF_H = 0xFD
    }

    public enum Bits
    {
        RESTART = 0x80,
        SLEEP = 0x10,
        ALLCALL = 0x01,
        INVRT = 0x10,
        OUTDRV = 0x04
    }
}
