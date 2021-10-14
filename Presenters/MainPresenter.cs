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

        public Log? Log { get; private set; }
        public Plots? Plots { get; private set; }

        private double? _limitXMax { get; set; }
        public double LimitXMax => _limitXMax ?? 0.0;

        private double? _limitXMin { get; set; }
        public double LimitXMin => _limitXMin ?? 0.0;

        private double? _limitYMax { get; set; }
        public double LimitYMax => _limitYMax ?? 0.0;

        private double? _limitYMin { get; set; }
        public double LimitYMin => _limitYMin ?? 0.0;

        public MainPresenter(MainWindow view)
        {
            _view = view;
        }

        public bool CanLoadPlots()
        {
            return Log != null;
        }

        public bool ShouldSetAxisLimits()
        {
            return _limitXMax == null || _limitXMin == null || _limitYMax == null || _limitYMin == null;
        }

        public void SetAxisLimits(double xMax, double xMin, double yMax, double yMin)
        {
            _limitXMax = xMax;
            _limitXMin = xMin;
            _limitYMax = yMax;
            _limitYMin = yMin;
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