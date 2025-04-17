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
	public class OrganizationMaintDunningLetter : PXGraphExtension<OrganizationMaint>
	{
		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<FeaturesSet.dunningLetter>();
		}

		public PXSetup<ARSetup> arSetup;

		[PXOverride]
		public void Init(Action baseMethod)
		{
			baseMethod();

			ARSetup arPrefs = arSetup.Select();

			var consolidateForCompany = arPrefs?.PrepareDunningLetters == ARSetup.prepareDunningLetters.ConsolidatedForCompany;
			PXUIFieldAttribute.SetReadOnly<BranchDunningLetter.isDunningCompanyBranchID>(Base.BranchesView.Cache, null, !consolidateForCompany);
			PXUIFieldAttribute.SetVisible<BranchDunningLetter.isDunningCompanyBranchID>(Base.BranchesView.Cache, null, consolidateForCompany);
			PXUIFieldAttribute.SetVisibility<BranchDunningLetter.isDunningCompanyBranchID>(Base.BranchesView.Cache, null, consolidateForCompany ? PXUIVisibility.Visible : PXUIVisibility.Invisible);
		}

		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXFormula(typeof(Selector<Branch.organizationID, Organization.dunningFeeBranchID>))]
		protected virtual void _(Events.CacheAttached<BranchDunningLetter.dunningCompanyBranchID> e) { }

		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXFormula(typeof(BranchDunningLetter.dunningCompanyBranchID.IsNotNull.And<Branch.branchID.IsEqual<BranchDunningLetter.dunningCompanyBranchID>>))]
		protected virtual void _(Events.CacheAttached<BranchDunningLetter.isDunningCompanyBranchID> e) { }

		protected virtual void _(Events.FieldUpdated<BranchDunningLetter.isDunningCompanyBranchID> e)
		{
			if (e.Row == null) return;

			Base.OrganizationView.Current.DunningFeeBranchID = ((bool?)e.NewValue == true) ? ((Branch)e.Row).BranchID : null;
			Base.OrganizationView.Cache.MarkUpdated(Base.OrganizationView.Current);

			foreach (Branch b in Base.BranchesView.Select())
			{
				if (b.BranchID != Base.OrganizationView.Current.DunningFeeBranchID)
				{
					Base.BranchesView.Cache.SetValue<BranchDunningLetter.isDunningCompanyBranchID>(b, false);
				}
			}

			Base.BranchesView.View.RequestRefresh();
		}

		[PXOverride]
		public void Persist(Action baseMethod)
		{
			Organization organization = Base.OrganizationView.Select();
			Organization origOrganization = (Organization)Base.OrganizationView.Cache.GetOriginal(organization);

			using (new RunningFlagScope<OrganizationMaint>())
			{
				baseMethod();
			}

			if (organization != null && organization.DunningFeeBranchID != origOrganization.DunningFeeBranchID)
			{
				Base.BranchesView.Cache.Clear();
			}
		}
	}
}
