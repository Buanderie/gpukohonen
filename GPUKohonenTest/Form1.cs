﻿using System;
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
        private KohonenMap lol_gpu;
        private KohonenMap lol_cpu;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            float[] pol = new float[500];
            lol_gpu = new KohonenMap(50, 50, 500, KohonenMap.GPUK_COMPUTATION_TYPE.GPU_COMPUTATION_TYPE);
            lol_cpu = new KohonenMap(50, 50, 500, KohonenMap.GPUK_COMPUTATION_TYPE.CPU_COMPUTATION_TYPE);

            lol_gpu.SetCurrentPattern(pol);
            lol_cpu.SetCurrentPattern(pol);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            double total = 0;
            System.Diagnostics.Stopwatch perf = new System.Diagnostics.Stopwatch();
            perf.Start();
            lol_gpu.FindBMU();
            perf.Stop();
            textBox1.Text += "FindBMU : " + perf.ElapsedMilliseconds.ToString() + "\r\n";
            total += perf.ElapsedMilliseconds;

            perf.Reset();

            perf.Start();
            lol_gpu.DoEpoch(10);
            pictureBox2.Image = lol_gpu.GetBitmap();
            perf.Stop();
            textBox1.Text += "Epoch : " + perf.ElapsedMilliseconds.ToString() + "\r\n";
            total += perf.ElapsedMilliseconds;
            textBox1.Text += "Total : " + total.ToString();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            double total = 0;
            System.Diagnostics.Stopwatch perf = new System.Diagnostics.Stopwatch();
            perf.Start();
            lol_cpu._CPU_FindBMU();
            pictureBox1.Image = lol_cpu._CPU_GetBitmap();
            perf.Stop();
            textBox2.Text += "FindBMU : " + perf.ElapsedMilliseconds.ToString() + "\r\n";
            total += perf.ElapsedMilliseconds;

            perf.Reset();

            perf.Start();
            lol_gpu._CPU_DoEpoch(10);
            pictureBox1.Image = lol_gpu._CPU_GetBitmap();
            perf.Stop();
            textBox2.Text += "Epoch : " + perf.ElapsedMilliseconds.ToString() + "\r\n";
            total += perf.ElapsedMilliseconds;
            textBox2.Text += "Total : " + total.ToString();
        }
    }
}
