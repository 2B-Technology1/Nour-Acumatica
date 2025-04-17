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

using CommonServiceLocator;
using PX.Data;
using PX.Objects.CN.Subcontracts.SC.Graphs;
using PX.Objects.CS;
using PX.Objects.GL;
using PX.Objects.IN;
using PX.Objects.PO;
using System;
using System.Collections;
using System.Linq;

namespace PX.Objects.PM
{
    public class POOrderEntryExt : CommitmentTracking<POOrderEntry>
	{
		[InjectDependency]
		public IProjectMultiCurrency MultiCurrencyService { get; set; }

		public static new bool IsActive()
		{
			return PXAccess.FeatureInstalled<FeaturesSet.projectAccounting>();
		}
		public PXSetup<PMSetup> Setup;

		[PXFilterable]
		[PXViewName(PM.Messages.ChangeOrder)]
		[PXCopyPasteHiddenView()]
		public PXSelectJoin<PMChangeOrderLine,
			InnerJoin<PMChangeOrder, On<PMChangeOrder.refNbr, Equal<PMChangeOrderLine.refNbr>>>,
			Where<PMChangeOrderLine.pOOrderType, Equal<Current<POOrder.orderType>>,
			And<PMChangeOrderLine.pOOrderNbr, Equal<Current<POOrder.orderNbr>>>>> ChangeOrders;

		[Project]
		protected virtual void _(Events.CacheAttached<PMChangeOrderLine.projectID> e) { }

		public override void Initialize()
		{
			base.Initialize();
			Base.OnBeforePersist += SetBehaviorBasedOnLines;
		}
		
		public PXAction<POOrder> viewOrigChangeOrder;
		[PXUIField(DisplayName = PM.Messages.ViewChangeOrder, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton(ImageKey = PX.Web.UI.Sprite.Main.DataEntry)]

		public virtual IEnumerable ViewOrigChangeOrder(PXAdapter adapter)
		{
			if (ChangeOrders.Current != null)
			{
				PMChangeOrder cOrder = PMChangeOrder.PK.Find(Base, ChangeOrders.Current.RefNbr);
				if (cOrder != null && cOrder.OrigRefNbr != null)
				{
					ChangeOrderEntry target = PXGraph.CreateInstance<ChangeOrderEntry>();
					target.Clear(PXClearOption.ClearAll);
					target.SelectTimeStamp();
					target.Document.Current = PMChangeOrder.PK.Find(Base, cOrder.OrigRefNbr);
					throw new PXRedirectRequiredException(target, true, "View Change Order") { Mode = PXBaseRedirectException.WindowMode.NewWindow };
				}
			}
			return adapter.Get();
		}

		public PXAction<POOrder> viewChangeOrder;
		[PXUIField(DisplayName = PM.Messages.ViewChangeOrder, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select, Enabled = true)]
		[PXButton(ImageKey = PX.Web.UI.Sprite.Main.Inquiry)]
		public virtual IEnumerable ViewChangeOrder(PXAdapter adapter)
		{
			if (ChangeOrders.Current != null)
			{
				ChangeOrderEntry target = PXGraph.CreateInstance<ChangeOrderEntry>();
				target.Clear(PXClearOption.ClearAll);
				target.SelectTimeStamp();
				target.Document.Current = PMChangeOrder.PK.Find(Base, ChangeOrders.Current.RefNbr);
				throw new PXRedirectRequiredException(target, true, "ViewInvoice") { Mode = PXBaseRedirectException.WindowMode.NewWindow };

			}
			return adapter.Get();
		}

		protected virtual void POOrder_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			ChangeOrders.Cache.AllowSelect = IsCommitmentsEnabled();
		}

		protected virtual void POOrder_RowDeleting(PXCache sender, PXRowDeletingEventArgs e)
		{
			POOrder doc = (POOrder)e.Row;
			if (doc.Hold != true && doc.Behavior == POBehavior.ChangeOrder)
			{
				throw new PXException(PX.Objects.PO.Messages.CanNotDeleteWithChangeOrderBehavior);
			}
		}

		public bool SkipProjectLockCommitmentsVerification;

		protected virtual void _(Events.FieldVerifying<POLine, POLine.projectID> e)
		{
			if (!SkipProjectLockCommitmentsVerification)
				VerifyProjectLockCommitments((int?)e.NewValue, e.Cache);

			VerifyExchangeRateExistsForProject((int?)e.NewValue);
		}

		protected virtual void _(Events.FieldVerifying<POOrder, POOrder.projectID> e)
		{
			if (!SkipProjectLockCommitmentsVerification)
				VerifyProjectLockCommitments((int?)e.NewValue,e.Cache);

			VerifyExchangeRateExistsForProject((int?)e.NewValue);
		}
		protected virtual void _(Events.FieldUpdated<POLine, POLine.lineType> e)
		{
			if (e.Row == null) return;

			POLine pOLine = e.Row;
			if (pOLine.LineType == POLineType.Description)
			{
				e.Cache.SetValueExt<POLine.projectID>(e.Row, ProjectDefaultAttribute.NonProject());
			}
		}

		public virtual void VerifyProjectLockCommitments(int? newProjectID, PXCache cache)
		{
			if (!PXAccess.FeatureInstalled<FeaturesSet.changeOrder>())
				return;

			PMProject project;
			var graphType = cache.Graph.GetType().BaseType;

			if (ProjectDefaultAttribute.IsProject(Base, newProjectID, out project) && project.LockCommitments == true)
			{
				if (graphType == typeof(SubcontractEntry))
				{
					var ex = new PXSetPropertyException(PM.Messages.ProjectCommintmentsLockedForSubcontracts, project.ContractCD);
					ex.ErrorValue = project.ContractCD;

					throw ex;
				}
				else if (graphType == typeof(POOrderEntry))
				{
					var ex = new PXSetPropertyException(PM.Messages.ProjectCommintmentsLockedForPurchaseOrders, project.ContractCD);
					ex.ErrorValue = project.ContractCD;

					throw ex;
				}
				else
				{
					var ex = new PXSetPropertyException(PM.Messages.ProjectCommintmentsLocked);
					ex.ErrorValue = project.ContractCD;

					throw ex;
				}
			}
		}

		public virtual void VerifyExchangeRateExistsForProject(int? newProjectID)
		{
			if (!PXAccess.FeatureInstalled<FeaturesSet.projectMultiCurrency>())
				return;

			if (!IsCommitmentsEnabled())
				return;

			PMProject project;
			if (ProjectDefaultAttribute.IsProject(Base, newProjectID, out project) && Base.Document.Current != null)
			{
				//with throw PXException if conversion is required but rate is not defined.
				decimal result = MultiCurrencyService.GetValueInProjectCurrency(Base, project, Base.Document.Current.CuryID, Base.Document.Current.OrderDate, 100m);
			}
		}

		private bool IsCommitmentsEnabled()
		{
			PMSetup setup = PXSelect<PMSetup>.Select(Base);
			var result = setup?.CostCommitmentTracking == true;
			return result;
		}

		public virtual void SetBehaviorBasedOnLines(PXGraph obj)
		{
			if (Base.Document.Current != null && PXAccess.FeatureInstalled<FeaturesSet.changeOrder>())
			{
				bool changeOrderBehaviorIsRequired = false;

				var select = new PXSelect<PMChangeOrderLine,
					Where<PMChangeOrderLine.pOOrderType, Equal<Current<POOrder.orderType>>,
					And<PMChangeOrderLine.pOOrderNbr, Equal<Current<POOrder.orderNbr>>>>>(Base);

				PMChangeOrderLine link = select.SelectSingle();
				if (link != null)
				{
					changeOrderBehaviorIsRequired = true;
				}

				if (changeOrderBehaviorIsRequired && Base.Document.Current.Behavior != POBehavior.ChangeOrder)
				{
					Base.Document.Current.Behavior = POBehavior.ChangeOrder;
					Base.Document.Update(Base.Document.Current);
				}
				else if (!changeOrderBehaviorIsRequired && Base.Document.Current.Behavior == POBehavior.ChangeOrder)
				{
					Base.Document.Current.Behavior = POBehavior.Standard;
					Base.Document.Update(Base.Document.Current);
				}
			}
		}

		[PXOverride]
		public virtual IEnumerable CreateAPInvoice(PXAdapter adapter, Func<PXAdapter, IEnumerable> baseHandler)
		{
			ValidatePOLines();
			return baseHandler(adapter);
		}

		private void ValidatePOLines()
		{
			bool accountValidationFailed = false;
			foreach (POLine line in Base.Transactions.Select())
			{
				Account account = Account.PK.Find(Base, line.ExpenseAcctID);
				if (account?.AccountGroupID == null && line.LineType == POLineType.Service && ProjectDefaultAttribute.IsProject(Base, line.ProjectID))
				{
					InventoryItem item = InventoryItem.PK.Find(Base, line.InventoryID);
					if (item != null && item.PostToExpenseAccount == InventoryItem.postToExpenseAccount.Sales)
						continue;

					PXTrace.WriteError(Messages.NoAccountGroup2, account.AccountCD);
					accountValidationFailed = true;
				}
			}

			if (accountValidationFailed)
			{
				throw new PXException(Messages.FailedToAddLine);
			}
		}

	}
}
