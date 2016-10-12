using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Xml.Linq;
using HelpingHandController.Utilities;

namespace HelpingHandController
{
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
            RightXaxis = 0,
            RightYaxis = 1,
            RightTrigger = 2
        }

        public void UpdateValue(ArduinoValues argument, int value)
        {
            valueHolder[(int)argument] = value;
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
            Console.WriteLine(data.Trim('&'));
            return data.Trim('&');
        }

        public void SendData(bool addNewLine = false)
        {
            try
            {
                socket.Send(Encoding.UTF8.GetBytes(GetDataString() + (addNewLine ? "\n" : "")));
            }
            catch (SocketException z)
            {
                Console.WriteLine(z.Message);
            }
        }
    }
}