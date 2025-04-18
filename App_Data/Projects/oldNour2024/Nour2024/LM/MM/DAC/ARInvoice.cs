﻿namespace MyMaintaince
{
    using System;
    using PX.Data;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using PX.Objects.TX;
    using PX.Objects.CR;
    using PX.TM;
    using PX.Objects.EP;
    using SOInvoiceEntry = PX.Objects.SO.SOInvoiceEntry;
    using PX.Objects.PM;
    using PX.Objects.CA;
    /**
    public partial class ARInvoice : ARRegister, IInvoice
    {
        #region Selected
        public new abstract class selected : IBqlField
        {
        }
        #endregion
        #region DocType
        public new abstract class docType : PX.Data.IBqlField
        {
        }

        /// <summary>
        /// The type of the document.
        /// This field is a part of the compound key of the document.
        /// </summary>
        /// <value>
        /// The field can have one of the values described in <see cref="ARInvoiceType.ListAttribute"/>.
        /// </value>
        [PXDBString(3, IsKey = true, IsFixed = true)]
        [PXDefault()]
        [ARInvoiceType.List()]
        [PXUIField(DisplayName = "Type", Visibility = PXUIVisibility.SelectorVisible, Enabled = true, TabOrder = 0)]
        [PXFieldDescription]
        public override String DocType
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
        public new abstract class refNbr : PX.Data.IBqlField
        {
        }

        /// <summary>
        /// The reference number of the document.
        /// This field is a part of the compound key of the document.
        /// </summary>
        /// <value>
        /// For most document types, the reference number is generated automatically from the corresponding
        /// <see cref="Numbering">numbering sequence</see>, which is specified in the <see cref="ARSetup">Accounts Receivable module preferences</see>.
        /// </value>
        [PXDBString(15, IsKey = true, IsUnicode = true, InputMask = ">CCCCCCCCCCCCCCC")]
        [PXDefault()]
        [PXUIField(DisplayName = "Reference Nbr.", Visibility = PXUIVisibility.SelectorVisible, TabOrder = 1)]
        [ARInvoiceType.RefNbr(typeof(Search2<Standalone.ARRegisterAlias.refNbr,
            InnerJoinSingleTable<ARInvoice, On<ARInvoice.docType, Equal<Standalone.ARRegisterAlias.docType>,
                And<ARInvoice.refNbr, Equal<Standalone.ARRegisterAlias.refNbr>>>,
            InnerJoinSingleTable<Customer, On<Standalone.ARRegisterAlias.customerID, Equal<Customer.bAccountID>>>>,
            Where<Standalone.ARRegisterAlias.docType, Equal<Optional<ARInvoice.docType>>,
                And2<Where<Standalone.ARRegisterAlias.origModule, Equal<BatchModule.moduleAR>,
                    Or<Standalone.ARRegisterAlias.released, Equal<True>>>,
                And<Match<Customer, Current<AccessInfo.userName>>>>>,
            OrderBy<Desc<Standalone.ARRegisterAlias.refNbr>>>), Filterable = true, IsPrimaryViewCompatible = true)]
        [ARInvoiceType.Numbering()]
        [ARInvoiceNbr()]
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
        #region OrigModule
        public new abstract class origModule : PX.Data.IBqlField
        {
        }
        #endregion
        #region CustomerID
        public new abstract class customerID : PX.Data.IBqlField
        {
        }

        /// <summary>
        /// The identifier of the <see cref="Customer"/> record associated with the document.
        /// </summary>
        /// <value>
        /// Corresponds to the <see cref="BAccount.BAccountID"/> field.
        /// </value>
        [CustomerActive(Visibility = PXUIVisibility.SelectorVisible, DescriptionField = typeof(Customer.acctName), Filterable = true, TabOrder = 2)]
        [PXDefault()]
        public override Int32? CustomerID
        {
            get
            {
                return this._CustomerID;
            }
            set
            {
                this._CustomerID = value;
            }
        }
        #endregion
        #region CustomerID_Customer_acctName
        public new abstract class customerID_Customer_acctName : IBqlField { }
        #endregion
        #region CustomerLocationID
        public new abstract class customerLocationID : PX.Data.IBqlField
        {
        }
        #endregion
        #region BranchID
        public new abstract class branchID : PX.Data.IBqlField
        {
        }

        /// <summary>
        /// The identifier of the <see cref="Branch">branch</see> to which the document belongs.
        /// </summary>
        /// <value>
        /// Corresponds to the <see cref="Branch.BranchID"/> field.
        /// </value>
        [Branch(typeof(Coalesce<
            Search<Location.cBranchID, Where<Location.bAccountID, Equal<Current<ARRegister.customerID>>, And<Location.locationID, Equal<Current<ARRegister.customerLocationID>>>>>,
            Search<GL.Branch.branchID, Where<GL.Branch.branchID, Equal<Current<AccessInfo.branchID>>>>>), IsDetail = false)]
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
        #region CuryID
        public new abstract class curyID : PX.Data.IBqlField
        {
        }
        #endregion
        #region BillAddressID
        public abstract class billAddressID : PX.Data.IBqlField
        {
        }
        protected Int32? _BillAddressID;

        /// <summary>
        /// The identifier of the <see cref="ARAddress">Billing Address object</see>, associated with the customer.
        /// </summary>
        /// <value>
        /// Corresponds to the <see cref="ARAddress.AddressID"/> field.
        /// </value>
        [PXDBInt()]
        [ARAddress(typeof(Select2<Customer,
            InnerJoin<CR.Standalone.Location, On<CR.Standalone.Location.bAccountID, Equal<Customer.bAccountID>, And<CR.Standalone.Location.locationID, Equal<Customer.defLocationID>>>,
            InnerJoin<Address, On<Address.bAccountID, Equal<Customer.bAccountID>, And<Address.addressID, Equal<Customer.defBillAddressID>>>,
            LeftJoin<ARAddress, On<ARAddress.customerID, Equal<Address.bAccountID>, And<ARAddress.customerAddressID, Equal<Address.addressID>, And<ARAddress.revisionID, Equal<Address.revisionID>, And<ARAddress.isDefaultBillAddress, Equal<True>>>>>>>>,
            Where<Customer.bAccountID, Equal<Current<ARInvoice.customerID>>>>))]
        public virtual Int32? BillAddressID
        {
            get
            {
                return this._BillAddressID;
            }
            set
            {
                this._BillAddressID = value;
            }
        }
        #endregion
        #region BillContactID
        public abstract class billContactID : PX.Data.IBqlField
        {
        }

        /// <summary>
        /// The identifier of the <see cref="ARContact">Billing Contact object</see>, associated with the customer.
        /// </summary>
        /// <value>
        /// Corresponds to the <see cref="ARContact.ContactID"/> field.
        /// </value>
        [PXDBInt]
        [PXSelector(typeof(ARContact.contactID), ValidateValue = false)]    //Attribute for showing contact email field on Automatic Notifications screen in the list of availible emails for
        //Invoices and Memos screen. Relies on the work of platform, which uses PXSelector to compose email list
        [PXUIField(DisplayName = "Billing Contact", Visible = false)]		//Attribute for displaying user friendly contact email field on Automatic Notifications screen in the list of availible emails.
        [ARContact(typeof(Select2<Customer,
                            InnerJoin<
                                      CR.Standalone.Location, On<CR.Standalone.Location.bAccountID, Equal<Customer.bAccountID>,
                                  And<CR.Standalone.Location.locationID, Equal<Customer.defLocationID>>>,
                            InnerJoin<
                                      Contact, On<Contact.bAccountID, Equal<Customer.bAccountID>,
                                  And<Contact.contactID, Equal<Customer.defBillContactID>>>,
                            LeftJoin<
                                     ARContact, On<ARContact.customerID, Equal<Contact.bAccountID>,
                                 And<ARContact.customerContactID, Equal<Contact.contactID>,
                                 And<ARContact.revisionID, Equal<Contact.revisionID>,
                                 And<ARContact.isDefaultContact, Equal<True>>>>>>>>,
                            Where<Customer.bAccountID, Equal<Current<ARInvoice.customerID>>>>))]
        public virtual int? BillContactID
        {
            get;
            set;
        }
        #endregion
        #region ARAccountID
        public new abstract class aRAccountID : PX.Data.IBqlField
        {
        }
        #endregion
        #region ARSubID
        public new abstract class aRSubID : PX.Data.IBqlField
        {
        }
        #endregion
        #region TermsID
        public abstract class termsID : PX.Data.IBqlField
        {
        }
        protected String _TermsID;

        /// <summary>
        /// The identifier of the <see cref="Terms">Credit Terms</see> object associated with the document.
        /// </summary>
        /// <value>
        /// Defaults to the <see cref="Customer.TermsID">credit terms</see> that are selected for the <see cref="CustomerID">customer</see>.
        /// Corresponds to the <see cref="Terms.TermsID"/> field.
        /// </value>
        [PXDBString(10, IsUnicode = true)]
        [PXDefault(typeof(Search<Customer.termsID, Where<Customer.bAccountID, Equal<Current<ARInvoice.customerID>>, And<Current<ARInvoice.docType>, NotEqual<ARDocType.creditMemo>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Terms", Visibility = PXUIVisibility.Visible)]
        [PXSelector(typeof(Search<Terms.termsID, Where<Terms.visibleTo, Equal<TermsVisibleTo.all>, Or<Terms.visibleTo, Equal<TermsVisibleTo.customer>>>>), DescriptionField = typeof(Terms.descr), Filterable = true)]
        [Terms(typeof(ARInvoice.docDate), typeof(ARInvoice.dueDate), typeof(ARInvoice.discDate), typeof(ARInvoice.curyOrigDocAmt), typeof(ARInvoice.curyOrigDiscAmt))]
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
        public new abstract class dueDate : PX.Data.IBqlField
        {
        }

        /// <summary>
        /// The due date of the document.
        /// </summary>
        [PXDBDate()]
        [PXUIField(DisplayName = "Due Date", Visibility = PXUIVisibility.SelectorVisible)]
        public override DateTime? DueDate
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
        public abstract class discDate : PX.Data.IBqlField
        {
        }
        protected DateTime? _DiscDate;

        /// <summary>
        /// The date when the cash discount can be taken in accordance with the <see cref="ARInvoice.TermsID">credit terms</see>.
        /// </summary>
        [PXDBDate()]
        [PXUIField(DisplayName = "Cash Discount Date", Visibility = PXUIVisibility.SelectorVisible)]
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
        public abstract class invoiceNbr : PX.Data.IBqlField
        {
        }
        protected String _InvoiceNbr;

        /// <summary>
        /// The original reference number or ID assigned by the customer to the customer document.
        /// </summary>
        [PXDBString(40, IsUnicode = true)]
        [PXUIField(DisplayName = "Customer Order", Visibility = PXUIVisibility.SelectorVisible, Required = false)]
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
        public abstract class invoiceDate : PX.Data.IBqlField
        {
        }
        protected DateTime? _InvoiceDate;

        /// <summary>
        /// The original date assigned by the customer to the customer document.
        /// </summary>
        [PXDBDate()]
        [PXDefault(TypeCode.DateTime, "01/01/1900")]
        [PXUIField(DisplayName = "Customer Ref. Date")]
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
        public abstract class taxZoneID : PX.Data.IBqlField
        {
        }
        protected String _TaxZoneID;

        /// <summary>
        /// The identifier of the <see cref="TaxZone"/> associated with the document.
        /// </summary>
        /// <value>
        /// Corresponds to the <see cref="TaxZone.TaxZoneID"/> field.
        /// </value>
        [PXDBString(10, IsUnicode = true)]
        [PXUIField(DisplayName = "Customer Tax Zone", Visibility = PXUIVisibility.Visible)]
        [PXRestrictor(typeof(Where<TaxZone.isManualVATZone, Equal<False>>), TX.Messages.CantUseManualVAT)]
        [PXSelector(typeof(TaxZone.taxZoneID), DescriptionField = typeof(TaxZone.descr), Filterable = true)]
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
        #region AvalaraCustomerUsageType
        public abstract class avalaraCustomerUsageType : PX.Data.IBqlField
        {
        }
        protected String _AvalaraCustomerUsageType;

        /// <summary>
        /// The customer entity type for reporting purposes. The field is used if the system is integrated with AvaTax by Avalara
        /// and the <see cref="FeaturesSet.AvalaraTax">Avalara Tax Integration</see> feature is enabled.
        /// </summary>
        /// <value>
        /// The field can have one of the values described in <see cref = "TXAvalaraCustomerUsageType.ListAttribute" />.
        /// Defaults to the <see cref="Location.CAvalaraCustomerUsageType">customer entity type</see>
        /// that is specified for the <see cref="CustomerLocationID">location of the customer</see>.
        /// </value>
        [PXDefault(typeof(Search<Location.cAvalaraCustomerUsageType, Where<Location.bAccountID, Equal<Current<ARInvoice.customerID>>, And<Location.locationID, Equal<Current<ARInvoice.customerLocationID>>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
        [PXDBString(1, IsFixed = true)]
        [PXUIField(DisplayName = "Customer Usage Type")]
        [TX.TXAvalaraCustomerUsageType.List]
        public virtual String AvalaraCustomerUsageType
        {
            get
            {
                return this._AvalaraCustomerUsageType;
            }
            set
            {
                this._AvalaraCustomerUsageType = value;
            }
        }
        #endregion
        #region DocDate
        public new abstract class docDate : PX.Data.IBqlField
        {
        }
        #endregion
        #region MasterRefNbr
        public abstract class masterRefNbr : PX.Data.IBqlField
        {
        }
        protected String _MasterRefNbr;

        /// <summary>
        /// For the document representing one of several installments this field stores the <see cref="RefNbr"/>
        /// of the master document - the one, to which the installment belongs.
        /// </summary>
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
        #region InstallmentCntr
        public abstract class installmentCntr : PX.Data.IBqlField
        {
        }
        protected short? _InstallmentCntr;

        /// <summary>
        /// The counter of <see cref="TermsInstallment">installments</see> associated with the document.
        /// </summary>
        [PXDBShort()]
        public virtual short? InstallmentCntr
        {
            get
            {
                return this._InstallmentCntr;
            }
            set
            {
                this._InstallmentCntr = value;
            }
        }
        #endregion
        #region InstallmentNbr
        public abstract class installmentNbr : PX.Data.IBqlField
        {
        }
        protected Int16? _InstallmentNbr;

        /// <summary>
        /// For the document representing one of several installments this field stores the number of the installment.
        /// </summary>
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
        public abstract class curyTaxTotal : PX.Data.IBqlField
        {
        }
        protected Decimal? _CuryTaxTotal;

        /// <summary>
        /// The total amount of tax associated with the document.
        /// Given in the <see cref="CuryID">currency of the document</see>.
        /// </summary>
        [PXDBCurrency(typeof(ARInvoice.curyInfoID), typeof(ARInvoice.taxTotal))]
        [PXUIField(DisplayName = "Tax Total", Visibility = PXUIVisibility.Visible, Enabled = false)]
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
        public abstract class taxTotal : PX.Data.IBqlField
        {
        }
        protected Decimal? _TaxTotal;

        /// <summary>
        /// The total amount of tax associated with the document.
        /// Given in the <see cref="Company.BaseCuryID">base currency of the company</see>.
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
        public abstract class curyLineTotal : PX.Data.IBqlField
        {
        }
        protected Decimal? _CuryLineTotal;

        /// <summary>
        /// The total amount of the <see cref="ARTran">lines</see> of the document.
        /// Given in the <see cref="CuryID">currency of the document</see>.
        /// </summary>
        [PXDBCurrency(typeof(ARInvoice.curyInfoID), typeof(ARInvoice.lineTotal))]
        [PXUIField(DisplayName = "Detail Total", Visibility = PXUIVisibility.Visible, Enabled = false)]
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
        public abstract class lineTotal : PX.Data.IBqlField
        {
        }
        protected Decimal? _LineTotal;

        /// <summary>
        /// The total amount of the <see cref="ARTran">lines</see> of the document.
        /// Given in the <see cref="Company.BaseCuryID">base currency of the company</see>.
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

        #region CuryVatExemptTotal
        public abstract class curyVatExemptTotal : PX.Data.IBqlField
        {
        }
        protected Decimal? _CuryVatExemptTotal;

        /// <summary>
        /// The portion of the document total that is exempt from VAT.
        /// Given in the <see cref="CuryID">currency of the document</see>.
        /// This field is relevant only if the <see cref="FeaturesSet.VatReporting">VAT Reporting</see> feature is enabled.
        /// </summary>
        /// <value>
        /// The value of this field is calculated as the taxable amount for the tax with <see cref="Tax.ExemptTax"/> set to <c>true</c>.
        /// </value>
        [PXDBCurrency(typeof(ARInvoice.curyInfoID), typeof(ARInvoice.vatExemptTotal))]
        [PXUIField(DisplayName = "VAT Exempt Total", Visibility = PXUIVisibility.Visible, Enabled = false)]
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
        public abstract class vatExemptTotal : PX.Data.IBqlField
        {
        }
        protected Decimal? _VatExemptTotal;

        /// <summary>
        /// The portion of the document total that is exempt from VAT.
        /// Given in the <see cref="Company.BaseCuryID">base currency</see> of the company.
        /// This field is relevant only if the <see cref="FeaturesSet.VatReporting">VAT Reporting</see> feature is enabled.
        /// </summary>
        /// <value>
        /// The value of this field is calculated as the taxable amount for the tax with <see cref="Tax.ExemptTax"/> set to <c>true</c>.
        /// </value>
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
        public abstract class curyVatTaxableTotal : PX.Data.IBqlField
        {
        }
        protected Decimal? _CuryVatTaxableTotal;

        /// <summary>
        /// The portion of the document total that is subjected to VAT.
        /// Given in the <see cref="CuryID">currency</see> of the document.
        /// This field is relevant only if the <see cref="FeaturesSet.VatReporting">VAT Reporting</see> feature is enabled.
        /// </summary>
        [PXDBCurrency(typeof(ARInvoice.curyInfoID), typeof(ARInvoice.vatTaxableTotal))]
        [PXUIField(DisplayName = "VAT Taxable Total", Visibility = PXUIVisibility.Visible, Enabled = false)]
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
        public abstract class vatTaxableTotal : PX.Data.IBqlField
        {
        }
        protected Decimal? _VatTaxableTotal;

        /// <summary>
        /// The portion of the document total that is subjected to VAT.
        /// Given in the <see cref="Company.BaseCuryID">base currency</see> of the company.
        /// This field is relevant only if the <see cref="FeaturesSet.VatReporting">VAT Reporting</see> feature is enabled.
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
        public new abstract class curyInfoID : PX.Data.IBqlField
        {
        }
        #endregion
        #region CuryOrigDocAmt
        public new abstract class curyOrigDocAmt : PX.Data.IBqlField
        {
        }
        #endregion
        #region OrigDocAmt
        public new abstract class origDocAmt : PX.Data.IBqlField
        {
        }
        #endregion
        #region CuryDocBal
        public new abstract class curyDocBal : PX.Data.IBqlField
        {
        }
        #endregion
        #region DocBal
        public new abstract class docBal : PX.Data.IBqlField
        {
        }
        #endregion
        #region CuryDiscBal
        public new abstract class curyDiscBal : PX.Data.IBqlField
        {
        }
        #endregion
        #region DiscBal
        public new abstract class discBal : PX.Data.IBqlField
        {
        }
        #endregion
        #region CuryOrigDiscAmt
        public new abstract class curyOrigDiscAmt : PX.Data.IBqlField
        {
        }
        #endregion
        #region OrigDiscAmt
        public new abstract class origDiscAmt : PX.Data.IBqlField
        {
        }
        #endregion
        #region DrCr
        public abstract class drCr : PX.Data.IBqlField
        {
        }
        protected string _DrCr;

        /// <summary>
        /// Read-only field indicating whether the document is of debit or credit type.
        /// The value of this field is based solely on the <see cref="DocType"/> field.
        /// </summary>
        /// <value>
        /// Possible values are <see cref="GL.DrCr.Credit"/> (for Invoice, Debit Memo, Financial Charge, Small Credit Write-Off and Cash Sale)
        /// and <see cref="GL.DrCr.Debit"/> (for Credit Memo and Cash Return).
        /// </value>
        [PXString(1, IsFixed = true)]
        public virtual string DrCr
        {
            [PXDependsOnFields(typeof(docType))]
            get
            {
                return ARInvoiceType.DrCr(this._DocType);
            }
            set
            {
            }
        }
        #endregion

        #region DiscTot
        public new abstract class discTot : PX.Data.IBqlField
        {
        }
        #endregion
        #region CuryDiscTot
        public new abstract class curyDiscTot : PX.Data.IBqlField
        {
        }
        #endregion
        #region DocDisc
        public new abstract class docDisc : PX.Data.IBqlField
        {
        }
        #endregion
        #region CuryDocDisc
        public new abstract class curyDocDisc : PX.Data.IBqlField
        {
        }
        #endregion

        #region Released
        public new abstract class released : PX.Data.IBqlField
        {
        }
        #endregion
        #region OpenDoc
        public new abstract class openDoc : PX.Data.IBqlField
        {
        }
        #endregion
        #region Hold
        public new abstract class hold : PX.Data.IBqlField
        {
        }
        #endregion
        #region BatchNbr
        public new abstract class batchNbr : PX.Data.IBqlField
        {
        }
        #endregion
        #region CommnPct
        public abstract class commnPct : PX.Data.IBqlField
        {
        }
        protected Decimal? _CommnPct;

        /// <summary>
        /// The commission percent used for the salesperson.
        /// </summary>
        [PXDBDecimal(6)]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Commission %", Enabled = false)]
        public virtual Decimal? CommnPct
        {
            get
            {
                return this._CommnPct;
            }
            set
            {
                this._CommnPct = value;
            }
        }
        #endregion
        #region CuryCommnAmt
        public abstract class curyCommnAmt : PX.Data.IBqlField
        {
        }
        protected Decimal? _CuryCommnAmt;

        /// <summary>
        /// The commission amount calculated on this document for the salesperson.
        /// Given in the <see cref="CuryID">currency</see> of the document.
        /// </summary>
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXDBCurrency(typeof(ARInvoice.curyInfoID), typeof(ARInvoice.commnAmt))]
        //[PXFormula(typeof(Mult<ARInvoice.curyCommnblAmt, Div<ARInvoice.commnPct, decimal100>>))]
        [PXUIField(DisplayName = "Commission Amt.", Enabled = false)]
        public virtual Decimal? CuryCommnAmt
        {
            get
            {
                return this._CuryCommnAmt;
            }
            set
            {
                this._CuryCommnAmt = value;
            }
        }
        #endregion
        #region CommnAmt
        public abstract class commnAmt : PX.Data.IBqlField
        {
        }
        protected Decimal? _CommnAmt;

        /// <summary>
        /// The commission amount calculated on this document for the salesperson.
        /// Given in the <see cref="Company.BaseCuryID">base currency</see> of the company.
        /// </summary>
        [PXDBDecimal(4)]
        [PXDefault(TypeCode.Decimal, "0.0")]
        public virtual Decimal? CommnAmt
        {
            get
            {
                return this._CommnAmt;
            }
            set
            {
                this._CommnAmt = value;
            }
        }
        #endregion
        #region ApplyOverdueCharge
        public abstract class applyOverdueCharge : PX.Data.IBqlField
        {
        }
        protected bool? _ApplyOverdueCharge;

        /// <summary>
        /// Specifies (if set to <c>true</c>) that the document can be available
        /// on the Calculate Overdue Charges (AR507000) processing form.
        /// </summary>
        [PXDBBool]
        [PXUIField(DisplayName = Messages.ApplyOverdueCharges, Visibility = PXUIVisibility.Visible)]
        [PXDefault(true)]
        public virtual bool? ApplyOverdueCharge
        {
            get
            {
                return _ApplyOverdueCharge;
            }
            set
            {
                _ApplyOverdueCharge = value;
            }
        }
        #endregion
        #region LastFinChargeDate
        public abstract class lastFinChargeDate : PX.Data.IBqlField
        {
        }
        protected DateTime? _LastFinChargeDate;

        /// <summary>
        /// The date of the most recent <see cref="ARFinChargeTran">Financial Charge</see> associated with this document.
        /// </summary>
        [PXDate()]
        [PXUIField(DisplayName = "Last Fin. Charge Date", Visibility = PXUIVisibility.SelectorVisible)]
        public virtual DateTime? LastFinChargeDate
        {
            get
            {
                return this._LastFinChargeDate;
            }
            set
            {
                this._LastFinChargeDate = value;
            }
        }
        #endregion

        #region LastPaymentDate
        public abstract class lastPaymentDate : PX.Data.IBqlField
        {
        }
        protected DateTime? _LastPaymentDate;

        /// <summary>
        /// The date of the most recent payment associated with this document.
        /// </summary>
        [PXDate()]
        [PXUIField(DisplayName = "Last Payment Date")]
        public virtual DateTime? LastPaymentDate
        {
            get
            {
                return this._LastPaymentDate;
            }
            set
            {
                this._LastPaymentDate = value;
            }
        }
        #endregion
        #region CuryCommnblAmt
        public abstract class curyCommnblAmt : PX.Data.IBqlField
        {
        }
        protected Decimal? _CuryCommnblAmt;

        /// <summary>
        /// The amount used as the base to calculate commission for this document.
        /// Given in the <see cref="CuryID">currency</see> of the document.
        /// </summary>
        [PXDBCurrency(typeof(ARInvoice.curyInfoID), typeof(ARInvoice.commnblAmt))]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Total Commissionable", Enabled = false)]
        public virtual Decimal? CuryCommnblAmt
        {
            get
            {
                return this._CuryCommnblAmt;
            }
            set
            {
                this._CuryCommnblAmt = value;
            }
        }
        #endregion
        #region CommnblAmt
        public abstract class commnblAmt : PX.Data.IBqlField
        {
        }
        protected Decimal? _CommnblAmt;

        /// <summary>
        /// The amount used as the base to calculate commission for this document.
        /// Given in the <see cref="Company.BaseCuryID">base currency</see> of the company.
        /// </summary>
        [PXDBDecimal(4)]
        [PXDefault(TypeCode.Decimal, "0.0")]
        public virtual Decimal? CommnblAmt
        {
            get
            {
                return this._CommnblAmt;
            }
            set
            {
                this._CommnblAmt = value;
            }
        }
        #endregion
        #region CuryWhTaxBal
        public abstract class curyWhTaxBal : PX.Data.IBqlField
        {
        }

        /// <summary>
        /// The balance of tax withheld on the document.
        /// Given in the <see cref="CuryID">currency</see> of the document.
        /// </summary>
        [PXDecimal(4)]
        [PXFormula(typeof(decimal0))]
        public virtual Decimal? CuryWhTaxBal
        {
            get;
            set;
        }

        #endregion
        #region WhTaxBal
        public abstract class whTaxBal : PX.Data.IBqlField
        {
        }

        /// <summary>
        /// The balance of tax withheld on the document.
        /// Given in the <see cref="Company.BaseCuryID">base currency</see> of the company.
        /// </summary>
        [PXDecimal(4)]
        [PXFormula(typeof(decimal0))]
        public virtual Decimal? WhTaxBal
        {
            get;
            set;
        }
        #endregion
        #region ScheduleID
        public new abstract class scheduleID : IBqlField
        {
        }
        #endregion
        #region Scheduled
        public new abstract class scheduled : IBqlField
        {
        }
        #endregion
        #region CreatedByID
        public new abstract class createdByID : PX.Data.IBqlField
        {
        }
        #endregion
        #region LastModifiedByID
        public new abstract class lastModifiedByID : PX.Data.IBqlField
        {
        }
        #endregion
        #region DontPrint
        public abstract class dontPrint : PX.Data.IBqlField
        {
        }
        protected Boolean? _DontPrint;

        /// <summary>
        /// When set to <c>true</c> indicates that the document should not be sent to the <see cref="CustomerID">Customer</see>
        /// as a printed document, and thus the system should not include it in the list of documents available for mass-printing.
        /// </summary>
        /// <value>
        /// Defaults to the value of the <see cref="Customer.PrintInvoices"/> setting of the <see cref="CustomerID">Customer</see>.
        /// </value>
        [PXDBBool()]
        [PXDefault(true)]
        [PXUIField(DisplayName = "Don't Print")]
        public virtual Boolean? DontPrint
        {
            get
            {
                return this._DontPrint;
            }
            set
            {
                this._DontPrint = value;
            }
        }
        #endregion
        #region Printed
        public abstract class printed : PX.Data.IBqlField
        {
        }
        protected Boolean? _Printed;

        /// <summary>
        /// Specifies (if set to <c>true</c>) that the document has been printed.
        /// </summary>
        [PXDBBool()]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Printed", Enabled = false)]
        public virtual Boolean? Printed
        {
            get
            {
                return this._Printed;
            }
            set
            {
                this._Printed = value;
            }
        }
        #endregion
        #region DontEmail
        public abstract class dontEmail : PX.Data.IBqlField
        {
        }
        protected Boolean? _DontEmail;

        /// <summary>
        /// When set to <c>true</c> indicates that the document should not be sent to the <see cref="CustomerID">Customer</see>
        /// by email, and thus the system should not include it in the list of documents available for mass-emailing.
        /// </summary>
        /// <value>
        /// Defaults to the value of the <see cref="Customer.MailInvoices"/> setting of the <see cref="CustomerID">Customer</see>.
        /// </value>
        [PXDBBool()]
        [PXDefault(true)]
        [PXUIField(DisplayName = "Don't Email")]
        public virtual Boolean? DontEmail
        {
            get
            {
                return this._DontEmail;
            }
            set
            {
                this._DontEmail = value;
            }
        }
        #endregion
        #region Emailed
        public abstract class emailed : PX.Data.IBqlField
        {
        }
        protected Boolean? _Emailed;

        /// <summary>
        /// Specifies (if set to <c>true</c>) that the document has been emailed to the <see cref="ARInvoice.CustomerID">customer</see>.
        /// </summary>
        [PXDBBool()]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Emailed", Enabled = false)]
        public virtual Boolean? Emailed
        {
            get
            {
                return this._Emailed;
            }
            set
            {
                this._Emailed = value;
            }
        }
        #endregion
        #region Voided
        public new abstract class voided : PX.Data.IBqlField
        {
        }
        #endregion
        #region PrintInvoice
        public abstract class printInvoice : IBqlField { }

        /// <summary>
        /// When set to <c>true</c>, indicates that the document awaits printing.
        /// The field is used in automation steps for the Invoices and Memos (SO303000) form and thus defines
        /// participates in determining whether the document is available for processing
        /// on the Process Invoices and Memos (SO505000) form.
        /// </summary>
        [PXBool]
        [PXDBCalced(typeof(Switch<Case<Where<dontPrint, Equal<False>, And<printed, Equal<False>>>, True>, False>), typeof(Boolean))]
        public virtual bool? PrintInvoice
        {
            [PXDependsOnFields(typeof(dontPrint), typeof(printed))]
            get
            {
                return _DontPrint != true && (_Printed == null || _Printed == false);
            }
        }
        #endregion
        #region EmailInvoice
        public abstract class emailInvoice : IBqlField { }

        /// <summary>
        /// When set to <c>true</c>, indicates that the document awaits emailing.
        /// The field is used in automation steps for the Invoices and Memos (SO303000) form and thus defines
        /// participates in determining whether the document is available for processing
        /// on the Process Invoices and Memos (SO505000) form.
        /// </summary>
        [PXBool]
        [PXDBCalced(typeof(Switch<Case<Where<dontEmail, Equal<False>, And<emailed, Equal<False>>>, True>, False>), typeof(Boolean))]
        public virtual bool? EmailInvoice
        {
            [PXDependsOnFields(typeof(dontEmail), typeof(emailed))]
            get
            {
                return _DontEmail != true && (_Emailed == null || _Emailed == false);
            }
        }
        #endregion
        #region CreditHold
        public abstract class creditHold : PX.Data.IBqlField
        {
        }
        protected Boolean? _CreditHold;

        /// <summary>
        /// When set to <c>true</c> indicates that the document is on credit hold,
        /// which means that the credit check failed for the <see cref="CustomerID">Customer</see>.
        /// The document can't be released while it's on credit hold.
        /// </summary>
        [PXDBBool()]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Credit Hold")]
        public virtual Boolean? CreditHold
        {
            get
            {
                return this._CreditHold;
            }
            set
            {
                this._CreditHold = value;
            }
        }
        #endregion
        #region ApprovedCredit
        public abstract class approvedCredit : PX.Data.IBqlField
        {
        }
        protected Boolean? _ApprovedCredit;

        /// <summary>
        /// Specifies (if set to <c>true</c>) that credit has been approved for the document.
        /// </summary>
        [PXDBBool()]
        [PXDefault(false)]
        public virtual Boolean? ApprovedCredit
        {
            get
            {
                return this._ApprovedCredit;
            }
            set
            {
                this._ApprovedCredit = value;
            }
        }
        #endregion
        #region ApprovedCreditAmt
        public abstract class approvedCreditAmt : PX.Data.IBqlField
        {
        }
        protected Decimal? _ApprovedCreditAmt;

        /// <summary>
        /// The amount of credit approved for the document.
        /// </summary>
        [PXDBDecimal(4)]
        [PXDefault(TypeCode.Decimal, "0.0")]
        public virtual Decimal? ApprovedCreditAmt
        {
            get
            {
                return this._ApprovedCreditAmt;
            }
            set
            {
                this._ApprovedCreditAmt = value;
            }
        }
        #endregion
        #region ProjectID
        public abstract class projectID : PX.Data.IBqlField
        {
        }
        protected Int32? _ProjectID;

        /// <summary>
        /// The identifier of the <see cref="PMProject">project</see> associated with the document
        /// or the <see cref="PMSetup.NonProjectCode">non-project code</see>, which indicates that the document is not related to any particular project.
        /// </summary>
        /// <value>
        /// Corresponds to the <see cref="PMProject.ProjectID"/> field.
        /// </value>
        [ProjectDefault(BatchModule.AR, typeof(Search<Location.cDefProjectID, Where<Location.bAccountID, Equal<Current<ARInvoice.customerID>>, And<Location.locationID, Equal<Current<ARInvoice.customerLocationID>>>>>))]
        [PM.ActiveProjectForAR(typeof(ARInvoice.customerID))]
        public virtual Int32? ProjectID
        {
            get
            {
                return this._ProjectID;
            }
            set
            {
                this._ProjectID = value;
            }
        }
        #endregion
        #region PaymentMethodID
        public abstract class paymentMethodID : PX.Data.IBqlField
        {
        }
        protected string _PaymentMethodID;

        /// <summary>
        /// The identifier of the <see cref="PaymentMethod">payment method</see> that is used for the document.
        /// </summary>
        /// <value>
        /// Corresponds to the <see cref="PaymentMethod.PaymentMethodID"/> field.
        /// </value>
        [PXDBString(10, IsUnicode = true)]
        [PXDefault(typeof(Coalesce<Search2<CustomerPaymentMethod.paymentMethodID,
                                            InnerJoin<Customer, On<CustomerPaymentMethod.bAccountID, Equal<Customer.bAccountID>,
                                                And<CustomerPaymentMethod.pMInstanceID, Equal<Customer.defPMInstanceID>,
                                                And<CustomerPaymentMethod.isActive, Equal<True>>>>,
                                            InnerJoin<PaymentMethod, On<PaymentMethod.paymentMethodID, Equal<CustomerPaymentMethod.paymentMethodID>,
                                                And<PaymentMethod.useForAR, Equal<True>,
                                                And<PaymentMethod.isActive, Equal<True>>>>>>,
                                            Where<Customer.bAccountID, Equal<Current<ARInvoice.customerID>>>>,
                                   Search2<Customer.defPaymentMethodID, InnerJoin<PaymentMethod, On<PaymentMethod.paymentMethodID, Equal<Customer.defPaymentMethodID>,
                                                And<PaymentMethod.useForAR, Equal<True>,
                                                And<PaymentMethod.isActive, Equal<True>>>>>,
                                         Where<Customer.bAccountID, Equal<Current<ARInvoice.customerID>>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
        [PXSelector(typeof(Search5<PaymentMethod.paymentMethodID, LeftJoin<CustomerPaymentMethod, On<CustomerPaymentMethod.paymentMethodID, Equal<PaymentMethod.paymentMethodID>,
                                    And<CustomerPaymentMethod.bAccountID, Equal<Current<ARInvoice.customerID>>>>>,
                                Where<PaymentMethod.isActive, Equal<True>,
                                And<PaymentMethod.useForAR, Equal<True>,
                                And<Where<PaymentMethod.aRIsOnePerCustomer, Equal<True>,
                                    Or<Where<CustomerPaymentMethod.pMInstanceID, IsNotNull>>>>>>, Aggregate<GroupBy<PaymentMethod.paymentMethodID>>>), DescriptionField = typeof(PaymentMethod.descr))]
        [PXUIFieldAttribute(DisplayName = "Payment Method")]
        public virtual String PaymentMethodID
        {
            get
            {
                return this._PaymentMethodID;
            }
            set
            {
                this._PaymentMethodID = value;
            }
        }
        #endregion
        #region PMInstanceID
        public abstract class pMInstanceID : PX.Data.IBqlField
        {
        }
        protected int? _PMInstanceID;

        /// <summary>
        /// The identifier of the <see cref="CustomerPaymentMethod">customer payment method</see> (card or account number) associated with the document.
        /// </summary>
        /// <value>
        /// Defaults according to the settings of the <see cref="CustomerPaymentMethod">customer payment methods</see>
        /// that are specified for the <see cref="CustomerID">customer</see> associated with the document.
        /// Corresponds to the <see cref="CustomerPaymentMethod.PMInstanceID"/> field.
        /// </value>
        [PXDBInt()]
        [PXUIField(DisplayName = "Card/Account No")]
        [PXDefault(typeof(Coalesce<
                        Search2<Customer.defPMInstanceID, InnerJoin<CustomerPaymentMethod, On<CustomerPaymentMethod.pMInstanceID, Equal<Customer.defPMInstanceID>,
                                And<CustomerPaymentMethod.bAccountID, Equal<Customer.bAccountID>>>>,
                                Where<Customer.bAccountID, Equal<Current2<ARInvoice.customerID>>,
                                And<CustomerPaymentMethod.isActive, Equal<True>,
                                And<CustomerPaymentMethod.paymentMethodID, Equal<Current2<ARInvoice.paymentMethodID>>>>>>,
                        Search<CustomerPaymentMethod.pMInstanceID,
                                Where<CustomerPaymentMethod.bAccountID, Equal<Current2<ARInvoice.customerID>>,
                                    And<CustomerPaymentMethod.paymentMethodID, Equal<Current2<ARInvoice.paymentMethodID>>,
                                    And<CustomerPaymentMethod.isActive, Equal<True>>>>,
                                OrderBy<Desc<CustomerPaymentMethod.expirationDate,
                                Desc<CustomerPaymentMethod.pMInstanceID>>>>>)
                        , PersistingCheck = PXPersistingCheck.Nothing)]
        [PXSelector(typeof(Search<CustomerPaymentMethod.pMInstanceID, Where<CustomerPaymentMethod.bAccountID, Equal<Current2<ARInvoice.customerID>>,
            And<CustomerPaymentMethod.paymentMethodID, Equal<Current2<ARInvoice.paymentMethodID>>,
            And<Where<CustomerPaymentMethod.isActive, Equal<True>, Or<CustomerPaymentMethod.pMInstanceID,
                    Equal<Current<ARInvoice.pMInstanceID>>>>>>>>), DescriptionField = typeof(CustomerPaymentMethod.descr))]
        public virtual int? PMInstanceID
        {
            get
            {
                return _PMInstanceID;
            }
            set
            {
                _PMInstanceID = value;
            }
        }
        #endregion

        #region CashAccountID
        public abstract class cashAccountID : PX.Data.IBqlField
        {
        }
        protected Int32? _CashAccountID;

        /// <summary>
        /// The identifier of the <see cref="CashAccount">Cash Account</see> associated with the document.
        /// </summary>
        /// <value>
        /// Defaults to the <see cref="CustomerPaymentMethod.CashAccountID">Cash Account</see> selected for the <see cref="PMInstanceID">Customer Payment Method</see>,
        /// or (if the above is unavailable) to the Cash Account selected as the default one for Accounts Receivable in the settings of the
        /// <see cref="PaymentMethodID">Payment Method</see> (see the <see cref="PaymentMethodAccount.ARIsDefault"/> field).
        /// </value>
        [PXDefault(typeof(Coalesce<Search2<CustomerPaymentMethod.cashAccountID,
                                        InnerJoin<PaymentMethodAccount, On<PaymentMethodAccount.cashAccountID, Equal<CustomerPaymentMethod.cashAccountID>,
                                            And<PaymentMethodAccount.paymentMethodID, Equal<CustomerPaymentMethod.paymentMethodID>,
                                            And<PaymentMethodAccount.useForAR, Equal<True>>>>>,
                                        Where<CustomerPaymentMethod.bAccountID, Equal<Current<ARInvoice.customerID>>,
                                            And<CustomerPaymentMethod.pMInstanceID, Equal<Current2<ARInvoice.pMInstanceID>>>>>,
                            Search2<CashAccount.cashAccountID,
                                InnerJoin<PaymentMethodAccount, On<PaymentMethodAccount.cashAccountID, Equal<CashAccount.cashAccountID>,
                                    And<PaymentMethodAccount.useForAR, Equal<True>,
                                    And<PaymentMethodAccount.aRIsDefault, Equal<True>,
                                    And<PaymentMethodAccount.paymentMethodID, Equal<Current2<ARInvoice.paymentMethodID>>>>>>>,
                                    Where<CashAccount.branchID, Equal<Current<ARInvoice.branchID>>,
                                        And<Match<Current<AccessInfo.userName>>>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
        [CashAccount(typeof(ARInvoice.branchID), typeof(Search2<CashAccount.cashAccountID,
                InnerJoin<PaymentMethodAccount, On<PaymentMethodAccount.cashAccountID, Equal<CashAccount.cashAccountID>,
                    And<PaymentMethodAccount.paymentMethodID, Equal<Current<ARInvoice.paymentMethodID>>,
                    And<PaymentMethodAccount.useForAR, Equal<True>>>>>, Where<Match<Current<AccessInfo.userName>>>>), Visibility = PXUIVisibility.Visible)]
        public virtual Int32? CashAccountID
        {
            get
            {
                return this._CashAccountID;
            }
            set
            {
                this._CashAccountID = value;
            }
        }
        #endregion
        #region IsCCPayment
        public abstract class isCCPayment : PX.Data.IBqlField
        {
        }

        protected bool? _IsCCPayment = false;

        /// <summary>
        /// Specifies (if set to <c>true</c>) that the document is paid with a credit card.
        /// </summary>
        [PXBool()]
        public virtual bool? IsCCPayment
        {
            get
            {
                return this._IsCCPayment;
            }
            set
            {
                this._IsCCPayment = value;
            }
        }
        #endregion
        #region Status
        public new abstract class status : IBqlField { }

        /// <summary>
        /// The status of the document.
        /// The value of the field is determined by the values of the status flags,
        /// such as <see cref="Hold"/>, <see cref="Released"/>, <see cref="Voided"/>, <see cref="Scheduled"/>.
        /// </summary>
        /// <value>
        /// The field can have one of the values described in <see cref="ARDocStatus.ListAttribute"/>.
        /// Defaults to <see cref="ARDocStatus.Hold"/>.
        /// </value>
        /// </value>
        [PXDBString(1, IsFixed = true)]
        [PXDefault(ARDocStatus.Hold)]
        [PXUIField(DisplayName = "Status", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
        [ARDocStatus.List]
        [SetStatus]
        [PXDependsOnFields(
            typeof(ARInvoice.voided),
            typeof(ARInvoice.hold),
            typeof(ARInvoice.creditHold),
            typeof(ARInvoice.printed),
            typeof(ARInvoice.dontPrint),
            typeof(ARInvoice.emailed),
            typeof(ARInvoice.dontEmail),
            typeof(ARInvoice.isCCPayment),
            typeof(ARInvoice.scheduled),
            typeof(ARInvoice.released),
            typeof(ARInvoice.openDoc))]
        public override string Status
        {
            get
            {
                return this._Status;
            }
            set
            {
                this._Status = value;
            }
        }
        #endregion
        #region DocDesc
        public new abstract class docDesc : PX.Data.IBqlField
        {
        }
        #endregion
        #region Methods
        /// <summary>
        /// This attribute is intended for the status syncronization in the ARInvoice<br/>
        /// Namely, it sets a corresponeded string to the Status field, depending <br/>
        /// upon Voided, Released, CreditHold, Hold, Sheduled,Released, OpenDoc, PrintInvoice,EmailInvoice<br/>
        /// of the ARInvoice<br/>
        /// [SetStatus()]
        /// </summary>
        protected new class SetStatusAttribute : PXEventSubscriberAttribute, IPXRowUpdatingSubscriber, IPXRowInsertingSubscriber
        {
            protected class Definition : IPrefetchable
            {
                public Boolean? _PrintBeforeRelease;
                public Boolean? _EmailBeforeRelease;
                void IPrefetchable.Prefetch()
                {
                    using (PXDataRecord rec =
                        PXDatabase.SelectSingle<ARSetup>(
                        new PXDataField("PrintBeforeRelease"),
                        new PXDataField("EmailBeforeRelease")))
                    {
                        _PrintBeforeRelease = rec != null ? rec.GetBoolean(0) : false;
                        _EmailBeforeRelease = rec != null ? rec.GetBoolean(1) : false;
                    }
                }
            }

            protected Definition _Definition;

            public override void CacheAttached(PXCache sender)
            {
                base.CacheAttached(sender);
                _Definition = PXDatabase.GetSlot<Definition>(typeof(SetStatusAttribute).FullName, typeof(ARSetup));
                sender.Graph.FieldUpdating.AddHandler<ARInvoice.hold>((cache, e) =>
                {
                    PXBoolAttribute.ConvertValue(e);

                    ARInvoice item = e.Row as ARInvoice;
                    if (item != null)
                    {
                        StatusSet(cache, item, (bool?)e.NewValue, item.CreditHold, item.Printed != true && item.DontPrint != true, item.Emailed != true && item.DontEmail != true);
                    }
                });

                sender.Graph.FieldUpdating.AddHandler<ARInvoice.creditHold>((cache, e) =>
                {
                    PXBoolAttribute.ConvertValue(e);

                    ARInvoice item = e.Row as ARInvoice;
                    if (item != null)
                    {
                        StatusSet(cache, item, item.Hold, (bool?)e.NewValue, item.Printed != true && item.DontPrint != true, item.Emailed != true && item.DontEmail != true);
                    }
                });

                sender.Graph.FieldUpdating.AddHandler<ARInvoice.printed>((cache, e) =>
                {
                    PXBoolAttribute.ConvertValue(e);

                    ARInvoice item = e.Row as ARInvoice;
                    if (item != null)
                    {
                        StatusSet(cache, item, item.Hold, item.CreditHold, (bool?)e.NewValue != true && item.DontPrint != true, item.Emailed != true && item.DontEmail != true);
                    }
                });
                sender.Graph.FieldUpdating.AddHandler<ARInvoice.emailed>((cache, e) =>
                {
                    PXBoolAttribute.ConvertValue(e);

                    ARInvoice item = e.Row as ARInvoice;
                    if (item != null)
                    {
                        StatusSet(cache, item, item.Hold, item.CreditHold, item.Printed != true && item.DontPrint != true, (bool?)e.NewValue != true && item.DontEmail != true);
                    }
                });
                sender.Graph.FieldUpdating.AddHandler<ARInvoice.dontPrint>((cache, e) =>
                {
                    PXBoolAttribute.ConvertValue(e);

                    ARInvoice item = e.Row as ARInvoice;
                    if (item != null)
                    {
                        StatusSet(cache, item, item.Hold, item.CreditHold, (bool?)e.NewValue != true && item.Printed != true, item.Emailed != true && item.DontEmail != true);
                    }
                });
                sender.Graph.FieldUpdating.AddHandler<ARInvoice.dontEmail>((cache, e) =>
                {
                    PXBoolAttribute.ConvertValue(e);

                    ARInvoice item = e.Row as ARInvoice;
                    if (item != null)
                    {
                        StatusSet(cache, item, item.Hold, item.CreditHold, item.Printed != true && item.DontPrint != true, (bool?)e.NewValue != true && item.Emailed != true);
                    }
                });


                sender.Graph.FieldVerifying.AddHandler<ARInvoice.status>((cache, e) => { e.NewValue = cache.GetValue<ARInvoice.status>(e.Row); });
                sender.Graph.RowSelecting.AddHandler<ARInvoice>(RowSelecting);
            }

            protected virtual void StatusSet(PXCache cache, ARInvoice item, bool? HoldVal, bool? CreditHoldVal, bool? toPrint, bool? toEmail)
            {
                if (item.Voided == true)
                {
                    item.Status = ARDocStatus.Voided;
                }
                else if (CreditHoldVal == true && item.IsCCPayment == false)
                {
                    item.Status = ARDocStatus.CreditHold;
                }
                else if (CreditHoldVal == true && item.IsCCPayment == true)
                {
                    item.Status = ARDocStatus.CCHold;
                }
                else if (HoldVal == true)
                {
                    item.Status = ARDocStatus.Hold;
                }
                else if (item.Scheduled == true)
                {
                    item.Status = ARDocStatus.Scheduled;
                }
                else if (item.Released == false)
                {
                    if ((item.DocType == ARDocType.Invoice || item.DocType == ARDocType.CreditMemo || item.DocType == ARDocType.DebitMemo))
                    {
                        if (_Definition != null && _Definition._PrintBeforeRelease == true && toPrint == true)
                            item.Status = ARDocStatus.PendingPrint;
                        else if (_Definition != null && _Definition._EmailBeforeRelease == true && toEmail == true)
                            item.Status = ARDocStatus.PendingEmail;
                        else
                            item.Status = ARDocStatus.Balanced;
                    }
                    else
                        item.Status = ARDocStatus.Balanced;
                }
                else if (item.OpenDoc == true)
                {
                    item.Status = ARDocStatus.Open;
                }
                else if (item.OpenDoc == false)
                {
                    item.Status = ARDocStatus.Closed;
                }
            }

            public virtual void RowSelecting(PXCache sender, PXRowSelectingEventArgs e)
            {
                ARInvoice item = (ARInvoice)e.Row;
                if (item != null)
                    StatusSet(sender, item, item.Hold, item.CreditHold, item.Printed != true && item.DontPrint != true, item.Emailed != true && item.DontEmail != true);
            }

            public virtual void RowInserting(PXCache sender, PXRowInsertingEventArgs e)
            {
                ARInvoice item = (ARInvoice)e.Row;
                StatusSet(sender, item, item.Hold, item.CreditHold, item.Printed != true && item.DontPrint != true, item.Emailed != true && item.DontEmail != true);
            }

            public virtual void RowUpdating(PXCache sender, PXRowUpdatingEventArgs e)
            {
                ARInvoice item = (ARInvoice)e.NewRow;
                StatusSet(sender, item, item.Hold, item.CreditHold, item.Printed != true && item.DontPrint != true, item.Emailed != true && item.DontEmail != true);
            }
        }
        #endregion
        #region WorkgroupID
        public abstract class workgroupID : PX.Data.IBqlField
        {
        }
        protected int? _WorkgroupID;

        /// <summary>
        /// The workgroup that is responsible for the document.
        /// </summary>
        /// <value>
        /// Corresponds to the <see cref="PX.TM.EPCompanyTree.WorkGroupID">EPCompanyTree.WorkGroupID</see> field.
        /// </value>
        [PXDBInt]
        [PXDefault(typeof(Customer.workgroupID), PersistingCheck = PXPersistingCheck.Nothing)]
        [PXCompanyTreeSelector]
        [PXUIField(DisplayName = "Workgroup", Visibility = PXUIVisibility.Visible)]
        public virtual int? WorkgroupID
        {
            get
            {
                return this._WorkgroupID;
            }
            set
            {
                this._WorkgroupID = value;
            }
        }
        #endregion
        #region OwnerID
        public abstract class ownerID : IBqlField { }
        protected Guid? _OwnerID;

        /// <summary>
        /// The <see cref="EPEmployee">Employee</see> responsible for the document.
        /// </summary>
        /// <value>
        /// Corresponds to the <see cref="EPEmployee.PKID"/> field.
        /// </value>
        [PXDBGuid()]
        [PXDefault(typeof(Customer.ownerID), PersistingCheck = PXPersistingCheck.Nothing)]
        [PXOwnerSelector(typeof(ARInvoice.workgroupID))]
        [PXUIField(DisplayName = "Owner", Visibility = PXUIVisibility.SelectorVisible)]
        public virtual Guid? OwnerID
        {
            get
            {
                return this._OwnerID;
            }
            set
            {
                this._OwnerID = value;
            }
        }
        #endregion
        #region SalesPersonID
        public new abstract class salesPersonID : PX.Data.IBqlField
        {
        }

        /// <summary>
        /// The identifier of the <see cref="CustSalesPeople">salesperson</see> to whom the document belongs.
        /// </summary>
        /// <value>
        /// Corresponds to the <see cref="CustSalesPeople.SalesPersonID"/> field.
        /// </value>
        [SalesPerson(DisplayName = "Default Salesperson")]
        [PXDefault(typeof(Search<CustDefSalesPeople.salesPersonID, Where<CustDefSalesPeople.bAccountID, Equal<Current<ARRegister.customerID>>, And<CustDefSalesPeople.locationID, Equal<Current<ARRegister.customerLocationID>>, And<CustDefSalesPeople.isDefault, Equal<True>>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
        public override Int32? SalesPersonID
        {
            get
            {
                return this._SalesPersonID;
            }
            set
            {
                this._SalesPersonID = value;
            }
        }
        #endregion
        #region NoteID
        public new abstract class noteID : PX.Data.IBqlField
        {
        }

        /// <summary>
        /// The identifier of the <see cref="PX.Data.Note">Note</see> object associated with the document.
        /// </summary>
        /// <value>
        /// Corresponds to the <see cref="PX.Data.Note.NoteID">Note.NoteID</see> field.
        /// </value>
        [PXSearchable(SM.SearchCategory.AR, "AR {0}: {1} - {3}", new Type[] { typeof(ARInvoice.docType), typeof(ARInvoice.refNbr), typeof(ARInvoice.customerID), typeof(Customer.acctName) },
            new Type[] { typeof(ARInvoice.invoiceNbr), typeof(ARInvoice.docDesc) },
            NumberFields = new Type[] { typeof(ARInvoice.refNbr) },
            Line1Format = "{0:d}{1}{2}", Line1Fields = new Type[] { typeof(ARInvoice.docDate), typeof(ARInvoice.status), typeof(ARInvoice.invoiceNbr) },
            Line2Format = "{0}", Line2Fields = new Type[] { typeof(ARInvoice.docDesc) },
            WhereConstraint = typeof(Where<Current<ARInvoice.origModule>, NotEqual<BatchModule.moduleSO>, And<ARRegister.docType, NotEqual<ARDocType.cashSale>, And<ARRegister.docType, NotEqual<ARDocType.cashReturn>>>>),//do not index SOInvoice as ARInvoice.
            MatchWithJoin = typeof(InnerJoin<Customer, On<Customer.bAccountID, Equal<ARInvoice.customerID>>>),
            SelectForFastIndexing = typeof(Select2<ARInvoice, InnerJoin<Customer, On<ARInvoice.customerID, Equal<Customer.bAccountID>>>, Where<ARInvoice.origModule, NotEqual<BatchModule.moduleSO>, And<ARRegister.docType, NotEqual<ARDocType.cashSale>, And<ARRegister.docType, NotEqual<ARDocType.cashReturn>>>>>)
        )]
        [PXNote(ShowInReferenceSelector = true)]
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
        public abstract class refNoteID : PX.Data.IBqlField
        {
        }
        protected Guid? _RefNoteID;

        /// <summary>
        /// The identifier of the <see cref="PX.Data.Note">Note</see> object associated with the document reference.
        /// </summary>
        /// <value>
        /// Corresponds to the <see cref="PX.Data.Note.NoteID">Note.NoteID</see> field.
        /// </value>
        [PXDBGuid()]
        public virtual Guid? RefNoteID
        {
            get
            {
                return this._RefNoteID;
            }
            set
            {
                this._RefNoteID = value;
            }
        }
        #endregion
        #region Hidden
        public abstract class hidden : PX.Data.IBqlField
        {
        }
        protected Boolean? _Hidden = false;

        /// <summary>
        /// Specifies (if set to <c>true</c>) that the document can be associated with only one sales order
        /// (which happens when <see cref="SO.SOOrder.BillSeparately"/> is set to <c>true</c> for the sales order).
        /// </summary>
        [PXBool()]
        public virtual Boolean? Hidden
        {
            get
            {
                return this._Hidden;
            }
            set
            {
                this._Hidden = value;
            }
        }
        #endregion
        #region HiddenOrderType
        public abstract class hiddenOrderType : PX.Data.IBqlField
        {
        }
        protected string _HiddenOrderType;

        /// <summary>
        /// The <see cref="SO.SOOrder.OrderType"/> type of the related sales order when the document can be associated
        /// with only one sales order (which happens when <see cref="SO.SOOrder.BillSeparately"/> is set to <c>true</c> for the sales order).
        /// </summary>
        [PXString()]
        public virtual string HiddenOrderType
        {
            get
            {
                return this._HiddenOrderType;
            }
            set
            {
                this._HiddenOrderType = value;
            }
        }
        #endregion
        #region HiddenOrderNbr
        public abstract class hiddenOrderNbr : PX.Data.IBqlField
        {
        }
        protected string _HiddenOrderNbr;

        /// <summary>
        /// The <see cref="SO.SOOrder.OrderNbr"/> reference number of the related sales order when the document can be associated
        /// with only one sales order (which happens when <see cref="SO.SOOrder.BillSeparately"/> is set to <c>true</c> for the sales order).
        /// </summary>
        [PXString()]
        public virtual string HiddenOrderNbr
        {
            get
            {
                return this._HiddenOrderNbr;
            }
            set
            {
                this._HiddenOrderNbr = value;
            }
        }
        #endregion
        #region IsTaxValid
        public new abstract class isTaxValid : PX.Data.IBqlField
        {
        }
        #endregion
        #region IsTaxPosted
        public new abstract class isTaxPosted : PX.Data.IBqlField
        {
        }
        #endregion
        #region IsTaxSaved
        public new abstract class isTaxSaved : PX.Data.IBqlField
        {
        }
        #endregion
        #region ApplyPaymentWhenTaxAvailable
        public abstract class applyPaymentWhenTaxAvailable : PX.Data.IBqlField
        {
        }

        /// <summary>
        /// Specifies (if set to <c>true</c>) that the Avalara taxes should be included in the document balance calculation
        /// (because these taxes will be calculated only during the release process).
        /// </summary>
        [PXBool()]
        public virtual bool? ApplyPaymentWhenTaxAvailable { get; set; }

        #endregion

        #region Revoked
        public abstract class revoked : PX.Data.IBqlField
        {
        }

        /// <summary>
        /// Specifies (if set to <c>true</c>) that the document has been revoked.
        /// </summary>
        [PXDBBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = Messages.Revoked, Enabled = true, Visible = false)]
        public virtual bool? Revoked
        {
            get;
            set;
        }
        #endregion

        #region PendingPPD
        public new abstract class pendingPPD : PX.Data.IBqlField
        {
        }
        #endregion
    }
    **/
}
