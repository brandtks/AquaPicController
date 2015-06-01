using System;
using Gtk;
using Cairo;

namespace MyWidgetLibrary
{
    public class TouchBarPlotWidget : Fixed
    {
        public string text {
            get { return label.text; }
            set {
                label.text = value;
            }
        }

        public float currentValue {
            get {
                return bar.currentProgress;
            }
            set {
                bar.currentProgress = value;
                textBox.text = value.ToString ("0.0");
            }
        }

        private TouchProgressBar bar;
        private TouchTextBox textBox;
        private TouchLabel label;

        public TouchBarPlotWidget () {
            SetSizeRequest (108, 195);

            var box = new MyBox (108, 195);
            Put (box, 0, 0);

            label = new TouchLabel ();
            label.text = "Plot";
            label.textColor = "pri";
            label.textRender.orientation = MyOrientation.Vertical;
            Put (label, 4, 2);

            bar = new TouchProgressBar ();
            bar.HeightRequest = 185;
            Put (bar, 73, 5);

            textBox = new TouchTextBox ();
        }

    }
}

