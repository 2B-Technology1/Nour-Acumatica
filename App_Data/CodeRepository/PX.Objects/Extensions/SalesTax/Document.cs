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
using PX.Objects.TX;

namespace PX.Objects.Extensions.SalesTax
{
    /// <summary>A mapped cache extension that represents a document that supports sales taxes.</summary>
    public class Document : PXMappedCacheExtension
    {
        #region BranchID
        /// <exclude />
        public abstract class branchID : PX.Data.BQL.BqlInt.Field<branchID> { }
        /// <exclude />
        protected Int32? _BranchID;

        /// <summary>The identifier of the branch associated with the document.</summary>
        public virtual Int32? BranchID
        {
            get
            {
                return _BranchID;
            }
            set
            {
                _BranchID = value;
            }
        }
        #endregion
        #region CuryID
        /// <exclude />
        public abstract class curyID : PX.Data.BQL.BqlString.Field<curyID> { }
        /// <exclude />
        protected String _CuryID;

        /// <summary>The identifier of the currency of the document.</summary>

        public virtual String CuryID
        {
            get
            {
                return _CuryID;
            }
            set
            {
                _CuryID = value;
            }
        }
        #endregion
        #region CuryInfoID
        /// <exclude />
        public abstract class curyInfoID : PX.Data.BQL.BqlLong.Field<curyInfoID> { }
        /// <exclude />
        protected Int64? _CuryInfoID;

        /// <summary>
        /// The identifier of the CurrencyInfo object associated with the document.
        /// </summary>

        public virtual Int64? CuryInfoID
        {
            get
            {
                return _CuryInfoID;
            }
            set
            {
                _CuryInfoID = value;
            }
        }
        #endregion
        #region DocumentDate
        /// <exclude />
        public abstract class documentDate : PX.Data.BQL.BqlDateTime.Field<documentDate> { }
        /// <exclude />
        protected DateTime? _DocumentDate;

        /// <summary>The date of the document.</summary>
        public virtual DateTime? DocumentDate
        {
            get
            {
                return _DocumentDate;
            }
            set
            {
                _DocumentDate = value;
            }
        }
        #endregion
        #region FinPeriodID
        /// <exclude />
        public abstract class finPeriodID : PX.Data.BQL.BqlString.Field<finPeriodID> { }

        /// <summary>The identifier of the financial period of the document.</summary>
        public virtual string FinPeriodID { get; set; }
        #endregion
        #region TaxZoneID
        /// <exclude />
        public abstract class taxZoneID : PX.Data.BQL.BqlString.Field<taxZoneID> { }

        /// <summary>The identifier of the tax zone.</summary>
        public virtual String TaxZoneID { get; set; }
        #endregion
        #region TermsID
        /// <exclude />
        public abstract class termsID : PX.Data.BQL.BqlString.Field<termsID> { }


        /// <summary>The identifier of the credit terms.</summary>
        public virtual String TermsID { get; set; }
        #endregion
        #region CuryLineTotal
        /// <exclude />
        public abstract class curyLineTotal : PX.Data.BQL.BqlDecimal.Field<curyLineTotal> { }

        /// <summary>The total amount of the lines of the document, in the currency of the document (<see cref="CuryID" />).</summary>
        public virtual decimal? CuryLineTotal { get; set; }
        #endregion
        #region CurydiscountLineTotal
        /// <exclude />
        public abstract class curyDiscountLineTotal : PX.Data.BQL.BqlDecimal.Field<curyDiscountLineTotal> { }

        /// <summary>The total discounts of the lines of the document, in the currency of the document (<see cref="CuryID" />).</summary>
        public virtual decimal? CuryDiscountLineTotal { get; set; }
        #endregion
        #region CuryExtPriceTotal
        /// <exclude />
        public abstract class curyExtPriceTotal : PX.Data.BQL.BqlDecimal.Field<curyExtPriceTotal> { }

        /// <summary>The total amount without discounts of the lines of the document, in the currency of the document (<see cref="CuryID" />).</summary>
        public virtual decimal? CuryExtPriceTotal { get; set; }
        #endregion
        #region CuryDocBal
        /// <exclude />
        public abstract class curyDocBal : PX.Data.BQL.BqlDecimal.Field<curyDocBal> { }

        /// <summary>The balance of the document, in the currency of the document (<see cref="CuryID" />).</summary>
        public decimal? CuryDocBal { get; set; }
        #endregion
        #region CuryTaxTotal
        /// <exclude />
        public abstract class curyTaxTotal : PX.Data.BQL.BqlDecimal.Field<curyTaxTotal> { }

        /// <summary>The total amount of tax paid on the document, in the currency of the document (<see cref="CuryID" />).</summary>
        public decimal? CuryTaxTotal { get; set; }
        #endregion
        #region CuryWhTaxTotal
        /// <exclude />
        public abstract class curyWhTaxTotal : PX.Data.BQL.BqlDecimal.Field<curyWhTaxTotal> { }

        public decimal? CuryWhTaxTotal { get; set; }
        #endregion
        #region CuryDiscTot
        /// <exclude />
        public abstract class curyDiscTot : PX.Data.BQL.BqlDecimal.Field<curyDiscTot> { }

        /// <summary>The total group and document discount for the document. The discount is in the currency of the document (<see cref="CuryID" />).</summary>
        public virtual decimal? CuryDiscTot { get; set; }
        #endregion
        #region CuryDiscAmt
        /// <exclude />
        public abstract class curyDiscAmt : PX.Data.BQL.BqlDecimal.Field<curyDiscAmt> { }

        /// <summary>The discount amount of the document, in the currency of the document (<see cref="CuryID" />).</summary>
        public virtual decimal? CuryDiscAmt { get; set; }
        #endregion
        #region CuryOrigWhTaxAmt
        /// <exclude />
        public abstract class curyOrigWhTaxAmt : PX.Data.BQL.BqlDecimal.Field<curyOrigWhTaxAmt> { }

        /// <summary>The amount of withholding tax calculated for the document, in the currency of the document (<see cref="CuryID" />).</summary>
        public virtual Decimal? CuryOrigWhTaxAmt { get; set; }
        #endregion
        #region CuryTaxRoundDiff
        /// <exclude />
        public abstract class curyTaxRoundDiff : PX.Data.BQL.BqlDecimal.Field<curyTaxRoundDiff> { }

        /// <summary>The tax amount discrepancy (that is, the difference between the tax amount calculated by the system and the tax amounts entered by a user manually for the
        /// tax-inclusive items). The amount is in the currency of the document (<see cref="CuryID" />).</summary>
        public decimal? CuryTaxRoundDiff { get; set; }
        #endregion
        #region TaxRoundDiff
        /// <exclude />
        public abstract class taxRoundDiff : PX.Data.BQL.BqlDecimal.Field<taxRoundDiff> { }

        /// <summary>The tax amount discrepancy (that is, the difference between the tax amount calculated by the system and the tax amounts entered by a user manually for the
        /// tax-inclusive items). The amount is in the base currency of the company.</summary>
        public decimal? TaxRoundDiff
        {
            get; set;
        }
        #endregion
        #region ExternalTaxesImportInProgress
        /// <exclude />
        public abstract class externalTaxesImportInProgress : PX.Data.BQL.BqlBool.Field<externalTaxesImportInProgress> { }
        /// <summary>Indicates (if set to <tt>true</tt>) that the tax information related to the document was was imported from the external tax engine.</summary>

        public virtual bool? ExternalTaxesImportInProgress
        {
            get;
            set;
        }
        #endregion
        #region IsTaxSaved
        /// <exclude />
        public abstract class isTaxSaved : PX.Data.BQL.BqlBool.Field<isTaxSaved> { }
        /// <summary>Indicates (if set to <tt>true</tt>) that the tax information related to the document was saved to the external tax engine (Avalara).</summary>

        public virtual bool? IsTaxSaved
        {
            get;
            set;
        }
        #endregion
        #region TaxCalc
        /// <exclude />
        public abstract class taxCalc : PX.Data.IBqlField { }

        /// <summary>Specifies whether taxes should be calculated and how they should be calculated.</summary>
        public TaxCalc? TaxCalc { get; set; }
        #endregion
        #region TaxCalcMode
        /// <exclude />
        public abstract class taxCalcMode : PX.Data.BQL.BqlString.Field<taxCalcMode> { }

        /// <summary>The tax calculation mode, which defines which amounts (tax-inclusive or tax-exclusive) should be entered in the detail lines of a document.</summary>
        public virtual string TaxCalcMode { get; set; }
        #endregion
    }
}
