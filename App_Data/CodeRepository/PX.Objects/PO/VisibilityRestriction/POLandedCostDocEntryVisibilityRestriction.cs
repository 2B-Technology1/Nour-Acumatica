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
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Objects.GL;
using PX.Objects.PO.LandedCosts;
using System.Collections.Generic;

namespace PX.Objects.PO
{
	public class POLandedCostDocEntryVisibilityRestriction: PXGraphExtension<POLandedCostDocEntry>
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
				script, containers, nameof(Base.CurrentDocument), nameof(POLandedCostDoc.BranchID));
		}

		public override void Initialize()
		{
			base.Initialize();

			Base.poReceiptSelectionView.Join<InnerJoin<BAccount2, On<POReceipt.vendorID.IsEqual<BAccount2.bAccountID>>>>();
			Base.poReceiptSelectionView.WhereAnd<Where<BAccount2.vOrgBAccountID, RestrictByUserBranches<Current<AccessInfo.userName>>>>();

			Base.poReceiptLinesSelectionView.Join<InnerJoin<BAccount2, On<POReceiptLineAdd.vendorID.IsEqual<BAccount2.bAccountID>>>>();
			Base.poReceiptLinesSelectionView.WhereAnd<Where<BAccount2.vOrgBAccountID, RestrictByUserBranches<Current<AccessInfo.userName>>>>();
		}
	}
}
