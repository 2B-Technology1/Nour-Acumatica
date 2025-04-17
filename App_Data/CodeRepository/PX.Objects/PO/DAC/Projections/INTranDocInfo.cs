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
using PX.Data.BQL.Fluent;
using PX.Objects.IN;
using System;

namespace PX.Objects.PO
{
	/// <summary>
	/// Represents information about referenced documents from grouping inventory transactions.
	/// </summary>
	[Serializable]
	[PXProjection(typeof(
		SelectFrom<INTran>.
		AggregateTo<
			GroupBy<INTran.docType>,
			GroupBy<INTran.refNbr>,
			GroupBy<INTran.pOReceiptNbr>,
			GroupBy<INTran.pOReceiptLineNbr>,
			GroupBy<INTran.pOReceiptType>
			>))]
	[PXHidden]
	public partial class INTranDocInfo : IBqlTable
	{
		#region DocType
		/// <inheritdoc cref="INTran.docType"/>
		public abstract new class docType : PX.Data.BQL.BqlString.Field<docType> { }

		/// <inheritdoc cref="INTran.docType"/>
		[PXDBString(1, IsFixed = true, IsKey = true, BqlField = typeof(INTran.docType))]
		public virtual String DocType { get; set; }
		#endregion

		#region RefNbr
		/// <inheritdoc cref="INTran.refNbr"/>
		public abstract new class refNbr : PX.Data.BQL.BqlString.Field<refNbr> { }

		/// <inheritdoc cref="INTran.refNbr"/>
		[PXDBString(15, IsUnicode = true, IsKey = true, BqlField = typeof(INTran.refNbr))]
		public virtual String RefNbr { get; set; }
		#endregion

		#region POReceiptNbr
		/// <inheritdoc cref="INTran.pOReceiptNbr"/>
		public abstract new class pOReceiptNbr : PX.Data.BQL.BqlString.Field<pOReceiptNbr> { }

		/// <inheritdoc cref="INTran.pOReceiptNbr"/>
		[PXDBString(15, IsUnicode = true, IsKey = true, BqlField = typeof(INTran.pOReceiptNbr))]
		public virtual String POReceiptNbr { get; set; }
		#endregion

		#region POReceiptLineNbr
		/// <inheritdoc cref="INTran.pOReceiptLineNbr"/>
		public abstract new class pOReceiptLineNbr : PX.Data.BQL.BqlInt.Field<pOReceiptLineNbr> { }

		/// <inheritdoc cref="INTran.pOReceiptLineNbr"/>
		[PXDBInt(IsKey = true, BqlField = typeof(INTran.pOReceiptLineNbr))]
		public virtual Int32? POReceiptLineNbr { get; set; }
		#endregion

		#region POReceiptType
		/// <inheritdoc cref="INTran.pOReceiptType"/>
		public abstract new class pOReceiptType : PX.Data.BQL.BqlString.Field<pOReceiptType> { }

		/// <inheritdoc cref="INTran.pOReceiptType"/>
		[PXDBString(2, IsFixed = true, IsKey = true, BqlField = typeof(INTran.pOReceiptType))]
		public virtual String POReceiptType { get; set; }
		#endregion
	}
}
