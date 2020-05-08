using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using MvcPodium.ConsoleApp.Extensions;

namespace MvcPodium.ConsoleApp.Services
{
    public class StringUtilService : IStringUtilService
    {
        public string MinifyStringLite(string str)
        {
            if (str is null) { return null; }
            return Regex.Replace(str, @"\s+", " ");
        }

        public string MinifyString(string str)
        {
            if (str is null) { return null; }
            return Regex.Replace(
                MinifyStringLite(str),
                @"(^\s+)|(\s+$)|((?<a>\S)\s+(?<b>[^A-Za-z0-9_\s])\s+(?<c>\S))|((?<d>[^A-Za-z0-9_\s])\s+(?<e>\S))|((?<f>\S)\s+(?<g>[^A-Za-z0-9_\s]))",
                "${a}${b}${c}${d}${e}${f}${g}"
            );
        }

        public string UntabString(string str, int untabLevels=-1, string tabString = null)
        {
            string tab = tabString ?? "    ";
            if (untabLevels == -1) 
            {
                untabLevels = CalculateTabLevels(str, tab);
            }
            if (untabLevels == 0) { return str; }
            //string newString = includeFirstLine ? Regex.Replace(str, $@"^({tab}){{{untabLevels}}}", "") : str;
            return Regex.Replace(str, $@"^({tab}){{{untabLevels}}}", "", RegexOptions.Multiline);
        }

        public TCollection TabStrings<TCollection>(
            TCollection stringCollection, 
            int tabLevels = 1, 
            string tabString = null
            ) where TCollection : ICollection<string>, new()
        {
            if (EqualityComparer<TCollection>.Default.Equals(stringCollection, default)) { return default; }
            
            var newStrings = new TCollection();
            foreach (var str in stringCollection)
            {
                newStrings.Add(TabString(str, tabLevels, tabString));
            }
            return newStrings;
        }

        public string TabString(string str, int tabLevels = 1, string tabString = null)
        {
            string tab = tabString ?? "    ";
            if (tabLevels == 0) { return str; }
            return Regex.Replace(str, "^", tab.Repeat(tabLevels), RegexOptions.Multiline);
        }

        public string IndentString(string str, string indentString)
        {
            return Regex.Replace(str, "^", indentString, RegexOptions.Multiline);
        }

        public int CalculateTabLevels(string str, string tabString = null)
        {
            if (str is null) { return 0; }
            string tab = tabString ?? "    ";
            int tabLevels = 0;

            var match1 = Regex.Match(str, $@"\r?\n({tab})+.*$");
            if (match1.Success)
            {
                tabLevels = match1.Groups[1].Captures.Count;
            }
            return tabLevels;
        }

        public HashSet<string> GetMissingStrings(IEnumerable<string> set1, IEnumerable<string> set2)
        {
            if (set2 is null)
            {
                return new HashSet<string>();
            }
            var missing = new HashSet<string>(set2);
            if (set1 != null)
            {
                missing.ExceptWith(set1);
            }
            return missing;
        }
    }
}
