using System;
using Gtk;
using Cairo;

namespace TouchWidgetLibrary
{
    public class TouchLabel : EventBox
    {
        public TouchText textRender;

        public string text {
            get {
                return textRender.text;
            }
            set {
                textRender.text = value;
            }
        }

        public TouchColor textColor {
            get {
                return textRender.font.color;
            }
            set {
                textRender.font.color = value;
            }
        }

        public int textSize {
            get {
                return textRender.font.size;
            }
            set {
                textRender.font.size = value;
            }
        }

        public TouchAlignment textAlignment {
            get {
                return textRender.alignment;
            }
            set {
                textRender.alignment = value;
            }
        }

        private bool _centered;
        public bool textHorizontallyCentered {
            get {
                return _centered;
            }
            set {
                _centered = value;
            }
        }

        public TouchLabel () {
            this.Visible = true;
            this.VisibleWindow = false;

            textRender = new TouchText (string.Empty);
            _centered = false;

            HeightRequest = 30;
            WidthRequest = 200;

            this.ExposeEvent += OnExpose;
        }

        protected void OnExpose (object sender, ExposeEventArgs args) {
            if (!_centered)
                textRender.Render (this, Allocation.Left, Allocation.Top, Allocation.Width);
            else 
                textRender.Render (this, Allocation.Left, Allocation.Top, Allocation.Width, Allocation.Height);
        }
    }
}

