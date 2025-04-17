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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.CS.Contracts.Interfaces;
using PX.Data;
using PX.Objects.CS;
using PX.Objects.EP;
using PX.TaxProvider;

namespace PX.Objects.TX
{
	[PXHidden]
	public class EntityWithTaxCalcMode : PXMappedCacheExtension
	{
		public abstract class taxCalcMode : IBqlField { }
		public string TaxCalcMode { get; set; }
	}

	public abstract class TaxCalculationModeExtension<TGraph, TPrimary> : PXGraphExtension<TGraph>
		where TGraph : PXGraph
		where TPrimary : class, IBqlTable, new()
	{
		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<PX.Objects.CS.FeaturesSet.netGrossEntryMode>();
		}

		protected class EntityMapping : IBqlMapping
		{
			public Type Extension => typeof(EntityWithTaxCalcMode);
			protected Type _table;
			public Type Table => _table;

			public EntityMapping(Type table)
			{
				_table = table;
			}
			public Type TaxCalcMode = typeof(EntityWithTaxCalcMode.taxCalcMode);
		}
		protected virtual EntityMapping GetEntityMapping() { return new EntityMapping(typeof(TPrimary)); }

		public PXSelectExtension<EntityWithTaxCalcMode> Entity;

		public IEnumerable<Tax> Taxes { get { return GetTaxes(); } }

		protected abstract IEnumerable<Tax> GetTaxes();

		public virtual bool SkipValidation { get { return false; } }

		public virtual void _(Events.RowPersisting<EntityWithTaxCalcMode> e)
		{
			if (SkipValidation || string.IsNullOrEmpty(e.Row?.TaxCalcMode) || Taxes.Count() == 0) return;

			try
			{
				VerifyTransactions(e.Row.TaxCalcMode, Taxes);
			}
			catch (PXException ex)
			{
				e.Cache.RaiseExceptionHandling<EntityWithTaxCalcMode.taxCalcMode>(e.Row, e.Row.TaxCalcMode, new PXSetPropertyException(ex.Message));
				throw ex;
			}
		}

		protected static void VerifyTransactions(string calcMode, IEnumerable<Tax> taxes)
		{
			if (calcMode == TaxCalculationMode.Gross)
			{
				if (taxes.Any(tax => tax.TaxType == CSTaxType.Use))
				{
					throw new PXException(Messages.GrossModeIsNotAvailableForUseType, Messages.Use);
				}

				if (taxes.Any(tax => tax.ReverseTax == true))
				{
					throw new PXException(Messages.GrossModeIsNotAvailableForReversedTax);
				}
			}
			else if (calcMode == TaxCalculationMode.Net)
			{
				if (taxes.Any(tax => tax.TaxType == CSTaxType.Withholding))
				{
					throw new PXException(Messages.NetModeIsNotAvailableForWithholdingType, Messages.Withholding);
				}
			}
		}
	}

	public class APInvoiceEntryTaxCalcModeExt : TaxCalculationModeExtension<AP.APInvoiceEntry, AP.APInvoice>
	{
		protected override IEnumerable<Tax> GetTaxes()
		{
			return Base.Taxes.Select().RowCast<Tax>();
		}
	}
	public class APQuickCheckEntryTaxCalcModeExt : TaxCalculationModeExtension<AP.APQuickCheckEntry, AP.Standalone.APQuickCheck>
	{
		protected override IEnumerable<Tax> GetTaxes()
		{
			return Base.Taxes.Select().RowCast<Tax>();
		}
	}
	public class CATranEntryTaxCalcModeExt : TaxCalculationModeExtension<CA.CATranEntry, CA.CAAdj>
	{
		protected override IEnumerable<Tax> GetTaxes()
		{
			return Base.Taxes.Select().RowCast<Tax>();
		}
	}
	public class ExpenseClaimDetailEntryTaxCalcModeExt : TaxCalculationModeExtension<EP.ExpenseClaimDetailEntry, EP.EPExpenseClaimDetails>
	{
		protected override IEnumerable<Tax> GetTaxes()
		{
			return Base.Taxes.Select().RowCast<Tax>();
		}
	}
	public class ExpenseClaimEntryTaxCalcModeExt : TaxCalculationModeExtension<EP.ExpenseClaimEntry, EP.EPExpenseClaim>
	{
		protected override IEnumerable<Tax> GetTaxes()
		{
			return Base.Taxes.Select().RowCast<Tax>();
		}

		public override void _(Events.RowPersisting<EntityWithTaxCalcMode> e)
		{
			if (SkipValidation || Taxes.Count() == 0) return;

			PXSetPropertyException pex = null;

			foreach (EPExpenseClaimDetails row in Base.ExpenseClaimDetails.Select())
			{
				try
				{
					VerifyTransactions(row.TaxCalcMode, Base.Tax_Rows.Select(row.ClaimDetailID).RowCast<Tax>());
				}
				catch (PXException ex)
				{
					pex = new PXSetPropertyException(ex.Message);
					Base.ExpenseClaimDetails.Cache.RaiseExceptionHandling<EPExpenseClaimDetails.curyTaxTotal>(row, row.CuryTaxTotal, pex);
				}
			}

			if (pex != null)
				throw pex;
		}

		public virtual void _(Events.RowUpdated<EPExpenseClaimDetails> e)
		{
			EPExpenseClaimDetails row = e.Row;
			EPExpenseClaimDetails oldrow = e.OldRow;

			if (!e.Cache.ObjectsEqual<
					EPExpenseClaimDetails.refNbr, 
					EPExpenseClaimDetails.taxCategoryID, 
					EPExpenseClaimDetails.taxCalcMode,
					EPExpenseClaimDetails.taxZoneID
				>(e.Row, e.OldRow))
			{
				try
				{
					VerifyTransactions(row.TaxCalcMode, Base.Tax_Rows.Select(row.ClaimDetailID).RowCast<Tax>());
				}
				catch (PXException ex)
				{
					e.Cache.RaiseExceptionHandling<EPExpenseClaimDetails.curyTaxTotal>(row, row.CuryTaxTotal, new PXSetPropertyException(ex.Message));
				}
			}
		}
	}
}
