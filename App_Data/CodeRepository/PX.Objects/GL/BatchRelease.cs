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
using System.Text;
using PX.Data;
using System.Collections;
using System.Collections.Generic;
using PX.Objects.BQLConstants;
using PX.Objects.CS;
using PX.Objects.CM;
using PX.SM;
using System.Linq;

namespace PX.Objects.GL
{
	[PX.Objects.GL.TableAndChartDashboardType]
	public class BatchRelease : PXGraph<BatchRelease>
	{
        #region Type Override events

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
		[PXViewDetailsButton(typeof(Batch.batchNbr), WindowMode = PXRedirectHelper.WindowMode.NewWindow )]
		[PXFilterable]
		public PXProcessing<Batch, 
			Where<Batch.released, Equal<boolFalse>, 
			And<Batch.scheduled, Equal<boolFalse>, 
			And<Batch.voided,Equal<boolFalse>,
            And<Batch.hold, Equal<boolFalse>>>>>> BatchList;

        public BatchRelease()
		{
			GLSetup setup = GLSetup.Current;
            BatchList.SetProcessDelegate<PostGraph>(ReleaseBatch);
            BatchList.SetProcessCaption(Messages.ProcRelease);
            BatchList.SetProcessAllCaption(Messages.ProcReleaseAll);
			PXNoteAttribute.ForcePassThrow<Batch.noteID>(BatchList.Cache);
		}

		public PXSetup<GLSetup> GLSetup;
		
		public static void ReleaseBatch(PostGraph pg, Batch batch)
        {
            pg.Clear();
            pg.ReleaseBatchProc(batch);
            if ((bool)batch.AutoReverse)
            {
                Batch copy = pg.ReverseBatchProc(batch);
                if (pg.AutoPost)
                {
                    pg.PostBatchProc(batch);
                }
                pg.Clear();
				pg.TimeStamp = copy.tstamp;
                pg.ReleaseBatchProc(copy);
                if (pg.AutoPost)
                {
                    pg.PostBatchProc(copy);
                }
            }
            else if (pg.AutoPost)
            {
                pg.PostBatchProc(batch);
            }
        }


	}
}