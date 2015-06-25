using System;
using AquaPic.Modules;

namespace AquaPic
{
    public delegate float GetDimmingLevelHandler ();

    public class DimmingLightBarPlot : BarPlotWidget
    {
        public GetDimmingLevelHandler GetDimmingLevel;

        public DimmingLightBarPlot (string name, GetDimmingLevelHandler GetDimmingLevel) {
            text = name;
            this.GetDimmingLevel = GetDimmingLevel;
        }

        public override void OnUpdate () {
            currentValue = GetDimmingLevel ();
        }
    }
}

