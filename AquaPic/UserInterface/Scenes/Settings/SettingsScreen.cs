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
using Gtk;
using Cairo;
using GoodtimeDevelopment.TouchWidget;
using AquaPic.Runtime;

namespace AquaPic.UserInterface
{
    public class SettingsWindow : SceneBase
    {
        private AquaPicGui topWindow;

        public SettingsWindow (params object[] options) : base () {
            sceneTitle = "Settings";

            if (options.Length >= 1) {
                topWindow = options [0] as AquaPicGui;
                if (topWindow != null) {
                    if (topWindow.IsTopLevel) {
                        var b = new TouchButton ();
                        b.SetSizeRequest (100, 60);
                        b.text = "Unfullscreen";
                        b.ButtonReleaseEvent += (o, args) => {
                            topWindow.ShowDecoration ();
                            #if RPI_BUILD
                            topWindow.Unfullscreen ();
                            #endif
                        };
                        Put (b, 685, 140);
                        b.Show ();

                        b = new TouchButton ();
                        b.SetSizeRequest (100, 60);
                        b.text = "Back";
                        b.ButtonReleaseEvent += (o, args) => AquaPicGui.AquaPicUserInterface.ChangeScreens ("Home");
                        Put (b, 685, 210);
                        b.Show ();
                    } else
                        topWindow = null;
                }
            }

            var btn = new TouchButton ();
            btn.SetSizeRequest (100, 60);
            btn.text = "Close";
            btn.ButtonReleaseEvent += OnCloseButtonRelease;
            Put (btn, 685, 70);
            btn.Show ();

            Show ();
        }

        public override void Dispose () {
            if (topWindow != null) {
                if (topWindow.IsTopLevel) {
                    topWindow.HideDecoration ();
#if RPI_BUILD
                    topWindow.Fullscreen ();
#endif
                }
            }
            base.Dispose ();
        }

        protected void OnCloseButtonRelease (object sender, ButtonReleaseEventArgs args) {
            var parent = this.Toplevel as Gtk.Window;
            if (parent != null) {
                if (!parent.IsTopLevel)
                    parent = null;
            }

            var ms = new TouchDialog ("Are you sure?\nClosing application ends all controller functionality!", parent);

            ms.Response += (o, a) => {
                if (a.ResponseId == ResponseType.Yes)
                    Application.Quit ();
            };

            ms.Run ();
            ms.Destroy ();
        }
    }
}

