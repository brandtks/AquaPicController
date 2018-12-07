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
    public delegate BarPlotWidget CreateBarPlotHandler (string groupName, int row, int column);

    public class BarPlotData
    {
        public CreateBarPlotHandler CreateInstanceEvent;

        public BarPlotData (CreateBarPlotHandler CreateInstanceEvent) {
            this.CreateInstanceEvent = CreateInstanceEvent;
        }

        public BarPlotWidget CreateInstance (string groupName, int row, int column) {
            if (CreateInstanceEvent != null)
                return CreateInstanceEvent (groupName, row, column);
            throw new Exception ("No bar plot constructor implemented");
        }
    }

    public class BarPlotWidget : HomeWidget, IHomeWidgetUpdatable
    {
        public string text {
            get { return nameLabel.text; }
            set {
                nameLabel.text = value;
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

        public UnitsOfMeasurement unitOfMeasurement {
            get {
                return textBox.textRender.unitOfMeasurement;
            }
            set {
                textBox.textRender.unitOfMeasurement = value;
            }
        }

        public float fullScale;

        private TouchProgressBar bar;
        protected TouchLabel textBox;
        protected TouchLabel nameLabel;

        public BarPlotWidget (int row, int column) : base ("BarPlot", row, column) {
            SetSizeRequest (100, 169);

            var box = new TouchGraphicalBox (100, 169);
            box.color = "grey4";
            box.transparency = 0.1f;
            Put (box, 0, 0);

            nameLabel = new TouchLabel ();
            nameLabel.text = "Plot";
            nameLabel.textColor = "grey3";
            nameLabel.HeightRequest = 151;
            nameLabel.WidthRequest = 5;
            nameLabel.textAlignment = TouchAlignment.Center;
            nameLabel.textRender.textWrap = TouchTextWrap.Shrink;
            nameLabel.textRender.orientation = TouchOrientation.Vertical;
            Put (nameLabel, 82, 9);

            bar = new TouchProgressBar ();
            bar.colorBackground = "grey3";
            bar.colorBackground.A = 0.15f;
            bar.SetSizeRequest (26, 161);
            Put (bar, 56, 4);

            fullScale = 100.0f;

            textBox = new TouchLabel ();
            textBox.SetSizeRequest (53, 30);
            textBox.textSize = 14;
            textBox.text = "0.0";
            textBox.textColor = "pri";
            textBox.textAlignment = TouchAlignment.Center;
            Put (textBox, 0, 49);
        }

        public virtual void Update () {
            throw new Exception ("Update method not implemented");
        }
    }
}

