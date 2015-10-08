using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media.Capture;
using Windows.Media.MediaProperties;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace FaceTrackingPC
{
	/// <summary>
	/// An empty page that can be used on its own or navigated to within a Frame.
	/// </summary>
	public sealed partial class MainPage : Page
	{
		private MediaCapture Media;

		public MainPage()
		{
			this.InitializeComponent();
			this.Loaded += MainPage_Loaded;

		}

		private async void MainPage_Loaded(object sender, RoutedEventArgs e)
		{
			Media = new MediaCapture();
			await Media.InitializeAsync();

			while (true)
			{
				// Take a Photo from the webcam
				var stream = new InMemoryRandomAccessStream();
				await Media.CapturePhotoToStreamAsync(ImageEncodingProperties.CreateJpeg(), stream);
			}
		}

	}
}
