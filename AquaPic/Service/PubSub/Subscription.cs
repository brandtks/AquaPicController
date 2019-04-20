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

namespace AquaPic.PubSub
{
    public class Subscription
    {
        public Guid token { get; private set; }
        public Type type { get; private set; }
        public object handler { get; private set; }

        public Subscription (Type type, Guid token, object handler) {
            this.type = type;
            this.token = token;
            this.handler = handler;
        }

        public void Handle<T> (T message) {
            var messageHandler = handler as Action<T>;
            messageHandler (message);
        }
    }
}
