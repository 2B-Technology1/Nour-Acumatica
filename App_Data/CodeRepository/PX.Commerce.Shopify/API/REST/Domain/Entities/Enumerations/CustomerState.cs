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

using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace PX.Commerce.Shopify.API
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum CustomerState
    {
		/// <summary>
		/// disabled: The customer doesn't have an active account. Customer accounts can be disabled from the Shopify admin at any time.
		/// </summary>
		[EnumMember(Value = "disabled")]
		Disabled = 0,

		/// <summary>
		/// invited: The customer has received an email invite to create an account.
		/// </summary>
		[EnumMember(Value = "invited")]
		Invited = 1,

		/// <summary>
		/// enabled: The customer has created an account.
		/// </summary>
		[EnumMember(Value = "enabled")]
		Enabled = 2,

		/// <summary>
		/// declined: The customer declined the email invite to create an account.
		/// </summary>
		[EnumMember(Value = "declined")]
		Declined = 3
    }
}
