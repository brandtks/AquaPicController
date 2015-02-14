using System;
using System.Collections.Generic;

namespace AquaPic.Alarm
{
    public static class alarm
    {
        private static List<alarmType> alarms;

        static alarm () {
            alarms = new List<alarmType> ();
        }

        public static void run () {

        }

        public static int subscribe (string shortName, string longName, bool clearOnAck = false) {
            alarms.Add (new alarmType (shortName, longName, clearOnAck));
            return alarms.Count - 1;
        }

        public static void post (int index, bool clearOnAck = false) {
            if (!alarms [index].alarming) {
                alarms [index].alarming = true;
                alarms [index].acknowledged = false;
                alarms [index].count = 1;
            } else
                ++alarms [index].count;
        }

        public static void clear (int index) {
            alarms [index].alarming = false;
            alarms [index].count = 0;
        }

        public static void acknowledge () {
            for (int i = 0; i < alarms.Count; ++i) {
                alarms [i].acknowledged = true;
                if (alarms [i].clearOnAck)
                    clear (i);
            }
        }

        public static bool checkAlarming (int index) {
            return alarms [index].alarming;
        }

        public static void addPostHandler (int index, alarmHandler handler) {
            alarms [index].onPost += handler;
        }

        public static void addAcknowledgeHandler (int index, alarmHandler handler) {
            alarms [index].onAcknowledge += handler;
        }

        public static void addClearHandler (int index, alarmHandler handler) {
            alarms [index].onClear += handler;
        }
    }
}

