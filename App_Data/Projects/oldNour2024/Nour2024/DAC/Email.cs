using PX.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nour2024.DAC
{ 
    [Serializable]
[PXCacheName("Email")]
public class Email : IBqlTable
{
        #region To
        [PXString(255)]
        [PXUIField(DisplayName = "TO")]
        public virtual string TO { get; set; }
        public abstract class to : PX.Data.BQL.BqlString.Field<to> { }
        #endregion

        #region 
        [PXString(500)]
        [PXUIField(DisplayName = "CC")]
        public virtual string CC { get; set; }
        public abstract class cc : PX.Data.BQL.BqlString.Field<cc> { }
        #endregion

    }
}
