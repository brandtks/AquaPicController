using System;
using Gtk;
using Cairo;
using MyWidgetLibrary;
using AquaPic.Globals;
using AquaPic.StateRuntime;

namespace AquaPic
{
    public class MainWindow : MyBackgroundWidget
    {
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
            Put (b1, 572, 335);
            b1.Show ();

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
            Put (b2, 685, 335);
            b2.Show ();

            Show ();
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
    }
}

