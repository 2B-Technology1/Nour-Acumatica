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
using PX.Objects.Common;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Objects.GL;
using PX.Objects.CM;

namespace PX.Objects.AP
{
	public class APInvoiceEntryMultipleBaseCurrencies : PXGraphExtension<APInvoiceEntry>
	{
		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<FeaturesSet.multipleBaseCurrencies>();
		}

		protected virtual void _(Events.FieldUpdated<APInvoice.branchID> e)
		{
			BranchAttribute.VerifyFieldInPXCache<APTran, APTran.branchID>(Base, Base.Transactions.Select());
		}

		[PXOverride]
		public virtual void Persist(Action persist)
		{
			BranchAttribute.VerifyFieldInPXCache<APTran, APTran.branchID>(Base, Base.Transactions.Select());

			persist();
		}

		[PXMergeAttributes(Method = MergeMethod.Merge)]
		//Restore the PXRestrictor attribute that is deleted in the APInvoiceEntry graph
		[PXRestrictor(typeof(Where<Current2<APInvoiceMultipleBaseCurrenciesRestriction.branchBaseCuryID>, IsNull,
			Or<Vendor.baseCuryID, IsNull,
			Or<Vendor.baseCuryID, Equal<Current2<APInvoiceMultipleBaseCurrenciesRestriction.branchBaseCuryID>>>>>), null)]
		protected virtual void _(Events.CacheAttached<APInvoice.vendorID> e) {}

		protected virtual void _(Events.FieldVerifying<APInvoice.branchID> e)
		{
			if (e.NewValue == null)
				return;

			Branch branch = PXSelectorAttribute.Select<APInvoice.branchID>(e.Cache, e.Row, (int)e.NewValue) as Branch;
			string vendorBaseCuryID = (string)PXFormulaAttribute.Evaluate<APInvoiceMultipleBaseCurrenciesRestriction.vendorBaseCuryID>(e.Cache, e.Row);

			if (vendorBaseCuryID != null && branch != null
				&& branch.BaseCuryID != vendorBaseCuryID)
			{
				e.NewValue = branch.BranchCD;
				BAccountR vendor = PXSelectorAttribute.Select<APInvoice.vendorID>(e.Cache, e.Row) as BAccountR;
				throw new PXSetPropertyException(Messages.BranchVendorDifferentBaseCury, PXOrgAccess.GetCD(vendor.VOrgBAccountID), vendor.AcctCD);
			}
		}

		protected virtual void _(Events.RowUpdated<APInvoice> e)
		{
			Branch branch = PXSelectorAttribute.Select<APInvoice.branchID>(e.Cache, e.Row, e.Row.BranchID) as Branch;
			PXFieldState vendorBaseCuryID = e.Cache.GetValueExt<APInvoiceMultipleBaseCurrenciesRestriction.vendorBaseCuryID>(e.Row) as PXFieldState;
			if (vendorBaseCuryID?.Value != null && branch != null
				&& branch.BaseCuryID != vendorBaseCuryID.ToString())
			{
				e.Row.BranchID = null;
			}
		}

		protected virtual void _(Events.RowSelected<APInvoice> e)
		{
			APInvoice invoice = e.Row;
			if (invoice == null) return;

			CurrencyInfo info = CurrencyInfoAttribute.GetCurrencyInfo<APInvoice.curyInfoID>(e.Cache, invoice);
			if (info == null) return;

			var docDateState = e.Cache.GetStateExt<APInvoice.docDate>(invoice) as PXFieldState;
			if (docDateState.ErrorLevel <= PXErrorLevel.Warning)
			{
				e.Cache.RaiseExceptionHandling<APInvoice.docDate>(invoice, invoice.DocDate, null);
			}

			if (info?.CuryRate == null || info?.CuryRate == 0.0m)
			{
				e.Cache.RaiseExceptionHandling<APInvoice.docDate>(invoice, invoice.DocDate,
					new PXSetPropertyException(CM.Messages.RateNotFound, PXErrorLevel.Warning));
			}
		}
	}
}
