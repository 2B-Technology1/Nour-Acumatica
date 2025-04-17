using System;
using System.Collections;
using System.Collections.Generic;
using PX.SM;
using PX.Data;
using PX.Objects.IN;


namespace Maintenance.CO
{
    public class COSetUpMaint : PXGraph<COSetUpMaint>
    {
        public PXSave<COSetUp> Save;
        public PXCancel<COSetUp> Cancel;
        public PXSelect<COSetUp> codSetUp;
    }
}