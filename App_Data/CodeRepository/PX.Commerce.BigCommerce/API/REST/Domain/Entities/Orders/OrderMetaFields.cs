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
using PX.Commerce.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PX.Commerce.BigCommerce.API.REST
{
	[Description(BigCommerceCaptions.Metafields)]
	[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
	public class OrdersMetaFieldData : BCAPIEntity
	{
		[JsonProperty("id")]
		[Description(BigCommerceCaptions.ID)]
		public virtual int? Id { get; set; }
		public bool ShouldSerializeId()
		{
			return false;
		}

		[JsonProperty("key")]
		[Description(BigCommerceCaptions.MetaKeywords)]
		public virtual string Key { get; set; }

		[JsonProperty("namespace", NullValueHandling = NullValueHandling.Ignore)]
		[Description(BigCommerceCaptions.MetaNamespace)]
		public virtual string Namespace { get; set; }

		[JsonProperty("value", NullValueHandling = NullValueHandling.Ignore)]
		[Description(BigCommerceCaptions.Value)]
		public virtual string Value { get; set; }

		[JsonProperty("permission_set", NullValueHandling = NullValueHandling.Ignore)]
		[Description(BigCommerceCaptions.Value)]
		public virtual PermissionSet? PermissionSet { get; set; }

	}

	[JsonObject(Description = "Order MetaFieldList (BigCommerce API v3 response)")]
	public class OrdersMetaFieldList : IEntitiesResponse<OrdersMetaFieldData>
	{
		[JsonProperty("data")]
		public List<OrdersMetaFieldData> Data { get; set; }

		[JsonProperty("meta")]
		public Meta Meta { get; set; }
	}

}
