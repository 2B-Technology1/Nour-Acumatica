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
using PX.Objects.CN.Compliance.CL.DAC;

namespace PX.Objects.CN.Compliance.CL.Descriptor.Attributes
{
	public class ComplianceDocumentLienWaiverTypeAttribute : PXEventSubscriberAttribute, IPXRowPersistingSubscriber
	{
		public void RowPersisting(PXCache cache, PXRowPersistingEventArgs args)
		{
			if (args.Row is ComplianceDocument complianceDocument && args.Operation != PXDBOperation.Delete)
			{
				var LienWaiverDocumentTypeId = new PXSelectReadonly<ComplianceAttributeType,
						Where<ComplianceAttributeType.type, Equal<ComplianceDocumentType.lienWaiver>>>(cache.Graph)
							.SelectSingle()?.ComplianceAttributeTypeID;

				if (LienWaiverDocumentTypeId != null
					&& complianceDocument.DocumentType != LienWaiverDocumentTypeId)
				{
					return;
				}
				if (complianceDocument.ProjectID == null)
				{
					cache.RaiseExceptionHandling<ComplianceDocument.projectID>(complianceDocument, complianceDocument.ProjectID,
						new PXSetPropertyException(ErrorMessages.FieldIsEmpty, PXUIFieldAttribute.GetDisplayName<ComplianceDocument.projectID>(cache)));
				}
				if (complianceDocument.VendorID == null)
				{
					cache.RaiseExceptionHandling<ComplianceDocument.vendorID>(complianceDocument, complianceDocument.VendorID,
						new PXSetPropertyException(ErrorMessages.FieldIsEmpty, PXUIFieldAttribute.GetDisplayName<ComplianceDocument.vendorID>(cache)));
				}
				if (complianceDocument.DocumentTypeValue == null)
				{
					cache.RaiseExceptionHandling<ComplianceDocument.documentTypeValue>(complianceDocument, complianceDocument.DocumentTypeValue,
						new PXSetPropertyException(ErrorMessages.FieldIsEmpty, PXUIFieldAttribute.GetDisplayName<ComplianceDocument.documentTypeValue>(cache)));
				}
			}
		}
	}
}
