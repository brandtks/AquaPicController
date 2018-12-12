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
using AquaPic.Runtime;

namespace AquaPic.Modules
{
    public class WaterLevelGroupSettings : IEntitySettings
    {
        [EntitySetting (typeof (StringMutator), "name")]
        public string name { get; set; }

        [EntitySetting (typeof (FloatMutator), "highAnalogAlarmSetpoint")]
        public float highAnalogAlarmSetpoint { get; set; }

        [EntitySetting (typeof (BoolMutatorDefaultTrue), "enableHighAnalogAlarm")]
        public bool enableHighAnalogAlarm { get; set; }

        [EntitySetting (typeof (FloatMutator), "lowAnalogAlarmSetpoint")]
        public float lowAnalogAlarmSetpoint { get; set; }

        [EntitySetting (typeof (BoolMutatorDefaultTrue), "enableLowAnalogAlarm")]
        public bool enableLowAnalogAlarm { get; set; }
    }
}
