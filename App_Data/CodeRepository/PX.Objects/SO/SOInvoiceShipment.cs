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
using System.Linq;
using PX.Common;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.AR;
using PX.Objects.AR.MigrationMode;
using PX.Objects.Common.Attributes;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Objects.IN;
using PX.Objects.PO;
using PX.SM;

namespace PX.Objects.SO
{
	using static SOInvoiceShipment.WellKnownActions.SOShipmentScreen;

	[GL.TableAndChartDashboardType]
	public class SOInvoiceShipment : PXGraph<SOInvoiceShipment>
	{
		public PXCancel<SOShipmentFilter> Cancel;
		public PXAction<SOShipmentFilter> viewDocument;
		public PXFilter<SOShipmentFilter> Filter;
		[PXFilterable]
		public PXFilteredProcessing<SOShipment, SOShipmentFilter> Orders;
		public PXSelect<SOShipLine> dummy_select_to_bind_events;
		public PXSetup<SOSetup> sosetup;


		[PXUIField(DisplayName = "", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXEditDetailButton]
		public virtual IEnumerable ViewDocument(PXAdapter adapter)
		{
			if (Orders.Current != null)
			{
				if (Orders.Current.ShipmentType == INDocType.DropShip)
				{
					POReceiptEntry docgraph = PXGraph.CreateInstance<POReceiptEntry>();
					var receiptType = Orders.Current.Operation == SOOperation.Receipt
						? POReceiptType.POReturn
						: POReceiptType.POReceipt;
					docgraph.Document.Current = docgraph.Document.Search<POReceipt.receiptNbr>(Orders.Current.ShipmentNbr, receiptType);

					throw new PXRedirectRequiredException(docgraph, true, PO.Messages.POReceipt) { Mode = PXBaseRedirectException.WindowMode.NewWindow };
				}
				else
				{
					SOShipmentEntry docgraph = PXGraph.CreateInstance<SOShipmentEntry>();
					docgraph.Document.Current = docgraph.Document.Search<SOShipment.shipmentNbr>(Orders.Current.ShipmentNbr);

					throw new PXRedirectRequiredException(docgraph, true, Messages.SOShipment) { Mode = PXBaseRedirectException.WindowMode.NewWindow };
				}
			}
			return adapter.Get();
		}

		public PXSelect<INSite> INSites;
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXUIField(DisplayName = Messages.SiteDescr, Visibility = PXUIVisibility.SelectorVisible)]
		protected virtual void _(Events.CacheAttached<INSite.descr> e) { }

		public PXSelect<Carrier> Carriers;
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXUIField(DisplayName = Messages.CarrierDescr, Visibility = PXUIVisibility.SelectorVisible)]
		protected virtual void _(Events.CacheAttached<Carrier.description> e) { }

		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[CopiedNoteID(typeof(SOShipment))]
		protected virtual void _(Events.CacheAttached<SOShipment.noteID> e) { }

		public SOInvoiceShipment()
		{
			ARSetupNoMigrationMode.EnsureMigrationModeDisabled(this);

			Orders.SetSelected<SOShipment.selected>();
			object item = sosetup.Current;
		}

		public virtual void SOShipmentFilter_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			if (e.Row == null) return;

			SOShipmentFilter filter = e.Row as SOShipmentFilter;
			if (filter != null && !String.IsNullOrEmpty(filter.Action))
			{
				Dictionary<string, object> parameters = Filter.Cache.ToDictionary(filter);
				Orders.SetProcessWorkflowAction(filter.Action, parameters);
			}

			PXUIFieldAttribute.SetEnabled<SOShipmentFilter.invoiceDate>(sender, filter, filter.Action.IsIn(CreateInvoice, CreateDropshipInvoice));
			PXUIFieldAttribute.SetEnabled<SOShipmentFilter.packagingType>(sender, filter, filter.Action != CreateDropshipInvoice);
			PXUIFieldAttribute.SetEnabled<SOShipmentFilter.siteID>(sender, filter, filter.Action != CreateDropshipInvoice);
			PXUIFieldAttribute.SetVisible<SOShipmentFilter.showPrinted>(sender, filter, filter.Action.IsIn(PrintShipmentConfirmation, PrintLabels, PrintCommercialInvoices, PrintPickList));

			PXUIFieldAttribute.SetDisplayName<SOShipment.shipmentNbr>(Orders.Cache, filter.Action == CreateDropshipInvoice ? Messages.ReceiptNbr : Messages.ShipmentNbr);
			PXUIFieldAttribute.SetDisplayName<SOShipment.shipDate>(Orders.Cache, filter.Action == CreateDropshipInvoice ? Messages.ReceiptDate : Messages.ShipmentDate);

			if (sosetup.Current.UseShipDateForInvoiceDate == true)
			{
				sender.RaiseExceptionHandling<SOShipmentFilter.invoiceDate>(filter, null, new PXSetPropertyException(Messages.UseInvoiceDateFromShipmentDateWarning, PXErrorLevel.Warning));
				PXUIFieldAttribute.SetEnabled<SOShipmentFilter.invoiceDate>(sender, filter, false);
			}

			bool warnShipNotInvoiced = !this.IsScheduler() &&
				filter.Action == UpdateIN && SOShipmentEntry.NeedWarningShipNotInvoicedUpdateIN(this, sosetup.Current, Orders.SelectMain());
			Exception warnShipNotInvoicedExc = warnShipNotInvoiced
				? new PXSetPropertyException(Messages.ShipNotInvoicedWarning, PXErrorLevel.Warning)
				: null;
			sender.RaiseExceptionHandling<SOShipmentFilter.action>(filter, null, warnShipNotInvoicedExc);

			bool showPrintSettings = IsPrintingAllowed(filter);

			PXUIFieldAttribute.SetVisible<SOShipmentFilter.printWithDeviceHub>(sender, filter, showPrintSettings);
			PXUIFieldAttribute.SetVisible<SOShipmentFilter.definePrinterManually>(sender, filter, showPrintSettings);
			PXUIFieldAttribute.SetVisible<SOShipmentFilter.printerID>(sender, filter, showPrintSettings);
			PXUIFieldAttribute.SetVisible<SOShipmentFilter.numberOfCopies>(sender, filter, showPrintSettings);

			if (PXContext.GetSlot<AUSchedule>() == null)
			{
				PXUIFieldAttribute.SetEnabled<SOShipmentFilter.definePrinterManually>(sender, filter, filter.PrintWithDeviceHub == true);
				PXUIFieldAttribute.SetEnabled<SOShipmentFilter.numberOfCopies>(sender, filter, filter.PrintWithDeviceHub == true);
				PXUIFieldAttribute.SetEnabled<SOShipmentFilter.printerID>(sender, filter, filter.PrintWithDeviceHub == true && filter.DefinePrinterManually == true);
			}

			if (filter.PrintWithDeviceHub != true || filter.DefinePrinterManually != true)
			{
				filter.PrinterID = null;
			}

			bool showInvoiceSeparately = filter.Action.IsIn(CreateInvoice, CreateDropshipInvoice);
			PXUIFieldAttribute.SetEnabled<SOShipment.billSeparately>(Orders.Cache, null, showInvoiceSeparately
				&& PXLongOperation.GetStatus(this.UID) == PXLongRunStatus.NotExists);
			PXUIFieldAttribute.SetVisible<SOShipment.billSeparately>(Orders.Cache, null, showInvoiceSeparately);
			PXUIFieldAttribute.SetVisible<SOShipment.billingInOrders>(Orders.Cache, null, filter.Action == CreateInvoice);

			Orders.Cache.Adjust<CopiedNoteIDAttribute>().For<SOShipment.noteID>(a =>
			{
				bool isPOReceipt = (filter.Action == CreateDropshipInvoice);
				a.EntityType = isPOReceipt ? typeof(POReceipt) : typeof(SOShipment);
				a.GraphType = isPOReceipt ? typeof(POReceiptEntry) : typeof(SOShipmentEntry);
			});
		}

		protected virtual bool IsPrintingAllowed(SOShipmentFilter filter)
		{
			return PXAccess.FeatureInstalled<FeaturesSet.deviceHub>() &&
				filter.Action.IsIn(
					PrintLabels,
					PrintCommercialInvoices,
					PrintPickList,
					PrintShipmentConfirmation);
		}

		protected bool _ActionChanged = false;

		public virtual void SOShipmentFilter_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			_ActionChanged = !sender.ObjectsEqual<SOShipmentFilter.action>(e.Row, e.OldRow);
			if (_ActionChanged && e.Row != null)
			{
				var row = ((SOShipmentFilter)e.Row);

				row.PackagingType = SOPackageType.ForFiltering.Both;

				if (row.Action == CreateDropshipInvoice)
					row.SiteID = null;
			}

			if ((_ActionChanged || !sender.ObjectsEqual<SOShipmentFilter.definePrinterManually>(e.Row, e.OldRow) || !sender.ObjectsEqual<SOShipmentFilter.printWithDeviceHub>(e.Row, e.OldRow))
				&& Filter.Current != null && PXAccess.FeatureInstalled<FeaturesSet.deviceHub>() && Filter.Current.PrintWithDeviceHub == true && Filter.Current.DefinePrinterManually == true
				&& (PXContext.GetSlot<AUSchedule>() == null || !(Filter.Current.PrinterID != null && ((SOShipmentFilter)e.OldRow).PrinterID == null)))
			{
				string actualReportID = SOReports.GetReportID(null, Filter.Current.Action);
				Filter.Current.PrinterID = new NotificationUtility(this).SearchPrinter(ARNotificationSource.Customer, actualReportID, Accessinfo.BranchID);
			}
		}

		protected virtual void SOShipmentFilter_PrinterName_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			SOShipmentFilter row = (SOShipmentFilter)e.Row;
			if (row != null)
			{
				if (!IsPrintingAllowed(row))
					e.NewValue = null;
			}
		}

		public virtual IEnumerable orders()
		{
			PXUIFieldAttribute.SetDisplayName<SOShipment.customerID>(Caches[typeof(SOShipment)], Messages.CustomerID);

			if (Filter.Current.Action == PX.Data.Automation.PXWorkflowMassProcessingAttribute.Undefined)
				yield break;

			if (_ActionChanged)
				Orders.Cache.Clear();

			PXSelectBase cmd = GetShipmentsSelectCommand(Filter.Current);

			if (cmd is PXSelectBase<SOShipment> shCmd)
			{
				ApplyShipmentFilters(shCmd, Filter.Current);

				int startRow = PXView.StartRow;
				int totalRows = 0;

				foreach (object res in shCmd.View.Select(null, null, PXView.Searches, PXView.SortColumns, PXView.Descendings, PXView.Filters, ref startRow, PXView.MaximumRows, ref totalRows))
				{
					SOShipment shipment = PXResult.Unwrap<SOShipment>(res);
					SOShipment cached = (SOShipment)Orders.Cache.Locate(shipment);
					if (cached != null)
					{
						shipment.Selected = cached.Selected;
						shipment.BillSeparately = cached.BillSeparately;
					}
					yield return res;
				}
				PXView.StartRow = 0;
			}

			if (cmd is PXSelectBase<POReceipt> rtCmd)
			{
				ApplyReceiptFilters(rtCmd, Filter.Current);

				int startRow = PXView.StartRow;
				int totalRows = 0;

				string[] sortColumns = AlterDropshipSortColumns(PXView.SortColumns);
				PXFilterRow[] filters = AlterDropshipFilters(PXView.Filters);
				foreach (object res in rtCmd.View.Select(null, null, PXView.Searches, sortColumns, PXView.Descendings, filters, ref startRow, PXView.MaximumRows, ref totalRows))
				{
					POReceipt receipt = PXResult.Unwrap<POReceipt>(res);
					SOShipment shipment = SOShipment.FromDropshipPOReceipt(receipt);
					SOShipment cached = (SOShipment)Orders.Cache.Locate(shipment);

					if (cached == null)
						Orders.Cache.SetStatus(shipment, PXEntryStatus.Held);
					else
					{
						shipment.Selected = cached.Selected;
						shipment.BillSeparately = cached.BillSeparately;
					}
					yield return shipment;
				}
				PXView.StartRow = 0;
			}
			Orders.Cache.IsDirty = false;
		}

		protected virtual PXSelectBase GetShipmentsSelectCommand(SOShipmentFilter filter)
		{
			PXSelectBase cmd;

			switch (filter.Action)
			{
				case CreateInvoice:
					{
						cmd = new
							SelectFrom<SOShipment>.
							InnerJoin<INSite>.On<SOShipment.FK.Site>.
							InnerJoin<Customer>.On<SOShipment.customerID.IsEqual<Customer.bAccountID>>.SingleTableOnly.
							LeftJoin<Carrier>.On<SOShipment.FK.Carrier>.
							Where<
								SOShipment.confirmed.IsEqual<True>.
								And<Match<Customer, AccessInfo.userName.FromCurrent>>.
								And<Match<INSite, AccessInfo.userName.FromCurrent>>.
								And<Exists<
									SelectFrom<SOOrderShipment>.
									Where<
										SOOrderShipment.shipmentNbr.IsEqual<SOShipment.shipmentNbr>.
										And<SOOrderShipment.shipmentType.IsEqual<SOShipment.shipmentType>>.
										And<SOOrderShipment.invoiceNbr.IsNull>.
										And<SOOrderShipment.createARDoc.IsEqual<True>>>>>>.
							View(this);
						break;
					}
				case UpdateIN:
					{
						cmd = new
							SelectFrom<SOShipment>.
							InnerJoin<INSite>.On<SOShipment.FK.Site>.
							LeftJoin<Customer>.On<SOShipment.customerID.IsEqual<Customer.bAccountID>>.SingleTableOnly.
							LeftJoin<Carrier>.On<SOShipment.FK.Carrier>.
							Where<
								SOShipment.confirmed.IsEqual<True>.
								And<
									Customer.bAccountID.IsNull.
									Or<Match<Customer, AccessInfo.userName.FromCurrent>>>.
								And<Match<INSite, AccessInfo.userName.FromCurrent>>.
								And<Exists<
									SelectFrom<SOOrderShipment>.
									Where<
										SOOrderShipment.shipmentNbr.IsEqual<SOShipment.shipmentNbr>.
										And<SOOrderShipment.shipmentType.IsEqual<SOShipment.shipmentType>>.
										And<SOOrderShipment.invtRefNbr.IsNull>.
										And<SOOrderShipment.createINDoc.IsEqual<True>>>>>>.
							View(this);
						break;
					}
				case CreateDropshipInvoice:
					{
						cmd = new
							SelectFrom<POReceipt>.
							Where<
								POReceipt.released.IsEqual<True>
								.And<Exists<SelectFrom<SOOrderShipment>
									.InnerJoin<SOOrderType>.On<SOOrderShipment.FK.OrderType>
									.InnerJoin<Customer>.On<SOOrderShipment.customerID.IsEqual<Customer.bAccountID>>.SingleTableOnly
									.Where<SOOrderShipment.shipmentNbr.IsEqual<POReceipt.receiptNbr>
										.And<SOOrderShipment.shipmentType.IsEqual<SOShipmentType.dropShip>>
										.And<SOOrderShipment.invoiceNbr.IsNull>
										.And<SOOrderShipment.operation.IsEqual<
											Use<Switch<Case<Where<POReceipt.receiptType.IsEqual<POReceiptType.poreturn>>, SOOperation.receipt>, SOOperation.issue>>.AsString>>
									.And<SOOrderShipment.createARDoc.IsEqual<True>>
										.And<Match<Customer, AccessInfo.userName.FromCurrent>>>>>>
							.View(this);

						break;
					}

				default:
					{
						cmd = new
							SelectFrom<SOShipment>.
							InnerJoin<INSite>.On<SOShipment.FK.Site>.
							LeftJoin<Customer>.On<SOShipment.customerID.IsEqual<Customer.bAccountID>>.SingleTableOnly.
							LeftJoin<Carrier>.On<SOShipment.FK.Carrier>.
							Where<
								Match<INSite, AccessInfo.userName.FromCurrent>.
								And<
									Customer.bAccountID.IsNull.
									Or<Match<Customer, AccessInfo.userName.FromCurrent>>>.
								And<Exists<
									SelectFrom<SOOrderShipment>.
									Where<
										SOOrderShipment.shipmentNbr.IsEqual<SOShipment.shipmentNbr>.
										And<SOOrderShipment.shipmentType.IsEqual<SOShipment.shipmentType>>>>>>.
							View(this);
						break;
					}
			}

			return cmd;
		}

		protected virtual void ApplyShipmentFilters(PXSelectBase<SOShipment> shCmd, SOShipmentFilter filter)
		{
			shCmd.WhereAnd<Where<SOShipment.shipDate.IsLessEqual<SOShipmentFilter.endDate.FromCurrent>>>();
			shCmd.WhereAnd<Where<WorkflowAction.IsEnabled<SOShipment, SOShipmentFilter.action>>>();

			if (filter.CustomerID != null)
				shCmd.WhereAnd<Where<SOShipment.customerID.IsEqual<SOShipmentFilter.customerID.FromCurrent>>>();

			if (!string.IsNullOrEmpty(filter.ShipVia))
				shCmd.WhereAnd<Where<SOShipment.shipVia.IsEqual<SOShipmentFilter.shipVia.FromCurrent>>>();

			if (filter.StartDate != null)
				shCmd.WhereAnd<Where<SOShipment.shipDate.IsGreaterEqual<SOShipmentFilter.startDate.FromCurrent>>>();

			if (!string.IsNullOrEmpty(filter.CarrierPluginID))
				shCmd.WhereAnd<Where<Carrier.carrierPluginID.IsEqual<SOShipmentFilter.carrierPluginID.FromCurrent>>>();

			if (filter.SiteID != null)
				shCmd.WhereAnd<Where<SOShipment.siteID.IsEqual<SOShipmentFilter.siteID.FromCurrent>>>();

			if (filter.ShowPrinted == false)
			{
				if (filter.Action == PrintShipmentConfirmation)
					shCmd.WhereAnd<Where<SOShipment.confirmationPrinted.IsEqual<False>>>();
				else if (filter.Action == PrintLabels)
					shCmd.WhereAnd<Where<SOShipment.labelsPrinted.IsEqual<False>>>();
				else if (filter.Action == PrintCommercialInvoices)
					shCmd.WhereAnd<Where<SOShipment.commercialInvoicesPrinted.IsEqual<False>>>();
				else if (filter.Action == PrintPickList)
					shCmd.WhereAnd<Where<SOShipment.pickListPrinted.IsEqual<False>>>();
			}

			if (filter.PackagingType == SOPackageType.ForFiltering.Manual)
				shCmd.WhereAnd<Where<SOShipment.isManualPackage.IsEqual<True>>>();
			else if (filter.PackagingType == SOPackageType.ForFiltering.Auto)
				shCmd.WhereAnd<Where<SOShipment.isManualPackage.IsEqual<False>>>();
		}

		protected virtual void ApplyReceiptFilters(PXSelectBase<POReceipt> rtCmd, SOShipmentFilter filter)
		{
			rtCmd.WhereAnd<Where<POReceipt.receiptDate.IsLessEqual<SOShipmentFilter.endDate.FromCurrent>>>();

			if (filter.CustomerID != null)
				rtCmd.WhereAnd<Where<POReceipt.dropshipCustomerID.IsEqual<SOShipmentFilter.customerID.FromCurrent>>>();

			if (filter.ShipVia != null)
				rtCmd.WhereAnd<Where<POReceipt.dropshipShipVia.IsEqual<SOShipmentFilter.shipVia.FromCurrent>>>();

			if (filter.CarrierPluginID != null)
			{
				rtCmd.Join<InnerJoin<Carrier, On<Carrier.carrierID.IsEqual<POReceipt.dropshipShipVia>>>>();
				rtCmd.WhereAnd<Where<Carrier.carrierPluginID.IsEqual<SOShipmentFilter.carrierPluginID.FromCurrent>>>();
			}

			if (filter.StartDate != null)
				rtCmd.WhereAnd<Where<POReceipt.receiptDate.IsGreaterEqual<SOShipmentFilter.startDate.FromCurrent>>>();
		}

		protected virtual Dictionary<string, string> DropshipFieldsMapping
			=> new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
			{
				{ nameof(SOShipment.shipmentNbr), nameof(POReceipt.receiptNbr) },
				{ nameof(SOShipment.customerID), nameof(POReceipt.dropshipCustomerID) },
				{ nameof(SOShipment.customerLocationID), nameof(POReceipt.dropshipCustomerLocationID) },
				{ nameof(SOShipment.customerOrderNbr), nameof(POReceipt.dropshipCustomerOrderNbr) },
				{ nameof(SOShipment.shipVia), nameof(POReceipt.dropshipShipVia) },
				{ nameof(SOShipment.shipDate), nameof(POReceipt.receiptDate) },
			};

		protected virtual string[] AlterDropshipSortColumns(string[] sortColumns)
		{
			var fieldsMapping = DropshipFieldsMapping;
			return sortColumns.Select(col => fieldsMapping.ContainsKey(col) ? fieldsMapping[col] : col).ToArray();
		}

		protected virtual PXFilterRow[] AlterDropshipFilters(PXView.PXFilterRowCollection filters)
		{
			var newFilters = new List<PXFilterRow>();
			var fieldsMapping = DropshipFieldsMapping;
			foreach (PXFilterRow filter in filters)
			{
				newFilters.Add(
					fieldsMapping.ContainsKey(filter.DataField)
					? new PXFilterRow(filter) { DataField = fieldsMapping[filter.DataField] }
					: filter);
			}
			return newFilters.ToArray();
		}

		public class WellKnownActions
		{
			public class SOShipmentScreen
			{
				public const string ScreenID = "SO302000";

				public const string ConfirmShipment
					= ScreenID + "$" + nameof(SOShipmentEntry.confirmShipmentAction);

				public const string CreateInvoice
					= ScreenID + "$" + nameof(SOShipmentEntry.createInvoice);

				public const string CreateDropshipInvoice
					= ScreenID + "$" + nameof(SOShipmentEntry.createDropshipInvoice);

				public const string UpdateIN
					= ScreenID + "$" + nameof(SOShipmentEntry.UpdateIN);

				public const string PrintLabels
					= ScreenID + "$" + nameof(SOShipmentEntry.printLabels);

				public const string PrintCommercialInvoices
					= ScreenID + "$" + nameof(SOShipmentEntry.printCommercialInvoices);

				public const string EmailShipment
					= ScreenID + "$" + nameof(SOShipmentEntry.emailShipment);

				public const string PrintPickList
					= ScreenID + "$" + nameof(SOShipmentEntry.printPickListAction);

				public const string PrintShipmentConfirmation
					= ScreenID + "$" + nameof(SOShipmentEntry.printShipmentConfirmation);
			}
		}
	}

	/// <summary>
	/// Duplicates disabling condition <see cref="SOShipmentEntry_Workflow.Conditions.IsNotHeldByPicking"/> that is used for <see cref="SOShipmentEntry.confirmShipmentAction"/>, but for mass processing
	/// </summary>
	public class ForbidConfirmationForShipmentsThatHavePickListInProgress : PXGraphExtension<SOInvoiceShipment>
	{
		public static bool IsActive() => PXAccess.FeatureInstalled<FeaturesSet.wMSAdvancedPicking>() || PXAccess.FeatureInstalled<FeaturesSet.wMSPaperlessPicking>();

		/// <summary>
		/// Overrides <see cref="SOInvoiceShipment.ApplyShipmentFilters(PXSelectBase{SOShipment}, SOShipmentFilter)"/>
		/// </summary>
		[PXOverride]
		public virtual void ApplyShipmentFilters(PXSelectBase<SOShipment> shCmd, SOShipmentFilter filter, Action<PXSelectBase<SOShipment>, SOShipmentFilter> base_ApplyShipmentFilters)
		{
			base_ApplyShipmentFilters(shCmd, filter);

			if (filter.Action == ConfirmShipment)
			{
				shCmd.Join<LeftJoin<SOPickingWorksheet, On<SOShipment.FK.Worksheet>>>();
				shCmd.WhereAnd<Where<
					SOShipment.currentWorksheetNbr.IsNull.
					Or<SOShipment.picked.IsEqual<True>>.
					Or<
						SOPickingWorksheet.worksheetType.IsIn<SOPickingWorksheet.worksheetType.wave, SOPickingWorksheet.worksheetType.batch>.
						And<SOPickingWorksheet.status.IsIn<SOPickingWorksheet.status.picked, SOPickingWorksheet.status.completed>>>.
					Or<
						SOPickingWorksheet.worksheetType.IsEqual<SOPickingWorksheet.worksheetType.single>.
						And<SOPickingWorksheet.status.IsNotEqual<SOPickingWorksheet.status.picking>>>
				>>();
			}
		}
	}

	[Serializable]
	public partial class SOShipmentFilter : IBqlTable, PX.SM.IPrintable
	{
		#region Action
		[PX.Data.Automation.PXWorkflowMassProcessing(DisplayName = "Action")]
		public virtual string Action { get; set; }
		public abstract class action : PX.Data.BQL.BqlString.Field<action> { }
		#endregion
		#region SiteID
		[Site(DescriptionField = typeof(INSite.descr))]
		public virtual Int32? SiteID { get; set; }
		public abstract class siteID : PX.Data.BQL.BqlInt.Field<siteID> { }
		#endregion
		#region StartDate
		[PXDBDate]
		[PXDefault]
		[PXUIField(DisplayName = "Start Date", Visibility = PXUIVisibility.SelectorVisible, Required = false)]
		public virtual DateTime? StartDate { get; set; }
		public abstract class startDate : PX.Data.BQL.BqlDateTime.Field<startDate> { }
		#endregion
		#region EndDate
		[PXDBDate]
		[PXUIField(DisplayName = "End Date", Visibility = PXUIVisibility.SelectorVisible)]
		[PXDefault(typeof(AccessInfo.businessDate))]
		public virtual DateTime? EndDate { get; set; }
		public abstract class endDate : PX.Data.BQL.BqlDateTime.Field<endDate> { }
		#endregion
		#region CarrierPluginID
		[PXDBString(15, IsUnicode = true, InputMask = ">aaaaaaaaaaaaaaa")]
		[PXUIField(DisplayName = "Carrier", Visibility = PXUIVisibility.SelectorVisible)]
		[PXSelector(typeof(Search<CarrierPlugin.carrierPluginID>))]
		public virtual String CarrierPluginID { get; set; }
		public abstract class carrierPluginID : PX.Data.BQL.BqlString.Field<carrierPluginID> { }
		#endregion
		#region ShipVia
		[PXDBString(15, IsUnicode = true)]
		[PXUIField(DisplayName = "Ship Via")]
		[PXSelector(typeof(Search<Carrier.carrierID>), DescriptionField = typeof(Carrier.description), CacheGlobal = true)]
		public virtual String ShipVia { get; set; }
		public abstract class shipVia : PX.Data.BQL.BqlString.Field<shipVia> { }
		#endregion
		#region CustomerID
		[Customer(DescriptionField = typeof(Customer.acctName))]
		public virtual int? CustomerID { get; set; }
		public abstract class customerID : PX.Data.BQL.BqlInt.Field<customerID> { }
		#endregion
		#region InvoiceDate
		[PXDBDate]
		[PXUIField(DisplayName = "Invoice Date")]
		[PXDefault(typeof(AccessInfo.businessDate))]
		public virtual DateTime? InvoiceDate { get; set; }
		public abstract class invoiceDate : PX.Data.BQL.BqlDateTime.Field<invoiceDate> { }
		#endregion
		#region PackagingType
		[PXDBString(1, IsFixed = true)]
		[PXDefault(SOPackageType.ForFiltering.Both)]
		[SOPackageType.ForFiltering.List]
		[PXUIField(DisplayName = "Packaging Type")]
		public virtual String PackagingType { get; set; }
		public abstract class packagingType : PX.Data.BQL.BqlString.Field<packagingType> { }
		#endregion
		#region ShowPrinted
		[PXBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Show Printed")]
		public virtual bool? ShowPrinted { get; set; }
		public abstract class showPrinted : PX.Data.BQL.BqlBool.Field<showPrinted> { }
		#endregion
		#region PrintWithDeviceHub
		[PXDBBool]
		[PXDefault(typeof(FeatureInstalled<FeaturesSet.deviceHub>))]
		[PXUIField(DisplayName = "Print with DeviceHub")]
		public virtual bool? PrintWithDeviceHub { get; set; }
		public abstract class printWithDeviceHub : PX.Data.BQL.BqlBool.Field<printWithDeviceHub> { }
		#endregion
		#region DefinePrinterManually
		[PXDBBool]
		[PXDefault(true)]
		[PXUIField(DisplayName = "Define Printer Manually")]
		public virtual bool? DefinePrinterManually { get; set; } = false;
		public abstract class definePrinterManually : PX.Data.BQL.BqlBool.Field<definePrinterManually> { }
		#endregion
		#region PrinterID
		[PXPrinterSelector]
		public virtual Guid? PrinterID { get; set; }
		public abstract class printerID : PX.Data.BQL.BqlGuid.Field<printerID> { }
		#endregion
		#region NumberOfCopies
		[PXDBInt(MinValue = 1)]
		[PXDefault(1)]
		[PXFormula(typeof(Selector<SOShipmentFilter.printerID, PX.SM.SMPrinter.defaultNumberOfCopies>))]
		[PXUIField(DisplayName = "Number of Copies", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual int? NumberOfCopies { get; set; }
		public abstract class numberOfCopies : PX.Data.BQL.BqlInt.Field<numberOfCopies> { }
		#endregion

		public class PackagingTypeListAttribute : SOPackageType.ForFiltering.ListAttribute { }
	}
}
