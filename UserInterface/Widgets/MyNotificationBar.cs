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
using System.Collections.Generic;
using Cairo;
using Gtk;
using AquaPic.Runtime;
using GoodtimeDevelopment.TouchWidget;

namespace AquaPic.UserInterface
{
    public class MyNotificationBar : EventBox
    {
        private uint timerId;
        private int displayedAlarm;
        private int updateAlarm;
        private string alarmName;

        //private string currentscreen = AquaPicGUI.currentScreen;

        public MyNotificationBar () {
            Visible = true;
            VisibleWindow = false;
            SetSizeRequest (800, 19);

            timerId = GLib.Timeout.Add (1000, onTimer);
            displayedAlarm = 0;
            updateAlarm = 0;
            UpdateAlarmText ();

            ExposeEvent += onExpose;
            ButtonReleaseEvent += OnButtonRelease;
            Alarm.AlarmsUpdatedEvent += OnAllAlarmsUpdated;
        }

        public override void Dispose () {
            GLib.Source.Remove (timerId);
            Alarm.AlarmsUpdatedEvent -= OnAllAlarmsUpdated;
            base.Dispose ();
        }

        protected void onExpose (object sender, ExposeEventArgs args) {
            var area = sender as EventBox;
            using (Context cr = Gdk.CairoHelper.Create (area.GdkWindow)) {
                int width = Allocation.Width;

                cr.MoveTo (0, 0);
                cr.LineTo (width, 0);
                cr.LineTo (width, 17);
                cr.LineTo (0, 17);
                cr.MoveTo (0, 0);
                cr.ClosePath ();
                cr.SetSourceRGB (0.15, 0.15, 0.15);
                cr.Fill ();

                cr.MoveTo (0, 17);
                cr.LineTo (width, 17);
                cr.LineTo (width, 19);
                cr.LineTo (0, 19);
                cr.LineTo (0, 17);
                cr.ClosePath ();

                Gradient pat = new LinearGradient (0, 19, width, 19);
                pat.AddColorStop (0.0, TouchColor.NewCairoColor ("grey2", 0.35));
                pat.AddColorStop (0.5, TouchColor.NewCairoColor ("pri"));
                pat.AddColorStop (1.0, TouchColor.NewCairoColor ("grey2", 0.35));
                cr.SetSource (pat);
                cr.Fill ();
                pat.Dispose ();

                TouchText textRender = new TouchText (DateTime.Now.ToLongTimeString ());
                textRender.alignment = TouchAlignment.Right;
                textRender.Render (this, width - 120, 0, 120, 19);

                textRender.text = alarmName;
                if (alarmName == "No Alarms")
                    textRender.font.color = "white";
                else
                    textRender.font.color = "compl";
                textRender.alignment = TouchAlignment.Left;
                textRender.Render (this, 0, 0, 500, 19);
            }
        }

        protected bool onTimer () {
            updateAlarm = ++updateAlarm % 6;
            if (updateAlarm == 0)
                UpdateAlarmText ();

            QueueDraw ();
            return true;
        }

        protected void UpdateAlarmText () {
            List<AlarmData> notAck = Alarm.GetAllNotAcknowledged ();

            if (notAck.Count != 0) {
                if (displayedAlarm >= notAck.Count)
                    displayedAlarm = 0;

                alarmName = notAck[displayedAlarm].name;
                displayedAlarm = ++displayedAlarm % notAck.Count;
            } else if (Alarm.AlarmCount () != 0)
                alarmName = "Alarms";
            else
                alarmName = "No Alarms";
        }

        protected void OnButtonRelease (object sender, ButtonReleaseEventArgs args) {
            if ((args.Event.X >= 0.0) && (args.Event.X <= 250.0)) {
                var tl = this.Toplevel;
                AquaPicGui.AquaPicUserInterface.ChangeScreens ("Alarms", tl, AquaPicGui.AquaPicUserInterface.currentScene);
            }
        }

        protected void OnAllAlarmsUpdated (object sender, AlarmEventArgs args) {
            UpdateAlarmText ();
            QueueDraw ();
        }
    }
}

