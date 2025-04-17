using PX.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nour2024.TT
{
    [Serializable]
    [PXCacheName("LeadsJobOrders")]
    public class LeadsJobOrders : IBqlTable
    {
        #region ContactID
        [PXDBInt()]
        [PXUIField(DisplayName = "Contact ID")]
        public virtual int? ContactID { get; set; }
        public abstract class contactID : PX.Data.BQL.BqlInt.Field<contactID> { }
        #endregion

        #region Phone1
        [PXDBString(50, IsUnicode = true, InputMask = "")]
        [PXUIField(DisplayName = "Phone1")]
        public virtual string Phone1 { get; set; }
        public abstract class phone1 : PX.Data.BQL.BqlString.Field<phone1> { }
        #endregion

        #region JobOrdrID
        [PXDBString(15, IsUnicode = true, InputMask = "")]
        [PXUIField(DisplayName = "Job Ordr ID")]
        public virtual string JobOrdrID { get; set; }
        public abstract class jobOrdrID : PX.Data.BQL.BqlString.Field<jobOrdrID> { }
        #endregion
    }
}





