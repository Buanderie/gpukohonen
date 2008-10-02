using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace GPUKohonenLib
{
    public class TimeSerieDataSource : IDataSource
    {
        private List<List<float>> m_patterns;
        private int m_chunksize;

        public TimeSerieDataSource( String File, int ChunkSize )
        {
            m_patterns = new List<List<float>>();
            m_chunksize = ChunkSize;

            System.IO.FileStream nf = new FileStream(File, FileMode.Open);
            System.IO.StreamReader sr = new StreamReader(nf);
            char c;
            String str = ""; 
            List<float> numbers = new List<float>();
            while (!sr.EndOfStream)
            {
                c = (char)(sr.Read());
                if (System.Char.IsWhiteSpace(c) && str != "")
                {
                    numbers.Add((float)(System.Convert.ToDecimal(str)));
                    str = "";
                }
                else
                    str += c;
            }

            //Chunking lol
            while (numbers.Count > ChunkSize)
            {
                List<float> tmp = new List<float>();
                for( int i = 0; i < ChunkSize; ++i )
                {
                    tmp.Add(numbers[0]);
                    numbers.RemoveAt(0);
                }
                m_patterns.Add(tmp);
            }
            //
        }

        public override float[] GetPattern(int Index)
        {
            return m_patterns[Index].ToArray();
        }

        public override int GetPatternLength()
        {
            return m_chunksize;
        }

        public override int GetPatternCount()
        {
            return m_patterns.Count;
        }
    }
}
