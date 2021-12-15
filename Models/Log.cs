using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PrettyScatter.Models
{
    public sealed class Log
    {
        public IImmutableList<string> LogTextList { get; }

        public Log(IEnumerable<string> logTexts)
        {
            LogTextList = logTexts.ToImmutableList();
        }

        public static Log From(string[] array)
        {
            return new Log(array);
        }
    }
}