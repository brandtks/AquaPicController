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

namespace AquaPic.Service
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
                if (!_enabled || timerFinished)
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

