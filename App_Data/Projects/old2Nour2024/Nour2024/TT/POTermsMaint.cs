using System;
using System.Collections;
using System.Collections.Generic;
using PX.SM;
using PX.Data;


namespace Maintenance
{
    public class POTermsMaint : PXGraph<POTermsMaint>
    {
        public PXCancel<POTerms> Cancel;
        public PXSave<POTerms> Save;

        public PXSelect<POTerms> Terms;
    }
}