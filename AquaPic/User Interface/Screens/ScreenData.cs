using System;
using TouchWidgetLibrary;

namespace AquaPic.UserInterface
{
    public delegate WindowBase CreateInstanceHandler (params object[] options);

    public class ScreenData
    {
        public string name;
        public CreateInstanceHandler CreateInstanceEvent;
        public bool showInMenu; 
        
        public ScreenData (string name, bool showInMenu, CreateInstanceHandler CreateInstanceEvent) {
            this.name = name;
            this.showInMenu = showInMenu;
            this.CreateInstanceEvent = CreateInstanceEvent;
        }

        public WindowBase CreateInstance (params object[] options) {
            if (CreateInstanceEvent != null)
                return CreateInstanceEvent (options);
            else
                throw new Exception ("No screen constructor");
        }
    }
}

