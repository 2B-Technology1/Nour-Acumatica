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
using PX.Data.EP;
using PX.Objects.CM.Extensions;
using PX.Objects.GL;
using PX.Objects.AR;
using PX.Objects.CS;

namespace PX.Objects.TX
{
	public static class SVATCalculation
	{
		public static void FillAmounts(this SVATConversionHist histSVAT, CurrencyInfo currencyInfo, decimal? curyTaxableAmt, decimal? curyTaxAmount, decimal mult)
		{
			decimal taxableAmt = currencyInfo.RoundCury(curyTaxableAmt * mult ?? 0m);
			decimal taxAmt = currencyInfo.RoundCury(curyTaxAmount * mult ?? 0m);

			histSVAT.CuryTaxableAmt = taxableAmt;
			histSVAT.TaxableAmt = currencyInfo.CuryConvBase(taxableAmt);

			histSVAT.CuryTaxAmt = taxAmt;
			histSVAT.TaxAmt = currencyInfo.CuryConvBase(taxAmt);

			histSVAT.CuryUnrecognizedTaxAmt = taxAmt;
			histSVAT.UnrecognizedTaxAmt = currencyInfo.CuryConvBase(taxAmt);
		}


		public static void FillBaseAmounts(this SVATConversionHist histSVAT, PXCache cache)
		{
			decimal taxableAmt;
			decimal taxAmt;
			decimal unrecognizedTaxAmt;

			CM.PXDBCurrencyAttribute.CuryConvBase<SVATConversionHist.curyInfoID>(cache, histSVAT, histSVAT.CuryTaxableAmt ?? 0m, out taxableAmt);
			CM.PXDBCurrencyAttribute.CuryConvBase<SVATConversionHist.curyInfoID>(cache, histSVAT, histSVAT.CuryTaxAmt ?? 0m, out taxAmt);
			CM.PXDBCurrencyAttribute.CuryConvBase<SVATConversionHist.curyInfoID>(cache, histSVAT, histSVAT.CuryUnrecognizedTaxAmt ?? 0m, out unrecognizedTaxAmt);

			histSVAT.TaxableAmt = taxableAmt;
			histSVAT.TaxAmt = taxAmt;
			histSVAT.UnrecognizedTaxAmt = unrecognizedTaxAmt;
		}
	}

	[Serializable]
	[PXCacheName(Messages.SVATConversionHist)]
	public partial class SVATConversionHist : IBqlTable
	{
		#region Keys
		public class PK : PrimaryKeyOf<SVATConversionHist>.By<module, adjdDocType, adjdRefNbr, adjdLineNbr, adjgDocType, adjgRefNbr, adjNbr, taxID>
		{
			public static SVATConversionHist Find(PXGraph graph, string module, string adjdDocType, string adjdRefNbr, int? adjdLineNbr, string adjgDocType, string adjgRefNbr, int? adjNbr, int? taxID, PKFindOptions options = PKFindOptions.None) 
				=> FindBy(graph, module, adjdDocType, adjdRefNbr, adjdLineNbr, adjgDocType, adjgRefNbr, adjNbr, taxID, options);
		}
		public static class FK
		{
			public class AdjdBranch : GL.Branch.PK.ForeignKeyOf<SVATConversionHist>.By<adjdBranchID> { }

			public class Tax : TX.Tax.PK.ForeignKeyOf<SVATConversionHist>.By<taxID> { }
			public class Vendor : AP.Vendor.PK.ForeignKeyOf<SVATConversionHist>.By<vendorID> { }
			public class CurrencyInfo : CM.CurrencyInfo.PK.ForeignKeyOf<SVATConversionHist>.By<curyInfoID> { }
		}
		#endregion

		#region Selected
		public abstract class selected : PX.Data.BQL.BqlBool.Field<selected> { }
		[PXBool]
		[PXUIField(DisplayName = "Selected")]
		public virtual bool? Selected
		{
			get;
			set;
		}
		#endregion
		#region Module
		public abstract class module : PX.Data.BQL.BqlString.Field<module> { }
		[PXDBString(2, IsKey = true, IsFixed = true)]
		[PXDefault]
		[PXUIField(DisplayName = "Module")]
		[BatchModule.List]
		[PXFieldDescription]
		public virtual string Module
		{
			get;
			set;
		}
		#endregion
		#region AdjdBranchID
		public abstract class adjdBranchID : PX.Data.BQL.BqlInt.Field<adjdBranchID> { }
		[Branch]
		public virtual int? AdjdBranchID
		{
			get;
			set;
		}
		#endregion
		#region AdjdDocType
		public abstract class adjdDocType : PX.Data.BQL.BqlString.Field<adjdDocType> { }
		[PXDBString(3, IsKey = true, IsFixed = true)]
		[PXDefault]
		[PXUIField(DisplayName = "Type")]
		public virtual string AdjdDocType
		{
			get;
			set;
		}
		#endregion
		#region AdjdRefNbr
		public abstract class adjdRefNbr : PX.Data.BQL.BqlString.Field<adjdRefNbr> { }
		[PXDBString(15, IsUnicode = true, IsKey = true)]
		[PXDefault]
		[PXUIField(DisplayName = "Reference Nbr.")]
		public virtual string AdjdRefNbr
		{
			get;
			set;
		}
		#endregion
		#region AdjdLineNbr
		public abstract class adjdLineNbr : PX.Data.BQL.BqlInt.Field<adjdLineNbr> { }

		[PXDBInt(IsKey = true)]
		[PXUIField(DisplayName = "Line Nbr.")]
		[PXDefault(0)]
		public virtual int? AdjdLineNbr
		{
			get;
			set;
		}
		#endregion
		#region AdjgDocType
		public abstract class adjgDocType : PX.Data.BQL.BqlString.Field<adjgDocType> { }
		[PXDBString(3, IsKey = true, IsFixed = true)]
		[PXDefault(typeof(SVATConversionHist.adjdDocType))]
		[PXUIField(DisplayName = "AdjgDocType")]
		public virtual string AdjgDocType
		{
			get;
			set;
		}
		#endregion
		#region AdjgRefNbr
		public abstract class adjgRefNbr : PX.Data.BQL.BqlString.Field<adjgRefNbr> { }
		[PXDBString(15, IsUnicode = true, IsKey = true)]
		[PXDefault(typeof(SVATConversionHist.adjdRefNbr))]
		[PXUIField(DisplayName = "AdjgRefNbr")]
		public virtual string AdjgRefNbr
		{
			get;
			set;
		}
		#endregion
		#region AdjNbr
		public abstract class adjNbr : PX.Data.BQL.BqlInt.Field<adjNbr> { }
		[PXDBInt(IsKey = true)]
		[PXDefault(-1)]
		[PXUIField(DisplayName = "Adjustment Nbr.")]
		public virtual int? AdjNbr
		{
			get;
			set;
		}
		#endregion
		#region AdjdDocDate
		public abstract class adjdDocDate : PX.Data.BQL.BqlDateTime.Field<adjdDocDate> { }
		[PXDBDate]
		[PXDefault(typeof(AccessInfo.businessDate))]
		[PXUIField(DisplayName = "Date")]
		public virtual DateTime? AdjdDocDate
		{
			get;
			set;
		}
		#endregion
		#region AdjdFinPeriodID
		public abstract class adjdFinPeriodID : PX.Data.BQL.BqlString.Field<adjdFinPeriodID> { }
		[FinPeriodID(
		    branchSourceType: typeof(SVATConversionHist.adjdBranchID),
		    masterFinPeriodIDType: typeof(SVATConversionHist.adjdTranPeriodID))]
		[PXDefault]
		[PXUIField(DisplayName = "Post Period")]
		public virtual string AdjdFinPeriodID
		{
			get;
			set;
		}
		#endregion
        #region AdjdTranPeriodID
        public abstract class adjdTranPeriodID : IBqlField
	    {
	    }

        [PeriodID]
	    public virtual string AdjdTranPeriodID { get; set; }
	    #endregion
		#region AdjgFinPeriodID
		public abstract class adjgFinPeriodID : PX.Data.BQL.BqlString.Field<adjgFinPeriodID> { }
	    [FinPeriodID(
	        branchSourceType: typeof(SVATConversionHist.adjdBranchID),
	        masterFinPeriodIDType: typeof(SVATConversionHist.adjgTranPeriodID))]
		[PXUIField(DisplayName = "Application Period")]
		public virtual string AdjgFinPeriodID
		{
			get;
			set;
		}
		#endregion
	    #region AdjgTranPeriodID
	    public abstract class adjgTranPeriodID : IBqlField
	    {
	    }

	    [PeriodID]
	    public virtual string AdjgTranPeriodID { get; set; }
	    #endregion
		#region AdjBatchNbr
		public abstract class adjBatchNbr : PX.Data.BQL.BqlString.Field<adjBatchNbr> { }
		[PXDBString(15, IsUnicode = true)]
		[PXSelector(typeof(Search<Batch.batchNbr,
			Where<Batch.module, Equal<BatchModule.moduleGL>, And<Batch.draft, Equal<False>>>, OrderBy<Desc<Batch.batchNbr>>>))]
		[PXUIField(DisplayName = "Batch Number")]
		public virtual string AdjBatchNbr
		{
			get;
			set;
		}
		#endregion
		#region TaxRecordID
		public abstract class taxRecordID : PX.Data.BQL.BqlInt.Field<taxRecordID> { }
		[PXDBInt]
		public virtual int? TaxRecordID
		{
			get;
			set;
		}
		#endregion

		#region TaxID
		public abstract class taxID : PX.Data.BQL.BqlString.Field<taxID> { }
		[PXDBString(Tax.taxID.Length, IsUnicode = true, IsKey = true)]
		[PXUIField(DisplayName = "Tax ID")]
		public virtual string TaxID
		{
			get;
			set;
		}
		#endregion
		#region TaxType
		public abstract class taxType : PX.Data.BQL.BqlString.Field<taxType> { }
		[PXDBString(1, IsFixed = true)]
		[PXDefault]
		public virtual string TaxType
		{
			get;
			set;
		}
		#endregion
		#region TaxRate
		public abstract class taxRate : PX.Data.BQL.BqlDecimal.Field<taxRate> { }
		[PXDBDecimal(6)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Tax Rate")]
		public virtual decimal? TaxRate
		{
			get;
			set;
		}
		#endregion
		#region VendorID
		public abstract class vendorID : PX.Data.BQL.BqlInt.Field<vendorID> { }
		[PXDBInt]
		public virtual int? VendorID
		{
			get;
			set;
		}
		#endregion
		#region TaxInvoiceNbr
		public abstract class taxInvoiceNbr : PX.Data.BQL.BqlString.Field<taxInvoiceNbr> { }
		[PXDBString(15, IsUnicode = true)]
		[PXUIField(DisplayName = "Tax Doc. Nbr")]
		public virtual string TaxInvoiceNbr
		{
			get;
			set;
		}
		#endregion
		#region TaxInvoiceDate
		public abstract class taxInvoiceDate : PX.Data.BQL.BqlDateTime.Field<taxInvoiceDate> { }
		[PXDBDate(InputMask = "d", DisplayMask = "d")]
		[PXUIField(DisplayName = "Tax Doc. Date")]
		public virtual DateTime? TaxInvoiceDate
		{
			get;
			set;
		}
		#endregion
		#region ReversalMethod
		public abstract class reversalMethod : PX.Data.BQL.BqlString.Field<reversalMethod> { }
		[PXDBString(1, IsFixed = true)]
		[PXDefault(SVATTaxReversalMethods.OnDocuments)]
		[SVATTaxReversalMethods.List]
		[PXUIField(DisplayName = "VAT Recognition Method")]
		public virtual string ReversalMethod
		{
			get;
			set;
		}
		#endregion

		#region CuryInfoID
		public abstract class curyInfoID : PX.Data.BQL.BqlLong.Field<curyInfoID> { }
		[PXDBLong]
		[CurrencyInfo]
		public virtual long? CuryInfoID
		{
			get;
			set;
		}
		#endregion
		#region CuryTaxableAmt
		public abstract class curyTaxableAmt : PX.Data.BQL.BqlDecimal.Field<curyTaxableAmt> { }
		[PXDBCurrency(typeof(SVATConversionHist.curyInfoID), typeof(SVATConversionHist.taxableAmt), BaseCalc = false)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Taxable Amount")]
		public virtual decimal? CuryTaxableAmt
		{
			get;
			set;
		}
		#endregion
		#region TaxableAmt
		public abstract class taxableAmt : PX.Data.BQL.BqlDecimal.Field<taxableAmt> { }
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Taxable Amount")]
		public virtual decimal? TaxableAmt
		{
			get;
			set;
		}
		#endregion
		#region CuryTaxAmt
		public abstract class curyTaxAmt : PX.Data.BQL.BqlDecimal.Field<curyTaxAmt> { }
		[PXDBCurrency(typeof(SVATConversionHist.curyInfoID), typeof(SVATConversionHist.taxAmt), BaseCalc = false)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Tax Amount")]
		public virtual decimal? CuryTaxAmt
		{
			get;
			set;
		}
		#endregion
		#region TaxAmt
		public abstract class taxAmt : PX.Data.BQL.BqlDecimal.Field<taxAmt> { }
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Tax Amount")]
		public virtual decimal? TaxAmt
		{
			get;
			set;
		}
		#endregion
		#region CuryUnrecognizedTaxAmt
		public abstract class curyUnrecognizedTaxAmt : PX.Data.BQL.BqlDecimal.Field<curyUnrecognizedTaxAmt> { }
		[PXDBCurrency(typeof(SVATConversionHist.curyInfoID), typeof(SVATConversionHist.unrecognizedTaxAmt), BaseCalc = false)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Unrecognized VAT")]
		public virtual decimal? CuryUnrecognizedTaxAmt
		{
			get;
			set;
		}
		#endregion
		#region UnrecognizedTaxAmt
		public abstract class unrecognizedTaxAmt : PX.Data.BQL.BqlDecimal.Field<unrecognizedTaxAmt> { }
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Unrecognized VAT")]
		public virtual decimal? UnrecognizedTaxAmt
		{
			get;
			set;
		}
		#endregion

		#region Processed
		public abstract class processed : PX.Data.BQL.BqlBool.Field<processed> { }
		[PXDBBool]
		[PXDefault(false)]
		public virtual bool? Processed
		{
			get;
			set;
		}
		#endregion

		#region tstamp
		public abstract class Tstamp : PX.Data.BQL.BqlByteArray.Field<Tstamp> { }
		[PXDBTimestamp]
		public virtual byte[] tstamp
		{
			get;
			set;
		}
		#endregion
		#region CreatedByID
		public abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID> { }
		[PXDBCreatedByID]
		public virtual Guid? CreatedByID
		{
			get;
			set;
		}
		#endregion
		#region CreatedByScreenID
		public abstract class createdByScreenID : PX.Data.BQL.BqlString.Field<createdByScreenID> { }
		[PXDBCreatedByScreenID]
		public virtual string CreatedByScreenID
		{
			get;
			set;
		}
		#endregion
		#region CreatedDateTime
		public abstract class createdDateTime : PX.Data.BQL.BqlDateTime.Field<createdDateTime> { }
		[PXDBCreatedDateTime]
		public virtual DateTime? CreatedDateTime
		{
			get;
			set;
		}
		#endregion
		#region LastModifiedByID
		public abstract class lastModifiedByID : PX.Data.BQL.BqlGuid.Field<lastModifiedByID> { }
		[PXDBLastModifiedByID]
		public virtual Guid? LastModifiedByID
		{
			get;
			set;
		}
		#endregion
		#region LastModifiedByScreenID
		public abstract class lastModifiedByScreenID : PX.Data.BQL.BqlString.Field<lastModifiedByScreenID> { }
		[PXDBLastModifiedByScreenID]
		public virtual string LastModifiedByScreenID
		{
			get;
			set;
		}
		#endregion
		#region LastModifiedDateTime
		public abstract class lastModifiedDateTime : PX.Data.BQL.BqlDateTime.Field<lastModifiedDateTime> { }
		[PXDBLastModifiedDateTime]
		public virtual DateTime? LastModifiedDateTime
		{
			get;
			set;
		}
		#endregion

		#region CuryTaxableAmtBalance
		public abstract class curyTaxableAmtBalance : PX.Data.BQL.BqlDecimal.Field<curyTaxableAmtBalance> { }
		/// <summary>
		/// Signed <see cref="CuryTaxableAmt"/>
		/// </summary>
		[PXDecimal()]
		[PXDependsOnFields(typeof(SVATConversionHist.adjgDocType))]
		[PXDBCalced(typeof(Mult<SVATConversionHist.curyTaxableAmt,
				Switch<Case<Where<SVATConversionHist.adjgDocType.IsEqual<ARDocType.invoice>>,
					decimal_1,
					Case<Where<SVATConversionHist.adjgDocType.IsIn<ARDocType.payment, ARDocType.prepayment>>,
					decimal1>>,
					decimal0>>), typeof(decimal))]
		public virtual decimal? CuryTaxableAmtBalance { get; set; }
		#endregion
		#region TaxableAmtBalance
		public abstract class taxableAmtBalance : PX.Data.BQL.BqlDecimal.Field<taxableAmtBalance> { }
		/// <summary>
		/// Signed <see cref="TaxableAmt"/>
		/// </summary>
		[PXDecimal()]
		[PXDependsOnFields(typeof(SVATConversionHist.adjgDocType))]
		[PXDBCalced(typeof(Mult<SVATConversionHist.taxableAmt,
				Switch<Case<Where<SVATConversionHist.adjgDocType.IsEqual<ARDocType.invoice>>,
					decimal_1,
					Case<Where<SVATConversionHist.adjgDocType.IsIn<ARDocType.payment, ARDocType.prepayment>>,
					decimal1>>,
					decimal0>>), typeof(decimal))]
		public virtual decimal? TaxableAmtBalance { get; set; }
		#endregion

		#region CuryTaxAmtBalance
		public abstract class curyTaxAmtBalance : PX.Data.BQL.BqlDecimal.Field<curyTaxAmtBalance> { }
		/// <summary>
		/// Signed <see cref="CuryTaxAmt"/>
		/// </summary>
		[PXDecimal()]
		[PXDependsOnFields(typeof(SVATConversionHist.adjgDocType))]
		[PXDBCalced(typeof(Mult<SVATConversionHist.curyTaxAmt,
				Switch<Case<Where<SVATConversionHist.adjgDocType.IsEqual<ARDocType.invoice>>,
					decimal_1,
					Case<Where<SVATConversionHist.adjgDocType.IsIn<ARDocType.payment, ARDocType.prepayment>>,
					decimal1>>,
					decimal0>>), typeof(decimal))]
		public virtual decimal? CuryTaxAmtBalance { get; set; }
		#endregion
		#region TaxAmtBalance
		public abstract class taxAmtBalance : PX.Data.BQL.BqlDecimal.Field<taxAmtBalance> { }
		/// <summary>
		/// Signed <see cref="TaxAmt"/>
		/// </summary>
		[PXDecimal()]
		[PXDependsOnFields(typeof(SVATConversionHist.adjgDocType))]
		[PXDBCalced(typeof(Mult<SVATConversionHist.taxAmt,
				Switch<Case<Where<SVATConversionHist.adjgDocType.IsEqual<ARDocType.invoice>>,
					decimal_1,
					Case<Where<SVATConversionHist.adjgDocType.IsIn<ARDocType.payment, ARDocType.prepayment>>,
					decimal1>>,
					decimal0>>), typeof(decimal))]
		public virtual decimal? TaxAmtBalance { get; set; }
		#endregion
	}
}
