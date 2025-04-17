using System;
using System.Collections;
using System.Collections.Generic;
using PX.SM;
using PX.Data;


namespace MyMaintaince.LM
{
    public class LMTSetUpMaint : PXGraph<LMTSetUpMaint>
    {
        public PXSave<LMTSetup> Save;
        public PXCancel<LMTSetup> Cancel;
        public PXSelect<LMTSetup> codSetUp;
    }
}