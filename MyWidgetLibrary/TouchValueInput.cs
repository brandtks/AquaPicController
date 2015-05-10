using System;
using System.Text;
using System.Windows.Forms;
using Cairo;
using Gtk;

namespace MyWidgetLibrary
{
    public class TouchValueInput : Gtk.Window
    {
        public ValueSetHandler valueSetter;

        private Entry entryBox;
        private KeyButton[] buttons;
        private Fixed fix;

        public TouchValueInput () : base (Gtk.WindowType.Toplevel){
            Name = "AquaPic.Keyboard.Input";
            Title = "Input";
            WindowPosition = (Gtk.WindowPosition)4;
            DefaultWidth = 500;
            DefaultHeight = 400;
            Resizable = false;
            AllowGrow = false;

            fix = new Fixed ();
            fix.WidthRequest = 500;
            fix.HeightRequest = 400;

            int x, y;
            buttons = new KeyButton[10];
            for (int i = 0; i < buttons.Length; ++i) {
                buttons [i] = new KeyButton (Convert.ToChar (i + 48));

                if (i == 0) {
                    x = 95;
                    y = 235;
                } else {
                    
                    if (i <= 3) {
                        x = ((i - 1) * 55) + 40;
                        y = 70;
                    } else if (i <= 6) {
                        x = ((i - 4) * 55) + 40;
                        y = 125;
                    } else {
                        x = ((i - 7) * 55) + 40;
                        y = 180;
                    }
                }

                fix.Put (buttons [i], x, y);
                buttons [i].Show ();
            }

            KeyButton plusMinus = new KeyButton ('-', false);
            plusMinus.ButtonReleaseEvent += (o, args) => {
                if (plusMinus.key == '-') {
                    int pos = 0;
                    entryBox.InsertText ("-", ref pos);
                    plusMinus.key = '+'; 
                } else {
                    entryBox.DeleteText (0, 1);
                    plusMinus.key = '-'; 
                }

                plusMinus.Text = plusMinus.key.ToString ();
                plusMinus.QueueDraw ();
            };
            fix.Put (plusMinus, 40, 235);
            plusMinus.Show ();

            KeyButton period = new KeyButton ('.', false);
            period.ButtonReleaseEvent += (o, args) => {
                if (!entryBox.Text.Contains ("."))
                    SendKeys.SendWait (period.key.ToString ());
            };
            fix.Put (period, 150, 235);
            period.Show ();

            entryBox = new Entry ();
            entryBox.WidthRequest = 500;
            entryBox.CanFocus = true;
            fix.Put (entryBox, 0, 0);
            entryBox.Show ();
            entryBox.GrabFocus ();

            Add (fix);
            fix.ShowAll ();
            Show ();
        }

        private class KeyButton : TouchButton {
            public char key;

            public KeyButton (char key, bool sendKey = true) {
                this.key = key;
                this.Text = key.ToString ();
                if (sendKey)
                    ButtonReleaseEvent += (o, args) => SendKeys.SendWait (key.ToString ());
                
            }
        }
    }
}

