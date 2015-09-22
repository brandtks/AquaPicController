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
        private VirtualKeyboard vkb;

        public TouchNumberInput (bool timeInput = false, Gtk.Window parent = null) : base ("Input", parent, DialogFlags.DestroyWithParent) {
            Name = "AquaPic.Keyboard.Input";
            Title = "Input";
            WindowPosition = (Gtk.WindowPosition)4;
            DefaultWidth = 205;
            DefaultHeight = 290;
            KeepAbove = true;

            #if RPI_BUILD
            Decorated = false;

            ExposeEvent += (o, args) => {
                using (Context cr = Gdk.CairoHelper.Create (GdkWindow)) {
                    cr.MoveTo (Allocation.Left, Allocation.Top);
                    cr.LineTo (Allocation.Right, Allocation.Top);
                    cr.LineTo (Allocation.Right, Allocation.Bottom);
                    cr.LineTo (Allocation.Left, Allocation.Bottom);
                    cr.ClosePath ();
                    cr.LineWidth = 1.8;
                    MyColor.SetSource (cr, "grey4");
                    cr.Stroke ();
                }
            };
            #endif

            fix = new Fixed ();
            fix.WidthRequest = 205;
            fix.HeightRequest = 290;

            this.ModifyBg (StateType.Normal, MyColor.NewGtkColor ("grey0"));



            entry = new Entry ();
            entry.WidthRequest = 145;
            entry.HeightRequest = 30;
            entry.CanFocus = true;
            entry.ModifyFont (Pango.FontDescription.FromString ("Sans 11"));
            entry.ModifyBase (StateType.Normal, MyColor.NewGtkColor ("grey4"));
            entry.ModifyText (StateType.Normal, MyColor.NewGtkColor ("black"));
            entry.Activated += (sender, e) => {
                if (NumberSetEvent != null)
                    NumberSetEvent (entry.Text);

                Destroy ();
            };

            fix.Put (entry, 5, 5);
            entry.GrabFocus ();

            var b = new TouchButton ();
            b.HeightRequest = 30;
            b.ButtonReleaseEvent += (o, args) => {
                if (vkb == null) {
                    fix.WidthRequest = 710;
                    fix.QueueDraw ();

                    SetSizeRequest (710, 290);
                    Show ();

                    entry.WidthRequest = 700;
                    entry.QueueDraw ();

                    b.Destroy ();

                    vkb = new VirtualKeyboard (entry, OnButtonRelease);
                    fix.Put (vkb, 205, 60);
                    vkb.Show ();
                }
            };
            fix.Put (b, 155, 5);
            b.Show ();

            int x, y;
            var buttons = new KeyButton[10];
            for (int i = 0; i < buttons.Length; ++i) {
                buttons [i] = new KeyButton (i.ToString (), OnButtonRelease);

                if (i == 0) {
                    x = 55;
                    y = 190;
                } else {
                    
                    if (i <= 3) {
                        x = ((i - 1) * 50) + 5;
                        y = 40;
                    } else if (i <= 6) {
                        x = ((i - 4) * 50) + 5;
                        y = 90;
                    } else {
                        x = ((i - 7) * 50) + 5;
                        y = 140;
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
            fix.Put (plusMinus, 5, 190);

            KeyButton period = new KeyButton (".", OnButtonRelease);
            fix.Put (period, 105, 190);

            KeyButton delete = new KeyButton (Convert.ToChar (0x232B).ToString (), null); //02FF, 25C0
            delete.ButtonReleaseEvent += (o, args) => {
                int pos = entry.Position;
                entry.DeleteText (entry.Position - 1, entry.Position);
            };
            fix.Put (delete, 155, 40);

            KeyButton clear = new KeyButton ("C", null);
            clear.ButtonReleaseEvent += (o, args) => {
                plusMinus.text = "-";
                entry.Text = string.Empty;
            };
            fix.Put (clear, 155, 90);

            KeyButton semi;
            if (timeInput)
                semi = new KeyButton (":", OnButtonRelease);
            else {
                semi = new KeyButton (":", null);
                semi.buttonColor = "grey1";
            }
            fix.Put (semi, 5, 240);

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
                pm.buttonColor = "grey1";
            fix.Put (pm, 55, 240);

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
                am.buttonColor = "grey1";
            fix.Put (am, 105, 240);

            KeyButton cancel = new KeyButton ("Cancel", null);
            cancel.textSize = 9;
            cancel.ButtonReleaseEvent += (o, args) => {
                Destroy ();
            };
            fix.Put (cancel, 155, 240);

            TouchButton enter = new TouchButton ();
            enter.text = Convert.ToChar (0x23CE).ToString ();
            enter.HeightRequest = 95;
            enter.ButtonReleaseEvent += (o, args) => {
                if (NumberSetEvent != null)
                    NumberSetEvent (entry.Text);

                Destroy ();
            };
            fix.Put (enter, 155, 140);

            foreach (Widget w in this.Children) {
                Remove (w);
                w.Dispose ();
            }

            Add (fix);
            fix.ShowAll ();
            Show ();
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

        private class VirtualKeyboard : Fixed {
            enum ShiftKeyState {
                Lower,
                Shifted,
                Caps
            };

            static char[] qwerty1Lower = {'q','w','e','r','t','y','u','i','o','p'};
            static char[] qwerty2Lower = {'a','s','d','f','g','h','j','k','l'};
            static char[] qwerty3Lower = {'z','x','c','v','b','n','m'};

            static char[] qwerty1Upper = {'Q','W','E','R','T','Y','U','I','O','P'};
            static char[] qwerty2Upper = {'A','S','D','F','G','H','J','K','L'};
            static char[] qwerty3Upper = {'Z','X','C','V','B','N','M'};

            KeyButton[] row1;
            KeyButton[] row2;
            KeyButton[] row3;

            ShiftKeyState shiftState;

            public VirtualKeyboard (Entry e, ButtonReleaseEventHandler handler) {
                SetSizeRequest (505, 205);

                row1 = new KeyButton[10];
                row2 = new KeyButton[9];
                row3 = new KeyButton[7];

                for (int i = 0; i < 10; ++i) {
                    row1 [i] = new KeyButton (qwerty1Lower [i].ToString (), handler);
                    Put (row1 [i], (i * 50) + 5, 5);
                    row1 [i].Show ();
                }

                for (int i = 0; i < 9; ++i) {
                    row2 [i] = new KeyButton (qwerty2Lower [i].ToString (), handler);
                    Put (row2 [i], (i * 50) + 30, 55);
                    row2 [i].Show ();
                }

                for (int i = 0; i < 7; ++i) {
                    row3 [i] = new KeyButton (qwerty3Lower [i].ToString (), handler);
                    Put (row3 [i], (i * 50) + 80, 105);
                    row3 [i].Show ();
                }

                shiftState = ShiftKeyState.Lower;
                var shiftKey = new TouchButton ();
                shiftKey.SetSizeRequest (70, 45);
                shiftKey.text = Convert.ToChar (0x21E7).ToString ();
                shiftKey.buttonColor = "grey3";
                shiftKey.ButtonReleaseEvent += (o, args) => {
                    if (shiftState == ShiftKeyState.Lower) {
                        ToUpper ();
                        shiftKey.buttonColor = "pri";
                        shiftKey.QueueDraw ();
                        shiftState = ShiftKeyState.Shifted;
                    } else if (shiftState == ShiftKeyState.Shifted) {
                        shiftKey.buttonColor = "seca";
                        shiftKey.QueueDraw ();
                        shiftState = ShiftKeyState.Caps;
                    } else { // Caps
                        ToLower ();
                        shiftKey.buttonColor = "grey3";
                        shiftKey.QueueDraw ();
                        shiftState = ShiftKeyState.Lower;
                    }
                };
                Put (shiftKey, 5, 105);
                shiftKey.Show ();

                var delete = new TouchButton ();
                delete.SetSizeRequest (70, 45);
                delete.text = Convert.ToChar (0x232B).ToString ();
                delete.textSize = 15;
                delete.ButtonReleaseEvent += (o, args) => {
                    int pos = e.Position;
                    e.DeleteText (e.Position - 1, e.Position);
                };
                Put (delete, 430, 105);
                delete.Show ();

                var space = new TouchButton ();
                space.text = "Space";
                space.SetSizeRequest (245, 45);
                space.ButtonReleaseEvent += (o, args) => {
                    int pos = e.Position;
                    e.InsertText (" ", ref pos);
                    ++e.Position;
                };
                Put (space, 130, 155);
                space.Show ();

                e.TextInserted += (o, args) => {
                    if (shiftState == ShiftKeyState.Shifted) {
                        ToLower ();
                        shiftKey.buttonColor = "grey3";
                        shiftKey.QueueDraw ();
                        shiftState = ShiftKeyState.Lower;
                    }
                };

                Show ();
            }

            public void ToUpper () {
                for (int i = 0; i < 10; ++i) {
                    row1 [i].text = qwerty1Upper [i].ToString ();
                    row1 [i].QueueDraw ();
                }

                for (int i = 0; i < 9; ++i) {
                    row2 [i].text = qwerty2Upper [i].ToString ();
                    row2 [i].QueueDraw ();
                }

                for (int i = 0; i < 7; ++i) {
                    row3 [i].text = qwerty3Upper [i].ToString ();
                    row3 [i].QueueDraw ();
                }
            }

            public void ToLower () {
                for (int i = 0; i < 10; ++i) {
                    row1 [i].text = qwerty1Lower [i].ToString ();
                    row1 [i].QueueDraw ();
                }

                for (int i = 0; i < 9; ++i) {
                    row2 [i].text = qwerty2Lower [i].ToString ();
                    row2 [i].QueueDraw ();
                }

                for (int i = 0; i < 7; ++i) {
                    row3 [i].text = qwerty3Lower [i].ToString ();
                    row3 [i].QueueDraw ();
                }
            }
        }
    }
}

