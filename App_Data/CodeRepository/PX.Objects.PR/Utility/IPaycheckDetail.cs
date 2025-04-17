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

namespace PX.Objects.PR
{
	public interface IPaycheckDetail<TKey>
	{
		int? BranchID { get; set; }
		TKey ParentKeyID { set; }
		decimal? Amount { get; set; }
	}

	public interface IPaycheckExpenseDetail<TKey> : IPaycheckDetail<TKey>
	{
		int? ProjectID { get; set; }
		int? ProjectTaskID { get; set; }
		int? CostCodeID { get; set; }
		string EarningTypeCD { get; set; }
		int? LabourItemID { get; set; }
	}
}
