using System;
using Gtk;
using Cairo;
using MyWidgetLibrary;
using AquaPic.Globals;
using AquaPic.StateRuntime;
using AquaPic.TemperatureModule;

namespace AquaPic
{
    public class MainWindow : MyBackgroundWidget
    {
        TouchPlot phPlot;
        TouchPlot tempPlot;

        public MainWindow (MenuReleaseHandler OnMenuRelease) : base (0, OnMenuRelease) {
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

            phPlot = new TouchPlot ();
            phPlot.text = "pH";
            Put (phPlot, 7, 30);

            tempPlot = new TouchPlot ();
            tempPlot.text = "Temperature";
            tempPlot.currentValue = Temperature.WaterTemperature;
            Put (tempPlot, 7, 130);

            var box3 = new TouchPlot ();
            box3.text = "thingy 1";
            Put (box3, 7, 230);

            var box4 = new TouchPlot ();
            box4.text = "thingy 2";
            Put (box4, 7, 330);

            var box5 = new MyBox (108, 195);
            Put (box5, 459, 30);

            var box6 = new MyBox (108, 195);
            Put (box6, 572, 30);

            var box7 = new MyBox (108, 195);
            Put (box7, 685, 30);

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

            return true;
        }
    }
}

