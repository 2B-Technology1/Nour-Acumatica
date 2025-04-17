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

namespace PX.Commerce.Shopify.API.GraphQL
{
	/// <summary>
	/// The Attribute that using for GraphQL argument
	/// </summary>
	public class GraphQLArgumentAttribute: System.Attribute 
	{
		public virtual string AgrumentLabel { get; protected set; }

		public virtual string AgrumentType { get; protected set; }

		public virtual bool? AllowNull { get; protected set; }

		public virtual bool CostCalculationRequired { get; protected set; }

		public GraphQLArgumentAttribute(string agrumentLabel, string agrumentType, bool allowNull = true, bool costCalculationRequired = false)
		{
			AgrumentLabel = agrumentLabel;
			AgrumentType = agrumentType;
			AllowNull = allowNull;
			CostCalculationRequired = costCalculationRequired;
		}
	}
}
