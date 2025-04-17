using System;
using System.Collections;
using System.Collections.Generic;
using PX.SM;
using PX.Data;


namespace PX.Objects.IN
{
    public class InventoryReceiptSequenceMaint : PXGraph<InventoryReceiptSequenceMaint>
    {
        public PXCancel<InventoryReceiptSequence> Cancel;
        public PXSave<InventoryReceiptSequence> Save;

        public PXSelect<InventoryReceiptSequence> sequences;
    }
}