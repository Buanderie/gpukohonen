using System;
using System.Collections.Generic;
using System.Text;

namespace GPUKohonenLib
{
    public class MSRAcceleratorKohonenCore : IKohonenCore
    {
        //Constructor
        public MSRAcceleratorKohonenCore():base()
        {

        }

        public override void Init(KohonenSOM parent)
        {
            this.m_Parent = parent;

        }

        public override void FindBMU(float[] Pattern)
        {
            throw new NotImplementedException();
        }

        public override void DoEpoch(float t, float round_t)
        {
            throw new NotImplementedException();
        }
    }
}
