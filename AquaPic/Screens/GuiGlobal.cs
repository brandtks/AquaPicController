using System;
using System.Collections.Generic;

namespace AquaPic
{
    public static class GuiGlobal
    {
        public static Dictionary<int, ScreenData> screenData = new Dictionary<int, ScreenData> () {
            { 0, new ScreenData ("Main", "pri") },
            { 1, new ScreenData ("Power", "secb") },
            { 2, new ScreenData ("Lighting", "seca") },
            { 3, new ScreenData ("Wave", "secc") },
            { 4, new ScreenData ("Condition", "compl") },
            { 5, new ScreenData ("Settings", "grey4") }
        };

        static GuiGlobal () {
        }
    }
}

