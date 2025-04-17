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
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.AR.BQL;
using PX.Objects.CM;
using PX.Objects.Common;
using PX.Objects.Common.Extensions;
using PX.Objects.GL;
using System;

namespace PX.Objects.AR
{
	/// <summary>
	/// A projection that is used in the AR Statement report. Cash sales and cash returns 
	/// are excluded to prevent them from appearing in the Statement Report.
	/// </summary>
	[PXCacheName(Messages.ARStatementDetailInfo)]
	[PXProjection(typeof(Select2<
		ARStatementDetail,
			LeftJoin<ARTranPostGL,
				On<ARTranPostGL.iD, Equal<ARStatementDetail.tranPostID>>,
			LeftJoin<ARRegister, On<ARTranPostGL.docType, Equal<ARRegister.docType>, And<ARTranPostGL.refNbr, Equal<ARRegister.refNbr>>>,
			LeftJoin<ARRegister2, On<ARTranPostGL.sourceDocType, Equal<ARRegister2.docType>, And<ARTranPostGL.sourceRefNbr, Equal<ARRegister2.refNbr>>>,
			LeftJoin<Standalone.ARInvoice,
				On<Standalone.ARInvoice.docType, Equal<ARRegister.docType>,
				And<Standalone.ARInvoice.refNbr, Equal<ARRegister.refNbr>>>,
			LeftJoin<Standalone.ARPayment,
				On<Standalone.ARPayment.docType, Equal<ARRegister.docType>,
				And<Standalone.ARPayment.refNbr, Equal<ARRegister.refNbr>>>>>>>>,
		Where2<IsNotSelfApplying<ARRegister.docType>, Or<ARRegister.docType.IsNull>>>),
		Persistent = false)]
	[Serializable]
	public partial class ARStatementDetailInfo : IBqlTable
	{
		#region Keys
		// <exclude/>
		public new class PK : PrimaryKeyOf<ARStatementDetailInfo>.By<branchID, customerID, curyID, statementDate, docType, refNbr, refNoteID>
		{
			public static ARStatementDetailInfo Find(PXGraph graph, Int32? branchID, Int32? customerID, String curyID, DateTime? statementDate, String docType, String refNbr, Guid? refNoteID, PKFindOptions options = PKFindOptions.None)
													=> FindBy(graph, branchID, customerID, curyID, statementDate, docType, refNbr, refNoteID, options);
		}
		public new static class FK
		{
			public class Branch : GL.Branch.PK.ForeignKeyOf<ARStatementDetailInfo>.By<branchID> { }
			public class Customer : AR.Customer.PK.ForeignKeyOf<ARStatementDetailInfo>.By<customerID> { }
			public class CurrencyInfo : CM.CurrencyInfo.PK.ForeignKeyOf<ARStatementDetailInfo>.By<curyInfoID> { }
			public class Currency : CM.Currency.PK.ForeignKeyOf<ARStatementDetailInfo>.By<curyID> { }
			public class Statement : AR.ARStatement.PK.ForeignKeyOf<ARStatementDetailInfo>.By<branchID, customerID, curyID, statementDate> { }
		}
		#endregion

		#region ID

		public abstract class iD : PX.Data.BQL.BqlInt.Field<iD>
		{
		}

		[PXDBInt(BqlTable = typeof(ARTranPostGL))]
		public virtual int? ID { get; set; }

		#endregion
		#region StatementDate
		public abstract class statementDate : PX.Data.BQL.BqlDateTime.Field<statementDate> { }
		/// <summary>
		/// The date of the <see cref="ARStatement">Customer Statement</see>, to which
		/// the detail belongs. This field is part of the compound key of the statement
		/// detail, and part of the foreign key referencing the <see cref="ARStatement">
		/// Customer Statement</see> record.
		/// Corresponds to the <see cref="ARStatement.StatementDate"/> field.
		/// </summary>
		[PXDBDate(IsKey = true, BqlTable = (typeof(ARStatementDetail)))]
		[PXDefault(typeof(ARStatement.statementDate))]
		[PXUIField(DisplayName = "Statement Date")]
		public virtual DateTime? StatementDate
		{
			get;
			set;
		}
		#endregion

		#region IsOpen
		public abstract class isOpen : PX.Data.BQL.BqlBool.Field<isOpen> { }
		/// <summary>
		/// If set to <c>true</c>, indicates that the document
		/// is open on the statement date.
		/// </summary>
		[PXDBBool(BqlTable = (typeof(ARStatementDetail)))]
		[PXDefault(true)]
		public virtual bool? IsOpen
		{
			get;
			set;
		}
		#endregion

		#region DocStatementDate
		public abstract class docStatementDate : PX.Data.BQL.BqlDateTime.Field<docStatementDate> { }
		[PXDBDate(BqlField = typeof(ARRegister.statementDate))]
		public virtual DateTime? DocStatementDate
		{
			get;
			set;
		}
		#endregion

		#region DocDesc
		public abstract class docDesc : PX.Data.BQL.BqlString.Field<docDesc> { }
		[PXDBString(60, IsUnicode = true, BqlField = typeof(ARRegister.docDesc))]
		[PXUIField(DisplayName = "Description", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual string DocDesc
		{
			get;
			set;
		}
		#endregion

		#region IsMigratedRecord
		public abstract class isMigratedRecord : PX.Data.BQL.BqlBool.Field<isMigratedRecord> { }

		/// <summary>
		/// Specifies (if set to <c>true</c>) that the record has been created 
		/// in migration mode without affecting GL module.
		/// </summary>
		[PXDBBool(BqlField = typeof(ARRegister.isMigratedRecord))]
		public virtual bool? IsMigratedRecord
		{
			get;
			set;
		}
		#endregion

		#region PrintDocType
		public abstract class printDocType : PX.Data.BQL.BqlString.Field<printDocType> { }
		[PXString(3, IsFixed = true)]
		[ARDocType.PrintList]
		[PXUIField(DisplayName = "Type", Visibility = PXUIVisibility.Visible, Enabled = true)]
		public virtual string PrintDocType
		{
			get
			{
				return this.DocType;
			}
		}
		#endregion

		#region Payable
		public abstract class payable : PX.Data.BQL.BqlBool.Field<payable> { }

		public virtual bool? Payable
		{
			[PXDependsOnFields(typeof(docType))]
			get
			{
				if (DocType == ARDocType.Refund || DocType == ARDocType.VoidRefund)
				{
					return true;
				}

				return ARDocType.Payable(this.DocType);
			}
		}
		#endregion

		#region BalanceSign
		public abstract class balanceSign : PX.Data.BQL.BqlDecimal.Field<balanceSign> { }

		public virtual decimal? BalanceSign
		{
			[PXDependsOnFields(typeof(docType))]
			get
			{
				return ARDocType.SignBalance(this.DocType);
			}
		}
		#endregion

		#region CuryInfoID
		public abstract class curyInfoID : PX.Data.BQL.BqlLong.Field<curyInfoID> { }
		[PXDBLong(BqlField = typeof(ARRegister.curyInfoID))]
		public virtual long? CuryInfoID
		{
			get;
			set;
		}
		#endregion

		#region RefNoteID
		public abstract class refNoteID : PX.Data.BQL.BqlInt.Field<refNoteID>
		{
		}

		[PXDBGuid(IsKey = true, BqlTable = (typeof(ARStatementDetail)))]
		public virtual Guid? RefNoteID { get; set; }
		#endregion

		#region CuryOrigDocAmt
		public abstract class curyOrigDocAmt : PX.Data.BQL.BqlDecimal.Field<curyOrigDocAmt> { }
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXDBCurrency(typeof(ARStatementDetailInfo.curyInfoID), typeof(ARStatementDetailInfo.origDocAmt), BqlField = typeof(ARRegister.curyOrigDocAmt))]
		[PXUIField(DisplayName = "Amount", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual decimal? CuryOrigDocAmt
		{
			get;
			set;
		}
		#endregion

		#region OrigDocAmt
		public abstract class origDocAmt : PX.Data.BQL.BqlDecimal.Field<origDocAmt> { }
		[PXDBBaseCury(BqlField = typeof(ARRegister.origDocAmt))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? OrigDocAmt
		{
			get;
			set;
		}
		#endregion

		#region CuryInitDocBal
		public abstract class curyInitDocBal : PX.Data.BQL.BqlDecimal.Field<curyInitDocBal> { }
		/// <summary>
		/// The entered in migration mode balance of the document.
		/// Given in the <see cref="CuryID">currency of the document</see>.
		/// </summary>
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXDBCurrency(typeof(ARStatementDetailInfo.curyInfoID), typeof(ARStatementDetailInfo.initDocBal), BqlField = typeof(ARRegister.curyInitDocBal))]
		public virtual decimal? CuryInitDocBal
		{
			get;
			set;
		}
		#endregion

		#region InitDocBal
		public abstract class initDocBal : PX.Data.BQL.BqlDecimal.Field<initDocBal> { }
		/// <summary>
		/// The entered in migration mode balance of the document.
		/// Given in the <see cref="Company.BaseCuryID">base currency of the company</see>.
		/// </summary>
		[PXDBBaseCury(BqlField = typeof(ARRegister.initDocBal))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? InitDocBal
		{
			get;
			set;
		}
		#endregion

		#region InvoiceNbr
		public abstract class invoiceNbr : PX.Data.BQL.BqlString.Field<invoiceNbr> { }
		[PXDBString(40, IsUnicode = true, BqlField = typeof(Standalone.ARInvoice.invoiceNbr))]
		[PXUIField(DisplayName = "Customer Ref. Nbr.", Visibility = PXUIVisibility.SelectorVisible, Required = false)]
		public virtual string InvoiceNbr
		{
			get;
			set;
		}
		#endregion

		#region DueDate
		public abstract class dueDate : PX.Data.BQL.BqlDateTime.Field<dueDate> { }
		[PXDBDate(BqlField = typeof(ARRegister.dueDate))]
		[PXUIField(DisplayName = "Due Date", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual DateTime? DueDate
		{
			get;
			set;
		}
		#endregion

		#region ExtRefNbr
		public abstract class extRefNbr : PX.Data.BQL.BqlString.Field<extRefNbr> { }
		[PXDBString(40, IsUnicode = true, BqlField = typeof(Standalone.ARPayment.extRefNbr))]
		[PXUIField(DisplayName = "Payment Ref.", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual string ExtRefNbr
		{
			get;
			set;
		}
		#endregion

		#region DocExtRefNbr
		[PXString(15, IsUnicode = true)]
		[PXUIField(DisplayName = "Ext. Ref.#", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual string DocExtRefNbr
		{
			[PXDependsOnFields(typeof(payable), typeof(invoiceNbr), typeof(extRefNbr))]
			get
			{
				bool? payable = ARDocType.Payable(DocType);
				return payable.HasValue
					? (payable.Value ? InvoiceNbr : ExtRefNbr)
					: string.Empty;
			}

		}
		#endregion

		#region CuryOrigDocAmtSigned

		[PXDecimal]
		[PXUIField(DisplayName = "Origin. Amt", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual decimal? CuryOrigDocAmtSigned
		{
			[PXDependsOnFields(typeof(docType), typeof(balanceSign), typeof(curyOrigDocAmt))]
			get
			{
				return BalanceSign * CuryOrigDocAmt;
			}
		}
		#endregion

		#region OrigDocAmtSigned
		[PXDecimal]
		public virtual decimal? OrigDocAmtSigned
		{
			[PXDependsOnFields(typeof(docType), typeof(balanceSign), typeof(origDocAmt))]
			get
			{
				return BalanceSign * OrigDocAmt;
			}
		}
		#endregion

		#region CuryInitDocBalSigned
		/// <summary>
		/// The entered in migration mode balance of the document.
		/// Given in the <see cref="CuryID">currency of the document</see>.
		/// </summary>
		[PXDecimal]
		public virtual decimal? CuryInitDocBalSigned
		{
			[PXDependsOnFields(typeof(docType), typeof(balanceSign), typeof(curyInitDocBal))]
			get
			{
				return BalanceSign * CuryInitDocBal;
			}
		}
		#endregion

		#region InitDocBalSigned
		/// <summary>
		/// The entered in migration mode balance of the document.
		/// Given in the <see cref="Company.BaseCuryID">base currency of the company</see>.
		/// </summary>
		[PXDecimal]
		public virtual decimal? InitDocBalSigned
		{
			[PXDependsOnFields(typeof(docType), typeof(balanceSign), typeof(initDocBal))]
			get
			{
				return BalanceSign * InitDocBal;
			}
		}
		#endregion

		#region DocBalance
		public abstract class docBalance : PX.Data.BQL.BqlDecimal.Field<docBalance> { }
		/// <summary>
		/// Indicates the balance, in base currency, that the document
		/// has on the statement date.
		/// </summary>
		[PXDBDecimal(4, BqlTable = (typeof(ARStatementDetail)))]
		[PXUIField(DisplayName = "Doc. Balance")]
		public virtual decimal? DocBalance
		{
			get;
			set;
		}
		#endregion

		#region CuryDocBalance
		public abstract class curyDocBalance : PX.Data.BQL.BqlDecimal.Field<curyDocBalance> { }
		/// <summary>
		/// Indicates the balance, in document currency, that the document
		/// has on the statement date.
		/// </summary>
		[PXDBDecimal(4, BqlTable = (typeof(ARStatementDetail)))]
		[PXUIField(DisplayName = "Cury. Doc. Balance")]
		public virtual decimal? CuryDocBalance
		{
			get;
			set;
		}
		#endregion

		#region CuryDocBalanceSigned

		[PXDecimal]
		[PXUIField(DisplayName = "Amount Due", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual decimal? CuryDocBalanceSigned
		{
			[PXDependsOnFields(typeof(payable), typeof(curyDocBalance))]
			get
			{
				return this.Payable.HasValue
					? (this.Payable.Value ? this.CuryDocBalance : -this.CuryDocBalance)
					: null;
			}
		}
		#endregion

		#region DocBalanceSigned
		[PXDecimal]
		public virtual decimal? DocBalanceSigned
		{
			[PXDependsOnFields(typeof(payable), typeof(docBalance))]
			get
			{
				return this.Payable.HasValue
					? (this.Payable.Value ? this.DocBalance : -this.DocBalance)
					: null;
			}
		}
		#endregion

		#region StatementType
		public abstract class statementType : PX.Data.BQL.BqlString.Field<statementType> { }
		/// <summary>
		/// The type of the customer statement.
		/// See <see cref="ARStatementDetail.StatementType"/>
		/// </summary>
		[PXDBString(1, IsFixed = true, BqlTable = (typeof(ARStatementDetail)))]
		[PXUIField(DisplayName = "Statement Type")]
		public virtual string StatementType
		{
			get;
			set;
		}
		#endregion
		#region BegBalance
		public abstract class begBalance : PX.Data.BQL.BqlDecimal.Field<begBalance> { }
		/// <summary>
		/// The beginning balance of the customer statement in the base currency. Only for the Balance Brought Forvard type.
		/// See <see cref="ARStatementDetail.BegBalance"/>
		/// </summary>
		[PXDBDecimal(4, BqlTable = (typeof(ARStatementDetail)))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Beg. Balance")]
		public virtual decimal? BegBalance
		{
			get;
			set;
		}
		#endregion
		#region CuryBegBalance
		public abstract class curyBegBalance : PX.Data.BQL.BqlDecimal.Field<curyBegBalance> { }
		/// <summary>
		/// The beginning balance of the customer statement in the foreign currency. Only for the Balance Brought Forvard type.
		/// See <see cref="ARStatementDetail.CuryBegBalance"/>
		/// </summary>
		[PXDBDecimal(4, BqlTable = (typeof(ARStatementDetail)))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Curr. Beg. Balance")]
		public virtual decimal? CuryBegBalance
		{
			get;
			set;
		}
		#endregion

		#region AgeBalance00
		public abstract class ageBalance00 : PX.Data.BQL.BqlDecimal.Field<ageBalance00> { }
		/// <summary>
		/// The customer statement's balance of the age bucket 00 in the base currency.
		/// See <see cref="ARStatementDetail.AgeBalance00"/>
		/// </summary>
		[PXDBDecimal(4, BqlTable = (typeof(ARStatementDetail)))]
		[PXUIField(DisplayName = "Age00 Balance")]
		public virtual decimal? AgeBalance00
		{
			get;
			set;
		}
		#endregion
		#region CuryAgeBalance00
		public abstract class curyAgeBalance00 : PX.Data.BQL.BqlDecimal.Field<curyAgeBalance00> { }
		/// <summary>
		/// The customer statement's balance of the age bucket 00 in the foreign currency.
		/// See <see cref="ARStatementDetail.CuryAgeBalance00"/>
		/// </summary>
		[PXDBDecimal(4, BqlTable = (typeof(ARStatementDetail)))]
		[PXUIField(DisplayName = "Cury. Age00 Balance")]
		public virtual decimal? CuryAgeBalance00
		{
			get;
			set;
		}
		#endregion
		#region AgeBalance01
		public abstract class ageBalance01 : PX.Data.BQL.BqlDecimal.Field<ageBalance01> { }
		/// <summary>
		/// The customer statement's balance of the age bucket 01 in the base currency.
		/// See <see cref="ARStatementDetail.AgeBalance01"/>
		/// </summary>
		[PXDBDecimal(4, BqlTable = (typeof(ARStatementDetail)))]
		[PXUIField(DisplayName = "Age01 Balance")]
		public virtual decimal? AgeBalance01
		{
			get;
			set;
		}
		#endregion
		#region CuryAgeBalance01
		public abstract class curyAgeBalance01 : PX.Data.BQL.BqlDecimal.Field<curyAgeBalance01> { }
		/// <summary>
		/// The customer statement's balance of the age bucket 01 in the foreign currency.
		/// See <see cref="ARStatementDetail.CuryAgeBalance01"/>
		/// </summary>
		[PXDBDecimal(4, BqlTable = (typeof(ARStatementDetail)))]
		[PXUIField(DisplayName = "Cury. Age01 Balance")]
		public virtual decimal? CuryAgeBalance01
		{
			get;
			set;
		}
		#endregion
		#region AgeBalance02
		public abstract class ageBalance02 : PX.Data.BQL.BqlDecimal.Field<ageBalance02> { }
		/// <summary>
		/// The customer statement's balance of the age bucket 02 in the base currency.
		/// See <see cref="ARStatementDetail.AgeBalance02"/>
		/// </summary>
		[PXDBDecimal(4, BqlTable = (typeof(ARStatementDetail)))]
		[PXUIField(DisplayName = "Age02 Balance")]
		public virtual decimal? AgeBalance02
		{
			get;
			set;
		}
		#endregion
		#region CuryAgeBalance02
		public abstract class curyAgeBalance02 : PX.Data.BQL.BqlDecimal.Field<curyAgeBalance02> { }
		/// <summary>
		/// The customer statement's balance of the age bucket 02 in the foreign currency.
		/// See <see cref="ARStatementDetail.CuryAgeBalance02"/>
		/// </summary>
		[PXDBDecimal(4, BqlTable = (typeof(ARStatementDetail)))]
		[PXUIField(DisplayName = "Cury. Age02 Balance")]
		public virtual decimal? CuryAgeBalance02
		{
			get;
			set;
		}
		#endregion
		#region AgeBalance03
		public abstract class ageBalance03 : PX.Data.BQL.BqlDecimal.Field<ageBalance03> { }
		/// <summary>
		/// The customer statement's balance of the age bucket 03 in the base currency.
		/// See <see cref="ARStatementDetail.AgeBalance03"/>
		/// </summary>
		[PXDBDecimal(4, BqlTable = (typeof(ARStatementDetail)))]
		[PXUIField(DisplayName = "Cury. Age03 Balance")]
		public virtual decimal? AgeBalance03
		{
			get;
			set;
		}
		#endregion
		#region CuryAgeBalance03
		public abstract class curyAgeBalance03 : PX.Data.BQL.BqlDecimal.Field<curyAgeBalance03> { }
		/// <summary>
		/// The customer statement's balance of the age bucket 03 in the foreign currency.
		/// See <see cref="ARStatementDetail.CuryAgeBalance03"/>
		/// </summary>
		[PXDBDecimal(4, BqlTable = (typeof(ARStatementDetail)))]
		[PXUIField(DisplayName = "Cury. Age03 Balance")]
		public virtual decimal? CuryAgeBalance03
		{
			get;
			set;
		}
		#endregion
		#region AgeBalance04
		public abstract class ageBalance04 : PX.Data.BQL.BqlDecimal.Field<ageBalance04> { }
		/// <summary>
		/// The customer statement's balance of the age bucket 04 in the base currency.
		/// See <see cref="ARStatementDetail.AgeBalance04"/>
		/// </summary>
		[PXDBDecimal(4, BqlTable = (typeof(ARStatementDetail)))]
		[PXUIField(DisplayName = "Age04 Balance")]
		public virtual decimal? AgeBalance04
		{
			get;
			set;
		}
		#endregion
		#region CuryAgeBalance04
		public abstract class curyAgeBalance04 : PX.Data.BQL.BqlDecimal.Field<curyAgeBalance04> { }
		/// <summary>
		/// The customer statement's balance of the age bucket 04 in the foreign currency.
		/// See <see cref="ARStatementDetail.CuryAgeBalance04"/>
		/// </summary>
		[PXDBDecimal(4, BqlTable = (typeof(ARStatementDetail)))]
		[PXUIField(DisplayName = "Cury. Age04 Balance")]
		public virtual decimal? CuryAgeBalance04
		{
			get;
			set;
		}
		#endregion

		#region Type

		public abstract class type : PX.Data.BQL.BqlString.Field<type>
		{
		}

		[PXDBString(BqlTable = typeof(ARTranPostGL))]
		[ARTranPost.type.List]
		public virtual string Type { get; set; }

		#endregion

		#region DocDate
		public abstract class docDate : PX.Data.BQL.BqlDateTime.Field<docDate>
		{
		}
		[PXDBDate(BqlTable = typeof(ARRegister))]
		[PXDefault(typeof(ARRegister.docDate))]
		[PXUIField(DisplayName = "Date", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual DateTime? DocDate { get; set; }
		#endregion

		#region DocType

		public abstract new class docType : PX.Data.BQL.BqlString.Field<docType>
		{
		}

		[PXDBString(IsKey = true, BqlTable = typeof(ARStatementDetail))]
		[PXUIField(DisplayName = "Doc. Type")]
		[ARDocType.List()]
		public virtual string DocType { get; set; }

		#endregion

		#region RefNbr
		public abstract new class refNbr : PX.Data.BQL.BqlString.Field<refNbr>
		{
		}

		[PXDBString(IsKey = true, BqlTable = typeof(ARStatementDetail))]
		[PXUIField(DisplayName = "Ref. Nbr.", Visibility = PXUIVisibility.Visible, Enabled = false)]
		public virtual string RefNbr { get; set; }
		#endregion

		#region SourceDocType

		public abstract class sourceDocType : PX.Data.BQL.BqlString.Field<sourceDocType>
		{
		}

		[PXUIField(DisplayName = "Source Doc. Type", BqlTable = typeof(ARTranPostGL))]
		[ARDocType.List()]
		[PXDBString(IsKey = true, BqlTable = typeof(ARTranPostGL))]
		public virtual string SourceDocType { get; set; }

		#endregion

		#region SourceRefNbr

		public abstract class sourceRefNbr : PX.Data.BQL.BqlString.Field<sourceRefNbr>
		{
		}

		[PXUIField(DisplayName = "Source Ref. Nbr.", Visibility = PXUIVisibility.Visible, Enabled = false)]
		[PXDBString(IsKey = true, BqlTable = typeof(ARTranPostGL))]
		public virtual string SourceRefNbr { get; set; }

		#endregion

		#region BranchID

		public abstract new class branchID : PX.Data.BQL.BqlInt.Field<branchID>
		{
		}

		[Branch(BqlTable = typeof(ARStatementDetail))]
		public virtual int? BranchID { get; set; }

		#endregion

		#region CustomerID

		public abstract new class customerID : PX.Data.BQL.BqlInt.Field<customerID>
		{
		}

		[Customer(BqlTable = typeof(ARStatementDetail))]
		public virtual int? CustomerID { get; set; }

		#endregion

		#region CuryBalanceAmt

		public abstract class curyBalanceAmt : PX.Data.BQL.BqlDecimal.Field<curyBalanceAmt>
		{
		}

		[PXDBDecimal(BqlTable = typeof(ARTranPostGL))]
		public virtual decimal? CuryBalanceAmt { get; set; }

		#endregion

		#region BalanceAmt

		public abstract class balanceAmt : PX.Data.BQL.BqlDecimal.Field<balanceAmt>
		{
		}

		[PXDBDecimal(BqlTable = typeof(ARTranPostGL))]
		public virtual decimal? BalanceAmt { get; set; }

		#endregion

		#region CuryTurnDiscAmt

		public abstract class curyTurnDiscAmt : PX.Data.BQL.BqlDecimal.Field<curyTurnDiscAmt>
		{
		}

		[PXDBDecimal(BqlTable = typeof(ARTranPostGL))]
		public virtual decimal? CuryTurnDiscAmt { get; set; }

		#endregion

		#region TurnDiscAmt

		public abstract class turnDiscAmt : PX.Data.BQL.BqlDecimal.Field<turnDiscAmt>
		{
		}

		[PXDBDecimal(BqlTable = typeof(ARTranPostGL))]
		public virtual decimal? TurnDiscAmt { get; set; }

		#endregion

		#region CuryTurnWOAmt

		public abstract class curyTurnWOAmt : PX.Data.BQL.BqlDecimal.Field<curyTurnWOAmt>
		{
		}

		[PXDBDecimal(BqlTable = typeof(ARTranPostGL))]
		public virtual decimal? CuryTurnWOAmt { get; set; }

		#endregion

		#region TurnRGOLAmt

		public abstract class rGOLAmt : PX.Data.BQL.BqlDecimal.Field<rGOLAmt>
		{
		}

		[PXDBDecimal(BqlTable = typeof(ARTranPostGL))]
		public virtual decimal? RGOLAmt { get; set; }

		#endregion

		#region TurnWOAmt

		public abstract class turnWOAmt : PX.Data.BQL.BqlDecimal.Field<turnWOAmt>
		{
		}

		[PXDBDecimal(BqlTable = typeof(ARTranPostGL))]
		public virtual decimal? TurnWOAmt { get; set; }

		#endregion

		#region TranType

		public abstract class tranType : PX.Data.BQL.BqlString.Field<tranType>
		{
		}

		[PXDBString(BqlTable = typeof(ARTranPostGL))]
		public virtual string TranType { get; set; }

		#endregion

		#region IsSelfApplyingVoidApplication
		public abstract class isSelfVoidingVoidApplication : PX.Data.BQL.BqlBool.Field<isSelfVoidingVoidApplication> { }

		[PXBool]
		[PXDBCalced(typeof(Switch<Case<
			Where<ARRegister2.voided.IsEqual<True>.And<ARTranPostGL.sourceDocType.IsEqual<ARDocType.smallBalanceWO>.Or<ARTranPostGL.sourceDocType.IsEqual<ARDocType.smallCreditWO>>>>, True>,
			False>), typeof(bool))]
		public virtual bool? IsSelfVoidingVoidApplication { get; set; }
		#endregion

		#region IsOrphanApplication
		public abstract class isOrphanApplication : PX.Data.BQL.BqlBool.Field<isOrphanApplication> { }

		[PXBool]
		[PXDependsOnFields(typeof(type), typeof(isDocumentPresent), typeof(isSourceDocumentPresent))]
		public virtual bool? IsOrphanApplication
		{
			get
			{
				switch (Type)
				{
					case ARTranPost.type.Adjustment: return !IsDocumentPresent;
					case ARTranPost.type.Application: return !IsSourceDocumentPresent;
					default: return false;
				}
			}
		}
		#endregion

		#region IsDocumentPresent
		public abstract class isDocumentPresent : PX.Data.BQL.BqlBool.Field<isDocumentPresent> { }

		[PXBool]
		[PXDBCalced(typeof(Switch<Case<Where<ARStatementDetail.statementDate.IsEqual<ARRegister.statementDate>>, True>, False>), typeof(bool))]
		public virtual bool? IsDocumentPresent { get; set; }
		#endregion

		#region IsSourceDocumentPresent
		public abstract class isSourceDocumentPresent : PX.Data.BQL.BqlBool.Field<isSourceDocumentPresent> { }

		[PXBool]
		[PXDBCalced(typeof(Switch<Case<Where<ARStatementDetail.statementDate.IsEqual<ARRegister2.statementDate>>, True>, False>), typeof(bool))]
		public virtual bool? IsSourceDocumentPresent { get; set; }
		#endregion

		#region CuryID
		public abstract new class curyID : PX.Data.BQL.BqlString.Field<curyID> { }

		[PXDBString(5, IsUnicode = true, InputMask = ">LLLLL", BqlTable = typeof(ARStatementDetail))]
		public virtual string CuryID { get; set; }
		#endregion

		#region SourceCuryID
		public abstract class sourceCuryID : PX.Data.BQL.BqlString.Field<curyID> { }

		[PXDBString(5, IsUnicode = true, InputMask = ">LLLLL", BqlTable = typeof(ARRegister2), BqlField = typeof(ARRegister2.curyID))]
		public virtual string SourceCuryID { get; set; }
		#endregion

		#region IsInterCurrencyApplication
		public abstract class isInterCurrencyApplication : PX.Data.BQL.BqlBool.Field<isInterCurrencyApplication> { }
		/// <summary>
		/// If set to <c>true</c>, indicates that the parent
		/// <see cref="ARAdjust">application</see> affects documents
		/// that have different <see cref="Currency">currencies</see>.
		/// </summary>
		[PXBool]
		[PXDependsOnFields(typeof(curyID), typeof(sourceCuryID))]
		public virtual bool? IsInterCurrencyApplication => CuryID != SourceCuryID;
		#endregion

		#region SourceBranchID

		public abstract class sourceBranchID : PX.Data.BQL.BqlInt.Field<sourceBranchID>
		{
		}

		[Branch(BqlTable = typeof(ARRegister2), BqlField = typeof(ARRegister2.branchID))]
		public virtual int? SourceBranchID { get; set; }

		#endregion

		#region IsInterBranchApplication
		public abstract class isInterBranchApplication : PX.Data.BQL.BqlBool.Field<isInterBranchApplication> { }
		/// <summary>
		/// If set to <c>true</c>, indicates that the parent 
		/// <see cref="ARAdjust">application</see> affects documents 
		/// that originate in different <see cref="Branch">branches</see>.
		/// </summary>
		[PXBool]
		[PXDependsOnFields(typeof(branchID), typeof(sourceBranchID))]
		public virtual bool? IsInterBranchApplication => BranchID != SourceBranchID;
		#endregion

		#region SourceCustomerID

		public abstract class sourceCustomerID : PX.Data.BQL.BqlInt.Field<sourceCustomerID>
		{
		}

		[Customer(BqlTable = typeof(ARRegister2), BqlField = typeof(ARRegister2.customerID))]
		public virtual int? SourceCustomerID { get; set; }

		#endregion

		#region IsInterCustomerApplication
		public abstract class isInterCustomerApplication : PX.Data.BQL.BqlBool.Field<isInterCustomerApplication> { }
		/// <summary>
		/// If set to <c>true</c>, indicates that the parent
		/// <see cref="ARAdjust">application</see> affects documents
		/// that belong to different <see cref="Customer">customers</see>.
		/// </summary>
		[PXBool]
		[PXDependsOnFields(typeof(customerID), typeof(sourceCustomerID))]
		public virtual bool? IsInterCustomerApplication => CustomerID != SourceCustomerID;
		#endregion

		#region IsInterStatementApplication
		public abstract class isInterStatementApplication : PX.Data.BQL.BqlBool.Field<isInterStatementApplication> { }
		/// <summary>
		/// If set to <c>true</c>, indicates that the parent
		/// <see cref="ARAdjust">application</see> affects two 
		/// <see cref="ARStatement">Customer Statements</see> at once.
		/// If and only if this flag is set to <c>true</c>, the ending balance
		/// of the parent statement will account for the application amount. 
		/// </summary>
		[PXBool]
		[PXDependsOnFields(
			typeof(isInterBranchApplication),
			typeof(isInterCurrencyApplication),
			typeof(isInterCustomerApplication))]
		public virtual bool? IsInterStatementApplication =>
			IsInterBranchApplication == true
			|| IsInterCurrencyApplication == true
			|| IsInterCustomerApplication == true;
		#endregion

		#region AdjgDocType
		public abstract class adjgDocType : PX.Data.BQL.BqlString.Field<adjgDocType> { }

		[PXDBString(3, IsKey = true, IsFixed = true, InputMask = "", BqlTable = typeof(ARAdjust))]
		[LabelList(typeof(ARDocType))]
		[PXDependsOnFields(typeof(type), typeof(docType), typeof(sourceDocType))]
		public virtual string AdjgDocType
		{
			get
			{
				switch (Type)
				{
					case ARTranPost.type.Adjustment: return DocType;
					case ARTranPost.type.Application: return SourceDocType;
					default: return null;
				}
			}
		}
		#endregion

		#region AdjgRefNbr
		public abstract class adjgRefNbr : PX.Data.BQL.BqlString.Field<adjgRefNbr> { }
		[PXString(15, IsKey = true, IsUnicode = true, InputMask = ">CCCCCCCCCCCCCCC", BqlTable = typeof(ARAdjust))]
		[PXDependsOnFields(typeof(type), typeof(docType), typeof(sourceDocType), typeof(isOrphanApplication))]
		public virtual string AdjgRefNbr
		{
			get
			{
				switch (Type)
				{
					case ARTranPost.type.Adjustment: return RefNbr;
					case ARTranPost.type.Application:
						if (IsDocumentPresent == true || IsOrphanApplication == true || IsInterStatementApplication == true)
							return SourceRefNbr;
						else return null;
					default: return null;
				}
			}
		}
		#endregion

		#region AdjdDocType
		public abstract class adjdDocType : PX.Data.BQL.BqlString.Field<adjdDocType> { }

		[PXString(3, IsKey = true, IsFixed = true, InputMask = "", BqlTable = typeof(ARAdjust))]
		[LabelList(typeof(ARDocType))]
		[PXDependsOnFields(typeof(type), typeof(docType), typeof(sourceDocType))]
		public virtual string AdjdDocType
		{
			get
			{
				switch (Type)
				{
					case ARTranPost.type.Adjustment: return SourceDocType;
					case ARTranPost.type.Application: return DocType;
					default: return null;
				}
			}
		}
		#endregion

		#region AdjdRefNbr
		public abstract class adjdRefNbr : PX.Data.BQL.BqlString.Field<adjdRefNbr> { }

		[PXString(15, IsUnicode = true, InputMask = ">CCCCCCCCCCCCCCC")]
		[PXDependsOnFields(typeof(type), typeof(docType), typeof(refNbr), typeof(sourceRefNbr))]
		public virtual string AdjdRefNbr
		{
			get
			{
				if(DocType != null && string.IsNullOrWhiteSpace(DocType)
					&& RefNbr != null && string.IsNullOrWhiteSpace(RefNbr))
				{
					return string.Empty;
				}
				switch (Type)
				{
					case ARTranPost.type.Adjustment: return SourceRefNbr;
					case ARTranPost.type.Application: return RefNbr;
					default: return null;
				}
			}
		}
		#endregion

		#region AdjdCustomerID
		public abstract class adjdCustomerID : PX.Data.BQL.BqlInt.Field<adjdCustomerID> { }

		[PXDefault]
		[Customer]
		[PXDependsOnFields(typeof(type), typeof(customerID), typeof(sourceCustomerID))]
		public virtual int? AdjdCustomerID
		{
			get
			{
				switch (Type)
				{
					case ARTranPost.type.Adjustment: return SourceCustomerID;
					case ARTranPost.type.Application: return CustomerID;
					default: return null;
				}
			}
		}
		#endregion

		#region AdjdCustomerID
		public abstract class adjgCustomerID : PX.Data.BQL.BqlInt.Field<adjgCustomerID> { }

		[PXDefault]
		[Customer]
		[PXDependsOnFields(typeof(type), typeof(customerID), typeof(sourceCustomerID))]
		public virtual int? AdjgCustomerID
		{
			get
			{
				switch (Type)
				{
					case ARTranPost.type.Adjustment: return CustomerID;
					case ARTranPost.type.Application: return SourceCustomerID;
					default: return null;
				}
			}
		}
		#endregion

		#region AdjdBranchID

		public abstract class adjdbranchID : PX.Data.BQL.BqlInt.Field<adjdbranchID>
		{
		}

		[Branch]
		[PXDependsOnFields(typeof(type), typeof(branchID), typeof(sourceBranchID))]
		public virtual int? AdjdBranchID
		{
			get
			{
				switch (Type)
				{
					case ARTranPost.type.Adjustment: return SourceBranchID;
					case ARTranPost.type.Application: return BranchID;
					default: return null;
				}
			}
		}

		#endregion

		#region AdjgBranchID

		public abstract class adjgbranchID : PX.Data.BQL.BqlInt.Field<adjgbranchID>
		{
		}

		[Branch]
		[PXDependsOnFields(typeof(type), typeof(branchID), typeof(sourceBranchID))]
		public virtual int? AdjgBranchID
		{
			get
			{
				switch (Type)
				{
					case ARTranPost.type.Adjustment: return BranchID;
					case ARTranPost.type.Application: return SourceBranchID;
					default: return null;
				}
			}
		}

		#endregion

		#region AdjdCuryID
		public abstract class adjdCuryID : PX.Data.BQL.BqlString.Field<adjdCuryID> { }

		[PXString]
		[PXDependsOnFields(typeof(type), typeof(curyID), typeof(sourceCuryID))]
		public virtual string AdjdCuryID
		{
			get
			{
				switch (Type)
				{
					case ARTranPost.type.Adjustment: return SourceCuryID;
					case ARTranPost.type.Application: return CuryID;
					default: return null;
				}
			}
		}
		#endregion

		#region AdjgCuryID
		public abstract class adjgCuryID : PX.Data.BQL.BqlString.Field<adjgCuryID> { }

		[PXString]
		[PXDependsOnFields(typeof(type), typeof(curyID), typeof(sourceCuryID))]
		public virtual string AdjgCuryID
		{
			get
			{
				switch (Type)
				{
					case ARTranPost.type.Adjustment: return CuryID;
					case ARTranPost.type.Application: return SourceCuryID;
					default: return null;
				}
			}
		}
		#endregion

		#region SignBalanceDelta
		/// <summary>
		/// The sign with which the application amount affects the
		/// <see cref="ARStatement">customer statement</see> balance.
		/// </summary>
		public abstract class signBalanceDelta : PX.Data.BQL.BqlDecimal.Field<signBalanceDelta> { }
		[PXDecimal]
		[PXDependsOnFields(
			typeof(isInterStatementApplication),
			typeof(adjgDocType),
			typeof(adjdDocType))]
		public virtual decimal? SignBalanceDelta
		{
			get
			{
				decimal sign = IsInterStatementApplication == true ? 1m : -1m;

				// For these types of applications, the balance
				// of the statement is affected in a reverse manner.
				bool flipSign =
					AdjgDocType == ARDocType.SmallBalanceWO
					|| AdjdDocType == ARDocType.SmallCreditWO
						&& (AdjgDocType == ARDocType.Payment || AdjgDocType == ARDocType.CreditMemo);

				return flipSign ? -sign : sign;
			}
		}
		#endregion
	}
}
