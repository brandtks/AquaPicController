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
using GoodtimeDevelopment.Utilites;

namespace AquaPic.UserInterface
{
    public delegate void ValueChangedHandler (object sender, float value);

    public class AnalogChannelDisplay : Fixed
    {
        public event ButtonReleaseEventHandler ForceButtonReleaseEvent;
		public event ButtonReleaseEventHandler SettingsButtonReleaseEvent;
        public event ValueChangedHandler ValueChangedEvent;

        public TouchLabel label;
        public TouchTextBox textBox;
        public TouchProgressBar progressBar;
        public TouchLabel typeLabel;
        public TouchButton forceButton;
		public TouchButton settingsButton;

        public int divisionSteps;

        public float currentValue {
            set {
                int v = value.ToInt ();
                textBox.text = v.ToString ("D");

                progressBar.currentProgress = value / divisionSteps;
            }
        }

        public AnalogChannelDisplay () {
            SetSizeRequest (710, 50);

            label = new TouchLabel ();
			label.WidthRequest = 490;
            Put (label, 5, 0);
            label.Show ();

            textBox = new TouchTextBox ();
            textBox.WidthRequest = 175; 
            textBox.TextChangedEvent += (sender, args) => {
                try {
                    currentValue = Convert.ToSingle (args.text);
                    ValueChanged ();
                } catch {
                    ;
                }
            };
            Put (textBox, 0, 20);
            textBox.Show ();

            progressBar = new TouchProgressBar (TouchOrientation.Horizontal);
            progressBar.WidthRequest = 395;
            progressBar.ProgressChangedEvent += (sender, args) => {
                currentValue = args.currentProgress * divisionSteps;
                ValueChanged ();
            };
            Put (progressBar, 185, 20);
            progressBar.Show ();

            typeLabel = new TouchLabel ();
            typeLabel.Visible = false;
            typeLabel.WidthRequest = 200;
            typeLabel.textAlignment = TouchAlignment.Right;
            Put (typeLabel, 500, 0);

            forceButton = new TouchButton ();
            forceButton.SetSizeRequest (85, 30);
			forceButton.buttonColor = "grey4";
            forceButton.text = "Force";
            forceButton.ButtonReleaseEvent += OnForceReleased;
            Put (forceButton, 590, 20);
            forceButton.Show ();

			settingsButton = new TouchButton ();
			settingsButton.SetSizeRequest (30, 30);
			settingsButton.buttonColor = "grey4";
			settingsButton.text = Convert.ToChar (0x2699).ToString ();
			settingsButton.ButtonReleaseEvent += OnSettingsRelease;
			Put (settingsButton, 680, 20);
			settingsButton.Show ();

            Show ();
        }

        protected void OnForceReleased (object sender, ButtonReleaseEventArgs args) {
            if (ForceButtonReleaseEvent != null)
                ForceButtonReleaseEvent (this, args);
            else
                throw new NotImplementedException ("Force button release not implemented");
        }

		protected void OnSettingsRelease (object sender, ButtonReleaseEventArgs args) {
			if (SettingsButtonReleaseEvent != null)
				SettingsButtonReleaseEvent (this, args);
            else
                throw new NotImplementedException ("Settings button release not implemented");
        }

        protected void ValueChanged () {
            if (ValueChangedEvent != null)
                ValueChangedEvent (this, progressBar.currentProgress * (float)divisionSteps);
            else
                throw new NotImplementedException ("Value changed not implemented");
        }
    }
}

