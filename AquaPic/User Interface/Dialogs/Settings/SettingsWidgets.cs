using System;
using System.Diagnostics;
using Cairo;
using Gtk;
using TouchWidgetLibrary;

namespace AquaPic.UserInterface
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
            label.textAlignment = TouchAlignment.Right;
            label.textColor = "white";
            label.textRender.textWrap = TouchTextWrap.Shrink;
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
            combo.ButtonPressEvent += OnComboButtonPressed;
            Put (combo, 120, 0);
            combo.Show ();
        }

        protected void OnComboButtonPressed (object sender, ButtonPressEventArgs args) {
            Fixed p = this.Parent as Fixed;
            if (p != null) {
                int x = this.Allocation.Left;
                int y = this.Allocation.Top;
                p.Remove (this);
                p.Put (this, x, y);
            }
        }
    }

    public class SettingSelectorSwitch : SettingsWidget
    {
        public TouchSelectorSwitch selectorSwitch;
        public string[] labels;

        public SettingSelectorSwitch (string label1, string label2) : base () {
            selectorSwitch = new TouchSelectorSwitch (2);
            selectorSwitch.currentSelected = 0;
            selectorSwitch.sliderSize = MySliderSize.Large;
            selectorSwitch.sliderColorOptions [0] = "pri";
            selectorSwitch.sliderColorOptions [1] = "grey2";

            selectorSwitch.SetSizeRequest (170, 30);
            selectorSwitch.ExposeEvent += OnExpose;
            Put (selectorSwitch, 120, 0);
            selectorSwitch.Show ();

            labels = new string[2];
            labels [0] = label1;
            labels [1] = label2;
        }

        public SettingSelectorSwitch () : this ("True", "False") { }

        protected void OnExpose (object sender, ExposeEventArgs args) {
            TouchSelectorSwitch ss = sender as TouchSelectorSwitch;
            int seperation = ss.Allocation.Width / ss.selectionCount;
            int x = ss.Allocation.Left;

            TouchText render = new TouchText ();
            render.textWrap = TouchTextWrap.Shrink;
            render.alignment = TouchAlignment.Center;
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

