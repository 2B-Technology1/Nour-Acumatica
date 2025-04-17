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
using PX.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace PX.Commerce.Shopify.API.GraphQL
{
	/// <summary>
	/// Create the query and related cost points info
	/// </summary>
	public class GraphQLQueryBuilder
	{
		/// <summary>
		/// Setup the depth of the path for Union type object
		/// </summary>
		public int UnionPathDepth { get => defaultUnionPathDepth; set => defaultUnionPathDepth = value > 0 ? value : 0; }
		/// <summary>
		/// Setup the depth of the path for Object type object
		/// </summary>
		public int ObjectPathDepth { get => defaultObjectPathDepth; set => defaultObjectPathDepth = value > 0 ? value : 0; }
		/// <summary>
		/// Setup the depth of the path for Connection type object
		/// </summary>
		public int ConnectionPathDepth { get => defaultConnectionPathDepth; set => defaultConnectionPathDepth = value > 0 ? value : 0; }
		/// <summary>
		/// Setup whether allow recursive search if the sub object is the same type with the top query object
		/// </summary>
		public bool AllowRecursiveObject { get => allowRecursiveObject; set => allowRecursiveObject = value; }

		private List<QueryNode> QueryNodesList = new List<QueryNode>();
		private List<GraphQLArgument> TotalQueryArguments = new List<GraphQLArgument> { };
		private Dictionary<Type, object> QueryArguments = new Dictionary<Type, object>();
		private Dictionary<string, List<string>> SpecifiedNames = new Dictionary<string, List<string>>();
		private Type TypeOfQueryObject = null;
		private int QueryOrder = 0;
		private int defaultUnionPathDepth = 2;
		private int defaultObjectPathDepth = 2;
		private int defaultConnectionPathDepth = 2;
		private bool allowRecursiveObject = false;
		private const string userErrorsName = "UserErrors";
		private const string userErrorNode = " userErrors{ field message } ";
		private const string paginationNode = " pageInfo{ hasNextPage endCursor hasPreviousPage startCursor} ";

		public GraphQLQueryBuilder() { }

		public GraphQLQueryBuilder(int unionPathDepth, int objectPathDepth, int connectionPathDepth)
		{
			defaultUnionPathDepth = unionPathDepth;
			defaultObjectPathDepth = objectPathDepth;
			defaultConnectionPathDepth = connectionPathDepth;
		}

		/// <summary>
		/// The method will generate the GraphQL query and cost point calculation expression
		/// </summary>
		/// <param name="typeOfQueryObject">The top object that the query generates from </param>
		/// <param name="queryType">The query type, should be the value from <see cref="GraphQLQueryType"/> </param>
		/// <param name="variables">The variables that apply to the arguments</param>
		/// <param name="includedSubFields">Whether includes the sub fields in the query</param>
		/// <param name="includedSubConnections">Whether includes the sub connections in the query</param>
		/// <param name="specifiedFieldsOnly">The generated query is based on the specified fields only </param>
		/// <returns></returns>
		public virtual GraphQLQueryInfo GetQueryResult(Type typeOfQueryObject, GraphQLQueryType queryType, Dictionary<Type, object> variables,
			bool includedSubFields = false, bool includedSubConnections = false, params string[] specifiedFieldsOnly)
		{
			TypeOfQueryObject = typeOfQueryObject;
			QueryArguments = variables.ToDictionary(x => x.Key, x => x.Value);
			GeneralSpecifiedNamesDic(specifiedFieldsOnly);
			ParseTopQueryObject(queryType, includedSubFields, includedSubConnections);
			
			return new GraphQLQueryInfo(string.Concat(QueryNodesList.OrderBy(x => x.QueryOrder).Select(x => x.QueryStr)), TotalQueryArguments, GetCostPointExpression(queryType));
		}

		/// <summary>
		/// Generate the query nodes from the top object
		/// </summary>
		/// <param name="queryType"></param>
		/// <param name="includedSubFields"></param>
		/// <param name="includedSubConnections"></param>
		/// <exception cref="Exception"></exception>
		/// <exception cref="PXException"></exception>
		protected virtual void ParseTopQueryObject(GraphQLQueryType queryType, bool includedSubFields = false, bool includedSubConnections = false)
		{
			var graphObjectAttr = TypeOfQueryObject.GetCustomAttribute<GraphQLObjectAttribute>();
			if (graphObjectAttr == null || (queryType == GraphQLQueryType.Node && string.IsNullOrEmpty(graphObjectAttr.NodeName))
				|| (queryType == GraphQLQueryType.Connection && string.IsNullOrEmpty(graphObjectAttr.ConnectionName))
				|| (queryType == GraphQLQueryType.Mutation && string.IsNullOrEmpty(graphObjectAttr.MutationName)))
			{
				return;
			}

			bool subItemFound = false;
			var queryName = queryType == GraphQLQueryType.Node ? graphObjectAttr.NodeName : (queryType == GraphQLQueryType.Connection ? graphObjectAttr.ConnectionName : graphObjectAttr.MutationName);
			int queryPath = 0, parentQueryOrder = 0;
			QueryNodesList.Clear();

			QueryNode queryHeader = CreateQueryNode(TypeOfQueryObject.Name, queryPath++, TypeOfQueryObject, parentQueryOrder, QueryNodeKind.Header, queryType == GraphQLQueryType.Mutation ? $"mutation {queryName}": $"query get{queryName}");

			var arguments = GetArgumentVariables(TypeOfQueryObject, queryType == GraphQLQueryType.Mutation ? false : (bool?)null);
			var queryStr = arguments.Count > 0 ? $" {queryName}({string.Join(" , ", arguments.OrderBy(x => x.ArgumentOrder).Select(x => x.GetArgumentExpression()))}) {{" : $" {queryName} {{";
			var costPointExp = string.Empty;
			if (queryType == GraphQLQueryType.Connection)
			{
				if (arguments?.Count <= 0)
				{
					throw new Exception(GraphQLMessages.QueryWithoutAgruments);
				}
				var arg = GetArgumentForCostPoint(arguments);
				queryStr += $" nodes {{ ";
				costPointExp = $"({CostPoint.Connection} + [{arg.VariableName}] * ({CostPoint.Object} ";
			}
			else if (queryType == GraphQLQueryType.Mutation)
			{
				if (arguments?.Count <= 0) throw new PXException(GraphQLMessages.MutationWithoutAgruments, queryName);
				costPointExp = null;
			}
			else
				costPointExp = $"({CostPoint.Object} ";
				
			QueryNode queryObjVariable = CreateQueryNode(TypeOfQueryObject.Name, queryPath++, TypeOfQueryObject, parentQueryOrder, QueryNodeKind.Header, queryStr, costPointExp, arguments);

			var fieldsInfo = TypeOfQueryObject.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
			foreach (var oneFieldInfo in fieldsInfo)
			{
				if (IsSpecifiedFieldName(oneFieldInfo.Name) == false)
					continue;

				var attr = oneFieldInfo.GetCustomAttribute<GraphQLFieldAttribute>();
				var ignoreAttr = oneFieldInfo.GetCustomAttribute<JsonIgnoreAttribute>();

				if (attr == null || ignoreAttr != null) continue;

				attr.AttachedFieldName= oneFieldInfo.Name;
				attr.AttachedFieldType ??= oneFieldInfo.PropertyType;

				subItemFound = ParseField(attr, queryPath, null, parentQueryOrder, includedSubFields, includedSubConnections) || subItemFound;
			}

			//only there are sub fields were found in the object, we should add the object to the query result.
			//graphQL doesn't allow empty object in the query
			if (subItemFound)
			{
				if(queryType == GraphQLQueryType.Mutation)
					AddUserErrorNodeForMutation(queryPath, parentQueryOrder);

				AddQueryNodeEnding(queryObjVariable, queryPath, queryType == GraphQLQueryType.Connection ? $" }} {paginationNode} }} }}" : $" }} }}", queryType == GraphQLQueryType.Connection ? $"))" : $")");

				var argumentDefinitions = TotalQueryArguments?.Select(a => a.GetArgumentDefinition())?.ToList();
				queryHeader.QueryStr += argumentDefinitions?.Count > 0 ? $"({string.Join(",", argumentDefinitions)}) {{" : $" {{ ";

				QueryNodesList.Add(queryHeader);
			}
			else
				throw new PXException(GraphQLMessages.NoQueryFields, TypeOfQueryObject.Name);

		}

		/// <summary>
		/// Generate the query nodes from the specified field
		/// </summary>
		/// <param name="attr"></param>
		/// <param name="queryPath"></param>
		/// <param name="allowPathDepth"></param>
		/// <param name="parentQueryOrder"></param>
		/// <param name="includedSubFields"></param>
		/// <param name="includedSubConnections"></param>
		/// <returns></returns>
		private bool ParseField(GraphQLFieldAttribute attr, int queryPath, int? allowPathDepth, int parentQueryOrder, bool? includedSubFields = null, bool? includedSubConnections = null)
		{
			bool subItemFound = false;
			//To avoid recursive query for the same object.
			if (!allowRecursiveObject && attr.AttachedFieldType == TypeOfQueryObject) return subItemFound;

			bool isDepthAllowed = (!allowPathDepth.HasValue || allowPathDepth.Value > 1);
			bool hasIncludedSubfields = (includedSubFields == null || includedSubFields.Value);
			if (attr.FieldDataType == GraphQLConstants.DataType.Scalar || attr.FieldDataType == GraphQLConstants.DataType.Enum)
			{
				QueryNodesList.Add(CreateQueryNode(attr.AttachedFieldName, queryPath, attr.AttachedFieldType, parentQueryOrder, QueryNodeKind.Node, $" {attr.FieldName} "));
				subItemFound = true;
			}
			else if (attr.FieldDataType == GraphQLConstants.DataType.Union && attr.AttachedFieldType != null
				&& hasIncludedSubfields && isDepthAllowed)
			{
				QueryNode subHeader = CreateQueryNode(attr.AttachedFieldName, queryPath, attr.AttachedFieldType, parentQueryOrder, QueryNodeKind.Header, $" {attr.FieldName}{{ ", $"({CostPoint.Union}");
				subItemFound = ParseUnion(subHeader, queryPath, allowPathDepth == null? defaultUnionPathDepth : (allowPathDepth.Value -1)) || subItemFound;
			}
			else if (attr.FieldDataType == GraphQLConstants.DataType.Interface && attr.AttachedFieldType != null
				&& hasIncludedSubfields && isDepthAllowed)
			{
				QueryNode subHeader = CreateQueryNode(attr.AttachedFieldName, queryPath, attr.AttachedFieldType, parentQueryOrder, QueryNodeKind.Header, String.Empty, $"({CostPoint.Interface}");
				subItemFound = ParseInterface(subHeader, queryPath, allowPathDepth == null? defaultUnionPathDepth : (allowPathDepth.Value -1)) || subItemFound;
			}
			else if (attr.FieldDataType == GraphQLConstants.DataType.Object && attr.AttachedFieldType != null
				&& hasIncludedSubfields && isDepthAllowed)
			{
				List<GraphQLArgument> objectArguments = GetArgumentVariables(attr.AttachedFieldType, false);
				string objectQueryStr = objectArguments.Count > 0 ? $" {attr.FieldName}({string.Join(" , ", objectArguments.OrderBy(x => x.ArgumentOrder).Select(x => x.GetArgumentExpression()))}) {{" : $" {attr.FieldName} {{";

				QueryNode subHeader = CreateQueryNode(attr.AttachedFieldName, queryPath, attr.AttachedFieldType, parentQueryOrder, QueryNodeKind.Header, objectQueryStr, $"({CostPoint.Object} ", objectArguments);
				subItemFound = ParseObject(subHeader, queryPath, allowPathDepth == null ? defaultObjectPathDepth : (allowPathDepth.Value - 1)) || subItemFound;
			}
			else if (attr.FieldDataType == GraphQLConstants.DataType.Connection && attr.AttachedFieldType != null
				&& (includedSubConnections == null || includedSubConnections.Value) && isDepthAllowed)
			{
				List<GraphQLArgument> conArguments = GetArgumentVariables(attr.AttachedFieldType);
				if (conArguments.Count > 0)
				{
					string objectQueryStr = $" {attr.FieldName}({string.Join(" , ", conArguments.OrderBy(x => x.ArgumentOrder).Select(x => x.GetArgumentExpression()))}) {{ nodes {{ ";
					var arg = GetArgumentForCostPoint(conArguments);
					var conCostPointExp = $"({CostPoint.Connection} + [{arg.VariableName}] * ({CostPoint.Object} ";

					QueryNode subHeader = CreateQueryNode(attr.AttachedFieldName, queryPath, attr.AttachedFieldType, parentQueryOrder, QueryNodeKind.Header, objectQueryStr, conCostPointExp, conArguments);
					subItemFound = ParseObject(subHeader, queryPath, allowPathDepth == null ? defaultConnectionPathDepth : (allowPathDepth.Value - 1), true) || subItemFound;
				}
			}

			return subItemFound;
		}

		/// <summary>
		/// Generate the query nodes from the Union type object
		/// </summary>
		/// <param name="header"></param>
		/// <param name="parentQueryPath"></param>
		/// <param name="allowPathDepth"></param>
		/// <returns></returns>
		protected virtual bool ParseUnion(QueryNode header, int parentQueryPath, int allowPathDepth)
		{
			bool subItemFound = false;
			int parentQueryOrder = QueryOrder, queryPath = parentQueryPath + 1;

			//stop if the depth of path is reached.
			if (allowPathDepth <= 0) return subItemFound;

			var fieldsInfo = header.QueryDataType.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
			IEnumerable<GraphQLFieldAttribute> unionFieldAttrs = fieldsInfo?.SelectMany(oneFieldInfo => GetGraphQLFieldAttributes(oneFieldInfo, header.QueryFieldName, false));			

			//all fields in Union object must be combined in the same union object type
			foreach (var groupedAttrs in unionFieldAttrs.GroupBy(x => x.UnionObjectType) ?? Enumerable.Empty<IGrouping<string, GraphQLFieldAttribute>>())
			{
				QueryNode subHeader = CreateQueryNode(header.QueryFieldName, queryPath, null, parentQueryOrder, QueryNodeKind.Header, $" ... on {groupedAttrs.Key} {{");
				bool subfieldFound = false;
				foreach (var oneAttr in groupedAttrs)
				{
					subfieldFound = ParseField(oneAttr, queryPath, allowPathDepth, parentQueryOrder) || subfieldFound;
				}

				if (subfieldFound)
				{
					AddQueryNodeEnding(subHeader, queryPath, $" }} ", null);
					subItemFound = true;
				}
			}

			if (subItemFound)
			{
				AddQueryNodeEnding(header, queryPath, $" }} ", $")");
			}

			return subItemFound;
		}

		/// <summary>
		/// Generate the query nodes from the Interface type object
		/// </summary>
		/// <param name="header"></param>
		/// <param name="parentQueryPath"></param>
		/// <param name="allowPathDepth"></param>
		/// <returns>A boolean indicating if the operation was successful.</returns>
		protected virtual bool ParseInterface(QueryNode header, int parentQueryPath, int allowPathDepth)
		{
			bool subItemFound = false;
			int parentQueryOrder = QueryOrder, queryPath = parentQueryPath +1;

			//stop if the depth of path is reached.
			if (allowPathDepth <= 0) return subItemFound;

			var fieldsInfo = header.QueryDataType.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
			IEnumerable<GraphQLFieldAttribute> interfaceFieldAttrs = fieldsInfo?.SelectMany(oneFieldInfo => GetGraphQLFieldAttributes(oneFieldInfo, header.QueryFieldName, false));

			//all fields with same Interfaces (UnionObjectType) must be combined in the same union object type.
			foreach (var groupedAttrs in interfaceFieldAttrs.GroupBy(x => x.UnionObjectType) ?? Enumerable.Empty<IGrouping<string, GraphQLFieldAttribute>>())
			{
				QueryNode subHeader = CreateQueryNode(header.QueryFieldName, queryPath, null, parentQueryOrder, QueryNodeKind.Header, $" ... on {groupedAttrs.Key} {{");
				bool subfieldFound = false;
				foreach (GraphQLFieldAttribute oneAttr in groupedAttrs)
				{
					subfieldFound = ParseField(oneAttr, queryPath, allowPathDepth, parentQueryOrder) || subfieldFound;
				}

				if (subfieldFound)
				{
					AddQueryNodeEnding(subHeader, queryPath, $" }} ", null);
					subItemFound = true;
				}
			}

			if (subItemFound)
			{
				AddQueryNodeEnding(header, queryPath, String.Empty, $")");
			}

			return subItemFound;
		}

		/// <summary>
		/// Generate the query nodes from the Object type object
		/// </summary>
		/// <param name="header"></param>
		/// <param name="parentQueryPath"></param>
		/// <param name="allowPathDepth"></param>
		/// <param name="isConnection"></param>
		/// <returns></returns>
		protected virtual bool ParseObject(QueryNode header, int parentQueryPath, int allowPathDepth, bool isConnection = false)
		{
			bool subItemFound = false;
			int parentQueryOrder = QueryOrder, queryPath = parentQueryPath + 1;

			//stop if the depth of path is reached.
			if (allowPathDepth <= 0) return subItemFound;

			var fieldsInfo = header.QueryDataType.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
			var graphQLFieldAttributes = fieldsInfo.SelectMany(oneFieldInfo => GetGraphQLFieldAttributes(oneFieldInfo, header.QueryFieldName, false));
			foreach (var attribute in graphQLFieldAttributes)
			{
				subItemFound = ParseField(attribute, queryPath, allowPathDepth, parentQueryOrder) || subItemFound;
			}

			if (subItemFound)
			{
				AddQueryNodeEnding(header, queryPath, isConnection ? $" }} {paginationNode} }}" : $" }} ", isConnection ? $"))" : $")");
			}

			return subItemFound;
		}

		/// <summary>
		/// Returns a list of <see cref="GraphQLFieldAttribute"/> for the provided <paramref name="oneFieldInfo"/>.
		/// </summary>
		/// <param name="oneFieldInfo"></param>
		/// <param name="queryFieldName"></param>
		/// <param name="checkUnionObjectType"></param>
		public virtual List<GraphQLFieldAttribute> GetGraphQLFieldAttributes(PropertyInfo oneFieldInfo, string queryFieldName, bool checkUnionObjectType = true)
		{
			IEnumerable<GraphQLFieldAttribute> attributes = oneFieldInfo.GetCustomAttributes<GraphQLFieldAttribute>();
			JsonIgnoreAttribute ignoreAttr = oneFieldInfo.GetCustomAttribute<JsonIgnoreAttribute>();
			List<GraphQLFieldAttribute> listOfResults = new List<GraphQLFieldAttribute>();
			foreach (GraphQLFieldAttribute attribute in attributes)
			{
				if (attribute == null || (checkUnionObjectType && string.IsNullOrEmpty(attribute.UnionObjectType)) || ignoreAttr != null) continue;
				if (IsSpecifiedFieldName($"{queryFieldName}.{oneFieldInfo.Name}", attribute.FieldDataType == GraphQLConstants.DataType.Object) == false)
					continue;

				attribute.AttachedFieldName = oneFieldInfo.Name;
				attribute.AttachedFieldType ??= oneFieldInfo.PropertyType;

				listOfResults.Add(attribute);
			}

			return listOfResults;
		}

		protected QueryNode CreateQueryNode(string fieldName, int queryPath, Type queryDataType, int? parentQueryOrder, QueryNodeKind? nodeKind, string queryStr, string costPointExpression = null, List<GraphQLArgument> arguments = null)
		{
			return new QueryNode()
			{
				QueryOrder = QueryOrder++,
				QueryPath = queryPath,
				QueryDataType = queryDataType,
				ParentQueryOrder = parentQueryOrder,
				QueryStr = queryStr,
				Arguments = arguments,
				QueryFieldName= fieldName,
				QueryNodeKind = nodeKind,
				CostPointExpression = costPointExpression
			};
		}

		protected string GetCostPointExpression(GraphQLQueryType? queryType = null)
		{
			//For all mutations, Shopify has a fixed 10 cost points.We don't need to do the calculation 
			if (queryType != null && queryType == GraphQLQueryType.Mutation)
			{
				return $"={CostPoint.Mutation}";
			}

			StringBuilder costPointExpr = new StringBuilder();
			bool isFirstExp = true;
			foreach (var queryNode in QueryNodesList.OrderBy(x => x.QueryOrder).Where(x => x.CostPointExpression != null))
			{
				if (isFirstExp)
				{
					costPointExpr.Clear();
					costPointExpr.Append($"={queryNode.CostPointExpression}");
					isFirstExp = false;
				}
				else if (queryNode.QueryNodeKind == QueryNodeKind.End)
					costPointExpr.Append($"{queryNode.CostPointExpression}");
				else
					costPointExpr.Append($"+{queryNode.CostPointExpression}");
			}
			return costPointExpr.ToString();
		}

		protected GraphQLArgument GetArgumentForCostPoint(List<GraphQLArgument> arguments)
		{
			if (arguments.All(x => x.AllowNull == false && x.ArgumentValue == null))
				throw new Exception(GraphQLMessages.QueryAgrumentWithoutValue);

			//Find the first argument has value and need to include in the cost point calculation.
			var arg = arguments.FirstOrDefault(x => x.ArgumentValue != null && x.CostCalculationRequired == true);
			if (arg == null)
				throw new Exception(GraphQLMessages.QueryWithoutAgruments);

			return arg;
		}

		protected List<GraphQLArgument> GetArgumentVariables(Type typeOfQueryObject, bool? costCalculationRequired = null)
		{
			List<GraphQLArgument> arguments = new List<GraphQLArgument>();
			int argumentOrder = 0;
			List<Type> removedKey = new List<Type>();
			foreach (var variable in QueryArguments)
			{
				if(variable.Key.GetGenericArguments().FirstOrDefault() == typeOfQueryObject ||
					typeOfQueryObject.GetNestedTypes().Any(nestedClass => variable.Key.DeclaringType.ReflectedType != null && nestedClass.FullName.StartsWith(variable.Key.DeclaringType.ReflectedType?.FullName)))
				{
					var attr = variable.Key.GetCustomAttribute<GraphQLArgumentAttribute>();
					if (attr != null)
					{
						arguments.Add(new GraphQLArgument()
						{
							QueryNodeOrder = QueryOrder,
							ArgumentOrder = argumentOrder++,
							ArgumentName = attr.AgrumentLabel,
							VariableName = $"{typeOfQueryObject.Name}_{attr.AgrumentLabel}{QueryOrder}",
							ArgumentType = attr.AllowNull == false ? $"{attr.AgrumentType}!" : attr.AgrumentType,
							AttachedObjType = typeOfQueryObject,
							ArgumentValue = variable.Value,
							AllowNull = attr.AllowNull,
							CostCalculationRequired = costCalculationRequired != null ? costCalculationRequired.Value : attr.CostCalculationRequired
						});
						removedKey.Add(variable.Key);
					}
					
				}
			}

			if(arguments.Count > 0) TotalQueryArguments.AddRange(arguments);

			//if the key has used, should remove it to avoid recursive usage.
			removedKey.ForEach(key => QueryArguments.Remove(key));

			return arguments;
		}

		private void AddQueryNodeEnding(QueryNode header, int queryPath, string queryNodeEndingStr, string costPointEndingStr = null)
		{
			QueryNodesList.Add(header);
			QueryNodesList.Add(CreateQueryNode(header.QueryFieldName, queryPath, header.QueryDataType, header.ParentQueryOrder, QueryNodeKind.End, queryNodeEndingStr, costPointEndingStr));
		}

		private void AddUserErrorNodeForMutation(int queryPath, int? parentQueryOrder)
		{
			if(QueryNodesList.Any(x => x.ParentQueryOrder == parentQueryOrder && string.Equals(x.QueryFieldName, userErrorsName, StringComparison.OrdinalIgnoreCase)) != true)
				QueryNodesList.Add(CreateQueryNode(userErrorsName, queryPath, typeof(MutationUserError), parentQueryOrder, QueryNodeKind.End, userErrorNode, null));
		}

		private void GeneralSpecifiedNamesDic(string[] specifiedFieldsOnly)
		{
			if (specifiedFieldsOnly?.Length > 0)
			{
				foreach (string field in specifiedFieldsOnly.Where(x => !string.IsNullOrEmpty(x)))
				{
					var key = field.ToLower();
					var keys = key.Split(new string[] { "." }, StringSplitOptions.RemoveEmptyEntries);

					if (SpecifiedNames.ContainsKey(keys[0]) == false)
						SpecifiedNames[keys[0]] = new List<string>() { key };
					else if (SpecifiedNames[keys[0]].Any(x => string.Equals(x, key, StringComparison.OrdinalIgnoreCase)) == false)
					{
						SpecifiedNames[keys[0]].Add(key);
					}
				}
			}
		}

		private bool IsSpecifiedFieldName(string fieldName, bool isSubObject = false)
		{
			if (SpecifiedNames?.Count == 0 || string.IsNullOrEmpty(fieldName)) return true;

			//If the field name is in the list
			var key = fieldName.ToLower();
			if (SpecifiedNames.ContainsKey(key) == true) return true;

			//If the field name is not in the list, we should check the object type. If the object type name is in the list, we should include the fields of the object type
			//For example: if specify the name is "LineItems", we should allow to get all field under "LineItems"; if the name is "LineItems.CreatedAt", we should only get "CreatedAt" field.
			var keys = key.Split(new string[] { "." }, StringSplitOptions.RemoveEmptyEntries);
			if (keys.Length > 1)
			{
				if (SpecifiedNames.TryGetValue(keys[0], out var specifiedValues) == true)
				{
					if (specifiedValues.Any(value => value == keys[0] || value == key))
					{
						if (isSubObject && SpecifiedNames.ContainsKey(keys[1]) == false)
						{
							SpecifiedNames[keys[1]] = new List<string>() { keys[1] };
						}
						return true;
					}
				}
			}

			return false;
		}

		protected class QueryNode
		{
			public int QueryOrder { get; set; } = 0;
			public int QueryPath { get; set; } = 0;
			public int? ParentQueryOrder { get; set; }
			public string QueryStr { get; set; }
			public List<GraphQLArgument> Arguments { get; set; } = new List<GraphQLArgument>();
			public Type QueryDataType { get; set; }
			public string QueryFieldName { get; set; }
			public QueryNodeKind? QueryNodeKind { get; set; }
			public string CostPointExpression { get; set; }
		}

		protected class CostPoint
		{
			public const int Connection = 2;
			public const int Enum = 1;
			public const int Union = 1;
			public const int Interface = 1;
			public const int Object = 1;
			public const int Mutation = 10;
		}

		protected enum QueryNodeKind
		{
			Header,
			Node,
			End
		}
	}
}
