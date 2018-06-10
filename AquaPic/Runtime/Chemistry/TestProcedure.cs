#region License

/*
    AquaPic Main Control - Handles all functionality for the AquaPic aquarium controller.

    Copyright (c) 2017 Goodtime Development

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see http://www.gnu.org/licenses/
*/

#endregion // License

using System;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using AquaPic.Globals;

namespace AquaPic.Runtime
{
    public class TestProcedure
    {
        public bool InProcedure {
            get {
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

            _name = (string)jo["Testing"];

            try {
                JObject jpoints = (JObject)jo["Points"];
                double x1 = Convert.ToDouble ((string)jpoints["Titration1"]);
                double y1 = Convert.ToDouble ((string)jpoints["Value1"]);
                double x2 = Convert.ToDouble ((string)jpoints["Titration2"]);
                double y2 = Convert.ToDouble ((string)jpoints["Value2"]);

                slope = (y2 - y1) / (x2 - x1);
                intercept = y1 - slope * x1;

                _unit = (string)jo["Units"];

                JArray ja = (JArray)jo["Procedure"];
                procedure = new string[ja.Count];
                for (int i = 0; i < ja.Count; ++i) {
                    procedure[i] = (string)ja[i];
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
                int start = procedure[_currentStep].IndexOf ('{');
                step = procedure[_currentStep].Substring (0, start - 1);
                action = procedure[_currentStep].Substring (start + 1, procedure[_currentStep].Length - 2 - start);
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

