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
using PX.Objects.BQLConstants;
using PX.Objects.CM;
using PX.Objects.CS;
using PX.SM;

namespace PX.Objects.GL
{
	[PX.Objects.GL.TableAndChartDashboardType]
	public class BatchPost : PXGraph<BatchPost>
	{
        #region Cache Attached Events
   
        #region ControlTotal
        
        [PXDBBaseCury(typeof(Batch.ledgerID))]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Control Total")]
        protected virtual void Batch_ControlTotal_CacheAttached(PXCache sender)
        {
        }
		#endregion

		#region BranchID

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXUIField(DisplayName = "Branch", Visible = false)]
		[PXUIVisible(typeof(FeatureInstalled<FeaturesSet.branch>.Or<FeatureInstalled<FeaturesSet.multiCompany>>))]
		protected virtual void _(Events.CacheAttached<Batch.branchID> e) { }
		#endregion

		#endregion

		public PXCancel<Batch> Cancel;
		[PXViewDetailsButton(typeof(Batch.batchNbr), WindowMode = PXRedirectHelper.WindowMode.NewWindow)]
       
		[PXFilterable]
		public PXProcessing<Batch, Where<Batch.status, Equal<statusU>>> BatchList;
    

        public BatchPost()
        {
			GLSetup setup = GLSetup.Current;
			BatchList.SetProcessDelegate<PostGraph>(
                delegate(PostGraph pg, Batch batch)
                {
                    pg.Clear();
                    pg.PostBatchProc(batch);
                });

			BatchList.SetProcessCaption(Messages.ProcPost);
			BatchList.SetProcessAllCaption(Messages.ProcPostAll);
			PXNoteAttribute.ForcePassThrow<Batch.noteID>(BatchList.Cache);
        }
	
		public PXSetup<GLSetup> GLSetup;
	}
}