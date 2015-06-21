using System;
using System.Collections.Generic;
using System.Text;
using Gtk;
using Cairo;
using MyWidgetLibrary;
using AquaPic.Runtime;

namespace AquaPic
{
    public class LoggerWindow : MyBackgroundWidget
    {
        private TextView tv;
        private uint timerId;

        public LoggerWindow (params object[] options) : base () {
            var box = new MyBox (780, 395);
            Put (box, 10, 30);

            var label = new TouchLabel ();
            label.text = "Logger";
            label.textSize = 13;
            label.textColor = "pri";
            label.WidthRequest = 780;
            label.textAlignment = MyAlignment.Center;
            Put (label, 10, 35);

            var b = new TouchButton ();
            b.SetSizeRequest (100, 40);
            b.text = "Clear Logger";
            b.ButtonReleaseEvent += (o, args) => {
                var ms = new MessageDialog (
                    null,
                    DialogFlags.DestroyWithParent,
                    MessageType.Question,
                    ButtonsType.YesNo,
                    "Are you sure you want to clear all the contents of the event logger");
                
                ms.Response += (sender, a) => {
                    if (a.ResponseId == ResponseType.Yes)
                        EventLogger.buffer.Clear ();
                };
                ms.Run ();
                ms.Destroy ();
            };
            Put (b, 685, 380);

            tv = new TextView ();
            tv.ModifyFont (Pango.FontDescription.FromString ("Courier New 11"));
            tv.ModifyBase (StateType.Normal, MyColor.NewGtkColor ("grey4"));
            tv.ModifyText (StateType.Normal, MyColor.NewGtkColor ("black"));
            tv.CanFocus = false;
            tv.Buffer = EventLogger.buffer;

            ScrolledWindow sw = new ScrolledWindow ();
            sw.SetSizeRequest (770, 315);
            sw.Add (tv);
            tv.Show ();
            Put (sw, 15, 60);
            sw.Show ();

            EventLogger.EventAddedEvent += OnEventAdded;

            Show ();
        }

        public override void Dispose () {
            EventLogger.EventAddedEvent -= OnEventAdded;

            base.Dispose ();
        }

        protected void OnEventAdded () {
            tv.Buffer = EventLogger.buffer;
            tv.Show ();
        }
    }
}

