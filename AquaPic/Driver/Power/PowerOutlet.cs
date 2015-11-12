using System;
using AquaPic.Utilites;
using AquaPic.Runtime;

namespace AquaPic.Drivers
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
            public string owner;

//            public event StateChangeHandler StateChangeEvent;
//            public event ModeChangedHandler ModeChangeEvent;

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
                this.OutletControl.ConditionChecker = () => {
                    return false;
                };
                this.OutletControl.OutputTrue = outputTrue;
                this.OutletControl.OutputFalse = outputFalse;
                owner = "Power";
            }

            public void SetAmpCurrent (float c) {
                ampCurrent = c;
                wattPower = ampCurrent * Voltage * powerFactor;
            }

//            public void OnModeChange (ModeChangeEventArgs args) {
////                if (ModeChangeEvent != null)
////                    ModeChangeEvent (this, args);
//            }

//            public void OnChangeState (StateChangeEventArgs args) {
//                currentState = args.state;
//
////                if (StateChangeEvent != null) {
////                    StateChangeEvent (this, args);
////                }
//            }
        }
    }
}

