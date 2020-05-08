

using System.Collections.Generic;

namespace MvcPodium.ConsoleApp.Services
{
    public interface IStringUtilService
    {
        string MinifyStringLite(string str);

        string MinifyString(string str);

        string UntabString(string str, int untabLevels=-1, string tabString = null);

        string TabString(string str, int tabLevels = 1, string tabString = null);

        string IndentString(string str, string indentString);

        int CalculateTabLevels(string str, string tabString = null);

        HashSet<string> GetMissingStrings(IEnumerable<string> set1, IEnumerable<string> set2);
    }
}
