using System;
using Gtk;

namespace AquaPic.TimerRuntime
{
    public class OffDelayTimer : Timer
    {
        protected bool timerFinished;

        public OffDelayTimer (uint timeDelay) : base (timeDelay) {
            autoReset = false;
            timerFinished = true;
            TimerElapsedEvent += OnTimerElapsed;
        }

        public bool Evaluate (bool enable) {
            if (enable) {
                if (!_enabled)
                    Start ();
            } else {
                if (timerFinished)
                    timerFinished = true;
                if (_enabled)
                    Stop ();
            }

            return timerFinished;
        }

        protected void OnTimerElapsed (object sender, TimerElapsedEventArgs args) {
            timerFinished = false;
        }
    }
}

