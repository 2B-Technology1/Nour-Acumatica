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
	internal class PurchaseReceiptAdapter
	{
		[FieldsProcessed(new[] {
			"Type",
			"ReceiptNbr",
			"Hold"
		})]
		protected virtual void PurchaseReceipt_Insert(PXGraph graph, EntityImpl entity, EntityImpl targetEntity)
		{
			var receiptGraph = (POReceiptEntry)graph;

			var typeField = targetEntity.Fields.SingleOrDefault(f => f.Name == "Type") as EntityValueField;
			var receiptNbrField = targetEntity.Fields.SingleOrDefault(f => f.Name == "ReceiptNbr") as EntityValueField;
			var holdField = targetEntity.Fields.SingleOrDefault(f => f.Name == "Hold") as EntityValueField;

			var poReceipt = (POReceipt)receiptGraph.Document.Cache.CreateInstance();

			if (!string.IsNullOrEmpty(typeField?.Value))
			{
				//we have a problem when set "Receipt" instead of "RT" - the system set "Re" value 
				string receiptType;
				if (!new POReceiptType.ListAttribute().TryGetValue(typeField.Value, out receiptType))
					receiptType = typeField?.Value;

				receiptGraph.Document.Cache.SetValueExt<POReceipt.receiptType>(poReceipt, receiptType);
			}

			if (receiptNbrField != null)
				poReceipt.ReceiptNbr = receiptNbrField.Value;

			receiptGraph.Document.Current = receiptGraph.Document.Insert(poReceipt);
			receiptGraph.SubscribeToPersistDependingOnBoolField(holdField, receiptGraph.putOnHold, receiptGraph.releaseFromHold);
		}

		[FieldsProcessed(new[] {
			"Hold"
		})]
		protected virtual void PurchaseReceipt_Update(PXGraph graph, EntityImpl entity, EntityImpl targetEntity)
		{
			var receiptGraph = (POReceiptEntry)graph;
			var holdField = targetEntity.Fields.SingleOrDefault(f => f.Name == "Hold") as EntityValueField;
			receiptGraph.SubscribeToPersistDependingOnBoolField(holdField, receiptGraph.putOnHold, receiptGraph.releaseFromHold);
		}
	}
}
