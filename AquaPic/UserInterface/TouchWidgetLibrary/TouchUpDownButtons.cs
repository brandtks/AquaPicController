#region License

/*
    AquaPic Main Control - Handles all functionality for the AquaPic aquarium controller.

    Copyright (c) 2017 Skyler Brandt

    This program is free software; you can redistribute it and/or
    modify it under the terms of the GNU General Public License
    as published by the Free Software Foundation; either version 2
    of the License, or (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program; if not, write to the Free Software
    Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.

    Optionally you can also view the license at <http://www.gnu.org/licenses/>.
*/

#endregion // License

﻿using System;
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

