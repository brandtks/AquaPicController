﻿using System;
using System.IO;
using Gtk;
using Cairo;

namespace MyWidgetLibrary
{
    public class MyBackgroundWidget : Fixed
    {
        private Image background;
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

            Gdk.Pixbuf pic = new Gdk.Pixbuf ("images/background3.png");
            this.background = new Image (pic);
            this.Put (background, 0, 0);
            this.background.Show ();
            pic.Dispose ();

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
                if (string.Compare (screenNames [i], currentScreen, StringComparison.InvariantCultureIgnoreCase) == 0) {
                    this.buttons [i].ButtonColor.ChangeColor ("yellow");
                    this.buttons [i].TextColor = "black";
                } else {
                    this.buttons [i].ButtonColor.ChangeColor ("light gray");
                    this.buttons [i].TextColor = "white";
                }
                this.buttons [i].clickAction = ButtonClickAction.Darken;
                this.buttons [i].SetSizeRequest (150, 100);
                this.buttons [i].TouchButtonReleasedHandler += OnTouchButtenRelease;
                this.Put (this.buttons [i], (i * 177) + 30, 690);
                this.buttons [i].Show ();
            }
        }

        protected void onAreaExpose (object sender, ExposeEventArgs args) {
            EventBox eb = (EventBox)sender;
            using (Context cr = Gdk.CairoHelper.Create (eb.GdkWindow)) {
                cr.Rectangle (0, 680, 1280, 120);
                cr.SetSourceRGBA (0.15, 0.15, 0.15, 0.65);
                cr.Fill ();
            }
        }
    }
}

