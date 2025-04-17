using System;
using System.Collections;
using System.Collections.Generic;
using PX.SM;
using PX.Data;


namespace MyMaintaince.LM
{
    public class LMOSetUpMaint : PXGraph<LMOSetUpMaint>
    {
        public PXSave<LMOSetup> Save;
        public PXCancel<LMOSetup> Cancel;
        public PXSelect<LMOSetup> codSetUp;
    }
}