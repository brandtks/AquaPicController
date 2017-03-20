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
using Cairo;
using TouchWidgetLibrary;
using AquaPic.SerialBus;
using AquaPic.Utilites;

namespace AquaPic.UserInterface
{
    public class SerialBusSlaveWidget : Fixed
    {
        public string name {
            set {
                nameTextBox.text = value;
            }
        }

        public int address {
            set {
                addressTextBox.text = value.ToString ();
            }
        }

        public AquaPicBusStatus status {
            set {
                statusTextBox.text = Utils.GetDescription (value);
            }
        }

        public int responseTime {
            set {
                responseTimeTextBox.text = value.ToString ();
            }
        }

        public TouchTextBox nameTextBox;
        public TouchTextBox addressTextBox;
        public TouchTextBox statusTextBox;
        public TouchTextBox responseTimeTextBox;

        public SerialBusSlaveWidget () {
            SetSizeRequest (715, 30);

            nameTextBox = new TouchTextBox ();
            nameTextBox.WidthRequest = 235;
            Put (nameTextBox, 0, 0);

            addressTextBox = new TouchTextBox ();
            addressTextBox.WidthRequest = 75;
            addressTextBox.textAlignment = TouchAlignment.Center;
            Put (addressTextBox, 240, 0);

            statusTextBox = new TouchTextBox ();
            statusTextBox.WidthRequest = 295;
            Put (statusTextBox, 320, 0);

            responseTimeTextBox = new TouchTextBox ();
            responseTimeTextBox.WidthRequest = 95;
            responseTimeTextBox.textAlignment = TouchAlignment.Center;
            Put (responseTimeTextBox, 620, 0);
        }
    }
}

