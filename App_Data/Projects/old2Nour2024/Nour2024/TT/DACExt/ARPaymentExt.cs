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
using PX.Objects;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System;
using Maintenance;

namespace PX.Objects.AR
{
  public class ARPaymentExt : PXCacheExtension<PX.Objects.AR.ARRegister>, IBqlTable
  {
      #region UsrRefNbr
      [PXDBString(10, IsKey=false, BqlTable=typeof(PX.Objects.AR.ARRegister), IsFixed=false, IsUnicode=true,InputMask = ">CCCCCCCCCCCCCCC")]
      [PXUIField(DisplayName="Custom RefNbr")]
      public virtual string UsrRefNbr { get; set; }
      public abstract class usrRefNbr : IBqlField { }
      #endregion

      #region UsrBranchName
      [PXDBString(15, IsKey=false, BqlTable=typeof(PX.Objects.AR.ARRegister), IsFixed=false, IsUnicode=true)]
      [PXUIField(DisplayName="Branch")]
      [PXSelector(typeof(BRBank.bRID),
           new Type[]
            {
                typeof(BRBank.bRID),
                typeof(BRBank.bRName)
            },
           SubstituteKey = typeof(BRBank.bRID),
           DescriptionField = typeof(BRBank.bRName))]
      public virtual string UsrBranchName { get; set; }
      public abstract class usrBranchName : IBqlField { }
      #endregion

      #region UsrDueDate
      [PXDBDate(IsKey=false, BqlTable=typeof(PX.Objects.AR.ARRegister))]
        [PXDefault(typeof(AccessInfo.businessDate))]
        [PXUIField(DisplayName="Due Date4")]
      public virtual DateTime? UsrDueDate { get; set; }
      public abstract class usrDueDate : IBqlField { }
      #endregion

      #region UsrBankRefNbr
      [PXDBString(20, IsKey=false, IsFixed=false, IsUnicode=true)]
      [PXUIField(DisplayName="Bank Ref. No",Visible = true)]
      public virtual string UsrBankRefNbr { get; set; }
      public abstract class usrBankRefNbr : IBqlField { }
      #endregion

      #region UsrBankName
      [PXDBString(50, IsKey=false, BqlTable=typeof(PX.Objects.AR.ARRegister), IsFixed=false, IsUnicode=true)]
      [PXUIField(DisplayName="Bank Name")]
      [PXSelector(typeof(Banks.bankID),
           new Type[]
            {
                typeof(Banks.bankID),
                typeof(Banks.bankName)
            },
           SubstituteKey = typeof(Banks.bankID),
           DescriptionField = typeof(Banks.bankName))]
      public virtual string UsrBankName { get; set; }
      public abstract class usrBankName : IBqlField { }
      #endregion

      #region UsrNRAccount
      [PXDBString(10, IsKey=false, BqlTable=typeof(PX.Objects.AR.ARRegister), IsFixed=false, IsUnicode=true)]
      [PXUIField(DisplayName="NR Account", Enabled = false)]
      public virtual string UsrNRAccount { get; set; }
      public abstract class usrNRAccount : IBqlField { }
      #endregion

      #region UsrCUCAccount
      [PXDBString(10, IsKey=false, BqlTable=typeof(PX.Objects.AR.ARRegister), IsFixed=false, IsUnicode=true)]
      [PXUIField(DisplayName="CUC Account",Enabled =false)]
      public virtual string UsrCUCAccount { get; set; }
      public abstract class usrCUCAccount : IBqlField { }
      #endregion

      #region UsrBankAccount
      [PXDBString(15, IsKey=false, BqlTable=typeof(PX.Objects.AR.ARRegister), IsFixed=false, IsUnicode=true)]
      [PXUIField(DisplayName="Bank Account",Enabled =false)]
      public virtual string UsrBankAccount { get; set; }
      public abstract class usrBankAccount : IBqlField { }
      #endregion

      #region UsrNRRefNbr
      [PXDBString(20, IsKey=false, BqlTable=typeof(PX.Objects.AR.ARRegister), IsFixed=false, IsUnicode=true)]
      [PXUIField(DisplayName="NR Ref No.")]
      public virtual string UsrNRRefNbr { get; set; }
      public abstract class usrNRRefNbr : IBqlField { }
      #endregion

      #region UsrDepositRefNbr
      [PXDBString(30, IsKey=false, BqlTable=typeof(PX.Objects.AR.ARRegister), IsFixed=false, IsUnicode=true)]
      [PXUIField(DisplayName="CUC Ref No.")]
      public virtual string UsrDepositRefNbr { get; set; }
      public abstract class usrDepositRefNbr : IBqlField { }
      #endregion

      #region UsrBankDepNbr
      [PXDBString(20, IsKey=false, BqlTable=typeof(PX.Objects.AR.ARRegister), IsFixed=false, IsUnicode=true)]
      [PXUIField(DisplayName="Bank Deposit Ref")]
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
        [PXDBBool(IsKey=false, BqlTable=typeof(PX.Objects.AR.ARRegister))]
        [PXUIField(DisplayName="NR", Visible = false)]
        [PXDefault(false)]
        public virtual bool? UsrNR { get; set; }
        public abstract class usrNR : IBqlField { }
        #endregion

        #region UsrCUC
        [PXDBBool(IsKey=false, BqlTable=typeof(PX.Objects.AR.ARRegister))]
        [PXUIField(DisplayName="CUC",Visible =  false)]
        [PXDefault(false)]
        public virtual bool? UsrCUC { get; set; }
        public abstract class usrCUC : IBqlField { }
        #endregion

        #region UsrBankDeposited
        [PXDBBool(IsKey=false, BqlTable=typeof(PX.Objects.AR.ARRegister))]
        [PXUIField(DisplayName="Bank Deposited",Visible =  false)]
        public virtual bool? UsrBankDeposited { get; set; }
        public abstract class usrBankDeposited : IBqlField { }
        #endregion

      #region UsrRefunded
      [PXDBBool(IsKey=false, BqlTable=typeof(PX.Objects.AR.ARRegister))]
      [PXUIField(DisplayName="Refunded",Visible=false)]
      [PXDefault(false)]
      public virtual bool? UsrRefunded { get; set; }
      public abstract class usrRefunded : IBqlField { }
      #endregion
  }
}