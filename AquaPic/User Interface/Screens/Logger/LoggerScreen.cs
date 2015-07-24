using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Gtk;
using Cairo;
using MyWidgetLibrary;
using AquaPic.Runtime;

namespace AquaPic.UserInterface
{
    public class LoggerWindow : WindowBase
    {
        private TextView tv;

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
            b.ButtonReleaseEvent += OnClearButtonRelease;
            Put (b, 685, 380);

            b = new TouchButton ();
            b.SetSizeRequest (100, 40);
            b.text = "Save Logger";
            b.ButtonReleaseEvent += (o, args) => SaveEvents ();
            Put (b, 575, 380);

            tv = new TextView ();
            tv.ModifyFont (Pango.FontDescription.FromString ("Sans 11"));
            tv.ModifyBase (StateType.Normal, MyColor.NewGtkColor ("grey4"));
            tv.ModifyText (StateType.Normal, MyColor.NewGtkColor ("black"));
            tv.CanFocus = false;
            tv.Editable = false;
            tv.Buffer = Logger.buffer;

            ScrolledWindow sw = new ScrolledWindow ();
            sw.SetSizeRequest (770, 315);
            sw.Add (tv);
            tv.Show ();
            Put (sw, 15, 60);
            sw.Show ();

            Logger.EventAddedEvent += OnEventAdded;

            Show ();
        }

        public override void Dispose () {
            Logger.EventAddedEvent -= OnEventAdded;

            base.Dispose ();
        }

        protected void OnEventAdded () {
            tv.Buffer = Logger.buffer;
            tv.Show ();
        }

        protected void OnClearButtonRelease (object sender, ButtonReleaseEventArgs args) {
            var ms = new MessageDialog (
                null,
                DialogFlags.DestroyWithParent,
                MessageType.Question,
                ButtonsType.YesNo,
                "Save events before clearing");

            ms.Response += (o, a) => {
                if (a.ResponseId == ResponseType.Yes) {
                    SaveEvents ();
                    Logger.buffer.Clear ();
                } else if (a.ResponseId == ResponseType.No) {
                    var d = new MessageDialog (
                        null,
                        DialogFlags.DestroyWithParent,
                        MessageType.Question,
                        ButtonsType.YesNo,
                        "Are you sure you want to clear all the contents of the event logger");

                    d.Response += (obj, arg) => {
                        if (arg.ResponseId == ResponseType.Yes)
                            Logger.buffer.Clear ();
                    };

                    d.Run ();
                    d.Destroy ();
                }
            };

            ms.Run ();
            ms.Destroy ();
        }

        protected void SaveEvents () {
            if (!string.IsNullOrWhiteSpace (Logger.buffer.Text)) {
                string path = System.IO.Path.Combine (Environment.GetEnvironmentVariable ("AquaPic"), "AquaPicRuntimeProject");
                path = System.IO.Path.Combine (path, "Logs");
                path = System.IO.Path.Combine (path, DateTime.Now.ToString ("yy-MM-dd-HH-mm-ss") + ".txt");

                List<string> lines = new List<string> ();
                for (int i = 0; i < Logger.buffer.LineCount; ++i) {
                    TextIter tis = Logger.buffer.GetIterAtLine (i);
                    TextIter tie = Logger.buffer.GetIterAtLine (i + 1);
                    lines.Add (Logger.buffer.GetText (tis, tie, true));
                }

                string[] l = lines.ToArray ();

                File.WriteAllLines (path, l);
            }
        }
    }
}

