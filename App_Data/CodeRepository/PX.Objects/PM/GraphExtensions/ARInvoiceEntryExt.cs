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
using System.Text;

using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Data.WorkflowAPI;
using PX.Objects.AR;
using PX.Objects.CM.Extensions;
using PX.Objects.CS;
using PX.Objects.CT;
using PX.Objects.Extensions.MultiCurrency;
using PX.Objects.GL;

using PMBudgetLite = PX.Objects.PM.Lite.PMBudget;

namespace PX.Objects.PM
{
	public class ARInvoiceEntryExt : PXGraphExtension<ARInvoiceEntry>
	{
		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<FeaturesSet.projectAccounting>();
		}

		[InjectDependency]
		public IProjectMultiCurrency MultiCurrencyService { get; set; }

		#region Views/Selects/Delegates

		[PXCopyPasteHiddenView]
		public PXSelect<PMBillingRecord> ProjectBillingRecord;

		[PXCopyPasteHiddenView]
		public PXSelect<PMProforma> ProjectProforma;

		[PXCopyPasteHiddenView]
		public PXSelect<PMRegister> ProjectRegister;

		public PXSelect<PMBudgetAccum> Budget;

		#endregion

		#region Actions/Buttons

		public PXAction<ARInvoice> viewProforma;
		[PXUIField(DisplayName = Messages.ViewProforma, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton(DisplayOnMainToolbar = false)]
		public virtual IEnumerable ViewProforma(PXAdapter adapter)
		{
			if (Base.Document.Current != null && Base.Document.Current.ProformaExists == true)
			{
				ProformaEntry target = PXGraph.CreateInstance<ProformaEntry>();
				target.Document.Current = PXSelect<PMProforma, Where<PMProforma.aRInvoiceDocType, Equal<Current<ARInvoice.docType>>,
					And<PMProforma.aRInvoiceRefNbr, Equal<Current<ARInvoice.refNbr>>, Or<PMProforma.aRInvoiceRefNbr, Equal<Current<ARInvoice.origRefNbr>>>>>>.Select(Base);
				throw new PXRedirectRequiredException(target, true, "ViewInvoice") { Mode = PXBaseRedirectException.WindowMode.NewWindow };
			}

			return adapter.Get();
		}

		public PXAction<ARInvoice> viewPMTrans;
		[PXUIField(DisplayName = Messages.ViewPMTrans, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton(DisplayOnMainToolbar = false)]
		public virtual IEnumerable ViewPMTrans(PXAdapter adapter)
		{
			if (Base.Document.Current != null)
			{
				var graph = PXGraph.CreateInstance<TransactionInquiry>();
				var filter = graph.Filter.Insert();
				filter.ARDocType = Base.Document.Current.DocType;
				filter.ARRefNbr = Base.Document.Current.RefNbr;

				throw new PXRedirectRequiredException(graph, true, "ViewPMTrans") { Mode = PXBaseRedirectException.WindowMode.NewWindow };
			}

			return adapter.Get();
		}

		#endregion

		#region Event Handlers

		protected virtual void _(Events.RowSelected<ARInvoice> e)
		{
			viewProforma.SetEnabled(e.Row?.ProformaExists == true);
			SetViewPMTransEnabled(e.Row);

			if (e.Row?.ProformaExists == true)
			{
				PXUIFieldAttribute.SetEnabled<ARInvoice.customerID>(e.Cache, e.Row, false);
				PXUIFieldAttribute.SetEnabled<ARInvoice.projectID>(e.Cache, e.Row, false);
				PXUIFieldAttribute.SetEnabled<ARInvoice.taxCalcMode>(e.Cache, e.Row, false);
			}

			var arInvoice = e.Row;

			if (arInvoice == null)
			{
				return;
			}

			CheckNoProFormaInvoiceForProFormaBilledProject(arInvoice);
		}

		private void SetViewPMTransEnabled(ARInvoice doc)
		{
			var enabled = false;
			PMProject project;
			if (doc != null && ProjectDefaultAttribute.IsProject(Base, doc.ProjectID, out project))
			{
				enabled = project.BaseType == CT.CTPRType.Project;
			}
			this.viewPMTrans.SetEnabled(enabled);
		}

		private bool isARInvoiceDeleting = false;
		protected virtual void _(Events.RowDeleting<ARInvoice> e)
		{
			if (e.Row != null && e.Row.ProjectID != null && e.Row.ProjectID != PM.ProjectDefaultAttribute.NonProject())
			{
				var selectReleased = new PXSelectJoin<PMBillingRecord,
				InnerJoin<PMBillingRecordEx, On<PMBillingRecord.projectID, Equal<PMBillingRecordEx.projectID>,
				And<PMBillingRecord.billingTag, Equal<PMBillingRecordEx.billingTag>,
				And<PMBillingRecord.recordID, Less<PMBillingRecordEx.recordID>,
				And<PMBillingRecordEx.proformaRefNbr, IsNotNull>>>>>,
				Where<PMBillingRecord.projectID, Equal<Required<PMBillingRecord.projectID>>,
				And<PMBillingRecord.aRDocType, Equal<Required<PMBillingRecord.aRDocType>>,
				And<PMBillingRecord.aRRefNbr, Equal<Required<PMBillingRecord.aRRefNbr>>>>>>(Base);

				var resultset = selectReleased.Select(e.Row.ProjectID, e.Row.DocType, e.Row.RefNbr);
				if (resultset.Count > 0)
				{
					StringBuilder sb = new StringBuilder();
					foreach (PXResult<PMBillingRecord, PMBillingRecordEx> res in resultset)
					{
						PMBillingRecordEx item = (PMBillingRecordEx)res;
						sb.AppendFormat("{0}-{1},", item.ARDocType, item.ARRefNbr);
					}

					string list = sb.ToString().TrimEnd(',');

					throw new PXException(AR.Messages.ReleasedProforma, list);
				}
			}

			isARInvoiceDeleting = true;
		}
		protected virtual void _(Events.RowDeleted<ARInvoice> e)
		{
			var select = new PXSelectJoin<PMBillingRecord,
				LeftJoin<PMProforma, On<PMBillingRecord.proformaRefNbr, Equal<PMProforma.refNbr>, And<PMProforma.corrected, Equal<False>>>>,
				Where<PMBillingRecord.aRDocType, Equal<Required<PMBillingRecord.aRDocType>>,
					And<PMBillingRecord.aRRefNbr, Equal<Required<PMBillingRecord.aRRefNbr>>>>>(Base);

			var resultset = select.Select(e.Row.DocType, e.Row.RefNbr);
			if (resultset.Count > 0)
			{
				PMBillingRecord billingRecord = PXResult.Unwrap<PMBillingRecord>(resultset[0]);
				if (billingRecord != null)
				{
					if (billingRecord.ProformaRefNbr != null)
					{
						billingRecord.ARDocType = null;
						billingRecord.ARRefNbr = null;
						ProjectBillingRecord.Update(billingRecord);

						PMProforma proforma = PXResult.Unwrap<PMProforma>(resultset[0]);
						if (proforma != null && !string.IsNullOrEmpty(proforma.RefNbr))
						{
							proforma.ARInvoiceDocType = null;
							proforma.ARInvoiceRefNbr = null;
							proforma.Released = false;
							proforma.Status = ProformaStatus.Open;
							ProjectProforma.Update(proforma);
						}
					}
					else
					{
						ProjectBillingRecord.Delete(billingRecord);
					}
				}

				PMRegister allocationReversal = PXSelect<PMRegister,
						Where<PMRegister.origDocType, Equal<PMOrigDocType.allocationReversal>,
							And<PMRegister.origNoteID, Equal<Required<ARInvoice.noteID>>,
							And<PMRegister.released, Equal<False>>>>>.Select(Base, e.Row.NoteID);
				if (allocationReversal != null)
					ProjectRegister.Delete(allocationReversal);
			}

			AddToUnbilledSummary(e.Row);

			var matchingProformas = SelectFrom<PMProforma>
				.Where<PMProforma.aRInvoiceDocType.IsEqual<P.AsString>
					.And<PMProforma.aRInvoiceRefNbr.IsEqual<P.AsString>>>
				.View.Select(Base, e.Row.DocType, e.Row.RefNbr);

			foreach (PMProforma proforma in matchingProformas)
			{
				proforma.ARInvoiceDocType = null;
				proforma.ARInvoiceRefNbr = null;
				ProjectProforma.Update(proforma);
			}
		}

		protected virtual void _(Events.RowInserted<ARTran> e)
		{
			if (Base.Document.Current.IsRetainageDocument != true && e.Row.TaskID != null && Base.Document.Current.ProformaExists != true)
			{
				AddToInvoiced(e.Row, GetProjectedAccountGroup(e.Row), (int)ARDocType.SignAmount(e.Row.TranType).GetValueOrDefault(1));
				AddToDraftRetained(e.Row, GetProjectedAccountGroup(e.Row), (int)ARDocType.SignAmount(e.Row.TranType).GetValueOrDefault(1));
				RemoveObsoleteLines();
			}
		}

		protected virtual void _(Events.RowUpdated<ARTran> e)
		{
			if (e.Row != null)
			{
				SyncBudgets(e.Row, e.OldRow);
			}
		}

		protected virtual void _(Events.RowUpdated<CurrencyInfo> e)
		{
			if (e.Row == null) return;

			foreach (ARTran tran in Base.Transactions.Select())
			{
				decimal newTranAmt = 0;
				if (e.Row.CuryRate != null)
					newTranAmt = e.Row.CuryConvBase(tran.CuryTranAmt.GetValueOrDefault());
				var newTran = Base.Transactions.Cache.CreateCopy(tran) as ARTran;
				newTran.TranAmt = newTranAmt;

				decimal oldTranAmt = 0;
				if (e.OldRow.CuryRate != null)
					oldTranAmt = e.OldRow.CuryConvBase(tran.CuryTranAmt.GetValueOrDefault());
				var oldTran = Base.Transactions.Cache.CreateCopy(tran) as ARTran;
				oldTran.TranAmt = oldTranAmt;

				SyncBudgets(newTran, oldTran);
			}
		}

		protected virtual void _(Events.RowDeleted<ARTran> e)
		{
			if (Base.Document.Current.IsRetainageDocument != true && e.Row.TaskID != null && Base.Document.Current.ProformaExists != true
				&& Base.Document.Current.IsMigratedRecord != true)
			{
				AddToInvoiced(e.Row, GetProjectedAccountGroup(e.Row), -1 * (int)ARDocType.SignAmount(e.Row.TranType).GetValueOrDefault(1));
				AddToDraftRetained(e.Row, GetProjectedAccountGroup(e.Row), -1 * (int)ARDocType.SignAmount(e.Row.TranType).GetValueOrDefault(1));

				var select = new PXSelect<PMTran, Where<PMTran.aRTranType, Equal<Required<PMTran.aRTranType>>,
					And<PMTran.aRRefNbr, Equal<Required<PMTran.aRRefNbr>>,
					And<PMTran.refLineNbr, Equal<Required<PMTran.refLineNbr>>>>>>(Base);

				string tranType = e.Row.TranType;
				string refNbr = e.Row.RefNbr;
				int? lineNbr = e.Row.LineNbr;

				if (tranType == ARDocType.CreditMemo && !string.IsNullOrEmpty(Base.Document.Current.OrigRefNbr))
				{
					tranType = Base.Document.Current.OrigDocType;
					refNbr = Base.Document.Current.OrigRefNbr;
					lineNbr = e.Row.OrigLineNbr;
				}

				PMTran original = select.SelectWindowed(0, 1, tranType, refNbr, lineNbr);

				if (original == null)//progressive line
					SubtractValuesToInvoice(((ARTran)e.Row), GetProjectedAccountGroup((ARTran)e.Row), -1 * (int)ARDocType.SignAmount(((ARTran)e.Row).TranType).GetValueOrDefault(1)); //Restoring AmountToInvoice

				RemoveObsoleteLines();
			}

			if (e.Row != null)
			{
				if (!isARInvoiceDeleting)
				{
					foreach (PXResult<PMTran, PMProject> result in Base.RefContractUsageTran
						.Select(e.Row.TranType, e.Row.RefNbr, e.Row.LineNbr))
					{
						PMTran tran = result;
						PMProject project = result;
						if (tran != null)
						{
							tran.ARRefNbr = null;
							tran.ARTranType = null;
							tran.RefLineNbr = null;
							if (Base.Document.Current != null
								&& Base.Document.Current.ProformaExists != true
								&& (project?.BaseType == CTPRType.Project
									|| ARInvoiceEntry.UnlinkContractUsagesOnDeleteScope.IsActive))
							{
								tran.Billed = false;
								tran.BilledDate = null;
								tran.InvoicedQty = 0;
								tran.InvoicedAmount = 0;
								PM.RegisterReleaseProcess.AddToUnbilledSummary(Base, tran);
							}

							Base.RefContractUsageTran.Update(tran);
						}
					}
				}
			}
		}

		protected virtual void _(Events.FieldDefaulting<ARTran, ARTran.costCodeID> e)
		{
			PMProject project;
			if (CostCodeAttribute.UseCostCode() && ProjectDefaultAttribute.IsProject(Base, e.Row.ProjectID, out project))
			{
				if (project.BudgetLevel == BudgetLevels.Task)
				{
					e.NewValue = CostCodeAttribute.GetDefaultCostCode();
				}
			}
		}

		protected virtual void _(Events.RowPersisted<ARInvoice> e)
		{
			if ((e.Operation & PXDBOperation.Command) == PXDBOperation.Delete)
			{
				bool unlinkContractUsages = ARInvoiceEntry.UnlinkContractUsagesOnDeleteScope.IsActive;
				PXUpdateJoin<Set<PMTran.aRTranType, Null,
					Set<PMTran.aRRefNbr, Null,
					Set<PMTran.refLineNbr, Null,
					Set<PMTran.billed,
						IIf<P.AsBool.IsNotEqual<True>
								.And<PMProject.baseType.IsEqual<CTPRType.project>.Or<P.AsBool.IsEqual<True>>>,
							False,
							PMTran.billed>,
					Set<PMTran.billedDate,
						IIf<P.AsBool.IsNotEqual<True>
								.And<PMProject.baseType.IsEqual<CTPRType.project>.Or<P.AsBool.IsEqual<True>>>,
							Null,
							PMTran.billedDate>,
					Set<PMTran.invoicedQty,
						IIf<P.AsBool.IsNotEqual<True>
								.And<PMProject.baseType.IsEqual<CTPRType.project>.Or<P.AsBool.IsEqual<True>>>,
							decimal0,
							PMTran.invoicedQty>,
					Set<PMTran.invoicedAmount,
						IIf<P.AsBool.IsNotEqual<True>
								.And<PMProject.baseType.IsEqual<CTPRType.project>.Or<P.AsBool.IsEqual<True>>>,
							decimal0,
							PMTran.invoicedAmount>
					>>>>>>>,
				PMTran,
				InnerJoin<PMProject,
					On<PMTran.projectID.IsEqual<PMProject.contractID>>>,
				Where<PMTran.aRTranType.IsEqual<P.AsString>
					.And<PMTran.aRRefNbr.IsEqual<P.AsString>>>>
				.Update(
					Base,
					e.Row.ProformaExists,
					unlinkContractUsages,
					e.Row.ProformaExists,
					unlinkContractUsages,
					e.Row.ProformaExists,
					unlinkContractUsages,
					e.Row.ProformaExists,
					unlinkContractUsages,
					e.Row.DocType,
					e.Row.RefNbr);
			}
		}

		protected virtual void _(Events.FieldUpdated<ARInvoice.projectID> e)
		{
			var arInvoice = e.Row as ARInvoice;

			if (arInvoice == null)
			{
				return;
			}

			CheckNoProFormaInvoiceForProFormaBilledProject(arInvoice);
		}

		private void CheckNoProFormaInvoiceForProFormaBilledProject(ARInvoice arInvoice)
		{
			// No warning if proforma is linked to invoice
			if (arInvoice.ProformaExists == true)
			{
				return;
			}

			if (!ProjectDefaultAttribute.IsProject(Base, arInvoice.ProjectID, out var project))
			{
				return;
			}

			if (project.CreateProforma == true)
			{
				Base.Document.Cache.RaiseExceptionHandling<ARInvoice.projectID>(
					arInvoice,
					arInvoice.ProjectID,
					new PXSetPropertyException(Messages.ProjectIsBilledWithProformaARInvoiceWarning,
						PXErrorLevel.Warning));
			}
		}

		#endregion

		private void SyncBudgets(ARTran row, ARTran oldRow)
		{
			if (Base.Document.Current.IsRetainageDocument != true &&
				Base.Document.Current.ProformaExists != true &&
				(row.TaskID != oldRow.TaskID ||
				row.TranAmt != oldRow.TranAmt ||
				row.AccountID != oldRow.AccountID ||
				row.CostCodeID != oldRow.CostCodeID ||
				row.UOM != oldRow.UOM ||
				row.Qty != oldRow.Qty))
			{
				if (oldRow.TaskID != null)
				{
					AddToInvoiced(oldRow, GetProjectedAccountGroup(oldRow), -1 * (int)ARDocType.SignAmount(oldRow.TranType).GetValueOrDefault(1));
					AddToDraftRetained(oldRow, GetProjectedAccountGroup(oldRow), -1 * (int)ARDocType.SignAmount(oldRow.TranType).GetValueOrDefault(1));
				}
				if (row.TaskID != null)
				{
					AddToInvoiced(row, GetProjectedAccountGroup(row), (int)ARDocType.SignAmount(oldRow.TranType).GetValueOrDefault(1));
					AddToDraftRetained(row, GetProjectedAccountGroup(row), (int)ARDocType.SignAmount(oldRow.TranType).GetValueOrDefault(1));
				}
				RemoveObsoleteLines();
			}
		}

		public virtual int? GetProjectedAccountGroup(ARTran line)
		{
			int? projectedRevenueAccountGroupID = null;

			if (line.AccountID != null)
			{
				Account revenueAccount = PXSelectorAttribute.Select<ARTran.accountID>(Base.Transactions.Cache, line, line.AccountID) as Account;
				if (revenueAccount != null)
				{
					if (revenueAccount.AccountGroupID != null)
						projectedRevenueAccountGroupID = revenueAccount.AccountGroupID;
				}
			}

			return projectedRevenueAccountGroupID;
		}

		public virtual void AddToInvoiced(ARTran line, int? revenueAccountGroup, int mult)
		{
			if (base.Base.Document.Current.Scheduled != true)
			{
				PMBudgetAccum budget = GetTargetBudget(revenueAccountGroup, line);

				if (budget != null)
				{
					decimal curyAmount = mult * GetValueInProjectCurrency(budget.ProjectID, line.CuryTranAmt + line.CuryRetainageAmt);
					budget.CuryInvoicedAmount += curyAmount;
					budget.InvoicedAmount += GetBaseValueForBudget(PMProject.PK.Find(Base, line.ProjectID), curyAmount);
					IN.INUnitAttribute.TryConvertGlobalUnits(Base, line.UOM, budget.UOM, line.Qty.GetValueOrDefault(), IN.INPrecision.QUANTITY, out decimal qty);
					budget.InvoicedQty += mult * qty;
				}
			}
		}

		public virtual void SubtractValuesToInvoice(ARTran line, int? revenueAccountGroup, int mult)
		{
			PMBudgetAccum budget = GetTargetBudget(revenueAccountGroup, line);

			if (budget != null)
			{
				decimal curyAmount = mult * GetValueInProjectCurrency(budget.ProjectID, line.CuryTranAmt + line.CuryRetainageAmt);
				budget.CuryAmountToInvoice -= curyAmount;
				budget.AmountToInvoice -= GetBaseValueForBudget(PMProject.PK.Find(Base, line.ProjectID), curyAmount);
				if (budget.ProgressBillingBase == ProgressBillingBase.Quantity)
				{
					IN.INUnitAttribute.TryConvertGlobalUnits(Base, line.UOM, budget.UOM, line.Qty.GetValueOrDefault(), IN.INPrecision.QUANTITY, out decimal qty);
					budget.QtyToInvoice -= mult * qty;
				}
			}
		}

		protected virtual void AddToUnbilledSummary(ARInvoice row)
		{
			if (row.ProformaExists != true)
			{
				foreach (PMTran pMRef in SelectFrom<PMTran>
					.InnerJoin<PMProject>
						.On<PMTran.projectID.IsEqual<PMProject.contractID>
							.And<PMProject.baseType.IsNotEqual<CTPRType.contract>>>
					.Where<PMTran.aRTranType.IsEqual<P.AsString>
						.And<PMTran.aRRefNbr.IsEqual<P.AsString>>>
					.View
					.Select(Base, row.DocType, row.RefNbr))
				{
					pMRef.Billed = false;
					PM.RegisterReleaseProcess.AddToUnbilledSummary(Base, pMRef);
				}
			}
		}

		public virtual void AddToDraftRetained(ARTran line, int? revenueAccountGroup, int mult)
		{
			PMBudgetAccum budget = GetTargetBudget(revenueAccountGroup, line);

			if (budget != null)
			{
				decimal curyAmount = mult * GetValueInProjectCurrency(budget.ProjectID, line.CuryRetainageAmt);
				budget.CuryDraftRetainedAmount += curyAmount;
				budget.DraftRetainedAmount += GetBaseValueForBudget(PMProject.PK.Find(Base, line.ProjectID), curyAmount);
			}
		}

		private PMBudgetAccum GetTargetBudget(int? accountGroupID, ARTran line)
		{
			if (line.ProjectID == null)
				return null;

			if (line.TaskID == null)
				return null;

			if (accountGroupID == null)
				return null;

			PMProject project = PMProject.PK.Find(Base, line.ProjectID);

			if (project == null)
				return null;

			if (project.NonProject == true)
				return null;

			PMAccountGroup ag = PMAccountGroup.PK.Find(Base, accountGroupID);

			BudgetService budgetService = new BudgetService(Base);
			PMBudgetLite budget = budgetService.SelectProjectBalance(ag, project, line.TaskID, line.InventoryID, line.CostCodeID, out bool isExisting);

			return Budget.Insert(new PMBudgetAccum
			{
				Type = budget.Type,
				ProjectID = budget.ProjectID,
				ProjectTaskID = budget.TaskID,
				AccountGroupID = budget.AccountGroupID,
				InventoryID = budget.InventoryID,
				CostCodeID = budget.CostCodeID,
				UOM = budget.UOM,
				Description = budget.Description,
				CuryInfoID = project.CuryInfoID,
				ProgressBillingBase = budget.ProgressBillingBase
			});
		}

		public sealed override void Configure(PXScreenConfiguration config) =>
			Configure(config.GetScreenConfigurationContext<ARInvoiceEntry, ARInvoice>());
		protected static void Configure(WorkflowContext<ARInvoiceEntry, ARInvoice> context)
		{
			var relatedDocumentsCategory = context.Categories.Get(ARInvoiceEntry_Workflow.CategoryID.RelatedDocuments);

			context.UpdateScreenConfigurationFor(screen =>
			{
				return screen
					.WithActions(actions =>
					{
						actions.Add<ARInvoiceEntryExt>(g => g.viewProforma,
							c => c.InFolder(relatedDocumentsCategory)
							.IsDisabledWhen(context.Conditions.Get("IsPrepaymentInvoiceReversing"))
							.IsHiddenWhen(context.Conditions.Get("IsFinCharge"))
							.IsHiddenWhen(context.Conditions.Get("IsPrepaymentInvoice"))
						);
						actions.Add<ARInvoiceEntryExt>(g => g.viewPMTrans,
							c => c.InFolder(FolderType.InquiriesFolder)
							.IsDisabledWhen(context.Conditions.Get("IsPrepaymentInvoiceReversing"))
							.IsHiddenWhen(context.Conditions.Get("IsPrepaymentInvoice"))
						);
					});
			});
		}

		protected virtual void RemoveObsoleteLines()
		{
			foreach (PMBudgetAccum item in Budget.Cache.Inserted)
			{
				if (item.CuryInvoicedAmount.GetValueOrDefault() == 0 && item.InvoicedAmount.GetValueOrDefault() == 0
					&& item.CuryAmountToInvoice.GetValueOrDefault() == 0 && item.AmountToInvoice.GetValueOrDefault() == 0
					&& item.InvoicedQty.GetValueOrDefault() == 0.0m)
				{
					Budget.Cache.Remove(item);
				}
			}
		}

		private decimal GetValueInProjectCurrency(int? projectID, decimal? value)
		{
			PMProject project = PMProject.PK.Find(Base, projectID);

			return MultiCurrencyService.GetValueInProjectCurrency(Base, project, Base.Document.Current.CuryID, Base.Document.Current.DocDate, value);
		}

		private decimal GetBaseValueForBudget(PMProject project, decimal curyValue)
		{
			if (project.CuryID == project.BaseCuryID) return curyValue;
			else
			{
				CurrencyInfo currencyInfo = Base.FindImplementation<IPXCurrencyHelper>().GetCurrencyInfo(project.CuryInfoID);
				return currencyInfo.CuryConvBase(curyValue);
			}
		}

		[PXOverride]
		public string GetCustomerReportID(string reportID, ARInvoice doc, Func<string, ARInvoice, string> baseGetCustomerReportID)
		{
			var activityExt = Base.GetExtension<ARInvoiceEntry_ActivityDetailsExt>();

			if (ProjectDefaultAttribute.IsProject(Base, doc.ProjectID) && activityExt.ProjectInvoiceReportActive(doc.ProjectID) != null && reportID == ARReports.InvoiceMemoReportID)
			{
				PMProject rec = PMProject.PK.Find(Base, doc.ProjectID);
				return GetProjectSpecificCustomerReportID(activityExt.ProjectInvoiceReportActive(doc.ProjectID), doc, rec);
			}
			return baseGetCustomerReportID(reportID, doc);
		}

		public virtual string GetProjectSpecificCustomerReportID(string reportID, ARInvoice doc, PMProject project)
		{
			NotificationSetup setup = GetSetup(PMNotificationSource.Project, reportID, doc.BranchID);

			if (setup == null) return reportID;
			NotificationSource notification = GetSource(PMNotificationSource.Project, project, (Guid)setup.SetupID, doc.BranchID);

			return notification == null || notification.ReportID == null
				? reportID :
				notification.ReportID;
		}

		private NotificationSetup GetSetup(string source, string reportID, int? branchID)
		{
			NotificationSetup setup;
			setup = SelectFrom<NotificationSetup>
				.Where<NotificationSetup.active.IsEqual<True>
					.And<NotificationSetup.sourceCD.IsEqual<@P.AsString>>
					.And<NotificationSetup.reportID.IsEqual<@P.AsString>>
					.And<NotificationSetup.nBranchID.IsEqual<@P.AsInt>.Or<NotificationSetup.nBranchID.IsNull>>>
				.OrderBy<NotificationSetup.nBranchID.Desc>
				.View.SelectWindowed(Base, 0, 1, source, reportID, branchID);
			return setup;
		}

		public NotificationSource GetSource(string sourceType, object row, Guid setupID, int? branchID)
		{
			if (row == null) return null;
			ProjectEntry graph = PXGraph.CreateInstance<ProjectEntry>();

			NotificationSource result = null;
			foreach (NotificationSource rec in graph.NotificationSources.View.SelectMulti().RowCast<NotificationSource>())
			{
				if (rec.SetupID == setupID && rec.NBranchID == branchID)
					return rec;
				if (rec.SetupID == setupID && rec.NBranchID == null)
					result = rec;
			}
			return result;
		}
		
	}
}
