using System;
using System.Collections.Generic;
using Cairo;
using Gtk;
using AquaPic.Runtime;
using MyWidgetLibrary;

namespace AquaPic.UserInterface
{
    public class MyNotificationBar : EventBox
    {
        private uint timerId;
        private int displayedAlarm;
        private int updateAlarm;
        private string alarmName;

        private string currentscreen = GuiGlobal.currentScreen;

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
            base.Dispose ();
        }

        protected void onExpose (object sender, ExposeEventArgs args) {
            var area = sender as EventBox;
            using (Context cr = Gdk.CairoHelper.Create (area.GdkWindow)) {
                cr.MoveTo (0, 0);
                cr.LineTo (800, 0);
                cr.LineTo (800, 17);
                cr.LineTo (0, 17);
                cr.MoveTo (0, 0);
                cr.ClosePath ();
                cr.SetSourceRGB (0.15, 0.15, 0.15);
                cr.Fill ();

                cr.MoveTo (0, 17);
                cr.LineTo (800, 17);
                cr.LineTo (800, 19);
                cr.LineTo (0, 19);
                cr.LineTo (0, 17);
                cr.ClosePath ();

                Gradient pat = new LinearGradient (0, 19, 800, 19);
                pat.AddColorStop (0.0, MyColor.NewCairoColor ("grey2", 0.35));
                pat.AddColorStop (0.5, MyColor.NewCairoColor ("pri"));
                pat.AddColorStop (1.0, MyColor.NewCairoColor ("grey2", 0.35));
                cr.SetSource (pat);
                cr.Fill ();
                pat.Dispose ();

                Pango.Layout l = new Pango.Layout (this.PangoContext);
                l.Width = Pango.Units.FromPixels (120);
                l.FontDescription = Pango.FontDescription.FromString ("Courier New 11");

                l.Alignment = Pango.Alignment.Right;
                l.SetMarkup ("<span color=\"white\">" 
                    + DateTime.Now.ToLongTimeString () 
                    + "</span>");
                GdkWindow.DrawLayout (Style.TextGC(StateType.Normal), 680, 0, l);

                string fontColor;
                if (alarmName == "No Alarms")
                    fontColor = "white";
                else
                    fontColor = MyColor.ToHTML ("compl");
                l.Alignment = Pango.Alignment.Left;
                l.Width = Pango.Units.FromPixels (400);
                l.SetMarkup ("<span color=\""
                    + fontColor
                    + "\">" 
                    + alarmName
                    + "</span>");
                GdkWindow.DrawLayout (Style.TextGC(StateType.Normal), 0, 0, l);

                l.Dispose ();
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

                alarmName = notAck [displayedAlarm].name;
                displayedAlarm = ++displayedAlarm % notAck.Count;
            } else if (Alarm.AlarmCount () != 0)
                alarmName = "Alarms";
            else
                alarmName = "No Alarms";
        }

        protected void OnButtonRelease (object sender, ButtonReleaseEventArgs args) {
            if ((args.Event.X >= 0.0) && (args.Event.X <= 250.0))
                GuiGlobal.ChangeScreens ("Alarms");
        }

        protected void OnAllAlarmsUpdated (object sender) {
            UpdateAlarmText ();
            QueueDraw ();
        }
    }
}

