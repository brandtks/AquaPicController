using System;
using System.Collections.Generic;

namespace AquaPic.CoilCondition
{
    public class ConditionLocker {
        //public ConditionLocker () { }
        //public static List<Condition> AllConditions = new List<Condition> ();
        private static Dictionary<string, Condition> AllConditions = new Dictionary<string, Condition> ();

        //public static Condition GetCondition (string name) {
        //    for (int i = 0; i < AllConditions.Count; ++i) {
        //        if (string.Compare (AllConditions [i].Name, name, StringComparison.InvariantCultureIgnoreCase) == 0)
        //            return AllConditions [i];
        //    }
        //    return null;
        //}

        public static void AddCondition (Condition c) {
            string n = c.Name.ToLowerInvariant ();
            AllConditions.Add (n, c);
        }

        public static Condition GetCondition (string key) {
            Condition c;
            key = key.ToLowerInvariant ();
            try {
                c = AllConditions [key];
            } catch {
                return null;
            }

            return c;
        }
    }
}

