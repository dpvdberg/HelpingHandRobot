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

    class ArduinoController
    {
        Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        Dictionary<int, int> valueHolder = new Dictionary<int, int>();

        public ArduinoController()
        {
            SetInitialValues();
            IPAddress[] IPs = Dns.GetHostAddresses("group114.local");
            socket.Connect(IPs[0], 5566);
        }

        public enum ArduinoValues
        {
            GimbalYaw = 0,
            GimbalPitch = 1,
            LeftMotorPwm = 2,
            RightMotorPwm = 3,
            LeftMotorCcw = 4,
            RightMotorCcw = 5
        }

        public void UpdateMotors(int x, int y, int trigger)
        {
            MotorData motorData = GetMotorData(x, y, trigger);
            valueHolder[(int) ArduinoValues.LeftMotorPwm] = motorData.LeftMotor.Speed;
            valueHolder[(int) ArduinoValues.LeftMotorCcw] = motorData.LeftMotor.IsCcw ? 2 : 1;
            valueHolder[(int) ArduinoValues.RightMotorPwm] = motorData.RightMotor.Speed;
            valueHolder[(int) ArduinoValues.RightMotorCcw] = motorData.RightMotor.IsCcw ? 2 : 1;
        }

        private MotorData GetMotorData(int x, int y, int trigger)
        {
            float xMapped = MapAxisFloat(x);
            float yMapped = MapAxisFloat(y);

            //Console.WriteLine($"x: {x}, xMap: {xMapped}");

            int LMotorPwm = 0;
            bool LMotorCcw = false;

            int RMotorPwm = 0;
            bool RMotorCcw = false;

            return new MotorData(
                new InnerMotorData(LMotorCcw, LMotorPwm),
                new InnerMotorData(RMotorCcw, RMotorPwm)
                );
        }

        public void UpdateValue(ArduinoValues argument, int value)
        {
            valueHolder[(int) argument] = value;
        }

        private void SetInitialValues()
        {
            XDocument xml = XDocument.Load(@"Data\ArduinoValues.xml");

            List<XElement> elements = xml.Root.Elements().ToList();

            foreach (XElement element in elements)
            {
                valueHolder.Add(
                    (int) (ArduinoValues) Enum.Parse(typeof(ArduinoValues), (string) element.Attribute("name")),
                    int.Parse(element.Value));
            }
        }

        /// <summary>
        /// Encodes data to readable format for arduino
        /// </summary>
        /// <returns></returns>
        public string GetDataString()
        {
            string data = "";
            foreach (KeyValuePair<int, int> v in valueHolder)
                data += $"{v.Key}:{v.Value}&";
            //Console.WriteLine(data.Trim('&'));
            return data.Trim('&');
        }

        public void SendData()
        {
            try
            {
                socket.Send(Encoding.UTF8.GetBytes(GetDataString()));
            }
            catch (SocketException z)
            {
                Console.WriteLine(z.Message);
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