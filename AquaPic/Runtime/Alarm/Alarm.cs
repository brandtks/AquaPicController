using System;
using System.Collections.Generic;

namespace AquaPic.Runtime
{
    public partial class Alarm
    {
        private static List<AlarmType> alarms = new List<AlarmType> ();

        static Alarm () {
            TaskManager.AddTask ("Alarm", 1000, Run);
        }

        public static void Run () {

        }

        public static int Subscribe (string name, string description, bool audible = false, bool clearOnAck = false) {
            AlarmType a = new AlarmType (name, description, audible, clearOnAck);
            alarms.Add (a);
            return alarms.IndexOf (a);
        }

        public static void Post (int index) {
            alarms [index].PostAlarm ();
        }

        public static void Clear (int index) {
            alarms [index].ClearAlarm ();
        }

        public static void Acknowledge () {
            foreach (var alarm in alarms)
                alarm.AcknowledgeAlarm ();
        }

        public static bool CheckAlarming (int index) {
            return alarms [index].alarming;
        }

        public static bool CheckAcknowledged (int index) {
            return alarms [index].acknowledged;
        }

        public static void AddPostHandler (int index, AlarmHandler handler) {
            alarms [index].postEvent += handler;
        }

        public static void AddAcknowledgeHandler (int index, AlarmHandler handler) {
            alarms [index].acknowledgeEvent += handler;
        }

        public static void AddClearHandler (int index, AlarmHandler handler) {
            alarms [index].clearEvent += handler;
        }
    }
}

