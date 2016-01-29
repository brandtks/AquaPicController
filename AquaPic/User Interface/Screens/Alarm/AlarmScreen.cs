using System;
using System.Collections.Generic;
using System.Text;
using Gtk;
using Cairo;
using TouchWidgetLibrary;
using AquaPic.Runtime;

namespace AquaPic.UserInterface
{
    public class AlarmWindow : WindowBase
    {
        private TextView tv;
        private uint timerId;

        public AlarmWindow (params object[] options) : base () {
            var box = new TouchGraphicalBox (780, 395);
            Put (box, 10, 30);

            var label = new TouchLabel ();
            label.text = "Current Alarms";
            label.textSize = 13;
            label.textColor = "pri";
            label.WidthRequest = 780;
            label.textAlignment = TouchAlignment.Center;
            Put (label, 10, 35);

            var b = new TouchButton ();
            b.SetSizeRequest (100, 40);
            b.text = "Acknowledge Alarms";
            b.ButtonReleaseEvent += (o, args) => {
                Alarm.Acknowledge ();
                OnTimer ();
            };
            Put (b, 685, 380);

            tv = new TextView ();
            tv.ModifyFont (Pango.FontDescription.FromString ("Sans 11"));
            tv.ModifyBase (StateType.Normal, TouchColor.NewGtkColor ("grey4"));
            tv.CanFocus = false;

            ScrolledWindow sw = new ScrolledWindow ();
            sw.SetSizeRequest (770, 315);
            sw.Add (tv);
            tv.Show ();
            Put (sw, 15, 60);
            sw.Show ();

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
            TextBuffer tb = tv.Buffer;
            tb.Text = string.Empty;

            foreach (var a in alarming) {
                var tag = new TextTag (null);
                if (a.acknowledged)
                    tag.ForegroundGdk = TouchColor.NewGtkColor ("seca");
                else
                    tag.ForegroundGdk = TouchColor.NewGtkColor ("compl");
                TextTagTable ttt = tb.TagTable;
                ttt.Add (tag);

                var ti = tb.EndIter;
                tb.InsertWithTags (ref ti, string.Format ("{0:MM/dd hh:mm:ss}: {1}\n", a.postTime, a.name), tag);
            }

            return true;
        }
    }
}

