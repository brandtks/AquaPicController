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
using Gtk;
using TouchWidgetLibrary;
using AquaPic.Runtime;

namespace AquaPic.UserInterface
{
    public delegate void ChangeSceneHandler (SceneData screen, params object [] options);

    public class AquaPicGui : Window
    {
        SceneBase current;
        Fixed f;
        MySideBar side;
        MyNotificationBar notification;

        string _currentScene;
        public string currentScene {
            get {
                return _currentScene;
            }
        }

        public Dictionary<string, SceneData> scenes;

        public event ChangeSceneHandler ChangeSceneEvent;

        static AquaPicGui _userInterface;
        public static AquaPicGui AquaPicUserInterface {
            get {
                return _userInterface;
            }
        }

        protected AquaPicGui () : base (WindowType.Toplevel) {
            Name = "AquaPicGUI";
            Title = "AquaPic Controller Version 1";
            WindowPosition = WindowPosition.Center;
            SetSizeRequest (800, 480);
            Resizable = false;
            AllowGrow = false;

            DeleteEvent += (o, args) => {
                Application.Quit ();
                args.RetVal = true;
            };

            ModifyBg (StateType.Normal, TouchColor.NewGtkColor ("grey0"));

#if RPI_BUILD
            this.Decorated = false;
            this.Fullscreen ();
#endif

            GLib.ExceptionManager.UnhandledException += (args) => {
                Exception ex = args.ExceptionObject as Exception;
                Logger.AddError (ex.ToString ());
                args.ExitApplication = false;
            };

            ChangeSceneEvent += ScreenChange;

            scenes = new Dictionary<string, SceneData>() {
                { "Power", new SceneData ("Power", true, (options) => {return new PowerWindow (options);}) },
                { "Lighting", new SceneData ("Lighting", true, (options) => {return new LightingWindow (options);}) },
                { "Temperature", new SceneData ("Temperature", true, (options) => {return new TemperatureWindow (options);}) },
                { "Water Level", new SceneData ("Water Level", true, (options) => {return new WaterLevelWindow (options);}) },
                { "Chemistry", new SceneData ("Chemistry", true, (options) => {return new ChemistryWindow (options);}) },
                { "Analog Output", new SceneData ("Analog Output", true, (options) => {return new AnalogOutputWindow (options);}) },
                { "Analog Input", new SceneData ("Analog Input", true, (options) => {return new AnalogInputWindow (options);}) },
                { "Digital Input", new SceneData ("Digital Input", true, (options) => {return new DigitalInputWindow (options);}) },
                { "Serial Bus", new SceneData ("Serial Bus", true, (options) => {return new SerialBusWindow (options);}) },
                { "Alarms", new SceneData ("Alarms", true, (options) => {return new AlarmWindow (options);}) },
                { "Logger", new SceneData ("Logger", true, (options) => {return new LoggerWindow (options);}) },
                { "Settings", new SceneData ("Settings", true, (options) => {return new SettingsWindow (options);}) },
                { "Menu", new SceneData ("Menu", false, (options) => {return new MenuWindow (options);}) },
                { "Home", new SceneData ("Home", true, (options) => {return new HomeWindow (options);}) }
            };

            _currentScene = "Home";

            f = new Fixed ();
            f.SetSizeRequest (800, 480);

            current = scenes [_currentScene].CreateInstance ();
            f.Put (current, 0, 0);
            current.Show ();

            side = new MySideBar (scenes, _currentScene);
            f.Put (side, 0, 20);
            side.Show ();

            notification = new MyNotificationBar ();
            f.Put (notification, 0, 0);
            notification.Show ();

            Add (f);
            f.Show ();

            Show ();
        }

        public static AquaPicGui CreateInstance () {
            if (_userInterface == null) {
                _userInterface = new AquaPicGui();
                return _userInterface;
            }

            throw new Exception("User interface is already created");
        }

        public void ChangeScreens (string name, params object [] options) {
            if (!scenes.ContainsKey (name))
                throw new Exception ("Screen does not exist");

            if (_currentScene == name) {
                Logger.Add ("Changing screen to current screen: {0}", name);
            }

            _currentScene = name;

            if (ChangeSceneEvent != null) {
                ChangeSceneEvent (scenes [name], options);
            }
        }

        public void ShowDecoration () {
            SetSizeRequest (800, 425);
            Decorated = true;
        }

        public void HideDecoration () {
            SetSizeRequest (800, 480);
#if RPI_BUILD
            Decorated = false;
#endif
        }

        protected void ScreenChange (SceneData screen, params object [] options) {
            f.Remove (current);
            current.Destroy ();
            current.Dispose ();
            current = screen.CreateInstance (options);
            f.Put (current, 0, 0);

            f.Remove (side);
            side.Destroy ();
            side.Dispose ();
            side = new MySideBar ();
            f.Put (side, 0, 20);
            side.Show ();

            if (_currentScene == "Logger") {
                var logScreen = current as LoggerWindow;
                if (logScreen != null) {
                    side.ExpandEvent += (sender, e) => {
                        logScreen.tv.Visible = false;
                        logScreen.tv.QueueDraw ();
                    };

                    side.CollapseEvent += (sender, e) => {
                        logScreen.tv.Visible = true;
                        logScreen.tv.QueueDraw ();
                    };
                }
            } else if (_currentScene == "Alarms") {
                var alarmScreen = current as AlarmWindow;
                if (alarmScreen != null) {
                    side.ExpandEvent += (sender, e) => {
                        alarmScreen.tv.Visible = false;
                        alarmScreen.tv.QueueDraw ();
                    };

                    side.CollapseEvent += (sender, e) => {
                        alarmScreen.tv.Visible = true;
                        alarmScreen.tv.QueueDraw ();
                    };
                }

            }

            f.Remove (notification);
            notification.Destroy ();
            notification.Dispose ();
            notification = new MyNotificationBar ();
            f.Put (notification, 0, 0);
            notification.Show ();

            QueueDraw ();
        }
    }
}

