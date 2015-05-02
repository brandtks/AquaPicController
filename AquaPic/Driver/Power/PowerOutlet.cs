using System;
using AquaPic.Globals;
using AquaPic.CoilRuntime;

namespace AquaPic.PowerDriver
{
    public partial class Power 
    {
        private class OutletData
        {
            public static float Voltage = 115;

            public float wattPower;
            public float ampCurrent;
            public Mode mode;
            public float powerFactor;
            public string name;
            public MyState currentState;
            public MyState manualState;
            public MyState fallback;
            public Coil OutletControl;

            #if SIMULATION
            public bool Updated;
            #endif

            public event StateChangeHandler onStateChange;
            public event ModeChangedHandler onAuto;
            public event ModeChangedHandler onManual;

            public OutletData (string name, OutputHandler outputTrue, OutputHandler outputFalse) {
                this.name = name;
                this.currentState = MyState.Off;
                this.manualState = MyState.Off;
                this.fallback = MyState.Off;
                this.mode = Mode.Manual;
                this.ampCurrent = 0.0f;
                this.wattPower = 0.0f;
                this.powerFactor = 1.0f;
                this.OutletControl = new Coil ();
                this.OutletControl.ConditionChecker = delegate() {
                    return false;
                };
                this.OutletControl.OutputTrue += outputTrue;
                this.OutletControl.OutputFalse += outputFalse;

                #if SIMULATION
                this.Updated = true;
                #endif
            }

            public void SetAmpCurrent (float c) {
                ampCurrent = c;
                wattPower = ampCurrent * Voltage * powerFactor;
            }

            public void OnChangeState (StateChangeEventArgs args) {
                currentState = args.state;
                #if SIMULATION
                this.Updated = true;
                #endif

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

