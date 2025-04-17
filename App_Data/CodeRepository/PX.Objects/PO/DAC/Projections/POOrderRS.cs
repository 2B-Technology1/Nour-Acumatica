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
using PX.Objects.CM.Extensions;
using PX.Objects.IN;
using System;

namespace PX.Objects.PO
{
	/// <summary>
	/// POOrder + Unbilled Service Items Projection
	/// </summary>
	[Serializable]
	[PXBreakInheritance]
	public partial class POOrderRS : POOrder
	{
		#region Selected
			public new abstract class selected : PX.Data.BQL.BqlBool.Field<selected>
		{
		}
		#endregion

		#region orderNbr
		public new abstract class orderNbr : PX.Data.BQL.BqlString.Field<orderNbr>
		{
		}
		#endregion
		#region CuryInfoID
		public new abstract class curyInfoID : PX.Data.BQL.BqlLong.Field<curyInfoID>
		{
		}
		#endregion
		#region CuryUnbilledOrderTotal
		public abstract new class curyUnbilledOrderTotal : PX.Data.BQL.BqlDecimal.Field<curyUnbilledOrderTotal>
		{
		}
		[PXDBCurrency(typeof(POOrderRS.curyInfoID), typeof(POOrderRS.unbilledOrderTotal))]
		[PXUIField(DisplayName = "Unbilled Amt.", Enabled = false)]
		public override decimal? CuryUnbilledOrderTotal
		{
			get;
			set;
		}
		#endregion
		#region UnbilledOrderTotal
		public abstract new class unbilledOrderTotal : PX.Data.BQL.BqlDecimal.Field<unbilledOrderTotal>
		{
		}
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public override decimal? UnbilledOrderTotal
		{
			get;
			set;
		}
		#endregion
		#region UnbilledOrderQty
		public abstract new class unbilledOrderQty : PX.Data.BQL.BqlDecimal.Field<unbilledOrderQty>
		{
		}
		[PXDBQuantity]
		[PXUIField(DisplayName = "Unbilled Qty.", Enabled = false)]
		public override decimal? UnbilledOrderQty
		{
			get;
			set;
		}
		#endregion
	}
}
