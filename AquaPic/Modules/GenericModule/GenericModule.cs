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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using GoodtimeDevelopment.Utilites;
using AquaPic.Runtime;
using AquaPic.Globals;

namespace AquaPic.Modules
{
    public class GenericModule
    {
        private static Dictionary<string, GenericGroup> groups;

        public static int groupCount {
            get {
                return groups.Count;
            }
        }

        public static string firstGroup {
            get {
                if (groups.Count > 0) {
                    var first = groups.First ();
                    return first.Key;
                }

                return string.Empty;
            }
        }

        public GenericModule () {
            groups = new Dictionary<string, GenericGroup> ();
        }

        public static void Run () {
            foreach (var group in groups.Values) {
                group.GroupRun ();
            }
        }

        /**************************************************************************************************************/
        /* Groups                                                                                                     */
        /**************************************************************************************************************/
        public static void AddGroup (GenericGroup group) {
            if (!GroupNameOk (group.name)) {
                throw new Exception (string.Format ("Group: {0} already exists", group.name));
            }

            groups.Add (group.name, group);
        }

        public static void RemoveGroup (string name) {
            CheckGroupKey (name);
            groups.Remove (name);
        }

        public static void CheckGroupKey (string name) {
            if (!groups.ContainsKey (name)) {
                throw new ArgumentException ("name");
            }
        }

        public static bool CheckGroupKeyNoThrow (string name) {
            try {
                CheckGroupKey (name);
                return true;
            } catch {
                return false;
            }
        }

        public static bool GroupNameOk (string name) {
            return !CheckGroupKeyNoThrow (name);
        }

        /***Getters****************************************************************************************************/
        /***Names***/
        public static string[] GetAllAtoGroupNames () {
            List<string> names = new List<string> ();
            foreach (var waterLevelGroup in groups.Values) {
                names.Add (waterLevelGroup.name);
            }
            return names.ToArray ();
        }
    }
}
