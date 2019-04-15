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
using GoodtimeDevelopment.Utilites;
using AquaPic.PubSub;

namespace AquaPic.Drivers
{
    public class GenericOutputChannel : GenericChannel
    {
        private OutputChannelValueSubscriber _subscriber;
        public string subscriptionKey {
            get {
                return _subscriber.subscriptionKey;
            }
        }

        public GenericOutputChannel (string name, Type valueType) : base (name, valueType) {
            _subscriber = new OutputChannelValueSubscriber (OnValueChanged);
        }

        protected virtual void OnValueChanged (string name, ValueType value) {
            SetValue (value);
        }

        public void Subscribe (string key) {
            if (_subscriber.subscriptionKey.IsNotEmpty ()) {
                throw new Exception (string.Format("Output channel {0} is already subscribed to {1}", name, _subscriber.subscriptionKey));
            }
            _subscriber.Subscribe (key);
        }
    }
}
