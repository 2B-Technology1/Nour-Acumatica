using System;
using System.Collections;
using System.Collections.Generic;
using PX.SM;
using PX.Data;


namespace Maintenance
{
    public class CheckNoEntry : PXGraph<CheckNoEntry>
    {
        public PXCancel<CheckNo> Cancel;
        public PXSave<CheckNo> Save;

        public PXSelect<CheckNo> check;
    }
}