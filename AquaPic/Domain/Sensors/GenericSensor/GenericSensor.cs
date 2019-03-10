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
using AquaPic.PubSub;

namespace AquaPic.Sensors
{
    public class GenericSensor : SensorPublisher
    {
        public string name { get; protected set; }
        public IndividualControl channel { get; protected set; }

        public GenericSensor (GenericSensorSettings settings) : base (settings.name) {
            name = settings.name;
            channel = settings.channel;
        }

        public virtual void OnCreate () => throw new NotImplementedException ();
        public virtual void OnRemove () => throw new NotImplementedException ();

        public virtual GenericSensorSettings OnUpdate (GenericSensorSettings settings) {
            return settings;
        }

        public virtual ValueType GetValue () => throw new NotImplementedException ();

        public virtual GenericSensor Clone () {
            return (GenericSensor)MemberwiseClone ();
        }
    }
}
