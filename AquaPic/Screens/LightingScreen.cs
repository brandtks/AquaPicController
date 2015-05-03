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
        private TouchProgressBar dimmingLevel;
        private uint timer;
        private bool isDimmingFixture;

        public LightingWindow (MenuReleaseHandler OnMenuRelease) : base (2, OnMenuRelease) {
            #region base level screen stuff that doesn't change after draw
            MyBox box1 = new MyBox (385, 395);
            Put (box1, 10, 30);
            box1.Show ();

            MyBox box2 = new MyBox (385, 395);
            Put (box2, 405, 30);
            box2.Show ();

            MyBox box3 = new MyBox (185, 385);
            box3.color = "grey2";
            Put (box3, 410, 35);
            box3.Show ();

            MyBox box4 = new MyBox (185, 350);
            box4.color = "grey2";
            Put (box4, 600, 70);
            box4.Show ();

            TouchLabel sunRiseLabel = new TouchLabel ();
            sunRiseLabel.Text = "Sunrise Today";
            sunRiseLabel.TextColor.ChangeColor ("grey4"); 
            Put (sunRiseLabel, 15, 39);
            sunRiseLabel.Show ();

            TouchTextBox sunRise = new TouchTextBox ();
            sunRise.WidthRequest = 200;
            sunRise.Text = Lighting.sunRiseToday.ToString ();
            Put (sunRise, 190, 35);
            sunRise.Show ();

            TouchLabel sunSetLabel = new TouchLabel ();
            sunSetLabel.Text = "Sunset Today";
            sunSetLabel.TextColor.ChangeColor ("grey4"); 
            Put (sunSetLabel, 15, 74);
            sunSetLabel.Show ();

            TouchTextBox sunSet = new TouchTextBox ();
            sunSet.WidthRequest = 200;
            sunSet.Text = Lighting.sunSetToday.ToString ();
            Put (sunSet, 190, 70);
            sunSet.Show ();

            TouchLabel sunRiseTomorrowLabel = new TouchLabel ();
            sunRiseTomorrowLabel.Text = "Sunrise Tomorrow";
            sunRiseTomorrowLabel.TextColor.ChangeColor ("grey4"); 
            Put (sunRiseTomorrowLabel, 15, 109);
            sunRiseTomorrowLabel.Show ();

            TouchTextBox sunRiseTomorrow = new TouchTextBox ();
            sunRiseTomorrow.WidthRequest = 200;
            sunRiseTomorrow.Text = Lighting.sunRiseTomorrow.ToString ();
            Put (sunRiseTomorrow, 190, 105);
            sunRiseTomorrow.Show ();

            TouchLabel sunSetTomorrowLabel = new TouchLabel ();
            sunSetTomorrowLabel.Text = "Sunset Tomorrow";
            sunSetTomorrowLabel.TextColor.ChangeColor ("grey4"); 
            Put (sunSetTomorrowLabel, 15, 144);
            sunSetTomorrowLabel.Show ();

            TouchTextBox sunSetTomorrow = new TouchTextBox ();
            sunSetTomorrow.WidthRequest = 200;
            sunSetTomorrow.Text = Lighting.sunSetTomorrow.ToString ();
            Put (sunSetTomorrow, 190, 140);
            sunSetTomorrow.Show ();
            #endregion

            fixtureID = 0;

            modeSelector = new LightingModeSlider ();
            Put (modeSelector, 625, 75);
            modeSelector.Show ();

            dimmingLevel = new TouchProgressBar ();
            Put (dimmingLevel, 415, 75);
            dimmingLevel.Show ();

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
                modeSelector.Visible = true;
                dimmingLevel.Visible = true;

                Mode m = Lighting.GetDimmingMode (fixtureID);
                if (m == Mode.Auto)
                    modeSelector.CurrentSelected = 1;
                else
                    modeSelector.CurrentSelected = 0;

                float level = Lighting.GetCurrentDimmingLevel (fixtureID);
                dimmingLevel.CurrentProgress = level / 100.0f;

                timer = GLib.Timeout.Add (1000, OnTimer);
            } else {
                modeSelector.Visible = false;
                dimmingLevel.Visible = false;
            }

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
            dimmingLevel.CurrentProgress = level / 100.0f;
            dimmingLevel.QueueDraw ();
            return isDimmingFixture;
        }
    }
}

