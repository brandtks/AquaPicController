using System;

namespace AquaPic.Runtime
{
    public enum AlarmEventType {
        Posted,
        Acknowledged,
        Cleared
    }

    public class AlarmEventArgs : EventArgs {
        public AlarmEventType type;
        public string name;

        public AlarmEventArgs (AlarmEventType type, string name) {
            this.type = type;
            this.name = name;
        }
    }

    public delegate void AlarmEventHandler (object sender, AlarmEventArgs args);
}

