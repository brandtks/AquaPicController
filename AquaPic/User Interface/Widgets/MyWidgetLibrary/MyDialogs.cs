using System;
using Cairo;
using Gtk;

namespace MyWidgetLibrary
{
    public class TouchMessageBox
    {
        public static void Show (string msg) {
            var ms = new Dialog (string.Empty, null, DialogFlags.DestroyWithParent);
            ms.KeepAbove = true;

            #if RPI_BUILD
            ms.Decorated = false;
            #endif

            ms.ModifyBg (StateType.Normal, MyColor.NewGtkColor ("grey0"));

            var btn = new TouchButton ();
            btn.text = "Ok";
            btn.HeightRequest = 30;
            btn.ButtonReleaseEvent += (o, args) =>
                ms.Respond (ResponseType.Ok);
            ms.ActionArea.Add (btn);

            var label = new Label ();
            label.LineWrap = true;
            label.Text = msg;
            label.ModifyFg (StateType.Normal, MyColor.NewGtkColor ("white"));
            label.ModifyFont (Pango.FontDescription.FromString ("Sans 11"));
            ms.VBox.Add (label);
            label.Show ();

            ms.ExposeEvent += (o, args) => {
                using (Context cr = Gdk.CairoHelper.Create (ms.GdkWindow)) {
                    cr.MoveTo (ms.Allocation.Left, ms.Allocation.Top);
                    cr.LineTo (ms.Allocation.Right, ms.Allocation.Top);
                    cr.LineTo (ms.Allocation.Right, ms.Allocation.Bottom);
                    cr.LineTo (ms.Allocation.Left, ms.Allocation.Bottom);
                    cr.ClosePath ();
                    cr.LineWidth = 1.8;
                    MyColor.SetSource (cr, "grey4");
                    cr.Stroke ();
                }
            };

            ms.Run ();
            ms.Destroy ();
        }
    }

    public class TouchMessageDialog : Gtk.Dialog
    {
        public TouchMessageDialog (string msg, Gtk.Window parent) 
            : base (string.Empty, parent, DialogFlags.DestroyWithParent)
        {
            this.ModifyBg (StateType.Normal, MyColor.NewGtkColor ("grey0"));

            this.KeepAbove = true;

            #if RPI_BUILD
            Decorated = false;
            #endif

            var btn = new TouchButton ();
            btn.text = "Yes";
            btn.HeightRequest = 30;
            btn.ButtonReleaseEvent += (o, args) => 
                this.Respond (ResponseType.Yes);
            this.ActionArea.Add (btn);

            btn = new TouchButton ();
            btn.text = "No";
            btn.HeightRequest = 30;
            btn.ButtonReleaseEvent += (o, args) => 
                this.Respond (ResponseType.No);
            this.ActionArea.Add (btn);

            var label = new Label ();
            label.LineWrap = true;
            label.Text = msg;
            label.ModifyFg (StateType.Normal, MyColor.NewGtkColor ("white"));
            label.ModifyFont (Pango.FontDescription.FromString ("Sans 11"));
            this.VBox.Add (label);
            label.Show ();

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
        }
    }
}

