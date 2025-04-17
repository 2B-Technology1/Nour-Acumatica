/* ---------------------------------------------------------------------*
*                             Acumatica Inc.                            *

*              Copyright (c) 2005-2023 All rights reserved.             *

*                                                                       *

*                                                                       *

* This file and its contents are protected by United States and         *

* International copyright laws.  Unauthorized reproduction and/or       *

* distribution of all or any portion of the code contained herein       *

* is strictly prohibited and will result in severe civil and criminal   *

* penalties.  Any violations of this copyright will be prosecuted       *

* to the fullest extent possible under law.                             *

*                                                                       *

* UNDER NO CIRCUMSTANCES MAY THE SOURCE CODE BE USED IN WHOLE OR IN     *

* PART, AS THE BASIS FOR CREATING A PRODUCT THAT PROVIDES THE SAME, OR  *

* SUBSTANTIALLY THE SAME, FUNCTIONALITY AS ANY ACUMATICA PRODUCT.       *

*                                                                       *

* THIS COPYRIGHT NOTICE MAY NOT BE REMOVED FROM THIS FILE.              *

* --------------------------------------------------------------------- */

using System;
using PX.Data;
using PX.Objects.IN;

namespace PX.Objects.AM.Attributes
{
    /// <summary>
    /// Production Material lot/serial number attribute
    /// </summary>
    public class AMProdMatlLotSerialNbrAttribute : INLotSerialNbrAttribute
    {
        public AMProdMatlLotSerialNbrAttribute(Type InventoryType, Type SubItemType, Type LocationType) : base(InventoryType, SubItemType, LocationType, typeof(CostCenter.freeStock))
        {
        }

        public AMProdMatlLotSerialNbrAttribute(Type InventoryType, Type SubItemType, Type LocationType, Type ParentLotSerialNbrType) : base(InventoryType, SubItemType, LocationType, ParentLotSerialNbrType, typeof(CostCenter.freeStock))
        {
        }

        public override void FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
        {
            AMProdMatlSplit split = e.Row as AMProdMatlSplit;

            if (split != null)
            {
                bool isAllocated = split.IsAllocated == true;

                PXResult<InventoryItem, INLotSerClass> item = this.ReadInventoryItem(sender, ((ILSMaster)e.Row).InventoryID);
                ((PXUIFieldAttribute)_Attributes[_UIAttrIndex]).Enabled = isAllocated;
            }
        }
    }
}

