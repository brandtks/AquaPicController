using System;
using System.Collections.Generic;
using AquaPic.Utilites;
using AquaPic.SerialBus;

namespace AquaPic.Drivers
{
    public class GenericCard<T>
    {
        public string name;
        public CardType cardType;
        public int cardId;
        public AquaPicBus.Slave slave;
        public GenericChannel<T>[] channels;

        public bool AquaPicBusCommunicationOk {
            get {
                return ((slave.Status == AquaPicBusStatus.CommunicationStart) || (slave.Status == AquaPicBusStatus.CommunicationSuccess));
            }
        }

        public int channelCount {
            get {
                return channels.Length;
            }
        }

        protected GenericCard (string name, 
            CardType cardType, 
            int cardId, 
            int address, 
            int numChannels)
        {
            this.name = name;
            this.cardType = cardType;
            this.cardId = cardId;
            slave = new AquaPicBus.Slave (address, string.Format ("{0} ({1})", this.name, Utils.GetDescription (cardType)));
            channels = new GenericChannel<T>[numChannels];
            for (int i = 0; i < channelCount; ++i) {
                channels [i] = ChannelCreater (i);
            }
        }

        protected virtual GenericChannel<T> ChannelCreater (int index) {
            throw new NotImplementedException ();
        }

        public virtual void AddChannel (int channel, string channelName) {
            CheckChannelRange (channel);

            if (!string.Equals (channels [channel].name, GetDefualtName (channel), StringComparison.InvariantCultureIgnoreCase)) {
                throw new Exception (string.Format ("Channel already taken by {0}", channels [channel].name));
            }
                
            try {
                // If the name exists, GetChannelIndex will return, if it doesn't it will throw a ArgumentException
                GetChannelIndex (channelName);

                throw new ArgumentException ("Channel name already exists");
            } catch (ArgumentException) {
                channels [channel].name = channelName;
            }
        }

        public virtual void RemoveChannel (int channel) {
            CheckChannelRange (channel);
            channels [channel] = ChannelCreater (channel);
        }

        public virtual int GetChannelIndex (string channelName) {
            for (int i = 0; i < channelCount; ++i) {
                if (string.Equals (channels [i].name, channelName, StringComparison.InvariantCultureIgnoreCase))
                    return i;
            }

            throw new ArgumentException (channelName + " does not exists");
        }

        public virtual IEnumerable<string> GetAllAvaiableChannels () {
            List<string> availableChannels = new List<string> ();
            for (int i = 0; i < channelCount; ++i) {
                string defaultName = GetDefualtName (i);
                if (channels [i].name == defaultName) {
                    availableChannels.Add (defaultName);
                }
            }
            return availableChannels;
        }

        protected virtual bool CheckChannelRange (int channel, bool throwException = true) {
            if ((channel < 0) || (channel >= channelCount)) {
                if (throwException) {
                    throw new ArgumentOutOfRangeException ("channel");
                } else {
                    return false;
                }
            }
            return true;
        }

        /**************************************************************************************************************/
        /* Channel Value Setters                                                                                      */
        /**************************************************************************************************************/
        public virtual void SetValueCommunication<CommunicationType> (int channel, CommunicationType value) {
            throw new NotImplementedException ();
        }

        public virtual void SetAllValuesCommunication<CommunicationType> (CommunicationType[] values) {
            throw new NotImplementedException ();
        }

        public virtual void SetChannelValue (int channel, T value) {
            CheckChannelRange (channel);
            channels [channel].SetValue (value);
        }

        public virtual void SetAllChannelValues (T[] values) {
            if (values.Length < channelCount)
                throw new ArgumentOutOfRangeException ("values length");

            for (int i = 0; i < channelCount; ++i) {
                channels [i].SetValue (values [i]);
            }
        }

        /**************************************************************************************************************/
        /* Channel Value Getters                                                                                      */
        /**************************************************************************************************************/
        public virtual void GetValueCommunication (int channel) {
            throw new NotImplementedException ();
        }

        public virtual void GetAllValuesCommunication () {
            throw new NotImplementedException ();
        }

        public virtual T GetChannelValue (int channel) {
            CheckChannelRange (channel);
            T value = channels [channel].value;
            return value;
        }

        public virtual T[] GetAllChannelValues () {
            T[] values = new T[channels.Length];
            for (int i = 0; i < channels.Length; ++i) {
                values [i] = channels [i].value;
            }
            return values;
        }

        /**************************************************************************************************************/
        /* Channel Name                                                                                               */
        /**************************************************************************************************************/
        public virtual string GetChannelName (int channel) {
            CheckChannelRange (channel);
            return channels [channel].name;
        }

        public virtual string GetDefualtName (int channel, string suffice = null) {
            CheckChannelRange (channel);
            if (suffice == null) {
                switch (cardType) {
                case CardType.AnalogInputCard:
                case CardType.DigitalInputCard:
                    suffice = ".i";
                    break;
                case CardType.AnalogOutputCard:
                    suffice = ".q";
                    break;
                case CardType.PowerStrip:
                    suffice = ".p";
                    break;
                }
            }
            return string.Format ("{0}{1}{2}", name, suffice, channel);
        }

        public virtual string[] GetAllChannelNames () {
            string[] names = new string[channelCount];
            for (int i = 0; i < channelCount; ++i) {
                names [i] = channels [i].name;
            }
            return names;
        }

        public virtual void SetChannelName (int channel, string name) {
            CheckChannelRange (channel);
            channels [channel].name = name;
        }

        /**************************************************************************************************************/
        /* Channel Mode                                                                                               */
        /**************************************************************************************************************/
        public virtual Mode GetChannelMode (int channel) {
            CheckChannelRange (channel);
            return channels [channel].mode;
        }

        public virtual Mode[] GetAllChannelModes () {
            Mode[] modes = new Mode[channelCount];
            for (int i = 0; i < channelCount; ++i) {
                modes [i] = channels [i].mode;
            }
            return modes;
        }

        public virtual void SetChannelMode (int channel, Mode mode) {
            CheckChannelRange (channel);
            channels [channel].mode = mode;
        }
    }
}

