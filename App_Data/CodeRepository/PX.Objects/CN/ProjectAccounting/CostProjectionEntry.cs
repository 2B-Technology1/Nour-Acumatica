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
using System.Linq;

using PX.Data;
using PX.Data.BQL.Fluent;
using PX.Objects.Common.Extensions;
using PX.Objects.CS;
using PX.Objects.EP;
using PX.Objects.GL;
using PX.Objects.GL.FinPeriods;
using PX.Objects.GL.FinPeriods.TableDefinition;
using PX.Objects.IN;
using PX.Objects.PM;

using Messages = PX.Objects.PM.Messages;

namespace PX.Objects.CN.ProjectAccounting
{
	public class CostProjectionEntry : PXGraph<CostProjectionEntry, PMCostProjection>, PXImportAttribute.IPXPrepareItems
	{
		#region DAC Overrides

		#region EPApproval Cache Attached - Approvals Fields
		[PXDBDate()]
		[PXDefault(typeof(PMCostProjection.date), PersistingCheck = PXPersistingCheck.Nothing)]
		protected virtual void _(Events.CacheAttached<EPApproval.docDate> e)
		{
		}

		[PXDBString(60, IsUnicode = true)]
		[PXDefault(typeof(PMCostProjection.description), PersistingCheck = PXPersistingCheck.Nothing)]
		protected virtual void _(Events.CacheAttached<EPApproval.descr> e)
		{
		}
		#endregion

		#endregion

		[PXViewName(Messages.CostProjection)]
		public SelectFrom<PMCostProjection>
			.LeftJoin<PMProject>
				.On<PMProject.contractID.IsEqual<PMCostProjection.projectID>>
			.Where<PMProject.contractID.IsNull
				.Or<MatchUserFor<PMProject>>>
			.View Document;

		public PXSelect<PMCostProjection,
			Where<PMCostProjection.projectID, Equal<Current<PMCostProjection.projectID>>,
				And<PMCostProjection.revisionID, Equal<Current<PMCostProjection.revisionID>>>>> DocumentSettings;

		[PXViewName(Messages.Project)]
		public PXSelect<PMProject,
			Where<PMProject.contractID, Equal<Current<PMCostProjection.projectID>>>> Project;

		[PXViewName(Messages.CostProjectionClass)]
		public PXSelect<PMCostProjectionClass,
			Where<PMCostProjectionClass.classID, Equal<Current<PMCostProjection.classID>>>> Class;

		[PXViewName(Messages.CostProjectionDetail)]
		[PXImport(typeof(PMCostProjection))]
		public PXSelect<PMCostProjectionLine,
			Where<PMCostProjectionLine.projectID, Equal<Current<PMCostProjection.projectID>>,
				And<PMCostProjectionLine.revisionID, Equal<Current<PMCostProjection.revisionID>>>>> Details;

		[PXViewName(Messages.CostProjectionHistory)]
		public PXSelectJoin<PMCostProjectionLine,
			InnerJoin<PMCostProjection, On<PMCostProjectionLine.projectID, Equal<PMCostProjection.projectID>,
				And<PMCostProjectionLine.revisionID, Equal<PMCostProjection.revisionID>>>>,
			Where<PMCostProjectionLine.projectID, Equal<Current<PMCostProjectionLine.projectID>>,
				And<PMCostProjectionLine.taskID, Equal<Current<PMCostProjectionLine.taskID>>,
				And<PMCostProjectionLine.accountGroupID, Equal<Current<PMCostProjectionLine.accountGroupID>>,
				And<PMCostProjectionLine.inventoryID, Equal<Current<PMCostProjectionLine.inventoryID>>,
				And<PMCostProjectionLine.costCodeID, Equal<Current<PMCostProjectionLine.costCodeID>>,
				And<PMCostProjection.classID, Equal<Current<PMCostProjection.classID>>>>>>>>> History;

		public PXSelectJoin<PMCostProjectionLine,
				InnerJoin<PMCostProjection, On<PMCostProjectionLine.projectID, Equal<PMCostProjection.projectID>>>,
				Where<PMCostProjectionLine.projectID, Equal<Current<PMCostProjection.projectID>>,
					And<PMCostProjection.released, Equal<True>>>> ReleasedDetails;

		public PXSelect<PMCostBudget,
			Where<PMCostBudget.projectID, Equal<Current<PMCostProjection.projectID>>,
				And<PMCostBudget.type, Equal<GL.AccountType.expense>>>> CostBudget;

		public PXSetup<Company> Company;

		public PXSetup<PMSetup> Setup;

		public PXFilter<PMCostProjectionCopyDialogInfo> CopyDialog;

		[PXCopyPasteHiddenView]
		[PXViewName(Messages.Approval)]
		public EPApprovalAutomation<PMCostProjection, PMCostProjection.approved, PMCostProjection.rejected, PMCostProjection.hold, PMSetupCostProjectionApproval> Approval;

		[PXViewName(Messages.Project)]
		public PXSelect<PMForecastProject,
			Where<PMForecastProject.contractID, Equal<Current<PMCostProjection.projectID>>>> ProjectTotals;
		public virtual IEnumerable projectTotals()
		{
			var projectTotals = new PXSelect<PMForecastProject, Where<PMForecastProject.contractID, Equal<Current<PMCostProjection.projectID>>>>(this);
			var costBudget = CostBudget.Select().RowCast<PMCostBudget>();
			var detailsKey = Details.Select().RowCast<PMCostProjectionLine>().Select(detail => GetBudgetKey(detail));
			var releasedDetailsKey = ReleasedDetails.Select().RowCast<PMCostProjectionLine>().Select(rdetail => GetBudgetKey(rdetail));
			foreach (PMForecastProject projectTotal in projectTotals.Select().RowCast<PMForecastProject>())
			{
				projectTotal.TotalBudgetedVarianceAmount = 0;
				foreach (var cost in costBudget)
				{
					BudgetKeyTuple costKey = BudgetKeyTuple.Create(cost);
					if (!detailsKey.Contains(costKey) && releasedDetailsKey.Contains(costKey))
						projectTotal.TotalBudgetedVarianceAmount += cost.CuryCostProjectionCostAtCompletion.GetValueOrDefault() - cost.CuryRevisedAmount.GetValueOrDefault();
				}
				projectTotal.TotalBudgetedVarianceAmount += Document.Current.TotalVarianceAmount.GetValueOrDefault();
				projectTotal.TotalProjectedGrossProfit = projectTotal.TotalBudgetedGrossProfit.GetValueOrDefault() - projectTotal.TotalBudgetedVarianceAmount.GetValueOrDefault();
				yield return projectTotal;
			}
			Project.Cache.IsDirty = false;
		}

		protected string budgetRecordsKey;
		protected Dictionary<BudgetKeyTuple, PMBudgetRecord> budgetRecords;

		private string ImportFromExcelActionName => string.Format("{0}$ImportAction", nameof(Details));

		private string GetRecordID(BudgetKeyTuple key) => string.Format("{0}.{1}.{2}.{3}.{4}", key.ProjectID, key.ProjectTaskID, key.AccountGroupID, key.CostCodeID, key.InventoryID);

		public CostProjectionEntry()
		{
			CopyPaste.SetVisible(false);
		}

		public PXAction<PMCostProjection> createRevision;
		[PXUIField(DisplayName = "Copy Revision", MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update)]
		[PXButton]
		protected virtual IEnumerable CreateRevision(PXAdapter adapter)
		{
			if (Document.Current != null)
				VerifyAndRaiseExceptionIfBudgetIncompatible(Document.Current.ClassID, true);

			Save.Press();

			if (CopyDialog.View.Answer == WebDialogResult.None)
			{
				CopyDialog.Cache.Clear();
				PMCostProjectionCopyDialogInfo filterdata = CopyDialog.Cache.Insert() as PMCostProjectionCopyDialogInfo;
			}

			if (CopyDialog.AskExt() != WebDialogResult.OK || string.IsNullOrEmpty(CopyDialog.Current.RevisionID))
			{
				return adapter.Get();
			}

			if (Document.Current != null)
			{
				CreateNewProjection(Document.Current, CopyDialog.Current);
			}

			return adapter.Get();
		}

		protected virtual void CreateNewProjection(PMCostProjection original, PMCostProjectionCopyDialogInfo info)
		{
			PMCostProjection newDoc = new PMCostProjection();
			newDoc.ProjectID = original.ProjectID;
			newDoc.ClassID = original.ClassID;
			newDoc.Description = original.Description;
			newDoc.RevisionID = info.RevisionID;

			CostProjectionEntry target = PXGraph.CreateInstance<CostProjectionEntry>();
			target.Clear();
			target.SelectTimeStamp();
			newDoc = target.Document.Insert(newDoc);

			if (info.CopyNotes == true)
			{
				string note = PXNoteAttribute.GetNote(Document.Cache, original);
				PXNoteAttribute.SetNote(target.Document.Cache, newDoc, note);
			}

			if (info.CopyFiles == true)
			{
				Guid[] files = PXNoteAttribute.GetFileNotes(Document.Cache, original);
				PXNoteAttribute.SetFileNotes(target.Document.Cache, newDoc, files);
			}

			foreach (PMCostProjectionLine line in Details.Select())
			{
				if (GetBudgetRecord(line) != null)
				{
					PMCostProjectionLine copy = (PMCostProjectionLine)Details.Cache.CreateCopy(line);
					copy.RevisionID = newDoc.RevisionID;
					copy.NoteID = null;
					copy.Mode = ProjectionMode.Manual;

					copy = target.Details.Insert(copy);
					copy.Mode = line.Mode;

					if (info.CopyNotes == true)
					{
						string note = PXNoteAttribute.GetNote(Details.Cache, line);
						PXNoteAttribute.SetNote(target.Details.Cache, copy, note);
					}

					if (info.CopyFiles == true)
					{
						Guid[] files = PXNoteAttribute.GetFileNotes(Details.Cache, line);
						PXNoteAttribute.SetFileNotes(target.Details.Cache, copy, files);
					}
				}
			}

			if (info.RefreshBudget == true)
				target.refreshBudget.Press();
			target.Save.Press();

			PXRedirectHelper.TryRedirect(target, PXRedirectHelper.WindowMode.Same);
		}

		public PXAction<PMCostProjection> viewCostCommitments;
		[PXUIField(DisplayName = Messages.ViewCommitments, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton(ImageKey = PX.Web.UI.Sprite.Main.DataEntry, DisplayOnMainToolbar = false)]
		public IEnumerable ViewCostCommitments(PXAdapter adapter)
		{
			if (Details.Current != null)
			{
				CommitmentInquiry graph = PXGraph.CreateInstance<CommitmentInquiry>();
				graph.Filter.Current.AccountGroupID = Details.Current.AccountGroupID;
				graph.Filter.Current.ProjectID = Details.Current.ProjectID;
				graph.Filter.Current.ProjectTaskID = Details.Current.TaskID;
				graph.Filter.Current.CostCode = Details.Current.CostCodeID;
				graph.Filter.Current.InventoryID = Details.Current.InventoryID;
				throw new PXPopupRedirectException(graph, Messages.CommitmentEntry + " - " + Messages.ViewCommitments, true);
			}
			return adapter.Get();
		}

		public PXAction<PMCostProjection> viewCostTransactions;
		[PXUIField(DisplayName = Messages.ViewTransactions, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton(ImageKey = PX.Web.UI.Sprite.Main.Inquiry, DisplayOnMainToolbar = false)]
		public virtual IEnumerable ViewCostTransactions(PXAdapter adapter)
		{
			if (Details.Current != null)
			{
				TransactionInquiry target = PXGraph.CreateInstance<TransactionInquiry>();
				target.Filter.Insert(new TransactionInquiry.TranFilter());
				target.Filter.Current.ProjectID = Details.Current.ProjectID;
				target.Filter.Current.AccountGroupID = Details.Current.AccountGroupID;
				target.Filter.Current.ProjectTaskID = Details.Current.TaskID;
				target.Filter.Current.IncludeUnreleased = false;
				if (Details.Current.InventoryID != PMInventorySelectorAttribute.EmptyInventoryID)
					target.Filter.Current.InventoryID = Details.Current.InventoryID;
				if (Details.Current.CostCodeID != CostCodeAttribute.DefaultCostCode)
					target.Filter.Current.CostCode = Details.Current.CostCodeID;
				target.Filter.Current.DateTo = Document.Current.Date;
				throw new PXPopupRedirectException(target, Messages.TransactionInquiry + " - " + Messages.ViewTransactions, true);
			}
			return adapter.Get();
		}

		protected virtual void OverrideProjectedProperties(PMCostProjectionLine target, IDictionary<BudgetKeyTuple, PMCostBudget> budget)
		{
			if (target == null)
			{
				throw new ArgumentNullException(nameof(target));
			}

			if (budget == null)
			{
				throw new ArgumentNullException(nameof(budget));
			}

			if (budget.Count == 0)
			{
				return;
			}

			if (budget.TryGetValue(GetBudgetKey(target), out var source))
			{
				OverrideProjectedProperties(source, target);
			}
		}

		protected virtual void OverrideProjectedProperties(PMBudget source, PMCostProjectionLine target)
		{
			if (source == null)
			{
				throw new ArgumentNullException(nameof(source));
			}

			if (target == null)
			{
				throw new ArgumentNullException(nameof(target));
			}

			target.Amount = source.CuryCostProjectionCostToComplete;
			target.Quantity = source.CostProjectionQtyToComplete;
			target.ProjectedAmount = source.CuryCostProjectionCostAtCompletion;
			target.ProjectedQuantity = source.CostProjectionQtyAtCompletion;

			Details.Update(target);
		}

		protected virtual HashSet<BudgetKeyTuple> GetReleasedCostProjectionLineTuples()
		{
			// Get all lines of released projections with current projectId and classId
			var releasedCostProjectionLines = new PXSelectJoin<PMCostProjectionLine,
						InnerJoin<PMCostProjection,
							On<PMCostProjectionLine.projectID, Equal<PMCostProjection.projectID>,
							And<PMCostProjectionLine.revisionID, Equal<PMCostProjection.revisionID>>>>,
						Where<PMCostProjection.projectID, Equal<Current<PMCostProjection.projectID>>,
							And<PMCostProjection.classID, Equal<Current<PMCostProjection.classID>>,
							And<PMCostProjection.released, Equal<True>>>>>(this);

			// Build hashset of released budget keys
			return releasedCostProjectionLines.Select()
				.RowCast<PMCostProjectionLine>()
				.Select(GetBudgetKey)
				.Distinct()
				.ToHashSet();
		}

		protected virtual IDictionary<BudgetKeyTuple, PMCostBudget> BuildReleasedCostBudgetDictionary()
		{
			var releasedLinesHashSet = GetReleasedCostProjectionLineTuples();

			// Build dictionary of released budget rows (intersection with released projection lines on budget key)
			var budgetDictionary = CostBudget.Select()
				.RowCast<PMCostBudget>()
				.Select(budget => new KeyValuePair<BudgetKeyTuple, PMCostBudget>(BudgetKeyTuple.Create(budget), budget))
				.Where(pair => releasedLinesHashSet.Contains(pair.Key))
				.ToDictionary(key => key.Key, value => value.Value);

			return budgetDictionary;
		}

		protected Dictionary<BudgetKeyTuple, PMCostProjectionLine> GetProjectionLines()
		{
			Dictionary<BudgetKeyTuple, PMCostProjectionLine> res = new Dictionary<BudgetKeyTuple, PMCostProjectionLine>();
			foreach (PMCostProjectionLine line in Details.Select())
			{
				if (!res.ContainsKey(GetBudgetKey(line)))
					res.Add(GetBudgetKey(line), line);
				else
				{
					PXTrace.WriteError("Projection lines contain duplicates for the same Budget key.");
				}
			}
			return res;
		}

		public PXSelect<PMBudgetRecord> AvailableCostBudget;
		public virtual IEnumerable availableCostBudget()
		{
			var existing = GetProjectionLines();
			bool found = false;
			foreach (PMBudgetRecord item in AvailableCostBudget.Cache.Inserted)
			{
				found = true;
				yield return item;
			}

			if (found)
				yield break;

			if (Document.Current != null)
			{
				foreach (PMBudgetRecord item in GetCostBudget(Document.Current).Values)
				{
					item.Selected = existing.ContainsKey(GetBudgetKey(item));
					if (AvailableCostBudget.Locate(item) == null)
						yield return AvailableCostBudget.Insert(item);
				}
			}

			AvailableCostBudget.Cache.IsDirty = false;
		}

		public PXAction<PMCostProjection> addCostBudget;
		[PXUIField(DisplayName = "Add Budget Lines")]
		[PXButton]
		public IEnumerable AddCostBudget(PXAdapter adapter)
		{
			if (AvailableCostBudget.View.AskExt() == WebDialogResult.OK)
			{
				AddSelectedCostBudget();
			}
			return adapter.Get();
		}

		public PXAction<PMCostProjection> appendSelectedCostBudget;
		[PXUIField(DisplayName = "Add Lines")]
		[PXButton]
		public IEnumerable AppendSelectedCostBudget(PXAdapter adapter)
		{
			AddSelectedCostBudget();
			return adapter.Get();
		}

		public virtual void AddSelectedCostBudget()
		{
			var lines = GetProjectionLines();
			foreach (PMBudgetRecord budget in AvailableCostBudget.Cache.Cached)
			{
				if (budget.Selected != true) continue;

				if (!lines.ContainsKey(GetBudgetKey(budget)))
				{
					Details.Insert(new PMCostProjectionLine() { ProjectID = budget.ProjectID, TaskID = budget.ProjectTaskID, AccountGroupID = budget.AccountGroupID, InventoryID = budget.InventoryID, CostCodeID = budget.CostCodeID });
				}
			}
		}

		public PXAction<PMCostProjection> showHistory;
		[PXUIField(DisplayName = "History")]
		[PXButton]
		public IEnumerable ShowHistory(PXAdapter adapter)
		{
			History.View.AskExt();
			return adapter.Get();
		}

		public PXAction<PMCostProjection> refreshBudget;
		[PXUIField(DisplayName = "Refresh and Recalculate")]
		[PXButton]
		public virtual IEnumerable RefreshBudget(PXAdapter adapter)
		{
			var releasedCostBudget = BuildReleasedCostBudgetDictionary();

			foreach (PMCostProjectionLine line in Details.Select())
			{
				PMBudgetRecord budget = GetBudgetRecord(line);
				if (budget != null)
				{
					InitFieldsFromBudget(line, budget);
					if (line.Mode != ProjectionMode.Manual)
					{
						line.CompletedPct = GetCompletedPct(budget, line.Mode);
						RecalculateFromCompletedPct(line);
					}

					Details.Cache.MarkUpdated(line);
					var updatedLine = Details.Update(line);

					OverrideProjectedProperties(updatedLine, releasedCostBudget);
				}
				else
				{
					Details.Delete(line);
				}
			}

			Document.Update(Document.Current);
			Save.Press();

			return adapter.Get();
		}

		public PXAction<PMCostProjection> release;
		[PXUIField(DisplayName = "Release")]
		[PXButton]
		public virtual IEnumerable Release(PXAdapter adapter)
		{
			if (Document.Current != null)
			{
				PMCostProjectionClass projectionClass = Class.Select();
				if (projectionClass != null)
				{
					VerifyAndRaiseExceptionIfBudgetIncompatible(projectionClass, ()
						=> throw new PXException(Messages.CostProjectionCantBeReleased, projectionClass.Description), false);

					if (projectionClass.AccountGroupID != true)
					{
						throw new PXException(Messages.Incompetable);
					}
				}

				var lines = GetProjectionLines();
				foreach (PMCostBudget costBudget in CostBudget.Select())
				{
					if (lines.TryGetValue(BudgetKeyTuple.Create(costBudget), out PMCostProjectionLine line))
					{
						costBudget.CostProjectionCompletedPct = line.CompletedPct;
						costBudget.CostProjectionQtyToComplete = line.Quantity;
						costBudget.CostProjectionQtyAtCompletion = line.ProjectedQuantity;
						costBudget.CuryCostProjectionCostToComplete = line.Amount;
						costBudget.CuryCostProjectionCostAtCompletion = line.ProjectedAmount;
						CostBudget.Update(costBudget);
					}
				}

				Document.Current.Released = true;
				Document.Update(Document.Current);
				Save.Press();
			}
			return adapter.Get();
		}

		public PXAction<PMCostProjection> hold;
		[PXButton(CommitChanges = true), PXUIField(DisplayName = "Hold")]
		protected virtual IEnumerable Hold(PXAdapter adapter) => adapter.Get();

		public PXAction<PMCostProjection> removeHold;
		[PXButton(CommitChanges = true), PXUIField(DisplayName = "Remove Hold")]
		protected virtual IEnumerable RemoveHold(PXAdapter adapter) => adapter.Get();

		public PXAction<PMCostProjection> costProjectionReport;
		[PXUIField(DisplayName = "Project WIP Detail", MapEnableRights = PXCacheRights.Select)]
		[PXButton(SpecialType = PXSpecialButtonType.Report)]
		protected virtual IEnumerable CostProjectionReport(PXAdapter adapter)
		{
			if (Document.Current != null)
			{
				var parameters = new Dictionary<string, string>();
				parameters["LvlOfDet"] = "A";
				parameters["ProjectID"] = PMProject.PK.Find(this, Document.Current.ProjectID).ContractCD;
				FinPeriod finPeriod = this.GetService<IFinPeriodRepository>().GetFinPeriodByDate(Accessinfo.BusinessDate, FinPeriod.organizationID.MasterValue);

				string periodParamValue = $"{finPeriod.PeriodNbr}{finPeriod.FinYear}";
				parameters["From_Date"] = periodParamValue;
				parameters["To_Date"] = periodParamValue;
				parameters["ActToPeriod"] = periodParamValue;
				parameters["PCostEst"] = "P";
				parameters["IncludePot"] = "False";
				throw new PXReportRequiredException(parameters, "PM652500", "PM652500");
			}
			return adapter.Get();
		}

		protected virtual void _(Events.RowSelected<PMCostProjection> e)
		{
			PMCostProjectionClass projectionClass = Class.Select();
			if (projectionClass != null)
			{
				SetVisibility<PMCostProjectionLine.accountGroupID>(Details.Cache, projectionClass.AccountGroupID);
				SetVisibility<PMBudgetRecord.accountGroupID>(AvailableCostBudget.Cache, projectionClass.AccountGroupID);
				SetVisibility<PMCostProjectionLine.taskID>(Details.Cache, projectionClass.TaskID);
				SetVisibility<PMBudgetRecord.projectTaskID>(AvailableCostBudget.Cache, projectionClass.TaskID);
				SetVisibility<PMCostProjectionLine.costCodeID>(Details.Cache, projectionClass.CostCodeID);
				SetVisibility<PMBudgetRecord.costCodeID>(AvailableCostBudget.Cache, projectionClass.CostCodeID);
				SetVisibility<PMCostProjectionLine.inventoryID>(Details.Cache, projectionClass.InventoryID);
				SetVisibility<PMBudgetRecord.inventoryID>(AvailableCostBudget.Cache, projectionClass.InventoryID);
			}

			bool isEditable = e.Row.Hold == true;
			Document.Cache.AllowDelete = isEditable;
			Details.Cache.AllowInsert = isEditable;
			Details.Cache.AllowUpdate = isEditable;
			Details.Cache.AllowDelete = isEditable;
			addCostBudget.SetEnabled(isEditable && !string.IsNullOrEmpty(e.Row.ClassID));
			if (Actions.Contains(ImportFromExcelActionName))
				Actions[ImportFromExcelActionName].SetEnabled(isEditable && !string.IsNullOrEmpty(e.Row.ClassID));

			PXUIFieldAttribute.SetEnabled<PMCostProjection.classID>(e.Cache, e.Row, isEditable && !string.IsNullOrEmpty(e.Row.RevisionID));
			PXUIFieldAttribute.SetEnabled<PMCostProjection.description>(e.Cache, e.Row, isEditable);
			PXUIFieldAttribute.SetEnabled<PMCostProjection.date>(e.Cache, e.Row, isEditable);
		}

		private void SetVisibility<Field>(PXCache cache, bool? value)
			 where Field : IBqlField
		{
			PXUIFieldAttribute.SetVisible<Field>(cache, null, value == true);
			PXUIFieldAttribute.SetVisibility<Field>(cache, null, value == true ? PXUIVisibility.Visible : PXUIVisibility.Invisible);
		}

		protected virtual void _(Events.RowSelected<PMCostProjectionLine> e)
		{
			if (e.Row != null)
			{
				PXUIFieldAttribute.SetEnabled<PMCostProjectionLine.taskID>(Details.Cache, e.Row, IsImportFromExcel);
				PXUIFieldAttribute.SetEnabled<PMCostProjectionLine.accountGroupID>(Details.Cache, e.Row, IsImportFromExcel);
				PXUIFieldAttribute.SetEnabled<PMCostProjectionLine.inventoryID>(Details.Cache, e.Row, IsImportFromExcel);
				PXUIFieldAttribute.SetEnabled<PMCostProjectionLine.costCodeID>(Details.Cache, e.Row, IsImportFromExcel);
				PXUIFieldAttribute.SetEnabled<PMCostProjectionLine.quantity>(Details.Cache, e.Row, !string.IsNullOrEmpty(e.Row.UOM));
				PXUIFieldAttribute.SetEnabled<PMCostProjectionLine.projectedQuantity>(Details.Cache, e.Row, !string.IsNullOrEmpty(e.Row.UOM));
			}
		}

		protected virtual void _(Events.FieldVerifying<PMCostProjection, PMCostProjection.classID> e)
		{
			if ((string)e.NewValue != e.Row.ClassID)
			{
				VerifyAndRaiseExceptionIfRowsExists();
			}
			VerifyAndRaiseExceptionIfBudgetIncompatible((string)e.NewValue, true);
		}

		protected virtual void _(Events.RowUpdated<PMCostProjection> e)
		{
			if (e.Row.ClassID != e.OldRow.ClassID)
			{
				AvailableCostBudget.Cache.Clear();
			}
		}

		protected virtual void _(Events.RowPersisting<PMBudgetRecord> e)
		{
			e.Cancel = true;
		}

		protected virtual void _(Events.FieldUpdated<PMCostProjectionLine, PMCostProjectionLine.quantity> e)
		{
			if (e.Row.Mode == ProjectionMode.Manual || e.Row.Mode == ProjectionMode.ManualQuantity)
				return;

			e.Row.ProjectedQuantity = e.Row.Quantity.GetValueOrDefault() + e.Row.CompletedQuantity.GetValueOrDefault();
			e.Row.VarianceQuantity = e.Row.ProjectedQuantity.GetValueOrDefault() - e.Row.BudgetedQuantity.GetValueOrDefault();
			if (e.Row.ProjectedQuantity.GetValueOrDefault() != 0)
			{
				decimal completedPct = e.Row.CompletedQuantity.GetValueOrDefault() * 100m / e.Row.ProjectedQuantity.GetValueOrDefault();
				e.Row.CompletedPct = PXDBQuantityAttribute.Round(completedPct);
				if (e.Row.Mode != ProjectionMode.ManualCost)
				{
					if (completedPct != 0)
					{
						e.Row.ProjectedAmount = Math.Round(e.Row.CompletedAmount.GetValueOrDefault() * 100 / completedPct, 2);
					}
					else
					{
						e.Row.ProjectedAmount = Math.Max(e.Row.BudgetedAmount.GetValueOrDefault(), e.Row.CompletedAmount.GetValueOrDefault());
					}
					e.Row.Amount = Math.Max(0, e.Row.ProjectedAmount.GetValueOrDefault() - e.Row.CompletedAmount.GetValueOrDefault());
					e.Row.VarianceAmount = e.Row.ProjectedAmount.GetValueOrDefault() - e.Row.BudgetedAmount.GetValueOrDefault();
				}
			}
		}

		protected virtual void _(Events.FieldUpdated<PMCostProjectionLine, PMCostProjectionLine.projectedQuantity> e)
		{
			if (e.Row.Mode == ProjectionMode.Manual || e.Row.Mode == ProjectionMode.ManualQuantity)
				return;

			e.Row.Quantity = Math.Max(0, e.Row.ProjectedQuantity.GetValueOrDefault() - e.Row.CompletedQuantity.GetValueOrDefault());
			e.Row.VarianceQuantity = e.Row.ProjectedQuantity.GetValueOrDefault() - e.Row.BudgetedQuantity.GetValueOrDefault();
			if (e.Row.ProjectedQuantity.GetValueOrDefault() != 0)
			{
				decimal completedPct = e.Row.CompletedQuantity.GetValueOrDefault() * 100m / e.Row.ProjectedQuantity.GetValueOrDefault();
				e.Row.CompletedPct = PXDBQuantityAttribute.Round(completedPct);
				if (e.Row.Mode != ProjectionMode.ManualCost)
				{
					if (completedPct != 0)
					{
						e.Row.ProjectedAmount = Math.Max(Math.Round(e.Row.CompletedAmount.GetValueOrDefault() * 100m / completedPct), e.Row.CompletedAmount.GetValueOrDefault());
					}
					else
					{
						e.Row.ProjectedAmount = Math.Max(e.Row.BudgetedAmount.GetValueOrDefault(), e.Row.CompletedAmount.GetValueOrDefault());
					}
					e.Row.Amount = Math.Max(0, e.Row.ProjectedAmount.GetValueOrDefault() - e.Row.CompletedAmount.GetValueOrDefault());
					e.Row.VarianceAmount = e.Row.ProjectedAmount.GetValueOrDefault() - e.Row.BudgetedAmount.GetValueOrDefault();
				}
			}
		}

		protected virtual void _(Events.FieldUpdated<PMCostProjectionLine, PMCostProjectionLine.varianceQuantity> e)
		{
			if (e.Row.Mode == ProjectionMode.Manual || e.Row.Mode == ProjectionMode.ManualQuantity)
				return;

			e.Row.ProjectedQuantity = e.Row.BudgetedQuantity.GetValueOrDefault() + e.Row.VarianceQuantity.GetValueOrDefault();
			e.Row.Quantity = Math.Max(0, e.Row.ProjectedQuantity.GetValueOrDefault() - e.Row.CompletedQuantity.GetValueOrDefault());
			if (e.Row.ProjectedQuantity.GetValueOrDefault() != 0)
			{
				decimal completedPct = e.Row.CompletedQuantity.GetValueOrDefault() * 100m / e.Row.ProjectedQuantity.GetValueOrDefault();
				e.Row.CompletedPct = PXDBQuantityAttribute.Round(completedPct);
				if (e.Row.Mode != ProjectionMode.ManualCost)
				{
					if (completedPct != 0)
					{
						e.Row.ProjectedAmount = Math.Max(Math.Round(e.Row.CompletedAmount.GetValueOrDefault() * 100m / completedPct), e.Row.CompletedAmount.GetValueOrDefault());
					}
					else
					{
						e.Row.ProjectedAmount = Math.Max(e.Row.BudgetedAmount.GetValueOrDefault(), e.Row.CompletedAmount.GetValueOrDefault());
					}
					e.Row.Amount = Math.Max(0, e.Row.ProjectedAmount.GetValueOrDefault() - e.Row.CompletedAmount.GetValueOrDefault());
					e.Row.VarianceAmount = e.Row.ProjectedAmount.GetValueOrDefault() - e.Row.BudgetedAmount.GetValueOrDefault();
				}
			}
		}

		protected virtual void _(Events.FieldUpdated<PMCostProjectionLine, PMCostProjectionLine.amount> e)
		{
			if (e.Row.Mode == ProjectionMode.Manual || e.Row.Mode == ProjectionMode.ManualCost)
				return;

			e.Row.ProjectedAmount = e.Row.Amount.GetValueOrDefault() + e.Row.CompletedAmount.GetValueOrDefault();
			e.Row.VarianceAmount = e.Row.ProjectedAmount.GetValueOrDefault() - e.Row.BudgetedAmount.GetValueOrDefault();
			if (e.Row.ProjectedAmount.GetValueOrDefault() != 0)
			{
				decimal completedPct = e.Row.CompletedAmount.GetValueOrDefault() * 100m / e.Row.ProjectedAmount.GetValueOrDefault();
				e.Row.CompletedPct = PXDBQuantityAttribute.Round(completedPct);
				if (e.Row.Mode != ProjectionMode.ManualQuantity)
				{
					if (completedPct != 0)
					{
						e.Row.ProjectedQuantity = PXDBQuantityAttribute.Round(e.Row.CompletedQuantity.GetValueOrDefault() * 100m / completedPct);
					}
					else
					{
						e.Row.ProjectedQuantity = Math.Max(e.Row.BudgetedQuantity.GetValueOrDefault(), e.Row.CompletedQuantity.GetValueOrDefault());
					}
					e.Row.Quantity = Math.Max(0, e.Row.ProjectedQuantity.GetValueOrDefault() - e.Row.CompletedQuantity.GetValueOrDefault());
					e.Row.VarianceQuantity = e.Row.ProjectedQuantity.GetValueOrDefault() - e.Row.BudgetedQuantity.GetValueOrDefault();
				}
			}
		}

		protected virtual void _(Events.FieldUpdated<PMCostProjectionLine, PMCostProjectionLine.projectedAmount> e)
		{
			if (e.Row.Mode == ProjectionMode.Manual || e.Row.Mode == ProjectionMode.ManualCost)
				return;

			e.Row.Amount = Math.Max(0, e.Row.ProjectedAmount.GetValueOrDefault() - e.Row.CompletedAmount.GetValueOrDefault());
			e.Row.VarianceAmount = e.Row.ProjectedAmount.GetValueOrDefault() - e.Row.BudgetedAmount.GetValueOrDefault();
			if (e.Row.ProjectedAmount.GetValueOrDefault() != 0)
			{
				decimal completedPct = e.Row.CompletedAmount.GetValueOrDefault() * 100m / e.Row.ProjectedAmount.GetValueOrDefault();
				e.Row.CompletedPct = PXDBQuantityAttribute.Round(completedPct);
				if (e.Row.Mode != ProjectionMode.ManualQuantity)
				{
					if (completedPct != 0)
					{
						e.Row.ProjectedQuantity = PXDBQuantityAttribute.Round(e.Row.CompletedQuantity.GetValueOrDefault() * 100m / completedPct);
					}
					else
					{
						e.Row.ProjectedQuantity = Math.Max(e.Row.BudgetedQuantity.GetValueOrDefault(), e.Row.CompletedQuantity.GetValueOrDefault());
					}
					e.Row.Quantity = Math.Max(0, e.Row.ProjectedQuantity.GetValueOrDefault() - e.Row.CompletedQuantity.GetValueOrDefault());
					e.Row.VarianceQuantity = e.Row.ProjectedQuantity.GetValueOrDefault() - e.Row.BudgetedQuantity.GetValueOrDefault();
				}
			}
		}

		protected virtual void _(Events.FieldUpdated<PMCostProjectionLine, PMCostProjectionLine.varianceAmount> e)
		{
			if (e.Row.Mode == ProjectionMode.Manual || e.Row.Mode == ProjectionMode.ManualCost)
				return;

			e.Row.ProjectedAmount = e.Row.BudgetedAmount.GetValueOrDefault() + e.Row.VarianceAmount.GetValueOrDefault();
			e.Row.Amount = Math.Max(0, e.Row.ProjectedAmount.GetValueOrDefault() - e.Row.CompletedAmount.GetValueOrDefault());
			if (e.Row.ProjectedAmount.GetValueOrDefault() != 0)
			{
				decimal completedPct = e.Row.CompletedAmount.GetValueOrDefault() * 100m / e.Row.ProjectedAmount.GetValueOrDefault();
				e.Row.CompletedPct = PXDBQuantityAttribute.Round(completedPct);
				if (e.Row.Mode != ProjectionMode.ManualQuantity)
				{
					if (completedPct != 0)
					{
						e.Row.ProjectedQuantity = PXDBQuantityAttribute.Round(e.Row.CompletedQuantity.GetValueOrDefault() * 100m / completedPct);
					}
					else
					{
						e.Row.ProjectedQuantity = Math.Max(e.Row.BudgetedQuantity.GetValueOrDefault(), e.Row.CompletedQuantity.GetValueOrDefault());
					}
					e.Row.Quantity = Math.Max(0, e.Row.ProjectedQuantity.GetValueOrDefault() - e.Row.CompletedQuantity.GetValueOrDefault());
					e.Row.VarianceQuantity = e.Row.ProjectedQuantity.GetValueOrDefault() - e.Row.BudgetedQuantity.GetValueOrDefault();
				}
			}
		}

		protected virtual void _(Events.FieldUpdated<PMCostProjectionLine, PMCostProjectionLine.completedPct> e)
		{
			if (e.Row.Mode != ProjectionMode.Manual)
			{
				RecalculateFromCompletedPct(e.Row);
			}
		}

		protected virtual void _(Events.FieldUpdated<PMCostProjectionLine, PMCostProjectionLine.mode> e)
		{
			if (e.Row.Mode != ProjectionMode.Manual)
			{
				//reset percentage:
				e.Row.CompletedPct = GetCompletedPct(e.Row);
				RecalculateFromCompletedPct(e.Row);
			}
		}

		protected virtual void _(Events.RowInserting<PMCostProjectionLine> e)
		{
			if (e.Row.Mode != ProjectionMode.Manual || IsImportFromExcel)
			{
				PMBudgetRecord budget = GetBudgetRecord(e.Row);
				if (budget != null)
				{
					if (e.Row.Mode != ProjectionMode.Manual)
						e.Row.CompletedPct = GetCompletedPct(budget, e.Row.Mode);

					e.Row.UOM = budget.UOM;
					e.Row.Description = budget.Description;
					InitFieldsFromBudget(e.Row, budget);
				}
			}
			Document.Update(Document.Current);
		}

		protected virtual void _(Events.FieldVerifying<PMCostProjectionCopyDialogInfo, PMCostProjectionCopyDialogInfo.revisionID> e)
		{
			var select = new PXSelect<PMCostProjection, Where<PMCostProjection.projectID, Equal<Current<PMCostProjection.projectID>>,
				And<PMCostProjection.revisionID, Equal<Required<PMCostProjection.revisionID>>>>>(this);

			PMCostProjection duplicate = select.Select(e.NewValue);
			if (duplicate != null)
			{
				throw new PXSetPropertyException<PMCostProjection.revisionID>(Messages.CostProjectionDuplicateID);
			}
		}

		protected virtual void InitFieldsFromBudget(PMCostProjectionLine row, PMBudgetRecord budget)
		{
			if (budget != null)
			{
				row.BudgetedQuantity = budget.RevisedQty;
				row.BudgetedAmount = budget.CuryRevisedAmount;
				row.ActualQuantity = budget.ActualQty;
				row.ActualAmount = budget.CuryActualAmount;
				row.UnbilledQuantity = budget.CommittedOpenQty;
				row.UnbilledAmount = budget.CuryCommittedOpenAmount;
				if (row.Mode != ProjectionMode.Manual && row.Mode != ProjectionMode.ManualCost)
				{
					row.ProjectedAmount = Math.Max(budget.CuryRevisedAmount.GetValueOrDefault(), row.CompletedAmount.GetValueOrDefault());
					row.Amount = Math.Max(0, budget.CuryRevisedAmount.GetValueOrDefault() - row.CompletedAmount.GetValueOrDefault());
				}

				if (row.Mode != ProjectionMode.Manual && row.Mode != ProjectionMode.ManualQuantity)
				{
					row.ProjectedQuantity = Math.Max(budget.RevisedQty.GetValueOrDefault(), row.CompletedQuantity.GetValueOrDefault());
					row.Quantity = Math.Max(0, budget.RevisedQty.GetValueOrDefault() - row.CompletedQuantity.GetValueOrDefault());
				}
			}
		}

		protected virtual void RecalculateFromCompletedPct(PMCostProjectionLine row)
		{
			decimal completedPctBase = GetCompletedPct(row);
			if (completedPctBase != row.CompletedPct && row.CompletedPct.GetValueOrDefault() != 0)
			{
				decimal newRevisedAmount;
				decimal newRevisedQty;
				if (completedPctBase != 0)
				{
					newRevisedAmount = Math.Round(row.BudgetedAmount.GetValueOrDefault() * completedPctBase / row.CompletedPct.GetValueOrDefault(), 2);
					newRevisedQty = PXDBQuantityAttribute.Round(row.BudgetedQuantity.GetValueOrDefault() * completedPctBase / row.CompletedPct.GetValueOrDefault());
				}
				else
				{
					newRevisedAmount = row.BudgetedAmount.GetValueOrDefault();
					newRevisedQty = row.BudgetedQuantity.GetValueOrDefault();
				}

				if (row.Mode != ProjectionMode.ManualQuantity)
				{
					row.ProjectedQuantity = Math.Max(newRevisedQty, row.CompletedQuantity.GetValueOrDefault());
					row.Quantity = Math.Max(0, row.ProjectedQuantity.GetValueOrDefault() - row.CompletedQuantity.GetValueOrDefault());
					row.VarianceQuantity = row.ProjectedQuantity.GetValueOrDefault() - row.BudgetedQuantity.GetValueOrDefault();
				}

				if (row.Mode != ProjectionMode.ManualCost)
				{
					row.ProjectedAmount = Math.Max(newRevisedAmount, row.CompletedAmount.GetValueOrDefault());
					row.Amount = Math.Max(0, row.ProjectedAmount.GetValueOrDefault() - row.CompletedAmount.GetValueOrDefault());
					row.VarianceAmount = row.ProjectedAmount.GetValueOrDefault() - row.BudgetedAmount.GetValueOrDefault();
				}
			}
			else
			{
				if (row.Mode != ProjectionMode.ManualQuantity)
				{
					row.ProjectedQuantity = Math.Max(row.BudgetedQuantity.GetValueOrDefault(), row.CompletedQuantity.GetValueOrDefault());
					row.Quantity = Math.Max(0, row.BudgetedQuantity.GetValueOrDefault() - row.CompletedQuantity.GetValueOrDefault());
					row.VarianceQuantity = row.ProjectedQuantity.GetValueOrDefault() - row.BudgetedQuantity.GetValueOrDefault();
				}
				if (row.Mode != ProjectionMode.ManualCost)
				{
					row.ProjectedAmount = Math.Max(row.BudgetedAmount.GetValueOrDefault(), row.CompletedAmount.GetValueOrDefault());
					row.Amount = Math.Max(0, row.BudgetedAmount.GetValueOrDefault() - row.CompletedAmount.GetValueOrDefault());
					row.VarianceAmount = row.ProjectedAmount.GetValueOrDefault() - row.BudgetedAmount.GetValueOrDefault();
				}
			}
		}

		private void VerifyAndRaiseExceptionIfRowsExists()
		{
			if (Details.Select().Count > 0)
				throw new PXSetPropertyException(Messages.ValueIsDisabled);
		}

		private void VerifyAndRaiseExceptionIfBudgetIncompatible(string classID, bool classCanBeLessDetailed)
		{
			PMCostProjectionClass projectionClass = PMCostProjectionClass.PK.Find(this, classID);

			if (Document.Current != null && projectionClass != null)
				VerifyAndRaiseExceptionIfBudgetIncompatible(projectionClass, () => throw new PXSetPropertyException(Messages.IncompetableClass), classCanBeLessDetailed);
		}

		private void VerifyAndRaiseExceptionIfBudgetIncompatible(PMCostProjectionClass projectionClass, Action throwExceptionMethod, bool classCanBeLessDetailed)
		{
			if (projectionClass == null)
			{
				throw new ArgumentNullException(nameof(projectionClass));
			}

			if (throwExceptionMethod == null)
			{
				throw new ArgumentNullException(nameof(throwExceptionMethod));
			}

			PMProject project = Project.Select();

			if (project != null)
			{
				if (project.CostBudgetLevel == BudgetLevels.Task)
				{
					if (projectionClass.TaskID != true && !classCanBeLessDetailed)
						throwExceptionMethod();

					if (projectionClass.InventoryID == true)
						throwExceptionMethod();

					if (projectionClass.CostCodeID == true)
						throwExceptionMethod();
				}

				else if (project.CostBudgetLevel == BudgetLevels.Item)
				{
					if (projectionClass.TaskID != true && !classCanBeLessDetailed)
						throwExceptionMethod();

					if (projectionClass.InventoryID != true && !classCanBeLessDetailed)
						throwExceptionMethod();

					if (projectionClass.CostCodeID == true)
						throwExceptionMethod();
				}

				else if (project.CostBudgetLevel == BudgetLevels.CostCode)
				{
					if (projectionClass.TaskID != true && !classCanBeLessDetailed)
						throwExceptionMethod();

					if (projectionClass.CostCodeID != true && !classCanBeLessDetailed)
						throwExceptionMethod();

					if (projectionClass.InventoryID == true)
						throwExceptionMethod();
				}

				else if (project.CostBudgetLevel == BudgetLevels.Detail && !classCanBeLessDetailed)
				{
					if (projectionClass.TaskID != true)
						throwExceptionMethod();

					if (projectionClass.CostCodeID != true)
						throwExceptionMethod();

					if (projectionClass.InventoryID != true)
						throwExceptionMethod();
				}
			}
		}

		protected virtual Dictionary<BudgetKeyTuple, PMBudgetRecord> GetCostBudget(PMCostProjection costProjection)
		{
			string classkey = string.Format("{0}.{1}", costProjection.ProjectID, costProjection.ClassID);
			if (classkey != budgetRecordsKey)
			{
				budgetRecords = null;
			}

			if (budgetRecords == null)
			{
				PMCostProjectionClass projectionClass = Class.Select();
				budgetRecords = new Dictionary<BudgetKeyTuple, PMBudgetRecord>();
				budgetRecordsKey = classkey;
				var selectCostBudget = new PXSelectReadonly<PMBudget,
					Where<PMBudget.projectID, Equal<Current<PMCostProjection.projectID>>,
					And<PMBudget.type, Equal<AccountType.expense>>>>(this);

				foreach (PMBudget budget in selectCostBudget.Select())
				{
					int? accountGroupID = budget.AccountGroupID;
					int? taskID = budget.TaskID;
					int inventoryID = budget.InventoryID.Value;
					int costCodeID = budget.CostCodeID.Value;

					if (projectionClass != null)
					{
						if (projectionClass.AccountGroupID != true)
						{
							accountGroupID = null;
						}
						if (projectionClass.TaskID != true)
						{
							taskID = null;
						}
						if (projectionClass.CostCodeID != true)
						{
							costCodeID = CostCodeAttribute.DefaultCostCode.Value;
						}
						if (projectionClass.InventoryID != true)
						{
							inventoryID = PMInventorySelectorAttribute.EmptyInventoryID;
						}
					}

					BudgetKeyTuple key = new BudgetKeyTuple(budget.ProjectID.Value, taskID.GetValueOrDefault(), accountGroupID.GetValueOrDefault(), inventoryID, costCodeID);
					if (!budgetRecords.TryGetValue(key, out PMBudgetRecord existing))
					{
						existing = new PMBudgetRecord
						{
							RecordID = GetRecordID(key),
							ProjectID = budget.ProjectID,
							ProjectTaskID = taskID,
							AccountGroupID = accountGroupID,
							CostCodeID = costCodeID,
							InventoryID = inventoryID,
							UOM = budget.UOM,
							Description = budget.Description,
							CuryUnitRate = budget.CuryUnitRate
						};
						budgetRecords.Add(key, existing);
					}
					Add(existing, budget);
				}
			}
			return budgetRecords;
		}

		protected virtual PMBudgetRecord Add(PMBudgetRecord x, PMBudget y)
		{
			if (x.UOM == y.UOM)
			{
				x.ActualQty = x.ActualQty.GetValueOrDefault() + y.ActualQty.GetValueOrDefault();
				x.ChangeOrderQty = x.ChangeOrderQty.GetValueOrDefault() + y.ChangeOrderQty.GetValueOrDefault();
				x.CommittedInvoicedQty = x.CommittedInvoicedQty.GetValueOrDefault() + y.CommittedInvoicedQty.GetValueOrDefault();
				x.CommittedOpenQty = x.CommittedOpenQty.GetValueOrDefault() + y.CommittedOpenQty.GetValueOrDefault();
				x.CommittedOrigQty = x.CommittedOrigQty.GetValueOrDefault() + y.CommittedOrigQty.GetValueOrDefault();
				x.CommittedQty = x.CommittedQty.GetValueOrDefault() + y.CommittedQty.GetValueOrDefault();
				x.CommittedReceivedQty = x.CommittedReceivedQty.GetValueOrDefault() + y.CommittedReceivedQty.GetValueOrDefault();
				x.DraftChangeOrderQty = x.DraftChangeOrderQty.GetValueOrDefault() + y.DraftChangeOrderQty.GetValueOrDefault();
				x.Qty = x.Qty.GetValueOrDefault() + y.Qty.GetValueOrDefault();
				x.RevisedQty = x.RevisedQty.GetValueOrDefault() + y.RevisedQty.GetValueOrDefault();
			}
			else
			{
				x.UOM = null;
				x.ActualQty = 0;
				x.ChangeOrderQty = 0;
				x.CommittedInvoicedQty = 0;
				x.CommittedOpenQty = 0;
				x.CommittedOrigQty = 0;
				x.CommittedQty = 0;
				x.CommittedReceivedQty = 0;
				x.DraftChangeOrderQty = 0;
				x.Qty = 0;
				x.RevisedQty = 0;
			}

			if (x.Description != y.Description)
			{
				x.Description = null;
			}

			if (x.CuryUnitRate != y.CuryUnitRate)
			{
				x.CuryUnitRate = null;
			}

			x.Amount = x.Amount.GetValueOrDefault() + y.Amount.GetValueOrDefault();
			x.BaseActualAmount = x.BaseActualAmount.GetValueOrDefault() + y.BaseActualAmount.GetValueOrDefault();
			x.ChangeOrderAmount = x.ChangeOrderAmount.GetValueOrDefault() + y.ChangeOrderAmount.GetValueOrDefault();
			x.CommittedAmount = x.CommittedAmount.GetValueOrDefault() + y.CommittedAmount.GetValueOrDefault();
			x.CommittedInvoicedAmount = x.CommittedInvoicedAmount.GetValueOrDefault() + y.CommittedInvoicedAmount.GetValueOrDefault();
			x.CommittedOpenAmount = x.CommittedOpenAmount.GetValueOrDefault() + y.CommittedOpenAmount.GetValueOrDefault();
			x.CommittedOrigAmount = x.CommittedOrigAmount.GetValueOrDefault() + y.CommittedOrigAmount.GetValueOrDefault();
			x.CuryActualAmount = x.CuryActualAmount.GetValueOrDefault() + y.CuryActualAmount.GetValueOrDefault();
			x.CuryAmount = x.CuryAmount.GetValueOrDefault() + y.CuryAmount.GetValueOrDefault();
			x.CuryChangeOrderAmount = x.CuryChangeOrderAmount.GetValueOrDefault() + y.CuryChangeOrderAmount.GetValueOrDefault();
			x.CuryCommittedAmount = x.CuryCommittedAmount.GetValueOrDefault() + y.CuryCommittedAmount.GetValueOrDefault();
			x.CuryCommittedInvoicedAmount = x.CuryCommittedInvoicedAmount.GetValueOrDefault() + y.CuryCommittedInvoicedAmount.GetValueOrDefault();
			x.CuryCommittedOpenAmount = x.CuryCommittedOpenAmount.GetValueOrDefault() + y.CuryCommittedOpenAmount.GetValueOrDefault();
			x.CuryCommittedOrigAmount = x.CuryCommittedOrigAmount.GetValueOrDefault() + y.CuryCommittedOrigAmount.GetValueOrDefault();
			x.CuryDraftChangeOrderAmount = x.CuryDraftChangeOrderAmount.GetValueOrDefault() + y.CuryDraftChangeOrderAmount.GetValueOrDefault();
			x.CuryInvoicedAmount = x.CuryInvoicedAmount.GetValueOrDefault() + y.CuryInvoicedAmount.GetValueOrDefault();
			x.CuryRevisedAmount = x.CuryRevisedAmount.GetValueOrDefault() + y.CuryRevisedAmount.GetValueOrDefault();
			x.DraftChangeOrderAmount = x.DraftChangeOrderAmount.GetValueOrDefault() + y.DraftChangeOrderAmount.GetValueOrDefault();
			x.InvoicedAmount = x.InvoicedAmount.GetValueOrDefault() + y.InvoicedAmount.GetValueOrDefault();
			x.RevisedAmount = x.RevisedAmount.GetValueOrDefault() + y.RevisedAmount.GetValueOrDefault();
			return x;
		}

		private decimal GetCompletedPct(PMBudgetRecord row, string mode)
		{
			if (mode == ProjectionMode.ManualCost)
			{
				decimal totalActualQty = row.ActualQty.GetValueOrDefault() + row.CommittedOpenQty.GetValueOrDefault();
				if (row.RevisedQty.GetValueOrDefault() == 0)
				{
					return totalActualQty == 0 ? 0m : 100m;
				}
				else
					return PXDBQuantityAttribute.Round(100m * totalActualQty / row.RevisedQty.GetValueOrDefault());
			}
			else
			{
				decimal totalActualAmt = row.CuryActualAmount.GetValueOrDefault() + row.CuryCommittedOpenAmount.GetValueOrDefault();
				if (row.CuryRevisedAmount.GetValueOrDefault() == 0)
				{
					return totalActualAmt == 0 ? 0m : 100m;
				}
				else
					return PXDBQuantityAttribute.Round(100m * totalActualAmt / row.CuryRevisedAmount.GetValueOrDefault());
			}
		}

		private decimal GetCompletedPct(PMCostProjectionLine row)
		{
			if (row.Mode == ProjectionMode.ManualCost)
			{
				if (row.BudgetedQuantity.GetValueOrDefault() == 0)
				{
					return row.CompletedQuantity == 0 ? 0m : 100m;
				}
				else
					return PXDBQuantityAttribute.Round(100m * row.CompletedQuantity.GetValueOrDefault() / row.BudgetedQuantity.GetValueOrDefault());
			}
			else
			{
				if (row.BudgetedAmount.GetValueOrDefault() == 0)
				{
					return row.CompletedAmount == 0 ? 0m : 100m;
				}
				else
					return PXDBQuantityAttribute.Round(100m * row.CompletedAmount.GetValueOrDefault() / row.BudgetedAmount.GetValueOrDefault());
			}
		}

		private PMBudgetRecord GetBudgetRecord(PMCostProjectionLine row)
		{
			PMBudgetRecord result = null;
			if (Document.Current != null)
			{
				var costBudget = GetCostBudget(Document.Current);
				costBudget.TryGetValue(GetBudgetKey(row), out result);
			}
			return result;
		}

		private BudgetKeyTuple GetBudgetKey(PMBudgetRecord item)
		{
			return new BudgetKeyTuple(
				item.ProjectID.GetValueOrDefault(),
				item.ProjectTaskID.GetValueOrDefault(),
				item.AccountGroupID.GetValueOrDefault(),
				item.InventoryID.GetValueOrDefault(PMInventorySelectorAttribute.EmptyInventoryID),
				item.CostCodeID.GetValueOrDefault(CostCodeAttribute.GetDefaultCostCode()));
		}

		private BudgetKeyTuple GetBudgetKey(PMCostProjectionLine item)
		{
			return new BudgetKeyTuple(
				item.ProjectID.GetValueOrDefault(),
				item.TaskID.GetValueOrDefault(),
				item.AccountGroupID.GetValueOrDefault(),
				item.InventoryID.GetValueOrDefault(PMInventorySelectorAttribute.EmptyInventoryID),
				item.CostCodeID.GetValueOrDefault(CostCodeAttribute.GetDefaultCostCode()));
		}

		#region PMImport Implementation

		private Dictionary<BudgetKeyTuple, PMCostProjectionLine> lines = null;

		public bool PrepareImportRow(string viewName, IDictionary keys, IDictionary values)
		{
			if (lines == null)
			{
				lines = GetProjectionLines();
			}

			PMCostProjectionClass projectionClass = Class.Select();
			if (Document.Current != null && Document.Current.ProjectID != null && projectionClass != null)
			{
				int? accountGroupID = null;
				if (values.Contains(nameof(PMCostProjectionLine.AccountGroupID)))
				{
					string accountGroupCD = (string)values[nameof(PMCostProjectionLine.AccountGroupID)];
					PMAccountGroup accountGroup = PMAccountGroup.UK.Find(this, accountGroupCD);
					if (accountGroup != null)
					{
						accountGroupID = accountGroup.GroupID;
					}
					else
					{
						throw new PXException(Messages.InvalidAccountGroup, accountGroupCD);
					}
				}

				int? taskID = null;
				if (values.Contains(nameof(PMCostProjectionLine.TaskID)))
				{
					string taskCD = (string)values[nameof(PMCostProjectionLine.TaskID)];
					PMTask task = PMTask.UK.Find(this, Document.Current.ProjectID, taskCD);
					if (task != null)
					{
						taskID = task.TaskID;
					}
					else
					{
						throw new PXException(Messages.InvalidCostTask, taskCD);
					}
				}

				int? costCodeID = null;
				if (values.Contains(nameof(PMCostProjectionLine.CostCodeID)))
				{
					string costCodeCD = (string)values[nameof(PMCostProjectionLine.CostCodeID)];
					PMCostCode costCode = PMCostCode.UK.Find(this, costCodeCD);
					if (costCode != null)
					{
						costCodeID = costCode.CostCodeID;
					}
					else
					{
						throw new PXException(Messages.InvalidCostCode, costCodeCD);
					}
				}

				int? inventoryID = null;
				if (values.Contains(nameof(PMCostProjectionLine.InventoryID)))
				{
					string inventoryCD = (string)values[nameof(PMCostProjectionLine.InventoryID)];
					InventoryItem item = InventoryItem.UK.Find(this, inventoryCD);
					if (item != null)
					{
						inventoryID = item.InventoryID;
					}
					else
					{
						throw new PXException(Messages.InvalidInventoryID, inventoryCD);
					}
				}

				if (projectionClass.AccountGroupID == true && accountGroupID == null)
				{
					throw new PXException(Messages.MissingAccountGroup);
				}

				PMProject project = PMProject.PK.Find(this, Document.Current.ProjectID);
				if (projectionClass.TaskID == true && taskID == null)
				{
					throw new PXException(Messages.MissingTaskID);
				}

				if (projectionClass.CostCodeID == true && (project.CostBudgetLevel == BudgetLevels.CostCode
					|| project.CostBudgetLevel == BudgetLevels.Detail) && costCodeID == null)
				{
					throw new PXException(Messages.MissingCostCode);
				}

				if (projectionClass.InventoryID == true && (project.CostBudgetLevel == BudgetLevels.Item
					|| project.CostBudgetLevel == BudgetLevels.Detail) && inventoryID == null)
				{
					throw new PXException(Messages.MissingInventoryID);
				}

				if (costCodeID == null)
				{
					costCodeID = CostCodeAttribute.DefaultCostCode;
				}

				if (inventoryID == null)
				{
					inventoryID = PMInventorySelectorAttribute.EmptyInventoryID;
				}

				BudgetKeyTuple key = new BudgetKeyTuple(
					Document.Current.ProjectID.Value,
					taskID.GetValueOrDefault(),
					accountGroupID.GetValueOrDefault(),
					inventoryID.GetValueOrDefault(),
					costCodeID.GetValueOrDefault());

				PMCostProjectionLine line = null;
				if (lines.TryGetValue(key, out line))
				{
					if (keys.Contains(nameof(PMCostProjectionLine.LineNbr)))
					{
						keys[nameof(PMCostProjectionLine.LineNbr)] = line.LineNbr;
					}
				}
			}
			return true;
		}

		public bool RowImporting(string viewName, object row)
		{
			return true;
		}

		public bool RowImported(string viewName, object row, object oldRow)
		{
			return oldRow == null;
		}

		public void PrepareItems(string viewName, IEnumerable items) { }

		#endregion

		[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
		[PXHidden]
		public class PMCostProjectionCopyDialogInfo : IBqlTable
		{
			#region RevisionID
			public abstract class revisionID : PX.Data.BQL.BqlString.Field<revisionID> { }
			[PXDBString(30, InputMask = ">aaaaaaaaaaaaaaaaaaaaaaaaaaaaaa")]
			[PXDefault]
			[PXUIField(DisplayName = "New Revision")]
			public virtual string RevisionID { get; set; }
			#endregion
			#region RefreshBudget
			public abstract class refreshBudget : PX.Data.BQL.BqlBool.Field<copyFiles> { }
			[PXUnboundDefault(true)]
			[PXBool()]
			[PXUIField(DisplayName = "Refresh Budget")]
			public virtual bool? RefreshBudget { get; set; }
			#endregion
			#region CopyNotes
			public abstract class copyNotes : PX.Data.BQL.BqlBool.Field<copyNotes> { }
			[PXBool()]
			[PXUIField(DisplayName = "Copy Notes")]
			public virtual bool? CopyNotes { get; set; }
			#endregion
			#region CopyFiles
			public abstract class copyFiles : PX.Data.BQL.BqlBool.Field<copyFiles> { }
			[PXBool()]
			[PXUIField(DisplayName = "Copy Files")]
			public virtual bool? CopyFiles { get; set; }
			#endregion
		}
	}
}
