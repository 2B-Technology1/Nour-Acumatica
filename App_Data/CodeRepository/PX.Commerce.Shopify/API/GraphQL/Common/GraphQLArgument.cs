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

namespace PX.Commerce.Shopify.API.GraphQL
{
	/// <summary>
	/// The data object for GraphQL Query argument
	/// </summary>
	public class GraphQLArgument
	{
		/// <summary>
		/// The sort order of the query node
		/// </summary>
		public int QueryNodeOrder { get; set; }

		/// <summary>
		/// The sort order of the argument in the query node
		/// </summary>
		public int ArgumentOrder { get; set; }

		/// <summary>
		/// The argument name in external platform
		/// </summary>
		public string ArgumentName { get; set; }

		/// <summary>
		/// The variable name for current data object type
		/// </summary>
		public string VariableName { get; set; }

		/// <summary>
		/// The data type of the argument in external platform
		/// </summary>
		public string ArgumentType { get; set; }

		/// <summary>
		/// The type is attached by the current argument
		/// </summary>
		public Type AttachedObjType { get; set; }

		/// <summary>
		/// The value of the argument
		/// </summary>
		public object ArgumentValue { get; set; }

		/// <summary>
		/// Use to identify the argument value whether can be null
		/// </summary>
		public bool? AllowNull { get; set; }

		/// <summary>
		/// Identity the argument value whether can be calculated in the cost point
		/// </summary>
		public bool CostCalculationRequired { get; set; }

		/// <summary>
		/// Get the argument expression that uses in the query body
		/// </summary>
		/// <returns></returns>
		public string GetArgumentExpression() => $"{ArgumentName}: ${VariableName}";

		/// <summary>
		/// Get the argument definition that uses in the query header
		/// </summary>
		/// <returns></returns>
		public string GetArgumentDefinition() => $"${VariableName}: {ArgumentType}";
	}
}
