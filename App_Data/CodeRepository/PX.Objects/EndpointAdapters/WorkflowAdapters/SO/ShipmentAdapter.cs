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
using PX.Objects.SO;

namespace PX.Objects.EndpointAdapters.WorkflowAdapters.SO
{
	[PXVersion("17.200.001", "Default")]
	[PXVersion("18.200.001", "Default")]
	[PXVersion("20.200.001", "Default")]
	[PXVersion("22.200.001", "Default")]
	[PXVersion("23.200.001", "Default")]
	internal class ShipmentAdapter
	{
		[FieldsProcessed(new[] {
			"ShipmentNbr",
			"Type",
			"Hold"
		})]
		protected virtual void Shipment_Insert(PXGraph graph, EntityImpl entity, EntityImpl targetEntity)
		{
			var shipmentGraph = (SOShipmentEntry)graph;

			var shipmentField = targetEntity.Fields.SingleOrDefault(f => f.Name == "ShipmentNbr") as EntityValueField;
			var typeField = targetEntity.Fields.SingleOrDefault(f => f.Name == "Type") as EntityValueField;
			var holdField = targetEntity.Fields.SingleOrDefault(f => f.Name == "Hold") as EntityValueField;

			var soShipment = (SOShipment)shipmentGraph.Document.Cache.CreateInstance();

			if (typeField != null)
				shipmentGraph.Document.Cache.SetValueExt<SOShipment.shipmentType>(soShipment, typeField.Value);

			if (shipmentField != null)
				soShipment.ShipmentNbr = shipmentField.Value;

			shipmentGraph.Document.Current = shipmentGraph.Document.Insert(soShipment);
			shipmentGraph.SubscribeToPersistDependingOnBoolField(holdField, shipmentGraph.putOnHold, shipmentGraph.releaseFromHold);
		}

		[FieldsProcessed(new[] {
			"Hold",
			"FreightAmount"
		})]
		protected virtual void Shipment_Update(PXGraph graph, EntityImpl entity, EntityImpl targetEntity)
		{
			var shipmentGraph = (SOShipmentEntry)graph;

			shipmentGraph.Document.View.Answer = WebDialogResult.Yes;

			var holdField = targetEntity.Fields.SingleOrDefault(f => f.Name == "Hold") as EntityValueField;
			var freightPrice = targetEntity.Fields.SingleOrDefault(f => f.Name == "FreightAmount") as EntityValueField;

			if (freightPrice != null)
			{
				var shipmentEntry = (SOShipmentEntry)graph;
				var shipment = shipmentEntry.Document.Current;
				if (shipment.FreightAmountSource != CS.FreightAmountSourceAttribute.OrderBased)
				{
					shipmentEntry.Document.SetValueExt<SOShipment.overrideFreightAmount>(shipment, true);
					shipmentEntry.Document.SetValueExt<SOShipment.curyFreightAmt>(shipment, freightPrice.Value);
				}
			}

			shipmentGraph.SubscribeToPersistDependingOnBoolField(holdField, shipmentGraph.putOnHold, shipmentGraph.releaseFromHold);
		}
	}
}
