using System;
using System.IO.Ports;
using PiSharp.LibGpio;
using PiSharp.LibGpio.Entities;
using AquaPic.Utilites;

namespace AquaPic.SerialBus
{
    public class AquaPicBusSerialPort
    {
        public SerialPort uart;

        public AquaPicBusSerialPort () {
            if (Utils.RunningPlatform == Platform.Linux) {
                LibGpio.Gpio.SetupChannel (BroadcomPinNumber.Eighteen, Direction.Output);
            }
        }

        public void Open (string port, int baudRate) {
            uart = new SerialPort (port, baudRate, Parity.Space, 8);
            uart.StopBits = StopBits.One;
            uart.Handshake = Handshake.None;
            uart.Open ();
        }

        public void Write (byte[] message) {
            if (Utils.RunningPlatform == Platform.Windows) {
                WindowsWrite (message);
            } else {
                LinuxWrite (message);
            }
        }

        protected void WindowsWrite (byte[] message) {
            uart.Parity = Parity.Mark;
            uart.Write (message, 0, 1);
            uart.Parity = Parity.Space;
            uart.Write (message, 1, message.Length - 1);
        }

        protected void LinuxWrite (byte[] message) {
            LibGpio.Gpio.OutputValue(BroadcomPinNumber.Eighteen, true);     //enable transmit
            WriteWithParity (message [0], Parity.Mark);                     //send address
            for (int i = 1; i < message.Length; ++i) {                      //send message
                WriteWithParity (message [i]);
            }
            LibGpio.Gpio.OutputValue(BroadcomPinNumber.Eighteen, false);    //enable receive
        }

        protected void WriteWithParity (byte data, Parity p = Parity.Space) {
            int count = 0;

            for (int i = 0; i < 8; ++i) {
                if (((data >> i) & 0x01) == 1)
                    ++count;
            }

            if ((count % 2) == 0) { // even number
                if (p == Parity.Mark)
                    uart.Parity = Parity.Even;
                else
                    uart.Parity = Parity.Odd;
            } else { // odd number
                if (p == Parity.Space)
                    uart.Parity = Parity.Odd;
                else
                    uart.Parity = Parity.Even;
            }

            uart.Write (new byte[] { data }, 0, 1);
        }
    }
}

