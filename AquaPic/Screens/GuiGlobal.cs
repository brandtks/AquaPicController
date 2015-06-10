using System;
using System.Collections.Generic;
using System.Collections;

namespace AquaPic
{
    public delegate void ChangeScreenHandler (ScreenData screen, params object[] options);

    public static class GuiGlobal
    {
        public static Dictionary<string, ScreenData> allWindows = new Dictionary<string, ScreenData> () {
            { "Main", new ScreenData ("Main", (options) => {return new MainWindow (options);}) },
            { "Power", new ScreenData ("Power", (options) => {return new PowerWindow (options);}) },
            { "Lighting", new ScreenData ("Lighting", (options) => {return new LightingWindow (options);}) },
            { "Wave", new ScreenData ("Wave", (options) => {return new WaveWindow (options);}) },
            { "Condition", new ScreenData ("Condition", (options) => {return new ConditionWindow (options);}) },
            { "Plugins", new ScreenData ("Plugins", (options) => {return new PluginWindow (options);}) },
            { "Settings", new ScreenData ("Settings", (options) => {return new SettingsWindow (options);}) },
            { "Menu", new ScreenData ("Menu", (options) => {return new MenuWindow (options);}) },
            { "Edit Plugin", new ScreenData ("Edit Plugin", (options) => {return new EditPluginWindow (options);}) },
        };
        public static string currentScreen;

        public static List<string> menuWindows = new List<string> () {"Menu", "Main", "Power", "Lighting", "Plugins", "Settings"};
        public static string[] menuColors = new string[6] {"pri", "secb", "seca", "secc", "compl", "grey4"};
        public static string currentSelectedMenu;

        public static ChangeScreenHandler ChangeScreenEvent;

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
    }
}

