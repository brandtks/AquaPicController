using System;
using Gtk;
using Cairo;
using MyWidgetLibrary;
using AquaPic.Drivers;
using AquaPic.Modules;
using AquaPic.Runtime;
using AquaPic.SerialBus;
using AquaPic.Utilites;

namespace AquaPic
{
    public class MainWindow : MyBackgroundWidget
    {
        TouchLinePlotWidget phPlot;
        TouchLinePlotWidget tempPlot;
        TouchBarPlotWidget waterLevel;
        TouchBarPlotWidget whiteLedDimming;
        TouchBarPlotWidget actinicLedDimming;

        uint timerId;

//        int testAlarmIndex;

        public MainWindow (params object[] options) : base () {
            TouchButton b1 = new TouchButton ();
            b1.ButtonReleaseEvent += OnButtonClick;
            MyState s1 = Bit.Check ("Clean Skimmer");
            if (s1 == MyState.Set)
                b1.buttonColor = "pri";
            else
                b1.buttonColor = "seca";
            b1.HeightRequest = 95;
            b1.WidthRequest = 108;
            b1.text = "Clean Skimmer";
            Put (b1, 572, 330);

            TouchButton b2 = new TouchButton ();
            b2.ButtonReleaseEvent += OnButtonClick;
            s1 = Bit.Check ("Water Change");
            if (s1 == MyState.Set)
                b2.buttonColor = "pri";
            else
                b2.buttonColor = "seca";
            b2.HeightRequest = 95;
            b2.WidthRequest = 108;
            b2.text = "Water Change";
            Put (b2, 685, 330);

//            var b3 = new TouchButton ();
//            b3.ButtonReleaseEvent += OnTestButtonClick;
//            b3.HeightRequest = 95;
//            b3.WidthRequest = 108;
//            b3.text = "Test Alarm";
//            testAlarmIndex = Alarm.Subscribe ("Test Alarm", true);
//            Put (b3, 459, 330);

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

            var t = new DeluxeTimerWidget ("Main");
            Put (t, 459, 230);

            OnUpdateTimer ();

            timerId = GLib.Timeout.Add (1000, OnUpdateTimer);

            ShowAll ();
        }

        public override void Dispose () {
            GLib.Source.Remove (timerId);
            base.Dispose ();
        }

        protected void OnButtonClick (object sender, ButtonReleaseEventArgs args) {
            TouchButton b = sender as TouchButton;
            string stateText = b.text;
            MyState s = Bit.Check (stateText);
            if (s == MyState.Set) {
                Bit.Reset (stateText);
                b.buttonColor = "seca";
            } else {
                Bit.Set (stateText);
                b.buttonColor = "pri";
            }
        }

//        protected void OnTestButtonClick (object sender, ButtonReleaseEventArgs args) {
//            MyState s = Bit.Toggle ("Test Alarm");
//
//            if (s == MyState.Set)
//                Alarm.Post (testAlarmIndex);
//            else
//                Alarm.Clear (testAlarmIndex);
//        }

        protected bool OnUpdateTimer () {
            tempPlot.currentValue = Temperature.WaterTemperature;
            tempPlot.QueueDraw ();

            whiteLedDimming.currentValue = Lighting.GetCurrentDimmingLevel (Lighting.GetLightIndex ("White LED"));
            whiteLedDimming.QueueDraw ();

            actinicLedDimming.currentValue = Lighting.GetCurrentDimmingLevel (Lighting.GetLightIndex ("Actinic LED"));
            actinicLedDimming.QueueDraw ();

            BaseScript bs = Plugin.AllPlugins ["WaterLevel"];
            waterLevel.currentValue = Convert.ToSingle (bs.OneShotRun ());
            waterLevel.QueueDraw ();

            return true;
        }
    }
}

