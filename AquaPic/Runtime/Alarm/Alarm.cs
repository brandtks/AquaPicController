using System;
using System.Collections.Generic;
using AquaPic.CoilRuntime;

namespace AquaPic.AlarmRuntime
{
    public partial class Alarm
    {
        private static List<AlarmType> alarms = new List<AlarmType> ();

        static Alarm () {
            TaskManagerRuntime.TaskManager.AddTask ("Alarm", 1000, Run);
        }

        public static void Run () {

        }

        public static int Subscribe (string shortName, string longName, bool clearOnAck = false) {
            int index = alarms.Count;
            alarms.Add (new AlarmType (shortName, longName, clearOnAck));
            return index;
        }

        public static void Post (int index) {
            if (!alarms [index].alarming) {
                alarms [index].alarming = true;
                alarms [index].acknowledged = false;
                alarms [index].count = 1;
            } else
                ++alarms [index].count;
        }

        public static void Clear (int index) {
            alarms [index].alarming = false;
            alarms [index].count = 0;
        }

        public static void Acknowledge () {
            for (int i = 0; i < alarms.Count; ++i) {
                alarms [i].acknowledged = true;
                if (alarms [i].clearOnAck)
                    Clear (i);
            }
        }

        public static bool CheckAlarming (int index) {
            return alarms [index].alarming;
        }

        public static void AddPostHandler (int index, AlarmHandler handler) {
            alarms [index].onPost += handler;
        }

        public static void AddAcknowledgeHandler (int index, AlarmHandler handler) {
            alarms [index].onAcknowledge += handler;
        }

        public static void AddClearHandler (int index, AlarmHandler handler) {
            alarms [index].onClear += handler;
        }
    }
}

