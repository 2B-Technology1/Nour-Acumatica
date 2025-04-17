using System;
using System.Collections;
using System.Collections.Generic;
using PX.SM;
using PX.Data;



namespace Maintenance
{
    public class CustomerActivityMaint : PXGraph<CustomerActivityMaint>
    {
        public PXCancel<CustomerActivity> Cancel;
        public PXSave<CustomerActivity> Save;

        public PXSelect<CustomerActivity> Activities;
    }
}