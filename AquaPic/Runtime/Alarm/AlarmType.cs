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
            //public event AlarmHandler postEvent;
            //public event AlarmHandler acknowledgeEvent;
            //public event AlarmHandler clearEvent;

            public event AlarmEventHandler AlarmEvent;

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

                    if (AlarmEvent != null)
                        AlarmEvent (this, new AlarmEventArgs (AlarmEventType.Posted, _name));
                }
            }

            public void AcknowledgeAlarm () {
                _acknowledged = true;

                if (AlarmEvent != null)
                    AlarmEvent (this, new AlarmEventArgs (AlarmEventType.Acknowledged, _name));
                
                if (_clearOnAck || !_alarming)
                    ClearAlarm ();
            }

            public void ClearAlarm () {
                _alarming = false;

                if (_acknowledged) {
                    if (AlarmEvent != null)
                        AlarmEvent (this, new AlarmEventArgs (AlarmEventType.Cleared, _name));
                }
            }
        }
    }
}

