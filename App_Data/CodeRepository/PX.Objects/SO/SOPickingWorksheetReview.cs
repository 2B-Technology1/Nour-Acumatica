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

using PX.Api;
using PX.Common;
using PX.Concurrency;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Data.WorkflowAPI;
using PX.Objects.Common.Extensions;
using PX.Objects.Common;
using PX.Objects.IN;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PX.Objects.SO
{
	using static BoundedTo<SOPickingWorksheetReview, SOPickingWorksheet>;

	public class SOPickingWorksheetReview : PXGraph<SOPickingWorksheetReview>
	{
		#region Views
		public PXSetup<SOSetup> setup;
		public PXSetup<SOPickPackShipSetup>.Where<SOPickPackShipSetup.branchID.IsEqual<AccessInfo.branchID.FromCurrent>> pickSetup;
		public
			SelectFrom<SOPickingWorksheet>.
			Where<SOPickingWorksheet.worksheetType.IsNotEqual<SOPickingWorksheet.worksheetType.single>>.
			View worksheet;
		public
			SelectFrom<SOPickingWorksheetLine>.
			LeftJoin<INLocation>.On<SOPickingWorksheetLine.FK.Location>.
			Where<SOPickingWorksheetLine.FK.Worksheet.SameAsCurrent>.
			OrderBy<INLocation.pathPriority.Asc, INLocation.locationCD.Asc>.
			View worksheetLines;
		public
			SelectFrom<SOPickingWorksheetLineSplit>.
			LeftJoin<INLocation>.On<SOPickingWorksheetLineSplit.FK.Location>.
			Where<SOPickingWorksheetLineSplit.FK.WorksheetLine.SameAsCurrent.And<SOPickingWorksheetLineSplit.isUnassigned.IsEqual<False>>>.
			OrderBy<INLocation.pathPriority.Asc, INLocation.locationCD.Asc>.
			View worksheetLineSplits;

		public SelectFrom<SOPickingWorksheetShipment>.Where<SOPickingWorksheetShipment.FK.Worksheet.SameAsCurrent>.View shipmentLinks;
		public SelectFrom<SOShipment>.Where<SOShipment.FK.Worksheet.SameAsCurrent>.View shipments;
		public SelectFrom<SOPicker>.View shipmentPickers;
		public virtual IEnumerable ShipmentPickers()
		{
			if (worksheet.Current?.WorksheetType == SOPickingWorksheet.worksheetType.Batch)
				return shipmentPickersForBatch.Select().AsEnumerable().RowCast<SOPicker>().ToArray();
			else if (worksheet.Current?.WorksheetType == SOPickingWorksheet.worksheetType.Wave)
				return shipmentPickersForWave.Select().AsEnumerable().RowCast<SOPicker>().ToArray();
			return Array.Empty<SOPicker>();
		}

		public
			SelectFrom<SOPicker>.
			InnerJoin<SOPickerListEntry>.On<SOPickerListEntry.FK.Picker>.
			Where<SOPickerListEntry.shipmentNbr.IsEqual<SOPickingWorksheetShipment.shipmentNbr.FromCurrent>.
				And<SOPicker.FK.Worksheet.SameAsCurrent>>.
			AggregateTo<
				GroupBy<SOPicker.worksheetNbr>,
				GroupBy<SOPicker.pickerNbr>>.
			View shipmentPickersForWave;

		public
			SelectFrom<SOPicker>.
			InnerJoin<SOPickerListEntry>.On<SOPickerListEntry.FK.Picker>.
			InnerJoin<SOShipLineSplit>.On<
				SOShipLineSplit.inventoryID.IsEqual<SOPickerListEntry.inventoryID>.
				And<SOShipLineSplit.subItemID.IsEqual<SOPickerListEntry.subItemID>>.
				And<SOShipLineSplit.siteID.IsEqual<SOPickerListEntry.siteID>>.
				And<SOShipLineSplit.locationID.IsEqual<SOPickerListEntry.locationID>>.
				And<AreSame<SOShipLineSplit.lotSerialNbr, SOPickerListEntry.lotSerialNbr>>>.
			Where<SOPicker.FK.Worksheet.SameAsCurrent.
				And<SOShipLineSplit.shipmentNbr.IsEqual<SOPickingWorksheetShipment.shipmentNbr.FromCurrent>>>.
			AggregateTo<
				GroupBy<SOPicker.worksheetNbr>,
				GroupBy<SOPicker.pickerNbr>>.
			View shipmentPickersForBatch;

		public SelectFrom<SOPicker>.Where<SOPicker.FK.Worksheet.SameAsCurrent>.View pickers;
		public
			SelectFrom<SOPickerListEntry>.
			InnerJoin<INLocation>.On<SOPickerListEntry.FK.Location>.
			InnerJoin<InventoryItem>.On<SOPickerListEntry.FK.InventoryItem>.
			Where<SOPickerListEntry.FK.Picker.SameAsCurrent>.
			AggregateTo<
				GroupBy<SOPickerListEntry.siteID>,
				GroupBy<SOPickerListEntry.locationID>,
				GroupBy<SOPickerListEntry.inventoryID>,
				GroupBy<SOPickerListEntry.subItemID>,
				GroupBy<SOPickerListEntry.lotSerialNbr>,
				Sum<SOPickerListEntry.qty>,
				Sum<SOPickerListEntry.baseQty>,
				Sum<SOPickerListEntry.pickedQty>,
				Sum<SOPickerListEntry.basePickedQty>>.
			OrderBy<
				INLocation.pathPriority.Asc,
				INLocation.locationCD.Asc,
				InventoryItem.inventoryCD.Asc,
				SOPickerListEntry.lotSerialNbr.Asc>.
			View pickerList;
		public
			SelectFrom<SOPickerToShipmentLink>.
			Where<SOPickerToShipmentLink.FK.Picker.SameAsCurrent>.
			AggregateTo<
				GroupBy<SOPickerToShipmentLink.worksheetNbr>,
				GroupBy<SOPickerToShipmentLink.pickerNbr>,
				GroupBy<SOPickerToShipmentLink.siteID>,
				GroupBy<SOPickerToShipmentLink.shipmentNbr>>.
			View pickerShipments;
		public
			SelectFrom<SOPickerListEntry>.
			InnerJoin<INLocation>.On<SOPickerListEntry.FK.Location>.
			InnerJoin<InventoryItem>.On<SOPickerListEntry.FK.InventoryItem>.
			Where<
				SOPickerListEntry.FK.Picker.SameAsCurrent.
				And<SOPickerListEntry.shipmentNbr.IsEqual<SOPickerToShipmentLink.shipmentNbr.FromCurrent>>>.
			OrderBy<
				INLocation.pathPriority.Asc,
				INLocation.locationCD.Asc,
				InventoryItem.inventoryCD.Asc,
				SOPickerListEntry.lotSerialNbr.Asc>.
			View pickerListByShipment;
		public
			SelectFrom<SOPickingJob>.
			Where<SOPickingJob.FK.Picker.SameAsCurrent>.
			View pickerJob;
		#endregion

		#region Actions
		public PXCancel<SOPickingWorksheet> Cancel;
		public PXSave<SOPickingWorksheet> Save;
		public PXDelete<SOPickingWorksheet> Delete;
		public PXFirst<SOPickingWorksheet> First;
		public PXPrevious<SOPickingWorksheet> Prev;
		public PXNext<SOPickingWorksheet> Next;
		public PXLast<SOPickingWorksheet> Last;

		public PXInitializeState<SOPickingWorksheet> InitializeState;

		[PXButton(CommitChanges = true), PXUIField(DisplayName = "Hold")]
		protected virtual IEnumerable putOnHold(PXAdapter adapter) => adapter.Get();
		public PXAction<SOPickingWorksheet> PutOnHold;

		[PXButton(CommitChanges = true), PXUIField(DisplayName = "Remove Hold")]
		protected virtual IEnumerable releaseFromHold(PXAdapter adapter) => adapter.Get();
		public PXAction<SOPickingWorksheet> ReleaseFromHold;

		[PXButton, PXUIField(DisplayName = "Line Details")]
		public virtual void showSplits() => worksheetLineSplits.AskExt();
		public PXAction<SOPickingWorksheet> ShowSplits;

		[PXButton, PXUIField(DisplayName = "View Pickers")]
		public virtual void showPickers() => shipmentPickers.AskExt();
		public PXAction<SOPickingWorksheet> ShowPickers;

		[PXButton, PXUIField(DisplayName = "View Shipments")]
		public virtual void showShipments() => pickerShipments.AskExt();
		public PXAction<SOPickingWorksheet> ShowShipments;

		[PXButton, PXUIField(DisplayName = "View Pick List")]
		public virtual void showPickList() => pickerList.AskExt();
		public PXAction<SOPickingWorksheet> ShowPickList;

		[PXButton(CommitChanges = true), PXUIField(DisplayName = "Unlink All Shipments")]
		public virtual IEnumerable unlinkAllShipments(PXAdapter a)
		{
			var worksheets = a.Get<SOPickingWorksheet>().ToArray();
			PXLongOperation.StartOperation(this, () => UnlinkAllShipmentsImpl(worksheets));
			return worksheets;
		}
		public PXAction<SOPickingWorksheet> UnlinkAllShipments;

		[PXButton(CommitChanges = true), PXUIField(DisplayName = "Cancel Worksheet")]
		public virtual IEnumerable cancelWorksheet(PXAdapter a)
		{
			if (worksheet.Ask(ErrorMessages.Warning, Msg.CancellationConfirmation, MessageButtons.OKCancel, MessageIcon.Warning) == WebDialogResult.OK)
			{
				Save.Press();
				string worksheetNbr = worksheet.Current.WorksheetNbr;

				PXLongOperation.StartOperation(this, () =>
				{
					using var tranScope = new PXTransactionScope();

					var worksheetGraph = CreateInstance<SOPickingWorksheetReview>();
					worksheetGraph.worksheet.Current = worksheetGraph.worksheet.Search<SOPickingWorksheet.worksheetNbr>(worksheetNbr);
					if (worksheetGraph.TryCancelWorksheet())
					{
						worksheetGraph.Actions.PressSave();
						UnlinkAllShipmentsImpl(new[] { worksheetGraph.worksheet.Current });
					}

					tranScope.Complete();
				});
			}

			return a.Get();
		}
		public PXAction<SOPickingWorksheet> CancelWorksheet;

		[PXButton, PXUIField(DisplayName = "Print Pick Lists")]
		public virtual IEnumerable printPickList(PXAdapter a)
		{
			var worksheets = a.Get<SOPickingWorksheet>().ToArray();
			if (!worksheets.Any())
				return worksheets;

			PX.SM.PrintSettings printerSettings = PXAccess.FeatureInstalled<CS.FeaturesSet.deviceHub>() ?
				PX.SM.SMPrintJobMaint.GetPrintSettings(a, new CR.NotificationUtility(this).SearchPrinter, SONotificationSource.Customer, SOReports.PrintPickerPickList, Accessinfo.BranchID)
				: null;

			LongOperationManager.StartAsyncOperation(async ct =>
			{
				PXReportRequiredException report = null;
				var worksheetReview = PXGraph.CreateInstance<SOPickingWorksheetReview>();
				foreach (var worksheet in worksheets)
				{
					PXReportRequiredException docReport = await worksheetReview.PrintPickListImpl(worksheet, printerSettings, ct);
					if (docReport == null)
						continue;

					if (report == null)
						report = docReport;
					else
						report.AddSibling(docReport.ReportID, docReport.Parameters);
				}

				if (report != null)
					throw report;
			});
			return a.Get();
		}
		public PXAction<SOPickingWorksheet> PrintPickList;

		[PXButton, PXUIField(DisplayName = "Print Packing Slips")]
		public virtual IEnumerable printPackSlips(PXAdapter a)
		{
			var worksheets = a.Get<SOPickingWorksheet>().ToArray();
			if (!worksheets.Any())
				return worksheets;

			LongOperationManager.StartAsyncOperation(async ct =>
			{
				PXReportRequiredException report = null;
				var worksheetReview = PXGraph.CreateInstance<SOPickingWorksheetReview>();
				foreach (var worksheet in worksheets)
				{
					PXReportRequiredException docReport = await worksheetReview.PrintPackSlipsImpl(worksheet, ct);
					if (docReport == null)
						continue;

					if (report == null)
						report = docReport;
					else
						report.AddSibling(docReport.ReportID, docReport.Parameters);
				}

				if (report != null)
					throw report;
			});
			return a.Get();
		}
		public PXAction<SOPickingWorksheet> PrintPackSlips;
		#endregion

		#region Initialization
		public SOPickingWorksheetReview()
		{
			worksheet.Cache.AllowInsert = false;
			worksheet.Cache.AllowDelete = false;
		}

		// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
		public class Workflow : PXGraphExtension<SOPickingWorksheetReview>
		{
			public class Conditions : Condition.Pack
			{
				public Condition IsOnHold => GetOrCreate(b => b.FromBql<
					SOPickingWorksheet.hold.IsEqual<True>
				>());
			}

			public sealed override void Configure(PXScreenConfiguration config) => Configure(config.GetScreenConfigurationContext<SOPickingWorksheetReview, SOPickingWorksheet>());
			protected static void Configure(WorkflowContext<SOPickingWorksheetReview, SOPickingWorksheet> context)
			{
				var conditions = context.Conditions.GetPack<Conditions>();

				#region Categories
				var commonCategories = CommonActionCategories.Get(context);
				var processingCategory = commonCategories.Processing;
				var printingAndMailingCategory = commonCategories.PrintingAndEmailing;
				var otherCategory = commonCategories.Other;
				#endregion

				const string initialState = "_";
				context.AddScreenConfigurationFor(screen => screen
					.StateIdentifierIs<SOPickingWorksheet.status>()
					.AddDefaultFlow(flow =>
					{
						return flow
						.WithFlowStates(fss =>
						{
							fss.Add(initialState, flowState => flowState.IsInitial(g => g.InitializeState));
							fss.Add<SOPickingWorksheet.status.hold>(fs => fs
								.WithActions(actions =>
								{
									actions.Add(g => g.ReleaseFromHold, a => a.IsDuplicatedInToolbar().WithConnotation(ActionConnotation.Success));
									actions.Add(g => g.PrintPickList, a => a.IsDuplicatedInToolbar());
									actions.Add(g => g.PrintPackSlips, a => a.IsDuplicatedInToolbar());
								}));
							fss.Add<SOPickingWorksheet.status.open>(fs => fs
								.WithActions(actions =>
								{
									actions.Add(g => g.PutOnHold, a => a.IsDuplicatedInToolbar());
									actions.Add(g => g.PrintPickList, a => a.IsDuplicatedInToolbar());
									actions.Add(g => g.PrintPackSlips, a => a.IsDuplicatedInToolbar());
								}));
							fss.Add<SOPickingWorksheet.status.picking>(fs => fs
								.WithActions(actions =>
								{
									actions.Add(g => g.PrintPickList, a => a.IsDuplicatedInToolbar());
									actions.Add(g => g.PrintPackSlips);
									actions.Add(g => g.CancelWorksheet);
								}));
							fss.Add<SOPickingWorksheet.status.picked>(fs => fs
								.WithActions(actions =>
								{
									actions.Add(g => g.PrintPickList);
									actions.Add(g => g.PrintPackSlips, a => a.IsDuplicatedInToolbar());
									actions.Add(g => g.UnlinkAllShipments);
								}));
							fss.Add<SOPickingWorksheet.status.completed>(fs => fs
								.WithActions(actions =>
								{
									actions.Add(g => g.PrintPickList);
									actions.Add(g => g.PrintPackSlips, a => a.IsDuplicatedInToolbar());
								}));
							fss.Add<SOPickingWorksheet.status.cancelled>(fs => fs
								.WithActions(actions =>
								{
									actions.Add(g => g.PrintPickList);
								}));
						})
						.WithTransitions(trans =>
						{
							trans.AddGroupFrom(initialState, gts =>
							{
								gts.Add(t => t
									.To<SOPickingWorksheet.status.hold>()
									.IsTriggeredOn(g => g.InitializeState)
									.When(conditions.IsOnHold));
								gts.Add(t => t
									.To<SOPickingWorksheet.status.open>()
									.IsTriggeredOn(g => g.InitializeState));
							});
							trans.AddGroupFrom<SOPickingWorksheet.status.hold>(gts =>
							{
								gts.Add(t => t
									.To<SOPickingWorksheet.status.open>()
									.IsTriggeredOn(g => g.ReleaseFromHold));
							});
							trans.AddGroupFrom< SOPickingWorksheet.status.open> (gts =>
							{
								gts.Add(t => t
									.To<SOPickingWorksheet.status.hold>()
									.IsTriggeredOn(g => g.PutOnHold));
							});
						});
					})
					.WithCategories(categories =>
					{
						categories.Add(processingCategory);
						categories.Add(printingAndMailingCategory);
						categories.Add(otherCategory);
					})
					.WithActions(actions =>
					{
						actions.Add(g => g.ReleaseFromHold, a => a
							.WithCategory(processingCategory)
							.WithFieldAssignments(fas => fas.Add<SOPickingWorksheet.hold>(false)));
						actions.Add(g => g.PutOnHold, a => a
							.WithCategory(processingCategory)
							.WithFieldAssignments(fas => fas.Add<SOPickingWorksheet.hold>(true)));

						actions.Add(g => g.PrintPickList, a => a
							.WithCategory(printingAndMailingCategory));
						actions.Add(g => g.PrintPackSlips, a => a
							.WithCategory(printingAndMailingCategory));

						actions.Add(g => g.UnlinkAllShipments, a => a
							.WithCategory(otherCategory));
						actions.Add(g => g.CancelWorksheet, a => a
							.WithCategory(otherCategory));
					}));
			}
		}
		#endregion

		#region DAC overrides
		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXRemoveBaseAttribute(typeof(PXSelectorAttribute))]
		[PXParent(typeof(SOShipment.FK.Worksheet), LeaveChildren = true)]
		[PXDBDefault(typeof(SOPickingWorksheet.worksheetNbr), PersistingCheck = PXPersistingCheck.Nothing)]
		protected void _(Events.CacheAttached<SOShipment.currentWorksheetNbr> e) { }

		[PXMergeAttributes]
		[PXCustomizeBaseAttribute(typeof(INTote.UnassignableToteAttribute), nameof(INTote.UnassignableToteAttribute.IsKey), false)]
		protected virtual void _(Events.CacheAttached<SOPickerToShipmentLink.toteID> e) { }
		#endregion

		#region Event handlers
		protected void _(Events.RowSelected<SOPickingWorksheet> e)
		{
			if (e.Row == null) return;

			worksheet.Cache.AllowDelete =
				e.Row.Status.IsIn(SOPickingWorksheet.status.Hold, SOPickingWorksheet.status.Open) ||
				e.Row.Status.IsIn(SOPickingWorksheet.status.Completed, SOPickingWorksheet.status.Cancelled) && shipmentLinks.SelectMain().All(sh => sh.Unlinked == true);
			worksheetLineSplits.Cache.AdjustUI().For<SOPickingWorksheetLineSplit.sortingLocationID>(a => a.Visible = e.Row.WorksheetType == SOPickingWorksheet.worksheetType.Batch);
			pickers.Cache.AdjustUI().For<SOPicker.sortingLocationID>(a => a.Visible = e.Row.WorksheetType == SOPickingWorksheet.worksheetType.Batch);

			ShowShipments.SetVisible(e.Row.WorksheetType == SOPickingWorksheet.worksheetType.Wave);
			UnlinkAllShipments.SetVisible(e.Row.WorksheetType == SOPickingWorksheet.worksheetType.Wave);

			CancelWorksheet.SetEnabled(
				e.Row.WorksheetType == SOPickingWorksheet.worksheetType.Wave ||
				e.Row.WorksheetType == SOPickingWorksheet.worksheetType.Batch && shipmentLinks.SelectMain().All(sh => sh.Picked == false || sh.Unlinked == true));
			PrintPackSlips.SetEnabled(
				e.Row.WorksheetType == SOPickingWorksheet.worksheetType.Wave ||
				e.Row.WorksheetType == SOPickingWorksheet.worksheetType.Batch && shipmentLinks.SelectMain().Any(sh => sh.Picked == true && sh.Unlinked == false));
		}
		#endregion

		private async Task<PXReportRequiredException> PrintPickListImpl(SOPickingWorksheet ws, PX.SM.PrintSettings printerSettings, CancellationToken cancellationToken)
		{
			worksheet.Current = SOPickingWorksheet.PK.Find(this, ws, PKFindOptions.IncludeDirty);

			PXReportRequiredException report = null;
			if (worksheet.Current.WorksheetType == SOPickingWorksheet.worksheetType.Batch)
			{
				// print only pick lists - pack slips will be printed on shipment fulfilment
				var parameters = new Dictionary<string, string>
				{
					[nameof(SOPickingWorksheet.WorksheetNbr)] = worksheet.Current.WorksheetNbr
				};
				report = new PXReportRequiredException(parameters, SOReports.PrintPickerPickList);
			}
			else if (worksheet.Current.WorksheetType == SOPickingWorksheet.worksheetType.Wave)
			{
				if (pickSetup.Current.PrintPickListsAndPackSlipsTogether == true)
				{
					// print both pick lists and pack slips at the same time
					foreach (SOPicker picker in pickers.Select())
					{
						pickers.Current = picker;

						var pickListParameters = new Dictionary<string, string>
						{
							[nameof(SOPickingWorksheet.WorksheetNbr)] = worksheet.Current.WorksheetNbr,
							[nameof(SOPicker.PickerNbr)] = picker.PickerNbr.ToString()
						};
						if (report == null)
							report = new PXReportRequiredException(pickListParameters, SOReports.PrintPickerPickList);
						else
							report.AddSibling(SOReports.PrintPickerPickList, pickListParameters);

						foreach (SOPickerToShipmentLink shipment in pickerShipments.Select())
						{
							var packSlipParameters = new Dictionary<string, string>
							{
								[nameof(SOPickingWorksheet.WorksheetNbr)] = worksheet.Current.WorksheetNbr,
								[nameof(SOShipment.ShipmentNbr)] = shipment.ShipmentNbr
							};
							report.AddSibling(SOReports.PrintPackSlipWave, packSlipParameters);
						}
					}
				}
				else
				{
					// print only pick lists
					var parameters = new Dictionary<string, string>
					{
						[nameof(SOPickingWorksheet.WorksheetNbr)] = worksheet.Current.WorksheetNbr
					};
					report = new PXReportRequiredException(parameters, SOReports.PrintPickerPickList);
				}
			}
			else if (worksheet.Current.WorksheetType == SOPickingWorksheet.worksheetType.Single)
			{
				var parameters = new Dictionary<string, string>
				{
					[nameof(SOPickingWorksheet.WorksheetNbr)] = worksheet.Current.WorksheetNbr
				};
				report = new PXReportRequiredException(parameters, SOReports.PrintPickerPickList);
			}

			if (report != null && printerSettings != null)
			{
				if (printerSettings.PrintWithDeviceHub != true)
				{
					printerSettings.PrintWithDeviceHub = true;
					printerSettings.DefinePrinterManually = true;
					printerSettings.PrinterID = new CR.NotificationUtility(this).SearchPrinter(SONotificationSource.Customer, SOReports.PrintPickerPickList, Accessinfo.BranchID);
					printerSettings.NumberOfCopies = 1;
				}
				await PX.SM.SMPrintJobMaint.CreatePrintJobGroup(printerSettings, report, null, cancellationToken);
			}

			return report;
		}

		private async Task<PXReportRequiredException> PrintPackSlipsImpl(SOPickingWorksheet ws, CancellationToken cancellationToken)
		{
			worksheet.Current = SOPickingWorksheet.PK.Find(this, ws, PKFindOptions.IncludeDirty);

			PXReportRequiredException report = null;
			if (worksheet.Current.WorksheetType == SOPickingWorksheet.worksheetType.Batch)
			{
				foreach (SOPickingWorksheetShipment shipment in shipmentLinks.Select())
				{
					if (shipment.Picked == false)
						continue;

					var packSlipParameters = new Dictionary<string, string>
					{
						[nameof(SOPickingWorksheet.WorksheetNbr)] = worksheet.Current.WorksheetNbr,
						[nameof(SOShipment.ShipmentNbr)] = shipment.ShipmentNbr
					};
					if (report == null)
						report = new PXReportRequiredException(packSlipParameters, SOReports.PrintPackSlipBatch);
					else
						report.AddSibling(SOReports.PrintPackSlipBatch, packSlipParameters);
				}

				if (report != null && PXAccess.FeatureInstalled<CS.FeaturesSet.deviceHub>())
				{
					await PX.SM.SMPrintJobMaint.CreatePrintJobGroup(
						new PX.SM.PrintSettings
						{
							PrintWithDeviceHub = true,
							DefinePrinterManually = true,
							PrinterID = new CR.NotificationUtility(this).SearchPrinter(SONotificationSource.Customer, SOReports.PrintPackSlipBatch, Accessinfo.BranchID),
							NumberOfCopies = 1
						}, report, null, cancellationToken);
				}
			}
			else if (worksheet.Current.WorksheetType == SOPickingWorksheet.worksheetType.Wave)
			{
				var packSlipParameters = new Dictionary<string, string>
				{
					[nameof(SOPickingWorksheet.WorksheetNbr)] = worksheet.Current.WorksheetNbr
				};
				report = new PXReportRequiredException(packSlipParameters, SOReports.PrintPackSlipWave);

				if (report != null && PXAccess.FeatureInstalled<CS.FeaturesSet.deviceHub>())
				{
					await PX.SM.SMPrintJobMaint.CreatePrintJobGroup(
						new PX.SM.PrintSettings
						{
							PrintWithDeviceHub = true,
							DefinePrinterManually = true,
							PrinterID = new CR.NotificationUtility(this).SearchPrinter(SONotificationSource.Customer, SOReports.PrintPackSlipWave, Accessinfo.BranchID),
							NumberOfCopies = 1
						}, report, null, cancellationToken);
				}
			}

			return report;
		}

		private static void UnlinkAllShipmentsImpl(IEnumerable<SOPickingWorksheet> worksheets)
		{
			var shipmentEntry = Lazy.By(() => PXGraph.CreateInstance<SOShipmentEntry>());
			foreach (var worksheet in worksheets)
			{
				var shipments = SOShipment.FK.Worksheet.SelectChildren(shipmentEntry.Value, worksheet);
				foreach (var shipment in shipments)
				{
					shipmentEntry.Value.Document.Current = shipmentEntry.Value.Document.Search<SOShipment.shipmentNbr>(shipment.ShipmentNbr);
					var worksheetUnlinker = shipmentEntry.Value.FindImplementation<SOShipmentEntryUnlinkWorksheetExt>();
					if (worksheetUnlinker.UnlinkFromWorksheet.GetEnabled())
						worksheetUnlinker.UnlinkFromWorksheet.Press();
				}
			}
		}

		public virtual bool TryCancelWorksheet()
		{
			if (worksheet.Current.Status == SOPickingWorksheet.status.Picking)
			{
				worksheet.Current.Status = SOPickingWorksheet.status.Cancelled;
				worksheet.UpdateCurrent();

				foreach (SOPickingJob job in
					SelectFrom<SOPickingJob>.
					Where<SOPickingJob.FK.Worksheet.SameAsCurrent>.
					View.Select(this))
				{
					job.Status = SOPickingJob.status.Cancelled;
					pickerJob.Update(job);
				}

				return true;
			}
			return false;
		}

		public SOPickingWorksheetPickListConfirmation PickListConfirmation => FindImplementation<SOPickingWorksheetPickListConfirmation>();

		[PXLocalizable]
		public static class Msg
		{
			public const string CancellationConfirmation = "You are about to cancel the picking worksheet. All the related pick lists will be canceled, and the picked quantity will be set to zero in the related shipments. The picked items should be moved to their initial locations manually by the warehouse staff; there is no automated guidance for this operation.";
		}
	}

	// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
	public class SOPickingWorksheetPickListConfirmation : PXGraphExtension<SOPickingWorksheetReview>
	{
		private Func<int?, bool> isEnterableDate;
		private Func<int?, bool> isWhenUsed;
		private Func<int?, bool> isEnterableOnIssue;

		public override void Initialize()
		{
			var isUserEnterable = InventoryLotSerialClassPredicate(lsc => lsc.LotSerIssueMethod == INLotSerIssueMethod.UserEnterable);

			isEnterableDate = InventoryLotSerialClassPredicate(lsc => lsc.LotSerTrackExpiration == true);
			isWhenUsed = InventoryLotSerialClassPredicate(lsc => lsc.LotSerAssign == INLotSerAssign.WhenUsed);
			isEnterableOnIssue = Func.Disjoin(isWhenUsed, isUserEnterable);

			Func<int?, bool> InventoryLotSerialClassPredicate(Func<INLotSerClass, bool> predicate) => Func.Memorize(
				(int? inventoryID) =>
					InventoryItem.PK.Find(Base, inventoryID)
					.With(ii => InventoryItem.FK.LotSerialClass.FindParent(Base, ii))
					.With(predicate));
		}

		#region DACs
		[PXHidden, Accumulator(BqlTable = typeof(SOPickingWorksheetLine))]
		public class SOPickingWorksheetLineDelta : IBqlTable
		{
			#region WorksheetNbr
			[PXDBString(15, IsUnicode = true, IsKey = true, InputMask = "")]
			public virtual String WorksheetNbr { get; set; }
			public abstract class worksheetNbr : PX.Data.BQL.BqlString.Field<worksheetNbr> { }
			#endregion
			#region LineNbr
			[PXDBInt(IsKey = true)]
			public virtual Int32? LineNbr { get; set; }
			public abstract class lineNbr : PX.Data.BQL.BqlInt.Field<lineNbr> { }
			#endregion

			#region PickedQty
			[PXDBDecimal(6)]
			public virtual Decimal? PickedQty { get; set; }
			public abstract class pickedQty : PX.Data.BQL.BqlDecimal.Field<pickedQty> { }
			#endregion
			#region BasePickedQty
			[PXDBDecimal(6)]
			public virtual Decimal? BasePickedQty { get; set; }
			public abstract class basePickedQty : PX.Data.BQL.BqlDecimal.Field<basePickedQty> { }
			#endregion

			public class AccumulatorAttribute : PXAccumulatorAttribute
			{
				public AccumulatorAttribute() => _SingleRecord = true;

				protected override bool PrepareInsert(PXCache sender, object row, PXAccumulatorCollection columns)
				{
					if (!base.PrepareInsert(sender, row, columns))
						return false;

					var returnRow = (SOPickingWorksheetLineDelta)row;

					columns.Update<pickedQty>(returnRow.PickedQty, PXDataFieldAssign.AssignBehavior.Summarize);
					columns.Update<basePickedQty>(returnRow.BasePickedQty, PXDataFieldAssign.AssignBehavior.Summarize);

					return true;
				}
			}
		}

		[PXHidden, Accumulator(BqlTable = typeof(SOPickingWorksheetLineSplit))]
		public class SOPickingWorksheetLineSplitDelta : IBqlTable
		{
			#region WorksheetNbr
			[PXDBString(15, IsUnicode = true, IsKey = true, InputMask = "")]
			public virtual String WorksheetNbr { get; set; }
			public abstract class worksheetNbr : PX.Data.BQL.BqlString.Field<worksheetNbr> { }
			#endregion
			#region LineNbr
			[PXDBInt(IsKey = true)]
			public virtual Int32? LineNbr { get; set; }
			public abstract class lineNbr : PX.Data.BQL.BqlInt.Field<lineNbr> { }
			#endregion
			#region SplitNbr
			[PXDBInt(IsKey = true)]
			public virtual Int32? SplitNbr { get; set; }
			public abstract class splitNbr : PX.Data.BQL.BqlInt.Field<splitNbr> { }
			#endregion

			#region Qty
			[PXDBDecimal(6)]
			public virtual Decimal? Qty { get; set; }
			public abstract class qty : PX.Data.BQL.BqlDecimal.Field<qty> { }
			#endregion
			#region BaseQty
			[PXDBDecimal(6)]
			public virtual Decimal? BaseQty { get; set; }
			public abstract class baseQty : PX.Data.BQL.BqlDecimal.Field<baseQty> { }
			#endregion
			#region PickedQty
			[PXDBDecimal(6)]
			public virtual Decimal? PickedQty { get; set; }
			public abstract class pickedQty : PX.Data.BQL.BqlDecimal.Field<pickedQty> { }
			#endregion
			#region BasePickedQty
			[PXDBDecimal(6)]
			public virtual Decimal? BasePickedQty { get; set; }
			public abstract class basePickedQty : PX.Data.BQL.BqlDecimal.Field<basePickedQty> { }
			#endregion
			#region SortingLocationID
			[PXDBInt]
			public virtual int? SortingLocationID { get; set; }
			public abstract class sortingLocationID : PX.Data.BQL.BqlInt.Field<sortingLocationID> { }
			#endregion

			public class AccumulatorAttribute : PXAccumulatorAttribute
			{
				public AccumulatorAttribute() => _SingleRecord = true;

				protected override bool PrepareInsert(PXCache sender, object row, PXAccumulatorCollection columns)
				{
					if (!base.PrepareInsert(sender, row, columns))
						return false;

					var returnRow = (SOPickingWorksheetLineSplitDelta)row;

					columns.Update<qty>(returnRow.Qty, PXDataFieldAssign.AssignBehavior.Summarize);
					columns.Update<baseQty>(returnRow.BaseQty, PXDataFieldAssign.AssignBehavior.Summarize);
					columns.Update<pickedQty>(returnRow.PickedQty, PXDataFieldAssign.AssignBehavior.Summarize);
					columns.Update<basePickedQty>(returnRow.BasePickedQty, PXDataFieldAssign.AssignBehavior.Summarize);
					columns.Update<sortingLocationID>(returnRow.SortingLocationID, PXDataFieldAssign.AssignBehavior.Initialize);
					return true;
				}
			}
		}
		#endregion

		#region Views
		public SelectFrom<SOPickingWorksheetLineDelta>.View WSLineDelta;
		public SelectFrom<SOPickingWorksheetLineSplitDelta>.View WSSplitDelta;

		public SelectFrom<INCartSplit>.View CartSplits;
		public SelectFrom<SOPickListEntryToCartSplitLink>.View CartLinks;
		#endregion

		#region Actions
		[PXButton, PXUIField(DisplayName = "Pick All Shipments")]
		protected virtual void fulfillShipments() => Base.LongOperationManager.StartAsyncOperation(ct=>FulfillShipmentsAndConfirmWorksheet(Base.worksheet.Current, ct));
		public PXAction<SOPickingWorksheet> FulfillShipments;
		#endregion

		#region Event Handlers
		protected virtual void _(Events.RowSelected<SOPickingWorksheet> e)
		{
			FulfillShipments.SetEnabled(
				Base.worksheet.Current != null &&
				Base.worksheet.Current.Status.IsIn(SOPickingWorksheet.status.Picking, SOPickingWorksheet.status.Picked) &&
				Base.shipmentLinks.SelectMain().Any(sh => sh.Picked == false && sh.Unlinked == false) &&
				Base.pickers.SelectMain().Any(p => p.Confirmed == true));
		}
		#endregion

		#region Confirm Pick List
		public virtual void ConfirmPickList(SOPicker pickList, int? sortingLocationID)
		{
			if (pickList == null)
				throw new PXArgumentException(nameof(pickList), ErrorMessages.ArgumentNullException);

			Base.Clear();

			Base.worksheet.Current = SOPickingWorksheet.PK.Find(Base, pickList.WorksheetNbr, PKFindOptions.IncludeDirty).With(ValidateMutability);
			Base.pickers.Current = SOPicker.PK.Find(Base, pickList.WorksheetNbr, pickList.PickerNbr, PKFindOptions.IncludeDirty).With(ValidateMutability);
			Base.pickerJob.Current = Base.pickerJob.Select();

			var wsSplits =
				SelectFrom<SOPickingWorksheetLine>.
				InnerJoin<SOPickingWorksheetLineSplit>.On<SOPickingWorksheetLineSplit.FK.WorksheetLine>.
				Where<SOPickingWorksheetLine.FK.Worksheet.SameAsCurrent>.
				View.ReadOnly.Select(Base)
				.AsEnumerable()
				.Select(r => (Line: r.GetItem<SOPickingWorksheetLine>(), Split: r.GetItem<SOPickingWorksheetLineSplit>()))
				.ToArray();

			var entries = new
				SelectFrom<SOPickerListEntry>.
				InnerJoin<INLocation>.On<SOPickerListEntry.FK.Location>.
				InnerJoin<InventoryItem>.On<SOPickerListEntry.FK.InventoryItem>.
				Where<SOPickerListEntry.FK.Picker.SameAsCurrent>.
				OrderBy<
					INLocation.pathPriority.Asc,
					INLocation.locationCD.Asc,
					InventoryItem.inventoryCD.Asc,
					SOPickerListEntry.lotSerialNbr.Asc>.
				View(Base)
				.SelectMain()
				.GroupBy(e => (InventoryID: e.InventoryID, SubItemID: e.SubItemID, OrderLineUOM: e.OrderLineUOM, LocationID: e.LocationID, LotSerialNbr: e.LotSerialNbr))
				.Select(g => (Key: g.Key, PickedQty: g.Sum(e => e.PickedQty ?? 0), ExpireDate: g.Min(e => e.ExpireDate)))
				.Where(e => e.PickedQty > 0)
				.ToArray();

			var pickListEnterableItems = entries
				.Select(e => e.Key.InventoryID)
				.Distinct()
				.Where(itemID => isEnterableOnIssue(itemID))
				.ToHashSet();

			(var entriesEnterable, var entriesFixed) = entries.DisuniteBy(e => pickListEnterableItems.Contains(e.Key.InventoryID.Value));
			(var wsSplitsEnterable, var wsSplitsFixed) = wsSplits.DisuniteBy(e => pickListEnterableItems.Contains(e.Split.InventoryID.Value));

			UpdateFixedWorksheetSplits(wsSplitsFixed, entriesFixed, sortingLocationID);
			UpdateEnterableWorksheetSplits(wsSplitsEnterable, entriesEnterable, sortingLocationID);

			Base.pickers.Current.Confirmed = true;
			if (sortingLocationID != null)
				Base.pickers.Current.SortingLocationID = sortingLocationID;
			Base.pickers.UpdateCurrent();

			if (Base.pickerJob.Current != null)
			{
				Base.pickerJob.Current.Status = SOPickingJob.status.Picked;
				Base.pickerJob.UpdateCurrent();
			}

			if (sortingLocationID != null)
				RemoveItemsFromPickerCart(Base.pickers.Current);

			if (Base.worksheet.Current.SingleShipmentNbr != null)
				Base.GetExtension<ShipmentWorkLog>().CloseFor(Base.worksheet.Current.SingleShipmentNbr);

			Base.Save.Press();
		}

		protected virtual void RemoveItemsFromPickerCart(SOPicker picker)
		{
			var links = SOPickListEntryToCartSplitLink.FK.Picker.SelectChildren(Base, picker);
			foreach (var link in links)
			{
				decimal linkQty = link.Qty.Value;
				CartLinks.Delete(link);

				var cartSplit = INCartSplit.PK.Find(Base, link.SiteID, link.CartID, link.CartSplitLineNbr);
				if (cartSplit != null)
				{
					decimal restQty = cartSplit.Qty.Value - linkQty;
					if (restQty <= 0)
						CartSplits.Delete(cartSplit);
					else
					{
						CartSplits.Cache.SetValueExt<INCartSplit.qty>(cartSplit, restQty);
						CartSplits.Update(cartSplit);
					}
				}
			}
		}

		protected virtual void UpdateFixedWorksheetSplits(
			IEnumerable<(SOPickingWorksheetLine Line, SOPickingWorksheetLineSplit Split)> wsSplits,
			IEnumerable<((int? InventoryID, int? SubItemID, string OrderLineUOM, int? LocationID, string LotSerialNbr) Key, decimal PickedQty, DateTime? ExpireDate)> entries,
			int? sortingLocationID)
		{
			var affectedLines =
				(from s in wsSplits
				 join e in entries
					 on (s.Split.InventoryID, s.Split.SubItemID, s.Line.UOM, s.Split.LocationID, s.Split.LotSerialNbr)
					 equals (e.Key.InventoryID, e.Key.SubItemID, e.Key.OrderLineUOM, e.Key.LocationID, e.Key.LotSerialNbr)
				 select (Line: s.Line, Split: s.Split, Entry: e))
				 .GroupBy(t => t.Line.LineNbr)
				 .ToArray();

			foreach (var splitsByLine in affectedLines)
			{
				var line = splitsByLine.First().Line;
				var lineBasePickedQty = splitsByLine.Sum(s => s.Entry.PickedQty);

				var lineDelta = PropertyTransfer.Transfer(line, new SOPickingWorksheetLineDelta());
				lineDelta.BasePickedQty = lineBasePickedQty;
				lineDelta.PickedQty = INUnitAttribute.ConvertFromBase(WSLineDelta.Cache, line.InventoryID, line.UOM, lineBasePickedQty, INPrecision.NOROUND);
				WSLineDelta.Insert(lineDelta);

				foreach (var (Line, Split, Entry) in splitsByLine)
				{
					var splitDelta = PropertyTransfer.Transfer(Split, new SOPickingWorksheetLineSplitDelta());
					splitDelta.BasePickedQty = Entry.PickedQty;
					splitDelta.PickedQty = Entry.PickedQty;
					splitDelta.BaseQty = 0;
					splitDelta.Qty = 0;
					splitDelta.SortingLocationID = sortingLocationID;
					WSSplitDelta.Insert(splitDelta);
				}
			}
		}

		protected virtual void UpdateEnterableWorksheetSplits(
			IEnumerable<(SOPickingWorksheetLine Line, SOPickingWorksheetLineSplit Split)> wsSplits,
			IEnumerable<((int? InventoryID, int? SubItemID, string OrderLineUOM, int? LocationID, string LotSerialNbr) Key, decimal PickedQty, DateTime? ExpireDate)> entries,
			int? sortingLocationID)
		{
			PopulateWorksheetSplitsFromPickListEntries(
				wsSplits.Where(r => r.Split.IsUnassigned == true || isEnterableOnIssue(r.Split.InventoryID)),
				entries,
				sortingLocationID);
		}

		protected virtual void PopulateWorksheetSplitsFromPickListEntries(
			IEnumerable<(SOPickingWorksheetLine Line, SOPickingWorksheetLineSplit Split)> wsSplits,
			IEnumerable<((int? InventoryID, int? SubItemID, string OrderLineUOM, int? LocationID, string LotSerialNbr) Key, decimal PickedQty, DateTime? ExpireDate)> entries,
			int? sortingLocationID)
		{
			var wsSplitsEnterableQueue = wsSplits
				.OrderBy(r => r.Split.InventoryID)
				.ThenBy(r => r.Split.SubItemID)
				.ThenBy(r => r.Line.UOM)
				.ThenBy(r => r.Split.LocationID)
				.ThenByDescending(r => r.Split.HasGeneratedLotSerialNbr == true)
				.ThenByDescending(r => r.Split.Qty)
				.ToQueue();

			var entriesEnterableQueue = entries
				.OrderBy(r => r.Key.InventoryID)
				.ThenBy(r => r.Key.SubItemID)
				.ThenBy(r => r.Key.OrderLineUOM)
				.ThenBy(r => r.Key.LocationID)
				.ThenByDescending(r => r.PickedQty)
				.ToQueue();

			while (wsSplitsEnterableQueue.Count > 0 && entriesEnterableQueue.Count > 0)
			{
				(var line, var split) = wsSplitsEnterableQueue.Peek();
				(var entry, decimal pickedQty, DateTime? expireDate) = entriesEnterableQueue.Peek();

				var entryKey =
				(
					InventoryID: entry.InventoryID,
					SubItemID: entry.SubItemID,
					UOM: entry.OrderLineUOM,
					LocationID: entry.LocationID
				);
				var splitKey =
				(
					InventoryID: split.InventoryID,
					SubItemID: split.SubItemID,
					UOM: line.UOM,
					LocationID: split.LocationID
				);

				if (!entryKey.Equals(splitKey))
				{
					bool bypassSubItemID = entryKey.InventoryID != splitKey.InventoryID;
					bool bypassUOM = bypassSubItemID || entryKey.SubItemID != splitKey.SubItemID;
					bool bypassLocationID = bypassUOM || entryKey.UOM != splitKey.UOM;

					if (entryKey.CompareTo(splitKey) > 0)
					{
						int dequeueCount = wsSplitsEnterableQueue
							.TakeWhile(r =>
								(r.Split.InventoryID == split.InventoryID) &&
								(bypassSubItemID || r.Split.SubItemID == split.SubItemID) &&
								(bypassUOM || r.Line.UOM == line.UOM) &&
								(bypassLocationID || r.Split.LocationID == split.LocationID))
							.Count();
						wsSplitsEnterableQueue.Dequeue(dequeueCount).Consume();
					}
					else
					{
						int dequeueCount = entriesEnterableQueue
							.TakeWhile(e =>
								(e.Key.InventoryID == entry.InventoryID) &&
								(bypassSubItemID || e.Key.SubItemID == entry.SubItemID) &&
								(bypassUOM || e.Key.OrderLineUOM == entry.OrderLineUOM) &&
								(bypassLocationID || e.Key.LocationID == entry.LocationID))
							.Count();
						entriesEnterableQueue.Dequeue(dequeueCount).Consume();
					}

					continue;
				}
				else
				{
					decimal splitQty = split.Qty.Value;

					do
					{
						decimal actualQty = Math.Min(splitQty, pickedQty);
						pickedQty -= actualQty;
						splitQty -= actualQty;

						var lineDelta = PropertyTransfer.Transfer(line, new SOPickingWorksheetLineDelta());
						lineDelta.BasePickedQty = actualQty;
						lineDelta.PickedQty = INUnitAttribute.ConvertFromBase(WSLineDelta.Cache, line.InventoryID, line.UOM, actualQty, INPrecision.NOROUND);
						var existingLine = WSLineDelta.Locate(lineDelta);
						if (existingLine == null)
						{
							WSLineDelta.Insert(lineDelta);
						}
						else
						{
							existingLine.BasePickedQty += lineDelta.BasePickedQty;
							existingLine.PickedQty += lineDelta.PickedQty;
						}

						var unassignedDelta = PropertyTransfer.Transfer(split, new SOPickingWorksheetLineSplitDelta());
						unassignedDelta.BaseQty = -actualQty;
						unassignedDelta.Qty = -actualQty;
						var existingUnassignedDelta = WSSplitDelta.Locate(unassignedDelta);
						if (existingUnassignedDelta == null)
						{
							if (split.SortingLocationID == null)
								unassignedDelta.SortingLocationID = sortingLocationID;
							WSSplitDelta.Insert(unassignedDelta);
						}
						else
						{
							existingUnassignedDelta.BaseQty -= actualQty;
							existingUnassignedDelta.Qty -= actualQty;
						}

						Base.worksheetLines.Current = line;
						var assignedSplit = PropertyTransfer.Transfer(split, new SOPickingWorksheetLineSplit());
						assignedSplit.SplitNbr = null;
						assignedSplit.LotSerialNbr = entry.LotSerialNbr;
						assignedSplit.ExpireDate = expireDate;
						assignedSplit.PickedQty = actualQty;
						assignedSplit.Qty = actualQty;
						assignedSplit.BasePickedQty = actualQty;
						assignedSplit.BaseQty = actualQty;
						assignedSplit.SortingLocationID = sortingLocationID;
						assignedSplit.IsUnassigned = false;
						assignedSplit.HasGeneratedLotSerialNbr = false;
						assignedSplit = Base.worksheetLineSplits.Insert(assignedSplit);

						if (pickedQty == 0)
						{
							entriesEnterableQueue.Dequeue();
							if (entriesEnterableQueue.Count == 0)
								break;

							(entry, pickedQty, expireDate) = entriesEnterableQueue.Peek();
							entryKey =
							(
								InventoryID: entry.InventoryID,
								SubItemID: entry.SubItemID,
								UOM: entry.OrderLineUOM,
								LocationID: entry.LocationID
							);
						}

						if (splitQty == 0)
						{
							wsSplitsEnterableQueue.Dequeue();
							if (wsSplitsEnterableQueue.Count == 0)
								break;

							(line, split) = wsSplitsEnterableQueue.Peek();
							splitQty = split.Qty.Value;
							splitKey =
							(
								InventoryID: split.InventoryID,
								SubItemID: split.SubItemID,
								UOM: line.UOM,
								LocationID: split.LocationID
							);
						}
					}
					while (entryKey.Equals(splitKey));
				}
			}
		}
		#endregion

		#region Fulfill Shipments
		public virtual IEnumerable<string> TryFulfillShipments(SOPickingWorksheet worksheet)
		{
			if (worksheet == null)
				throw new PXArgumentException(nameof(worksheet), ErrorMessages.ArgumentNullException);

			var fulfilledShipmentNbrs =
				worksheet.WorksheetType == SOPickingWorksheet.worksheetType.Single ? FulfillShipmentsSingle(worksheet) :
				worksheet.WorksheetType == SOPickingWorksheet.worksheetType.Wave ? FulfillShipmentsWave(worksheet) :
				worksheet.WorksheetType == SOPickingWorksheet.worksheetType.Batch ? FulfillShipmentsBatch(worksheet) :
				throw new NotSupportedException();

			return fulfilledShipmentNbrs;
		}

		#region Fulfill Merged
		protected virtual IEnumerable<string> FulfillShipmentsBatch(SOPickingWorksheet worksheet)
		{
			Base.Clear();

			Base.worksheet.Current = SOPickingWorksheet.PK.Find(Base, worksheet.WorksheetNbr, PKFindOptions.IncludeDirty).With(ValidateMutability);

			var wsSplits =
				SelectFrom<SOPickingWorksheetLine>.
				InnerJoin<SOPickingWorksheetLineSplit>.On<SOPickingWorksheetLineSplit.FK.WorksheetLine>.
				Where<
					SOPickingWorksheetLine.FK.Worksheet.SameAsCurrent.
					And<SOPickingWorksheetLineSplit.isUnassigned.IsEqual<False>>>.
				View.Select(Base)
				.AsEnumerable()
				.Select(r =>
				(
					Line: r.GetItem<SOPickingWorksheetLine>(),
					Split: r.GetItem<SOPickingWorksheetLineSplit>()
				))
				.ToArray();

			var expirationDates = wsSplits
				.Where(s => isEnterableDate(s.Split.InventoryID))
				.Distinct(s => (s.Split.InventoryID, s.Split.LotSerialNbr))
				.ToDictionary(s => (s.Split.InventoryID, s.Split.LotSerialNbr), s => s.Split.ExpireDate.Value)
				.AsReadOnly();

			var baseAvailability =
				wsSplits
				.GroupBy(s =>
				(
					InventoryID: s.Split.InventoryID,
					SubItemID: s.Split.SubItemID,
					UOM: s.Line.UOM,
					LocationID: s.Split.LocationID,
					SortLocationID: s.Split.SortingLocationID,
					LotSerialNbr: s.Split.LotSerialNbr
				))
				.ToDictionary(g => g.Key, g => g.Sum(s => s.Split.PickedQty ?? 0));

			var shipmentAssignedSplits =
				SelectFrom<SOShipment>.
				InnerJoin<SOShipLine>.On<SOShipLine.FK.Shipment>.
				InnerJoin<SOShipLineSplit>.On<SOShipLineSplit.FK.ShipmentLine>.
				LeftJoin<INTranSplit>.On<
					INTranSplit.FK.ShipLineSplit.
					And<INTranSplit.locationID.IsNotEqual<INTranSplit.toLocationID>>>.
				Where<SOShipment.FK.Worksheet.SameAsCurrent>.
				View.Select(Base)
				.AsEnumerable()
				.Select(r =>
				(
					Shipment: r.GetItem<SOShipment>(),
					Line: r.GetItem<SOShipLine>(),
					Split: r.GetItem<SOShipLineSplit>(),
					TransferSplit: r.GetItem<INTranSplit>()
				))
				.ToArray();

			var shipmentUnassignedSplits =
				SelectFrom<SOShipment>.
				InnerJoin<SOShipLine>.On<SOShipLine.FK.Shipment>.
				InnerJoin<Unassigned.SOShipLineSplit>.On<Unassigned.SOShipLineSplit.FK.ShipmentLine>.
				LeftJoin<INTranSplit>.On<True.IsEqual<False>>.
				Where<SOShipment.FK.Worksheet.SameAsCurrent>.
				View.Select(Base)
				.AsEnumerable()
				.Select(r =>
				(
					Shipment: r.GetItem<SOShipment>(),
					Line: r.GetItem<SOShipLine>(),
					Split: PropertyTransfer.Transfer(r.GetItem<Unassigned.SOShipLineSplit>(), new SOShipLineSplit()), // make of assigned-type
					TransferSplit: r.GetItem<INTranSplit>()
				))
				.ToArray();

			var shipmentSplits = shipmentAssignedSplits
				.Concat(shipmentUnassignedSplits)
				.OrderBy(r => r.Shipment.ShipDate)
				.ThenBy(r => r.Shipment.LineTotal)
				.ThenBy(r => r.Shipment.ShipmentNbr)
				.ToArray();

			(var pickedShipmentSplits, var vacantShipmentSplits) = shipmentSplits.DisuniteBy(s => s.Shipment.Picked == true).With(pair => (pair.Affirmatives.ToArray(), pair.Negatives.ToArray()));
			if (pickedShipmentSplits.Length + vacantShipmentSplits.Length != shipmentSplits.Length || pickedShipmentSplits.IntersectBy(vacantShipmentSplits, s => s.Shipment.ShipmentNbr).Count() != 0)
				throw new PXInvalidOperationException();

			var transferLinesForUnassignedSplits =
				pickedShipmentSplits
				.Where(s => s.TransferSplit.LocationID == null)
				.Select(s => s.Line)
				.Distinct(l => (l.ShipmentNbr, l.LineNbr))
				.ToDictionary(
					l => (l.ShipmentNbr, l.LineNbr),
					l => (INTran)
						SelectFrom<INTran>.
						InnerJoin<INTranSplit>.On<INTranSplit.FK.Tran>.
						Where<
							INTranSplit.FK.SOShipmentLine.SameAsCurrent.
							And<INTranSplit.locationID.IsNotEqual<INTranSplit.toLocationID>>>.
						View.SelectSingleBound(Base, new[] { l }));

			var alreadyPickedQty =
				pickedShipmentSplits
				.GroupBy(s =>
				(
					InventoryID: s.Split.InventoryID,
					SubItemID: s.Split.SubItemID,
					UOM: s.Line.UOM,
					LocationID: s.TransferSplit.LocationID ?? transferLinesForUnassignedSplits[(s.Line.ShipmentNbr, s.Line.LineNbr)].LocationID,
					SortLocationID: s.Split.LocationID,
					LotSerialNbr: s.Split.LotSerialNbr
				))
				.ToDictionary(g => g.Key, g => g.Sum(s => s.Split.PickedQty ?? 0));

			if (alreadyPickedQty.Any())
			{
				foreach (var deduction in alreadyPickedQty)
					baseAvailability[deduction.Key] -= deduction.Value;
				baseAvailability.RemoveRange(baseAvailability.Where(kvp => kvp.Value == 0).Select(kvp => kvp.Key).ToArray());
			}
			var availability = baseAvailability
				.GroupBy(kvp =>
				(
					InventoryID: kvp.Key.InventoryID,
					SubItemID: kvp.Key.SubItemID,
					UOM: kvp.Key.UOM,
					LocationID: kvp.Key.LocationID,
					LotSerialNbr: kvp.Key.LotSerialNbr
				))
				.Where(g => g.Any(e => e.Value > 0))
				.ToDictionary(
					g => g.Key,
					g => g.Select(e => (SortLocationID: e.Key.SortLocationID, PickedQty: e.Value)).OrderByDescending(e => e.PickedQty).ToList());

			var itemVariety = availability.Select(kvp => kvp.Key.InventoryID).ToHashSet();
			var locationVariety = availability.Select(kvp => kvp.Key.LocationID).ToHashSet();
			var uomVariety = availability.Select(kvp => kvp.Key.UOM).ToHashSet();

			var shipmentsApplicableByAllVarieties =
				vacantShipmentSplits
				.GroupBy(s => s.Shipment.ShipmentNbr)
				.Where(g => g.All(s =>
					itemVariety.Contains(s.Split.InventoryID) &&
					locationVariety.Contains(s.Split.LocationID) &&
					uomVariety.Contains(s.Line.UOM)))
				.Select(splitsByShipment =>
				(
					ShipmentNbr: splitsByShipment.Key,
					SiteID: splitsByShipment.First().Shipment.SiteID,
					Demands: splitsByShipment
						.GroupBy(s =>
						(
							InventoryID: s.Split.InventoryID,
							SubItemID: s.Split.SubItemID,
							UOM: s.Line.UOM,
							LocationID: s.Split.LocationID,
							LotSerialNbr: s.Split.LotSerialNbr
						))
						.ToDictionary(g => g.Key, g => g.Sum(s => s.Split.Qty ?? 0)),
					Details: splitsByShipment.Select(d => (Line: d.Line, Split: d.Split)).ToArray()
				))
				.ToArray();

			var transferGraph = Lazy.By(() => PXGraph.CreateInstance<INTransferEntry>());
			var shipmentGraph = Lazy.By(() => PXGraph.CreateInstance<SOShipmentEntry>());
			var fulfilledShipments = new List<string>();
			using (var ts = new PXTransactionScope())
			{
				foreach (var shipment in shipmentsApplicableByAllVarieties)
				{
					if (shipment.Demands.All(demand => isEnterableOnIssue(demand.Key.InventoryID)
						? availability
							.Where(kvp =>
								kvp.Key.InventoryID == demand.Key.InventoryID &&
								kvp.Key.SubItemID == demand.Key.SubItemID &&
								kvp.Key.UOM == demand.Key.UOM &&
								kvp.Key.LocationID == demand.Key.LocationID)
							.Sum(kvp => kvp.Value.Sum(s => s.PickedQty)) >= demand.Value
						: availability.ContainsKey(demand.Key) && availability[demand.Key].Sum(s => s.PickedQty) >= demand.Value))
					{
						HoldShipment(shipmentGraph.Value, shipment.ShipmentNbr);

						var soSplitToTransferSplit = CreateTransferSplits(availability, shipment.Details);

						var assignedSplits = MakeAllSplitsAssigned(shipmentGraph.Value, shipment.ShipmentNbr, availability, shipment.Details, expirationDates);

						foreach (var assigned in assignedSplits)
							DecreaseAvailability(availability, assigned);

						CreateTransferToStorageLocations(transferGraph.Value, shipment.SiteID, soSplitToTransferSplit.Select(s => s.tranSplit));

						PutShipmentOnStorageLocations(
							shipmentGraph.Value,
							shipment.ShipmentNbr,
							(from asp in assignedSplits
							join tsp in soSplitToTransferSplit on asp.Split.LineNbr equals tsp.soSplit.LineNbr
							select (Split: asp.Split, SortLocationID: tsp.tranSplit.ToLocationID))
							.Distinct(r => r.Split.SplitLineNbr));

						fulfilledShipments.Add(shipment.ShipmentNbr);
					}
				}
				ts.Complete();
			}

			return fulfilledShipments;
		}

		protected virtual IEnumerable<(SOShipLine Line, SOShipLineSplit Split)>
			MakeAllSplitsAssigned(
				SOShipmentEntry shipmentEntry,
				string shipmentNbr,
				IReadOnlyDictionary<
					(int? InventoryID, int? SubItemID, string UOM, int? LocationID, string LotSerialNbr),
					List<(int? SortLocationID, decimal PickedQty)>
				> availability,
				IEnumerable<(SOShipLine Line, SOShipLineSplit Split)> details,
				IReadOnlyDictionary<(int? InventoryID, string LotSerialNbr), DateTime> expirationDates)
		{
			(var enterableSplits, var fixedSplits) = details.DisuniteBy(r => r.Split.IsUnassigned == true || r.Split.HasGeneratedLotSerialNbr == true);
			if (enterableSplits.Any() == false)
				return fixedSplits;

			shipmentEntry.Clear();
			shipmentEntry.Document.Current = shipmentEntry.Document.Search<SOShipment.shipmentNbr>(shipmentNbr);
			PXSelectBase<SOShipLine> shLines = shipmentEntry.Transactions;
			PXSelectBase<SOShipLineSplit> shSplits = shipmentEntry.splits;

			var enterableItems = enterableSplits.Select(s => s.Split.InventoryID).ToHashSet();
			var availabilityQueue =
				availability
				.Where(a => enterableItems.Contains(a.Key.InventoryID))
				.SelectMany(a => a.Value.Select(e =>
				(
					Key:
					(
						a.Key.InventoryID,
						a.Key.SubItemID,
						a.Key.UOM,
						a.Key.LocationID
					),
					SortLocationID: e.SortLocationID,
					LotSerialNbr: a.Key.LotSerialNbr,
					PickedQty: e.PickedQty
				)))
				.OrderBy(e => e.Key.InventoryID)
				.ThenBy(e => e.Key.SubItemID)
				.ThenBy(e => e.Key.UOM)
				.ThenBy(e => e.Key.LocationID)
				.ThenBy(e => e.LotSerialNbr)
				.ToQueue();

			enterableSplits = enterableSplits
				.OrderBy(e => e.Split.InventoryID)
				.ThenBy(e => e.Split.SubItemID)
				.ThenBy(e => e.Line.UOM)
				.ThenBy(e => e.Split.LocationID)
				.ThenBy(e => e.Line.LineNbr)
				.ToArray();

			var currentEntry = availabilityQueue.Dequeue();
			decimal availableQty = currentEntry.PickedQty;

			var assignedSplits = new List<(SOShipLine Line, SOShipLineSplit Split)>();
			foreach (var detail in enterableSplits)
			{
				shLines.Current = shLines.Search<SOShipLine.shipmentNbr, SOShipLine.lineNbr>(shipmentNbr, detail.Line.LineNbr);
				decimal unassignedQty = detail.Split.Qty.Value;
				var detailKey = (detail.Split.InventoryID, detail.Split.SubItemID, detail.Line.UOM, detail.Split.LocationID);

				while (!detailKey.Equals(currentEntry.Key))
				{
					if (availabilityQueue.Count == 0)
					{
						currentEntry = default;
						availableQty = 0;
						break;
					}
					else
					{
						currentEntry = availabilityQueue.Dequeue();
						availableQty = currentEntry.PickedQty;
					}
				}

				while (unassignedQty > 0 && availableQty > 0 && detailKey.Equals(currentEntry.Key))
				{
					var qty = Math.Min(unassignedQty, availableQty);
					unassignedQty -= qty;
					availableQty -= qty;

					if (detail.Split.HasGeneratedLotSerialNbr == true)
					{
						if (detail.Split.Qty == qty)
						{
							shSplits.Delete(detail.Split);
						}
						else
						{
							detail.Split.Qty -= qty;
							shSplits.Update(detail.Split);
						}
					}

					var newSplit = shSplits.Insert();
					newSplit.Qty = qty;
					newSplit.BaseQty = qty;
					newSplit.LocationID = currentEntry.Key.LocationID;
					newSplit.LotSerialNbr = currentEntry.LotSerialNbr;
					if (expirationDates.TryGetValue((currentEntry.Key.InventoryID, currentEntry.LotSerialNbr), out DateTime expireDate))
						newSplit.ExpireDate = expireDate;
					newSplit = shSplits.Update(newSplit);
					assignedSplits.Add((shLines.Current, newSplit));

					if (availableQty == 0)
					{
						if (availabilityQueue.Count == 0)
							break;

						currentEntry = availabilityQueue.Dequeue();
						availableQty = currentEntry.PickedQty;
					}
				}
			}

			shipmentEntry.Save.Press();

			return fixedSplits
				.Concat(assignedSplits)
				.OrderBy(r => r.Line.LineNbr)
				.ThenBy(r => r.Split.SplitLineNbr)
				.ToArray();
		}

		protected virtual IEnumerable<(SOShipLineSplit soSplit, INTranSplit tranSplit)>
			CreateTransferSplits(
				IReadOnlyDictionary<
					(int? InventoryID, int? SubItemID, string UOM, int? LocationID, string LotSerialNbr),
					List<(int? SortLocationID, decimal PickedQty)>
				> availability,
				IEnumerable<(SOShipLine Line, SOShipLineSplit Split)> details)
		{
			var availabilityForTransfer = availability
				.Select(kvp =>
				(
					Key:
					(
						InventoryID: kvp.Key.InventoryID,
						SubItemID: kvp.Key.SubItemID,
						UOM: kvp.Key.UOM,
						LocationID: kvp.Key.LocationID
					),
					Value: kvp.Value.Select(e =>
					(
						SortLocationID: e.SortLocationID,
						LotSerailNbr: isWhenUsed(kvp.Key.InventoryID) ? "" : kvp.Key.LotSerialNbr,
						PickedQty: e.PickedQty
					)).ToList()
				))
				.GroupBy(r => r.Key)
				.ToDictionary(
					r => r.Key,
					r => r.Aggregate(
								Enumerable.Empty<(int? SortLocationID, string LotSerialNbr, decimal PickedQty)>(),
								(acc, elem) => acc.Concat(elem.Value))
							.GroupBy(t => (t.SortLocationID, t.LotSerialNbr), t => t.PickedQty)
							.Select(t =>
							(
								SortLocationID: t.Key.SortLocationID,
								LotSerialNbr: t.Key.LotSerialNbr,
								PickedQty: t.Sum()
							))
							.ToList());

			var soSplitToTransferSplit = new List<(SOShipLineSplit soSplit, INTranSplit tranSplit)>();
			foreach (var detail in details)
			{
				var detailKey =
				(
					InventoryID: detail.Split.InventoryID,
					SubItemID: detail.Split.SubItemID,
					UOM: detail.Line.UOM,
					LocationID: detail.Split.LocationID
				);

				decimal restDemandQty = detail.Split.BaseQty.Value;
				var availableOrdered = availabilityForTransfer[detailKey]
					.OrderByDescending(a => string.Equals(a.LotSerialNbr, detail.Split.LotSerialNbr, StringComparison.OrdinalIgnoreCase)) // same lot serial first
					.OrderBy(a => a.PickedQty);

				foreach (var available in availableOrdered)
				{
					decimal qtyToApply = Math.Min(available.PickedQty, restDemandQty);

					if (available.PickedQty > qtyToApply)
					{
						decimal restPickedQty = available.PickedQty - qtyToApply;

						availabilityForTransfer[detailKey].Insert(
							index: availabilityForTransfer[detailKey]
								.TakeWhile(a => a.PickedQty < restPickedQty)
								.Count(),
							item:
							(
								SortLocationID: available.SortLocationID,
								LotSerialNbr: available.LotSerialNbr,
								PickedQty: restPickedQty
							));
					}
					availabilityForTransfer[detailKey].Remove(available);

					var tranSplit = new INTranSplit
					{
						SiteID = detail.Split.SiteID,
						ToSiteID = detail.Split.SiteID,
						LocationID = detail.Split.LocationID,
						ToLocationID = available.SortLocationID,

						InventoryID = detail.Split.InventoryID,
						SubItemID = detail.Split.SubItemID,
						LotSerialNbr = available.LotSerialNbr,
						ExpireDate = detail.Split.ExpireDate,

						Qty = qtyToApply,
						UOM = detail.Split.UOM,
						BaseQty = qtyToApply,

						ShipmentNbr = detail.Split.ShipmentNbr,
						ShipmentLineNbr = detail.Split.LineNbr,
						ShipmentLineSplitNbr = detail.Split.SplitLineNbr
					};

					soSplitToTransferSplit.Add((detail.Split, tranSplit));

					restDemandQty -= qtyToApply;
					if (restDemandQty <= 0)
						break;
				}
			}

			return soSplitToTransferSplit;
		}

		protected virtual (int? SortLocationID, decimal PickedQty) DecreaseAvailability(
			IReadOnlyDictionary<
				(int? InventoryID, int? SubItemID, string UOM, int? LocationID, string LotSerialNbr),
				List<(int? SortLocationID, decimal PickedQty)>
			> availability,
			(SOShipLine Line, SOShipLineSplit Split) detail)
		{
			var detailKey =
			(
				InventoryID: detail.Split.InventoryID,
				SubItemID: detail.Split.SubItemID,
				UOM: detail.Line.UOM,
				LocationID: detail.Split.LocationID,
				LotSerialNbr: detail.Split.LotSerialNbr
			);

			var available = availability[detailKey].OrderBy(a => a.PickedQty).First(a => a.PickedQty >= detail.Split.BaseQty);
			if (available.PickedQty > detail.Split.BaseQty)
			{
				decimal restQty = available.PickedQty - detail.Split.BaseQty.Value;
				availability[detailKey].Insert(
					index: availability[detailKey]
						.TakeWhile(a => a.PickedQty < restQty)
						.Count(),
					item:
					(
						SortLocationID: available.SortLocationID,
						PickedQty: restQty
					));
			}
			availability[detailKey].Remove(available);
			return available;
		}

		protected virtual void HoldShipment(SOShipmentEntry shipmentEntry, string shipmentNbr)
		{
			shipmentEntry.Clear();
			shipmentEntry.Document.Current = shipmentEntry.Document.Search<SOShipment.shipmentNbr>(shipmentNbr);
			shipmentEntry.putOnHold.Press();
		}

		protected virtual void CreateTransferToStorageLocations(INTransferEntry transferEntry, int? siteID, IEnumerable<INTranSplit> tranSplits)
		{
			transferEntry.Clear();

			transferEntry.insetup.Current.RequireControlTotal = false;
			transferEntry.transfer.With(_ => _.Insert() ?? _.Insert());
			transferEntry.transfer.SetValueExt<INRegister.siteID>(transferEntry.transfer.Current, siteID);
			transferEntry.transfer.SetValueExt<INRegister.toSiteID>(transferEntry.transfer.Current, siteID);
			transferEntry.transfer.UpdateCurrent();
			foreach (var protoTranSplit in tranSplits)
			{
				if (InventoryItem.PK.Find(Base, protoTranSplit.InventoryID).StkItem != true)
					continue;

				INTran tran = transferEntry.transactions.With(_ => _.Insert() ?? _.Insert());
				tran.InventoryID = protoTranSplit.InventoryID;
				tran.SubItemID = protoTranSplit.SubItemID;
				tran.LotSerialNbr = protoTranSplit.LotSerialNbr;
				tran.ExpireDate = protoTranSplit.ExpireDate;
				tran.UOM = protoTranSplit.UOM;
				tran = transferEntry.transactions.Update(tran);
				tran.SiteID = protoTranSplit.SiteID;
				tran.LocationID = protoTranSplit.LocationID;
				tran.ToSiteID = protoTranSplit.SiteID;
				tran.ToLocationID = protoTranSplit.ToLocationID;
				tran = transferEntry.transactions.Update(tran);
				INTranSplit tranSplit = transferEntry.splits.Search<INTranSplit.lineNbr>(tran.LineNbr);
				if (tranSplit == null)
				{
					tranSplit = transferEntry.splits.With(_ => _.Insert() ?? _.Insert());
					tranSplit.LotSerialNbr = protoTranSplit.LotSerialNbr;
					tranSplit.ExpireDate = protoTranSplit.ExpireDate;
					tranSplit.ToSiteID = protoTranSplit.SiteID;
					tranSplit.ToLocationID = protoTranSplit.ToLocationID;
				}
				tranSplit.ShipmentNbr = protoTranSplit.ShipmentNbr;
				tranSplit.ShipmentLineNbr = protoTranSplit.ShipmentLineNbr;
				tranSplit.ShipmentLineSplitNbr = protoTranSplit.ShipmentLineSplitNbr;

				tranSplit.Qty = protoTranSplit.Qty;
				tranSplit = transferEntry.splits.Update(tranSplit);
			}
			transferEntry.transfer.SetValueExt<INRegister.hold>(transferEntry.transfer.Current, false);
			transferEntry.release.Press();
		}

		protected virtual void PutShipmentOnStorageLocations(SOShipmentEntry shipmentEntry, string shipmentNbr, IEnumerable<(SOShipLineSplit Split, int? SortLocationID)> soSplitToSortLocation)
		{
			shipmentEntry.Clear();

			var kitSpecHelper = new NonStockKitSpecHelper(shipmentEntry);

			shipmentEntry.Document.Current = shipmentEntry.Document.Search<SOShipment.shipmentNbr>(shipmentNbr);
			PXSelectBase<SOShipLine> shLines = shipmentEntry.Transactions;
			PXSelectBase<SOShipLineSplit> shSplits = shipmentEntry.splits;

			var RequireShipping = Func.Memorize((int inventoryID) => InventoryItem.PK.Find(shipmentEntry, inventoryID).With(item => item.StkItem == true || item.NonStockShip == true));

			decimal docQty = 0;
			foreach (var line in soSplitToSortLocation.GroupBy(d => d.Split.LineNbr))
			{
				shLines.Current = shLines.Search<SOShipLine.shipmentNbr, SOShipLine.lineNbr>(shipmentNbr, line.Key);

				decimal lineBaseQty = 0;
				foreach (var split in line)
				{
					shSplits.Current = shSplits.Search<SOShipLineSplit.shipmentNbr, SOShipLineSplit.lineNbr, SOShipLineSplit.splitLineNbr>(shipmentNbr, line.Key, split.Split.SplitLineNbr);
					shSplits.SetValueExt<SOShipLineSplit.locationID>(shSplits.Current, split.SortLocationID);
					shSplits.SetValueExt<SOShipLineSplit.pickedQty>(shSplits.Current, split.Split.Qty);
					shSplits.SetValueExt<SOShipLineSplit.basePickedQty>(shSplits.Current, split.Split.BaseQty);
					shSplits.UpdateCurrent();

					lineBaseQty += shSplits.Current.BaseQty.Value;
				}
				if (kitSpecHelper.IsNonStockKit(shLines.Current.InventoryID))
				{
					// kitInventoryID -> compInventory -> qty
					var nonStockKitSpec = kitSpecHelper.GetNonStockKitSpec(shLines.Current.InventoryID.Value).Where(pair => RequireShipping(pair.Key)).ToDictionary();
					var nonStockKitSplits = shSplits.SelectMain().GroupBy(r => r.InventoryID.Value).ToDictionary(g => g.Key, g => g.Sum(s => s.PickedQty ?? 0));

					decimal integerKitQty = nonStockKitSpec.Keys.Count() == 0 || nonStockKitSpec.Keys.Except(nonStockKitSplits.Keys).Count() > 0
						? 0
						: (from split in nonStockKitSplits
						   join spec in nonStockKitSpec on split.Key equals spec.Key
						   select Math.Floor(decimal.Divide(split.Value, spec.Value))).Min();

					lineBaseQty = INUnitAttribute.ConvertToBase(shLines.Cache, shLines.Current.InventoryID, shLines.Current.UOM, integerKitQty, INPrecision.NOROUND);
				}

				shLines.Current.BasePickedQty = lineBaseQty;
				shLines.Current.PickedQty = INUnitAttribute.ConvertFromBase(shLines.Cache, shLines.Current.InventoryID, shLines.Current.UOM, lineBaseQty, INPrecision.NOROUND);
				shLines.Cache.MarkUpdated(shLines.Current, assertError: true);
				docQty += shLines.Current.PickedQty.Value;
			}
			shipmentEntry.Document.SetValueExt<SOShipment.picked>(shipmentEntry.Document.Current, true);
			shipmentEntry.Document.SetValueExt<SOShipment.pickedQty>(shipmentEntry.Document.Current, docQty);
			shipmentEntry.Document.SetValueExt<SOShipment.pickedViaWorksheet>(shipmentEntry.Document.Current, true);
			shipmentEntry.Document.UpdateCurrent();
			shipmentEntry.releaseFromHold.Press();
		}
		#endregion

		#region Filfill Grouped
		protected virtual IEnumerable<string> FulfillShipmentsSingle(SOPickingWorksheet worksheet)
			=> FulfillShipmentsGrouped(worksheet);

		protected virtual IEnumerable<string> FulfillShipmentsWave(SOPickingWorksheet worksheet)
			=> FulfillShipmentsGrouped(worksheet);

		protected virtual IEnumerable<string> FulfillShipmentsGrouped(SOPickingWorksheet worksheet)
		{
			Base.Clear();

			Base.worksheet.Current = SOPickingWorksheet.PK.Find(Base, worksheet.WorksheetNbr, PKFindOptions.IncludeDirty).With(ValidateMutability);

			var recentlyPickedShipments =
				SelectFrom<SOShipment>.
				InnerJoin<SOPickerToShipmentLink>.On<SOPickerToShipmentLink.FK.Shipment>.
				InnerJoin<SOPicker>.On<SOPickerToShipmentLink.FK.Picker>.
				Where<SOPicker.confirmed.IsEqual<True>.
					And<SOPicker.FK.Worksheet.SameAsCurrent>>.
				View.Select(Base)
				.AsEnumerable()
				.Cast<PXResult<SOShipment, SOPickerToShipmentLink, SOPicker>>()
				.Distinct(row => row.GetItem<SOShipment>().ShipmentNbr)
				.GroupBy(
					row => (SOPicker)row,
					row => (SOShipment)row,
					Base.pickers.Cache.GetComparer())
				.Where(g => g.All(sh => sh.Picked == false))
				.SelectMany(g => g)
				.ToArray();

			var shipmentGraph = Lazy.By(() => PXGraph.CreateInstance<SOShipmentEntry>());
			var fulfilledShipments = new List<string>();
			using (var ts = new PXTransactionScope())
			{
				foreach (var shipment in recentlyPickedShipments)
				{
					MarkShipmentPicked(shipmentGraph.Value, shipment.ShipmentNbr);
					fulfilledShipments.Add(shipment.ShipmentNbr);
				}
				ts.Complete();
			}

			return fulfilledShipments;
		}

		protected virtual void MarkShipmentPicked(SOShipmentEntry shipmentEntry, string shipmentNbr)
		{
			shipmentEntry.Clear();

			var kitSpecHelper = new NonStockKitSpecHelper(shipmentEntry);

			var picker =
				SelectFrom<SOPicker>.
				InnerJoin<SOPickerToShipmentLink>.On<SOPickerToShipmentLink.FK.Picker>.
				Where<
					SOPicker.worksheetNbr.IsEqual<@P.AsString>.
					And<SOPickerToShipmentLink.shipmentNbr.IsEqual<@P.AsString>>>.
				View.Select(shipmentEntry, Base.worksheet.Current.WorksheetNbr, shipmentNbr).TopFirst;

			var pickerSplits =
				SelectFrom<SOPickerListEntry>.
				LeftJoin<SOPickListEntryToCartSplitLink>.On<SOPickListEntryToCartSplitLink.FK.PickListEntry>.
				Where<
					SOPickerListEntry.worksheetNbr.IsEqual<@P.AsString>.
					And<SOPickerListEntry.pickerNbr.IsEqual<@P.AsInt>>.
					And<SOPickerListEntry.shipmentNbr.IsEqual<@P.AsString>>>.
				View
				.Select(shipmentEntry, picker.WorksheetNbr, picker.PickerNbr, shipmentNbr)
				.AsEnumerable()
				.Select(r =>
				(
					Entry: r.GetItem<SOPickerListEntry>(),
					Link: r.GetItem<SOPickListEntryToCartSplitLink>()
				))
				.ToArray();

			var availability = pickerSplits
				.GroupBy(e =>
				(
					InventoryID: e.Entry.InventoryID,
					SubItemID: e.Entry.SubItemID,
					OrderLineUOM: e.Entry.OrderLineUOM,
					LocationID: e.Entry.LocationID,
					LotSerialNbr: e.Entry.LotSerialNbr
				))
				.Select(g =>
				(
					Key: g.Key,
					PickedQty: g.Sum(e => e.Entry.PickedQty ?? 0),
					Links: g.Select(e => e.Link).WhereNotNull().ToList().AsReadOnly(),
					ExpiredDate: isEnterableDate(g.Key.InventoryID) ? g.Select(x => x.Entry.ExpireDate).Where(x => x != null).FirstOrDefault() : null
				))
				.ToDictionary(r => r.Key, r => (r.PickedQty, r.Links, r.ExpiredDate));

			shipmentEntry.Document.Current = shipmentEntry.Document.Search<SOShipment.shipmentNbr>(shipmentNbr);
			PXSelectBase<SOShipLine> shLines = shipmentEntry.Transactions;
			PXSelectBase<SOShipLineSplit> shSplits = shipmentEntry.splits;
			PXSelectBase<Unassigned.SOShipLineSplit> shSplitsUnassigned = shipmentEntry.unassignedSplits;

			var RequireShipping = Func.Memorize((int inventoryID) => InventoryItem.PK.Find(shipmentEntry, inventoryID).With(item => item.StkItem == true || item.NonStockShip == true));

			decimal docQty = 0;
			foreach (SOShipLine line in shLines.Select())
			{
				shLines.Current = shLines.Search<SOShipLine.shipmentNbr, SOShipLine.lineNbr>(shipmentNbr, line.LineNbr);

				decimal lineBaseQty = 0;
				foreach (SOShipLineSplit split in shSplits.Select())
				{
					shSplits.Current = shSplits.Search<SOShipLineSplit.shipmentNbr, SOShipLineSplit.lineNbr, SOShipLineSplit.splitLineNbr>(shipmentNbr, line.LineNbr, split.SplitLineNbr);

					var splitKey =
					(
						InventoryID: shSplits.Current.InventoryID,
						SubItemID: shSplits.Current.SubItemID,
						OrderLineUOM: shLines.Current.UOM,
						LocationID: shSplits.Current.LocationID,
						LotSerialNbr: shSplits.Current.LotSerialNbr
					);

					if (isEnterableOnIssue(shSplits.Current.InventoryID))
					{
						// recreate splits that might be picked with other Lot/Serial Nbrs
						var deletedSplit = shSplits.DeleteCurrent();
						lineBaseQty += CreateAssignedSplits(deletedSplit);
					}
					else if (availability.ContainsKey(splitKey))
					{
						decimal newQty = Math.Min(availability[splitKey].PickedQty, shSplits.Current.Qty.Value);

						if (shSplits.Current.PickedQty != newQty)
						{
							shSplits.SetValueExt<SOShipLineSplit.pickedQty>(shSplits.Current, newQty);
							shSplits.UpdateCurrent();
						}

						lineBaseQty += newQty;
						if (newQty > 0)
						{
							availability[splitKey] = (availability[splitKey].PickedQty - newQty, availability[splitKey].Links, availability[splitKey].ExpiredDate);
							shipmentEntry.CartSupportExt?.TransformCartLinks(shSplits.Current, availability[splitKey].Links);
						}
					}
				}

				foreach (Unassigned.SOShipLineSplit usplit in shSplitsUnassigned.Select())
				{
					lineBaseQty += CreateAssignedSplits(PropertyTransfer.Transfer(usplit, new SOShipLineSplit()));
				}

				decimal CreateAssignedSplits(SOShipLineSplit protoSplit)
				{
					var usplitKey =
					(
						InventoryID: protoSplit.InventoryID,
						SubItemID: protoSplit.SubItemID,
						OrderLineUOM: shLines.Current.UOM,
						LocationID: protoSplit.LocationID
					);

					decimal addLineBaseQty = 0;
					decimal restQty = protoSplit.Qty.Value;
					foreach (var item in
						availability
						.Where(t => usplitKey.Equals((t.Key.InventoryID, t.Key.SubItemID, t.Key.OrderLineUOM, t.Key.LocationID)) && t.Value.PickedQty > 0)
						.OrderByAccordanceTo(t => t.Key.LotSerialNbr == protoSplit.LotSerialNbr)
						.ToArray())
					{
						if (restQty == 0) break;

						decimal newQty = Math.Min(item.Value.PickedQty, restQty);

						if (newQty > 0)
						{
							var newSplit = PropertyTransfer.Transfer(protoSplit, new SOShipLineSplit());
							newSplit.SplitLineNbr = null;
							newSplit.LotSerialNbr = item.Key.LotSerialNbr;
							if (item.Value.ExpiredDate != null)
								newSplit.ExpireDate = item.Value.ExpiredDate;
							newSplit.Qty = newQty;
							newSplit.PickedQty = newQty;
							newSplit.PackedQty = 0;
							newSplit.IsUnassigned = false;
							newSplit.HasGeneratedLotSerialNbr = false;
							newSplit.PlanID = null;

							newSplit = shipmentEntry.splits.Insert(newSplit);

							addLineBaseQty += newQty;
							availability[item.Key] = (availability[item.Key].PickedQty - newQty, availability[item.Key].Links, availability[item.Key].ExpiredDate);
							restQty -= newQty;

							shipmentEntry.CartSupportExt?.TransformCartLinks(newSplit, availability[item.Key].Links);
						}
					}

					return addLineBaseQty;
				}

				if (kitSpecHelper.IsNonStockKit(line.InventoryID))
				{
					// kitInventoryID -> compInventory -> qty
					var nonStockKitSpec = kitSpecHelper.GetNonStockKitSpec(line.InventoryID.Value).Where(pair => RequireShipping(pair.Key)).ToDictionary();
					var nonStockKitSplits = shSplits.SelectMain().GroupBy(r => r.InventoryID.Value).ToDictionary(g => g.Key, g => g.Sum(s => s.PickedQty ?? 0));

					decimal integerKitQty = nonStockKitSpec.Keys.Count() == 0 || nonStockKitSpec.Keys.Except(nonStockKitSplits.Keys).Count() > 0
						? 0
						: (from split in nonStockKitSplits
						   join spec in nonStockKitSpec on split.Key equals spec.Key
						   select Math.Floor(decimal.Divide(split.Value, spec.Value))).Min();

					lineBaseQty = INUnitAttribute.ConvertToBase(shLines.Cache, shLines.Current.InventoryID, shLines.Current.UOM, integerKitQty, INPrecision.NOROUND);
				}

				if (shLines.Current.BasePickedQty != lineBaseQty)
				{
					shLines.Current.BasePickedQty = lineBaseQty;
					shLines.Current.PickedQty = INUnitAttribute.ConvertFromBase(shLines.Cache, shLines.Current.InventoryID, shLines.Current.UOM, lineBaseQty, INPrecision.NOROUND);
					shLines.Cache.MarkUpdated(shLines.Current, assertError: true);
				}

				docQty += shLines.Current.PickedQty.Value;
			}

			if (docQty > 0)
			{
				shipmentEntry.Document.SetValueExt<SOShipment.picked>(shipmentEntry.Document.Current, true);
				shipmentEntry.Document.SetValueExt<SOShipment.pickedQty>(shipmentEntry.Document.Current, docQty);
				shipmentEntry.Document.SetValueExt<SOShipment.pickedViaWorksheet>(shipmentEntry.Document.Current, true);
				shipmentEntry.Document.UpdateCurrent();

				if (shipmentEntry.Document.Current.Hold == true)
					shipmentEntry.releaseFromHold.Press();
				else
					shipmentEntry.Save.Press();
			}
			else
				shipmentEntry.Save.Press();
		}
		#endregion
		#endregion

		public virtual bool TryMarkWorksheetPicked(SOPickingWorksheet worksheet)
		{
			if (worksheet == null)
				throw new PXArgumentException(nameof(worksheet), ErrorMessages.ArgumentNullException);

			Base.Clear();

			Base.worksheet.Current = SOPickingWorksheet.PK.Find(Base, worksheet.WorksheetNbr, PKFindOptions.IncludeDirty).With(ValidateMutability);

			if (Base.pickers.SelectMain().All(p => p.Confirmed == true))
			{
				foreach (SOPickingWorksheetLine wsLine in Base.worksheetLines.Select())
				{
					if (wsLine.Qty == 0m)
					{
						Base.worksheetLines.Delete(wsLine);
					}
					else
					{
						Base.worksheetLines.Current = wsLine;

						var usedLotSerialNbrs = new HashSet<string>();
						foreach (SOPickingWorksheetLineSplit wsSplit in Base.worksheetLineSplits.Select())
						{
							if (wsSplit.Qty == 0m)
								Base.worksheetLineSplits.Delete(wsSplit);
							else if (!string.IsNullOrEmpty(wsSplit.LotSerialNbr))
								usedLotSerialNbrs.Add(wsSplit.LotSerialNbr);
						}

						if (usedLotSerialNbrs.Count == 1 && usedLotSerialNbrs.First() is string singleLotSerial && !string.Equals(wsLine.LotSerialNbr, singleLotSerial, StringComparison.OrdinalIgnoreCase))
						{
							wsLine.LotSerialNbr = singleLotSerial;
							Base.worksheetLines.Update(wsLine);
						}
						else if (wsLine.LotSerialNbr != null)
						{
							wsLine.LotSerialNbr = null;
							Base.worksheetLines.Update(wsLine);
						}
					}
				}

				Base.worksheet.Current.Status = SOPickingWorksheet.status.Picked;
				Base.worksheet.UpdateCurrent();

				Base.Save.Press();
				return true;
			}
			return false;
		}


		protected virtual SOPickingWorksheet ValidateMutability(SOPickingWorksheet worksheet)
		{
			if (worksheet.Status.IsIn(SOPickingWorksheet.status.Hold, SOPickingWorksheet.status.Completed, SOPickingWorksheet.status.Cancelled))
				throw new PXInvalidOperationException(Msg.WorksheetCannotBeUpdatedInCurrentStatus, worksheet.WorksheetNbr, Base.worksheet.Cache.GetStateExt<SOPickingWorksheet.status>(worksheet));
			return worksheet;
		}

		protected virtual SOPicker ValidateMutability(SOPicker pickList)
		{
			if (pickList.Confirmed == true)
				throw new PXInvalidOperationException(Msg.PickListIsAlreadyConfirmed, pickList.PickListNbr);
			return pickList;
		}


		public virtual async Task FulfillShipmentsAndConfirmWorksheet(SOPickingWorksheet worksheet, CancellationToken cancellationToken)
		{
			PXReportRequiredException report = null;
			using (var ts = new PXTransactionScope())
			{
				var fulfilledShipments = TryFulfillShipments(worksheet);
				if (fulfilledShipments.Any())
				{
					if (worksheet.WorksheetType == SOPickingWorksheet.worksheetType.Batch && PXAccess.FeatureInstalled<CS.FeaturesSet.deviceHub>())
					{
						foreach (var shipmentNbr in fulfilledShipments)
						{
							var packSlipParameters = new Dictionary<string, string>
							{
								[nameof(SOPickingWorksheet.WorksheetNbr)] = worksheet.WorksheetNbr,
								[nameof(SOShipment.ShipmentNbr)] = shipmentNbr
							};
							if (report == null)
								report = new PXReportRequiredException(packSlipParameters, SOReports.PrintPackSlipBatch);
							else
								report.AddSibling(SOReports.PrintPackSlipBatch, packSlipParameters);
						}

						if (report != null)
							await PX.SM.SMPrintJobMaint.CreatePrintJobGroup(
								new PX.SM.PrintSettings
								{
									PrintWithDeviceHub = true,
									DefinePrinterManually = true,
									PrinterID = new CR.NotificationUtility(Base).SearchPrinter(SONotificationSource.Customer, SOReports.PrintPackSlipBatch, Base.Accessinfo.BranchID),
									NumberOfCopies = 1
								}, report, null, cancellationToken);
					}
				}

				TryMarkWorksheetPicked(worksheet);

				ts.Complete();
			}
			if (report != null)
				throw report;
		}

		// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
		public class ShipmentWorkLog : GraphExtensions.ShipmentWorkLog<SOPickingWorksheetReview> { }

		[PXLocalizable]
		public static class Msg
		{
			public const string PickListIsAlreadyConfirmed = "The {0} pick list is already confirmed.";
			public const string WorksheetCannotBeUpdatedInCurrentStatus = "The {0} picking worksheet cannot be modified because it has the {1} status.";

			[Obsolete]
			public const string PickListConfirmedButShipmentDoesNot = WMS.WorksheetPicking.Msg.PickListConfirmedButShipmentDoesNot;
		}

		// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
		public class WorkflowChanges : PXGraphExtension<SOPickingWorksheetReview.Workflow, SOPickingWorksheetReview>
		{
			public sealed override void Configure(PXScreenConfiguration config) => Configure(config.GetScreenConfigurationContext<SOPickingWorksheetReview, SOPickingWorksheet>());
			protected static void Configure(WorkflowContext<SOPickingWorksheetReview, SOPickingWorksheet> context)
			{
				context.UpdateScreenConfigurationFor(screen => screen
					.WithActions(actions => actions
						.Add<SOPickingWorksheetPickListConfirmation>(e => e.FulfillShipments, a => a
							.WithCategory(CommonActionCategories.Get(context).Processing)
							.PlaceAfterInCategory(g => g.PrintPackSlips))));
			}
		}
	}


	public static class PickListActionCategory
	{
		public const string ID = "Pick List";

		[PXLocalizable]
		public static class DisplayNames
		{
			public const string Value = "Pick List";
		}

		public static BoundedTo<SOShipmentEntry, SOShipment>.ActionCategory.IConfigured Get(WorkflowContext<SOShipmentEntry, SOShipment> context)
		{
			return
				context.Categories.Get(ID) ??
				context.Categories
					.CreateNew(ID, category => category
						.DisplayName(DisplayNames.Value)
						.PlaceAfter(Common.CommonActionCategories.Get(context).Processing))
					.Apply(category =>
						context.UpdateScreenConfigurationFor(screen =>
							screen.WithCategories(categories => categories.Add(category))));
		}
	}

	public class SOShipmentEntryUnlinkWorksheetExt : PXGraphExtension<SOShipmentEntry>
	{
		public static bool IsActive() => PXAccess.FeatureInstalled<CS.FeaturesSet.wMSPaperlessPicking>() || PXAccess.FeatureInstalled<CS.FeaturesSet.wMSAdvancedPicking>();

		[PXButton(CommitChanges = true), PXUIField(DisplayName = "Remove from Worksheet")]
		protected virtual IEnumerable unlinkFromWorksheet(PXAdapter adapter)
		{
			Base.Save.Press();
			string shipmentNbr = Base.Document.Current.ShipmentNbr;
			PXLongOperation.StartOperation(Base, () => 
				PXGraph.CreateInstance<SOShipmentEntry>().FindImplementation<SOShipmentEntryUnlinkWorksheetExt>().Unlink(shipmentNbr));
			return adapter.Get();
		}
		public PXAction<SOShipment> UnlinkFromWorksheet;

		public virtual void Unlink(string shipmentNbr)
		{
			Base.Document.Current = Base.Document.Search<SOShipment.shipmentNbr>(shipmentNbr);
			using (var ts = new PXTransactionScope())
			{
				ClearPickingData(Base.Document.Current);
				UnlinkShipment(Base.Document.Current);
				ts.Complete();
			}
		}

		protected virtual void ClearPickingData(SOShipment shipment)
		{
			if (shipment?.PickedQty > 0)
			{
				Base.Document.Current = Base.Document.Search<SOShipment.shipmentNbr>(shipment.ShipmentNbr);
				Base.CartSupportExt?.RemoveItemsFromCart();

				PXSelectBase<SOShipLine> shLines = Base.Transactions;
				PXSelectBase<SOShipLineSplit> shSplits = Base.splits;

				foreach (SOShipLine line in shLines.Select())
				{
					shLines.Current = shLines.Search<SOShipLine.shipmentNbr, SOShipLine.lineNbr>(Base.Document.Current.ShipmentNbr, line.LineNbr);

					foreach (SOShipLineSplit split in shSplits.Select())
					{
						shSplits.Current = shSplits.Search<
						  SOShipLineSplit.shipmentNbr, SOShipLineSplit.lineNbr, SOShipLineSplit.splitLineNbr>(
						  Base.Document.Current.ShipmentNbr, line.LineNbr, split.SplitLineNbr);

						shSplits.SetValueExt<SOShipLineSplit.pickedQty>(shSplits.Current, 0m);
						shSplits.SetValueExt<SOShipLineSplit.basePickedQty>(shSplits.Current, 0m);
						shSplits.UpdateCurrent();
					}
					shLines.Current.BasePickedQty = 0;
					shLines.Current.PickedQty = 0;
					shLines.Cache.MarkUpdated(shLines.Current, assertError: true);
				}

				Base.Document.SetValueExt<SOShipment.picked>(Base.Document.Current, false);
				Base.Document.SetValueExt<SOShipment.pickedQty>(Base.Document.Current, 0m);
				Base.Document.SetValueExt<SOShipment.pickedViaWorksheet>(Base.Document.Current, false);
				Base.Document.UpdateCurrent();
				Base.Save.Press();
			}
		}

		protected virtual void UnlinkShipment(SOShipment shipment)
		{
			if (shipment?.CurrentWorksheetNbr != null)
			{
				var worksheetEntry = PXGraph.CreateInstance<SOPickingWorksheetReview>();
				worksheetEntry.SelectTimeStamp();

				var worksheet = worksheetEntry.worksheet.Current = SOPickingWorksheet.PK.Find(worksheetEntry, shipment.CurrentWorksheetNbr, PKFindOptions.IncludeDirty);
				if (worksheet.WorksheetType == SOPickingWorksheet.worksheetType.Single)
				{
					worksheetEntry.worksheet.Delete(worksheet);
				}
				else
				{
					if (worksheet.WorksheetType == SOPickingWorksheet.worksheetType.Wave)
					{
						var pickerShipmentLink =
							SelectFrom<SOPickerToShipmentLink>.
							Where<
								SOPickerToShipmentLink.worksheetNbr.IsEqual<@P.AsString>.
								And<SOPickerToShipmentLink.shipmentNbr.IsEqual<@P.AsString>>>.
							View.Select(worksheetEntry, worksheet.WorksheetNbr, shipment.ShipmentNbr).TopFirst;

						if (pickerShipmentLink != null)
						{
							worksheetEntry.pickerShipments.Delete(pickerShipmentLink);
							worksheetEntry.EnsureCachePersistence<SOPickerToShipmentLink>();
						}
					}

					worksheetEntry.shipments.Current = worksheetEntry.shipments.Search<SOShipment.shipmentNbr>(Base.Document.Current.ShipmentNbr);
					worksheetEntry.shipments.SetValueExt<SOShipment.currentWorksheetNbr>(worksheetEntry.shipments.Current, null);
					worksheetEntry.shipments.UpdateCurrent();

					Base.TryCompleteWorksheet(worksheetEntry, worksheet);
				}

				worksheetEntry.Save.Press();
			}
		}

		public virtual bool CanUnlinkWorksheetFrom(SOShipment shipment)
		{
			bool unlinkable = shipment != null && shipment.Confirmed == false && shipment.CurrentWorksheetNbr != null;
			if (unlinkable)
			{
				var worksheet = SOPickingWorksheet.PK.Find(Base, shipment.CurrentWorksheetNbr);
				unlinkable &=
					worksheet.Status.IsIn(SOPickingWorksheet.status.Picked, SOPickingWorksheet.status.Completed, SOPickingWorksheet.status.Cancelled) ||
					(worksheet.WorksheetType == SOPickingWorksheet.worksheetType.Single && worksheet.Status != SOPickingWorksheet.status.Picking);
				unlinkable &=
					(worksheet.WorksheetType == SOPickingWorksheet.worksheetType.Single && SOPickPackShipSetup.PK.Find(Base, Base.Accessinfo.BranchID)?.IsPackOnly == true) ||
					SelectFrom<SOShipLineSplit>.
					Where<
						SOShipLineSplit.shipmentNbr.IsEqual<@P.AsString>.
						And<SOShipLineSplit.packedQty.IsNotEqual<CS.decimal0>>>.
					View.Select(Base, shipment.ShipmentNbr).AsEnumerable().Any() == false;
			}
			return unlinkable;
		}

		protected virtual void _(Events.RowSelected<SOShipment> e)
		{
			bool isWaveBatch = e.Row
				.With(sh => SOPickingWorksheet.PK.Find(Base, sh.CurrentWorksheetNbr))
				.With(ws => ws.WorksheetType)
				.IsIn(SOPickingWorksheet.worksheetType.Wave, SOPickingWorksheet.worksheetType.Batch);

			UnlinkFromWorksheet.SetEnabled(isWaveBatch && CanUnlinkWorksheetFrom(e.Row));
			UnlinkFromWorksheet.SetVisible(PXAccess.FeatureInstalled<CS.FeaturesSet.wMSAdvancedPicking>());
		}

		/// Overrides <see cref="SOShipmentEntry.CorrectShipment(SOOrderEntry, SOShipment)"/>
		[PXOverride]
		public virtual void CorrectShipment(SOOrderEntry docgraph, SOShipment shiporder, Action<SOOrderEntry, SOShipment> base_CorrectShipment)
		{
			if (!string.IsNullOrEmpty(shiporder?.CurrentWorksheetNbr))
				Unlink(shiporder.ShipmentNbr);

			base_CorrectShipment(docgraph, shiporder);
		}

		public class WorkflowChanges : PXGraphExtension<SOShipmentEntry_Workflow, SOShipmentEntry>
		{
			public static bool IsActive() => PXAccess.FeatureInstalled<CS.FeaturesSet.wMSAdvancedPicking>();

			public sealed override void Configure(PXScreenConfiguration config) =>
				Configure(config.GetScreenConfigurationContext<SOShipmentEntry, SOShipment>());

			protected static void Configure(WorkflowContext<SOShipmentEntry, SOShipment> context)
			{
				if (IsActive())
				{
					var pickListCategory = PickListActionCategory.Get(context);
					context.UpdateScreenConfigurationFor(screen =>
						screen.WithActions(actions =>
							actions.Add<SOShipmentEntryUnlinkWorksheetExt>(g => g.UnlinkFromWorksheet, a => a
								.WithCategory(pickListCategory))));
				}
			}
		}
	}

	public class SOShipmentEntryShowPickListPopup : GraphExtensions.
		ShowPickListPopup.
		On<SOShipmentEntry, SOShipment>.
		FilteredBy<Where<
			SOPickingWorksheet.worksheetType.IsEqual<SOPickingWorksheet.worksheetType.single>.
			And<SOPickingWorksheet.worksheetNbr.IsEqual<SOShipment.currentWorksheetNbr.FromCurrent>>>>
	{
		public static bool IsActive() => PXAccess.FeatureInstalled<CS.FeaturesSet.wMSPaperlessPicking>();
		protected SOShipmentEntryUnlinkWorksheetExt Unlinker => Base.FindImplementation<SOShipmentEntryUnlinkWorksheetExt>();

		protected override bool IsPickListExternalViewMode => false;

		protected override bool CanDeletePickList(SOShipment primaryRow) => base.CanDeletePickList(primaryRow) && Unlinker.CanUnlinkWorksheetFrom(primaryRow);

		protected override void PerformPickListDeletion()
		{
			Base.Save.Press();
			string shipmentNbr = Base.Document.Current.ShipmentNbr;
			PXLongOperation.StartOperation(Base, () =>
				PXGraph.CreateInstance<SOShipmentEntry>().FindImplementation<SOShipmentEntryUnlinkWorksheetExt>().Unlink(shipmentNbr));
		}

		public class WorkflowChanges : PXGraphExtension<SOShipmentEntry_Workflow, SOShipmentEntry>
		{
			public static bool IsActive() => SOShipmentEntryShowPickListPopup.IsActive();

			public sealed override void Configure(PXScreenConfiguration config) =>
				Configure(config.GetScreenConfigurationContext<SOShipmentEntry, SOShipment>());

			protected static void Configure(WorkflowContext<SOShipmentEntry, SOShipment> context)
			{
				if (IsActive())
				{
					var pickListCategory = PickListActionCategory.Get(context);
					context.UpdateScreenConfigurationFor(screen =>
						screen.WithActions(actions =>
							actions.Add<SOShipmentEntryShowPickListPopup>(ex => ex.ShowPickList, a => a
								.WithCategory(pickListCategory))));
				}
			}

			public class SortExtensions : SortExtensionsBy<ExtensionOrderFor<SOShipmentEntry>.FilledWith<
				SOShipmentEntryShowPickListPopup.WorkflowChanges,
				SOShipmentEntryUnlinkWorksheetExt.WorkflowChanges>>
			{ }
		}
	}
}
