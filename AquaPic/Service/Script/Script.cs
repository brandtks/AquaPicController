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
using System.Text;
using CSScriptLibrary;

namespace AquaPic.Service
{
    public interface IOutletScript
    {
        bool GetOutletState ();
    }

    public class Script
    {
        public static IOutletScript CompileOutletStateGetter (string script) {
            var sb = new StringBuilder ();
            sb.AppendLine ("using GoodtimeDevelopment.Utilites;");
            sb.AppendLine ("using AquaPic.Runtime;");
            sb.AppendLine ("using AquaPic.Modules;");
            sb.AppendLine ("using AquaPic.Globals;");
            sb.AppendLine ("using AquaPic.Drivers;");
            sb.AppendLine ("using AquaPic.Gadgets;");
            sb.AppendLine ("using AquaPic.Gadgets.Sensor;");
            sb.AppendLine ("public class MyOutletScipt : IOutletScript {");
            sb.AppendLine ("public bool GetOutletState () {");
            sb.AppendLine (script);
            sb.AppendLine ("}");
            sb.AppendLine ("}");
            var code = sb.ToString ();
            var outletScript = CSScript.Evaluator.LoadCode<IOutletScript> (code);
            return outletScript;
        }
    }
}

