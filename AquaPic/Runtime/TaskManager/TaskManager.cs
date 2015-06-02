using System;
using System.Collections.Generic;
using Gtk;
using AquaPic.AlarmRuntime;
using AquaPic.AnalogInputDriver;
using AquaPic.AnalogOutputDriver;
using AquaPic.DigitalInputDriver;
using AquaPic.LightingModule;
using AquaPic.PluginRuntime;
using AquaPic.PowerDriver;
using AquaPic.TemperatureModule;

namespace AquaPic.TaskManagerRuntime
{
    public delegate void RunHandler ();

    public class TaskManager
    {
        private static Dictionary<int, List<ITask>> tasks = new Dictionary<int, List<ITask>> ();

        public static void Start () {
            foreach (var timeInterval in tasks.Keys) {
                #if SIMULATION
                int time = timeInterval * 4;
                GLib.Timeout.Add (time, () => {
                    foreach (var task in tasks[timeInterval])
                        task.OnRun ();
                    return true;
                });
                #else
                GLib.Timeout.Add ((uint)timeInterval, () => {
                    foreach (var task in tasks[timeInterval])
                        task.OnRun ();
                    return true;
                });
                #endif
            }
        }

        public static void AddTask (string name, int timeInterval, RunHandler OnRun) {
            if (TaskExists (name))
                throw new Exception ("Task already exists");

            if (!tasks.ContainsKey (timeInterval))
                tasks.Add (timeInterval, new List<ITask> ());

            tasks [timeInterval].Add (new ITask (name, OnRun));
        }

        public static bool TaskExists (string name) {
            foreach (var taskList in tasks.Values) {
                foreach (var task in taskList) {
                    if (string.Compare (task.name, name, StringComparison.InvariantCultureIgnoreCase) == 0)
                        return true;
                }
            }

            return false;
        }

        private class ITask
        {
            public RunHandler OnRun;
            public string name;

            public ITask (string name, RunHandler OnRun) {
                this.name = name;
                this.OnRun = OnRun;
            }
        }
    }
}

