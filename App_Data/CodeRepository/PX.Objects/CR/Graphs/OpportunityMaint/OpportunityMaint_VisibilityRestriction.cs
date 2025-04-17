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
using PX.Objects.AR;
using PX.Objects.CS;
using PX.Objects.GL;
using System.Collections.Generic;
using PX.Objects.Common;

namespace PX.Objects.CR
{
	public class OpportunityMaint_VisibilityRestriction : PXGraphExtension<OpportunityMaint>
	{
		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<FeaturesSet.visibilityRestriction>();
		}

		[Branch(typeof(AccessInfo.branchID), IsDetail = false, TabOrder = 0, BqlField = typeof(Standalone.CROpportunityRevision.branchID))]
		[PXFormula(typeof(Switch<
							Case<
								Where<PendingValue<CROpportunity.branchID>, IsNotNull>,
								Null,
							Case<
								Where<CROpportunity.locationID, IsNotNull,
									And<Selector<CROpportunity.locationID, Location.cBranchID>, IsNotNull>>,
								Selector<CROpportunity.locationID, Location.cBranchID>,
							Case<
								Where<CROpportunity.bAccountID, IsNotNull,
									And<Not<Selector<CROpportunity.bAccountID, BAccount.cOrgBAccountID>, RestrictByBranch<Current2<CROpportunity.branchID>>>>>,
								Null,
							Case<
								Where<Current2<CROpportunity.branchID>, IsNotNull>,
								Current2<CROpportunity.branchID>>>>>,
							Current<AccessInfo.branchID>>))]
		public virtual void _(Events.CacheAttached<CROpportunity.branchID> e) { }

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[RestrictCustomerByBranch(typeof(BAccount.cOrgBAccountID), branchID: typeof(CROpportunity.branchID))]
		public virtual void _(Events.CacheAttached<CROpportunity.bAccountID> e) { }

		public delegate void CopyPasteGetScriptDelegate(bool isImportSimple, List<Api.Models.Command> script, List<Api.Models.Container> containers);

		[PXOverride]
		public void CopyPasteGetScript(bool isImportSimple, List<Api.Models.Command> script, List<Api.Models.Container> containers, CopyPasteGetScriptDelegate baseMethod)
		{
			baseMethod(isImportSimple, script, containers);

			// We need to process fields together that are related to the Branch and Customer for proper validation. For this:
			// 1) set the right order of the fields
			// 2) insert dependent fields after the BranchID field
			// 3) all fields must belong to the same view.
			
			string branchViewName = nameof(OpportunityMaint.OpportunityCurrent);
			string customerViewName = nameof(OpportunityMaint.Opportunity);

			(string name, string viewName) branch = (nameof(CROpportunity.BranchID), branchViewName);

			List<(string name, string viewName)> fieldList = new List<(string name, string viewName)>();
			fieldList.Add((nameof(CROpportunity.BAccountID), customerViewName));
			fieldList.Add((nameof(CROpportunity.LocationID), customerViewName));

			Common.Utilities.SetDependentFieldsAfterBranch(script, branch, fieldList);
		}
	}
}
