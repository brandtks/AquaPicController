using System;
using System.Collections.Generic;

namespace AquaPic.Runtime
{
    public delegate void TimerInterumHandler (object sender);

    public class DeluxeTimer : Timer
    {
        private static Dictionary<string, DeluxeTimer> deluxeTimers = new Dictionary<string, DeluxeTimer> ();

        public string name;

        public uint secondsRemaining;
        public uint totalSeconds;

        public TimerInterumHandler TimerInterumEvent;

        private DeluxeTimer (string name, uint minutes, uint seconds) : base (1000) {
            this.name = name;
            SetTime (minutes, seconds);
            secondsRemaining = totalSeconds;
        }

        public static DeluxeTimer GetTimer (string name) {
            return GetTimer (name, 0, 0);
        }

        public static DeluxeTimer GetTimer (string name, uint minutes, uint seconds) {
            if (deluxeTimers.ContainsKey (name))
                return deluxeTimers [name];

            DeluxeTimer dt = new DeluxeTimer (name, minutes, seconds);
            deluxeTimers.Add (name, dt);
            return dt;
        }

        public override void Start () {
            secondsRemaining = totalSeconds;
            _enabled = true;
            timerId = GLib.Timeout.Add (1000, OnTimer);
        }

        public override void Stop () {
            _enabled = false;
            GLib.Source.Remove (timerId);
        }

        public void SetTime (uint minutes, uint seconds) {
            totalSeconds = minutes * 60 + seconds;
        }

        protected bool OnTimer () {
            if (_enabled) {
                --secondsRemaining;

                if (TimerInterumEvent != null)
                    TimerInterumEvent (this);

                if (secondsRemaining <= 0) {
                    if (TimerElapsedEvent != null)
                        TimerElapsedEvent (this, new TimerElapsedEventArgs ());
                    Stop ();
                }
            }
            return _enabled;
        }
    }
}

