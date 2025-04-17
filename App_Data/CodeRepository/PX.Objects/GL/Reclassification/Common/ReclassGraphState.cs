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

using System.Collections.Generic;
using PX.Data;

namespace PX.Objects.GL.Reclassification.Common
{
	public class ReclassGraphState : IBqlTable
	{
		public virtual ReclassScreenMode ReclassScreenMode { get; set; }

		public virtual string EditingBatchModule { get; set; }
		public virtual string EditingBatchNbr { get; set; }
		public virtual string EditingBatchMasterPeriodID { get; set; }
		public virtual string EditingBatchCuryID { get; set; }

		public virtual string OrigBatchModuleToReverse { get; set; }
		public virtual string OrigBatchNbrToReverse { get; set; }

        private int currentSplitLineNbr;
        public virtual int CurrentSplitLineNbr { get { return currentSplitLineNbr; } }
        public virtual void IncSplitLineNbr()
        {
            currentSplitLineNbr += 1;
        }

        public virtual HashSet<GLTranForReclassification> GLTranForReclassToDelete { get; set; }

		public Dictionary<GLTranKey, List<GLTranKey>> SplittingGroups { get; set; }

        public ReclassGraphState()
		{
			GLTranForReclassToDelete = new HashSet<GLTranForReclassification>();
            SplittingGroups = new Dictionary<GLTranKey, List<GLTranKey>>();
            currentSplitLineNbr = int.MinValue;
        }

        public void ClearSplittingGroups()
        {
            SplittingGroups = new Dictionary<GLTranKey, List<GLTranKey>>();
            currentSplitLineNbr = int.MinValue;
        }
    }
}
