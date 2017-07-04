#region License

/*
    AquaPic Main Control - Handles all functionality for the AquaPic aquarium controller. 

    Copyright (c) 2017 Skyler Brandt

    This program is free software; you can redistribute it and/or
    modify it under the terms of the GNU General Public License
    as published by the Free Software Foundation; either version 2
    of the License, or (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program; if not, write to the Free Software
    Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.

    Optionally you can also view the license at <http://www.gnu.org/licenses/>.
*/

#endregion // License

using System;
using System.Collections.Generic;
using Cairo;
using Gtk;
using TouchWidgetLibrary;
using AquaPic.Utilites;

namespace AquaPic.UserInterface
{
    public class CalibrationArguments {
        public double zeroActual;
        public double zeroValue;
        public double fullScaleActual;
        public double fullScaleValue;

        public CalibrationArguments () {
            zeroActual = 0;
            zeroValue = 0;
            fullScaleActual = 0;
            fullScaleValue = 0;
        }
    }

    public delegate void CalibrationCompleteHandler (CalibrationArguments args);
    public delegate double GetCalibrationValueHandler ();

    public enum CalibrationState {
        ZeroActual,
        ZeroValue,
        FullScaleActual,
        FullScaleValue
    }

    public class CalibrationDialog : Gtk.Dialog
    {
        private TextView tv;
        private TouchTextBox valTb;
        private TouchButton actionBtn, skipBtn;
        private uint timerId;
        private bool forced, init;
        private CalibrationState calState;

        protected Fixed fix;

        public string zeroActualInstructions;
        public string zeroValueInstructions;
        public string fullScaleActualInstructions;
        public string fullScaleValueInstructions;

        public GetCalibrationValueHandler GetCalibrationValue;
        public event CalibrationCompleteHandler CalibrationCompleteEvent;

        public CalibrationArguments calArgs;

        public CalibrationDialog (
            string name, 
            GetCalibrationValueHandler GetCalibrationValue, 
            CalibrationState startingState = CalibrationState.ZeroValue
        ) {
            Name = "AquaPic.Calibration." + name;
            Title = name + " Calibration";
            WindowPosition = (Gtk.WindowPosition)4;
            SetSizeRequest (600, 300);

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
                    TouchColor.SetSource (cr, "grey4");
                    cr.Stroke ();
                }
            };
            #endif

            ModifyBg (StateType.Normal, TouchColor.NewGtkColor ("grey0"));

            foreach (var w in Children) {
                Remove (w);
                w.Dispose ();
            }

            ExposeEvent += OnExpose;

            this.GetCalibrationValue = GetCalibrationValue;
            if ((startingState == CalibrationState.ZeroActual) || (startingState == CalibrationState.ZeroValue)) {
                calState = startingState;
            } else {
                calState = CalibrationState.ZeroValue;
            }
            calArgs = new CalibrationArguments ();
            forced = false;
            init = false;

            fix = new Fixed ();
            fix.SetSizeRequest (600, 300);

            tv = new TextView ();
            tv.ModifyFont (Pango.FontDescription.FromString ("Sans 11"));
            tv.ModifyBase (StateType.Normal, TouchColor.NewGtkColor ("grey4"));
            tv.ModifyText (StateType.Normal, TouchColor.NewGtkColor ("black"));
            tv.CanFocus = false;
            tv.Editable = false;

            ScrolledWindow sw = new ScrolledWindow ();
            sw.SetSizeRequest (580, 225);
            sw.Add (tv);
            tv.Show ();
            fix.Put (sw, 10, 25);
            sw.Show ();

            var l = new TouchLabel ();
            l.text = name;
            l.WidthRequest = 600;
            l.textAlignment = TouchAlignment.Center;
            fix.Put (l, 0, 3);
            l.Show ();

            valTb = new TouchTextBox ();
            valTb.SetSizeRequest (110, 30);
            valTb.textAlignment = TouchAlignment.Center;
            if (calState == CalibrationState.ZeroActual) {
                valTb.text = "Actual";
                valTb.enableTouch = true;
                valTb.TextChangedEvent += OnValueTextBoxTextChanged;
            } else {
                valTb.text = GetCalibrationValue ().ToString ("F2");
            }
            fix.Put (valTb, 10, 260);
            valTb.Show ();

            actionBtn = new TouchButton ();
            actionBtn.SetSizeRequest (150, 30);
            if (calState == CalibrationState.ZeroActual) {
                actionBtn.text = "Zero Actual";
            } else {
                actionBtn.text = "Zero Value";
            }
            actionBtn.ButtonReleaseEvent += OnActionButtonReleaseEvent;
            fix.Put (actionBtn, 440, 260);
            actionBtn.Show ();

            var cancelBtn = new TouchButton ();
            cancelBtn.SetSizeRequest (100, 30);
            cancelBtn.text = "Cancel";
            cancelBtn.buttonColor = "compl";
            cancelBtn.ButtonReleaseEvent += (o, args) => Destroy ();
            fix.Put (cancelBtn, 335, 260);
            cancelBtn.Show ();

            skipBtn = new TouchButton ();
            skipBtn.SetSizeRequest (100, 30);
            skipBtn.text = "Skip";
            skipBtn.buttonColor = "seca";
            skipBtn.ButtonReleaseEvent += OnSkipButtonReleaseEvent;
            fix.Put (skipBtn, 230, 260);
            skipBtn.Show ();

            var forceBtn = new TouchButton ();
            forceBtn.SetSizeRequest (100, 30);
            forceBtn.text = "Force";
            forceBtn.buttonColor = "grey3";
            forceBtn.ButtonReleaseEvent += OnForceButtonReleaseEvent;
            fix.Put (forceBtn, 125, 260);
            forceBtn.Show ();

            Add (fix);
            fix.Show ();

            timerId = GLib.Timeout.Add (1000, OnUpdateTimer);

            Show ();
        }

        public override void Dispose () {
            GLib.Source.Remove (timerId);
            base.Dispose ();
        }

        public override void Destroy () {
            base.Destroy ();
            Dispose ();
        }

        protected bool OnUpdateTimer () {
            if ((calState != CalibrationState.ZeroActual) && (calState != CalibrationState.FullScaleActual) && (!forced)) {
                valTb.text = GetCalibrationValue ().ToString ("F2");
                valTb.QueueDraw ();
            }

            return true;
        }

        protected void OnExpose (object sender, ExposeEventArgs args) {
            if (!init) {
                if (calState == CalibrationState.ZeroActual) {
                    TextBuffer tb = tv.Buffer;
                    if (zeroActualInstructions.IsNotEmpty ()) {
                        tb.Text = zeroValueInstructions;
                    } else {
                        tb.Text = "Please enter the zero actual value.\n" +
                            "Once the zero actual value is entered press the button.\n\n";
                    }

                    var tag = new TextTag (null);
                    tag.ForegroundGdk = TouchColor.NewGtkColor ("seca");
                    tb.TagTable.Add (tag);

                    var ti = tb.EndIter;
                    tb.InsertWithTags (ref ti, string.Format ("Previous zero actual: {0:F2}", calArgs.zeroActual), tag);
                } else if (calState == CalibrationState.ZeroValue) {
                    TextBuffer tb = tv.Buffer;
                    if (!string.IsNullOrWhiteSpace (zeroValueInstructions))
                        tb.Text = zeroValueInstructions;
                    else
                        tb.Text = "Place the instrument in its zero state.\n" +
                            "Once value has settled, press the button.\n\n";

                    var tag = new TextTag (null);
                    tag.ForegroundGdk = TouchColor.NewGtkColor ("seca");
                    tb.TagTable.Add (tag);

                    var ti = tb.EndIter;
                    tb.InsertWithTags (ref ti, string.Format ("Previous zero value: {0:F2}", calArgs.zeroValue), tag);
                }

                tv.QueueDraw ();
                init = true;
            }
        }

        protected void OnActionButtonReleaseEvent (object sender, ButtonReleaseEventArgs args) {
            switch (calState) {
                case CalibrationState.ZeroActual:
                    if (valTb.text == "Actual")
                        MessageBox.Show ("Please enter the zero actual value");
                    else {
                        calArgs.zeroActual = Convert.ToDouble (valTb.text);
                        MoveToNextState ();
                    }

                    break;
                case CalibrationState.ZeroValue:
                    if (!forced)
                        calArgs.zeroValue = GetCalibrationValue ();
                    else
                        calArgs.zeroValue = Convert.ToDouble (valTb.text);

                    MoveToNextState ();
                    break;
                case CalibrationState.FullScaleActual:
                    if (valTb.text == "Actual")
                        MessageBox.Show ("Please enter the full scale actual value");
                    else {
                        calArgs.fullScaleActual = Convert.ToDouble (valTb.text);
                        MoveToNextState ();
                    }
                
                    break;
                case CalibrationState.FullScaleValue:
                    if (!forced)
                        calArgs.fullScaleValue = GetCalibrationValue ();
                    else
                        calArgs.fullScaleValue = Convert.ToDouble (valTb.text);

                    if (CalibrationCompleteEvent != null)
                        CalibrationCompleteEvent (calArgs);
                
                    Destroy ();
                    break;
                default:
                    break;
            }
        }

        protected void OnSkipButtonReleaseEvent (object sender, ButtonReleaseEventArgs args) {
            MoveToNextState ();
        }

        protected void MoveToNextState () {
            TextBuffer tb;
            TextTag tag;
            TextIter ti;

            switch (calState) {
                case CalibrationState.ZeroActual:
                    if (!forced) {
                        valTb.enableTouch = false;
                        valTb.TextChangedEvent -= OnValueTextBoxTextChanged;

                    }
                    valTb.text = GetCalibrationValue ().ToString ("F2");
                    valTb.QueueDraw ();

                    actionBtn.text = "Zero Value";
                    actionBtn.QueueDraw ();

                    tb = tv.Buffer;
                    if (!string.IsNullOrWhiteSpace (zeroValueInstructions))
                        tb.Text = zeroValueInstructions;
                    else
                        tb.Text = "Place the instrument in its zero state.\n" +
                            "Once value has settled, press the button.\n\n";

                    tag = new TextTag (null);
                    tag.ForegroundGdk = TouchColor.NewGtkColor ("seca");
                    tb.TagTable.Add (tag);

                    ti = tb.EndIter;
                    tb.InsertWithTags (ref ti, string.Format ("Previous zero value: {0:F2}", calArgs.zeroValue), tag);

                    tv.QueueDraw ();

                    calState = CalibrationState.ZeroValue;

                    break;
                case CalibrationState.ZeroValue:
                    if (!forced) {
                        valTb.enableTouch = true;
                        valTb.TextChangedEvent += OnValueTextBoxTextChanged;
                    }
                    valTb.text = "Actual";
                    valTb.QueueDraw ();

                    actionBtn.text = "Full Scale Actual";
                    actionBtn.QueueDraw ();

                    tb = tv.Buffer;
                    if (!string.IsNullOrWhiteSpace (fullScaleActualInstructions))
                        tb.Text = fullScaleActualInstructions;
                    else
                        tb.Text = "Please enter the full scale actual value.\n" +
                            "Once the full scale value is entered press the button.\n\n";

                    tag = new TextTag (null);
                    tag.ForegroundGdk = TouchColor.NewGtkColor ("seca");
                    tb.TagTable.Add (tag);

                    ti = tb.EndIter;
                    tb.InsertWithTags (ref ti, string.Format ("Previous full scale actual: {0:F2}", calArgs.fullScaleActual), tag);

                    tv.QueueDraw ();

                    calState = CalibrationState.FullScaleActual;
                    break;
                case CalibrationState.FullScaleActual:
                    if (!forced) {
                        valTb.enableTouch = false;
                        valTb.TextChangedEvent -= OnValueTextBoxTextChanged;

                    }
                    valTb.text = GetCalibrationValue ().ToString ("F2");
                    valTb.QueueDraw ();

                    actionBtn.text = "Full Scale Value";
                    actionBtn.QueueDraw ();

                    tb = tv.Buffer;
                    if (!string.IsNullOrWhiteSpace (fullScaleValueInstructions))
                        tb.Text = fullScaleValueInstructions;
                    else
                        tb.Text = "Please the instrument in its full scale state.\n" +
                        "Once value has settled, press the button.\n\n";

                    tag = new TextTag (null);
                    tag.ForegroundGdk = TouchColor.NewGtkColor ("seca");
                    tb.TagTable.Add (tag);

                    ti = tb.EndIter;
                    tb.InsertWithTags (ref ti, string.Format ("Previous full scale value: {0:F2}", calArgs.fullScaleValue), tag);

                    tv.QueueDraw ();

                    calState = CalibrationState.FullScaleValue;
                    break;
                case CalibrationState.FullScaleValue:
                    if (CalibrationCompleteEvent != null)
                        CalibrationCompleteEvent (calArgs);

                    Destroy ();
                    break;
                default:
                    break;
            }
        }

        protected void OnForceButtonReleaseEvent (object sender, ButtonReleaseEventArgs args) {
            var btn = sender as TouchButton;
            if (btn != null) {
                if (!forced) {
                    btn.buttonColor = "pri";
                    btn.QueueDraw ();

                    valTb.enableTouch = true;
                    valTb.bkgndColor = "seca";
                    valTb.TextChangedEvent += OnValueTextBoxTextChanged;
                    valTb.QueueDraw ();

                    forced = true;
                } else {
                    btn.buttonColor = "grey3";
                    btn.QueueDraw ();

                    if (calState != CalibrationState.FullScaleActual) {
                        valTb.enableTouch = false;
                        valTb.bkgndColor = "grey4";
                        valTb.text = GetCalibrationValue ().ToString ("F2");
                        valTb.TextChangedEvent -= OnValueTextBoxTextChanged;
                        valTb.QueueDraw ();
                    }

                    forced = false;
                }
            }
        }

        protected void OnValueTextBoxTextChanged (object sender, TextChangedEventArgs args) {
            try {
                Convert.ToDouble (args.text);
            } catch {
                args.keepText = false;
            }
        }
    }
}

