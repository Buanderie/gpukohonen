using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GPUKohonenLib
{
    public abstract class IDataSource
    {
        public IDataSource() { }
        public abstract float[] GetPattern( int Index );
        public abstract int GetPatternLength();
    }
}
