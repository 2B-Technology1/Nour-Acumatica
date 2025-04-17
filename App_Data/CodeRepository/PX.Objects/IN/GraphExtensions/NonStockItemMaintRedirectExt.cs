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
using PX.Objects.Extensions;
using PX.Objects.PO;
using System.Collections;

namespace PX.Objects.IN.GraphExtensions
{
	public class NonStockItemMaintRedirectExt : PXGraphExtension<RedirectExtension<NonStockItemMaint>, NonStockItemMaint>
	{
		private RedirectExtension<NonStockItemMaint> BaseRedirect { get; set; }

		public PXAction<POVendorInventory> viewVendorEmployee;
		[PXUIField(DisplayName = "View Vendor", MapEnableRights = PXCacheRights.Select,
			MapViewRights = PXCacheRights.Select)]
		[PXButton(DisplayOnMainToolbar = false, VisibleOnProcessingResults = false, PopupVisible = false)]
		public virtual IEnumerable ViewVendorEmployee(PXAdapter adapter)
		{
			BaseRedirect = Base.GetExtension<RedirectExtension<NonStockItemMaint>>();
			return BaseRedirect.ViewCustomerVendorEmployee<POVendorInventory.vendorID>(adapter);
		}

		public PXAction<INItemXRef> viewBAccount;
		[PXUIField(DisplayName = "View Vendor", MapEnableRights = PXCacheRights.Select,
			MapViewRights = PXCacheRights.Select)]
		[PXButton(DisplayOnMainToolbar = false, VisibleOnProcessingResults = false, PopupVisible = false)]
		public virtual IEnumerable ViewBAccount(PXAdapter adapter)
		{
			BaseRedirect = Base.GetExtension<RedirectExtension<NonStockItemMaint>>();
			return BaseRedirect.ViewCustomerVendorEmployee<INItemXRef.bAccountID>(adapter);
		}
	}
}
