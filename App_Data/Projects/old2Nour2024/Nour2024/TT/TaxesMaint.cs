using System;
using System.Collections;
using System.Collections.Generic;
using PX.SM;
using PX.Data;


namespace Maintenance
{
    public class TaxesMaint : PXGraph<TaxesMaint>
    {
        public PXCancel<Taxes> Cancel;
        public PXSave<Taxes> Save;

        public PXSelect<Taxes> TaxesCommissions;
    }
}