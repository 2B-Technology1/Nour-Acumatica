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
using PX.Data;


namespace PX.Objects.PM
{
	public class AllocationAudit : PXGraph<AllocationAudit>
	{
		#region Select
		public PXFilter<AllocationPMTran> destantion;
		[PXHidden]
		public PXSelect<PMAllocationSourceTran> allocationSourceTran;
		public PXSelectJoin<PMTran, LeftJoin<PMAllocationSourceTran, On<PMTran.tranID, Equal<PMAllocationSourceTran.tranID>>>> source;

		public IEnumerable Source(PXAdapter adapter)
		{
			return GetSources(destantion.Current.TranID);
		}

		public IEnumerable GetSources(Int64? dst)
		{
			if (dst == null)
				yield break;

			var selectAudit = new PXSelect<PMAllocationAuditTran, Where<PMAllocationAuditTran.tranID, Equal<Required<PMAllocationAuditTran.tranID>>>>(this);
			var selectSource = new PXSelectJoin<PMTran, LeftJoin<PMAllocationSourceTran, On<PMTran.tranID, Equal<PMAllocationSourceTran.tranID>>>, Where<PMAllocationSourceTran.tranID, Equal<Required<PMAllocationSourceTran.tranID>>>>(this);
			foreach (PMAllocationAuditTran trail in selectAudit.Select(dst))
			{
				foreach (PXResult<PMTran, PMAllocationSourceTran> source in selectSource.Select(trail.SourceTranID))
				{
					yield return source;
				}
			}
		}
		#endregion

		#region Ctr
		public AllocationAudit()
		{
			source.Cache.AllowDelete = false;
			source.Cache.AllowInsert = false;
			source.Cache.AllowUpdate = false;
		}
		#endregion

		#region Action
		public PXAction<PMTran> viewBatch;
		[PXUIField(DisplayName = Messages.ViewBatch, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton]
		public virtual IEnumerable ViewBatch(PXAdapter adapter)
		{
			PMRegister row = PXSelect<PMRegister, Where<PMRegister.refNbr, Equal<Current<PMTran.refNbr>>, And<PMRegister.module, Equal<Current<PMTran.tranType>>>>>.Select(this);
			PXRedirectHelper.TryRedirect(this, row, PXRedirectHelper.WindowMode.NewWindow);
			return adapter.Get();
		}


		public PXAction<PMTran> viewAllocationRule;
		[PXUIField(DisplayName = Messages.ViewBatch, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton]
		public virtual IEnumerable ViewAllocationRule(PXAdapter adapter)
		{
			AllocationMaint graph = CreateInstance<AllocationMaint>();
			PMAllocation row = PXSelectJoin<
				PMAllocation
				, InnerJoin<PMAllocationSourceTran, On<PMAllocation.allocationID, Equal<PMAllocationSourceTran.allocationID>>>
				, Where<PMAllocationSourceTran.tranID, Equal<Required<PMTran.refNbr>>>
				>.Select(this, source.Current.TranID);
			graph.Allocations.Current = row;
			PXRedirectHelper.TryRedirect(graph, PXRedirectHelper.WindowMode.NewWindow);
			return adapter.Get();
		}

		#endregion

		#region Handler

		[PXUIField(DisplayName = "Allocation Step")]
		[PXDBInt(IsKey = true)]
		public void _(Events.CacheAttached<PMAllocationSourceTran.stepID> e)
		{
		}

		#endregion

	}

	[Serializable]
	[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
	[PXHidden]
	public partial class AllocationPMTran : IBqlTable
	{
		#region TranID
		public abstract class tranID : PX.Data.BQL.BqlLong.Field<tranID> { }
		protected Int64? _TranID;
		[PXDBLongIdentity(IsKey = true)]
		public virtual Int64? TranID { get; set; }
		#endregion

	}
}
