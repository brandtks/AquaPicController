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
                    AlarmEvent?.Invoke (this, new AlarmEventArgs (AlarmEventType.Posted, _name));
                }
            }

            public void AcknowledgeAlarm () {
                _acknowledged = true;
                AlarmEvent?.Invoke (this, new AlarmEventArgs (AlarmEventType.Acknowledged, _name));
                if (_clearOnAck || !_alarming) {
                    ClearAlarm ();
                }
            }

            public void ClearAlarm () {
                _alarming = false;
                if (_acknowledged) {
                    AlarmEvent?.Invoke (this, new AlarmEventArgs (AlarmEventType.Cleared, _name));
                }
            }
        }
    }
}

) {
                    AlarmEvent?.Invoke (this, new AlarmEventArgs (AlarmEventType.Cleared, _name));
                }
            }
        }
    }
}

