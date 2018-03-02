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
using AquaPic.Globals;
using AquaPic.Drivers;

namespace AquaPic.UserInterface
{
    public delegate void UpdateScreenHandler ();

    public class PowerOutletSlider : Fixed
    {
        public UpdateScreenHandler UpdateScreen;

        public TouchLabel outletName;
        public TouchLabel statusLabel;
        public TouchSelectorSwitch ss;
        public TouchCurvedProgressBar ampBar;
        public TouchLabel ampText;
        public EventBox settingsButton;

        public float amps {
            set {
                float v;
                if (value > 10.0f)
                    v = 10.0f;
                else if (value < 0.0f)
                    v = 0.0f;
                else
                    v = value;

                ampText.text = v.ToString ("F1");
                ampBar.progress = v / 10.0f;
            }
        }

        public PowerOutletSlider (int id) {
            SetSizeRequest (180, 180);

            ampBar = new TouchCurvedProgressBar ();
            ampBar.SetSizeRequest (170, 135);
            ampBar.curveStyle = CurveStyle.ThreeQuarterCurve;
            Put (ampBar, 5, 5);
            ampBar.Show ();

            ampText = new TouchLabel ();
            ampText.WidthRequest = 180;
            ampText.textAlignment = TouchAlignment.Center;
            ampText.textRender.unitOfMeasurement = UnitsOfMeasurement.Amperage;
            ampText.text = "0.0";
            ampText.textSize = 20;
            ampText.textColor = "pri";
            Put (ampText, 0, 105);
            ampText.Show ();

            ss = new TouchSelectorSwitch (id, 3, 0, TouchOrientation.Horizontal);
            ss.sliderSize = MySliderSize.Large;
            ss.WidthRequest = 170;
            ss.HeightRequest = 30;
            ss.sliderColorOptions [0] = "grey2";
            ss.sliderColorOptions [1] = "pri";
            ss.sliderColorOptions [2] = "seca";
            ss.textOptions [0] = "Off";
            ss.textOptions [1] = "Auto";
            ss.textOptions [2] = "On";
            Put (ss, 5, 145);
            ss.Show ();

            outletName = new TouchLabel ();
            outletName.textColor = "grey3";
            outletName.WidthRequest = 100;
            outletName.textRender.textWrap = TouchTextWrap.Shrink;
            outletName.textAlignment = TouchAlignment.Center;
            Put (outletName, 40, 67);
            outletName.Show ();

            statusLabel = new TouchLabel ();
            statusLabel.text = "Off";
            statusLabel.textSize = 20;
            statusLabel.textColor = "grey4";
            statusLabel.WidthRequest = 180;
            statusLabel.textAlignment = TouchAlignment.Center;
            Put (statusLabel, 0, 37);
            statusLabel.Show ();

            settingsButton = new EventBox ();
            settingsButton.VisibleWindow = false;
            settingsButton.SetSizeRequest (180, 140);
            settingsButton.ButtonReleaseEvent += OnSettingButtonRelease;
            Put (settingsButton, 0, 0);
            settingsButton.Show ();

            ShowAll ();
        }

        protected void OnSettingButtonRelease (object sender, ButtonReleaseEventArgs args) {
            IndividualControl ic = Power.GetOutletIndividualControl (outletName.text);
            string owner = Power.GetOutletOwner (ic);
            if (owner == "Power") {
                string n = string.Format ("{0}.p{1}", Power.GetPowerStripName (ic.Group), ic.Individual);
                OutletSettings os;
                if (n == outletName.text)
                    os = new OutletSettings (outletName.text, false, ic);
                else
                    os = new OutletSettings (outletName.text, true, ic);

                os.Run ();
                os.Destroy ();
                os.Dispose ();

                if (UpdateScreen != null)
                    UpdateScreen ();
            } else {
                MessageBox.Show ("Can't edit outlet,\nOwned by " + owner);
            }
        }
    }
}

