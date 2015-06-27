using System;
using System.Collections.Generic;

namespace AquaPic.Runtime
{
    public partial class Alarm
    {
        private static List<AlarmType> alarms = new List<AlarmType> ();

        public static event AlarmHandler AlarmsUpdatedEvent;

        public static int totalAlarms {
            get {
                return alarms.Count;
            }
        }

        public static int Subscribe (string name, bool audible = false, bool clearOnAck = false) {
            if (!ListContains (name)) {
                AlarmType a = new AlarmType (name, audible, clearOnAck);
                alarms.Add (a);
                return alarms.IndexOf (a);
            } 

            return ListIndexOf (name);
        }

        public static void Post (int index) {
            if ((index >= 0) && (index <= (alarms.Count - 1))) {
                if (!alarms [index].alarming) {
                    alarms [index].PostAlarm ();

                    EventLogger.Add (string.Format ("{0} posted", alarms [index].name));

                    if (AlarmsUpdatedEvent != null)
                        AlarmsUpdatedEvent (null);
                }
            }
        }

        public static void Clear (int index) {
            if ((index >= 0) && (index <= (alarms.Count - 1))) {
                alarms [index].ClearAlarm ();

                EventLogger.Add (string.Format ("{0} cleared", alarms [index].name));

                if (AlarmsUpdatedEvent != null)
                    AlarmsUpdatedEvent (null);
            }
        }

        public static void Acknowledge () {
            foreach (var alarm in alarms)
                alarm.AcknowledgeAlarm ();

            EventLogger.Add ("All alarms acknowledged");

            if (AlarmsUpdatedEvent != null)
                AlarmsUpdatedEvent (null);
        }

        public static bool CheckAlarming (int index) {
            if ((index >= 0) && (index <= (alarms.Count - 1)))
                return alarms [index].alarming;
            return false;
        }

        public static bool CheckAlarming (string name) {
            int index = ListIndexOf (name);
            return CheckAlarming (index);
        }

        public static bool CheckAcknowledged (int index) {
            if ((index >= 0) && (index <= (alarms.Count - 1)))
                return alarms [index].acknowledged;
            return false;
        }

        public static bool CheckAcknowledged (string name) {
            int index = ListIndexOf (name);
            return CheckAcknowledged (index);
        }

        public static void AddPostHandler (int index, AlarmHandler handler) {
            if ((index >= 0) && (index <= (alarms.Count - 1)))
                alarms [index].postEvent += handler;
        }

        public static void AddAcknowledgeHandler (int index, AlarmHandler handler) {
            if ((index >= 0) && (index <= (alarms.Count - 1)))
                alarms [index].acknowledgeEvent += handler;
        }

        public static void AddClearHandler (int index, AlarmHandler handler) {
            if ((index >= 0) && (index <= (alarms.Count - 1)))
                alarms [index].clearEvent += handler;
        }

        public static bool ListContains (string name) {
            foreach (var a in alarms) {
                if (string.Equals (a.name, name, StringComparison.InvariantCultureIgnoreCase))
                    return true;
            }

            return false;
        }

        public static int ListIndexOf (string name) {
            for (int i = 0; i < alarms.Count; ++i) {
                if (string.Equals (alarms [i].name, name, StringComparison.InvariantCultureIgnoreCase))
                    return i;
            }

            return -1;
        }

        public static int AlarmCount () {
            int alarming = 0;
            foreach (var a in alarms) {
                if (a.alarming)
                    ++alarming;
            }
            return alarming;
        }

        public static List<AlarmData> GetAllAlarming () {
            List<AlarmData> alarming = new List<AlarmData> ();
            foreach (var a in alarms) {
                if ((!a.acknowledged) || (a.alarming && a.acknowledged))
                    alarming.Add (a);
            }
            return alarming;
        }

        public static List<AlarmData> GetAllNotAcknowledged () {
            List<AlarmData> notAck = new List<AlarmData> ();
            foreach (var a in alarms) {
                if (!a.acknowledged)
                    notAck.Add (a);
            }
            return notAck;
        }
    }
}

