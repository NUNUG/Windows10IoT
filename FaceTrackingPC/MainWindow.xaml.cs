using Accord.Vision.Detection;
using Accord.Vision.Detection.Cascades;
using AForge.Video;
using AForge.Video.DirectShow;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Net;
using System.Net.Sockets;
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

		private Rectangle? FocusedOnFace;
		private AForge.Point CameraVector;

		private TcpClient Client;
		private NetworkStream ClientStream;

		private static int WIDTH = 1280;
		private static int HEIGHT = 720;
		private static double DegreesPerPixel;

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
			// Setup Socket Client
			Client = new TcpClient();
			Client.Connect(IPAddress.Parse("10.10.10.12"), 1911);
			ClientStream = Client.GetStream();

			// enumerate video devices
			videoDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);
			// create video source
			videoSource = new VideoCaptureDevice(videoDevices[1].MonikerString);
			videoSource.SnapshotResolution = videoSource.VideoCapabilities[7];
			// set NewFrame event handler
			videoSource.NewFrame += new NewFrameEventHandler(video_NewFrame);
			// start the video source
			videoSource.Start();

			// Setup Face Detection
			Cascade = new FaceHaarCascade();
			Detector = new HaarObjectDetector(Cascade, 30);
			Detector.SearchMode = ObjectDetectorSearchMode.Average;
			//Detector.ScalingFactor = 1f;
			Detector.ScalingMode = ObjectDetectorScalingMode.GreaterToSmaller;
			Detector.UseParallelProcessing = true;
			Detector.Suppression = 3;

		}


		private void video_NewFrame(object sender, NewFrameEventArgs eventArgs)
		{
			// get new frame
			Bitmap bitmap = eventArgs.Frame;

			// process the frame
			var faces = Detector.ProcessFrame(bitmap);

			Debug.WriteLine("Total Objects Detected: " + faces.Length);

			if (faces.Length > 0)
				FocusedOnFace = faces[0];

			FollowFace(FocusedOnFace);
		}

		private void FollowFace(Rectangle? face)
		{


			if (face == null)
				return;

			Rectangle location = face.Value;

			int offset = location.Left - location.Width;
			int center = WIDTH / 2;

			int changeDegrees = (int)(Math.Abs(offset - center) * DegreesPerPixel);
			if (offset < 0 && CameraVector.X - changeDegrees > 0)
				CameraVector.X -= changeDegrees;

			if (offset > 0 && CameraVector.X + changeDegrees < 180)
				CameraVector.X += changeDegrees;

			offset = location.Top - location.Height;
			center = HEIGHT / 2;

			changeDegrees = (int)(Math.Abs(offset - center) * DegreesPerPixel);

			if (offset < 0 && CameraVector.Y - changeDegrees > 0)
				CameraVector.Y -= changeDegrees;
			if (offset > 0 && CameraVector.Y - changeDegrees < 180)
				CameraVector.Y += changeDegrees;

			string value = CameraVector.X.ToString().PadLeft(3, '0') + "," + CameraVector.Y.ToString().PadLeft(3, '0');
			byte[] values = System.Text.Encoding.UTF8.GetBytes(value);

			ClientStream.Write(values, 0, values.Length);
		}
	}
}
