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
    public class Adjust2: PXMappedCacheExtension
    {
        #region AdjgBranchID
        public abstract class adjgBranchID : PX.Data.IBqlField { }

        public virtual int? AdjgBranchID { get; set; }
        #endregion
        #region AdjgFinPeriodID
        public abstract class adjgFinPeriodID : PX.Data.IBqlField{}

        public virtual String AdjgFinPeriodID { get; set; }
        #endregion
        #region AdjgTranPeriodID
        public abstract class adjgTranPeriodID : PX.Data.IBqlField { }

        public virtual String AdjgTranPeriodID { get; set; }
        #endregion
        #region AdjdBranchID
        public abstract class adjdBranchID : PX.Data.IBqlField { }

        public virtual int? AdjdBranchID { get; set; }
        #endregion
        #region AdjdFinPeriodID
        public abstract class adjdFinPeriodID : PX.Data.IBqlField { }

        public virtual String AdjdFinPeriodID { get; set; }
        #endregion
        #region AdjdTranPeriodID
        public abstract class adjdTranPeriodID : PX.Data.IBqlField { }

        public virtual String AdjdTranPeriodID { get; set; }
        #endregion
    }
}
