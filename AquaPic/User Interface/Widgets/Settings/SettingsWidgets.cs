using System;
using System.Diagnostics;
using Cairo;
using Gtk;

namespace MyWidgetLibrary
{
    public class SettingsWidget : Fixed
    {
        public bool optionalSetting;
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

            optionalSetting = false;

            label = new TouchLabel ();
            label = new TouchLabel ();
            label.WidthRequest = 115;
            label.textAlignment = MyAlignment.Right;
            label.textColor = "white";
            label.render.textWrap = MyTextWrap.Shrink;
            label.text = "Error: No Title";
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

    public class SettingSelectorSwitch : SettingsWidget
    {
        public TouchSelectorSwitch selectorSwitch;
        public string[] labels;

        public SettingSelectorSwitch () : base () {
            selectorSwitch = new TouchSelectorSwitch ();
            selectorSwitch.SelectionCount = 2;
            selectorSwitch.CurrentSelected = 0;
            selectorSwitch.SliderSize = MySliderSize.Large;
            selectorSwitch.SliderColorOptions [0] = "pri";
            selectorSwitch.SliderColorOptions [1] = "grey2";

            selectorSwitch.SetSizeRequest (170, 30);
            selectorSwitch.ExposeEvent += OnExpose;
            Put (selectorSwitch, 120, 0);
            selectorSwitch.Show ();

            labels = new string[2];
            labels [0] = "True";
            labels [1] = "False";
        }

        protected void OnExpose (object sender, ExposeEventArgs args) {
            TouchSelectorSwitch ss = sender as TouchSelectorSwitch;
            int seperation = ss.Allocation.Width / ss.SelectionCount;
            int x = ss.Allocation.Left;

            MyText render = new MyText ();
            render.textWrap = MyTextWrap.Shrink;
            render.alignment = MyAlignment.Center;
            render.font.color = "white";

            foreach (var l in labels) {
                render.text = l;
                render.Render (ss, x, ss.Allocation.Top, seperation, ss.Allocation.Height);
                x += seperation;
            }
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

