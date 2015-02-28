using System;
using AquaPic.Globals;

namespace AquaPic.PowerDriver
{
    public partial class Power 
    {
        private class PlugData
        {
            private float _power;
            private float _current;
            private Mode _currentMode;

            public float current {
                get { return _current; }
            }
            public float power {
                get { return _power; }
            }
            public Mode currentMode { 
                get { return _currentMode; } 
            }

            public float powerFactor { get; set; }
            public string name { get; set; }
            public MyState currentState { get; set; }
            public MyState requestedState { get; set; }
            public bool returnToRequested { get; set; }


            public event StateChangeHandler onStateChange;
            public event ModeChangedHandler onAuto;
            public event ModeChangedHandler onManual;

            public PlugData () {
                this.name = null;
                this.currentState = MyState.Off;
                this.requestedState = MyState.Off;
                this.returnToRequested = false;
                this._currentMode = Mode.Manual;
                this._current = 0.0f;
                this._power = 0.0f;
                this.powerFactor = 1.0f;
            }

            public void SetCurrent (float c) {
                _current = c;
                _power = _current * Voltage * powerFactor;
            }

            public void SetMode (Mode mode) {
                if ((mode == Mode.Auto) || (mode == Mode.Manual))
                    _currentMode = mode;
            }

            public void OnChangeState (StateChangeEventArgs args) {
                if (onStateChange != null)
                    onStateChange (this, args);
            }

            public void OnModeChangedAuto (ModeChangeEventArgs args) {
                if (returnToRequested) {
                    IndividualControl p;
                    p.Group = args.powerID;
                    p.Individual = args.plugID;
                    SetPlugState (p, requestedState);
                }

                if (onAuto != null)
                    onAuto (this, args);
            }

            public void OnModeChangedManual (ModeChangeEventArgs args) {
                if (onManual != null)
                    onManual (this, args);
            }
        }
    }
}

