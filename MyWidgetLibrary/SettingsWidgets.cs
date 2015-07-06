using System;
using System.Diagnostics;
using Cairo;
using Gtk;

namespace MyWidgetLibrary
{
    public class SettingsWidget : Fixed
    {
        public TouchLabel label;

        public virtual string text {
            get {
                return label.text;
            }
            set {
                label.text = value;
            }
        }

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

        public override string text {
            get {
                return label.text;
            }
            set {
                textBox.name = value;
                label.text = value;
            }
        }
        
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

//    public class SettingEntry : SettingsWidget
//    {
//        public event TextChangedHandler TextChangedEvent;
//
//        public Entry entry;
//        private Process osk;
//        private string oldText;
//
//        public SettingEntry () : base () {
//            entry = new Entry ();
//            entry.SetSizeRequest (170, 30);
//            entry.CanFocus = true;
//            entry.Activated += (sender, e) => {
//                TextChangedEventArgs args = new TextChangedEventArgs (entry.Text);
//                if (TextChangedEvent != null)
//                    TextChangedEvent (this, args);
//
//                if (!args.keepText)
//                    entry.Text = oldText;
//
//                if (osk != null) {
//                    osk.CloseMainWindow ();
//                    osk.Close ();
//                }
//            };
//
//            entry.FocusInEvent += (o, args) => {
//                oldText = entry.Text;
//                entry.Text = string.Empty;
//                if (osk == null)
//                    osk = Process.Start ("osk.exe");
//            };
//
//            entry.FocusOutEvent += (o, args) => {
//                if (osk != null) {
//                    osk.CloseMainWindow ();
//                    osk.Close ();
//                }
//            };
//
//            Put (entry, 120, 0);
//            entry.Show ();
//        }
//    }
}

