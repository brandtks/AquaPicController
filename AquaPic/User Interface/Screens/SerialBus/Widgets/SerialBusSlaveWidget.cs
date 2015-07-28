using System;
using Gtk;
using Cairo;
using MyWidgetLibrary;
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
            SetSizeRequest (760, 30);

            nameTextBox = new TouchTextBox ();
            nameTextBox.WidthRequest = 250;
            Put (nameTextBox, 0, 0);

            addressTextBox = new TouchTextBox ();
            addressTextBox.WidthRequest = 75;
            addressTextBox.textAlignment = MyAlignment.Center;
            Put (addressTextBox, 255, 0);

            statusTextBox = new TouchTextBox ();
            statusTextBox.WidthRequest = 325;
            Put (statusTextBox, 335, 0);

            responseTimeTextBox = new TouchTextBox ();
            responseTimeTextBox.WidthRequest = 95;
            Put (responseTimeTextBox, 665, 0);
        }
    }
}

