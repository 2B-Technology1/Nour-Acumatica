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
using PX.Objects.Common.Attributes;
using PX.Objects.AR;
using PX.Data;

namespace PX.Objects.SO
{
	public class CopyLinkToSOInvoiceAttribute : CopyChildLinkAttribute
	{
		public CopyLinkToSOInvoiceAttribute(Type counterField, Type amountField, Type[] linkChildKeys, Type[] linkParentKeys)
			:base(counterField, amountField, linkChildKeys, linkParentKeys)
		{ }

		public override void RowDeleted(PXCache sender, PXRowDeletedEventArgs e)
		{
			if (IsParentDeleted(sender, e.Row))
				return;//for other cases(single parent) this verification is doing inside PXFormulaAttribute.UpdateParent parent

			base.RowDeleted(sender, e);
		}

		protected override bool IsParentDeleted(PXCache childCache, object childRow)
		{
			//SOInvoice row is still not deleted. The sequence of RowDeleted events: ARInvoice -> ARTran -> ... -> SOInvoice -> ...
			var arInvoice = PXParentAttribute.SelectParent(childCache, childRow, typeof(ARInvoice));
			return arInvoice == null;
		}
	}
}
