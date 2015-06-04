using System;
using Gtk;
using Cairo;
using MyWidgetLibrary;
using AquaPic.Utilites;
using AquaPic.StateRuntime;
using AquaPic.TemperatureModule;
using AquaPic.LightingModule;
using AquaPic.TimerRuntime;

namespace AquaPic
{
    public class MainWindow : MyBackgroundWidget
    {
        TouchLinePlotWidget phPlot;
        TouchLinePlotWidget tempPlot;
        TouchBarPlotWidget waterLevel;
        TouchBarPlotWidget whiteLedDimming;
        TouchBarPlotWidget actinicLedDimming;

        public MainWindow () : base () {
            TouchButton b1 = new TouchButton ();
            b1.ButtonReleaseEvent += OnButtonClick;
            MyState s1 = ControllerState.Check ("Clean Skimmer");
            if (s1 == MyState.Set)
                b1.buttonColor = "pri";
            else
                b1.buttonColor = "grey3";
            b1.HeightRequest = 95;
            b1.WidthRequest = 108;
            b1.text = "Clean Skimmer";
            Put (b1, 572, 330);

            TouchButton b2 = new TouchButton ();
            b2.ButtonReleaseEvent += OnButtonClick;
            s1 = ControllerState.Check ("Water Change");
            if (s1 == MyState.Set)
                b2.buttonColor = "pri";
            else
                b2.buttonColor = "grey3";
            b2.HeightRequest = 95;
            b2.WidthRequest = 108;
            b2.text = "Water Change";
            Put (b2, 685, 330);

            phPlot = new TouchLinePlotWidget ();
            phPlot.text = "pH";
            Put (phPlot, 7, 30);

            tempPlot = new TouchLinePlotWidget ();
            tempPlot.text = "Temperature";
            tempPlot.currentValue = Temperature.WaterTemperature;
            Put (tempPlot, 7, 130);

            var box3 = new TouchLinePlotWidget ();
            box3.text = "thingy 1";
            Put (box3, 7, 230);

            var box4 = new TouchLinePlotWidget ();
            box4.text = "thingy 2";
            Put (box4, 7, 330);

            waterLevel = new TouchBarPlotWidget ();
            waterLevel.text = "Water Level";
            Put (waterLevel, 459, 30);

            whiteLedDimming = new TouchBarPlotWidget ();
            whiteLedDimming.text = "White LED";
            Put (whiteLedDimming, 572, 30);

            actinicLedDimming = new TouchBarPlotWidget ();
            actinicLedDimming.text = "Actinic LED";
            Put (actinicLedDimming, 685, 30);

            GLib.Timeout.Add (1000, OnUpdateTimer);

            ShowAll ();
        }

        protected void OnButtonClick (object sender, ButtonReleaseEventArgs args) {
            TouchButton b = sender as TouchButton;
            string stateText = b.text;
            MyState s = ControllerState.Check (stateText);
            if (s == MyState.Set) {
                ControllerState.Reset (stateText);
                b.buttonColor = "grey3";
            } else {
                ControllerState.Set (stateText);
                b.buttonColor = "pri";
            }
        }

        protected bool OnUpdateTimer () {
            tempPlot.currentValue = Temperature.WaterTemperature;
            tempPlot.QueueDraw ();

            whiteLedDimming.currentValue = Lighting.GetCurrentDimmingLevel (Lighting.GetLightIndex ("White LED"));
            whiteLedDimming.QueueDraw ();

            actinicLedDimming.currentValue = Lighting.GetCurrentDimmingLevel (Lighting.GetLightIndex ("Actinic LED"));
            actinicLedDimming.QueueDraw ();

            return true;
        }
    }
}

