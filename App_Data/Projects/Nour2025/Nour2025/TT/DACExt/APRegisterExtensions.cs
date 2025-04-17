using APQuickCheck = PX.Objects.AP.Standalone.APQuickCheck;
using CRLocation = PX.Objects.CR.Standalone.Location;
using IRegister = PX.Objects.CM.IRegister;
using PX.Common;
using PX.Data.EP;
using PX.Data;
using PX.Objects.AP.BQL;
using PX.Objects.AP;
using PX.Objects.CM;
using PX.Objects.Common;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Objects.GL;
using PX.Objects;
using PX.TM;
using System.Collections.Generic;
using System.Linq;
using System;
using PX.Objects.CA;
using PX.Objects.PO;
using Maintenance;
namespace PX.Objects.AP
{
  public class APRegisterExt : PXCacheExtension<APRegister>
  {
        #region UsrDueDate
        [PXDBDate]
        [PXUIField(DisplayName = "Due Date")]
        [PXDefault(typeof(AccessInfo.businessDate),PersistingCheck=PXPersistingCheck.Nothing)]
        public virtual DateTime? UsrDueDate { get; set; }
        public abstract class usrDueDate : IBqlField { }
        #endregion

        #region UsrCheckNbr
        [PXDBString(50, IsKey=false, BqlTable=typeof(APRegister), IsFixed=false, IsUnicode=true)]
        [PXUIField(DisplayName="Check No")]
        [PXSelector(typeof(Search2<CheckNo.checkNumber,
                     LeftJoin<APRegister, On<CheckNo.checkNumber, Equal<usrCheckNbr>>,
                     LeftJoin<CAAdj, On<CAAdjExt.usrCheckNbr, Equal<CheckNo.checkNumber>>>>,
                     Where<CheckNo.cashAccountID, Equal<Current<APPayment.cashAccountID>>,
                     And2<Where<APRegister.released, IsNull, Or<APRegister.released, Equal<False>>>,
                     And2<Where<CAAdj.released, IsNull, Or<CAAdj.released, Equal<False>>>,
                     Or<APRegister.released, Equal<True>, And<APRegister.refNbr, Equal<Current<APRegister.refNbr>>>>>>>>),
                        new Type[]
            {
                  typeof(CheckNo.checkNumber),
                        typeof(APRegister.refNbr),
                        typeof(CAAdj.adjRefNbr)
            
            },
                        SubstituteKey = typeof(CheckNo.checkNumber))]
        
      public virtual string UsrCheckNbr { get; set; }
      public abstract class usrCheckNbr : IBqlField { }
      #endregion

        #region UsrRefNbr
            [PXDBString(10, IsKey=false, BqlTable=typeof(APRegister), IsFixed=false, IsUnicode=true)]
      [PXUIField(DisplayName="Custom RefNbr",Enabled=false)]

      public virtual string UsrRefNbr { get; set; }
      public abstract class usrRefNbr : IBqlField { }
        #endregion

        #region UsrRefunded
        [PXDBBool(IsKey=false, BqlTable=typeof(APRegister))]
        [PXDefault(false)]
        [PXUIField(DisplayName="Refunded",Visible = false)]

        public virtual bool? UsrRefunded { get; set; }
        public abstract class usrRefunded : IBqlField { }
        #endregion

        #region UsrCheckDispersant
        [PXDBString(50, IsKey=false, BqlTable=typeof(APRegister), IsFixed=false, IsUnicode=true)]
        [PXUIField(DisplayName="Check Dispersant")]

      public virtual string UsrCheckDispersant { get; set; }
      public abstract class usrCheckDispersant : IBqlField { }
        #endregion

        #region UsrChecked
        [PXDBBool(IsKey=false, BqlTable=typeof(APRegister))]
        [PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName="Checked",Visible = false)]
        public virtual bool? UsrChecked { get; set; }
        public abstract class usrChecked : IBqlField { }
        #endregion

        #region UsrBillNbr
            [PXDBString(15, IsKey=false, BqlTable=typeof(APRegister), IsFixed=false, IsUnicode=true)]
      [PXUIField(DisplayName="Bill Ref. No.")]
      [APInvoiceType.RefNbr(typeof(Search<APInvoice.refNbr,
      Where<APInvoice.docType, Equal<APInvoiceType.invoice>,
      And<APInvoice.vendorID,Equal<Current<APInvoice.vendorID>>>>>), Filterable = true)]

      public virtual string UsrBillNbr { get; set; }
      public abstract class usrBillNbr : IBqlField { }
        #endregion

        #region UsrCO
        [PXDBBool]
        [PXUIField(DisplayName = "CO")]

        public virtual bool? UsrCO { get; set; }
        public abstract class usrCO : IBqlField { }
        #endregion

        #region UsrPONbr
        [PXDBString(50)]
        [PXUIField(DisplayName = "PO Nbr")]
        [PXSelector(typeof(Search<POOrder.orderNbr, Where<POOrder.vendorID, Equal<Current<APRegister.vendorID>>>>)
            , new Type[]
            {
                  typeof(POOrder.orderType), typeof(POOrder.orderNbr), typeof(POOrder.vendorID),
            })]
        public virtual string UsrPONbr { get; set; }
        public abstract class usrPONbr : IBqlField { }
        #endregion

  }
}