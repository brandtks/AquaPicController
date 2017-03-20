#region License

/*
    AquaPic Main Control - Handles all functionality for the AquaPic aquarium controller.

    Copyright (c) 2017 Skyler Brandt

    This program is free software; you can redistribute it and/or
    modify it under the terms of the GNU General Public License
    as published by the Free Software Foundation; either version 2
    of the License, or (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program; if not, write to the Free Software
    Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.

    Optionally you can also view the license at <http://www.gnu.org/licenses/>.
*/

#endregion // License

﻿using System;
using Gtk;
using Cairo;
using AquaPic.Utilites;

namespace TouchWidgetLibrary
{
    public delegate void ProgressChangeEventHandler (object sender, ProgressChangeEventArgs args);

    public class ProgressChangeEventArgs : EventArgs
    {
        public float currentProgress;

        public ProgressChangeEventArgs (float currentProgress) {
            this.currentProgress = currentProgress;
        }
    }

    public class TouchProgressBar : EventBox
    {
        protected TouchOrientation _orient;
        private float _currentProgress;
        public float currentProgress {
            get {
                return _currentProgress;
            }
            set {
                if (value < 0.0f)
                    _currentProgress = 0.0f;
                else if (value > 1.0f)
                    _currentProgress = 1.0f;
                else
                    _currentProgress = value;
            }
        }

        public TouchColor colorBackground;
        public TouchColor colorProgress;
        public bool enableTouch;

        public event ProgressChangeEventHandler ProgressChangedEvent;
        public event ProgressChangeEventHandler ProgressChangingEvent;

        private uint timerId;
        private bool clicked;

        public TouchProgressBar (
            TouchColor colorBackground, 
            TouchColor colorProgress,
            float currentProgress, 
            bool enableTouch, 
            TouchOrientation orientation
        ) {
            this.Visible = true;
            this.VisibleWindow = false;

            this.colorBackground = colorBackground;
            this.colorProgress = colorProgress;
            this._currentProgress = currentProgress;
            this.enableTouch = enableTouch;
            _orient = orientation;
            timerId = 0;
            clicked = false;

            if (_orient == TouchOrientation.Vertical) {
                SetSizeRequest (30, 200);
            } else {
                SetSizeRequest (200, 30);
            }

            ExposeEvent += OnExpose;
            ButtonPressEvent += OnProgressBarPress;
            ButtonReleaseEvent += OnProgressBarRelease;
        }

        public override void Dispose () {
            if (timerId != 0) {
                GLib.Source.Remove (timerId);
            }
            base.Dispose ();
        }

        public TouchProgressBar () 
            : this (new TouchColor ("grey4"), new TouchColor ("pri"), 0.0f, false, TouchOrientation.Vertical) { 
        }

        public TouchProgressBar (TouchOrientation orientation)
            : this (new TouchColor ("grey4"), new TouchColor ("pri"), 0.0f, false, orientation) {
        }

        protected void OnExpose (object sender, ExposeEventArgs args) {
            using (Context cr = Gdk.CairoHelper.Create (this.GdkWindow)) {
                int left = Allocation.Left;
                int top = Allocation.Top;
                int width = Allocation.Width;
                int height = Allocation.Height;
                int radius, bottom = top + height;
                double difference;

                if (_orient == TouchOrientation.Vertical) {
                    radius = width / 2;

                    TouchGlobal.DrawRoundedRectangle (cr, left, top, width, height, radius);
                    colorBackground.SetSource (cr);
                    cr.Fill ();

                    difference = (height - width) * _currentProgress;
                    top += (height - width - difference.ToInt ());

                    cr.MoveTo (left, bottom - radius);
                    cr.ArcNegative (left + radius, bottom - radius, radius, (180.0).ToRadians (), (0.0).ToRadians ());
                    cr.LineTo (left + width, top + radius);
                    cr.ArcNegative (left + radius, top + radius, radius, (0.0).ToRadians (), (180.0).ToRadians ());
                    cr.ClosePath ();
                    colorProgress.SetSource (cr);
                    cr.Fill ();
                } else {
                    radius = height / 2;

                    TouchGlobal.DrawRoundedRectangle (cr, left, top, width, height, radius);
                    colorBackground.SetSource (cr);
                    cr.Fill ();

                    difference = ((width - height) * _currentProgress);

                    cr.MoveTo (left + radius, top);
                    cr.ArcNegative (left + radius, top + radius, radius, (-90.0).ToRadians (), (90.0).ToRadians ());
                    cr.LineTo (left + difference + height - radius, bottom);
                    cr.ArcNegative (left + difference + height - radius, top + radius, radius, (90.0).ToRadians (), (-90.0).ToRadians ());
                    cr.ClosePath ();
                    colorProgress.SetSource (cr);
                    cr.Fill ();
                }
            }
        }

        protected void OnProgressBarPress (object o, ButtonPressEventArgs args) {
            if (enableTouch) {
                timerId = GLib.Timeout.Add (20, OnTimerEvent);
                clicked = true;
            }
        }

        protected void OnProgressBarRelease (object o, ButtonReleaseEventArgs args) {
            clicked = false;

            if (ProgressChangedEvent != null)
                ProgressChangedEvent (this, new ProgressChangeEventArgs (_currentProgress));
        }

        protected bool OnTimerEvent () {
            if (clicked) {
                int x, y;
                GetPointer (out x, out y);
                if (_orient == TouchOrientation.Vertical)
                    _currentProgress = (float)(Allocation.Height - y) / (float)Allocation.Height;
                else
                    _currentProgress = (float)x / (float)Allocation.Width;

                if (_currentProgress > 1.0f)
                    _currentProgress = 1.0f;
                if (_currentProgress < 0.0f)
                    _currentProgress = 0.0f;

                if (ProgressChangingEvent != null)
                    ProgressChangingEvent (this, new ProgressChangeEventArgs (_currentProgress));

                QueueDraw ();
            }

            return clicked;
        }
    }
}

