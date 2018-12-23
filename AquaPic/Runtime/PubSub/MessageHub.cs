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
using AquaPic.Runtime;

namespace AquaPic.PubSub
{
    public class MessageHub
    {
        Dictionary<string, List<Subscription>> subscriptions;

        public static MessageHub Instance { get; } = new MessageHub ();

        protected MessageHub () {
            subscriptions = new Dictionary<string, List<Subscription>> ();
        }

        public void Publish<TEvent> (TEvent message) {
            Publish ("global", message);
        }

        public void Publish<TEvent> (string key, TEvent message) {
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
            return Subscribe ("global", action);
        }

        public Guid Subscribe<T> (string key, Action<T> action) {
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
                subscriptions.Remove (subscription.Key);
            }
        }

        public bool IsSubscribed (Guid token) {
            return subscriptions.Values.Any (subs => subs.Any (s => s.token == token));
        }
    }
}
