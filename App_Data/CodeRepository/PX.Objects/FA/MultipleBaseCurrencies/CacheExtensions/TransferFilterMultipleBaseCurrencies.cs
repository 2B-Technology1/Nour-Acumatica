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
using PX.Objects.GL;
using static PX.Objects.FA.TransferProcess;

namespace PX.Objects.FA
{
	public sealed class TransferFilterMultipleBaseCurrencies : PXCacheExtension<TransferFilter>
	{
		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<FeaturesSet.multipleBaseCurrencies>();
		}

		#region BranchTo
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXRestrictor(
			typeof(Where<Branch.baseCuryID, EqualBaseCuryID<Current2<TransferFilter.branchFrom>>,
				Or<Current2<TransferFilter.branchFrom>, IsNull>>),
			Messages.DestinationBranchBaseCurrencyDiffersFromSourceBranch,
			typeof(Branch.baseCuryID),
			typeof(Branch.branchCD),
			typeof(TransferFilter.branchFrom))]
		public int? BranchTo
		{
			get;
			set;
		}
		#endregion

		#region BranchFrom
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXRestrictor(
			typeof(Where<Branch.baseCuryID, EqualBaseCuryID<Current2<TransferFilter.branchTo>>,
				Or<Current2<TransferFilter.branchTo>, IsNull>>),
			Messages.SourceBranchBaseCurrencyDiffersFromDestinationBranch,
			typeof(Branch.baseCuryID),
			typeof(Branch.branchCD),
			typeof(TransferFilter.branchTo))]
		public int? BranchFrom
		{
			get;
			set;
		}
		#endregion
	}
}
