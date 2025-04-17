using System;
using System.Collections;
using System.Collections.Generic;
using PX.SM;
using PX.Data;


namespace Maintenance
{
    public class VendorClass2Maint : PXGraph<VendorClass2Maint>
    {
        public PXCancel<VendorClass2> Cancel;
        public PXSave<VendorClass2> Save;

        public PXSelect<VendorClass2> vendorClasses2;
    }
}