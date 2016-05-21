using System;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;

namespace Flurl.Http
{
    public static class HttpStatusRangeParser
    {
	    public static bool IsMatch(string pattern, HttpStatusCode value) {
		    return IsMatch(pattern, (int)value);
	    }

	    public static bool IsMatch(string pattern, int value) {
		    if (pattern == null)
			    return false;

		    foreach (var range in pattern.Split(',').Select(p => p.Trim())) {
			    if (range == "")
				    continue;

			    if (range == "*")
				    return true; // special case - allow everything

			    var bounds = range.Split('-');
			    int lower = 0, upper = 0;

			    var valid =
				    bounds.Length <= 2 &&
				    int.TryParse(Regex.Replace(bounds.First().Trim(), "[*xX]", "0"), out lower) &&
				    int.TryParse(Regex.Replace(bounds.Last().Trim(), "[*xX]", "9"), out upper);

			    if (!valid) {
				    throw new ArgumentException(string.Format(
					    "Invalid range pattern: \"{0}\". Examples of allowed patterns: \"400\", \"4xx\", \"300,400-403\", \"*\".", pattern));
			    }

			    if (value >= lower && value <= upper)
				    return true;
		    }
		    return false;
	    }
    }
}
