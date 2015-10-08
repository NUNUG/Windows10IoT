using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;

namespace Gpio
{
	public class SofwarePwm : IPwm, IDisposable
	{
		protected Thread Worker;
		protected volatile bool IsRunning;


		public Action<bool> Pin { get; protected set; }
		public int Frequency { get; protected set; }
		public float DutyCycle { get; protected set; }
		public bool Running { get { return IsRunning; } }

		public SofwarePwm(Action<bool> pin, int frequency, float dutyCycle)
		{
			Pin = pin;
			ChangeFrequency(frequency);
			ChangeDutyCycle(dutyCycle);
		}

		public void Start()
		{
			if (IsRunning)
				throw new InvalidOperationException("PWM already started.");

			Worker = new Thread(PwmProc);
			Worker.Name = "Software PWM Thread";
			Worker.Priority = ThreadPriority.Highest;
			IsRunning = true;
			Worker.Start();
		}

		public void Stop()
		{
			if (!IsRunning)
				throw new InvalidOperationException("PWM not started.");

			IsRunning = false;
			Worker.Join();
		}

		protected void Restart()
		{
			if (IsRunning)
			{
				Stop();
				Start();
			}
		}

		public void ChangeFrequency(int frequency)
		{
			if (frequency < 1 || frequency > 2000)
				throw new ArgumentOutOfRangeException("Frequency");

			Frequency = frequency;
			Restart();
		}

		public void ChangeDutyCycle(float dutyCycle)
		{
			if (dutyCycle < 0 || dutyCycle > 100)
				throw new ArgumentOutOfRangeException("DutyCycle");

			DutyCycle = dutyCycle;
			Restart();
		}

		public void PwmProc()
		{
			// totalCycle = (1/n)*1000 = Total MS for the full cycle
			// onCycle = totalCycle * (DutyCycle / 100)
			// offCycle = totalCycle - onCycle;

			SpinWait sp = new SpinWait();
			Stopwatch sw = new Stopwatch();

			int totalCycleTicks = (int)((1f / Frequency) * Stopwatch.Frequency);
			int onTicks = (int)Math.Round(totalCycleTicks * (DutyCycle / 100f));
			int offTicks = totalCycleTicks - onTicks;

			Debug.WriteLine("On Ticks: " + onTicks);

			while (IsRunning)
			{
				/**** ON ****/
				// Turn the pin on for the duty cycle
				Pin(true);

				// Let the pin stay high until we stop or 
				sw.Reset();
				sp.Reset();
				sw.Start();
				while (sw.ElapsedTicks < onTicks) ;
					//sp.SpinOnce();

				Debug.WriteLine("Total Spin On: " + sw.ElapsedTicks + " and needed " + onTicks);

				/**** OFF ****/
				// Turn the pin off for the rest period
				Pin(false);

				sw.Reset();
				sp.Reset();
				sw.Start();
				while (sw.ElapsedTicks < offTicks) ;
					//sp.SpinOnce();

				Debug.WriteLine("Total Spin Off: " + sw.ElapsedTicks + " and needed " + offTicks);
			}

			// Turn off the PIN when we're done
			Pin(false);
		}

		public void Dispose()
		{
			if (IsRunning)
				Stop();
		}
	}
}
