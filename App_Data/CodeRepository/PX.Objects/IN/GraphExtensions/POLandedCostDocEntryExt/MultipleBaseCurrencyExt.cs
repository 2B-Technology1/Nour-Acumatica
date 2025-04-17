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
using PX.Objects.Common.Extensions;
using PX.Objects.CS;
using PX.Objects.CM;
using PX.Objects.CR;
using PX.Objects.GL;
using PX.Objects.PO;
using PX.Objects.PO.LandedCosts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Common;

namespace PX.Objects.IN.GraphExtensions.POLandedCostDocEntryExt
{
	public class MultipleBaseCurrencyExt :
		MultipleBaseCurrencyExtBase<POLandedCostDocEntry, POLandedCostDoc, POLandedCostReceiptLine,
			POLandedCostDoc.branchID, POLandedCostReceiptLine.branchID, POLandedCostReceiptLine.siteID>
	{
		public static bool IsActive()
			=> PXAccess.FeatureInstalled<FeaturesSet.multipleBaseCurrencies>();

		public override void Initialize()
		{
			base.Initialize();

			Base.poReceiptSelectionView.Join<InnerJoin<Branch, On<POReceipt.FK.Branch>>>();
			Base.poReceiptSelectionView.WhereAnd<Where<Branch.baseCuryID, EqualBaseCuryID<Current2<POLandedCostDoc.branchID>>>>();

			Base.poReceiptLinesSelectionView.Join<InnerJoin<Branch, On<POReceiptLineAdd.branchID.IsEqual<Branch.branchID>>>>();
			Base.poReceiptLinesSelectionView.WhereAnd<Where<Branch.baseCuryID, EqualBaseCuryID<Current2<POLandedCostDoc.branchID>>>>();
		}

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXRestrictor(typeof(Where<Current2<POLandedCostDoc.branchID>, IsNull,
			Or<Vendor.baseCuryID, EqualBaseCuryID<Current2<POLandedCostDoc.branchID>>,
			Or<Vendor.baseCuryID, IsNull>>>),
			Messages.CustomerOrVendorHasDifferentBaseCurrency, typeof(Vendor.vOrgBAccountID), typeof(Vendor.acctCD))]
		protected virtual void _(Events.CacheAttached<POLandedCostDoc.vendorID> e)
		{
		}

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXRestrictor(typeof(Where<Vendor.baseCuryID, EqualBaseCuryID<Current2<POLandedCostDoc.branchID>>,
			Or<Vendor.baseCuryID, IsNull>>),
			Messages.CustomerOrVendorHasDifferentBaseCurrency, typeof(Vendor.vOrgBAccountID), typeof(Vendor.acctCD))]
		protected virtual void _(Events.CacheAttached<POReceiptFilter.vendorID> e)
		{
		}

		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[Branch(typeof(Coalesce<
			Search<Location.vBranchID, Where<Location.bAccountID, Equal<Current<POLandedCostDoc.vendorID>>, And<Location.locationID, Equal<Current<POLandedCostDoc.vendorLocationID>>>>>,
			Search<Branch.branchID, Where<Branch.branchID, Equal<Current2<POLandedCostDoc.branchID>>>>,
			Search<Branch.branchID, Where<Branch.branchID, Equal<Current<AccessInfo.branchID>>>>>), IsDetail = false)]
		protected virtual void _(Events.CacheAttached<POLandedCostDoc.branchID> e)
		{
		}

		protected virtual void _(Events.RowPersisting<POLandedCostDoc> e)
		{
			if (e.Operation.Command().IsNotIn(PXDBOperation.Insert, PXDBOperation.Update))
				return;

			e.Cache.VerifyFieldAndRaiseException<POLandedCostDoc.branchID>(e.Row);
		}

		protected override PXSelectBase<POLandedCostReceiptLine> GetTransactionView()
			=> Base.ReceiptLines;
	}
}
