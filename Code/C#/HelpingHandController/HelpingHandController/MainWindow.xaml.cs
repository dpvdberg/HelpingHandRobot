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
        private ArduinoController Controller = new ArduinoController();
        private Config Config = new Config();
        private System.Timers.Timer ArduinoTimer;

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
            _selectedController = XboxController.RetrieveController(0);
            _selectedController.StateChanged += _selectedController_StateChanged;
            XboxController.StartPolling();
            ArduinoTimer = new System.Timers.Timer();
            ArduinoTimer.Interval = Config.DataSendDelay;
            ArduinoTimer.Elapsed += ArduinoTimer_Elapsed;
            ArduinoTimer.Start();

            System.Timers.Timer DebugTimer = new System.Timers.Timer();
            DebugTimer.Interval = 200;
            DebugTimer.Elapsed += DebugTimer_Elapsed;
            DebugTimer.Start();

            SetupSweepers();
        }

        private void ArduinoTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            Controller.SetMotorValues();
            Controller.SendData();
        }

        private void DebugTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            Console.WriteLine(Controller.GetDataString());
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
            get { return Controller; }
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

        private void RightXAxis_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Controller.UpdateValue(ArduinoController.ArduinoValues.GimbalYaw, ArduinoController.MapAxisToServo(_selectedController.RightThumbStick.X));
        }

        private void RightYAxis_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Controller.UpdateValue(ArduinoController.ArduinoValues.GimbalPitch, ArduinoController.MapAxisToServo(_selectedController.RightThumbStick.Y));
        }

        private void RightTrigger_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Controller.UpdateMotors(
                _selectedController.LeftThumbStick.X,
                _selectedController.LeftThumbStick.Y,
                _selectedController.RightTrigger);
        }

        private void LeftXAxis_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Controller.UpdateMotors(
                _selectedController.LeftThumbStick.X,
                _selectedController.LeftThumbStick.Y,
                _selectedController.RightTrigger);
        }

        private void LeftYAxis_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Controller.UpdateMotors(
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
            Controller.Connect(txtArduinoIP.Text, int.Parse(txtArduinoPort.Text));
            btnConnect.Content = Controller.Socket.Connected ? "Disconnect" : "Connect";
        }

        #region ArmControl 
        public class Sweeper
        {
            private Config Config = new Config();
            private ArduinoController.ArduinoValues _arduinoValue;
            private bool IsSweepPositive;
            private CheckBox _positive;
            private CheckBox _negative;
            private MainWindow _window;
            private ArduinoController _controller;
            private System.Timers.Timer Timer;
            private int Angle;
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
                Timer = new System.Timers.Timer(Config.SweepDelay);
                Angle = SweepAngle;

                SetSweepers();
            }

            private void SetSweepers()
            {
                Timer.Elapsed += TimerElapsed;

                _positive.Checked += CheckChanged;
                _positive.Unchecked += CheckChanged;

                _negative.Checked += CheckChanged;
                _negative.Unchecked += CheckChanged;
            }

            private void TimerElapsed(object sender, ElapsedEventArgs e)
            {
                _controller.UpdateValue(_arduinoValue, IsSweepPositive ? Angle : -Angle, true, true);
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
                    _controller.UpdateValue(_arduinoValue, IsSweepPositive ? Angle : -Angle, true, true);
                    Thread.Sleep(Config.InitializeSweeperDelay);
                    if ((bool)CurrentCheckbox.IsChecked)
                        Timer.Start();
                }
                else
                {
                    Timer.Stop();
                }
            }
        }
        
        private void SetupSweepers()
        {
            new Sweeper(
                ArduinoController.ArduinoValues.ArmRotator,
                ref CheckboxDPadRightButton,
                ref CheckboxDPadLeftButton,
                ref Controller,
                Config.ArmRotatorAngleAdjustment);

            new Sweeper(
                ArduinoController.ArduinoValues.ArmShoulder,
                ref CheckboxDPadUpButton,
                ref CheckboxDPadDownButton,
                ref Controller,
                Config.ArmShoulderAngleAdjustment);

            new Sweeper(
                ArduinoController.ArduinoValues.ArmElbow,
                ref CheckboxRightShoulderButton,
                ref CheckboxLeftShoulderButton,
                ref Controller,
                Config.ArmShoulderAngleAdjustment);

            new Sweeper(
                ArduinoController.ArduinoValues.ArmWrist,
                ref CheckboxStartButton,
                ref CheckboxBackButton,
                ref Controller,
                Config.ArmWristAngleAdjustment);

            new Sweeper(
                ArduinoController.ArduinoValues.ArmGrabber,
                ref CheckboxAButton,
                ref CheckboxBButton,
                ref Controller,
                Config.ArmGrabberAngleAdjustment);
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
                Controller.SetArmMode(Config.ArmModes["Tower"]);
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
                Controller.SetArmMode(Config.ArmModes["Low"]);
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
