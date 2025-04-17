using System;
using System.Collections;
using System.Collections.Generic;
using PX.SM;
using PX.Data;


namespace Maintenance
{
    public class AccountClass2Maint : PXGraph<AccountClass2Maint>
    {
        public PXCancel<AccountClass2> Cancel;
        public PXSave<AccountClass2> Save;

        public PXSelect<AccountClass2> accountClasses2;
    }
}