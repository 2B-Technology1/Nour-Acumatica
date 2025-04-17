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
using PX.Objects.Common;
using PX.Objects.Common.Bql;
using PX.Objects.Common.Extensions;
using PX.Objects.AR;
using PX.Objects.CS;
using PX.Objects.IN;
using PX.Objects.CR;
using PX.SM;

namespace PX.Objects.SO
{
	public class SOPickingWorksheetProcess : PXGraph<SOPickingWorksheetProcess>, PXImportAttribute.IPXPrepareItems, PXImportAttribute.IPXProcess
	{
		public SOPickingWorksheetProcess()
		{
			Shipments.SuppressUpdate = true;
		}

		public class ProcessAction
		{
			public const string None = "N";
			public const string CreateWavePickList = "W";
			public const string CreateBatchPickList = "B";
			public const string CreateSinglePickList = "S";

			public class none : BqlString.Constant<none> { public none() : base(None) { } }
			public class createWavePickList : BqlString.Constant<createWavePickList> { public createWavePickList() : base(CreateWavePickList) { } }
			public class createBatchPickList : BqlString.Constant<createBatchPickList> { public createBatchPickList() : base(CreateBatchPickList) { } }
			public class createSinglePickList : BqlString.Constant<createSinglePickList> { public createSinglePickList() : base(CreateSinglePickList) { } }

			[PXLocalizable]
			public abstract class DisplayNames
			{
				public const string None = "<SELECT>";
				public const string CreateWavePickList = "Create Wave Pick Lists";
				public const string CreateBatchPickList = "Create Batch Pick Lists";
				public const string CreateSinglePickList = "Create Single-Shipment Pick Lists";
			}

			public class ListAttribute : PXStringListAttribute
			{
				public ListAttribute() : base
				(
					Pair(None, DisplayNames.None),
					Pair(CreateSinglePickList, DisplayNames.CreateSinglePickList),
					Pair(CreateWavePickList, DisplayNames.CreateWavePickList),
					Pair(CreateBatchPickList, DisplayNames.CreateBatchPickList)
				) { }
			}
		}

		#region DACs
		[PXCacheName(CacheNames.Filter)]
		public class HeaderFilter : IBqlTable
		{
			#region Action
			[PXString(15, IsUnicode = true)]
			[PXUIField(DisplayName = "Action", Required = true)]
			[PXUnboundDefault(typeof(ProcessAction.none.When<Where<FeatureInstalled<FeaturesSet.wMSAdvancedPicking>>>.Else<ProcessAction.createSinglePickList>))]
			[ProcessAction.List]
			public virtual String Action { get; set; }
			public abstract class action : BqlString.Field<action> { }
			#endregion
			#region SiteID
			[Site(Required = true)]
			[InterBranchRestrictor(typeof(Where<SameOrganizationBranch<INSite.branchID, Current<AccessInfo.branchID>>>))]
			public virtual Int32? SiteID { get; set; }
			public abstract class siteID : BqlInt.Field<siteID> { }
			#endregion
			#region StartDate
			[PXDate]
			[PXUIField(DisplayName = "Start Date")]
			public virtual DateTime? StartDate { get; set; }
			public abstract class startDate : BqlDateTime.Field<startDate> { }
			#endregion
			#region EndDate
			[PXDate]
			[PXUnboundDefault(typeof(AccessInfo.businessDate))]
			[PXUIField(DisplayName = "End Date")]
			public virtual DateTime? EndDate { get; set; }
			public abstract class endDate : BqlDateTime.Field<endDate> { }
			#endregion
			#region CustomerID
			[Customer]
			public virtual Int32? CustomerID { get; set; }
			public abstract class customerID : BqlInt.Field<customerID> { }
			#endregion
			#region CarrierPluginID
			[PXString(15, IsUnicode = true, InputMask = ">aaaaaaaaaaaaaaa")]
			[PXUIField(DisplayName = "Carrier", Visibility = PXUIVisibility.SelectorVisible)]
			[PXSelector(typeof(Search<CarrierPlugin.carrierPluginID>))]
			public virtual String CarrierPluginID { get; set; }
			public abstract class carrierPluginID : BqlString.Field<carrierPluginID> { }
			#endregion
			#region ShipVia
			[PXString(15, IsUnicode = true)]
			[PXUIField(DisplayName = "Ship Via")]
			[PXSelector(typeof(Search<Carrier.carrierID, Where<carrierPluginID.FromCurrent.IsNull.Or<Carrier.carrierPluginID.IsEqual<carrierPluginID.FromCurrent>>>>), DescriptionField = typeof(Carrier.description), CacheGlobal = true)]
			public virtual String ShipVia { get; set; }
			public abstract class shipVia : BqlString.Field<shipVia> { }
			#endregion
			#region PackagingType
			[PXString(1, IsFixed = true)]
			[PXUnboundDefault(SOPackageType.ForFiltering.Both)]
			[SOPackageType.ForFiltering.List]
			[PXUIField(DisplayName = "Packaging Type")]
			public virtual String PackagingType { get; set; }
			public abstract class packagingType : BqlString.Field<packagingType> { }
			#endregion
			#region InventoryID
			[Inventory]
			public virtual Int32? InventoryID { get; set; }
			public abstract class inventoryID : BqlInt.Field<inventoryID> { }
			#endregion
			#region LocationID
			[Location(typeof(siteID))]
			public virtual Int32? LocationID { get; set; }
			public abstract class locationID : BqlInt.Field<locationID> { }
			#endregion
			#region MaxNumberOfLinesInShipment
			[PXInt]
			[PXUIField(DisplayName = "Max. Number of Lines in Shipment")]
			public int? MaxNumberOfLinesInShipment { get; set; }
			public abstract class maxNumberOfLinesInShipment : BqlInt.Field<maxNumberOfLinesInShipment> { }
			#endregion
			#region MaxQtyInLines
			[PXInt]
			[PXUIField(DisplayName = "Max. Quantity in Lines")]
			public int? MaxQtyInLines { get; set; }
			public abstract class maxQtyInLines : BqlInt.Field<maxQtyInLines> { }
			#endregion
		}

		// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
		public sealed class HeaderSettings : PXCacheExtension<HeaderFilter>, PX.SM.IPrintable
		{
			#region NumberOfPickers
			[PXInt(MinValue = 1)]
			[PXUnboundDefault(1)]
			[PXUIField(DisplayName = "Max. Number of Pickers")]
			[PXUIVisible(typeof(HeaderFilter.action.IsIn<ProcessAction.createWavePickList, ProcessAction.createBatchPickList>))]
			public int? NumberOfPickers { get; set; }
			public abstract class numberOfPickers : BqlInt.Field<numberOfPickers> { }
			#endregion
			#region NumberOfTotesPerPicker
			[PXInt(MinValue = 1)]
			[PXUnboundDefault(0, PersistingCheck = PXPersistingCheck.Nothing)]
			[PXUIField(DisplayName = "Max. Number of Totes per Picker")]
			[PXUIVisible(typeof(HeaderFilter.action.IsEqual<ProcessAction.createWavePickList>))]
			public int? NumberOfTotesPerPicker { get; set; }
			public abstract class numberOfTotesPerPicker : BqlInt.Field<numberOfTotesPerPicker> { }
			#endregion
			#region SendToQueue
			[PXBool]
			[PXUnboundDefault(false)]
			[PXUIField(DisplayName = "Send to Picking Queue")]
			[PXUIVisible(typeof(HeaderFilter.action.IsIn<ProcessAction.createWavePickList, ProcessAction.createBatchPickList, ProcessAction.createSinglePickList>))]
			public bool? SendToQueue { get; set; }
			public abstract class sendToQueue : BqlBool.Field<sendToQueue> { }
			#endregion
			#region PrintPickLists
			[PXBool]
			[PXUnboundDefault(false)]
			[PXUIField(DisplayName = "Print Pick Lists")]
			[PXUIVisible(typeof(HeaderFilter.action.IsIn<ProcessAction.createWavePickList, ProcessAction.createBatchPickList, ProcessAction.createSinglePickList>))]
			public bool? PrintPickLists { get; set; }
			public abstract class printPickLists : BqlBool.Field<printPickLists> { }
			#endregion
			#region AutomaticShipmentConfirmation
			[PXBool]
			[PXUnboundDefault(false)]
			[PXFormula(typeof(False.When<HeaderFilter.action.IsNotEqual<ProcessAction.createSinglePickList>>.Else<automaticShipmentConfirmation>))]
			[PXUIField(DisplayName = "Confirm Shipment on Pick List Confirmation")]
			[PXUIVisible(typeof(HeaderFilter.action.IsEqual<ProcessAction.createSinglePickList>))]
			public bool? AutomaticShipmentConfirmation { get; set; }
			public abstract class automaticShipmentConfirmation : BqlBool.Field<automaticShipmentConfirmation> { }
			#endregion
			#region PrintWithDeviceHub
			[PXBool]
			[PXUnboundDefault(typeof(printPickLists.IsEqual<True>.And<FeatureInstalled<FeaturesSet.deviceHub>>))]
			[PXFormula(typeof(False.When<printPickLists.IsEqual<False>>.Else<printWithDeviceHub>))]
			[PXUIField(DisplayName = "Print with DeviceHub", FieldClass = "DeviceHub")]
			[PXUIVisible(typeof(printPickLists))]
			public bool? PrintWithDeviceHub { get; set; }
			public abstract class printWithDeviceHub : BqlBool.Field<printWithDeviceHub> { }
			#endregion
			#region DefinePrinterManually
			[PXBool]
			[PXUnboundDefault(true)]
			[PXUIVisible(typeof(printWithDeviceHub))]
			[PXUIEnabled(typeof(printWithDeviceHub))]
			[PXFormula(typeof(False.When<printWithDeviceHub.IsEqual<False>>.Else<definePrinterManually>))]
			[PXUIField(DisplayName = "Define Printer Manually", FieldClass = "DeviceHub")]
			public bool? DefinePrinterManually { get; set; } = false;
			public abstract class definePrinterManually : BqlBool.Field<definePrinterManually> { }
			#endregion
			#region Printer
			[PX.SM.PXPrinterSelector]
			[PXUIVisible(typeof(definePrinterManually))]
			[PXFormula(typeof(Null.When<definePrinterManually.IsEqual<False>>.Else<printerID>))]
			public Guid? PrinterID { get; set; }
			public abstract class printerID: BqlGuid.Field<printerID> { }
			#endregion
			#region NumberOfCopies
			[PXInt]
			public int? NumberOfCopies { get; set; }
			public abstract class numberOfCopies : BqlGuid.Field<numberOfCopies> { }
			#endregion
		}
		#endregion

		#region Views
		public PXFilter<HeaderFilter> Filter;

		[PXFilterable]
		public PXFilteredProcessing<SOShipment, HeaderFilter> Shipments;
		public IEnumerable shipments() => (Filter.Current?.Action).IsIn(null, ProcessAction.None) ? Array.Empty<SOShipment>() : GetShipments();
		#endregion

		#region Actions
		public PXCancel<HeaderFilter> Cancel;

		[PXSuppressActionValidation]
		[PXUIField(DisplayName = "", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXEditDetailButton]
		public virtual IEnumerable ViewDocument(PXAdapter adapter)
		{
			if (Shipments.Current != null)
			{
				SOShipmentEntry docgraph = PXGraph.CreateInstance<SOShipmentEntry>();
				docgraph.Document.Current = docgraph.Document.Search<SOShipment.shipmentNbr>(Shipments.Current.ShipmentNbr);

				throw new PXRedirectRequiredException(docgraph, true, Messages.SOShipment) { Mode = PXBaseRedirectException.WindowMode.NewWindow };
			}
			return adapter.Get();
		}
		#endregion

		#region DAC overrides
		#region INSite
		public SelectFrom<INSite>.View INSites;
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXUIField(DisplayName = Messages.SiteDescr, Visibility = PXUIVisibility.SelectorVisible)]
		protected virtual void _(Events.CacheAttached<INSite.descr> e) { }
		#endregion
		#region Carrier
		public SelectFrom<Carrier>.View Carriers;

		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXUIField(DisplayName = Messages.CarrierDescr, Visibility = PXUIVisibility.SelectorVisible)]
		protected virtual void _(Events.CacheAttached<Carrier.description> e) { }
		#endregion
		#region Location
		public SelectFrom<CR.Location>.View DummyLocation;

		[PXMergeAttributes]
		[PXCustomizeBaseAttribute(typeof(CS.LocationRawAttribute), nameof(CS.LocationRawAttribute.DisplayName), "Customer Location ID")]
		protected virtual void _(Events.CacheAttached<CR.Location.locationCD> e) { }

		[PXMergeAttributes]
		[PXCustomizeBaseAttribute(typeof(PXUIFieldAttribute), nameof(PXUIFieldAttribute.DisplayName), "Customer Location Name")]
		protected virtual void _(Events.CacheAttached<CR.Location.descr> e) { }
		#endregion
		#region SOShipment
		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXSelector(typeof(SOPickingWorksheet.worksheetNbr))]
		[PXUIVisible(typeof(HeaderFilter.action.FromCurrent.IsNotEqual<ProcessAction.createSinglePickList>))]
		protected void _(Events.CacheAttached<SOShipment.currentWorksheetNbr> e) { }
		#endregion
		#endregion

		#region Event handlers
		protected virtual void _(Events.RowSelected<HeaderFilter> e)
		{
			string action = e.Row.Action;
			var settings = e.Row.GetExtension<HeaderSettings>();
			Shipments.SetProcessDelegate(list => ProcessShipmentsHandler(action, settings, list));

			if (PXContext.GetSlot<AUSchedule>() == null)
			{
				PXUIFieldAttribute.SetEnabled<HeaderSettings.printerID>(e.Cache, e.Row, settings.PrintWithDeviceHub == true && settings.DefinePrinterManually == true);
			}
		}

		protected virtual void _(Events.RowUpdated<HeaderFilter> e)
		{
			var settings = e.Row.GetExtension<HeaderSettings>();
			var OldSettings = e.OldRow.GetExtension<HeaderSettings>();

			if (!e.Cache.ObjectsEqual<HeaderFilter.action, HeaderSettings.definePrinterManually, HeaderSettings.printWithDeviceHub>(e.Row, e.OldRow) &&
				PXAccess.FeatureInstalled<FeaturesSet.deviceHub>() && settings != null && settings.PrintWithDeviceHub == true && settings.DefinePrinterManually == true
				&& (PXContext.GetSlot<AUSchedule>() == null || !(settings.PrinterID != null && OldSettings.PrinterID == null)))
			{
				settings.PrinterID = new NotificationUtility(this).SearchPrinter(SONotificationSource.Customer, SOReports.PrintPickList, Accessinfo.BranchID);
			}
		}

		protected virtual void _(Events.RowSelected<SOShipment> e)
		{
			if (e.Row == null) return;

			if (HasNonStockLinesWithEmptyLocation(e.Row.ShipmentNbr))
			{
				e.Cache.RaiseExceptionHandling<SOShipment.shipmentNbr>(e.Row, e.Row.ShipmentNbr, new PXSetPropertyException(Msg.ShipmentContainsNonStockItemWithEmptyLocation, PXErrorLevel.RowError, e.Row.ShipmentNbr));
			}
		}

		protected virtual void _(Events.FieldVerifying<SOShipment, SOShipment.selected> e)
		{
			if (e.Row == null) return;

			if (((bool?)e.NewValue) == true)
			{
				if (HasNonStockLinesWithEmptyLocation(e.Row.ShipmentNbr))
				{
					e.NewValue = false;
				}
			}
		}
		#endregion

		#region PXImportAttribute.IPXPrepareItems and PXImportAttribute.IPXProcess implementations
		public virtual bool PrepareImportRow(string viewName, IDictionary keys, IDictionary values) => true;

		public virtual bool RowImporting(string viewName, object row) => row == null;

		public virtual bool RowImported(string viewName, object row, object oldRow) => oldRow == null;

		public virtual void PrepareItems(string viewName, IEnumerable items) { }

		public virtual void ImportDone(PXImportAttribute.ImportMode.Value mode) { }
		#endregion

		protected HeaderSettings ProcessSettings { get; set; }

		protected virtual IEnumerable<SOShipment> GetShipments()
		{
			HeaderFilter filter = Filter.Current;

			BqlCommand cmd = new
				SelectFrom<SOShipment>.
				LeftJoin<Carrier>.On<SOShipment.FK.Carrier>.
				InnerJoin<INSite>.On<SOShipment.FK.Site>.
				InnerJoin<SOOrderShipment>.On<
					SOOrderShipment.shipmentType.IsEqual<SOShipment.shipmentType>.
					And<SOOrderShipment.shipmentNbr.IsEqual<SOShipment.shipmentNbr>>>.
				InnerJoin<SOOrder>.On<SOOrderShipment.FK.Order>.
				LeftJoin<Customer>.On<SOOrder.customerID.IsEqual<Customer.bAccountID>>.SingleTableOnly.
				Where<
					Match<INSite, AccessInfo.userName.FromCurrent>.
					And<
						Customer.bAccountID.IsNull.
						Or<Match<Customer, AccessInfo.userName.FromCurrent>>>.
					And<SOShipment.shipDate.IsLessEqual<HeaderFilter.endDate.FromCurrent>>.
					And<SOShipment.siteID.IsEqual<HeaderFilter.siteID.FromCurrent>>.
					And<SOShipment.status.IsEqual<SOShipmentStatus.open>>.
					And<SOShipment.operation.IsEqual<SOOperation.issue>>.
					And<Not<Exists<
						SelectFrom<SOShipLineSplit>.
						Where<
							SOShipLineSplit.shipmentNbr.IsEqual<SOShipment.shipmentNbr>.
							And<SOShipLineSplit.pickedQty.IsGreater<decimal0>>>
					>>>>.
				AggregateTo<
					GroupBy<SOShipment.shipmentNbr>>.
				View(this).View.BqlSelect;
			var parameters = new List<object>();

			cmd = AppendFilter(cmd, parameters, filter);

			var view = new PXView(this, false, cmd);
			int startRow = PXView.StartRow;
			int totalRows = 0;

			List<PXFilterRow> newFilters = new List<PXFilterRow>();
			foreach (PXFilterRow f in PXView.Filters)
			{
				if (string.Equals(f.DataField, nameof(SOOrder.CustomerOrderNbr), StringComparison.OrdinalIgnoreCase))
					f.DataField = nameof(SOOrder) + "__" + nameof(SOOrder.CustomerOrderNbr);
				newFilters.Add(f);
			}

			foreach (object res in view.Select(null, parameters.ToArray(), PXView.Searches, PXView.SortColumns, PXView.Descendings, newFilters.ToArray(), ref startRow, PXView.MaximumRows, ref totalRows))
			{
				SOShipment shipment = PXResult.Unwrap<SOShipment>(res);
				SOOrder order = PXResult.Unwrap<SOOrder>(res);

				if (shipment.BilledOrderCntr + shipment.UnbilledOrderCntr + shipment.ReleasedOrderCntr == 1)
					shipment.CustomerOrderNbr = order.CustomerOrderNbr;

				SOShipment cached = Shipments.Locate(shipment);
				if (cached != null)
					shipment.Selected = cached.Selected;
				yield return shipment;
			}

			PXView.StartRow = 0;
			Shipments.Cache.IsDirty = false;
		}

		public virtual BqlCommand AppendFilter(BqlCommand cmd, IList<object> parameters, HeaderFilter filter)
		{
			if (!string.IsNullOrEmpty(filter.CarrierPluginID))
				cmd = cmd.WhereAnd<Where<Carrier.carrierPluginID.IsEqual<HeaderFilter.carrierPluginID.FromCurrent>>>();

			if (!string.IsNullOrEmpty(filter.ShipVia))
				cmd = cmd.WhereAnd<Where<SOShipment.shipVia.IsEqual<HeaderFilter.shipVia.FromCurrent>>>();

			if (filter.StartDate != null)
				cmd = cmd.WhereAnd<Where<SOShipment.shipDate.IsGreaterEqual<HeaderFilter.startDate.FromCurrent>>>();

			if (filter.CustomerID != null)
				cmd = cmd.WhereAnd<Where<SOShipment.customerID.IsEqual<HeaderFilter.customerID.FromCurrent>>>();

			if (filter.PackagingType == SOPackageType.ForFiltering.Manual)
				cmd = cmd.WhereAnd<Where<SOOrder.isManualPackage.IsEqual<True>>>();
			else if (filter.PackagingType == SOPackageType.ForFiltering.Auto)
				cmd = cmd.WhereAnd<Where<SOOrder.isManualPackage.IsNotEqual<True>>>();

			if (filter.Action.IsIn(ProcessAction.CreateBatchPickList, ProcessAction.CreateWavePickList, ProcessAction.CreateSinglePickList))
				cmd = cmd.WhereAnd<Where<SOShipment.currentWorksheetNbr.IsNull>>();

			if (filter.MaxNumberOfLinesInShipment.IsNotIn(null, 0) ||
				filter.MaxQtyInLines.IsNotIn(null, 0))
			{
				cmd = cmd.WhereAnd<Where<Exists<
					SelectFrom<SOShipLine>.
					Where<
						SOShipLine.shipmentNbr.IsEqual<SOShipment.shipmentNbr>>.
					AggregateTo<
						Count,
						Max<SOShipLine.shippedQty>>.
					Having<
						Count.IsLessEqual<@P.AsInt>.
						And<SOShipLine.shippedQty.Maximized.IsLessEqual<@P.AsDecimal>>
				>>>>();

				parameters.Add(filter.MaxNumberOfLinesInShipment.IsNotIn(null, 0) ? filter.MaxNumberOfLinesInShipment.Value : int.MaxValue);
				parameters.Add(filter.MaxQtyInLines.IsNotIn(null, 0) ? filter.MaxQtyInLines.Value : int.MaxValue);
			}

			return cmd;
		}

		private static void ProcessShipmentsHandler(string action, HeaderSettings settings, IEnumerable<SOShipment> shipments)
			=> CreateInstance<SOPickingWorksheetProcess>().ProcessShipments(action, settings, shipments);

		protected virtual void ProcessShipments(string action, HeaderSettings settings, IEnumerable<SOShipment> shipments)
		{
			foreach (SOShipment shipment in shipments)
			{
				if (!ValidateShipment(shipment, action))
					throw new PXInvalidOperationException(Msg.ShipmentIsAlreadyProcessedOnPickPackShip, shipment.ShipmentNbr);
			}
			
			ProcessSettings = settings;
			switch (action)
			{
				case ProcessAction.CreateWavePickList:
					CreateWavePickList(settings, shipments);
					break;
				case ProcessAction.CreateBatchPickList:
					CreateBatchPickList(settings, shipments);
					break;
				case ProcessAction.CreateSinglePickList:
					CreateSinglePickList(settings, shipments);
					break;
				default:
					break;
			}
		}

		protected virtual bool ValidateShipment(SOShipment shipment, [ProcessAction.List] string processAction)
		{
			return
			SelectFrom<SOShipment>.
			Where<
				SOShipment.shipmentNbr.IsEqual<@P.AsString>.
				And<SOShipment.status.IsEqual<SOShipmentStatus.open>>.
				And<SOShipment.operation.IsEqual<SOOperation.issue>>.
				And<SOShipment.currentWorksheetNbr.IsNull>.
				And<NotExists<
						SelectFrom<SOShipLineSplit>.
						Where<SOShipLineSplit.shipmentNbr.IsEqual<SOShipment.shipmentNbr>.
						And<SOShipLineSplit.pickedQty.IsGreater<decimal0>>>>>>.
			View.ReadOnly.Select(this, shipment.ShipmentNbr).Any_();
		}

		private PXAdapter CreateAdapter<TRow>(PXGraph graph, HeaderSettings settings, IEnumerable<TRow> rows) where TRow : class, IBqlTable, new()
			=> new PXAdapter(PXView.Dummy.For(graph, rows))
			{
				MassProcess = true,
				Arguments =
				{
					[nameof(PX.SM.IPrintable.PrintWithDeviceHub)] = settings.PrintWithDeviceHub,
					[nameof(PX.SM.IPrintable.DefinePrinterManually)] = settings.DefinePrinterManually,
					[nameof(PX.SM.IPrintable.PrinterID)] = settings.PrinterID,
					[nameof(PX.SM.IPrintable.NumberOfCopies)] = 1
				}
			};

		#region Wave Pick Lists
		protected virtual void CreateWavePickList(HeaderSettings settings, IEnumerable<SOShipment> shipments)
		{
			if (shipments.Any() == false)
				return;

			if (HasAnyShipmentContainsNonStockShipLinesWithEmptyLocation(shipments))
				throw new PXInvalidOperationException(Msg.OneOrMultipleShipmentContainsNonStockItemWithEmptyLocationSummary);

			if (shipments.GroupBy(s => s.SiteID).Count() > 1)
				throw new PXInvalidOperationException(Msg.SingleSiteIDForAllShipmentsRequired);

			int actualShipmentCountPerPicker = (settings.NumberOfTotesPerPicker ?? 0) != 0
				? settings.NumberOfTotesPerPicker ?? 0
				: (int)Math.Ceiling(Decimal.Divide(shipments.Count(), settings.NumberOfPickers ?? 0));
			int actualPickerCount = (int)Math.Ceiling(Decimal.Divide(shipments.Count(), actualShipmentCountPerPicker));

			if (actualPickerCount > (settings.NumberOfPickers ?? 0))
				throw new PXInvalidOperationException(Msg.OverallTotesShouldBeNotLessThanShipments);

			IEnumerable<SOPickingWorksheetLineSplit> worksheetLineSplits = MergeSplits(shipments);

			if (worksheetLineSplits.Any())
			{
				var worksheetGraph = CreateInstance<SOPickingWorksheetReview>();

				(SOPickingWorksheet worksheet, var lines, var splits) = CreateWorksheet(worksheetGraph, SOPickingWorksheet.worksheetType.Wave, shipments.First().SiteID, worksheetLineSplits);
				LinkShipments(worksheetGraph, shipments, worksheet);
				SpreadShipmentsAmongPickers(worksheetGraph, shipments, actualPickerCount, actualShipmentCountPerPicker);

				worksheetGraph.Save.Press();

				if (settings.PrintPickLists == true)
					worksheetGraph.PrintPickList.Press(CreateAdapter(worksheetGraph, settings, new[] { worksheet })).Consume();
			}
		}

		protected virtual void SpreadShipmentsAmongPickers(SOPickingWorksheetReview worksheetGraph, IEnumerable<SOShipment> shipments, int pickerCount, int shipmentCountPerPicker)
		{
			var shArr = shipments.ToArray();
			int? siteID = shArr[0].SiteID;

			IReadOnlyDictionary<string, IEnumerable<(SOShipLineSplit Split, SOShipLine Line)>> splitsOfShipment =
				shipments
				.ToDictionary(
					s => s.ShipmentNbr,
					s => GetShipmentSplits(s)
						.Select(ss => (Split: ss.GetItem<SOShipLineSplit>(), Line: ss.GetItem<SOShipLine>()))
						.ToArray()
						.AsEnumerable())
				.AsReadOnly();

			var fullPath = new WMSPath(
				"FullPath",
				splitsOfShipment
					.SelectMany(kvp => kvp.Value)
					.Select(s => s.Split.LocationID)
					.With(GetSortedLocations));

			IEnumerable<WMSPath> shipmentPaths =
				splitsOfShipment
				.Select(sh => fullPath.GetIntersectionWith(sh.Value.Select(s => s.Split.LocationID), newName: sh.Key))
				.With(paths => WMSPath.SortPaths(paths, includePathLength: true))
				.ToArray();

			IReadOnlyList<List<WMSPath>> shipmentsOfPicker =
				Enumerable
				.Range(0, pickerCount)
				.Select(n =>
					shipmentPaths
					.Skip(n * shipmentCountPerPicker)
					.Take(shipmentCountPerPicker)
					.ToList())
				.ToArray()
				.With(Array.AsReadOnly);

			List<WMSPath> lastPickerShipments = shipmentsOfPicker.Last();
			if (lastPickerShipments.Count < shipmentCountPerPicker)
				lastPickerShipments.AddRange(Enumerable.Range(0, shipmentCountPerPicker - lastPickerShipments.Count).Select(n => WMSPath.MakeFake($"fake{n}")));

			WMSPath[] fullPathOfPicker =
				shipmentsOfPicker
				.Select((shs, i) => WMSPath.MergePaths(shs, $"picker{i}"))
				.ToArray();

			if (shipmentCountPerPicker != 1)
				BalancePathsAmongPickers(shipmentsOfPicker, fullPathOfPicker);

			for (int pickerNbr = 0; pickerNbr < shipmentsOfPicker.Count; pickerNbr++)
			{
				WMSPath path = fullPathOfPicker[pickerNbr];
				List<WMSPath> assignedShipments = shipmentsOfPicker[pickerNbr];

				worksheetGraph.pickers.Insert(new SOPicker
				{
					NumberOfTotes = assignedShipments.Count,
					PathLength = path.PathLength,
					FirstLocationID = path.First().LocationID,
					LastLocationID = path.Last().LocationID
				});

				if (PXAccess.FeatureInstalled<FeaturesSet.wMSPaperlessPicking>())
					worksheetGraph.pickerJob.Insert(new SOPickingJob
					{
						Status = ProcessSettings.SendToQueue == true
							? SOPickingJob.status.Enqueued
							: default,
						AutomaticShipmentConfirmation = ProcessSettings.AutomaticShipmentConfirmation
					});

				foreach (var shipmentPath in assignedShipments.Where(sh => sh.IsFake == false))
				{
					PXCache<SOPickerToShipmentLink>.Insert(worksheetGraph, new SOPickerToShipmentLink { ShipmentNbr = shipmentPath.Name, SiteID = siteID });

					foreach ((SOShipLineSplit split, SOShipLine line) in splitsOfShipment[shipmentPath.Name])
						worksheetGraph.pickerList.Insert(PickerListEntryFrom(split, line.UOM, shipmentPath.Name));
				}
			}

			worksheetGraph.EnsureCachePersistence<SOPickerToShipmentLink>();
		}

		protected virtual void BalancePathsAmongPickers(IReadOnlyList<List<WMSPath>> pathsOfPicker, WMSPath[] fullPathOfPicker)
		{
			while (true)
			{
				bool exchangedOccurred = false;

				for (int pickerNbr = 0; pickerNbr < pathsOfPicker.Count - 1; pickerNbr++)
				{
					IReadOnlyCollection<int> pickersToExchange =
						Enumerable
						.Range(0, fullPathOfPicker.Length)
						.Skip(pickerNbr + 1)
						.Where(pn => pn == pickerNbr + 1 || fullPathOfPicker[pickerNbr].GetIntersectionWith(fullPathOfPicker[pn], null).With(inter => inter.IsEmpty == false))
						.ToArray()
						.With(Array.AsReadOnly);

					foreach (int pickerToExchange in pickersToExchange)
					{
						var pairsToExchange =
							(from leftPath in pathsOfPicker[pickerNbr].Where(lp =>
								lp.IsFake || fullPathOfPicker[pickerNbr].EndsWith(lp) || fullPathOfPicker[pickerToExchange].Contains(lp))
							 from rightPath in pathsOfPicker[pickerToExchange].Where(rp =>
								 rp.IsFake || fullPathOfPicker[pickerToExchange].EndsWith(rp) || fullPathOfPicker[pickerNbr].Contains(rp))
							 where leftPath != null && rightPath != null && !(leftPath.IsFake && rightPath.IsFake)
							 select (leftPath: leftPath, rightPath: rightPath))
								.ToArray();

						var modifiedPickerPaths =
							pairsToExchange
							.Select(pair => new
							{
								LeftPickerNewFullPath =
									pathsOfPicker[pickerNbr]
									.Except(pair.leftPath)
									.Append(pair.rightPath)
									.With(ps => WMSPath.MergePaths(ps, $"picker{pickerNbr}")),
								RightPickerNewFullPath =
									pathsOfPicker[pickerToExchange]
									.Except(pair.rightPath)
									.Append(pair.leftPath)
									.With(ps => WMSPath.MergePaths(ps, $"picker{pickerToExchange}"))
							})
							.ToArray();

						var exchageProfits =
							modifiedPickerPaths
							.Select(pair =>
								fullPathOfPicker[pickerNbr].PathLength - pair.LeftPickerNewFullPath.PathLength
								+ fullPathOfPicker[pickerToExchange].PathLength - pair.RightPickerNewFullPath.PathLength)
							.ToArray();

						if (exchageProfits.Any(profit => profit > 0) == false)
							continue;

						int maxProfit = exchageProfits.Max();
						int maxProfitIndex = exchageProfits.SelectIndexesWhere(profit => profit == maxProfit).First();
						var pairToExchange = pairsToExchange[maxProfitIndex];
						var newPaths = modifiedPickerPaths[maxProfitIndex];

						pathsOfPicker[pickerNbr].Remove(pairToExchange.leftPath);
						pathsOfPicker[pickerNbr].Add(pairToExchange.rightPath);
						fullPathOfPicker[pickerNbr] = newPaths.LeftPickerNewFullPath;

						pathsOfPicker[pickerToExchange].Remove(pairToExchange.rightPath);
						pathsOfPicker[pickerToExchange].Add(pairToExchange.leftPath);
						fullPathOfPicker[pickerToExchange] = newPaths.RightPickerNewFullPath;

						exchangedOccurred |= true;
					}
				}

				if (exchangedOccurred == false)
					break;
			}
		}
		#endregion

		#region Batch Pick Lists
		protected virtual void CreateBatchPickList(HeaderSettings settings, IEnumerable<SOShipment> shipments)
		{
			if (shipments.Any() == false)
				return;

			if (HasAnyShipmentContainsNonStockShipLinesWithEmptyLocation(shipments))
				throw new PXInvalidOperationException(Msg.OneOrMultipleShipmentContainsNonStockItemWithEmptyLocationSummary);

			if (shipments.GroupBy(s => s.SiteID).Count() > 1)
				throw new PXInvalidOperationException(Msg.SingleSiteIDForAllShipmentsRequired);

			IEnumerable<SOPickingWorksheetLineSplit> worksheetLineSplits = MergeSplits(shipments);

			if (worksheetLineSplits.Any())
			{
				var worksheetGraph = CreateInstance<SOPickingWorksheetReview>();

				(SOPickingWorksheet worksheet, var lines, var splits) = CreateWorksheet(worksheetGraph, SOPickingWorksheet.worksheetType.Batch, shipments.First().SiteID, worksheetLineSplits);
				LinkShipments(worksheetGraph, shipments, worksheet);
				SpreadSplitsAmongPickers(worksheetGraph, splits.Join(lines, s => s.LineNbr, l => l.LineNbr, (s, l) => (s, l.UOM)).ToArray(), settings.NumberOfPickers ?? 0);

				worksheetGraph.Save.Press();

				if (settings.PrintPickLists == true)
					worksheetGraph.PrintPickList.Press(CreateAdapter(worksheetGraph, settings, new[] { worksheet })).Consume();
			}
		}

		protected virtual void SpreadSplitsAmongPickers(SOPickingWorksheetReview worksheetGraph, IEnumerable<(SOPickingWorksheetLineSplit Split, string UOM)> splits, int maxPickersCount)
		{
			if (maxPickersCount == 1)
			{
				GiveAllSplitsToSinglePicker(worksheetGraph, splits);
				return;
			}

			INLocation[] locations = GetSortedLocations(splits.Select(s => s.Split.LocationID));
			if (locations.Length == 1)
			{
				SpreadSplitsByItems(worksheetGraph, splits, maxPickersCount);
				return;
			}

			int actualPickersCount = Math.Min(maxPickersCount, locations.Length);
			var sortedSplits = splits.OrderBy(s => s.Split.LocationID, locations.Select(loc => loc.LocationID).ToArray()).ToArray();
			INLocation[] separatingLocations = null;

			var distances = GetSortedDistancesBetween(locations).ToArray();

			decimal distanceMean = distances.Average(d => (decimal)d.distance);
			decimal distanceMedian = distances.OrderBy(d => d.distance).Skip((distances.Length % 2 == 0 ? distances.Length : distances.Length - 1) / 2).First().distance;

			var obviousGaps = distances.Where(d => d.distance > 4 * distanceMedian);
			bool hasObviousGaps = (distanceMean > 2 * distanceMedian) && obviousGaps.Any();
			if (hasObviousGaps && obviousGaps.Count() > actualPickersCount - 2)
			{
				separatingLocations = distances
					.Take(actualPickersCount - 1)
					.OrderBy(d => d.source.PathPriority)
					.ThenBy(d => d.source.LocationCD)
					.Select(d => d.target)
					.ToArray();
			}
			else
			{
				decimal fullPathLength = distances.Sum(d => d.distance);
				decimal weightedPathLength = 1.333m * fullPathLength / actualPickersCount;

				var separatingLocationList = new List<INLocation>(actualPickersCount - 1);
				var pathSegments = distances
					.OrderBy(l => l.source.PathPriority)
					.ThenBy(l => l.source.LocationCD)
					.ToArray();
				int currentPathSeg = 0;
				decimal currentPathLength = 0;
				while (true)
				{
					if (separatingLocationList.Count == actualPickersCount - 1)
						break;

					do
					{
						currentPathLength += pathSegments[currentPathSeg].distance;
						currentPathSeg++;
					}
					while (currentPathSeg < pathSegments.Length && currentPathLength + pathSegments[currentPathSeg].distance < weightedPathLength);

					if (currentPathSeg < pathSegments.Length)
					{
						separatingLocationList.Add(pathSegments[currentPathSeg].target);
						currentPathLength = 0;
					}
					else
						break;
				}
				separatingLocations = separatingLocationList.ToArray();
			}

			int currentPicker = 0;
			var pathOfPicker = new WMSPath[separatingLocations.Length + 1];
			var locationsOfCurrentPicker = new List<INLocation>();
			foreach (var location in locations)
			{
				if (currentPicker == separatingLocations.Length || location.LocationID != separatingLocations[currentPicker].LocationID)
				{
					locationsOfCurrentPicker.Add(location);
				}
				else
				{
					pathOfPicker[currentPicker] = new WMSPath($"picker{currentPicker}", locationsOfCurrentPicker);
					locationsOfCurrentPicker.Clear();
					locationsOfCurrentPicker.Add(location);
					currentPicker++;
				}
			}
			pathOfPicker[currentPicker] = new WMSPath($"picker{currentPicker}", locationsOfCurrentPicker);

			int[] separators =
				separatingLocations
				  .Select(loc => sortedSplits.SelectIndexesWhere(s => s.Split.LocationID == loc.LocationID).First())
				  .ToArray();

			currentPicker = 0;
			for (int i = 0; i < sortedSplits.Length; i++)
			{
				if (i == 0 || i.IsIn(separators))
				{
					var path = pathOfPicker[currentPicker++];
					worksheetGraph.pickers.Insert(new SOPicker
					{
						PathLength = path.PathLength,
						FirstLocationID = path.First().LocationID,
						LastLocationID = path.Last().LocationID
					});

					if (PXAccess.FeatureInstalled<FeaturesSet.wMSPaperlessPicking>())
						worksheetGraph.pickerJob.Insert(new SOPickingJob
						{
							Status = ProcessSettings.SendToQueue == true
								? SOPickingJob.status.Enqueued
								: default,
							AutomaticShipmentConfirmation = ProcessSettings.AutomaticShipmentConfirmation
						});
				}

				worksheetGraph.pickerList.Insert(PickerListEntryFrom(sortedSplits[i].Split, sortedSplits[i].UOM));
			}
		}

		protected virtual void GiveAllSplitsToSinglePicker(SOPickingWorksheetReview worksheetGraph, IEnumerable<(SOPickingWorksheetLineSplit Split, string UOM)> splits)
		{
			INLocation[] locations = GetSortedLocations(splits.Select(s => s.Split.LocationID));
			var path = new WMSPath("fullPath", locations);

			var sortedSplits = splits.OrderBy(s => s.Split.LocationID, locations.Select(loc => loc.LocationID).ToArray()).ToArray();

			for (int i = 0; i < sortedSplits.Length; i++)
			{
				if (i == 0)
				{
					worksheetGraph.pickers.Insert(new SOPicker
					{
						PathLength = path.PathLength,
						FirstLocationID = path.First().LocationID,
						LastLocationID = path.Last().LocationID
					});

					if (PXAccess.FeatureInstalled<FeaturesSet.wMSPaperlessPicking>())
						worksheetGraph.pickerJob.Insert(new SOPickingJob
						{
							Status = ProcessSettings.SendToQueue == true
								? SOPickingJob.status.Enqueued
								: default,
							AutomaticShipmentConfirmation = ProcessSettings.AutomaticShipmentConfirmation
						});
				}

				worksheetGraph.pickerList.Insert(PickerListEntryFrom(sortedSplits[i].Split, sortedSplits[i].UOM));
			}
		}

		protected virtual void SpreadSplitsByItems(SOPickingWorksheetReview worksheetGraph, IEnumerable<(SOPickingWorksheetLineSplit Split, string UOM)> splits, int maxPickersCount)
		{
			var items = splits.Select(s => s.Split.InventoryID).Distinct().ToArray();
			var sortedSplits = splits.OrderBy(s => s.Split.InventoryID).ToArray();

			int actualPickersCount = Math.Min(maxPickersCount, items.Length);

			int itemPerPicker = (int)Math.Ceiling(Decimal.Divide(items.Length, actualPickersCount));
			var separatingItems = items.Batch(itemPerPicker, batch => batch.First()).Skip(1).ToArray();
			int[] separators =
				separatingItems
				  .Select(item => sortedSplits.SelectIndexesWhere(s => s.Split.InventoryID == item).First())
				  .ToArray();

			for (int i = 0; i < sortedSplits.Length; i++)
			{
				if (i == 0 || i.IsIn(separators))
				{
					worksheetGraph.pickers.Insert(new SOPicker
					{
						PathLength = 0,
						FirstLocationID = sortedSplits[i].Split.LocationID,
						LastLocationID = sortedSplits[i].Split.LocationID
					});

					if (PXAccess.FeatureInstalled<FeaturesSet.wMSPaperlessPicking>())
						worksheetGraph.pickerJob.Insert(new SOPickingJob
						{
							Status = ProcessSettings.SendToQueue == true
								? SOPickingJob.status.Enqueued
								: default,
							AutomaticShipmentConfirmation = ProcessSettings.AutomaticShipmentConfirmation
						});
				}

				worksheetGraph.pickerList.Insert(PickerListEntryFrom(sortedSplits[i].Split, sortedSplits[i].UOM));
			}
		}
		#endregion

		#region Single Pick Lists
		protected virtual void CreateSinglePickList(HeaderSettings settings, IEnumerable<SOShipment> shipments)
		{
			if (shipments.Any() == false)
				return;

			if (shipments.GroupBy(s => s.SiteID).Count() > 1)
				throw new PXInvalidOperationException(Msg.SingleSiteIDForAllShipmentsRequired);

			var worksheetGraph = CreateInstance<SOPickingWorksheetReview>();
			var createdWorksheets = new List<SOPickingWorksheet>();
			int currentIndex = 0;

			foreach (var shipment in shipments)
			{
				currentIndex++;

				if (HasNonStockLinesWithEmptyLocation(shipment.ShipmentNbr))
				{
					PXProcessing<SOShipment>.SetError(currentIndex - 1, PXMessages.LocalizeFormat(Msg.OneOrMultipleShipmentContainsNonStockItemWithEmptyLocationInfo, shipment.ShipmentNbr));
					continue;
				}

				var singleShipment = SOShipment.PK.Find(worksheetGraph, shipment).AsSingleEnumerable();

				IEnumerable<SOPickingWorksheetLineSplit> worksheetLineSplits = MergeSplits(singleShipment);
				if (worksheetLineSplits.Any())
				{
					(SOPickingWorksheet worksheet, var lines, var splits) = CreateWorksheet(worksheetGraph, SOPickingWorksheet.worksheetType.Single, shipment.SiteID, worksheetLineSplits);
					worksheetGraph.worksheet.SetValueExt<SOPickingWorksheet.singleShipmentNbr>(worksheet, shipment.ShipmentNbr);
					LinkShipments(worksheetGraph, singleShipment, worksheet);
					SpreadShipmentsAmongPickers(worksheetGraph, singleShipment, 1, 1);

					worksheetGraph.Save.Press();
					createdWorksheets.Add(worksheetGraph.worksheet.Current);

					worksheetGraph.Clear();
					worksheetGraph.SelectTimeStamp();
				}
			}

			if (settings.PrintPickLists == true)
				worksheetGraph.PrintPickList.Press(CreateAdapter(worksheetGraph, settings, createdWorksheets)).Consume();
		}
		#endregion

		private bool HasAnyShipmentContainsNonStockShipLinesWithEmptyLocation(IEnumerable<SOShipment> shipments)
		{
			bool hasAnyNonStockShipLinesWithEmptyLocation = false;

			foreach (var shipment in shipments)
			{
				if (HasNonStockLinesWithEmptyLocation(shipment.ShipmentNbr))
				{
					PXTrace.WriteError(Msg.OneOrMultipleShipmentContainsNonStockItemWithEmptyLocationInfo, shipment.ShipmentNbr);
					hasAnyNonStockShipLinesWithEmptyLocation = true;
				}
			}

			return hasAnyNonStockShipLinesWithEmptyLocation;
		}

		public virtual bool HasNonStockLinesWithEmptyLocation(string shipmentNbr)
		{
			SOShipLine shipLine =
				SelectFrom<SOShipLine>.
				InnerJoin<InventoryItem>.On<SOShipLine.FK.InventoryItem>.
				Where<
					InventoryItem.stkItem.IsEqual<False>.
					And<InventoryItem.kitItem.IsEqual<False>>.
					And<SOShipLine.locationID.IsNull>.
					And<SOShipLine.shipmentNbr.IsEqual<@P.AsString>>>.
				View.ReadOnly.Select(this, shipmentNbr);

			return shipLine != null;
		}

		protected virtual IEnumerable<SOPickingWorksheetLineSplit> MergeSplits(IEnumerable<SOShipment> shipments)
		{
			var worksheetLineSplits = new List<SOPickingWorksheetLineSplit>();

			foreach (var shipment in shipments)
			{
				var splits = GetShipmentSplits(shipment);
				foreach ((var split, var line) in splits)
				{
					var existingWSSplit = split.IsUnassigned == true
						? null
						: worksheetLineSplits.FirstOrDefault(s =>
							s.InventoryID == split.InventoryID &&
							s.SubItemID == split.SubItemID &&
							s.SiteID == split.SiteID &&
							s.LocationID == split.LocationID &&
							s.UOM == line.UOM &&
							string.Equals(s.LotSerialNbr, split.LotSerialNbr, StringComparison.OrdinalIgnoreCase));

					var actualWSSplit = existingWSSplit
						?? new SOPickingWorksheetLineSplit
						{
							InventoryID = split.InventoryID,
							SubItemID = split.SubItemID,
							SiteID = split.SiteID,
							LocationID = split.LocationID,
							UOM = line.UOM,
							LotSerialNbr = split.LotSerialNbr,
							ExpireDate = split.ExpireDate,
							Qty = 0,
							IsUnassigned = split.IsUnassigned,
							HasGeneratedLotSerialNbr = split.HasGeneratedLotSerialNbr
						};

					actualWSSplit.Qty += split.Qty;
					if (existingWSSplit == null)
						worksheetLineSplits.Add(actualWSSplit);
				}
			}

			return worksheetLineSplits;
		}

		protected virtual
			(SOPickingWorksheet worksheet, IEnumerable<SOPickingWorksheetLine> lines, IEnumerable<SOPickingWorksheetLineSplit> splits)
			CreateWorksheet
			(SOPickingWorksheetReview worksheetGraph, string worksheetType, int? siteID, IEnumerable<SOPickingWorksheetLineSplit> worksheetLineSplits)
		{
			var worksheet = worksheetGraph.worksheet.Insert(new SOPickingWorksheet { WorksheetType = worksheetType, SiteID = siteID });
			var wsLines = new List<SOPickingWorksheetLine>();
			var wsSplits = new List<SOPickingWorksheetLineSplit>();

			var lines = worksheetLineSplits.GroupBy(s => new { s.SiteID, s.InventoryID, s.SubItemID, s.UOM }).ToArray();
			foreach (var line in lines)
			{
				InventoryItem item = InventoryItem.PK.Find(this, line.Key.InventoryID);
				var lotSerClass = item.With(ii => InventoryItem.FK.LotSerialClass.FindParent(this, ii));

				var singleLocation = line.Select(s => s.LocationID).Distinct().Count() == 1;
				var singleLotSerial = line.Select(s => s.LotSerialNbr).Distinct(StringComparer.OrdinalIgnoreCase).Count() == 1;

				var wsLine = worksheetGraph.worksheetLines.Insert(
					new SOPickingWorksheetLine
					{
						InventoryID = line.Key.InventoryID,
						SubItemID = line.Key.SubItemID,
						SiteID = line.Key.SiteID,
						UOM = line.Key.UOM,
						LocationID = singleLocation ? line.First().LocationID : null,
						LotSerialNbr = lotSerClass == null || lotSerClass.LotSerTrack == INLotSerTrack.NotNumbered ? "" : singleLotSerial ? line.First().LotSerialNbr : null,
						ExpireDate = singleLotSerial ? line.First().ExpireDate : null,
						Qty = INUnitAttribute.ConvertFromBase(worksheetGraph.worksheetLines.Cache, line.Key.InventoryID, line.Key.UOM, line.Sum(s => s.Qty ?? 0), INPrecision.NOROUND)
					});
				wsLines.Add(wsLine);

				foreach (var split in line)
				{
					split.UOM = item.BaseUnit;
					var inserted = worksheetGraph.worksheetLineSplits.Insert(split);
					wsSplits.Add(inserted);
				}
			}

			return (worksheet, wsLines, wsSplits);
		}

		protected virtual void LinkShipments(SOPickingWorksheetReview worksheetGraph, IEnumerable<SOShipment> shipments, SOPickingWorksheet worksheet)
		{
			foreach (var shipment in shipments)
			{
				worksheet.Qty += shipment.ShipmentQty ?? 0;
				worksheet.ShipmentWeight += shipment.ShipmentWeight ?? 0;
				worksheet.ShipmentVolume += shipment.ShipmentVolume ?? 0;

				shipment.CurrentWorksheetNbr = worksheetGraph.worksheet.Current.WorksheetNbr;
				worksheetGraph.shipments.Update(shipment);

				worksheetGraph.shipmentLinks.Insert(new SOPickingWorksheetShipment { ShipmentNbr = shipment.ShipmentNbr });
			}
			worksheetGraph.worksheet.Update(worksheet);
		}

		protected virtual PXResult<SOShipLineSplit, SOShipLine>[] GetShipmentSplits(SOShipment shipment)
		{
			var assigned =
				SelectFrom<SOShipLineSplit>.
				InnerJoin<SOShipLine>.On<SOShipLineSplit.FK.ShipmentLine>.
				Where<SOShipLine.FK.Shipment.SameAsCurrent>.
				View.SelectMultiBound(this, new[] { shipment })
				.AsEnumerable()
				.Cast<PXResult<SOShipLineSplit, SOShipLine>>();

			var unassigned =
				SelectFrom<Unassigned.SOShipLineSplit>.
				InnerJoin<SOShipLine>.On<Unassigned.SOShipLineSplit.FK.ShipmentLine>.
				Where<SOShipLine.FK.Shipment.SameAsCurrent>.
				View.SelectMultiBound(this, new[] { shipment })
				.AsEnumerable()
				.Cast<PXResult<Unassigned.SOShipLineSplit, SOShipLine>>()
				.Select(r => new PXResult<SOShipLineSplit, SOShipLine>(MakeAssigned(r), r));

			return assigned.Concat(unassigned).ToArray();
		}

		protected virtual INLocation[] GetSortedLocations(IEnumerable<int?> locationIDs)
		{
			var locations = locationIDs
				.Distinct()
				.Select(id => INLocation.PK.Find(this, id))
				.OrderBy(l => l.PathPriority)
				.ThenBy(l => l.LocationCD)
				.ToArray();

			// we don't want to affect shared records - those who are stored in global cache
			var locationsCopies = locations.Select(PXCache<INLocation>.CreateCopy).ToArray();
			int lastPathPriority = 0;
			foreach (var location in locationsCopies)
			{
				if (location.PathPriority.HasValue)
					lastPathPriority = location.PathPriority.Value;
				else
					location.PathPriority = ++lastPathPriority;
			}

			return locationsCopies;
		}

		protected virtual SOPickerListEntry PickerListEntryFrom(SOPickingWorksheetLineSplit split, string orderLineUom)
		{
			return new SOPickerListEntry
			{
				SiteID = split.SiteID,
				LocationID = split.LocationID,
				InventoryID = split.InventoryID,
				SubItemID = split.SubItemID,
				LotSerialNbr = split.LotSerialNbr,
				ExpireDate = split.ExpireDate,
				UOM = split.UOM,
				Qty = split.Qty,
				IsUnassigned = split.IsUnassigned,
				HasGeneratedLotSerialNbr = split.HasGeneratedLotSerialNbr,
				OrderLineUOM = orderLineUom
			};
		}

		protected virtual SOPickerListEntry PickerListEntryFrom(SOShipLineSplit split, string orderLineUom, string shipmentNbr)
		{
			return new SOPickerListEntry
			{
				ShipmentNbr = shipmentNbr,
				SiteID = split.SiteID,
				LocationID = split.LocationID,
				InventoryID = split.InventoryID,
				SubItemID = split.SubItemID,
				LotSerialNbr = split.LotSerialNbr,
				ExpireDate = split.ExpireDate,
				UOM = split.UOM,
				Qty = split.Qty,
				IsUnassigned = split.IsUnassigned,
				HasGeneratedLotSerialNbr = split.HasGeneratedLotSerialNbr,
				OrderLineUOM = orderLineUom
			};
		}

		protected IEnumerable<(INLocation source, INLocation target, int distance)> GetSortedDistancesBetween(IEnumerable<INLocation> locations)
		{
			var distances = new List<(INLocation source, INLocation target, int distance)>();
			using (var e = locations.GetEnumerator())
			{
				if (e.MoveNext())
				{
					INLocation prevLoc = prevLoc = e.Current;
					while (e.MoveNext())
					{
						distances.Add((prevLoc, e.Current, 1 + e.Current.PathPriority.Value - prevLoc.PathPriority.Value));
						prevLoc = e.Current;
					}
				}
			}
			return distances.OrderByDescending(d => d.distance).ThenBy(d => d.source.LocationCD).ToArray();
		}

		private SOShipLineSplit MakeAssigned(Unassigned.SOShipLineSplit unassignedSplit) => PropertyTransfer.Transfer(unassignedSplit, new SOShipLineSplit());

		#region Well-known Extensions
		// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
		public class InventoryFilterExt : IN.GraphExtensions.InventoryLinkFilterExtensionBase<SOPickingWorksheetProcess, HeaderFilter, HeaderFilter.inventoryID>
		{
			// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
			public class descr : AttachedInventoryDescription<descr> { }

			[PXMergeAttributes(Method = MergeMethod.Replace)]
			[Inventory(IsKey = true)]
			protected override void _(Events.CacheAttached<IN.GraphExtensions.InventoryLinkFilter.inventoryID> e) { }

			/// Overrides <see cref="SOPickingWorksheetProcess.AppendFilter(BqlCommand, IList{object}, HeaderFilter)"/>
			[PXOverride]
			public BqlCommand AppendFilter(BqlCommand cmd, IList<object> parameters, HeaderFilter filter,
				Func<BqlCommand, IList<object>, HeaderFilter, BqlCommand> base_AppendFilter)
			{
				cmd = base_AppendFilter(cmd, parameters, filter);

				var inventories = GetSelectedEntities(filter).ToArray();
				if (inventories.Length > 0)
				{
					cmd = cmd.WhereAnd<Where<Not<Exists<
							SelectFrom<SOShipLine>.
							Where<
								SOShipLine.shipmentNbr.IsEqual<SOShipment.shipmentNbr>.
								And<SOShipLine.inventoryID.IsNotIn<@P.AsInt>>>
						>>>>();
					parameters.Add(inventories);
				}

				return cmd;
			}
		}

		// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
		public class LocationFilterExt : IN.GraphExtensions.LocationLinkFilterExtensionBase<SOPickingWorksheetProcess, HeaderFilter, HeaderFilter.locationID>
		{
			// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
			public class descr : AttachedLocationDescription<descr> { }

			[PXMergeAttributes(Method = MergeMethod.Replace)]
			[Location(typeof(HeaderFilter.siteID), IsKey = true)]
			protected override void _(Events.CacheAttached<IN.GraphExtensions.LocationLinkFilter.locationID> e) { }

			/// Overrides <see cref="SOPickingWorksheetProcess.AppendFilter(BqlCommand, IList{object}, HeaderFilter)"/>
			[PXOverride]
			public BqlCommand AppendFilter(BqlCommand cmd, IList<object> parameters, HeaderFilter filter,
				Func<BqlCommand, IList<object>, HeaderFilter, BqlCommand> base_AppendFilter)
			{
				cmd = base_AppendFilter(cmd, parameters, filter);

				var locations = GetSelectedEntities(filter).ToArray();
				if (locations.Length > 0)
				{
					cmd = cmd.WhereAnd<Where<Not<Exists<
						SelectFrom<SOShipLineSplit>.
						Where<
							SOShipLineSplit.shipmentNbr.IsEqual<SOShipment.shipmentNbr>.
							And<SOShipLineSplit.locationID.IsNotIn<@P.AsInt>>>
					>>>>();
					parameters.Add(locations);
				}

				return cmd;
			}
		}
		#endregion

		[PXLocalizable]
		public abstract class Msg
		{
			public const string SingleSiteIDForAllShipmentsRequired = "All selected shipments should have same Site ID.";
			public const string OverallTotesShouldBeNotLessThanShipments = "Overall number of totes should be at least as many as number of shipments.";
			public const string ShipmentContainsNonStockItemWithEmptyLocation = "The {0} shipment cannot be added to the pick list because it contains non-stock item with empty location. This shipment should be processed manually via the Shipments (SO302000) form.";
			public const string OneOrMultipleShipmentContainsNonStockItemWithEmptyLocationSummary = "The picking worksheet cannot be created because one or multiple shipments contain non-stock items with empty location. For more details see Trace.";
			public const string OneOrMultipleShipmentContainsNonStockItemWithEmptyLocationInfo = "The picking worksheet cannot be created because one or multiple shipments contain non-stock items with empty location: {0}.";
			public const string ShipmentIsAlreadyProcessedOnPickPackShip = "The pick list for the {0} shipment cannot be created because this shipment is already being processed on the Pick, Pack, and Ship (SO302020) form.";
		}

		[PXLocalizable]
		public abstract class CacheNames
		{
			public const string Filter = "Filter";
		}
	}

	public class WMSPath : IEnumerable<INLocation>, IEquatable<WMSPath>
	{
		public string Name { get; }
		public int PathLength { get; }
		private readonly INLocation[] _locations;
		public bool IsEmpty { get; }
		public bool IsDot { get; }
		public bool IsFake { get; private set; }

		public override string ToString() => $"{Name}:{(IsFake ? "IsFake" : PathLength.ToString())}";

		public static WMSPath MakeFake(string name) => new WMSPath(name, Enumerable.Empty<INLocation>()) { IsFake = true };

		public WMSPath(string name, IEnumerable<INLocation> locations)
		{
			Name = name;
			_locations = locations.ToArray();
			IsEmpty = _locations.Length == 0;
			IsDot = _locations.Length == 1;
			PathLength = GetFullPathLength(_locations);
		}

		public WMSPath MergeWith(WMSPath another, string newName)
		{
			if (this.IsEmpty && another.IsEmpty)
				return new WMSPath(newName, Enumerable.Empty<INLocation>());
			if (this.IsEmpty)
				return new WMSPath(newName, another);
			if (another.IsEmpty)
				return new WMSPath(newName, this);
			return
				new[] { this, another }
				.With(ps => SortPaths(ps))
				.Select(p => p.AsEnumerable())
				.Aggregate((pA, pB) => pA.Concat(pB))
				.Distinct(loc => loc.LocationID)
				.ToArray()
				.With(a => new WMSPath(newName, a));
		}

		public WMSPath GetIntersectionWith(WMSPath another, string newName) => GetIntersectionWith(another.AsEnumerable(), newName);
		public WMSPath GetIntersectionWith(IEnumerable<INLocation> locations, string newName) => GetIntersectionWith(locations.Select(loc => loc.LocationID), newName);
		public WMSPath GetIntersectionWith(IEnumerable<int?> locationIDs, string newName)
		{
			int currentIndex = 0;
			int firstLocationIndex = -1;
			int lastLocationIndex = 0;
			var ids = locationIDs.ToHashSet();

			foreach (var loc in _locations)
			{
				if (firstLocationIndex == -1 && ids.Contains(loc.LocationID))
					firstLocationIndex = currentIndex;
				if (ids.Contains(loc.LocationID))
					lastLocationIndex = currentIndex - firstLocationIndex;

				currentIndex++;
			}

			return firstLocationIndex == -1
				? new WMSPath(newName, Enumerable.Empty<INLocation>())
				: new WMSPath(newName, _locations.Skip(firstLocationIndex).Take(lastLocationIndex + 1).ToArray());
		}

		public bool Contains(WMSPath another) => this.GetIntersectionWith(another, "temp").PathLength == another.PathLength;
		public bool StartsWith(WMSPath another) => this.First().LocationID == another.First().LocationID && this.Contains(another);
		public bool EndsWith(WMSPath another) => this.Last().LocationID == another.Last().LocationID && this.Contains(another);

		public static IEnumerable<WMSPath> SortPaths(IEnumerable<WMSPath> paths, bool includePathLength = false)
		{
			return paths
				.OrderByDescending(p => p.Any())
				.OrderBy(p => p.First().PathPriority.Value)
				.ThenBy(p => p.First().LocationCD)
				.ThenBy(p => includePathLength ? p.PathLength : 0);
		}
		public static WMSPath MergePaths(IEnumerable<WMSPath> paths, string newName)
			=> paths.ToArray().With(a => a.Length == 1
				? new WMSPath(newName, a[0])
				: a.Aggregate((path, nextPath) => path.MergeWith(nextPath, newName)));

		private int GetFullPathLength(INLocation[] locations) =>
			locations.Length == 0
				? 0
				: locations[locations.Length - 1].PathPriority.Value - locations[0].PathPriority.Value;

		public IEnumerator<INLocation> GetEnumerator() => _locations.AsEnumerable().GetEnumerator();
		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

		public override bool Equals(object obj) => Equals(obj as WMSPath);
		public bool Equals(WMSPath other) => other != null && Name == other.Name;
		public override int GetHashCode() => 539060726 + EqualityComparer<string>.Default.GetHashCode(Name);
	}
}
