﻿using System;
using Gtk;
using Cairo;
using MyWidgetLibrary;

namespace AquaPic
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
                bar.currentProgress = value / 100;
                textBox.text = value.ToString ("F1");
            }
        }

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
            label.WidthRequest = 187;
            Put (label, 4, 2);

            bar = new TouchProgressBar ();
            bar.HeightRequest = 168;
            bar.WidthRequest = 20;
            Put (bar, 83, 22);

            textBox = new TouchTextBox ();
            textBox.WidthRequest = 75;
            textBox.HeightRequest = 35;
            textBox.textSize = 16;
            textBox.text = "0.0";
            textBox.textAlignment = MyAlignment.Center;
            Put (textBox, 3, 28);
        }

        public virtual void OnUpdate () {
            throw new Exception ("Update method not implemented");
        }
    }
}
