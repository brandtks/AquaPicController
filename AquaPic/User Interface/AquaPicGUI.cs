using System;
using System.Diagnostics;
using Cairo;
using Gtk;
using TouchWidgetLibrary;
using AquaPic.Runtime;
using AquaPic.SerialBus;
using Gdk;

namespace AquaPic.UserInterface
{
    public partial class AquaPicGUI : Gtk.Window
    {
        WindowBase current;
        Fixed f;
        MySideBar side;
        MyNotificationBar notification;

        public AquaPicGUI () : base (Gtk.WindowType.Toplevel) {
            Name = "AquaPicGUI";
            Title = "AquaPic Controller Version 1";
            WindowPosition = WindowPosition.Center;
            SetSizeRequest (800, 480); 
            Resizable = false;
            AllowGrow = false;

            DeleteEvent += (o, args) => {
                Application.Quit ();
                args.RetVal = true;
            };

            ModifyBg (StateType.Normal, TouchColor.NewGtkColor ("grey0"));

#if RPI_BUILD
            this.Decorated = false;
            this.Fullscreen ();
#endif
            
            GLib.ExceptionManager.UnhandledException += (args) => {
                Exception ex = args.ExceptionObject as Exception;
                Logger.AddError (ex.ToString ());
                args.ExitApplication = false;
            };

            ChangeScreenEvent += ScreenChange;

            currentScreen = "Home";

            f = new Fixed ();
            f.SetSizeRequest (800, 480);

            current = allWindows [currentScreen].CreateInstance ();
            f.Put (current, 0, 0);
            current.Show ();

            side = new MySideBar ();
            f.Put (side, 0, 20);
            side.Show ();

            notification = new MyNotificationBar ();
            f.Put (notification, 0, 0);
            notification.Show ();

            Add (f);
            f.Show ();

            Show ();
        }

        public void ScreenChange (ScreenData screen, params object[] options) {
            f.Remove (current);
            current.Destroy ();
            current.Dispose ();
            current = screen.CreateInstance (options);
            f.Put (current, 0, 0);

            f.Remove (side);
            side.Destroy ();
            side.Dispose ();
            side = new MySideBar ();
            f.Put (side, 0, 20);
            side.Show ();

            if (currentScreen == "Logger") {
                var logScreen = current as LoggerWindow;
                if (logScreen != null) {
                    side.ExpandEvent += (sender, e) => {
                        logScreen.tv.Visible = false;
                        logScreen.tv.QueueDraw ();
                    };

                    side.CollapseEvent += (sender, e) => {
                        logScreen.tv.Visible = true;
                        logScreen.tv.QueueDraw ();
                    };
                }
            } else if (currentScreen == "Alarms") {
                var alarmScreen = current as AlarmWindow;
                if (alarmScreen != null) {
                    side.ExpandEvent += (sender, e) => {
                        alarmScreen.tv.Visible = false;
                        alarmScreen.tv.QueueDraw ();
                    };

                    side.CollapseEvent += (sender, e) => {
                        alarmScreen.tv.Visible = true;
                        alarmScreen.tv.QueueDraw ();
                    };
                }

            }

            f.Remove (notification);
            notification.Destroy ();
            notification.Dispose ();
            notification = new MyNotificationBar ();
            f.Put (notification, 0, 0);
            notification.Show ();

            QueueDraw ();
        }

        public void ShowDecoration () {
            SetSizeRequest (800, 425);
            Decorated = true;
        }

        public void HideDecoration () {
            SetSizeRequest (800, 480);
#if RPI_BUILD
            Decorated = false;
#endif
        }
    }
}

