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
using System;
using System.Collections.Generic;
using System.Linq;

namespace PX.Objects.PR
{
	public class InvoiceDescriptionType
	{
		public class TaxInvoiceListAttribute : PXStringListAttribute
		{
			public TaxInvoiceListAttribute()
				: base(
				new string[] { Code, CodeName, CodeAndCodeName,
					PaymentDate, PaymentDateAndCode, PaymentDateAndCodeName,
					FreeFormatEntry },
				new string[] { Messages.Code, Messages.CodeName, Messages.CodeAndCodeName,
					Messages.PaymentDate, Messages.PaymentDateAndCode, Messages.PaymentDateAndCodeName,
					Messages.FreeFormatEntry })
			{ }
		}

		public class DeductionInvoiceDescriptionListAttribute : PXStringListAttribute
		{
			public DeductionInvoiceDescriptionListAttribute(Type filterField)
			{
				_FilterField = filterField;
			}

			public override void CacheAttached(PXCache sender)
			{
				sender.Graph.FieldUpdated.AddHandler(_BqlTable, _FilterField.Name, FilterFieldUpdated);
				base.CacheAttached(sender);
				strings = new List<(string internalValue, string externalValue, bool alwaysInclude)>()
				{
					(Code, Messages.Code, true),
					(CodeName, Messages.CodeName, true),
					(CodeAndCodeName, Messages.CodeAndCodeName, true),
					(EmployeeGarnishDescription, Messages.EmployeeGarnishDescription, false),
					(EmployeeGarnishDescriptionPlusPaymentDate, Messages.EmployeeGarnishDescriptionPlusPaymentDate, false),
					(PaymentDate, Messages.PaymentDate, true),
					(PaymentDateAndCode, Messages.PaymentDateAndCode, true),
					(PaymentDateAndCodeName, Messages.PaymentDateAndCodeName, true),
					(FreeFormatEntry, Messages.FreeFormatEntry, true)
				}.Select(x => (x.internalValue, PXMessages.LocalizeNoPrefix(x.externalValue), x.alwaysInclude)).ToList();
			}

			public override void FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
			{
				var row = e.Row;
				if (row == null)
				{
					return;
				}

				var useFiltered = sender.GetValue(row, _FilterField.Name) as bool? == true;

				_AllowedValues = strings.Where(x => useFiltered || x.alwaysInclude).Select(x => x.internalValue).ToArray();
				_AllowedLabels = strings.Where(x => useFiltered || x.alwaysInclude).Select(x => x.externalValue).ToArray();

				base.FieldSelecting(sender, e);
			}

			protected virtual void FilterFieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
			{
				var row = e.Row;
				if (row == null)
				{
					return;
				}

				object value = sender.GetValue(row, _FieldName);
				sender.RaiseFieldSelecting(_FieldName, row, ref value, true);
			}

			private List<(string internalValue, string externalValue, bool alwaysInclude)> strings;
			private Type _FilterField;
		}

		public class freeFormatEntry : PX.Data.BQL.BqlString.Constant<freeFormatEntry>
		{
			public freeFormatEntry() : base(FreeFormatEntry) { }
		}

		public const string Code = "COD";
		public const string CodeName = "CNM";
		public const string CodeAndCodeName = "CCN";
		public const string EmployeeGarnishDescription = "EGD";
		public const string EmployeeGarnishDescriptionPlusPaymentDate = "EGP";
		public const string PaymentDate = "PDT";
		public const string PaymentDateAndCode = "PDC";
		public const string PaymentDateAndCodeName = "PDN";
		public const string FreeFormatEntry = "FFE";
	}
}
