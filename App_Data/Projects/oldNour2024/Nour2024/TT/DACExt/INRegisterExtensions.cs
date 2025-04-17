using PX.Data.ReferentialIntegrity.Attributes;
using PX.Data;
using PX.Objects.CM;
using PX.Objects.CS;
using PX.Objects.GL;
using PX.Objects.IN;
using PX.Objects;
using System.Collections.Generic;
using System;
using MyMaintaince;
namespace PX.Objects.IN
{
  public class INRegisterExt : PXCacheExtension<PX.Objects.IN.INRegister>
  {
      #region UsrOrderID
      [PXDefault("",PersistingCheck=PXPersistingCheck.Nothing)]
      [PXDBString(50,IsFixed=false, IsUnicode=true,IsKey=false)]
      [PXUIField(DisplayName="Job Order ID")]

[PXSelector(typeof(Search<JobOrder.jobOrdrID, Where<JobOrder.Status, Equal<JobOrderStatus.started>>>)
                               , new Type[] { typeof(JobOrder.jobOrdrID), typeof(JobOrder.customer) })]
      public virtual string UsrOrderID { get; set; }
      public abstract class usrOrderID : IBqlField { }
      #endregion

      #region UsrWarehouseSerial
      [PXDefault(0, PersistingCheck = PXPersistingCheck.Nothing)] //khalifa
      [PXDBInt]
      [PXUIField(DisplayName="Warehouse Serial",Enabled=false)]

      public virtual int? UsrWarehouseSerial { get; set; }
      public abstract class usrWarehouseSerial : IBqlField { }
      #endregion

      #region UsrRtrn
      [PXDBBool]
      [PXUIField(DisplayName="Return")]
      [PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)] //khalifa
      public virtual bool? UsrRtrn { get; set; }
      public abstract class usrRtrn : IBqlField { }
      #endregion
      #region Virtual Fields Unbounded DAC Fields


      #region Customer
      public abstract class customer : PX.Data.IBqlField
      {
      }
      [PXString(300, IsUnicode = true)]
      [PXUIField(DisplayName = "Customer",IsReadOnly=true)]
      public virtual string Customer { get; set; }
      #endregion

      #region Code
      public abstract class code : PX.Data.IBqlField
      {
      }
      [PXString(100, IsUnicode = true)]
      [PXUIField(DisplayName = "Chassis No",IsReadOnly=true)]
     
      public virtual string Code { get; set; }
      #endregion

     

      #endregion
  }

      [PXNonInstantiatedExtension]
  public class IN_INRegister_ExistingColumn : PXCacheExtension<PX.Objects.IN.INRegister>
  {
      #region TransferType  
            [PXDBString(1, IsFixed = true)]
      [PXDefault(INTransferType.TwoStep)]
      [INTransferType.List()]
      [PXUIField(DisplayName = "Transfer Type")]

      public string TransferType { get; set; }
      #endregion
  }

}