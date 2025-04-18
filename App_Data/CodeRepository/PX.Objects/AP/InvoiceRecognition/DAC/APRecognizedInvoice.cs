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

using PX.CloudServices.DAC;
using PX.Common;
using PX.Data;
using PX.Data.BQL;
using PX.Objects.AP.InvoiceRecognition.Feedback;
using System;
using System.Collections.Generic;

namespace PX.Objects.AP.InvoiceRecognition.DAC
{
	[PXInternalUseOnly]
	[PXCacheName(Messages.RecognizedAPInvoice)]
    [PXBreakInheritance]
	[PXProjection(typeof(
		Select2<RecognizedRecord,
		LeftJoin<APInvoice,
		On<APInvoice.noteID, Equal<RecognizedRecord.documentLink>>>,
		Where<RecognizedRecord.entityType, Equal<RecognizedRecordEntityTypeListAttribute.aPDocument>>>), Persistent = true)]
	public class APRecognizedInvoice : APInvoice
	{
        #region RecognizedRecord

        [PXUIField(DisplayName = "Recognized Record Ref. Nbr.", Visible = false)]
        [PXDefault]
        [PXDBGuid(withDefaulting: true, IsKey = true, BqlField = typeof(RecognizedRecord.refNbr))]
        [PXExtraKey]
        public virtual Guid? RecognizedRecordRefNbr { get; set; }
        public abstract class recognizedRecordRefNbr : BqlGuid.Field<recognizedRecordRefNbr> { }

        [PXUIField(DisplayName = "Entity Type")]
        [PXDefault]
        [RecognizedRecordEntityTypeList]
        [PXDBString(3, IsFixed = true, BqlField = typeof(RecognizedRecord.entityType))]
        public virtual string EntityType { get; set; }
        public abstract class entityType : BqlString.Field<entityType> { }

		[PXUIField(DisplayName = "File Hash")]
		[PXDBBinary(16, IsFixed = true, BqlField = typeof(RecognizedRecord.fileHash))]
		public virtual byte[] FileHash { get; set; }
		public abstract class fileHash : BqlByteArray.Field<fileHash> { }

		[PXUIField(DisplayName = "Status")]
		[PXDefault(RecognizedRecordStatusListAttribute.PendingRecognition)]
		[RecognizedRecordStatusList]
		[PXDBString(1, IsFixed = true, BqlField = typeof(RecognizedRecord.status))]
		public virtual string RecognizedRecordStatus { get; set; }
		public abstract class recognizedRecordStatus : BqlString.Field<recognizedRecordStatus> { }

		[PXUIField(DisplayName = "Recognition Started", Enabled = false)]
		[PXDefault(false)]
		[PXDBBool(BqlField = typeof(RecognizedRecord.recognitionStarted))]
		public virtual bool? RecognitionStarted { get; set; }
		public abstract class recognitionStarted : BqlBool.Field<recognitionStarted> { }

		[PXUIField(DisplayName = "Recognition Result", Visible = false)]
		[PXDBString(IsUnicode = true, BqlField = typeof(RecognizedRecord.recognitionResult))]
		public virtual string RecognitionResult { get; set; }
		public abstract class recognitionResult : BqlString.Field<recognitionResult> { }

        [PXUIField(DisplayName = "Recognition Feedback", Visible = false)]
        [PXDBString(IsUnicode = true)]
        public virtual string RecognitionFeedback { get; set; }
        public abstract class recognitionFeedback : BqlString.Field<recognitionFeedback> { }

		[PXUIField(DisplayName = "Document Link", Visible = false)]
		[PXDBGuid(BqlField = typeof(RecognizedRecord.documentLink))]
		public virtual Guid? DocumentLink { get; set; }
		public abstract class documentLink : BqlGuid.Field<documentLink> { }

		[PXUIField(DisplayName = "Link to Duplicate File", Visible = false)]
		[PXDBGuid(BqlField = typeof(RecognizedRecord.duplicateLink))]
		public virtual Guid? DuplicateLink { get; set; }
		public abstract class duplicateLink : BqlGuid.Field<duplicateLink> { }

		[PXDBString(500, IsUnicode = true, BqlField = typeof(RecognizedRecord.mailFrom))]
		[PXUIField(DisplayName = "From", Enabled = false)]
		public virtual string MailFrom { get; set; }
		public abstract class mailFrom : BqlString.Field<mailFrom> { }

		[PXDBString(256, IsUnicode = true, BqlField = typeof(RecognizedRecord.subject))]
		[PXUIField(DisplayName = "Summary", Enabled = false)]
		public virtual string Subject { get; set; }
		public abstract class subject : BqlString.Field<subject> { }

		[PXUIField(DisplayName = "Message ID", Enabled = false)]
		[PXDBString(255, IsUnicode = true, BqlField = typeof(RecognizedRecord.messageID))]
		public virtual string MessageID { get; set; }
		public abstract class messageID : BqlString.Field<messageID> { }

		[PXUIField(DisplayName = "Owner", Enabled = false)]
		[PXDBInt(BqlField = typeof(RecognizedRecord.owner))]
		public virtual int? Owner { get; set; }
		public abstract class owner : BqlInt.Field<owner> { }

		[PXUIField(DisplayName = "Custom Info", Visible = false)]
		[PXDBString(IsUnicode = true, BqlField = typeof(RecognizedRecord.customInfo))]
		public virtual string CustomInfo { get; set; }
		public abstract class customInfo : BqlString.Field<customInfo> { }

		[PXUIField(DisplayName = "Error Message", Visible = false, Enabled = false)]
		[PXDBString(IsUnicode = true, BqlField = typeof(RecognizedRecord.errorMessage))]
		public virtual string ErrorMessage { get; set; }
		public abstract class errorMessage : BqlString.Field<errorMessage> { }

        [PXNote(BqlField = typeof(RecognizedRecord.noteID))]
        public override Guid? NoteID { get; set; }
        public abstract class noteID : BqlGuid.Field<noteID> { }

        [PXDBCreatedByID(BqlField = typeof(RecognizedRecord.createdByID))]
		public virtual Guid? RecognizedRecordCreatedByID { get; set; }
		public abstract class recognizedRecordCreatedByID : BqlGuid.Field<recognizedRecordCreatedByID> { }

		[PXDBCreatedByScreenID(BqlField = typeof(RecognizedRecord.createdByScreenID))]
		public virtual String RecognizedRecordCreatedByScreenID { get; set; }
		public abstract class recognizedRecordCreatedByScreenID : BqlString.Field<recognizedRecordCreatedByScreenID> { }

		[PXDBCreatedDateTime(BqlField = typeof(RecognizedRecord.createdDateTime))]
		public virtual DateTime? RecognizedRecordCreatedDateTime { get; set; }
		public abstract class recognizedRecordCreatedDateTime : BqlDateTime.Field<recognizedRecordCreatedDateTime> { }

		[PXDBLastModifiedByID(BqlField = typeof(RecognizedRecord.lastModifiedByID))]
		public virtual Guid? RecognizedRecordLastModifiedByID { get; set; }
		public abstract class recognizedRecordLastModifiedByID : BqlGuid.Field<recognizedRecordLastModifiedByID> { }

		[PXDBLastModifiedByScreenID(BqlField = typeof(RecognizedRecord.lastModifiedByScreenID))]
		public virtual string RecognizedRecordLastModifiedByScreenID { get; set; }
		public abstract class recognizedRecordLastModifiedByScreenID : BqlString.Field<recognizedRecordLastModifiedByScreenID> { }

		[PXDBLastModifiedDateTime(BqlField = typeof(RecognizedRecord.lastModifiedDateTime))]
		public virtual DateTime? RecognizedRecordLastModifiedDateTime { get; set; }
		public abstract class recognizedRecordLastModifiedDateTime : BqlDateTime.Field<recognizedRecordLastModifiedDateTime> { }

		[PXDBTimestamp(VerifyTimestamp = VerifyTimestampOptions.FromRecord, BqlField = typeof(RecognizedRecord.tStamp))]
		public virtual byte[] RecognizedRecordTStamp { get; set; }
		public abstract class recognizedRecordTStamp : BqlByteArray.Field<recognizedRecordTStamp> { }

        #endregion

        internal DocumentFeedbackBuilder FeedbackBuilder { get; set; }

		internal Dictionary<string, Uri> Links { get; set; }

		[PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
		[PXBool]
		[PXUIField(DisplayName = "Is Redirect")]
		public virtual bool? IsRedirect { get; set; }
		public abstract class isRedirect : BqlBool.Field<isRedirect> { }

		[PXUIField(DisplayName = "Status", Enabled = false)]
        [PXDefault(APRecognizedInvoiceRecognitionStatusListAttribute.PendingFile, PersistingCheck = PXPersistingCheck.Nothing)]
        [APRecognizedInvoiceRecognitionStatusList]
        [PXString(1, IsFixed = true)]
        public virtual string RecognitionStatus { get; set; }
        public abstract class recognitionStatus : BqlString.Field<status> { }

        [PXDefault(APInvoiceRecognitionEntry.PdfExtension, PersistingCheck = PXPersistingCheck.Nothing)]
        [PXString]
        [PXUIField(DisplayName = "Allow Files", Visible = false)]
        public virtual string AllowFiles { get; set; }
        public abstract class allowFiles : BqlString.Field<allowFiles> { }

        [PXString]
        [PXUIField(DisplayName = "Allow File Message", Visible = false)]
        public virtual string AllowFilesMsg { get; set; }
        public abstract class allowFilesMsg : BqlString.Field<allowFilesMsg> { }

		[PXDefault(true, PersistingCheck = PXPersistingCheck.Nothing)]
		[PXBool]
		[PXUIField(DisplayName = "Allow File Upload", Visible = false)]
		public virtual bool? AllowUploadFile { get; set; }
		public abstract class allowUploadFile : BqlBool.Field<allowUploadFile> { }

		[PXGuid]
		[PXUIField(DisplayName = "File ID", Visible = false)]
		public virtual Guid? FileID { get; set; }
		public abstract class fileID : BqlGuid.Field<fileID> { }

		[PXString]
		[PXUIField(DisplayName = "Recognized Data", Visible = false)]
		public virtual string RecognizedDataJson { get; set; }
		public abstract class recognizedDataJson : BqlString.Field<recognizedDataJson> { }

		[PXInt]
		[PXUIField(DisplayName = "Vendor Term", Visible = false)]
		public virtual int? VendorTermIndex { get; set; }
		public abstract class vendorTermIndex : BqlString.Field<vendorTermIndex> { }

		[PXString(IsUnicode = true)]
		public virtual string VendorName { get; set; }
		public abstract class vendorName : BqlString.Field<vendorName> { }

		[PXString]
		public virtual string VendorSearchError { get; set; }
		public abstract class vendorSearchError : BqlString.Field<vendorSearchError> { }

		[PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
		[PXBool]
		[PXUIField(DisplayName = "Is Data Loaded")]
		public virtual bool? IsDataLoaded { get; set; }

		public abstract class isDataLoaded : BqlBool.Field<isDataLoaded> { }

		public new abstract class docType : BqlString.Field<docType> { }

		public new abstract class curyLineTotal : BqlDecimal.Field<curyLineTotal> { }

		public new abstract class refNbr : BqlString.Field<refNbr> { }

		public new abstract class docDate : BqlDateTime.Field<docDate> { }

		public new abstract class vendorID : BqlInt.Field<vendorID> { }

		public new abstract class vendorLocationID : BqlInt.Field<vendorLocationID> { }

		public new abstract class suppliedByVendorID : BqlInt.Field<suppliedByVendorID> { }

		public new abstract class suppliedByVendorLocationID : BqlInt.Field<suppliedByVendorLocationID> { }
	}
}
