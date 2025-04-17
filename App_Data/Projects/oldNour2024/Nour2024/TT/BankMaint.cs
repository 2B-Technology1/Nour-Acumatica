using System;
using System.Collections;
using System.Collections.Generic;
using PX.SM;
using PX.Data;


namespace Maintenance
{
    public class BankMaint : PXGraph<BankMaint>
    {
        public PXCancel<Banks> Cancel;
        public PXSave<Banks> Save;

        public PXSelect<Banks> banks;
        //public PXSelect<Banks> banks;
    }
}