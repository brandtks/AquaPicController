using System;
using System.Collections.Generic;
using MyWidgetLibrary;

namespace AquaPic.Runtime
{
    public delegate void TimerHandler (object sender);

    public class DeluxeTimer : Timer
    {
        private static Dictionary<string, DeluxeTimer> deluxeTimers = new Dictionary<string, DeluxeTimer> ();

        public string name;

        public uint secondsRemaining;
        public uint totalSeconds;

        public TimerHandler TimerInterumEvent;
        public TimerHandler TimerStartEvent;
        public TimerHandler TimerStopEvent;

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
            if (totalSeconds > 0) {
                secondsRemaining = totalSeconds;
                _enabled = true;
                timerId = GLib.Timeout.Add (1000, OnTimer);
                if (TimerStartEvent != null)
                    TimerStartEvent (this);
            }
        }

        public override void Stop () {
            _enabled = false;
            GLib.Source.Remove (timerId);
            if (TimerStopEvent != null)
                TimerStopEvent (this);
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
                    _enabled = false;
                    secondsRemaining = totalSeconds;

                    // We want any user code to execute first before the dialog screen is shown
                    if (TimerElapsedEvent != null)
                        TimerElapsedEvent (this, new TimerElapsedEventArgs ());

                    Console.WriteLine ("{0:T}: Calling Dialog to indicate {1} elapsed", DateTime.Now, name); 
                    MessageBox.Show (string.Format ("{0}", name));
                }
            }
            return _enabled;
        }
    }
}

