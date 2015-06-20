using System;
using Gtk;
using Cairo;
using MyWidgetLibrary;

namespace AquaPic
{
    public class PowerOutletSlider : TouchSelectorSwitch
    {
        private string[] labels;
        public string OutletName;
        public string Status;
        public MyColor StatusColor;
        public TouchSelectorSwitch ss;

        public PowerOutletSlider (int id) : base (id, 3, 0, MyOrientation.Horizontal) {
            SliderSize = MySliderSize.Large;
            WidthRequest = 170;
            HeightRequest = 30;
            SliderColorOptions [0] = "grey2";
            SliderColorOptions [1] = "pri";
            SliderColorOptions [2] = "seca";

            labels = new string[3];
            labels [0] = "Off";
            labels [1] = "Auto";
            labels [2] = "On";

            Name = string.Empty;
            Status = "Off";
            StatusColor = new MyColor ("grey4");

            ExposeEvent += OnOutletExpose;
        }
        
        protected void OnOutletExpose (object sender, ExposeEventArgs args) {
            using (Context cr = Gdk.CairoHelper.Create (this.GdkWindow)) {
                cr.Rectangle (
                    Allocation.Left - 5, 
                    Allocation.Top - 20, 
                    Allocation.Width + 10, 
                    Allocation.Height + 40);
                MyColor.SetSource (cr, "grey3", 0.55);
                cr.Fill ();

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

                l.Width = Pango.Units.FromPixels (Allocation.Width + 4);
                l.Alignment = Pango.Alignment.Left;
                l.SetMarkup ("<span color=\"" + MyColor.ToHTML ("grey4") + "\">"
                    + OutletName 
                    + "</span>"); 
                // GdkWindow.DrawLayout (Style.TextGC (StateType.Normal), Allocation.Left + 8, Allocation.Top - 19, l);
                GdkWindow.DrawLayout (
                    Style.TextGC (StateType.Normal), 
                    Allocation.Left - 2, 
                    Allocation.Top + Allocation.Height + 2, 
                    l);
                
                l.FontDescription = Pango.FontDescription.FromString ("Courier New 12");
                l.Alignment = Pango.Alignment.Right;
                l.SetMarkup ("<span color=\"" + StatusColor.ToHTML () + "\">"
                    + Status 
                    + "</span>"); 
                GdkWindow.DrawLayout (Style.TextGC (StateType.Normal), Allocation.Left - 2, Allocation.Top - 19, l);


                l.Dispose ();
            }
        }
    }
}

