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
        public static IOutletScript CompileOutletConditionCheck (IEnumerable<string> conditions) {
            try {
                return CompileOutletConditionCheckNoCatch (conditions);
            } catch (Exception ex) {
                Logger.AddError (ex.ToString ());
            }

            return null;
        }

        public static IOutletScript CompileOutletConditionCheckNoCatch (IEnumerable<string> conditions) {
            List<string> cond = conditions.ToList<string> ();
            List<string> preprocess = new List<string> ();

            foreach (var l in cond) {
                string line = l.Trim ();
                if (line.StartsWith ("#hardware"))
                    preprocess.Add (line);
            }

            foreach (var rm in preprocess)
                cond.Remove (rm);

            StringBuilder sb = new StringBuilder ();
            sb.AppendLine ("using AquaPic.Runtime;");
            sb.AppendLine ("using AquaPic.Modules;");
            sb.AppendLine ("using AquaPic.Utilites;");
            sb.AppendLine ("using AquaPic.Drivers;");
            sb.AppendLine ("using AquaPic.SerialBus;");
            sb.AppendLine ("public class MyOutletScipt : IOutletScript {");
            sb.AppendLine ("public bool OutletConditionCheck () {");
            foreach (var s in cond) {
                sb.AppendLine (s);
            }
            sb.AppendLine ("}");
            sb.AppendLine ("}");

            string code = sb.ToString ();

            IOutletScript outletScript = CSScript.Evaluator.LoadCode<IOutletScript> (code);

            //"preprocessor" conditions are ran after the code is compiled because these add actual real world stuff
            if (outletScript != null) {
                EvaluatePreprocessor (preprocess);
            }

            return outletScript;
        }

        protected static void EvaluatePreprocessor (IEnumerable<string> preprocess) {
            foreach (var pre in preprocess) {
                string line = pre;
                int idx = line.IndexOf (' ') + 1;
                line = line.Substring (idx);
                idx = line.IndexOf (' ');
                string eq = line.Substring (0, idx);
                line = line.Substring (idx + 1);

                if (string.Equals (eq, "DigitalInput", StringComparison.InvariantCultureIgnoreCase)) {
                    var args = line.Split (',');
                    int cardId = AquaPicDrivers.DigitalInput.GetCardIndex (args [0]);
                    int inputId = Convert.ToInt32 (args [1]);
                    AquaPicDrivers.DigitalInput.AddChannel (cardId, inputId, args [2]);
                }
            }
        }

        public static void UndoPreprocessor (IEnumerable<string> conditions) {
            List<string> preprocess = new List<string> ();

            foreach (var l in conditions) {
                string line = l.Trim ();
                if (line.StartsWith ("#hardware"))
                    preprocess.Add (line);
            }

            foreach (var pre in preprocess) {
                string line = pre;
                int idx = line.IndexOf (' ') + 1;
                line = line.Substring (idx);
                idx = line.IndexOf (' ');
                string eq = line.Substring (0, idx);
                line = line.Substring (idx + 1);

                if (string.Equals (eq, "DigitalInput", StringComparison.InvariantCultureIgnoreCase)) {
                    var args = line.Split (',');
                    int cardId = AquaPicDrivers.DigitalInput.GetCardIndex (args [0]);
                    int inputId = Convert.ToInt32 (args [1]);
                    AquaPicDrivers.DigitalInput.RemoveChannel (cardId, inputId);
                }
            }
        }
    }
}

