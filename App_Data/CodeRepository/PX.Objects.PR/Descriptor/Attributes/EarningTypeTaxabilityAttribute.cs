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
using PX.Payroll.Data;
using PX.Payroll.Data.Vertex;
using System;

namespace PX.Objects.PR
{
	public class EarningTypeTaxabilityAttribute : PXIntListAttribute, IPXRowSelectedSubscriber, IPXFieldVerifyingSubscriber, IPXRowPersistingSubscriber
	{
		private Type _CountryIDField;
		private Type _TaxIDField;

		public EarningTypeTaxabilityAttribute(Type countryIDField, Type taxIDField) : base(
			new (int, string)[]
			{
				((int)CompensationType.CashSubjectTaxable, Messages.CashSubjectTaxable),
				((int)CompensationType.CashSubjectNonTaxable, Messages.CashSubjectNonTaxable),
				((int)CompensationType.CashNonSubjectNonTaxable, Messages.CashNonSubjectNonTaxable),
				((int)CompensationType.NonCashSubjectTaxable, Messages.NonCashSubjectTaxable),
				((int)CompensationType.NonCashSubjectNonTaxable, Messages.NonCashSubjectNonTaxable),
				((int)CompensationType.NonCashNonSubjectNonTaxable, Messages.NonCashNonSubjectNonTaxable),
			})
		{
			_CountryIDField = countryIDField;
			_TaxIDField = taxIDField;
		}

		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);

			sender.Graph.RowSelected.AddHandler(_TaxIDField, (cache, e) =>
			{
				if (e.Row == null)
				{
					return;
				}

				PXUIFieldAttribute.SetEnabled(sender, e.Row, _TaxIDField.Name, UseTaxabilityField(sender, e.Row));
			});
		}

		public void FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			if (e.Row == null || !UseTaxabilityField(sender, e.Row) || e.NewValue != null)
			{
				return;
			}

			throw new PXSetPropertyException(ErrorMessages.FieldIsEmpty, PXUIFieldAttribute.GetDisplayName(sender, FieldName));
		}

		public void RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			if (e.Row == null || !UseTaxabilityField(sender, e.Row) || sender.GetValue(e.Row, FieldName) != null)
			{
				return;
			}

			throw new PXSetPropertyException(ErrorMessages.FieldIsEmpty, PXUIFieldAttribute.GetDisplayName(sender, FieldName));
		}

		public void RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			if (e.Row == null)
			{
				return;
			}

			PXUIFieldAttribute.SetVisible(sender, e.Row, FieldName, UseTaxabilityField(sender, e.Row));
		}

		private bool UseTaxabilityField(PXCache sender, object row)
		{
			return Equals(sender.GetValue(row, _CountryIDField.Name), LocationConstants.CanadaCountryCode);
		}
	}
}
