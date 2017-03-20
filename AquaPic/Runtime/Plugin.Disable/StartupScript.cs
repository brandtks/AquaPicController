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

﻿/***************************************************************************************************/
/*        NOT COMPILED                                                                             */
/***************************************************************************************************/

using System;
using System.IO;
using System.Reflection;

namespace AquaPic.Runtime
{
    public class StartupScript : BaseScript
    {
        public StartupScript (string name, string path) : base (name, path) {
            flags |= ScriptFlags.Initializer;
        }

        protected override void CreateInstance (Assembly pluginAssembly) {
            if (flags.HasFlag (ScriptFlags.Compiled)) {
                foreach (var t in pluginAssembly.GetTypes ()) {
                    if (t.GetInterface ("IStartupScript") != null) {
                        try {
                            instance = Activator.CreateInstance (t) as IStartupScript;

                            if (instance == null)
                                flags &= ~ScriptFlags.Compiled;
                        } catch (Exception ex) {
                            flags &= ~ScriptFlags.Compiled;
                            errors.Add (new ScriptMessage ("StartupScript Constructor", "  " + ex.ToString ()));
                        }
                    }
                }
            }
        }
    }
}

