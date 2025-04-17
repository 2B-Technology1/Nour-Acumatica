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
	public class DiscountApplicationGQLInterfaces
	{
		/// <summary>
		/// Types implemented in <see cref="DiscountApplicationGQL"/>.
		/// </summary>
		public enum InterfaceNames
		{
			ManualDiscountApplication,
			AutomaticDiscountApplication,
			DiscountCodeApplication,
			ScriptDiscountApplication
		}

		[GraphQLField("description", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.String, nameof(InterfaceNames.ManualDiscountApplication))]
		public string Description { get; set; }

		[GraphQLField("title", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.String, nameof(InterfaceNames.ManualDiscountApplication))]
		[GraphQLField("title", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.String, nameof(InterfaceNames.AutomaticDiscountApplication))]
		[GraphQLField("title", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.String, nameof(InterfaceNames.ScriptDiscountApplication))]
		public string Title { get; set; }

		[GraphQLField("code", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.String, nameof(InterfaceNames.DiscountCodeApplication))]
		public string Code { get; set; }
	}
}
