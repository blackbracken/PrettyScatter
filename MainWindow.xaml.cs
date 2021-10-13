using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using PrettyScatter.Presenters;
using PrettyScatter.Utils.Ext;
using ScottPlot;
using ScottPlot.Plottable;

namespace PrettyScatter
{
    public partial class MainWindow : Window
    {
        private readonly MainPresenter _presenter;

        private ScatterPlot _highlightedPoint;
        private ScatterPlot? _myScatterPlot;
        private int _lastHighlightedIndex;
        private bool _mouseInScatterPlot;

        public MainWindow()
        {
            InitializeComponent();
            _presenter = new MainPresenter(this);

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

        private void Graph_OnMouseHoverPoint(object sender, MouseEventArgs ev)
        {
            if (!_mouseInScatterPlot) return;

            HighlightNearestPlot(sender, ev);
        }

        private void Graph_OnMouseGoInScatterPlot(object sender, MouseEventArgs ev)
        {
            _mouseInScatterPlot = true;

            HighlightNearestPlot(sender, ev, true);
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

            ResetGraph();

            var groups = plots.PlotList.GroupBy(p => p.Cluster);

            _myScatterPlot = Graph.Plot.AddScatterPoints(
                plots.PlotList.Select(p => p.X).ToArray(),
                plots.PlotList.Select(p => p.Y).ToArray()
            );
            foreach (var g in groups)
            {
                var cluster = g.First().Cluster;
                var xs = g.Select(p => p.X).ToArray();
                var ys = g.Select(p => p.Y).ToArray();

                var color = cluster switch
                {
                    0 => Color.OrangeRed,
                    1 => Color.Green,
                    2 => Color.BlueViolet,
                    3 => Color.Blue,
                    4 => Color.Brown,
                    _ => Color.DarkKhaki
                };

                Graph.Plot.AddScatter(xs, ys, lineWidth: 0, color: color);
            }


            Graph.Refresh();
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

            LogGrid.ItemsSource = new ObservableCollection<LogListItem>(log.StartedNewLineLogTextList
                .Select((elem, idx) =>
                    new LogListItem
                    {
                        Index = idx,
                        Content = elem,
                    }
                )
            );
        }

        private void ResetGraph()
        {
            Graph.Plot.Clear();

            _highlightedPoint = Graph.Plot.AddPoint(0, 0);
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