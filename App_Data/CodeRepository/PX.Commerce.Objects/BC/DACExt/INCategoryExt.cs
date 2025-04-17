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
using PX.Objects.CR;
using PX.Data.EP;
using PX.Objects.IN;
using PX.SM;
using PX.Commerce.Core;

namespace PX.Commerce.Objects
{
	#region INCategoryExt
	/// <summary>
    	/// DAC extension of CSAttribute to add additional attributes.
    	/// </summary>
	[PXNonInstantiatedExtension]
	public sealed class BCINCategoryExt : PXCacheExtension<INCategory>
	{
		public static bool IsActive() { return CommerceFeaturesHelper.CommerceEdition; }

		#region CategoryID
		/// <summary>
        /// <inheritdoc cref="INCategory.CategoryID"/>
        /// </summary>
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXFieldDescription]
		public int? CategoryID { get; set; }
		/// <inheritdoc cref="CategoryID"/>
		public abstract class categoryID : PX.Data.BQL.BqlInt.Field<categoryID> { }
		#endregion

		#region Description
		/// <summary>
		/// <inheritdoc cref="INCategory.Description"/>
		/// </summary>
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXFieldDescription]
		public String Description { get; set; }
		/// <inheritdoc cref="Description"/>
		public abstract class description : PX.Data.BQL.BqlString.Field<description> { }
		#endregion
	}
	#endregion
}
