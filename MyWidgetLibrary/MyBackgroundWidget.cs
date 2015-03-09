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
            this.SetSizeRequest (1280, 800);
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

            EventBox box = new EventBox ();
            box.Visible = true;
            box.VisibleWindow = false;
            box.SetSizeRequest (1280, 800);
            box.ExposeEvent += OnBackGroundExpose;
            this.Put (box, 0, 0);
            box.Show ();

            this.notification = new MyNotificationBar ();
            this.Put (notification, 0, 0);
            this.notification.Show ();

            EventBox eb = new EventBox ();
            eb.Visible = true;
            eb.VisibleWindow = false;
            eb.SetSizeRequest (1280, 120);
            eb.ExposeEvent += onAreaExpose;
            this.Put (eb, 0, 680);
            eb.Show ();

            //            TouchButton main = new TouchButton ();
            //            main.Text = "Main";
            //            main.ButtonColor.changeColor ("light gray");
            //            main.clickAction = ButtonClickAction.Darken;
            //            main.TextColor = "white";
            //            main.SetSizeRequest (100, 100);
            //            this.Put (main, 20, 690);
            //            main.Show ();

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
                this.buttons [i].SetSizeRequest (87, 87);
                this.buttons [i].TouchButtonReleasedHandler += OnTouchButtenRelease;
                this.Put (this.buttons [i], (i * 162) + 111, 688);
                this.buttons [i].Show ();
            }
        }

        protected void onAreaExpose (object sender, ExposeEventArgs args) {
            var box = sender as EventBox;
            using (Context cr = Gdk.CairoHelper.Create (box.GdkWindow)) {
                //cr.Rectangle (0, 680, 1280, 120);
                //cr.SetSourceRGBA (0.15, 0.15, 0.15, 0.65);

                cr.MoveTo (50, 796);
                cr.LineTo (100, 735);
                cr.LineTo (1180, 735);
                cr.LineTo (1230, 796);
                cr.LineTo (50, 796);
                cr.ClosePath ();

                Gradient pat = new LinearGradient (640, 735, 640, 800);
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
                cr.Rectangle (0, 0, 1280, 800);

                Gradient pat = new LinearGradient (640, 0, 640, 800);
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

