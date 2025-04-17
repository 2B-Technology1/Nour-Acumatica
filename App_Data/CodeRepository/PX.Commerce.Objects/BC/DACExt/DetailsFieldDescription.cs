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

using PX.Commerce.Core;
using PX.CS;
using PX.Data;
using PX.Data.EP;
using PX.Objects.AR;
using PX.Objects.SO;
using System;

namespace PX.Commerce.Objects
{
	/// <summary>
	/// DAC extension of CSAttribute to add additional attributes.
	/// </summary>
	public sealed class BCAttributeExt : PXCacheExtension<CSAttribute>
	{
		public static bool IsActive() { return CommerceFeaturesHelper.CommerceEdition; }
		#region Attribute ID
		/// <summary>
		/// <inheritdoc cref="CSAttribute.AttributeID"/>
		/// </summary>
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXFieldDescription]
		public string AttributeID { get; set; }
		/// <inheritdoc cref="AttributeID"/>
		public abstract class attributeID : PX.Data.BQL.BqlString.Field<attributeID> { }
		#endregion		
	}

	/// <summary>
	/// DAC extension of CSAttributeDetail to add additional attributes.
	/// </summary>
	public sealed class BCAttributeValueExt : PXCacheExtension<CSAttributeDetail>
	{
		public static bool IsActive() { return CommerceFeaturesHelper.CommerceEdition; }
		#region Value ID
		/// <summary>
		/// <inheritdoc cref="CSAttributeDetail.ValueID"/>
		/// </summary>
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXFieldDescription]
		public string ValueID { get; set; }
		/// <inheritdoc cref="ValueID"/>
		public abstract class valueID : PX.Data.BQL.BqlString.Field<valueID> { }
		#endregion
	}
}
