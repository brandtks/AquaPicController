using System;
using System.IO;
using Gtk;
using Cairo;

namespace MyWidgetLibrary
{
    public class MyBackgroundWidget : Fixed
    {
        //private Image background;
        private MyNotificationBar notification;
        private TouchButton[] buttons;
        private string[] screenNames;
        public string currentScreen;

        public MyBackgroundWidget (string currentScreen, ButtonReleaseEventHandler OnTouchButtenRelease) {
            this.SetSizeRequest (800, 480);
            this.currentScreen = currentScreen;

            this.screenNames = new string[7] {
                "Main",
                "Power",
                "Lighting",
                "Placeholder",
                "Placeholder",
                "Placeholder",
                "Settings"
            };

            //uncomment for background not save with png
            //            Gdk.Pixbuf display = new Gdk.Pixbuf("images/background2.jpg");
            //            string bpath = "temp", tempname = "temp";
            //            for (int i = 0; File.Exists (tempname); i++)
            //              tempname = bpath + i.ToString ();
            //            display.Save (tempname, "png");
            //            background = new Image (tempname);
            //            Put (background, 0, 0);
            //            background.Show ();
            //            File.Delete (tempname);
            //            Gdk.Pixbuf pic = new Gdk.Pixbuf ("images/background3.png");
            //            this.background = new Image (pic);
            //            this.Put (background, 0, 0);
            //            this.background.Show ();
            //            pic.Dispose ();

            EventBox background = new EventBox ();
            background.Visible = true;
            background.VisibleWindow = false;
            background.SetSizeRequest (800, 480);
            background.ExposeEvent += OnBackGroundExpose;
            this.Put (background, 0, 0);
            background.Show ();

            this.notification = new MyNotificationBar ();
            this.Put (notification, 0, 0);
            this.notification.Show ();

            EventBox menuBar = new EventBox ();
            menuBar.Visible = true;
            menuBar.VisibleWindow = false;
            menuBar.SetSizeRequest (800, 120);
            menuBar.ExposeEvent += OnMenuBarExpose;
            this.Put (menuBar, 0, 380);
            menuBar.Show ();

            this.buttons = new TouchButton[7];
            for (int i = 0; i < buttons.Length; ++i) {
                this.buttons [i] = new TouchButton ();
                this.buttons [i].Text = screenNames [i];
                this.buttons [i].TextColor = "white";
                if (string.Compare (screenNames [i], currentScreen, StringComparison.InvariantCultureIgnoreCase) == 0)
                    this.buttons [i].ButtonColor.ChangeColor ("yellow");
                else
                    this.buttons [i].ButtonColor.ChangeColor ("light grey");
                this.buttons [i].clickAction = ButtonClickAction.Darken;
                this.buttons [i].SetSizeRequest (62, 62);
                this.buttons [i].TouchButtonReleasedHandler += OnTouchButtenRelease;
                this.Put (this.buttons [i], (i * 95) + 83, 396);
                this.buttons [i].Show ();
            }
        }

        protected void OnMenuBarExpose (object sender, ExposeEventArgs args) {
            var box = sender as EventBox;
            using (Context cr = Gdk.CairoHelper.Create (box.GdkWindow)) {
                cr.MoveTo (25, 476);
                cr.LineTo (75, 427);
                cr.LineTo (725, 427);
                cr.LineTo (775, 476);
                cr.LineTo (25, 476);
                cr.ClosePath ();

                Gradient pat = new LinearGradient (400, 427, 400, 476);
                pat.AddColorStop (0.0, new Color (0.45, 0.45, 0.45, 0.85));
                pat.AddColorStop (1.0, new Color (0.35, 0.35, 0.35, 0.55));
                cr.SetSource (pat);
                cr.Fill ();
                pat.Dispose ();
            }
        }

        protected void OnBackGroundExpose (object sender, ExposeEventArgs args) {
            var box = sender as EventBox;
            using (Context cr = Gdk.CairoHelper.Create (box.GdkWindow)) {
                cr.Rectangle (0, 0, 800, 480);

                Gradient pat = new LinearGradient (400, 0, 400, 480);
                pat.AddColorStop (0.0, new Color (0.15, 0.15, 0.15, 1.0));
                pat.AddColorStop (0.5, new Color (0.25, 0.25, 0.25, 1.0));
                pat.AddColorStop (1.0, new Color (0.15, 0.15, 0.15, 1.0));
                cr.SetSource (pat);

                //cr.SetSourceRGB (0.82, 0.85, 0.88);
                cr.Fill ();
                pat.Dispose ();
            }
        }
    }
}

