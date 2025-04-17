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
using PX.Objects.AR;
using PX.Objects.CS;
using PX.Objects.IN;
using PX.SM;

namespace PX.Objects.SO
{
	public class SOPickingJobProcess : PXGraph<SOPickingJobProcess>, PXImportAttribute.IPXPrepareItems, PXImportAttribute.IPXProcess
	{
		public class ProcessAction
		{
			public const string None = "N";
			public const string Send = "S";
			public const string Remove = "R";
			public const string Priority = "P";
			public const string Assign = "A";

			public class none : BqlString.Constant<none> { public none() : base(None) { } }
			public class send : BqlString.Constant<send> { public send() : base(Send) { } }
			public class remove : BqlString.Constant<remove> { public remove() : base(Remove) { } }
			public class priority : BqlString.Constant<priority> { public priority() : base(Priority) { } }
			public class assign : BqlString.Constant<assign> { public assign() : base(Assign) { } }

			[PXLocalizable]
			public abstract class DisplayNames
			{
				public const string None = "<SELECT>";
				public const string Send = "Send to Picking Queue";
				public const string Remove = "Remove from Picking Queue";
				public const string Priority = "Change Picking Priority";
				public const string Assign = "Assign Pick Lists";
			}

			public class ListAttribute : PXStringListAttribute
			{
				public ListAttribute() : base
				(
					Pair(None, DisplayNames.None),
					Pair(Send, DisplayNames.Send),
					Pair(Priority, DisplayNames.Priority),
					Pair(Assign, DisplayNames.Assign),
					Pair(Remove, DisplayNames.Remove)
				) { }
			}
		}

		#region DACs
		[PXCacheName(CacheNames.Filter)]
		public class HeaderFilter : IBqlTable
		{
			#region Action
			[PXString(15, IsUnicode = true)]
			[ProcessAction.List]
			[PXUnboundDefault(ProcessAction.None)]
			[PXUIField(DisplayName = "Action", Required = true)]
			public virtual String Action { get; set; }
			public abstract class action : BqlString.Field<action> { }
			#endregion
			#region WorksheetType
			[PXString(2, IsFixed = true)]
			[worksheetType.List]
			[PXUnboundDefault(typeof(worksheetType.all.When<Where<FeatureInstalled<FeaturesSet.wMSAdvancedPicking>>>.Else<SOPickingWorksheet.worksheetType.single>))]
			[PXUIField(DisplayName = "Pick List Type", FieldClass = nameof(FeaturesSet.WMSAdvancedPicking))]
			public virtual String WorksheetType { get; set; }
			public abstract class worksheetType : BqlString.Field<worksheetType>
			{
				public const string All = "AL";
				public class all : BqlString.Constant<all> { public all() : base(All) { } }

				[PX.Common.PXLocalizable]
				public static class DisplayNames
				{
					public const string All = "All";
				}

				public class ListAttribute : PXStringListAttribute
				{
					public ListAttribute() : base
					(
						Pair(All, DisplayNames.All),
						Pair(SOPickingWorksheet.worksheetType.Single, SOPickingWorksheet.worksheetType.DisplayNames.Single),
						Pair(SOPickingWorksheet.worksheetType.Wave, SOPickingWorksheet.worksheetType.DisplayNames.Wave),
						Pair(SOPickingWorksheet.worksheetType.Batch, SOPickingWorksheet.worksheetType.DisplayNames.Batch)
					) { }
				}
			}
			#endregion
			#region Priority
			[PXInt]
			[priority.List]
			[PXUnboundDefault(priority.All)]
			[PXUIField(DisplayName = "Priority")]
			public virtual int? Priority { get; set; }
			public abstract class priority : BqlInt.Field<priority>
			{
				public const int All = -1;
				public class all : BqlInt.Constant<all> { public all() : base(All) { } }

				[PX.Common.PXLocalizable]
				public static class DisplayNames
				{
					public const string All = "All";
				}

				public class ListAttribute : PXIntListAttribute
				{
					public ListAttribute() : base
					(
						Pair(All, DisplayNames.All),
						Pair(WMSJob.priority.Urgent, WMSJob.priority.DisplayNames.Urgent),
						Pair(WMSJob.priority.High, WMSJob.priority.DisplayNames.High),
						Pair(WMSJob.priority.Medium, WMSJob.priority.DisplayNames.Medium),
						Pair(WMSJob.priority.Low, WMSJob.priority.DisplayNames.Low)
					) { }
				}
			}
			#endregion
			#region EndDate
			[PXDate]
			[PXUnboundDefault(typeof(AccessInfo.businessDate))]
			[PXUIField(DisplayName = "End Date")]
			public virtual DateTime? EndDate { get; set; }
			public abstract class endDate : BqlDateTime.Field<endDate> { }
			#endregion

			#region SiteID
			[Site(Required = true)]
			[InterBranchRestrictor(typeof(Where<SameOrganizationBranch<INSite.branchID, Current<AccessInfo.branchID>>>))]
			public virtual Int32? SiteID { get; set; }
			public abstract class siteID : BqlInt.Field<siteID> { }
			#endregion
			#region CustomerID
			[Customer]
			[PXUIVisible(typeof(worksheetType.IsEqual<SOPickingWorksheet.worksheetType.single>))]
			public virtual Int32? CustomerID { get; set; }
			public abstract class customerID : BqlInt.Field<customerID> { }
			#endregion
			#region CarrierPluginID
			[PXDBString(15, IsUnicode = true, InputMask = ">aaaaaaaaaaaaaaa")]
			[PXSelector(typeof(Search<CarrierPlugin.carrierPluginID>))]
			[PXUIField(DisplayName = "Carrier")]
			[PXUIVisible(typeof(worksheetType.IsEqual<SOPickingWorksheet.worksheetType.single>))]
			public virtual String CarrierPluginID { get; set; }
			public abstract class carrierPluginID : BqlString.Field<carrierPluginID> { }
			#endregion
			#region ShipVia
			[PXString(15, IsUnicode = true)]
			[PXSelector(typeof(Search<Carrier.carrierID, Where<carrierPluginID.FromCurrent.IsNull.Or<Carrier.carrierPluginID.IsEqual<carrierPluginID.FromCurrent>>>>), DescriptionField = typeof(Carrier.description), CacheGlobal = true)]
			[PXUIField(DisplayName = "Ship Via")]
			[PXUIVisible(typeof(worksheetType.IsEqual<SOPickingWorksheet.worksheetType.single>))]
			public virtual String ShipVia { get; set; }
			public abstract class shipVia : BqlString.Field<shipVia> { }
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
			#region MaxNumberOfLinesInPickList
			[PXInt]
			[PXUIField(DisplayName = "Max. Number of Lines in Pick List")]
			public int? MaxNumberOfLinesInPickList { get; set; }
			public abstract class maxNumberOfLinesInPickList : BqlInt.Field<maxNumberOfLinesInPickList> { }
			#endregion
			#region MaxQtyInLines
			[PXInt]
			[PXUIField(DisplayName = "Max. Quantity in Lines")]
			public int? MaxQtyInLines { get; set; }
			public abstract class maxQtyInLines : BqlInt.Field<maxQtyInLines> { }
			#endregion
		}

		// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
		public sealed class HeaderSettings : PXCacheExtension<HeaderFilter>
		{
			#region NewPriority
			[PXInt]
			[WMSJob.priority.List]
			[PXUnboundDefault(WMSJob.priority.Medium)]
			[PXUIField(DisplayName = "Set Picking Priority to")]
			[PXUIVisible(typeof(HeaderFilter.action.IsEqual<ProcessAction.priority>))]
			public int? NewPriority { get; set; }
			public abstract class newPriority : BqlInt.Field<newPriority> { }
			#endregion
			#region AssigneeID
			[PXGuid]
			[PXSelector(typeof(Search<Users.pKID, Where<Users.isHidden.IsEqual<False>>>), SubstituteKey = typeof(Users.username))]
			[PXUIField(DisplayName = "Assign to Picker")]
			[PXUIVisible(typeof(HeaderFilter.action.IsEqual<ProcessAction.assign>))]
			public Guid? AssigneeID { get; set; }
			public abstract class assigneeID : BqlGuid.Field<assigneeID> { }
			#endregion
		}	
		#endregion

		#region Views
		public PXFilter<HeaderFilter> Filter;

		[PXFilterable]
		public
			SelectFrom<SOPickingJob>.
			ProcessingView.FilteredBy<HeaderFilter>
			PickingJobs;
		public IEnumerable pickingJobs() => GetPickingJobs();
		#endregion

		#region Actions
		public PXCancel<HeaderFilter> Cancel;
		#endregion

		#region Popups
		// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
		public class ShowPickListPopup : GraphExtensions.
			ShowPickListPopup.
			On<SOPickingJobProcess, HeaderFilter>.
			FilteredBy<Where<SOPickingJob.jobID.IsEqual<SOPickingJob.jobID.FromCurrent>>> { }
		#endregion

		#region DAC overrides
		#region SOPickingWorksheet
		public SelectFrom<SOPickingWorksheet>.View DummyWorksheet;

		[PXMergeAttributes]
		[PXCustomizeBaseAttribute(typeof(PXUIFieldAttribute), nameof(PXUIFieldAttribute.DisplayName), "Pick List Type")]
		protected virtual void _(Events.CacheAttached<SOPickingWorksheet.worksheetType> e) { }

		[PXMergeAttributes]
		[PXCustomizeBaseAttribute(typeof(PXUIFieldAttribute), nameof(PXUIFieldAttribute.DisplayName), "Pick List Date")]
		protected virtual void _(Events.CacheAttached<SOPickingWorksheet.pickDate> e) { }
		#endregion
		#region SOShipment
		public SelectFrom<SOShipment>.View DummyShipment;

		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXUIVisible(typeof(HeaderFilter.worksheetType.FromCurrent.IsEqual<SOPickingWorksheet.worksheetType.single>))]
		protected virtual void _(Events.CacheAttached<SOShipment.shipmentQty> e) { }

		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXUIVisible(typeof(HeaderFilter.worksheetType.FromCurrent.IsEqual<SOPickingWorksheet.worksheetType.single>))]
		protected virtual void _(Events.CacheAttached<SOShipment.shipmentVolume> e) { }

		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXUIVisible(typeof(HeaderFilter.worksheetType.FromCurrent.IsEqual<SOPickingWorksheet.worksheetType.single>))]
		protected virtual void _(Events.CacheAttached<SOShipment.shipmentWeight> e) { }
		#endregion
		#region Carrier
		public SelectFrom<Carrier>.View DummyCarrier;

		[PXMergeAttributes]
		[PXUIVisible(typeof(HeaderFilter.worksheetType.FromCurrent.IsEqual<SOPickingWorksheet.worksheetType.single>))]
		protected virtual void _(Events.CacheAttached<Carrier.carrierID> e) { }

		[PXMergeAttributes]
		[PXCustomizeBaseAttribute(typeof(PXUIFieldAttribute), nameof(PXUIFieldAttribute.DisplayName), Messages.CarrierDescr)]
		[PXUIVisible(typeof(HeaderFilter.worksheetType.FromCurrent.IsEqual<SOPickingWorksheet.worksheetType.single>))]
		protected virtual void _(Events.CacheAttached<Carrier.description> e) { }
		#endregion
		#region Customer
		public SelectFrom<Customer>.View DummyCustomer;

		[PXMergeAttributes]
		[PXUIVisible(typeof(HeaderFilter.worksheetType.FromCurrent.IsEqual<SOPickingWorksheet.worksheetType.single>))]
		protected virtual void _(Events.CacheAttached<Customer.acctCD> e) { }

		[PXMergeAttributes]
		[PXUIVisible(typeof(HeaderFilter.worksheetType.FromCurrent.IsEqual<SOPickingWorksheet.worksheetType.single>))]
		protected virtual void _(Events.CacheAttached<Customer.acctName> e) { }
		#endregion
		#region Location
		public SelectFrom<CR.Location>.View DummyLocation;

		[PXMergeAttributes]
		[PXCustomizeBaseAttribute(typeof(CS.LocationRawAttribute), nameof(CS.LocationRawAttribute.DisplayName), "Customer Location ID")]
		[PXUIVisible(typeof(HeaderFilter.worksheetType.FromCurrent.IsEqual<SOPickingWorksheet.worksheetType.single>))]
		protected virtual void _(Events.CacheAttached<CR.Location.locationCD> e) { }

		[PXMergeAttributes]
		[PXCustomizeBaseAttribute(typeof(PXUIFieldAttribute), nameof(PXUIFieldAttribute.DisplayName), "Customer Location Name")]
		[PXUIVisible(typeof(HeaderFilter.worksheetType.FromCurrent.IsEqual<SOPickingWorksheet.worksheetType.single>))]
		protected virtual void _(Events.CacheAttached<CR.Location.descr> e) { }
		#endregion
		#endregion

		#region Event handlers
		protected virtual void _(Events.RowSelected<HeaderFilter> e)
		{
			string action = e.Row.Action;
			var settings = e.Row.GetExtension<HeaderSettings>();
			PickingJobs.SetProcessDelegate(list => ProcessPickingJobsHandler(action, settings, list));
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

		protected virtual IEnumerable<PXResult<SOPickingJob>> GetPickingJobs()
		{
			HeaderFilter filter = Filter.Current;

			if (filter.Action == ProcessAction.None)
				yield break;

			BqlCommand cmd = new
				SelectFrom<SOPickingJob>.
				InnerJoin<SOPicker>.On<SOPickingJob.FK.Picker>.
				InnerJoin<SOPickingWorksheet>.On<SOPicker.FK.Worksheet>.
				InnerJoin<INSite>.On<SOPickingWorksheet.FK.Site>.
				LeftJoin<SOPickerToShipmentLink>.On<
					SOPickerToShipmentLink.FK.Picker.
					And<SOPickingWorksheet.worksheetType.IsEqual<SOPickingWorksheet.worksheetType.single>>>.
				LeftJoin<SOShipment>.On<SOPickerToShipmentLink.FK.Shipment>.
				LeftJoin<Carrier>.On<SOShipment.FK.Carrier>.
				LeftJoin<Customer>.On<SOShipment.FK.Customer>.
				LeftJoin<CR.Location>.On<SOShipment.FK.CustomerLocation>.
				Where<
					Match<INSite, AccessInfo.userName.FromCurrent>.
					And<SOPickingWorksheet.pickDate.IsLessEqual<HeaderFilter.endDate.FromCurrent>>.
					And<SOPickingWorksheet.siteID.IsEqual<HeaderFilter.siteID.FromCurrent>>>.
				View(this).View.BqlSelect;
			var parameters = new List<object>();
			cmd = AppendFilter(cmd, parameters, filter);
			var view = new PXView(this, false, cmd);

			int startRow = PXView.StartRow;
			int totalRows = 0;
			foreach (PXResult<SOPickingJob> res in view.Select(PXView.Currents, parameters.ToArray(), PXView.Searches, PXView.SortColumns, PXView.Descendings, PXView.Filters, ref startRow, PXView.MaximumRows, ref totalRows))
			{
				SOPickingJob job = PXResult.Unwrap<SOPickingJob>(res);
				SOPicker picker = PXResult.Unwrap<SOPicker>(res);

				if (filter.MaxNumberOfLinesInPickList.IsNotIn(null, 0))
				{
					int linesCount =
						SelectFrom<SOPickerListEntry>.
						Where<SOPickerListEntry.FK.Picker.SameAsCurrent>.
						AggregateTo<Count>.
						View.ReadOnly
						.SelectMultiBound(this, new[] { picker })
						.RowCount ?? 0;

					if (linesCount > filter.MaxNumberOfLinesInPickList)
						continue;
				}

				if (filter.MaxQtyInLines.IsNotIn(null, 0))
				{
					int linesCount =
						SelectFrom<SOPickerListEntry>.
						Where<
							SOPickerListEntry.FK.Picker.SameAsCurrent.
							And<SOPickerListEntry.qty.IsGreater<@P.AsDecimal>>>.
						AggregateTo<Count>.
						View.ReadOnly
						.SelectMultiBound(this, new[] { picker }, filter.MaxQtyInLines.Value)
						.RowCount ?? 0;

					if (linesCount > 0)
						continue;
				}

				if (PickingJobs.Locate(job) is SOPickingJob cached)
					job.Selected = cached.Selected;

				yield return res;
			}
			PXView.StartRow = 0;

			PickingJobs.Cache.IsDirty = false;
		}

		public virtual BqlCommand AppendFilter(BqlCommand cmd, IList<object> parameters, HeaderFilter filter)
		{
			switch (filter.Action)
			{
				case ProcessAction.Send:
					cmd = cmd.WhereAnd<Where<SOPickingJob.status.IsEqual<SOPickingJob.status.onHold>>>();
					break;
				case ProcessAction.Remove:
					cmd = cmd.WhereAnd<Where<
						SOPickingJob.status.IsIn<SOPickingJob.status.enqueued, SOPickingJob.status.reenqueued, SOPickingJob.status.assigned>.
						And<SOPickingJob.actualAssigneeID.IsNull>>>();
					break;
				case ProcessAction.Priority:
					cmd = cmd.WhereAnd<Where<SOPickingJob.status.IsIn<SOPickingJob.status.onHold, SOPickingJob.status.enqueued, SOPickingJob.status.reenqueued, SOPickingJob.status.assigned, SOPickingJob.status.picking>>>();
					break;
				case ProcessAction.Assign:
					cmd = cmd.WhereAnd<Where<
						SOPickingJob.status.IsIn<SOPickingJob.status.onHold, SOPickingJob.status.enqueued, SOPickingJob.status.reenqueued, SOPickingJob.status.assigned>.
						And<SOPickingJob.actualAssigneeID.IsNull>>>();
					break;
			}

			if (filter.WorksheetType != HeaderFilter.worksheetType.All)
				cmd = cmd.WhereAnd<Where<SOPickingWorksheet.worksheetType.IsEqual<HeaderFilter.worksheetType.FromCurrent>>>();

			if (filter.Priority != HeaderFilter.priority.All)
				cmd = cmd.WhereAnd<Where<SOPickingJob.priority.IsEqual<HeaderFilter.priority.FromCurrent>>>();

			if (filter.WorksheetType == SOPickingWorksheet.worksheetType.Single)
			{
				if (!string.IsNullOrEmpty(filter.CarrierPluginID))
					cmd = cmd.WhereAnd<Where<Carrier.carrierPluginID.IsEqual<HeaderFilter.carrierPluginID.FromCurrent>>>();

				if (!string.IsNullOrEmpty(filter.ShipVia))
					cmd = cmd.WhereAnd<Where<Carrier.carrierID.IsEqual<HeaderFilter.shipVia.FromCurrent>>>();

				if (filter.CustomerID != null)
					cmd = cmd.WhereAnd<Where<SOShipment.customerID.IsEqual<HeaderFilter.customerID.FromCurrent>>>();
			}

			return cmd;
		}

		private static void ProcessPickingJobsHandler(string action, HeaderSettings settings, IEnumerable<SOPickingJob> jobs)
			=> CreateInstance<SOPickingJobProcess>().ProcessPickingJobs(action, settings, jobs);

		protected virtual void ProcessPickingJobs(string action, HeaderSettings settings, IEnumerable<SOPickingJob> jobs)
		{
			switch (action)
			{
				case ProcessAction.Send:
					SendToQueue(settings, jobs);
					break;
				case ProcessAction.Remove:
					RemoveFromQueue(settings, jobs);
					break;
				case ProcessAction.Priority:
					ChangePriority(settings, jobs);
					break;
				case ProcessAction.Assign:
					AssignToPicker(settings, jobs);
					break;
			}
		}

		protected virtual void SendToQueue(HeaderSettings settings, IEnumerable<SOPickingJob> jobs)
			=> BulkUpdateJobs(settings, jobs, (j, s) => PickingJobs.Cache.SetValueExt<SOPickingJob.status>(j, SOPickingJob.status.Enqueued));

		protected virtual void RemoveFromQueue(HeaderSettings settings, IEnumerable<SOPickingJob> jobs)
			=> BulkUpdateJobs(settings, jobs, (j, s) => PickingJobs.Cache.SetValueExt<SOPickingJob.status>(j, SOPickingJob.status.OnHold));

		protected virtual void ChangePriority(HeaderSettings settings, IEnumerable<SOPickingJob> jobs)
			=> BulkUpdateJobs(settings, jobs, (j, s) => PickingJobs.Cache.SetValueExt<SOPickingJob.priority>(j, s.NewPriority));

		protected virtual void AssignToPicker(HeaderSettings settings, IEnumerable<SOPickingJob> jobs)
			=> BulkUpdateJobs(settings, jobs, (j, s) => PickingJobs.Cache.SetValueExt<SOPickingJob.preferredAssigneeID>(j, s.AssigneeID));

		protected virtual void BulkUpdateJobs(HeaderSettings settings, IEnumerable<SOPickingJob> jobs, Action<SOPickingJob, HeaderSettings> change)
		{
			foreach (var job in jobs)
			{
				change(job, settings);
				PickingJobs.Update(job);
			}

			if (PickingJobs.Cache.IsDirty)
				Persist();
		}

		#region Well-known Extensions
		// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
		public class InventoryLinkFilterExt : IN.GraphExtensions.InventoryLinkFilterExtensionBase<SOPickingJobProcess, HeaderFilter, HeaderFilter.inventoryID>
		{
			// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
			public class descr : AttachedInventoryDescription<descr> { }

			[PXMergeAttributes(Method = MergeMethod.Replace)]
			[Inventory(IsKey = true)]
			protected override void _(Events.CacheAttached<IN.GraphExtensions.InventoryLinkFilter.inventoryID> e) { }

			/// Overrides <see cref="SOPickingJobProcess.AppendFilter(BqlCommand, IList{object}, HeaderFilter)"/>
			[PXOverride]
			public BqlCommand AppendFilter(BqlCommand cmd, IList<object> parameters, HeaderFilter filter,
				Func<BqlCommand, IList<object>, HeaderFilter, BqlCommand> base_AppendFilter)
			{
				cmd = base_AppendFilter(cmd, parameters, filter);

				var inventories = GetSelectedEntities(filter).ToArray();
				if (inventories.Length > 0)
				{
					cmd = cmd.WhereAnd<Where<Not<Exists<
							SelectFrom<SOPickerListEntry>.
							Where<
								SOPickerListEntry.FK.Picker.
								And<SOPickerListEntry.inventoryID.IsNotIn<@P.AsInt>>>
						>>>>();
					parameters.Add(inventories);
				}

				return cmd;
			}
		}

		// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
		public class LocationFilterExt : IN.GraphExtensions.LocationLinkFilterExtensionBase<SOPickingJobProcess, HeaderFilter, HeaderFilter.locationID>
		{
			// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
			public class descr : AttachedLocationDescription<descr> { }

			[PXMergeAttributes(Method = MergeMethod.Replace)]
			[Location(typeof(HeaderFilter.siteID), IsKey = true)]
			protected override void _(Events.CacheAttached<IN.GraphExtensions.LocationLinkFilter.locationID> e) { }

			/// Overrides <see cref="SOPickingJobProcess.AppendFilter(BqlCommand, IList{object}, HeaderFilter)"/>
			[PXOverride]
			public BqlCommand AppendFilter(BqlCommand cmd, IList<object> parameters, HeaderFilter filter,
				Func<BqlCommand, IList<object>, HeaderFilter, BqlCommand> base_AppendFilter)
			{
				cmd = base_AppendFilter(cmd, parameters, filter);

				var locations = GetSelectedEntities(filter).ToArray();
				if (locations.Length > 0)
				{
					cmd = cmd.WhereAnd<Where<Not<Exists<
						SelectFrom<SOPickerListEntry>.
						Where<
							SOPickerListEntry.FK.Picker.
							And<SOPickerListEntry.locationID.IsNotIn<@P.AsInt>>>
					>>>>();
					parameters.Add(locations);
				}

				return cmd;
			}
		}
		#endregion

		[PXLocalizable]
		public abstract class CacheNames
		{
			public const string Filter = "Filter";
		}
	}
}
