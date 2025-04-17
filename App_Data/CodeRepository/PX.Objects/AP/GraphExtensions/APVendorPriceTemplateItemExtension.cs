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
using System.Linq;
using PX.Common;
using PX.Data;
using PX.Objects.Common;
using PX.Objects.IN;

namespace PX.Objects.AP
{
	public class APVendorPriceMaintTemplateItemExtension : PXGraphExtension<APVendorPriceMaint>
	{
		public static bool IsActive() => PXAccess.FeatureInstalled<CS.FeaturesSet.matrixItem>();

		public delegate BqlCommand CreateUnitCostSelectCommandParameterLessOrig();
		/// Overrides <see cref="APVendorPriceMaint.CreateUnitCostSelectCommand()" />
		[PXOverride]
		public virtual BqlCommand CreateUnitCostSelectCommand(CreateUnitCostSelectCommandParameterLessOrig baseMethod)
		{
			return baseMethod().OrderByNew(typeof(OrderBy<Desc<APVendorPrice.isPromotionalPrice,
						Desc<APVendorPrice.siteID,
						Desc<APVendorPrice.vendorID,
						Asc<InventoryItem.isTemplate,
						Desc<APVendorPrice.breakQty>>>>>>));
		}

		public delegate int?[] GetInventoryIDsOrig(PXCache sender, int? inventoryID);
		/// Overrides <see cref="APVendorPriceMaint.GetInventoryIDs(PXCache, int?)" />
		[PXOverride]
		public virtual int?[] GetInventoryIDs(PXCache sender, int? inventoryID, GetInventoryIDsOrig baseMethod)
		{
			int? templateInventoryID = InventoryItem.PK.Find(sender.Graph, inventoryID)?.TemplateItemID;

			return templateInventoryID != null ? new int?[] { inventoryID, templateInventoryID } : baseMethod(sender, inventoryID);
		}
	}
}
