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
        public event SelectorChangedEventHandler TypeSelectorChangedEvent;

        public TouchLabel label;
        public TouchTextBox textBox;
        public TouchProgressBar progressBar;
        public TouchLabel typeLabel;
        public TouchButton button;
        public TouchSelectorSwitch ss;

        public int divisionSteps;

        public float currentValue {
            set {
                int v = value.ToInt ();
                textBox.text = v.ToString ("D");

                progressBar.currentProgress = value / divisionSteps;
            }
        }

        public AnalogChannelDisplay () {
            SetSizeRequest (760, 65);

            label = new TouchLabel ();
            Put (label, 5, 15);
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
            Put (textBox, 0, 35);
            textBox.Show ();

            progressBar = new TouchProgressBar (MyOrientation.Horizontal);
            progressBar.WidthRequest = 440;
            progressBar.ProgressChangedEvent += (sender, args) => {
                currentValue = args.currentProgress * (float)divisionSteps;
                ValueChanged ();
            };
            Put (progressBar, 210, 35);
            progressBar.Show ();

            typeLabel = new TouchLabel ();
            typeLabel.Visible = false;
            typeLabel.WidthRequest = 200;
            typeLabel.textAlignment = MyAlignment.Right;
            Put (typeLabel, 550, 15);

            button = new TouchButton ();
            button.SetSizeRequest (100, 30);
            button.buttonColor = "grey3";
            button.text = "Force";
            button.ButtonReleaseEvent += OnForceReleased;
            Put (button, 660, 35);
            button.Show ();

            ss = new TouchSelectorSwitch ();
            ss.SetSizeRequest (100, 30);
            ss.SliderColorOptions [0] = "pri";
            ss.SliderColorOptions [1] = "seca";
            ss.SelectorChangedEvent += OnSelectorSwitchChanged;
            ss.ExposeEvent += OnExpose;
            ss.Visible = false;
            Put (ss, 660, 0);

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

        protected void OnSelectorSwitchChanged (object sender, SelectorChangedEventArgs args) {
            if (TypeSelectorChangedEvent != null)
                TypeSelectorChangedEvent (this, args);
            else
                throw new NotImplementedException ("Type selector not impletemented");
        }

        protected void OnExpose (object sender, ExposeEventArgs args) {
            TouchSelectorSwitch ss = sender as TouchSelectorSwitch;
            int seperation = ss.Allocation.Width / ss.SelectionCount;
            int x = ss.Allocation.Left;

            MyText render = new MyText ();
            render.textWrap = MyTextWrap.Shrink;
            render.alignment = MyAlignment.Center;
            render.font.color = "white";

            string[] labels = {"0-10V", "PWM"};

            foreach (var l in labels) {
                render.text = l;
                render.Render (ss, x, ss.Allocation.Top, seperation, ss.Allocation.Height);
                x += seperation;
            }
        }
    }
}

