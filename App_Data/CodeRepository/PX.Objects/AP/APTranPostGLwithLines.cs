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
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.AP.DAC;
using PX.Objects.CM.Extensions;
using PX.Objects.CS;
using PX.Objects.GL;
using PX.Objects.GL.FinPeriods.TableDefinition;
using PX.Objects.PM;
using System;
using static PX.Objects.AP.CalcAPTranGLwithLinesReport;

namespace PX.Objects.AP
{
	/// <summary>
	/// AP Document Post GL with Lines.
	/// </summary>
	[PXProjection(typeof(SelectFrom<APRegisterReport>
		.CrossJoin<APAROrd>
		.LeftJoin<APTran>
			.On<APRegisterReport.docType.IsEqual<APTran.tranType>
				.And<APRegisterReport.refNbr.IsEqual<APTran.refNbr>>
				.And<APRegisterReport.paymentsByLinesAllowed.IsEqual<True>>>
		.LeftJoin<APTranPostGL>
			.On<APRegisterReport.docType.IsEqual<APTranPostGL.docType>
				.And<APRegisterReport.refNbr.IsEqual<APTranPostGL.refNbr>>
				.And<APTranPostGL.lineNbr.IsEqual<APTran.lineNbr>.Or<APTranPostGL.lineNbr.IsEqual<Zero>>>
				.And<APTranPostGL.type.IsNotEqual<APTranPost.type.origin>>
				.And<APAROrd.ord.IsEqual<decimal1>>>), Persistent = false)]
	[PXCacheName("AP Document Post GL with Lines")]
	public class APTranPostGLwithLines : IBqlTable
	{
		#region Keys

		public class PK : PrimaryKeyOf<APTranPostGLwithLines>.By<projectID, docType, refNbr, ord, lineNbr, iD>
		{
			public static APTranPostGLwithLines Find(PXGraph graph, int projectID, string docType, string refNbr, int? ord, int? lineNbr, int? iD) =>
				FindBy(graph, projectID, docType, refNbr, ord, lineNbr, iD);
		}

		#endregion

		#region DocType

		public abstract class docType : PX.Data.BQL.BqlString.Field<docType>
		{
		}

		/// <summary>
		/// The type of the document.
		/// </summary>
		/// <value>
		/// The field can have one of the following values:
		/// <list>
		/// <item><description>INV: Invoice</description></item>
		/// <item><description>ACR: Credit Adjustment</description></item>
		/// <item><description>ADR: Debit Adjustment</description></item>
		/// <item><description>CHK: Payment</description></item>
		/// <item><description>VCK: Voided Payment</description></item>
		/// <item><description>PPM: Prepayment</description></item>
		/// <item><description>REF: Refund</description></item>
		/// <item><description>VRF: Voided Refund</description></item>
		/// <item><description>QCK: Cash Purchase</description></item>
		/// <item><description>VQC: Voided Cash Purchase</description></item>
		/// </list>
		/// </value>
		[PXDBString(IsKey = true, BqlTable = typeof(APRegisterReport))]
		[PXUIField(DisplayName = "Doc. Type")]
		[APDocType.List()]
		public virtual string DocType { get; set; }

		#endregion

		#region RefNbr
		public abstract class refNbr : PX.Data.BQL.BqlString.Field<refNbr>
		{
		}

		/// <summary>
		/// Reference number of the document.
		/// </summary>
		[PXDBString(IsKey = true, BqlTable = typeof(APRegisterReport))]
		[PXUIField(DisplayName = "Ref. Nbr.", Visibility = PXUIVisibility.Visible, Enabled = false)]
		public virtual string RefNbr { get; set; }
		#endregion

		#region OrigDocType
		public abstract class origDocType : PX.Data.BQL.BqlString.Field<origDocType> { }

		/// <summary>
		/// Type of the original (source) document.
		/// </summary>
		[PXDBString(3, IsFixed = true, BqlTable = typeof(APRegisterReport))]
		[APDocType.List()]
		[PXUIField(DisplayName = "Orig. Doc. Type")]
		public virtual String OrigDocType { get; set; }
		#endregion

		#region OrigRefNbr
		public abstract class origRefNbr : PX.Data.BQL.BqlString.Field<origRefNbr> { }

		/// <summary>
		/// Reference number of the original (source) document.
		/// </summary>
		[PXDBString(15, IsUnicode = true, InputMask = "", BqlTable = typeof(APRegisterReport))]
		[PXUIField(DisplayName = "Orig. Ref. Nbr.")]
		public virtual string OrigRefNbr { get; set; }
		#endregion

		#region TranType
		public abstract class tranType : PX.Data.BQL.BqlString.Field<tranType> { }

		/// <summary>
		/// The type of the transaction.
		/// </summary>
		[PXString]
		[PXDBCalced(typeof(
				IsNull<APTranPostGL.tranType, APTran.tranType>), typeof(String))]
		public virtual String TranType { get; set; }

		#endregion

		#region Ord
		public abstract class ord : PX.Data.BQL.BqlShort.Field<ord> { }

		/// <summary>
		/// The field is used in reports for joining and filtering purposes.
		/// </summary>
		[PXDBShort(IsKey = true, BqlTable = typeof(APAROrd))]
		public virtual Int16? Ord { get; set; }
		#endregion

		#region PrintDocType
		public abstract class printDocType : PX.Data.BQL.BqlString.Field<printDocType> { }

		/// <summary>
		/// Type of the document for displaying in reports.
		/// This field has the same set of possible internal values as the <see cref="DocType"/> field,
		/// but exposes different user-friendly values.
		/// </summary>
		[PXString(3, IsFixed = true)]
		[APDocType.PrintList()]
		[PXUIField(DisplayName = "Type", Visibility = PXUIVisibility.Visible, Enabled = true)]
		public virtual String PrintDocType
		{
			get
			{
				return this.DocType;
			}
			set
			{
			}
		}
		#endregion

		#region LineNbr
		public abstract class lineNbr : PX.Data.BQL.BqlInt.Field<lineNbr> {	}

		/// <summary>
		/// The number of the transaction line in the document.
		/// </summary>
		[PXInt(IsKey = true)]
		[PXUIField(DisplayName = "Line Nbr.")]
		[PXDBCalced(typeof(
				IsNull<APTranPostGL.lineNbr, IsNull<APTran.lineNbr, Zero>>), typeof(Int32))]
		public virtual Int32? LineNbr { get; set; }
		#endregion

		#region ID

		public abstract class iD : PX.Data.BQL.BqlInt.Field<iD>
		{
		}

		/// <summary>
		/// Id of corresponding APTranPost table record.
		/// </summary>
		[PXInt(IsKey = true)]
		[PXDBCalced(typeof(
				IsNull<APTranPostGL.iD, Zero>), typeof(Int32))]
		public virtual int? ID { get; set; }

		#endregion

		#region ProjectID
		public abstract class projectID : PX.Data.BQL.BqlInt.Field<projectID>
		{
		}

		/// <summary>
		/// The <see cref="PX.Objects.PM.PMProject">project</see> with which the item is associated or the non-project code if the item is not intended for any project.
		/// The field is relevant only if the <see cref="PX.Objects.CS.FeaturesSet.ProjectModule">Project Module</see> is enabled.
		/// </summary>
		[PXInt(IsKey = true)]
		[PXDBCalced(typeof(
				IsNull<APTran.projectID, APRegisterReport.projectID>), typeof(Int32))]
		public virtual Int32? ProjectID { get; set; }
		#endregion

		#region Released
		public abstract class released : PX.Data.BQL.BqlBool.Field<released> { }

		/// <summary>
		/// When set to <c>true</c> indicates that the document was released.
		/// </summary>
		[Released(PreventDeletingReleased = true, BqlTable = typeof(APRegisterReport))]
		public virtual Boolean? Released { get; set; }
		#endregion

		#region OpenDoc
		public abstract class openDoc : PX.Data.BQL.BqlBool.Field<openDoc> { }

		/// <summary>
		/// When set to <c>true</c> indicates that the document is open.
		/// </summary>
		[PXDBBool(BqlTable = typeof(APRegisterReport))]
		public virtual Boolean? OpenDoc { get; set; }
		#endregion

		#region Prebooked
		public abstract class prebooked : PX.Data.BQL.BqlBool.Field<prebooked> { }

		/// <summary>
		/// When set to <c>true</c> indicates that the document was prebooked.
		/// </summary>
		[PXDBBool(BqlTable = typeof(APRegisterReport))]
		public virtual Boolean? Prebooked { get; set; }
		#endregion

		#region Voided
		public abstract class voided : PX.Data.BQL.BqlBool.Field<voided> { }

		/// <summary>
		/// When set to <c>true</c> indicates that the document was voided. In this case <see cref="VoidBatchNbr"/> field will hold the number of the voiding <see cref="Batch"/>.
		/// </summary>
		[PXDBBool(BqlTable = typeof(APRegisterReport))]
		public virtual Boolean? Voided { get; set; }
		#endregion

		#region VendorID
		public abstract class vendorID : PX.Data.BQL.BqlInt.Field<vendorID> { }

		/// <summary>
		/// Identifier of the <see cref="Vendor"/>, whom the document belongs to.
		/// </summary>
		[VendorActive(
			Visibility = PXUIVisibility.SelectorVisible,
			DescriptionField = typeof(Vendor.acctName),
			CacheGlobal = true,
			Filterable = true,
			BqlTable = typeof(APRegisterReport))]
		[PXDefault]

		public virtual int? VendorID
		{
			get;
			set;
		}
		#endregion

		#region BranchID
		public abstract class branchID : PX.Data.BQL.BqlInt.Field<branchID> { }

		/// <summary>
		/// Identifier of the <see cref="PX.Objects.GL.Branch">Branch</see>, to which the document belongs.
		/// </summary>
		[GL.Branch(BqlTable = typeof(APRegisterReport))]
		public virtual Int32? BranchID { get; set; }
		#endregion

		#region ClosedFinPeriodID
		public abstract class closedFinPeriodID : PX.Data.BQL.BqlString.Field<closedFinPeriodID> { }

		/// <summary>
		/// The <see cref="PX.Objects.GL.FinancialPeriod">Financial Period</see>, in which the document was closed.
		/// </summary>
		/// <value>
		/// Corresponds to the <see cref="FinPeriodID"/> field.
		/// </value>
		[FinPeriodID(BqlTable = typeof(APRegisterReport))]
		public virtual String ClosedFinPeriodID { get; set; }
		#endregion

		#region ClosedTranPeriodID
		public abstract class closedTranPeriodID : PX.Data.BQL.BqlString.Field<closedTranPeriodID> { }

		/// <summary>
		/// The <see cref="PX.Objects.GL.FinancialPeriod">Financial Period</see>, in which the document was closed.
		/// </summary>
		/// <value>
		/// Corresponds to the <see cref="TranPeriodID"/> field.
		/// </value>
		[PeriodID(BqlTable = typeof(APRegisterReport))]
		public virtual String ClosedTranPeriodID { get; set; }
		#endregion

		#region SourceDocType

		public abstract class sourceDocType : PX.Data.BQL.BqlString.Field<sourceDocType>
		{
		}

		/// <summary>
		/// Source document doc type.
		/// </summary>
		[PXDBString(BqlTable = typeof(APTranPostGL))]
		[PXUIField(DisplayName = "Source Doc. Type")]
		[APDocType.List()]

		public virtual string SourceDocType { get; set; }

		#endregion

		#region SourceRefNbr

		public abstract class sourceRefNbr : PX.Data.BQL.BqlString.Field<sourceRefNbr>
		{
		}

		/// <summary>
		/// Source document ref number.
		/// </summary>
		[PXDBString(BqlTable = typeof(APTranPostGL))]
		[PXUIField(DisplayName = "Source Ref. Nbr.", Visibility = PXUIVisibility.Visible, Enabled = false)]
		public virtual string SourceRefNbr { get; set; }

		#endregion

		#region CuryID

		public abstract class curyID : PX.Data.BQL.BqlLong.Field<curyID> { }

		/// <summary>
		/// Identifier of the <see cref="Currency"/> of this Currency Info object.
		/// </summary>
		[PXDBString(BqlTable = typeof(CurrencyInfo))]
		public virtual string CuryID { get; set; }

		#endregion

		#region CuryInfoID

		public abstract class curyInfoID : PX.Data.BQL.BqlLong.Field<curyInfoID> { }

		/// <summary>
		/// Identifier of the <see cref="PX.Objects.CM.CurrencyInfo">CurrencyInfo</see> object associated with the document.
		/// </summary>
		[PXDBLong(BqlTable = typeof(APTranPostGL))]
		public virtual long? CuryInfoID { get; set; }

		#endregion

		#region AccountID

		public abstract class accountID : PX.Data.BQL.BqlInt.Field<accountID>
		{
		}

		/// <summary>
		/// AccountID of corresponding APTranPostGL table record.
		/// </summary>
		[Account(BqlTable = typeof(APTranPostGL))]
		public virtual int? AccountID { get; set; }

		#endregion


		#region SubID

		public abstract class subID : PX.Data.BQL.BqlInt.Field<subID>
		{
		}

		/// <summary>
		/// SubID Account ID of corresponding APTranPostGL table record.
		/// </summary>
		[SubAccount(BqlTable = typeof(APTranPostGL))]
		public virtual int? SubID { get; set; }

		#endregion

		#region FinPeriodID
		public abstract class finPeriodID : PX.Data.BQL.BqlString.Field<finPeriodID> { }

		/// <summary>
		/// <see cref="FinPeriod">Financial Period</see> of the document.
		/// </summary>
		[PXString]
		[PXDBCalced(typeof(
			Switch<
				Case<Where<APAROrd.ord.IsEqual<Zero>>,
					APRegisterReport.finPeriodID>,
				APTranPostGL.finPeriodID>
		), typeof(string))]
		[PXUIField(DisplayName = "Application Period")]
		public virtual string FinPeriodID { get; set; }

		#endregion

		#region TranPeriodID
		public abstract class tranPeriodID : PX.Data.BQL.BqlString.Field<tranPeriodID> { }

		/// <summary>
		/// <see cref="FinPeriod">Financial Period</see> of the document.
		/// </summary>
		[PXString]
		[PXDBCalced(typeof(
			Switch<
				Case<Where<APAROrd.ord.IsEqual<Zero>>,
					APRegisterReport.tranPeriodID>,
				APTranPostGL.tranPeriodID>
		), typeof(string))]
		public virtual string TranPeriodID { get; set; }
		#endregion

		#region APRegisterTranPeriodID
		public abstract class aPRegisterTranPeriodID : PX.Data.BQL.BqlString.Field<aPRegisterTranPeriodID> { }

		/// <summary>
		/// <see cref="FinPeriod">Financial Period</see> of the document.
		/// </summary>
		/// <value>
		[PeriodID(BqlField = typeof(APRegisterReport.tranPeriodID))]
		public virtual string APRegisterTranPeriodID { get; set; }
		#endregion

		#region APRegisterDocDate
		public abstract class aPRegisterDocDate : PX.Data.BQL.BqlDateTime.Field<aPRegisterDocDate> { }

		/// <summary>
		/// Date of the document.
		/// </summary>
		[PXDBDate(BqlField = typeof(APRegisterReport.docDate))]
		[PXUIField(DisplayName = "Date", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual DateTime? APRegisterDocDate { get; set; }
		#endregion

		#region DocDate
		public abstract class docDate : PX.Data.BQL.BqlDateTime.Field<docDate> { }

		/// <summary>
		/// Date of the document.
		/// </summary>
		[PXDate]
		[PXDBCalced(typeof(
			Switch<
				Case<Where<APAROrd.ord.IsEqual<Zero>>,
					APRegisterReport.docDate>,
				APTranPostGL.docDate>
		), typeof(DateTime?))]
		[PXUIField(DisplayName = "Date", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual DateTime? DocDate { get; set; }
		#endregion

		#region ClosedDate
		public abstract class closedDate : PX.Data.BQL.BqlDateTime.Field<closedDate> { }

		/// <summary>
		/// The date of the last application.
		/// </summary>
		[PXDBDate(BqlTable = typeof(APRegisterReport))]
		public virtual DateTime? ClosedDate { get; set; }
		#endregion

		#region BalanceSign
		public abstract class balanceSign : PX.Data.BQL.BqlShort.Field<balanceSign> { }

		/// <summary>
		/// Sign of the Balance.
		/// </summary>
		[PXDBShort(BqlTable = typeof(APTranPostGL))]
		public virtual short? BalanceSign { get; set; }
		#endregion

		#region Type

		public abstract class type : PX.Data.BQL.BqlString.Field<type>
		{
		}

		/// <summary>
		/// Transaction type.
		/// </summary>
		[PXDBString(BqlTable = typeof(APTranPostGL))]
		[APTranPost.type.List]
		public virtual string Type { get; set; }

		#endregion

		#region TranClass

		public abstract class tranClass : PX.Data.BQL.BqlString.Field<tranClass>
		{
		}

		/// <summary>
		/// Transaction class.
		/// </summary>
		[PXDBString(BqlTable = typeof(APTranPostGL))]
		public virtual string TranClass { get; set; }

		#endregion

		#region TranRefNbr

		public abstract class tranRefNbr : PX.Data.BQL.BqlString.Field<tranRefNbr>
		{
		}

		/// <summary>
		/// Transaction ref number.
		/// </summary>
		[PXDBString(BqlTable = typeof(APTranPostGL))]
		public virtual string TranRefNbr { get; set; }

		#endregion

		#region ReferenceID

		public abstract class referenceID : PX.Data.BQL.BqlInt.Field<referenceID>
		{
		}

		/// <summary>
		/// Transaction Reference ID.
		/// </summary>
		[PXDBInt(BqlTable = typeof(APTranPostGL))]
		public virtual int? ReferenceID { get; set; }

		#endregion

		#region RefNoteID
		public new abstract class refNoteID : PX.Data.BQL.BqlGuid.Field<refNoteID> { }

		/// <summary>
		/// AP document transaction note id.
		/// </summary>
		[PXDBGuidAttribute(BqlTable = typeof(APTranPostGL))]
		public virtual Guid? RefNoteID
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
		[PXDBBool(BqlTable = typeof(APTranPostGL))]
		public virtual bool? IsMigratedRecord { get; set; }
		#endregion

		#region PaymentsByLinesAllowed
		public abstract class paymentsByLinesAllowed : PX.Data.BQL.BqlBool.Field<paymentsByLinesAllowed> { }

		/// <summary>
		/// Specifies (if set to <c>true</c>) that the record has been created 
		/// with activated <see cref="FeaturesSet.PaymentsByLines"/> feature and
		/// such document allow payments by lines.
		/// </summary>
		[PXDBBool(BqlTable = typeof(APRegisterReport))]
		[PXDefault(false)]
		public virtual bool? PaymentsByLinesAllowed
		{
			get;
			set;
		}
		#endregion

		#region OrigBalanceAmt

		public abstract class origBalanceAmt : PX.Data.BQL.BqlDecimal.Field<origBalanceAmt>
		{
		}

		/// <summary>
		/// The signed amount to be paid for the document in the base currency of the company. (See <see cref="Company.BaseCuryID"/>)
		/// </summary>
		[PXBaseCury]
		[PXDBCalced(typeof(
			Switch<
				Case<Where<APAROrd.ord.IsEqual<Zero>>,
					IsNull<APRegisterReport.signBalance.Multiply<APTran.origTranAmt>, APRegisterReport.signBalance.Multiply<APRegisterReport.origDocAmt>>>,
				Zero>
		), typeof(decimal))]
		public virtual decimal? OrigBalanceAmt { get; set; }

		#endregion

		#region BalanceAmt

		public abstract class balanceAmt : PX.Data.BQL.BqlDecimal.Field<balanceAmt>
		{
		}

		/// <summary>
		/// Balance amtount.
		/// </summary>
		[PXBaseCury]
		[PXDBCalced(typeof(
			Switch<
				Case<Where<APAROrd.ord.IsEqual<decimal1>>,
					APTranPostGL.balanceAmt>,
				Zero>
		), typeof(decimal))]
		public virtual decimal? BalanceAmt { get; set; }

		#endregion

		#region CuryDebitAPAmt
		public abstract class curyDebitAPAmt : PX.Data.BQL.BqlDecimal.Field<curyDebitAPAmt>
		{
		}

		/// <summary>
		/// Debit AP amount.
		/// </summary>
		[PXCurrency(typeof(curyInfoID), typeof(debitAPAmt), BaseCalc = false, BqlTable = typeof(APTranPostGL))]
		[PXUIField(DisplayName = "Debit AP Amt.")]
		public virtual decimal? CuryDebitAPAmt { get; set; }

		#endregion

		#region DebitAPAmt

		public abstract class debitAPAmt : PX.Data.BQL.BqlDecimal.Field<debitAPAmt>
		{
		}

		/// <summary>
		/// Debit AP amount in the base currency of the company.
		/// </summary>
		[PXBaseCury(BqlTable = typeof(APTranPostGL))]
		public virtual decimal? DebitAPAmt { get; set; }

		#endregion

		#region CuryCreditAPAmt

		public abstract class curyCreditAPAmt : PX.Data.BQL.BqlDecimal.Field<curyCreditAPAmt>
		{
		}

		/// <summary>
		/// Credit AP amount.
		/// </summary>
		[PXCurrency(typeof(curyInfoID), typeof(creditAPAmt), BaseCalc = false, BqlTable = typeof(APTranPostGL))]
		[PXUIField(DisplayName = "Credit AP Amt.")]
		public virtual decimal? CuryCreditAPAmt { get; set; }

		#endregion

		#region CreditAPAmt

		public abstract class creditAPAmt : PX.Data.BQL.BqlDecimal.Field<creditAPAmt>
		{
		}

		/// <summary>
		/// Credit AP amount in the base currency of the company.
		/// </summary>
		[PXBaseCury(BqlTable = typeof(APTranPostGL))]
		public virtual decimal? CreditAPAmt { get; set; }

		#endregion

		#region CuryTurnDiscAmt

		public abstract class curyTurnDiscAmt : PX.Data.BQL.BqlDecimal.Field<curyTurnDiscAmt>
		{
		}

		/// <exclude/>
		[PXCurrency(typeof(curyInfoID), typeof(turnDiscAmt), BaseCalc = false, BqlTable = typeof(APTranPostGL))]
		public virtual decimal? CuryTurnDiscAmt { get; set; }

		#endregion

		#region TurnDiscAmt

		public abstract class turnDiscAmt : PX.Data.BQL.BqlDecimal.Field<turnDiscAmt>
		{
		}

		/// <exclude/>
		[PXBaseCury(BqlTable = typeof(APTranPostGL))]
		public virtual decimal? TurnDiscAmt { get; set; }

		#endregion

		#region CuryTurnWHTaxAmt

		public abstract class curyTurnWhTaxAmt : PX.Data.BQL.BqlDecimal.Field<curyTurnWhTaxAmt>
		{
		}

		/// <exclude/>
		[PXCurrency(typeof(curyInfoID), typeof(turnWhTaxAmt), BaseCalc = false, BqlTable = typeof(APTranPostGL))]
		public virtual decimal? CuryTurnWHTaxAmt { get; set; }

		#endregion

		#region TurnWHTaxAmt

		public abstract class turnWhTaxAmt : PX.Data.BQL.BqlDecimal.Field<turnWhTaxAmt>
		{
		}

		/// <exclude/>
		[PXBaseCury(BqlTable = typeof(APTranPostGL))]
		public virtual decimal? TurnWHTaxAmt { get; set; }

		#endregion

		#region TurnRGOLAmt

		public abstract class turnRGOLAmt : PX.Data.BQL.BqlDecimal.Field<turnRGOLAmt>
		{
		}

		/// <exclude/>
		[PXBaseCury(BqlTable = typeof(APTranPostGL))]
		public virtual decimal? TurnRGOLAmt { get; set; }

		#endregion

		#region RGOLAmt
		public abstract class rGOLAmt : PX.Data.BQL.BqlDecimal.Field<rGOLAmt>
		{
		}

		/// <summary>
		/// Realized gains or losses amount.
		/// </summary>
		[PXDBBaseCury(BqlTable = typeof(APTranPostGL))]
		public virtual decimal? RGOLAmt { get; set; }
		#endregion

		#region OrigRetainageAmt
		public abstract class origRetainageAmt : PX.Data.BQL.BqlDecimal.Field<origRetainageAmt> { }

		/// <summary>
		/// Original retainage amount.
		/// </summary>
		[PXBaseCury]
		[PXDBCalced(typeof(
			Mult<APRegisterReport.signAmount,
				Switch<
					Case<Where<APAROrd.ord.IsEqual<Zero>>,
						IsNull<APTran.origRetainageAmt, APRegisterReport.retainageTotal>>,
					Zero>>), typeof(decimal))]
		[PXUIField(DisplayName = "Original Retainage", FieldClass = nameof(FeaturesSet.Retainage))]
		public virtual decimal? OrigRetainageAmt { get; set; }
		#endregion

		#region ReleasedRetainageAmt
		public abstract class releasedRetainageAmt : PX.Data.BQL.BqlDecimal.Field<releasedRetainageAmt> { }

		/// <summary>
		/// Released retainage amount.
		/// </summary>
		[PXBaseCury]
		[PXDBCalced(typeof(
			Switch<
				Case<Where<APAROrd.ord.IsEqual<decimal1>>,
					APTranPostGL.balanceSign.Multiply<APTranPostGL.retainageReleasedAmt>>,
				Zero>
		), typeof(decimal))]
		public virtual decimal? ReleasedRetainageAmt { get; set; }
		#endregion
	}

	/// <summary>
	/// Aggrigate AP Document Post GL with Lines.
	/// </summary>
	[PXProjection(typeof(SelectFrom<APTranPostGLwithLines>
			.CrossJoin<PX.SM.DateInfo>
		.Where<APTranPostGLwithLines.docDate.IsLessEqual<PX.SM.DateInfo.date>
			.And<APTranPostGLwithLines.closedDate.IsGreater<PX.SM.DateInfo.date>
				.Or<APTranPostGLwithLines.closedDate.IsNull>>
			.And<APTranPostGLwithLines.released.IsEqual<True>
				.Or<APTranPostGLwithLines.openDoc.IsEqual<True>>
				.Or<APTranPostGLwithLines.prebooked.IsEqual<True>>
				.And<APTranPostGLwithLines.voided.IsNotEqual<True>>>>
		.AggregateTo<
			GroupBy<APTranPostGLwithLines.projectID>,
			GroupBy<APTranPostGLwithLines.docType>,
			GroupBy<APTranPostGLwithLines.refNbr>,
			Sum<APTranPostGLwithLines.origBalanceAmt>,
			Sum<APTranPostGLwithLines.balanceAmt>,
			Sum<APTranPostGLwithLines.origRetainageAmt>,
			Sum<APTranPostGLwithLines.releasedRetainageAmt>>))]
	[PXCacheName("Aggrigate AP Document Post GL with Lines")]
	public class CalcAPTranGLwithLinesReport : IBqlTable
	{
		#region ProjectID
		public abstract class projectID : PX.Data.BQL.BqlInt.Field<projectID> { }

		/// <summary>
		/// The <see cref="PX.Objects.PM.PMProject">project</see> with which the item is associated or the non-project code if the item is not intended for any project.
		/// The field is relevant only if the <see cref="PX.Objects.CS.FeaturesSet.ProjectModule">Project Module</see> is enabled.
		/// </summary>
		[PXDBInt(IsKey = true, BqlField = typeof(APTranPostGLwithLines.projectID))]
		public virtual Int32? ProjectID { get; set; }
		#endregion

		#region DocType

		public abstract class docType : PX.Data.BQL.BqlString.Field<docType> { }

		/// <summary>
		/// The type of the document.
		/// </summary>		
		[PXDBString(IsKey = true, BqlTable = typeof(APTranPostGLwithLines))]
		[PXUIField(DisplayName = "Doc. Type")]
		public virtual string DocType { get; set; }
		#endregion

		#region RefNbr
		public abstract class refNbr : PX.Data.BQL.BqlString.Field<refNbr> { }

		/// <summary>
		/// Reference number of the document.
		/// </summary>
		[PXDBString(IsKey = true, BqlTable = typeof(APTranPostGLwithLines))]
		[PXUIField(DisplayName = "Ref. Nbr.", Visibility = PXUIVisibility.Visible, Enabled = false)]
		public virtual string RefNbr { get; set; }
		#endregion

		#region OrigDocType
		public abstract class origDocType : PX.Data.BQL.BqlString.Field<origDocType> { }

		/// <summary>
		/// Type of the original (source) document.
		/// </summary>
		[PXDBString(IsKey = true, BqlTable = typeof(APTranPostGLwithLines))]
		[PXUIField(DisplayName = "Orig Doc. Type")]
		public virtual string OrigDocType { get; set; }
		#endregion

		#region OrigRefNbr
		public abstract class origRefNbr : PX.Data.BQL.BqlString.Field<origRefNbr> { }

		/// <summary>
		/// Reference number of the original (source) document.
		/// </summary>
		[PXDBString(IsKey = true, BqlTable = typeof(APTranPostGLwithLines))]
		[PXUIField(DisplayName = "Orig Ref. Nbr.", Visibility = PXUIVisibility.Visible, Enabled = false)]
		public virtual string OrigRefNbr { get; set; }
		#endregion

		#region AgingDate
		public abstract class agingDate : PX.Data.BQL.BqlType<Data.BQL.IBqlDateTime, DateTime>.Field<agingDate> { }

		/// <summary>
		/// Aging date.
		/// </summary>
		[PXDBDate(IsKey = true, BqlField = typeof(PX.SM.DateInfo.date))]
		public virtual DateTime? AgingDate { get; set; }
		#endregion

		#region OrigBalanceAmt
		public abstract class origBalanceAmt : PX.Data.BQL.BqlDecimal.Field<origBalanceAmt> { }

		/// <summary>
		/// The signed amount to be paid for the document in the base currency of the company. (See <see cref="Company.BaseCuryID"/>)
		/// </summary>
		[PXDBBaseCury(BqlTable = typeof(APTranPostGLwithLines))]
		public virtual decimal? OrigBalanceAmt { get; set; }
		#endregion

		#region BalanceAmt
		public abstract class balanceAmt : PX.Data.BQL.BqlDecimal.Field<balanceAmt> { }

		/// <summary>
		/// Balance amtount.
		/// </summary>
		[PXDBBaseCury(BqlTable = typeof(APTranPostGLwithLines))]
		public virtual decimal? BalanceAmt { get; set; }

		#endregion

		#region OrigRetainageAmt
		public abstract class origRetainageAmt : PX.Data.BQL.BqlDecimal.Field<origRetainageAmt> { }

		/// <summary>
		/// Original retainage amount.
		/// </summary>
		[PXDBBaseCury(BqlTable = typeof(APTranPostGLwithLines))]
		public virtual decimal? OrigRetainageAmt { get; set; }
		#endregion

		#region ReleasedRetainageAmt
		public abstract class releasedRetainageAmt : PX.Data.BQL.BqlDecimal.Field<releasedRetainageAmt> { }

		/// <summary>
		/// Released retainage amount.
		/// </summary>
		[PXDBBaseCury(BqlTable = typeof(APTranPostGLwithLines))]
		public virtual decimal? ReleasedRetainageAmt { get; set; }
		#endregion
	}
}
