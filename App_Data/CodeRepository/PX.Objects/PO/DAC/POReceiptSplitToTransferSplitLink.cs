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
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.IN;

namespace PX.Objects.PO
{
	[PXCacheName(Messages.POReceiptSplitToTransferSplitLink, PXDacType.Details)]
	public class POReceiptSplitToTransferSplitLink : IBqlTable
	{
		#region Keys
		public class PK : PrimaryKeyOf<POReceiptSplitToTransferSplitLink>.By<receiptType, receiptNbr, receiptLineNbr, receiptSplitLineNbr, transferDocType, transferRefNbr, transferLineNbr, transferSplitLineNbr>
		{
			public static POReceiptSplitToTransferSplitLink Find(PXGraph graph, string receiptType, string receiptNbr, int? receiptLineNbr, int? receiptSplitLineNbr, 
				string transferDocType, string transferRefNbr, int? transferLineNbr, int? transferSplitLineNbr, PKFindOptions options = PKFindOptions.None)
				=> FindBy(graph, receiptType, receiptNbr, receiptLineNbr, receiptSplitLineNbr, transferDocType, transferRefNbr, transferLineNbr, transferSplitLineNbr, options);
		}
		public static class FK
		{
			public class Receipt : POReceipt.PK.ForeignKeyOf<POReceiptSplitToTransferSplitLink>.By<receiptType, receiptNbr> { }
			public class ReceiptLine : POReceiptLine.PK.ForeignKeyOf<POReceiptSplitToTransferSplitLink>.By<receiptType, receiptNbr, receiptLineNbr> { }
			public class ReceiptLineSplit : Objects.PO.POReceiptLineSplit.PK.ForeignKeyOf<POReceiptSplitToTransferSplitLink>.By<receiptType, receiptNbr, receiptLineNbr, receiptSplitLineNbr> { }
			public class Transfer : INRegister.PK.ForeignKeyOf<POReceiptSplitToTransferSplitLink>.By<transferDocType, transferRefNbr> { }
			public class TransferLine : IN.INTran.PK.ForeignKeyOf<POReceiptSplitToTransferSplitLink>.By<transferDocType, transferRefNbr, transferLineNbr> { }
			public class TransferLineSplit : IN.INTranSplit.PK.ForeignKeyOf<POReceiptSplitToTransferSplitLink>.By<transferDocType, transferRefNbr, transferLineNbr, transferSplitLineNbr> { }
		}
		#endregion

		#region ReceiptType
		[PXDBString(2, IsFixed = true, IsKey = true)]
		[PXDefault]
		public virtual string ReceiptType { get; set; }
		public abstract class receiptType : PX.Data.BQL.BqlString.Field<receiptType> { }
		#endregion
		#region ReceiptNbr
		[PXDBString(15, IsUnicode = true, IsKey = true, InputMask = "")]
		[PXDefault]
		public virtual String ReceiptNbr { get; set; }
		public abstract class receiptNbr : PX.Data.BQL.BqlString.Field<receiptNbr> { }
		#endregion
		#region ReceiptLineNbr
		[PXDBInt(IsKey = true)]
		[PXDefault]
		public virtual Int32? ReceiptLineNbr { get; set; }
		public abstract class receiptLineNbr : PX.Data.BQL.BqlInt.Field<receiptLineNbr> { }
		#endregion
		#region ReceiptSplitLineNbr
		[PXDBInt(IsKey = true)]
		[PXDefault]
		[PXParent(typeof(FK.ReceiptLineSplit))]
		public virtual Int32? ReceiptSplitLineNbr { get; set; }
		public abstract class receiptSplitLineNbr : PX.Data.BQL.BqlInt.Field<receiptSplitLineNbr> { }
		#endregion
		#region TransferDocType
		[PXDBString(1, IsFixed = true, IsKey = true)]
		public virtual String TransferDocType { get { return INDocType.Transfer; } set { } }
		public abstract class transferDocType : PX.Data.BQL.BqlString.Field<transferDocType> { }
		#endregion
		#region TransferRefNbr
		[PXDBString(15, IsUnicode = true, IsKey = true, InputMask = "")]
		[PXDefault]
		[PXSelector(typeof(
			Search<INRegister.refNbr,
			Where<INRegister.docType, Equal<INDocType.transfer>,
				And<INRegister.transferType, Equal<INTransferType.oneStep>>>>))]
		public virtual String TransferRefNbr { get; set; }
		public abstract class transferRefNbr : PX.Data.BQL.BqlString.Field<transferRefNbr> { }
		#endregion
		#region TransferLineNbr
		[PXDBInt(IsKey = true)]
		[PXDefault]
		public virtual Int32? TransferLineNbr { get; set; }
		public abstract class transferLineNbr : PX.Data.BQL.BqlInt.Field<transferLineNbr> { }
		#endregion
		#region TransferSplitLineNbr
		[PXDBInt(IsKey = true)]
		[PXDefault]
		[PXParent(typeof(FK.TransferLineSplit))]
		public virtual Int32? TransferSplitLineNbr { get; set; }
		public abstract class transferSplitLineNbr : PX.Data.BQL.BqlInt.Field<transferSplitLineNbr> { }
		#endregion
		#region Qty
		[PXDBQuantity]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Quantity", Enabled = false)]
		public virtual Decimal? Qty { get; set; }
		public abstract class qty : PX.Data.BQL.BqlDecimal.Field<qty> { }
		#endregion
	}
}
