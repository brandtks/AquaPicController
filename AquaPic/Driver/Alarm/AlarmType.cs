using System;

namespace AquaPic.Alarm
{
    public class alarmType
    {
        public string shortName;
        public string longName;
        public uint count;
        public bool alarming;
        public bool acknowledged;
        public bool clearOnAck;
        public event alarmHandler onPost;
        public event alarmHandler onAcknowledge;
        public event alarmHandler onClear;

        public alarmType (string shortName, string longName, bool clearOnAck) {
            this.shortName = shortName;
            this.longName = longName;
            this.count = 0;
            this.alarming = false;
            this.acknowledged = false;
            this.clearOnAck = clearOnAck;
        }

        public void onAlarmPost () {
            if (onPost != null)
                onPost (this);
        }

        public void onAlarmAcknowledge () {
            if (onAcknowledge != null)
                onAcknowledge (this);
        }

        public void onAlarmClear () {
            if (onClear != null)
                onClear (this);
        }
    }
}

