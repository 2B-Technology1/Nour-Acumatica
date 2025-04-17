using CRLocation = PX.Objects.CR.Standalone.Location;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Data;
using PX.Objects.AP;
using PX.Objects.CM;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Objects.GL;
using PX.Objects.IN;
using PX.Objects.PO;
using PX.Objects;
using System.Collections.Generic;
using System;

namespace PX.Objects.PO
{
  public class POReceiptExt : PXCacheExtension<PX.Objects.PO.POReceipt>
  {
      #region UsrRefNbr
      [PXDBString(30, IsKey=false, BqlTable=typeof(PX.Objects.PO.POReceipt), IsFixed=false, IsUnicode=true)]
      [PXUIField(DisplayName="Custom Ref No.")]
      public virtual string UsrRefNbr { get; set; }
      public abstract class usrRefNbr : IBqlField { }
      #endregion
  }
}