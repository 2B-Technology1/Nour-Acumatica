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
using System.Collections.Generic;
using PX.Objects.GL;
using PX.Objects.AM.Attributes;
using PX.Data.BQL.Fluent;
using PX.Data.BQL;
using static PX.Objects.FS.ServiceOrderFilter;
using System.Windows.Forms;


namespace PX.Objects.AM
{
	/// <summary>
	/// Lock production orders (AM517000) process graph
	/// </summary>
	public class LockOrderProcess : PXGraph<LockOrderProcess>
	{
		public PXCancel<AMProdItem> Cancel;

		[PXFilterable]
		public SelectFrom<AMProdItem>
			.InnerJoin<Branch>
				.On<AMProdItem.branchID.IsEqual<Branch.branchID>>
			.Where<Branch.baseCuryID.IsEqual<AccessInfo.baseCuryID.FromCurrent>
				.And<AMPSetup.lockWorkflowEnabled.FromCurrent.IsEqual<True>
				.And<Brackets<Brackets<AMProdItem.completed.IsEqual<True>
						.And<AMProdItem.locked.IsEqual<False>
						.And<AMProdItem.closed.IsEqual<False>
						.And<LockProductionProcessFilter.processAction.FromCurrent.IsEqual<LockProcessActions.lockAction>>>>>
					.Or<Brackets<Brackets<AMProdItem.locked.IsEqual<True>
						.And<AMProdItem.closed.IsEqual<False>
						.And<LockProductionProcessFilter.processAction.FromCurrent.IsEqual<LockProcessActions.unlockAction>>>>>>>>>>
			.ProcessingView ProductionOrderList;
		public PXFilter<LockProductionProcessFilter> Filter;
		public PXSetup<AMPSetup> ampsetup;

		public LockOrderProcess()
		{
			LockProductionProcessFilter filter = Filter.Current;
			ProductionOrderList.SetProcessDelegate(delegate (List<AMProdItem> list)
			{
				LockProductionOrders(list, filter);
			});

			InquiresDropMenu.AddMenuAction(TransactionsByProductionOrderInq);
		}

		public PXAction<AMProdItem> InquiresDropMenu;
		[PXUIField(DisplayName = Messages.Inquiries)]
		[PXButton(MenuAutoOpen = true)]
		protected virtual IEnumerable inquiresDropMenu(PXAdapter adapter)
		{
			return adapter.Get();
		}

		public static void LockProductionOrders(List<AMProdItem> list, LockProductionProcessFilter filter)
		{
			if (list == null)
			{
				return;
			}
			var failed = false;
			var prodMaint = CreateInstance<ProdMaint>();
			for (var i = 0; i < list.Count; i++)
			{
				var prodItem = list[i];
				try
				{
					prodMaint.Clear();
					prodMaint.ProdMaintRecords.Current = prodItem;					
					var runAction = filter.ProcessAction == LockProcessActions.LockAction ? prodMaint.lockOrder : prodMaint.unlockOrder;
					runAction.Press();
					PXProcessing<AMProdItem>.SetInfo(i, ActionsMessages.RecordProcessed);
				}
				catch (Exception e)
				{
					PXProcessing<AMProdItem>.SetError(i, e.Message);
					failed = true;
				}
			}
			if (failed)
			{
				throw new PXOperationCompletedException(ErrorMessages.SeveralItemsFailed);
			}
		}

		public PXAction<AMProdItem> TransactionsByProductionOrderInq;
		[PXUIField(DisplayName = "Unreleased Transactions", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXLookupButton]
		public virtual IEnumerable transactionsByProductionOrderInq(PXAdapter adapter)
		{
			CallTransactionsByProductionOrderGenericInquiry();

			return adapter.Get();
		}
		protected virtual void CallTransactionsByProductionOrderGenericInquiry()
		{
			var gi = new GITransactionsByProductionOrder();
			gi.SetFilterByProductionStatus(ProductionOrderStatus.Completed);
			gi.SetFilterByUnreleasedBatches();
			gi.CallGenericInquiry();
		}
	}

	/// <summary>
	/// Processing filter for Lock Production Orders (AM517000) processing screen.
	/// </summary>
	[PXCacheName("Lock Production Process Filter")]
	[Serializable]
	public class LockProductionProcessFilter : IBqlTable
	{
		#region ProcessAction

		public abstract class processAction : PX.Data.BQL.BqlString.Field<processAction> { }
		/// <summary>
		/// Production Order Lock Process Action
		/// </summary>
		[PXString(1, IsFixed = true)]
		[PXUnboundDefault(LockProcessActions.LockAction)]
		[PXUIField(DisplayName = "Action")]
		[LockProcessActions.List]
		public virtual string ProcessAction { get; set; }

		#endregion
	}

	/// <summary>
	/// Processing actions for Lock Production Orders (AM517000) processing screen.
	/// </summary>
	public class LockProcessActions
	{
		/// <summary>
		/// Action to lock production orders during processing.
		/// </summary>
		public const string LockAction = "L";
		/// <summary>
		/// Action to unlock previously locked production orders during processing.
		/// </summary>
		public const string UnlockAction = "U";

		public class ListAttribute : PXStringListAttribute
		{
			public ListAttribute()
				: base(new string[] { LockAction, UnlockAction }
				, new string[] { "Lock Order", "Unlock Order" })
			{ }
		}

		public class lockAction : PX.Data.BQL.BqlString.Constant<lockAction>
		{
			public lockAction() : base(LockAction) {; }
		}

		public class unlockAction : PX.Data.BQL.BqlString.Constant<unlockAction>
		{
			public unlockAction() : base(UnlockAction) {; }
		}
	}
}
