using System;
using System.Text;
using System.Windows.Forms;
using Cairo;
using Gtk;

namespace MyWidgetLibrary
{
    public delegate void NumberSetEventHandler (string value);

    public class TouchNumberInput : Gtk.Dialog
    {
        public NumberSetEventHandler NumberSetEvent;

        private Entry entryBox;
        private KeyButton[] buttons;
        private Fixed fix;

        public TouchNumberInput () {
            Name = "AquaPic.Keyboard.Input";
            Title = "Input";
            WindowPosition = (Gtk.WindowPosition)4;
            DefaultWidth = 205;
            DefaultHeight = 235;

            fix = new Fixed ();
            fix.WidthRequest = 205;
            fix.HeightRequest = 235;

            MyBox bkgnd = new MyBox (205, 235);
            bkgnd.color = "black";
            fix.Put (bkgnd, 0, 0);

            entryBox = new Entry ();
            entryBox.WidthRequest = 205;
            entryBox.CanFocus = true;
            entryBox.Activated += (sender, e) => {
                if (NumberSetEvent != null)
                    NumberSetEvent (entryBox.Text);

                Destroy ();
            };
            fix.Put (entryBox, 0, 0);
            entryBox.GrabFocus ();

            int x, y;
            buttons = new KeyButton[10];
            for (int i = 0; i < buttons.Length; ++i) {
                buttons [i] = new KeyButton (i.ToString ());

                if (i == 0) {
                    x = 55;
                    y = 185;
                } else {
                    
                    if (i <= 3) {
                        x = ((i - 1) * 50) + 5;
                        y = 35;
                    } else if (i <= 6) {
                        x = ((i - 4) * 50) + 5;
                        y = 85;
                    } else {
                        x = ((i - 7) * 50) + 5;
                        y = 135;
                    }
                }

                fix.Put (buttons [i], x, y);
            }

            KeyButton plusMinus = new KeyButton ("-", false);
            plusMinus.ButtonReleaseEvent += (o, args) => {
                if (plusMinus.key == "-") {
                    int pos = 0;
                    entryBox.InsertText ("-", ref pos);
                    ++entryBox.Position;
                    plusMinus.key = "+"; 
                } else {
                    entryBox.DeleteText (0, 1);
                    plusMinus.key = "-"; 
                }

                plusMinus.text = plusMinus.key.ToString ();
                plusMinus.QueueDraw ();
            };
            fix.Put (plusMinus, 5, 185);

            KeyButton period = new KeyButton (".");
            fix.Put (period, 105, 185);

            KeyButton delete = new KeyButton ("{BS}", Convert.ToChar (0x02FF).ToString ());
            fix.Put (delete, 155, 35);

            KeyButton clear = new KeyButton ("C", false);
            clear.ButtonReleaseEvent += (o, args) => {
                plusMinus.key = "-";
                entryBox.Text = string.Empty;
            };
            fix.Put (clear, 155, 85);

            TouchButton enter = new TouchButton ();
            enter.text = Convert.ToChar (0x23CE).ToString ();
            enter.HeightRequest = 95;
            enter.ButtonReleaseEvent += (o, args) => {
                if (NumberSetEvent != null)
                    NumberSetEvent (entryBox.Text);

                Destroy ();
            };
            fix.Put (enter, 155, 135);

            foreach (Widget w in this.Children) {
                Remove (w);
                w.Dispose ();
            }

            Add (fix);
            fix.ShowAll ();
            Show ();
        }

        private class KeyButton : TouchButton {
            public string key;

            public KeyButton (string key, bool sendKey = true) {
                this.key = key;
                this.text = key;
                if (sendKey)
                    ButtonReleaseEvent += (o, args) => SendKeys.SendWait (key);
                
            }

            public KeyButton (string key, string text, bool sendKey = true) 
                : this (key, sendKey) {
                this.text = text;
            }
        }
    }
}

