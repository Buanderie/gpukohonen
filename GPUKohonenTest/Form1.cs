using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using GPUKohonenLib;

namespace GPUKohonenTest
{
    public partial class Form1 : Form
    {
        KohonenSOM somcpu;
        KohonenSOM somgpu;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

            //New version
            IDataSource ds = new ColorFromTextDataSource("colors");
            IDataSource ds2 = new TimeSerieDataSource("stocks.txt", 6);
            int[] mapsize = new int [2];
            mapsize[0] = 10;
            mapsize[1] = 10;
            IMapShape ms = new SquareShape(mapsize);
            IKohonenCore kcgpu = new MSRAcceleratorKohonenCore();
            IKohonenCore kccpu = new CPUKohonenCore();
            somcpu = new KohonenSOM(kccpu, ms, ds);
            somgpu = new KohonenSOM(kcgpu, ms, ds);
            somcpu.Init();
            somgpu.Init();
            //pictureBox1.Image = som.ToWeightBitmap();
            //
        }

        private void button1_Click(object sender, EventArgs e)
        {
            double total = 0;
            System.Diagnostics.Stopwatch perf = new System.Diagnostics.Stopwatch();
            perf.Start();
            somgpu.DoRound(1);
            perf.Stop();
            textBox1.Text += "FindBMU : " + perf.ElapsedMilliseconds.ToString() + "\r\n";
        }

        private void button2_Click(object sender, EventArgs e)
        {
            double total = 0;
            System.Diagnostics.Stopwatch perf = new System.Diagnostics.Stopwatch();
            perf.Start();
            somcpu.DoRound(1);
            perf.Stop();
            textBox2.Text += "FindBMU : " + perf.ElapsedMilliseconds.ToString() + "\r\n";
        }
    }
}
