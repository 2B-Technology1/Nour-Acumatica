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
using PX.Objects.IN;
using PX.Data.ReferentialIntegrity.Attributes;

namespace PX.Objects.CA
{
    [PXCacheName("Bank Feed Expense Items")]
    public class CABankFeedExpense : IBqlTable
    {
        #region Keys
        public class PK : PrimaryKeyOf<CABankFeedExpense>.By<bankFeedID, lineNbr>
        {
            public static CABankFeedExpense Find(PXGraph graph, string bankFeedID, int? bankFeedExpenseID, PKFindOptions options = PKFindOptions.None) => FindBy(graph, bankFeedID, bankFeedExpenseID, options);
        }

        public static class FK
        {
            public class BankFeed : CABankFeed.PK.ForeignKeyOf<CABankFeedExpense>.By<bankFeedID> { }
            public class InventoryItem : IN.InventoryItem.PK.ForeignKeyOf<CABankFeedExpense>.By<inventoryItemID> { }
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

        #region MatchRule
        [PXDBString(1)]
		[PXDefault(CABankFeedMatchRule.StartsWith)]
		[CABankFeedMatchRule.List(false)]
		[PXUIField(DisplayName = "Rule", Required = true)]
        public virtual string MatchRule { get; set; }
        public abstract class matchRule : PX.Data.BQL.BqlString.Field<matchRule> { }
        #endregion

        #region MatchField
        [PXDBString(1)]
        [PXDefault(CABankFeedMatchField.Category)]
        [CABankFeedMatchField.List(CABankFeedMatchField.SetOfValues.ExpenseReceipts)]
        [PXUIField(DisplayName = "Field to Match", Required = true)]
        public virtual string MatchField { get; set; }
        public abstract class matchField : PX.Data.BQL.BqlString.Field<matchField> { }
        #endregion

        #region MatchValue
        [PXDefault]
        [PXDBString(100, IsUnicode = true)]
        [PXUIField(DisplayName = "Value", Required = true)]
        public virtual string MatchValue { get; set; }
        public abstract class matchValue : PX.Data.BQL.BqlString.Field<matchValue> { }
        #endregion

        #region InventoryItemID
        [PXDBInt]
        [PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
        [PXSelector(typeof(Search<InventoryItem.inventoryID, Where<InventoryItem.itemType, Equal<INItemTypes.expenseItem>,
			And<InventoryItem.itemStatus, NotEqual<InventoryItemStatus.inactive>,
			And<InventoryItem.itemStatus, NotEqual<InventoryItemStatus.markedForDeletion>>>>>),
            SubstituteKey = typeof(InventoryItem.inventoryCD),
			DescriptionField = typeof(InventoryItem.descr))]
        [PXUIField(DisplayName = "Expense Item")]
        public virtual int? InventoryItemID { get; set; }
        public abstract class inventoryItemID : PX.Data.BQL.BqlInt.Field<inventoryItemID> { }
        #endregion

        #region DoNotCreate
        [PXDBBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Skip")]
        public virtual bool? DoNotCreate { get; set; }
        public abstract class doNotCreate : PX.Data.BQL.BqlBool.Field<doNotCreate> { }
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
