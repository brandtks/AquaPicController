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

        private List<string[]> iScripts;
        private bool individualized;

        public ConditionScript () { 
            iScripts = new List<string[]> ();
            individualized = false;
            script = string.Empty;
        }

        public bool EvaluateScript () {
            bool state = false;

            if (!individualized)
                IndividualizeScript ();

            string outerCommand = string.Empty;
            for (int i = 0; i < iScripts.Count; ++i) {
                bool temp, inner;
                Condition c;

                if ((string.IsNullOrEmpty (iScripts [i] [1])) && (iScripts [i] [0] != "START")) {
                    outerCommand = iScripts [i] [0];
                    ++i;
                }

                if (iScripts [i] [0] == "START") {
                    ++i;

                    c = ConditionLocker.GetCondition (iScripts [i] [1]);
                    if (c != null) {
                        temp = c.CheckState ();
                        if (iScripts [i] [0] == "NOT")
                            inner = !temp;
                        else
                            inner = temp;
                    } else
                        inner = true;

                    while (iScripts [++i] [0] != "END") {
                        c = ConditionLocker.GetCondition (iScripts [i] [1]);
                        if (c != null) {
                            temp = c.CheckState ();

                            if (iScripts [i] [0] == "AND")
                                inner &= temp;
                            else if (iScripts [i] [0] == "AND NOT")
                                inner &= !temp;
                            else if (iScripts [i] [0] == "OR")
                                inner |= temp;
                            else if (iScripts [i] [0] == "OR NOT")
                                inner |= !temp;
                        }
                    }

                    if (!string.IsNullOrEmpty (outerCommand)) {
                        if (outerCommand == "AND")
                            state &= inner;
                        else if (outerCommand == "AND NOT")
                            state &= !inner;
                        else if (outerCommand == "OR")
                            state |= inner;
                        else if (outerCommand == "OR NOT")
                            state |= !inner;
                        else if (outerCommand == "NOT")
                            state = !inner;
                    } else
                        state = inner;

                } else {
                    c = ConditionLocker.GetCondition (iScripts [i] [1]);
                    if (c != null) {
                        temp = c.CheckState ();

                        if (!string.IsNullOrEmpty (iScripts [i] [0])) {
                            if (iScripts [i] [0] == "AND")
                                state &= temp;
                            else if (iScripts [i] [0] == "AND NOT")
                                state &= !temp;
                            else if (iScripts [i] [0] == "OR")
                                state |= temp;
                            else if (iScripts [i] [0] == "OR NOT")
                                state |= !temp;
                            else if (iScripts [i] [0] == "NOT")
                                state = !temp;
                        } else
                            state = temp;
                    }
                }

            }

            return state;
        }

        protected void IndividualizeScript () {
            string[] tokens = script.Split (' ');
            string command = string.Empty;
            string condition = string.Empty;
            string lastToken = string.Empty;

            iScripts.Clear ();

            foreach (var token in tokens) {
                if (token == "START") {
                    if (!string.IsNullOrEmpty (lastToken))
                        AddIScript (ref command, ref condition);

                    command = "START";
                    condition = string.Empty;
                    AddIScript (ref command, ref condition);
                } else if (token == "END") {
                    AddIScript (ref command, ref condition);
                    command = token;
                    condition = string.Empty;
                    AddIScript (ref command, ref condition);
                } else if ((token == "AND") || (token == "OR")) {
                    if (lastToken != "END") // END command was already added
                        AddIScript (ref command, ref condition);
                    command = token;
                } else if (token == "NOT")
                    command += string.IsNullOrEmpty (command) ? "NOT" : " NOT";
                else
                    condition += string.IsNullOrEmpty (condition) ? token : " " + token;

                lastToken = token;
            }

            if (lastToken != "END")
                AddIScript (ref command, ref condition);
        }

        protected void AddIScript (ref string command, ref string condition) {
            string[] i = new string[2];
            i [0] = command;
            i [1] = condition;
            iScripts.Add (i);
            command = string.Empty;
            condition = string.Empty;
        }
    }
}

