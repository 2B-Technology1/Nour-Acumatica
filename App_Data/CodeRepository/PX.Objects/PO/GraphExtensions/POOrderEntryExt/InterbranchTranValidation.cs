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
using System.Collections.Generic;

namespace PX.Objects.PO.GraphExtensions.POOrderEntryExt
{
	public class InterbranchTranValidation
		: Extensions.InterbranchSiteRestrictionExtension<POOrderEntry, POOrder.branchID, POLine, POLine.siteID>
	{
		protected override void _(Events.FieldVerifying<POOrder.branchID> e)
		{
			var headerRow = Base.CurrentDocument.Current;
			if (headerRow?.OrderType == POOrderType.StandardBlanket)
				return;

			base._(e);
		}

		protected override void _(Events.RowPersisting<POLine> e)
		{
			var headerRow = Base.CurrentDocument.Current;
			if (headerRow?.OrderType == POOrderType.StandardBlanket)
				return;

			base._(e);
		}

		protected override IEnumerable<POLine> GetDetails()
		{
			return Base.Transactions.Select().RowCast<POLine>();
		}
	}
}