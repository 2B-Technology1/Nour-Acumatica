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

using PX.Commerce.Core;
using PX.Data;

namespace PX.Commerce.BigCommerce
{
	public class BigCommerceEntityMaintExt : PXGraphExtension<BCEntityMaint>
	{
		public static bool IsActive() { return CommerceFeaturesHelper.BigCommerceConnector; }

		protected virtual void _(Events.RowSelected<BCEntityImportFilter> e)
		{
			BCEntityImportFilter row = e.Row as BCEntityImportFilter;
			if (row != null && row?.ConnectorType == BCConnector.TYPE && (row?.FieldName == BCConstants.CreatedDateTime || row?.FieldName == BCConstants.ModifiedDateTime))
				e.Cache.RaiseExceptionHandling<BCEntityImportFilter.fieldName>(row, row.FieldName,
						new PXSetPropertyException(BCMessages.DateTimeDiscrepancyWarning, PXErrorLevel.RowWarning));
		}

	}
}