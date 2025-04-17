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
using PX.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace PX.Commerce.Shopify.API.GraphQL
{
	/// <summary>
	/// Returns information about pagination in a connection, in accordance with the Relay specification.
	/// </summary>
	public static class GraphQLHelper 
	{
		/// <summary>
		/// Get the fields list from a query object, these fields will be used in the GraphQL query string
		/// </summary>
		/// <param name="typeOfQueryObject">The query object</param>
		/// <param name="includedSubFields">Identify whether includes the sub fields of current query object</param>
		/// <param name="isSubItemObject">Identify current query object is a top level object or sub level object of the top level object</param>
		/// <param name="specifiedFieldNamesOnly">The specified fields should be used in the query string</param>
		/// <returns>The fields list with JsonPropertyAttribute in the query object</returns>
		public static List<string> GetQueryFields(Type typeOfQueryObject, bool includedSubFields = false, bool isSubItemObject = false, params string[] specifiedFieldNamesOnly)
		{
			List<string> fieldsList = new List<string>();
			var fieldsInfo = typeOfQueryObject.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
			foreach (var oneFieldInfo in fieldsInfo)
			{
				var attr = oneFieldInfo.GetCustomAttribute<JsonPropertyAttribute>();
				var ignoreAttr = oneFieldInfo.GetCustomAttribute<JsonIgnoreAttribute>();
				//sometime the fields in the sub object may cause the unknow exception, so if the field in the sub object and its ReferenceLoopHandling is Ignore, we skip this field.
				if (attr == null || ignoreAttr != null || (isSubItemObject && attr.ReferenceLoopHandling == ReferenceLoopHandling.Ignore)) continue;
				if ((specifiedFieldNamesOnly?.Length > 0) && specifiedFieldNamesOnly.Any(x => string.Equals(x, oneFieldInfo.Name, StringComparison.OrdinalIgnoreCase)) == false)
					continue;
				var fieldType = oneFieldInfo.PropertyType;
				if (fieldType == typeof(string) || fieldType == typeof(int) || fieldType == typeof(decimal) || fieldType == typeof(double) || fieldType == typeof(float) || fieldType == typeof(Enum)
					 || fieldType == typeof(Boolean) || fieldType == typeof(DateTime) || fieldType == typeof(long) || fieldType == typeof(Guid) || fieldType == typeof(Byte)
					 || (fieldType.IsGenericType && fieldType.GetGenericTypeDefinition() == typeof(Nullable<>)))
				{
					fieldsList.Add(attr.PropertyName);
				}
				else if ((includedSubFields || ((specifiedFieldNamesOnly?.Length > 0) && specifiedFieldNamesOnly.Any(x => string.Equals(x, oneFieldInfo.Name, StringComparison.OrdinalIgnoreCase))))
					&& fieldType.IsClass && !fieldType.IsGenericType)
				{
					var subFields = GetQueryFields(fieldType, false, true);
					if (subFields.Any())
					{
						fieldsList.Add($"{attr.PropertyName}{{ {string.Join(",", subFields)} }}");
					}
				}
			}
			return fieldsList;
		}

		public static TMutationObj ConvertToMutationObject<TMutationObj, TOriginalObj>(TOriginalObj originalObject)
			where TMutationObj : class, new()
			where TOriginalObj : class
		{
			if (originalObject == null)
				return null;
			TMutationObj newT = new TMutationObj();
			var fieldsInfo = typeof(TMutationObj).GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
			var originalFieldsInfo = typeof(TOriginalObj).GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
			foreach (var oneFieldInfo in fieldsInfo)
			{
				var attr = oneFieldInfo.GetCustomAttribute<JsonPropertyAttribute>();
				if (attr == null) continue;
				var fieldType = oneFieldInfo.PropertyType;
				if (fieldType == typeof(string) || fieldType == typeof(int) || fieldType == typeof(decimal) || fieldType == typeof(double) || fieldType == typeof(float) || fieldType == typeof(Enum)
					 || fieldType == typeof(Boolean) || fieldType == typeof(DateTime) || fieldType == typeof(long) || fieldType == typeof(Guid) || fieldType == typeof(Byte)
					 || (fieldType.IsGenericType && fieldType.GetGenericTypeDefinition() == typeof(Nullable<>)))
				{
					var matchedField = originalFieldsInfo.FirstOrDefault(x => x.GetCustomAttribute<JsonPropertyAttribute>()?.PropertyName == attr.PropertyName);
					if (matchedField != null)
					{
						oneFieldInfo.SetValue(newT, matchedField.GetValue(originalObject));
					}
				}
			}
			return newT;
		}
	}
}
