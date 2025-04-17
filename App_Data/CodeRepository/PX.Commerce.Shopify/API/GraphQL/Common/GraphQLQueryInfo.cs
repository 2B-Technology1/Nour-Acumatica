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
using System.Linq;
using System.Reflection;

namespace PX.Commerce.Shopify.API.GraphQL
{
	/// <summary>
	/// Save the query, arguments and related cost points information
	/// </summary>
	public class GraphQLQueryInfo
	{
		/// <summary>
		/// The query string for fetching data or mutation data
		/// </summary>
		public string Query { get; private set; }

		/// <summary>
		/// The agrument variables that use in the query
		/// </summary>
		public Dictionary<string, object> Variables { get => Arguments?.ToDictionary(x => x.VariableName, x => x.ArgumentValue); } 
		private List<GraphQLArgument> Arguments { get; set; } = new List<GraphQLArgument>();
		private string CostExpression { get; set; }

		/// <summary>
		/// Create a new GraphQLQueryInfo
		/// </summary>
		/// <param name="query">The query string for fetching data or mutation data</param>
		/// <param name="arguments">The all argument details for each query nodes</param>
		/// <param name="costExpression">The cost point expression for cost point calculation</param>
		public GraphQLQueryInfo(string query, List<GraphQLArgument> arguments, string costExpression)
		{
			Query = query;
			Arguments = arguments;
			CostExpression = costExpression;
		}

		/// <summary>
		/// Get the total cost points for fetching data or mutation data with current query
		/// </summary>
		/// <returns></returns>
		public virtual int GetCostPoints()
		{
			int costPoints = 0;
			if (CostExpression != null)
			{
				costPoints = CalculateCostPoints();
			}
			return costPoints; 
		}

		/// <summary>
		/// Set the argument variable by specified the data object and the argument name.
		/// Because there are multiple argument name are the same in different query node, you need to specify which data object to set the exact argument variable
		/// For example: If you have the argument "First" for both order connection and order item connection in the same query, you need to specify the value of "First" argument is use for Order Connection or Order item connection
		/// </summary>
		/// <param name="dataObject">The data object type in the query node</param>
		/// <param name="argumentName">The argument name</param>
		/// <param name="value">The new value</param>
		/// <returns>Set the value successfully will return true, otherwise return false</returns>
		public virtual bool SetArgumentVariable(Type dataObject, string argumentName, object value)
		{
			if (Arguments?.Count <= 0) return false;

			var argument = Arguments.FirstOrDefault(x => x.AttachedObjType == dataObject && string.Equals(x.ArgumentName, argumentName, StringComparison.OrdinalIgnoreCase));
			if(argument != null )
			{
				argument.ArgumentValue = value;
				return true;
			}

			return false;
		}

		/// <summary>
		/// The cost point calculation approach
		/// The cost point calculation expression is generated based on the query structure and arguments, the cost point may change if query structure or argument is changed.
		/// </summary>
		/// <param name="node"></param>
		/// <returns></returns>
		protected virtual int CalculateCostPoints(string node = null)
		{
			int costPoints = 0;
			if (!string.IsNullOrEmpty(node))
			{
				if (Variables.TryGetValue(node, out var value))
				{
					int.TryParse(value?.ToString(), out costPoints);
				}

				return costPoints;
			}

			ECFormulaProcessor formulaProcessor = new ECFormulaProcessor();

			ECFormulaFinalDelegate getter = (nodename, names) =>
			{
				return CalculateCostPoints(names[0]);
			};
			var result = formulaProcessor.Evaluate(CostExpression, getter);
			int.TryParse(result?.ToString(), out costPoints);

			return costPoints;
		}
	}
}
