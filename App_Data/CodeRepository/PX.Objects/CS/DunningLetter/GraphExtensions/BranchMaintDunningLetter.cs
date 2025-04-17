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
using PX.Data;
using PX.Objects.CS;
using System.Linq;
using PX.Objects.Common;
using PX.Objects.GL;
using PX.Objects.GL.DAC;

namespace PX.Objects.AR
{
	public class BranchMaintDunningLetter : PXGraphExtension<BranchMaint>
	{
		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<FeaturesSet.dunningLetter>();
		}

		public PXSetup<ARSetup> arSetup;

		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXDBCalced(typeof(Branch.branchID), typeof(int))]
		protected virtual void _(Events.CacheAttached<BranchBAccountDunningLetter.branchBranchID> e) { }

		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXFormula(typeof(Selector<BranchMaint.BranchBAccount.organizationID, Organization.dunningFeeBranchID>))]
		protected virtual void _(Events.CacheAttached<BranchBAccountDunningLetter.dunningFeeBranchID> e) { }

		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXFormula(typeof(IIf<Where<BranchBAccountDunningLetter.dunningFeeBranchID, IsNotNull, And<BranchBAccountDunningLetter.branchBranchID, Equal<BranchBAccountDunningLetter.dunningFeeBranchID>>>, True, False>))]
		protected virtual void _(Events.CacheAttached<BranchBAccountDunningLetter.isDunningCompanyBranchID> e) { }

		protected virtual void _(Events.RowSelected<BranchMaint.BranchBAccount> e)
		{
			PXUIFieldAttribute.SetVisible<BranchBAccountDunningLetter.isDunningCompanyBranchID>(Base.CurrentBAccount.Cache, null, false);

			if (e.Row == null || Base.CurrentOrganizationView.Current == null) return;

			ARSetup arPrefs = arSetup.Select();
			var organization = Base.CurrentOrganizationView.Current;

			var consolidateForCompany = arPrefs?.PrepareDunningLetters == ARSetup.prepareDunningLetters.ConsolidatedForCompany;
			var singleCompany = organization.OrganizationType == OrganizationTypes.WithoutBranches;

			if (consolidateForCompany && !singleCompany)
			{
				PXUIFieldAttribute.SetVisible<BranchBAccountDunningLetter.isDunningCompanyBranchID>(Base.CurrentBAccount.Cache, e.Row, true);
			}
		}

		[PXOverride]
		public void AfterPersist()
		{
			if (RunningFlagScope<OrganizationMaint>.IsRunning) return;

			Organization organization = Base.CurrentOrganizationView.Current;
			if (organization.OrganizationType == OrganizationTypes.WithoutBranches) return;

			BranchMaint.BranchBAccount baccount = Base.CurrentBAccount.Current;

			if (baccount == null)
			{
				if (organization.DunningFeeBranchID == null) return;
				var dunningBranch = (Branch)PXSelect<Branch,
						Where<Branch.branchID, Equal<Required<Branch.branchID>>>>
						.Select(Base, organization.DunningFeeBranchID);

				if (dunningBranch == null)
				{
					PXDatabase.Update(typeof(Organization),
						new PXDataFieldAssign(nameof(Organization.DunningFeeBranchID), PXDbType.Int, null),
						new PXDataFieldRestrict(nameof(Organization.organizationID), PXDbType.Int, organization.OrganizationID));
				}
			}
			else
			{
				var branchExt = Base.CurrentBAccount.Cache.GetExtension<BranchBAccountDunningLetter>(baccount);
				var branch = (Branch)PXSelect<Branch,
						Where<Branch.bAccountID, Equal<Required<Branch.bAccountID>>>>
						.Select(Base, baccount.BAccountID);

				if (branchExt.IsDunningCompanyBranchID == true && organization.DunningFeeBranchID != branch.BranchID)
				{
					PXDatabase.Update(typeof(Organization),
						new PXDataFieldAssign(nameof(Organization.DunningFeeBranchID), PXDbType.Int, branch.BranchID),
						new PXDataFieldRestrict(nameof(Organization.organizationID), PXDbType.Int, organization.OrganizationID));
					Base.CurrentOrganizationView.Cache.Clear();
					Base.SelectTimeStamp();
				}
				else if (branchExt.IsDunningCompanyBranchID != true && organization.DunningFeeBranchID == branch.BranchID)
				{
					PXDatabase.Update(typeof(Organization),
						new PXDataFieldAssign(nameof(Organization.DunningFeeBranchID), PXDbType.Int, null),
						new PXDataFieldRestrict(nameof(Organization.organizationID), PXDbType.Int, organization.OrganizationID));
					Base.CurrentOrganizationView.Cache.Clear();
					Base.SelectTimeStamp();
				}
			}
		}
	}
}
