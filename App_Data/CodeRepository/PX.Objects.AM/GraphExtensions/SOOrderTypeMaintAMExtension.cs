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
using PX.Objects.SO;

namespace PX.Objects.AM.CacheExtensions
{
	public class SOOrderTypeMaintAMExtension : PXGraphExtension<SOOrderTypeMaint>
	{
		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<CS.FeaturesSet.manufacturing>();
		}

       	protected virtual void _(Events.RowSelected<SOOrderType> e)
		{
			if (e.Row == null)
			{
				return;
			}
			
			bool isBlanket = e.Row.Behavior == SOBehavior.BL;
			if (isBlanket)
			{
				e.Cache.Adjust<PXUIFieldAttribute>(e.Row)
					.For<SOOrderTypeExt.aMProductionOrderEntry>(a =>
					{
						a.Visible = a.Enabled = false;
					})
					.SameFor<SOOrderTypeExt.aMProductionOrderEntryOnHold>()
					.SameFor<SOOrderTypeExt.aMEstimateEntry>()
					.SameFor<SOOrderTypeExt.aMEnableWarehouseLinkedProduction>()
					.SameFor<SOOrderTypeExt.aMMTOOrder>()
					.SameFor<SOOrderTypeExt.aMConfigurationEntry>();
			}

            var rowExt = PXCache<SOOrderType>.GetExtension<SOOrderTypeExt>(e.Row);
			var productionAllowed = e.Row.RequireShipping.GetValueOrDefault();

			PXUIFieldAttribute.SetEnabled<SOOrderTypeExt.aMProductionOrderEntry>(e.Cache, e.Row, productionAllowed);
			PXUIFieldAttribute.SetEnabled<SOOrderTypeExt.aMProductionOrderEntryOnHold>(e.Cache, e.Row, productionAllowed);

			var shipableProductionOrderType = (rowExt?.AMProductionOrderEntry == true || rowExt?.AMProductionOrderEntryOnHold == true) && productionAllowed;
			PXUIFieldAttribute.SetEnabled<SOOrderTypeExt.aMMTOOrder>(e.Cache, e.Row, shipableProductionOrderType || e.Row.Behavior == SOBehavior.QT);
		}

		protected virtual void _(Events.RowUpdated<SOOrderType> e)
		{
			if (e.Row == null || e.Row.Behavior == SOBehavior.QT)
			{
				return;
			}

			var rowExt = PXCache<SOOrderType>.GetExtension<SOOrderTypeExt>(e.Row);
			var productionAllowed = e.Row.RequireShipping.GetValueOrDefault();

			if (rowExt.AMProductionOrderEntry == true && !productionAllowed)
			{
				rowExt.AMProductionOrderEntry = false;
			}

			if (rowExt.AMProductionOrderEntryOnHold == true && !productionAllowed)
			{
				rowExt.AMProductionOrderEntryOnHold = false;
			}

			if (rowExt?.AMProductionOrderEntry != true && rowExt?.AMProductionOrderEntryOnHold != true)
			{
				rowExt.AMMTOOrder = false;
			}
		}


		protected virtual void _(Events.FieldVerifying<SOOrderType, SOOrderTypeExt.aMEstimateEntry> e)
		{
			if (e.Row == null)
				return;

			if ((bool?)e.NewValue == true && e.Row.DisableAutomaticTaxCalculation == true)
			{
				throw new PXSetPropertyException(Messages.EstimatesAndDisableTaxCalculationConflictSOOrderType);
			}
		}

		protected virtual void _(Events.FieldVerifying<SOOrderType, SOOrderType.disableAutomaticTaxCalculation> e)
		{
			if (e.Row == null)
				return;

			var row = PXCache<SOOrderType>.GetExtension<SOOrderTypeExt>(e.Row);
			if ((bool?)e.NewValue == true && row.AMEstimateEntry == true)
			{
				throw new PXSetPropertyException(Messages.EstimatesAndDisableTaxCalculationConflictSOOrderType);
			}
		}
	}
}
