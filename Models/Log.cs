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
        public IImmutableList<string> StartedNewLineLogTextList { get; }

        public Log(IEnumerable<string> logTexts, IEnumerable<string> startedNewLineLogTexts)
        {
            LogTextList = logTexts.ToImmutableList();
            StartedNewLineLogTextList = startedNewLineLogTexts.ToImmutableList();
        }

        public static Log From(string[] array)
        {
            var startedNewLines = array
                .Select(text => Regex.Replace(text, @"(?<=\G.{45})(?!$)", Environment.NewLine));

            return new Log(array, startedNewLines);
        }
    }
}