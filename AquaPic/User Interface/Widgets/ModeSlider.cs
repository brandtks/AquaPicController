using System;
using Gtk;
using Cairo;
using TouchWidgetLibrary;

namespace AquaPic.UserInterface
{
    public class ModeSelector : TouchSelectorSwitch
    {
        public string[] labels;

        public ModeSelector () : base (0, 2, 0, TouchOrientation.Horizontal) {
            SliderSize = MySliderSize.Large;
            WidthRequest = 135;
            HeightRequest = 30;
            SliderColorOptions [0] = "grey2";
            SliderColorOptions [1] = "pri";

            labels = new string[2];
            labels [0] = "Manual";
            labels [1] = "Auto";

            ExposeEvent += OnModeExpose;
        }

        protected void OnModeExpose (object sender, ExposeEventArgs args) {
            int seperation = Allocation.Width / SelectionCount;
            int x = Allocation.Left;

            TouchText render = new TouchText ();
            render.textWrap = TouchTextWrap.Shrink;
            render.alignment = TouchAlignment.Center;
            render.font.color = "white";

            foreach (var l in labels) {
                render.text = l;
                render.Render (this, x, Allocation.Top + 6, seperation);
                x += seperation;
            }
        }
    }
}

