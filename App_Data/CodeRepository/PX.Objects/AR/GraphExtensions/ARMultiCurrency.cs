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
using PX.Objects.AR;
using System;
using System.Collections.Generic;

namespace PX.Objects.Extensions.MultiCurrency.AR
{
	public abstract class ARMultiCurrencyGraph<TGraph, TPrimary> : FinDocMultiCurrencyGraph<TGraph, TPrimary>
		where TGraph : PXGraph
		where TPrimary : class, IBqlTable, new()
	{
		protected override string Module => GL.BatchModule.AR;

		protected override IEnumerable<Type> FieldWhichShouldBeRecalculatedAnyway
		{
			get
			{
				yield return typeof(ARInvoice.curyDocBal);
				yield return typeof(ARInvoice.curyDiscBal);
			}
		}

		protected override CurySourceMapping GetCurySourceMapping()
		{
			return new CurySourceMapping(typeof(Customer));
		}

		protected override bool ShouldBeDisabledDueToDocStatus()
		{
			switch (DocumentStatus)
			{
				case ARDocStatus.Open:
				case ARDocStatus.Closed:
				case ARDocStatus.Reserved:
				case ARDocStatus.PendingApproval:
				case ARDocStatus.PendingPayment:
					return true;
				default: return false;
			}
		}
	}
}
