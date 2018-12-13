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
using Newtonsoft.Json.Linq;
using GoodtimeDevelopment.Utilites;
using AquaPic.Globals;
using AquaPic.Runtime;

namespace AquaPic.Sensors
{
    public class FloatSwitchSettings : IEntitySettings
    {
        [EntitySetting (typeof (StringMutator), "name")]
        public string name { get; set; }

        [EntitySetting (typeof (IndividualControlMutator), new string[] { "inputCard", "channel" })]
        public IndividualControl channel { get; set; }

        [EntitySetting (typeof (FloatMutator), "physicalLevel")]
        public float physicalLevel { get; set; }

        [EntitySetting (typeof (SwitchTypeMutator), "switchType")]
        public SwitchType switchType { get; set; }

        [EntitySetting (typeof (SwitchFunctionMutator), "switchFuntion")]
        public SwitchFunction switchFuntion { get; set; }

        [EntitySetting (typeof (UIntMutator), "timeOffset")]
        public uint timeOffset { get; set; }

        [EntitySetting (typeof (StringMutator), "waterLevelGroupName")]
        public string waterLevelGroupName { get; set; }
    }
}
