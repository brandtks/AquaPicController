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
        private TouchButton fixtureSettingBtn;
        private uint timerId;
        private bool isDimmingFixture;
        private bool dimmingIsManual;

        public LightingWindow (params object[] options) : base () {
            #region base level screen stuff that doesn't change after draw
            TouchGraphicalBox box1 = new TouchGraphicalBox (385, 395);
            Put (box1, 10, 30);
            box1.Show ();

            TouchGraphicalBox box2 = new TouchGraphicalBox (385, 395);
            Put (box2, 405, 30);
            box2.Show ();

            TouchGraphicalBox box3 = new TouchGraphicalBox (205, 350);
            box3.color = "grey3";
            Put (box3, 410, 70);
            box3.Show ();

            TouchGraphicalBox box4 = new TouchGraphicalBox (165, 350);
            box4.color = "grey3";
            Put (box4, 620, 70);
            box4.Show ();

            TouchLabel fixtureLabel = new TouchLabel ();
            fixtureLabel.text = "Lighting Fixtures";
            fixtureLabel.textColor = "pri";
            fixtureLabel.textSize = 12;
            Put (fixtureLabel, 413, 40);
            fixtureLabel.Show ();

            TouchLabel genInfoLabel = new TouchLabel ();
            genInfoLabel.text = "General Lighting Information";
            genInfoLabel.WidthRequest = 370;
            genInfoLabel.textColor = "pri";
            genInfoLabel.textSize = 12;
            Put (genInfoLabel, 15, 40);
            genInfoLabel.Show ();

            TouchLabel sunRiseLabel = new TouchLabel ();
            sunRiseLabel.text = "Sunrise Today";
            sunRiseLabel.textColor = "grey4"; 
            Put (sunRiseLabel, 15, 74);
            sunRiseLabel.Show ();

            TouchTextBox sunRise = new TouchTextBox ();
            sunRise.WidthRequest = 200;
            sunRise.text = Lighting.sunRiseToday.ToString ();
            Put (sunRise, 190, 70);
            sunRise.Show ();

            TouchLabel sunSetLabel = new TouchLabel ();
            sunSetLabel.text = "Sunset Today";
            sunSetLabel.textColor = "grey4"; 
            Put (sunSetLabel, 15, 109);
            sunSetLabel.Show ();

            TouchTextBox sunSet = new TouchTextBox ();
            sunSet.WidthRequest = 200;
            sunSet.text = Lighting.sunSetToday.ToString ();
            Put (sunSet, 190, 105);
            sunSet.Show ();

            TouchLabel sunRiseTomorrowLabel = new TouchLabel ();
            sunRiseTomorrowLabel.text = "Sunrise Tomorrow";
            sunRiseTomorrowLabel.textColor = "grey4"; 
            Put (sunRiseTomorrowLabel, 15, 144);
            sunRiseTomorrowLabel.Show ();

            TouchTextBox sunRiseTomorrow = new TouchTextBox ();
            sunRiseTomorrow.WidthRequest = 200;
            sunRiseTomorrow.text = Lighting.sunRiseTomorrow.ToString ();
            Put (sunRiseTomorrow, 190, 140);
            sunRiseTomorrow.Show ();

            TouchLabel sunSetTomorrowLabel = new TouchLabel ();
            sunSetTomorrowLabel.text = "Sunset Tomorrow";
            sunSetTomorrowLabel.textColor = "grey4"; 
            Put (sunSetTomorrowLabel, 15, 179);
            sunSetTomorrowLabel.Show ();

            TouchTextBox sunSetTomorrow = new TouchTextBox ();
            sunSetTomorrow.WidthRequest = 200;
            sunSetTomorrow.text = Lighting.sunSetTomorrow.ToString ();
            Put (sunSetTomorrow, 190, 175);
            sunSetTomorrow.Show ();
            #endregion

            //<TODO> this is a stupid fix for when there are no lights add
            //will be changed after I implement adding and removing lights during runtime
            if (Lighting.fixtureCount == 0) {
                fixtureID = -1;
                fixtureLabel.text = "No lighing fixtures added";

                //combo = new TouchComboBox ();
                //combo.Active = 0;
                //combo.WidthRequest = 185;
                //combo.List.Add ("New fixture...");
                //combo.ChangedEvent += OnComboChanged;
                //Put (combo, 600, 35);
                //combo.Show ();
            } else {
                dimmingIsManual = false;
                fixtureID = 0;
            }

            dimmingHeader = new TouchLabel ();
            dimmingHeader.textAlignment = TouchAlignment.Center;
            dimmingHeader.WidthRequest = 165;
            dimmingHeader.textColor = "secb";
            Put (dimmingHeader, 615, 77);
            dimmingHeader.Show ();

            modeSelector = new ModeSelector ();
            modeSelector.SelectorChangedEvent += OnSelectorChanged;
            Put (modeSelector, 630, 105);
            modeSelector.Show ();

            dimmingProgressBar = new TouchLayeredProgressBar ();
            //dimmingLevel = new TouchProgressBar ();
            dimmingProgressBar.colorProgress = "seca";
            dimmingProgressBar.colorProgressSecondary = "pri";
            dimmingProgressBar.drawPrimaryWhenEqual = false;
            dimmingProgressBar.ProgressChangedEvent += OnProgressChanged;
            dimmingProgressBar.ProgressChangingEvent += OnProgressChanging;
            Put (dimmingProgressBar, 745, 145);
            dimmingProgressBar.Show ();

            dimmingTextBox = new TouchTextBox ();
            dimmingTextBox.textAlignment = TouchAlignment.Center;
            Put (dimmingTextBox, 633, 168);
            dimmingTextBox.Show ();

            dimmingLabel = new TouchLabel ();
            dimmingLabel.text = "Current";
            dimmingLabel.textColor = "pri";
            dimmingLabel.WidthRequest = 125;
            dimmingLabel.textAlignment = TouchAlignment.Right;
            Put (dimmingLabel, 608, 147);
            dimmingLabel.Show ();

            requestTextBox = new TouchTextBox ();
            requestTextBox.textAlignment = TouchAlignment.Center;
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
            Put (requestTextBox, 633, 231);
            requestTextBox.Show ();

            requestLabel = new TouchLabel ();
            requestLabel.text = "Requested";
            requestLabel.textColor = "seca";
            requestLabel.textColor.ModifyColor (1.45);
            requestLabel.WidthRequest = 125;
            requestLabel.textAlignment = TouchAlignment.Right;
            requestLabel.WidthRequest = 125;
            requestLabel.HeightRequest = 25;
            Put (requestLabel, 608, 210);
            requestLabel.Show ();

            autoTextBox = new TouchTextBox ();
            autoTextBox.textAlignment = TouchAlignment.Center;
            autoTextBox.Visible = false;
            Put (autoTextBox, 680, 385);
            autoTextBox.Show ();

            autoLabel = new TouchLabel ();
            autoLabel.text = "Auto";
            autoLabel.textColor = "grey4";
            autoLabel.textAlignment = TouchAlignment.Right;
            autoLabel.SetSizeRequest (75, 20);
            autoLabel.Visible = false;
            Put (autoLabel, 596, 389);
            autoLabel.Show ();

            onTimeLabel = new TouchLabel ();
            onTimeLabel.text = "On Time";
            onTimeLabel.textColor = "grey4"; 
            Put (onTimeLabel, 415, 75);
            onTimeLabel.Show ();

            onTimeTextBox = new TouchTextBox ();
            onTimeTextBox.WidthRequest = 195;
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
            Put (onTimeTextBox, 415, 95);
            onTimeTextBox.Show ();

            offTimeLabel = new TouchLabel ();
            offTimeLabel.text = "Off Time";
            offTimeLabel.textColor = "grey4";
            Put (offTimeLabel, 415, 130);
            offTimeLabel.Show ();

            offTimeTextBox = new TouchTextBox ();
            offTimeTextBox.WidthRequest = 195;
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
            Put (offTimeTextBox, 415, 150);
            offTimeTextBox.Show ();

            string[] names = Lighting.GetAllFixtureNames ();
            combo = new TouchComboBox (names);
            combo.Active = 0;
            combo.WidthRequest = 185;
            combo.List.Add ("New fixture...");
            combo.ChangedEvent += OnComboChanged;
            Put (combo, 600, 35);
            combo.Show ();

            var settingsBtn = new TouchButton ();
            settingsBtn.text = "Settings";
            settingsBtn.SetSizeRequest (100, 30);
            settingsBtn.ButtonReleaseEvent += (o, args) => {
                var s = new LightingSettings ();
                s.Run ();
                s.Destroy ();

                sunRise.text = Lighting.sunRiseToday.ToString ();
                sunSet.text = Lighting.sunSetToday.ToString ();
                sunRiseTomorrow.text = Lighting.sunRiseTomorrow.ToString ();
                sunSetTomorrow.text = Lighting.sunSetTomorrow.ToString ();
            };
            Put (settingsBtn, 15, 390);
            settingsBtn.Show ();

            fixtureSettingBtn = new TouchButton ();
            fixtureSettingBtn.text = "Fixture Setup";
            fixtureSettingBtn.SetSizeRequest (100, 30);
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
            Put (fixtureSettingBtn, 415, 385);
            fixtureSettingBtn.Show ();

            GetFixtureData ();

            Show ();
        }

        public override void Dispose () {
            GLib.Source.Remove (timerId);

            base.Dispose ();
        }

        protected void GetFixtureData () {
            if (fixtureID != -1) {
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

                onTimeTextBox.text = Lighting.GetFixtureOnTime (fixtureID).ToString ();
                onTimeTextBox.QueueDraw ();
                offTimeTextBox.text = Lighting.GetFixtureOffTime (fixtureID).ToString ();
                offTimeTextBox.QueueDraw ();

                QueueDraw ();
            } else {
                onTimeLabel.Visible = false;
                onTimeTextBox.Visible = false;
                offTimeLabel.Visible = false;
                offTimeTextBox.Visible = false;
                dimmingHeader.Visible = false;
                fixtureSettingBtn.Visible = false;

                modeSelector.Visible = false;
                dimmingProgressBar.Visible = false;
                dimmingTextBox.Visible = false;
                dimmingLabel.Visible = false;
                autoTextBox.Visible = false;
                autoLabel.Visible = false;
                requestTextBox.Visible = false;
                requestLabel.Visible = false;
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

                    onTimeLabel.Visible = true;
                    onTimeTextBox.Visible = true;
                    offTimeLabel.Visible = true;
                    offTimeTextBox.Visible = true;
                    dimmingHeader.Visible = true;
                    fixtureSettingBtn.Visible = true;

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
                        fixtureID = id;
                        GetFixtureData ();
                    }
                } catch {
                    ;
                }
            }
        }

        protected bool OnTimer () {
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
        }

        protected void OnSelectorChanged (object sender, SelectorChangedEventArgs args) {
            if (args.currentSelectedIndex == 0) {
                Lighting.SetDimmingMode (fixtureID, Mode.Manual);
                dimmingProgressBar.enableTouch = true;
                requestTextBox.enableTouch = true;
                autoTextBox.Visible = true;
                autoLabel.Visible = true;
                dimmingIsManual = true;
                autoTextBox.text = string.Format ("{0:N2}", Lighting.GetAutoDimmingLevel (fixtureID));
            } else {
                Lighting.SetDimmingMode (fixtureID, Mode.Auto);
                dimmingProgressBar.enableTouch = false;
                requestTextBox.enableTouch = false;
                autoTextBox.Visible = false;
                autoLabel.Visible = false;
                dimmingIsManual = false;
            }

            dimmingProgressBar.QueueDraw ();
            autoTextBox.QueueDraw ();
            autoLabel.QueueDraw ();
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
    }
}