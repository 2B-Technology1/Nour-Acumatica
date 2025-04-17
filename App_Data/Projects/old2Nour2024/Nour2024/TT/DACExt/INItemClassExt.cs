using System;
using PX.Data;
using PX.Objects.TX;
using PX.Objects.CS;
using PX.Objects.DR;
using PX.TM;
using System.Collections.Generic;
using PX.Objects;
using PX.Objects.IN;


namespace PX.Objects.IN
{
    public class INItemClassExt : PXCacheExtension<PX.Objects.IN.INItemClass>
    {



        #region UsrReasonCode



        [PXDBString(10, IsKey = false, BqlTable = typeof(PX.Objects.IN.INItemClass), IsFixed = false, IsUnicode = true)]
        [PXUIField(DisplayName = "ReasonCode")]
        [PXSelector(typeof(Search<ReasonCode.reasonCodeID>))]

        public virtual string UsrReasonCode { get; set; }
        public abstract class usrReasonCode : IBqlField { }

        #endregion




    }




}