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

namespace PX.Commerce.Objects
{
	/// <summary>
	/// Describes the default payment mapping for a store type.
	/// </summary>
	[PXHidden]
	[PXCacheName("BCDefaultShippingMapping")]
	public class BCDefaultPaymentMapping : IBqlTable
	{
		#region ConnectorType
		/// <summary>
		/// Represents a connector to which the store belongs.
		/// The property is a key field.
		/// </summary>
		[PXDBString(IsKey = true)]
		public virtual string ConnectorType { get; set; }
		/// <inheritdoc cref="ConnectorType" />
		public abstract class connectorType : PX.Data.BQL.BqlString.Field<connectorType> { }
		#endregion
		#region StorePaymentMethod
		/// <summary>
		/// A default payment method for the store type.
		/// </summary>
		[PXDBString(IsKey = true)]
		public virtual string StorePaymentMethod { get; set; }
		/// <inheritdoc cref="StorePaymentMethod" />
		public abstract class storePaymentMethod : PX.Data.BQL.BqlString.Field<storePaymentMethod> { }
		#endregion
		#region CreatePaymentfromOrder
		/// <summary>
		/// Indicates whether to create payments from orders.
		/// </summary>
		[PXDBBool]
		[PXDefault(false)]
		public virtual bool? CreatePaymentfromOrder { get; set; }
		/// <inheritdoc cref="CreatePaymentfromOrder" />
		public abstract class createPaymentfromOrder : PX.Data.BQL.BqlBool.Field<createPaymentfromOrder> { }
		#endregion
	}
}
