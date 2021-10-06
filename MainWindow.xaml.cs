using System.Windows;

namespace PrettyScatter
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            double[] dataX = new double[] { 1, 2, 3, 4, 5 };
            double[] dataY = new double[] { 1, 4, 9, 16, 25 };
            SamplePlot.Plot.AddScatter(dataX, dataY);
            SamplePlot.Refresh();
        }
    }
}
