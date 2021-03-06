﻿#region License

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

namespace AquaPic.Service
{
    public class ValuePublisher
    {
        public Guid key { get; protected set; }

        public ValuePublisher () {
            key = Guid.NewGuid ();
        }

        public void NotifyValueChanged (string name, ValueType newValue, ValueType oldValue) {
            MessageHub.Instance.Publish (key, new ValueChangedEvent (name, newValue, oldValue));
        }

        public void NotifyValueUpdated (string name, ValueType newValue) {
            MessageHub.Instance.Publish (key, new ValueUpdatedEvent (name, newValue));
        }
    }

    public class ValueChangedEvent
    {
        public string name;
        public ValueType newValue;
        public ValueType oldValue;

        public ValueChangedEvent (string name, ValueType newValue, ValueType oldValue) {
            this.name = name;
            this.newValue = newValue;
            this.oldValue = oldValue;
        }
    }

    public class ValueUpdatedEvent
    {
        public string name;
        public ValueType value;

        public ValueUpdatedEvent (string name, ValueType value) {
            this.name = name;
            this.value = value;
        }
    }
}
