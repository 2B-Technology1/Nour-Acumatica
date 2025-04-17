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
using System;
using System.Collections.Generic;
using System.Linq;

namespace PX.Commerce.BigCommerce.API.REST
{
	public abstract class RestError
	{
		[JsonProperty("status")]
		public int Status { get; set; }

	}
	public class RestError1 : RestError
	{
		[JsonProperty("message")]
		public string Message { get; set; }

		[JsonProperty("details")]
		public Details Details { get; set; }

		[JsonProperty("errors")]
        public Errors Errors { get; set; }

		public override string ToString()
		{
			if (!string.IsNullOrEmpty(Details?.InvalidReason))
				return Details?.InvalidReason;
			return Message;
		}
	}
	public class RestError2 : RestError
	{
		[JsonProperty("title")]
		public string Title { get; set; }

		[JsonProperty("errors")]
		public Dictionary<String, String> Errors { get; set; }

		public override string ToString()
		{
			return Errors == null ? Title : String.Join("; ", Errors.Select(e => e.Value).ToArray());
		}
	}
	public class RestError3 : RestError
	{
		[JsonProperty("title")]
		public string Title { get; set; }

		[JsonProperty("errors")]
		public string[] Errors { get; set; }

		public override string ToString()
		{
			return Errors == null ? Title : String.Join("; ", Errors);
		}
	}
	public class Errors
    {
        [JsonProperty("name")]
        public string Name { get; set; }
	}

	public class Details
	{
		[JsonProperty("invalid_reason")]
		public string InvalidReason { get; set; }
	}
}
