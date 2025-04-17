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
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.CM;
using PX.Objects.Common.Bql;

namespace PX.Objects.CA
{
	[Serializable]
	[PXCacheName("CCBatchStatistics")]
	public class CCBatchStatistics : IBqlTable
	{
		#region Keys
		public class PK : PrimaryKeyOf<CCBatchStatistics>.By<CCBatchStatistics.batchID, CCBatchStatistics.procCenterCardTypeCode>
		{
			public static CCBatchStatistics Find(PXGraph graph, int? batchID, string cardType, PKFindOptions options = PKFindOptions.None) => FindBy(graph, batchID, cardType, options);
		}

		public static class FK
		{
			public class CCBatch : CA.CCBatch.PK.ForeignKeyOf<CCBatchStatistics>.By<batchID> { }
		}
		#endregion

		#region BatchID
		[PXDBInt(IsKey = true)]
		[PXUIField(DisplayName = "Batch ID")]
		[PXDBDefault(typeof(CCBatch.batchID))]
		[PXParent(typeof(Select<CCBatch, Where<CCBatch.batchID, Equal<Current<CCBatchStatistics.batchID>>>>))]	
		[PXUnboundFormula(typeof(Add<settledCount, Add<refundCount, Add<voidCount, Add<declineCount, errorCount>>>>),
			typeof(SumCalc<CCBatch.transactionCount>))]
		public virtual int? BatchID { get; set; }
		public abstract class batchID : PX.Data.BQL.BqlInt.Field<batchID> { }
		#endregion

		#region ProcCenterCardTypeCode
		public abstract class procCenterCardTypeCode : PX.Data.BQL.BqlString.Field<procCenterCardTypeCode> { }

		/// <summary>
		/// Original card type value received from the processing center.
		/// </summary>
		[PXDBString(25, IsUnicode = true, InputMask = "", IsKey = true)]
		[PXUIField(DisplayName = "Proc. Center Card Type", Enabled = false)]
		public virtual string ProcCenterCardTypeCode { get; set; }
		#endregion

		#region CardTypeCode
		public abstract class cardTypeCode : PX.Data.BQL.BqlString.Field<cardTypeCode> { }

		/// <summary>
		/// Type of a card associated with the customer payment method. 
		/// </summary>
		[PXDBString(3, IsFixed = true)]
		[PXUIField(DisplayName = "Card Type", Enabled = false)]
		[AR.CardType.List]
		public virtual string CardTypeCode { get; set; }
		#endregion

		#region DisplayCardType
		public abstract class displayCardType : PX.Data.BQL.BqlString.Field<displayCardType> { }

		/// <summary>
		/// Specifies display card type value.
		/// This is a virtual field and it has no representation in the database.
		/// </summary>
		[PXString(20, IsFixed = true)]
		[PXUIField(DisplayName = "Card Type", Enabled = false)]
		[PXFormula(typeof(Switch<
			Case<Where<cardTypeCode, IsNull>, procCenterCardTypeCode,
			Case<Where<cardTypeCode, Equal<AR.CardType.other>, And<procCenterCardTypeCode, IsNotNull>>,
				ListLabelOf<cardTypeCode>.Concat<Colon>.Concat<procCenterCardTypeCode>>>,
			ListLabelOf<cardTypeCode>>))]
		public virtual string DisplayCardType { get; set; }
		#endregion

		#region SettledAmount
		[PXDBCury(typeof(CCBatch.curyID))]
		[PXUIField(DisplayName = "Settled Amount")]
		[PXFormula(null, typeof(SumCalc<CCBatch.settledAmount>))]
		public virtual decimal? SettledAmount { get; set; }
		public abstract class settledAmount : PX.Data.BQL.BqlDecimal.Field<settledAmount> { }
		#endregion

		#region SettledCount
		[PXDBInt]
		[PXUIField(DisplayName = "Settled Count")]
		[PXFormula(null, typeof(SumCalc<CCBatch.settledCount>))]
		public virtual int? SettledCount { get; set; }
		public abstract class settledCount : PX.Data.BQL.BqlInt.Field<settledCount> { }
		#endregion

		#region RefundAmount
		[PXDBCury(typeof(CCBatch.curyID))]
		[PXUIField(DisplayName = "Refund Amount")]
		[PXFormula(null, typeof(SumCalc<CCBatch.refundAmount>))]
		public virtual decimal? RefundAmount { get; set; }
		public abstract class refundAmount : PX.Data.BQL.BqlDecimal.Field<refundAmount> { }
		#endregion

		#region RefundCount
		[PXDBInt]
		[PXUIField(DisplayName = "Refund Count")]
		[PXFormula(null, typeof(SumCalc<CCBatch.refundCount>))]
		public virtual int? RefundCount { get; set; }
		public abstract class refundCount : PX.Data.BQL.BqlInt.Field<refundCount> { }
		#endregion

		#region VoidCount
		[PXDBInt]
		[PXUIField(DisplayName = "Void Count")]
		[PXFormula(null, typeof(SumCalc<CCBatch.voidCount>))]
		public virtual int? VoidCount { get; set; }
		public abstract class voidCount : PX.Data.BQL.BqlInt.Field<voidCount> { }
		#endregion

		#region DeclineCount
		[PXDBInt]
		[PXUIField(DisplayName = "Decline Count")]
		[PXFormula(null, typeof(SumCalc<CCBatch.declineCount>))]
		public virtual int? DeclineCount { get; set; }
		public abstract class declineCount : PX.Data.BQL.BqlInt.Field<declineCount> { }
		#endregion

		#region ErrorCount
		[PXDBInt]
		[PXUIField(DisplayName = "Error Count")]
		[PXFormula(null, typeof(SumCalc<CCBatch.errorCount>))]
		public virtual int? ErrorCount { get; set; }
		public abstract class errorCount : PX.Data.BQL.BqlInt.Field<errorCount> { }
		#endregion

		#region CreatedDateTime
		[PXDBCreatedDateTime]
		public virtual DateTime? CreatedDateTime { get; set; }
		public abstract class createdDateTime : PX.Data.BQL.BqlDateTime.Field<createdDateTime> { }
		#endregion

		#region CreatedByID
		[PXDBCreatedByID]
		public virtual Guid? CreatedByID { get; set; }
		public abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID> { }
		#endregion

		#region CreatedByScreenID
		[PXDBCreatedByScreenID]
		public virtual string CreatedByScreenID { get; set; }
		public abstract class createdByScreenID : PX.Data.BQL.BqlString.Field<createdByScreenID> { }
		#endregion

		#region LastModifiedDateTime
		[PXDBLastModifiedDateTime]
		public virtual DateTime? LastModifiedDateTime { get; set; }
		public abstract class lastModifiedDateTime : PX.Data.BQL.BqlDateTime.Field<lastModifiedDateTime> { }
		#endregion

		#region LastModifiedByID
		[PXDBLastModifiedByID]
		public virtual Guid? LastModifiedByID { get; set; }
		public abstract class lastModifiedByID : PX.Data.BQL.BqlGuid.Field<lastModifiedByID> { }
		#endregion

		#region LastModifiedByScreenID
		[PXDBLastModifiedByScreenID]
		public virtual string LastModifiedByScreenID { get; set; }
		public abstract class lastModifiedByScreenID : PX.Data.BQL.BqlString.Field<lastModifiedByScreenID> { }
		#endregion

		#region Tstamp
		[PXDBTimestamp]
		[PXUIField(DisplayName = "Tstamp")]
		public virtual byte[] Tstamp { get; set; }
		public abstract class tstamp : PX.Data.BQL.BqlByteArray.Field<tstamp> { }
		#endregion

		#region Noteid
		[PXNote]
		public virtual Guid? Noteid { get; set; }
		public abstract class noteid : PX.Data.BQL.BqlGuid.Field<noteid> { }
		#endregion
	}
}
