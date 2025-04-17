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

namespace PX.Objects.IN
{
	[Serializable()]
	[PXHidden]
	public partial class InventoryQtyByPlanType : IBqlTable
	{
		#region PlanType
		public abstract class planType : PX.Data.BQL.BqlString.Field<planType>
        {
		}
		[PXString(IsKey = true)]
		[PXUIField(DisplayName = "Plan Type", Visibility = PXUIVisibility.SelectorVisible, Enabled = false, IsReadOnly = true)]
		public virtual String PlanType
		{
			get;
			set;
		}
		#endregion
		#region Qty
		public abstract class qty : PX.Data.BQL.BqlDecimal.Field<qty>
        {
		}
		[PXDecimal()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Quantity", Visibility = PXUIVisibility.SelectorVisible, Enabled = false, IsReadOnly = true)]
		public virtual Decimal? Qty
		{
			get;
			set;
		}
		#endregion
		#region Included
		public abstract class included : PX.Data.BQL.BqlBool.Field<included>
        {
		}
		[PXBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Included", Visibility = PXUIVisibility.SelectorVisible, Enabled = false, IsReadOnly = true)]
		public virtual Boolean? Included
		{
			get;
			set;
		}
		#endregion
		#region IsTotal
		public abstract class isTotal : PX.Data.BQL.BqlBool.Field<isTotal>
        {
		}
		[PXBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "IsTotal", Visibility = PXUIVisibility.SelectorVisible, Enabled = false, IsReadOnly = true, Visible = false)]
		public virtual Boolean? IsTotal
		{
			get;
			set;
		}
		#endregion
		#region SortOrder
		public abstract class sortOrder : PX.Data.BQL.BqlInt.Field<sortOrder>
        {
		}
		[PXInt()]
		[PXDefault(0)]
		public virtual int? SortOrder
		{
			get;
			set;
		}
		#endregion
	}
}
