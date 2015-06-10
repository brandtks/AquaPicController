using System;
using MyWidgetLibrary;

namespace AquaPic
{
    public delegate MyBackgroundWidget CreateInstanceHandler (params object[] options);

    public class ScreenData
    {
        public string name;
        CreateInstanceHandler CreateInstanceEvent;
        
        public ScreenData (string name, CreateInstanceHandler CreateInstanceEvent) {
            this.name = name;
            this.CreateInstanceEvent = CreateInstanceEvent;
        }

        public MyBackgroundWidget CreateInstance (params object[] options) {
            if (CreateInstanceEvent != null)
                return CreateInstanceEvent (options);
            else
                throw new Exception ("No screen constructor");
        }
    }
}

