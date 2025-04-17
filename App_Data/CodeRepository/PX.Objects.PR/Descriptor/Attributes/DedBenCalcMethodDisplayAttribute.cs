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
using System.Linq;
using PX.Data;
using PX.Payroll.Data;

namespace PX.Objects.PR
{
	public abstract class DedBenCalcMethodDisplayAttribute : PXEventSubscriberAttribute, IPXFieldSelectingSubscriber
	{
		protected abstract bool IsDeductionField { get; }

		public void FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			PRDeductCode row = e.Row as PRDeductCode;
			if (row == null)
			{
				return;
			}

			if (IsDeductionField && row.ContribType == ContributionType.EmployerContribution ||
				!IsDeductionField && row.ContribType == ContributionType.EmployeeDeduction)
			{
				e.ReturnValue = null;
			}
		}
	}

	public class DeductionCalcMethodDisplayAttribute : DedBenCalcMethodDisplayAttribute
	{
		protected override bool IsDeductionField => true;
	}

	public class BenefitCalcMethodDisplayAttribute : DedBenCalcMethodDisplayAttribute
	{
		protected override bool IsDeductionField => false;
	}
}
