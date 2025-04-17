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
using Newtonsoft.Json;
using PX.Commerce.Core;

namespace PX.Commerce.Shopify.API.REST
{
	#region WebHookCustomer
	[JsonObject()]
	[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
	public class WebHookCustomer
	{
		[JsonProperty("type")]
		public virtual string Type { get; set; }

		[JsonProperty("id")]
		public virtual int? Id { get; set; }

		public override string ToString()
		{
			return Id?.ToString();
		}
	}
	#endregion
	#region WebHookCustomerAddress
	[JsonObject()]
	[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
	public class WebHookCustomerAddress
	{
		[JsonProperty("type")]
		public virtual string Type { get; set; }

		[JsonProperty("id")]
		public virtual int? Id { get; set; }

		[JsonProperty("address")]
		public virtual WebHookCustomerAddressParent Customer { get; set; }

		public override string ToString()
		{
			return new Object[] { Customer?.CustomerId, Id }.KeyCombine();
		}
	}
	[JsonObject()]
	[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
	public class WebHookCustomerAddressParent
	{
		[JsonProperty("customer_id")]
		public virtual int? CustomerId { get; set; }
	}
	#endregion
	#region WebHookProduct
	[JsonObject()]
	[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
	public class WebHookProduct
	{
		[JsonProperty("type")]
		public virtual string Type { get; set; }

		[JsonProperty("id")]
		public virtual int? Id { get; set; }

		public override string ToString()
		{
			return Id?.ToString();
		}
	}
	#endregion
	#region WebHookProductCategory
	[JsonObject()]
	[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
	public class WebHookProductCategory
	{
		[JsonProperty("type")]
		public virtual string Type { get; set; }

		[JsonProperty("id")]
		public virtual int? Id { get; set; }

		public override string ToString()
		{
			return Id?.ToString();
		}
	}
	#endregion
	#region WebHookOrder
	[JsonObject()]
	[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
	public class WebHookOrder
	{
		[JsonProperty("type")]
		public virtual string Type { get; set; }

		[JsonProperty("id")]
		public virtual int? Id { get; set; }

		public override string ToString()
		{
			return Id?.ToString();
		}
	}
	#endregion
}
