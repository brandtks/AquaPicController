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
using AquaPic.Sensors;

namespace AquaPic.Consumers
{
    public class SensorConsumer : ValueConsumer
    {
        public virtual void OnSensorUpdatedEvent (object sender, SensorUpdatedEventArgs args) => throw new NotImplementedException ();
        public virtual void OnSensorRemovedEvent (object sender, SensorRemovedEventArgs args) => throw new NotImplementedException ();
    }

    public class SensorUpdatedEventArgs : EventArgs
    {
        public string name;
        public GenericSensorSettings settings;

        public SensorUpdatedEventArgs (string name, GenericSensorSettings settings) {
            this.name = name;
            this.settings = settings;
        }
    }

    public class SensorRemovedEventArgs : EventArgs
    {
        public string name;

        public SensorRemovedEventArgs (string name) {
            this.name = name;
        }
    }
}
