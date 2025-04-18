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
using PX.Data;
using PX.Objects.CS;
using PX.Objects.GL;
using PX.Objects.IN;

namespace PX.Objects.RUTROT
{
	[Obsolete(Common.Messages.ItemIsObsoleteAndWillBeRemoved2021R2)]
	public class NonStockItemMaintRUTROT : PXGraphExtension<NonStockItemMaint>
	{
		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<FeaturesSet.rutRotDeduction>();
		}

		#region Events
		protected virtual void InventoryItem_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			InventoryItem item = e.Row as InventoryItem;
			if (item == null)
				return;
			InventoryItemRUTROT itemRR = PXCache<InventoryItem>.GetExtension<InventoryItemRUTROT>(item);
			Branch branch = Base.CurrentBranch.SelectSingle(PXAccess.GetBranchID());
			BranchRUTROT branchRR = RUTROTHelper.GetExtensionNullable<Branch, BranchRUTROT>(branch);

			bool allowRUTROT = branchRR?.AllowsRUTROT == true;
			bool isOtherCostOrNull = itemRR.RUTROTItemType == null || itemRR.RUTROTItemType == RUTROTItemTypes.OtherCost;

			PXUIFieldAttribute.SetVisible<InventoryItemRUTROT.isRUTROTDeductible>(sender, item, allowRUTROT);
			PXUIFieldAttribute.SetVisible<InventoryItemRUTROT.isRUTROTDeductible>(Base.Item.Cache, item, allowRUTROT);
			PXUIFieldAttribute.SetVisible<InventoryItemRUTROT.rUTROTItemType>(Base.Item.Cache, item, allowRUTROT);
			PXUIFieldAttribute.SetVisible<InventoryItemRUTROT.rUTROTType>(Base.Item.Cache, item, allowRUTROT);
			PXUIFieldAttribute.SetVisible<InventoryItemRUTROT.rUTROTWorkTypeID>(Base.Item.Cache, item, allowRUTROT);

			PXUIFieldAttribute.SetEnabled<InventoryItemRUTROT.rUTROTWorkTypeID>(sender, item, !isOtherCostOrNull && itemRR.IsRUTROTDeductible == true);
			PXUIFieldAttribute.SetEnabled<InventoryItemRUTROT.rUTROTItemType>(sender, item, itemRR.IsRUTROTDeductible == true);
			PXUIFieldAttribute.SetEnabled<InventoryItemRUTROT.rUTROTType>(sender, item, itemRR.IsRUTROTDeductible == true);
			if (!isOtherCostOrNull && !RUTROTHelper.IsUpToDateWorkType(itemRR.RUTROTWorkTypeID, this.Base.Accessinfo.BusinessDate ?? DateTime.Now, this.Base))
			{
				sender.RaiseExceptionHandling<InventoryItemRUTROT.rUTROTWorkTypeID>(item, 
					itemRR.RUTROTWorkTypeID, 
					new PXSetPropertyException(RUTROTMessages.ObsoleteWorkTypeWarning, PXErrorLevel.Warning));
			}
			else
			{
				sender.RaiseExceptionHandling<InventoryItemRUTROT.rUTROTWorkTypeID>(item, itemRR.RUTROTWorkTypeID, null);
			}
		}

		protected virtual void InventoryItem_IsRUTROTDeductible_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			InventoryItem row = (InventoryItem)e.Row;
			if (row == null)
				return;
			InventoryItemRUTROT itemRR = PXCache<InventoryItem>.GetExtension<InventoryItemRUTROT>(row);
			if (itemRR.RUTROTType == null)
			{
				sender.SetDefaultExt<InventoryItemRUTROT.rUTROTType>(row);
			}
		}

		protected virtual void InventoryItem_BaseUnit_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			CheckProperUnitOfMeasure(sender, e);
		}

		protected virtual void InventoryItem_RUTROTItemType_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			InventoryItem row = (InventoryItem)e.Row;
			InventoryItemRUTROT itemRR = RUTROTHelper.GetExtensionNullable<InventoryItem, InventoryItemRUTROT>(row);
			if (row != null && itemRR?.RUTROTItemType == RUTROTItemTypes.OtherCost)
			{
				sender.SetValueExt<InventoryItemRUTROT.rUTROTWorkTypeID>(row, null);
			}
			CheckProperUnitOfMeasure(sender, e);
		}

		protected virtual void InventoryItem_RUTROTType_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			InventoryItem row = (InventoryItem)e.Row;
			InventoryItemRUTROT itemRR = RUTROTHelper.GetExtensionNullable<InventoryItem, InventoryItemRUTROT>(row);
			if (row != null && itemRR?.RUTROTType != (string)e.OldValue)
			{
				sender.SetValueExt<InventoryItemRUTROT.rUTROTWorkTypeID>(row, null);
			}
		}

		private static void CheckProperUnitOfMeasure(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			InventoryItem row = (InventoryItem)e.Row;
			InventoryItemRUTROT itemRR = RUTROTHelper.GetExtensionNullable<InventoryItem, InventoryItemRUTROT>(row);
			if (row != null && itemRR?.RUTROTItemType == RUTROTItemTypes.Service)
			{
				sender.RaiseExceptionHandling<InventoryItemRUTROT.rUTROTItemType>(row, 
					itemRR.RUTROTItemType, 
					new PXSetPropertyException(RUTROTMessages.ServiceMustBeHour, PXErrorLevel.Warning));
			}
		}
		#endregion
	}
}
