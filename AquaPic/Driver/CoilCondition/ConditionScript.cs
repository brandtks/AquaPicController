using System;
using System.Collections.Generic;

namespace AquaPic.CoilCondition
{
    public class ConditionScript
    {
        private string script;
        public string Script {
            get {
                return script;
            }
            set {
                script = value;
                individualized = false;
            }
        }

        private List<string[]> IndividualScrips;
        private bool individualized;

        public ConditionScript () { 
            IndividualScrips = new List<string[]> ();
            individualized = false;
            script = string.Empty;
        }

        public bool EvaluateScript () {
            bool state = true;

            if (!individualized)
                InividualizeScript ();

            foreach (var iScript in IndividualScrips) {
                Condition c = ConditionLocker.GetCondition (iScript [1]);
                if (c != null) {
                    if (string.Compare (iScript [0], "AND") == 0)
                        state &= c.CheckState ();
                    else if (string.Compare (iScript [0], "AND NOT") == 0)
                        state &= !c.CheckState ();
                }
            }

            return state;
        }

        protected void InividualizeScript () {
            string[] tokens = script.Split (' ');
            string command = string.Empty;
            string condition = string.Empty;

            IndividualScrips.Clear ();

            foreach (var token in tokens) {
                if (string.Compare (token, "AND") == 0) {
                    if (!string.IsNullOrEmpty (command) && !string.IsNullOrEmpty (condition)) {
                        string[] i = new string[2];
                        i [0] = command;
                        i [1] = condition;
                        IndividualScrips.Add (i);
                        condition = string.Empty;
                    }

                    command = "AND";
                } else if (string.Compare (token, "NOT") == 0)
                    command += " NOT";
                else
                    condition += string.IsNullOrEmpty (condition) ? token : " " + token;
            }

            string[] ind = new string[2];
            ind [0] = command;
            ind [1] = condition;
            IndividualScrips.Add (ind);
            individualized = true;
        }
    }
}

