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

namespace PX.Objects.PR
{
	[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
	public class DedAndBenPackageEventSubscriberAttribute : PXEventSubscriberAttribute, IPXFieldUpdatedSubscriber
	{
		private Type _deductionAmountField;
		private Type _deductionRateField;
		private Type _benefitAmountField;
		private Type _benefitRateField;
		private Type _effectiveDateField;

		public DedAndBenPackageEventSubscriberAttribute(Type deductionAmountField, Type deductionRateField, Type benefitAmountField, Type benefitRateField, Type effectiveDateField)
		{
			_deductionAmountField = deductionAmountField;
			_deductionRateField = deductionRateField;
			_benefitAmountField = benefitAmountField;
			_benefitRateField = benefitRateField;
			_effectiveDateField = effectiveDateField;
		}

		public void FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			if (e.Row == null)
				return;

			sender.SetDefaultExt(e.Row, _deductionAmountField.Name);
			sender.SetDefaultExt(e.Row, _deductionRateField.Name);
			sender.SetDefaultExt(e.Row, _benefitAmountField.Name);
			sender.SetDefaultExt(e.Row, _benefitRateField.Name);
			sender.SetValue(e.Row, _effectiveDateField.Name, null);
		}
	}
}
