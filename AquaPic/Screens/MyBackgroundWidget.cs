using System;
using System.IO;
using Cairo;
using Gtk;
using MyWidgetLibrary;

namespace AquaPic
{
    public class MyBackgroundWidget : Fixed
    {
//        private Image background;
        private MyNotificationBar notification;
        private MyMenuBar menu;
//        private TouchButton[] buttons;
        public int currentScreen;

        public MyBackgroundWidget (int currentScreen, MenuReleaseHandler OnMenuRelease) {
            this.SetSizeRequest (800, 480);
            this.currentScreen = currentScreen;

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

            this.notification = new MyNotificationBar (currentScreen);
            this.Put (notification, 0, 0);
            this.notification.Show ();

            menu = new MyMenuBar (currentScreen, OnMenuRelease);
            Put (menu, 0, 435);
            menu.Show ();

//            EventBox menuBar = new EventBox ();
//            menuBar.Visible = true;
//            menuBar.VisibleWindow = false;
//            menuBar.SetSizeRequest (800, 120);
//            menuBar.ExposeEvent += OnMenuBarExpose;
//            this.Put (menuBar, 0, 380);
//            menuBar.Show ();

//            this.buttons = new TouchButton[6];
//            for (int i = 0; i < buttons.Length; ++i) {
//                this.buttons [i] = new TouchButton ();
//                this.buttons [i].Text = GuiGlobal.screenData [i].name;
//                this.buttons [i].TextColor.ChangeColor ("white");
//                if (i == currentScreen) {
//                    this.buttons [i].ButtonColor.ChangeColor ("pri");
//                    this.buttons [i].clickAction = ButtonClickAction.Brighten;
//                } else {
//                    this.buttons [i].ButtonColor.ChangeColor ("grey4");
//                    this.buttons [i].clickAction = ButtonClickAction.Darken;
//                }
//                this.buttons [i].SetSizeRequest (62, 62);
//                this.buttons [i].TouchButtonReleasedHandler += OnTouchButtenRelease;
//                this.Put (this.buttons [i], (i * 114) + 84, 396);
//                this.buttons [i].Show ();
//            }
        }

//        protected void OnMenuBarExpose (object sender, ExposeEventArgs args) {
//            var box = sender as EventBox;
//            using (Context cr = Gdk.CairoHelper.Create (box.GdkWindow)) {
//                cr.MoveTo (25, 476);
//                cr.LineTo (75, 427);
//                cr.LineTo (725, 427);
//                cr.LineTo (775, 476);
//                cr.LineTo (25, 476);
//                cr.ClosePath ();
//
//                Gradient pat = new LinearGradient (400, 427, 400, 476);
//                pat.AddColorStop (0.0, MyColor.NewColor ("grey3", 0.75));
//                pat.AddColorStop (1.0, MyColor.NewColor ("grey2", 0.50));
//                //pat.AddColorStop (1.0, MyColor.NewColor ("seca", 0.50));
//                cr.SetSource (pat);
//                cr.Fill ();
//                pat.Dispose ();
//            }
//        }

        protected void OnBackGroundExpose (object sender, ExposeEventArgs args) {
            var box = sender as EventBox;
            using (Context cr = Gdk.CairoHelper.Create (box.GdkWindow)) {
                cr.Rectangle (0, 0, 800, 480);

                Gradient pat = new LinearGradient (400, 0, 400, 480);
                pat.AddColorStop (0.0, MyColor.NewGdkColor ("grey0"));
                pat.AddColorStop (0.5, MyColor.NewGdkColor ("grey1"));
                pat.AddColorStop (1.0, MyColor.NewGdkColor ("grey0"));
                cr.SetSource (pat);

                //cr.SetSourceRGB (0.82, 0.85, 0.88);
                cr.Fill ();
                pat.Dispose ();
            }
        }
    }
}

