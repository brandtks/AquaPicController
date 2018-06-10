#region License

/*
    AquaPic Main Control - Handles all functionality for the AquaPic aquarium controller.

    Copyright (c) 2017 Goodtime Development

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see http://www.gnu.org/licenses/
*/

#endregion // License

using System;
using System.IO.Ports; // for SerialPort
using System.Collections.Generic;
using Gtk;
using GoodtimeDevelopment.TouchWidget;
using GoodtimeDevelopment.Utilites;
using AquaPic.SerialBus;

namespace AquaPic.UserInterface
{
    public class SerialBusWindow : SceneBase
    {
        SerialBusSlaveWidget[] slaves;
        TouchComboBox c;

        public SerialBusWindow (params object[] options) : base () {
            sceneTitle = "AquaPic Bus";

            var l = new TouchLabel ();
            l.text = "Name";
            l.textAlignment = TouchAlignment.Center;
            l.WidthRequest = 235;
            Put (l, 60, 121);
            l.Show ();

            l = new TouchLabel ();
            l.text = "Address";
            l.textAlignment = TouchAlignment.Center;
            l.WidthRequest = 75;
            Put (l, 303, 121);
            l.Show ();

            l = new TouchLabel ();
            l.text = "Status";
            l.textAlignment = TouchAlignment.Center;
            l.WidthRequest = 295;
            Put (l, 380, 121);
            l.Show ();

            l = new TouchLabel ();
            l.text = "Response Time";
            l.textAlignment = TouchAlignment.Right;
            l.WidthRequest = 170;
            Put (l, 605, 121);
            l.Show ();

            slaves = new SerialBusSlaveWidget[AquaPicBus.slaveCount];
            string[] names = AquaPicBus.slaveNames;
            int[] addresses = AquaPicBus.slaveAdresses;

            for (int i = 0; i < slaves.Length; ++i) {
                slaves[i] = new SerialBusSlaveWidget ();
                slaves[i].name = names[i];
                slaves[i].address = addresses[i];
                Put (slaves[i], 65, (i * 35) + 143);
                slaves[i].Show ();
            }

            if (AquaPicBus.slaveCount == 0) {
                var blankWidget = new SerialBusSlaveWidget ();
                Put (blankWidget, 65, 143);
                blankWidget.Show ();
            }

            var b = new TouchButton ();
            b.SetSizeRequest (100, 30);
            b.text = "Open";
            if (AquaPicBus.isOpen) {
                b.buttonColor = "grey3";
            } else {
                b.ButtonReleaseEvent += OnOpenButtonRelease;
            }
            Put (b, 685, 70);
            b.Show ();

            c = new TouchComboBox ();
            if (!AquaPicBus.isOpen) {
                string[] portNames = SerialPort.GetPortNames ();
                if (Utils.ExecutingOperatingSystem == Platform.Linux) {
                    var sortedPortNames = new List<string> ();
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
            Show ();
        }

        protected void GetSlaveData () {
            AquaPicBusStatus[] status = AquaPicBus.slaveStatus;
            int[] times = AquaPicBus.slaveResponseTimes;

            for (int i = 0; i < slaves.Length; ++i) {
                slaves[i].status = status[i];
                slaves[i].responseTime = times[i];
                slaves[i].QueueDraw ();
            }
        }

        protected override bool OnUpdateTimer () {
            GetSlaveData ();
            return true;
        }

        protected void OnOpenButtonRelease (object sender, ButtonReleaseEventArgs args) {
            var b = sender as TouchButton;

            if (b != null) {
                if (c.activeIndex != -1) {
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

