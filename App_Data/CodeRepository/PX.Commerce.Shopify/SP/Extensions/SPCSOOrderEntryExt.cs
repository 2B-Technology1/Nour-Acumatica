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
using PX.Commerce.Core;
using PX.Data;
using PX.Objects.SO;
using PX.Common;
using PX.Data.WorkflowAPI;
using PX.Objects.SO.GraphExtensions.SOOrderEntryExt;
using PX.Objects.SO.Workflow.SalesOrder;
using System.Collections;
using PX.Objects.TX;
using PX.Commerce.Objects;
using System.Collections.Concurrent;

namespace PX.Commerce.Shopify
{
	public class SPCSOOrderEntryExt : PXGraphExtension<SOOrderEntry>
	{
		public static bool IsActive() { return CommerceFeaturesHelper.ShopifyConnector; }

		protected virtual void _(PX.Data.Events.RowSelected<SOLine> e)
		{
			SOLine row = e.Row;
			if (row == null) return;

			if (Base.Document.Current == null) return;

			SOOrder order = Base.Document.Current;
			if (!BCAPISyncScope.IsScoped())
			{
				BCSOOrderExt orderExt = order.GetExtension<BCSOOrderExt>();

				if (orderExt?.ExternalOrderExported == true)
				{
					BCSOLineExt lineExt = row.GetExtension<BCSOLineExt>();
					//Check the SO line whether exports to external platform
					if (!string.IsNullOrEmpty(lineExt?.ExternalRef))
					{
						PXUIFieldAttribute.SetEnabled<SOLine.orderQty>(e.Cache, row, false);
						PXUIFieldAttribute.SetEnabled<SOLine.curyUnitPrice>(e.Cache, row, false);
						PXUIFieldAttribute.SetEnabled<SOLine.curyExtPrice>(e.Cache, row, false);
						PXUIFieldAttribute.SetEnabled<SOLine.manualPrice>(e.Cache, row, false);
						PXUIFieldAttribute.SetEnabled<SOLine.discPct>(e.Cache, row, false);
						PXUIFieldAttribute.SetEnabled<SOLine.curyDiscAmt>(e.Cache, row, false);
						PXUIFieldAttribute.SetEnabled<SOLine.taxCategoryID>(e.Cache, row, false);
					}
				}
			}
		}

		protected virtual void _(PX.Data.Events.RowDeleting<SOLine> e)
		{
			if (Base.Document.Current == null) return;

			SOOrder order = Base.Document.Current;
			if (e.ExternalCall && !BCAPISyncScope.IsScoped()
				&& ((bool?)Base.Document.Cache.GetValue<BCSOOrderExt.externalOrderExported>(order) == true))
			{
				BCSOLineExt lineExt = e.Row.GetExtension<BCSOLineExt>();
				if (!string.IsNullOrEmpty(lineExt?.ExternalRef))
				{
					throw new PXSetPropertyException(BCMessages.OrderLineNotAllowDeletion, PXErrorLevel.RowError);
				}

			}
		}

		protected virtual void _(PX.Data.Events.RowUpdated<SOLine> e)
		{
			if (Base.Document.Current == null) return;

			List<Type> monitoringTypes = new List<Type>();
			monitoringTypes.Add(typeof(SOLine.orderQty));
			monitoringTypes.Add(typeof(SOLine.curyUnitPrice));
			monitoringTypes.Add(typeof(SOLine.curyExtPrice));
			monitoringTypes.Add(typeof(SOLine.manualPrice));
			monitoringTypes.Add(typeof(SOLine.discPct));
			monitoringTypes.Add(typeof(SOLine.inventoryID));
			monitoringTypes.Add(typeof(SOLine.curyDiscAmt));
			monitoringTypes.Add(typeof(SOLine.curyLineAmt));
			monitoringTypes.Add(typeof(SOLine.taxCategoryID));

			if (e.ExternalCall && !BCAPISyncScope.IsScoped()
				&& ((bool?)Base.Document.Cache.GetValue<BCSOOrderExt.externalOrderOriginal>(Base.Document.Current) == true)
				&& monitoringTypes.Any(t => !object.Equals(e.Cache.GetValue(e.Row, t.Name), e.Cache.GetValue(e.OldRow, t.Name))))
			{
				Base.Document.Cache.SetValueExt<BCSOOrderExt.externalOrderOriginal>(Base.Document.Current, false);
			}
		}

		protected virtual void _(PX.Data.Events.RowSelecting<SOLine> e)
		{
			SetExcludeFromExtport(e.Cache, e.Row);
		}

		protected virtual void _(PX.Data.Events.RowInserted<SOLine> e)
		{
			SetExcludeFromExtport(e.Cache, e.Row);
		}

		protected virtual void _(PX.Data.Events.RowSelected<SOTaxTran> e)
		{
			SOTaxTran row = e.Row;
			if (row == null) return;

			if (Base.Document.Current == null) return;

			SOOrder order = Base.Document.Current;
			if (!BCAPISyncScope.IsScoped())
			{
				BCSOOrderExt orderExt = order.GetExtension<BCSOOrderExt>();

				if (orderExt?.ExternalOrderExported == true)
				{
					PXUIFieldAttribute.SetEnabled<SOTaxTran.curyTaxableAmt>(e.Cache, row, false);
					PXUIFieldAttribute.SetEnabled<SOTaxTran.curyTaxAmt>(e.Cache, row, false);
				}
			}
		}

		protected virtual void _(PX.Data.Events.FieldUpdated<SOTaxTran.curyTaxAmt> e)
		{
			ResetExternalOrderOriginal(e.ExternalCall);
		}

		protected virtual void _(PX.Data.Events.RowSelected<SOOrderDiscountDetail> e)
		{
			SOOrderDiscountDetail row = e.Row;
			if (row == null) return;

			if (Base.Document.Current == null) return;

			SOOrder order = Base.Document.Current;
			if (!BCAPISyncScope.IsScoped() && !string.IsNullOrEmpty(order.CustomerRefNbr))
			{
				BCSOOrderExt orderExt = order.GetExtension<BCSOOrderExt>();

				if (orderExt?.ExternalOrderExported == true)
				{
					PXUIFieldAttribute.SetEnabled<SOOrderDiscountDetail.isManual>(e.Cache, row, false);
					PXUIFieldAttribute.SetEnabled<SOOrderDiscountDetail.freeItemID>(e.Cache, row, false);
					PXUIFieldAttribute.SetEnabled<SOOrderDiscountDetail.freeItemQty>(e.Cache, row, false);
					PXUIFieldAttribute.SetEnabled<SOOrderDiscountDetail.curyDiscountAmt>(e.Cache, row, false);
					PXUIFieldAttribute.SetEnabled<SOOrderDiscountDetail.discountPct>(e.Cache, row, false);
				}
			}
		}

		protected virtual void _(PX.Data.Events.FieldUpdated<SOOrderDiscountDetail.curyDiscountAmt> e)
		{
			ResetExternalOrderOriginal(e.ExternalCall);
		}

		protected virtual void _(PX.Data.Events.FieldUpdated<SOOrderDiscountDetail.discountPct> e)
		{
			ResetExternalOrderOriginal(e.ExternalCall);
		}

		protected virtual void _(PX.Data.Events.RowSelected<SOOrder> e)
		{
			SOOrder row = e.Row;
			if (row == null) return;

			BCSOOrderExt orderExt = row.GetExtension<BCSOOrderExt>();
			if (!BCAPISyncScope.IsScoped() && orderExt?.ExternalOrderExported == true)
			{
				PXUIFieldAttribute.SetEnabled<SOOrder.overrideFreightAmount>(e.Cache, row, false);
				PXUIFieldAttribute.SetEnabled<SOOrder.curyFreightAmt>(e.Cache, row, false);
				PXUIFieldAttribute.SetEnabled<SOOrder.curyPremiumFreightAmt>(e.Cache, row, false);

				Base.recalculateDiscountsAction.SetEnabled(false);
				Base.Taxes.AllowDelete = Base.Taxes.AllowInsert = false;
				Base.DiscountDetails.AllowDelete = Base.DiscountDetails.AllowInsert = false;

				//If the value is true, the Add and Delete action in the Details tab of the Sales Order (BC301000) form will be enabled,and the user would be able to add items to synchronized orders.
				//Otherwise the Add and Delete action will be disabled in the Details tab
				if (orderExt?.AllowModifyingItems != true)
				{
					Base.Transactions.AllowInsert = Base.Transactions.AllowDelete = false;
				}
			}
			
			PXUIFieldAttribute.SetVisible<BCSOLineExt.excludedFromExport>(Base.Transactions.Cache, null, orderExt?.ExternalOrderExported == true);
		}

		protected virtual void _(PX.Data.Events.RowSelecting<SOOrder> e)
		{
			SOOrder row = e.Row;
			if (row == null) return;

			if (!BCAPISyncScope.IsScoped() && !string.IsNullOrEmpty(row.CustomerRefNbr))
			{
				BCSOOrderExt orderExt = row.GetExtension<BCSOOrderExt>();
				
				if (orderExt?.ExternalOrderExported != true)
				{
					BCSyncStatus syncStatus = null;
					BCBindingExt bindingExt = null;
					BCEntity entity = null;
					using (new PXConnectionScope())
					{
						var result = PXSelectReadonly2<BCSyncStatus,
										InnerJoin<BCBindingExt, On<BCBindingExt.bindingID, Equal<BCSyncStatus.bindingID>>,
										LeftJoin<BCEntity, On<BCEntity.connectorType, Equal<BCSyncStatus.connectorType>,
											And<BCEntity.entityType, Equal<BCSyncStatus.entityType>, And<BCEntity.bindingID, Equal<BCSyncStatus.bindingID>>>>>>,
										Where<BCSyncStatus.connectorType, Equal<Required<BCSyncStatus.connectorType>>,
											And<BCSyncStatus.entityType, Equal<Required<BCSyncStatus.entityType>>,
											And<BCSyncStatus.localID, Equal<Required<BCSyncStatus.localID>>>>>>
										.Select(Base, ShopifyConstants.ShopifyConnector, BCEntitiesAttribute.Order, row.NoteID).FirstOrDefault();
						if (result != null)
						{
							syncStatus = result.GetItem<BCSyncStatus>();
							bindingExt = result.GetItem<BCBindingExt>();
							entity = result.GetItem<BCEntity>();
						}
					}

					e.Cache.SetValueExt<BCSOOrderExt.externalOrderExported>(row, entity?.Direction != BCSyncDirectionAttribute.Import && syncStatus?.ExternID != null && syncStatus?.LocalID != null);
					e.Cache.SetValueExt<BCSOOrderExt.allowModifyingItems>(row, bindingExt?.AllowOrderEdit == true);
				}
			}
			
		}

		protected virtual void SetExcludeFromExtport(PXCache cache, SOLine row)
		{
			if (Base.Document.Current == null) return;

			if (!BCAPISyncScope.IsScoped()
				&& ((bool?)Base.Document.Cache.GetValue<BCSOOrderExt.externalOrderExported>(Base.Document.Current) == true))
			{
				cache.SetValueExt<BCSOLineExt.excludedFromExport>(row, string.IsNullOrEmpty(row.GetExtension<BCSOLineExt>()?.ExternalRef));
			}
		}

		protected virtual void ResetExternalOrderOriginal(bool externalCall)
		{
			if (Base.Document.Current == null) return;

			if (externalCall && !BCAPISyncScope.IsScoped()
				&& ((bool?)Base.Document.Cache.GetValue<BCSOOrderExt.externalOrderOriginal>(Base.Document.Current) == true))
			{
				Base.Document.Cache.SetValueExt<BCSOOrderExt.externalOrderOriginal>(Base.Document.Current, false);
			}
		}

	}
}
