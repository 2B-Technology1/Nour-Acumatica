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
using PX.Data.BQL.Fluent;
using PX.Objects.CS;
using PX.Objects.Extensions.MultiCurrency;
using PX.Objects.IN;

namespace PX.Objects.PM
{
	[Serializable]
	public class ProjectBalanceMaint : PXGraph<ProjectBalanceMaint>, PXImportAttribute.IPXPrepareItems
	{
		public class MultiCurrency : MultiCurrencyGraph<ProjectBalanceMaint, PMBudget>
		{
			protected override CurySourceMapping GetCurySourceMapping()
			{
				return new CurySourceMapping(typeof(PMProject))
				{
					AllowOverrideCury = typeof(PMProject.allowOverrideCury),
					AllowOverrideRate = typeof(PMProject.allowOverrideRate),
					CuryRateTypeID = typeof(PMProject.rateTypeID),
					CuryID = typeof(PMProject.curyID)
				};
			}

			protected override bool AllowOverrideCury() => false;

			protected override string Module => "PM";

			protected override DocumentMapping GetDocumentMapping()
			{
				return new DocumentMapping(typeof(PMBudget))
				{
					BAccountID = typeof(PMBudget.projectID),
					CuryInfoID = typeof(PMBudget.curyInfoID),
				};
			}

			protected override PXSelectBase[] GetChildren() => new[]
			{
				Base.Items
			};

			protected override void DocumentRowInserting<CuryInfoID, CuryID>(PXCache sender, object row)
			{
				Document document = row as Document;
				PMBudget budgetLine = document?.Base as PMBudget;
				if (budgetLine == null) return;
				PMProject project = Base.project.Select(budgetLine.ProjectID);
				if (project == null) return;
				document.CuryInfoID = project.CuryInfoID;
				budgetLine.CuryInfoID = project.CuryInfoID;
			}
		}


		[InjectDependency]
		public IUnitRateService RateService { get; set; }

		#region DAC Attributes Override

		[PXDefault]
		[PXParent(typeof(Select<
			PMProject,
			Where<PMProject.contractID, Equal<Current<PMBudget.projectID>>>>))]
		[Project(typeof(Where<PMProject.nonProject, Equal<False>, And<PMProject.baseType, Equal<CT.CTPRType.project>>>), IsKey = true)]
		protected virtual void _(Events.CacheAttached<PMBudget.projectID> e)
		{
		}

		[PMTaskCompleted]
		[PXDefault(typeof(Search<PMTask.taskID, Where<PMTask.projectID, Equal<Current<PMBudget.projectID>>, And<PMTask.isDefault, Equal<True>>>>))]
		[ProjectTask(typeof(PMBudget.projectID), IsKey = true, AlwaysEnabled = true)]
		protected virtual void _(Events.CacheAttached<PMBudget.projectTaskID> e)
		{
		}

		[PXDefault]
		[AccountGroup(IsKey = true)]
		protected virtual void _(Events.CacheAttached<PMBudget.accountGroupID> e)
		{
		}

		[PXMergeAttributes(Method = MergeMethod.Replace)]
		[PXBool]
		[PXDefault(false)]
		protected virtual void _(Events.CacheAttached<PMCostCode.isProjectOverride> e)
		{
		}

		#endregion

		public ProjectBalanceMaint()
		{
			SetDefaultColumnVisibility();
		}

		[PXImport(typeof(PMBudget))]
		[PXViewName(Messages.Budget)]
		[PXFilterable]
		public SelectFrom<PMBudget>
			.InnerJoin<PMTask>
				.On<PMTask.projectID.IsEqual<PMBudget.projectID>
					.And<PMTask.taskID.IsEqual<PMBudget.projectTaskID>>>
			.InnerJoin<PMProject>.On<PMProject.contractID.IsEqual<PMBudget.projectID>>
			.InnerJoin<PMAccountGroup>.On<PMAccountGroup.groupID.IsEqual<PMBudget.accountGroupID>>
			.Where<
				PMProject.nonProject.IsEqual<False>
				.And<PMProject.baseType.IsEqual<CT.CTPRType.project>>
				.And<MatchUser>
				.And<MatchUserFor<PMProject>>>
			.OrderBy<
				PMProject.contractCD.Asc,
				PMTask.taskCD.Asc,
				PMAccountGroup.groupCD.Asc>
			.View Items;

		[PXCopyPasteHiddenView]
		[PXHidden]
		public PXSelect<PMCostCode> dummyCostCode;

		public PXSetup<PMProject>.Where<PMProject.contractID.IsEqual<PMBudget.projectID.AsOptional>> project;

		public PXSavePerRow<PMBudget> Save;
		public PXCancel<PMBudget> Cancel;
		public PXSetup<PMSetup> Setup;
		public PXSelect<PMTask> Task; //Needed to save changes to Completed % by [PMTaskCompleted]


		public PXAction<PMBudget> viewProject;
		[PXUIField(DisplayName = Messages.ViewProject, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton]
		public IEnumerable ViewProject(PXAdapter adapter)
		{
			if (Items.Current != null)
			{
				var service = PXGraph.CreateInstance<PM.ProjectAccountingService>();
				service.NavigateToProjectScreen(Items.Current.ProjectID, PXRedirectHelper.WindowMode.NewWindow);
			}
			return adapter.Get();
		}

		public PXAction<PMBudget> viewTask;
		[PXUIField(DisplayName = Messages.ViewTask, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton]
		public IEnumerable ViewTask(PXAdapter adapter)
		{
			if (Items.Current != null)
			{
				ProjectTaskEntry graph = PXGraph.CreateInstance<ProjectTaskEntry>();
				graph.Task.Current = PMTask.PK.FindDirty(this, Items.Current.ProjectID, Items.Current.ProjectTaskID);

				throw new PXPopupRedirectException(graph, Messages.ProjectTaskEntry + " - " + Messages.ViewTask, true);
			}
			return adapter.Get();
		}

		public PXAction<PMBudget> viewTransactions;
		[PXUIField(DisplayName = Messages.ViewTransactions, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton]
		public IEnumerable ViewTransactions(PXAdapter adapter)
		{
			if (Items.Current != null)
			{
				TransactionInquiry graph = PXGraph.CreateInstance<TransactionInquiry>();
				graph.Filter.Current.ProjectID = Items.Current.ProjectID;
				graph.Filter.Current.ProjectTaskID = Items.Current.ProjectTaskID;
				graph.Filter.Current.AccountGroupID = Items.Current.AccountGroupID;
				graph.Filter.Current.InventoryID = Items.Current.InventoryID == PMInventorySelectorAttribute.EmptyInventoryID ? null : Items.Current.InventoryID;

				throw new PXPopupRedirectException(graph, Messages.ViewTransactions, true);
			}
			return adapter.Get();
		}

		public PXAction<PMBudget> viewCommitments;
		[PXUIField(DisplayName = Messages.ViewCommitments, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton]
		public IEnumerable ViewCommitments(PXAdapter adapter)
		{
			if (Items.Current != null)
			{
				CommitmentInquiry graph = PXGraph.CreateInstance<CommitmentInquiry>();
				graph.Filter.Current.AccountGroupID = Items.Current.AccountGroupID;
				graph.Filter.Current.ProjectID = Items.Current.ProjectID;
				graph.Filter.Current.ProjectTaskID = Items.Current.ProjectTaskID;
				graph.Filter.Current.InventoryID = Items.Current.InventoryID == PMInventorySelectorAttribute.EmptyInventoryID ? null : Items.Current.InventoryID;


				throw new PXPopupRedirectException(graph, Messages.CommitmentEntry + " - " + Messages.ViewCommitments, true);
			}
			return adapter.Get();
		}

		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXUIField(DisplayName = "Project Currency")]
		protected virtual void _(Events.CacheAttached<PMProject.curyID> e) { }

		protected virtual void _(Events.RowSelected<PMBudget> e)
		{
			if (e.Row != null)
			{
				PMProject project = PMProject.PK.Find(this, e.Row.ProjectID);

				PXUIFieldAttribute.SetEnabled<PMCostBudget.curyUnitRate>(e.Cache, null, project?.BudgetFinalized != true);
				PXUIFieldAttribute.SetEnabled<PMCostBudget.qty>(e.Cache, null, project?.BudgetFinalized != true);
				PXUIFieldAttribute.SetEnabled<PMCostBudget.curyAmount>(e.Cache, null, project?.BudgetFinalized != true);

				PXUIFieldAttribute.SetEnabled<PMCostBudget.revisedQty>(e.Cache, null, project?.ChangeOrderWorkflow != true);
				PXUIFieldAttribute.SetEnabled<PMCostBudget.curyRevisedAmount>(e.Cache, null, project?.ChangeOrderWorkflow != true);
			}
		}

		protected virtual void _(Events.FieldUpdated<PMBudget, PMCostBudget.curyAmount> e)
		{
			if (e.Row != null)
			{
				PMBudget row = (PMBudget)e.Row;
				PMProject project = PMProject.PK.Find(this, row.ProjectID);

				if (project?.BudgetFinalized == true)
				{
					row.CuryAmount = e.OldValue as decimal?;
				}
			}
		}

		protected virtual void PMBudget_UOM_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			PMBudget row = e.Row as PMBudget;
			if (row == null || string.IsNullOrEmpty(row.UOM)) return;

			var select = new PXSelect<PMTran, Where<PMTran.projectID, Equal<Current<PMBudget.projectID>>,
				And<PMTran.taskID, Equal<Current<PMBudget.projectTaskID>>,
				And<PMTran.costCodeID, Equal<Current<PMBudget.costCodeID>>,
				And<PMTran.inventoryID, Equal<Current<PMBudget.inventoryID>>,
				And2<Where<PMTran.accountGroupID, Equal<Current<PMBudget.accountGroupID>>, Or<PMTran.offsetAccountGroupID, Equal<Current<PMBudget.accountGroupID>>>>,
				And<PMTran.released, Equal<True>,
				And<PMTran.uOM, NotEqual<Required<PMTran.uOM>>>>>>>>>>(this);

			string uom = (string)e.NewValue;
			if (!string.IsNullOrEmpty(uom))
			{
				PMTran tranInOtherUOM = select.SelectWindowed(0, 1, uom);

				if (tranInOtherUOM != null)
				{
					var ex = new PXSetPropertyException(Messages.OtherUomUsedInTransaction);
					ex.ErrorValue = uom;
					throw ex;
				}
			}
		}

		protected virtual void _(Events.FieldUpdated<PMBudget, PMBudget.accountGroupID> e)
		{
			e.Cache.SetDefaultExt<PMBudget.type>(e.Row);
			e.Cache.SetDefaultExt<PMBudget.curyUnitRate>(e.Row);
		}

		protected virtual void _(Events.FieldUpdated<PMBudget, PMBudget.curyAmount> e)
		{
			if (e.Row != null)
			{
				e.Row.CuryRevisedAmount = e.Row.CuryAmount;
			}
		}

		protected virtual void _(Events.FieldUpdated<PMBudget, PMBudget.qty> e)
		{
			if (e.Row != null)
			{
				e.Row.RevisedQty = e.Row.Qty;
			}
		}

		protected virtual void _(Events.FieldDefaulting<PMBudget, PMBudget.type> e)
		{
			PMAccountGroup ag = PMAccountGroup.PK.Find(this, e.Row.AccountGroupID);
			if (ag != null)
			{
				if (ag.Type == PMAccountType.OffBalance)
					e.NewValue = ag.IsExpense == true ? GL.AccountType.Expense : ag.Type;
				else
					e.NewValue = ag.Type;
			}
		}

		protected virtual void _(Events.FieldDefaulting<PMBudget, PMBudget.costCodeID> e)
		{
			if (e.Row == null) return;

			PMProject project = PMProject.PK.Find(this, e.Row.ProjectID);
			if (project != null)
			{
				if (project.BudgetLevel != BudgetLevels.CostCode)
				{
					e.NewValue = CostCodeAttribute.GetDefaultCostCode();
				}
			}
		}

		protected virtual void _(Events.FieldDefaulting<PMBudget, PMBudget.inventoryID> e)
		{
			if (e.Row == null) return;

			e.NewValue = PM.PMInventorySelectorAttribute.EmptyInventoryID;
		}

		protected virtual void _(Events.FieldDefaulting<PMBudget, PMBudget.description> e)
		{
			if (e.Row == null) return;

			if (CostCodeAttribute.UseCostCode())
			{
				if (e.Row.CostCodeID != null && e.Row.CostCodeID != CostCodeAttribute.GetDefaultCostCode())
				{
					PMCostCode costCode = PXSelectorAttribute.Select<PMBudget.costCodeID>(e.Cache, e.Row) as PMCostCode;
					if (costCode != null)
					{
						e.NewValue = costCode.Description;
					}
				}
			}
			else
			{
				if (e.Row.InventoryID != null && e.Row.InventoryID != PMInventorySelectorAttribute.EmptyInventoryID)
				{
					InventoryItem item = PXSelectorAttribute.Select<PMBudget.inventoryID>(e.Cache, e.Row) as InventoryItem;
					if (item != null)
					{
						e.NewValue = item.Descr;
					}
				}
			}
		}

		protected virtual void _(Events.FieldUpdated<PMBudget, PMBudget.inventoryID> e)
		{
			e.Cache.SetDefaultExt<PMBudget.description>(e.Row);

			if (e.Row.AccountGroupID == null)
				e.Cache.SetDefaultExt<PMBudget.accountGroupID>(e.Row);

			e.Cache.SetDefaultExt<PMBudget.uOM>(e.Row);
			e.Cache.SetDefaultExt<PMBudget.curyUnitRate>(e.Row);
			e.Cache.SetDefaultExt<PMBudget.taxCategoryID>(e.Row);
		}

		protected virtual void _(Events.FieldUpdated<PMBudget, PMBudget.uOM> e)
		{
			e.Cache.SetDefaultExt<PMBudget.curyUnitRate>(e.Row);
		}

		protected virtual void _(Events.FieldUpdated<PMBudget, PMBudget.costCodeID> e)
		{
			PMProject project = PMProject.PK.Find(this, e.Row.ProjectID);
			if (CostCodeAttribute.UseCostCode() && project?.BudgetLevel == BudgetLevels.CostCode)
				e.Cache.SetDefaultExt<PMBudget.description>(e.Row);
		}

		protected virtual void _(Events.FieldDefaulting<PMBudget, PMBudget.curyUnitRate> e)
		{
			PMAccountGroup ag = PMAccountGroup.PK.Find(this, e.Row.AccountGroupID);
			if (ag != null)
			{
				PMProject project = PMProject.PK.Find(this, e.Row.ProjectID);
				if (ag.IsExpense == true)
				{
					decimal? unitCost = RateService.CalculateUnitCost(e.Cache, e.Row.ProjectID, e.Row.ProjectTaskID, e.Row.InventoryID, e.Row.UOM, null, project.StartDate, project.CuryInfoID);
					e.NewValue = unitCost ?? 0m;
				}
				else
				{
					decimal? unitPrice = RateService.CalculateUnitPrice(e.Cache, e.Row.ProjectID, e.Row.ProjectTaskID, e.Row.InventoryID, e.Row.UOM, e.Row.Qty, project.StartDate, project.CuryInfoID);
					e.NewValue = unitPrice ?? 0m;
				}
			}
		}

		#region PMImport Implementation
		public bool PrepareImportRow(string viewName, IDictionary keys, IDictionary values)
		{
			if (!CostCodeAttribute.UseCostCode())
			{
				PMCostCode defaultCostCode = PXSelect<PMCostCode, Where<PMCostCode.isDefault, Equal<True>>>.Select(this);
				if (defaultCostCode != null)
				{
					keys[nameof(PMBudget.CostCodeID)] = defaultCostCode.CostCodeCD;
					values[nameof(PMBudget.CostCodeID)] = defaultCostCode.CostCodeCD;
				}
			}

			return true;
		}

		public bool RowImporting(string viewName, object row)
		{
			return row == null;
		}

		public bool RowImported(string viewName, object row, object oldRow)
		{
			return oldRow == null;
		}

		public void PrepareItems(string viewName, IEnumerable items) { }
		#endregion

		public virtual void SetDefaultColumnVisibility()
		{
			if (PXAccess.FeatureInstalled<FeaturesSet.costCodes>())
			{
				PXUIFieldAttribute.SetVisible<PMBudget.inventoryID>(Items.Cache, null, false);
			}

			bool commitmentTracking = Setup.Current.CostCommitmentTracking == true;
			viewCommitments.SetVisible(commitmentTracking);
			PXUIFieldAttribute.SetVisible<PMBudget.curyCommittedAmount>(Items.Cache, null, commitmentTracking);
			PXUIFieldAttribute.SetVisible<PMBudget.committedQty>(Items.Cache, null, commitmentTracking);
			PXUIFieldAttribute.SetVisible<PMBudget.curyCommittedInvoicedAmount>(Items.Cache, null, commitmentTracking);
			PXUIFieldAttribute.SetVisible<PMBudget.committedInvoicedQty>(Items.Cache, null, commitmentTracking);
			PXUIFieldAttribute.SetVisible<PMBudget.curyCommittedOpenAmount>(Items.Cache, null, commitmentTracking);
			PXUIFieldAttribute.SetVisible<PMBudget.committedOpenQty>(Items.Cache, null, commitmentTracking);
			PXUIFieldAttribute.SetVisible<PMBudget.committedReceivedQty>(Items.Cache, null, commitmentTracking);
		}

		public override int ExecuteUpdate(string viewName, IDictionary keys, IDictionary values, params object[] parameters)
		{
			return base.ExecuteUpdate(viewName, keys, values, parameters);
		}
	}
}
