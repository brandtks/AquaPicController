using System;
using Cairo;
using Gtk;
using TouchWidgetLibrary;
using AquaPic.Utilites;

namespace AquaPic.UserInterface
{
    public class MySideBar : EventBox
    {
        private bool clicked, expanded;
        private uint clickTimer;
        private int clickX, clickY;
        private int highlighedScreenIndex;
        private double offset, yDeltaPercentage;
        private string[] windows;

        public event EventHandler ExpandEvent;
        public event EventHandler CollapseEvent;

        public MySideBar () {
            Visible = true;
            VisibleWindow = false;

            SetSizeRequest (50, 460);

            ExposeEvent += onExpose;
            ButtonPressEvent += OnButtonPress;
            ButtonReleaseEvent += OnButtonRelease;

            var wins = AquaPicGUI.allWindows.Keys;
            windows = new string[wins.Count];
            wins.CopyTo (windows, 0);

            highlighedScreenIndex = Array.IndexOf (windows, AquaPicGUI.currentScreen);

            yDeltaPercentage = 0.0;
            offset = 0.0;
        }

        protected void onExpose (object sender, ExposeEventArgs args) {
            using (Context cr = Gdk.CairoHelper.Create (this.GdkWindow)) {
                int left = Allocation.Left;
                int top = Allocation.Top;
                int width = Allocation.Width;
                int height = Allocation.Height;

                double originY = (double)top + ((double)height / 2);
                double originX = -850.105;
                double radius = 1075.105;

                if (expanded) {
                    cr.Rectangle (left, top, width, height);
                    TouchColor.SetSource (cr, "grey0", 0.80);
                    cr.Fill ();

                    cr.MoveTo (left, top);
                    cr.LineTo (200, top);
                    cr.Arc (originX, originY, radius, (192.41466).ToRadians (), (167.58534).ToRadians ());
                    cr.LineTo (200, top + height);
                    cr.LineTo (left, top + height);
                    cr.LineTo (left, top);
                    cr.ClosePath ();
                    TouchColor.SetSource (cr, "grey3", 0.80);
                    cr.Fill ();

                    TouchGlobal.DrawRoundedRectangle (cr, left - 50, (height / 2) - 30 + top, 300, 60, 20);
                    TouchColor.SetSource (cr, "pri");
                    cr.LineWidth = 4;
                    cr.StrokePreserve ();
                    TouchColor.SetSource (cr, "grey4");
                    cr.Fill ();

                    var t = new TouchText (windows [highlighedScreenIndex]);
                    t.font.color = "pri";
                    t.alignment = TouchAlignment.Right;
                    if (clicked) {
                        t.font.size = 17;
                        double radians = (180 + (offset * 2.75881)).ToRadians ();
                        double textWidth = 210 - CalcX (radius, radians);
                        t.Render (this, left, (height / 2) - 25 + top - (offset * 50.0).ToInt (), textWidth.ToInt (), 50);
                    } else {
                        t.font.size = 19;
                        t.Render (this, left, (height / 2) - 25 + top, 245, 50);
                    }

                    t.font.color = "black";
                    for (int i = 0; i < 4; ++i) {
                        int drawIndex = highlighedScreenIndex + 1 + i;
                        if (drawIndex >= windows.Length) {
                            drawIndex -= windows.Length;
                        }

                        double radians = (177.24119 - (i * 2.75881) + (offset * 2.75881)).ToRadians ();
                        double textWidth = 210 - CalcX (radius, radians);

                        t.text = windows [drawIndex];
                        t.font.size = 15 - ((double)i * 0.75).ToInt ();
                        int textY = (height / 2) + top + 25 + (i * 50) - (offset * 50.0).ToInt ();

                        t.Render (
                            this, 
                            left, 
                            textY, 
                            textWidth.ToInt (),
                            50);

                        drawIndex = highlighedScreenIndex - 1 - i;
                        if (drawIndex < 0) {
                            drawIndex = windows.Length + drawIndex; //adding drawIndex because its negative
                        }

                        t.text = windows [drawIndex];
                        textY = (height / 2) + top - 75 - (i * 50) - (offset * 50.0).ToInt ();

                        //Console.WriteLine ("{2} radians: {3}, textY: {0}, textWidth {1}", textY, textWidth, t.text, radians);

                        t.Render (
                            this, 
                            left, 
                            textY, 
                            textWidth.ToInt (),
                            50);
                    }
                } else {
                    /*
                    cr.MoveTo (10, originY + 40);
                    cr.ArcNegative (10, originY, 40, Math.PI / 2, -Math.PI / 2);
                    cr.LineTo (10, top + 30);
                    cr.LineTo (left - 5, top + 30);
                    cr.LineTo (left - 5, top + height - 30);
                    cr.LineTo (10, top + height - 30);
                    cr.ClosePath ();

                    TouchColor.SetSource (cr, "pri", 0.75);
                    cr.LineWidth = 0.75;
                    cr.StrokePreserve ();

                    TouchColor.SetSource (cr, "grey3", 0.25);
                    cr.Fill ();
                    */

                    cr.Arc (originX - 208, originY, radius, 0, 2 * Math.PI);
                    cr.ClosePath ();

                    TouchColor.SetSource (cr, "grey3", 0.75);
                    cr.LineWidth = 0.75;
                    cr.StrokePreserve ();

                    TouchColor.SetSource (cr, "grey2", 0.25);
                    cr.Fill ();

                    TouchGlobal.DrawRoundedRectangle (cr, left - 50, (height / 2) - 30 + top, 95, 60, 20);
                    TouchColor.SetSource (cr, "pri");
                    cr.LineWidth = 0.75;
                    cr.StrokePreserve ();
                    TouchColor.SetSource (cr, "grey3");
                    cr.Fill ();
                }
            } 
        }

        protected double CalcX (double radius, double radians) {
            return radius + radius * Math.Cos (radians);
        }

        protected void OnButtonPress (object o, ButtonPressEventArgs args) {
            if (expanded) {
                clicked = true;
                yDeltaPercentage = 0.0;
                offset = 0.0;
                GetPointer (out clickX, out clickY);
                clickTimer = GLib.Timeout.Add (20, OnTimerEvent);
            }
        }

        protected void OnButtonRelease (object sender, ButtonReleaseEventArgs args) {
            clicked = false;
            if (!expanded) {
                expanded = true;
                offset = 0.0;
                SetSizeRequest (800, 460);
                QueueDraw ();

                if (ExpandEvent != null) {
                    ExpandEvent (this, new EventArgs ());
                }
            } else {
                int x, y;
                GetPointer (out x, out y);

                if (y.WithinRange (clickY, 25)) {
                    if (x <= 250) {
                        if (y.WithinRange (Allocation.Height / 2 + Allocation.Top, 25)) {
                            if (windows [highlighedScreenIndex] != AquaPicGUI.currentScreen) {
                                var topWidget = this.Toplevel;
                                AquaPicGUI.ChangeScreens (windows [highlighedScreenIndex], topWidget, AquaPicGUI.currentScreen);
                            } else {
                                CollapseMenu ();
                            }
                        } else {
                            double yDelta = y - ((Allocation.Height / 2) + Allocation.Top);
                            yDeltaPercentage = yDelta / 50.0;

                            int increment = Math.Floor (yDeltaPercentage).ToInt () + 1;

                            highlighedScreenIndex += increment;

                            if (highlighedScreenIndex >= windows.Length) {
                                highlighedScreenIndex = 0 + (highlighedScreenIndex - windows.Length);
                            } else if (highlighedScreenIndex < 0) {
                                highlighedScreenIndex = windows.Length + highlighedScreenIndex;
                            }

                            if (windows [highlighedScreenIndex] != AquaPicGUI.currentScreen) {
                                var topWidget = this.Toplevel;
                                AquaPicGUI.ChangeScreens (windows [highlighedScreenIndex], topWidget, AquaPicGUI.currentScreen);
                            } else {
                                CollapseMenu ();
                            }
                        }
                    } else {
                        CollapseMenu ();
                    }
                } else {
                    //Timer handles all menu movement so do a little cleanup and draw
                    offset = 0.0;
                    QueueDraw ();
                }
            }
        }

        protected void CollapseMenu () {
            expanded = false;
            highlighedScreenIndex = Array.IndexOf (windows, AquaPicGUI.currentScreen);
            SetSizeRequest (50, 460);
            QueueDraw ();

            if (CollapseEvent != null) {
                CollapseEvent (this, new EventArgs ());
            }
        }

        protected bool OnTimerEvent () {
            if (clicked) {
                int x, y;
                GetPointer (out x, out y);

                double yDelta = clickY - y;
                double yDeltaPercentageOld = yDeltaPercentage;
                yDeltaPercentage = yDelta / 35.0;
                offset = yDeltaPercentage - Math.Floor (yDeltaPercentage);

                int oldDeltaPercentageFloor = Math.Floor (yDeltaPercentageOld).ToInt ();
                int deltaPercentageFloor = Math.Floor (yDeltaPercentage).ToInt ();
            
                if (deltaPercentageFloor != oldDeltaPercentageFloor) {
                    int increment = deltaPercentageFloor - oldDeltaPercentageFloor;

                    //Console.Write ("increment: {0}, ", increment);

                    highlighedScreenIndex += increment;

                    if (highlighedScreenIndex >= windows.Length) {
                        highlighedScreenIndex = 0;
                    } else if (highlighedScreenIndex < 0) {
                        highlighedScreenIndex = windows.Length - 1;
                    }
                }

                //Console.WriteLine ("yDelta: {0}, yDeltaPercentage: {1}, offset: {2}", yDelta, yDeltaPercentage, offset);

                QueueDraw ();
            }

            return clicked;
        }
    }
}

