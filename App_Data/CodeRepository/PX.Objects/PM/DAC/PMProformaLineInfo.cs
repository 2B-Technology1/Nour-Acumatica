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
using PX.Objects.IN;
using PX.Objects.PM;
using System;

namespace PX.Objects.PM
{
	/// <summary>
	/// Virtual Table used in Report
	/// </summary>
	/// 
	[PXCacheName(Messages.ProformaLineInfo)]
	public class PMProformaLineInfo : PX.Data.IBqlTable
	{
		#region RefNbr
		public abstract class refNbr : PX.Data.BQL.BqlString.Field<refNbr>
		{
		}
		/// <summary>
		/// Gets or sets Proforma Reference Number.
		/// </summary>
		[PXDBString(PMProforma.refNbr.Length, IsUnicode = true, IsKey = true, InputMask = ">CCCCCCCCCCCCCCC")]
		public virtual String RefNbr
		{
			get; set;
		}
		#endregion
		#region LineNbr
		public abstract class lineNbr : PX.Data.BQL.BqlInt.Field<lineNbr> { }
		/// <summary>
		/// Gets or sets Proforma Line Number 
		/// </summary>
		[PXDBInt(IsKey = true)]
		public virtual Int32? LineNbr
		{
			get;
			set;
		}
		#endregion

		#region PreviousQty
		public abstract class previousQty : PX.Data.BQL.BqlDecimal.Field<previousQty> { }

		/// <summary>
		/// Gets or sets PreviousQty
		/// </summary>
		[PXDBQuantity]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Previous Qty")]
		public Decimal? PreviousQty
		{
			get;
			set;
		}
		#endregion
		#region ChangeOrderAmountToDate
		public abstract class changeOrderAmountToDate : PX.Data.BQL.BqlDecimal.Field<changeOrderAmountToDate> { }

		/// <summary>
		/// Gets or sets ChangeOrder Amount to date
		/// </summary>
		[PXDBDecimal]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Change Order Amount To Date")]
		public virtual Decimal? ChangeOrderAmountToDate
		{
			get; set;
		}
		#endregion

		#region ChangeOrderQtyToDate
		public abstract class changeOrderQtyToDate : PX.Data.BQL.BqlDecimal.Field<changeOrderQtyToDate> { }
		/// <summary>
		/// Gets or sets ChangeOrder Quantity to date
		/// </summary>
		[PXDBDecimal]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Change Order Quantity To Date")]
		public virtual Decimal? ChangeOrderQtyToDate
		{
			get; set;
		}
		#endregion

		#region QuantityBaseCompleterdPct
		public abstract class quantityBaseCompleterdPct : PX.Data.BQL.BqlDecimal.Field<quantityBaseCompleterdPct> { }
		/// <summary>
		/// Gets or sets completed percent for lines with Quantity progress billing base
		/// </summary>
		[PXDecimal]
		[PXUnboundDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? QuantityBaseCompleterdPct
		{
			get; set;
		}
		#endregion
	}
}
