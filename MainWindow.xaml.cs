using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Drawing;
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
        private readonly ScatterPlot _highlightedPoint;
        private ScatterPlot? _myScatterPlot;
        private int _lastHighlightedIndex;
        private bool _mouseInScatterPlot;

        private Log? _log;
        private Plots? _plots;

        public MainWindow()
        {
            InitializeComponent();

            Title = "PrettyScatter";

            // register listener for D&D
            {
                SamplePlot.AllowDrop = true;
                SamplePlot.PreviewDragOver += (_, ev) =>
                {
                    if (ev.IsGottenFile())
                    {
                        ev.Effects = ev.Effects = DragDropEffects.Copy;
                    }

                    ev.Handled = true;
                };
                SamplePlot.PreviewDrop += (_, ev) =>
                {
                    if (ev.IsGottenFile())
                    {
                        OnDropClusterFiles(ev.GetFilePathsUnsafe());
                    }
                };
            }
            {
                LogList.AllowDrop = true;
                LogList.PreviewDragOver += (_, ev) =>
                {
                    if (ev.IsGottenFile())
                    {
                        ev.Effects = ev.Effects = DragDropEffects.Copy;
                    }

                    ev.Handled = true;
                };
                LogList.PreviewDrop += (_, ev) =>
                {
                    if (ev.IsGottenFile())
                    {
                        OnDropLogFiles(ev.GetFilePathsUnsafe());
                    }
                };
            }

            // settings for scatter plot
            {
                SamplePlot.Configuration.DoubleClickBenchmark = false;

                _highlightedPoint = SamplePlot.Plot.AddPoint(0, 0);
                _highlightedPoint.Color = System.Drawing.Color.Red;
                _highlightedPoint.MarkerSize = 10;
                _highlightedPoint.MarkerShape = MarkerShape.openCircle;
                _highlightedPoint.IsVisible = false;

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
            if (GetPointNearest() is not (_, _, _) nearest) return;
            var (pointX, pointY, pointIndex) = nearest;

            _highlightedPoint.Xs[0] = pointX;
            _highlightedPoint.Ys[0] = pointY;
            _highlightedPoint.IsVisible = true;

            if (_lastHighlightedIndex != pointIndex || updateForce)
            {
                _lastHighlightedIndex = pointIndex;

                SamplePlot.Refresh();
            }
        }

        private (double, double, int)? GetPointNearest()
        {
            var (mouseX, mouseY) = SamplePlot.GetMouseCoordinates();
            var xyRatio = SamplePlot.Plot.XAxis.Dims.PxPerUnit / SamplePlot.Plot.YAxis.Dims.PxPerUnit;

            return _myScatterPlot?.GetPointNearest(mouseX, mouseY, xyRatio);
        }

        private void LogList_OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (e.OriginalSource is not DependencyObject source) return;
            if (ItemsControl.ContainerFromElement((DataGrid)sender, source) is not DataGridRow row) return;

            Debug.Print($"{row.GetIndex()}");
        }

        private void SamplePlot_OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (!_mouseInScatterPlot) return;

            if (GetPointNearest() is not (_, _, _) nearest) return;
            var (_, _, pointIndex) = nearest;

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

        private async void OnDropClusterFiles(string[] paths)
        {
            if (_log == null)
            {
                MessageBox.Show("ログデータを先に読み込ませて下さい", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            _plots = await Plots.FromFile(paths[0]);

            {
                SamplePlot.Plot.Clear();

                var cs = _plots.PlotList.Select(p => p.cluster).ToArray();

                var group = _plots.PlotList.GroupBy(p => p.cluster);

                _myScatterPlot = SamplePlot.Plot.AddScatterPoints(
                    _plots.PlotList.Select(p => p.x).ToArray(),
                    _plots.PlotList.Select(p => p.y).ToArray()
                );
                foreach (var g in group)
                {
                    var cluster = g.First().cluster;
                    var xs = g.Select(p => p.x).ToArray();
                    var ys = g.Select(p => p.y).ToArray();

                    var color = cluster switch
                    {
                        0 => Color.OrangeRed,
                        1 => Color.GreenYellow,
                        2 => Color.BlueViolet,
                        3 => Color.Aquamarine,
                        4 => Color.Brown,
                        _ => Color.DarkKhaki
                    };

                    SamplePlot.Plot.AddScatter(xs, ys, lineWidth: 0, color: color);
                }


                SamplePlot.Refresh();
            }
        }

        private async void OnDropLogFiles(string[] paths)
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