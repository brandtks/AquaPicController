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
        private int fixtureID;
        private TouchComboBox combo;
        private ModeSelector modeSelector;
        private TouchLayeredProgressBar dimmingProgressBar;
        private TouchLabel dimmingHeader;
        private TouchTextBox dimmingTextBox;
        private TouchLabel dimmingLabel;
        private TouchTextBox autoTextBox;
        private TouchLabel autoLabel;
        private TouchTextBox requestTextBox;
        private TouchLabel requestLabel;
        private TouchLabel onTimeLabel;
        private TouchTextBox onTimeTextBox;
        private TouchLabel offTimeLabel;
        private TouchTextBox offTimeTextBox;
        private TouchLabel outletLabel;
        private TouchLabel outletStateLabel;
        private TouchSelectorSwitch outletSelectorSwitch;
        private TouchButton fixtureSettingBtn;
        private uint timerId;
        private bool isDimmingFixture;
        private bool dimmingIsManual;

        public LightingWindow (params object[] options) : base () {
            #region base level screen stuff that doesn't change after draw
            //TouchGraphicalBox box1 = new TouchGraphicalBox (385, 395);
            //Put (box1, 10, 30);
            //box1.Show ();

            //TouchGraphicalBox box2 = new TouchGraphicalBox (385, 395);
            //Put (box2, 405, 30);
            //box2.Show ();

            //TouchGraphicalBox box3 = new TouchGraphicalBox (205, 350);
            //box3.color = "grey3";
            //Put (box3, 410, 70);
            //box3.Show ();

            //TouchGraphicalBox box4 = new TouchGraphicalBox (165, 350);
            //box4.color = "grey3";
            //Put (box4, 620, 70);
            //box4.Show ();

            screenTitle = "Lighting";

            TouchLabel fixtureLabel = new TouchLabel ();
            fixtureLabel.text = "Lighting Fixtures";
            fixtureLabel.textColor = "seca";
            fixtureLabel.textSize = 12;
            Put (fixtureLabel, 415, 80);
            fixtureLabel.Show ();

            TouchLabel genInfoLabel = new TouchLabel ();
            genInfoLabel.text = "General Lighting Information";
            genInfoLabel.textAlignment = TouchAlignment.Center;
            genInfoLabel.WidthRequest = 342;
            genInfoLabel.textColor = "seca";
            genInfoLabel.textSize = 12;
            Put (genInfoLabel, 60, 80);
            genInfoLabel.Show ();

            TouchLabel sunRiseLabel = new TouchLabel ();
            sunRiseLabel.text = "Sunrise Today";
            sunRiseLabel.textColor = "grey4"; 
            Put (sunRiseLabel, 60, 124);
            sunRiseLabel.Show ();

            TouchTextBox sunRise = new TouchTextBox ();
            sunRise.WidthRequest = 155;
            sunRise.text = Lighting.sunRiseToday.ToString ();
            Put (sunRise, 235, 120);
            sunRise.Show ();

            TouchLabel sunSetLabel = new TouchLabel ();
            sunSetLabel.text = "Sunset Today";
            sunSetLabel.textColor = "grey4"; 
            Put (sunSetLabel, 60, 159);
            sunSetLabel.Show ();

            TouchTextBox sunSet = new TouchTextBox ();
            sunSet.WidthRequest = 155;
            sunSet.text = Lighting.sunSetToday.ToString ();
            Put (sunSet, 235, 155);
            sunSet.Show ();

            TouchLabel sunRiseTomorrowLabel = new TouchLabel ();
            sunRiseTomorrowLabel.text = "Sunrise Tomorrow";
            sunRiseTomorrowLabel.textColor = "grey4"; 
            Put (sunRiseTomorrowLabel, 60, 194);
            sunRiseTomorrowLabel.Show ();

            TouchTextBox sunRiseTomorrow = new TouchTextBox ();
            sunRiseTomorrow.WidthRequest = 155;
            sunRiseTomorrow.text = Lighting.sunRiseTomorrow.ToString ();
            Put (sunRiseTomorrow, 235, 190);
            sunRiseTomorrow.Show ();

            TouchLabel sunSetTomorrowLabel = new TouchLabel ();
            sunSetTomorrowLabel.text = "Sunset Tomorrow";
            sunSetTomorrowLabel.textColor = "grey4"; 
            Put (sunSetTomorrowLabel, 60, 229);
            sunSetTomorrowLabel.Show ();

            TouchTextBox sunSetTomorrow = new TouchTextBox ();
            sunSetTomorrow.WidthRequest = 155;
            sunSetTomorrow.text = Lighting.sunSetTomorrow.ToString ();
            Put (sunSetTomorrow, 235, 225);
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

            #endregion

            if (Lighting.fixtureCount == 0) {
                fixtureID = -1;
                fixtureLabel.text = "No lighing fixtures added";
            } else {
                dimmingIsManual = false;
                fixtureID = 0;
            }

            dimmingHeader = new TouchLabel ();
            dimmingHeader.textAlignment = TouchAlignment.Center;
            dimmingHeader.WidthRequest = 165;
            dimmingHeader.textColor = "secb";
            Put (dimmingHeader, 615, 117);
            dimmingHeader.Show ();

            modeSelector = new ModeSelector ();
            modeSelector.SelectorChangedEvent += OnDimmingModeSelectorChanged;
            Put (modeSelector, 630, 145);
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

            dimmingTextBox = new TouchTextBox ();
            dimmingTextBox.textAlignment = TouchAlignment.Center;
            dimmingTextBox.WidthRequest = 110;
            Put (dimmingTextBox, 623, 208);
            dimmingTextBox.Show ();

            dimmingLabel = new TouchLabel ();
            dimmingLabel.text = "Current";
            dimmingLabel.textColor = "pri";
            dimmingLabel.WidthRequest = 125;
            dimmingLabel.textAlignment = TouchAlignment.Right;
            Put (dimmingLabel, 608, 187);
            dimmingLabel.Show ();

            requestTextBox = new TouchTextBox ();
            requestTextBox.textAlignment = TouchAlignment.Center;
            requestTextBox.WidthRequest = 110;
            requestTextBox.TextChangedEvent += (sender, args) => {
                try {
                    float newLevel = Convert.ToSingle (args.text);
                    if (newLevel < 0.0f)
                        newLevel = 0.0f;
                    if (newLevel > 100.0f)
                        newLevel = 100.0f;
                    Lighting.SetDimmingLevel (fixtureID, newLevel);
                } catch (Exception ex) {
                    MessageBox.Show (ex.ToString ());
                }
            };
            Put (requestTextBox, 623, 271);
            requestTextBox.Show ();

            requestLabel = new TouchLabel ();
            requestLabel.text = "Requested";
            requestLabel.textColor = "seca";
            requestLabel.textColor.ModifyColor (1.45);
            requestLabel.WidthRequest = 125;
            requestLabel.textAlignment = TouchAlignment.Right;
            requestLabel.WidthRequest = 125;
            requestLabel.HeightRequest = 25;
            Put (requestLabel, 608, 250);
            requestLabel.Show ();

            autoTextBox = new TouchTextBox ();
            autoTextBox.textAlignment = TouchAlignment.Center;
            autoTextBox.Visible = false;
            autoTextBox.WidthRequest = 110;
            Put (autoTextBox, 623, 334);
            autoTextBox.Show ();

            autoLabel = new TouchLabel ();
            autoLabel.text = "Auto";
            autoLabel.textColor = "grey4";
            autoLabel.textAlignment = TouchAlignment.Right;
            autoLabel.WidthRequest = 125;
            autoLabel.HeightRequest = 25;
            autoLabel.Visible = false;
            Put (autoLabel, 608, 313);
            autoLabel.Show ();

            onTimeLabel = new TouchLabel ();
            onTimeLabel.text = "On Time";
            onTimeLabel.textColor = "grey4"; 
            Put (onTimeLabel, 415, 124);
            onTimeLabel.Show ();

            onTimeTextBox = new TouchTextBox ();
            onTimeTextBox.WidthRequest = 185;
            onTimeTextBox.enableTouch = true;
            onTimeTextBox.includeTimeFunctions = true;
            onTimeTextBox.TextChangedEvent += (sender, args) => {
                try {
                    var t = Time.Parse (args.text);

                    if (t.CompareToTime (new Time ()) == 1) {
                        var td = Lighting.GetFixtureOnTime (fixtureID);
                        td.SetTime (t);
                        args.text = td.ToString ();
                    } else {
                        MessageBox.Show ("Can modify time to before current time");
                        args.keepText = false;
                    }
                } catch {
                    MessageBox.Show ("Incorrect time format");
                    args.keepText = false;
                }
            };
            Put (onTimeTextBox, 415, 145);
            onTimeTextBox.Show ();

            offTimeLabel = new TouchLabel ();
            offTimeLabel.text = "Off Time";
            offTimeLabel.textColor = "grey4";
            Put (offTimeLabel, 415, 187);
            offTimeLabel.Show ();

            offTimeTextBox = new TouchTextBox ();
            offTimeTextBox.WidthRequest = 185;
            offTimeTextBox.enableTouch = true;
            offTimeTextBox.includeTimeFunctions = true;
            offTimeTextBox.TextChangedEvent += (sender, args) => {
                try {
                    var t = Time.Parse (args.text);

                    if (t.CompareToTime (new Time ()) == 1) {
                        var td = Lighting.GetFixtureOffTime (fixtureID);
                        td.SetTime (t);
                        args.text = td.ToString ();
                    } else {
                        MessageBox.Show ("Can modify time to before current time");
                        args.keepText = false;
                    }
                } catch {
                    MessageBox.Show ("Incorrect time format");
                    args.keepText = false;
                }
            };
            Put (offTimeTextBox, 415, 208);
            offTimeTextBox.Show ();

            string[] names = Lighting.GetAllFixtureNames ();
            combo = new TouchComboBox (names);
            combo.Active = 0;
            combo.WidthRequest = 235;
            combo.List.Add ("New fixture...");
            combo.ChangedEvent += OnComboChanged;
            Put (combo, 550, 77);
            combo.Show ();

            var settingsBtn = new TouchButton ();
            settingsBtn.text = "Settings";
            settingsBtn.SetSizeRequest (100, 60);
            settingsBtn.ButtonReleaseEvent += (o, args) => {
                var s = new LightingSettings ();
                s.Run ();
                s.Destroy ();

                sunRise.text = Lighting.sunRiseToday.ToString ();
                sunSet.text = Lighting.sunSetToday.ToString ();
                sunRiseTomorrow.text = Lighting.sunRiseTomorrow.ToString ();
                sunSetTomorrow.text = Lighting.sunSetTomorrow.ToString ();
            };
            Put (settingsBtn, 290, 405);
            settingsBtn.Show ();

            outletLabel = new TouchLabel ();
            outletLabel.text = "Outlet:";
            outletLabel.textColor = "grey4";
            Put (outletLabel, 415, 250);
            outletLabel.Show ();

            outletStateLabel = new TouchLabel ();
            Put (outletStateLabel, 463, 250);
            outletStateLabel.Show ();

            outletSelectorSwitch = new TouchSelectorSwitch (0, 3, 0, TouchOrientation.Horizontal);
            outletSelectorSwitch.SliderSize = MySliderSize.Large;
            outletSelectorSwitch.WidthRequest = 185;
            outletSelectorSwitch.HeightRequest = 30;
            outletSelectorSwitch.SliderColorOptions [0] = "grey2";
            outletSelectorSwitch.SliderColorOptions [1] = "pri";
            outletSelectorSwitch.SliderColorOptions [2] = "seca";
            outletSelectorSwitch.TextOptions = new string[] {"Off", "Auto", "On"};
            outletSelectorSwitch.SelectorChangedEvent += OnOutletControlSelectorChanged;
            Put (outletSelectorSwitch, 415, 271);
            outletSelectorSwitch.Show ();

            fixtureSettingBtn = new TouchButton ();
            fixtureSettingBtn.text = "Fixture Setup";
            fixtureSettingBtn.SetSizeRequest (100, 60);
            fixtureSettingBtn.ButtonReleaseEvent += (o, args) => {
                if (fixtureID != -1) {
                    string name = Lighting.GetFixtureName (fixtureID);
                    var s = new FixtureSettings (name, fixtureID, true);
                    s.Run ();
                    s.Destroy ();

                    try {
                        Lighting.GetFixtureIndex (name);
                    } catch (ArgumentException) {
                        combo.List.Remove (name);
                        if (Lighting.fixtureCount != 0) {
                            fixtureID = 0;
                            combo.Active = fixtureID;
                            GetFixtureData ();
                        } else {
                            fixtureID = -1;
                            combo.Active = 0;

                            fixtureLabel.text = "No lighing fixtures added";
                            fixtureLabel.QueueDraw ();

                            GetFixtureData ();
                        }
                    }
                } else {
                    int fixtureCount = Lighting.fixtureCount;

                    var s = new FixtureSettings ("New fixture", -1, false);
                    s.Run ();
                    s.Destroy ();

                    if (Lighting.fixtureCount > fixtureCount) { // a fixture was added
                        fixtureID = Lighting.fixtureCount - 1;
                        int listIdx = combo.List.IndexOf ("New fixture...");
                        combo.List.Insert (listIdx, Lighting.GetFixtureName (fixtureID));
                        combo.Active = listIdx;
                        combo.QueueDraw ();
                        GetFixtureData ();
                    } else {
                        if (fixtureID != -1)
                            combo.Active = fixtureID;
                        else
                            combo.Active = 0;
                    }
                }

                combo.QueueDraw ();
            };
            Put (fixtureSettingBtn, 415, 405);
            fixtureSettingBtn.Show ();

            GetFixtureData ();

            Show ();
        }

        public override void Dispose () {
            Power.RemoveHandlerOnStateChange (Lighting.GetFixtureOutletIndividualControl (fixtureID), OnOutletStateChange);
            GLib.Source.Remove (timerId);
            base.Dispose ();
        }

        protected void GetFixtureData () {
            if (fixtureID != -1) {
                onTimeLabel.Visible = true;
                onTimeTextBox.Visible = true;
                offTimeLabel.Visible = true;
                offTimeTextBox.Visible = true;
                dimmingHeader.Visible = true;
                outletLabel.Visible = true;
                outletStateLabel.Visible = true;

                onTimeTextBox.text = Lighting.GetFixtureOnTime (fixtureID).ToString ();
                offTimeTextBox.text = Lighting.GetFixtureOffTime (fixtureID).ToString ();

                IndividualControl ic = Lighting.GetFixtureOutletIndividualControl (fixtureID);
                Power.AddHandlerOnStateChange (ic, OnOutletStateChange);

                if (Power.GetOutletMode (ic) == Mode.Auto) {
                    outletSelectorSwitch.CurrentSelected = 1;
                    outletSelectorSwitch.QueueDraw ();
                } else {
                    if (Power.GetOutletManualState (ic) == MyState.Off) {
                        outletSelectorSwitch.CurrentSelected = 0;
                    } else {
                        outletSelectorSwitch.CurrentSelected = 2;
                    }
                }

                if (Power.GetOutletState (ic) == MyState.Off) {
                    outletStateLabel.text = "Off";
                    outletStateLabel.textColor = "grey4";
                } else {
                    outletStateLabel.text = "On";
                    outletStateLabel.textColor = "secb";
                }

                isDimmingFixture = Lighting.IsDimmingFixture (fixtureID);
                if (isDimmingFixture) {
                    dimmingHeader.text = "Dimming Control";

                    modeSelector.Visible = true;
                    dimmingProgressBar.Visible = true;
                    dimmingTextBox.Visible = true;
                    dimmingLabel.Visible = true;
                    requestTextBox.Visible = true;
                    requestLabel.Visible = true;

                    Mode m = Lighting.GetDimmingMode (fixtureID);
                    dimmingIsManual = m == Mode.Manual;
                    if (!dimmingIsManual) {
                        modeSelector.CurrentSelected = 1;
                        dimmingProgressBar.enableTouch = false;
                        requestTextBox.enableTouch = false;
                        autoTextBox.Visible = false;
                        autoLabel.Visible = false;
                    } else {
                        modeSelector.CurrentSelected = 0;
                        dimmingProgressBar.enableTouch = true;
                        requestTextBox.enableTouch = true;
                        autoTextBox.Visible = true;
                        autoLabel.Visible = true;
                        autoTextBox.text = string.Format ("{0:N2}", Lighting.GetAutoDimmingLevel (fixtureID));
                    }

                    float level = Lighting.GetCurrentDimmingLevel (fixtureID);
                    dimmingProgressBar.currentProgressSecondary = level / 100.0f;
                    dimmingTextBox.text = string.Format ("{0:N2}", level);

                    level = Lighting.GetRequestedDimmingLevel (fixtureID);
                    dimmingProgressBar.currentProgress = level / 100.0f;
                    requestTextBox.text = string.Format ("{0:N2}", level);

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
                    dimmingTextBox.Visible = false;
                    dimmingLabel.Visible = false;
                    autoTextBox.Visible = false;
                    autoLabel.Visible = false;
                    requestTextBox.Visible = false;
                    requestLabel.Visible = false;
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
                dimmingTextBox.Visible = false;
                dimmingLabel.Visible = false;
                autoTextBox.Visible = false;
                autoLabel.Visible = false;
                requestTextBox.Visible = false;
                requestLabel.Visible = false;
                outletLabel.Visible = false;
                outletStateLabel.Visible = false;
                outletSelectorSwitch.Visible = false;
            }
        }

        protected void OnComboChanged (object sender, ComboBoxChangedEventArgs e) {
            if (e.ActiveText == "New fixture...") {
                int fixtureCount = Lighting.fixtureCount;

                var s = new FixtureSettings ("New fixture", -1, false);
                s.Run ();
                s.Destroy ();

                if (Lighting.fixtureCount > fixtureCount) { // a fixture was added
                    fixtureID = Lighting.fixtureCount - 1;
                    int listIdx = combo.List.IndexOf ("New fixture...");
                    combo.List.Insert (listIdx, Lighting.GetFixtureName (fixtureID));
                    combo.Active = listIdx;
                    combo.QueueDraw ();

                    GetFixtureData ();
                } else {
                    if (fixtureID != -1)
                        combo.Active = fixtureID;
                    else
                        combo.Active = 0;
                }
            } else {
                try {
                    int id = Lighting.GetFixtureIndex (e.ActiveText);
                    if (id != -1) {
                        Power.RemoveHandlerOnStateChange (Lighting.GetFixtureOutletIndividualControl (fixtureID), OnOutletStateChange);
                        fixtureID = id;
                        GetFixtureData ();
                    }
                } catch {
                    ;
                }
            }
        }

        protected bool OnTimer () {
            if (fixtureID != -1) {
                if (isDimmingFixture) {
                    float level = Lighting.GetCurrentDimmingLevel (fixtureID);
                    dimmingProgressBar.currentProgressSecondary = level / 100.0f;
                    dimmingTextBox.text = string.Format ("{0:N2}", level);

                    level = Lighting.GetRequestedDimmingLevel (fixtureID);
                    dimmingProgressBar.currentProgress = level / 100.0f;
                    requestTextBox.text = string.Format ("{0:N2}", level);

                    dimmingTextBox.QueueDraw ();
                    requestTextBox.QueueDraw ();
                    dimmingProgressBar.QueueDraw ();

                    if (dimmingIsManual) {
                        autoTextBox.text = string.Format ("{0:N2}", Lighting.GetAutoDimmingLevel (fixtureID));
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
                Lighting.SetDimmingMode (fixtureID, Mode.Manual);
                IndividualControl ic = Lighting.GetFixtureOutletIndividualControl (fixtureID);
                Power.SetOutletManualState (ic, Power.GetOutletState (ic));
                Power.SetOutletMode (ic, Mode.Manual);
                dimmingProgressBar.enableTouch = true;
                requestTextBox.enableTouch = true;
                autoTextBox.Visible = true;
                autoLabel.Visible = true;
                dimmingIsManual = true;
                autoTextBox.text = string.Format ("{0:N2}", Lighting.GetAutoDimmingLevel (fixtureID));
                if (Power.GetOutletState (ic) == MyState.Off) {
                    outletSelectorSwitch.CurrentSelected = 0;
                } else {
                    outletSelectorSwitch.CurrentSelected = 2;
                }
                outletSelectorSwitch.QueueDraw ();
            } else {
                Lighting.SetDimmingMode (fixtureID, Mode.Auto);
                Power.SetOutletMode (Lighting.GetFixtureOutletIndividualControl (fixtureID), Mode.Auto);
                dimmingProgressBar.enableTouch = false;
                requestTextBox.enableTouch = false;
                autoTextBox.Visible = false;
                autoLabel.Visible = false;
                dimmingIsManual = false;
                outletSelectorSwitch.CurrentSelected = 1;
                outletSelectorSwitch.QueueDraw ();
            }

            dimmingProgressBar.QueueDraw ();
            autoTextBox.QueueDraw ();
            autoLabel.QueueDraw ();
        }

        protected void OnOutletControlSelectorChanged (object sender, SelectorChangedEventArgs args) {
            IndividualControl ic = Lighting.GetFixtureOutletIndividualControl (fixtureID);

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
            Lighting.SetDimmingLevel (fixtureID, args.currentProgress * 100.0f);
            float level = Lighting.GetCurrentDimmingLevel (fixtureID);
            dimmingTextBox.text = string.Format ("{0:N2}", level);
            dimmingTextBox.QueueDraw ();
        }

        protected void OnProgressChanging (object sender, ProgressChangeEventArgs args) {
            requestTextBox.text = string.Format ("{0:N2}", args.currentProgress * 100.0f);
            requestTextBox.QueueDraw ();
        }

        protected void OnOutletStateChange (object sender, StateChangeEventArgs args) {
            IndividualControl ic = Lighting.GetFixtureOutletIndividualControl (fixtureID);
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