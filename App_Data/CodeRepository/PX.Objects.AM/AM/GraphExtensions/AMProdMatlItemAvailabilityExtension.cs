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

namespace PX.Objects.AM
{
	public abstract class AMProdMatlItemAvailabilityExtension<TGraph> : IN.GraphExtensions.ItemAvailabilityExtension<TGraph, AMProdMatl, AMProdMatlSplit>
		where TGraph : PXGraph
	{
		protected override AMProdMatlSplit EnsureSplit(ILSMaster row) => Base.FindImplementation<AMProdMatlLineSplittingExtension<TGraph>>().EnsureSplit(row);

		protected override decimal GetUnitRate(AMProdMatl line) => GetUnitRate<AMProdMatl.inventoryID, AMProdMatl.uOM>(line);

		protected override string GetStatus(AMProdMatl line) => string.Empty;

		protected override void Optimize()
		{
			base.Optimize();

			foreach (PXResult<AMProdMatl, INUnit, INSiteStatusByCostCenter> res in
				SelectFrom<AMProdMatl>.
				InnerJoin<INUnit>.On<
					INUnit.inventoryID.IsEqual<AMProdMatl.inventoryID>.
					And<INUnit.fromUnit.IsEqual<AMProdMatl.uOM>>>.
				InnerJoin<INSiteStatusByCostCenter>.On<
					AMProdMatl.inventoryID.IsEqual<INSiteStatusByCostCenter.inventoryID>.
					And<AMProdMatl.subItemID.IsEqual<INSiteStatusByCostCenter.subItemID>>.
					And<AMProdMatl.siteID.IsEqual<INSiteStatusByCostCenter.siteID>>.
					And<INSiteStatusByCostCenter.costCenterID.IsEqual<CostCenter.freeStock>>>.
				Where<
					AMProdMatl.orderType.IsEqual<AMProdMatl.orderType.FromCurrent>.
					And<AMProdMatl.prodOrdID.IsEqual<AMProdMatl.prodOrdID.FromCurrent>>>.
				View.ReadOnly.Select(Base))
			{
				SelectFrom<INUnit>.
				Where<
					INUnit.unitType.IsEqual<INUnitType.inventoryItem>.
					And<INUnit.inventoryID.IsEqual<@P.AsInt>>.
					And<INUnit.toUnit.IsEqual<@P.AsString>>.
					And<INUnit.fromUnit.IsEqual<@P.AsString>>>.
				View.ReadOnly.StoreResult(Base, (INUnit)res);

				INSiteStatusByCostCenter.PK.StoreResult(Base, res);
			}
		}

		protected override void RaiseQtyExceptionHandling(AMProdMatl line, PXExceptionInfo ei, decimal? newValue)
		{
			LineCache.RaiseExceptionHandling<AMProdMatl.qtyRemaining>(line, null,
				new PXSetPropertyException(ei.MessageFormat, PXErrorLevel.Warning,
					LineCache.GetStateExt<AMProdMatl.inventoryID>(line),
					LineCache.GetStateExt<AMProdMatl.subItemID>(line),
					LineCache.GetStateExt<AMProdMatl.siteID>(line),
					LineCache.GetStateExt<AMProdMatl.locationID>(line),
					LineCache.GetValue<AMProdMatl.lotSerialNbr>(line)));
		}

		protected override void RaiseQtyExceptionHandling(AMProdMatlSplit split, PXExceptionInfo ei, decimal? newValue)
		{
			SplitCache.RaiseExceptionHandling<AMProdMatlSplit.qty>(split, null,
				new PXSetPropertyException(ei.MessageFormat, PXErrorLevel.Warning,
					SplitCache.GetStateExt<AMProdMatlSplit.inventoryID>(split),
					SplitCache.GetStateExt<AMProdMatlSplit.subItemID>(split),
					SplitCache.GetStateExt<AMProdMatlSplit.siteID>(split),
					SplitCache.GetStateExt<AMProdMatlSplit.locationID>(split),
					SplitCache.GetValue<AMProdMatlSplit.lotSerialNbr>(split)));
		}
	}
}
