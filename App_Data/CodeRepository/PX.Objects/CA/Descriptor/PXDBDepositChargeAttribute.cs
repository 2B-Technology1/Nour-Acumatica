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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PX.Objects.CA.Descriptor
{
	public class PXDBDepositChargeAttribute : PXDBStringAttribute, IPXFieldVerifyingSubscriber
	{
		public PXDBDepositChargeAttribute(int length) : base(length)
		{
		}

		public void FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			CADepositCharge charge = e.Row as CADepositCharge;
			string newValue = e.NewValue?.ToString();

			if (charge is null)
			{
				return;
			}

			CADepositCharge dublicate = null;

			if (_FieldName == nameof(CADepositCharge.EntryTypeID))
			{
				dublicate = PXSelect<CADepositCharge,
					Where<CADepositCharge.tranType, Equal<Required<CADepositCharge.tranType>>,
					And<CADepositCharge.refNbr, Equal<Required<CADepositCharge.refNbr>>,
					And<CADepositCharge.entryTypeID, Equal<Required<CADepositCharge.entryTypeID>>,
					And<CADepositCharge.paymentMethodID, Equal<Required<CADepositCharge.paymentMethodID>>>>>>>
					.Select(sender.Graph, charge?.TranType, charge?.RefNbr, newValue, charge?.PaymentMethodID);
			}
			else if (_FieldName == nameof(CADepositCharge.PaymentMethodID))
			{
				dublicate = PXSelect<CADepositCharge,
					Where<CADepositCharge.tranType, Equal<Required<CADepositCharge.tranType>>,
					And<CADepositCharge.refNbr, Equal<Required<CADepositCharge.refNbr>>,
					And<CADepositCharge.entryTypeID, Equal<Required<CADepositCharge.entryTypeID>>,
					And<CADepositCharge.paymentMethodID, Equal<Required<CADepositCharge.paymentMethodID>>>>>>>
					.Select(sender.Graph, charge?.TranType, charge?.RefNbr, charge?.EntryTypeID, newValue);
			}

			PXEntryStatus status = sender.GetStatus(charge);

			if (dublicate != null &&
				(status == PXEntryStatus.Inserted || status == PXEntryStatus.Updated))
			{
				sender.RaiseExceptionHandling<CADepositCharge.entryTypeID>(charge, charge.EntryTypeID, new PXSetPropertyException(Messages.ChargeAlreadyExists, PXErrorLevel.Error));
				sender.RaiseExceptionHandling<CADepositCharge.paymentMethodID>(charge, charge.PaymentMethodID, new PXSetPropertyException(Messages.ChargeAlreadyExists, PXErrorLevel.Error));
				throw new PXSetPropertyException(Messages.ChargeAlreadyExists, PXErrorLevel.Error);
			}
		}
	}
}

