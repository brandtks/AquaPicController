using System;
using System.IO;
using Cairo;
using Gtk;
using TouchWidgetLibrary;

namespace AquaPic.UserInterface
{
    public class WindowBase : Fixed
    {
        //private Image background;
        private TouchLabel label;

        public string screenTitle {
            get {
                return label.text;
            }
            set {
                label.text = value;
            }
        }

        public bool showTitle {
            get {
                return label.Visible;
            }
            set {
                label.Visible = value;
            }
        }

        public WindowBase () {
            SetSizeRequest (800, 416);

            label = new TouchLabel ();
            label.text = "NO TITLE";
            label.textSize = 14;
            label.textColor = "pri";
            label.WidthRequest = 700;
            label.textAlignment = TouchAlignment.Center;
            Put (label, 50, 37);
            label.Show ();

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

