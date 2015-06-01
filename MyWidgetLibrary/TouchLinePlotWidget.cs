using System;
using Gtk;
using Cairo;

namespace MyWidgetLibrary
{
    public class TouchLinePlotWidget : Fixed
    {
        public string text {
            get { return displayLabel.text; }
            set {
                displayLabel.text = value;
            }
        }

        public float currentValue {
            get {
                return Convert.ToSingle (textBox.text);
            }
            set {
                textBox.text = value.ToString ("0.0");
            }
        }

        private TouchLabel displayLabel;
        private TouchTextBox textBox;

        public TouchLinePlotWidget () {
            SetSizeRequest (447, 95);

            var box1 = new MyBox (447, 95);
            Put (box1, 0, 0);

            displayLabel = new TouchLabel ();
            displayLabel.text = "Plot";
            displayLabel.WidthRequest = 110;
            displayLabel.textColor = "pri";
            Put (displayLabel, 4, 2);

            var box2 = new MyBox (328, 89);
            box2.color = "grey4";
            box2.transparency = 0.85f;
            Put (box2, 116, 3);

            //<TEMP> just here until I get a plot library and plotting done
            var label1 = new TouchLabel ();
            label1.text = "Not Implemented: For line plot";
            label1.WidthRequest = 320;
            Put (label1, 120, 5);

            textBox = new TouchTextBox ();
            textBox.WidthRequest = 80;
            textBox.HeightRequest = 35;
            textBox.textSize = 14;
            textBox.text = "0.0";
            textBox.textAlignment = MyAlignment.Center;
            Put (textBox, 18, 30);

            ShowAll ();
        }
    }
}

