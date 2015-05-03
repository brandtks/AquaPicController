using System;
using Gtk;
using Cairo;
using MyWidgetLibrary;

namespace AquaPic
{
    public class LightingModeSlider : TouchSelectorSwitch
    {
        private string[] labels;

        public LightingModeSlider () : base (0, 2, 0, MyOrientation.Horizontal) {
            SliderSize = MySliderSize.Large;
            WidthRequest = 135;
            HeightRequest = 30;
            SliderColorOptions [0].ChangeColor ("grey2");
            SliderColorOptions [1].ChangeColor ("pri");

            labels = new string[2];
            labels [0] = "Manual";
            labels [1] = "Auto";

            ExposeEvent += OnModeExpose;
        }

        protected void OnModeExpose (object sender, ExposeEventArgs args) {
            using (Context cr = Gdk.CairoHelper.Create (this.GdkWindow)) {
                int seperation = Allocation.Width / SelectionCount;
                int x = Allocation.Left;

                Pango.Layout l = new Pango.Layout (PangoContext);
                l.Width = Pango.Units.FromPixels (seperation);
                l.Wrap = Pango.WrapMode.WordChar;
                l.Alignment = Pango.Alignment.Center;
                l.FontDescription = Pango.FontDescription.FromString ("Courier New 11");

                for (int i = 0; i < labels.Length; ++i) {
                    l.SetMarkup ("<span color=\"white\">" 
                        + labels[i] 
                        + "</span>"); 
                    GdkWindow.DrawLayout (Style.TextGC (StateType.Normal), x, Allocation.Top + 6, l);
                    x += seperation;
                }
                l.Dispose ();
            }
        }
    }
}

