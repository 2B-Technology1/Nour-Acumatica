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


namespace PX.Commerce.Shopify.API.GraphQL
{
	/// <summary>
	/// Constants for Shopify GraphQL field and parameter names.
	/// </summary>
	public static class GraphQLConstants
	{
		#region Connection
		public static class Connection
		{
			public const string Edges = "edges";
			public const string Nodes = "nodes";
			public const string PageInfo = "pageInfo";
			#region Arguments
			public class Arguments
			{
				public const string First = "first";
				public const string Last = "last";
				public const string After = "after";
				public const string Before = "before";
				public const string Query = "query";
				public const string Reverse = "reverse";
				public const string SortKey = "sortKey";
			}
			#endregion
		}
		#endregion
		#region Edge
		public static class Edge
		{
			public const string Cursor = "cursor";
			public const string Node = "node";
		}

		#endregion
		#region PageInfo
		public static class PageInfo
		{
			public const string EndCursor = "endCursor";
			public const string HasNextPage = "hasNextPage";
			public const string HasPreviousPage = "hasPreviousPage";
			public const string StartCursor = "startCursor";
		}
		#endregion

		#region DataType
		public static class ScalarType
		{
			public const string ARN = "ARN";
			public const string Boolean = "Boolean";
			public const string Date = "Date";
			public const string DateTime = "DateTime";
			public const string Decimal = "Decimal";
			public const string Float = "Float";
			public const string FormattedString = "FormattedString";
			public const string HTML = "HTML";
			public const string ID = "ID";
			public const string Int = "Int";
			public const string JSON = "JSON";
			public const string Money = "Money";
			public const string StorefrontID = "StorefrontID";
			public const string String = "String";
			public const string UnsignedInt64 = "UnsignedInt64";
			public const string URL = "URL";
			public const string UtcOffset = "UtcOffset";
		}

		public static class DataType
		{
			public const string Scalar = "Saclar";
			public const string Union = "Union";
			public const string Enum = "Enum";
			public const string Object = "Object";
			public const string Connection = "Connection";
			public const string Interface = "Interface";
		}
		#endregion
	}
}
