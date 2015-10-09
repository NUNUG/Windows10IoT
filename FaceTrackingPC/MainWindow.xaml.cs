using Accord.Vision.Detection;
using Accord.Vision.Detection.Cascades;
using AForge.Video;
using AForge.Video.DirectShow;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;


namespace FaceTrackingPC
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{

		private FilterInfoCollection videoDevices;
		private VideoCaptureDevice videoSource;

		private HaarObjectDetector Detector;
		private FaceHaarCascade Cascade;

		private Rectangle FaceRect;
		private AForge.Point CameraVector;

		private TcpClient Client;
		private NetworkStream ClientStream;

		private static int WIDTH = 1280;
		private static int HEIGHT = 720;
		private static double DegreesPerPixel = 0.1;
		private DateTime? LastReposition;

		public MainWindow()
		{
			InitializeComponent();
			Loaded += MainWindow_Loaded;
			Closing += MainWindow_Closing;
		}

		private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			videoSource.SignalToStop();
		}

		private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
		{
			// Wait for the client to get setup
			await Task.Delay(5000);

			// Setup Socket Client
			Client = new TcpClient();
			Client.Connect(IPAddress.Parse("10.10.10.100"), 1911);
			ClientStream = Client.GetStream();

			// enumerate video devices
			videoDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);
			// create video source
			videoSource = new VideoCaptureDevice(videoDevices[1].MonikerString);
			videoSource.SnapshotResolution = videoSource.VideoCapabilities[7];
			// set NewFrame event handler
			videoSource.NewFrame += new NewFrameEventHandler(video_NewFrame);

			// Setup Face Detection
			Cascade = new FaceHaarCascade();
			Detector = new HaarObjectDetector(Cascade, 30);
			Detector.SearchMode = ObjectDetectorSearchMode.Average;
			//Detector.ScalingFactor = 1f;
			Detector.ScalingMode = ObjectDetectorScalingMode.GreaterToSmaller;
			Detector.UseParallelProcessing = true;
			Detector.Suppression = 3;

			// Setup Tracking Data
			CameraVector.X = 90;
			CameraVector.Y = 90;
			//ClientStream.Write(Encoding.UTF8.GetBytes("090,090"), 0, 7);

			//await Task.Delay(3000);

			// start the video source
			videoSource.Start();
		}


		private void video_NewFrame(object sender, NewFrameEventArgs eventArgs)
		{
			// get new frame
			Bitmap bitmap = eventArgs.Frame;

			// process the frame
			var faces = Detector.ProcessFrame(bitmap);

			//if (faces.Length > 0)
			//	Debug.WriteLine("Total Objects Detected: " + faces.Length);

			if (faces.Length > 0)
				FaceRect = faces[0];

			FollowFace(FaceRect);
		}

		private void FollowFace(Rectangle? face)
		{
			if (face == null)
				return;

			if (LastReposition != null && LastReposition.Value.AddSeconds(3) > DateTime.Now)
				return;
			else
				LastReposition = DateTime.Now;

			FaceRect = face.Value;

			int offset = FaceRect.Left + (FaceRect.Width / 2);
			int center = WIDTH / 2;

			int changeDegrees = (int)((offset - center) * DegreesPerPixel);
			if ((CameraVector.X + changeDegrees > 0) && (CameraVector.X + changeDegrees < 180))
				CameraVector.X += changeDegrees;

			offset = FaceRect.Top - (FaceRect.Height / 2);
			center = HEIGHT / 2;

			changeDegrees = (int)((offset - center) * DegreesPerPixel);

			if ((CameraVector.Y + changeDegrees > 0) && (CameraVector.Y + changeDegrees < 180))
				CameraVector.Y += changeDegrees;

			string value = CameraVector.X.ToString().PadLeft(3, '0') + "," + CameraVector.Y.ToString().PadLeft(3, '0');
			byte[] values = Encoding.UTF8.GetBytes(value);

			Debug.WriteLine("Sending Reposition to: " + value);
			ClientStream.Write(values, 0, values.Length);
		}
	}
}
