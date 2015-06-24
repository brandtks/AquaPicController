using System;
using Gtk;
using Cairo;
using MyWidgetLibrary;

namespace AquaPic
{
    public delegate LinePlotWidget CreateLinePlotHandler ();

    public class LinePlotData {
        public CreateLinePlotHandler CreateInstanceEvent;

        public LinePlotData (CreateLinePlotHandler CreateInstanceEvent) {
            this.CreateInstanceEvent = CreateInstanceEvent;
        }

        public LinePlotWidget CreateInstance () {
            if (CreateInstanceEvent != null)
                return CreateInstanceEvent ();
            else
                throw new Exception ("No line plot constructor implemented");
        }
    }

    public class LinePlotWidget : Fixed
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

        public LinePlotWidget () {
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

            //<TEMPORARY> just here until I get a plot library and data logging
            var label1 = new TouchLabel ();
            label1.text = "Not Implemented: For line plot";
            label1.textColor = "black";
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

        public virtual void OnUpdate () {
            throw new Exception ("Update method not implemented");
        }
    }
}

