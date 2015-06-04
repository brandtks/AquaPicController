using System;
using System.Collections.Generic;

namespace AquaPic
{
    public delegate void ChangeScreenHandler (ScreenData screen);

    public static class GuiGlobal
    {
        public static Dictionary<string, ScreenData> screenData = new Dictionary<string, ScreenData> () {
            { "Main", new ScreenData ("Main", "pri", 0, () => {return new MainWindow ();}) },
            { "Power", new ScreenData ("Power", "secb", 1, () => {return new PowerWindow ();}) },
            { "Lighting", new ScreenData ("Lighting", "seca", 2, () => {return new LightingWindow ();}) },
            { "Wave", new ScreenData ("Wave", "secc", 3, () => {return new WaveWindow ();}) },
            { "Condition", new ScreenData ("Condition", "compl", 4, () => {return new ConditionWindow ();}) },
            { "Settings", new ScreenData ("Settings", "grey4", 5, () => {return new SettingsWindow ();}) }
        };

        public static string currentScreen;
        public static ChangeScreenHandler ChangeScreenEvent;

        static GuiGlobal () {
        }

        public static void ChangeScreens (string name) {
            if (!screenData.ContainsKey (name))
                throw new Exception ("Screen does not exist");

            if (ChangeScreenEvent != null)
                ChangeScreenEvent (screenData [name]);
        }
    }
}

