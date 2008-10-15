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

            Stack<float> stack = new Stack<float>();

            float[] test = new float[500];
            /*float[] kkkk = new float[10];
            for(int j = 0; j < 10; ++j )
                kkkk[j] = (float)j;
            DisposableFloatParallelArray kp = new DisposableFloatParallelArray(kkkk);
            FloatParallelArray kpk = ParallelArrays.Replicate(kp, kp.Shape[0] - 1);
            ParallelArrays.ToArray(kpk, out test);
            */

            ParallelStack stk = new ParallelStack();

            System.Diagnostics.Stopwatch perf = new System.Diagnostics.Stopwatch();

            //GPU
            for (float i = 0.0f; i < 800.0f; i=i+1.0f)
                stk.Push(i);
            for (int i = 0; i < 699; ++i)
                stk.Pop();
            //

            //CPU
            perf.Start();
            for (float i = 0.0f; i < 800.0f; i = i + 1.0f)
                stack.Push(i);
            for (int i = 0; i < 699; ++i)
                stack.Pop();
            perf.Stop();
            label6.Text = perf.ElapsedMilliseconds.ToString();
            //

            perf.Reset();

            perf.Start();
            ParallelArrays.ToArray(stk.GetStackArray(), out test);
            perf.Stop();
            label5.Text = perf.ElapsedMilliseconds.ToString();
            
            int popopo = 45;
        }
    }
}
