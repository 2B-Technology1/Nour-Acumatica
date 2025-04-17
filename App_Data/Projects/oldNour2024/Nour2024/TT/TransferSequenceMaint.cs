using System;
using System.Collections;
using System.Collections.Generic;
using PX.SM;
using PX.Data;


namespace PX.Objects.IN
{
    public class TransferSequenceMaint : PXGraph<TransferSequenceMaint>
    {
        public PXCancel<TransferSequence> Cancel;
        public PXSave<TransferSequence> Save;

        public PXSelect<TransferSequence> sequences;
    }
}