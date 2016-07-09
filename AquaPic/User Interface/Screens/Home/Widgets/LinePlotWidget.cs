using System;
using Gtk;
using Cairo;
using TouchWidgetLibrary;

namespace AquaPic.UserInterface
{
    public delegate LinePlotWidget CreateLinePlotHandler (params object[] options);

    public class LinePlotData {
        public CreateLinePlotHandler CreateInstanceEvent;

        public LinePlotData (CreateLinePlotHandler CreateInstanceEvent) {
            this.CreateInstanceEvent = CreateInstanceEvent;
        }

        public LinePlotWidget CreateInstance (params object[] options) {
            if (CreateInstanceEvent != null)
                return CreateInstanceEvent (options);
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

        public UnitsOfMeasurement unitOfMeasurement {
            get {
                return textBox.textRender.unitOfMeasurement;
            }
            set {
                textBox.textRender.unitOfMeasurement = value;
            }
        }

        private TouchLabel displayLabel;
        protected TouchLabel textBox;
        protected TouchLinePlot linePlot;

        public LinePlotWidget () {
            SetSizeRequest (310, 82);

            var box1 = new TouchGraphicalBox (310, 82);
            box1.color = "grey4";
            box1.transparency = 0.1f;
            Put (box1, 0, 0);

            linePlot = new TouchLinePlot ();
            Put (linePlot, 59, 3);

            displayLabel = new TouchLabel ();
            displayLabel.SetSizeRequest (152, 16);
            displayLabel.text = "Plot";
            displayLabel.textColor = "grey3";
            displayLabel.textAlignment = TouchAlignment.Left;
            displayLabel.textHorizontallyCentered = true;
            Put (displayLabel, 3, 63);

            textBox = new TouchLabel ();
            textBox.SetSizeRequest (57, 60);
            textBox.textSize = 19;
            textBox.text = "0.0";
            textBox.textColor = "pri";
            textBox.textAlignment = TouchAlignment.Center;
            textBox.textHorizontallyCentered = true;
            Put (textBox, 1, 3);
            
            ShowAll ();
        }

        public virtual void OnUpdate () {
            throw new Exception ("Update method not implemented");
        }
    }
}

