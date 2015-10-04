# Adafruit.Pwm
C# implementation of [Adafruit PWM Servo](http://www.adafruit.com/products/2327) driver

The library can be used from the Windows 10 IoT Core release to control the Adafruit PWM servo.  It is based on the C++ the sample provided by Adafruit which is available [here](https://github.com/adafruit/Adafruit-PWM-Servo-Driver-Library).

**How to use:**

1. Add a reference to the `Adafruit.Pwm` library to your Universal Windows application.
2. Create an instance of the `Adafruit.Pwm.PwmController()`.  You can optionally specify the base address if your device is not located at the default of `0x40`
3. Optionally invoke `SetDesiredFrequency`; if not set, a default of 60Hz is used.
4. Invoke `SetPulseParameters`

**Example:**

    //The servoMin/servoMax values are dependant on the hardware you are using.
    //The values below are for my SR-4303R continuous rotating servos.
    //If you are working with a non-continous rotatng server, it will have an explicit
    //minimum and maximum range; crossing that range can cause the servo to attempt to
    //spin beyond its capability, possibly damaging the gears.

    const int servoMin = 300;  // Min pulse length out of 4095
    const int servoMax = 480;  // Max pulse length out of 4095
    
    using (var hat = new Adafruit.Pwm.PwmController())
    {
    	DateTime timeout = DateTime.Now.AddSeconds(10);
    	hat.SetDesiredFrequency(60);
    	while (timeout >= DateTime.Now)
    	{
    		hat.SetPulseParameters(0, servoMin, false);
    		Task.Delay(TimeSpan.FromSeconds(1)).Wait();
    		hat.SetPulseParameters(0, servoMax, false);
    		Task.Delay(TimeSpan.FromSeconds(1)).Wait();
    	}
    }

