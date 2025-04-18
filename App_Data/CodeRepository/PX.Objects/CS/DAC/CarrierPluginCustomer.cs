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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PX.Data;
using PX.Objects.AR;
using PX.Objects.CR;

namespace PX.Objects.CS
{
	[System.SerializableAttribute()]
	[PXCacheName(Messages.CarrierPluginCustomer)]
	public partial class CarrierPluginCustomer : PX.Data.IBqlTable
	{
		#region CarrierPluginID
		public abstract class carrierPluginID : PX.Data.BQL.BqlString.Field<carrierPluginID> { }
		protected String _CarrierPluginID;
		[PXUIField(DisplayName="Carrier")]
		[PXSelector(typeof(CarrierPlugin.carrierPluginID))]
		[PXParent(typeof(Select<CarrierPlugin, Where<CarrierPlugin.carrierPluginID, Equal<Current<CarrierPluginCustomer.carrierPluginID>>>>))]
		[PXDBString(15, IsUnicode = true, IsKey = true)]
		[PXDefault(typeof(CarrierPlugin.carrierPluginID))]
		public virtual String CarrierPluginID
		{
			get
			{
				return this._CarrierPluginID;
			}
			set
			{
				this._CarrierPluginID = value;
			}
		}
		#endregion
		#region RecordID
		public abstract class recordID : PX.Data.BQL.BqlInt.Field<recordID> { }
		protected Int32? _RecordID;
		[PXDBIdentity(IsKey = true)]
		public virtual Int32? RecordID
		{
			get
			{
				return this._RecordID;
			}
			set
			{
				this._RecordID = value;
			}
		}
		#endregion
		
		#region CustomerID
		public abstract class customerID : PX.Data.BQL.BqlInt.Field<customerID> { }
		protected Int32? _CustomerID;
		[Customer(DescriptionField = typeof(Customer.acctName), Filterable = true)]
		[PXUIField(DisplayName = "Customer ID")]
		[PXDefault()]
		public virtual Int32? CustomerID
		{
			get
			{
				return this._CustomerID;
			}
			set
			{
				this._CustomerID = value;
			}
		}
		#endregion
		#region CustomerLocationID
		public abstract class customerLocationID : PX.Data.BQL.BqlInt.Field<customerLocationID> { }
		protected Int32? _CustomerLocationID;
		[PXCheckUnique(typeof(CarrierPluginCustomer.carrierPluginID), typeof(CarrierPluginCustomer.customerID), IgnoreNulls=false)]
		[LocationID(typeof(Where<Location.bAccountID, Equal<Current<CarrierPluginCustomer.customerID>>>), DisplayName = "Location", DescriptionField = typeof(Location.descr))]
		public virtual Int32? CustomerLocationID
		{
			get
			{
				return this._CustomerLocationID;
			}
			set
			{
				this._CustomerLocationID = value;
			}
		}
		#endregion
		#region IsActive
		public abstract class isActive : PX.Data.BQL.BqlBool.Field<isActive> { }
		protected Boolean? _IsActive;
		[PXDBBool()]
		[PXDefault(true)]
		[PXUIField(DisplayName = "Active")]
		public virtual Boolean? IsActive
		{
			get
			{
				return this._IsActive;
			}
			set
			{
				this._IsActive = value;
			}
		}
		#endregion
		#region CarrierAccount
		public abstract class carrierAccount : PX.Data.BQL.BqlString.Field<carrierAccount> { }
		protected String _CarrierAccount;
		[PXDefault]
		[PXDBString()]
		[PXUIField(DisplayName = "Carrier Account")]
		public virtual String CarrierAccount
		{
			get
			{
				return this._CarrierAccount;
			}
			set
			{
				this._CarrierAccount = value;
			}
		}
		#endregion
		#region PostalCode
		public abstract class postalCode : PX.Data.BQL.BqlString.Field<postalCode> { }
		protected String _PostalCode;
		[PXDefault(typeof(Search2<Address.postalCode, InnerJoin<Location, On<Address.addressID, Equal<Location.defAddressID>, And<Location.bAccountID, Equal<Current<CarrierPluginCustomer.customerID>>, And<Location.locationID, Equal<Current<CarrierPluginCustomer.customerLocationID>>>>>>>), PersistingCheck=PXPersistingCheck.Nothing)]
		[PXDBString()]
		[PXUIField(DisplayName = "Postal Code")]
		public virtual String PostalCode
		{
			get
			{
				return this._PostalCode;
			}
			set
			{
				this._PostalCode = value;
			}
		}
		#endregion
		#region CarrierBillingType
		[PXDBString(1)]
		[CarrierBillingTypes.List]
		[PXDefault(CarrierBillingTypes.Receiver, PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Carrier Billing Type")]
		public virtual string CarrierBillingType { get; set; }
		public abstract class carrierBillingType : PX.Data.BQL.BqlString.Field<carrierBillingType> { }
		#endregion

		#region CountryID
		/// <summary>
		/// The unique two-letter identifier of the Country.
		/// </summary>
		/// <value>
		/// The identifiers of the countries are defined by the ISO 3166 standard.
		/// </value>
		[PXDBString(2, IsUnicode = true, InputMask = "")]
		[PXUIField(DisplayName = "Country")]
		[PXSelector(typeof(PX.Objects.CS.Country.countryID), CacheGlobal = true, DescriptionField = typeof(PX.Objects.CS.Country.description))]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual string CountryID { get; set; }
		public abstract class countryID : PX.Data.BQL.BqlString.Field<countryID> { }
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
