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
using PX.Objects.CM.Extensions;
using PX.Objects.AP;
using PX.Objects.IN;
using PX.Objects.PO.LandedCosts.Attributes;

namespace PX.Objects.PO.LandedCosts
{
	[PXProjection(typeof(Select5<POLandedCostReceipt,
		InnerJoin<POLandedCostDoc, On<POLandedCostDoc.docType, Equal<POLandedCostReceipt.lCDocType>, And<POLandedCostDoc.refNbr, Equal<POLandedCostReceipt.lCRefNbr>>>,
		InnerJoin<POLandedCostDetail, On<POLandedCostDetail.docType, Equal<POLandedCostReceipt.lCDocType>, And<POLandedCostDetail.refNbr, Equal<POLandedCostReceipt.lCRefNbr>>>,
		InnerJoin<POLandedCostReceiptLine,
			On<POLandedCostReceiptLine.docType, Equal<POLandedCostReceipt.lCDocType>,
			And<POLandedCostReceiptLine.refNbr, Equal<POLandedCostReceipt.lCRefNbr>,
			And<POLandedCostReceiptLine.pOReceiptNbr, Equal<POLandedCostReceipt.pOReceiptNbr>>>>,
		InnerJoin<POLandedCostSplit,
			On<POLandedCostSplit.receiptLineNbr, Equal<POLandedCostReceiptLine.lineNbr>,
			And<POLandedCostSplit.detailLineNbr, Equal<POLandedCostDetail.lineNbr>,
			And<POLandedCostSplit.refNbr, Equal<POLandedCostReceipt.lCRefNbr>,
			And<POLandedCostSplit.docType, Equal<POLandedCostReceipt.lCDocType>>>>>>>>>,
		Aggregate<
			GroupBy<POLandedCostReceipt.lCDocType,
			GroupBy<POLandedCostReceipt.lCRefNbr,
			GroupBy<POLandedCostDetail.lineNbr,
			GroupBy<POLandedCostReceipt.pOReceiptType,
			GroupBy<POLandedCostReceipt.pOReceiptNbr,
			Sum<POLandedCostSplit.lineAmt,
			Sum<POLandedCostSplit.curyLineAmt>>>>>>>>>), Persistent = false)]
	[PXBreakInheritance]
	[PXCacheName(Messages.POLandedCostReceipt)]
	[Serializable]
	public class POReceiptLandedCostDetail : POLandedCostReceipt
	{
		#region LCDocType
		public new abstract class lCDocType : PX.Data.BQL.BqlString.Field<lCDocType>
		{
		}

		[POLandedCostDocType.List()]
		[PXDBString(1, IsKey = true, IsFixed = true)]
		[PXUIField(DisplayName = "Landed Cost Type", Visible = false)]
		public override String LCDocType
		{
			get;
			set;
		}
		#endregion
		#region LCRefNbr
		public new abstract class lCRefNbr : PX.Data.BQL.BqlString.Field<lCRefNbr>
		{
		}

		[PXDBString(15, IsUnicode = true, IsKey = true)]
		[PXUIField(DisplayName = "Landed Cost Nbr.")]
		[PXSelector(typeof(Search<POLandedCostDoc.refNbr, Where<POLandedCostDoc.docType, Equal<Current<lCDocType>>>>))]
		public override String LCRefNbr
		{
			get;
			set;
		}
		#endregion

		#region LineNbr
		public abstract class lineNbr : PX.Data.BQL.BqlInt.Field<lineNbr> { }

		/// <inheritdoc cref="POLandedCostDetail.LineNbr"/>
		[PXDBInt(IsKey = true, BqlField = typeof(POLandedCostDetail.lineNbr))]
		[PXUIField(DisplayName = "Line Nbr.", Visibility = PXUIVisibility.Visible, Visible = false)]
		public virtual Int32? LineNbr
		{
			get;
			set;
		}
		#endregion

		#region POReceiptType
		public new abstract class pOReceiptType : PX.Data.BQL.BqlString.Field<pOReceiptType> { }

		/// <summary>
		/// The type of the receipt document.
		/// </summary>
		[PXDBString(2, IsKey = true, IsFixed = true)]
		[PXUIField(DisplayName = "PO Receipt Type", Visible = false)]
		public override String POReceiptType
		{
			get;
			set;
		}
		#endregion
		#region POReceiptNbr
		public new abstract class pOReceiptNbr : PX.Data.BQL.BqlString.Field<pOReceiptNbr> { }

		/// <summary>
		/// The reference number of the receipt document.
		/// </summary>
		[PXDBString(15, IsUnicode = true, IsKey = true)]
		[PXUIField(DisplayName = "PO Receipt Nbr.")]
		public override String POReceiptNbr
		{
			get;
			set;
		}
		#endregion

		#region Status
		public abstract class status : PX.Data.BQL.BqlString.Field<status> { }

		/// <inheritdoc cref="POLandedCostDoc.Status"/>
		[PXDBString(1, IsFixed = true, BqlField = typeof(POLandedCostDoc.status))]
		[PXUIField(DisplayName = "Status", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
		[POLandedCostDocStatus.List]
		public virtual string Status
		{
			get;
			set;
		}
		#endregion

		#region DocDate
		public abstract class docDate : PX.Data.BQL.BqlDateTime.Field<docDate> { }

		/// <inheritdoc cref="POLandedCostDoc.DocDate"/>
		// Acuminator disable once PX1007 NoXmlCommentForPublicEntityOrDacProperty to be documented later [Justification: false alert, see ATR-741]
		[PXDBDate(BqlField = typeof(POLandedCostDoc.docDate))]
		[PXUIField(DisplayName = "Date", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual DateTime? DocDate
		{
			get;
			set;
		}
		#endregion

		#region VendorID
		public abstract class vendorID : PX.Data.BQL.BqlInt.Field<vendorID> { }

		/// <inheritdoc cref="POLandedCostDoc.VendorID"/>
		[Vendor(Visibility = PXUIVisibility.SelectorVisible, DescriptionField = typeof(Vendor.acctName), CacheGlobal = true, Filterable = true, BqlField = typeof(POLandedCostDoc.vendorID))]
		public virtual int? VendorID
		{
			get;
			set;
		}
		#endregion

		#region CuryID
		public abstract class curyID : PX.Data.BQL.BqlString.Field<curyID> { }

		/// <inheritdoc cref="POLandedCostDoc.CuryID"/>
		[PXDBString(5, IsUnicode = true, InputMask = ">LLLLL", BqlField = typeof(POLandedCostDoc.curyID))]
		[PXUIField(DisplayName = "Currency", Visibility = PXUIVisibility.SelectorVisible)]
		[PXSelector(typeof(Currency.curyID))]
		public virtual String CuryID
		{
			get;
			set;
		}
		#endregion

		#region CuryInfoID
		public abstract class curyInfoID : PX.Data.BQL.BqlLong.Field<curyInfoID> { }
		[PXDBLong(BqlField = typeof(POLandedCostDetail.curyInfoID))]
		public virtual Int64? CuryInfoID
		{
			get;
			set;
		}
		#endregion

		#region LandedCostCodeID
		public abstract class landedCostCodeID : PX.Data.BQL.BqlString.Field<landedCostCodeID> { }

		[PXDBString(15, IsUnicode = true, IsFixed = false, BqlField = typeof(POLandedCostDetail.landedCostCodeID))]
		[PXUIField(DisplayName = "Landed Cost Code")]
		[PXSelector(typeof(Search<LandedCostCode.landedCostCodeID>))]
		public virtual String LandedCostCodeID
		{
			get;
			set;
		}
		#endregion

		#region Descr
		public abstract class descr : PX.Data.BQL.BqlString.Field<descr> { }

		[PXDBString(150, IsUnicode = true, BqlField = typeof(POLandedCostDetail.descr))]
		[PXUIField(DisplayName = "Description", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual String Descr
		{
			get;
			set;
		}
		#endregion

		#region AllocationMethod
		public abstract class allocationMethod : PX.Data.BQL.BqlString.Field<allocationMethod> { }
		[PXDBString(1, IsFixed = true, BqlField = typeof(POLandedCostDetail.allocationMethod))]
		[LandedCostAllocationMethod.List()]
		[PXUIField(DisplayName = "Allocation Method", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
		public virtual String AllocationMethod
		{
			get;
			set;
		}
		#endregion

		#region CuryLineAmt
		public abstract class curyLineAmt : PX.Data.BQL.BqlDecimal.Field<curyLineAmt> { }

		[PXDBCurrency(typeof(curyInfoID), typeof(lineAmt), BqlField = typeof(POLandedCostSplit.curyLineAmt))]
		[PXUIField(DisplayName = "Amount")]
		public virtual Decimal? CuryLineAmt
		{
			get;
			set;
		}
		#endregion

		#region LineAmt
		public abstract class lineAmt : PX.Data.BQL.BqlDecimal.Field<lineAmt> { }

		[PXDBBaseCury(BqlField = typeof(POLandedCostSplit.lineAmt))]
		public virtual Decimal? LineAmt
		{
			get;
			set;
		}
		#endregion

		#region APDocType
		public abstract class aPDocType : PX.Data.BQL.BqlString.Field<aPDocType> { }

		[PXDBString(3, IsFixed = true, BqlField = typeof(POLandedCostDetail.aPDocType))]
		[PXUIField(DisplayName = "AP Doc. Type", Enabled = false)]
		[APDocType.List()]
		public virtual String APDocType
		{
			get;
			set;
		}
		#endregion
		#region APRefNbr
		public abstract class aPRefNbr : PX.Data.BQL.BqlString.Field<aPRefNbr> { }

		[PXDBString(15, IsUnicode = true, BqlField = typeof(POLandedCostDetail.aPRefNbr))]
		[PXUIField(DisplayName = "AP Ref. Nbr.", Enabled = false)]
		[PXSelector(typeof(Search<AP.APInvoice.refNbr, Where<AP.APInvoice.docType, Equal<Current<aPDocType>>>>))]
		public virtual String APRefNbr
		{
			get;
			set;
		}
		#endregion
		#region INDocType
		public abstract class iNDocType : PX.Data.BQL.BqlString.Field<iNDocType> { }

		[PXDBString(2, IsFixed = true, BqlField = typeof(POLandedCostDetail.iNDocType))]
		[PXUIField(DisplayName = "IN Doc. Type", Enabled = false)]
		[INDocType.List()]
		public virtual String INDocType
		{
			get;
			set;
		}
		#endregion
		#region INRefNbr
		public abstract class iNRefNbr : PX.Data.BQL.BqlString.Field<iNRefNbr> { }

		[PXDBString(15, IsUnicode = true, BqlField = typeof(POLandedCostDetail.iNRefNbr))]
		[PXUIField(DisplayName = "IN Ref. Nbr.", Enabled = false)]
		[PXSelector(typeof(Search<INRegister.refNbr, Where<INRegister.docType, Equal<Current<iNDocType>>>>))]
		public virtual String INRefNbr
		{
			get;
			set;
		}
		#endregion
	}
}
