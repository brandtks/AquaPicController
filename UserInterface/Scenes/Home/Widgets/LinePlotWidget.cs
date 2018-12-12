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
    public delegate LinePlotWidget CreateLinePlotHandler (string group, int row, int column);

    public class LinePlotData
    {
        public CreateLinePlotHandler CreateInstanceEvent;

        public LinePlotData (CreateLinePlotHandler CreateInstanceEvent) {
            this.CreateInstanceEvent = CreateInstanceEvent;
        }

        public LinePlotWidget CreateInstance (string group, int row, int column) {
            if (CreateInstanceEvent != null) {
                return CreateInstanceEvent (group, row, column);
            }

            throw new Exception ("No line plot constructor implemented");
        }
    }

    public class LinePlotWidget : HomeWidget, IHomeWidgetUpdatable
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

        public LinePlotWidget (string name, string group, int row, int column) : base ("LinePlot", name, group, row, column) {
            SetSizeRequest (310, 82);

            var box1 = new TouchGraphicalBox (310, 82);
            box1.color = "grey4";
            box1.transparency = 0.1f;
            Put (box1, 0, 0);

            linePlot = new TouchLinePlot ();
            Put (linePlot, 59, 3);

            displayLabel = new TouchLabel ();
            displayLabel.SetSizeRequest (200, 16);
            displayLabel.text = "Plot";
            displayLabel.textColor = "grey3";
            displayLabel.textAlignment = TouchAlignment.Left;
            displayLabel.textHorizontallyCentered = true;
            displayLabel.textRender.textWrap = TouchTextWrap.Shrink;
            Put (displayLabel, 3, 63);

            textBox = new TouchLabel ();
            textBox.SetSizeRequest (57, 60);
            textBox.textSize = 19;
            textBox.text = "0.0";
            textBox.textColor = "pri";
            textBox.textAlignment = TouchAlignment.Center;
            textBox.textHorizontallyCentered = true;
            textBox.textRender.textWrap = TouchTextWrap.Shrink;
            Put (textBox, 1, 3);

            ShowAll ();
        }

        public virtual void Update () {
            throw new Exception ("Update method not implemented");
        }
    }
}

