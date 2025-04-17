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
using PX.Objects.AR;
using PX.Objects.CS;

namespace PX.Objects.CC
{
	/// <summary>
	/// Represents database fields which store Payment Link specific data.
	/// </summary>
	public sealed class CustomerClassPayLink : PXCacheExtension<CustomerClass>
	{
		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<FeaturesSet.acumaticaPayments>();
		}

		#region DisablePayLinkSync
		public abstract class disablePayLink : Data.BQL.BqlBool.Field<disablePayLink> { }
		/// <summary>
		/// Disable the Payment Link feature for Cusotmers that belong to Customer Class.
		/// </summary>
		[PXDBBool]
		[PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Exclude from Payment Link Processing")]
		public bool? DisablePayLink { get; set; }
		#endregion

		#region DeliveryMethod
		public abstract class deliveryMethod : Data.BQL.BqlString.Field<deliveryMethod> { }
		/// <summary>
		/// Payment Link delivery method (N - none, E - email).
		/// </summary>
		[PXDBString(1, IsFixed = true)]
		[PXDefault(PayLinkDeliveryMethod.None, PersistingCheck = PXPersistingCheck.Nothing)]
		[PayLinkDeliveryMethod.List]
		[PXUIField(DisplayName = "Delivery Method")]
		public string DeliveryMethod { get; set; }
		#endregion

		#region AllowOverwriteDeliveryMethod
		public abstract class allowOverrideDeliveryMethod : Data.BQL.BqlBool.Field<allowOverrideDeliveryMethod> { }
		/// <summary>
		/// Allow delivery method override on the document level.
		/// </summary>
		[PXDBBool]
		[PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Enable Delivery Method Override")]
		public bool? AllowOverrideDeliveryMethod { get; set; }
		#endregion

		#region PayLinkPaymentMethod
		public abstract class payLinkPaymentMethod : Data.BQL.BqlString.Field<payLinkPaymentMethod> { }
		/// <summary>
		/// Allowed means of payments for Payment Link (N - cc + eft, E - eft, C - credit card)
		/// </summary>
		[PXDBString(1, IsFixed = true)]
		[PXDefault(CC.PayLinkPaymentMethod.NotSpecified, PersistingCheck = PXPersistingCheck.Nothing)]
		[PayLinkPaymentMethod.List]
		[PXUIField(DisplayName = "Allowed Means of Payment")]
		public string PayLinkPaymentMethod { get; set; }
		#endregion
	}
}
