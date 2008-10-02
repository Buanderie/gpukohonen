using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace GPUKohonenLib
{
    public class ColorFromTextDataSource : IDataSource
    {
        private String m_Directory;
        private List<System.Drawing.Color> m_Colors;

        public ColorFromTextDataSource(String Dir)
            : base()
        {
            m_Colors = new List<System.Drawing.Color>();

            m_Directory = Dir;
            String [] Files = Directory.GetFiles( m_Directory );
            foreach( String f in Files )
            {
                int r, g, b;
                String tmp;
                StreamReader reader = new StreamReader(f);
                tmp = reader.ReadLine();
                r = System.Convert.ToInt16(tmp);
                tmp = reader.ReadLine();
                g = System.Convert.ToInt16(tmp);
                tmp = reader.ReadLine();
                b = System.Convert.ToInt16(tmp);
                m_Colors.Add( System.Drawing.Color.FromArgb( r, g, b ) );
            }
        }

        public override float[] GetPattern( int Index )
        {
            /*if (Index > m_Colors.Count)
                throw new Exception(this.ToString + " !! " + "Pattern index out of bound");
            */
            float[] pattern = new float[3];
            pattern[0] = ((float)(m_Colors[Index].R));///((float)(255));
            pattern[1] = ((float)(m_Colors[Index].G));// / ((float)(255));
            pattern[2] = ((float)(m_Colors[Index].B));// / ((float)(255));
            return pattern;
        }

        public override int GetPatternLength()
        {
            return 3;
        }

        public override int GetPatternCount()
        {
            return this.m_Colors.Count;
        }
    }
}
