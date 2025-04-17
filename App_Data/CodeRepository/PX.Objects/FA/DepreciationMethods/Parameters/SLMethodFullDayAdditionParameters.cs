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

using PX.Objects.Common;
using System;
using System.Collections.Generic;

namespace PX.Objects.FA.DepreciationMethods.Parameters
{
	public class SLMethodFullDayAdditionParameters : SLMethodAdditionParameters
	{
		/// <summary>
		/// Last date of the depreciation (including)
		/// </summary>
		public DateTime DepreciateToDate;

		/// <summary>
		/// The period in which the depreciation is starting
		/// </summary>
		public FABookPeriod DepreciateFromPeriod;

		/// <summary>
		/// The period in which the depreciation is ending
		/// </summary>
		public FABookPeriod DepreciateToPeriod;

		/// <summary>
		/// Number of days in the first period (<see cref="DepreciateFromPeriod"/>) in which the fixed asset is depreciated.
		/// This is the difference between the <see cref="SLMethodAdditionParameters.DepreciateFromDate"/> and the end of the <see cref="DepreciateFromPeriod"/>
		/// </summary>
		public double DaysHeldInFromPeriod;

		/// <summary>
		/// Number of days in the last period (<see cref="DepreciateToPeriod"/>) in which the fixed asset is depreciated.
		/// This is the difference between the start of the <see cref="DepreciateToPeriod"/> and the <see cref="DepreciateToDate"/>
		/// </summary>
		public double DaysHeldInToPeriod;

		/// <summary>
		/// Number of total days in <see cref="DepreciateFromPeriod"/>
		/// </summary>
		public double TotalDaysInFromPeriod;

		/// <summary>
		/// Number of total days in <see cref="DepreciateToPeriod"/>
		/// </summary>
		public double TotalDaysInToPeriod;

		/// <summary>
		/// True when depreciation starts from the start of the first period
		/// (<see cref="SLMethodAdditionParameters.DepreciateFromDate"/> is a first date of the <see cref="DepreciateFromPeriod"/>)
		/// </summary>
		public bool IsFirstPeriodFull;
		/// <summary>
		/// True when the ddition is an original acquisition (the addition Period is the <see cref="DepreciateFromPeriod"/>)
		/// </summary>
		public bool IsFirstAddition;
	}
}
