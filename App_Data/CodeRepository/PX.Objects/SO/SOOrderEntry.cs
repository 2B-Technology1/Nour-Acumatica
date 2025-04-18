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
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using PX.Common;
using PX.Data;
using PX.Objects.AP;
using PX.Objects.AR;
using PX.Objects.CA;
using PX.Objects.CM;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Objects.DR;
using PX.Objects.EP;
using PX.Objects.GL;
using PX.Objects.IN;
using PX.Objects.PM;
using PX.Objects.PO;
using PX.Objects.TX;
using POLine = PX.Objects.PO.POLine;
using POOrder = PX.Objects.PO.POOrder;
using PX.CarrierService;
using PX.Concurrency;
using CRLocation = PX.Objects.CR.Standalone.Location;
using PX.Objects.AR.CCPaymentProcessing;
using PX.Objects.AR.CCPaymentProcessing.Common;
using PX.Objects.AR.CCPaymentProcessing.Helpers;
using PX.Objects.AR.CCPaymentProcessing.Interfaces;
using ARRegisterAlias = PX.Objects.AR.Standalone.ARRegisterAlias;
using PX.Objects.AR.MigrationMode;
using PX.Objects.Common;
using PX.Objects.Common.Discount;
using PX.Objects.Common.Extensions;
using PX.CS.Contracts.Interfaces;
using Message = PX.CarrierService.Message;
using PX.TaxProvider;
using PX.Data.DependencyInjection;
using PX.Data.WorkflowAPI;
using PX.LicensePolicy;
using PX.Objects.Extensions.PaymentTransaction;
using PX.Objects.SO.GraphExtensions.CarrierRates;
using PX.Objects.SO.GraphExtensions.SOOrderEntryExt;
using PX.Objects.SO.Attributes;
using PX.Objects.Common.Attributes;
using PX.Objects.Common.Bql;
using OrderActions = PX.Objects.SO.SOOrderEntryActionsAttribute;
using PX.Objects.SO.DAC.Projections;
using PX.Data.BQL.Fluent;
using PX.Data.Localization;
using PX.Data.Reports;
using PX.Objects.IN.InventoryRelease;
using PX.Data.BQL;
using PX.Objects.IN.InventoryRelease.Utility;
using PX.Objects.IN.InventoryRelease.Accumulators.QtyAllocated;

namespace PX.Objects.SO
{
	[Serializable]
	public partial class SOOrderEntry : PXGraph<SOOrderEntry, SOOrder>, PXImportAttribute.IPXPrepareItems, PXImportAttribute.IPXProcess, IGraphWithInitialization
	{
		private DiscountEngine<SOLine, SOOrderDiscountDetail> _discountEngine => DiscountEngineProvider.GetEngineFor<SOLine, SOOrderDiscountDetail>();
		public SOOrderLineSplittingExtension LineSplittingExt => FindImplementation<SOOrderLineSplittingExtension>();
		public SOOrderLineSplittingAllocatedExtension LineSplittingAllocatedExt => FindImplementation<SOOrderLineSplittingAllocatedExtension>();
		public SOOrderItemAvailabilityExtension ItemAvailabilityExt => FindImplementation<SOOrderItemAvailabilityExtension>();

		[PXHidden]
		public PXSelect<BAccount> bAccountBasic;

		[PXHidden]
		public PXSelect<BAccountR> bAccountRBasic;

		public ToggleCurrency<SOOrder> CurrencyView;
		public PXFilter<Vendor> _Vendor;

		/// <summary>
		/// If true the SO-PO Link dialog will display PO Orders on hold.
		/// </summary>
		/// <remarks>This setting is used when linking On-Hold PO Orders with SO created through RQRequisitionEntry.</remarks>
		public bool SOPOLinkShowDocumentsOnHold { get; set; }

		#region Selects
		[PXViewName(Messages.SOOrder)]
		[PXCopyPasteHiddenFields(typeof(SOOrder.showDiscountsTab))]
		public PXSelectJoin<SOOrder,
			LeftJoinSingleTable<Customer, On<Customer.bAccountID, Equal<SOOrder.customerID>>>,
			Where<SOOrder.orderType, Equal<Optional<SOOrder.orderType>>,
			And<Where<Customer.bAccountID, IsNull,
			Or<Match<Customer, Current<AccessInfo.userName>>>>>>> Document;
		[PXCopyPasteHiddenFields(typeof(SOOrder.cancelled), typeof(SOOrder.ownerID), typeof(SOOrder.workgroupID), typeof(SOOrder.extRefNbr), typeof(SOOrder.updateNextNumber), typeof(SOOrder.emailed))]
		public PXSelect<SOOrder, Where<SOOrder.orderType, Equal<Current<SOOrder.orderType>>, And<SOOrder.orderNbr, Equal<Current<SOOrder.orderNbr>>>>> CurrentDocument;

		public PXSelect<RUTROT.RUTROT, Where<True, Equal<False>>> Rutrots;
		public PXSelect<RQ.RQRequisitionOrder> rqrequisitionorder;
		public PXSelect<RQ.RQRequisition, Where<RQ.RQRequisition.reqNbr, Equal<Required<RQ.RQRequisition.reqNbr>>>> rqrequisition;

		public PXSelect<SOOrderSite, Where<SOOrderSite.orderType, Equal<Current<SOOrder.orderType>>, And<SOOrderSite.orderNbr, Equal<Current<SOOrder.orderNbr>>>>> SiteList;
		public PXSelect<SOOrderSite,
			Where<SOOrderSite.orderType, Equal<Required<SOOrderSite.orderType>>,
				And<SOOrderSite.orderNbr, Equal<Required<SOOrderSite.orderNbr>>,
				And<SOOrderSite.siteID, Equal<Required<SOOrderSite.siteID>>>>>> OrderSite;

		[PXViewName(Messages.SOLine)]
		[PXImport(typeof(SOOrder))]
		[PXCopyPasteHiddenFields(typeof(SOLine.completed), typeof(SOLine.isLegacyDropShip), typeof(SOLine.curyUnbilledAmt))]
		public PXOrderedSelect<SOOrder, SOLine,
			Where<SOLine.orderType, Equal<Current<SOOrder.orderType>>,
				And<SOLine.orderNbr, Equal<Current<SOOrder.orderNbr>>>>,
			OrderBy<Asc<SOLine.orderType, Asc<SOLine.orderNbr, Asc<SOLine.sortOrder, Asc<SOLine.lineNbr>>>>>> Transactions;

		public PXSelectReadonly<INItemCost,
			Where<INItemCost.inventoryID, Equal<Required<SOLine.inventoryID>>,
				And<INItemCost.curyID, Equal<Required<Branch.baseCuryID>>>>> initemcost;

		public PXSelectReadonly<INItemStats,
			Where<INItemStats.inventoryID, Equal<Required<SOLine.inventoryID>>,
				And<INItemStats.siteID, Equal<Required<SOLine.siteID>>>>> initemstats;

		[Api.Export.PXOptimizationBehavior(IgnoreBqlDelegate = true)]
		protected virtual IEnumerable transactions()
		{
			PrefetchWithDetails();

			PXSelectBase<SOLine> query =
				new PXSelectReadonly2<SOLine,
						InnerJoin<INItemCost, On<INItemCost.inventoryID, Equal<SOLine.inventoryID>,
							And<INItemCost.curyID, EqualBaseCuryID<Current2<SOOrder.branchID>>>>,
						InnerJoin<INItemStats, On<INItemStats.inventoryID, Equal<SOLine.inventoryID>,
							And<INItemStats.siteID, Equal<SOLine.siteID>>>>>,
						Where<SOLine.orderType, Equal<Current<SOOrder.orderType>>,
							And<SOLine.orderNbr, Equal<Current<SOOrder.orderNbr>>>>,
						OrderBy<Asc<SOLine.orderType, Asc<SOLine.orderNbr, Asc<SOLine.sortOrder, Asc<SOLine.lineNbr>>>>>>(this);

			using (new PXFieldScope(query.View, typeof(INItemCost), typeof(INItemStats)))
			{
				int startRow = PXView.StartRow;
				int totalRows = 0;
				foreach (PXResult<SOLine, INItemCost, INItemStats> record in query.View.Select(
				PXView.Currents, PXView.Parameters, PXView.Searches, PXView.SortColumns,
				PXView.Descendings, PXView.Filters, ref startRow, PXView.MaximumRows, ref totalRows))
				{
					INItemCost itemCost = (INItemCost)record;
					initemcost.StoreResult(itemCost);
					initemstats.StoreResult((INItemStats)record);
				}
			}
			return null;
		}

		public virtual void PrefetchWithDetails()
		{
			LoadEntityDiscounts();
		}

		public PXSelect<ExternalTransaction,
			Where<ExternalTransaction.origRefNbr, Equal<Current<SOOrder.orderNbr>>,
				And<ExternalTransaction.origDocType, Equal<Current<SOOrder.orderType>>>>,
			OrderBy<Desc<ExternalTransaction.transactionID>>> ExternalTran;

		public PXSelectOrderBy<CCProcTran, OrderBy<Desc<CCProcTran.tranNbr>>> ccProcTran;
		public IEnumerable CcProcTran()
		{
			var externalTrans = ExternalTran.Select();
			var query = new PXSelect<CCProcTran,
				Where<CCProcTran.transactionID, Equal<Required<CCProcTran.transactionID>>>>(this);
			foreach (ExternalTransaction extTran in externalTrans)
			{
				foreach (CCProcTran procTran in query.Select(extTran.TransactionID))
				{
					yield return procTran;
				}
			}
		}

		public PXSelect<SiteStatusByCostCenter> sitestatusview;
		public PXSelect<INItemSite> initemsite;
		public PXSelect<SOTax, Where<SOTax.orderType, Equal<Current<SOOrder.orderType>>, And<SOTax.orderNbr, Equal<Current<SOOrder.orderNbr>>>>, OrderBy<Asc<SOTax.orderType, Asc<SOTax.orderNbr, Asc<SOTax.taxID>>>>> Tax_Rows;
		public PXSelectJoin<SOTaxTran, LeftJoin<Tax, On<Tax.taxID, Equal<SOTaxTran.taxID>>>,
				Where<SOTaxTran.orderType, Equal<Current<SOOrder.orderType>>,
					And<SOTaxTran.orderNbr, Equal<Current<SOOrder.orderNbr>>>>> Taxes;

		[PXViewIncludesArchivedRecords]
		public
			PXSelectJoin<SOOrderShipment,
			LeftJoin<SOShipment, On<
				SOShipment.shipmentNbr, Equal<SOOrderShipment.shipmentNbr>,
				And<SOShipment.shipmentType, Equal<SOOrderShipment.shipmentType>>>>,
			Where<
				SOOrderShipment.orderType, Equal<Current<SOOrder.orderType>>,
				And<SOOrderShipment.orderNbr, Equal<Current<SOOrder.orderNbr>>>>,
			OrderBy<Asc<SOOrderShipment.shipmentNbr>>>
			shipmentlist;

		public PXSelect<INItemSiteSettings, Where<INItemSiteSettings.inventoryID, Equal<Required<INItemSiteSettings.inventoryID>>, And<INItemSiteSettings.siteID, Equal<Required<INItemSiteSettings.siteID>>>>> initemsettings;

		public PXSelect<ARRegister> arregister;

		[PXViewName(Messages.BillingAddress)]
		public PXSelect<SOBillingAddress, Where<SOBillingAddress.addressID, Equal<Current<SOOrder.billAddressID>>>> Billing_Address;
		[PXViewName(Messages.ShippingAddress)]
		public PXSelect<SOShippingAddress, Where<SOShippingAddress.addressID, Equal<Current<SOOrder.shipAddressID>>>> Shipping_Address;
		[PXViewName(Messages.BillingContact)]
		public PXSelect<SOBillingContact, Where<SOBillingContact.contactID, Equal<Current<SOOrder.billContactID>>>> Billing_Contact;
		[PXViewName(Messages.ShippingContact)]
		public PXSelect<SOShippingContact, Where<SOShippingContact.contactID, Equal<Current<SOOrder.shipContactID>>>> Shipping_Contact;

		public PXSelect<SOSetupApproval, Where<SOSetupApproval.orderType, Equal<Optional<SOOrder.orderType>>>> SetupApproval;

		[PXViewName(Messages.Approval)]
		public SOOrderApprovalAutomation Approval;

		public PXSelect<CurrencyInfo, Where<CurrencyInfo.curyInfoID, Equal<Current<SOOrder.curyInfoID>>>> currencyinfo;

		public PXSetup<ARSetup> arsetup;
		[PXViewName(AR.Messages.Customer)]
		public PXSetup<Customer, Where<Customer.bAccountID, Equal<Optional<SOOrder.customerID>>>> customer;
		public PXSetup<CustomerClass, Where<CustomerClass.customerClassID, Equal<Current<Customer.customerClassID>>>> customerclass;
		public PXSetup<TaxZone, Where<TaxZone.taxZoneID, Equal<Current<SOOrder.taxZoneID>>>> taxzone;
		public PXSetup<Location, Where<Location.bAccountID, Equal<Current<SOOrder.customerID>>, And<Location.locationID, Equal<Optional<SOOrder.customerLocationID>>>>> location;
		public PXSelect<ARBalances> arbalances;
		public PXSetup<SOOrderType, Where<SOOrderType.orderType, Equal<Optional<SOOrder.orderType>>>> soordertype;
		public PXSetup<SOOrderTypeOperation,
			Where<SOOrderTypeOperation.orderType, Equal<Optional<SOOrderType.orderType>>,
			And<SOOrderTypeOperation.operation, Equal<Optional<SOOrderType.defaultOperation>>>>> sooperation;

		[PXCopyPasteHiddenView()]
		[PXFilterable]
		public PXSelect<SOLineSplit, Where<SOLineSplit.orderType, Equal<Current<SOLine.orderType>>, And<SOLineSplit.orderNbr, Equal<Current<SOLine.orderNbr>>, And<SOLineSplit.lineNbr, Equal<Current<SOLine.lineNbr>>>>>> splits;
		public PXSelect<SOLine, Where<SOLine.orderType, Equal<Current<SOOrder.orderType>>, And<SOLine.orderNbr, Equal<Current<SOOrder.orderNbr>>, And<SOLine.isFree, Equal<boolTrue>>>>, OrderBy<Asc<SOLine.orderType, Asc<SOLine.orderNbr, Asc<SOLine.lineNbr>>>>> FreeItems;

		public PXSelect<SOOrderDiscountDetail, Where<SOOrderDiscountDetail.orderType, Equal<Current<SOOrder.orderType>>, And<SOOrderDiscountDetail.orderNbr, Equal<Current<SOOrder.orderNbr>>>>, OrderBy<Asc<SOOrderDiscountDetail.lineNbr>>> DiscountDetails;

		public PXSetup<INSetup> insetup;
		public PXSetup<SOSetup> sosetup;
		public PXSetup<Branch, 
			InnerJoin<INSite, 
				On<INSite.branchID, Equal<Branch.branchID>>>, 
			Where<INSite.siteID, Equal<Optional<SOOrder.destinationSiteID>>>> Company;
		public PXSetupOptional<CommonSetup> commonsetup;

		public PXSelect<CurrencyInfo> DummyCuryInfo;
		
		public PXFilter<SOParamFilter> soparamfilter;
		public PXFilter<AddInvoiceFilter> addinvoicefilter;
		public PXFilter<CopyParamFilter> copyparamfilter;
		public PXFilter<RecalcDiscountsParamFilter> recalcdiscountsfilter;

		public PXSelect<INTranSplit> intransplit;

		[PXVirtualDAC]
		public PXSelect<InvoiceSplits> invoicesplits;

		[PXCopyPasteHiddenView()]
		public PXSelectJoin<SOLineSplit2, 
			LeftJoin<INItemPlan, 
				On<INItemPlan.planID, Equal<SOLineSplit2.planID>>>, 
			Where<SOLineSplit2.sOOrderType, Equal<Optional<SOLineSplit.orderType>>, 
				And<SOLineSplit2.sOOrderNbr, Equal<Optional<SOLineSplit.orderNbr>>, 
				And<SOLineSplit2.sOLineNbr, Equal<Optional<SOLineSplit.lineNbr>>, 
				And<SOLineSplit2.sOSplitLineNbr, Equal<Optional<SOLineSplit.splitLineNbr>>>>>>> sodemand;

		public PXSelect<SOSalesPerTran, Where<SOSalesPerTran.orderType, Equal<Current<SOOrder.orderType>>, And<SOSalesPerTran.orderNbr, Equal<Current<SOOrder.orderNbr>>>>> SalesPerTran;

		public PXSelect<SOOrder, Where<SOOrder.orderType, Equal<Current<SOOrder.orderType>>, And<SOOrder.orderNbr, Equal<Current<SOOrder.orderNbr>>>>> DocumentProperties;
		[PXCopyPasteHiddenView]
		public PXSelect<SOPackageInfoEx, Where<SOPackageInfoEx.orderType, Equal<Current<SOOrder.orderType>>, And<SOPackageInfoEx.orderNbr, Equal<Current<SOOrder.orderNbr>>>>> Packages;

		public PXSelect<INReplenishmentOrder> Replenihment;

		public PXSelectJoin<INReplenishmentLine,
			InnerJoin<INItemPlan, On<INItemPlan.planID, Equal<INReplenishmentLine.planID>>>,
			Where<INReplenishmentLine.sOType, Equal<Current<SOOrder.orderType>>,
				And<INReplenishmentLine.sONbr, Equal<Current<SOOrder.orderNbr>>>>> ReplenishmentLinesWithPlans;

		[PXFilterable]
		[PXCopyPasteHiddenView()]
		public PXSelectJoin<SOAdjust,
				InnerJoin<ARPayment, On<ARPayment.docType, Equal<SOAdjust.adjgDocType>, And<ARPayment.refNbr, Equal<SOAdjust.adjgRefNbr>>>,
				LeftJoin<ExternalTransaction, On<ExternalTransaction.transactionID, Equal<ARPayment.cCActualExternalTransactionID>>>>>
			Adjustments;
		public PXSelectJoin<SOAdjust,
							InnerJoin<ARRegisterAlias, On<ARRegisterAlias.docType, Equal<SOAdjust.adjgDocType>, And<ARRegisterAlias.refNbr, Equal<SOAdjust.adjgRefNbr>>>,
							InnerJoinSingleTable<ARPayment, On<ARRegisterAlias.docType, Equal<ARPayment.docType>, And<ARRegisterAlias.refNbr, Equal<ARPayment.refNbr>>>,
							LeftJoin<ExternalTransaction, On<ExternalTransaction.transactionID, Equal<ARPayment.cCActualExternalTransactionID>>>>>,
							Where<SOAdjust.adjdOrderType, Equal<Current<SOOrder.orderType>>,
								And<SOAdjust.adjdOrderNbr, Equal<Current<SOOrder.orderNbr>>>>>
			Adjustments_Raw;

		public PXSelect<RUTROT.RUTROTDistribution,
					Where<True, Equal<False>>> RRDistribution;

		[PXViewName(CR.Messages.SalesPerson)]
		public PXSelect<EPEmployee, Where<EPEmployee.salesPersonID, Equal<Current<SOOrder.salesPersonID>>>> SalesPerson;

		[PXViewName(CR.Messages.MainContact)]
		public PXSelect<Contact> DefaultCompanyContact;

		[PXViewName(CR.Messages.Employee)]
		public PXSelect<EPEmployee, Where<EPEmployee.defContactID, Equal<Current<SOOrder.ownerID>>>> Employee;
		public override void CopyPasteGetScript(bool isImportSimple, List<PX.Api.Models.Command> script, List<PX.Api.Models.Container> containers)
		{
			script.Where(_ => _.ObjectName.StartsWith("Transactions")).ForEach(_ => _.Commit = false);
			script.Where(_ => _.ObjectName.StartsWith("Transactions")).Last().Commit = true;

			// Customer Order Nbr field may raise an exception, so it should be copied at the end.
			ProcessAtTheEnd(script, containers, nameof(SOOrder.CustomerOrderNbr));
		}

		protected virtual void ProcessAtTheEnd(List<Api.Models.Command> script, List<Api.Models.Container> containers, string fieldName)
		{
			int fieldIndex = script.FindIndex(_ => _.FieldName == fieldName);
			if (fieldIndex == -1)
				return;

			Api.Models.Command fieldCmd = script[fieldIndex];
			Api.Models.Container fieldCnt = containers[fieldIndex];

			script.Remove(fieldCmd);
			containers.Remove(fieldCnt);

			script.Add(fieldCmd);
			containers.Add(fieldCnt);
		}

		protected virtual IEnumerable defaultCompanyContact()
		{
			return OrganizationMaint.GetDefaultContactForCurrentOrganization(this);
		}

		public virtual IEnumerable adjustments()
		{
			CurrencyInfo inv_info = currencyinfo.Select();
			Adjustments_Raw.View.Clear();
			foreach (PXResult<SOAdjust, ARRegisterAlias, ARPayment> res in Adjustments_Raw.Select())
			{
				ARPayment payment = (ARPayment)res;
				SOAdjust adj = (SOAdjust)res;

				if (adj == null)
					continue;

				if (payment != null)
					PXCache<ARRegister>.RestoreCopy(payment, (ARRegisterAlias)res);

				CalculateApplicationBalance(inv_info, payment, adj);

				yield return res;
			}
		}

		public virtual void CalculateApplicationBalance(CurrencyInfo inv_info, ARPayment payment, SOAdjust adj)
		{
			var paymentCopy = PXCache<ARPayment>.CreateCopy(payment);
			CalculatePaymentBalance(paymentCopy, adj);

			decimal CuryDocBal;
			if (string.Equals(payment.CuryID, inv_info.CuryID))
			{
				CuryDocBal = (decimal)paymentCopy.CuryDocBal;
			}
			else
			{
				var docBalDiff = payment.DocBal - paymentCopy.DocBal;
				decimal docBal = (((payment.Released == true) ? payment.DocBal : payment.OrigDocAmt) ?? 0m) - (docBalDiff ?? 0m);
				PXDBCurrencyAttribute.CuryConvCury(Adjustments.Cache, inv_info, docBal, out CuryDocBal) ;
			}

			if (adj.Voided != true)
			{
				if (adj.CuryAdjdAmt > CuryDocBal)
				{
					adj.CuryDocBal = 0m;
				}
				else
				{
					adj.CuryDocBal = CuryDocBal - adj.CuryAdjdAmt;
				}
			}
		}

		protected virtual void CalculatePaymentBalance(ARPayment payment, SOAdjust adj)
		{
			SOAdjust other = PXSelectGroupBy<SOAdjust, Where<SOAdjust.adjgDocType, Equal<Required<SOAdjust.adjgDocType>>, And<SOAdjust.adjgRefNbr, Equal<Required<SOAdjust.adjgRefNbr>>, And<Where<SOAdjust.adjdOrderType, NotEqual<Required<SOAdjust.adjdOrderType>>, Or<SOAdjust.adjdOrderNbr, NotEqual<Required<SOAdjust.adjdOrderNbr>>>>>>>, Aggregate<GroupBy<SOAdjust.adjgDocType, GroupBy<SOAdjust.adjgRefNbr, Sum<SOAdjust.curyAdjgAmt, Sum<SOAdjust.adjAmt>>>>>>.Select(this, adj.AdjgDocType, adj.AdjgRefNbr, adj.AdjdOrderType, adj.AdjdOrderNbr);
			if (other != null && other.AdjdOrderNbr != null)
			{
				payment.CuryDocBal -= other.CuryAdjgAmt ?? 0m;
				payment.DocBal -= other.AdjAmt ?? 0m;
			}

			ARAdjust fromar = PXSelectGroupBy<ARAdjust, Where<ARAdjust.adjgDocType, Equal<Required<ARAdjust.adjgDocType>>, And<ARAdjust.adjgRefNbr, Equal<Required<ARAdjust.adjgRefNbr>>, And<ARAdjust.released, Equal<boolFalse>>>>, Aggregate<GroupBy<ARAdjust.adjgDocType, GroupBy<ARAdjust.adjgRefNbr, Sum<ARAdjust.curyAdjgAmt, Sum<ARAdjust.adjAmt>>>>>>.Select(this, adj.AdjgDocType, adj.AdjgRefNbr);
			if (fromar != null && fromar.AdjdRefNbr != null)
			{
				payment.CuryDocBal -= fromar.CuryAdjgAmt ?? 0m;
				payment.DocBal -= fromar.AdjAmt ?? 0m;
			}
		}

		public PXSelect<PaymentMethod,
		  Where<PaymentMethod.paymentMethodID, Equal<Optional<AR.CustomerPaymentMethod.paymentMethodID>>>> PaymentMethodDef;

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXParent(typeof(Select<SOOrder, Where<SOOrder.orderType, Equal<Current<CCProcTran.origDocType>>, And<SOOrder.orderNbr, Equal<Current<CCProcTran.origRefNbr>>>>>))]
		protected virtual void CCProcTran_RefNbr_CacheAttached(PXCache sender)
		{
		}

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXParent(typeof(Select<SOOrder, Where<SOOrder.orderType, Equal<Current<RQ.RQRequisitionOrder.orderType>>, And<SOOrder.orderNbr, Equal<Current<RQ.RQRequisitionOrder.orderNbr>>>>>))]
		protected virtual void RQRequisitionOrder_OrderNbr_CacheAttached(PXCache sender)
		{
		}

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXParent(typeof(RQ.RQRequisitionOrder.FK.Requisition))]
		protected virtual void _(Events.CacheAttached<RQ.RQRequisitionOrder.reqNbr> e)
		{
		}

		public virtual IEnumerable Invoicesplits()
		{
			List<InvoiceSplits> list = new List<InvoiceSplits>(invoicesplits.Cache.Inserted.Cast<InvoiceSplits>());

			if (list.Count > 0)
				return list; //return cached.

			int lineNbr = 0;

			if (addinvoicefilter.Current != null && addinvoicefilter.Current.RefNbr != null)
			{
				SOInvoicedRecords splits = ItemAvailabilityExt.SelectInvoicedRecords(addinvoicefilter.Current.DocType, addinvoicefilter.Current.RefNbr);

				foreach (SOInvoicedRecords.Record record in splits.Records)
				{
					bool expand = record.Transactions.Count > 0;

					if (record.Item.KitItem == true && record.Item.StkItem == false)
					{
						expand = addinvoicefilter.Current.Expand == true && record.Transactions.Count > 0;
					}

					List<InvoiceSplits> expandedList = new List<InvoiceSplits>();
					foreach (SOInvoicedRecords.INTransaction tr in record.Transactions.Values)
					{
						foreach (Tuple<INTranSplit, bool> s in tr.Splits)
						{
							if (s.Item2)//IsAvailable
							{
								InvoiceSplits invSplit = CreateInvoiceSplits(record.ARTran, record.SOLine, record.SalesPerTran, tr.Transaction, s.Item1);
								expandedList.Add(invSplit);

								if (!string.IsNullOrEmpty(s.Item1.LotSerialNbr) && Document.Current.Behavior == SOBehavior.CM && record.Item.StkItem == false)
								{
									expand = true; //force expand.
								}
							}
						}
					}

					if (expand)
					{
						foreach (InvoiceSplits split in expandedList)
						{
							split.LineNbr = lineNbr++;
							InvoiceSplits invSplit = invoicesplits.Insert(split);
							list.Add(invSplit);
						}
					}
					else
					{
						InvoiceSplits invSplit = CreateInvoiceSplits(record.ARTran, record.SOLine, record.SalesPerTran, null, null);
						invSplit.LineNbr = lineNbr++;
						invSplit = invoicesplits.Insert(invSplit);
						list.Add(invSplit);
					}
				}

			}

			return list;
		}

		public virtual InvoiceSplits CreateInvoiceSplits(ARTran artran, SOLine line, SOSalesPerTran sptran, INTran tran, INTranSplit split)
		{
			InvoiceSplits invSplit = new InvoiceSplits();
			invSplit.TranTypeARTran = artran.TranType;
			invSplit.RefNbrARTran = artran.RefNbr;
			invSplit.LineNbrARTran = artran.LineNbr;

			invSplit.OrderTypeSOLine = line.OrderType;
			invSplit.OrderNbrSOLine = line.OrderNbr;
			invSplit.LineNbrSOLine = line.LineNbr;
			invSplit.LineTypeSOLine = line.LineType;
			invSplit.TranDesc = line.TranDesc;
			invSplit.InventoryID = line.InventoryID;
			invSplit.SiteID = line.SiteID;
			invSplit.LocationID = line.LocationID;
			invSplit.LotSerialNbr = line.LotSerialNbr;
			invSplit.UOM = artran.UOM;
			invSplit.Qty = artran.Qty;
			invSplit.BaseQty = artran.BaseQty;
			invSplit.DropShip = artran.SOShipmentType == INDocType.DropShip;

			if (tran != null)
			{
				invSplit.DocTypeINTran = tran.DocType;
				invSplit.RefNbrINTran = tran.RefNbr;
				invSplit.LineNbrINTran = tran.LineNbr;
				invSplit.InventoryID = tran.InventoryID;
				invSplit.SubItemID = split.SubItemID ?? tran.SubItemID;
				invSplit.SiteID = tran.SiteID;
				invSplit.LocationID = split.LocationID ?? tran.LocationID;
				invSplit.LotSerialNbr = split.LotSerialNbr;
				invSplit.UOM = split.UOM ?? tran.UOM;
				invSplit.Qty = split.Qty ?? tran.Qty;
				invSplit.BaseQty = split.BaseQty ?? tran.BaseQty;

				invSplit.DocTypeINTranSplit = split.DocType ?? String.Empty;
				invSplit.RefNbrINTranSplit = split.RefNbr ?? String.Empty;
				invSplit.LineNbrINTranSplit = split.LineNbr ?? 0;
				invSplit.SplitLineNbrINTranSplit = split.SplitLineNbr ?? 0;
			}

			if (sptran != null)
			{
				invSplit.OrderTypeSOSalesPerTran = sptran.OrderType;
				invSplit.OrderNbrSOSalesPerTran = sptran.OrderNbr;
				invSplit.SalespersonIDSOSalesPerTran = sptran.SalespersonID;
			}

			return invSplit;
		}

		public PXSelect<IN.InventoryItem> dummy_stockitem_for_redirect_newitem;

		#endregion

		#region Buttons And Delegates

		public PXInitializeState<SOOrder> initializeState;

		public PXAction<SOOrder> createShipment;
		[PXButton(CommitChanges = true), PXUIField(DisplayName = OrderActions.DisplayNames.CreateShipment, MapEnableRights = PXCacheRights.Select, Visible = false)]
		protected virtual IEnumerable CreateShipment(PXAdapter adapter,
			[PXDate] DateTime? shipDate,
			[PXInt] int? siteID,
			[SOOperation.List] string operation)
		{
			List<SOOrder> list = adapter.Get<SOOrder>().ToList();

			if (shipDate != null)
				soparamfilter.Current.ShipDate = shipDate;
			if (siteID != null)
				soparamfilter.Current.SiteID = siteID;

			if (soparamfilter.Current.ShipDate == null)
				soparamfilter.Current.ShipDate = Accessinfo.BusinessDate;

			if (!adapter.MassProcess)
			{
				if (soparamfilter.Current.SiteID == null)
					soparamfilter.Current.SiteID = GetPreferedSiteID();
				if (adapter.ExternalCall)
					soparamfilter.AskExt(true);
			}
			if (soparamfilter.Current.SiteID != null || adapter.MassProcess)
			{
				try
				{
					RecalculateExternalTaxesSync = true;
					Save.Press();
				}
				finally
				{
					RecalculateExternalTaxesSync = false;
				}

				SOParamFilter filter = soparamfilter.Current;
				var adapterSlice = (adapter.MassProcess, adapter.QuickProcessFlow, adapter.AllowRedirect);
				string userName = Accessinfo.UserName;

				PXLongOperation.StartOperation(this, delegate ()
				{
					bool anyfailed = false;
					var orderEntry = PXGraph.CreateInstance<SOOrderEntry>();
					var shipmentEntry = CreateInstance<SOShipmentEntry>();
					var created = new DocumentList<SOShipment>(shipmentEntry);

					//address AC-92776
					for (int i = 0; i < list.Count; i++)
					{
						SOOrder order = list[i];
						if (adapterSlice.MassProcess)
							PXProcessing<SOOrder>.SetCurrentItem(order);

						List<int?> sites = new List<int?>();

						if (filter.SiteID != null)
						{
							sites.Add(filter.SiteID);
						}
						else
						{
							foreach (SOShipmentPlan plan in PXSelectJoinGroupBy<SOShipmentPlan,
								LeftJoin<SOOrderShipment,
									On<SOOrderShipment.orderType, Equal<SOShipmentPlan.orderType>,
										And<SOOrderShipment.orderNbr, Equal<SOShipmentPlan.orderNbr>,
										And<SOOrderShipment.siteID, Equal<SOShipmentPlan.siteID>,
										And<SOOrderShipment.confirmed, Equal<boolFalse>>>>>>,
								Where<SOShipmentPlan.orderType, Equal<Current<SOOrder.orderType>>,
									And<SOShipmentPlan.orderNbr, Equal<Current<SOOrder.orderNbr>>,
									And<SOOrderShipment.orderNbr, IsNull>>>,
								Aggregate<GroupBy<SOShipmentPlan.siteID>>,
								OrderBy<Asc<SOShipmentPlan.siteID>>>.SelectMultiBound(shipmentEntry, new object[] { order }))
							{
								INSite inSite = INSite.PK.Find(shipmentEntry, plan.SiteID);

								// AC-144778. We can't use Match<> inside long run operation
								if (GroupHelper.IsAccessibleToUser(shipmentEntry.Caches[typeof(INSite)], inSite, userName, forceUnattended: true))
									sites.Add(plan.SiteID);
							}
						}

						foreach (int? SiteID in sites)
						{
							SOOrder ordercopy = PXCache<SOOrder>.CreateCopy(order);
							try
							{
								using (var ts = new PXTransactionScope())
								{
									PXTransactionScope.SetSuppressWorkflow(true);
									shipmentEntry.CreateShipment(new CreateShipmentArgs
									{
										Graph = orderEntry,
										MassProcess = adapterSlice.MassProcess,
										Order = order,
										SiteID = SiteID,
										ShipDate = filter.ShipDate,
										UseOptimalShipDate = adapterSlice.MassProcess,
										Operation = operation,
										ShipmentList = created,
										QuickProcessFlow = adapterSlice.QuickProcessFlow,
									});
									ts.Complete();
								}

								if (adapterSlice.MassProcess)
									PXProcessing<SOOrder>.SetProcessed();
							}
							catch (SOShipmentException ex)
							{
								PXCache<SOOrder>.RestoreCopy(order, ordercopy);
								if (!adapterSlice.MassProcess)
									throw;

								order.LastSiteID = SiteID;
								order.LastShipDate = filter.ShipDate;

								shipmentEntry.Clear();

								orderEntry.Clear();
								orderEntry.Document.Current = order;
								orderEntry.Document.Cache.MarkUpdated(order, assertError: true);

								try
								{
									SOOrder.Events.Select(e => e.ShipmentCreationFailed).FireOn(orderEntry, order);
									orderEntry.Save.Press();

									PXTrace.WriteInformation(ex);
									PXProcessing<SOOrder>.SetWarning(ex);
								}
								catch (Exception inner)
								{
									PXCache<SOOrder>.RestoreCopy(order, ordercopy);
									PXProcessing<SOOrder>.SetError(inner);
									anyfailed = true;
								}
							}
							catch (Exception ex)
							{
								PXCache<SOOrder>.RestoreCopy(order, ordercopy);
								shipmentEntry.Clear();

								if (!adapterSlice.MassProcess)
									throw;

								PXProcessing<SOOrder>.SetError(ex);
								anyfailed = true;
							}
						}
					}
					if (adapterSlice.AllowRedirect && !adapterSlice.MassProcess && created.Count > 0)
					{
						using (new PXTimeStampScope(null))
						{
							shipmentEntry.Clear();
							shipmentEntry.Document.Current = shipmentEntry.Document.Search<SOShipment.shipmentNbr>(created[0].ShipmentNbr);
							throw new PXRedirectRequiredException(shipmentEntry, "Shipment");
						}
					}

					if (anyfailed)
						throw new PXOperationCompletedWithErrorException(ErrorMessages.SeveralItemsFailed);
				});
			}
			return list;
		}

		public PXAction<SOOrder> createShipmentIssue;
		[PXButton(CommitChanges = true), PXUIField(DisplayName = "Create Shipment", MapEnableRights = PXCacheRights.Select)]
		public virtual IEnumerable CreateShipmentIssue(PXAdapter adapter, [PXDate] DateTime? shipDate, [PXInt] int? siteID) => CreateShipment(adapter, shipDate, siteID, SOOperation.Issue);

		public PXAction<SOOrder> createShipmentReceipt;
		[PXButton(CommitChanges = true), PXUIField(DisplayName = "Create Receipt", MapEnableRights = PXCacheRights.Select)]
		public virtual IEnumerable CreateShipmentReceipt(PXAdapter adapter, [PXDate] DateTime? shipDate, [PXInt] int? siteID) => CreateShipment(adapter, shipDate, siteID, SOOperation.Receipt);

		public PXAction<SOOrder> applyAssignmentRules;
		[PXButton(CommitChanges = true), PXUIField(DisplayName = OrderActions.DisplayNames.ApplyAssignmentRules, Visible = false, MapEnableRights = PXCacheRights.Select)]
		protected virtual IEnumerable ApplyAssignmentRules(PXAdapter adapter)
		{
			if (sosetup.Current.DefaultOrderAssignmentMapID == null)
				throw new PXSetPropertyException(Messages.AssignNotSetup, Messages.SOSetup);

			List<SOOrder> list = adapter.Get<SOOrder>().ToList();
			var processor = CreateInstance<EPAssignmentProcessor<SOOrder>>();
			processor.Assign(Document.Current, sosetup.Current.DefaultOrderAssignmentMapID);
			Document.Update(Document.Current);

			return list;
		}

		public PXAction<SOOrder> createPurchaseOrder;
		[PXButton(CommitChanges = true), PXUIField(DisplayName = OrderActions.DisplayNames.CreatePurchaseOrder, MapEnableRights = PXCacheRights.Select)]
		protected virtual IEnumerable CreatePurchaseOrder(PXAdapter adapter)
		{
			List<SOOrder> list = adapter.Get<SOOrder>().ToList();

			if (list.Count > 0)
			{
				Save.Press();
				POCreate graph = PXGraph.CreateInstance<POCreate>();
				graph.Filter.Current.BranchID = Document.Current?.BranchID;
				graph.Filter.Current.OrderType = list[0].OrderType;
				graph.Filter.Current.OrderNbr = list[0].OrderNbr;
				throw new PXRedirectRequiredException(graph, PO.Messages.PurchaseOrderCreated);
			}

			return list;
		}

		public PXAction<SOOrder> createTransferOrder;
		[PXButton(CommitChanges = true), PXUIField(DisplayName = OrderActions.DisplayNames.CreateTransferOrder, MapEnableRights = PXCacheRights.Select)]
		protected virtual IEnumerable CreateTransferOrder(PXAdapter adapter)
		{
			List<SOOrder> list = adapter.Get<SOOrder>().ToList();

			if (list.Count > 0)
			{
				Save.Press();
				SOCreate graph = PXGraph.CreateInstance<SOCreate>();
				graph.Filter.Current.OrderType = list[0].OrderType;
				graph.Filter.Current.OrderNbr = list[0].OrderNbr;
				throw new PXRedirectRequiredException(graph, Messages.TransferOrderCreated);
			}

			return list;
		}

		public PXAction<SOOrder> reopenOrder;
		[PXButton(CommitChanges = true), PXUIField(DisplayName = OrderActions.DisplayNames.ReopenOrder, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		public virtual IEnumerable ReopenOrder(PXAdapter adapter) => adapter.Get();

		public PXAction<SOOrder> openOrder;
		[PXButton(CommitChanges = true), PXUIField(DisplayName = "Open Order", MapEnableRights = PXCacheRights.Select)]
		protected virtual IEnumerable OpenOrder(PXAdapter adapter) => adapter.Get<SOOrder>();

		public PXAction<SOOrder> cancelOrder;
		[PXButton(CommitChanges = true), PXUIField(DisplayName = "Cancel Order", MapEnableRights = PXCacheRights.Select)]
		protected virtual IEnumerable CancelOrder(PXAdapter adapter) => adapter.Get<SOOrder>();

		public PXAction<SOOrder> putOnHold;
		[PXButton(CommitChanges = true), PXUIField(DisplayName = "Hold")]
		protected virtual IEnumerable PutOnHold(PXAdapter adapter) => adapter.Get<SOOrder>();

		public PXAction<SOOrder> releaseFromHold;
		[PXButton(CommitChanges = true), PXUIField(DisplayName = "Remove Hold")]
		protected virtual IEnumerable ReleaseFromHold(PXAdapter adapter) => adapter.Get<SOOrder>();

		public PXAction<SOOrder> releaseFromCreditHold;
		[PXButton(CommitChanges = true), PXUIField(DisplayName = "Remove Credit Hold", MapEnableRights = PXCacheRights.Select)]
		protected virtual IEnumerable ReleaseFromCreditHold(PXAdapter adapter) => adapter.Get<SOOrder>();

		public PXAction<SOOrder> placeOnBackOrder;
		[PXButton(CommitChanges = true), PXUIField(DisplayName = "Place on Back Order", MapEnableRights = PXCacheRights.Select)]
		protected virtual IEnumerable PlaceOnBackOrder(PXAdapter adapter) => adapter.Get<SOOrder>();

		public PXAction<SOOrder> completeOrder;
		[PXButton(CommitChanges = true), PXUIField(DisplayName = "Complete Order", MapEnableRights = PXCacheRights.Select)]
		protected virtual IEnumerable CompleteOrder(PXAdapter adapter)
		{
			if (Document.Current.OpenLineCntr > 0)
			{
				foreach (var line in Transactions.SelectMain().Where(l => l.Completed != true))
				{
					line.Completed = true;
					Transactions.Update(line);
				}
			}

			return adapter.Get<SOOrder>();
		}

		private Int32? GetPreferedSiteID()
		{
			int? siteID = null;
			PXResultset<SOOrderSite> osites = PXSelectJoin<SOOrderSite,
				InnerJoin<INSite, On<SOOrderSite.FK.Site>>,
				Where<SOOrderSite.orderType, Equal<Current<SOOrder.orderType>>,
					And<SOOrderSite.orderNbr, Equal<Current<SOOrder.orderNbr>>,
					And<SOOrderSite.openLineCntr, Greater<int0>,
					And<SOOrderSite.openShipmentCntr, Equal<int0>,
					And<Match<INSite, Current<AccessInfo.userName>>>>>>>>.Select(this);
			SOOrderSite preferred;
			if (osites.Count == 1)
			{
				siteID = ((SOOrderSite)osites).SiteID;
			}
			else if ((preferred = PXSelectJoin<SOOrderSite,
						InnerJoin<INSite,
							On<SOOrderSite.FK.Site>>,
						Where<SOOrderSite.orderType, Equal<Current<SOOrder.orderType>>,
							And<SOOrderSite.orderNbr, Equal<Current<SOOrder.orderNbr>>,
								And<SOOrderSite.siteID, Equal<Current<SOOrder.defaultSiteID>>,
									And<Match<INSite, Current<AccessInfo.userName>>>>>>>.Select(this)) != null)
			{
				siteID = preferred.SiteID;
			}
			return siteID;
		}

		public PXAction<SOOrder> inquiry;
		[PXUIField(DisplayName = "Inquiries", MapEnableRights = PXCacheRights.Select)]
		[PXButton(SpecialType = PXSpecialButtonType.InquiriesFolder, MenuAutoOpen = true)]
		protected virtual IEnumerable Inquiry(PXAdapter adapter,
			[PXInt]
			[PXIntList(new int[] { }, new string[] { })]
			int? inquiryID,
			[PXString()]
			string ActionName
			)
		{
			if (!string.IsNullOrEmpty(ActionName))
			{
				PXAction action = this.Actions[ActionName];

				if (action != null)
				{
					Save.Press();
					foreach (object data in action.Press(adapter)) ;
				}
			}
			return adapter.Get();
		}

		public PXAction<SOOrder> report;
		[PXUIField(DisplayName = "Reports", MapEnableRights = PXCacheRights.Select)]
		[PXButton(SpecialType = PXSpecialButtonType.ReportsFolder, MenuAutoOpen = true)]
		public virtual IEnumerable Report(PXAdapter adapter,
			[PXString(8, InputMask = "CC.CC.CC.CC")]
			string reportID
			)
		{
			List<SOOrder> list = adapter.Get<SOOrder>().ToList();
			if (!String.IsNullOrEmpty(reportID))
			{
				Dictionary<string, string> parameters = new Dictionary<string, string>();
				string actualReportID = null;

				PXReportRequiredException ex = null;
				Dictionary<PX.SM.PrintSettings, PXReportRequiredException> reportsToPrint =
					new Dictionary<PX.SM.PrintSettings, PXReportRequiredException>();

				foreach (SOOrder order in list)
				{
					parameters = new Dictionary<string, string>();
					parameters["SOOrder.OrderType"] = order.OrderType;
					parameters["SOOrder.OrderNbr"] = order.OrderNbr;

					actualReportID =
						new NotificationUtility(this).SearchCustomerReport(reportID, order.CustomerID,
							order.BranchID);
					ex = PXReportRequiredException.CombineReport(ex, actualReportID, parameters,
						OrganizationLocalizationHelper.GetCurrentLocalization(this));
					ex.Mode = PXBaseRedirectException.WindowMode.New;

					reportsToPrint = PX.SM.SMPrintJobMaint.AssignPrintJobToPrinter(reportsToPrint, parameters,
						adapter, new NotificationUtility(this).SearchPrinter, SONotificationSource.Customer,
						reportID, actualReportID, order.BranchID, OrganizationLocalizationHelper.GetCurrentLocalization(this));
				}

				if (ex != null)
				{
					LongOperationManager.StartAsyncOperation(async ct =>
					{
						await PX.SM.SMPrintJobMaint.CreatePrintJobGroups(reportsToPrint, ct);
						throw ex;
					});
				}
			}

			return list;

		}

		public PXAction<SOOrder> printSalesOrder;
		[PXButton(CommitChanges = true), PXUIField(DisplayName = "Print Sales Order", MapEnableRights = PXCacheRights.Select)]
		public virtual IEnumerable PrintSalesOrder(PXAdapter adapter, string reportID = null) => Report(adapter.Apply(it => it.Menu = "Print Sales Order"), reportID ?? "SO641010");

		public PXAction<SOOrder> printQuote;
		[PXButton(CommitChanges = true), PXUIField(DisplayName = "Print Quote", MapEnableRights = PXCacheRights.Select)]
		public virtual IEnumerable PrintQuote(PXAdapter adapter, string reportID = null) => Report(adapter.Apply(it => it.Menu = "Print Quote"), reportID ?? "SO641000");

		public PXAction<SOOrder> notification;
		[PXUIField(DisplayName = "Notifications", Visible = false)]
		[PXButton(ImageKey = PX.Web.UI.Sprite.Main.DataEntryF)]
		public virtual IEnumerable Notification(PXAdapter adapter, [PXString] string notificationCD)
		{
			bool massProcess = adapter.MassProcess;
			var orders = adapter.Get<SOOrder>().ToArray();

			PXLongOperation.StartOperation(this, () =>
			{
				var soEntry = Lazy.By(() => PXGraph.CreateInstance<SOOrderEntry>());
				bool anyfailed = false;

				foreach (SOOrder order in orders)
			{
					if (massProcess) PXProcessing<SOOrder>.SetCurrentItem(order);

					try
					{
						soEntry.Value.Clear();

						using (var ts = new PXTransactionScope())
						{
						var parameters = new Dictionary<string, string>
						{
							[nameof(SOOrder) + "." + nameof(SOOrder.OrderType)] = order.OrderType,
							[nameof(SOOrder) + "." + nameof(SOOrder.OrderNbr)] = order.OrderNbr,
						};

						soEntry.Value.Document.Current = order;
						soEntry.Value.GetExtension<SOOrderEntry_ActivityDetailsExt>().SendNotification(ARNotificationSource.Customer, notificationCD, order.BranchID, parameters);

						if (massProcess) PXProcessing<SOOrder>.SetProcessed();

							ts.Complete();
						}
					}
					catch (Exception exception) when (massProcess)
					{
						PXProcessing<SOOrder>.SetError(exception);
						anyfailed = true;
					}
			}

				if (anyfailed)
					throw new PXOperationCompletedWithErrorException(ErrorMessages.SeveralItemsFailed);
			});

			return orders;
		}

		public PXAction<SOOrder> emailSalesOrder;
		[PXButton(CommitChanges = true), PXUIField(DisplayName = "Email Sales Order", MapEnableRights = PXCacheRights.Select)]
		public virtual IEnumerable EmailSalesOrder(
			PXAdapter adapter,
			[PXString]
			string notificationCD = null) => Notification(adapter, notificationCD ?? "SALES ORDER");

		public PXAction<SOOrder> emailQuote;
		[PXButton(CommitChanges = true), PXUIField(DisplayName = "Email Quote", MapEnableRights = PXCacheRights.Select)]
		public virtual IEnumerable EmailQuote(
			PXAdapter adapter,
			[PXString]
			string notificationCD = null) => Notification(adapter, notificationCD ?? "QUOTE");

		public PXAction<SOOrder> prepareInvoice;
		[PXButton(CommitChanges = true), PXUIField(DisplayName = OrderActions.DisplayNames.PrepareInvoice, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		public virtual IEnumerable PrepareInvoice(PXAdapter adapter)
		{
			List<SOOrder> list = adapter.Get<SOOrder>().ToList();

			foreach (SOOrder order in list)
			{
				if (this.Document.Cache.GetStatus(order) != PXEntryStatus.Inserted)
					this.Document.Cache.MarkUpdated(order, assertError: true);
			}

			if (!adapter.MassProcess)
			{
				try
				{
					RecalculateExternalTaxesSync = true;
					Save.Press();
				}
				finally
				{
					RecalculateExternalTaxesSync = false;
				}
			}

			Dictionary<string, object> arguments = adapter.Arguments;
			bool massProcess = adapter.MassProcess;
			PXQuickProcess.ActionFlow quickProcessFlow = adapter.QuickProcessFlow;

			PXLongOperation.StartOperation(this, delegate ()
			{
				var graph = CreateInstance<SOOrderEntry>();
				graph.InvoiceOrders(list, arguments, massProcess, quickProcessFlow);
			});
			return list;
		}

		protected virtual void InvoiceOrders(List<SOOrder> list, Dictionary<string, object> arguments,
			bool massProcess, PXQuickProcess.ActionFlow quickProcessFlow)
		{
			var shipmentEntry = CreateInstance<SOShipmentEntry>();
			var created = new InvoiceList(shipmentEntry);

			InvoiceOrder(arguments, list, created, massProcess, quickProcessFlow, false);

			if (massProcess) // order is updated and saved somewhere in InvoiceOrder method
				list.ForEach(o => shipmentEntry.soorder.Cache.RestoreCopy(o, SOOrder.PK.Find(shipmentEntry, o)));

			if (!massProcess && created.Count > 0)
			{
				using (new PXTimeStampScope(null))
				{
					SOInvoiceEntry ie = PXGraph.CreateInstance<SOInvoiceEntry>();
					ie.Document.Current = ie.Document.Search<ARInvoice.docType, ARInvoice.refNbr>(((ARInvoice)created[0]).DocType, ((ARInvoice)created[0]).RefNbr, ((ARInvoice)created[0]).DocType);
					throw new PXRedirectRequiredException(ie, "Invoice");
				}
			}
		}

		public PXAction<SOOrder> addInvoice;
		[PXUIField(DisplayName = Messages.AddInvoice, MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Select, Visible = true)]
		[PXLookupButton()]
		public virtual IEnumerable AddInvoice(PXAdapter adapter)
		{
			try
			{
				SOOrder order = Document.Current;
				if ((order?.IsCreditMemoOrder == true || order?.IsRMAOrder == true || order?.IsMixedOrder == true)
					&& Transactions.Cache.AllowInsert && invoicesplits.AskExt() == WebDialogResult.OK)
				{
					foreach (InvoiceSplits res in invoicesplits.Cache.Cached.RowCast<InvoiceSplits>().Where(res => res.Selected == true))
					{
						INTran tran = PXSelectReadonly<INTran,
							Where<INTran.docType, Equal<Required<INTran.docType>>,
								And<INTran.lineNbr, Equal<Required<INTran.lineNbr>>,
								And<INTran.refNbr, Equal<Required<INTran.refNbr>>>>>>
							.Select(this, res.DocTypeINTran, res.LineNbrINTran, res.RefNbrINTran);
						SOLine origLine = SOLine.PK.Find(this, res.OrderTypeSOLine, res.OrderNbrSOLine, res.LineNbrSOLine);
						ARTran artran = PXSelectReadonly<ARTran,
							Where<ARTran.lineNbr, Equal<Required<ARTran.lineNbr>>,
								And<ARTran.refNbr, Equal<Required<ARTran.refNbr>>,
								And<ARTran.tranType, Equal<Required<ARTran.tranType>>>>>>
							.Select(this, res.LineNbrARTran, res.RefNbrARTran, res.TranTypeARTran);
						ARRegister invoice = PXSelectReadonly<ARRegister,
							Where<ARRegister.docType, Equal<Required<ARRegister.docType>>,
								And<ARRegister.refNbr, Equal<Required<ARRegister.refNbr>>>>>
							.Select(this, res.TranTypeARTran, res.RefNbrARTran);

						SOLine existing = PXSelect<SOLine, Where<SOLine.orderType, Equal<Current<SOOrder.orderType>>,
									And<SOLine.orderNbr, Equal<Current<SOOrder.orderNbr>>,
									And<SOLine.origOrderType, Equal<Required<SOLine.origOrderType>>,
									And<SOLine.origOrderNbr, Equal<Required<SOLine.origOrderNbr>>,
									And<SOLine.origLineNbr, Equal<Required<SOLine.origLineNbr>>,
									And<SOLine.inventoryID, Equal<Required<SOLine.inventoryID>>,
									And<SOLine.invoiceType, Equal<Required<SOLine.invoiceType>>,
							And<SOLine.invoiceNbr, Equal<Required<SOLine.invoiceNbr>>,
							And<SOLine.invoiceLineNbr, Equal<Required<SOLine.invoiceLineNbr>>>>>>>>>>>>
							.Select(this, origLine.OrderType, origLine.OrderNbr, origLine.LineNbr, res.InventoryID,
										tran != null ? tran.ARDocType : (artran != null ? artran.TranType : null),
								tran != null ? tran.ARRefNbr : (artran != null ? artran.RefNbr : null),
								tran != null ? tran.ARLineNbr : (artran != null ? artran.LineNbr : null));
						if (existing != null)
						{
							Transactions.Current = existing;
						}
						else
						{
							SOLine newline = new SOLine();
							newline.BranchID = origLine.BranchID;

							if (tran != null)
							{
								newline.InvoiceType = tran.ARDocType;
								newline.InvoiceNbr = tran.ARRefNbr;
								newline.InvoiceLineNbr = tran.ARLineNbr;
							}
							else if (artran != null)
							{
								newline.InvoiceType = artran.TranType;
								newline.InvoiceNbr = artran.RefNbr;
								newline.InvoiceLineNbr = artran.LineNbr;
							}

							newline.InvoiceDate = artran.TranDate;
							newline.OrigOrderType = res.OrderTypeSOLine;
							newline.OrigOrderNbr = res.OrderNbrSOLine;
							newline.OrigLineNbr = res.LineNbrSOLine;
							newline.OrigShipmentType = artran.SOShipmentType;
							newline.SalesAcctID = null;
							newline.SalesSubID = null;
							newline.TaxCategoryID = origLine.TaxCategoryID;
							newline.SalesPersonID = res.SalespersonIDSOSalesPerTran;
							newline.Commissionable = artran.Commissionable;
							newline.IsSpecialOrder = origLine.IsSpecialOrder;
							newline.CostCenterID = origLine.CostCenterID;

							newline.ManualPrice = true;
							newline.ManualDisc = true;

							newline.InventoryID = res.InventoryID;
							newline.SubItemID = res.SubItemID;
							newline.SiteID = res.SiteID;
							//newline.LocationID = res.LocationID;
							newline.LotSerialNbr = res.LotSerialNbr;
							newline.UOM = (newline.InventoryID == artran.InventoryID) ? artran.UOM : res.UOM;
							newline.InvoiceUOM = newline.UOM;
							newline.CuryInfoID = Document.Current.CuryInfoID;

							if (artran?.AvalaraCustomerUsageType != null)
							{
								newline.AvalaraCustomerUsageType = artran.AvalaraCustomerUsageType;
							}

							if (origLine.LineType == SOLineType.MiscCharge || origLine.LineType == SOLineType.NonInventory || tran == null)
							{
								if (newline.InventoryID == artran.InventoryID)
									newline.UnitCost = artran.BaseQty > 0m ? (artran.TranCost / artran.BaseQty) : artran.TranCost;
							}
							else
							{
								if (newline.InventoryID == tran.InventoryID)
									newline.UnitCost = tran.Qty > 0m ? (tran.TranCost / tran.Qty) : tran.TranCost;
							}

							newline.DiscPctDR = artran.DiscPctDR;
							if (artran.CuryUnitPriceDR != null && invoice != null)
							{
								if (Document.Current.CuryID == invoice.CuryID)
								{
									newline.CuryUnitPriceDR = artran.CuryUnitPriceDR;
								}
								else
								{
									decimal unitPriceDR = 0m;
									PXDBCurrencyAttribute.CuryConvBase(this.Caches[typeof(ARTran)], artran, artran.CuryUnitPriceDR ?? 0m, out unitPriceDR, true);

									decimal orderCuryUnitPriceDR = 0m;
									PXDBCurrencyAttribute.CuryConvCury(Transactions.Cache, newline, unitPriceDR, out orderCuryUnitPriceDR, CommonSetupDecPl.PrcCst);
									newline.CuryUnitPriceDR = orderCuryUnitPriceDR;
								}
							}

							newline.DRTermStartDate = artran.DRTermStartDate;
							newline.DRTermEndDate = artran.DRTermEndDate;

							newline.ReasonCode = origLine.ReasonCode;
							newline.TaskID = artran.TaskID;
							newline.CostCodeID = artran.CostCodeID;

							if (!string.IsNullOrEmpty(artran.DeferredCode))
							{
								DRSchedule drSchedule = null;
								DRSetup dRSetup = new PXSelect<DRSetup>(this).Select();
								if (PXAccess.FeatureInstalled<FeaturesSet.aSC606>())
								{
									drSchedule = PXSelectReadonly<DRSchedule,
										Where<DRSchedule.module, Equal<BatchModule.moduleAR>,
											And<DRSchedule.docType, Equal<Required<ARTran.tranType>>,
											And<DRSchedule.refNbr, Equal<Required<ARTran.refNbr>>>>>>
										.Select(this, artran.TranType, artran.RefNbr);
								}
								else
								{
									drSchedule = PXSelectReadonly<DRSchedule,
									Where<DRSchedule.module, Equal<BatchModule.moduleAR>,
										And<DRSchedule.docType, Equal<Required<ARTran.tranType>>,
										And<DRSchedule.refNbr, Equal<Required<ARTran.refNbr>>,
										And<DRSchedule.lineNbr, Equal<Required<ARTran.lineNbr>>>>>>>
									.Select(this, artran.TranType, artran.RefNbr, artran.LineNbr);
								}
								if (drSchedule != null)
								{
									newline.DefScheduleID = drSchedule.ScheduleID;
								}
							}

							decimal CuryUnitCost;
							PXDBCurrencyAttribute.CuryConvCury(Transactions.Cache, newline, (decimal)newline.UnitCost, out CuryUnitCost, CommonSetupDecPl.PrcCst);
							newline.CuryUnitCost = CuryUnitCost;

							if (invoice != null && newline.InventoryID == artran.InventoryID)
							{
								if (Document.Current.CuryID == invoice.CuryID)
								{
									decimal UnitPrice;
									PXDBCurrencyAttribute.CuryConvBase(Transactions.Cache, newline, (decimal)artran.CuryUnitPrice, out UnitPrice, CommonSetupDecPl.PrcCst);
									newline.CuryUnitPrice = artran.CuryUnitPrice;
									newline.UnitPrice = UnitPrice;
								}
								else
								{
									decimal CuryUnitPrice;
									PXDBCurrencyAttribute.CuryConvCury(Transactions.Cache, newline, (decimal)artran.UnitPrice, out CuryUnitPrice, CommonSetupDecPl.PrcCst);
									newline.CuryUnitPrice = CuryUnitPrice;
									newline.UnitPrice = artran.UnitPrice;
								}
							}

							newline.SkipLineDiscounts = artran.SkipLineDiscounts;
							newline = Transactions.Insert(newline);

							newline.Operation = SOOperation.Receipt;
							newline = Transactions.Update(newline);

							SOSalesPerTran pertran = PXSelectReadonly<SOSalesPerTran,
								Where<SOSalesPerTran.orderNbr, Equal<Required<SOSalesPerTran.orderNbr>>,
									And<SOSalesPerTran.orderType, Equal<Required<SOSalesPerTran.orderType>>,
									And<SOSalesPerTran.salespersonID, Equal<Required<SOSalesPerTran.salespersonID>>>>>>
								.Select(this, res.OrderNbrSOSalesPerTran, res.OrderTypeSOSalesPerTran, res.SalespersonIDSOSalesPerTran);
							if (SalesPerTran.Current != null && SalesPerTran.Cache.ObjectsEqual<SOSalesPerTran.salespersonID>(pertran, SalesPerTran.Current))
							{
								SOSalesPerTran salespertran_copy = PXCache<SOSalesPerTran>.CreateCopy(SalesPerTran.Current);
								SalesPerTran.Cache.SetValueExt<SOSalesPerTran.commnPct>(SalesPerTran.Current, pertran.CommnPct);
								SalesPerTran.Cache.RaiseRowUpdated(SalesPerTran.Current, salespertran_copy);
							}

							//clear splits
							LineSplittingExt.RaiseRowDeleted(newline);

							existing = newline;
						}
						SOLine copy = PXCache<SOLine>.CreateCopy(existing);

						INTranSplit split = PXSelectReadonly<INTranSplit,
							Where<INTranSplit.docType, Equal<Required<INTranSplit.docType>>,
								And<INTranSplit.lineNbr, Equal<Required<INTranSplit.lineNbr>>,
								And<INTranSplit.refNbr, Equal<Required<INTranSplit.refNbr>>,
								And<INTranSplit.splitLineNbr, Equal<Required<INTranSplit.splitLineNbr>>>>>>>
							.Select(this, res.DocTypeINTranSplit, res.LineNbrINTranSplit, res.RefNbrINTranSplit, res.SplitLineNbrINTranSplit);
						bool processSplits = split != null && (LineSplittingExt.IsLSEntryEnabled || LineSplittingAllocatedExt.IsAllocationEntryEnabled) && (!string.IsNullOrEmpty(split.LotSerialNbr) || LineSplittingExt.IsLocationEnabled);
						if (!processSplits)
						{
							copy.BaseQty += copy.LineSign * res.BaseQty;
						}

						if (copy.BaseQty == 0m)
						{
							if (Document.Current.CuryID == invoice.CuryID)
							{
								decimal LineAmt;
								PXDBCurrencyAttribute.CuryConvBase<SOLine.curyInfoID>(Transactions.Cache, copy, (decimal)artran.CuryTranAmt, out LineAmt);
								copy.CuryLineAmt = artran.CuryTranAmt;
								copy.LineAmt = LineAmt;
							}
							else
							{
								decimal CuryLineAmt;
								PXDBCurrencyAttribute.CuryConvCury<SOLine.curyInfoID>(Transactions.Cache, copy, (decimal)artran.TranAmt, out CuryLineAmt);
								copy.CuryLineAmt = CuryLineAmt;
								copy.LineAmt = artran.TranAmt;
							}
						}

						PXDBQuantityAttribute.CalcTranQty<SOLine.orderQty>(Transactions.Cache, copy);

						try
						{
							copy = Transactions.Update(copy);
						}
						catch (PXSetPropertyException) {; }

						if (processSplits)
						{
							SOLineSplit newsplit = new SOLineSplit();
							newsplit.SubItemID = split.SubItemID;
							if (LineSplittingExt.IsLocationEnabled)
								newsplit.LocationID = split.LocationID;
							newsplit.LotSerialNbr = split.LotSerialNbr;
							newsplit.ExpireDate = split.ExpireDate;
							newsplit.UOM = split.UOM;

							newsplit = splits.Insert(newsplit);
							newsplit.Qty = split.Qty;
							newsplit = splits.Update(newsplit);
							string error = PXUIFieldAttribute.GetError<SOLineSplit.qty>(splits.Cache, newsplit);

							if (!string.IsNullOrEmpty(error))
							{
								newsplit.Qty = 0;
								newsplit = splits.Update(newsplit);
							}
						}

						decimal DiscAmt;
						decimal CuryDiscAmt;

						if (Document.Current.CuryID == invoice.CuryID)
						{
							PXDBCurrencyAttribute.CuryConvBase<SOLine.curyInfoID>(Transactions.Cache, copy, (decimal)artran.CuryDiscAmt, out DiscAmt);
							CuryDiscAmt = (decimal)artran.CuryDiscAmt;
						}
						else
						{
							PXDBCurrencyAttribute.CuryConvCury<SOLine.curyInfoID>(Transactions.Cache, copy, (decimal)artran.DiscAmt, out CuryDiscAmt);
							DiscAmt = (decimal)artran.DiscAmt;
						}

						if (artran.Qty != copy.Qty)
						{
							copy.CuryDiscAmt = (CuryDiscAmt / artran.Qty) * copy.Qty;
							copy.DiscAmt = (DiscAmt / artran.Qty) * copy.Qty;
						}
						else
						{
							copy.CuryDiscAmt = CuryDiscAmt;
							copy.DiscAmt = DiscAmt;
						}

						copy.DiscPct = artran.DiscPct;
						copy.FreezeManualDisc = true;

						try
						{
							copy = Transactions.Update(copy);
						}
						catch (PXSetPropertyException) {; }

						if (artran.DiscountsAppliedToLine?.Any() == true)
						{
							Transactions.Cache.RaiseExceptionHandling<SOLine.invoiceNbr>(copy, copy.InvoiceNbr, new PXSetPropertyException(Messages.DiscountsWereNotCopiedToReturnOrder, PXErrorLevel.RowWarning, copy.InvoiceNbr));
						}
					}
				}

				if (addinvoicefilter.Current != null)
				{
					if (!IsImport)
						addinvoicefilter.Current.RefNbr = null;
					else
						addinvoicefilter.Current = null;
				}
			}
			finally
			{
				this.invoicesplits.Cache.Clear();
				this.invoicesplits.View.Clear();
			}

			return adapter.Get();
		}

		public PXAction<SOOrder> addInvoiceOK;
		[PXUIField(
			DisplayName = "Add",
			MapEnableRights = PXCacheRights.Select,
			MapViewRights = PXCacheRights.Select)]
		[PXLookupButton(DisplayOnMainToolbar = false)]
		public virtual IEnumerable AddInvoiceOK(PXAdapter adapter)
		{
			invoicesplits.View.Answer = WebDialogResult.OK;

			return AddInvoice(adapter);
		}

		public PXAction<SOOrder> checkCopyParams;
		[PXUIField(DisplayName = "OK", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXLookupButton(IgnoresArchiveDisabling = true)]
		public virtual IEnumerable CheckCopyParams(PXAdapter adapter)
		{
			return adapter.Get();
		}

		//This is a temporary solution to be able to have "Copy Order" action in Processing category for QT orders.
		public PXAction<SOOrder> copyOrderQT;
		[PXButton(CommitChanges = true, IgnoresArchiveDisabling = true), PXUIField(DisplayName = "Copy Order", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		public virtual IEnumerable CopyOrderQT(PXAdapter adapter)
		{
			return CopyOrder(adapter);
		}

		public PXAction<SOOrder> copyOrder;
		[PXButton(CommitChanges = true, IgnoresArchiveDisabling = true), PXUIField(DisplayName = "Copy Order", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		public virtual IEnumerable CopyOrder(PXAdapter adapter)
		{
			List<SOOrder> list = adapter.Get<SOOrder>().ToList();
			WebDialogResult dialogResult = copyparamfilter.AskExt(setStateFilter, true);
			if ((dialogResult == WebDialogResult.OK || (this.IsContractBasedAPI && dialogResult == WebDialogResult.Yes)) && string.IsNullOrEmpty(copyparamfilter.Current.OrderType) == false)
			{
				this.Save.Press();
				SOOrder order = PXCache<SOOrder>.CreateCopy(Document.Current);

				IsCopyOrder = true;
				try
				{
					using (IsArchiveContext == true ? new PXReadThroughArchivedScope() : null)
						this.CopyOrderProc(order, copyparamfilter.Current);
				}
				finally
				{
					IsCopyOrder = false;
				}

				List<SOOrder> rs = new List<SOOrder> { Document.Current };
				return rs;
			}
			return list;
		}

		private void setStateFilter(PXGraph aGraph, string ViewName)
		{
			checkCopyParams.SetEnabled(!string.IsNullOrEmpty(copyparamfilter.Current.OrderType) && !string.IsNullOrEmpty(copyparamfilter.Current.OrderNbr));
		}


		public virtual void CopyOrderProc(SOOrder sourceOrder, CopyParamFilter copyFilter)
		{
			string newOrderType = copyFilter.OrderType;
			string newOrderNbr = copyFilter.OrderNbr;
			bool recalcUnitPrices = (bool)copyFilter.RecalcUnitPrices;
			bool overrideManualPrices = (bool)copyFilter.OverrideManualPrices;
			bool recalcDiscounts = (bool)copyFilter.RecalcDiscounts;
			bool overrideManualDiscounts = (bool)copyFilter.OverrideManualDiscounts;
			bool disableTaxRecalculation = false;

			var userDefinedFieldValues = Document.Cache.Fields
				.Where(Document.Cache.IsKvExtAttribute)
				.ToDictionary(
					udField => udField,
					udField => ((PXFieldState)Document.Cache.GetValueExt(sourceOrder, udField))?.Value);

			SOOrderType ordertype = soordertype.SelectWindowed(0, 1, sourceOrder.OrderType);

			this.Clear(PXClearOption.PreserveTimeStamp);

			PXResultset<SOOrder> orderWithCurrency =
				PXSelectJoin<SOOrder,
				InnerJoin<CurrencyInfo, On<CurrencyInfo.curyInfoID, Equal<SOOrder.curyInfoID>>>,
				Where<SOOrder.orderType, Equal<Required<SOOrder.orderType>>,
					And<SOOrder.orderNbr, Equal<Required<SOOrder.orderNbr>>>>>
				.Select(this, sourceOrder.OrderType, sourceOrder.OrderNbr);
			foreach (PXResult<SOOrder, CurrencyInfo> res in orderWithCurrency)
			{
				SOOrder orderBeingCopied = (SOOrder)res;
				CurrencyInfo currencyInfo = (CurrencyInfo)res;

				if (orderBeingCopied.Behavior == SOBehavior.QT)
				{
					orderBeingCopied.MarkCompleted();
					Document.Cache.MarkUpdated(orderBeingCopied, assertError: true);
				}

				CurrencyInfo info = PXCache<CurrencyInfo>.CreateCopy(currencyInfo);
				info.CuryInfoID = null;
				info.IsReadOnly = false;
				info = this.currencyinfo.Insert(info);
				CurrencyInfo copyinfo = PXCache<CurrencyInfo>.CreateCopy(info);

				var newOrder = new SOOrder
				{
					CuryInfoID = info.CuryInfoID,
					OrderType = newOrderType,
					OrderNbr = newOrderNbr,
					GroundCollect = orderBeingCopied.GroundCollect
				};

				if (PXAccess.FeatureInstalled<FeaturesSet.multipleBaseCurrencies>())
					newOrder.BranchID = orderBeingCopied.BranchID;

				newOrder = Document.Insert(newOrder);

				//Automation
				newOrder = Document.Search<SOOrder.orderNbr>(newOrder.OrderNbr, newOrder.OrderType);

				//Disable tax calculation for freight as well
				TaxBaseAttribute.SetTaxCalc<SOOrder.freightTaxCategoryID>(this.Document.Cache, null, TaxCalc.ManualCalc);

				SOOrder targetOrder = PXCache<SOOrder>.CreateCopy(orderBeingCopied);

				targetOrder.OwnerID = newOrder.OwnerID;
				targetOrder.WorkgroupID = null;
				targetOrder.OrderType = newOrder.OrderType;
				targetOrder.OrderNbr = newOrder.OrderNbr;
				targetOrder.Behavior = newOrder.Behavior;
				targetOrder.ARDocType = newOrder.ARDocType;
				targetOrder.DefaultOperation = newOrder.DefaultOperation;
				targetOrder.DefaultTranType = newOrder.DefaultTranType;
				targetOrder.ShipAddressID = newOrder.ShipAddressID;
				targetOrder.ShipContactID = newOrder.ShipContactID;
				targetOrder.BillAddressID = newOrder.BillAddressID;
				targetOrder.BillContactID = newOrder.BillContactID;
				targetOrder.IsCashSaleOrder = newOrder.IsCashSaleOrder;
				targetOrder.IsCreditMemoOrder = newOrder.IsCreditMemoOrder;
				targetOrder.IsDebitMemoOrder = newOrder.IsDebitMemoOrder;
				targetOrder.IsInvoiceOrder = newOrder.IsInvoiceOrder;
				targetOrder.IsNoAROrder = newOrder.IsNoAROrder;
				targetOrder.IsPaymentInfoEnabled = newOrder.IsPaymentInfoEnabled;
				targetOrder.IsRMAOrder = newOrder.IsRMAOrder;
				targetOrder.IsMixedOrder = newOrder.IsMixedOrder;
				targetOrder.IsTransferOrder = newOrder.IsTransferOrder;
				targetOrder.IsUserInvoiceNumbering = newOrder.IsUserInvoiceNumbering;
				targetOrder.OrigOrderType = orderBeingCopied.OrderType;
				targetOrder.OrigOrderNbr = orderBeingCopied.OrderNbr;
				targetOrder.ShipmentCntr = 0;
				targetOrder.OpenShipmentCntr = 0;
				targetOrder.OpenSiteCntr = 0;
				targetOrder.OpenLineCntr = 0;
				targetOrder.ReleasedCntr = 0;
				targetOrder.BilledCntr = 0;
				targetOrder.OrderQty = 0m;
				targetOrder.OrderWeight = 0m;
				targetOrder.OrderVolume = 0m;
				targetOrder.OpenOrderQty = 0m;
				targetOrder.UnbilledOrderQty = 0m;
				targetOrder.CuryInfoID = newOrder.CuryInfoID;
				targetOrder.PrepaymentReqSatisfied = newOrder.PrepaymentReqSatisfied;
				targetOrder.Status = newOrder.Status;
				targetOrder.Hold = newOrder.Hold;
				targetOrder.Approved = newOrder.Approved;
				targetOrder.CreditHold = newOrder.CreditHold;
				targetOrder.Completed = newOrder.Completed;
				targetOrder.Cancelled = newOrder.Cancelled;
				targetOrder.InclCustOpenOrders = newOrder.InclCustOpenOrders;
				targetOrder.OrderDate = newOrder.OrderDate;
				targetOrder.CuryGoodsExtPriceTotal = 0m;
				targetOrder.CuryMiscTot = 0m;
				targetOrder.CuryMiscExtPriceTotal = 0m;
				targetOrder.CuryDetailExtPriceTotal = 0m;
				targetOrder.CuryUnbilledMiscTot = 0m;
				targetOrder.CuryLineTotal = 0m;
				targetOrder.CuryOpenLineTotal = 0m;
				targetOrder.CuryUnbilledLineTotal = 0m;
				targetOrder.CuryVatExemptTotal = 0m;
				targetOrder.CuryVatTaxableTotal = 0m;
				targetOrder.CuryTaxTotal = 0m;
				targetOrder.CuryOrderTotal = 0m;
				targetOrder.CuryOpenOrderTotal = 0m;
				targetOrder.CuryOpenTaxTotal = 0m;
				targetOrder.CuryUnbilledOrderTotal = 0m;
				targetOrder.CuryUnbilledTaxTotal = 0m;
				targetOrder.CuryUnbilledDiscTotal = 0m;
				targetOrder.CuryOpenDiscTotal = 0m;
				targetOrder.CuryPaymentTotal = 0m;
				targetOrder.CuryUnreleasedPaymentAmt = 0m;
				targetOrder.CuryCCAuthorizedAmt = 0m;
				targetOrder.CuryPaidAmt = 0m;
				targetOrder.CuryBilledPaymentTotal = 0m;
				targetOrder.CuryPaymentOverall = 0m;
				targetOrder.CurySalesCostTotal = 0m;
				targetOrder.CuryNetSalesTotal = 0m;
				targetOrder.CuryLineDiscTotal = 0m;
				targetOrder.CuryGroupDiscTotal = 0m;
				targetOrder.FreightTaxCategoryID = null;
				targetOrder.CreatedByID = newOrder.CreatedByID;
				targetOrder.CreatedByScreenID = newOrder.CreatedByScreenID;
				targetOrder.CreatedDateTime = newOrder.CreatedDateTime;
				targetOrder.DisableAutomaticDiscountCalculation = orderBeingCopied.DisableAutomaticDiscountCalculation;
				targetOrder.ApprovedCredit = false;
				targetOrder.ApprovedCreditByPayment = false;
				targetOrder.ApprovedCreditAmt = 0m;
				targetOrder.PackageWeight = 0m;
				targetOrder.Emailed = false;
				targetOrder.Printed = false;

				//Blanket-order/child order-specific fields
				targetOrder.BlanketLineCntr = 0;
				targetOrder.ChildLineCntr = 0;
				targetOrder.QtyOnOrders = 0m;
				targetOrder.BlanketOpenQty = 0m;
				targetOrder.CuryTransferredToChildrenPaymentTotal = 0m;
				targetOrder.TransferredToChildrenPaymentTotal = 0m;

				if (targetOrder.RequestDate < newOrder.OrderDate)
				{
					targetOrder.RequestDate = newOrder.RequestDate;
				}

				if (targetOrder.ShipDate < newOrder.OrderDate)
				{
					targetOrder.ShipDate = newOrder.ShipDate;
				}

				if (orderBeingCopied.Behavior == SOBehavior.QT)
				{
					targetOrder.BillSeparately = newOrder.BillSeparately;
					targetOrder.ShipSeparately = newOrder.ShipSeparately;
				}

				if (orderBeingCopied.Behavior != SOBehavior.QT)
				{
					Document.Cache.SetDefaultExt<SOOrder.disableAutomaticTaxCalculation>(targetOrder);
				}
				disableTaxRecalculation = targetOrder.DisableAutomaticTaxCalculation ?? false;

				Document.Cache.SetDefaultExt<SOOrder.invoiceDate>(targetOrder);
				Document.Cache.SetDefaultExt<SOOrder.finPeriodID>(targetOrder);
				targetOrder.ExtRefNbr = null;
				targetOrder.NoteID = null;
				Document.Cache.ForceExceptionHandling = true;

				PXFieldDefaulting cancelDefaulting = (c, e) => { e.Cancel = true; e.NewValue = null; };
				FieldDefaulting.AddHandler<SOOrder.shipTermsID>(cancelDefaulting);
				try
				{
					targetOrder = Document.Update(targetOrder);
				}
				finally
				{
					FieldDefaulting.RemoveHandler<SOOrder.shipTermsID>(cancelDefaulting);
				}

				PXNoteAttribute.CopyNoteAndFiles(Document.Cache, orderBeingCopied, Document.Cache, targetOrder, ordertype);

				if (orderBeingCopied.Behavior.IsIn(SOBehavior.QT, targetOrder.Behavior))
					foreach ((string fieldName, object value) in userDefinedFieldValues)
						Document.Cache.SetValueExt(targetOrder, fieldName, value);

				if (info != null)
				{
					info.CuryID = copyinfo.CuryID;
					info.CuryEffDate = copyinfo.CuryEffDate;
					info.CuryRateTypeID = copyinfo.CuryRateTypeID;
					info.CuryRate = copyinfo.CuryRate;
					info.RecipRate = copyinfo.RecipRate;
					info.CuryMultDiv = copyinfo.CuryMultDiv;
					this.currencyinfo.Update(info);
				}
			}
			AddressAttribute.CopyRecord<SOOrder.billAddressID>(Document.Cache, Document.Current, sourceOrder, false);
			SOBillingAddress origBillAddress = SOBillingAddress.PK.Find(this, sourceOrder.BillAddressID);
			if (origBillAddress != null && origBillAddress.IsValidated == true && Billing_Address.Current != null)
				Billing_Address.Current.IsValidated = origBillAddress.IsValidated;
			ContactAttribute.CopyRecord<SOOrder.billContactID>(Document.Cache, Document.Current, sourceOrder, false);

			AddressAttribute.CopyRecord<SOOrder.shipAddressID>(Document.Cache, Document.Current, sourceOrder, false);
			SOShippingAddress origShipAddress = SOShippingAddress.PK.Find(this, sourceOrder.ShipAddressID);
			if (origShipAddress != null && origShipAddress.IsValidated == true && Shipping_Address.Current != null)
				Shipping_Address.Current.IsValidated = origShipAddress.IsValidated;
			ContactAttribute.CopyRecord<SOOrder.shipContactID>(Document.Cache, Document.Current, sourceOrder, false);

			OrderCreated(Document.Current, sourceOrder);
			bool exceptionHappenedOnSOLineCopying = false;

			TaxBaseAttribute.SetTaxCalc<SOLine.taxCategoryID>(this.Transactions.Cache, null, TaxCalc.ManualCalc);

			ReloadCustomerCreditRule();

			string[] notSupportingPOtoSOLinkBehaviors = { SOBehavior.QT, SOBehavior.CM, SOBehavior.IN, SOBehavior.MO };
			string[] notSupportingPOtoSORedefaultingBehaviors = { SOBehavior.CM, SOBehavior.IN, SOBehavior.MO }; // it is allowed to redefault PO Source when copying from QT order

			PXResultset<SOLine> sourceSOLines =
				PXSelectReadonly<SOLine,
				Where<SOLine.orderType, Equal<Required<SOLine.orderType>>,
					And<SOLine.orderNbr, Equal<Required<SOLine.orderNbr>>>>>
				.Select(this, sourceOrder.OrderType, sourceOrder.OrderNbr);
			foreach (SOLine sourceSOLine in sourceSOLines)
			{
				SOLine targetSOLine = PXCache<SOLine>.CreateCopy(sourceSOLine);
				targetSOLine.OrigOrderType = targetSOLine.OrderType;
				targetSOLine.OrigOrderNbr = targetSOLine.OrderNbr;
				targetSOLine.OrigLineNbr = targetSOLine.LineNbr;
				targetSOLine.Behavior = null;
				targetSOLine.OrderType = null;
				targetSOLine.OrderNbr = null;
				targetSOLine.InvtMult = null;
				targetSOLine.CuryInfoID = null;
				targetSOLine.PlanType = null;
				targetSOLine.TranType = null;
				targetSOLine.RequireShipping = null;
				targetSOLine.RequireAllocation = null;
				targetSOLine.RequireLocation = null;
				targetSOLine.RequireReasonCode = null;
				targetSOLine.OpenLine = null;
				targetSOLine.Completed = false;
				targetSOLine.Cancelled = false;
				targetSOLine.CancelDate = null;
				targetSOLine.IsLegacyDropShip = false;
				//targetSOLine.POType = null;
				//targetSOLine.PONbr = null;
				//targetSOLine.POLineNbr = null;
				targetSOLine.OrderDate = null;
				targetSOLine.POCreated = null;
				targetSOLine.LineType = null;
				targetSOLine.IsStockItem = null;
				targetSOLine.AutomaticDiscountsDisabled = Document.Current.Behavior == SOBehavior.BL;

				//Blanket-order/child order-specific fields
				targetSOLine.BlanketType = null;
				targetSOLine.BlanketNbr = null;
				targetSOLine.BlanketLineNbr = null;
				targetSOLine.BlanketSplitLineNbr = null;
				targetSOLine.QtyOnOrders = 0m;
				targetSOLine.BaseQtyOnOrders = 0m;
				targetSOLine.ChildLineCntr = 0;
				targetSOLine.OpenChildLineCntr = 0;

				if (targetSOLine.RequestDate < Document.Current.OrderDate)
				{
					targetSOLine.RequestDate = null;
				}
				if (targetSOLine.ShipDate < Document.Current.OrderDate)
				{
					targetSOLine.ShipDate = null;
				}

				if (notSupportingPOtoSOLinkBehaviors.Contains(sourceOrder.Behavior) || notSupportingPOtoSOLinkBehaviors.Contains(Document.Current.Behavior))
				{
					targetSOLine.POCreate = null;
					targetSOLine.POSource = null;
				}

				if (soordertype.Current.RequireLocation == true && targetSOLine.ShipComplete == SOShipComplete.BackOrderAllowed)
				{
					targetSOLine.ShipComplete = null;
				}

				if (soordertype.Current.RequireLocation == false)
				{
					targetSOLine.LocationID = null;
					targetSOLine.LotSerialNbr = null;
					targetSOLine.ExpireDate = null;
				}

				targetSOLine.InvoiceType = null;
				targetSOLine.InvoiceNbr = null;
				targetSOLine.InvoiceLineNbr = null;
				targetSOLine.InvoiceUOM = null;
				targetSOLine.InvoiceDate = null;

				targetSOLine.DefaultOperation = null;
				targetSOLine.Operation = null;
				targetSOLine.LineSign = null;

				if (targetSOLine.IsFree == true && targetSOLine.ManualDisc == false && recalcDiscounts)
				{
					continue;
				}

				if (overrideManualDiscounts)
				{
					targetSOLine.ManualDisc = false;
				}

				if (recalcUnitPrices && targetSOLine.ManualPrice != true)
				{
					targetSOLine.CuryUnitPrice = null;
					targetSOLine.CuryExtPrice = null;
				}

				if (overrideManualPrices)
				{
					targetSOLine.ManualPrice = false;
				}

				if (!recalcDiscounts)
				{
					targetSOLine.ManualDisc = true;
					targetSOLine.SkipDisc = true;
				}

				if (soordertype.Current.ActiveOperationsCntr <= 1)
				{
					var lineType = (string)PXFormulaAttribute.Evaluate<SOLine.lineType>(Transactions.Cache, targetSOLine);
					if (targetSOLine.OrderQty < 0m && lineType.IsIn(SOLineType.Inventory, SOLineType.NonInventory))
					{
						targetSOLine.OrderQty = -targetSOLine.OrderQty;
						targetSOLine.CuryExtPrice = -targetSOLine.CuryExtPrice;
						targetSOLine.CuryDiscAmt = -targetSOLine.CuryDiscAmt;
					}
					targetSOLine.AutoCreateIssueLine = false;
				}
				targetSOLine.UnassignedQty = 0m;
				targetSOLine.OpenQty = null;
				targetSOLine.ClosedQty = 0m;
				targetSOLine.BilledQty = 0m;
				targetSOLine.UnbilledQty = null;
				targetSOLine.ShippedQty = 0m;
				targetSOLine.CuryBilledAmt = 0m;
				targetSOLine.CuryUnbilledAmt = null;
				targetSOLine.CuryOpenAmt = null;
				targetSOLine.CuryLineAmt = null;
				targetSOLine.CuryNetSales = null;
				targetSOLine.CuryMarginAmt = null;
				targetSOLine.MarginPct = null;

				if (recalcDiscounts)
                {
                    targetSOLine.DocumentDiscountRate = 1;
                    targetSOLine.GroupDiscountRate = 1;
                    targetSOLine.DiscountsAppliedToLine = new ushort[0];
                }

				targetSOLine.NoteID = null;

				if (Document.Current.Behavior.IsNotIn(SOBehavior.SO, SOBehavior.RM, SOBehavior.QT)
					|| targetSOLine.Operation == SOOperation.Receipt)
				{
					targetSOLine.IsSpecialOrder = false;
				}
				targetSOLine.CostCenterID = null;

				try
				{
					FieldUpdated.RemoveHandler<SOLine.discountID>(SOLine_DiscountID_FieldUpdated);
					FieldDefaulting.RemoveHandler<SOLine.salesAcctID>(SOLine_SalesAcctID_FieldDefaulting);
					FieldDefaulting.RemoveHandler<SOLine.salesSubID>(SOLine_SalesSubID_FieldDefaulting);
					try
					{
						try
						{
							Transactions.Cache.ForceExceptionHandling = true;
							targetSOLine = Transactions.Insert(targetSOLine);
						}
						catch (AlternatieIDNotUniqueException ex)
						{
							targetSOLine.AlternateID = null;
							targetSOLine = Transactions.Insert(targetSOLine);
							if (targetSOLine != null)
								Transactions.Cache.RaiseExceptionHandling<SOLine.alternateID>(targetSOLine, null, ex);
						}

						if (targetSOLine == null)
							continue;

						PXNoteAttribute.CopyNoteAndFiles(Transactions.Cache, sourceSOLine, Transactions.Cache, targetSOLine, ordertype);

						bool clearMarkForPO = notSupportingPOtoSORedefaultingBehaviors.Contains(sourceOrder.Behavior)
											  || notSupportingPOtoSOLinkBehaviors.Contains(Document.Current.Behavior)
											  || targetSOLine.Operation == SOOperation.Receipt;
						if (clearMarkForPO)
						{
							targetSOLine.POCreate = false;
							targetSOLine.POSource = null;
						}

						if (targetSOLine.Operation == SOOperation.Issue) //issue lines don't support AutoCreateIssueLine functionality
							targetSOLine.AutoCreateIssueLine = false;

						if (targetSOLine.IsSpecialOrder != true)
							Transactions.Cache.SetDefaultExt<SOLine.unitCost>(targetSOLine);

						Transactions.Update(targetSOLine);
					}
					catch (PXSetPropertyException)
					{
						exceptionHappenedOnSOLineCopying = true;
					}
				}
				finally
				{
					this.FieldUpdated.AddHandler<SOLine.discountID>(SOLine_DiscountID_FieldUpdated);
					FieldDefaulting.AddHandler<SOLine.salesAcctID>(SOLine_SalesAcctID_FieldDefaulting);
					FieldDefaulting.AddHandler<SOLine.salesSubID>(SOLine_SalesSubID_FieldDefaulting);
				}
			}

			bool recalcTaxes = (exceptionHappenedOnSOLineCopying || recalcDiscounts || recalcUnitPrices || overrideManualDiscounts || overrideManualPrices) && !disableTaxRecalculation;

			if (recalcTaxes)
			{
				TaxBaseAttribute.SetTaxCalc<SOLine.taxCategoryID>(this.Transactions.Cache, null, TaxCalc.ManualCalc);
			}

			PXResultset<SOTaxTran> sourceSOTaxTrans =
				PXSelect<SOTaxTran,
				Where<SOTaxTran.orderType, Equal<Required<SOTaxTran.orderType>>,
					And<SOTaxTran.orderNbr, Equal<Required<SOTaxTran.orderNbr>>>>>
				.Select(this, sourceOrder.OrderType, sourceOrder.OrderNbr);
			foreach (SOTaxTran sourceTaxTran in sourceSOTaxTrans)
			{
				var targetTaxTran = new SOTaxTran
				{
					OrderType = Document.Current.OrderType,
					OrderNbr = Document.Current.OrderNbr,
					LineNbr = int.MaxValue,
					TaxID = sourceTaxTran.TaxID
				};

				targetTaxTran = this.Taxes.Insert(targetTaxTran);

				if (!recalcTaxes && targetTaxTran != null)
				{
					targetTaxTran = PXCache<SOTaxTran>.CreateCopy(targetTaxTran);
					targetTaxTran.TaxRate = sourceTaxTran.TaxRate;
					targetTaxTran.CuryTaxableAmt = sourceTaxTran.CuryTaxableAmt;
					targetTaxTran.CuryExemptedAmt = sourceTaxTran.CuryExemptedAmt;
					targetTaxTran.CuryTaxAmt = sourceTaxTran.CuryTaxAmt;

					targetTaxTran.CuryUnshippedTaxableAmt = sourceTaxTran.CuryTaxableAmt;
					targetTaxTran.CuryUnshippedTaxAmt = sourceTaxTran.CuryTaxAmt;
					targetTaxTran.CuryUnbilledTaxableAmt = sourceTaxTran.CuryTaxableAmt;
					targetTaxTran.CuryUnbilledTaxAmt = sourceTaxTran.CuryTaxAmt;

					this.Taxes.Update(targetTaxTran);
				}
			}

			if (sourceOrder.FreightTaxCategoryID != null)
			{
				TaxBaseAttribute.SetTaxCalc<SOOrder.freightTaxCategoryID>(this.Document.Cache, null, TaxCalc.ManualLineCalc);
				Document.Current.FreightTaxCategoryID = sourceOrder.FreightTaxCategoryID;
				Document.Update(Document.Current);
			}

			if (recalcTaxes)
			{
				TaxBaseAttribute.SetTaxCalc<SOLine.taxCategoryID>(this.Transactions.Cache, null, TaxCalc.ManualLineCalc);
			}

			if (!DisableGroupDocDiscount)
			{
				//copy all discounts except free-items:
				PXResultset<SOOrderDiscountDetail> soOrderDiscountDetails =
					PXSelect<SOOrderDiscountDetail,
					Where<SOOrderDiscountDetail.orderType, Equal<Required<SOOrderDiscountDetail.orderType>>,
						And<SOOrderDiscountDetail.orderNbr, Equal<Required<SOOrderDiscountDetail.orderNbr>>,
						And<SOOrderDiscountDetail.freeItemID, IsNull>>>>
					.Select(this, sourceOrder.OrderType, sourceOrder.OrderNbr);
				foreach (SOOrderDiscountDetail sourceOrderDiscount in soOrderDiscountDetails)
				{
					if (!recalcDiscounts || sourceOrderDiscount.IsManual == true)
					{
						SOOrderDiscountDetail targetOrderDiscount = PXCache<SOOrderDiscountDetail>.CreateCopy(sourceOrderDiscount);
						targetOrderDiscount.OrderType = Document.Current.OrderType;
						targetOrderDiscount.OrderNbr = Document.Current.OrderNbr;
						targetOrderDiscount.IsManual = true;
						_discountEngine.InsertDiscountDetail(this.DiscountDetails.Cache, DiscountDetails, targetOrderDiscount);
					}
				}
			}

			RecalcDiscountsParamFilter filter = recalcdiscountsfilter.Current;
			filter.OverrideManualDiscounts = overrideManualDiscounts;
			filter.OverrideManualDocGroupDiscounts = overrideManualDiscounts;
			filter.OverrideManualPrices = overrideManualPrices;
			filter.RecalcDiscounts = recalcDiscounts;
			filter.RecalcUnitPrices = recalcUnitPrices;
			filter.RecalcTarget = RecalcDiscountsParamFilter.AllLines;
			_discountEngine.RecalculatePricesAndDiscounts(
				cache: Transactions.Cache,
				lines: Transactions,
				currentLine: Transactions.Current,
				discountDetails: DiscountDetails,
				locationID: Document.Current.CustomerLocationID,
				date: Document.Current.OrderDate,
				recalcFilter: filter,
				discountCalculationOptions: DiscountEngine.DefaultARDiscountCalculationParameters);

			RecalculateTotalDiscount();

			RefreshFreeItemLines(Transactions.Cache);
		}

		public virtual void ReloadCustomerCreditRule()
		{
			if (this.customer.Current != null && this.customer.Current.CreditRule == null)
			{
				Customer cust = Customer.PK.Find(this, Document.Current?.CustomerID);
				if (cust != null)
					this.customer.Current.CreditRule = cust.CreditRule;
			}
		}

		public delegate void OrderCreatedDelegate(SOOrder document, SOOrder source);
		protected virtual void OrderCreated(SOOrder document, SOOrder source)
		{

		}
		public PXAction<SOOrder> itemAvailability;
		[PXUIField(DisplayName = "Item Availability", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXLookupButton()]
		public virtual IEnumerable ItemAvailability(PXAdapter adapter)
		{
			PXCache tCache = Transactions.Cache;
			SOLine line = Transactions.Current;
			if (line == null) return adapter.Get();

			InventoryItem item = InventoryItem.PK.Find(this, line.InventoryID);
			if (item != null && item.StkItem == true)
			{
				INSubItem sbitem = (INSubItem)PXSelectorAttribute.Select<SOLine.subItemID>(tCache, line);

				InventoryAllocDetEnq.Redirect(item.InventoryID,
											 ((sbitem != null) ? sbitem.SubItemCD : null),
											 line.LotSerialNbr,
											 line.SiteID,
											 line.LocationID,
											 PXBaseRedirectException.WindowMode.New);
			}
			return adapter.Get();
		}

		public PXAction<SOOrder> calculateFreight;
		[PXUIField(DisplayName = Messages.RefreshFreight, MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update)]
		[PXProcessButton()]
		public virtual IEnumerable CalculateFreight(PXAdapter adapter)
		{
			if (Document.Current != null && Document.Current.IsManualPackage != true && Document.Current.IsPackageValid != true)
			{
				CarrierRatesExt.RecalculatePackagesForOrder(Document.Current);
			}

			CalculateFreightCost(false);

			return adapter.Get();
		}

		public PXAction<SOOrder> validateAddresses;
		[PXUIField(DisplayName = CS.Messages.ValidateAddresses, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select, FieldClass = CS.Messages.ValidateAddress)]
		[PXButton(DisplayOnMainToolbar = false/*ImageKey = PX.Web.UI.Sprite.Main.Process*/)]
		public virtual IEnumerable ValidateAddresses(PXAdapter adapter)
		{
			foreach (SOOrder current in adapter.Get<SOOrder>())
			{
				if (current != null)
				{
					FindAllImplementations<IAddressValidationHelper>().ValidateAddresses();
				}
				yield return current;
			}
		}

		public PXAction<SOOrder> recalculateDiscountsAction;
		[PXUIField(DisplayName = "Recalculate Prices", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton(CommitChanges = true, DisplayOnMainToolbar = false)]
		public virtual IEnumerable RecalculateDiscountsAction(PXAdapter adapter)
		{
			if (adapter.MassProcess)
			{
				PXLongOperation.StartOperation(this, () => this.RecalculateDiscountsProc(false));
			}
			else if (adapter.ExternalCall == false || this.IsImport == true)
				{
					this.RecalculateDiscountsProc(true);
				}
			else if (recalcdiscountsfilter.AskExt() == WebDialogResult.OK)
				{
					SOOrderEntry clone = this.Clone();
					PXLongOperation.StartOperation(this, () => clone.RecalculateDiscountsProc(true));
				}
			return adapter.Get();
		}

		public PXAction<SOOrder> RecalculateDiscountsFromImport;
		[PXUIField(DisplayName = "Recalculate Discounts on Import", Visible = true)]
		[PXButton(DisplayOnMainToolbar = false)]
		public void recalculateDiscountsFromImport()
		{
			if (Document.Current != null)
			{
				try
				{
					Document.Current.DeferPriceDiscountRecalculation = false;
					_discountEngine.AutoRecalculatePricesAndDiscounts(Transactions.Cache, Transactions, null, DiscountDetails, Document.Current.CustomerLocationID, Document.Current.OrderDate, GetDefaultSODiscountCalculationOptions(Document.Current, true) | DiscountEngine.DiscountCalculationOptions.DisablePriceCalculation | DiscountEngine.DiscountCalculationOptions.CalculateDiscountsFromImport);
				}
				finally
				{
					Document.Current.DeferPriceDiscountRecalculation = soordertype.Current.DeferPriceDiscountRecalculation;
				}
				this.Save.Press();
			}
		}

		public PXAction<SOOrder> RecalculatePricesAndDiscountsFromImport;
		[PXUIField(DisplayName = "Recalculate Prices and Discounts on Import", Visible = true)]
		[PXButton(DisplayOnMainToolbar = false)]
		public void recalculatePricesAndDiscountsFromImport()
		{
			if (Document.Current != null)
			{
				try
				{
					Document.Current.DeferPriceDiscountRecalculation = false;
					_discountEngine.AutoRecalculatePricesAndDiscounts(Transactions.Cache, Transactions, null, DiscountDetails, Document.Current.CustomerLocationID, Document.Current.OrderDate, GetDefaultSODiscountCalculationOptions(Document.Current, true) | DiscountEngine.DiscountCalculationOptions.CalculateDiscountsFromImport);
				}
				finally
				{
					Document.Current.DeferPriceDiscountRecalculation = soordertype.Current.DeferPriceDiscountRecalculation; ;
				}
				Save.Press();
			}
		}

		protected virtual void RecalculateDiscountsProc(bool redirect)
		{
			_discountEngine.RecalculatePricesAndDiscounts(Transactions.Cache, Transactions, Transactions.Current, DiscountDetails, Document.Current.CustomerLocationID, Document.Current.OrderDate, recalcdiscountsfilter.Current, DiscountEngine.DefaultARDiscountCalculationParameters);
			if (redirect)
			{
				PXLongOperation.SetCustomInfo(this);
			}
			else
			{
				this.Save.Press();
			}
		}

		public PXAction<SOOrder> recalcOk;
		[PXUIField(DisplayName = "OK", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXLookupButton()]
		public virtual IEnumerable RecalcOk(PXAdapter adapter)
		{
			return adapter.Get();
		}

		public PXAction<SOOrder> viewBlanketOrder;
		[PXUIField(DisplayName = PO.Messages.ViewParentOrder, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select, Visible = false)]
		[PXLookupButton]
		public virtual IEnumerable ViewBlanketOrder(PXAdapter adapter)
		{
			PXRedirectHelper.TryRedirect(
				Document.Cache,
				SOOrder.PK.Find(this, Transactions.Current?.BlanketType, Transactions.Current?.BlanketNbr),
				PO.Messages.ViewParentOrder,
				PXRedirectHelper.WindowMode.NewWindow);

			return adapter.Get();
		}

		#endregion

		#region Entity Event Handlers
		public PXWorkflowEventHandler<SOOrder> OnOrderDeleted_ReopenQuote;
		public PXWorkflowEventHandler<SOOrder> OnShipmentCreationFailed;

		public PXWorkflowEventHandler<SOOrder> OnPaymentRequirementsSatisfied;
		public PXWorkflowEventHandler<SOOrder> OnPaymentRequirementsViolated;

		public PXWorkflowEventHandler<SOOrder> OnObtainedPaymentInPendingProcessing;
		public PXWorkflowEventHandler<SOOrder> OnLostLastPaymentInPendingProcessing;

		public PXWorkflowEventHandler<SOOrder> OnCreditLimitSatisfied;
		public PXWorkflowEventHandler<SOOrder> OnCreditLimitViolated;

		public PXWorkflowEventHandler<SOOrder> OnBlanketCompleted;
		public PXWorkflowEventHandler<SOOrder> OnBlanketReopened;

		public PXWorkflowEventHandler<SOOrder, SOOrderShipment, SOInvoice> OnInvoiceLinked;
		public PXWorkflowEventHandler<SOOrder, SOOrderShipment, SOInvoice> OnInvoiceUnlinked;

		public PXWorkflowEventHandler<SOOrder, SOOrderShipment, SOShipment> OnShipmentLinked;
		public PXWorkflowEventHandler<SOOrder, SOOrderShipment, SOShipment> OnShipmentUnlinked;

		public PXWorkflowEventHandler<SOOrder, SOInvoice> OnInvoiceReleased;
		public PXWorkflowEventHandler<SOOrder, SOInvoice> OnInvoiceCancelled;

		public PXWorkflowEventHandler<SOOrder> OnShipmentConfirmed;
		public PXWorkflowEventHandler<SOOrder> OnShipmentCorrected;

		#endregion

		#region SiteStatus Lookup
		public PXFilter<SOSiteStatusFilter> sitestatusfilter;
		[PXFilterable]
		[PXCopyPasteHiddenView]
		public SOSiteStatusLookup<SOSiteStatusSelected, SOSiteStatusFilter> sitestatus;

		public PXAction<SOOrder> addInvBySite;
		[PXUIField(DisplayName = "Add Items", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXLookupButton]
		public virtual IEnumerable AddInvBySite(PXAdapter adapter)
		{
			if (sitestatus.AskExt((PXGraph g, string viewName) => sitestatusfilter.Cache.Clear()) == WebDialogResult.OK)
			{
				return AddInvSelBySite(adapter);
			}
			sitestatusfilter.Cache.Clear();
			sitestatus.Cache.Clear();
			return adapter.Get();
		}

		public PXAction<SOOrder> addInvSelBySite;
		[PXUIField(DisplayName = "Add", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXLookupButton]
		public virtual IEnumerable AddInvSelBySite(PXAdapter adapter)
		{
			Transactions.Cache.ForceExceptionHandling = true;

			foreach (SOSiteStatusSelected line in sitestatus.Cache.Cached)
			{
				if (line.Selected == true && line.QtySelected > 0)
				{
					SOLine newline = PXCache<SOLine>.CreateCopy(Transactions.Insert(new SOLine()));
					newline.SiteID = line.SiteID ?? newline.SiteID;
					newline.InventoryID = line.InventoryID;
					if (line.SubItemID != null) // line.SubItemID is null when the line doesn't have INSiteStatusByCostCenter
						newline.SubItemID = line.SubItemID;
					
					newline.UOM = line.SalesUnit;
					newline.AlternateID = line.AlternateID;
					if (sitestatusfilter.Current?.CustomerLocationID != null)
						newline.CustomerLocationID = sitestatusfilter.Current.CustomerLocationID;
                    newline = PXCache<SOLine>.CreateCopy(Transactions.Update(newline));
                    if (newline.RequireLocation != true)
					{
						newline.LocationID = null;
						newline = PXCache<SOLine>.CreateCopy(Transactions.Update(newline));
					}
					newline.Qty = line.QtySelected;
					Transactions.Update(newline);
                }
			}
			sitestatus.Cache.Clear();
			return adapter.Get();
		}

		protected virtual void InvoiceSplits_InventoryID_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			e.Cancel = true;
		}

		protected virtual void InvoiceSplits_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
		{
			InvoiceSplits row = (InvoiceSplits)e.Row;
			if (row != null)
			{
				InventoryItem item = InventoryItem.PK.Find(this, row.InventoryID);
				if(item.ItemStatus.IsIn(INItemStatus.Inactive, INItemStatus.ToDelete))
					PXUIFieldAttribute.SetEnabled<InvoiceSplits.selected>(cache, row, false);
			}
		}

		protected virtual void SOSiteStatusFilter_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
		{
			SOSiteStatusFilter row = (SOSiteStatusFilter)e.Row;
			if (row != null && !PXAccess.FeatureInstalled<FeaturesSet.inventory>())
				row.OnlyAvailable = false;
		}
		protected virtual void SOSiteStatusFilter_RowInserted(PXCache cache, PXRowInsertedEventArgs e)
		{
			SOSiteStatusFilter row = (SOSiteStatusFilter)e.Row;
			if (row != null && Document.Current != null)
			{
				row.SiteID = Document.Current.DefaultSiteID;
				row.Behavior = Document.Current.Behavior;
				row.CustomerLocationID = Document.Current.CustomerLocationID;
			}
		}
		#endregion

		#region CurrencyInfo events
		protected virtual void CurrencyInfo_CuryID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (PXAccess.FeatureInstalled<FeaturesSet.multicurrency>())
			{
				if (e.Row != null && IsCopyOrder)
				{
					e.NewValue = ((CurrencyInfo)e.Row).CuryID ?? customer?.Current?.CuryID;
					e.Cancel = true;
				}
				else
				{
					if (customer.Current != null && !string.IsNullOrEmpty(customer.Current.CuryID))
					{
						e.NewValue = customer.Current.CuryID;
						e.Cancel = true;
					}
				}


			}
		}

		protected virtual void CurrencyInfo_CuryRateTypeID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (PXAccess.FeatureInstalled<FeaturesSet.multicurrency>())
			{
				if (e.Row != null && IsCopyOrder)
				{
					e.NewValue = ((CurrencyInfo)e.Row).CuryRateTypeID;
					e.Cancel = true;
				}
				else
				{
					if (customer.Current != null && !string.IsNullOrEmpty(customer.Current.CuryRateTypeID))
					{
						e.NewValue = customer.Current.CuryRateTypeID;
						e.Cancel = true;
					}
					else
					{
						CMSetup cmsetup = PXSelect<CMSetup>.Select(this);
						if (cmsetup != null)
						{
							e.NewValue = cmsetup.ARRateTypeDflt;
							e.Cancel = true;
						}
					}
				}

			}
		}

		protected virtual void CurrencyInfo_CuryEffDate_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (Document.Cache.Current != null)
			{
				e.NewValue = ((SOOrder)Document.Cache.Current).OrderDate;
				e.Cancel = true;
			}
		}

		protected virtual void CurrencyInfo_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			CurrencyInfo info = e.Row as CurrencyInfo;
			if (info != null)
			{
				bool curyenabled = info.AllowUpdate(this.Transactions.Cache);

				if (customer.Current != null && !(bool)customer.Current.AllowOverrideRate)
				{
					curyenabled = false;
				}

				PXUIFieldAttribute.SetEnabled<CurrencyInfo.curyRateTypeID>(sender, info, curyenabled);
				PXUIFieldAttribute.SetEnabled<CurrencyInfo.curyEffDate>(sender, info, curyenabled);
				PXUIFieldAttribute.SetEnabled<CurrencyInfo.sampleCuryRate>(sender, info, curyenabled);
				PXUIFieldAttribute.SetEnabled<CurrencyInfo.sampleRecipRate>(sender, info, curyenabled);
			}
		}
		#endregion

		public override void InitCacheMapping(Dictionary<Type, Type> map)
		{
			base.InitCacheMapping(map);

			this.Caches.AddCacheMapping(typeof(INSiteStatusByCostCenter), typeof(INSiteStatusByCostCenter));
			this.Caches.AddCacheMapping(typeof(SiteStatusByCostCenter), typeof(SiteStatusByCostCenter));
			this.Caches.AddCacheMapping(typeof(CustomerPaymentMethod), typeof(CustomerPaymentMethod));
		}

		protected virtual void ParentFieldUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			if (!sender.ObjectsEqual<SOOrder.orderDate, SOOrder.curyID>(e.Row, e.OldRow))
			{
				foreach (SOLine tran in Transactions.Select())
				{
					Transactions.Cache.MarkUpdated(tran, assertError: true);
				}
			}
		}

		public SOOrderEntry()
		{
			RowUpdated.AddHandler<SOOrder>(ParentFieldUpdated);

			shipmentlist.Cache.AllowUpdate = false; // Needs to save notes (see PXNoteAttribute.NoteTextGenericFieldUpdating: "!sender.AllowUpdate")

			PXUIFieldAttribute.SetVisible<SOOrderShipment.operation>(shipmentlist.Cache, null, false);
			PXUIFieldAttribute.SetVisible<SOOrderShipment.orderType>(shipmentlist.Cache, null, false);
			PXUIFieldAttribute.SetVisible<SOOrderShipment.orderNbr>(shipmentlist.Cache, null, false);
			PXUIFieldAttribute.SetVisible<SOOrderShipment.shipmentNbr>(shipmentlist.Cache, null, false);

			{
				SOSetup record = sosetup.Current;
			}

			ARSetupNoMigrationMode.EnsureMigrationModeDisabled(this);

			PXFieldState state = (PXFieldState)this.Transactions.Cache.GetStateExt<SOLine.inventoryID>(null);
			viewInventoryID = state != null ? state.ViewName : null;

			PXUIFieldAttribute.SetVisible<SOLine.taskID>(Transactions.Cache, null, PM.ProjectAttribute.IsPMVisible(BatchModule.SO));

			FieldDefaulting.AddHandler<BAccountR.type>((sender, e) => { if (e.Row != null) e.NewValue = BAccountType.CustomerType; });
			FieldDefaulting.AddHandler<InventoryItem.stkItem>((sender, e) => { if (e.Row != null && InventoryHelper.CanCreateStockItem(sender.Graph) == false) e.NewValue = false; });
			PXUIFieldAttribute.SetEnabled(invoicesplits.Cache, null, false);
			PXUIFieldAttribute.SetEnabled<InvoiceSplits.selected>(invoicesplits.Cache, null, true);

			if (!PXAccess.FeatureInstalled<FeaturesSet.carrierIntegration>())
			{
				CarrierRatesExt.shopRates.SetCaption(PXMessages.LocalizeNoPrefix(Messages.Packages));
			}
			CurrencyView.SetCommitChanges(true);
		}


		[InjectDependency]
		protected ILicenseLimitsService _licenseLimits { get; set; }

		void IGraphWithInitialization.Initialize()
		{
			if (_licenseLimits != null)
			{
				OnBeforeCommit += _licenseLimits.GetCheckerDelegate<SOOrder>(
					new TableQuery(TransactionTypes.LinesPerMasterRecord, typeof(SOLine), (graph) =>
					{
						return new PXDataFieldValue[]
						{
								new PXDataFieldValue<SOLine.orderType>(PXDbType.Char, ((SOOrderEntry)graph).Document.Current?.OrderType),
								new PXDataFieldValue<SOLine.orderNbr>(((SOOrderEntry)graph).Document.Current?.OrderNbr)
						};
					}),
					new TableQuery(TransactionTypes.SerialsPerDocument, typeof(SOLineSplit), (graph) =>
					{
						SOOrder order = ((SOOrderEntry)graph).Document.Current;
						bool lotSerialInput = order?.Behavior.IsIn(SOBehavior.CM, SOBehavior.IN, SOBehavior.MO) == true;
						return new PXDataFieldValue[]
						{
							new PXDataFieldValue<SOLineSplit.orderType>(PXDbType.Char, lotSerialInput ? order?.OrderType : null),
							new PXDataFieldValue<SOLineSplit.orderNbr>(lotSerialInput ? order?.OrderNbr : null)
						};
					}));
			}
		}

		#region SOAdjust Events

		#region SOAdjust Cache Attached

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXRemoveBaseAttribute(typeof(PXDefaultAttribute))]
		[PXDBDefault(typeof(SOOrder.customerID))]
		[PXUIField(DisplayName = "Customer ID", Visibility = PXUIVisibility.Visible, Visible = false)]
		protected virtual void _(Events.CacheAttached<SOAdjust.customerID> eventArgs)
		{
		}

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXRemoveBaseAttribute(typeof(PXDefaultAttribute))]
		[PXRemoveBaseAttribute(typeof(PXSelectorAttribute))]
		[PXDBDefault(typeof(SOOrder.orderType))]
		protected virtual void _(Events.CacheAttached<SOAdjust.adjdOrderType> eventArgs)
		{
		}

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXRemoveBaseAttribute(typeof(PXDefaultAttribute))]
		[PXRemoveBaseAttribute(typeof(PXSelectorAttribute))]
		[PXRemoveBaseAttribute(typeof(PXRestrictorAttribute))]
		[PXDBDefault(typeof(SOOrder.orderNbr))]
		protected virtual void _(Events.CacheAttached<SOAdjust.adjdOrderNbr> eventArgs)
		{
		}

		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXDefault(typeof(Switch<Case<Where<Current<SOOrderType.canHaveRefunds>, Equal<True>>,
			ARDocType.refund>,
			ARDocType.payment>))]
		[ARPaymentType.SOList]
		[PXUIField(DisplayName = "Doc. Type")]
		protected virtual void _(Events.CacheAttached<SOAdjust.adjgDocType> eventArgs)
		{
		}

		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXRemoveBaseAttribute(typeof(PXDBDefaultAttribute))]
		[PXDBString(SOAdjust.AdjgRefNbrLength, IsUnicode = true, IsKey = true, InputMask = ">CCCCCCCCCCCCCCC")]
		[PXDefault]
		[PXUIField(DisplayName = "Reference Nbr.")]
		[ARPaymentType.AdjgRefNbr(typeof(Search<ARPayment.refNbr,
			Where<ARPayment.customerID, In3<Current<SOOrder.customerID>, Current<Customer.consolidatingBAccountID>>,
			And<ARPayment.docType, Equal<Optional<SOAdjust.adjgDocType>>,
			And<ARPayment.openDoc, Equal<True>>>>>), Filterable = true)]
		protected virtual void _(Events.CacheAttached<SOAdjust.adjgRefNbr> eventArgs)
		{
		}

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXRemoveBaseAttribute(typeof(PXDBDecimalAttribute))]
		[PXDBCurrency(typeof(SOAdjust.adjdCuryInfoID), typeof(SOAdjust.adjAmt))]
		[PXUIField(DisplayName = "Applied To Order")]
		protected virtual void _(Events.CacheAttached<SOAdjust.curyAdjdAmt> eventArgs)
		{
		}

		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXRemoveBaseAttribute(typeof(CM.Extensions.PXDBCurrencyAttribute))]
		[PXRemoveBaseAttribute(typeof(PXUIFieldAttribute))]
		[PXDBDecimal(4)]
		[PXFormula(typeof(Maximum<Sub<SOAdjust.curyOrigAdjgAmt, SOAdjust.curyAdjgBilledAmt>, decimal0>))]
		protected virtual void _(Events.CacheAttached<SOAdjust.curyAdjgAmt> eventArgs)
		{
		}

		//Probably, original attributes should be restored when SOOrder will be switched to new CM flow
		//[PXMergeAttributes(Method = MergeMethod.Merge)]
		//[PXRemoveBaseAttribute(typeof(PXDefaultAttribute))]
		[PXMergeAttributes(Method = MergeMethod.Replace)] //SOAdjust now have CM.Extended attributess which have to be replaced
		[PXDBLong]
		[CurrencyInfo(typeof(SOOrder.curyInfoID), ModuleCode = BatchModule.SO, CuryIDField = "AdjdOrigCuryID")]
		protected virtual void _(Events.CacheAttached<SOAdjust.adjdOrigCuryInfoID> eventArgs)
		{
		}

		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXRemoveBaseAttribute(typeof(CM.Extensions.CurrencyInfo))]
		[CurrencyInfo(ModuleCode = BatchModule.SO, CuryIDField = "AdjgCuryID")]
		[PXDefault]
		protected virtual void _(Events.CacheAttached<SOAdjust.adjgCuryInfoID> eventArgs)
		{
		}

		//Probably, original attributes should be restored when SOOrder will be switched to new CM flow
		//[PXMergeAttributes(Method = MergeMethod.Merge)]
		//[PXRemoveBaseAttribute(typeof(PXDefaultAttribute))]
		[PXMergeAttributes(Method = MergeMethod.Replace)] //SOAdjust now have CM.Extended attributess which have to be replaced
		[PXDBLong]
		[CurrencyInfo(typeof(SOOrder.curyInfoID), ModuleCode = BatchModule.SO, CuryIDField = "AdjdCuryID")]
		protected virtual void _(Events.CacheAttached<SOAdjust.adjdCuryInfoID> eventArgs)
		{
		}

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXRemoveBaseAttribute(typeof(PXDefaultAttribute))]
		[PXDBDefault(typeof(SOOrder.orderDate))]
		protected virtual void _(Events.CacheAttached<SOAdjust.adjgDocDate> eventArgs)
		{
		}

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXRemoveBaseAttribute(typeof(PXDefaultAttribute))]
		[PXDBDefault(typeof(SOOrder.orderDate))]
		protected virtual void _(Events.CacheAttached<SOAdjust.adjdOrderDate> eventArgs)
		{
		}

		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[DenormalizedFrom(
			new[] { typeof(ARPayment.isCCPayment), typeof(ARPayment.released), typeof(ARPayment.isCCAuthorized), typeof(ARPayment.isCCCaptured), typeof(ARPayment.voided),
					typeof(ARPayment.hold), typeof(ARPayment.adjDate), typeof(ARPayment.paymentMethodID), typeof(ARPayment.cashAccountID), typeof(ARPayment.pMInstanceID),
					typeof(ARPayment.processingCenterID), typeof(ARPayment.extRefNbr), typeof(ARPayment.docDesc), typeof(ARPayment.curyOrigDocAmt), typeof(ARPayment.origDocAmt),
					typeof(ARPayment.syncLock), typeof(ARPayment.syncLockReason) },
			new[] { typeof(SOAdjust.isCCPayment), typeof(SOAdjust.paymentReleased), typeof(SOAdjust.isCCAuthorized), typeof(SOAdjust.isCCCaptured), typeof(SOAdjust.voided),
					typeof(SOAdjust.hold), typeof(SOAdjust.adjgDocDate), typeof(SOAdjust.paymentMethodID), typeof(SOAdjust.cashAccountID), typeof(SOAdjust.pMInstanceID),
					typeof(SOAdjust.processingCenterID), typeof(SOAdjust.extRefNbr), typeof(SOAdjust.docDesc), typeof(SOAdjust.curyOrigDocAmt), typeof(SOAdjust.origDocAmt),
					typeof(SOAdjust.syncLock), typeof(SOAdjust.syncLockReason) },
			childToParentLinkField: typeof(SOAdjust.adjgRefNbr))]
		protected virtual void _(Events.CacheAttached<SOAdjust.isCCPayment> eventArgs)
		{
		}

		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[CashAccount(typeof(SOOrder.branchID), typeof(Search<CashAccount.cashAccountID>), Visibility = PXUIVisibility.Visible)]
		protected virtual void _(Events.CacheAttached<SOAdjust.cashAccountID> eventArgs)
		{
		}

		#endregion

		protected virtual void SOAdjust_AdjgRefNbr_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			CurrencyInfo inv_info = PXSelect<CurrencyInfo, Where<CurrencyInfo.curyInfoID, Equal<Current<SOOrder.curyInfoID>>>>.Select(this);
			SOAdjust adj = (SOAdjust)e.Row;

			PXSelectBase<ARPayment> s = new PXSelectReadonly2<ARPayment,
				InnerJoin<CurrencyInfo, On<CurrencyInfo.curyInfoID, Equal<ARPayment.curyInfoID>>>,
				Where<ARPayment.customerID, In3<Current<SOOrder.customerID>, Current<Customer.consolidatingBAccountID>>,
					And<ARPayment.docType, In3<ARDocType.payment, ARDocType.prepayment, ARDocType.creditMemo, ARDocType.refund>,
					And<ARPayment.openDoc, Equal<boolTrue>,
					And<ARPayment.docType, Equal<Required<ARPayment.docType>>,
					And<ARPayment.refNbr, Equal<Required<ARPayment.refNbr>>>>>>>>(this);

			foreach (PXResult<ARPayment, CurrencyInfo> res in s.Select(adj.AdjgDocType, adj.AdjgRefNbr))
			{
				ARPayment payment = PXCache<ARPayment>.CreateCopy(res);

				CurrencyInfo pay_info = (CurrencyInfo)res;

				adj.CustomerID = Document.Current.CustomerID;
				adj.AdjdOrderType = Document.Current.OrderType;
				adj.AdjdOrderNbr = Document.Current.OrderNbr;
				adj.AdjgDocType = payment.DocType;
				adj.AdjgRefNbr = payment.RefNbr;

				SOAdjust other = PXSelectGroupBy<SOAdjust, Where<SOAdjust.adjgDocType, Equal<Required<SOAdjust.adjgDocType>>, And<SOAdjust.adjgRefNbr, Equal<Required<SOAdjust.adjgRefNbr>>, And<Where<SOAdjust.adjdOrderType, NotEqual<Required<SOAdjust.adjdOrderType>>, Or<SOAdjust.adjdOrderNbr, NotEqual<Required<SOAdjust.adjdOrderNbr>>>>>>>, Aggregate<GroupBy<SOAdjust.adjgDocType, GroupBy<SOAdjust.adjgRefNbr, Sum<SOAdjust.curyAdjgAmt, Sum<SOAdjust.adjAmt>>>>>>.Select(this, adj.AdjgDocType, adj.AdjgRefNbr, adj.AdjdOrderType, adj.AdjdOrderNbr);
				if (other != null && other.AdjdOrderNbr != null)
				{
					payment.CuryDocBal -= other.CuryAdjgAmt;
					payment.DocBal -= other.AdjAmt;
				}

				ARAdjust fromar = PXSelectGroupBy<ARAdjust, Where<ARAdjust.adjgDocType, Equal<Required<ARAdjust.adjgDocType>>, And<ARAdjust.adjgRefNbr, Equal<Required<ARAdjust.adjgRefNbr>>, And<ARAdjust.released, Equal<boolFalse>>>>, Aggregate<GroupBy<ARAdjust.adjgDocType, GroupBy<ARAdjust.adjgRefNbr, Sum<ARAdjust.curyAdjgAmt, Sum<ARAdjust.adjAmt>>>>>>.Select(this, adj.AdjgDocType, adj.AdjgRefNbr);
				if (fromar != null && fromar.AdjdRefNbr != null)
				{
					payment.CuryDocBal -= fromar.CuryAdjgAmt;
					payment.DocBal -= fromar.AdjAmt;
				}

				if (Adjustments.Cache.Locate(adj) == null)
				{
					adj.AdjgCuryInfoID = payment.CuryInfoID;
					adj.AdjdOrigCuryInfoID = Document.Current.CuryInfoID;
					//if LE constraint is removed from payment selection this must be reconsidered
					adj.AdjdCuryInfoID = Document.Current.CuryInfoID;

					decimal CuryDocBal;
					if (string.Equals(pay_info.CuryID, inv_info.CuryID))
					{
						CuryDocBal = (decimal)payment.CuryDocBal;
					}
					else
					{
						decimal docBal = ((payment.Released == true) ? payment.DocBal : payment.OrigDocAmt) ?? 0m;

						PXDBCurrencyAttribute.CuryConvCury(Adjustments.Cache, inv_info, docBal, out CuryDocBal);
					}
					adj.CuryDocBal = CuryDocBal;
				}
			}
		}

		protected virtual void SOAdjust_Hold_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			e.NewValue = true;
			e.Cancel = true;
		}

		protected virtual void SOAdjust_CuryAdjdAmt_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			SOAdjust adj = (SOAdjust)e.Row;

			if ((decimal)e.NewValue < 0m)
			{
				throw new PXSetPropertyException(CS.Messages.Entry_GE, ((int)0).ToString());
			}

			Terms terms = PXSelect<Terms, Where<Terms.termsID, Equal<Current<SOOrder.termsID>>>>.Select(this);

			if (terms != null && terms.InstallmentType != TermsInstallmentType.Single && (decimal)e.NewValue > 0m)
			{
				throw new PXSetPropertyException(AR.Messages.PrepaymentAppliedToMultiplyInstallments);
			}

			if (adj.AdjgCuryInfoID == null || adj.CuryDocBal == null)
			{
				PXResult<ARPayment, CurrencyInfo> res = (PXResult<ARPayment, CurrencyInfo>)PXSelectJoin<ARPayment, InnerJoin<CurrencyInfo, On<CurrencyInfo.curyInfoID, Equal<ARPayment.curyInfoID>>>, Where<ARPayment.docType, Equal<Required<ARPayment.docType>>, And<ARPayment.refNbr, Equal<Required<ARPayment.refNbr>>>>>.Select(this, adj.AdjgDocType, adj.AdjgRefNbr);

				ARPayment payment = PXCache<ARPayment>.CreateCopy(res);

				if (payment == null && sender.Graph.IsContractBasedAPI)
				{
					return;
				}

				CurrencyInfo pay_info = (CurrencyInfo)res;
				CurrencyInfo inv_info = PXSelect<CurrencyInfo, Where<CurrencyInfo.curyInfoID, Equal<Current<SOOrder.curyInfoID>>>>.Select(this);

				SOAdjust other = PXSelectGroupBy<SOAdjust, Where<SOAdjust.adjgDocType, Equal<Required<SOAdjust.adjgDocType>>, And<SOAdjust.adjgRefNbr, Equal<Required<SOAdjust.adjgRefNbr>>, And<Where<SOAdjust.adjdOrderType, NotEqual<Required<SOAdjust.adjdOrderType>>, Or<SOAdjust.adjdOrderNbr, NotEqual<Required<SOAdjust.adjdOrderNbr>>>>>>>, Aggregate<GroupBy<SOAdjust.adjgDocType, GroupBy<SOAdjust.adjgRefNbr, Sum<SOAdjust.curyAdjgAmt, Sum<SOAdjust.adjAmt>>>>>>.Select(this, adj.AdjgDocType, adj.AdjgRefNbr, adj.AdjdOrderType, adj.AdjdOrderNbr);
				if (other != null && other.AdjdOrderNbr != null)
				{
					payment.CuryDocBal -= other.CuryAdjgAmt;
					payment.DocBal -= other.AdjAmt;
				}

				ARAdjust fromar = PXSelectGroupBy<ARAdjust, Where<ARAdjust.adjgDocType, Equal<Required<ARAdjust.adjgDocType>>, And<ARAdjust.adjgRefNbr, Equal<Required<ARAdjust.adjgRefNbr>>, And<ARAdjust.released, Equal<boolFalse>>>>, Aggregate<GroupBy<ARAdjust.adjgDocType, GroupBy<ARAdjust.adjgRefNbr, Sum<ARAdjust.curyAdjgAmt, Sum<ARAdjust.adjAmt>>>>>>.Select(this, adj.AdjgDocType, adj.AdjgRefNbr);
				if (fromar != null && fromar.AdjdRefNbr != null)
				{
					payment.CuryDocBal -= fromar.CuryAdjgAmt;
					payment.DocBal -= fromar.AdjAmt;
				}

				decimal CuryDocBal;
				if (string.Equals(pay_info.CuryID, inv_info.CuryID))
				{
					CuryDocBal = (decimal)payment.CuryDocBal;
				}
				else
				{
					decimal docBal = ((payment.Released == true) ? payment.DocBal : payment.OrigDocAmt) ?? 0m;

					PXDBCurrencyAttribute.CuryConvCury(sender, inv_info, docBal, out CuryDocBal);
				}

				adj.CuryDocBal = CuryDocBal - adj.CuryAdjdAmt;
				adj.AdjgCuryInfoID = payment.CuryInfoID;
			}

			if (adj.AdjdCuryInfoID == null || adj.AdjdOrigCuryInfoID == null)
			{
				adj.AdjdCuryInfoID = Document.Current.CuryInfoID;
				adj.AdjdOrigCuryInfoID = Document.Current.CuryInfoID;
			}

			decimal newBalance = (decimal)adj.CuryDocBal + (decimal)adj.CuryAdjdAmt - (decimal)e.NewValue;
			if (newBalance < 0)
			{
				throw new PXSetPropertyException(AR.Messages.Entry_LE, ((decimal)adj.CuryDocBal + (decimal)adj.CuryAdjdAmt).ToString());
			}
		}

		protected virtual void SOAdjust_CuryAdjdAmt_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			SOAdjust adj = (SOAdjust)e.Row;
			decimal CuryAdjgAmt;
			decimal AdjdAmt;
			decimal AdjgAmt;

			PXDBCurrencyAttribute.CuryConvBase<SOAdjust.adjdCuryInfoID>(sender, e.Row, (decimal)adj.CuryAdjdAmt, out AdjdAmt);

			CurrencyInfo pay_info = PXSelect<CurrencyInfo, Where<CurrencyInfo.curyInfoID, Equal<Current<SOAdjust.adjgCuryInfoID>>>>.SelectSingleBound(this, new object[] { adj });
			CurrencyInfo inv_info = PXSelect<CurrencyInfo, Where<CurrencyInfo.curyInfoID, Equal<Current<SOAdjust.adjdCuryInfoID>>>>.SelectSingleBound(this, new object[] { adj });

			if (pay_info == null && sender.Graph.IsContractBasedAPI)
			{
				return;
			}

			if (string.Equals(pay_info.CuryID, inv_info.CuryID))
			{
				CuryAdjgAmt = (decimal)adj.CuryAdjdAmt;
			}
			else
			{
				PXDBCurrencyAttribute.CuryConvCury<SOAdjust.adjgCuryInfoID>(sender, e.Row, AdjdAmt, out CuryAdjgAmt);
			}

			if (object.Equals(pay_info.CuryID, inv_info.CuryID) && object.Equals(pay_info.CuryRate, inv_info.CuryRate) && object.Equals(pay_info.CuryMultDiv, inv_info.CuryMultDiv))
			{
				AdjgAmt = AdjdAmt;
			}
			else
			{
				PXDBCurrencyAttribute.CuryConvBase<SOAdjust.adjgCuryInfoID>(sender, e.Row, CuryAdjgAmt, out AdjgAmt);
			}

			adj.CuryAdjgAmt = CuryAdjgAmt;
			adj.AdjAmt = AdjdAmt;
			adj.RGOLAmt = AdjgAmt - AdjdAmt;
			adj.CuryDocBal = adj.CuryDocBal + (decimal?)e.OldValue - adj.CuryAdjdAmt;
		}

		protected virtual void SOAdjust_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			SOAdjust row = e.Row as SOAdjust;
			if (row == null) return;

			PXUIFieldAttribute.SetEnabled<SOAdjust.adjgDocType>(sender, row, row.AdjgRefNbr == null);

			bool allowEditFromAPI = sender.Graph.IsContractBasedAPI && this.Document.Current != null && this.Document.Current.Completed != true && this.Document.Current.Cancelled != true;
			PXUIFieldAttribute.SetEnabled<SOAdjust.extRefNbr>(sender, row, allowEditFromAPI);
			PXUIFieldAttribute.SetEnabled<SOAdjust.paymentMethodID>(sender, row, allowEditFromAPI);
			PXUIFieldAttribute.SetEnabled<SOAdjust.pMInstanceID>(sender, row, allowEditFromAPI);
			PXUIFieldAttribute.SetEnabled<SOAdjust.processingCenterID>(sender, row, allowEditFromAPI);
			PXUIFieldAttribute.SetEnabled<SOAdjust.cashAccountID>(sender, row, allowEditFromAPI);
			PXUIFieldAttribute.SetEnabled<SOAdjust.curyOrigDocAmt>(sender, row, allowEditFromAPI);
			PXUIFieldAttribute.SetEnabled<SOAdjust.docDesc>(sender, row, allowEditFromAPI);
			PXUIFieldAttribute.SetEnabled<SOAdjust.hold>(sender, row, allowEditFromAPI);
			PXUIFieldAttribute.SetEnabled<SOAdjust.adjgDocDate>(sender, row, allowEditFromAPI);
		}

		#endregion

		#region SOShippingAddress Cache Attached

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXCustomizeBaseAttribute(typeof(PXUIFieldAttribute), nameof(PXUIFieldAttribute.Visible), true)]
		public virtual void _(Events.CacheAttached<SOShippingAddress.latitude> e) { }

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXCustomizeBaseAttribute(typeof(PXUIFieldAttribute), nameof(PXUIFieldAttribute.Visible), true)]
		public virtual void _(Events.CacheAttached<SOShippingAddress.longitude> e) { }

		#endregion

		#region SOOrderShipment Events
		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXRemoveBaseAttribute(typeof(PXDBGuidAttribute))]
		[CopiedShipmentNoteID(IsKey = true)]
		protected virtual void SOOrderShipment_ShippingRefNoteID_CacheAttached(PXCache sender)
		{
		}

		[PXDBGuid]
		protected virtual void _(Events.CacheAttached<SOOrderShipment.orderNoteID> args)
		{
		}

		protected virtual void SOOrderShipment_ShipmentNbr_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			e.Cancel = true;
		}

		protected virtual void SOOrderShipment_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			PXUIFieldAttribute.SetEnabled(sender, e.Row, false);
			PXUIFieldAttribute.SetEnabled<SOOrderShipment.selected>(sender, e.Row, true);
		}
		#endregion

		#region SOOrder Cache Attached

		[PXFormula(typeof(Selector<SOOrder.orderType, SOOrderType.deferPriceDiscountRecalculation>))]
		[PXMergeAttributes(Method = MergeMethod.Append)]
		public virtual void SOOrder_DeferPriceDiscountRecalculation_CacheAttached(PXCache sender)
		{
		}

		#endregion

		#region SOOrder Events

		protected virtual void SOOrder_Cancelled_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			SOOrder order = (SOOrder)e.Row;
			if (e.Row == null || (bool?)e.NewValue != true)
				return;

			SOOrderShipment orderShipment = PXSelectReadonly<SOOrderShipment,
				Where<SOOrderShipment.orderType, Equal<Current<SOOrder.orderType>>,
					And<SOOrderShipment.orderNbr, Equal<Current<SOOrder.orderNbr>>>>>
				.SelectSingleBound(this, new object[] { order });
			if (orderShipment != null)
			{
				var exception = orderShipment.Confirmed == true ? new PXException(Messages.OrderHasShipmentAndCantBeCancelled, order.OrderNbr)
					: new PXException(Messages.OrderCantBeCancelled, order.OrderNbr, order.OrderType, orderShipment.ShipmentNbr);
				throw exception;
			}
		}

		protected virtual void SOOrder_TaxZoneID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			SOOrder row = e.Row as SOOrder;
			e.NewValue = GetDefaultTaxZone(row);
		}

		public virtual string GetDefaultTaxZone(SOOrder row)
		{
			string result = null;
			if (row != null)
			{
				//Do not redefault if value exists and overide flag is ON:
				if (!string.IsNullOrEmpty(row.TaxZoneID) && row.OverrideTaxZone == true)
				{
					result = row.TaxZoneID;
				}
				else
				{
					Location customerLocation = location.Select();
					result = GetDefaultTaxZone(customerLocation, true, row.ShipVia, row.BranchID);
				}				
			}

			return result;
		}

		public virtual string GetDefaultTaxZone(Location customerLocation, bool useOrderAddress, string shipVia, int? branchID)
		{
			if (!string.IsNullOrEmpty(customerLocation?.CTaxZoneID))
			{
				TaxZone taxZone = PXSelect<TaxZone,
					Where<TaxZone.taxZoneID, Equal<Required<TaxZone.taxZoneID>>>>
					.Select(this, customerLocation.CTaxZoneID);
				if (taxZone != null)
				{
					return customerLocation.CTaxZoneID;
				}
			}

			if (IsCommonCarrier(shipVia))
			{
				IAddressBase address;
				if (useOrderAddress)
				{
					address = (SOShippingAddress)Shipping_Address.Select();
				}
				else
				{
					address = (Address)PXSelect<Address,
						Where<Address.bAccountID, Equal<Current<Location.bAccountID>>,
							And<Address.addressID, Equal<Current<Location.defAddressID>>>>>
						.SelectSingleBound(this, new[] { customerLocation });
				}
				if (address != null )
					return TaxBuilderEngine.GetTaxZoneByAddress(this, address);
			}
			else
			{
				BAccount companyAccount = PXSelectJoin<BAccountR,
					InnerJoin<Branch, On<Branch.bAccountID, Equal<BAccountR.bAccountID>>>,
					Where<Branch.branchID, Equal<Required<Branch.branchID>>>>
					.Select(this, branchID);
				if (companyAccount != null)
				{
					Location companyLocation = PXSelect<Location,
						Where<Location.bAccountID, Equal<Required<Location.bAccountID>>,
							And<Location.locationID, Equal<Required<Location.locationID>>>>>
						.Select(this, companyAccount.BAccountID, companyAccount.DefLocationID);
					if (companyLocation != null)
						return companyLocation.VTaxZoneID;
				}
			}
			return null;
		}

		protected virtual void SOOrder_BranchID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			sender.SetDefaultExt<SOOrder.taxZoneID>(e.Row);
		}

		protected virtual void SOOrder_CreditHold_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			SOOrder row = e.Row as SOOrder;
			if (row != null && PXAccess.FeatureInstalled<FeaturesSet.approvalWorkflow>())
			{
				SOSetupApproval setupApproval = SetupApproval.Select();
				if (setupApproval != null && (setupApproval.IsActive ?? false))
				{
					sender.RaiseFieldUpdated<SOOrder.hold>(row, true);
				}
			}
		}

		protected virtual bool IsCommonCarrier(string carrierID)
		{
			if (string.IsNullOrEmpty(carrierID))
			{
				return false; //pickup;
			}
			else
			{
				Carrier carrier = Carrier.PK.Find(this, carrierID);
				if (carrier == null)
				{
					return false;
				}
				else
				{
					return carrier.IsCommonCarrier == true;
				}
			}
		}

		protected virtual void SOOrder_OrderType_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (orderType != null)
			{
				e.NewValue = orderType;
				e.Cancel = true;
			}
		}

		protected virtual void SOOrder_DestinationSiteID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			var order = e.Row as SOOrder;

			if (e.Row == null || order?.IsTransferOrder != true)
			{
				e.NewValue = null;
				e.Cancel = true;
			}
		}

		protected virtual void SOOrder_DestinationSiteID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			var order = e.Row as SOOrder;

			Company.RaiseFieldUpdated(sender, e.Row);
			Branch company = null;
			string destinationSiteIdErrorMessage = string.Empty;

			using (new PXReadBranchRestrictedScope())
			{
				company = Company.Select();

			if (order?.IsTransferOrder == true && company != null)
			{
				sender.SetValueExt<SOOrder.customerID>(e.Row, company.BranchCD);
			}

			try
			{
				SOShippingAddressAttribute.DefaultRecord<SOOrder.shipAddressID>(sender, e.Row);
			}
			catch (SharedRecordMissingException)
			{
				var missingaddressexception = new PXSetPropertyException(Messages.DestinationSiteAddressMayNotBeEmpty, PXErrorLevel.Error);
				if (UnattendedMode)
					throw missingaddressexception;
				else
					sender.RaiseExceptionHandling<SOOrder.destinationSiteID>(e.Row, sender.GetValueExt<SOOrder.destinationSiteID>(e.Row), missingaddressexception);

				destinationSiteIdErrorMessage = Messages.DestinationSiteAddressMayNotBeEmpty;
				sender.SetValueExt<SOOrder.shipAddressID>(e.Row, null);
			}
			try
			{
				SOShippingContactAttribute.DefaultRecord<SOOrder.shipContactID>(sender, e.Row);
			}
			catch (SharedRecordMissingException)
			{
				var missingcontactexception = new PXSetPropertyException(Messages.DestinationSiteContactMayNotBeEmpty, PXErrorLevel.Error);
				if (UnattendedMode)
					throw missingcontactexception;
				else
					sender.RaiseExceptionHandling<SOOrder.destinationSiteID>(e.Row, sender.GetValueExt<SOOrder.destinationSiteID>(e.Row), missingcontactexception);

				destinationSiteIdErrorMessage = Messages.DestinationSiteContactMayNotBeEmpty;
				sender.SetValueExt<SOOrder.shipContactID>(e.Row, null);
			}
			}

			sender.SetValueExt<SOOrder.destinationSiteIdErrorMessage>(e.Row, destinationSiteIdErrorMessage);
		}

		protected virtual void SOOrder_CustomerLocationID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			sender.SetDefaultExt<SOOrder.salesPersonID>(e.Row);
			sender.SetDefaultExt<SOOrder.taxCalcMode>(e.Row);
			sender.SetDefaultExt<SOOrder.externalTaxExemptionNumber>(e.Row);
			sender.SetDefaultExt<SOOrder.avalaraCustomerUsageType>(e.Row);
			sender.SetDefaultExt<SOOrder.workgroupID>(e.Row);
			sender.SetDefaultExt<SOOrder.shipVia>(e.Row);
			sender.SetDefaultExt<SOOrder.fOBPoint>(e.Row);
			sender.SetDefaultExt<SOOrder.resedential>(e.Row);
			sender.SetDefaultExt<SOOrder.saturdayDelivery>(e.Row);
			sender.SetDefaultExt<SOOrder.groundCollect>(e.Row);
			sender.SetDefaultExt<SOOrder.insurance>(e.Row);
			sender.SetDefaultExt<SOOrder.shipTermsID>(e.Row);
			sender.SetDefaultExt<SOOrder.shipZoneID>(e.Row);
			if (PXAccess.FeatureInstalled<FeaturesSet.inventory>())
			{
				sender.SetDefaultExt<SOOrder.defaultSiteID>(e.Row);
			}
			sender.SetDefaultExt<SOOrder.priority>(e.Row);
			if (CustomerChanged)
			{
				if (!HasDetailRecords())
					sender.SetDefaultExt<SOOrder.shipComplete>(e.Row);
			}
			else
			{
				sender.SetDefaultExt<SOOrder.shipComplete>(e.Row);
			}
			sender.SetDefaultExt<SOOrder.shipDate>(e.Row);

			try
			{
				try
				{
					SOShippingAddressAttribute.DefaultRecord<SOOrder.shipAddressID>(sender, e.Row);
				}
				catch (SharedRecordMissingException)
				{
					var missingaddressexception = new PXSetPropertyException(Messages.DestinationSiteAddressMayNotBeEmpty, PXErrorLevel.Error);
					if (UnattendedMode)
						throw missingaddressexception;
					else
						sender.RaiseExceptionHandling<SOOrder.destinationSiteID>(e.Row, sender.GetValueExt<SOOrder.destinationSiteID>(e.Row), missingaddressexception);
				}
				try
				{
					SOShippingContactAttribute.DefaultRecord<SOOrder.shipContactID>(sender, e.Row);
				}
				catch (SharedRecordMissingException)
				{
					var missingcontactexception = new PXSetPropertyException(Messages.DestinationSiteContactMayNotBeEmpty, PXErrorLevel.Error);
					if (UnattendedMode)
						throw missingcontactexception;
					else
						sender.RaiseExceptionHandling<SOOrder.destinationSiteID>(e.Row, sender.GetValueExt<SOOrder.destinationSiteID>(e.Row), missingcontactexception);
				}
			}
			catch (PXFieldValueProcessingException ex)
			{
				ex.ErrorValue = location.Current.LocationCD;
				throw;
			}

			foreach (SOLine line in Transactions.Select())
			{
				try
				{
					Transactions.Cache.SetDefaultExt<SOLine.salesAcctID>(line);
				}
				catch (PXSetPropertyException)
				{
					Transactions.Cache.SetValue<SOLine.salesAcctID>(line, null);
				}
			}
		}

		protected virtual void SOOrder_CustomerID_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			SOOrder row = e.Row as SOOrder;
			if (row == null || object.Equals(e.NewValue, row.CustomerID)) return;

			if (!HasDetailRecords())
				return;

			Customer newCustomer = PXSelect<Customer, Where<Customer.bAccountID, Equal<Required<Customer.bAccountID>>>>.Select(this, e.NewValue);
			if (newCustomer != null)
			{
				if (newCustomer.AllowOverrideCury != true && newCustomer.CuryID != row.CuryID && !string.IsNullOrEmpty(newCustomer.CuryID))
				{
					RaiseCustomerIDSetPropertyException(sender, row, e.NewValue, Messages.CustomerChangedOnOrderWithRestrictedCurrency);
				}

				if (currencyinfo.Current != null &&
					currencyinfo.Current.CuryID != currencyinfo.Current.BaseCuryID &&
					newCustomer.AllowOverrideRate != true)
				{
					var newRateTypeID = !string.IsNullOrEmpty(newCustomer.CuryRateTypeID) ?
						newCustomer.CuryRateTypeID : new PXSetup<CMSetup>(this).Current.ARRateTypeDflt;

					if (newRateTypeID != currencyinfo.Current.CuryRateTypeID)
					RaiseCustomerIDSetPropertyException(sender, row, e.NewValue, Messages.CustomerChangedOnOrderWithRestrictedRateType);
				}
			}

			if (Document.Cache.GetStatus(Document.Current) != PXEntryStatus.Inserted)
			{
				SOOrderShipment topShipment = shipmentlist.SelectSingle();
				if (topShipment != null)
				{
					RaiseCustomerIDSetPropertyException(sender, row, e.NewValue, Messages.CustomerChangedOnShippedOrder);
				}

				PXSelectBase<POLine> selectlinkedDropShips = new PXSelectJoin<POLine,
					InnerJoin<SOLineSplit, On<SOLineSplit.pOType, Equal<POLine.orderType>, And<SOLineSplit.pONbr, Equal<POLine.orderNbr>, And<SOLineSplit.pOLineNbr, Equal<POLine.lineNbr>>>>>,
					Where<SOLineSplit.orderType, Equal<Current<SOOrder.orderType>>,
					And<SOLineSplit.orderNbr, Equal<Current<SOOrder.orderNbr>>,
					And<POLine.orderType, Equal<POOrderType.dropShip>>>>>(this);

				POLine topPOLine = selectlinkedDropShips.SelectWindowed(0, 1);
				if (topPOLine != null)
				{
					RaiseCustomerIDSetPropertyException(sender, row, e.NewValue, Messages.CustomerChangedOnOrderWithDropShip);
				}

				if (Document.Current.Behavior == SOBehavior.QT)
				{
				SOOrder associatedOrder = PXSelect<SOOrder, Where<SOOrder.origOrderType, Equal<Current<SOOrder.orderType>>,
				And<SOOrder.origOrderNbr, Equal<Current<SOOrder.orderNbr>>>>>.Select(this);
				if (associatedOrder != null && associatedOrder.CustomerID != (int?)e.NewValue)
				{
					RaiseCustomerIDSetPropertyException(sender, row, e.NewValue, Messages.CustomerChangedOnQuoteWithOrder);
				}
				}

				SOAdjust topSOAdjust = Adjustments.SelectSingle();
				if (topSOAdjust != null)
				{
					RaiseCustomerIDSetPropertyException(sender, row, e.NewValue, Messages.CustomerChangedOnOrderWithARPayments);
				}
			}

			SOLine invoiced = (SOLine)Transactions.Select().AsEnumerable().Where(res => !string.IsNullOrEmpty(((SOLine)res).InvoiceNbr)).FirstOrDefault();
			if (invoiced != null)
			{
				RaiseCustomerIDSetPropertyException(sender, row, e.NewValue, Messages.CustomerChangedOnOrderWithInvoices);
			}


		}

		public virtual void RaiseCustomerIDSetPropertyException(PXCache sender, SOOrder order, object newCustomerID, string error)
		{
			BAccountR newAccount = (BAccountR)PXSelectorAttribute.Select<SOOrder.customerID>(sender, order, newCustomerID);
			var ex = new PXSetPropertyException(error);
			ex.ErrorValue = newAccount?.AcctCD;
			throw ex;
		}

		public virtual bool HasDetailRecords()
		{
			if (Transactions.Current != null)
				return true;

			if (Document.Cache.GetStatus(Document.Current) == PXEntryStatus.Inserted)
			{
				return Transactions.Cache.IsDirty;
			}
			else
			{
				return Transactions.Select().Count > 0;
			}
		}

		public bool CustomerChanged { get; protected set; }

		[CustomerOrderNbr]
		[PXMergeAttributes(Method = MergeMethod.Append)]
		protected virtual void SOOrder_CustomerOrderNbr_CacheAttached(PXCache sender)
		{
		}

		[PopupMessage]
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		protected virtual void SOOrder_CustomerID_CacheAttached(PXCache sender)
		{
		}


		protected virtual void SOOrder_CustomerID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			int? oldCustomerID = (int?)e.OldValue;
			CustomerChanged = oldCustomerID != null && ((SOOrder)e.Row).CustomerID != oldCustomerID;

			if (CustomerChanged && ((SOOrder)e.Row).CreditHold == true)
			{
                SOOrder.Events.Select(ev => ev.CreditLimitSatisfied).FireOn(this, (SOOrder)e.Row);
			}

			sender.SetValue<SOOrder.extRefNbr>(e.Row, null);
			sender.SetValue<SOOrder.approvedCredit>(e.Row, false);
			sender.SetValue<SOOrder.approvedCreditAmt>(e.Row, 0m);
			sender.SetValue<SOOrder.overrideTaxZone>(e.Row, false);
			sender.SetDefaultExt<SOOrder.paymentMethodID>(e.Row);
			sender.SetDefaultExt<SOOrder.billSeparately>(e.Row);
			sender.SetDefaultExt<SOOrder.shipSeparately>(e.Row);
			
			var orderType = SOOrderType.PK.Find(this, ((SOOrder)e.Row).OrderType);
			if (orderType.Behavior != SOBehavior.TR)
			{
			sender.SetValue<SOOrder.origOrderType>(e.Row, null);
			sender.SetValue<SOOrder.origOrderNbr>(e.Row, null);
			}
			sender.SetDefaultExt<SOOrder.projectID>(e.Row);

			if (CustomerChanged)
			{
				sender.SetValue<SOOrder.customerRefNbr>(e.Row, null);
			}

			if (!e.ExternalCall && customer.Current != null)
			{
				customer.Current.CreditRule = null;
			}

			if (PXAccess.FeatureInstalled<FeaturesSet.multicurrency>() 
				&& (e.ExternalCall || sender.GetValuePending<SOOrder.curyID>(e.Row) == null))
			{
				if (oldCustomerID == null || CustomerChanged && !HasDetailRecords())
				{
					CurrencyInfo info = CurrencyInfoAttribute.SetDefaults<SOOrder.curyInfoID>(sender, e.Row);

					string message = PXUIFieldAttribute.GetError<CurrencyInfo.curyEffDate>(currencyinfo.Cache, info);
					if (string.IsNullOrEmpty(message) == false)
					{
						sender.RaiseExceptionHandling<SOOrder.orderDate>(e.Row, ((SOOrder)e.Row).OrderDate, new PXSetPropertyException(message, PXErrorLevel.Warning));
					}

					if (info != null)
					{
						sender.SetValue<SOOrder.curyID>(e.Row, info.CuryID);
					}
				}
				else
				{
					// We should not change the currency when we have details, just reloading rate by effective date.
					CurrencyInfoAttribute.SetEffectiveDate<SOOrder.orderDate>(sender, e);
				}
			}

			{
				sender.SetDefaultExt<SOOrder.customerLocationID>(e.Row);
				if (e.ExternalCall || sender.GetValuePending<SOOrder.termsID>(e.Row) == null)
				{
					if (soordertype.Current.ARDocType != ARDocType.CreditMemo)
					{
						sender.SetDefaultExt<SOOrder.termsID>(e.Row);
					}
					else
					{
						sender.SetValueExt<SOOrder.termsID>(e.Row, null);
					}
				}
				//sender.SetDefaultExt<SOOrder.pMInstanceID>(e.Row);
			}


			try
			{
				SOBillingAddressAttribute.DefaultRecord<SOOrder.billAddressID>(sender, e.Row);
				SOBillingContactAttribute.DefaultRecord<SOOrder.billContactID>(sender, e.Row);
			}
			catch (PXFieldValueProcessingException ex)
			{
				ex.ErrorValue = customer.Current.AcctCD;
				throw;
			}
			SOOrder row = e.Row as SOOrder;
			if (row != null)
			{
				sender.SetDefaultExt<SOOrder.ownerID>(row);

			}
			sender.SetDefaultExt<SOOrder.taxZoneID>(e.Row);

			foreach (SOLine line in Transactions.Select())
			{
				line.CustomerID = Document.Current.CustomerID;
				Transactions.Update(line);
			}
			if (row.ProjectID != null)
			{
				//show warning if Project.Customer - Customer missmatch
				object val = row.ProjectID;
				sender.RaiseFieldVerifying<SOOrder.projectID>(row, ref val);
			}
			sender.SetValue<SOOrder.emailed>(row, false);
		}

		protected virtual void SOOrder_PaymentMethodID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			sender.SetDefaultExt<SOOrder.pMInstanceID>(e.Row);
			sender.SetDefaultExt<SOOrder.cashAccountID>(e.Row);
		}

		protected virtual void SOOrder_PMInstanceID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			sender.SetDefaultExt<SOOrder.cashAccountID>(e.Row);
		}

		protected virtual void SOOrder_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			SOOrder doc = (SOOrder)e.Row;

			PXDefaultAttribute.SetPersistingCheck<SOOrder.invoiceDate>(sender, doc, (doc.IsInvoiceOrder == true && doc.BillSeparately == true) ? PXPersistingCheck.NullOrBlank : PXPersistingCheck.Nothing);
			PXDefaultAttribute.SetPersistingCheck<SOOrder.finPeriodID>(sender, doc, (doc.IsInvoiceOrder == true && doc.BillSeparately == true) ? PXPersistingCheck.NullOrBlank : PXPersistingCheck.Nothing);
			PXDefaultAttribute.SetPersistingCheck<SOOrder.invoiceNbr>(sender, doc, (doc.IsInvoiceOrder == true && doc.IsUserInvoiceNumbering == true) ? PXPersistingCheck.NullOrBlank : PXPersistingCheck.Nothing);
			PXDefaultAttribute.SetPersistingCheck<SOOrder.termsID>(sender, doc, (doc.ARDocType != ARDocType.Undefined
							&& doc.ARDocType != ARDocType.CreditMemo) ? PXPersistingCheck.NullOrBlank : PXPersistingCheck.Nothing);

			bool isPMCreditCard = false;
			if (doc.IsPaymentInfoEnabled == true && String.IsNullOrEmpty(doc.PaymentMethodID) == false)
			{
				PaymentMethod pm = PXSelect<PaymentMethod, Where<PaymentMethod.paymentMethodID, Equal<Required<PaymentMethod.paymentMethodID>>>>.Select(this, doc.PaymentMethodID);
				isPMCreditCard = pm != null && pm.PaymentType == PaymentMethodType.CreditCard;
			}
			PXDefaultAttribute.SetPersistingCheck<SOOrder.dueDate>(sender, doc, (doc.IsInvoiceOrder == true && doc.BillSeparately == true
							&& doc.ARDocType != ARDocType.CreditMemo) ? PXPersistingCheck.NullOrBlank : PXPersistingCheck.Nothing);
			PXDefaultAttribute.SetPersistingCheck<SOOrder.discDate>(sender, doc, (doc.IsInvoiceOrder == true && doc.BillSeparately == true
							&& doc.ARDocType != ARDocType.CreditMemo) ? PXPersistingCheck.NullOrBlank : PXPersistingCheck.Nothing);
			PXDefaultAttribute.SetPersistingCheck<SOOrder.paymentMethodID>(sender, doc, (doc.IsInvoiceOrder == true && doc.BillSeparately == true && (doc.ARDocType == ARDocType.CashSale
							|| doc.ARDocType == ARDocType.CashReturn)) ? PXPersistingCheck.NullOrBlank : PXPersistingCheck.Nothing);
			PXDefaultAttribute.SetPersistingCheck<SOOrder.cashAccountID>(sender, doc, (doc.IsInvoiceOrder == true && doc.BillSeparately == true && (doc.ARDocType == ARDocType.CashSale
							|| doc.ARDocType == ARDocType.CashReturn) && !string.IsNullOrEmpty(doc.PaymentMethodID)) ? PXPersistingCheck.NullOrBlank : PXPersistingCheck.Nothing);
			PXDefaultAttribute.SetPersistingCheck<SOOrder.extRefNbr>(sender, doc, (doc.IsInvoiceOrder == true && doc.BillSeparately == true && (doc.ARDocType == ARDocType.CashSale
							|| doc.ARDocType == ARDocType.CashReturn) && !isPMCreditCard && arsetup.Current.RequireExtRef == true) ? PXPersistingCheck.NullOrBlank : PXPersistingCheck.Nothing);

			if (e.Operation == PXDBOperation.Insert || e.Operation == PXDBOperation.Update)
			{
				if (PXAccess.FeatureInstalled<FeaturesSet.multicurrency>() && doc.IsTransferOrder != true && doc.CuryInfoID != null)
				{
					var curinfo = (CurrencyInfo)currencyinfo.SelectWindowed(0, 1, doc.CuryInfoID);
					CMSetup cmsetup = PXSelect<CMSetup>.Select(this);

					if (cmsetup != null && curinfo != null && curinfo.BaseCuryID != curinfo.CuryID && curinfo.CuryRateTypeID == null)
						throw new PXRowPersistingException(typeof(SOOrder.curyID).Name, doc.CuryID, Messages.EmptyCurrencyRateType);
				}

				if ((doc.CuryDiscTot ?? 0m) > Math.Abs((doc.CuryLineTotal ?? 0m) + (doc.CuryMiscTot ?? 0m)))
				{
					if (sender.RaiseExceptionHandling<SOOrder.curyDiscTot>(e.Row, doc.CuryDiscTot, new PXSetPropertyException(AR.Messages.DiscountGreaterDetailTotal, PXErrorLevel.Error)))
					{
						throw new PXRowPersistingException(typeof(SOOrder.curyDiscTot).Name, null, AR.Messages.DiscountGreaterDetailTotal);
					}
				}

				if (doc.Status == SOOrderStatus.Completed && doc.Hold == true)
				{
					throw new PXRowPersistingException(typeof(SOOrder.status).Name, null, PXMessages.LocalizeFormatNoPrefixNLA(Messages.DocumentOnHoldCannotBeCompleted, doc.OrderNbr));
				}

				if (doc != null && doc.IsTransferOrder == true)
				{
					var destinationSiteIdErrorString = PXUIFieldAttribute.GetError<SOOrder.destinationSiteID>(sender, e.Row);
					if (destinationSiteIdErrorString == null)
					{
						var destinationSiteIdErrorMessage = doc.DestinationSiteIdErrorMessage;
						if (!string.IsNullOrWhiteSpace(destinationSiteIdErrorMessage))
						{
							throw new PXRowPersistingException(typeof(SOOrder.destinationSiteID).Name, sender.GetValueExt<SOOrder.destinationSiteID>(e.Row), destinationSiteIdErrorMessage);
						}
					}
				}

				//Inclusive taxes and tax calculation mode = gross are not currently allowed when automatic tax calculation is disabled
				if (doc?.DisableAutomaticTaxCalculation == true)
				{
					if (doc.TaxCalcMode == TaxCalculationMode.Gross)
					{
						throw new PXRowPersistingException(typeof(SOOrder.taxCalcMode).Name, null, PXMessages.LocalizeNoPrefix(Messages.GrossModeIsNotAllowedWhenTaxCalculationDisabled));
					}

					if (doc.TaxCalcMode != TaxCalculationMode.Net)
					{
						foreach (PXResult<SOTaxTran, Tax> soTax in Taxes.Select())
						{
							Tax tax = soTax;
							if (tax?.TaxCalcLevel == CSTaxCalcLevel.Inclusive)
								throw new PXRowPersistingException(typeof(SOOrder.disableAutomaticTaxCalculation).Name, doc.DisableAutomaticTaxCalculation, PXMessages.LocalizeNoPrefix(Messages.InclusiveTaxesNotAllowedWhenTaxCalculationDisabled));
						}
					}
				}
			}

			if (e.Operation == PXDBOperation.Update)
			{
				if (doc.ShipmentCntr < 0 || doc.OpenShipmentCntr < 0 || doc.ShipmentCntr < doc.BilledCntr + doc.ReleasedCntr && doc.Behavior == SOBehavior.SO)
				{
					throw new Exceptions.InvalidShipmentCountersException();
				}
			}

			if (IsMobile) // check control total when persisting from mobile
			{
				if (doc.Hold == false && doc.Completed == false)
				{
					if (doc.CuryOrderTotal != doc.CuryControlTotal)
					{
						sender.RaiseExceptionHandling<SOOrder.curyControlTotal>(e.Row, doc.CuryControlTotal, new PXSetPropertyException(Messages.DocumentOutOfBalance));
					}
					else if (doc.CuryOrderTotal < 0m && doc.ARDocType != ARDocType.NoUpdate && doc.Behavior.IsNotIn(SOBehavior.RM, SOBehavior.MO))
					{
						if (soordertype.Current.RequireControlTotal == true)
						{
							sender.RaiseExceptionHandling<SOOrder.curyControlTotal>(e.Row, doc.CuryControlTotal, new PXSetPropertyException(Messages.DocumentBalanceNegative));
						}
						else
						{
							sender.RaiseExceptionHandling<SOOrder.curyOrderTotal>(e.Row, doc.CuryOrderTotal, new PXSetPropertyException(Messages.DocumentBalanceNegative));
						}
					}
					else
					{
						if (soordertype.Current.RequireControlTotal == true)
						{
							sender.RaiseExceptionHandling<SOOrder.curyControlTotal>(e.Row, null, null);
						}
						else
						{
							sender.RaiseExceptionHandling<SOOrder.curyOrderTotal>(e.Row, null, null);
						}
					}
				}
			}
		}

		protected virtual void SOOrder_OrderDate_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			CurrencyInfoAttribute.SetEffectiveDate<SOOrder.orderDate>(sender, e);
		}

		protected virtual void SOOrder_BillSeparately_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			sender.SetDefaultExt<SOOrder.invoiceDate>(e.Row);
			sender.SetDefaultExt<SOOrder.invoiceNbr>(e.Row);
			sender.SetDefaultExt<SOOrder.extRefNbr>(e.Row);

			if (((SOOrder)e.Row).BillSeparately == false)
			{
				sender.SetValuePending<SOOrder.invoiceDate>(e.Row, null);
				sender.SetValuePending<SOOrder.invoiceNbr>(e.Row, null);
				sender.SetValuePending<SOOrder.extRefNbr>(e.Row, null);
				sender.SetValuePending<SOOrder.finPeriodID>(e.Row, null);
			}
		}

		protected virtual void SOOrder_FinPeriodID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			var order = e.Row as SOOrder;
			if (e.Row == null || order.BillSeparately == false || soordertype.Current == null || order.IsInvoiceOrder == false)
			{
				e.NewValue = null;
				e.Cancel = true;
			}
		}

		protected virtual void SOOrder_InvoiceDate_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			var order = e.Row as SOOrder;
			if (e.Row == null || order.BillSeparately == false || soordertype.Current == null || order.IsInvoiceOrder == false)
			{
				e.NewValue = null;
			}
			else
			{
				e.NewValue = sosetup.Current.UseShipDateForInvoiceDate == true ? sender.GetValue<SOOrder.shipDate>(e.Row) : sender.GetValue<SOOrder.orderDate>(e.Row);
			}
			e.Cancel = true;
		}

		protected virtual void SOOrder_InvoiceNbr_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			var order = e.Row as SOOrder;
			if (e.Row == null || order.BillSeparately == false || soordertype.Current == null || order.IsInvoiceOrder == false)
			{
				e.NewValue = null;
				e.Cancel = true;
			}
		}

		protected virtual void SOOrder_Priority_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			SOOrder row = e.Row as SOOrder;
			if (row != null)
			{
				if (location.Current != null && location.Current.COrderPriority != null)
				{
					e.NewValue = location.Current.COrderPriority ?? 0;
				}
			}
		}

		protected virtual void SOOrder_ShipComplete_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			SOOrder row = e.Row as SOOrder;
			if (row != null)
			{
				if (location.Current != null && !string.IsNullOrEmpty(location.Current.CShipComplete))
				{
					e.NewValue = location.Current.CShipComplete;
				}
				else
				{
					e.NewValue = SOShipComplete.CancelRemainder;
				}

				if ((string)e.NewValue == SOShipComplete.BackOrderAllowed && soordertype.Current != null && soordertype.Current.RequireLocation == true)
				{
					e.NewValue = SOShipComplete.CancelRemainder;
				}
			}
		}

		protected virtual void SOOrder_PMInstanceID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			var order = e.Row as SOOrder;
			if (e.Row == null || soordertype.Current == null || order.IsPaymentInfoEnabled == false)
			{
				e.NewValue = null;
				e.Cancel = true;
			}
		}

		protected virtual void SOOrder_PaymentMethodID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			var order = e.Row as SOOrder;
			if (e.Row == null || soordertype.Current == null || order.IsPaymentInfoEnabled == false)
			{
				e.NewValue = null;
				e.Cancel = true;
			}
		}

		protected virtual void SOOrder_CashAccountID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			var order = e.Row as SOOrder;
			if (e.Row == null || soordertype.Current == null || order.IsPaymentInfoEnabled == false)
			{
				e.NewValue = null;
				e.Cancel = true;
			}
		}
		protected virtual void SOOrder_CashAccountID_ExceptionHandling(PXCache sender, PXExceptionHandlingEventArgs e)
		{
			if (e.Exception == null)
				return;

			SOOrder doc = e.Row as SOOrder;
			if (doc != null)
			{
				e.Cancel = true;
				doc.CashAccountID = null;
			}
		}

		protected virtual void SOOrder_OverrideTaxZone_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			SOOrder row = e.Row as SOOrder;
			if (row == null) return;

			if (row.OverrideTaxZone != true)
			{
				sender.SetDefaultExt<SOOrder.taxZoneID>(e.Row);
			}
		}

		protected virtual void SOOrder_ShipVia_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			SOOrder row = e.Row as SOOrder;
			if (row != null)
			{
				if (row.OverrideTaxZone != true && (e.OldValue == null || (e.OldValue != null && IsCommonCarrier(e.OldValue.ToString()) != IsCommonCarrier(row.ShipVia))))
					sender.SetDefaultExt<SOOrder.taxZoneID>(e.Row);

				sender.SetDefaultExt<SOOrder.freightTaxCategoryID>(e.Row);
				row.UseCustomerAccount = CanUseCustomerAccount(row);
			}
		}

		protected virtual void SOOrder_IsManualPackage_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			SOOrder row = e.Row as SOOrder;
			if (row != null && row.IsManualPackage != true)
			{
				foreach (SOPackageInfoEx pack in Packages.Select())
				{
					Packages.Delete(pack);
				}
				row.PackageWeight = 0;
				sender.SetValue<SOOrder.isPackageValid>(row, false);
			}
		}

		protected virtual void SOOrder_ProjectID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			SOOrder row = e.Row as SOOrder;
			if (row != null)
			{
				foreach (SOLine tran in Transactions.Select())
				{
					tran.ProjectID = row.ProjectID;
					Transactions.Update(tran);
				}

			}
		}

		protected virtual bool CanUseCustomerAccount(SOOrder row)
		{
			Carrier carrier = Carrier.PK.Find(this, row.ShipVia);
			if (carrier != null && !string.IsNullOrEmpty(carrier.CarrierPluginID))
			{
				foreach (CarrierPluginCustomer cpc in PXSelect<CarrierPluginCustomer,
						Where<CarrierPluginCustomer.carrierPluginID, Equal<Required<CarrierPluginCustomer.carrierPluginID>>,
						And<CarrierPluginCustomer.customerID, Equal<Required<CarrierPluginCustomer.customerID>>,
						And<CarrierPluginCustomer.isActive, Equal<True>>>>>.Select(this, carrier.CarrierPluginID, row.CustomerID))
				{
					if (!string.IsNullOrEmpty(cpc.CarrierAccount) &&
						(cpc.CustomerLocationID == row.CustomerLocationID || cpc.CustomerLocationID == null)
						)
					{
						return true;
					}
				}
			}

			return false;
		}

		protected virtual bool CanUseGroundCollect(SOOrder row)
		{
			if (string.IsNullOrEmpty(row.ShipVia))
				return false;

			Carrier carrier = Carrier.PK.Find(this, row.ShipVia);
			if (carrier?.IsExternal != true || string.IsNullOrEmpty(carrier?.CarrierPluginID))
				return false;

			return CarrierPluginMaint.GetCarrierPluginAttributes(this, carrier.CarrierPluginID).Contains("COLLECT");
		}

		protected virtual void SOOrder_UseCustomerAccount_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			SOOrder row = e.Row as SOOrder;
			if (row != null)
			{
				bool canBeTrue = CanUseCustomerAccount(row);

				if (e.NewValue != null && ((bool)e.NewValue) && !canBeTrue)
				{
					e.NewValue = false;
					throw new PXSetPropertyException(Messages.CustomeCarrierAccountIsNotSetup);
				}
			}
		}


		protected virtual void SOOrder_ShipDate_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
		{
			cache.SetDefaultExt<SOOrder.invoiceDate>(e.Row);

			DateTime? oldDate = (DateTime?)e.OldValue;
			if (oldDate != ((SOOrder)e.Row).ShipDate)
			{
				if (Document.View.Answer == WebDialogResult.None && !this.IsMobile && ((SOOrder)e.Row).ShipComplete == SOShipComplete.BackOrderAllowed && HasDetailRecords())
					Document.Ask(GL.Messages.Confirmation, Messages.ConfirmShipDateRecalc, MessageButtons.YesNo);
			}
		}

		protected virtual void SOOrder_ExtRefNbr_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			var order = e.Row as SOOrder;
			if (e.Row == null || order.BillSeparately == false || soordertype.Current == null || order.IsInvoiceOrder == false || soordertype.Current.ARDocType != ARDocType.CashSale && soordertype.Current.ARDocType != ARDocType.CashReturn)
			{
				e.NewValue = null;
				e.Cancel = true;
			}
		}

		public bool IsCopyOrder
		{
			get;
			protected set;
		}

		/// <summary>
		/// The flag indicates that group and document discounts are disabled.
		/// </summary>
		protected virtual bool DisableGroupDocDiscount
		{
			get
			{
				return (Document.Current?.IsRMAOrder == true || Document.Current?.IsTransferOrder == true)
					&& soordertype.Current.ARDocType == ARDocType.NoUpdate;
			}
		}

		protected virtual void SOOrder_DontApprove_FieldDefaulting(PXCache cache, PXFieldDefaultingEventArgs e)
		{
			var row = (SOOrder)e.Row;
			if (!string.IsNullOrEmpty(row?.OrderType) && sosetup.Current.OrderRequestApproval == true)
			{
				SOSetupApproval setupApproval = SetupApproval.Select(row.OrderType);
				if (setupApproval != null)
				{
					e.NewValue = setupApproval.IsActive != true;
					e.Cancel = true;
				}
			}
		}

		protected virtual void SOOrder_RowSelecting(PXCache cache, PXRowSelectingEventArgs e)
		{
			var row = (SOOrder)e.Row;
			if (!string.IsNullOrEmpty(row?.OrderType) && sosetup.Current.OrderRequestApproval == true)
			{
				// Acuminator disable once PX1042 DatabaseQueriesInRowSelecting [false positive]
				SOSetupApproval setupApproval = SetupApproval.Select(row.OrderType);
				if (setupApproval != null)
				{
					row.DontApprove = setupApproval.IsActive != true;
				}
			}
		}

		protected SOOrder _LastSelected;
		protected string orderType;
		protected virtual void SOOrder_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
		{
			SOOrder doc = e.Row as SOOrder;

			if (doc == null)
			{
				return;
			}

			orderType = doc.OrderType;

			if (doc.DeferPriceDiscountRecalculation == true && this.IsCopyOrder == false)
			{
				TaxAttribute.SetTaxCalc<SOLine.taxCategoryID>(this.Transactions.Cache, null, TaxCalc.ManualCalc | TaxCalc.RedefaultAlways);
			}

			if (!object.ReferenceEquals(doc, _LastSelected))
			{
				PXUIFieldAttribute.SetVisible<SOLine.operation>(this.Transactions.Cache, null, soordertype.Current.ActiveOperationsCntr > 1);
				PXUIFieldAttribute.SetVisible<SOPackageInfo.operation>(this.Packages.Cache, null, soordertype.Current.ActiveOperationsCntr > 1);
				PXUIFieldAttribute.SetVisible<SOLine.autoCreateIssueLine>(this.Transactions.Cache, null, soordertype.Current.ActiveOperationsCntr > 1);
				PXUIFieldAttribute.SetVisible<SOLine.curyUnitCost>(this.Transactions.Cache, null, IsCuryUnitCostVisible(doc));
				_LastSelected = doc;
			}
			PXUIFieldAttribute.SetVisible<SOOrder.curyID>(cache, doc,
				PXAccess.FeatureInstalled<FeaturesSet.multicurrency>() && doc.IsTransferOrder != true);

			bool customerDiscountsFeatureInstalled = PXAccess.FeatureInstalled<FeaturesSet.customerDiscounts>();

			ValidateControlTotal(cache, e);

			bool curyenabled = IsCurrencyEnabled(doc);
			bool isPMCreditCard = false;
			bool isFreightEditable = doc.IsFreightAvailable ?? false;

			this.prepareInvoice.SetEnabled(doc.Hold == false && doc.Cancelled == false &&
				// The system doesn't support the Prepare Invoice action for blanket order on the mass processing screen.
				(soordertype.Current.ARDocType != ARDocType.NoUpdate || doc.Behavior == SOBehavior.BL) &&
				(doc.ShipmentCntr - doc.OpenShipmentCntr - doc.BilledCntr - doc.ReleasedCntr) > 0 ||
				HasMiscLinesToInvoice(doc) ||
				(SOBehavior.GetRequireShipmentValue(doc.Behavior, soordertype.Current.RequireShipping) == false));

			bool allowAllocation = AllowAllocation();
			if (doc == null || doc.Completed == true || doc.Cancelled == true || !allowAllocation)
			{
				PXUIFieldAttribute.SetEnabled(cache, doc, false);
				cache.AllowDelete = false;
				cache.AllowUpdate = allowAllocation;
				Transactions.Cache.AllowDelete = false;
				Transactions.Cache.AllowUpdate = false;
				Transactions.Cache.AllowInsert = false;

				this.Caches.SubscribeCacheCreated(Adjustments.GetItemType(), delegate
				{
					Adjustments.Cache.AllowInsert = false;
					Adjustments.Cache.AllowUpdate = false;
					Adjustments.Cache.AllowDelete = false;
				});


				DiscountDetails.Cache.AllowDelete = false;
				DiscountDetails.Cache.AllowUpdate = false;
				DiscountDetails.Cache.AllowInsert = false;

				Taxes.Cache.AllowUpdate = false;
				SalesPerTran.Cache.AllowUpdate = false;
			}
			else
			{
				bool isCashReturn = doc.ARDocType == ARDocType.CashReturn;
				PXUIFieldAttribute.SetEnabled(cache, doc, true);
				PXUIFieldAttribute.SetEnabled<SOOrder.status>(cache, doc, false);
				PXUIFieldAttribute.SetEnabled<SOOrder.orderQty>(cache, doc, false);
				PXUIFieldAttribute.SetEnabled<SOOrder.orderWeight>(cache, doc, false);
				PXUIFieldAttribute.SetEnabled<SOOrder.orderVolume>(cache, doc, false);
				PXUIFieldAttribute.SetEnabled<SOOrder.packageWeight>(cache, doc, false);
				PXUIFieldAttribute.SetEnabled<SOOrder.curyOrderTotal>(cache, doc, false);
				PXUIFieldAttribute.SetEnabled<SOOrder.curyUnpaidBalance>(cache, doc, false);
				PXUIFieldAttribute.SetEnabled<SOOrder.curyLineTotal>(cache, doc, false);
				PXUIFieldAttribute.SetEnabled<SOOrder.curyGoodsExtPriceTotal>(cache, doc, false);
				PXUIFieldAttribute.SetEnabled<SOOrder.curyMiscTot>(cache, doc, false);
				PXUIFieldAttribute.SetEnabled<SOOrder.curyMiscExtPriceTotal>(cache, doc, false);
				PXUIFieldAttribute.SetEnabled<SOOrder.curyDetailExtPriceTotal>(cache, doc, false);
				PXUIFieldAttribute.SetEnabled<SOOrder.curyFreightCost>(cache, doc, false);
				PXUIFieldAttribute.SetEnabled<SOOrder.freightCostIsValid>(cache, doc, false);
				PXUIFieldAttribute.SetEnabled<SOOrder.curyFreightAmt>(cache, doc, doc.OverrideFreightAmount == true);
				PXUIFieldAttribute.SetEnabled<SOOrder.curyTaxTotal>(cache, doc, false);
				PXUIFieldAttribute.SetEnabled<SOOrder.openOrderQty>(cache, doc, false);
				PXUIFieldAttribute.SetEnabled<SOOrder.curyOpenOrderTotal>(cache, doc, false);
				PXUIFieldAttribute.SetEnabled<SOOrder.curyOpenLineTotal>(cache, doc, false);
				PXUIFieldAttribute.SetEnabled<SOOrder.curyOpenTaxTotal>(cache, doc, false);
				PXUIFieldAttribute.SetEnabled<SOOrder.unbilledOrderQty>(cache, doc, false);
				PXUIFieldAttribute.SetEnabled<SOOrder.curyUnbilledOrderTotal>(cache, doc, false);
				PXUIFieldAttribute.SetEnabled<SOOrder.curyUnbilledLineTotal>(cache, doc, false);
				PXUIFieldAttribute.SetEnabled<SOOrder.curyUnbilledTaxTotal>(cache, doc, false);

				PXUIFieldAttribute.SetEnabled<SOOrder.curyPaymentTotal>(cache, doc, false);
				PXUIFieldAttribute.SetEnabled<SOOrder.curyID>(cache, doc, curyenabled);

				PXUIFieldAttribute.SetEnabled<SOOrder.curyUnreleasedPaymentAmt>(cache, doc, false);
				PXUIFieldAttribute.SetEnabled<SOOrder.curyCCAuthorizedAmt>(cache, doc, false);
				PXUIFieldAttribute.SetEnabled<SOOrder.curyPaidAmt>(cache, doc, false);
				PXUIFieldAttribute.SetEnabled<SOOrder.curyBilledPaymentTotal>(cache, doc, false);

				PXUIFieldAttribute.SetEnabled<SOOrder.origOrderType>(cache, doc, false);
				PXUIFieldAttribute.SetEnabled<SOOrder.origOrderNbr>(cache, doc, false);
				PXUIFieldAttribute.SetEnabled<SOOrder.curyVatExemptTotal>(cache, doc, false);
				PXUIFieldAttribute.SetEnabled<SOOrder.curyVatTaxableTotal>(cache, doc, false);
				PXUIFieldAttribute.SetEnabled<SOOrder.overrideFreightAmount>(cache, doc, AllowChangingOverrideFreightAmount(doc));
				PXUIFieldAttribute.SetEnabled<SOOrder.freightAmountSource>(cache, doc, false);
				PXUIFieldAttribute.SetEnabled<SOOrder.disableAutomaticTaxCalculation>(cache, doc, doc.BilledCntr == 0 && doc.ReleasedCntr == 0);

				PXUIFieldAttribute.SetEnabled<SOOrder.emailed>(cache, doc, false);
				PXUIFieldAttribute.SetEnabled<SOOrder.printed>(cache, doc, false);

				if (soordertype.Current != null)
				{
					bool isInvoiceInfoEnabled = doc.IsInvoiceOrder == true && doc.BillSeparately == true;
					bool isPMInstanceRequired = false;
					bool hasPaymentMethod = !String.IsNullOrEmpty(doc.PaymentMethodID);
					if (doc.IsPaymentInfoEnabled == true && hasPaymentMethod)
					{
						PaymentMethod pm = PXSelect<PaymentMethod, Where<PaymentMethod.paymentMethodID, Equal<Required<PaymentMethod.paymentMethodID>>>>.Select(this, doc.PaymentMethodID);
						if (pm != null)
						{
							isPMInstanceRequired = pm.IsAccountNumberRequired == true;
							isPMCreditCard = pm.PaymentType == PaymentMethodType.CreditCard;
						}
					}
					PXUIFieldAttribute.SetEnabled<SOOrder.billSeparately>(cache, doc, doc.IsNoAROrder != true && soordertype.Current.Behavior != SOBehavior.MO);
					PXUIFieldAttribute.SetEnabled<SOOrder.invoiceDate>(cache, doc, isInvoiceInfoEnabled);
					PXUIFieldAttribute.SetRequired<SOOrder.invoiceDate>(cache, (isInvoiceInfoEnabled && soordertype.Current.Behavior == SOBehavior.IN) || (doc.ARDocType == ARDocType.CreditMemo && doc.BillSeparately == true));
					PXUIFieldAttribute.SetEnabled<SOOrder.invoiceNbr>(cache, doc, isInvoiceInfoEnabled);
					PXUIFieldAttribute.SetEnabled<SOOrder.finPeriodID>(cache, doc, isInvoiceInfoEnabled);
					PXUIFieldAttribute.SetRequired<SOOrder.finPeriodID>(cache, (isInvoiceInfoEnabled && soordertype.Current.Behavior == SOBehavior.IN) || (doc.ARDocType == ARDocType.CreditMemo && doc.BillSeparately == true));

					bool enablePaymentMethodSelection = (doc.IsPaymentInfoEnabled == true && doc.CustomerID.HasValue);
					PXUIFieldAttribute.SetEnabled<SOOrder.paymentMethodID>(cache, doc, enablePaymentMethodSelection);
					PXUIFieldAttribute.SetRequired<SOOrder.paymentMethodID>(cache, doc.BillSeparately == true && doc.ARDocType.IsIn(ARDocType.CashSale, ARDocType.CashReturn));
					PXUIFieldAttribute.SetEnabled<SOOrder.pMInstanceID>(cache, doc, enablePaymentMethodSelection && isPMInstanceRequired);
					PXUIFieldAttribute.SetEnabled<SOOrder.cashAccountID>(cache, doc, doc.IsPaymentInfoEnabled == true && hasPaymentMethod);
					PXUIFieldAttribute.SetRequired<SOOrder.cashAccountID>(cache, doc.IsPaymentInfoEnabled == true && hasPaymentMethod && doc.IsInvoiceOrder == true
						&& doc.BillSeparately == true && doc.ARDocType.IsIn(ARDocType.CashSale, ARDocType.CashReturn));

					PXUIFieldAttribute.SetEnabled<SOOrder.extRefNbr>(cache, doc, doc.IsCashSaleOrder == true);
					PXUIFieldAttribute.SetRequired<SOOrder.extRefNbr>(cache, (doc.IsCashSaleOrder == true && doc.IsInvoiceOrder == true && doc.BillSeparately == true) && !isPMCreditCard && arsetup.Current.RequireExtRef == true);

					if (isInvoiceInfoEnabled && doc.InvoiceDate != null)
					{
						OpenPeriodAttribute.SetValidatePeriod<SOOrder.finPeriodID>(cache, doc, PeriodValidation.DefaultSelectUpdate);
					}
					else
					{
						OpenPeriodAttribute.SetValidatePeriod<SOOrder.finPeriodID>(cache, doc, PeriodValidation.Nothing);
					}
					PXUIFieldAttribute.SetEnabled<SOOrder.dueDate>(cache, doc, isInvoiceInfoEnabled && soordertype.Current.ARDocType != ARDocType.CreditMemo);
					PXUIFieldAttribute.SetRequired<SOOrder.dueDate>(cache, doc.BillSeparately == true
						&& (soordertype.Current.ARDocType == ARDocType.CashSale || soordertype.Current.ARDocType == ARDocType.CashReturn || soordertype.Current.ARDocType == ARDocType.Invoice));
					PXUIFieldAttribute.SetEnabled<SOOrder.discDate>(cache, doc, isInvoiceInfoEnabled && soordertype.Current.ARDocType != ARDocType.CreditMemo);
					PXUIFieldAttribute.SetRequired<SOOrder.discDate>(cache, doc.BillSeparately == true
						&& (soordertype.Current.ARDocType == ARDocType.CashSale || soordertype.Current.ARDocType == ARDocType.CashReturn || soordertype.Current.ARDocType == ARDocType.Invoice));

					bool isInserted = cache.GetStatus(doc) == PXEntryStatus.Inserted;
				}
				cache.AllowUpdate = true;
				cache.AllowDelete = doc.Status != SOOrderStatus.BackOrder;

				Transactions.Cache.AllowDelete = true;
				Transactions.Cache.AllowUpdate = true;
				Transactions.Cache.AllowInsert = (doc.CustomerID != null && doc.CustomerLocationID != null && (doc.ProjectID != null || !PM.ProjectAttribute.IsPMVisible(BatchModule.SO)));
				PXUIFieldAttribute.SetEnabled<SOOrder.curyDiscTot>(cache, doc, !PXAccess.FeatureInstalled<FeaturesSet.customerDiscounts>());
				PXUIFieldAttribute.SetEnabled<SOOrder.curyPremiumFreightAmt>(cache, doc, isFreightEditable);

				Taxes.Cache.AllowUpdate = true;
				SalesPerTran.Cache.AllowUpdate = true;

				bool applicationsEnabled = (soordertype.Current.CanHavePayments == true
					|| soordertype.Current.CanHaveRefunds == true) && doc.Behavior != SOBehavior.MO;

				Adjustments.Cache.AllowInsert = IsAddingPaymentsAllowed(doc, soordertype.Current) && doc.Behavior != SOBehavior.MO;
				Adjustments.Cache.AllowDelete = applicationsEnabled;
				Adjustments.Cache.AllowUpdate = applicationsEnabled;

				DiscountDetails.Cache.AllowDelete = true;
				DiscountDetails.Cache.AllowUpdate = !DisableGroupDocDiscount;
				DiscountDetails.Cache.AllowInsert = !DisableGroupDocDiscount;
			}
			splits.Cache.AllowInsert = Transactions.Cache.AllowInsert;
			splits.Cache.AllowUpdate = Transactions.Cache.AllowUpdate;
			splits.Cache.AllowDelete = Transactions.Cache.AllowDelete;

			PXUIFieldAttribute.SetEnabled<SOOrder.orderType>(cache, doc);
			PXUIFieldAttribute.SetEnabled<SOOrder.orderNbr>(cache, doc);
			PXUIFieldAttribute.SetVisible<SOLine.invoiceType>(Transactions.Cache, null, doc.IsCreditMemoOrder == true || doc.IsRMAOrder == true || doc.IsMixedOrder == true);
			PXUIFieldAttribute.SetVisible<SOLine.invoiceNbr>(Transactions.Cache, null, doc.IsCreditMemoOrder == true || doc.IsRMAOrder == true || doc.IsMixedOrder == true);
			PXUIFieldAttribute.SetEnabled<SOLine.reasonCode>(Transactions.Cache, null, true);
			addInvoice.SetEnabled((doc.IsCreditMemoOrder == true || doc.IsRMAOrder == true || doc.IsMixedOrder == true) && Transactions.Cache.AllowInsert);

			Taxes.Cache.AllowDelete = Transactions.Cache.AllowDelete;
			Taxes.Cache.AllowInsert = Transactions.Cache.AllowInsert;

			PXNoteAttribute.SetTextFilesActivitiesRequired<SOLine.noteID>(Transactions.Cache, null);
			PXUIFieldAttribute.SetVisible<SOLine.branchID>(this.Transactions.Cache, null, doc.IsTransferOrder != true);
			PXUIFieldAttribute.SetEnabled<SOLine.branchID>(this.Transactions.Cache, null, doc.IsTransferOrder != true);
			PXUIFieldAttribute.SetVisible<SOLine.curyLineAmt>(this.Transactions.Cache, null, doc.IsTransferOrder != true);
			PXUIFieldAttribute.SetVisible<SOLine.curyUnitPrice>(this.Transactions.Cache, null, doc.IsTransferOrder != true);
			PXUIFieldAttribute.SetVisible<SOLine.curyExtCost>(this.Transactions.Cache, null, doc.IsTransferOrder != true);
			PXUIFieldAttribute.SetVisible<SOLine.curyExtPrice>(this.Transactions.Cache, null, doc.IsTransferOrder != true);
			PXUIFieldAttribute.SetVisible<SOLine.discPct>(this.Transactions.Cache, null, doc.IsTransferOrder != true);
			PXUIFieldAttribute.SetVisible<SOLine.discAmt>(this.Transactions.Cache, null, doc.IsTransferOrder != true);
			PXUIFieldAttribute.SetVisible<SOLine.curyDiscAmt>(this.Transactions.Cache, null, doc.IsTransferOrder != true);
			PXUIFieldAttribute.SetVisible<SOLine.curyDiscPrice>(this.Transactions.Cache, null, doc.IsTransferOrder != true);
			PXUIFieldAttribute.SetVisible<SOLine.manualDisc>(this.Transactions.Cache, null, doc.IsTransferOrder != true);
			PXUIFieldAttribute.SetVisible<SOLine.discountID>(this.Transactions.Cache, null, doc.IsTransferOrder != true);
			PXUIFieldAttribute.SetEnabled<SOLine.discountID>(this.Transactions.Cache, null, doc.IsTransferOrder != true);
			PXUIFieldAttribute.SetVisible<SOLine.curyUnbilledAmt>(this.Transactions.Cache, null, doc.IsTransferOrder != true);
			PXUIFieldAttribute.SetVisible<SOLine.salesPersonID>(this.Transactions.Cache, null, doc.IsTransferOrder != true);
			PXUIFieldAttribute.SetVisible<SOLine.taxCategoryID>(this.Transactions.Cache, null, doc.IsTransferOrder != true);
			PXUIFieldAttribute.SetVisible<SOLine.commissionable>(this.Transactions.Cache, null, doc.IsTransferOrder != true);

			if (soordertype.Current != null)
			{
				PXUIFieldAttribute.SetVisible<SOOrder.curyControlTotal>(cache, e.Row, soordertype.Current.RequireControlTotal == true);
			}

			if (soordertype.Current.RequireLocation == true)
			{
				PXStringListAttribute.SetList<SOLine.shipComplete>(Transactions.Cache, null, new string[] { SOShipComplete.CancelRemainder, SOShipComplete.ShipComplete }, new string[] { Messages.CancelRemainder, Messages.ShipComplete });
			}
			else
			{
				PXStringListAttribute.SetList<SOLine.shipComplete>(Transactions.Cache, null, new string[] { SOShipComplete.BackOrderAllowed, SOShipComplete.CancelRemainder, SOShipComplete.ShipComplete }, new string[] { Messages.BackOrderAllowed, Messages.CancelRemainder, Messages.ShipComplete });
			}
			PXUIFieldAttribute.SetVisible<SOOrder.destinationSiteID>(cache, e.Row, doc.IsTransferOrder == true);
			PXUIFieldAttribute.SetVisible<SOOrder.customerOrderNbr>(cache, e.Row, doc.IsTransferOrder != true);

			Packages.Cache.AllowInsert = ((SOOrder)e.Row).IsManualPackage == true;
			Packages.Cache.AllowDelete = ((SOOrder)e.Row).IsManualPackage == true;
			PXUIFieldAttribute.SetEnabled<SOPackageInfo.inventoryID>(Packages.Cache, null, ((SOOrder)e.Row).IsManualPackage == true);
			PXUIFieldAttribute.SetEnabled<SOPackageInfo.boxID>(Packages.Cache, null, ((SOOrder)e.Row).IsManualPackage == true);
			PXUIFieldAttribute.SetEnabled<SOPackageInfo.declaredValue>(Packages.Cache, null, ((SOOrder)e.Row).IsManualPackage == true);
			PXUIFieldAttribute.SetEnabled<SOPackageInfo.length>(Packages.Cache, null, ((SOOrder)e.Row).IsManualPackage == true);
			PXUIFieldAttribute.SetEnabled<SOPackageInfo.width>(Packages.Cache, null, ((SOOrder)e.Row).IsManualPackage == true);
			PXUIFieldAttribute.SetEnabled<SOPackageInfo.height>(Packages.Cache, null, ((SOOrder)e.Row).IsManualPackage == true);

			if (!string.IsNullOrEmpty(((SOOrder)e.Row).ShipVia))
			{
				Carrier shipVia = Carrier.PK.Find(this, ((SOOrder)e.Row).ShipVia);
				if (shipVia != null)
				{
					PXUIFieldAttribute.SetVisible<SOPackageInfo.declaredValue>(Packages.Cache, null, shipVia.PluginMethod != null);
					PXUIFieldAttribute.SetVisible<SOPackageInfo.cOD>(Packages.Cache, null, shipVia.PluginMethod != null);
					PXUIFieldAttribute.SetEnabled<SOOrder.curyFreightCost>(cache, doc, isFreightEditable && shipVia.CalcMethod == CarrierCalcMethod.Manual);
				}
			}
			cache.RaiseExceptionHandling<SOOrder.shipVia>(doc, doc.ShipVia, this.BuildShipViaException(doc));
			PXUIFieldAttribute.SetEnabled<SOOrder.taxZoneID>(cache, e.Row, ((SOOrder)e.Row).OverrideTaxZone == true ||
				this.IsContractBasedAPI); //TODO: BlanketSO: temporary solution for override tax zone checkbox

			if (!UnattendedMode)
			{
				this.validateAddresses.SetEnabled(doc.Completed == false && doc.Cancelled == false && FindAllImplementations<IAddressValidationHelper>().RequiresValidation());
			}

			PXUIFieldAttribute.SetVisible<SOOrder.groundCollect>(cache, doc, this.CanUseGroundCollect(doc));

			PXUIFieldAttribute.SetEnabled<SOOrder.customerID>(cache, doc, doc.IsTransferOrder != true);
			PXUIFieldAttribute.SetEnabled<SOOrder.customerLocationID>(cache, doc, doc.IsTransferOrder != true);
			PXUIFieldAttribute.SetEnabled<SOOrder.contactID>(cache, doc, doc.IsTransferOrder != true);
			PXUIFieldAttribute.SetEnabled<SOOrder.salesPersonID>(cache, doc, doc.IsTransferOrder != true);

			PXUIFieldAttribute.SetVisible<SOOrder.customerID>(cache, doc, doc.IsTransferOrder != true);
			PXUIFieldAttribute.SetVisible<SOOrder.customerLocationID>(cache, doc, doc.IsTransferOrder != true);
			PXUIFieldAttribute.SetVisible<SOOrder.contactID>(cache, doc, doc.IsTransferOrder != true);
			PXUIFieldAttribute.SetVisible<SOOrder.curyVatExemptTotal>(cache, doc, doc.IsTransferOrder != true);
			PXUIFieldAttribute.SetVisible<SOOrder.curyVatTaxableTotal>(cache, doc, doc.IsTransferOrder != true);
			PXUIFieldAttribute.SetVisible<SOOrder.curyTaxTotal>(cache, doc, doc.IsTransferOrder != true);
			PXUIFieldAttribute.SetVisible<SOOrder.curyOrderTotal>(cache, doc, doc.IsTransferOrder != true);

			PXUIFieldAttribute.SetVisible<SOOrder.taxZoneID>(cache, doc, doc.IsTransferOrder != true);
			PXUIFieldAttribute.SetVisible<SOOrder.overrideTaxZone>(cache, doc, doc.IsTransferOrder != true);
			PXUIFieldAttribute.SetVisible<SOOrder.externalTaxExemptionNumber>(cache, doc, doc.IsTransferOrder != true);
			PXUIFieldAttribute.SetVisible<SOOrder.avalaraCustomerUsageType>(cache, doc, doc.IsTransferOrder != true);
			PXUIFieldAttribute.SetVisible<SOOrder.billSeparately>(cache, doc, doc.IsTransferOrder != true);
			PXUIFieldAttribute.SetVisible<SOOrder.invoiceNbr>(cache, doc, doc.IsTransferOrder != true);
			PXUIFieldAttribute.SetVisible<SOOrder.invoiceDate>(cache, doc, doc.IsTransferOrder != true);
			PXUIFieldAttribute.SetVisible<SOOrder.termsID>(cache, doc, doc.IsTransferOrder != true);
			PXUIFieldAttribute.SetVisible<SOOrder.dueDate>(cache, doc, doc.IsTransferOrder != true);
			PXUIFieldAttribute.SetVisible<SOOrder.discDate>(cache, doc, doc.IsTransferOrder != true);
			PXUIFieldAttribute.SetVisible<SOOrder.finPeriodID>(cache, doc, doc.IsTransferOrder != true);
			PXUIFieldAttribute.SetVisible<SOOrder.salesPersonID>(cache, doc, doc.IsTransferOrder != true);

			PXUIFieldAttribute.SetVisible<SOOrder.curyLineTotal>(cache, doc, doc.IsTransferOrder != true);
			PXUIFieldAttribute.SetVisible<SOOrder.curyGoodsExtPriceTotal>(cache, doc, doc.IsTransferOrder != true);
			PXUIFieldAttribute.SetVisible<SOOrder.curyMiscTot>(cache, doc, doc.IsTransferOrder != true);
			PXUIFieldAttribute.SetVisible<SOOrder.curyMiscExtPriceTotal>(cache, doc, doc.IsTransferOrder != true);
			PXUIFieldAttribute.SetVisible<SOOrder.curyFreightCost>(cache, doc, doc.IsFreightAvailable == true);
			PXUIFieldAttribute.SetVisible<SOOrder.freightCostIsValid>(cache, doc, doc.IsFreightAvailable == true);
			PXUIFieldAttribute.SetVisible<SOOrder.overrideFreightAmount>(cache, doc, doc.IsFreightAvailable == true);
			PXUIFieldAttribute.SetVisible<SOOrder.freightAmountSource>(cache, doc, doc.IsFreightAvailable == true);
			PXUIFieldAttribute.SetVisible<SOOrder.curyFreightAmt>(cache, doc, doc.IsFreightAvailable == true);
			PXUIFieldAttribute.SetVisible<SOOrder.curyPremiumFreightAmt>(cache, doc, doc.IsFreightAvailable == true);
			PXUIFieldAttribute.SetVisible<SOOrder.freightTaxCategoryID>(cache, doc, doc.IsFreightAvailable == true);
			PXUIFieldAttribute.SetVisible<SOOrder.curyOpenOrderTotal>(cache, doc, doc.IsTransferOrder != true);
			PXUIFieldAttribute.SetVisible<SOOrder.unbilledOrderQty>(cache, doc, doc.IsTransferOrder != true);
			PXUIFieldAttribute.SetVisible<SOOrder.curyUnbilledOrderTotal>(cache, doc, doc.IsTransferOrder != true);
			PXUIFieldAttribute.SetVisible<SOOrder.curyUnreleasedPaymentAmt>(cache, doc, doc.IsTransferOrder != true && doc.IsCashSaleOrder != true);
			PXUIFieldAttribute.SetVisible<SOOrder.curyCCAuthorizedAmt>(cache, doc, doc.IsTransferOrder != true && doc.IsCashSaleOrder != true
				&& soordertype.Current.CanHaveRefunds != true);
			PXUIFieldAttribute.SetVisible<SOOrder.curyPaidAmt>(cache, doc, doc.IsTransferOrder != true && doc.IsCashSaleOrder != true);
			PXUIFieldAttribute.SetVisible<SOOrder.curyBilledPaymentTotal>(cache, doc, doc.IsTransferOrder != true && doc.IsCashSaleOrder != true);
			PXUIFieldAttribute.SetVisible<SOOrder.curyPaymentTotal>(cache, doc, doc.IsTransferOrder != true);
			PXUIFieldAttribute.SetVisible<SOOrder.curyUnpaidBalance>(cache, doc, doc.IsTransferOrder != true);
			PXUIFieldAttribute.SetVisible<SOOrder.disableAutomaticTaxCalculation>(cache, doc, doc.IsTransferOrder != true);
			PXUIFieldAttribute.SetVisible<SOOrder.curyLineDiscTotal>(cache, doc, doc.IsTransferOrder != true);
			PXUIFieldAttribute.SetVisible<SOOrder.curyDiscTot>(cache, doc, doc.IsTransferOrder != true);
			PXUIFieldAttribute.SetVisible<SOOrder.curyFreightTot>(cache, doc, doc.IsTransferOrder != true);
			PXUIFieldAttribute.SetVisible<SOOrder.curyDetailExtPriceTotal>(cache, doc, doc.IsTransferOrder != true);


			this.calculateFreight.SetVisible(doc.IsFreightAvailable == true);

			Taxes.View.AllowSelect = doc.IsTransferOrder != true;
			DiscountDetails.View.AllowSelect = doc.IsTransferOrder != true;
			Adjustments.View.AllowSelect = doc.IsTransferOrder != true && doc.IsCashSaleOrder != true;

			SalesPerTran.AllowSelect = doc.IsTransferOrder != true;
			SalesPerTran.AllowInsert = doc.IsTransferOrder != true;

			PXUIFieldAttribute.SetVisible<SOOrder.paymentMethodID>(cache, doc, doc.IsTransferOrder != true);
			PXUIFieldAttribute.SetVisible<SOOrder.pMInstanceID>(cache, doc, doc.IsTransferOrder != true);
			PXUIFieldAttribute.SetVisible<SOOrder.cashAccountID>(cache, doc, doc.IsTransferOrder != true);
			PXUIFieldAttribute.SetVisible<SOOrder.extRefNbr>(cache, doc, doc.IsTransferOrder != true && doc.IsInvoiceOrder == true && doc.IsCashSaleOrder == true);

			PXUIFieldAttribute.SetRequired<SOOrder.termsID>(cache, (doc.ARDocType != ARDocType.Undefined
				&& doc.ARDocType != ARDocType.CreditMemo));

			Billing_Contact.View.AllowSelect = doc.IsTransferOrder != true;
			Billing_Address.View.AllowSelect = doc.IsTransferOrder != true;

			PXUIFieldAttribute.SetVisible<SOOrder.approved>(cache, doc, doc.DontApprove != true);
			Approval.AllowSelect = doc.DontApprove != true;
			if (doc.Hold != true && doc.DontApprove != true)
			{
				PXUIFieldAttribute.SetEnabled(cache, doc, false);
				PXUIFieldAttribute.SetEnabled<SOOrder.orderType>(cache, doc, true);
				PXUIFieldAttribute.SetEnabled<SOOrder.orderNbr>(cache, doc, true);

				Transactions.Cache.AllowDelete = false;
				Transactions.Cache.AllowUpdate = false;
				Transactions.Cache.AllowInsert = false;

				splits.Cache.AllowDelete = false;
				splits.Cache.AllowUpdate = false;
				splits.Cache.AllowInsert = false;

				Adjustments.Cache.AllowInsert = false;
				Adjustments.Cache.AllowUpdate = false;
				Adjustments.Cache.AllowDelete = false;

				DiscountDetails.Cache.AllowDelete = false;
				DiscountDetails.Cache.AllowUpdate = false;
				DiscountDetails.Cache.AllowInsert = false;

				Taxes.Cache.AllowInsert = false;
				Taxes.Cache.AllowUpdate = false;
				Taxes.Cache.AllowDelete = false;

				SalesPerTran.Cache.AllowUpdate = false;
			}

			if (doc.DisableAutomaticTaxCalculation == true && (doc.BilledCntr > 0 || doc.ReleasedCntr > 0))
			{
				Taxes.Cache.AllowInsert = false;
				Taxes.Cache.AllowUpdate = false;
				Taxes.Cache.AllowDelete = false;
			}

			addInvBySite.SetEnabled(Transactions.Cache.AllowInsert);

			if (soordertype.Current == null || soordertype.Current.Active != true)
			{
				SetReadOnly(true);
				cache.RaiseExceptionHandling<SOOrder.orderType>(doc, doc.OrderType, new PXSetPropertyException(Messages.OrderTypeInactive, PXErrorLevel.Warning));
			}

			if (!PXPreserveScope.IsScoped() && PXLongOperation.GetStatus(this.UID) == PXLongRunStatus.InProcess && !IsImportFromExcel)
			{
				SetReadOnly(true);
			}

			if (doc?.IsTransferOrder == true)
			{
				var destinationSiteIdErrorString = PXUIFieldAttribute.GetError<SOOrder.destinationSiteID>(cache, e.Row);
				if (destinationSiteIdErrorString == null)
				{
					var destinationSiteIdErrorMessage = doc.DestinationSiteIdErrorMessage;
					if (!string.IsNullOrWhiteSpace(destinationSiteIdErrorMessage))
					{
						if (UnattendedMode)
							throw new PXSetPropertyException(destinationSiteIdErrorMessage, PXErrorLevel.Error);
						else
							cache.RaiseExceptionHandling<POOrder.siteID>(e.Row, cache.GetValueExt<POOrder.siteID>(e.Row), new PXSetPropertyException(destinationSiteIdErrorMessage, PXErrorLevel.Error));
					}
				}
			}
			PXUIFieldAttribute.SetVisible<SOOrder.emailed>(cache, doc, !new[] { SOBehavior.RM, SOBehavior.CM }.Contains(doc.Behavior));

			if (doc.IsCashSaleOrder == true && doc.PaymentMethodID != null)
			{
				PaymentMethod pm = PaymentMethodDef.Select(doc.PaymentMethodID);
				bool ccPayment = pm?.PaymentType == PaymentMethodType.CreditCard && pm.IsAccountNumberRequired == true;
				AR.PaymentRefAttribute.SetAllowAskUpdateLastRefNbr<SOOrder.extRefNbr>(cache, !ccPayment);
			}
			else
			{
				AR.PaymentRefAttribute.SetAllowAskUpdateLastRefNbr<SOOrder.extRefNbr>(cache, false);
			}

			bool createShipmentEnabled = doc.OpenSiteCntr > 0;
			createShipmentIssue.SetEnabled(createShipmentEnabled);
			createShipmentReceipt.SetEnabled(createShipmentEnabled);
		}

		public virtual bool IsAddingPaymentsAllowed(SOOrder order, SOOrderType orderType)
		{
			return orderType?.CanHavePayments == true
				|| orderType?.CanHaveRefunds == true && orderType.AllowRefundBeforeReturn == true;
		}

		protected virtual bool IsCurrencyEnabled(SOOrder order)
		{
			return (customer.Current == null || customer.Current.AllowOverrideCury == true);
		}

		protected virtual bool IsCuryUnitCostEnabled(SOLine line, SOOrder order)
		{
			return order.IsRMAOrder == true || order.IsCreditMemoOrder == true || order.IsMixedOrder == true;
		}

		protected virtual bool IsCuryUnitCostVisible(SOOrder order)
		{
			return order.IsRMAOrder == true || order.IsCreditMemoOrder == true || order.IsMixedOrder == true;
		}

		protected virtual PXException BuildShipViaException(SOOrder order)
		{
			if (string.IsNullOrEmpty(order.ShipVia))
				return null;

			var shipVia = Carrier.PK.Find(this, order.ShipVia);
			if (shipVia?.IsExternal != true)
				return null;

			var plugin = CarrierPlugin.PK.Find(this, shipVia.CarrierPluginID);
			if (plugin?.SiteID == null)
				return null;

			return SiteList.Select().RowCast<SOOrderSite>().All(s => s.SiteID == plugin.SiteID)
				? null
				: new PXSetPropertyException(Messages.ShipViaNotApplicableToOrder, PXErrorLevel.Warning);
		}

		protected virtual bool AllowChangingOverrideFreightAmount(SOOrder doc)
		{
			// need to prevent changing Freight Amount Source if a shipment exists
			ShipTerms terms = ShipTerms.PK.Find(this, doc.ShipTermsID);
			return terms == null && doc.ShipmentCntr <= 0 || terms?.FreightAmountSource == FreightAmountSourceAttribute.OrderBased;
		}

		protected virtual void SOOrder_RowDeleting(PXCache sender, PXRowDeletingEventArgs e)
		{
			SOOrder order = (SOOrder)e.Row;
			SOOrderShipment orderShipment = PXSelectReadonly<SOOrderShipment,
				Where<SOOrderShipment.orderType, Equal<Current<SOOrder.orderType>>,
					And<SOOrderShipment.orderNbr, Equal<Current<SOOrder.orderNbr>>>>>
				.SelectSingleBound(this, new object[] { order });
			if (orderShipment != null)
			{
				throw new PXException(Messages.ShippedSOOrderCannotBeDeleted);
		}

			if (this.Adjustments.Select().Count > 0 && Document.Ask(Messages.Warning, Messages.SalesOrderWillBeDeleted, MessageButtons.OKCancel) != WebDialogResult.OK)
			{
				e.Cancel = true;
			}
		}

		protected virtual void _(Events.RowDeleted<SOOrder> e)
		{
			if (e.Row != null)
				SOOrder.Events.Select(ev => ev.OrderDeleted).FireOn(this, e.Row);
		}

		protected virtual void RQRequisitionOrder_RowDeleted(PXCache sender, PXRowDeletedEventArgs e)
		{
			var reqord = (RQ.RQRequisitionOrder)e.Row;
			SOOrderType ordtype = soordertype.SelectWindowed(0, 1, reqord.OrderType);
			if (ordtype.Behavior == SOBehavior.QT)
			{
				var req = (RQ.RQRequisition)PXParentAttribute.SelectParent(sender, reqord, typeof(RQ.RQRequisition));
				req.Quoted = false;
				rqrequisition.Cache.MarkUpdated(req, assertError: true);
			}
		}

		public virtual void UpdateControlTotal(PXCache sender, PXRowUpdatedEventArgs e)
		{
			SOOrder row = e.Row as SOOrder;

			if (row.Completed == false)
			{
				if (soordertype.Current.RequireControlTotal == false)
				{
					if (row.CuryOrderTotal != row.CuryControlTotal)
					{
						if (row.CuryOrderTotal != null && row.CuryOrderTotal != 0)
							sender.SetValueExt<SOOrder.curyControlTotal>(e.Row, row.CuryOrderTotal);
						else
							sender.SetValueExt<SOOrder.curyControlTotal>(e.Row, 0m);
					}
				}
			}
		}

		public virtual void ValidateControlTotal(PXCache sender, PXRowSelectedEventArgs e)
		{
			SOOrder row = e.Row as SOOrder;
			if (row == null) return;

			bool isex = false;

			if (row.Hold == false && row.Completed == false && row.Cancelled == false)
			{
				if (soordertype.Current.RequireControlTotal == true)
				{
					if (row.CuryOrderTotal != row.CuryControlTotal)
					{
						sender.RaiseExceptionHandling<SOOrder.curyControlTotal>(e.Row, row.CuryControlTotal, new PXSetPropertyException(Messages.DocumentOutOfBalance));
						isex = true;
					}
					else if (row.CuryOrderTotal < 0m && row.ARDocType != ARDocType.NoUpdate && row.Behavior.IsNotIn(SOBehavior.RM, SOBehavior.MO))
					{
						sender.RaiseExceptionHandling<SOOrder.curyControlTotal>(e.Row, row.CuryControlTotal, new PXSetPropertyException(Messages.DocumentBalanceNegative));
						isex = true;
					}
				}
				else
				{
					if (row.CuryOrderTotal < 0m && row.ARDocType != ARDocType.NoUpdate && row.Behavior.IsNotIn(SOBehavior.RM, SOBehavior.MO))
					{
						sender.RaiseExceptionHandling<SOOrder.curyOrderTotal>(e.Row, row.CuryOrderTotal, new PXSetPropertyException(Messages.DocumentBalanceNegative));
						isex = true;
					}
				}
			}

			if (!isex)
			{
				if (soordertype.Current.RequireControlTotal == true)
				{
					sender.RaiseExceptionHandling<SOOrder.curyControlTotal>(e.Row, null, null);
				}
				else
				{
					sender.RaiseExceptionHandling<SOOrder.curyOrderTotal>(e.Row, null, null);
				}
			}
		}

		protected virtual void SOOrder_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			SOOrder row = e.Row as SOOrder;
			SOOrder oldRow = e.OldRow as SOOrder;
			if (row == null) return;

			if (!sender.ObjectsEqual<SOOrder.hold>(oldRow, row))
			{
				Transactions.Cache.ClearItemAttributes();
				//needed to reset field states to set them correctly afterwards - it helps to avoid unwanted effects in UI behavior when 
				//using PXUIFieldAttribute.SetEnabled(sender, row, false) or similar statements in RowSelected.
			}

			if (row.DeferPriceDiscountRecalculation == true && !sender.ObjectsEqual<SOOrder.orderDate, SOOrder.taxZoneID>(oldRow, row))
			{
				row.IsPriceAndDiscountsValid = false;
			}

			if (!sender.ObjectsEqual<SOOrder.shipVia>(e.OldRow, e.Row))
			{
				if (oldRow.ShipVia != null && oldRow.ShipVia == row.ShipVia || row.IsManualPackage == true)
				{
					// do not delete packages
				}
				else
				{
					//autopackaging
					if (string.IsNullOrEmpty(row.ShipVia))
					{
						foreach (SOPackageInfoEx package in Packages.Select())
						{
							Packages.Delete(package);
						}
						row.PackageWeight = 0;
					}
					else
					{
						CarrierRatesExt.RecalculatePackagesForOrder(Document.Current);
					}
				}
			}

			if (e.ExternalCall && (!sender.ObjectsEqual<SOOrder.disableAutomaticDiscountCalculation>(e.OldRow, e.Row)) && row.DisableAutomaticDiscountCalculation == true)
			{
				foreach (SOOrderDiscountDetail discountDetail in DiscountDetails.Select())
				{
					discountDetail.IsManual = true;
					DiscountDetails.Update(discountDetail);
				}

				foreach (SOLine line in Transactions.Select())
				{
					if (line.IsFree != true && line.LineType != SOLineType.Discount)
					{
						Transactions.Cache.SetValueExt<SOLine.manualDisc>(line, true);
						Transactions.Cache.MarkUpdated(line);
					}
				}

				Document.Cache.RaiseExceptionHandling<SOOrder.disableAutomaticDiscountCalculation>(row, null,
							new PXSetPropertyException(Messages.ManualDiscountFlagSetOnAllLines, PXErrorLevel.Warning));
			}

			bool recalcTotalDiscount = false;
			if (!sender.ObjectsEqual<SOOrder.customerLocationID, SOOrder.orderDate>(e.OldRow, e.Row))
			{
				//Acuminator disable once PX1045 PXGraphCreateInstanceInEventHandlers 
				using (GetPriceCalculationScope().AppendContext<SOOrder.customerLocationID, SOOrder.orderDate>())
					_discountEngine.AutoRecalculatePricesAndDiscounts(Transactions.Cache, Transactions, null, DiscountDetails, row.CustomerLocationID, row.OrderDate, GetDefaultSODiscountCalculationOptions(Document.Current));
				recalcTotalDiscount = true;
			}

			//Acuminator disable once PX1045 PXGraphCreateInstanceInEventHandlers 
			if (!_discountEngine.IsInternalDiscountEngineCall && e.ExternalCall && sender.GetStatus(row) != PXEntryStatus.Deleted && !sender.ObjectsEqual<SOOrder.curyDiscTot>(e.OldRow, e.Row))
			{
				//Acuminator disable once PX1045 PXGraphCreateInstanceInEventHandlers 
				_discountEngine.SetTotalDocDiscount(Transactions.Cache, Transactions, DiscountDetails,
					Document.Current.CuryDiscTot, DiscountEngine.DiscountCalculationOptions.DisableAPDiscountsCalculation);
				recalcTotalDiscount = true;	
			}

			if(recalcTotalDiscount == true)
				RecalculateTotalDiscount();

			if (row.CustomerID != null && row.CuryDiscTot != null && row.CuryDiscTot > 0 && row.CuryLineTotal != null && (row.CuryLineTotal > 0 || row.CuryMiscTot > 0))
			{
				//Acuminator disable once PX1045 PXGraphCreateInstanceInEventHandlers 
				decimal discountLimit = _discountEngine.GetDiscountLimit(sender, row.CustomerID);
				if (((row.CuryLineTotal + row.CuryMiscTot) / 100 * discountLimit) < (row.CuryDiscTot))
				{
					PXUIFieldAttribute.SetWarning<SOOrder.curyDiscTot>(sender, row, PXMessages.LocalizeFormatNoPrefix(AR.Messages.DocDiscountExceedLimit, discountLimit));
				}
			}

			if (!sender.ObjectsEqual<SOOrder.lineTotal, SOOrder.orderWeight, SOOrder.packageWeight, SOOrder.orderVolume, SOOrder.shipTermsID, SOOrder.shipZoneID, SOOrder.shipVia, SOOrder.useCustomerAccount>(e.OldRow, e.Row)
				|| !sender.ObjectsEqual<SOOrder.curyFreightCost, SOOrder.overrideFreightAmount>(e.OldRow, e.Row))
			{
				Carrier carrier = Carrier.PK.Find(sender.Graph, row.ShipVia);
				if (!sender.ObjectsEqual<SOOrder.shipVia>(e.OldRow, e.Row) && carrier?.CalcMethod == CarrierCalcMethod.Manual && !IsCopyOrder)
				{
					row.FreightCost = 0m;
				}
					if (!IsImportFromExcel)
				{
					CalcFreight(row);
				}
			}

			if (row.IsManualPackage != true &&
				(row.OrderWeight != oldRow.OrderWeight ||
				(row.ShipVia != oldRow.ShipVia && !string.IsNullOrEmpty(oldRow.ShipVia))))
			{
				sender.SetValue<SOOrder.isPackageValid>(row, false);
			}

			if (!sender.ObjectsEqual<SOOrder.curyFreightTot, SOOrder.curyUnbilledFreightTot, SOOrder.freightTaxCategoryID>(e.OldRow, e.Row))
			{
				SOOrderTaxAttribute.Calculate<SOOrder.freightTaxCategoryID>(sender, e);
			}
			if (!sender.ObjectsEqual<SOOrder.hold>(e.Row, e.OldRow) && row.Hold != true)
			{
				if (soordertype.Current.RequireShipping == true && soordertype.Current.ARDocType != ARDocType.NoUpdate)
					foreach (SOLine line in Transactions.Select())
					{
						if ((line.SalesAcctID == null || line.SalesSubID == null))
							Transactions.Cache.MarkUpdated(line, assertError: true);

						PXDefaultAttribute.SetPersistingCheck<SOLine.salesAcctID>(Transactions.Cache, line, PXPersistingCheck.NullOrBlank);

						PXDefaultAttribute.SetPersistingCheck<SOLine.salesSubID>(Transactions.Cache, line, PXPersistingCheck.NullOrBlank);
					}
			}

			UpdateControlTotal(sender, e);

			// If CustomerLocationID changed SOOrder_RowUpdated will be recursively called from DiscountEngine.
			UpdateCustomerBalances(sender, row, oldRow);

			if (!sender.ObjectsEqual<SOOrder.completed, SOOrder.cancelled>(e.Row, e.OldRow) && (row.Completed == true && row.BilledCntr == 0 && row.ShipmentCntr <= row.BilledCntr + row.ReleasedCntr || row.Cancelled == true))
			{
				foreach (SOAdjust adj in Adjustments_Raw.Select())
				{
					SOAdjust copy = PXCache<SOAdjust>.CreateCopy(adj);
					copy.CuryAdjdAmt = 0m;
					copy.CuryAdjgAmt = 0m;
					copy.AdjAmt = 0m;
					Adjustments.Update(copy);
				}
			}
		}

		protected virtual void UpdateCustomerBalances(PXCache cache, SOOrder row, SOOrder oldRow)
		{
		}

		protected virtual void CalcFreight(SOOrder row)
		{
			if (soordertype.Current?.CalculateFreight != false)
			{
				Carrier carrier = Carrier.PK.Find(this, row.ShipVia);
				FreightCalculator freightCalculator = CreateFreightCalculator();

				CalcFreightCost(row, carrier, freightCalculator);
				ApplyFreightTerms(row, carrier, freightCalculator);
			}
		}

		protected virtual void CalcFreightCost(SOOrder row, Carrier carrier, FreightCalculator freightCalculator)
				{
			if (carrier?.IsExternal != true)
			{
				freightCalculator.CalcFreightCost<SOOrder, SOOrder.curyFreightCost>(Document.Cache, row);
				row.FreightCostIsValid = true;
			}
			else
			{
				row.FreightCostIsValid = false;
			}
				}

		protected virtual void ApplyFreightTerms(SOOrder row, Carrier carrier, FreightCalculator freightCalculator)
		{
			if (row.OverrideFreightAmount == true)
				return;

			if (carrier?.IsExternal != true || freightCalculator.IsFlatRate(Document.Cache, row))
			{
				freightCalculator.ApplyFreightTerms<SOOrder, SOOrder.curyFreightAmt>(Document.Cache, row, new Lazy<int?>(() => Transactions.Select().Count));
			}
		}

		protected virtual void SOOrder_RowInserted(PXCache sender, PXRowInsertedEventArgs e)
		{
			SOOrder row = (SOOrder)e.Row;
			if (row == null) return;

			if (!PXAccess.FeatureInstalled<FeaturesSet.autoPackaging>())
			{
				row.IsManualPackage = true;
			}
		}

		protected virtual void SOOrder_TermsID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			Terms terms = (Terms)PXSelectorAttribute.Select<SOOrder.termsID>(sender, e.Row);

			if (terms != null && terms.InstallmentType != TermsInstallmentType.Single)
			{
				PXResultset<SOAdjust> adjustments = Adjustments.Select();
				if (adjustments.Count > 0)
				{
					PXUIFieldAttribute.SetWarning<SOOrder.termsID>(sender, e.Row, Messages.TermsChangedToMultipleInstallment);
					foreach (SOAdjust adj in adjustments)
					{
						Adjustments.Cache.Delete(adj);
					}
				}
			}
		}

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXRestrictor(typeof(Where<Current<SOOrder.shipmentCntr>, Equal<int0>, And<Current<SOOrder.overrideFreightAmount>, Equal<False>,
			Or<ShipTerms.freightAmountSource, Equal<Current<SOOrder.freightAmountSource>>>>>),
			Messages.CantSelectShipTermsWithFreightAmountSource, typeof(ShipTerms.freightAmountSource))]
		protected virtual void SOOrder_ShipTermsID_CacheAttached(PXCache sender)
		{
		}

		protected virtual void SOOrder_CuryFreightAmt_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			ShowWarningIfPartiallyInvoiced<SOOrder.curyFreightAmt>(sender, (SOOrder)e.Row);
		}

		protected virtual void SOOrder_CuryPremiumFreightAmt_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			ShowWarningIfPartiallyInvoiced<SOOrder.curyPremiumFreightAmt>(sender, (SOOrder)e.Row);
		}

		protected virtual void ShowWarningIfPartiallyInvoiced<amtField>(PXCache sender, SOOrder doc)
			where amtField : IBqlField
		{
			if (!this.UnattendedMode
				&& sosetup.Current.FreightAllocation == FreightAllocationList.FullAmount
				&& doc?.BilledCntr + doc?.ReleasedCntr > 0)
			{
				sender.RaiseExceptionHandling<amtField>(
					doc, sender.GetValue<amtField>(doc),
					new PXSetPropertyException(Messages.PleaseAdjustManuallyInInvoice, PXErrorLevel.Warning,
						PXUIFieldAttribute.GetDisplayName<amtField>(sender)));
			}
		}
		
		#endregion

		#region SOLine events

		public object GetValue<Field>(object data)
			where Field : IBqlField
		{
			return this.Caches[BqlCommand.GetItemType(typeof(Field))].GetValue(data, typeof(Field).Name);
		}

		[PXBool]
		[DRTerms.Dates(typeof(SOLine.dRTermStartDate), typeof(SOLine.dRTermEndDate), typeof(SOLine.inventoryID), VerifyDatesPresent = false)]
		protected virtual void SOLine_ItemRequiresTerms_CacheAttached(PXCache sender) { }

		protected virtual void SOLine_SiteID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (Document.Current != null && Document.Current.DefaultSiteID != null)
			{
				e.NewValue = Document.Current.DefaultSiteID;
				e.Cancel = true;
				return;
			}

			SOLine line = (SOLine)e.Row;
			if (line == null) return;

			Branch branch = Branch.PK.Find(this, line.BranchID);
			InventoryItemCurySettings itemCurySettings = InventoryItemCurySettings.PK.Find(this, line.InventoryID, branch?.BaseCuryID);
			InventoryItem item = InventoryItem.PK.Find(this, line.InventoryID);

			if (item != null && item.StkItem != true)
			{
				e.NewValue = itemCurySettings?.DfltSiteID;
				e.Cancel = true;
			}
		}

		protected virtual void SOLine_InventoryID_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			SOLine row = (SOLine)e.Row;
			if (row == null) return;

			object oldValue = sender.GetValue<SOLine.inventoryID>(row);
			if (oldValue != null)
			{
				if (row.InvoiceNbr != null)
				{
					e.NewValue = oldValue;
				}
				else
				{
					foreach (SOLineSplit split in splits.Select())
					{
						if (split.Completed == true)
						{
							e.NewValue = oldValue;
							sender.RaiseExceptionHandling<SOLine.inventoryID>(row, oldValue, new PXSetPropertyException(PXMessages.LocalizeFormatNoPrefixNLA(Messages.InventoryIDCannotBeChanged, split.SplitLineNbr.ToString()), PXErrorLevel.Warning));
						}
					}
				}
			}

			if(row.Operation != SOOperation.Receipt && e.NewValue != oldValue)
			{
				var item = InventoryItem.PK.Find(this, (int?)e.NewValue);

				if(item != null && item.StkItem == false && item.KitItem == true)
				{
					var stockDet = new SelectFrom<INKitSpecStkDet>.InnerJoin<InventoryItem>.On<InventoryItem.inventoryID.IsEqual<INKitSpecStkDet.compInventoryID>>
								.Where<INKitSpecStkDet.kitInventoryID.IsEqual<Data.BQL.@P.AsInt>
									.And<InventoryItem.itemStatus.IsEqual<InventoryItemStatus.noSales>>>.View.ReadOnly(this);

					var nonStockDet = new SelectFrom<INKitSpecNonStkDet>.InnerJoin<InventoryItem>.On<InventoryItem.inventoryID.IsEqual<INKitSpecNonStkDet.compInventoryID>>
								.Where<INKitSpecNonStkDet.kitInventoryID.IsEqual<Data.BQL.@P.AsInt>
									.And<InventoryItem.itemStatus.IsEqual<InventoryItemStatus.noSales>>>.View.ReadOnly(this);

					// Acuminator disable once PX1015 IncorrectNumberOfSelectParameters [False positive]
					if (stockDet.SelectWindowed(0, 1, item.InventoryID).Count != 0 || nonStockDet.SelectWindowed(0, 1, item.InventoryID).Count != 0)
					{
						var ex = new PXSetPropertyException<SOLine.inventoryID>(Messages.NonStockKitWithNoSalesComponent);
						ex.ErrorValue = item.InventoryCD;

						throw ex;
					}
				}
			}
		}

		protected virtual void SOLine_SalesAcctID_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			SOLine row = e.Row as SOLine;
			if (row != null && row.ProjectID != null && !PM.ProjectDefaultAttribute.IsNonProject(row.ProjectID) && row.TaskID != null)
			{
				Account account = PXSelect<Account, Where<Account.accountID, Equal<Required<Account.accountID>>>>.Select(this, e.NewValue);
				if (account != null && account.AccountGroupID == null)
				{
					sender.RaiseExceptionHandling<SOLine.salesAcctID>(e.Row, account.AccountCD, new PXSetPropertyException(PM.Messages.NoAccountGroup, PXErrorLevel.Warning, account.AccountCD));
				}
			}
		}


		protected virtual void SOLine_SalesAcctID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			SOLine row = e.Row as SOLine;
			if (row != null
				&& row.TaskID == null
				&& !IsCopyPasteContext)
			{
				sender.SetDefaultExt<SOLine.taskID>(e.Row);
			}
		}

		protected virtual void SOLine_SubItemID_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			SOLine line = (SOLine)e.Row;
			if (line != null && line.InvoiceNbr != null)
			{
				object oldValue = sender.GetValue<SOLine.subItemID>(e.Row);
				if (oldValue != null)
				{
					e.NewValue = oldValue;
				}
			}
		}

		protected virtual void SOLine_SubItemID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			SOLine line = (SOLine)e.Row;
			if (line != null && line.InvoiceNbr != null)
			{
				e.Cancel = true;
			}
		}

		protected virtual void SOLine_UOM_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (((SOLine)e.Row)?.InvoiceNbr != null)
			{
				e.NewValue = null;
				e.Cancel = true;
			}
		}

		protected virtual void SOLine_UOM_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			SOLine line = (SOLine)e.Row;
			if (line == null) return;

			if (line.InvoiceNbr != null)
			{
				object oldValue = sender.GetValue<SOLine.uOM>(line);
				if (oldValue != null)
				{
					var inventory = InventoryItem.PK.Find(this, line.InventoryID);
					if (inventory != null && line.InvoiceUOM != null)
					{
						var unit = INUnit.UK.ByInventory.Find(this, inventory.InventoryID, (string)e.NewValue);
						if (unit != null)
						{
							if (!unit.FromUnit.IsIn(line.InvoiceUOM, unit.ToUnit))
								throw new PXSetPropertyException(Messages.InvalidInvoiceUOM);

							return;
						}
					}
					e.NewValue = oldValue;
				}
			}
		}

		protected virtual void SOLine_SalesAcctID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			SOLine line = (SOLine)e.Row;

			if (line != null && Document.Current?.IsTransferOrder == false)
			{
				InventoryItem item = InventoryItem.PK.Find(this, line.InventoryID);
				if (item == null)
					return;

				switch (soordertype.Current.SalesAcctDefault)
				{
					case SOSalesAcctSubDefault.MaskItem:
						e.NewValue = GetValue<InventoryItem.salesAcctID>(item);
						e.Cancel = true;
						break;
					case SOSalesAcctSubDefault.MaskSite:
						INSite site = INSite.PK.Find(this, line.SiteID);
						if (site != null)
						{
							e.NewValue = GetValue<INSite.salesAcctID>(site);
							e.Cancel = true;
						}
						break;
					case SOSalesAcctSubDefault.MaskClass:
						INPostClass postclass = INPostClass.PK.Find(this, item.PostClassID) ?? new INPostClass();
						e.NewValue = GetValue<INPostClass.salesAcctID>(postclass);
						e.Cancel = true;
						break;
					case SOSalesAcctSubDefault.MaskLocation:
						Location customerloc = location.Current;
						e.NewValue = GetValue<Location.cSalesAcctID>(customerloc);
						e.Cancel = true;
						break;
					case SOSalesAcctSubDefault.MaskReasonCode:
						ReasonCode reasoncode = ReasonCode.PK.Find(this, line.ReasonCode);
						e.NewValue = GetValue<ReasonCode.salesAcctID>(reasoncode);
						e.Cancel = true;
						break;
				}
			}
		}

		protected virtual void SOLine_SalesSubID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			SOLine line = (SOLine)e.Row;

			if (line != null && Document.Current?.IsTransferOrder == false && line.SalesAcctID != null)
			{
				InventoryItem item = InventoryItem.PK.Find(this, line.InventoryID);
				INSite site = INSite.PK.Find(this, line.SiteID);
				ReasonCode reasoncode = ReasonCode.PK.Find(this, line.ReasonCode);
				SalesPerson salesperson = (SalesPerson)PXSelectorAttribute.Select<SOLine.salesPersonID>(sender, e.Row);
				INPostClass postclass = INPostClass.PK.Find(this, item?.PostClassID) ?? new INPostClass();
				EPEmployee employee = (EPEmployee)PXSelect<EPEmployee, Where<EPEmployee.defContactID, Equal<Current<SOOrder.ownerID>>>>.Select(this);
				CRLocation companyloc =
					PXSelectJoin<CRLocation, InnerJoin<BAccountR, On<CRLocation.bAccountID, Equal<BAccountR.bAccountID>, And<CRLocation.locationID, Equal<BAccountR.defLocationID>>>, InnerJoin<Branch, On<BAccountR.bAccountID, Equal<Branch.bAccountID>>>>, Where<Branch.branchID, Equal<Required<SOLine.branchID>>>>.Select(this, line.BranchID);
				Location customerloc = location.Current;

				object item_SubID = GetValue<InventoryItem.salesSubID>(item);
				object site_SubID = GetValue<INSite.salesSubID>(site);
				object postclass_SubID = GetValue<INPostClass.salesSubID>(postclass);
				object customer_SubID = GetValue<Location.cSalesSubID>(customerloc);
				object employee_SubID = GetValue<EPEmployee.salesSubID>(employee);
				object company_SubID = GetValue<CRLocation.cMPSalesSubID>(companyloc);
				object salesperson_SubID = GetValue<SalesPerson.salesSubID>(salesperson);
				object reasoncode_SubID = GetValue<ReasonCode.salesSubID>(reasoncode);

				object value = null;
				bool exceptionThrown = false;

				try
				{
					value = SOSalesSubAccountMaskAttribute.MakeSub<SOOrderType.salesSubMask>(this, soordertype.Current.SalesSubMask,
																							 new object[]
																							 {
																								 item_SubID,
																								 site_SubID,
																								 postclass_SubID,
																								 customer_SubID,
																								 employee_SubID,
																								 company_SubID,
																								 salesperson_SubID,
																								 reasoncode_SubID
																							 },
																							 new Type[]
																							 {
																								 typeof(InventoryItem.salesSubID),
																								 typeof(INSite.salesSubID),
																								 typeof(INPostClass.salesSubID),
																								 typeof(Location.cSalesSubID),
																								 typeof(EPEmployee.salesSubID),
																								 typeof(Location.cMPSalesSubID),
																								 typeof(SalesPerson.salesSubID),
																								 typeof(ReasonCode.subID)
																							 });

					sender.RaiseFieldUpdating<SOLine.salesSubID>(line, ref value);
				}
				catch (PXMaskArgumentException ex)
				{
					sender.RaiseExceptionHandling<SOLine.salesSubID>(e.Row, null, new PXSetPropertyException(ex.Message));
					value = null;
					exceptionThrown = true;
				}
				catch (PXSetPropertyException ex)
				{
					sender.RaiseExceptionHandling<SOLine.salesSubID>(e.Row, value, ex);
					value = null;
					exceptionThrown = true;
				}

				if (!exceptionThrown && (this.IsImportFromExcel || this.IsImport || this.IsCopyPasteContext || this.IsContractBasedAPI))
				{
					// Acuminator disable once PX1075 RaiseExceptionHandlingInEventHandlers [we should clear the error set in the code above]
					sender.RaiseExceptionHandling<SOLine.salesSubID>(e.Row, value, null);
				}

				e.NewValue = (int?)value;
				e.Cancel = true;
			}
		}

		protected virtual void SOLine_Completed_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (e.Row != null && ((SOLine)e.Row).LineType == SOLineType.MiscCharge)
			{
				e.NewValue = true;
				e.Cancel = true;
			}
		}

		protected virtual void SOLine_ShipComplete_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			SOLine row = e.Row as SOLine;
			if (row != null && row.Completed == false)
			{
				foreach (SOLineSplit split in PXParentAttribute.SelectChildren(splits.Cache, e.Row, typeof(SOLine)))
				{
					if (split.Completed != true && split.POCompleted != true && split.POCancelled != true)
						splits.Cache.SetValue<SOLineSplit.shipComplete>(split, row.ShipComplete);
				}
			}
		}

		protected virtual void SOLine_LocationID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			SOLine row = (SOLine)e.Row;
			if (row != null && row.RequireLocation != true)
			{
				e.NewValue = null;
				e.Cancel = true;
			}
		}
		protected virtual void SOLineSplit_LocationID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			SOLineSplit row = (SOLineSplit)e.Row;
			if (row != null && row.RequireLocation != true)
			{
				e.NewValue = null;
				e.Cancel = true;
			}
		}

		protected virtual void SOLine_Completed_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			var row = (SOLine)e.Row;
			sender.SetValueExt<SOLine.closedQty>(e.Row, row.Completed == true ? row.OrderQty : row.ShippedQty);
		}

		protected virtual void SiteStatusByCostCenter_RowInserted(PXCache sender, PXRowInsertedEventArgs e)
		{
			if (ItemAvailabilityExt.IsFetching) return;
			var row = (SiteStatusByCostCenter)e.Row;
			if (row == null)
				return;

			//selecting INItemSiteSettings is quite expensive here -> select INSiteStatusByCostCenter that is per-row cached instead
			var sitestatus = INSiteStatusByCostCenter.PK.Find(this, row.InventoryID, row.SubItemID, row.SiteID, row.CostCenterID);
			if (Document.Current != null && Document.Current.Behavior != SOBehavior.QT && row.SiteID != null && sitestatus == null)
				row.InitSiteStatus = true;
		}

		protected virtual void SOLineSplit_LotSerialNbr_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			SOLineSplit row = (SOLineSplit)e.Row;
			if (row != null && row.RequireLocation != true)
			{
				e.Cancel = true;
			}
		}

		protected virtual void SOLine_UOM_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{

			SOLine row = (SOLine)e.Row;
			string oldUOM = (string)e.OldValue;

			if (row != null && oldUOM != row.UOM)
			{
				sender.SetDefaultExt<SOLine.curyUnitPrice>(row);
				sender.SetDefaultExt<SOLine.unitCost>(row);
				sender.SetValueExt<SOLine.extWeight>(row, row.BaseQty * row.UnitWeigth);
				sender.SetValueExt<SOLine.extVolume>(row, row.BaseQty * row.UnitVolume);
			}
		}

		protected virtual void SOLine_Operation_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			sender.SetDefaultExt<SOLine.tranType>(e.Row);
			sender.SetDefaultExt<SOLine.invtMult>(e.Row);
			sender.SetDefaultExt<SOLine.planType>(e.Row);
			sender.SetDefaultExt<SOLine.requireReasonCode>(e.Row);
			sender.SetDefaultExt<SOLine.autoCreateIssueLine>(e.Row);

			SOLine row = (SOLine)e.Row;
			if (row == null) return;
			sender.SetValueExt<SOLine.curyUnitPrice>(e.Row, row.CuryUnitPrice);
			sender.SetValueExt<SOLine.discPct>(e.Row, row.DiscPct);
			sender.SetValueExt<SOLine.curyDiscAmt>(e.Row, row.CuryDiscAmt);
		}

		protected virtual void SOLine_SalesPersonID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			sender.SetDefaultExt<SOLine.salesSubID>(e.Row);
		}

		protected virtual void SOLine_ReasonCode_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			SOLine row = e.Row as SOLine;
			if (row != null)
			{
				ReasonCode reasoncd = ReasonCode.PK.Find(this, (string)e.NewValue);

				if (reasoncd != null)
				{
					if (row.TranType == INTranType.Transfer && reasoncd.Usage == ReasonCodeUsages.Transfer ||
					row.TranType != INTranType.Transfer && reasoncd.Usage == ReasonCodeUsages.Issue ||
						row.TranType != INTranType.Issue && row.TranType != INTranType.Return && reasoncd.Usage == ReasonCodeUsages.Sales)
					{
						e.Cancel = true;
					}
					else
					{
						throw new PXSetPropertyException(IN.Messages.ReasonCodeDoesNotMatch);
					}

				}
			}
		}

		protected virtual void SOLine_ReasonCode_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			sender.SetDefaultExt<SOLine.salesAcctID>(e.Row);
			try
			{
				sender.SetDefaultExt<SOLine.salesSubID>(e.Row);
			}
			catch (PXSetPropertyException)
			{
				sender.SetValue<SOLine.salesSubID>(e.Row, null);
			}
		}

		protected virtual void SOLine_SiteID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			var row = (SOLine)e.Row;
			sender.SetDefaultExt<SOLine.salesAcctID>(row);
			try
			{
				sender.SetDefaultExt<SOLine.salesSubID>(row);
			}
			catch (PXSetPropertyException)
			{
				sender.SetValue<SOLine.salesSubID>(row, null);
			}

			if (string.IsNullOrEmpty(row.InvoiceNbr))
			{
				sender.SetDefaultExt<SOLine.unitCost>(row);
			}
			sender.SetDefaultExt<SOLine.pOSiteID>(row);
			using (GetPriceCalculationScope().AppendContext<SOLine.siteID>())
			sender.SetDefaultExt<SOLine.curyUnitPrice>(row);
		}

		[PopupMessage]
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		protected virtual void SOLine_InventoryID_CacheAttached(PXCache sender)
		{
		}


		protected virtual void SOLine_InventoryID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			SOLine row = e.Row as SOLine;

			sender.SetDefaultExt<SOLine.lineType>(e.Row);
			if (row.Operation == null)
				sender.SetDefaultExt<SOLine.operation>(e.Row);
			sender.SetDefaultExt<SOLine.tranType>(e.Row);
			sender.RaiseExceptionHandling<SOLine.uOM>(e.Row, null, null);
			sender.SetDefaultExt<SOLine.uOM>(e.Row);
			sender.SetValue<SOLine.closedQty>(e.Row, 0m);
			sender.SetDefaultExt<SOLine.orderQty>(e.Row);
			if (IsImport)
			{
				sender.SetDefaultExt<SOLine.salesPersonID>(e.Row);
				sender.SetDefaultExt<SOLine.reasonCode>(e.Row);
			}
			sender.SetDefaultExt<SOLine.salesAcctID>(e.Row);
			try
			{
				sender.SetDefaultExt<SOLine.salesSubID>(e.Row);
			}
			catch (PXSetPropertyException)
			{
				sender.SetValue<SOLine.salesSubID>(e.Row, null);
			}
			sender.SetDefaultExt<SOLine.tranDesc>(e.Row);
			sender.SetDefaultExt<SOLine.taxCategoryID>(e.Row);
			sender.SetDefaultExt<SOLine.vendorID>(e.Row);
			sender.SetDefaultExt<SOLine.unitCost>(e.Row);
			sender.SetDefaultExt<SOLine.unitWeigth>(e.Row);
			sender.SetDefaultExt<SOLine.unitVolume>(e.Row);
			sender.SetDefaultExt<SOLine.pOSiteID>(e.Row);
			sender.SetDefaultExt<SOLine.completeQtyMin>(e.Row);
			sender.SetDefaultExt<SOLine.completeQtyMax>(e.Row);
		}

		protected virtual void SOLine_OrderQty_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			SOLine row = e.Row as SOLine;
			decimal? oldOrderQty = (decimal?)e.OldValue;

			if (row != null && row.Qty != oldOrderQty)
			{
				if (row.Qty == 0)
				{
					sender.SetValueExt<SOLine.curyDiscAmt>(row, decimal.Zero);
					sender.SetValueExt<SOLine.discPct>(row, decimal.Zero);
				}
				using (GetPriceCalculationScope().AppendContext<SOLine.orderQty>())
				sender.SetDefaultExt<SOLine.curyUnitPrice>(row);
			}
		}

		protected virtual void SOLine_ManualPrice_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			SOLine row = e.Row as SOLine;
			if (row == null)
				return;
			sender.SetDefaultExt<SOLine.curyUnitPrice>(row);
			if (row.ManualPrice == true && row.SkipLineDiscounts == true)
			{
				sender.SetValue<SOLine.skipLineDiscounts>(row, false);
			}
		}

		protected virtual void SOLine_CustomerID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			SOLine row = e.Row as SOLine;
			if (row == null) return;

			sender.SetDefaultExt<SOLine.salesPersonID>(e.Row);

			try
			{
				sender.SetDefaultExt<SOLine.salesSubID>(e.Row);
			}
			catch (PXSetPropertyException)
			{
				sender.SetValue<SOLine.salesSubID>(e.Row, null);
			}
		}

		public virtual bool RecalculatePriceAndDiscount()
		{
			//TODO based on setting. Default is True, If UnattendedMode & PrompUser - use Default.
			return true;
		}

		protected virtual void SOLine_OrderQty_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			var row = (SOLine)e.Row;
			if (row.LineType.IsNotIn(SOLineType.Inventory, SOLineType.NonInventory))
				return;

			decimal? newValue = (decimal?)e.NewValue;
			decimal? controlValue = (row.RequireShipping == true) ? row.ClosedQty : 0m;
			if (soordertype.Current.ActiveOperationsCntr <= 1)
			{
				if (newValue < controlValue && row.TranType != INTranType.NoUpdate)
				{
					throw new PXSetPropertyException(CS.Messages.Entry_GE, controlValue.Value.ToString("0.####"));
				}
			}
			else
			{
				if (newValue >= 0m && newValue < controlValue)
				{
					throw new PXSetPropertyException(CS.Messages.Entry_GE, controlValue.Value.ToString("0.####"));
				}
				else if (newValue < 0m && newValue > controlValue)
				{
					throw new PXSetPropertyException(CS.Messages.Entry_LE, controlValue.Value.ToString("0.####"));
				}

				if (!string.IsNullOrEmpty(row.InvoiceNbr) && row.Operation == SOOperation.Receipt)
				{
					if (newValue > 0m && row.OrderQty <= 0m && row.LineSign < 0)
					{
						throw new PXSetPropertyException(CS.Messages.Entry_LE, 0m.ToString());
					}
					else if (newValue < 0m && row.OrderQty >= 0m && row.LineSign > 0)
					{
						throw new PXSetPropertyException(CS.Messages.Entry_GE, 0m.ToString());
					}
				}
			}
		}

		protected virtual void SOLine_DiscPct_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			SOLine row = e.Row as SOLine;
		}

		protected virtual void SOLine_DiscPct_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			SOLine row = e.Row as SOLine;

			if (row == null)
				return;

			sender.RaiseExceptionHandling<SOLine.discPct>(row, null, null);

			if (this.GetMinGrossProfitValidationOption(sender, row) == MinGrossProfitValidationType.None)
				return;

			var mgpc = new MinGrossProfitClass
			{
				DiscPct = (decimal?)e.NewValue,
				CuryDiscAmt = row.CuryDiscAmt,
				CuryUnitPrice = row.CuryUnitPrice
			};

			SOLineValidateMinGrossProfit(sender, row, mgpc);

			e.NewValue = mgpc.DiscPct;
		}

		protected virtual void SOLine_CuryDiscAmt_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			SOLine row = e.Row as SOLine;

			if (row == null)
				return;

			sender.RaiseExceptionHandling<SOLine.curyDiscAmt>(row, null, null);

			if (this.GetMinGrossProfitValidationOption(sender, row) == MinGrossProfitValidationType.None)
				return;

			var mgpc = new MinGrossProfitClass
			{
				DiscPct = row.DiscPct,
				CuryDiscAmt = (decimal?)e.NewValue,
				CuryUnitPrice = row.CuryUnitPrice
			};

			SOLineValidateMinGrossProfit(sender, row, mgpc);

			e.NewValue = mgpc.CuryDiscAmt;
		}


		/// <summary>
		/// Checks if ManualPrice flag should be set automatically on import from Excel.
		/// This method is intended to be called from _FieldVerifying event handler.
		/// </summary>
		protected virtual bool IsManualPriceFlagNeeded(PXCache sender, SOLine row)
		{
			if (row != null && row.ManualPrice != true && (sender.Graph.IsImportFromExcel || sender.Graph.IsContractBasedAPI))
			{
				decimal price;

				object curyUnitPrice = sender.GetValuePending<SOLine.curyUnitPrice>(row);
				object curyExtPrice = sender.GetValuePending<SOLine.curyExtPrice>(row);
				object manualPrice = sender.GetValuePending<SOLine.manualPrice>(row);

				if (((curyUnitPrice != PXCache.NotSetValue && curyUnitPrice != null && Decimal.TryParse(curyUnitPrice.ToString(), out price))
					|| (curyExtPrice != PXCache.NotSetValue && curyExtPrice != null && Decimal.TryParse(curyExtPrice.ToString(), out price)))
					&& (manualPrice == PXCache.NotSetValue || manualPrice == null))
				{
					return true;
				}
			}
			return false;
		}

		protected virtual void SOLine_CuryExtPrice_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			SOLine row = e.Row as SOLine;

			if (row == null)
				return;

			if (row.OrderQty > 0m && (decimal?)e.NewValue < 0m)
			{
				throw new PXSetPropertyException(CS.Messages.Entry_GE, 0m.ToString("0.####"));
			}
			else if (row.OrderQty < 0m && (decimal?)e.NewValue > 0m)
			{
				throw new PXSetPropertyException(CS.Messages.Entry_LE, 0m.ToString("0.####"));
			}

			if (IsManualPriceFlagNeeded(sender, row))
				row.ManualPrice = true;
		}

		public virtual SOOrderPriceCalculationScope CheckSourceChange() => new SOOrderPriceCalculationScope();

		protected virtual void SOLine_CuryUnitPrice_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			SOLine row = e.Row as SOLine;

			if (row == null)
				return;

			if ((decimal?)e.NewValue < 0m)
			{
				throw new PXSetPropertyException(CS.Messages.Entry_GE, 0m.ToString("0.####"));
			}

			var isPriceTypeValidationNeeded = true;
			if (e.NewValue != null)
			{
				isPriceTypeValidationNeeded = CheckSourceChange().Any(); 
			}

			if (row.IsFree != true && (this.GetMinGrossProfitValidationOption(sender, row, isPriceTypeValidationNeeded) != MinGrossProfitValidationType.None))
			{
				sender.RaiseExceptionHandling<SOLine.curyUnitPrice>(row, null, null);

				var mgpc = new MinGrossProfitClass
				{
					DiscPct = row.DiscPct,
					CuryDiscAmt = row.CuryDiscAmt,
					CuryUnitPrice = (decimal?)e.NewValue
				};

				SOLineValidateMinGrossProfit(sender, row, mgpc, isPriceTypeValidationNeeded);

				e.NewValue = mgpc.CuryUnitPrice;
			}

			if (IsManualPriceFlagNeeded(sender, row))
				row.ManualPrice = true;
		}

		protected virtual void SOLine_CuryUnitPrice_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			var row = (SOLine)e.Row;
			if (row == null) return;

			bool isPriceUpdateNeeded;
			using (var priceScope = GetPriceCalculationScope())
				isPriceUpdateNeeded = priceScope.IsUpdateNeeded<SOLine.inventoryID>();

			if (row.TranType == INTranType.Transfer)
			{
				e.NewValue = 0m;
			}
			else if (row.InventoryID != null && row.ManualPrice != true && row.IsFree != true && !sender.Graph.IsCopyPasteContext
				&& isPriceUpdateNeeded)
			{
				string customerPriceClass = ARPriceClass.EmptyPriceClass;
				Location c = location.Select();
				if (!string.IsNullOrEmpty(c?.CPriceClassID))
					customerPriceClass = c.CPriceClassID;
				CurrencyInfo curyInfo = currencyinfo.Select();

				ARSalesPriceMaint salesPriceMaint = ARSalesPriceMaint.SingleARSalesPriceMaint;
				bool alwaysFromBaseCury = salesPriceMaint.GetAlwaysFromBaseCurrencySetting(sender);
				ARSalesPriceMaint.SalesPriceItem priceItem = null;
				try
				{
					priceItem = salesPriceMaint.FindSalesPrice(
						sender,
						customerPriceClass,
						row.CustomerID,
						row.InventoryID,
						row.SiteID,
						curyInfo.BaseCuryID,
						alwaysFromBaseCury ? curyInfo.BaseCuryID : curyInfo.CuryID,
						Math.Abs(row.Qty ?? 0m),
						row.UOM,
						Document.Current.OrderDate.Value,
						Document.Current.TaxCalcMode);
					// Acuminator disable once PX1047 RowChangesInEventHandlersForbiddenForArgs [Regular C# field used for optimization]
					row.SkipLineDiscountsBuffer = priceItem?.SkipLineDiscounts;
				}
				catch (PXUnitConversionException)
				{
				}

				decimal? price = salesPriceMaint.AdjustSalesPrice(sender, priceItem, row.InventoryID, curyInfo, row.UOM);
				e.NewValue = price ?? 0m;

				ARSalesPriceMaint.CheckNewUnitPrice<SOLine, SOLine.curyUnitPrice>(sender, row, price);

				if (priceItem?.UOM != row.UOM || priceItem?.CuryID != Document.Current.CuryID)
				{
					priceItem = null;
				}
				row.PriceType = priceItem?.PriceType;
				row.IsPromotionalPrice = priceItem?.IsPromotionalPrice ?? false;
			}
			else
				e.NewValue = row.CuryUnitPrice ?? 0m;
		}

		protected virtual void SOLine_IsFree_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			SOLine row = e.Row as SOLine;
			if (row != null)
			{
				if (row.IsFree == true)
				{
					sender.SetValueExt<SOLine.curyUnitPrice>(row, 0m);
					sender.SetValueExt<SOLine.discPct>(row, 0m);
					sender.SetValueExt<SOLine.curyDiscAmt>(row, 0m);
					if (e.ExternalCall)
						sender.SetValueExt<SOLine.manualDisc>(row, true);
				}
				else
				{
					if (e.ExternalCall)
					{
						sender.SetDefaultExt<SOLine.curyUnitPrice>(row);
						sender.SetValueExt<SOLine.manualPrice>(row, false);
					}
				}
			}
		}

		protected virtual void SOLine_IsPromotionalPrice_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			SOLine row = (SOLine)e.Row;
			if (row == null) return;

			bool raiseMinGrossProfitValidation = ((bool?)e.OldValue == true && row.IsPromotionalPrice == false);
			if (raiseMinGrossProfitValidation)
			{
				object val = null;
				sender.RaiseFieldVerifying<SOLine.curyUnitPrice>(row, ref val);
				sender.RaiseFieldVerifying<SOLine.discPct>(row, ref val);
				sender.RaiseFieldVerifying<SOLine.curyDiscAmt>(row, ref val);
			}
		}

		protected virtual void SOLine_PriceType_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			SOLine row = (SOLine)e.Row;
			if (row == null) return;

			bool raiseMinGrossProfitValidation = ((string)e.OldValue).IsIn(PriceTypes.Customer, PriceTypes.CustomerPriceClass)
				&& row.PriceType.IsNotIn(PriceTypes.Customer, PriceTypes.CustomerPriceClass);
			if (raiseMinGrossProfitValidation)
			{
				object val = null;
				if(CheckSourceChange().Any())
				sender.RaiseFieldVerifying<SOLine.curyUnitPrice>(row, ref val);
				sender.RaiseFieldVerifying<SOLine.discPct>(row, ref val);
				sender.RaiseFieldVerifying<SOLine.curyDiscAmt>(row, ref val);
			}
		}

		protected virtual void SOLine_DiscountID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			SOLine row = e.Row as SOLine;
			if (e.ExternalCall && row != null)
			{
				try
				{
					Document.Current.DeferPriceDiscountRecalculation = false;
					//Acuminator disable once PX1045 PXGraphCreateInstanceInEventHandlers 
					_discountEngine.UpdateManualLineDiscount(sender, Transactions, row, DiscountDetails, Document.Current.BranchID, Document.Current.CustomerLocationID, Document.Current.OrderDate, GetDefaultSODiscountCalculationOptions(Document.Current, true));
				}
				finally
				{
					Document.Current.DeferPriceDiscountRecalculation = soordertype.Current.DeferPriceDiscountRecalculation; ;
				}
			}
		}

		protected virtual void SOLine_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			SOLine row = (SOLine)e.Row;
			if (row == null) return;
			bool lineTypeInventory = row.LineType == SOLineType.Inventory;
			bool isNonStockKit = (!row.IsStockItem ?? false) && (row.IsKit ?? false);
			PXUIFieldAttribute.SetEnabled<SOLine.subItemID>(sender, row, lineTypeInventory && !isNonStockKit);
			PXUIFieldAttribute.SetEnabled<SOLine.locationID>(sender, row, lineTypeInventory && LineSplittingExt.IsLocationEnabled);

			PXUIFieldAttribute.SetEnabled<SOLine.curyUnitPrice>(sender, row, row.IsFree != true);

			bool autoFreeItem = row.ManualDisc != true && row.IsFree == true;
			bool freeItem = row.IsFree == true;

			PXUIFieldAttribute.SetEnabled<SOLine.manualDisc>(sender, e.Row, !freeItem && (Document.Current != null && Document.Current.DisableAutomaticDiscountCalculation != true));
			PXUIFieldAttribute.SetEnabled<SOLine.orderQty>(sender, e.Row, !autoFreeItem);
			PXUIFieldAttribute.SetEnabled<SOLine.isFree>(sender, e.Row, !autoFreeItem && row.InventoryID != null);
			PXUIFieldAttribute.SetEnabled<SOLine.skipLineDiscounts>(sender, e.Row, IsCopyPasteContext);

			bool? Completed = ((SOLine)e.Row).Completed;

			if (row.POSource != INReplenishmentSource.DropShipToOrder || row.IsLegacyDropShip == true)
			{
				PXUIFieldAttribute.SetEnabled<SOLine.pOLinkActive>(sender, e.Row, false);
			}

			if (((SOLine)e.Row).ShippedQty > 0m)
			{
				PXUIFieldAttribute.SetEnabled(sender, e.Row, false);
				PXUIFieldAttribute.SetEnabled<SOLine.tranDesc>(sender, e.Row);
				PXUIFieldAttribute.SetEnabled<SOLine.orderQty>(sender, e.Row, Completed == false);
				PXUIFieldAttribute.SetEnabled<SOLine.shipComplete>(sender, e.Row, Completed == false);
				PXUIFieldAttribute.SetEnabled<SOLine.completeQtyMin>(sender, e.Row, Completed == false);
				PXUIFieldAttribute.SetEnabled<SOLine.completeQtyMax>(sender, e.Row, Completed == false);
				PXUIFieldAttribute.SetEnabled<SOLine.completed>(sender, e.Row, true);
			}

			Transactions.Cache.Adjust<PXUIFieldAttribute>(row)
				.For<SOLine.vendorID>(a => a.Enabled = row.POCreate == true)
				.SameFor<SOLine.pOSiteID>();

			SOLine line = (SOLine)e.Row;
			if (line != null && line.Operation == SOOperation.Issue)
			{
				PXUIFieldAttribute.SetEnabled<SOLine.autoCreateIssueLine>(sender, e.Row, false);
			}

			InventoryItem item = InventoryItem.PK.Find(this, row.InventoryID);
			bool isConverted = (item?.IsConverted == true && row.IsStockItem != null && item.StkItem != row.IsStockItem);
			if (isConverted)
				PXUIFieldAttribute.SetEnabled(sender, e.Row, false);

			splits.Cache.AllowInsert = Transactions.Cache.AllowInsert && Completed != true && !isConverted;
			splits.Cache.AllowUpdate = Transactions.Cache.AllowUpdate && Completed != true && !isConverted;
			splits.Cache.AllowDelete = Transactions.Cache.AllowDelete && Completed != true && !isConverted;

			SOOrder header = Document.Current;

			if (header != null && !isConverted)
			{
				PXUIFieldAttribute.SetEnabled<SOLine.shipDate>(sender, row, header.ShipComplete == SOShipComplete.BackOrderAllowed);

				if (header.Hold != true && header.DontApprove != true)
				{
					PXUIFieldAttribute.SetEnabled(sender, row, false);
				}

				PXUIFieldAttribute.SetEnabled<SOLine.curyUnitCost>(sender, row, IsCuryUnitCostEnabled(row, header));
			}

			if (row != null && row.CuryUnitPrice != null && row.DiscPct != null && row.DiscAmt != null)
			{
				SOLineValidateMinGrossProfit(sender, row, new MinGrossProfitClass { CuryDiscAmt = row.CuryDiscAmt, CuryUnitPrice = row.CuryUnitPrice, DiscPct = row.DiscPct });
			}

			if (row.InvoiceNbr != null)
			{
				PXUIFieldAttribute.SetEnabled<SOLine.operation>(sender, row, false);
			}
		}

		protected virtual void SOLine_RowInserted(PXCache sender, PXRowInsertedEventArgs e)
		{
			SOLine row = e.Row as SOLine;
			if (row == null)
				return;

			if (Document.Current != null && Document.Current.DisableAutomaticDiscountCalculation == true && row.IsFree != true)
				row.ManualDisc = true;

			if (sender.Graph.IsCopyPasteContext)
			{
				if (row.RequireLocation == false)
					row.LocationID = null;
				if (row.ManualDisc != true && row.IsFree == true)
				{
					ResetQtyOnFreeItem(sender, row);
					//Acuminator disable once PX1045 PXGraphCreateInstanceInEventHandlers 
					_discountEngine.ClearDiscount(sender, row);
				}

				//Acuminator disable once PX1045 PXGraphCreateInstanceInEventHandlers
				RecalculateDiscounts(sender, row);

				if (row.ManualDisc != true)
				{
					var discountCode = (ARDiscount)PXSelectorAttribute.Select<SOLine.discountID>(sender, row);
					row.DiscPctDR = (discountCode != null && discountCode.IsAppliedToDR == true) ? row.DiscPct : 0.0m;
				}

				row.ManualPrice = true;

				TaxAttribute.Calculate<SOLine.taxCategoryID>(sender, e);

				DirtyFormulaAttribute.RaiseRowUpdated<SOLine.openLine>(sender, new PXRowUpdatedEventArgs(e.Row, new SOLine(), e.ExternalCall));
			}
			else
			{
				if (row.SkipLineDiscountsBuffer != null)
				{
					row.SkipLineDiscounts = row.SkipLineDiscountsBuffer;
				}

				if (!this.IsImportFromExcel)
				{
					//Acuminator disable once PX1045 PXGraphCreateInstanceInEventHandlers
					RecalculateDiscounts(sender, (SOLine)e.Row);
				}

				TaxAttribute.Calculate<SOLine.taxCategoryID>(sender, e);
			}
		}

		protected virtual void SOLine_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			if (e.Operation.Command().IsNotIn(PXDBOperation.Insert, PXDBOperation.Update))
				return;

			var line = (SOLine)e.Row;

				PXDefaultAttribute.SetPersistingCheck<SOLine.salesAcctID>(sender, e.Row,
						soordertype.Current == null || Document.Current == null ||
						soordertype.Current.ARDocType == ARDocType.NoUpdate ||
						Document.Current.Hold == true
						? PXPersistingCheck.Nothing : PXPersistingCheck.NullOrBlank);

				PXDefaultAttribute.SetPersistingCheck<SOLine.salesSubID>(sender, e.Row,
						soordertype.Current == null || Document.Current == null ||
						soordertype.Current.ARDocType == ARDocType.NoUpdate ||
						Document.Current.Hold == true
						? PXPersistingCheck.Nothing : PXPersistingCheck.NullOrBlank);

				PXDefaultAttribute.SetPersistingCheck<SOLine.subItemID>(sender, e.Row,
					soordertype.Current == null || soordertype.Current.RequireLocation == true || line.LineType != SOLineType.Inventory || ((!line.IsStockItem ?? false) && (line.IsKit ?? false))
					? PXPersistingCheck.Nothing : PXPersistingCheck.NullOrBlank);

				PXDefaultAttribute.SetPersistingCheck<SOLine.reasonCode>(sender, e.Row,
					line.RequireReasonCode == true
					? PXPersistingCheck.NullOrBlank : PXPersistingCheck.Nothing);

				PXDefaultAttribute.SetPersistingCheck<SOLine.taskID>(sender, e.Row, ProjectDefaultAttribute.IsProject(this, ((SOLine)e.Row).ProjectID) ? PXPersistingCheck.NullOrBlank : PXPersistingCheck.Nothing);

				ItemAvailabilityExt.MemoCheck(line, true);
		}

		protected virtual void SOLine_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			SOLine row = e.Row as SOLine;
			SOLine oldRow = e.OldRow as SOLine;

			if (row != null && row.RequireLocation == false)
				row.LocationID = null;

			if (Document.Current.DeferPriceDiscountRecalculation == true && !sender.ObjectsEqual<SOLine.taxCategoryID>(oldRow, row))
			{
				Document.Current.IsPriceAndDiscountsValid = false;
			}

			if ((e.ExternalCall || sender.Graph.IsImport)
				&& sender.ObjectsEqual<SOLine.customerID>(e.Row, e.OldRow)
				&& sender.ObjectsEqual<SOLine.inventoryID>(e.Row, e.OldRow) && sender.ObjectsEqual<SOLine.uOM>(e.Row, e.OldRow)
				&& sender.ObjectsEqual<SOLine.orderQty>(e.Row, e.OldRow) && sender.ObjectsEqual<SOLine.branchID>(e.Row, e.OldRow)
				&& sender.ObjectsEqual<SOLine.siteID>(e.Row, e.OldRow)
				&& sender.ObjectsEqual<SOLine.manualPrice>(e.Row, e.OldRow)
				&& (!sender.ObjectsEqual<SOLine.curyUnitPrice>(e.Row, e.OldRow) || !sender.ObjectsEqual<SOLine.curyExtPrice>(e.Row, e.OldRow)))
			{
				row.ManualPrice = true;
				sender.SetValue<SOLine.skipLineDiscounts>(row, false);
				sender.SetValueExt<SOLine.priceType>(row, null);
				sender.SetValueExt<SOLine.isPromotionalPrice>(row, false);
			}
			else
			{
				if (row.SkipLineDiscountsBuffer != null)
				{
					sender.SetValue<SOLine.skipLineDiscounts>(row, row.SkipLineDiscountsBuffer);
					row.SkipLineDiscountsBuffer = null;
				}
			}

			if (!sender.ObjectsEqual<SOLine.branchID>(e.Row, e.OldRow) || !sender.ObjectsEqual<SOLine.customerID>(e.Row, e.OldRow) || !sender.ObjectsEqual<SOLine.inventoryID>(e.Row, e.OldRow) ||
					!sender.ObjectsEqual<SOLine.siteID>(e.Row, e.OldRow) || !sender.ObjectsEqual<SOLine.baseOrderQty>(e.Row, e.OldRow) || !sender.ObjectsEqual<SOLine.isFree>(e.Row, e.OldRow) ||
					!sender.ObjectsEqual<SOLine.curyUnitPrice>(e.Row, e.OldRow) || !sender.ObjectsEqual<SOLine.curyExtPrice>(e.Row, e.OldRow) || !sender.ObjectsEqual<SOLine.curyLineAmt>(e.Row, e.OldRow) ||
					!sender.ObjectsEqual<SOLine.curyDiscAmt>(e.Row, e.OldRow) || !sender.ObjectsEqual<SOLine.discPct>(e.Row, e.OldRow) ||
					!sender.ObjectsEqual<SOLine.skipLineDiscounts>(e.Row, e.OldRow) ||
					!sender.ObjectsEqual<SOLine.manualDisc>(e.Row, e.OldRow) || !sender.ObjectsEqual<SOLine.discountID>(e.Row, e.OldRow))
				if (row.ManualDisc != true)
				{
					if (oldRow.ManualDisc == true)//Manual Discount Unckecked
					{
						if (row.IsFree == true)
						{
							ResetQtyOnFreeItem(sender, row);
						}
					}

					if (row.IsFree == true)
					{
						//Acuminator disable once PX1045 PXGraphCreateInstanceInEventHandlers 
						_discountEngine.ClearDiscount(sender, row);
					}

					//Acuminator disable once PX1045 PXGraphCreateInstanceInEventHandlers
					RecalculateDiscounts(sender, row);
				}
				else
				{
					//Acuminator disable once PX1045 PXGraphCreateInstanceInEventHandlers
					RecalculateDiscounts(sender, row, oldRow);
				}

			if (row.ManualDisc != true)
			{
				var discountCode = (ARDiscount)PXSelectorAttribute.Select<SOLine.discountID>(sender, row);
				row.DiscPctDR = (discountCode != null && discountCode.IsAppliedToDR == true) ? row.DiscPct : 0.0m;
			}

			if (row.ManualPrice != true)
			{
				row.CuryUnitPriceDR = row.CuryUnitPrice;
			}

			TaxAttribute.Calculate<SOLine.taxCategoryID>(sender, e);

			DirtyFormulaAttribute.RaiseRowUpdated<SOLine.openLine>(sender, e);

			if ((e.ExternalCall || sender.Graph.IsImport)
			   && !sender.ObjectsEqual<SOLine.completed>(e.Row, e.OldRow) && ((SOLine)e.Row).Completed != true && ((SOLine)e.Row).ShipComplete != SOShipComplete.BackOrderAllowed)
			{
				foreach (SOLineSplit split in PXParentAttribute.SelectChildren(splits.Cache, e.Row, typeof(SOLine)))
				{
					if (split.ShipmentNbr != null || split.ShippedQty > 0m)
						sender.SetValueExt<SOLine.shipComplete>(e.Row, SOShipComplete.BackOrderAllowed);
				}
			}

			if (sender.Graph.IsMobile)
			{
				var cur = sender.Locate(e.Row);
				sender.Current = cur;
			}
		}

		protected virtual void SOLine_RowDeleting(PXCache sedner, PXRowDeletingEventArgs e)
		{
			SOLine row = e.Row as SOLine;
			if (row != null && (row.ShippedQty > 0 || splits.Select().AsEnumerable().Where(x => ((SOLineSplit)x).ShipmentNbr != null).Count() > 0))
				throw new PXException(Messages.ShippedLineDeleting);
		}

		protected virtual void SOLineSplit_LotSerialNbr_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			SOLineSplit row = (SOLineSplit)e.Row;
			if (row == null) return;

			if (soordertype.Current?.RequireShipping == true && soordertype.Current.INDocType != INTranType.NoUpdate
				&& row.Operation == SOOperation.Issue && row.LotSerialNbr != null)
			{
				var item = InventoryItem.PK.Find(this, row.InventoryID);
				var lotserialclass = INLotSerClass.PK.Find(this, item?.LotSerClassID);
				if (lotserialclass != null && lotserialclass.LotSerAssign != INLotSerAssign.WhenReceived)
				{
					splits.Cache.RaiseExceptionHandling<SOLineSplit.lotSerialNbr>(row, null,
						new PXSetPropertyException(Messages.LotSerialSelectionForOnReceiptOnly, PXErrorLevel.Warning));
					row.LotSerialNbr = null;
				}
			}
		}

		protected virtual void SOLineSplit_RowDeleting(PXCache sender, PXRowDeletingEventArgs e)
		{
			SOLineSplit row = e.Row as SOLineSplit;
			if (row != null && (row.ShippedQty > 0 || row.ShipmentNbr != null))
				throw new PXException(Messages.ShippedLineDeleting);

			if (row != null && (row.ReceivedQty > 0 || row.PONbr != null))
			{
				SOLine soline = PXParentAttribute.SelectParent<SOLine>(sender, e.Row);
				bool deleted = (soline == null);
				if (!deleted)
					throw new PXException(Messages.ReceivedLineDeleting);
			}
		}


		[PXDBString(15, IsUnicode = true, BqlField = typeof(SOLineSplit.sOOrderNbr))]
		[PXDBDefault(typeof(SOOrder.orderNbr), PersistingCheck = PXPersistingCheck.Nothing)]
		protected virtual void SOLineSplit2_SOOrderNbr_CacheAttached(PXCache sender)
		{
		}

		protected virtual void SOLineSplit_RowDeleted(PXCache sender, PXRowDeletedEventArgs e)
		{
			if (Document.Current?.IsTransferOrder != true) return;

			foreach (PXResult<SOLineSplit2, INItemPlan> r in this.sodemand.View.SelectMultiBound(new object[] { e.Row }))
			{
				SOLineSplit2 upd = PXCache<SOLineSplit2>.CreateCopy(r);
				INItemPlan plan = r;

				upd.SOOrderType = null;
				upd.SOOrderNbr = null;
				upd.SOLineNbr = null;
				upd.SOSplitLineNbr = null;
				upd.RefNoteID = null;
				upd.CostCenterID = plan.CostCenterID;

				upd = this.sodemand.Update(upd);

				if (plan.PlanType != null)
				{
					plan.SiteID = upd.SiteID;
					plan.SourceSiteID = upd.SiteID;
					plan.PlanType = upd.IsAllocated == true ? INPlanConstants.Plan61 : INPlanConstants.Plan60;
					plan.SupplyPlanID = null;
					plan.FixedSource = INReplenishmentSource.Transfer;

					sender.Graph.Caches[typeof(INItemPlan)].Update(plan);
				}
			}

			var split = (SOLineSplit)e.Row;
			if (split?.PlanID != null)
			{
				INItemPlan orphanedPlan = PXSelectReadonly<INItemPlan,
					Where<INItemPlan.supplyPlanID, Equal<Required<INItemPlan.supplyPlanID>>,
						And<INItemPlan.planType, Equal<INPlanConstants.plan94>>>>
					.Select(this, split.PlanID);
				if (orphanedPlan != null)
				{
					Caches[typeof(INItemPlan)].Delete(orphanedPlan);
				}
			}
		}

		protected virtual void SOLine_RowDeleted(PXCache sender, PXRowDeletedEventArgs e)
		{
			SOLine row = e.Row as SOLine;

			if (Document.Current != null && Document.Cache.GetStatus(Document.Current) != PXEntryStatus.Deleted && Document.Cache.GetStatus(Document.Current) != PXEntryStatus.InsertedDeleted && !(row.IsFree == true && row.ManualDisc == false))
			{
				if (!DisableGroupDocDiscount)
				{
					try
					{
						Document.Current.DeferPriceDiscountRecalculation = false;
						//Acuminator disable once PX1045 PXGraphCreateInstanceInEventHandlers 
						_discountEngine.RecalculateGroupAndDocumentDiscounts(sender, Transactions, null, DiscountDetails, Document.Current.BranchID, Document.Current.CustomerLocationID, Document.Current.OrderDate, GetDefaultSODiscountCalculationOptions(Document.Current, true));
					}
					finally
					{
						Document.Current.DeferPriceDiscountRecalculation = soordertype.Current.DeferPriceDiscountRecalculation; ;
					}
				}
				RecalculateTotalDiscount();
				RefreshFreeItemLines(sender);
			}

			if (Document.Current != null)
			{
				Document.Cache.MarkUpdated(Document.Current, assertError: true);
			}
		}

		protected virtual void SOLine_AvgCost_FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			if (e.Row != null)
			{
				decimal? AvgCost = (decimal?)sender.GetValue<SOLine.avgCost>(e.Row);

				if (AvgCost == null)
				{
					SOLine row = (SOLine)e.Row;
					INItemStats stats = initemstats.Select(row.InventoryID, row.SiteID);

					if (stats == null) return;

					row.AvgCost = stats.QtyOnHand == 0 ? null : stats.TotalCost / stats.QtyOnHand;
					AvgCost = row.AvgCost;

					if (AvgCost == null) return;
				}

				AvgCost = INUnitAttribute.ConvertToBase<SOLine.inventoryID, SOLine.uOM>(sender, e.Row, (decimal)AvgCost, INPrecision.UNITCOST);

				if (!sender.Graph.Accessinfo.CuryViewState)
				{
					decimal CuryAvgCost;
					PXCurrencyAttribute.CuryConvCury(sender, e.Row, (decimal)AvgCost, out CuryAvgCost, CommonSetupDecPl.PrcCst);
					e.ReturnValue = CuryAvgCost;
				}
				else
				{
					e.ReturnValue = AvgCost;
				}
			}
		}

		public class MinGrossProfitClass
		{
			public decimal? CuryUnitPrice { get; set; }
			public decimal? CuryDiscAmt { get; set; }
			public decimal? DiscPct { get; set; }
			public MinGrossProfitClass() { }
		}

		protected virtual void SOLineValidateMinGrossProfit(PXCache sender, SOLine row, MinGrossProfitClass mgpc)
		{
			SOLineValidateMinGrossProfit(sender, row, mgpc, true);
		}
		protected virtual void SOLineValidateMinGrossProfit(PXCache sender, SOLine row, MinGrossProfitClass mgpc, bool isPriceTypeValidationNeeded)
		{
			if (row == null) return;

			string minGrossProfitValidation = this.GetMinGrossProfitValidationOption(sender, row, isPriceTypeValidationNeeded);
			if (minGrossProfitValidation == MinGrossProfitValidationType.None)
				return;

			if (row.IsFree == true || sender.Graph.UnattendedMode)
				return;

			if (Document.Current?.IsTransferOrder == true || row.Operation == SOOperation.Receipt)
				return;

			if (row.InventoryID != null && row.UOM != null && mgpc.CuryUnitPrice >= 0 && row.BranchID != null)
			{
				InventoryItem inItem = InventoryItem.PK.Find(this, row.InventoryID);
				Branch branch = Branch.PK.Find(this, row.BranchID);
				INItemCost itemCost = initemcost.Select(row.InventoryID, branch.BaseCuryID);

				mgpc.CuryUnitPrice = MinGrossProfitValidator<SOLine>.ValidateUnitPrice<SOLine.curyInfoID, SOLine.inventoryID, SOLine.uOM>(sender, row, inItem, itemCost, mgpc.CuryUnitPrice, minGrossProfitValidation);

				if (mgpc.DiscPct != 0)
				{
					mgpc.DiscPct = MinGrossProfitValidator<SOLine>.ValidateDiscountPct<SOLine.inventoryID, SOLine.uOM>(sender, row, inItem, itemCost, row.UnitPrice, mgpc.DiscPct, minGrossProfitValidation);
				}

				if (mgpc.CuryDiscAmt != 0 && row.Qty != null && Math.Abs(row.Qty.GetValueOrDefault()) != 0)
				{
					mgpc.CuryDiscAmt = MinGrossProfitValidator<SOLine>.ValidateDiscountAmt<SOLine.inventoryID, SOLine.uOM>(sender, row, inItem, itemCost, row.UnitPrice, mgpc.CuryDiscAmt, minGrossProfitValidation);
				}
				}
			}

		public virtual string GetMinGrossProfitValidationOption(PXCache sender, SOLine row)
		{
			return GetMinGrossProfitValidationOption(sender, row,true);
		}

		public virtual string GetMinGrossProfitValidationOption(PXCache sender, SOLine row, bool isPriceTypeValidationNeeded)
		{
			if (isPriceTypeValidationNeeded && (row.IsPromotionalPrice == true && sosetup.Current.IgnoreMinGrossProfitPromotionalPrice == true
				|| row.PriceType == PriceTypes.Customer && sosetup.Current.IgnoreMinGrossProfitCustomerPrice == true
				|| row.PriceType == PriceTypes.CustomerPriceClass && sosetup.Current.IgnoreMinGrossProfitCustomerPriceClass == true))
			{
				return MinGrossProfitValidationType.None;
			}
			return sosetup.Current.MinGrossProfitValidation;
		}

		#endregion


		#region SOOrderDiscountDetail events

		protected virtual void SOOrderDiscountDetail_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			SOOrderDiscountDetail discountDetail = (SOOrderDiscountDetail)e.Row;
			if (discountDetail == null) return;

			//Event handler is kept to avoid breaking changes.
		}

		protected virtual void SOOrderDiscountDetail_RowInserted(PXCache sender, PXRowInsertedEventArgs e)
		{
			SOOrderDiscountDetail discountDetail = (SOOrderDiscountDetail)e.Row;
			//Acuminator disable once PX1045 PXGraphCreateInstanceInEventHandlers 
			if (!_discountEngine.IsInternalDiscountEngineCall && discountDetail != null)
			{
				try
				{
					Document.Current.DeferPriceDiscountRecalculation = false;
				if (discountDetail.DiscountID != null)
			{
						//Acuminator disable once PX1045 PXGraphCreateInstanceInEventHandlers 
						_discountEngine.InsertManualDocGroupDiscount(Transactions.Cache, Transactions, DiscountDetails, discountDetail, discountDetail.DiscountID, null, Document.Current.BranchID, Document.Current.CustomerLocationID, Document.Current.OrderDate, GetDefaultSODiscountCalculationOptions(Document.Current, true));
				RefreshTotalsAndFreeItems(sender);
			}

					//Acuminator disable once PX1045 PXGraphCreateInstanceInEventHandlers 
					if (_discountEngine.SetExternalManualDocDiscount(Transactions.Cache, Transactions, DiscountDetails, discountDetail, null, GetDefaultSODiscountCalculationOptions(Document.Current, true)))
					RecalculateTotalDiscount();
			}
				finally
				{
					Document.Current.DeferPriceDiscountRecalculation = soordertype.Current.DeferPriceDiscountRecalculation;
				}
			}

			if (discountDetail != null && discountDetail.DiscountID != null && discountDetail.DiscountSequenceID != null && discountDetail.Description == null)
			{
				object description = null;
				sender.RaiseFieldDefaulting<SOOrderDiscountDetail.description>(discountDetail, out description);
				sender.SetValue<SOOrderDiscountDetail.description>(discountDetail, description);
			}
		}

		protected virtual void SOOrderDiscountDetail_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			SOOrderDiscountDetail discountDetail = (SOOrderDiscountDetail)e.Row;
			SOOrderDiscountDetail oldDiscountDetail = (SOOrderDiscountDetail)e.OldRow;
			//Acuminator disable once PX1045 PXGraphCreateInstanceInEventHandlers 
			if (!_discountEngine.IsInternalDiscountEngineCall && discountDetail != null)
			{
				try
				{
					Document.Current.DeferPriceDiscountRecalculation = false;
				if (!sender.ObjectsEqual<SOOrderDiscountDetail.skipDiscount>(e.Row, e.OldRow))
				{
						//Acuminator disable once PX1045 PXGraphCreateInstanceInEventHandlers 
						_discountEngine.UpdateDocumentDiscount(Transactions.Cache, Transactions, DiscountDetails, Document.Current.BranchID, Document.Current.CustomerLocationID, Document.Current.OrderDate, discountDetail.Type != DiscountType.Document, GetDefaultSODiscountCalculationOptions(Document.Current, true));
					RefreshTotalsAndFreeItems(sender);
				}
				if (!sender.ObjectsEqual<SOOrderDiscountDetail.discountID>(e.Row, e.OldRow) || !sender.ObjectsEqual<SOOrderDiscountDetail.discountSequenceID>(e.Row, e.OldRow))
				{
						//Acuminator disable once PX1045 PXGraphCreateInstanceInEventHandlers 
						_discountEngine.UpdateManualDocGroupDiscount(Transactions.Cache, Transactions, DiscountDetails, discountDetail, discountDetail.DiscountID, sender.ObjectsEqual<SOOrderDiscountDetail.discountID>(e.Row, e.OldRow) ? discountDetail.DiscountSequenceID : null, Document.Current.BranchID, Document.Current.CustomerLocationID, Document.Current.OrderDate, GetDefaultSODiscountCalculationOptions(Document.Current, true));
					RefreshTotalsAndFreeItems(sender);
				}

					//Acuminator disable once PX1045 PXGraphCreateInstanceInEventHandlers 
					if (_discountEngine.SetExternalManualDocDiscount(Transactions.Cache, Transactions, DiscountDetails, discountDetail, oldDiscountDetail, GetDefaultSODiscountCalculationOptions(Document.Current, true)))
					RecalculateTotalDiscount();
			}
				finally
				{
					Document.Current.DeferPriceDiscountRecalculation = soordertype.Current.DeferPriceDiscountRecalculation;
				}
			}
		}

		protected virtual void SOOrderDiscountDetail_RowDeleted(PXCache sender, PXRowDeletedEventArgs e)
		{
			SOOrderDiscountDetail discountDetail = (SOOrderDiscountDetail)e.Row;
			//Acuminator disable once PX1045 PXGraphCreateInstanceInEventHandlers 
			if (!_discountEngine.IsInternalDiscountEngineCall && discountDetail != null && !DisableGroupDocDiscount)
			{
				try
				{
					Document.Current.DeferPriceDiscountRecalculation = false;
					//Acuminator disable once PX1045 PXGraphCreateInstanceInEventHandlers 
					_discountEngine.UpdateDocumentDiscount(Transactions.Cache, Transactions, DiscountDetails, Document.Current.BranchID, Document.Current.CustomerLocationID, Document.Current.OrderDate, (discountDetail.Type != null && discountDetail.Type != DiscountType.Document && discountDetail.Type != DiscountType.ExternalDocument), GetDefaultSODiscountCalculationOptions(Document.Current, true));
				}
				finally
				{
					Document.Current.DeferPriceDiscountRecalculation = soordertype.Current.DeferPriceDiscountRecalculation;
				}
			}
			//Acuminator disable once PX1045 PXGraphCreateInstanceInEventHandlers 
			if (_discountEngine.IsInternalDiscountEngineCall && Document.Current != null && Document.Current.DisableAutomaticDiscountCalculation == true)
			{
				Document.Cache.RaiseExceptionHandling<SOOrder.disableAutomaticDiscountCalculation>(Document.Current, Document.Current.DisableAutomaticDiscountCalculation, new PXSetPropertyException(Messages.OneOrMoreDiscountsDeleted, PXErrorLevel.Warning));
			}
			RefreshTotalsAndFreeItems(sender);
		}

		protected virtual void SOOrderDiscountDetail_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			SOOrderDiscountDetail discountDetail = (SOOrderDiscountDetail)e.Row;

			bool isExternalDiscount = discountDetail.Type == DiscountType.ExternalDocument;

			PXDefaultAttribute.SetPersistingCheck<SOOrderDiscountDetail.discountID>(sender, discountDetail, isExternalDiscount ? PXPersistingCheck.Nothing : PXPersistingCheck.NullOrBlank);
			PXDefaultAttribute.SetPersistingCheck<SOOrderDiscountDetail.discountSequenceID>(sender, discountDetail, isExternalDiscount ? PXPersistingCheck.Nothing : PXPersistingCheck.NullOrBlank);
		}
		#endregion

		#region ARPaymentTotals events
		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXDBDefault(typeof(SOOrder.orderNbr), PersistingCheck = PXPersistingCheck.Nothing)]
		protected virtual void _(Events.CacheAttached<ARPaymentTotals.adjdOrderNbr> eventArgs)
		{
		}
		#endregion

		[PXDefault()]
		[PXUIFieldAttribute(DisplayName = "Customer Tax Zone", Enabled = false)]
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		protected virtual void SOTaxTran_TaxZoneID_CacheAttached(PXCache sender)
		{
		}

		protected virtual void SOTaxTran_TaxZoneID_ExceptionHandling(PXCache sender, PXExceptionHandlingEventArgs e)
		{
			Exception ex = e.Exception as PXSetPropertyException;
			if (ex != null)
			{
				Document.Cache.RaiseExceptionHandling<SOOrder.taxZoneID>(Document.Current, null, ex);
			}
		}

		protected virtual void SOTaxTran_TaxZoneID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (Document.Current != null && Document.Current.Behavior != SOBehavior.BL)
			{
				e.NewValue = Document.Current.TaxZoneID;
				e.Cancel = true;
			}
		}

		protected virtual void SOTaxTran_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			if (!(e.Row is SOTaxTran soTaxTran))
				return;

			PXUIFieldAttribute.SetEnabled<SOTaxTran.taxID>(sender, e.Row, sender.GetStatus(e.Row) == PXEntryStatus.Inserted);			
		}

		protected virtual void _(Events.RowUpdated<SOShippingAddress> e)
		{
			SOShippingAddress row = e.Row ;
			SOShippingAddress oldRow = e.OldRow;

			if (row == null) return;

			if (oldRow?.CountryID != row.CountryID || oldRow?.PostalCode != row.PostalCode || oldRow?.State != row.State)
			{
				Document.Cache.SetDefaultExt<SOOrder.taxZoneID>(Document.Current);
			}
		}

		protected virtual void AddInvoiceFilter_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			invoicesplits.Cache.Clear();
		}


		#region SOPackageInfo
		protected virtual void SOPackageInfoEx_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			SOPackageInfo row = e.Row as SOPackageInfo;
			if (row != null && Document.Current != null)
			{
				row.WeightUOM = commonsetup.Current.WeightUOM;
				PXUIFieldAttribute.SetEnabled<SOPackageInfo.inventoryID>(sender, e.Row, Document.Current.IsManualPackage == true);
				PXUIFieldAttribute.SetEnabled<SOPackageInfo.siteID>(sender, e.Row, Document.Current.IsManualPackage == true);
			}
		}

		protected virtual void SOPackageInfoEx_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			SOPackageInfo row = e.Row as SOPackageInfo;
			if (row != null)
			{
				PXDefaultAttribute.SetPersistingCheck<SOPackageInfo.siteID>(sender, row, PXAccess.FeatureInstalled<FeaturesSet.inventory>() ? PXPersistingCheck.NullOrBlank : PXPersistingCheck.Nothing);
			}
		}
		protected virtual void SOPackageInfoEx_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			SOPackageInfo row = e.Row as SOPackageInfo;
			if (row != null)
			{
				CSBox box = PXSelect<CSBox, Where<CSBox.boxID, Equal<Required<CSBox.boxID>>>>.Select(this, row.BoxID);
				if (box != null && box.MaxWeight < row.GrossWeight)
				{
					sender.RaiseExceptionHandling<SOPackageInfo.grossWeight>(row, row.GrossWeight, new PXSetPropertyException(Messages.WeightExceedsBoxSpecs));
				}
			}
		}
		#endregion

		protected readonly string viewInventoryID;

		#region Discount

		protected virtual void RecalculateDiscounts(PXCache sender, SOLine line)
		{
			RecalculateDiscounts(sender, line, null);
		}

		protected virtual void RecalculateDiscounts(PXCache sender, SOLine line, SOLine oldline)
		{
			if (PXAccess.FeatureInstalled<FeaturesSet.customerDiscounts>() && 
			    (line.InventoryID != null || (sender.Graph.IsImportFromExcel && line.SkipDisc != true)) && line.Qty != null && line.CuryLineAmt != null && 
			    (line.IsFree != true || (oldline != null && !sender.ObjectsEqual<SOLine.isFree>(line, oldline))))
			{
				DiscountEngine.DiscountCalculationOptions discountCalculationOptions = GetDefaultSODiscountCalculationOptions(Document.Current);
				if (line.CalculateDiscountsOnImport == true)
					discountCalculationOptions = discountCalculationOptions | DiscountEngine.DiscountCalculationOptions.CalculateDiscountsFromImport;

				//Acuminator disable once PX1045 PXGraphCreateInstanceInEventHandlers 
				_discountEngine.SetDiscounts(
					sender, 
					Transactions, 
					line, 
					DiscountDetails, 
					Document.Current.BranchID, 
					Document.Current.CustomerLocationID, 
					Document.Current.CuryID, 
					Document.Current.OrderDate,
					recalcdiscountsfilter.Current,
					discountCalculationOptions);

				RecalculateTotalDiscount();

				RefreshFreeItemLines(sender);
			}
			else if (!PXAccess.FeatureInstalled<FeaturesSet.customerDiscounts>() && Document.Current != null)
			{
				//Acuminator disable once PX1045 PXGraphCreateInstanceInEventHandlers 
				_discountEngine.CalculateDocumentDiscountRate(Transactions.Cache, Transactions, line, DiscountDetails);
			}

		}

		public virtual DiscountEngine.DiscountCalculationOptions GetDefaultSODiscountCalculationOptions(SOOrder doc)
		{
			return GetDefaultSODiscountCalculationOptions(doc, false);
		}

		public virtual DiscountEngine.DiscountCalculationOptions GetDefaultSODiscountCalculationOptions(SOOrder doc, bool doNotDeferDiscountCalculation)
		{
			var options = DiscountEngine.DefaultARDiscountCalculationParameters | (DisableGroupDocDiscount ? DiscountEngine.DiscountCalculationOptions.DisableGroupAndDocumentDiscounts : DiscountEngine.DiscountCalculationOptions.CalculateAll) |
					((doc != null && doc.DisableAutomaticDiscountCalculation == true) ? DiscountEngine.DiscountCalculationOptions.DisableAllAutomaticDiscounts : DiscountEngine.DiscountCalculationOptions.CalculateAll);

			if (doc.DeferPriceDiscountRecalculation == true && doNotDeferDiscountCalculation == false)
			{
				doc.IsPriceAndDiscountsValid = false;
				return options | DiscountEngine.DiscountCalculationOptions.DisablePriceCalculation | DiscountEngine.DiscountCalculationOptions.DisableGroupAndDocumentDiscounts | DiscountEngine.DiscountCalculationOptions.DisableARDiscountsCalculation | DiscountEngine.DiscountCalculationOptions.DisableFreeItemDiscountsCalculation;
			}

			return options;
		}

		protected virtual void RefreshFreeItemLines(PXCache sender)
		{
			if (sender.Graph.IsCopyPasteContext || sender.Graph.IsImportFromExcel)
				return;

			Dictionary<int, decimal> groupedByInventory = new Dictionary<int, decimal>();
			Dictionary<int, string> baseUomOfInventories = new Dictionary<Int32, String>();

			PXSelectBase<SOOrderDiscountDetail> select =
				new PXSelectJoin<SOOrderDiscountDetail,
				InnerJoin<InventoryItem, On<SOOrderDiscountDetail.FK.FreeInventoryItem>>,
				Where<SOOrderDiscountDetail.orderType, Equal<Current<SOOrder.orderType>>,
				And<SOOrderDiscountDetail.orderNbr, Equal<Current<SOOrder.orderNbr>>,
				And<SOOrderDiscountDetail.skipDiscount, NotEqual<boolTrue>>>>>(this);

			foreach (PXResult<SOOrderDiscountDetail, InventoryItem> row in select.Select())
			{
				SOOrderDiscountDetail discountDetail = row;
				InventoryItem item = row;

				if (discountDetail.FreeItemID != null)
				{
					if (groupedByInventory.ContainsKey(discountDetail.FreeItemID.Value))
					{
						groupedByInventory[discountDetail.FreeItemID.Value] += discountDetail.FreeItemQty ?? 0;
					}
					else
					{
						groupedByInventory.Add(discountDetail.FreeItemID.Value, discountDetail.FreeItemQty ?? 0);
						baseUomOfInventories.Add(item.InventoryID.Value, item.BaseUnit);
					}

				}

			}

			bool refreshView = false;

			#region Delete Unvalid FreeItems
			foreach (SOLine line in FreeItems.Select())
			{
				if (line.ManualDisc == false && line.InventoryID != null)
				{
					if (line.ShippedQty == 0m)
					{
						if (groupedByInventory.ContainsKey(line.InventoryID.Value))
						{
							if (groupedByInventory[line.InventoryID.Value] == 0)
							{
								FreeItems.Delete(line);
								refreshView = true;
							}
						}
						else
						{
							FreeItems.Delete(line);
							refreshView = true;
						}
					}
					else
					{
						PXUIFieldAttribute.SetWarning<SOLine.orderQty>(FreeItems.Cache, line, Messages.CannotRecalculateFreeItemQuantity);
						refreshView = true;
					}
				}
			}

			#endregion

			int? defaultWarehouse = GetDefaultWarehouse();
			foreach (KeyValuePair<int, decimal> kv in groupedByInventory)
			{
				SOLine currentLine = this.Transactions.Current;
				SOLine freeLine = GetFreeLineByItemID(kv.Key);

				if (freeLine == null)
				{
					if (kv.Value > 0)
					{
						SOLine line = new SOLine();
						line.InventoryID = kv.Key;
						line.IsFree = true;
						line.SiteID = defaultWarehouse;
						line.OrderQty = kv.Value;

						if (arsetup.Current.ApplyQuantityDiscountBy == ApplyQuantityDiscountType.BaseUOM)
							line.UOM = baseUomOfInventories[line.InventoryID.Value];

						line = FreeItems.Insert(line);

						refreshView = true;
					}
				}
				else
				{
					if (freeLine.ShippedQty == 0m)
					{
						if (freeLine.OrderQty != kv.Value)
						{
							SOLine copy = PXCache<SOLine>.CreateCopy(freeLine);
							copy.OrderQty = kv.Value;
							FreeItems.Cache.Update(copy);

							refreshView = true;
						}
					}
					else
					{
						PXUIFieldAttribute.SetWarning<SOLine.orderQty>(FreeItems.Cache, freeLine, Messages.CannotRecalculateFreeItemQuantity);
						refreshView = true;
					}
				}
				if (currentLine != null && currentLine != this.Transactions.Current)
				{
					this.Transactions.Current = currentLine;
				}
			}

			if (refreshView)
			{
				Transactions.View.RequestRefresh();
			}
		}

		private SOLine GetFreeLineByItemID(int? inventoryID)
		{
			return PXSelect<SOLine,
				Where<SOLine.orderType, Equal<Current<SOOrder.orderType>>,
				And<SOLine.orderNbr, Equal<Current<SOOrder.orderNbr>>,
				And<SOLine.isFree, Equal<boolTrue>,
				And<SOLine.inventoryID, Equal<Required<SOLine.inventoryID>>,
				And<SOLine.manualDisc, Equal<boolFalse>>>>>>>.Select(this, inventoryID);
		}

		private void ResetQtyOnFreeItem(PXCache sender, SOLine line)
		{
			PXSelectBase<SOOrderDiscountDetail> select = new PXSelect<SOOrderDiscountDetail,
				Where<SOOrderDiscountDetail.orderType, Equal<Current<SOOrder.orderType>>,
				And<SOOrderDiscountDetail.orderNbr, Equal<Current<SOOrder.orderNbr>>,
				And<SOOrderDiscountDetail.freeItemID, Equal<Required<SOOrderDiscountDetail.freeItemID>>>>>>(this);

			decimal? qtyTotal = 0;
			foreach (SOOrderDiscountDetail item in select.Select(line.InventoryID))
			{
				if (item.SkipDiscount != true && item.FreeItemID != null && item.FreeItemQty != null && item.FreeItemQty.Value > 0)
				{
					qtyTotal += item.FreeItemQty.Value;
				}
			}

			sender.SetValueExt<SOLine.orderQty>(line, qtyTotal);
		}

		/// <summary>
		/// If all lines are from one site/warehouse - return this warehouse otherwise null;
		/// </summary>
		/// <returns>Default Wartehouse for Free Item</returns>
		private int? GetDefaultWarehouse()
		{
			PXResultset<SOOrderSite> osites = PXSelectJoin<SOOrderSite,
				InnerJoin<INSite, 
					On<SOOrderSite.FK.Site>>,
				Where<SOOrderSite.orderType, Equal<Current<SOOrder.orderType>>,
					And<SOOrderSite.orderNbr, Equal<Current<SOOrder.orderNbr>>,
					And<Match<INSite, Current<AccessInfo.userName>>>>>>.Select(this);

			if (osites.Count == 1)
			{
				return ((SOOrderSite)osites).SiteID;
			}
			return null;
		}

		private void RecalculateTotalDiscount()
		{
			if (Document.Current != null && Document.Cache.GetStatus(Document.Current) != PXEntryStatus.Deleted && Document.Cache.GetStatus(Document.Current) != PXEntryStatus.InsertedDeleted)
			{
				SOOrder copy = PXCache<SOOrder>.CreateCopy(Document.Current);
				var discountTotals = _discountEngine.GetDiscountTotals(DiscountDetails);

				//Acuminator disable once PX1045 PXGraphCreateInstanceInEventHandlers 
				Document.Cache.SetValueExt<SOOrder.curyGroupDiscTotal>(Document.Current, discountTotals.groupDiscountTotal);
				//Acuminator disable once PX1045 PXGraphCreateInstanceInEventHandlers 
				Document.Cache.SetValueExt<SOOrder.curyDocumentDiscTotal>(Document.Current, discountTotals.documentDiscountTotal);
				//Acuminator disable once PX1045 PXGraphCreateInstanceInEventHandlers 
				Document.Cache.SetValueExt<SOOrder.curyDiscTot>(Document.Current, discountTotals.discountTotal);

				Document.Cache.RaiseRowUpdated(Document.Current, copy);
			}
		}

		private void RefreshTotalsAndFreeItems(PXCache sender)
		{
			RecalculateTotalDiscount();
			RefreshFreeItemLines(sender);
		}
		#endregion

		#region Carrier Freight Cost

		protected virtual bool CollectFreight
		{
			get
			{
				if (DocumentProperties.Current != null)
				{
					if (DocumentProperties.Current.UseCustomerAccount == true)
						return false;

					if (DocumentProperties.Current.GroundCollect == true && this.CanUseGroundCollect(DocumentProperties.Current))
						return false;
				}

				return true;
			}
		}

		private void CalculateFreightCost(bool supressErrors)
		{
			if (Document.Current.ShipVia != null)
			{
				Carrier carrier = Carrier.PK.Find(this, Document.Current.ShipVia);
				if (carrier != null && carrier.IsExternal == true)
				{
					CarrierPlugin plugin = CarrierPlugin.PK.Find(this, carrier.CarrierPluginID);
					CarrierResult<ICarrierService> serviceResult = CarrierPluginMaint.CreateCarrierService(this, plugin, true);
					ICarrierService cs = serviceResult.Result;
					cs.Method = carrier.PluginMethod;

					CarrierRequest cr = CarrierRatesExt.BuildRateRequest(Document.Current);
					CarrierResult<RateQuote> result = cs.GetRateQuote(cr);

					if (result != null)
					{
						StringBuilder sb = new StringBuilder();
						foreach (Message message in result.Messages)
						{
							sb.AppendFormat("{0}:{1} ", message.Code, message.Description);
						}

						if (result.IsSuccess)
						{
							decimal baseCost = ConvertAmtToBaseCury(result.Result.Currency, arsetup.Current.DefaultRateTypeID, Document.Current.OrderDate.Value, result.Result.Amount);
							SetFreightCost(baseCost);

							//show warnings:
                            if (result.Messages.Count > 0)
							{
                                if (!supressErrors)
								Document.Cache.RaiseExceptionHandling<SOOrder.curyFreightCost>(Document.Current, Document.Current.CuryFreightCost,
									new PXSetPropertyException(sb.ToString(), PXErrorLevel.Warning));
                                else
                                    PXTrace.WriteWarning(sb.ToString());
							}
						}
						else
						{
                            if (!supressErrors)
						{
							Document.Cache.RaiseExceptionHandling<SOOrder.curyFreightCost>(Document.Current, Document.Current.CuryFreightCost,
									new PXSetPropertyException(Messages.CarrierServiceError, PXErrorLevel.Error, sb.ToString()));

							throw new PXException(Messages.CarrierServiceError, sb.ToString());
						}
                            else
                            {
                                PXTrace.WriteError(string.Format(Messages.CarrierServiceError, sb.ToString()));
                            }
						}

					}
				}
			}
		}

		public virtual FreightCalculator CreateFreightCalculator()
		{
			return new FreightCalculator(this);
		}

		protected virtual void SetFreightCost(decimal baseCost)
		{
			SOOrder copy = (SOOrder)Document.Cache.CreateCopy(Document.Current);

			if (soordertype.Current != null && soordertype.Current.CalculateFreight == false)
			{
				copy.FreightCost = 0;
				CM.PXCurrencyAttribute.CuryConvCury<SOOrder.curyFreightCost>(Document.Cache, copy);
			}
			else
			{
				if (!CollectFreight)
					baseCost = 0;

				copy.FreightCost = baseCost;
				CM.PXCurrencyAttribute.CuryConvCury<SOOrder.curyFreightCost>(Document.Cache, copy);
				if (copy.OverrideFreightAmount != true)
				{
					PXResultset<SOLine> res = Transactions.Select();
					FreightCalculator fc = CreateFreightCalculator();
					fc.ApplyFreightTerms<SOOrder, SOOrder.curyFreightAmt>(Document.Cache, copy, res.Count);
				}
			}

			copy = (SOOrder)Document.Update(copy);
			copy.FreightCostIsValid = true;//FreightCostIsValid field is clearing in SOOrder_RowUpdated when FreightCost is changed
			Document.Update(copy);
		}

		private decimal ConvertAmtToBaseCury(string from, string rateType, DateTime effectiveDate, decimal amount)
		{
			decimal result = amount;

			using (ReadOnlyScope rs = new ReadOnlyScope(DummyCuryInfo.Cache))
			{
				CurrencyInfo ci = new CurrencyInfo();
				ci.CuryRateTypeID = rateType;
				ci.CuryID = from;
				object newval;
				DummyCuryInfo.Cache.RaiseFieldDefaulting<CurrencyInfo.baseCuryID>(ci, out newval);
				DummyCuryInfo.Cache.SetValue<CurrencyInfo.baseCuryID>(ci, newval);

				DummyCuryInfo.Cache.RaiseFieldDefaulting<CurrencyInfo.basePrecision>(ci, out newval);
				DummyCuryInfo.Cache.SetValue<CurrencyInfo.basePrecision>(ci, newval);

				DummyCuryInfo.Cache.RaiseFieldDefaulting<CurrencyInfo.curyPrecision>(ci, out newval);
				DummyCuryInfo.Cache.SetValue<CurrencyInfo.curyPrecision>(ci, newval);

				DummyCuryInfo.Cache.RaiseFieldDefaulting<CurrencyInfo.curyRate>(ci, out newval);
				DummyCuryInfo.Cache.SetValue<CurrencyInfo.curyRate>(ci, newval);

				DummyCuryInfo.Cache.RaiseFieldDefaulting<CurrencyInfo.recipRate>(ci, out newval);
				DummyCuryInfo.Cache.SetValue<CurrencyInfo.recipRate>(ci, newval);

				DummyCuryInfo.Cache.RaiseFieldDefaulting<CurrencyInfo.curyMultDiv>(ci, out newval);
				DummyCuryInfo.Cache.SetValue<CurrencyInfo.curyMultDiv>(ci, newval);

				ci.SetCuryEffDate(DummyCuryInfo.Cache, effectiveDate);
				PXCurrencyAttribute.CuryConvBase(DummyCuryInfo.Cache, ci, amount, out result);
			}

			return result;
		}

		#endregion

		#region External Tax

		/// <summary>
		/// <see cref="ExternalTaxBase{TGraph, TPrimary}.IsExternalTax(string)"/>
		/// </summary>
		public virtual bool IsExternalTax(string TaxZoneID)
		{
			return false;
		}

		public virtual SOOrder CalculateExternalTax(SOOrder order)
		{
			return order;
		}

		public bool RecalculateExternalTaxesSync { get; set; }

		protected virtual void RecalculateExternalTaxes()
		{
		}

		protected virtual void InsertImportedTaxes()
		{
		}

		#endregion

		public virtual PXResultset<SOOrderType> GetOrderShipments(PXGraph docgraph, SOOrder order) =>
			 PXSelectReadonly2<SOOrderType,
						LeftJoin<SOOrderShipment,
							On2<SOOrderShipment.FK.OrderType,
								And<SOOrderShipment.orderNbr, Equal<Required<SOOrder.orderNbr>>,
								And<SOOrderShipment.confirmed, Equal<True>,
								And<SOOrderShipment.invoiceNbr, IsNull>>>>,
					  LeftJoin<SOOrderTypeOperation,
									On2<SOOrderTypeOperation.FK.OrderType,
									And<Where2<Where<SOOrderShipment.operation, IsNull,
													And<SOOrderTypeOperation.operation, Equal<SOOrderType.defaultOperation>>>,
												  Or<Where<SOOrderTypeOperation.operation, Equal<SOOrderShipment.operation>>>>>>,
						CrossJoin<CurrencyInfo, CrossJoin<SOAddress, CrossJoin<SOContact, CrossJoin<Customer>>>>>>,
						Where<SOOrderType.orderType, Equal<Required<SOOrder.orderType>>,
							And<CurrencyInfo.curyInfoID, Equal<Required<SOOrder.curyInfoID>>,
							And<SOAddress.addressID, Equal<Required<SOOrder.billAddressID>>,
							And<SOContact.contactID, Equal<Required<SOOrder.billContactID>>,
							And<Customer.bAccountID, Equal<Required<SOOrder.customerID>>>>>>>>
							.Select(docgraph, order.OrderNbr, order.OrderType, order.CuryInfoID, order.BillAddressID, order.BillContactID, order.CustomerID);

		public virtual void InvoiceOrder(Dictionary<string, object> parameters, IEnumerable<SOOrder> list, InvoiceList created, bool isMassProcess, PXQuickProcess.ActionFlow quickProcessFlow, bool groupByCustomerOrderNumber)
		{
			SOShipmentEntry docgraph = PXGraph.CreateInstance<SOShipmentEntry>();
			SOInvoiceEntry ie = PXGraph.CreateInstance<SOInvoiceEntry>();

			foreach (SOOrder order in list.OrderBy(o => o.OrderType).ThenBy(o => o.OrderNbr))
			{
				try
				{
					if (isMassProcess) PXProcessing<SOOrder>.SetCurrentItem(order);

					ie.Clear();
					ie.Clear(PXClearOption.ClearQueriesOnly);
					ie.ARSetup.Current.RequireControlTotal = false;

					List<PXResult<SOOrderShipment>> shipments = new List<PXResult<SOOrderShipment>>();
					PXResultset<SOShipLine, SOLine> details = null;

					foreach (PXResult<SOOrderType, SOOrderShipment, SOOrderTypeOperation, CurrencyInfo, SOAddress, SOContact, Customer> res in
						GetOrderShipments(docgraph, order))
					{
						SOOrderShipment shipment = (SOOrderShipment)res;

						if (((SOOrderType)res).RequireShipping == false || ((SOOrderTypeOperation)res).INDocType == INTranType.NoUpdate)
						{
							//if order is created with zero lines, invoiced, and then new line added, this will save us
							if (shipment.ShipmentNbr == null)
							{
								shipment = SOOrderShipment.FromSalesOrder(order);
								shipment.ShipmentType = INTranType.DocType(((SOOrderTypeOperation)res).INDocType);
							}

							if (details == null)
							{
								details = new PXResultset<SOShipLine, SOLine>();
							}

							foreach (SOLine line in PXSelectJoin<SOLine, 
								InnerJoin<InventoryItem, 
									On<SOLine.FK.InventoryItem>>, 
								Where<SOLine.orderType, Equal<Required<SOLine.orderType>>, 
								And<SOLine.orderNbr, Equal<Required<SOLine.orderNbr>>, 
								And<SOLine.lineType, NotEqual<SOLineType.miscCharge>>>>>.Select(docgraph, order.OrderType, order.OrderNbr))
							{
								details.Add(new PXResult<SOShipLine, SOLine>(SOShipLine.FromSOLine(line), line));
							}
						}
						else if (HasMiscLinesToInvoice(order) && shipment.ShipmentNbr == null)
						{
							shipment = SOOrderShipment.FromSalesOrder(order, miscOnly: true);
							shipment.ShipmentType = INDocType.Invoice;
						}

						if (shipment.ShipmentType == SOShipmentType.DropShip)
						{
							details = details ?? new PXResultset<SOShipLine, SOLine>();
							details.AddRange(docgraph.CollectDropshipDetails(shipment));
						}

						if (shipment.ShipmentNbr != null)
						{
							shipments.Add(new PXResult<SOOrderShipment, SOOrder, CurrencyInfo, SOAddress, SOContact, SOOrderType, SOOrderTypeOperation, Customer>(shipment, order, (CurrencyInfo)res, (SOAddress)res, (SOContact)res, (SOOrderType)res, (SOOrderTypeOperation)res, (Customer)res));
						}
					}

					shipments = new List<PXResult<SOOrderShipment>>(shipments.OrderBy(s => PXResult.Unwrap<SOOrderShipment>(s).Operation == PXResult.Unwrap<SOOrderType>(s).DefaultOperation ? 0 : 1)
						.ThenBy(s => PXResult.Unwrap<SOOrderShipment>(s).ShipmentNbr));

					foreach (PXResult<SOOrderShipment, SOOrder, CurrencyInfo, SOAddress, SOContact, SOOrderType, SOOrderTypeOperation> res in shipments)
					{
						
						Clear();
						var soorder = (SOOrder)res;
						Document.Current = Document.Search<SOOrder.orderNbr>(soorder.OrderNbr, soorder.OrderType);
						if (PX.SM.WorkflowAction.HasWorkflowActionEnabled(this, g => g.prepareInvoice, Document.Current) == false)
						{
							throw new PXInvalidOperationException(Messages.ActionNotAvailableInCurrentState,
								prepareInvoice.GetCaption(), Document.Cache.GetRowDescription(Document.Current));
						}

						using (var ts = new PXTransactionScope())
						{
							ie.InvoiceOrder(new InvoiceOrderArgs(res)
							{
								InvoiceDate = ie.Accessinfo.BusinessDate.Value,
								Customer = customer.Current,
								List = created,
								Details = details,
								QuickProcessFlow = quickProcessFlow,
								GroupByDefaultOperation = !isMassProcess,
								GroupByCustomerOrderNumber = groupByCustomerOrderNumber
							});

							Clear();
							ts.Complete();
						}

						PXProcessing<SOOrder>.SetProcessed();
					}
				}
				catch (Exception ex) when (isMassProcess)
				{
					PXProcessing<SOOrder>.SetError(ex);
				}
			}
		}

		public virtual void PostOrder(INIssueEntry docgraph, SOOrder order, DocumentList<INRegister> list, SOOrderShipment orderShipment)
		{
			this.Clear();
			docgraph.Clear();

			bool isOrderShipmentPassed = (orderShipment != null);
			var reattachedPlans = new List<INItemPlan>();
			using (docgraph.TranSplitPlanExt.ReleaseModeScope())
			{
				docgraph.insetup.Current.HoldEntry = false;
				docgraph.insetup.Current.RequireControlTotal = false;

				Document.Current = Document.Search<SOOrder.orderNbr>(order.OrderNbr, order.OrderType);

				if (!isOrderShipmentPassed)
				{
					orderShipment = PXSelect<SOOrderShipment,
						Where<SOOrderShipment.orderType, Equal<Current<SOOrder.orderType>>,
						And<SOOrderShipment.orderNbr, Equal<Current<SOOrder.orderNbr>>,
						And<SOOrderShipment.invtRefNbr, IsNull>>>>.Select(this, new object[] { order });
				}

				//TODO: Temporary solution. Review when AC-80210 is fixed
				if (orderShipment != null && orderShipment.ShipmentType != SOShipmentType.DropShip && orderShipment.ShipmentNbr != Constants.NoShipmentNbr && orderShipment.Confirmed != true)
				{
					throw new PXException(Messages.UnableToProcessUnconfirmedShipment, orderShipment.ShipmentNbr);
				}

				ARRegister ardoc = null;
				if (orderShipment != null)
				{
					ardoc = PXSelect<ARRegister, Where<ARRegister.docType, Equal<Required<ARRegister.docType>>, And<ARRegister.refNbr, Equal<Required<ARRegister.refNbr>>>>>.Select(this, orderShipment.InvoiceType, orderShipment.InvoiceNbr);
				}

				INRegister newdoc = new INRegister()
				{
					BranchID = ardoc?.BranchID ?? order.BranchID,
					DocType = INTranType.DocType(sooperation.Current.INDocType),
					SiteID = null,
					TranDate = ardoc?.DocDate,
					FinPeriodID = ardoc?.FinPeriodID,
					OrigModule = GL.BatchModule.SO,
				};
				if (docgraph.issue.Insert(newdoc) == null)
				{
					return;
				}

				SOLine prev_line = null;
				INTran newline = null;

				string arDocType =
					order.ARDocType == ARDocType.InvoiceOrCreditMemo ? (order.CuryOrderTotal >= 0m ? ARDocType.Invoice : ARDocType.CreditMemo)
					: order.ARDocType == ARDocType.CashSaleOrReturn ? (order.CuryOrderTotal >= 0m ? ARDocType.CashSale : ARDocType.CashReturn)
					: order.ARDocType;

				foreach (PXResult<SOLine, SOLineSplit, ARTran, INTran, INItemPlan> res in
					PXSelectJoin<SOLine,
					LeftJoin<SOLineSplit, On<SOLineSplit.FK.OrderLine>,
					LeftJoin<ARTran, On<ARTran.sOOrderType, Equal<SOLine.orderType>, And<ARTran.sOOrderNbr, Equal<SOLine.orderNbr>, And<ARTran.sOOrderLineNbr, Equal<SOLine.lineNbr>, And<ARTran.lineType, Equal<SOLine.lineType>>>>>,
					LeftJoin<INTran, On<INTran.sOOrderType, Equal<SOLine.orderType>, And<INTran.sOOrderNbr, Equal<SOLine.orderNbr>, And<INTran.sOOrderLineNbr, Equal<SOLine.lineNbr>>>>,
					LeftJoin<INItemPlan, On<INItemPlan.planID, Equal<SOLineSplit.planID>>>>>>,
				Where<SOLine.orderType, Equal<Current<SOOrder.orderType>>, And<SOLine.orderNbr, Equal<Current<SOOrder.orderNbr>>, And<SOLine.lineType, Equal<SOLineType.inventory>,
					And2<Where<Required<ARTran.tranType>, Equal<ARDocType.noUpdate>,
							Or<ARTran.tranType, Equal<Required<ARTran.tranType>>,
								And<ARTran.refNbr, IsNotNull>>>, And<INTran.refNbr, IsNull>>>>>,
				OrderBy<Asc<SOLine.orderType, Asc<SOLine.orderNbr, Asc<SOLine.lineNbr>>>>>.Select(this, arDocType, arDocType))
				{
					SOLine line = res;
					SOLineSplit split = ((SOLineSplit)res).SplitLineNbr != null ? (SOLineSplit)res : (SOLineSplit)line;
					INItemPlan plan = res;
					INPlanType plantype = INPlanType.PK.Find(this, plan.PlanType) ?? new INPlanType();
					ARTran artran = res;

					//avoid ReadItem()
					if (plan.PlanID != null)
					{
						Caches[typeof(INItemPlan)].SetStatus(plan, PXEntryStatus.Notchanged);
					}

					bool reattachExistingPlan = false;
					if (plantype.DeleteOnEvent == true)
					{
						reattachExistingPlan = true;
						Caches[typeof(SOLineSplit)].MarkUpdated(split, assertError: true);
						split = (SOLineSplit)Caches[typeof(SOLineSplit)].Locate(split);
					if (split != null)
					{
						split.PlanID = null;
						split.Completed = true;
					}
						Caches[typeof(SOLineSplit)].IsDirty = true;
					}
					else if (string.IsNullOrEmpty(plantype.ReplanOnEvent) == false)
					{
						plan = PXCache<INItemPlan>.CreateCopy(plan);
						plan.PlanType = plantype.ReplanOnEvent;
						Caches[typeof(INItemPlan)].Update(plan);

						//split.Confirmed = true;
						Caches[typeof(SOLineSplit)].MarkUpdated(split, assertError: true);
						Caches[typeof(SOLineSplit)].IsDirty = true;
					}

				bool soLineChanged = !Caches[typeof(SOLine)].ObjectsEqual(prev_line, line);
				if (soLineChanged)
				{
					line.Completed = true;
					Transactions.Cache.MarkUpdated(line);
					Transactions.Cache.IsDirty = true;
				}
				bool isReturnSpecific = (line.Operation == SOOperation.Receipt)
					&& InventoryItem.PK.Find(this, line.InventoryID)?.ValMethod == INValMethod.Specific
					&& !string.IsNullOrEmpty(split.LotSerialNbr);
				if ((soLineChanged
					|| line.InventoryID != split.InventoryID
					|| isReturnSpecific)
					&& split.IsStockItem == true && line.Qty != 0)
					{
						newline = new INTran();
						newline.BranchID = artran.BranchID ?? line.BranchID;
						newline.TranType = line.TranType;
						newline.SOShipmentNbr = Constants.NoShipmentNbr;
						newline.SOShipmentType = docgraph.issue.Current.DocType;
						newline.SOShipmentLineNbr = null;
						newline.SOOrderType = line.OrderType;
						newline.SOOrderNbr = line.OrderNbr;
						newline.SOOrderLineNbr = line.LineNbr;
						newline.SOLineType = line.LineType;
						newline.ARDocType = artran.TranType;
						newline.ARRefNbr = artran.RefNbr;
						newline.ARLineNbr = artran.LineNbr;
						newline.AcctID = artran.AccountID;
						newline.SubID = artran.SubID;

					newline.IsStockItem = split.IsStockItem;
						newline.InventoryID = split.InventoryID;
						newline.SiteID = line.SiteID;
						newline.BAccountID = line.CustomerID;
						newline.InvtMult = line.OrderQty < 0m ? (short?)-line.InvtMult : line.InvtMult;
						newline.Qty = 0m;
						newline.ProjectID = line.ProjectID;
						newline.TaskID = line.TaskID;
						newline.CostCodeID = line.CostCodeID;
						newline.IsSpecialOrder = line.IsSpecialOrder;
						newline.IsComponentItem = line.InventoryID != split.InventoryID && line.IsKit == true;
						if (line.InventoryID != split.InventoryID)
						{
							newline.SubItemID = split.SubItemID;
							newline.UOM = split.UOM;
							newline.UnitPrice = 0m;
							newline.UnitCost = GetINTranUnitCost(line, split, false);
							newline.TranDesc = null;
						}
						else if (isReturnSpecific)
						{
							newline.SubItemID = split.SubItemID;
							newline.UOM = split.UOM;
							newline.UnitPrice = INUnitAttribute.ConvertFromBase(
							Transactions.Cache,
							artran.InventoryID,
							artran.UOM,
							artran.UnitPrice ?? 0m,
							INPrecision.UNITCOST);
							newline.UnitCost = GetINTranUnitCost(line, split, true);
							newline.TranDesc = line.TranDesc;
							newline.ReasonCode = line.ReasonCode;
						}
						else
						{
							newline.SubItemID = line.SubItemID;
							newline.UOM = line.UOM;
							newline.UnitPrice = artran.UnitPrice ?? 0m;
							newline.UnitCost = line.UnitCost;
							newline.TranDesc = line.TranDesc;
							newline.ReasonCode = line.ReasonCode;
						}

					docgraph.CostCenterDispatcherExt?.SetCostLayerType(newline);
						newline = docgraph.LineSplittingExt.lsselect.Insert(newline);
					}

					prev_line = line;

					if (split.IsStockItem == true)
					{
						if (split.Qty != 0)
						{
							INTranSplit newsplit = (INTranSplit)newline;
							newsplit.SplitLineNbr = null;
							newsplit.SubItemID = split.SubItemID;
							newsplit.LocationID = split.LocationID;
							newsplit.LotSerialNbr = split.LotSerialNbr;
							newsplit.ExpireDate = split.ExpireDate;
							newsplit.UOM = split.UOM;
							newsplit.Qty = split.Qty;
							newsplit.BaseQty = null;
							if (reattachExistingPlan)
							{
								newsplit.PlanID = plan.PlanID;
								reattachedPlans.Add(plan);
							}

							docgraph.splits.Insert(newsplit);
						}
						else
						{
							Caches[typeof(INItemPlan)].Delete(plan);
						}

						if (line.InventoryID == split.InventoryID && !isReturnSpecific && line.Qty != 0)
						{
							bool signMismatch = artran.DrCr == DrCr.Credit && artran.SOOrderLineOperation == SOOperation.Receipt
								|| artran.DrCr == DrCr.Debit && artran.SOOrderLineOperation == SOOperation.Issue;

							newline.TranCost = line.LineSign * line.ExtCost;
							newline.TranAmt = (signMismatch ? -artran.TranAmt : artran.TranAmt) ?? 0m;
						}
					}
					else if (plantype.DeleteOnEvent == true)
					{
						Caches[typeof(INItemPlan)].Delete(plan);
					}
				}
			}

			INRegister copy = PXCache<INRegister>.CreateCopy(docgraph.issue.Current);
			PXFormulaAttribute.CalcAggregate<INTran.qty>(docgraph.transactions.Cache, copy);
			PXFormulaAttribute.CalcAggregate<INTran.tranAmt>(docgraph.transactions.Cache, copy);
			PXFormulaAttribute.CalcAggregate<INTran.tranCost>(docgraph.transactions.Cache, copy);
			docgraph.issue.Update(copy);

			using (PXTransactionScope ts = new PXTransactionScope())
			{
				if (docgraph.transactions.Cache.IsDirty)
				{
					docgraph.Save.Press();

					PXSelectBase<SOOrderShipment> cmd = new PXSelect<SOOrderShipment, Where<SOOrderShipment.orderType, Equal<Current<SOOrder.orderType>>, And<SOOrderShipment.orderNbr, Equal<Current<SOOrder.orderNbr>>>>>(this);
					if (isOrderShipmentPassed)
					{
						cmd.WhereAnd<Where<SOOrderShipment.shippingRefNoteID, Equal<Current<SOOrderShipment.shippingRefNoteID>>>>();
					}
					else
					{
						cmd.WhereAnd<Where<SOOrderShipment.invtRefNbr, IsNull>>();
					}

					foreach (SOOrderShipment item in cmd.View.SelectMultiBound(new object[] { order, orderShipment }))
					{
						item.InvtDocType = docgraph.issue.Current.DocType;
						item.InvtRefNbr = docgraph.issue.Current.RefNbr;
						item.InvtNoteID = docgraph.issue.Current.NoteID;

						shipmentlist.Cache.Update(item);

						UpdatePlansRefNoteID(item, item.InvtNoteID, reattachedPlans);
					}

					this.Save.Press();

					INRegister existing;
					if ((existing = list.Find(docgraph.issue.Current)) == null)
					{
						list.Add(docgraph.issue.Current);
					}
					else
					{
						docgraph.issue.Cache.RestoreCopy(existing, docgraph.issue.Current);
					}
				}
				ts.Complete();
			}
		}

		protected virtual decimal? GetINTranUnitCost(SOLine line, SOLineSplit split, bool isReturnSpecific)
		{
			if (line.Operation == SOOperation.Receipt)
			{
				if (isReturnSpecific)
				{
					var origINTranCosts = SelectFrom<INTranCost>
						.InnerJoin<INTran>.On<INTranCost.FK.Tran>
						.InnerJoin<ARTran>
							.On<ARTran.tranType.IsEqual<INTran.aRDocType>
								.And<ARTran.refNbr.IsEqual<INTran.aRRefNbr>>
								.And<ARTran.lineNbr.IsEqual<INTran.aRLineNbr>>>
						.Where<INTranCost.lotSerialNbr.IsEqual<Data.BQL.@P.AsString>
							.And<ARTran.tranType.IsEqual< Data.BQL.@P.AsString.ASCII>>
							.And<ARTran.refNbr.IsEqual<Data.BQL.@P.AsString>>
							.And<ARTran.lineNbr.IsEqual<Data.BQL.@P.AsInt>>>
						.View.ReadOnly.Select(this,
							split.LotSerialNbr,
							line.InvoiceType,
							line.InvoiceNbr,
							line.InvoiceLineNbr)
						.RowCast<INTranCost>().ToList();
					decimal? qtySum = origINTranCosts.Sum(c => c.Qty);
					if ((qtySum ?? 0m) != 0m)
					{
						return PXPriceCostAttribute.Round(origINTranCosts.Sum(c => c.TranCost).Value / qtySum.Value);
					}
				}

				var invoiceLines = INTran.FK.SOLine.SelectChildren(this, new SOLine
				{
					OrderType = line.OrigOrderType,
					OrderNbr = line.OrigOrderNbr,
					LineNbr = line.OrigLineNbr
				}).ToList();
				if (invoiceLines.Any())
				{
					INTran invoiceLine = invoiceLines[0];
					return INUnitAttribute.ConvertFromBase(Transactions.Cache, invoiceLine.InventoryID, invoiceLine.UOM, invoiceLine.UnitCost ?? 0m, INPrecision.UNITCOST);
				}
				else
				{
					var branch = GL.Branch.PK.Find(this, line.BranchID);
					var itemCost = INItemCost.PK.Find(this, split.InventoryID, branch?.BaseCuryID);
					if (itemCost != null)
						return itemCost.LastCost;
				}
			}
			return 0m;
		}

		public virtual void UpdatePlansRefNoteID(SOOrderShipment orderShipment, Guid? refNoteID, IEnumerable<INItemPlan> reattachedPlans)
		{
			if (!reattachedPlans.Any()) return;

			// supposed that at this point there may be only deleted INItemPlan records in our graph
			// they should be persisted before the following direct update
			this.Caches[typeof(INItemPlan)].Persist(PXDBOperation.Delete);
			this.Caches[typeof(INItemPlan)].Persisted(false);

			// update INItemPlan.RefNoteID with the new IN Issue identifier
			PXUpdateJoin<
				Set<INItemPlan.refNoteID, Required<INItemPlan.refNoteID>,
				Set<INItemPlan.refEntityType, Common.Constants.DACName<INRegister>>>,
				INItemPlan,
					InnerJoin<SOLineSplit, On<SOLineSplit.planID, Equal<INItemPlan.planID>>>,
				Where<SOLineSplit.orderType, Equal<Required<SOLineSplit.orderType>>,
					And<SOLineSplit.orderNbr, Equal<Required<SOLineSplit.orderNbr>>>>>
			.Update(this,
				refNoteID,
				orderShipment.OrderType,
				orderShipment.OrderNbr);

			var stamp = PXDatabase.SelectTimeStamp();
			foreach (var plan in reattachedPlans)
				PXTimeStampScope.PutPersisted(this.Caches[typeof(INItemPlan)], plan, stamp);
		}

		#region EPApproval Cahce Attached
		[PXDefault(typeof(SOOrder.orderDate), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		protected virtual void EPApproval_DocDate_CacheAttached(PXCache sender)
		{
		}

		[PXDefault(typeof(SOOrder.customerID), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		protected virtual void EPApproval_BAccountID_CacheAttached(PXCache sender)
		{
		}

		[PXDefault(typeof(SOOrder.ownerID), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		protected virtual void EPApproval_DocumentOwnerID_CacheAttached(PXCache sender)
		{
		}

		[PXDefault(typeof(SOOrder.orderDesc), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		protected virtual void EPApproval_Descr_CacheAttached(PXCache sender)
		{
		}

		[CurrencyInfo(typeof(SOOrder.curyInfoID))]
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		protected virtual void EPApproval_CuryInfoID_CacheAttached(PXCache sender)
		{
		}

		[PXDefault(typeof(SOOrder.curyOrderTotal), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		protected virtual void EPApproval_CuryTotalAmount_CacheAttached(PXCache sender)
		{
		}

		[PXDefault(typeof(SOOrder.orderTotal), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		protected virtual void EPApproval_TotalAmount_CacheAttached(PXCache sender)
		{
		}
		#endregion

		public bool AllowAllocation()
		{
			bool allowAllocation = soordertype.Current != null && soordertype.Current.RequireAllocation != true
				|| PXAccess.FeatureInstalled<FeaturesSet.warehouseLocation>()
				|| PXAccess.FeatureInstalled<FeaturesSet.lotSerialTracking>()
				|| PXAccess.FeatureInstalled<FeaturesSet.subItem>()
				|| PXAccess.FeatureInstalled<FeaturesSet.replenishment>()
				|| PXAccess.FeatureInstalled<FeaturesSet.sOToPOLink>();
			return allowAllocation;
		}

		protected virtual void RemoveOrphanReplenishmentLines()
		{
			if (this.UnattendedMode)
				return;

			var removedPlans = Caches[typeof(INItemPlan)].Deleted
				.Cast<INItemPlan>()
				.Select(p => p.PlanID)
				.ToHashSet();

			if (removedPlans.Count == 0)
				return;

			foreach (var replenishmentLine in ReplenishmentLinesWithPlans.Select())
			{
				var plan = replenishmentLine.GetItem<INItemPlan>();
				if (plan?.SupplyPlanID == null || !removedPlans.Contains(plan.SupplyPlanID))
					continue;

				ReplenishmentLinesWithPlans.Delete(replenishmentLine);
				Caches[typeof(INItemPlan)].Delete(plan);
			}
		}

		private int _persistNesting = 0;
		public override void Persist()
		{
			try
			{
				_persistNesting++;
				PersistImpl();
			}
			finally
			{
				_persistNesting--;
			}
		}

		protected virtual void RecalculatePricesAndDiscountsOnPersist(IEnumerable<SOOrder> orders)
		{
			foreach (SOOrder doc in orders.Where(doc => doc.DeferPriceDiscountRecalculation == true && doc.IsPriceAndDiscountsValid == false))
			{
				if (!object.ReferenceEquals(Document.Current, doc))
				{
					Document.Current = doc;
				}

				TaxAttribute.SetTaxCalc<SOLine.taxCategoryID>(this.Transactions.Cache, null, TaxCalc.ManualLineCalc | TaxCalc.RecalculateAlways);

				doc.DeferPriceDiscountRecalculation = false;

				try
				{
					_discountEngine.AutoRecalculatePricesAndDiscounts(Transactions.Cache, Transactions, null, DiscountDetails,
						Document.Current.CustomerLocationID, Document.Current.OrderDate, GetDefaultSODiscountCalculationOptions(doc, true) | DiscountEngine.DiscountCalculationOptions.EnableOptimizationOfGroupAndDocumentDiscountsCalculation);

					doc.IsPriceAndDiscountsValid = true;
				}
				finally
				{
					doc.DeferPriceDiscountRecalculation = true;
				}
			}
		}

		protected virtual void PersistImpl()
		{
			Stopwatch sw = new Stopwatch();
			sw.Start();
			Debug.Print("{0} Enter Persist()", DateTime.Now.TimeOfDay);
			Debug.Indent();

			RemoveOrphanReplenishmentLines();

			foreach (var statusGroup in sitestatusview.Cache.Inserted.Cast<SiteStatusByCostCenter>()
				.Where(status => status.InitSiteStatus == true)
				.GroupBy(status => new { status.InventoryID, status.SiteID }))
			{
				INItemSite itemsite = INReleaseProcess.SelectItemSite(this, statusGroup.Key.InventoryID, statusGroup.Key.SiteID);
				if (itemsite == null)
				{
					InventoryItem item = InventoryItem.PK.Find(this, statusGroup.Key.InventoryID);
					if (item.StkItem == true)
					{
						INSite site = INSite.PK.Find(this, statusGroup.Key.SiteID);
						INPostClass postclass = INPostClass.PK.Find(this, item.PostClassID);
						var itemCurySettings = InventoryItemCurySettings.PK.Find(this, item.InventoryID, site.BaseCuryID);

						itemsite = new INItemSite();
						itemsite.InventoryID = statusGroup.Key.InventoryID;
						itemsite.SiteID = statusGroup.Key.SiteID;
						INItemSiteMaint.DefaultItemSiteByItem(this, itemsite, item, site, postclass, itemCurySettings);
						itemsite = initemsite.Insert(itemsite);
					}
				}

			}

			var orders = Document.Cache.Inserted
				.Concat_(Document.Cache.Updated)
				.Cast<SOOrder>();

			RecalculatePricesAndDiscountsOnPersist(orders);

			foreach (SOOrder doc in orders.Where(doc => doc.Completed == false))
			{
				VerifyAppliedToOrderAmount(doc);
			}

			if (Document.Current != null
				&& Document.Current.IsPackageValid != true
				&& !string.IsNullOrEmpty(Document.Current.ShipVia)
				&& (soordertype.Current.RequireShipping == true || soordertype.Current?.Behavior == SOBehavior.QT))
			{
				try
				{
					if (Document.Current.IsManualPackage != true)
					{
						CarrierRatesExt.RecalculatePackagesForOrder(Document.Current);
					}
				}
				catch (Exception ex)
				{
					PXTrace.WriteError(ex);
				}
			}


			if (Document.Current != null
				&& Document.Current.FreightCostIsValid != true
				&& soordertype.Current?.CalculateFreight == true
				&& !string.IsNullOrEmpty(Document.Current.ShipVia))
			{
				try
				{
					CalculateFreightCost(true);
				}
				catch (Exception ex)
				{
					PXTrace.WriteError(ex);
				}
			}

			foreach (SOOrder order in Document.Cache.Updated)
				TryAutoCompleteOrder(order);

			_discountEngine.ValidateDiscountDetails(DiscountDetails);

			//When the calling process is a long-running operation recalculate taxes on the same thread before the Persist.
			// PXAutomation.CompleteAction is called even if there is an Exception after the base.Persist() call.
			if (RecalculateExternalTaxesSync)
				RecalculateExternalTaxes();

			//Taxes can be imported as-is, ignoring all the validation rules when certain conditions are met. See SOOrderEntryExternalTaxImport extension.
			InsertImportedTaxes();

			base.Persist();

			if (!RecalculateExternalTaxesSync && _persistNesting == 1) //When the calling process is the 'UI' thread, but only for the first call in the stack.
				RecalculateExternalTaxes();

			sw.Stop();
			Debug.Unindent();
			Debug.Print("{0} Exit Persist in {1} millisec", DateTime.Now.TimeOfDay, sw.ElapsedMilliseconds);
		}

		protected virtual void VerifyAppliedToOrderAmount(SOOrder doc)
		{
			SOOrderType orderType = (this.soordertype.Current?.OrderType == doc.OrderType) ? this.soordertype.Current : this.soordertype.Select(doc.OrderType);
			if (orderType.CanHavePayments == true || orderType.CanHaveRefunds == true)
			{
				decimal? CuryApplAmt = 0m;
				bool appliedToOrderUpdated = false;

				foreach (PXResult<SOAdjust, ARRegisterAlias, ARPayment> res in Adjustments_Raw.View.SelectMultiBound(new object[] { doc }))
				{
					SOAdjust adj = (SOAdjust)res;

					if (adj?.Voided == false)
					{
						CuryApplAmt += adj.CuryAdjdAmt;

						if (Adjustments.Cache.GetStatus(adj).IsIn(PXEntryStatus.Updated, PXEntryStatus.Inserted))
							appliedToOrderUpdated = true;

						bool paymentSignMismatch = (doc.Behavior == SOBehavior.MO)
							&& (doc.CuryOrderTotal >= 0m && ARPaymentType.DrCr(adj.AdjgDocType) == DrCr.Credit
							|| doc.CuryOrderTotal < 0m && ARPaymentType.DrCr(adj.AdjgDocType) == DrCr.Debit);

						if (paymentSignMismatch
							|| doc.CuryDocBal - CuryApplAmt < 0m && CuryApplAmt > 0m
								&& !ExternalTaxRecalculationScope.IsScoped() && (appliedToOrderUpdated || ((decimal?)Document.Cache.GetValueOriginal<SOOrder.curyOrderTotal>(doc) != doc.CuryOrderTotal)))
						{
							// We should perform checks only if SOAdjust was updated/inserted or SOOrder.curyOrderTotal was changed.
							string message = (doc.Behavior == SOBehavior.MO)
								? (ARPaymentType.DrCr(adj.AdjgDocType) == DrCr.Credit
									? Messages.OrderApplyAmount_Cannot_Exceed_Unrefunded
									: Messages.OrderApplyAmount_Cannot_Exceed_Unbilled)
								: Messages.OrderApplyAmount_Cannot_Exceed_OrderTotal;

							Adjustments.Cache.MarkUpdated(adj, assertError: true);
							Adjustments.Cache.RaiseExceptionHandling<SOAdjust.curyAdjdAmt>(adj, adj.CuryAdjdAmt, new PXSetPropertyException(message));
							throw new PXException(message);
						}
					}
				}
			}
		}

		protected virtual bool TryAutoCompleteOrder(SOOrder order)
		{
			if (order.Behavior.IsIn(SOBehavior.SO, SOBehavior.TR, SOBehavior.RM, SOBehavior.BL)
				&& order.ShipmentCntr > 0 && order.OpenShipmentCntr == 0 && order.OpenLineCntr == 0
				&& (order.ForceCompleteOrder == true || (int?)Document.Cache.GetValueOriginal<SOOrder.openLineCntr>(order) > 0))
			{
                order.Approved = Approval.IsApproved(order);
				order.Hold = false;
				order.ForceCompleteOrder = false;
				order.CreditHold = false;
				order.InclCustOpenOrders = true;
				order.MarkCompleted();
				Document.Update(order);
				Document.Search<SOOrder.orderNbr>(Document.Current.OrderNbr, Document.Current.OrderType);
				return true;
			}
			return false;
		}

		protected void SetReadOnly(bool isReadOnly)
		{
			bool oldAllowInsert = Document.Cache.AllowInsert;

			PXCache[] cachearr = new PXCache[Caches.Count];
			try
			{
				Caches.Values.CopyTo(cachearr, 0);
			}
			catch (ArgumentException)
			{
				cachearr = new PXCache[Caches.Count + 5];
				Caches.Values.CopyTo(cachearr, 0);
			}
			foreach (PXCache cache in cachearr)
			{
				if (cache != null)
				{
					cache.AllowDelete = !isReadOnly;
					cache.AllowUpdate = !isReadOnly;
					cache.AllowInsert = !isReadOnly;
				}
			}

			Document.Cache.AllowInsert = oldAllowInsert;
		}

		#region CopyParamFilter
		protected virtual void CopyParamFilter_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			if (e.Row == null) return;
			CopyParamFilter row = e.Row as CopyParamFilter;

			if (row.OrderType != null)
			{
				var orderType = SOOrderType.PK.Find(this, row.OrderType);
				
				Numbering numbering = PXSelect<Numbering, 
					Where<Numbering.numberingID, Equal<Required<Numbering.numberingID>>>>.Select(this, orderType.OrderNumberingID);

				PXUIFieldAttribute.SetEnabled<CopyParamFilter.orderNbr>(sender, e.Row, numbering.UserNumbering == true);
			}
			else
			{
				PXUIFieldAttribute.SetEnabled<CopyParamFilter.orderNbr>(sender, e.Row, false);
			}
			checkCopyParams.SetEnabled(!string.IsNullOrEmpty(row.OrderType) && !string.IsNullOrEmpty(row.OrderNbr));
			if (string.IsNullOrEmpty(row.OrderType))
				PXUIFieldAttribute.SetWarning<CopyParamFilter.orderType>(sender, e.Row, PXMessages.LocalizeFormatNoPrefix(ErrorMessages.FieldIsEmpty, PXUIFieldAttribute.GetDisplayName<CopyParamFilter.orderType>(sender)));
			else
				PXUIFieldAttribute.SetWarning<CopyParamFilter.orderType>(sender, e.Row, null);
			if (string.IsNullOrEmpty(row.OrderNbr))
				PXUIFieldAttribute.SetWarning<CopyParamFilter.orderNbr>(sender, e.Row, PXMessages.LocalizeFormatNoPrefix(ErrorMessages.FieldIsEmpty, PXUIFieldAttribute.GetDisplayName<CopyParamFilter.orderNbr>(sender)));
			else
				PXUIFieldAttribute.SetWarning<CopyParamFilter.orderNbr>(sender, e.Row, null);

			PXUIFieldAttribute.SetEnabled<CopyParamFilter.overrideManualDiscounts>(sender, row, (row.RecalcDiscounts == true && Document.Current != null && Document.Current.DisableAutomaticDiscountCalculation != true));
			PXUIFieldAttribute.SetEnabled<CopyParamFilter.overrideManualPrices>(sender, row, row.RecalcUnitPrices == true);

			sender.IsDirty = false;
		}

		protected virtual void CopyParamFilter_OrderType_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			CopyParamFilter row = e.Row as CopyParamFilter;
			if (row != null)
			{
				if (row.OrderType != null)
					sender.SetDefaultExt<CopyParamFilter.orderNbr>(e.Row);
				else
					row.OrderNbr = null;
			}
		}
		#endregion

		#region Implementation of IPXPrepareItems

		public MultiDuplicatesSearchEngine<SOLine> DuplicateFinder { get; set; }

		private bool DontUpdateExistRecords
		{
			get
			{
				object dontUpdateExistRecords;
				return IsImportFromExcel && PXExecutionContext.Current.Bag.TryGetValue(PXImportAttribute._DONT_UPDATE_EXIST_RECORDS, out dontUpdateExistRecords) &&
					true.Equals(dontUpdateExistRecords);
			}
		}

		protected virtual Type[] GetAlternativeKeyFields()
		{
			var keys = new List<Type>()
			{
				typeof(SOLine.branchID),
				typeof(SOLine.inventoryID),
				typeof(SOLine.siteID),
				typeof(SOLine.locationID),
				typeof(SOLine.alternateID),
				typeof(SOLine.invoiceNbr),
			};

			if (PXAccess.FeatureInstalled<FeaturesSet.subItem>())
				keys.Add(typeof(SOLine.subItemID));

			return keys.ToArray();
		}

		public virtual bool PrepareImportRow(string viewName, IDictionary keys, IDictionary values)
		{
			if (string.Compare(viewName, nameof(Transactions), true) == 0)
			{
				if (values.Contains(nameof(SOLine.orderType))) values[nameof(SOLine.orderType)] = Document.Current.OrderType;
				else values.Add(nameof(SOLine.orderType), Document.Current.OrderType);

				if (values.Contains(nameof(SOLine.orderNbr))) values[nameof(SOLine.orderNbr)] = Document.Current.OrderNbr;
				else values.Add(nameof(SOLine.orderNbr), Document.Current.OrderNbr);
				//this._blockUIUpdate = true;

				if (!DontUpdateExistRecords)
				{
					if (DuplicateFinder == null)
					{
						var details = Transactions.SelectMain();
						DuplicateFinder = new MultiDuplicatesSearchEngine<SOLine>(Transactions.Cache, GetAlternativeKeyFields(), details);
					}
					var duplicate = DuplicateFinder.Find(values);
					if (duplicate != null)
					{
						DuplicateFinder.RemoveItem(duplicate);

						if (keys.Contains(nameof(SOLine.lineNbr)))
							keys[nameof(SOLine.LineNbr)] = duplicate.LineNbr;
						else
							keys.Add(nameof(SOLine.LineNbr), duplicate.LineNbr);
					}
					else if (keys.Contains(nameof(SOLine.lineNbr)))
					{
						bool lineExists = false;

						object value = keys[nameof(SOLine.lineNbr)];
						if (Transactions.Cache.RaiseFieldUpdating<SOLine.lineNbr>(null, ref value) &&
							value is int lineNbr)
						{
							var line = new SOLine()
							{
								OrderType = Document.Current.OrderType,
								OrderNbr = Document.Current.OrderNbr,
								LineNbr = lineNbr
							};

							lineExists = Transactions.Cache.Locate(line) != null;
						}
						
						if (lineExists)			
							keys.Remove(nameof(SOLine.lineNbr));
					}
				}
			}

			return true;
		}

		public bool RowImporting(string viewName, object row)
		{
			return row == null;
		}

		public bool RowImported(string viewName, object row, object oldRow)
		{
			return oldRow == null;
		}

		public virtual void PrepareItems(string viewName, IEnumerable items)
		{
		}

		public virtual void ImportDone(PXImportAttribute.ImportMode.Value mode)
		{
			DuplicateFinder = null;

			var soOrder = Document.Current;
			if (soOrder != null)
			{
				CalcFreight(soOrder);
				
				try
				{
					Document.Current.DeferPriceDiscountRecalculation = false;
				_discountEngine.AutoRecalculatePricesAndDiscounts(
					cache: Transactions.Cache, 
					lines: Transactions, 
					currentLine: null, 
					discountDetails: DiscountDetails, 
					locationID: Document.Current.CustomerLocationID, 
					date: Document.Current.OrderDate, 
						discountCalculationOptions: GetDefaultSODiscountCalculationOptions(Document.Current, true) | DiscountEngine.DiscountCalculationOptions.EnableOptimizationOfGroupAndDocumentDiscountsCalculation);
				}
				finally
				{
					Document.Current.DeferPriceDiscountRecalculation = soordertype.Current.DeferPriceDiscountRecalculation;
				}
			}
		}

		#endregion

		#region SelectEntityDiscounts enhancement

		protected string EntityDiscountsLoadedFor;

		protected virtual void LoadEntityDiscounts()
		{
			if (Document.Current?.OrderNbr == null
				|| !PXAccess.FeatureInstalled<FeaturesSet.customerDiscounts>())
				return;

			var order = $"{Document.Current.OrderType}-{Document.Current.OrderNbr}";

			if (EntityDiscountsLoadedFor == order)
				return;

			var query = new
				SelectFrom<SOLineShort>
					.LeftJoin<DiscountItem>
						.On<DiscountItem.inventoryID.IsEqual<SOLineShort.inventoryID>>
					.LeftJoin<DiscountSequence>
						.On<DiscountSequence.isActive.IsEqual<True>
						.And<DiscountItem.FK.DiscountSequence>>
					.Where<SOLineShort.FK.Order.SameAsCurrent>
				.View
				.ReadOnly(this);
			
			var items = new Dictionary<int, HashSet<DiscountSequenceKey>>();

			using (new PXFieldScope(query.View,
					typeof(SOLineShort.inventoryID),
					typeof(DiscountSequence.discountID),
					typeof(DiscountSequence.discountSequenceID)))
			{
				foreach(PXResult<SOLineShort, DiscountItem, DiscountSequence> res in query.Select())
				{
					SOLineShort line = res;
					DiscountSequence seq = res;
					HashSet<DiscountSequenceKey> seqSet;

					if (line.InventoryID != null)
					{
						if (!items.TryGetValue(line.InventoryID.Value, out seqSet))
							items.Add(line.InventoryID.Value, seqSet = new HashSet<DiscountSequenceKey>());

						if (seq.DiscountID != null && seq.DiscountSequenceID != null)
							seqSet.Add(new DiscountSequenceKey(seq.DiscountID, seq.DiscountSequenceID));
					}
				}
			}

			DiscountEngine.UpdateEntityCache();
			DiscountEngine.PutEntityDiscountsToSlot<DiscountItem, int>(items);

			EntityDiscountsLoadedFor = order;
		}

		#endregion

		public static void ProcessPOReceipt(PXGraph graph, IEnumerable<PXResult<INItemPlan, INPlanType>> list, string POReceiptType, string POReceiptNbr)
		{
			var soorder = new PXSelect<SOOrder>(graph);
			if (!graph.Views.Caches.Contains(typeof(SOOrder)))
				graph.Views.Caches.Add(typeof(SOOrder));
			var solinesplit = new PXSelect<SOLineSplit>(graph);
			if (!graph.Views.Caches.Contains(typeof(SOLineSplit)))
				graph.Views.Caches.Add(typeof(SOLineSplit));
			var initemplan = new PXSelect<INItemPlan>(graph);

			List<SOLineSplit> splitsToDeletePlanID = new List<SOLineSplit>();

			List<SOLineSplit> insertedSchedules = new List<SOLineSplit>();
			List<INItemPlan> deletedPlans = new List<INItemPlan>();

			foreach (PXResult<INItemPlan, INPlanType> res in list)
			{
				INItemPlan plan = PXCache<INItemPlan>.CreateCopy(res);
				INPlanType plantype = res;

				//avoid ReadItem()
				if (initemplan.Cache.GetStatus(plan) != PXEntryStatus.Inserted)
				{
					initemplan.Cache.SetStatus(plan, PXEntryStatus.Notchanged);
				}

				//Original Schedule Marked for PO / Allocated on Remote Whse
				//SOLineSplit schedule = PXSelect<SOLineSplit, Where<SOLineSplit.planID, Equal<Required<SOLineSplit.planID>>, And<SOLineSplit.completed, Equal<False>>>>.Select(this, plan.DemandPlanID);
				SOLineSplit schedule = PXSelect<SOLineSplit, Where<SOLineSplit.planID, Equal<Required<SOLineSplit.planID>>>>.Select(graph, plan.DemandPlanID);

				if (schedule != null && (schedule.Completed == false || solinesplit.Cache.GetStatus(schedule) == PXEntryStatus.Updated))
				{
					schedule = PXCache<SOLineSplit>.CreateCopy(schedule);

					schedule.BaseReceivedQty += plan.PlanQty;
					schedule.ReceivedQty = INUnitAttribute.ConvertFromBase(solinesplit.Cache, schedule.InventoryID, schedule.UOM, (decimal)schedule.BaseReceivedQty, INPrecision.QUANTITY);

					solinesplit.Cache.Update(schedule);

					INItemPlan origplan = PXSelect<INItemPlan, Where<INItemPlan.planID, Equal<Required<INItemPlan.planID>>>>.Select(graph, plan.DemandPlanID);
					if (origplan != null)
					{
						origplan.PlanQty = schedule.BaseQty - schedule.BaseReceivedQty;
						initemplan.Cache.Update(origplan);
					}

					//select Allocated line if any, exclude allocated on Remote Whse
					PXSelectBase<INItemPlan> cmd = new PXSelectJoin<INItemPlan,
						InnerJoin<SOLineSplit, On<SOLineSplit.planID, Equal<INItemPlan.planID>>>,
						Where<INItemPlan.demandPlanID, Equal<Required<INItemPlan.demandPlanID>>,
							And<SOLineSplit.isAllocated, Equal<True>,
							And<SOLineSplit.siteID, Equal<Required<SOLineSplit.siteID>>>>>>(graph);
					if (!string.IsNullOrEmpty(plan.LotSerialNbr))
					{
						cmd.WhereAnd<Where<INItemPlan.lotSerialNbr, Equal<Required<INItemPlan.lotSerialNbr>>>>();
					}
					PXResult<INItemPlan> allocres = cmd.Select(plan.DemandPlanID, plan.SiteID, plan.LotSerialNbr);

					if (allocres != null)
					{
						schedule = PXResult.Unwrap<SOLineSplit>(allocres);
						solinesplit.Cache.SetStatus(schedule, PXEntryStatus.Notchanged);
						schedule = PXCache<SOLineSplit>.CreateCopy(schedule);
						schedule.BaseQty += plan.PlanQty;
						schedule.Qty = INUnitAttribute.ConvertFromBase(solinesplit.Cache, schedule.InventoryID, schedule.UOM, (decimal)schedule.BaseQty, INPrecision.QUANTITY);
						schedule.POReceiptType = POReceiptType;
						schedule.POReceiptNbr = POReceiptNbr;

						solinesplit.Cache.Update(schedule);

						INItemPlan allocplan = PXCache<INItemPlan>.CreateCopy(res);
						allocplan.PlanQty += plan.PlanQty;

						initemplan.Cache.Update(allocplan);

						plantype = PXCache<INPlanType>.CreateCopy(plantype);
						plantype.ReplanOnEvent = null;
						plantype.DeleteOnEvent = true;
					}
					else
					{
						soorder.Current = (SOOrder)PXParentAttribute.SelectParent(solinesplit.Cache, schedule, typeof(SOOrder));
						schedule = PXCache<SOLineSplit>.CreateCopy(schedule);

						long? oldPlanID = schedule.PlanID;
						ClearScheduleReferences(ref schedule);

						schedule.IsAllocated = (plantype.ReplanOnEvent != INPlanConstants.Plan60);
						schedule.LotSerialNbr = plan.LotSerialNbr;
						schedule.POCreate = false;
						schedule.POSource = null;
						schedule.POReceiptType = POReceiptType;
						schedule.POReceiptNbr = POReceiptNbr;
						schedule.SiteID = plan.SiteID;
						schedule.CostCenterID = plan.CostCenterID;
						schedule.VendorID = null;

						schedule.BaseReceivedQty = 0m;
						schedule.ReceivedQty = 0m;
						schedule.QtyOnOrders = 0m;
						schedule.BaseQty = plan.PlanQty;
						schedule.Qty = INUnitAttribute.ConvertFromBase(solinesplit.Cache, schedule.InventoryID, schedule.UOM, (decimal)schedule.BaseQty, INPrecision.QUANTITY);

						if (!string.IsNullOrEmpty(schedule.LotSerialNbr) && schedule.CostCenterID != CostCenter.FreeStock)
						{
							SOLine line = PXParentAttribute.SelectParent<SOLine>(solinesplit.Cache, schedule);
							if (line?.IsSpecialOrder == true)
							{
								var inventoryItem = InventoryItem.PK.Find(graph, schedule.InventoryID);
								var lotSerClass = INLotSerClass.PK.Find(graph, inventoryItem?.LotSerClassID);
								if (lotSerClass?.LotSerTrack == INLotSerTrack.SerialNumbered)
								{
									schedule.UOM = inventoryItem.BaseUnit;
									schedule.Qty = plan.PlanQty;
								}
							}
						}

						//update SupplyPlanID in existing item plans (replenishment)
						foreach (PXResult<INItemPlan> demand_res in PXSelect<INItemPlan,
							Where<INItemPlan.supplyPlanID, Equal<Required<INItemPlan.supplyPlanID>>>>.Select(graph, oldPlanID))
						{
							INItemPlan demand_plan = PXCache<INItemPlan>.CreateCopy(demand_res);
							initemplan.Cache.SetStatus(demand_plan, PXEntryStatus.Notchanged);
							demand_plan.SupplyPlanID = plan.PlanID;
							initemplan.Cache.Update(demand_plan);
						}

						schedule.PlanID = plan.PlanID;

						schedule = (SOLineSplit)solinesplit.Cache.Insert(schedule);
						insertedSchedules.Add(schedule);
					}
				}
				else if (plan.DemandPlanID != null)
				{
					//Original schedule was completed/plan record deleted by Cancel Order or Confirm Shipment
					plantype = PXCache<INPlanType>.CreateCopy(plantype);
					plantype.ReplanOnEvent = null;
					plantype.DeleteOnEvent = true;
				}
				else
				{
					//Original schedule Marked for PO
					//TODO: verify this is sufficient for Original SO marked for TR.
					schedule = PXSelect<SOLineSplit, Where<SOLineSplit.planID, Equal<Required<SOLineSplit.planID>>, And<SOLineSplit.completed, Equal<False>>>>.Select(graph, plan.PlanID);
					if (schedule != null)
					{
						solinesplit.Cache.SetStatus(schedule, PXEntryStatus.Notchanged);
						schedule = PXCache<SOLineSplit>.CreateCopy(schedule);

						schedule.Completed = (schedule.OpenChildLineCntr == 0);
						schedule.POCompleted = true;
						splitsToDeletePlanID.Add(schedule);
						solinesplit.Cache.Update(schedule);

						INItemPlan origplan = PXSelect<INItemPlan, Where<INItemPlan.planID, Equal<Required<INItemPlan.planID>>>>.Select(graph, plan.PlanID);
						deletedPlans.Add(origplan);

						initemplan.Cache.Delete(origplan);
					}
				}

				if (plantype.ReplanOnEvent != null)
				{
					plan.PlanType = plantype.ReplanOnEvent;
					plan.SupplyPlanID = null;
					plan.DemandPlanID = null;
					initemplan.Cache.Update(plan);
				}
				else if (plantype.DeleteOnEvent == true)
				{
					initemplan.Delete(plan);
				}
			}

			//Create new schedules for partially received schedules marked for PO.
			SOLineSplit prevSplit = null;
			foreach (SOLineSplit newsplit in insertedSchedules)
			{
				if (prevSplit != null && prevSplit.OrderType == newsplit.OrderType && prevSplit.OrderNbr == newsplit.OrderNbr
					&& prevSplit.LineNbr == newsplit.LineNbr && prevSplit.InventoryID == newsplit.InventoryID
					&& prevSplit.SubItemID == newsplit.SubItemID && prevSplit.ParentSplitLineNbr == newsplit.ParentSplitLineNbr
					&& prevSplit.LotSerialNbr != null && newsplit.LotSerialNbr != null)
					continue;

				SOLineSplit parentschedule = PXSelect<SOLineSplit, Where<SOLineSplit.orderType, Equal<Required<SOLineSplit.orderType>>,
					And<SOLineSplit.orderNbr, Equal<Required<SOLineSplit.orderNbr>>,
					And<SOLineSplit.lineNbr, Equal<Required<SOLineSplit.lineNbr>>,
					And<SOLineSplit.splitLineNbr, Equal<Required<SOLineSplit.parentSplitLineNbr>>>>>>>.Select(graph, newsplit.OrderType, newsplit.OrderNbr, newsplit.LineNbr, newsplit.ParentSplitLineNbr);

				if (parentschedule?.POCompleted == true
					&& (parentschedule.Completed == true || parentschedule.OpenChildLineCntr != 0)
					&& parentschedule.BaseQty > parentschedule.BaseQtyOnOrders + parentschedule.BaseReceivedQty
					&& deletedPlans.Exists(x => x.PlanID == parentschedule.PlanID))
				{
					soorder.Current = (SOOrder)PXParentAttribute.SelectParent(solinesplit.Cache, parentschedule, typeof(SOOrder));

					parentschedule = PXCache<SOLineSplit>.CreateCopy(parentschedule);
					INItemPlan demand = PXCache<INItemPlan>.CreateCopy(deletedPlans.First(x => x.PlanID == parentschedule.PlanID));

					UpdateSchedulesFromCompletedPO(graph, solinesplit, initemplan, parentschedule, soorder, demand);
				}
				prevSplit = newsplit;
			}

			//Added because of MySql AutoIncrement counters behavior
			foreach (SOLineSplit split in splitsToDeletePlanID)
			{
				SOLineSplit schedule = (SOLineSplit)solinesplit.Cache.Locate(split);
				if (schedule != null)
				{
					schedule.PlanID = null;
					solinesplit.Cache.Update(schedule);
				}
			}
		}

		public static void ProcessPOOrder(PXGraph graph, POOrder poOrder)
		{
			if (poOrder == null) return;

			var soorder = new PXSelect<SOOrder>(graph);
			if (!graph.Views.Caches.Contains(typeof(SOOrder)))
				graph.Views.Caches.Add(typeof(SOOrder));
			var solinesplit = new PXSelect<SOLineSplit>(graph);
			if (!graph.Views.Caches.Contains(typeof(SOLineSplit)))
				graph.Views.Caches.Add(typeof(SOLineSplit));
			var initemplan = new PXSelect<INItemPlan>(graph);

			//Search for completed/cancelled POLines with uncompleted linked schedules
			foreach (PXResult<POLine, POOrder, SOLineSplit> res in
				PXSelectJoin<POLine,
				InnerJoin<POOrder, On<POLine.FK.Order>,
				InnerJoin<SOLineSplit, On<SOLineSplit.pOType, Equal<POLine.orderType>, And<SOLineSplit.pONbr, Equal<POLine.orderNbr>, And<SOLineSplit.pOLineNbr, Equal<POLine.lineNbr>>>>>>,
			Where<POLine.orderType, Equal<Required<POLine.orderType>>, And<POLine.orderNbr, Equal<Required<POLine.orderNbr>>,
				And2<Where<POLine.cancelled, Equal<boolTrue>, Or<POLine.completed, Equal<boolTrue>>>,
				And<POOrder.orderType, Equal<POOrderType.dropShip>, And<POOrder.isLegacyDropShip, Equal<True>,
				And<SOLineSplit.receivedQty, LessEqual<SOLineSplit.qty>,
				And<SOLineSplit.pOCancelled, NotEqual<boolTrue>,
				And<SOLineSplit.completed, NotEqual<boolTrue>>>>>>>>>>
				.Select(graph, poOrder.OrderType, poOrder.OrderNbr))
			{
				POLine poline = res;
				SOLineSplit parentschedule = PXCache<SOLineSplit>.CreateCopy(res);
				INItemPlan selectedPlan = INItemPlan.PK.Find(graph, parentschedule.PlanID);
				if (selectedPlan != null && selectedPlan.SupplyPlanID == poline.PlanID)
				{
					INItemPlan plan = PXCache<INItemPlan>.CreateCopy(selectedPlan);

					soorder.Current = (SOOrder)PXParentAttribute.SelectParent(solinesplit.Cache, parentschedule, typeof(SOOrder));

					if (parentschedule.Completed != true && parentschedule.POCancelled != true && parentschedule.BaseQty >= parentschedule.BaseReceivedQty)
					{
						bool cancelDropShip = poline.Cancelled == true && POLineType.IsDropShip(poline.LineType);

						UpdateSchedulesFromCompletedPO(graph, solinesplit, initemplan, parentschedule, soorder, plan, cancelDropShip);

						if (initemplan.Cache.GetStatus(plan) != PXEntryStatus.Inserted)
						{
							initemplan.Delete(plan);
						}

						solinesplit.Cache.SetStatus(parentschedule, PXEntryStatus.Notchanged);
						parentschedule = PXCache<SOLineSplit>.CreateCopy(parentschedule);

						parentschedule.PlanID = null;
						parentschedule.Completed = true;

						parentschedule.POCompleted = poline.Completed;
						parentschedule.POCancelled = poline.Cancelled;
						solinesplit.Cache.Update(parentschedule);
					}
				}
			}
		}

		private static void UpdateSchedulesFromCompletedPO(PXGraph graph, PXSelect<SOLineSplit> solinesplit, PXSelect<INItemPlan> initemplan, SOLineSplit parentschedule, PXSelect<SOOrder> soorder, INItemPlan demand, bool cancelDropShip = false)
		{
			graph.FieldDefaulting.AddHandler<SOLineSplit.locationID>((sender, e) =>
			{
				if (e.Row != null && ((SOLineSplit)e.Row).RequireLocation != true)
				{
					e.NewValue = null;
					e.Cancel = true;
				}
			});

			SOLineSplit newschedule = PXCache<SOLineSplit>.CreateCopy(parentschedule);

			ClearScheduleReferences(ref newschedule);

			newschedule.LotSerialNbr = demand.LotSerialNbr;
			newschedule.SiteID = demand.SiteID;

			decimal? processedBaseQty = (parentschedule.POSource == INReplenishmentSource.DropShipToOrder ? parentschedule.BaseShippedQty : parentschedule.BaseReceivedQty);
			newschedule.BaseQty = parentschedule.BaseQty - parentschedule.BaseQtyOnOrders - processedBaseQty;
			newschedule.Qty = INUnitAttribute.ConvertFromBase(solinesplit.Cache, newschedule.InventoryID, newschedule.UOM, (decimal)newschedule.BaseQty, INPrecision.QUANTITY);
			newschedule.BaseReceivedQty = 0m;
			newschedule.ReceivedQty = 0m;
			newschedule.BaseShippedQty = 0m;
			newschedule.ShippedQty = 0m;
			newschedule.QtyOnOrders = 0m;

			SOLine line = (SOLine)PXParentAttribute.SelectParent(solinesplit.Cache, parentschedule, typeof(SOLine));
			bool isSpecialOrder = line.IsSpecialOrder == true;

			if (cancelDropShip)
			{
				newschedule.POCreate = true;
				newschedule.POSource = INReplenishmentSource.DropShipToOrder;
			}
			else if (isSpecialOrder)
			{
				newschedule.POCreate = true;
				newschedule.POSource = INReplenishmentSource.PurchaseToOrder;
			}

			//creating new plan
			INItemPlan newPlan = null;
			if (soorder.Current?.Behavior == SOBehavior.BL)
			{
				if (parentschedule.Completed != true)
				{
					parentschedule.Qty -= newschedule.Qty;
					solinesplit.Cache.Update(parentschedule);
				}
			}
			else
			{
				newPlan = PXCache<INItemPlan>.CreateCopy(demand);
				newPlan.PlanType = cancelDropShip ? demand.PlanType
					: isSpecialOrder ? INPlanConstants.Plan66
					: (soorder.Current?.Hold == true) ? INPlanConstants.Plan69 : INPlanConstants.Plan60;
				newPlan.PlanID = null;
				newPlan.SupplyPlanID = null;
				newPlan.DemandPlanID = null;
				newPlan.PlanQty = newschedule.BaseQty;
				newPlan.VendorID = cancelDropShip || isSpecialOrder ? demand.VendorID : null;
				newPlan.VendorLocationID = cancelDropShip || isSpecialOrder ? demand.VendorLocationID : null;
				newPlan.FixedSource = cancelDropShip || isSpecialOrder ? INReplenishmentSource.Purchased : INReplenishmentSource.None;
				newPlan = (INItemPlan)initemplan.Cache.Insert(newPlan);
			}

			newschedule.PlanID = newPlan?.PlanID;
			solinesplit.Cache.Insert(newschedule);
		}

		public static void ClearScheduleReferences(ref SOLineSplit schedule)
		{
			schedule.ParentSplitLineNbr = schedule.SplitLineNbr;
			schedule.SplitLineNbr = null;
			schedule.Completed = false;
			schedule.PlanID = null;

			schedule.ClearPOFlags();
			schedule.ClearPOReferences();
			schedule.POSource = INReplenishmentSource.None;

			schedule.ClearSOReferences();

			schedule.RefNoteID = null;
		}
		public virtual UpdateIfFieldsChangedScope GetPriceCalculationScope()
			=> new SOOrderPriceCalculationScope();

		public virtual void ConfirmSingleLine(SOLine line, SOShipLine shipline, string lineShippingRule, ref bool backorderExists)
		{
			if (line.POSource == INReplenishmentSource.DropShipToOrder)
				return;

			using (LineSplittingExt.SuppressedModeScope(true))
			{
				decimal? unsignedBaseShippedQty = line.LineSign * line.BaseShippedQty;
				decimal? unsignedBaseOrderQty = line.LineSign * line.BaseOrderQty;
				if (line.IsFree == true && line.ManualDisc == false)
				{
					if (unsignedBaseShippedQty >= unsignedBaseOrderQty * line.CompleteQtyMin / 100m || !backorderExists)
					{
						line.OpenQty = 0m;
						line.Completed = true;
						line.ClosedQty = line.OrderQty;
						line.BaseClosedQty = line.BaseOrderQty;
						line.OpenLine = false;

						line = Transactions.Update(line);
						LineSplittingAllocatedExt.CompleteSchedules(line);
					}
					else
					{
						line.OpenQty = line.OrderQty - line.ShippedQty;
						line.BaseOpenQty = line.BaseOrderQty - line.BaseShippedQty;
						line.ClosedQty = line.ShippedQty;
						line.BaseClosedQty = line.BaseShippedQty;

						line = Transactions.Update(line);
					}
				}
				else
				{
					if (lineShippingRule == SOShipComplete.BackOrderAllowed && unsignedBaseShippedQty < unsignedBaseOrderQty * line.CompleteQtyMin / 100m)
					{
						line.OpenQty = line.OrderQty - line.ShippedQty;
						line.BaseOpenQty = line.BaseOrderQty - line.BaseShippedQty;
						line.ClosedQty = line.ShippedQty;
						line.BaseClosedQty = line.BaseShippedQty;

						line = Transactions.Update(line);

						backorderExists = true;
					}
					else if (shipline.ShipmentNbr != null || lineShippingRule != SOShipComplete.ShipComplete)
					{
						//Completed will be true for orders with locations enabled which requireshipping. check DefaultAttribute
						if (line.OpenLine == true)
						{
							Document.Current.OpenLineCntr--;
						}

						if (Document.Current.OpenLineCntr <= 0)
						{
							Document.Current.Completed = true;
						}

						line.OpenQty = 0m;
						line.ClosedQty = line.OrderQty;
						line.BaseClosedQty = line.BaseOrderQty;
						line.OpenLine = false;
						line.Completed = true;

						line = Transactions.Update(line);
						LineSplittingAllocatedExt.CompleteSchedules(line);
					}
				}
			}
		}

		public virtual SOLine CorrectSingleLine(SOLine line, SOShipLine shipLine, bool lineSwitched,
			Dictionary<int?, (SOLine, decimal?, decimal?)> lineOpenQuantities)
		{
			//if it was never shipped or is included in the shipment
			if (line.Completed == true && line.ShippedQty == 0m || line.OpenLine == false && shipLine.ShippedQty > 0m)
			{
				//skip auto free lines, must be consistent with OpenLineCalc<> and ConfirmShipment()
				if (line.IsFree == false || line.ManualDisc == true)
				{
					Document.Current.OpenLineCntr++;
				}
			}

			line = PXCache<SOLine>.CreateCopy(line);
			line.Completed = false;

			decimal? unsignedOpenQty = 0m, unsignedBaseOpenQty = 0m;
			if ((shipLine.ShippedQty ?? 0m) == 0m)
			{
				line.OpenQty = line.OrderQty - line.ShippedQty;
				unsignedOpenQty = line.LineSign * line.OpenQty;
				line.BaseOpenQty = line.BaseOrderQty - line.BaseShippedQty;
				unsignedBaseOpenQty = line.LineSign * line.BaseOpenQty;
				line.ClosedQty = line.ShippedQty;
				line.BaseClosedQty = line.BaseShippedQty;
			}
			else
			{
				if (lineSwitched)
				{
					line.OpenQty = line.OrderQty - line.ShippedQty;
					unsignedOpenQty = line.LineSign * line.OpenQty;
					line.BaseClosedQty = line.BaseOpenQty = line.BaseOrderQty - line.BaseShippedQty;
					unsignedBaseOpenQty = line.LineSign * line.BaseOpenQty;
					line.ClosedQty = line.ShippedQty;
					line.BaseClosedQty = line.BaseShippedQty;
				}
				line.BaseOpenQty += shipLine.SOLineSign * shipLine.BaseShippedQty;
				if (line.BaseOpenQty == line.BaseOrderQty)
				{
					line.OpenQty = line.OrderQty;
				}
				else
				{
					PXDBQuantityAttribute.CalcTranQty<SOLine.openQty>(Transactions.Cache, line);
				}

				line.BaseClosedQty -= shipLine.SOLineSign * shipLine.BaseShippedQty;
				PXQuantityAttribute.CalcTranQty<SOLine.closedQty>(Transactions.Cache, line);
			}

			line = Transactions.Update(line);
			//perform dirty Update() for OpenLineCalc<>
			line.OpenLine = true;

			if (!lineOpenQuantities.ContainsKey(line.LineNbr))
			{
				if (line.POCreate == true && line.ShippedQty != line.Qty)
				{
					foreach (SOLineSplit split in PXParentAttribute.SelectChildren(splits.Cache, line, typeof(SOLine)))
					{
						if (split.POCreate == true && split.POCompleted != true && split.POCancelled != true
							&& split.POReceiptNbr == null && split.Completed != true && split.IsAllocated != true)
						{
							var unreceivedQty = line.UOM == split.UOM
								? split.UnreceivedQty ?? 0m
								: INUnitAttribute.ConvertFromBase(splits.Cache, split.InventoryID, line.UOM, split.BaseUnreceivedQty ?? 0m, INPrecision.QUANTITY);
							unsignedBaseOpenQty -= split.BaseUnreceivedQty;
							unsignedOpenQty -= unreceivedQty;
						}
					}
				}
				lineOpenQuantities.Add(line.LineNbr, (line, unsignedBaseOpenQty, unsignedOpenQty));
			}

			return line;
		}

		protected virtual bool HasMiscLinesToInvoice(SOOrder order)
			=> (order.OrderQty == 0 || order.OpenLineCntr == 0 && order.IsLegacyMiscBilling == false)
				&& (order.CuryUnbilledMiscTot != 0 || order.UnbilledOrderQty > 0);

		#region Well-known extensions
		public SOQuickProcess SOQuickProcessExt => FindImplementation<SOQuickProcess>();
		public class SOQuickProcess : PXGraphExtension<SOOrderEntry>
		{
			public static bool IsActive() => true;

			[PXLocalizable]
			public static class Msg
			{
				public const string DoNotEmail = "Invoice will be emailed during quick processing though the {0} customer does not require sending invoices by email.";
				public const string DoEmail = "Invoice emailing will be skipped during quick processing though the {0} customer requires sending invoices by email.";
				public const string DoNotPrint = "Invoice will be printed during quick processing though the {0} customer does not require printing invoices.";
				public const string DoPrint = "Invoice printing will be skipped during quick processing though the {0} customer requires printing invoices.";
				public const string CannotShip = "The order cannot be shipped. See availability section for more info.";
				public const string OnlyCurrentShipmentWillBeInvoiced = "Only the shipment created by this process will be invoiced.";
				public const string SomeLinesWillBeSkipedDueToDateSelection = "This date selection will skip {0} open sales order lines from shipment.";
			}

			public PXQuickProcess.Action<SOOrder>.ConfiguredBy<SOQuickProcessParameters> quickProcess;
			[PXButton(CommitChanges = true), PXUIField(DisplayName = "Quick Process")]
			protected virtual IEnumerable QuickProcess(PXAdapter adapter)
			{
				QuickProcessParameters.AskExt(InitQuickProcessPanel);
				Base.Save.Press();
				PXQuickProcess.Start(Base, Base.Document.Current, QuickProcessParameters.Current);
				return new[] { Base.Document.Current };
			}

			public PXAction<SOOrder> quickProcessOk;
			[PXButton, PXUIField(DisplayName = "OK")]
			public virtual IEnumerable QuickProcessOk(PXAdapter adapter) => adapter.Get();

			public PXFilter<SOQuickProcessParameters> QuickProcessParameters;

			[Obsolete]
			/// <summary><see cref="SOOrder"/> Selected</summary>
			protected virtual void SOOrder_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
			{
			}

			/// <summary><see cref="SOLine.SiteID"/> Updated</summary>
			protected virtual void SOLine_SiteID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
			{
				if (Base.soordertype.Current?.AllowQuickProcess == true && string.IsNullOrEmpty(QuickProcessParameters.Current?.OrderType) == false)
				{
					QuickProcessParameters.Current.SiteID = null;
				}
			}

			[PXRemoveBaseAttribute(typeof(PXDefaultAttribute))]
			[PXCustomizeBaseAttribute(typeof(PXDBStringAttribute), nameof(PXDBStringAttribute.IsKey), false)]
			/// <summary><see cref="SOQuickProcessParameters.OrderType"/> CacheAttached</summary>
			protected virtual void SOQuickProcessParameters_OrderType_CacheAttached(PXCache cache) { }

			/// <summary><see cref="SOQuickProcessParameters.SiteID"/> Updated</summary>
			protected virtual void SOQuickProcessParameters_SiteID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
			{
				// Acuminator disable once PX1043 SavingChangesInEventHandlers [nothing is saved here]
				RecalculateAvailabilityStatus((SOQuickProcessParameters) e.Row);
				EnsureSiteID(sender, (SOQuickProcessParameters) e.Row);
			}

			/// <summary><see cref="SOQuickProcessParameters.ShipDate"/> Updated</summary>
			// Acuminator disable once PX1043 SavingChangesInEventHandlers [nothing is saved here]
			protected virtual void SOQuickProcessParameters_ShipDate_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e) => RecalculateAvailabilityStatus((SOQuickProcessParameters) e.Row);

			/// <summary><see cref="SOQuickProcessParameters"/> Inserted</summary>
			protected virtual void SOQuickProcessParameters_RowInserted(PXCache sender, PXRowInsertedEventArgs e) => sender.IsDirty = false;

			/// <summary><see cref="SOQuickProcessParameters"/> Updated</summary>
			protected virtual void SOQuickProcessParameters_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e) => sender.IsDirty = false;

			/// <summary><see cref="SOQuickProcessParameters"/> Selected</summary>
			protected virtual void SOQuickProcessParameters_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
			{
				quickProcessOk.SetEnabled(false);
				var row = (SOQuickProcessParameters)e.Row;
				if (row != null && Base.soordertype.Current?.AllowQuickProcess == true && string.IsNullOrEmpty(row.OrderType) == false)
				{
					VerifyPrepareInvoice(row);
					VerifyPrintInvoice(row);
					VerifyEmailInvoice(row);

					if (row.CreateShipment == true)
					{
						bool siteIsValid = (Base.Document.Current.OpenOrderQty > 0 && PXAccess.FeatureInstalled<FeaturesSet.warehouse>()).Implies(row.SiteID != null);

						String status = QuickProcessParameters.Cache.GetExtension<SOQuickProcessParametersAvailabilityExt>(row).AvailabilityStatus;
						Boolean canShip = status.IsIn(AvailabilityStatus.CanShipAll, AvailabilityStatus.CanShipPartBackOrder, AvailabilityStatus.CanShipPartCancelRemainder);

						sender.RaiseExceptionHandling<SOQuickProcessParameters.siteID>(
							row,
							row.SiteID,
							siteIsValid
								? null
								: new PXSetPropertyException(
									ErrorMessages.FieldIsEmpty,
									PXUIFieldAttribute.GetDisplayName<SOQuickProcessParameters.siteID>(sender),
									PXErrorLevel.Error));
						sender.RaiseExceptionHandling<SOQuickProcessParameters.createShipment>(
							row,
							row.CreateShipment,
							canShip && siteIsValid
								? null
								: new PXSetPropertyException(Msg.CannotShip, PXErrorLevel.Error));

						quickProcessOk.SetEnabled(canShip && siteIsValid);
					}
					else
					{
						quickProcessOk.SetEnabled(true);
					}
				}
			}

			protected virtual void EnsureSiteID(PXCache sender, SOQuickProcessParameters row)
			{
				if (row.SiteID == null)
				{
					Int32? preferedSiteID = Base.GetPreferedSiteID();
					if (preferedSiteID != null)
						sender.SetValueExt<SOQuickProcessParameters.siteID>(row, preferedSiteID);
				}
			}

			protected virtual void VerifyPrepareInvoice(SOQuickProcessParameters row)
			{
				if (row != null && Base.Document.Current != null)
				{
					SOOrder doc = Base.Document.Current;
					Boolean alreadyHasShipments = doc.ShipmentCntr - doc.OpenShipmentCntr - doc.BilledCntr - doc.ReleasedCntr > 0;
					QuickProcessParameters.Cache.RaiseExceptionHandling<SOQuickProcessParameters.prepareInvoiceFromShipment>(
						row,
						row.PrepareInvoiceFromShipment,
						row.PrepareInvoiceFromShipment == true && alreadyHasShipments
							? new PXSetPropertyException<SOQuickProcessParameters.prepareInvoiceFromShipment>(
								Msg.OnlyCurrentShipmentWillBeInvoiced, PXErrorLevel.Warning)
							: null);
				}
			}
			protected virtual void VerifyEmailInvoice(SOQuickProcessParameters row)
			{
				if (row != null && Base.customer.Current != null)
				{
					QuickProcessParameters.Cache.RaiseExceptionHandling<SOQuickProcessParameters.emailInvoice>(
						row,
						row.EmailInvoice,
						row.EmailInvoice != Base.customer.Current.MailInvoices
							? new PXSetPropertyException<SOQuickProcessParameters.emailInvoice>(
								row.EmailInvoice == true ? Msg.DoNotEmail : Msg.DoEmail,
								PXErrorLevel.Warning,
								Base.customer.Current.AcctCD)
							: null);
				}
			}
			protected virtual void VerifyPrintInvoice(SOQuickProcessParameters row)
			{
				if (row != null && Base.customer.Current != null)
				{
					Boolean? printInvoice = QuickProcessParameters.Cache.GetExtension<SOQuickProcessParametersReportsExt>(row).PrintInvoice;
					QuickProcessParameters.Cache.RaiseExceptionHandling<SOQuickProcessParametersReportsExt.printInvoice>(
						row,
						printInvoice,
						printInvoice != Base.customer.Current.PrintInvoices
							? new PXSetPropertyException<SOQuickProcessParametersReportsExt.printInvoice>(
								printInvoice == true ? Msg.DoNotPrint : Msg.DoPrint,
								PXErrorLevel.Warning,
								Base.customer.Current.AcctCD)
							: null);
				}
			}

			protected virtual Tuple<string, int> OrderAvailabilityStatus(int? SiteID, DateTime? ShipDate)
			{
				if (SiteID == null || ShipDate == null) return new Tuple<string, int>(AvailabilityStatus.NothingToShip, 0);

				var splits = PXSelect<SOLineSplit,
					Where<SOLineSplit.orderType, Equal<Current<SOOrder.orderType>>,
					And<SOLineSplit.orderNbr, Equal<Current<SOOrder.orderNbr>>,
					And<SOLineSplit.completed, Equal<False>>>>>.Select(Base).RowCast<SOLineSplit>()
					.Where(s => (s.SiteID == SiteID || s.ToSiteID == SiteID && s.IsAllocated == true) &&
						(s.LineType == SOLineType.Inventory || s.LineType == SOLineType.NonInventory) &&
						s.RequireShipping == true &&
						s.Operation == SOOperation.Issue).ToList();

				decimal? availabilityFetch(int? InventoryID, int? SubItemID, int? CostCenterID)
				{
					if (!PXTransactionScope.IsScoped)
					{
						using (var cs = new PXConnectionScope())
						using (var ts = new PXTransactionScope())
						{
							try
							{
								// Acuminator disable once PX1043 SavingChangesInEventHandlers Moved from suppression file
								Base.Caches[typeof(ItemLotSerial)].PersistInserted();
								// Acuminator disable once PX1043 SavingChangesInEventHandlers Moved from suppression file
								Base.Caches[typeof(SiteLotSerial)].PersistInserted();
								// Acuminator disable once PX1043 SavingChangesInEventHandlers Moved from suppression file
								// Acuminator disable once PX1043 SavingChangesInEventHandlers Moved from suppression file
								Base.Caches[typeof(SiteStatusByCostCenter)].PersistInserted();
							}
							//TODO: create separate exception class for accumulators validations
							catch (PXException)
							{
								return decimal.MinValue;
							}
						}
					}

					SiteStatusByCostCenter delta = new SiteStatusByCostCenter
					{
						InventoryID = InventoryID,
						SiteID = SiteID,
						SubItemID = SubItemID,
						CostCenterID = CostCenterID
					};
					delta = (SiteStatusByCostCenter)Base.Caches[typeof(SiteStatusByCostCenter)].Insert(delta);

					bool allowShipNegQty = (delta.NegQty == true && Base.soordertype.Current.ShipFullIfNegQtyAllowed == true);
					if (allowShipNegQty)
					{
						INLotSerClass lotSerClass = INLotSerClass.PK.Find(Base, delta.LotSerClassID);
						if (lotSerClass?.LotSerTrack == INLotSerTrack.NotNumbered || lotSerClass?.LotSerAssign == INLotSerAssign.WhenUsed)
						{
							return decimal.MaxValue;
						}
					}

					InventoryItem inventoryItem = InventoryItem.PK.Find(Base, InventoryID);
					// Impossible to perform availability validation in Quick Process for non-stock kits. Availability validation will be ignored.
					if (inventoryItem.StkItem != true && inventoryItem.KitItem == true) return decimal.MaxValue;

					SiteStatusByCostCenter status = PXSelectReadonly<SiteStatusByCostCenter, Where<SiteStatusByCostCenter.inventoryID, Equal<Required<SiteStatusByCostCenter.inventoryID>>,
						And<SiteStatusByCostCenter.siteID, Equal<Required<SiteStatusByCostCenter.siteID>>,
						And<SiteStatusByCostCenter.subItemID, Equal<Required<SiteStatusByCostCenter.subItemID>>,
						And<SiteStatusByCostCenter.costCenterID, Equal<Required<SiteStatusByCostCenter.costCenterID>>>>>>>.Select(Base, InventoryID, SiteID, SubItemID, CostCenterID);

					if (status != null)
					{
						return status.QtyHardAvail + delta.QtyHardAvail;
					}

					return delta.QtyHardAvail;
				};

				var splitsgrouped = splits
					.GroupBy(t => new
					{
						t.InventoryID,
						t.SiteID,
						t.SubItemID,
						Allocated = t.IsAllocated == true && t.SiteID == SiteID,
						RemoteAllocated = t.IsAllocated == true && t.SiteID != SiteID,
						MarkedForPO = t.POCreate == true,
						NonStock = t.LineType == SOLineType.NonInventory,
						FutureShipments = t.ShipDate > ShipDate,
						t.CostCenterID
					})
					.Select(tg => new
					{
						Item = tg.Key,
						SumBaseQty = tg.Sum(q => q.BaseQty),
						SumBaseReceivedQty = tg.Sum(q => q.BaseReceivedQty),
						SumDeduction =
							tg.Key.Allocated == false &&
							tg.Key.MarkedForPO == false &&
							tg.Key.NonStock == false &&
							tg.Key.FutureShipments == false ? tg.Sum(q => q.BaseQty) : 0m,
						AvailableForShipping =
							tg.Key.RemoteAllocated == false &&
							tg.Key.MarkedForPO == false &&
							tg.Key.NonStock == false &&
							tg.Key.FutureShipments == false ? availabilityFetch(tg.Key.InventoryID, tg.Key.SubItemID, tg.Key.CostCenterID) : 0m
					}).ToList();

				var availableForShipping = splitsgrouped.Where(s => !s.Item.FutureShipments && s.SumBaseQty > 0m && (s.AvailableForShipping > 0 || s.Item.NonStock || s.Item.Allocated));
				var remoteAllocated = splitsgrouped.Where(s => !s.Item.FutureShipments && s.SumBaseQty > 0m && s.Item.RemoteAllocated);
				var markedForPO = splitsgrouped.Where(s => !s.Item.FutureShipments && s.SumBaseQty > s.SumBaseReceivedQty && s.Item.MarkedForPO);
				var futureShipments = splitsgrouped.Where(s => s.SumBaseQty > 0m && s.Item.FutureShipments);
				var notAvailableForShipping = splitsgrouped.Where(s => !s.Item.FutureShipments && s.SumDeduction > s.AvailableForShipping && !s.Item.NonStock);

				if (!availableForShipping.Any())
				{
					return new Tuple<string, int>(AvailabilityStatus.NothingToShip, 0);
				}

				int skippedLinesCount = 0;

				if (futureShipments.Any())
				{
					skippedLinesCount = splits
						.Where(line => line.ShipDate > ShipDate)
						.GroupBy(s => new { s.LineNbr })
						.Count();
				}

				if (notAvailableForShipping.Any() || markedForPO.Any() || remoteAllocated.Any())
				{
					switch (Base.Document.Current.ShipComplete)
					{
						case SOShipComplete.ShipComplete:
							return new Tuple<string, int>(AvailabilityStatus.NoItemsAvailableToShip, skippedLinesCount);
						case SOShipComplete.CancelRemainder:
							return new Tuple<string, int>(AvailabilityStatus.CanShipPartCancelRemainder, skippedLinesCount);
						case SOShipComplete.BackOrderAllowed:
							return new Tuple<string, int>(AvailabilityStatus.CanShipPartBackOrder, skippedLinesCount);
					}
				}

				return new Tuple<string, int>(AvailabilityStatus.CanShipAll, skippedLinesCount);
			}

			private void RecalculateAvailabilityStatus(SOQuickProcessParameters row)
			{
				DateTime? shipDate = QuickProcessParameters.Cache.GetExtension<SOQuickProcessParametersShipDateExt>(row).ShipDate;
				var status = OrderAvailabilityStatus(row.SiteID, shipDate);
				QuickProcessParameters.Cache.SetValueExt<SOQuickProcessParametersAvailabilityExt.availabilityStatus>(row, status.Item1);
				QuickProcessParameters.Cache.SetValueExt<SOQuickProcessParametersAvailabilityExt.skipByDateMsg>(row, status.Item2 == 0 ? "" : PXLocalizer.LocalizeFormat(Msg.SomeLinesWillBeSkipedDueToDateSelection, status.Item2));
				QuickProcessParameters.Cache.RaiseRowSelected(QuickProcessParameters.Current);
			}

			public static void InitQuickProcessPanel(PXGraph graph, string viewName)
			{
				var ext = ((SOOrderEntry)graph).SOQuickProcessExt;
				if (string.IsNullOrEmpty(ext.QuickProcessParameters.Current.OrderType))
				{
					ext.QuickProcessParameters.Cache.Clear();
					ext.QuickProcessParameters.Insert(PXSelectReadonly<SOQuickProcessParameters, Where<SOQuickProcessParameters.orderType, Equal<Current<SOOrder.orderType>>>>.Select(ext.Base));
				}

				if (ext.QuickProcessParameters.Current.CreateShipment == true)
				{
					ext.EnsureSiteID(ext.QuickProcessParameters.Cache, ext.QuickProcessParameters.Current);
					DateTime? shipDate = ext.Base.Accessinfo.BusinessDate > ext.Base.Document.Current.ShipDate ? ext.Base.Accessinfo.BusinessDate : ext.Base.Document.Current.ShipDate;
					SOQuickProcessParametersShipDateExt.SetDate(ext.QuickProcessParameters.Cache, ext.QuickProcessParameters.Current, shipDate.Value);
					ext.RecalculateAvailabilityStatus(ext.QuickProcessParameters.Current);
				}
			}
		}

		public CarrierRates CarrierRatesExt => FindImplementation<CarrierRates>();
		public class CarrierRates : CarrierRatesExtension<SOOrderEntry, SOOrder>
		{
            public virtual void RecalculatePackagesForOrder(SOOrder order) => base.RecalculatePackagesForOrder(Documents.Cache.GetExtension<Document>(order));
			public virtual CarrierRequest BuildRateRequest(SOOrder order) => base.BuildRateRequest(Documents.Cache.GetExtension<Document>(order));
			public virtual CarrierRequest BuildQuoteRequest(SOOrder order, CarrierPlugin plugin) => base.BuildQuoteRequest(Documents.Cache.GetExtension<Document>(order), plugin);

			protected override DocumentMapping GetDocumentMapping() => new DocumentMapping(typeof(SOOrder)) { DocumentDate = typeof(SOOrder.orderDate) };

			protected override CarrierRequest GetCarrierRequest(Document doc, UnitsType unit, List<string> methods, List<CarrierBoxEx> boxes)
			{
				var order = (SOOrder)Documents.Cache.GetMain(doc);

				SOShippingAddress shipAddress = Base.Shipping_Address.Select();
				BAccount companyAccount = PXSelectJoin<BAccountR, InnerJoin<Branch, On<Branch.bAccountID, Equal<BAccountR.bAccountID>>>, Where<Branch.branchID, Equal<Required<Branch.branchID>>>>.Select(Base, Base.Accessinfo.BranchID);
				Address companyAddress = PXSelect<Address, Where<Address.addressID, Equal<Required<Address.addressID>>>>.Select(Base, companyAccount.DefAddressID);
				SOShippingContact shipContact = Base.Shipping_Contact.Select();
				Contact companyContact = PXSelect<Contact, Where<Contact.contactID, Equal<Required<Contact.contactID>>>>.Select(Base, companyAccount.DefContactID);

				CarrierRequest cr = new CarrierRequest(unit, order.CuryID);
				cr.Shipper = companyAddress;
				cr.Origin = null;
				cr.Destination = shipAddress;
				cr.PackagesEx = boxes;
				cr.Resedential = order.Resedential == true;
				cr.SaturdayDelivery = order.SaturdayDelivery == true;
				cr.Insurance = order.Insurance == true;
				cr.ShipDate = Tools.Max(Base.Accessinfo.BusinessDate.Value.Date, order.ShipDate.Value);
				cr.Methods = methods;
				cr.Attributes = new List<string>();
				cr.InvoiceLineTotal = Base.Document.Current.CuryLineTotal.GetValueOrDefault();
				cr.ShipperContact = companyContact;
				cr.DestinationContact = shipContact;

				if (order.GroundCollect == true && Base.CanUseGroundCollect(order))
					cr.Attributes.Add("COLLECT");

				return cr;
			}

			protected override IList<SOPackageEngine.PackSet> GetPackages(Document doc, bool suppressRecalc)
			{
				var order = (SOOrder)Documents.Cache.GetMain(doc);
				return order.IsPackageValid == true || order.IsManualPackage == true || suppressRecalc
					? GetPackages(order)
					: CalculatePackages(doc, null);
			}

			protected virtual IList<SOPackageEngine.PackSet> GetPackages(SOOrder order)
			{
				Dictionary<int, SOPackageEngine.PackSet> packs = new Dictionary<int, SOPackageEngine.PackSet>();
				foreach (SOPackageInfoEx package in Base.Packages.View.SelectMultiBound(new object[] { order }))
				{
					SOPackageEngine.PackSet set = null;
					if (!packs.ContainsKey(package.SiteID.Value))
					{
						set = new SOPackageEngine.PackSet(package.SiteID.Value);
						packs.Add(set.SiteID, set);
					}
					else
					{
						set = packs[package.SiteID.Value];
					}

					set.Packages.Add(package);
				}

				return packs.Values.ToList();
			}

			protected virtual void _(Events.FieldUpdated<SOPackageInfoEx, SOPackageInfoEx.boxID> e)
			{
				if (e.Row != null)
				{
					Base.Packages.Cache.SetDefaultExt<SOPackageInfoEx.description>(e.Row);
					Base.Packages.Cache.SetDefaultExt<SOPackageInfoEx.carrierBox>(e.Row);
					Base.Packages.Cache.SetDefaultExt<SOPackageInfoEx.length>(e.Row);
					Base.Packages.Cache.SetDefaultExt<SOPackageInfoEx.width>(e.Row);
					Base.Packages.Cache.SetDefaultExt<SOPackageInfoEx.height>(e.Row);
					Base.Packages.Cache.SetDefaultExt<SOPackageInfoEx.boxWeight>(e.Row);
					Base.Packages.Cache.SetDefaultExt<SOPackageInfoEx.maxWeight>(e.Row);
				}
			}

			protected override void ValidatePackages()
			{
				if (Base.Document.Current.IsManualPackage == true)
				{
					PXResultset<SOPackageInfoEx> resultset = Base.Packages.Select();

					if (resultset.Count == 0)
						throw new PXException(Messages.AtleastOnePackageIsRequired);
					else
					{
						bool failed = false;
						foreach (SOPackageInfoEx p in resultset)
						{
							if (p.SiteID == null)
							{
								Base.Packages.Cache.RaiseExceptionHandling<SOPackageInfoEx.siteID>(p, null, new PXSetPropertyException(ErrorMessages.FieldIsEmpty, PXErrorLevel.Error, $"[{nameof(SOPackageInfoEx.siteID)}]"));
								failed = true;
							}
						}
						if (failed)
							throw new PXException(Messages.AtleastOnePackageIsInvalid);
					}
				}
			}

			protected override void RateHasBeenSelected(SOCarrierRate cr)
			{
				if (Base.CollectFreight)
				{
					decimal baseCost = Base.ConvertAmtToBaseCury(Base.Document.Current.CuryID, arsetup.Current.DefaultRateTypeID, Base.Document.Current.OrderDate.Value, cr.Amount.Value);
					Base.SetFreightCost(baseCost);
				}
			}

			protected override WebDialogResult AskForRateSelection() => Base.DocumentProperties.AskExt();

			protected override void ClearPackages(Document doc)
			{
				foreach (SOPackageInfoEx package in Base.Packages.View.SelectMultiBound(new object[] { Documents.Cache.GetMain(doc) }))
					Base.Packages.Delete(package);
			}

			protected override void InsertPackages(IEnumerable<SOPackageInfoEx> packages)
			{
				foreach (SOPackageInfoEx package in packages)
					Base.Packages.Insert(package);
			}

			protected override IEnumerable<Tuple<ILineInfo, InventoryItem>> GetLines(Document doc)
			{
				var order = (SOOrder)Documents.Cache.GetMain(doc);

				return 
					PXSelectJoin<SOLine,
					InnerJoin<InventoryItem, On<SOLine.FK.InventoryItem>>,
					Where<SOLine.orderType, Equal<Required<SOOrder.orderType>>,
						And<SOLine.orderNbr, Equal<Required<SOOrder.orderNbr>>>>,
					OrderBy<Asc<SOLine.orderType, Asc<SOLine.orderNbr, Asc<SOLine.lineNbr>>>>>
					.Select(Base, order.OrderType, order.OrderNbr).AsEnumerable()
					.Cast<PXResult<SOLine, InventoryItem>>()
					.Select(r => Tuple.Create<ILineInfo, InventoryItem>(new LineInfo(r), r));
			}

			protected override IEnumerable<CarrierPlugin> GetApplicableCarrierPlugins()
			{
				var orderSites = new Lazy<SOOrderSite[]>(() => Base.SiteList.Select().RowCast<SOOrderSite>().ToArray());
				return base.GetApplicableCarrierPlugins()
					.Where(p => p.SiteID == null || orderSites.Value.Any(s => s.SiteID == p.SiteID));
			}

			private class LineInfo : ILineInfo
			{
				private SOLine _line;
				public LineInfo(SOLine line) { _line = line; }

				public decimal? BaseQty => _line.BaseQty;
				public decimal? CuryLineAmt => _line.CuryLineAmt;
				public decimal? ExtWeight => _line.ExtWeight;
				public int? SiteID => _line.SiteID;
				public string Operation => _line.Operation;
			}
		}

		#endregion

		#region Address Lookup Extension
		/// <exclude/>
		public class SOOrderEntryAddressLookupExtension : CR.Extensions.AddressLookupExtension<SOOrderEntry, SOOrder, SOBillingAddress>
		{
			protected override string AddressView => nameof(Base.Billing_Address);
		}

		/// <exclude/>
		public class SOOrderEntryShippingAddressLookupExtension : CR.Extensions.AddressLookupExtension<SOOrderEntry, SOOrder, SOShippingAddress>
		{
			protected override string AddressView => nameof(Base.Shipping_Address);
		}

		public class SOOrderEntryBillingAddressCachingHelper : AddressValidationExtension<SOOrderEntry, SOBillingAddress>
		{
			protected override IEnumerable<PXSelectBase<SOBillingAddress>> AddressSelects()
			{
				yield return Base.Billing_Address;
			}
		}

		public class SOOrderEntryShippingAddressCachingHelper : AddressValidationExtension<SOOrderEntry, SOShippingAddress>
		{
			protected override IEnumerable<PXSelectBase<SOShippingAddress>> AddressSelects()
			{
				yield return Base.Shipping_Address;
			}
		}
		#endregion

		/// <exclude/>
		public class ExtensionSort
		: SortExtensionsBy<ExtensionOrderFor<SOOrderEntry>
			.FilledWith<
				SOLineSplitPlan,
				Blanket
			>>
		{ }
	}

	[Serializable()]
	public partial class AddInvoiceFilter : IBqlTable
	{
		#region DocType
		public abstract class docType : PX.Data.BQL.BqlString.Field<docType> { }
		protected String _DocType;
		[PXString(3, IsFixed = true)]
		[PXDefault(ARDocType.Invoice)]
		[PXStringList(
				new string[] { ARDocType.Invoice, ARDocType.CashSale, ARDocType.DebitMemo },
				new string[] { AR.Messages.Invoice, AR.Messages.CashSale, AR.Messages.DebitMemo })]
		[PXUIField(DisplayName = "Type")]
		public virtual String DocType
		{
			get
			{
				return this._DocType;
			}
			set
			{
				this._DocType = value;
			}
		}
		#endregion
		#region RefNbr
		public abstract class refNbr : PX.Data.BQL.BqlString.Field<refNbr> { }
		protected string _RefNbr;
		[PXString(15, IsUnicode = true, InputMask = "")]
		[PXDefault()]
		[PXUIField(DisplayName = "Reference Nbr.", Visibility = PXUIVisibility.SelectorVisible, TabOrder = 1)]
		[ARInvoiceType.RefNbr(typeof(Search2<AR.Standalone.ARRegisterAlias.refNbr,
			InnerJoinSingleTable<ARInvoice, On<ARInvoice.docType, Equal<AR.Standalone.ARRegisterAlias.docType>,
				And<ARInvoice.refNbr, Equal<AR.Standalone.ARRegisterAlias.refNbr>>>>,
			Where<AR.Standalone.ARRegisterAlias.docType, Equal<Optional<AddInvoiceFilter.docType>>,
				And<AR.Standalone.ARRegisterAlias.released, Equal<boolTrue>,
				And<AR.Standalone.ARRegisterAlias.origModule, Equal<BatchModule.moduleSO>,
				And<AR.Standalone.ARRegisterAlias.customerID, Equal<Current<SOOrder.customerID>>>>>>,
			OrderBy<Desc<AR.Standalone.ARRegisterAlias.refNbr>>>), Filterable = true)]
		[PXRestrictor(typeof(Where<ARRegisterAlias.canceled, NotEqual<True>>),
			Messages.InvoiceCanceled, typeof(ARRegisterAlias.refNbr))]
		[PXRestrictor(typeof(Where<ARRegisterAlias.isUnderCorrection, NotEqual<True>>),
			Messages.InvoiceUnderCorrection, typeof(ARRegisterAlias.refNbr))]
		[PXFormula(typeof(Default<AddInvoiceFilter.docType>))]
		public virtual String RefNbr
		{
			get
			{
				return this._RefNbr;
			}
			set
			{
				this._RefNbr = value;
			}
		}
		#endregion
		#region Expand
		public abstract class expand : PX.Data.BQL.BqlBool.Field<expand> { }
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Show Non-Stock Kits by Components")]
		public virtual Boolean? Expand
		{
			get; set;
		}
		#endregion
	}
	[Serializable]
	public partial class SOParamFilter : IBqlTable
	{
		#region ShipDate
		public abstract class shipDate : PX.Data.BQL.BqlDateTime.Field<shipDate> { }
		protected DateTime? _ShipDate;
		[PXDate]
		[PXUIField(DisplayName = "Shipment Date", Required = true)]
		public virtual DateTime? ShipDate
		{
			get
			{
				return this._ShipDate;
			}
			set
			{
				this._ShipDate = value;
			}
		}
		#endregion
		#region SiteID
		public abstract class siteID : PX.Data.BQL.BqlInt.Field<siteID> { }
		protected Int32? _SiteID;
		[PXDBInt()]
		[PXUIField(DisplayName = "Warehouse ID", Required = true, FieldClass = SiteAttribute.DimensionName)]
		[OrderSiteSelector()]
		public virtual Int32? SiteID
		{
			get
			{
				return this._SiteID;
			}
			set
			{
				this._SiteID = value;
			}
		}
		#endregion
	}

	[Serializable()]
	public partial class CopyParamFilter : IBqlTable
	{
		#region OrderType
		public abstract class orderType : PX.Data.BQL.BqlString.Field<orderType> { }
		protected String _OrderType;
		[PXDBString(2, IsFixed = true, InputMask = ">aa")]
		[PXDefault(typeof(Search<SOSetup.defaultOrderType>))]
		[PXSelector(typeof(Search2<SOOrderType.orderType, 
			InnerJoin<SOOrderTypeOperation, 
				On2<SOOrderTypeOperation.FK.OrderType, And<SOOrderTypeOperation.operation, Equal<SOOrderType.defaultOperation>>>>>))]
		[PXRestrictor(typeof(Where<SOOrderTypeOperation.iNDocType, NotEqual<INTranType.transfer>, Or<FeatureInstalled<FeaturesSet.warehouse>>>), ErrorMessages.ElementDoesntExist, typeof(CopyParamFilter.orderType))]
		[PXRestrictor(typeof(Where<SOOrderType.requireAllocation, NotEqual<True>, Or<AllocationAllowed>>), ErrorMessages.ElementDoesntExist, typeof(CopyParamFilter.orderType))]
		[PXRestrictor(typeof(Where<SOOrderType.active, Equal<True>>), ErrorMessages.ElementDoesntExist, typeof(CopyParamFilter.orderType))]
		[PXRestrictor(typeof(Where<SOOrderType.behavior, NotEqual<SOBehavior.bL>>), ErrorMessages.ElementDoesntExist, typeof(CopyParamFilter.orderType))]
		[PXUIField(DisplayName = "Order Type")]
		public virtual String OrderType
		{
			get
			{
				return this._OrderType;
			}
			set
			{
				this._OrderType = value;
			}
		}
		#endregion
		#region RecalcUnitPrices
		public abstract class recalcUnitPrices : PX.Data.BQL.BqlBool.Field<recalcUnitPrices> { }
		protected Boolean? _RecalcUnitPrices;
		[PXDBBool()]
		[PXDefault(true)]
		[PXUIField(DisplayName = "Recalculate Unit Prices")]
		public virtual Boolean? RecalcUnitPrices
		{
			get
			{
				return this._RecalcUnitPrices;
			}
			set
			{
				this._RecalcUnitPrices = value;
			}
		}
		#endregion
		#region OverrideManualPrices
		public abstract class overrideManualPrices : PX.Data.BQL.BqlBool.Field<overrideManualPrices> { }
		protected Boolean? _OverrideManualPrices;
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Override Manual Prices")]
		[PXFormula(typeof(False.When<recalcUnitPrices.IsEqual<False>>.Else<overrideManualPrices>))]
		public virtual Boolean? OverrideManualPrices
		{
			get
			{
				return this._OverrideManualPrices;
			}
			set
			{
				this._OverrideManualPrices = value;
			}
		}
		#endregion
		#region RecalcDiscounts
		public abstract class recalcDiscounts : PX.Data.BQL.BqlBool.Field<recalcDiscounts> { }
		protected Boolean? _RecalcDiscounts;
		[PXDBBool()]
		[PXDefault(true)]
		[PXUIField(DisplayName = "Recalculate Discounts")]
		public virtual Boolean? RecalcDiscounts
		{
			get
			{
				return this._RecalcDiscounts;
			}
			set
			{
				this._RecalcDiscounts = value;
			}
		}
		#endregion
		#region OverrideManualDiscounts
		public abstract class overrideManualDiscounts : PX.Data.BQL.BqlBool.Field<overrideManualDiscounts> { }
		protected Boolean? _OverrideManualDiscounts;
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Override Manual Discounts")]
		[PXFormula(typeof(False.When<recalcDiscounts.IsEqual<False>>.Else<overrideManualDiscounts>))]
		public virtual Boolean? OverrideManualDiscounts
		{
			get
			{
				return this._OverrideManualDiscounts;
			}
			set
			{
				this._OverrideManualDiscounts = value;
			}
		}
		#endregion
		#region OrderNbr
		public abstract class orderNbr : PX.Data.BQL.BqlString.Field<orderNbr> { }
		protected String _OrderNbr;
		[PXDBString(15, IsUnicode = true, InputMask = ">CCCCCCCCCCCCCCC")]
		[PXUIField(DisplayName = "Order Nbr.", Visibility = PXUIVisibility.SelectorVisible)]
		[PXDefault(typeof(Search2<Numbering.newSymbol, InnerJoin<SOOrderType, On<Numbering.numberingID, Equal<SOOrderType.orderNumberingID>>>, Where<SOOrderType.orderType, Equal<Current<CopyParamFilter.orderType>>, And<Numbering.userNumbering, Equal<False>>>>))]
		public virtual String OrderNbr
		{
			get
			{
				return this._OrderNbr;
			}
			set
			{
				this._OrderNbr = value;
			}
		}
		#endregion
	}

	[Serializable]
	public partial class SOSiteStatusFilter : INSiteStatusFilter
	{
		#region SiteID
		public new abstract class siteID : PX.Data.BQL.BqlInt.Field<siteID> { }

		[PXUIField(DisplayName = "Warehouse")]
		[SiteAttribute]
		[InterBranchRestrictor(typeof(Where2<SameOrganizationBranch<INSite.branchID, Current<SOOrder.branchID>>,
			Or<Current<SOOrder.behavior>, Equal<SOBehavior.qT>>>))]
		[PXDefault(typeof(INRegister.siteID), PersistingCheck = PXPersistingCheck.Nothing)]
		public override Int32? SiteID
		{
			get
			{
				return this._SiteID;
			}
			set
			{
				this._SiteID = value;
			}
		}
		#endregion
		#region Inventory
		public new abstract class inventory : PX.Data.BQL.BqlString.Field<inventory> { }
		#endregion
		#region Mode
		public abstract class mode : PX.Data.BQL.BqlInt.Field<mode> { }
		protected int? _Mode;
		[PXDBInt]
		[PXDefault(0)]
		[PXUIField(DisplayName = "Selection Mode")]
		[SOAddItemMode.List]
		public virtual int? Mode
		{
			get
			{
				return _Mode;
			}
			set
			{
				_Mode = value;
			}
		}
		#endregion
		#region HistoryDate
		public abstract class historyDate : PX.Data.BQL.BqlDateTime.Field<historyDate> { }
		protected DateTime? _HistoryDate;
		[PXDBDate()]
		[PXUIField(DisplayName = "Sold Since")]
		public virtual DateTime? HistoryDate
		{
			get
			{
				return this._HistoryDate;
			}
			set
			{
				this._HistoryDate = value;
			}
		}
		#endregion
		#region DropShipSales
		public abstract class dropShipSales : PX.Data.BQL.BqlBool.Field<dropShipSales> { }
		[PXDBBool]
		[PXDefault(false)]
		[PXFormula(typeof(Default<mode>))]
		[PXUIField(DisplayName = "Show Drop-Ship Sales")]
		public virtual bool? DropShipSales
		{
			get;
			set;
		}
		#endregion
		#region Behavior
		public abstract class behavior : Data.BQL.BqlString.Field<behavior> { }
		[PXDBString(2, IsFixed = true, InputMask = ">aa")]
		public virtual string Behavior
		{
			get;
			set;
		}
		#endregion
		#region CustomerLocationID
		public abstract class customerLocationID : Data.BQL.BqlInt.Field<customerLocationID> { }
		[LocationActive(typeof(Where<Location.bAccountID, Equal<Current<SOOrder.customerID>>,
			And<MatchWithBranch<Location.cBranchID>>>), DescriptionField = typeof(Location.descr))]
		[PXUIField(DisplayName = "Ship-To Location")]
		public virtual int? CustomerLocationID
		{
			get;
			set;
		}
		#endregion
	}

	public class SOAddItemMode
	{
		public const int BySite = 0;
		public const int ByCustomer = 1;

		public class ListAttribute : PXIntListAttribute
		{
			public ListAttribute() : base(
				new[]
			{
					Pair(BySite, Messages.BySite),
					Pair(ByCustomer, Messages.ByCustomer),
				})
			{ }
		}

		public class bySite : PX.Data.BQL.BqlInt.Constant<bySite> { public bySite() : base(BySite) { } }
		public class byCustomer : PX.Data.BQL.BqlInt.Constant<byCustomer> { public byCustomer() : base(ByCustomer) { } }
	}

	[PXProjection(typeof(Select2<InventoryItem,
		LeftJoin<INSiteStatusByCostCenter,
			On<INSiteStatusByCostCenter.inventoryID, Equal<InventoryItem.inventoryID>,
				And<InventoryItem.stkItem, Equal<boolTrue>,
				And<INSiteStatusByCostCenter.siteID, NotEqual<SiteAttribute.transitSiteID>,
				And<INSiteStatusByCostCenter.costCenterID, Equal<CostCenter.freeStock>>>>>,
		LeftJoin<INSubItem,
			On<INSiteStatusByCostCenter.FK.SubItem>,
		LeftJoin<INSite,
			On2<INSiteStatusByCostCenter.FK.Site,
				And<INSite.baseCuryID, EqualBaseCuryID<Current2<SOOrder.branchID>>>>,
		LeftJoin<INItemXRef,
			On<INItemXRef.inventoryID, Equal<InventoryItem.inventoryID>,
				And2<Where<INItemXRef.subItemID, Equal<INSiteStatusByCostCenter.subItemID>,
						Or<INSiteStatusByCostCenter.subItemID, IsNull>>,
				And<Where<CurrentValue<SOSiteStatusFilter.barCode>, IsNotNull,
				And<INItemXRef.alternateType, In3<INAlternateType.barcode, INAlternateType.gIN>>>>>>,
		LeftJoin<INItemPartNumber,
			On<INItemPartNumber.inventoryID, Equal<InventoryItem.inventoryID>,
				And<INItemPartNumber.alternateID, Like<CurrentValue<SOSiteStatusFilter.inventory_Wildcard>>,
				And2<Where<INItemPartNumber.bAccountID, Equal<Zero>,
					Or<INItemPartNumber.bAccountID, Equal<CurrentValue<SOOrder.customerID>>,
					Or<INItemPartNumber.alternateType, Equal<INAlternateType.vPN>>>>,
				And<Where<INItemPartNumber.subItemID, Equal<INSiteStatusByCostCenter.subItemID>,
					Or<INSiteStatusByCostCenter.subItemID, IsNull>>>>>>,
		LeftJoin<INItemClass,
			On<InventoryItem.FK.ItemClass>,
		LeftJoin<INPriceClass,
			On<INPriceClass.priceClassID, Equal<InventoryItem.priceClassID>>,
		LeftJoin<InventoryItemCurySettings,
			On<InventoryItemCurySettings.inventoryID, Equal<InventoryItem.inventoryID>,
				And<InventoryItemCurySettings.curyID, EqualBaseCuryID<Current2<SOOrder.branchID>>>>,
		LeftJoin<BAccountR,
			On<BAccountR.bAccountID, Equal<InventoryItemCurySettings.preferredVendorID>>,
		LeftJoin<INItemCustSalesStats,
			On<CurrentValue<SOSiteStatusFilter.mode>, Equal<SOAddItemMode.byCustomer>,
				And<INItemCustSalesStats.inventoryID, Equal<InventoryItem.inventoryID>,
				And<INItemCustSalesStats.subItemID, Equal<INSiteStatusByCostCenter.subItemID>,
				And<INItemCustSalesStats.siteID, Equal<INSiteStatusByCostCenter.siteID>,
				And<INItemCustSalesStats.bAccountID, Equal<CurrentValue<SOOrder.customerID>>,
				And<Where<INItemCustSalesStats.lastDate, GreaterEqual<CurrentValue<SOSiteStatusFilter.historyDate>>,
					Or<CurrentValue<SOSiteStatusFilter.dropShipSales>, Equal<True>,
						And<INItemCustSalesStats.dropShipLastDate, GreaterEqual<CurrentValue<SOSiteStatusFilter.historyDate>>>>>>>>>>>,
		LeftJoin<INUnit,
			On<INUnit.inventoryID, Equal<InventoryItem.inventoryID>,
				And<INUnit.unitType, Equal<INUnitType.inventoryItem>,
				And<INUnit.fromUnit, Equal<InventoryItem.salesUnit>,
				And<INUnit.toUnit, Equal<InventoryItem.baseUnit>>>>>
			>>>>>>>>>>>,
		Where<CurrentValue<SOOrder.customerID>, IsNotNull,
			And2<CurrentMatch<InventoryItem, AccessInfo.userName>,
			And2<Where<INSiteStatusByCostCenter.siteID, IsNull, Or<INSite.branchID, IsNotNull, And2<CurrentMatch<INSite, AccessInfo.userName>,
				And<Where2<FeatureInstalled<FeaturesSet.interBranch>,
					Or2<SameOrganizationBranch<INSite.branchID, Current<SOOrder.branchID>>,
					Or<CurrentValue<SOOrder.behavior>, Equal<SOBehavior.qT>>>>>>>>,
			And2<Where<INSiteStatusByCostCenter.subItemID, IsNull, Or<CurrentMatch<INSubItem, AccessInfo.userName>>>,
			And2<Where<CurrentValue<INSiteStatusFilter.onlyAvailable>, Equal<boolFalse>,
				Or<INSiteStatusByCostCenter.qtyAvail, Greater<CS.decimal0>>>,
			And2<Where<CurrentValue<SOSiteStatusFilter.mode>, Equal<SOAddItemMode.bySite>,
				Or<INItemCustSalesStats.lastQty, Greater<decimal0>,
				Or<CurrentValue<SOSiteStatusFilter.dropShipSales>, Equal<True>, And<INItemCustSalesStats.dropShipLastQty, Greater<decimal0>>>>>,
			And<InventoryItem.isTemplate, Equal<False>,
			And<InventoryItem.itemStatus, NotIn3<
				InventoryItemStatus.unknown,
				InventoryItemStatus.inactive,
				InventoryItemStatus.markedForDeletion,
				InventoryItemStatus.noSales>>>>>>>>>>), Persistent = false)]
	public partial class SOSiteStatusSelected : IBqlTable
	{
		#region Selected
		public abstract class selected : PX.Data.BQL.BqlBool.Field<selected> { }
		protected bool? _Selected = false;
		[PXBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Selected")]
		public virtual bool? Selected
		{
			get
			{
				return _Selected;
			}
			set
			{
				_Selected = value;
			}
		}
		#endregion

		#region InventoryID
		public abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }
		protected Int32? _InventoryID;
		[Inventory(BqlField = typeof(InventoryItem.inventoryID), IsKey = true)]
		[PXDefault()]
		public virtual Int32? InventoryID
		{
			get
			{
				return this._InventoryID;
			}
			set
			{
				this._InventoryID = value;
			}
		}
		#endregion

		#region InventoryCD
		public abstract class inventoryCD : PX.Data.BQL.BqlString.Field<inventoryCD> { }
		protected string _InventoryCD;
		[PXDefault()]
		[InventoryRaw(BqlField = typeof(InventoryItem.inventoryCD))]
		public virtual String InventoryCD
		{
			get
			{
				return this._InventoryCD;
			}
			set
			{
				this._InventoryCD = value;
			}
		}
		#endregion

		#region Descr
		public abstract class descr : PX.Data.BQL.BqlString.Field<descr> { }

		protected string _Descr;
		[PXDBLocalizableString(Common.Constants.TranDescLength, IsUnicode = true, BqlField = typeof(InventoryItem.descr), IsProjection = true)]
		[PXUIField(DisplayName = "Description", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual String Descr
		{
			get
			{
				return this._Descr;
			}
			set
			{
				this._Descr = value;
			}
		}
		#endregion

		#region ItemClassID
		public abstract class itemClassID : PX.Data.BQL.BqlInt.Field<itemClassID> { }
		protected int? _ItemClassID;
		[PXDBInt(BqlField = typeof(InventoryItem.itemClassID))]
		[PXUIField(DisplayName = "Item Class ID", Visible = false)]
		[PXDimensionSelector(INItemClass.Dimension, typeof(INItemClass.itemClassID), typeof(INItemClass.itemClassCD), ValidComboRequired = true)]
		public virtual int? ItemClassID
		{
			get
			{
				return this._ItemClassID;
			}
			set
			{
				this._ItemClassID = value;
			}
		}
		#endregion

		#region ItemClassCD
		public abstract class itemClassCD : PX.Data.BQL.BqlString.Field<itemClassCD> { }
		protected string _ItemClassCD;
		[PXDBString(30, IsUnicode = true, BqlField = typeof(INItemClass.itemClassCD))]
		public virtual string ItemClassCD
		{
			get
			{
				return this._ItemClassCD;
			}
			set
			{
				this._ItemClassCD = value;
			}
		}
		#endregion

		#region ItemClassDescription
		public abstract class itemClassDescription : PX.Data.BQL.BqlString.Field<itemClassDescription> { }
		protected String _ItemClassDescription;
		[PXDBLocalizableString(Common.Constants.TranDescLength, IsUnicode = true, BqlField = typeof(INItemClass.descr), IsProjection = true)]
		[PXUIField(DisplayName = "Item Class Description", Visible = false, ErrorHandling = PXErrorHandling.Always)]
		public virtual String ItemClassDescription
		{
			get
			{
				return this._ItemClassDescription;
			}
			set
			{
				this._ItemClassDescription = value;
			}
		}
		#endregion

		#region PriceClassID
		public abstract class priceClassID : PX.Data.BQL.BqlString.Field<priceClassID> { }

		protected string _PriceClassID;
		[PXDBString(10, IsUnicode = true, BqlField = typeof(InventoryItem.priceClassID))]
		[PXUIField(DisplayName = "Price Class ID", Visible = false)]
		public virtual String PriceClassID
		{
			get
			{
				return this._PriceClassID;
			}
			set
			{
				this._PriceClassID = value;
			}
		}
		#endregion

		#region PriceClassDescription
		public abstract class priceClassDescription : PX.Data.BQL.BqlString.Field<priceClassDescription> { }
		protected String _PriceClassDescription;
		[PXDBString(Common.Constants.TranDescLength, IsUnicode = true, BqlField = typeof(INPriceClass.description))]
		[PXUIField(DisplayName = "Price Class Description", Visible = false, ErrorHandling = PXErrorHandling.Always)]
		public virtual String PriceClassDescription
		{
			get
			{
				return this._PriceClassDescription;
			}
			set
			{
				this._PriceClassDescription = value;
			}
		}
		#endregion

		#region PreferredVendorID
		public abstract class preferredVendorID : PX.Data.BQL.BqlInt.Field<preferredVendorID> { }

		protected Int32? _PreferredVendorID;
		[AP.VendorNonEmployeeActive(DisplayName = "Preferred Vendor ID", Required = false, DescriptionField = typeof(BAccountR.acctName), BqlField = typeof(InventoryItemCurySettings.preferredVendorID), Visible = false, ErrorHandling = PXErrorHandling.Always)]
		public virtual Int32? PreferredVendorID
		{
			get
			{
				return this._PreferredVendorID;
			}
			set
			{
				this._PreferredVendorID = value;
			}
		}
		#endregion

		#region PreferredVendorDescription
		public abstract class preferredVendorDescription : PX.Data.BQL.BqlString.Field<preferredVendorDescription> { }
		protected String _PreferredVendorDescription;
		[PXDBString(250, IsUnicode = true, BqlField = typeof(BAccountR.acctName))]
		[PXUIField(DisplayName = "Preferred Vendor Name", Visible = false, ErrorHandling = PXErrorHandling.Always)]
		public virtual String PreferredVendorDescription
		{
			get
			{
				return this._PreferredVendorDescription;
			}
			set
			{
				this._PreferredVendorDescription = value;
			}
		}
		#endregion

		#region BarCode
		public abstract class barCode : PX.Data.BQL.BqlString.Field<barCode> { }
		protected String _BarCode;
		[PXDBString(255, BqlField = typeof(INItemXRef.alternateID), IsUnicode = true)]
		[PXUIField(DisplayName = "Barcode", Visible = false)]
		public virtual String BarCode
		{
			get
			{
				return this._BarCode;
			}
			set
			{
				this._BarCode = value;
			}
		}
		#endregion

		#region AlternateID
		public abstract class alternateID : PX.Data.BQL.BqlString.Field<alternateID> { }
		protected String _AlternateID;
		[PXString(225, IsUnicode = true, InputMask = "")]
		[PXDBCalced(typeof(IsNull<INItemXRef.alternateID, INItemPartNumber.alternateID>), typeof(string))]
		[PXUIField(DisplayName = "Alternate ID")]
		[PXExtraKey]
		public virtual String AlternateID
		{
			get
			{
				return this._AlternateID;
			}
			set
			{
				this._AlternateID = value;
			}
		}
		#endregion

		#region AlternateType
		public abstract class alternateType : PX.Data.BQL.BqlString.Field<alternateType> { }
		protected String _AlternateType;
		[PXString(4)]
		[PXDBCalced(typeof(IsNull<INItemXRef.alternateType, INItemPartNumber.alternateType>), typeof(string))]
		[INAlternateType.List()]
		[PXDefault(INAlternateType.Global)]
		[PXUIField(DisplayName = "Alternate Type")]
		public virtual String AlternateType
		{
			get
			{
				return this._AlternateType;
			}
			set
			{
				this._AlternateType = value;
			}
		}
		#endregion

		#region Descr
		public abstract class alternateDescr : PX.Data.BQL.BqlString.Field<alternateDescr> { }
		protected String _AlternateDescr;
		[PXString(60, IsUnicode = true)]
		[PXDBCalced(typeof(IsNull<INItemXRef.descr, INItemPartNumber.descr>), typeof(string))]
		[PXUIField(DisplayName = "Alternate Description", Visible = false)]
		public virtual String AlternateDescr
		{
			get
			{
				return this._AlternateDescr;
			}
			set
			{
				this._AlternateDescr = value;
			}
		}
		#endregion

		#region SiteID
		public abstract class siteID : PX.Data.BQL.BqlInt.Field<siteID> { }
		protected int? _SiteID;
		[PXUIField(DisplayName = "Warehouse")]
		[SiteAttribute(BqlField = typeof(INSiteStatusByCostCenter.siteID))]
		public virtual Int32? SiteID
		{
			get
			{
				return this._SiteID;
			}
			set
			{
				this._SiteID = value;
			}
		}
		#endregion

		#region SiteCD
		public abstract class siteCD : PX.Data.BQL.BqlString.Field<siteCD> { }
		protected String _SiteCD;
		[PXString(IsUnicode = true, IsKey = true)]
		[PXDBCalced(typeof(IsNull<Data.RTrim<INSite.siteCD>, Empty>), typeof(string))]
		public virtual String SiteCD
		{
			get
			{
				return this._SiteCD;
			}
			set
			{
				this._SiteCD = value;
			}
		}
		#endregion

		#region SubItemID
		public abstract class subItemID : PX.Data.BQL.BqlInt.Field<subItemID> { }
		protected int? _SubItemID;
		[SubItem(typeof(SOSiteStatusSelected.inventoryID), BqlField = typeof(INSubItem.subItemID))]
		public virtual Int32? SubItemID
		{
			get
			{
				return this._SubItemID;
			}
			set
			{
				this._SubItemID = value;
			}
		}
		#endregion

		#region SubItemCD
		public abstract class subItemCD : PX.Data.BQL.BqlString.Field<subItemCD> { }
		protected String _SubItemCD;
		[PXString(IsUnicode = true, IsKey = true)]
		[PXDBCalced(typeof(IsNull<Data.RTrim<INSubItem.subItemCD>, Empty>), typeof(string))]
		public virtual String SubItemCD
		{
			get
			{
				return this._SubItemCD;
			}
			set
			{
				this._SubItemCD = value;
			}
		}
		#endregion

		#region BaseUnit
		public abstract class baseUnit : PX.Data.BQL.BqlString.Field<baseUnit> { }

		protected string _BaseUnit;
		[INUnit(DisplayName = "Base Unit", Visibility = PXUIVisibility.Visible, BqlField = typeof(InventoryItem.baseUnit))]
		public virtual String BaseUnit
		{
			get
			{
				return this._BaseUnit;
			}
			set
			{
				this._BaseUnit = value;
			}
		}
		#endregion

		#region CuryID
		public abstract class curyID : PX.Data.BQL.BqlString.Field<curyID> { }
		protected String _CuryID;
		[PXString(5, IsUnicode = true, InputMask = ">LLLLL")]
		[PXUIField(DisplayName = "Currency", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual String CuryID
		{
			get
			{
				return this._CuryID;
			}
			set
			{
				this._CuryID = value;
			}
		}
		#endregion

		#region CuryInfoID
		public abstract class curyInfoID : PX.Data.BQL.BqlLong.Field<curyInfoID> { }
		protected Int64? _CuryInfoID;
		[PXLong()]
		[CurrencyInfo()]
		public virtual Int64? CuryInfoID
		{
			get
			{
				return this._CuryInfoID;
			}
			set
			{
				this._CuryInfoID = value;
			}
		}
		#endregion

		#region SalesUnit
		public abstract class salesUnit : PX.Data.BQL.BqlString.Field<salesUnit> { }
		protected string _SalesUnit;
		[INUnit(typeof(SOSiteStatusSelected.inventoryID), DisplayName = "Sales Unit", BqlField = typeof(InventoryItem.salesUnit))]
		public virtual String SalesUnit
		{
			get
			{
				return this._SalesUnit;
			}
			set
			{
				this._SalesUnit = value;
			}
		}
		#endregion

		#region QtySelected
		public abstract class qtySelected : PX.Data.BQL.BqlDecimal.Field<qtySelected> { }
		protected Decimal? _QtySelected;
		[PXQuantity]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Qty. Selected")]
		public virtual Decimal? QtySelected
		{
			get
			{
				return this._QtySelected ?? 0m;
			}
			set
			{
				if (value != null && value != 0m)
					this._Selected = true;
				this._QtySelected = value;
			}
		}
		#endregion

		#region QtyOnHand
		public abstract class qtyOnHand : PX.Data.BQL.BqlDecimal.Field<qtyOnHand> { }
		protected Decimal? _QtyOnHand;
		[PXDBQuantity(BqlField = typeof(INSiteStatusByCostCenter.qtyOnHand))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Qty. On Hand")]
		public virtual Decimal? QtyOnHand
		{
			get
			{
				return this._QtyOnHand;
			}
			set
			{
				this._QtyOnHand = value;
			}
		}
		#endregion

		#region QtyAvail
		public abstract class qtyAvail : PX.Data.BQL.BqlDecimal.Field<qtyAvail> { }
		protected Decimal? _QtyAvail;
		[PXDBQuantity(BqlField = typeof(INSiteStatusByCostCenter.qtyAvail))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Qty. Available")]
		public virtual Decimal? QtyAvail
		{
			get
			{
				return this._QtyAvail;
			}
			set
			{
				this._QtyAvail = value;
			}
		}
		#endregion

		#region QtyLast
		public abstract class qtyLast : PX.Data.BQL.BqlDecimal.Field<qtyLast> { }
		protected Decimal? _QtyLast;
		[PXDBQuantity(BqlField = typeof(INItemCustSalesStats.lastQty))]
		public virtual Decimal? QtyLast
		{
			get
			{
				return this._QtyLast;
			}
			set
			{
				this._QtyLast = value;
			}
		}
		#endregion

		#region BaseUnitPrice
		public abstract class baseUnitPrice : PX.Data.BQL.BqlDecimal.Field<baseUnitPrice> { }
		protected Decimal? _BaseUnitPrice;
		[PXDBPriceCost(true, BqlField = typeof(INItemCustSalesStats.lastUnitPrice))]
		public virtual Decimal? BaseUnitPrice
		{
			get
			{
				return this._BaseUnitPrice;
			}
			set
			{
				this._BaseUnitPrice = value;
			}
		}
		#endregion

		#region CuryUnitPrice
		public abstract class curyUnitPrice : PX.Data.BQL.BqlDecimal.Field<curyUnitPrice> { }
		protected Decimal? _CuryUnitPrice;
		[PXUnitPriceCuryConv(typeof(SOSiteStatusSelected.curyInfoID), typeof(SOSiteStatusSelected.baseUnitPrice))]
		[PXUIField(DisplayName = "Last Unit Price", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual Decimal? CuryUnitPrice
		{
			get
			{
				return this._CuryUnitPrice;
			}
			set
			{
				this._CuryUnitPrice = value;
			}
		}
		#endregion

		#region QtyAvailSale
		public abstract class qtyAvailSale : PX.Data.BQL.BqlDecimal.Field<qtyAvailSale> { }
		protected Decimal? _QtyAvailSale;
		[PXDBCalced(typeof(Switch<Case<Where<INUnit.unitMultDiv, Equal<MultDiv.divide>>,
			Mult<INSiteStatusByCostCenter.qtyAvail, INUnit.unitRate>>,
			Div<INSiteStatusByCostCenter.qtyAvail, INUnit.unitRate>>), typeof(decimal))]
		[PXQuantity()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Qty. Available")]
		public virtual Decimal? QtyAvailSale
		{
			get
			{
				return this._QtyAvailSale;
			}
			set
			{
				this._QtyAvailSale = value;
			}
		}
		#endregion

		#region QtyOnHandSale
		public abstract class qtyOnHandSale : PX.Data.BQL.BqlDecimal.Field<qtyOnHandSale> { }
		protected Decimal? _QtyOnHandSale;
		[PXDBCalced(typeof(Switch<Case<Where<INUnit.unitMultDiv, Equal<MultDiv.divide>>,
			Mult<INSiteStatusByCostCenter.qtyOnHand, INUnit.unitRate>>,
			Div<INSiteStatusByCostCenter.qtyOnHand, INUnit.unitRate>>), typeof(decimal))]
		[PXQuantity()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Qty. On Hand")]
		public virtual Decimal? QtyOnHandSale
		{
			get
			{
				return this._QtyOnHandSale;
			}
			set
			{
				this._QtyOnHandSale = value;
			}
		}
		#endregion

		#region QtyLastSale
		public abstract class qtyLastSale : PX.Data.BQL.BqlDecimal.Field<qtyLastSale> { }
		protected Decimal? _QtyLastSale;
		[PXDBCalced(typeof(Switch<Case<Where<INUnit.unitMultDiv, Equal<MultDiv.divide>>,
			Mult<INItemCustSalesStats.lastQty, INUnit.unitRate>>,
			Div<INItemCustSalesStats.lastQty, INUnit.unitRate>>), typeof(decimal))]
		[PXQuantity()]
		[PXUIField(DisplayName = "Qty. Last Sales")]
		public virtual Decimal? QtyLastSale
		{
			get
			{
				return this._QtyLastSale;
			}
			set
			{
				this._QtyLastSale = value;
			}
		}
		#endregion

		#region LastSalesDate
		public abstract class lastSalesDate : PX.Data.BQL.BqlDateTime.Field<lastSalesDate> { }
		protected DateTime? _LastSalesDate;
		[PXDBDate(BqlField = typeof(INItemCustSalesStats.lastDate))]
		[PXUIField(DisplayName = "Last Sales Date")]
		public virtual DateTime? LastSalesDate
		{
			get
			{
				return this._LastSalesDate;
			}
			set
			{
				this._LastSalesDate = value;
			}
		}
		#endregion

		#region DropShipLastQty
		public abstract class dropShipLastBaseQty : PX.Data.BQL.BqlDecimal.Field<dropShipLastBaseQty> { }
		[PXDBQuantity(BqlField = typeof(INItemCustSalesStats.dropShipLastQty))]
		public virtual Decimal? DropShipLastBaseQty
		{
			get;
			set;
		}
		#endregion

		#region DropShipLastQty
		public abstract class dropShipLastQty : PX.Data.BQL.BqlDecimal.Field<dropShipLastQty> { }
		[PXDBCalced(typeof(Switch<Case<Where<INUnit.unitMultDiv, Equal<MultDiv.divide>>,
			Mult<INItemCustSalesStats.dropShipLastQty, INUnit.unitRate>>,
			Div<INItemCustSalesStats.dropShipLastQty, INUnit.unitRate>>), typeof(decimal))]
		[PXQuantity()]
		[PXUIField(DisplayName = "Qty. of Last Drop Ship")]
		public virtual Decimal? DropShipLastQty
		{
			get;
			set;
		}
		#endregion

		#region DropShipLastUnitPrice
		public abstract class dropShipLastUnitPrice : PX.Data.BQL.BqlDecimal.Field<dropShipLastUnitPrice> { }
		[PXDBPriceCost(true, BqlField = typeof(INItemCustSalesStats.dropShipLastUnitPrice))]
		public virtual Decimal? DropShipLastUnitPrice
		{
			get;
			set;
		}
		#endregion

		#region DropShipCuryUnitPrice
		public abstract class dropShipCuryUnitPrice : PX.Data.BQL.BqlDecimal.Field<dropShipCuryUnitPrice> { }
		[PXUnitPriceCuryConv(typeof(SOSiteStatusSelected.curyInfoID), typeof(SOSiteStatusSelected.dropShipLastUnitPrice))]
		[PXUIField(DisplayName = "Unit Price of Last Drop Ship", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual Decimal? DropShipCuryUnitPrice
		{
			get;
			set;
		}
		#endregion

		#region DropShipLastDate
		public abstract class dropShipLastDate : PX.Data.BQL.BqlDateTime.Field<dropShipLastDate> { }
		[PXDBDate(BqlField = typeof(INItemCustSalesStats.dropShipLastDate))]
		[PXUIField(DisplayName = "Date of Last Drop Ship")]
		public virtual DateTime? DropShipLastDate
		{
			get;
			set;
		}
		#endregion

		#region NoteID
		public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }
		protected Guid? _NoteID;
		[PXNote(BqlField = typeof(InventoryItem.noteID))]
		public virtual Guid? NoteID
		{
			get
			{
				return this._NoteID;
			}
			set
			{
				this._NoteID = value;
			}
		}
		#endregion
	}

	[System.SerializableAttribute()]
	public class InvoiceSplits : IBqlTable
	{
		#region Selected
		public abstract class selected : PX.Data.BQL.BqlBool.Field<selected> { }
		protected bool? _Selected = false;
		[PXBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Selected")]
		public virtual bool? Selected
		{
			get
			{
				return _Selected;
			}
			set
			{
				_Selected = value;
			}
		}
		#endregion

		#region LineNbr
		public abstract class lineNbr : PX.Data.BQL.BqlInt.Field<lineNbr> { }
		protected Int32? _LineNbr;
		[PXInt(IsKey = true)]
		public virtual Int32? LineNbr
		{
			get
			{
				return this._LineNbr;
			}
			set
			{
				this._LineNbr = value;
			}
		}
		#endregion

		#region ARTran
		#region TranType
		public abstract class tranTypeARTran : PX.Data.BQL.BqlString.Field<tranTypeARTran> { }
		protected String _TranTypeARTran;
		[PXString(3, IsFixed = true, IsKey = true)]
		[PXUIField(DisplayName = "Tran. Type", Visibility = PXUIVisibility.Visible, Visible = false)]
		public virtual String TranTypeARTran
		{
			get
			{
				return this._TranTypeARTran;
			}
			set
			{
				this._TranTypeARTran = value;
			}
		}
		#endregion
		#region RefNbr
		public abstract class refNbrARTran : PX.Data.BQL.BqlString.Field<refNbrARTran> { }
		protected String _RefNbrARTran;
		[PXString(15, IsUnicode = true, IsKey = true)]
		[PXUIField(DisplayName = "Reference Nbr.", Visibility = PXUIVisibility.Visible, Visible = false)]
		public virtual String RefNbrARTran
		{
			get
			{
				return this._RefNbrARTran;
			}
			set
			{
				this._RefNbrARTran = value;
			}
		}
		#endregion
		#region LineNbr
		public abstract class lineNbrARTran : PX.Data.BQL.BqlInt.Field<lineNbrARTran> { }
		protected Int32? _LineNbrARTran;
		[PXInt(IsKey = true)]
		[PXUIField(DisplayName = "Line Nbr.", Visibility = PXUIVisibility.Visible, Visible = false)]
		public virtual Int32? LineNbrARTran
		{
			get
			{
				return this._LineNbrARTran;
			}
			set
			{
				this._LineNbrARTran = value;
			}
		}
		#endregion
		#region DropShip
		public abstract class dropShip : PX.Data.BQL.BqlBool.Field<dropShip> { }

		[PXBool]
		[PXUIField(DisplayName = Messages.DropShip)]
		public virtual bool? DropShip
		{
			get;
			set;
		}
		#endregion

		#endregion

		#region SOLine
		#region OrderType
		public abstract class orderTypeSOLine : PX.Data.BQL.BqlString.Field<orderTypeSOLine> { }
		protected String _OrderTypeSOLine;
		[PXString(2, IsFixed = true)]
		[PXUIField(DisplayName = "Order Type", Visible = false)]
		[PXSelector(typeof(Search<SOOrderType.orderType>), CacheGlobal = true)]
		public virtual String OrderTypeSOLine
		{
			get
			{
				return this._OrderTypeSOLine;
			}
			set
			{
				this._OrderTypeSOLine = value;
			}
		}
		#endregion
		#region OrderNbr
		public abstract class orderNbrSOLine : PX.Data.BQL.BqlString.Field<orderNbrSOLine> { }
		protected String _OrderNbrSOLine;
		[PXString(15, IsUnicode = true, InputMask = "")]
		[PXUIField(DisplayName = "Order Nbr.", Visible = false)]
		public virtual String OrderNbrSOLine
		{
			get
			{
				return this._OrderNbrSOLine;
			}
			set
			{
				this._OrderNbrSOLine = value;
			}
		}
		#endregion
		#region LineNbr
		public abstract class lineNbrSOLine : PX.Data.BQL.BqlInt.Field<lineNbrSOLine> { }
		protected Int32? _LineNbrSOLine;
		[PXInt()]
		[PXUIField(DisplayName = "Line Nbr.", Visible = false)]
		public virtual Int32? LineNbrSOLine
		{
			get
			{
				return this._LineNbrSOLine;
			}
			set
			{
				this._LineNbrSOLine = value;
			}
		}
		#endregion

		#region LineTypeSOLine
		public abstract class lineTypeSOLine : PX.Data.BQL.BqlString.Field<lineTypeSOLine> { }
		protected String _LineTypeSOLine;
		[PXString(2, IsFixed = true)]
		public virtual String LineTypeSOLine
		{
			get
			{
				return this._LineTypeSOLine;
			}
			set
			{
				this._LineTypeSOLine = value;
			}
		}
		#endregion


		#endregion

		#region INTran
		#region DocType
		public abstract class docTypeINTran : PX.Data.BQL.BqlString.Field<docTypeINTran> { }
		protected String _DocTypeINTran;
		[PXString(1, IsFixed = true)]
		public virtual String DocTypeINTran
		{
			get
			{
				return this._DocTypeINTran;
			}
			set
			{
				this._DocTypeINTran = value;
			}
		}
		#endregion
		#region RefNbr
		public abstract class refNbrINTran : PX.Data.BQL.BqlString.Field<refNbrINTran> { }
		protected String _RefNbrINTran;
		[PXString(15, IsUnicode = true)]
		public virtual String RefNbrINTran
		{
			get
			{
				return this._RefNbrINTran;
			}
			set
			{
				this._RefNbrINTran = value;
			}
		}
		#endregion
		#region LineNbr
		public abstract class lineNbrINTran : PX.Data.BQL.BqlInt.Field<lineNbrINTran> { }
		protected Int32? _LineNbrINTran;
		[PXInt()]
		public virtual Int32? LineNbrINTran
		{
			get
			{
				return this._LineNbrINTran;
			}
			set
			{
				this._LineNbrINTran = value;
			}
		}
		#endregion
		#region Inventory
		public abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }
		protected Int32? _InventoryID;
		[Inventory(DisplayName = "Inventory ID")]
		public virtual Int32? InventoryID
		{
			get
			{
				return _InventoryID;
			}
			set
			{
				this._InventoryID = value;
			}
		}
		#endregion

		#region SubItemID
		public abstract class subItemID : PX.Data.BQL.BqlInt.Field<subItemID> { }
		protected Int32? _SubItemID;
		[IN.SubItem(typeof(inventoryID))]
		public virtual Int32? SubItemID
		{
			get
			{
				return this._SubItemID;
			}
			set
			{
				this._SubItemID = value;
			}
		}
		#endregion
		#region SiteID
		public abstract class siteID : PX.Data.BQL.BqlInt.Field<siteID> { }
		protected Int32? _SiteID;
		[IN.Site()]
		public virtual Int32? SiteID
		{
			get
			{
				return this._SiteID;
			}
			set
			{
				this._SiteID = value;
			}
		}
		#endregion

		#region LocationID
		public abstract class locationID : PX.Data.BQL.BqlInt.Field<locationID> { }
		protected Int32? _LocationID;
		[IN.Location(typeof(siteID))]
		[PXDefault()]
		public virtual Int32? LocationID
		{
			get
			{
				return this._LocationID;
			}
			set
			{
				this._LocationID = value;
			}
		}
		#endregion

		#region LotSerialNbr
		public abstract class lotSerialNbr : PX.Data.BQL.BqlString.Field<lotSerialNbr> { }
		protected String _LotSerialNbr;
		[LotSerialNbr]
		public virtual String LotSerialNbr
		{
			get
			{
				return this._LotSerialNbr;
			}
			set
			{
				this._LotSerialNbr = value;
			}
		}
		#endregion

		#region UOM
		public abstract class uOM : PX.Data.BQL.BqlString.Field<uOM> { }
		protected String _UOM;
		[INUnit(typeof(inventoryID), DisplayName = "UOM", Enabled = false)]
		public virtual String UOM
		{
			get
			{
				return this._UOM;
			}
			set
			{
				this._UOM = value;
			}
		}
		#endregion

		#region Qty
		public abstract class qty : PX.Data.BQL.BqlDecimal.Field<qty> { }
		protected Decimal? _Qty;
		[PXQuantity()]
		[PXUIField(DisplayName = "Quantity")]
		public virtual Decimal? Qty
		{
			get
			{
				return this._Qty;
			}
			set
			{
				this._Qty = value;
			}
		}
		#endregion

		#region BaseQty
		public abstract class baseQty : PX.Data.BQL.BqlDecimal.Field<baseQty> { }
		protected Decimal? _BaseQty;
		[PXQuantity()]
		public virtual Decimal? BaseQty
		{
			get
			{
				return this._BaseQty;
			}
			set
			{
				this._BaseQty = value;
			}
		}
		#endregion

		#region TranDesc
		public abstract class tranDesc : PX.Data.BQL.BqlString.Field<tranDesc> { }
		protected String _TranDesc;
		[PXString(256, IsUnicode = true)]
		[PXUIField(DisplayName = "Line Description")]
		public virtual String TranDesc
		{
			get
			{
				return this._TranDesc;
			}
			set
			{
				this._TranDesc = value;
			}
		}
		#endregion

		#endregion

		#region INTranSplit
		#region DocType
		public abstract class docTypeINTranSplit : PX.Data.BQL.BqlString.Field<docTypeINTranSplit> { }
		protected String _DocTypeINTranSplit;
		[PXString(1, IsFixed = true)]
		public virtual String DocTypeINTranSplit
		{
			get
			{
				return this._DocTypeINTranSplit;
			}
			set
			{
				this._DocTypeINTranSplit = value;
			}
		}
		#endregion
		#region RefNbr
		public abstract class refNbrINTranSplit : PX.Data.BQL.BqlString.Field<refNbrINTranSplit> { }
		protected String _RefNbrINTranSplit;
		[PXString(15, IsUnicode = true)]
		public virtual String RefNbrINTranSplit
		{
			get
			{
				return this._RefNbrINTranSplit;
			}
			set
			{
				this._RefNbrINTranSplit = value;
			}
		}
		#endregion
		#region LineNbr
		public abstract class lineNbrINTranSplit : PX.Data.BQL.BqlInt.Field<lineNbrINTranSplit> { }
		protected Int32? _LineNbrINTranSplit;
		[PXInt()]
		public virtual Int32? LineNbrINTranSplit
		{
			get
			{
				return this._LineNbrINTranSplit;
			}
			set
			{
				this._LineNbrINTranSplit = value;
			}
		}
		#endregion

		#region LineNbr
		public abstract class splitLineNbrINTranSplit : PX.Data.BQL.BqlInt.Field<splitLineNbrINTranSplit> { }
		protected Int32? _SplitLineNbrINTranSplit;
		[PXInt()]
		public virtual Int32? SplitLineNbrINTranSplit
		{
			get
			{
				return this._SplitLineNbrINTranSplit;
			}
			set
			{
				this._SplitLineNbrINTranSplit = value;
			}
		}
		#endregion
		#endregion

		#region SOSalesPerTran
		#region OrderType
		public abstract class orderTypeSOSalesPerTran : PX.Data.BQL.BqlString.Field<orderTypeSOSalesPerTran> { }
		protected String _OrderTypeSOSalesPerTran;
		[PXString(2, IsFixed = true)]
		public virtual String OrderTypeSOSalesPerTran
		{
			get
			{
				return this._OrderTypeSOSalesPerTran;
			}
			set
			{
				this._OrderTypeSOSalesPerTran = value;
			}
		}
		#endregion
		#region OrderNbr
		public abstract class orderNbrSOSalesPerTran : PX.Data.BQL.BqlString.Field<orderNbrSOSalesPerTran> { }
		protected String _OrderNbrSOSalesPerTran;
		[PXString(15, IsUnicode = true)]
		public virtual String OrderNbrSOSalesPerTran
		{
			get
			{
				return this._OrderNbrSOSalesPerTran;
			}
			set
			{
				this._OrderNbrSOSalesPerTran = value;
			}
		}
		#endregion
		#region SalespersonID
		public abstract class salespersonIDSOSalesPerTran : PX.Data.BQL.BqlInt.Field<salespersonIDSOSalesPerTran> { }
		protected Int32? _SalespersonIDSOSalesPerTran;
		[PXInt()]
		public virtual Int32? SalespersonIDSOSalesPerTran
		{
			get
			{
				return this._SalespersonIDSOSalesPerTran;
			}
			set
			{
				this._SalespersonIDSOSalesPerTran = value;
			}
		}
		#endregion

		#endregion
	}
}
