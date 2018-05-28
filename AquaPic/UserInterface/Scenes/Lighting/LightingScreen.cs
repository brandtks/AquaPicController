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
        private string fixtureName;
        private TouchComboBox combo;
        private TouchSelectorSwitch modeSelector;
        private TouchLayeredProgressBar dimmingProgressBar;
        private TouchLabel dimmingHeader;
        private TouchLabel actualTextBox;
        private TouchLabel actualLabel;
        private TouchLabel requestedLabel;
        private TouchLabel requestedTextLabel;
        private TouchTextBox requestedTextBox;
        private TouchLabel autoTextBox;
        private TouchLabel autoLabel;
        private TouchLabel onTimeLabel;
        private TouchLabel onTimeTextBox;
        private TouchLabel offTimeLabel;
        private TouchLabel offTimeTextBox;
        private TouchLabel outletLabel;
        private TouchLabel outletStateLabel;
        private TouchSelectorSwitch outletSelectorSwitch;
        private TouchButton fixtureSettingBtn;
        private TouchButton modifyOnTime;
        private TouchButton modifyOffTime;
        private bool isDimmingFixture;
        private bool dimmingIsManual;

        public LightingWindow (params object[] options) : base (false) {
            sceneTitle = "Lighting";

            var fixtureLabel = new TouchLabel ();
            fixtureLabel.text = "Lighting Fixtures";
            fixtureLabel.textColor = "seca";
            fixtureLabel.textSize = 12;
            Put (fixtureLabel, 415, 80);
            fixtureLabel.Show ();

            var sunRiseLabel = new TouchLabel ();
            sunRiseLabel.text = "Sunrise Today";
            sunRiseLabel.textColor = "grey3"; 
            sunRiseLabel.textAlignment = TouchAlignment.Center;
            sunRiseLabel.WidthRequest = 150;
            Put (sunRiseLabel, 60, 125);
            sunRiseLabel.Show ();

            var sunRise = new TouchLabel ();
            sunRise.text = Lighting.sunRiseToday.ToShortDateString ();
            sunRise.textAlignment = TouchAlignment.Center;
			sunRise.textRender.textWrap = TouchTextWrap.Shrink;
            sunRise.textSize = 20;
            sunRise.WidthRequest = 150;
            Put (sunRise, 60, 90);
            sunRise.Show ();

            var sunSetLabel = new TouchLabel ();
            sunSetLabel.text = "Sunset Today";
            sunSetLabel.textColor = "grey3"; 
            sunSetLabel.textAlignment = TouchAlignment.Center;
            sunSetLabel.WidthRequest = 150;
            Put (sunSetLabel, 220, 125);
            sunSetLabel.Show ();

            var sunSet = new TouchLabel ();
            sunSet.text = Lighting.sunSetToday.ToShortDateString ();
            sunSet.textAlignment = TouchAlignment.Center;
			sunSet.textRender.textWrap = TouchTextWrap.Shrink;
            sunSet.textSize = 20;
            sunSet.WidthRequest = 150;
            Put (sunSet, 220, 90);
            sunSet.Show ();

            var sunRiseTomorrowLabel = new TouchLabel ();
            sunRiseTomorrowLabel.text = "Sunrise Tomorrow";
            sunRiseTomorrowLabel.textColor = "grey3"; 
            sunRiseTomorrowLabel.textAlignment = TouchAlignment.Center;
            sunRiseTomorrowLabel.WidthRequest = 150;
            Put (sunRiseTomorrowLabel, 60, 225);
            sunRiseTomorrowLabel.Show ();

            var sunRiseTomorrow = new TouchLabel ();
            sunRiseTomorrow.text = Lighting.sunRiseTomorrow.ToShortDateString ();
            sunRiseTomorrow.textAlignment = TouchAlignment.Center;
			sunRiseTomorrow.textRender.textWrap = TouchTextWrap.Shrink;
            sunRiseTomorrow.textSize = 20;
            sunRiseTomorrow.WidthRequest = 150;
            Put (sunRiseTomorrow, 60, 190);
            sunRiseTomorrow.Show ();

            var sunSetTomorrowLabel = new TouchLabel ();
            sunSetTomorrowLabel.text = "Sunset Tomorrow";
            sunSetTomorrowLabel.textColor = "grey3"; 
            sunSetTomorrowLabel.textAlignment = TouchAlignment.Center;
            sunSetTomorrowLabel.WidthRequest = 150;
            Put (sunSetTomorrowLabel, 220, 225);
            sunSetTomorrowLabel.Show ();

            var sunSetTomorrow = new TouchLabel ();
            sunSetTomorrow.text = Lighting.sunSetTomorrow.ToShortDateString ();
            sunSetTomorrow.textAlignment = TouchAlignment.Center;
			sunSetTomorrow.textRender.textWrap = TouchTextWrap.Shrink;
            sunSetTomorrow.textSize = 20;
            sunSetTomorrow.WidthRequest = 150;
            Put (sunSetTomorrow, 220, 190);
            sunSetTomorrow.Show ();

            ExposeEvent += (o, args) => {
                using (Context cr = Gdk.CairoHelper.Create (GdkWindow)) {
                    cr.MoveTo (402.5, 70);
                    cr.LineTo (402.5, 460);
                    cr.ClosePath ();
                    cr.LineWidth = 3;
                    TouchColor.SetSource (cr, "grey3", 0.75);
                    cr.Stroke ();
                }
            };

            fixtureName = Lighting.defaultFixture;
            if (fixtureName.IsNotEmpty ()) {
                dimmingIsManual = false;
            }

            if (options.Length >= 3) {
                string requestedFixture = options [2] as string;
                if (requestedFixture != null) {
                    if (Lighting.CheckFixtureKeyNoThrow (requestedFixture)) {
                        fixtureName = requestedFixture;
                    }
                }
            }

            dimmingHeader = new TouchLabel ();
            dimmingHeader.textAlignment = TouchAlignment.Center;
            dimmingHeader.WidthRequest = 165;
            Put (dimmingHeader, 625, 117);
            dimmingHeader.Show ();

            modeSelector = new TouchSelectorSwitch (2);
            modeSelector.SetSizeRequest (135, 30);
            modeSelector.sliderSize = MySliderSize.Large;
            modeSelector.textOptions [0] = "Manual";
            modeSelector.textOptions [1] = "Auto";
            modeSelector.sliderColorOptions [0] = "grey2";
            modeSelector.sliderColorOptions [1] = "pri";
            modeSelector.SelectorChangedEvent += OnDimmingModeSelectorChanged;
            Put (modeSelector, 640, 145);
            modeSelector.Show ();

            dimmingProgressBar = new TouchLayeredProgressBar ();
            dimmingProgressBar.colorProgress = "seca";
            dimmingProgressBar.colorProgressSecondary = "pri";
            dimmingProgressBar.drawPrimaryWhenEqual = false;
            dimmingProgressBar.ProgressChangedEvent += OnProgressChanged;
            dimmingProgressBar.ProgressChangingEvent += OnProgressChanging;
            dimmingProgressBar.HeightRequest = 260;
            Put (dimmingProgressBar, 745, 185);
            dimmingProgressBar.Show ();

            actualTextBox = new TouchLabel ();
            actualTextBox.WidthRequest = 110;
            actualTextBox.textSize = 20;
            actualTextBox.textColor = "pri";
            actualTextBox.textAlignment = TouchAlignment.Center;
            actualTextBox.textRender.unitOfMeasurement = UnitsOfMeasurement.Percentage;
            Put (actualTextBox, 623, 187);
            actualTextBox.Show ();

            actualLabel = new TouchLabel ();
            actualLabel.WidthRequest = 110;
            actualLabel.text = "Current";
            actualLabel.textColor = "grey3";
            actualLabel.textAlignment = TouchAlignment.Center;
            Put (actualLabel, 623, 222);
            actualLabel.Show ();

            requestedLabel = new TouchLabel ();
            requestedLabel.WidthRequest = 110;
            requestedLabel.textSize = 20;
            requestedLabel.textColor = "seca";
            requestedLabel.textAlignment = TouchAlignment.Center;
            requestedLabel.textRender.unitOfMeasurement = UnitsOfMeasurement.Percentage;
            Put (requestedLabel, 623, 250);
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
                    MessageBox.Show (ex.ToString ());
                }
            };
            requestedTextBox.SetSizeRequest (110, 36);
            requestedTextBox.textSize = 20;
            requestedTextBox.textColor = "seca";
            requestedTextBox.textAlignment = TouchAlignment.Center;
            requestedTextBox.textRender.unitOfMeasurement = UnitsOfMeasurement.Percentage;
            requestedTextBox.Visible = false;
            Put (requestedTextBox, 623, 247);
            requestedTextBox.Show ();

            requestedTextLabel = new TouchLabel ();
            requestedTextLabel.WidthRequest = 110;
            requestedTextLabel.text = "Requested";
            requestedTextLabel.textColor = "grey3";
            requestedTextLabel.textAlignment = TouchAlignment.Center;
            Put (requestedTextLabel, 623, 285);
            requestedTextLabel.Show ();

            autoTextBox = new TouchLabel ();
            autoTextBox.WidthRequest = 110;
            autoTextBox.Visible = false;
            autoTextBox.textSize = 20;
            autoTextBox.textColor = "grey4";
            autoTextBox.textAlignment = TouchAlignment.Center;
            autoTextBox.textRender.unitOfMeasurement = UnitsOfMeasurement.Percentage;
            Put (autoTextBox, 623, 313);
            autoTextBox.Show ();

            autoLabel = new TouchLabel ();
            autoLabel.WidthRequest = 110;
            autoLabel.Visible = false;
            autoLabel.text = "Auto";
            autoLabel.textColor = "grey3";
            autoLabel.textAlignment = TouchAlignment.Center;
            Put (autoLabel, 623, 348);
            autoLabel.Show ();

            onTimeLabel = new TouchLabel ();
            onTimeLabel.WidthRequest = 185;
            onTimeLabel.text = "On Time";
            onTimeLabel.textColor = "grey3"; 
            onTimeLabel.textAlignment = TouchAlignment.Center;
            Put (onTimeLabel, 415, 222);
            onTimeLabel.Show ();

            onTimeTextBox = new TouchLabel ();
            onTimeTextBox.WidthRequest = 195;
            onTimeTextBox.textSize = 20;
            onTimeTextBox.textAlignment = TouchAlignment.Center;
            Put (onTimeTextBox, 410, 187);
            onTimeTextBox.Show ();

            modifyOnTime = new TouchButton ();
            modifyOnTime.SetSizeRequest (100, 60);
            modifyOnTime.text = "Modify On Time";
            modifyOnTime.ButtonReleaseEvent += (o, args) => {
                TouchNumberInput numberInput;
                var parent = Toplevel as Window;
                if (parent != null) {
                    if (parent.IsTopLevel)
                        numberInput = new TouchNumberInput (true, parent);
                    else
                        numberInput = new TouchNumberInput (true);
                } else {
                    numberInput = new TouchNumberInput (true);
                }

                numberInput.NumberSetEvent += (value) => {
                    try {
                        var t = Time.Parse (value);

                        bool timeOk = true;
                        if (t.Before (new Time ())) {
                            timeOk = false;
                            MessageBox.Show ("Can't modify time to before current time");
                        }

                        if (t.After (Lighting.GetFixtureOffTime (fixtureName))) {
                            timeOk = false;
                            MessageBox.Show ("Can't modify on time to after off time");
                        }

                        if (timeOk) {
                            var td = Lighting.GetFixtureOffTime (fixtureName);
                            td.UpdateTime (t);
                            onTimeTextBox.text = td.ToShortDateString ();
                            onTimeTextBox.QueueDraw ();
                        }
                    } catch {
                        MessageBox.Show ("Incorrect time format");
                    }
                };

                numberInput.Run ();
                numberInput.Destroy ();
            };
            Put (modifyOnTime, 415, 405);
            modifyOnTime.Show ();

            offTimeLabel = new TouchLabel ();
            offTimeLabel.WidthRequest = 185;
            offTimeLabel.text = "Off Time";
            offTimeLabel.textColor = "grey3"; 
            offTimeLabel.textAlignment = TouchAlignment.Center;
            Put (offTimeLabel, 415, 285);
            offTimeLabel.Show ();

            offTimeTextBox = new TouchLabel ();
            offTimeTextBox.WidthRequest = 195;
            offTimeTextBox.textSize = 20;
            offTimeTextBox.textAlignment = TouchAlignment.Center;
            Put (offTimeTextBox, 410, 250);
            offTimeTextBox.Show ();

            modifyOffTime = new TouchButton ();
            modifyOffTime.SetSizeRequest (100, 60);
            modifyOffTime.text = "Modify Off Time";
            modifyOffTime.ButtonReleaseEvent += (o, args) => {
                TouchNumberInput numberInput;
                var parent = Toplevel as Window;
                if (parent != null) {
                    if (parent.IsTopLevel)
                        numberInput = new TouchNumberInput (true, parent);
                    else
                        numberInput = new TouchNumberInput (true);
                } else {
                    numberInput = new TouchNumberInput (true);
                }

                numberInput.NumberSetEvent += (value) => {
                    try {
                        var t = Time.Parse (value);

                        bool timeOk = true;
                        if (t.Before (new Time ())) {
                            timeOk = false;
                            MessageBox.Show ("Can't modify time to before current time");
                        }

                        if (t.After (Lighting.GetFixtureOnTime (fixtureName))) {
                            timeOk = false;
                            MessageBox.Show ("Can't modify off time to before on time");
                        }

                        if (timeOk) {
                            var td = Lighting.GetFixtureOffTime (fixtureName);
                            td.UpdateTime (t);
                            offTimeTextBox.text = td.ToShortDateString ();
                            offTimeTextBox.QueueDraw ();
                        }
                    } catch {
                        MessageBox.Show ("Incorrect time format");
                    }
                };

                numberInput.Run ();
                numberInput.Destroy ();
            };
            Put (modifyOffTime, 525, 405);
            modifyOffTime.Show ();

            var settingsBtn = new TouchButton ();
            settingsBtn.text = "Settings";
            settingsBtn.SetSizeRequest (100, 60);
            settingsBtn.ButtonReleaseEvent += (o, args) => {
                var s = new LightingSettings ();
                s.Run ();
                s.Destroy ();
                s.Dispose ();

                sunRise.text = Lighting.sunRiseToday.ToShortDateString ();
                sunSet.text = Lighting.sunSetToday.ToShortDateString ();
                sunRiseTomorrow.text = Lighting.sunRiseTomorrow.ToShortDateString ();
                sunSetTomorrow.text = Lighting.sunSetTomorrow.ToShortDateString ();
            };
            Put (settingsBtn, 290, 405);
            settingsBtn.Show ();

            outletLabel = new TouchLabel ();
            outletLabel.text = "Outlet:";
            outletLabel.textColor = "grey4";
            Put (outletLabel, 415, 117);
            outletLabel.Show ();

            outletStateLabel = new TouchLabel ();
            Put (outletStateLabel, 465, 117);
            outletStateLabel.Show ();

            outletSelectorSwitch = new TouchSelectorSwitch (0, 3, 0, TouchOrientation.Horizontal);
            outletSelectorSwitch.sliderSize = MySliderSize.Large;
            outletSelectorSwitch.WidthRequest = 185;
            outletSelectorSwitch.HeightRequest = 30;
            outletSelectorSwitch.sliderColorOptions [0] = "grey2";
            outletSelectorSwitch.sliderColorOptions [1] = "pri";
            outletSelectorSwitch.sliderColorOptions [2] = "seca";
            outletSelectorSwitch.textOptions = new string[] {"Off", "Auto", "On"};
            outletSelectorSwitch.SelectorChangedEvent += OnOutletControlSelectorChanged;
            Put (outletSelectorSwitch, 415, 145);
            outletSelectorSwitch.Show ();

            fixtureSettingBtn = new TouchButton ();
			fixtureSettingBtn.text = Convert.ToChar (0x2699).ToString ();
            fixtureSettingBtn.SetSizeRequest (30, 30);
			fixtureSettingBtn.buttonColor = "grey4";
            fixtureSettingBtn.ButtonReleaseEvent += (o, args) => {
                var s = new FixtureSettings (fixtureName, fixtureName.IsNotEmpty ());
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
			Put (fixtureSettingBtn, 755, 77);
            fixtureSettingBtn.Show ();
            
			combo = new TouchComboBox (Lighting.GetAllFixtureNames ());
            combo.activeIndex = 0;
            combo.WidthRequest = 200;
            combo.comboList.Add ("New fixture...");
            combo.ComboChangedEvent += OnComboChanged;
            Put (combo, 550, 77);
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
                onTimeLabel.Visible = true;
                onTimeTextBox.Visible = true;
                offTimeLabel.Visible = true;
                offTimeTextBox.Visible = true;
                dimmingHeader.Visible = true;
                outletLabel.Visible = true;
                outletStateLabel.Visible = true;
                outletSelectorSwitch.Visible = true;
                modifyOnTime.Visible = true;
                modifyOffTime.Visible = true;

                onTimeTextBox.text = Lighting.GetFixtureOnTime (fixtureName).ToShortDateString ();
                offTimeTextBox.text = Lighting.GetFixtureOffTime (fixtureName).ToShortDateString ();

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
                    outletStateLabel.text = "Off";
                    outletStateLabel.textColor = "grey4";
                } else {
                    outletStateLabel.text = "On";
                    outletStateLabel.textColor = "secb";
                }

                isDimmingFixture = Lighting.IsDimmingFixture (fixtureName);
                if (isDimmingFixture) {
                    dimmingHeader.text = "Dimming Control";

                    modeSelector.Visible = true;
                    dimmingProgressBar.Visible = true;
                    actualTextBox.Visible = true;
                    actualLabel.Visible = true;
                    requestedLabel.Visible = true;
                    requestedTextLabel.Visible = true;
                    modifyOnTime.Visible = true;
                    modifyOffTime.Visible = true;

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
                    dimmingHeader.text = "Dimming not available";
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
                onTimeLabel.Visible = false;
                onTimeTextBox.Visible = false;
                offTimeLabel.Visible = false;
                offTimeTextBox.Visible = false;
                dimmingHeader.Visible = false;
                modeSelector.Visible = false;
                dimmingProgressBar.Visible = false;
                actualTextBox.Visible = false;
                actualLabel.Visible = false;
                autoTextBox.Visible = false;
                autoLabel.Visible = false;
                requestedLabel.Visible = false;
                requestedTextLabel.Visible = false;
                outletLabel.Visible = false;
                outletStateLabel.Visible = false;
                outletSelectorSwitch.Visible = false;
                requestedTextBox.Visible = false;
                modifyOnTime.Visible = false;
                modifyOffTime.Visible = false;
            }
        }

        protected void OnComboChanged (object sender, ComboBoxChangedEventArgs e) {
            if (e.activeText == "New fixture...") {
                var s = new FixtureSettings (string.Empty, false);
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
                outletStateLabel.text = "Off";
                outletStateLabel.textColor = "grey4";
            } else {
                outletStateLabel.text = "On";
                outletStateLabel.textColor = "secb";
            }
            outletStateLabel.QueueDraw ();
        }
    }
}

