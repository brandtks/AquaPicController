using System;
using System.IO;
using Gtk;
using Cairo;
using MyWidgetLibrary;
using AquaPic.PowerDriver;
using AquaPic.Globals;

namespace AquaPic
{
    public partial class PowerWindow : MyBackgroundWidget
    {
        private MyPlugWidget[] plugs;

        public PowerWindow (ButtonReleaseEventHandler OnTouchButtonRelease) : base ("Power", OnTouchButtonRelease) { 
            DrawingArea box = new DrawingArea ();
            box.ExposeEvent += onBoxExpose;
            box.SetSizeRequest (600, 270);
            Put (box, 555, 390);
            box.Show ();
             
            MyState[] states = Power.GetAllStates (0);
            string[] names = Power.GetAllNames (0);
            int x, y;
            IndividualControl IndCon;
            IndCon.Group = 0;
            plugs = new MyPlugWidget[8];

            for (int i = 0; i < 8; ++i) { 
                plugs [i] = new MyPlugWidget ((byte)i);

                plugs [i].PlugClicked += plugClick;
                if (states [i] == MyState.On)
                    plugs [i].onOff = true;

                if (string.IsNullOrWhiteSpace (names [i]))
                    plugs [i].PlugName = (i + 1).ToString ();
                else
                    plugs [i].PlugName = names [i];
                IndCon.Individual = (byte)i;
                Power.AddHandlerOnStateChange (IndCon, PlugStateChange);

                if (i < 4) {
                    x = (i * 100) + 755;
                    y = 400;
                } else {
                    x = ((i - 4) * 100) + 755;
                    y = 560;
                }

                Put (plugs [i], x, y);
                plugs [i].Show ();
            }

            Show ();
        }

        protected void OnDeleteEvent (object sender, DeleteEventArgs a) {
            Application.Quit ();
            a.RetVal = true;
        }

        protected void onBoxExpose (object sender, ExposeEventArgs args) {
            DrawingArea area = (DrawingArea)sender;

            using (Context cr = Gdk.CairoHelper.Create (area.GdkWindow)) {
                cr.SetSourceRGB(0.15, 0.15, 0.15);
                cr.Rectangle (0, 0, 600, 270);
                cr.Fill ();
            }
        }

        protected void plugClick (object o, ButtonPressEventArgs args) {
            MyPlugWidget plug = (MyPlugWidget)o;

            IndividualControl p;
            MyState s;
            p.Group = 0;
            p.Individual = (byte)plug.id;
            if (!plug.onOff)
                s = MyState.On;
            else
                s = MyState.Off;

            Power.SetPlugState (p, s, true);
        }

        protected void PlugStateChange (object sender, StateChangeEventArgs args) {
            if (args.state == MyState.On)
                plugs [args.plugID].onOff = true;
            else
                plugs [args.plugID].onOff = false;

            plugs [args.plugID].QueueDraw ();
        }
    }
}

