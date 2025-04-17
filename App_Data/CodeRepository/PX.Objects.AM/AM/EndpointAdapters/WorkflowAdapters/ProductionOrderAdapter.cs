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
using PX.Api.ContractBased;
using PX.Api;
using PX.Data;
using PX.Api.ContractBased.Models;
using PX.Objects.EndpointAdapters;

namespace PX.Objects.AM.EndpointAdapters.WorkflowAdapters
{
	[PXVersion("23.100.001", "MANUFACTURING")]
	[PXVersion("21.200.001", "MANUFACTURING")]
	[PXVersion("20.200.001", "MANUFACTURING")]
	internal class ProductionOrderAdapter
	{
		[FieldsProcessed(new[]
		{
			"OrderType",
			"OrderNbr",
			"Hold"
		})]
		protected virtual void ProductionOrder_Insert(PXGraph graph, EntityImpl entity, EntityImpl targetEntity)
		{
			var productionOrderGraph = (ProdMaint)graph;

			var orderTypeField = targetEntity.Fields.SingleOrDefault(f => f.Name == "OrderType") as EntityValueField;
			var prodOrdIDField = targetEntity.Fields.SingleOrDefault(f => f.Name == "ProductionNbr") as EntityValueField;
			var holdField = targetEntity.Fields.SingleOrDefault(f => f.Name == "Hold") as EntityValueField;

			var prodOrder = (AMProdItem)productionOrderGraph.ProdMaintRecords.Cache.CreateInstance();

			if (orderTypeField != null)
				prodOrder.OrderType = orderTypeField.Value;

			if (prodOrdIDField != null)
				prodOrder.ProdOrdID = prodOrdIDField.Value;

			productionOrderGraph.ProdMaintRecords.Current = productionOrderGraph.ProdMaintRecords.Insert(prodOrder);
			productionOrderGraph.SubscribeToPersistDependingOnBoolField(holdField, productionOrderGraph.putOnHold, productionOrderGraph.releaseFromHold);
		}

		[FieldsProcessed(new[]
		{
			"Hold"
		})]
		protected virtual void ProductionOrder_Update(PXGraph graph, EntityImpl entity, EntityImpl targetEntity)
		{
			var productionOrderGraph = (ProdMaint)graph;

			productionOrderGraph.ProdMaintRecords.View.Answer = WebDialogResult.Yes;

			var holdField = targetEntity.Fields.SingleOrDefault(f => f.Name == "Hold") as EntityValueField;

			productionOrderGraph.SubscribeToPersistDependingOnBoolField(holdField, productionOrderGraph.putOnHold, productionOrderGraph.releaseFromHold);
		}
	}
}
