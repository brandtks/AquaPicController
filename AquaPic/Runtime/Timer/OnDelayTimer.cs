using System;
using Gtk;

namespace AquaPic.Runtime
{
    public class OnDelayTimer : Timer
    {
        protected bool timerFinished;

        public OnDelayTimer (uint timeDelay) : base (timeDelay) {
            autoReset = false;
            timerFinished = false;
            TimerElapsedEvent += OnTimerElapsed;
        }

        public bool Evaluate (bool enable) {
            if (enable) {
                if (!_enabled)
                    Start ();
            } else {
                if (timerFinished)
                    timerFinished = false;
                if (_enabled)
                    Stop ();
            }

            return timerFinished;
        }

        protected void OnTimerElapsed (object sender, TimerElapsedEventArgs args) {
            timerFinished = true;
        }
    }
}

