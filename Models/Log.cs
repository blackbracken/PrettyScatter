using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Threading.Tasks;

namespace PrettyScatter.Models
{
    public sealed class Log
    {
        public IImmutableList<string> LogTextList { get; }

        public Log(IEnumerable<string> logTextList)
        {
            LogTextList = logTextList.ToImmutableList();
        }

        public static async Task<Log> FromFile(string path)
        {
            var logTexts = await Task.Run(() => File.ReadAllLines(path));
            return new Log(logTexts.ToImmutableList());
        }
    }
}
