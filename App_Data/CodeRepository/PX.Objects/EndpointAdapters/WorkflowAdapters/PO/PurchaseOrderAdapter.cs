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
using PX.Api;
using PX.Api.ContractBased;
using PX.Api.ContractBased.Models;
using PX.Data;
using PX.Objects.PO;

namespace PX.Objects.EndpointAdapters.WorkflowAdapters.PO
{
	[PXVersion("17.200.001", "Default")]
	[PXVersion("18.200.001", "Default")]
	[PXVersion("20.200.001", "Default")]
	[PXVersion("22.200.001", "Default")]
	[PXVersion("23.200.001", "Default")]
	internal class PurchaseOrderAdapter
	{
		[FieldsProcessed(new[] {
			"Type",
			"OrderNbr",
			"Hold"
		})]
		protected virtual void PurchaseOrder_Insert(PXGraph graph, EntityImpl entity, EntityImpl targetEntity)
		{
			var orderGraph = (POOrderEntry)graph;

			var typeField = targetEntity.Fields.SingleOrDefault(f => f.Name == "Type") as EntityValueField;
			var orderNbrField = targetEntity.Fields.SingleOrDefault(f => f.Name == "OrderNbr") as EntityValueField;
			var holdField = targetEntity.Fields.SingleOrDefault(f => f.Name == "Hold") as EntityValueField;

			var poOrder = (POOrder)orderGraph.Document.Cache.CreateInstance();

			if (!string.IsNullOrEmpty(typeField?.Value))
			{
				//we have a problem when set "Normal" instead of "RO" - the system set "No" value
				string orderType;
				if (!new POOrderType.ListAttribute().TryGetValue(typeField.Value, out orderType))
					orderType = typeField?.Value;

				orderGraph.Document.Cache.SetValueExt<POOrder.orderType>(poOrder, orderType);
			}

			if (orderNbrField != null)
				poOrder.OrderNbr = orderNbrField.Value;

			orderGraph.Document.Current = orderGraph.Document.Insert(poOrder);
			orderGraph.SubscribeToPersistDependingOnBoolField(holdField, orderGraph.putOnHold, orderGraph.releaseFromHold);
		}

		[FieldsProcessed(new[] {
			"Hold"
		})]
		protected virtual void PurchaseOrder_Update(PXGraph graph, EntityImpl entity, EntityImpl targetEntity)
		{
			var orderGraph = (POOrderEntry)graph;

			if (orderGraph.Document.Current == null || orderGraph.Document.Current.Behavior == POBehavior.ChangeOrder)
				return;

			var holdField = targetEntity.Fields.SingleOrDefault(f => f.Name == "Hold") as EntityValueField;
			orderGraph.SubscribeToPersistDependingOnBoolField(holdField, orderGraph.putOnHold, orderGraph.releaseFromHold);
		}
	}
}
