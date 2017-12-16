#region License

/*
    AquaPic Main Control - Handles all functionality for the AquaPic aquarium controller.

    Copyright (c) 2017 Goodtime Development

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see http://www.gnu.org/licenses/
*/

#endregion // License

using System;
using GoodtimeDevelopment.Utilites;

namespace AquaPic.Globals
{
    public enum Mode : byte
    {
        Manual = 1,
        Auto
    }

    public enum AnalogType : byte
    {
        [Description ("0-10Vdc")]
        ZeroTen = 0,
        [Description ("0-5Vdc")]
        ZeroFive,
        PWM = 255
    }

    public enum MyState : byte
    {
        Off = 0,
        On = 1,
        Set,
        Reset,
        Invalid
    }

    public enum LightingTime : byte
    {
        Daytime = 1,
        Nighttime
    }

    public static class Globals 
    {
        public static MyState ToMyState (this bool value) {
            if (value) {
                return MyState.On;
            }

            return MyState.Off;
        }

        public static bool ToBool (this MyState value) {
            if (value == MyState.On) {
                return true;
            }

            return false;
        }
    }
}

ol (this MyState value) {
            if (value == MyState.On) {
                return true;
            }

            return false;
        }
    }
}

