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

namespace PX.Objects.PR
{
	public interface IPTOBank
	{
		string BankID { get; set; }

		string AccrualMethod { get; set; }

		decimal? AccrualRate { get; set; }

		decimal? HoursPerYear { get; set; }

		decimal? AccrualLimit { get; set; }

		bool? IsActive { get; set; }

		DateTime? StartDate { get; set; }

		DateTime? PTOYearStartDate { get; set; }

		string CarryoverType { get; set; }

		decimal? CarryoverAmount { get; set; }

		decimal? FrontLoadingAmount { get; set; }

		bool? AllowNegativeBalance { get; set; }

		int? CarryoverPayMonthLimit { get; set; }

		bool? DisburseFromCarryover { get; set; }

		string DisbursingType { get; set; }

		bool? CreateFinancialTransaction { get; set; }
	}
}
