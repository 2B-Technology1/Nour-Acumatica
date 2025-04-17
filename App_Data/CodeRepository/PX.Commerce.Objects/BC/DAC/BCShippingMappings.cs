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

using PX.Commerce.Core;
using PX.Data;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects;
using PX.Objects.CS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PX.Commerce.Objects
{
	/// <summary>
	/// Represents a mapping between a shipping method in an external store and in ERP.
	/// </summary>
	[Serializable]
	[PXCacheName("BCShippingMappings")]
	public class BCShippingMappings : IBqlTable
	{
		#region Keys

		public static class FK
		{
			public class Binding : BCBinding.BindingIndex.ForeignKeyOf<BCPaymentMethods>.By<bindingID> { }
			public class ShipZone : BCBinding.BindingIndex.ForeignKeyOf<ShippingZone>.By<zoneID> { }
		}
		#endregion
		#region ShippingMappingID
		/// <summary>
		/// The identity of this record.
		/// </summary>
		[PXDBIdentity(IsKey = true)]
		public int? ShippingMappingID { get; set; }
		/// <inheritdoc cref="ShippingMappingID"/>
		public abstract class shippingMappingID : PX.Data.BQL.BqlInt.Field<shippingMappingID> { }
		#endregion

		#region BindingID
		/// <summary>
		/// Represents a store to which the entity belongs.
		/// The property is a key field.
		/// </summary>
		[PXDBInt()]
		[PXUIField(DisplayName = "Store")]
		[PXParent(typeof(Select<BCBinding,
			Where<BCBinding.bindingID, Equal<Current<BCShippingMappings.bindingID>>>>))]
		[PXDBDefault(typeof(BCBinding.bindingID))]

		public virtual int? BindingID { get; set; }
		/// <inheritdoc cref="BindingID"/>
		public abstract class bindingID : PX.Data.BQL.BqlInt.Field<bindingID> { }
		#endregion

		#region BC Shipping zone
		/// <summary>
		/// The shipping zone in the external store.
		/// </summary>
		[PXDBString(200, IsUnicode = true, InputMask = "")]
		[PXUIField(DisplayName = "Store Shipping Zone")]
		public virtual string ShippingZone { get; set; }
		/// <inheritdoc cref="ShippingZone"/>
		public abstract class shippingZone : PX.Data.BQL.BqlString.Field<shippingZone> { }
		#endregion
		#region BC Shipping method
		/// <summary>
		/// The shipping method in the external store.
		/// </summary>
		[PXDBString(200, IsUnicode = true, InputMask = "")]
		[PXUIField(DisplayName = "Store Shipping Method")]
		[PXDefault()]
		public virtual string ShippingMethod { get; set; }
		/// <inheritdoc cref="ShippingMethod"/>
		public abstract class shippingMethod : PX.Data.BQL.BqlString.Field<shippingMethod> { }
		#endregion

		#region Erp Ship Via
		/// <summary>
		/// The ship via in ERP.
		/// </summary>
		[PXDBString(15, IsUnicode = true)]
		[PXUIField(DisplayName = "Ship Via")]
		[PXSelector(typeof(Carrier.carrierID),
					SubstituteKey = typeof(Carrier.carrierID),
					DescriptionField = typeof(Carrier.description))]
		public virtual String CarrierID { get; set; }
		/// <inheritdoc cref="CarrierID"/>
		public abstract class carrierID : PX.Data.BQL.BqlString.Field<carrierID> { }
		#endregion
		#region Erp Shipping zone
		/// <summary>
		/// The shipping zone in ERP
		/// </summary>
		[PXDBString(15, IsUnicode = true)]
		[PXUIField(DisplayName = "Shipping Zone")]
		[PXSelector(typeof(ShippingZone.zoneID),
					SubstituteKey = typeof(ShippingZone.zoneID),
					DescriptionField = typeof(ShippingZone.description))]
		public virtual String ZoneID { get; set; }
		/// <inheritdoc cref="ZoneID"/>
		public abstract class zoneID : PX.Data.BQL.BqlString.Field<zoneID> { }
		#endregion
		#region Erp Shipping terms
		/// <summary>
		/// The shipping terms to use when mapping this shipping method.
		/// </summary>
		[PXDBString(10, IsUnicode = true)]
		[PXUIField(DisplayName = "Shipping Terms")]
		[PXSelector(typeof(ShipTerms.shipTermsID),
					SubstituteKey = typeof(ShipTerms.shipTermsID),
					DescriptionField = typeof(ShipTerms.description))]
		public virtual String ShipTermsID { get; set; }
		/// <inheritdoc cref="ShipTermsID"/>
		public abstract class shipTermsID : PX.Data.BQL.BqlString.Field<shipTermsID> { }
		#endregion

		#region Active
		/// <summary>
		/// Indicates whether this mapping is currently active.
		/// </summary>
		[PXDBBool]
		[PXUIField(DisplayName = "Active")]
		[PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual bool? Active { get; set; }
		/// <inheritdoc cref="Active"/>
		public abstract class active : PX.Data.BQL.BqlBool.Field<active> { }
		#endregion
	}

	/// <summary>
	/// Represents a shipping zone in Big Commerce.
	/// </summary>
	[Serializable]
	[PXCacheName("BCShippingZones")]
	[PXHidden]
	public class BCShippingZones : IBqlTable
	{
		#region Keys
		public static class FK
		{
			public class Binding : BCBinding.BindingIndex.ForeignKeyOf<BCPaymentMethods>.By<bindingID> { }
		}
		#endregion

		#region BindingID
		/// <summary>
		/// Represents a store to which the entity belongs.
		/// The property is a key field.
		/// </summary>
		[PXDBInt(IsKey = true)]
		public virtual int? BindingID { get; set; }
		/// <inheritdoc cref="BindingID"/>
		public abstract class bindingID : PX.Data.BQL.BqlInt.Field<bindingID> { }
		#endregion

		#region BC Shipping zone
		/// <summary>
		/// The Big Commerce shipping zone.
		/// </summary>
		[PXDBString(200, IsKey = true, IsUnicode = true, InputMask = "")]
		public virtual string ShippingZone { get; set; }
		/// <inheritdoc cref="ShippingZone"/>
		public abstract class shippingZone : PX.Data.BQL.BqlString.Field<shippingZone> { }
		#endregion

		#region BC Shipping method
		/// <summary>
		/// The Big Commerce Shipping method.
		/// </summary>
		[PXDBString(200, IsKey = true, IsUnicode = true, InputMask = "")]
		public virtual string ShippingMethod { get; set; }
		/// <inheritdoc cref="ShippingMethod"/>
		public abstract class shippingMethod : PX.Data.BQL.BqlString.Field<shippingMethod> { }
		#endregion

		#region Enabled
		/// <summary>
		/// Indicates whether this record is enabled.
		/// </summary>
		[PXBool]
		public virtual bool? Enabled { get; set; }
		/// <inheritdoc cref="Enabled"/>
		public abstract class enabled : PX.Data.BQL.BqlBool.Field<enabled> { }
		#endregion
	}
}
