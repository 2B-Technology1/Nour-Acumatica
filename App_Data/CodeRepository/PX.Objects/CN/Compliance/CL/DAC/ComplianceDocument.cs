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
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.AP;
using PX.Objects.AR;
using PX.Objects.CA;
using PX.Objects.CM;
using PX.Objects.CN.Common.Descriptor;
using PX.Objects.CN.Common.Descriptor.Attributes;
using PX.Objects.CN.Compliance.CL.Descriptor.Attributes;
using PX.Objects.CN.Compliance.CL.Descriptor.Attributes.ComplianceDocumentRefNote;
using PX.Objects.CN.Compliance.CL.Descriptor.Attributes.LienWaiver;
using PX.Objects.CN.Compliance.Descriptor;
using PX.Objects.CN.ProjectAccounting.Descriptor;
using PX.Objects.CN.ProjectAccounting.PM.CacheExtensions;
using PX.Objects.CN.ProjectAccounting.PM.Descriptor;
using PX.Objects.CT;
using PX.Objects.GL;
using PX.Objects.PM;
using PX.Objects.PO;

namespace PX.Objects.CN.Compliance.CL.DAC
{
	/// <summary>
	/// Represents a compliance document.
	/// The records of this type are created and edited through the Compliance Management (CL401000) form
	/// (which corresponds to the <see cref="Graphs.ComplianceDocumentEntry"/> graph)
	/// and through many other forms that contain the <b>Compliance</b> tab.
	/// </summary>
	[Serializable]
    [PXCacheName("Compliance Document")]
    public class ComplianceDocument : IBqlTable
    {
        [PXBool]
        [PXUIField]
        public virtual bool? Selected
        {
            get;
            set;
        }

        [PXUIField(DisplayName = "Document Id", Enabled = false, Visibility = PXUIVisibility.Invisible)]
		[PXDBIdentity(IsKey = true)]
        public virtual int? ComplianceDocumentID
        {
            get;
            set;
        }        

        [PXDBBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Required", Visibility = PXUIVisibility.SelectorVisible)]
        public virtual bool? Required
        {
            get;
            set;
        }

        [PXDBBool]
        [PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Received from Vendor", Visibility = PXUIVisibility.SelectorVisible)]
        public virtual bool? Received
        {
            get;
            set;
        }

        [PXDBDate(PreserveTime = false)]
        [PXDefault(typeof(AccessInfo.businessDate), PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Creation Date", Visibility = PXUIVisibility.SelectorVisible)]
        public virtual DateTime? CreationDate
        {
            get;
            set;
        }

        [PXDBDate(PreserveTime = false)]
        [PXFormula(typeof(IIf<Where<received.IsEqual<True>>,
            IIf<Where<receivedDate.FromCurrent.IsNull>,
                AccessInfo.businessDate.FromCurrent, receivedDate.FromCurrent>, Null>))]
        [PXUIField(DisplayName = "Received Date (Vendor)", Visibility = PXUIVisibility.SelectorVisible)]
        public virtual DateTime? ReceivedDate
        {
            get;
            set;
        }

        [PXDBDate(PreserveTime = false)]
        [PXUIField(DisplayName = "Sent Date", Visibility = PXUIVisibility.SelectorVisible)]
        public virtual DateTime? SentDate
        {
            get;
            set;
        }

        [PXDBDate(PreserveTime = false)]
        [PXUIField(DisplayName = "Effective Date", Visibility = PXUIVisibility.SelectorVisible)]
        public virtual DateTime? EffectiveDate
        {
            get;
            set;
        }

        [PXDBDate(PreserveTime = false)]
        [PXUIField(DisplayName = "Expiration Date", Visibility = PXUIVisibility.SelectorVisible)]
        public virtual DateTime? ExpirationDate
        {
            get;
            set;
        }

        [PXDBBaseCury]
        [PXUIField(DisplayName = "Limit", Visibility = PXUIVisibility.SelectorVisible)]
        public virtual decimal? Limit
        {
            get;
            set;
        }

        [PXDBInt]
        [PXDefault(typeof(SearchFor<ComplianceAttributeType.complianceAttributeTypeID>.
            Where<ComplianceAttributeType.type.IsEqual<FilterRow.valueSt.FromCurrent>>))]
        [PXSelector(typeof(Search<ComplianceAttributeType.complianceAttributeTypeID,
                Where<ComplianceAttributeType.type, NotEqual<ComplianceDocumentType.status>>>),
            SubstituteKey = typeof(ComplianceAttributeType.type))]
        [PXUIField(DisplayName = "Document Type", Visibility = PXUIVisibility.SelectorVisible)]
        [ComplianceDocumentType]
        public virtual int? DocumentType
        {
            get;
            set;
        }

        [PXDBInt]
        [PXSelector(typeof(Search<ComplianceAttribute.attributeId,
                Where<ComplianceAttribute.type, Equal<Current<documentType>>>>),
            SubstituteKey = typeof(ComplianceAttribute.value))]
		[ComplianceDocumentLienWaiverTypeAttribute]
		[PXUIField(DisplayName = "Document Category", Visibility = PXUIVisibility.SelectorVisible)]
        public virtual int? DocumentTypeValue
        {
            get;
            set;
        }

        [PXDBInt]
        [PXSelector(typeof(Search2<ComplianceAttribute.attributeId,
                InnerJoin<ComplianceAttributeType,
                    On<ComplianceAttributeType.complianceAttributeTypeID, Equal<ComplianceAttribute.type>>>,
                Where<ComplianceAttributeType.type, Equal<ComplianceDocumentType.status>>>),
            SubstituteKey = typeof(ComplianceAttribute.value))]
        [PXUIField(DisplayName = "Status", Visibility = PXUIVisibility.SelectorVisible)]
        public virtual int? Status
        {
            get;
            set;
        }

        [PXDBString(255, IsUnicode = true)]
        [PXUIField(DisplayName = "Method Sent", Visibility = PXUIVisibility.SelectorVisible)]
        public virtual string MethodSent
        {
            get;
            set;
        }

		[PXDBInt]
		[PXDimensionSelector(ProjectAttribute.DimensionName,
			typeof(Search<PMProject.contractID,
				Where<PMProject.baseType, Equal<CTPRType.project>,
					And<PMProject.nonProject, Equal<False>>>>),
			typeof(PMProject.contractCD),
			typeof(PMProject.contractCD), typeof(PMProject.customerID), typeof(PMProject.description),
			typeof(PMProject.status),
			DescriptionField = typeof(PMProject.description))]
		[ComplianceDocumentLienWaiverTypeAttribute]
		[PXUIField(DisplayName = "Project", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual int? ProjectID
		{
			get;
			set;
		}

		[PXDBInt]
        [PXDimensionSelector(ProjectTaskAttribute.DimensionName,typeof(Search<PMTask.taskID,
                Where<PMTask.projectID, Equal<Current<ComplianceDocument.projectID>>,
                    And<PMTask.type, NotEqual<ProjectTaskType.revenue>>>>),
            typeof(PMTask.taskCD),
            DescriptionField = typeof(PMTask.description))]
        [PXUIField(DisplayName = ProjectAccountingLabels.CostTask, Visibility = PXUIVisibility.SelectorVisible)]
        public virtual int? CostTaskID
		{
            get;
            set;
        }

        [PXDBInt]
        [PXDimensionSelector(ProjectTaskAttribute.DimensionName, typeof(Search<PMTask.taskID,
                Where<PMTask.projectID, Equal<Current<ComplianceDocument.projectID>>,
                    And<PMTask.type, NotEqual<ProjectTaskType.cost>>>>),
			typeof(PMTask.taskCD),
            DescriptionField = typeof(PMTask.description))]
        [PXUIField(DisplayName = ProjectAccountingLabels.RevenueTask, Visibility = PXUIVisibility.SelectorVisible)]
        public virtual int? RevenueTaskID
        {
            get;
            set;
        }

        [PXDBInt]
		[CostCodeDimensionSelector(typeof(PMCostCode.costCodeID))]
		[PXUIField(DisplayName = "Cost Code", Visibility = PXUIVisibility.SelectorVisible)]
        public virtual int? CostCodeID
        {
            get;
            set;
        }

        [Customer(DescriptionField = typeof(Customer.bAccountID), Visibility = PXUIVisibility.SelectorVisible)]
        [ComplianceDocumentCustomer]
        public virtual int? CustomerID
        {
            get;
            set;
        }

        [PXString(255, IsUnicode = true)]
        [PXUIField(DisplayName = "Customer Name", Enabled = false, Visibility = PXUIVisibility.SelectorVisible)]
        [PXDBScalar(typeof(Search<Customer.acctName,
            Where<Customer.bAccountID, Equal<customerID>>>))]
        public virtual string CustomerName
        {
            get;
            set;
        }

        

        [Vendor(Visibility = PXUIVisibility.SelectorVisible)]
		[ComplianceDocumentLienWaiverTypeAttribute]
		[ComplianceDocumentVendor]
        public virtual int? VendorID
        {
            get;
            set;
        }

        [PXString(255, IsUnicode = true)]
        [PXUIField(DisplayName = "Vendor Name", Enabled = false, Visibility = PXUIVisibility.SelectorVisible)]
        [PXDBScalar(typeof(Search<Vendor.acctName,
            Where<Vendor.bAccountID, Equal<vendorID>>>))]
        public virtual string VendorName
        {
            get;
            set;
        }

        [Vendor(Visibility = PXUIVisibility.SelectorVisible, DisplayName = "Secondary Vendor", Visible = false)]
        [ComplianceDocumentSecondaryVendor]
        public virtual int? SecondaryVendorID
        {
            get;
            set;
        }

        [PXString(255, IsUnicode = true)]
        [PXUIField(DisplayName = "Secondary Vendor Name", Enabled = false, Visible = false,
            Visibility = PXUIVisibility.SelectorVisible)]
        [PXDBScalar(typeof(Search<Vendor.acctName,
            Where<Vendor.bAccountID, Equal<secondaryVendorID>>>))]
        public virtual string SecondaryVendorName
        {
            get;
            set;
        }

        [FieldDescriptionForDynamicColumns]
        [ComplianceDocumentRefNote(typeof(POOrder))]
        [PXUIField(DisplayName = "Purchase Order", Visibility = PXUIVisibility.SelectorVisible)]
        public virtual Guid? PurchaseOrder
        {
            get;
            set;
        }

        [PXDBInt]
        [ComplianceDocumentPurchaseOrderLineSelector]
        [PXUIField(DisplayName = "Purchase Order Line Item", Visibility = PXUIVisibility.SelectorVisible)]
        [DependsOnField(typeof(purchaseOrder))]
        public virtual int? PurchaseOrderLineItem
        {
            get;
            set;
        }

        [PXDBString(15)]
        [PXSelector(typeof(Search2<POOrder.orderNbr,
                InnerJoin<Vendor, On<POOrder.vendorID, Equal<Vendor.bAccountID>,
                    And<Match<Vendor, Current<AccessInfo.userName>>>>>,
                Where<POOrder.orderType, Equal<POOrderType.regularSubcontract>>,
                OrderBy<Desc<POOrder.orderNbr>>>),
            typeof(POOrder.orderNbr),
            typeof(POOrder.vendorRefNbr),
            typeof(POOrder.orderDate),
            typeof(POOrder.status),
            typeof(POOrder.vendorID),
            typeof(POOrder.vendorID_Vendor_acctName),
            typeof(POOrder.vendorLocationID),
            typeof(POOrder.curyID),
            typeof(POOrder.curyOrderTotal),
            Filterable = true,
            Headers = new[]
            {
                ComplianceLabels.Subcontract.SubcontractNumber,
                ComplianceLabels.Subcontract.VendorReference,
                ComplianceLabels.Subcontract.Date,
                ComplianceLabels.Subcontract.Status,
                ComplianceLabels.Subcontract.Vendor,
                ComplianceLabels.Subcontract.VendorName,
                ComplianceLabels.Subcontract.Location,
                ComplianceLabels.Subcontract.Currency,
                ComplianceLabels.Subcontract.SubcontractTotal
            })]
        [PXUIField(DisplayName = "Subcontract", Visibility = PXUIVisibility.SelectorVisible)]
        [SubcontractLink]
        public virtual string Subcontract
        {
            get;
            set;
        }

        [PXDBInt]
        [PXSelector(typeof(Search2<POLine.lineNbr,
                LeftJoin<POOrder, On<POOrder.orderNbr, Equal<POLine.orderNbr>,
                    And<POOrder.orderType, Equal<POOrderType.regularSubcontract>>>>,
                Where<POOrder.orderNbr, Equal<Current<subcontract>>>>),
            typeof(POLine.lineNbr),
            typeof(POLine.branchID),
            typeof(POLine.inventoryID),
            typeof(POLine.lineType),
            typeof(POLine.tranDesc),
            typeof(POLine.orderQty),
            typeof(POLine.curyUnitCost))]
        [PXUIField(DisplayName = "Subcontract Line Item", Visibility = PXUIVisibility.SelectorVisible)]
        [DependsOnField(typeof(subcontract))]
        public virtual int? SubcontractLineItem
        {
            get;
            set;
        }

        [PXDBString(15)]
        [PXSelector(typeof(Search3<PMChangeOrder.refNbr,
                InnerJoin<Customer, On<PMChangeOrder.customerID, Equal<Customer.bAccountID>,
                    And<Match<Customer, Current<AccessInfo.userName>>>>>,
                OrderBy<Desc<PMChangeOrder.refNbr>>>),
            typeof(PMChangeOrder.refNbr),
            typeof(PMChangeOrder.projectID),
            typeof(PMChangeOrder.status),
            typeof(PMChangeOrder.projectNbr),
            typeof(PMChangeOrder.date),
            typeof(PMChangeOrder.completionDate),
            Filterable = true)]
        [PXUIField(DisplayName = "Change Order", Visibility = PXUIVisibility.SelectorVisible)]
        public virtual string ChangeOrderNumber
        {
            get;
            set;
        }

        [FieldDescriptionForDynamicColumns]
        [ComplianceDocumentInvoice]
        [ComplianceDocumentRefNote(typeof(ARInvoice))]
        [PXUIField(DisplayName = "AR Invoice", Visibility = PXUIVisibility.SelectorVisible)]
        public virtual Guid? InvoiceID
        {
            get;
            set;
        }

        [PXBaseCury]
        [PXDBScalar(typeof(Search2<ARInvoice.origDocAmt,
            LeftJoin<ComplianceDocumentReference, On<ComplianceDocumentReference.refNoteId, Equal<ARInvoice.noteID>>>,
            Where<ComplianceDocumentReference.complianceDocumentReferenceId, Equal<invoiceID>>>))]
        [PXUIField(DisplayName = "AR Invoice Amount", Enabled = false, Visibility = PXUIVisibility.SelectorVisible)]
        public virtual decimal? InvoiceAmount
        {
            get;
            set;
        }

        [FieldDescriptionForDynamicColumns]
        [ComplianceDocumentBill]
        [ComplianceDocumentRefNote(typeof(APInvoice))]
        [PXUIField(DisplayName = "Bill", Visibility = PXUIVisibility.SelectorVisible)]
        public virtual Guid? BillID
        {
            get;
            set;
        }

        [PXBaseCury]
        [PXDBScalar(typeof(Search2<APInvoice.origDocAmt,
            LeftJoin<ComplianceDocumentReference, On<ComplianceDocumentReference.refNoteId, Equal<APInvoice.noteID>>>,
            Where<ComplianceDocumentReference.complianceDocumentReferenceId, Equal<billID>>>))]
        [PXUIField(DisplayName = "Bill Amount", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
        public virtual decimal? BillAmount
        {
            get;
            set;
        }

        [PXDBBaseCury]
        [PXUIField(DisplayName = "Lien Waiver Amount (Vendor)", Visibility = PXUIVisibility.SelectorVisible)]
        public virtual decimal? LienWaiverAmount
        {
            get;
            set;
        }

        [PXDBString(255, IsUnicode = true)]
        [PXUIField(DisplayName = "Sponsor Organization", Visibility = PXUIVisibility.SelectorVisible)]
        public virtual string SponsorOrganization
        {
            get;
            set;
        }

        [PXDBString(255, IsUnicode = true)]
        [PXUIField(DisplayName = "Certificate Number", Visibility = PXUIVisibility.SelectorVisible)]
        public virtual string CertificateNumber
        {
            get;
            set;
        }

        [PXDBString(255, IsUnicode = true)]
        [PXUIField(DisplayName = "Insurance Company", Visibility = PXUIVisibility.SelectorVisible)]
        public virtual string InsuranceCompany
        {
            get;
            set;
        }

        [PXDBString(255, IsUnicode = true)]
        [PXUIField(DisplayName = "Policy", Visibility = PXUIVisibility.SelectorVisible)]
        [ComplianceDocumentPolicyUnique]
        public virtual string Policy
        {
            get;
            set;
        }

        [PXInt]
        [PXDBScalar(typeof(Search<ComplianceAttributeType.complianceAttributeTypeID,
            Where<ComplianceAttributeType.type, Equal<ComplianceDocumentType.insurance>>>))]
        public virtual int? InsuranceDocumentTypeId
        {
            get;
            set;
        }

        [PXDBString(10)]
        [PXUIField(DisplayName = "AP Payment Method", Visibility = PXUIVisibility.SelectorVisible)]
        [PXSelector(typeof(Search<PaymentMethod.paymentMethodID,
            Where<PaymentMethod.useForAP, Equal<True>, And<PaymentMethod.isActive, Equal<True>>>>))]
        public virtual string ApPaymentMethodID
        {
            get;
            set;
        }

        [PXDBString(10)]
        [PXUIField(DisplayName = "AR Payment Method", Visibility = PXUIVisibility.SelectorVisible)]
        [PXSelector(typeof(Search<PaymentMethod.paymentMethodID,
            Where<PaymentMethod.useForAR, Equal<True>, And<PaymentMethod.isActive, Equal<True>>>>))]
        public virtual string ArPaymentMethodID
        {
            get;
            set;
        }

        [AccountAny]
        public virtual int? AccountID
        {
            get;
            set;
        }

        [FieldDescriptionForDynamicColumns]
        [ComplianceDocumentRefNote(typeof(APPayment))]
        [ComplianceDocumentCheck]
        [PXUIField(DisplayName = "AP Payment", Visibility = PXUIVisibility.SelectorVisible)]
        public virtual Guid? ApCheckID
        {
            get;
            set;
        }

        [PXString(40, IsUnicode = true)]
        [PXUIField(DisplayName = "Payment Ref.", Enabled = false, Visibility = PXUIVisibility.SelectorVisible)]
        [PXDBScalar(typeof(Search2<APPayment.extRefNbr,
            LeftJoin<ComplianceDocumentReference, On<ComplianceDocumentReference.refNoteId, Equal<APPayment.noteID>>>,
            Where<ComplianceDocumentReference.complianceDocumentReferenceId, Equal<apCheckId>>>))]
        public virtual string CheckNumber
        {
            get;
            set;
        }

        [FieldDescriptionForDynamicColumns]
        [ComplianceDocumentRefNote(typeof(ARPayment))]
        [PXUIField(DisplayName = "AR Payment", Visibility = PXUIVisibility.SelectorVisible)]
        public virtual Guid? ArPaymentID
        {
            get;
            set;
        }

        [FieldDescriptionForDynamicColumns]
        [ComplianceDocumentRefNote(typeof(PMRegister))]
        [PXUIField(DisplayName = "Project Transaction", Visibility = PXUIVisibility.SelectorVisible)]
        public virtual Guid? ProjectTransactionID
        {
            get;
            set;
        }

        [PXDBDate(PreserveTime = false)]
        [PXUIField(DisplayName = "Receipt Date", Visibility = PXUIVisibility.SelectorVisible)]
        public virtual DateTime? ReceiptDate
        {
            get;
            set;
        }

        [PXDBDate(PreserveTime = false)]
        [PXUIField(DisplayName = "Date Issued", Visibility = PXUIVisibility.SelectorVisible)]
        public virtual DateTime? DateIssued
        {
            get;
            set;
        }

        [PXDBDate(PreserveTime = false)]
        [PXUIField(DisplayName = "Through Date", Visibility = PXUIVisibility.SelectorVisible)]
        public virtual DateTime? ThroughDate
        {
            get;
            set;
        }

        [PXDBDate(PreserveTime = false)]
        [PXUIField(DisplayName = "Receive Date", Visibility = PXUIVisibility.SelectorVisible)]
        public virtual DateTime? ReceiveDate
        {
            get;
            set;
        }

        [PXDBDate(PreserveTime = false)]
        [PXUIField(DisplayName = "Payment Date", Visibility = PXUIVisibility.SelectorVisible)]
        public virtual DateTime? PaymentDate
        {
            get;
            set;
        }

        [PXDBString(255, IsUnicode = true)]
        [PXUIField(DisplayName = "Received By", Visibility = PXUIVisibility.SelectorVisible)]
        public virtual string ReceivedBy
        {
            get;
            set;
        }

        [PXDBString(10)]
        [PXDefault(ComplianceDocumentSourceTypeAttribute.ApBill)]
        [PXUIField(DisplayName = "Source", Visibility = PXUIVisibility.SelectorVisible)]
        [ComplianceDocumentSourceType]
        public virtual string SourceType
        {
            get;
            set;
        }

        [PXDBBool]
        [PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Requires Joint Payment", Visibility = PXUIVisibility.SelectorVisible)]
        public virtual bool? IsRequiredJointCheck
        {
            get;
            set;
        }

        [Vendor(Visibility = PXUIVisibility.SelectorVisible, DisplayName = "Joint Payee (Vendor)")]
        [JointVendorRequired]
        public virtual int? JointVendorInternalId
        {
            get;
            set;
        }

        [PXDBString(30)]
        [JointVendorRequired]
        [PXUIField(DisplayName = "Joint Payee")]
        public virtual string JointVendorExternalName
        {
            get;
            set;
        }

        [PXDBBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Link To Payment", Visibility = PXUIVisibility.Invisible)]
        public virtual bool? LinkToPayment
        {
            get;
            set;
        }

        [PXDBBaseCury]
        [PXUIField(DisplayName = "Joint Amount Paid", Visibility = PXUIVisibility.SelectorVisible)]
        public virtual decimal? JointAmount
        {
            get;
            set;
        }

        [PXDBString(255, IsUnicode = true)]
        [PXUIField(DisplayName = "Joint Release", Visibility = PXUIVisibility.SelectorVisible)]
        public virtual string JointRelease
        {
            get;
            set;
        }

        [PXDBBool]
        [PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Joint Release Received", Visibility = PXUIVisibility.SelectorVisible)]
        public virtual bool? JointReleaseReceived
        {
            get;
            set;
        }

        [PXBool]
        [UiInformationField]
        [PXUnboundDefault(typeof(IIf<expirationDate.IsLess<AccessInfo.businessDate.FromCurrent>,
            True, False>))]
        [PXFormula(typeof(Default<expirationDate>))]
        [PXUIField(DisplayName = "Expired", IsReadOnly = true)]
        public virtual bool? IsExpired
        {
            get;
            set;
        }

        [PXDBBool]
        [PXUIField(DisplayName = "Processed", Enabled = false)]
        public virtual bool? IsProcessed
        {
            get;
            set;
        }

        [PXDBBool]
        [PXUIField(DisplayName = "Voided")]
        public virtual bool? IsVoided
        {
            get;
            set;
        }

        [PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Created Automatically", Enabled = false)]
        public virtual bool? IsCreatedAutomatically
        {
            get;
            set;
        }

        [PXDBBaseCury]
        [PXUIField(DisplayName = "Lien Notice Amount")]
        public virtual decimal? LienNoticeAmount
        {
            get;
            set;
        }

        [PXDBBaseCury]
        [PXUIField(DisplayName = "Joint Lien Notice Amount")]
        public virtual decimal? JointLienNoticeAmount
        {
            get;
            set;
        }

        [PXDBBool]
        [PXUIField(DisplayName = "Received from Joint Payee (Vendor)")]
        public virtual bool? IsReceivedFromJointVendor
        {
            get;
            set;
        }

        [PXDBDate(PreserveTime = false)]
        [PXFormula(typeof(IIf<Where<isReceivedFromJointVendor.IsEqual<True>>,
            IIf<Where<jointReceivedDate.FromCurrent.IsNull>,
                AccessInfo.businessDate.FromCurrent, jointReceivedDate.FromCurrent>, Null>))]
        [PXUIField(DisplayName = "Joint Payee Received Date")]
        public virtual DateTime? JointReceivedDate
        {
            get;
            set;
        }

        [PXDBBaseCury]
        [PXUIField(DisplayName = "Joint Payee Lien Waiver Amount")]
        public virtual decimal? JointLienWaiverAmount
        {
            get;
            set;
        }

		#region tstamp
		public abstract class tstamp : PX.Data.BQL.BqlByteArray.Field<tstamp> { }

		[PXDBTimestamp()]
		public virtual Byte[] Tstamp { get; set; }
		#endregion

		[PXDBCreatedByID(Visible = false, Visibility = PXUIVisibility.Invisible)]
        public virtual Guid? CreatedById
        {
            get;
            set;
        }

        [PXDBCreatedByScreenID]
        public virtual string CreatedByScreenId
        {
            get;
            set;
        }

        [PXDBCreatedDateTime]
        public virtual DateTime? CreatedDateTime
        {
            get;
            set;
        }

        [PXDBLastModifiedByID(Visible = false, Visibility = PXUIVisibility.Invisible)]
        public virtual Guid? LastModifiedById
        {
            get;
            set;
        }

        [PXDBLastModifiedByScreenID]
        public virtual string LastModifiedByScreenId
        {
            get;
            set;
        }

        [PXDBLastModifiedDateTime]
        public virtual DateTime? LastModifiedDateTime
        {
            get;
            set;
        }

        [PXNote]
        public virtual Guid? NoteID
        {
            get;
            set;
        }

		#region Skip Reference
		[PXBool]
		public virtual bool? SkipInit
		{
			get;
			set;
		}
		public abstract class skipInit : BqlBool.Field<skipInit>
		{
		} 
		#endregion

		public abstract class selected : BqlBool.Field<selected>
        {
        }

        public abstract class createdById : BqlBool.Field<createdById>
        {
        }

        public abstract class linkToPayment : BqlBool.Field<linkToPayment>
        {
        }

        public abstract class jointAmount : BqlDecimal.Field<jointAmount>
        {
        }

        public abstract class jointRelease : BqlString.Field<jointRelease>
        {
        }

        public abstract class jointReleaseReceived : BqlBool.Field<jointReleaseReceived>
        {
        }

        public abstract class complianceDocumentID : BqlInt.Field<complianceDocumentID>
        {
        }

        public abstract class complianceDocumentIdForReport : BqlInt.Field<complianceDocumentIdForReport>
        {
        }

        public abstract class required : BqlBool.Field<required>
        {
        }

        public abstract class received : BqlBool.Field<received>
        {
        }

        public abstract class creationDate : BqlDateTime.Field<creationDate>
        {
        }

        public abstract class receivedDate : BqlDateTime.Field<receivedDate>
        {
        }

        public abstract class sentDate : BqlDateTime.Field<sentDate>
        {
        }

        public abstract class effectiveDate : BqlDateTime.Field<effectiveDate>
        {
        }

        public abstract class expirationDate : BqlDateTime.Field<expirationDate>
        {
        }

        public abstract class limit : BqlDecimal.Field<limit>
        {
        }

        public abstract class documentType : BqlInt.Field<documentType>
        {
        }

        public abstract class documentTypeValue : BqlInt.Field<documentTypeValue>
        {
        }

        public abstract class status : BqlInt.Field<status>
        {
        }

        public abstract class methodSent : BqlString.Field<methodSent>
        {
        }

        public abstract class costTaskID : BqlInt.Field<costTaskID>
        {
        }

        public abstract class revenueTaskID : BqlInt.Field<revenueTaskID>
        {
        }

        public abstract class costCodeID : BqlInt.Field<costCodeID>
        {
        }

        public abstract class customerID : BqlInt.Field<customerID>
        {
        }

        public abstract class customerName : BqlString.Field<customerName>
        {
        }

        public abstract class projectID : BqlInt.Field<projectID>
        {
        }

        public abstract class vendorID : BqlInt.Field<vendorID>
        {
        }

        public abstract class vendorName : BqlString.Field<vendorName>
        {
        }

        public abstract class secondaryVendorID : BqlInt.Field<secondaryVendorID>
        {
        }

        public abstract class secondaryVendorName : BqlString.Field<secondaryVendorName>
        {
        }

        public abstract class purchaseOrderLineItem : BqlInt.Field<purchaseOrderLineItem>
        {
        }

        public abstract class purchaseOrder : BqlGuid.Field<purchaseOrder>
        {
        }

        public abstract class subcontract : BqlString.Field<subcontract>
        {
        }

        public abstract class subcontractLineItem : BqlInt.Field<subcontractLineItem>
        {
        }

        public abstract class changeOrderNumber : BqlString.Field<changeOrderNumber>
        {
        }

        public abstract class invoiceID : BqlGuid.Field<invoiceID>
        {
        }

        public abstract class invoiceAmount : BqlDecimal.Field<invoiceAmount>
        {
        }

        public abstract class billID : BqlGuid.Field<billID>
        {
        }

        public abstract class billAmount : BqlDecimal.Field<billAmount>
        {
        }

        public abstract class lienWaiverAmount : BqlDecimal.Field<lienWaiverAmount>
        {
        }

        public abstract class sponsorOrganization : BqlString.Field<sponsorOrganization>
        {
        }

        public abstract class certificateNumber : BqlString.Field<certificateNumber>
        {
        }

        public abstract class insuranceCompany : BqlString.Field<insuranceCompany>
        {
        }

        public abstract class policy : BqlString.Field<policy>
        {
        }

        public abstract class insuranceDocumentTypeId : BqlInt.Field<insuranceDocumentTypeId>
        {
        }

        public abstract class apPaymentMethodID : BqlString.Field<apPaymentMethodID>
        {
        }

        public abstract class arPaymentMethodID : BqlString.Field<arPaymentMethodID>
        {
        }

        public abstract class accountID : BqlInt.Field<accountID>
        {
        }

        public abstract class apCheckId : BqlGuid.Field<apCheckId>
        {
        }

        public abstract class checkNumber : BqlString.Field<checkNumber>
        {
        }

        public abstract class arPaymentID : BqlGuid.Field<arPaymentID>
        {
        }

        public abstract class projectTransactionID : BqlGuid.Field<projectTransactionID>
        {
        }

        public abstract class receiptDate : BqlDateTime.Field<receiptDate>
        {
        }

        public abstract class dateIssued : BqlDateTime.Field<dateIssued>
        {
        }

        public abstract class throughDate : BqlDateTime.Field<throughDate>
        {
        }

        public abstract class receiveDate : BqlDateTime.Field<receiveDate>
        {
        }

        public abstract class paymentDate : BqlDateTime.Field<paymentDate>
        {
        }

        public abstract class receivedBy : BqlString.Field<receivedBy>
        {
        }

        public abstract class sourceType : BqlString.Field<sourceType>
        {
        }

        public abstract class isRequiredJointCheck : BqlBool.Field<isRequiredJointCheck>
        {
        }

        public abstract class jointVendorInternalId : BqlInt.Field<jointVendorInternalId>
        {
        }

        public abstract class jointVendorExternalName : BqlString.Field<jointVendorExternalName>
        {
        }

        public abstract class noteID : BqlGuid.Field<noteID>
        {
        }

        public abstract class isExpired : BqlBool.Field<isExpired>
        {
        }

        public abstract class lastModifiedDateTime : BqlDateTime.Field<lastModifiedDateTime>
        {
        }

        public abstract class isProcessed : BqlBool.Field<isProcessed>
        {
        }

        public abstract class isVoided : BqlBool.Field<isVoided>
        {
        }

        public abstract class isCreatedAutomatically : BqlBool.Field<isCreatedAutomatically>
        {
        }

        public abstract class lienNoticeAmount : BqlDecimal.Field<lienNoticeAmount>
        {
        }

        public abstract class jointLienNoticeAmount : BqlDecimal.Field<jointLienNoticeAmount>
        {
        }

        public abstract class isReceivedFromJointVendor : BqlBool.Field<isReceivedFromJointVendor>
        {
        }

        public abstract class jointReceivedDate : BqlDateTime.Field<jointReceivedDate>
        {
        }

        public abstract class jointLienWaiverAmount : BqlDecimal.Field<jointLienWaiverAmount>
        {
        }

		public abstract class createdDateTime : BqlDateTime.Field<createdDateTime>
		{
		}

        public class complianceClassId : BqlString.Constant<complianceClassId>
        {
            public complianceClassId()
                : base(Constants.ComplianceAttributeClassId)
            {
            }
        }

        public class typeName : BqlString.Constant<typeName>
        {
            public typeName()
                : base(typeof(ComplianceDocument).FullName)
            {
            }
        }
    }
}
