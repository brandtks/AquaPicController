﻿#define TEST_PARITY_ON_WINDOWS
//#define USE_TRANSMIT_RECIEVE_PIN

using System;
using System.IO.Ports;
#if USE_TRANSMIT_RECIEVE_PIN
using PiSharp.LibGpio;
using PiSharp.LibGpio.Entities;
#endif
using AquaPic.Utilites;

namespace AquaPic.SerialBus
{
    public class AquaPicBusSerialPort
    {
        public SerialPort uart;

        public AquaPicBusSerialPort () {
            #if USE_TRANSMIT_RECIEVE_PIN
            if (Utils.RunningPlatform == Platform.Linux) {
                LibGpio.Gpio.SetupChannel (BroadcomPinNumber.Eighteen, Direction.Output);
            }
            #endif
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
            #if TEST_PARITY_ON_WINDOWS
            WriteWithParity (message [0], Parity.Mark);                     //send address
            for (int i = 1; i < message.Length; ++i) {                      //send message
                WriteWithParity (message [i]);
            }
            #else
            uart.Parity = Parity.Mark;
            uart.Write (message, 0, 1);
            uart.Parity = Parity.Space;
            uart.Write (message, 1, message.Length - 1);
            #endif
        }

        protected void LinuxWrite (byte[] message) {
            #if USE_TRANSMIT_RECIEVE_PIN
            LibGpio.Gpio.OutputValue(BroadcomPinNumber.Eighteen, true);     //enable transmit
            #endif
            WriteWithParity (message [0], Parity.Mark);                     //send address
            for (int i = 1; i < message.Length; ++i) {                      //send message
                WriteWithParity (message [i]);
            }
            #if USE_TRANSMIT_RECIEVE_PIN
            LibGpio.Gpio.OutputValue(BroadcomPinNumber.Eighteen, false);    //enable receive
            #endif
        }

        protected void WriteWithParity (byte data, Parity p = Parity.Space) {
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
        }
    }
}
