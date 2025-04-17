using System;
using System.Collections;
using System.Collections.Generic;
using PX.SM;
using PX.Data;


namespace Maintenance.CO
{
    public class CentersEntry : PXGraph<CentersEntry>
    {
        public PXSave<CompensationCenters> Save;
        public PXCancel<CompensationCenters> Cancel;
        public PXSelect<CompensationCenters> compensationCenters;
    }
}