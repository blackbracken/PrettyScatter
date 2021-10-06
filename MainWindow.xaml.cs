using System;
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

            double[] dataX = new double[] { 1, 2, 3, 4, 5 };
            double[] dataY = new double[] { 1, 4, 9, 16, 25 };
            SamplePlot.Plot.AddScatter(dataX, dataY);
            SamplePlot.Refresh();
        }

        private async void OnDropFiles(string[] paths)
        {
            double[] dataX = { 30, 50, 0, 640, 501 };
            double[] dataY = { 0, 3, 0, 305, 20 };

            log = await Log.FromFile(paths[0]);
            Debug.Print(log.LogTextList[10000]);

            SamplePlot.Plot.RemoveAt(0);
            SamplePlot.Plot.AddScatter(dataX, dataY);
            SamplePlot.Refresh();
        }
    }
}