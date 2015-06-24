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

//            var box = new MyBox (180, 70);
//            box.color = "grey3";
//            Put (box, 0, 0);

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

            labels = new string[3];

            labels [0] = "Off";
            labels [1] = "Auto";
            labels [2] = "On";

            OutletName = new TouchLabel ();
            OutletName.textColor = "grey4";
            OutletName.WidthRequest = 150;
            OutletName.render.textWrap = MyTextWrap.Shrink;
            Put (OutletName, 15, 1);
            OutletName.Show ();

            Status = new TouchLabel ();
            Status.text = "Off";
            Status.textSize = 12;
            Status.textColor = "grey4";
            Status.WidthRequest = 100;
            Status.textAlignment = MyAlignment.Right;
            Put (Status, 65, 52);
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
}

