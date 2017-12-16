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
using AquaPic.Globals;
using AquaPic.Runtime;

namespace AquaPic.Drivers
{
    public class GenericBase<T>
    {
        protected List<GenericCard<T>> cards;
        private string name;

        public int cardCount {
            get {
                return cards.Count;
            }
        }

        public GenericBase (string name, uint runtime = 1000) {
            cards = new List<GenericCard<T>> ();
            this.name = name;
            TaskManager.AddCyclicInterrupt (name, runtime, Run);
        }

        protected virtual void Run () {
            Logger.AddWarning (name + " does not have an implemented Runtime function");
            TaskManager.RemoveCyclicInterrupt (name);
        }

        protected virtual GenericCard<T> CardCreater (string cardName, int cardId, int address) {
            throw new NotImplementedException ();
        }

        /**************************************************************************************************************/
        /* Cards                                                                                                      */
        /**************************************************************************************************************/
        public virtual int AddCard (int address, string cardName) {
            int cardId = cards.Count;
            cards.Add (CardCreater (cardName, cardId, address));
            return cardId;
        }

        public virtual int GetCardIndex (string cardName) {
            for (int i = 0; i < cardCount; ++i) {
                if (string.Equals (cards [i].name, cardName, StringComparison.InvariantCultureIgnoreCase))
                    return i;
            }

            throw new ArgumentException (cardName + " does not exists");
        }

        public virtual string GetCardName (int card) {
            CheckCardRange (card);
            return cards [card].name;
        }

        public virtual string[] GetAllCardNames () {
            string[] names = new string[cards.Count];
            for (int i = 0; i < cards.Count; ++i) {
                names [i] = cards [i].name;
            }
            return names;
        }

        //Base class doesn't check channel range because card handles that
        protected virtual void CheckCardRange (int card) {
            if ((card < 0) && (card >= cards.Count)) {
                throw new ArgumentOutOfRangeException ("channel");
            }
        }

        public virtual bool CheckCardRangeNoThrow (int card) {
            try {
                CheckCardRange (card);
                return true;
            } catch (ArgumentOutOfRangeException) {
                return false;
            } 
        }

        public bool AquaPicBusCommunicationOk (string name) {
            int card = GetCardIndex (name);
            return AquaPicBusCommunicationOk (card);
        }

        public bool AquaPicBusCommunicationOk (IndividualControl ic) {
            return AquaPicBusCommunicationOk (ic.Group);
        }

        public bool AquaPicBusCommunicationOk (int card) {
            CheckCardRange (card);
            return cards [card].AquaPicBusCommunicationOk;
        }

        /**************************************************************************************************************/
        /* Channels                                                                                                   */
        /**************************************************************************************************************/
        public virtual void AddChannel (IndividualControl channel, string channelName) {
            AddChannel (channel.Group, channel.Individual, channelName);
        }

        public virtual void AddChannel (int card, int channel, string channelName) {
            CheckCardRange (card);
            cards [card].AddChannel (channel, channelName);
        }

        public virtual void RemoveChannel (string channelName) {
            IndividualControl channel = GetChannelIndividualControl (channelName);
            RemoveChannel (channel.Group, channel.Individual);
        }

        public virtual void RemoveChannel (IndividualControl channel) {
            RemoveChannel (channel.Group, channel.Individual);
        }

        public virtual void RemoveChannel (int card, int channel) {
            CheckCardRange (card);
            cards [card].RemoveChannel (channel);
        }

        public virtual IndividualControl GetChannelIndividualControl (string channelName) {
            var channel = IndividualControl.Empty;

            for (int i = 0; i < cardCount; ++i) {
                for (int j = 0; j < cards [i].channels.Length; ++j) {
                    if (string.Equals (cards [i].channels [j].name, channelName, StringComparison.InvariantCultureIgnoreCase)) {
                        channel.Group = i;
                        channel.Individual = j;
                        return channel;
                    }
                }
            }

            throw new ArgumentException (channelName + " does not exists");
        }

        public virtual int GetChannelIndex (string cardName, string channelName) {
            int card = GetCardIndex (cardName);
            return GetChannelIndex (card, channelName);
        }

        public virtual int GetChannelIndex (int card, string channelName) {
            CheckCardRange (card);
            return cards [card].GetChannelIndex (channelName);
        }

        public virtual string[] GetAllAvaiableChannels () {
            List<string> availableChannels = new List<string> ();
            foreach (var card in cards) {
                availableChannels.AddRange (card.GetAllAvaiableChannels ());
            }
            return availableChannels.ToArray ();
        }

        /**************************************************************************************************************/
        /* Channel Value Getters                                                                                      */
        /**************************************************************************************************************/
        public virtual T GetChannelValue (string channelName) {
            IndividualControl channel = GetChannelIndividualControl (channelName);
            return GetChannelValue (channel.Group, channel.Individual);
        }

        public virtual T GetChannelValue (IndividualControl channel) {
            return GetChannelValue (channel.Group, channel.Individual);
        }

        public virtual T GetChannelValue (int card, int channel) {
            CheckCardRange (card);
            return cards [card].GetChannelValue (channel);
        }

        public virtual T[] GetAllChannelValues (string cardName) {
            int card = GetCardIndex (cardName);
            return GetAllChannelValues (card);
        }

        public virtual T[] GetAllChannelValues (int card) {
            CheckCardRange (card);
            return cards [card].GetAllChannelValues ();
        }

        /**************************************************************************************************************/
        /* Channel Value Setters                                                                                      */
        /**************************************************************************************************************/
        public virtual void SetChannelValue (string channelName, T value) {
            IndividualControl channel = GetChannelIndividualControl (channelName);
            SetChannelValue (channel.Group, channel.Individual, value);
        }

        public virtual void SetChannelValue (IndividualControl channel, T value) {
            SetChannelValue (channel.Group, channel.Individual, value);
        }

        public virtual void SetChannelValue (int card, int channel, T value) {
            CheckCardRange (card);
            cards [card].SetChannelValue (channel, value);
        }

        public virtual void SetAllChannelValues (string cardName, T[] values) {
            int card = GetCardIndex (cardName);
            SetAllChannelValues (card, values);
        }

        public virtual void SetAllChannelValues (int card, T[] values) {
            CheckCardRange (card);
            cards [card].SetAllChannelValues (values);
        }

        /**************************************************************************************************************/
        /* Channel Name Getters                                                                                       */
        /**************************************************************************************************************/
        public virtual string GetChannelName (IndividualControl channel) {
            return GetChannelName (channel.Group, channel.Individual);
        }

        public virtual string GetChannelName (int card, int channel) {
            CheckCardRange (card);
            return cards [card].GetChannelName (channel);
        }

        public virtual string[] GetAllChannelNames (int card) {
            CheckCardRange (card);
            return cards [card].GetAllChannelNames ();
        }

        /**************************************************************************************************************/
        /* Channel Name Setters                                                                                       */
        /**************************************************************************************************************/
        public virtual void SetChannelName (IndividualControl channel, string name) {
            SetChannelName (channel.Group, channel.Individual, name);
        }

        public virtual void SetChannelName (int card, int channel, string name) {
            CheckCardRange (card);
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

        public virtual Mode GetChannelMode (int card, int channel) {
            CheckCardRange (card);
            return cards [card].GetChannelMode (channel);
        }

        public virtual Mode[] GetAllChannelModes (string cardName) {
            int card = GetCardIndex (cardName);
            return GetAllChannelModes (card);
        }

        public virtual Mode[] GetAllChannelModes (int card) {
            CheckCardRange (card);
            return cards [card].GetAllChannelModes ();
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

        public virtual void SetChannelMode (int card, int channel, Mode mode) {
            CheckCardRange (card);
            cards [card].SetChannelMode (channel, mode);
        }
    }
}

lMode (int card, int channel, Mode mode) {
            CheckCardRange (card);
            cards [card].SetChannelMode (channel, mode);
        }
    }
}

