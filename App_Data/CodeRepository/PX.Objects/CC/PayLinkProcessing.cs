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
using PX.Objects.AR;
using PX.Objects.CC.PaymentProcessing.Helpers;
using PX.Objects.GL;
using PX.Objects.SO;
using PX.Objects.CC.GraphExtensions;
using System;
using System.Collections;
using System.Collections.Generic;

namespace PX.Objects.CC
{
	public class PayLinkProcessing : PXGraph<PayLinkProcessing>
	{
		public PXCancel<PayLinkFilter> Cancel;

		public PXFilter<PayLinkFilter> Filter;

		[PXHidden]
		public PXSelect<CCPayLink> PayLinks;

		[PXFilterable]
		[PXVirtualDAC]
		public PXFilteredProcessing<PayLinkDocument, PayLinkFilter> DocumentList;

		public virtual IEnumerable documentList()
		{
			var filter = Filter.Current;

			var invoiceQuery = new PXSelectJoin<ARInvoice,
				InnerJoin<Customer, On<Customer.bAccountID, Equal<ARInvoice.customerID>>,
				InnerJoin<CustomerClass, On<CustomerClass.customerClassID, Equal<Customer.customerClassID>>,
				InnerJoin<Branch, On<Branch.branchID, Equal<ARInvoice.branchID>>,
				LeftJoin<CCPayLink, On<CCPayLink.payLinkID, Equal<ARInvoicePayLink.payLinkID>>>>>>,
				Where<ARRegister.docType, Equal<ARDocType.invoice>,
					And<ARRegister.released, Equal<True>, And<ARInvoicePayLink.processingCenterID, IsNotNull,
					And<CustomerClassPayLink.disablePayLink, Equal<False>,
					And<ARRegister.openDoc, Equal<True>>>>>>>(this);

			if (filter?.Action == PayLinkProcessingAction.Create)
			{
				invoiceQuery.WhereAnd<Where<CCPayLink.payLinkID, IsNull,
					Or<CCPayLink.linkStatus, In3<PayLinkStatus.none, PayLinkStatus.closed>>>>();
			}
			else
			{
				invoiceQuery.WhereAnd<Where<CCPayLink.linkStatus, Equal<PayLinkStatus.open>>>();
			}

			var prms = new object[1];
			if (filter?.CustomerID != null)
			{
				prms[0] = filter.CustomerID;
				invoiceQuery.WhereAnd<Where<Customer.bAccountID, Equal<Required<Customer.bAccountID>>>>();
			}
			var invoiceResult = invoiceQuery.Select(prms);

			foreach (PXResult<ARInvoice, Customer, CustomerClass, Branch, CCPayLink> item in invoiceResult)
			{
				ARInvoice invoice = item;
				CCPayLink payLink = item;
				Customer cust = item;
				Branch branch = item;
				CustomerClass custClass = item;
				ARInvoicePayLink invoiceExt = Caches[typeof(ARInvoice)].GetExtension<ARInvoicePayLink>(invoice);

				var payLinkDoc = new PayLinkDocument();
				payLinkDoc.DocType = invoice.DocType;
				payLinkDoc.RefNbr = invoice.RefNbr;
				payLinkDoc.AcctName = cust.AcctName;
				payLinkDoc.BranchID = branch.BranchCD;
				payLinkDoc.CuryDocBal = invoice.CuryDocBal;
				payLinkDoc.CuryID = invoice.CuryID;
				payLinkDoc.CuryOrigDocAmt = invoice.CuryOrigDocAmt;
				payLinkDoc.CustomerClassID = custClass.CustomerClassID;
				payLinkDoc.CustomerID = cust.BAccountID;
				payLinkDoc.DocDate = invoice.DocDate;
				payLinkDoc.DueDate = invoice.DueDate;
				payLinkDoc.ProcessingCenterID = invoiceExt.ProcessingCenterID;
				payLinkDoc.NoteID = invoice.NoteID;
				SetPayLinkDocFields(payLinkDoc, payLink);

				DocumentList.Cache.Hold(payLinkDoc);
				yield return payLinkDoc;
			}

			if (filter.Action == PayLinkProcessingAction.Create) yield break;

			var orderQuery = new PXSelectJoin<SOOrder,
				InnerJoin<Customer, On<Customer.bAccountID, Equal<SOOrder.customerID>>,
				InnerJoin<CustomerClass, On<CustomerClass.customerClassID, Equal<Customer.customerClassID>>,
				InnerJoin<Branch, On<Branch.branchID, Equal<SOOrder.branchID>>,
				InnerJoin<SOOrderType, On<SOOrderType.orderType, Equal<SOOrder.orderType>>,
				LeftJoin<CCPayLink, On<CCPayLink.payLinkID, Equal<SOOrderPayLink.payLinkID>>>>>>>,
				Where<SOOrderType.canHavePayments, Equal<True>, And<SOOrderPayLink.processingCenterID, IsNotNull,
					And<CustomerClassPayLink.disablePayLink, Equal<False>,
					And<SOOrder.behavior, NotIn3<SOBehavior.iN, SOBehavior.mO>,
					And<Where<CCPayLink.linkStatus, Equal<PayLinkStatus.open>>>>>>>>(this);

			if (filter?.CustomerID != null)
			{
				prms[0] = filter.CustomerID;
				orderQuery.WhereAnd<Where<Customer.bAccountID, Equal<Required<Customer.bAccountID>>>>();
			}
			var orderResult = orderQuery.Select(prms);

			foreach (PXResult<SOOrder, Customer, CustomerClass, Branch, SOOrderType, CCPayLink> item in orderResult)
			{
				SOOrder order = item;
				CCPayLink payLink = item;
				Customer cust = item;
				Branch branch = item;
				CustomerClass custClass = item;
				SOOrderPayLink orderExt = Caches[typeof(SOOrder)].GetExtension<SOOrderPayLink>(order);

				var payLinkDoc = new PayLinkDocument();
				payLinkDoc.DocType = order.OrderType;
				payLinkDoc.RefNbr = order.OrderNbr;
				payLinkDoc.AcctName = cust.AcctName;
				payLinkDoc.BranchID = branch.BranchCD;
				payLinkDoc.CuryDocBal = order.CuryUnpaidBalance;
				payLinkDoc.CuryID = order.CuryID;
				payLinkDoc.CuryOrigDocAmt = order.CuryOrderTotal;
				payLinkDoc.CustomerClassID = custClass.CustomerClassID;
				payLinkDoc.CustomerID = cust.BAccountID;
				payLinkDoc.DocDate = order.OrderDate;
				payLinkDoc.DueDate = order.RequestDate;
				payLinkDoc.ProcessingCenterID = orderExt.ProcessingCenterID;
				payLinkDoc.NoteID = order.NoteID;
				SetPayLinkDocFields(payLinkDoc, payLink);

				DocumentList.Cache.Hold(payLinkDoc);
				yield return payLinkDoc;
			}
		}

		public PXAction<PayLinkDocument> viewCustomer;
		[PXUIField(DisplayName = AR.Messages.ViewCustomer)]
		[PXButton]
		public virtual void ViewCustomer()
		{
			var row = DocumentList.Current;
			if (row == null)
				return;

			Customer customer = PXSelect<Customer,
				Where<Customer.bAccountID, Equal<Required<Customer.bAccountID>>>>
				.Select(this, row.CustomerID);
			var graph = CreateInstance<CustomerMaint>();
			graph.BAccount.Current = customer;
			throw new PXRedirectRequiredException(graph, true, null)
			{ Mode = PXBaseRedirectException.WindowMode.NewWindow };
		}

		public PXAction<PayLinkDocument> viewDocument;
		[PXUIField]
		[PXButton]
		public virtual void ViewDocument()
		{
			var row = DocumentList.Current;
			if (row == null)
				return;

			if (row.DocType == ARDocType.Invoice)
			{
				ARInvoice invoice = PXSelect<ARInvoice, Where<ARInvoice.docType, Equal<Required<ARInvoice.docType>>,
					And<ARInvoice.refNbr, Equal<Required<ARInvoice.refNbr>>>>>.Select(this, row.DocType, row.RefNbr);
				var graph = CreateInstance<ARInvoiceEntry>();
				graph.Document.Current = invoice;

				throw new PXRedirectRequiredException(graph, true, null)
				{ Mode = PXBaseRedirectException.WindowMode.NewWindow };
			}
			else
			{
				SOOrder order = PXSelect<SOOrder, Where<SOOrder.orderType, Equal<Required<SOOrder.orderType>>,
					And<SOOrder.orderNbr, Equal<Required<SOOrder.orderNbr>>>>>.Select(this, row.DocType, row.RefNbr);
				var graph = CreateInstance<SOOrderEntry>();
				graph.Document.Current = order;

				throw new PXRedirectRequiredException(graph, true, null)
				{ Mode = PXBaseRedirectException.WindowMode.NewWindow };
			}
		}

		public PayLinkProcessing()
		{
			var filter = Filter.Current;
			DocumentList.SetProcessDelegate((List<PayLinkDocument> items) =>
			{
				Process(items, filter);
			});
		}

		private void SetPayLinkDocFields(PayLinkDocument payLinkDoc, CCPayLink payLink)
		{
			if (payLink != null && !PayLinkHelper.PayLinkWasProcessed(payLink))
			{
				payLinkDoc.ExternalID = payLink.ExternalID;
				payLinkDoc.ErrorMessage = payLink.ErrorMessage;
				payLinkDoc.StatusDate = payLink.StatusDate;
				payLinkDoc.NeedSync = payLink.NeedSync;
				payLinkDoc.PayLinkAmt = payLink.Amount;
			}
		}

		private static ARInvoiceEntry GetInvoiceGraph()
		{
			return CreateInstance<ARInvoiceEntry>();
		}

		private static SOOrderEntry GetOrderGraph()
		{
			return CreateInstance<SOOrderEntry>();
		}

		private static void Process(List<PayLinkDocument> items, PayLinkFilter filter)
		{
			ARInvoiceEntry invoiceGraph = null;
			SOOrderEntry orderGraph = null;

			for (int i = 0; i < items.Count; i++)
			{
				try
				{
					var item = items[i];
					if (item.DocType == ARDocType.Invoice)
					{
						if (invoiceGraph == null)
						{
							invoiceGraph = GetInvoiceGraph();
						}
						ProcessInvoicePayLink(item, filter, invoiceGraph);
					}
					else
					{
						if (orderGraph == null)
						{
							orderGraph = GetOrderGraph();
						}
						ProcessSalesOrderPayLink(item, filter, orderGraph);
					}
					PXProcessing<PayLinkDocument>.SetInfo(i, ActionsMessages.RecordProcessed);
				}
				catch (Exception ex)
				{
					PXProcessing<PayLinkDocument>.SetError(i, ex);
				}
			}
		}

		private static void ProcessSalesOrderPayLink(PayLinkDocument item, PayLinkFilter filter, SOOrderEntry graph)
		{
			graph.Clear();
			graph.Document.Current = graph.Document.Search<ARInvoice.refNbr>(item.RefNbr, item.DocType);
			var graphExt = graph.GetExtension<SOOrderEntryPayLink>();
			if (filter.Action == PayLinkProcessingAction.Create)
			{
				graphExt.createLink.Press();
			}
			else
			{
				graphExt.syncLink.Press();
			}
		}

		private static void ProcessInvoicePayLink(PayLinkDocument item, PayLinkFilter filter, ARInvoiceEntry graph)
		{
			graph.Clear();
			graph.Document.Current = graph.Document.Search<ARInvoice.refNbr>(item.RefNbr, item.DocType);
			var graphExt = graph.GetExtension<ARInvoiceEntryPayLink>();
			if (filter.Action == PayLinkProcessingAction.Create)
			{
				graphExt.createLink.Press();
			}
			else
			{
				graphExt.syncLink.Press();
			}
		}

		[PXHidden]
		public class PayLinkFilter : IBqlTable
		{
			#region Action
			public abstract class pendingOperation : Data.BQL.BqlString.Field<pendingOperation> { }
			[PXString(IsFixed = true)]
			[PXDefault(PayLinkProcessingAction.Create)]
			[PayLinkProcessingAction.List]
			[PXUIField(DisplayName = "Action")]
			public virtual string Action { get; set; }
			#endregion

			#region CustomerID
			public abstract class customerID : Data.BQL.BqlInt.Field<customerID> { }
			[Customer(DescriptionField = typeof(Customer.acctName))]
			public virtual int? CustomerID { get; set; }
			#endregion
		}

		/// <summary>
		/// Represents a row on the Payment Link processing screen.
		/// </summary>
		[PXCacheName("Document Payment Link")]
		public class PayLinkDocument : IBqlTable
		{
			public abstract class selected : Data.BQL.BqlBool.Field<selected> { }
			/// <exclude/>
			[PXBool]
			[PXUnboundDefault(false)]
			[PXUIField(DisplayName = "Selected")]
			public virtual bool? Selected { get; set; }

			public abstract class docType : Data.BQL.BqlString.Field<docType> { }
			/// <exclude/>
			[PXString(IsKey = true)]
			[PXUIField(DisplayName = "Document Type")]
			public virtual string DocType { get; set; }

			public abstract class refNbr : Data.BQL.BqlString.Field<refNbr> { }
			/// <exclude/>
			[PXString(IsKey = true)]
			[PXUIField(DisplayName = "Reference Nbr.")]
			public virtual string RefNbr { get; set; }

			public abstract class branchID : Data.BQL.BqlString.Field<branchID> { }
			/// <exclude/>
			[PXString]
			[PXUIField(DisplayName = "Branch")]
			public virtual string BranchID { get; set; }

			public abstract class customerID : Data.BQL.BqlInt.Field<customerID> { }
			/// <exclude/>
			[Customer(typeof(Search<Customer.bAccountID>), typeof(Customer.acctCD), DisplayName = "Customer")]
			public virtual int? CustomerID { get; set; }

			public abstract class acctName : Data.BQL.BqlString.Field<acctName> { }
			/// <exclude/>
			[PXString]
			[PXUIField(DisplayName = "Customer Name")]
			public virtual string AcctName { get; set; }

			public abstract class customerClassID : Data.BQL.BqlString.Field<customerClassID> { }
			/// <exclude/>
			[PXString]
			[PXUIField(DisplayName = "Customer Class")]
			public virtual string CustomerClassID { get; set; }

			public abstract class docDate : Data.BQL.BqlDateTime.Field<docDate> { }
			/// <exclude/>
			[PXDate]
			[PXUIField(DisplayName = "Document Date")]
			public virtual DateTime? DocDate { get; set; }

			public abstract class dueDate : Data.BQL.BqlDateTime.Field<dueDate> { }
			/// <exclude/>
			[PXDate]
			[PXUIField(DisplayName = "Due Date")]
			public virtual DateTime? DueDate { get; set; }

			public abstract class curyOrigDocAmt : Data.BQL.BqlDecimal.Field<curyOrigDocAmt> { }
			/// <exclude/>
			[PXDecimal]
			[PXUIField(DisplayName = "Document Total Amount")]
			public virtual decimal? CuryOrigDocAmt { get; set; }

			public abstract class curyDocBal : Data.BQL.BqlDecimal.Field<curyDocBal> { }
			/// <exclude/>
			[PXDecimal]
			[PXUIField(DisplayName = "Unpaid Balance")]
			public virtual decimal? CuryDocBal { get; set; }

			public abstract class payLinkAmt : Data.BQL.BqlDecimal.Field<payLinkAmt> { }
			/// <summary>
			/// Amount to be payed by Payment Link.
			/// </summary>
			[PXDecimal]
			[PXUIField(DisplayName = "Payment Link Amount")]
			public virtual decimal? PayLinkAmt { get; set; }

			public abstract class curyID : Data.BQL.BqlString.Field<curyID> { }
			/// <exclude/>
			[PXString]
			[PXUIField(DisplayName = "Currency")]
			public virtual string CuryID { get; set; }

			public abstract class processingCenterID : Data.BQL.BqlString.Field<processingCenterID> { }
			/// <exclude/>
			[PXString]
			[PXUIField(DisplayName = "Proc. Center ID")]
			public virtual string ProcessingCenterID { get; set; }

			public abstract class statusDate : Data.BQL.BqlDateTime.Field<statusDate> { }
			/// <summary>
			/// Date of the last interaction with the Payment Link webportal.
			/// </summary>
			[PXDate]
			[PXUIField(DisplayName = "Status Date")]
			public virtual DateTime? StatusDate { get; set; }

			public abstract class errorMessage : Data.BQL.BqlString.Field<errorMessage> { }
			/// <exclude/>
			[PXString]
			[PXUIField(DisplayName = "Error Message")]
			public virtual string ErrorMessage { get; set; }

			public abstract class externalID : Data.BQL.BqlString.Field<externalID> { }
			/// <summary>
			/// Payment Link webportal specific Id.
			/// </summary>
			[PXString]
			[PXUIField(DisplayName = "Link External ID", Visible = false)]
			public virtual string ExternalID { get; set; }

			public abstract class needSync : Data.BQL.BqlBool.Field<needSync> { }
			/// <summary>
			/// Need update Payment Link after the document update.
			/// </summary>
			[PXDBBool]
			[PXDefault(false)]
			[PXUIField(DisplayName = "Sync Required", Enabled = false)]
			public virtual bool? NeedSync { get; set; }

			public abstract class noteID : Data.BQL.BqlGuid.Field<noteID> { }
			/// <exclude/>
			[PXNote]
			public virtual Guid? NoteID { get; set; }
		}
	}
}
