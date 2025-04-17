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
using PX.Data.BQL;
using PX.Objects.IN;

namespace PX.Objects.SO
{
	/// <exclude/>
	[PXHidden]
	public class SalesAllocationStatus: IBqlTable
	{
		#region SiteID
		[PXDBInt(IsKey = true)]
		public virtual int? SiteID { get; set; }
		public abstract class siteID : BqlInt.Field<siteID> { }
		#endregion

		#region InventoryID
		[PXDBInt(IsKey = true)]
		public virtual int? InventoryID { get; set; }
		public abstract class inventoryID : BqlInt.Field<inventoryID> { }
		#endregion

		#region AvailableQty
		[PXDBQuantity]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? AvailableQty { get; set; }
		public abstract class availableQty : BqlDecimal.Field<availableQty> { }
		#endregion

		#region AllocatedQty
		[PXDBQuantity]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? AllocatedQty { get; set; }
		public abstract class allocatedQty : BqlDecimal.Field<allocatedQty> { }
		#endregion

		#region BufferedQty
		[PXDBQuantity]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? BufferedQty { get; set; }
		public abstract class bufferedQty : BqlDecimal.Field<bufferedQty> { }
		#endregion

		#region AllocatedSelectedQty
		[PXDBQuantity]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? AllocatedSelectedQty { get; set; }
		public abstract class allocatedSelectedQty : BqlDecimal.Field<allocatedSelectedQty> { }
		#endregion

		#region UnallocatedQty
		[PXDBQuantity]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? UnallocatedQty { get; set; }
		public abstract class unallocatedQty : BqlDecimal.Field<unallocatedQty> { }
		#endregion

		#region IsDirty
		[PXDBBool]
		[PXDefault(false)]
		public virtual bool? IsDirty { get; set; }
		public abstract class isDirty : BqlBool.Field<isDirty> { }
		#endregion
	}
}
