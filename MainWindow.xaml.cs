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
        private ScatterPlot _highlightedPoint;
        private ScatterPlot? _myScatterPlot;
        private int _lastHighlightedIndex;
        private bool _mouseInScatterPlot;

        private Log? _log;
        private Plots? _plots;

        public MainWindow()
        {
            InitializeComponent();

            Title = "PrettyScatter";

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
                LogGrid.AllowDrop = true;
                LogGrid.PreviewDragOver += (_, ev) =>
                {
                    if (ev.IsGottenFile())
                    {
                        ev.Effects = ev.Effects = DragDropEffects.Copy;
                    }

                    ev.Handled = true;
                };
                LogGrid.PreviewDrop += (_, ev) =>
                {
                    if (ev.IsGottenFile())
                    {
                        OnDropLogFiles(ev.GetFilePathsUnsafe());
                    }
                };
            }

            {
                ResetPlot();
                SamplePlot.Configuration.DoubleClickBenchmark = false;

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

                // TODO: ちゃんとはみ出ないように
                var text = $"選択: {pointIndex} / {_log?.LogTextList[pointIndex] ?? "none"}";
                LogText.Text = text.Length < 100 ? text : (new string(text.Take(200).ToArray()) + "...");

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

            // TODO: implement
            Debug.Print($"{row.GetIndex()}");
        }

        private void SamplePlot_OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (!_mouseInScatterPlot) return;

            if (GetPointNearest() is not (_, _, _) nearest) return;
            var (_, _, pointIndex) = nearest;

            if (pointIndex < 0 || LogGrid.Items.Count <= pointIndex)
            {
                MessageBox.Show("プロットに対応するコンテンツがありません", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            object item = LogGrid.Items.GetItemAt(pointIndex);
            LogGrid.SelectionMode = DataGridSelectionMode.Extended;
            LogGrid.SelectedItem = item;
            LogGrid.Focus();
            LogGrid.ScrollIntoView(item);
        }

        private async void OnDropClusterFiles(string[] paths)
        {
            if (_log == null)
            {
                MessageBox.Show("ログデータを先に読み込ませて下さい", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (await Plots.FromFile(paths[0]) is { } plots)
            {
                _plots = plots;
            }
            else
            {
                MessageBox.Show("ファイルの読み込みに失敗しました", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }


            {
                ResetPlot();

                var group = _plots.PlotList.GroupBy(p => p.Cluster);

                _myScatterPlot = SamplePlot.Plot.AddScatterPoints(
                    _plots.PlotList.Select(p => p.X).ToArray(),
                    _plots.PlotList.Select(p => p.Y).ToArray()
                );
                foreach (var g in group)
                {
                    var cluster = g.First().Cluster;
                    var xs = g.Select(p => p.X).ToArray();
                    var ys = g.Select(p => p.Y).ToArray();

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

            LogText.Text = $"データ読み込み: {paths[0]}";
        }

        private async void OnDropLogFiles(string[] paths)
        {
            if (await Log.FromFile(paths[0]) is { } log)
            {
                _log = log;
            }
            else
            {
                MessageBox.Show("ファイルの読み込みに失敗しました", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            {
                LogGrid.ItemsSource = new ObservableCollection<LogListItem>(_log.LogTextList.Select((elem, idx) =>
                    new LogListItem
                    {
                        Index = idx,
                        Content = elem,
                    }));
            }

            LogText.Text = $"データ読み込み: {paths[0]}";
        }

        private void ResetPlot()
        {
            SamplePlot.Plot.Clear();

            _highlightedPoint = SamplePlot.Plot.AddPoint(0, 0);
            _highlightedPoint.Color = Color.Red;
            _highlightedPoint.MarkerSize = 10;
            _highlightedPoint.MarkerShape = MarkerShape.openCircle;
            _highlightedPoint.IsVisible = false;
        }

        public struct LogListItem
        {
            public int Index { get; set; }
            public string Content { get; set; }
        }
    }
}