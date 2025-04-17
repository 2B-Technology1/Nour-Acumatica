using PX.Data.EP;
using PX.Data;
using PX.Objects.AP;
using PX.Objects.CA;
using PX.Objects.CM;
using PX.Objects.CS;
using PX.Objects.EP.Standalone;
using PX.Objects.GL;
using PX.Objects.PO;
using PX.Objects.TX;
using PX.Objects;
using PX.SM;
using System.Collections.Generic;
using System;

namespace PX.Objects.AP
{
  public class VendorClassExt : PXCacheExtension<PX.Objects.AP.VendorClass>
  {
      #region UsrVendorClass2
      [PXDBString(10, IsKey=false, BqlTable=typeof(PX.Objects.AP.VendorClass), IsFixed=false, IsUnicode=true)]
      [PXUIField(DisplayName="Vendor Class 2")]
      [PXSelector(typeof(CAAdj.tranDesc))]
/*[PXSelector(typeof(CeloPack.VendorClass2.vendorClassID),
            new Type[]
            {
                typeof(CeloPack.VendorClass2.vendorClassID),
                typeof(CeloPack.VendorClass2.descr)
            },
            SubstituteKey = typeof(CeloPack.VendorClass2.descr))]*/
      public virtual string UsrVendorClass2 { get; set; }
      public abstract class usrVendorClass2 : IBqlField { }
      #endregion
  }
}