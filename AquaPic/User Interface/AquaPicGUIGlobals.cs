using System;
using System.Collections.Generic;
using System.Collections;

namespace AquaPic.UserInterface
{
    public delegate void ChangeScreenHandler (ScreenData screen, params object[] options);

    public partial class AquaPicGUI
    {
        public static Dictionary<string, ScreenData> allWindows = new Dictionary<string, ScreenData> () {
            { "Power", new ScreenData ("Power", true, (options) => {return new PowerWindow (options);}) },
            { "Lighting", new ScreenData ("Lighting", true, (options) => {return new LightingWindow (options);}) },
            { "Temperature", new ScreenData ("Temperature", true, (options) => {return new TemperatureWindow (options);}) },
            { "Water Level", new ScreenData ("Water Level", true, (options) => {return new WaterLevelWindow (options);}) },
            { "Chemistry", new ScreenData ("Chemistry", true, (options) => {return new ChemistryWindow (options);}) },
            { "Analog Output", new ScreenData ("Analog Output", true, (options) => {return new AnalogOutputWindow (options);}) },
            { "Analog Input", new ScreenData ("Analog Input", true, (options) => {return new AnalogInputWindow (options);}) },
            { "Digital Input", new ScreenData ("Digital Input", true, (options) => {return new DigitalInputWindow (options);}) },
            { "Serial Bus", new ScreenData ("Serial Bus", true, (options) => {return new SerialBusWindow (options);}) },
            { "Alarms", new ScreenData ("Alarms", true, (options) => {return new AlarmWindow (options);}) },
            { "Logger", new ScreenData ("Logger", true, (options) => {return new LoggerWindow (options);}) },
            { "Settings", new ScreenData ("Settings", true, (options) => {return new SettingsWindow (options);}) },
            { "Menu", new ScreenData ("Menu", false, (options) => {return new MenuWindow (options);}) },
            { "Home", new ScreenData ("Home", true, (options) => {return new MainWindow (options);}) }//,
            //{ "Plugins", new ScreenData ("Plugins", true, (options) => {return new PluginWindow (options);}) },
            //{ "Edit Plugin", new ScreenData ("Edit Plugin", false, (options) => {return new EditPluginWindow (options);}) }
        };

        public static string currentScreen;

        public static List<string> menuWindows = new List<string> () {"Home", "Power", "Water Level", "Lighting", "Temperature", "Menu"};
        public static string[] menuColors = new string[6] {"pri", "secb", "seca", "secc", "compl", "grey4"};
        public static string currentSelectedMenu;

        public static event ChangeScreenHandler ChangeScreenEvent;

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

