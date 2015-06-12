using System;
using System.Media;

namespace AquaPic.Runtime
{
    public partial class Alarm
    {
        private class AlarmType
        {
            private bool _alarming;
            private bool _acknowledged;
            public bool alarming {
                get {
                    return _alarming;
                }
            }
            public bool acknowledged {
                get {
                    return _acknowledged;
                }
            }

            public string name;
            public string description;
            public bool clearOnAck;
            public bool audible;
            public event AlarmHandler postEvent;
            public event AlarmHandler acknowledgeEvent;
            public event AlarmHandler clearEvent;

            public AlarmType (string name, string description, bool audible, bool clearOnAck) {
                _alarming = false;
                _acknowledged = false;

                this.name = name;
                this.description = description;
                this.clearOnAck = clearOnAck;
                this.audible = audible;
            }

            public void PostAlarm () {
                if (!_alarming) {
                    _alarming = true;
                    _acknowledged = false;

                    if (audible)
                        SystemSounds.Beep.Play ();

                    if (postEvent != null)
                        postEvent (this);
                }
            }

            public void AcknowledgeAlarm () {
                _acknowledged = true;

                if (acknowledgeEvent != null)
                    acknowledgeEvent (this);
                
                if (clearOnAck)
                    ClearAlarm ();
            }

            public void ClearAlarm () {
                _alarming = false;

                if (_acknowledged) {
                    if (clearEvent != null)
                        clearEvent (this);
                }
            }
        }
    }
}

