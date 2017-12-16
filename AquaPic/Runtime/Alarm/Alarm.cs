#region License

/*
    AquaPic Main Control - Handles all functionality for the AquaPic aquarium controller.

    Copyright (c) 2017 Goodtime Development

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see http://www.gnu.org/licenses/
*/

#endregion // License

using System;
using System.Collections.Generic;

namespace AquaPic.Runtime
{
    public partial class Alarm
    {
        private static List<AlarmType> alarms = new List<AlarmType> ();

        public static event AlarmEventHandler AlarmsUpdatedEvent;

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
                if (!alarms[index].alarming) {
                    alarms[index].PostAlarm ();
                    Logger.AddError (string.Format ("{0} posted", alarms [index].name));
                    AlarmsUpdatedEvent?.Invoke (null, new AlarmEventArgs (AlarmEventType.Posted, alarms[index].name));
                }
            }
        }

        public static void Clear (int index) {
            if ((index >= 0) && (index <= (alarms.Count - 1))) {
                if (alarms[index].alarming) {
                    alarms[index].ClearAlarm ();
                    Logger.AddInfo (string.Format ("{0} cleared", alarms[index].name));
                    AlarmsUpdatedEvent?.Invoke (null, new AlarmEventArgs (AlarmEventType.Cleared, alarms[index].name));
                }
            }
        }

        public static void Acknowledge () {
            foreach (var alarm in alarms) {
                alarm.AcknowledgeAlarm ();
            }
            Logger.Add ("All alarms acknowledged");
            AlarmsUpdatedEvent?.Invoke (null, new AlarmEventArgs (AlarmEventType.Acknowledged, "All alarms"));
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

        public static void AddAlarmHandler (int index, AlarmEventHandler handler) {
            if ((index < 0) || (index >= alarms.Count))
                throw new ArgumentException ("index");
            
            alarms [index].AlarmEvent += handler;
        }

        public static void RemoveAlarmHandler (int index, AlarmEventHandler handler) {
            if ((index < 0) || (index >= alarms.Count))
                throw new ArgumentException ("index");

            alarms [index].AlarmEvent -= handler;
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

 (var a in alarms) {
                if (!a.acknowledged)
                    notAck.Add (a);
            }
            return notAck;
        }
    }
}

