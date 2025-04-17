using System;
using System.Collections;
using System.Collections.Generic;
using PX.SM;
using PX.Data;


namespace Maintenance
{
    public class LegalFormMaint : PXGraph<LegalFormMaint>
    {
        public PXCancel<LegalForm> Cancel;
        public PXSave<LegalForm> Save;

        public PXSelect<LegalForm> Records;
    }
}