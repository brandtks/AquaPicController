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

            public OutletData (string name, OutputHandler outputTrue, OutputHandler outputFalse) {
                this.name = name;
                currentState = MyState.Off;
                manualState = MyState.Off;
                fallback = MyState.Off;
                mode = Mode.Manual;
                ampCurrent = 0.0f;
                wattPower = 0.0f;
                powerFactor = 1.0f;
                OutletControl = new Coil ();
                OutletControl.ConditionChecker = () => {
                    return false;
                };
                OutletControl.OutputTrue = outputTrue;
                OutletControl.OutputFalse = outputFalse;
                owner = "Power";
            }

            public void SetAmpCurrent (float c) {
                ampCurrent = c;
                wattPower = ampCurrent * Voltage * powerFactor;
            }
        }
    }
}

