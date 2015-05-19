//using System;
//using System.Reflection;
//
//namespace AquaPic.PluginRuntime
//{
//    public class OutletScript : BaseScript
//    {
//        public bool defaultReturn;
//
//        public OutletScript (string name, string path) : base (name, path) {
//            flags |= ScriptFlags.Outlet | ScriptFlags.Initializer;
//
//            defaultReturn = false;
//        }
//
//        protected override void CreateInstance (Assembly pluginAssembly) {
//            if (flags.HasFlag (ScriptFlags.Compiled)) {
//                foreach (var t in pluginAssembly.GetTypes ()) {
//                    if (t.GetInterface ("IOutletScript") != null) {
//                        try {
//                            instance = Activator.CreateInstance (t) as IOutletScript;
//
//                            if (instance == null)
//                                flags &= ~ScriptFlags.Compiled;
//                        } catch {
//                            flags &= ~ScriptFlags.Compiled;
//                        }
//                    }
//                }
//            }
//        }
//
//        public bool RunOutletCondition () {
//            if (flags.HasFlag (ScriptFlags.Compiled | ScriptFlags.Outlet)) {
//                try {
//                    var i = instance as IOutletScript;
//                    return i.OutletCondition ();
//                } catch {
//                    flags &= ~ScriptFlags.Compiled;
//                    return defaultReturn;
//                }
//            } else
//                return defaultReturn;
//        }
//    }
//}

