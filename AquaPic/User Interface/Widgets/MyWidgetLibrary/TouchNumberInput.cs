using System;
using System.Diagnostics;
using System.Text;
using Cairo;
using Gtk;

namespace MyWidgetLibrary
{
    public delegate void NumberSetEventHandler (string value);

    public class TouchNumberInput : Gtk.Dialog
    {
        public event NumberSetEventHandler NumberSetEvent;

        private Entry entry;
        private Fixed fix;
        private Process osk;

        public TouchNumberInput (bool timeInput = false) {
            Name = "AquaPic.Keyboard.Input";
            Title = "Input";
            WindowPosition = (Gtk.WindowPosition)4;
            DefaultWidth = 205;
            DefaultHeight = 285;

            DestroyEvent += OnDestroy;

            fix = new Fixed ();
            fix.WidthRequest = 205;
            fix.HeightRequest = 285;

            MyBox bkgnd = new MyBox (205, 285);
            bkgnd.color = "black";
            fix.Put (bkgnd, 0, 0);

            entry = new Entry ();
            entry.WidthRequest = 150;
            entry.CanFocus = true;
            entry.ModifyFont (Pango.FontDescription.FromString ("Sans 11"));
            entry.ModifyBase (StateType.Normal, MyColor.NewGtkColor ("grey4"));
            entry.ModifyText (StateType.Normal, MyColor.NewGtkColor ("black"));
            entry.Activated += (sender, e) => {
                if (NumberSetEvent != null)
                    NumberSetEvent (entry.Text);

                Destroy ();
            };

            fix.Put (entry, 0, 0);
            entry.GrabFocus ();

            var b = new TouchButton ();
            b.HeightRequest = 30;
            b.ButtonReleaseEvent += (o, args) => {
                //<WINDOWS> need different virtual keyboard for Linux
                Console.WriteLine ("Starting On Screen KeyBoard");
                osk = Process.Start ("osk.exe");
            };
            fix.Put (b, 155, 0);
            b.Show ();

            int x, y;
            var buttons = new KeyButton[10];
            for (int i = 0; i < buttons.Length; ++i) {
                buttons [i] = new KeyButton (i.ToString (), OnButtonRelease);

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

            KeyButton plusMinus = new KeyButton ("-", null);
            plusMinus.ButtonReleaseEvent += (o, args) => {
                if (plusMinus.text == "-") {
                    int pos = 0;
                    entry.InsertText ("-", ref pos);
                    ++entry.Position;
                    plusMinus.text = "+"; 
                } else {
                    entry.DeleteText (0, 1);
                    plusMinus.text = "-"; 
                }

                plusMinus.text = plusMinus.text.ToString ();
                plusMinus.QueueDraw ();
            };
            fix.Put (plusMinus, 5, 185);

            KeyButton period = new KeyButton (".", OnButtonRelease);
            fix.Put (period, 105, 185);

            KeyButton delete = new KeyButton (Convert.ToChar (0x25C0).ToString (), null); //02FF
            delete.ButtonReleaseEvent += (o, args) => {
                int pos = entry.Position;
                entry.DeleteText (entry.Position - 1, entry.Position);
            };
            fix.Put (delete, 155, 35);

            KeyButton clear = new KeyButton ("C", null);
            clear.ButtonReleaseEvent += (o, args) => {
                plusMinus.text = "-";
                entry.Text = string.Empty;
            };
            fix.Put (clear, 155, 85);

            KeyButton semi;
            if (timeInput)
                semi = new KeyButton (":", OnButtonRelease);
            else {
                semi = new KeyButton (":", null);
                semi.buttonColor = "grey3";
            }
            fix.Put (semi, 5, 235);

            KeyButton pm = new KeyButton ("PM", null);
            if (timeInput) {
                pm.ButtonReleaseEvent += (o, args) => {
                    int len = entry.Text.Length;
                    if (len >= 3) {
                        string last = entry.Text.Substring (len - 2);
                        if (last == "AM") {
                            int pos = entry.Text.Length - 2;
                            entry.DeleteText (pos, pos + 2);
                            entry.InsertText ("PM", ref pos);
                        } else if (last == "PM") {
                            int pos = entry.Text.Length - 3;
                            entry.DeleteText (pos, pos + 3);
                        } else {
                            int pos = entry.Text.Length;
                            entry.InsertText (" PM", ref pos);
                        }
                    } else {
                        int pos = entry.Text.Length;
                        entry.InsertText (" PM", ref pos);
                    }
                };
            } else
                pm.buttonColor = "grey3";
            fix.Put (pm, 55, 235);

            KeyButton am = new KeyButton ("AM", null);
            if (timeInput) {
                am.ButtonReleaseEvent += (o, args) => {
                    int len = entry.Text.Length;
                    if (len >= 3) {
                        string last = entry.Text.Substring (len - 2);
                        if (last == "PM") {
                            int pos = entry.Text.Length - 2;
                            entry.DeleteText (pos, pos + 2);
                            entry.InsertText ("AM", ref pos);
                        } else if (last == "AM") {
                            int pos = entry.Text.Length - 3;
                            entry.DeleteText (pos, pos + 3);
                        } else {
                            int pos = entry.Text.Length;
                            entry.InsertText (" AM", ref pos);
                        }
                    } else {
                        int pos = entry.Text.Length;
                        entry.InsertText (" AM", ref pos);
                    }
                };
            } else
                am.buttonColor = "grey3";
            fix.Put (am, 105, 235);

            KeyButton cancel = new KeyButton ("Cancel", null);
            cancel.textSize = 9;
            cancel.ButtonReleaseEvent += (o, args) => {
                Destroy ();
            };
            fix.Put (cancel, 155, 235);

            TouchButton enter = new TouchButton ();
            enter.text = Convert.ToChar (0x23CE).ToString ();
            enter.HeightRequest = 95;
            enter.ButtonReleaseEvent += (o, args) => {
                if (NumberSetEvent != null)
                    NumberSetEvent (entry.Text);

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

        protected void OnDestroy (object sender, DestroyEventArgs args) {
            if (osk != null) {
                osk.CloseMainWindow ();
                osk.Close ();
            }
        }

        protected void OnButtonRelease (object sender, ButtonReleaseEventArgs args) {
            KeyButton k = sender as KeyButton;
            int pos = entry.Position;
            entry.InsertText (k.text, ref pos);
            ++entry.Position;
        }

        private class KeyButton : TouchButton {
            public KeyButton (string text, ButtonReleaseEventHandler handler) {
                this.text = text;
                this.textSize = 13;
                if (handler != null)
                    ButtonReleaseEvent += handler;
            }
        }
    }
}

