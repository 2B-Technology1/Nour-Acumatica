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
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PX.Commerce.Core;

namespace PX.Commerce.BigCommerce.API.REST
{
    [JsonObject(Description = BCCaptions.WebHookMessage)]
	[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
	public class WebHookMessage
    {
        [JsonProperty("scope")]
        public virtual string Scope { get; set; }

        [JsonProperty("store_id")]
        public virtual int? StoreID { get; set; }

        [JsonProperty("hash")]
        public virtual string Hash { get; set; }

        [JsonProperty("producer")]
        public virtual string Producer { get; set; }

        [JsonProperty("created_at")]
        public virtual string DateCreatedUT { get; set; }

		[JsonIgnore]
		public virtual string Data { get; set; }

		[JsonExtensionData]
		public IDictionary<string, JToken> _additionalData;

		[OnDeserialized]
		private void OnDeserialized(StreamingContext context)
		{
			// data is not deserialized to any property and so it is added to the extension data dictionary
			Data = (string)_additionalData["data"].ToString();
		}
		public bool ShouldSerializeDateCreatedUT()
		{
			return false;
		}
	}
}
