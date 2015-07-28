using System;
using System.Text;
using CSScriptLibrary;

namespace AquaPic.Runtime
{
    public interface IOutletScript
    {
        bool OutletConditionCheck ();
    }

    public class Script
    {
        public static IOutletScript CompileOutletConditionCheck (string[] conditions) {
            StringBuilder sb = new StringBuilder ();
            sb.AppendLine ("using AquaPic.Runtime;");
            sb.AppendLine ("using AquaPic.Modules;");
            sb.AppendLine ("using AquaPic.Utilites;");
            sb.AppendLine ("using AquaPic.Drivers;");
            sb.AppendLine ("using AquaPic.SerialBus;");
            sb.AppendLine ("public class MyOutletScipt : IOutletScript {");
            sb.AppendLine ("    public bool OutletConditionCheck () {");
            foreach (var s in conditions) {
                sb.AppendLine ("        " + s);
            }
            sb.AppendLine ("    }");
            sb.AppendLine ("}");

            string code = sb.ToString ();

            try {
                IOutletScript outletScript = CSScript.Evaluator.LoadCode<IOutletScript> (code);
                return outletScript;
            } catch (Exception ex) {
                Logger.AddError (ex.ToString ());
            }

            return null;
        }
    }
}

