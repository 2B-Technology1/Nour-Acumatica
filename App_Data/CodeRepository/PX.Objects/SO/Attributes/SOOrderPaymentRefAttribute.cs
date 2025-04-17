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

using PX.Common;
using PX.Data;
using PX.Objects.AR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PX.Objects.SO.Attributes
{
	/// <summary>
	/// This attribute implements auto-generation of the next check sequential number for Sales Order document<br/>
	/// according to the settings in the CashAccount and PaymentMethod. <br/>
	/// </summary>
	public class SOOrderPaymentRefAttribute : PaymentRefAttribute
	{
		public SOOrderPaymentRefAttribute(Type cashAccountID, Type paymentTypeID, Type updateNextNumber)
			: base(cashAccountID, paymentTypeID, updateNextNumber)
		{
		}

		protected virtual bool IsEnabled(SOOrder order)
			=> (order?.ARDocType?.IsIn(ARDocType.CashSale, ARDocType.CashReturn) == true);

		public override void FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (!IsEnabled(e.Row as SOOrder))
				return;

			// Acuminator disable once PX1075 RaiseExceptionHandlingInEventHandlers Legacy code - PaymentRefAttribute
			base.FieldDefaulting(sender, e);
		}

		public override void FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			if (!IsEnabled(e.Row as SOOrder))
				return;

			base.FieldVerifying(sender, e);
		}

		public override void RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			if (!IsEnabled(e.Row as SOOrder))
				return;

			// Acuminator disable once PX1043 SavingChangesInEventHandlers Legacy code - PaymentRefAttribute
			base.RowPersisting(sender, e);
		}

		protected override void DefaultRef(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			if (!IsEnabled(e.Row as SOOrder))
				return;

			base.DefaultRef(sender, e);
		}
	}
}
