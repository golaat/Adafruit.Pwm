using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Devices.I2c;
using Windows.Devices.Pwm;
using Windows.Devices.Pwm.Provider;
using Windows.Foundation;

namespace Adafruit.Pwm
{
    /// <summary>
    /// C# implementation of Adafruit PWM Servo Pi Hat driver based on C++ sample code
    /// </summary>
    /// <see cref="https://github.com/adafruit/Adafruit-PWM-Servo-Driver-Library"/>
    /// <seealso cref="http://www.adafruit.com/products/2327"/>
    /// <seealso cref="https://learn.adafruit.com/adafruit-16-channel-pwm-servo-hat-for-raspberry-pi/overview"/>
    public class PwmController : IDisposable, IPwmControllerProvider
    {
        private I2cDevice _servoPiHat;
        private readonly byte _baseAddress;
        private bool _initialized; 

        /// <summary>
        /// base address of the Servo PWM Pi Hat
        /// </summary>
        private const byte PIHAT_I2C_ADDR = 0x40;

        /// <summary>
        /// Create an instance of the PWM Servo driver targeing the default Hat device base address (0x40)
        /// </summary>
        public PwmController():this(PIHAT_I2C_ADDR) { }

        /// <summary>
        /// Create an instance of the PWM Servo driver targeing a specific Pi Hat device base address
        /// </summary>
        /// <param name="baseAddress">base address; default: 0x40</param>
        public PwmController(byte baseAddress)
        {
            MaxFrequency = 1000;
            MinFrequency = 40;
            PinCount = 16;
            _initialized = false;
            _baseAddress = baseAddress;

            Initialize();
        }

        private void Initialize()
        {
            if (_initialized)
                return;

            /* Create an I2cDevice with our selected bus controller and I2C settings */
            var settings = new I2cConnectionSettings(_baseAddress) {BusSpeed = I2cBusSpeed.FastMode};
            DeviceInformationCollection devices = null;

            Task.Run(async () =>
            {

                // Get a selector string that will return all I2C controllers on the system 
                string aqs = I2cDevice.GetDeviceSelector();

                // Find the I2C bus controller device with our selector string
                devices = await DeviceInformation.FindAllAsync(aqs);

                //search for the controller
                if (!devices.Any())
                    throw new IOException("No I2C controllers were found on the system");

                //see if we can find the hat
                _servoPiHat = await I2cDevice.FromIdAsync(devices[0].Id, settings);

            }).Wait();

            if (_servoPiHat == null)
            {
                string message;
                if (devices != null && devices.Count > 0)
                {
                    message = string.Format(
                        "Slave address {0} on I2C Controller {1} is currently in use by another application. Please ensure that no other applications are using I2C.",
                        settings.SlaveAddress,
                        devices[0].Id);
                }
                else
                {
                    message = "Could not initialize the device!";
                }

                throw new IOException(message);
            }

            _initialized = true;
            Reset();

        }

        public void Reset()
        {
            _servoPiHat.Write(new byte[] { (byte)Registers.MODE1, 0x0 }); // reset the device
            SetAllPwm(0,0);
        }

        /// <summary>
        /// toggles the pulse for a given pin; note: to completely shut off an LED, send in a value of 4096
        /// </summary>
        /// <param name="pin">the pin between 0 and 15</param>
        /// <param name="dutyCycle">value between 0-4095; if maximum value exceeded, 4095 will be used</param>
        /// <param name="invertPolarity"></param>
        public void SetPulseParameters(int pin, double dutyCycle, bool invertPolarity = false)
        {
            // Clamp value between 0 and 4095 inclusive. 
            dutyCycle = Math.Min(dutyCycle, 4095);

            ushort value = (ushort) dutyCycle;
            byte channel = (byte)pin;

            if ((int)channel > PinCount - 1)
                throw new ArgumentOutOfRangeException(nameof(channel), "Channel must be between 0 and 15");

            if (invertPolarity)
            {
                // Special value for signal fully on/off.
                switch (value)
                {
                    case 0:
                        SetPwm(channel, 4095, 0);
                        break;
                    case 4095:
                        SetPwm(channel, 0, 4095);
                        break;
                    case 4096:
                        SetPwm(channel, 0, 4096);
                        break;
                    default:
                        SetPwm(channel, 0, (ushort)(4095 - value));
                        break;
                }
            }
            else
            {
                // Special value for signal fully on/off. 
                switch (value)
                {
                    case 4095:
                        SetPwm(channel, 4095, 0);
                        break;
                    case 4096:
                        SetPwm(channel, 4096, 0);
                        break;
                    case 0:
                        SetPwm(channel, 0, 4095);
                        break;
                    default:
                        SetPwm(channel, 0, value);
                        break;
                }
            }
        }

        /// <summary>
        /// toggles the pulse for all pins; note: to completely shut off an LED, send in a value of 4096
        /// </summary>
        /// <param name="value">value between 0-4095; if maximum value exceeded, 4095 will be used</param>
        /// <param name="invertPolarity"></param>
        public void SetPulseParameters(double value, bool invertPolarity = false)
        {
            // Clamp value between 0 and 4095 inclusive. 
            value = Math.Min(value, 4095);

            if (invertPolarity)
            {
                // Special value for signal fully on.
                switch ((ushort)value)
                {
                    case 0:
                        SetAllPwm(4095, 0); 
                        break;
                    case 4095:
                        SetAllPwm(0, 4095);
                        break;
                    case 4096:
                        SetAllPwm(0, 4096);
                        break;
                    default:
                        SetAllPwm(0, (ushort)(4095 - value));
                        break;
                }
            }
            else
            {
                // Special value for signal fully on. 
                switch ((ushort)value)
                {
                    case 4095:
                        SetAllPwm(4095, 0);
                        break;
                    case 4096:
                        SetAllPwm(4096, 0);
                        break;
                    case 0:
                        SetAllPwm(0, 4095);
                        break;
                    default:
                        SetAllPwm(0, (ushort)value);
                        break;
                }
            }
        }

        /// <summary>
        /// Toggles a pin
        /// </summary>
        /// <param name="channel">The channel that should be updated with the new values (0..15)</param>
        /// <param name="on">The tick (between 0..4095) when the signal should transition from low to high</param>
        /// <param name="off">the tick (between 0..4095) when the signal should transition from high to low</param>
        private void SetPwm(byte channel, ushort on, ushort off)
        {
            _servoPiHat.Write(new byte[] { (byte)(Registers.LED0_ON_L + 4 * channel), (byte)(on & 0xFF) });
            _servoPiHat.Write(new byte[] { (byte)(Registers.LED0_ON_H + 4 * channel), (byte)(on >> 8) });
            _servoPiHat.Write(new byte[] { (byte)(Registers.LED0_OFF_L + 4 * channel), (byte)(off & 0xFF) });
            _servoPiHat.Write(new byte[] { (byte)(Registers.LED0_OFF_H + 4 * channel), (byte)(off >> 8) });
        }

        private void SetAllPwm(ushort on, ushort off)
        {
            _servoPiHat.Write(new byte[] { (byte)Registers.ALL_LED_ON_L, (byte)(on & 0xFF) });
            _servoPiHat.Write(new byte[] { (byte)Registers.ALL_LED_ON_H, (byte)(on >> 8) });
            _servoPiHat.Write(new byte[] { (byte)Registers.ALL_LED_OFF_L, (byte)(off & 0xFF) });
            _servoPiHat.Write(new byte[] { (byte)Registers.ALL_LED_OFF_H, (byte)(off >> 8) });
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_servoPiHat != null)
                {
                    try
                    {
                        Reset();
                        _servoPiHat.Dispose();
                    }
                    catch (Exception)
                    {
                        //eat the exception
                    }
                }
            }

        }

        /// <summary>
        /// Specifies the frequency; defaults to 60hz if not set. This determines how many full pulses per second are generated.
        /// </summary>
        /// <param name="frequency">A number representing the frequency in Hz, between 40 and 1000</param>
        public double SetDesiredFrequency(double frequency)
        {
            if (frequency > MaxFrequency || frequency < MinFrequency)
                throw new ArgumentOutOfRangeException(nameof(frequency), "Frequency must be between 40 and 1000hz");

            frequency *= 0.9f;  // Correct for overshoot in the frequency setting (see issue #11).
            double prescaleval = 25000000f;
            prescaleval /= 4096;
            prescaleval /= frequency;
            prescaleval -= 1;

            byte prescale = (byte)Math.Floor(prescaleval + 0.5f);

            var readBuffer = new byte[1];
            _servoPiHat.WriteRead(new byte[] { (byte)Registers.MODE1 }, readBuffer);

            byte oldmode = readBuffer[0];
            byte newmode = (byte)((oldmode & 0x7F) | 0x10); //sleep
            _servoPiHat.Write(new byte[] { (byte)Registers.MODE1, newmode });
            _servoPiHat.Write(new byte[] { (byte)Registers.PRESCALE, prescale });
            _servoPiHat.Write(new byte[] { (byte)Registers.MODE1, oldmode });
            Task.Delay(TimeSpan.FromMilliseconds(5)).Wait();
            _servoPiHat.Write(new byte[] { (byte)Registers.MODE1, (byte)(oldmode | 0xa1) });

            ActualFrequency = frequency;
            return ActualFrequency;

        }

        public void AcquirePin(int pin)
        {
            throw new NotImplementedException();
        }

        public void ReleasePin(int pin)
        {
            throw new NotImplementedException();
        }

        public void EnablePin(int pin)
        {
            throw new NotImplementedException();
        }

        public void DisablePin(int pin)
        {
            throw new NotImplementedException();
        }

        public double ActualFrequency { get; private set; }
        public double MaxFrequency { get; }
        public double MinFrequency { get; }
        public int PinCount { get; }
    }
}
