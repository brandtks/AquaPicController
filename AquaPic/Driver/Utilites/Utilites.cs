using System;
using System.Reflection;

namespace AquaPic.Utilites
{
	public class Description : Attribute {
		public string Text;

		public Description (string text) {
			Text = text;
		}
	}

	public static class Utils
	{
		static Utils () {
		}

		public static string GetDescription (Enum en) {
			Type type = en.GetType ();
			MemberInfo[] memInfo = type.GetMember (en.ToString());

			if (memInfo != null && memInfo.Length > 0) {
				object[] attrs = memInfo[0].GetCustomAttributes (typeof (Description), false);

				if (attrs != null && attrs.Length > 0)
					return ((Description)attrs[0]).Text;
			}

			return en.ToString();
		}

        public static float CalcParabola(TimeDate start, TimeDate end, TimeDate now, float min, float max) {
            double period = start.DifferenceInTime(end);
            double phase = start.DifferenceInTime(now);
            double delta = max - min;
            double degrees = (180 * (phase / period)) / Math.PI;
            return min + (float)(delta * Math.Sin(degrees));
        }

        public static int Map (this int value, int from1, int from2, int to1, int to2) {
            return (value - from1) / (to1 - from1) * (to2 - from2) + from2;
        }

        public static float Map (this float value, float from1, float from2, float to1, float to2) {
            return (value - from1) / (to1 - from1) * (to2 - from2) + from2;
        }

        public static double Map (this double value, double from1, double from2, double to1, double to2) {
            return (value - from1) / (to1 - from1) * (to2 - from2) + from2;
        }
	}
}

