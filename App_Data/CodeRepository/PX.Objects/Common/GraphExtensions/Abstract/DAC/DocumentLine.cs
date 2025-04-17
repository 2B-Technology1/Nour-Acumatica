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
using PX.Data;

namespace PX.Objects.Common.GraphExtensions.Abstract.DAC
{
    public class DocumentLine : PXMappedCacheExtension
    {
        #region BranchID

        /// <exclude />
        public abstract class branchID : IBqlField
        {
        }

        /// <summary>The identifier of the branch associated with the document.</summary>
        public virtual Int32? BranchID { get; set; }

        #endregion

        #region TranDate

        public abstract class tranDate : PX.Data.IBqlField
        {
        }

        public virtual DateTime? TranDate { get; set; }

        #endregion

        #region FinPeriodID

        /// <exclude />
        public abstract class finPeriodID : IBqlField
        {
        }

        public virtual string FinPeriodID { get; set; }

        #endregion

        #region TranPeriodID

        /// <exclude />
        public abstract class tranPeriodID : IBqlField
        {
        }

        public virtual string TranPeriodID { get; set; }

        #endregion
    }
}
