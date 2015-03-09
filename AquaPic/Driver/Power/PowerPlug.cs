using System;
using AquaPic.Globals;

namespace AquaPic.PowerDriver
{
    public partial class Power 
    {
        private class PlugData
        {
            public float wattPower;
            public float ampCurrent;
            public Mode mode;
            public float powerFactor;
            public string name;
            public MyState currentState;
            public MyState requestedState;
            public MyState fallback;
            public bool returnToRequested;

            public event StateChangeHandler onStateChange;
            public event ModeChangedHandler onAuto;
            public event ModeChangedHandler onManual;

            public PlugData () {
                this.name = null;
                this.currentState = MyState.Off;
                this.requestedState = MyState.Off;
                this.fallback = MyState.Off;
                this.returnToRequested = false;
                this.mode = Mode.Manual;
                this.ampCurrent = 0.0f;
                this.wattPower = 0.0f;
                this.powerFactor = 1.0f;
            }

            public void SetAmpCurrent (float c) {
                ampCurrent = c;
                wattPower = ampCurrent * Voltage * powerFactor;
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

