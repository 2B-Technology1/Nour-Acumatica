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

using PX.Common;

using PX.Data;
using PX.Data.BQL;
using PX.Data.EP;

using PX.Objects.GL;
using PX.Objects.CM.Extensions;
using PX.Objects.CS;
using PX.Objects.TX;
using PX.Objects.CR;
using PX.Objects.CA;

using CRLocation = PX.Objects.CR.Standalone.Location;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Data.WorkflowAPI;
using PX.Objects.AP.MigrationMode;
using PX.Objects.AR;
using PX.Data.BQL.Fluent;
using PX.Objects.Common;
using PX.Objects.Common.Attributes;
using PX.Objects.Common;

namespace PX.Objects.AP
{
	public class APInvoiceType : APDocType
	{
		/// <summary>
		/// Specialized selector for APInvoice RefNbr.<br/>
		/// By default, defines the following set of columns for the selector:<br/>
		/// APInvoice.refNbr,APInvoice.docDate, APInvoice.finPeriodID,<br/>
		/// APInvoice.vendorID, APInvoice.vendorID_Vendor_acctName,<br/>
		/// APInvoice.vendorLocationID, APInvoice.curyID, APInvoice.curyOrigDocAmt,<br/>
		/// APInvoice.curyDocBal,APInvoice.status, APInvoice.dueDate, APInvoice.invoiceNbr<br/>
		/// </summary>
		public class RefNbrAttribute : PXSelectorAttribute
		{
			public RefNbrAttribute(Type SearchType)
				: base(SearchType,
				typeof(APRegister.refNbr), 
				typeof(APRegister.docDate),
				typeof(APRegister.finPeriodID),
				typeof(APRegister.vendorID),
				typeof(APRegister.vendorID_Vendor_acctName),
				typeof(APRegister.vendorLocationID),
				typeof(APInvoice.invoiceNbr),
				typeof(APRegister.curyID),
				typeof(APRegister.curyOrigDocAmt),
				typeof(APRegister.curyDocBal),
				typeof(APRegister.status),
				typeof(APInvoice.dueDate),
				typeof(APRegister.docDesc))
			{
			}
		}

		public class AdjdRefNbrAttribute : PXSelectorAttribute
		{
			public AdjdRefNbrAttribute(Type SearchType)
				: base(SearchType,
				typeof(APRegister.branchID),
				typeof(APRegister.refNbr),
				typeof(APRegister.docDate),
				typeof(APRegister.finPeriodID),
				typeof(APRegister.vendorLocationID),
				typeof(APRegister.curyID),
				typeof(APRegister.curyOrigDocAmt),
				typeof(APRegister.curyDocBal),
				typeof(APRegister.status),
				typeof(APAdjust.APInvoice.dueDate),
				typeof(APAdjust.APInvoice.invoiceNbr),
				typeof(APRegister.docDesc))
			{
			}

			protected override bool IsReadDeletedSupported => false;
		}

		public class AdjdLineNbrAttribute : PXSelectorAttribute
		{
			public AdjdLineNbrAttribute()
				: base(typeof(Search2<APTran.lineNbr,
				InnerJoin<APInvoice, On<APInvoice.docType, Equal<APTran.tranType>,
					And<APInvoice.refNbr, Equal<APTran.refNbr>>>>,
				Where<APTran.tranType, Equal<Optional<APAdjust.adjdDocType>>,
					And<APTran.refNbr, Equal<Optional<APAdjust.adjdRefNbr>>,
					And<APInvoice.paymentsByLinesAllowed, Equal<True>,
					And<Where<APTran.curyTranBal, NotEqual<decimal0>,
						Or<APInvoice.retainageApply, Equal<True>, And<APInvoice.retainageUnreleasedAmt, Equal<decimal0>, And<APInvoice.retainageReleased, Equal<decimal0>>>>>>>>>>),
				typeof(APTran.lineNbr),
				typeof(APTran.inventoryID),
				typeof(APTran.tranDesc),
				typeof(APTran.projectID),
				typeof(APTran.taskID),
				typeof(APTran.costCodeID),
				typeof(APTran.accountID),
				typeof(APTran.curyTranBal))
			{
			}

			public override void FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
			{
				if (Equals(e.NewValue, 0))
				{
					e.Cancel = true;
				}
				else
				{
					base.FieldVerifying(sender, e);
				}
			}
		}

		/// <summary>
		/// Specialized for APInvoices version of the <see cref="AutoNumberAttribute"/><br/>
		/// It defines how the new numbers are generated for the AP Invoice. <br/>
		/// References APInvoice.docType and APInvoice.docDate fields of the document,<br/>
		/// and also define a link between  numbering ID's defined in AP Setup and APInvoice types:<br/>
		/// namely APSetup.invoiceNumberingID, APSetup.adjustmentNumberingID,<br/>
		/// APSetup.adjustmentNumberingID, APSetup.invoiceNumberingID <br/>
		/// </summary>
		public class NumberingAttribute : AutoNumberAttribute
		{
            private static string[] _DocTypes
            {
                get
                {
                    return new string[] { Invoice, CreditAdj, DebitAdj, Prepayment };
                }
            }

            private static Type[] _SetupFields
            {
                get
                {
                    return new Type[] 
                    { 
                        typeof(APSetup.invoiceNumberingID),
                        typeof(APSetup.creditAdjNumberingID),
                        typeof(APSetup.debitAdjNumberingID),
                        typeof(APSetup.invoiceNumberingID)
                    };
                }
            }

            public static Type GetNumberingIDField(string docType)
            {
                foreach (var pair in _DocTypes.Zip(_SetupFields))
                    if (pair.Item1 == docType)
                        return pair.Item2;

                return null;
            }

			public NumberingAttribute()
				: base(typeof(APInvoice.docType), typeof(APInvoice.docDate), _DocTypes, _SetupFields)
            {
            }
		}

		public new static readonly string[] Values = { Invoice, CreditAdj, DebitAdj, Prepayment };
		public new static readonly string[] Labels = { Messages.Invoice, Messages.CreditAdj, Messages.DebitAdj, Messages.Prepayment };

		public new class ListAttribute : PXStringListAttribute
		{
			public ListAttribute()
				: base(Values, Labels)
			{
			}
		}

		public class TaxInvoiceListAttribute : PXStringListAttribute
		{
			public TaxInvoiceListAttribute()
				: base(
				new string[] { Invoice, CreditAdj, DebitAdj },
				new string[] { Messages.Invoice, Messages.CreditAdj, Messages.DebitAdj }) { }
		}

		public class AdjdListAttribute : PXStringListAttribute
		{
			public AdjdListAttribute()
				: base(
				new string[] { Invoice, CreditAdj},
				new string[] { Messages.Invoice, Messages.CreditAdj }) { }
		}
		
		/// <summary>
		/// String list with DocType, suitable for the APInvoice document.<br/>
		/// Used in the DocType selector of APInvoices. <br/>
		/// </summary>
		public class InvoiceListAttribute : PXStringListAttribute
		{
			public InvoiceListAttribute()
				: base(
				new string[] { Invoice, CreditAdj, Prepayment },
				new string[] { Messages.Invoice, Messages.CreditAdj, Messages.Prepayment }) { }
		}

		public static string DrCr(string DocType)
		{
			switch (DocType)
			{
				case Invoice:
				case CreditAdj:
				case Prepayment:
				case QuickCheck:
					return GL.DrCr.Debit;
				case DebitAdj:
				case VoidQuickCheck:
					return GL.DrCr.Credit;
				default:
					return null;
			}
		}
	}

	public class APPaymentBy 
	{
		public const int DueDate = 0;
		public const int DiscountDate = 1;

		public class List : PXIntListAttribute
		{
			public List()
				: base(
					new[] {DueDate, DiscountDate},
					new[] {Messages.DueDate, Messages.DiscountDate})
			{
			}
		}

		public class dueDate : PX.Data.BQL.BqlInt.Constant<dueDate>
		{
			public dueDate() : base(DueDate)
			{
			}
		}
		public class discountDate : PX.Data.BQL.BqlInt.Constant<discountDate>
		{
			public discountDate() : base(DiscountDate)
			{
			}
		}

	}

    /// <summary>
    /// Represents AP Invoices, Credit and Debit Adjustments.
    /// The DAC is based on <see cref="APRegister"/> and extends it with the fields 
    /// relevant to the documents of the above types.
    /// </summary>
	[System.SerializableAttribute()]
	[PXTable()]
	[PXSubstitute(GraphType = typeof(APInvoiceEntry))]
	[PXSubstitute(GraphType = typeof(TX.TXInvoiceEntry))]
	[PXPrimaryGraph(typeof(APInvoiceEntry))]
	[PXCacheName(Messages.APInvoice)]
	[PXGroupMask(typeof(InnerJoinSingleTable<Vendor, On<Vendor.bAccountID, Equal<APInvoice.vendorID>, And<Match<Vendor, Current<AccessInfo.userName>>>>>))]
	public class APInvoice : APRegister, CM.IInvoice, IAssign, IApprovable
	{
		#region Keys
		public new class PK : PrimaryKeyOf<APInvoice>.By<docType, refNbr>
		{
			public static APInvoice Find(PXGraph graph, string docType, string refNbr, PKFindOptions options = PKFindOptions.None) => FindBy(graph, docType, refNbr, options);
		}
		public new static class FK
		{
			public class Branch : GL.Branch.PK.ForeignKeyOf<APInvoice>.By<branchID> { }
			public class Vendor : AP.Vendor.PK.ForeignKeyOf<APInvoice>.By<vendorID> { }
			public class VendorLocation : CR.Location.PK.ForeignKeyOf<APInvoice>.By<vendorID, vendorLocationID> { }

			public class SuppliedByVendor : AP.Vendor.PK.ForeignKeyOf<APInvoice>.By<suppliedByVendorID> { }
			public class SuppliedByVendorLocation : CR.Location.PK.ForeignKeyOf<APInvoice>.By<suppliedByVendorID, suppliedByVendorLocationID> { }

			public class CurrencyInfo : CM.CurrencyInfo.PK.ForeignKeyOf<APInvoice>.By<curyInfoID> { }
			public class Currency : CM.Currency.PK.ForeignKeyOf<APInvoice>.By<curyID> { }
			public class APAccount : GL.Account.PK.ForeignKeyOf<APInvoice>.By<aPAccountID> { }
			public class APSubaccount : GL.Sub.PK.ForeignKeyOf<APInvoice>.By<aPSubID> { }
			public class Schedule : GL.Schedule.PK.ForeignKeyOf<APInvoice>.By<scheduleID> { }

			public class Terms : CS.Terms.PK.ForeignKeyOf<APInvoice>.By<termsID> { }
			public class TaxZone : TX.TaxZone.PK.ForeignKeyOf<APInvoice>.By<taxZoneID> { }

			public class PayAccount : CA.CashAccount.PK.ForeignKeyOf<APInvoice>.By<payAccountID> { }

			public class PrebookAccount : GL.Account.PK.ForeignKeyOf<APInvoice>.By<prebookAcctID> { }
			public class PrebookSubaccount : GL.Sub.PK.ForeignKeyOf<APInvoice>.By<prebookSubID> { }

			public class PayType : CA.PaymentMethod.PK.ForeignKeyOf<APInvoice>.By<payTypeID> { }
		}
		#endregion
		#region Events
		public class Events : PXEntityEvent<APInvoice>.Container<Events>
		{
			public PXEntityEvent<APInvoice> ReleaseDocument;
			public PXEntityEvent<APInvoice> OpenDocument;
			public PXEntityEvent<APInvoice> CloseDocument;
			public PXEntityEvent<APInvoice> VoidDocument;
		}
		#endregion
		#region Selected
		public new abstract class selected : PX.Data.BQL.BqlBool.Field<selected> { }
		#endregion
		#region BranchID
		public new abstract class branchID : PX.Data.BQL.BqlInt.Field<branchID> { }

		/// <summary>
		/// Identifier of the <see cref="PX.Objects.GL.Branch">Branch</see>, to which the document belongs.
		/// </summary>
		/// <value>
		/// Corresponds to the <see cref="PX.Objects.GL.Branch.BranchID">Branch.BranchID</see> field.
		/// </value>
		[Branch(typeof(AccessInfo.branchID), IsDetail = false, TabOrder = 0)]
		[PXFormula(typeof(Switch<Case<Where<PendingValue<APInvoice.branchID>, IsPending>, Null,
								Case<Where<APInvoice.vendorLocationID, IsNotNull,
										And<Selector<APInvoice.vendorLocationID, Location.vBranchID>, IsNotNull>>,
									Selector<APInvoice.vendorLocationID, Location.vBranchID>,
								Case<Where<Current2<APInvoice.branchID>, IsNotNull>,
									Current2<APInvoice.branchID>>>>,
								Current<AccessInfo.branchID>>))]
		public override Int32? BranchID
		{
			get
			{
				return this._BranchID;
			}
			set
			{
				this._BranchID = value;
			}
		}
		#endregion
		#region DocType
		public new abstract class docType : PX.Data.BQL.BqlString.Field<docType> { }

        /// <summary>
        /// Type of the document.
        /// </summary>
        /// <value>
        /// Possible values are: "INV" - Invoice, "ACR" - Credit Adjustment, "ADR" - Debit Adjustment, 
        /// "PPM" - Prepayment.
        /// </value>
		[PXDBString(3, IsKey = true, IsFixed = true)]
		[PXDefault]
		[APMigrationModeDependentInvoiceTypeList]
		[PXUIField(DisplayName = "Type", Visibility = PXUIVisibility.SelectorVisible, Enabled = true, TabOrder = 0)]
		[PXFieldDescription]
		public override string DocType
		{
			get
			{
				return _DocType;
			}
			set
			{
				_DocType = value;
			}
		}
		#endregion
		#region RefNbr
		public new abstract class refNbr : PX.Data.BQL.BqlString.Field<refNbr> { }

        /// <summary>
        /// Reference number of the document.
        /// </summary>
		[PXDBString(15, IsKey = true, IsUnicode = true, InputMask = ">CCCCCCCCCCCCCCC")]
		[PXDefault()]
		[PXUIField(DisplayName = "Reference Nbr.", Visibility = PXUIVisibility.SelectorVisible, TabOrder = 1)]
		[APInvoiceType.RefNbr(typeof(Search2<Standalone.APRegisterAlias.refNbr,
			InnerJoinSingleTable<APInvoice, On<APInvoice.docType, Equal<Standalone.APRegisterAlias.docType>,
				And<APInvoice.refNbr, Equal<Standalone.APRegisterAlias.refNbr>>>,
			InnerJoinSingleTable<Vendor, On<Standalone.APRegisterAlias.vendorID, Equal<Vendor.bAccountID>>>>,
			Where<Standalone.APRegisterAlias.docType,Equal<Optional<APInvoice.docType>>, 
				And2<Where<Standalone.APRegisterAlias.origModule, NotEqual<BatchModule.moduleTX>, 
					Or<Standalone.APRegisterAlias.released, Equal<True>>>,
				And<Match<Vendor, Current<AccessInfo.userName>>>>>, 
			OrderBy<Desc<Standalone.APRegisterAlias.refNbr>>>), Filterable = true, IsPrimaryViewCompatible = true)]
		[APInvoiceType.Numbering()]
		[PXFieldDescription]
		public override String RefNbr
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
		#region Status
		public new abstract class status : PX.Data.BQL.BqlString.Field<status> { }
		#endregion
		#region FinPeriodID
		public new abstract class finPeriodID : PX.Data.BQL.BqlString.Field<finPeriodID> { }
		#endregion
		#region TranPeriodID
		public new abstract class tranPeriodID : PX.Data.BQL.BqlString.Field<tranPeriodID> { }
		#endregion

		#region VendorID
		public new abstract class vendorID : PX.Data.BQL.BqlInt.Field<vendorID> { }
		#endregion

		#region VendorLocationID
		public new abstract class vendorLocationID : PX.Data.BQL.BqlInt.Field<vendorLocationID> { }
		[LocationActive(
			typeof(Where<Location.bAccountID, Equal<Optional<APRegister.vendorID>>,
				And<MatchWithBranch<Location.vBranchID>>>),
			DescriptionField = typeof(Location.descr),
			Visibility = PXUIVisibility.SelectorVisible)]
		[PXDefault(typeof(Coalesce<
			Search2<Vendor.defLocationID,
				InnerJoin<CRLocation,
					On<CRLocation.locationID, Equal<Vendor.defLocationID>,
					And<CRLocation.bAccountID, Equal<Vendor.bAccountID>>>>,
				Where<Vendor.bAccountID, Equal<Current<APRegister.vendorID>>,
					And<CRLocation.isActive, Equal<True>, And<MatchWithBranch<CRLocation.vBranchID>>>>>,
			Search<CRLocation.locationID,
				Where<CRLocation.bAccountID, Equal<Current<APRegister.vendorID>>,
					And<CRLocation.isActive, Equal<True>, And<MatchWithBranch<CRLocation.vBranchID>>>>>>))]
		[PXFormula(typeof(Default<APInvoice.vendorID>))]
		public override int? VendorLocationID
		{
			get
			{
				return this._VendorLocationID;
			}
			set
			{
				this._VendorLocationID = value;
			}
		}
		#endregion
		#region PaymentInfoLocationID
		public new abstract class paymentInfoLocationID : PX.Data.BQL.BqlInt.Field<paymentInfoLocationID> { }
		[PXInt]
		[PXSelector(typeof(Search<Location.locationID, Where<Location.bAccountID, Equal<APInvoice.vendorID.FromCurrent>>>))]
		[PXFormula(typeof(IIf<Where<Selector<APInvoice.vendorLocationID, Location.isAPPaymentInfoSameAsMain>, Equal<True>>,
			Selector<APInvoice.vendorLocationID, Location.vPaymentInfoLocationID>,
			APInvoice.vendorLocationID>))]
		public int? PaymentInfoLocationID
		{
			get;
			set;
		}
		#endregion

		#region SuppliedByVendorID
		public abstract class suppliedByVendorID : PX.Data.BQL.BqlInt.Field<suppliedByVendorID> { }

		/// <summary>
		/// A reference to the <see cref="Vendor"/>.
		/// </summary>
		/// <value>
		/// An integer identifier of the vendor that supplied the goods. 
		/// </value>
		[Vendor(
			DisplayName = "Supplied-by Vendor",
			DescriptionField = typeof(Vendor.acctName),
			FieldClass = nameof(FeaturesSet.VendorRelations),
			CacheGlobal = true,
			Filterable = true,
			Required = true)]
		[PXRestrictor(
			typeof(Where<Vendor.vStatus, NotEqual<VendorStatus.inactive>,
				And<Vendor.vStatus, NotEqual<VendorStatus.hold>>>),
			Messages.VendorIsInStatus, 
			typeof(Vendor.vStatus))]
		[PXRestrictor(
			typeof(Where<Vendor.bAccountID, Equal<Current<APInvoice.vendorID>>,
				Or<Vendor.payToVendorID, Equal<Current<APInvoice.vendorID>>>>),
			Messages.NotSuppliedByVendor)]
		[PXFormula(typeof(APInvoice.vendorID))]
		[PXDefault]
		[PXForeignReference(typeof(Field<APInvoice.suppliedByVendorID>.IsRelatedTo<Vendor.bAccountID>))]
		public virtual int? SuppliedByVendorID { get; set; }
		#endregion

		#region SuppliedByVendorLocationID
		public abstract class suppliedByVendorLocationID : PX.Data.BQL.BqlInt.Field<suppliedByVendorLocationID> { }

		/// <summary>
		/// Identifier of the <see cref="Location">Location</see> of the <see cref="Vendor">Supplied-by Vendor</see>, associated with the document.
		/// </summary>
		/// <value>
		/// Corresponds to the <see cref="Location.LocationID"/> field. Defaults to AP bill's <see cref="APInvoice.VendorLocationID">vendor location</see>.
		/// </value>
		[LocationActive(
			typeof(Where<Location.bAccountID, Equal<Current<APInvoice.suppliedByVendorID>>,
				And<MatchWithBranch<Location.vBranchID>>>),
			DisplayName = "Supplied-by Vendor Location",
			DescriptionField = typeof(Location.descr),
			FieldClass = nameof(FeaturesSet.VendorRelations),
			Visibility = PXUIVisibility.SelectorVisible,
			Required = true)]
		[PXFormula(typeof(Switch<
			Case2<
				Where<Not<IsPOLinkedAPBill>>,
				Switch<
					Case2<
						Where2<FeatureInstalled<FeaturesSet.vendorRelations>,
							And<Current<APInvoice.vendorID>, NotEqual<APInvoice.suppliedByVendorID>>>,
						Selector<APInvoice.suppliedByVendorID, BAccount.defLocationID>>,
				ExternalValue<APInvoice.vendorLocationID>>>,
			APInvoice.suppliedByVendorLocationID>))]
		[PXDefault]
		[PXForeignReference(
			typeof(CompositeKey<
				Field<APInvoice.suppliedByVendorID>.IsRelatedTo<Location.bAccountID>,
				Field<APInvoice.suppliedByVendorLocationID>.IsRelatedTo<Location.locationID>>))]
		public virtual int? SuppliedByVendorLocationID { get; set; }
		#endregion

		#region CuryID
		public new abstract class curyID : PX.Data.BQL.BqlString.Field<curyID> { }
		#endregion
		#region APAccountID
		public new abstract class aPAccountID : PX.Data.BQL.BqlInt.Field<aPAccountID> { }
		#endregion
		#region APSubID
		public new abstract class aPSubID : PX.Data.BQL.BqlInt.Field<aPSubID> { }
		#endregion
		#region TermsID
		public abstract class termsID : PX.Data.BQL.BqlString.Field<termsID> { }

        /// <summary>
        /// The <see cref="PX.Objects.CS.Terms">credit terms</see> associated with the document (unavailable for prepayments and debit adjustments).\
        /// Defaults to the <see cref="Vendor.TermsID">credit terms of the vendor</see>.
        /// </summary>
		[PXDBString(10, IsUnicode = true)]
		[PXDefault(typeof(Search<Vendor.termsID,
			Where<Vendor.bAccountID, Equal<Current<APInvoice.vendorID>>,
				And2<Where<Current<APInvoice.docType>, NotEqual<APDocType.debitAdj>>,
					Or<Where<Current<APSetup.termsInDebitAdjustments>, Equal<True>>>>>>),
			PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Terms", Visibility = PXUIVisibility.Visible)]
		[APTermsSelector]
		[Terms(typeof(APInvoice.docDate), typeof(APInvoice.dueDate), typeof(APInvoice.discDate), typeof(APInvoice.curyOrigDocAmt), typeof(APInvoice.curyOrigDiscAmt), typeof(APInvoice.curyTaxTotal))]
		public virtual string TermsID
		{
			get;
			set;
		}
		#endregion
		#region DueDate
		public abstract class dueDate : PX.Data.BQL.BqlDateTime.Field<dueDate> { }
		protected DateTime? _DueDate;

        /// <summary>
        /// The date when payment for the document is due in accordance with the <see cref="APInvoice.TermsID">credit terms</see>.
        /// </summary>
		[PXDBDate()]
		[PXUIField(DisplayName="Due Date", Visibility=PXUIVisibility.SelectorVisible)]
		public virtual DateTime? DueDate
		{
			get
			{
				return this._DueDate;
			}
			set
			{
				this._DueDate = value;
			}
		}
		#endregion
		#region DiscDate
		public abstract class discDate : PX.Data.BQL.BqlDateTime.Field<discDate> { }
		protected DateTime? _DiscDate;

        /// <summary>
        /// The date when the cash discount can be taken in accordance with the <see cref="APInvoice.TermsID">credit terms</see>.
        /// </summary>
		[PXDBDate()]
		[PXUIField(DisplayName="Cash Discount Date", Visibility=PXUIVisibility.SelectorVisible)]
		public virtual DateTime? DiscDate
		{
			get
			{
				return this._DiscDate;
			}
			set
			{
				this._DiscDate = value;
			}
		}
		#endregion
		#region MasterRefNbr
		public abstract class masterRefNbr : PX.Data.BQL.BqlString.Field<masterRefNbr> { }
		/// <summary>
		/// For an installment this field holds the <see cref="RefNbr">reference number</see> of the master document.
		/// </summary>
		[PXDBString(15, IsUnicode = true)]
		public virtual string MasterRefNbr { get; set; }
		#endregion
		#region InstallmentNbr
		public abstract class installmentNbr : PX.Data.BQL.BqlShort.Field<installmentNbr> { }
		/// <summary>
		/// The number of the installment, which the document represents.
		/// </summary>
		/// <value>
		/// Corresponds to the <see cref="PX.Objects.CS.TermsInstallments.InstallmentNbr">TermsInstallments.InstallmentNbr</see> field.
		/// </value>
		[PXDBShort]
		public virtual short? InstallmentNbr { get; set; }
		#endregion
		#region InstallmentCntr
		public abstract class installmentCntr : PX.Data.BQL.BqlShort.Field<installmentCntr> { }
		/// <summary>
		/// Counter of the document's installments. 
		/// </summary>
		[PXDBShort]
		[PXUIField(DisplayName = "Number of Installments")]
		public virtual short? InstallmentCntr { get; set; }
		#endregion
		#region InvoiceNbr
		public abstract class invoiceNbr : PX.Data.BQL.BqlString.Field<invoiceNbr> { }
		protected String _InvoiceNbr;

        /// <summary>
        /// The document’s original reference number as assigned by the vendor (for informational purposes).
        /// The reference to the vendor document is required if <see cref="APSetup.RequireVendorRef"/> is set to <c>true</c>.
        /// The reference should also be unique if <see cref="APSetup.RaiseErrorOnDoubleInvoiceNbr"/> is set to <c>true</c>.
        /// </summary>
		[PXDBString(40, IsUnicode=true)]
		[PXUIField(DisplayName="Vendor Ref.", Visibility=PXUIVisibility.SelectorVisible)]
		[APVendorRefNbr]
		public virtual String InvoiceNbr
		{
			get
			{
				return this._InvoiceNbr;
			}
			set
			{
				this._InvoiceNbr = value;
			}
		}
		#endregion
		#region InvoiceDate
		public abstract class invoiceDate : PX.Data.BQL.BqlDateTime.Field<invoiceDate> { }
		protected DateTime? _InvoiceDate;

        /// <summary>
        /// The document’s original date as assigned by the vendor (for informational purposes).
        /// </summary>
		[PXDBDate()]
		[PXDefault(TypeCode.DateTime, "01/01/1900")]
		[PXUIField(DisplayName="Vendor Ref. Date", Visibility=PXUIVisibility.Invisible)]
		public virtual DateTime? InvoiceDate
		{
			get
			{
				return this._InvoiceDate;
			}
			set
			{
				this._InvoiceDate = value;
			}
		}
		#endregion

		#region TaxZoneID
		public abstract class taxZoneID : PX.Data.BQL.BqlString.Field<taxZoneID> { }

        /// <summary>
        /// Identifier of the <see cref="TaxZone">tax zone</see> associated with the document.
        /// Defaults to <see cref="PX.Objects.CR.Location.VTaxZoneID">vendor's tax zone</see>.
        /// </summary>
		[PXDBString(10, IsUnicode = true)]
		[PXUIField(DisplayName = "Vendor Tax Zone", Visibility = PXUIVisibility.Visible)]
		[PXSelector(typeof(TaxZone.taxZoneID), DescriptionField=typeof(TaxZone.descr), Filterable =true)]
		[PXFormula(typeof(Default<APInvoice.suppliedByVendorLocationID, APInvoice.suppliedByVendorID, APInvoice.vendorLocationID>))]
		public virtual string TaxZoneID { get; set; }
		#endregion

		#region ExternalTaxesImportInProgress
		/// <summary>
		/// Indicates that taxes were calculated by an external system
		/// </summary>
		public abstract class externalTaxesImportInProgress : PX.Data.BQL.BqlBool.Field<externalTaxesImportInProgress> { }
		[PXBool()]
		[PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual Boolean? ExternalTaxesImportInProgress
		{
			get;
			set;
		}
		#endregion
		#region DocDate
		public new abstract class docDate : PX.Data.BQL.BqlDateTime.Field<docDate> { }
		#endregion
		#region CuryTaxTotal
		public abstract class curyTaxTotal : PX.Data.BQL.BqlDecimal.Field<curyTaxTotal> { }
		protected Decimal? _CuryTaxTotal;

        /// <summary>
        /// The total amount of taxes associated with the document. (Presented in the currency of the document, see <see cref="CuryID"/>)
        /// </summary>
		[PXDBCurrency(typeof(APInvoice.curyInfoID), typeof(APInvoice.taxTotal))]
		[PXUIField(DisplayName="Tax Total",Visibility=PXUIVisibility.Visible, Enabled=false)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? CuryTaxTotal
		{
			get
			{
				return this._CuryTaxTotal;
			}
			set
			{
				this._CuryTaxTotal = value;
			}
		}
		#endregion
		#region TaxTotal
		public abstract class taxTotal : PX.Data.BQL.BqlDecimal.Field<taxTotal> { }
		protected Decimal? _TaxTotal;

        /// <summary>
        /// The total amount of taxes associated with the document. (Presented in the base currency of the company, see <see cref="Company.BaseCuryID"/>)
        /// </summary>
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? TaxTotal
		{
			get
			{
				return this._TaxTotal;
			}
			set
			{
				this._TaxTotal = value;
			}
		}
		#endregion
		#region CuryLineTotal
		public abstract class curyLineTotal : PX.Data.BQL.BqlDecimal.Field<curyLineTotal> { }
		protected Decimal? _CuryLineTotal;

        /// <summary>
        /// The document total presented in the currency of the document. (See <see cref="CuryID"/>)
        /// </summary>
		[PXDBCurrency(typeof(APInvoice.curyInfoID), typeof(APInvoice.lineTotal))]
		[PXUIField(DisplayName="Detail Total",Visibility=PXUIVisibility.Visible, Enabled=false)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? CuryLineTotal
		{
			get
			{
				return this._CuryLineTotal;
			}
			set
			{
				this._CuryLineTotal = value;
			}
		}
		#endregion
		#region LineTotal
		public abstract class lineTotal : PX.Data.BQL.BqlDecimal.Field<lineTotal> { }
		protected Decimal? _LineTotal;

        /// <summary>
        /// The document total presented in the base currency of the company. (See <see cref="Company.BaseCuryID"/>)
        /// </summary>
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? LineTotal
		{
			get
			{
				return this._LineTotal;
			}
			set
			{
				this._LineTotal = value;
			}
		}
		#endregion
        #region DiscTot
        public new abstract class discTot : PX.Data.BQL.BqlDecimal.Field<discTot> { }
        #endregion
        #region CuryDiscTot
        public new abstract class curyDiscTot : PX.Data.BQL.BqlDecimal.Field<curyDiscTot> { }
        #endregion
		#region CuryTaxAmt
		public abstract class curyTaxAmt : PX.Data.BQL.BqlDecimal.Field<curyTaxAmt> { }
		[PXDBCurrency(typeof(APInvoice.curyInfoID), typeof(APInvoice.taxAmt))]
		[PXUIField(DisplayName = "Tax Amount")]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? CuryTaxAmt { get; set; }
		#endregion
		#region TaxAmt
		public abstract class taxAmt : PX.Data.BQL.BqlDecimal.Field<taxAmt> { }
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? TaxAmt { get; set; }
		#endregion

        #region DocDisc
		[Obsolete("This field is obsolete and will be removed in 2021R1.")]
        public new abstract class docDisc : PX.Data.BQL.BqlDecimal.Field<docDisc> { }
        #endregion
        #region CuryDocDisc
		[Obsolete("This field is obsolete and will be removed in 2021R1.")]
        public new abstract class curyDocDisc : PX.Data.BQL.BqlDecimal.Field<curyDocDisc> { }
        #endregion
        #region RoundDiff
        public new abstract class roundDiff : PX.Data.BQL.BqlDecimal.Field<roundDiff> { }
        #endregion
        #region CuryRoundDiff
        public new abstract class curyRoundDiff : PX.Data.BQL.BqlDecimal.Field<curyRoundDiff> { }
        #endregion
		#region CuryVatExemptTotal
		public abstract class curyVatExemptTotal : PX.Data.BQL.BqlDecimal.Field<curyVatExemptTotal> { }
		protected Decimal? _CuryVatExemptTotal;

        /// <summary>
        /// The part of the document total that is exempt from VAT. 
        /// This total is calculated as a sum of the taxable amounts for the <see cref="PX.Objects.TX.Tax">taxes</see>
        /// of <see cref="PX.Objects.TX.Tax.TaxType">type</see> VAT, which are marked as <see cref="PX.Objects.TX.Tax.ExemptTax">exempt</see> 
        /// and are neither <see cref="PX.Objects.TX.Tax.StatisticalTax">statistical</see> nor <see cref="PX.Objects.TX.Tax.ReverseTax">reverse</see>.
        /// (Presented in the currency of the document, see <see cref="CuryID"/>)
        /// </summary>
		[PXDBCurrency(typeof(APInvoice.curyInfoID), typeof(APInvoice.vatExemptTotal))]
		[PXUIField(DisplayName = "VAT Exempt", Visibility = PXUIVisibility.Visible, Enabled = false)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? CuryVatExemptTotal
		{
			get
			{
				return this._CuryVatExemptTotal;
			}
			set
			{
				this._CuryVatExemptTotal = value;
			}
		}
		#endregion

		#region VatExemptTaxTotal
		public abstract class vatExemptTotal : PX.Data.BQL.BqlDecimal.Field<vatExemptTotal> { }
		protected Decimal? _VatExemptTotal;

        /// <summary>
        /// The part of the document total that is exempt from VAT. 
        /// This total is calculated as a sum of the taxable amounts for the <see cref="PX.Objects.TX.Tax">taxes</see>
        /// of <see cref="PX.Objects.TX.Tax.TaxType">type</see> VAT, which are marked as <see cref="PX.Objects.TX.Tax.ExemptTax">exempt</see> 
        /// and are neither <see cref="PX.Objects.TX.Tax.StatisticalTax">statistical</see> nor <see cref="PX.Objects.TX.Tax.ReverseTax">reverse</see>.
        /// (Presented in the base currency of the company, see <see cref="Company.BaseCuryID"/>)
        /// </summary>
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? VatExemptTotal
		{
			get
			{
				return this._VatExemptTotal;
			}
			set
			{
				this._VatExemptTotal = value;
			}
		}
		#endregion
				
		#region CuryVatTaxableTotal
		public abstract class curyVatTaxableTotal : PX.Data.BQL.BqlDecimal.Field<curyVatTaxableTotal> { }
		protected Decimal? _CuryVatTaxableTotal;

        /// <summary>
        /// The part of the document total, which is subject to VAT.
        /// This total is calculated as a sum of the taxable amounts for the <see cref="PX.Objects.TX.Tax">taxes</see>
        /// of <see cref="PX.Objects.TX.Tax.TaxType">type</see> VAT, which are neither <see cref="PX.Objects.TX.Tax.ExemptTax">exempt</see>, 
        /// nor <see cref="PX.Objects.TX.Tax.StatisticalTax">statistical</see>, nor <see cref="PX.Objects.TX.Tax.ReverseTax">reverse</see>.
        /// (Presented in the currency of the document, see <see cref="CuryID"/>)
        /// </summary>
		[PXDBCurrency(typeof(APInvoice.curyInfoID), typeof(APInvoice.vatTaxableTotal))]
		[PXUIField(DisplayName = "VAT Taxable", Visibility = PXUIVisibility.Visible, Enabled = false)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? CuryVatTaxableTotal
		{
			get
			{
				return this._CuryVatTaxableTotal;
			}
			set
			{
				this._CuryVatTaxableTotal = value;
			}
		}
		#endregion

		#region VatTaxableTotal
		public abstract class vatTaxableTotal : PX.Data.BQL.BqlDecimal.Field<vatTaxableTotal> { }
		protected Decimal? _VatTaxableTotal;

        /// <summary>
        /// The part of the document total, which is subject to VAT.
        /// This total is calculated as a sum of the taxable amounts for the <see cref="PX.Objects.TX.Tax">taxes</see>
        /// of <see cref="PX.Objects.TX.Tax.TaxType">type</see> VAT, which are neither <see cref="PX.Objects.TX.Tax.ExemptTax">exempt</see>, 
        /// nor <see cref="PX.Objects.TX.Tax.StatisticalTax">statistical</see>, nor <see cref="PX.Objects.TX.Tax.ReverseTax">reverse</see>.
        /// (Presented in the base currency of the company, see <see cref="Company.BaseCuryID"/>)
        /// </summary>
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? VatTaxableTotal
		{
			get
			{
				return this._VatTaxableTotal;
			}
			set
			{
				this._VatTaxableTotal = value;
			}
		}
		#endregion

		#region CuryInfoID
		public new abstract class curyInfoID : PX.Data.BQL.BqlLong.Field<curyInfoID> { }
		#endregion
		#region CuryOrigDocAmt
		public new abstract class curyOrigDocAmt : PX.Data.BQL.BqlDecimal.Field<curyOrigDocAmt> { }
		#endregion
		#region OrigDocAmt
		public new abstract class origDocAmt : PX.Data.BQL.BqlDecimal.Field<origDocAmt> { }
		#endregion
		#region CuryDocBal
		public new abstract class curyDocBal : PX.Data.BQL.BqlDecimal.Field<curyDocBal> { }
		#endregion
		#region DocBal
		public new abstract class docBal : PX.Data.BQL.BqlDecimal.Field<docBal> { }
		#endregion
		#region CuryInitDocBal
		public new abstract class curyInitDocBal : PX.Data.BQL.BqlDecimal.Field<curyInitDocBal> { }

		/// <summary>
		/// The entered in migration mode balance of the document.
		/// Given in the <see cref="CuryID">currency of the document</see>.
		/// </summary>
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXDBCurrency(typeof(APRegister.curyInfoID), typeof(APRegister.initDocBal))]
		[PXUIField(DisplayName = "Balance", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
		[PXUIVerify(typeof(
			Where<APInvoice.hold, Equal<True>,
				Or<APInvoice.isMigratedRecord, NotEqual<True>,
				Or<Where<APInvoice.curyInitDocBal, GreaterEqual<decimal0>,
					And<APInvoice.curyInitDocBal, LessEqual<APInvoice.curyOrigDocAmt>>>>>>),
			PXErrorLevel.Error, Common.Messages.IncorrectMigratedBalance,
			CheckOnInserted = false, 
			CheckOnRowSelected = false, 
			CheckOnVerify = false, 
			CheckOnRowPersisting = true)]
		public override decimal? CuryInitDocBal
		{
			get;
			set;
		}
		#endregion
		#region CuryDiscBal
		public new abstract class curyDiscBal : PX.Data.BQL.BqlDecimal.Field<curyDiscBal> { }
		#endregion
		#region DiscBal
		public new abstract class discBal : PX.Data.BQL.BqlDecimal.Field<discBal> { }
		#endregion
		#region CuryOrigDiscAmt
		public new abstract class curyOrigDiscAmt : PX.Data.BQL.BqlDecimal.Field<curyOrigDiscAmt> { }
		#endregion
		#region OrigDiscAmt
		public new abstract class origDiscAmt : PX.Data.BQL.BqlDecimal.Field<origDiscAmt> { }
		#endregion
		#region CuryOrigWhTaxAmt
		public new abstract class curyOrigWhTaxAmt : PX.Data.BQL.BqlDecimal.Field<curyOrigWhTaxAmt> { }
		#endregion
		#region OrigWhTaxAmt
		public new abstract class origWhTaxAmt : PX.Data.BQL.BqlDecimal.Field<origWhTaxAmt> { }
		#endregion
		#region CuryWhTaxBal
		public new abstract class curyWhTaxBal : PX.Data.BQL.BqlDecimal.Field<curyWhTaxBal> { }
		#endregion
		#region WhTaxBal
		public new abstract class whTaxBal : PX.Data.BQL.BqlDecimal.Field<whTaxBal> { }
		#endregion
		#region CuryTaxWheld
		public new abstract class curyTaxWheld : PX.Data.BQL.BqlDecimal.Field<curyTaxWheld> { }
		#endregion
		#region TaxWheld
		public new abstract class taxWheld : PX.Data.BQL.BqlDecimal.Field<taxWheld> { }
		#endregion
		#region DrCr
		public abstract class drCr : PX.Data.BQL.BqlString.Field<drCr> { }
		protected string _DrCr;

		/// <summary>
		/// Read-only field indicating whether the document is of debit or credit type.
		/// The value of this field is based solely on the <see cref="DocType"/> field.
		/// </summary>
		/// <value>
		/// Possible values are <c>"D"</c> (for Invoice, Credit Adjustment, Prepayment, Cash Purchase)
		/// and <c>"C"</c> (for Debit Adjustment and Voided Cash Purchase).
		/// </value>
		[PXString(1,IsFixed = true)]
		public string DrCr
		{
			[PXDependsOnFields(typeof(docType))]
			get
			{
				return APInvoiceType.DrCr(this._DocType);
			}
			set
			{
			}
		}
		#endregion
		#region SeparateCheck
		public abstract class separateCheck : PX.Data.BQL.BqlBool.Field<separateCheck> { }
		protected Boolean? _SeparateCheck;

        /// <summary>
        /// When set to <c>true</c> indicates that the document should be paid for by a separate check.
        /// In other words, the payment to such a document must not be consolidated with other payments.
        /// </summary>
        /// <value>
        /// Defaults to the value of the <see cref="PX.Objects.CR.Location.SeparateCheck">same setting</see> for vendor.
        /// </value>
		[PXDBBool()]
		[PXUIField(DisplayName = "Pay Separately", Visibility = PXUIVisibility.Visible)]
		[PXDefault(typeof(Search<Location.separateCheck,Where<Location.bAccountID,Equal<Current<APInvoice.vendorID>>, And<Location.locationID, Equal<Current<APInvoice.vendorLocationID>>>>>))]
		public virtual Boolean? SeparateCheck
		{
			get
			{
				return this._SeparateCheck;
			}
			set
			{
				this._SeparateCheck = value;
			}
		}
		#endregion
		#region PaySel
		public abstract class paySel : PX.Data.BQL.BqlBool.Field<paySel> { }
		protected bool? _PaySel;

        /// <summary>
        /// When set to <c>true</c> indicates that the document is approved for payment.
        /// </summary>
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Approved for Payment")]
		public bool? PaySel
		{
			get
			{
				return _PaySel;
			}
			set
			{
				_PaySel = value;
			}
		}
		#endregion
		#region PayLocationID
		public abstract class payLocationID : PX.Data.BQL.BqlInt.Field<payLocationID> { }
		protected Int32? _PayLocationID;

        /// <summary>
        /// <see cref="Vendor"/> location for payment.
        /// </summary>
        /// <value>
        /// Defaults to vendor's <see cref="Vendor.DefLocationID">default location</see>
        /// or to the first active location of the vendor if the former is not set.
        /// </value>
		[LocationActive(
			typeof(Where<Location.bAccountID, Equal<Current<APRegister.vendorID>>,
				And<MatchWithBranch<Location.vBranchID>>>),
			DescriptionField = typeof(Location.descr),
			Visibility = PXUIVisibility.SelectorVisible,
            DisplayName = "Payment Location")]
		[PXDefault(typeof(Coalesce<
			Search2<Vendor.defLocationID,
			InnerJoin<CRLocation,
				On<CRLocation.locationID, Equal<Vendor.defLocationID>,
				And<CRLocation.bAccountID, Equal<Vendor.bAccountID>>>>,
			Where<Vendor.bAccountID, Equal<Current<APRegister.vendorID>>,
				And<CRLocation.isActive, Equal<True>, And<MatchWithBranch<CRLocation.vBranchID>>>>>,
			Search<CRLocation.locationID,
			Where<CRLocation.bAccountID, Equal<Current<APRegister.vendorID>>,
			And<CRLocation.isActive, Equal<True>, And<MatchWithBranch<CRLocation.vBranchID>>>>>>))]
		public virtual Int32? PayLocationID
		{
			get
			{
				return this._PayLocationID;
			}
			set
			{
				this._PayLocationID = value;
			}
		}
		#endregion
		#region PayDate
		public abstract class payDate : PX.Data.BQL.BqlDateTime.Field<payDate> { }
		protected DateTime? _PayDate;

        /// <summary>
        /// The date when the bill has been approved for payment.
        /// </summary>
		[PXDBDate()]
		[PXUIField(DisplayName = "Pay Date", Visibility = PXUIVisibility.Visible)]
		[PXDefault()]
		[PXFormula(typeof(SO.DateMinusDaysNotLessThenDate<
			Switch<
				Case<Where<Selector<APInvoice.paymentInfoLocationID, Location.vPaymentByType>, Equal<APPaymentBy.discountDate>,
							 And<APInvoice.discDate, IsNotNull>>, APInvoice.discDate>,
				APInvoice.dueDate>,
			IsNull<Selector<APInvoice.paymentInfoLocationID, Location.vPaymentLeadTime>, decimal0>, APInvoice.docDate>))]
		public virtual DateTime? PayDate
		{
			get
			{
				return this._PayDate;
			}
			set
			{
				this._PayDate = value;
			}
		}
		#endregion
		#region PayTypeID
		public abstract class payTypeID : PX.Data.BQL.BqlString.Field<payTypeID> { }
		protected String _PayTypeID;

        /// <summary>
        /// The <see cref="PX.Objects.CA.PaymentMethod">payment method</see> used for the document.
        /// </summary>
        /// <value>
        /// Corresponds to the <see cref="PX.Objects.CA.PaymentMethod.PaymentMethodID">PaymentMethod.PaymentMethodID</see> field.
        /// Defaults to the payment method associated with the vendor location.
        /// </value>
		[PXDBString(10, IsUnicode = true)]
		[PXUIField(DisplayName = "Payment Method", Visibility = PXUIVisibility.Visible)]

		[PXSelector(typeof(Search<PaymentMethod.paymentMethodID,
						  Where<PaymentMethod.useForAP, Equal<True>,
							And<PaymentMethod.isActive, Equal<True>>>>), DescriptionField = typeof(PaymentMethod.descr))]
		[PXDefault(typeof(Search<Location.paymentMethodID, Where<Location.bAccountID, Equal<Current<APInvoice.vendorID>>, And<Location.locationID, Equal<Current<APInvoice.payLocationID>>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual String PayTypeID
		{
			get
			{
				return this._PayTypeID;
			}
			set
			{
				this._PayTypeID = value;
			}
		}
		#endregion
		#region PayAccountID
		public abstract class payAccountID : PX.Data.BQL.BqlInt.Field<payAccountID> { }
		protected Int32? _PayAccountID;

        /// <summary>
        /// The <see cref="PX.Objects.CA.CashAccount">cash account</see> used for the payment.
        /// </summary>
        /// <value>
        /// Defaults to the cash account associated with the selected <see cref="APInvoice.PayLocationID">location</see> and <see cref="APInvoice.PayTypeID">payment method</see>.
        /// In case such account is not found the default value will be the cash account which is specified as 
        /// <see cref="PX.Objects.CA.PaymentMethodAccount.APIsDefault">default for AP</see> for the selected payment method.
        /// </value>
		[PXDefault(typeof(Coalesce<Coalesce<
			Search2<
				Location.cashAccountID,
					InnerJoin<PaymentMethodAccount, 
						On<PaymentMethodAccount.cashAccountID, Equal<Location.cashAccountID>,
											And<PaymentMethodAccount.paymentMethodID, Equal<Location.vPaymentMethodID>,
											And<PaymentMethodAccount.useForAP, Equal<True>>>>>,
				Where<
					Location.bAccountID, Equal<Current<APInvoice.vendorID>>,
										And<Location.locationID, Equal<Current<APInvoice.payLocationID>>,
										And<Location.vPaymentMethodID, Equal<Current<APInvoice.payTypeID>>>>>>,
			// Handle the case when the payment settings for the currently selected payment 
			// location are set to "Same as Main" (the first condition in the Where clause).
			// -
			Search2<
				Location.cashAccountID,
					InnerJoin<CR.Standalone.LocationAlias,
						On<Location.locationID, Equal<CR.Standalone.LocationAlias.vPaymentInfoLocationID>>,
					InnerJoin<PaymentMethodAccount,
						On<PaymentMethodAccount.cashAccountID, Equal<Location.cashAccountID>,
						And<PaymentMethodAccount.paymentMethodID, Equal<Location.vPaymentMethodID>,
						And<PaymentMethodAccount.useForAP, Equal<True>>>>>>,
				Where<
					CR.Standalone.LocationAlias.locationID, NotEqual<CR.Standalone.LocationAlias.vPaymentInfoLocationID>,
					And<CR.Standalone.LocationAlias.bAccountID, Equal<Current<APInvoice.vendorID>>,
					And<CR.Standalone.LocationAlias.locationID, Equal<Current<APInvoice.payLocationID>>,
					And<Location.vPaymentMethodID, Equal<Current<APInvoice.payTypeID>>>>>>>>,
			Search2<
				PaymentMethodAccount.cashAccountID, 
					InnerJoin<CashAccount, 
						On<CashAccount.cashAccountID, Equal<PaymentMethodAccount.cashAccountID>>>,
				Where<
					PaymentMethodAccount.paymentMethodID, Equal<Current<APInvoice.payTypeID>>,
											And<CashAccount.branchID, Equal<Current<APInvoice.branchID>>,
											And<PaymentMethodAccount.useForAP, Equal<True>,
					And<PaymentMethodAccount.aPIsDefault, Equal<True>>>>>>>), 
			PersistingCheck = PXPersistingCheck.Nothing)]
		[CashAccount(typeof(APInvoice.branchID), typeof(Search2<CashAccount.cashAccountID,
						InnerJoin<PaymentMethodAccount,
							On<PaymentMethodAccount.cashAccountID, Equal<CashAccount.cashAccountID>>>,
						Where2<Match<Current<AccessInfo.userName>>,
							And<CashAccount.clearingAccount, Equal<False>,
							And<PaymentMethodAccount.paymentMethodID, Equal<Current<APInvoice.payTypeID>>,
							And<PaymentMethodAccount.useForAP, Equal<True>>>>>>),
										Visibility = PXUIVisibility.Visible)]
		[PXFormula(typeof(Validate<APInvoice.curyID>))]
		[PXForeignReference(typeof(Field<APInvoice.payAccountID>.IsRelatedTo<CashAccount.cashAccountID>))]
		public virtual Int32? PayAccountID
		{
			get
			{
				return this._PayAccountID;
			}
			set
			{
				this._PayAccountID = value;
			}
		}
		#endregion
		#region PrebookAcctID
		public abstract class prebookAcctID : PX.Data.BQL.BqlInt.Field<prebookAcctID> { }
		protected Int32? _PrebookAcctID;
		
        /// <summary>
        /// The expense <see cref="PX.Objects.GL.Account">account</see> used to record the expenses pending reclassification.
        /// The field is relevant only if the <see cref="PX.Objects.CS.FeaturesSet.Prebooking">Support for Expense Reclassification feature</see> is activated 
        /// and the document has or has had the Prebooked (<c>"K"</c>) status. (See <see cref="APRegister.Prebooked"/>)
        /// </summary>
        /// <value>
        /// Defaults to the account associated with the vendor of the document.
        /// Corresponds to the <see cref="PX.Objects.GL.Account.AccountID">AccountID</see> field.
        /// </value>
		[PXDefault(typeof(Select<Vendor, Where<Vendor.bAccountID, Equal<Current<vendorID>>, And<FeatureInstalled<FeaturesSet.prebooking>>>>), 
			SourceField = typeof(Vendor.prebookAcctID), PersistingCheck = PXPersistingCheck.Nothing)]
        [Account(DisplayName = "Reclassification Account", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Account.description), AvoidControlAccounts = true)]
		public virtual Int32? PrebookAcctID
		{
			get
			{
				return this._PrebookAcctID;
			}
			set
			{
				this._PrebookAcctID = value;
			}
		}
		#endregion
		#region PrebookSubID
		public abstract class prebookSubID : PX.Data.BQL.BqlInt.Field<prebookSubID> { }
		protected Int32? _PrebookSubID;


        /// <summary>
        /// The <see cref="PX.Objects.GL.Sub">subaccount</see> used to record the expenses pending reclassification.
        /// The field is relevant only if the <see cref="PX.Objects.CS.FeaturesSet.Prebooking">Support for Expense Reclassification feature</see> is activated 
        /// and the document has or has had the Prebooked (<c>"K"</c>) status. (See <see cref="APRegister.Prebooked"/>)
        /// </summary>
        /// <value>
        /// Defaults to the subaccount associated with the vendor of the document.
        /// Corresponds to the <see cref="PX.Objects.GL.Sub.SubID">AccountID</see> field.
        /// </value>
		[PXDefault(typeof(Select<Vendor, Where<Vendor.bAccountID, Equal<Current<APInvoice.vendorID>>, And<FeatureInstalled<FeaturesSet.prebooking>>>>), 
			SourceField = typeof(Vendor.prebookSubID), PersistingCheck = PXPersistingCheck.Nothing)]
        [SubAccount(typeof(APInvoice.prebookAcctID), DisplayName = "Reclassification Subaccount", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Sub.description))]
		public virtual Int32? PrebookSubID
		{
			get
			{
				return this._PrebookSubID;
			}
			set
			{
				this._PrebookSubID = value;
			}
		}
		#endregion
		#region Released
		public new abstract class released : PX.Data.BQL.BqlBool.Field<released> { }
		#endregion
		#region OpenDoc
		public new abstract class openDoc : PX.Data.BQL.BqlBool.Field<openDoc> { }
		#endregion
		#region Hold
		public new abstract class hold : PX.Data.BQL.BqlBool.Field<hold> { }
		#endregion
		#region Printed
		public new abstract class printed : PX.Data.BQL.BqlBool.Field<printed> { }
		#endregion
		#region Voided
		public new abstract class voided : PX.Data.BQL.BqlBool.Field<voided> { }
		#endregion		
		#region Prebooked
		public new abstract class prebooked : PX.Data.BQL.BqlBool.Field<prebooked> { }
		#endregion
		#region BatchNbr
		public new abstract class batchNbr : PX.Data.BQL.BqlString.Field<batchNbr> { }
		#endregion
		#region PrebookBatchNbr
		public new abstract class prebookBatchNbr : PX.Data.BQL.BqlString.Field<prebookBatchNbr> { }
		#endregion
		#region VoidBatchNbr
		public new abstract class voidBatchNbr : PX.Data.BQL.BqlString.Field<voidBatchNbr> { }
		#endregion
		#region ScheduleID
		public new abstract class scheduleID : PX.Data.BQL.BqlString.Field<scheduleID> { }
		#endregion
		#region Scheduled
		public new abstract class scheduled : PX.Data.BQL.BqlBool.Field<scheduled> { }
		#endregion
		#region DocDesc
		public new abstract class docDesc : PX.Data.BQL.BqlString.Field<docDesc> { }
		#endregion
		#region VendorID_Vendor_acctName
		public new abstract class vendorID_Vendor_acctName : PX.Data.BQL.BqlString.Field<vendorID_Vendor_acctName> { }
		#endregion
		#region EstPayDate
		public abstract class estPayDate : PX.Data.BQL.BqlDateTime.Field<estPayDate> { }
		protected DateTime? _EstPayDate;

        /// <summary>
        /// Estimated payment date.
        /// </summary>
        /// <value>
        /// The field is calculated and equals either <see cref="APInvoice.PayDate"/> if the document is
        /// <see cref="APInvoice.PaySel">selected for payment</see> or the <see cref="APInvoice.DueDate"/> otherwise.
        /// </value>
		[PXDBCalced(typeof(Switch<Case<Where<APInvoice.paySel,Equal<True>>, APInvoice.payDate>, APInvoice.dueDate>), typeof(DateTime))]
		public virtual DateTime? EstPayDate
		{
			get
			{
				return this._EstPayDate;
			}
			set
			{
				this._EstPayDate = value;
			}
		}
		#endregion
		#region LCEnabled
		public abstract class lCEnabled : PX.Data.BQL.BqlBool.Field<lCEnabled> { }
		protected Boolean? _LCEnabled = false;

		/// <summary>
		/// Indicates whether landed cost is enabled for the document.
		/// </summary>
		/// <value>
		/// Equals <c>true</c> if the vendor of the document is a <see cref="Vendor.LandedCostVendor">Landed Cost vendor</see>.
		/// </value>
		[PXBool()]
		[PXUIField(Visible = false)]
		public virtual Boolean? LCEnabled
		{
			get
			{
				return this._LCEnabled;
			}
			set
			{
				this._LCEnabled = value;
			}
		}
		#endregion
		#region IsTaxValid
		public new abstract class isTaxValid : PX.Data.BQL.BqlBool.Field<isTaxValid> { }
		#endregion
		#region IsTaxPosted
		public new abstract class isTaxPosted : PX.Data.BQL.BqlBool.Field<isTaxPosted> { }
		#endregion
		#region IsTaxSaved
		public new abstract class isTaxSaved : PX.Data.BQL.BqlBool.Field<isTaxSaved> { }
		#endregion
		#region NonTaxable
		public new abstract class nonTaxable : PX.Data.BQL.BqlBool.Field<nonTaxable> { }
		#endregion
        #region NoteID
        public new abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }

        /// <summary>
        /// Identifier of the <see cref="PX.Data.Note">Note</see> object, associated with the document.
        /// </summary>
        /// <value>
        /// Corresponds to the <see cref="PX.Data.Note.NoteID">Note.NoteID</see> field. 
        /// </value>
		[PXSearchable(SM.SearchCategory.AP, Messages.SearchableTitleDocument, new Type[] { typeof(APInvoice.docType), typeof(APInvoice.refNbr), typeof(APInvoice.vendorID), typeof(Vendor.acctName) },
			new Type[] { typeof(APInvoice.invoiceNbr), typeof(APInvoice.docDesc)},
			NumberFields = new Type[] { typeof(APInvoice.refNbr) },
			Line1Format = "{0:d}{1}{2}", Line1Fields = new Type[] { typeof(APInvoice.docDate), typeof(APInvoice.status), typeof(APInvoice.invoiceNbr) },
			Line2Format = "{0}", Line2Fields = new Type[] { typeof(APInvoice.docDesc) },
            MatchWithJoin = typeof(InnerJoin<Vendor, On<Vendor.bAccountID, Equal<APInvoice.vendorID>>>),
            SelectForFastIndexing = typeof(Select2<APInvoice, InnerJoin<Vendor, On<APInvoice.vendorID, Equal<Vendor.bAccountID>>>>)
		)]
		[PXNote(ShowInReferenceSelector = true, Selector = typeof(
			Search2<
				APInvoice.refNbr,
			InnerJoinSingleTable<APRegister, On<APInvoice.docType, Equal<APRegister.docType>,
				And<APInvoice.refNbr, Equal<APRegister.refNbr>>>,
			InnerJoinSingleTable<Vendor, On<APRegister.vendorID, Equal<Vendor.bAccountID>>>>,
			Where2<
				Where<APRegister.origModule, NotEqual<BatchModule.moduleTX>,
					Or<APRegister.released, Equal<True>>>,
				And<Match<Vendor, Current<AccessInfo.userName>>>>,
			OrderBy<Desc<APRegister.refNbr>>>))]
        public override Guid? NoteID
        {
            get
            {
                return this._NoteID;
            }
            set
            {
                this._NoteID = value;
            }
        }
        #endregion

        #region RefNoteID
        public new abstract class refNoteID : PX.Data.BQL.BqlGuid.Field<refNoteID> { }
        #endregion

		#region TaxCalcMode
		public new abstract class taxCalcMode : PX.Data.BQL.BqlString.Field<taxCalcMode> { }
		#endregion
		#region HasWithHoldTax
		public abstract class hasWithHoldTax : PX.Data.BQL.BqlBool.Field<hasWithHoldTax> { }
		protected bool? _HasWithHoldTax;
		[PXBool]
		[RestrictWithholdingTaxCalcMode(typeof(APInvoice.taxZoneID), typeof(APInvoice.taxCalcMode), typeof(APInvoice.origModule))]
	    public virtual bool? HasWithHoldTax
	    {
		    get { return this._HasWithHoldTax; }
		    set { this._HasWithHoldTax = value; }
	    }
	    #endregion
		#region HasUseTax
		public abstract class hasUseTax : PX.Data.BQL.BqlBool.Field<hasUseTax> { }
		protected bool? _HasUseTax;
		[PXBool]
		[RestrictUseTaxCalcMode(typeof(APInvoice.taxZoneID), typeof(APInvoice.taxCalcMode), typeof(APInvoice.origModule))]
		public virtual bool? HasUseTax
		{
			get { return this._HasUseTax; }
			set { this._HasUseTax = value; }
		}
		#endregion
		#region UsesManualVAT
		public abstract class usesManualVAT : PX.Data.BQL.BqlBool.Field<usesManualVAT> { }
		protected bool? _UsesManualVAT;
		[PXDBBool]
		[RestrictManualVAT(typeof(APInvoice.taxZoneID), typeof(APInvoice.taxCalcMode))]
		[PXUIField(DisplayName = "Manual VAT Entry", Enabled = false)]
		public virtual bool? UsesManualVAT
		{
			get
			{
				return this._UsesManualVAT;
			}
			set
			{
				this._UsesManualVAT = value;
			}
		}
		#endregion

		#region IsTaxDocument
		public abstract class isTaxDocument : PX.Data.BQL.BqlBool.Field<isTaxDocument> { }

		/// <summary>
		/// Specifies (if set to <c>true</c>) that the document is tax bill and orginally has been created by tax reporting process.
		/// </summary>
		[PXDBBool]
		public virtual bool? IsTaxDocument { get; set; }
		#endregion

		#region IsMigratedRecord
		public new abstract class isMigratedRecord : PX.Data.BQL.BqlBool.Field<isMigratedRecord> { }
		#endregion
		#region PaymentsByLinesAllowed
		public new abstract class paymentsByLinesAllowed : PX.Data.BQL.BqlBool.Field<paymentsByLinesAllowed> { }

		[PXDBBool]
		[PXUIField(DisplayName = "Pay by Line",
			Visibility = PXUIVisibility.Visible,
			FieldClass = nameof(FeaturesSet.PaymentsByLines))]
		[PXDefault(false)]
		public override bool? PaymentsByLinesAllowed
		{
			get;
			set;
		}
		#endregion
		#region IsJointPayees
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Joint Payees", FieldClass = nameof(FeaturesSet.Construction))]
		public bool? IsJointPayees
		{
			get;
			set;
		}
		public abstract class isJointPayees : BqlBool.Field<isJointPayees>
		{
		}
		#endregion

		#region RetainageApply
		public new abstract class retainageApply : PX.Data.BQL.BqlBool.Field<retainageApply> { }
		#endregion
		#region RetainageUnreleasedAmt
		public new abstract class retainageUnreleasedAmt : PX.Data.BQL.BqlDecimal.Field<retainageUnreleasedAmt> { }
		#endregion
		#region RetainageReleasedAmt
		public new abstract class retainageReleased : PX.Data.BQL.BqlDecimal.Field<retainageReleased> { }
		#endregion
		#region IsRetainageDocument
		public new abstract class isRetainageDocument : PX.Data.BQL.BqlBool.Field<isRetainageDocument> { }
		#endregion

		#region CuryLineDiscTotal
		public abstract class curyLineDiscTotal : PX.Data.BQL.BqlDecimal.Field<curyLineDiscTotal> { }
		/// <summary>
		/// The total <see cref="lineDiscTotal">discount of the document</see> (in the currency of the document),
		/// which is calculated as the sum of line discounts of the order.
		/// </summary>
		[PXDBCurrency(typeof(APInvoice.curyInfoID), typeof(APInvoice.lineDiscTotal))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Line Discounts", Enabled = false)]
		public virtual Decimal? CuryLineDiscTotal
		{
			get;
			set;
		}
		#endregion
		#region LineDiscTotal
		public abstract class lineDiscTotal : PX.Data.BQL.BqlDecimal.Field<lineDiscTotal> { }

		/// <summary>
		/// The total line discount of the document, which is calculated as the sum of line discounts of the invoice.
		/// </summary>
		[PXDBBaseCury()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Line Discounts", Enabled = false)]
		public virtual Decimal? LineDiscTotal
		{
			get;
			set;
		}
		#endregion
		#region DetailExtPriceTotal
		public abstract class detailExtPriceTotal : PX.Data.BQL.BqlDecimal.Field<detailExtPriceTotal> { }

		/// <summary>
		/// Sum of ext value (line total and line discount total)
		/// </summary>
		[PXBaseCury()]
		public virtual Decimal? DetailExtPriceTotal
		{
			get;
			set;
		}
		#endregion
		#region CuryDetailExtPriceTotal
		public abstract class curyDetailExtPriceTotal : PX.Data.BQL.BqlDecimal.Field<curyDetailExtPriceTotal> { }

		/// <summary>
		/// Sum of ext value (line total and line discount total)
		/// </summary>
		[PXCurrency(typeof(APInvoice.curyInfoID), typeof(APInvoice.detailExtPriceTotal))]
		[PXUnboundDefault(TypeCode.Decimal, "0.0")]
		[PXFormula(typeof(Add<Add<APInvoice.curyLineTotal, APInvoice.curyLineDiscTotal>, APInvoice.curyLineRetainageTotal>))]
		[PXUIField(DisplayName = "Detail Total")]
		public virtual Decimal? CuryDetailExtPriceTotal
		{
			get;
			set;
		}
		#endregion
		#region CuryOrderDiscTotal
		public abstract class curyOrderDiscTotal : PX.Data.BQL.BqlDecimal.Field<curyOrderDiscTotal> { }

		/// <summary>
		/// The total <see cref="orderDiscTotal">discount of the document</see> (in the currency of the document),
		/// which is calculated as the sum of all group, document and line discounts of the invoice.
		/// </summary>
		[PXCurrency(typeof(APInvoice.curyInfoID), typeof(APInvoice.orderDiscTotal))]
		[PXUnboundDefault(TypeCode.Decimal, "0.0")]
		[PXFormula(typeof(Add<APInvoice.curyDiscTot, APInvoice.curyLineDiscTotal>))]
		[PXUIField(DisplayName = "Discount Total", Enabled = false)]
		[PXUIVisible(typeof(FeatureInstalled<FeaturesSet.vendorDiscounts>))]
		public virtual Decimal? CuryOrderDiscTotal
		{
			get;
			set;
		}
		#endregion
		#region OrderDiscTotal
		public abstract class orderDiscTotal : PX.Data.BQL.BqlDecimal.Field<orderDiscTotal> { }

		/// <summary>
		/// The total discount of the document, which is calculated as the sum of group, document and line discounts of the invoice.
		/// </summary>
		[PXBaseCury()]
		[PXUnboundDefault(TypeCode.Decimal, "0.0")]
		[PXFormula(typeof(Add<APInvoice.discTot, APInvoice.lineDiscTotal>))]
		[PXUIField(DisplayName = "Discount Total")]
		public virtual Decimal? OrderDiscTotal
		{
			get;
			set;
		}
		#endregion

		#region DontApprove
		public new abstract class dontApprove : PX.Data.BQL.BqlBool.Field<dontApprove> { }
		/// <summary>
		/// Indicates that the current document should be excluded from the 
		/// approval process.
		/// </summary>
		[PXDBBool]
		[PXDefault(true, PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Don't Approve", Visible = false, Enabled = false)]
		public override bool? DontApprove
		{
			get;
			set;
		}
		#endregion
		#region PendingPPD
		public new abstract class pendingPPD : PX.Data.BQL.BqlBool.Field<pendingPPD> { }
		#endregion
		#region SetWarningOnDiscount
		[PXBool]
		public bool? SetWarningOnDiscount
		{
			get;
			set;
		}
		public abstract class setWarningOnDiscount : PX.Data.BQL.BqlBool.Field<setWarningOnDiscount> { }
		#endregion
		#region ManualEntry
		public abstract class manualEntry : PX.Data.BQL.BqlBool.Field<manualEntry> { }

		[PXBool]
		[PXDBCalced(typeof(False), typeof(bool))]
		public virtual bool? ManualEntry
		{
			get;
			set;
		}
		#endregion
				
		public new abstract class origDocType : PX.Data.BQL.BqlString.Field<origDocType> { }
		public new abstract class origRefNbr : PX.Data.BQL.BqlString.Field<origRefNbr> { }

		#region IntercompanyInvoiceNoteID
		public abstract class intercompanyInvoiceNoteID : PX.Data.BQL.BqlGuid.Field<intercompanyInvoiceNoteID> { }

		[PXDBGuid]
		[PXUIField(DisplayName = "Related AR Document", FieldClass = nameof(FeaturesSet.InterBranch))]
		[DocumentSelector(typeof(
			SearchFor<ARInvoice.noteID>
			.In<
				SelectFrom<ARInvoice>
				.LeftJoin<APInvoice>
					.On<ARInvoice.noteID.IsEqual<APInvoice.intercompanyInvoiceNoteID>>
				.InnerJoin<Branch> // Branch which extended to Customer
					.On<ARInvoice.customerID.IsEqual<Branch.bAccountID>>
				.InnerJoin<BranchAlias> // Originating AR Branch
					.On<ARInvoice.branchID.IsEqual<BranchAlias.branchID>>
				.Where<ARInvoice.released.IsEqual<True>
					.And<Branch.branchID.IsEqual<branchID.FromCurrent>>
					.And<BranchAlias.bAccountID.IsEqual<vendorID.FromCurrent>>
					.And<APInvoice.refNbr.IsNull>
					.And<ARInvoice.isHiddenInIntercompanySales.IsNotEqual<True>>
					.And<APInvoice.docType.FromCurrent.IsIn<APDocType.invoice, APDocType.creditAdj>
							.And<ARInvoice.docType.IsIn<ARDocType.invoice, ARDocType.debitMemo, ARDocType.smallCreditWO, ARDocType.finCharge>>
						.Or<APInvoice.docType.FromCurrent.IsEqual<APDocType.debitAdj>
							.And<ARInvoice.docType.IsEqual<ARDocType.creditMemo>>.And<ARInvoice.pendingPPD.IsNotEqual<True>>>>
					.Or<ARInvoice.noteID.IsEqual<APInvoice.intercompanyInvoiceNoteID.FromCurrent>>>>),
			typeof(ARInvoice.docType),
			typeof(ARInvoice.refNbr),
			typeof(ARInvoice.docDate),
			typeof(ARInvoice.finPeriodID),
			typeof(ARInvoice.curyOrigDocAmt),
			typeof(ARInvoice.curyID),
			typeof(ARInvoice.docDesc),
			typeof(ARInvoice.invoiceNbr),
			SubstituteKey = typeof(ARInvoice.documentKey),
			SelectorMode = PXSelectorMode.DisplayModeValue | PXSelectorMode.TextModeReadonly)]
		public virtual Guid? IntercompanyInvoiceNoteID { get; set; }
		#endregion

		#region DisableAutomaticDiscountCalculation
		public abstract class disableAutomaticDiscountCalculation : PX.Data.BQL.BqlBool.Field<disableAutomaticDiscountCalculation> { }
		protected Boolean? _DisableAutomaticDiscountCalculation;
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Disable Automatic Discount Update", Visible = false)]
		public virtual Boolean? DisableAutomaticDiscountCalculation
		{
			get { return this._DisableAutomaticDiscountCalculation; }
			set { this._DisableAutomaticDiscountCalculation = value; }
		}
		#endregion

		#region DocumentKey
		public new abstract class documentKey : PX.Data.BQL.BqlGuid.Field<documentKey> { }

		[PXString(50, IsUnicode = true)]
		[PXUIField(DisplayName = "Document Description", FieldClass = nameof(FeaturesSet.InterBranch))]
		[PXDBCalced(typeof(docType.Concat<Space>.Concat<refNbr>), typeof(string))]
		[DocumentKey(typeof(APDocType.ListAttribute))]
		public virtual string DocumentKey
		{
			get;
			set;
		}
		#endregion

		public new abstract class projectID : PX.Data.BQL.BqlInt.Field<projectID> { }
	}

	[PXHidden]
	public class APInvoice2 : APInvoice
    {
		public new abstract class docType : PX.Data.BQL.BqlString.Field<docType> { }
		public new abstract class refNbr : PX.Data.BQL.BqlString.Field<refNbr> { }
		public new abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }
	}
}

namespace PX.Objects.AP.Standalone
{
	[Serializable]
	[PXHidden(ServiceVisible = false)]
	public partial class APInvoice : PX.Data.IBqlTable
	{
		#region DocType
		public abstract class docType : PX.Data.BQL.BqlString.Field<docType> { }
		protected string _DocType;
		[PXDBString(3, IsKey = true, IsFixed = true)]
		[PXDefault()]
		public virtual String DocType
		{
			get
			{
				return this._DocType;
			}
			set
			{
				this._DocType = value;
			}
		}
		#endregion
		#region RefNbr
		public abstract class refNbr : PX.Data.BQL.BqlString.Field<refNbr> { }
		protected string _RefNbr;
		[PXDBString(15, IsUnicode = true, IsKey = true, InputMask = "")]
		[PXDefault()]
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
		#region TermsID
		public abstract class termsID : PX.Data.BQL.BqlString.Field<termsID> { }
		protected String _TermsID;
		[PXDBString(10, IsUnicode = true)]
		public virtual String TermsID
		{
			get
			{
				return this._TermsID;
			}
			set
			{
				this._TermsID = value;
			}
		}
		#endregion
		#region DueDate
		public abstract class dueDate : PX.Data.BQL.BqlDateTime.Field<dueDate> { }
		protected DateTime? _DueDate;
		[PXDBDate()]
		public virtual DateTime? DueDate
		{
			get
			{
				return this._DueDate;
			}
			set
			{
				this._DueDate = value;
			}
		}
		#endregion
		#region DiscDate
		public abstract class discDate : PX.Data.BQL.BqlDateTime.Field<discDate> { }
		protected DateTime? _DiscDate;
		[PXDBDate()]
		public virtual DateTime? DiscDate
		{
			get
			{
				return this._DiscDate;
			}
			set
			{
				this._DiscDate = value;
			}
		}
		#endregion
		#region InvoiceNbr
		public abstract class invoiceNbr : PX.Data.BQL.BqlString.Field<invoiceNbr> { }
		protected String _InvoiceNbr;
		[PXDBString(40, IsUnicode = true)]
		public virtual String InvoiceNbr
		{
			get
			{
				return this._InvoiceNbr;
			}
			set
			{
				this._InvoiceNbr = value;
			}
		}
		#endregion
		#region InvoiceDate
		public abstract class invoiceDate : PX.Data.BQL.BqlDateTime.Field<invoiceDate> { }
		protected DateTime? _InvoiceDate;
		[PXDBDate()]
		[PXDefault(TypeCode.DateTime, "01/01/1900")]
		[PXUIField(DisplayName = "Vendor Ref. Date", Visibility = PXUIVisibility.Invisible)]
		public virtual DateTime? InvoiceDate
		{
			get
			{
				return this._InvoiceDate;
			}
			set
			{
				this._InvoiceDate = value;
			}
		}
		#endregion
		#region TaxZoneID
		public abstract class taxZoneID : PX.Data.BQL.BqlString.Field<taxZoneID> { }
		protected String _TaxZoneID;
		[PXDBString(10, IsUnicode = true)]
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
		#region MasterRefNbr
		public abstract class masterRefNbr : PX.Data.BQL.BqlString.Field<masterRefNbr> { }
		protected String _MasterRefNbr;
		[PXDBString(15, IsUnicode = true)]
		public virtual String MasterRefNbr
		{
			get
			{
				return this._MasterRefNbr;
			}
			set
			{
				this._MasterRefNbr = value;
			}
		}
		#endregion
		#region InstallmentNbr
		public abstract class installmentNbr : PX.Data.BQL.BqlShort.Field<installmentNbr> { }
		protected Int16? _InstallmentNbr;
		[PXDBShort()]
		public virtual Int16? InstallmentNbr
		{
			get
			{
				return this._InstallmentNbr;
			}
			set
			{
				this._InstallmentNbr = value;
			}
		}
		#endregion
		#region CuryTaxTotal
		public abstract class curyTaxTotal : PX.Data.BQL.BqlDecimal.Field<curyTaxTotal> { }
		protected Decimal? _CuryTaxTotal;
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? CuryTaxTotal
		{
			get
			{
				return this._CuryTaxTotal;
			}
			set
			{
				this._CuryTaxTotal = value;
			}
		}
		#endregion
		#region TaxTotal
		public abstract class taxTotal : PX.Data.BQL.BqlDecimal.Field<taxTotal> { }
		protected Decimal? _TaxTotal;
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? TaxTotal
		{
			get
			{
				return this._TaxTotal;
			}
			set
			{
				this._TaxTotal = value;
			}
		}
		#endregion
		#region CuryTaxAmt
		public abstract class curyTaxAmt : PX.Data.BQL.BqlDecimal.Field<curyTaxAmt> { }
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? CuryTaxAmt { get; set; }
		#endregion
		#region TaxAmt
		public abstract class taxAmt : PX.Data.BQL.BqlDecimal.Field<taxAmt> { }
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? TaxAmt { get; set; }
		#endregion
		#region CuryVatTaxableTotal
		public abstract class curyVatTaxableTotal : PX.Data.BQL.BqlDecimal.Field<curyVatTaxableTotal> { }
		protected Decimal? _CuryVatTaxableTotal;
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? CuryVatTaxableTotal
		{
			get
			{
				return this._CuryVatTaxableTotal;
			}
			set
			{
				this._CuryVatTaxableTotal = value;
			}
		}
		#endregion
		#region VatTaxableTotal
		public abstract class vatTaxableTotal : PX.Data.BQL.BqlDecimal.Field<vatTaxableTotal> { }
		protected Decimal? _VatTaxableTotal;
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? VatTaxableTotal
		{
			get
			{
				return this._VatTaxableTotal;
			}
			set
			{
				this._VatTaxableTotal = value;
			}
		}
		#endregion
		#region CuryVatExemptTotal
		public abstract class curyVatExemptTotal : PX.Data.BQL.BqlDecimal.Field<curyVatExemptTotal> { }
		protected Decimal? _CuryVatExemptTotal;
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? CuryVatExemptTotal
		{
			get
			{
				return this._CuryVatExemptTotal;
			}
			set
			{
				this._CuryVatExemptTotal = value;
			}
		}
		#endregion
		#region VatExemptTotal
		public abstract class vatExemptTotal : PX.Data.BQL.BqlDecimal.Field<vatExemptTotal> { }
		protected Decimal? _VatExemptTotal;
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? VatExemptTotal
		{
			get
			{
				return this._VatExemptTotal;
			}
			set
			{
				this._VatExemptTotal = value;
			}
		}
		#endregion
		#region CuryLineTotal
		public abstract class curyLineTotal : PX.Data.BQL.BqlDecimal.Field<curyLineTotal> { }
		protected Decimal? _CuryLineTotal;
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? CuryLineTotal
		{
			get
			{
				return this._CuryLineTotal;
			}
			set
			{
				this._CuryLineTotal = value;
			}
		}
		#endregion
		#region LineTotal
		public abstract class lineTotal : PX.Data.BQL.BqlDecimal.Field<lineTotal> { }
		protected Decimal? _LineTotal;
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? LineTotal
		{
			get
			{
				return this._LineTotal;
			}
			set
			{
				this._LineTotal = value;
			}
		}
		#endregion
		#region SeparateCheck
		public abstract class separateCheck : PX.Data.BQL.BqlBool.Field<separateCheck> { }
		protected Boolean? _SeparateCheck;
		[PXDBBool()]
		[PXDefault(true)]
		public virtual Boolean? SeparateCheck
		{
			get
			{
				return this._SeparateCheck;
			}
			set
			{
				this._SeparateCheck = value;
			}
		}
		#endregion
		#region PaySel
		public abstract class paySel : PX.Data.BQL.BqlBool.Field<paySel> { }
		protected bool? _PaySel = false;
		[PXDBBool]
		[PXDefault(false)]
		public bool? PaySel
		{
			get
			{
				return _PaySel;
			}
			set
			{
				_PaySel = value;
			}
		}
		#endregion
		#region PayDate
		public abstract class payDate : PX.Data.BQL.BqlDateTime.Field<payDate> { }
		protected DateTime? _PayDate;
		[PXDBDate()]
		public virtual DateTime? PayDate
		{
			get
			{
				return this._PayDate;
			}
			set
			{
				this._PayDate = value;
			}
		}
		#endregion
		#region PayTypeID
		public abstract class payTypeID : PX.Data.BQL.BqlString.Field<payTypeID> { }
		protected String _PayTypeID;
		[PXDBString(10, IsUnicode = true)]
		public virtual String PayTypeID
		{
			get
			{
				return this._PayTypeID;
			}
			set
			{
				this._PayTypeID = value;
			}
		}
		#endregion
		#region PayAccountID
		public abstract class payAccountID : PX.Data.BQL.BqlInt.Field<payAccountID> { }
		protected Int32? _PayAccountID;
		[PXDBInt()]
		public virtual Int32? PayAccountID
		{
			get
			{
				return this._PayAccountID;
			}
			set
			{
				this._PayAccountID = value;
			}
		}
		#endregion
		#region PayLocationID
		public abstract class payLocationID : PX.Data.BQL.BqlInt.Field<payLocationID> { }
		protected Int32? _PayLocationID;
		[PXDBInt()]
		public virtual Int32? PayLocationID
		{
			get
			{
				return this._PayLocationID;
			}
			set
			{
				this._PayLocationID = value;
			}
		}
		#endregion
		#region PrebookAcctID
		public abstract class prebookAcctID : PX.Data.BQL.BqlInt.Field<prebookAcctID> { }
		protected Int32? _PrebookAcctID;

		[PXDBInt()]
		public virtual Int32? PrebookAcctID
		{
			get
			{
				return this._PrebookAcctID;
			}
			set
			{
				this._PrebookAcctID = value;
			}
		}
		#endregion
		#region PrebookSubID
		public abstract class prebookSubID : PX.Data.BQL.BqlInt.Field<prebookSubID> { }
		protected Int32? _PrebookSubID;

		[PXDBInt()]
		public virtual Int32? PrebookSubID
		{
			get
			{
				return this._PrebookSubID;
			}
			set
			{
				this._PrebookSubID = value;
			}
		}
		#endregion
		#region TaxCalcMode
		public abstract class taxCalcMode : PX.Data.IBqlField { }
		#endregion
		#region SuppliedByVendorID
		public abstract class suppliedByVendorID : PX.Data.BQL.BqlInt.Field<suppliedByVendorID> { }
        [PXDBInt]
        public virtual int? SuppliedByVendorID { get; set; }
		#endregion
        #region SuppliedByVendorLocationID
        public abstract class suppliedByVendorLocationID : PX.Data.BQL.BqlInt.Field<suppliedByVendorLocationID> { }
	    [PXDBInt]
	    public virtual int? SuppliedByVendorLocationID { get; set; }
		#endregion
		#region DisableAutomaticDiscountCalculation
		public abstract class disableAutomaticDiscountCalculation : PX.Data.BQL.BqlBool.Field<disableAutomaticDiscountCalculation> { }
		protected Boolean? _DisableAutomaticDiscountCalculation;
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Disable Automatic Discount Update")]
		public virtual Boolean? DisableAutomaticDiscountCalculation
		{
			get { return this._DisableAutomaticDiscountCalculation; }
			set { this._DisableAutomaticDiscountCalculation = value; }
		}
		#endregion
		#region IsJointPayees
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Joint Payees", FieldClass = nameof(FeaturesSet.Construction))]
		public bool? IsJointPayees
		{
			get;
			set;
		}
		public abstract class isJointPayees : BqlBool.Field<isJointPayees>
		{
		}
		#endregion
	}
}
