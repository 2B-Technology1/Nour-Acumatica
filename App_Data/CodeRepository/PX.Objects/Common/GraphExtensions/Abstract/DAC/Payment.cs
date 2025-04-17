/* ---------------------------------------------------------------------*
*                             Acumatica Inc.                            *

*              Copyright (c) 2005-2023 All rights reserved.             *

*                                                                       *

*                                                                       *

* This file and its contents are protected by United States and         *

* International copyright laws.  Unauthorized reproduction and/or       *

* distribution of all or any portion of the code contained herein       *

* is strictly prohibited and will result in severe civil and criminal   *

* penalties.  Any violations of this copyright will be prosecuted       *

* to the fullest extent possible under law.                             *

*                                                                       *

* UNDER NO CIRCUMSTANCES MAY THE SOURCE CODE BE USED IN WHOLE OR IN     *

* PART, AS THE BASIS FOR CREATING A PRODUCT THAT PROVIDES THE SAME, OR  *

* SUBSTANTIALLY THE SAME, FUNCTIONALITY AS ANY ACUMATICA PRODUCT.       *

*                                                                       *

* THIS COPYRIGHT NOTICE MAY NOT BE REMOVED FROM THIS FILE.              *

* --------------------------------------------------------------------- */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Data;

namespace PX.Objects.Common.GraphExtensions.Abstract.DAC
{
    public class Payment: PXMappedCacheExtension
    {
        #region BranchID
        public abstract class branchID : PX.Data.IBqlField
        {
        }

        public virtual Int32? BranchID { get; set; }
        #endregion
        #region AdjDate
        public abstract class adjDate : PX.Data.IBqlField
        {
        }

        public virtual DateTime? AdjDate { get; set; }
        #endregion
        #region AdjFinPeriodID
        public abstract class adjFinPeriodID : PX.Data.IBqlField
        {
        }

        public virtual String AdjFinPeriodID { get; set; }
        #endregion
        #region AdjTranPeriodID
        public abstract class adjTranPeriodID : PX.Data.IBqlField
        {
        }

        public virtual String AdjTranPeriodID { get; set; }
        #endregion
        #region CuryID
        public abstract class curyID : PX.Data.IBqlField
        {
        }

        public virtual String CuryID { get; set; }
        #endregion
    }
}
