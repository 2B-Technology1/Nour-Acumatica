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
using PX.Objects.AP;
using PX.Objects.AR;
using PX.Objects.Common.Formula;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Objects.GL;
using System.Collections.Generic;
using System.Linq;

namespace PX.Objects.SO
{
	public class SOOrderEntryVisibilityRestriction : PXGraphExtension<SOOrderEntry>
	{
		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<FeaturesSet.visibilityRestriction>();
		}

		public delegate void CopyPasteGetScriptDelegate(bool isImportSimple, List<Api.Models.Command> script, List<Api.Models.Container> containers);

		[PXOverride]
		public void CopyPasteGetScript(bool isImportSimple, List<Api.Models.Command> script, List<Api.Models.Container> containers,
			CopyPasteGetScriptDelegate baseMethod)
		{
			baseMethod.Invoke(isImportSimple, script, containers);

			Common.Utilities.SetFieldCommandToTheTop(
				script, containers, nameof(Base.CurrentDocument), nameof(SOOrder.BranchID));
		}

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[RestrictVendorByUserBranches]
		public void _(Events.CacheAttached<SOLine.vendorID> e)
		{
		}
	}


	public sealed class SOOrderVisibilityRestriction : PXCacheExtension<SOOrder>
	{
		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<FeaturesSet.visibilityRestriction>();
		}

		#region BranchID
		[Branch(typeof(AccessInfo.branchID), IsDetail = false, TabOrder = 0)]
		[PXFormula(typeof(Switch<
				Case<Where<IsCopyPasteContext, Equal<True>, And<Current2<SOOrder.branchID>, IsNotNull>>, Current2<SOOrder.branchID>,
				Case<Where<SOOrder.customerLocationID, IsNotNull,
						And<Selector<SOOrder.customerLocationID, Location.cBranchID>, IsNotNull>>,
					Selector<SOOrder.customerLocationID, Location.cBranchID>,
				Case<Where<Current2<SOOrder.customerID>, IsNotNull,
						And<Selector<Current2<SOOrder.customerID>, Customer.cOrgBAccountID>, IsNotNull,
						And<Not<Selector<Current2<SOOrder.customerID>, Customer.cOrgBAccountID>, RestrictByBranch<Current2<SOOrder.branchID>>>>>>,
					Null,
				Case<Where<Current2<SOOrder.branchID>, IsNotNull>,
					Current2<SOOrder.branchID>>>>>,
				Current<AccessInfo.branchID>>), KeepIdleSelfUpdates = true)]
		public int? BranchID { get; set; }
		#endregion

		#region CustomerID
		[PXMergeAttributes(Method = MergeMethod.Append)]
		[RestrictCustomerByBranch(typeof(BAccountR.cOrgBAccountID), branchID: typeof(SOOrder.branchID))]
		public int? CustomerID { get; set; }
		#endregion
	}
}
