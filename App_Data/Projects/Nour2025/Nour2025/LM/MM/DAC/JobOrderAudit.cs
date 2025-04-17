using System;
using PX.Data;

namespace Maintenance 
{
    [Serializable]
    public class JobOrderAudit : IBqlTable
    {


        #region AuditID

        [PXDBInt(IsKey = true)]
        [PXUIField(DisplayName = "Audit ID")]
        public int? AuditID { get; set; }

        public class auditID : IBqlField { }

        #endregion


        #region JobOrderID

        [PXDBString(50, IsUnicode = true, InputMask = "")]
        [PXUIField(DisplayName = "Job Order ID")]
        public string JobOrderID { get; set; }

        public class jobOrderID : IBqlField { }

        #endregion


        #region AuditDateTime

        [PXDBDateAndTime()]
        [PXUIField(DisplayName = "Audit Date Time")]
        public DateTime? AuditDateTime { get; set; }

        public class auditDateTime : IBqlField { }

        #endregion


        #region StatusNew

        [PXDBString(50, IsUnicode = true, InputMask = "")]
        [PXUIField(DisplayName = "Status New")]
        public string StatusNew { get; set; }

        public class statusNew : IBqlField { }

        #endregion


        #region StatusOld

        [PXDBString(50, IsUnicode = true, InputMask = "")]
        [PXUIField(DisplayName = "Status Old")]
        public string StatusOld { get; set; }

        public class statusOld : IBqlField { }

        #endregion


        #region UserName

        [PXDBString(50, IsUnicode = true, InputMask = "")]
        [PXUIField(DisplayName = "User Name")]
        public string UserName { get; set; }

        public class userName : IBqlField { }

        #endregion


        #region Description

        [PXDBString(350, IsUnicode = true, InputMask = "")]
        [PXUIField(DisplayName = "Description")]
        public string Description { get; set; }

        public class description : IBqlField { }

        #endregion

    }
}