using System;
using System.IO.Ports; // for SerialPort
using System.Collections.Generic;
using Gtk;
using Cairo;
using TouchWidgetLibrary;
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
            screenTitle = "AquaPic Bus";

            var f = new Fixed ();
            f.SetSizeRequest (715, 360);

            Put (f, 60, 120);
            f.Show ();

            var l = new TouchLabel ();
            l.text = "Name";
            l.textAlignment = TouchAlignment.Center;
            l.WidthRequest = 235;
            f.Put (l, 0, 1);
            l.Show ();

            l = new TouchLabel ();
            l.text = "Address";
            l.textAlignment = TouchAlignment.Center;
            l.WidthRequest = 75;
            f.Put (l, 243, 1);
            l.Show ();

            l = new TouchLabel ();
            l.text = "Status";
            l.textAlignment = TouchAlignment.Center;
            l.WidthRequest = 295;
            f.Put (l, 320, 1);
            l.Show ();

            l = new TouchLabel ();
            l.text = "Response Time";
            l.textAlignment = TouchAlignment.Right;
            l.WidthRequest = 170;
            f.Put (l, 545, 1);
            l.Show ();

            slaves = new SerialBusSlaveWidget[AquaPicBus.slaveCount];
            string[] names = AquaPicBus.slaveNames;
            int[] addresses = AquaPicBus.slaveAdresses;

            for (int i = 0; i < slaves.Length; ++i) {
                slaves [i] = new SerialBusSlaveWidget ();
                slaves [i].name = names [i];
                slaves [i].address = addresses [i];
                f.Put (slaves [i], 5, (i * 35) + 23);
                slaves [i].Show ();
            }

            var b = new TouchButton ();
            b.SetSizeRequest (100, 30);
            b.text = "Open";
            if (AquaPicBus.isOpen)
                b.buttonColor = "grey3";
            else
                b.ButtonReleaseEvent += OnOpenButtonRelease;
            Put (b, 685, 70);
            b.Show ();

            c = new TouchComboBox ();
            if (!AquaPicBus.isOpen) {
                string[] portNames = SerialPort.GetPortNames ();
                if (Utils.ExecutingOperatingSystem == Platform.Linux) {
                    List<string> sortedPortNames = new List<string> ();
                    foreach (var name in portNames) {
                        if (name.Contains ("USB")) {
                            sortedPortNames.Add (name);
                        }
                    }
                    portNames = sortedPortNames.ToArray ();
                }
                c.comboList.AddRange (portNames);
                c.nonActiveMessage = "Select Port";
            } else {
                c.comboList.Add (AquaPicBus.portName);
                c.activeText = AquaPicBus.portName;
            }
            c.WidthRequest = 300;
            Put (c, 380, 70);
            c.Show ();

            GetSlaveData ();

            timerId = GLib.Timeout.Add (1000, OnTimer);

            Show ();
        }

        public override void Dispose () {
            GLib.Source.Remove (timerId);
            base.Dispose ();
        }

        protected void GetSlaveData () {
            AquaPicBusStatus[] status = AquaPicBus.slaveStatus;
            int[] times = AquaPicBus.slaveResponseTimes;

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
                if (c.active != -1) {
                    AquaPicBus.Open (c.activeText);
                    if (AquaPicBus.isOpen) {
                        b.buttonColor = "grey3";
                        b.ButtonReleaseEvent -= OnOpenButtonRelease;
                        b.QueueDraw ();
                    }
                } else
                    MessageBox.Show ("No communication port selected");
            }
        }
    }
}

