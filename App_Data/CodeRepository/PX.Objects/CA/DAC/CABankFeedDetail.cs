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
using PX.Data.ReferentialIntegrity.Attributes;
using System;

namespace PX.Objects.CA
{
    [PXCacheName("Bank Feed Detail")]
    public class CABankFeedDetail : IBqlTable
    {
        #region Keys
        public class PK : PrimaryKeyOf<CABankFeedDetail>.By<bankFeedID, lineNbr>
        {
            public static CABankFeedDetail Find(PXGraph graph, string bankFeedID, int? bankFeedDetailID, PKFindOptions options = PKFindOptions.None) => FindBy(graph, bankFeedID, bankFeedDetailID, options);
        }

        public static class FK
        {
            public class BankFeed : CABankFeed.PK.ForeignKeyOf<CABankFeedDetail>.By<bankFeedID> { }
            public class CashAccount : CA.CashAccount.PK.ForeignKeyOf<CABankFeedDetail>.By<cashAccountID> { }
        }
		#endregion

		#region BankFeedID
		public abstract class bankFeedID : PX.Data.BQL.BqlString.Field<bankFeedID> { }
		[PXDBString(10, IsUnicode = true, IsKey = true)]
		[PXDBDefault(typeof(CABankFeed.bankFeedID))]
		[PXParent(typeof(FK.BankFeed))]
		public virtual string BankFeedID { get; set; }
		#endregion

		#region LineNbr
		[PXDBInt(IsKey = true)]
		[PXLineNbr(typeof(CABankFeed))]
		[PXUIField(Visible = false)]
        public virtual int? LineNbr { get; set; }
        public abstract class lineNbr : PX.Data.BQL.BqlInt.Field<lineNbr> { }
        #endregion

        #region AccountID
        [PXDBString(100, IsUnicode = true)]
        [PXUIField(DisplayName = "Account ID", Enabled = false)]
        public virtual string AccountID { get; set; }
        public abstract class accountID : PX.Data.BQL.BqlString.Field<accountID> { }
        #endregion

        #region AccountName
        [PXDBString(250)]
        [PXUIField(DisplayName = "Account Name", Enabled = false)]
        public virtual string AccountName { get; set; }
        public abstract class accountName : PX.Data.BQL.BqlString.Field<accountName> { }
		#endregion

		#region Account Mask
		[PXDBString(50)]
        [PXUIField(DisplayName = "Account Mask", Enabled = false)]
        public virtual string AccountMask { get; set; }
        public abstract class accountMask : PX.Data.BQL.BqlString.Field<accountMask> { }
        #endregion

        #region AccountType
        [PXDBString(100)]
        [PXUIField(DisplayName = "Account Type", Enabled = false)]
        public virtual string AccountType { get; set; }
        public abstract class accountType : PX.Data.BQL.BqlString.Field<accountType> { }
        #endregion

        #region SubType
        [PXDBString(100)]
        [PXUIField(DisplayName = "Account Subtype", Enabled = false)]
        public virtual string AccountSubType { get; set; }
        public abstract class accountSubType : PX.Data.BQL.BqlString.Field<accountSubType> { }
		#endregion

		#region Currency
		public abstract class currency : PX.Data.BQL.BqlString.Field<currency> { }
		[PXDBString(5, IsUnicode = true)]
		[PXUIField(DisplayName = "Currency", Enabled = false)]
		public virtual string Currency { get; set; }
		#endregion

		#region Descr
		[PXDBString(60)]
        [PXUIField(DisplayName = "Description")]
        public virtual string Descr { get; set; }
        public abstract class descr : PX.Data.BQL.BqlString.Field<descr> { }
		#endregion

		#region CashAccountID
		public abstract class cashAccountID : PX.Data.BQL.BqlInt.Field<cashAccountID> { }
		[PXDBInt]
		[PXSelector(typeof(Search<CashAccount.cashAccountID>), SubstituteKey = typeof(CashAccount.cashAccountCD),
			DescriptionField = typeof(CashAccount.descr))]
		[PXUIField(DisplayName = "Cash Account")]
		public virtual int? CashAccountID { get; set; }
		#endregion

		#region StatementPeriod
		[PXDefault(CABankFeedStatementPeriod.Month)]
		[PXDBString]
		[CABankFeedStatementPeriod.List]
		[PXUIField(DisplayName = "Statement Period")]
		public virtual string StatementPeriod { get; set; }
		public abstract class statementPeriod : PX.Data.BQL.BqlString.Field<statementPeriod> { }
		#endregion

		#region StatementStartDay
		[PXDefault(1)]
		[PXDBInt]
		[CABankFeedStatementStartDay(typeof(statementPeriod))]
		[PXUIField(DisplayName = "Statement Start Day")]
		public virtual int? StatementStartDay { get; set; }
		public abstract class statementStartDay : PX.Data.BQL.BqlInt.Field<statementStartDay> { }
		#endregion

		#region ImportStartDate
		public abstract class importStartDate : PX.Data.BQL.BqlDateTime.Field<importStartDate> { }
		[PXDate]
		[PXUIField(DisplayName = "Import Transactions From", Enabled = false)]
		public virtual DateTime? ImportStartDate { get; set; }
		#endregion

		#region RetrievalStatus
		public abstract class retrievalStatus : PX.Data.BQL.BqlString.Field<retrievalStatus> { }
		[PXDBString(1, IsFixed = true)]
		[CABankFeedRetrievalStatus.List]
		[PXUIField(DisplayName = "Retrieval Status", Enabled = false)]
		public virtual string RetrievalStatus { get; set; }
		#endregion

		#region RetrievalDate
		public abstract class retrievalDate : PX.Data.BQL.BqlDateTime.Field<retrievalDate> { }
		[PXDBDate(PreserveTime = true)]
		[PXUIField(DisplayName = "Retrieval Date", Enabled = false)]
		public DateTime? RetrievalDate { get; set; }
		#endregion

		#region ErrorMessage
		public abstract class errorMessage : PX.Data.BQL.BqlString.Field<errorMessage> { }
		[PXDBString(250, IsUnicode = true)]
		[PXUIField(DisplayName = "Error message", Enabled = false)]
		public virtual string ErrorMessage { get; set; }
		#endregion

		#region Hidden
		public abstract class hidden : PX.Data.BQL.BqlBool.Field<hidden> { }
		/// <summary>
		/// Specifies (if set to <c>true</c>) that this Cash Account has been hidden from details on the Bank Feed (CA205500) form.
		/// </summary>
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Hidden", Enabled = false)]
		public virtual bool? Hidden { get; set; }
		#endregion


		#region Tstamp
		[PXDBTimestamp()]
        [PXUIField(DisplayName = "Tstamp")]
        public virtual byte[] Tstamp { get; set; }
        public abstract class tstamp : PX.Data.BQL.BqlByteArray.Field<tstamp> { }
        #endregion

        #region CreatedByID
        [PXDBCreatedByID()]
        public virtual Guid? CreatedByID { get; set; }
        public abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID> { }
        #endregion

        #region CreatedByScreenID
        [PXDBCreatedByScreenID()]
        public virtual string CreatedByScreenID { get; set; }
        public abstract class createdByScreenID : PX.Data.BQL.BqlString.Field<createdByScreenID> { }
        #endregion

        #region CreatedDateTime
        [PXDBCreatedDateTime()]
        [PXUIField(DisplayName = "Created Date Time")]
        public virtual DateTime? CreatedDateTime { get; set; }
        public abstract class createdDateTime : PX.Data.BQL.BqlDateTime.Field<createdDateTime> { }
        #endregion

        #region LastModifiedByID
        [PXDBLastModifiedByID()]
        public virtual Guid? LastModifiedByID { get; set; }
        public abstract class lastModifiedByID : PX.Data.BQL.BqlGuid.Field<lastModifiedByID> { }
        #endregion

        #region LastModifiedByScreenID
        [PXDBLastModifiedByScreenID()]
        public virtual string LastModifiedByScreenID { get; set; }
        public abstract class lastModifiedByScreenID : PX.Data.BQL.BqlString.Field<lastModifiedByScreenID> { }
        #endregion

        #region LastModifiedDateTime
        [PXDBLastModifiedDateTime()]
        [PXUIField(DisplayName = "Last Modified Date Time")]
        public virtual DateTime? LastModifiedDateTime { get; set; }
        public abstract class lastModifiedDateTime : PX.Data.BQL.BqlDateTime.Field<lastModifiedDateTime> { }
        #endregion

        #region Noteid
        [PXNote()]
        [PXUIField(DisplayName = "Noteid")]
        public virtual Guid? Noteid { get; set; }
        public abstract class noteid : PX.Data.BQL.BqlGuid.Field<noteid> { }
		#endregion
    }
}
