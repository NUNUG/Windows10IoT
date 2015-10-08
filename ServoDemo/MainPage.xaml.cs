using Gpio;
using System.ComponentModel;
using Windows.Devices.Gpio;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace ServoDemo
{
	/// <summary>
	/// An empty page that can be used on its own or navigated to within a Frame.
	/// </summary>
	public sealed partial class MainPage : Page, INotifyPropertyChanged

	{
		protected SofwarePwm Pwm;
		protected ServoController Servo;

		public event PropertyChangedEventHandler PropertyChanged;

		private int angle;
		public int Angle
		{
			get { return angle; }
			set { angle = value; NotifyPropertyChanged("Angle"); }
		}


		public MainPage()
		{
			this.InitializeComponent();
			this.Loaded += MainPage_Loaded;
			this.Unloaded += MainPage_Unloaded;
			DataContext = this;

			PropertyChanged += MainPage_PropertyChanged;
		}
		private void NotifyPropertyChanged(string property)
		{
			var tmp = PropertyChanged;
			if (tmp != null)
				tmp(this, new PropertyChangedEventArgs(property));
		}

		private void MainPage_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			Servo.SetAngle(Angle);
		}

		private async void MainPage_Loaded(object sender, RoutedEventArgs e)
		{
			var controller = GpioController.GetDefault();
			var pin = controller.OpenPin(18, GpioSharingMode.Exclusive);
			pin.SetDriveMode(GpioPinDriveMode.Output);


			Pwm = new SofwarePwm(n => pin.Write(n ? GpioPinValue.High : GpioPinValue.Low), 10, 0);
			Servo = new ServoController(Pwm);
			Servo.Start(1);
			PropertyChanged(this, new PropertyChangedEventArgs("Angle"));
		}

		private void MainPage_Unloaded(object sender, RoutedEventArgs e)
		{
			Pwm.Dispose();
		}
	}
}
