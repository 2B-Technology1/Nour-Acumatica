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
using PX.Data;

namespace PX.Objects.AR.CCPaymentProcessing.Common
{
	public sealed class DocDateWarningDisplay : IPXCustomInfo
	{
		public void Complete(PXLongRunStatus status, PXGraph graph)
		{
			if (status == PXLongRunStatus.Completed && graph is ARPaymentEntry)
			{
				((ARPaymentEntry)graph).RowSelected.AddHandler<ARPayment>((sender, e) =>
				{
					ARPayment payment = e.Row as ARPayment;
					if (payment != null && payment.Released == false && DateTime.Compare(payment.AdjDate.Value, _NewDate) != 0)
					{
						sender.RaiseExceptionHandling<ARPayment.adjDate>(payment, payment.AdjDate, new PXSetPropertyException(Messages.ApplicationDateChanged, PXErrorLevel.Warning));
					}
				});
			}
		}
		private readonly DateTime _NewDate;
		public DocDateWarningDisplay(DateTime newDate)
		{
			_NewDate = newDate;
		}
	}
}
