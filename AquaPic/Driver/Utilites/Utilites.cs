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
	}
}

