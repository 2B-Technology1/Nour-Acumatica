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
using PX.Objects.PR;
using System;

namespace PX.Objects.PR
{
	[PXHidden]
	public partial class PRCalculationEngine : PXGraph<PRCalculationEngine>
	{
		protected class WCPremiumDetails
		{
			public void Add(WCPremiumDetails other)
			{
				this.ApplicableRegularEarningAmountForDed += other.ApplicableRegularEarningAmountForDed;
				this.ApplicableRegularEarningAmountForBen += other.ApplicableRegularEarningAmountForBen;
				this.ApplicableRegularEarningHoursForDed += other.ApplicableRegularEarningHoursForDed;
				this.ApplicableRegularEarningHoursForBen += other.ApplicableRegularEarningHoursForBen;
				this.ApplicableOvertimeEarningAmountForDed += other.ApplicableOvertimeEarningAmountForDed;
				this.ApplicableOvertimeEarningAmountForBen += other.ApplicableOvertimeEarningAmountForBen;
				this.ApplicableOvertimeEarningHoursForDed += other.ApplicableOvertimeEarningHoursForDed;
				this.ApplicableOvertimeEarningHoursForBen += other.ApplicableOvertimeEarningHoursForBen;
			}

			public decimal ApplicableRegularEarningAmountForDed = 0m;
			public decimal ApplicableRegularEarningAmountForBen = 0m;
			public decimal ApplicableRegularEarningHoursForDed = 0m;
			public decimal ApplicableRegularEarningHoursForBen = 0m;
			public decimal ApplicableOvertimeEarningAmountForDed = 0m;
			public decimal ApplicableOvertimeEarningAmountForBen = 0m;
			public decimal ApplicableOvertimeEarningHoursForDed = 0m;
			public decimal ApplicableOvertimeEarningHoursForBen = 0m;

			public decimal ApplicableTotalEarningAmountForDed => ApplicableRegularEarningAmountForDed + ApplicableOvertimeEarningAmountForDed;
			public decimal ApplicableTotalEarningAmountForBen => ApplicableRegularEarningAmountForBen + ApplicableOvertimeEarningAmountForBen;
			public decimal ApplicableTotalEarningHoursForDed => ApplicableRegularEarningHoursForDed + ApplicableOvertimeEarningHoursForDed;
			public decimal ApplicableTotalEarningHoursForBen => ApplicableRegularEarningHoursForBen + ApplicableOvertimeEarningHoursForBen;

			public bool SameApplicableForDedAndBen => ApplicableRegularEarningAmountForDed == ApplicableRegularEarningAmountForBen
				&& ApplicableRegularEarningHoursForDed == ApplicableRegularEarningHoursForBen
				&& ApplicableOvertimeEarningAmountForDed == ApplicableOvertimeEarningAmountForBen
				&& ApplicableOvertimeEarningHoursForDed == ApplicableOvertimeEarningHoursForBen;
			public bool HasAnyApplicableForDed => ApplicableRegularEarningAmountForDed != 0
				|| ApplicableRegularEarningHoursForDed != 0
				|| ApplicableOvertimeEarningAmountForDed != 0
				|| ApplicableOvertimeEarningHoursForDed != 0;
			public bool HasAnyApplicableForBen => ApplicableRegularEarningAmountForBen != 0
				|| ApplicableRegularEarningHoursForBen != 0
				|| ApplicableOvertimeEarningAmountForBen != 0
				|| ApplicableOvertimeEarningHoursForBen != 0;

			#region Obsolete 2020R2
			[Obsolete]
			public decimal ApplicableEarningAmountForDed = 0m;
			[Obsolete]
			public decimal ApplicableEarningAmountForBen = 0m;
			[Obsolete]
			public decimal ApplicableEarningHoursForDed = 0m;
			[Obsolete]
			public decimal ApplicableEarningHoursForBen = 0m;
			[Obsolete]
			public decimal RegularHours = 0m;
			[Obsolete]
			public decimal RegularWageBase = 0m;
			[Obsolete]
			public decimal OvertimeHours = 0m;
			[Obsolete]
			public decimal OvertimeWageBase = 0m;
			#endregion
		}
	}
}
