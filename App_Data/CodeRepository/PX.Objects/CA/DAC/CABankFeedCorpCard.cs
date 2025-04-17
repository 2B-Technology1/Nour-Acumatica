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
using PX.Objects.EP;
using PX.Objects.EP.DAC;
using PX.Data.ReferentialIntegrity.Attributes;

namespace PX.Objects.CA
{
    [PXCacheName("Bank Feed Corporate Cards")]
    public class CABankFeedCorpCard : IBqlTable
    {
        #region Keys
        public class PK : PrimaryKeyOf<CABankFeedCorpCard>.By<bankFeedID, lineNbr>
        {
            public static CABankFeedCorpCard Find(PXGraph graph, string bankFeedID, int? bankFeedCorpCardID, PKFindOptions options = PKFindOptions.None) => FindBy(graph, bankFeedID, bankFeedCorpCardID, options);
        }

        public static class FK
        {
            public class BankFeed : CABankFeed.PK.ForeignKeyOf<CABankFeedCorpCard>.By<bankFeedID> { }
            public class Employee : EPEmployee.PK.ForeignKeyOf<CABankFeedCorpCard>.By<employeeID> { }
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
        public virtual int? LineNbr { get; set; }
        public abstract class lineNbr : PX.Data.BQL.BqlInt.Field<lineNbr> { }
        #endregion

        #region AccountID
        [PXDBString(100)]
		[PXDefault]
        [PXUIField(DisplayName = "Account Name", Required = true)]
        [PXSelector(typeof(Search<CABankFeedDetail.accountID,
            Where<CABankFeedDetail.bankFeedID, Equal<Current<CABankFeed.bankFeedID>>, And<CABankFeedDetail.cashAccountID, IsNotNull>>>),
			typeof(CABankFeedDetail.accountName), typeof(CABankFeedDetail.accountMask))]
        public virtual string AccountID { get; set; }
        public abstract class accountID : PX.Data.BQL.BqlString.Field<accountID> { }
		#endregion

		#region CashAccountCD
		public abstract class cashAccountID : PX.Data.BQL.BqlString.Field<cashAccountID> { }
		[PXDBInt]
		[PXSelector(typeof(Search<CashAccount.cashAccountID>),
			SubstituteKey = typeof(CashAccount.cashAccountCD),
			DescriptionField = typeof(CashAccount.descr))]
		[PXUIField(DisplayName = "Cash Account", Enabled = false)]
		public virtual int? CashAccountID { get; set; }
		#endregion

		#region CorpCardID
		[PXDBInt]
        [PXDefault]
        [PXUIField(DisplayName = "Corporate Card ID", Required = true)]
        [PXSelector(typeof(Search<CACorpCard.corpCardID, Where<CACorpCard.isActive, Equal<True>,
			And<CACorpCard.cashAccountID, Equal<Current<CABankFeedCorpCard.cashAccountID>>>>>),
            SubstituteKey = typeof(CACorpCard.corpCardCD),
            DescriptionField = typeof(CACorpCard.name))]
        public virtual int? CorpCardID { get; set; }
        public abstract class corpCardID : PX.Data.BQL.BqlInt.Field<corpCardID> { }
		#endregion

		#region CardNumber
		public abstract class cardNumber : PX.Data.BQL.BqlString.Field<cardNumber> { }
		[PXString]
		[PXFormula(typeof(Selector<corpCardID, CACorpCard.cardNumber>))]
		[PXUIField(DisplayName = "Card Number", Enabled = false)]
		public virtual string CardNumber { get; set; }
		#endregion

		#region CardName
		public abstract class cardName : PX.Data.BQL.BqlString.Field<cardName> { }
		[PXString]
		[PXFormula(typeof(Selector<corpCardID, CACorpCard.name>))]
		[PXUIField(DisplayName = "Name", Enabled = false)]
		public virtual string CardName { get; set; }
		#endregion

		#region EmployeeID
		[PXDBInt]
        [PXDefault]
        [PXSelector(typeof(Search2<EPEmployee.bAccountID,
            InnerJoin<EPEmployeeCorpCardLink, On<EPEmployee.bAccountID, Equal<EPEmployeeCorpCardLink.employeeID>>>,
            Where<EPEmployeeCorpCardLink.corpCardID, Equal<Current<CABankFeedCorpCard.corpCardID>>>>),
            SubstituteKey = typeof(EPEmployee.acctCD), DescriptionField = typeof(EPEmployee.acctName))]
        [PXUIField(DisplayName = "Employee ID", Required = true)]
        public virtual int? EmployeeID { get; set; }
        public abstract class employeeID : PX.Data.BQL.BqlInt.Field<employeeID> { }
		#endregion

		#region EmployeeName
		public abstract class employeeName : PX.Data.BQL.BqlString.Field<employeeName> { }
		[PXString]
		[PXFormula(typeof(Selector<employeeID, EPEmployee.acctName>))]
		[PXUIField(DisplayName = "Employee Name", Enabled = false)]
		public virtual string EmployeeName { get; set; }
		#endregion

		#region MatchField
		public abstract class matchField : PX.Data.BQL.BqlString.Field<matchField> { }
		[PXDBString(1)]
		[PXDefault(CABankFeedMatchField.Empty)]
		[CABankFeedMatchField.List(CABankFeedMatchField.SetOfValues.CorporateCard)]
        [PXUIField(DisplayName = "Field to Match")]
        public virtual string MatchField { get; set; }
		#endregion

		#region MatchRule
		public abstract class matchRule : PX.Data.BQL.BqlString.Field<matchRule> { }
		[PXDBString(1)]
		[PXDefault(CABankFeedMatchRule.Empty)]
		[CABankFeedMatchRule.List(true)]
		[PXUIField(DisplayName = "Rule")]
		public virtual string MatchRule { get; set; }
		#endregion

		#region MatchValue
		public abstract class matchValue : PX.Data.BQL.BqlString.Field<matchValue> { }
		[PXDefault]
		[PXDBString(100, IsUnicode = true)]
        [PXUIField(DisplayName = "Value", Enabled = false)]
        public virtual string MatchValue { get; set; }
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
