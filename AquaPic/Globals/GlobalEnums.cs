#region License

/*
    AquaPic Main Control - Handles all functionality for the AquaPic aquarium controller.

    Copyright (c) 2017 Skyler Brandt

    This program is free software; you can redistribute it and/or
    modify it under the terms of the GNU General Public License
    as published by the Free Software Foundation; either version 2
    of the License, or (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program; if not, write to the Free Software
    Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.

    Optionally you can also view the license at <http://www.gnu.org/licenses/>.
*/

#endregion // License

﻿using System;

namespace AquaPic.Utilites
{
    public enum Mode : byte {
        Manual = 1,
        Auto
    }

    public enum AnalogType : byte {
        [Description("0-10Vdc")]
        ZeroTen = 0,
        [Description("0-5Vdc")]
        ZeroFive,
        PWM = 255
    }

    public enum MyState : byte {
        Off = 0,
        On = 1,
        Set,
        Reset,
        Invalid
    }

    public enum LightingTime : byte {
        Daytime = 1,
        Nighttime
    }
}

