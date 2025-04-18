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

namespace PX.Objects.AP
{
	using System;
	using PX.Data;
	using PX.Objects.CS;
    using PX.Objects.CR;
	using PX.Objects.CA;
	using PX.Data.ReferentialIntegrity.Attributes;

	/// <summary>
	/// Vendor-specific values for AP-related payment method settings
	/// (which are stored in <see cref="PaymentMethodDetail"/>).
	/// They are edited on the Payment tab of the Vendor Locations (AP303010) form.
	/// For the main vendor location, they can also be edited on the Payment tab of the Vendors (AP303000) form.
	/// </summary>
	[System.SerializableAttribute()]
	[PXCacheName(Messages.VendorPaymentTypeDetail)]
	public partial class VendorPaymentMethodDetail : PX.Data.IBqlTable
	{
		#region Keys
		public class PK : PrimaryKeyOf<VendorPaymentMethodDetail>.By<bAccountID, locationID, paymentMethodID, detailID>
		{
			public static VendorPaymentMethodDetail Find(PXGraph graph, Int32? bAccountID, Int32? locationID, string paymentMethodID, String detailID, PKFindOptions options = PKFindOptions.None) => FindBy(graph, bAccountID, locationID, paymentMethodID, detailID, options);
		}
		public static class FK
		{
			public class BAccount : CR.BAccount.PK.ForeignKeyOf<VendorPaymentMethodDetail>.By<bAccountID> { }
			public class Location : CR.Location.PK.ForeignKeyOf<VendorPaymentMethodDetail>.By<bAccountID, locationID> { }
			public class PaymentMethod : CA.PaymentMethod.PK.ForeignKeyOf<VendorPaymentMethodDetail>.By<paymentMethodID> { }
		}
		#endregion

		#region BAccountID
		public abstract class bAccountID : PX.Data.BQL.BqlInt.Field<bAccountID> { }
		protected Int32? _BAccountID;

		[PXDBInt(IsKey = true)]
		[PXDBDefault(typeof(Location.bAccountID))]
		[PXUIField(DisplayName = "BAccountID", Visible = false, Enabled = false)]
		[PXParent(typeof(Select<BAccount, Where<BAccount.bAccountID, Equal<Current<VendorPaymentMethodDetail.bAccountID>>>>))]
		public virtual Int32? BAccountID
		{
			get
			{
				return this._BAccountID;
			}
			set
			{
				this._BAccountID = value;
			}
		}
		#endregion
		#region LocationID
		public abstract class locationID : PX.Data.BQL.BqlInt.Field<locationID> { }
		protected Int32? _LocationID;
		[PXDBInt(IsKey = true)]
		[PXDBDefault(typeof(Location.locationID))]
		[PXUIField(Visible = false, Enabled = false, Visibility = PXUIVisibility.Invisible)]
		[PXParent(typeof(Select<Location, Where<Location.bAccountID, Equal<Current<VendorPaymentMethodDetail.bAccountID>>, And<Location.locationID, Equal<Current<VendorPaymentMethodDetail.locationID>>>>>))]
		public virtual Int32? LocationID
		{
			get
			{
				return this._LocationID;
			}
			set
			{
				this._LocationID = value;
			}
		}
		#endregion
		#region PaymentMethodID
		public abstract class paymentMethodID : PX.Data.BQL.BqlString.Field<paymentMethodID> { }
		protected String _PaymentMethodID;
		[PXDBString(10, IsUnicode = true, IsKey = true)]
		[PXDefault(typeof(Search<Location.vPaymentMethodID,Where<Location.bAccountID, Equal<Current<VendorPaymentMethodDetail.bAccountID>>, And<Location.locationID, Equal<Current<VendorPaymentMethodDetail.locationID>>>>>))]
		[PXUIField(DisplayName = "Payment Method", Visible=false)]
		[PXSelector(typeof(PaymentMethod.paymentMethodID), DescriptionField = typeof(CA.PaymentMethod.descr))]
		[PXParent(typeof(Select<PaymentMethod, Where<PaymentMethod.paymentMethodID, Equal<Current<VendorPaymentMethodDetail.paymentMethodID>>>>))]
		public virtual String PaymentMethodID
		{
			get
			{
				return this._PaymentMethodID;
			}
			set
			{
				this._PaymentMethodID = value;
			}
		}
				#endregion
        #region DetailID
        public abstract class detailID : PX.Data.BQL.BqlString.Field<detailID> { }
        protected String _DetailID;
        [PXDBString(10, IsUnicode = true, IsKey = true)]
        [PXDefault()]
        [PXSelector(typeof(Search<PaymentMethodDetail.detailID, Where<PaymentMethodDetail.paymentMethodID,
                Equal<Current<VendorPaymentMethodDetail.paymentMethodID>>>>))]
        [PXUIField(DisplayName = "ID", Visible = true, Enabled = true)]
		[PXParent(typeof(Select<PaymentMethodDetail, Where<PaymentMethodDetail.paymentMethodID, Equal<Current<VendorPaymentMethodDetail.paymentMethodID>>, 
														And<PaymentMethodDetail.useFor, Equal<PaymentMethodDetailUsage.useForVendor>, 
															And<PaymentMethodDetail.detailID, Equal<Current<VendorPaymentMethodDetail.detailID>>>>>>))]
		public virtual String DetailID
        {
            get
            {
                return this._DetailID;
            }
            set
            {
                this._DetailID = value;
            }
        }
        #endregion		
		#region DetailValue
		public abstract class detailValue : PX.Data.BQL.BqlString.Field<detailValue> { }
		protected String _DetailValue;
		[PXDBStringWithMask(255,typeof(Search<PaymentMethodDetail.entryMask, Where<PaymentMethodDetail.paymentMethodID, Equal<Current<VendorPaymentMethodDetail.paymentMethodID>>,
									   And<PaymentMethodDetail.detailID, Equal<Current<VendorPaymentMethodDetail.detailID>>,
                                       And<Where<PaymentMethodDetail.useFor, Equal<PaymentMethodDetailUsage.useForVendor>,
                                            Or<PaymentMethodDetail.useFor, Equal<PaymentMethodDetailUsage.useForAll>>>>>>>), IsUnicode = true)]
		[DynamicValueValidation(typeof(Search<PaymentMethodDetail.validRegexp, Where<PaymentMethodDetail.paymentMethodID, Equal<Current<VendorPaymentMethodDetail.paymentMethodID>>,
									   And<PaymentMethodDetail.detailID, Equal<Current<VendorPaymentMethodDetail.detailID>>,
                                       And<Where<PaymentMethodDetail.useFor, Equal<PaymentMethodDetailUsage.useForVendor>,
                                            Or<PaymentMethodDetail.useFor, Equal<PaymentMethodDetailUsage.useForAll>>>>>>>))]
		[PXDefault(PersistingCheck=PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Value")]
		public virtual String DetailValue
		{
			get
			{
				return this._DetailValue;
			}
			set
			{
				this._DetailValue = value;
			}
		}
		#endregion
		#region CreatedByID
		public abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID> { }
		protected Guid? _CreatedByID;
		[PXDBCreatedByID()]
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
		public abstract class createdByScreenID : PX.Data.BQL.BqlString.Field<createdByScreenID> { }
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
		public abstract class createdDateTime : PX.Data.BQL.BqlDateTime.Field<createdDateTime> { }
		protected DateTime? _CreatedDateTime;
		[PXDBCreatedDateTime()]
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
		public abstract class lastModifiedByID : PX.Data.BQL.BqlGuid.Field<lastModifiedByID> { }
		protected Guid? _LastModifiedByID;
		[PXDBLastModifiedByID()]
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
		public abstract class lastModifiedByScreenID : PX.Data.BQL.BqlString.Field<lastModifiedByScreenID> { }
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
		public abstract class lastModifiedDateTime : PX.Data.BQL.BqlDateTime.Field<lastModifiedDateTime> { }
		protected DateTime? _LastModifiedDateTime;
		[PXDBLastModifiedDateTime()]
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
		#region tstamp
		public abstract class Tstamp : PX.Data.BQL.BqlByteArray.Field<Tstamp> { }
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
	}
}
