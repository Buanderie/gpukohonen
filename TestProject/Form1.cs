using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Microsoft.Research.DataParallelArrays;
using AcceleratorUtils;
using System.Collections;

namespace TestProject
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            label3.Text = "Dunno lol";
            label4.Text = ParallelArrays.MaximumArrayDimensions[0].ToString() + "x" + ParallelArrays.MaximumArrayDimensions[1].ToString();

            

            for (int mwidth = 10; mwidth < 4000; mwidth += 10)
            {

                float[] test = new float[mwidth];

                ParallelStack stk = new ParallelStack();
                Stack<float> stack = new Stack<float>();

                System.Diagnostics.Stopwatch perf = new System.Diagnostics.Stopwatch();

                //GPU
                perf.Start();
                    stk.PushN(test);
                    stk.PopN(mwidth);
                perf.Stop();
                label10.Text = perf.ElapsedMilliseconds.ToString();
                textBox2.Text += label10.Text + "\r\n";
                //

                perf.Reset();

                //CPU
                perf.Start();
                    for (float i = 0.0f; i < 1000*mwidth; i = i + 1.0f)
                        stack.Push(i);
                    for (float i = 0.0f; i < 1000*mwidth; i = i + 1.0f)
                        stack.Pop();
                perf.Stop();
                label6.Text = perf.ElapsedMilliseconds.ToString();
                textBox1.Text += label6.Text + "\r\n";
                //

                Application.DoEvents();
            }
            int popopo = 45;
        }
    }
}
