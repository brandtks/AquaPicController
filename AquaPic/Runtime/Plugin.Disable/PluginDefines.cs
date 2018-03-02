#region License

/*
    AquaPic Main Control - Handles all functionality for the AquaPic aquarium controller.

    Copyright (c) 2017 Goodtime Development

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see http://www.gnu.org/licenses/
*/

#endregion // License

using System;

namespace AquaPic.Runtime
{
    [Flags]
    public enum ScriptFlags {
        None = 0,
        Compiled = 1,
        Cyclic = 2,
        Event = 4,
        Initializer = 8
    }

    public class AquaPicScript : Attribute
    {
        public string scriptType;

        public AquaPicScript (string scriptType) {
            this.scriptType = scriptType;
        }
    }

    public interface ICyclicScript : IStartupScript
    { 
        void CyclicRun ();
    }

    public interface IStartupScript : IScript
    {
        void Initialize ();
    }

    public interface IEventScript : IScript
    {
        void OneShotRun (ref object rtnValue);
    }

    public interface IModuleScript : ICyclicScript, IEventScript
    { }

    public interface IScript
    { }
}