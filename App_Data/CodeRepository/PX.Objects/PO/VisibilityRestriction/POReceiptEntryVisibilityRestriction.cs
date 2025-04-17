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
using PX.Objects.Common.Formula;
using PX.Objects.CR;
using PX.Objects.CM;
using PX.Objects.CS;
using PX.Objects.GL;
using PX.Objects.IN;
using System.Collections.Generic;

namespace PX.Objects.PO
{
	public class POReceiptEntryVisibilityRestriction : PXGraphExtension<POReceiptEntry>
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
				script, containers, nameof(Base.CurrentDocument), nameof(POReceipt.BranchID));
		}

		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[Branch(typeof(AccessInfo.branchID), IsDetail = false, TabOrder = 0)]
		[PXFormula(typeof(Switch<
			Case<Where<IsCopyPasteContext, Equal<True>, And<Current2<POReceipt.branchID>, IsNotNull>>, Current2<POReceipt.branchID>,
			Case<Where<POReceipt.receiptType, Equal<POReceiptType.transferreceipt>>,
				Selector<POReceipt.siteID, INSite.branchID>,
			Case<Where<POReceipt.vendorLocationID, IsNotNull,
					And<Selector<POReceipt.vendorLocationID, Location.vBranchID>, IsNotNull>>,
				Selector<POReceipt.vendorLocationID, Location.vBranchID>,
			Case<Where<POReceipt.vendorID, IsNotNull,
					And<Not<Selector<POReceipt.vendorID, Vendor.vOrgBAccountID>, RestrictByBranch<Current2<POReceipt.branchID>>>>>,
				Null,
			Case<Where<Current2<POReceipt.branchID>, IsNotNull>,
				Current2<POReceipt.branchID>>>>>>,
			Current<AccessInfo.branchID>>))]
		public virtual void POReceipt_BranchID_CacheAttached(PXCache sender)
		{
		}
	}
}
