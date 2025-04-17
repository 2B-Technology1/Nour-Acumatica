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
using PX.Data;
using PX.Objects.AP;

namespace PX.Objects.CA
{
	public class AddendaItemInfo
	{
		public string FieldName { get; set; }
		public string FieldFormat { get; set; }
		public Type FieldType { get; set; }
		public string FieldValue { get; set; }
	}

	public class AddendaHelper
	{
		public const string fieldStart = "[";
		public const string fieldEnd = "]";
		public const char separator = '*';
		public const char terminator = '\\';
		public const string formatter = ":";

		public static string ParseAddendaTemplate(string templateString, out AddendaItemInfo[] parameters)
		{
			var resultFields = new List<AddendaItemInfo>();
			var resultString = new StringBuilder();

			int fieldsCounter = 0;

			var items = templateString.Split(separator);
			foreach (var item in items)
			{
				if (item.StartsWith(fieldStart))
				{
					if (!item.EndsWith(fieldEnd) && !item.EndsWith(terminator.ToString()))
					{
						throw new PXException(Messages.FormatStringMissingClosingBracket);
					}

					var startPosition = item.IndexOf(fieldStart);
					var endPosition = item.IndexOf(fieldEnd);
					if (item.Contains(formatter))
					{
						var formatterPosition = item.IndexOf(formatter);
						var fieldName = item.Substring(startPosition + 1, formatterPosition - 1);
						resultFields.Add(new AddendaItemInfo { FieldName = fieldName, FieldFormat = item.Substring(formatterPosition + 1, endPosition - formatterPosition - 1), FieldType = Mapping[fieldName] });
					}
					else
					{
						var fieldName = item.Substring(startPosition + 1, endPosition - 1);
						resultFields.Add(new AddendaItemInfo { FieldName = fieldName, FieldType = Mapping[fieldName] });
					}

					resultString.Append(string.Concat("{", fieldsCounter.ToString(), "}"));
					fieldsCounter++;
				}
				else
				{
					resultString.Append(item);
				}

				resultString.Append(separator.ToString());
			}

			resultString[resultString.Length - 1] = terminator;

			parameters = resultFields.ToArray();
			return resultString.ToString();
		}

		public static Dictionary<string, Type> Mapping = new Dictionary<string, Type>
		{
			{ "Payment Ref", typeof(APPayment.refNbr) },
			{ "Payment Descr", typeof(APPayment.docDesc) },
			{ "Payment Period", typeof(APPayment.finPeriodID) },
			{ "Payment Date", typeof(APPayment.docDate) },
			{ "Payment Ext Ref", typeof(APPayment.extRefNbr) },
			{ "Payment Amount", typeof(APPayment.curyOrigDocAmt) },
			{ "Bill Ref", typeof(APInvoice.refNbr) },
			{ "Bill Ext Ref", typeof(APInvoice.invoiceNbr) },
			{ "Bill Amount", typeof(APInvoice.origDiscAmt) },
			{ "Bill Descr", typeof(APInvoice.docDesc) },
			{ "Bill Date", typeof(APInvoice.docDate) },
			{ "Bill Period", typeof(APInvoice.finPeriodID) },
			{ "Bill Tax Amount", typeof(APInvoice.curyTaxTotal) },
		};

		public static string BuildAddenda(PXResultset<APPayment> records, string templateString)
		{
			var resultString = new StringBuilder();
			AddendaItemInfo[] parameters;
			var template = ParseAddendaTemplate(templateString, out parameters);

			foreach (PXResult<APPayment, APAdjust, APInvoice> result in records)
			{
				var payment = (APPayment)result;
				var adjust = (APAdjust)result;
				var invoice = (APInvoice)result;

				resultString.Append(string.Format(template, GetParameters(parameters, payment, adjust, invoice)));

				if (resultString.Length > 80)
				{
					resultString = new StringBuilder(resultString.ToString().Substring(0, 80));
					break;
				}
			}

			if (resultString[resultString.Length - 1] != terminator)
			{
				for (int i = resultString.Length - 1; i >= 0; i--)
				{
					if (resultString[i] == separator)
					{
						resultString = new StringBuilder(resultString.ToString().Substring(0, i));
						resultString.Append(terminator);
						break;
					}
				}
			}

			return resultString.ToString();
		}

		public static string[] GetParameters(AddendaItemInfo[] parameters, APPayment payment, APAdjust adjust, APInvoice invoice)
		{
			var result = new List<string>();

			foreach (var parameter in parameters)
			{
				switch (parameter.FieldName)
				{
					case "Payment Ref": result.Add(payment.RefNbr); break;
					case "Payment Descr": result.Add(payment.DocDesc); break;
					case "Payment Period": result.Add(payment.FinPeriodID); break;
					case "Payment Date": result.Add(string.IsNullOrEmpty(parameter.FieldFormat) ? payment.DocDate.ToString() : payment.DocDate.Value.ToString(parameter.FieldFormat)); break;
					case "Payment Ext Ref": result.Add(payment.ExtRefNbr); break;
					case "Payment Amount": result.Add(payment.CuryOrigDocAmt.ToString()); break;
					case "Bill Ref": result.Add(invoice.RefNbr); break;
					case "Bill Ext Ref": result.Add(invoice.InvoiceNbr); break;
					case "Bill Amount": result.Add(invoice.OrigDocAmt.ToString()); break;
					case "Bill Descr": result.Add(invoice.DocDesc); break;
					case "Bill Date": result.Add(string.IsNullOrEmpty(parameter.FieldFormat) ? invoice.DocDate.ToString() : invoice.DocDate.Value.ToString(parameter.FieldFormat)); break;
					case "Bill Period": result.Add(invoice.FinPeriodID); break;
					case "Bill Tax Amount": result.Add(invoice.CuryTaxTotal.ToString()); break;
				}
			}

			return result.ToArray();
		}
	}
}
