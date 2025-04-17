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
using PX.Common;
using PX.Data;
using PX.Objects.IN.InventoryRelease;
using PX.Objects.IN.InventoryRelease.DAC;

namespace PX.Objects.IN.GraphExtensions.INReleaseProcessExt
{
	public class SpecialOrderCostCenterSupport : PXGraphExtension<INReleaseProcess>
	{
		public static bool IsActive()
			=> PXAccess.FeatureInstalled<CS.FeaturesSet.specialOrders>();

		/// <summary>
		/// Overrides <see cref="INReleaseProcess.UpdateSplitDestinationLocation"/>
		/// </summary>
		[PXOverride]
		public virtual void UpdateSplitDestinationLocation(INTran tran, INTranSplit split, int? value, Action<INTran, INTranSplit, int?> baseMethod)
		{
			baseMethod(tran, split, value);

			if (split.SkipCostUpdate == true && CostLayerType.Special.IsIn(tran.CostLayerType, tran.ToCostLayerType))
			{
				if (tran.CostCenterID != tran.ToCostCenterID)
				{
					split.SkipCostUpdate = false;
					Base.intransplit.Cache.MarkUpdated(split, assertError: true);
				}
			}
		}

		/// <summary>
		/// Overrides <see cref="INReleaseProcess.CreatePositiveOneStepTransferINTran"/>
		/// </summary>
		[PXOverride]
		public virtual INTran CreatePositiveOneStepTransferINTran(INRegister doc, INTran tran, INTranSplit split,
			Func<INRegister, INTran, INTranSplit, INTran> baseFunc)
		{
			INTran newtran = baseFunc(doc, tran, split);

			newtran.SpecialOrderCostCenterID = (newtran.ToCostLayerType == CostLayerType.Special) ? newtran.ToCostCenterID : null;
			newtran.IsSpecialOrder = (newtran.ToCostLayerType == CostLayerType.Special);
			newtran.ToSpecialOrderCostCenterID = null;

			return newtran;
		}

		/// <summary>
		/// Overrides <see cref="INReleaseProcess.UseStandardCost"/>
		/// </summary>
		[PXOverride]
		public virtual bool UseStandardCost(string valMethod, INTran tran, Func<string, INTran, bool> baseFunc)
			=> baseFunc(valMethod, tran) && tran.CostLayerType != CostLayerType.Special;

		/// <summary>
		/// Overrides <see cref="INReleaseProcess.AccumOversoldCostStatus(INTran, INTranSplit, InventoryItem)"/>
		/// </summary>
		[PXOverride]
		public virtual INCostStatus AccumOversoldCostStatus(INTran tran, INTranSplit split, InventoryItem item,
			Func<INTran, INTranSplit, InventoryItem, INCostStatus> baseFunc)
		{
			if (tran.CostLayerType == CostLayerType.Special)
			{
				throw new PXException(Messages.SpecialCostLayerNegativeQty, item.InventoryCD, tran.SOOrderNbr);
			}

			return baseFunc(tran, split, item);
		}

		/// <summary>
		/// Overrides <see cref="INReleaseProcess.ThrowNegativeQtyException"/>
		/// </summary>
		[PXOverride]
		public virtual void ThrowNegativeQtyException(INTran tran, INTranSplit split, INCostStatus lastLayer,
			Action<INTran, INTranSplit, INCostStatus> baseImpl)
		{
			if (tran.CostLayerType == CostLayerType.Special)
			{
				InventoryItem item = InventoryItem.PK.Find(Base, tran.InventoryID);

				throw new PXException(Messages.SpecialCostLayerNegativeQty, item?.InventoryCD, tran.SOOrderNbr);
			}

			baseImpl(tran, split, lastLayer);
		}

		/// <summary>
		/// Overrides <see cref="INReleaseProcess.GetTransitCostSiteID(INTran)"/>
		/// </summary>
		[PXOverride]
		public virtual int? GetTransitCostSiteID(INTran tran, Func<INTran, int?> baseMethod)
		{
			if (tran.IsSpecialOrder == true && !Base.IsOneStepTransfer())
			{
				if (Base.IsIngoingTransfer(tran))
				{
					return tran.CostCenterID;
				}
				else if (tran.TranType == INTranType.Transfer)
				{
					return tran.ToCostCenterID;
				}
			}

			return baseMethod(tran);
		}

		/// Overrides <see cref="INReleaseProcess.Copy(INTran, ReadOnlyCostStatus, InventoryItem)"/>
		[PXOverride]
		public virtual INTran Copy(INTran tran, ReadOnlyCostStatus layer, InventoryItem item,
			Func<INTran, ReadOnlyCostStatus, InventoryItem, INTran> base_Copy)
		{
			INTran newtran = base_Copy(tran, layer, item);
			newtran.IsSpecialOrder = tran.IsSpecialOrder;
			return newtran;
		}
	}
}
