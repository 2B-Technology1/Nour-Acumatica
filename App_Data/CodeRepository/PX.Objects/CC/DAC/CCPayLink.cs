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

namespace PX.Objects.CC
{
	///<summary>
	///DAC represents a row in the database with information about Payment Link.
	///</summary>
	[PXCacheName("Payment Link")]
	public class CCPayLink : IBqlTable
	{
		#region Keys
		public class PK : PrimaryKeyOf<CCPayLink>.By<payLinkID>
		{
			public static CCPayLink Find(PXGraph graph, int? tranNbr) => FindBy(graph, tranNbr);
		}

		public static class FK
		{
			public class ARInvoice : AR.ARInvoice.PK.ForeignKeyOf<CCPayLink>.By<docType, refNbr> { }
			public class SOOrder : SO.SOOrder.PK.ForeignKeyOf<CCPayLink>.By<orderType, orderNbr> { }
		}
		#endregion

		#region PayLinkID
		public abstract class payLinkID : Data.BQL.BqlInt.Field<payLinkID> { }
		/// <summary>
		/// Acumatica specific Payment Link Id.
		/// </summary>
		[PXDBIdentity(IsKey = true)]
		[PXUIField(DisplayName = "Pay Link ID", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual int? PayLinkID { get; set; }
		#endregion

		#region DeliveryMethod
		public abstract class deliveryMethod : Data.BQL.BqlString.Field<deliveryMethod> { }
		/// <summary>
		/// Payment Link delivery method (N - none, E - email).
		/// </summary>
		[PXDBString(1, IsFixed = true)]
		[PXDefault(PayLinkDeliveryMethod.None)]
		[PayLinkDeliveryMethod.List]
		[PXUIField(DisplayName = "Link Delivery Method")]
		public virtual string DeliveryMethod { get; set; }
		#endregion

		#region ProcessingCenterID
		public abstract class processingCenterID : Data.BQL.BqlString.Field<processingCenterID> { }
		/// <summary>
		/// Id of Processing Center related to Payment link.
		/// </summary>
		[PXDBString(10, IsUnicode = true)]
		[PXDefault]
		[PXUIField(DisplayName = "Processing Center")]
		public virtual string ProcessingCenterID { get; set; }
		#endregion

		#region CuryID
		public abstract class curyID : Data.BQL.BqlString.Field<curyID> { }
		/// <summary>
		/// Currency Id of Payment Link.
		/// </summary>
		[PXDBString(5, IsUnicode = true)]
		[PXDefault]
		[PXUIField(DisplayName = "Currency")]
		public virtual string CuryID { get; set; }
		#endregion

		#region Amount
		public abstract class amount : Data.BQL.BqlDecimal.Field<amount> { }
		/// <summary>
		/// Amount to be payed by Payment Link.
		/// </summary>
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Amount", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual decimal? Amount { get; set; }
		#endregion

		#region DueDate
		public abstract class dueDate : Data.BQL.BqlDateTime.Field<dueDate> { }
		/// <summary>
		/// Due date transferred to the Payment Link webportal.
		/// </summary>
		[PXDBDate]
		[PXDefault]
		public virtual DateTime? DueDate { get; set; }
		#endregion

		#region DocType
		public abstract class docType : Data.BQL.BqlString.Field<docType> { }
		/// <exclude/>
		[PXDBString(3)]
		[PXUIField(DisplayName = "Doc. Type", Enabled = false)]
		public virtual string DocType { get; set; }
		#endregion

		#region RefNbr
		public abstract class refNbr : Data.BQL.BqlString.Field<refNbr> { }
		/// <exclude/>
		[PXDBString(15, IsUnicode = true)]
		[PXUIField(DisplayName = "Doc. Reference Nbr.", Enabled = false)]
		public virtual string RefNbr { get; set; }
		#endregion

		#region OrderType
		public abstract class orderType : Data.BQL.BqlString.Field<orderType> { }
		/// <exclude/>
		[PXDBString(2)]
		[PXUIField(DisplayName = "Order Type", Enabled = false)]
		public virtual string OrderType { get; set; }
		#endregion

		#region OrderNbr
		public abstract class orderNbr : Data.BQL.BqlString.Field<orderNbr> { }
		/// <exclude/>
		[PXDBString(15, IsUnicode = true)]
		[PXUIField(DisplayName = "Order Reference Nbr.", Enabled = false)]
		public virtual string OrderNbr { get; set; }
		#endregion

		#region Action
		public abstract class action : Data.BQL.BqlString.Field<action> { }
		/// <summary>
		/// Interaction with the Payment link webportal (I - insert,  U - update, R - read, C - close).
		/// </summary>
		[PXDBString(1, IsFixed = true)]
		[PXDefault(PayLinkAction.Insert)]
		[PayLinkAction.List]
		public virtual string Action { get; set; }
		#endregion

		#region Status
		public abstract class actionStatus : Data.BQL.BqlString.Field<actionStatus> { }
		/// <summary>
		/// Result of the last interaction with the Payment Link webportal (O - open, S - success, E - error).
		/// </summary>
		[PXDBString(1, IsFixed = true)]
		[PXDefault(PayLinkActionStatus.Open)]
		[PayLinkActionStatus.List]
		public virtual string ActionStatus { get; set; }
		#endregion

		#region StatusDate
		public abstract class statusDate : Data.BQL.BqlDateTime.Field<statusDate> { }
		/// <summary>
		/// Date of the last interaction with the Payment Link webportal.
		/// </summary>
		[PXDefault]
		[PXDBDate(PreserveTime = true)]
		[PXUIField(DisplayName = "Status Date")]
		public DateTime? StatusDate { get; set; }
		#endregion

		#region LinkStatus
		public abstract class linkStatus : Data.BQL.BqlString.Field<linkStatus> { }
		/// <summary>
		/// Link status of Payment Link (N - none, O - open, C - closed).
		/// </summary>
		[PXDBString(1, IsFixed = true)]
		[PXDefault(PayLinkStatus.None)]
		[PayLinkStatus.List]
		[PXUIField(DisplayName = "Link Status", Enabled = false)]
		public virtual string LinkStatus { get; set; }
		#endregion

		#region PaymentStatus
		public abstract class paymentStatus : Data.BQL.BqlString.Field<paymentStatus> { }
		/// <summary>
		/// Payment status of Payment Link (N - none, U - unpaid, I - incomplete, P - paid).
		/// </summary>
		[PXDBString(1, IsFixed = true)]
		[PXDefault(PayLinkPaymentStatus.None)]
		[PayLinkPaymentStatus.List]
		[PXUIField(DisplayName = "Payment Status", Enabled = false)]
		public virtual string PaymentStatus { get; set; }
		#endregion

		#region NeedSync
		public abstract class needSync : Data.BQL.BqlBool.Field<needSync> { }
		/// <summary>
		/// Need update Payment Link after the document update.
		/// </summary>
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Synchronization Required", Enabled = false)]
		public virtual bool? NeedSync
		{
			get;
			set;
		}
		#endregion

		#region Url
		public abstract class url : Data.BQL.BqlString.Field<url> { }
		/// <summary>
		/// URL of Payment Link.
		/// </summary>
		[PXDBString(300, IsUnicode = true)]
		[PXUIField(DisplayName = "Payment Link", Enabled = false)]
		public virtual string Url { get; set; }
		#endregion

		#region ExternalID

		public abstract class externalID : Data.BQL.BqlString.Field<externalID> { }
		/// <summary>
		/// Payment Link webportal specific Id.
		/// </summary>
		[PXDBString(300, IsUnicode = true)]
		[PXUIField(DisplayName = "Link External ID", Enabled = false)]
		public virtual string ExternalID { get; set; }
		#endregion

		#region ErrorMessage
		[Obsolete(PX.Objects.Common.Messages.ItemIsObsoleteAndWillBeRemoved2024R2, true)]
		public abstract class erorMessage : Data.BQL.BqlString.Field<erorMessage> { }

		public abstract class errorMessage : Data.BQL.BqlString.Field<errorMessage> { }
		/// <exclude/>
		[PXDBString(512, IsUnicode = true)]
		[PXUIField(DisplayName = "Error Message")]
		public virtual string ErrorMessage { get; set; }
		#endregion

		#region NoteID
		public abstract class noteID : Data.BQL.BqlGuid.Field<noteID> { }
		/// <exclude/>
		[PXNote]
		public virtual Guid? NoteID { get; set; }
		#endregion

		#region CreatedByID
		public abstract class createdByID : Data.BQL.BqlGuid.Field<createdByID> { }
		/// <exclude/>
		[PXDBCreatedByID]
		public virtual Guid? CreatedByID { get; set; }
		#endregion
		#region CreatedByScreenID
		public abstract class createdByScreenID : Data.BQL.BqlString.Field<createdByScreenID> { }
		/// <exclude/>
		[PXDBCreatedByScreenID]
		public virtual string CreatedByScreenID { get; set; }
		#endregion
		#region CreatedDateTime
		public abstract class createdDateTime : Data.BQL.BqlDateTime.Field<createdDateTime> { }
		/// <exclude/>
		[PXDBCreatedDateTime]
		public virtual DateTime? CreatedDateTime { get; set; }
		#endregion
		#region LastModifiedByID
		public abstract class lastModifiedByID : Data.BQL.BqlGuid.Field<lastModifiedByID> { }
		/// <exclude/>
		[PXDBLastModifiedByID]
		public virtual Guid? LastModifiedByID { get; set; }
		#endregion
		#region LastModifiedByScreenID
		public abstract class lastModifiedByScreenID : Data.BQL.BqlString.Field<lastModifiedByScreenID> { }
		/// <exclude/>
		[PXDBLastModifiedByScreenID]
		public virtual string LastModifiedByScreenID { get; set; }
		#endregion
		#region LastModifiedDateTime
		public abstract class lastModifiedDateTime : Data.BQL.BqlDateTime.Field<lastModifiedDateTime> { }
		/// <exclude/>
		[PXDBLastModifiedDateTime]
		public virtual DateTime? LastModifiedDateTime { get; set; }
		#endregion

		#region tstamp
		public abstract class Tstamp : Data.BQL.BqlByteArray.Field<Tstamp> { }
		/// <exclude/>
		[PXDBTimestamp(VerifyTimestamp = VerifyTimestampOptions.FromRecord)]
		public virtual byte[] tstamp { get; set; }
		#endregion
	}
}
