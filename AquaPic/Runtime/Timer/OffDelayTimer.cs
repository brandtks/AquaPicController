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

