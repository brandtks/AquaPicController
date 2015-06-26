using System;
using System.IO;
using Cairo;
using Gtk;
using MyWidgetLibrary;

namespace AquaPic
{
    public class WindowBase : Fixed
    {
//        private Image background;

        public WindowBase () {
            SetSizeRequest (800, 416);

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

        }

        public override void Dispose () {
            foreach (var w in this.Children) {
                w.Dispose ();
            }

            base.Dispose ();
        }
    }
}

