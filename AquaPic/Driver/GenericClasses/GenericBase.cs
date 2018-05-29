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
using AquaPic.Globals;
using AquaPic.Runtime;

namespace AquaPic.Drivers
{
	public class GenericBase
	{
		protected Dictionary<string, GenericCard> cards;
		string name;

		public int cardCount {
			get {
				return cards.Count;
			}
		}

		public string firstCard {
			get {
				if (cards.Count > 0) {
					var first = cards.First ();
                    return first.Key;
                }

                return string.Empty;
            }
		}

		public GenericBase (string name, uint runtime = 1000) {
			cards = new Dictionary<string, GenericCard> ();
			this.name = name;
			TaskManager.AddCyclicInterrupt (name, runtime, Run);
		}

		protected virtual void Run () {
			Logger.AddWarning (name + " does not have an implemented Runtime function");
			TaskManager.RemoveCyclicInterrupt (name);
		}

		protected virtual GenericCard CardCreater (string cardName, int address) {
			throw new NotImplementedException ();
		}

		public virtual string GetCardAcyronym () {
			throw new NotImplementedException ();
		}

		public virtual CardType GetCardType () {
			throw new NotImplementedException ();
		}

		/**************************************************************************************************************/
		/* Cards                                                                                                      */
		/**************************************************************************************************************/
		public virtual void AddCard (string card, int address) {
			cards.Add (card, CardCreater (card, address));
		}

		public virtual void RemoveCard (string card) {
			CheckCardKey (card);
			if (!CheckCardEmpty (card)) {
                throw new Exception ("At least one channel is occupied");
            }
            cards[card].RemoveSlave ();
            cards.Remove (card);
        }

		public virtual string[] GetAllCardNames () {
			var names = new List<string> ();
			foreach (var card in cards.Values) {
				names.Add (card.name);
			}
			return names.ToArray ();
		}

		//Base class doesn't check channel range because card handles that
		protected virtual void CheckCardKey (string card) {
			if (!cards.ContainsKey (card)) {
				throw new ArgumentOutOfRangeException (nameof (card));
			}
		}

		public virtual bool CheckCardKeyNoThrow (string card) {
			try {
				CheckCardKey (card);
				return true;
			} catch (ArgumentOutOfRangeException) {
				return false;
			}
		}

		public virtual bool CardNameOk (string card) {
			return !CheckCardKeyNoThrow (card);
		}

		public virtual bool CheckCardEmpty (string card) {
			CheckCardKey (card);
			if (GetAllAvaiableChannels (card).Length == cards[card].channelCount) {
				return true;
			}
			return false;
		}

		public virtual int GetLowestCardNameIndex () {
            var nameIndexes = new List<int> ();
            var lowestNameIndex = 1;
            foreach (var card in cards.Values) {
                // All names start with two letter acyronym, so everything after that is the name index
                nameIndexes.Add (Convert.ToInt32 (card.name.Substring (2)));
            }

            bool lowestFound = false;
            while (!lowestFound) {
                if (nameIndexes.Contains (lowestNameIndex)) {
                    ++lowestNameIndex;
                } else {
                    lowestFound = true;
                }
            }

            return lowestNameIndex;
        }

		public bool AquaPicBusCommunicationOk (IndividualControl ic) {
			return AquaPicBusCommunicationOk (ic.Group);
		}

		public bool AquaPicBusCommunicationOk (string card) {
			CheckCardKey (card);
			return cards[card].AquaPicBusCommunicationOk;
		}

		/**************************************************************************************************************/
		/* Channels                                                                                                   */
		/**************************************************************************************************************/
		public virtual void AddChannel (IndividualControl channel, string channelName) {
			AddChannel (channel.Group, channel.Individual, channelName);
		}

		public virtual void AddChannel (string card, int channel, string channelName) {
			CheckCardKey (card);
			if (!ChannelNameOk (card, name)) {
				throw new Exception (string.Format ("Channel name {0} already exists", name));
			}
			cards[card].AddChannel (channel, channelName);
		}

		public virtual void RemoveChannel (string channelName) {
			IndividualControl channel = GetChannelIndividualControl (channelName);
			RemoveChannel (channel);
		}

		public virtual void RemoveChannel (IndividualControl channel) {
			RemoveChannel (channel.Group, channel.Individual);
		}

		public virtual void RemoveChannel (string card, int channel) {
			CheckCardKey (card);
			cards[card].RemoveChannel (channel);
		}

		public virtual IndividualControl GetChannelIndividualControl (string channelName) {
			var channel = IndividualControl.Empty;

			foreach (var card in cards.Values) {
				for (int j = 0; j < cards[card.name].channels.Length; ++j) {
					if (string.Equals (cards[card.name].channels[j].name, channelName, StringComparison.InvariantCultureIgnoreCase)) {
						channel.Group = card.name;
						channel.Individual = j;
						return channel;
					}
				}
			}

			throw new ArgumentException (channelName + " does not exists");
		}

		public virtual int GetChannelIndex (string card, string channelName) {
			CheckCardKey (card);
			return cards[card].GetChannelIndex (channelName);
		}

		public virtual string[] GetAllAvaiableChannels () {
			List<string> availableChannels = new List<string> ();
			foreach (var card in cards.Values) {
				availableChannels.AddRange (card.GetAllAvaiableChannels ());
			}
			return availableChannels.ToArray ();
		}

		public virtual string[] GetAllAvaiableChannels (string card) {
			var availableChannels = new List<string> (cards[card].GetAllAvaiableChannels ());
			return availableChannels.ToArray ();
		}

		/**************************************************************************************************************/
		/* Channel Value Getters                                                                                      */
		/**************************************************************************************************************/
		public virtual dynamic GetChannelValue (string channelName) {
			IndividualControl channel = GetChannelIndividualControl (channelName);
			return GetChannelValue (channel.Group, channel.Individual);
		}

		public virtual dynamic GetChannelValue (IndividualControl channel) {
			return GetChannelValue (channel.Group, channel.Individual);
		}

		public virtual dynamic GetChannelValue (string card, int channel) {
			CheckCardKey (card);
			return cards[card].GetChannelValue (channel);
		}

		public virtual dynamic[] GetAllChannelValues (string card) {
			CheckCardKey (card);
			return cards[card].GetAllChannelValues ();
		}

		/**************************************************************************************************************/
		/* Channel Value Setters                                                                                      */
		/**************************************************************************************************************/
		public virtual void SetChannelValue (string channelName, ValueType value) {
			IndividualControl channel = GetChannelIndividualControl (channelName);
			SetChannelValue (channel.Group, channel.Individual, value);
		}

		public virtual void SetChannelValue (IndividualControl channel, ValueType value) {
			SetChannelValue (channel.Group, channel.Individual, value);
		}

		public virtual void SetChannelValue (string card, int channel, ValueType value) {
			CheckCardKey (card);
			cards[card].SetChannelValue (channel, value);
		}

		public virtual void SetAllChannelValues (string card, ValueType[] values) {
			CheckCardKey (card);
			cards[card].SetAllChannelValues (values);
		}

		/**************************************************************************************************************/
		/* Channel Name Check                                                                                         */
		/**************************************************************************************************************/
        public virtual bool ChannelNameOk (string card, string channelName) {
			CheckCardKey (card);
			bool nameOk;
			try {
				GetChannelIndex (card, channelName);
				nameOk = false;
			} catch (ArgumentException) {
				nameOk = true;
			}
			return nameOk;
		}

		/**************************************************************************************************************/
		/* Channel Name Getters                                                                                       */
		/**************************************************************************************************************/
		public virtual string GetChannelName (IndividualControl channel) {
			return GetChannelName (channel.Group, channel.Individual);
		}

		public virtual string GetChannelName (string card, int channel) {
			CheckCardKey (card);
			return cards[card].GetChannelName (channel);
		}

		public virtual string[] GetAllChannelNames (string card) {
			CheckCardKey (card);
			return cards[card].GetAllChannelNames ();
		}

		/**************************************************************************************************************/
		/* Channel Name Setters                                                                                       */
		/**************************************************************************************************************/
		public virtual void SetChannelName (IndividualControl channel, string name) {
			SetChannelName (channel.Group, channel.Individual, name);
		}

		public virtual void SetChannelName (string card, int channel, string name) {
			CheckCardKey (card);
			if (!ChannelNameOk (card, name)) {
				throw new Exception (string.Format ("Channel name {0} already exists", name));
			}
			cards[card].SetChannelName (channel, name);
		}

		/**************************************************************************************************************/
		/* Channel Mode Getters                                                                                       */
		/**************************************************************************************************************/
		public virtual Mode GetChannelMode (string channelName) {
			IndividualControl channel = GetChannelIndividualControl (channelName);
			return GetChannelMode (channel.Group, channel.Individual);
		}

		public virtual Mode GetChannelMode (IndividualControl channel) {
			return GetChannelMode (channel.Group, channel.Individual);
		}

		public virtual Mode GetChannelMode (string card, int channel) {
			CheckCardKey (card);
			return cards[card].GetChannelMode (channel);
		}

		public virtual Mode[] GetAllChannelModes (string card) {
			CheckCardKey (card);
			return cards[card].GetAllChannelModes ();
		}

		/**************************************************************************************************************/
		/* Channel Mode Setters                                                                                       */
		/**************************************************************************************************************/
		public virtual void SetChannelMode (string channelName, Mode mode) {
			IndividualControl channel = GetChannelIndividualControl (channelName);
			SetChannelMode (channel.Group, channel.Individual, mode);
		}

		public virtual void SetChannelMode (IndividualControl channel, Mode mode) {
			SetChannelMode (channel.Group, channel.Individual, mode);
		}

		public virtual void SetChannelMode (string card, int channel, Mode mode) {
			CheckCardKey (card);
			cards[card].SetChannelMode (channel, mode);
		}
	}
}

