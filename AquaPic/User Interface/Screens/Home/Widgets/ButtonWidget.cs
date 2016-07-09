using System;
using Gtk;
using TouchWidgetLibrary;
using AquaPic.Runtime;

namespace AquaPic.UserInterface
{
//    public delegate ButtonWidget CreateButtonHandler ();
//
//    public class ButtonData {
//        public CreateButtonHandler CreateInstanceEvent;
//
//        public ButtonData (CreateButtonHandler CreateInstanceEvent) {
//            this.CreateInstanceEvent = CreateInstanceEvent;
//        }
//
//        public ButtonWidget CreateInstance () {
//            if (CreateInstanceEvent != null)
//                return CreateInstanceEvent ();
//            else
//                throw new Exception ("No bar plot constructor implemented");
//        }
//    }

    public class ButtonWidget : TouchButton
    {
        public ButtonWidget (string name) : base () {
            SetSizeRequest (100, 82);

            text = name;
            bool s1 = Bit.Check (text);
            if (s1)
                buttonColor = "pri";
            else
                buttonColor = "seca";
            
            ButtonReleaseEvent += OnButtonRelease;
        }

        public virtual void OnButtonRelease (object sender, ButtonReleaseEventArgs args) {
            TouchButton b = sender as TouchButton;
            string stateText = b.text;
            bool s = Bit.Check (stateText);
            if (s) {
                Bit.Reset (stateText);
                b.buttonColor = "seca";
            } else {
                Bit.Set (stateText);
                b.buttonColor = "pri";
            }
        }
    }
}

