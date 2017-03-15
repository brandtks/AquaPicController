using System;
using AquaPic.Utilites;
using AquaPic.Runtime;
using AquaPic.Drivers;

namespace AquaPic.Sensors
{
    public enum SwitchType {
        [Description("Normally Opened")]
        NormallyOpened,
        [Description("Normally Closed")]
        NormallyClosed
    }

    public enum SwitchFunction {
        LowLevel,
        HighLevel,
        ATO,
        Other
    }

    public class FloatSwitch : ISensor<bool>
    {
        protected string _name;
        public string name {
            get {
                return _name;
            }
        }

        protected OnDelayTimer _onDelayTimer;
        public OnDelayTimer onDelayTimer {
            get {
                return _onDelayTimer;
            }
        }

        protected IndividualControl _channel;
        public IndividualControl channel {
            get {
                return _channel;
            }
        }

        protected bool _activated;
        public bool activated {
            get {
                return _activated;
            }
        }

        protected SwitchType _type;
        public SwitchType type {
            get {
                return _type;
            }
            set {
                if (value != _type) { // when switching type activation reverses
                    _activated = !_activated;
                }
                _type = value;
            }
        }

        public SwitchFunction function;
        public float physicalLevel;

        public FloatSwitch (
            string name,
            SwitchType type,
            SwitchFunction function,
            float physicalLevel,
            IndividualControl channel,
            uint timeOffset
        ) {
            _activated = false;
            _name = name;
            _type = type;
            this.function = function;
            this.physicalLevel = physicalLevel;
            _onDelayTimer = new OnDelayTimer (timeOffset);

            Add (channel);
        }

        public void Add (IndividualControl channel) {
            _channel = channel;
            AquaPicDrivers.DigitalInput.AddChannel (_channel, _name);
        }

        public void Remove () {
            AquaPicDrivers.DigitalInput.RemoveChannel (_channel);
        }

        public bool Get () {
            var state = AquaPicDrivers.DigitalInput.GetChannelValue (_channel);
            bool timerFinished;

            if (_type == SwitchType.NormallyClosed)
                state = !state; //normally closed switches are reversed

            timerFinished = _onDelayTimer.Evaluate (_activated != state); // if current state and switch activation do not match start timer
            if (timerFinished) // once timer has finished, toggle switch activation
                _activated = !_activated;

            return _activated;
        }

        public void SetName (string name) {
            _name = name;
            AquaPicDrivers.DigitalInput.SetChannelName (_channel, _name);
        }
    }
}

