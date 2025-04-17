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
using PX.Data;
using PX.Objects.SO;
using System.Collections.Generic;
using PX.Commerce.Core;
using PX.Data.WorkflowAPI;
using PX.Data.EP;

namespace PX.Commerce.Objects
{
	/// <summary>
	/// DAC extension of SOLine to add additional attributes and properties.
	/// </summary>
	[Serializable]
	public sealed class BCSOLineExt : PXCacheExtension<SOLine>
	{
		public static bool IsActive() { return CommerceFeaturesHelper.CommerceEdition; }

		#region OrderType
		/// <summary>
		/// <inheritdoc cref="SOLine.OrderType"/>
		/// </summary>
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXFieldDescription]
		public String OrderType { get; set; }
		/// <inheritdoc cref="OrderType" />
		public abstract class orderType : PX.Data.BQL.BqlString.Field<orderType> { }

		#endregion
		#region OrderNbr
		/// <summary>
		/// <inheritdoc cref="SOLine.OrderNbr"/>
		/// </summary>
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXFieldDescription]
		public String OrderNbr { get; set; }
		/// <inheritdoc cref="OrderNbr" />
		public abstract class orderNbr : PX.Data.BQL.BqlString.Field<orderNbr> { }
		#endregion

		#region ExternalRef
		/// <summary>
		/// The external reference number for this sales order line.
		/// </summary>
		[PXDBString(64, IsUnicode = true)]
		[PXUIField(DisplayName = "External Ref.")]
		public string ExternalRef { get; set; }
		/// <inheritdoc cref="ExternalRef" />
		public abstract class externalRef : PX.Data.BQL.BqlString.Field<externalRef> { }
		#endregion

		#region AssociatedOrderLineNbr
		/// <summary>
		/// An optional associated order line number.
		/// </summary>
		[PXDBInt]
		[PXUIField(DisplayName = "Associated Order Line Nbr.", Visible = false, IsReadOnly = true)]
		public int? AssociatedOrderLineNbr { get; set; }
		/// <inheritdoc cref="AssociatedOrderLineNbr" />
		public abstract class associatedOrderLineNbr : PX.Data.BQL.BqlInt.Field<associatedOrderLineNbr> { }
		#endregion

		#region GiftMessage
		/// <summary>
		/// A gift message associated with this sales order line.
		/// </summary>
		[PXDBString(200, IsUnicode = true)]
		[PXUIField(DisplayName = "Gift Message", Visible = false)]
		public string GiftMessage { get; set; }
		/// <inheritdoc cref="GiftMessage" />
		public abstract class giftMessage : PX.Data.BQL.BqlString.Field<giftMessage> { }
		#endregion

		#region ExcludedFromExport
		/// <summary>
		/// Indicates if this sales order line should be excluded from export.
		/// </summary>
		[PXBool]
		[PXUIField(DisplayName = "Excluded from Export", Visible = false, IsReadOnly = true)]
		public bool? ExcludedFromExport { get; set; }
		/// <inheritdoc cref="ExcludedFromExport" />
		public abstract class excludedFromExport : PX.Data.BQL.BqlBool.Field<excludedFromExport> { }
		#endregion
	}
}
