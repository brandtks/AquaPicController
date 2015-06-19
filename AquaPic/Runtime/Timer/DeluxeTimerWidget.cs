using System;
using Gtk;
using MyWidgetLibrary;

namespace AquaPic.Runtime
{
    public class DeluxeTimerWidget : Fixed
    {
        public DeluxeTimer timer;

        private TouchTextBox minutes;
        private TouchTextBox seconds;
        private UpDownButtons minUpDown;
        private UpDownButtons secUpDown;

        public DeluxeTimerWidget (string name) {
            SetSizeRequest (334, 95);

            timer = DeluxeTimer.GetTimer (name);
            timer.TimerInterumEvent += OnTimerInterum;

            var box = new MyBox (334, 95);
            Put (box, 0, 0);

            minutes = new TouchTextBox ();
            minutes.enableTouch = true;
            minutes.SetSizeRequest (75, 89);
            Put (minutes, 3, 3);

            minUpDown = new UpDownButtons ();
            minUpDown.up.ButtonReleaseEvent += (o, args) => {
                if (!timer.enabled) {
                    uint time = (Convert.ToUInt32 (minutes.text) + 1) * 60;
                    time += Convert.ToUInt32 (seconds.text);
                    UpdateText (time);
                }
            };
            minUpDown.down.ButtonReleaseEvent += (o, args) => {
                if (!timer.enabled) {
                    if (minutes.text != "0") {
                        uint time = (Convert.ToUInt32 (minutes.text) - 1) * 60;
                        time += Convert.ToUInt32 (seconds.text);
                        UpdateText (time);
                    }
                }
            };
            Put (minUpDown, 79, 3);

            seconds = new TouchTextBox ();
            seconds.enableTouch = true;
            seconds.TextChangedEvent += (sender, args) => UpdateText (Convert.ToUInt32 (args.text));
            seconds.SetSizeRequest (75, 89);
            Put (seconds, 125, 3);

            secUpDown = new UpDownButtons ();
            secUpDown.up.ButtonPressEvent += (o, args) => {
                if (!timer.enabled) {
                    uint time = Convert.ToUInt32 (minutes.text) * 60;
                    time += Convert.ToUInt32 (seconds.text);
                    ++time;
                    UpdateText (time);
                }
            };
            secUpDown.down.ButtonPressEvent += (o, args) => {
                if (!timer.enabled) {
                    uint time = Convert.ToUInt32 (minutes.text) * 60;
                    time += Convert.ToUInt32 (seconds.text);
                    if (time != 0) {
                        --time;
                        UpdateText (time);
                    }
                }
            };
            Put (secUpDown, 201, 3);

            var b = new TouchButton ();
            b.SetSizeRequest (89, 89);
            if (timer.enabled) {
                b.text = "Stop";
                b.buttonColor = "pri";
            } else {
                b.text = "Start";
                b.buttonColor = "seca";
            }
            b.ButtonReleaseEvent += OnButtonRelease;
            Put (b, 248, 3);

            UpdateText (timer.secondsRemaining);
        }

        protected void OnTimerInterum (object sender) {
            UpdateText (timer.secondsRemaining);
        }

        protected void OnButtonRelease (object sender, ButtonReleaseEventArgs args) {
            TouchButton b = sender as TouchButton;

            if (b.text == "Start") {
                timer.SetTime (Convert.ToUInt32 (minutes.text), Convert.ToUInt32 (seconds.text));
                timer.Start ();
                b.text = "Stop";
                b.buttonColor = "pri";
            } else {
                timer.Stop ();
                b.text = "Start";
                b.buttonColor = "seca";
            }

            b.QueueDraw ();
        }

        protected void UpdateText (uint time) {
            minutes.text = (time / 60).ToString ();
            seconds.text = (time % 60).ToString ();
            minutes.QueueDraw ();
            seconds.QueueDraw ();

            if (!timer.enabled) {
                if (time == 0) {
                    secUpDown.down.buttonColor = "grey3";
                    secUpDown.down.QueueDraw ();
                } else {
                    secUpDown.down.buttonColor = "pri";
                    secUpDown.down.QueueDraw ();
                }

                if (minutes.text == "0") {
                    minUpDown.down.buttonColor = "grey3";
                    minUpDown.down.QueueDraw ();
                } else {
                    minUpDown.down.buttonColor = "pri";
                    minUpDown.down.QueueDraw ();
                }
            }
        }
    }

    public class UpDownButtons : Fixed
    {
        public TouchButton up;
        public TouchButton down;

        public UpDownButtons () {
            SetSizeRequest (44, 89);

            up = new TouchButton ();
            up.SetSizeRequest (44, 44);
            up.text = Convert.ToChar (0x22C0).ToString (); // 2191
            Put (up, 0, 0);

            down = new TouchButton ();
            down.SetSizeRequest (44, 44);
            down.text = Convert.ToChar (0x22C1).ToString (); // 2193
            Put (down, 0, 45);
        }
    }
}

