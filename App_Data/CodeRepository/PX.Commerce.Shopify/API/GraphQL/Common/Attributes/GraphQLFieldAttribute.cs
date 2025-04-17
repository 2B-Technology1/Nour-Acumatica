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
	/// The Attribute that using for the field in GraphQL data object
	/// </summary>
	[AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
	public class GraphQLFieldAttribute : System.Attribute
	{
		/// <summary>
		/// The field name in the external data object
		/// </summary>
		public virtual string FieldName { get; protected set; }

		/// <summary>
		/// The data type in the external data object
		/// The value should be one of the following value: Scalar, Object, Enum, Union, Connection
		/// </summary>
		public virtual string FieldDataType { get; protected set; }

		/// <summary>
		/// The basic data type if the FieldDataType is Scalar
		/// </summary>
		public virtual string FieldScalarType { get; protected set; }

		/// <summary>
		/// The specified data object name in the external platform if the FieldDataType is Union
		/// </summary>
		public virtual string UnionObjectType { get; set; }

		/// <summary>
		/// The type of the attached field
		/// </summary>
		public virtual Type AttachedFieldType { get; set; }

		/// <summary>
		/// The name of the attached field
		/// </summary>
		public virtual string AttachedFieldName { get; set; }

		public GraphQLFieldAttribute(string fieldName, string fieldDataType, string fieldScalarType, string unionObjectType = null)
		{
			FieldName = fieldName?.Trim();
			FieldDataType = fieldDataType?.Trim();
			FieldScalarType = fieldScalarType?.Trim();
			UnionObjectType = unionObjectType?.Trim();
		}

		public GraphQLFieldAttribute(string fieldName, string fieldDataType, Type fieldType, string unionObjectType = null)
		{
			FieldName = fieldName?.Trim();
			FieldDataType = fieldDataType?.Trim();
			AttachedFieldType = fieldType;
			UnionObjectType = unionObjectType?.Trim();
		}
	}
}
