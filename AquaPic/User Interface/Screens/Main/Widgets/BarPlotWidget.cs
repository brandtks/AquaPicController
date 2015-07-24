using System;
using Gtk;
using Cairo;
using MyWidgetLibrary;

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
        private TouchTextBox textBox;
        private TouchLabel label;

        public BarPlotWidget () {
            SetSizeRequest (108, 195);

            var box = new MyBox (108, 195);
            Put (box, 0, 0);

            label = new TouchLabel ();
            label.text = "Plot";
            label.textColor = "pri";
            label.textSize = 12;
            label.WidthRequest = 100;
            label.textAlignment = MyAlignment.Center;
            label.render.textWrap = MyTextWrap.Shrink;
            label.render.orientation = MyOrientation.Vertical;
            Put (label, 3, 38);

            bar = new TouchProgressBar ();
            bar.HeightRequest = 156;
            bar.WidthRequest = 30;
            Put (bar, 75, 36);

            fullScale = 100.0f;

            textBox = new TouchTextBox ();
            textBox.WidthRequest = 102;
            textBox.HeightRequest = 30;
            textBox.textSize = 14;
            textBox.text = "0.0";
            textBox.textAlignment = MyAlignment.Center;
            Put (textBox, 3, 3);
        }

        public virtual void OnUpdate () {
            throw new Exception ("Update method not implemented");
        }
    }
}

