using System;
using Cairo;
using Gtk;

namespace MyWidgetLibrary
{
    public class TouchMessageBox
    {
        public static void Show (string msg) {
            var ms = new Dialog (string.Empty, null, DialogFlags.DestroyWithParent);

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

            ms.Run ();
            ms.Destroy ();
        }
    }

    public class TouchMessageDialog : Gtk.Dialog
    {
        public TouchMessageDialog (string msg) 
            : base (string.Empty, null, DialogFlags.DestroyWithParent)
        {
            this.ModifyBg (StateType.Normal, MyColor.NewGtkColor ("grey0"));

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
        }
    }
}

