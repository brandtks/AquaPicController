using System;
using Gtk;
using Cairo;
using TouchWidgetLibrary;
using AquaPic.Runtime;

namespace AquaPic.UserInterface
{
    public class DeluxeTimerWidget : Fixed
    {
        private DeluxeTimer[] timers;
        private TimerTab[] tabs;
        private int timerIndex;
        private TouchTextBox minutes;
        private TouchTextBox seconds;
        private TouchUpDownButtons minUpDown;
        private TouchUpDownButtons secUpDown;
        private TouchButton startStopButton;
        private TouchButton resetButton;

        public DeluxeTimerWidget (string name) {
            timerIndex = 0;

            SetSizeRequest (310, 169);

            timers = new DeluxeTimer[3];
            for (int i = 0; i < timers.Length; ++i) {
                timers [i] = DeluxeTimer.GetTimer ("Timer " + name + " " + (i + 1).ToString ());
                timers [i].TimerInterumEvent += OnTimerInterum;
                timers [i].TimerElapsedEvent += OnTimerElapsed;
                timers [i].TimerStartEvent += OnTimerStartStop;
                timers [i].TimerStopEvent += OnTimerStartStop;
            }

            var box2 = new TimerBackground (310, 129);
            box2.color = "grey1";
            box2.transparency = 1.0f;
            Put (box2, 0, 40);

            tabs = new TimerTab[3];
            for (int i = 2; i >= 0; --i) {
                tabs [i] = new TimerTab (i);
                tabs [i].text = "Timer " + (i + 1).ToString ();
                tabs [i].ButtonReleaseEvent += OnTabButtonRelease;
                Put (tabs [i], 90 * i, 0);
                tabs [i].Show ();
            }

            tabs [0].selected = true;

            var minuteLabel = new TouchLabel ();
            minuteLabel.text = "Minutes";
            Put (minuteLabel, 5, 47);
            minuteLabel.Show ();

            minutes = new TouchTextBox ();
            minutes.SetSizeRequest (99, 46);
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
            Put (minutes, 3, 67);

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
            Put (minUpDown, 3, 117);
            minUpDown.Show ();

            var secondsLabel = new TouchLabel ();
            secondsLabel.text = "Seconds";
            Put (secondsLabel, 108, 47);
            secondsLabel.Show ();

            seconds = new TouchTextBox ();
            seconds.SetSizeRequest (98, 46);
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
            Put (seconds, 106, 67);

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
            Put (secUpDown, 106, 117);
            secUpDown.Show ();

            startStopButton = new TouchButton ();
            startStopButton.SetSizeRequest (98, 56);
            startStopButton.ButtonReleaseEvent += OnStartStopButtonRelease;
            Put (startStopButton, 209, 47);

            resetButton = new TouchButton ();
            resetButton.SetSizeRequest (98, 56);
            resetButton.text = "Reset";
            resetButton.ButtonReleaseEvent += OnResetButtonRelease;
            Put (resetButton, 209, 107);

            if (timers [timerIndex].enabled)
                UpdateTime (timers [timerIndex].secondsRemaining, false);
            else
                UpdateTime (timers [timerIndex].totalSeconds, false);
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
            DeluxeTimer timer = sender as DeluxeTimer;
            int tIdx = Convert.ToInt32 (timer.name [timer.name.Length - 1].ToString ()) - 1;
            if (timerIndex == tIdx)
                UpdateTime (timer.secondsRemaining, false);
        }

        protected void OnTimerElapsed (object sender, TimerElapsedEventArgs args) {
            DeluxeTimer timer = sender as DeluxeTimer;
            int tIdx = Convert.ToInt32 (timer.name [timer.name.Length - 1].ToString ()) - 1;
            if (timerIndex == tIdx)
                UpdateTime (timers [timerIndex].totalSeconds);

            MessageBox.Show (timer.name);
        }

        protected void OnTimerStartStop (object sender) {
            DeluxeTimer timer = sender as DeluxeTimer;
            int tIdx = Convert.ToInt32 (timer.name [timer.name.Length - 1].ToString ()) - 1;
            if (timerIndex == tIdx)
                UpdateTime (timers [timerIndex].secondsRemaining, false);
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
            if (timers [timerIndex].state != DeluxeTimerState.Waiting) {
                timers [timerIndex].Reset ();
            }

            if (timers [timerIndex].secondsRemaining != timers [timerIndex].totalSeconds)
                UpdateTime (timers [timerIndex].totalSeconds);

            UpdateScreen ();
        }

        protected void OnTabButtonRelease (object sender, ButtonReleaseEventArgs args) {
            TimerTab b = sender as TimerTab;
            timerIndex = b.position;

            if (timers [timerIndex].enabled)
                UpdateTime (timers [timerIndex].secondsRemaining, false);
            else
                UpdateTime (timers [timerIndex].totalSeconds, false);

            foreach (var tab in tabs) {
                
                tab.selected = false;
            }

            Remove (tabs [timerIndex]);
            tabs [timerIndex].Destroy ();
            tabs [timerIndex].Dispose ();
            tabs [timerIndex] = new TimerTab (timerIndex);
            tabs [timerIndex].text = "Timer " + (timerIndex + 1).ToString ();
            tabs [timerIndex].ButtonReleaseEvent += OnTabButtonRelease;
            Put (tabs [timerIndex], 90 * timerIndex, 0);
            tabs [timerIndex].Show ();
            tabs [timerIndex].selected = true;
        }

        protected void UpdateTime (uint time, bool changeTimerTime = true) {
            minutes.text = (time / 60).ToString ();
            seconds.text = (time % 60).ToString ();
            minutes.QueueDraw ();
            seconds.QueueDraw ();

            if (changeTimerTime) {
                timers [timerIndex].totalSeconds = time;
            }

            UpdateScreen ();
        }

        protected void UpdateScreen () {
            if (timers [timerIndex].state == DeluxeTimerState.Waiting)
                resetButton.buttonColor = "grey2";
            else
                resetButton.buttonColor = "pri";

            if (timers [timerIndex].enabled) {
                startStopButton.text = "Stop";
                startStopButton.buttonColor = "pri";
            } else {
                startStopButton.text = "Start";
                startStopButton.buttonColor = "seca";

                if (timers [timerIndex].totalSeconds == 0)
                    secUpDown.down.buttonColor = "grey2";
                else
                    secUpDown.down.buttonColor = "pri";

                if (minutes.text == "0")
                    minUpDown.down.buttonColor = "grey2";
                else
                    minUpDown.down.buttonColor = "pri";

                secUpDown.down.QueueDraw ();
                minUpDown.down.QueueDraw ();
            }

            resetButton.QueueDraw ();
            startStopButton.QueueDraw ();
        }

        private class TimerTab : EventBox
        {
            public TouchColor color;
            public string text;
            public bool selected;
            public int position;

            public TimerTab (int position) {
                SetSizeRequest (130, 40);

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
                    int radius = 10;

                    if (!selected) {
                        width -= 26;
                        left += position * 13;
                        top += 10;
                        height -= 10;
                    }

                    cr.MoveTo (left, top + radius);
                    cr.Arc (left + radius, top + radius, radius, Math.PI, -Math.PI / 2);
                    cr.LineTo (left + width - radius, top);
                    cr.Arc (left + width - radius, top + radius, radius, -Math.PI / 2, 0);
                    cr.LineTo (left + width, top + height);
                    cr.LineTo (left, top + height);
                    cr.ClosePath ();

                    if (selected) {
                        color = "pri";
                    } else {
                        color = "grey3";
                    }
                    color.SetSource (cr);

                    if (selected) {
                        cr.Fill ();
                    } else {
                        cr.FillPreserve ();
                        cr.LineWidth = 0.4;
                        TouchColor.SetSource (cr, "black");
                        cr.Stroke ();
                    }

                    TouchText render = new TouchText (text);
                    render.alignment = TouchAlignment.Center;
                    render.font.color = "black";
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

                    cr.MoveTo (Allocation.Left, Allocation.Top - 1);
                    cr.LineTo (Allocation.Right, Allocation.Top - 1);
                    TouchColor.SetSource (cr, "pri");
                    cr.LineWidth = 8.0;
                    cr.Stroke ();
                }
            }
        }
    }
}

