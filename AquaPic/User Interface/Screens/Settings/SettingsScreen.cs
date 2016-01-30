using System;
using Gtk;
using Cairo;
using TouchWidgetLibrary;
using AquaPic.Runtime;

namespace AquaPic.UserInterface
{
    public class SettingsWindow : WindowBase
    {
        private AquaPicGUI tw;

        public SettingsWindow (params object[] options) : base () {
            var box = new TouchGraphicalBox (110, 395);
            Put (box, 680, 30);
            box.Show ();

            if (options.Length >= 1) {
                tw = options [0] as AquaPicGUI;
                if (tw != null) {
                    if (tw.IsTopLevel) {
                        var b = new TouchButton ();
                        b.SetSizeRequest (100, 30);
                        b.text = "Unfullscreen";
                        b.ButtonReleaseEvent += (o, args) => {
                            tw.ShowDecoration ();
                            #if RPI_BUILD
                            tw.Unfullscreen ();
                            #endif
                        };
                        Put (b, 685, 70);
                        b.Show ();

                        b = new TouchButton ();
                        b.SetSizeRequest (100, 30);
                        b.text = "Back";
                        b.ButtonReleaseEvent += (o, args) => AquaPicGUI.ChangeScreens ("Main");
                        Put (b, 685, 105);
                        b.Show ();
                    } else
                        tw = null;
                }
            }

            var btn = new TouchButton ();
            btn.SetSizeRequest (100, 30);
            btn.text = "Close";
            btn.ButtonReleaseEvent += OnCloseButtonRelease;
            Put (btn, 685, 35);
            btn.Show ();

            Show ();
        }

        public override void Dispose () {
            if (tw != null) {
                if (tw.IsTopLevel) {
                    tw.HideDecoration ();
                    #if RPI_BUILD
                    tw.Fullscreen ();
                    #endif
                }
            }
            base.Dispose ();
        }

        protected void OnCloseButtonRelease (object sender, ButtonReleaseEventArgs args) {
            var parent = this.Toplevel as Gtk.Window;
            if (parent != null) {
                if (!parent.IsTopLevel)
                    parent = null;
            }

            var ms = new TouchDialog ("Are you sure?\nClosing application ends all controller functionality!", parent);

            ms.Response += (o, a) => {
                if (a.ResponseId == ResponseType.Yes)
                    Application.Quit ();
            };

            ms.Run ();
            ms.Destroy ();
        }
    }
}

