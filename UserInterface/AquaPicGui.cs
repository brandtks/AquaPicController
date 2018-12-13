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
using Gtk;
using GoodtimeDevelopment.TouchWidget;
using AquaPic.Runtime;

namespace AquaPic.UserInterface
{
    public delegate void ChangeSceneHandler (SceneData screen, params object[] options);

    public class AquaPicGui : Window
    {
        SceneBase current;
        Fixed f;
        MySideBar side;
        MyNotificationBar notification;
        public string currentScene { get; private set; }

        public Dictionary<string, SceneData> scenes;

        public event ChangeSceneHandler ChangeSceneEvent;

        public static AquaPicGui AquaPicUserInterface { get; private set; }

        protected AquaPicGui () : base (WindowType.Toplevel) {
            Name = "AquaPicGUI";
            Title = "AquaPic Controller Version 0";
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

            scenes = new Dictionary<string, SceneData> () {
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

            currentScene = "Home";

            f = new Fixed ();
            f.SetSizeRequest (800, 480);

            current = scenes[currentScene].CreateInstance ();
            f.Put (current, 0, 0);
            current.Show ();

            side = new MySideBar (scenes, currentScene);
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
            if (AquaPicUserInterface == null) {
                AquaPicUserInterface = new AquaPicGui ();
                return AquaPicUserInterface;
            }

            throw new Exception ("User interface is already created");
        }

        public void ChangeScreens (string name, params object[] options) {
            if (!scenes.ContainsKey (name))
                throw new Exception ("Screen does not exist");

            currentScene = name;

            ChangeSceneEvent?.Invoke (scenes[name], options);
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

        protected void ScreenChange (SceneData screen, params object[] options) {
            f.Remove (current);
            current.Destroy ();
            current = screen.CreateInstance (options);
            f.Put (current, 0, 0);

            f.Remove (side);
            side.Destroy ();
            side = new MySideBar ();
            f.Put (side, 0, 20);
            side.Show ();

            if (currentScene == "Logger") {
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
            } else if (currentScene == "Alarms") {
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
            notification = new MyNotificationBar ();
            f.Put (notification, 0, 0);
            notification.Show ();

            QueueDraw ();
        }
    }
}

