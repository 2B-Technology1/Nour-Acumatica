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

namespace PX.Objects.AR
{
	using System;
	using PX.Data;
	using PX.Objects.CM.Extensions;
	using PX.Objects.TX;
	using PX.Objects.CS;
	using PX.Data.ReferentialIntegrity.Attributes;

	/// <summary>
	/// A line-level tax detail of an accounts receivable document.
	/// The entities of this type cannot be edited directly. Instead,
	/// <see cref="TaxBaseAttribute"/> descendants aggregate them to 
	/// <see cref="ARTaxTran"/> records, which can be edited on the Invoices 
	/// and Memos (AR301000) and Cash Sales (AR304000) forms (corresponding
	/// to the <see cref="ARInvoiceEntry"/> and <see cref="ARCashSaleEntry"/> 
	/// graphs, respectively).
	/// </summary>
	[System.SerializableAttribute()]
	[PXCacheName(Messages.ARTax)]
	public partial class ARTax : TaxDetail, PX.Data.IBqlTable, CM.ITranTax, ITaxDetailWithLineNbr
	{
		#region Keys
		public class PK : PrimaryKeyOf<ARTax>.By<tranType, refNbr, lineNbr, taxID>
		{
			public static ARTax Find(PXGraph graph, String tranType, String refNbr, Int32? lineNbr, String taxID, PKFindOptions options = PKFindOptions.None) => FindBy(graph, tranType, refNbr, lineNbr, taxID, options);
		}
		public static class FK
		{
			public class Document : AP.APRegister.PK.ForeignKeyOf<ARTax>.By<tranType, refNbr> { }
			public class DocumentLine : AP.APTran.PK.ForeignKeyOf<ARTax>.By<tranType, refNbr, lineNbr> { }
			public class CurrencyInfo : CM.CurrencyInfo.PK.ForeignKeyOf<ARTax>.By<curyInfoID> { }
			public class Tax : TX.Tax.PK.ForeignKeyOf<ARTax>.By<taxID> { }
		}
		#endregion

		#region TranType
		public abstract class tranType : PX.Data.BQL.BqlString.Field<tranType> { }
		protected String _TranType;
		[PXDBString(3, IsKey = true, IsFixed = true)]
		[PXDBDefault(typeof(ARRegister.docType))]
		[PXUIField(DisplayName = "Tran. Type", Visibility = PXUIVisibility.Visible, Visible = false)]
		public virtual String TranType
		{
			get
			{
				return this._TranType;
			}
			set
			{
				this._TranType = value;
			}
		}
		#endregion
		#region RefNbr
		public abstract class refNbr : PX.Data.BQL.BqlString.Field<refNbr> { }
		protected String _RefNbr;
		[PXDBString(15, IsUnicode = true, IsKey = true)]
		[PXDBDefault(typeof(ARRegister.refNbr))]
		[PXUIField(DisplayName = "Reference Nbr.", Visibility = PXUIVisibility.Visible, Visible = false)]
		public virtual String RefNbr
		{
			get
			{
				return this._RefNbr;
			}
			set
			{
				this._RefNbr = value;
			}
		}
		#endregion
		#region LineNbr
		public abstract class lineNbr : PX.Data.BQL.BqlInt.Field<lineNbr> { }
		protected Int32? _LineNbr;
		[PXDBInt(IsKey = true)]
		[PXDefault()]
		[PXUIField(DisplayName = "Line Nbr.", Visibility = PXUIVisibility.Visible, Visible = false)]
		[PXParent(typeof(Select<ARRegister, Where<ARRegister.docType, Equal<Current<ARTax.tranType>>, And<ARRegister.refNbr, Equal<Current<ARTax.refNbr>>>>>))]
		[PXParent(typeof(Select<ARTran, Where<ARTran.tranType, Equal<Current<ARTax.tranType>>, And<ARTran.refNbr, Equal<Current<ARTax.refNbr>>, And<ARTran.lineNbr, Equal<Current<ARTax.lineNbr>>>>>>))]
		public virtual Int32? LineNbr
		{
			get
			{
				return this._LineNbr;
			}
			set
			{
				this._LineNbr = value;
			}
		}
		#endregion
		#region TaxID
		public abstract class taxID : PX.Data.BQL.BqlString.Field<taxID> { }
		[PXDBString(Tax.taxID.Length, IsUnicode = true, IsKey = true)]
		[PXDefault()]
		[PXUIField(DisplayName = "Tax ID")]
		[PXSelector(typeof(Tax.taxID), DescriptionField = typeof(Tax.descr))]
		public override String TaxID
		{
			get
			{
				return this._TaxID;
			}
			set
			{
				this._TaxID = value;
			}
		}
		#endregion
		#region TaxRate
		public abstract class taxRate : PX.Data.BQL.BqlDecimal.Field<taxRate> { }
		#endregion
		#region CuryInfoID
		public abstract class curyInfoID : PX.Data.BQL.BqlLong.Field<curyInfoID> { }
		[PXDBLong()]
		[CurrencyInfo(typeof(ARRegister.curyInfoID), Required = true)]
		public override Int64? CuryInfoID
		{
			get
			{
				return this._CuryInfoID;
			}
			set
			{
				this._CuryInfoID = value;
			}
		}
		#endregion
		#region CuryOrigTaxableAmt
		public abstract class curyOrigTaxableAmt : PX.Data.BQL.BqlDecimal.Field<curyOrigTaxableAmt> { }

		[PXDBCurrency(typeof(ARTax.curyInfoID), typeof(ARTax.origTaxableAmt))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? CuryOrigTaxableAmt { get; set; }
		#endregion
		#region OrigTaxableAmt
		public abstract class origTaxableAmt : PX.Data.BQL.BqlDecimal.Field<origTaxableAmt> { }

		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? OrigTaxableAmt { get; set; }
		#endregion
		#region CuryTaxableAmt
		public abstract class curyTaxableAmt : PX.Data.BQL.BqlDecimal.Field<curyTaxableAmt> { }
		protected Decimal? _CuryTaxableAmt;
		[PXDBCurrency(typeof(ARTax.curyInfoID), typeof(ARTax.taxableAmt))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Taxable Amount", Visibility = PXUIVisibility.Visible)]
		public virtual Decimal? CuryTaxableAmt
		{
			get
			{
				return this._CuryTaxableAmt;
			}
			set
			{
				this._CuryTaxableAmt = value;
			}
		}
		#endregion
		#region TaxableAmt
		public abstract class taxableAmt : PX.Data.BQL.BqlDecimal.Field<taxableAmt> { }
		protected Decimal? _TaxableAmt;
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Taxable Amount", Visibility = PXUIVisibility.Visible)]
		public virtual Decimal? TaxableAmt
		{
			get
			{
				return this._TaxableAmt;
			}
			set
			{
				this._TaxableAmt = value;
			}
		}
		#endregion
		#region CuryTaxableDiscountAmt
		public abstract class curyTaxableDiscountAmt : PX.Data.BQL.BqlDecimal.Field<curyTaxableDiscountAmt> { }
		[PXDBCurrency(typeof(curyInfoID), typeof(taxableDiscountAmt))]
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual decimal? CuryTaxableDiscountAmt
		{
			get;
			set;
		}
		#endregion
		#region TaxableDiscountAmt
		public abstract class taxableDiscountAmt : PX.Data.BQL.BqlDecimal.Field<taxableDiscountAmt> { }
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual decimal? TaxableDiscountAmt
		{
			get;
			set;
		}
		#endregion
		#region CuryTaxDiscountAmt
		public abstract class curyTaxDiscountAmt : PX.Data.BQL.BqlDecimal.Field<curyTaxDiscountAmt> { }
		[PXCurrency(typeof(curyInfoID), typeof(taxDiscountAmt))]
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual decimal? CuryTaxDiscountAmt
		{
			get;
			set;
		}
		#endregion
		#region TaxDiscountAmt
		public abstract class taxDiscountAmt : PX.Data.BQL.BqlDecimal.Field<taxDiscountAmt> { }
		[PXDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual decimal? TaxDiscountAmt
		{
			get;
			set;
		}
		#endregion
		#region CuryTaxAmt
		public abstract class curyTaxAmt : PX.Data.BQL.BqlDecimal.Field<curyTaxAmt> { }
		protected Decimal? _CuryTaxAmt;
		[PXDBCurrency(typeof(ARTax.curyInfoID), typeof(ARTax.taxAmt))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Tax Amount", Visibility = PXUIVisibility.Visible)]
		public virtual Decimal? CuryTaxAmt
		{
			get
			{
				return this._CuryTaxAmt;
			}
			set
			{
				this._CuryTaxAmt = value;
			}
		}
		#endregion
		#region TaxAmt
		public abstract class taxAmt : PX.Data.BQL.BqlDecimal.Field<taxAmt> { }
		protected Decimal? _TaxAmt;
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Tax Amount", Visibility = PXUIVisibility.Visible)]
		public virtual Decimal? TaxAmt
		{
			get
			{
				return this._TaxAmt;
			}
			set
			{
				this._TaxAmt = value;
			}
		}
		#endregion
		#region CuryExpenseAmt
		public abstract class curyExpenseAmt : PX.Data.BQL.BqlDecimal.Field<curyExpenseAmt> { }
		[PXDBCurrency(typeof(ARTax.curyInfoID), typeof(ARTax.expenseAmt))]
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Expense Amount", Visibility = PXUIVisibility.Visible)]
		public override Decimal? CuryExpenseAmt
		{
			get; set;
		}
		#endregion
		#region ExpenseAmt
		public abstract class expenseAmt : PX.Data.BQL.BqlDecimal.Field<expenseAmt> { }
		#endregion
		#region CuryExemptedAmt
		public abstract class curyExemptedAmt : IBqlField { }

		/// <summary>
		/// The exempted amount in the record currency.
		/// </summary>
		[PXDBCurrency(typeof(TaxTran.curyInfoID), typeof(TaxTran.exemptedAmt))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Exempted Amount", Visibility = PXUIVisibility.Visible, FieldClass = nameof(FeaturesSet.ExemptedTaxReporting))]
		public virtual decimal? CuryExemptedAmt
		{
			get;
			set;
		}
		#endregion
		#region ExemptedAmt
		public abstract class exemptedAmt : IBqlField { }

		/// <summary>
		/// The exempted amount in the base currency.
		/// </summary>
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Exempted Amount", Visibility = PXUIVisibility.Visible, FieldClass = nameof(FeaturesSet.ExemptedTaxReporting))]
		public virtual decimal? ExemptedAmt
		{
			get;
			set;
		}
		#endregion
		#region CuryRetainedTaxableAmt
		public abstract class curyRetainedTaxableAmt : PX.Data.BQL.BqlDecimal.Field<curyRetainedTaxableAmt> { }

		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		[PXDBCurrency(typeof(ARTax.curyInfoID), typeof(ARTax.retainedTaxableAmt))]
		[PXUIField(DisplayName = "Retained Taxable Amount", Visibility = PXUIVisibility.Visible, FieldClass = nameof(FeaturesSet.Retainage))]
		public virtual decimal? CuryRetainedTaxableAmt
		{
			get;
			set;
		}
		#endregion
		#region RetainedTaxableAmt
		public abstract class retainedTaxableAmt : PX.Data.BQL.BqlDecimal.Field<retainedTaxableAmt> { }

		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		[PXDBDecimal(4)]
		[PXUIField(DisplayName = "Retained Taxable Amount", Visibility = PXUIVisibility.Visible)]
		public virtual decimal? RetainedTaxableAmt
		{
			get;
			set;
		}
		#endregion
		#region CuryRetainedTaxAmt
		public abstract class curyRetainedTaxAmt : PX.Data.BQL.BqlDecimal.Field<curyRetainedTaxAmt> { }

		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		[PXDBCurrency(typeof(ARTax.curyInfoID), typeof(ARTax.retainedTaxAmt))]
		[PXUIField(DisplayName = "Retained Tax", Visibility = PXUIVisibility.Visible, FieldClass = nameof(FeaturesSet.Retainage))]
		public virtual decimal? CuryRetainedTaxAmt
		{
			get;
			set;
		}
		#endregion
		#region RetainedTaxAmt
		public abstract class retainedTaxAmt : PX.Data.BQL.BqlDecimal.Field<retainedTaxAmt> { }

		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		[PXDBDecimal(4)]
		[PXUIField(DisplayName = "Retained Tax", Visibility = PXUIVisibility.Visible)]
		public virtual decimal? RetainedTaxAmt
		{
			get;
			set;
		}
		#endregion

		#region tstamp
		public abstract class Tstamp : PX.Data.BQL.BqlByteArray.Field<Tstamp> { }
		protected Byte[] _tstamp;
		[PXDBTimestamp()]
		public virtual Byte[] tstamp
		{
			get
			{
				return this._tstamp;
			}
			set
			{
				this._tstamp = value;
			}
		}
        #endregion
	}
}
