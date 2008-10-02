using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GPUKohonenLib
{
    public abstract class IDataSource
    {
        public IDataSource() { }

        public int PatternCount
        {
            get
            {
                return this.GetPatternCount();
            }
            set
            {
            }
        }

        public int PatternLength
        {
            get
            {
                return this.GetPatternLength();
            }
            set
            {
            }
        }
    
        public abstract float[] GetPattern( int Index );
        public abstract int GetPatternLength();
        public abstract int GetPatternCount();
    }
}
