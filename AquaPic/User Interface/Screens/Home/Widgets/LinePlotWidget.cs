using System;
using Gtk;
using Cairo;
using TouchWidgetLibrary;

namespace AquaPic.UserInterface
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
            get { 
                return displayLabel.text; 
            }
            set {
                displayLabel.text = value;
            }
        }

        public float currentValue {
            get {
                return Convert.ToSingle (textBox.text);
            }
            set {
                textBox.text = value.ToString ("F1");
            }
        }

        private TouchLabel displayLabel;
        private TouchLabel textBox;

        public LinePlotWidget () {
            SetSizeRequest (415, 82);

            var box1 = new TouchGraphicalBox (415, 82);
            box1.color = "grey4";
            box1.transparency = 0.1f;
            Put (box1, 0, 0);

            displayLabel = new TouchLabel ();
            displayLabel.text = "Plot";
            displayLabel.WidthRequest = 110;
            displayLabel.textColor = "grey3";
            displayLabel.textAlignment = TouchAlignment.Center;
            Put (displayLabel, 3, 50);

            textBox = new TouchLabel ();
            textBox.WidthRequest = 110;
            textBox.textSize = 20;
            textBox.text = "0.0";
            textBox.textColor = "pri";
            textBox.textAlignment = TouchAlignment.Center;
            Put (textBox, 3, 15);

            var box2 = new TouchGraphicalBox (296, 76);
            box2.color = "grey3";
            box2.transparency = 0.15f;
            Put (box2, 116, 3);

            //<TEMPORARY> just here until I get a plot library and data logging
            var label1 = new TouchLabel ();
            label1.text = "Not Implemented: For line plot";
            label1.textColor = "white";
            label1.WidthRequest = 296;
            label1.textSize = 10;
            label1.textAlignment = TouchAlignment.Right;
            Put (label1, 116, 62);
            
            ShowAll ();
        }

        public virtual void OnUpdate () {
            throw new Exception ("Update method not implemented");
        }
    }
}

