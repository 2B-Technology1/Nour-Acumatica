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
using PX.Objects.GL;
using PX.Objects.IN;
using PX.Objects.SO;

namespace PX.Objects.IN.GraphExtensions.INRegisterEntryBaseExt
{
	public abstract class SpecialOrderCostCenterSupport<T> : SpecialOrderCostCenterSupport<T, INTran>, IINTranCostCenterSupport
		where T : INRegisterEntryBase
	{		
		public bool IsSupported(string layerType)
		{
			return layerType == CostLayerType.Special;
		}

		public string GetCostLayerType(INTran tran)
		{
			return CostLayerType.Special;
		}

		public override IEnumerable<Type> GetFieldsDependOn()
		{
			yield return typeof(INTran.isSpecialOrder);
			yield return typeof(INTran.siteID);
			yield return typeof(INTran.sOOrderType);
			yield return typeof(INTran.sOOrderNbr);
			yield return typeof(INTran.sOOrderLineNbr);
		}

		public override bool IsSpecificCostCenter(INTran tran)
		{
			return tran.IsSpecialOrder == true
				&& tran.SiteID != null
				&& !string.IsNullOrEmpty(tran.SOOrderType)
				&& !string.IsNullOrEmpty(tran.SOOrderNbr)
				&& tran.SOOrderLineNbr != null;
		}

		protected override CostCenterKeys GetCostCenterKeys(INTran line)
		{
			if (line.DocType == INDocType.Issue && line.InvtMult == 1)
			{
				var soReturnLine = SOLine.PK.Find(Base, line.SOOrderType, line.SOOrderNbr, line.SOOrderLineNbr);
				if (string.IsNullOrEmpty(soReturnLine?.OrigOrderType)
					|| string.IsNullOrEmpty(soReturnLine.OrigOrderNbr)
					|| soReturnLine.OrigLineNbr == null)
				{
					throw new PXInvalidOperationException();
				}
				return new CostCenterKeys
				{
					SiteID = line.SiteID,
					OrderType = soReturnLine.OrigOrderType,
					OrderNbr = soReturnLine.OrigOrderNbr,
					LineNbr = soReturnLine.OrigLineNbr,
				};
			}
			else if (line.DocType == INDocType.Transfer && line.OrigModule == BatchModule.SO)
			{
				var soTransferLine = SOLine.PK.Find(Base, line.SOOrderType, line.SOOrderNbr, line.SOOrderLineNbr);
				if (string.IsNullOrEmpty(soTransferLine?.OrigOrderType)
					|| string.IsNullOrEmpty(soTransferLine.OrigOrderNbr)
					|| soTransferLine.OrigLineNbr == null)
				{
					throw new PXInvalidOperationException();
				}
				return new CostCenterKeys
				{
					SiteID = line.SiteID,
					OrderType = soTransferLine.OrigOrderType,
					OrderNbr = soTransferLine.OrigOrderNbr,
					LineNbr = soTransferLine.OrigLineNbr,
				};
			}
			else
			{
				return new CostCenterKeys
				{
					SiteID = line.SiteID,
					OrderType = line.SOOrderType,
					OrderNbr = line.SOOrderNbr,
					LineNbr = line.SOOrderLineNbr,
				};
			}
		}

		public virtual IEnumerable<Type> GetDestinationFieldsDependOn()
		{
			yield return typeof(INTran.isSpecialOrder);
			yield return typeof(INTran.toSiteID);
			yield return typeof(INTran.sOOrderType);
			yield return typeof(INTran.sOOrderNbr);
			yield return typeof(INTran.sOOrderLineNbr);
		}

		public virtual bool IsDestinationSpecificCostCenter(INTran tran)
		{
			return tran.IsSpecialOrder == true
				&& tran.ToSiteID != null
				&& !string.IsNullOrEmpty(tran.SOOrderType)
				&& !string.IsNullOrEmpty(tran.SOOrderNbr)
				&& tran.SOOrderLineNbr != null;
		}

		protected virtual CostCenterKeys GetDestinationCostCenterKeys(INTran line)
		{
			if (line.DocType == INDocType.Transfer && line.OrigModule == BatchModule.SO)
			{
				var soTransferLine = SOLine.PK.Find(Base, line.SOOrderType, line.SOOrderNbr, line.SOOrderLineNbr);
				if (string.IsNullOrEmpty(soTransferLine?.OrigOrderType)
					|| string.IsNullOrEmpty(soTransferLine.OrigOrderNbr)
					|| soTransferLine.OrigLineNbr == null)
				{
					throw new PXInvalidOperationException();
				}
				return new CostCenterKeys
				{
					SiteID = line.ToSiteID,
					OrderType = soTransferLine.OrigOrderType,
					OrderNbr = soTransferLine.OrigOrderNbr,
					LineNbr = soTransferLine.OrigLineNbr,
				};
			}

			return new CostCenterKeys
			{
				SiteID = line.ToSiteID,
				OrderType = line.SOOrderType,
				OrderNbr = line.SOOrderNbr,
				LineNbr = line.SOOrderLineNbr,
			};
		}

		public virtual int GetDestinationCostCenterID(INTran tran)
		{
			return (int)FindOrCreateCostCenter(GetDestinationCostCenterKeys(tran));
		}

		public virtual void OnCostLayerTypeChanged(INTran tran, string newCostLayerType)
		{
			var cache = Base.Caches<INTran>();
			cache.SetValueExt<INTran.isSpecialOrder>(tran, false);
			cache.SetValueExt<INTran.specialOrderCostCenterID>(tran, null);
			cache.SetValueExt<INTran.sOOrderType>(tran, null);
			cache.SetValueExt<INTran.sOOrderNbr>(tran, null);
			cache.SetValueExt<INTran.sOOrderLineNbr>(tran, null);
		}

		public virtual void OnDestinationCostLayerTypeChanged(INTran tran, string newCostLayerType)
		{
			Base.Caches<INTran>().SetValueExt<INTran.toSpecialOrderCostCenterID>(tran, null);
		}

		public virtual void ValidateForPersisting(INTran tran)
		{
			if (!IsSpecificCostCenter(tran))
			{
				Base.Caches<INTran>().RaiseExceptionHandling<INTran.specialOrderCostCenterID>(tran, null,
					new PXSetPropertyException(Messages.SpecaiCostLayerOrderNbr, PXErrorLevel.Error));

				throw new PXRowPersistingException(nameof(INTran.specialOrderCostCenterID),
					tran.SpecialOrderCostCenterID, Messages.SpecaiCostLayerOrderNbr);
			}
		}

		public virtual void ValidateDestinationForPersisting(INTran tran)
		{
			if (!IsDestinationSpecificCostCenter(tran))
			{
				Base.Caches<INTran>().RaiseExceptionHandling<INTran.toSpecialOrderCostCenterID>(tran, null,
					new PXSetPropertyException(Messages.SpecaiCostLayerToOrderNbr, PXErrorLevel.Error));

				throw new PXRowPersistingException(nameof(INTran.toSpecialOrderCostCenterID),
					tran.ToSpecialOrderCostCenterID, Messages.SpecaiCostLayerToOrderNbr);
			}
		}

		/// <summary>
		/// Overrides <see cref="INRegisterEntryBase.IsProjectTaskEnabled(INTran)" />
		/// </summary>
		[PXOverride]
		public virtual (bool? Project, bool? Task) IsProjectTaskEnabled(INTran row,
			Func<INTran, (bool? Project, bool? Task)> baseMethod)
		{
			var result = baseMethod(row);

			return ((result.Project ?? true) && row.CostLayerType != CostLayerType.Special,
					(result.Task ?? true) && row.CostLayerType != CostLayerType.Special);
		}

		protected virtual void _(Events.RowSelected<INTran> e)
		{
			if (e.Row == null)
				return;

			PXUIFieldAttribute.SetEnabled<INTran.costCodeID>(e.Cache, e.Row, e.Row.CostLayerType != CostLayerType.Special);
		}

		protected virtual void _(Events.FieldUpdated<INTran, INTran.specialOrderCostCenterID> e)
		{
			if (Equals(e.OldValue, e.Row.SpecialOrderCostCenterID) ||
				Base.INRegisterDataMember.Current?.OrigModule.IsIn(BatchModule.IN, INRegister.origModule.PI) != true)
			{
				return;
			}

			// PI adjustment disables UOM
			bool isPIAdjust = (Base.INRegisterDataMember.Current.OrigModule == INRegister.origModule.PI);
			if (e.Row.SpecialOrderCostCenterID == null)
			{
				if (!isPIAdjust) e.Cache.SetDefaultExt<INTran.uOM>(e.Row);
				e.Cache.SetDefaultExt<INTran.unitCost>(e.Row);
				return;
			}
			
			var costCenter = INCostCenter.PK.Find(Base, e.Row.SpecialOrderCostCenterID);
			if (costCenter == null)
				throw new Common.Exceptions.RowNotFoundException(Base.Caches<INCostCenter>(), e.Row.SpecialOrderCostCenterID);

			var soLine = SOLine.PK.Find(Base, costCenter.SOOrderType, costCenter.SOOrderNbr, costCenter.SOOrderLineNbr);
			if (soLine == null)
			{
				throw new Common.Exceptions.RowNotFoundException(Base.Caches<SOLine>(),
					costCenter?.SOOrderType, costCenter?.SOOrderNbr, costCenter?.SOOrderLineNbr);
			}

			if (isPIAdjust)
			{
				decimal? unitCost = INUnitAttribute.ConvertFromTo<INTran.inventoryID>(e.Cache, e.Row, e.Row.UOM, soLine.UOM, (decimal)soLine.UnitCost, INPrecision.UNITCOST);
				e.Cache.SetValueExt<INTran.unitCost>(e.Row, unitCost);
			}
			else
			{
				e.Cache.SetValueExt<INTran.uOM>(e.Row, soLine.UOM);
				e.Cache.SetValueExt<INTran.unitCost>(e.Row, soLine.UnitCost);
			}
		}
	}
}
