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

using CommonServiceLocator;
using PX.Data;
using PX.Objects.CM.Extensions;
using PX.Objects.Extensions.MultiCurrency;
using System;

namespace PX.Objects.CA.MultiCurrency
{
	// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
	public sealed class CABankTransactionsMaintMultiCurrency : CABankTransactionsBaseMultiCurrency<CABankTransactionsMaint>
	{
		#region SettingUp
		protected override string Module => CurrentModule;
		private string CurrentModule = GL.BatchModule.CA;

		protected override PXSelectBase[] GetChildren()
		{
			return new PXSelectBase[]
				{
					Base.Details,
					Base.TranSplit,
					Base.Adjustments,
					Base.caTran,
					Base.TranMatch,
					Base.Taxes,
					Base.TaxTrans
				};
		}

		#endregion

		public CurrencyInfo CreateCurrencyInfo()
		{
			CurrencyInfo info = (CurrencyInfo)currencyinfo.Cache.Insert(new CurrencyInfo());
			base.defaultCurrencyRate(currencyinfo.Cache, info, true, false);
			return info;
		}

		#region CABankTran

		protected void _(Events.FieldUpdated<CABankTran, CABankTran.origModule> e)
		{
			if(e.NewValue != null)
			{
				CurrentModule = e.Row.OrigModule;
				SourceFieldUpdated<CABankTran.curyInfoID, CABankTran.curyID, CABankTran.tranDate>(e.Cache, e.Row);
			}
		}

		protected override void _(Events.FieldDefaulting<CurrencyInfo, CurrencyInfo.curyRateTypeID> e)
		{
			if (PXAccess.FeatureInstalled<CS.FeaturesSet.multicurrency>())
			{
				CurySource source = CurrentSourceSelect();
				if (!string.IsNullOrEmpty(source?.CuryRateTypeID))
				{
					e.NewValue = source.CuryRateTypeID;
					e.Cancel = true;
				}
				else if (e.Row != null && !String.IsNullOrEmpty(Base.Details.Current?.OrigModule))
				{
					e.NewValue = ServiceLocator.Current.GetInstance<Func<PXGraph, IPXCurrencyService>>()(Base).DefaultRateTypeID(Base.Details.Current?.OrigModule);
					e.Cancel = true;
				}
			}
		}

		#endregion

		#region Details

		protected void _(Events.RowUpdating<CABankTranDetail> e, PXRowUpdating baseMethod)
		{
			UpdateNewTranDetailCuryTranAmtOrCuryUnitPrice(e.Cache, e.Row, e.NewRow);
			baseMethod(e.Cache, e.Args);
		}

		protected void _(Events.RowInserting<CABankTranAdjustment> e)
		{
			InsertNewCurrencyInfoWithoutCuryID<CABankTranAdjustment.adjdCuryInfoID>(e.Cache, e.Row);
			InsertNewCurrencyInfoWithoutCuryID<CABankTranAdjustment.adjgCuryInfoID>(e.Cache, e.Row);
		}		

		protected void _(Events.FieldUpdated<CABankTranAdjustment.adjdCuryRate> e)
		{
			CABankTranAdjustment adj = (CABankTranAdjustment)e.Row;

			CurrencyInfo pay_info = PXSelect<
				CurrencyInfo,
				Where<CurrencyInfo.curyInfoID, Equal<Current<CABankTranAdjustment.adjgCuryInfoID>>>>
				.SelectSingleBound(Base, new object[] { e.Row });
			CurrencyInfo vouch_info = PXSelect<
				CurrencyInfo,
				Where<CurrencyInfo.curyInfoID, Equal<Current<CABankTranAdjustment.adjdCuryInfoID>>>>
				.SelectSingleBound(Base, new object[] { e.Row });

			decimal payment_docbal = (decimal)adj.CuryAdjgAmt;
			decimal discount_docbal = (decimal)adj.CuryAdjgDiscAmt;
			decimal invoice_amount;

			if (string.Equals(pay_info.CuryID, vouch_info.CuryID) && adj.AdjdCuryRate != 1m)
			{
				adj.AdjdCuryRate = 1m;
				//vouch_info.SetCuryEffDate(currencyinfo.Cache, DetailsForPaymentCreation.Current.TranDate);
				vouch_info.CuryEffDate = Base.DetailsForPaymentCreation.Current.TranDate;
				defaultCurrencyRate(e.Cache, vouch_info, true, false);
			}
			else if (string.Equals(vouch_info.CuryID, vouch_info.BaseCuryID))
			{
				adj.AdjdCuryRate = pay_info.CuryMultDiv == "M" ? 1 / pay_info.CuryRate : pay_info.CuryRate;
			}
			else
			{
				vouch_info.CuryRate = adj.AdjdCuryRate;
				vouch_info.RecipRate = Math.Round(1m / (decimal)adj.AdjdCuryRate, 8, MidpointRounding.AwayFromZero);
				vouch_info.CuryMultDiv = "M";

				payment_docbal = vouch_info.CuryConvBase((decimal)adj.CuryAdjdAmt);
				discount_docbal = vouch_info.CuryConvBase((decimal)adj.CuryAdjdDiscAmt);
				invoice_amount = vouch_info.CuryConvBase((decimal)adj.CuryAdjdAmt + (decimal)adj.CuryAdjdDiscAmt);

				vouch_info.CuryRate = Math.Round((decimal)adj.AdjdCuryRate * (pay_info.CuryMultDiv == "M" ? (decimal)pay_info.CuryRate : 1m / (decimal)pay_info.CuryRate), 8, MidpointRounding.AwayFromZero);
				vouch_info.RecipRate = Math.Round((pay_info.CuryMultDiv == "M" ? 1m / (decimal)pay_info.CuryRate : (decimal)pay_info.CuryRate) / (decimal)adj.AdjdCuryRate, 8, MidpointRounding.AwayFromZero);

				if (payment_docbal + discount_docbal != invoice_amount)
					discount_docbal += invoice_amount - discount_docbal - payment_docbal;
			}

			Base.Caches[typeof(CurrencyInfo)].MarkUpdated(vouch_info);

			if (payment_docbal != (decimal)adj.CuryAdjgAmt)
				e.Cache.SetValue<CABankTranAdjustment.curyAdjgAmt>(e.Row, payment_docbal);

			if (discount_docbal != (decimal)adj.CuryAdjgDiscAmt)
				e.Cache.SetValue<CABankTranAdjustment.curyAdjgDiscAmt>(e.Row, discount_docbal);

			Base.UpdateBalance((CABankTranAdjustment)e.Row, true);
		}

		protected void _(Events.FieldSelecting<CABankTranAdjustment, CABankTranAdjustment.adjdCuryID> e)
		{
			e.ReturnValue = CuryIDFieldSelecting<CABankTranAdjustment.adjdCuryInfoID>(e.Cache, e.Row);
		}

		#endregion
	}
}
