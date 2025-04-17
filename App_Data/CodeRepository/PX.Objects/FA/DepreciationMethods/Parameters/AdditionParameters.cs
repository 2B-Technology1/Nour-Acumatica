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
	/// <exclude/>
	public class AdditionParameters
	{
		public string DepreciateFromPeriodID;
		public string DepreciateToPeriodID;
	}

	/// <exclude/>
	public class FAAddition
	{
		public FAAddition(decimal amount, string finPeriodID, DateTime date, int precision, decimal businessUse)
		{
			Amount = amount;
			PeriodID = finPeriodID;
			Date = date;
			Precision = precision;
			BusinessUse = businessUse * 0.01m;
		}

		public string PeriodID { get; protected set; }
		public DateTime Date { get; protected set; }
		public decimal Amount { get; set; }
		public bool IsOriginal { get; protected set; }

		public decimal SalvageAmount { get; protected set; } = 0m;
		public decimal Section179Amount { get; protected set; } = 0m;
		public decimal BonusAmount { get; protected set; } = 0m;
		public decimal BusinessUse { get; protected set; } = 1m;

		protected int Precision;

		public decimal DepreciationBasis => PXRounder.Round(Amount * BusinessUse, Precision) - Section179Amount - BonusAmount - SalvageAmount;

		public AdditionParameters CalculatedAdditionParameters;

		public void MarkOriginal(FABookBalance bookBalance)
		{
			IsOriginal = true;
			BusinessUse = bookBalance.BusinessUse * 0.01m ?? 1m;
			SalvageAmount = bookBalance.SalvageAmount ?? 0m;
			Section179Amount = bookBalance.Tax179Amount ?? 0m;
			BonusAmount = bookBalance.BonusAmount ?? 0m;
		}

		public void MarkOriginal()
		{
			IsOriginal = true;
		}
	}
}
