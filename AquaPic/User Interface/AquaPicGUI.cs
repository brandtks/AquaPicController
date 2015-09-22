using System;
using System.Diagnostics;
using Cairo;
using Gtk;
using MyWidgetLibrary;
using AquaPic.Runtime;

namespace AquaPic.UserInterface
{
    public partial class AquaPicGUI : Gtk.Window
    {
        WindowBase current;
        Fixed f;
        MyMenuBar menu;

        public AquaPicGUI () : base (Gtk.WindowType.Toplevel) {
            this.Name = "AquaPic.GUI";
            this.Title = global::Mono.Unix.Catalog.GetString ("AquaPic Controller Version 1");
            this.WindowPosition = ((global::Gtk.WindowPosition)(4));
            this.DefaultWidth = 800;
            this.DefaultHeight = 480;
            this.DeleteEvent += new global::Gtk.DeleteEventHandler (this.OnDeleteEvent);
            this.Resizable = false;
            this.AllowGrow = false;

            this.ModifyBg (StateType.Normal, MyColor.NewGtkColor ("grey0"));

            #if RPI_BUILD
            this.Decorated = false;
            //this.Fullscreen ();
            #endif

            GLib.ExceptionManager.UnhandledException += (args) => {
                Exception ex = args.ExceptionObject as Exception;
                Logger.AddError (ex.ToString ());
                args.ExitApplication = false;
            };

            GuiGlobal.ChangeScreenEvent += ScreenChange;

            GuiGlobal.currentSelectedMenu = GuiGlobal.menuWindows [0];
            GuiGlobal.currentScreen = GuiGlobal.currentSelectedMenu;

            f = new Fixed ();
            f.SetSizeRequest (800, 480);

//            var background = new EventBox ();
//            background.Visible = true;
//            background.VisibleWindow = false;
//            background.SetSizeRequest (800, 480);
//            background.ExposeEvent += OnBackGroundExpose;
//            f.Put (background, 0, 0);
//            background.Show ();

            var notification = new MyNotificationBar ();
            f.Put (notification, 0, 0);
            notification.Show ();

            menu = new MyMenuBar ();
            f.Put (menu, 0, 435);
            menu.Show ();

            current = GuiGlobal.allWindows [GuiGlobal.currentScreen].CreateInstance ();
            f.Put (current, 0, 0);
            current.Show ();

            Add (f);
            f.Show ();

            this.Show ();
        }

        protected void OnDeleteEvent (object sender, DeleteEventArgs a) {
            Application.Quit ();
            a.RetVal = true;
        }

        public void ScreenChange (ScreenData screen, params object[] options) {
            f.Remove (current);
            current.Destroy ();
            current.Dispose ();
            current = screen.CreateInstance (options);
            f.Put (current, 0, 0);
            menu.UpdateScreens ();
            QueueDraw ();
        }

        protected void OnBackGroundExpose (object sender, ExposeEventArgs args) {
            var box = sender as EventBox;
            using (Context cr = Gdk.CairoHelper.Create (box.GdkWindow)) {
                cr.Rectangle (0, 0, 800, 480);

                Gradient pat = new LinearGradient (400, 0, 400, 480);
                pat.AddColorStop (0.0, MyColor.NewCairoColor ("grey0"));
                pat.AddColorStop (0.5, MyColor.NewCairoColor ("grey1"));
                pat.AddColorStop (1.0, MyColor.NewCairoColor ("grey0"));
                cr.SetSource (pat);

                cr.Fill ();
                pat.Dispose ();
            }
        }

        public void ShowDecoration () {
            f.Move (menu, 0, 380);
            SetSizeRequest (800, 425);
            Decorated = true;
        }

        public void HideDecoration () {
            f.Move (menu, 0, 435);
            SetSizeRequest (800, 480);
            #if RPI_BUILD
            Decorated = false;
            #endif
        }
    }
}

