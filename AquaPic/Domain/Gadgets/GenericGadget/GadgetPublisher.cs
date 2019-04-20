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
using AquaPic.Service;

namespace AquaPic.Gadgets
{
    public class GadgetPublisher : ChannelSubscriber
    {
        public Guid key { get; protected set; }

        public GadgetPublisher () {
            key = Guid.NewGuid ();
        }

        public void NotifyValueChanged (string name, ValueType newValue, ValueType oldValue) {
            MessageHub.Instance.Publish (key, new ValueChangedEvent (name, newValue, oldValue));
        }

        public void NotifyValueUpdated (string name, ValueType value) {
            MessageHub.Instance.Publish (key, new ValueUpdatedEvent (name, value));
        }

        public void NotifyGadgetUpdated (string name, GenericGadgetSettings settings) {
            MessageHub.Instance.Publish (key, new GadgetUpdatedEvent (name, settings));
        }

        public void NotifyGadgetRemoved (string name) {
            MessageHub.Instance.Publish (key, new GadgetRemovedEvent (name));
        }
    }

    public class GadgetUpdatedEvent
    {
        public string name;
        public GenericGadgetSettings settings;

        public GadgetUpdatedEvent (string name, GenericGadgetSettings settings) {
            this.name = name;
            this.settings = settings;
        }
    }

    public class GadgetRemovedEvent
    {
        public string name;

        public GadgetRemovedEvent (string name) {
            this.name = name;
        }
    }
}
