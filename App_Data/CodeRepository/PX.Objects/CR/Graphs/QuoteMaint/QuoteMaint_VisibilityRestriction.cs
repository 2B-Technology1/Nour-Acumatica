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
	public class QuoteMaint_VisibilityRestriction : PXGraphExtension<QuoteMaint>
	{
		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<FeaturesSet.visibilityRestriction>();
		}

		#region BranchID
		[Branch(typeof(AccessInfo.branchID), IsDetail = false, TabOrder = 0, BqlField = typeof(Standalone.CROpportunityRevision.branchID))]
		[PXFormula(typeof(Switch<
							Case<
								Where<PendingValue<CRQuote.branchID>, IsNotNull>,
								Null,
							Case<
								Where<CRQuote.locationID, IsNotNull,
									And<Selector<CRQuote.locationID, Location.cBranchID>, IsNotNull>>,
								Selector<CRQuote.locationID, Location.cBranchID>,
							Case<
								Where<CRQuote.bAccountID, IsNotNull,
									And<Not<Selector<CRQuote.bAccountID, BAccount.cOrgBAccountID>, RestrictByBranch<Current2<CRQuote.branchID>>>>>,
								Null,
							Case<
								Where<Current2<CRQuote.branchID>, IsNotNull>,
								Current2<CRQuote.branchID>>>>>,
							Current<AccessInfo.branchID>>))]
		public virtual void _(Events.CacheAttached<CRQuote.branchID> e) { }
		#endregion

		#region BAccount
		[PXMergeAttributes(Method = MergeMethod.Append)]
		[RestrictCustomerByBranch(typeof(BAccount.cOrgBAccountID), branchID: typeof(CRQuote.branchID))]
		public virtual void _(Events.CacheAttached<CRQuote.bAccountID> e) { }
		#endregion

		public delegate void CopyPasteGetScriptDelegate(bool isImportSimple, List<Api.Models.Command> script, List<Api.Models.Container> containers);

		[PXOverride]
		public void CopyPasteGetScript(bool isImportSimple, List<Api.Models.Command> script, List<Api.Models.Container> containers, CopyPasteGetScriptDelegate baseMethod)
		{
			baseMethod(isImportSimple, script, containers);

			// We need to process fields together that are related to the Branch and Customer for proper validation. For this:
			// 1) set the right order of the fields
			// 2) insert dependent fields after the BranchID field
			// 3) all fields must belong to the same view.

			string branchViewName = nameof(QuoteMaint.QuoteCurrent) + ": 2";
			string quoteViewName = nameof(QuoteMaint.Quote);

			(string name, string viewName) branch = (nameof(CRQuote.BranchID), branchViewName);

			List<(string name, string viewName)> fieldList = new List<(string name, string viewName)>();
			fieldList.Add((nameof(CRQuote.BAccountID), quoteViewName));
			fieldList.Add((nameof(CRQuote.LocationID), quoteViewName));

			Common.Utilities.SetDependentFieldsAfterBranch(script, branch, fieldList);
		}
	}
}
