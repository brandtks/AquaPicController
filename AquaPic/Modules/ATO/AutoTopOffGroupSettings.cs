#region License

/*
    AquaPic Main Control - Handles all functionality for the AquaPic aquarium controller.

    Copyright (c) 2018 Goodtime Development

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
using AquaPic.Globals;

namespace AquaPic.Modules
{
    public class AutoTopOffGroupSettings : IEntitySettings
    {
        [EntitySetting (typeof (StringMutator), "name")]
        public string name { get; set; }

        [EntitySetting (typeof (BoolMutator), "enable")]
        public bool enable { get; set; }

        [EntitySetting (typeof (StringMutator), "requestBitName")]
        public string requestBitName { get; set; }

        [EntitySetting (typeof (StringMutator), "waterLevelGroupName")]
        public string waterLevelGroupName { get; set; }

        [EntitySetting (typeof (UIntMutator), "maximumRuntime")]
        public uint maximumRuntime { get; set; }

        [EntitySetting (typeof (UIntMutator), "minimumCooldown")]
        public uint minimumCooldown { get; set; }

        [EntitySetting (typeof (BoolMutator), "useAnalogSensors")]
        public bool useAnalogSensors { get; set; }

        [EntitySetting (typeof (FloatMutator), "analogOnSetpoint")]
        public float analogOnSetpoint { get; set; }

        [EntitySetting (typeof (FloatMutator), "analogOffSetpoint")]
        public float analogOffSetpoint { get; set; }

        [EntitySetting (typeof (BoolMutator), "useFloatSwitches")]
        public bool useFloatSwitches { get; set; }
    }
}
