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
using PX.Objects.AR;

namespace PX.Objects.EndpointAdapters.WorkflowAdapters.AR
{
	[PXVersion("17.200.001", "Default")]
	[PXVersion("18.200.001", "Default")]
	[PXVersion("20.200.001", "Default")]
	[PXVersion("22.200.001", "Default")]
	[PXVersion("23.200.001", "Default")]
	internal class InvoiceAdapter
	{
		[FieldsProcessed(new[] { "Type", "ReferenceNbr", "Hold" })]
		protected void Invoice_Insert(PXGraph graph, EntityImpl entity, EntityImpl targetEntity)
		{
			ARInvoiceEntry invoiceGraph = (ARInvoiceEntry)graph;

			EntityValueField typeField = targetEntity.Fields.SingleOrDefault(f => f.Name == "Type") as EntityValueField;
			EntityValueField nbrField = targetEntity.Fields.SingleOrDefault(f => f.Name == "ReferenceNbr") as EntityValueField;
			EntityValueField holdField = targetEntity.Fields.SingleOrDefault(f => f.Name == "Hold") as EntityValueField;

			if (
				string.IsNullOrEmpty(nbrField?.Value) || //client actually wish to add new entity
				invoiceGraph.Document.Current?.RefNbr != nbrField?.Value
				)
			{
				ARInvoice invoice = (ARInvoice)invoiceGraph.Document.Cache.CreateInstance();

				if (typeField == null) invoiceGraph.Document.Cache.SetDefaultExt<ARRegister.docType>(invoice);
				else invoiceGraph.SetDropDownValue<ARInvoice.docType, ARInvoice>(typeField.Value, invoice);

				if (nbrField == null) invoiceGraph.Document.Cache.SetDefaultExt<ARInvoice.refNbr>(invoice);
				else invoiceGraph.Document.Cache.SetValueExt<ARInvoice.refNbr>(invoice, nbrField.Value);

				invoiceGraph.Document.Current = invoiceGraph.Document.Insert(invoice);
			}
			invoiceGraph.SubscribeToPersistDependingOnBoolField(holdField, invoiceGraph.putOnHold, invoiceGraph.releaseFromHold);
		}

		[FieldsProcessed(new[] { "Hold" })]
		protected void Invoice_Update(PXGraph graph, EntityImpl entity, EntityImpl targetEntity)
		{
			ARInvoiceEntry invoiceGraph = (ARInvoiceEntry)graph;
			EntityValueField holdField = targetEntity.Fields.SingleOrDefault(f => f.Name == "Hold") as EntityValueField;
			invoiceGraph.SubscribeToPersistDependingOnBoolField(holdField, invoiceGraph.putOnHold, invoiceGraph.releaseFromHold);
		}

		protected void Action_ReleaseInvoice(PXGraph graph, ActionImpl action)
		{
			ARInvoiceEntry invoiceGraph = (ARInvoiceEntry)graph;
			invoiceGraph.Save.Press();
			invoiceGraph.release.Press();
		}
	}
}
