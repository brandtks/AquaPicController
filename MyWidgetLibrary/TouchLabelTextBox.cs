using System;
using Cairo;
using Gtk;

namespace MyWidgetLibrary
{
    public class TouchLabelTextBox : Fixed
    {
        public TouchLabel label;
        public TouchTextBox textBox;
        
        public TouchLabelTextBox () {
            SetSizeRequest (290, 30);

            label = new TouchLabel ();
            label.WidthRequest = 115;
            label.textAlignment = MyAlignment.Right;
            label.textColor = "white";
            Put (label, 0, 4);
            label.Show ();

            textBox = new TouchTextBox ();
            textBox.SetSizeRequest (170, 30);
            textBox.enableTouch = true;
            Put (textBox, 120, 0);
            textBox.Show ();

            Show ();
        }
    }
}

