using System;
using Gtk;
using Cairo;
using MyWidgetLibrary;
using AquaPic.LightingModule;
using AquaPic.Globals;

namespace AquaPic
{
    public partial class LightingWindow : MyBackgroundWidget
    {
        private int fixtureID;
        private TouchComboBox combo;
        private LightingModeSlider modeSelector;
        private TouchLayeredProgressBar dimmingProgressBar;
        //private TouchProgressBar dimmingLevel;
        private TouchLabel dimmingHeader;
        private TouchTextBox dimmingTextBox;
        private TouchLabel dimmingLabel;
        private TouchTextBox autoTextBox;
        private TouchLabel autoLabel;
        private TouchTextBox requestTextBox;
        private TouchLabel requestLabel;
        private TouchTextBox onTimeTextBox;
        private TouchTextBox offTimeTextBox;
        private uint timer;
        private bool isDimmingFixture;
        private bool dimmingIsManual;

        public LightingWindow (MenuReleaseHandler OnMenuRelease) : base (2, OnMenuRelease) {
            #region base level screen stuff that doesn't change after draw
            MyBox box1 = new MyBox (385, 395);
            Put (box1, 10, 30);
            box1.Show ();

            MyBox box2 = new MyBox (385, 395);
            Put (box2, 405, 30);
            box2.Show ();

            MyBox box3 = new MyBox (195, 350);
            box3.color = "grey2";
            Put (box3, 410, 70);
            box3.Show ();

            MyBox box4 = new MyBox (175, 350);
            box4.color = "grey2";
            Put (box4, 610, 70);
            box4.Show ();

            TouchLabel fixtureLabel = new TouchLabel ();
            fixtureLabel.Text = "Lighting Fixtures";
            fixtureLabel.TextColor.ChangeColor ("pri");
            fixtureLabel.TextSize = 12;
            Put (fixtureLabel, 413, 40);
            fixtureLabel.Show ();

            TouchLabel onTimeLabel = new TouchLabel ();
            onTimeLabel.Text = "On Time";
            onTimeLabel.TextColor.ChangeColor ("grey4"); 
            Put (onTimeLabel, 415, 75);
            onTimeLabel.Show ();

            TouchLabel offTimeLabel = new TouchLabel ();
            offTimeLabel.Text = "Off Time";
            offTimeLabel.TextColor.ChangeColor ("grey4"); 
            Put (offTimeLabel, 415, 130);
            offTimeLabel.Show ();

            TouchLabel sunRiseLabel = new TouchLabel ();
            sunRiseLabel.Text = "Sunrise Today";
            sunRiseLabel.TextColor.ChangeColor ("grey4"); 
            Put (sunRiseLabel, 15, 39);
            sunRiseLabel.Show ();

            TouchTextBox sunRise = new TouchTextBox ();
            sunRise.WidthRequest = 200;
            sunRise.text = Lighting.sunRiseToday.ToString ();
            Put (sunRise, 190, 35);
            sunRise.Show ();

            TouchLabel sunSetLabel = new TouchLabel ();
            sunSetLabel.Text = "Sunset Today";
            sunSetLabel.TextColor.ChangeColor ("grey4"); 
            Put (sunSetLabel, 15, 74);
            sunSetLabel.Show ();

            TouchTextBox sunSet = new TouchTextBox ();
            sunSet.WidthRequest = 200;
            sunSet.text = Lighting.sunSetToday.ToString ();
            Put (sunSet, 190, 70);
            sunSet.Show ();

            TouchLabel sunRiseTomorrowLabel = new TouchLabel ();
            sunRiseTomorrowLabel.Text = "Sunrise Tomorrow";
            sunRiseTomorrowLabel.TextColor.ChangeColor ("grey4"); 
            Put (sunRiseTomorrowLabel, 15, 109);
            sunRiseTomorrowLabel.Show ();

            TouchTextBox sunRiseTomorrow = new TouchTextBox ();
            sunRiseTomorrow.WidthRequest = 200;
            sunRiseTomorrow.text = Lighting.sunRiseTomorrow.ToString ();
            Put (sunRiseTomorrow, 190, 105);
            sunRiseTomorrow.Show ();

            TouchLabel sunSetTomorrowLabel = new TouchLabel ();
            sunSetTomorrowLabel.Text = "Sunset Tomorrow";
            sunSetTomorrowLabel.TextColor.ChangeColor ("grey4"); 
            Put (sunSetTomorrowLabel, 15, 144);
            sunSetTomorrowLabel.Show ();

            TouchTextBox sunSetTomorrow = new TouchTextBox ();
            sunSetTomorrow.WidthRequest = 200;
            sunSetTomorrow.text = Lighting.sunSetTomorrow.ToString ();
            Put (sunSetTomorrow, 190, 140);
            sunSetTomorrow.Show ();
            #endregion

            fixtureID = 0;

            dimmingHeader = new TouchLabel ();
            dimmingHeader.TextAlignment = Justify.Center;
            dimmingHeader.AreaWidth = 165;
            dimmingHeader.TextColor.ChangeColor ("secb");
            Put (dimmingHeader, 615, 77);
            dimmingHeader.Show ();

            modeSelector = new LightingModeSlider ();
            modeSelector.SelectorChangedEvent += OnSelectorChanged;
            Put (modeSelector, 630, 105);
            modeSelector.Show ();

            dimmingProgressBar = new TouchLayeredProgressBar ();
            //dimmingLevel = new TouchProgressBar ();
            dimmingProgressBar.colorProgress.ChangeColor ("seca");
            dimmingProgressBar.colorProgressSecondary.ChangeColor ("pri");
            dimmingProgressBar.drawPrimaryWhenEqual = false;
            dimmingProgressBar.ProgressChangedEvent += OnProgressChanged;
            dimmingProgressBar.ProgressChangingEvent += OnProgressChanging;
            Put (dimmingProgressBar, 745, 145);
            dimmingProgressBar.Show ();

            dimmingTextBox = new TouchTextBox ();
            dimmingTextBox.textAlignment = Justify.Center;
            Put (dimmingTextBox, 633, 168);
            dimmingTextBox.Show ();

            dimmingLabel = new TouchLabel ();
            dimmingLabel.Text = "Current";
            dimmingLabel.TextColor.ChangeColor ("pri");
            dimmingLabel.AreaWidth = 125;
            dimmingLabel.TextAlignment = Justify.Right;
            Put (dimmingLabel, 608, 147);
            dimmingLabel.Show ();

            requestTextBox = new TouchTextBox ();
            requestTextBox.textAlignment = Justify.Center;
            Put (requestTextBox, 633, 231);
            requestTextBox.Show ();

            requestLabel = new TouchLabel ();
            requestLabel.Text = "Requested";
            requestLabel.TextColor.ChangeColor ("seca");
            requestLabel.TextColor.ModifyColor (1.45);
            requestLabel.AreaWidth = 125;
            requestLabel.TextAlignment = Justify.Right;
            requestLabel.WidthRequest = 125;
            requestLabel.HeightRequest = 25;
            Put (requestLabel, 608, 210);
            requestLabel.Show ();

            autoTextBox = new TouchTextBox ();
            autoTextBox.textAlignment = Justify.Center;
            Put (autoTextBox, 680, 385);
            autoTextBox.Show ();

            autoLabel = new TouchLabel ();
            autoLabel.Text = "Auto";
            autoLabel.TextColor.ChangeColor ("grey4");
            autoLabel.AreaWidth = 75;
            autoLabel.TextAlignment = Justify.Right;
            autoLabel.WidthRequest = 75;
            autoLabel.HeightRequest = 20;
            Put (autoLabel, 596, 389);
            autoLabel.Show ();

            onTimeTextBox = new TouchTextBox ();
            onTimeTextBox.WidthRequest = 185;
            Put (onTimeTextBox, 415, 95);
            onTimeTextBox.Show ();

            offTimeTextBox = new TouchTextBox ();
            offTimeTextBox.WidthRequest = 185;
            Put (offTimeTextBox, 415, 150);
            offTimeTextBox.Show ();

            string[] names = Lighting.GetAllFixtureNames ();
            combo = new TouchComboBox (names);
            combo.Active = fixtureID;
            combo.WidthRequest = 185;
            combo.ChangedEvent += OnComboChanged;
            Put (combo, 600, 35);
            combo.Show ();

            GetFixtureData ();

            Show ();
        }

        protected void GetFixtureData () {
            isDimmingFixture = Lighting.IsDimmingFixture (fixtureID);
            if (isDimmingFixture) {
                dimmingHeader.Text = "Dimming Control";

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
                    autoTextBox.Visible = false;
                    autoLabel.Visible = false;
                } else {
                    modeSelector.CurrentSelected = 0;
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

                timer = GLib.Timeout.Add (1000, OnTimer);
            } else {
                dimmingHeader.Text = "Dimming not available";
                dimmingIsManual = false;
                modeSelector.Visible = false;
                dimmingProgressBar.Visible = false;
                dimmingTextBox.Visible = false;
                dimmingLabel.Visible = false;
                autoTextBox.Visible = false;
                autoLabel.Visible = false;
                requestTextBox.Visible = false;
                requestLabel.Visible = false;
            }

            onTimeTextBox.text = Lighting.GetOnTime (fixtureID).ToString ();
            offTimeTextBox.text = Lighting.GetOffTime (fixtureID).ToString ();

            QueueDraw ();
        }

        protected void OnComboChanged (object sender, ComboBoxChangedEventArgs e) {
            int id = Lighting.GetLightIndex (e.ActiveText);
            if (id != -1) {
                fixtureID = id;
                GetFixtureData ();
            }
        }

        protected bool OnTimer () {
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

            return isDimmingFixture;
        }

        protected void OnSelectorChanged (object sender, SelectorChangedEventArgs args) {
            if (args.currentSelectedIndex == 0) {
                Lighting.SetMode (fixtureID, Mode.Manual);
                dimmingProgressBar.enableTouch = true;
                autoTextBox.Visible = true;
                autoLabel.Visible = true;
                dimmingIsManual = true;
                autoTextBox.text = string.Format ("{0:N2}", Lighting.GetAutoDimmingLevel (fixtureID));
            } else {
                Lighting.SetMode (fixtureID, Mode.Auto);
                dimmingProgressBar.enableTouch = false;
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

