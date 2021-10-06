using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using PrettyScatter.Models;
using PrettyScatter.Utils.Ext;

namespace PrettyScatter
{
    public partial class MainWindow : Window
    {
        private Log? log = null;

        public MainWindow()
        {
            InitializeComponent();

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

            double[] dataX = { 1, 2, 3, 4, 5 };
            double[] dataY = { 1, 4, 9, 16, 25 };
            SamplePlot.Plot.AddScatter(dataX, dataY);
            SamplePlot.Refresh();
        }

        private async void OnDropFiles(string[] paths)
        {
            double[] dataX = { 30, 50, 0, 640, 501 };
            double[] dataY = { 0, 3, 0, 305, 20 };

            log = await Log.FromFile(paths[0]);
            {
                LogList.ItemsSource = new ObservableCollection<LogListItem>(log.LogTextList.Select((elem, idx) =>
                    new LogListItem
                    {
                        Index = idx,
                        Content = elem,
                    }));
            }

            SamplePlot.Plot.RemoveAt(0);
            SamplePlot.Plot.AddScatter(dataX, dataY);
            SamplePlot.Refresh();
        }

        public struct LogListItem
        {
            public int Index { get; set; }
            public string Content { get; set; }
        }
    }
}