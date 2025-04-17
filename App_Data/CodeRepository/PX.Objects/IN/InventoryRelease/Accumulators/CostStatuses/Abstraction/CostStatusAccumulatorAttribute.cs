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
using PX.Common;
using PX.Data;
using PX.Objects.CS;

namespace PX.Objects.IN.InventoryRelease.Accumulators.CostStatuses.Abstraction
{
	using static PXDataFieldAssign.AssignBehavior;

	public class CostStatusAccumulatorAttribute : PXAccumulatorAttribute
	{
		protected Type _QuantityField;
		protected Type _CostField;
		protected Type _InventoryIDField;
		protected Type _SubItemIDField;
		protected Type _SiteIDField;
		protected Type _SpecificNumberField;
		protected Type _LayerTypeField;
		protected Type _ReceiptNbr;

		public CostStatusAccumulatorAttribute(Type quantityField, Type costField, Type inventoryIDField, Type subItemIDField, Type siteIDField, Type specificNumberField, Type layerTypeField, Type receiptNbr)
			: this()
		{
			_QuantityField = quantityField;
			_CostField = costField;
			_InventoryIDField = inventoryIDField;
			_SubItemIDField = subItemIDField;
			_SiteIDField = siteIDField;
			_SpecificNumberField = specificNumberField;
			_LayerTypeField = layerTypeField;
			_ReceiptNbr = receiptNbr;
			PersistOrder = PersistOrder.Regular;
		}

		public CostStatusAccumulatorAttribute(Type quantityField, Type costField, Type inventoryIDField, Type subItemIDField, Type siteIDField, Type layerTypeField, Type receiptNbr)
			: this(quantityField, costField, inventoryIDField, subItemIDField, siteIDField, null, layerTypeField, receiptNbr)
		{
		}

		protected CostStatusAccumulatorAttribute()
		{
			SingleRecord = true;
		}

		protected override bool PrepareInsert(PXCache cache, object row, PXAccumulatorCollection columns)
		{
			if (!base.PrepareInsert(cache, row, columns))
				return false;

			INCostStatus bal = (INCostStatus)row;
			if (bal.LayerType != INLayerType.Unmanaged)
			{
				columns.AppendException(_SpecificNumberField == null ? Messages.StatusCheck_QtyNegative : Messages.StatusCheck_QtyNegative2,
					new PXAccumulatorRestriction(_QuantityField.Name, PXComp.GE, 0m),
					new PXAccumulatorRestriction(_LayerTypeField.Name, PXComp.EQ, INLayerType.Oversold));
				columns.AppendException(_SpecificNumberField == null ? Messages.StatusCheck_QtyNegative : Messages.StatusCheck_QtyNegative2,
					new PXAccumulatorRestriction(_QuantityField.Name, PXComp.LE, 0m),
					new PXAccumulatorRestriction(_LayerTypeField.Name, PXComp.EQ, INLayerType.Normal));
				columns.AppendException(Messages.StatusCheck_QtyCostImblance,
					new PXAccumulatorRestriction(_QuantityField.Name, PXComp.NE, 0m),
					new PXAccumulatorRestriction(_CostField.Name, PXComp.EQ, 0m));
				columns.AppendException(Messages.StatusCheck_QtyCostImblance,
					new PXAccumulatorRestriction(_QuantityField.Name, PXComp.LE, 0m),
					new PXAccumulatorRestriction(_CostField.Name, PXComp.GE, 0m));
				columns.AppendException(Messages.StatusCheck_QtyCostImblance,
					new PXAccumulatorRestriction(_QuantityField.Name, PXComp.GE, 0m),
					new PXAccumulatorRestriction(_CostField.Name, PXComp.LE, 0m));
			}
			else
			{
				columns.AppendException(Messages.StatusCheck_QtyCostImblance,
					new PXAccumulatorRestriction(_QuantityField.Name, PXComp.NE, 0m));
				columns.AppendException(Messages.StatusCheck_QtyCostImblance,
					new PXAccumulatorRestriction(_CostField.Name, PXComp.NE, 0m));
			}

			columns.Update<INCostStatus.unitCost>(Replace);
			columns.Update<INCostStatus.origQty>(Initialize);
			columns.Update<INCostStatus.qtyOnHand>(Summarize);
			columns.Update<INCostStatus.totalCost>(Summarize);

			return true;
		}

		public override bool PersistInserted(PXCache cache, object row)
		{
			try
			{
				return base.PersistInserted(cache, row);
			}
			catch (PXRestrictionViolationException e)
			{
				List<object> pars = new List<object>();

				if (cache.BqlKeys.Contains(_InventoryIDField))
					pars.Add(PXForeignSelectorAttribute.GetValueExt(cache, row, _InventoryIDField.Name));

				if (cache.BqlKeys.Contains(_SubItemIDField))
					pars.Add(PXForeignSelectorAttribute.GetValueExt(cache, row, _SubItemIDField.Name));

				if (cache.BqlKeys.Contains(_SiteIDField))
					pars.Add(PXForeignSelectorAttribute.GetValueExt(cache, row, _SiteIDField.Name));

				if (e.Index.IsIn(0, 1) && _SpecificNumberField != null && cache.BqlKeys.Contains(_SpecificNumberField))
					pars.Add(PXForeignSelectorAttribute.GetValueExt(cache, row, _SpecificNumberField.Name));

				throw new PXException(
					e.Index.IsIn(0, 1)
						? _SpecificNumberField == null
							? Messages.StatusCheck_QtyNegative
							: Messages.StatusCheck_QtyNegative2
						: Messages.StatusCheck_QtyCostImblance,
					pars.ToArray());
			}
		}
	}
}
