#region License

/*
    AquaPic Main Control - Handles all functionality for the AquaPic aquarium controller.

    Copyright (c) 2017 Skyler Brandt

    This program is free software; you can redistribute it and/or
    modify it under the terms of the GNU General Public License
    as published by the Free Software Foundation; either version 2
    of the License, or (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program; if not, write to the Free Software
    Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.

    Optionally you can also view the license at <http://www.gnu.org/licenses/>.
*/

#endregion // License

﻿using System;
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

