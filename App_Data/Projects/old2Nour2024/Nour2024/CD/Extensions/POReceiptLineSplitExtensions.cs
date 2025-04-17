using PX.Data;
using System;
using MyMaintaince;

namespace PX.Objects.PO
{
    public class POReceiptLineSplitExt : PXCacheExtension<PX.Objects.PO.POReceiptLineSplit>
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
        [PXUIField(DisplayName = "front electric Motor Number")]

        public virtual string UsrfrontelectricMotorNumber { get; set; }
        public abstract class usrfrontelectricMotorNumber : PX.Data.BQL.BqlString.Field<usrfrontelectricMotorNumber> { }
        #endregion

        #region UsrRearElectricMotorNumber
        [PXDBString(50)]
        [PXUIField(DisplayName = "Rear Electric Motor Number")]

        public virtual string UsrRearElectricMotorNumber { get; set; }
        public abstract class usrRearElectricMotorNumber : PX.Data.BQL.BqlString.Field<usrRearElectricMotorNumber> { }
        #endregion
    }
}