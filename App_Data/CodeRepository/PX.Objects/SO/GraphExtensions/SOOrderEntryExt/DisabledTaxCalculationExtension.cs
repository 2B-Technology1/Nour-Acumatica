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

using System.Linq;
using PX.Common;
using PX.Data;
using PX.Objects.IN;
using PX.Objects.TX;

namespace PX.Objects.SO.GraphExtensions.SOOrderEntryExt
{
	// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
	public class DisabledTaxCalculationExtension : PXGraphExtension<SOOrderEntry>
	{
		protected virtual void _(Events.RowSelected<SOOrder> e)
		{
			if (e.Row == null)
				return;

			if (e.Row.DisableAutomaticTaxCalculation == true)
			{
				if (Base.Taxes.Select().ToList().Count > 0)
				{
					PXUIFieldAttribute.SetWarning<SOOrder.curyTaxTotal>(e.Cache, e.Row, e.Row.IsManualTaxesValid == false ? AR.Messages.TaxIsNotUptodate : null);
				}
				else
				{
					PXUIFieldAttribute.SetWarning<SOOrder.curyTaxTotal>(e.Cache, e.Row, e.Row.IsManualTaxesValid == false ? Messages.TaxCalculationDisabled : null);
				}
			}
		}

		protected virtual void _(Events.RowUpdated<SOTaxTran> e)
		{
			if (e.Row == null)
				return;

			if (Base.Document.Current?.DisableAutomaticTaxCalculation == true)
			{
				Base.Document.Current.IsManualTaxesValid = true;
				Base.Document.Cache.MarkUpdated(Base.Document.Current);
			}
		}

		protected virtual void _(Events.RowUpdated<SOOrder> e)
		{
			if (!e.Cache.ObjectsEqual<SOOrder.disableAutomaticTaxCalculation>(e.Row, e.OldRow))
				e.Row.IsManualTaxesValid = true;

			if (e.Row.DisableAutomaticTaxCalculation == true)
			{
				if (!e.Cache.ObjectsEqual<
					SOOrder.orderDate,
					SOOrder.taxZoneID,
					SOOrder.customerID,
					SOOrder.shipAddressID,
					SOOrder.willCall,
					SOOrder.curyFreightTot,
					SOOrder.freightTaxCategoryID>(e.Row, e.OldRow) ||
					!e.Cache.ObjectsEqual<
					SOOrder.externalTaxExemptionNumber,
					SOOrder.avalaraCustomerUsageType,
					SOOrder.curyDiscTot,
					SOOrder.branchID>(e.Row, e.OldRow))
				{
					e.Row.IsManualTaxesValid = false;
				}

				bool taxZoneChanged = !e.Cache.ObjectsEqual<SOOrder.taxZoneID>(e.Row, e.OldRow);

				if (taxZoneChanged)
				{
					foreach (SOLine soline in Base.Transactions.Select())
					{
						Base.Transactions.Cache.SetValue<SOLine.taxZoneID>(soline, e.Row?.TaxZoneID); //no need to call any additional logic in this case
						Base.Transactions.Cache.MarkUpdated(soline);
					}
				}
			}
		}

		protected virtual void _(Events.RowInserted<SOLine> e)
		{
			InvalidateManualTaxes(Base.Document.Current);
		}

		protected virtual void _(Events.RowUpdated<SOLine> e)
		{
			if (Base.Document.Current?.DisableAutomaticTaxCalculation == true)
			{
				if (!e.Cache.ObjectsEqual<
						SOLine.avalaraCustomerUsageType,
						SOLine.salesAcctID,
						SOLine.inventoryID,
						SOLine.tranDesc,
						SOLine.lineAmt,
						SOLine.orderDate,
						SOLine.taxCategoryID,
						SOLine.siteID
					>(e.Row, e.OldRow) ||
					(e.Row.POSource == INReplenishmentSource.DropShipToOrder) != (e.OldRow.POSource == INReplenishmentSource.DropShipToOrder))
				{
					InvalidateManualTaxes(Base.Document.Current);
				}
			}
		}

		protected virtual void _(Events.RowDeleted<SOLine> e)
		{
			if (Base.Document.Current?.DisableAutomaticTaxCalculation == true)
			{
				InvalidateManualTaxes(Base.Document.Current);
				Base.Document.Cache.MarkUpdated(Base.Document.Current);
			}
		}

		#region SOShippingAddress Events
		protected virtual void _(Events.RowUpdated<SOShippingAddress> e)
		{
			if (e.Row == null) return;
			if (e.Cache.ObjectsEqual<SOShippingAddress.postalCode, SOShippingAddress.countryID,
					SOShippingAddress.state, SOShippingAddress.latitude, SOShippingAddress.longitude>(e.Row, e.OldRow) == false)
			{
				InvalidateManualTaxes(Base.Document.Current);
			}
		}

		protected virtual void _(Events.RowInserted<SOShippingAddress> e)
		{
			if (e.Row == null) return;
			InvalidateManualTaxes(Base.Document.Current);
		}

		protected virtual void _(Events.RowDeleted<SOShippingAddress> e)
		{
			if (e.Row == null) return;
			InvalidateManualTaxes(Base.Document.Current);
		}

		protected virtual void _(Events.FieldUpdating<SOShippingAddress, SOShippingAddress.overrideAddress> e)
		{
			if (e.Row == null) return;
			InvalidateManualTaxes(Base.Document.Current);
		}
		#endregion

		public void InvalidateManualTaxes(SOOrder order)
		{
			if (order?.DisableAutomaticTaxCalculation == true)
				order.IsManualTaxesValid = false;
		}

		protected virtual void _(Events.FieldUpdated<SOOrder, SOOrder.disableAutomaticTaxCalculation> e)
		{
			if (e.Row == null) return;

			foreach (SOLine line in Base.Transactions.Select())
			{
				line.DisableAutomaticTaxCalculation = e.Row.DisableAutomaticTaxCalculation ?? false;
				Base.Transactions.Update(line);
			}

			if (e.Row.DisableAutomaticTaxCalculation != true)
			{
				TaxAttribute.SetTaxCalc<SOLine.taxCategoryID>(Base.Transactions.Cache, null, TaxCalc.ManualLineCalc | TaxCalc.RecalculateAlways);
				Base.Document.Cache.RaiseFieldUpdated<SOOrder.taxZoneID>(e.Row, null);
			}
		}

		protected virtual void _(Events.FieldVerifying<SOOrder, SOOrder.disableAutomaticTaxCalculation> e)
		{
			if (e.Row == null) return;

			if (((bool?)e.NewValue) == true)
			{
				if (e.Row.TaxCalcMode == TaxCalculationMode.Gross)
				{
					e.NewValue = false;
					throw new PXSetPropertyException(Messages.GrossModeIsNotAllowedWhenTaxCalculationDisabled);
				}

				if (e.Row.TaxCalcMode != TaxCalculationMode.Net)
				{
					foreach (PXResult<SOTaxTran, Tax> soTax in Base.Taxes.Select())
					{
						Tax tax = soTax;
						if (tax?.TaxCalcLevel == CSTaxCalcLevel.Inclusive)
						{
							e.NewValue = false;
							throw new PXSetPropertyException(Messages.InclusiveTaxesNotAllowedWhenTaxCalculationDisabled);
						}
					}
				}
			}
		}

		protected virtual void _(Events.FieldVerifying<SOOrder, SOOrder.taxCalcMode> e)
		{
			if (e.Row == null) return;

			if (((string)e.NewValue) == TaxCalculationMode.Gross && e.Row.DisableAutomaticTaxCalculation == true)
				{
					throw new PXSetPropertyException(Messages.GrossModeIsNotAllowedWhenTaxCalculationDisabled);
				}
		}

		protected virtual void _(Events.FieldVerifying<SOTaxTran, SOTaxTran.taxID> e)
		{
			if (e.Row == null) return;

			if (Base.Document.Current ?.DisableAutomaticTaxCalculation == true && e.NewValue != null && Base.Document.Current?.TaxCalcMode != TaxCalculationMode.Net)
			{
				Tax tax = Tax.PK.Find(Base, (string)e.NewValue);
				if (tax?.TaxCalcLevel == CSTaxCalcLevel.Inclusive)
				{
					e.Cache.RaiseExceptionHandling<SOTaxTran.taxID>(e.Row, e.Row.TaxID,
						new PXSetPropertyException(Messages.ManualInclusiveTaxesNotAllowedWhenTaxCalculationDisabled, PXErrorLevel.RowError));
				}
			}
		}
	}
}
