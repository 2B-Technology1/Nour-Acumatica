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
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Data.SQLTree;
using PX.Objects.Common.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PX.Objects.PO
{
	[PXHidden]
	[PXProjection(typeof(
		SelectFrom<POReceiptLine>
		.Where<
			POReceiptLine.lineType.IsNotIn<POLineType.service, POLineType.freight>>
		.AggregateTo<
			GroupBy<POReceiptLine.receiptType,
			GroupBy<POReceiptLine.receiptNbr>>>))]
	public class POReceiptLinesCount: IBqlTable
	{
		#region Keys
		public class PK : PrimaryKeyOf<POReceiptLinesCount>.By<receiptType, receiptNbr>
		{
			public static POReceiptLinesCount Find(PXGraph graph, string receiptType, string receiptNbr, PKFindOptions options = PKFindOptions.None) => FindBy(graph, receiptType, receiptNbr, options);
		}
		public static class FK
		{
			public class Receipt : POReceipt.PK.ForeignKeyOf<POReceiptLinesCount>.By<receiptType, receiptNbr> { }
		}
		#endregion

		#region ReceiptType
		[PXDBString(POReceiptLine.receiptType.Length, IsFixed = true, IsKey = true, BqlField = typeof(POReceiptLine.receiptType))]
		public virtual string ReceiptType { get; set; }
		public abstract class receiptType : BqlString.Field<receiptType> { }
		#endregion

		#region ReceiptNbr
		[PXDBString(15, IsUnicode = true, IsKey = true, InputMask = "", BqlField = typeof(POReceiptLine.receiptNbr))]
		public virtual string ReceiptNbr { get; set; }
		public abstract class receiptNbr : BqlString.Field<receiptNbr> { }
		#endregion

		#region LinesCount
		[PXDBCount]
		public virtual int? LinesCount { get; set; }
		public abstract class linesCount : BqlInt.Field<linesCount> { }
		#endregion
	}
}
