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
        Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        Dictionary<int, int> valueHolder = new Dictionary<int, int>();

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

        public void UpdateMotors(int x, int y, int trigger)
        {
            MotorData motorData = GetMotorData(x, y, trigger);
            valueHolder[(int) ArduinoValues.LeftMotorPwm] = motorData.LeftMotor.Speed;
            valueHolder[(int) ArduinoValues.LeftMotorCcw] = motorData.LeftMotor.IsCcw ? 2 : 1;
            valueHolder[(int) ArduinoValues.RightMotorPwm] = motorData.RightMotor.Speed;
            valueHolder[(int) ArduinoValues.RightMotorCcw] = motorData.RightMotor.IsCcw ? 2 : 1;

            //Console.WriteLine($"Lpwm: " + (motorData.LeftMotor.IsCcw ? "CCW:" : "CW:") + $"{motorData.LeftMotor.Speed}" + $" Rpwm: " + (motorData.RightMotor.IsCcw ? "CCW:" : "CW:") + $"{motorData.RightMotor.Speed}");
        }

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

        public void UpdateValue(ArduinoValues argument, int value, bool increment = false, bool limit = false)
        {
            valueHolder[(int) argument] = increment ? value + valueHolder[(int)argument] : value;
            if (limit)
                valueHolder[(int)argument] = valueHolder[(int)argument] < 0 ? 0 : valueHolder[(int)argument] > 180 ? 180 : valueHolder[(int)argument];
        }

        private void SetInitialValues()
        {
            XDocument xml = XDocument.Load(@"Data\ArduinoValues.xml");

            List<XElement> elements = xml.Root.Element("ArduinoDefaultValues").Elements().ToList();

            foreach (XElement element in elements)
            {
                valueHolder.Add(
                    (int) (ArduinoValues) Enum.Parse(typeof(ArduinoValues), (string) element.Attribute("name")),
                    int.Parse(element.Value));
            }
        }

        public void SetArmMode(ArmMode mode)
        {
            valueHolder[(int)ArduinoValues.ArmRotator] = mode.RotatorAngle;
            valueHolder[(int)ArduinoValues.ArmShoulder] = mode.ShoulderAngle;
            valueHolder[(int)ArduinoValues.ArmElbow] = mode.ElbowAngle;
            valueHolder[(int)ArduinoValues.ArmWrist] = mode.WristAngle;
            valueHolder[(int)ArduinoValues.ArmGrabber] = mode.GrabberAngle;
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
                foreach (KeyValuePair<int, int> v in valueHolder.ToList())
                    data += $"{v.Key}:{v.Value}&";
            }
            catch
            {
                return null;
            }
            return data.Trim('&');
        }

        private bool connecting = false;
        public void Connect(string ip, int port)
        {
            if (socket.Connected)
                socket.Disconnect(false);

            connecting = true;
            try
            {
                //var t = new Thread(() =>
                //{
                    IPAddress[] IPs = Dns.GetHostAddresses(ip);
                    socket.Connect(IPs[0], port);
                //});
                ArduinoStatus = "Connecting..";
                // Most ugly hack ever, need fix later
                //System.Windows.Application.Current.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Background,
                //                          new Action(delegate { }));
                //t.Start();
                //t.Join();
            } catch (Exception)
            {}
            connecting = false;
        }

        private string previous = "";
        public void SendData()
        {
            if (previous == GetDataString())
                return;
            Console.WriteLine(GetDataString());
            previous = GetDataString();
            if (connecting)
                return;

            if (socket.Connected)
            {
                string data = GetDataString();
                ArduinoStatus = "Connected";
                try
                {
                    if (data != null)
                        socket.Send(Encoding.UTF8.GetBytes(data));
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

        public static float MapAxisFloat(int x)
        {
            return
                (float)
                    (-2.63432*Math.Pow(10, -27)*x*(-1.09541*Math.Pow(10, 16) + x)*(-500 + x)*(500 + x)*
                     (0.000930051 + 9.35528*Math.Pow(10, -20)*x + 5.13256*Math.Pow(10, -14)*Math.Pow(x, 2)));
        }

        const int MAX_AXIS_VALUE = 32768;
        public static int MapAxisToServo(int value)
        {
            return (int) (-(MapAxisFloat(value)*90-90));
        }
    }
}