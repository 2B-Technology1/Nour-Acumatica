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

using PX.Common;
using PX.Data;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.IN;
using System;

namespace PX.Objects.CA
{
	[PXCacheName("Bank Feed")]
	[PXPrimaryGraph(typeof(CABankFeedMaint))]
	public class CABankFeed : IBqlTable
	{
		#region Keys
		public class PK : PrimaryKeyOf<CABankFeed>.By<bankFeedID>
		{
			public static CABankFeed Find(PXGraph graph, string bankFeedID, PKFindOptions options = PKFindOptions.None) => FindBy(graph, bankFeedID, options);
		}

		public static class FK
		{
			public class ExternalUserID : CABankFeedUser.PK.ForeignKeyOf<CABankFeedUser>.By<externalUserID> { }
		}
		#endregion

		#region Selected
		public abstract class selected : PX.Data.BQL.BqlBool.Field<selected> { }
		[PXBool]
		[PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Selected")]
		public bool? Selected
		{
			get;
			set;
		}
		#endregion

		#region BankFeedID
		public abstract class bankFeedID : PX.Data.BQL.BqlString.Field<bankFeedID> { }
		[PXDBDefault]
		[PXDBString(10, IsUnicode = true, IsKey = true)]
		[PXUIField(DisplayName = "Bank Feed ID")]
		[PXSelector(typeof(Search<CABankFeed.bankFeedID>))]
		public virtual string BankFeedID { get; set; }
		#endregion

		#region OrganizationID
		public abstract class organizationID : PX.Data.BQL.BqlInt.Field<organizationID> { }

		[PXDBInt]
		[PXDefault(0)]
		public int? OrganizationID { get; set; }
		#endregion

		#region Status
		public abstract class status : PX.Data.BQL.BqlString.Field<status> { }
		[PXDBString(1, IsFixed = true)]
		[PXDefault(CABankFeedStatus.Disconnected)]
		[CABankFeedStatus.List]
		[PXUIField(DisplayName = "Status")]
		public virtual string Status { get; set; }
		#endregion

		#region RetrievalStatus
		public abstract class retrievalStatus : PX.Data.BQL.BqlString.Field<retrievalStatus> { }
		[PXDBString(1, IsFixed = true)]
		[CABankFeedRetrievalStatus.List]
		[PXUIField(DisplayName = "Retrieval Status")]
		public virtual string RetrievalStatus { get; set; }
		#endregion

		#region RetrievalDate
		public abstract class retrievalDate : PX.Data.BQL.BqlDateTime.Field<retrievalDate> { }
		[PXDBDate(PreserveTime = true)]
		[PXUIField(DisplayName = "Retrieval Date")]
		public DateTime? RetrievalDate { get; set; }
		#endregion

		#region BankFeedType
		public abstract class type : PX.Data.BQL.BqlString.Field<type> { }
		[PXDBString]
        [PXDefault]
        [CABankFeedType.List]
        [PXUIField(DisplayName = "Bank Feed Type")]
        public virtual string Type { get; set; }
		#endregion

		#region AccessToken
		public abstract class accessToken : PX.Data.BQL.BqlString.Field<accessToken> { }
		[PXRSACryptString(IsUnicode = true, IsViewDecrypted = true)]
		[PXUIField(Enabled = false)]
		public virtual string AccessToken { get; set; }
		#endregion

		#region ExternalItemID
		public abstract class externalItemID : PX.Data.BQL.BqlString.Field<externalItemID> { }
		[PXDBString(50, IsUnicode = true)]
		[PXUIField(DisplayName = "Item ID/Member ID", Enabled = false)]
		public virtual string ExternalItemID { get; set; }
		#endregion

		#region ExternalUserID
		public abstract class externalUserID : PX.Data.BQL.BqlInt.Field<externalUserID> { }
		[PXDBString(50, IsUnicode = true)]
		[PXUIField(DisplayName = "User ID", Enabled = false)]
		public virtual string ExternalUserID { get; set; }
		#endregion

		#region Institution
		public abstract class institution : PX.Data.BQL.BqlString.Field<institution> { }

		[PXDBString(150, IsUnicode = true)]
        [PXUIField(DisplayName = "Financial Institution", Enabled = false)]
        public virtual string Institution { get; set; }
		#endregion

		#region InstitutionID
		public abstract class institutionID : PX.Data.BQL.BqlString.Field<institutionID> { }
		[PXDBString(50, IsUnicode = true)]
        [PXUIField(DisplayName = "", Enabled = false)]
        public virtual string InstitutionID { get; set; }
		#endregion

		#region Descr
		public abstract class descr : PX.Data.BQL.BqlString.Field<descr> { }
		[PXDBString(Common.Constants.TranDescLength, IsUnicode = true)]
        [PXUIField(DisplayName = "Description")]
        public virtual string Descr { get; set; }
		#endregion

		#region CreateExpenseReceipts
		public abstract class createExpenseReceipt : PX.Data.BQL.BqlString.Field<createExpenseReceipt> { }
		[PXDefault(false)]
        [PXDBDefault]
        [PXDBBool]
        [PXUIField(DisplayName = "Create Expense Receipts")]
        public virtual bool? CreateExpenseReceipt { get; set; }
		#endregion

		#region CreateReceiptForPendingTran
		public abstract class createReceiptForPendingTran : PX.Data.BQL.BqlString.Field<createReceiptForPendingTran> { }
		[PXDefault(false)]
        [PXDBDefault]
        [PXDBBool]
        [PXUIField(DisplayName = "Create Expense Receipts for Pending Transactions")]
        public virtual bool? CreateReceiptForPendingTran { get; set; }
		#endregion

		#region DefaultExpenseItemID
		public abstract class defaultExpenseItemID : PX.Data.BQL.BqlInt.Field<defaultExpenseItemID> { }
		[PXDBInt]
		[PXSelector(typeof(Search<InventoryItem.inventoryID, Where<InventoryItem.itemType, Equal<INItemTypes.expenseItem>,
			And<InventoryItem.itemStatus, NotEqual<InventoryItemStatus.inactive>,
			And<InventoryItem.itemStatus, NotEqual<InventoryItemStatus.markedForDeletion>>>>>),
			SubstituteKey = typeof(InventoryItem.inventoryCD),
			DescriptionField = typeof(InventoryItem.descr))]
		[PXUIField(DisplayName = "Default Expense Item", Enabled = false)]
		public virtual int? DefaultExpenseItemID { get; set; }
		#endregion

		#region ImportStartDate
		public abstract class importStartDate : PX.Data.BQL.BqlDateTime.Field<importStartDate> { }
		[PXDBDate]
		[PXDefault]
		[PXUIField(DisplayName = "Import Start Date")]
		public virtual DateTime? ImportStartDate { get; set; }
		#endregion

		#region ErrorMessage
		public abstract class errorMessage : PX.Data.BQL.BqlString.Field<errorMessage> { }
		[PXDBString(250, IsUnicode = true)]
		[PXUIField(DisplayName = "Error message")]
		public virtual string ErrorMessage { get; set; }
		#endregion

		#region IsTestFeed
		public abstract class isTestFeed : PX.Data.BQL.BqlBool.Field<isTestFeed> { }

		/// <summary>
		/// Returns <c>true</c> when Bank Feed is using a sandbox
		/// </summary>
		[PXBool]
		public virtual bool? IsTestFeed
		{
			get => this.Type == CABankFeedType.TestPlaid;
			set { }
		}
		#endregion

		#region
		public abstract class statementImportSource : PX.Data.BQL.BqlString.Field<statementImportSource> { }

		[PXString]
		[PXUIField(DisplayName = "Statement Import Source", Enabled = false, Visible = false)]
		public virtual string StatementImportSource { get; set; }
		#endregion

		#region CreatedByID
		public abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID> { }
		[PXDBCreatedByID]
        public virtual Guid? CreatedByID { get; set; }
		#endregion

		#region CreatedByScreenID
		public abstract class createdByScreenID : PX.Data.BQL.BqlString.Field<createdByScreenID> { }

		[PXDBCreatedByScreenID]
        public virtual string CreatedByScreenID { get; set; }
		#endregion

		#region AccountQty
		public abstract class accountQty : PX.Data.BQL.BqlInt.Field<accountQty> { }
		[PXInt]
		[PXDBScalar(typeof(Search4<CABankFeedDetail.lineNbr,
			Where<CABankFeedDetail.bankFeedID, Equal<CABankFeed.bankFeedID>>,
			Aggregate<Count<CABankFeedDetail.lineNbr>>>))]
		[PXUIField(DisplayName = "Accounts")]
		public virtual int? AccountQty { get; set; }
		#endregion

		#region UnmatchedAccountQty
		public abstract class unmatchedAccountQty : PX.Data.BQL.BqlInt.Field<unmatchedAccountQty> { }
		[PXInt]
		[PXDBScalar(typeof(Search4<CABankFeedDetail.lineNbr,
			Where<CABankFeedDetail.bankFeedID, Equal<CABankFeed.bankFeedID>, And<CABankFeedDetail.cashAccountID,IsNull>>,
			Aggregate<Count<CABankFeedDetail.lineNbr>>>))]
		[PXUIField(DisplayName = "Unmatched Accounts")]
		public virtual int? UnmatchedAccountQty { get; set; }
		#endregion

		#region CreatedDateTime
		public abstract class createdDateTime : PX.Data.BQL.BqlDateTime.Field<createdDateTime> { }
		[PXDBCreatedDateTime]
        [PXUIField(DisplayName = "Created Date Time")]
        public virtual DateTime? CreatedDateTime { get; set; }
		#endregion

		#region LastModifiedByID
		public abstract class lastModifiedByID : PX.Data.BQL.BqlGuid.Field<lastModifiedByID> { }
		[PXDBLastModifiedByID]
        public virtual Guid? LastModifiedByID { get; set; }
		#endregion

		#region LastModifiedByScreenID
		public abstract class lastModifiedByScreenID : PX.Data.BQL.BqlString.Field<lastModifiedByScreenID> { }
		[PXDBLastModifiedByScreenID]
        public virtual string LastModifiedByScreenID { get; set; }
		#endregion

		#region LastModifiedDateTime
		public abstract class lastModifiedDateTime : PX.Data.BQL.BqlDateTime.Field<lastModifiedDateTime> { }
		[PXDBLastModifiedDateTime]
        [PXUIField(DisplayName = "Last Modified Date Time")]
        public virtual DateTime? LastModifiedDateTime { get; set; }
		#endregion

		#region Noteid
		public abstract class noteid : PX.Data.BQL.BqlGuid.Field<noteid> { }
		[PXNote]
        [PXUIField(DisplayName = "Noteid")]
        public virtual Guid? Noteid { get; set; }
		#endregion

		#region Tstamp
		public abstract class tstamp : PX.Data.BQL.BqlByteArray.Field<tstamp> { }
		[PXDBTimestamp]
		[PXUIField(DisplayName = "Tstamp")]
		public virtual byte[] Tstamp { get; set; }
		#endregion
	}
}
