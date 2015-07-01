using System;
using Cairo;
using Gtk;

namespace MyWidgetLibrary
{
    public class SettingsWidget : Fixed
    {
        public TouchLabel label;

        public SettingsWidget () {
            SetSizeRequest (290, 30);

            label = new TouchLabel ();
            label = new TouchLabel ();
            label.WidthRequest = 115;
            label.textAlignment = MyAlignment.Right;
            label.textColor = "white";
            label.render.textWrap = MyTextWrap.Shrink;
            Put (label, 0, 4);
            label.Show ();

            Show ();
        }
    }

    public class SettingTextBox : SettingsWidget
    {
        public TouchTextBox textBox;
        
        public SettingTextBox () : base () {
            textBox = new TouchTextBox ();
            textBox.SetSizeRequest (170, 30);
            textBox.enableTouch = true;
            Put (textBox, 120, 0);
            textBox.Show ();
        }
    }

    public class SettingComboBox : SettingsWidget
    {
        public TouchComboBox combo;

        public SettingComboBox () : base () {
            combo = new TouchComboBox ();
            combo.SetSizeRequest (170, 30);
            Put (combo, 120, 0);
            combo.Show ();
        }
    }
}

