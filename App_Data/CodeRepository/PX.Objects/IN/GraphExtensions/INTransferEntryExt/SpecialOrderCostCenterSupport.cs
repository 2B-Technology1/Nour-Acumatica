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
using PX.Objects.Common.Extensions;
using PX.Objects.CS;
using PX.Objects.GL;

namespace PX.Objects.IN.GraphExtensions.INTransferEntryExt
{
	public class SpecialOrderCostCenterSupport : INRegisterEntryBaseExt.SpecialOrderCostCenterSupport<INTransferEntry>
	{
		public static bool IsActive()
			=> PXAccess.FeatureInstalled<CS.FeaturesSet.specialOrders>();

		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[SpecialOrderCostCenterSelector(typeof(INTran.inventoryID), typeof(INTran.siteID), typeof(INTran.invtMult),
			SOOrderTypeField = typeof(INTran.sOOrderType), SOOrderNbrField = typeof(INTran.sOOrderNbr),
			SOOrderLineNbrField = typeof(INTran.sOOrderLineNbr), IsSpecialOrderField = typeof(INTran.isSpecialOrder),
			CostCenterIDField = typeof(INTran.costCenterID), CostLayerTypeField = typeof(INTran.costLayerType),
			OrigModuleField = typeof(INTran.origModule), ReleasedField = typeof(INTran.released),
			ProjectIDField = typeof(INTran.projectID), TaskIDField = typeof(INTran.taskID), CostCodeIDField = typeof(INTran.costCodeID))]
		protected virtual void _(Events.CacheAttached<INTran.specialOrderCostCenterID> e) { }

		protected virtual void _(Events.RowSelected<INRegister> e)
		{
			if (e.Row == null)
				return;

			bool inModule = (e.Row.OrigModule == BatchModule.IN);
			bool oneStep = (e.Row.TransferType == INTransferType.OneStep);

			Base.transactions.Cache.AdjustUI()
				.For<INTran.specialOrderCostCenterID>(a => a.Visible = !inModule || oneStep)
				.For<INTran.toSpecialOrderCostCenterID>(a => a.Visible = oneStep);
		}

		protected override void _(Events.RowSelected<INTran> e)
		{
			base._(e);

			if (e.Row == null)
				return;

			var doc = Base.CurrentDocument.Current;
			bool inModule = (doc?.OrigModule == BatchModule.IN);
			bool oneStep = (doc?.TransferType == INTransferType.OneStep);
			bool released = (doc?.Released == true);

			e.Cache.Adjust<CostLayerType.ListAttribute>(e.Row)
				.For<INTran.costLayerType>(a => a.AllowSpecialOrders = !inModule || oneStep)
				.For<INTran.toCostLayerType>(a => a.AllowSpecialOrders = e.Row.CostLayerType == CostLayerType.Special);

			var numberOfValues = e.Cache.GetAttributes<INTran.costLayerType>(e.Row).OfType<CostLayerType.ListAttribute>()
				.FirstOrDefault()?.SetValues(e.Cache, e.Row);

			e.Cache.GetAttributes<INTran.toCostLayerType>(e.Row).OfType<CostLayerType.ListAttribute>()
				.FirstOrDefault()?.SetValues(e.Cache, e.Row);

			e.Cache.AdjustUI(e.Row)
				.For<INTran.costLayerType>(a => a.Enabled = (numberOfValues > 1 && !released && inModule))
				.For<INTran.toCostCodeID>(a => a.Enabled = e.Row.ToCostLayerType != CostLayerType.Special);

			if (inModule && !released)
			{
				e.Cache.AdjustUI().For<INTran.uOM>(a => a.Enabled = e.Row.CostLayerType != CostLayerType.Special);
			}
		}

		/// <summary>
		/// Overrides <see cref="INRegisterEntryBase.IsToProjectTaskEnabled(INTran)" />
		/// </summary>
		[PXOverride]
		public virtual (bool? ToProject, bool? ToTask) IsToProjectTaskEnabled(INTran row,
			Func<INTran, (bool? ToProject, bool? ToTask)> baseMethod)
		{
			var result = baseMethod(row);

			return ((result.ToProject ?? true) && row.ToCostLayerType != CostLayerType.Special,
					(result.ToTask ?? true) && row.ToCostLayerType != CostLayerType.Special);
		}

		protected virtual void _(Events.RowUpdated<INRegister> e)
		{
			if (!e.Cache.ObjectsEqual<INRegister.transferType>(e.OldRow, e.Row))
			{
				foreach (INTran transaction in Base.transactions.Select())
				{
					if (CostLayerType.Special.IsIn(transaction.CostLayerType, transaction.ToCostLayerType))
					{
						if (transaction.CostLayerType == CostLayerType.Special)
							transaction.CostLayerType = CostLayerType.Normal;

						if (transaction.ToCostLayerType == CostLayerType.Special)
							transaction.ToCostLayerType = CostLayerType.Normal;

						Base.transactions.Update(transaction);
					}
				}

				Base.transactions.View.RequestRefresh();
			}

			if (!e.Cache.ObjectsEqual<INRegister.siteID, INRegister.toSiteID>(e.OldRow, e.Row))
			{
				foreach (INTran transaction in Base.transactions.Select())
				{
					var tranCache = Base.transactions.Cache;
					tranCache.MarkUpdated(transaction);
					tranCache.VerifyFieldAndRaiseException<INTran.costLayerType>(transaction);
				}

				Base.transactions.View.RequestRefresh();
			}
		}

		protected virtual void _(Events.RowPersisting<INTran> e)
		{
			if (e.Operation.Command().IsNotIn(PXDBOperation.Insert, PXDBOperation.Update))
				return;

			e.Cache.VerifyFieldAndRaiseException<INTran.costLayerType>(e.Row);
		}

		protected virtual void _(Events.FieldVerifying<INTran, INTran.costLayerType> e)
		{
			var document = Base.CurrentDocument.Current;
			if (object.Equals(e.NewValue, CostLayerType.Special) && e.Row.OrigModule == BatchModule.IN && document != null)
			{
				if (document.TransferType != INTransferType.OneStep)
					throw new PXSetPropertyException(Messages.SpecialCostLayerOneStepTransfer);

				if (document.SiteID != null && document.ToSiteID != null && document.SiteID != document.ToSiteID)
					throw new PXSetPropertyException(Messages.SpecialCostLayerTransferToDifferentWarehouse);
			}
		}

		protected virtual void _(Events.FieldUpdated<INTran, INTran.costLayerType> e)
		{
			if (e.Row.CostLayerType != CostLayerType.Special && e.Row.ToCostLayerType == CostLayerType.Special)
				e.Cache.SetValueExt<INTran.toCostLayerType>(e.Row, CostLayerType.Normal);

			if (e.Row.CostLayerType == CostLayerType.Special && e.Row.ToCostLayerType.IsNotIn(CostLayerType.Special, CostLayerType.Normal))
				e.Cache.SetValueExt<INTran.toCostLayerType>(e.Row, CostLayerType.Normal);
		}
	}
}
