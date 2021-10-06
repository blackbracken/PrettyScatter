using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using PrettyScatter.Models;
using PrettyScatter.Utils.Ext;
using ScottPlot;
using ScottPlot.Plottable;

namespace PrettyScatter
{
    public partial class MainWindow : Window
    {
        private readonly ScatterPlot _myScatterPlot;
        private readonly ScatterPlot _highlightedPoint;
        private int _lastHighlightedIndex;
        private bool _mouseInScatterPlot;

        private Log? _log;

        public MainWindow()
        {
            InitializeComponent();

            Title = "PrettyScatter";

            // register listener for D&D
            {
                RootGrid.AllowDrop = true;
                RootGrid.PreviewDragOver += (_, ev) =>
                {
                    if (ev.IsGottenFile())
                    {
                        ev.Effects = ev.Effects = DragDropEffects.Copy;
                    }

                    ev.Handled = true;
                };
                RootGrid.PreviewDrop += (_, ev) =>
                {
                    if (ev.IsGottenFile())
                    {
                        OnDropFiles(ev.GetFilePathsUnsafe());
                    }
                };
            }

            // settings for scatter plot
            {
                double[] dataX = { 1, 2, 3, 4, 5 };
                double[] dataY = { 1, 4, 9, 16, 25 };

                SamplePlot.Configuration.DoubleClickBenchmark = false;

                _highlightedPoint = SamplePlot.Plot.AddPoint(0, 0);
                _highlightedPoint.Color = System.Drawing.Color.Red;
                _highlightedPoint.MarkerSize = 10;
                _highlightedPoint.MarkerShape = MarkerShape.openCircle;
                _highlightedPoint.IsVisible = false;

                _myScatterPlot = SamplePlot.Plot.AddScatterPoints(dataX, dataY);
                SamplePlot.Plot.AddScatter(dataX, dataY);

                SamplePlot.Refresh();
            }
        }

        private void OnMouseHoverPoint(object sender, MouseEventArgs ev)
        {
            if (!_mouseInScatterPlot) return;

            HighlightNearestPlot(sender, ev);
        }

        private void OnMouseGoInScatterPlot(object sender, MouseEventArgs ev)
        {
            _mouseInScatterPlot = true;

            HighlightNearestPlot(sender, ev, true);
        }

        private void OnMouseGoOutScatterPlot(object sender, MouseEventArgs ev)
        {
            _mouseInScatterPlot = false;

            _highlightedPoint.IsVisible = false;
            SamplePlot.Refresh();
        }

        private void HighlightNearestPlot(object sender, MouseEventArgs ev, bool updateForce = false)
        {
            (var pointX, var pointY, var pointIndex) = GetPointNearest();

            _highlightedPoint.Xs[0] = pointX;
            _highlightedPoint.Ys[0] = pointY;
            _highlightedPoint.IsVisible = true;

            if (_lastHighlightedIndex != pointIndex || updateForce)
            {
                _lastHighlightedIndex = pointIndex;

                SamplePlot.Refresh();
            }
        }

        private (double, double, int) GetPointNearest()
        {
            (var mouseCoordX, var mouseCoordY) = SamplePlot.GetMouseCoordinates();
            var xyRatio = SamplePlot.Plot.XAxis.Dims.PxPerUnit / SamplePlot.Plot.YAxis.Dims.PxPerUnit;

            return _myScatterPlot.GetPointNearest(mouseCoordX, mouseCoordY, xyRatio);
        }

        private void OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (!_mouseInScatterPlot) return;

            (_, _, var pointIndex) = GetPointNearest();
            if (pointIndex < 0 || LogList.Items.Count <= pointIndex)
            {
                MessageBox.Show("プロットに対応するコンテンツがありません", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            object item = LogList.Items.GetItemAt(pointIndex);
            LogList.SelectionMode = DataGridSelectionMode.Extended;
            LogList.SelectedItem = item;
            LogList.Focus();
            LogList.ScrollIntoView(item);
        }

        private async void OnDropFiles(string[] paths)
        {
            _log = await Log.FromFile(paths[0]);

            {
                LogList.ItemsSource = new ObservableCollection<LogListItem>(_log.LogTextList.Select((elem, idx) =>
                    new LogListItem
                    {
                        Index = idx,
                        Content = elem,
                    }));
            }
        }

        public struct LogListItem
        {
            public int Index { get; set; }
            public string Content { get; set; }
        }
    }
}