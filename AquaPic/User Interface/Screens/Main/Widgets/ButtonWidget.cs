using System;
using Gtk;
using MyWidgetLibrary;

namespace AquaPic.UserInterface
{
    public delegate ButtonWidget CreateButtonHandler ();

    public class ButtonData {
        public CreateButtonHandler CreateInstanceEvent;

        public ButtonData (CreateButtonHandler CreateInstanceEvent) {
            this.CreateInstanceEvent = CreateInstanceEvent;
        }

        public ButtonWidget CreateInstance () {
            if (CreateInstanceEvent != null)
                return CreateInstanceEvent ();
            else
                throw new Exception ("No bar plot constructor implemented");
        }
    }

    public class ButtonWidget : TouchButton
    {
        public ButtonWidget () : base () {
            SetSizeRequest (108, 95);
            ButtonReleaseEvent += OnButtonRelease;
        }

        public virtual void OnButtonRelease (object sender, ButtonReleaseEventArgs args) {
            throw new Exception ("Button release method is not implemented");
        }
    }
}

