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

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PX.Data;
using PX.Objects.AP;
using PX.Objects.CN.Common.Extensions;
using PX.Objects.CN.ProjectAccounting.AP.CacheExtensions;
using PX.Objects.CS;
using PX.Objects.PM;

namespace PX.Objects.CN.ProjectAccounting.PM.GraphExtensions
{
    public class CostCodeMaintExt : PXGraphExtension<CostCodeMaint>
    {
        public static bool IsActive()
        {
            return PXAccess.FeatureInstalled<FeaturesSet.construction>();
        }

        protected void _(Events.RowPersisting<PMCostCode> args)
        {
            if (args.Cache != null)
            {
                UpdateVendorsIfRequired(args.Cache.Deleted);
            }
        }

        private void UpdateVendorsIfRequired(IEnumerable costCodes)
        {
            var costCodeIds = costCodes.Cast<PMCostCode>().Select(c => c.CostCodeID).ToArray();
            if (costCodeIds.Any())
            {
                var vendors = GetVendors(costCodeIds).ToList();
                var cache = Base.Caches<Vendor>();
                vendors.ForEach(UpdateVendorDefaultCostCodeId);
                cache.UpdateAll(vendors);
                cache.Persist(PXDBOperation.Update);
            }
        }

        private void UpdateVendorDefaultCostCodeId(Vendor vendor)
        {
            var vendorExtension = PXCache<Vendor>.GetExtension<VendorExt>(vendor);
            vendorExtension.VendorDefaultCostCodeId = null;
        }

        private IEnumerable<Vendor> GetVendors(int?[] costCodeIds)
        {
            return new PXSelect<Vendor,
                    Where<VendorExt.vendorDefaultCostCodeId,
                        In<Required<VendorExt.vendorDefaultCostCodeId>>>>(Base)
                .Select(costCodeIds).FirstTableItems;
        }
    }
}