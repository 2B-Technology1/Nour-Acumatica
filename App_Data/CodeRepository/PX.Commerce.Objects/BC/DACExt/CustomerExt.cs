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
using PX.Objects.AR;
using System;

namespace PX.Commerce.Objects
{
	/// <summary>
	/// DAC extension of Customer to add additional properties.
	/// </summary>
	[Serializable]
	public sealed class CustomerExt : PXCacheExtension<Customer>
	{
		public static bool IsActive() { return CommerceFeaturesHelper.CommerceEdition; }

		#region CustomerCategory
		/// <summary>
		/// The customer category, indicating whether the customer is an individual or an organization.
		/// </summary>
		[PXMergeAttributes(Method = MergeMethod.Replace)]
		[PXDBString(1, IsFixed = true)]
		[PXDefault(BCCustomerCategoryAttribute.IndividualValue)]
		[BCCustomerCategory]
		[PXUIField(DisplayName = BCAPICaptions.CustomerCategory, Visible = true, Visibility = PXUIVisibility.Visible)]
		public string CustomerCategory { get; set; }
		///<inheritdoc cref="CustomerCategory"/>
		public abstract class customerCategory : PX.Data.BQL.BqlString.Field<customerCategory> { }
		#endregion

		#region IsGuestAccount
		/// <summary>
		/// Indicates whether the customer record is a Guest Customer used for importing guest orders.
		/// </summary>
		[PXDBBool]
		[PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
		public bool? IsGuestCustomer { get; set; }
		///<inheritdoc cref="IsGuestCustomer"/>
		public abstract class isGuestCustomer : Data.BQL.BqlBool.Field<isGuestCustomer> { }
		#endregion
	}
}
