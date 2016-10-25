using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Xml.Linq;
using HelpingHandController.Utilities;
using System.ComponentModel;
using System.Threading;

namespace HelpingHandController
{
    class InnerMotorData
    {
        public bool IsCcw;
        public int Speed;

        public InnerMotorData(bool isCcw, int speed)
        {
            IsCcw = isCcw;
            Speed = speed;
        }
    }

    class MotorData
    {
        public InnerMotorData LeftMotor;
        public InnerMotorData RightMotor;

        public MotorData(InnerMotorData LeftMotor, InnerMotorData RightMotor)
        {
            this.LeftMotor = LeftMotor;
            this.RightMotor = RightMotor;
        }
    }

    public class ArduinoController : INotifyPropertyChanged
    {
        private MotorData CurrentMotorData;
        private MotorData TargetMotorData;
        private Config Config = new Config();
        private Dictionary<int, int> ValueHolder = new Dictionary<int, int>();
        private bool IsTransitioning = false;


        public Socket Socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        private string _ArduinoStatus;
        public string ArduinoStatus
        {
            get { return _ArduinoStatus; }
            set
            {
                _ArduinoStatus = value;
                NotifyPropertyChanged("ArduinoStatus");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void NotifyPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public ArduinoController()
        {
            SetInitialValues();
        }

        public enum ArduinoValues
        {
            GimbalYaw = 0,
            GimbalPitch = 1,
            LeftMotorPwm = 2,
            RightMotorPwm = 3,
            LeftMotorCcw = 4,
            RightMotorCcw = 5,
            ArmRotator = 6,
            ArmShoulder = 7,
            ArmElbow = 8,
            ArmWrist = 9,
            ArmGrabber = 10
        }

        /// <summary>
        /// This method will be called on the timer interval
        /// </summary>
        public void SetMotorValues()
        {
            MotorData intermediateData = CurrentMotorData;

            if (TargetMotorData == null)
                TargetMotorData = CurrentMotorData;
            
            intermediateData.LeftMotor = IntermediatePwmCalculator(CurrentMotorData.LeftMotor, TargetMotorData.LeftMotor);
            intermediateData.RightMotor = IntermediatePwmCalculator(CurrentMotorData.RightMotor, TargetMotorData.RightMotor);

            ValueHolder[(int)ArduinoValues.LeftMotorPwm] = intermediateData.LeftMotor.Speed;
            ValueHolder[(int)ArduinoValues.LeftMotorCcw] = intermediateData.LeftMotor.IsCcw ? 1 : 0;
            ValueHolder[(int)ArduinoValues.RightMotorPwm] = intermediateData.RightMotor.Speed;
            ValueHolder[(int)ArduinoValues.RightMotorCcw] = intermediateData.RightMotor.IsCcw ? 1 : 0;
        }

        /// <summary>
        /// Calculates intermediate PWM using old InnerMotorData and target InnerMotorData
        /// </summary>
        /// <param name="oldData"></param>
        /// <param name="targetData"></param>
        /// <returns></returns>
        private InnerMotorData IntermediatePwmCalculator(InnerMotorData oldData, InnerMotorData targetData)
        {
            InnerMotorData steppedData = oldData;

            int allowedPwmChange = Config.MaxPWMChangePerKnock;

            bool isMotorDirectionChanged = targetData.IsCcw != oldData.IsCcw;

            int pwmStepped;
            // Distinguish cases; do we need to change direction
            if (isMotorDirectionChanged)
            {
                // Let's go down to zero
                pwmStepped = oldData.Speed - allowedPwmChange;
                if (pwmStepped < 0)
                {
                    // If we hit zero, set to zero and change polarity
                    steppedData.IsCcw = !steppedData.IsCcw;
                    pwmStepped = 0;
                }
            }
            else
            {
                // Is our change positive or negative
                bool pwmChangeNegative = targetData.Speed < oldData.Speed;
                if (pwmChangeNegative)
                {
                    // Let's step down
                    pwmStepped = oldData.Speed - allowedPwmChange;
                    // If we stepped over goal, set to goal
                    pwmStepped = pwmStepped < targetData.Speed ? targetData.Speed : pwmStepped;
                }
                else
                {
                    // Let's step up
                    pwmStepped = oldData.Speed + allowedPwmChange;
                    // If we stepped over goal, set to goal
                    pwmStepped = pwmStepped > targetData.Speed ? targetData.Speed : pwmStepped;
                }
            }
            steppedData.Speed = pwmStepped;
            return steppedData;
        }

        /// <summary>
        /// Set target
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="trigger"></param>
        public void UpdateMotors(int x, int y, int trigger)
        {
            TargetMotorData = GetMotorData(x, y, trigger);
        }

        /// <summary>
        /// Create motor values based on thumbstick and trigger values.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="trigger"></param>
        /// <returns></returns>
        private MotorData GetMotorData(int x, int y, int trigger)
        {
            float xMapped = MapAxisFloat(x);
            float yMapped = MapAxisFloat(y);

            int LMotorPwm = (int) (yMapped * trigger + xMapped / 2 * trigger);
            bool LMotorCcw = LMotorPwm < 0;
            LMotorPwm = Math.Abs(LMotorPwm) > 255 ? 255 : Math.Abs(LMotorPwm);
            LMotorPwm = Math.Abs(LMotorPwm) > 255 ? 255 : Math.Abs(LMotorPwm);

            int RMotorPwm = (int) (yMapped * trigger - xMapped / 2 * trigger);
            bool RMotorCcw = RMotorPwm > 0;
            RMotorPwm = Math.Abs(RMotorPwm) > 255 ? 255 : Math.Abs(RMotorPwm);

            return new MotorData(
                new InnerMotorData(LMotorCcw, LMotorPwm),
                new InnerMotorData(RMotorCcw, RMotorPwm)
                );
        }

        /// <summary>
        /// Updates a specific value in valueHolder
        /// </summary>
        /// <param name="argument"></param>
        /// <param name="value"></param>
        /// <param name="increment"></param>
        /// <param name="limit"></param>
        public void UpdateValue(ArduinoValues argument, int value, bool increment = false, bool limit = false)
        {
            if (IsTransitioning)
                return;
            ValueHolder[(int) argument] = increment ? value + ValueHolder[(int)argument] : value;
            if (limit)
                ValueHolder[(int)argument] = ValueHolder[(int)argument] < 0 ? 0 : ValueHolder[(int)argument] > 180 ? 180 : ValueHolder[(int)argument];
        }

        /// <summary>
        /// Set valueHolder initial values
        /// </summary>
        private void SetInitialValues()
        {
            XDocument xml = XDocument.Load(@"Data\ArduinoValues.xml");

            List<XElement> elements = xml.Root.Element("ArduinoDefaultValues").Elements().ToList();

            foreach (XElement element in elements)
            {
                ValueHolder.Add(
                    (int) (ArduinoValues) Enum.Parse(typeof(ArduinoValues), (string) element.Attribute("name")),
                    int.Parse(element.Value));
            }

            CurrentMotorData = new MotorData(
                new InnerMotorData(ValueHolder[(int)ArduinoValues.LeftMotorCcw] == 1, ValueHolder[(int)ArduinoValues.LeftMotorPwm]),
                new InnerMotorData(ValueHolder[(int)ArduinoValues.LeftMotorCcw] == 1, ValueHolder[(int)ArduinoValues.LeftMotorPwm])
                );
        }

        /// <summary>
        /// Initializes transition to new arm mode
        /// </summary>
        /// <param name="newMode"></param>
        public void SetArmMode(ArmMode newMode)
        {
            IsTransitioning = true;

            ArmMode oldMode = new ArmMode(
                ValueHolder[(int)ArduinoValues.ArmRotator],
                ValueHolder[(int)ArduinoValues.ArmShoulder],
                ValueHolder[(int)ArduinoValues.ArmElbow],
                ValueHolder[(int)ArduinoValues.ArmWrist],
                ValueHolder[(int)ArduinoValues.ArmGrabber]
                );

            new Thread(() =>
            {
                TransitionArmMode(oldMode, newMode);
                IsTransitioning = false;
            }).Start();
        }

        /// <summary>
        /// Transitions to new arm mode using recursion
        /// </summary>
        /// <param name="oldMode"></param>
        /// <param name="newMode"></param>
        /// <param name="currentTime"></param>
        private void TransitionArmMode(ArmMode oldMode, ArmMode newMode, double currentTime = 0.0)
        {
            double transitionConstant = currentTime / Config.ModeTransitionDuration;

            bool finalTransition = false;
            if (transitionConstant > 1)
            {
                transitionConstant = 1;
                finalTransition = true;
            }
            ValueHolder[(int)ArduinoValues.ArmRotator] = LinearTransition(transitionConstant, oldMode.RotatorAngle, newMode.RotatorAngle);
            ValueHolder[(int)ArduinoValues.ArmShoulder] = LinearTransition(transitionConstant, oldMode.ShoulderAngle, newMode.ShoulderAngle);
            ValueHolder[(int)ArduinoValues.ArmElbow] = LinearTransition(transitionConstant, oldMode.ElbowAngle, newMode.ElbowAngle);
            ValueHolder[(int)ArduinoValues.ArmWrist] = LinearTransition(transitionConstant, oldMode.WristAngle, newMode.WristAngle);
            ValueHolder[(int)ArduinoValues.ArmGrabber] = LinearTransition(transitionConstant, oldMode.GrabberAngle, newMode.GrabberAngle);

            Thread.Sleep(Config.ModeTransitionDelay);
            
            if (!finalTransition)
                TransitionArmMode(oldMode, newMode, currentTime + Config.ModeTransitionDelay);
        }

        /// <summary>
        /// Simple linear transition of from oldValue to newValue
        /// using a transitionConstant in the interval [0,1]
        /// </summary>
        /// <param name="transitionConstant"></param>
        /// <param name="oldValue"></param>
        /// <param name="newValue"></param>
        /// <returns></returns>
        private int LinearTransition(double transitionConstant, int oldValue, int newValue)
        {
            return (int) Math.Floor((1 - transitionConstant) * oldValue + newValue * transitionConstant);
        }

        /// <summary>
        /// Encodes data to readable format for arduino
        /// </summary>
        /// <returns></returns>
        public string GetDataString()
        {
            string data = "";
            try
            {
                foreach (KeyValuePair<int, int> v in ValueHolder.ToList())
                    data += $"{v.Key}:{v.Value}&";
            }
            catch
            {
                return null;
            }
            return data.Trim('&');
        }
        
        /// <summary>
        /// Connect to Arduino at ip:port
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        public void Connect(string ip, int port)
        {
            if (Socket.Connected)
                Socket.Disconnect(false);
            else
            {
                try
                {
                    IPAddress[] IPs = Dns.GetHostAddresses(ip);
                    Socket.Connect(IPs[0], port);
                }
                catch (Exception)
                { }
            }
        }

        private string previous = "";
        /// <summary>
        /// Sends data to the Arduino
        /// </summary>
        public void SendData()
        {
            string data = GetDataString();
            Console.WriteLine(data);

            if (previous == data)
                return;
            previous = data;

            if (Socket.Connected)
            {
                ArduinoStatus = "Connected";
                try
                {
                    if (data != null)
                    {
                        
                        Socket.Send(Encoding.UTF8.GetBytes(data));
                    }
                    else
                        Console.WriteLine("Malfunctioned data!");
                }
                catch (SocketException z)
                {
                    Console.WriteLine(z.Message);
                }
            } else
            {
                ArduinoStatus = "Disconnected";
            }
        }

        /// <summary>
        /// Function made in Mathematica to create a deadzone around x = 0.
        /// x is mappped from [-MAX_AXIS_VALUE,MAX_AXIS_VALUE] to [-1,1].
        /// 
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        public static float MapAxisFloat(int x)
        {
            return
                (float)
                    (-2.63432*Math.Pow(10, -27)*x*(-1.09541*Math.Pow(10, 16) + x)*(-500 + x)*(500 + x)*
                     (0.000930051 + 9.35528*Math.Pow(10, -20)*x + 5.13256*Math.Pow(10, -14)*Math.Pow(x, 2)));
        }

        /// <summary>
        /// Uses MapAxisFloat function to map from [0,180], creating a dead zone around 90.
        /// </summary>
        const int MAX_AXIS_VALUE = 32768;
        public static int MapAxisToServo(int value)
        {
            return (int) (-(MapAxisFloat(value)*90-90));
        }
    }
}