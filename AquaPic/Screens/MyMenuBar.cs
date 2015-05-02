using System;
using System.Collections.Generic;
using Cairo;
using Gtk;
using MyWidgetLibrary;

namespace AquaPic
{
    public delegate void MenuReleaseHandler (int screenKey);

    public class MyMenuBar : EventBox
    {
        private int currentScreen;
        private int highlightedScreen;
        private uint timer;
        private bool menuTouched;
        private event MenuReleaseHandler MenuReleaseEvent;

        public MyMenuBar (int currentScreen, MenuReleaseHandler OnMenuRelease) {
            this.Visible = true;
            this.VisibleWindow = false;
            this.SetSizeRequest (800, 435);

            this.currentScreen = currentScreen;
            this.highlightedScreen = currentScreen;
            this.MenuReleaseEvent = OnMenuRelease;

            this.ExposeEvent += onExpose;
            this.ButtonPressEvent += OnButtonPress;
            this.ButtonReleaseEvent += OnButtonRelease;
        }

        protected void onExpose (object sender, ExposeEventArgs args) {
            using (Context cr = Gdk.CairoHelper.Create (this.GdkWindow)) {
                int x = 0;
                int width = 133;

                for (int i = 0; i < GuiGlobal.screenData.Count; ++i) {
                    if ((x == 0) || (x == (GuiGlobal.screenData.Count - 1)))
                        width = 134;
                    else
                        width = 133;

                    if ((i == currentScreen) || (menuTouched && (i == highlightedScreen)))
                        cr.Rectangle (x, 435, width, 45);
                    else
                        cr.Rectangle (x, 472, width, 8);
                    
                    GuiGlobal.screenData [i].color.SetSource (cr);
                    cr.Fill ();

                    x += width;
                }
                    
                x = (currentScreen * 133) + 1;

                Pango.Layout l = new Pango.Layout (this.PangoContext);
                l.Width = Pango.Units.FromPixels (width);
                l.Wrap = Pango.WrapMode.Word;
                l.Alignment = Pango.Alignment.Center;
                //l.SetText (ButtonLabel);
                l.SetMarkup ("<span color=\"black\">"
                    + GuiGlobal.screenData [currentScreen].name 
                    + "</span>");
                l.FontDescription = Pango.FontDescription.FromString ("Courier New 11");
                GdkWindow.DrawLayout (Style.TextGC(StateType.Normal), x, Allocation.Top + 14, l);

                if ((menuTouched) && (currentScreen != highlightedScreen)) {
                    x = (highlightedScreen * 133) + 1;
                    l.SetMarkup ("<span color=\"black\">"
                        + GuiGlobal.screenData [highlightedScreen].name 
                        + "</span>");
                    GdkWindow.DrawLayout (Style.TextGC(StateType.Normal), x, Allocation.Top + 14, l);
                }

                l.Dispose ();
            }
        }

        protected void OnButtonPress (object o, ButtonPressEventArgs args) {
            timer = GLib.Timeout.Add (20, OnTimerEvent);
            menuTouched = true;
            QueueDraw ();
        }

        protected void OnButtonRelease (object sender, ButtonReleaseEventArgs args) {
            menuTouched = false;

            int x = (int)args.Event.X;
            int y = (int)args.Event.Y;

            if ((y >= 0) && (y <= Allocation.Height)) {
                int left = 0;
                int width;

                for (int i = 0; i < GuiGlobal.screenData.Count; ++i) {
                    if ((x == 0) || (x == (GuiGlobal.screenData.Count - 1)))
                        width = 134;
                    else
                        width = 133;

                    if ((x >= left) && (x <= (left + width))) {
                        highlightedScreen = i;
                        QueueDraw ();
                        break;
                    }

                    left += width;
                }
            }

            if (MenuReleaseEvent != null)
                MenuReleaseEvent (highlightedScreen);
        }

        protected bool OnTimerEvent () {
            if (menuTouched) {
                int x, y;
                GetPointer (out x, out y);

                if ((y >= 0) && (y <= Allocation.Height)) {
                    int left = 0;
                    int width;

                    for (int i = 0; i < GuiGlobal.screenData.Count; ++i) {
                        if ((x == 0) || (x == (GuiGlobal.screenData.Count - 1)))
                            width = 134;
                        else
                            width = 133;

                        if ((x >= left) && (x <= (left + width))) {
                            highlightedScreen = i;
                            QueueDraw ();
                            break;
                        }

                        left += width;
                    }
                }
            }

            return menuTouched;
        }

        /* old stuff
        private int dialPos;
        private Dictionary<int, string> iconPath;

        //below is part of the menu bar swipe wheel <CURRENTLY DOESN'T WORK>
        //private Timer clickTimer;
        //private bool clicked;
        //private int clickAction, clickX1, clickX2, clickY1, clickY2;

        public MyMenuBar (int dialPos) {
            this.SetSizeRequest (1280, 200);
            this.Visible = true;
            this.VisibleWindow = false;

            this.dialPos = dialPos;

            //removed image and drawing menu bar during expose event
//            Gdk.Pixbuf pic = new Gdk.Pixbuf ("images/MyMenuBar.png");
//            Image img = new Image (pic);
//            this.Child = img;
//            img.Show ();
//            img.Dispose ();

            //below is part of the menu bar swipe wheel <CURRENTLY DOESN'T WORK>
            //this.ButtonPressEvent += onButtonPress;
            //this.clickTimer = new Timer (20);
            //this.clickTimer.Elapsed += onTimerEvent;
            //this.clicked = false;
            //this.clickAction = 0;
            //this.clickX1 = 0;
            //this.clickX2 = 0;
            //this.clickY1 = 0;
            //this.clickY2 = 0;

            this.iconPath = new Dictionary<int, string> () {
                { 0, "images/IconPower.png" },
                { 1, "images/IconCondition.png" },
                { 2, "images/IconLights.png" },
                { 3, "images/IconMain.png" },
                { 4, "images/IconSettings.png" },
                { 5, "images/IconPlaceholder.png" },
                { 6, "images/IconPlaceHolder2.png" } };

            this.ExposeEvent += onExpose;
            this.ButtonReleaseEvent += onButtonRelease;
        }

        protected void onExpose (object sender, ExposeEventArgs args) {
            using (Context cr = Gdk.CairoHelper.Create (this.GdkWindow)) {
                ImageSurface backgroundTop = new ImageSurface ("images/background3_top.png");
                cr.SetSourceSurface (backgroundTop, 0, 0);
                cr.Paint ();
                backgroundTop.Dispose ();

                cr.MoveTo (1280, 0);
                cr.LineTo (1280, 150);
                cr.Arc (640, -3921, 4121, 81.1 * (Math.PI / 180), 98.9 * (Math.PI / 180));
                cr.LineTo (0, 150);
                cr.LineTo (0, 0);
                cr.ClosePath ();
    
                Gradient pat = new RadialGradient (640, -3921, 4121, 640, -450, 450);
                pat.AddColorStop (0, new Color (0.15, 0.15, 0.15, 0.75));
                pat.AddColorStop (0.1, new Color (0.1, 0.1, 0.1, 0.75));
                pat.AddColorStop (0.50, new Color (0.1, 0.1, 0.1, 0.75));
                pat.AddColorStop (0.75, new Color (0.25, 0.25, 0.25, 0.75));
                pat.AddColorStop (1, new Color (0.95, 0.95, 0.95, 0.75));
                cr.SetSource (pat);
                cr.Fill ();

                //testing for just the center icon only
//                Gdk.Pixbuf icon = new Gdk.Pixbuf("images/IconPower.png");
//                icon = icon.ScaleSimple (150, 150, Gdk.InterpType.Bilinear);
//                string bpath = "temp", tempname = "temp";
//                for (int i = 0; File.Exists (tempname); i++)
//                    tempname = bpath + i.ToString ();
//                icon.Save (tempname, "png");

                for (int i = 0; i < iconPath.Count; ++i) {
                    int x = (i * 170) + 80;
                    x += dialPos;
                    if (x > 1100)
                        x -= 1190;
                    Console.WriteLine (x.ToString ());
                    Gdk.Pixbuf icon = new Gdk.Pixbuf(iconPath [i]);
                    icon = icon.ScaleSimple (100, 100, Gdk.InterpType.Bilinear);
                    string bpath = "temp", tempname = "temp";
                    for (int j = 0; File.Exists (tempname); j++)
                        tempname = bpath + j.ToString ();
                    icon.Save (tempname, "png");
                    icon.Dispose ();
                    ImageSurface surface = new ImageSurface (tempname);
                    cr.SetSourceSurface (surface, x, 40);
                    cr.Paint ();
                    surface.Dispose ();
                    File.Delete (tempname);
                }
            }
        }

        protected void onButtonRelease (object sender, ButtonReleaseEventArgs args) {
            // <TODO> determine which icon was pressed
        }*/

        /* below is code for swiping the menu around like a wheel (causes crashes)
         * but drawing the screen too many time crashes the program
         * it can't access the temp file of the resize image
         * maybe if I procedurally draw each icon it will work
         * 
        protected void onButtonPress (object sender, ButtonPressEventArgs args) {
            clickTimer.Enabled = true;
            clicked = true;

            clickX1 = (int)args.Event.X;
            clickY1 = (int)args.Event.Y;
            clickAction = 0;
        }

        protected void onButtonRelease (object sender, ButtonReleaseEventArgs args) {
            clickTimer.Enabled = false;
            clicked = false;

            if (clickAction == 0) {
                if ((args.Event.X <= (clickX1 + 25)) && (args.Event.X >= (clickX1 - 25)) && (args.Event.Y <= (clickY1 + 25)) && (args.Event.Y >= (clickY1 - 25)))
                    clickAction = 100;
            }

            switch (clickAction) {
            case 1:
                Console.WriteLine ("drag right");
                //calculateDialPosition ();
                //QueueDraw ();
                break;
            case -1:
                Console.WriteLine ("drag left");
                //calculateDialPosition ();
                //QueueDraw ();
                break;
            case 100:
                Console.WriteLine ("click release");
                break;
            default:
                break;
            }
        }

        protected void onTimerEvent (object sender, ElapsedEventArgs args) {
            if (clicked) {
                int x, y;
                GetPointer (out x, out y);

                if (clickAction == 0) {
                    if (x > (clickX1 + 25)) {
                        clickAction = 1;
                        clickX2 = x;
                    } else if (x < (clickX1 - 25)) {
                        clickAction = -1;
                        clickX2 = x;
                    }
                } else if ((clickAction == 1) || (clickAction == -1)) {
                    if (x > (clickX1 + 25)) {
                    //if (x > (clickX2 + 170)) {
                        clickAction = 1;
                        //if (x > (clickX2 + 170)) {
                            dialPos -= (clickX1 - clickX2);
                            if (dialPos < 0)
                                dialPos = 1020;
                            if (dialPos > 1020)
                                dialPos = 0;
                            Console.WriteLine ("Click X1 is " + clickX1.ToString ());
                            Console.WriteLine ("Click X2 is " + clickX2.ToString ());
                            Console.WriteLine ("Dial Position is " + dialPos.ToString ());
                            clickX2 = x;
                            //QueueDraw ();
                        //}
                        clickX1 = x;
                    } else if (x < (clickX1 - 25)) {
                    //} else if (x < (clickX2 - 170)) {
                        clickAction = -1;
                        //if (x < (clickX2 - 170)) {
                            dialPos -= (clickX1 - clickX2);
                            if (dialPos < 0)
                                dialPos = 1020;
                            if (dialPos > 1020)
                                dialPos = 0;
                            Console.WriteLine ("Click X1 is " + clickX1.ToString ());
                            Console.WriteLine ("Click X2 is " + clickX2.ToString ());
                            Console.WriteLine ("Dial Position is " + dialPos.ToString ());
                            clickX2 = x;
                            //QueueDraw ();
                        //}
                        clickX1 = x;
                    }
                }
            }
        }

        private void calculateDialPosition () {
            for (int i = 0; i < 7; ++i) {
                int x = (i * 170) + 80;
                if (((x + 85) > dialPos) && ((x - 85) < dialPos)) {
                    dialPos = x + 90;
                    break;
                }
            }
        }
        */
    }
}

