using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Http;
using Windows.ApplicationModel.Background;
using System.Net.Sockets;
using System.Net;

// The Background Application template is documented at http://go.microsoft.com/fwlink/?LinkID=533884&clcid=0x409

namespace FaceTrackingPi
{
    public sealed class StartupTask : IBackgroundTask
    {
		private Socket Server;

        public void Run(IBackgroundTaskInstance taskInstance)
        {
			// Setup Netowrk Socket
			Server = new Socket(SocketType.Stream, ProtocolType.Tcp);
			Server.Bind(new IPEndPoint(IPAddress.Any, 1911));
			Server.Listen(100);

			// Setup Servos


			while(true)
			{
				Server.Listen(10);
				Server.
			}
        }
    }
}
