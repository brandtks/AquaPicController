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

            SetSizeRequest (25, 460);

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
            if (expanded) {
                using (Context cr = Gdk.CairoHelper.Create (this.GdkWindow)) {
                    int left = Allocation.Left;
                    int top = Allocation.Top;
                    int width = Allocation.Width;
                    int height = Allocation.Height;

                    double originY = (double)top + ((double)height / 2);
                    double originX = -850.105;
                    double radius = 1075.105;

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

                    cr.Rectangle (left - 5, (height / 2) - 30 + top, 255, 60);
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
                yDeltaPercentage = 0.0;
                offset = 0.0;
                SetSizeRequest (800, 460);
                QueueDraw ();

                if (ExpandEvent != null) {
                    ExpandEvent (this, new EventArgs ());
                }
            } else {
                int x, y;
                GetPointer (out x, out y);

                if (y.WithinRange (clickY, 15)) {
                    if (x <= 250) {
                        if (y.WithinRange (Allocation.Height / 2 + Allocation.Top, 30)) {
                            if (windows [highlighedScreenIndex] != AquaPicGUI.currentScreen) {
                                var topWidget = this.Toplevel;
                                AquaPicGUI.ChangeScreens (windows [highlighedScreenIndex], topWidget);
                            } else {
                                offset = 0.0;
                                QueueDraw ();
                            }
                        } else {
                            double yDelta = y - ((Allocation.Height / 2) + Allocation.Top);
                            yDeltaPercentage = yDelta / 50.0;

                            int increment = Math.Floor (yDeltaPercentage).ToInt () + 1;

                            highlighedScreenIndex += increment;

                            if (highlighedScreenIndex >= windows.Length) {
                                highlighedScreenIndex = 0;
                            } else if (highlighedScreenIndex < 0) {
                                highlighedScreenIndex = windows.Length - 1;
                            }

                            offset = 0.0;
                            QueueDraw ();
                        }
                    } else {
                        expanded = false;
                        SetSizeRequest (25, 460);
                        QueueDraw ();

                        if (CollapseEvent != null) {
                            CollapseEvent (this, new EventArgs ());
                        }
                    }
                } else {
                    //Timer handles all menu movement so do a little cleanup and draw
                    offset = 0.0;
                    QueueDraw ();
                }
            }
        }

        protected bool OnTimerEvent () {
            if (clicked) {
                int x, y;
                GetPointer (out x, out y);

                double yDelta = y - clickY;
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

