using PX.Data;
using PX.Objects.AR;
using PX.Objects.CA;
using PX.Objects.CM;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Objects.GL;
using PX.Objects.PM;
using PX.Objects.TX;
using PX.Objects;
using System.Collections.Generic;
using System;
using PX.Objects.PO;
using PX.Objects.SO;
namespace PX.Objects.GL
{
  public class GLTranDocExt : PXCacheExtension<PX.Objects.GL.GLTranDoc>
  {
      #region UsrDueDate
      [PXDBDate]
      [PXDefault(typeof(AccessInfo.businessDate))]
      [PXUIField(DisplayName="S Due Date")]

      public virtual DateTime? UsrDueDate { get; set; }
      public abstract class usrDueDate : IBqlField { }
      #endregion

      #region UsrCheckNbr
            [PXDBString(50)]
      [PXUIField(DisplayName="CheckNbr")]

      public virtual string UsrCheckNbr { get; set; }
      public abstract class usrCheckNbr : IBqlField { }
      #endregion

      #region UsrPONbr
            [PXDBString(50)]
      [PXUIField(DisplayName="PO Nbr")]

[PXSelector(typeof(Search<POOrder.orderNbr,Where<POOrder.vendorID,Equal<Current<GLTranDoc.bAccountID>>>>)
            ,new Type[]{
                  typeof(POOrder.orderType),
                  typeof(POOrder.orderNbr),
                  typeof(POOrder.vendorID),
            }
            )]

      public virtual string UsrPONbr { get; set; }
      public abstract class usrPONbr : IBqlField { }
      #endregion

      #region UsrSONbr
            [PXDBString(50)]
      [PXUIField(DisplayName="SO Nbr")]

[PXSelector(typeof(Search<SOOrder.orderNbr,Where<SOOrder.customerID,Equal<Current<GLTranDoc.bAccountID>>>>)
            ,new Type[]{
                  typeof(SOOrder.orderType),
                  typeof(SOOrder.orderNbr),
                  typeof(SOOrder.customerID),
            }
            )]

      public virtual string UsrSONbr { get; set; }
      public abstract class usrSONbr : IBqlField { }
      #endregion
  }
}