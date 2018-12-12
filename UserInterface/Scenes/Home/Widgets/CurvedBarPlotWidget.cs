#region License

/*
    AquaPic Main Control - Handles all functionality for the AquaPic aquarium controller.

    Copyright (c) 2017 Goodtime Development

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see http://www.gnu.org/licenses/
*/

#endregion // License

using System;
using Gtk;
using Cairo;
using GoodtimeDevelopment.TouchWidget;

namespace AquaPic.UserInterface
{
    public delegate CurvedBarPlotWidget CreateCurvedBarPlotHandler (string group, int row, int column);

    public class CurvedBarPlotData
    {
        public CreateCurvedBarPlotHandler CreateInstanceEvent;

        public CurvedBarPlotData (CreateCurvedBarPlotHandler CreateInstanceEvent) {
            this.CreateInstanceEvent = CreateInstanceEvent;
        }

        public CurvedBarPlotWidget CreateInstance (string group, int row, int column) {
            if (CreateInstanceEvent != null) {
                return CreateInstanceEvent (group, row, column);
            }
            throw new Exception ("No bar plot constructor implemented");
        }
    }

    public class CurvedBarPlotWidget : HomeWidget, IHomeWidgetUpdatable
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

        public CurvedBarPlotWidget (string name, string group, int row, int column) : base ("CurvedBarPlot", name, group, row, column) {
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

        public virtual void Update () {
            throw new Exception ("Update method not implemented");
        }
    }
}

