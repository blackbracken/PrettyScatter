using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PrettyScatter.Models;

namespace PrettyScatter.Presenters
{
    public class MainPresenter
    {
        private readonly MainWindow _view;

        public Log? Log { get; private set; } = null;
        public Plots? Plots { get; private set; } = null;


        public MainPresenter(MainWindow view)
        {
            _view = view;
        }

        public bool CanLoadPlots()
        {
            return Log != null;
        }

        public async Task<bool> LoadLog(string filePath)
        {
            var logTexts = await File.ReadAllLinesAsync(filePath);
            Log = Log.From(logTexts);

            return true;
        }

        public async Task<bool> LoadPlots(string filePath)
        {
            var rawPlots = await File.ReadAllLinesAsync(filePath);
            Plots = await Plots.From(rawPlots);

            return Plots != null;
        }
    }
}