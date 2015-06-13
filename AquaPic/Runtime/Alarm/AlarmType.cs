using System;

namespace AquaPic.Runtime
{
    public class AlarmData
    {
        protected bool _alarming;
        protected bool _acknowledged;
        protected string _name;
        protected bool _clearOnAck;
        protected bool _audible;
        protected DateTime _postTime;

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

        public string name {
            get {
                return _name;
            }
        }
        
        public bool clearOnAck {
            get {
                return _clearOnAck;
            }
        }

        public bool audible {
            get {
                return _audible;
            }
        }

        public DateTime postTime {
            get {
                return _postTime;
            }
        }
    }

    public partial class Alarm
    {
        private class AlarmType : AlarmData
        {
            public event AlarmHandler postEvent;
            public event AlarmHandler acknowledgeEvent;
            public event AlarmHandler clearEvent;

            public AlarmType (string name, bool audible, bool clearOnAck) {
                _alarming = false;
                _acknowledged = true;
                _name = name;
                _clearOnAck = clearOnAck;
                _audible = audible;
            }

            public void PostAlarm () {
                if (!_alarming) {
                    _alarming = true;
                    _acknowledged = false;
                    _postTime = DateTime.Now;

                    if (postEvent != null)
                        postEvent (this);
                }
            }

            public void AcknowledgeAlarm () {
                _acknowledged = true;

                if (acknowledgeEvent != null)
                    acknowledgeEvent (this);
                
                if (_clearOnAck || !_alarming)
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

