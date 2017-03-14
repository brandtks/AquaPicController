using System;
using System.Collections.Generic;
using Cairo;
using Gtk;
using TouchWidgetLibrary;

namespace AquaPic.UserInterface
{
    public delegate bool SaveHandler (object sender);

    public enum TouchSettingsOutcome {
        Added,
        Modified,
        Deleted,
        Cancelled
    }
    
    public class TouchSettingsDialog : Dialog
    {
        public Fixed fix;
        public event SaveHandler SaveEvent;
        public event SaveHandler DeleteButtonEvent;
        public Dictionary<string, SettingsWidget> settings;
        public bool includeDelete;
        public bool showOptional;
        public TouchSettingsOutcome outcome;

        public TouchButton saveBtn;
        public TouchButton cancelButton;
        public TouchButton deleteButton;

        public TouchSettingsDialog (string name) : this (name, false, 320) { }

        public TouchSettingsDialog (string name, bool includeDelete) : this (name, includeDelete, 320) { }

        public TouchSettingsDialog (string name, bool includeDelete, int height) {
            Name = "AquaPic.Settings." + name;
            Title = name + " Settings";
            WindowPosition = (WindowPosition)4;
            SetSizeRequest (600, height);

#if RPI_BUILD
            Decorated = false;

            ExposeEvent += (o, args) => {
                using (Context cr = Gdk.CairoHelper.Create (GdkWindow)) {
                    cr.MoveTo (Allocation.Left, Allocation.Top);
                    cr.LineTo (Allocation.Right, Allocation.Top);
                    cr.LineTo (Allocation.Right, Allocation.Bottom);
                    cr.LineTo (Allocation.Left, Allocation.Bottom);
                    cr.ClosePath ();
                    cr.LineWidth = 1.8;
                    TouchColor.SetSource (cr, "grey4");
                    cr.Stroke ();
                }
            };
#endif

            ModifyBg (StateType.Normal, TouchColor.NewGtkColor ("grey0"));

            foreach (Widget w in this.Children) {
                Remove (w);
                w.Dispose ();
            }

            fix = new Fixed ();
            fix.SetSizeRequest (600, height);

            saveBtn = new TouchButton ();
            saveBtn.SetSizeRequest (100, 30);
            saveBtn.text = "Save";
            saveBtn.ButtonReleaseEvent += (o, args) => {
                bool success = true;

                if (SaveEvent != null)
                    success = SaveEvent (this);

                if (success) {
                    if (this.includeDelete) {
                        outcome = TouchSettingsOutcome.Modified;
                    } else {
                        outcome = TouchSettingsOutcome.Added;
                    }

                    Destroy ();
                }
            };
            fix.Put (saveBtn, 495, height - 35);
            saveBtn.Show ();

            cancelButton = new TouchButton ();
            cancelButton.SetSizeRequest (100, 30);
            cancelButton.text = "Cancel";
            cancelButton.ButtonReleaseEvent += (o, args) => {
                outcome = TouchSettingsOutcome.Cancelled;
                Destroy ();
            };
            fix.Put (cancelButton, 385, height - 35);
            cancelButton.Show ();

            this.includeDelete = includeDelete;
            if (this.includeDelete) {
                deleteButton = new TouchButton ();
                deleteButton.SetSizeRequest (100, 30);
                deleteButton.text = "Delete";
                deleteButton.buttonColor = "compl";
                deleteButton.ButtonReleaseEvent += (obj, args) => {
                    var parent = this.Toplevel as Gtk.Window;
                    if (parent != null) {
                        if (!parent.IsTopLevel)
                            parent = null;
                    }

                    var ms = new TouchDialog ("Are you sure you with to delete " + name, parent);

                    ms.Response += (o, a) => {
                        if (a.ResponseId == ResponseType.Yes) {
                            bool success = true;

                            if (DeleteButtonEvent != null)
                                success = DeleteButtonEvent (this);

                            if (success) {
                                outcome = TouchSettingsOutcome.Deleted;
                                Destroy ();
                            }
                        }
                    };
                    
                    ms.Run ();
                    ms.Destroy ();
                };
                fix.Put (deleteButton, 10, height - 35);
                deleteButton.Show ();
            }

            Add (fix);
            fix.Show ();

            settings = new Dictionary<string, SettingsWidget> ();

            showOptional = true;

            Show ();
        }

        public override void Destroy () {
            base.Destroy ();
            Dispose ();
        }

        protected void DrawSettings () {
            int x = 5;
            int y = 5;
            
            foreach (var s in settings.Values) {
                fix.Put (s, x, y);

                if (x == 5)
                    x = 305;
                else {
                    x = 5;
                    y += 35;
                }

                if (y > 250)
                    throw new Exception ("Too many settings for window");
            }

            UpdateSettingsVisibility ();
        }

        protected void UpdateSettingsVisibility () {
            foreach (var s in settings.Values) {
                if (s.optionalSetting && showOptional)
                    s.Visible = true;
                else if (!s.optionalSetting)
                    s.Visible = true;
                else
                    s.Visible = false;

                s.QueueDraw ();
            }
        }

        protected void AddSetting (SettingsWidget w) {
            settings.Add (w.label.text, w);
        }

        protected void AddOptionalSetting (SettingsWidget w) {
            w.optionalSetting = true;
            settings.Add (w.label.text, w);
        }
    }
}

