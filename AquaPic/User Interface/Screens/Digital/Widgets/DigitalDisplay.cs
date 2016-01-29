using System;
using Gtk;
using Cairo;
using TouchWidgetLibrary;

namespace AquaPic.UserInterface
{
    public class DigitalDisplay : Fixed
    {
        public event ButtonReleaseEventHandler ForceButtonReleaseEvent;
        public event SelectorChangedEventHandler StateSelectedChangedEvent;

        public TouchLabel label;
        public TouchTextBox textBox;
        public TouchButton button;
        public ModeSelector selector;

        public DigitalDisplay () {
            SetSizeRequest (120, 140);

            label = new TouchLabel ();
            label.WidthRequest = 120;
            label.textAlignment = TouchAlignment.Center;
            label.render.textWrap = TouchTextWrap.Shrink;
            Put (label, 0, 0);

            textBox = new TouchTextBox ();
            textBox.text = "Open";
            textBox.bkgndColor = "seca";
            textBox.textAlignment = TouchAlignment.Center;
            textBox.SetSizeRequest (120, 30);
            Put (textBox, 0, 20);

            button = new TouchButton ();
            button.text = "Force";
            button.buttonColor = "grey3";
            button.ButtonReleaseEvent += OnForceReleased;
            button.SetSizeRequest (120, 30);
            Put (button, 0, 55);

            selector = new ModeSelector ();
            selector.Visible = false;
            selector.labels [0] = "Open";
            selector.labels [1] = "Closed";
            selector.SelectorChangedEvent += OnSelectorChange;
            selector.SetSizeRequest (120, 30);
            Put (selector, 0, 90);

            Show ();
        }

        protected void OnForceReleased (object sender, ButtonReleaseEventArgs args) {
            if (ForceButtonReleaseEvent != null)
                ForceButtonReleaseEvent (this, args);
            else
                throw new NotImplementedException ("Force button release not implemented");
        }

        protected void OnSelectorChange (object sender, SelectorChangedEventArgs args) {
            if (StateSelectedChangedEvent != null)
                StateSelectedChangedEvent (this, args);
            else
                throw new NotImplementedException ("State selector changed not implemented");
        }
    }
}

