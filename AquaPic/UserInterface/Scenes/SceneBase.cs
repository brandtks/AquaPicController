#region License

/*
    AquaPic Main Control - Handles all functionality for the AquaPic aquarium controller.

    Copyright (c) 2017 Goodtime Development

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see http://www.gnu.org/licenses/
*/

#endregion // License

using System;
using System.IO;
using Cairo;
using Gtk;
using GoodtimeDevelopment.TouchWidget;

namespace AquaPic.UserInterface
{
    public class SceneBase : Fixed
    {
        //Image background;
        TouchLabel label;
        protected uint timerId;

        public string sceneTitle {
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

        public SceneBase (bool addTimer = true) {
            SetSizeRequest (800, 416);

            label = new TouchLabel ();
            label.text = "NO TITLE";
            label.textSize = 14;
            label.textColor = "pri";
            label.WidthRequest = 700;
            label.textAlignment = TouchAlignment.Center;
            Put (label, 50, 37);
            label.Show ();
            
            if (addTimer) {
                timerId = GLib.Timeout.Add (1000, OnUpdateTimer);
            }

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

        protected virtual bool OnUpdateTimer () {
            return false;
        }

        public override void Dispose () {
            GLib.Source.Remove (timerId);
            foreach (var w in Children) {
                w.Dispose ();
            }
            base.Dispose ();
        }
    }
}

