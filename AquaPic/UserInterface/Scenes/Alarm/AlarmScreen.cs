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
using System.Collections.Generic;
using System.Text;
using Gtk;
using Cairo;
using TouchWidgetLibrary;
using AquaPic.Runtime;

namespace AquaPic.UserInterface
{
    public class AlarmWindow : SceneBase
    {
        public TextView tv;

        public AlarmWindow (params object[] options) : base () {
            sceneTitle = "Current Alarms";

            var acknowledgeButton = new TouchButton ();
            acknowledgeButton.SetSizeRequest (100, 60);
            acknowledgeButton.text = "Acknowledge Alarms";
            acknowledgeButton.buttonColor = "compl";
            acknowledgeButton.ButtonReleaseEvent += (o, args) => {
                Alarm.Acknowledge ();
                Update ();
            };

            tv = new TextView ();
            tv.ModifyFont (Pango.FontDescription.FromString ("Sans 11"));
            tv.ModifyBase (StateType.Normal, TouchColor.NewGtkColor ("grey4"));
            tv.CanFocus = false;

            var sw = new ScrolledWindow ();
            sw.SetSizeRequest (720, 340);
            sw.VScrollbar.WidthRequest = 30;
            sw.HScrollbar.HeightRequest = 30;
            sw.Add (tv);
            Put (sw, 65, 60);
            sw.Show ();
            tv.Show ();

            if (options.Length >= 2) {
                var lastScreen = options [1] as string;
                if (lastScreen != null) {
                    var b = new TouchButton ();
                    b.SetSizeRequest (100, 60);
                    b.text = "Back\n" + lastScreen;

                    b.ButtonReleaseEvent += (o, args) => {
                        var tl = this.Toplevel;
                        AquaPicGui.AquaPicUserInterface.ChangeScreens (lastScreen, tl, AquaPicGui.AquaPicUserInterface.currentScene);
                    };
                    Put (b, 575, 405);
                    b.Show ();

                    Put (acknowledgeButton, 685, 405);
                } else {
                    Put (acknowledgeButton, 575, 405);
                }
            }
            acknowledgeButton.Show ();

            Update ();
            Show ();
        }

        protected void Update () {
            List<AlarmData> alarming = Alarm.GetAllAlarming ();
            TextBuffer tb = tv.Buffer;
            tb.Text = string.Empty;

            foreach (var a in alarming) {
                var tag = new TextTag (null);
                if (a.acknowledged)
                    tag.ForegroundGdk = TouchColor.NewGtkColor ("seca");
                else
                    tag.ForegroundGdk = TouchColor.NewGtkColor ("compl");
                TextTagTable ttt = tb.TagTable;
                ttt.Add (tag);

                var ti = tb.EndIter;
                tb.InsertWithTags (ref ti, string.Format ("{0:MM/dd hh:mm:ss}: {1}\n", a.postTime, a.name), tag);
            }
        }

        protected override bool OnUpdateTimer () {
            Update ();
            return true;
        }
    }
}

