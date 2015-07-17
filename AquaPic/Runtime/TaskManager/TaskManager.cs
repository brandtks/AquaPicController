using System;
using System.Collections.Generic;
using Gtk;
using AquaPic.Drivers;
using AquaPic.Modules;
using AquaPic.Utilites;

namespace AquaPic.Runtime
{
    public delegate void RunHandler ();

    public class TaskManager
    {
        private static Dictionary<uint, List<ICyclicTask>> cyclicTasks = new Dictionary<uint, List<ICyclicTask>> ();
        private static List<ITodTask> todTasks = new List<ITodTask> ();

        static TaskManager () {
            AddCyclicInterrupt ("TimeOfDayInterrupts", 5000, () => {
                DateTime now = DateTime.Now;

                if ((now.Second >= 0) && (now.Second <= 4)) {
                    foreach (var task in todTasks) {
                        if (task.time.EqualsShortTime (now))
                            task.OnRun ();
                    }
                }
            });
        }

        public static void AddCyclicInterrupt (string name, uint timeInterval, RunHandler OnRun) {
            if (CyclicInterruptExists (name))
                throw new Exception ("Task already exists");

            if (!cyclicTasks.ContainsKey (timeInterval)) { // this time interval doesn't exist, add it
                cyclicTasks.Add (timeInterval, new List<ICyclicTask> ());

                #if SIMULATION
                uint time = timeInterval * 4;
                #else
                uint time = timeInterval;
                #endif

                GLib.Timeout.Add (time, () => {
                    foreach (var task in cyclicTasks[timeInterval]) {
                        try {
                            task.OnRun ();
                        } catch (Exception ex) {
                            Logger.AddError (string.Format ("{0} throw an exception", task.name));
                            Logger.AddError (ex.ToString ());
                        }
                    }

                    return true;
                });
            }

            //add the task to the time interval
            cyclicTasks [timeInterval].Add (new ICyclicTask (name, OnRun));
        }

        public static void AddTimeOfDayInterrupt (string name, Time time, RunHandler OnRun) {
            if (TimeOfDayInterruptExists (name))
                throw new Exception ("Task already exists");

            todTasks.Add (new ITodTask (name, time, OnRun));
        }

        public static bool CyclicInterruptExists (string name) {
            foreach (var taskList in cyclicTasks.Values) {
                foreach (var task in taskList) {
                    if (string.Equals (task.name, name, StringComparison.InvariantCultureIgnoreCase))
                        return true;
                }
            }

            return false;
        }

        public static bool TimeOfDayInterruptExists (string name) {
            foreach (var task in todTasks) {
                if (string.Equals (task.name, name, StringComparison.InvariantCultureIgnoreCase))
                    return true;
            }

            return false;
        }

        private class ICyclicTask
        {
            public RunHandler OnRun;
            public string name;

            public ICyclicTask (string name, RunHandler OnRun) {
                this.name = name;
                this.OnRun = OnRun;
            }
        }

        private class ITodTask : ICyclicTask
        {
            public Time time;

            public ITodTask (string name, Time time, RunHandler OnRun) : base (name, OnRun) {
                this.time = time;
            }
        }
    }
}

