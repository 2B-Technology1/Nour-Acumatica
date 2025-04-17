using ARCashSale = PX.Objects.AR.Standalone.ARCashSale;
using CRLocation = PX.Objects.CR.Standalone.Location;
using IRegister = PX.Objects.CM.IRegister;
using PX.Data;
using PX.Objects.AR;
using PX.Objects.CM;
using PX.Objects.Common;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Objects.GL;
using PX.Objects.SO;
using PX.Objects;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System;
using PX.Objects.AP;
using PX.Objects.CA;
using Maintenance;

namespace PX.Objects.AR
{
    public class ARRegisterExt : PXCacheExtension<ARRegister>
  {
        #region UsrBankRefNbr
        [PXDBString(20, IsKey = false, IsFixed = false, IsUnicode = true)]
        [PXUIField(DisplayName = "Bank Ref. No", Visible = true)]
        public virtual string UsrBankRefNbr { get; set; }
        public abstract class usrBankRefNbr : IBqlField { }
        #endregion

        #region UsrJobOrdID
        [PXDBString(50, IsKey = false, IsUnicode = true)]
        [PXUIField(DisplayName = "JobOrdID")]
        public virtual string UsrJobOrdID { get; set; }
        public abstract class usrJobOrdID : IBqlField { }
        #endregion

        #region UsrDueDate
        //[PXDBDate]
        //[PXUIField(DisplayName = "S Due Date")]
        //public virtual DateTime? UsrDueDate { get; set; }
        //public abstract class usrDueDate : IBqlField { }
        //#endregion

        //#region UsrDueDate
        [PXDBDate(IsKey = false, BqlTable = typeof(PX.Objects.AR.ARRegister))]
        [PXDefault(typeof(AccessInfo.businessDate),PersistingCheck=PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Due Date3")]
        public virtual DateTime? UsrDueDate { get; set; }
        public abstract class usrDueDate : IBqlField { }
        #endregion
        #region UsrSONbr
        [PXDBString(30)]
        [PXUIField(DisplayName = "SO Nbr")]

        [PXSelector(typeof(Search<SOOrder.orderNbr, Where<SOOrder.customerID, Equal<Current<ARRegister.customerID>>>>)
                    , new Type[]{
                  typeof(SOOrder.orderType),
                  typeof(SOOrder.orderNbr),
                  typeof(SOOrder.customerID)})]
        public virtual string UsrSONbr { get; set; }
        public abstract class usrSONbr : IBqlField { }
        #endregion
        #region UsrCheckNbr
        [PXDBString(50, IsKey = false, BqlTable = typeof(ARRegister), IsFixed = false, IsUnicode = true)]
        [PXUIField(DisplayName = "Check No")]
        [PXSelector(typeof(Search2<CheckNo.checkNumber,
                 LeftJoin<ARRegister, On<CheckNo.checkNumber, Equal<ARRegisterExt.usrCheckNbr>>,
                 LeftJoin<CAAdj, On<CAAdjExt.usrCheckNbr, Equal<CheckNo.checkNumber>>>>,
                 Where<CheckNo.cashAccountID, Equal<Current<ARPayment.cashAccountID>>,
                 And2<Where<ARRegister.released, IsNull, Or<ARRegister.released, Equal<False>>>,
                 And2<Where<CAAdj.released, IsNull, Or<CAAdj.released, Equal<False>>>,
                 Or<ARRegister.released, Equal<True>, And<ARRegister.refNbr, Equal<Current<ARRegister.refNbr>>>>>>>>),
                    new Type[]{
                  typeof(CheckNo.checkNumber),
                  typeof(ARRegister.refNbr),
                  typeof(CAAdj.adjRefNbr)},
                    SubstituteKey = typeof(CheckNo.checkNumber))]

        public virtual string UsrCheckNbr { get; set; }
        public abstract class usrCheckNbr : IBqlField { }
        #endregion
        #region UsrRefNbr
        [PXDBString(10, IsKey = false, BqlTable = typeof(PX.Objects.AR.ARRegister), IsFixed = false, IsUnicode = true, InputMask = ">CCCCCCCCCCCCCCC")]
        [PXUIField(DisplayName = "Custom RefNbr")]
        public virtual string UsrRefNbr { get; set; }
        public abstract class usrRefNbr : IBqlField { }
        #endregion

        #region UsrBranchName
        [PXDBString(15, IsKey = false, BqlTable = typeof(PX.Objects.AR.ARRegister), IsFixed = false, IsUnicode = true)]
        [PXUIField(DisplayName = "Branch")]
        [PXSelector(typeof(Maintenance.BRBank.bRID),
             new Type[]
            {
                typeof(Maintenance.BRBank.bRID),
                typeof(Maintenance.BRBank.bRName)
            },
             SubstituteKey = typeof(Maintenance.BRBank.bRID),
             DescriptionField = typeof(Maintenance.BRBank.bRName))]
        public virtual string UsrBranchName { get; set; }
        public abstract class usrBranchName : IBqlField { }
        #endregion

        #region UsrBankName
        [PXDBString(50, IsKey = false, BqlTable = typeof(PX.Objects.AR.ARRegister), IsFixed = false, IsUnicode = true)]
        [PXUIField(DisplayName = "Bank Name")]
        [PXSelector(typeof(Maintenance.Banks.bankID),
             new Type[]
            {
                typeof(Maintenance.Banks.bankID),
                typeof(Maintenance.Banks.bankName)
            },
             SubstituteKey = typeof(Maintenance.Banks.bankID),
             DescriptionField = typeof(Maintenance.Banks.bankName))]
        public virtual string UsrBankName { get; set; }
        public abstract class usrBankName : IBqlField { }
        #endregion

        #region UsrNRAccount
        [PXDBString(10, IsKey = false, BqlTable = typeof(PX.Objects.AR.ARRegister), IsFixed = false, IsUnicode = true)]
        [PXUIField(DisplayName = "NR Account")]
        public virtual string UsrNRAccount { get; set; }
        public abstract class usrNRAccount : IBqlField { }
        #endregion

        #region UsrCUCAccount
        [PXDBString(10, IsKey = false, BqlTable = typeof(PX.Objects.AR.ARRegister), IsFixed = false, IsUnicode = true)]
        [PXUIField(DisplayName = "CUC Account")]
        public virtual string UsrCUCAccount { get; set; }
        public abstract class usrCUCAccount : IBqlField { }
        #endregion

        #region UsrBankAccount
        [PXDBString(15, IsKey = false, BqlTable = typeof(PX.Objects.AR.ARRegister), IsFixed = false, IsUnicode = true)]
        [PXUIField(DisplayName = "Bank Account")]
        public virtual string UsrBankAccount { get; set; }
        public abstract class usrBankAccount : IBqlField { }
        #endregion

        #region UsrNRRefNbr
        [PXDBString(20, IsKey = false, BqlTable = typeof(PX.Objects.AR.ARRegister), IsFixed = false, IsUnicode = true)]
        [PXUIField(DisplayName = "NR Ref No.")]
        public virtual string UsrNRRefNbr { get; set; }
        public abstract class usrNRRefNbr : IBqlField { }
        #endregion

        #region UsrDepositRefNbr
        [PXDBString(30, IsKey = false, BqlTable = typeof(PX.Objects.AR.ARRegister), IsFixed = false, IsUnicode = true)]
        [PXUIField(DisplayName = "CUC Ref No.")]
        public virtual string UsrDepositRefNbr { get; set; }
        public abstract class usrDepositRefNbr : IBqlField { }
        #endregion

        #region UsrBankDepNbr
        [PXDBString(20, IsKey = false, BqlTable = typeof(PX.Objects.AR.ARRegister), IsFixed = false, IsUnicode = true)]
        [PXUIField(DisplayName = "Bank Deposit Ref")]
        public virtual string UsrBankDepNbr { get; set; }
        public abstract class usrBankDepNbr : IBqlField { }
        #endregion

        #region UsrGLBalane
        [PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXCury(typeof(ARPayment.curyID))]
        [PXUIField(DisplayName = "GL Balance", Enabled = false)]
        [PX.Objects.CA.GLBalance(typeof(ARPayment.cashAccountID), null, typeof(ARPayment.docDate))]
        public virtual Decimal? UsrGLBalane { get; set; }
        public abstract class usrGLBalane : IBqlField { }
        #endregion

        #region UsrCashBalance
        [PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXCury(typeof(ARPayment.curyID))]
        [PXUIField(DisplayName = "Available Balance", Enabled = false)]
        [PX.Objects.CA.CashBalance(typeof(ARPayment.cashAccountID))]
        public virtual Decimal? UsrCashBalance { get; set; }
        public abstract class usrCashBalance : IBqlField { }
        #endregion

        #region UsrNR
        [PXDefault(false)]
        [PXDBBool(IsKey = false, BqlTable = typeof(PX.Objects.AR.ARRegister))]
        [PXUIField(DisplayName = "NR", Visible = false)]
        public virtual bool? UsrNR { get; set; }
        public abstract class usrNR : IBqlField { }
        #endregion

        #region UsrCUC
        [PXDefault(false)]
        [PXDBBool(IsKey = false, BqlTable = typeof(PX.Objects.AR.ARRegister))]
        [PXUIField(DisplayName = "CUC", Visible = false)]
        public virtual bool? UsrCUC { get; set; }
        public abstract class usrCUC : IBqlField { }
        #endregion

        #region UsrBankDeposited
        [PXDBBool(IsKey = false, BqlTable = typeof(PX.Objects.AR.ARRegister))]
        [PXUIField(DisplayName = "Bank Deposited", Visible = false)]
        public virtual bool? UsrBankDeposited { get; set; }
        public abstract class usrBankDeposited : IBqlField { }
        #endregion
        #region UsrRefunded
        [PXDefault(false)]
        [PXDBBool(IsKey = false, BqlTable = typeof(PX.Objects.AR.ARRegister))]
        [PXUIField(DisplayName = "Refunded", Visible = false)]
        public virtual bool? UsrRefunded { get; set; }
        public abstract class usrRefunded : IBqlField { }
        #endregion
        #region UsrPayReturn
        [PXDBBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "PayReturn")]

        public virtual bool? UsrPayReturn { get; set; }
        public abstract class usrPayReturn : IBqlField { }
        #endregion
        #region UsrReturnBankName
        [PXDBString(200)]
        [PXUIField(DisplayName = "ReturnBankName")]

        public virtual string UsrReturnBankName { get; set; }
        public abstract class usrReturnBankName : IBqlField { }
        #endregion
        #region UsrCustRefundRefNbr
        [PXDBString(15, IsKey = false, BqlTable = typeof(ARRegister), IsFixed = false, IsUnicode = true)]
        [PXUIField(DisplayName = "Cust Refund RefNbr")]
        [PXSelector(typeof(Search2<ARRegister.refNbr,
                 InnerJoin<ARPayment, On<ARRegister.refNbr, Equal<ARPayment.refNbr>,
                 And<ARRegister.docType, Equal<ARPayment.docType>>>>,
                 Where<ARRegister.customerID, Equal<Current<ARRegister.customerID>>,
                 And<ARPayment.cashAccountID, Equal<Current<ARPayment.cashAccountID>>,
                 And<ARRegister.released, Equal<True>,
                 And<ARRegister.docType, Equal<ARDocType.refund>,
                 And<ARRegisterExt.usrPayReturn, Equal<False>>>>>>>),
                    new Type[]{
                  typeof(ARRegister.refNbr),
                  typeof(ARRegister.curyOrigDocAmt),
                  typeof(ARPayment.adjDate)},
                    SubstituteKey = typeof(ARRegister.refNbr))]

        //public PXFilteredProcessingJoin<ARPayment, ProcessFilter,
        //  InnerJoin<CashAccount, On<CashAccount.cashAccountID, Equal<ARPayment.cashAccountID>>,
        //  InnerJoin<BankCheck, On<BankCheck.cashAccountID, Equal<Current<ProcessFilter.cashAccountID>>>>>,
        //  Where<CashAccountExt.usrCheckDispersant, Equal<True>,
        // And<ARPayment.docType, Equal<ARDocType.refund>,
        //  And<ARPayment.cashAccountID, Equal<BankCheck.checkAccountID>,
        // And<ARRegisterExt.usrPayReturn, Equal<False>>>>>> Records;

        //public PXFilteredProcessingJoin<ARRegister, ProcessFilter,
        //                        InnerJoin<ARPayment, On<ARRegister.refNbr, Equal<ARPayment.refNbr>,
        //                                  And<ARRegister.docType, Equal<ARPayment.docType>>>,
        //                        InnerJoin<CashAccount, On<CashAccount.cashAccountID, Equal<ARPayment.cashAccountID>>,
        //                        InnerJoin<BAccount, On<ARRegister.customerID, Equal<BAccount.bAccountID>>,
        //                            LeftJoin<CATransfer, On<CATransferExt.usrType, Equal<ARPayment.docType>,
        //                                           And<CATransferExt.usrRefNbr, Equal<ARPayment.refNbr>>>>>>>,
        //                            Where<ARRegisterExt.usrPayReturn, Equal<True>,
        //                            And<Where<ARRegister.docType, Equal<ARDocType.refund>>>>> RecordsVoid;

        public virtual string UsrCustRefundRefNbr { get; set; }
        public abstract class usrCustRefundRefNbr : IBqlField { }
        #endregion

        //---------------------
        #region UsrLoyPoints
        [PXDBInt]
        [PXUIField(DisplayName = "Invoice Points", IsReadOnly = true)]
        public virtual int? UsrLoyPoints { get; set; }
        public abstract class usrLoyPoints : IBqlField { }
        #endregion

        #region UsrPtsBalance
        [PXInt]
        [PXUIField(DisplayName = "Points Bal", IsReadOnly = true)]
        public virtual int? UsrPtsBalance { get; set; }
        public abstract class usrPtsBalance : IBqlField { }
        #endregion

        #region UsrRedPts
        [PXInt]
        [PXUIField(DisplayName = "Redemption Pts")]

        public virtual int? UsrRedPts { get; set; }
        public abstract class usrRedPts : IBqlField { }
        #endregion

        #region UsrRedEqv
        [PXDecimal]
        [PXUIField(DisplayName = "Redemption Amt", IsReadOnly = true)]
        public virtual Decimal? UsrRedEqv { get; set; }
        public abstract class usrRedEqv : IBqlField { }
        #endregion

        #region Usrarword
        [PXDBString(1000)]
        [PXUIField(DisplayName = "arword")]
        public virtual string Usrarword { get; set; }
        public abstract class usrarword : IBqlField { }
        #endregion

        #region Usrarword
        [PXDBString(50)] //UsrArchieveRef
        [PXUIField(DisplayName = "Archieve Ref")]
        public virtual string UsrArchieveRef { get; set; }
        public abstract class usrArchieveRef : IBqlField { }
        #endregion

        #region UsrCustReplicate
        [PXDBString(4000,IsUnicode =true)]
        [PXUIField(DisplayName = "Cust Replicate", IsReadOnly = true)]
        public virtual string UsrCustReplicate { get; set; }
        public abstract class usrCustReplicate : IBqlField { }
        #endregion


        #region UsrCustAmount

        [PXDBDecimal(2)]
        [PXUIField(DisplayName = "Cust Amount")]
        public virtual Decimal? UsrCustAmount { get; set; }

        public abstract class usrCustAmount : IBqlField { }

        #endregion

        #region UsrTaxPortal
        [PXDBBool]
        [PXUIField(DisplayName = "TaxPortal")]

        public virtual bool? UsrTaxPortal { get; set; }
        public abstract class usrTaxPortal : IBqlField { }
        #endregion

        #region UsrTaxRefNbr
        [PXDBString(50)]
        [PXUIField(DisplayName = "Tax RefNbr", Enabled = false, IsReadOnly = true)]
        public virtual string UsrTaxRefNbr { get; set; }
        public abstract class usrTaxRefNbr : IBqlField { }
        #endregion

        #region Sales Order Reference
        [PXDBString(50)]
        [PXUIField(DisplayName = "Sales Order Reference", Enabled = false)]
        public virtual string UsrSalesOrderReference { get; set; }
        public abstract class usrSalesOrderReference : IBqlField { }
        #endregion

        #region Purchase Order Reference
        [PXDBString(50)]
        [PXUIField(DisplayName = "Purchase Order Reference", Enabled = false)]
        public virtual string UsrPurchaseOrderReference { get; set; }
        public abstract class usrPurchaseOrderReference : IBqlField { }
        #endregion

        #region Purchase Order Description
        [PXDBString(200)]
        [PXUIField(DisplayName = "Purchase Order Description", Enabled = false)]
        public virtual string UsrPurchaseOrderDescription { get; set; }
        public abstract class usrPurchaseOrderDescription : IBqlField { }
        #endregion

    }
}