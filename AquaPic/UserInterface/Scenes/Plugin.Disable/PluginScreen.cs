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

﻿/***************************************************************************************************/
/*        NOT COMPILED                                                                             */
/***************************************************************************************************/

using System;
using Gtk;
using Cairo;
using MyWidgetLibrary;
using AquaPic.Runtime;

namespace AquaPic.UserInterface
{
    public class PluginWindow : WindowBase
    {
        public PluginWindow (params object[] options) : base () {
            var box = new MyBox (780, 395);
            Put (box, 10, 30);

            int x = 15;
            int y = 35;
            foreach (var p in Plugin.AllPlugins.Values) {
                var b = new TouchButton ();
                b.SetSizeRequest (250, 30);
                b.text = p.name;
                b.textColor = "black";
                if (!p.flags.HasFlag (ScriptFlags.Compiled))
                    b.buttonColor = "compl";
                b.ButtonReleaseEvent += OnButtonClick;
                Put (b, x, y);

                x += 260;

                if (x >= 795) {
                    x = 15;
                    y += 40;
                }
            }

            ShowAll ();
        }

        protected void OnButtonClick (object sender, ButtonReleaseEventArgs args) {
            TouchButton b = sender as TouchButton;
            GuiGlobal.ChangeScreens ("Edit Plugin", Plugin.AllPlugins [b.text]);
        }
    }
}