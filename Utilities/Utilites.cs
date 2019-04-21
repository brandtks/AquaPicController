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
using System.IO;
using System.Linq;
using System.Text;

namespace GoodtimeDevelopment.Utilites
{
    public class Description : Attribute
    {
        public string Text;

        public Description (string text) {
            Text = text;
        }
    }

    public enum Platform
    {
        Windows,
        Linux,
        Mac
    }

    public static class Utils
    {
        private static string _aquaPicEnvironment = string.Empty;
        public static string AquaPicEnvironment {
            get {
                return _aquaPicEnvironment;
            }
            set {
                if (_aquaPicEnvironment != string.Empty) {
                    throw new Exception ("Environment already set");
                }

                _aquaPicEnvironment = value;
            }
        }

        private static string _aquaPicSettings = string.Empty;
        public static string AquaPicSettings {
            get {
                return _aquaPicSettings;
            }
            set {
                if (_aquaPicSettings != string.Empty) {
                    throw new Exception ("Settings already set");
                }

                _aquaPicSettings = value;
            }
        }

        public static Platform ExecutingOperatingSystem {
            get {
                switch (Environment.OSVersion.Platform) {
                case PlatformID.Unix:
                    // Well, there are chances MacOSX is reported as Unix instead of MacOSX.
                    // Instead of platform check, we'll do a feature checks (Mac specific root folders)
                    if (Directory.Exists ("/Applications")
                        & Directory.Exists ("/System")
                        & Directory.Exists ("/Users")
                        & Directory.Exists ("/Volumes")) {
                        return Platform.Mac;
                    }
                    return Platform.Linux;

                case PlatformID.MacOSX:
                    return Platform.Mac;

                default:
                    return Platform.Windows;
                }
            }
        }

        public static string GetDescription (Enum en) {
            var type = en.GetType ();
            var memInfo = type.GetMember (en.ToString ());

            if (memInfo != null && memInfo.Length > 0) {
                var attrs = memInfo[0].GetCustomAttributes (typeof (Description), false);

                if (attrs != null && attrs.Length > 0)
                    return ((Description)attrs[0]).Text;
            }

            return en.ToString ();
        }

        public static bool IsEmpty (this string value) {
            return string.IsNullOrWhiteSpace (value);
        }

        public static bool IsNotEmpty (this string value) {
            return !value.IsEmpty ();
        }

        public static string RemoveWhitespace (this string value) {
            return new string (value.ToCharArray ()
                .Where (c => !char.IsWhiteSpace (c))
                .ToArray ());
        }

        public static float CalcParabola (DateSpan start, DateSpan end, DateSpan now, float min, float max) {
            var period = end.DifferenceInMinutes (start);
            var phase = now.DifferenceInMinutes (start);
            var radian = (phase / period).Map (0, 1, 0, 180).Constrain (0, 180).ToRadians ();
            var delta = max - min;
            return min + (float)(delta * Math.Sin (radian));
        }

        public static float CalcHalfParabola (DateSpan startTime, DateSpan endTime, DateSpan now, float startLevel, float endLevel) {
            float mapFrom1, mapFrom2, basePoint;
            if (endLevel <= startLevel) {
                mapFrom1 = 1;
                mapFrom2 = 0;
                basePoint = endLevel;
            } else {
                mapFrom1 = 0;
                mapFrom2 = 1;
                basePoint = startLevel;
            }

            var period = endTime.DifferenceInMinutes (startTime);
            var phase = now.DifferenceInMinutes (startTime);
            var radian = (phase / period).Map (mapFrom1, mapFrom2, 0, 90).Constrain (0, 90).ToRadians ();
            var delta = Math.Abs (endLevel - startLevel);
            return basePoint + (float)(delta * Math.Sin (radian));
        }

        public static float CalcLinearRamp (DateSpan startTime, DateSpan endTime, DateSpan now, float startLevel, float endLevel) {
            var period = endTime.DifferenceInMinutes (startTime);
            var phase = now.DifferenceInMinutes (startTime);
            return (float)(phase / period).Map (0, 1, startLevel, endLevel);
        }

        public static uint SecondsToMilliseconds (double seconds) {
            return (uint)Math.Round (seconds * 1000);
        }

        public static double ToRadians (this double angle) {
            return (Math.PI / 180) * angle;
        }

        public static double ToDegrees (this double angle) {
            return (180.0 * angle) / Math.PI;
        }

        public static int Map (this int value, int from1, int from2, int to1, int to2) {
            return to1 + (value - from1) * (to2 - to1) / (from2 - from1);
        }

        public static int Constrain (this int value, int min, int max) {
            if (value < min)
                return min;
            if (value > max)
                return max;
            return value;
        }

        public static float Map (this float value, float from1, float from2, float to1, float to2) {
            return to1 + (value - from1) * (to2 - to1) / (from2 - from1);
        }

        public static float Constrain (this float value, float min, float max) {
            if (value < min)
                return min;
            if (value > max)
                return max;
            return value;
        }

        public static double Map (this double value, double from1, double from2, double to1, double to2) {
            return to1 + (value - from1) * (to2 - to1) / (from2 - from1);
        }

        public static double Constrain (this double value, double min, double max) {
            if (value < min)
                return min;
            else if (value > max)
                return max;
            else
                return value;
        }

        public static int ToInt (this double value) {
            return (int)Math.Floor (value + 0.5);
        }

        public static int ToInt (this float value) {
            return (int)Math.Floor (value + 0.5);
        }

        public static bool WithinSetpoint (this double value, double setpoint, double deadband) {
            return (value <= (setpoint + deadband)) && (value >= (setpoint - deadband));
        }

        public static bool WithinSetpoint (this int value, int setpoint, int deadband) {
            return (value <= (setpoint + deadband)) && (value >= (setpoint - deadband));
        }

        public static bool WithinRange (this double value, double lower, double upper) {
            return value >= lower && value <= upper;
        }

        public static bool WithinRange (this int value, int lower, int upper) {
            return value >= lower && value <= upper;
        }

        public static bool MaskToBoolean (byte mask, int shift) {
            byte b = mask;
            byte _shift = (byte)shift;
            b >>= _shift;
            if ((b & 0x01) == 1)
                return true;
            else
                return false;
        }

        public static void BooleanToMask (ref byte mask, bool b, int shift) {
            if (b)
                mask |= (byte)Math.Pow (2, shift);
            else {
                int m = ~(int)Math.Pow (2, shift);
                mask &= (byte)m;
            }
        }

        public static string SecondsToString (this uint time) {
            StringBuilder sb = new StringBuilder ();
            TimeSpan t = TimeSpan.FromSeconds (time);

            if (t.Hours != 0)
                sb.Append (string.Format ("{0}h", t.Hours));

            if (t.Minutes != 0) {
                if (sb.Length != 0) // there are hours
                    sb.Append (":");

                sb.Append (string.Format ("{0}m", t.Minutes));
            }

            if (t.Seconds != 0) {
                if (sb.Length != 0) // there are either hours or mins
                    sb.Append (":");

                sb.Append (string.Format ("{0}s", t.Seconds));
            }

            string ts = sb.ToString ();

            if (string.IsNullOrWhiteSpace (ts))
                ts = "0s";

            return ts;
        }

        public static bool TypeIs (this Type type, Type compareBaseType) {
            if (compareBaseType == null) {
                return false;
            }

            for (var baseType = type; baseType != null; baseType = baseType.BaseType) {
                if (baseType == compareBaseType) {
                    return true;
                }

                var interfaces = baseType.GetInterfaces ();

                for (var i = interfaces.Length - 1; i >= 0; --i) {
                    var compare = i < 0 ? baseType : interfaces[i];
                    if (compare == compareBaseType || (compare.IsGenericType && compare.GetGenericTypeDefinition () == compareBaseType)) {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}

