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
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Data.WorkflowAPI;
using PX.Common;
using PX.Objects.AR;
using PX.Objects.CM;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Objects.EP;
using PX.Objects.GL;
using PX.Objects.IN;
using PX.SM;
using POLineType = PX.Objects.PO.POLineType;
using POReceiptLine = PX.Objects.PO.POReceiptLine;
using PX.CarrierService;
using PX.Data.DependencyInjection;
using PX.LicensePolicy;
using PX.Objects.SO.Services;
using PX.Objects.PO;
using PX.Objects.AR.MigrationMode;
using PX.Objects.Common;
using PX.Objects.Common.Discount;
using PX.Objects.Common.Extensions;
using PX.Common.Collection;
using PX.Objects.SO.GraphExtensions.CarrierRates;
using PX.Objects.SO.GraphExtensions.SOShipmentEntryExt;
using PX.Api;
using ShipmentActions = PX.Objects.SO.SOShipmentEntryActionsAttribute;
using PdfSharp.Pdf.IO;
using PX.Objects.IN.Attributes;
using PX.Concurrency;
using PX.Objects.SO.GraphExtensions.SOOrderEntryExt;
using PX.Objects.GL.FinPeriods.TableDefinition;
using PX.Objects.GL.FinPeriods;
using PX.Objects.IN.InventoryRelease;
using PX.Objects.IN.InventoryRelease.Accumulators.QtyAllocated;

namespace PX.Objects.SO
{
	public partial class SOShipmentEntry : PXGraph<SOShipmentEntry, SOShipment>, IGraphWithInitialization
	{
		private DiscountEngine<SOShipLine, SOShipmentDiscountDetail> _discountEngine => DiscountEngineProvider.GetEngineFor<SOShipLine, SOShipmentDiscountDetail>();
		public SOShipmentLineSplittingExtension LineSplittingExt => FindImplementation<SOShipmentLineSplittingExtension>();
		public SOShipmentItemAvailabilityExtension ItemAvailabilityExt => FindImplementation<SOShipmentItemAvailabilityExtension>();

		public ToggleCurrency<SOShipment> CurrencyView;
		[PXViewName(Messages.SOShipment)]
		public PXSelectJoin<SOShipment,
			LeftJoinSingleTable<Customer, On<Customer.bAccountID, Equal<SOShipment.customerID>>,
			LeftJoin<INSite, 
				On<SOShipment.FK.Site>>>,
			Where2<Where<Customer.bAccountID, IsNull,
			Or<Match<Customer, Current<AccessInfo.userName>>>>,
			And<Where<SOShipment.siteID, IsNull,
				Or<INSite.branchID, IsNotNull, And<Match<INSite, Current<AccessInfo.userName>>>>>>>> Document;
		public PXSelect<SOShipment, Where<SOShipment.shipmentNbr, Equal<Current<SOShipment.shipmentNbr>>>> CurrentDocument;
		public PXSelect<SOShipLine, Where<SOShipLine.shipmentNbr, Equal<Current<SOShipment.shipmentNbr>>>, OrderBy<Asc<SOShipLine.shipmentNbr, Asc<SOShipLine.sortOrder>>>> Transactions;
		public PXSelect<SOShipLineSplit, Where<SOShipLineSplit.shipmentNbr, Equal<Current<SOShipLine.shipmentNbr>>, And<SOShipLineSplit.lineNbr, Equal<Current<SOShipLine.lineNbr>>>>> splits;
		public PXSelect<Unassigned.SOShipLineSplit, Where<Unassigned.SOShipLineSplit.shipmentNbr, Equal<Current<SOShipLine.shipmentNbr>>, And<Unassigned.SOShipLineSplit.lineNbr, Equal<Current<SOShipLine.lineNbr>>>>> unassignedSplits;
		[PXViewName(Messages.ShippingAddress)]
		public PXSelect<SOShipmentAddress, Where<SOShipmentAddress.addressID, Equal<Current<SOShipment.shipAddressID>>>> Shipping_Address;
		[PXViewName(Messages.ShippingContact)]
		public PXSelect<SOShipmentContact, Where<SOShipmentContact.contactID, Equal<Current<SOShipment.shipContactID>>>> Shipping_Contact;
		[PXViewName(Messages.SOOrderShipment)]
		public
			PXSelectJoin<SOOrderShipment,
			LeftJoin<SOOrder, On<
				SOOrder.orderType, Equal<SOOrderShipment.orderType>,
				And<SOOrder.orderNbr, Equal<SOOrderShipment.orderNbr>>>,
			LeftJoin<CurrencyInfo, On<CurrencyInfo.curyInfoID, Equal<SOOrder.curyInfoID>>,
			LeftJoin<SOAddress, On<SOAddress.addressID, Equal<SOOrder.billAddressID>>,
			LeftJoin<SOContact, On<SOContact.contactID, Equal<SOOrder.billContactID>>>>>>,
			Where<
				SOOrderShipment.shipmentNbr, Equal<Current<SOShipment.shipmentNbr>>,
				And<SOOrderShipment.shipmentType, Equal<Current<SOShipment.shipmentType>>>>>
			OrderList;

		public PXSelect<SOOrder,
			Where<SOOrder.orderType, Equal<Required<SOOrder.orderType>>,
				And<SOOrder.orderNbr, Equal<Required<SOOrder.orderNbr>>>>>
			soorder;
		public PXSetup<SOOrderType, Where<SOOrderType.orderType, Equal<Optional<SOOrder.orderType>>>> soordertype;
		public PXSelect<SOSetupApproval> sosetupapproval;
		public EPApprovalAutomation<SOOrder, SOOrder.approved, SOOrder.rejected, SOOrder.hold, SOSetupApproval> Approval;

		public PXSelect<SOOrderShipment, Where<SOOrderShipment.shipmentNbr, Equal<Current<SOShipment.shipmentNbr>>, And<SOOrderShipment.shipmentType, Equal<Current<SOShipment.shipmentType>>>>> OrderListSimple;
		public PXSelect<SOShipmentDiscountDetail, Where<SOShipmentDiscountDetail.shipmentNbr, Equal<Current<SOShipment.shipmentNbr>>>> DiscountDetails;
		public PXSelect<SOShipLine, Where<SOShipLine.shipmentNbr, Equal<Current<SOShipment.shipmentNbr>>, And<SOShipLine.isFree, Equal<boolTrue>>>, OrderBy<Asc<SOShipLine.shipmentNbr, Asc<SOShipLine.lineNbr>>>> FreeItems;
		[PXViewName(Messages.SOPackageDetail)]
		public PXSelect<SOPackageDetailEx, Where<SOPackageDetailEx.shipmentNbr, Equal<Current<SOShipment.shipmentNbr>>>> Packages;
		[PXHidden]
		public PXSelect<SOPackageDetailEx, Where<SOPackageDetailEx.shipmentNbr, Equal<Current<SOShipment.shipmentNbr>>>> PackagesForRates;
		[PXHidden]
		[PXCopyPasteHiddenView]
		public PXSelect<CarrierLabelHistory> LabelHistory;
		public PXSetup<Carrier, Where<Carrier.carrierID, Equal<Current<SOShipment.shipVia>>>> carrier;
		public PXSelect<CurrencyInfo, Where<CurrencyInfo.curyInfoID, Equal<Current<SOShipment.curyInfoID>>>> currencyinfo;
		public PXSelect<CurrencyInfo> DummyCuryInfo;

		public PXSetup<INSetup> insetup;
		public PXSetup<SOSetup> sosetup;
		public PXSetup<ARSetup> arsetup;
		public PXSetupOptional<CommonSetup> commonsetup;

		public PXSetup<GL.Branch, InnerJoin<INSite, On<INSite.branchID, Equal<GL.Branch.branchID>>>, Where<INSite.siteID, Equal<Optional<SOShipment.destinationSiteID>>, And<Current<SOShipment.shipmentType>, Equal<SOShipmentType.transfer>, Or<INSite.siteID, Equal<Optional<SOShipment.siteID>>, And<Current<SOShipment.shipmentType>, NotEqual<SOShipmentType.transfer>>>>>> Company; //TODO: Need review INRegister Branch and SOShipment SiteID/DestinationSiteID AC-55773
		public PXSetup<Customer, Where<Customer.bAccountID, Equal<Optional<SOShipment.customerID>>>> customer;
		public PXSetup<Location, Where<Location.bAccountID, Equal<Current<SOShipment.customerID>>, And<Location.locationID, Equal<Optional<SOShipment.customerLocationID>>>>> location;

		public PXSelect<SOLine2> soline;
		public PXSelect<SOLineSplit2> solinesplit;
		public PXSelect<SOLine> dummy_soline; //will prevent collection was modified if no Select<SOLine> was executed prior to Persist()

		public PXFilter<AddSOFilter> addsofilter;
        public PXSelectJoinOrderBy<SOShipmentPlan,
               InnerJoin<SOLineSplit,
               On<SOLineSplit.planID, Equal<SOShipmentPlan.planID>>,
               InnerJoin<SOLine,
               On<SOLine.orderType, Equal<SOLineSplit.orderType>, And<SOLine.orderNbr, Equal<SOLineSplit.orderNbr>, And<SOLine.lineNbr, Equal<SOLineSplit.lineNbr>>>>>>,
			   OrderBy<Asc<SOLine.sortOrder, Asc<SOLine.lineNbr, Asc<SOLineSplit.lineNbr>>>>> soshipmentplan;

		[PXViewName(CR.Messages.MainContact)]
		public PXSelect<Contact> DefaultCompanyContact;
		protected virtual IEnumerable defaultCompanyContact()
		{
			return OrganizationMaint.GetDefaultContactForCurrentOrganization(this);
		}

		[PXCopyPasteHiddenView()]
		public SelectFrom<SOOrderSite>
			.InnerJoin<SOOrderShipment>.On<SOOrderShipment.FK.OrderSite>
			.Where<SOOrderShipment.FK.Shipment.SameAsCurrent>.View OrderSite;

		public PXAction<SOShipment> putOnHold;
		[PXButton(CommitChanges = true), PXUIField(DisplayName = "Hold")]
		protected virtual IEnumerable PutOnHold(PXAdapter adapter) => adapter.Get();

		public PXAction<SOShipment> releaseFromHold;
		[PXButton(CommitChanges = true), PXUIField(DisplayName = "Remove Hold")]
		protected virtual IEnumerable ReleaseFromHold(PXAdapter adapter) => adapter.Get();

		public PXInitializeState<SOShipment> initializeState;

		public PXAction<SOShipment> notification;
		[PXUIField(DisplayName = "Notifications", Visible = false)]
		[PXButton(ImageKey = PX.Web.UI.Sprite.Main.DataEntryF)]
		protected virtual IEnumerable Notification(PXAdapter adapter,
		[PXString]
		string notificationCD
		)
		{
			foreach (SOShipment shipment in adapter.Get<SOShipment>())
			{
				Document.Current = shipment;

				var parameters = new Dictionary<string, string>();
				parameters["SOShipment.ShipmentNbr"] = shipment.ShipmentNbr;

				GL.Branch branch = PXSelectReadonly2<GL.Branch, InnerJoin<INSite, On<INSite.branchID, Equal<GL.Branch.branchID>>>,
						Where<INSite.siteID, Equal<Optional<SOShipment.destinationSiteID>>,
								And<Current<SOShipment.shipmentType>, Equal<SOShipmentType.transfer>, 
							Or<INSite.siteID, Equal<Optional<SOShipment.siteID>>,
								And<Current<SOShipment.shipmentType>, NotEqual<SOShipmentType.transfer>>>>>>
					.SelectSingleBound(this, new object[] {shipment});
				
				this.GetExtension<SOShipmentEntry_ActivityDetailsExt>().SendNotification(ARNotificationSource.Customer, notificationCD, (branch != null && branch.BranchID != null) ? branch.BranchID : Accessinfo.BranchID, parameters, adapter.MassProcess);

				yield return shipment;
			}
		}

		public PXAction<SOShipment> emailShipment;
		[PXButton(CommitChanges = true), PXUIField(DisplayName = "Email Shipment", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		public virtual IEnumerable EmailShipment(
			PXAdapter adapter,
			[PXString]
			string notificationCD = null) => Notification(adapter, notificationCD ?? "SHIPMENT");

		#region Action menu items

		public PXAction<SOShipment> confirmShipmentAction;
		[PXButton(CommitChanges = true), PXUIField(DisplayName = ShipmentActions.Messages.ConfirmShipment, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]		
		protected virtual IEnumerable ConfirmShipmentAction(PXAdapter adapter)
		{
			var list = adapter.Get<SOShipment>().ToList();
			bool massProcess = adapter.MassProcess;

			Save.Press();

			PXLongOperation.StartOperation(this, delegate ()
			{
				SOShipmentEntry docgraph = PXGraph.CreateInstance<SOShipmentEntry>();
				SOOrderEntry orderentry = PXGraph.CreateInstance<SOOrderEntry>();

				docgraph.MergeStatusCachesBetweenGraphs(docgraph, orderentry);

				PXCache cache = orderentry.Caches[typeof(SOShipLineSplit)];
				cache = orderentry.Caches[typeof(INTranSplit)];

				foreach (SOShipment shipment in list)
				{
					if (massProcess)
						PXProcessing<SOShipment>.SetCurrentItem(shipment);

					try
					{
						docgraph.PrepareShipmentForConfirmation(shipment);

						docgraph.ShipPackages(shipment);

						docgraph.IsShipmentReadyForConfirmation = true;
						docgraph.ConfirmShipment(orderentry, shipment);

						if (massProcess)
							PXProcessing<SOShipment>.SetProcessed();
					}
					catch (Exception ex) when (massProcess)
					{
						PXProcessing<SOShipment>.SetError(ex);
					}
					finally
					{
						docgraph.IsShipmentReadyForConfirmation = false;
					}
				}
			});

			return list;
		}

		public PXAction<SOShipment> createInvoice;
		[PXButton(CommitChanges = true), PXUIField(DisplayName = ShipmentActions.Messages.CreateInvoice, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		protected virtual IEnumerable CreateInvoice(PXAdapter adapter)
		{
			var shipments = adapter.Get<SOShipment>().ToList();
			var adapterSlice = (adapter.MassProcess, adapter.AllowRedirect, adapter.QuickProcessFlow);
			var redirectRequired = !this.IsImport;
			if (!adapter.Arguments.TryGetValue(nameof(SOShipmentFilter.InvoiceDate), out object invoiceDate) || invoiceDate == null)
				invoiceDate = Accessinfo.BusinessDate;

			Save.Press();

			PXLongOperation.StartOperation(this, delegate ()
			{
				var shipmentEntry = CreateInstance<SOShipmentEntry>();
				var invoiceEntry = CreateInstance<SOInvoiceEntry>();

				InvoiceList createdInvoices = new ShipmentInvoices(shipmentEntry);

				foreach (SOShipment shipment in shipments)
				{
					try
					{
						shipmentEntry.SelectTimeStamp();
						invoiceEntry.SelectTimeStamp();

						if (adapterSlice.MassProcess)
							PXProcessing<SOShipment>.SetCurrentItem(shipment);

						shipmentEntry.InvoiceShipment(invoiceEntry, shipment, (DateTime)invoiceDate, createdInvoices, adapterSlice.QuickProcessFlow);

						if (adapterSlice.MassProcess) // shipment is updated and saved somewhere in InvoiceShipment method
							shipmentEntry.Document.Cache.RestoreCopy(shipment, SOShipment.PK.Find(shipmentEntry, shipment));
					}
					catch (Exception ex) when (adapterSlice.MassProcess)
					{
						PXProcessing<SOShipment>.SetError(ex);
					}
				}

				if (adapterSlice.AllowRedirect && !adapterSlice.MassProcess && redirectRequired && createdInvoices.Count > 0)
				{
					using (new PXTimeStampScope(null))
					{
						ARInvoice firstInvoice = createdInvoices[0];
						invoiceEntry = PXGraph.CreateInstance<SOInvoiceEntry>();

						invoiceEntry.Document.Current = invoiceEntry.Document.Search<ARInvoice.docType, ARInvoice.refNbr>(firstInvoice.DocType, firstInvoice.RefNbr, firstInvoice.DocType);
						throw new PXRedirectRequiredException(invoiceEntry, "Invoice");
					}
				}
			});

			return shipments;
		}

		public PXAction<SOShipment> UpdateIN;
		[PXButton(CommitChanges = true), PXUIField(DisplayName = ShipmentActions.Messages.PostInvoiceToIN, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]		
		protected virtual IEnumerable updateIN(PXAdapter adapter, List<SOShipment> shipmentList = null)
		{
			List<SOShipment> shipments = shipmentList ?? adapter.Get<SOShipment>().ToList();
			var adapterSlice = (adapter.MassProcess, adapter.QuickProcessFlow);

			if (!UnattendedMode && NeedWarningShipNotInvoicedUpdateIN(this, sosetup.Current, shipments, true))
			{
				WebDialogResult result = Document.View.Ask(Document.Current, GL.Messages.Confirmation, Messages.ShipNotInvoicedUpdateIN, MessageButtons.YesNo, MessageIcon.Question);
				if (result != WebDialogResult.Yes)
					return shipments;
			}

			Save.Press();

			PXLongOperation.StartOperation(this, delegate ()
			{
				var shipmentEntry = CreateInstance<SOShipmentEntry>();
				var factory = new INRegisterEntryFactory(shipmentEntry);
				var createdINDocs = new DocumentList<INRegister>(shipmentEntry);

				foreach (SOShipment shipment in shipments)
				{
					try
					{
						if (adapterSlice.MassProcess)
							PXProcessing<SOShipment>.SetCurrentItem(shipment);

						shipmentEntry.PostShipment(factory, shipment, createdINDocs);

						if (adapterSlice.MassProcess) // shipment is updated and saved somewhere in PostShipment method
							shipmentEntry.Document.Cache.RestoreCopy(shipment, SOShipment.PK.Find(shipmentEntry, shipment));
					}
					catch (Exception ex) when (adapterSlice.MassProcess)
					{
						PXProcessing<SOShipment>.SetError(ex);
					}
				}

				if (shipmentEntry.sosetup.Current.AutoReleaseIN == true && createdINDocs.Count > 0 && createdINDocs[0].Hold == false)
					INDocumentRelease.ReleaseDoc(createdINDocs, false, processFlow: adapterSlice.QuickProcessFlow);

				if (createdINDocs.Count == 1 && adapterSlice.QuickProcessFlow != PXQuickProcess.ActionFlow.NoFlow)
					INDocumentRelease.RedirectTo(createdINDocs[0]);
			});

			return shipments;
		}

		public static bool NeedWarningShipNotInvoicedUpdateIN(PXGraph graph, SOSetup setup, IEnumerable<SOShipment> shipments, bool validateEachShipmentLine = false)
		{
			bool IsNotBillable(SOShipment shipment) =>
				shipment.Confirmed == true &&
				shipment.UnbilledOrderCntr == 0 &&
				shipment.BilledOrderCntr == 0 &&
				shipment.ReleasedOrderCntr == 0;

			if (setup.UseShipDateForInvoiceDate != true)
			{
				bool shippedNotInvoicedIsDisabled = false;
				if (validateEachShipmentLine)
				{
					foreach (SOShipment shipment in shipments)
					{
						SOShipLine shipLineWithOrderTypeWithDisbledShippedNotInvoiced = SelectFrom<SOShipLine>.
							InnerJoin<SOOrderType>.On<SOOrderType.orderType.IsEqual<SOShipLine.origOrderType>.
								And<SOOrderType.aRDocType.IsNotEqual<ARDocType.noUpdate>>.
								And<SOOrderType.requireShipping.IsEqual<True>>.
								And<SOOrderType.useShippedNotInvoiced.IsEqual<False>>>.
							Where<SOShipLine.shipmentNbr.IsEqual<@P.AsString>>.View.SelectSingleBound(graph, null, shipment.ShipmentNbr);

						if (shipLineWithOrderTypeWithDisbledShippedNotInvoiced != null)
						{
							shippedNotInvoicedIsDisabled = true;
							break;
						}
					}
				}
				else
				{
					SOOrderType orderTypeWithDisbledShippedNotInvoiced = SelectFrom<SOOrderType>.
						Where<SOOrderType.active.IsEqual<True>.
						And<SOOrderType.aRDocType.IsNotEqual<ARDocType.noUpdate>>.
						And<SOOrderType.requireShipping.IsEqual<True>>.
						And<SOOrderType.useShippedNotInvoiced.IsEqual<False>>>.View.SelectSingleBound(graph, null);

					if (orderTypeWithDisbledShippedNotInvoiced != null)
						shippedNotInvoicedIsDisabled = true;

				}

				if (shippedNotInvoicedIsDisabled)
				{
					return shipments.Any(shipment => shipment.Status == SOShipmentStatus.Confirmed && !IsNotBillable(shipment));
				}
			}

			return false;
		}

		public PXAction<SOShipment> applyAssignmentRules;
		[PXButton(CommitChanges = true), PXUIField(DisplayName = ShipmentActions.Messages.ApplyAssignmentRules, Visible = false)]
		protected virtual IEnumerable ApplyAssignmentRules(PXAdapter adapter)
		{
			if (sosetup.Current.DefaultShipmentAssignmentMapID == null)
				throw new PXSetPropertyException(Messages.AssignNotSetup, Messages.SOSetup);

			var list = adapter.Get<SOShipment>().ToList();

			var processor = CreateInstance<EPAssignmentProcessor<SOShipment>>();
			processor.Assign(Document.Current, sosetup.Current.DefaultShipmentAssignmentMapID);
			Document.Update(Document.Current);

			return list;
		}

		public PXAction<SOShipment> correctShipmentAction;
		[PXButton(CommitChanges = true), PXUIField(DisplayName = ShipmentActions.Messages.CorrectShipment, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		protected virtual IEnumerable CorrectShipmentAction(PXAdapter adapter)
		{
			var list = adapter.Get<SOShipment>().ToList();
			bool massProcess = adapter.MassProcess;

			Save.Press();

			PXLongOperation.StartOperation(this, delegate ()
			{
				var docgraph = CreateInstance<SOShipmentEntry>();
				var orderentry = CreateInstance<SOOrderEntry>();

				docgraph.MergeStatusCachesBetweenGraphs(docgraph, orderentry);

				PXCache cache = orderentry.Caches[typeof(SOShipLineSplit)];
				cache = orderentry.Caches[typeof(INTranSplit)];

				foreach (SOShipment shipment in list)
				{
					try
					{
						if (massProcess)
							PXProcessing<SOShipment>.SetCurrentItem(shipment);

						using (PXTransactionScope ts = new PXTransactionScope())
						{
							docgraph.SetSuppressWorkflowOnCorrectShipment();
							docgraph.CorrectShipment(orderentry, shipment);
							docgraph.CancelPackages(shipment);

							ts.Complete();
						}

					}
					catch (Exception ex) when (massProcess)
					{
						PXProcessing<SOShipment>.SetError(ex);
					}
				}
			});

			return list;
		}

		public PXAction<SOShipment> createDropshipInvoice;
		[PXButton(CommitChanges = true), PXUIField(DisplayName = ShipmentActions.Messages.CreateDropshipInvoice, Visible = false, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		protected virtual IEnumerable CreateDropshipInvoice(PXAdapter adapter)
		{
			var list = adapter.Get<SOShipment>().ToList();
			var adapterSlice = (adapter.MassProcess, adapter.Arguments);

			PXLongOperation.StartOperation(this, delegate ()
			{
				var shipmentEntry = CreateInstance<SOShipmentEntry>();
				InvoiceList createdInvoices = new ShipmentInvoices(shipmentEntry);

				InvoiceReceipt(adapterSlice.Arguments, list, createdInvoices, adapterSlice.MassProcess);

				if (adapterSlice.MassProcess) // shipment is updated and saved somewhere in InvoiceReceipt method
					list.ForEach(sh => shipmentEntry.Document.Cache.RestoreCopy(sh, SOShipment.PK.Find(shipmentEntry, shipmentEntry.Document.Current)));

				if (!adapterSlice.MassProcess && createdInvoices.Count > 0)
				{
					using (new PXTimeStampScope(null))
					{
						var invoiceEntry = CreateInstance<SOInvoiceEntry>();
						invoiceEntry.Document.Current = createdInvoices[0];
						throw new PXRedirectRequiredException(invoiceEntry, "Invoice");
					}
				}
			});

			return list;
		}

		public PXAction<SOShipment> printLabels;
		[PXButton(CommitChanges = true), PXUIField(DisplayName = ShipmentActions.Messages.PrintLabels, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		protected virtual IEnumerable PrintLabels(PXAdapter adapter)
		{
			var list = adapter.Get<SOShipment>().ToList();

			if (adapter.MassProcess)
			{
				Save.Press();

				var printArgs = new PrintPackageFilesArgs
				{
					Shipments = list,
					Adapter = adapter,
					Category = PackageFileCategory.CarrierLabel
				};

				var graph = PXGraph.CreateInstance<SOShipmentEntry>();
				LongOperationManager.StartAsyncOperation(ct => graph.PrintPackageFiles(printArgs, ct));
			}
			else
			{
				PrintCarrierLabels();
			}

			return list;
		}

		public PXAction<SOShipment> printCommercialInvoices;
		[PXButton(CommitChanges = true)]
		[PXUIField(DisplayName = ShipmentActions.Messages.PrintCommercialInvoices, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select, FieldClass = nameof(FeaturesSet.CarrierIntegration))]
		protected virtual IEnumerable PrintCommercialInvoices(PXAdapter adapter)
		{
			var list = adapter.Get<SOShipment>().ToList();

			if (adapter.MassProcess)
			{
				Save.Press();

				var printArgs = new PrintPackageFilesArgs
				{
					Shipments = list,
					Adapter = adapter,
					Category = PackageFileCategory.CommercialInvoice
				};

				var graph = PXGraph.CreateInstance<SOShipmentEntry>();
				LongOperationManager.StartAsyncOperation(ct => graph.PrintPackageFiles(printArgs, ct));
			}
			else
			{
				LongOperationManager.StartAsyncOperation(ct => PrintCommercInvoices(ct));
			}

			return list;
		}

		public PXAction<SOShipment> getReturnLabelsAction;
		[PXButton(CommitChanges = true), PXUIField(DisplayName = ShipmentActions.Messages.GetReturnLabels, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		protected virtual IEnumerable GetReturnLabelsAction(PXAdapter adapter)
		{
			var list = adapter.Get<SOShipment>().ToList();

			Save.Press();

			bool massProcess = adapter.MassProcess;

			PXLongOperation.StartOperation(this, delegate ()
			{
				SOShipmentEntry docgraph = PXGraph.CreateInstance<SOShipmentEntry>();

				foreach (SOShipment order in list)
				{
					try
					{
						if (massProcess)
							PXProcessing<SOShipment>.SetCurrentItem(order);
						docgraph.GetReturnLabels(order);
					}
					catch (Exception ex)
					{
						if (!massProcess)
							throw;

						PXProcessing<SOShipment>.SetError(ex);
					}
				}
			});

			return list;
		}


		public PXAction<SOShipment> printPickListAction;
		[PXButton(CommitChanges = true), PXUIField(DisplayName = ShipmentActions.Messages.PrintPickList, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		protected virtual IEnumerable PrintPickListAction(PXAdapter adapter)
		{
			var list = adapter.Get<SOShipment>().ToList();
			LongOperationManager.StartAsyncOperation(ct=>PrintPickList(list, adapter, ct));
			return list;
		}

		#endregion

		public PXAction<SOShipment> inquiry;
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

		//throw new PXReportRequiredException(parameters, "SO642000", "Shipment Confirmation");
		public PXAction<SOShipment> report;
		[PXUIField(DisplayName = "Reports", MapEnableRights = PXCacheRights.Select)]
		[PXButton(SpecialType = PXSpecialButtonType.ReportsFolder, MenuAutoOpen = true)]
		public virtual IEnumerable Report(PXAdapter adapter, [PXString(8, InputMask = "CC.CC.CC.CC")] string reportID)
		{
			var shipments = adapter.Get<SOShipment>().ToImmutableList();
			if (!String.IsNullOrEmpty(reportID) && shipments.Any())
			{
				Save.Press();

				string GetActualReportID(SOShipment shipment)
				{
					Document.Current = shipment;
					GL.Branch company = null;
					using (new PXReadBranchRestrictedScope()) company = Company.Select();
					return new NotificationUtility(this).SearchCustomerReport(reportID, shipment.CustomerID, company.BranchID);
				}

				PXReportRequiredException combinedReport = shipments
					.Select(sh =>
					(
						ActualReportID: GetActualReportID(sh),
						Parameters: new Dictionary<string, string> { ["SOShipment.ShipmentNbr"] = sh.ShipmentNbr }
					))
					.Aggregate(
						(PXReportRequiredException)null,
						(acc, elem) =>
						{
							CurrentLocalization localization = null;
							INSite warehouse = INSite.PK.Find(this, Document.Current.SiteID);
							if (warehouse != null)
							{
								localization = new CurrentLocalization(OrganizationLocalizationHelper.GetCurrentLocalizationCodeForBranch(warehouse.BranchID));
							}
							var report = PXReportRequiredException.CombineReport(acc, elem.ActualReportID, elem.Parameters, localization);
							report.Mode = PXBaseRedirectException.WindowMode.New;
							return report;
						});

				if (combinedReport != null)
				{
					if (PXAccess.FeatureInstalled<FeaturesSet.deviceHub>())
					{
						LongOperationManager.StartAsyncOperation(async ct=>
						await SMPrintJobMaint.CreatePrintJobGroup(adapter, new NotificationUtility(this).SearchPrinter,
							SONotificationSource.Customer, reportID, Accessinfo.BranchID, combinedReport, null, ct));
					}

					throw combinedReport;
				}
			}
			return shipments;
		}

		public PXAction<SOShipment> printShipmentConfirmation;
		[PXButton(CommitChanges = true), PXUIField(DisplayName = "Print Shipment Confirmation", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		public virtual IEnumerable PrintShipmentConfirmation(PXAdapter adapter) => Report(adapter.Apply(it => it.Menu = "Print Shipment Confirmation"), "SO642000");

		public virtual void PrintConfirmation() => PrintShipmentConfirmation(CreateDummyAdapter());

		public virtual void SOShipmentPlan_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			if (e.Row == null) return;

			SOShipmentPlan plan = (SOShipmentPlan)e.Row;
			if (Document.Current.ShipDate < plan.PlanDate)
			{
				PXUIFieldAttribute.SetWarning<SOShipmentPlan.planDate>(sender, plan, Messages.PlanDateGreaterShipDate);
			}
		}

		public PXSelectJoin<SOShipmentPlan,
					InnerJoin<SOLineSplit, On<SOLineSplit.planID, Equal<SOShipmentPlan.planID>>,
					LeftJoin<SOOrderShipment,
						On<SOOrderShipment.orderType, Equal<SOShipmentPlan.orderType>,
							And<SOOrderShipment.orderNbr, Equal<SOShipmentPlan.orderNbr>,
							And<SOOrderShipment.operation, Equal<SOLineSplit.operation>,
							And<SOOrderShipment.siteID, Equal<SOShipmentPlan.siteID>,
							And<SOOrderShipment.confirmed, Equal<boolFalse>,
							And<SOOrderShipment.shipmentNbr, NotEqual<Current<SOShipment.shipmentNbr>>>>>>>>,
					LeftJoin<SOLine,
						On<SOLineSplit.orderType, Equal<SOLine.orderType>,
							And<SOLineSplit.orderNbr, Equal<SOLine.orderNbr>,
							And<SOLineSplit.lineNbr, Equal<SOLine.lineNbr>>>>>>>,
					Where<SOShipmentPlan.orderType, Equal<Current<AddSOFilter.orderType>>,
						And<SOShipmentPlan.orderNbr, Equal<Current<AddSOFilter.orderNbr>>,
						And2<Where<SOLine.lineNbr, Equal<Current<AddSOFilter.orderLineNbr>>, Or<Current<AddSOFilter.orderLineNbr>, IsNull>>,
						And<SOShipmentPlan.siteID, Equal<Current<SOShipment.siteID>>,
						And<SOOrderShipment.shipmentNbr, IsNull,
						And<SOLineSplit.operation, Equal<Current<AddSOFilter.operation>>,
						And2<Where<Current<SOShipment.destinationSiteID>, IsNull,
							Or<SOShipmentPlan.destinationSiteID, Equal<Current<SOShipment.destinationSiteID>>>>,
						And2<Where<SOShipmentPlan.inclQtySOShipping, Equal<True>,
							Or<SOShipmentPlan.inclQtySOShipped, Equal<True>,
							Or2<Where<SOShipmentPlan.requireAllocation, Equal<False>, Or<SOLineSplit.operation, Equal<SOOperation.receipt>>>,
							Or<SOLineSplit.lineType, Equal<SOLineType.nonInventory>>>>>,
						And<Where<Current<SOShipment.isManualPackage>, IsNull,
							Or<SOShipmentPlan.isManualPackage, Equal<Current<SOShipment.isManualPackage>>>>>>>>>>>>>> sOshipmentplanSelect;
							
		public virtual IEnumerable sOshipmentplan()
		{
			string shipmentFreightSrc = this.Document.Current?.FreightAmountSource,
				orderFreightSrc = this.addsofilter.Current?.FreightAmountSource;
			if (!shipmentFreightSrc.IsIn(null, this.addsofilter.Current?.FreightAmountSource))
				yield break;

			var shipmentSOLineSplits = new Lazy<OrigSOLineSplitSet>(() => CollectShipmentOrigSOLineSplits());

			foreach (PXResult<SOShipmentPlan, SOLineSplit, SOOrderShipment, SOLine> res in
					sOshipmentplanSelect.Select())
			{
				SOLineSplit sls = (SOLineSplit)res;
				if (!shipmentSOLineSplits.Value.Contains(sls))
				{
					yield return new PXResult<SOShipmentPlan, SOLineSplit, SOLine>((SOShipmentPlan)res, sls, (SOLine)res);
				}
			}
		}

		protected virtual OrigSOLineSplitSet CollectShipmentOrigSOLineSplits()
		{
			var ret = new OrigSOLineSplitSet();
			PXSelectBase<SOShipLine> cmd = new PXSelectReadonly<SOShipLine, Where<SOShipLine.shipmentNbr, Equal<Current<SOShipment.shipmentNbr>>>>(this);
			using (new PXFieldScope(cmd.View, typeof(SOShipLine.shipmentNbr), typeof(SOShipLine.lineNbr),
					typeof(SOShipLine.origOrderType), typeof(SOShipLine.origOrderNbr), typeof(SOShipLine.origLineNbr), typeof(SOShipLine.origSplitLineNbr)))
			{
				foreach (SOShipLine sl in cmd.Select())
				{
					ret.Add(sl);
				}
			}
			foreach (SOShipLine sl in Transactions.Cache.Deleted)
			{
				ret.Remove(sl);
			}
			foreach (SOShipLine sl in Transactions.Cache.Inserted)
			{
				ret.Add(sl);
			}
			return ret;
		}

		public PXAction<SOShipment> inventorySummary;
		[PXUIField(DisplayName = "Inventory Summary", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXLookupButton]
		public virtual IEnumerable InventorySummary(PXAdapter adapter)
		{
			PXCache tCache = Transactions.Cache;
			SOShipLine line = Transactions.Current;
			if (line == null) return adapter.Get();

			InventoryItem item = InventoryItem.PK.Find(this, line.InventoryID);
			if (item != null && item.StkItem == true)
			{
				INSubItem sbitem = (INSubItem)PXSelectorAttribute.Select<SOShipLine.subItemID>(tCache, line);
				InventorySummaryEnq.Redirect(item.InventoryID,
											 ((sbitem != null) ? sbitem.SubItemCD : null),
											 line.SiteID,
											 line.LocationID);
			}
			return adapter.Get();
		}

		public SOShipmentEntry()
		{
			if (PXAccess.FeatureInstalled<FeaturesSet.inventory>())
			{
				INSetup inrecord = insetup.Current;
			}

			CommonSetup csrecord = commonsetup.Current;
			SOSetup sorecord = sosetup.Current;


			ARSetupNoMigrationMode.EnsureMigrationModeDisabled(this);

			CopyPaste.SetVisible(false);
			PXDBDefaultAttribute.SetDefaultForInsert<SOOrderShipment.shipAddressID>(OrderList.Cache, null, true);
			PXDBDefaultAttribute.SetDefaultForUpdate<SOOrderShipment.shipAddressID>(OrderList.Cache, null, true);

			PXDBDefaultAttribute.SetDefaultForInsert<SOOrderShipment.shipContactID>(OrderList.Cache, null, true);
			PXDBDefaultAttribute.SetDefaultForUpdate<SOOrderShipment.shipContactID>(OrderList.Cache, null, true);

			PXDBLiteDefaultAttribute.SetDefaultForInsert<SOOrderShipment.shipmentNbr>(OrderList.Cache, null, true);
			PXDBLiteDefaultAttribute.SetDefaultForUpdate<SOOrderShipment.shipmentNbr>(OrderList.Cache, null, true);

			PXUIFieldAttribute.SetDisplayName<Contact.salutation>(Caches[typeof(Contact)], CR.Messages.Attention);
			this.Views.Caches.Add(typeof(SOLineSplit));
			this.Views.Caches.Add(typeof(NoteDoc));

			FieldDefaulting.AddHandler<BAccountR.type>((sender, e) => { if (e.Row != null) e.NewValue = BAccountType.CustomerType; });
		}

		#region Entity Event Handlers
		public PXWorkflowEventHandler<SOShipment> OnShipmentConfirmed;
		public PXWorkflowEventHandler<SOShipment> OnShipmentCorrected;

		public PXWorkflowEventHandler<SOShipment, SOOrderShipment, SOInvoice> OnInvoiceLinked;
		public PXWorkflowEventHandler<SOShipment, SOOrderShipment, SOInvoice> OnInvoiceUnlinked;

		public PXWorkflowEventHandler<SOShipment, SOInvoice> OnInvoiceReleased;
		public PXWorkflowEventHandler<SOShipment, SOInvoice> OnInvoiceCancelled;
		#endregion

		[InjectDependency]
		protected ILicenseLimitsService _licenseLimits { get; set; }

		void IGraphWithInitialization.Initialize()
		{
			if (_licenseLimits != null)
			{
				OnBeforeCommit += CheckLicenseLimitsBeforeCommitHandler;
			}
		}

		private void CheckLicenseLimitsBeforeCommitHandler(PXGraph e)
					{
			var checkTransactions = _licenseLimits.GetCheckerDelegate<SOShipment>(new TableQuery(TransactionTypes.LinesPerMasterRecord, typeof(SOShipLine), (graph) =>
						{
				return new PXDataFieldValue[] { new PXDataFieldValue<SOShipLine.shipmentNbr>(((SOShipmentEntry)graph).Document.Current?.ShipmentNbr) };
			}));

			try
					{
				checkTransactions.Invoke(e);
			}
			catch (PXException)
						{
				throw new PXException(Messages.LicenseShipLine);
			}


			var checkSplits = _licenseLimits.GetCheckerDelegate<SOShipment>(new TableQuery(TransactionTypes.SerialsPerDocument, typeof(SOShipLineSplit), (graph) =>
					{
				return new PXDataFieldValue[] { new PXDataFieldValue<SOShipLineSplit.shipmentNbr>(((SOShipmentEntry)graph).Document.Current?.ShipmentNbr) };
			}));

			try
						{
				checkSplits.Invoke(e);
			}
			catch (PXException)
			{
				throw new PXException(Messages.LicenseShipLineSplit);
			}


			var checkPackages = _licenseLimits.GetCheckerDelegate<SOShipment>(new TableQuery(TransactionTypes.LinesPerMasterRecord, typeof(SOPackageDetail), (graph) =>
			{
				return new PXDataFieldValue[] { new PXDataFieldValue<SOPackageDetail.shipmentNbr>(((SOShipmentEntry)graph).Document.Current?.ShipmentNbr) };
					}));

			try
			{
				checkPackages.Invoke(e);
			}
			catch (PXException)
			{
				throw new PXException(Messages.LicensePackageDetail);
			}
		}

		public PXAction<SOShipment> selectSO;
		[PXUIField(DisplayName = "Add Order", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXLookupButton]
		public virtual IEnumerable SelectSO(PXAdapter adapter)
		{
			if (this.Document.Cache.AllowDelete)
				addsofilter.AskExt();
			return adapter.Get();
		}

		public PXAction<SOShipment> addSO;
		[PXUIField(DisplayName = "", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select, Visible = false)]
		[PXLookupButton]
		public virtual IEnumerable AddSO(PXAdapter adapter)
		{
			SOOrder order = PXSelect<SOOrder,
				Where<SOOrder.orderType, Equal<Optional<AddSOFilter.orderType>>,
					And<SOOrder.orderNbr, Equal<Optional<AddSOFilter.orderNbr>>>>>.Select(this);

			bool selected = order != null &&
				(addsofilter.Current?.AddAllLines == true || AnySelected<SOShipmentPlan.selected>(soshipmentplan.Cache));

			if (selected)
			{
				try
				{
					using (LineSplittingExt.ForceUnattendedModeScope(true))
						CreateShipment(new CreateShipmentArgs
						{
							MassProcess = false,
							Order = order,
							OrderLineNbr = addsofilter.Current.OrderLineNbr,
							SiteID = Document.Current.SiteID,
							ShipDate = Document.Current.ShipDate,
							UseOptimalShipDate = false,
							Operation = addsofilter.Current.Operation,
							ShipmentList = addsofilter.Current.AddAllLines == true ? new DocumentList<SOShipment>(this) : null,
						});
				}
				finally
				{
					addsofilter.Current.AddAllLines = false;
				}

			}

			if (addsofilter.Current != null && !IsImport)
			{
				try
				{
					addsofilter.Cache.SetDefaultExt<AddSOFilter.orderType>(addsofilter.Current);
					addsofilter.Current.OrderNbr = null;
				}
				catch { }
			}

			soshipmentplan.Cache.Clear();
			soshipmentplan.View.Clear();
			soshipmentplan.Cache.ClearQueryCacheObsolete();
			sOshipmentplanSelect.View.Clear();
			ShipmentScheduleSelect.View.Clear();

			return adapter.Get();
		}

		public PXAction<SOShipment> addSOCancel;
		[PXUIField(DisplayName = "", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select, Visible = false)]
		[PXLookupButton]
		public virtual IEnumerable AddSOCancel(PXAdapter adapter)
		{
			addsofilter.Cache.SetDefaultExt<AddSOFilter.orderType>(addsofilter.Current);
			addsofilter.Current.OrderNbr = null;
			soshipmentplan.Cache.Clear();
			soshipmentplan.View.Clear();

			return adapter.Get();
		}

		#region SOOrder Events
		protected virtual void SOOrder_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			SOOrder order = e.Row as SOOrder;

			if (e.Operation == PXDBOperation.Update)
			{
				if (order.ShipmentCntr < 0 || order.OpenShipmentCntr < 0 || (order.ShipmentCntr == 0 || order.OpenShipmentCntr == 0) &&
					((IEnumerable<SOOrderShipment>)OrderList.Cache.Inserted).Any(a => a.OrderType == order.OrderType && a.OrderNbr == order.OrderNbr) ||
					order.ShipmentCntr == 0 &&
					((IEnumerable<SOOrderShipment>)OrderList.Cache.Updated).Any(a => a.OrderType == order.OrderType && a.OrderNbr == order.OrderNbr))
				{
					throw new Exceptions.InvalidShipmentCountersException();
				}
			}
		}

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXRestrictor(typeof(Where<Current<SOShipment.orderCntr>, Equal<int0>, And<Current<SOShipment.overrideFreightAmount>, Equal<False>,
			Or<ShipTerms.freightAmountSource, Equal<Current<SOShipment.freightAmountSource>>>>>),
			Messages.CantSelectShipTermsWithFreightAmountSource, typeof(ShipTerms.freightAmountSource))]
		protected virtual void SOShipment_ShipTermsID_CacheAttached(PXCache sender)
		{
		}

		#endregion

		#region SOLine CacheAttached

		[PXDBBool()]
		[PXFormula(typeof(Switch<Case<Where<SOLine.requireShipping, Equal<True>, And<SOLine.lineType, NotEqual<SOLineType.miscCharge>, And<SOLine.completed, NotEqual<True>>>>, True>, False>))]
		[DirtyFormula(typeof(Switch<Case<Where<SOLine.openLine, Equal<True>, And<Where<SOLine.isFree, NotEqual<True>, Or<SOLine.manualDisc, Equal<True>>>>>, int1>, int0>),
			typeof(SumCalc<SOOrder.openLineCntr>), IsUnbound:true)]
		[PXUIField(DisplayName = "Open Line", Enabled = false)]
		public virtual void SOLine_OpenLine_CacheAttached(PXCache sender)
		{
		}

		#endregion

		#region SOShipLine Cache Attached

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXRemoveBaseAttribute(typeof(INUnitAttribute))]
		[SOShipLineUnit(DisplayName = "UOM")]
		protected virtual void SOShipLine_UOM_CacheAttached(PXCache sender) { }

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXFormula(null, typeof(CountCalc<SOOrderShipment.lineCntr>))]
		protected virtual void SOShipLine_ShipmentNbr_CacheAttached(PXCache sender)
		{
		}

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXFormula(null, typeof(SumCalc<SOShipment.shipmentQty>))]
		[PXFormula(null, typeof(SumCalc<SOOrderShipment.shipmentQty>))]
		protected virtual void SOShipLine_ShippedQty_CacheAttached(PXCache sender)
		{
		}

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXFormula(typeof(Mult<SOShipLine.shippedQty, Mult<SOShipLine.unitPrice, Sub<decimal1, Div<SOShipLine.discPct, decimal100>>>>), typeof(SumCalc<SOShipment.lineTotal>))]
		[PXFormula(null, typeof(SumCalc<SOOrderShipment.lineTotal>))]
		protected virtual void SOShipLine_LineAmt_CacheAttached(PXCache sender)
		{
		}

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXFormula(typeof(Mult<Row<SOShipLine.baseShippedQty>.WithDependencies<SOShipLine.shippedQty, SOShipLine.uOM>, SOShipLine.unitWeigth>), typeof(SumCalc<SOShipment.shipmentWeight>))]
		[PXFormula(null, typeof(SumCalc<SOOrderShipment.shipmentWeight>))]
		protected virtual void SOShipLine_ExtWeight_CacheAttached(PXCache sender)
		{
		}

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXFormula(typeof(Mult<Row<SOShipLine.baseShippedQty>.WithDependencies<SOShipLine.shippedQty, SOShipLine.uOM>, SOShipLine.unitVolume>), typeof(SumCalc<SOShipment.shipmentVolume>))]
		[PXFormula(null, typeof(SumCalc<SOOrderShipment.shipmentVolume>))]
		protected virtual void SOShipLine_ExtVolume_CacheAttached(PXCache sender)
		{
		}

		#endregion

		#region SOShipmentAddress Cache Attached

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXCustomizeBaseAttribute(typeof(PXUIFieldAttribute), nameof(PXUIFieldAttribute.Visible), true)]
		public virtual void _(Events.CacheAttached<SOShipmentAddress.latitude> e) { }

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXCustomizeBaseAttribute(typeof(PXUIFieldAttribute), nameof(PXUIFieldAttribute.Visible), true)]
		public virtual void _(Events.CacheAttached<SOShipmentAddress.longitude> e) { }

		#endregion

		#region SOLine2 Events

		[PXDBString(TX.TaxCategory.taxCategoryID.Length, IsUnicode = true, BqlField = typeof(SOLine.taxCategoryID))]
		public virtual void SOLine2_TaxCategoryID_CacheAttached(PXCache sender)
		{
		}

		[PXDBLong(BqlField = typeof(SOLineSplit.planID), IsImmutable = true)]
		protected virtual void SOLineSplit2_PlanID_CacheAttached(PXCache sender) { }

		#endregion

		public PXAction<SOShipment> validateAddresses;
		[PXButton(CommitChanges = true), PXUIField(DisplayName = CS.Messages.ValidateAddresses, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select, FieldClass = CS.Messages.ValidateAddress)]
		public virtual IEnumerable ValidateAddresses(PXAdapter adapter)
		{
			foreach (SOShipment current in adapter.Get<SOShipment>())
			{
				if (current != null)
				{
					FindAllImplementations<IAddressValidationHelper>().ValidateAddresses();
				}
				yield return current;
			}
		}

		#region CurrencyInfo events


		protected virtual void CurrencyInfo_CuryEffDate_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (Document.Current != null)
			{
				e.NewValue = Document.Current.ShipDate;
				e.Cancel = true;
			}
		}

		protected virtual void CurrencyInfo_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			CurrencyInfo info = e.Row as CurrencyInfo;
			if (info != null)
			{
				bool curyenabled = info.AllowUpdate(this.Transactions.Cache);

				PXUIFieldAttribute.SetEnabled<CurrencyInfo.curyRateTypeID>(sender, info, curyenabled);
				PXUIFieldAttribute.SetEnabled<CurrencyInfo.curyEffDate>(sender, info, curyenabled);
				PXUIFieldAttribute.SetEnabled<CurrencyInfo.sampleCuryRate>(sender, info, curyenabled);
				PXUIFieldAttribute.SetEnabled<CurrencyInfo.sampleRecipRate>(sender, info, curyenabled);
			}
		}
		#endregion

		#region SOShipment Events
		protected virtual void SOShipment_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			var row = (SOShipment)e.Row;
			var oldRow = (SOShipment)e.OldRow;

			EnsureControlQty(row);

			if (IsFreightRecalculationNeeded(row, oldRow))
				RecalculateFreight(row, oldRow);

			if (!sender.ObjectsEqual<SOShipment.shipDate>(row, oldRow))
				SyncShipDateWithLinks(row);
		}

		protected virtual void EnsureControlQty(SOShipment shipment)
		{
			var cache = Document.Cache;

			if (sosetup.Current.RequireShipmentTotal != true)
			{
				cache.SetValue<SOShipment.controlQty>(shipment, shipment.ShipmentQty ?? 0m);
			}
			else if (shipment.Hold == false && shipment.Confirmed == false)
			{
				var controlQtyMsg = shipment.ShipmentQty != shipment.ControlQty && shipment.ControlQty != 0m
					? new PXSetPropertyException(Messages.DocumentOutOfBalance)
					: null;

				cache.RaiseExceptionHandling<SOShipment.controlQty>(shipment, shipment.ControlQty, controlQtyMsg);
			}
		}

		protected virtual bool IsFreightRecalculationNeeded(SOShipment row, SOShipment oldRow)
		{
			return
				!Document.Cache.ObjectsEqualBy<BqlFields.FilledWith<
					SOShipment.lineTotal,
					SOShipment.shipmentWeight,
					SOShipment.packageWeight,
					SOShipment.shipmentVolume,
					SOShipment.shipTermsID,
					SOShipment.shipZoneID,
					SOShipment.shipVia,
					SOShipment.curyFreightCost,
					SOShipment.overrideFreightAmount
				>>(oldRow, row);
		}

		protected virtual void RecalculateFreight(SOShipment row, SOShipment oldRow)
		{
			PXResultset<SOShipLine> shipLines = Transactions.Select();
			if (shipLines != null)
			{
				Carrier carrier = Carrier.PK.Find(this, row.ShipVia);
				if (!Document.Cache.ObjectsEqual<SOShipment.shipVia>(oldRow, row) && carrier?.CalcMethod == CarrierCalcMethod.Manual)
					row.FreightCost = 0m;

				if (UseFreightCalculator(row, carrier))
				{
					FreightCalculator fc = CreateFreightCalculator();
					fc.CalcFreightCost<SOShipment, SOShipment.curyFreightCost>(Document.Cache, row);

					if (row.OverrideFreightAmount != true)
						fc.ApplyFreightTerms<SOShipment, SOShipment.curyFreightAmt>(Document.Cache, row, shipLines.Count);
				}
					else if (UseCarrierService(row, carrier) && row.OverrideFreightAmount != true)
					{
						FreightCalculator fc = CreateFreightCalculator();
						if (fc.IsFlatRate(Document.Cache, row))
						{
							fc.ApplyFreightTerms<SOShipment, SOShipment.curyFreightAmt>(Document.Cache, row, shipLines.Count);
						}
					}
			}
		}

		protected virtual void SyncShipDateWithLinks(SOShipment shipment)
		{
			var cache = OrderList.Cache;

			foreach (SOOrderShipment link in OrderList.Select())
			{
				if (link.ShipmentType != SOShipmentType.DropShip)
				{
					cache.SetValue<SOOrderShipment.shipDate>(link, shipment.ShipDate);
					cache.MarkUpdated(link, assertError: true);
				}
			}
		}

		protected virtual void SOShipment_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			if (e.Row == null)
			{
				return;
			}

			bool shallBePackedViaPaperlessPack(SOShipment shipment) =>
				SOPickingWorksheet.PK.Find(this, shipment.CurrentWorksheetNbr)?.WorksheetType == SOPickingWorksheet.worksheetType.Single &&
				SOPickPackShipSetup.PK.Find(this, Accessinfo.BranchID)?.IsPackOnly == true;

			SOShipment row = (SOShipment)e.Row;
			bool isTransfer = row.ShipmentType == SOShipmentType.Transfer;
			bool isNotConfirmed = row.Confirmed == false;
			bool isNotAddedToWorksheet = row.CurrentWorksheetNbr == null;
			bool isNotHeldByPicking = isNotAddedToWorksheet || !shallBePackedViaPaperlessPack(row) && row.Picked == true;
			bool isNotReadonly = isNotConfirmed;

			PXUIFieldAttribute.SetVisible<SOShipment.curyID>(sender, e.Row,
				PXAccess.FeatureInstalled<FeaturesSet.multicurrency>() && !isTransfer);

			bool curyenabled = true;

			PXUIFieldAttribute.SetEnabled(sender, e.Row, isNotReadonly);
			PXUIFieldAttribute.SetEnabled<SOShipment.curyID>(sender, e.Row, isNotReadonly && curyenabled);
			PXUIFieldAttribute.SetEnabled<SOShipment.status>(sender, e.Row, false);
			PXUIFieldAttribute.SetEnabled<SOShipment.shipmentQty>(sender, e.Row, false);
			PXUIFieldAttribute.SetEnabled<SOShipment.shipmentWeight>(sender, e.Row, false);
			PXUIFieldAttribute.SetEnabled<SOShipment.packageWeight>(sender, e.Row, false);
			PXUIFieldAttribute.SetEnabled<SOShipment.shipmentVolume>(sender, e.Row, false);
			PXUIFieldAttribute.SetEnabled<SOShipment.curyFreightCost>(sender, e.Row, false);
			PXUIFieldAttribute.SetEnabled<SOShipment.curyFreightAmt>(sender, e.Row, isNotReadonly && row.OverrideFreightAmount == true);
			PXUIFieldAttribute.SetEnabled<SOShipment.overrideFreightAmount>(sender, e.Row, AllowChangingOverrideFreightAmount(row));
			PXUIFieldAttribute.SetEnabled<SOShipment.freightAmountSource>(sender, e.Row, false);
			PXUIFieldAttribute.SetEnabled<SOShipment.currentWorksheetNbr>(sender, e.Row, false);

			sender.AllowInsert = true;
			sender.AllowUpdate = isNotConfirmed;
			sender.AllowDelete = isNotReadonly && isNotAddedToWorksheet;
			selectSO.SetEnabled(row.SiteID != null && sender.AllowDelete);

			Transactions.Cache.AllowInsert = false;
			Transactions.Cache.AllowUpdate = isNotReadonly && isNotAddedToWorksheet;
			Transactions.Cache.AllowDelete = isNotReadonly && isNotAddedToWorksheet;

			splits.Cache.AllowInsert = isNotReadonly && isNotAddedToWorksheet;
			splits.Cache.AllowUpdate = isNotReadonly && isNotAddedToWorksheet;
			splits.Cache.AllowDelete = isNotReadonly && isNotAddedToWorksheet;

			Packages.Cache.AllowInsert = isNotConfirmed && isNotHeldByPicking;
			Packages.Cache.AllowUpdate = isNotConfirmed && isNotHeldByPicking;
			Packages.Cache.AllowDelete = isNotConfirmed && isNotHeldByPicking;

			PXUIFieldAttribute.SetVisible<SOShipment.controlQty>(sender, e.Row, (bool)sosetup.Current.RequireShipmentTotal || row.Confirmed == true);

			bool allowUpdateAndHasNoDetails = sender.AllowUpdate && Transactions.Select().Count == 0;
			PXUIFieldAttribute.SetEnabled<SOShipment.shipmentType>(sender, e.Row,
				allowUpdateAndHasNoDetails && sender.GetStatus(e.Row) == PXEntryStatus.Inserted);
			PXUIFieldAttribute.SetEnabled<SOShipment.operation>(sender, e.Row, allowUpdateAndHasNoDetails);
			PXUIFieldAttribute.SetEnabled<SOShipment.customerID>(sender, e.Row, allowUpdateAndHasNoDetails);
			PXUIFieldAttribute.SetEnabled<SOShipment.customerLocationID>(sender, e.Row, allowUpdateAndHasNoDetails);
			PXUIFieldAttribute.SetEnabled<SOShipment.siteID>(sender, e.Row, allowUpdateAndHasNoDetails);
			PXUIFieldAttribute.SetEnabled<SOShipment.destinationSiteID>(sender, e.Row, allowUpdateAndHasNoDetails && isTransfer);

			this.validateAddresses.SetEnabled(isNotReadonly && FindAllImplementations<IAddressValidationHelper>().RequiresValidation());

			if (((SOShipment)e.Row).ShipVia != null)
			{
				Carrier carrier = Carrier.PK.Find(this, row.ShipVia);

				if (carrier != null)
				{
					PXUIFieldAttribute.SetEnabled<SOShipment.curyFreightCost>(sender, e.Row, carrier.CalcMethod == CarrierCalcMethod.Manual && isNotReadonly);
				}
			}

			PXUIFieldAttribute.SetVisible<SOShipment.groundCollect>(sender, e.Row, this.CanUseGroundCollect(row));

			PXUIFieldAttribute.SetVisible<SOShipment.customerID>(sender, e.Row, !isTransfer);
			PXUIFieldAttribute.SetVisible<SOShipment.customerLocationID>(sender, e.Row, !isTransfer);

			PXUIFieldAttribute.SetVisible<SOShipment.destinationSiteID>(sender, e.Row, isTransfer);

			PXUIFieldAttribute.SetVisible<SOShipLine.isFree>(Transactions.Cache, null, !isTransfer);

            PXUIFieldAttribute.SetRequired<SOShipment.destinationSiteID>(sender, true);

			PXUIFieldAttribute.SetVisible<SOShipment.curyFreightAmt>(sender, e.Row, row.FreightAmountSource.IsIn(null, FreightAmountSourceAttribute.ShipmentBased));
			PXUIFieldAttribute.SetVisible<SOShipment.overrideFreightAmount>(sender, e.Row, row.FreightAmountSource.IsIn(null, FreightAmountSourceAttribute.ShipmentBased));
		}

		protected virtual bool AllowChangingOverrideFreightAmount(SOShipment doc)
		{
			return doc.Confirmed == false &&
				doc.FreightAmountSource.IsIn(null, FreightAmountSourceAttribute.ShipmentBased);
		}

		protected virtual bool UseFreightCalculator(SOShipment row, Carrier carrier) 
			=> carrier == null
				|| (carrier.IsExternal != true //for external carrier cost and terms are calculated in ShipPackages().
					&& AllowCalculateFreight(row, carrier));

		protected virtual bool UseCarrierService(SOShipment row, Carrier carrier)
			=> carrier != null && carrier.IsExternal == true && AllowCalculateFreight(row, carrier);

		protected virtual bool AllowCalculateFreight(SOShipment row, Carrier carrier)
		{
			if (row.Operation == SOOperation.Receipt)
				return carrier.CalcFreightOnReturn == true;
			return true;
		}

		protected virtual void SOShipment_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
            if ((e.Operation & PXDBOperation.Command) == PXDBOperation.Delete) return;
			SOShipment doc = (SOShipment)e.Row;
			if (doc.ShipmentType == SOShipmentType.Transfer && doc.DestinationSiteID == null)
			{
				throw new PXRowPersistingException(typeof(SOOrder.destinationSiteID).Name, null, ErrorMessages.FieldIsEmpty, typeof(SOOrder.destinationSiteID).Name);
			}
		}

		protected virtual void SOShipment_CustomerID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			sender.SetDefaultExt<SOShipment.customerLocationID>(e.Row);
		}

		protected virtual void SOShipment_CustomerLocationID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			if (((SOShipment)e.Row).ShipmentType != SOShipmentType.Transfer && (((SOShipment)e.Row).SiteID == null || e.ExternalCall))
				sender.SetDefaultExt<SOShipment.siteID>(e.Row);
			SOShipmentAddressAttribute.DefaultRecord<SOShipment.shipAddressID>(sender, e.Row);
			SOShipmentContactAttribute.DefaultRecord<SOShipment.shipContactID>(sender, e.Row);
		}

		protected virtual void SOShipment_DestinationSiteID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			SOShipment shipment = e.Row as SOShipment;
			if (shipment == null || shipment.ShipmentType != SOShipmentType.Transfer)
			{
				e.NewValue = null;
				e.Cancel = true;
			}
		}

		protected virtual void SOShipment_DestinationSiteID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			Company.RaiseFieldUpdated(sender, e.Row);
			GL.Branch company = null;
			using (new PXReadBranchRestrictedScope())
			{
				company = Company.Select();
			}

			if (((SOShipment)e.Row).ShipmentType == SOShipmentType.Transfer && company != null)
			{
				sender.SetValueExt<SOShipment.customerID>(e.Row, company.BranchCD);
			}

			SOShipmentAddressAttribute.DefaultRecord<SOShipment.shipAddressID>(sender, e.Row);
			SOShipmentContactAttribute.DefaultRecord<SOShipment.shipContactID>(sender, e.Row);
		}

		protected virtual void SOShipment_ShipVia_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			RedefaultCurrencyInfo(sender, e);

			sender.SetDefaultExt<SOShipment.taxCategoryID>(e.Row);

			SOShipment row = e.Row as SOShipment;
			if (row != null)
			{
				object pendingValue = sender.GetValuePending<SOShipment.useCustomerAccount>(e.Row);
				if (pendingValue != PXCache.NotSetValue && row.ShipViaUpdateFromShopForRate != true)
				{
					row.UseCustomerAccount = CanUseCustomerAccount(row) && (bool?)pendingValue == true;
				}
				else
				{
					row.UseCustomerAccount = CanUseCustomerAccount(row);
				}

				sender.SetValue<SOShipment.isPackageValid>(row, false);
				Document.Current.RecalcPackagesReason = (Document.Current.RecalcPackagesReason ?? 0) | SOShipment.recalcPackagesReason.ShipVia;
			}
		}

		public virtual void RedefaultCurrencyInfo(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			if (PXAccess.FeatureInstalled<FeaturesSet.multicurrency>())
			{
				CurrencyInfo info = CurrencyInfoAttribute.SetDefaults<SOShipment.curyInfoID>(sender, e.Row);

				string message = PXUIFieldAttribute.GetError<CurrencyInfo.curyEffDate>(currencyinfo.Cache, info);
				if (string.IsNullOrEmpty(message) == false)
				{
					sender.RaiseExceptionHandling<SOShipment.shipDate>(e.Row, ((SOShipment)e.Row).ShipDate, new PXSetPropertyException(message, PXErrorLevel.Warning));
				}

				if (info != null)
				{
					sender.SetValue<SOShipment.curyID>(e.Row, info.CuryID);
				}
			}
		}

		protected virtual bool CanUseCustomerAccount(SOShipment row)
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

		protected virtual bool CanUseGroundCollect(SOShipment row)
		{
			if (string.IsNullOrEmpty(row.ShipVia))
				return false;

			Carrier carrier = Carrier.PK.Find(this, row.ShipVia);
			if (carrier?.IsExternal != true || string.IsNullOrEmpty(carrier?.CarrierPluginID))
				return false;

			return CarrierPluginMaint.GetCarrierPluginAttributes(this, carrier.CarrierPluginID).Contains("COLLECT");
		}

		protected virtual void SOShipment_UseCustomerAccount_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			SOShipment row = e.Row as SOShipment;
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

		protected virtual void SOShipment_ShipTermsID_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			var row = (SOShipment)e.Row;
			if (row != null && row.OrderCntr > 0 && row.FreightAmountSource == FreightAmountSourceAttribute.OrderBased)
			{
				PXUIFieldAttribute.SetWarning<SOShipment.shipTermsID>(sender, e.Row, Messages.FreightPriceNotRecalcInSO);
			}
		}

		#endregion

		#region SOOrderShipment Events

		protected virtual void UpdateShipmentCntr(PXCache sender, SOOrderShipment row, short? Counter)
		{
			SOOrder order = (SOOrder)PXParentAttribute.SelectParent(sender, row, typeof(SOOrder));
			if (order != null)
			{
				order.ShipmentDeleted = (Counter == -1) ? true : (bool?)null;
				order.ShipmentCntr += Counter;
				if (row.Confirmed == false)
				{
					order.OpenShipmentCntr += Counter;
				}
				soorder.Cache.Update(order);
			}
			SOOrderSite orderSite = (SOOrderSite)PXParentAttribute.SelectParent(sender, row, typeof(SOOrderSite));
			if (orderSite != null && Counter != 0)
			{
				orderSite.ShipmentCntr += Counter;
				if (row.Confirmed == false)
					orderSite.OpenShipmentCntr += Counter;

				orderSite = OrderSite.Update(orderSite);
			}
		}

		protected virtual void SOOrderShipment_RowInserted(PXCache sender, PXRowInsertedEventArgs e)
		{
			UpdateShipmentCntr(sender, (SOOrderShipment)e.Row, 1);
		}

		protected virtual void SOOrderShipment_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			//during correct shipment this will eliminate overwrite of SOOrder in SOShipmentEntry.Persist()
			if (!object.ReferenceEquals(e.Row, e.OldRow))
			{
				UpdateShipmentCntr(sender, (SOOrderShipment)e.OldRow, -1);
				UpdateShipmentCntr(sender, (SOOrderShipment)e.Row, 1);
			}
		}

		protected virtual void SOOrderShipment_RowDeleted(PXCache sender, PXRowDeletedEventArgs e)
		{
			var link = (SOOrderShipment)e.Row;
			UpdateShipmentCntr(sender, link, -1);

			SOOrderShipment.Events
				.Select(ev => ev.ShipmentUnlinked)
				.FireOn(this, link, Document.Current);

			RestoreCustomerOrderNbr();
			ResetManualPackageFlag();
		}

		protected virtual void RestoreCustomerOrderNbr()
		{
			SOShipment shipment = Document.Current;
			if (shipment == null || shipment.OrderCntr != 1 || shipment.CustomerOrderNbr != null)
				return;

			// If we have single Order within shipment we should fill CustomerOrderNbr.
			SOOrderShipment orderShipment = OrderListSimple.Select();
			if (orderShipment == null)
				return;

			SOOrder order = PXParentAttribute.SelectParent<SOOrder>(OrderListSimple.Cache, orderShipment);
			if (!string.IsNullOrEmpty(order.CustomerOrderNbr))
			{
				shipment.CustomerOrderNbr = order.CustomerOrderNbr;
				Document.Update(shipment);
			}
		}

		protected virtual void ResetManualPackageFlag()
		{
			SOShipment shipment = Document.Current;
			if (shipment == null || shipment.OrderCntr != 0)
				return;

			shipment.IsManualPackage = null;
			Document.Update(shipment);
		}

		protected virtual void SOOrderShipment_ShipmentNbr_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			e.Cancel = true;
		}

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXRemoveBaseAttribute(typeof(PXDefaultAttribute))]
		[PXDBDefault(typeof(SOShipment.siteID), PersistingCheck = PXPersistingCheck.Nothing)]
		protected void SOOrderShipment_SiteID_CacheAttached(PXCache sender) { }

		[InjectDependency]
		public IFinPeriodRepository FinPeriodRepository { get; set; }

		protected virtual void SOShipment_ShipDate_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			var row = (SOShipment)e.Row;
			if (e.NewValue != null)				
			{
					FinPeriod finPeriod = FinPeriodRepository.FindFinPeriodByDate((DateTime?)e.NewValue, FinPeriod.organizationID.MasterValue);

					if (finPeriod == null)
					{
						throw new PXSetPropertyException<SOOrder.shipDate>(GL.Messages.TranDateOutOfRange, e.NewValue, PXAccess.GetOrganizationCD(FinPeriod.organizationID.MasterValue));
					}
			}
		}

		protected virtual void SOOrderShipment_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			PXUIFieldAttribute.SetEnabled(sender, e.Row, false);
			PXUIFieldAttribute.SetEnabled<SOOrderShipment.selected>(sender, e.Row, true);

		}

		#endregion

		#region SOShipLine Events
		protected virtual void SOShipLine_InventoryID_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			object oldValue = sender.GetValue<SOShipLine.inventoryID>(e.Row);
			if (oldValue != null)
			{
				e.NewValue = oldValue;
			}
		}

		protected virtual void SOShipLine_SubItemID_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			object oldValue = sender.GetValue<SOShipLine.subItemID>(e.Row);
			if (oldValue != null && e.NewValue != null && e.ExternalCall)
			{
				e.NewValue = oldValue;
			}
		}

		protected virtual void SOShipLine_SiteID_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			object oldValue = sender.GetValue<SOShipLine.siteID>(e.Row);
			if (oldValue != null && e.ExternalCall)
			{
				e.NewValue = oldValue;
			}
		}

		protected virtual void SOShipLine_InventoryID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			sender.SetDefaultExt<SOShipLine.uOM>(e.Row);
			sender.SetDefaultExt<SOShipLine.unitWeigth>(e.Row);
			sender.SetDefaultExt<SOShipLine.unitVolume>(e.Row);

			SOShipLine tran = e.Row as SOShipLine;
			InventoryItem item = InventoryItem.PK.Find(this, tran?.InventoryID);
			if (item != null && tran != null)
			{
				tran.TranDesc = PXDBLocalizableStringAttribute.GetTranslation(Caches[typeof(InventoryItem)], item, nameof(InventoryItem.Descr), customer.Current?.LocaleName);
			}
		}

		protected virtual void SOShipLine_LocationID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (PXAccess.FeatureInstalled<FeaturesSet.warehouseLocation>())
			{
				e.NewValue = null;
				e.Cancel = true;
			}
		}

		protected virtual void DefaultUnitPrice(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			object UnitPrice;
			sender.RaiseFieldDefaulting<SOShipLine.unitPrice>(e.Row, out UnitPrice);

			if (UnitPrice != null && (decimal)UnitPrice != 0m)
			{
				decimal? unitprice = INUnitAttribute.ConvertFromTo<SOShipLine.inventoryID>(sender, e.Row, ((SOShipLine)e.Row).UOM, ((SOShipLine)e.Row).OrderUOM, (decimal)UnitPrice, INPrecision.UNITCOST);
				sender.SetValueExt<SOShipLine.unitPrice>(e.Row, unitprice);
			}
		}

		protected virtual void DefaultUnitCost(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			object UnitCost;
			sender.RaiseFieldDefaulting<SOShipLine.unitCost>(e.Row, out UnitCost);

			if (UnitCost != null && (decimal)UnitCost != 0m)
			{
				decimal? unitcost = INUnitAttribute.ConvertFromTo<SOShipLine.inventoryID>(sender, e.Row, ((SOShipLine)e.Row).UOM, ((SOShipLine)e.Row).OrderUOM, (decimal)UnitCost, INPrecision.UNITCOST);
				sender.SetValueExt<SOShipLine.unitCost>(e.Row, unitcost);
			}
		}

		protected virtual void SOShipLine_UOM_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			SOShipLine row = e.Row as SOShipLine;
			if (row != null)
			{
				DefaultUnitPrice(sender, e);
				DefaultUnitCost(sender, e);

				Transactions.Cache.RaiseFieldUpdated<SOShipLine.origOrderQty>(row, null);
			}
		}

		protected virtual void SOShipLine_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			SOShipLine row = e.Row as SOShipLine;
			if (row != null)
			{
				bool lineTypeInventory = row.LineType == SOLineType.Inventory;
				PXUIFieldAttribute.SetEnabled<SOShipLine.subItemID>(sender, row, lineTypeInventory);
				PXUIFieldAttribute.SetEnabled<SOShipLine.locationID>(sender, row, lineTypeInventory);

				InventoryItem item = InventoryItem.PK.Find(this, row.InventoryID);
				if (item != null)
					PXUIFieldAttribute.SetEnabled<SOShipLineSplit.inventoryID>(splits.Cache, null, item.KitItem == true && item.StkItem != true);

				splits.Cache.AllowInsert = sender.AllowUpdate && SyncLineWithOrder(row);
			}
		}

		protected virtual void SOShipLine_RowInserted(PXCache sender, PXRowInsertedEventArgs e)
		{
			Document.SetValueExt<SOShipment.isPackageValid>(Document.Current, false);
			Document.Current.RecalcPackagesReason = (Document.Current.RecalcPackagesReason ?? 0) | SOShipment.recalcPackagesReason.ShipLine;

			SOShipLine row = e.Row as SOShipLine;
			if (row != null)
			{
				row.SortOrder = row.LineNbr;
			}
		}

		protected virtual void SOShipLine_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			SOShipLine row = e.Row as SOShipLine;
			SOShipLine oldRow = e.OldRow as SOShipLine;
			if (row != null && sender.GetStatus(row) == PXEntryStatus.Inserted)
			{
				row.OriginalShippedQty = row.ShippedQty;
				row.BaseOriginalShippedQty = row.BaseShippedQty;
			}

			if (row != null && row.IsFree != true && !sender.ObjectsEqual<SOShipLine.shippedQty>(e.Row, e.OldRow))
			{
				PXSelectBase<SOShipmentDiscountDetail> selectDiscountDetailsByOrder = new PXSelect<SOShipmentDiscountDetail,
					Where<SOShipmentDiscountDetail.orderType, Equal<Required<SOShipmentDiscountDetail.orderType>>,
					And<SOShipmentDiscountDetail.orderNbr, Equal<Required<SOShipmentDiscountDetail.orderNbr>>,
					And<SOShipmentDiscountDetail.shipmentNbr, Equal<Required<SOShipmentDiscountDetail.shipmentNbr>>,
					And<SOShipmentDiscountDetail.type, Equal<DiscountType.LineDiscount>>>>>>(this);

				foreach (SOShipmentDiscountDetail sdd in selectDiscountDetailsByOrder.Select(row.OrigOrderType, row.OrigOrderNbr, row.ShipmentNbr))
				{
					_discountEngine.DeleteDiscountDetail(sender, DiscountDetails, sdd);
				}

				SOOrder order = soorder.Select(row.OrigOrderType, row.OrigOrderNbr);

				if (order != null && !sender.Graph.UnattendedMode)
				{
					AllocateGroupFreeItems(order);
					AdjustFreeItemLines();
				}
			}

			if (row != null && oldRow != null && (row.BaseQty != oldRow.BaseQty))
			{
				Document.SetValueExt<SOShipment.isPackageValid>(Document.Current, false);
				Document.Current.RecalcPackagesReason = (Document.Current.RecalcPackagesReason ?? 0) | SOShipment.recalcPackagesReason.ShipLine;
			}
		}

		protected virtual void SOShipLine_RowDeleted(PXCache sender, PXRowDeletedEventArgs e)
		{
			SOShipLine deleted = (SOShipLine)e.Row;
			if (deleted == null) return;

			var parentDeleted = Document.Cache.GetStatus(Document.Current) == PXEntryStatus.Deleted;
			if (parentDeleted)
				return;

			bool orderDeleted = false;
			SOShipLine line = PXSelect<SOShipLine, Where<SOShipLine.shipmentType, Equal<Current<SOShipLine.shipmentType>>, And<SOShipLine.shipmentNbr, Equal<Current<SOShipLine.shipmentNbr>>, And<SOShipLine.origOrderType, Equal<Current<SOShipLine.origOrderType>>, And<SOShipLine.origOrderNbr, Equal<Current<SOShipLine.origOrderNbr>>>>>>>.SelectSingleBound(this, new object[] { deleted });
			if (line == null)
			{
				SOOrderShipment oship = PXSelect<SOOrderShipment, Where<SOOrderShipment.shipmentType, Equal<Current<SOShipLine.shipmentType>>, And<SOOrderShipment.shipmentNbr, Equal<Current<SOShipLine.shipmentNbr>>, And<SOOrderShipment.orderType, Equal<Current<SOShipLine.origOrderType>>, And<SOOrderShipment.orderNbr, Equal<Current<SOShipLine.origOrderNbr>>>>>>>.SelectSingleBound(this, new object[] { deleted });
				OrderList.Delete(oship);
				orderDeleted = true;
			}

			SOOrder order = soorder.Select(deleted.OrigOrderType, deleted.OrigOrderNbr);

			if (order != null)
			{
				AllocateGroupFreeItems(order);
				AdjustFreeItemLines();
				if (deleted.KeepManualFreight != true)
					UpdateManualFreightCost(Document.Current, order, orderDeleted, true);
				deleted.KeepManualFreight = false;

				if (line == null)
				{
					Guid[] orderFileGuids = PXNoteAttribute.GetFileNotes(this.Caches[typeof(SOOrder)], order);

					foreach (NoteDoc file in this.Caches[typeof(NoteDoc)].Cached)
					{
						if (orderFileGuids.Contains(file.FileID ?? Guid.Empty) && file.NoteID == Document.Current.NoteID)
						{
							this.Caches[typeof(NoteDoc)].Delete(file);
						}
					}
				}
			}

			Document.SetValueExt<SOShipment.isPackageValid>(Document.Current, false);
			Document.Current.RecalcPackagesReason = (Document.Current.RecalcPackagesReason ?? 0) | SOShipment.recalcPackagesReason.ShipLine;
		}

		protected virtual void SOShipLine_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			SOShipLine row = (SOShipLine)e.Row;

			if (((e.Operation & PXDBOperation.Command) == PXDBOperation.Insert || (e.Operation & PXDBOperation.Command) == PXDBOperation.Update))
			{
				CheckSplitsForSameTask(sender, row);
				CheckLocationTaskRule(sender, row);
			}
		}

		protected virtual void SOShipLine_RowUpdating(PXCache sender, PXRowUpdatingEventArgs e)
		{
			if (!SyncLineWithOrder((SOShipLine)e.Row))
			{
				e.Cancel = true;
				var error = new PXSetPropertyException<SOShipLine.shippedQty>(Messages.CannotEditShipLineWithDiffSite, PXErrorLevel.Warning);
				if (sender.RaiseExceptionHandling<SOShipLine.shippedQty>(e.NewRow, ((SOShipLine)e.Row).ShippedQty, error))
					throw error;
			}
		}
		#endregion

		#region SOShipLineSplit Events

		protected virtual void SOShipLineSplit_InventoryID_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			SOShipLine line = PXParentAttribute.SelectParent(sender, e.Row, typeof(SOShipLine)) as SOShipLine;
			if (line != null )
			{
				InventoryItem item = InventoryItem.PK.Find(this, line.InventoryID);
				if (item != null && item.KitItem == true && item.StkItem != true)
				{
					INKitSpecHdr detail = 
						PXSelectJoin<INKitSpecHdr, 
							LeftJoin<INKitSpecStkDet, On<INKitSpecStkDet.kitInventoryID, Equal<INKitSpecHdr.kitInventoryID>>,
							LeftJoin<INKitSpecNonStkDet, On<INKitSpecNonStkDet.kitInventoryID, Equal<INKitSpecHdr.kitInventoryID>>>>,
						Where<INKitSpecHdr.kitInventoryID, Equal<Required<INKitSpecStkDet.kitInventoryID>>,
						And<
							Where<INKitSpecStkDet.compInventoryID, Equal<Required<INKitSpecStkDet.compInventoryID>>,
							Or<INKitSpecNonStkDet.compInventoryID, Equal<Required<INKitSpecNonStkDet.compInventoryID>>>>>>>.SelectWindowed(this, 0, 1, line.InventoryID, e.NewValue, e.NewValue);

					if (detail == null)
					{
						InventoryItem val = InventoryItem.PK.Find(this, (int?)e.NewValue);

						var ex = new PXSetPropertyException<SOShipLineSplit.inventoryID>(Messages.NotKitsComponent);
						ex.ErrorValue = val?.InventoryCD;

						throw ex;
					}
				}
			}
		}

		#endregion

		#region AddSOFilter Events
		protected virtual void AddSOFilter_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			var doc = this.Document.Current;
			PXUIFieldAttribute.SetEnabled<AddSOFilter.operation>(sender, e.Row, doc?.Operation == null);

			var filter = (AddSOFilter)e.Row;
			if (filter == null)
				return;

			var warningException =
				(doc?.FreightAmountSource == null || filter.FreightAmountSource == null || doc.FreightAmountSource == filter.FreightAmountSource)
				? null
				: new PXSetPropertyException(Messages.CantAddOrderWithFreightAmountSource, PXErrorLevel.Warning,
					sender.GetValueExt<AddSOFilter.freightAmountSource>(filter));
			sender.RaiseExceptionHandling<AddSOFilter.orderNbr>(e.Row, filter.FreightAmountSource, warningException);
		}
		#endregion

		#region SOPackageDetail Events

		protected virtual void SOPackageDetailEx_Weight_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			SOPackageDetail row = e.Row as SOPackageDetail;
			if (row != null)
			{
				row.Confirmed = true;
			}
		}

		protected virtual void SOPackageDetailEx_Weight_FieldVerifying(PXCache cache, PXFieldVerifyingEventArgs e)
		{
			var row = (SOPackageDetail) e.Row;
			if (row != null)
			{
				CSBox box = SOPackageDetail.FK.Box.FindParent(cache.Graph, row);
				if (box != null && box.MaxWeight < (decimal?) e.NewValue)
					throw new PXSetPropertyException(Messages.WeightExceedsBoxSpecs);
			}
		}

		protected virtual void SOPackageDetailEx_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			SOPackageDetail row = e.Row as SOPackageDetail;
			if (row != null)
			{
				row.WeightUOM = commonsetup.Current.WeightUOM;
			}
		}

		#endregion

		#region SOShipmentContact Events

		private const string UpsCarrierPlugin = "PX.UpsCarrier.UpsCarrier";
		private const int MaxNameLengthForUps = 35;
		private const string ReturnLabel = "ReturnLabel";

		protected virtual bool BusinessNameLengthIsExceeded(SOShipment doc, SOShipmentContact contact)
		{
			if (doc == null || doc.ShipVia == null || doc.Confirmed == true
				|| contact == null || (contact.FullName ?? string.Empty).Length <= MaxNameLengthForUps)
				return false;

			Carrier carrier = Carrier.PK.Find(this, Document.Current.ShipVia);
			if (carrier == null || carrier.IsExternal != true)
				return false;

			CarrierPlugin carrierPlugin = CarrierPlugin.PK.Find(this, carrier.CarrierPluginID);
			if (carrierPlugin == null || carrierPlugin.PluginTypeName != UpsCarrierPlugin)
				return false;

			return true;
		}

		protected virtual void SOShipmentContact_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			var contact = (SOShipmentContact)e.Row;
			bool warnOnBusinessName = BusinessNameLengthIsExceeded(Document.Current, contact);
			var warnOnBusinessNameExc = !warnOnBusinessName ? null : new PXSetPropertyException(
				Messages.TooLongValueForUPS, PXErrorLevel.Warning,
				PXUIFieldAttribute.GetDisplayName<SOShipmentContact.fullName>(sender), MaxNameLengthForUps);

			sender.RaiseExceptionHandling<SOShipmentContact.fullName>(e.Row, contact.FullName, warnOnBusinessNameExc);
		}

		#endregion

		#region Processing
		public virtual decimal? ShipAvailableLots(SOShipmentPlan plan, SOShipLine newline, INLotSerClass lotserclass)
		{
			return CreateSplitsForAvailableLots(plan.PlanQty, plan.PlanType, plan.LotSerialNbr, newline, lotserclass);
		}

		public virtual decimal? CreateSplitsForAvailableLots(
			decimal? PlannedQty, string origPlanType, string origLotSerialNbr,
			SOShipLine newline, INLotSerClass lotserclass)
		{
			return CreateSplitsForAvailableLotsImpl<
				INLotSerialStatusByCostCenter,
				LotSerialStatusByCostCenter,
				INSiteStatusByCostCenter,
				SiteStatusByCostCenter>(PlannedQty, origPlanType, origLotSerialNbr, newline, lotserclass);
		}

		protected virtual List<PXResult> SelectLotSerialStatus(string origLotSerialNbr, SOShipLine newline, INLotSerClass lotserclass)
		{
			PXSelectBase<INLotSerialStatusByCostCenter> cmd;
			if (!string.IsNullOrEmpty(origLotSerialNbr))
			{
				cmd = new PXSelectReadonly2<INLotSerialStatusByCostCenter,
					InnerJoin<INLocation, On<INLotSerialStatusByCostCenter.FK.Location>,
					LeftJoin<INSiteStatusByCostCenter, On<INSiteStatusByCostCenter.inventoryID, Equal<INLotSerialStatusByCostCenter.inventoryID>,
						And<INSiteStatusByCostCenter.subItemID, Equal<INLotSerialStatusByCostCenter.subItemID>,
						And<INSiteStatusByCostCenter.siteID, Equal<INLotSerialStatusByCostCenter.siteID>,
						And<INSiteStatusByCostCenter.costCenterID, Equal<INLotSerialStatusByCostCenter.costCenterID>>>>>>>,
				Where<INLotSerialStatusByCostCenter.inventoryID, Equal<Required<INLotSerialStatusByCostCenter.inventoryID>>,
					And<INLotSerialStatusByCostCenter.subItemID, Equal<Required<INLotSerialStatusByCostCenter.subItemID>>,
					And<INLotSerialStatusByCostCenter.siteID, Equal<Required<INLotSerialStatusByCostCenter.siteID>>,
					And<INLotSerialStatusByCostCenter.costCenterID, Equal<Required<INLotSerialStatusByCostCenter.costCenterID>>,
					And<INLocation.salesValid, Equal<boolTrue>,
					And<INLocation.inclQtyAvail, Equal<boolTrue>>>>>>>>(this);
			}
			else
			{
				cmd = new PXSelectReadonly2<INLotSerialStatusByCostCenter,
					InnerJoin<INLocation, On<INLotSerialStatusByCostCenter.FK.Location>,
					LeftJoin<INSiteStatusByCostCenter, On<INSiteStatusByCostCenter.inventoryID, Equal<INLotSerialStatusByCostCenter.inventoryID>,
						And<INSiteStatusByCostCenter.subItemID, Equal<INLotSerialStatusByCostCenter.subItemID>,
						And<INSiteStatusByCostCenter.siteID, Equal<INLotSerialStatusByCostCenter.siteID>,
						And<INSiteStatusByCostCenter.costCenterID, Equal<INLotSerialStatusByCostCenter.costCenterID>>>>>,
					InnerJoin<INSiteLotSerial, On<INSiteLotSerial.inventoryID, Equal<INLotSerialStatusByCostCenter.inventoryID>,
						And<INSiteLotSerial.siteID, Equal<INLotSerialStatusByCostCenter.siteID>,
						And<INSiteLotSerial.lotSerialNbr, Equal<INLotSerialStatusByCostCenter.lotSerialNbr>>>>>>>,
				Where<INLotSerialStatusByCostCenter.inventoryID, Equal<Required<INLotSerialStatusByCostCenter.inventoryID>>,
					And<INLotSerialStatusByCostCenter.subItemID, Equal<Required<INLotSerialStatusByCostCenter.subItemID>>,
					And<INLotSerialStatusByCostCenter.siteID, Equal<Required<INLotSerialStatusByCostCenter.siteID>>,
					And<INLotSerialStatusByCostCenter.costCenterID, Equal<Required<INLotSerialStatusByCostCenter.costCenterID>>,
					And<INLocation.salesValid, Equal<boolTrue>,
					And<INLocation.inclQtyAvail, Equal<boolTrue>,
					And<INLotSerialStatusByCostCenter.qtyOnHand, Greater<decimal0>,
					And<INSiteLotSerial.qtyHardAvail, Greater<decimal0>>>>>>>>>>(this);
			}

			var pars = new List<object>(capacity: 8) { newline.InventoryID, newline.SubItemID, newline.SiteID, newline.CostCenterID };

			if (!string.IsNullOrEmpty(origLotSerialNbr))
			{
				cmd.WhereAnd<Where<INLotSerialStatusByCostCenter.lotSerialNbr, Equal<Required<INLotSerialStatusByCostCenter.lotSerialNbr>>>>();
				pars.Add(origLotSerialNbr);
			}

			AppendFiltersForStatusSelect(newline, cmd, pars);

			LineSplittingExt.AppendSerialStatusCmdOrderBy(cmd, newline, lotserclass);

			return cmd.Select(pars.ToArray()).AsEnumerable().Cast<PXResult>().ToList();
		}

		protected virtual void AppendFiltersForStatusSelect<TStatus>(SOShipLine line, PXSelectBase<TStatus> select, List<object> parameters)
			where TStatus : class, IBqlTable, new()
		{
			if (line.ProjectID != null && line.TaskID != null)
			{
				select.WhereAnd<Where<INLocation.projectID, IsNull, Or<INLocation.projectID, Equal<Required<INLocation.projectID>>>>>();
				parameters.Add(line.ProjectID);
			}

			if (IsSyncUnassignedScope && line.LocationID != null)
			{
				select.WhereAnd<Where<INLocation.locationID, Equal<Required<INLocation.locationID>>>>();
				parameters.Add(line.LocationID);
			}
		}

		public virtual decimal? CreateSplitsForAvailableLotsImpl<L, LA, S, SA>(
			decimal? PlannedQty, string origPlanType, string origLotSerialNbr,
			SOShipLine newline, INLotSerClass lotserclass)
			where L : class, IStatus, ILotSerial, IBqlTable, new()
			where LA : class, IStatus, ILotSerial, IBqlTable, new()
			where S : class, IStatus, IBqlTable, new()
			where SA : class, IStatus, IBqlTable, new()
		{
			if (lotserclass.LotSerTrack == INLotSerTrack.SerialNumbered)
			{
				PlannedQty = Math.Floor((decimal)PlannedQty);
			}

			List<PXResult> resultset = SelectLotSerialStatus(origLotSerialNbr, newline, lotserclass);
			ResortStockForShipment(newline, resultset);

			PXCache lcache = Caches[typeof(L)];
			PXCache scache = Caches[typeof(S)];
			PXCache tcache = Caches[typeof(INSiteLotSerial)];

			bool isFullLineAllocation = (PlannedQty >= newline.BaseShippedQty);
			int locCounter = 0;
			int? assignedLocation = null;
			int? assignedTaskID = null;
			if (string.IsNullOrEmpty(origLotSerialNbr))
			{
				foreach (PXResult<L, INLocation, S, INSiteLotSerial> available in resultset)
				{
					var location = (INLocation)available;
					if (locCounter > 0 && newline.TaskID != null && assignedTaskID != location.TaskID)
					{
						continue;
					}

					L avail = (L)available;
					INSiteLotSerial siteLotAvail = (INSiteLotSerial)available;

					LA accumavail = new LA();
					lcache.RestoreCopy(accumavail, avail);

					SiteLotSerial accumSiteLotAvail = new SiteLotSerial();
					tcache.RestoreCopy(accumSiteLotAvail, siteLotAvail);

					accumSiteLotAvail = (SiteLotSerial)this.Caches[typeof(SiteLotSerial)].Insert(accumSiteLotAvail);

					accumavail = (LA)this.Caches[typeof(LA)].Insert(accumavail);

					S siteavail = (S)available;
					SA accumsiteavail = new SA();
					scache.RestoreCopy(accumsiteavail, siteavail);
					accumsiteavail = (SA)this.Caches[typeof(SA)].Insert(accumsiteavail);

					decimal? AvailableQty = 0m;

					decimal? SiteLotAvailableQty = siteLotAvail.QtyHardAvail + accumSiteLotAvail.QtyHardAvail;
					decimal? StatusAvailableQty = avail.QtyHardAvail + accumavail.QtyHardAvail;
					decimal? SiteAvailableQty = siteavail.QtyHardAvail + accumsiteavail.QtyHardAvail;

					//We should not check INSiteStatus for allocated lines
					if (!origPlanType.IsIn(INPlanConstants.Plan61, INPlanConstants.Plan63, INPlanConstants.PlanM7))
					{
						AvailableQty = Math.Min(SiteAvailableQty.GetValueOrDefault(), Math.Min(SiteLotAvailableQty.GetValueOrDefault(), StatusAvailableQty.GetValueOrDefault()));
					}
					else
					{
						AvailableQty = Math.Min(SiteLotAvailableQty.GetValueOrDefault(), StatusAvailableQty.GetValueOrDefault());
					}

					if (AvailableQty <= 0m)
					{
						continue;
					}

					IBqlTable newsplit = (newline.IsUnassigned == true) ? (IBqlTable)newline.ToUnassignedSplit() : (SOShipLineSplit)newline;
					PXCache cache = (newline.IsUnassigned == true) ? unassignedSplits.Cache : splits.Cache;

					cache.SetValue<SOShipLineSplit.uOM>(newsplit, null);
					cache.SetValue<SOShipLineSplit.splitLineNbr>(newsplit, null);
					cache.SetValue<SOShipLineSplit.locationID>(newsplit, avail.LocationID);
					cache.SetValue<SOShipLineSplit.lotSerialNbr>(newsplit, newline.IsUnassigned == true ? string.Empty : avail.LotSerialNbr);
					cache.SetValue<SOShipLineSplit.expireDate>(newsplit, avail.ExpireDate);
					cache.SetValue<SOShipLineSplit.isUnassigned>(newsplit, newline.IsUnassigned);
					cache.SetValue<SOShipLineSplit.qty>(newsplit, (AvailableQty < PlannedQty) ? AvailableQty : PlannedQty);
					cache.SetValue<SOShipLineSplit.baseQty>(newsplit, null);
					cache.Insert(newsplit);

					if (locCounter == 0)
					{
						if (newline.TaskID != null)
							assignedTaskID = location.TaskID;
						assignedLocation = location.LocationID;
					}
					else if (assignedLocation != location.LocationID)
					{
						assignedLocation = null;
					}
					locCounter++;

					if (AvailableQty < PlannedQty)
					{
						PlannedQty -= AvailableQty;
					}
					else
					{
						PlannedQty = 0m;
						break;
					}
				}
			}
			else
			{
				foreach (PXResult<L, INLocation, S> available in resultset)
				{
					var location = (INLocation)available;
					if (locCounter > 0 && newline.TaskID != null && assignedTaskID != location.TaskID)
					{
						continue;
					}

					L avail = (L)available;
					LA accumavail = new LA();
					lcache.RestoreCopy(accumavail, avail);

					S siteavail = (S)available;
					SA accumsiteavail = new SA();
					scache.RestoreCopy(accumsiteavail, siteavail);

					accumavail = (LA)this.Caches[typeof(LA)].Insert(accumavail);
					accumsiteavail = (SA)this.Caches[typeof(SA)].Insert(accumsiteavail);

					decimal? AvailableQty = avail.QtyHardAvail + accumavail.QtyHardAvail;
					decimal? SiteAvailableQty = siteavail.QtyHardAvail + accumsiteavail.QtyHardAvail;

					//We should not check INSiteStatus for allocated lines
					AvailableQty = (SiteAvailableQty < AvailableQty && !origPlanType.IsIn(INPlanConstants.Plan61, INPlanConstants.Plan63)) ? SiteAvailableQty : AvailableQty;

					if (AvailableQty <= 0m)
					{
						continue;
					}

					IBqlTable newsplit = (newline.IsUnassigned == true) ? (IBqlTable)newline.ToUnassignedSplit() : (SOShipLineSplit)newline;
					PXCache cache = (newline.IsUnassigned == true) ? unassignedSplits.Cache : splits.Cache;

					cache.SetValue<SOShipLineSplit.uOM>(newsplit, null);
					cache.SetValue<SOShipLineSplit.splitLineNbr>(newsplit, null);
					cache.SetValue<SOShipLineSplit.locationID>(newsplit, avail.LocationID);
					cache.SetValue<SOShipLineSplit.lotSerialNbr>(newsplit, avail.LotSerialNbr);
					cache.SetValue<SOShipLineSplit.expireDate>(newsplit, avail.ExpireDate);
					cache.SetValue<SOShipLineSplit.isUnassigned>(newsplit, newline.IsUnassigned);
					cache.SetValue<SOShipLineSplit.qty>(newsplit, (AvailableQty < PlannedQty) ? AvailableQty : PlannedQty);
					cache.SetValue<SOShipLineSplit.baseQty>(newsplit, null);
					cache.Insert(newsplit);

					if (locCounter == 0)
					{
						if (newline.TaskID != null)
							assignedTaskID = location.TaskID;
						assignedLocation = location.LocationID;
					}
					else if (assignedLocation != location.LocationID)
					{
						assignedLocation = null;
					}
					locCounter++;

					if (AvailableQty < PlannedQty)
					{
						PlannedQty -= AvailableQty;
					}
					else
					{
						PlannedQty = 0m;
						break;
					}
				}
			}

			if (newline.IsUnassigned == true && isFullLineAllocation && assignedLocation != null)
			{
				/// for assigned lines the location is set by <see cref="LSSOShipLine"/>
				this.Transactions.Cache.SetValue<SOShipLine.locationID>(newline, assignedLocation);
			}

			return PlannedQty;
		}

		public virtual decimal? ShipAvailableNonLots(SOShipmentPlan plan, SOShipLine newline, INLotSerClass lotserclass)
		{
			return CreateSplitsForAvailableNonLots(plan.PlanQty, plan.PlanType, newline, lotserclass);
		}

		public virtual decimal? CreateSplitsForAvailableNonLots(
			decimal? PlannedQty, string origPlanType,
			SOShipLine newline, INLotSerClass lotserclass)
		{
			return CreateSplitsForAvailableNonLotsImpl<
				INLocationStatusByCostCenter,
				LocationStatusByCostCenter,
				INSiteStatusByCostCenter,
				SiteStatusByCostCenter>(PlannedQty, origPlanType, newline, lotserclass);
		}

		protected virtual List<PXResult> SelectLocationStatus(SOShipLine newline)
		{
			var select = new PXSelectReadonly2<INLocationStatusByCostCenter,
				InnerJoin<INLocation, On<INLocationStatusByCostCenter.FK.Location>,
				LeftJoin<INSiteStatusByCostCenter, On<INSiteStatusByCostCenter.inventoryID, Equal<INLocationStatusByCostCenter.inventoryID>,
					And<INSiteStatusByCostCenter.subItemID, Equal<INLocationStatusByCostCenter.subItemID>,
					And<INSiteStatusByCostCenter.siteID, Equal<INLocationStatusByCostCenter.siteID>,
					And<INSiteStatusByCostCenter.costCenterID, Equal<INLocationStatusByCostCenter.costCenterID>>>>>>>,
				Where<INLocationStatusByCostCenter.inventoryID, Equal<Required<INLocationStatusByCostCenter.inventoryID>>,
					And<INLocationStatusByCostCenter.siteID, Equal<Required<INLocationStatusByCostCenter.siteID>>,
					And<INLocationStatusByCostCenter.costCenterID, Equal<Required<INLocationStatusByCostCenter.costCenterID>>,
					And<INLocation.salesValid, Equal<boolTrue>,
					And<INLocation.inclQtyAvail, Equal<boolTrue>>>>>>,
				OrderBy<Asc<INLocation.pickPriority, Asc<INLocation.locationCD>>>>(this);

			var pars = new List<object>(capacity: 8) { newline.InventoryID, newline.SiteID, newline.CostCenterID };
			if (PXAccess.FeatureInstalled<FeaturesSet.subItem>())
			{
				select.WhereAnd<Where<INLocationStatusByCostCenter.subItemID, Equal<Required<INLocationStatusByCostCenter.subItemID>>>>();
				pars.Add(newline.SubItemID);
			}

			AppendFiltersForStatusSelect(newline, select, pars);

			return select.Select(pars.ToArray()).AsEnumerable().Cast<PXResult>().ToList();
		}

		public virtual decimal? CreateSplitsForAvailableNonLotsImpl<L,LA, S, SA>(
			decimal? PlannedQty, string origPlanType,
			SOShipLine newline, INLotSerClass lotserclass)
			where L : class, IStatus, IBqlTable, new()
			where LA : class, IStatus, IBqlTable, new()
			where S : class, IStatus, IBqlTable, new()
			where SA : class, IStatus, IBqlTable, new()
		{
			List<PXResult> resultset = SelectLocationStatus(newline);
			ResortStockForShipment(newline, resultset);

			bool isFullLineAllocation = (PlannedQty >= newline.BaseShippedQty);
			int locCounter = 0;
			int? assignedLocation = null;
			int? assignedTaskID = null;
			PXCache lcache = Caches[typeof(L)];
			PXCache scache = Caches[typeof(S)];
			foreach (PXResult<L, INLocation, S> available in resultset)
			{
				var location = (INLocation)available;
				if (locCounter > 0 && newline.TaskID != null && assignedTaskID != location.TaskID)
				{
					continue;
				}

				L avail = (L) available;
				LA accumavail = new LA();
				lcache.RestoreCopy(accumavail, avail);

				S siteavail = (S)available;
				SA accumsiteavail = new SA();
				scache.RestoreCopy(accumsiteavail, siteavail);

				accumavail = (LA)this.Caches[typeof(LA)].Insert(accumavail);
				accumsiteavail = (SA)this.Caches[typeof(SA)].Insert(accumsiteavail);

				decimal? AvailableQty = avail.QtyHardAvail + accumavail.QtyHardAvail;
				decimal? SiteAvailableQty = siteavail.QtyHardAvail + accumsiteavail.QtyHardAvail;

				//We should not check INSiteStatus for allocated lines
				AvailableQty = (SiteAvailableQty < AvailableQty && !origPlanType.IsIn(INPlanConstants.Plan61, INPlanConstants.Plan63)) ? SiteAvailableQty : AvailableQty;

				if (AvailableQty <= 0m)
				{
					continue;
				}

				InsertSplitsForNonLotsOnLocation(newline, lotserclass, location.LocationID, AvailableQty, PlannedQty);

				if (locCounter == 0)
				{
					if (newline.TaskID != null)
						assignedTaskID = location.TaskID;
					assignedLocation = location.LocationID;
				}
				else if (assignedLocation != location.LocationID)
				{
					assignedLocation = null;
				}
				locCounter++;

				if (AvailableQty < PlannedQty)
				{
					PlannedQty -= AvailableQty;
				}
				else
				{
					PlannedQty = 0m;
					break;
				}
			}

			if (PlannedQty > 0m && (lotserclass.LotSerTrack == INLotSerTrack.NotNumbered || lotserclass.LotSerAssign == INLotSerAssign.WhenUsed))
			{
				InventoryItem item = InventoryItem.PK.Find(this, newline.InventoryID);
				if (item?.NegQty == true)
				{
					SOOrderType orderType = soordertype.Select(newline.OrigOrderType);
					if (orderType?.ShipFullIfNegQtyAllowed == true)
					{
						int? locationID = GetLocationIDForNotAvailableStock(item, newline.SiteID);
						if (locationID == null)
						{
							throw new PXException(Messages.NegShipmentCantBeCreatedLocationNotSetup, item.InventoryCD);
						}

						bool addNegQtyLocation = true;
						if (locCounter > 0)
						{
							INLocation location = INLocation.PK.Find(this, locationID);
							addNegQtyLocation = (location?.TaskID == assignedTaskID);
						}

						if (addNegQtyLocation)
						{
							InsertSplitsForNonLotsOnLocation(newline, lotserclass, locationID, PlannedQty, PlannedQty);
							PlannedQty = 0m;
							if (locCounter == 0)
							{
								assignedLocation = locationID;
							}
							else if (assignedLocation != locationID)
							{
								assignedLocation = null;
							}
						}
					}
				}
			}

			if (newline.IsUnassigned == true && isFullLineAllocation && assignedLocation != null)
			{
				/// for assigned lines the location is set by <see cref="LSSOShipLine"/>
				this.Transactions.Cache.SetValue<SOShipLine.locationID>(newline, assignedLocation);
			}

			return PlannedQty;
		}

		protected virtual void ResortStockForShipment(SOShipLine newline, List<PXResult> resultset)
		{
			ResortStockForShipmentByDefaultItemLocation(newline, resultset);
			ResortStockForShipmentByProjectAndTask(newline, resultset);
		}

		protected virtual void ResortStockForShipmentByDefaultItemLocation(SOShipLine newline, List<PXResult> resultset)
		{
			if (INSite.PK.Find(this, newline.SiteID)?.UseItemDefaultLocationForPicking != true)
				return;

			var dfltShipLocationID = INItemSite.PK.Find(this, newline.InventoryID, newline.SiteID)?.DfltShipLocationID;
			if (dfltShipLocationID == null)
				return;
			
			var listOrderedByDfltShipLocationID = resultset.OrderByDescending(
				r => PXResult.Unwrap<INLocation>(r).LocationID == dfltShipLocationID).ToList();
			resultset.Clear();
			resultset.AddRange(listOrderedByDfltShipLocationID);
		}

		protected virtual void ResortStockForShipmentByProjectAndTask(SOShipLine newline, List<PXResult> resultset)
		{
			if (newline.ProjectID == null || newline.TaskID == null)
				return;

			int capacity = resultset.Count;
			var first = new List<PXResult>(capacity);//matching ProjectID and TaskID
			var second = new List<PXResult>(capacity);//matching ProjectID, TaskID not specified
			var third = new List<PXResult>(capacity);//ProjectID and TaskID not specified
			var forth = new List<PXResult>(capacity);//matching ProjectID, different TaskID

			foreach (PXResult available in resultset)
			{
				INLocation location = PXResult.Unwrap<INLocation>(available);
				if (location.ProjectID != null && location.ProjectID == newline.ProjectID && location.TaskID == newline.TaskID)
				{
					first.Add(available);
				}
				else if (location.ProjectID != null && location.ProjectID == newline.ProjectID && location.TaskID == null)
				{
					second.Add(available);
				}
				else if (location.ProjectID == null && location.TaskID == null)
				{
					third.Add(available);
				}
				else if (location.ProjectID != null && location.ProjectID == newline.ProjectID && location.TaskID != null)
				{
					forth.Add(available);
				}
			}

			resultset.Clear();
			resultset.AddRange(first);
			resultset.AddRange(second);
			resultset.AddRange(third);
			resultset.AddRange(forth);
		}


		public virtual void InsertSplitsForNonLotsOnLocation(SOShipLine newline, INLotSerClass lotserclass, int? locationID, decimal? availableQty, decimal? plannedQty)
        {
            IBqlTable newsplit = (newline.IsUnassigned == true) ? (IBqlTable)newline.ToUnassignedSplit() : (SOShipLineSplit)newline;
            PXCache cache = (newline.IsUnassigned == true) ? unassignedSplits.Cache : splits.Cache;

			cache.SetValue<SOShipLineSplit.uOM>(newsplit, null);
			cache.SetValue<SOShipLineSplit.splitLineNbr>(newsplit, null);
			cache.SetValue<SOShipLineSplit.locationID>(newsplit, locationID);
            cache.SetValue<SOShipLineSplit.isUnassigned>(newsplit, newline.IsUnassigned);
            if (newline.IsUnassigned == true)
            {
                cache.SetValue<SOShipLineSplit.lotSerialNbr>(newsplit, string.Empty);
            }

            if (newline.IsClone == false)
            {
                PXParentAttribute.SetParent(cache, newsplit, typeof(SOShipLine), newline);
            }

            decimal? qtyAllocate = (availableQty < plannedQty) ? availableQty : plannedQty;
            if (lotserclass.LotSerTrack == INLotSerTrack.SerialNumbered &&
                (lotserclass.LotSerAssign != INLotSerAssign.WhenUsed
                || newline.ShipmentType != SOShipmentType.Transfer && newline.IsIntercompany != true))
            {
				cache.SetValue<SOShipLineSplit.baseQty>(newsplit, 1m);
                cache.SetValue<SOShipLineSplit.qty>(newsplit, 1m);

                for (int i = 0; i < (int)qtyAllocate; i++)
                {
                    cache.Insert(newsplit);
                }
            }
            else
            {
				cache.SetValue<SOShipLineSplit.qty>(newsplit, qtyAllocate);
                cache.SetValue<SOShipLineSplit.baseQty>(newsplit, null);
                cache.Insert(newsplit);
            }
        }

		public virtual int? GetLocationIDForNotAvailableStock(InventoryItem item, int? siteID)
		{
			var itemSite = (INItemSite)PXSelectReadonly<INItemSite,
				Where<INItemSite.inventoryID, Equal<Required<INItemSite.inventoryID>>,
					And<INItemSite.siteID, Equal<Required<INItemSite.siteID>>>>>
				.Select(this, item.InventoryID, siteID);
			if (itemSite?.DfltShipLocationID != null)
			{
				return itemSite.DfltShipLocationID;
			}

			var site = INSite.PK.Find(this, siteID);
			InventoryItemCurySettings itemCurySettings = InventoryItemCurySettings.PK.Find(this, item.InventoryID, site?.BaseCuryID);

			if (itemCurySettings?.DfltSiteID == siteID && itemCurySettings.DfltShipLocationID != null)
			{
				return itemCurySettings.DfltShipLocationID;
			}

			return site?.ShipLocationID;
		}

		public virtual decimal? ShipNonStock(SOShipmentPlan plan, SOShipLine newline)
		{
			decimal? PlannedQty = plan.PlanQty;

			SOShipLineSplit newsplit = (SOShipLineSplit)newline;
			newsplit.UOM = null;
			newsplit.SplitLineNbr = null;
			newsplit.LocationID = INSite.PK.Find(this, newsplit.SiteID)?.NonStockPickingLocationID;
			newsplit.Qty = PlannedQty;
			newsplit.BaseQty = null;
			splits.Insert(newsplit);

			return 0m;
		}

		public virtual decimal? ShipAvailable(SOShipmentPlan plan, SOShipLine newline, PXResult<InventoryItem, INLotSerClass> item)
		{
			INLotSerClass lotserclass = item;
			InventoryItem initem = item;

			if (initem.StkItem == false && initem.KitItem == true)
			{
				decimal? kitqty = plan.PlanQty;
				object lastComponentID = null;
				bool HasSerialComponents = false;
				SOShipLine copy;

				ShipNonStockKit(plan, newline, ref kitqty, ref lastComponentID, ref HasSerialComponents);

                bool hassplits = false;
                foreach(SOShipLineSplit split in splits.Cache.Inserted)
                {
                    if (split.ShipmentNbr == newline.ShipmentNbr && split.LineNbr == newline.LineNbr)
                    {
                        hassplits = true;
                        break;
                    }
                }

                if (!hassplits)
                {
                    RemoveLineFromShipment(newline, true);
                    return 0m;
                }
                
				copy = PXCache<SOShipLine>.CreateCopy(newline);
				copy.ShippedQty = (copy.UOM == copy.OrderUOM && kitqty == copy.BaseFullOrderQty) ? copy.FullOrderQty
					: INUnitAttribute.ConvertFromBase<SOShipLine.inventoryID>(Transactions.Cache, copy, copy.UOM, (decimal)kitqty, INPrecision.QUANTITY, INMidpointRounding.FLOOR);
				LineSplittingExt.LastComponentID = (int?)lastComponentID;
				try
				{
					Transactions.Update(copy);
				}
				finally
				{
					LineSplittingExt.LastComponentID = null;
				}

				return 0m;
			}
			else if (lotserclass == null || lotserclass.LotSerTrack == null)
			{
				return ShipNonStock(plan, newline);
			}
			else if (lotserclass.LotSerTrack == INLotSerTrack.NotNumbered || lotserclass.LotSerAssign == INLotSerAssign.WhenUsed || newline.IsUnassigned == true)
			{
				return ShipAvailableNonLots(plan, newline, lotserclass);
			}
			else
			{
				return ShipAvailableLots(plan, newline, lotserclass);
			}
		}

		public virtual void ReceiveLotSerial(SOShipmentPlan plan, SOShipLine newline, SOLineSplit soSplit, PXResult<InventoryItem, INLotSerClass> item)
		{
			PXSelectBase<INLotSerialStatusByCostCenter> cmd = new PXSelectReadonly2<INLotSerialStatusByCostCenter,
				InnerJoin<INLocation,
					On<INLotSerialStatusByCostCenter.FK.Location>>,
				Where<INLotSerialStatusByCostCenter.inventoryID, Equal<Required<INLotSerialStatusByCostCenter.inventoryID>>,
					And<INLotSerialStatusByCostCenter.subItemID, Equal<Required<INLotSerialStatusByCostCenter.subItemID>>,
					And<INLotSerialStatusByCostCenter.siteID, Equal<Required<INLotSerialStatusByCostCenter.siteID>>,
					And<INLotSerialStatusByCostCenter.costCenterID, Equal<Required<INLotSerialStatusByCostCenter.costCenterID>>,
					And<INLocation.salesValid, Equal<boolTrue>>>>>>>(this);

			if (!string.IsNullOrEmpty(plan.LotSerialNbr))
			{
				cmd.WhereAnd<Where<INLotSerialStatusByCostCenter.lotSerialNbr, Equal<Required<INLotSerialStatusByCostCenter.lotSerialNbr>>>>();
			}

			INLotSerialStatusByCostCenter avail = cmd.SelectWindowed(0, 1, newline.InventoryID, newline.SubItemID, newline.SiteID, newline.CostCenterID, plan.LotSerialNbr);

			SOShipLineSplit newsplit = (SOShipLineSplit)newline;
			newsplit.UOM = null;
			newsplit.Qty = newsplit.BaseQty;
			newsplit.SplitLineNbr = null;
			if (avail != null)
			{
				if (newsplit.LocationID == null)
					newsplit.LocationID = avail.LocationID;
				newsplit.LotSerialNbr = avail.LotSerialNbr;
				newsplit.ExpireDate = soSplit?.ExpireDate ?? avail.ExpireDate;
			}
			else
			{
				INSite site = INSite.PK.Find(this, newline.SiteID);
				newsplit.LocationID = site.ReturnLocationID;
				newsplit.LotSerialNbr = plan.LotSerialNbr;
				newsplit.ExpireDate = soSplit?.ExpireDate ?? newline.ExpireDate;
			}

			if (plan.LotSerialNbr != null)
				splits.Update(newsplit);
		}

        public virtual void PromptReplenishment(PXCache sender, SOShipLine newline, InventoryItem item, SOShipmentPlan plan)
		{
			if (newline.ProjectID != null && newline.TaskID != null)
			{
				// we can't prompt replenishment reliably for lines assigned to project and task
				return;
			}

            decimal planrequired = (plan.PlanQty ?? 0m) - newline.BaseShippedQty.GetValueOrDefault();
            decimal qtyrequired = planrequired;

            SOLine soLine = PXSelect<SOLine, Where<SOLine.orderType, Equal<Required<SOLine.orderType>>,
            And<SOLine.orderNbr, Equal<Required<SOLine.orderNbr>>,
            And<SOLine.lineNbr, Equal<Required<SOLine.lineNbr>>>>>>.Select(this, newline.OrigOrderType, newline.OrigOrderNbr, newline.OrigLineNbr);

            if (item.StkItem == false && item.KitItem == true)
            {
                if (soLine.ShipComplete != SOShipComplete.ShipComplete)
                {
                    //if it's not shipcomplete than we must check if we can assemble at least one non-stock kit
                    qtyrequired = 1;
            }

                List<InventoryItem> itemsNotAvailable = new List<InventoryItem>();
                decimal? maxPromptQty = null;

				foreach (PXResult<INKitSpecStkDet, InventoryItem> compres in PXSelectJoin<INKitSpecStkDet,
					InnerJoin<InventoryItem, On<InventoryItem.inventoryID, Equal<INKitSpecStkDet.compInventoryID>>>,
					Where<INKitSpecStkDet.kitInventoryID, Equal<Required<INKitSpecStkDet.kitInventoryID>>>>.Select(this, newline.InventoryID))
				{
                    INKitSpecStkDet spec = (INKitSpecStkDet)compres;

					if (spec.DfltCompQty.GetValueOrDefault() == 0)
						continue;

                    Tuple<decimal, decimal> availability = CalculateItemAvailability(spec.CompInventoryID, spec.CompSubItemID, newline.SiteID, newline.CostCenterID);

                    if ((qtyrequired * spec.DfltCompQty) > availability.Item1)
                    {
                        //actually it's a error, but it will be thrown further
                        return;
                    }
                    else
					{
                        decimal possibleQty = Math.Floor(availability.Item1 / spec.DfltCompQty.Value);
                        if (maxPromptQty == null || possibleQty < maxPromptQty)
                            maxPromptQty = possibleQty;
                    }
                    }
                if (maxPromptQty <= 0m)
                    return;

                foreach (PXResult<INKitSpecStkDet, InventoryItem> compres in PXSelectJoin<INKitSpecStkDet,
                  InnerJoin<InventoryItem, On<InventoryItem.inventoryID, Equal<INKitSpecStkDet.compInventoryID>>>,
                  Where<INKitSpecStkDet.kitInventoryID, Equal<Required<INKitSpecStkDet.kitInventoryID>>>>.Select(this, newline.InventoryID))
                {
                    INKitSpecStkDet spec = (INKitSpecStkDet)compres;
					if (spec.DfltCompQty.GetValueOrDefault() == 0)
						continue;

                    Tuple<decimal, decimal> availability = CalculateItemAvailability(spec.CompInventoryID, spec.CompSubItemID, newline.SiteID, newline.CostCenterID);

                    if (availability.Item2 < (maxPromptQty * spec.DfltCompQty))
                        itemsNotAvailable.Add((InventoryItem)compres);
					}

                if (itemsNotAvailable.Count == 0)
                    return;

                StringBuilder invetoryCDs = new StringBuilder(itemsNotAvailable[0].InventoryCD);
                for (int i = 1; i < itemsNotAvailable.Count; i++)
                {
                    invetoryCDs.Append(", " + itemsNotAvailable[i].InventoryCD);
				}

                throw new PXException(Messages.PromptReplenishment, invetoryCDs);
			}
			else
			{
                Tuple<decimal, decimal> availability = CalculateItemAvailability(newline.InventoryID, newline.SubItemID, newline.SiteID, newline.CostCenterID);

                if (soLine.ShipComplete != SOShipComplete.ShipComplete)
                {
                    //if it's not shipcomplete than we must throw error if we can ship at least smthing more
                    qtyrequired = 0m;
                }

                //actually it's a error, but it will be thrown further
                if (qtyrequired > availability.Item1)
                    return;

                if (availability.Item1 > 0m)
					throw new PXException(Messages.PromptReplenishment, sender.GetValueExt<SOShipLine.inventoryID>(newline));
				}
			}

        private Tuple<decimal, decimal> CalculateItemAvailability(int? inventoryID, int? subItemID, int? siteID, int? costCenterID)
        {
            decimal totalAvalableQty = 0;
            decimal totalAvalableForSalesQty = 0;

			INSiteStatusByCostCenter sitestatus = PXSelectReadonly<INSiteStatusByCostCenter,
				Where<INSiteStatusByCostCenter.inventoryID, Equal<Required<INSiteStatusByCostCenter.inventoryID>>,
				And<INSiteStatusByCostCenter.siteID, Equal<Required<INSiteStatusByCostCenter.siteID>>,
				And<INSiteStatusByCostCenter.costCenterID, Equal<Required<INSiteStatusByCostCenter.costCenterID>>,
				And<Where<INSiteStatusByCostCenter.subItemID, Equal<Required<INSiteStatusByCostCenter.subItemID>>,
					Or<Required<INSiteStatusByCostCenter.subItemID>, IsNull>>>>>>>
				.SelectSingleBound(this, new object[] { }, inventoryID, siteID, costCenterID, subItemID, subItemID);

			// AC-71766: Correction is required to consider items allocated in Sales Order but without created shipment yet.
			// This items are considered in SiteStatus but not considered in LocationStatus yet.
			decimal allocatedcorrection = 0m;
			if (sitestatus != null)
			{
				allocatedcorrection = -1 * (
					(sitestatus.QtySOShipping ?? 0m) +
					(sitestatus.QtyFSSrvOrdAllocated ?? 0m) +
					(sitestatus.QtyProductionAllocated ?? 0m));
			}

			var select = new PXSelectReadonly2<INLocationStatusByCostCenter,
				InnerJoin<INLocation, On<INLocationStatusByCostCenter.FK.Location>>,
				Where<INLocationStatusByCostCenter.inventoryID, Equal<Required<INLocationStatusByCostCenter.inventoryID>>,
					And<INLocationStatusByCostCenter.siteID, Equal<Required<INLocationStatusByCostCenter.siteID>>,
					And<INLocationStatusByCostCenter.costCenterID, Equal<Required<INLocationStatusByCostCenter.costCenterID>>,
					And<INLocation.inclQtyAvail, Equal<boolTrue>>>>>,
				OrderBy<Asc<INLocation.pickPriority>>>(this);

			object[] pars;
			if (PXAccess.FeatureInstalled<FeaturesSet.subItem>())
			{
				select.WhereAnd<Where<INLocationStatusByCostCenter.subItemID, Equal<Required<INLocationStatusByCostCenter.subItemID>>>>();
				pars = new object[] { inventoryID, siteID, costCenterID, subItemID };
			}
			else
			{
				pars = new object[] { inventoryID, siteID, costCenterID };
			}
			PXResultset<INLocationStatusByCostCenter> resultset = select.Select(pars);

			foreach (PXResult<INLocationStatusByCostCenter, INLocation> res in resultset)
            {
                INLocation loc = (INLocation)res;
				INLocationStatusByCostCenter avail = (INLocationStatusByCostCenter)res;
				LocationStatusByCostCenter accumavail = new LocationStatusByCostCenter();
				PXCache<INLocationStatusByCostCenter>.RestoreCopy(accumavail, avail);
				accumavail = (LocationStatusByCostCenter)this.Caches[typeof(LocationStatusByCostCenter)].Insert(accumavail);

                allocatedcorrection += 
					(avail.QtySOShipping ?? 0m) +
					(avail.QtyFSSrvOrdAllocated ?? 0m) +
					(avail.QtyProductionAllocated ?? 0m);
                decimal qtyAvailable = avail.QtyHardAvail.GetValueOrDefault() + accumavail.QtyHardAvail.GetValueOrDefault();
                totalAvalableQty += qtyAvailable;
                if (loc.SalesValid == true)
                {
                    totalAvalableForSalesQty += qtyAvailable;
                }
            }

            return new Tuple<decimal, decimal>(totalAvalableQty + allocatedcorrection, totalAvalableForSalesQty + allocatedcorrection);
		}

		public virtual void ShipNonStockKit(SOShipmentPlan plan, SOShipLine newline, ref decimal? kitqty, ref object lastComponentID, ref bool HasSerialComponents)
		{
			SOShipLine copy;
		    object lastSubitemID = null;

			using (LineSplittingExt.KitProcessingScope(InventoryItem.PK.Find(this, newline.InventoryID)))
			{
				foreach (PXResult<INKitSpecStkDet, InventoryItem, INLotSerClass> compres in
					PXSelectJoin<INKitSpecStkDet,
					InnerJoin<InventoryItem,
						On<INKitSpecStkDet.FK.ComponentInventoryItem>,
					InnerJoin<INLotSerClass,
						On<InventoryItem.FK.LotSerialClass>>>,
					Where<INKitSpecStkDet.kitInventoryID, Equal<Required<INKitSpecStkDet.kitInventoryID>>>>.Select(this, newline.InventoryID))
				{
					INKitSpecStkDet compitem = (INKitSpecStkDet)compres;
					InventoryItem component = (InventoryItem)compres;

					if (component.ItemStatus == INItemStatus.Inactive)
					{
						throw new PXException(Messages.KitComponentIsInactive, component.InventoryCD);
					}
					copy = LineSplittingExt.Clone(newline);

					copy.IsStockItem = true;
					copy.InventoryID = compitem.CompInventoryID;
					copy.SubItemID = compitem.CompSubItemID;
					copy.UOM = compitem.UOM;
					copy.Qty = compitem.DfltCompQty * plan.PlanQty;

					//clear splits with correct ComponentID
					LineSplittingExt.RaiseRowDeleted(copy);

					SOShipmentPlan plancopy = PXCache<SOShipmentPlan>.CreateCopy(plan);
					plancopy.PlanQty = INUnitAttribute.ConvertToBase<SOShipLine.inventoryID>(Transactions.Cache, copy, copy.UOM, (decimal)copy.Qty, INPrecision.QUANTITY);
					if (copy.Operation == SOOperation.Receipt)
					{
						INSite site = INSite.PK.Find(this, copy.SiteID);
						if (site != null)
						{
							if (site.ReturnLocationID == null)
								throw new PXException(Messages.NoRMALocation, site.SiteCD);

							if (((INLotSerClass)compres).LotSerTrack == INLotSerTrack.SerialNumbered)
							{
								for ( int i = 0; i < copy.Qty; i++ )
								{
									SOShipLineSplit newsplit = (SOShipLineSplit)copy;
									newsplit.Qty = 1;
									newsplit.SplitLineNbr = null;
									newsplit.LocationID = site.ReturnLocationID;
									newsplit = splits.Insert(newsplit);
									PXDefaultAttribute.SetPersistingCheck<SOShipLineSplit.lotSerialNbr>(splits.Cache, newsplit, PXPersistingCheck.Nothing);
									PXDefaultAttribute.SetPersistingCheck<SOShipLineSplit.expireDate>(splits.Cache, newsplit, PXPersistingCheck.Nothing);
								}
							}
							else
							{
								SOShipLineSplit newsplit = (SOShipLineSplit)copy;
								newsplit.SplitLineNbr = null;
								newsplit.LocationID = site.ReturnLocationID;
								newsplit = splits.Insert(newsplit);
								PXDefaultAttribute.SetPersistingCheck<SOShipLineSplit.lotSerialNbr>(splits.Cache, newsplit, PXPersistingCheck.Nothing);
								PXDefaultAttribute.SetPersistingCheck<SOShipLineSplit.expireDate>(splits.Cache, newsplit, PXPersistingCheck.Nothing);
							}
						}
					}
					else
					{
						decimal? unshippedqty = ShipAvailable(plancopy, copy, new PXResult<InventoryItem, INLotSerClass>(compres, compres));

						if (plancopy.PlanQty != 0m && (plancopy.PlanQty - unshippedqty) * plan.PlanQty / plancopy.PlanQty < kitqty)
						{
							kitqty = (plancopy.PlanQty - unshippedqty) * plan.PlanQty / plancopy.PlanQty;
							lastComponentID = copy.InventoryID;
							lastSubitemID = copy.SubItemID;

						}
					}
					HasSerialComponents |= ((INLotSerClass)compres).LotSerTrack == INLotSerTrack.SerialNumbered;
				}
			}

			foreach (PXResult<INKitSpecNonStkDet, InventoryItem> compres in PXSelectJoin<INKitSpecNonStkDet,
				InnerJoin<InventoryItem, On<INKitSpecNonStkDet.FK.ComponentInventoryItem>>,
				Where<INKitSpecNonStkDet.kitInventoryID, Equal<Required<INKitSpecNonStkDet.kitInventoryID>>,
					And<Where<InventoryItem.kitItem, Equal<True>, Or<InventoryItem.nonStockShip, Equal<True>>>>>>.Select(this, newline.InventoryID))
			{
				INKitSpecNonStkDet compitem = compres;
				InventoryItem item = compres;

				copy = LineSplittingExt.Clone(newline);

				copy.IsStockItem = false;
				copy.InventoryID = compitem.CompInventoryID;
				copy.SubItemID = null;
				copy.UOM = compitem.UOM;
				copy.Qty = compitem.DfltCompQty * plan.PlanQty;

				//clear splits with correct ComponentID
				LineSplittingExt.RaiseRowDeleted(copy);

				SOShipmentPlan plancopy = PXCache<SOShipmentPlan>.CreateCopy(plan);
				plancopy.PlanQty = INUnitAttribute.ConvertToBase<SOShipLine.inventoryID>(Transactions.Cache, copy, copy.UOM, (decimal)copy.Qty, INPrecision.QUANTITY);

				if (item.StkItem == false && item.KitItem == true)
				{
					decimal? subkitqty = plancopy.PlanQty;

					ShipNonStockKit(plancopy, copy, ref subkitqty, ref lastComponentID, ref HasSerialComponents);

					if (plancopy.PlanQty != 0m && subkitqty * plan.PlanQty / plancopy.PlanQty < kitqty)
					{
						kitqty = subkitqty * plan.PlanQty / plancopy.PlanQty;
					}
				}
				else
				{
					ShipAvailable(plancopy, copy, new PXResult<InventoryItem, INLotSerClass>(compres, null));
				}
			}

			if (HasSerialComponents)
			{
				kitqty = decimal.Floor((decimal)kitqty);
			}

			if (kitqty <= 0m
				&& lastComponentID != null)
			{
				object lastComponentCD = lastComponentID;
			    object lastSubitemCD = lastSubitemID;

				Transactions.Cache.RaiseFieldSelecting<SOShipLine.inventoryID>(newline, ref lastComponentCD, true);
			    Transactions.Cache.RaiseFieldSelecting<SOShipLine.subItemID>(newline, ref lastSubitemCD, true);

			    if (PXAccess.FeatureInstalled<FeaturesSet.subItem>() && lastSubitemID != null)
			    {
                    PXTrace.WriteInformation(Messages.ItemWithSubitemNotAvailableTraced, lastComponentCD, Transactions.GetValueExt<SOShipLine.siteID>(newline), lastSubitemCD);
                }
			    else
			    {
                    PXTrace.WriteInformation(Messages.ItemNotAvailableTraced, lastComponentCD, Transactions.GetValueExt<SOShipLine.siteID>(newline));
                }
			}
		}

		public virtual bool RemoveLineFromShipment(SOShipLine shipline, bool RemoveFlag)
		{
			if (RemoveFlag)
			{
				if (shipline.CostCenterID != CostCenter.FreeStock && INCostCenter.PK.Find(this, shipline.CostCenterID)?.CostLayerType == CostLayerType.Special)
				{
					PXTrace.WriteInformation(Messages.ItemNotAvailableTraced_Special, Transactions.GetValueExt<SOShipLine.inventoryID>(shipline), Transactions.GetValueExt<SOShipLine.siteID>(shipline));
				}
				else if (PXAccess.FeatureInstalled<FeaturesSet.subItem>() && shipline != null && shipline.SubItemID != null)
                {
                    PXTrace.WriteInformation(Messages.ItemWithSubitemNotAvailableTraced, Transactions.GetValueExt<SOShipLine.inventoryID>(shipline), Transactions.GetValueExt<SOShipLine.siteID>(shipline), Transactions.GetValueExt<SOShipLine.subItemID>(shipline));
                }
                else
                {
                    PXTrace.WriteInformation(Messages.ItemNotAvailableTraced, Transactions.GetValueExt<SOShipLine.inventoryID>(shipline), Transactions.GetValueExt<SOShipLine.siteID>(shipline));
                }
				shipline.KeepManualFreight = true;
				Transactions.Delete(shipline);
				return true;
			}

			Transactions.Cache.RaiseExceptionHandling<SOShipLine.shippedQty>(shipline, null, new PXSetPropertyException(Messages.ItemNotAvailable, PXErrorLevel.RowWarning));
			return false;
		}

		public virtual bool CreateShipmentFromSchedules(PXResult<SOShipmentPlan, SOLineSplit, SOLine, InventoryItem, INLotSerClass, INSite, SOShipLine> res, SOShipLine newline, SOOrderType ordertype, string operation, DocumentList<SOShipment> list)
		{
			bool deleted = false;

			SOShipmentPlan plan = res;
			SOLine line = res;
			SOLineSplit linesplit = res;
			INSite site = res;
			InventoryItem item = res;

			if (plan.Selected == true || list != null && ((plan.RequireAllocation == false || linesplit.Operation == SOOperation.Receipt) || plan.InclQtySOShipping != 0 || plan.InclQtySOShipped != 0 || linesplit.LineType == SOLineType.NonInventory))
			{
				ValidateLineBeforeShipment(line);

				newline.OrigOrderType = line.OrderType;
				newline.OrigOrderNbr = line.OrderNbr;
				newline.OrigLineNbr = line.LineNbr;
				newline.OrigPlanType = (linesplit.POCreate != true && linesplit.IsAllocated != true) ? linesplit.PlanType: plan.PlanType;
				newline.IsStockItem = line.IsStockItem;
				newline.InventoryID = line.InventoryID;
				newline.SubItemID = line.SubItemID;
				newline.SiteID = line.SiteID;
				newline.TranDesc = line.TranDesc;
				newline.CustomerID = line.CustomerID;
				newline.InvtMult = line.OrderQty < 0m ? (short?)-line.InvtMult : line.InvtMult;
				newline.SOLineSign = line.LineSign;
				newline.Operation = line.Operation;
				newline.LineType = line.LineType;
				newline.ReasonCode = line.ReasonCode;
				newline.ProjectID = line.ProjectID;
				newline.TaskID = line.TaskID;
				newline.CostCodeID = line.CostCodeID;
				newline.UOM = line.UOM;
				newline.IsFree = line.IsFree;
				newline.ManualDisc = line.ManualDisc;

				newline.DiscountID = line.DiscountID;
				newline.DiscountSequenceID = line.DiscountSequenceID;

				newline.AlternateID = line.AlternateID;
				newline.BlanketType = line.BlanketType;
				newline.BlanketNbr = line.BlanketNbr;
				newline.BlanketLineNbr = line.BlanketLineNbr;
				newline.BlanketSplitLineNbr = line.BlanketSplitLineNbr;

				newline.IsSpecialOrder = line.IsSpecialOrder;
				newline.CostCenterID = line.CostCenterID;

				UpdateOrigValues(newline, line, plan.PlanQty);

				ValidateLineType(line, item, Messages.CannotCreateShipmentNonInventoryNonStockKit);

				INLotSerClass lotSerClass = (INLotSerClass)res;
				bool isNonStock = lotSerClass.LotSerTrack == null;
				if (isNonStock)
				{
					newline.ShippedQty = (newline.UOM == newline.OrderUOM && plan.PlanQty == newline.BaseFullOrderQty) ? newline.FullOrderQty
						: INUnitAttribute.ConvertFromBase(Transactions.Cache, newline.InventoryID, newline.UOM, (decimal)plan.PlanQty, INPrecision.QUANTITY);
					newline = LineSplittingExt.InsertWithoutSplits(newline);

					try
					{
						ShipAvailable(plan, newline, new PXResult<InventoryItem, INLotSerClass>(res, res));
					}
					catch (PXException ex)
					{
						LineSplittingExt.lsselect.Delete(newline);
						throw ex;
					}
				}
				else if (operation == SOOperation.Receipt)
				{
					newline.ShippedQty = (newline.UOM == newline.OrderUOM && plan.PlanQty == newline.BaseFullOrderQty) ? newline.FullOrderQty
						: INUnitAttribute.ConvertFromBase(Transactions.Cache, newline.InventoryID, newline.UOM, (decimal)plan.PlanQty, INPrecision.QUANTITY);
					newline.LocationID = site.ReturnLocationID;
					if (newline.LocationID == null && list != null)
						throw new PXException(Messages.NoRMALocation, site.SiteCD);
					newline = Transactions.Insert(newline);
					ReceiveLotSerial(plan, newline, linesplit, new PXResult<InventoryItem, INLotSerClass>(res, res));
				}
				else
				{
					SOShipLine existing = (SOShipLine)Transactions.Cache.Locate(newline);
					if (existing == null || Transactions.Cache.GetStatus(existing).IsIn(PXEntryStatus.Deleted, PXEntryStatus.InsertedDeleted))
					{
						newline.ShippedQty = 0m;
						newline = LineSplittingExt.InsertWithoutSplits(newline);
					}
					newline.IsUnassigned = lotSerClass.IsManualAssignRequired == true && plan.PlanQty > 0 && string.IsNullOrEmpty(plan.LotSerialNbr)
						&& (lotSerClass.LotSerAssign != INLotSerAssign.WhenUsed || newline.ShipmentType != SOShipmentType.Transfer && newline.IsIntercompany != true);

					decimal? notShipped = ShipAvailable(plan, newline, new PXResult<InventoryItem, INLotSerClass>(res, res));
					if (newline.IsUnassigned == true)
					{
						var oldRow = (SOShipLine)Transactions.Cache.CreateCopy(newline);
						newline.UnassignedQty = plan.PlanQty - notShipped;
						newline.BaseShippedQty = plan.PlanQty - notShipped;
						newline.ShippedQty = (newline.UOM == newline.OrderUOM && newline.BaseShippedQty == newline.BaseFullOrderQty) ? newline.FullOrderQty
							: INUnitAttribute.ConvertFromBase(unassignedSplits.Cache, newline.InventoryID, newline.UOM, (decimal)newline.BaseShippedQty, INPrecision.QUANTITY);

						using (LineSplittingExt.SuppressedModeScope(true))
						{
							Transactions.Cache.RaiseFieldUpdated<SOShipLine.shippedQty>(newline, oldRow.ShippedQty);
							Transactions.Cache.RaiseRowUpdated(newline, oldRow);
						}
					}
				}

				if (newline.BaseShippedQty < plan.PlanQty && string.IsNullOrEmpty(plan.LotSerialNbr))
				{
					PromptReplenishment(Transactions.Cache, newline, (InventoryItem)res, plan);
				}

				PXNoteAttribute.CopyNoteAndFiles(Caches[typeof(SOLine)], line, Caches[typeof(SOShipLine)], newline, ordertype.CopyLineNotesToShipment, ordertype.CopyLineFilesToShipment);

				if (newline.ShippedQty == 0m)
				{
					deleted = RemoveLineFromShipment(newline, list != null && sosetup.Current.AddAllToShipment == false);
				}

				if (newline.BaseShippedQty < plan.PlanQty * line.CompleteQtyMin / 100m && line.ShipComplete == SOShipComplete.ShipComplete)
				{
					deleted = RemoveLineFromShipment(newline, list != null);
				}

				if (!deleted && plan.PlanType != linesplit.PlanType && linesplit.POCreate != true && linesplit.IsAllocated != true)
				{
					INItemPlan actualPlan = PXSelect<INItemPlan, Where<INItemPlan.planID, Equal<Required<INItemPlan.planID>>>>.Select(this, plan.PlanID);
					if (actualPlan != null)
					{
						actualPlan.PlanType = linesplit.PlanType;
						Caches[typeof(INItemPlan)].Update(actualPlan);
					}
				}
			}
			return deleted;
		}

		protected virtual bool ValidateLineBeforeShipment(SOLine line) => true;

		public PXSelectJoin<SOShipmentPlan,
						InnerJoin<SOLineSplit, On<SOLineSplit.planID, Equal<SOShipmentPlan.planID>>,
						InnerJoin<SOLine, On<SOLine.orderType, Equal<SOLineSplit.orderType>, And<SOLine.orderNbr, Equal<SOLineSplit.orderNbr>, And<SOLine.lineNbr, Equal<SOLineSplit.lineNbr>>>>,
						InnerJoin<InventoryItem, On<InventoryItem.inventoryID, Equal<SOShipmentPlan.inventoryID>>,
						LeftJoin<INLotSerClass,
							On<InventoryItem.FK.LotSerialClass>,
						LeftJoin<INSite,
							On<SOLine.FK.Site>>>>>>,
						Where<SOShipmentPlan.siteID, Equal<Optional<SOOrderFilter.siteID>>,
						And<SOShipmentPlan.planDate, LessEqual<Optional<SOOrderFilter.endDate>>,
						And<SOShipmentPlan.orderType, Equal<Required<SOOrder.orderType>>,
						And<SOShipmentPlan.orderNbr, Equal<Required<SOOrder.orderNbr>>,
						And2<Where<SOLine.lineNbr, Equal<Required<SOLine.lineNbr>>, Or<Required<SOLine.lineNbr>, IsNull>>,
						And<SOLine.operation, Equal<Required<SOLine.operation>>,
							And<Not<Exists<Select<SOShipLine,
								Where<SOShipLine.origOrderType, Equal<SOLineSplit.orderType>,
								And<SOShipLine.origOrderNbr, Equal<SOLineSplit.orderNbr>,
								And<SOShipLine.origLineNbr, Equal<SOLineSplit.lineNbr>,
								And<SOShipLine.origSplitLineNbr, Equal<SOLineSplit.splitLineNbr>,
								And<Where<SOShipLine.shipmentNbr, IsNull,
									Or<SOShipLine.shipmentNbr, NotEqual<Current<SOShipment.shipmentNbr>>>>>>>>>>>>>>>>>>>,
						OrderBy<Asc<SOLineSplit.orderType, 
							Asc<SOLineSplit.orderNbr, 
							Asc<SOLineSplit.lineNbr, 
							Asc<SOLineSplit.splitLineNbr>>>>>> ShipmentScheduleSelect;

		public virtual void CreateShipment(CreateShipmentArgs args)
		{
			SOOrder order = args.Order;
			SiteLotSerial.AccumulatorAttribute.ForceAvailQtyValidation(this, true);
			ItemLotSerial.AccumulatorAttribute.ForceAvailQtyValidation(this, true);
			SOOrderType ordertype = soordertype.Select(order.OrderType);
			SOShipment newdoc;
			string operation = args.Operation ?? ordertype.DefaultOperation;
			SOOrderTypeOperation orderoperation = SOOrderTypeOperation.PK.Find(this, order.OrderType, operation);
			DateTime? shipDate = args.ShipDate;

			if (args.QuickProcessFlow != PXQuickProcess.ActionFlow.NoFlow)
				sosetup.Current.HoldShipments = false;

			if (orderoperation != null && orderoperation.Active == true && String.IsNullOrEmpty(orderoperation.ShipmentPlanType))
            {
                object state = this.Caches<SOOrderTypeOperation>().GetStateExt<SOOrderTypeOperation.operation>(orderoperation);
                throw new PXException(Messages.ShipmentPlanTypeNotSetup, order.OrderType, state);
            }

			if (args.UseOptimalShipDate == true)
			{
				SOShipmentPlan plan =
					order.ShipComplete == SOShipComplete.BackOrderAllowed
						? PXSelectJoinGroupBy<SOShipmentPlan,
						  	InnerJoin<SOLineSplit, On<SOLineSplit.planID, Equal<SOShipmentPlan.planID>>>,
							Where<SOShipmentPlan.siteID, Equal<Required<SOOrderFilter.siteID>>,
						  	And<SOShipmentPlan.orderType, Equal<Required<SOOrder.orderType>>,
						  	And<SOShipmentPlan.orderNbr, Equal<Required<SOOrder.orderNbr>>,
						  	And<SOLineSplit.operation, Equal<Required<SOLineSplit.operation>>>>>>,
						  Aggregate<Min<SOShipmentPlan.planDate>>>.Select(this, args.SiteID, order.OrderType, order.OrderNbr, operation)
						: PXSelectJoinGroupBy<SOShipmentPlan,
						  	InnerJoin<SOLineSplit, On<SOLineSplit.planID, Equal<SOShipmentPlan.planID>>>,
						  Where<SOShipmentPlan.siteID, Equal<Required<SOOrderFilter.siteID>>,
						  	And<SOShipmentPlan.orderType, Equal<Required<SOOrder.orderType>>,
						  	And<SOShipmentPlan.orderNbr, Equal<Required<SOOrder.orderNbr>>,
						  	And<SOLineSplit.operation, Equal<Required<SOLineSplit.operation>>>>>>,
						  Aggregate<Max<SOShipmentPlan.planDate>>>.Select(this, args.SiteID, order.OrderType, order.OrderNbr, operation);

				if (plan.PlanDate > shipDate)
					shipDate = plan.PlanDate;
			}
			bool addOrder = (args.ShipmentList == null);
			if (!addOrder)
			{
				this.Clear();

				order = ActualizeAndValidateOrder(args.Graph, order, operation);

				if (order.ShipSeparately == false)
				{
					newdoc = args.ShipmentList.Find
						<SOShipment.customerID, SOShipment.shipDate, SOShipment.shipAddressID, SOShipment.shipContactID,
							SOShipment.siteID, SOShipment.fOBPoint, SOShipment.shipVia, SOShipment.shipTermsID, SOShipment.shipZoneID, SOShipment.createARDoc,
								SOShipment.useCustomerAccount, SOShipment.shipmentType, SOShipment.freightAmountSource, SOShipment.hidden, SOShipment.isManualPackage>
						(order.CustomerID, shipDate, order.ShipAddressID, order.ShipContactID,
							args.SiteID, order.FOBPoint, order.ShipVia, order.ShipTermsID, order.ShipZoneID, order.ARDocType != ARDocType.NoUpdate,
								order.UseCustomerAccount, INTranType.DocType(orderoperation.INDocType), order.FreightAmountSource, false, order.IsManualPackage)
						?? new SOShipment();
				}
				else
				{
					newdoc = new SOShipment();
					newdoc.Hidden = true;
				}

				bool newlyCreated = (newdoc.ShipmentNbr == null);
				if (newlyCreated)
				{
					newdoc = Document.Insert(newdoc);
				}
				else
				{
					Document.Current = Document.Search<SOShipment.shipmentNbr>(newdoc.ShipmentNbr);
				}

				bool updatedFromOrder = SetShipmentFieldsFromOrder(order, newdoc, args.SiteID, shipDate, operation, orderoperation, newlyCreated);

				if (!newlyCreated && args.MassProcess && OrderList.Select<SOOrderShipment>().Count() == 1)
				{
					newdoc.ShipmentDesc = PXMessages.LocalizeNoPrefix(Messages.MultiOrderShipment);
					updatedFromOrder = true;
				}

				if (updatedFromOrder)
				{
					newdoc = Document.Update(newdoc);
				}
				if (newlyCreated)
				{
					SetShipAddressAndContact(newdoc, order.ShipAddressID, order.ShipContactID);

					newdoc = Document.Update(newdoc);
					newdoc = Document.Search<SOShipment.shipmentNbr>(newdoc.ShipmentNbr);
				}
			}
			else
			{
				newdoc = PXCache<SOShipment>.CreateCopy(Document.Current);

				bool newlyCreated = (newdoc.OrderCntr == 0);
				bool updatedFromOrder = SetShipmentFieldsFromOrder(order, newdoc, args.SiteID, shipDate, operation, orderoperation, newlyCreated);
				if (newlyCreated)
				{
					SetShipAddressAndContact(newdoc, order.ShipAddressID, order.ShipContactID);
					
				}
				if (updatedFromOrder)
				{
					newdoc = Document.Update(newdoc);
				}
			}

			if (order.OpenShipmentCntr > 0)
			{
				SOOrderShipment openShipment = PXSelectReadonly<SOOrderShipment,
					Where<SOOrderShipment.orderType, Equal<Required<SOOrderShipment.orderType>>,
					And<SOOrderShipment.orderNbr, Equal<Required<SOOrderShipment.orderNbr>>,
					And<SOOrderShipment.siteID, Equal<Required<SOOrderShipment.siteID>>,
					And<SOOrderShipment.shipmentNbr, NotEqual<Required<SOOrderShipment.shipmentNbr>>,
					And<SOOrderShipment.confirmed, Equal<boolFalse>>>>>>>.Select(this, order.OrderType, order.OrderNbr, args.SiteID, newdoc.ShipmentNbr);
				if (openShipment != null)
				{
					throw new PXException(Messages.OrderHasOpenShipment, order.OrderType, order.OrderNbr, openShipment.ShipmentNbr);
				}
			}

			CopyOrderHeaderNoteAndFiles(order, Document.Current, ordertype);

			var neworder = new SOOrderShipment
			{
				OrderType = order.OrderType,
				OrderNbr = order.OrderNbr,
				OrderNoteID = order.NoteID,
				ShipmentNbr = Document.Current.ShipmentNbr,
				ShipmentType = Document.Current.ShipmentType,
				ShippingRefNoteID = Document.Current.NoteID,
				Operation = Document.Current.Operation,
				ProjectID = order.ProjectID
			};

			soorder.Cache.Hold(order);
			PXParentAttribute.SetParent(OrderList.Cache, neworder, typeof(SOOrder), order);

			var orderlist = OrderListSimple.Select().ToList();
			var located = OrderList.Locate(neworder);
			var newOrderShipment = false;

			if (located == null || OrderList.Cache.GetStatus(located).IsIn(PXEntryStatus.Deleted, PXEntryStatus.InsertedDeleted))
			{
				neworder = OrderList.Insert(located ?? neworder);
				newOrderShipment = true;
			}
			else
				neworder = located;

			PXRowDeleting SOOrderShipment_RowDeleting = delegate (PXCache sender, PXRowDeletingEventArgs e)
			{
				e.Cancel = true;
			};

			this.RowDeleting.AddHandler<SOOrderShipment>(SOOrderShipment_RowDeleting);

			bool anyDeleted = false;
			var lineships = new Dictionary<SOLine2, LineShipment>();
			void SOShipLine_RowDeleted(PXCache sender, PXRowDeletedEventArgs e)
			{
				var row = (SOShipLine)e.Row;
				var keys = new SOLine2
			{
					OrderType = row.OrigOrderType,
					OrderNbr = row.OrigOrderNbr,
					LineNbr = row.OrigLineNbr
			};
				var cached = (SOLine2)soline.Cache.Locate(keys);

				if (lineships.TryGetValue(cached, out LineShipment lineship))
					lineship.AnyDeleted = true;

				anyDeleted = true;
			}

			this.RowDeleted.AddHandler<SOShipLine>(SOShipLine_RowDeleted);

			foreach (SOLine2 sl in PXSelect<SOLine2,
				Where<SOLine2.orderType, Equal<Required<SOLine2.orderType>>, And<SOLine2.orderNbr, Equal<Required<SOLine2.orderNbr>>,
					And<SOLine2.siteID, Equal<Required<SOLine2.siteID>>, And<SOLine2.operation, Equal<Required<SOLine2.operation>>,
					And<SOLine2.completed, NotEqual<True>>>>>>>
				.Select(this, order.OrderType, order.OrderNbr, args.SiteID, operation))
			{
				PXParentAttribute.SetParent(soline.Cache, sl, typeof(SOOrder), order);
			}

			foreach (SOLineSplit2 sl in PXSelect<SOLineSplit2,
				Where<SOLineSplit2.orderType, Equal<Required<SOLineSplit2.orderType>>, And<SOLineSplit2.orderNbr, Equal<Required<SOLineSplit2.orderNbr>>,
					And<SOLineSplit2.siteID, Equal<Required<SOLineSplit2.siteID>>, And<SOLineSplit2.operation, Equal<Required<SOLineSplit2.operation>>,
					And<SOLineSplit2.completed, NotEqual<True>>>>>>>
				.Select(this, order.OrderType, order.OrderNbr, args.SiteID, operation))
			{
				//just place into cache
			}

			foreach (SOShipLine sl in PXSelect<SOShipLine,
				Where<SOShipLine.shipmentType, Equal<Current<SOShipLine.shipmentType>>,
					And<SOShipLine.shipmentNbr, Equal<Current<SOShipLine.shipmentNbr>>>>>
				.Select(this))
			{
				PXParentAttribute.SetParent(Transactions.Cache, sl, typeof(SOOrder), order);
			}

			SOShipLine newline = null;
			skipAdjustFreeItemLines = true;// Free items will still be Adjusted at the end of this method
			bool hasUnallocatedSplits = false;
			List<ShipmentSchedule> schedulesList = new List<ShipmentSchedule>();
			try
			{
				foreach (PXResult<SOShipmentPlan, SOLineSplit, SOLine, InventoryItem, INLotSerClass, INSite> res in
					ShipmentScheduleSelect.Select(args.SiteID, shipDate, order.OrderType, order.OrderNbr, args.OrderLineNbr, args.OrderLineNbr, operation))
				{
					SOShipmentPlan plan = res;
					SOLineSplit split = res;

					if (plan.RequireAllocation == true && split.LineType != SOLineType.NonInventory && split.Operation != SOOperation.Receipt
						&& plan.InclQtySOShipping != 1 && plan.InclQtySOShipped != 1)
					{
						hasUnallocatedSplits = true;
						continue;
					}

					newline = new SOShipLine();
					newline.OrigSplitLineNbr = ((SOLineSplit)res).SplitLineNbr;

					schedulesList.Add(new ShipmentSchedule(new PXResult<SOShipmentPlan, SOLineSplit, SOLine, InventoryItem, INLotSerClass, INSite, SOShipLine>(plan, split, res, res, res, res, new SOShipLine()), newline));
				}

				schedulesList.Sort();

				foreach (ShipmentSchedule ss in schedulesList)
				{
					ss.ShipLine.ShipmentType = Document.Current.ShipmentType;
					ss.ShipLine.ShipmentNbr = Document.Current.ShipmentNbr;
					ss.ShipLine.LineNbr = (int?)PXLineNbrAttribute.NewLineNbr<SOShipLine.lineNbr>(Transactions.Cache, Document.Current);

					PXParentAttribute.SetParent(Transactions.Cache, ss.ShipLine, typeof(SOOrder), order);

					SOLine2 sl = new SOLine2();

					sl.OrderType = ((SOLine)ss.Result).OrderType;
					sl.OrderNbr = ((SOLine)ss.Result).OrderNbr;
					sl.LineNbr = ((SOLine)ss.Result).LineNbr;

					sl = soline.Locate(sl);
					if (sl != null)
					{
						PXParentAttribute.SetParent(Transactions.Cache, ss.ShipLine, typeof(SOLine2), sl);
					}

					LineShipment lineship = lineships.Ensure(sl, () => new LineShipment());
					lineship.Add(ss.ShipLine);

					SOLineSplit2 sp = new SOLineSplit2();
					sp.OrderType = ((SOLineSplit)ss.Result).OrderType;
					sp.OrderNbr = ((SOLineSplit)ss.Result).OrderNbr;
					sp.LineNbr = ((SOLineSplit)ss.Result).LineNbr;
					sp.SplitLineNbr = ((SOLineSplit)ss.Result).SplitLineNbr;

					sp = solinesplit.Locate(sp);
					if (sp != null)
					{
						PXParentAttribute.SetParent(Transactions.Cache, ss.ShipLine, typeof(SOLineSplit2), sp);
					}

					PXParentAttribute.SetParent(Transactions.Cache, ss.ShipLine, typeof(SOOrderShipment), neworder);

					if (args.ShipmentList == null || sl.ShipComplete != SOShipComplete.ShipComplete || lineship.AnyDeleted == false)
					{
						CreateShipmentFromSchedules(ss.Result, ss.ShipLine, ordertype, operation, args.ShipmentList);
					}

					if (args.ShipmentList != null && sl.ShipComplete == SOShipComplete.ShipComplete && lineship.AnyDeleted)
					{
						foreach (SOShipLine shipline in lineship)
						{
							Transactions.Delete(shipline);
						}
						lineship.Clear();
					}
				}

				foreach (KeyValuePair<SOLine2, LineShipment> pair in lineships)
				{
					if (pair.Key.ShipComplete == SOShipComplete.ShipComplete && pair.Key.ShippedQty < pair.Key.OrderQty * pair.Key.CompleteQtyMin / 100m)
					{
						foreach (SOShipLine shipline in pair.Value)
						{
							RemoveLineFromShipment(shipline, args.ShipmentList != null);
						}
					}
				}
			}
			finally
			{
				skipAdjustFreeItemLines = false;
			}

			if (args.QuickProcessFlow != PXQuickProcess.ActionFlow.NoFlow && sosetup.Current.RequireShipmentTotal == true)
				Document.Current.ControlQty = Document.Current.ShipmentQty;

			AllocateGroupFreeItems(order);
			AdjustFreeItemLines();
			UpdateManualFreightCost(Document.Current, order, newOrderShipment);

			this.RowDeleting.RemoveHandler<SOOrderShipment>(SOOrderShipment_RowDeleting);
			this.RowDeleted.RemoveHandler<SOShipLine>(SOShipLine_RowDeleted);

			foreach (SOOrderShipment item in OrderList.Cache.Inserted)
			{
				if (args.ShipmentList == null && item.ShipmentQty == 0m)
				{
					SOShipLine shipline = PXSelect<SOShipLine, Where<SOShipLine.shipmentType, Equal<Required<SOOrderShipment.shipmentType>>, 
						And<SOShipLine.shipmentNbr, Equal<Required<SOOrderShipment.shipmentNbr>>, 
						And<SOShipLine.origOrderType, Equal<Required<SOOrderShipment.orderType>>, 
						And<SOShipLine.origOrderNbr, Equal<Required<SOOrderShipment.orderNbr>>>>>>>.SelectSingleBound(this, null, item.ShipmentType, item.ShipmentNbr, item.OrderType, item.OrderNbr);
					if (shipline == null)
					{
						OrderList.Delete(item);
					}
				}

				try
				{
					if (args.ShipmentList != null && item.LineCntr > 0 && item.ShipmentQty == 0m && sosetup.Current.AddAllToShipment == true && sosetup.Current.CreateZeroShipments != true)
					{
						throw new SOShipmentException(SOShipmentException.ErrorCode.CannotShipTraced, item, Messages.CannotShipTraced, item.OrderType, item.OrderNbr);
					}

					if (args.ShipmentList != null && item.LineCntr == 0)
					{
						if (hasUnallocatedSplits)
						{
							throw new SOShipmentException(SOShipmentException.ErrorCode.NotAllocatedLines, item, Messages.NotAllocatedLines);
						}
						else if (anyDeleted)
						{
							throw new SOShipmentException(SOShipmentException.ErrorCode.CannotShipCompleteTraced, item, Messages.CannotShipCompleteTraced, item.OrderType, item.OrderNbr);
						}
						else if (operation == SOOperation.Issue)
						{
							throw new SOShipmentException(SOShipmentException.ErrorCode.NothingToShipTraced, item, Messages.NothingToShipTraced, item.OrderType, item.OrderNbr, item.ShipDate);
						}
						else
						{
							throw new SOShipmentException(SOShipmentException.ErrorCode.NothingToReceiveTraced, item, Messages.NothingToReceiveTraced, item.OrderType, item.OrderNbr, item.ShipDate);
						}
					}

					if (args.ShipmentList != null && item.ShipComplete == SOShipComplete.ShipComplete)
					{
						foreach (SOLine2 line in PXSelect<SOLine2, Where<SOLine2.orderType, Equal<Required<SOLine2.orderType>>, And<SOLine2.orderNbr, Equal<Required<SOLine2.orderNbr>>, And<SOLine2.siteID, Equal<Required<SOLine2.siteID>>, And<SOLine2.operation, Equal<Required<SOLine2.operation>>, And<SOLine2.completed, NotEqual<True>>>>>>>.Select(this, item.OrderType, item.OrderNbr, item.SiteID, item.Operation))
						{
							var original = (SOLine2)this.Caches<SOLine2>().GetOriginal(line);
							if (line.LineType == SOLineType.Inventory && line.ShippedQty - original?.ShippedQty == 0m && DateTime.Compare((DateTime)line.ShipDate, (DateTime)item.ShipDate) <= 0 && line.POSource != INReplenishmentSource.DropShipToOrder)
								throw new SOShipmentException(Messages.CannotShipCompleteTraced, order.OrderType, order.OrderNbr);
						}
					}


				}
				catch (SOShipmentException)
				{
					//decrement OpenShipmentCntr
					UpdateShipmentCntr(OrderList.Cache, item, -1);
					//clear ShipmentDeleted flag
					UpdateShipmentCntr(OrderList.Cache, item, 0);
					throw;
				}
			}

			if (operation == SOOperation.Issue)
				neworder.LinkShipment(Document.Current, this);

			if (args.ShipmentList != null)
			{
				if (OrderList.Cache.Inserted.Count() > 0 || OrderList.SelectWindowed(0, 1) != null)
				{
					Save.Press();

					//obtain modified object back.
					if (soorder.Locate(order) is SOOrder cached)
					{
						bool? selected = args.Order.Selected;
						PXCache<SOOrder>.RestoreCopy(args.Order, cached);
						args.Order.Selected = selected;
					}

					if (args.ShipmentList.Find(Document.Current) is null)
						args.ShipmentList.Add(Document.Current);
				}
			}
			ItemLotSerial.AccumulatorAttribute.ForceAvailQtyValidation(this, false);
			SiteLotSerial.AccumulatorAttribute.ForceAvailQtyValidation(this, false);
		}

		/// <summary>
		/// Copies notes and files from Sales Order to Shipment
		/// </summary>
		public virtual void CopyOrderHeaderNoteAndFiles(SOOrder srcOrder, SOShipment dstShipment, SOOrderType orderType)
		{
			bool copyNote = PXNoteAttribute.GetNote(Document.Cache, dstShipment) == null ? (orderType.CopyHeaderNotesToShipment ?? false) : false;
			PXNoteAttribute.CopyNoteAndFiles(Caches[typeof(SOOrder)], srcOrder, Document.Cache, dstShipment, copyNote, orderType.CopyHeaderFilesToShipment);
		}

		protected virtual SOOrder ActualizeAndValidateOrder(SOOrderEntry orderEntry, SOOrder order, string operation)
		{
			order = soorder.Select(order.OrderType, order.OrderNbr);
			if (orderEntry == null) return order;

			bool? isWorkflowActionEnabled = (operation == SOOperation.Receipt)
				? WorkflowAction.HasWorkflowActionEnabled(orderEntry, g => g.createShipmentReceipt, order)
				: WorkflowAction.HasWorkflowActionEnabled(orderEntry, g => g.createShipmentIssue, order);
			if (isWorkflowActionEnabled == false)
			{
				var action = (operation == SOOperation.Receipt) ? orderEntry.createShipmentReceipt : orderEntry.createShipmentIssue;
				throw new PXInvalidOperationException(Messages.ActionNotAvailableInCurrentState,
					action.GetCaption(), soorder.Cache.GetRowDescription(order));
			}

			return order;
		}

		public virtual bool SetShipmentFieldsFromOrder(SOOrder order, SOShipment shipment,
			int? siteID, DateTime? shipDate, string operation, SOOrderTypeOperation orderOperation,
			bool newlyCreated)
		{
			if (newlyCreated)
			{
				// unconditionally copy fields from the first added order only
				shipment.SiteID = siteID;
				shipment.ShipmentType = INTranType.DocType(orderOperation.INDocType);
				shipment.Operation = operation;
				shipment.ShipDate = shipDate;

				shipment.CustomerID = order.CustomerID;
				shipment.CustomerLocationID = order.CustomerLocationID;
				shipment.UseCustomerAccount = order.UseCustomerAccount;
				shipment.CustomerOrderNbr = order.CustomerOrderNbr;
				shipment.Resedential = order.Resedential;
				shipment.SaturdayDelivery = order.SaturdayDelivery;
				shipment.Insurance = order.Insurance;
				shipment.GroundCollect = order.GroundCollect;
				shipment.FOBPoint = order.FOBPoint;
				shipment.ShipTermsID = order.ShipTermsID;
				shipment.ShipVia = order.ShipVia;
				shipment.ShipZoneID = order.ShipZoneID;
				shipment.TaxCategoryID = order.FreightTaxCategoryID;
				shipment.DestinationSiteID = order.DestinationSiteID;
				shipment.FreightAmountSource = order.FreightAmountSource;
				shipment.IsManualPackage = order.IsManualPackage;

				if(string.IsNullOrEmpty(shipment.ShipmentDesc))
					shipment.ShipmentDesc = order.OrderDesc;

				return true;
			}
			else
			{
				if (shipment.FreightAmountSource != order.FreightAmountSource)
				{
					// double check that we don't mix orders with different Freight Amount Source
					throw new PXException();
				}

				if (OrderList.Select<SOOrderShipment>().Count() == 1)
				{
					SOOrder firstOrder = (PXResult<SOOrderShipment, SOOrder, CurrencyInfo, SOAddress, SOContact>)OrderList.Select();

					if (firstOrder.OrderNbr != order.OrderNbr || firstOrder.OrderType != order.OrderType)
					{
						if (!string.IsNullOrEmpty(shipment.CustomerOrderNbr))
						{
							// If we have several Orders within shipment we can't fill CustomerOrderNbr.
							shipment.CustomerOrderNbr = null;
							return true;
						}
					}
				}

				return false;
			}
		}

		public virtual void SetShipAddressAndContact(SOShipment shipment, int? shipAddressID, int? shipContactID)
		{
			foreach (SOShipmentAddress address in this.Shipping_Address.Select())
			{
				if (address.AddressID < 0)
				{
					Shipping_Address.Delete(address);
				}
			}

			foreach (SOShipmentContact contact in this.Shipping_Contact.Select())
			{
				if (contact.ContactID < 0)
				{
					Shipping_Contact.Delete(contact);
				}
			}

			shipment.ShipAddressID = shipAddressID;
			shipment.ShipContactID = shipContactID;
		}

        public virtual void CorrectShipment(SOOrderEntry docgraph, SOShipment shiporder)
        {
            this.Clear();

            Document.Current = Document.Search<SOShipment.shipmentNbr>(shiporder.ShipmentNbr);
			if (WorkflowAction.HasWorkflowActionEnabled(this, g => g.correctShipmentAction, Document.Current) == false)
			{
				throw new PXInvalidOperationException(Messages.ActionNotAvailableInCurrentState,
					correctShipmentAction.GetCaption(), Document.Cache.GetRowDescription(Document.Current));
			}
			MarkOpen(Document.Current);

			Document.Cache.MarkUpdated(Document.Current, assertError: true);
            Document.Cache.IsDirty = true;
            ItemAvailabilityExt.AdvancedCheck = false;

            PXFormulaAttribute.SetAggregate<SOLine.openLine>(docgraph.Transactions.Cache, null, typeof(SOOrder.openLineCntr));

            using (PXTransactionScope ts = new PXTransactionScope())
			using (docgraph.LineSplittingExt.SuppressedModeScope(true))
            {
				var shipLinesClearedSOAllocation = new HashSet<int?>();

                foreach (PXResult<SOOrderShipment, SOOrder, CurrencyInfo, SOAddress, SOContact> ordres in OrderList.Select())
                {
                    SOOrderShipment order = ordres;
					SOOrder soorder = ordres;
                    var ordertype = SOOrderType.PK.Find(this, order.OrderType);

                    if (!string.IsNullOrEmpty(order.InvoiceNbr) && ordertype.ARDocType != ARDocType.NoUpdate || !string.IsNullOrEmpty(order.InvtRefNbr))
                    {
                        throw new PXException(Messages.ShipmentInvoicedCannotReopen, order.OrderType, order.OrderNbr);
                    }
                    if (soorder.Cancelled == true)
                    {
                        throw new PXException(Messages.ShipmentCancelledCannotReopen, order.OrderType, order.OrderNbr);
                    }


                    docgraph.Clear();

                    docgraph.Document.Current = docgraph.Document.Search<SOOrder.orderNbr>(order.OrderNbr, order.OrderType);
                    docgraph.Document.Current.OpenShipmentCntr++;
                    docgraph.Document.Current.Completed = false;
                    docgraph.Document.Cache.MarkUpdated(docgraph.Document.Current, assertError: true);

					var orderSite = docgraph.OrderSite.SelectSingle(order.OrderType, order.OrderNbr, order.SiteID);
					orderSite.OpenShipmentCntr++;
					orderSite = docgraph.OrderSite.Update(orderSite);

                    docgraph.soordertype.Current.RequireControlTotal = false;
                    docgraph.RecalculateExternalTaxesSync = true;

					order.CreateINDoc = false;
                    order.Confirmed = false;
                    this.OrderList.Cache.Update(order);

                    if (docgraph.Document.Current.OpenShipmentCntr > 1)
                    {
                        foreach (SOOrderShipment shipment2 in PXSelect<SOOrderShipment, Where<SOOrderShipment.orderType, Equal<Current<SOOrderShipment.orderType>>, And<SOOrderShipment.orderNbr, Equal<Current<SOOrderShipment.orderNbr>>, And<SOOrderShipment.siteID, Equal<Current<SOOrderShipment.siteID>>, And<SOOrderShipment.shipmentNbr, NotEqual<Current<SOOrderShipment.shipmentNbr>>, And<SOOrderShipment.shipmentType, NotEqual<SOShipmentType.dropShip>>>>>>>.SelectSingleBound(this, new object[] { order }))
                        {
                            throw new PXException(Messages.ShipmentExistsForSiteCannotReopen, order.OrderType, order.OrderNbr);
                        }
                    }

                    Dictionary<int?, List<INItemPlan>> demand = new Dictionary<int?, List<INItemPlan>>();

                    foreach (PXResult<SOShipLineSplit, INItemPlan> res in PXSelectReadonly2<SOShipLineSplit,
                        InnerJoin<INItemPlan, On<INItemPlan.supplyPlanID, Equal<SOShipLineSplit.planID>>>,
                        Where<SOShipLineSplit.shipmentNbr, Equal<Required<SOOrderShipment.shipmentNbr>>,
						And<SOShipLineSplit.origOrderType, Equal<Required<SOOrderShipment.orderType>>,
						And<SOShipLineSplit.origOrderNbr, Equal<Required<SOOrderShipment.orderNbr>>>>>>.Select(docgraph, order.ShipmentNbr, order.OrderType, order.OrderNbr))
                    {
                        SOShipLineSplit line = res;
                        INItemPlan plan = res;

                        List<INItemPlan> ex;
                        if (!demand.TryGetValue(line.LineNbr, out ex))
                        {
                            demand[line.LineNbr] = ex = new List<INItemPlan>();
                        }
                        ex.Add(plan);
                    }

                    HashSet<int?> toSkipReopen = new HashSet<int?>();
                    var lineOpenQuantities = new Dictionary<int?, (SOLine Line, decimal? BaseOpenQty, decimal? OpenQty)>();
                    SOLine prev_line = null;

                    //no Misc lines will be selected because of SiteID constraint
                    foreach (PXResult<SOLine, SOShipLine> res in
                        PXSelectJoin<SOLine,
                            LeftJoin<SOShipLine,
                                On<SOShipLine.origOrderType, Equal<SOLine.orderType>,
                                And<SOShipLine.origOrderNbr, Equal<SOLine.orderNbr>,
                                And<SOShipLine.origLineNbr, Equal<SOLine.lineNbr>,
                                And<SOShipLine.shipmentNbr, Equal<Current<SOOrderShipment.shipmentNbr>>>>>>>,
                            Where<SOLine.orderType, Equal<Current<SOOrderShipment.orderType>>,
                                And<SOLine.orderNbr, Equal<Current<SOOrderShipment.orderNbr>>,
								And<SOLine.operation, Equal<Current<SOOrderShipment.operation>>,
								And<SOLine.lineType, NotEqual<SOLineType.miscCharge>,
								And <Where<SOLine.siteID, Equal<Current<SOOrderShipment.siteID>>, Or<SOShipLine.shipmentNbr, IsNotNull>>>>>>>>
							.SelectMultiBound(docgraph, new [] { order }))
                    {
                        SOLine line = (SOLine)res;
                        SOShipLine shipline = (SOShipLine)res;

						if (shipline.InventoryID != null)
						{
							var item = InventoryItem.PK.Find(this, shipline.InventoryID);
							if (item?.IsConverted == true && shipline.IsStockItem != null && shipline.IsStockItem != item.StkItem)
								throw new PXException(Messages.CannotCorrectShipmentItemConverted);
						}

						if(shipline.SiteID != null && line.SiteID != shipline.SiteID)
						{
							if (shipline.ShippedQty == 0)
							{
								var shipLineCache = Transactions.Cache;
								shipline = (SOShipLine)shipLineCache.Locate(shipline) ?? shipline;
								shipline.Confirmed = false;
								shipLineCache.MarkUpdated(shipline, assertError: true);
								shipLineCache.IsDirty = true;
							}
							continue;
						}

						bool cancelRemainder =
							(line.ShipComplete == SOShipComplete.CancelRemainder &&
							soorder.ShipComplete == SOShipComplete.CancelRemainder &&
							soorder.SiteCntr == 1);

                        if (shipline.ShipmentNbr == null &&
							(line.Completed == false || line.LineSign * line.ShippedQty > 0m || (line.ShipDate > order.ShipDate && !cancelRemainder)))
                        {
                            toSkipReopen.Add(line.LineNbr);
                            continue;
                        }

						bool lineSwitched = (prev_line == null || prev_line.LineNbr != line.LineNbr);
						prev_line = docgraph.CorrectSingleLine(line, shipline, lineSwitched, lineOpenQuantities);
                    }

                    decimal? UnallocatedQty = 0m;
                    SOLineSplit prev_split = null;

                    PXResultset<SOLineSplit> allocations = PXSelectJoin<SOLineSplit,
                        LeftJoin<SOShipLine, On<SOShipLine.origOrderType, Equal<SOLineSplit.orderType>,
                            And<SOShipLine.origOrderNbr, Equal<SOLineSplit.orderNbr>,
                            And<SOShipLine.origLineNbr, Equal<SOLineSplit.lineNbr>,
                            And<SOShipLine.origSplitLineNbr, Equal<SOLineSplit.splitLineNbr>,
                            And<SOShipLine.shipmentNbr, Equal<Current<SOOrderShipment.shipmentNbr>>>>>>>>,
                        Where<SOLineSplit.orderType, Equal<Current<SOOrderShipment.orderType>>,
                            And<SOLineSplit.orderNbr, Equal<Current<SOOrderShipment.orderNbr>>,
                            And<SOLineSplit.siteID, Equal<Current<SOOrderShipment.siteID>>,
                            And<SOLineSplit.operation, Equal<Current<SOOrderShipment.operation>>,
							And2<Where<SOLineSplit.pOReceiptNbr, IsNull, // We should not process DropShip splits.
								Or<SOLineSplit.pOSource, NotEqual<INReplenishmentSource.dropShipToOrder>,
								Or<SOLineSplit.pOSource, IsNull>>>,
                            And2<Where<SOLineSplit.fixedSource, Equal<INReplenishmentSource.none>,
                            Or<Where<SOLineSplit.fixedSource, Equal<INReplenishmentSource.purchased>, //marked for PO splits that are not received yet will be reopened 
									And<SOLineSplit.completed, Equal<boolTrue>, And<SOLineSplit.pOCompleted, Equal<boolFalse>,
									And<SOLineSplit.pOCancelled, Equal<boolFalse>, And<SOLineSplit.pONbr, IsNull,
									And<SOLineSplit.pOReceiptNbr, IsNull, And<SOLineSplit.isAllocated, Equal<boolFalse>>>>>>>>>>,
							And<Where<SOLineSplit.shipmentNbr, IsNull, Or<SOLineSplit.shipmentNbr, Equal<Current<SOOrderShipment.shipmentNbr>>>>>>>>>>>,
                        OrderBy<
                            Asc<SOLineSplit.orderType,
                            Asc<SOLineSplit.orderNbr,
                            Asc<SOLineSplit.lineNbr,
                            Desc<SOLineSplit.shipmentNbr,
                            Asc<SOLineSplit.isAllocated,
                            Desc<SOLineSplit.shipDate,
                            Desc<SOLineSplit.pOCreate,
                            Desc<SOLineSplit.splitLineNbr>>>>>>>>>>.SelectMultiBound(docgraph, new object[] { order });

					allocations.ForEach(_ => _.CreateCopy());

					foreach (PXResult<SOLineSplit, SOShipLine> res in allocations)
                    {
                        SOLineSplit split = PXCache<SOLineSplit>.CreateCopy(res);
                        SOShipLine shipline = res;

                        foreach (SOLineSplit sibling in PXParentAttribute.SelectSiblings(docgraph.splits.Cache, split, typeof(SOLine)))
                        {
                            if (sibling.ShipmentNbr != null && split.ShipmentNbr != null && sibling.ParentSplitLineNbr == split.SplitLineNbr)
                            {
                                throw new PXException(Messages.OrderHasSubsequentShipments, split.ShipmentNbr, docgraph.splits.Cache.GetValueExt<SOLineSplit.inventoryID>(split), order.OrderType, order.OrderNbr);
                            }
                        }

                        if (toSkipReopen.Contains(split.LineNbr))
                            continue;

                        if (prev_split == null || prev_split.LineNbr != split.LineNbr)
                        {
                            UnallocatedQty = 0m;

                            (SOLine Line, decimal? BaseOpenQty, decimal? OpenQty) lineQty;
                            if (lineOpenQuantities.TryGetValue(split.LineNbr, out lineQty))
                            {
								if (lineQty.Line.UOM == split.UOM)
									UnallocatedQty = lineQty.OpenQty;
								else
									UnallocatedQty = INUnitAttribute.ConvertFromBase(docgraph.splits.Cache, split.InventoryID, split.UOM, (decimal)lineQty.BaseOpenQty, INPrecision.QUANTITY);
                            }
                        }

                        prev_split = split;
                        if (object.Equals(split.ShipmentNbr, order.ShipmentNbr))
                        {
                            decimal? QtyToAllocate = 0m;

                            if (split.IsAllocated == true)
                            {
								decimal? siteStatusQtyHardAvail = GetQtyHardAvailFromSiteStatus(docgraph, split);

								decimal? PrevAllocatedQty = allocations.AsEnumerable()
									.RowCast<SOLineSplit>()
									.TakeWhile(_ => !docgraph.splits.Cache.ObjectsEqual(_, split))
									.Where(_ => _.InventoryID == split.InventoryID && _.SubItemID == split.SubItemID && _.SiteID == split.SiteID && _.ShipmentNbr == order.ShipmentNbr && _.IsAllocated == true)
                                    .Sum(_ => _.BaseQty);

								decimal? NextAllocatedQty = allocations.AsEnumerable()
									.RowCast<SOLineSplit>()
									.Where(_ => _.ParentSplitLineNbr == split.SplitLineNbr && _.IsAllocated == true)
									.Sum(_ => _.BaseQty);

								decimal? QtyHardAvail = siteStatusQtyHardAvail + PrevAllocatedQty + NextAllocatedQty > 0 ? siteStatusQtyHardAvail + PrevAllocatedQty + NextAllocatedQty : 0;
                                QtyHardAvail = INUnitAttribute.ConvertFromBase(docgraph.splits.Cache, split.InventoryID, split.UOM, (decimal)QtyHardAvail, INPrecision.QUANTITY);

                                QtyToAllocate = Math.Min((decimal)UnallocatedQty, (decimal)QtyHardAvail);
                            }
                            else
                            {
                                QtyToAllocate = UnallocatedQty;
                            }

                            if (QtyToAllocate >= split.Qty - split.ShippedQty)
                            {
                                UnallocatedQty -= split.Qty - split.ShippedQty;
                            }
                            else
                            {
                                UnallocatedQty -= QtyToAllocate;
                                split.Qty = split.ShippedQty + QtyToAllocate;
                            }
                        }
                        else if (split.Qty >= UnallocatedQty)
                        {
                            split.Qty = UnallocatedQty;
                            UnallocatedQty = 0m;
                        }
                        else
                        {
                            UnallocatedQty -= split.Qty;
                        }

						bool shippedSplit = !string.IsNullOrEmpty(split.ShipmentNbr);
                        split.Completed = false;
                        split.ShipmentNbr = null;

						if (split.IsAllocated == true && !string.IsNullOrEmpty(split.LotSerialNbr)
							&& !string.IsNullOrEmpty(shipline.ShipmentNbr)
							&& !string.Equals(split.LotSerialNbr, shipline.LotSerialNbr, StringComparison.InvariantCultureIgnoreCase))
                            {
							// 1. SN1 is allocated in SO#1, then it is changed in Shipment#1 to SN2. Shipment#1 is confirmed.
							// 2. SN1 is allocated or shipped or issued somewhere else.
							// 3. Correct Shipment#1 => trying to allocate both SN1 and SN2 and stuck.
							// To avoid this situation the allocation of SO#1 is cleared.
							INSiteLotSerial status = PXSelectReadonly<INSiteLotSerial, Where<INSiteLotSerial.inventoryID, Equal<Required<INSiteLotSerial.inventoryID>>,
								And<INSiteLotSerial.siteID, Equal<Required<INSiteLotSerial.siteID>>, And<INSiteLotSerial.lotSerialNbr, Equal<Required<INSiteLotSerial.lotSerialNbr>>>>>>
								.Select(this, split.InventoryID, split.SiteID, split.LotSerialNbr);
							if (status == null || status.QtyHardAvail < split.BaseQty)
                                {
                                    split.IsAllocated = false;
                                    split.LotSerialNbr = null;
								shipLinesClearedSOAllocation.Add(shipline.LineNbr);
                            }
                        }

						SOLineSplit deletedSplit = null;
                        if (split.Qty <= 0 && !shippedSplit || docgraph.splits.Cache.GetStatus(split) == PXEntryStatus.Inserted)
                        {
                            docgraph.splits.Delete(split);
							deletedSplit = split;
                            split = null;
                        }
                        else
                        {
                            split = docgraph.splits.Update(split);
                        }

                        //reattach demand from shipment back to SO schedules
                        if (split != null && split.PlanID != null && shipline.LineNbr != null)
                        {
                            List<INItemPlan> scheduledemand;
                            if (demand.TryGetValue(shipline.LineNbr, out scheduledemand))
                            {
                                foreach (INItemPlan item in scheduledemand)
                                {
                                    item.SupplyPlanID = split.PlanID;
                                    docgraph.Caches[typeof(INItemPlan)].MarkUpdated(item, assertError: true);
                                }
                                demand.Remove(shipline.LineNbr);
                            }
                        }
						//reattach demand from deleted back orders back to SO schedules
						if (deletedSplit?.PlanID > 0 && deletedSplit.ParentSplitLineNbr != null)
						{
							SOLineSplit parentSplit = docgraph.splits.Locate(new SOLineSplit
							{
								OrderType = deletedSplit.OrderType,
								OrderNbr = deletedSplit.OrderNbr,
								LineNbr = deletedSplit.LineNbr,
								SplitLineNbr = deletedSplit.ParentSplitLineNbr,
							});
							if (parentSplit != null)
							{
								foreach (INItemPlan plan in SelectFrom<INItemPlan>
									.Where<INItemPlan.supplyPlanID.IsEqual<@P.AsLong>>
									.View.Select(docgraph, deletedSplit.PlanID))
								{
									plan.SupplyPlanID = parentSplit.PlanID;
									docgraph.Caches[typeof(INItemPlan)].MarkUpdated(plan);
								}
							}
						}
                    }

                    SOOrder copy = PXCache<SOOrder>.CreateCopy(docgraph.Document.Current);
                    PXFormulaAttribute.CalcAggregate<SOLine.orderQty>(docgraph.Transactions.Cache, copy);
                    docgraph.Document.Update(copy);

					SOOrder.Events
						.Select(e => e.GotShipmentCorrected)
						.FireOn(docgraph, docgraph.Document.Current);
					docgraph.Save.Press();
                }

                foreach (PXResult<INItemPlan, SOShipLineSplit, SOLineSplit> res in PXSelectJoin<INItemPlan,
                    InnerJoin<SOShipLineSplit, 
						On<SOShipLineSplit.planID, Equal<INItemPlan.planID>>,
					LeftJoin<SOLineSplit,
						On<SOShipLineSplit.FK.OriginalOrderLineSplit>>>,
					Where<SOShipLineSplit.shipmentNbr, Equal<Current<SOShipment.shipmentNbr>>,
						And<SOShipLineSplit.origOrderType, IsNotNull>>>.Select(this))
                {
                    INItemPlan plan = res;
					SOShipLineSplit split = res;
					SOLineSplit soLineSplit = res;
					SOOrderType ordertype = SOOrderType.PK.Find(this, split.OrigOrderType);
					if (ordertype != null)
					{
						split.Confirmed = false;
						if (shipLinesClearedSOAllocation.Contains(split.LineNbr))
						{
							split.OrigPlanType = INPlanConstants.Plan60;
						}
						Caches[typeof(SOShipLineSplit)].MarkUpdated(split, assertError: true);
						Caches[typeof(SOShipLineSplit)].IsDirty = true;

						plan = PXCache<INItemPlan>.CreateCopy(plan);

						plan.PlanType = split.PlanType;
						plan.OrigPlanType = split.OrigPlanType;
						plan.OrigPlanID = soLineSplit?.PlanID;

						this.Caches[typeof(INItemPlan)].Update(plan);
					}
                }

                //this is done to reset BackOrder plans back to Order Plans because SOLinePlanIDAttribute does not initialize plans normally
                foreach (PXResult<SOLineSplit2, SOLine, SOShipLine, INItemPlan> line2 in PXSelectJoin<SOLineSplit2,
                    InnerJoin<SOLine, On<SOLine.orderType, Equal<SOLineSplit2.orderType>, And<SOLine.orderNbr, Equal<SOLineSplit2.orderNbr>, And<SOLine.lineNbr, Equal<SOLineSplit2.lineNbr>>>>,
                    InnerJoin<SOShipLine, On<SOShipLine.origOrderType, Equal<SOLineSplit2.orderType>,
                    And<SOShipLine.origOrderNbr, Equal<SOLineSplit2.orderNbr>,
                    And<SOShipLine.origLineNbr, Equal<SOLineSplit2.lineNbr>,
                    And<SOShipLine.origSplitLineNbr, Equal<SOLineSplit2.splitLineNbr>>>>>,
                    InnerJoin<INItemPlan, On<INItemPlan.planID, Equal<SOLineSplit2.planID>>>>>,
                    Where<SOShipLine.shipmentNbr, Equal<Current<SOShipment.shipmentNbr>>,
                    And<SOShipLine.shipmentType, Equal<Current<SOShipment.shipmentType>>>>>.Select(this))
                {
                    SOLineSplit2 solinesplit2 = (SOLineSplit2)line2;
                    SOLine soline = (SOLine)line2;
                    SOShipLine soshipline = (SOShipLine)line2;
                    INItemPlan plan = (INItemPlan)line2;

                    SOLineSplit2 copy = PXCache<SOLineSplit2>.CreateCopy(solinesplit2);
                    this.Caches[typeof(SOLineSplit2)].RaiseRowUpdated(solinesplit2, copy);

                    SOShipLine shiplinecopy = PXCache<SOShipLine>.CreateCopy(soshipline);
                    shiplinecopy.Confirmed = false;
					if (shipLinesClearedSOAllocation.Contains(shiplinecopy.LineNbr))
					{
						shiplinecopy.OrigPlanType = INPlanConstants.Plan60;
					}

					UpdateOrigValues(shiplinecopy, soline, plan.PlanQty);

                    this.Caches[typeof(SOShipLine)].Update(shiplinecopy);
                }

				this.Caches<SOOrder>().Clear();
				this.Caches<SOOrder>().ClearQueryCache();
				this.Caches<SOOrderSite>().Clear();
				this.Caches<SOOrderSite>().ClearQueryCache();
				this.Caches<SOLine2>().Clear();
				this.Caches<SOLine2>().ClearQueryCache();

				SOShipment.Events
					.Select(e => e.ShipmentCorrected)
					.FireOn(this, Document.Current);
				Save.Press();

				ts.Complete();
				Document.Cache.RestoreCopy(shiporder, Document.Current);
			}
		}

		protected virtual decimal? GetQtyHardAvailFromSiteStatus(PXGraph docgraph, SOLineSplit split)
        {
			var accum = new SiteStatusByCostCenter()
			{
				InventoryID = split.InventoryID,
				SiteID = split.SiteID,
				SubItemID = split.SubItemID,
				CostCenterID = split.CostCenterID,
			};
			accum = (SiteStatusByCostCenter)docgraph.Caches[typeof(SiteStatusByCostCenter)].Insert(accum);

			var stat = INSiteStatusByCostCenter.PK.Find(docgraph, split.InventoryID, split.SubItemID, split.SiteID, split.CostCenterID);

			return accum.QtyHardAvail + (stat?.QtyHardAvail ?? 0m);
		}

		public virtual void PrepareShipmentForConfirmation(SOShipment shiporder)
		{
			this.Clear();

			Document.Current = Document.Search<SOShipment.shipmentNbr>(shiporder.ShipmentNbr);
			if (WorkflowAction.HasWorkflowActionEnabled(this, g => g.confirmShipmentAction, Document.Current) == false)
			{
				throw new PXInvalidOperationException(Messages.ActionNotAvailableInCurrentState,
					confirmShipmentAction.GetCaption(), Document.Cache.GetRowDescription(Document.Current));
			}
			ItemAvailabilityExt.AdvancedCheck = true;

	        ValidateShipment(shiporder);
        }
        
        public virtual void ValidateShipment(SOShipment shiporder)
        {
	        if (sosetup.Current.RequireShipmentTotal == true)
			{
				if (Document.Current.ShipmentQty != Document.Current.ControlQty)
				{
					throw new PXException(Messages.MissingShipmentControlTotal);
				}
			}

			if (Document.Current.ShipmentQty == 0)
				throw new PXException(Messages.UnableConfirmZeroShipment, Document.Current.ShipmentNbr);

	        if ((SOOrderShipment) OrderList.SelectWindowed(0, 1) == null)
				throw new PXException(Messages.UnableConfirmShipment, Document.Current.ShipmentNbr);

			Carrier carrier = Carrier.PK.Find(this, Document.Current.ShipVia);
			if (carrier != null && carrier.IsExternal == true && carrier.PackageRequired == true)
			{
		        //check for at least one package
				SOPackageDetail p = Packages.SelectSingle();
				if (p == null)
					throw new PXException(Messages.PackageIsRequired);
	        }

			foreach (SOShipLine line in Transactions.Select())
				ConvertedInventoryItemAttribute.ValidateRow(Transactions.Cache, line);
        }

		private bool IsShipmentReadyForConfirmation = false;
		public virtual void ConfirmShipment(SOOrderEntry docgraph, SOShipment shiporder)
        {
	        if (!IsShipmentReadyForConfirmation)
		        PrepareShipmentForConfirmation(shiporder);

			if (Document.Current == null)
				return;
			
			MarkConfirmed(Document.Current);
			Document.Cache.MarkUpdated(Document.Current, assertError: true);
			Document.Cache.IsDirty = true;

			foreach (PXResult<INItemPlan, SOShipLineSplit, SOOrder> res in PXSelectJoin<INItemPlan,
				InnerJoin<SOShipLineSplit, On<SOShipLineSplit.planID, Equal<INItemPlan.planID>>,
				LeftJoin<SOOrder, On<SOOrder.orderType, Equal<SOShipLineSplit.origOrderType>, And<SOOrder.orderNbr, Equal<SOShipLineSplit.origOrderNbr>>>>>,
			Where<SOShipLineSplit.shipmentNbr, Equal<Current<SOShipment.shipmentNbr>>>>.Select(this))
			{
				SOOrder order = (SOOrder)res;
				SOShipLineSplit split = (SOShipLineSplit)res;
				INItemPlan plan = PXCache<INItemPlan>.CreateCopy((INItemPlan)res);
				INPlanType plantype = INPlanType.PK.Find(this, plan.PlanType);



				if ((bool)plantype.DeleteOnEvent)
				{
					Caches[typeof(INItemPlan)].Delete(plan);
				}
				else if (string.IsNullOrEmpty(plantype.ReplanOnEvent) == false)
				{
					plan.PlanType = plantype.ReplanOnEvent;
					plan.OrigPlanType = null;
					plan.OrigNoteID = order.NoteID;
					Caches[typeof(INItemPlan)].Update(plan);
				}
				split = (SOShipLineSplit)splits.Cache.Locate(split) ?? split;
				splits.Cache.MarkUpdated(split, assertError: true);
				if (split != null)
				{
					split.Confirmed = true;
					if ((bool)plantype.DeleteOnEvent)
					{
						split.PlanID = null;
					}
				}
				splits.Cache.IsDirty = true;
			}

			if ((PXAccess.FeatureInstalled<FeaturesSet.wMSAdvancedPicking>() || PXAccess.FeatureInstalled<FeaturesSet.wMSPaperlessPicking>()) && Document.Current.CurrentWorksheetNbr != null)
			{
				//clear dirty object loaded in scope of separate PXConnectionScope via PXFormulaAttribute
				this.Caches<SOPickingWorksheet>().Clear();
				this.Caches<SOPickingWorksheet>().ClearQueryCacheObsolete();

				SOPickingWorksheet worksheet =
					SelectFrom<SOPickingWorksheet>.
					Where<SOPickingWorksheet.worksheetNbr.IsEqual<@P.AsString>>.
					View.Select(this, Document.Current.CurrentWorksheetNbr);

				if (worksheet.WorksheetType.IsIn(SOPickingWorksheet.worksheetType.Wave, SOPickingWorksheet.worksheetType.Single))
					CartSupportExt?.RemoveItemsFromCart();

				TryCompleteWorksheet(worksheet);
			}

			PXFormulaAttribute.SetAggregate<SOLine.openLine>(docgraph.Transactions.Cache, null, typeof(SOOrder.openLineCntr));

			using (PXTransactionScope ts = new PXTransactionScope())
			{
				SetSuppressWorkflowOnConfirmShipment();

				foreach (SOOrderShipment order in OrderList.Select())
				{
					if (order.ShipmentQty <= 0m)
						throw new PXException(Messages.UnableConfirmZeroOrderShipment, Document.Current.ShipmentNbr, order.OrderType, order.OrderNbr);

					order.Confirmed = true;
					OrderList.Cache.MarkUpdated(order, assertError: true);
					OrderList.Cache.IsDirty = true;

					docgraph.Clear();

					docgraph.Document.Current = docgraph.Document.Search<SOOrder.orderNbr>(order.OrderNbr, order.OrderType);
					docgraph.Document.Current.OpenShipmentCntr--;
					docgraph.Document.Current.LastSiteID = order.SiteID;
					docgraph.Document.Current.LastShipDate = order.ShipDate;
					docgraph.Document.Cache.MarkUpdated(docgraph.Document.Current, assertError: true);

					var orderSite = docgraph.OrderSite.SelectSingle(order.OrderType, order.OrderNbr, order.SiteID);
					orderSite.OpenShipmentCntr--;
					orderSite = docgraph.OrderSite.Update(orderSite);

					docgraph.soordertype.Current.RequireControlTotal = false;

					bool backorderExists = false;
					var schedulesClosing = new HashSet<SOLine>(docgraph.Transactions.Cache.GetComparer());
					Dictionary<long?, List<INItemPlan>> demand = new Dictionary<long?, List<INItemPlan>>();

					foreach (PXResult<SOLineSplit, INItemPlan> res in PXSelectReadonly2<SOLineSplit,
						InnerJoin<INItemPlan, On<INItemPlan.supplyPlanID, Equal<SOLineSplit.planID>>>,
						Where<SOLineSplit.orderType, Equal<Required<SOOrderShipment.orderType>>,
							And<SOLineSplit.orderNbr, Equal<Required<SOOrderShipment.orderNbr>>>>>.Select(docgraph, order.OrderType, order.OrderNbr))
					{
						SOLineSplit line = res;
						INItemPlan plan = res;

						List<INItemPlan> ex;
						if (!demand.TryGetValue(line.PlanID, out ex))
						{
							demand[line.PlanID] = ex = new List<INItemPlan>();
						}
						ex.Add(plan);
					}

					foreach (PXResult<SOLine, SOShipLine> res in PXSelectJoin<SOLine,
						LeftJoin<SOShipLine, 
							On<SOShipLine.origOrderType, Equal<SOLine.orderType>, 
							And<SOShipLine.origOrderNbr, Equal<SOLine.orderNbr>, 
							And<SOShipLine.origLineNbr, Equal<SOLine.lineNbr>, 
							And<SOShipLine.shipmentNbr, Equal<Current<SOOrderShipment.shipmentNbr>>>>>>>,
						Where<SOLine.orderType, Equal<Current<SOOrderShipment.orderType>>,
							And<SOLine.orderNbr, Equal<Current<SOOrderShipment.orderNbr>>,
							And<SOLine.operation, Equal<Current<SOOrderShipment.operation>>,
							And2<Where<SOLine.siteID, Equal<Current<SOOrderShipment.siteID>>, Or<SOShipLine.shipmentNbr, IsNotNull>>,
							And<SOLine.lineType, NotEqual<SOLineType.miscCharge>,
							And<Where<SOShipLine.shipmentNbr, IsNotNull,
								Or2<Where<Current<SOOrder.siteCntr>, Equal<int1>, // The siteCntr condition is from BA requirements (Jira: AC-216918), to fix only the known scenario.
										And<Current<SOOrder.shipComplete>, Equal<SOShipComplete.cancelRemainder>,
										And<SOLine.shipComplete, Equal<SOShipComplete.cancelRemainder>,
										Or<SOLine.shipDate, LessEqual<Current<SOOrderShipment.shipDate>>>>>>,
									And<Where<SOLine.openQty, Greater<decimal0>, Or<SOLine.openLine, Equal<True>>>>>>>>>>>>,
						OrderBy<Asc<SOLine.isFree>>>.SelectMultiBound(docgraph, new object[] { order, docgraph.Document.Current }))
					{
						SOLine line = (SOLine)res;
						SOShipLine shipline = (SOShipLine)res;

						if(shipline.SiteID > 0 && line.SiteID != shipline.SiteID)
						{
							if (shipline.ShippedQty == 0)
							{
								var shipLineCache = Transactions.Cache;
								shipline = (SOShipLine)shipLineCache.Locate(shipline) ?? shipline;
								shipline.Confirmed = true;
								shipline.RequireINUpdate = false;
								shipLineCache.MarkUpdated(shipline, assertError: true);
								shipLineCache.IsDirty = true;
							}
							continue;
						}

						InventoryItem ii = InventoryItem.PK.Find(this, line.InventoryID);

						if (shipline.ShipmentNbr != null && Math.Abs((decimal)shipline.BaseQty) < 0.0000005m && this.sosetup.Current.AddAllToShipment == false)
						{
							Caches[typeof(SOShipLine)].SetStatus(shipline, PXEntryStatus.Deleted);
							Caches[typeof(SOShipLine)].ClearQueryCacheObsolete();

							shipline = new SOShipLine();
						}

						string lineShippingRule = GetShippingRule(line, shipline);
						if (shipline.ShipmentNbr != null && lineShippingRule == SOShipComplete.ShipComplete
							&& line.LineSign * line.BaseShippedQty < line.LineSign * line.BaseOrderQty * line.CompleteQtyMin / 100m)
						{
							throw new PXException(Messages.CannotShipComplete_Line, ii.InventoryCD);
						}

						if (shipline.ShipmentNbr == null && order.ShipComplete == SOShipComplete.ShipComplete && line.POSource != INReplenishmentSource.DropShipToOrder)
						{
							throw new PXException(Messages.CannotShipComplete_Order, ii.InventoryCD);
						}

						if (shipline.ShipmentNbr != null && ii.StkItem == false && ii.KitItem == true &&
							Math.Abs((decimal)shipline.BaseQty) >= 0.0000005m &&
							((SOShipLineSplit)PXSelect<SOShipLineSplit, Where<SOShipLineSplit.shipmentNbr, Equal<Current<SOShipLine.shipmentNbr>>,
													And<SOShipLineSplit.lineNbr, Equal<Current<SOShipLine.lineNbr>>>>>.SelectSingleBound(this, new object[] { shipline })) == null)
						{
							throw new PXException(Messages.CannotShipEmptyNonStockKit, ii.InventoryCD, shipline.LineNbr);
						}

						ValidateLineType(line, ii, Messages.CannotConfirmShipmentNonInventoryNonStockKit);

						bool IsLineShippedOrCancelRemainder = (shipline.ShipmentNbr != null || lineShippingRule == SOShipComplete.CancelRemainder);
						if ((IsLineShippedOrCancelRemainder || this.insetup.Current.ReplanBackOrders == true)
							&& schedulesClosing.Add(line))
						{
								foreach (PXResult<SOLineSplit, INItemPlan, SOShipLine, INSiteStatusByCostCenter> schedres
									in PXSelectJoin<SOLineSplit,
								InnerJoin<INItemPlan,
									On<INItemPlan.planID, Equal<SOLineSplit.planID>>,
								LeftJoin<SOShipLine,
									On<SOShipLine.origOrderType, Equal<SOLineSplit.orderType>,
										And<SOShipLine.origOrderNbr, Equal<SOLineSplit.orderNbr>,
										And<SOShipLine.origLineNbr, Equal<SOLineSplit.lineNbr>,
										And<SOShipLine.origSplitLineNbr, Equal<SOLineSplit.splitLineNbr>,
										And<SOShipLine.shipmentNbr, Equal<Required<SOOrderShipment.shipmentNbr>>>>>>>,
									LeftJoin<INSiteStatusByCostCenter,
										On<INSiteStatusByCostCenter.inventoryID, Equal<SOLineSplit.inventoryID>,
											And<INSiteStatusByCostCenter.subItemID, Equal<SOLineSplit.subItemID>,
											And<INSiteStatusByCostCenter.siteID, Equal<SOLineSplit.siteID>,
											And<INSiteStatusByCostCenter.costCenterID, Equal<SOLineSplit.costCenterID>>>>>>>>,
								Where<SOLineSplit.orderType, Equal<Required<SOLineSplit.orderType>>,
									And<SOLineSplit.orderNbr, Equal<Required<SOLineSplit.orderNbr>>,
									And<SOLineSplit.lineNbr, Equal<Required<SOLineSplit.lineNbr>>,
									And<SOLineSplit.siteID, Equal<Required<SOLineSplit.siteID>>,
									//And<SOLineSplit.pOCreate, Equal<False>,
									//And<SOLineSplit.fixedSource, Equal<INReplenishmentSource.none>,
									And<Where<SOLineSplit.shipDate, LessEqual<Required<SOLineSplit.shipDate>>,
										Or<SOShipLine.shipmentNbr, IsNotNull,
										Or<SOLineSplit.isAllocated, Equal<False>>>>>>>>>>.Select(docgraph, order.ShipmentNbr, line.OrderType, line.OrderNbr, line.LineNbr, order.SiteID, order.ShipDate))
							{
								SOLineSplit schedule = schedres;
								INItemPlan schedplan = schedres;
								SOShipLine shline = schedres;

								if (IsLineShippedOrCancelRemainder)
								{
									if (shline.ShipmentNbr != null && Math.Abs((decimal)shline.BaseQty) < 0.0000005m && this.sosetup.Current.AddAllToShipment == false)
									{
										shline = new SOShipLine();
									}

									List<INItemPlan> scheduleDemand = null;
									if (schedule.PlanID != null && demand.TryGetValue(schedule.PlanID, out scheduleDemand))
									{
										INItemPlan shipPlan = PXSelectJoin<INItemPlan,
														InnerJoin<SOShipLineSplit,
															On<SOShipLineSplit.planID, Equal<INItemPlan.planID>>>,
														Where<SOShipLineSplit.shipmentNbr, Equal<Current2<SOShipLine.shipmentNbr>>,
															And<SOShipLineSplit.lineNbr, Equal<Current2<SOShipLine.lineNbr>>>>>
											.SelectSingleBound(this, new object[] { shline });

										if (shipPlan?.PlanID != null)
										{
											foreach (INItemPlan item in scheduleDemand)
											{
												item.SupplyPlanID = shipPlan.PlanID;
												docgraph.Caches[typeof(INItemPlan)].MarkUpdated(item, assertError: true);
											}
											demand.Remove(schedule.PlanID);
											scheduleDemand = null;
										}
									}

									using (docgraph.LineSplittingExt.SuppressedModeScope(true))
									{
										if (schedule.FixedSource != INReplenishmentSource.None && schedule.FixedSource != INReplenishmentSource.DropShipToOrder && lineShippingRule == SOShipComplete.CancelRemainder)
										{
											schedule = PXCache<SOLineSplit>.CreateCopy(schedule);
											schedule.Completed = true;
											schedule.ShipComplete = line.ShipComplete;

											schedule = docgraph.splits.Update(schedule);
											docgraph.Caches[typeof(INItemPlan)].Delete(schedplan);

											schedule.PlanID = null;
										}

										//should precede back-order insertion
										if (shline.ShipmentNbr != null || lineShippingRule == SOShipComplete.ShipComplete || lineShippingRule == SOShipComplete.CancelRemainder && schedule.FixedSource == INReplenishmentSource.None)
										{
											schedule = PXCache<SOLineSplit>.CreateCopy(schedule);
											schedule.Completed = true;
											schedule.ShipmentNbr = shline.ShipmentNbr != null ? order.ShipmentNbr : null;
											schedule.ShipComplete = line.ShipComplete;
											schedule = docgraph.splits.Update(schedule);
											docgraph.Caches[typeof(INItemPlan)].Delete(schedplan);

											schedule.PlanID = null;

											if (lineShippingRule == SOShipComplete.CancelRemainder && schedule.FixedSource == INReplenishmentSource.None && schedule.ShippedQty == 0m)
											{
												INItemPlan demandPlan =
													PXSelect<INItemPlan, Where<INItemPlan.supplyPlanID, Equal<Current<INItemPlan.planID>>,
														And<INItemPlan.planType, Equal<INPlanConstants.plan94>>>>.SelectSingleBound(this, new object[] { schedplan });
												if (demandPlan != null)
												{
													docgraph.Caches[typeof(INItemPlan)].Delete(demandPlan);
												}
											}
										}

									if (shline.ShipmentNbr != null && lineShippingRule.IsNotIn(SOShipComplete.ShipComplete, SOShipComplete.CancelRemainder)
											&& line.LineSign * line.BaseShippedQty < line.LineSign * line.BaseOrderQty * line.CompleteQtyMin / 100m)
										{
											SOLineSplit split = PXCache<SOLineSplit>.CreateCopy(schedule);
											split.PlanID = null;
											split.PlanType = split.BackOrderPlanType;
											split.ParentSplitLineNbr = split.SplitLineNbr;
											split.SplitLineNbr = null;
											split.IsAllocated = schedule.IsAllocated;
											split.Completed = false;
											split.ShipmentNbr = null;
											split.LotSerialNbr = schedule.LotSerialNbr;
											split.LotSerClassID = schedule.LotSerClassID;

											split.ClearPOFlags();
											split.ClearPOReferences();
											split.ClearSOReferences();
											split.VendorID = null;
											split.RefNoteID = null;

											split.BaseReceivedQty = 0m;
											split.ReceivedQty = 0m;
											split.BaseShippedQty = 0m;
											split.ShippedQty = 0m;
											split.BaseQty = (schedule.BaseQty - schedule.BaseShippedQty);
											split.Qty = INUnitAttribute.ConvertFromBase(docgraph.splits.Cache, split.InventoryID, split.UOM, (decimal)split.BaseQty, INPrecision.QUANTITY);

											if (PXAccess.FeatureInstalled<FeaturesSet.multipleUnitMeasure>() && commonsetup.Current != null && commonsetup.Current.DecPlQty == 0m)
											{
												if (INUnitAttribute.ConvertToBase(docgraph.splits.Cache, split.InventoryID, split.UOM, (decimal)split.Qty, INPrecision.QUANTITY) != split.BaseQty)
												{
													throw new PXException(Messages.LowQuantityPrecision, docgraph.splits.GetValueExt<SOLineSplit.inventoryID>(split).ToString().Trim());
												}
											}

											if (split.BaseQty > 0m)
											{
												docgraph.Transactions.Current = docgraph.Transactions.Search<SOLine.orderType, SOLine.orderNbr, SOLine.lineNbr>(split.OrderType, split.OrderNbr, split.LineNbr);
												schedule = docgraph.LineSplittingAllocatedExt.InsertShipmentRemainder(split);

												if (scheduleDemand != null)
												{
													// linking the demand to the back orders if nothing was shipped
													foreach (INItemPlan item in scheduleDemand)
													{
														item.SupplyPlanID = schedule.PlanID;
														docgraph.Caches[typeof(INItemPlan)].MarkUpdated(item);
													}
													scheduleDemand = null;
												}
											}
										}
									}
								}

								if (schedule != null &&
									schedule.Completed == false &&
									schedule.PlanID != null &&
									(this.insetup.Current.ReplanBackOrders == true ||
									 schedule.IsAllocated == true))
								{
										var hardAvail = ((INSiteStatusByCostCenter)schedres).QtyHardAvail ?? 0;

									if ((hardAvail > 0 &&
										(lineShippingRule != SOShipComplete.ShipComplete || hardAvail >= schedule.BaseQty)) ||
										schedule.IsAllocated == true)
									{
										INItemPlan plan = PXSelect<INItemPlan, Where<INItemPlan.planID, Equal<Current<SOLineSplit.planID>>>>.SelectSingleBound(docgraph, new[] { schedule });

										if (plan != null)
										{
											SOOrderType ordertype = PXSetup<SOOrderType>.Select(docgraph);
											// We should skip allocated plans. In general we should process only "normal" plans.
											var initPlanType = docgraph.FindImplementation<SOLineSplitPlan>().IsPlanRegular(ordertype, plan);
											if (initPlanType == true)
											{
												schedule.PlanType = schedule.IsAllocated == true
													? schedule.AllocatedPlanType
													: schedule.BookedPlanType;

												plan = PXCache<INItemPlan>.CreateCopy(plan);
												plan.IsSkippedWhenBackOrdered = true;
												plan.PlanType = schedule.PlanType;
												docgraph.Caches<INItemPlan>().Update(plan);
											}
										}
									}
								}
							}
						}

						CreateNewSOLines(docgraph, line, shipline);

						docgraph.ConfirmSingleLine(line, shipline, lineShippingRule, ref backorderExists);

						if (shipline.ShipmentNbr != null)
						{
							object cached = Caches[typeof(SOShipLine)].Locate(shipline);
							if (cached != null)
							{
								shipline = (SOShipLine)cached;
							}

							if (Math.Abs((decimal)shipline.BaseQty) < 0.0000005m)
							{
								LineSplittingExt.RaiseRowDeleted(shipline);
							}

							shipline.Confirmed = true;

							if (shipline.LineType == SOLineType.Inventory)
							{
								if (ii.StkItem == false && ii.KitItem == true &&
									((SOShipLineSplit)PXSelectJoin<SOShipLineSplit,
											InnerJoin<InventoryItem, 
												On2<SOShipLineSplit.FK.InventoryItem,
												And<InventoryItem.stkItem, Equal<True>>>>,
											 Where<SOShipLineSplit.shipmentNbr, Equal<Current<SOShipLine.shipmentNbr>>,
												And<SOShipLineSplit.lineNbr, Equal<Current<SOShipLine.lineNbr>>>>>.SelectSingleBound(this, new object[] { shipline })) == null)
								{
									shipline.RequireINUpdate = false;
								}
								else
								{
									shipline.RequireINUpdate = true;
									order.CreateINDoc = true;
								}
							}
							else
							{
								shipline.RequireINUpdate = false;
							}

							Caches[typeof(SOShipLine)].MarkUpdated(shipline, assertError: true);
							Caches[typeof(SOShipLine)].IsDirty = true;
						}
					}

					SOOrder.Events
						.Select(e => e.GotShipmentConfirmed)
						.FireOn(docgraph, docgraph.Document.Current);
					docgraph.Save.Press();
				}

				WorkLogExt?.CloseFor(Document.Current.ShipmentNbr);

				SOShipment.Events
					.Select(e => e.ShipmentConfirmed)
					.FireOn(this, Document.Current);
				Save.Press();

				ts.Complete();
			}

			Document.Cache.RestoreCopy(shiporder, Document.Current);
		}

		public virtual bool TryCompleteWorksheet(SOPickingWorksheet worksheet)
			=> TryCompleteWorksheet(this, worksheet);

		public virtual bool TryCompleteWorksheet(PXGraph graph, SOPickingWorksheet worksheet)
		{
			bool needToCompleteWorksheet =
				worksheet.Status.IsNotIn(SOPickingWorksheet.status.Completed, SOPickingWorksheet.status.Cancelled) &&
				SelectFrom<SOShipment>.
				Where<SOShipment.currentWorksheetNbr.IsEqual<@P.AsString>>.
				View.Select(graph, worksheet.WorksheetNbr)
				.RowCast<SOShipment>()
				.AsEnumerable()
				.All(sh => sh.Confirmed == true);
			if (needToCompleteWorksheet)
			{
				worksheet.Status = SOPickingWorksheet.status.Completed;
				graph.Caches<SOPickingWorksheet>().Update(worksheet);
				graph.EnsureCachePersistence<SOPickingWorksheet>();

				var pickingJobs =
					SelectFrom<SOPickingJob>.
					Where<SOPickingJob.worksheetNbr.IsEqual<@P.AsString>>.
					View.Select(graph, worksheet.WorksheetNbr);
				foreach (SOPickingJob job in pickingJobs)
				{
					job.Status = SOPickingJob.status.Completed;
					graph.Caches<SOPickingJob>().Update(job);
					graph.EnsureCachePersistence<SOPickingJob>();
				}

				return true;
			}
			return false;
		}

		protected virtual string GetShippingRule(SOLine line, SOShipLine shipline)
		{
			return line.ShipComplete;
		}

		protected virtual void CreateNewSOLines(SOOrderEntry docgraph, SOLine line, SOShipLine shipline)
		{
			//do not create issue lines if nothing is in the current shipment i.e. shipline.Operation == null
			if (line.AutoCreateIssueLine == true && shipline.Operation == SOOperation.Receipt)
			{
				SOLine newLine = PXSelect<SOLine,
					Where<
						SOLine.orderType, Equal<Required<SOLine.orderType>>,
						And<SOLine.orderNbr, Equal<Required<SOLine.orderNbr>>,
						And<SOLine.origOrderType, Equal<Required<SOLine.origOrderType>>,
						And<SOLine.origOrderNbr, Equal<Required<SOLine.origOrderNbr>>,
						And<SOLine.origLineNbr, Equal<Required<SOLine.origLineNbr>>>>>>>>
					.SelectWindowed(docgraph, 0, 1, line.OrderType, line.OrderNbr, line.OrderType, line.OrderNbr, line.LineNbr);
				if (newLine == null)
				{
					newLine = new SOLine();
					newLine.OrderType = line.OrderType;
					newLine.OrderNbr = line.OrderNbr;
					newLine = PXCache<SOLine>.CreateCopy(docgraph.Transactions.Insert(newLine));
					newLine.IsStockItem = line.IsStockItem;
					newLine.InventoryID = line.InventoryID;
					newLine.SubItemID = line.SubItemID;
					newLine.UOM = line.UOM;
					newLine.SiteID = line.SiteID;
					newLine.OrderQty = -line.OrderQty;
					newLine.OrigOrderType = line.OrderType;
					newLine.OrigOrderNbr = line.OrderNbr;
					newLine.OrigLineNbr = line.LineNbr;
					newLine.ManualDisc = line.ManualDisc;
					newLine.ManualPrice = true;
					newLine.CuryUnitPrice = line.CuryUnitPrice;
					newLine.SalesPersonID = line.SalesPersonID;
					newLine.ProjectID = line.ProjectID;
					newLine.TaskID = line.TaskID;
					newLine.CostCodeID = line.CostCodeID;
					newLine.ReasonCode = line.ReasonCode;
					newLine.IsSpecialOrder = false;

					if (line.ManualDisc == true)
					{
						newLine.DiscPct = line.DiscPct;
						newLine.CuryDiscAmt = line.CuryDiscAmt;
						newLine.CuryLineAmt = line.CuryLineAmt;
					}

					docgraph.Transactions.Update(newLine);

					//Update manually
					docgraph.Document.Current.OpenLineCntr++;
				}
			}
		}

		public virtual void UpdateOrigValues(SOShipLine shipline, SOLine soline, decimal? baseOrigQty)
		{
			shipline.ShipComplete = soline.ShipComplete;
			shipline.CompleteQtyMin = soline.CompleteQtyMin;
			shipline.OrderUOM = soline.UOM;
			shipline.FullOrderQty = soline.LineSign * soline.OrderQty;
			shipline.BaseFullOrderQty = soline.LineSign * soline.BaseOrderQty;
			shipline.BaseOrigOrderQty = baseOrigQty;
			shipline.OrigOrderQty = (shipline.UOM == shipline.OrderUOM && baseOrigQty == shipline.BaseFullOrderQty) ? shipline.FullOrderQty
				: INUnitAttribute.ConvertFromBase(Transactions.Cache, shipline.InventoryID, shipline.UOM, baseOrigQty ?? 0m, INPrecision.QUANTITY);
		}

		public virtual void InvoiceShipment(SOInvoiceEntry docgraph, SOShipment shiporder, DateTime invoiceDate, InvoiceList list, PXQuickProcess.ActionFlow quickProcessFlow)
		{
			this.Clear();

			Document.Current = Document.Search<SOShipment.shipmentNbr>(shiporder.ShipmentNbr);
			if (WorkflowAction.HasWorkflowActionEnabled(this, g => g.createInvoice, Document.Current) == false)
			{
				throw new PXInvalidOperationException(Messages.ActionNotAvailableInCurrentState,
					createInvoice.GetCaption(), Document.Cache.GetRowDescription(Document.Current));
			}

			Document.Current.Status = shiporder.Status;

			Document.Cache.MarkUpdated(Document.Current, assertError: true);

			using (PXTransactionScope ts = new PXTransactionScope())
			{
				this.Save.Press();

				foreach (PXResult<SOOrderShipment, SOOrder, CurrencyInfo, SOAddress, SOContact, SOOrderType> order in PXSelectJoin<SOOrderShipment,
					InnerJoin<SOOrder, On<SOOrder.orderType, Equal<SOOrderShipment.orderType>, And<SOOrder.orderNbr, Equal<SOOrderShipment.orderNbr>>>,
					InnerJoin<CurrencyInfo, On<CurrencyInfo.curyInfoID, Equal<SOOrder.curyInfoID>>,
					InnerJoin<SOAddress, On<SOAddress.addressID, Equal<SOOrder.billAddressID>>,
					InnerJoin<SOContact, On<SOContact.contactID, Equal<SOOrder.billContactID>>,
					InnerJoin<SOOrderType, On<SOOrderType.orderType, Equal<SOOrder.orderType>>,
					InnerJoin<SOOrderTypeOperation,
								 On<SOOrderTypeOperation.orderType, Equal<SOOrderType.orderType>,
										And<SOOrderTypeOperation.operation, Equal<SOOrderType.defaultOperation>>>>>>>>>,
					Where<SOOrderShipment.shipmentType, Equal<Current<SOShipment.shipmentType>>,
						And<SOOrderShipment.shipmentNbr, Equal<Current<SOShipment.shipmentNbr>>,
						And<SOOrderShipment.invoiceNbr, IsNull>>>>.Select(this))
				{
					((SOOrderShipment)order).BillShipmentSeparately = shiporder.BillSeparately;

					docgraph.Clear();
					docgraph.Clear(PXClearOption.ClearQueriesOnly);
					docgraph.ARSetup.Current.RequireControlTotal = false;

					var shipmentInvoices = list as ShipmentInvoices;
					if (shipmentInvoices != null)
					{
						var orderType = SOOrderType.PK.Find(this, ((SOOrder)order).OrderType);
						var docType = docgraph.GetInvoiceDocType(orderType, ((SOOrder)order), ((SOOrderShipment)order).Operation);
						var subList = new InvoiceList(docgraph);
						subList.AddRange(shipmentInvoices.GetInvoices(docType));
						int oldCount = subList.Count;
						docgraph.InvoiceOrder(new InvoiceOrderArgs(order)
						{
							InvoiceDate = invoiceDate,
							Customer = customer.Current,
							List = subList,
							QuickProcessFlow = quickProcessFlow
						});
							
						if (subList.Count > oldCount)
							list.Add(subList[oldCount], subList[oldCount], (CM.Extensions.CurrencyInfo)subList[oldCount][typeof(CM.Extensions.CurrencyInfo)]);
					}
					else
						docgraph.InvoiceOrder(new InvoiceOrderArgs(order)
						{
							InvoiceDate = invoiceDate,
							Customer = customer.Current,
							List = list,
							QuickProcessFlow = quickProcessFlow
						});
				}
				ts.Complete();
			}
		}

		public static void InvoiceReceipt(Dictionary<string, object> parameters, List<SOShipment> list, InvoiceList created, bool isMassProcess = false)
		{
			SOShipmentEntry docgraph = PXGraph.CreateInstance<SOShipmentEntry>();
			SOInvoiceEntry ie = PXGraph.CreateInstance<SOInvoiceEntry>();

			list.Sort((x,y)=> { return (x.ShipmentNbr).CompareTo(y.ShipmentNbr);});

			foreach (SOShipment poreceipt in list)
			{
				try
				{
					if (isMassProcess)
					{
						PXProcessing<SOShipment>.SetCurrentItem(poreceipt);
					}

					ie.Clear();
					ie.Clear(PXClearOption.ClearQueriesOnly);
					ie.ARSetup.Current.RequireControlTotal = false;

					char[] a = typeof(SOShipmentFilter.invoiceDate).Name.ToCharArray();
					a[0] = char.ToUpper(a[0]);
					object invoiceDate;
					if (!parameters.TryGetValue(new string(a), out invoiceDate))
					{
						invoiceDate = ie.Accessinfo.BusinessDate;
					}

					foreach (PXResult<SOOrderShipment, SOOrder, CurrencyInfo, SOAddress, SOContact> res in PXSelectJoin<SOOrderShipment,
					InnerJoin<SOOrder, On<SOOrder.orderType, Equal<SOOrderShipment.orderType>, And<SOOrder.orderNbr, Equal<SOOrderShipment.orderNbr>>>,
					InnerJoin<CurrencyInfo, On<CurrencyInfo.curyInfoID, Equal<SOOrder.curyInfoID>>,
					InnerJoin<SOAddress, On<SOAddress.addressID, Equal<SOOrder.billAddressID>>,
					InnerJoin<SOContact, On<SOContact.contactID, Equal<SOOrder.billContactID>>>>>>,
					Where<SOOrderShipment.shipmentType, Equal<SOShipmentType.dropShip>, 
						And<SOOrderShipment.shipmentNbr, Equal<Required<SOOrderShipment.shipmentNbr>>, 
						And<SOOrderShipment.invoiceNbr, IsNull>>>>.Select(docgraph, poreceipt.ShipmentNbr))
					{
						SOOrderShipment shipment = res;
						shipment.BillShipmentSeparately = poreceipt.BillSeparately;
						SOOrder order = res;
						PXResult<SOOrderShipment, SOOrder, CurrencyInfo, SOAddress, SOContact> record =
							new PXResult<SOOrderShipment, SOOrder, CurrencyInfo, SOAddress, SOContact>(shipment, order, (CurrencyInfo)res, (SOAddress)res, (SOContact)res);

						PXResultset<SOShipLine, SOLine> details = new PXResultset<SOShipLine, SOLine>();
						details.AddRange(docgraph.CollectDropshipDetails(shipment));

						var shipmentInvoices = created as ShipmentInvoices;
						if (shipmentInvoices != null)
						{
							var orderType = SOOrderType.PK.Find(docgraph, order.OrderType);
							var docType = ie.GetInvoiceDocType(orderType, order, shipment.Operation);

							var subList = new InvoiceList(docgraph);
							subList.AddRange(shipmentInvoices.GetInvoices(docType));
							int oldCount = subList.Count;

							ie.InvoiceOrder(new InvoiceOrderArgs(record)
							{
								InvoiceDate = (DateTime)invoiceDate,
								Details = details,
								List = subList
							});

							if (subList.Count > oldCount)
								created.Add(subList[oldCount], subList[oldCount], (CM.Extensions.CurrencyInfo)subList[oldCount][typeof(CM.Extensions.CurrencyInfo)]);
						}
						else
							ie.InvoiceOrder(new InvoiceOrderArgs(record)
							{
								InvoiceDate = (DateTime)invoiceDate,
								Details = details,
								List = created
							});

						if (ie.Caches.ContainsKey(typeof(SOOrder)) && PXTimeStampScope.GetPersisted(ie.Caches[typeof(SOOrder)], order) != null)
							PXTimeStampScope.PutPersisted(ie.Caches[typeof(SOOrder)], order, ie.TimeStamp);
					}
					if (isMassProcess)
					{
						PXProcessing<SOShipment>.SetProcessed();
					}
				}
				catch (Exception ex)
				{
					if (!isMassProcess)
					{
						throw;
					}
					PXProcessing<SOShipment>.SetError(ex);
				}
			}
		}

		public virtual IEnumerable<PXResult<SOShipLine, SOLine>> CollectDropshipDetails(SOOrderShipment shipment)
		{
			foreach (PXResult<POReceiptLine, SOLineSplit, SOLine> line in PXSelectJoin<POReceiptLine,
				InnerJoin<SOLineSplit, On<SOLineSplit.pOType, Equal<POReceiptLine.pOType>, And<SOLineSplit.pONbr, Equal<POReceiptLine.pONbr>,
					And<SOLineSplit.pOLineNbr, Equal<POReceiptLine.pOLineNbr>>>>,
				InnerJoin<SOLine, On<SOLineSplit.FK.OrderLine>>>,
				Where<POReceiptLine.lineType, In3<POLineType.goodsForDropShip, POLineType.nonStockForDropShip>,
					And<POReceiptLine.receiptType, Equal<Required<POReceiptLine.receiptType>>,
					And<POReceiptLine.receiptNbr, Equal<Required<POReceiptLine.receiptNbr>>,
					And<SOLine.orderType, Equal<Current<SOOrderShipment.orderType>>,
					And<SOLine.orderNbr, Equal<Current<SOOrderShipment.orderNbr>>>>>>>>
				.SelectMultiBound(this,
					new object[] { shipment },
					shipment.Operation == SOOperation.Receipt ? POReceiptType.POReturn : POReceiptType.POReceipt,
					shipment.ShipmentNbr
				).AsEnumerable()
				.Cast<PXResult<POReceiptLine, SOLineSplit, SOLine>>())
			{
				yield return new PXResult<SOShipLine, SOLine>(SOShipLine.FromDropShip(line, line), line);
			}
		}

		public virtual void PostReceipt(INIssueEntry docgraph, PXResult<SOOrderShipment, SOOrder> sh, ARInvoice invoice, DocumentList<INRegister> list)
		{
			SOOrderShipment shiporder = sh;
			SOOrder order = sh;

			this.Clear();
			docgraph.Clear();

			docgraph.insetup.Current.HoldEntry = false;
			docgraph.insetup.Current.RequireControlTotal = false;

			INRegister newdoc =
				list.Find<INRegister.srcDocType, INRegister.srcRefNbr>(shiporder.ShipmentType, shiporder.ShipmentNbr)
				?? new INRegister();

			if (newdoc.RefNbr != null)
			{
				docgraph.issue.Current = docgraph.issue.Search<INRegister.docType, INRegister.refNbr>(newdoc.DocType, newdoc.RefNbr);
				if (docgraph.issue.Current != null && docgraph.issue.Current.SrcRefNbr == null) //Non-db fields cannot be restored after .Clear()
				{
					docgraph.issue.Current.SrcDocType = shiporder.ShipmentType;
					docgraph.issue.Current.SrcRefNbr = shiporder.ShipmentNbr;
				}
			}
			else
			{
				newdoc.BranchID = order.BranchID;
				newdoc.DocType = INDocType.Issue;
				newdoc.SiteID = shiporder.SiteID;
				newdoc.TranDate = invoice.DocDate;
				newdoc.OrigModule = GL.BatchModule.SO;
				newdoc.SrcDocType = shiporder.ShipmentType;
				newdoc.SrcRefNbr = shiporder.ShipmentNbr;
				newdoc.FinPeriodID = invoice.FinPeriodID;

				docgraph.issue.Insert(newdoc);
			}

			INTran newline = null;
			POReceiptLine prev_line = null;

			BqlCommand selectDropshipReceiptsCmd = BqlCommand.CreateInstance(
				typeof(Select2<POReceiptLine,
					InnerJoin<InventoryItem, On<POReceiptLine.FK.InventoryItem>,
					LeftJoin<INLotSerClass, On<InventoryItem.FK.LotSerialClass>,
					LeftJoin<INPostClass, On<InventoryItem.FK.PostClass>,
					InnerJoin<INSite, On<POReceiptLine.FK.Site>,
					LeftJoin<INLocation, On<INLocation.locationID, Equal<INSite.dropShipLocationID>>>>>>>,
				Where<POReceiptLine.receiptNbr, Equal<Current<SOOrderShipment.shipmentNbr>>,
					And<SOLine.orderType, Equal<Current<SOOrderShipment.orderType>>,
					And<SOLine.orderNbr, Equal<Current<SOOrderShipment.orderNbr>>,
					And<POReceiptLine.receiptQty, NotEqual<decimal0>,
					And<POReceiptLine.lineType, In3<POLineType.goodsForDropShip, POLineType.nonStockForDropShip>,
					And<INTran.refNbr, IsNull>>>>>>>));
			// TODO: DropshipReturn
			// unify join to SOLine or move to a separate method
			if (shiporder.Operation == SOOperation.Receipt)
			{
				selectDropshipReceiptsCmd = selectDropshipReceiptsCmd.WhereAnd<Where<POReceiptLine.receiptType, Equal<POReceiptType.poreturn>>>();

				selectDropshipReceiptsCmd = BqlCommand.AppendJoin<
					InnerJoin<SOLine, On<POReceiptLine.FK.SOLine>>>(selectDropshipReceiptsCmd);
			}
			else
			{
				selectDropshipReceiptsCmd = selectDropshipReceiptsCmd.WhereAnd<Where<POReceiptLine.receiptType, Equal<POReceiptType.poreceipt>>>();

				selectDropshipReceiptsCmd = BqlCommand.AppendJoin<
					InnerJoin<SOLineSplit, On<SOLineSplit.pOType, Equal<POReceiptLine.pOType>, And<SOLineSplit.pONbr, Equal<POReceiptLine.pONbr>, And<SOLineSplit.pOLineNbr, Equal<POReceiptLine.pOLineNbr>>>>,
					InnerJoin<SOLine, On<SOLineSplit.FK.OrderLine>>>>(selectDropshipReceiptsCmd);
			}
			selectDropshipReceiptsCmd = BqlCommand.AppendJoin<
				InnerJoin<ARTran,
					On<ARTran.sOShipmentNbr, Equal<POReceiptLine.receiptNbr>,
					And<ARTran.sOShipmentType, Equal<SOShipmentType.dropShip>,
					And<ARTran.sOOrderType, Equal<SOLine.orderType>,
					And<ARTran.sOOrderNbr, Equal<SOLine.orderNbr>,
					And<ARTran.sOOrderLineNbr, Equal<SOLine.lineNbr>,
					And<ARTran.lineType, Equal<SOLine.lineType>>>>>>>,
				LeftJoin<INTran,
					On<INTran.sOShipmentNbr, Equal<POReceiptLine.receiptNbr>,
					And<INTran.sOShipmentType, Equal<SOShipmentType.dropShip>,
					And<INTran.sOShipmentLineNbr, Equal<POReceiptLine.lineNbr>,
					And<INTran.sOOrderType, Equal<SOLine.orderType>,
					And<INTran.sOOrderNbr, Equal<SOLine.orderNbr>,
					And<INTran.sOOrderLineNbr, Equal<SOLine.lineNbr>>>>>>>>>>(selectDropshipReceiptsCmd);
			var selectDropshipReceiptsView = new PXView(this, false, selectDropshipReceiptsCmd);

			foreach (PXResult<POReceiptLine> res in selectDropshipReceiptsView.SelectMultiBound(new object[] { shiporder }))
			{
				POReceiptLine line = res;
				SOLine soline = PXResult.Unwrap<SOLine>(res);
				ARTran artran = PXResult.Unwrap<ARTran>(res);
				var orderoperation = SOOrderTypeOperation.PK.Find(this, soline.OrderType, soline.Operation);
				INLocation loc = PXResult.Unwrap<INLocation>(res);
				INLotSerClass lsclass = PXResult.Unwrap<INLotSerClass>(res);
				INPostClass postclass = PXResult.Unwrap<INPostClass>(res);
				InventoryItem item = PXResult.Unwrap<InventoryItem>(res);
				INSite site = PXResult.Unwrap<INSite>(res);

				if (Caches[typeof(POReceiptLine)].ObjectsEqual(prev_line, line))
					continue;

				if (line.LineType == POLineType.GoodsForDropShip && loc.LocationID == null)
				{
					throw new PXException(Messages.NoDropShipLocation, Caches[typeof(POReceiptLine)].GetValueExt<POReceiptLine.siteID>(line));
				}

				newline = new INTran();
				newline.BranchID = soline.BranchID;
				newline.TranType = orderoperation.INDocType;
				newline.POReceiptType = line.ReceiptType;
				newline.POReceiptNbr = line.ReceiptNbr;
				newline.POReceiptLineNbr = line.LineNbr;
				newline.POLineType = line.LineType;
				newline.SOShipmentNbr = line.ReceiptNbr;
				newline.SOShipmentType = SOShipmentType.DropShip;
				newline.SOShipmentLineNbr = line.LineNbr;
				newline.SOOrderType = soline.OrderType;
				newline.SOOrderNbr = soline.OrderNbr;
				newline.SOOrderLineNbr = soline.LineNbr;
				newline.ARDocType = artran.TranType;
				newline.ARRefNbr = artran.RefNbr;
				newline.ARLineNbr = artran.LineNbr;

				newline.InventoryID = line.InventoryID;
				newline.SubItemID = line.SubItemID;
				newline.SiteID = line.SiteID;
				newline.LocationID = loc.LocationID;
				newline.BAccountID = soline.CustomerID;
				newline.InvtMult = (short)0;
                newline.IsCostUnmanaged = true;

				newline.UOM = line.UOM;
				newline.Qty = line.ReceiptQty;
				newline.UnitPrice = artran.UnitPrice ?? 0m;
				bool signMismatch = artran.DrCr == DrCr.Credit && artran.SOOrderLineOperation == SOOperation.Receipt
					|| artran.DrCr == DrCr.Debit && artran.SOOrderLineOperation == SOOperation.Issue;
				newline.TranAmt = (signMismatch ? -artran.TranAmt : artran.TranAmt) ?? 0m;
				newline.UnitCost = line.UnitCost;
				newline.TranCost = line.TranCostFinal;
				newline.TranDesc = soline.TranDesc;
				newline.ReasonCode = soline.ReasonCode;
				newline.AcctID = line.POAccrualAcctID;
				newline.SubID = line.POAccrualSubID;
				newline.ReclassificationProhibited = true;
				if (line.ExpenseAcctID == null && postclass != null && postclass.COGSSubFromSales == true)
				{
					newline.COGSAcctID = INReleaseProcess.GetAccountDefaults<INPostClass.cOGSAcctID>(this, item, site, postclass);
					newline.COGSSubID = artran.SubID;
				}
				else
				{
					newline.COGSAcctID = line.ExpenseAcctID;
					newline.COGSSubID = (postclass != null && postclass.COGSSubFromSales == true ? artran.SubID : null) ?? line.ExpenseSubID;
				}
				newline.ProjectID = line.ProjectID;
				newline.TaskID = line.TaskID;
				newline.CostCodeID = line.CostCodeID;
				docgraph.CostCenterDispatcherExt?.SetCostLayerType(newline);
				newline = docgraph.transactions.Insert(newline);

				PXSelectBase<POReceiptLineSplit> selectSplits = new PXSelect<POReceiptLineSplit,
					Where<POReceiptLineSplit.receiptType, Equal<Required<POReceiptLineSplit.receiptType>>,
					And<POReceiptLineSplit.receiptNbr, Equal<Required<POReceiptLineSplit.receiptNbr>>,
					And<POReceiptLineSplit.lineNbr, Equal<Required<POReceiptLineSplit.lineNbr>>,
					And<POReceiptLineSplit.qty, NotEqual<decimal0>>>>>>(this);

				foreach (POReceiptLineSplit split in selectSplits.Select(line.ReceiptType, line.ReceiptNbr, line.LineNbr))
				{
					INTranSplit newsplit = (INTranSplit)newline;
					newsplit.SplitLineNbr = null;
					newsplit.LotSerialNbr = split.LotSerialNbr;
					newsplit.ExpireDate = split.ExpireDate;
					newsplit.BaseQty = split.BaseQty;
					newsplit.Qty = split.Qty;
					newsplit.UOM = split.UOM;
					newsplit.InvtMult = 0;

					docgraph.splits.Insert(newsplit);
				}

				prev_line = line;
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

					{
						shiporder.InvtDocType = docgraph.issue.Current.DocType;
						shiporder.InvtRefNbr = docgraph.issue.Current.RefNbr;
						shiporder.InvtNoteID = docgraph.issue.Current.NoteID;

						OrderList.Cache.Update(shiporder);
					}

					PXDBDefaultAttribute.SetDefaultForUpdate<SOOrderShipment.shipAddressID>(OrderList.Cache, null, false);
					PXDBDefaultAttribute.SetDefaultForUpdate<SOOrderShipment.shipContactID>(OrderList.Cache, null, false);
					PXDBLiteDefaultAttribute.SetDefaultForUpdate<SOOrderShipment.shipmentNbr>(OrderList.Cache, null, false);

					this.Save.Press();

					if (list.Find(docgraph.issue.Current) == null)
					{
						list.Add(docgraph.issue.Current);
					}
				}
				ts.Complete();
			}
		}

		public virtual INRegisterEntryFactory CreateINRegisterFactory()
		{
			return new INRegisterEntryFactory(this);
		}

		public void MergeCachesWithINRegisterEntry(INRegisterEntryBase graph)
		{
			MergeStatusCachesBetweenGraphs(this, graph);
		}

		protected virtual void MergeStatusCachesBetweenGraphs(PXGraph source, PXGraph target)
		{
			target.Caches[typeof(SiteStatusByCostCenter)] = source.Caches[typeof(SiteStatusByCostCenter)];
			target.Caches[typeof(LocationStatusByCostCenter)] = source.Caches[typeof(LocationStatusByCostCenter)];
			target.Caches[typeof(LotSerialStatusByCostCenter)] = source.Caches[typeof(LotSerialStatusByCostCenter)];
			target.Caches[typeof(SiteLotSerial)] = source.Caches[typeof(SiteLotSerial)];
			target.Caches[typeof(ItemLotSerial)] = source.Caches[typeof(ItemLotSerial)];

			target.Views.Caches.Remove(typeof(SiteStatusByCostCenter));
			target.Views.Caches.Remove(typeof(LocationStatusByCostCenter));
			target.Views.Caches.Remove(typeof(LotSerialStatusByCostCenter));
			target.Views.Caches.Remove(typeof(SiteLotSerial));
			target.Views.Caches.Remove(typeof(ItemLotSerial));
		}

		public virtual void PostShipment(INRegisterEntryFactory factory, SOShipment shiporder, DocumentList<INRegister> list)
		{
			this.Clear();
			INRegisterEntryBase docgraph = factory.GetOrCreateINRegisterEntry(shiporder);

			Document.Current = Document.Search<SOShipment.shipmentNbr>(shiporder.ShipmentNbr);
			if (WorkflowAction.HasWorkflowActionEnabled(this, g => g.UpdateIN, Document.Current) == false)
			{
				throw new PXInvalidOperationException(Messages.ActionNotAvailableInCurrentState,
					UpdateIN.GetCaption(), Document.Cache.GetRowDescription(Document.Current));
			}

			Document.Current.Status = shiporder.Status;
			Document.Cache.MarkUpdated(Document.Current, assertError: true);
			Document.Cache.IsDirty = true;

			using (PXTransactionScope ts = new PXTransactionScope())
			{
				SetSuppressWorkflowOnUpdateIN();

				foreach (PXResult<SOOrderShipment, SOOrder> res in PXSelectJoin<SOOrderShipment, 
					InnerJoin<SOOrder, On<SOOrder.orderType, Equal<SOOrderShipment.orderType>, And<SOOrder.orderNbr, Equal<SOOrderShipment.orderNbr>>>>, 
					Where<SOOrderShipment.shipmentType, Equal<Current<SOShipment.shipmentType>>, And<SOOrderShipment.shipmentNbr, Equal<Current<SOShipment.shipmentNbr>>, 
					And<SOOrderShipment.invtRefNbr, IsNull>>>>.SelectMultiBound(this, new object[] { shiporder }))
				{
					this.PostShipment(docgraph, res, list, null);
				}
				ts.Complete();
			}
		}

		public virtual void ShipmentINTranRowPersisted(PXCache sender, PXRowPersistedEventArgs e)
		{
			INTran row = e.Row as INTran;

			if (e.Operation != PXDBOperation.Insert|| e.TranStatus != PXTranStatus.Open || row == null)
				return;

			using (PXDataRecord rec = PXDatabase.SelectSingle<INTran>(
				new PXDataField<INTran.refNbr>(),
				new PXDataFieldValue<INTran.sOShipmentType>(row.SOShipmentType),
				new PXDataFieldValue<INTran.sOShipmentNbr>(row.SOShipmentNbr),
				new PXDataFieldValue<INTran.sOShipmentLineNbr>(row.SOShipmentLineNbr),
				new PXDataFieldValue<INTran.docType>(row.DocType),
				new PXDataFieldValue<INTran.refNbr>(row.RefNbr, PXComp.NE)))
			{
				if (rec != null)
					throw new PXException(ErrorMessages.RecordAddedByAnotherProcess, sender.DisplayName, ErrorMessages.ChangesWillBeLost);
			}
		}

		public virtual void PostShipment(INRegisterEntryBase docgraph, PXResult<SOOrderShipment, SOOrder> sh, DocumentList<INRegister> list, ARInvoice invoice)
		{
			try
			{
				SOOrderShipment shiporder = sh;
				SOOrder order = sh;
				var reattachedPlans = new List<INItemPlan>();
				var orderEntry = new Lazy<SOOrderEntry>(() => PXGraph.CreateInstance<SOOrderEntry>());
				using (docgraph.TranSplitPlanExt.ReleaseModeScope())
				{
					docgraph.RowPersisted.AddHandler<INTran>(ShipmentINTranRowPersisted);

					GL.Branch branch = PXSelectJoin<GL.Branch, InnerJoin<INSite, On<INSite.branchID, Equal<GL.Branch.branchID>>>, Where<INSite.siteID, Equal<Current<SOShipment.siteID>>>>.SelectSingleBound(this, null); //TODO: Need review INRegister Branch and SOShipment SiteID/DestinationSiteID AC-55773

					if (!Document.Cache.IsDirty)
					{
						this.Clear();
						docgraph.Clear();

						Document.Current = Document.Search<SOShipment.shipmentNbr>(shiporder.ShipmentNbr);
					}

					docgraph.insetup.Current.HoldEntry = false;
					docgraph.insetup.Current.RequireControlTotal = false;

					bool needInsertNewDoc = false;
					INRegister newdoc =
						list.Find<INRegister.srcDocType, INRegister.srcRefNbr>(shiporder.ShipmentType, shiporder.ShipmentNbr)
						?? new INRegister();

					if (newdoc.RefNbr != null)
					{
						docgraph.INRegisterDataMember.Current = PXSelect<INRegister>.Search<INRegister.docType, INRegister.refNbr>(docgraph, newdoc.DocType, newdoc.RefNbr);
						if (docgraph.INRegisterDataMember.Current != null && docgraph.INRegisterDataMember.Current.SrcRefNbr == null) //Non-db fields cannot be restored after .Clear()
						{
							docgraph.INRegisterDataMember.Current.SrcDocType = shiporder.ShipmentType;
							docgraph.INRegisterDataMember.Current.SrcRefNbr = shiporder.ShipmentNbr;
						}
					}
					else
					{
						newdoc.BranchID = (shiporder.ShipmentType == SOShipmentType.Transfer) ? branch.BranchID : (invoice?.BranchID ?? order.BranchID);
						newdoc.DocType = shiporder.ShipmentType;
						newdoc.SiteID = shiporder.SiteID;
						newdoc.ToSiteID = Document.Current.DestinationSiteID;
						if (newdoc.DocType == SOShipmentType.Transfer)
						{
							newdoc.TransferType = INTransferType.TwoStep;
						}
						if (invoice == null)
						{
							newdoc.TranDate = shiporder.ShipDate;
						}
						else
						{
							newdoc.TranDate = invoice.DocDate;
							newdoc.FinPeriodID = invoice.FinPeriodID;
						}
						newdoc.OrigModule = GL.BatchModule.SO;
						newdoc.SrcDocType = shiporder.ShipmentType;
						newdoc.SrcRefNbr = shiporder.ShipmentNbr;

						needInsertNewDoc = true; // IN Doc will be inserted only if IN Transactions are actually created to prevent unneeded validations
					}

					SOShipLine prev_line = null;
					ARTran prev_artran = null;
					INTran newline = null;

					Dictionary<long?, List<INItemPlan>> demand = new Dictionary<long?, List<INItemPlan>>();

					foreach (PXResult<SOShipLine, SOShipLineSplit, INItemPlan> res in PXSelectJoin<SOShipLine,
						InnerJoin<SOShipLineSplit, On<SOShipLineSplit.shipmentNbr, Equal<SOShipLine.shipmentNbr>, And<SOShipLineSplit.lineNbr, Equal<SOShipLine.lineNbr>>>,
						InnerJoin<INItemPlan, On<INItemPlan.supplyPlanID, Equal<SOShipLineSplit.planID>>>>,
					Where<SOShipLine.shipmentNbr, Equal<Current<SOOrderShipment.shipmentNbr>>,
						And<SOShipLine.origOrderType, Equal<Current<SOOrderShipment.orderType>>,
						And<SOShipLine.origOrderNbr, Equal<Current<SOOrderShipment.orderNbr>>>>>>.SelectMultiBound(this, new object[] { shiporder }))
					{
						SOShipLineSplit split = res;
						INItemPlan plan = res;

						List<INItemPlan> ex;
						if (!demand.TryGetValue(split.PlanID, out ex))
						{
							demand[split.PlanID] = ex = new List<INItemPlan>();
						}
						ex.Add(plan);
					}

					foreach (PXResult<SOShipLine, SOShipLineSplit, SOLine, ARTran, INTran, INItemPlan> res in PXSelectJoin<SOShipLine,
						InnerJoin<SOShipLineSplit,
							On<SOShipLineSplit.shipmentNbr, Equal<SOShipLine.shipmentNbr>,
							And<SOShipLineSplit.lineNbr, Equal<SOShipLine.lineNbr>>>,
						LeftJoin<SOLine,
							On<SOLine.orderType, Equal<SOShipLine.origOrderType>,
							And<SOLine.orderNbr, Equal<SOShipLine.origOrderNbr>,
							And<SOLine.lineNbr, Equal<SOShipLine.origLineNbr>>>>,
						LeftJoin<ARTran,
							On<ARTran.sOShipmentNbr, Equal<SOShipLine.shipmentNbr>,
							And<ARTran.sOShipmentType, NotEqual<SOShipmentType.dropShip>,
							And<ARTran.lineType, Equal<SOShipLine.lineType>,
							And<ARTran.sOOrderType, Equal<SOShipLine.origOrderType>,
							And<ARTran.sOOrderNbr, Equal<SOShipLine.origOrderNbr>,
							And<ARTran.sOOrderLineNbr, Equal<SOShipLine.origLineNbr>,
							And<Where<ARTran.sOShipmentLineNbr, Equal<SOShipLine.lineNbr>, Or<SOShipLine.isIntercompany, Equal<False>>>>>>>>>>,
							//the splits are merging to single ARTRan when SOInvoiceEntry.IsShiplineMergeAllowed() == true
						LeftJoin<INTran, On<INTran.sOShipmentNbr, Equal<SOShipLine.shipmentNbr>, And<INTran.sOShipmentType, NotEqual<SOShipmentType.dropShip>, And<INTran.sOShipmentLineNbr, Equal<SOShipLine.lineNbr>>>>,
						LeftJoin<INItemPlan, On<INItemPlan.planID, Equal<SOShipLineSplit.planID>>>>>>>,
					Where<SOShipLine.shipmentNbr, Equal<Current<SOOrderShipment.shipmentNbr>>,
						And<SOShipLine.origOrderType, Equal<Current<SOOrderShipment.orderType>>,
						And<SOShipLine.origOrderNbr, Equal<Current<SOOrderShipment.orderNbr>>,
						And<INTran.refNbr, IsNull>>>>,
					OrderBy<Asc<SOShipLine.shipmentNbr, Asc<SOShipLine.lineNbr>>>>.SelectMultiBound(this, new object[] { shiporder }))
					{
						SOShipLine line = res;
						SOShipLineSplit split = res;
						INItemPlan plan = res;
						INPlanType plantype = INPlanType.PK.Find(this, plan.PlanType) ?? new INPlanType();
						SOLine soline = res;
						ARTran artran = res;
						SOOrderType ordertype = SOOrderType.PK.Find(this, shiporder.OrderType);
						SOShipLineSplit splitcopy = PXCache<SOShipLineSplit>.CreateCopy(split);

						//TODO: Temporary solution. Review when AC-80210 is fixed
						if ((shiporder.ShipmentNbr != Constants.NoShipmentNbr && shiporder.ShipmentType != SOShipmentType.DropShip && shiporder.Confirmed != true) ||
							line.Confirmed != true || line.IsUnassigned == true ||
							(split.LineType == SOLineType.Inventory && split.IsStockItem == true && split.Confirmed != true))
						{
							throw new PXException(Messages.UnableToProcessUnconfirmedShipment, shiporder.ShipmentNbr);
						}

						//avoid ReadItem()
						if (plan.PlanID != null)
						{
							Caches[typeof(INItemPlan)].SetStatus(plan, PXEntryStatus.Notchanged);
						}

						bool zeroLine = line.BaseShippedQty < 0.0000005m;
						bool reattachExistingPlan = false;
						if (plantype.DeleteOnEvent == true || zeroLine)
						{
							if (zeroLine)
							{
								Caches[typeof(INItemPlan)].Delete(plan);
							}
							else
							{
								reattachExistingPlan = true;
							}

							Caches[typeof(SOShipLineSplit)].MarkUpdated(split, assertError: true);
							split = (SOShipLineSplit)Caches[typeof(SOShipLineSplit)].Locate(split);
							if (split != null)
							{
								split.PlanID = null;
								split.Released = true;
							}

							Caches[typeof(SOShipLineSplit)].IsDirty = true;

							if (zeroLine)
							{
								continue;
							}
						}
						else if (string.IsNullOrEmpty(plantype.ReplanOnEvent) == false)
						{
							plan = PXCache<INItemPlan>.CreateCopy(plan);
							plan.PlanType = plantype.ReplanOnEvent;
							Caches[typeof(INItemPlan)].Update(plan);

							Caches[typeof(SOShipLineSplit)].MarkUpdated(split, assertError: true);
							Caches[typeof(SOShipLineSplit)].IsDirty = true;
						}

						if ((Caches[typeof(SOShipLine)].ObjectsEqual(prev_line, line) == false || object.Equals(line.InventoryID, split.InventoryID) == false || (line.TaskID != null && line.LocationID != split.LocationID)) && split.IsStockItem == true)
						{
							if (needInsertNewDoc)
							{
								docgraph.INRegisterDataMember.Insert(newdoc);
								needInsertNewDoc = false;
							}
							line.Released = true;
							Caches[typeof(SOShipLine)].MarkUpdated(line, assertError: true);
							Caches[typeof(SOShipLine)].IsDirty = true;

							bool artranReleased = (artran?.Released == true);
							bool shippedNotInvoicedScenario = !artranReleased && line.OrigOrderNbr != null && line.ShipmentNbr != null
									&& ordertype?.UseShippedNotInvoiced == true;
							newline = new INTran();
							newline.BranchID = shiporder.ShipmentType == SOShipmentType.Transfer ? branch.BranchID : (artranReleased ? artran.BranchID : soline.BranchID);
							newline.DocType = newdoc.DocType;
							newline.TranType = line.TranType;
							newline.SOShipmentNbr = line.ShipmentNbr;
							newline.SOShipmentType = line.ShipmentType;
							newline.SOShipmentLineNbr = line.LineNbr;
							newline.SOOrderType = line.OrigOrderType;
							newline.SOOrderNbr = line.OrigOrderNbr;
							newline.SOOrderLineNbr = line.OrigLineNbr;
							newline.SOLineType = line.LineType;
							newline.ARDocType = artran.TranType;
							newline.ARRefNbr = artran.RefNbr;
							newline.ARLineNbr = artran.LineNbr;
							newline.BAccountID = line.CustomerID;
							newline.UpdateShippedNotInvoiced = shippedNotInvoicedScenario;
							if (shippedNotInvoicedScenario)
							{
								newline.COGSAcctID = ordertype.ShippedNotInvoicedAcctID;
								newline.COGSSubID = ordertype.ShippedNotInvoicedSubID;
							}
							if (ordertype.ARDocType != ARDocType.NoUpdate)
							{
								newline.AcctID = artran.AccountID ?? soline.SalesAcctID;
								newline.SubID = artran.SubID ?? soline.SalesSubID;

								if (newline.AcctID == null)
								{
									throw new PXException(ErrorMessages.FieldIsEmpty, PXUIFieldAttribute.GetDisplayName<SOLine.salesAcctID>(Caches[typeof(SOLine)]));
								}

								if (newline.SubID == null)
								{
									throw new PXException(ErrorMessages.FieldIsEmpty, PXUIFieldAttribute.GetDisplayName<SOLine.salesSubID>(Caches[typeof(SOLine)]));
								}
							}
							newline.ProjectID = line.ProjectID;
							newline.TaskID = line.TaskID;
							newline.CostCodeID = line.CostCodeID;

							newline.IsStockItem = split.IsStockItem;
							newline.InventoryID = split.InventoryID;
							newline.SiteID = line.SiteID;
							newline.ToSiteID = Document.Current.DestinationSiteID;
							newline.InvtMult = line.InvtMult;
							newline.IsIntercompany = line.IsIntercompany;
							newline.IsSpecialOrder = soline.IsSpecialOrder;
							newline.Qty = 0m;

							if (object.Equals(line.InventoryID, split.InventoryID) == false)
							{
								newline.IsComponentItem = split.IsComponentItem;
								newline.SubItemID = split.SubItemID;
								newline.UOM = split.UOM;
								newline.UnitPrice = 0m;
								newline.UnitCost = GetNSKitComponentUnitCost(soline, line, split) ?? 0;
								newline.TranDesc = null;
							}
							else
							{
								newline.SubItemID = line.SubItemID;
								newline.UOM = line.UOM;
								newline.UnitPrice = artran.UnitPrice ?? 0m;
								newline.UnitCost = GetINTranUnitCost(soline, line, split);
								newline.TranDesc = line.TranDesc;
								newline.ReasonCode = line.ReasonCode;

								newline.OrigUOM = line.UOM;
								newline.OrigFullQty = line.ShippedQty;
								newline.BaseOrigFullQty = line.BaseShippedQty;
							}
							docgraph.CostCenterDispatcherExt?.SetCostLayerType(newline);

							PXFieldDefaulting cancelDefaulting = (c, e) => { e.Cancel = true; e.NewValue = null; };
							docgraph.FieldDefaulting.AddHandler<INTran.locationID>(cancelDefaulting);
							try
							{
							newline = docgraph.LSSelectDataMember.Insert(newline);
						}
							finally
							{
								docgraph.FieldDefaulting.RemoveHandler<INTran.locationID>(cancelDefaulting);
							}
						}

						prev_line = line;
						prev_artran = artran;

						if (split.IsStockItem == true && split.Qty != 0m)
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
							if (line.ShipmentType == SOShipmentType.Transfer)
							{
								newsplit.TransferType = INTransferType.TwoStep;
							}
							if (reattachExistingPlan)
							{
								newsplit.PlanID = plan.PlanID;
								reattachedPlans.Add(plan);
							}

							PXParentAttribute.SetParent(docgraph.INTranSplitDataMember.Cache, newsplit, typeof(INTran), newline);

							newsplit = docgraph.INTranSplitDataMember.Insert(newsplit);

							if (splitcopy.PlanID != null && demand.TryGetValue(splitcopy.PlanID, out List<INItemPlan> demandPlans))
							{
								SplitDemandAndAssignSupply(orderEntry, docgraph, line, splitcopy, newsplit, demandPlans);
							}

							if (object.Equals(line.InventoryID, split.InventoryID))
							{
								INTran copy = PXCache<INTran>.CreateCopy(newline);

								docgraph.LSSelectDataMember.Cache.SetValueExt<INTran.tranCost>(newline, newline.Qty * newline.UnitCost);

								bool signMismatch = artran.DrCr == DrCr.Credit && artran.SOOrderLineOperation == SOOperation.Receipt
									|| artran.DrCr == DrCr.Debit && artran.SOOrderLineOperation == SOOperation.Issue;

								newline.TranAmt = (signMismatch ? -artran.TranAmt : artran.TranAmt) ?? 0m;
								if (artran.Qty != null && artran.Qty != 0m && (artran.SOShipmentLineNbr == null || artran.TaskID != null))
								{
									newline.TranAmt = newline.TranAmt / artran.Qty * newline.Qty;
								}

								docgraph.LSSelectDataMember.Cache.RaiseRowUpdated(newline, copy);
							}
						}
					}
				}

				if (docgraph.LSSelectDataMember.Cache.IsDirty)
				{
					using (PXTransactionScope ts = new PXTransactionScope())
					{
						docgraph.Save.Press();
						if (orderEntry.IsValueCreated)
							orderEntry.Value.Save.Press();

						PXResultset<SOOrderShipment> res = PXSelect<SOOrderShipment,
							Where<SOOrderShipment.shipmentNbr, Equal<Required<SOOrderShipment.shipmentNbr>>,
								And<SOOrderShipment.shipmentType, Equal<Required<SOOrderShipment.shipmentType>>>>>
							.Select(this, shiporder.ShipmentNbr, shiporder.ShipmentType);

						bool shipmentReleased = true;
						foreach (SOOrderShipment item in res)
						{
							if (item.OrderType == order.OrderType && item.OrderNbr == order.OrderNbr)
							{
								item.InvtDocType = docgraph.INRegisterDataMember.Current.DocType;
								item.InvtRefNbr = docgraph.INRegisterDataMember.Current.RefNbr;
								item.InvtNoteID = docgraph.INRegisterDataMember.Current.NoteID;

								OrderList.Cache.Update(item);

								UpdatePlansRefNoteID(item, item.InvtNoteID, reattachedPlans);
							}

						shipmentReleased &= item.InvtRefNbr != null || item.CreateINDoc == false;
						}

						Document.Current.Released = shipmentReleased;
						UpdateStatusOnPostShipment(Document.Current);
						Document.Cache.MarkUpdated(Document.Current);

						this.Save.Press();

						INRegister existing;
						if ((existing = list.Find(docgraph.INRegisterDataMember.Current)) == null)
						{
							list.Add(docgraph.INRegisterDataMember.Current);
						}
						else
						{
							docgraph.INRegisterDataMember.Cache.RestoreCopy(existing, docgraph.INRegisterDataMember.Current);
						}
						ts.Complete();
					}
				}
				else
				{
					SOShipment shipment = Document.Current;

					bool isTransfer = shipment.BilledOrderCntr == 0 &&
						shipment.UnbilledOrderCntr == 0 &&
						shipment.ReleasedOrderCntr == 0;

					if (isTransfer)
					{
						bool hasOnlyNonStockItems = PXParentAttribute.SelectChildren(OrderList.Cache, shipment, typeof(SOShipment))
								.Cast<SOOrderShipment>().All(s => s.CreateINDoc != true);

						if (isTransfer && hasOnlyNonStockItems)
						{
							shipment.Released = true;
							UpdateStatusOnPostShipment(shipment);
						Document.Cache.MarkUpdated(shipment);
							Save.Press();
						}
					}
				}
			}
			finally
			{
				docgraph.RowPersisted.RemoveHandler<INTran>(ShipmentINTranRowPersisted);
			}
		}

		protected virtual void SplitDemandAndAssignSupply(Lazy<SOOrderEntry> orderEntry, INRegisterEntryBase docgraph,
			SOShipLine line, SOShipLineSplit splitcopy, INTranSplit newsplit, List<INItemPlan> demandPlans)
		{
			decimal? restShipQty = line.BaseShippedQty;
			foreach (INItemPlan demandPlan in demandPlans)
			{
				if (restShipQty >= demandPlan.PlanQty)
				{
					demandPlan.SupplyPlanID = newsplit.PlanID;
					docgraph.Caches<INItemPlan>().MarkUpdated(demandPlan, assertError: true);
					restShipQty -= demandPlan.PlanQty;
				}
				else
				{
					restShipQty -= SplitDemandAndAssignSupply(orderEntry.Value, line, demandPlan, newsplit, restShipQty);
				}
			}
		}

		protected virtual decimal? SplitDemandAndAssignSupply(SOOrderEntry orderEntry, SOShipLine line, INItemPlan demandPlan, INTranSplit newsplit, decimal? restShipQty)
		{
			if (!LoadDemandOrder(orderEntry, demandPlan))
				return 0m;
			demandPlan.SupplyPlanID = newsplit.PlanID;
			orderEntry.Caches<INItemPlan>().MarkUpdated(demandPlan, assertError: true);
			decimal? restDemandQty = demandPlan.PlanQty - restShipQty;

			// 1. create first split and assign part to the back ordered qty in the transfer order if any
			PXResult<SOLineSplit> backOrdered = SelectFrom<SOLineSplit>
				.InnerJoin<INItemPlan>.On<SOLineSplit.FK.ItemPlan>
				.Where<SOLineSplit.orderType.IsEqual<SOShipLine.origOrderType.FromCurrent>
					.And<SOLineSplit.orderNbr.IsEqual<SOShipLine.origOrderNbr.FromCurrent>
					.And<SOLineSplit.lineNbr.IsEqual<SOShipLine.origLineNbr.FromCurrent>
					.And<SOLineSplit.parentSplitLineNbr.IsEqual<SOShipLine.origSplitLineNbr.FromCurrent>
					.And<SOLineSplit.completed.IsEqual<False>>>>>>
				.View.SelectSingleBound(orderEntry, new[] { line });
			if (backOrdered != null)
			{
				INItemPlan backOrderedPlan = PXResult.Unwrap<INItemPlan>(backOrdered);
				decimal? assignedToBackOrderQty = restDemandQty <= backOrderedPlan.PlanQty ? restDemandQty : backOrderedPlan.PlanQty;

				InsertNewSOLineSplit(orderEntry, assignedToBackOrderQty, backOrderedPlan.PlanID);

				restDemandQty -= assignedToBackOrderQty;
			}
			// 2. create second split for the rest qty to create a new transfer order further
			if (restDemandQty > 0m)
			{
				InsertNewSOLineSplit(orderEntry, restDemandQty, null);

				restDemandQty = 0m;
			}

			return restShipQty;
		}

		protected virtual SOLineSplit InsertNewSOLineSplit(SOOrderEntry orderEntry, decimal? baseQty, long? supplyPlanID)
		{
			using (orderEntry.LineSplittingExt.SuppressedModeScope(true))
			{
				SOLineSplit oldSplit = orderEntry.splits.Current;
				decimal? reducedBaseQty = oldSplit.BaseQty - baseQty;
				if (reducedBaseQty <= 0m)
					return null;
				oldSplit = PXCache<SOLineSplit>.CreateCopy(oldSplit);
				oldSplit.BaseQty = reducedBaseQty;
				oldSplit.Qty = INUnitAttribute.ConvertFromBase(orderEntry.splits.Cache, oldSplit.InventoryID, oldSplit.UOM, (decimal)oldSplit.BaseQty, INPrecision.QUANTITY);
				oldSplit = orderEntry.splits.Update(oldSplit);
				var selectPlanCmd = new SelectFrom<INItemPlan>.Where<INItemPlan.planID.IsEqual<@P.AsLong>>.View(orderEntry);
				INItemPlan oldPlan = selectPlanCmd.Select(oldSplit.PlanID);
				oldPlan = PXCache<INItemPlan>.CreateCopy(oldPlan);
				oldPlan.SiteID = oldSplit.ToSiteID;

				if (oldPlan.CostCenterID != CostCenter.FreeStock)
				{
					var line = SOLineSplit.FK.OrderLine.FindParent(this, oldSplit);
					if (line == null)
						throw new Common.Exceptions.RowNotFoundException(soline.Cache, oldSplit.OrderType, oldSplit.OrderNbr, oldSplit.LineNbr);

					oldPlan.CostCenterID = line.CostCenterID;
				}

				oldPlan = (INItemPlan)orderEntry.Caches<INItemPlan>().Update(oldPlan);

				SOLineSplit newSplit = PXCache<SOLineSplit>.CreateCopy(oldSplit);
				newSplit.PlanID = null;
				newSplit.SplitLineNbr = null;
				newSplit.ShipmentNbr = null;
				if (supplyPlanID == null)
				{
					newSplit.IsAllocated = false;
					newSplit.SiteID = null;
					newSplit.CostCenterID = null;
					newSplit.ClearPOFlags();
					newSplit.ClearSOReferences();
					newSplit.POType = null;
					newSplit.PONbr = null;
					newSplit.POLineNbr = null;
					newSplit.VendorID = null;
					newSplit.RefNoteID = null;
				}
				newSplit.BaseReceivedQty = 0m;
				newSplit.ReceivedQty = 0m;
				newSplit.BaseShippedQty = 0m;
				newSplit.ShippedQty = 0m;
				newSplit.BaseQty = 0m;
				newSplit.Qty = 0m;
				newSplit = orderEntry.splits.Insert(newSplit);
				if (supplyPlanID == null)
				{
					int? saveSiteID = newSplit.SiteID;
					newSplit = PXCache<SOLineSplit>.CreateCopy(newSplit);
					newSplit.IsAllocated = true;
					newSplit.SiteID = oldSplit.SiteID;
					newSplit = orderEntry.splits.Update(newSplit);
					var accumStatus = new SiteStatusByCostCenter()
					{
						InventoryID = newSplit.InventoryID,
						SubItemID = newSplit.SubItemID,
						SiteID = newSplit.SiteID,
						CostCenterID = newSplit.CostCenterID,
					};
					var status = INSiteStatusByCostCenter.PK.Find(orderEntry, accumStatus);
					accumStatus = (SiteStatusByCostCenter)orderEntry.Caches<SiteStatusByCostCenter>().Locate(accumStatus);
					decimal qtyHardAvail = status?.QtyHardAvail ?? 0m + accumStatus?.QtyHardAvail ?? 0m;
					if (qtyHardAvail < baseQty)
					{
						// revert hard allocation if not enough stock
						newSplit = PXCache<SOLineSplit>.CreateCopy(newSplit);
						newSplit.IsAllocated = false;
						newSplit.SiteID = saveSiteID;
						newSplit = PXCache<SOLineSplit>.CreateCopy(orderEntry.splits.Update(newSplit));
						newSplit.POCreate = false;
						newSplit = orderEntry.splits.Update(newSplit);
					}
				}
				newSplit = PXCache<SOLineSplit>.CreateCopy(newSplit);
				newSplit.BaseQty = baseQty;
				newSplit.Qty = INUnitAttribute.ConvertFromBase(orderEntry.splits.Cache, newSplit.InventoryID, newSplit.UOM, (decimal)newSplit.BaseQty, INPrecision.QUANTITY);
				newSplit = orderEntry.splits.Update(newSplit);
				if (supplyPlanID != null)
				{
					INItemPlan newPlan = selectPlanCmd.Select(newSplit.PlanID);
					newPlan = PXCache<INItemPlan>.CreateCopy(newPlan);
					newPlan.SupplyPlanID = supplyPlanID;
					newPlan.PlanType = INPlanConstants.Plan93;
					newPlan.SiteID = newSplit.ToSiteID;
					newPlan.CostCenterID = oldPlan.CostCenterID;
					newPlan.FixedSource = null;
					newPlan = (INItemPlan)orderEntry.Caches<INItemPlan>().Update(newPlan);
				}
				orderEntry.splits.Current = oldSplit;
				return newSplit;
			}
		}

		protected virtual bool LoadDemandOrder(SOOrderEntry orderEntry, INItemPlan demandPlan)
		{
			SOLineSplit demandSplit = SelectFrom<SOLineSplit>.Where<SOLineSplit.planID.IsEqual<@P.AsLong>>.View.Select(orderEntry, demandPlan.PlanID);
			if (demandSplit == null)
				return false;
			orderEntry.Document.Current = orderEntry.Document.Search<SOOrder.orderNbr>(demandSplit.OrderNbr, demandSplit.OrderType);
			orderEntry.Transactions.Current = orderEntry.Transactions.Search<SOLine.orderType, SOLine.orderNbr, SOLine.lineNbr>(
				demandSplit.OrderType, demandSplit.OrderNbr, demandSplit.LineNbr);
			orderEntry.splits.Current = demandSplit;
			return true;
		}

		protected virtual void UpdateStatusOnPostShipment(SOShipment shipment)
		{
			if (shipment.UnbilledOrderCntr == 0 &&
				shipment.BilledOrderCntr == 0 &&
				shipment.ReleasedOrderCntr == 0 &&
				shipment.Released == true)
			{
				MarkCompleted(shipment);
			}
		}

		protected virtual decimal? GetINTranUnitCost(SOLine soline, SOShipLine line, SOShipLineSplit split)
		{
			if (line.Operation == SOOperation.Receipt && !string.IsNullOrEmpty(line.LotSerialNbr)
				&& InventoryItem.PK.Find(this, line.InventoryID)?.ValMethod == INValMethod.Specific)
			{
				var origINTranCosts = SelectFrom<INTranCost>
					.InnerJoin<INTran>.On<INTranCost.FK.Tran>
					.InnerJoin<ARTran>
						.On<ARTran.tranType.IsEqual<INTran.aRDocType>
							.And<ARTran.refNbr.IsEqual<INTran.aRRefNbr>>
							.And<ARTran.lineNbr.IsEqual<INTran.aRLineNbr>>>
					.Where<INTranCost.lotSerialNbr.IsEqual<@P.AsString>
						.And<ARTran.tranType.IsEqual<@P.AsString.ASCII>>
						.And<ARTran.refNbr.IsEqual<@P.AsString>>
						.And<ARTran.lineNbr.IsEqual<@P.AsInt>>>
					.View.ReadOnly.Select(this,
						line.LotSerialNbr,
						soline.InvoiceType,
						soline.InvoiceNbr,
						soline.InvoiceLineNbr)
					.RowCast<INTranCost>().ToList();
				decimal? qtySum = origINTranCosts.Sum(c => c.Qty);
				if ((qtySum ?? 0m) != 0m)
				{
					return INUnitAttribute.ConvertToBase(
						Transactions.Cache,
						line.InventoryID,
						line.UOM,
						origINTranCosts.Sum(c => c.TranCost).Value / qtySum.Value,
						INPrecision.UNITCOST);
				}
			}

			return line.UnitCost;
		}

		protected virtual decimal? GetNSKitComponentUnitCost(SOLine soline, SOShipLine line, SOShipLineSplit split)
		{
			INTran invoiced = null;

			if (line.Operation == SOOperation.Receipt)
			{
				var invoiceLines = INTran.FK.SOLine.SelectChildren(this, new SOLine
				{
					OrderType = line.OrigOrderType,
					OrderNbr = line.OrigOrderNbr,
					LineNbr = line.OrigLineNbr
				}).ToList();
				if (invoiceLines.Any())
				{
					invoiced = invoiceLines[0];
					return invoiced.UnitCost;
				}

				var branch = GL.Branch.PK.Find(this, soline.BranchID);
				var itemCost = INItemCost.PK.Find(this, split.InventoryID, branch?.BaseCuryID);
				if (itemCost != null)
					return itemCost.LastCost;
			}
			
			PXResultset<INTran> resultset = PXSelectJoin<INTran,
				InnerJoin<ARTran, On<ARTran.tranType, Equal<INTran.aRDocType>, And<ARTran.refNbr, Equal<INTran.aRRefNbr>, And<ARTran.lineNbr, Equal<INTran.aRLineNbr>>>>>,
				Where<ARTran.tranType, Equal<Required<ARTran.tranType>>,
				And<ARTran.refNbr, Equal<Required<ARTran.refNbr>>,
				And<ARTran.inventoryID, Equal<Required<ARTran.inventoryID>>,
				And<INTran.inventoryID, Equal<Required<INTran.inventoryID>>>>>>>
				.Select(this, soline.InvoiceType, soline.InvoiceNbr, line.InventoryID, split.InventoryID);
			if (!string.IsNullOrEmpty(split.LotSerialNbr))
			{
				invoiced = resultset.AsEnumerable().Where(intran => string.Equals(((INTran)intran).LotSerialNbr, split.LotSerialNbr, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
				if (invoiced != null)
					return invoiced.UnitCost;
			}
			invoiced = resultset;
			if (invoiced != null)
				return invoiced.UnitCost;

			return null;
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
					InnerJoin<SOShipLineSplit, On<SOShipLineSplit.planID, Equal<INItemPlan.planID>>>,
				Where<SOShipLineSplit.origOrderType, Equal<Required<SOShipLineSplit.origOrderType>>,
					And<SOShipLineSplit.origOrderNbr, Equal<Required<SOShipLineSplit.origOrderNbr>>,
					And<SOShipLineSplit.shipmentNbr, Equal<Required<SOShipLineSplit.shipmentNbr>>>>>>
			.Update(this,
				refNoteID,
				orderShipment.OrderType,
				orderShipment.OrderNbr,
				orderShipment.ShipmentNbr);

			var stamp = PXDatabase.SelectTimeStamp();
			foreach (var plan in reattachedPlans)
				PXTimeStampScope.PutPersisted(this.Caches[typeof(INItemPlan)], plan, stamp);
		}

		#endregion

		#region Discount

		private void AllocateGroupFreeItems(SOOrder order)
		{
			Dictionary<DiscKey, decimal> freeItems = new Dictionary<DiscKey, decimal>();
			List<SOShipLine> shipLinesToCheck = new List<SOShipLine>();
			bool freeItemPresent = false;

			foreach (SOShipLine line in Transactions.Select())
			{
				if (line.OrigOrderType == order.OrderType && line.OrigOrderNbr == order.OrderNbr && line.IsFree == false) shipLinesToCheck.Add(line);
				if (line.IsFree == true) freeItemPresent = true;
			}

			bool useBaseQty = DiscountEngine.ApplyQuantityDiscountByBaseUOMForAR(this);

			if (freeItemPresent)
			{
				soorder.Current = order;
				PXCache cache = this.Caches[typeof(SOLine)];
				PXSelectBase<SOLine> transactions = new PXSelect<SOLine, Where<SOLine.orderType, Equal<Current<SOOrder.orderType>>, And<SOLine.orderNbr, Equal<Current<SOOrder.orderNbr>>>>>(this);
				PXSelectBase<SOOrderDiscountDetail> discountdetail = new PXSelect<SOOrderDiscountDetail, Where<SOOrderDiscountDetail.orderType, Equal<Current<SOOrder.orderType>>, And<SOOrderDiscountDetail.orderNbr, Equal<Current<SOOrder.orderNbr>>>>>(this);

				TwoWayLookup<SOOrderDiscountDetail, SOLine> discountCodesWithApplicableSOLines = DiscountEngineProvider.GetEngineFor<SOLine, SOOrderDiscountDetail>()
					.GetListOfLinksBetweenDiscountsAndDocumentLines(cache, transactions, discountdetail);

				if (sosetup.Current.FreeItemShipping == FreeItemShipType.Proportional)
				{
					foreach (SOOrderDiscountDetail dsGroup in discountCodesWithApplicableSOLines.LeftValues.Where(x => x.FreeItemQty > 0m && x.SkipDiscount != true))
					{
						decimal shippedQty = 0m;
						decimal shippedGroupQty = 0m;
						foreach (SOLine soLine in discountCodesWithApplicableSOLines.RightsFor(dsGroup))
						{
							foreach (SOShipLine shipLine in shipLinesToCheck)
							{
								if (soLine.LineNbr == shipLine.OrigLineNbr)
								{
									shippedGroupQty += ((useBaseQty ? shipLine.BaseShippedQty : shipLine.ShippedQty) ?? 0m);
								}
							}
						}

						shippedQty = (shippedGroupQty * (decimal)dsGroup.FreeItemQty / (decimal)dsGroup.DiscountableQty);

						DiscKey discKey = new DiscKey(dsGroup.DiscountID, dsGroup.DiscountSequenceID, (int)dsGroup.FreeItemID);
						freeItems.Add(discKey, Math.Floor(shippedQty));
					}
				}
				else
				{
					//Ship on last shipment
					foreach (SOOrderDiscountDetail dsGroup in discountCodesWithApplicableSOLines.LeftValues.Where(x => x.FreeItemQty > 0m && x.SkipDiscount != true))
					{
						decimal shippedBOGroupQty = 0m;
						decimal orderBOGroupQty = 0m;
						decimal shippedGroupQty = 0m;
						decimal orderGroupQty = 0m;

						decimal shippedQty = 0m;
						foreach (SOLine soLine in discountCodesWithApplicableSOLines.RightsFor(dsGroup))
						{
							SOLine2 keys = new SOLine2();
							keys.OrderType = soLine.OrderType;
							keys.OrderNbr = soLine.OrderNbr;
							keys.LineNbr = soLine.LineNbr;

							SOLine2 solineWithUpdatedShippedQty = (SOLine2)this.Caches[typeof(SOLine2)].Locate(keys);
							if (solineWithUpdatedShippedQty != null)
							{
								orderGroupQty += soLine.Qty ?? 0m;
								if (soLine.ShipComplete == SOShipComplete.BackOrderAllowed)
								{
									orderBOGroupQty += soLine.Qty ?? 0m;
									if (solineWithUpdatedShippedQty.ShippedQty >= soLine.OrderQty)
									{
										if (soLine.LineNbr == solineWithUpdatedShippedQty.LineNbr)
										{
											shippedBOGroupQty += (solineWithUpdatedShippedQty.ShippedQty ?? 0m);
										}
									}
								}
								else
								{
									shippedGroupQty += solineWithUpdatedShippedQty.ShippedQty ?? 0m;
								}
							}
						}

							if (shippedGroupQty + shippedBOGroupQty < orderGroupQty)
							shippedQty = ((shippedGroupQty + shippedBOGroupQty) / (decimal)dsGroup.DiscountableQty) * (decimal)dsGroup.FreeItemQty;
							else
							shippedQty = (decimal)dsGroup.FreeItemQty;

						DiscKey discKey = new DiscKey(dsGroup.DiscountID, dsGroup.DiscountSequenceID, (int)dsGroup.FreeItemID);
						freeItems.Add(discKey, shippedBOGroupQty >= orderBOGroupQty ? Math.Floor(shippedQty) : 0m);
					}
				}

				foreach (KeyValuePair<DiscKey, decimal> kv in freeItems)
				{
					SOShipmentDiscountDetail sdd = new SOShipmentDiscountDetail();
					sdd.Type = DiscountType.Line;
					sdd.OrderType = order.OrderType;
					sdd.OrderNbr = order.OrderNbr;
					sdd.DiscountID = kv.Key.DiscID;
					sdd.DiscountSequenceID = kv.Key.DiscSeqID;
					sdd.FreeItemID = kv.Key.FreeItemID;
					sdd.FreeItemQty = kv.Value;

					UpdateInsertDiscountTrace(sdd);
				}
			}
		}

		private struct DiscKey
		{
			string discID;
			string discSeqID;
			int freeItemID;

			public string DiscID { get { return discID; } }
			public string DiscSeqID { get { return discSeqID; } }
			public int FreeItemID { get { return freeItemID; } }

			public DiscKey(string discID, string discSeqID, int freeItemID)
			{
				this.discID = discID;
				this.discSeqID = discSeqID;
				this.freeItemID = freeItemID;
			}
		}
		
		private DiscountSequence GetDiscountSequenceByID(string discountID, string discountSequenceID)
		{
			return PXSelect<DiscountSequence,
				Where<DiscountSequence.discountID, Equal<Required<DiscountSequence.discountID>>,
				And<DiscountSequence.discountSequenceID, Equal<Required<DiscountSequence.discountSequenceID>>>>>.Select(this, discountID, discountSequenceID);

		}

		private void RecalculateFreeItemQtyTotal()
		{
			if (Document.Current != null)
			{
				Document.Cache.SetValueExt<SOShipment.freeItemQtyTot>(Document.Current, SumFreeItemQtyTotal());
			}
		}

		private decimal SumFreeItemQtyTotal()
		{
			PXSelectBase<SOShipmentDiscountDetail> select =
					new PXSelect<SOShipmentDiscountDetail,
					Where<SOShipmentDiscountDetail.shipmentNbr, Equal<Current<SOShipment.shipmentNbr>>>>(this);

			decimal total = 0;
			foreach (SOShipmentDiscountDetail record in select.Select())
			{
				total += record.FreeItemQty ?? 0;
			}

			return total;
		}

		private void AdjustFreeItemLines()
		{
			foreach (SOShipLine line in Transactions.Select())
			{
				if (line.IsFree == true && line.ManualDisc != true)
					AdjustFreeItemLines(line);
			}

			Transactions.View.RequestRefresh();
		}

		private bool skipAdjustFreeItemLines = false;

		private void AdjustFreeItemLines(SOShipLine line)
		{
			if (skipAdjustFreeItemLines) return;

			PXSelectBase<SOShipmentDiscountDetail> select = new PXSelect<SOShipmentDiscountDetail,
				Where<SOShipmentDiscountDetail.shipmentNbr, Equal<Current<SOShipment.shipmentNbr>>,
				And<SOShipmentDiscountDetail.freeItemID, Equal<Required<SOShipmentDiscountDetail.freeItemID>>,
				And<SOShipmentDiscountDetail.orderType, Equal<Required<SOShipmentDiscountDetail.orderType>>,
				And<SOShipmentDiscountDetail.orderNbr, Equal<Required<SOShipmentDiscountDetail.orderNbr>>>>>>>(this);

			PXResultset<SOShipmentDiscountDetail> shipmentDiscountDetails = select.Select(line.InventoryID, line.OrigOrderType, line.OrigOrderNbr);
			if (shipmentDiscountDetails.Count != 0)
			{
				decimal? qtyTotal = 0;
				foreach (SOShipmentDiscountDetail item in shipmentDiscountDetails)
				{
					if (item.FreeItemID != null && item.FreeItemQty != null && item.FreeItemQty.Value > 0)
					{
						qtyTotal += item.FreeItemQty.Value;
					}
				}

				SOShipLine oldLine = PXCache<SOShipLine>.CreateCopy(line);
				oldLine.ShippedQty = qtyTotal;
				FreeItems.Update(oldLine);
			}
			//Note: Do not delete Free item line if its qty = 0. 
			//New free item is not inserted if the qty of the original line is increased.
		}

		private void UpdateInsertDiscountTrace(SOShipmentDiscountDetail newTrace)
		{
			SOShipmentDiscountDetail trace = PXSelect<SOShipmentDiscountDetail,
					Where<SOShipmentDiscountDetail.shipmentNbr, Equal<Current<SOShipment.shipmentNbr>>,
					And<SOShipmentDiscountDetail.orderType, Equal<Required<SOShipmentDiscountDetail.orderType>>,
					And<SOShipmentDiscountDetail.orderNbr, Equal<Required<SOShipmentDiscountDetail.orderNbr>>,
					And<SOShipmentDiscountDetail.type, Equal<Required<SOShipmentDiscountDetail.type>>,
					And<SOShipmentDiscountDetail.discountID, Equal<Required<SOShipmentDiscountDetail.discountID>>,
					And<SOShipmentDiscountDetail.discountSequenceID, Equal<Required<SOShipmentDiscountDetail.discountSequenceID>>>>>>>>>.Select(this, newTrace.OrderType, newTrace.OrderNbr, newTrace.Type, newTrace.DiscountID, newTrace.DiscountSequenceID);

			if (trace != null)
			{
				trace.DiscountableQty = newTrace.DiscountableQty;
				trace.DiscountPct = newTrace.DiscountPct;
				trace.FreeItemID = newTrace.FreeItemID;
				trace.FreeItemQty = newTrace.FreeItemQty;

				_discountEngine.UpdateDiscountDetail(DiscountDetails.Cache, DiscountDetails, trace);
			}
			else
				_discountEngine.InsertDiscountDetail(DiscountDetails.Cache, DiscountDetails, newTrace);
		}

		private bool ProrateDiscount
		{
			get
			{
				SOSetup sosetup = PXSelect<SOSetup>.Select(this);

				if (sosetup == null)
				{
					return true;//default true
				}
				else
				{
					if (sosetup.ProrateDiscounts == null)
						return true;
					else
						return sosetup.ProrateDiscounts == true;
				}

			}
		}

		#endregion

		#region Packaging into boxes

		protected virtual SOPackageEngine CreatePackageEngine()
		{
			return new SOPackageEngine(this);
		}

		#endregion

		protected virtual bool SyncLineWithOrder(SOShipLine row)
		{
			if (row.ShippedQty == 0)
			{
				var soLine = PXParentAttribute.SelectParent<SOLine2>(Transactions.Cache, row);
				if (soLine != null)
					return soLine.SiteID == row.SiteID;
			}
			return true;
		}

		protected virtual void CheckLocationTaskRule(PXCache sender, SOShipLine row)
		{
			if (row.TaskID != null)
			{
				INLocation selectedLocation = INLocation.PK.Find(this, row.LocationID);

				if (selectedLocation != null && selectedLocation.TaskID != row.TaskID && selectedLocation.TaskID != null)
				{
					sender.RaiseExceptionHandling<SOShipLine.locationID>(row, selectedLocation.LocationCD,
						new PXSetPropertyException(IN.Messages.LocationIsMappedToAnotherTask, PXErrorLevel.Warning));
				}
			}
		}
		
		[Obsolete]
		protected virtual void CheckSplitsForSameTask(PXCache sender, SOShipLine row)
		{
        }

		public virtual void ShipPackages(SOShipment shiporder)
		{
			var carrier = Carrier.PK.Find(this, shiporder.ShipVia);
			if (!UseCarrierService(shiporder, carrier))
				return;
			
			CarrierPlugin plugin = null;

			if (carrier.IsExternal == true)
			{
				plugin = CarrierPlugin.PK.Find(this, carrier.CarrierPluginID);
				if (plugin?.SiteID != null && plugin.SiteID != shiporder.SiteID)
				{
					throw new PXException(Messages.ShipViaNotApplicableToShipment, Document.Cache.GetValueExt<SOShipment.siteID>(shiporder));
				}
			}

			if (shiporder.ShippedViaCarrier != true)
			{
				ICarrierService cs = CarrierMaint.CreateCarrierService(this, shiporder.ShipVia);
				CarrierRequest cr = CarrierRatesExt.BuildRequest(shiporder);
				if (cr.Packages.Count > 0)
				{
					CarrierResult<ShipResult> result = cs.Ship(cr);

					if (result != null)
					{
						StringBuilder sb = new StringBuilder();
						foreach (Message message in result.Messages)
						{
							sb.AppendFormat("{0}:{1} ", message.Code, message.Description);
						}

						if (result.IsSuccess)
						{
							using (PXTransactionScope ts = new PXTransactionScope())
							{
								PXTransactionScope.SetSuppressWorkflow(true);
								//re-read document, do not use passed object because it contains fills from Automation that will be committed even 
								//if shipment confirmation will fail later.
								Document.Current = Document.Search<SOShipment.shipmentNbr>(shiporder.ShipmentNbr);

								decimal freightCost = 0;

								if (shiporder.UseCustomerAccount != true && (shiporder.GroundCollect != true || !this.CanUseGroundCollect(shiporder)))
								{
									freightCost = ConvertAmtToBaseCury(result.Result.Cost.Currency, arsetup.Current.DefaultRateTypeID, shiporder.ShipDate.Value, result.Result.Cost.Amount);
								}

								Document.Current.FreightCost = freightCost;
								CM.PXCurrencyAttribute.CuryConvCury<SOShipment.curyFreightCost>(Document.Cache, Document.Current);

								if (Document.Current.OverrideFreightAmount != true)
								{
									PXResultset<SOShipLine> res = Transactions.Select();
									FreightCalculator fc = CreateFreightCalculator();
									fc.ApplyFreightTerms<SOShipment, SOShipment.curyFreightAmt>(Document.Cache, Document.Current, res.Count);
								}

								Document.Current.ShippedViaCarrier = true;

								UploadFileMaintenance upload = PXGraph.CreateInstance<UploadFileMaintenance>();

								if (result.Result.Image != null)
								{
									string fileName = string.Format("High Value Report.{0}", result.Result.Format);
									FileInfo file = new FileInfo(fileName, null, result.Result.Image);
									try
									{
										upload.SaveFile(file, FileExistsAction.CreateVersion);
									}
									catch (PXNotSupportedFileTypeException exc)
									{
										throw new PXException(exc, Messages.NotSupportedFileTypeFromCarrier, result.Result.Format);
									}
									PXNoteAttribute.SetFileNotes(Document.Cache, Document.Current, file.UID.Value);
								}
								Document.Update(Document.Current);

								foreach (PackageData pd in result.Result.Data)
								{
									SOPackageDetailEx sdp = PXSelect<SOPackageDetailEx,
										Where<SOPackageDetailEx.shipmentNbr, Equal<Required<SOShipment.shipmentNbr>>,
										And<SOPackageDetailEx.lineNbr, Equal<Required<SOPackageDetailEx.lineNbr>>>>>.Select(this, shiporder.ShipmentNbr, pd.RefNbr);

									if (sdp != null)
									{
										if (pd.Image != null)
										{
											string fileName = string.Format("Label #{0}.{1}", pd.TrackingNumber, pd.Format);
											FileInfo file = new FileInfo(fileName, null, pd.Image);
											try
											{
												upload.SaveFile(file);
											}
											catch (PXNotSupportedFileTypeException exc)
											{
												throw new PXException(exc, Messages.NotSupportedFileTypeFromCarrier, pd.Format);
											}
											PXNoteAttribute.SetFileNotes(Packages.Cache, sdp, file.UID.Value);

											var pluginMethod = PXSelectorAttribute.Select<Carrier.pluginMethod>(this.carrier.Cache, carrier) as PX.Objects.CS.CarrierMethodSelectorAttribute.CarrierPluginMethod;
											string serviceMethod = $"{carrier.PluginMethod} - {pluginMethod?.Description}";
											if (serviceMethod.Length > CarrierLabelHistory.serviceMethod.Length)
											{
												serviceMethod = serviceMethod.Substring(0, CarrierLabelHistory.serviceMethod.Length);
											}
											decimal rateAmount = ConvertAmtToBaseCury(result.Result.Cost.Currency, arsetup.Current.DefaultRateTypeID, shiporder.ShipDate.Value, pd.RateAmount);

											LabelHistory.Insert(new CarrierLabelHistory()
											{
												ShipmentNbr = shiporder.ShipmentNbr,
												LineNbr = pd.RefNbr,
												PluginTypeName = plugin?.PluginTypeName,
												ServiceMethod = serviceMethod,
												RateAmount = rateAmount
											});
										}
										sdp.TrackNumber = pd.TrackingNumber;
										sdp.TrackUrl = pd.TrackingUrl;
										sdp.TrackData = pd.TrackingData;
										Packages.Update(sdp);
									}
								}

								this.Save.Press();
								ts.Complete();
							}
							Document.Cache.RestoreCopy(shiporder, Document.Current);

							//show warnings:
							if (result.Messages.Count > 0)
							{
								Document.Cache.RaiseExceptionHandling<SOShipment.curyFreightCost>(shiporder, shiporder.CuryFreightCost,
									new PXSetPropertyException(sb.ToString(), PXErrorLevel.Warning));

								PXTrace.WriteWarning(sb.ToString());
							}

						}
						else
						{
							if (!string.IsNullOrEmpty(result.RequestData))
								PXTrace.WriteError(result.RequestData);

							Document.Cache.RaiseExceptionHandling<SOShipment.curyFreightCost>(shiporder, shiporder.CuryFreightCost,
									new PXSetPropertyException(Messages.CarrierServiceError, PXErrorLevel.Error, sb.ToString()));

							throw new PXException(Messages.CarrierServiceError, sb.ToString());
						}

					}
				}
			}
		}

		public virtual void GetReturnLabels(SOShipment shiporder)
		{
			if (IsWithLabels(shiporder.ShipVia))
			{
				ICarrierService cs = CarrierMaint.CreateCarrierService(this, shiporder.ShipVia);
				CarrierRequest cr = CarrierRatesExt.BuildRequest(shiporder);

				var results = cs.GetType().FullName.IsIn("PX.FedExCarrier.FedExCarrier", "PX.UpsCarrier.UpsCarrier", "PX.UpsRestCarrier.UpsRestCarrier")
					? cr.Packages
						.ToArray()
						.Select(package =>
						{
							cr.Packages = new List<CarrierBox>() { package };
							return cs.Return(cr);
						})
						.ToArray()
					: new[] { cs.Return(cr) };

				StringBuilder warningSB = new StringBuilder();
				StringBuilder errorSB = new StringBuilder();

				foreach (var result in results.WhereNotNull())
				{
					if (result.IsSuccess)
					{
						var packagesRefNbrs = result.Result.Data.Select(t => t.RefNbr).ToHashSet();
						UploadFileMaintenance upload = PXGraph.CreateInstance<UploadFileMaintenance>();

						foreach (SOPackageDetail pd in PXSelect<SOPackageDetail, Where<SOPackageDetail.shipmentNbr, Equal<Required<SOShipment.shipmentNbr>>>>.Select(this, shiporder.ShipmentNbr))
						{
							if (packagesRefNbrs.Contains(pd.LineNbr.Value))
							{
								FileInfo returnLabelFile = GetReturnLabelFileForPackage(pd, upload);

								if (returnLabelFile != null)
								{
									foreach (NoteDoc nd in PXSelect<NoteDoc, Where<NoteDoc.noteID, Equal<Required<NoteDoc.noteID>>,
											And<NoteDoc.fileID, Equal<Required<NoteDoc.fileID>>>>>.Select(this, pd.NoteID, returnLabelFile.UID))
									{
										UploadFileMaintenance.DeleteFile(nd.FileID);
									}
								}
							}
						}

						using (PXTransactionScope ts = new PXTransactionScope())
						{
							PXTransactionScope.SetSuppressWorkflow(true);

							foreach (PackageData pd in result.Result.Data)
							{
								SOPackageDetailEx sdp =
									PXSelect<SOPackageDetailEx,
										Where<SOPackageDetailEx.shipmentNbr, Equal<Required<SOShipment.shipmentNbr>>,
												And<SOPackageDetailEx.lineNbr, Equal<Required<SOPackageDetailEx.lineNbr>>>>>
										.Select(this, shiporder.ShipmentNbr, pd.RefNbr);
								if (sdp != null)
								{
									string fileName = string.Format("{2} #{0}.{1}", pd.TrackingNumber, pd.Format, ReturnLabel);
									FileInfo file = new FileInfo(fileName, null, pd.Image);
									upload.SaveFile(file);

									sdp.ReturnTrackNumber = pd.TrackingNumber;
									PXNoteAttribute.SetFileNotes(Packages.Cache, sdp, file.UID.Value);
									Packages.Update(sdp);

									Carrier carrier = Carrier.PK.Find(this, shiporder.ShipVia);
									CarrierPlugin plugin = CarrierPlugin.PK.Find(this, carrier.CarrierPluginID);

									var pluginMethod = PXSelectorAttribute.Select<Carrier.pluginMethod>(this.carrier.Cache, carrier) as PX.Objects.CS.CarrierMethodSelectorAttribute.CarrierPluginMethod;
									string serviceMethod = $"{carrier.PluginMethod} - {pluginMethod?.Description}";
									if (serviceMethod.Length > CarrierLabelHistory.serviceMethod.Length)
									{
										serviceMethod = serviceMethod.Substring(0, CarrierLabelHistory.serviceMethod.Length);
									}
									decimal rateAmount = ConvertAmtToBaseCury(result.Result.Cost.Currency, arsetup.Current.DefaultRateTypeID, shiporder.ShipDate.Value, pd.RateAmount);

									LabelHistory.Insert(new CarrierLabelHistory()
									{
										ShipmentNbr = shiporder.ShipmentNbr,
										LineNbr = pd.RefNbr,
										PluginTypeName = plugin?.PluginTypeName,
										ServiceMethod = serviceMethod,
										RateAmount = rateAmount
									});
								}
							}

							this.Actions.PressSave();
							ts.Complete();
						}

						foreach (Message message in result.Messages)
							warningSB.AppendFormat("{0}:{1} ", message.Code, message.Description);
					}
					else
					{
						foreach (Message message in result.Messages)
							errorSB.AppendFormat("{0}:{1} ", message.Code, message.Description);
					}
				}

				if (errorSB.Length > 0 && warningSB.Length > 0)
				{
					string msg = string.Format("Errors: {0}" + Environment.NewLine + "Warnings: {1}", errorSB.ToString(), warningSB.ToString());
					Document.Cache.RaiseExceptionHandling<SOShipment.curyFreightCost>(
						shiporder,
						shiporder.CuryFreightCost,
						new PXSetPropertyException(Messages.CarrierServiceError, PXErrorLevel.Error, msg));

					throw new PXException(Messages.CarrierServiceError, msg);
				}
				else if (errorSB.Length > 0)
				{
					Document.Cache.RaiseExceptionHandling<SOShipment.curyFreightCost>(
						shiporder,
						shiporder.CuryFreightCost,
						new PXSetPropertyException(Messages.CarrierServiceError, PXErrorLevel.Error, errorSB.ToString()));

					throw new PXException(Messages.CarrierServiceError, errorSB.ToString());
				}
				else if (warningSB.Length > 0)
				{
					Document.Cache.RaiseExceptionHandling<SOShipment.curyFreightCost>(
						shiporder,
						shiporder.CuryFreightCost,
						new PXSetPropertyException(warningSB.ToString(), PXErrorLevel.Warning));
				}
			}
		}

		protected virtual FreightCalculator CreateFreightCalculator()
		{
			return new FreightCalculator(this);
		}

		public virtual void CancelPackages(SOShipment shiporder, bool isReturn = false)
		{
			if (shiporder.ShippedViaCarrier == true && IsWithLabels(shiporder.ShipVia))
			{
                SOShipment currentShipment = Document.Search<SOShipment.shipmentNbr>(shiporder.ShipmentNbr);

                ICarrierService cs = CarrierMaint.CreateCarrierService(this, currentShipment.ShipVia);

				SOPackageDetailEx sdp = PXSelect<SOPackageDetailEx,
					Where<SOPackageDetailEx.shipmentNbr, Equal<Required<SOShipment.shipmentNbr>>>>.SelectWindowed(this, 0, 1, currentShipment.ShipmentNbr);

				string trackNumber = isReturn ? sdp.ReturnTrackNumber : sdp.TrackNumber;

				if (sdp != null && !string.IsNullOrEmpty(trackNumber))
				{
					CarrierResult<string> result = cs.Cancel(trackNumber, sdp.TrackData);

					if (result != null)
					{
						StringBuilder sb = new StringBuilder();
						foreach (Message message in result.Messages)
						{
							sb.AppendFormat("{0}:{1} ", message.Code, message.Description);
						}

						//Clear Tracking numbers no matter where the call to the carrier were successfull or not

						foreach (SOPackageDetailEx pd in PXSelect<SOPackageDetailEx, Where<SOPackageDetailEx.shipmentNbr, Equal<Required<SOShipment.shipmentNbr>>>>.Select(this, currentShipment.ShipmentNbr))
						{
							pd.Confirmed = false;
							if (!isReturn)
								pd.TrackNumber = null;
							pd.ReturnTrackNumber = null;
							pd.TrackUrl = null;
							Packages.Update(pd);

							foreach (NoteDoc nd in PXSelect<NoteDoc, Where<NoteDoc.noteID, Equal<Required<NoteDoc.noteID>>>>.Select(this, pd.NoteID))
							{
								UploadFileMaintenance.DeleteFile(nd.FileID);
							}
						}

                        currentShipment.CuryFreightCost = 0;
						if (currentShipment.OverrideFreightAmount != true)
						{
							currentShipment.CuryFreightAmt = 0;
						}
                        currentShipment.ShippedViaCarrier = false;
						Document.Update(currentShipment);
						Document.Cache.RestoreCopy(shiporder, Document.Current);

						this.Save.Press();

						//Log errors if any: (Log Errors/Warnings to Trace do not return them - In processing warning are displayed as errors (( )
						//CancelPackages should not throw Exceptions since CorrectShipment follows it and must be executed.
						if (!result.IsSuccess)
						{
							//Document.Cache.RaiseExceptionHandling<SOPackageDetail.trackNumber>(shiporder, shiporder.CuryFreightCost,
							//        new PXSetPropertyException(Messages.CarrierServiceError, PXErrorLevel.Error, sb.ToString()));

							//throw new PXException(Messages.CarrierServiceError, sb.ToString());

							PXTrace.WriteWarning("Tracking Numbers and Labels for the shipment was succesfully cleared but Carrier Void Service Returned Error: " + sb.ToString());
						}
						else
						{
							//show warnings:
							if (result.Messages.Count > 0)
							{
								//Document.Cache.RaiseExceptionHandling<SOPackageDetail.trackNumber>(shiporder, shiporder.CuryFreightCost,
								//    new PXSetPropertyException(sb.ToString(), PXErrorLevel.Warning));

								PXTrace.WriteWarning("Tracking Numbers and Labels for the shipment was succesfully cleared but Carrier Void Service Returned Warnings: " + sb.ToString());
							}
						}
					}
				}
			}
		}

		protected virtual System.Threading.Tasks.Task PrintPickList(List<SOShipment> list, CancellationToken cancellationToken)
		{
			return PrintPickList(list, null, cancellationToken);
		}

		protected virtual async System.Threading.Tasks.Task PrintPickList(List<SOShipment> list, PXAdapter adapter, CancellationToken cancellationToken)
		{
			if (list.Count == 0) return;
			Document.Current = list[0];
			int? branchID;
			using (new PXReadBranchRestrictedScope())
			{
				GL.Branch company = Company.Select();
				branchID = company.BranchID;
			}

			PXReportRequiredException ex = null;
			foreach (SOShipment order in list)
			{
				order.PickListPrinted = true;
				Document.Update(order);

				if (order.Hold == true)
					this.releaseFromHold.PressWithSuppressedWorkflowPersist();
			}

			PXRowPersisted shipmentPersisted = (sender, eventArgs) =>
			{
				if (eventArgs != null && eventArgs.Row != null && eventArgs.TranStatus == PXTranStatus.Completed)
				{
					var shipment = (SOShipment)eventArgs.Row;

					if (shipment.PickListPrinted == true)
					{
						Dictionary<string, string> parameters = new Dictionary<string, string>();
						parameters["SOShipment.ShipmentNbr"] = shipment.ShipmentNbr;
						string actualReportID = new NotificationUtility(this).SearchCustomerReport(SOReports.PrintPickList, shipment.CustomerID, branchID);
						ex = PXReportRequiredException.CombineReport(ex, actualReportID, parameters);
						ex.Mode = PXBaseRedirectException.WindowMode.New;
					}
				}
			};

			RowPersisted.AddHandler<SOShipment>(shipmentPersisted);

			try
			{
				this.Save.Press();
			}
			finally
			{
				RowPersisted.RemoveHandler<SOShipment>(shipmentPersisted);
			}

			if (ex != null)
			{
				if (PXAccess.FeatureInstalled<FeaturesSet.deviceHub>())
					await SMPrintJobMaint.CreatePrintJobGroup(
						adapter,
						new NotificationUtility(this).SearchPrinter,
						SONotificationSource.Customer,
						SOReports.PrintPickList,
						Accessinfo.BranchID, ex,
						ShipmentActions.Messages.PrintPickList, cancellationToken);

				throw ex;
			}
		}

		#region Print Labels and Commercial Invoices

		public virtual void PrintCarrierLabels()
		{
			if (Document.Current == null)
				return;

			if (Document.Current.LabelsPrinted == true)
			{
				WebDialogResult result = Document.View.Ask(Document.Current, GL.Messages.Confirmation, Messages.ReprintLabels, MessageButtons.YesNo, MessageIcon.Question);
				if (result != WebDialogResult.Yes)
				{
					return;
				}
				else
				{
					PXTrace.WriteInformation("User Forced Labels Reprint for Shipment {0}", Document.Current.ShipmentNbr);
				}
			}

			LongOperationManager.StartAsyncOperation(ct => PrintCarrierLabels(ct));
		}

		public virtual async System.Threading.Tasks.Task PrintCarrierLabels(CancellationToken cancellationToken)
		{
			if (Document.Current == null)
				return;

			var args = new PrintPackageFilesArgs
			{
				Shipments = new List<SOShipment> { Document.Current },
				Adapter = CreateDummyAdapter(),
				Category = PackageFileCategory.CarrierLabel
			};

			await PrintPackageFiles(args, cancellationToken);
		}

		public virtual async System.Threading.Tasks.Task PrintCommercInvoices(CancellationToken cancellationToken)
		{
			if (Document.Current == null)
				return;

			var args = new PrintPackageFilesArgs
			{
				Shipments = new List<SOShipment> { Document.Current },
				Adapter = CreateDummyAdapter(),
				Category = PackageFileCategory.CommercialInvoice
			};

			await PrintPackageFiles(args, cancellationToken);
		}

		protected virtual void MarkPackageFilesPrinted(SOShipment shipment, PackageFileCategory category)
		{
			if (category.HasFlag(PackageFileCategory.CarrierLabel))
				shipment.LabelsPrinted = true;
			if (category.HasFlag(PackageFileCategory.CommercialInvoice))
				shipment.CommercialInvoicesPrinted = true;
		}

		protected virtual string GetPrintFormID(PackageFileCategory category)
		{
			if (category.HasFlag(PackageFileCategory.CarrierLabel))
				return SOReports.PrintLabels;
			if (category.HasFlag(PackageFileCategory.CommercialInvoice))
				return SOReports.PrintCommercialInvoices;
			return null;
		}

		public virtual void LoadPackageFiles(PrintPackageFilesArgs printArg)
		{
			Guid? userDefinedPrinterID = SMPrintJobMaint.GetPrintSettings(printArg.Adapter)?.PrinterID;

			PXReportRequiredException reportRedirect = null;
			var printerToReportsMap = new Dictionary<Guid, ShipmentRelatedReports>();

			var searchPrinter = Func.Memorize(
				(string shipVia) => SearchPrinter(SONotificationSource.Customer, printArg.PrintFormID, Accessinfo.BranchID, shipVia));

			var notificationUtility = new NotificationUtility(this);
			var searchReport = Func.Memorize(
				(int? customerID) => notificationUtility.SearchCustomerReport(printArg.PrintFormID, customerID, Accessinfo.BranchID));

			{
				UploadFileMaintenance upload = PXGraph.CreateInstance<UploadFileMaintenance>();

				foreach (SOShipment shiporder in printArg.Shipments)
				{
					Guid printerID = userDefinedPrinterID ?? searchPrinter(shiporder.ShipVia ?? string.Empty) ?? Guid.Empty;
					var reports = printerToReportsMap.Ensure(printerID, () => new ShipmentRelatedReports());

					PXResultset<SOPackageDetailEx> packagesResultset = PXSelect<SOPackageDetailEx, Where<SOPackageDetailEx.shipmentNbr, Equal<Required<SOShipment.shipmentNbr>>>>.Select(this, shiporder.ShipmentNbr);
					if (packagesResultset.Count > 0)
					{
						var firstFile = GetFirstFile(packagesResultset, printArg.Category);
						if (firstFile != null)
						{
							if (IsThermalPrinter(firstFile))
							{
								reports.LabelFiles.AddRange(LoadFiles(packagesResultset, printArg.Category));
							}
							else
							{
								reports.LaserLabels.Add(shiporder.ShipmentNbr);

								string actualReportID = searchReport(shiporder.CustomerID);
								var parameters = new Dictionary<string, string>
								{
									[$"{nameof(SOShipment)}.{nameof(SOShipment.ShipmentNbr)}"] = shiporder.ShipmentNbr
								};
								reports.ReportRedirect = PXReportRequiredException.CombineReport(reports.ReportRedirect, actualReportID, parameters);
								reportRedirect = PXReportRequiredException.CombineReport(reportRedirect, actualReportID, parameters);
								reportRedirect.Mode = PXBaseRedirectException.WindowMode.New;
							}
						}
						else
						{
							PXTrace.WriteWarning("No Label files to print for Shipment {0}", shiporder.ShipmentNbr);
						}
					}
				}

				string screenID = Accessinfo.ScreenID?.Replace(".", "") ?? "";

				if (!string.IsNullOrEmpty(screenID))
					upload.FieldDefaulting.AddHandler<UploadFile.primaryScreenID>(args => args.NewValue = screenID);

				foreach (var pair in printerToReportsMap)
				{
					ShipmentRelatedReports reports = pair.Value;

					if (reports.LabelFiles.Count > 1 && CanMerge(reports.LabelFiles))
					{
						FileInfo mergedFile = MergeFiles(reports.LabelFiles);
						reports.LabelFiles.Clear();

						if (upload.SaveFile(mergedFile))
							reports.LabelFiles.Add(mergedFile);
						else
							throw new PXException(Messages.FailedToSaveMergedFile);
					}
				}
			}

			printArg.PrinterToReportsMap = printerToReportsMap;
			printArg.RedirectToReport = reportRedirect;
		}

		public virtual async System.Threading.Tasks.Task PrintPackageFiles(PrintPackageFilesArgs printArg, CancellationToken cancellationToken)
		{
			if (printArg.Category == PackageFileCategory.None)
				return;

			printArg.PrintFormID = printArg.PrintFormID ?? GetPrintFormID(printArg.Category);

			using (PXTransactionScope ts = new PXTransactionScope())
			{
				LoadPackageFiles(printArg);

				foreach (SOShipment shiporder in printArg.Shipments)
				{
					MarkPackageFilesPrinted(shiporder, printArg.Category);
					Document.Update(shiporder);
				}

				this.Save.Press();
				ts.Complete();
			}

			if (printArg.Adapter?.MassProcess == true)
			{
				for (var i = 0; i < printArg.Shipments.Count; i++)
					PXProcessing<SOShipment>.SetInfo(i, ActionsMessages.RecordProcessed);
			}

			if (PXAccess.FeatureInstalled<FeaturesSet.deviceHub>())
				await CreatePrintJobs(printArg, cancellationToken);

			try
			{
				RedirectToFiles(printArg);
			}
			finally
			{
				printArg.PrinterToReportsMap = null;
				printArg.RedirectToReport = null;
			}
		}

		public virtual async System.Threading.Tasks.Task CreatePrintJobs(PrintPackageFilesArgs printArg, CancellationToken cancellationToken)
		{
			if (printArg.PrinterToReportsMap == null)
				return;

			var description = string.Empty;
			if (printArg.Category.HasFlag(PackageFileCategory.CarrierLabel))
				description = PXMessages.Localize(ShipmentActions.Messages.PrintLabels);
			if (printArg.Category.HasFlag(PackageFileCategory.CommercialInvoice))
				description = PXMessages.Localize(ShipmentActions.Messages.PrintCommercialInvoices);

			foreach (var (printerID, reports) in printArg.PrinterToReportsMap)
			{
				if (printerID == Guid.Empty)
					continue;

				foreach (FileInfo file in reports.LabelFiles)
				{
					await SMPrintJobMaint.CreatePrintJobForRawFile(
					printArg.Adapter,
					delegate { return printerID; }, // already found
					SONotificationSource.Customer,
					printArg.PrintFormID,
					Accessinfo.BranchID,
					new Dictionary<string, string> { { "FILEID", file.UID.ToString() } },
					description, cancellationToken);
				}

				if (reports.LaserLabels.Count > 0 && reports.ReportRedirect != null)
				{
					await SMPrintJobMaint.CreatePrintJobGroup(
					printArg.Adapter,
					delegate { return printerID; }, // already found
					SONotificationSource.Customer,
					printArg.PrintFormID,
					Accessinfo.BranchID,
					reports.ReportRedirect,
					description, cancellationToken);
				}
			}
		}

		public virtual void RedirectToFiles(PrintPackageFilesArgs printArg)
		{
			if (printArg.PrinterToReportsMap == null)
				return;

			PXReportRequiredException reportRedirect = printArg.RedirectToReport;
			PXRedirectToFileException targetFileRedirect = null;

			if (printArg.PrinterToReportsMap.Count == 1 && printArg.PrinterToReportsMap.First().Value.LabelFiles.Count == 1)
				targetFileRedirect = new PXRedirectToFileException(printArg.PrinterToReportsMap.First().Value.LabelFiles.First().UID, forceDownload: true);

			if (targetFileRedirect != null && reportRedirect != null)
				return; // can't redirect to both a file and a report

			if (targetFileRedirect != null)
				throw targetFileRedirect;

			if (reportRedirect != null)
				throw reportRedirect;
		}

		private PXAdapter CreateDummyAdapter()
		{
			return new PXAdapter(PXView.Dummy.For<SOShipment>(this))
			{
				MassProcess = true, //Device Hub require this flag to know if supported
				Arguments =
							{
								[nameof(IPrintable.PrintWithDeviceHub)] = true,
								[nameof(IPrintable.DefinePrinterManually)] = false
							}
			};
		}

		protected virtual Guid? SearchPrinter(string source, string reportID, int? branchID, string shipVia)
		{
			NotificationSetupUserOverride userSetup =
				SelectFrom<NotificationSetupUserOverride>
				.InnerJoin<NotificationSetup>.On<NotificationSetupUserOverride.FK.DefaultSetup>
				.Where<NotificationSetupUserOverride.userID.IsEqual<AccessInfo.userID.FromCurrent>
					.And<NotificationSetupUserOverride.active.IsEqual<True>>
					.And<NotificationSetup.active.IsEqual<True>>
					.And<NotificationSetup.sourceCD.IsEqual<@P.AsString>>
					.And<NotificationSetup.reportID.IsEqual<@P.AsString>>
					.And<NotificationSetupUserOverride.shipVia.IsEqual<@P.AsString>>
					.And<NotificationSetup.nBranchID.IsEqual<@P.AsInt>.Or<NotificationSetup.nBranchID.IsNull>>>
				.OrderBy<NotificationSetup.nBranchID.Desc>
				.View.Select(this, source, reportID, shipVia, branchID);
			if (userSetup?.DefaultPrinterID != null)
				return userSetup.DefaultPrinterID;

			if (source != null && reportID != null)
			{
				NotificationSetup setup =
					SelectFrom<NotificationSetup>
					.Where<NotificationSetup.active.IsEqual<True>
						.And<NotificationSetup.sourceCD.IsEqual<@P.AsString>>
						.And<NotificationSetup.reportID.IsEqual<@P.AsString>>
						.And<NotificationSetup.shipVia.IsEqual<@P.AsString>>
						.And<NotificationSetup.nBranchID.IsEqual<@P.AsInt>.Or<NotificationSetup.nBranchID.IsNull>>>
					.OrderBy<NotificationSetup.nBranchID.Desc>
					.View.SelectWindowed(this, 0, 1, source, reportID, shipVia, branchID);
				if (setup?.DefaultPrinterID != null)
					return setup.DefaultPrinterID;
			}

			return new NotificationUtility(this).SearchPrinter(source, reportID, branchID);
		}

		protected virtual bool IsWithLabels(string shipVia)
		{
			Carrier carrier = Carrier.PK.Find(this, shipVia);
			return carrier != null && carrier.IsExternal == true;
		}

		protected virtual bool IsThermalPrinter(FileInfo fileInfo)
		{
			if (System.IO.Path.HasExtension(fileInfo.Name))
			{
				string extension = System.IO.Path.GetExtension(fileInfo.Name).Substring(1).ToLower();
				if (extension.Length > 2)
				{
					string ext = extension.Substring(0, 3);
					if (ext == "zpl" || ext == "epl" || ext == "dpl" || ext == "spl" || extension == "starpl" || ext == "pdf")
						return true;
					else
						return false;
				}
				else
					return false;
			}
			else
				return false;
		}

		protected virtual PackageFileCategory GetFileCategory(FileInfo fileInfo)
		{
			var packageNoteAttribute = Packages.Cache.GetAttributesReadonly(null).OfType<SOPackageNoteAttribute>().FirstOrDefault();
			return packageNoteAttribute?.GetFileCategory(fileInfo.Name) ?? PackageFileCategory.None;
		}

		protected virtual FileInfo GetFirstFile(PXResultset<SOPackageDetailEx> packages, PackageFileCategory category)
		{
			UploadFileMaintenance upload = PXGraph.CreateInstance<UploadFileMaintenance>();

			foreach (SOPackageDetailEx package in packages)
			{
				foreach (Guid fileID in PXNoteAttribute.GetFileNotes(Packages.Cache, package))
				{
					var fileInfo = upload.GetFileWithNoData(fileID);//no need to load data

					if (category.HasFlag(GetFileCategory(fileInfo)))
						return fileInfo;
				}
			}
			return null;
		}

		protected virtual IList<FileInfo> LoadFiles(PXResultset<SOPackageDetailEx> packages, PackageFileCategory category)
		{
			var files = new List<FileInfo>();
			UploadFileMaintenance upload = PXGraph.CreateInstance<UploadFileMaintenance>();

			foreach (SOPackageDetailEx package in packages)
			{
				FileInfo firstFile = null;

				foreach (Guid fileID in PXNoteAttribute.GetFileNotes(Packages.Cache, package))
				{
					var fileInfo = upload.GetFileWithNoData(fileID);

					if (category.HasFlag(GetFileCategory(fileInfo)))
					{
						if (firstFile == null)
						{
							upload.Filter.Current.FileRevisionID = null;
							firstFile = upload.GetFile(fileID);
							files.Add(firstFile);
						}
						else
						{
							PXTrace.WriteWarning("There are more then one file attached to the package. But only first will be processed for Shipment {0}/{1}",
								package.ShipmentNbr, package.LineNbr);
						}
					}
				}
			}

			return files;
		}

		protected virtual FileInfo GetReturnLabelFileForPackage(SOPackageDetail result, UploadFileMaintenance uploadFileMaintenance)
		{
			FileInfo returnFileInfo = null;
			if (result != null)
			{
				Guid[] ids = PXNoteAttribute.GetFileNotes(Packages.Cache, result);
				if (ids.Length > 0)
				{
					for (int i = 0; i < ids.Length; i++)
					{
						FileInfo fileInfo = uploadFileMaintenance.GetFile(ids[i]);

						if (fileInfo.Name.StartsWith(ReturnLabel))
						{
							returnFileInfo = fileInfo;
							break;
						}
					}
				}
			}
			return returnFileInfo;
		}

		private readonly string[] mergeExtensions = { "zpl", "epl", "dpl", "spl", "starpl", "pdf" };

		protected virtual bool CanMerge(IList<FileInfo> files)
		{
			string previousExt = null;
			foreach (var file in files)
			{
				string fileExt = System.IO.Path.GetExtension(file.Name).ToLower();
				if (fileExt.StartsWith("."))
					fileExt = fileExt.Substring(1);
				if (string.IsNullOrEmpty(fileExt))
					return false;

				previousExt = previousExt ?? fileExt;
				if (previousExt != fileExt
				  || (!mergeExtensions.Contains(fileExt)
					&& (fileExt.Length <= 2 || !mergeExtensions.Contains(fileExt.Substring(0, 3)))))
					return false;
			}
			return true;
		}

		protected virtual FileInfo MergeFiles(IList<FileInfo> files)
		{
			FileInfo result = null;
				string extension = null;
			PdfSharp.Pdf.PdfDocument mergedPDFLabels = null;

			try
			{
				using (System.IO.MemoryStream stream = new System.IO.MemoryStream())
				{
				foreach (FileInfo file in files)
				{
					string ext = System.IO.Path.GetExtension(file.Name);

					if (extension == null)
					{
						extension = ext;
					}
					else
					{
						if (ext.ToLowerInvariant() != extension.ToLowerInvariant())
							throw new PXException(Messages.CannotMergeFiles);
					}

						if (ext.ToLowerInvariant() == ".pdf")
						{
							mergedPDFLabels = mergedPDFLabels ?? new PdfSharp.Pdf.PdfDocument();
							using (System.IO.MemoryStream mem = new System.IO.MemoryStream(file.BinData))
							{
								using (var pdfPages = PdfReader.Open(mem, PdfDocumentOpenMode.Import))
								{
									foreach (PdfSharp.Pdf.PdfPage page in pdfPages.Pages)
									{
										mergedPDFLabels.AddPage(page);
				}
			}
							}
							mergedPDFLabels.Save(stream, false);
						}
						else
						{
							stream.Write(file.BinData, 0, file.BinData.Length);
						}
					}
					mergedPDFLabels?.Close();
					string fileName = Guid.NewGuid().ToString() + extension;
					result = new FileInfo(fileName, null, stream.ToArray());
				}
			return result;
		}
			finally
			{
				mergedPDFLabels?.Dispose();
			}
		}

		#endregion

		protected virtual bool ValidateAvailablePackages()
		{
			if (string.IsNullOrEmpty(Document.Current.ShipVia))
				return false;

			var boxes = CreatePackageEngine()
				.GetBoxesByCarrierID(Document.Current.ShipVia)
				.Select(b => b.BoxID)
				.ToHashSet();

			foreach (SOPackageDetail package in Packages.Select())
			{
				if (!boxes.Contains(package.BoxID))
					return false;
			}

			return true;
		}

		public override void Persist()
		{
			try
			{
				IsSyncUnassignedScope = true;

				foreach (SOShipLine line in Transactions.Cache.Deleted
					.Concat_(Transactions.Cache.Updated)
					.Concat_(Transactions.Cache.Inserted))
				{
					this.SyncUnassigned(line);
				}
			}
			finally
			{
				IsSyncUnassignedScope = false;
			}

			base.Persist();
		}

		protected bool IsSyncUnassignedScope;

		public virtual void SyncUnassigned(SOShipLine line)
		{
			if (line.IsUnassigned != true && line.UnassignedQty == 0m || line.Operation != SOOperation.Issue)
				return;

			var item = InventoryItem.PK.Find(this, line.InventoryID.Value);
			INLotSerClass lotSerClass = null;
			if (item != null && item.StkItem == true)
			{
				lotSerClass = INLotSerClass.PK.Find(this, item.LotSerClassID);
			}
			if (lotSerClass == null || lotSerClass.IsManualAssignRequired != true)
				return;

			bool deleteUnassigned = false;
			bool recreateUnassigned = false;
			List<PXResult<Unassigned.SOShipLineSplit>> unassignedSplitRows = null;
			var linesCache = Transactions.Cache;
			if (linesCache.GetStatus(line) == PXEntryStatus.Deleted || line.UnassignedQty == 0m)
			{
				deleteUnassigned = true;
			}
			else if (splits.Cache.Updated.RowCast<SOShipLineSplit>().Any(s => s.LineNbr == line.LineNbr)
				|| splits.Cache.Deleted.RowCast<SOShipLineSplit>().Any(s => s.LineNbr == line.LineNbr)
				|| unassignedSplits.Cache.Updated.RowCast<Unassigned.SOShipLineSplit>().Any(s => s.LineNbr == line.LineNbr)
				|| unassignedSplits.Cache.Deleted.RowCast<Unassigned.SOShipLineSplit>().Any(s => s.LineNbr == line.LineNbr))
			{
				recreateUnassigned = true;
			}
			else if (!Equals(line.LocationID, linesCache.GetValueOriginal<SOShipLineSplit.locationID>(line)))
			{
				recreateUnassigned = true;
			}
			else
			{
				var insertedSplits = splits.Cache.Inserted.RowCast<SOShipLineSplit>().ToList();
				decimal? insertedSplitsQty = insertedSplits.Sum(s => s.BaseQty ?? 0m);

				unassignedSplitRows = PXSelectJoin<Unassigned.SOShipLineSplit,
					LeftJoin<INLocation, On<INLocation.locationID, Equal<Unassigned.SOShipLineSplit.locationID>>>,
					Where<Unassigned.SOShipLineSplit.shipmentNbr, Equal<Required<Unassigned.SOShipLineSplit.shipmentNbr>>,
						And<Unassigned.SOShipLineSplit.lineNbr, Equal<Required<Unassigned.SOShipLineSplit.lineNbr>>>>,
					OrderBy<Asc<INLocation.pickPriority>>>
					.Select(this, line.ShipmentNbr, line.LineNbr).ToList();
				decimal? unassignedSplitsQty = unassignedSplitRows.Sum(r => ((Unassigned.SOShipLineSplit)r).BaseQty);

				decimal? qtyToReduceUnassigned = unassignedSplitsQty - line.UnassignedQty;
				if (insertedSplitsQty <= qtyToReduceUnassigned)
				{
					var locations = new List<int>();
					var locationsAssignedQty = new Dictionary<int, decimal?>();
					foreach (SOShipLineSplit split in insertedSplits)
					{
						int locationID = split.LocationID ?? -1;
						if (!locationsAssignedQty.ContainsKey(locationID))
						{
							locations.Add(locationID);
							locationsAssignedQty.Add(locationID, 0m);
						}
						locationsAssignedQty[locationID] += split.BaseQty;
					}
					locations.Add(int.MinValue);
					locationsAssignedQty[int.MinValue] = qtyToReduceUnassigned - insertedSplitsQty;

					ApplyAssignedQty(locations, locationsAssignedQty, unassignedSplitRows, true);
					ApplyAssignedQty(locations, locationsAssignedQty, unassignedSplitRows, false);
				}
				else
				{
					recreateUnassigned = true;
				}
			}

			if (deleteUnassigned || recreateUnassigned)
			{
				this.DeleteUnassignedSplits(line, unassignedSplitRows);
			}
			line.IsUnassigned = (line.UnassignedQty != 0m);

			if (recreateUnassigned && line.IsUnassigned == true)
			{
				var remainQty = RecreateUnassignedSplits(line, lotSerClass);

				if (remainQty > 0 && line.LocationID != null)
				{
					var pars = new object[]
					{
						linesCache.GetStateExt<SOShipLine.inventoryID>(line),
						linesCache.GetStateExt<SOShipLine.subItemID>(line),
						linesCache.GetStateExt<SOShipLine.siteID>(line),
						linesCache.GetStateExt<SOShipLine.locationID>(line)
					};

					linesCache.RaiseExceptionHandling<SOShipLine.locationID>(line,
						linesCache.GetStateExt<SOShipLine.locationID>(line),
						new PXSetPropertyException(IN.Messages.StatusCheck_QtyLocationNegative, PXErrorLevel.Error, pars));

					throw new PXException(IN.Messages.StatusCheck_QtyLocationNegative, pars);
				}
			}
		}

		private void ApplyAssignedQty(
			List<int> locations, Dictionary<int, decimal?> locationsAssignedQty,
			List<PXResult<Unassigned.SOShipLineSplit>> unassignedSplitRows,
			bool onlyCoincidentLocation)
		{
			foreach (int locationID in locations)
			{
				decimal? qtyToAssign = locationsAssignedQty[locationID];
				while (qtyToAssign > 0m && unassignedSplitRows.Count > 0)
				{
					var coincidentLocIndexes = unassignedSplitRows
						.SelectIndexesWhere(r => ((Unassigned.SOShipLineSplit)r).LocationID == locationID);
					int? selectedIndex = coincidentLocIndexes.Any() ? coincidentLocIndexes.First()
						: !onlyCoincidentLocation ? unassignedSplitRows.Count - 1 : (int?)null;
					if (!selectedIndex.HasValue)
						break;

					var selectedUnassigned = unassignedSplitRows[selectedIndex.Value];
					var split = (Unassigned.SOShipLineSplit)selectedUnassigned;

					if (qtyToAssign >= split.BaseQty)
					{
						qtyToAssign -= split.BaseQty;
						unassignedSplits.Delete(split);
						unassignedSplitRows.RemoveAt(selectedIndex.Value);
					}
					else
					{
						split.BaseQty -= qtyToAssign;
						split.Qty = INUnitAttribute.ConvertFromBase(unassignedSplits.Cache, split.InventoryID, split.UOM, (decimal)split.BaseQty, INPrecision.QUANTITY);
						qtyToAssign = 0m;
						unassignedSplits.Update(split);
					}
				}
				locationsAssignedQty[locationID] = qtyToAssign;
			}
		}

		public virtual void DeleteUnassignedSplits(SOShipLine line, IEnumerable<PXResult<Unassigned.SOShipLineSplit>> unassignedSplitRows)
		{
			if (unassignedSplitRows == null)
			{
				unassignedSplitRows = PXSelect<Unassigned.SOShipLineSplit,
					Where<Unassigned.SOShipLineSplit.shipmentNbr, Equal<Required<Unassigned.SOShipLineSplit.shipmentNbr>>,
						And<Unassigned.SOShipLineSplit.lineNbr, Equal<Required<Unassigned.SOShipLineSplit.lineNbr>>>>>
					.Select(this, line.ShipmentNbr, line.LineNbr).AsEnumerable();
			}
			foreach (Unassigned.SOShipLineSplit s in unassignedSplitRows)
			{
				unassignedSplits.Cache.Delete(s);
			}
		}

		public virtual decimal? RecreateUnassignedSplits(SOShipLine line, INLotSerClass lotSerClass)
		{
			Transactions.Current = line;

			if (lotSerClass.LotSerAssign == INLotSerAssign.WhenReceived)
			{
				SOLineSplit origSplit = PXSelectReadonly<SOLineSplit,
					Where<SOLineSplit.orderType, Equal<Required<SOLineSplit.orderType>>,
						And<SOLineSplit.orderNbr, Equal<Required<SOLineSplit.orderNbr>>,
						And<SOLineSplit.lineNbr, Equal<Required<SOLineSplit.lineNbr>>,
						And<SOLineSplit.splitLineNbr, Equal<Required<SOLineSplit.splitLineNbr>>>>>>>
					.Select(this, line.OrigOrderType, line.OrigOrderNbr, line.OrigLineNbr, line.OrigSplitLineNbr);

				if (!string.IsNullOrEmpty(origSplit?.LotSerialNbr))
				{
					return CreateSplitsForAvailableLots(line.UnassignedQty, line.OrigPlanType, origSplit?.LotSerialNbr, line, lotSerClass);
				}
			}

			return CreateSplitsForAvailableNonLots(line.UnassignedQty, line.OrigPlanType, line, lotSerClass);
		}

		private decimal ConvertAmtToBaseCury(string from, string rateType, DateTime effectiveDate, decimal amount)
		{
			decimal result = amount;

			using (ReadOnlyScope rs = new ReadOnlyScope(DummyCuryInfo.Cache))
			{
				CurrencyInfo ci = new CurrencyInfo();
				ci.CuryRateTypeID = rateType;
				ci.CuryID = from;
				ci = (CurrencyInfo)DummyCuryInfo.Cache.Insert(ci);
				ci.SetCuryEffDate(DummyCuryInfo.Cache, effectiveDate);
				DummyCuryInfo.Cache.Update(ci);
				PXCurrencyAttribute.CuryConvBase(DummyCuryInfo.Cache, ci, amount, out result);
				DummyCuryInfo.Cache.Delete(ci);
			}

			return result;
		}

		private void UpdateManualFreightCost(SOShipment shipment, SOOrder order, bool linkModified, bool substract = false)
		{
			if (shipment != null && order != null)
			{
				Carrier carrier = Carrier.PK.Find(this, order.ShipVia);
				if (carrier != null && carrier.CalcMethod == CarrierCalcMethod.Manual)
				{
					if (sosetup.Current?.FreightAllocation == FreightAllocationList.FullAmount
						&& (order.ShipmentCntr > 1 || !linkModified))
						return;

					SOShipment shipmentCopy = PXCache<SOShipment>.CreateCopy(shipment);
					decimal origFreightCost = shipment.ShipmentQty > 0m ? (!substract ? (order.CuryFreightCost ?? 0m) : -(order.CuryFreightCost ?? 0m)) : 0m;
					if (sosetup.Current != null && sosetup.Current.FreightAllocation == FreightAllocationList.Prorate && order.OrderQty != null && order.OrderQty > 0)
					{
						origFreightCost = (shipment.ShipmentQty ?? 0m) / (order.OrderQty ?? 1m) * origFreightCost;
					}
					origFreightCost = substract ? (shipmentCopy.CuryFreightCost ?? 0m) - ((shipmentCopy.CuryFreightCost ?? 0m) + origFreightCost) : (shipmentCopy.CuryFreightCost ?? 0m) + origFreightCost;
					decimal curyFreightCost = 0m;
					PXCurrencyAttribute.CuryConvBase<SOOrder.curyInfoID>(soorder.Cache, order, origFreightCost, out curyFreightCost);
					shipmentCopy.CuryFreightCost = curyFreightCost;
					Document.Update(shipmentCopy);
				}
			}
		}

		public virtual decimal GetQtyThreshold(SOShipLineSplit sosplit)
		{
			decimal threshold =
				SelectFrom<SOLine>
				.InnerJoin<SOShipLine>.On<SOShipLine.FK.OrderLine>
				.Where<SOShipLine.shipmentNbr.IsEqual<@P.AsString>
					.And<SOShipLine.lineNbr.IsEqual<@P.AsInt>>>
				.View.Select(this, sosplit.ShipmentNbr, sosplit.LineNbr)
				.TopFirst?.CompleteQtyMax ?? 100m;
			return threshold / 100m;
		}

		public virtual decimal GetMinQtyThreshold(SOShipLineSplit sosplit)
		{
			decimal threshold =
				SelectFrom<SOLine>
				.InnerJoin<SOShipLine>.On<SOShipLine.FK.OrderLine>
				.Where<SOShipLine.shipmentNbr.IsEqual<@P.AsString>
					.And<SOShipLine.lineNbr.IsEqual<@P.AsInt>>>
				.View.Select(this, sosplit.ShipmentNbr, sosplit.LineNbr)
				.TopFirst?.CompleteQtyMin ?? 100m;
			return threshold / 100m;
		}

		protected virtual bool AnySelected<TSelectedField>(PXCache cache)
			where TSelectedField : IBqlField
		{
			return cache.Cached.Cast<IBqlTable>().Any(
				p => (bool?)cache.GetValue<TSelectedField>(p) == true &&
				cache.GetStatus(p).IsNotIn(PXEntryStatus.Deleted, PXEntryStatus.InsertedDeleted));
		}

		protected virtual void ValidateLineType(SOLine line, InventoryItem item, string message)
		{
			if (item.KitItem == true && item.StkItem != true && line.LineType == SOLineType.NonInventory)
			{
				throw new PXException(message, line.LineNbr, line.OrderNbr);
			}
		}

		protected virtual void MarkConfirmed(SOShipment shipment)
		{
			shipment.Confirmed = true;
			shipment.ConfirmedToVerify = false;
			shipment.Status = SOShipmentStatus.Confirmed;
		}

		protected virtual void MarkOpen(SOShipment shipment)
		{
			shipment.Confirmed = false;
			shipment.ConfirmedToVerify = true;
			shipment.Status = SOShipmentStatus.Open;

			shipment.LabelsPrinted = false;
			shipment.CommercialInvoicesPrinted = false;
		}

		protected virtual void MarkCompleted(SOShipment shipment)
		{
			shipment.Status = SOShipmentStatus.Completed;
		}

		[PXInternalUseOnly]
		protected virtual void SetSuppressWorkflowOnConfirmShipment()
			=> PXTransactionScope.SetSuppressWorkflow(true);

		[PXInternalUseOnly]
		protected virtual void SetSuppressWorkflowOnCorrectShipment()
			=> PXTransactionScope.SetSuppressWorkflow(true);

		[PXInternalUseOnly]
		protected virtual void SetSuppressWorkflowOnUpdateIN()
			=> PXTransactionScope.SetSuppressWorkflow(true);

		public class LineShipment : IEnumerable<SOShipLine>, ICollection<SOShipLine>
		{
			private List<SOShipLine> _List = new List<SOShipLine>();
			public bool AnyDeleted = false;

			#region Ctor
			public LineShipment()
			{
			}
			#endregion
			#region Implementation
			public int Count
			{
				get
				{
					return ((ICollection<SOShipLine>)_List).Count;
				}
			}

			public bool IsReadOnly
			{
				get
				{
					return ((ICollection<SOShipLine>)_List).IsReadOnly;
				}
			}

			public IEnumerator<SOShipLine> GetEnumerator()
			{
				return ((IEnumerable<SOShipLine>)_List).GetEnumerator();
			}

			IEnumerator IEnumerable.GetEnumerator()
			{
				return ((IEnumerable<SOShipLine>)_List).GetEnumerator();
			}

			public void Clear()
			{
				((ICollection<SOShipLine>)_List).Clear();
			}

			public bool Contains(SOShipLine item)
			{
				return ((ICollection<SOShipLine>)_List).Contains(item);
			}

			public void CopyTo(SOShipLine[] array, int arrayIndex)
			{
				((ICollection<SOShipLine>)_List).CopyTo(array, arrayIndex);
			}

			public bool Remove(SOShipLine item)
			{
				return ((ICollection<SOShipLine>)_List).Remove(item);
			}

			public void Add(SOShipLine item)
			{
				((ICollection<SOShipLine>)_List).Add(item);
			}
			#endregion
		}

		private class ShipmentSchedule : IComparable<ShipmentSchedule>
		{
			private int sortOrder;
			private int soLineNbr;
			private int splitLineNbr;

			public ShipmentSchedule(PXResult<SOShipmentPlan, SOLineSplit, SOLine, InventoryItem, INLotSerClass, INSite, SOShipLine> result, SOShipLine shipLine)
			{
				this.sortOrder = ((SOLine)result).SortOrder.GetValueOrDefault(1000);
				this.soLineNbr = ((SOLine)result).LineNbr.GetValueOrDefault(int.MaxValue);
				this.splitLineNbr = ((SOLineSplit)result).SplitLineNbr.GetValueOrDefault(int.MaxValue);
				this.Result = result;
				this.ShipLine = shipLine;
			}

			public PXResult<SOShipmentPlan, SOLineSplit, SOLine, InventoryItem, INLotSerClass, INSite, SOShipLine> Result { get; private set; }
			public SOShipLine ShipLine;

			public int CompareTo(ShipmentSchedule other)
			{
				int compareResult = sortOrder.CompareTo(other.sortOrder);
				if (compareResult == 0)
				{
					compareResult = soLineNbr.CompareTo(other.soLineNbr);
					if(compareResult == 0)
					{
						compareResult = splitLineNbr.CompareTo(other.splitLineNbr);
					}
				}

				return compareResult;
			}
		}

		public class OrigSOLineSplitSet : HashSet<SOShipLine>
		{
			public class SplitComparer : IEqualityComparer<SOShipLine>
			{
				public bool Equals(SOShipLine a, SOShipLine b)
				{
					return a.OrigOrderType == b.OrigOrderType && a.OrigOrderNbr == b.OrigOrderNbr
						&& a.OrigLineNbr == b.OrigLineNbr && a.OrigSplitLineNbr == b.OrigSplitLineNbr;
				}

				public int GetHashCode(SOShipLine a)
				{
					unchecked
					{
						int hash = 17;
						hash = hash * 23 + a.OrigOrderType?.GetHashCode() ?? 0;
						hash = hash * 23 + a.OrigOrderNbr?.GetHashCode() ?? 0;
						hash = hash * 23 + a.OrigLineNbr.GetHashCode();
						hash = hash * 23 + a.OrigSplitLineNbr.GetHashCode();
						return hash;
					}
				}
			}

			private SOShipLine _shipLine = new SOShipLine();

			public OrigSOLineSplitSet()
				: base(new SplitComparer())
			{
			}

			public bool Contains(SOLineSplit sls)
			{
				_shipLine.OrigOrderType = sls.OrderType;
				_shipLine.OrigOrderNbr = sls.OrderNbr;
				_shipLine.OrigLineNbr = sls.LineNbr;
				_shipLine.OrigSplitLineNbr = sls.SplitLineNbr;
				return this.Contains(_shipLine);
			}
		}

		#region Well-known extension
		public PackageDetail PackageDetailExt => FindImplementation<PackageDetail>();
		public class PackageDetail : PXGraphExtension<SOShipmentEntry>
		{
			public PXSelect<SOShipLineSplitPackage,
				Where<SOShipLineSplitPackage.shipmentNbr, Equal<Optional<SOPackageDetail.shipmentNbr>>,
				And<SOShipLineSplitPackage.packageLineNbr, Equal<Optional<SOPackageDetail.lineNbr>>>>> PackageDetailSplit;

			protected virtual void _(Events.RowSelected<SOPackageDetail> e)
			{
				PackageDetailSplit.Cache.AllowInsert = Base.Packages.AllowInsert && e.Row != null;
				PackageDetailSplit.AllowDelete = Base.Packages.AllowDelete;
				PackageDetailSplit.AllowSelect = Base.Packages.AllowSelect;
				PackageDetailSplit.AllowUpdate = Base.Packages.AllowUpdate;
			}

			protected virtual void _(Events.RowSelected<SOShipment> e)
			{
				if (e.Row == null)
					return;

				Exception packageException = null;

				if (e.Row.IsPackageContentDeleted == true)
					packageException = new PXSetPropertyException(Messages.PackageContentDeleted, PXErrorLevel.Warning);
				
				e.Cache.RaiseExceptionHandling<SOShipment.packageCount>(e.Row, null, packageException);
			}

			protected virtual void _(Events.RowInserted<SOShipLineSplitPackage> e)
			{
				Base.Document.Current.IsPackageContentDeleted = false;
				UpdateParentShipmentLine(e.Cache, e.Row, null);
			}
			protected virtual void _(Events.RowUpdated<SOShipLineSplitPackage> e) => UpdateParentShipmentLine(e.Cache, e.Row, e.OldRow);
			protected virtual void _(Events.RowDeleted<SOShipLineSplitPackage> e) => UpdateParentShipmentLine(e.Cache, null, e.Row);

			protected virtual void _(Events.FieldUpdated<SOShipLineSplitPackage, SOShipLineSplitPackage.shipmentLineNbr> e)
			{
				if (e.Row != null)
				{
					var shipmentLineSplit = PXParentAttribute.SelectParent<SOShipLineSplit>(e.Cache, e.Row);
					e.Row.InventoryID = shipmentLineSplit?.InventoryID;
					e.Row.UOM = shipmentLineSplit?.UOM;
					e.Row.PackedQty = shipmentLineSplit?.Qty - shipmentLineSplit?.PackedQty;
				}
			}

			protected virtual void _(Events.FieldVerifying<SOShipLineSplitPackage, SOShipLineSplitPackage.packedQty> e)
			{
				if (e.Row == null || e.NewValue == null) return;

				decimal adjustment = (decimal)e.NewValue - e.Row.PackedQty.GetValueOrDefault();
				var shipmentLineSplit = PXParentAttribute.SelectParent<SOShipLineSplit>(e.Cache, e.Row);
				if (shipmentLineSplit != null && shipmentLineSplit.PackedQty + adjustment > shipmentLineSplit.Qty * Base.GetQtyThreshold(shipmentLineSplit))
					throw new PXSetPropertyException(Messages.QuantityPackedExceedsShippedQuantityForLine);
			}

			protected void UpdateParentShipmentLine(PXCache sender, SOShipLineSplitPackage row, SOShipLineSplitPackage oldRow)
			{
				if (row != null && oldRow != null && row.ShipmentLineNbr == oldRow.ShipmentLineNbr)
				{
					var shipmentLineSplit = PXParentAttribute.SelectParent<SOShipLineSplit>(sender, row);
					if (shipmentLineSplit != null)
					{
						UpdateShipmentLine(sender, shipmentLineSplit, row, row.PackedQty.GetValueOrDefault() - oldRow.PackedQty.GetValueOrDefault());
					}
				}
				else
				{
					if (row != null)
					{
						var shipmentLineSplit = PXParentAttribute.SelectParent<SOShipLineSplit>(sender, row);
						if (shipmentLineSplit != null)
						{
							var shipmentLine = PXParentAttribute.SelectParent<SOShipLine>(Base.splits.Cache, shipmentLineSplit);
							var lineItem = InventoryItem.PK.Find(Base, shipmentLine.InventoryID);
							
							decimal unitPrice = shipmentLine.UnitPrice ?? 0m;
							decimal factor = 1m;
							if (lineItem.StkItem != true && lineItem.KitItem == true)
							{
								var kitComponentsCount = GetNonStockKitComponentsCount(shipmentLine, lineItem);
								factor = kitComponentsCount != 0m ? kitComponentsCount : 1m;
								unitPrice = INUnitAttribute.ConvertFromBase<SOShipLine.inventoryID>(Base.Transactions.Cache, shipmentLine, shipmentLine.UOM, unitPrice, INPrecision.NOROUND);
							}
							else
							{
								unitPrice = INUnitAttribute.ConvertFromTo<SOShipLine.inventoryID>(Base.Transactions.Cache,
								shipmentLine,
								shipmentLineSplit.UOM,
								shipmentLine.UOM,
									unitPrice,
								INPrecision.NOROUND);
							}

							row.UnitPriceFactor = PXDBPriceCostAttribute.Round(
								unitPrice * (1m - (shipmentLine.DiscPct ?? 0m)/100m) / factor);
							row.WeightFactor = factor;

							UpdateShipmentLine(sender, shipmentLineSplit, row, row.PackedQty.GetValueOrDefault());
						}
					}

					if (oldRow != null)
					{
						var shipmentLineSplit = PXParentAttribute.SelectParent<SOShipLineSplit>(sender, oldRow);
						if (shipmentLineSplit != null)
						{
							UpdateShipmentLine(sender, shipmentLineSplit, row, -oldRow.PackedQty.GetValueOrDefault());
						}
					}
				}
			}

			protected virtual decimal GetNonStockKitComponentsCount(SOShipLine shipmentLine, InventoryItem item)
			{
				if ((shipmentLine.BaseShippedQty ?? 0m) == 0m)
					return 0m;

				var lineSplits = PXParentAttribute.SelectChildren(Base.splits.Cache, shipmentLine, typeof(SOShipLine));
				return lineSplits.Sum(s => ((SOShipLineSplit)s).Qty ?? 0m) / shipmentLine.BaseShippedQty.Value;
			}

			protected void UpdateShipmentLine(PXCache sender, SOShipLineSplit shipmentLineSplit, SOShipLineSplitPackage packageDetailSplit, decimal adjustment)
			{
				if (adjustment != 0)
				{
					var shipmentLine = PXParentAttribute.SelectParent<SOShipLine>(Base.splits.Cache, shipmentLineSplit);
					var originalShipmentLine = PXCache<SOShipLine>.CreateCopy(shipmentLine);
					decimal? originalLinePackagedQty = shipmentLine.PackedQty;

					var originalShipmentLineSplit = PXCache<SOShipLineSplit>.CreateCopy(shipmentLineSplit);
					decimal? originalSplitPackagedQty = shipmentLineSplit.PackedQty;

					shipmentLineSplit.PackedQty = shipmentLineSplit.PackedQty.GetValueOrDefault() + adjustment;

					var kitSpecHelper = new NonStockKitSpecHelper(Base);

					if (kitSpecHelper.IsNonStockKit(shipmentLine.InventoryID))
					{
						var RequireShipping = Func.Memorize((int inventoryID) => InventoryItem.PK.Find(Base, inventoryID).With(item => item.StkItem == true || item.NonStockShip == true));
						var nonStockKitSpec = kitSpecHelper.GetNonStockKitSpec(shipmentLine.InventoryID.Value).Where(pair => RequireShipping(pair.Key)).ToDictionary();

						shipmentLine.PackedQty = nonStockKitSpec.Keys.Count() == 0 ? 0 : Math.Floor(decimal.Divide(shipmentLineSplit.PackedQty.Value, nonStockKitSpec[shipmentLineSplit.InventoryID.Value]));
					}
					else
					{
						shipmentLine.BasePackedQty = (shipmentLine.BasePackedQty ?? 0m) + adjustment;
						PXDBQuantityAttribute.CalcTranQty<SOShipLine.packedQty>(Base.Transactions.Cache, shipmentLine);
					}

					if (adjustment > 0 && shipmentLineSplit.PackedQty > shipmentLineSplit.PickedQty ||
						adjustment < 0 && (PXAccess.FeatureInstalled<FeaturesSet.wMSFulfillment>() == false || SOPickPackShipSetup.PK.Find(Base, Base.Accessinfo.BranchID)?.ShowPickTab == false))
					{
						shipmentLineSplit.PickedQty = shipmentLineSplit.PackedQty;
					}

					//Pattern used in LSSelect::UpdateParent - is this proper?
					Base.splits.Cache.MarkUpdated(shipmentLineSplit, assertError: true);
					Base.splits.Cache.RaiseFieldUpdated<SOShipLineSplit.packedQty>(shipmentLineSplit, originalSplitPackagedQty);
					Base.splits.Cache.RaiseRowUpdated(shipmentLineSplit, originalShipmentLineSplit);

					Base.Transactions.Cache.MarkUpdated(shipmentLine, assertError: true);
					Base.Transactions.Cache.RaiseFieldUpdated<SOShipLine.packedQty>(shipmentLine, originalLinePackagedQty);
				}
			}

			[PXOverride]
			public virtual void ShipPackages(SOShipment shiporder, Action<SOShipment> baseMethod)
			{
				Carrier carrier = Carrier.PK.Find(Base, shiporder.ShipVia);
				if (carrier != null)
				{
					if (carrier.ValidatePackedQty == true)
					{
						ValidatePackagedQuantities(shiporder);
					}

					// Automatically print return label if enabled for selected ship via when issuing
					if (carrier.IsExternal == true && shiporder.ShippedViaCarrier != true && carrier.ReturnLabel == true && shiporder.Operation == SOOperation.Issue)
					{
						Base.GetReturnLabels(shiporder);
					}
				}

				baseMethod(shiporder);
			}

			protected virtual void ValidatePackagedQuantities(SOShipment shiporder)
			{
				Base.Document.Current = Base.Document.Search<SOShipment.shipmentNbr>(shiporder.ShipmentNbr);
				if (Base.Document.Current.ShipmentType == SOShipmentType.Issue)
				{
					foreach (SOShipLine line in Base.Transactions.Select())
					{
						Base.Transactions.Current = Base.Transactions.Search<SOShipLine.shipmentNbr, SOShipLine.lineNbr>(line.ShipmentNbr, line.LineNbr);
						if (line.LineType == SOLineType.Inventory)
						{
							if (line.BaseShippedQty != line.BasePackedQty)
							{
								InventoryItem item = InventoryItem.PK.Find(Base, line.InventoryID);

								if (item.StkItem == true || item.KitItem != true)
								throw new PXException(Messages.ShipmentLineQuantityNotPacked, item?.InventoryCD.Trim());
							}

							foreach (SOShipLineSplit split in Base.splits.Select())
							{
								if (split.BaseQty != split.BasePackedQty)
								{
									InventoryItem item = InventoryItem.PK.Find(Base, line.InventoryID);
									throw new PXException(Messages.ShipmentLineQuantityNotPacked, item?.InventoryCD.Trim());
								}
							}
						}
					}
				}
			}

			public virtual void OnBeforeRecalculatePackages(Document doc)
			{
				Base.Document.Current.IsPackageContentDeleted = false;
			}

			public virtual void OnAutoPackageContentDeleted(SOShipLineSplitPackage row)
			{
				Base.Document.Current.IsPackageContentDeleted = true;
			}
		}

		public CarrierRates CarrierRatesExt => FindImplementation<CarrierRates>();
		public class CarrierRates : PX.Objects.SO.GraphExtensions.CarrierRates.CarrierRatesExtension<SOShipmentEntry, SOShipment>
		{
			protected override DocumentMapping GetDocumentMapping() => new DocumentMapping(typeof(SOShipment)) { DocumentDate = typeof(SOShipment.shipDate) };

			protected override CarrierRequest GetCarrierRequest(Document doc, UnitsType unit, List<string> methods, List<CarrierBoxEx> boxes)
			{
				var shipment = (SOShipment)Documents.Cache.GetMain(doc);

				SOShipmentAddress shipAddress = Base.Shipping_Address.Select();
				BAccount companyAccount = PXSelectJoin<BAccountR, InnerJoin<GL.Branch, On<GL.Branch.bAccountID, Equal<BAccountR.bAccountID>>>, Where<GL.Branch.branchID, Equal<Required<GL.Branch.branchID>>>>.Select(Base, Base.Accessinfo.BranchID);
				Address companyAddress = PXSelect<Address, Where<Address.addressID, Equal<Required<Address.addressID>>>>.Select(Base, companyAccount.DefAddressID);
				SOShipmentContact shipContact = Base.Shipping_Contact.Select();
				Contact companyContact = PXSelect<Contact, Where<Contact.contactID, Equal<Required<Contact.contactID>>>>.Select(Base, companyAccount.DefContactID);

				CarrierRequest cr = new CarrierRequest(unit, shipment.CuryID);
				cr.Shipper = companyAddress;
				cr.Origin = null;
				cr.Destination = shipAddress;
				cr.PackagesEx = boxes;
				cr.Resedential = shipment.Resedential == true;
				cr.SaturdayDelivery = shipment.SaturdayDelivery == true;
				cr.Insurance = shipment.Insurance == true;
				cr.ShipDate = Tools.Max(Base.Accessinfo.BusinessDate.Value.Date, shipment.ShipDate.Value);
				cr.Methods = methods;
				cr.Attributes = new List<string>();
				cr.InvoiceLineTotal = Base.Document.Current.LineTotal.GetValueOrDefault();
				cr.ShipperContact = companyContact;
				cr.DestinationContact = shipContact;

				if (shipment.GroundCollect == true && Base.CanUseGroundCollect(shipment))
					cr.Attributes.Add("COLLECT");

				return cr;
			}

			protected override IEnumerable<Tuple<ILineInfo, InventoryItem>> GetLines(Document doc)
			{
				var shipment = (SOShipment)Documents.Cache.GetMain(doc);

				return
					PXSelectJoin<SOShipLine,
					InnerJoin<InventoryItem, On<SOShipLine.FK.InventoryItem>>,
					Where<SOShipLine.FK.Shipment.SameAsCurrent>,
					OrderBy<Asc<SOShipLine.shipmentType, Asc<SOShipLine.shipmentNbr, Asc<SOShipLine.lineNbr>>>>>
					.SelectMultiBound(Base, new object[] { shipment }).AsEnumerable()
					.Cast<PXResult<SOShipLine, InventoryItem>>()
					.Select(r => Tuple.Create<ILineInfo, InventoryItem>(new LineInfo(r), r));
			}

			protected override IList<SOPackageEngine.PackSet> GetPackages(Document doc, bool suppressRecalc = false)
			{
				var shipment = (SOShipment)Documents.Cache.GetMain(doc);

				SOPackageEngine.PackSet set = new SOPackageEngine.PackSet(shipment.SiteID.Value);
				foreach (SOPackageDetailEx package in Base.Packages.View.SelectMultiBound(new object[] { shipment }))
					set.Packages.Add(package.ToPackageInfo(shipment.SiteID.Value));

				return set.AsSingleEnumerable().ToList();
			}

			protected override void ClearPackages(Document doc)
			{
				foreach (SOPackageDetailEx package in Base.Packages.View.SelectMultiBound(new object[] { Documents.Cache.GetMain(doc) }))
					Base.Packages.Delete(package);
			}

			protected override void InsertPackages(IEnumerable<SOPackageInfoEx> packages)
			{
				foreach (SOPackageInfoEx package in packages)
					Base.Packages.Insert(package.ToPackageDetail(SOPackageType.Auto).Apply(d => d.ShipmentNbr = Base.Document.Current.ShipmentNbr));
			}

			protected override void RecalculatePackagesForOrder(Document doc)
			{
				if (Base.Document.Current != null)
				{
					if (Base.Document.Current.Released == true || Base.Document.Current.Confirmed == true)
						throw new PXException(Messages.PackagesRecalcErrorReleasedDocument);

					if (Base.Document.Current.SiteID == null)
						throw new PXException(Messages.PackagesRecalcErrorWarehouseIdNotSpecified);

					Base.PackageDetailExt.OnBeforeRecalculatePackages(doc);

					PXRowDeleted packageContentDeleted = (s,e) =>
						Base.PackageDetailExt.OnAutoPackageContentDeleted((SOShipLineSplitPackage)e.Row);

					int packageCount = 0;
					decimal weightTotal = 0;
					SOPackageEngine.PackSet manualPackSet;
					IList<SOPackageEngine.PackSet> packsets = CalculatePackages(Base.Document.Current, out manualPackSet);

					try
					{
						Base.RowDeleted.AddHandler<SOShipLineSplitPackage>(packageContentDeleted);

						foreach (SOPackageDetailEx package in Base.Packages.Select())
						{
							if (manualPackSet.Packages.Count == 0 && package.PackageType != SOPackageType.Auto)
							{
								weightTotal += package.Weight.GetValueOrDefault();
								packageCount++;
								continue;
							}
							Base.Packages.Delete(package);
						}
					}
					finally
					{
						Base.RowDeleted.RemoveHandler<SOShipLineSplitPackage>(packageContentDeleted);
					}

					foreach (SOPackageEngine.PackSet ps in packsets)
					{
						foreach (SOPackageInfoEx package in ps.Packages)
						{
							weightTotal += package.GrossWeight.GetValueOrDefault();

							SOPackageDetailEx detail = new SOPackageDetailEx();
							detail.PackageType = SOPackageType.Auto;
							detail.ShipmentNbr = Base.Document.Current.ShipmentNbr;
							detail.BoxID = package.BoxID;
							detail.Weight = package.GrossWeight;
							detail.WeightUOM = package.WeightUOM;
							detail.Qty = package.Qty;
							detail.QtyUOM = package.QtyUOM;
							detail.InventoryID = package.InventoryID;
							detail.DeclaredValue = package.DeclaredValue;

							detail = Base.Packages.Insert(detail);
							detail.Confirmed = false;
							packageCount++;
						}
					}

					foreach (SOPackageInfoEx package in manualPackSet.Packages)
					{
						weightTotal += package.GrossWeight.GetValueOrDefault();

						SOPackageDetailEx detail = new SOPackageDetailEx();
						detail.PackageType = SOPackageType.Manual;
						detail.ShipmentNbr = Base.Document.Current.ShipmentNbr;
						detail.BoxID = package.BoxID;
						detail.Weight = package.GrossWeight;
						detail.WeightUOM = package.WeightUOM;
						detail.Qty = package.Qty;
						detail.QtyUOM = package.QtyUOM;
						detail.InventoryID = package.InventoryID;
						detail.DeclaredValue = package.DeclaredValue;
						detail.Height = package.Height;
						detail.Width = package.Width;
						detail.Length = package.Length;

						detail = Base.Packages.Insert(detail);
						detail.Confirmed = false;
						packageCount++;
					}

					Base.Document.Current.IsPackageValid = true;
					Base.Document.Current.RecalcPackagesReason = SOShipment.recalcPackagesReason.None;
					Base.Document.Current.PackageWeight = weightTotal;
					Base.Document.Current.PackageCount = packageCount;

					Base.Document.Update(Base.Document.Current);
				}
			}

			protected virtual IList<SOPackageEngine.PackSet> CalculatePackages(SOShipment shipment, out SOPackageEngine.PackSet manualPackSet)
			{
				Dictionary<string, SOPackageEngine.ItemStats> stats = new Dictionary<string, SOPackageEngine.ItemStats>();

				PXSelectBase<SOPackageInfoEx> selectManual = new PXSelect<SOPackageInfoEx,
						Where<SOPackageInfoEx.orderType, Equal<Required<SOOrder.orderType>>,
						And<SOPackageInfoEx.orderNbr, Equal<Required<SOOrder.orderNbr>>,
						And<SOPackageInfoEx.siteID, Equal<Required<SOPackageInfoEx.siteID>>>>>>(Base);

				SOPackageEngine.OrderInfo orderInfo = new SOPackageEngine.OrderInfo(shipment.ShipVia);

				manualPackSet = new SOPackageEngine.PackSet(shipment.SiteID.Value);
				List<string> processedManualPackageOrders = new List<string>();
				foreach (SOShipLine line in Base.Transactions.View.SelectMultiBound(new object[] { shipment }))
				{
					SOOrder order = (SOOrder)PXParentAttribute.SelectParent(Base.Transactions.Cache, line, typeof(SOOrder));

					if (PXAccess.FeatureInstalled<FeaturesSet.autoPackaging>() == false || order?.IsManualPackage == true)
					{
						string key = string.Format("{0}.{1}.{2}", order.OrderType, order.OrderNbr, shipment.SiteID);
						if (!processedManualPackageOrders.Contains(key))
						{
							foreach (SOPackageInfoEx box in selectManual.Select(order.OrderType, order.OrderNbr, shipment.SiteID))
							{
								// DeclaredValue from Sales Order should be converted to base currency.
								decimal baseCuryDeclaredValue;
								PXDBCurrencyAttribute.CuryConvBase<SOOrder.curyInfoID>(
									Base.soorder.Cache, order, box.DeclaredValue ?? 0m, out baseCuryDeclaredValue);

								box.DeclaredValue = baseCuryDeclaredValue;
								manualPackSet.Packages.Add(box);
							}
							processedManualPackageOrders.Add(key);
						}
					}
					else
					{
						InventoryItem item = InventoryItem.PK.Find(Base, line.InventoryID);

						if (item.PackageOption == INPackageOption.Manual)
							continue;

						orderInfo.AddLine(item, line.BaseQty);


						int inventoryID = item.PackSeparately == true
							? item.InventoryID.Value
							: SOPackageEngine.ItemStats.Mixed;

						string key = string.Format("{0}.{1}.{2}.{3}", line.SiteID, inventoryID, item.PackageOption, line.Operation);

						SOPackageEngine.ItemStats stat;
						if (stats.ContainsKey(key))
						{
							stat = stats[key];
							stat.BaseQty += line.BaseQty.GetValueOrDefault();
							stat.BaseWeight += line.ExtWeight.GetValueOrDefault();
							stat.DeclaredValue += line.LineAmt ?? 0m;
							stat.AddLine(item, line.BaseQty);
						}
						else
						{
							stat = new SOPackageEngine.ItemStats();
							stat.SiteID = line.SiteID;
							stat.InventoryID = inventoryID;
							stat.Operation = line.Operation;
							stat.PackOption = item.PackageOption;
							stat.BaseQty += line.BaseQty.GetValueOrDefault();
							stat.BaseWeight += line.ExtWeight.GetValueOrDefault();
							stat.DeclaredValue += line.LineAmt ?? 0m;
							stat.AddLine(item, line.BaseQty);
							stats.Add(key, stat);
						}
					}
				}
				orderInfo.Stats.AddRange(stats.Values);

				SOPackageEngine engine = CreatePackageEngine();
				return engine.Pack(orderInfo);
			}


			protected virtual IList<CarrierBox> GetPackages(SOShipment shiporder, Carrier carrier, CarrierPlugin plugin)
			{
				List<CarrierBox> list = new List<CarrierBox>();

				PXSelectBase<SOPackageDetailEx> select = new PXSelect<SOPackageDetailEx,
				Where<SOPackageDetailEx.shipmentNbr, Equal<Required<SOShipment.shipmentNbr>>>>(Base);

				bool failed = false;
				foreach (SOPackageDetailEx detail in select.Select(shiporder.ShipmentNbr))
				{
					if (carrier.ConfirmationRequired == true)
					{
						if (detail.Confirmed != true)
						{
							failed = true;

							Base.Packages.Cache.RaiseExceptionHandling<SOPackageDetail.confirmed>(detail, detail.Confirmed,
								new PXSetPropertyException(Messages.ConfirmationIsRequired, PXErrorLevel.Error));
						}
					}

					list.Add(BuildCarrierPackage(detail, plugin));
				}

				if (failed)
				{
					throw new PXException(Messages.ConfirmationIsRequired);
				}

				return list;
			}

			public virtual CarrierBox BuildCarrierPackage(SOPackageDetailEx detail, CarrierPlugin plugin)
			{
				CarrierBox box = new CarrierBox(detail.LineNbr.Value, ConvertWeightValue(detail.Weight ?? 0, plugin));
				box.Description = detail.Description;
				box.DeclaredValue = detail.DeclaredValue ?? 0;
				box.COD = detail.COD ?? 0;
				box.Length = ConvertLinearValue(detail.Length ?? 0, plugin);
				box.Width = ConvertLinearValue(detail.Width ?? 0, plugin);
				box.Height = ConvertLinearValue(detail.Height ?? 0, plugin);
				box.CarrierPackage = detail.CarrierBox;
				box.CustomRefNbr1 = detail.CustomRefNbr1;
				box.CustomRefNbr2 = detail.CustomRefNbr2;

				return box;
			}

			public virtual CarrierRequest BuildRequest(SOShipment shiporder)
			{
				INSite warehouse = INSite.PK.Find(Base, shiporder.SiteID);
				if (warehouse == null)
				{
					Base.Document.Cache.RaiseExceptionHandling<SOShipment.siteID>(shiporder, shiporder.SiteID,
								new PXSetPropertyException(Messages.WarehouseIsRequired, PXErrorLevel.Error));

					throw new PXException(Messages.WarehouseIsRequired);
				}

				SOShipmentAddress shipAddress = PXSelect<SOShipmentAddress, Where<SOShipmentAddress.addressID, Equal<Required<SOShipment.shipAddressID>>>>.Select(Base, shiporder.ShipAddressID);
				SOShipmentContact shipToContact = PXSelect<SOShipmentContact, Where<SOShipmentContact.contactID, Equal<Required<SOShipment.shipContactID>>>>.Select(Base, shiporder.ShipContactID);
				Address warehouseAddress = PXSelect<Address, Where<Address.addressID, Equal<Required<Address.addressID>>>>.Select(Base, warehouse.AddressID);
				Contact warehouseContact = PXSelect<Contact, Where<Contact.contactID, Equal<Required<Contact.contactID>>>>.Select(Base, warehouse.ContactID);
				PXResult<BAccountR, GL.Branch, GL.DAC.Organization> result = (PXResult<BAccountR, GL.Branch, GL.DAC.Organization>)
																			PXSelectJoin<BAccountR,
																			InnerJoin<GL.Branch, On<GL.Branch.bAccountID, Equal<BAccountR.bAccountID>>,
																			InnerJoin<GL.DAC.Organization, On<GL.DAC.Organization.organizationID, Equal<GL.Branch.organizationID>>>>, 
																			Where<GL.Branch.branchID, Equal<Required<GL.Branch.branchID>>>>.Select(Base, warehouse.BranchID);
				BAccount companyAccount = result;
				GL.Branch branch = result;
				GL.DAC.Organization organization = result;

				Address shipperAddress = PXSelect<Address, Where<Address.addressID, Equal<Required<Address.addressID>>>>.Select(Base, companyAccount.DefAddressID);
				Contact shipperContact = PXSelect<Contact, Where<Contact.contactID, Equal<Required<Contact.contactID>>>>.Select(Base, companyAccount.DefContactID);

				Carrier carrier = Carrier.PK.Find(Base, shiporder.ShipVia);
				CarrierPlugin plugin = CarrierPlugin.PK.Find(Base, carrier.CarrierPluginID);
				ValidatePlugin(plugin);
				
				CarrierRequest cr = new CarrierRequest(GetUnitsType(plugin), shiporder.CuryID);
				cr.Attributes = new List<string>();

				Location customerLocation = PXSelect<Location, Where<Location.bAccountID, Equal<Required<Location.bAccountID>>, And<Location.locationID, Equal<Required<Location.locationID>>>>>.Select(Base, shiporder.CustomerID, shiporder.CustomerLocationID);

				bool useGroundCollect = (shiporder.GroundCollect == true && Base.CanUseGroundCollect(shiporder));
				if (useGroundCollect || shiporder.UseCustomerAccount == true)
				{
					//by customer and location
					CarrierPluginCustomer cpc = PXSelect<CarrierPluginCustomer,
						Where<CarrierPluginCustomer.carrierPluginID, Equal<Required<CarrierPluginCustomer.carrierPluginID>>,
							And<CarrierPluginCustomer.customerID, Equal<Required<CarrierPluginCustomer.customerID>>,
							And<CarrierPluginCustomer.isActive, Equal<True>,
							And<Where<CarrierPluginCustomer.customerLocationID, Equal<Required<CarrierPluginCustomer.customerLocationID>>, Or<CarrierPluginCustomer.customerLocationID, IsNull>>>>>>,
						OrderBy<Desc<CarrierPluginCustomer.customerLocationID>>>
						.Select(Base, plugin.CarrierPluginID, shiporder.CustomerID, shiporder.CustomerLocationID);

					if (!string.IsNullOrEmpty(cpc?.CarrierAccount))
					{
						cr.ThirdPartyAccountID = cpc.CarrierAccount;

						Address customerAddress = PXSelect<Address, Where<Address.addressID, Equal<Required<Address.addressID>>>>.Select(Base, customerLocation.DefAddressID);
						cr.ThirdPartyPostalCode = cpc.PostalCode ?? customerAddress.PostalCode;
						cr.ThirdPartyCountryCode = cpc.CountryID ?? customerAddress.CountryID;
					}
					else if (shiporder.UseCustomerAccount == true)
					{
						throw new PXException(Messages.CustomeCarrierAccountIsNotSetup);
					}

					if (shiporder.UseCustomerAccount == true && cpc?.CarrierBillingType == CarrierBillingTypes.Receiver)
					{
						cr.Attributes.Add("RECEIVER");
					}
				}

				decimal freightCharge = 0m;
				if (shiporder.FreightAmountSource == FreightAmountSourceAttribute.OrderBased)
				{
					IEnumerable<SOOrderShipment> sOOrderShipments = Base.OrderListSimple.Select().RowCast<SOOrderShipment>();

					//if the freight amount is based on Sales Order and the shipment has multiple SO or if it is a partial shipment, then add attribute "SKIPFREIGHTCHARGE"
					//don't send the overriden freight price field in this case. Otherwise send FreightAmt + PremiumFreightAmt from SO
					if (sOOrderShipments.Count() == 1)
					{
						SOOrderShipment soOrderShipment = sOOrderShipments.FirstOrDefault();
						SOOrder order = Base.soorder.Select(soOrderShipment?.OrderType, soOrderShipment?.OrderNbr);

						if (order?.OrderQty == soOrderShipment?.ShipmentQty)
						{
							freightCharge = (order?.FreightAmt ?? 0m) + (order?.PremiumFreightAmt ?? 0m);
						}
						else
						{
							cr.Attributes.Add("SKIPFREIGHTCHARGE");
						}
					}
					else
					{
						cr.Attributes.Add("SKIPFREIGHTCHARGE");
					}
				}
				else
				{
					freightCharge = shiporder.FreightAmt ?? 0m;
				}

				cr.FreightCharge = freightCharge;

				var soOrderList = Base.OrderList.Select();
				SOAddress billToAddress = soOrderList.RowCast<SOAddress>().FirstOrDefault();
				SOContact billToContact = soOrderList.RowCast<SOContact>().FirstOrDefault();

				if (soOrderList.Count() == 1)
				{
					cr.BillToAddress = billToAddress;
					cr.BillToContact = billToContact;
				}

				cr.Shipper = shipperAddress;
				cr.ShipperContact = shipperContact;
				cr.Origin = warehouseAddress;
				cr.OriginContact = warehouseContact;
				cr.Destination = shipAddress;
				cr.DestinationContact = shipToContact;
				cr.Packages = GetPackages(shiporder, carrier, plugin);
				cr.Resedential = shiporder.Resedential == true;
				cr.SaturdayDelivery = shiporder.SaturdayDelivery == true;
				cr.Insurance = shiporder.Insurance == true;
				cr.ShipDate = Tools.Max(Base.Accessinfo.BusinessDate.Value.Date, shiporder.ShipDate.Value.Date);
				cr.ReceiverTaxID = customerLocation?.TaxRegistrationID;
				cr.ShipperTaxID = companyAccount.TaxRegistrationID;

				if (useGroundCollect)
				{
					cr.Attributes.Add("COLLECT");
				}
				cr.InvoiceLineTotal = shiporder.LineTotal.GetValueOrDefault();

				if (!string.IsNullOrWhiteSpace(warehouse.CarrierFacility))
				{
					cr.Attributes.Add(string.Concat("CarrierFacilityW:", warehouse.CarrierFacility));
				}

				if (!string.IsNullOrWhiteSpace(branch.CarrierFacility))
				{
					cr.Attributes.Add(string.Concat("CarrierFacilityB:", branch.CarrierFacility));
				}
				else if (!string.IsNullOrWhiteSpace(organization.CarrierFacility))
				{
					cr.Attributes.Add(string.Concat("CarrierFacilityB:", organization.CarrierFacility));
				}

				return cr;
			}

			protected override WebDialogResult AskForRateSelection() => Base.CurrentDocument.AskExt();

			protected virtual void _(Events.FieldUpdated<SOPackageDetailEx, SOPackageDetailEx.boxID> e)
			{
				if (e.Row != null)
				{
					Base.Packages.Cache.SetDefaultExt<SOPackageDetailEx.boxDescription>(e.Row);
					Base.Packages.Cache.SetDefaultExt<SOPackageDetailEx.carrierBox>(e.Row);
					Base.Packages.Cache.SetDefaultExt<SOPackageDetailEx.length>(e.Row);
					Base.Packages.Cache.SetDefaultExt<SOPackageDetailEx.width>(e.Row);
					Base.Packages.Cache.SetDefaultExt<SOPackageDetailEx.height>(e.Row);
					Base.Packages.Cache.SetDefaultExt<SOPackageDetailEx.boxWeight>(e.Row);
					Base.Packages.Cache.SetDefaultExt<SOPackageDetailEx.maxWeight>(e.Row);
				}
			}

			[PXOverride]
			public virtual void Persist(Action baseMtd)
			{
				if (Base.Document.Current != null && Base.Document.Current.IsPackageValid != true &&
					Base.Document.Current.Released != true && Base.Document.Current.Confirmed != true && Base.Document.Current.SiteID != null)
				{
					if (Base.Document.Current.RecalcPackagesReason == SOShipment.recalcPackagesReason.ShipVia && Base.ValidateAvailablePackages())
					{
						foreach (SOPackageDetail package in Base.Packages.Select())
						{
							if (package.PackageType == SOPackageType.Auto)
								package.Confirmed = false;
						}

						Base.Document.Current.IsPackageValid = true;
					}
					else
					{
					recalculatePackages.Press();
				}
				}

				baseMtd();
			}

			protected override IEnumerable<CarrierPlugin> GetApplicableCarrierPlugins()
			{
				return PXSelectReadonly<CarrierPlugin,
					Where<CarrierPlugin.siteID, IsNull, Or<CarrierPlugin.siteID, Equal<Current<SOShipment.siteID>>>>>
					.Select(Base)
					.RowCast<CarrierPlugin>();
			}

			private class LineInfo : ILineInfo
			{
				private SOShipLine _line;
				public LineInfo(SOShipLine line) { _line = line; }

				public decimal? BaseQty => _line.BaseQty;
				public decimal? CuryLineAmt => _line.LineAmt;
				public decimal? ExtWeight => _line.ExtWeight;
				public int? SiteID => _line.SiteID;
				public string Operation => _line.Operation;
			}
		}

		public CartSupport CartSupportExt => FindImplementation<CartSupport>();
		public class CartSupport : PXGraphExtension<SOShipmentEntry>
		{
			public static bool IsActive() => PXAccess.FeatureInstalled<FeaturesSet.wMSCartTracking>();

			public SelectFrom<SOShipmentSplitToCartSplitLink>.Where<SOShipmentSplitToCartSplitLink.FK.Shipment.SameAsCurrent>.View ShipmentCartLinks;
			public SelectFrom<SOPickListEntryToCartSplitLink>.View PickListCartLinks;
			public SelectFrom<INCartSplit>.View CartLinks;

			public virtual void RemoveItemsFromCart()
			{
				var links =
					SelectFrom<SOShipLineSplit>.
					InnerJoin<SOShipmentSplitToCartSplitLink>.On<SOShipmentSplitToCartSplitLink.FK.ShipmentSplitLine>.
					InnerJoin<INCartSplit>.On<SOShipmentSplitToCartSplitLink.FK.CartSplit>.
					Where<SOShipLineSplit.FK.Shipment.SameAsCurrent>.
					View
					.Select(Base)
					.AsEnumerable()
					.Cast<PXResult<SOShipLineSplit, SOShipmentSplitToCartSplitLink, INCartSplit>>()
					.ToArray();

				foreach ((var sosplit, var link, var cartsplit) in links)
				{
					decimal cartQty = Math.Min(sosplit.Qty.Value, link.Qty.Value);

					var actualLink = ShipmentCartLinks.Locate(link) ?? link;
					actualLink.Qty -= cartQty;
					if (actualLink.Qty <= 0)
						ShipmentCartLinks.Delete(actualLink);
					else
						ShipmentCartLinks.Update(actualLink);

					var actualCartSplit = CartLinks.Locate(cartsplit) ?? cartsplit;
					actualCartSplit.Qty -= cartQty;
					if (actualCartSplit.Qty <= 0)
						CartLinks.Delete(actualCartSplit);
					else
						CartLinks.Update(actualCartSplit);
				}
			}

			public virtual SOShipmentSplitToCartSplitLink TransformCartLinks(SOShipLineSplit shipSplit, IReadOnlyCollection<SOPickListEntryToCartSplitLink> pickerCartLinks)
			{
				if (pickerCartLinks.Any(link => link.Qty > 0))
				{
					var firstOldLink = pickerCartLinks.Where(link => link.Qty > 0).First();

					SOShipmentSplitToCartSplitLink existingLink = ShipmentCartLinks.Search<
						SOShipmentSplitToCartSplitLink.shipmentNbr,
						SOShipmentSplitToCartSplitLink.shipmentLineNbr,
						SOShipmentSplitToCartSplitLink.shipmentSplitLineNbr,
						SOShipmentSplitToCartSplitLink.siteID,
						SOShipmentSplitToCartSplitLink.cartID,
						SOShipmentSplitToCartSplitLink.cartSplitLineNbr>(
						shipSplit.ShipmentNbr,
						shipSplit.LineNbr,
						shipSplit.SplitLineNbr,
						firstOldLink.SiteID,
						firstOldLink.CartID,
						firstOldLink.CartSplitLineNbr);

					if (existingLink == null)
					{
					var newLink = new SOShipmentSplitToCartSplitLink
					{
						ShipmentNbr = shipSplit.ShipmentNbr,
						ShipmentLineNbr = shipSplit.LineNbr,
						ShipmentSplitLineNbr = shipSplit.SplitLineNbr,

							SiteID = firstOldLink.SiteID,
							CartID = firstOldLink.CartID,
							CartSplitLineNbr = firstOldLink.CartSplitLineNbr,
						Qty = 0m
					};
						existingLink = ShipmentCartLinks.Insert(newLink);
					}

					decimal linkRestQty = shipSplit.Qty.Value;
					foreach (var oldLink in pickerCartLinks.Where(link => link.Qty > 0))
					{
						if (linkRestQty == 0)
							break;

						decimal linkQty = Math.Min(oldLink.Qty.Value, linkRestQty);

						existingLink.Qty += linkQty;
						oldLink.Qty -= linkQty;
						if (oldLink.Qty > 0)
							PickListCartLinks.Update(oldLink);
						else
							PickListCartLinks.Delete(oldLink);

						linkRestQty -= linkQty;
					}

					return ShipmentCartLinks.Update(existingLink);
				}

				return null;
			}
		}

		public WorkLog WorkLogExt => FindImplementation<WorkLog>();
		public class WorkLog : GraphExtensions.ShipmentWorkLog<SOShipmentEntry>
		{
			public static bool IsActive() => PXAccess.FeatureInstalled<FeaturesSet.wMSFulfillment>();
		}
		#endregion

		#region Address Lookup Extension
		/// <exclude/>
		public class SOShipmentEntryAddressLookupExtension : CR.Extensions.AddressLookupExtension<SOShipmentEntry, SOShipment, SOShipmentAddress>
		{
			protected override string AddressView => nameof(Base.Shipping_Address);
		}

		public class SOShipmentEntryShippingAddressCachingHelper : AddressValidationExtension<SOShipmentEntry, SOShipmentAddress>
		{
			protected override IEnumerable<PXSelectBase<SOShipmentAddress>> AddressSelects()
			{
				yield return Base.Shipping_Address;
			}
		}
		#endregion
	}

	public class SOShipmentException : PXException
	{
		public ErrorCode Code { get; private set; }
		public SOOrderShipment Item { get; private set; }

		public SOShipmentException(ErrorCode code, SOOrderShipment item, string message, params object[] args)
			: base(message, args)
		{
			this.Code = code;
			this.Item = item;
		}

		public SOShipmentException(string message, params object[] args)
			: base(message, args)
		{
		}

		public SOShipmentException(string message)
			: base(message)
		{
		}


		public SOShipmentException(SerializationInfo info, StreamingContext context)
				: base(info, context)
		{
		}

		public enum ErrorCode
		{
			None,
			CannotShipTraced,
			NotAllocatedLines,
			CannotShipCompleteTraced,
			NothingToShipTraced,
			NothingToReceiveTraced
		}

	}

	[PXProjection(typeof(Select2<SOOrder,
		InnerJoin<SOOrderType, On<SOOrder.FK.OrderType>,
		InnerJoin<INItemPlan, On<INItemPlan.refNoteID, Equal<SOOrder.noteID>>,
		InnerJoin<INPlanType, On<INItemPlan.FK.PlanType>>>>,
	Where<INItemPlan.hold, Equal<boolFalse>,
	  And<INItemPlan.planQty, Greater<decimal0>,
		And<INPlanType.isDemand, Equal<boolTrue>,
		And<INPlanType.isFixed, Equal<boolFalse>,
		And<INPlanType.isForDate, Equal<boolTrue>,
		And<Where<INItemPlan.fixedSource, IsNull, Or<INItemPlan.fixedSource, NotEqual<INReplenishmentSource.transfer>>>>>>>>>>))]
	[Serializable]
	public partial class SOShipmentPlan : IBqlTable
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
		#region OrderType
		public abstract class orderType : PX.Data.BQL.BqlString.Field<orderType> { }
		protected String _OrderType;
		[PXDBString(2, IsKey = true, IsFixed = true, InputMask = ">LL", BqlField = typeof(SOOrder.orderType))]
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
		#region OrderNbr
		public abstract class orderNbr : PX.Data.BQL.BqlString.Field<orderNbr> { }
		protected String _OrderNbr;
		[PXDBString(15, IsUnicode = true, IsKey = true, InputMask = "", BqlField = typeof(SOOrder.orderNbr))]
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
		#region DestinationSiteID
		public abstract class destinationSiteID : PX.Data.BQL.BqlInt.Field<destinationSiteID> { }
		protected Int32? _DestinationSiteID;
		[PXDefault()]
		[IN.ToSite(DisplayName = "Destination Warehouse", DescriptionField = typeof(INSite.descr), BqlField = typeof(SOOrder.destinationSiteID))]
		public virtual Int32? DestinationSiteID
		{
			get
			{
				return this._DestinationSiteID;
			}
			set
			{
				this._DestinationSiteID = value;
			}
		}
		#endregion
		#region InventoryID
		public abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }
		protected Int32? _InventoryID;
		[PXDBInt(BqlField = typeof(INItemPlan.inventoryID))]
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
		#region SubItemID
		public abstract class subItemID : PX.Data.BQL.BqlInt.Field<subItemID> { }
		protected Int32? _SubItemID;
		[PXDBInt(BqlField = typeof(INItemPlan.subItemID))]
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
		[PXDBInt(BqlField = typeof(INItemPlan.siteID))]
		[PXSelector(typeof(Search<INSite.siteID>), CacheGlobal = true, SubstituteKey = typeof(INSite.siteCD))]
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
		#region LotSerialNbr
		public abstract class lotSerialNbr : PX.Data.BQL.BqlString.Field<lotSerialNbr> { }
		[PXDBString(100, IsUnicode = true, BqlField = typeof(INItemPlan.lotSerialNbr))]
		public virtual String LotSerialNbr
		{
			get;
			set;
		}
		#endregion
		#region PlanType
		public abstract class planType : PX.Data.BQL.BqlString.Field<planType> { }
		[PXDBString(2, IsFixed = true, BqlField = typeof(INItemPlan.planType))]
		public virtual String PlanType
		{
			get;
			set;
		}
		#endregion
		#region PlanDate
		public abstract class planDate : PX.Data.BQL.BqlDateTime.Field<planDate> { }
		protected DateTime? _PlanDate;
		[PXDBDate(BqlField = typeof(INItemPlan.planDate))]
		[PXUIField(DisplayName = "Sched. Ship. Date")]
		public virtual DateTime? PlanDate
		{
			get
			{
				return this._PlanDate;
			}
			set
			{
				this._PlanDate = value;
			}
		}
		#endregion
		#region PlanID
		public abstract class planID : PX.Data.BQL.BqlLong.Field<planID> { }
		protected Int64? _PlanID;
		[PXDBLong(IsKey = true, BqlField = typeof(INItemPlan.planID))]
		public virtual Int64? PlanID
		{
			get
			{
				return this._PlanID;
			}
			set
			{
				this._PlanID = value;
			}
		}
		#endregion
		#region DemandPlanID
		public abstract class demandPlanID : PX.Data.BQL.BqlLong.Field<demandPlanID> { }
		[PXDBLong(BqlField = typeof(INItemPlan.demandPlanID))]
		public virtual Int64? DemandPlanID
		{
			get;
			set;
		}
		#endregion
		#region PlanQty
		public abstract class planQty : PX.Data.BQL.BqlDecimal.Field<planQty> { }
		protected Decimal? _PlanQty;
		[PXDBDecimal(6, BqlField = typeof(INItemPlan.planQty))]
		public virtual Decimal? PlanQty
		{
			get
			{
				return this._PlanQty;
			}
			set
			{
				this._PlanQty = value;
			}
		}
		#endregion
		#region Reverse
		public abstract class reverse : PX.Data.BQL.BqlBool.Field<reverse> { }
		protected Boolean? _Reverse;
		[PXDBBool(BqlField = typeof(INItemPlan.reverse))]
		public virtual Boolean? Reverse
		{
			get
			{
				return this._Reverse;
			}
			set
			{
				this._Reverse = value;
			}
		}
		#endregion
		#region InclQtySOBackOrdered
		public abstract class inclQtySOBackOrdered : PX.Data.BQL.BqlShort.Field<inclQtySOBackOrdered> { }
		protected Int16? _InclQtySOBackOrdered;
		[PXDBShort(BqlField = typeof(INPlanType.inclQtySOBackOrdered))]
		public virtual Int16? InclQtySOBackOrdered
		{
			get
			{
				return this._InclQtySOBackOrdered;
			}
			set
			{
				this._InclQtySOBackOrdered = value;
			}
		}
		#endregion
		#region InclQtySOShipping
		public abstract class inclQtySOShipping : PX.Data.BQL.BqlShort.Field<inclQtySOShipping> { }
		protected Int16? _InclQtySOShipping;
		[PXDBShort(BqlField = typeof(INPlanType.inclQtySOShipping))]
		public virtual Int16? InclQtySOShipping
		{
			get
			{
				return this._InclQtySOShipping;
			}
			set
			{
				this._InclQtySOShipping = value;
			}
		}
		#endregion
		#region InclQtySOShipped
		public abstract class inclQtySOShipped : PX.Data.BQL.BqlShort.Field<inclQtySOShipped> { }
		protected Int16? _InclQtySOShipped;
		[PXDBShort(BqlField = typeof(INPlanType.inclQtySOShipped))]
		public virtual Int16? InclQtySOShipped
		{
			get
			{
				return this._InclQtySOShipped;
			}
			set
			{
				this._InclQtySOShipped = value;
			}
		}
		#endregion
		#region RequireAllocation
		public abstract class requireAllocation : PX.Data.BQL.BqlBool.Field<requireAllocation> { }
		protected Boolean? _RequireAllocation;
		[PXDBBool(BqlField = typeof(SOOrderType.requireAllocation))]
		public virtual Boolean? RequireAllocation
		{
			get
			{
				return this._RequireAllocation;
			}
			set
			{
				this._RequireAllocation = value;
			}
		}
		#endregion
		#region IsManualPackage
		public abstract class isManualPackage : PX.Data.BQL.BqlBool.Field<isManualPackage> { }

		[PXDBBool(BqlField = typeof(SOOrder.isManualPackage))]
		public virtual bool? IsManualPackage
		{
			get;
			set;
		}
		#endregion
	}


	[PXProjection(typeof(Select2<SOLine,
		InnerJoin<SOOrderType, 
			On<SOLine.FK.OrderType>,
		InnerJoin<SOOrderTypeOperation,
			On<SOLine.FK.OrderTypeOperation>>>,
		Where<SOLine.lineType, NotEqual<SOLineType.miscCharge>>>), new Type[] { typeof(SOLine) })]
	[Serializable]
	public partial class SOLine2 : IBqlTable, ISortOrder
	{
		#region OrderType
		public abstract class orderType : PX.Data.BQL.BqlString.Field<orderType> { }
		protected string _OrderType;
		[PXDBString(2, IsKey = true, IsFixed = true, BqlField = typeof(SOLine.orderType))]
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
		#region OrderNbr
		public abstract class orderNbr : PX.Data.BQL.BqlString.Field<orderNbr> { }
		protected string _OrderNbr;
		[PXDBString(15, IsUnicode = true, IsKey = true, InputMask = "", BqlField = typeof(SOLine.orderNbr))]
		[PXParent(typeof(Select<SOOrder, Where<SOOrder.orderType, Equal<Current<SOLine2.orderType>>, And<SOOrder.orderNbr, Equal<Current<SOLine2.orderNbr>>>>>))]
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
		#region LineNbr
		public abstract class lineNbr : PX.Data.BQL.BqlInt.Field<lineNbr> { }
		protected Int32? _LineNbr;
		[PXDBInt(IsKey = true, BqlField = typeof(SOLine.lineNbr))]
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
		#region SortOrder
		public abstract class sortOrder : PX.Data.BQL.BqlInt.Field<sortOrder> { }
		protected Int32? _SortOrder;
		[PXDBInt(BqlField = typeof(SOLine.sortOrder))]
		public virtual Int32? SortOrder
		{
			get
			{
				return this._SortOrder;
			}
			set
			{
				this._SortOrder = value;
			}
		}
		#endregion
		#region LineType
		public abstract class lineType : PX.Data.BQL.BqlString.Field<lineType> { }
		protected String _LineType;
		[PXDBString(2, IsFixed = true, BqlField = typeof(SOLine.lineType))]
		public virtual String LineType
		{
			get
			{
				return this._LineType;
			}
			set
			{
				this._LineType = value;
			}
		}
		#endregion
		#region Operation
		public abstract class operation : PX.Data.BQL.BqlString.Field<operation> { }
		protected String _Operation;
		[PXDBString(1, IsFixed = true, InputMask = ">a", BqlField = typeof(SOLine.operation))]
		[PXUIField(DisplayName = "Operation")]
		[SOOperation.List]
		public virtual String Operation
		{
			get
			{
				return this._Operation;
			}
			set
			{
				this._Operation = value;
			}
		}
		#endregion
		#region LineSign
		public abstract class lineSign : BqlShort.Field<lineSign> { }
		[PXDBShort(BqlField = typeof(SOLine.lineSign))]
		[PXDefault]
		public virtual short? LineSign
		{
			get;
			set;
		}
		#endregion
		#region ShipComplete
		public abstract class shipComplete : PX.Data.BQL.BqlString.Field<shipComplete> { }
		protected String _ShipComplete;
		[PXDBString(1, IsFixed = true, BqlField = typeof(SOLine.shipComplete))]
		public virtual String ShipComplete
		{
			get
			{
				return this._ShipComplete;
			}
			set
			{
				this._ShipComplete = value;
			}
		}
		#endregion
		#region Completed
		public abstract class completed : PX.Data.BQL.BqlBool.Field<completed> { }
		protected Boolean? _Completed;
		[PXDBBool(BqlField = typeof(SOLine.completed))]
		public virtual Boolean? Completed
		{
			get
			{
				return this._Completed;
			}
			set
			{
				this._Completed = value;
			}
		}
		#endregion
		#region InventoryID
		public abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }
		protected Int32? _InventoryID;
		[PXDBInt(BqlField = typeof(SOLine.inventoryID))]
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
		#region SubItemID
		public abstract class subItemID : PX.Data.BQL.BqlInt.Field<subItemID> { }
		protected Int32? _SubItemID;
		[PXDBInt(BqlField = typeof(SOLine.subItemID))]
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
		[PXDBInt(BqlField = typeof(SOLine.siteID))]
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
		#region SalesAcctID
		public abstract class salesAcctID : PX.Data.BQL.BqlInt.Field<salesAcctID> { }
		protected Int32? _SalesAcctID;
		[PXDBInt(BqlField = typeof(SOLine.salesAcctID))]
		public virtual Int32? SalesAcctID
		{
			get
			{
				return this._SalesAcctID;
			}
			set
			{
				this._SalesAcctID = value;
			}
		}
		#endregion
		#region SalesSubID
		public abstract class salesSubID : PX.Data.BQL.BqlInt.Field<salesSubID> { }
		protected Int32? _SalesSubID;
		[PXDBInt(BqlField = typeof(SOLine.salesSubID))]
		public virtual Int32? SalesSubID
		{
			get
			{
				return this._SalesSubID;
			}
			set
			{
				this._SalesSubID = value;
			}
		}
		#endregion
		#region TranDesc
		public abstract class tranDesc : PX.Data.BQL.BqlString.Field<tranDesc> { }
		protected String _TranDesc;
		[PXDBString(256, IsUnicode = true, BqlField = typeof(SOLine.tranDesc))]
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
		#region UOM
		public abstract class uOM : PX.Data.BQL.BqlString.Field<uOM> { }
		protected String _UOM;
		[INUnit(typeof(SOLine2.inventoryID), BqlField = typeof(SOLine.uOM))]
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
		#region OrderQty
		public abstract class orderQty : PX.Data.BQL.BqlDecimal.Field<orderQty> { }
		protected Decimal? _OrderQty;
		[PXDBDecimal(6, BqlField = typeof(SOLine.orderQty))]
		[PXDefault]
		public virtual Decimal? OrderQty
		{
			get
			{
				return this._OrderQty;
			}
			set
			{
				this._OrderQty = value;
			}
		}
		#endregion
		#region BaseOrderQty
		public abstract class baseOrderQty : PX.Data.BQL.BqlDecimal.Field<baseOrderQty> { }
		[PXDBDecimal(6, BqlField = typeof(SOLine.baseOrderQty))]
		[PXDefault]
		public virtual decimal? BaseOrderQty
		{
			get;
			set;
		}
		#endregion
		#region BaseShippedQty
		public abstract class baseShippedQty : PX.Data.BQL.BqlDecimal.Field<baseShippedQty> { }
		protected Decimal? _BaseShippedQty;
		[PXDBBaseQtyWithOrigQty(typeof(uOM), typeof(shippedQty), typeof(uOM), typeof(baseOrderQty), typeof(orderQty),
			BqlField = typeof(SOLine.baseShippedQty))]
		[PXDefault]
		public virtual Decimal? BaseShippedQty
		{
			get
			{
				return this._BaseShippedQty;
			}
			set
			{
				this._BaseShippedQty = value;
			}
		}
		#endregion
		#region ShippedQty
		public abstract class shippedQty : PX.Data.BQL.BqlDecimal.Field<shippedQty> { }
		protected Decimal? _ShippedQty;
		[PXDBDecimal(6, BqlField = typeof(SOLine.shippedQty))]
		[PXDefault]
		public virtual Decimal? ShippedQty
		{
			get
			{
				return this._ShippedQty;
			}
			set
			{
				this._ShippedQty = value;
			}
		}
		#endregion
		#region BilledQty
		public abstract class billedQty : PX.Data.BQL.BqlDecimal.Field<billedQty> { }
		protected Decimal? _BilledQty;
		[PXDBDecimal(6, BqlField = typeof(SOLine.billedQty))]
		[PXDefault]
		public virtual Decimal? BilledQty
		{
			get
			{
				return this._BilledQty;
			}
			set
			{
				this._BilledQty = value;
			}
		}
		#endregion
		#region BaseBilledQty
		public abstract class baseBilledQty : PX.Data.BQL.BqlDecimal.Field<baseBilledQty> { }
		protected Decimal? _BaseBilledQty;
		[PXDBBaseQuantity(typeof(SOLine2.uOM), typeof(SOLine2.billedQty), BqlField = typeof(SOLine.baseBilledQty))]
		[PXDefault]
		public virtual Decimal? BaseBilledQty
		{
			get
			{
				return this._BaseBilledQty;
			}
			set
			{
				this._BaseBilledQty = value;
			}
		}
		#endregion
		#region UnbilledQty
		public abstract class unbilledQty : PX.Data.BQL.BqlDecimal.Field<unbilledQty> { }
		protected Decimal? _UnbilledQty;
		[PXDBQuantity(BqlField = typeof(SOLine.unbilledQty))]
		[PXUnboundFormula(typeof(unbilledQty.Multiply<lineSign>), typeof(SumCalc<SOOrder.unbilledOrderQty>))]
		[PXDefault]
		public virtual Decimal? UnbilledQty
		{
			get
			{
				return this._UnbilledQty;
			}
			set
			{
				this._UnbilledQty = value;
			}
		}
		#endregion
		#region BaseUnbilledQty
		public abstract class baseUnbilledQty : PX.Data.BQL.BqlDecimal.Field<baseUnbilledQty> { }
		protected Decimal? _BaseUnbilledQty;
		[PXDBBaseQuantity(typeof(SOLine2.uOM), typeof(SOLine2.unbilledQty), BqlField = typeof(SOLine.baseUnbilledQty))]
		[PXDefault]
		public virtual Decimal? BaseUnbilledQty
		{
			get
			{
				return this._BaseUnbilledQty;
			}
			set
			{
				this._BaseUnbilledQty = value;
			}
		}
		#endregion
		#region OpenQty
		public abstract class openQty : PX.Data.BQL.BqlDecimal.Field<openQty> { }
		protected Decimal? _OpenQty;
		[PXDBDecimal(6, MinValue = 0, BqlField = typeof(SOLine.openQty))]
		[PXDefault]
		public virtual Decimal? OpenQty
		{
			get
			{
				return this._OpenQty;
			}
			set
			{
				this._OpenQty = value;
			}
		}
		#endregion
		#region BaseOpenQty
		public abstract class baseOpenQty : PX.Data.BQL.BqlDecimal.Field<baseOpenQty> { }
		protected Decimal? _BaseOpenQty;
		[PXDBDecimal(6, MinValue = 0, BqlField = typeof(SOLine.baseOpenQty))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Base Open Qty.")]
		public virtual Decimal? BaseOpenQty
		{
			get
			{
				return this._BaseOpenQty;
			}
			set
			{
				this._BaseOpenQty = value;
			}
		}
		#endregion
		#region CompleteQtyMin
		public abstract class completeQtyMin : PX.Data.BQL.BqlDecimal.Field<completeQtyMin> { }
		protected Decimal? _CompleteQtyMin;
		[PXDBDecimal(2, MinValue = 0.0, MaxValue = 99.0, BqlField = typeof(SOLine.completeQtyMin))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? CompleteQtyMin
		{
			get
			{
				return this._CompleteQtyMin;
			}
			set
			{
				this._CompleteQtyMin = value;
			}
		}
		#endregion
		#region CompleteQtyMax
		public abstract class completeQtyMax : PX.Data.BQL.BqlDecimal.Field<completeQtyMax> { }
		protected Decimal? _CompleteQtyMax;
		[PXDBDecimal(2, MinValue = 100.0, MaxValue = 999.0, BqlField = typeof(SOLine.completeQtyMax))]
		[PXDefault(TypeCode.Decimal, "100.0")]
		public virtual Decimal? CompleteQtyMax
		{
			get
			{
				return this._CompleteQtyMax;
			}
			set
			{
				this._CompleteQtyMax = value;
			}
		}
		#endregion
		#region ShipDate
		public abstract class shipDate : PX.Data.BQL.BqlDateTime.Field<shipDate> { }
		protected DateTime? _ShipDate;
		[PXDBDate(BqlField = typeof(SOLine.shipDate))]
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
		#region CuryInfoID
		public abstract class curyInfoID : PX.Data.BQL.BqlLong.Field<curyInfoID> { }
		protected Int64? _CuryInfoID;
		[PXDBLong(BqlField = typeof(SOLine.curyInfoID))]
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
		#region CuryUnitPrice
		public abstract class curyUnitPrice : PX.Data.BQL.BqlDecimal.Field<curyUnitPrice> { }
		protected Decimal? _CuryUnitPrice;
		[PXDBDecimal(6, BqlField = typeof(SOLine.curyUnitPrice))]
		[PXDefault]
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
		#region ActualUnitPrice
		public abstract class actualUnitPrice : PX.Data.BQL.BqlDecimal.Field<actualUnitPrice>
        {
		}
		[PXDBPriceCostCalced(typeof(
			Switch<Case<Where<SOLine.orderQty, Equal<decimal0>>, SOLine.unitPrice>,
				Div<SOLine.extPrice, SOLine.orderQty>>),
			typeof(decimal),
			CastToScale = 9, CastToPrecision = 25)]
		public virtual decimal? ActualUnitPrice
		{
			get;
			set;
		}
		#endregion
		#region UnitCost
		public abstract class unitCost : PX.Data.BQL.BqlDecimal.Field<unitCost>
        {
		}
		[PXDBDecimal(6, BqlField = typeof(SOLine.unitCost))]
		[PXDefault]
		public virtual decimal? UnitCost
		{
			get;
			set;
		}
		#endregion
		#region DiscPct
		public abstract class discPct : PX.Data.BQL.BqlDecimal.Field<discPct> { }
		protected Decimal? _DiscPct;
		[PXDBDecimal(6, BqlField = typeof(SOLine.discPct))]
		[PXDefault]
		public virtual Decimal? DiscPct
		{
			get
			{
				return this._DiscPct;
			}
			set
			{
				this._DiscPct = value;
			}
		}
		#endregion
		#region CuryBilledAmt
		public abstract class curyBilledAmt : PX.Data.BQL.BqlDecimal.Field<curyBilledAmt> { }
		protected Decimal? _CuryBilledAmt;
		[PXFormula(typeof(Mult<Mult<SOLine2.billedQty, SOLine2.curyUnitPrice>, Sub<decimal1, Div<SOLine2.discPct, decimal100>>>))]
		[PXDBCurrency(typeof(SOLine2.curyInfoID), typeof(SOLine2.billedAmt), BqlField = typeof(SOLine.curyBilledAmt))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? CuryBilledAmt
		{
			get
			{
				return this._CuryBilledAmt;
			}
			set
			{
				this._CuryBilledAmt = value;
			}
		}
		#endregion
		#region BilledAmt
		public abstract class billedAmt : PX.Data.BQL.BqlDecimal.Field<billedAmt> { }
		protected Decimal? _BilledAmt;
		[PXDBDecimal(4, BqlField = typeof(SOLine.billedAmt))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? BilledAmt
		{
			get
			{
				return this._BilledAmt;
			}
			set
			{
				this._BilledAmt = value;
			}
		}
		#endregion
		#region CuryOpenAmt
		public abstract class curyOpenAmt : PX.Data.BQL.BqlDecimal.Field<curyOpenAmt> { }
		protected Decimal? _CuryOpenAmt;
		[PXDBCurrency(typeof(SOLine2.curyInfoID), typeof(SOLine2.openAmt), BqlField = typeof(SOLine.curyOpenAmt))]
		[PXUIField(DisplayName = "Open Amount")]
		[PXDefault]
		public virtual Decimal? CuryOpenAmt
		{
			get
			{
				return this._CuryOpenAmt;
			}
			set
			{
				this._CuryOpenAmt = value;
			}
		}
		#endregion
		#region OpenAmt
		public abstract class openAmt : PX.Data.BQL.BqlDecimal.Field<openAmt> { }
		protected Decimal? _OpenAmt;
		[PXDBDecimal(4, BqlField = typeof(SOLine.openAmt))]
		[PXDefault]
		public virtual Decimal? OpenAmt
		{
			get
			{
				return this._OpenAmt;
			}
			set
			{
				this._OpenAmt = value;
			}
		}
		#endregion
		#region CuryUnbilledAmt
		public abstract class curyUnbilledAmt : PX.Data.BQL.BqlDecimal.Field<curyUnbilledAmt> { }
		protected Decimal? _CuryUnbilledAmt;
		[PXDBCurrency(typeof(SOLine2.curyInfoID), typeof(SOLine2.unbilledAmt), BqlField = typeof(SOLine.curyUnbilledAmt))]
		[PXFormula(typeof(Mult<Mult<SOLine2.unbilledQty, SOLine2.curyUnitPrice>, Sub<decimal1, Div<SOLine2.discPct, decimal100>>>))]
		[PXDefault]
		public virtual Decimal? CuryUnbilledAmt
		{
			get
			{
				return this._CuryUnbilledAmt;
			}
			set
			{
				this._CuryUnbilledAmt = value;
			}
		}
		#endregion
		#region UnbilledAmt
		public abstract class unbilledAmt : PX.Data.BQL.BqlDecimal.Field<unbilledAmt> { }
		protected Decimal? _UnbilledAmt;
		[PXDBDecimal(4, BqlField = typeof(SOLine.unbilledAmt))]
		[PXDefault]
		public virtual Decimal? UnbilledAmt
		{
			get
			{
				return this._UnbilledAmt;
			}
			set
			{
				this._UnbilledAmt = value;
			}
		}
		#endregion
		#region GroupDiscountRate
		public abstract class groupDiscountRate : PX.Data.BQL.BqlDecimal.Field<groupDiscountRate> { }
		protected Decimal? _GroupDiscountRate;
		[PXDBDecimal(18, BqlField = typeof(SOLine.groupDiscountRate))]
		[PXDefault(TypeCode.Decimal, "1.0")]
		public virtual Decimal? GroupDiscountRate
		{
			get
			{
				return this._GroupDiscountRate;
			}
			set
			{
				this._GroupDiscountRate = value;
			}
		}
		#endregion
		#region DocumentDiscountRate
		public abstract class documentDiscountRate : PX.Data.BQL.BqlDecimal.Field<documentDiscountRate> { }
		protected Decimal? _DocumentDiscountRate;
		[PXDBDecimal(18, BqlField = typeof(SOLine.documentDiscountRate))]
		[PXDefault(TypeCode.Decimal, "1.0")]
		public virtual Decimal? DocumentDiscountRate
		{
			get
			{
				return this._DocumentDiscountRate;
			}
			set
			{
				this._DocumentDiscountRate = value;
			}
		}
		#endregion
		#region DisableAutomaticTaxCalculation
		public abstract class disableAutomaticTaxCalculation : PX.Data.BQL.BqlBool.Field<disableAutomaticTaxCalculation> { }
		[PXDBBool(BqlField = typeof(SOLine.disableAutomaticTaxCalculation))]
		public virtual Boolean? DisableAutomaticTaxCalculation
		{
			get;
			set;
		}
		#endregion
		#region TaxZoneID
		public abstract class taxZoneID : PX.Data.BQL.BqlString.Field<taxZoneID> { }

		[PXDBString(10, IsUnicode = true, BqlField = typeof(SOLine.taxZoneID))]
		public virtual String TaxZoneID
		{
			get;
			set;
		}
		#endregion
		#region TaxCategoryID
		public abstract class taxCategoryID : PX.Data.BQL.BqlString.Field<taxCategoryID> { }
		protected String _TaxCategoryID;
		[PXDBString(TX.TaxCategory.taxCategoryID.Length, IsUnicode = true, BqlField = typeof(SOLine.taxCategoryID))]
		[SOUnbilledTax2(typeof(SOOrder), typeof(SOTax), typeof(SOTaxTran),
			   //Per Unit Tax settings
			   Inventory = typeof(SOLine2.inventoryID), UOM = typeof(SOLine2.uOM), LineQty = typeof(SOLine2.unbilledQty))]
		[SOOpenTax2(typeof(SOOrder), typeof(SOTax), typeof(SOTaxTran),
			   //Per Unit Tax settings
			   Inventory = typeof(SOLine2.inventoryID), UOM = typeof(SOLine2.uOM), LineQty = typeof(SOLine2.openQty))]
		public virtual String TaxCategoryID
		{
			get
			{
				return this._TaxCategoryID;
			}
			set
			{
				this._TaxCategoryID = value;
			}
		}
		#endregion
		#region PlanType
		public abstract class planType : PX.Data.BQL.BqlString.Field<planType> { }
		protected String _PlanType;
		[PXDBString(2, IsFixed = true, BqlField = typeof(SOOrderTypeOperation.orderPlanType))]
		public virtual String PlanType
		{
			get
			{
				return this._PlanType;
			}
			set
			{
				this._PlanType = value;
			}
		}
		#endregion
		#region POSource
		public abstract class pOSource : PX.Data.BQL.BqlString.Field<pOSource> { }
		protected string _POSource;
		[PXDBString(BqlField = typeof(SOLine.pOSource))]
		public virtual string POSource
		{
			get
			{
				return this._POSource;
			}
			set
			{
				this._POSource = value;
			}
		}
		#endregion

		#region LastModifiedByID
		public abstract class lastModifiedByID : PX.Data.BQL.BqlGuid.Field<lastModifiedByID> { }

		[PXDBLastModifiedByID(BqlField = typeof(SOLine.lastModifiedByID))]
		public virtual Guid? LastModifiedByID { get; set; }
		#endregion
		#region LastModifiedByScreenID
		public abstract class lastModifiedByScreenID : PX.Data.BQL.BqlString.Field<lastModifiedByScreenID> { }

		[PXDBLastModifiedByScreenID(BqlField = typeof(SOLine.lastModifiedByScreenID))]
		public virtual string LastModifiedByScreenID { get; set; }
		#endregion
		#region LastModifiedDateTime
		public abstract class lastModifiedDateTime : PX.Data.BQL.BqlDateTime.Field<lastModifiedDateTime> { }

		[PXDBLastModifiedDateTime(BqlField = typeof(SOLine.lastModifiedDateTime))]
		public virtual DateTime? LastModifiedDateTime { get; set; }
		#endregion
		#region tstamp
		public abstract class Tstamp : PX.Data.BQL.BqlByteArray.Field<Tstamp> { }

		[PXDBTimestamp(BqlField = typeof(SOLine.Tstamp), VerifyTimestamp = VerifyTimestampOptions.BothFromGraphAndRecord)]
		public virtual byte[] tstamp { get; set; }
		#endregion
	}

	[PXProjection(typeof(Select2<SOLineSplit,
		InnerJoin<SOOrderType,
			On<SOLineSplit.FK.OrderType>,
		InnerJoin<SOOrderTypeOperation,
			On<SOLineSplit.FK.OrderTypeOperation>>>>), new Type[] { typeof(SOLineSplit) })]
	[Serializable]
	public partial class SOLineSplit2 : IBqlTable
	{
		#region OrderType
		public abstract class orderType : PX.Data.BQL.BqlString.Field<orderType> { }
		protected string _OrderType;
		[PXDBString(2, IsKey = true, IsFixed = true, BqlField = typeof(SOLineSplit.orderType))]
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
		#region OrderNbr
		public abstract class orderNbr : PX.Data.BQL.BqlString.Field<orderNbr> { }
		protected string _OrderNbr;
		[PXDBString(15, IsUnicode = true, IsKey = true, InputMask = "", BqlField = typeof(SOLineSplit.orderNbr))]
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
		#region LineNbr
		public abstract class lineNbr : PX.Data.BQL.BqlInt.Field<lineNbr> { }
		protected Int32? _LineNbr;
		[PXDBInt(IsKey = true, BqlField = typeof(SOLineSplit.lineNbr))]
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
		#region SplitLineNbr
		public abstract class splitLineNbr : PX.Data.BQL.BqlInt.Field<splitLineNbr> { }
		protected Int32? _SplitLineNbr;
		[PXDBInt(IsKey = true, BqlField = typeof(SOLineSplit.splitLineNbr))]
		public virtual Int32? SplitLineNbr
		{
			get
			{
				return this._SplitLineNbr;
			}
			set
			{
				this._SplitLineNbr = value;
			}
		}
		#endregion
		#region Operation
		public abstract class operation : PX.Data.BQL.BqlString.Field<operation> { }
		protected String _Operation;
		[PXDBString(1, IsFixed = true, InputMask = ">a", BqlField = typeof(SOLineSplit.operation))]
		[PXUIField(DisplayName = "Operation")]
		[SOOperation.List]
		public virtual String Operation
		{
			get
			{
				return this._Operation;
			}
			set
			{
				this._Operation = value;
			}
		}
		#endregion
		#region Completed
		public abstract class completed : PX.Data.BQL.BqlBool.Field<completed> { }
		protected Boolean? _Completed;
		[PXDBBool(BqlField = typeof(SOLineSplit.completed))]
		public virtual Boolean? Completed
		{
			get
			{
				return this._Completed;
			}
			set
			{
				this._Completed = value;
			}
		}
		#endregion
		#region InventoryID
		public abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }
		protected Int32? _InventoryID;
		[PXDBInt(BqlField = typeof(SOLineSplit.inventoryID))]
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
		#region SiteID
		public abstract class siteID : PX.Data.BQL.BqlInt.Field<siteID> { }
		protected Int32? _SiteID;
		[PXDBInt(BqlField = typeof(SOLineSplit.siteID))]
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
		#region ToSiteID
		public abstract class toSiteID : PX.Data.BQL.BqlInt.Field<toSiteID> { }
		protected Int32? _ToSiteID;
		[PXDBInt(BqlField = typeof(SOLineSplit.toSiteID))]
		public virtual Int32? ToSiteID
		{
			get
			{
				return this._ToSiteID;
			}
			set
			{
				this._ToSiteID = value;
			}
		}
		#endregion
		#region CostCenterID
		public abstract class costCenterID : Data.BQL.BqlInt.Field<costCenterID> { }

		/// <exclude />
		[PXDBInt(BqlField = typeof(SOLineSplit.costCenterID))]
		public virtual int? CostCenterID
		{
			get;
			set;
		}
		#endregion
		#region LotSerialNbr
		public abstract class lotSerialNbr : PX.Data.BQL.BqlString.Field<lotSerialNbr> { }
		protected String _LotSerialNbr;
		[PXDBString(100, IsUnicode = true, BqlField = typeof(SOLineSplit.lotSerialNbr))]
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
		[INUnit(typeof(SOLineSplit2.inventoryID), BqlField = typeof(SOLineSplit.uOM))]
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
		[PXDBDecimal(6, BqlField = typeof(SOLineSplit.qty))]
		[PXDefault]
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
		[PXDBDecimal(6, BqlField = typeof(SOLineSplit.baseQty))]
		[PXDefault]
		public virtual Decimal? BaseQty
		{
			get;
			set;
		}
		#endregion
		#region ShippedQty
		public abstract class shippedQty : PX.Data.BQL.BqlDecimal.Field<shippedQty> { }
		protected Decimal? _ShippedQty;
		[PXDBDecimal(6, BqlField = typeof(SOLineSplit.shippedQty))]
		[PXDefault]
		public virtual Decimal? ShippedQty
		{
			get
			{
				return this._ShippedQty;
			}
			set
			{
				this._ShippedQty = value;
			}
		}
		#endregion
		#region BaseShippedQty
		public abstract class baseShippedQty : PX.Data.BQL.BqlDecimal.Field<baseShippedQty> { }
		protected Decimal? _BaseShippedQty;
		[PXDBBaseQtyWithOrigQty(typeof(uOM), typeof(shippedQty), typeof(uOM), typeof(baseQty), typeof(qty),
			BqlField = typeof(SOLineSplit.baseShippedQty))]
		[PXDefault]
		public virtual Decimal? BaseShippedQty
		{
			get
			{
				return this._BaseShippedQty;
			}
			set
			{
				this._BaseShippedQty = value;
			}
		}
		#endregion
		#region ShipDate
		public abstract class shipDate : PX.Data.BQL.BqlDateTime.Field<shipDate> { }
		protected DateTime? _ShipDate;
		[PXDBDate(BqlField = typeof(SOLineSplit.shipDate))]
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
		#region PlanType
		public abstract class planType : PX.Data.BQL.BqlString.Field<planType> { }
		protected String _PlanType;
		[PXDBString(2, IsFixed = true, BqlField = typeof(SOOrderTypeOperation.orderPlanType))]
		public virtual String PlanType
		{
			get
			{
				return this._PlanType;
			}
			set
			{
				this._PlanType = value;
			}
		}
		#endregion
		#region POCreate
		public abstract class pOCreate : PX.Data.BQL.BqlBool.Field<pOCreate> { }
		protected Boolean? _POCreate;
		[PXDBBool(BqlField = typeof(SOLineSplit.pOCreate))]
		public virtual Boolean? POCreate
		{
			get;
			set;
		}
		#endregion
		#region IsAllocated
		public abstract class isAllocated : PX.Data.BQL.BqlBool.Field<isAllocated> { }
		protected Boolean? _IsAllocated;
		[PXDBBool(BqlField = typeof(SOLineSplit.isAllocated))]
		public virtual Boolean? IsAllocated
		{
			get;
			set;
		}
		#endregion
		#region RefNoteID
		public abstract class refNoteID : PX.Data.BQL.BqlGuid.Field<refNoteID> { }
		protected Guid? _RefNoteID;
		[PXRefNote(BqlField = typeof(SOLineSplit.refNoteID))]
		public virtual Guid? RefNoteID
		{
			get
			{
				return this._RefNoteID;
			}
			set
			{
				this._RefNoteID = value;
			}
		}
		#endregion
		#region PlanID
		public abstract class planID : PX.Data.BQL.BqlLong.Field<planID> { }
		protected Int64? _PlanID;
		[PXDBLong(BqlField = typeof(SOLineSplit.planID), IsImmutable = true)]
		public virtual Int64? PlanID
		{
			get
			{
				return this._PlanID;
			}
			set
			{
				this._PlanID = value;
			}
		}
		#endregion
		#region SOOrderType
		public abstract class sOOrderType : PX.Data.BQL.BqlString.Field<sOOrderType> { }
		protected String _SOOrderType;
		[PXDBString(2, IsFixed = true, BqlField = typeof(SOLineSplit.sOOrderType))]
		public virtual String SOOrderType
		{
			get
			{
				return this._SOOrderType;
			}
			set
			{
				this._SOOrderType = value;
			}
		}
		#endregion
		#region SOOrderNbr
		public abstract class sOOrderNbr : PX.Data.BQL.BqlString.Field<sOOrderNbr> { }
		protected String _SOOrderNbr;
		[PXDBString(15, IsUnicode = true, BqlField = typeof(SOLineSplit.sOOrderNbr))]
		public virtual String SOOrderNbr
		{
			get
			{
				return this._SOOrderNbr;
			}
			set
			{
				this._SOOrderNbr = value;
			}
		}
		#endregion
		#region SOLineNbr
		public abstract class sOLineNbr : PX.Data.BQL.BqlInt.Field<sOLineNbr> { }
		protected Int32? _SOLineNbr;
		[PXDBInt(BqlField = typeof(SOLineSplit.sOLineNbr))]
		public virtual Int32? SOLineNbr
		{
			get
			{
				return this._SOLineNbr;
			}
			set
			{
				this._SOLineNbr = value;
			}
		}
		#endregion
		#region SOSplitLineNbr
		public abstract class sOSplitLineNbr : PX.Data.BQL.BqlInt.Field<sOSplitLineNbr> { }
		protected Int32? _SOSplitLineNbr;
		[PXDBInt(BqlField = typeof(SOLineSplit.sOSplitLineNbr))]
		public virtual Int32? SOSplitLineNbr
		{
			get
			{
				return this._SOSplitLineNbr;
			}
			set
			{
				this._SOSplitLineNbr = value;
			}
		}
		#endregion

		#region LastModifiedByID
		public abstract class lastModifiedByID : PX.Data.BQL.BqlGuid.Field<lastModifiedByID> { }

		[PXDBLastModifiedByID(BqlField = typeof(SOLineSplit.lastModifiedByID))]
		public virtual Guid? LastModifiedByID { get; set; }
		#endregion
		#region LastModifiedByScreenID
		public abstract class lastModifiedByScreenID : PX.Data.BQL.BqlString.Field<lastModifiedByScreenID> { }

		[PXDBLastModifiedByScreenID(BqlField = typeof(SOLineSplit.lastModifiedByScreenID))]
		public virtual string LastModifiedByScreenID { get; set; }
		#endregion
		#region LastModifiedDateTime
		public abstract class lastModifiedDateTime : PX.Data.BQL.BqlDateTime.Field<lastModifiedDateTime> { }

		[PXDBLastModifiedDateTime(BqlField = typeof(SOLineSplit.lastModifiedDateTime))]
		public virtual DateTime? LastModifiedDateTime { get; set; }
		#endregion
		#region tstamp
		public abstract class Tstamp : PX.Data.BQL.BqlByteArray.Field<Tstamp> { }

		[PXDBTimestamp(BqlField = typeof(SOLineSplit.Tstamp), VerifyTimestamp = VerifyTimestampOptions.BothFromGraphAndRecord)]
		public virtual byte[] tstamp { get; set; }
		#endregion
	}

	[PXProjection(typeof(Select<SOLine, Where<SOLine.lineType, NotEqual<SOLineType.miscCharge>>>), Persistent = true)]
	[Serializable]
	public partial class SOLine4 : IBqlTable, ISortOrder
	{
		#region BranchID
		public abstract class branchID : PX.Data.BQL.BqlInt.Field<branchID> { }
		[PXDBInt(BqlField = typeof(SOLine.branchID))]
		public virtual int? BranchID
		{
			get;
			set;
		}
		#endregion
		#region OrderType
		public abstract class orderType : PX.Data.BQL.BqlString.Field<orderType> { }
		protected string _OrderType;
		[PXDBString(2, IsKey = true, IsFixed = true, BqlField = typeof(SOLine.orderType))]
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
		#region OrderNbr
		public abstract class orderNbr : PX.Data.BQL.BqlString.Field<orderNbr> { }
		protected string _OrderNbr;
		[PXDBString(15, IsUnicode = true, IsKey = true, InputMask = "", BqlField = typeof(SOLine.orderNbr))]
		[PXParent(typeof(Select<SOOrder, Where<SOOrder.orderType, Equal<Current<SOLine4.orderType>>, And<SOOrder.orderNbr, Equal<Current<SOLine4.orderNbr>>>>>))]
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
		#region LineNbr
		public abstract class lineNbr : PX.Data.BQL.BqlInt.Field<lineNbr> { }
		protected Int32? _LineNbr;
		[PXDBInt(IsKey = true, BqlField = typeof(SOLine.lineNbr))]
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
		#region SortOrder
		public abstract class sortOrder : PX.Data.BQL.BqlInt.Field<sortOrder> { }
		protected Int32? _SortOrder;
		[PXDBInt(BqlField = typeof(SOLine.sortOrder))]
		public virtual Int32? SortOrder
		{
			get
			{
				return this._SortOrder;
			}
			set
			{
				this._SortOrder = value;
			}
		}
		#endregion
		#region Operation
		public abstract class operation : PX.Data.BQL.BqlString.Field<operation> { }
		protected String _Operation;
		[PXDBString(1, IsFixed = true, InputMask = ">a", BqlField = typeof(SOLine.operation))]
		[PXUIField(DisplayName = "Operation")]
		[SOOperation.List]
		public virtual String Operation
		{
			get
			{
				return this._Operation;
			}
			set
			{
				this._Operation = value;
			}
		}
		#endregion
		#region LineSign
		public abstract class lineSign : BqlShort.Field<lineSign> { }
		[PXDBShort(BqlField = typeof(SOLine.lineSign))]
		[PXDefault]
		public virtual short? LineSign
		{
			get;
			set;
		}
		#endregion
		#region ShipComplete
		public abstract class shipComplete : PX.Data.BQL.BqlString.Field<shipComplete> { }
		protected String _ShipComplete;
		[PXDBString(1, IsFixed = true, BqlField = typeof(SOLine.shipComplete))]
		public virtual String ShipComplete
		{
			get
			{
				return this._ShipComplete;
			}
			set
			{
				this._ShipComplete = value;
			}
		}
		#endregion
		#region InventoryID
		public abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }
		protected Int32? _InventoryID;
		[PXDBInt(BqlField = typeof(SOLine.inventoryID))]
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
		#region SiteID
		/// <inheritdoc cref="SOLine.SiteID"/>
		[PXParent(typeof(Select<SOOrderSite,
			Where<SOOrderSite.orderType, Equal<Current<SOLine4.orderType>>, And<SOOrderSite.orderNbr, Equal<Current<SOLine4.orderNbr>>,
				And<SOOrderSite.siteID, Equal<Current2<SOLine4.siteID>>>>>>), LeaveChildren = true, ParentCreate = true)]
		[PXUnboundFormula(typeof(IIf<Where<SOLine4.openLine.IsEqual<True>>, int1, int0>), typeof(SumCalc<SOOrderSite.openLineCntr>),
			SkipZeroUpdates = false, ValidateAggregateCalculation = true)]
		[PXDBInt(BqlField = typeof(SOLine.siteID))]
		public virtual Int32? SiteID { get; set; }
		public abstract class siteID : PX.Data.BQL.BqlInt.Field<siteID> { }
		#endregion
		#region UOM
		public abstract class uOM : PX.Data.BQL.BqlString.Field<uOM> { }
		protected String _UOM;
		[INUnit(typeof(SOLine4.inventoryID), BqlField = typeof(SOLine.uOM))]
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
		#region BaseOrderQty
		public abstract class baseOrderQty : PX.Data.BQL.BqlDecimal.Field<baseOrderQty> { }
		[PXDBBaseQuantity(typeof(SOLine4.uOM), typeof(SOLine4.orderQty), BqlField = typeof(SOLine.baseOrderQty))]
		[PXDefault]
		public virtual decimal? BaseOrderQty
		{
			get;
			set;
		}
		#endregion
		#region OrderQty
		public abstract class orderQty : PX.Data.BQL.BqlDecimal.Field<orderQty> { }
		protected Decimal? _OrderQty;
		[PXDBDecimal(6, BqlField = typeof(SOLine.orderQty))]
		[PXDefault]
		public virtual Decimal? OrderQty
		{
			get
			{
				return this._OrderQty;
			}
			set
			{
				this._OrderQty = value;
			}
		}
		#endregion
		#region BaseShippedQty
		public abstract class baseShippedQty : PX.Data.BQL.BqlDecimal.Field<baseShippedQty> { }
		protected Decimal? _BaseShippedQty;
		[PXDBBaseQuantity(typeof(SOLine4.uOM), typeof(SOLine4.shippedQty), BqlField = typeof(SOLine.baseShippedQty))]
		public virtual Decimal? BaseShippedQty
		{
			get
			{
				return this._BaseShippedQty;
			}
			set
			{
				this._BaseShippedQty = value;
			}
		}
		#endregion
		#region ShippedQty
		public abstract class shippedQty : PX.Data.BQL.BqlDecimal.Field<shippedQty> { }
		protected Decimal? _ShippedQty;
		[PXDBDecimal(6, BqlField = typeof(SOLine.shippedQty))]
		[PXDefault]
		public virtual Decimal? ShippedQty
		{
			get
			{
				return this._ShippedQty;
			}
			set
			{
				this._ShippedQty = value;
			}
		}
		#endregion
		#region UnbilledQty
		public abstract class unbilledQty : PX.Data.BQL.BqlDecimal.Field<unbilledQty> { }
		protected Decimal? _UnbilledQty;
		[PXDBQuantity(typeof(SOLine4.uOM), typeof(SOLine4.baseUnbilledQty), MinValue = 0, BqlField = typeof(SOLine.unbilledQty))]
		[PXUnboundFormula(typeof(unbilledQty.Multiply<lineSign>), typeof(SumCalc<SOOrder.unbilledOrderQty>))]
		[PXDefault]
		public virtual Decimal? UnbilledQty
		{
			get
			{
				return this._UnbilledQty;
			}
			set
			{
				this._UnbilledQty = value;
			}
		}
		#endregion
		#region BaseUnbilledQty
		public abstract class baseUnbilledQty : PX.Data.BQL.BqlDecimal.Field<baseUnbilledQty> { }
		protected Decimal? _BaseUnbilledQty;
		[PXDBDecimal(6, BqlField = typeof(SOLine.baseUnbilledQty))]
		public virtual Decimal? BaseUnbilledQty
		{
			get
			{
				return this._BaseUnbilledQty;
			}
			set
			{
				this._BaseUnbilledQty = value;
			}
		}
		#endregion
		#region OpenQty
		public abstract class openQty : PX.Data.BQL.BqlDecimal.Field<openQty> { }
		protected Decimal? _OpenQty;
		[PXDBQuantity(typeof(SOLine4.uOM), typeof(SOLine4.baseOpenQty), BqlField = typeof(SOLine.openQty))]
		[PXFormula(typeof(Sub<SOLine4.orderQty, SOLine4.shippedQty>))]
		[PXUnboundFormula(typeof(openQty.Multiply<lineSign>), typeof(SumCalc<SOOrder.openOrderQty>))]
		[PXDefault]
		public virtual Decimal? OpenQty
		{
			get
			{
				return this._OpenQty;
			}
			set
			{
				this._OpenQty = value;
			}
		}
		#endregion
		#region BaseOpenQty
		public abstract class baseOpenQty : PX.Data.BQL.BqlDecimal.Field<baseOpenQty> { }
		protected Decimal? _BaseOpenQty;
		[PXDBDecimal(6, MinValue = 0, BqlField = typeof(SOLine.baseOpenQty))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Base Open Qty.")]
		public virtual Decimal? BaseOpenQty
		{
			get
			{
				return this._BaseOpenQty;
			}
			set
			{
				this._BaseOpenQty = value;
			}
		}
		#endregion
		#region CompleteQtyMin
		public abstract class completeQtyMin : PX.Data.BQL.BqlDecimal.Field<completeQtyMin> { }
		protected Decimal? _CompleteQtyMin;
		[PXDBDecimal(2, MinValue = 0.0, MaxValue = 99.0, BqlField = typeof(SOLine.completeQtyMin))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? CompleteQtyMin
		{
			get
			{
				return this._CompleteQtyMin;
			}
			set
			{
				this._CompleteQtyMin = value;
			}
		}
		#endregion
		#region CompleteQtyMax
		public abstract class completeQtyMax : PX.Data.BQL.BqlDecimal.Field<completeQtyMax> { }
		protected Decimal? _CompleteQtyMax;
		[PXDBDecimal(2, MinValue = 100.0, MaxValue = 999.0, BqlField = typeof(SOLine.completeQtyMax))]
		[PXDefault(TypeCode.Decimal, "100.0")]
		public virtual Decimal? CompleteQtyMax
		{
			get
			{
				return this._CompleteQtyMax;
			}
			set
			{
				this._CompleteQtyMax = value;
			}
		}
		#endregion
		#region Completed
		public abstract class completed : PX.Data.BQL.BqlBool.Field<completed> { }
		[PXDBBool(BqlField = typeof(SOLine.completed))]
		public virtual Boolean? Completed
		{
			get;
			set;
		}
		#endregion
		#region CuryInfoID
		public abstract class curyInfoID : PX.Data.BQL.BqlLong.Field<curyInfoID> { }
		protected Int64? _CuryInfoID;
		[PXDBLong(BqlField = typeof(SOLine.curyInfoID))]
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
		#region CuryUnitPrice
		public abstract class curyUnitPrice : PX.Data.BQL.BqlDecimal.Field<curyUnitPrice> { }
		protected Decimal? _CuryUnitPrice;
		[PXDBDecimal(6, BqlField = typeof(SOLine.curyUnitPrice))]
		[PXDefault]
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
		#region UnitPrice
		public abstract class unitPrice : PX.Data.BQL.BqlDecimal.Field<unitPrice> { }
		protected Decimal? _UnitPrice;
		[PXDBDecimal(6, BqlField = typeof(SOLine.unitPrice))]
		[PXDefault]
		public virtual Decimal? UnitPrice
		{
			get
			{
				return this._UnitPrice;
			}
			set
			{
				this._UnitPrice = value;
			}
		}
		#endregion
		#region DiscPct
		public abstract class discPct : PX.Data.BQL.BqlDecimal.Field<discPct> { }
		protected Decimal? _DiscPct;
		[PXDBDecimal(6, BqlField = typeof(SOLine.discPct))]
		[PXDefault]
		public virtual Decimal? DiscPct
		{
			get
			{
				return this._DiscPct;
			}
			set
			{
				this._DiscPct = value;
			}
		}
		#endregion
		#region CuryOpenAmt
		public abstract class curyOpenAmt : PX.Data.BQL.BqlDecimal.Field<curyOpenAmt> { }
		protected Decimal? _CuryOpenAmt;
		[PXDBCurrency(typeof(SOLine4.curyInfoID), typeof(SOLine4.openAmt), BqlField = typeof(SOLine.curyOpenAmt))]
		[PXFormula(typeof(Mult<Mult<SOLine4.openQty, SOLine4.curyUnitPrice>, Sub<decimal1, Div<SOLine4.discPct, decimal100>>>))]
		[PXUIField(DisplayName = "Open Amount")]
		[PXDefault]
		public virtual Decimal? CuryOpenAmt
		{
			get
			{
				return this._CuryOpenAmt;
			}
			set
			{
				this._CuryOpenAmt = value;
			}
		}
		#endregion
		#region OpenAmt
		public abstract class openAmt : PX.Data.BQL.BqlDecimal.Field<openAmt> { }
		protected Decimal? _OpenAmt;
		[PXDBDecimal(4, BqlField = typeof(SOLine.openAmt))]
		[PXDefault]
		public virtual Decimal? OpenAmt
		{
			get
			{
				return this._OpenAmt;
			}
			set
			{
				this._OpenAmt = value;
			}
		}
		#endregion
		#region CuryUnbilledAmt
		public abstract class curyUnbilledAmt : PX.Data.BQL.BqlDecimal.Field<curyUnbilledAmt> { }
		protected Decimal? _CuryUnbilledAmt;
		[PXDBCurrency(typeof(SOLine4.curyInfoID), typeof(SOLine4.unbilledAmt), BqlField = typeof(SOLine.curyUnbilledAmt))]
		[PXFormula(typeof(Mult<Mult<SOLine4.unbilledQty, SOLine4.curyUnitPrice>, Sub<decimal1, Div<SOLine4.discPct, decimal100>>>))]
		[PXDefault]
		public virtual Decimal? CuryUnbilledAmt
		{
			get
			{
				return this._CuryUnbilledAmt;
			}
			set
			{
				this._CuryUnbilledAmt = value;
			}
		}
		#endregion
		#region UnbilledAmt
		public abstract class unbilledAmt : PX.Data.BQL.BqlDecimal.Field<unbilledAmt> { }
		protected Decimal? _UnbilledAmt;
		[PXDBDecimal(4, BqlField = typeof(SOLine.unbilledAmt))]
		[PXDefault]
		public virtual Decimal? UnbilledAmt
		{
			get
			{
				return this._UnbilledAmt;
			}
			set
			{
				this._UnbilledAmt = value;
			}
		}
		#endregion
		#region GroupDiscountRate
		public abstract class groupDiscountRate : PX.Data.BQL.BqlDecimal.Field<groupDiscountRate> { }
		protected Decimal? _GroupDiscountRate;
		[PXDBDecimal(18, BqlField = typeof(SOLine.groupDiscountRate))]
		[PXDefault(TypeCode.Decimal, "1.0")]
		public virtual Decimal? GroupDiscountRate
		{
			get
			{
				return this._GroupDiscountRate;
			}
			set
			{
				this._GroupDiscountRate = value;
			}
		}
		#endregion
		#region DocumentDiscountRate
		public abstract class documentDiscountRate : PX.Data.BQL.BqlDecimal.Field<documentDiscountRate> { }
		protected Decimal? _DocumentDiscountRate;
		[PXDBDecimal(18, BqlField = typeof(SOLine.documentDiscountRate))]
		[PXDefault(TypeCode.Decimal, "1.0")]
		public virtual Decimal? DocumentDiscountRate
		{
			get
			{
				return this._DocumentDiscountRate;
			}
			set
			{
				this._DocumentDiscountRate = value;
			}
		}
		#endregion
		#region TaxCategoryID
		public abstract class taxCategoryID : PX.Data.BQL.BqlString.Field<taxCategoryID> { }
		protected String _TaxCategoryID;
		[PXDBString(TX.TaxCategory.taxCategoryID.Length, IsUnicode = true, BqlField = typeof(SOLine.taxCategoryID))]
		[SOOpenTax4(typeof(SOOrder), typeof(SOTax), typeof(SOTaxTran),
			   //Per Unit Tax settings
			   Inventory = typeof(SOLine4.inventoryID), UOM = typeof(SOLine4.uOM), LineQty = typeof(SOLine4.openQty))]
		[SOUnbilledTax4(typeof(SOOrder), typeof(SOTax), typeof(SOTaxTran),
			   //Per Unit Tax settings
			   Inventory = typeof(SOLine4.inventoryID), UOM = typeof(SOLine4.uOM), LineQty = typeof(SOLine4.unbilledQty))]
		public virtual String TaxCategoryID
		{
			get
			{
				return this._TaxCategoryID;
			}
			set
			{
				this._TaxCategoryID = value;
			}
		}
		#endregion
		#region ShipDate
		public abstract class shipDate : PX.Data.BQL.BqlDateTime.Field<shipDate> { }
		[PXDBDate(BqlField = typeof(SOLine.shipDate))]
		public virtual DateTime? ShipDate
		{
			get;
			set;
		}
		#endregion
		#region LineAmt
		public abstract class lineAmt : PX.Data.BQL.BqlDecimal.Field<lineAmt> { }
		[PXDBDecimal(4, BqlField = typeof(SOLine.lineAmt))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? LineAmt
		{
			get;
			set;
		}
		#endregion
		#region SalesAcctID
		public abstract class salesAcctID : PX.Data.BQL.BqlInt.Field<salesAcctID> { }
		[PXDBInt(BqlField = typeof(SOLine.salesAcctID))]
		public virtual Int32? SalesAcctID
		{
			get;
			set;
		}
		#endregion
		#region ProjectID
		public abstract class projectID : PX.Data.BQL.BqlInt.Field<projectID> { }
		[PXDBInt(BqlField = typeof(SOLine.projectID))]
		public virtual Int32? ProjectID
		{
			get;
			set;
		}
		#endregion
		#region TaskID
		public abstract class taskID : PX.Data.BQL.BqlInt.Field<taskID> { }
		[PXDBInt(BqlField = typeof(SOLine.taskID))]
		public virtual Int32? TaskID
		{
			get;
			set;
		}
		#endregion
		#region OpenLine
		public abstract class openLine : PX.Data.BQL.BqlBool.Field<openLine> { }
		protected Boolean? _OpenLine;
		[PXDBBool(BqlField = typeof(SOLine.openLine))]
		public virtual Boolean? OpenLine
		{
			get
			{
				return this._OpenLine;
			}
			set
			{
				this._OpenLine = value;
			}
		}
		#endregion
		#region POCreate
		public abstract class pOCreate : PX.Data.BQL.BqlBool.Field<pOCreate> { }
		[PXDBBool(BqlField = typeof(SOLine.pOCreate))]
		public virtual bool? POCreate
		{
			get;
			set;
		}
		#endregion
		#region POSource
		public abstract class pOSource : PX.Data.BQL.BqlString.Field<pOSource> { }
		[PXDBString(BqlField = typeof(SOLine.pOSource))]
		public virtual string POSource
		{
			get;
			set;
		}
		#endregion
		#region tstamp
		public abstract class Tstamp : PX.Data.BQL.BqlByteArray.Field<Tstamp> { }

		[PXDBTimestamp(BqlField = typeof(SOLine.Tstamp), VerifyTimestamp = VerifyTimestampOptions.BothFromGraphAndRecord)]
		public virtual byte[] tstamp { get; set; }
		#endregion
		#region BlanketType
		/// <inheritdoc cref="BlanketType"/>
		public abstract class blanketType : BqlString.Field<blanketType> { }

		/// <inheritdoc cref="SOLine.BlanketType"/>
		[PXDBString(2, IsFixed = true, BqlField = typeof(SOLine.blanketType))]
		public virtual string BlanketType
		{
			get;
			set;
		}
		#endregion
		#region BlanketNbr
		/// <inheritdoc cref="BlanketNbr"/>
		public abstract class blanketNbr : BqlString.Field<blanketNbr> { }

		/// <inheritdoc cref="SOLine.BlanketNbr"/>
		[PXDBString(15, IsUnicode = true, BqlField = typeof(SOLine.blanketNbr))]
		public virtual string BlanketNbr
		{
			get;
			set;
		}
		#endregion
		#region BlanketLineNbr
		/// <inheritdoc cref="BlanketLineNbr"/>
		public abstract class blanketLineNbr : BqlInt.Field<blanketLineNbr> { }

		/// <inheritdoc cref="SOLine.BlanketLineNbr"/>
		[PXDBInt(BqlField = typeof(SOLine.blanketLineNbr))]
		public virtual int? BlanketLineNbr
		{
			get;
			set;
		}
		#endregion
		#region BlanketSplitLineNbr
		/// <inheritdoc cref="BlanketSplitLineNbr"/>
		public abstract class blanketSplitLineNbr : BqlInt.Field<blanketSplitLineNbr> { }

		/// <inheritdoc cref="SOLine.BlanketSplitLineNbr"/>
		[PXDBInt(BqlField = typeof(SOLine.blanketSplitLineNbr))]
		public virtual int? BlanketSplitLineNbr
		{
			get;
			set;
		}
		#endregion
	}

	[PXProjection(typeof(Select<SOLine, Where<SOLine.lineType, Equal<SOLineType.miscCharge>>>), Persistent = true)]
	[Serializable]
	public partial class SOMiscLine2 : IBqlTable, ISortOrder
	{
		#region BranchID
		public abstract class branchID : PX.Data.BQL.BqlInt.Field<branchID> { }
		protected Int32? _BranchID;
		[PXDBInt(BqlField = typeof(SOLine.branchID))]
		public virtual Int32? BranchID
		{
			get
			{
				return this._BranchID;
			}
			set
			{
				this._BranchID = value;
			}
		}
		#endregion
		#region OrderType
		public abstract class orderType : PX.Data.BQL.BqlString.Field<orderType> { }
		protected string _OrderType;
		[PXDBString(2, IsKey = true, IsFixed = true, BqlField = typeof(SOLine.orderType))]
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
		#region OrderNbr
		public abstract class orderNbr : PX.Data.BQL.BqlString.Field<orderNbr> { }
		protected string _OrderNbr;
		[PXDBString(15, IsUnicode = true, IsKey = true, InputMask = "", BqlField = typeof(SOLine.orderNbr))]
		[PXParent(typeof(Select<SOOrder, Where<SOOrder.orderType, Equal<Current<SOMiscLine2.orderType>>, And<SOOrder.orderNbr, Equal<Current<SOMiscLine2.orderNbr>>>>>))]
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
		#region LineNbr
		public abstract class lineNbr : PX.Data.BQL.BqlInt.Field<lineNbr> { }
		protected Int32? _LineNbr;
		[PXDBInt(IsKey = true, BqlField = typeof(SOLine.lineNbr))]
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
		#region SortOrder
		public abstract class sortOrder : PX.Data.BQL.BqlInt.Field<sortOrder> { }
		protected Int32? _SortOrder;
		[PXDBInt(BqlField = typeof(SOLine.sortOrder))]
		public virtual Int32? SortOrder
		{
			get
			{
				return this._SortOrder;
			}
			set
			{
				this._SortOrder = value;
			}
		}
		#endregion
		#region DefaultOperation
		public abstract class defaultOperation : PX.Data.BQL.BqlString.Field<defaultOperation> { }
		[PXDBString(1, IsFixed = true, BqlField = typeof(SOLine.defaultOperation))]
		public virtual string DefaultOperation
		{
			get;
			set;
		}
		#endregion
		#region Operation
		public abstract class operation : PX.Data.BQL.BqlString.Field<operation> { }
		protected String _Operation;
		[PXDBString(1, IsFixed = true, InputMask = ">a", BqlField = typeof(SOLine.operation))]
		[PXUIField(DisplayName = "Operation")]
		[SOOperation.List]
		public virtual String Operation
		{
			get
			{
				return this._Operation;
			}
			set
			{
				this._Operation = value;
			}
		}
		#endregion
		#region LineSign
		public abstract class lineSign : BqlShort.Field<lineSign> { }
		[PXDBShort(BqlField = typeof(SOLine.lineSign))]
		[PXDefault]
		public virtual short? LineSign
		{
			get;
			set;
		}
		#endregion
		#region Completed
		public abstract class completed : PX.Data.BQL.BqlBool.Field<completed> { }
		[PXDBBool(BqlField = typeof(SOLine.completed))]
		[PXDefault]
		[PXUIField(DisplayName = "Completed")]
		public virtual Boolean? Completed
		{
			get;
			set;
		}
		#endregion
		#region InventoryID
		public abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }
		protected Int32? _InventoryID;
		[NonStockItem(BqlField = typeof(SOLine.inventoryID))]
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
		#region SiteID
		public abstract class siteID : PX.Data.BQL.BqlInt.Field<siteID> { }
		protected Int32? _SiteID;
		[PXDBInt(BqlField = typeof(SOLine.siteID))]
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
		#region ProjectID
		public abstract class projectID : PX.Data.BQL.BqlInt.Field<projectID> { }
		protected Int32? _ProjectID;
		[PXDBInt(BqlField = typeof(SOLine.projectID))]
		public virtual Int32? ProjectID
		{
			get
			{
				return this._ProjectID;
			}
			set
			{
				this._ProjectID = value;
			}
		}
		#endregion
		#region ShipDate
		public abstract class shipDate : PX.Data.BQL.BqlDateTime.Field<shipDate> { }
		protected DateTime? _ShipDate;
		[PXDBDate(BqlField =typeof(SOLine.shipDate))]
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
		#region InvoiceType
		public abstract class invoiceType : PX.Data.BQL.BqlString.Field<invoiceType> { }

		/// <summary>
		/// Type of the Invoice to which the return SO line is applied.
		/// </summary>
		[PXDBString(3, IsFixed = true, BqlField = typeof(SOLine.invoiceType))]
		public virtual string InvoiceType { get; set; }
		#endregion
		#region InvoiceNbr
		public abstract class invoiceNbr : PX.Data.BQL.BqlString.Field<invoiceNbr> { }

		/// <summary>
		/// Number of the Invoice to which the return SO line is applied.
		/// </summary>
		[PXDBString(15, IsUnicode = true, BqlField = typeof(SOLine.invoiceNbr))]
		public virtual string InvoiceNbr { get; set; }
		#endregion
		#region InvoiceLineNbr
		public abstract class invoiceLineNbr : PX.Data.BQL.BqlInt.Field<invoiceLineNbr> { }
		/// <summary>
		/// Number of the Invoice line to which the return SO line is applied.
		/// </summary>
		[PXDBInt(BqlField = typeof(SOLine.invoiceLineNbr))]
		public virtual int? InvoiceLineNbr
		{
			get;
			set;
		}
		#endregion
		#region InvoiceDate
		public abstract class invoiceDate : PX.Data.BQL.BqlDateTime.Field<invoiceDate> { }

		[PXDBDate(BqlField = typeof(SOLine.invoiceDate))]
		public virtual DateTime? InvoiceDate { get; set; }
		#endregion
		#region CuryInfoID
		public abstract class curyInfoID : PX.Data.BQL.BqlLong.Field<curyInfoID> { }
		protected Int64? _CuryInfoID;
		[PXDBLong(BqlField = typeof(SOLine.curyInfoID))]
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
		#region UOM
		public abstract class uOM : PX.Data.BQL.BqlString.Field<uOM> { }
		protected String _UOM;
		[INUnit(typeof(SOMiscLine2.inventoryID), BqlField = typeof(SOLine.uOM))]
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
		#region OrderQty
		public abstract class orderQty : PX.Data.BQL.BqlDecimal.Field<orderQty> { }
		protected Decimal? _OrderQty;
		[PXDBDecimal(6, BqlField =typeof(SOLine.orderQty))]
		[PXDefault]
		public virtual Decimal? OrderQty
		{
			get
			{
				return this._OrderQty;
			}
			set
			{
				this._OrderQty = value;
			}
		}
		#endregion
		#region BilledQty
		public abstract class billedQty : PX.Data.BQL.BqlDecimal.Field<billedQty> { }
		protected Decimal? _BilledQty;
		[PXDBDecimal(6, BqlField = typeof(SOLine.billedQty))]
		[PXDefault]
		public virtual Decimal? BilledQty
		{
			get
			{
				return this._BilledQty;
			}
			set
			{
				this._BilledQty = value;
			}
		}
		#endregion
		#region BaseBilledQty
		public abstract class baseBilledQty : PX.Data.BQL.BqlDecimal.Field<baseBilledQty> { }
		protected Decimal? _BaseBilledQty;
		[PXDBBaseQuantity(typeof(SOMiscLine2.uOM), typeof(SOMiscLine2.billedQty), BqlField = typeof(SOLine.baseBilledQty))]
		[PXDefault]
		public virtual Decimal? BaseBilledQty
		{
			get
			{
				return this._BaseBilledQty;
			}
			set
			{
				this._BaseBilledQty = value;
			}
		}
		#endregion
		#region UnbilledQty
		public abstract class unbilledQty : PX.Data.BQL.BqlDecimal.Field<unbilledQty> { }
		protected Decimal? _UnbilledQty;
		[PXDBQuantity(BqlField = typeof(SOLine.unbilledQty))]
		[PXUnboundFormula(typeof(unbilledQty.Multiply<lineSign>), typeof(SumCalc<SOOrder.unbilledOrderQty>))]
		[PXDefault]
		public virtual Decimal? UnbilledQty
		{
			get
			{
				return this._UnbilledQty;
			}
			set
			{
				this._UnbilledQty = value;
			}
		}
		#endregion
		#region BaseUnbilledQty
		public abstract class baseUnbilledQty : PX.Data.BQL.BqlDecimal.Field<baseUnbilledQty> { }
		protected Decimal? _BaseUnbilledQty;
		[PXDBBaseQuantity(typeof(SOMiscLine2.uOM), typeof(SOMiscLine2.unbilledQty), BqlField = typeof(SOLine.baseUnbilledQty))]
		public virtual Decimal? BaseUnbilledQty
		{
			get
			{
				return this._BaseUnbilledQty;
			}
			set
			{
				this._BaseUnbilledQty = value;
			}
		}
		#endregion
		#region CuryUnitPrice
		public abstract class curyUnitPrice : PX.Data.BQL.BqlDecimal.Field<curyUnitPrice> { }
		protected Decimal? _CuryUnitPrice;
		[PXDBDecimal(6, BqlField = typeof(SOLine.curyUnitPrice))]
		[PXDefault]
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
		#region CuryExtPrice
		public abstract class curyExtPrice : PX.Data.BQL.BqlDecimal.Field<curyExtPrice>
        {
		}
		[PXDBDecimal(6, BqlField = typeof(SOLine.curyExtPrice))]
		[PXDefault]
		public virtual decimal? CuryExtPrice
		{
			get;
			set;
		}
		#endregion
		#region CuryLineAmt
		public abstract class curyLineAmt : PX.Data.BQL.BqlDecimal.Field<curyLineAmt> { }
		protected Decimal? _CuryLineAmt;
		[PXDBCurrency(typeof(SOMiscLine2.curyInfoID), typeof(SOMiscLine2.lineAmt), BqlField = typeof(SOLine.curyLineAmt))]
		[PXUIField(DisplayName = "Ext. Amount")]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? CuryLineAmt
		{
			get
			{
				return this._CuryLineAmt;
			}
			set
			{
				this._CuryLineAmt = value;
			}
		}
		#endregion
		#region LineAmt
		public abstract class lineAmt : PX.Data.BQL.BqlDecimal.Field<lineAmt> { }
		protected Decimal? _LineAmt;
		[PXDBDecimal(4, BqlField = typeof(SOLine.lineAmt))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? LineAmt
		{
			get
			{
				return this._LineAmt;
			}
			set
			{
				this._LineAmt = value;
			}
		}
		#endregion
		#region CuryBilledAmt
		public abstract class curyBilledAmt : PX.Data.BQL.BqlDecimal.Field<curyBilledAmt> { }
		protected Decimal? _CuryBilledAmt;
		[PXDBCurrency(typeof(SOMiscLine2.curyInfoID), typeof(SOMiscLine2.billedAmt), BqlField = typeof(SOLine.curyBilledAmt))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? CuryBilledAmt
		{
			get
			{
				return this._CuryBilledAmt;
			}
			set
			{
				this._CuryBilledAmt = value;
			}
		}
		#endregion
		#region BilledAmt
		public abstract class billedAmt : PX.Data.BQL.BqlDecimal.Field<billedAmt> { }
		protected Decimal? _BilledAmt;
		[PXDBDecimal(4, BqlField = typeof(SOLine.billedAmt))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? BilledAmt
		{
			get
			{
				return this._BilledAmt;
			}
			set
			{
				this._BilledAmt = value;
			}
		}
		#endregion
		#region CuryUnbilledAmt
		public abstract class curyUnbilledAmt : PX.Data.BQL.BqlDecimal.Field<curyUnbilledAmt> { }
		protected Decimal? _CuryUnbilledAmt;
		[PXDBCurrency(typeof(SOMiscLine2.curyInfoID), typeof(SOMiscLine2.unbilledAmt), BqlField = typeof(SOLine.curyUnbilledAmt))]
		[PXDefault]
		public virtual Decimal? CuryUnbilledAmt
		{
			get
			{
				return this._CuryUnbilledAmt;
			}
			set
			{
				this._CuryUnbilledAmt = value;
			}
		}
		#endregion
		#region UnbilledAmt
		public abstract class unbilledAmt : PX.Data.BQL.BqlDecimal.Field<unbilledAmt> { }
		protected Decimal? _UnbilledAmt;
		[PXDBDecimal(4, BqlField = typeof(SOLine.unbilledAmt))]
		[PXDefault]
		public virtual Decimal? UnbilledAmt
		{
			get
			{
				return this._UnbilledAmt;
			}
			set
			{
				this._UnbilledAmt = value;
			}
		}
		#endregion
		#region CuryDiscAmt
		public abstract class curyDiscAmt : PX.Data.BQL.BqlDecimal.Field<curyDiscAmt> { }
		protected Decimal? _CuryDiscAmt;
		[PXDBCurrency(typeof(SOMiscLine2.curyInfoID), typeof(SOMiscLine2.discAmt), BqlField = typeof(SOLine.curyDiscAmt))]
		[PXUIField(DisplayName = "Ext. Amount")]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? CuryDiscAmt
		{
			get
			{
				return this._CuryDiscAmt;
			}
			set
			{
				this._CuryDiscAmt = value;
			}
		}
		#endregion
		#region DiscAmt
		public abstract class discAmt : PX.Data.BQL.BqlDecimal.Field<discAmt> { }
		protected Decimal? _DiscAmt;
		[PXDBDecimal(4, BqlField = typeof(SOLine.discAmt))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? DiscAmt
		{
			get
			{
				return this._DiscAmt;
			}
			set
			{
				this._DiscAmt = value;
			}
		}
		#endregion
		#region DiscPct
		public abstract class discPct : PX.Data.BQL.BqlDecimal.Field<discPct> { }
		protected Decimal? _DiscPct;
		[PXDBDecimal(6, BqlField = typeof(SOLine.discPct))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? DiscPct
		{
			get
			{
				return this._DiscPct;
			}
			set
			{
				this._DiscPct = value;
			}
		}
		#endregion
		#region GroupDiscountRate
		public abstract class groupDiscountRate : PX.Data.BQL.BqlDecimal.Field<groupDiscountRate> { }
		protected Decimal? _GroupDiscountRate;
		[PXDBDecimal(18, BqlField = typeof(SOLine.groupDiscountRate))]
		[PXDefault(TypeCode.Decimal, "1.0")]
		public virtual Decimal? GroupDiscountRate
		{
			get
			{
				return this._GroupDiscountRate;
			}
			set
			{
				this._GroupDiscountRate = value;
			}
		}
		#endregion
		#region DocumentDiscountRate
		public abstract class documentDiscountRate : PX.Data.BQL.BqlDecimal.Field<documentDiscountRate> { }
		protected Decimal? _DocumentDiscountRate;
		[PXDBDecimal(18, BqlField = typeof(SOLine.documentDiscountRate))]
		[PXDefault(TypeCode.Decimal, "1.0")]
		public virtual Decimal? DocumentDiscountRate
		{
			get
			{
				return this._DocumentDiscountRate;
			}
			set
			{
				this._DocumentDiscountRate = value;
			}
		}
		#endregion
		#region TaxZoneID
		[PXDBString(10, IsUnicode = true, BqlField = typeof(SOLine.taxZoneID))]
		public virtual string TaxZoneID { get; set; }
		public abstract class taxZoneID : BqlString.Field<taxZoneID> { }
		#endregion
		#region TaxCategoryID
		public abstract class taxCategoryID : PX.Data.BQL.BqlString.Field<taxCategoryID> { }
		protected String _TaxCategoryID;
		[PXDBString(TX.TaxCategory.taxCategoryID.Length, IsUnicode = true, BqlField = typeof(SOLine.taxCategoryID))]
		[SOUnbilledMiscTax2(typeof(SOOrder), typeof(SOTax), typeof(SOTaxTran),
			   //Per Unit Tax settings
			   Inventory = typeof(SOMiscLine2.inventoryID), UOM = typeof(SOMiscLine2.uOM), LineQty = typeof(SOMiscLine2.unbilledQty))]
		public virtual String TaxCategoryID
		{
			get
			{
				return this._TaxCategoryID;
			}
			set
			{
				this._TaxCategoryID = value;
			}
		}
		#endregion
		#region SalesPersonID
		public abstract class salesPersonID : PX.Data.BQL.BqlInt.Field<salesPersonID> { }
		protected Int32? _SalesPersonID;
		[SalesPerson(BqlField = typeof(SOLine.salesPersonID))]
		public virtual Int32? SalesPersonID
		{
			get
			{
				return this._SalesPersonID;
			}
			set
			{
				this._SalesPersonID = value;
			}
		}
		#endregion
		#region SalesAcctID
		public abstract class salesAcctID : PX.Data.BQL.BqlInt.Field<salesAcctID> { }
		protected Int32? _SalesAcctID;
		[Account(Visible = false, BqlField = typeof(SOLine.salesAcctID))]
		public virtual Int32? SalesAcctID
		{
			get
			{
				return this._SalesAcctID;
			}
			set
			{
				this._SalesAcctID = value;
			}
		}
		#endregion
		#region SalesSubID
		public abstract class salesSubID : PX.Data.BQL.BqlInt.Field<salesSubID> { }
		protected Int32? _SalesSubID;
		[SubAccount(typeof(SOMiscLine2.salesAcctID), Visible = false, BqlField = typeof(SOLine.salesSubID))]
		public virtual Int32? SalesSubID
		{
			get
			{
				return this._SalesSubID;
			}
			set
			{
				this._SalesSubID = value;
			}
		}
		#endregion
		#region TaskID
		public abstract class taskID : PX.Data.BQL.BqlInt.Field<taskID> { }
		protected Int32? _TaskID;
		[PX.Objects.PM.ActiveProjectTask(typeof(SOLine.projectID), BatchModule.SO, BqlField = typeof(SOLine.taskID), DisplayName = "Project Task")]
		public virtual Int32? TaskID
		{
			get
			{
				return this._TaskID;
			}
			set
			{
				this._TaskID = value;
			}
		}
		#endregion
		#region CostCodeID
		public abstract class costCodeID : PX.Data.BQL.BqlInt.Field<costCodeID> { }
		protected Int32? _CostCodeID;
		[PXDBInt(BqlField = typeof(SOLine.costCodeID))]
		public virtual Int32? CostCodeID
		{
			get
			{
				return this._CostCodeID;
			}
			set
			{
				this._CostCodeID = value;
			}
		}
		#endregion
		#region TranDesc
		public abstract class tranDesc : PX.Data.BQL.BqlString.Field<tranDesc> { }
		protected String _TranDesc;
		[PXDBString(256, IsUnicode = true, BqlField = typeof(SOLine.tranDesc))]
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
		#region NoteID
		public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }
		protected Guid? _NoteID;
		[PXNote(BqlField = typeof(SOLine.noteID))]
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
		#region Commissionable
		public abstract class commissionable : PX.Data.BQL.BqlBool.Field<commissionable> { }
		protected bool? _Commissionable;
		[PXDBBool(BqlField = typeof(SOLine.commissionable))]
		public bool? Commissionable
		{
			get
			{
				return _Commissionable;
			}
			set
			{
				_Commissionable = value;
			}
		}
		#endregion
		#region IsFree
		public abstract class isFree : PX.Data.BQL.BqlBool.Field<isFree> { }
		protected Boolean? _IsFree;
		[PXDBBool(BqlField = typeof(SOLine.isFree))]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Free Item")]
		public virtual Boolean? IsFree
		{
			get
			{
				return this._IsFree;
			}
			set
			{
				this._IsFree = value;
			}
		}
		#endregion
		#region ManualPrice
		public abstract class manualPrice : PX.Data.BQL.BqlBool.Field<manualPrice> { }
		protected Boolean? _ManualPrice;
		[PXDBBool(BqlField = typeof(SOLine.manualPrice))]
		[PXDefault(false)]
		public virtual Boolean? ManualPrice
		{
			get
			{
				return this._ManualPrice;
			}
			set
			{
				this._ManualPrice = value;
			}
		}
		#endregion
		#region ManualDisc
		public abstract class manualDisc : PX.Data.BQL.BqlBool.Field<manualDisc> { }
		protected Boolean? _ManualDisc;
		[PXDBBool(BqlField = typeof(SOLine.manualDisc))]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Manual Discount", Visibility = PXUIVisibility.Visible)]
		public virtual Boolean? ManualDisc
		{
			get
			{
				return this._ManualDisc;
			}
			set
			{
				this._ManualDisc = value;
			}
		}
		#endregion

		#region DiscountID
		public abstract class discountID : PX.Data.BQL.BqlString.Field<discountID> { }
		protected String _DiscountID;
		[PXDBString(10, IsUnicode = true, BqlField = typeof(SOLine.discountID))]
		[PXSelector(typeof(Search<ARDiscount.discountID, Where<ARDiscount.type, Equal<DiscountType.LineDiscount>>>))]
		[PXUIField(DisplayName = "Discount Code", Visible = true, Enabled = false)]
		public virtual String DiscountID
		{
			get
			{
				return this._DiscountID;
			}
			set
			{
				this._DiscountID = value;
			}
		}
		#endregion
		#region DiscountSequenceID
		public abstract class discountSequenceID : PX.Data.BQL.BqlString.Field<discountSequenceID> { }
		protected String _DiscountSequenceID;
		[PXDBString(10, IsUnicode = true, BqlField = typeof(SOLine.discountSequenceID))]
		[PXUIField(DisplayName = "Discount Sequence", Visible = false, Enabled = false)]
		public virtual String DiscountSequenceID
		{
			get
			{
				return this._DiscountSequenceID;
			}
			set
			{
				this._DiscountSequenceID = value;
			}
		}
		#endregion
		#region DRTermStartDate
		public abstract class dRTermStartDate : PX.Data.BQL.BqlDateTime.Field<dRTermStartDate> { }

		protected DateTime? _DRTermStartDate;

		[PXDBDate(BqlField = typeof(SOLine.dRTermStartDate))]
		[PXUIField(DisplayName = "Term Start Date")]
		public DateTime? DRTermStartDate
		{
			get { return _DRTermStartDate; }
			set { _DRTermStartDate = value; }
		}
		#endregion
		#region DRTermEndDate
		public abstract class dRTermEndDate : PX.Data.BQL.BqlDateTime.Field<dRTermEndDate> { }

		protected DateTime? _DRTermEndDate;

		[PXDBDate(BqlField = typeof(SOLine.dRTermEndDate))]
		[PXUIField(DisplayName = "Term End Date")]
		public DateTime? DRTermEndDate
		{
			get { return _DRTermEndDate; }
			set { _DRTermEndDate = value; }
		}
		#endregion
		#region CuryUnitPriceDR
		public abstract class curyUnitPriceDR : PX.Data.BQL.BqlDecimal.Field<curyUnitPriceDR> { }

		protected decimal? _CuryUnitPriceDR;

		[PXUIField(DisplayName = "Unit Price for DR", Visible = false)]
		[PXDBDecimal(typeof(Search<CommonSetup.decPlPrcCst>), BqlField = typeof(SOLine.curyUnitPriceDR))]
		public virtual decimal? CuryUnitPriceDR
		{
			get { return _CuryUnitPriceDR; }
			set { _CuryUnitPriceDR = value; }
		}
		#endregion
		#region LineDiscountDR
		public abstract class discPctDR : PX.Data.BQL.BqlDecimal.Field<discPctDR> { }

		protected decimal? _DiscPctDR;

		[PXUIField(DisplayName = "Discount Percent for DR", Visible = false)]
		[PXDBDecimal(6, MinValue = -100, MaxValue = 100, BqlField = typeof(SOLine.discPctDR))]
		public virtual decimal? DiscPctDR
		{
			get { return _DiscPctDR; }
			set { _DiscPctDR = value; }
		}
		#endregion
		#region DefScheduleID
		public abstract class defScheduleID : PX.Data.BQL.BqlInt.Field<defScheduleID> { }
		protected int? _DefScheduleID;
		[PXDBInt(BqlField = typeof(SOLine.defScheduleID))]
		public virtual int? DefScheduleID
		{
			get
			{
				return this._DefScheduleID;
			}
			set
			{
				this._DefScheduleID = value;
			}
		}
		#endregion

		#region BlanketType
		public abstract class blanketType : Data.BQL.BqlString.Field<blanketType> { }
		[PXDBString(2, IsFixed = true, BqlField = typeof(SOLine.blanketType))]
		public virtual string BlanketType
		{
			get;
			set;
		}
		#endregion
		#region BlanketNbr
		public abstract class blanketNbr : Data.BQL.BqlString.Field<blanketNbr> { }
		[PXDBString(15, IsUnicode = true, BqlField = typeof(SOLine.blanketNbr))]
		public virtual string BlanketNbr
		{
			get;
			set;
		}
		#endregion
		#region BlanketLineNbr
		public abstract class blanketLineNbr : Data.BQL.BqlInt.Field<blanketLineNbr> { }
		[PXDBInt(BqlField = typeof(SOLine.blanketLineNbr))]
		public virtual int? BlanketLineNbr
		{
			get;
			set;
		}
		#endregion
		#region BlanketSplitLineNbr
		public abstract class blanketSplitLineNbr : Data.BQL.BqlInt.Field<blanketSplitLineNbr> { }
		[PXDBInt(BqlField = typeof(SOLine.blanketSplitLineNbr))]
		public virtual int? BlanketSplitLineNbr
		{
			get;
			set;
		}
		#endregion
		#region tstamp
		public abstract class Tstamp : PX.Data.BQL.BqlByteArray.Field<Tstamp> { }

		[PXDBTimestamp(BqlField = typeof(SOLine.Tstamp), VerifyTimestamp = VerifyTimestampOptions.BothFromGraphAndRecord)]
		public virtual byte[] tstamp { get; set; }
		#endregion
	}

	[Serializable()]
	public partial class AddSOFilter : IBqlTable
	{
		#region Operation
		public abstract class operation : PX.Data.BQL.BqlString.Field<operation> { }
		protected String _Operation;
		[PXDBString(1, IsFixed = true, InputMask = ">a")]
		[PXUIField(DisplayName = "Operation")]
		[PXDefault(SOOperation.Issue, typeof(SOShipment.operation))]
		[SOOperation.List]
		public virtual String Operation
		{
			get
			{
				return this._Operation;
			}
			set
			{
				this._Operation = value;
			}
		}
		#endregion
		#region OrderType
		public abstract class orderType : PX.Data.BQL.BqlString.Field<orderType> { }
		protected String _OrderType;
		[PXDBString(2, IsFixed = true, InputMask = ">aa")]
		[PXSelector(typeof(Search2<SOOrderType.orderType,
			InnerJoin<SOOrderTypeOperation, On<SOOrderTypeOperation.FK.OrderType>>,
			Where<SOOrderType.active, Equal<True>, And<SOOrderType.requireShipping, Equal<True>, And<SOOrderTypeOperation.active, Equal<True>,
				And<SOOrderTypeOperation.operation, Equal<Current<AddSOFilter.operation>>,
				And<Where<SOOrderTypeOperation.iNDocType, Equal<INTranType.transfer>, And<Current<SOShipment.shipmentType>, Equal<SOShipmentType.transfer>,
				Or<SOOrderTypeOperation.iNDocType, NotEqual<INTranType.transfer>, And<Current<SOShipment.shipmentType>, Equal<SOShipmentType.issue>>>>>>>>>>>))]
		[PXRestrictor(typeof(Where<Current<SOShipment.createARDoc>, IsNull,
			Or<Current<SOShipment.createARDoc>, Equal<IIf<Where<SOOrderType.aRDocType, Equal<ARDocType.noUpdate>>, False, True>>>>),
			Messages.OrderTypeCantSelectBecauseARDocType, typeof(SOOrderType.orderType))]
		[PXDefault(typeof(Search2<SOOrderType.orderType,
			InnerJoin<SOOrderTypeOperation, On<SOOrderTypeOperation.FK.OrderType>, 
			LeftJoin<SOSetup, On<SOSetup.defaultOrderType, Equal<SOOrderType.orderType>>>>,
			Where<SOOrderType.active, Equal<True>, And<SOOrderType.requireShipping, Equal<True>, And<SOOrderTypeOperation.active, Equal<True>,
				And<SOOrderTypeOperation.operation, Equal<Current<AddSOFilter.operation>>,
				And2<Where<SOOrderTypeOperation.iNDocType, Equal<INTranType.transfer>, And<Current<SOShipment.shipmentType>, Equal<SOShipmentType.transfer>,
					Or<SOOrderTypeOperation.iNDocType, NotEqual<INTranType.transfer>, And<Current<SOShipment.shipmentType>, Equal<SOShipmentType.issue>>>>>,
				And<Where<Current<SOShipment.createARDoc>, IsNull,
					Or<Current<SOShipment.createARDoc>, Equal<IIf<Where<SOOrderType.aRDocType, Equal<ARDocType.noUpdate>>, False, True>>>>>>>>>>,
			OrderBy<Desc<SOSetup.defaultOrderType, Asc<SOOrderType.orderType>>>>))]
		[PXUIField(DisplayName = "Order Type")]
		[PXFormula(typeof(Default<AddSOFilter.operation>))]
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
		#region OrderNbr
		public abstract class orderNbr : PX.Data.BQL.BqlString.Field<orderNbr> { }
		protected String _OrderNbr;
		[PXDBString(15, IsUnicode = true, InputMask = "")]
		[PXUIField(DisplayName = "Order Nbr.")]
		[PXDefault]
		[SO.RefNbr(typeof(Search<SOOrder.orderNbr,
			Where<SOOrder.orderType, Equal<Current<AddSOFilter.orderType>>,
			And<SOOrder.customerID, Equal<Current<SOShipment.customerID>>,
			And2<Where<SOOrder.customerLocationID, Equal<Current<SOShipment.customerLocationID>>, Or<Current<SOShipment.orderCntr>, Equal<int0>>>,
			And<SOOrder.cancelled, Equal<False>,
			And<SOOrder.completed, Equal<False>,
			And<SOOrder.hold, Equal<False>,
			And<SOOrder.creditHold, Equal<False>,
			And<SOOrder.approved, Equal<True>>>>>>>>>>), Filterable = true)]
		[PXFormula(typeof(Default<AddSOFilter.orderType>))]
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

		#region OrderLineNbr
		/// <exclude/>
		public abstract class orderLineNbr : PX.Data.BQL.BqlInt.Field<orderLineNbr> { }
		/// <exclude/>
		[PXDBInt()]
		[PXUIField(DisplayName = "Line Nbr.", Visible = false, Enabled = false)]
		public virtual Int32? OrderLineNbr
		{
			get;
			set;
		}
		#endregion

		#region FreightAmountSource
		public abstract class freightAmountSource : PX.Data.BQL.BqlString.Field<freightAmountSource> { }
		[PXDBString(1, IsFixed = true)]
		[FreightAmountSource]
		[PXFormula(typeof(Selector<AddSOFilter.orderNbr, SOOrder.freightAmountSource>))]
		public virtual string FreightAmountSource
		{
			get;
			set;
		}
		#endregion
		#region AddAllLines
		public abstract class addAllLines : PX.Data.BQL.BqlBool.Field<addAllLines> { }
		protected Boolean? _AddAllLines;
		[PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual Boolean? AddAllLines
		{
			get
			{
				return this._AddAllLines;
			}
			set
			{
				this._AddAllLines = value;
			}
		}
		#endregion
	}
}
