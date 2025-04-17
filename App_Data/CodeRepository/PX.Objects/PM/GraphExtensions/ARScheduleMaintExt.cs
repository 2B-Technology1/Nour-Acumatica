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

using PX.Data;
using PX.Objects.AR;
using PX.Objects.AR.Overrides.ScheduleMaint;
using PX.Objects.CS;
using PX.Objects.GL;
using PX.Objects.SO;
using PMBudgetLite = PX.Objects.PM.Lite.PMBudget;

namespace PX.Objects.PM
{
	public class ARScheduleMaintExt : PXGraphExtension<ARScheduleMaint>
	{
		public PXSelect<PMBudgetAccum> Budget;

		[PXViewName(PX.Objects.AR.Messages.ARTran)]
		[PXImport(typeof(ARInvoice))]
		public PXOrderedSelect<ARInvoice, ARTran,
			Where<
				ARTran.tranType, Equal<Current<ARInvoice.docType>>,
				And<ARTran.refNbr, Equal<Current<ARInvoice.refNbr>>,
				And<Where<ARTran.lineType, IsNull, Or<ARTran.lineType, NotEqual<SOLineType.discount>>>>>>,
			OrderBy<
				Asc<ARTran.tranType,
					Asc<ARTran.refNbr,
					Asc<ARTran.sortOrder,
					Asc<ARTran.lineNbr>>>>>>
			Transactions;

		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<FeaturesSet.projectAccounting>();
		}

		[PXOverride]
		public void AddDocumentToSchedule(ARRegister documentAsRegister)
		{
			ReduceDraftProjectBalance(documentAsRegister.RefNbr);
		}

		private void ReduceDraftProjectBalance(string refNbr)
		{
			PXResultset<PMProject, ARTran> result = PXSelectJoin<PMProject,
									InnerJoin<ARTran, On<PMProject.contractID, Equal<ARTran.projectID>>>,
									Where<ARTran.refNbr, Equal<Required<ARTran.refNbr>>>>.Select<PXResultset<PMProject, ARTran>>(Base, refNbr);

			PMProject project = result;
			ARTran tran = result;

			if (project != null && project.NonProject != true && tran.TaskID != null)
			{
				var revenueAccountGroup = GetProjectedAccountGroup(tran);

				PMBudgetAccum invoiced = GetTargetBudget(revenueAccountGroup, tran);
				invoiced = Budget.Insert(invoiced);

				if (project.CuryID == project.BillingCuryID)
				{
					invoiced.CuryInvoicedAmount -= tran.CuryTranAmt.GetValueOrDefault() + tran.CuryRetainageAmt.GetValueOrDefault();
					invoiced.InvoicedAmount -= tran.TranAmt.GetValueOrDefault() + tran.RetainageAmt.GetValueOrDefault();
				}
				else
				{
					invoiced.CuryInvoicedAmount -= tran.TranAmt.GetValueOrDefault() + tran.RetainageAmt.GetValueOrDefault();
					invoiced.InvoicedAmount -= tran.TranAmt.GetValueOrDefault() + tran.RetainageAmt.GetValueOrDefault();
				}
			}
		}

		private PMBudgetAccum GetTargetBudget(int? accountGroupID, ARTran line)
		{
			PMAccountGroup ag = PMAccountGroup.PK.Find(Base, accountGroupID);
			PMProject project = PMProject.PK.Find(Base, line.ProjectID);

			BudgetService budgetService = new BudgetService(Base);
			PMBudgetLite budget = budgetService.SelectProjectBalance(ag, project, line.TaskID, line.InventoryID, line.CostCodeID, out bool _);

			PMBudgetAccum target = new PMBudgetAccum();
			target.Type = budget.Type;
			target.ProjectID = budget.ProjectID;
			target.ProjectTaskID = budget.TaskID;
			target.AccountGroupID = budget.AccountGroupID;
			target.InventoryID = budget.InventoryID;
			target.CostCodeID = budget.CostCodeID;
			target.UOM = budget.UOM;
			target.Description = budget.Description;
			target.CuryInfoID = project.CuryInfoID;

			return target;
		}

		private int? GetProjectedAccountGroup(ARTran line)
		{
			int? projectedRevenueAccountGroupID = null;

			if (line.AccountID != null)
			{
				Account revenueAccount = PXSelectorAttribute.Select<ARTran.accountID>(Transactions.Cache, line, line.AccountID) as Account;
				if (revenueAccount != null)
				{
					if (revenueAccount.AccountGroupID == null)
						throw new PXException(PM.Messages.RevenueAccountIsNotMappedToAccountGroup, revenueAccount.AccountCD);

					projectedRevenueAccountGroupID = revenueAccount.AccountGroupID;
				}
			}
			return projectedRevenueAccountGroupID;
		}

	}
}
