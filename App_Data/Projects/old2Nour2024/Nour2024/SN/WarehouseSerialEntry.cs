using System;
using System.Collections;
using System.Collections.Generic;
using PX.SM;
using PX.Data;


namespace MyMaintaince.SN
{
    public class WarehouseSerialEntry : PXGraph<WarehouseSerialEntry>
    {
        public PXSave<InventoryWarehouseSerials> Save;
        public PXCancel<InventoryWarehouseSerials> Cancel;
        public PXSelect<InventoryWarehouseSerials> warehouseSerial;
    }
}