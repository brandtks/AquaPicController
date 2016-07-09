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
                if (TimerElapsedEvent != null)
                    TimerElapsedEvent (this, new TimerElapsedEventArgs ());
            }
            return _enabled & autoReset;
        }
    }
}

