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

        public MyBackgroundWidget () {
            this.SetSizeRequest (800, 480);

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

            menu = new MyMenuBar ();
            Put (menu, 0, 435);
            menu.Show ();
        }

        public override void Dispose () {
            foreach (var w in this.Children) {
                w.Dispose ();
            }

            base.Dispose ();
        }

        protected void OnBackGroundExpose (object sender, ExposeEventArgs args) {
            var box = sender as EventBox;
            using (Context cr = Gdk.CairoHelper.Create (box.GdkWindow)) {
                cr.Rectangle (0, 0, 800, 480);

                Gradient pat = new LinearGradient (400, 0, 400, 480);
                pat.AddColorStop (0.0, MyColor.NewGdkColor ("grey0"));
                pat.AddColorStop (0.5, MyColor.NewGdkColor ("grey1"));
                pat.AddColorStop (1.0, MyColor.NewGdkColor ("grey0"));
                cr.SetSource (pat);

                cr.Fill ();
                pat.Dispose ();
            }
        }
    }
}

