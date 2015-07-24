using System;
using System.Collections.Generic;
using System.Collections;

namespace AquaPic.UserInterface
{
    public delegate void ChangeScreenHandler (ScreenData screen, params object[] options);

    public static class GuiGlobal
    {
        public static Dictionary<string, ScreenData> allWindows = new Dictionary<string, ScreenData> () {
            { "Main", new ScreenData ("Main", true, (options) => {return new MainWindow (options);}) },
            { "Power", new ScreenData ("Power", true, (options) => {return new PowerWindow (options);}) },
            { "Lighting", new ScreenData ("Lighting", true, (options) => {return new LightingWindow (options);}) },
            { "Temperature", new ScreenData ("Temperature", true, (options) => {return new TemperatureWindow (options);}) },
            //{ "Plugins", new ScreenData ("Plugins", true, (options) => {return new PluginWindow (options);}) },
            { "Settings", new ScreenData ("Settings", true, (options) => {return new SettingsWindow (options);}) },
            { "Menu", new ScreenData ("Menu", false, (options) => {return new MenuWindow (options);}) },
            //{ "Edit Plugin", new ScreenData ("Edit Plugin", false, (options) => {return new EditPluginWindow (options);}) },
            { "Alarms", new ScreenData ("Alarms", true, (options) => {return new AlarmWindow (options);}) },
            { "Logger", new ScreenData ("Logger", true, (options) => {return new LoggerWindow (options);}) },
            { "Analog Output", new ScreenData ("Analog Output", true, (options) => {return new AnalogOutputWindow (options);}) },
            { "Analog Input", new ScreenData ("Analog Input", true, (options) => {return new AnalogInputWindow (options);}) },
            { "Digital Input", new ScreenData ("Digital Input", true, (options) => {return new DigitalInputWindow (options);}) },
            { "Water Level", new ScreenData ("Water Level", true, (options) => {return new WaterLevelWindow (options);}) }
        };

        public static string currentScreen;

        public static List<string> menuWindows = new List<string> () {"Main", "Power", "Water Level", "Lighting", "Temperature", "Menu"};
        public static string[] menuColors = new string[6] {"pri", "secb", "seca", "secc", "compl", "grey4"};
        public static string currentSelectedMenu;

        public static event ChangeScreenHandler ChangeScreenEvent;

        static GuiGlobal () {
        }

        public static void ChangeScreens (string name, params object[] options) {
            if (!allWindows.ContainsKey (name))
                throw new Exception ("Screen does not exist");

            if (currentScreen != name) {
                currentScreen = name;

                if (menuWindows.Contains (name))
                    currentSelectedMenu = name;

                if (ChangeScreenEvent != null)
                    ChangeScreenEvent (allWindows [name], options);
            }
        }

        public static void SwitchMenuScreen (string name, int position) {
            if ((position < 0) || (position >= menuWindows.Count))
                throw new Exception ("Screen posistion out of bounds");

            if (!allWindows.ContainsKey (name))
                throw new Exception ("Screen does not exist");

            menuWindows [position] = name;
        }
    }
}

