using System;
using AquaPic.Modules;

namespace AquaPic
{
    public class ActinicBarPlot : BarPlotWidget
    {
        public ActinicBarPlot () : base () {
            text = "Actinic LED";
            OnUpdate ();
        }

        public override void OnUpdate () {
            currentValue = Lighting.GetCurrentDimmingLevel (Lighting.GetLightIndex ("Actinic LED"));
        }
    }

    public class WhiteBarPlot : BarPlotWidget
    {
        public WhiteBarPlot () : base () {
            text = "White LED";
            OnUpdate ();
        }

        public override void OnUpdate () {
            currentValue = Lighting.GetCurrentDimmingLevel (Lighting.GetLightIndex ("White LED"));
        }
    }
}

