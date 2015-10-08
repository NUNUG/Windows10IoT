using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Gpio
{
	public class ServoController
	{
		protected IPwm Pin;
		protected int DegreesOfMotion = 180;
		protected int TargetFrequency = 50;

		public int Angle { get; protected set; }

		public ServoController(IPwm pwmPin)
		{
			Pin = pwmPin;
		}

		public void Start(int angle)
		{
			SetAngle(angle);
			Pin.ChangeFrequency(TargetFrequency);
			Pin.Start();
		}

		public void Stop(int angle)
		{
			Pin.Stop();
		}

		public void SetAngle(int angle)
		{
			// Typical Servo Limits:
			//    Left  - 0.5ms
			//    TDC   - 1.5ms
			//    Right - 2.5ms
			// Period   = 15-25ms?

			// 50 Hz = 20ms
			// .5 + (angle / DegreesOfMotion) * 2 = ?ms

			// 0.5 = 2.5% @ 50Hz - 
			// 1.5 = 7.5% @ 50Hz - 5% change
			// 2.5 = 12.5 @ 50Hz - 5% change

			// percentage from angle
			// percentage = 2.5 + (angle / DegreesOfMotion) * ( 500 / Frequency)

			float dutyCycle = 2.5f + ((float)angle / DegreesOfMotion) * (500f / TargetFrequency);
			Debug.WriteLine("Changing Duty Cycle to: " + dutyCycle);
			Pin.ChangeDutyCycle(dutyCycle);
		}


	}
}
