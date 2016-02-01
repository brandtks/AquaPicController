using System;
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

