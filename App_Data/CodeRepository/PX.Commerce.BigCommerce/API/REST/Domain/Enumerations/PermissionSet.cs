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

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;

namespace PX.Commerce.BigCommerce.API.REST
{
	[JsonConverter(typeof(StringEnumConverter))]
	public enum PermissionSet
	{
		[EnumMember(Value = "app_only")]
		App_Only,
		[EnumMember(Value = "read")]
		Read,
		[EnumMember(Value = "write")]
		Write,
		[EnumMember(Value = "read_and_sf_access")]
		ReadAndSFAccess,
		[EnumMember(Value = "write_and_sf_access")]
		WriteAndSfAccess,
	}
}
		