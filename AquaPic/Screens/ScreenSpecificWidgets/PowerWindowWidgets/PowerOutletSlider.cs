using System;
using Gtk;
using Cairo;
using MyWidgetLibrary;

namespace AquaPic
{
    public class PowerOutletSlider : Fixed
    {
        private string[] labels;
        public TouchLabel OutletName;
        public TouchLabel Status;
        public TouchSelectorSwitch ss;

        public PowerOutletSlider (int id) {
            SetSizeRequest (180, 70);

            var box = new MyBox (180, 70);
            box.color = "grey3";
            Put (box, 0, 0);

            ss = new TouchSelectorSwitch (id, 3, 0, MyOrientation.Horizontal);
            ss.SliderSize = MySliderSize.Large;
            ss.WidthRequest = 170;
            ss.HeightRequest = 30;
            ss.SliderColorOptions [0] = "grey2";
            ss.SliderColorOptions [1] = "pri";
            ss.SliderColorOptions [2] = "seca";
            ss.Name = string.Empty;
            ss.ExposeEvent += OnExpose;
            Put (ss, 5, 20);
            ss.Show ();


//            labels = new TouchLabel[3];
//            for (int i = 0; i < labels.Length; ++i)
//                labels [i] = new TouchLabel ();
//
//            foreach (var l in labels) { 
//                l.render.alignment = MyAlignment.Center;
//                l.WidthRequest = seperation;
//                l.render.textWrap = MyTextWrap.Shrink;
//                l.textColor = "grey3";
//                Put (l, x, 26);
//                l.Show ();
//
//                x += seperation;
//            }

            labels = new string[3];

            labels [0] = "Off";
            labels [1] = "Auto";
            labels [2] = "On";

            OutletName = new TouchLabel ();
            OutletName.textColor = "grey4";
            Put (OutletName, 3, 1);
            OutletName.Show ();

            Status = new TouchLabel ();
            Status.text = "Off";
            Status.textSize = 12;
            Status.textColor = "grey4";
            Put (Status, 3, 52);
            Status.Show ();

            ShowAll ();
        }

        protected void OnExpose (object sender, ExposeEventArgs args) {
            TouchSelectorSwitch tss = sender as TouchSelectorSwitch;
            using (Context cr = Gdk.CairoHelper.Create (tss.GdkWindow)) {
                int seperation = tss.Allocation.Width / tss.SelectionCount;
                int x = tss.Allocation.Left;

                Pango.Layout l = new Pango.Layout (tss.PangoContext);
                l.Width = Pango.Units.FromPixels (seperation);
                l.Wrap = Pango.WrapMode.WordChar;
                l.Alignment = Pango.Alignment.Center;
                l.FontDescription = Pango.FontDescription.FromString ("Courier New 11");

                for (int i = 0; i < labels.Length; ++i) {
                    l.SetMarkup ("<span color=\"white\">"
                    + labels [i]
                    + "</span>"); 
                    GdkWindow.DrawLayout (Style.TextGC (StateType.Normal), x, tss.Allocation.Top + 6, l);
                    x += seperation;
                }

                l.Dispose ();
            }
        }
    }
}

