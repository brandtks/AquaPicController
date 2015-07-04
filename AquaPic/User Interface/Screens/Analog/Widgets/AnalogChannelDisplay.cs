using System;
using Gtk;
using Cairo;
using MyWidgetLibrary;
using AquaPic.Utilites;

namespace AquaPic.UserInterface
{
    public delegate void ValueChangedHandler (object sender, float value);

    public class AnalogChannelDisplay : Fixed
    {
        public event ButtonReleaseEventHandler ForceButtonReleaseEvent;
        public event ValueChangedHandler ValueChangedEvent;

        public TouchLabel label;
        public TouchTextBox textBox;
        public TouchProgressBar progressBar;
        public TouchLabel typeLabel;
        public TouchButton button;

        public int divisionSteps;

        public float currentValue {
            set {
                int v = value.ToInt ();
                textBox.text = v.ToString ("D");

                progressBar.currentProgress = value / divisionSteps;
            }
        }

        public AnalogChannelDisplay () {
            SetSizeRequest (760, 50);

            label = new TouchLabel ();
            Put (label, 5, 0);
            label.Show ();

            textBox = new TouchTextBox ();
            textBox.WidthRequest = 200; 
            textBox.TextChangedEvent += (sender, args) => {
                try {
                    currentValue = Convert.ToSingle (args.text);
                    ValueChanged ();
                } catch {
                    ;
                }
            };
            Put (textBox, 0, 20);
            textBox.Show ();

            progressBar = new TouchProgressBar (MyOrientation.Horizontal);
            progressBar.WidthRequest = 440;
            progressBar.ProgressChangedEvent += (sender, args) => {
                currentValue = args.currentProgress * (float)divisionSteps;
                ValueChanged ();
            };
            Put (progressBar, 210, 20);
            progressBar.Show ();

            typeLabel = new TouchLabel ();
            typeLabel.Visible = false;
            typeLabel.WidthRequest = 200;
            typeLabel.textAlignment = MyAlignment.Right;
            Put (typeLabel, 550, 0);

            button = new TouchButton ();
            button.SetSizeRequest (100, 30);
            button.buttonColor = "grey3";
            button.text = "Force";
            button.ButtonReleaseEvent += OnForceReleased;
            Put (button, 660, 20);
            button.Show ();

            Show ();
        }

        protected void OnForceReleased (object sender, ButtonReleaseEventArgs args) {
            if (ForceButtonReleaseEvent != null)
                ForceButtonReleaseEvent (this, args);
            else
                throw new NotImplementedException ("Force button release not implemented");
        }

        protected void ValueChanged () {
            if (ValueChangedEvent != null)
                ValueChangedEvent (this, progressBar.currentProgress * (float)divisionSteps);
            else
                throw new NotImplementedException ("Value changed not implemented");
        }
    }
}

