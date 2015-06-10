using System;
using System.Collections.Generic;
using System.Collections;

namespace AquaPic
{
    public delegate void ChangeScreenHandler (ScreenData screen);

    public static class GuiGlobal
    {
        public static Dictionary<string, ScreenData> allWindows = new Dictionary<string, ScreenData> () {
            { "Main", new ScreenData ("Main", "pri", () => {return new MainWindow ();}) },
            { "Power", new ScreenData ("Power", "secb", () => {return new PowerWindow ();}) },
            { "Lighting", new ScreenData ("Lighting", "seca", () => {return new LightingWindow ();}) },
            { "Wave", new ScreenData ("Wave", "secc", () => {return new WaveWindow ();}) },
            { "Condition", new ScreenData ("Condition", "pri", () => {return new ConditionWindow ();}) },
            { "Plugin", new ScreenData ("Plugin", "secc", () => {return new PluginWindow ();}) },
            { "Settings", new ScreenData ("Settings", "grey4", () => {return new SettingsWindow ();}) },
            { "Menu", new ScreenData ("Menu", "compl", () => {return new MenuWindow ();}) }
        };
        public static string currentScreen;

        public static List<string> menuWindows = new List<string> () {"Main", "Power", "Lighting", "Plugin", "Settings", "Menu"};
        public static string[] menuColors = new string[6] {"pri", "secb", "seca", "secc", "compl", "grey4"};
        public static string currentSelectedMenu;

        public static ChangeScreenHandler ChangeScreenEvent;

        static GuiGlobal () {
        }

        public static void ChangeScreens (string name) {
            if (!allWindows.ContainsKey (name))
                throw new Exception ("Screen does not exist");

            if (currentScreen != name) {
                currentScreen = name;

                if (menuWindows.Contains (name))
                    currentSelectedMenu = name;

                if (ChangeScreenEvent != null)
                    ChangeScreenEvent (allWindows [name]);
            }
        }
    }
}

