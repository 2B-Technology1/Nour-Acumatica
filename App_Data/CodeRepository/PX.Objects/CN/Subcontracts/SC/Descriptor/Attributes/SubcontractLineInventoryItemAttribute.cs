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

using PX.Data;
using PX.Objects.CN.Common.Descriptor;
using PX.Objects.CN.Subcontracts.SC.DAC;
using PX.Objects.IN;
using System;

namespace PX.Objects.CN.Subcontracts.SC.Descriptor.Attributes
{
    [Obsolete(Objects.Common.InternalMessages.ClassIsObsoleteAndWillBeRemoved2021R1)]
    [PXDBInt]
    [PXUIField(DisplayName = BusinessMessages.InventoryID, Visibility = PXUIVisibility.Visible)]
    [PXRestrictor(typeof(Where<InventoryItem.itemStatus, NotEqual<InventoryItemStatus.noPurchases>>),
        Messages.SubcontractLineInventoryItemAttribute.LineItemNotPurchased)]
    [PXRestrictor(typeof(Where<InventoryItem.itemStatus, NotEqual<InventoryItemStatus.unknown>>),
        Messages.SubcontractLineInventoryItemAttribute.LineItemReserved)]
    public class SubcontractLineInventoryItemAttribute : CrossItemAttribute
    {
        public SubcontractLineInventoryItemAttribute()
            : base(typeof(Search<SubcontractInventoryItem.inventoryID,
                    Where<Match<Current<AccessInfo.userName>>>>),
                typeof(SubcontractInventoryItem.inventoryCD),
                typeof(SubcontractInventoryItem.descr), INPrimaryAlternateType.VPN)
        {
        }
    }
}