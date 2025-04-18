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
using System.Collections;
using PX.Data;
using PX.Objects.CM.Extensions;
using PX.Objects.Extensions.MultiCurrency;

namespace PX.Objects.CA.MultiCurrency
{
	public sealed class CashTransferEntryMultiCurrency : MultiCurrencyGraph<CashTransferEntry, CATransfer>
	{
		#region SettingUp
		protected override string Module => GL.BatchModule.CA;

		protected override CurySourceMapping GetCurySourceMapping()
		{
			return new CurySourceMapping(typeof(CashAccount))
			{
				CuryID = typeof(CashAccount.curyID),
				CuryRateTypeID = typeof(CashAccount.curyRateTypeID),
				AllowOverrideCury = typeof(CashAccount.allowOverrideCury),
				AllowOverrideRate = typeof(CashAccount.allowOverrideRate),
			};
		}
		protected override DocumentMapping GetDocumentMapping()
		{
			return new DocumentMapping(typeof(CATransfer))
			{
				CuryID = typeof(CATransfer.inCuryID),
				CuryInfoID = typeof(CATransfer.inCuryInfoID),
				BAccountID = typeof(CATransfer.inAccountID),
				DocumentDate = typeof(CATransfer.inDate),
				BranchID = typeof(CATransfer.inBranchID)
			};
		}
		protected override PXSelectBase[] GetChildren()
		{
			return new PXSelectBase[] { Base.Transfer, Base.Expenses, Base.ExpenseTaxes, Base.ExpenseTaxTrans };
		}
		protected int? AccountProcessing;
		protected override CurySource CurrentSourceSelect()
		{
			return CurySource.Select(AccountProcessing);
		}

		[Obsolete(Common.Messages.ItemIsObsoleteAndWillBeRemoved2022R2)]
		public override object GetRow(PXCache sender, object row)
		{
			return row ?? sender.Current;
		}
		#endregion
		#region CATrasfer
		public PXSelect<CurrencyInfo>
					currencyinfoout;
		protected IEnumerable currencyInfoOut()
		{
			CurrencyInfo info = PXSelect<CurrencyInfo,
					Where<CurrencyInfo.curyInfoID, Equal<Current<CATransfer.outCuryInfoID>>>>
					.Select(Base);
			if (info != null)
			{
				info.IsReadOnly = (!Base.UnattendedMode && !Documents.AllowUpdate);
				yield return info;
			}
			yield break;
		}
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[CurrencyInfo(typeof(CurrencyInfo.curyInfoID))]
		protected void _(Events.CacheAttached<CATransfer.outCuryInfoID> e)
		{
		}
		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXCustomizeBaseAttribute(typeof(PXUIFieldAttribute), nameof(PXUIFieldAttribute.DisplayName), "Currency")]
		protected void _(Events.CacheAttached<CATransfer.outCuryID> e)
		{
		}
		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXCustomizeBaseAttribute(typeof(PXUIFieldAttribute), nameof(PXUIFieldAttribute.DisplayName), "Currency")]
		protected void _(Events.CacheAttached<CATransfer.inCuryID> e)
		{
		}
		protected override void _(Events.RowInserting<Document> e)
		{
			AccountProcessing = (int?)e.Cache.GetValue<CATransfer.inAccountID>(e.Row);
			base._(e);
		}
		protected void _(Events.RowInserting<CATransfer> e)
		{
			AccountProcessing = e.Row.OutAccountID;
			DocumentRowInserting<CATransfer.outCuryInfoID, CATransfer.outCuryID>(e.Cache, e.Row);
		}
		protected void _(Events.FieldUpdated<CATransfer, CATransfer.inAccountID> e)
		{
			AccountProcessing = e.Row.InAccountID;
			SourceFieldUpdated<CATransfer.inCuryInfoID, CATransfer.inCuryID, CATransfer.inDate>(e.Cache, e.Row);
		}
		protected void _(Events.FieldUpdated<CATransfer, CATransfer.outAccountID> e)
		{
			e.Cache.SetDefaultExt<CATransfer.outBranchID>(e.Row);
			if(this.Documents.Current != null)
				this.Documents.Current.BranchID = e.Row.OutBranchID;
			AccountProcessing = e.Row.OutAccountID;
			SourceFieldUpdated<CATransfer.outCuryInfoID, CATransfer.outCuryID, CATransfer.outDate>(e.Cache, e.Row);
			AccountProcessing = e.Row.InAccountID;
			SourceFieldUpdated<CATransfer.inCuryInfoID, CATransfer.inCuryID, CATransfer.inDate>(e.Cache, e.Row);
		}
		protected void _(Events.FieldSelecting<CATransfer, CATransfer.outCuryID> e)
		{
			e.ReturnValue = CuryIDFieldSelecting<CATransfer.outCuryInfoID>(e.Cache, e.Row);
		}
		protected void _(Events.FieldUpdated<CATransfer, CATransfer.outDate> e)
		{
			DateFieldUpdated<CATransfer.outCuryInfoID, CATransfer.outDate>(e.Cache, e.Row);
		}
		protected void _(Events.FieldSelecting<CATransfer, CATransferMultiCurrency.outCuryRate> e)
		{
			e.ReturnValue = CuryRateFieldSelecting<CATransfer.outCuryInfoID>(e.Cache, e.Row);
		}
		[PXNonInstantiatedExtension]
		public sealed class CATransferMultiCurrency : PXCacheExtension<CATransfer>
		{
			public abstract class outCuryRate : PX.Data.BQL.BqlDecimal.Field<outCuryRate> { }
			[PXDecimal]
			public decimal? OutCuryRate
			{
				get;
				set;
			}
		}
		protected override void _(Events.RowUpdating<Document> e)
		{
			AccountProcessing = (int?)e.Cache.GetValue<CATransfer.inAccountID>(e.Row);
			base._(e);
		}
		protected void _(Events.RowUpdating<CATransfer> e)
		{
			AccountProcessing = e.Row.OutAccountID;
			DocumentRowUpdating<CATransfer.outCuryInfoID, CATransfer.outCuryID>(e.Cache, e.NewRow);
			if ((e.NewRow.TranOut != e.Row.TranOut || e.NewRow.InDate != e.Row.InDate || e.NewRow.InCuryID != e.Row.InCuryID || e.NewRow.InAccountID != e.Row.InAccountID) && e.Cache.Graph.IsCopyPasteContext == false)
			{
				CalcTranIn(e.NewRow);
			}
		}
		protected override void _(Events.RowUpdated<CurrencyInfo> e)
		{
			base._(e);
			if (Base.Transfer.Current == null) return;
			else if (
				Base.Transfer.Current.InCuryInfoID == e.Row.CuryInfoID || 
				Base.Transfer.Current.OutCuryInfoID == e.Row.CuryInfoID
				)
			{
				CalcTranIn(Base.Transfer.Current);
				Base.Transfer.Cache.MarkUpdated(Base.Transfer.Current);
			}
		}

		protected void CalcTranIn(CATransfer row)
		{
			if (row.OutCuryID == row.InCuryID)
			{
				row.CuryTranIn = row.CuryTranOut;
			}
			else
			{
				try
				{
					CurrencyInfo currencyInfo = GetDefaultCurrencyInfo();

					row.CuryTranIn = currencyInfo.CuryConvCury(row.TranOut ?? decimal.Zero);
					row.TranIn = currencyInfo.CuryConvBase(row.CuryTranIn ?? decimal.Zero);
				}
				catch (CM.PXRateNotFoundException)
				{
				}
			}
		}
		protected override void _(Events.RowSelected<Document> e)
		{
			base._(e);
			PXUIFieldAttribute.SetEnabled<Document.curyID>(e.Cache, e.Row, false);
		}
		protected void _(Events.RowSelected<CATransfer> e)
		{
			bool msFeatureInstalled = PXAccess.FeatureInstalled<CS.FeaturesSet.multicurrency>();
			PXUIFieldAttribute.SetVisible<CATransfer.inCuryID>(e.Cache, e.Row, msFeatureInstalled);
			PXUIFieldAttribute.SetVisible<CATransfer.outCuryID>(e.Cache, e.Row, msFeatureInstalled);
			PXUIFieldAttribute.SetVisible<CATransfer.rGOLAmt>(e.Cache, e.Row, msFeatureInstalled);

			PXUIFieldAttribute.SetEnabled<CATransfer.curyTranOut>(e.Cache, e.Row, Base.Accessinfo.CuryViewState == false);
			PXUIFieldAttribute.SetEnabled<CATransfer.curyTranIn>(e.Cache, e.Row, Base.Accessinfo.CuryViewState == false);

			if (e.Row?.OutCuryID == e.Row?.InCuryID)
			{
				PXUIFieldAttribute.SetEnabled<CATransfer.curyTranIn>(e.Cache, e.Row, false);
			}
		}
		protected void _(Events.RowPersisting<CATransfer> e)
		{
			CurrencyInfo currencyInfoOut = CurrencyInfoAttribute.GetCurrencyInfo< CATransfer.outCuryInfoID>(e.Cache, e.Row)??
				PXSelect<CurrencyInfo,
				Where<CurrencyInfo.curyInfoID, Equal<Required<CATransfer.outCuryInfoID>>,
				And<CurrencyInfo.curyID, Equal<Required<CATransfer.outCuryID>>>>>
				.Select(Base, e.Row.OutCuryInfoID, e.Row.OutCuryInfoID);
			CurrencyInfo currencyInfoIn = CurrencyInfoAttribute.GetCurrencyInfo<CATransfer.inCuryInfoID>(e.Cache, e.Row) ??
				PXSelect<CurrencyInfo,
				Where<CurrencyInfo.curyInfoID, Equal<Required<CATransfer.inCuryInfoID>>,
				And<CurrencyInfo.curyID, Equal<Required<CATransfer.inCuryID>>>>>
				.Select(Base, e.Row.InCuryInfoID, e.Row.InCuryInfoID);

			if (currencyInfoOut?.CuryRate == null)
			{
				e.Cache.RaiseExceptionHandling<CATransfer.outCuryID>(e.Row, e.Row.OutCuryID, new PXSetPropertyException(CM.Messages.RateNotFound));
			}
			if (currencyInfoIn?.CuryRate == null)
			{
				e.Cache.RaiseExceptionHandling<CATransfer.inCuryID>(e.Row, e.Row.InCuryID, new PXSetPropertyException(CM.Messages.RateNotFound));
			}
		}
		[PXOverride]
		public void SwapInOutFields(CATransfer currentTransfer, CATransfer reverseTransfer, Action<CATransfer, CATransfer> del)
		{
			del(currentTransfer, reverseTransfer);
			long? id = reverseTransfer.OutCuryInfoID;
			reverseTransfer.OutCuryInfoID = reverseTransfer.InCuryInfoID;
			reverseTransfer.InCuryInfoID = id;
			currencyinfo.Current = null;
			AccountProcessing = null;
		}
		#endregion
		#region CAExpense
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[CurrencyInfo(typeof(CurrencyInfo.curyInfoID))]
		protected void _(Events.CacheAttached<CAExpense.curyInfoID> e)
		{
		}
		protected void _(Events.RowInserting<CAExpense> e)
		{
			AccountProcessing = e.Row.CashAccountID;
			DocumentRowInserting<CAExpense.curyInfoID, CAExpense.curyID>(e.Cache, e.Row);
		}
		protected void _(Events.RowUpdating<CAExpense> e)
		{
			AccountProcessing = e.Row.CashAccountID;
			DocumentRowUpdating<CAExpense.curyInfoID, CAExpense.curyID>(e.Cache, e.Row);
		}
		protected void _(Events.FieldUpdated<CAExpense, CAExpense.cashAccountID> e)
		{
			AccountProcessing = e.Row.CashAccountID;
			SourceFieldUpdated<CAExpense.curyInfoID, CAExpense.curyID, CAExpense.tranDate>(e.Cache, e.Row);
		}
		protected void _(Events.FieldSelecting<CAExpense, CAExpense.curyID> e)
		{
			e.ReturnValue = CuryIDFieldSelecting<CAExpense.curyInfoID>(e.Cache, e.Row);
		}
		protected void _(Events.FieldUpdated<CAExpense, CAExpense.adjCuryRate> e)
		{
			CurrencyInfo info = PXSelect<CurrencyInfo,
				Where<CurrencyInfo.curyInfoID, Equal<Required<CAExpense.curyInfoID>>>>
				.Select(Base, e.Row.CuryInfoID);
			if (e.Row.CuryID != info.BaseCuryID)
			{
				info.CuryRate = e.Row.AdjCuryRate;
				info.RecipRate = Math.Round(1m / (decimal)e.Row.AdjCuryRate, 8, MidpointRounding.AwayFromZero);
				info.CuryMultDiv = "M";
				info.CuryRate = Math.Round((decimal)e.Row.AdjCuryRate, 8, MidpointRounding.AwayFromZero);
				info.RecipRate = Math.Round((decimal)e.Row.AdjCuryRate, 8, MidpointRounding.AwayFromZero);
				if (currencyinfo.Cache.GetStatus(info) == PXEntryStatus.Notchanged || currencyinfo.Cache.GetStatus(info) == PXEntryStatus.Held)
				{
					currencyinfo.Cache.SetStatus(info, PXEntryStatus.Updated);
				}
			}
		}
		protected void _(Events.FieldVerifying<CAExpense, CAExpense.adjCuryRate> e)
		{
			if ((decimal?)e.NewValue <= 0m)
			{
				throw new PXSetPropertyException(CS.Messages.Entry_GT, ((int)0).ToString());
			}
		}
		protected void _(Events.FieldSelecting<CAExpense, CAExpense.adjCuryRate> e)
		{
			e.ReturnValue = CuryRateFieldSelecting<CAExpense.curyInfoID>(e.Cache, e.Row);
		}
		protected void _(Events.RowSelected<CAExpense> e)
		{
			if (e.Row != null)
			{
				CurrencyInfo info = PXSelect<CurrencyInfo,
					Where<CurrencyInfo.curyInfoID, Equal<Required<CAExpense.curyInfoID>>>>
					.Select(Base, e.Row.CuryInfoID);
				PXUIFieldAttribute.SetEnabled<CAExpense.adjCuryRate>(e.Cache, e.Row, e.Row.CuryID != info?.BaseCuryID);
				PXUIFieldAttribute.SetEnabled<CAExpense.curyTaxableAmt>(e.Cache, e.Row, Base.Accessinfo.CuryViewState == false);
			}
		}

		protected void _(Events.FieldUpdated<CAExpense, CAExpense.tranDate> e)
		{
			AccountProcessing = e.Row.CashAccountID;
			DateFieldUpdated<CAExpense.curyInfoID, CAExpense.tranDate>(e.Cache, e.Row);
		}
		#endregion
	}
}
