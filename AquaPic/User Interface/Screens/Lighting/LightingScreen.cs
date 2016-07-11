using System;
using Gtk;
using Cairo;
using TouchWidgetLibrary;
using AquaPic.Drivers;
using AquaPic.Modules;
using AquaPic.Runtime;
using AquaPic.SerialBus;
using AquaPic.Utilites;

namespace AquaPic.UserInterface
{
    public class LightingWindow : WindowBase
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
        private uint timerId;
        private bool isDimmingFixture;
        private bool dimmingIsManual;

        public LightingWindow (params object[] options) : base () {
            screenTitle = "Lighting";

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
            sunRise.text = Lighting.sunRiseToday.ToTimeString ();
            sunRise.textAlignment = TouchAlignment.Center;
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
            sunSet.text = Lighting.sunSetToday.ToTimeString ();
            sunSet.textAlignment = TouchAlignment.Center;
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
            sunRiseTomorrow.text = Lighting.sunRiseTomorrow.ToTimeString ();
            sunRiseTomorrow.textAlignment = TouchAlignment.Center;
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
            sunSetTomorrow.text = Lighting.sunSetTomorrow.ToTimeString ();
            sunSetTomorrow.textAlignment = TouchAlignment.Center;
            sunSetTomorrow.textSize = 20;
            sunSetTomorrow.WidthRequest = 150;
            Put (sunSetTomorrow, 220, 190);
            sunSetTomorrow.Show ();

            ExposeEvent += (o, args) => {
                using (Context cr = Gdk.CairoHelper.Create (this.GdkWindow)) {
                    cr.MoveTo (402.5, 70);
                    cr.LineTo (402.5, 460);
                    cr.ClosePath ();
                    cr.LineWidth = 3;
                    TouchColor.SetSource (cr, "grey3", 0.75);
                    cr.Stroke ();
                }
            };

            fixtureName = Lighting.defaultFixture;
            if (fixtureName.IsEmpty ()) {
                fixtureLabel.text = "No lighing fixtures added";
            } else {
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

            var btn = new TouchButton ();
            btn.SetSizeRequest (100, 60);
            btn.text = "Modify On Time";
            btn.ButtonReleaseEvent += (o, args) => {
                TouchNumberInput numberInput;
                var parent = this.Toplevel as Gtk.Window;
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
                        if (t.CompareToTime (new Time ()) == -1) {
                            timeOk = false;
                            MessageBox.Show ("Can't modify time to before current time");
                        }

                        if (t.CompareToTime (Lighting.GetFixtureOffTime (fixtureName)) == 1) {
                            timeOk = false;
                            MessageBox.Show ("Can't modify on time to after off time");
                        }

                        if (timeOk) {
                            var td = Lighting.GetFixtureOffTime (fixtureName);
                            td.SetTime (t);
                            onTimeTextBox.text = td.ToShortString ();
                            onTimeTextBox.QueueDraw ();
                        }
                    } catch {
                        MessageBox.Show ("Incorrect time format");
                    }
                };

                numberInput.Run ();
                numberInput.Destroy ();
            };
            Put (btn, 525, 405);
            btn.Show ();

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

            btn = new TouchButton ();
            btn.SetSizeRequest (100, 60);
            btn.text = "Modify Off Time";
            btn.ButtonReleaseEvent += (o, args) => {
                TouchNumberInput numberInput;
                var parent = this.Toplevel as Gtk.Window;
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
                        if (t.CompareToTime (new Time ()) == -1) {
                            timeOk = false;
                            MessageBox.Show ("Can't modify time to before current time");
                        }

                        if (t.CompareToTime (Lighting.GetFixtureOnTime (fixtureName)) == -1) {
                            timeOk = false;
                            MessageBox.Show ("Can't modify off time to before on time");
                        }

                        if (timeOk) {
                            var td = Lighting.GetFixtureOffTime (fixtureName);
                            td.SetTime (t);
                            offTimeTextBox.text = td.ToShortString ();
                            offTimeTextBox.QueueDraw ();
                        }
                    } catch {
                        MessageBox.Show ("Incorrect time format");
                    }
                };

                numberInput.Run ();
                numberInput.Destroy ();
            };
            Put (btn, 635, 405);
            btn.Show ();

            var settingsBtn = new TouchButton ();
            settingsBtn.text = "Settings";
            settingsBtn.SetSizeRequest (100, 60);
            settingsBtn.ButtonReleaseEvent += (o, args) => {
                var s = new LightingSettings ();
                s.Run ();
                s.Destroy ();

                sunRise.text = Lighting.sunRiseToday.ToTimeString ();
                sunSet.text = Lighting.sunSetToday.ToTimeString ();
                sunRiseTomorrow.text = Lighting.sunRiseTomorrow.ToTimeString ();
                sunSetTomorrow.text = Lighting.sunSetTomorrow.ToTimeString ();
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
            fixtureSettingBtn.text = "Fixture Setup";
            fixtureSettingBtn.SetSizeRequest (100, 60);
            fixtureSettingBtn.ButtonReleaseEvent += (o, args) => {
                var s = new FixtureSettings (fixtureName, fixtureName.IsNotEmpty ());
                s.Run ();
                var newFixtureName = s.newOrUpdatedFixtureName;
                s.Destroy ();

                if ((newFixtureName != fixtureName) && fixtureName.IsNotEmpty ()) { // The fixture name was changed
                    var index = combo.comboList.IndexOf (fixtureName);
                    combo.comboList[index] = newFixtureName;
                    fixtureName = newFixtureName;
                } else if (Lighting.CheckFixtureKeyNoThrow (newFixtureName)) { // A new fixture was added
                    combo.comboList.Insert (combo.comboList.Count - 1, newFixtureName);
                    combo.activeText = newFixtureName;
                    fixtureName = newFixtureName;
                } else if (!Lighting.CheckFixtureKeyNoThrow (fixtureName)) { // The fixture was deleted
                    combo.comboList.Remove (fixtureName);
                    fixtureName = Lighting.defaultFixture;
                    combo.activeText = fixtureName;
                }  

                combo.QueueDraw ();
                GetFixtureData ();
            };
            Put (fixtureSettingBtn, 415, 405);
            fixtureSettingBtn.Show ();

            string[] names = Lighting.GetAllFixtureNames ();
            combo = new TouchComboBox (names);
            combo.active = 0;
            combo.WidthRequest = 235;
            combo.comboList.Add ("New fixture...");
            combo.ChangedEvent += OnComboChanged;
            Put (combo, 550, 77);
            combo.Show ();

            if (fixtureName.IsNotEmpty ()) {
                combo.activeText = fixtureName;
            } else {
                combo.active = 0;
            }

            GetFixtureData ();

            Show ();
        }

        public override void Dispose () {
            Power.RemoveHandlerOnStateChange (Lighting.GetFixtureOutletIndividualControl (fixtureName), OnOutletStateChange);
            GLib.Source.Remove (timerId);
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

                onTimeTextBox.text = Lighting.GetFixtureOnTime (fixtureName).ToShortString ();
                offTimeTextBox.text = Lighting.GetFixtureOffTime (fixtureName).ToShortString ();

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
                    // I think there's another way to do this but I can't remember what it is or if it ever works
                    combo.Visible = false;
                    combo.Visible = true;

                    timerId = GLib.Timeout.Add (1000, OnTimer);
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
            }
        }

        protected void OnComboChanged (object sender, ComboBoxChangedEventArgs e) {
            if (e.ActiveText == "New fixture...") {
                var s = new FixtureSettings (string.Empty, false);
                s.Run ();
                var newFixtureName = s.newOrUpdatedFixtureName;
                s.Destroy ();

                if (Lighting.CheckFixtureKeyNoThrow (newFixtureName)) { // A new fixture was added
                    combo.comboList.Insert (combo.comboList.Count - 1, newFixtureName);
                    combo.activeText = newFixtureName;
                    fixtureName = newFixtureName;
                }
            } else {
                fixtureName = e.ActiveText;
            }

            combo.QueueDraw ();
            GetFixtureData ();
        }

        protected bool OnTimer () {
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
            } else {
                return false;
            }
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