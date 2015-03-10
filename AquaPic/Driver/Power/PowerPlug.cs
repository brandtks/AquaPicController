using System;
using AquaPic.Globals;
using AquaPic.CoilCondition;

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
            public MyState manualState;
            public MyState fallback;
            public Coil plugControl;

            public event StateChangeHandler onStateChange;
            public event ModeChangedHandler onAuto;
            public event ModeChangedHandler onManual;

            public PlugData (string name, Condition manualControl, OutputHandler outputTrue, OutputHandler outputFalse) {
                this.name = name;
                this.currentState = MyState.Off;
                this.manualState = MyState.Off;
                this.fallback = MyState.Off;
                this.mode = Mode.Manual;
                this.ampCurrent = 0.0f;
                this.wattPower = 0.0f;
                this.powerFactor = 1.0f;
                this.plugControl = new Coil (name);
                this.plugControl.Conditions.Script = manualControl.Name;
                this.plugControl.OutputTrue += outputTrue;
                this.plugControl.OutputFalse += outputFalse;
            }

            public void SetAmpCurrent (float c) {
                ampCurrent = c;
                wattPower = ampCurrent * Voltage * powerFactor;
            }

            public void OnChangeState (StateChangeEventArgs args) {
                currentState = args.state;

                if (onStateChange != null)
                    onStateChange (this, args);
            }

            public void OnModeChangedAuto (ModeChangeEventArgs args) {
//                if (returnToRequested) {
//                    IndividualControl p;
//                    p.Group = args.powerID;
//                    p.Individual = args.plugID;
//                    SetPlugState (p, requestedState);
//                }

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

