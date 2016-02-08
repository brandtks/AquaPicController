using System;
using System.IO.Ports;
using RaspberryGPIOManager;
using AquaPic.Utilites;

namespace AquaPic.SerialBus
{
    public class AquaPicBusSerialPort
    {
        public SerialPort uart;
        private GPIOPinDriver txRxSelectPin;

        public AquaPicBusSerialPort () {
            if (Utils.RunningPlatform == Platform.Linux) {
                txRxSelectPin = new GPIOPinDriver (
                    GPIOPinDriver.Pin.GPIO18, 
                    GPIOPinDriver.GPIODirection.Out,
                    GPIOPinDriver.GPIOState.Low);
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
            txRxSelectPin.State = GPIOPinDriver.GPIOState.High;     //enable transmit
            WriteWithParity (message [0], Parity.Mark);             //send address
            for (int i = 1; i < message.Length; ++i) {              //send message
                WriteWithParity (message [i]);
            }
            txRxSelectPin.State = GPIOPinDriver.GPIOState.Low;      //enable receive
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

