using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace System
{
    public static class UriExtensions
    {
        private static readonly Regex queryStringRegex;
        static UriExtensions()
        {
            queryStringRegex = new Regex(@"[\?&](?<name>[^&=]+)=(?<value>[^&=]+)");
        }

        public static IEnumerable<KeyValuePair<string, string>> ParseQueryString(this Uri uri)
        {
            if (uri == null)
                throw new ArgumentException("uri");

            var matches = queryStringRegex.Matches(uri.OriginalString);
            for (int i = 0; i < matches.Count; i++)
            {
                var match = matches[i];
                yield return new KeyValuePair<string, string>(match.Groups["name"].Value, match.Groups["value"].Value);
            }
        }
    }
}
