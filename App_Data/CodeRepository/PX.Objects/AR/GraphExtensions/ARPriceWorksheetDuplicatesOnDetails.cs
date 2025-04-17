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
using PX.Objects.Common.GraphExtensions;
using System;
using System.Collections.Generic;

namespace PX.Objects.AR
{
	// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
	public class ARPriceWorksheetDuplicatesOnDetails : ForbidDuplicateDetailsExtension<ARPriceWorksheetMaint, ARPriceWorksheet, ARPriceWorksheetDetail>
	{
		protected override IEnumerable<Type> GetDetailUniqueFields()
		{
			yield return typeof(ARPriceWorksheetDetail.refNbr);
			yield return typeof(ARPriceWorksheetDetail.priceType);
			yield return typeof(ARPriceWorksheetDetail.customerID);
			yield return typeof(ARPriceWorksheetDetail.custPriceClassID);
			yield return typeof(ARPriceWorksheetDetail.inventoryID);
			yield return typeof(ARPriceWorksheetDetail.siteID);
			yield return typeof(ARPriceWorksheetDetail.subItemID);
			yield return typeof(ARPriceWorksheetDetail.uOM);
			yield return typeof(ARPriceWorksheetDetail.curyID);
			yield return typeof(ARPriceWorksheetDetail.breakQty);
		}

		/// Overrides <see cref="ARPriceWorksheetMaint.CheckForDuplicateDetails"/>
		[PXOverride]
		public virtual void CheckForDuplicateDetails(Action baseImpl)
		{
			baseImpl();

			CheckForDuplicates();
		}

		protected override void RaiseDuplicateError(ARPriceWorksheetDetail duplicate)
		{
			DetailsCache.RaiseExceptionHandling<ARPriceWorksheetDetail.priceCode>(
				duplicate,
				duplicate.PriceCode,
				new PXSetPropertyException(
					Messages.DuplicateSalesPriceWS,
					PXErrorLevel.RowError,
					typeof(ARPriceWorksheetDetail.priceCode).Name));
		}
	}
}
