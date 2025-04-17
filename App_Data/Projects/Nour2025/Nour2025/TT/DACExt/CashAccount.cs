using PX.Data.EP;
using PX.Data;
using PX.Objects.AP;
using PX.Objects.CA;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Objects.GL;
using PX.Objects;
using System.Collections.Generic;
using System;

namespace PX.Objects.CA
{
  public class CashAccountExt : PXCacheExtension<PX.Objects.CA.CashAccount>
  {
      #region UsrCUC
      [PXDBBool(IsKey=false, BqlTable=typeof(PX.Objects.CA.CashAccount))]
      [PXUIField(DisplayName="CUC Account")]
      public virtual bool? UsrCUC { get; set; }
      public abstract class usrCUC : IBqlField { }
      #endregion

      #region UsrNR
      [PXDBBool(IsKey=false, BqlTable=typeof(PX.Objects.CA.CashAccount))]
      [PXUIField(DisplayName="NR Account")]
      public virtual bool? UsrNR { get; set; }
      public abstract class usrNR : IBqlField { }
      #endregion

      #region UsrBank
      [PXDBBool(IsKey=false, BqlTable=typeof(PX.Objects.CA.CashAccount))]
      [PXUIField(DisplayName="Bank Account")]
      public virtual bool? UsrBank { get; set; }
      public abstract class usrBank : IBqlField { }
      #endregion

      #region UsrCheckDispersant
      [PXDBBool(IsKey=false, BqlTable=typeof(PX.Objects.CA.CashAccount))]
      [PXUIField(DisplayName="Check Dispersant")]
      public virtual bool? UsrCheckDispersant { get; set; }
      public abstract class usrCheckDispersant : IBqlField { }
      #endregion
  }
}