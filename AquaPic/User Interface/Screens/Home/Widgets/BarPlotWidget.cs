using System;
using Gtk;
using Cairo;
using TouchWidgetLibrary;

namespace AquaPic.UserInterface
{
    public delegate BarPlotWidget CreateBarPlotHandler ();

    public class BarPlotData {
        public CreateBarPlotHandler CreateInstanceEvent;

        public BarPlotData (CreateBarPlotHandler CreateInstanceEvent) {
            this.CreateInstanceEvent = CreateInstanceEvent;
        }

        public BarPlotWidget CreateInstance () {
            if (CreateInstanceEvent != null)
                return CreateInstanceEvent ();
            else
                throw new Exception ("No bar plot constructor implemented");
        }
    }

    public class BarPlotWidget : Fixed
    {
        public string text {
            get { return label.text; }
            set {
                label.text = value;
            }
        }

        public float currentValue {
            get {
                return bar.progress;
            }
            set {
                bar.progress = value / fullScale;
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

        public float fullScale;

        private TouchCurvedProgressBar bar;
        private TouchLabel textBox;
        private TouchLabel label;

        public BarPlotWidget () {
            SetSizeRequest (205, 169);

            var box = new TouchGraphicalBox (205, 169);
            box.color = "grey4";
            box.transparency = 0.1f;
            Put (box, 0, 0);

            label = new TouchLabel ();
            label.text = "Plot";
            label.textColor = "grey3";
            label.WidthRequest = 199;
            label.textAlignment = TouchAlignment.Center;
            label.textRender.textWrap = TouchTextWrap.Shrink;
            Put (label, 3, 80);

            bar = new TouchCurvedProgressBar ();
            bar.backgroundColor = "grey3";
            bar.backgroundColor.A = 0.15f;
            bar.curveStyle = CurveStyle.ThreeQuarterCurve;
            bar.SetSizeRequest (199, 163);
            Put (bar, 3, 3);

            fullScale = 100.0f;

            textBox = new TouchLabel ();
            textBox.SetSizeRequest (199, 30);
            textBox.textSize = 20;
            textBox.text = "0.0";
            textBox.textColor = "pri";
            textBox.textAlignment = TouchAlignment.Center;
            Put (textBox, 3, 130);
        }

        public virtual void OnUpdate () {
            throw new Exception ("Update method not implemented");
        }
    }
}

