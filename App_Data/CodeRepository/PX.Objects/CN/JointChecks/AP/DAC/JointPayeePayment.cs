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
using PX.Common.Serialization;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.AP;
using PX.Objects.CM;

namespace PX.Objects.CN.JointChecks
{
    [PXSerializable]
    [PXCacheName("Joint Payee Payment")]
    public class JointPayeePayment : IBqlTable
    {
		#region Keys
		public static class FK
	    {
		    public class JointPayee : PX.Objects.CN.JointChecks.JointPayee.PK.ForeignKeyOf<JointPayeePayment>.By<jointPayeeId> { }
	    }
		#endregion

		#region JointPayeePaymentId
		[PXDBIdentity(IsKey = true)]
		public virtual int? JointPayeePaymentId
		{
			get;
			set;
		}
		public abstract class jointPayeePaymentId : BqlInt.Field<jointPayeePaymentId>
		{
		}
		#endregion

		#region JointPayeeId
		[PXDBInt]
		[PXForeignReference(typeof(FK.JointPayee))]
		public virtual int? JointPayeeId
		{
			get;
			set;
		}
		public abstract class jointPayeeId : BqlInt.Field<jointPayeeId>
		{
		}
		#endregion

		#region BillLineNumber
		[PXInt]
		[PXUnboundDefault(typeof(SelectFrom<JointPayee>
				.Where<JointPayee.jointPayeeId.IsEqual<jointPayeeId.FromCurrent>>),
			SourceField = typeof(JointPayee.aPLineNbr))]
		[PXUIField(DisplayName = "Bill Line Nbr.",
			Visibility = PXUIVisibility.Invisible, Enabled = false)]
		public virtual int? BillLineNumber
		{
			get;
			set;
		}
		public abstract class billLineNumber : BqlInt.Field<billLineNumber>
		{
		}
		#endregion

		#region PaymentRefNbr
		[PXParent(typeof(Select<APPayment, Where<APPayment.docType, Equal<Current<paymentDocType>>,
			And<APPayment.refNbr, Equal<Current<paymentRefNbr>>>>>))]
		[PXDBString]
		public virtual string PaymentRefNbr
		{
			get;
			set;
		}
		public abstract class paymentRefNbr : BqlString.Field<paymentRefNbr>
		{
		}
		#endregion

		#region PaymentDocType
		[PXDBString]
		public virtual string PaymentDocType
		{
			get;
			set;
		}
		public abstract class paymentDocType : BqlString.Field<paymentDocType>
		{
		}
		#endregion

		#region InvoiceRefNbr
		[PXDBString]
		[PXSelector(typeof(Search<APInvoice.refNbr, Where<APInvoice.docType, Equal<APDocType.invoice>>>),
			SubstituteKey = typeof(APInvoice.refNbr))]
		[PXUIField(DisplayName = "AP Bill Nbr.", Enabled = false)]
		public virtual string InvoiceRefNbr
		{
			get;
			set;
		}
		public abstract class invoiceRefNbr : BqlString.Field<invoiceRefNbr>
		{
		}
		#endregion

		#region InvoiceDocType
		[PXDBString]
		public virtual string InvoiceDocType
		{
			get;
			set;
		}
		public abstract class invoiceDocType : BqlString.Field<invoiceDocType>
		{
		}
		#endregion

		#region AdjustmentNumber
		[PXDBInt]
		public virtual int? AdjustmentNumber
		{
			get;
			set;
		}
		public abstract class adjustmentNumber : BqlInt.Field<adjustmentNumber>
		{
		}
		#endregion

		#region CuryJointAmountToPay
		[PXDBCurrency(typeof(APRegister.curyInfoID), typeof(jointAmountToPay))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Joint Amount To Pay")]
		public virtual decimal? CuryJointAmountToPay
		{
			get;
			set;
		}
		public abstract class curyJointAmountToPay : BqlDecimal.Field<curyJointAmountToPay>
		{
		}
		#endregion

		#region JointAmountToPay
		[PXDBBaseCury]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? JointAmountToPay
		{
			get;
			set;
		}
		public abstract class jointAmountToPay : BqlDecimal.Field<jointAmountToPay>
		{
		}
		#endregion

		#region IsVoided
		public abstract class isVoided : PX.Data.BQL.BqlBool.Field<isVoided>
		{
		}
		
		[PXDBBool()]
		[PXDefault(false)]
		public virtual Boolean? IsVoided
		{
			get;
			set;
		}
		#endregion

		#region System Columns
		#region NoteID
		public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID>
		{
		}
		protected Guid? _NoteID;
		[PXNote]
		public virtual Guid? NoteID
		{
			get
			{
				return this._NoteID;
			}
			set
			{
				this._NoteID = value;
			}
		}
		#endregion
		#region tstamp
		public abstract class Tstamp : PX.Data.BQL.BqlByteArray.Field<Tstamp>
		{
		}
		protected Byte[] _tstamp;
		[PXDBTimestamp()]
		public virtual Byte[] tstamp
		{
			get
			{
				return this._tstamp;
			}
			set
			{
				this._tstamp = value;
			}
		}
		#endregion
		#region CreatedByID
		public abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID>
		{
		}
		protected Guid? _CreatedByID;
		[PXDBCreatedByID]
		public virtual Guid? CreatedByID
		{
			get
			{
				return this._CreatedByID;
			}
			set
			{
				this._CreatedByID = value;
			}
		}
		#endregion
		#region CreatedByScreenID
		public abstract class createdByScreenID : PX.Data.BQL.BqlString.Field<createdByScreenID>
		{
		}
		protected String _CreatedByScreenID;
		[PXDBCreatedByScreenID()]
		public virtual String CreatedByScreenID
		{
			get
			{
				return this._CreatedByScreenID;
			}
			set
			{
				this._CreatedByScreenID = value;
			}
		}
		#endregion
		#region CreatedDateTime
		public abstract class createdDateTime : PX.Data.BQL.BqlDateTime.Field<createdDateTime>
		{
		}
		protected DateTime? _CreatedDateTime;
		[PXUIField(DisplayName = PXDBLastModifiedByIDAttribute.DisplayFieldNames.CreatedDateTime, Enabled = false, IsReadOnly = true)]
		[PXDBCreatedDateTime]
		public virtual DateTime? CreatedDateTime
		{
			get
			{
				return this._CreatedDateTime;
			}
			set
			{
				this._CreatedDateTime = value;
			}
		}
		#endregion
		#region LastModifiedByID
		public abstract class lastModifiedByID : PX.Data.BQL.BqlGuid.Field<lastModifiedByID>
		{
		}
		protected Guid? _LastModifiedByID;
		[PXDBLastModifiedByID]
		public virtual Guid? LastModifiedByID
		{
			get
			{
				return this._LastModifiedByID;
			}
			set
			{
				this._LastModifiedByID = value;
			}
		}
		#endregion
		#region LastModifiedByScreenID
		public abstract class lastModifiedByScreenID : PX.Data.BQL.BqlString.Field<lastModifiedByScreenID>
		{
		}
		protected String _LastModifiedByScreenID;
		[PXDBLastModifiedByScreenID()]
		public virtual String LastModifiedByScreenID
		{
			get
			{
				return this._LastModifiedByScreenID;
			}
			set
			{
				this._LastModifiedByScreenID = value;
			}
		}
		#endregion
		#region LastModifiedDateTime
		public abstract class lastModifiedDateTime : PX.Data.BQL.BqlDateTime.Field<lastModifiedDateTime>
		{
		}
		protected DateTime? _LastModifiedDateTime;
		[PXUIField(DisplayName = PXDBLastModifiedByIDAttribute.DisplayFieldNames.LastModifiedDateTime, Enabled = false, IsReadOnly = true)]
		[PXDBLastModifiedDateTime]
		public virtual DateTime? LastModifiedDateTime
		{
			get
			{
				return this._LastModifiedDateTime;
			}
			set
			{
				this._LastModifiedDateTime = value;
			}
		}
		#endregion
		#endregion
	}
}
