using System;
using System.Collections;
using System.Collections.Generic;
using PX.SM;
using PX.Data;


namespace PX.Objects.SO
{
    public class ShipmentSequenceMaint : PXGraph<ShipmentSequenceMaint>
    {
        public PXCancel<ShipmentSequence> Cancel;
        public PXSave<ShipmentSequence> Save;

        public PXSelect<ShipmentSequence> sequences;
    }
}