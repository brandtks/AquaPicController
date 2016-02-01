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
        public TextView tv;
        private uint timerId;

        public AlarmWindow (params object[] options) : base () {
            //var box = new TouchGraphicalBox (730, 440);
            //Put (box, 60, 30);

            //var label = new TouchLabel ();
            //label.text = "Current Alarms";
            //label.textSize = 14;
            //label.textColor = "pri";
            //label.WidthRequest = 780;
            //label.textAlignment = TouchAlignment.Center;
            //Put (label, 10, 35);
            //label.Show ();

            screenTitle = "Current Alarms";

            var b = new TouchButton ();
            b.SetSizeRequest (100, 60);
            b.text = "Acknowledge Alarms";
            b.ButtonReleaseEvent += (o, args) => {
                Alarm.Acknowledge ();
                OnTimer ();
            };
            Put (b, 685, 405);
            b.Show ();

            tv = new TextView ();
            tv.ModifyFont (Pango.FontDescription.FromString ("Sans 11"));
            tv.ModifyBase (StateType.Normal, TouchColor.NewGtkColor ("grey4"));
            tv.CanFocus = false;

            ScrolledWindow sw = new ScrolledWindow ();
            sw.SetSizeRequest (720, 340);
            sw.VScrollbar.WidthRequest = 30;
            sw.HScrollbar.HeightRequest = 30;
            sw.Add (tv);
            Put (sw, 65, 60);
            sw.Show ();
            tv.Show ();

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

