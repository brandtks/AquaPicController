using System;
using Gtk;
using Cairo;
using MyWidgetLibrary;
using AquaPic.Runtime;

namespace AquaPic.UserInterface
{
    public class DeluxeTimerWidget : Fixed
    {
        private DeluxeTimer[] timers;
        private TouchTab[] tabs;
        private int t;
        private TouchTextBox minutes;
        private TouchTextBox seconds;
        private UpDownButtons minUpDown;
        private UpDownButtons secUpDown;
        private TouchButton startStopButton;
        private TouchButton resetButton;

        public DeluxeTimerWidget (string name) {
            t = 0;

            SetSizeRequest (334, 95);

            timers = new DeluxeTimer[3];
            for (int i = 0; i < timers.Length; ++i) {
                timers [i] = DeluxeTimer.GetTimer ("Timer " + name + " " + (i + 1).ToString ());
                timers [i].TimerInterumEvent += OnTimerInterum;
                timers [i].TimerElapsedEvent += OnTimerElapsed;
                timers [i].TimerStartEvent += OnTimerStartStop;
                timers [i].TimerStopEvent += OnTimerStartStop;
            }

            var box2 = new MyBox (333, 66);
            box2.color = "grey2";
            Put (box2, 0, 29);

            tabs = new TouchTab[3];
            for (int i = 0; i < tabs.Length; ++i) {
                tabs [i] = new TouchTab ();
                tabs [i].text = "Timer " + (i + 1).ToString ();
                tabs [i].ButtonReleaseEvent += OnTabButtonRelease;
                Put (tabs [i], 0 + (111 * i), 0);
                tabs [i].Show ();
            }

            minutes = new TouchTextBox ();
            minutes.SetSizeRequest (75, 61);
            minutes.enableTouch = true;
            minutes.textSize = 16;
            minutes.textAlignment = MyAlignment.Center;
            minutes.TextChangedEvent += (sender, args) => {
                try {
                    uint time = Convert.ToUInt32 (args.text) * 60;
                    time += Convert.ToUInt32 (seconds.text);
                    UpdateTime (time);
                } catch {
                    args.keepText = false;
                }
            };
            Put (minutes, 3, 32);

            minUpDown = new UpDownButtons ();
            minUpDown.up.ButtonReleaseEvent += (o, args) => {
                if (!timers[t].enabled) {
                    uint time = (Convert.ToUInt32 (minutes.text) + 1) * 60;
                    time += Convert.ToUInt32 (seconds.text);
                    UpdateTime (time);
                }
            };
            minUpDown.down.ButtonReleaseEvent += (o, args) => {
                if (!timers[t].enabled) {
                    if (minutes.text != "0") {
                        uint time = (Convert.ToUInt32 (minutes.text) - 1) * 60;
                        time += Convert.ToUInt32 (seconds.text);
                        UpdateTime (time);
                    }
                }
            };
            Put (minUpDown, 79, 32);
            minUpDown.Show ();

            seconds = new TouchTextBox ();
            seconds.SetSizeRequest (75, 61);
            seconds.enableTouch = true;
            seconds.textAlignment = MyAlignment.Center;
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
            Put (seconds, 125, 32);

            secUpDown = new UpDownButtons ();
            secUpDown.up.ButtonPressEvent += (o, args) => {
                if (!timers[t].enabled) {
                    uint time = Convert.ToUInt32 (minutes.text) * 60;
                    time += Convert.ToUInt32 (seconds.text);
                    ++time;
                    UpdateTime (time);
                }
            };
            secUpDown.down.ButtonPressEvent += (o, args) => {
                if (!timers[t].enabled) {
                    uint time = Convert.ToUInt32 (minutes.text) * 60;
                    time += Convert.ToUInt32 (seconds.text);
                    if (time != 0) {
                        --time;
                        UpdateTime (time);
                    }
                }
            };
            Put (secUpDown, 201, 32);
            secUpDown.Show ();

            startStopButton = new TouchButton ();
            startStopButton.SetSizeRequest (83, 30);
            startStopButton.ButtonReleaseEvent += OnStartStopButtonRelease;
            Put (startStopButton, 248, 32);

            resetButton = new TouchButton ();
            resetButton.SetSizeRequest (83, 30);
            resetButton.text = "Reset";
            resetButton.ButtonReleaseEvent += OnResetButtonRelease;
            Put (resetButton, 248, 63);

            if (timers [t].enabled)
                UpdateTime (timers [t].secondsRemaining, false);
            else
                UpdateTime (timers [t].totalSeconds, false);
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
            if (t == tIdx)
                UpdateTime (timer.secondsRemaining, false);
        }

        protected void OnTimerElapsed (object sender, TimerElapsedEventArgs args) {
            DeluxeTimer timer = sender as DeluxeTimer;
            int tIdx = Convert.ToInt32 (timer.name [timer.name.Length - 1].ToString ()) - 1;
            if (t == tIdx)
                UpdateTime (timers [t].totalSeconds);
        }

        protected void OnTimerStartStop (object sender) {
            DeluxeTimer timer = sender as DeluxeTimer;
            int tIdx = Convert.ToInt32 (timer.name [timer.name.Length - 1].ToString ()) - 1;
            if (t == tIdx)
                UpdateTime (timers [t].secondsRemaining, false);
        }

        protected void OnStartStopButtonRelease (object sender, ButtonReleaseEventArgs args) {
            if (startStopButton.text == "Start") {
                timers[t].SetTime (Convert.ToUInt32 (minutes.text), Convert.ToUInt32 (seconds.text));
                timers[t].Start ();
            } else {
                timers[t].Stop ();
            }

            UpdateScreen ();
        }

        protected void OnResetButtonRelease (object sender, ButtonReleaseEventArgs args) {
            if (timers [t].enabled) {
                timers [t].Stop ();
            }

            if (timers [t].secondsRemaining != timers [t].totalSeconds)
                UpdateTime (timers [t].totalSeconds);

            UpdateScreen ();
        }

        protected void OnTabButtonRelease (object sender, ButtonReleaseEventArgs args) {
            TouchTab b = sender as TouchTab;
            t = Convert.ToInt32 ((b.text [b.text.Length - 1]).ToString ()) - 1;

            if (timers [t].enabled)
                UpdateTime (timers [t].secondsRemaining, false);
            else
                UpdateTime (timers [t].totalSeconds, false);
        }

        protected void UpdateTime (uint time, bool changeTimerTime = true) {
            minutes.text = (time / 60).ToString ();
            seconds.text = (time % 60).ToString ();
            minutes.QueueDraw ();
            seconds.QueueDraw ();

            if (changeTimerTime) {
                timers [t].totalSeconds = time;
                if (!timers [t].enabled)
                    timers [t].secondsRemaining = time;
            }

            UpdateScreen ();
        }

        protected void UpdateScreen () {
            for (int i = 0; i < tabs.Length; ++i) {
                if (i == t)
                    tabs [i].color = "pri";
                else
                    tabs [i].color = "grey3";

                tabs [i].QueueDraw ();
            }

            if ((timers [t].secondsRemaining == timers [t].totalSeconds) && !timers [t].enabled)
                resetButton.buttonColor = "grey2";
            else
                resetButton.buttonColor = "pri";

            if (timers[t].enabled) {
                startStopButton.text = "Stop";
                startStopButton.buttonColor = "pri";
            } else {
                startStopButton.text = "Start";
                startStopButton.buttonColor = "seca";

                if (timers [t].totalSeconds == 0)
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

        private class UpDownButtons : Fixed
        {
            public TouchButton up;
            public TouchButton down;

            public UpDownButtons () {
                SetSizeRequest (44, 61);

                up = new TouchButton ();
                up.SetSizeRequest (44, 30);
                up.text = Convert.ToChar (0x22C0).ToString (); // 2191
                Put (up, 0, 0);

                down = new TouchButton ();
                down.SetSizeRequest (44, 30);
                down.text = Convert.ToChar (0x22C1).ToString (); // 2193
                Put (down, 0, 31);
            }
        }

        private class TouchTab : EventBox
        {
            public MyColor color;
            public string text;

            public TouchTab () {
                SetSizeRequest (111, 29);

                VisibleWindow = false;
                ExposeEvent += OnEventBoxExpose;
                ButtonPressEvent += OnEventBoxButtonPress;
                ButtonReleaseEvent += OnEventBoxButtonRelease;

                color = "pri";
                text = string.Empty;
            }

            protected void OnEventBoxExpose (object sender, ExposeEventArgs args) {
                EventBox eb = sender as EventBox;

                using (Context cr = Gdk.CairoHelper.Create (eb.GdkWindow)) {
                    int height = Allocation.Height;
                    int width = Allocation.Width;
                    int top = Allocation.Top;
                    int left = Allocation.Left;
                    int radius = 10;

                    cr.MoveTo (left, top + radius);
                    cr.Arc (left + radius, top + radius, radius, Math.PI, -Math.PI / 2);
                    cr.LineTo (left + width - radius, top);
                    cr.Arc (left + width - radius, top + radius, radius, -Math.PI / 2, 0);
                    cr.LineTo (left + width, top + height);
                    cr.LineTo (left, top + height);
                    cr.ClosePath ();

                    color.SetSource (cr);
                    cr.FillPreserve ();

                    cr.LineWidth = 0.5;
                    MyColor.SetSource (cr, "black");
                    cr.Stroke ();

                    Pango.Layout l = new Pango.Layout (eb.PangoContext);
                    l.Width = Pango.Units.FromPixels (width - 2);
                    l.Wrap = Pango.WrapMode.Word;
                    l.Alignment = Pango.Alignment.Center;
                    l.SetMarkup ("<span color=" + (char)34 + "black" + (char)34 + ">" + text + "</span>"); 
                    l.FontDescription = Pango.FontDescription.FromString ("Courier New 11");
                    int y = (top + (height / 2)) - 8;
                    y -= ((l.LineCount - 1) * 9);
                    GdkWindow.DrawLayout (Style.TextGC(StateType.Normal), left + 1, y, l);
                    l.Dispose ();
                }
            }

            protected void OnEventBoxButtonPress (object o, ButtonPressEventArgs args) {
                if (args.Event.Type == Gdk.EventType.ButtonPress) {
                    color.ModifyColor (0.75);
                    this.QueueDraw ();
                }
            }

            protected void OnEventBoxButtonRelease (object o, ButtonReleaseEventArgs args) {
                color.RestoreColor ();
                this.QueueDraw ();
            }
        }
    }
}

