using PX.Data.ReferentialIntegrity.Attributes;
using PX.Data;
using PX.Objects.CM;
using PX.Objects.CS;
using PX.Objects.CT;
using PX.Objects.GL;
using PX.Objects.IN;
using PX.Objects.PM;
using PX.Objects.PO;
using PX.Objects.SO;
using PX.Objects;
using System.Collections.Generic;
using System;

namespace PX.Objects.IN
{
  public class INTranExt : PXCacheExtension<PX.Objects.IN.INTran>
  {
      #region UsrOrdID
      [PXDBString(50, IsKey=false, IsFixed=false, IsUnicode=true)]
      [PXUIField(DisplayName="Job Order ID")]
      [PXDBDefault(typeof(INRegisterExt.usrOrderID),PersistingCheck=PXPersistingCheck.Nothing)] //khalifa
     
      public virtual string UsrOrdID { get; set; }
      public abstract class usrOrdID : IBqlField { }
      #endregion

      
    
  }
}