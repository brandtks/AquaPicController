using System;
using System.Collections.Generic;
using Cairo;
using Gtk;
using MyWidgetLibrary;

namespace AquaPic
{
    public class MyMenuBar : EventBox
    {
        private string currentScreen;
        private string highlightedScreen;
        private uint timer;
        private bool menuTouched;

        public MyMenuBar () {
            Visible = true;
            VisibleWindow = false;
            SetSizeRequest (800, 435);

            currentScreen = GuiGlobal.currentScreen;
            highlightedScreen = GuiGlobal.currentScreen;

            ExposeEvent += onExpose;
            ButtonPressEvent += OnButtonPress;
            ButtonReleaseEvent += OnButtonRelease;
        }

        protected void onExpose (object sender, ExposeEventArgs args) {
            using (Context cr = Gdk.CairoHelper.Create (this.GdkWindow)) {
                int x = 0;
                int width = 134;

                foreach (var screen in GuiGlobal.screenData.Values) {

                    if ((GuiGlobal.currentScreen == screen.name) || (menuTouched && (highlightedScreen == screen.name)))
                        cr.Rectangle (x, 435, width, 45);
                    else
                        cr.Rectangle (x, 472, width, 8);

                    screen.color.SetSource (cr);
                    cr.Fill ();

                    x += width;
                }

                Pango.Layout l = new Pango.Layout (this.PangoContext);
                l.Width = Pango.Units.FromPixels (width);
                l.Wrap = Pango.WrapMode.Word;
                l.Alignment = Pango.Alignment.Center;
                l.FontDescription = Pango.FontDescription.FromString ("Courier New 11");

                x = (GuiGlobal.screenData [currentScreen].menuPosition * width) + 1;
                l.SetMarkup ("<span color=\"black\">"
                    + GuiGlobal.screenData [currentScreen].name 
                    + "</span>");
                GdkWindow.DrawLayout (Style.TextGC(StateType.Normal), x, Allocation.Top + 14, l);

                if ((menuTouched) && (currentScreen != highlightedScreen)) {
                    x = (GuiGlobal.screenData [highlightedScreen].menuPosition * width) + 1;
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
                        foreach (var screen in GuiGlobal.screenData.Values) {
                            if (screen.menuPosition == i) {
                                highlightedScreen = screen.name;
                                break;
                            }
                        }
                        QueueDraw ();
                        break;
                    }

                    left += width;
                }
            }

            GuiGlobal.ChangeScreens (highlightedScreen);
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
                            foreach (var screen in GuiGlobal.screenData.Values) {
                                if (screen.menuPosition == i) {
                                    highlightedScreen = screen.name;
                                    break;
                                }
                            }
                            QueueDraw ();
                            break;
                        }

                        left += width;
                    }
                }
            }

            return menuTouched;
        }
    }
}

