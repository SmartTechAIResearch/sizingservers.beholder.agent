/*
 * 2018 Sizing Servers Lab
 * University College of West-Flanders, Department GKG
 * 
 */

using System.Collections.Generic;

namespace sizingservers.beholder.agent.shared {
    public static class Helper {
        /// <summary>
        /// Combines a component dictionary (key = name, value = number of) to a flat string.
        /// </summary>
        /// <param name="componentDict">The component dictionary.</param>
        /// <returns
        /// </returns>
        public static string ComponentDictToString(SortedDictionary<string, int> componentDict) {
            string[] arr = new string[componentDict.Count];
            int i = 0;
            foreach (var kvp in componentDict) {
                string key = kvp.Key;
                if (kvp.Value > 1) key += " x" + kvp.Value;

                arr[i++] = key;
            }
            return string.Join("\t", arr);
        }
    }
}
