using System;
using AquaPic.Globals;

namespace AquaPic.PowerDriver
{
    public partial class Power 
    {
        private class PlugData
        {
            public float powerWatts;
            public float currentAmps;
            public Mode mode;
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
                this.mode = Mode.Manual;
                this.currentAmps = 0.0f;
                this.powerWatts = 0.0f;
                this.powerFactor = 1.0f;
            }

            public void SetCurrent (float c) {
                currentAmps = c;
                powerWatts = currentAmps * Voltage * powerFactor;
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

