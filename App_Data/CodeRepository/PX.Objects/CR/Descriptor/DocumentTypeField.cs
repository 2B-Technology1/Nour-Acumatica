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

namespace PX.Objects.CR
{
	public class DocumentTypeField
	{
		public enum SetOfValues
		{
			SORelatedDocumentTypes,
			PORelatedDocumentTypes,
			ARRelatedDocumentTypes,
			APRelatedDocumentTypes,
			CRRelatedDocumentTypes,
			PMRelatedDocumentTypes
		}

		public const string CashSales = "CS";
		public const string ChecksAndPayments = "CP";
		public const string InvoicesAndMemo = "IM";
		public const string Invoices = "IN";
		public const string Opportunities = "OP";
		public const string Projects = "PR";
		public const string ProjectsQuote = "PQ";
		public const string PurchaseOrder = "PO";
		public const string SalesOrder = "SO";
		public const string SalesQuote = "SQ";
		public const string Shipments = "SH";

		public class ListAttribute : PXStringListAttribute
		{
			public ListAttribute(SetOfValues setOfValues)
			{
				switch (setOfValues)
				{
					case SetOfValues.SORelatedDocumentTypes:
						_AllowedValues = new string[] { SalesOrder, Shipments, Invoices };
						_AllowedLabels = new string[] { CR.MessagesNoPrefix.SalesOrderDesc, CR.MessagesNoPrefix.ShipmentsDesc, CR.MessagesNoPrefix.InvoicesDesc };
						break;
					case SetOfValues.PORelatedDocumentTypes:
						_AllowedValues = new string[] { PurchaseOrder };
						_AllowedLabels = new string[] { CR.MessagesNoPrefix.PurchaseOrderDesc };
						break;
					case SetOfValues.ARRelatedDocumentTypes:
						_AllowedValues = new string[] { CashSales, InvoicesAndMemo };
						_AllowedLabels = new string[] { CR.MessagesNoPrefix.CashSalesDesc, CR.MessagesNoPrefix.InvoicesAndMemoDesc };
						break;
					case SetOfValues.APRelatedDocumentTypes:
						_AllowedValues = new string[] { ChecksAndPayments };
						_AllowedLabels = new string[] { CR.MessagesNoPrefix.ChecksAndPaymentsDesc };
						break;
					case SetOfValues.CRRelatedDocumentTypes:
						_AllowedValues = new string[] { Opportunities, SalesQuote, ProjectsQuote };
						_AllowedLabels = new string[] { CR.MessagesNoPrefix.OpportunitiesDesc, CR.MessagesNoPrefix.SalesQuoteDesc, CR.MessagesNoPrefix.ProjectsQuoteDesc };
						break;
					case SetOfValues.PMRelatedDocumentTypes:
						_AllowedValues = new string[] { Projects, ProjectsQuote };
						_AllowedLabels = new string[] { CR.MessagesNoPrefix.ProjectsDesc, CR.MessagesNoPrefix.ProjectsQuoteDesc };
						break;
				}
			}
		}
	}
}
