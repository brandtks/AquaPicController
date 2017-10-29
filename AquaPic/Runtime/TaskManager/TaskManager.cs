#region License

/*
    AquaPic Main Control - Handles all functionality for the AquaPic aquarium controller.

    Copyright (c) 2017 Skyler Brandt

    This program is free software; you can redistribute it and/or
    modify it under the terms of the GNU General Public License
    as published by the Free Software Foundation; either version 2
    of the License, or (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program; if not, write to the Free Software
    Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.

    Optionally you can also view the license at <http://www.gnu.org/licenses/>.
*/

#endregion // License

﻿using System;
using GoodtimeDevelopment.Utilites;
using System.Collections.Generic;

namespace AquaPic.Runtime
{
    public delegate void RunHandler();

    public class TaskManager
    {
        protected static Dictionary<uint, CyclicTaskLocker> cyclicTasks = new Dictionary<uint, CyclicTaskLocker>();
        protected static List<TimeOfDayTask> todTasks = new List<TimeOfDayTask>();

        static TaskManager() {
            AddCyclicInterrupt("TimeOfDayInterrupts", 5000, () => {
                DateTime now = DateTime.Now;

                if ((now.Second >= 0) && (now.Second <= 4)) {
                    foreach (var task in todTasks) {
                        if (task.time.EqualsShortTime(now))
                            task.OnRun();
                    }
                }
            });
        }

        public static void AddCyclicInterrupt(string name, uint timeInterval, RunHandler OnRun) {
            if (CyclicInterruptExists(name))
                throw new Exception("Task already exists");

            if (!cyclicTasks.ContainsKey(timeInterval)) { // this time interval doesn't exist, add it
                CyclicTaskLocker locker = new CyclicTaskLocker();
                cyclicTasks.Add(timeInterval, locker);
                locker.timerId = GLib.Timeout.Add(timeInterval, locker.OnTaskTimeout);
            }

            //add the task to the time interval
            cyclicTasks[timeInterval].tasks.Add(new CyclicTask(name, OnRun));
        }

        public static void RemoveCyclicInterrupt(string name) {
            uint runtime;
            try {
                runtime = GetCyclicRuntime(name);
            } catch (ArgumentException) {
                throw new Exception("Task doesn't exist");
            }

            var task = GetCyclicTask(name, runtime);
            cyclicTasks[runtime].tasks.Remove(task);

            if (cyclicTasks[runtime].tasks.Count == 0) {
                GLib.Source.Remove(cyclicTasks[runtime].timerId);
                cyclicTasks.Remove(runtime);
            }
        }

        public static void AddTimeOfDayInterrupt(string name, Time time, RunHandler OnRun) {
            if (TimeOfDayInterruptExists(name))
                throw new Exception("Task already exists");

            todTasks.Add(new TimeOfDayTask(name, time, OnRun));
        }

        public static bool CyclicInterruptExists(string name) {
            try {
                GetCyclicRuntime(name);
                return true;
            } catch (ArgumentException) {
                return false;
            }
        }

        protected static uint GetCyclicRuntime(string name) {
            foreach (var runtime in cyclicTasks.Keys) {
                foreach (var task in cyclicTasks[runtime].tasks) {
                    if (string.Equals(task.name, name, StringComparison.InvariantCultureIgnoreCase))
                        return runtime;
                }
            }

            throw new ArgumentException(name + " doesn't exist");
        }

        protected static CyclicTask GetCyclicTask(string name, uint runtime) {
            foreach (var task in cyclicTasks[runtime].tasks) {
                if (string.Equals(task.name, name, StringComparison.InvariantCultureIgnoreCase)) {
                    return task;
                }
            }
            return null;
        }

        public static bool TimeOfDayInterruptExists(string name) {
            foreach (var task in todTasks) {
                if (string.Equals(task.name, name, StringComparison.InvariantCultureIgnoreCase))
                    return true;
            }

            return false;
        }

        protected class CyclicTask
        {
            public RunHandler OnRun;
            public string name;

            public CyclicTask(string name, RunHandler OnRun) {
                this.name = name;
                this.OnRun = OnRun;
            }
        }

        protected class TimeOfDayTask : CyclicTask
        {
            public Time time;

            public TimeOfDayTask(string name, Time time, RunHandler OnRun) : base(name, OnRun) {
                this.time = time;
            }
        }

        protected class CyclicTaskLocker
        {
            public List<CyclicTask> tasks;
            public uint timerId;

            public CyclicTaskLocker() {
                tasks = new List<CyclicTask>();
            }

            public bool OnTaskTimeout() {
                for (int i = 0; i < tasks.Count; ++i) {
                    try {
                        tasks[i].OnRun();
                    } catch (Exception ex) {
                        Logger.AddError("{0} throw an exception", tasks[i].name);
                        Logger.AddError(ex.ToString());
                    }
                }

                return true;
            }
        }
    }
}

