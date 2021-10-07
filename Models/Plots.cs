using System;
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

        public static async Task<Plots?> FromFile(string path)
        {
            try
            {
                var plots = await Task.Run(() => File
                    .ReadAllLines(path)
                    .Select(line =>
                    {
                        var split = line.Split(",");
                        return new Plot
                        {
                            X = double.Parse(split[0]),
                            Y = double.Parse(split[1]),
                            Cluster = int.Parse(split[2]),
                        };
                    })
                    .ToList()
                );

                return new Plots(plots);
            }
            catch (Exception)
            {
                return null;
            }
        }
    }

    public struct Plot
    {
        public double X { get; set; }
        public double Y { get; set; }
        public int Cluster { get; set; }
    }
}