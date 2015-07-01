using System;
using Gtk;
using Cairo;
using MyWidgetLibrary;
using AquaPic.Utilites;

namespace AquaPic
{
    public class PowerOutletSlider : Fixed
    {
        private string[] labels;
        public TouchLabel OutletName;
        public TouchLabel Status;
        public TouchSelectorSwitch ss;
        public TouchCurvedProgressBar ampBar;
        public TouchTextBox ampText;

        public float amps {
            set {
                float v;
                if (value > 10.0f)
                    v = 10.0f;
                else if (value < 0.0f)
                    v = 0.0f;
                else
                    v = value;

                ampText.text = v.ToString ("F1");
                ampBar.currentProgress = v / 10.0f;
            }
        }

        public PowerOutletSlider (int id) {
            SetSizeRequest (180, 145);

            var box = new MyBox (180, 145);
            box.color = "grey3";
            Put (box, 0, 0);

            ampBar = new TouchCurvedProgressBar ();
            Put (ampBar, 10, 22);
            ampBar.Show ();

            ampText = new TouchTextBox ();
            ampText.SetSizeRequest( 76, 20);
            ampText.textSize = 10;
            ampText.textAlignment = MyAlignment.Center;
            ampText.text = (ampBar.currentProgress * 10.0f).ToString ("F1");
            Put (ampText, 52, 75);
            ampText.Show ();

            ss = new TouchSelectorSwitch (id, 3, 0, MyOrientation.Horizontal);
            ss.SliderSize = MySliderSize.Large;
            ss.WidthRequest = 170;
            ss.HeightRequest = 30;
            ss.SliderColorOptions [0] = "grey2";
            ss.SliderColorOptions [1] = "pri";
            ss.SliderColorOptions [2] = "seca";
            ss.Name = string.Empty;
            ss.ExposeEvent += OnExpose;
            Put (ss, 5, 110);
            ss.Show ();

            labels = new string[3];

            labels [0] = "Off";
            labels [1] = "Auto";
            labels [2] = "On";

            OutletName = new TouchLabel ();
            OutletName.textColor = "grey4";
            OutletName.WidthRequest = 150;
            OutletName.render.textWrap = MyTextWrap.Shrink;
            Put (OutletName, 5, 2);
            OutletName.Show ();

            Status = new TouchLabel ();
            Status.text = "Off";
            Status.textSize = 12;
            Status.textColor = "grey4";
            Status.WidthRequest = 66;
            Status.textAlignment = MyAlignment.Center;
            Put (Status, 57, 55);
            Status.Show ();

            ShowAll ();
        }

        protected void OnExpose (object sender, ExposeEventArgs args) {
            TouchSelectorSwitch tss = sender as TouchSelectorSwitch;
            int seperation = tss.Allocation.Width / tss.SelectionCount;
            int x = tss.Allocation.Left;

            MyText render = new MyText ();
            render.textWrap = MyTextWrap.Shrink;
            render.alignment = MyAlignment.Center;
            render.font.color = "white";

            foreach (var l in labels) {
                render.text = l;
                render.Render (tss, x, tss.Allocation.Top + 6, seperation);
                x += seperation;
            }
        }
    }

//    public class MyAmpMeter : EventBox
//    {
//        private double _currentAmps;
//        public double currentAmps {
//            get {
//                return _currentAmps;
//            }
//            set {
//                if (value > 10.0)
//                    _currentAmps = 10.0;
//                else if (value < 0.0)
//                    _currentAmps = 0.0;
//                else
//                    _currentAmps = value;
//            }
//        }
//
//        public MyAmpMeter () {
//            SetSizeRequest (160, 70);
//            WidthRequest = 160;
//            VisibleWindow = false;
//            Visible = true;
//
//            _currentAmps = 0.0;
//
//            ExposeEvent += OnEventBoxExpose;
//        }
//
//        protected void OnEventBoxExpose (object sender, ExposeEventArgs args) {
//            EventBox eb = sender as EventBox;
//            using (Context cr = Gdk.CairoHelper.Create (eb.GdkWindow)) {
//                int x = eb.Allocation.Left;
//                int y = eb.Allocation.Top;
//                int width = eb.Allocation.Width;
//                int height = eb.Allocation.Height;
//
//                WidgetGlobal.DrawRoundedRectangle (cr, x, y, width, height, 5);
//                MyColor.SetSource (cr, "white");
//                cr.FillPreserve ();
//
//                MyColor.SetSource (cr, "black");
//                cr.LineWidth = 0.5;
//                cr.Stroke ();
//
//                MyText t = new MyText ();
//
//                double radians, posX, posY;
//
//                double radius = CalcRadius (width - 13, height);
//                double originX = x + (width / 2) - 6.5;
//                double originY = y + radius;
//
//                for (int i = 0; i < 11; ++i) {
//                    radians = -15 * i + 165; //165 - 15 degrees
//                    radians = radians.ToRadians ();
//
//                    posX = CalcX (originX, radius, radians);
//                    posY = CalcY (originY, radius, -radians);
//
//                    t.text = i.ToString ();
//                    t.font.size = 8;
//                    t.font.color = "black";
//                    t.Render (eb, posX.ToInt (), posY.ToInt (), 10);
//                }
//
//                radius = CalcRadius (width, height - 6);
//                originX = x + (width / 2);
//                originY = y + 6 + radius;
//
//                radians = -14.4 * _currentAmps + 162; 
//                radians = radians.ToRadians ();
//
//                posX = CalcX (originX, radius, radians);
//                posY = CalcY (originY, radius, -radians);
//
//                cr.MoveTo (posX, posY);
//                cr.LineTo (originX, y + height - 2);
//                cr.LineWidth = 2.0;
//                MyColor.SetSource (cr, "compl");
//                cr.Stroke ();
//
//                WidgetGlobal.DrawRoundedRectangle (cr, x + 53, y + 25, 54, 16, 2);
//                MyColor.SetSource (cr, "black");
//                cr.LineWidth = 0.75;
//                cr.Stroke ();
//
//                t.text = _currentAmps.ToString ("F2");
//                t.alignment = MyAlignment.Center;
//                t.textWrap = MyTextWrap.Shrink;
//                t.font.size = 11;
//                t.Render (eb, x + 55, y + 25, 50);
//            }
//        }
//
//        protected double CalcRadius (double width, double height) {
//            return (height / 2.0) + (Math.Pow (width, 2.0) / (8 * height));
//        }
//
//        protected double CalcX (double orginX, double radius, double radians) {
//            return orginX + radius * Math.Cos (radians);
//        }
//
//        protected double CalcY (double orginY, double radius, double radians) {
//            return orginY + radius * Math.Sin (radians);
//        }
//    }

}