using MyMaintaince;
using PX.Data;
using System;

namespace PX.Objects.SO
{
    
    public class SOShipLineSplitExt : PXCacheExtension<PX.Objects.SO.SOShipLineSplit>
    {
        #region UsrMotorNo
        [PXDBString(50)]
        [PXUIField(DisplayName = "MotorNo")]

        public virtual string UsrMotorNo { get; set; }
        public abstract class usrMotorNo : IBqlField { }
        #endregion

        #region UsrColor

        [PXDBInt]
        [PXUIField(DisplayName = "Color")]
        [PXSelector(typeof(Colors.colorID),
                       new Type[] { typeof(Colors.colorCD), typeof(Colors.descr) }
                       , DescriptionField = typeof(Colors.descr)
                       , SubstituteKey = typeof(Colors.colorCD))]
        public virtual int? UsrColor { get; set; }
        public abstract class usrColor : IBqlField { }
        #endregion

        #region UsrModelYear
        [PXDBString(4)]
        [PXUIField(DisplayName = "ModelYear")]

        public virtual string UsrModelYear { get; set; }
        public abstract class usrModelYear : IBqlField { }
        #endregion

        #region UsrfrontelectricMotorNumber
        [PXDBString(50)]
        [PXUIField(DisplayName = "front electric Motor Number", Enabled = false)]

        public virtual string UsrfrontelectricMotorNumber { get; set; }
        public abstract class usrfrontelectricMotorNumber : PX.Data.BQL.BqlString.Field<usrfrontelectricMotorNumber> { }
        #endregion

        #region UsrRearElectricMotorNumber
        [PXDBString(50)]
        [PXUIField(DisplayName = "Rear Electric Motor Number", Enabled = false)]

        public virtual string UsrRearElectricMotorNumber { get; set; }
        public abstract class usrRearElectricMotorNumber : PX.Data.BQL.BqlString.Field<usrRearElectricMotorNumber> { }
        #endregion
    }
}