using System;
using System.Collections.Generic;

namespace AquaPic.CoilCondition
{
    public class ConditionLocker {
        private static Dictionary<string, Condition> AllConditions = new Dictionary<string, Condition> ();

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

