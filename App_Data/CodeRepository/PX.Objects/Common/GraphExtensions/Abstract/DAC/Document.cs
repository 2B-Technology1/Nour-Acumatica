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
    public class Document: PXMappedCacheExtension
    {
        #region BranchID
        /// <exclude />
        public abstract class branchID : IBqlField
        {
        }

        public virtual int? BranchID { get; set; }
        #endregion
        #region HeaderDocDate
        /// <exclude />
        public abstract class headerDocDate : IBqlField
        {
        }

        public virtual DateTime? HeaderDocDate { get; set; }
        #endregion
        #region HeaderTranPeriodID
        /// <exclude />
        public abstract class headerTranPeriodID : IBqlField
        {
        }

        public virtual string HeaderTranPeriodID { get; set; }
		#endregion
		#region HeaderFinPeriodID
		/// <exclude />
		public abstract class headerFinPeriodID : IBqlField
		{
		}

		public virtual string HeaderFinPeriodID { get; set; }
		#endregion
	}
}
