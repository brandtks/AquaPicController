/***************************************************************************************************/
/*        NOT COMPILED                                                                             */
/***************************************************************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace AquaPic.Runtime
{
    public class BaseScript
    {
        //protected Assembly pluginAssembly;

        public IScript instance;
        public ScriptFlags flags;
        public string name;
        public string path;
        public HashSet<ScriptMessage> errors;
        
        public BaseScript (string name, string path) {
            this.name = name;
            this.path = path;
            flags = ScriptFlags.None;
            instance = null;
            errors = new HashSet<ScriptMessage> ();

            CompileAndLoad ();
        }

        public void CompileAndLoad () {
            Assembly pluginAssembly = null;
            try {
                if (Plugin.CompileCode (this)) {
                    var dllPath = Path.Combine (Utils.AquaPicEnvironment, "AquaPicRuntimeProject");
                    dllPath = Path.Combine (dllPath, "Scripts");
                    dllPath = Path.Combine (dllPath, "dll");
                    dllPath = Path.Combine (dllPath, name + ".dll");

                    pluginAssembly = Assembly.LoadFrom (dllPath);
                    flags |= ScriptFlags.Compiled;
                }
            } catch (Exception ex) {
                flags &= ~ScriptFlags.Compiled;
                //Console.WriteLine (ex.ToString ());
                errors.Add (new ScriptMessage ("Base Constructor", "  " + ex.ToString ()));
            }

            if (pluginAssembly != null)
                CreateInstance (pluginAssembly);
            else {
                flags &= ~ScriptFlags.Compiled;
                errors.Add (new ScriptMessage ("Base Constructor", "  .dll Assembly was not loaded"));
            }
        }

        protected virtual void CreateInstance (Assembly pluginAssembly) {
            if (flags.HasFlag (ScriptFlags.Compiled)) {
                foreach (var t in pluginAssembly.GetTypes ()) {
                    if (t.GetInterface ("IScript") != null) {
                        try {
                            instance = Activator.CreateInstance (t) as IScript;

                            if (instance == null) {
                                flags &= ~ScriptFlags.Compiled;
                                errors.Add (new ScriptMessage ("CreateInstance", "  Instance of script could not be created"));
                            }
                        } catch (Exception ex) {
                            flags &= ~ScriptFlags.Compiled;
                            //Console.WriteLine (ex.ToString ());
                            errors.Add (new ScriptMessage ("CreateInstance", "  " + ex.ToString ()));
                        }
                    }
                }
            }
        }

        public void RunInitialize () {
            if (flags.HasFlag (ScriptFlags.Initializer | ScriptFlags.Compiled)) {
                try {
                    IStartupScript i = instance as IStartupScript;
                    i.Initialize ();
                } catch (Exception ex) {
                    flags &= ~ScriptFlags.Compiled;
                    errors.Add (new ScriptMessage ("RunInitialize", "  " + ex.ToString ()));
                }
            }
        }

        public virtual void CyclicRun () {
            if (flags.HasFlag (ScriptFlags.Compiled | ScriptFlags.Cyclic)) {
                try {
                    var i = instance as ICyclicScript;
                    i.CyclicRun ();
                } catch (Exception ex) {
                    flags &= ~ScriptFlags.Compiled;
                    errors.Add (new ScriptMessage ("RunPlugin", "  " + ex.ToString ()));
                }
            }
        }

        public virtual void OneShotRun (ref object rtnValue) {
            if (flags.HasFlag (ScriptFlags.Compiled | ScriptFlags.Event)) {
                try {
                    var i = instance as IEventScript;
                    i.OneShotRun (ref rtnValue);
                } catch (Exception ex) {
                    flags &= ~ScriptFlags.Compiled;
                    errors.Add (new ScriptMessage ("RunPlugin", "  " + ex.ToString ()));
                }
            }
        }
    }
}

