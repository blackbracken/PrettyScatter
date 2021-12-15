using System.ComponentModel;
using System.Linq;

namespace PrettyScatter.ViewModels
{
    public class MainViewModel
    {
        public LogItem[] LogItems { get; }

        public MainViewModel()
        {
            LogItems = Enumerable.Range(0, 10).Select(n => new LogItem(n, "item")).ToArray();
        }
    }

    public class LogItem
    {
        public int Index { get; set; }
        public string Text { get; set; }

        public LogItem(int index, string text)
        {
            Index = index;
            Text = text;
        }
    }
}