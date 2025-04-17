using System;
using System.Collections;
using System.Collections.Generic;
using PX.SM;
using PX.Data;


namespace Maintenance
{
    public class BRBankMaint : PXGraph<BRBankMaint>
    {
        public PXCancel<BRBank> Cancel;
        public PXSave<BRBank> Save;

        public PXSelect<BRBank> brBanks;
    }
}