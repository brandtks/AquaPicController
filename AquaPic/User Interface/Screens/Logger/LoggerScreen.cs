using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Gtk;
using Cairo;
using TouchWidgetLibrary;
using AquaPic.Runtime;
using AquaPic.Utilites;

namespace AquaPic.UserInterface
{
    public class LoggerWindow : WindowBase
    {
        public TextView tv;

        public LoggerWindow (params object[] options) : base () {
            screenTitle = "Logger";

            var b = new TouchButton ();
            b.SetSizeRequest (100, 60);
            b.text = "Clear Logger";
            b.ButtonReleaseEvent += OnClearButtonRelease;
            Put (b, 685, 405);

            b = new TouchButton ();
            b.SetSizeRequest (100, 60);
            b.text = "Save Logger";
            b.ButtonReleaseEvent += (o, args) => SaveEvents ();
            Put (b, 575, 405);

            tv = new TextView (Logger.buffer);
            tv.ModifyFont (Pango.FontDescription.FromString ("Sans 11"));
            tv.ModifyBase (StateType.Normal, TouchColor.NewGtkColor ("grey4"));
            tv.ModifyText (StateType.Normal, TouchColor.NewGtkColor ("black"));
            tv.CanFocus = false;
            tv.Editable = false;

            ScrolledWindow sw = new ScrolledWindow ();
            sw.SetSizeRequest (720, 340);
            sw.VScrollbar.WidthRequest = 30;
            sw.HScrollbar.HeightRequest = 30;
            sw.Add (tv);
            Put (sw, 65, 60);
            sw.Show ();
            tv.Show ();

            Logger.EventAddedEvent += OnEventAdded;
            Show ();
        }

        public override void Dispose () {
            Logger.EventAddedEvent -= OnEventAdded;
            base.Dispose ();
        }

        protected void OnEventAdded () {
            tv.Buffer = Logger.buffer;
            tv.QueueDraw ();
        }

        protected void OnClearButtonRelease (object sender, ButtonReleaseEventArgs args) {
            var parent = this.Toplevel as Gtk.Window;
            if (parent != null) {
                if (!parent.IsTopLevel)
                    parent = null;
            }

            var ms = new TouchDialog ("Save events before clearing", parent);

            ms.Response += (o, a) => {
                if (a.ResponseId == ResponseType.Yes) {
                    SaveEvents ();
                    Logger.buffer.Clear ();
                } else if (a.ResponseId == ResponseType.No) {
                    ms.Destroy ();

                    var parent2 = this.Toplevel as Gtk.Window;
                    if (parent != null) {
                        if (!parent.IsTopLevel)
                            parent = null;
                    }
                    var d = new TouchDialog ("Are you sure you want to clear all the contents of the event logger", parent2);

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
                string path = System.IO.Path.Combine (Utils.AquaPicEnvironment, "AquaPicRuntimeProject");
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

