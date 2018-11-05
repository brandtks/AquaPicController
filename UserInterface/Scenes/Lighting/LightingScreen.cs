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
using AquaPic.Drivers;
using AquaPic.Modules;
using AquaPic.Globals;

namespace AquaPic.UserInterface
{
    public class LightingWindow : SceneBase
    {
        string fixtureName;
        TouchComboBox combo;
        TouchSelectorSwitch modeSelector;
        TouchLayeredProgressBar dimmingProgressBar;
        TouchLabel dimmingHeader;
        TouchLabel actualTextBox;
        TouchLabel actualLabel;
        TouchLabel requestedLabel;
        TouchLabel requestedTextLabel;
        TouchTextBox requestedTextBox;
        TouchLabel autoTextBox;
        TouchLabel autoLabel;
        TouchLabel outletStateLabel;
        TouchSelectorSwitch outletSelectorSwitch;
        TouchButton fixtureSettingBtn;
        bool isDimmingFixture;
        bool dimmingIsManual;
        LightingStateDisplay lightingStateDisplay;

        public LightingWindow (params object[] options) : base (false) {
            sceneTitle = "Lighting";

            fixtureName = Lighting.defaultFixture;
            if (fixtureName.IsNotEmpty ()) {
                dimmingIsManual = false;
            }

            if (options.Length >= 3) {
                string requestedFixture = options[2] as string;
                if (requestedFixture != null) {
                    if (Lighting.CheckFixtureKeyNoThrow (requestedFixture)) {
                        fixtureName = requestedFixture;
                    }
                }
            }

            outletSelectorSwitch = new TouchSelectorSwitch (0, 3, 0, TouchOrientation.Horizontal);
            outletSelectorSwitch.sliderSize = MySliderSize.Large;
            outletSelectorSwitch.WidthRequest = 180;
            outletSelectorSwitch.HeightRequest = 30;
            outletSelectorSwitch.sliderColorOptions[0] = "grey2";
            outletSelectorSwitch.sliderColorOptions[1] = "pri";
            outletSelectorSwitch.sliderColorOptions[2] = "seca";
            outletSelectorSwitch.textOptions = new string[] { "Off", "Auto", "On" };
            outletSelectorSwitch.SelectorChangedEvent += OnOutletControlSelectorChanged;
            Put (outletSelectorSwitch, 605, 77);
            outletSelectorSwitch.Show ();

            outletStateLabel = new TouchLabel ();
            outletStateLabel.textAlignment = TouchAlignment.Center;
            outletStateLabel.WidthRequest = 180;
            outletStateLabel.textSize = 14;
            Put (outletStateLabel, 605, 110);
            outletStateLabel.Show ();

            dimmingHeader = new TouchLabel ();
            dimmingHeader.textAlignment = TouchAlignment.Center;
            dimmingHeader.WidthRequest = 180;
            dimmingHeader.text = "Dimming Control";
            Put (dimmingHeader, 605, 148);
            dimmingHeader.Show ();

            modeSelector = new TouchSelectorSwitch (2);
            modeSelector.SetSizeRequest (140, 30);
            modeSelector.sliderSize = MySliderSize.Large;
            modeSelector.textOptions[0] = "Manual";
            modeSelector.textOptions[1] = "Auto";
            modeSelector.sliderColorOptions[0] = "grey2";
            modeSelector.sliderColorOptions[1] = "pri";
            modeSelector.SelectorChangedEvent += OnDimmingModeSelectorChanged;
            Put (modeSelector, 605, 173);
            modeSelector.Show ();

            dimmingProgressBar = new TouchLayeredProgressBar ();
            dimmingProgressBar.colorProgress = "seca";
            dimmingProgressBar.colorProgressSecondary = "pri";
            dimmingProgressBar.drawPrimaryWhenEqual = false;
            dimmingProgressBar.ProgressChangedEvent += OnProgressChanged;
            dimmingProgressBar.ProgressChangingEvent += OnProgressChanging;
            dimmingProgressBar.HeightRequest = 280;
            Put (dimmingProgressBar, 755, 173);
            dimmingProgressBar.Show ();

            actualTextBox = new TouchLabel ();
            actualTextBox.WidthRequest = 140;
            actualTextBox.textSize = 20;
            actualTextBox.textColor = "pri";
            actualTextBox.textAlignment = TouchAlignment.Center;
            actualTextBox.textRender.unitOfMeasurement = UnitsOfMeasurement.Percentage;
            Put (actualTextBox, 605, 209);
            actualTextBox.Show ();

            actualLabel = new TouchLabel ();
            actualLabel.WidthRequest = 140;
            actualLabel.text = "Current";
            actualLabel.textColor = "grey3";
            actualLabel.textAlignment = TouchAlignment.Center;
            Put (actualLabel, 605, 244);
            actualLabel.Show ();

            requestedLabel = new TouchLabel ();
            requestedLabel.WidthRequest = 140;
            requestedLabel.textSize = 20;
            requestedLabel.textColor = "seca";
            requestedLabel.textAlignment = TouchAlignment.Center;
            requestedLabel.textRender.unitOfMeasurement = UnitsOfMeasurement.Percentage;
            Put (requestedLabel, 605, 272);
            requestedLabel.Show ();

            requestedTextBox = new TouchTextBox ();
            requestedTextBox.enableTouch = true;
            requestedTextBox.TextChangedEvent += (sender, args) => {
                try {
                    float newLevel = Convert.ToSingle (args.text);
                    if (newLevel < 0.0f)
                        newLevel = 0.0f;
                    if (newLevel > 100.0f)
                        newLevel = 100.0f;
                    Lighting.SetDimmingLevel (fixtureName, newLevel);
                } catch (Exception ex) {
                    MessageBox.Show (ex.Message);
                    args.keepText = false;
                }
            };
            requestedTextBox.SetSizeRequest (110, 36);
            requestedTextBox.textSize = 20;
            requestedTextBox.textColor = "seca";
            requestedTextBox.textAlignment = TouchAlignment.Center;
            requestedTextBox.textRender.unitOfMeasurement = UnitsOfMeasurement.Percentage;
            requestedTextBox.Visible = false;
            Put (requestedTextBox, 620, 272);
            requestedTextBox.Show ();

            requestedTextLabel = new TouchLabel ();
            requestedTextLabel.WidthRequest = 140;
            requestedTextLabel.text = "Requested";
            requestedTextLabel.textColor = "grey3";
            requestedTextLabel.textAlignment = TouchAlignment.Center;
            Put (requestedTextLabel, 605, 307);
            requestedTextLabel.Show ();

            autoTextBox = new TouchLabel ();
            autoTextBox.WidthRequest = 140;
            autoTextBox.Visible = false;
            autoTextBox.textSize = 20;
            autoTextBox.textColor = "grey4";
            autoTextBox.textAlignment = TouchAlignment.Center;
            autoTextBox.textRender.unitOfMeasurement = UnitsOfMeasurement.Percentage;
            Put (autoTextBox, 605, 335);
            autoTextBox.Show ();

            autoLabel = new TouchLabel ();
            autoLabel.WidthRequest = 140;
            autoLabel.Visible = false;
            autoLabel.text = "Auto";
            autoLabel.textColor = "grey3";
            autoLabel.textAlignment = TouchAlignment.Center;
            Put (autoLabel, 605, 370);
            autoLabel.Show ();

            fixtureSettingBtn = new TouchButton ();
            fixtureSettingBtn.text = Convert.ToChar (0x2699).ToString ();
            fixtureSettingBtn.SetSizeRequest (30, 30);
            fixtureSettingBtn.buttonColor = "pri";
            fixtureSettingBtn.ButtonReleaseEvent += (o, args) => {
                var parent = Toplevel as Window;
                var s = new FixtureSettings (fixtureName, fixtureName.IsNotEmpty (), parent);
                s.Run ();
                var newFixtureName = s.newOrUpdatedFixtureName;
                var outcome = s.outcome;
                s.Destroy ();
                s.Dispose ();

                if ((outcome == TouchSettingsOutcome.Modified) && (newFixtureName != fixtureName)) {
                    var index = combo.comboList.IndexOf (fixtureName);
                    combo.comboList[index] = newFixtureName;
                    fixtureName = newFixtureName;
                } else if (outcome == TouchSettingsOutcome.Added) {
                    combo.comboList.Insert (combo.comboList.Count - 1, newFixtureName);
                    combo.activeText = newFixtureName;
                    fixtureName = newFixtureName;
                } else if (outcome == TouchSettingsOutcome.Deleted) {
                    combo.comboList.Remove (fixtureName);
                    fixtureName = Lighting.defaultFixture;
                    combo.activeText = fixtureName;
                }

                combo.QueueDraw ();
                GetFixtureData ();
            };
            Put (fixtureSettingBtn, 755, 35);
            fixtureSettingBtn.Show ();

            lightingStateDisplay = new LightingStateDisplay ();
            lightingStateDisplay.SetSizeRequest (540, 360);
            Put (lightingStateDisplay, 55, 90);
            lightingStateDisplay.Show ();

            combo = new TouchComboBox (Lighting.GetAllFixtureNames ());
            combo.comboList.Add ("New fixture...");
            combo.activeIndex = 0;
            combo.WidthRequest = 250;
            combo.ComboChangedEvent += OnComboChanged;
            Put (combo, 500, 35);
            combo.Show ();

            if (fixtureName.IsNotEmpty ()) {
                combo.activeText = fixtureName;
            } else {
                combo.activeIndex = 0;
            }

            GetFixtureData ();
            Show ();
        }

        public override void Dispose () {
            if (fixtureName.IsNotEmpty ()) {
                Power.RemoveHandlerOnStateChange (Lighting.GetFixtureOutletIndividualControl (fixtureName), OnOutletStateChange);
            }
            base.Dispose ();
        }

        protected void GetFixtureData () {
            if (fixtureName.IsNotEmpty ()) {
                dimmingHeader.Visible = true;
                outletStateLabel.Visible = true;
                outletSelectorSwitch.Visible = true;

                IndividualControl ic = Lighting.GetFixtureOutletIndividualControl (fixtureName);
                Power.AddHandlerOnStateChange (ic, OnOutletStateChange);

                if (Power.GetOutletMode (ic) == Mode.Auto) {
                    outletSelectorSwitch.currentSelected = 1;
                    outletSelectorSwitch.QueueDraw ();
                } else {
                    if (Power.GetOutletManualState (ic) == MyState.Off) {
                        outletSelectorSwitch.currentSelected = 0;
                    } else {
                        outletSelectorSwitch.currentSelected = 2;
                    }
                }

                if (Power.GetOutletState (ic) == MyState.Off) {
                    outletStateLabel.text = "Light Off";
                    outletStateLabel.textColor = "grey4";
                } else {
                    outletStateLabel.text = "Light On";
                    outletStateLabel.textColor = "secb";
                }

                isDimmingFixture = Lighting.IsDimmingFixture (fixtureName);

                lightingStateDisplay.SetStates (Lighting.GetLightingStates (fixtureName), isDimmingFixture);
                lightingStateDisplay.QueueDraw ();

                if (isDimmingFixture) {
                    dimmingHeader.Visible = true;
                    modeSelector.Visible = true;
                    dimmingProgressBar.Visible = true;
                    actualTextBox.Visible = true;
                    actualLabel.Visible = true;
                    requestedLabel.Visible = true;
                    requestedTextLabel.Visible = true;

                    Mode m = Lighting.GetDimmingMode (fixtureName);
                    dimmingIsManual = m == Mode.Manual;
                    if (!dimmingIsManual) {
                        modeSelector.currentSelected = 1;
                        dimmingProgressBar.enableTouch = false;
                        requestedTextBox.Visible = false;
                        autoTextBox.Visible = false;
                        autoLabel.Visible = false;
                    } else {
                        modeSelector.currentSelected = 0;
                        dimmingProgressBar.enableTouch = true;
                        requestedTextBox.Visible = true;
                        requestedTextBox.text = string.Format ("{0:N2}", Lighting.GetRequestedDimmingLevel (fixtureName));
                        autoTextBox.Visible = true;
                        autoLabel.Visible = true;
                        autoTextBox.text = string.Format ("{0:N2}", Lighting.GetAutoDimmingLevel (fixtureName));
                    }

                    float level = Lighting.GetCurrentDimmingLevel (fixtureName);
                    dimmingProgressBar.currentProgressSecondary = level / 100.0f;
                    actualTextBox.text = string.Format ("{0:N2}", level);

                    level = Lighting.GetRequestedDimmingLevel (fixtureName);
                    dimmingProgressBar.currentProgress = level / 100.0f;
                    requestedLabel.text = string.Format ("{0:N2}", level);

                    // bastardized way of getting the combobox in front of other widgets
                    // I think there's another way to do this but I can't remember what it is or if it even works
                    combo.Visible = false;
                    combo.Visible = true;

                    timerId = GLib.Timeout.Add (1000, OnUpdateTimer);
                } else {
                    dimmingHeader.Visible = false;
                    dimmingIsManual = false;
                    modeSelector.Visible = false;
                    dimmingProgressBar.Visible = false;
                    dimmingProgressBar.enableTouch = false;
                    actualTextBox.Visible = false;
                    actualLabel.Visible = false;
                    autoTextBox.Visible = false;
                    autoLabel.Visible = false;
                    requestedLabel.Visible = false;
                    requestedTextLabel.Visible = false;
                    requestedTextBox.Visible = false;
                }

                QueueDraw ();
            } else {
                dimmingHeader.Visible = false;
                modeSelector.Visible = false;
                dimmingProgressBar.Visible = false;
                actualTextBox.Visible = false;
                actualLabel.Visible = false;
                autoTextBox.Visible = false;
                autoLabel.Visible = false;
                requestedLabel.Visible = false;
                requestedTextLabel.Visible = false;
                outletStateLabel.Visible = false;
                outletSelectorSwitch.Visible = false;
                requestedTextBox.Visible = false;
            }
        }

        protected void OnComboChanged (object sender, ComboBoxChangedEventArgs e) {
            if (e.activeText == "New fixture...") {
                var parent = Toplevel as Window;
                var s = new FixtureSettings (string.Empty, false, parent);
                s.Run ();
                var newFixtureName = s.newOrUpdatedFixtureName;
                var outcome = s.outcome;
                s.Destroy ();
                s.Dispose ();

                if (outcome == TouchSettingsOutcome.Added) {
                    combo.comboList.Insert (combo.comboList.Count - 1, newFixtureName);
                    combo.activeText = newFixtureName;
                    fixtureName = newFixtureName;
                } else {
                    combo.activeText = fixtureName;
                }
            } else {
                fixtureName = e.activeText;
            }

            combo.QueueDraw ();
            GetFixtureData ();
        }

        protected override bool OnUpdateTimer () {
            if (fixtureName.IsNotEmpty ()) {
                if (isDimmingFixture) {
                    float level = Lighting.GetCurrentDimmingLevel (fixtureName);
                    dimmingProgressBar.currentProgressSecondary = level / 100.0f;
                    actualTextBox.text = string.Format ("{0:N2}", level);

                    level = Lighting.GetRequestedDimmingLevel (fixtureName);
                    dimmingProgressBar.currentProgress = level / 100.0f;
                    requestedLabel.text = string.Format ("{0:N2}", level);
                    requestedTextBox.text = string.Format ("{0:N2}", level);

                    actualTextBox.QueueDraw ();
                    requestedLabel.QueueDraw ();
                    dimmingProgressBar.QueueDraw ();

                    if (dimmingIsManual) {
                        autoTextBox.text = string.Format ("{0:N2}", Lighting.GetAutoDimmingLevel (fixtureName));
                        autoTextBox.QueueDraw ();
                    }
                }

                return isDimmingFixture;
            }

            return false;
        }

        protected void OnDimmingModeSelectorChanged (object sender, SelectorChangedEventArgs args) {
            if (args.currentSelectedIndex == 0) {
                Lighting.SetDimmingMode (fixtureName, Mode.Manual);
                IndividualControl ic = Lighting.GetFixtureOutletIndividualControl (fixtureName);
                Power.SetOutletManualState (ic, Power.GetOutletState (ic));
                Power.SetOutletMode (ic, Mode.Manual);
                dimmingProgressBar.enableTouch = true;
                requestedTextBox.Visible = true;
                requestedTextBox.text = string.Format ("{0:N2}", Lighting.GetRequestedDimmingLevel (fixtureName));
                autoTextBox.Visible = true;
                autoLabel.Visible = true;
                dimmingIsManual = true;
                autoTextBox.text = string.Format ("{0:N2}", Lighting.GetAutoDimmingLevel (fixtureName));
                if (Power.GetOutletState (ic) == MyState.Off) {
                    outletSelectorSwitch.currentSelected = 0;
                } else {
                    outletSelectorSwitch.currentSelected = 2;
                }
                outletSelectorSwitch.QueueDraw ();
            } else {
                Lighting.SetDimmingMode (fixtureName, Mode.Auto);
                Power.SetOutletMode (Lighting.GetFixtureOutletIndividualControl (fixtureName), Mode.Auto);
                dimmingProgressBar.enableTouch = false;
                requestedTextBox.Visible = false;
                autoTextBox.Visible = false;
                autoLabel.Visible = false;
                dimmingIsManual = false;
                outletSelectorSwitch.currentSelected = 1;
                outletSelectorSwitch.QueueDraw ();
            }

            dimmingProgressBar.QueueDraw ();
            autoTextBox.QueueDraw ();
            autoLabel.QueueDraw ();
        }

        protected void OnOutletControlSelectorChanged (object sender, SelectorChangedEventArgs args) {
            IndividualControl ic = Lighting.GetFixtureOutletIndividualControl (fixtureName);

            if (args.currentSelectedIndex == 0) { // Manual Off
                Power.SetOutletMode (ic, Mode.Manual);
                Power.SetOutletManualState (ic, MyState.Off);
            } else if (args.currentSelectedIndex == 2) { // Manual On
                Power.SetOutletMode (ic, Mode.Manual);
                Power.SetOutletManualState (ic, MyState.On);
            } else if (args.currentSelectedIndex == 1) {
                Power.SetOutletMode (ic, Mode.Auto);
            }

            GetFixtureData ();
        }

        protected void OnProgressChanged (object sender, ProgressChangeEventArgs args) {
            Lighting.SetDimmingLevel (fixtureName, args.currentProgress * 100.0f);
            float level = Lighting.GetCurrentDimmingLevel (fixtureName);
            actualTextBox.text = string.Format ("{0:N2}", level);
            actualTextBox.QueueDraw ();
        }

        protected void OnProgressChanging (object sender, ProgressChangeEventArgs args) {
            requestedTextBox.text = string.Format ("{0:N2}", args.currentProgress * 100.0f);
            requestedTextBox.QueueDraw ();
        }

        protected void OnOutletStateChange (object sender, StateChangeEventArgs args) {
            if (args.state == MyState.Off) {
                outletStateLabel.text = "Light Off";
                outletStateLabel.textColor = "grey4";
            } else {
                outletStateLabel.text = "Light On";
                outletStateLabel.textColor = "secb";
            }
            outletStateLabel.QueueDraw ();
        }
    }
}

