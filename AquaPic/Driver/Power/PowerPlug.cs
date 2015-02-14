using System;
using AquaPic.Globals;

namespace AquaPic.PowerDriver
{
    public partial class Power 
    {
        private class PlugData 
        {
            public string name { get; set; }
            public bool currentState { get; set; }
            public bool requestedState { get; set; }
            public Mode mode { get; set; }
            public bool rtnToRequested;
            public event modeChangedHandler onAuto;
            public event modeChangedHandler onManual;
            public event stateChangeHandler onStateChange;

            /* <For Future Expansion>
            public plugData (string name, bool rtnToRequested) {
                this.name = name;
                this.currentState = false;
                this.requestedState = false;
                this.mode = Mode.Auto;
                this.rtnToRequested = rtnToRequested;
            }*/

            public PlugData () {
                this.name = null;
                this.currentState = false;
                this.requestedState = false;
                this.mode = Mode.Auto;
                this.rtnToRequested = false;
            }

            public void onModeChangedAuto (modeChangeEventArgs args) {
                if (rtnToRequested) {
                    pwrPlug p;
                    p.powerID = args.powerID;
                    p.plugID = args.plugID;
                    //power.setPlug (p, requestedState);
                }

                if (onAuto != null)
                    onAuto (this, args);
            }

            public void onModeChangedManual (modeChangeEventArgs args) {
                if (onManual != null)
                    onManual (this, args);
            }

            public void onChangeState (stateChangeEventArgs args) {
                if (onStateChange != null)
                    onStateChange (this, args);
            }
        }
    }
}

