using System;
using System.Collections.Generic;
using Cairo;
using Gtk;

namespace MyWidgetLibrary
{
    public delegate bool SaveHandler (object sender);

    public class TouchSettingsDialog : Gtk.Dialog
    {
        public Fixed fix;
        public event SaveHandler SaveEvent;
        public Dictionary<string, SettingsWidget> settings;

        public TouchSettingsDialog (string name) {
            Name = "AquaPic.Settings." + name;
            Title = name + "Settings";
            WindowPosition = (Gtk.WindowPosition)4;
            SetSizeRequest (600, 320);

            foreach (Widget w in this.Children) {
                Remove (w);
                w.Dispose ();
            }

            fix = new Fixed ();
            fix.SetSizeRequest (600, 320);

            var saveBtn = new TouchButton ();
            saveBtn.SetSizeRequest (100, 30);
            saveBtn.text = "Save";
            saveBtn.ButtonReleaseEvent += (o, args) => {
                bool success = true;

                if (SaveEvent != null)
                    success = SaveEvent (this);

                if (success)
                    Destroy ();
            };
            fix.Put (saveBtn, 495, 285);
            saveBtn.Show ();

            var cancelButton = new TouchButton ();
            cancelButton.SetSizeRequest (100, 30);
            cancelButton.text = "Cancel";
            cancelButton.ButtonReleaseEvent += (o, args) => {
                Destroy ();
            };
            fix.Put (cancelButton, 385, 285);

            Add (fix);
            fix.Show ();

            settings = new Dictionary<string, SettingsWidget> ();

            Show ();
        }

        protected void DrawSettings () {
            int x = 5;
            int y = 5;

            foreach (var s in settings.Values) {
                fix.Put (s, x, y);
                s.Show ();

                if (x == 5)
                    x = 305;
                else {
                    x = 5;
                    y += 35;
                }

                if (y > 250)
                    throw new Exception ("Too many settings for window");
            }
        }

        public void AddSetting (SettingsWidget w) {
            settings.Add (w.label.text, w);
        }
    }
}

