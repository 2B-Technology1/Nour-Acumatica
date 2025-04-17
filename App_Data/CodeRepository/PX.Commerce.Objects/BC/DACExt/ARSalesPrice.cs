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

using PX.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Data.EP;
using PX.Objects.AR;
using PX.Commerce.Core;

namespace PX.Commerce.Objects
{
	#region BCARPriceClassExt
	/// <summary>
	/// DAC Extension of ARSalesPrice to add additional attributes.
	/// </summary>
	[PXNonInstantiatedExtension]
	public sealed class BCARSalesPriceExt : PXCacheExtension<ARSalesPrice>
	{
		public static bool IsActive() { return CommerceFeaturesHelper.CommerceEdition; }

		#region PriceType
		/// <summary>
		/// <inheritdoc cref="ARSalesPrice.PriceType"/>
		/// </summary>
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXFieldDescription]
		public String PriceType { get; set; }
		/// <inheritdoc cref="PriceType"/>
		public abstract class priceType : PX.Data.BQL.BqlString.Field<priceType> { }
		#endregion
		#region PriceCode
		/// <summary>
		/// <inheritdoc cref="ARSalesPrice.PriceCode"/>
		/// </summary>
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXFieldDescription]
		public String PriceCode { get; set; }
		/// <inheritdoc cref="PriceCode"/>
		public abstract class priceCode : PX.Data.BQL.BqlString.Field<priceCode> { }
		#endregion
		#region CustPriceClassID
		/// <summary>
		/// <inheritdoc cref="ARSalesPrice.CustPriceClassID"/>
		/// </summary>
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXFieldDescription]
		public String CustPriceClassID { get; set; }
		/// <inheritdoc cref="CustPriceClassID"/>
		public abstract class custPriceClassID : PX.Data.BQL.BqlString.Field<custPriceClassID> { }
		#endregion
		#region CustomerID
		/// <summary>
		/// <inheritdoc cref="ARSalesPrice.CustomerID"/>
		/// </summary>
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXFieldDescription]
		public Int32? CustomerID { get; set; }
		/// <inheritdoc cref="CustomerID"/>
		public abstract class customerID : PX.Data.BQL.BqlInt.Field<customerID> { }
		#endregion
	}
	#endregion
}
