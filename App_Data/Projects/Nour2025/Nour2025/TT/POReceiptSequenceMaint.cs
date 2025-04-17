using System;
using System.Collections;
using System.Collections.Generic;
using PX.SM;
using PX.Data;


namespace PX.Objects.PO
{
    public class POReceiptSequenceMaint : PXGraph<POReceiptSequenceMaint>
    {
        public PXCancel<POReceiptSequence> Cancel;
        public PXSave<POReceiptSequence> Save;

        public PXSelect<POReceiptSequence> sequences;
    }
}