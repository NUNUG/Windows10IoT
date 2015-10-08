using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gpio
{
	public interface IPwm
	{
		int Frequency { get; }
		float DutyCycle { get; }
		bool Running { get; }
		void Start();
		void Stop();
		void ChangeFrequency(int frequency);
		void ChangeDutyCycle(float dutyCycle);
	}
}
