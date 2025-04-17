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
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.Common.Exceptions;
using PX.Objects.IN;
using PX.Objects.SO.GraphExtensions;

namespace PX.Objects.FS
{
	// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
	public class FSServiceOrderItemAvailabilityExtension : SOBaseItemAvailabilityExtension<ServiceOrderEntry, FSSODet, FSSODetSplit>
	{
		protected override FSSODetSplit EnsureSplit(ILSMaster row)
			=> Base.FindImplementation<FSServiceOrderLineSplittingExtension>().EnsureSplit(row);

		protected override decimal GetUnitRate(FSSODet line) => GetUnitRate<FSSODet.inventoryID, FSSODet.uOM>(line);


		protected override string GetStatus(FSSODet line)
		{
			string status = string.Empty;

			if (FetchWithLineUOM(line, excludeCurrent: line?.Completed != true, CostCenter.FreeStock) is IStatus availability)
			{
				status = FormatStatus(availability, line.UOM);
				Check(line, availability);
			}

			return status;
		}

		private string FormatStatus(IStatus availability, string uom)
		{
			return PXMessages.LocalizeFormatNoPrefix(SO.Messages.Availability_Info,
				uom,
				FormatQty(availability.QtyOnHand),
				FormatQty(availability.QtyAvail),
				FormatQty(availability.QtyHardAvail));
		}

		protected override IStatus Fetch(ILSDetail split, bool excludeCurrent, int? costCenterID)
		{
			return base.Fetch(split, excludeCurrent, costCenterID);
		}

		public override void Check(ILSMaster row, int? costCenterID)
		{
			base.Check(row, costCenterID);
			MemoCheck(row);
		}

		protected virtual void MemoCheck(ILSMaster row)
		{
			if (row is FSSODet line)
			{
				MemoCheck(line);

				FSSODetSplit split = EnsureSplit(line);
				MemoCheck(line, split, triggeredBySplit: false);

				if (split.LotSerialNbr == null)
					row.LotSerialNbr = null;
			}
			else if (row is FSSODetSplit split)
			{
				line = PXParentAttribute.SelectParent<FSSODet>(SplitCache, split);
				MemoCheck(line);
				MemoCheck(line, split, triggeredBySplit: true);
			}
		}

		public virtual bool MemoCheck(FSSODet line) => MemoCheckQty(line);
		protected virtual bool MemoCheckQty(FSSODet row) => true;
		protected virtual bool MemoCheck(FSSODet line, FSSODetSplit split, bool triggeredBySplit) => true;

		protected override int DetailsCountToEnableOptimization => 50;
		protected override void Optimize()
		{
			base.Optimize();

			foreach (PXResult<FSSODet, INUnit, INSiteStatusByCostCenter> res in
				SelectFrom<FSSODet>.
				InnerJoin<INUnit>.On<
					INUnit.inventoryID.IsEqual<FSSODet.inventoryID>.
					And<INUnit.fromUnit.IsEqual<FSSODet.uOM>>>.
				InnerJoin<INSiteStatusByCostCenter>.On<
					FSSODet.inventoryID.IsEqual<INSiteStatusByCostCenter.inventoryID>.
					And<FSSODet.subItemID.IsEqual<INSiteStatusByCostCenter.subItemID>>.
					And<FSSODet.siteID.IsEqual<INSiteStatusByCostCenter.siteID>>.
					And<INSiteStatusByCostCenter.costCenterID.IsEqual<CostCenter.freeStock>>>.
				Where<FSSODet.FK.ServiceOrder.SameAsCurrent>.
				View.ReadOnly.Select(Base))
			{
				INUnit.UK.ByInventory.StoreResult(Base, res);
				INSiteStatusByCostCenter.PK.StoreResult(Base, res);
			}
		}

		protected override void RaiseQtyExceptionHandling(FSSODet line, PXExceptionInfo ei, decimal? newValue)
		{
			LineCache.RaiseExceptionHandling<FSSODet.orderQty>(line, newValue,
				new PXSetPropertyException(ei.MessageFormat, PXErrorLevel.Warning,
					LineCache.GetStateExt<FSSODet.inventoryID>(line),
					LineCache.GetStateExt<FSSODet.subItemID>(line),
					LineCache.GetStateExt<FSSODet.siteID>(line),
					LineCache.GetStateExt<FSSODet.locationID>(line),
					LineCache.GetValue<FSSODet.lotSerialNbr>(line)));
		}

		protected override void RaiseQtyExceptionHandling(FSSODetSplit split, PXExceptionInfo ei, decimal? newValue)
		{
			SplitCache.RaiseExceptionHandling<FSSODetSplit.qty>(split, newValue,
				new PXSetPropertyException(ei.MessageFormat, PXErrorLevel.Warning,
					SplitCache.GetStateExt<FSSODetSplit.inventoryID>(split),
					SplitCache.GetStateExt<FSSODetSplit.subItemID>(split),
					SplitCache.GetStateExt<FSSODetSplit.siteID>(split),
					SplitCache.GetStateExt<FSSODetSplit.locationID>(split),
					SplitCache.GetValue<FSSODetSplit.lotSerialNbr>(split)));
		}
	}
}
