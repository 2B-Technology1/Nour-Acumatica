using PX.Data.EP;
using PX.Data;
using PX.Objects.AR;
using PX.Objects.CA;
using PX.Objects.CM;
using PX.Objects.CS;
using PX.Objects.GL;
using PX.Objects;
using System.Collections.Generic;
using System;

namespace PX.Objects.CA
{
  public class CATransferExt : PXCacheExtension<PX.Objects.CA.CATransfer>
  {
      #region UsrType
      [PXDBString(30, IsKey=false, BqlTable=typeof(PX.Objects.CA.CATransfer), IsFixed=false, IsUnicode=true)]
      [PXUIField(DisplayName="Type")]
      public virtual string UsrType { get; set; }
      public abstract class usrType : IBqlField { }
      #endregion

      #region UsrRefNbr
      [PXDBString(20, IsKey=false, BqlTable=typeof(PX.Objects.CA.CATransfer), IsFixed=false, IsUnicode=true)]
      [PXUIField(DisplayName="Transaction Ref")]
      public virtual string UsrRefNbr { get; set; }
      public abstract class usrRefNbr : IBqlField { }
      #endregion
  }
}