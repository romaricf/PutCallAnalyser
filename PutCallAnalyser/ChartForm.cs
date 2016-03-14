using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using Utils;

namespace PutCallAnalyser
{
    public partial class ChartForm : Form
    {
        private IEnumerable<Tuple<string, IEnumerable<Row>>> cboeData;

        //public ChartForm()
        //{
        //    InitializeComponent();
        //}

        public ChartForm(IEnumerable<Tuple<string, IEnumerable<Row>>> cboeData)
        {
            this.Load += new System.EventHandler(this.Form1_Load);
            this.cboeData = cboeData;
            InitializeComponent();
            chart1.Palette = ChartColorPalette.Bright;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            chart1.Series.Clear();
            
            cboeData.ForEach(keyValuePair =>
            {
                var series1 = new System.Windows.Forms.DataVisualization.Charting.Series
                {
                    Name = keyValuePair.Item1,
                    //Color = keyValuePair.Item3,
                    IsVisibleInLegend = true,
                    IsXValueIndexed = false,
                    ChartType = SeriesChartType.Line,
                };
                this.chart1.Series.Add(series1);

                foreach (var row in keyValuePair.Item2)
                {
                    series1.Points.AddXY(row.Date, row.Ratio);
                }
            });
            chart1.Invalidate();
        }

        private void ChartForm_Load(object sender, EventArgs e)
        {

        }
    }
}
