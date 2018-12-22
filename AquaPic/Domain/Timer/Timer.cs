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
    public delegate void TimerElapsedHandler (object sender, TimerElapsedEventArgs args);

    public class TimerElapsedEventArgs : EventArgs
    {
        public DateTime signalTime;

        public TimerElapsedEventArgs () {
            signalTime = DateTime.Now;
        }
    }

    public partial class Timer
    {
        protected bool _enabled;
        public bool enabled {
            get {
                return _enabled;
            }
            set {
                _enabled = value;
                if (_enabled)
                    Start ();
                else
                    Stop ();
            }
        }
        protected uint timerId;

        public event TimerElapsedHandler TimerElapsedEvent;
        public uint timerInterval;
        public bool autoReset;

        public Timer () : this (0) { }

        public Timer (uint timerInterval) {
            _enabled = false;
            this.timerInterval = timerInterval;
            autoReset = true;
            timerId = 0;
        }

        public virtual void Start () {
            _enabled = true;
            timerId = GLib.Timeout.Add (timerInterval, OnTimeout);
        }

        public virtual void Stop () {
            _enabled = false;
            GLib.Source.Remove (timerId);
        }

        protected virtual bool OnTimeout () {
            if (_enabled) {
                TimerElapsedEvent?.Invoke (this, new TimerElapsedEventArgs ());
            }
            return _enabled & autoReset;
        }
    }
}

