using System;
using System.Collections.Generic;
using System.Text;
using Gtk;
using Cairo;
using MyWidgetLibrary;
using AquaPic.Runtime;

namespace AquaPic
{
    public class AlarmWindow : MyBackgroundWidget
    {
        private AlarmLabel[] labels;
        private uint timerId;

        public AlarmWindow (params object[] options) : base () {
            var box = new MyBox (780, 395);
            Put (box, 10, 30);

            var box2 = new MyBox (770, 315);
            box2.color = "grey2";
            Put (box2, 15, 60);

            var label = new TouchLabel ();
            label.text = "Current Alarms";
            label.textSize = 13;
            label.textColor = "pri";
            label.WidthRequest = 780;
            label.textAlignment = MyAlignment.Center;
            Put (label, 10, 35);

            var b = new TouchButton ();
            b.SetSizeRequest (100, 40);
            b.text = "Acknowledge Alarms";
            b.ButtonReleaseEvent += (o, args) => {
                Alarm.Acknowledge ();
                OnTimer ();
            };
            Put (b, 685, 380);

            int y = 65;
            labels = new AlarmLabel[Alarm.totalAlarms];
            for (var i = 0; i < labels.Length; ++i) {
                labels [i] = new AlarmLabel ();
                Put (labels [i], 20, y);
                labels [i].Show ();
                y += 25;
            }

            OnTimer ();

            timerId = GLib.Timeout.Add (1000, OnTimer);

            Show ();
        }

        public override void Dispose () {
            GLib.Source.Remove (timerId);
            base.Dispose ();
        }

        protected bool OnTimer () {
            List<AlarmData> alarming = Alarm.GetAllAlarming ();
            for (var i = 0; i < labels.Length; ++i) {
                if (i < alarming.Count) {
                    if (alarming [i].acknowledged)
                        labels [i].color = "seca";
                    else
                        labels [i].color = "compl";
                    labels [i].nameLabel.text = alarming [i].name;
                    labels [i].dateLabel.text = alarming [i].postTime.ToString ("MM/dd hh:mm");
                    labels [i].box.Visible = true;
                } else {
                    labels [i].nameLabel.text = string.Empty;
                    labels [i].dateLabel.text = string.Empty;
                    labels [i].box.Visible = false;
                }
                labels [i].QueueDraw ();
            }

            return true;
        }
    }

    public class AlarmLabel : Fixed
    {
        public string color {
            set {
                nameLabel.textColor = value;
                dateLabel.textColor = value;
            }
        }

        public TouchLabel nameLabel;
        public TouchLabel dateLabel;
        public MyBox box;

        public AlarmLabel () {
            SetSizeRequest (760, 20);

            box = new MyBox (760, 20);
            box.color = "grey4";
            box.transparency = 1.0f;
            Put (box, 0, 0);
            box.Show ();

            nameLabel = new TouchLabel ();
            nameLabel.WidthRequest = 595;
            nameLabel.textAlignment = MyAlignment.Left;
            Put (nameLabel, 5, 0);
            nameLabel.Show ();

            dateLabel = new TouchLabel ();
            dateLabel.WidthRequest = 155;
            dateLabel.textAlignment = MyAlignment.Right;
            Put (dateLabel, 600, 0);
            dateLabel.Show ();
        }
    }
}

