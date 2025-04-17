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
using System.Collections.Generic;

namespace PX.Commerce.Shopify.API.GraphQL
{
	/// <summary>
	/// GraphQL error response
	/// </summary>
	[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
	public class Errors
	{
		/// <summary>
		/// The error message
		/// </summary>
		[JsonProperty("message")]
		public string Message { get; set; }

		/// <summary>
		/// The error locations list
		/// </summary>
		[JsonProperty("locations")]
		public List<ErrorLocation> Locations { get; set; }

		/// <summary>
		/// The error path list
		/// </summary>
		[JsonProperty("path")]
		public string[] Paths { get; set; }

		public string Path { get => string.Join(BCConstants.Arrow, Paths); }

		/// <summary>
		/// The error extensions
		/// </summary>
		[JsonProperty("extensions")]
		public ErrorExtensions Extensions { get; set; }

	}

	[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
	public class ErrorLocation
	{
		/// <summary>
		/// THe line number that occurs error
		/// </summary>
		[JsonProperty("line")]
		public int? Line { get; set; }

		/// <summary>
		/// THe column number that occurs error
		/// </summary>
		[JsonProperty("column")]
		public int? Column { get; set; }

	}

	[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
	public class ErrorExtensions
	{
		/// <summary>
		/// THe error code
		/// </summary>
		[JsonProperty("code")]
		public string Code { get; set; }

		/// <summary>
		/// THe type name related to error
		/// </summary>
		[JsonProperty("typeName")]
		public string TypeName { get; set; }

	}
}
