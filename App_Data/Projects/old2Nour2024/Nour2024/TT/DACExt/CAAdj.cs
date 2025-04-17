using IRegister = PX.Objects.CM.IRegister;
using PX.Data.EP;
using PX.Data;
using PX.Objects.AP;
using PX.Objects.CA;
using PX.Objects.CM;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Objects.EP;
using PX.Objects.GL;
using PX.Objects.TX;
using PX.Objects;
using PX.TM;
using System.Collections.Generic;
using System;

using Maintenance;
namespace PX.Objects.CA
{
  public class CAAdjExt : PXCacheExtension<CAAdj>
  {
      #region UsrDueDate
      [PXDBDate(IsKey=false, BqlTable=typeof(PX.Objects.CA.CAAdj))]
        [PXDefault(typeof(AccessInfo.businessDate))]
        [PXUIField(DisplayName="Due Date")]
      public virtual DateTime? UsrDueDate { get; set; }
      public abstract class usrDueDate : IBqlField { }
      #endregion

      #region UsrBankName
      [PXDBString(15, IsKey=false, BqlTable=typeof(PX.Objects.CA.CAAdj), IsFixed=false, IsUnicode=true)]
      [PXUIField(DisplayName="Bank name")]

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

      #region UsrCheckNbr
      [PXDBString(50, IsKey=false, BqlTable=typeof(CAAdj), IsFixed=false, IsUnicode=true)]
      [PXUIField(DisplayName="Check No.",Visibility=PXUIVisibility.SelectorVisible)]
      [PXSelector(typeof(
         Search2<CheckNo.checkNumber,
     LeftJoin<APRegister, On<CheckNo.checkNumber, Equal<APRegisterExt.usrCheckNbr>>,
     LeftJoin<CAAdj, On<CAAdjExt.usrCheckNbr, Equal<CheckNo.checkNumber>>>>,

     Where<CheckNo.cashAccountID, Equal<Current<CAAdj.cashAccountID>>,
     And2<Where<APRegister.released, IsNull, Or<APRegister.released, Equal<False>>>,
     And2<Where<CAAdj.released, IsNull, Or<CAAdj.released, Equal<False>>>,
     And2<Where<CAAdj.adjRefNbr, IsNull>,
     And<Where<APRegister.refNbr,IsNull>>>>>>>
          
          //Search2<CheckNo.checkNumber,
          //           LeftJoin<APRegister, On<CheckNo.checkNumber, Equal<APRegisterExt.usrCheckNbr>>,
          //           LeftJoin<CAAdj, On<CAAdjExt.usrCheckNbr, Equal<CheckNo.checkNumber>>>>,
                     
          //           Where<CheckNo.cashAccountID, Equal<Current<CAAdj.cashAccountID>>, 

          //               And<CAAdj.adjRefNbr, Equal<Null>, And<APRegister.refNbr, Equal<Null>,

          //           And<Where<APRegister.released, IsNull, Or<APRegister.released, Equal<False>>>
          //           /*,And<Where<CAAdj.released, IsNull, Or<CAAdj.released, Equal<False>>>>*/>>>>,
          //           OrderBy<Asc<StrLen<CheckNo.checkNumber>/*,Asc<CeloPack.CheckNo.checkNumber>*/>>>
                     ),
                        new Type[]
            {
                  typeof(CheckNo.checkNumber),
                        typeof(APRegister.refNbr),
                        typeof(CAAdj.adjRefNbr)

            },
                        SubstituteKey = typeof(CheckNo.checkNumber))]



     //Search2<CheckNo.checkNumber,
     //LeftJoin<APRegister,On<CheckNo.checkNumber,Equal<APRegisterExt.usrCheckNbr>>,
     //LeftJoin<CAAdj,On<CAAdjExt.usrCheckNbr,Equal<CheckNo.checkNumber>>>>,
     
     //Where<CheckNo.cashAccountID,Equal<Current<CAAdj.cashAccountID>>,
     //And2<Where<APRegister.released,IsNull,Or<APRegister.released,Equal<False>>>,
     //And<Where<CAAdj.released,IsNull,Or<CAAdj.released,Equal<False>>>>>>>





      public virtual string UsrCheckNbr { get; set; }
      public abstract class usrCheckNbr : IBqlField { }
      #endregion

      #region UsrNRCheckNo
      [PXDBString(30, IsKey=false, BqlTable=typeof(PX.Objects.CA.CAAdj), IsFixed=false, IsUnicode=true)]
      [PXUIField(DisplayName="NR Check No.")]
      public virtual string UsrNRCheckNo { get; set; }
      public abstract class usrNRCheckNo : IBqlField { }
      #endregion

      #region UsrBranchName
      [PXDBString(15, IsKey=false, BqlTable=typeof(PX.Objects.CA.CAAdj), IsFixed=false, IsUnicode=true)]
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

      #region UsrGLBalance
      [PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
      [PXCury(typeof(CAAdj.curyID))]
      [PXUIField(DisplayName = "GL Balance", Enabled = false)]
      [PX.Objects.CA.GLBalance(typeof(CAAdj.cashAccountID), null, typeof(CAAdj.tranDate))]
      public virtual Decimal? UsrGLBalance { get; set; }
      public abstract class usrGLBalance : IBqlField { }
      #endregion

      #region UsrCashBalance
      [PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
      [PXCury(typeof(CAAdj.curyID))]
      [PXUIField(DisplayName = "Available Balance", Enabled = false)]
      [PX.Objects.CA.CashBalance(typeof(CAAdj.cashAccountID))]
      public virtual Decimal? UsrCashBalance { get; set; }
      public abstract class usrCashBalance : IBqlField { }
      #endregion

      #region UsrCUC
      [PXDBBool(IsKey=false, BqlTable=typeof(PX.Objects.CA.CAAdj))]
      [PXUIField(DisplayName="CUC",Visible = false)]
      [PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual bool? UsrCUC { get; set; }
      public abstract class usrCUC : IBqlField { }
      #endregion

      #region UsrBankDeposited
      [PXDBBool(IsKey=false, BqlTable=typeof(PX.Objects.CA.CAAdj))]
      [PXUIField(DisplayName="Bank Deposited",Visible = false)]
      [PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual bool? UsrBankDeposited { get; set; }
      public abstract class usrBankDeposited : IBqlField { }
      #endregion

      #region UsrCheck
      [PXDBBool(IsKey=false, BqlTable=typeof(PX.Objects.CA.CAAdj))]
      [PXUIField(DisplayName="Check Dispersant",Visible = false)]
      [PXDefault( false, PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual bool? UsrCheck { get; set; }
      public abstract class usrCheck : IBqlField { }
      #endregion

      #region UsrCUCAccount
      [PXDBInt(IsKey=false, BqlTable=typeof(PX.Objects.CA.CAAdj))]
      [PXUIField(DisplayName="CUC Account",Visible = false)]
      public virtual int? UsrCUCAccount { get; set; }
      public abstract class usrCUCAccount : IBqlField { }
      #endregion

      #region UsrBankAccount
      [PXDBInt(IsKey=false, BqlTable=typeof(PX.Objects.CA.CAAdj))]
      [PXUIField(DisplayName="Bank Account",Visible = false)]
      public virtual int? UsrBankAccount { get; set; }
      public abstract class usrBankAccount : IBqlField { }
      #endregion

      #region UsrCheckDispersant
      [PXDBInt(IsKey=false, BqlTable=typeof(PX.Objects.CA.CAAdj))]
      [PXUIField(DisplayName="Check Dispersant",Visible = false)]
        public virtual int? UsrCheckDispersant { get; set; }
      public abstract class usrCheckDispersant : IBqlField { }
      #endregion

      #region UsrTransferRefNbr
      [PXDBString(20, IsKey=false, BqlTable=typeof(PX.Objects.CA.CAAdj), IsFixed=false, IsUnicode=true)]
      [PXUIField(DisplayName="CUC Transfer No.",Visible = false)]
      public virtual string UsrTransferRefNbr { get; set; }
      public abstract class usrTransferRefNbr : IBqlField { }
      #endregion

      #region UsrRefNbr
      [PXDBString(10, IsKey=false, BqlTable=typeof(PX.Objects.CA.CAAdj), IsFixed=false, IsUnicode=true)]
      [PXUIField(DisplayName="Custom RefNbr")]
      public virtual string UsrRefNbr { get; set; }
      public abstract class usrRefNbr : IBqlField { }
      #endregion

      #region UsrReversed
      [PXDBBool(IsKey = false)]
      [PXUIField(DisplayName = "Reversed", Visible = false)]
      [PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]//added persisting check:nothig 
      public virtual bool? UsrReversed { get; set; }
      public abstract class usrReversed : IBqlField { }
      #endregion
  }
}