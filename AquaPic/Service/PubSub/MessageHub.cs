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
using System.Collections.Generic;
using System.Linq;
using AquaPic.Service;

namespace AquaPic.PubSub
{
    public class MessageHub
    {
        Dictionary<Guid, List<Subscription>> subscriptions;
        public Guid globalGuid { get; protected set; }

        public static MessageHub Instance { get; } = new MessageHub ();

        protected MessageHub () {
            subscriptions = new Dictionary<Guid, List<Subscription>> ();
            globalGuid = Guid.NewGuid ();
        }

        public void Publish<TEvent> (TEvent message) {
            Publish (globalGuid, message);
        }

        public void Publish<TEvent> (Guid key, TEvent message) {
            if (!subscriptions.ContainsKey (key)) {
                return;
            }
            var subs = subscriptions[key];
            var eventType = typeof (TEvent);
            foreach (var subscription in subs) {
                if (!subscription.type.IsAssignableFrom (eventType)) { 
                    continue; 
                }

                try {
                    subscription.Handle (message);
                } catch (Exception e) {
                    Logger.AddError (e.ToString ());
                }
            }
        }

        public Guid Subscribe<T> (Action<T> action) {
            return Subscribe (globalGuid, action);
        }

        public Guid Subscribe<T> (Guid key, Action<T> action) {
            var guid = Guid.NewGuid ();

            if (!subscriptions.ContainsKey (key)) {
                subscriptions.Add (key, new List<Subscription> ());
            }

            subscriptions[key].Add (new Subscription (typeof (T), guid, action));

            return guid;
        }

        public void Unsubscribe (Guid token) {
            var subscription = subscriptions.FirstOrDefault (subs => subs.Value.Any (s => s.token == token));
            if (subscription.Value != null) {
                Unsubscribe (subscription.Key, token);
            }
        }

        public void Unsubscribe (Guid key, Guid token) {
            if (!subscriptions.ContainsKey (key)) {
                return;
            }
            var sub = subscriptions[key].Find (s => s.token == token);
            if (sub != null) {
                subscriptions[key].Remove (sub);
            }
        }

        public bool IsSubscribed (Guid token) {
            return subscriptions.Values.Any (subs => subs.Any (s => s.token == token));
        }
    }
}
