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
                return bar.currentProgress;
            }
            set {
                bar.currentProgress = value / fullScale;
                textBox.text = value.ToString ("F1");
            }
        }

        public float fullScale;

        private TouchProgressBar bar;
        private TouchLabel textBox;
        private TouchLabel label;

        public BarPlotWidget () {
            SetSizeRequest (100, 169);

            var box = new TouchGraphicalBox (100, 169);
            box.color = "grey4";
            box.transparency = 0.1f;
            Put (box, 0, 0);

            label = new TouchLabel ();
            label.text = "Plot";
            label.textColor = "pri";
            label.WidthRequest = 100;
            label.textAlignment = TouchAlignment.Center;
            label.textRender.textWrap = TouchTextWrap.Shrink;
            label.textRender.orientation = TouchOrientation.Vertical;
            Put (label, 29, 3);

            bar = new TouchProgressBar ();
            bar.colorBackground = "grey3";
            bar.colorBackground.A = 0.15f;
            bar.SetSizeRequest (26, 163);
            Put (bar, 3, 3);

            fullScale = 100.0f;

            textBox = new TouchLabel ();
            textBox.SetSizeRequest (65, 30);
            textBox.textSize = 20;
            textBox.text = "0.0";
            textBox.textAlignment = TouchAlignment.Center;
            Put (textBox, 32, 130);
        }

        public virtual void OnUpdate () {
            throw new Exception ("Update method not implemented");
        }
    }
}

