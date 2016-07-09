using System;
using System.Collections.Generic;
using TouchWidgetLibrary;

namespace AquaPic.Runtime
{
    public delegate void TimerHandler (object sender);

    public enum DeluxeTimerState {
        Waiting,
        Running,
        Paused
    };

    public class DeluxeTimer
    {
        private static Dictionary<string, DeluxeTimer> deluxeTimers = new Dictionary<string, DeluxeTimer> ();

        public string name;

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

        private DeluxeTimerState _state;
        public DeluxeTimerState state {
            get {
                return _state;
            }
        }

        private uint _secondsRemaining;
        public uint secondsRemaining {
            get {
                return _secondsRemaining;
            }
        }

        private uint _totalSeconds;
        public uint totalSeconds {
            get {
                return _totalSeconds;
            }
            set {
                if (_state == DeluxeTimerState.Waiting)
                    _totalSeconds = value;
            }
        }

        public event TimerElapsedHandler TimerElapsedEvent;
        public event TimerHandler TimerInterumEvent;
        public event TimerHandler TimerStartEvent;
        public event TimerHandler TimerStopEvent;

        private DeluxeTimer (string name, uint minutes, uint seconds) {
            this.name = name;
            SetTime (minutes, seconds);
            _secondsRemaining = _totalSeconds;
            _state = DeluxeTimerState.Waiting;
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

        public void Start () {
            if (_state == DeluxeTimerState.Waiting) {
                if (_totalSeconds > 0) {
                    _secondsRemaining = _totalSeconds;
                    _enabled = true;
                    _state = DeluxeTimerState.Running;
                    timerId = GLib.Timeout.Add (1000, OnTimeout);
                    if (TimerStartEvent != null)
                        TimerStartEvent (this);
                }
            } else if (_state == DeluxeTimerState.Paused) {
                if (_secondsRemaining > 0) {
                    _enabled = true;
                    _state = DeluxeTimerState.Running;
                    timerId = GLib.Timeout.Add (1000, OnTimeout);
                    if (TimerStartEvent != null)
                        TimerStartEvent (this);
                }
            }
        }

        public void Stop () {
            if (_state == DeluxeTimerState.Running) {
                _enabled = false;
                _state = DeluxeTimerState.Paused;
                GLib.Source.Remove (timerId);
                if (TimerStopEvent != null)
                    TimerStopEvent (this);
            }
        }

        public void Reset () {
            Stop ();
            _state = DeluxeTimerState.Waiting;
        }

        public void SetTime (uint minutes, uint seconds) {
            if (_state == DeluxeTimerState.Waiting) {
                _totalSeconds = minutes * 60 + seconds;
            }
        }

        protected bool OnTimeout () {
            if (_enabled) {
                --_secondsRemaining;

                if (TimerInterumEvent != null)
                    TimerInterumEvent (this);

                if (_secondsRemaining <= 0) {
                    _enabled = false;
                    _secondsRemaining = _totalSeconds;
                    _state = DeluxeTimerState.Waiting;

                    // We want any user code to execute first before the dialog screen is shown
                    if (TimerElapsedEvent != null)
                        TimerElapsedEvent (this, new TimerElapsedEventArgs ());
                }
            }

            return _enabled;
        }
    }
}

