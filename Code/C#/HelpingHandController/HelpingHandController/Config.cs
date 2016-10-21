using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace HelpingHandController
{
    class Config
    {
        public const int InitializeSweeperDelay = 100;
        public const int ArmSweepDelay = 100;
        public int ArmRotatorAngleAdjustment { get; set; }
        public int ArmShoulderAngleAdjustment { get; set; }
        public int ArmElbowAngleAdjustment { get; set; }
        public int ArmWristAngleAdjustment { get; set; }
        public int ArmGrabberAngleAdjustment { get; set; }

        public Config()
        {
            XDocument xml = XDocument.Load(@"Data\ArduinoValues.xml");

            Console.WriteLine(typeof(Config).GetProperties());

            List<XElement> elements = xml.Root.Element("ArduinoArmValues").Elements().ToList();

            
            foreach (XElement element in elements)
                GetType().GetProperty(element.Attribute("name").Value).SetValue(this, int.Parse(element.Value)); 
        }
    }
}
