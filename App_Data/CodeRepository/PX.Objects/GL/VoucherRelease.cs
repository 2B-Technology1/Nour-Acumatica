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
using System.Collections;
using System.Collections.Generic;

using PX.Data;
using PX.Objects.BQLConstants;
using PX.Objects.CM;
using PX.Objects.CS;
using PX.SM;

namespace PX.Objects.GL
{
    [PX.Objects.GL.TableAndChartDashboardType]
    public class VoucherRelease : PXGraph<VoucherRelease>
    {
		#region Type Override events

		#region BranchID

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXUIField(DisplayName = "Branch", Visible = false)]
		[PXUIVisible(typeof(FeatureInstalled<FeaturesSet.branch>.Or<FeatureInstalled<FeaturesSet.multiCompany>>))]
		protected virtual void _(Events.CacheAttached<GLDocBatch.branchID> e) { }
		#endregion

		#endregion

		public PXCancel<GLDocBatch> Cancel;

		[Obsolete(Common.Messages.WillBeRemovedInAcumatica2019R1)]
		public PXAction<GLDocBatch> EditDetail;

        [PXFilterable]
        public PXProcessing<GLDocBatch, Where<GLDocBatch.hold, Equal<boolFalse>,
                                        And<GLDocBatch.released, Equal<boolFalse>>>> Documents;

		public static void ReleaseVoucher(GLBatchDocRelease graph, GLDocBatch batch)
		{
			graph.ReleaseBatchProc(batch, false);
		}

		[PXUIField(DisplayName = "", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select, Visible = false)]
        [PXEditDetailButton]
        public virtual IEnumerable editDetail(PXAdapter adapter)
        {
            if (this.Documents.Current != null)
            {
                JournalWithSubEntry graph = PXGraph.CreateInstance<JournalWithSubEntry>();
                graph.BatchModule.Current = graph.BatchModule.Search<GLDocBatch.batchNbr>(this.Documents.Current.BatchNbr, this.Documents.Current.Module);
                if (graph.BatchModule.Current != null)
                {
                    throw new PXRedirectRequiredException(graph, true, "ViewBatch") { Mode = PXBaseRedirectException.WindowMode.NewWindow };
                }
            }
            return adapter.Get();
        }
		
        public VoucherRelease()
        {
            Documents.SetProcessDelegate<GLBatchDocRelease>(ReleaseVoucher);
            Documents.SetProcessCaption(Messages.ProcRelease);
            Documents.SetProcessAllCaption(Messages.ProcReleaseAll);
		}
    }
}