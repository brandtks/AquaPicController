using System;
using TouchWidgetLibrary;

namespace AquaPic.UserInterface
{
    public delegate SceneBase CreateInstanceHandler (params object[] options);

    public class SceneData
    {
        public string name;
        public CreateInstanceHandler CreateInstanceEvent;
        public bool showInMenu; 
        
        public SceneData (string name, bool showInMenu, CreateInstanceHandler CreateInstanceEvent) {
            this.name = name;
            this.showInMenu = showInMenu;
            this.CreateInstanceEvent = CreateInstanceEvent;
        }

        public SceneBase CreateInstance (params object[] options) {
            if (CreateInstanceEvent != null)
                return CreateInstanceEvent (options);
            else
                throw new Exception ("No screen constructor");
        }
    }
}

