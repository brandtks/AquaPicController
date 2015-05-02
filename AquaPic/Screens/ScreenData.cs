using System;
using MyWidgetLibrary;

namespace AquaPic
{
    public class ScreenData
    {
        public string name;
        public MyColor color;
        
        public ScreenData (string name, string color) {
            this.name = name;
            this.color = new MyColor (color);
        }
    }
}

