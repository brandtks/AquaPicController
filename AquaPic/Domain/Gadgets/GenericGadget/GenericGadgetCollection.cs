#region License

/*
    AquaPic Main Control - Handles all functionality for the AquaPic aquarium controller.

    Copyright (c) 2019 Goodtime Development

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
using System.Linq;
using AquaPic.Globals;
using AquaPic.Service;

namespace AquaPic.Gadgets
{
    public class GenericGadgetCollection
    {
        protected Dictionary<string, GenericGadget> gadgets;
        protected readonly string gadgetSettingsFileName;
        public string gadgetSettingsArrayName { get; protected set; }

        public int gadgetCount {
            get {
                return gadgets.Count;
            }
        }

        public string defaultGadget {
            get {
                if (gadgets.Count > 0) {
                    var first = gadgets.First ();
                    return first.Key;
                }
                return string.Empty;
            }
        }

        public GenericGadgetCollection (string gadgetSettingsFileName, string gadgetSettingsArrayName) {
            gadgets = new Dictionary<string, GenericGadget> ();
            this.gadgetSettingsFileName = gadgetSettingsFileName;
            this.gadgetSettingsArrayName = gadgetSettingsArrayName;
        }

        public void CreateGadget (GenericGadgetSettings settings) {
            CreateGadget (settings, true);
        }

        protected void CreateGadget (GenericGadgetSettings settings, bool saveToFile) {
            if (GadgetNameExists (settings.name)) {
                throw new Exception (string.Format ("gadget: {0} already exists", settings.name));
            }
            var equip = GadgetCreater (settings);
            if (equip.Valid ()) {
                gadgets[equip.name] = equip;
                if (saveToFile) {
                    AddGadgetSettingsToFile (settings);
                }
            } else {
                equip.Dispose ();
            }
        }

        protected virtual GenericGadget GadgetCreater (GenericGadgetSettings settings) => throw new NotImplementedException ();

        public void UpdateGadget (string name, GenericGadgetSettings settings) {
            if (GadgetNameExists (name)) {
                settings = gadgets[name].OnUpdate (settings);
                RemoveGadget (name, false);
            }
            CreateGadget (settings, true);
            gadgets[settings.name].NotifyGadgetUpdated (name, settings);
        }

        public void RemoveGadget (string name) {
            RemoveGadget (name, true);
        }

        protected void RemoveGadget (string name, bool callRemoveEvent) {
            CheckGadgetKey (name);
            var gadget = gadgets[name];
            gadget.Dispose ();
            gadgets.Remove (name);
            DeleteGadgetSettingsFromFile (name);
            if (callRemoveEvent) {
                gadget.NotifyGadgetRemoved (name);
            }
        }

        public void CheckGadgetKey (string name) {
            if (!gadgets.ContainsKey (name)) {
                throw new ArgumentException (name + " isn't a valid gadget");
            }
        }

        public bool CheckGadgetKeyNoThrow (string name) {
            try {
                CheckGadgetKey (name);
                return true;
            } catch {
                return false;
            }
        }

        public bool GadgetNameExists (string name) {
            return CheckGadgetKeyNoThrow (name);
        }

        /***Getters****************************************************************************************************/
        public virtual string[] GetAllGadgetNames () {
            List<string> names = new List<string> ();
            foreach (var equip in gadgets.Values) {
                names.Add (equip.name);
            }
            return names.ToArray ();
        }

        public virtual GenericGadget GetGadget (string name) {
            CheckGadgetKey (name);
            return gadgets[name].Clone ();
        }

        public virtual IndividualControl GetGadgetChannel (string name) {
            CheckGadgetKey (name);
            return gadgets[name].channel;
        }

        public virtual Guid GetGadgetEventPublisherKey (string name) {
            CheckGadgetKey (name);
            return gadgets[name].key;
        }

        /***Settings***************************************************************************************************/
        public virtual GenericGadgetSettings GetGadgetSettings (string name) {
            CheckGadgetKey (name);
            var settings = new GenericGadgetSettings ();
            settings.name = name;
            settings.channel = gadgets[name].channel;
            return settings;
        }

        public virtual void ReadAllGadgetsFromFile () => throw new NotImplementedException ();

        protected void AddGadgetSettingsToFile (IEntitySettings settings) {
            SettingsHelper.AddSettingsToArray (gadgetSettingsFileName, gadgetSettingsArrayName, settings);
        }

        protected void UpdateGadgetSettingsInFile (string name) {
            SettingsHelper.UpdateSettingsInArray (gadgetSettingsFileName, gadgetSettingsArrayName, name, GetGadgetSettings (name));
        }

        protected void DeleteGadgetSettingsFromFile (string name) {
            SettingsHelper.DeleteSettingsFromArray (gadgetSettingsFileName, gadgetSettingsArrayName, name);
        }
    }
}
