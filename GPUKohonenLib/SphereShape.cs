using System;
using System.Collections.Generic;
using System.Text;

namespace GPUKohonenLib
{
    public class SphereShape : IMapShape
    {
        public SphereShape(float Radius, int XRes, int YRes):base()
        {

        }

        public override int GetFlatLength()
        {
            throw new NotImplementedException();
        }

        public override float[] GetSpatialPosition(int Index)
        {
            throw new NotImplementedException();
        }
    }
}
