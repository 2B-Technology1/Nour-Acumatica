//namespace MyMaintaince
//{
//    using System;
//    using PX.Data;
//    using PX.Objects.AR;
//    using PX.Objects.EP;
//    using PX.Objects.CR;
//    using Maintenance.MM;

//    [System.SerializableAttribute()]
//    public class ReserveFormTime
//    {
//        #region TimeID
//        public abstract class timeID : PX.Data.IBqlField
//        {
//        }
//        [PXDBIdentity()]
//        [PXUIField(DisplayName = "TimeID")]
//        public virtual int? TimeID { get; set; }
//        #endregion

//        #region TimeCD
//        public abstract class timeCD : PX.Data.IBqlField
//        {
//        }
//        [PXDecimal(2)]
//        [PXUIField(DisplayName = "TimeCD")]
//        public virtual Decimal? TimeCD { get; set; }
//        #endregion
//    }
//}
