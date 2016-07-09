using System;

namespace AquaPic.Drivers
{
    public class AquaPicDrivers {
        public static AnalogInputBase AnalogInput = AnalogInputBase.SharedAnalogInputInstance;
        public static AnalogOutputBase AnalogOutput = AnalogOutputBase.SharedAnalogOutputInstance;
        public static DigitalInputBase DigitalInput = DigitalInputBase.SharedDigitalInputInstance;
    }
}