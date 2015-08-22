using System;
using System.IO.Ports; // for SerialPort
using Gtk;
using Cairo;
using MyWidgetLibrary;
using AquaPic.Drivers;
using AquaPic.Modules;
using AquaPic.Runtime;
using AquaPic.SerialBus;
using AquaPic.Utilites;

namespace AquaPic.UserInterface
{
    public class SerialBusWindow : WindowBase
    {
        SerialBusSlaveWidget[] slaves;
        TouchComboBox c;
        uint timerId;

        public SerialBusWindow (params object[] options) : base () {
            slaves = new SerialBusSlaveWidget[AquaPicBus.Bus1.slaveCount];

            var box = new MyBox (780, 395);
            Put (box, 10, 30);

//            int height = (slaves.Length * 35) + 27;
            var f = new Fixed ();
            f.SetSizeRequest (760, 350);

            var eb = new MyBox (770, 350);
            eb.color = "grey3";
//            eb.SetSizeRequest (750, height);
//            eb.ModifyBg (StateType.Normal, MyColor.NewGtkColor ("grey3"));
//            eb.Add (f);
            Put (eb, 15, 70);
            eb.Show ();

            Put (f, 15, 75);
            f.Show ();

            var l = new TouchLabel ();
            l.text = "Name";
            l.textColor = "black";
            l.textAlignment = MyAlignment.Center;
            l.WidthRequest = 250;
            f.Put (l, 0, 1);
            l.Show ();

            l = new TouchLabel ();
            l.text = "Address";
            l.textColor = "black";
            l.textAlignment = MyAlignment.Center;
            l.WidthRequest = 75;
            f.Put (l, 258, 1);
            l.Show ();

            l = new TouchLabel ();
            l.text = "Status";
            l.textColor = "black";
            l.textAlignment = MyAlignment.Center;
            l.WidthRequest = 325;
            f.Put (l, 335, 1);
            l.Show ();

            l = new TouchLabel ();
            l.text = "Response Time";
            l.textColor = "black";
            l.textAlignment = MyAlignment.Right;
            l.WidthRequest = 170;
            f.Put (l, 595, 1);
            l.Show ();

            string[] names = AquaPicBus.Bus1.slaveNames;
            int[] addresses = AquaPicBus.Bus1.slaveAdresses;

            for (int i = 0; i < slaves.Length; ++i) {
                slaves [i] = new SerialBusSlaveWidget ();
                slaves [i].name = names [i];
                slaves [i].address = addresses [i];
                f.Put (slaves [i], 5, (i * 35) + 23);
                slaves [i].Show ();
            }
                
//            ScrolledWindow sw = new ScrolledWindow ();
//            sw.SetSizeRequest (770, 350);
//            sw.AddWithViewport (eb);
//            //sw.AddWithViewport (f);
//            Put (sw, 15, 70);
//            sw.Show ();

            var b = new TouchButton ();
            b.SetSizeRequest (100, 30);
            b.text = "Open";
            if (AquaPicBus.Bus1.isOpen)
                b.buttonColor = "grey3";
            else
                b.ButtonReleaseEvent += OnOpenButtonRelease;
            Put (b, 685, 35);
            b.Show ();

            if (!AquaPicBus.Bus1.isOpen) {
                string[] portNames = SerialPort.GetPortNames ();
                c = new TouchComboBox (portNames);
            } else {
                c = new TouchComboBox ();
                c.List.Add (AquaPicBus.Bus1.portName);
            }
            c.NonActiveMessage = "Select Port";
            c.WidthRequest = 300;
            Put (c, 380, 35);
            c.Show ();

//            eb = new EventBox ();
//            eb.SetSizeRequest (300, 380);
//            eb.VisibleWindow = false;
//            eb.Visible = true;
//            eb.Add (c);
//            Put (eb, 380, 35);
//            eb.Show ();
//            c.Show ();

            GetSlaveData ();

            timerId = GLib.Timeout.Add (1000, OnTimer);

            Show ();
        }

        public override void Dispose () {
            GLib.Source.Remove (timerId);

            base.Dispose ();
        }

        protected void GetSlaveData () {
            AquaPicBusStatus[] status = AquaPicBus.Bus1.slaveStatus;
            int[] times = AquaPicBus.Bus1.slaveResponseTimes;

            for (int i = 0; i < slaves.Length; ++i) {
                slaves [i].status = status [i];
                slaves [i].responseTime = times [i];
                slaves [i].QueueDraw ();
            }
        }

        protected bool OnTimer () {
            GetSlaveData ();

            return true;
        }

        protected void OnOpenButtonRelease (object sender, ButtonReleaseEventArgs args) {
            TouchButton b = sender as TouchButton;

            if (b != null) { 
                if (c.Active != -1) {
                    AquaPicBus.Bus1.Open (c.activeText);
                    if (AquaPicBus.Bus1.isOpen) {
                        b.buttonColor = "grey3";
                        b.ButtonReleaseEvent -= OnOpenButtonRelease;
                        b.QueueDraw ();
                    }
                } else
                    TouchMessageBox.Show ("No communication port selected");
            }
        }
    }
}

