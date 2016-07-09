//#define HACK_PARITY_ON_WINDOWS
//#define HACK_PARITY_ON_LINUX

using System;
using System.IO.Ports;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using AquaPic.Utilites;

namespace AquaPic.SerialBus
{
    public class AquaPicBusSerialPort
    {
        public SerialPort uart;

        #if !HACK_PARITY_ON_LINUX
        int fd; // termios fd for setting parity on linux
        #endif

        public AquaPicBusSerialPort () {
            
        }

        public void Open (string port, int baudRate) {
            uart = new SerialPort (port, baudRate, Parity.Even, 8);
            uart.StopBits = StopBits.One;
            uart.Handshake = Handshake.None;
            uart.Open ();

            #if !HACK_PARITY_ON_LINUX
            if (Utils.ExecutingOperatingSystem == Platform.Linux) {
                FieldInfo fieldInfo = uart.BaseStream.GetType().GetField("fd", BindingFlags.Instance | BindingFlags.NonPublic);
                fd = (int)fieldInfo.GetValue(uart.BaseStream);
            }
            #endif
        }

        public void Write (byte[] message) {
            if (Utils.ExecutingOperatingSystem == Platform.Windows) {
                WindowsWrite (message);
            } else {
                LinuxWrite (message);
            }
        }

        protected void WindowsWrite (byte[] message) {
            #if HACK_PARITY_ON_WINDOWS
            WriteWithHackParity (message [0], Parity.Mark);                 //send address
            for (int i = 1; i < message.Length; ++i) {                      //send message
                WriteWithHackParity (message [i]);
            }
            #else
            uart.Parity = Parity.Mark;
            uart.Write (message, 0, 1);
            Thread.Sleep (10);
            uart.Parity = Parity.Space;
            uart.Write (message, 1, message.Length - 1);
            #endif
        }

        #if !HACK_PARITY_ON_LINUX
        [DllImport ("AquaPicParityHelper.so")]
        protected static extern bool SetLinuxParity (int fd, Parity parity);
        #endif

        protected void LinuxWrite (byte[] message) {
            #if HACK_PARITY_ON_LINUX
            WriteWithHackParity (message [0], Parity.Mark);                 //send address
            for (int i = 1; i < message.Length; ++i) {                      //send message
                WriteWithHackParity (message [i]);
            }
            #else
            if (!SetLinuxParity (fd, Parity.Mark)) {
                throw new ExternalException ("SetLinuxParity");
            }
            uart.Write (message, 0, 1);
            Thread.Sleep (10);
            
            if (!SetLinuxParity (fd, Parity.Space)) {
                throw new ExternalException ("SetLinuxParity");
            }
            uart.Write (message, 1, message.Length - 1);
            #endif
        }

        protected void WriteWithHackParity (byte data, Parity p = Parity.Space) {
            int count = 0;

            for (int i = 0; i < 8; ++i) {
                if (((data >> i) & 0x01) == 1)
                    ++count;
            }

            if (p == Parity.Mark) {
                uart.Parity = ((count % 2) == 0) ? Parity.Odd : Parity.Even;
            } else {
                uart.Parity = ((count % 2) == 0) ? Parity.Even : Parity.Odd;
            }

            uart.Write (new byte[] { data }, 0, 1);
            Thread.Sleep (10);
        }
    }
}

