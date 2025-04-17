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
using System.Collections.Generic;
using System.Text;
using PX.Data;
using PX.Objects.TX;
using PX.Objects.CM;
using PX.Objects.CS;
using PX.Data.ReferentialIntegrity.Attributes;

namespace PX.Objects.SO
{
	/// <summary>
	/// Represents tax details at the sales order document level, that is, information on all individual taxes applied
	/// to the document lines.
	/// </summary>
	/// <remarks>
	/// It may be an aggregation of the appropriate SOTax records in case of internal tax calculation, or when data is
	/// calculated externally.
	/// The records of this type are created and edited on the <i>Sales Orders (SO301000)</i> form (corresponding to
	/// the <see cref="SOOrderEntry"/> graph).
	/// </remarks>
    [PXCacheName(Messages.SOTaxTran)]
    [System.SerializableAttribute()]
    public partial class SOTaxTran : TaxDetail, PX.Data.IBqlTable
    {
		#region Keys
		public class PK : PrimaryKeyOf<SOTaxTran>.By<orderType, orderNbr, lineNbr, taxID, recordID>
		{
			public static SOTaxTran Find(PXGraph graph, string orderType, string orderNbr, int? lineNbr, string taxID, int? recordID, PKFindOptions options = PKFindOptions.None)
				=> FindBy(graph, orderType, orderNbr, lineNbr, taxID, recordID, options);
		}
		public class UK
		{
			public class ByRecordID : PrimaryKeyOf<SOTaxTran>.By<recordID>
			{
				public static SOTaxTran Find(PXGraph graph, string recordID, PKFindOptions options = PKFindOptions.None) => FindBy(graph, recordID, options);
			}
			public class ByJurisNameAndType : PrimaryKeyOf<SOTaxTran>.By<orderType, orderNbr, lineNbr, taxID, jurisName, jurisType>
			{
				public static SOTaxTran Find(PXGraph graph, string orderType, string orderNbr, int? lineNbr, string taxID, string jurisName, string jurisType, PKFindOptions options = PKFindOptions.None)
					=> FindBy(graph, orderType, orderNbr, lineNbr, taxID, jurisName, jurisType, options);
			}
		}
		public static class FK
		{
			public class Order : SOOrder.PK.ForeignKeyOf<SOTaxTran>.By<orderType, orderNbr> { }
			public class OrderLine : SOLine.PK.ForeignKeyOf<SOTaxTran>.By<orderType, orderNbr, lineNbr> { }
			public class SOTax : Objects.SO.SOTax.PK.ForeignKeyOf<SOTaxTran>.By<orderType, orderNbr, lineNbr, taxID> { }
			public class Tax : TX.Tax.PK.ForeignKeyOf<SOTaxTran>.By<taxID> { }
			public class TaxZone : TX.TaxZone.PK.ForeignKeyOf<SOTaxTran>.By<taxZoneID> { }
			public class CurrencyInfo : CM.CurrencyInfo.PK.ForeignKeyOf<SOTaxTran>.By<curyInfoID> { }
		}
		#endregion
		#region OrderType
		/// <inheritdoc cref="OrderType"/>
		public abstract class orderType : PX.Data.BQL.BqlString.Field<orderType> { }
        protected String _OrderType;

        /// <summary>
        /// The type of the sales order in which this tax item is listed.
        /// </summary>
        /// <remarks>
        /// The field is included in the following foreign keys:
        /// <list type="bullet">
        /// <item><see cref="FK.Order"/>, the field is a part of the identifier of the sales order
        /// <see cref="SOOrder"/>.<see cref="SOOrder.orderType"/></item>
        /// <item><see cref="FK.OrderLine"/>, the field is a part of the identifier of the sales order line
        /// <see cref="SOLine"/>.<see cref="SOLine.orderType"/></item>
        /// <item><see cref="FK.SOTax"/>, the field is a part of the identifier of the parent sales order's tax line
        /// <see cref="Objects.SO.SOTax"/>.<see cref="Objects.SO.SOTax.orderType"/></item>
        /// </list>
        /// </remarks>
		// Acuminator disable once PX1055 DacKeyFieldsWithIdentityKeyField [Such PK is needed for good SQL performance]
        [PXDBString(2, IsFixed = true, IsKey = true)]
        [PXDBDefault(typeof(SOOrder.orderType))]
        [PXUIField(DisplayName = "Order Type", Enabled = false, Visible = false)]
        public virtual String OrderType
        {
            get
            {
                return this._OrderType;
            }
            set
            {
                this._OrderType = value;
            }
        }
        #endregion
        #region OrderNbr
        /// <inheritdoc cref="OrderNbr"/>
        public abstract class orderNbr : PX.Data.BQL.BqlString.Field<orderNbr> { }
        protected String _OrderNbr;

        /// <summary>
        /// The reference number of the sales order in which this tax item is listed.
        /// </summary>
        /// <remarks>
        /// The field is included in the following foreign keys:
        /// <list type="bullet">
        /// <item><see cref="FK.Order"/>, the field is a part of the identifier of the sales order
        /// <see cref="SOOrder"/>.<see cref="SOOrder.orderNbr"/></item>
        /// <item><see cref="FK.OrderLine"/>, the field is a part of the identifier of the sales order line
        /// <see cref="SOLine"/>.<see cref="SOLine.orderNbr"/></item>
        /// <item><see cref="FK.SOTax"/>, the field is a part of the identifier of the parent sales order's tax line
        /// <see cref="Objects.SO.SOTax"/>.<see cref="Objects.SO.SOTax.orderNbr"/></item>
        /// </list>
        /// </remarks>
		// Acuminator disable once PX1055 DacKeyFieldsWithIdentityKeyField [Such PK is needed for good SQL performance]
        [PXDBString(15, IsUnicode = true, InputMask = "", IsKey = true)]
        [PXDBDefault(typeof(SOOrder.orderNbr))]
        [PXUIField(DisplayName = "Order Nbr.", Enabled = false, Visible = false)]
        public virtual String OrderNbr
        {
            get
            {
                return this._OrderNbr;
            }
            set
            {
                this._OrderNbr = value;
            }
        }
        #endregion
        #region LineNbr
        /// <inheritdoc cref="LineNbr"/>
        public abstract class lineNbr : PX.Data.BQL.BqlInt.Field<lineNbr> { }
        protected Int32? _LineNbr;

        /// <summary>
        /// The line number of this tax item.
        /// </summary>
        /// <remarks>
        /// The field is included in the following foreign keys:
        /// <list type="bullet">
        /// <item><see cref="FK.OrderLine"/>, the field is a part of the identifier of the sales order line
        /// <see cref="SOLine"/>.<see cref="SOLine.lineNbr"/></item>
        /// <item><see cref="FK.SOTax"/>, the field is a part of the identifier of the parent sales order's tax line
        /// <see cref="Objects.SO.SOTax"/>.<see cref="Objects.SO.SOTax.lineNbr"/></item>
        /// </list>
        /// </remarks>
		// Acuminator disable once PX1055 DacKeyFieldsWithIdentityKeyField [Such PK is needed for good SQL performance]
        [PXDBInt(IsKey = true)]
        [PXDefault(int.MaxValue)]
        [PXUIField(DisplayName = "Line Nbr.", Visibility = PXUIVisibility.Visible, Visible = false)]
		[PXParent(typeof(FK.Order))]
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
        /// <inheritdoc cref="TaxID"/>
        public abstract class taxID : PX.Data.BQL.BqlString.Field<taxID> { }

        /// <summary>
        /// The identifier of the specific tax applied to the document.
        /// </summary>
        /// <remarks>
        /// The field is included in the following foreign keys:
        /// <list type="bullet">
        /// <item><see cref="FK.SOTax"/>, the field is a part of the identifier of the parent sales order's tax line
        /// <see cref="Objects.SO.SOTax"/>.<see cref="Objects.SO.SOTax.taxID"/></item>
        /// <item><see cref="FK.Tax"/>, the field is the identifier of the tax
        /// <see cref="TX.Tax"/>.<see cref="TX.Tax.taxID"/></item>
        /// </list>
        /// </remarks>
		[TaxID]
		[PXDefault()]
        [PXUIField(DisplayName = "Tax ID", Visibility = PXUIVisibility.Visible)]
		[PXSelector(typeof(Tax.taxID), DescriptionField = typeof(Tax.descr), DirtyRead = true)]
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
		#region RecordID
		/// <inheritdoc cref="RecordID"/>
		public abstract class recordID : PX.Data.BQL.BqlInt.Field<recordID>
		{
		}
		protected Int32? _RecordID;

		/// <summary>
		/// This is an auto-numbered field, which is a part of the primary key.
		/// </summary>
		// Acuminator disable once PX1055 DacKeyFieldsWithIdentityKeyField [Such PK is needed for good SQL performance]
		[PXDBIdentity(IsKey = true)]
		public virtual Int32? RecordID
		{
			get
			{
				return this._RecordID;
			}
			set
			{
				this._RecordID = value;
			}
		}
		#endregion
		#region JurisType
		/// <inheritdoc cref="JurisType"/>
		public abstract class jurisType : PX.Data.BQL.BqlString.Field<jurisType> { }
        protected String _JurisType;

        /// <summary>
        /// The tax jurisdiction type. The field is used for the Avalara taxes.
        /// </summary>
        [PXDBString(9, IsUnicode = true)]
        [PXUIField(DisplayName = "Tax Jurisdiction Type")]
        public virtual String JurisType
        {
            get
            {
                return this._JurisType;
            }
            set
            {
                this._JurisType = value;
            }
        }
        #endregion
        #region JurisName
        /// <inheritdoc cref="JurisName"/>
        public abstract class jurisName : PX.Data.BQL.BqlString.Field<jurisName> { }
        protected String _JurisName;

        /// <summary>
        /// The tax jurisdiction name. The field is used for the Avalara taxes.
        /// </summary>
        [PXDBString(200, IsUnicode = true)]
        [PXUIField(DisplayName = "Tax Jurisdiction Name")]
        public virtual String JurisName
        {
            get
            {
                return this._JurisName;
            }
            set
            {
                this._JurisName = value;
            }
        }
        #endregion
        #region TaxRate
        /// <inheritdoc cref="TaxDetail.TaxRate"/>
        public abstract class taxRate : PX.Data.BQL.BqlDecimal.Field<taxRate> { }
        #endregion
        #region CuryInfoID
        /// <inheritdoc cref="CuryInfoID"/>
        public abstract class curyInfoID : PX.Data.BQL.BqlLong.Field<curyInfoID> { }

        /// <summary>
        /// The identifier of the <see cref="CM.CurrencyInfo">currency and exchange rate information</see>.
        /// The field is included in the foreign key <see cref="FK.CurrencyInfo"/>.
        /// </summary>
        /// <value>
        /// The value of this field corresponds to the value of the <see cref="CM.CurrencyInfo.curyInfoID"/> field.
        /// </value>
        [PXDBLong()]
        [CurrencyInfo(typeof(SOOrder.curyInfoID))]
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
        #region CuryTaxableAmt
        /// <inheritdoc cref="CuryTaxableAmt"/>
        public abstract class curyTaxableAmt : PX.Data.BQL.BqlDecimal.Field<curyTaxableAmt> { }
		protected decimal? _CuryTaxableAmt;

		/// <summary>
		/// The <see cref="taxableAmt">taxable amount</see> for the specific tax calculated through the document
		/// (in the currency of the document).
		/// </summary>
        [PXDBCurrency(typeof(SOTaxTran.curyInfoID), typeof(SOTaxTran.taxableAmt))]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Taxable Amount", Visibility = PXUIVisibility.Visible)]
        [PXUnboundFormula(typeof(Switch<Case<Where<WhereExempt<SOTaxTran.taxID>>, SOTaxTran.curyTaxableAmt>, decimal0>), typeof(SumCalc<SOOrder.curyVatExemptTotal>), CancelParentUpdate = true)]
		[PXUnboundFormula(typeof(Switch<Case<Where<WhereTaxable<SOTaxTran.taxID>>, SOTaxTran.curyTaxableAmt>, decimal0>), typeof(SumCalc<SOOrder.curyVatTaxableTotal>), CancelParentUpdate = true)]
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
		#region CuryExemptedAmt
		/// <inheritdoc cref="CuryExemptedAmt"/>
		public abstract class curyExemptedAmt : IBqlField { }

		/// <summary>
		/// The <see cref="exemptedAmt">exempt amount</see> for the specific tax calculated through the document
		/// (in the currency of the document).
		/// </summary>
		[PXDBCurrency(typeof(SOTaxTran.curyInfoID), typeof(SOTaxTran.exemptedAmt))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Exempted Amount", Visibility = PXUIVisibility.Visible, FieldClass = nameof(FeaturesSet.ExemptedTaxReporting))]
		public decimal? CuryExemptedAmt
		{
			get;
			set;
		}
		#endregion
		#region CuryUnshippedTaxableAmt
		/// <inheritdoc cref="CuryUnshippedTaxableAmt"/>
		public abstract class curyUnshippedTaxableAmt : PX.Data.BQL.BqlDecimal.Field<curyUnshippedTaxableAmt> { }
		protected Decimal? _CuryUnshippedTaxableAmt;

		/// <summary>
		/// The <see cref="unshippedTaxableAmt">taxable amount</see> for the specific tax calculated through the
		/// document lines with nonzero unshipped quantities of stock items (in the currency of the document).
		/// </summary>
		[PXDBCurrency(typeof(SOTaxTran.curyInfoID), typeof(SOTaxTran.unshippedTaxableAmt))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Unshipped Taxable Amount", Visibility = PXUIVisibility.Visible)]
		public virtual Decimal? CuryUnshippedTaxableAmt
		{
			get
			{
				return this._CuryUnshippedTaxableAmt;
			}
			set
			{
				this._CuryUnshippedTaxableAmt = value;
			}
		}
		#endregion
		#region CuryUnbilledTaxableAmt
		/// <inheritdoc cref="CuryUnbilledTaxableAmt"/>
		public abstract class curyUnbilledTaxableAmt : PX.Data.BQL.BqlDecimal.Field<curyUnbilledTaxableAmt> { }
		protected Decimal? _CuryUnbilledTaxableAmt;

		/// <summary>
		/// The <see cref="unbilledTaxableAmt">taxable amount</see> for the specific tax calculated through the
		/// unbilled document lines (in the currency of the document).
		/// The taxable amount for the specific tax calculated through the unbilled document lines.
		/// </summary>
		[PXDBCurrency(typeof(SOTaxTran.curyInfoID), typeof(SOTaxTran.unbilledTaxableAmt))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Unbilled Taxable Amount", Visibility = PXUIVisibility.Visible)]
		public virtual Decimal? CuryUnbilledTaxableAmt
		{
			get
			{
				return this._CuryUnbilledTaxableAmt;
			}
			set
			{
				this._CuryUnbilledTaxableAmt = value;
			}
		}
		#endregion
        #region TaxableAmt
        /// <inheritdoc cref="TaxableAmt"/>
        public abstract class taxableAmt : PX.Data.BQL.BqlDecimal.Field<taxableAmt> { }
		protected Decimal? _TaxableAmt;

		/// <summary>
		/// The taxable amount for the specific tax calculated through the document.
		/// </summary>
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
		#region ExemptedAmt
		/// <inheritdoc cref="ExemptedAmt"/>
		public abstract class exemptedAmt : IBqlField { }

		/// <summary>
		/// The exempt amount for the specific tax calculated through the document.
		/// </summary>
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Exempted Amount", Visibility = PXUIVisibility.Visible, FieldClass = nameof(FeaturesSet.ExemptedTaxReporting))]
		public decimal? ExemptedAmt
		{
			get;
			set;
		}
		#endregion
		#region UnshippedTaxableAmt
		/// <inheritdoc cref="UnshippedTaxableAmt"/>
		public abstract class unshippedTaxableAmt : PX.Data.BQL.BqlDecimal.Field<unshippedTaxableAmt> { }
		protected Decimal? _UnshippedTaxableAmt;

		/// <summary>
		/// The taxable amount for the specific tax calculated through the document lines with nonzero unshipped
		/// quantities of stock items.
		/// </summary>
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? UnshippedTaxableAmt
		{
			get
			{
				return this._UnshippedTaxableAmt;
			}
			set
			{
				this._UnshippedTaxableAmt = value;
			}
		}
		#endregion
		#region UnbilledTaxableAmt
		/// <inheritdoc cref="UnbilledTaxableAmt"/>
		public abstract class unbilledTaxableAmt : PX.Data.BQL.BqlDecimal.Field<unbilledTaxableAmt> { }
		protected Decimal? _UnbilledTaxableAmt;

		/// <summary>
		/// The taxable amount for the specific tax calculated through the unbilled document lines.
		/// </summary>
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? UnbilledTaxableAmt
		{
			get
			{
				return this._UnbilledTaxableAmt;
			}
			set
			{
				this._UnbilledTaxableAmt = value;
			}
		}
		#endregion
        #region CuryTaxAmt
        /// <inheritdoc cref="CuryTaxAmt"/>
        public abstract class curyTaxAmt : PX.Data.BQL.BqlDecimal.Field<curyTaxAmt> { }
		protected decimal? _CuryTaxAmt;

		/// <summary>
		/// The <see cref="taxAmt">tax amount</see> for the specific tax (in the currency of the document).
		/// </summary>
        [PXDBCurrency(typeof(SOTaxTran.curyInfoID), typeof(SOTaxTran.taxAmt))]
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
		#region CuryUnshippedTaxAmt
		/// <inheritdoc cref="CuryUnshippedTaxAmt"/>
		public abstract class curyUnshippedTaxAmt : PX.Data.BQL.BqlDecimal.Field<curyUnshippedTaxAmt> { }
		protected Decimal? _CuryUnshippedTaxAmt;

		/// <summary>
		/// The <see cref="unshippedTaxAmt">taxable amount</see> for the specific tax calculated through the unshipped
		/// document lines (in the currency of the document).
		/// </summary>
		[PXDBCurrency(typeof(SOTaxTran.curyInfoID), typeof(SOTaxTran.unshippedTaxAmt))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Unshipped Tax Amount", Visibility = PXUIVisibility.Visible)]
		public virtual Decimal? CuryUnshippedTaxAmt
		{
			get
			{
				return this._CuryUnshippedTaxAmt;
			}
			set
			{
				this._CuryUnshippedTaxAmt = value;
			}
		}
		#endregion
		#region CuryUnbilledTaxAmt
		public abstract class curyUnbilledTaxAmt : PX.Data.BQL.BqlDecimal.Field<curyUnbilledTaxAmt> { }
		protected Decimal? _CuryUnbilledTaxAmt;

		/// <summary>
		/// The <see cref="unbilledTaxAmt">tax amount</see> for the specific tax calculated through the unbilled
		/// document lines (in the currency of the document).
		/// </summary>
		[PXDBCurrency(typeof(SOTaxTran.curyInfoID), typeof(SOTaxTran.unbilledTaxAmt))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Unbilled Tax Amount", Visibility = PXUIVisibility.Visible)]
		public virtual Decimal? CuryUnbilledTaxAmt
		{
			get
			{
				return this._CuryUnbilledTaxAmt;
			}
			set
			{
				this._CuryUnbilledTaxAmt = value;
			}
		}
		#endregion

		#region Per Unit/Specific Tax Feature
		#region UnshippedTaxableQty
		/// <inheritdoc cref="UnshippedTaxableQty"/>
		public abstract class unshippedTaxableQty : PX.Data.BQL.BqlDecimal.Field<unshippedTaxableQty> { }

		/// <summary>
		///The unshipped taxable quantity for per unit taxes.
		/// </summary>
		[IN.PXDBQuantity]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Unshipped Taxable Qty.", Enabled = false)]
		public virtual decimal? UnshippedTaxableQty
		{
			get;
			set;
		}
		#endregion

		#region UnbilledTaxableQty
		/// <inheritdoc cref="UnbilledTaxableQty"/>
		public abstract class unbilledTaxableQty : PX.Data.BQL.BqlDecimal.Field<unbilledTaxableQty> { }

		/// <summary>
		///The unbilled taxable quantity for per unit taxes.
		/// </summary>
		[IN.PXDBQuantity]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Unbilled Taxable Qty.", Enabled = false)]
		public virtual decimal? UnbilledTaxableQty
		{
			get;
			set;
		}
		#endregion
		#endregion

		#region TaxAmt
		/// <inheritdoc cref="TaxAmt"/>
		public abstract class taxAmt : PX.Data.BQL.BqlDecimal.Field<taxAmt> { }
		protected Decimal? _TaxAmt;

		/// <summary>
		/// The tax amount for the specific tax calculated through the document.
		/// </summary>
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
		/// <inheritdoc cref="CuryExpenseAmt"/>
		public abstract class curyExpenseAmt : PX.Data.BQL.BqlDecimal.Field<curyExpenseAmt> { }

		/// <summary>
		/// The <see cref="expenseAmt">expense amount</see> of the tax line (in the currency of the document).
		/// </summary>
		[PXDBCurrency(typeof(SOTaxTran.curyInfoID), typeof(SOTaxTran.expenseAmt))]
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Expense Amount", Visibility = PXUIVisibility.Visible)]
		public override Decimal? CuryExpenseAmt
		{
			get; set;
		}
		#endregion
		#region ExpenseAmt
		/// <inheritdoc cref="TaxDetail.ExpenseAmt"/>
		public abstract class expenseAmt : PX.Data.BQL.BqlDecimal.Field<expenseAmt> { }
		#endregion
		#region UnshippedTaxAmt
		/// <inheritdoc cref="UnshippedTaxAmt"/>
		public abstract class unshippedTaxAmt : PX.Data.BQL.BqlDecimal.Field<unshippedTaxAmt> { }
		protected Decimal? _UnshippedTaxAmt;

		/// <summary>
		/// The taxable amount for the specific tax calculated through the unshipped document lines.
		/// </summary>
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? UnshippedTaxAmt
		{
			get
			{
				return this._UnshippedTaxAmt;
			}
			set
			{
				this._UnshippedTaxAmt = value;
			}
		}
		#endregion
		#region UnbilledTaxAmt
		/// <inheritdoc cref="UnbilledTaxAmt"/>
		public abstract class unbilledTaxAmt : PX.Data.BQL.BqlDecimal.Field<unbilledTaxAmt> { }
		protected Decimal? _UnbilledTaxAmt;

		/// <summary>
		/// The tax amount for the specific tax calculated through the unbilled document lines.
		/// </summary>
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? UnbilledTaxAmt
		{
			get
			{
				return this._UnbilledTaxAmt;
			}
			set
			{
				this._UnbilledTaxAmt = value;
			}
		}
		#endregion
		#region TaxZoneID
		/// <inheritdoc cref="TaxZoneID"/>
		public abstract class taxZoneID : PX.Data.BQL.BqlString.Field<taxZoneID> { }
		protected String _TaxZoneID;

		/// <summary>
		/// The tax zone to be used to process customer sales orders.
		/// </summary>
		/// <value>
		/// The value of this field corresponds to the value of the <see cref="TX.TaxZone.taxZoneID"/> field.
		/// </value>
		/// <remarks>
		///  The field is the identifier of the <see cref="TX.TaxZone">tax zone</see>. The field is included in the
		/// <see cref="FK.TaxZone"/> foreign key.
		/// </remarks>
		// Acuminator disable once PX1055 DacKeyFieldsWithIdentityKeyField [Such PK is needed for good SQL performance]
		[PXDBString(10, IsUnicode = true, IsKey = true)]
		[PXUIField(DisplayName = "Customer Tax Zone", Visibility = PXUIVisibility.Visible, Enabled = false)]
		[PXSelector(typeof(TaxZone.taxZoneID), DescriptionField = typeof(TaxZone.descr), Filterable = true)]
		[PXRestrictor(typeof(Where<TaxZone.isManualVATZone, Equal<False>>), TX.Messages.CantUseManualVAT)]
		public virtual String TaxZoneID
		{
			get
			{
				return this._TaxZoneID;
			}
			set
			{
				this._TaxZoneID = value;
			}
		}
		#endregion
	}
}
