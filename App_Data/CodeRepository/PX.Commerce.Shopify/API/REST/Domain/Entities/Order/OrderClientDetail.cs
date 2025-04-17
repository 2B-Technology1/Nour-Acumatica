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
using System.ComponentModel;
using Newtonsoft.Json;
using PX.Commerce.Core;

namespace PX.Commerce.Shopify.API.REST
{

	[JsonObject(Description = "client_details")]
	[Description(ShopifyCaptions.ClientDetails)]
	[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
	public class OrderClientDetails : BCAPIEntity
	{
		/// <summary>
		/// The languages and locales that the browser understands.
		/// </summary>
		[JsonProperty("accept_language")]
		public virtual string AcceptLanguage { get; set; }

		/// <summary>
		/// The browser screen height in pixels, if available.
		/// </summary>
		[JsonProperty("browser_height")]
		public virtual int? BrowserHeight { get; set; }

		/// <summary>
		/// The browser IP address.
		/// </summary>
		[JsonProperty("browser_ip")]
		public virtual string BrowserIP { get; set; }

		/// <summary>
		/// The browser screen width in pixels, if available.
		/// </summary>
		[JsonProperty("browser_width")]
		public virtual int? BrowserWidth { get; set; }

		/// <summary>
		/// A hash of the session.
		/// </summary>
		[JsonProperty("session_hash")]
		public virtual string SessionHash { get; set; }

		/// <summary>
		/// Details of the browsing client, including software and operating versions.
		/// </summary>
		[JsonProperty("user_agent")]
		public string UserAgent { get; set; }
	}

}
