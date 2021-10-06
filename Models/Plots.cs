using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace PrettyScatter.Models
{
    public class Plots
    {
        public IImmutableList<Plot> PlotList { get; }

        public Plots(IEnumerable<Plot> plots)
        {
            PlotList = plots.ToImmutableList();
        }

        public static async Task<Plots> FromFile(string path)
        {
            var plots = await Task.Run(() => File
                .ReadAllLines(path)
                .Select(line =>
                {
                    var split = line.Split(",");
                    return new Plot
                    {
                        x = double.Parse(split[0]),
                        y = double.Parse(split[1]),
                        cluster = int.Parse(split[2]),
                    };
                })
                .ToList()
            );

            return new Plots(plots);
        }
    }

    public struct Plot
    {
        public double x { get; set; }
        public double y { get; set; }
        public int cluster { get; set; }
    }
}