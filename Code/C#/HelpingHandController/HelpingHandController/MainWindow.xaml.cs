using J2i.Net.XInputWrapper;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
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
        private ArduinoController controller = new ArduinoController();
        private Config config = new Config();
        private System.Timers.Timer timer;

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
            _selectedController = XboxController.RetrieveController(0);
            _selectedController.StateChanged += _selectedController_StateChanged;
            XboxController.StartPolling();
            timer = new System.Timers.Timer();
            timer.Interval = 15;
            timer.Elapsed += timer_Elapsed;
            timer.Start();

            SetupSweepTimers();
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

        XboxController _selectedController;
        public XboxController SelectedController
        {
            get { return _selectedController; }
        }
        
        public ArduinoController AC
        {
            get { return controller; }
        }

        volatile bool _keepRunning;

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
            controller.UpdateValue(ArduinoController.ArduinoValues.GimbalYaw, ArduinoController.MapAxisToServo(_selectedController.RightThumbStick.X));
        }

        private void RightYAxis_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            controller.UpdateValue(ArduinoController.ArduinoValues.GimbalPitch, ArduinoController.MapAxisToServo(_selectedController.RightThumbStick.Y));
        }

        private void RightTrigger_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            controller.UpdateMotors(
                _selectedController.LeftThumbStick.X,
                _selectedController.LeftThumbStick.Y,
                _selectedController.RightTrigger);
        }

        private void LeftXAxis_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            controller.UpdateMotors(
                _selectedController.LeftThumbStick.X,
                _selectedController.LeftThumbStick.Y,
                _selectedController.RightTrigger);
        }

        private void LeftYAxis_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            controller.UpdateMotors(
                _selectedController.LeftThumbStick.X,
                _selectedController.LeftThumbStick.Y,
                _selectedController.RightTrigger);
        }

        private void NumericTextBox(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !IsTextAllowed(e.Text);
        }

        private bool IsTextAllowed(string text)
        {
            Regex regex = new Regex("[^0-9.-]+");
            return !regex.IsMatch(text);
        }

        private void btnConnect_Click(object sender, RoutedEventArgs e)
        {
            controller.Connect(txtArduinoIP.Text, int.Parse(txtArduinoPort.Text));
            btnConnect.Content = txtArduinoStatus.Text == "Connected" ? "Disconnect" : "Connect";
        }

        #region ArmControl 
        private void SweepStart(System.Timers.Timer sweeper, CheckBox chk)
        {
            new Thread(() =>
            {
                Thread.Sleep(Config.InitializeSweeperDelay);
                Dispatcher.Invoke(() =>
                {
                    if ((bool)chk.IsChecked)
                        sweeper.Start();
                });
            }).Start();
        }

        System.Timers.Timer RightShoulderSweeper = new System.Timers.Timer(Config.ArmSweepDelay);
        bool RightShoulderSweepPositive;
        private void SetupSweepTimers()
        {
            RightShoulderSweeper.Elapsed += RightShoulderSweeper_Elapsed;
        }

        private void RightShoulderSweeper_Elapsed(object sender, ElapsedEventArgs e)
        {
            controller.UpdateValue(
                ArduinoController.ArduinoValues.ArmRotator,
                RightShoulderSweepPositive ? config.ArmRotatorAngleAdjustment : -config.ArmRotatorAngleAdjustment,
                true, true);
        }

        private void CheckboxRightShoulderButton_Checked(object sender, RoutedEventArgs e)
        {
            RightShoulderSweepPositive = true;
            SweepStart(RightShoulderSweeper, CheckboxRightShoulderButton);
            controller.UpdateValue(ArduinoController.ArduinoValues.ArmRotator, config.ArmRotatorAngleAdjustment, true, true);
        }

        private void CheckboxRightShoulderButton_Unchecked(object sender, RoutedEventArgs e)
        {
            RightShoulderSweeper.Stop();
        }

        private void CheckboxLeftShoulderButton_Checked(object sender, RoutedEventArgs e)
        {
            RightShoulderSweepPositive = false;
            SweepStart(RightShoulderSweeper, CheckboxLeftShoulderButton);
            controller.UpdateValue(ArduinoController.ArduinoValues.ArmRotator, -config.ArmRotatorAngleAdjustment, true, true);
        }

        private void CheckboxLeftStickButton_Unchecked(object sender, RoutedEventArgs e)
        {
            RightShoulderSweeper.Stop();
        }
        #endregion
    }
}
