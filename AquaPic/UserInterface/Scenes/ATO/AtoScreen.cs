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

﻿using System;
using Gtk;
using Cairo;
using GoodtimeDevelopment.TouchWidget;
using GoodtimeDevelopment.Utilites;
using AquaPic.Modules;
using AquaPic.Runtime;

namespace AquaPic.UserInterface
{
    public class AtoWindow : SceneBase
    {
		string atoGroupName;
		TouchLabel atoStateTextBox;
        TouchButton atoClearFailBtn;
		TouchComboBox atoGroupCombo;

		public AtoWindow (params object[] options) : base () {
			sceneTitle = "Auto Top Off";         

            /**************************************************************************************************************/
            /* ATO                                                                                                        */
            /**************************************************************************************************************/
			atoGroupName = AutoTopOff.firstAtoGroup;

            var stateLabel = new TouchLabel ();
            stateLabel.text = "ATO State";
            stateLabel.textColor = "grey3";
            stateLabel.WidthRequest = 329;
            stateLabel.textAlignment = TouchAlignment.Center;
            Put (stateLabel, 60, 155);
            stateLabel.Show ();

            atoStateTextBox = new TouchLabel ();
            atoStateTextBox.WidthRequest = 329;
            atoStateTextBox.textSize = 20;
            atoStateTextBox.textAlignment = TouchAlignment.Center;
            Put (atoStateTextBox, 60, 120);
            atoStateTextBox.Show ();

            var atoSettingsBtn = new TouchButton ();
            atoSettingsBtn.text = "Settings";
            atoSettingsBtn.SetSizeRequest (100, 60);
            atoSettingsBtn.ButtonReleaseEvent += (o, args) => {
				var s = new AtoSettings (atoGroupName, atoGroupName.IsNotEmpty ());
                s.Run ();
				var newGroupName = s.atoGroupName;
                var outcome = s.outcome;
                s.Destroy ();
                s.Dispose ();

                if (outcome == TouchSettingsOutcome.Added) {
                    atoGroupName = newGroupName;
                    atoGroupCombo.comboList.Insert (atoGroupCombo.comboList.Count - 1, atoGroupName);
                    atoGroupCombo.activeText = atoGroupName;
                } else if (outcome == TouchSettingsOutcome.Deleted) {
                    atoGroupCombo.comboList.Remove (atoGroupName);
                    atoGroupName = WaterLevel.firstWaterLevelGroup;
                    atoGroupCombo.activeText = atoGroupName;
                }

                atoGroupCombo.QueueDraw ();
                GetAtoGroupData ();
            };
            Put (atoSettingsBtn, 290, 405);
            atoSettingsBtn.Show ();

            atoClearFailBtn = new TouchButton ();
            atoClearFailBtn.SetSizeRequest (100, 60);
            atoClearFailBtn.text = "Reset ATO";
            atoClearFailBtn.buttonColor = "compl";
            atoClearFailBtn.ButtonReleaseEvent += (o, args) => {
				if (atoGroupName.IsNotEmpty ()) {
					if (!AutoTopOff.ClearAtoAlarm (atoGroupName))
						MessageBox.Show ("Please acknowledge alarms first");
				}
            };
            Put (atoClearFailBtn, 70, 405);

			atoGroupCombo = new TouchComboBox (AutoTopOff.GetAllAtoGroupNames ());
            if (atoGroupName.IsNotEmpty ()) {
                atoGroupCombo.activeText = atoGroupName;
            } else {
                atoGroupCombo.activeIndex = 0;
            }
            atoGroupCombo.WidthRequest = 235;
            atoGroupCombo.comboList.Add ("New group...");
            atoGroupCombo.ComboChangedEvent += OnGroupComboChanged;
            Put (atoGroupCombo, 153, 77);
            atoGroupCombo.Show ();

			GetAtoGroupData ();

			Show ();
        }

		protected override bool OnUpdateTimer () {
			GetAtoGroupData ();
            return true;
        }

        
		protected void OnGroupComboChanged (object sender, ComboBoxChangedEventArgs e) {
            if (e.activeText == "New group...") {
				var s = new AtoSettings (string.Empty, false);
                s.Run ();
				var newGroupName = s.atoGroupName;
                var outcome = s.outcome;
                s.Destroy ();
                s.Dispose ();

                if (outcome == TouchSettingsOutcome.Added) {
                    atoGroupCombo.comboList.Insert (atoGroupCombo.comboList.Count - 1, newGroupName);
                    atoGroupCombo.activeText = newGroupName;
                    atoGroupName = newGroupName;
                } else {
                    Console.WriteLine ("Moving combo back to {0}", atoGroupName);
                    atoGroupCombo.activeText = atoGroupName;
                }

                atoGroupCombo.QueueDraw ();
            } else {
                atoGroupName = e.activeText;
            }
			GetAtoGroupData ();
        }

		protected void GetAtoGroupData () {
			if (atoGroupName.IsNotEmpty ()) {
				if (AutoTopOff.GetAtoGroupEnable (atoGroupName)) {
					atoStateTextBox.text = string.Format ("{0} : {1}",
						AutoTopOff.GetAtoGroupState (atoGroupName),
						AutoTopOff.GetAtoGroupAtoTime (atoGroupName).SecondsToString ());

					if (Alarm.CheckAlarming (AutoTopOff.GetAtoGroupFailAlarmIndex (atoGroupName))) {
						atoClearFailBtn.Visible = true;
						atoClearFailBtn.Show ();
					} else {
						atoClearFailBtn.Visible = false;
					}

				} else {
					atoStateTextBox.text = "ATO Disabled";
					atoClearFailBtn.Visible = false;
				}
			} else {
				atoStateTextBox.text = "ATO Disabled";
                atoClearFailBtn.Visible = false;
			}

			atoStateTextBox.QueueDraw ();
		}
    }
}

