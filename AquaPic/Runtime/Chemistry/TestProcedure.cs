using System;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using AquaPic.Utilites;

namespace AquaPic.Runtime
{
    public class TestProcedure
    {
        public bool InProcedure {
            get {
                //Console.WriteLine ("Is -1: {0}", _currentStep == -1);
                //Console.WriteLine ("Is array length: {0}", _currentStep == (procedure.Length - 1));
                return ((_currentStep == -1) || (_currentStep == procedure.Length));
            }
        }

        public bool Done {
            get {
                return _currentStep == procedure.Length;
            }
        }

        public bool NotStarted {
            get {
                return _currentStep == -1;
            }
        }

        private string _name;
        public string name {
            get {
                return _name;
            }
        }

        private string _unit;
        public string unit {
            get {
                return _unit;
            }
        }

        private int _currentStep;
        public int currentStep {
            get {
                return _currentStep;
            }
        }

        private string[] procedure;

        private double slope;
        private double intercept;

        public double level1;
        public double level2;

        public TestProcedure (string path) {
            JObject jo = JObject.Parse (File.ReadAllText (path));

            _name = (string)jo ["Testing"];

            try {
                JObject jpoints = (JObject)jo ["Points"];
                double x1 = Convert.ToDouble ((string)jpoints ["Titration1"]);
                double y1 = Convert.ToDouble ((string)jpoints ["Value1"]);
                double x2 = Convert.ToDouble ((string)jpoints ["Titration2"]);
                double y2 = Convert.ToDouble ((string)jpoints ["Value2"]);

                slope = (y2 - y1) / (x2 - x1);
                intercept = y1 - slope * x1;

                _unit = (string)jo ["Units"];

                JArray ja = (JArray)jo ["Procedure"];
                procedure = new string[ja.Count];
                for (int i = 0; i < ja.Count; ++i) {
                    procedure [i] = (string)ja [i];
                }
            } catch {
                throw new Exception ("Error while parsing json");
            }

            _currentStep = -1;
        }

        public void Restart () {
            _currentStep = -1;
        }

        public bool GetNextStep (out string step, out string action) {
            if (_currentStep < (procedure.Length - 1)) {
                ++_currentStep;
                int start = procedure [_currentStep].IndexOf ('{');
                step = procedure [_currentStep].Substring (0, start - 1);
                action = procedure [_currentStep].Substring (start + 1, procedure [_currentStep].Length - 2 - start);
            } else if (_currentStep == (procedure.Length - 1)) {
                _currentStep = procedure.Length;
                // Yes future self, this is ugly and hacky.
                // Its here to get the test procedure GUI to work correctly.
                step = string.Empty;
                action = string.Empty;
            } else {
                step = string.Empty;
                action = string.Empty;
            }

            return InProcedure;
        }

        public double CalculateResults () {
            double difference = level1 - level2;
            return (slope * difference) + intercept;
        }
    }
}
    


