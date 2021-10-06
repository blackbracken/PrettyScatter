using System.Windows;
using PrettyScatter.Utils.Ext;

namespace PrettyScatter
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

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

        private void OnDropFiles(string[] paths)
        {
            double[] dataX = { 30, 50, 0, 640, 501 };
            double[] dataY = { 0, 3, 0, 305, 20 };

            SamplePlot.Plot.RemoveAt(0);
            SamplePlot.Plot.AddScatter(dataX, dataY);
            SamplePlot.Refresh();
        }
    }
}
