using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSScriptLibrary;
using AquaPic.Drivers;
using AquaPic.Utilites;

namespace AquaPic.Runtime
{
    public interface IOutletScript {
        bool OutletConditionCheck ();
    }

    public class Script
    {
        public static IOutletScript CompileOutletConditionCheck (string[] conditions) {
            try {
                return CompileOutletConditionCheckNoCatch (conditions);
            } catch (Exception ex) {
                Logger.AddError (ex.ToString ());
            }

            return null;
        }

        public static IOutletScript CompileOutletConditionCheckNoCatch (string[] conditions) {
            List<string> cond = conditions.ToList<string> ();
            List<string> preprocess = new List<string> ();

            foreach (var l in cond) {
                string line = l.Trim ();
                if (line.StartsWith ("#hardware"))
                    preprocess.Add (line);
            }

            foreach (var rm in preprocess)
                cond.Remove (rm);

            conditions = cond.ToArray ();

            StringBuilder sb = new StringBuilder ();
            sb.AppendLine ("using AquaPic.Runtime;");
            sb.AppendLine ("using AquaPic.Modules;");
            sb.AppendLine ("using AquaPic.Utilites;");
            sb.AppendLine ("using AquaPic.Drivers;");
            sb.AppendLine ("using AquaPic.SerialBus;");
            sb.AppendLine ("public class MyOutletScipt : IOutletScript {");
            sb.AppendLine ("public bool OutletConditionCheck () {");
            foreach (var s in conditions) {
                sb.AppendLine (s);
            }
            sb.AppendLine ("}");
            sb.AppendLine ("}");

            string code = sb.ToString ();

            IOutletScript outletScript = CSScript.Evaluator.LoadCode<IOutletScript> (code);

            //"preprocessor" conditions are ran after the code is compiled because these add actual real world stuff
            if (outletScript != null) {
                foreach (var pre in preprocess) {
                    string line = pre;
                    int idx = line.IndexOf (' ') + 1;
                    line = line.Substring (idx);
                    idx = line.IndexOf (' ');
                    string eq = line.Substring (0, idx);
                    line = line.Substring (idx + 1);
                    if (string.Equals (eq, "DigitalInput", StringComparison.InvariantCultureIgnoreCase)) {
                        var args = line.Split (',');
                        int cardId = DigitalInput.GetCardIndex (args [0]);
                        int inputId = Convert.ToInt32 (args [1]);
                        DigitalInput.AddInput (cardId, inputId, args [2]);
                    }
                }
            }

            return outletScript;
        }
    }
}

