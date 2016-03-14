using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Utils;

namespace PutCallAnalyser
{
    class Program
    {
        static void Main(string[] args)
        {
            //WebClient Client = new WebClient();
            //Client.DownloadFile("http://www.cboe.com/publish/scheduledtask/mktdata/datahouse/indexpc.csv", @"indexpc.csv");
            //Client.DownloadFile("http://www.cboe.com/publish/scheduledtask/mktdata/datahouse/equitypc.csv", @"equitypc.csv");
            //Client.DownloadFile("http://www.cboe.com/publish/scheduledtask/mktdata/datahouse/totalpc.csv", @"totalpc.csv");

            var totalRatioData = ParseCsv("totalpc.csv");
            var indexRatioData = ParseCsv("indexpc.csv");
            var equityRatioData = ParseCsv("equitypc.csv");
            //var vixRatioData = ParseCsv("../../../vixpc.csv");

            var numberOfYearsOfData = 10;
            var totalRatioDataFiltrated = totalRatioData.Where(x => x.Date > DateTime.Now.AddYears(-numberOfYearsOfData)).ToList();
            var indexRatioDataFiltrated = indexRatioData.Where(x => x.Date > DateTime.Now.AddYears(-numberOfYearsOfData)).ToList();
            var equityRatioDataFiltrated = equityRatioData.Where(x => x.Date > DateTime.Now.AddYears(-numberOfYearsOfData)).ToList();

            //var espilon = 0.2;
            //var totalRatioDataTreated = RdpCurveTreatment(totalRatioDataFiltrated, espilon);
            //var indexRatioDataTreated = RdpCurveTreatment(indexRatioDataFiltrated, espilon);
            //var equityRatioDataTreated = RdpCurveTreatment(equityRatioDataFiltrated, espilon);

            var averageWindows = 20;
            var totalRatioDataAveraged = AverageCurveTreatment(totalRatioDataFiltrated, averageWindows);
            var indexRatioDataAveraged = AverageCurveTreatment(indexRatioDataFiltrated, averageWindows);
            var equityRatioDataAveraged = AverageCurveTreatment(equityRatioDataFiltrated, averageWindows);

            var averageWindows2 = 50;
            var totalRatioDataAveraged2 = AverageCurveTreatment(totalRatioDataFiltrated, averageWindows2);
            var indexRatioDataAveraged2 = AverageCurveTreatment(indexRatioDataFiltrated, averageWindows2);
            var equityRatioDataAveraged2 = AverageCurveTreatment(equityRatioDataFiltrated, averageWindows2);

            var indexMinusEquity = indexRatioDataAveraged2.Where(x => equityRatioDataAveraged2.Select(y => y.Date).Contains(x.Date)).Select(x => new Row() {Date = x.Date, Ratio = (x.Ratio + equityRatioDataAveraged2.First(y => y.Date == x.Date).Ratio) /2}).ToList();

            //var equityRatioDataTreated02 = AverageCurveTreatment(equityRatioDataFiltrated, 2);
            //var equityRatioDataTreated10 = AverageCurveTreatment(equityRatioDataFiltrated, 10);
            //var equityRatioDataTreated50 = AverageCurveTreatment(equityRatioDataFiltrated, 50);
            
            var cboeData = new List<Tuple<string, IEnumerable<Row>>>
            {
                
                //{"Total Ratio Data Averaged", totalRatioDataTreated2, System.Drawing.Color.Green},
                {"Index Ratio Data Averaged "+averageWindows, indexRatioDataAveraged},
                {"Equity Ratio Data Averaged "+averageWindows, equityRatioDataAveraged},

                {"Index Ratio Data Averaged "+averageWindows2, indexRatioDataAveraged2},
                {"Equity Ratio Data Averaged "+averageWindows2, equityRatioDataAveraged2},
                {"Total Ratio Data "+averageWindows2, totalRatioDataAveraged2},

                {"Index Minus Equity "+averageWindows2, indexMinusEquity},
                //{"Equity Ratio Data Averaged 2", equityRatioDataTreated02},
                //{"Equity Ratio Data Averaged 10", equityRatioDataTreated10},
                //{"Equity Ratio Data Averaged 50", equityRatioDataTreated50},
            };
            // "VIX Ratio Data", vixRatioData


            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new ChartForm(cboeData));
        }

        private static List<Row> AverageCurveTreatment(IEnumerable<Row> inputCurveIE, int windows)
        {
            var inputCurve = inputCurveIE.ToList();

            var result = new List<Row>();
            for (int i = 0; i < inputCurve.Count-windows; i++)
            {
                var average = inputCurve.Skip(i).Take(windows).Sum(x => x.Ratio) / windows;
                var Row = new Row() {Ratio = average, Date = inputCurve.ElementAt(i + windows).Date};
                result.Add(Row);
            }
            return result;
        }

        private static List<Row> RdpCurveTreatment(IEnumerable<Row> inputCurveIE, double epsilon)
        {
            var inputCurve = inputCurveIE.ToList();

            if (inputCurve.Count > 2)
            {
                // Find the point with the maximum distance
                var curveWithDistance = inputCurve.Select(x => new Tuple<Row, double>(x, PerpendicularDistance(x, inputCurve.First(), inputCurve.Last()))).ToList();

                var pointWithDistance = curveWithDistance.First(x => x.Item2 == curveWithDistance.Max(y => y.Item2));

                if (pointWithDistance.Item2 > epsilon)
                {
                    // Recursive calls
                    var part1 = RdpCurveTreatment(inputCurve.Take(inputCurve.IndexOf(pointWithDistance.Item1)).ToList(), epsilon);
                    var part2 = RdpCurveTreatment(inputCurve.Skip(inputCurve.IndexOf(pointWithDistance.Item1)).ToList(), epsilon);

                    // Build the result list
                    return part1.AddTo(part2).ToList();
                }
                return new List<Row> { inputCurve.First(), inputCurve.Last() };
            }
            return inputCurve;
        }

        private static double PerpendicularDistance(Row point, Row point1, Row point2)
        {
            Double area = Math.Abs(.5 * (point1.Date.Ticks * point2.Ratio + point2.Date.Ticks *
                point.Ratio + point.Date.Ticks * point1.Ratio - point2.Date.Ticks * point1.Ratio - point.Date.Ticks *
                point2.Ratio - point1.Date.Ticks * point.Ratio));
            Double bottom = Math.Sqrt(Math.Pow(point1.Date.Ticks - point2.Date.Ticks, 2) +
                Math.Pow(point1.Ratio - point2.Ratio, 2));
            Double height = area / bottom * 2;

            return height;
        }

        private static List<Row> ParseCsv(string fileUrl)
        {
            List<Row> result = new List<Row>();
            var lines = File.ReadAllLines(fileUrl);
            foreach (var line in lines)
            {
                try
                {
                    var split = line.Split(',');
                    var date = DateTime.ParseExact(split[0], "M/d/yyyy", CultureInfo.InvariantCulture);
                    var Calls = Int32.Parse(split[1]);
                    var Puts = Int32.Parse(split[2]);
                    var Total = Int32.Parse(split[3]);
                    var Ratio = Double.Parse(split[4]);

                    result.Add(new Row
                    {
                        Date = date,
                        Ratio = Ratio,
                        Calls = Calls,
                        Puts = Puts,
                        Total = Total
                    });
                }
                catch (Exception e)
                {
                    Console.Error.WriteLine("Cannot parse row: "+ line);
                }
            }
            return result;
        }
    }

    public class Row
    {
        public DateTime Date;
        public int Calls;
        public int Puts;
        public int Total;
        public double Ratio;
    }
}
