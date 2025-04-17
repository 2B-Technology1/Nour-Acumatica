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
using PX.Data.BQL;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.AP;
using PX.Objects.CM.Extensions;
using System;

namespace PX.Objects.CN.JointChecks
{
	[Serializable]
	[PXCacheName("Joint Payee")]
	public class JointPayee : IBqlTable
	{
		#region Keys
		public class PK : PrimaryKeyOf<JointPayee>.By<jointPayeeId>
		{
			public static JointPayee Find(PXGraph graph, int? jointPayeeId, PKFindOptions options = PKFindOptions.None) => FindBy(graph, jointPayeeId, options);
		}
		#endregion

		[PXDBIdentity(IsKey = true)]
		[PXReferentialIntegrityCheck]
		public virtual int? JointPayeeId
		{
			get;
			set;
		}
		public abstract class jointPayeeId : BqlInt.Field<jointPayeeId>
		{
		}

		#region IsMainPayee
		public abstract class isMainPayee : BqlBool.Field<isMainPayee> { }

		[PXDefault(false)]
		[PXDBBool]
		public virtual bool? IsMainPayee { get; set; }
		#endregion

		[Vendor(DisplayName = "Joint Payee (Vendor)")]
		[PXUIVerify(typeof(Where<Brackets<JointPayee.jointPayeeInternalId.IsNotNull
						.And<JointPayee.jointPayeeExternalName.IsNull>>
					.Or<JointPayee.jointPayeeInternalId.IsNull.And<JointPayee.jointPayeeExternalName.IsNotNull>>
					.And<JointPayee.jointPayeeExternalName.IsNotEqual<StringEmpty>>>),
				PXErrorLevel.Error, JointCheckMessages.OnlyOneVendorIsAllowed,
			CheckOnInserted=false, CheckOnRowSelected = false, CheckOnVerify = true)]
		public virtual int? JointPayeeInternalId
		{
			get;
			set;
		}
		public abstract class jointPayeeInternalId : BqlInt.Field<jointPayeeInternalId>
		{
		}

		#region JointPayeeExternalName
		[PXDBString(30)]
		[PXUIVerify(typeof(Where<Brackets<JointPayee.jointPayeeInternalId.IsNotNull
						.And<JointPayee.jointPayeeExternalName.IsNull>>
					.Or<JointPayee.jointPayeeInternalId.IsNull.And<JointPayee.jointPayeeExternalName.IsNotNull>>
					.And<JointPayee.jointPayeeExternalName.IsNotEqual<StringEmpty>>>),
				PXErrorLevel.Error, JointCheckMessages.OnlyOneVendorIsAllowed,
			CheckOnInserted = false, CheckOnRowSelected = false, CheckOnVerify = true)]
		[PXUIField(DisplayName = "Joint Payee")]
		public virtual string JointPayeeExternalName
		{
			get;
			set;
		}
		public abstract class jointPayeeExternalName : BqlString.Field<jointPayeeExternalName>
		{
		}
		#endregion

		#region CuryInfoID
		public abstract class curyInfoID : PX.Data.BQL.BqlLong.Field<curyInfoID>
		{
		}

		/// <summary>
		/// Identifier of the <see cref="PX.Objects.CM.CurrencyInfo">CurrencyInfo</see> object associated with the transaction.
		/// </summary>
		/// <value>
		/// Generated automatically. Corresponds to the <see cref="PX.Objects.CM.CurrencyInfo.CurrencyInfoID"/> field.
		/// </value>
		[PXDBLong()]
		[CurrencyInfo(typeof(APRegister.curyInfoID))]
		public virtual Int64? CuryInfoID
		{
			get;
			set;
		}
		#endregion

		#region CuryJointAmountOwed
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		[PXDBCurrency(typeof(curyInfoID), typeof(JointPayee.jointAmountOwed))]
		[PXUIField(DisplayName = "Joint Amount Owed")]
		public virtual decimal? CuryJointAmountOwed
		{
			get;
			set;
		}
		public abstract class curyJointAmountOwed : BqlDecimal.Field<curyJointAmountOwed>
		{
		}
		#endregion

		#region JointAmountOwed
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		[PXDBBaseCury]
		public virtual decimal? JointAmountOwed
		{
			get;
			set;
		}
		public abstract class jointAmountOwed : BqlDecimal.Field<jointAmountOwed>
		{
		}
		#endregion

		#region CuryJointAmountPaid
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		[PXDBCurrency(typeof(curyInfoID), typeof(JointPayee.jointAmountPaid))]
		[PXUIField(DisplayName = "Joint Amount Paid", IsReadOnly = true)]
		public virtual decimal? CuryJointAmountPaid
		{
			get;
			set;
		}
		public abstract class curyJointAmountPaid : BqlDecimal.Field<curyJointAmountPaid>
		{
		}
		#endregion

		#region JointAmountPaid
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		[PXDBBaseCury]
		public virtual decimal? JointAmountPaid
		{
			get;
			set;
		}
		public abstract class jointAmountPaid : BqlDecimal.Field<jointAmountPaid>
		{
		}
		#endregion

		#region CuryJointBalance
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		[PXDBCurrency(typeof(curyInfoID), typeof(JointPayee.jointBalance))]
		[PXFormula(typeof(Sub<curyJointAmountOwed, curyJointAmountPaid>))]
		[PXUIField(DisplayName = "Joint Balance", IsReadOnly = true)]
		public virtual decimal? CuryJointBalance
		{
			get;
			set;
		}
		public abstract class curyJointBalance : BqlDecimal.Field<curyJointBalance>
		{
		}
		#endregion

		#region JointBalance
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		[PXDBBaseCury]
		
		public virtual decimal? JointBalance
		{
			get;
			set;
		}
		public abstract class jointBalance : BqlDecimal.Field<jointBalance>
		{
		}
		#endregion

		#region APDocType
		public abstract class aPDocType : PX.Data.BQL.BqlString.Field<aPDocType> { }

		[PXDBString(3, IsFixed = true)]
		public virtual string APDocType
		{
			get;
			set;
		}
		#endregion
		#region APRefNbr
		public abstract class aPRefNbr : PX.Data.BQL.BqlString.Field<aPRefNbr> { }

		[PXDBString(15, IsUnicode = true, InputMask = ">CCCCCCCCCCCCCCC")]
		public virtual String APRefNbr
		{
			get;
			set;
		}
		#endregion
		#region APLineNbr
		
		[PXDBInt]
		[PXSelector(typeof(Search<APTran.lineNbr,
				Where<APTran.tranType, Equal<Current<APInvoice.docType>>,
					And<APTran.refNbr, Equal<Current<APInvoice.refNbr>>>>>),
			typeof(APTran.lineNbr),
			typeof(APTran.inventoryID),
			typeof(APTran.tranDesc),
			typeof(APTran.projectID),
			typeof(APTran.taskID),
			typeof(APTran.costCodeID),
			typeof(APTran.accountID),
			typeof(APTran.curyTranAmt), DirtyRead = true)]
		[PXUIField(DisplayName = "Bill Line Nbr.")]
		public virtual int? APLineNbr
		{
			get;
			set;
		}
		public abstract class aPLineNbr : BqlInt.Field<aPLineNbr>
		{
		}
		#endregion
		[PXFormula(typeof(Selector<aPLineNbr, APTran.curyLineAmt>))]
		[PXDecimal]
		[PXUIField(DisplayName = "Bill Line Amount", Visibility = PXUIVisibility.Invisible,
			Enabled = false)]
		public virtual decimal? BillLineAmount
		{
			get;
			set;
		}
		public abstract class billLineAmount : BqlDecimal.Field<billLineAmount>
		{
		}

		#region LinkedToPayment
		public abstract class linkedToPayment : BqlBool.Field<linkedToPayment> { }

		[PXDefault(false)]
		[PXDBBool]
		public virtual bool? LinkedToPayment { get; set; }
		#endregion

		

		#region CanDelete
		public abstract class canDelete : PX.Data.BQL.BqlBool.Field<canDelete> { }

		[PXBool]
		public virtual bool? CanDelete { get; set; }
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

	[PXHidden]
	[PXProjection(typeof(Select2<JointPayee,
		LeftJoin<Vendor, On<Vendor.bAccountID, Equal<JointPayee.jointPayeeInternalId>>>>), Persistent = false)]
	public class JointPayeeDisplay : IBqlTable
	{
		[PXDBInt(IsKey = true, BqlField = typeof(JointPayee.jointPayeeId))]
		public virtual int? JointPayeeId
		{
			get;
			set;
		}
		public abstract class jointPayeeId : BqlInt.Field<jointPayeeId>
		{
		}

		#region JointPayeeExternalName
		[PXDBString(30, BqlField = typeof(JointPayee.jointPayeeExternalName))]
		[PXUIField(DisplayName = "Joint Payee")]
		public virtual string JointPayeeExternalName
		{
			get;
			set;
		}
		public abstract class jointPayeeExternalName : BqlString.Field<jointPayeeExternalName>
		{
		}
		#endregion

		#region JointVendorName (Vendor.AcctName)
		public abstract class jointVendorName : PX.Data.BQL.BqlString.Field<jointVendorName> { }
		[PXDBString(255, IsUnicode = true, BqlField = typeof(Vendor.acctName))]
		[PXDefault()]
		public string JointVendorName
		{
			get;
			set;
		}
		#endregion

		#region Name
		[PXString(30)]
		[PXUIField(DisplayName = "Joint Payee Name")]
		public virtual string Name
		{
			[PXDependsOnFields(typeof(jointPayeeExternalName), typeof(jointVendorName))]
			get
			{	
				return string.IsNullOrEmpty(JointVendorName) ? JointPayeeExternalName : JointVendorName;
			}
		}
		public abstract class name : BqlString.Field<name>
		{
		}
		#endregion
	}

	[PXHidden]
	[PXBreakInheritance]
	[PXProjection(typeof(Select2<JointPayee,
		InnerJoin<APInvoice, On<APInvoice.docType, Equal<JointPayee.aPDocType>,
			And<APInvoice.refNbr, Equal<JointPayee.aPRefNbr>,
			And<APInvoice.paymentsByLinesAllowed, Equal<False>>>>,
		LeftJoin<Vendor, On<Vendor.bAccountID, Equal<JointPayee.jointPayeeInternalId>>,
		LeftJoin<APAdjust, On<APAdjust.adjdDocType, Equal<APInvoice.docType>,
			And<APAdjust.adjdRefNbr, Equal<APInvoice.refNbr>,
			And<APAdjust.released, Equal<False>,
			And<APAdjust.jointPayeeID, Equal<JointPayee.jointPayeeId>>>>>>>>,
		Where<APAdjust.adjdRefNbr, IsNull>>))]
	public class JointPayeePerDoc : JointPayee
	{
		public new abstract class jointPayeeId : BqlInt.Field<jointPayeeId> { }
		public new abstract class aPDocType : PX.Data.BQL.BqlString.Field<aPDocType> { }
		public new abstract class aPRefNbr : PX.Data.BQL.BqlString.Field<aPRefNbr> { }

		public new abstract class isMainPayee : BqlBool.Field<isMainPayee> { }

		public new abstract class jointPayeeExternalName : BqlString.Field<jointPayeeExternalName>{}

		#region JointVendorName (Vendor.AcctName)
		public abstract class jointVendorName : PX.Data.BQL.BqlString.Field<jointVendorName> { }
		[PXDBString(255, IsUnicode = true, BqlField = typeof(Vendor.acctName))]
		[PXDefault()]
		[PXUIField(DisplayName = "Joint Payee Name")]
		public string JointVendorName
		{
			get;
			set;
		}
		#endregion
	}

	[PXHidden]
	[PXBreakInheritance]
	[PXProjection(typeof(Select2<JointPayee,
		InnerJoin<APInvoice, On<APInvoice.docType, Equal<JointPayee.aPDocType>,
			And<APInvoice.refNbr, Equal<JointPayee.aPRefNbr>,
			And<APInvoice.paymentsByLinesAllowed, Equal<True>>>>,
		InnerJoin<APTran, On<APInvoice.docType, Equal<APTran.tranType>,
			And<APInvoice.refNbr, Equal<APTran.refNbr>,
			And<JointPayee.aPLineNbr, Equal<APTran.lineNbr>>>>,
		LeftJoin<Vendor, On<Vendor.bAccountID, Equal<JointPayee.jointPayeeInternalId>>,
		LeftJoin<APAdjust, On<APAdjust.adjdDocType, Equal<APInvoice.docType>,
			And<APAdjust.adjdRefNbr, Equal<APInvoice.refNbr>,
			And<APAdjust.released, Equal<False>,
			And<APAdjust.jointPayeeID, Equal<JointPayee.jointPayeeId>>>>>>>>>,
		Where<APAdjust.adjdRefNbr, IsNull>>))]
	public class JointPayeePerLine : JointPayee
	{
		public new abstract class jointPayeeId : BqlInt.Field<jointPayeeId> { }
		public new abstract class aPDocType : PX.Data.BQL.BqlString.Field<aPDocType> { }
		public new abstract class aPRefNbr : PX.Data.BQL.BqlString.Field<aPRefNbr> { }
		public new abstract class aPLineNbr : BqlInt.Field<aPLineNbr>{}

		public new abstract class isMainPayee : BqlBool.Field<isMainPayee> { }

		public new abstract class jointPayeeExternalName : BqlString.Field<jointPayeeExternalName> { }

		#region JointVendorName (Vendor.AcctName)
		public abstract class jointVendorName : PX.Data.BQL.BqlString.Field<jointVendorName> { }
		[PXDBString(255, IsUnicode = true, BqlField = typeof(Vendor.acctName))]
		[PXDefault()]
		[PXUIField(DisplayName = "Joint Payee Name")]
		public string JointVendorName
		{
			get;
			set;
		}
		#endregion
	}
}
