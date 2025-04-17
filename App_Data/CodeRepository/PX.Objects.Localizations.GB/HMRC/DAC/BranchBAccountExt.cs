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
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.CS;
using PX.Objects.GL;

namespace PX.Objects.Localizations.GB.HMRC.DAC
{
	/// <summary>
	/// Business account branch that is used to file VAT with the MTD (Making Tax Digital) application.
	/// </summary>
	public sealed class BranchBAccountExt : PXCacheExtension<BranchMaint.BranchBAccount>
	{
		public static bool IsActive() => PXAccess.FeatureInstalled<FeaturesSet.uKLocalization>();
		#region MTDApplicationID
		public abstract class mTDApplicationID : BqlInt.Field<mTDApplicationID> { }
		/// <summary>
		/// MTD Application ID
		/// </summary>
		[PXInt]
		[PXUIField(DisplayName = "MTD External Application")]
		[PXSelector(typeof(SearchFor<HMRCOAuthApplication.applicationID>.In<
				SelectFrom<HMRCOAuthApplication>.
				LeftJoin<BAccountMTDApplication>.On<HMRCOAuthApplication.applicationID.IsEqual<BAccountMTDApplication.applicationID>>.
				Where<HMRCOAuthApplication.type.IsEqual<HMRCOAuthApplication.HMRCApplicationType>.
					And<
						Brackets
						<BAccountMTDApplication.applicationID.IsNull>.
						Or<BAccountMTDApplication.bAccountID.IsEqual<BranchMaint.BranchBAccount.bAccountID.FromCurrent>>>>>),
			typeof(HMRCOAuthApplication.applicationID),
			typeof(HMRCOAuthApplication.applicationName),
			SubstituteKey = typeof(HMRCOAuthApplication.applicationID),
			DescriptionField = typeof(HMRCOAuthApplication.applicationName))]
		[PXDBScalar(typeof(SearchFor<HMRCOAuthApplication.applicationID>.In<
			SelectFrom<HMRCOAuthApplication>.InnerJoin<BAccountMTDApplication>.On<
				HMRCOAuthApplication.applicationID.IsEqual<BAccountMTDApplication.applicationID>>.Where<
				BAccountMTDApplication.bAccountID.IsEqual<Branch.bAccountID>
			>>))]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		public int? MTDApplicationID
		{
			get;
			set;
		}
		#endregion
	}
}
