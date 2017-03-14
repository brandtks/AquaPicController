using System;
using Gtk;

namespace TouchWidgetLibrary  
{
    public class TouchUpDownButtons : Fixed
    {
        public TouchButton up;
        public TouchButton down;
        private bool buttonsPlaced;

        public TouchUpDownButtons () {
            SetSizeRequest (98, 51);

            up = new TouchButton ();
            up.Name = "Up";
            up.text = Convert.ToChar (0x22C0).ToString (); // 2191

            down = new TouchButton ();
            down.Name = "Down";
            down.text = Convert.ToChar (0x22C1).ToString (); // 2193

            buttonsPlaced = false;

            ExposeEvent += (o, args) => {
                if (!buttonsPlaced)
                    PlaceButtons ();
            };
        }

        void PlaceButtons () {
            int width = (Allocation.Width - 1) / 2;
            int height = Allocation.Height;

            up.SetSizeRequest (width, height);
            Put (up, 0, 0);
            up.QueueDraw ();

            down.SetSizeRequest (width, height);
            Put (down, width + 1, 0);
            down.QueueDraw ();

            buttonsPlaced = true;
        }
    }
}

