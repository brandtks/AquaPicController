using System;
using Gtk;
using Cairo;

namespace MyWidgetLibrary
{
    public class TouchPlot : Fixed
    {
        public string text {
            get { return displayLabel.text; }
            set {
                displayLabel.text = value;
            }
        }

        public float currentValue {
            get {
                return Convert.ToSingle (currentTextBox.text);
            }
            set {
                currentTextBox.text = value.ToString ("0.0");
            }
        }

        private TouchLabel displayLabel;
        private TouchTextBox currentTextBox;

        public TouchPlot () {
            SetSizeRequest (447, 95);

            var box1 = new MyBox (447, 95);
            Put (box1, 0, 0);

            displayLabel = new TouchLabel ();
            displayLabel.text = "Plot";
            displayLabel.textColor = "pri";
            Put (displayLabel, 4, 2);

            var box2 = new MyBox (328, 89);
            box2.color = "grey4";
            box2.transparency = 0.85f;
            Put (box2, 116, 3);

            //<TEMP> just here until I get a plot library and plotting done
            var label1 = new TouchLabel ();
            label1.text = "Not Implemented: For line plot";
            Put (label1, 120, 5);

            currentTextBox = new TouchTextBox ();
            currentTextBox.WidthRequest = 80;
            currentTextBox.HeightRequest = 35;
            currentTextBox.textSize = 16;
            currentTextBox.text = "0.0";
            currentTextBox.textAlignment = Justify.Center;
            Put (currentTextBox, 18, 30);

            ShowAll ();
        }
    }
}

