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
        Dictionary<string, int> valueHolder = new Dictionary<string, int>();


        public ArduinoController()
        {
            SetInitialValues();
            IPAddress[] IPs = Dns.GetHostAddresses("group114.local");
            socket.Connect(IPs[0], 5566);
        }

        public enum ArduinoValues
        {
            [StringValue("GXA")] GimbalXaxis,
            [StringValue("GYA")] GimbalYaxis,
        }

        public void UpdateValue(ArduinoValues argument, int value)
        {
            valueHolder[StringEnum.GetStringValue(argument)] = value;
        }

        private void SetInitialValues()
        {
            XDocument xml = XDocument.Load(@"Data\ArduinoValues.xml");

            List<XElement> elements = xml.Root.Elements().ToList();

            foreach (XElement element in elements)
            {
                valueHolder.Add(
                    StringEnum.GetStringValue(
                        (ArduinoValues) Enum.Parse(typeof(ArduinoValues), (string) element.Attribute("name"))),
                    int.Parse(element.Value));
            }
        }


        public int GetDataString()
        {
            return valueHolder["GXA"];
            string data = "";
            foreach (KeyValuePair<string, int> v in valueHolder)
                data += $"{v.Key}:{v.Value}|";
            //return data;
        }

        public void SendData(bool addNewLine = true)
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