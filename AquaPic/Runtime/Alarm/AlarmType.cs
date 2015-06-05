using System;

namespace AquaPic.Runtime
{
    public partial class Alarm
    {
        private class AlarmType
        {
            public string shortName;
            public string longName;
            public uint count;
            public bool alarming;
            public bool acknowledged;
            public bool clearOnAck;
            public event AlarmHandler onPost;
            public event AlarmHandler onAcknowledge;
            public event AlarmHandler onClear;

            public AlarmType (string shortName, string longName, bool clearOnAck) {
                this.shortName = shortName;
                this.longName = longName;
                this.count = 0;
                this.alarming = false;
                this.acknowledged = false;
                this.clearOnAck = clearOnAck;
            }

            public void OnAlarmPost () {
                if (onPost != null)
                    onPost (this);
            }

            public void OnAlarmAcknowledge () {
                if (onAcknowledge != null)
                    onAcknowledge (this);
            }

            public void OnAlarmClear () {
                if (onClear != null)
                    onClear (this);
            }
        }
    }
}

