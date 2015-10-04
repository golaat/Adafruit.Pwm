using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Devices.I2c;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Adafruit;


namespace Adafruit.Pwm.Example
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {

        public MainPage()
        {
            this.InitializeComponent();
            RotateExample();
            Application.Current.Exit();
        }

        private void RotateExample()
        {
            try
            {
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
            }

            /* If the write fails display the error and stop running */
            catch (Exception ex)
            {
                Text_Status.Text = "Failed to communicate with device: " + ex.Message;
                return;
            }

        }
    }
}
