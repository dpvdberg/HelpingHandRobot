using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace HelpingHandController
{
    public class ArmMode
    {
        private readonly int _rotator;
        private readonly int _shoulder;
        private readonly int _elbow;
        private readonly int _wrist;
        private readonly int _grabber;

        public ArmMode(
            int rotator,
            int shoulder,
            int elbow,
            int wrist,
            int graber)
        {
            _rotator = rotator;
            _shoulder = shoulder;
            _elbow = elbow;
            _wrist = wrist;
            _grabber = GrabberAngle;
        }

        public int RotatorAngle { get { return _rotator; } }
        public int ShoulderAngle { get { return _shoulder; } }
        public int ElbowAngle { get { return _elbow; } }
        public int WristAngle { get { return _wrist; } }
        public int GrabberAngle { get { return _grabber; } }
    }

    class Config
    {
        public const int InitializeSweeperDelay = 100;
        public int ArmRotatorAngleAdjustment { get; set; }
        public int ArmShoulderAngleAdjustment { get; set; }
        public int ArmElbowAngleAdjustment { get; set; }
        public int ArmWristAngleAdjustment { get; set; }
        public int ArmGrabberAngleAdjustment { get; set; }
        public int SweepDelay { get; set; }

        public Dictionary<string, ArmMode> ArmModes = new Dictionary<string, ArmMode>();

        public Config()
        {
            XDocument xml = XDocument.Load(@"Data\ArduinoValues.xml");

            Console.WriteLine(typeof(Config).GetProperties());

            List<XElement> elements = xml.Root.Element("ArduinoArmValues").Elements("ArduinoValue").ToList();
            
            foreach (XElement element in elements)
                GetType().GetProperty(element.Attribute("name").Value).SetValue(this, int.Parse(element.Value));

            List<XElement> modes = xml.Root.Element("ArduinoArmValues").Element("Modes").Elements().ToList();
            foreach (XElement mode in modes)
            {
                Dictionary<string, int> armValues = new Dictionary<string, int>();
                foreach (XElement armvalue in mode.Elements().ToList())
                    armValues.Add(armvalue.Attribute("name").Value, int.Parse(armvalue.Value));
                ArmModes.Add(mode.Name.ToString(), new ArmMode(
                    armValues["ArmRotator"],
                    armValues["ArmShoulder"],
                    armValues["ArmElbow"],
                    armValues["ArmWrist"],
                    armValues["ArmGrabber"]));

            }
        }
    }
}
