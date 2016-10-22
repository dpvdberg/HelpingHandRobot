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

            SetupSweepers();
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
        public class Sweeper
        {
            private Config config = new Config();
            private ArduinoController.ArduinoValues _arduinoValue;
            private bool IsSweepPositive;
            private CheckBox _positive;
            private CheckBox _negative;
            private MainWindow _window;
            private ArduinoController _controller;
            private System.Timers.Timer timer;
            private int angle;
            public Sweeper(ArduinoController.ArduinoValues ArduinoValue,
                ref CheckBox SweepPositive,
                ref CheckBox SweepNegative,
                ref ArduinoController controller,
                int SweepAngle)
            {
                _arduinoValue = ArduinoValue;
                _positive = SweepPositive;
                _negative = SweepNegative;
                _controller = controller;
                _window = (MainWindow)Application.Current.MainWindow;
                timer = new System.Timers.Timer(config.SweepDelay);
                angle = SweepAngle;

                SetSweepers();
            }

            private void SetSweepers()
            {
                timer.Elapsed += TimerElapsed;

                _positive.Checked += CheckChanged;
                _positive.Unchecked += CheckChanged;

                _negative.Checked += CheckChanged;
                _negative.Unchecked += CheckChanged;
            }

            private void TimerElapsed(object sender, ElapsedEventArgs e)
            {
                _controller.UpdateValue(_arduinoValue, IsSweepPositive ? angle : -angle, true, true);
            }

            private void CheckChanged(object sender, RoutedEventArgs e)
            {
                CheckBox currentCheckbox = (CheckBox)sender;
                IsSweepPositive = currentCheckbox == _positive;

                new Thread(() => {
                    _window.Dispatcher.Invoke(() => InitializeSweep(currentCheckbox));
                }).Start();
            }

            private void InitializeSweep(CheckBox CurrentCheckbox)
            {
                if ((bool)CurrentCheckbox.IsChecked)
                {
                    _controller.UpdateValue(_arduinoValue, IsSweepPositive ? angle : -angle, true, true);
                    Thread.Sleep(Config.InitializeSweeperDelay);
                    if ((bool)CurrentCheckbox.IsChecked)
                        timer.Start();
                }
                else
                {
                    timer.Stop();
                }
            }
        }
        
        private void SetupSweepers()
        {
            new Sweeper(
                ArduinoController.ArduinoValues.ArmRotator,
                ref CheckboxDPadRightButton,
                ref CheckboxDPadLeftButton,
                ref controller,
                config.ArmRotatorAngleAdjustment);

            new Sweeper(
                ArduinoController.ArduinoValues.ArmShoulder,
                ref CheckboxDPadUpButton,
                ref CheckboxDPadDownButton,
                ref controller,
                config.ArmShoulderAngleAdjustment);

            new Sweeper(
                ArduinoController.ArduinoValues.ArmElbow,
                ref CheckboxRightShoulderButton,
                ref CheckboxLeftShoulderButton,
                ref controller,
                config.ArmShoulderAngleAdjustment);

            new Sweeper(
                ArduinoController.ArduinoValues.ArmWrist,
                ref CheckboxStartButton,
                ref CheckboxBackButton,
                ref controller,
                config.ArmWristAngleAdjustment);

            new Sweeper(
                ArduinoController.ArduinoValues.ArmGrabber,
                ref CheckboxAButton,
                ref CheckboxBButton,
                ref controller,
                config.ArmGrabberAngleAdjustment);
        }

        private bool Ydouble = false;
        /// <summary>
        /// Double tap sets arm to specific mode
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CheckboxYButton_Checked(object sender, RoutedEventArgs e)
        {
            if (Ydouble)
            {
                Ydouble = false;
                controller.SetArmMode(config.ArmModes["Tower"]);
            }
            else
            {
                Ydouble = true;
                new Thread(() =>
                {
                    Thread.Sleep(500);
                    Ydouble = false;
                }).Start();
            }
        }

        private bool Xdouble = false;
        /// <summary>
        /// Double tap sets arm to specific mode
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CheckboxXButton_Checked(object sender, RoutedEventArgs e)
        {
            if (Xdouble)
            {
                Xdouble = false;
                controller.SetArmMode(config.ArmModes["Low"]);
            }
            else
            {
                Xdouble = true;
                new Thread(() =>
                {
                    Thread.Sleep(500);
                    Xdouble = false;
                }).Start();
            }
        }
        #endregion
    }
}
