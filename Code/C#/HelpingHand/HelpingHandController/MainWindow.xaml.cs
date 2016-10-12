using J2i.Net.XInputWrapper;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace HelpingHandController
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        ArduinoController controller = new ArduinoController();
        private Timer timer;

        const int MAX_AXIS_VALUE = 32768;

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
            _selectedController = XboxController.RetrieveController(0);
            _selectedController.StateChanged += _selectedController_StateChanged;
            XboxController.StartPolling();
            timer = new Timer();
            timer.Interval = 15;
            timer.Elapsed += timer_Elapsed;
            timer.Start(); 
        }

        private void timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            controller.SendData();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            XboxController.StopPolling();
            base.OnClosing(e);
        }

        void _selectedController_StateChanged(object sender, XboxControllerStateChangedEventArgs e)
        {
            OnPropertyChanged("SelectedController");
        }

        private int MapAxisToServo(int value)
        {
            return (int) (180.0/(2*MAX_AXIS_VALUE)*(-value + MAX_AXIS_VALUE));
        }

        
        public XboxController SelectedController
        {

            get { return _selectedController; }
        }


        volatile bool _keepRunning;
        XboxController _selectedController;


        public void OnPropertyChanged(string name)
        {
            if (PropertyChanged != null)
            {
                Action a = ()=>{PropertyChanged(this, new PropertyChangedEventArgs(name));};
                Dispatcher.BeginInvoke(a, null);
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;

        private void SelectedControllerChanged(object sender, SelectionChangedEventArgs e)
        {
            _selectedController = XboxController.RetrieveController(((ComboBox)sender).SelectedIndex);
            OnPropertyChanged("SelectedController");
        }

        private void SendVibration_Click(object sender, RoutedEventArgs e)
        {
            double leftMotorSpeed = LeftMotorSpeed.Value;
            double rightMotorSpeed = RightMotorSpeed.Value;
            _selectedController.Vibrate(leftMotorSpeed, rightMotorSpeed, TimeSpan.FromSeconds(2));
        }

        private void CheckboxDPadDownButton_Checked(object sender, RoutedEventArgs e)
        {
            controller.SendData();
        }

        private void CheckboxDPadUpButton_Checked(object sender, RoutedEventArgs e)
        {
            //controller.SendData(1001);
        }

        private void CheckboxDPadRightButton_Checked(object sender, RoutedEventArgs e)
        {
            //controller.SendData(1004);
        }

        private void CheckboxDPadLeftButton_Checked(object sender, RoutedEventArgs e)
        {
            //controller.SendData(1002);
        }

        private void RightXAxis_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            controller.UpdateValue(ArduinoController.ArduinoValues.RightXaxis, MapAxisToServo(_selectedController.RightThumbStick.X));
        }

        private void RightYAxis_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            controller.UpdateValue(ArduinoController.ArduinoValues.RightYaxis, MapAxisToServo(_selectedController.RightThumbStick.Y));
        }

        private void RightTrigger_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            controller.UpdateValue(ArduinoController.ArduinoValues.RightTrigger, _selectedController.RightTrigger);
        }
    }
}
