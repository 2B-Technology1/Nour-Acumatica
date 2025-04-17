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
	public class AustralianDiminishingValueAdditionParameters : AdditionParameters
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
		/// Base value used for depreciation calculation
		/// </summary>
		public decimal DepreciationBasis;

		/// <summary>
		/// Depreciate From Date
		/// </summary>
		public DateTime DepreciateFromDate;

		/// <summary>
		/// True when the adition is an original acquisition
		/// </summary>
		public bool IsFirstAddition;
	}
}
