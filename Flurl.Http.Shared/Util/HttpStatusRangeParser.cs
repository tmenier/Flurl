using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Flurl.Http.Shared.Util
{
    public static class HttpStatusRangeParser
    {
	    public static bool IsMatch(string pattern, int value) {
		    foreach (var range in pattern.Split(',').Select(p => p.Trim())) {
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
