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
using PX.Objects.IN;
using System;

namespace PX.Objects.CN.Subcontracts.SC.DAC
{
    /// <summary>
    /// This class is required as Acumatica use Filters with SELECTOR type across all system.
    /// We needed to override ViewName of the <see cref="T:PX.Data.FilterHeader" /> to specific entity type.
    /// </summary>
    /// 
    [Obsolete(Objects.Common.InternalMessages.ClassIsObsoleteAndWillBeRemoved2021R1)]
    [PXCacheName("Subcontract Inventory Item")]
    public class SubcontractInventoryItem : InventoryItem
    {
        public new abstract class inventoryID : IBqlField
        {
        }

        public new abstract class inventoryCD : IBqlField
        {
        }

        public new abstract class descr : IBqlField
        {
        }
    }
}
