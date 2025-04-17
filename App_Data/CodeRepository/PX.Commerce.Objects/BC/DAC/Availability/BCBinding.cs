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
using PX.Commerce.Core;

namespace PX.Commerce.Objects.Availability
{
	/// <summary>
	/// <inheritdoc cref="PX.Commerce.Core.BCBinding"/>
	/// </summary>
	[PXHidden]
	public class BCBinding : IBqlTable
	{
		#region BindingID
		/// <summary>
		/// <inheritdoc cref="PX.Commerce.Core.BCBinding.BindingID"/>
		/// </summary>
		[PXDBIdentity]
		[PXUIField(DisplayName = "Store", Visible = false)]
		public int? BindingID { get; set; }
		/// <inheritdoc cref="BindingID"/>
		public abstract class bindingID : PX.Data.BQL.BqlInt.Field<bindingID> { }
		#endregion
		#region IsActive
		/// <summary>
		/// <inheritdoc cref="PX.Commerce.Core.BCBinding.IsActive"/>
		/// </summary>
		[PXDBBool]
		[PXUIField(DisplayName = "Active")]
		[PXDefault(true)]
		public virtual bool? IsActive { get; set; }
		/// <inheritdoc cref="IsActive"/>
		public abstract class isActive : PX.Data.BQL.BqlBool.Field<isActive> { }
		#endregion
		#region ConnectorType
		/// <summary>
		/// <inheritdoc cref="PX.Commerce.Core.BCBinding.ConnectorType"/>
		/// </summary>
		[PXDBString(3, IsKey = true)]
		[PXUIField(DisplayName = "Connector", Enabled = false)]
		[BCConnectors]
		public virtual string ConnectorType { get; set; }
		/// <inheritdoc cref="ConnectorType"/>
		public abstract class connectorType : PX.Data.BQL.BqlString.Field<connectorType> { }
		#endregion
	}
}
