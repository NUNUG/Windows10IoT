using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Http;
using Windows.ApplicationModel.Background;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using System.Diagnostics;
using Gpio;
using Windows.Devices.Gpio;

// The Background Application template is documented at http://go.microsoft.com/fwlink/?LinkID=533884&clcid=0x409

namespace FaceTrackingPi
{
	public sealed class StartupTask : IBackgroundTask
	{
		private Socket Server;
		private AutoResetEvent DataReadEvent = new AutoResetEvent(false);
		private ServoController Azimuth;
		private ServoController Inclenation;

		public void Run(IBackgroundTaskInstance taskInstance)
		{
			// Setup Netowrk Socket
			Server = new Socket(SocketType.Stream, ProtocolType.Tcp);
			Server.Bind(new IPEndPoint(IPAddress.Any, 1911));
			Server.Listen(100);

			// Setup Servos
			var controller = GpioController.GetDefault();
			
			// GPIO 18
			var pin = controller.OpenPin(18, GpioSharingMode.Exclusive);
			pin.SetDriveMode(GpioPinDriveMode.Output);
			Azimuth = new ServoController(new SofwarePwm(n => pin.Write(n ? GpioPinValue.High : GpioPinValue.Low), 50, 0));
			Azimuth.Start(90);

			// GPIO 23
			pin = controller.OpenPin(23, GpioSharingMode.Exclusive);
			pin.SetDriveMode(GpioPinDriveMode.Output);
			Inclenation = new ServoController(new SofwarePwm(n => pin.Write(n ? GpioPinValue.High : GpioPinValue.Low), 50, 0));
			Inclenation.Start(90);

			Server.Listen(10);
			var connectEvent = new AutoResetEvent(false);

			while(true)
			{
				SocketAsyncEventArgs e = new SocketAsyncEventArgs();
				e.Completed += AcceptCallback;
				if (!Server.AcceptAsync(e))
				{
					AcceptCallback(Server, e);
					connectEvent.Set();
				}

				connectEvent.WaitOne(1000);
			}
		}

	
		private void AcceptCallback(object sender, SocketAsyncEventArgs e)
		{
			Socket listenSocket = (Socket)sender;
			do
			{
				try
				{
					Socket newSocket = e.AcceptSocket;
					Debug.Assert(newSocket != null);
					// do your magic here with the new socket
					while (true)
					{
						var args = new SocketAsyncEventArgs();
						args.SetBuffer(new byte[1024], 0, 1024);
						args.Completed += Args_Completed;
						newSocket.ReceiveAsync(args);
						DataReadEvent.WaitOne();
					}
				}
				catch
				{
					// handle any exceptions here;
				}
				finally
				{
					e.AcceptSocket = null; // to enable reuse
				}
			} while (!listenSocket.AcceptAsync(e));
		}

		private void Args_Completed(object sender, SocketAsyncEventArgs e)
		{
			string value = Encoding.UTF8.GetString(e.Buffer, 0, e.BytesTransferred);
			// 000,000 L/R angle, U/D angle

			if (value.Length == 7)
			{
				int azimuth = int.Parse(value.Substring(0, 3));
				int inclenation = int.Parse(value.Substring(4));

				Azimuth.SetAngle(azimuth);
				Inclenation.SetAngle(inclenation);
			}
		}
	}
}
