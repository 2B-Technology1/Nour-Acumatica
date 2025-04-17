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
using System.Collections.Generic;
using PX.Objects.Common.GraphExtensions;
using PX.Data;

namespace PX.Objects.AP
{
	// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
	public class APPriceWorksheetDuplicatesOnDetails: ForbidDuplicateDetailsExtension<APPriceWorksheetMaint, APPriceWorksheet, APPriceWorksheetDetail>
	{
		protected override IEnumerable<Type> GetDetailUniqueFields()
		{
			yield return typeof(APPriceWorksheetDetail.refNbr);
			yield return typeof(APPriceWorksheetDetail.vendorID);
			yield return typeof(APPriceWorksheetDetail.inventoryID);
			yield return typeof(APPriceWorksheetDetail.siteID);
			yield return typeof(APPriceWorksheetDetail.subItemID);
			yield return typeof(APPriceWorksheetDetail.uOM);
			yield return typeof(APPriceWorksheetDetail.curyID);
			yield return typeof(APPriceWorksheetDetail.breakQty);
		}

		/// Overrides <see cref="APPriceWorksheetMaint.CheckForDuplicateDetails"/>
		[PXOverride]
		public virtual void CheckForDuplicateDetails(Action baseImpl)
		{
			baseImpl();

			CheckForDuplicates();
		}

		protected override void RaiseDuplicateError(APPriceWorksheetDetail duplicate)
		{
			DetailsCache.RaiseExceptionHandling<APPriceWorksheetDetail.vendorID>(
					duplicate,
					duplicate.VendorID,
					new PXSetPropertyException(
						Messages.DuplicateVendorPrice,
						PXErrorLevel.RowError,
						typeof(APPriceWorksheetDetail.vendorID).Name));
		}
	}
}
