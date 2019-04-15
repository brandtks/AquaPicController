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
using AquaPic.SerialBus;

namespace AquaPic.Drivers
{
    public class GenericCard : AquaPicBus.Slave
    {
        public string name;
        public GenericChannel[] channels;

        public bool AquaPicBusCommunicationOk {
            get {
                return ((status == AquaPicBusStatus.CommunicationStart) || (status == AquaPicBusStatus.CommunicationSuccess));
            }
        }

        public int channelCount {
            get {
                return channels.Length;
            }
        }

        protected GenericCard (string name, int address, int numChannels) : base (address, name) {
            this.name = name;

            channels = new GenericChannel[numChannels];
            for (int i = 0; i < channelCount; ++i) {
                channels[i] = ChannelCreater (i);
            }
        }

        protected virtual GenericChannel ChannelCreater (int index) => throw new NotImplementedException ();

        public virtual void AddChannel (int channel, string channelName) {
            CheckChannelRange (channel);

            if (!string.Equals (channels[channel].name, GetDefualtName (channel), StringComparison.InvariantCultureIgnoreCase)) {
                throw new Exception (string.Format ("Channel already taken by {0}", channels[channel].name));
            }

            try {
                // If the name exists, GetChannelIndex will return, if it doesn't it will throw a ArgumentException
                GetChannelIndex (channelName);

                throw new ArgumentException ("Channel name already exists");
            } catch (ArgumentException) {
                channels[channel].name = channelName;
            }
        }

        public virtual void RemoveChannel (int channel) {
            CheckChannelRange (channel);
            channels[channel] = ChannelCreater (channel);
        }

        public virtual int GetChannelIndex (string channelName) {
            for (int i = 0; i < channelCount; ++i) {
                if (string.Equals (channels[i].name, channelName, StringComparison.InvariantCultureIgnoreCase))
                    return i;
            }

            throw new ArgumentException (channelName + " does not exists");
        }

        public virtual IEnumerable<string> GetAllAvaiableChannels () {
            List<string> availableChannels = new List<string> ();
            for (int i = 0; i < channelCount; ++i) {
                string defaultName = GetDefualtName (i);
                if (channels[i].name == defaultName) {
                    availableChannels.Add (defaultName);
                }
            }
            return availableChannels;
        }

        protected virtual bool CheckChannelRange (int channel, bool throwException = true) {
            if ((channel < 0) || (channel >= channelCount)) {
                if (throwException) {
                    throw new ArgumentOutOfRangeException (nameof (channel));
                }
                return false;
            }
            return true;
        }

        public virtual string GetChannelPrefix () {
            return string.Empty;
        }

        /**************************************************************************************************************/
        /* Channel Value Getters                                                                                      */
        /**************************************************************************************************************/
        public virtual void GetValueCommunication (int channel) => throw new NotImplementedException ();

        public virtual void GetAllValuesCommunication () => throw new NotImplementedException ();

        public virtual ValueType GetChannelValue (int channel) {
            CheckChannelRange (channel);
            dynamic value;
            try {
                value = (ValueType)Convert.ChangeType (channels[channel].value, channels[channel].valueType);
            } catch {
                value = Activator.CreateInstance (channels[channel].valueType); ;
            }
            return value;
        }

        public virtual ValueType[] GetAllChannelValues () {
            var values = new ValueType[channels.Length];
            for (int i = 0; i < channels.Length; ++i) {
                values[i] = GetChannelValue (i);
            }
            return values;
        }

        /**************************************************************************************************************/
        /* Channel Value Setters                                                                                      */
        /**************************************************************************************************************/
        public virtual void SetValueCommunication (int channel, ValueType value) => throw new NotImplementedException ();

        public virtual void SetAllValuesCommunication (ValueType[] values) => throw new NotImplementedException ();

        public virtual void SetChannelValue (int channel, ValueType value) {
            CheckChannelRange (channel);
            channels[channel].SetValue (value);
        }

        public virtual void SetAllChannelValues (ValueType[] values) {
            if (values.Length < channelCount)
                throw new ArgumentOutOfRangeException (nameof (values));

            for (int i = 0; i < channelCount; ++i) {
                channels[i].SetValue (values[i]);
            }
        }

        /**************************************************************************************************************/
        /* Channel Name                                                                                               */
        /**************************************************************************************************************/
        public virtual string GetChannelName (int channel) {
            CheckChannelRange (channel);
            return channels[channel].name;
        }

        public virtual string GetDefualtName (int channel, string prefix = null) {
            CheckChannelRange (channel);
            if (prefix == null) {
                prefix = GetChannelPrefix ();
            }
            return string.Format ("{0}.{1}{2}", name, prefix, channel);
        }

        public virtual string[] GetAllChannelNames () {
            string[] names = new string[channelCount];
            for (int i = 0; i < channelCount; ++i) {
                names[i] = channels[i].name;
            }
            return names;
        }

        public virtual void SetChannelName (int channel, string name) {
            CheckChannelRange (channel);
            channels[channel].name = name;
        }

        /**************************************************************************************************************/
        /* Channel Mode                                                                                               */
        /**************************************************************************************************************/
        public virtual Mode GetChannelMode (int channel) {
            CheckChannelRange (channel);
            return channels[channel].mode;
        }

        public virtual Mode[] GetAllChannelModes () {
            Mode[] modes = new Mode[channelCount];
            for (int i = 0; i < channelCount; ++i) {
                modes[i] = channels[i].mode;
            }
            return modes;
        }

        public virtual void SetChannelMode (int channel, Mode mode) {
            CheckChannelRange (channel);
            channels[channel].SetMode (mode);
        }
    }
}

