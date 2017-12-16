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
using System.Collections.Generic;
using Cairo;
using Gtk;
using GoodtimeDevelopment.TouchWidget;
using GoodtimeDevelopment.Utilites;
using AquaPic.Globals;

namespace AquaPic.UserInterface
{
    public class MySideBar : EventBox
    {
        private bool clicked, expanded;
        private uint clickTimer;
        private int clickY;
        private int highlighedScreenIndex;
        private double offset, yDeltaPercentage;
        private string[] windows;

        public event EventHandler ExpandEvent;
        public event EventHandler CollapseEvent;

        public MySideBar() 
            : this (AquaPicGui.AquaPicUserInterface.scenes, AquaPicGui.AquaPicUserInterface.currentScene) { }

        public MySideBar (Dictionary<string, SceneData> scenes, string currentScene) {
            Visible = true;
            VisibleWindow = false;

            SetSizeRequest (50, 460);

            ExposeEvent += onExpose;
            ButtonPressEvent += OnButtonPress;
            ButtonReleaseEvent += OnButtonRelease;
            ScrollEvent += OnScollEvent;

            var wins = scenes.Keys;
            windows = new string[wins.Count];
            wins.CopyTo (windows, 0);

            highlighedScreenIndex = Array.IndexOf (windows, currentScene);

            yDeltaPercentage = 0.0;
            offset = 0.0;
        }

        public override void Dispose () {
            if (clickTimer != 0) {
                GLib.Source.Remove (clickTimer);
            }
            base.Dispose ();
        }

        protected void onExpose (object sender, ExposeEventArgs args) {
            using (Context cr = Gdk.CairoHelper.Create (GdkWindow)) {
                int left = Allocation.Left;
                int right = Allocation.Right;
                int top = Allocation.Top;
                int bottom = Allocation.Bottom;
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

                    cr.MoveTo (right + 5, top);
                    cr.LineTo (right - 120, top);
                    cr.ArcNegative (right - 80, top, 40, 0, Math.PI / 2);
                    cr.LineTo (right + 5, top + 40);
                    cr.ClosePath ();
                    TouchColor.SetSource (cr, "grey4", 0.80);
                    cr.Fill ();

                    cr.MoveTo (right + 5, bottom + 5);
                    cr.LineTo (right - 120, bottom + 5);
                    cr.Arc (right - 80, bottom, 40, 0, -Math.PI / 2);
                    cr.LineTo (right + 5, bottom - 40);
                    cr.ClosePath ();
                    TouchColor.SetSource (cr, "grey4", 0.80);
                    cr.Fill ();

                    var t = new TouchText (windows [highlighedScreenIndex]);
                    t.font.color = "pri";
                    t.alignment = TouchAlignment.Right;
                    if (clicked) {
                        t.font.size = 18;
                        double radians = (180 + (offset * 2.75881)).ToRadians ();
                        double textWidth = 210 - CalcX (radius, radians);
                        t.Render (this, left, (height / 2) - 25 + top - (offset * 50.0).ToInt (), textWidth.ToInt (), 50);
                    } else {
                        t.font.size = 20;
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
                        t.font.size = 16 - ((double)i * 0.75).ToInt ();
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

                        t.Render (
                            this, 
                            left, 
                            textY, 
                            textWidth.ToInt (),
                            50);
                    }

                    t.text = "Menu";
                    t.font.size = 18;
                    t.alignment = TouchAlignment.Right;
                    t.Render (this, right - 120, top, 115, 40);

                    t.text = "Home";
                    t.Render (this, right - 120, bottom - 40, 115, 40);
                } else {
                    cr.Arc (originX - 208, originY, radius, 0, 2 * Math.PI);
                    cr.ClosePath ();

                    TouchColor.SetSource (cr, "grey3", 0.75);
                    cr.LineWidth = 0.75;
                    cr.StrokePreserve ();

                    TouchColor.SetSource (cr, "grey2", 0.25);
                    cr.Fill ();

                    TouchGlobal.DrawRoundedRectangle (cr, left - 50, (height / 2) - 30 + top, 85, 60, 20);
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
                int clickX;
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
                        if (y.WithinRange (Allocation.Height / 2 + Allocation.Top, 15)) {
                            if (windows [highlighedScreenIndex] != AquaPicGui.AquaPicUserInterface.currentScene) {
                                var topWidget = this.Toplevel;
                                AquaPicGui.AquaPicUserInterface.ChangeScreens (windows [highlighedScreenIndex], topWidget, AquaPicGui.AquaPicUserInterface.currentScene);
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

                            if (windows [highlighedScreenIndex] != AquaPicGui.AquaPicUserInterface.currentScene) {
                                var topWidget = this.Toplevel;
                                AquaPicGui.AquaPicUserInterface.ChangeScreens (windows [highlighedScreenIndex], topWidget, AquaPicGui.AquaPicUserInterface.currentScene);
                            } else {
                                CollapseMenu ();
                            }
                        }
                    } else {
                        if (x >= 680) {
                            if (y <= 40) {
                                AquaPicGui.AquaPicUserInterface.ChangeScreens ("Menu", Toplevel, AquaPicGui.AquaPicUserInterface.currentScene);
                            } else if (y >= 420) {
                                AquaPicGui.AquaPicUserInterface.ChangeScreens ("Home", Toplevel, AquaPicGui.AquaPicUserInterface.currentScene);
                            } else {
                                CollapseMenu ();
                            }
                        } else if ((x >= 640) && (y >= 420)) {
                            var s = new HomeSettings ();
                            s.Run ();
                            s.Destroy ();
                            s.Dispose ();
                            AquaPicGui.AquaPicUserInterface.ChangeScreens ("Home", Toplevel, AquaPicGui.AquaPicUserInterface.currentScene);
                        } else {
                            CollapseMenu ();
                        }
                    }
                } else {
                    //Timer handles all menu movement so do a little cleanup and draw
                    offset = 0.0;
                    QueueDraw ();
                }
            }
        }

        protected void OnScollEvent (object sender, ScrollEventArgs args) {
            int x = args.Event.X.ToInt ();
            if ((x >= 0) && (x < 250)) {
                if (args.Event.Direction == Gdk.ScrollDirection.Down) {
                    ++highlighedScreenIndex;
                } else if (args.Event.Direction == Gdk.ScrollDirection.Up) {
                    --highlighedScreenIndex;
                }

                if (highlighedScreenIndex >= windows.Length) {
                    highlighedScreenIndex = 0 + (highlighedScreenIndex - windows.Length);
                } else if (highlighedScreenIndex < 0) {
                    highlighedScreenIndex = windows.Length + highlighedScreenIndex;
                }

                QueueDraw ();
            }
        }

        protected void CollapseMenu () {
            expanded = false;
            highlighedScreenIndex = Array.IndexOf (windows, AquaPicGui.AquaPicUserInterface.currentScene);
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
                double oldYDeltaPercentage = yDeltaPercentage;
                yDeltaPercentage = yDelta / 35.0;
                offset = yDeltaPercentage - Math.Floor (yDeltaPercentage + 0.5);

                int oldDeltaPercentageFloor, deltaPercentageFloor;
                if (yDeltaPercentage > oldYDeltaPercentage) {
                    oldDeltaPercentageFloor = Math.Floor (oldYDeltaPercentage - 0.5).ToInt ();
                    deltaPercentageFloor = Math.Floor (yDeltaPercentage - 0.5).ToInt ();
                } else {
                    oldDeltaPercentageFloor = Math.Floor (oldYDeltaPercentage + 0.5).ToInt ();
                    deltaPercentageFloor = Math.Floor (yDeltaPercentage + 0.5).ToInt ();
                }

                if (deltaPercentageFloor != oldDeltaPercentageFloor) {
                    int increment = deltaPercentageFloor - oldDeltaPercentageFloor;

                    highlighedScreenIndex += increment;

                    if (highlighedScreenIndex >= windows.Length) {
                        highlighedScreenIndex = 0;
                    } else if (highlighedScreenIndex < 0) {
                        highlighedScreenIndex = windows.Length - 1;
                    }
                }

                QueueDraw ();
            }

            return clicked;
        }
    }
}

 windows.Length - 1;
                    }
                }

                QueueDraw ();
            }

            return clicked;
        }
    }
}

