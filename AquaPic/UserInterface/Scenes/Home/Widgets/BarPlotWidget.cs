#region License

/*
    AquaPic Main Control - Handles all functionality for the AquaPic aquarium controller.

    Copyright (c) 2017 Skyler Brandt

    This program is free software; you can redistribute it and/or
    modify it under the terms of the GNU General Public License
    as published by the Free Software Foundation; either version 2
    of the License, or (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program; if not, write to the Free Software
    Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.

    Optionally you can also view the license at <http://www.gnu.org/licenses/>.
*/

#endregion // License

﻿using System;
using Gtk;
using Cairo;
using GoodtimeDevelopment.TouchWidget;

namespace AquaPic.UserInterface
{
    public delegate BarPlotWidget CreateBarPlotHandler (params object[] options);

    public class BarPlotData
    {
        public CreateBarPlotHandler CreateInstanceEvent;

        public BarPlotData (CreateBarPlotHandler CreateInstanceEvent) {
            this.CreateInstanceEvent = CreateInstanceEvent;
        }

        public BarPlotWidget CreateInstance (params object[] options) {
            if (CreateInstanceEvent != null)
                return CreateInstanceEvent (options);
            else
                throw new Exception ("No bar plot constructor implemented");
        }
    }

    public class BarPlotWidget : Fixed
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

        public BarPlotWidget () {
            SetSizeRequest (100, 169);

            var box = new TouchGraphicalBox (100, 169);
            box.color = "grey4";
            box.transparency = 0.1f;
            Put (box, 0, 0);

            nameLabel = new TouchLabel ();
            nameLabel.text = "Plot";
            nameLabel.textColor = "grey3";
            nameLabel.HeightRequest = 151;
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

        public virtual void OnUpdate () {
            throw new Exception ("Update method not implemented");
        }
    }
}

