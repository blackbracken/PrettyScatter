using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using PrettyScatter.Models;
using PrettyScatter.Presenters;
using PrettyScatter.Utils.Ext;
using PrettyScatter.ViewModels;
using ScottPlot;
using ScottPlot.Plottable;

namespace PrettyScatter.Ui.Main
{
    public partial class MainWindow : Window
    {
        private readonly MainPresenter _presenter;
        private MainViewModel ViewModel => (MainViewModel)DataContext;

        private ScatterPlot _highlightedPoint;
        private ScatterPlot? _myScatterPlot;
        private int _lastHighlightedIndex;
        private bool _mouseInScatterPlot;

        private readonly ObservableCollection<ClusterListBoxItem> _clusterList;
        private readonly Dictionary<int, Color> _clusterColorMap;

        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainViewModel();

            _presenter = new MainPresenter(this);

            _clusterList = new ObservableCollection<ClusterListBoxItem>();
            ClusterListBox.ItemsSource = _clusterList;
            _clusterColorMap = new Dictionary<int, Color>();

            Title = "PrettyScatter";

            {
                Graph.AllowDrop = true;
                Graph.PreviewDragOver += (_, ev) =>
                {
                    if (ev.IsGottenFile())
                    {
                        ev.Effects = DragDropEffects.Copy;
                    }

                    ev.Handled = true;
                };
                Graph.PreviewDrop += (_, ev) =>
                {
                    if (ev.IsGottenFile())
                    {
                        LoadPlotsFile(ev.GetFilePathsUnsafe()[0]);
                    }
                };
            }
            {
                LogGrid.AllowDrop = true;
                LogGrid.PreviewDragOver += (_, ev) =>
                {
                    if (ev.IsGottenFile())
                    {
                        ev.Effects = DragDropEffects.Copy;
                    }

                    ev.Handled = true;
                };
                LogGrid.PreviewDrop += (_, ev) =>
                {
                    if (ev.IsGottenFile())
                    {
                        LoadLogFile(ev.GetFilePathsUnsafe()[0]);
                    }
                };
            }

            {
                ResetGraph();
                Graph.Configuration.DoubleClickBenchmark = false;

                Graph.Refresh();
            }
        }

        private void Graph_OnMouseGoInScatterPlot(object sender, MouseEventArgs ev)
        {
            _mouseInScatterPlot = true;
        }

        private void Graph_OnMouseGoOutScatterPlot(object sender, MouseEventArgs ev)
        {
            _mouseInScatterPlot = false;

            _highlightedPoint.IsVisible = false;
            Graph.Refresh();
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

                // TODO: ちゃんとはみ出ないように
                var text = $"選択: {pointIndex} / {_presenter.Log?.LogTextList[pointIndex] ?? "none"}";
                LogText.Text = text.Length < 100 ? text : (new string(text.Take(200).ToArray()) + "...");

                Graph.Refresh();
            }
        }

        private (double, double, int)? GetPointNearest()
        {
            var (mouseX, mouseY) = Graph.GetMouseCoordinates();
            var xyRatio = Graph.Plot.XAxis.Dims.PxPerUnit / Graph.Plot.YAxis.Dims.PxPerUnit;


            return _myScatterPlot?.GetPointNearest(mouseX, mouseY, xyRatio);
        }

        private void Graph_OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (!_mouseInScatterPlot) return;

            if (GetPointNearest() is not (_, _, _) nearest) return;
            var (x, y, pointIndex) = nearest;

            if (pointIndex < 0 || LogGrid.Items.Count <= pointIndex)
            {
                MessageBox.Show("プロットに対応するコンテンツがありません", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (_presenter.Plots?.PlotList
                ?.First(p => Math.Abs(p.X - x) < 0.00001 && Math.Abs(p.Y - y) < 0.00001) is not { } target) return;
            if (_presenter.Plots?.PlotList?.ToList()?.IndexOf(target) is not { } index) return;

            LogText.Text = $"[ログ選択] X: {x:F2} Y: {y:F2} クラスター: {target.Cluster} 内容: {((LogListItem) LogGrid.Items.GetItemAt(index)).Content}";

            object item = LogGrid.Items.GetItemAt(index);
            LogGrid.SelectionMode = DataGridSelectionMode.Extended;
            LogGrid.SelectedItem = item;
            LogGrid.Focus();
            LogGrid.ScrollIntoView(item);
        }


        private async void LoadPlotsFile(string path)
        {
            if (!_presenter.CanLoadPlots())
            {
                MessageBox.Show("ログデータを先に読み込ませて下さい", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (await _presenter.LoadPlots(path))
            {
                LogText.Text = $"データ読み込み: {path}";
            }
            else
            {
                MessageBox.Show("ファイルの読み込みに失敗しました", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (_presenter.Plots is not { } plots) return;

            _clusterColorMap.Clear();
            foreach (var c in plots.PlotList.Select(p => p.Cluster).Distinct())
            {
                var rnd = new Random(c);
                _clusterColorMap.Add(c, Color.FromArgb(255, rnd.Next(0, 255), rnd.Next(0, 255), rnd.Next(0, 255)));
            }

            RerenderGraph();

            _clusterList.Clear();
            var clusterList = plots.PlotList.Select(p => p.Cluster).Distinct().ToList();
            clusterList.Sort();

            foreach (var c in clusterList)
            {
                _clusterList.Add(new ClusterListBoxItem(c));
            }
        }

        private async void LoadLogFile(string path)
        {
            LogText.Text = $"データ読み込み中: {path}";

            if (await _presenter.LoadLog(path))
            {
                LogText.Text = $"データ読み込み完了: {path}";
            }
            else
            {
                MessageBox.Show("ファイルの読み込みに失敗しました", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (_presenter.Log is not { } log) return;

            LogGrid.ItemsSource = new ObservableCollection<LogListItem>(log.LogTextList
                .Select((elem, idx) =>
                    new LogListItem
                    {
                        Index = idx + 1,
                        Content = elem,
                    }
                )
            );
        }

        private void RerenderGraph(Func<int, bool>? clusterFilter = null)
        {
            ResetGraph();
            if (_presenter.Plots is not { } plots) return;

            var groups = plots.PlotList.GroupBy(p => p.Cluster);

            _myScatterPlot = Graph.Plot.AddScatterPoints(
                plots.PlotList.Where(p => clusterFilter?.Invoke(p.Cluster) ?? true).Select(p => p.X).ToArray(),
                plots.PlotList.Where(p => clusterFilter?.Invoke(p.Cluster) ?? true).Select(p => p.Y).ToArray()
            );

            foreach (var g in groups)
            {
                var cluster = g.First().Cluster;
                if (!(clusterFilter?.Invoke(cluster) ?? true)) continue;

                var xs = g.Select(p => p.X).ToArray();
                var ys = g.Select(p => p.Y).ToArray();

                Graph.Plot.AddScatter(xs, ys, lineWidth: 0, color: _clusterColorMap[cluster]);
            }

            if (_presenter.ShouldSetAxisLimits())
            {
                var limits = Graph.Plot.GetAxisLimits();
                _presenter.SetAxisLimits(limits.XMax, limits.XMin, limits.YMax, limits.YMin);
            }
            else
            {
                Graph.Plot.SetAxisLimitsX(_presenter.LimitXMin, _presenter.LimitXMax);
                Graph.Plot.SetAxisLimitsY(_presenter.LimitYMin, _presenter.LimitYMax);
            }

            Graph.Refresh(true);
        }

        private void ResetGraph()
        {
            Graph.Plot.Clear();
            _myScatterPlot = null;

            _highlightedPoint = Graph.Plot.AddPoint(0, 0);
            _highlightedPoint.Color = Color.Red;
            _highlightedPoint.MarkerSize = 10;
            _highlightedPoint.MarkerShape = MarkerShape.openCircle;
            _highlightedPoint.IsVisible = false;
        }

        private void ClusterListBox_OnSelectionChanged(object sender, RoutedEventArgs e)
        {
            if (sender is not ListBox { SelectedItem: ClusterListBoxItem item }) return;

            RerenderGraph(c => c == item.ClusterId);
        }

        private void ResetClusterButton_OnClick(object sender, RoutedEventArgs e)
        {
            ClusterListBox.UnselectAll();

            RerenderGraph();
        }

        public class ClusterListBoxItem
        {
            public string DisplayName { get; }
            public readonly int ClusterId;

            public ClusterListBoxItem(int clusterId)
            {
                DisplayName = clusterId.ToString();
                ClusterId = clusterId;
            }
        }

        public class LogListItem
        {
            public int Index { get; set; }
            public string Content { get; set; }
        }

        private void Graph_OnAxesChanged(object? sender, EventArgs e)
        {
            var limits = Graph.Plot.GetAxisLimits();
            _presenter.SetAxisLimits(limits.XMax, limits.XMin, limits.YMax, limits.YMin);
        }

        private void LogGridCopy_OnClicked(object sender, RoutedEventArgs e)
        {
            if (sender is not Button { Tag: var log and LogListItem }) return;
            if (log is not LogListItem item) return;

            try
            {
                Clipboard.SetData(DataFormats.Text, item.Content);
            }
            catch
            {
                MessageBox.Show("クリップボードへのコピーに失敗しました", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}