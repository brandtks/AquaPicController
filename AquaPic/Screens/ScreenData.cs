using System;
using MyWidgetLibrary;

namespace AquaPic
{
    public delegate MyBackgroundWidget CreateInstanceHandler ();

    public class ScreenData
    {
        public string name;
        public MyColor color;
        public int menuPosition;
        CreateInstanceHandler CreateInstanceEvent;
        
        public ScreenData (string name, string color, int menuPosition, CreateInstanceHandler CreateInstanceEvent) {
            this.name = name;
            this.color = new MyColor (color);
            this.menuPosition = menuPosition;
            this.CreateInstanceEvent = CreateInstanceEvent;
        }

        public MyBackgroundWidget CreateInstance () {
            if (CreateInstanceEvent != null)
                return CreateInstanceEvent ();
            else
                throw new Exception ("No screen constructor");
        }
    }
}

