using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
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

        #if UNSAFE_COMMS
        public unsafe void CopyBuffer (void* data, int size) {
            byte* d = (byte*)data;
            for (int i = 0; i < size; ++i)
                *d++ = readBuffer [3 + i];
        }
        #endif

        public T GetDataFromReadBuffer<T> (int offset) {
            GCHandle handle = GCHandle.Alloc (readBuffer, GCHandleType.Pinned);
            IntPtr ptr = new IntPtr (handle.AddrOfPinnedObject ().ToInt64 () + offset);
            T data = (T)Marshal.PtrToStructure (ptr, typeof(T));
            handle.Free ();
            return data;
        }
    }

    public class WriteBuffer
    {
        private List<byte> _buffer;

        public byte[] buffer {
            get {
                return _buffer.ToArray ();
            }
        }

        public int size {
            get {
                return _buffer.Count;
            }
        }

        public WriteBuffer () {
            _buffer = new List<byte> ();
        }

        public void Add (object value, int maxLength) {
            int rawsize = Marshal.SizeOf (value);
            byte[] rawdata = new byte[rawsize];
            GCHandle handle = GCHandle.Alloc (rawdata, GCHandleType.Pinned);
            Marshal.StructureToPtr (value, handle.AddrOfPinnedObject (), false);
            handle.Free ();

            if (maxLength < rawdata.Length) {
                byte[] temp = new byte[maxLength];
                Array.Copy (rawdata, temp, maxLength);
                _buffer.AddRange (temp);
            } else {
                _buffer.AddRange (rawdata);
            }
        }
    }

    #if UNSAFE_COMMS
    public partial class AquaPicBus
    {
        private class Utilities
        {
            public static unsafe void MemoryCopy (void* fromData, void* toData, int size) {
                byte* f = (byte*)fromData;
                byte* t = (byte*)toData;

                while (size-- != 0)
                    *t++ = *f++;
            }
        }
    }
    #endif
}
