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
using PX.Objects.AR.CCPaymentProcessing.Interfaces;
namespace PX.Objects.CA
{
	/// <summary>
	/// The additional setting that are required to use the payment method.
	/// </summary>
	[Serializable]
	[PXCacheName(Messages.PaymentMethodDetail)]
	public partial class PaymentMethodDetail : IBqlTable,ICCPaymentMethodDetail
	{
		#region Keys
		public class PK : PrimaryKeyOf<PaymentMethodDetail>.By<paymentMethodID, detailID>
		{
			public static PaymentMethodDetail Find(PXGraph graph, string paymentMethodID, String detailID, PKFindOptions options = PKFindOptions.None) => FindBy(graph, paymentMethodID, detailID, options);
		}
		public static class FK
		{
			public class PaymentMethod : CA.PaymentMethod.PK.ForeignKeyOf<PaymentMethodDetail>.By<paymentMethodID> { }
		}
		#endregion
		#region PaymentMethodID
		public abstract class paymentMethodID : PX.Data.BQL.BqlString.Field<paymentMethodID> { }
		[PXDBString(10, IsUnicode = true, IsKey = true)]
		[PXDefault(typeof(PaymentMethod.paymentMethodID))]
		[PXUIField(DisplayName = "Payment Method", Visible = false)]
		[PXParent(typeof(Select<PaymentMethod, Where<PaymentMethod.paymentMethodID, Equal<Current<PaymentMethodDetail.paymentMethodID>>>>))]
		public virtual string PaymentMethodID
			{
			get;
			set;
		}
		#endregion
		#region UseFor
		public abstract class useFor : PX.Data.BQL.BqlString.Field<useFor> { }

		/// <summary>
		/// The field identifies the type of records it belongs to.
		/// The list of the possible values can be found in <see cref="PaymentMethodDetailUsage"/> class.
		/// </summary>
		[PXDBString(1, IsFixed = true, IsKey = true)]
		[PXDefault(PaymentMethodDetailUsage.UseForAll)]
		[PXUIField(DisplayName = "Used In")]
		public virtual string UseFor
		{
			get;
			set;
		}
		#endregion		

		#region DetailID
		public abstract class detailID : PX.Data.BQL.BqlString.Field<detailID> { }

		[PXDBString(10, IsUnicode = true, IsKey = true)]
		[PXDefault]
		[PXUIField(DisplayName = "ID", Visible = true)]
		public virtual string DetailID
			{
			get;
			set;
		}
		#endregion

		#region Descr
		public abstract class descr : PX.Data.BQL.BqlString.Field<descr> { }

		[PXDBLocalizableString(60, IsUnicode = true)]
		[PXUIField(DisplayName = "Description")]
		public virtual string Descr
			{
			get;
			set;
		}
		#endregion
		#region EntryMask
		public abstract class entryMask : PX.Data.BQL.BqlString.Field<entryMask> { }
		[PXDBString(255)]
		[PXUIField(DisplayName = "Entry Mask")]
		public virtual string EntryMask
		{
			get;
			set;
		}
		#endregion
		#region ValidRegexp
		public abstract class validRegexp : PX.Data.BQL.BqlString.Field<validRegexp> { }
		[PXDBString(255)]
		[PXUIField(DisplayName = "Validation Reg. Exp.")]
		public virtual string ValidRegexp
		{
			get;
			set;
		}
		#endregion
		#region DisplayMask
		public abstract class displayMask : PX.Data.BQL.BqlString.Field<displayMask> { }
		[PXDBString(255)]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Display Mask", Enabled = false)]
		public virtual string DisplayMask
		{
			get;
			set;
		}
		#endregion
		#region IsEncrypted
		public abstract class isEncrypted : PX.Data.BQL.BqlBool.Field<isEncrypted> { }
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Encrypted")]
		public virtual bool? IsEncrypted
			{
			get;
			set;
		}
		#endregion
		#region IsRequired
		public abstract class isRequired : PX.Data.BQL.BqlBool.Field<isRequired> { }
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Required")]
		public virtual bool? IsRequired
			{
			get;
			set;
		}
		#endregion
		#region IsIdentifier
		public abstract class isIdentifier : PX.Data.BQL.BqlBool.Field<isIdentifier> { }
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Card/Account Nbr.")]
		[Common.UniqueBool(typeof(PaymentMethodDetail.paymentMethodID))]
		public virtual bool? IsIdentifier
			{
			get;
			set;
		}
			#endregion
		#region IsExpirationDate
		public abstract class isExpirationDate : PX.Data.BQL.BqlBool.Field<isExpirationDate> { }
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Exp. Date")]
		[Common.UniqueBool(typeof(PaymentMethodDetail.paymentMethodID))]
		public virtual bool? IsExpirationDate
			{
			get;
			set;
		}
		#endregion
		#region IsOwnerName
		public abstract class isOwnerName : PX.Data.BQL.BqlBool.Field<isOwnerName> { }
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Name on Card")]
		[Common.UniqueBool(typeof(PaymentMethodDetail.paymentMethodID))]
		public virtual bool? IsOwnerName
			{
			get;
			set;
		}
		#endregion
		#region IsCCProcessingID
		public abstract class isCCProcessingID : PX.Data.BQL.BqlBool.Field<isCCProcessingID> { }
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Payment Profile ID")]
		[Common.UniqueBool(typeof(PaymentMethodDetail.paymentMethodID))]
		public virtual bool? IsCCProcessingID
		{
			get;
			set;
		}
		#endregion
		#region IsCVV
		public abstract class isCVV : PX.Data.BQL.BqlBool.Field<isCVV> { }
		protected Boolean? _IsCVV;
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "CVV Code")]
		[Common.UniqueBool(typeof(PaymentMethodDetail.paymentMethodID))]
		public virtual Boolean? IsCVV
		{
			get
			{
				return this._IsCVV;
			}
			set
			{
				this._IsCVV = value;
			}
		}
		#endregion
		#region ControlType
		public abstract class controlType : PX.Data.BQL.BqlInt.Field<controlType> {	}

		[PXDBInt]
		[PXDefault((int)PaymentMethodDetailType.Text)]
		[PXUIField(DisplayName = "Control Type")]
		[PXIntList(new int[] { (int)PaymentMethodDetailType.Text, (int)PaymentMethodDetailType.AccountType }, new string[] { "Text", "Account Type List" })]
		public virtual int? ControlType
		{
			get;
			set;
		}
		#endregion
		#region DefaultValue
		public abstract class defaultValue : PX.Data.BQL.BqlString.Field<defaultValue> { }

		[PXDBString(255, IsUnicode = true)]
		[PXIntList(new int[] { (int)ACHPlugInBase.TransactionCode.CheckingAccount, (int)ACHPlugInBase.TransactionCode.SavingAccount }, new string[] { Messages.CheckingAccount, Messages.SavingAccount })]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Default Value")]
		public virtual string DefaultValue { get; set; }
		#endregion
		#region OrderIndex
		public abstract class orderIndex : PX.Data.BQL.BqlShort.Field<orderIndex> { }
		[PXDBShort]
		[PXUIField(DisplayName = "Sort Order")]
		public virtual short? OrderIndex
			{
			get;
			set;
		}
		#endregion
		#region tstamp
		public abstract class Tstamp : PX.Data.BQL.BqlByteArray.Field<Tstamp> { }
		[PXDBTimestamp]
		public virtual byte[] tstamp
		{
			get;
			set;
		}
		#endregion
		#region CreatedByID
		public abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID> { }
		[PXDBCreatedByID]
		public virtual Guid? CreatedByID
		{
			get;
			set;
		}
		#endregion
		#region CreatedByScreenID
		public abstract class createdByScreenID : PX.Data.BQL.BqlString.Field<createdByScreenID> { }
		[PXDBCreatedByScreenID]
		public virtual string CreatedByScreenID
		{
			get;
			set;
		}
		#endregion
		#region CreatedDateTime
		public abstract class createdDateTime : PX.Data.BQL.BqlDateTime.Field<createdDateTime> { }
		[PXDBCreatedDateTime]
		public virtual DateTime? CreatedDateTime
		{
			get;
			set;
		}
		#endregion
		#region LastModifiedByID
		public abstract class lastModifiedByID : PX.Data.BQL.BqlGuid.Field<lastModifiedByID> { }
		[PXDBLastModifiedByID]
		public virtual Guid? LastModifiedByID
		{
			get;
			set;
		}
		#endregion
		#region LastModifiedByScreenID
		public abstract class lastModifiedByScreenID : PX.Data.BQL.BqlString.Field<lastModifiedByScreenID> { }
		[PXDBLastModifiedByScreenID]
		public virtual string LastModifiedByScreenID
			{
			get;
			set;
		}
		#endregion
		#region LastModifiedDateTime
		public abstract class lastModifiedDateTime : PX.Data.BQL.BqlDateTime.Field<lastModifiedDateTime> { }
		[PXDBLastModifiedDateTime]
		public virtual DateTime? LastModifiedDateTime
		{
			get;
			set;
		}
		#endregion

		#region NoteID
		public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }
		[PXNote]
		public virtual Guid? NoteID
		{
			get;
			set;
		}
		#endregion
	}

	public class PaymentMethodDetailUsage
	{

		public const string UseForVendor = "V";
		public const string UseForCashAccount = "C";
		public const string UseForAll = "A";
		public const string UseForARCards = "R";
		public const string UseForAPCards = "P";


		public class useForVendor : PX.Data.BQL.BqlString.Constant<useForVendor>
		{
			public useForVendor() : base(UseForVendor) { }
		}

		public class useForCashAccount : PX.Data.BQL.BqlString.Constant<useForCashAccount>
		{
			public useForCashAccount() : base(UseForCashAccount) { }
		}

		public class useForAll : PX.Data.BQL.BqlString.Constant<useForAll>
		{
			public useForAll() : base(UseForAll) { }
		}

		public class useForARCards : PX.Data.BQL.BqlString.Constant<useForARCards>
		{
			public useForARCards() : base(UseForARCards) { }
		}

		public class useForAPCards : PX.Data.BQL.BqlString.Constant<useForAPCards>
		{
			public useForAPCards() : base(UseForAPCards) { }
		}

	}
}
