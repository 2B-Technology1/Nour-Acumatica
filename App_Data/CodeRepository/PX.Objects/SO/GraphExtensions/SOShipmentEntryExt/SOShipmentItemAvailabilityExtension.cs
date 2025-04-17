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

using PX.Common;
using PX.Data;
using PX.Data.BQL.Fluent;

using PX.Objects.Common;
using PX.Objects.Common.Exceptions;
using PX.Objects.IN;
using PX.Objects.IN.GraphExtensions;

namespace PX.Objects.SO.GraphExtensions.SOShipmentEntryExt
{
	// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
	public class SOShipmentItemAvailabilityExtension : ItemAvailabilityExtension<SOShipmentEntry, SOShipLine, SOShipLineSplit>
	{
		protected override SOShipLineSplit EnsureSplit(ILSMaster row)
			=> Base.FindImplementation<SOShipmentLineSplittingExtension>().EnsureSplit(row);

		public override void Initialize()
		{
			base.Initialize();
			ManualEvent.Row<SOShipLine>.Persisting.Subscribe(Base, EventHandler);
			ManualEvent.Row<SOShipLineSplit>.Persisting.Subscribe(Base, EventHandler);
		}

		#region Event Handlers
		protected virtual void EventHandler(ManualEvent.Row<SOShipLine>.Persisting.Args e) // former SOShipLine_RowPersisting from LSSOShipLine
		{
			if (e.Operation.Command().IsIn(PXDBOperation.Insert, PXDBOperation.Update) && e.Row != null && e.Row.BaseQty != 0m && AdvancedCheck == true)
				Check(e.Row, e.Row.CostCenterID);
		}

		protected virtual void EventHandler(ManualEvent.Row<SOShipLineSplit>.Persisting.Args e) // former SOShipLineSplit_RowPersisting from LSSOShipLine
		{
			if (e.Operation.Command().IsIn(PXDBOperation.Insert, PXDBOperation.Update))
			{
				bool requireLocationAndSubItem = e.Row.IsStockItem == true && e.Row.BaseQty != 0m;

				PXDefaultAttribute.SetPersistingCheck<SOShipLineSplit.subItemID>(e.Cache, e.Row, requireLocationAndSubItem ? PXPersistingCheck.NullOrBlank : PXPersistingCheck.Nothing);
				PXDefaultAttribute.SetPersistingCheck<SOShipLineSplit.locationID>(e.Cache, e.Row, requireLocationAndSubItem ? PXPersistingCheck.NullOrBlank : PXPersistingCheck.Nothing);

				if (AdvancedCheck == true && e.Row.BaseQty != 0m)
				{
					SOShipLine soline = PXParentAttribute.SelectParent<SOShipLine>(e.Cache, e.Row);
					Check(e.Row, soline.CostCenterID);
				}
			}
		}
		#endregion

		protected override decimal GetUnitRate(SOShipLine line) => GetUnitRate<SOShipLine.inventoryID, SOShipLine.uOM>(line);

		protected override string GetStatus(SOShipLine line)
		{
			string status = string.Empty;

			if (FetchWithLineUOM(line, excludeCurrent: true, line.CostCenterID) is IStatus availability)
			{
				status = FormatStatus(availability, line.UOM);
				Check(line, availability);
			}

			return status;
		}

		private string FormatStatus(IStatus availability, string uom)
		{
			return PXMessages.LocalizeFormatNoPrefixNLA(
				IN.Messages.Availability_Info,
				uom,
				FormatQty(availability.QtyOnHand),
				FormatQty(availability.QtyAvail),
				FormatQty(availability.QtyHardAvail));
		}

		public virtual void OrderCheck(SOShipLine line)
		{
			if (Base.FindImplementation<SOShipmentLineSplittingExtension>().UnattendedMode)
				return;

			if (line.OrigOrderNbr != null)
			{
				SOLineSplit2 split = PXSelect<SOLineSplit2,
					Where<SOLineSplit2.orderType, Equal<Current<SOShipLine.origOrderType>>,
						And<SOLineSplit2.orderNbr, Equal<Current<SOShipLine.origOrderNbr>>,
						And<SOLineSplit2.lineNbr, Equal<Current<SOShipLine.origLineNbr>>,
						And<SOLineSplit2.splitLineNbr, Equal<Current<SOShipLine.origSplitLineNbr>>>>>>>
					.SelectSingleBound(Base, new object[] { line });

				SOLine2 soLine = PXSelect<SOLine2,
					Where<SOLine2.orderType, Equal<Current<SOShipLine.origOrderType>>,
						And<SOLine2.orderNbr, Equal<Current<SOShipLine.origOrderNbr>>,
						And<SOLine2.lineNbr, Equal<Current<SOShipLine.origLineNbr>>>>>>
					.SelectSingleBound(Base, new object[] { line });

				if (split != null && soLine != null)
				{
					if (split.IsAllocated == true && split.Qty * soLine.CompleteQtyMax / 100 < split.ShippedQty)
						throw new PXSetPropertyException(Messages.OrderSplitCheck_QtyNegative,
							LineCache.GetValueExt<SOShipLine.inventoryID>(line),
							LineCache.GetValueExt<SOShipLine.subItemID>(line),
							LineCache.GetValueExt<SOShipLine.origOrderType>(line),
							LineCache.GetValueExt<SOShipLine.origOrderNbr>(line));

					decimal? soLineOrderQty = soLine.LineSign * soLine.OrderQty;
					decimal? soLineShippedQty = soLine.LineSign * soLine.ShippedQty;
					if (PXDBPriceCostAttribute.Round((decimal)(soLineOrderQty * soLine.CompleteQtyMax / 100m - soLineShippedQty)) < 0m &&
						PXDBPriceCostAttribute.Round((decimal)(split.Qty * soLine.CompleteQtyMax / 100m - split.ShippedQty)) < 0m)
					{
						throw new PXSetPropertyException(Messages.OrderCheck_QtyNegative,
							LineCache.GetValueExt<SOShipLine.inventoryID>(line),
							LineCache.GetValueExt<SOShipLine.subItemID>(line),
							LineCache.GetValueExt<SOShipLine.origOrderType>(line),
							LineCache.GetValueExt<SOShipLine.origOrderNbr>(line));
					}
				}
			}
		}

		public override void Check(ILSMaster row, int? costCenterID)
		{
			base.Check(row, costCenterID);

			if (row is SOShipLine line)
			{
				try
				{
					OrderCheck(line);
				}
				catch (PXSetPropertyException ex)
				{
					LineCache.RaiseExceptionHandling<SOShipLine.shippedQty>(line, line.ShippedQty, ex);
				}
			}
			else if (row is SOShipLineSplit split)
			{
				line = PXParentAttribute.SelectParent<SOShipLine>(SplitCache, split);
				try
				{
					OrderCheck(line);
				}
				catch (PXSetPropertyException ex)
				{
					SplitCache.RaiseExceptionHandling<SOShipLineSplit.qty>(split, split.Qty, ex);
				}
			}
		}

		protected override void Check(ILSMaster row, IStatus availability)
		{
			base.Check(row, availability);

			foreach (var errorInfo in GetCheckErrorsQtyOnHand(row, availability))
				RaiseQtyExceptionHandling(row, errorInfo, row.Qty);
		}

		protected virtual IEnumerable<PXExceptionInfo> GetCheckErrorsQtyOnHand(ILSMaster row, IStatus availability)
		{
			if (!IsAvailableOnHandQty(row, availability))
			{
				string message = GetErrorMessageQtyOnHand(GetStatusLevel(availability));

				if (message != null)
					yield return new PXExceptionInfo(PXErrorLevel.Warning, message);
			}
		}

		protected virtual bool IsAvailableOnHandQty(ILSMaster row, IStatus availability)
		{
			if (row.InvtMult == -1 && row.BaseQty > 0m && availability != null)
				if (availability.QtyOnHand - row.Qty < 0m && Base.Document.Current?.Confirmed == false)
					return false;

			return true;
		}

		protected override void Summarize(IStatus allocated, IStatus existing)
		{
			base.Summarize(allocated, existing);
			allocated.QtyAvail = allocated.QtyHardAvail;
		}

		protected override void ExcludeCurrent(ILSDetail currentSplit, IStatus allocated, AvailabilitySigns signs)
		{
			if (signs.SignQtyHardAvail != Sign.Zero)
			{
				allocated.QtyAvail -= signs.SignQtyHardAvail * (currentSplit.BaseQty ?? 0m);
				allocated.QtyNotAvail += signs.SignQtyHardAvail * (currentSplit.BaseQty ?? 0m);
				allocated.QtyHardAvail -= signs.SignQtyHardAvail * (currentSplit.BaseQty ?? 0m);
			}

			//Exclude Unassigned
			foreach (Unassigned.SOShipLineSplit detail in SelectUnassignedDetails((SOShipLineSplit)currentSplit))
			{
				if (signs.SignQtyHardAvail != Sign.Zero && (currentSplit.LocationID == null || currentSplit.LocationID == detail.LocationID) &&
					(currentSplit.LotSerialNbr == null || string.IsNullOrEmpty(detail.LotSerialNbr) || string.Equals(currentSplit.LotSerialNbr, detail.LotSerialNbr, StringComparison.InvariantCultureIgnoreCase)))
				{
					allocated.QtyAvail -= signs.SignQtyHardAvail * (detail.BaseQty ?? 0m);
					allocated.QtyHardAvail -= signs.SignQtyHardAvail * (detail.BaseQty ?? 0m);
				}
			}
		}


		private bool? _advancedCheck;
		public bool? AdvancedCheck
		{
			get
			{
				if (Base.Caches<SOSetup>().Current is SOSetup setup && setup.AdvancedAvailCheck == true)
					if (_advancedCheck != null)
						return _advancedCheck == true;

				return false;
			}
			set => _advancedCheck = value;
		}

		protected override void Optimize()
		{
			base.Optimize();

			foreach (PXResult<SOShipLine, INSiteStatusByCostCenter, INLocationStatusByCostCenter, INLotSerialStatusByCostCenter> res in
				SelectFrom<SOShipLine>.
				InnerJoin<INSiteStatusByCostCenter>.On<SOShipLine.FK.SiteStatusByCostCenter>.
				LeftJoin<INLocationStatusByCostCenter>.On<SOShipLine.FK.LocationStatusByCostCenter>.
				LeftJoin<INLotSerialStatusByCostCenter>.On<SOShipLine.FK.LotSerialStatusByCostCenter>.
				Where<SOShipLine.FK.Shipment.SameAsCurrent>.
				View.ReadOnly.Select(Base))
			{
				(var _, var siteStatusByCostCenter, var locStatusByCostCenter, var lotSerStatusByCostCenter) = res;

				INSiteStatusByCostCenter.PK.StoreResult(Base, siteStatusByCostCenter);

				if (locStatusByCostCenter.LocationID != null)
					INLocationStatusByCostCenter.PK.StoreResult(Base, locStatusByCostCenter);

				if (lotSerStatusByCostCenter?.LotSerialNbr != null)
					INLotSerialStatusByCostCenter.PK.StoreResult(Base, lotSerStatusByCostCenter);
			}
		}


		public virtual List<Unassigned.SOShipLineSplit> SelectUnassignedDetails(SOShipLineSplit assignedSplit)
		{
			var unassignedRow = new Unassigned.SOShipLineSplit
			{
				ShipmentNbr = assignedSplit.ShipmentNbr,
				LineNbr = assignedSplit.LineNbr,
				SplitLineNbr = assignedSplit.SplitLineNbr
			};

			return PXParentAttribute
				.SelectSiblings(
					Base.Caches<Unassigned.SOShipLineSplit>(),
					unassignedRow,
					IsOptimizationEnabled ? typeof(SOShipment) : typeof(SOShipLine))
				.Cast<Unassigned.SOShipLineSplit>()
				.Where(us =>
					us.InventoryID == assignedSplit.InventoryID &&
					us.LineNbr == assignedSplit.LineNbr)
				.ToList();
		}


		protected override void RaiseQtyExceptionHandling(SOShipLine line, PXExceptionInfo ei, decimal? newValue)
		{
			LineCache.RaiseExceptionHandling<SOShipLine.shippedQty>(line, newValue,
				new PXSetPropertyException(ei.MessageFormat, AdvancedCheck == true ? PXErrorLevel.Error : PXErrorLevel.Warning,
					LineCache.GetStateExt<SOShipLine.inventoryID>(line),
					LineCache.GetStateExt<SOShipLine.subItemID>(line),
					LineCache.GetStateExt<SOShipLine.siteID>(line),
					LineCache.GetStateExt<SOShipLine.locationID>(line),
					LineCache.GetValue<SOShipLine.lotSerialNbr>(line)));
		}

		protected override void RaiseQtyExceptionHandling(SOShipLineSplit split, PXExceptionInfo ei, decimal? newValue)
		{
			SplitCache.RaiseExceptionHandling<SOShipLineSplit.qty>(split, newValue,
				new PXSetPropertyException(ei.MessageFormat, AdvancedCheck == true ? PXErrorLevel.Error : PXErrorLevel.Warning,
					SplitCache.GetStateExt<SOShipLineSplit.inventoryID>(split),
					SplitCache.GetStateExt<SOShipLineSplit.subItemID>(split),
					SplitCache.GetStateExt<SOShipLineSplit.siteID>(split),
					SplitCache.GetStateExt<SOShipLineSplit.locationID>(split),
					SplitCache.GetValue<SOShipLineSplit.lotSerialNbr>(split)));
		}
	}
}
