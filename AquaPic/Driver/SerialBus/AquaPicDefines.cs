using System;
using AquaPic.Utilites; // for Description

namespace AquaPic.SerialBus
{
    public enum AquaPicBusStatus {
        [Description("AquaPic bus port not open")]
        notOpen = 1,

        [Description("AquaPic bus port is open")]
        open,

        [Description("Starting Communication")]
        communicationStart,

        [Description("Read/Write was successful")]
        communicationSuccess,

        [Description("An exception error has occurred")]
        exception = 101,

        [Description("Response timed out from slave")]
        timeout,

        [Description("Cyclic reducency check error")]
        crcError
    }

    public delegate void ResponseCallback (CallbackArgs args);
    public delegate void StatusUpdateHandler (object sender);

    public class CallbackArgs {
        public byte[] readBuffer;

        public CallbackArgs (byte[] readBuffer) {
            this.readBuffer = readBuffer;
        }

        public unsafe void copyBuffer (void* data, int size) {
            byte* d = (byte*)data;
            for (int i = 0; i < size; ++i)
                *d++ = readBuffer [3 + i];
        }
    }
}
