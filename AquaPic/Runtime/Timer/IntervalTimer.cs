using System;
using System.Collections.Generic;

namespace AquaPic.Runtime
{
    public delegate void TimerHandler (object sender);

    public enum IntervalTimerState {
        Waiting,
        Running,
        Paused
    };

    public class IntervalTimer
    {
        protected static Dictionary<string, IntervalTimer> intervalTimers = new Dictionary<string, IntervalTimer> ();

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

        protected IntervalTimerState _state;
        public IntervalTimerState state {
            get {
                return _state;
            }
        }

        protected uint _secondsRemaining;
        public uint secondsRemaining {
            get {
                return _secondsRemaining;
            }
        }

        protected uint _totalSeconds;
        public uint totalSeconds {
            get {
                return _totalSeconds;
            }
            set {
                if (_state == IntervalTimerState.Waiting)
                    _totalSeconds = value;
            }
        }

        public event TimerElapsedHandler TimerElapsedEvent;
        public event TimerHandler TimerInterumEvent;
        public event TimerHandler TimerStartEvent;
        public event TimerHandler TimerStopEvent;

        protected IntervalTimer (string name, uint minutes, uint seconds) {
            this.name = name;
            SetTime (minutes, seconds);
            _secondsRemaining = _totalSeconds;
            _state = IntervalTimerState.Waiting;
        }

        public static IntervalTimer GetTimer (string name) {
            return GetTimer (name, 0, 0);
        }

        public static IntervalTimer GetTimer (string name, uint minutes, uint seconds) {
            if (intervalTimers.ContainsKey (name))
                return intervalTimers [name];

            IntervalTimer intervalTimer = new IntervalTimer (name, minutes, seconds);
            intervalTimers.Add (name, intervalTimer);
            return intervalTimer;
        }

        public void Start () {
            if (_state == IntervalTimerState.Waiting) {
                if (_totalSeconds > 0) {
                    _secondsRemaining = _totalSeconds;
                    _enabled = true;
                    _state = IntervalTimerState.Running;
                    timerId = GLib.Timeout.Add (1000, OnTimeout);
                    if (TimerStartEvent != null)
                        TimerStartEvent (this);
                }
            } else if (_state == IntervalTimerState.Paused) {
                if (_secondsRemaining > 0) {
                    _enabled = true;
                    _state = IntervalTimerState.Running;
                    timerId = GLib.Timeout.Add (1000, OnTimeout);
                    if (TimerStartEvent != null)
                        TimerStartEvent (this);
                }
            }
        }

        public void Stop () {
            if (_state == IntervalTimerState.Running) {
                _enabled = false;
                _state = IntervalTimerState.Paused;
                GLib.Source.Remove (timerId);
                if (TimerStopEvent != null)
                    TimerStopEvent (this);
            }
        }

        public void Reset () {
            Stop ();
            _state = IntervalTimerState.Waiting;
        }

        public void SetTime (uint minutes, uint seconds) {
            if (_state == IntervalTimerState.Waiting) {
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
                    _state = IntervalTimerState.Waiting;

                    // We want any user code to execute first before the dialog screen is shown
                    if (TimerElapsedEvent != null)
                        TimerElapsedEvent (this, new TimerElapsedEventArgs ());
                }
            }

            return _enabled;
        }
    }
}

