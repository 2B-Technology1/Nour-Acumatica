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
using PX.Objects.AP;
using PX.Objects.CN.Common.Descriptor;
using PX.Objects.CN.ProjectAccounting.Descriptor;
using PX.Objects.CS;
using PX.Objects.GL;
using PX.Objects.IN;
using PX.Objects.PM;

namespace PX.Objects.CN.ProjectAccounting.AP.CacheExtensions
{
    public sealed class VendorExt : PXCacheExtension<Vendor>
    {
        [PXDBInt]
        [CostCodeDimensionSelector(null, null, null, null, false)]
        [PXUIField(DisplayName = "Cost Code", FieldClass = CostCodeAttribute.COSTCODE)]
        public int? VendorDefaultCostCodeId
        {
            get;
            set;
        }

        [PXDBInt]
        [PXDimensionSelector(InventoryAttribute.DimensionName, typeof(Search2<InventoryItem.inventoryID,
            InnerJoin<Account, On<Account.accountID, Equal<InventoryItem.cOGSAcctID>>,
            InnerJoin<PMAccountGroup, On<PMAccountGroup.groupID, Equal<Account.accountGroupID>>>>,
            Where<PMAccountGroup.type, Equal<AccountType.expense>,
                And<InventoryItem.stkItem, Equal<False>>>>), typeof(InventoryItem.inventoryCD))]
        [PXUIField(DisplayName = BusinessMessages.InventoryID, FieldClass = nameof(FeaturesSet.Construction))]       
        public int? VendorDefaultInventoryId
        {
            get;
            set;
        }

        public static bool IsActive()
        {
            return PXAccess.FeatureInstalled<FeaturesSet.construction>() || PXAccess.FeatureInstalled<FeaturesSet.costCodes>();
        }

        public abstract class vendorDefaultInventoryId : IBqlField
        {
        }

        public abstract class vendorDefaultCostCodeId : IBqlField
        {
        }
    }
}
