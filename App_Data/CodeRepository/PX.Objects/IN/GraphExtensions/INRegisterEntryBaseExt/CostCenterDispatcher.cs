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
using PX.Common;
using PX.Data;
using PX.Objects.IN.Attributes;

namespace PX.Objects.IN.GraphExtensions.INRegisterEntryBaseExt
{
	public class CostCenterDispatcher : CostCenterDispatcher<INRegisterEntryBase, INTran, INTran.costCenterID>
	{
		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<CS.FeaturesSet.projectAccounting>()
				|| PXAccess.FeatureInstalled<CS.FeaturesSet.specialOrders>();
		}

		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[CostCenterDBDefault]
		protected virtual void _(Events.CacheAttached<INTran.costCenterID> e) { }

		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[CostCenterDBDefault]
		protected virtual void _(Events.CacheAttached<INTran.toCostCenterID> e) { }


		protected override void SubscribeToFieldsDependOn()
		{
			base.SubscribeToFieldsDependOn();

			Base.FieldUpdated.AddHandler(typeof(INTran), typeof(INTran.costLayerType).Name, FieldDependOnUpdated);

			foreach (Type field in Base.FindAllImplementations<IINTranCostCenterSupport>()
				.SelectMany(ext => ext.GetDestinationFieldsDependOn())
				.Append(typeof(INTran.toCostLayerType)))
			{
				if (!typeof(IBqlField).IsAssignableFrom(field) || BqlCommand.GetItemType(field) != typeof(INTran))
				{
					throw new PXArgumentException();
				}
				Base.FieldUpdated.AddHandler(typeof(INTran), field.Name, FieldDestinationDependOnUpdated);
			}
		}

		private IINTranCostCenterSupport GetCostCenterSupportByINTran(INTran tran)
		{
			return Base.FindAllImplementations<IINTranCostCenterSupport>()
				.OrderBy(e => e.SortOrder)
				.FirstOrDefault(e => e.IsSpecificCostCenter(tran));
		}

		private IINTranCostCenterSupport GetCostCenterSupportByINTranDestination(INTran tran)
		{
			return Base.FindAllImplementations<IINTranCostCenterSupport>()
				.OrderBy(e => e.SortOrder)
				.FirstOrDefault(e => e.IsDestinationSpecificCostCenter(tran));
		}

		private IINTranCostCenterSupport GetCostCenterSupportByCostLayerType(string costLayerType)
		{
			return Base.FindAllImplementations<IINTranCostCenterSupport>()
				.SingleOrDefault(e => e.IsSupported(costLayerType));
		}

		public void SetCostLayerType(INTran tran)
		{
			// TODO: Special: check if this method is still needed when we have subscription to FieldDependOnUpdated
			var srcExt = GetCostCenterSupportByINTran(tran);
			if (srcExt != null)
				tran.CostLayerType = srcExt.GetCostLayerType(tran);
			var destExt = GetCostCenterSupportByINTranDestination(tran);
			if (destExt != null)
				tran.ToCostLayerType = destExt.GetCostLayerType(tran);
		}

		protected virtual void _(Events.FieldUpdated<INTran, INTran.costLayerType> e)
		{
			string newCostLayerType = (string)e.NewValue,
				oldCostLayerType = (string)e.OldValue;
			if (oldCostLayerType.IsIn(newCostLayerType, CostLayerType.Normal, null))
				return;
			var ext = GetCostCenterSupportByCostLayerType(oldCostLayerType);
			ext?.OnCostLayerTypeChanged(e.Row, newCostLayerType);
		}

		protected virtual void _(Events.FieldUpdated<INTran, INTran.toCostLayerType> e)
		{
			string newCostLayerType = (string)e.NewValue,
				oldCostLayerType = (string)e.OldValue;
			if (oldCostLayerType.IsIn(newCostLayerType, CostLayerType.Normal, null))
				return;
			var ext = GetCostCenterSupportByCostLayerType(oldCostLayerType);
			ext?.OnDestinationCostLayerTypeChanged(e.Row, newCostLayerType);
		}

		protected virtual void _(Events.RowPersisting<INTran> e)
		{
			if (e.Operation.Command().IsIn(PXDBOperation.Insert, PXDBOperation.Update))
			{
				var srcExt = GetCostCenterSupportByCostLayerType(e.Row.CostLayerType);
				if (srcExt == null && e.Row.CostLayerType != CostLayerType.Normal)
				{
					throw new InvalidOperationException($"Needed graph extension for Cost Layer Type = '{ e.Row.CostLayerType }' was not found.");
				}
				srcExt?.ValidateForPersisting(e.Row);

				var destExt = GetCostCenterSupportByCostLayerType(e.Row.ToCostLayerType);
				if (destExt == null && e.Row.ToCostLayerType != CostLayerType.Normal)
				{
					throw new InvalidOperationException($"Needed graph extension for Cost Layer Type = '{ e.Row.ToCostLayerType }' was not found.");
				}
				destExt?.ValidateDestinationForPersisting(e.Row);
			}
		}

		protected override void FieldDependOnUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
		{
			var row = (INTran)e.Row;
			var costCenterSupport = GetCostCenterSupportByCostLayerType(row.CostLayerType);
			int newCostCenterID = (costCenterSupport?.IsSpecificCostCenter(row) == true)
				? costCenterSupport.GetCostCenterID(row)
				: CostCenter.FreeStock;
			if (row.CostCenterID != newCostCenterID)
			{
				cache.SetValueExt<INTran.costCenterID>(row, newCostCenterID);
			}
		}

		protected virtual void FieldDestinationDependOnUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
		{
			var row = (INTran)e.Row;
			var costCenterSupport = GetCostCenterSupportByCostLayerType(row.ToCostLayerType);
			int newCostCenterID = (costCenterSupport?.IsDestinationSpecificCostCenter(row) == true)
				? costCenterSupport.GetDestinationCostCenterID(row)
				: CostCenter.FreeStock;
			if (row.ToCostCenterID != newCostCenterID)
			{
				cache.SetValueExt<INTran.toCostCenterID>(row, newCostCenterID);
			}
		}
	}
}
