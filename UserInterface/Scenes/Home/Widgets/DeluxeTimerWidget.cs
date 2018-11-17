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
using Gtk;
using Cairo;
using GoodtimeDevelopment.TouchWidget;
using AquaPic.Runtime;

namespace AquaPic.UserInterface
{
    public class DeluxeTimerWidget : Fixed
    {
        IntervalTimer[] timers;
        TimerTab[] tabs;
        int timerIndex;
        TouchTextBox minutes;
        TouchTextBox seconds;
        TouchUpDownButtons minUpDown;
        TouchUpDownButtons secUpDown;
        TouchButton startStopButton;
        TouchButton resetButton;

        public DeluxeTimerWidget (string name) {
            timerIndex = 0;

            SetSizeRequest (310, 169);

            timers = new IntervalTimer[3];
            for (int i = 0; i < timers.Length; ++i) {
                timers[i] = IntervalTimer.GetTimer ("Timer " + name + " " + (i + 1).ToString ());
                timers[i].TimerInterumEvent += OnTimerInterum;
                timers[i].TimerElapsedEvent += OnTimerElapsed;
                timers[i].TimerStartEvent += OnTimerStartStop;
                timers[i].TimerStopEvent += OnTimerStartStop;
            }

            var box2 = new TimerBackground (310, 139);
            box2.color = "grey4";
            box2.transparency = 0.1f;
            Put (box2, 0, 30);

            tabs = new TimerTab[3];
            for (int i = 2; i >= 0; --i) {
                tabs[i] = new TimerTab (i);
                tabs[i].text = "Timer " + (i + 1).ToString ();
                tabs[i].ButtonReleaseEvent += OnTabButtonRelease;
                Put (tabs[i], 90 * i, 0);
                tabs[i].Show ();
            }

            tabs[0].selected = true;

            var minuteLabel = new TouchLabel ();
            minuteLabel.text = "Minutes";
            minuteLabel.textColor = "grey3";
            Put (minuteLabel, 5, 37);
            minuteLabel.Show ();

            minutes = new TouchTextBox ();
            minutes.SetSizeRequest (99, 51);
            minutes.enableTouch = true;
            minutes.textSize = 16;
            minutes.textAlignment = TouchAlignment.Center;
            minutes.TextChangedEvent += (sender, args) => {
                try {
                    uint time = Convert.ToUInt32 (args.text) * 60;
                    time += Convert.ToUInt32 (seconds.text);
                    UpdateTime (time);
                } catch {
                    args.keepText = false;
                }
            };
            Put (minutes, 3, 57);

            minUpDown = new TouchUpDownButtons ();
            minUpDown.up.ButtonReleaseEvent += (o, args) => {
                if (!timers[timerIndex].enabled) {
                    uint time = (Convert.ToUInt32 (minutes.text) + 1) * 60;
                    time += Convert.ToUInt32 (seconds.text);
                    UpdateTime (time);
                }
            };
            minUpDown.down.ButtonReleaseEvent += (o, args) => {
                if (!timers[timerIndex].enabled) {
                    if (minutes.text != "0") {
                        uint time = (Convert.ToUInt32 (minutes.text) - 1) * 60;
                        time += Convert.ToUInt32 (seconds.text);
                        UpdateTime (time);
                    }
                }
            };
            Put (minUpDown, 3, 113);
            minUpDown.Show ();

            var secondsLabel = new TouchLabel ();
            secondsLabel.text = "Seconds";
            secondsLabel.textColor = "grey3";
            Put (secondsLabel, 108, 37);
            secondsLabel.Show ();

            seconds = new TouchTextBox ();
            seconds.SetSizeRequest (98, 51);
            seconds.enableTouch = true;
            seconds.textAlignment = TouchAlignment.Center;
            seconds.textSize = 16;
            seconds.TextChangedEvent += (sender, args) => {
                try {
                    uint time = Convert.ToUInt32 (args.text);

                    if (time < 60)
                        time += Convert.ToUInt32 (minutes.text) * 60;

                    UpdateTime (time);
                } catch {
                    args.keepText = false;
                }
            };
            Put (seconds, 106, 57);

            secUpDown = new TouchUpDownButtons ();
            secUpDown.up.ButtonReleaseEvent += (o, args) => {
                if (!timers[timerIndex].enabled) {
                    uint time = Convert.ToUInt32 (minutes.text) * 60;
                    time += Convert.ToUInt32 (seconds.text);
                    ++time;
                    UpdateTime (time);
                }
            };
            secUpDown.down.ButtonReleaseEvent += (o, args) => {
                if (!timers[timerIndex].enabled) {
                    uint time = Convert.ToUInt32 (minutes.text) * 60;
                    time += Convert.ToUInt32 (seconds.text);
                    if (time != 0) {
                        --time;
                        UpdateTime (time);
                    }
                }
            };
            Put (secUpDown, 106, 113);
            secUpDown.Show ();

            startStopButton = new TouchButton ();
            startStopButton.SetSizeRequest (98, 51);
            startStopButton.ButtonReleaseEvent += OnStartStopButtonRelease;
            Put (startStopButton, 209, 57);

            resetButton = new TouchButton ();
            resetButton.SetSizeRequest (98, 51);
            resetButton.text = "Reset";
            resetButton.ButtonReleaseEvent += OnResetButtonRelease;
            Put (resetButton, 209, 113);

            if (timers[timerIndex].enabled)
                UpdateTime (timers[timerIndex].secondsRemaining, false);
            else
                UpdateTime (timers[timerIndex].totalSeconds, false);
        }

        public override void Dispose () {
            foreach (var timer in timers) {
                timer.TimerElapsedEvent -= OnTimerElapsed;
                timer.TimerStartEvent -= OnTimerStartStop;
                timer.TimerStopEvent -= OnTimerStartStop;
                timer.TimerInterumEvent -= OnTimerInterum;
            }

            base.Dispose ();
        }

        protected void OnTimerInterum (object sender) {
            IntervalTimer timer = sender as IntervalTimer;
            int tIdx = Convert.ToInt32 (timer.name[timer.name.Length - 1].ToString ()) - 1;
            if (timerIndex == tIdx)
                UpdateTime (timer.secondsRemaining, false);
        }

        protected void OnTimerElapsed (object sender, TimerElapsedEventArgs args) {
            IntervalTimer timer = sender as IntervalTimer;
            int tIdx = Convert.ToInt32 (timer.name[timer.name.Length - 1].ToString ()) - 1;
            if (timerIndex == tIdx)
                UpdateTime (timers[timerIndex].totalSeconds);

            MessageBox.Show (timer.name);
        }

        protected void OnTimerStartStop (object sender) {
            IntervalTimer timer = sender as IntervalTimer;
            int tIdx = Convert.ToInt32 (timer.name[timer.name.Length - 1].ToString ()) - 1;
            if (timerIndex == tIdx)
                UpdateTime (timers[timerIndex].secondsRemaining, false);
        }

        protected void OnStartStopButtonRelease (object sender, ButtonReleaseEventArgs args) {
            if (startStopButton.text == "Start") {
                timers[timerIndex].SetTime (Convert.ToUInt32 (minutes.text), Convert.ToUInt32 (seconds.text));
                timers[timerIndex].Start ();
            } else {
                timers[timerIndex].Stop ();
            }

            UpdateScreen ();
        }

        protected void OnResetButtonRelease (object sender, ButtonReleaseEventArgs args) {
            if (timers[timerIndex].state != IntervalTimerState.Waiting) {
                timers[timerIndex].Reset ();
            }

            if (timers[timerIndex].secondsRemaining != timers[timerIndex].totalSeconds)
                UpdateTime (timers[timerIndex].totalSeconds);

            UpdateScreen ();
        }

        protected void OnTabButtonRelease (object sender, ButtonReleaseEventArgs args) {
            TimerTab b = sender as TimerTab;
            timerIndex = b.position;

            if (timers[timerIndex].enabled)
                UpdateTime (timers[timerIndex].secondsRemaining, false);
            else
                UpdateTime (timers[timerIndex].totalSeconds, false);

            foreach (var tab in tabs) {

                tab.selected = false;
            }

            Remove (tabs[timerIndex]);
            tabs[timerIndex].Destroy ();
            tabs[timerIndex].Dispose ();
            tabs[timerIndex] = new TimerTab (timerIndex);
            tabs[timerIndex].text = "Timer " + (timerIndex + 1).ToString ();
            tabs[timerIndex].ButtonReleaseEvent += OnTabButtonRelease;
            Put (tabs[timerIndex], 90 * timerIndex, 0);
            tabs[timerIndex].Show ();
            tabs[timerIndex].selected = true;
        }

        protected void UpdateTime (uint time, bool changeTimerTime = true) {
            minutes.text = (time / 60).ToString ();
            seconds.text = (time % 60).ToString ();
            minutes.QueueDraw ();
            seconds.QueueDraw ();

            if (changeTimerTime) {
                timers[timerIndex].totalSeconds = time;
            }

            UpdateScreen ();
        }

        protected void UpdateScreen () {
            if (timers[timerIndex].state == IntervalTimerState.Waiting)
                resetButton.buttonColor = "grey1";
            else
                resetButton.buttonColor = "pri";

            if (timers[timerIndex].enabled) {
                startStopButton.text = "Stop";
                startStopButton.buttonColor = "pri";
            } else {
                startStopButton.text = "Start";
                startStopButton.buttonColor = "seca";

                if (timers[timerIndex].totalSeconds == 0)
                    secUpDown.down.buttonColor = "grey1";
                else
                    secUpDown.down.buttonColor = "pri";

                if (minutes.text == "0")
                    minUpDown.down.buttonColor = "grey1";
                else
                    minUpDown.down.buttonColor = "pri";

                secUpDown.down.QueueDraw ();
                minUpDown.down.QueueDraw ();
            }

            resetButton.QueueDraw ();
            startStopButton.QueueDraw ();
        }

        class TimerTab : EventBox
        {
            public TouchColor color;
            public string text;
            public bool selected;
            public int position;

            public TimerTab (int position) {
                SetSizeRequest (130, 30);

                VisibleWindow = false;
                ExposeEvent += OnEventBoxExpose;

                color = "grey3";
                text = string.Empty;
                selected = false;
                this.position = position;
            }

            protected void OnEventBoxExpose (object sender, ExposeEventArgs args) {
                EventBox eb = sender as EventBox;

                using (Context cr = Gdk.CairoHelper.Create (eb.GdkWindow)) {
                    int height = Allocation.Height;
                    int width = Allocation.Width;
                    int top = Allocation.Top;
                    int left = Allocation.Left;
                    int radius = 4;

                    if (!selected) {
                        width -= 25;
                        left += position * 13;
                        top += 7;
                        height -= 7;
                    }

                    cr.MoveTo (left, top + radius);
                    cr.Arc (left + radius, top + radius, radius, Math.PI, -Math.PI / 2);
                    cr.LineTo (left + width - radius, top);
                    cr.Arc (left + width - radius, top + radius, radius, -Math.PI / 2, 0);
                    cr.LineTo (left + width, top + height);
                    cr.LineTo (left, top + height);
                    cr.ClosePath ();

                    if (selected) {
                        color = "grey2";
                        color.A = 1.0f;
                    } else {
                        color = "grey4";
                        color.A = 0.1f;
                    }
                    color.SetSource (cr);

                    cr.Fill ();

                    TouchText render = new TouchText (text);
                    render.alignment = TouchAlignment.Center;
                    render.font.color = "white";
                    render.Render (this, left, top, width, height);
                }
            }
        }

        private class TimerBackground : TouchGraphicalBox
        {
            public TimerBackground (int width, int height) : base (width, height) { }

            protected override void OnExpose (object sender, ExposeEventArgs args) {
                using (Context cr = Gdk.CairoHelper.Create (this.GdkWindow)) {
                    cr.Rectangle (Allocation.Left, Allocation.Top, Allocation.Width, Allocation.Height);
                    TouchColor.SetSource (cr, color, transparency);
                    cr.Fill ();

                    cr.MoveTo (Allocation.Left, Allocation.Top);
                    cr.LineTo (Allocation.Right + 1, Allocation.Top);
                    TouchColor.SetSource (cr, "grey2", 1.0);
                    cr.LineWidth = 2.0;
                    cr.Stroke ();
                }
            }
        }
    }
}

