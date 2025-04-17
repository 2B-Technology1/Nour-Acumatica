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
using PX.Objects.CS;

namespace PX.Objects.EP.DAC
{
	public sealed class ExpenseClaimDetailsBankFeedExt : PXCacheExtension<EPExpenseClaimDetails>
	{
		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<FeaturesSet.bankFeedIntegration>();
		}

		#region Category
		public abstract class category : PX.Data.BQL.BqlString.Field<category> { }
		[PXDBString(100, IsUnicode = true)]
		[PXUIField(DisplayName = "Category")]
		public string Category { get; set; }
		#endregion

		#region BankTranStatus
		public abstract class bankTranStatus : PX.Data.BQL.BqlString.Field<bankTranStatus> { }
		[PXDBString]
		[EPBankTranStatus.List]
		[PXUIField(DisplayName = "Bank Transaction Status", Enabled = false)]
		public string BankTranStatus { get; set; }
		#endregion
	}
}
