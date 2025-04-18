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
using PX.Objects.CS;
using System.Collections.Generic;
using System.Linq;

namespace PX.Objects.PM
{
	public class AllocationMaint : PXGraph<AllocationMaint, PMAllocation>
	{
		// ReSharper disable InconsistentNaming
		[InjectDependency]
		private ICurrentUserInformationProvider _currentUserInformationProvider { get; set; }
		// ReSharper restore InconsistentNaming

		#region Metadata for PMFormulaEditor
		public PXSelect<PMTran> PMTranMetaData;
		public PXSelect<PMProject> PMProjectMetaData;
		public PXSelect<PMTask> PMTaskMetaData;
		public PXSelect<PMAccountGroup> PMAccountGroupMetaData;
		public PXSelect<PMBudget> PMBudget;
		public PXSelect<PX.Objects.EP.EPEmployee> EmployeesMetaData;
		public PXSelect<PX.Objects.AP.Vendor> VendorMetaData;
		public PXSelect<PX.Objects.AR.Customer> CustomerMetaData;
		public PXSelect<PX.Objects.IN.InventoryItem> InventoryItemMetaData;
		#endregion

		#region Views/Selects

		public PXSelect<PMAllocation> Allocations;
		public PXSelect<PMAllocationDetail, Where<PMAllocationDetail.allocationID, Equal<Current<PMAllocation.allocationID>>>> Steps;
		public PXSelect<PMAllocationDetail, Where<PMAllocationDetail.allocationID, Equal<Current<PMAllocationDetail.allocationID>>, And<PMAllocationDetail.stepID, Equal<Current<PMAllocationDetail.stepID>>>>> Step;
		public PXSelect<PMAllocationDetail, Where<PMAllocationDetail.allocationID, Equal<Current<PMAllocationDetail.allocationID>>, And<PMAllocationDetail.stepID, Equal<Current<PMAllocationDetail.stepID>>>>> StepRules;
		public PXSelect<PMAllocationDetail, Where<PMAllocationDetail.allocationID, Equal<Current<PMAllocationDetail.allocationID>>, And<PMAllocationDetail.stepID, Equal<Current<PMAllocationDetail.stepID>>>>> StepSettings;

		#endregion

		#region Event Handlers

		protected virtual void PMAllocationDetail_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			PMAllocationDetail row = e.Row as PMAllocationDetail;
			if (row != null)
			{
				PXUIFieldAttribute.SetEnabled<PMAllocationDetail.selectOption>(sender, e.Row, row.Method == PMMethod.Transaction);
				PXUIFieldAttribute.SetEnabled<PMAllocationDetail.post>(sender, e.Row, row.Method == PMMethod.Transaction);
				PXUIFieldAttribute.SetEnabled<PMAllocationDetail.dateSource>(sender, e.Row, row.Method == PMMethod.Transaction);

				PXUIFieldAttribute.SetVisible<PMAllocationDetail.accountOrigin>(sender, e.Row, row.UpdateGL == true);
				PXUIFieldAttribute.SetVisible<PMAllocationDetail.accountID>(sender, e.Row, row.UpdateGL == true);
				PXUIFieldAttribute.SetVisible<PMAllocationDetail.subMask>(sender, e.Row, row.UpdateGL == true);
				PXUIFieldAttribute.SetVisible<PMAllocationDetail.subID>(sender, e.Row, row.UpdateGL == true);
				PXUIFieldAttribute.SetEnabled<PMAllocationDetail.accountOrigin>(sender, e.Row, row.UpdateGL == true);
				PXUIFieldAttribute.SetVisible<PMAllocationDetail.offsetProjectOrigin>(sender, e.Row, row.UpdateGL != true);
				PXUIFieldAttribute.SetVisible<PMAllocationDetail.offsetProjectID>(sender, e.Row, row.UpdateGL != true);
				PXUIFieldAttribute.SetVisible<PMAllocationDetail.offsetTaskOrigin>(sender, e.Row, row.UpdateGL != true);

				PXUIFieldAttribute.SetEnabled<PMAllocationDetail.projectOrigin>(sender, e.Row, row.Method == PMMethod.Transaction);
				PXUIFieldAttribute.SetEnabled<PMAllocationDetail.taskOrigin>(sender, e.Row, row.Method == PMMethod.Transaction);

				PXUIFieldAttribute.SetVisible<PMAllocationDetail.offsetAccountOrigin>(sender, e.Row, row.UpdateGL == true);
				PXUIFieldAttribute.SetVisible<PMAllocationDetail.offsetAccountID>(sender, e.Row, row.UpdateGL == true);
				PXUIFieldAttribute.SetVisible<PMAllocationDetail.offsetSubMask>(sender, e.Row, row.UpdateGL == true);
				PXUIFieldAttribute.SetVisible<PMAllocationDetail.offsetSubID>(sender, e.Row, row.UpdateGL == true);
				PXUIFieldAttribute.SetEnabled<PMAllocationDetail.offsetProjectOrigin>(sender, e.Row, row.Method == PMMethod.Transaction);
				PXUIFieldAttribute.SetEnabled<PMAllocationDetail.offsetTaskOrigin>(sender, e.Row, row.Method == PMMethod.Transaction);

				PXUIFieldAttribute.SetVisible<PMAllocationDetail.taskID>(sender, e.Row, row.ProjectID != null);
				PXUIFieldAttribute.SetVisible<PMAllocationDetail.taskCD>(sender, e.Row, row.ProjectID == null);
				PXUIFieldAttribute.SetEnabled<PMAllocationDetail.sourceBranchID>(sender, e.Row, row.SelectOption == PMSelectOption.Transaction);
				PXUIFieldAttribute.SetEnabled<PMAllocationDetail.projectID>(sender, e.Row, row.ProjectOrigin == PMOrigin.Change && row.AccountGroupOrigin != PMOrigin.None);
				PXUIFieldAttribute.SetEnabled<PMAllocationDetail.taskID>(sender, e.Row, row.TaskOrigin == PMOrigin.Change && row.AccountGroupOrigin != PMOrigin.None);
				PXUIFieldAttribute.SetEnabled<PMAllocationDetail.taskCD>(sender, e.Row, row.TaskOrigin == PMOrigin.Change && row.AccountGroupOrigin != PMOrigin.None);
				PXUIFieldAttribute.SetEnabled<PMAllocationDetail.accountGroupID>(sender, e.Row, row.AccountGroupOrigin == PMOrigin.Change);
				PXUIFieldAttribute.SetEnabled<PMAllocationDetail.accountID>(sender, e.Row, row.AccountOrigin == PMOrigin.Change);

				PXUIFieldAttribute.SetVisible<PMAllocationDetail.offsetTaskID>(sender, e.Row, row.UpdateGL != true && row.OffsetProjectID != null);
				PXUIFieldAttribute.SetVisible<PMAllocationDetail.offsetTaskCD>(sender, e.Row, row.UpdateGL != true && row.OffsetProjectID == null);
				PXUIFieldAttribute.SetVisible<PMAllocationDetail.offsetBranchOrigin>(sender, e.Row, ShowBranchOptions());
				PXUIFieldAttribute.SetEnabled<PMAllocationDetail.targetBranchID>(sender, e.Row, row.OffsetBranchOrigin == PMOrigin.Change);
				PXUIFieldAttribute.SetEnabled<PMAllocationDetail.offsetProjectID>(sender, e.Row, row.OffsetProjectOrigin == PMOrigin.Change);
				PXUIFieldAttribute.SetEnabled<PMAllocationDetail.offsetTaskID>(sender, e.Row, row.OffsetTaskOrigin == PMOrigin.Change);
				PXUIFieldAttribute.SetEnabled<PMAllocationDetail.offsetTaskCD>(sender, e.Row, row.OffsetTaskOrigin == PMOrigin.Change);
				PXUIFieldAttribute.SetEnabled<PMAllocationDetail.offsetAccountGroupID>(sender, e.Row, row.OffsetAccountGroupOrigin == PMOrigin.Change);
				PXUIFieldAttribute.SetEnabled<PMAllocationDetail.offsetAccountID>(sender, e.Row, row.OffsetAccountOrigin == PMOrigin.Change);

				PXUIFieldAttribute.SetEnabled<PMAllocationDetail.rangeStart>(sender, e.Row, row.SelectOption == PMSelectOption.Step);
				PXUIFieldAttribute.SetEnabled<PMAllocationDetail.rangeEnd>(sender, e.Row, row.SelectOption == PMSelectOption.Step);
				PXUIFieldAttribute.SetEnabled<PMAllocationDetail.accountGroupFrom>(sender, e.Row, row.SelectOption != PMSelectOption.Step);
				PXUIFieldAttribute.SetEnabled<PMAllocationDetail.accountGroupTo>(sender, e.Row, row.SelectOption != PMSelectOption.Step);
				PXUIFieldAttribute.SetEnabled<PMAllocationDetail.rateTypeID>(sender, e.Row, row.Method == PMMethod.Transaction);
				PXUIFieldAttribute.SetEnabled<PMAllocationDetail.billableQtyFormula>(sender, e.Row, row.Post == true && row.Method == PMMethod.Transaction);
				PXUIFieldAttribute.SetEnabled<PMAllocationDetail.allocateZeroAmount>(sender, e.Row, row.Method != PMMethod.Budget);
				PXUIFieldAttribute.SetEnabled<PMAllocationDetail.qtyFormula>(sender, e.Row, row.Post == true && row.Method == PMMethod.Transaction);
				PXUIFieldAttribute.SetEnabled<PMAllocationDetail.amountFormula>(sender, e.Row, row.Post == true && row.Method == PMMethod.Transaction);
				PXUIFieldAttribute.SetEnabled<PMAllocationDetail.descriptionFormula>(sender, e.Row, row.Post == true);

				PXUIFieldAttribute.SetEnabled<PMAllocationDetail.groupByDate>(sender, e.Row, row.Post == true && row.Method == PMMethod.Transaction);
				PXUIFieldAttribute.SetEnabled<PMAllocationDetail.groupByEmployee>(sender, e.Row, row.Post == true && row.Method == PMMethod.Transaction);
				PXUIFieldAttribute.SetEnabled<PMAllocationDetail.groupByItem>(sender, e.Row, row.Post == true && row.Method == PMMethod.Transaction);
				PXUIFieldAttribute.SetEnabled<PMAllocationDetail.groupByVendor>(sender, e.Row, row.Post == true && row.Method == PMMethod.Transaction);
				ValidateWarnings(row);
			}
		}

		public virtual bool ShowBranchOptions()
		{
			if (!PXAccess.FeatureInstalled<FeaturesSet.branch>() && !PXAccess.FeatureInstalled<FeaturesSet.multiCompany>())
				return false;

			var ids = _currentUserInformationProvider.GetActiveBranches();

			return !(ids == null || ids.Count() <= 1);
		}

		protected virtual void _(Events.FieldUpdated<PMAllocationDetail, PMAllocationDetail.offsetBranchOrigin> e)
		{
			if (e.Row != null && e.Row.OffsetBranchOrigin != null && e.Row.OffsetBranchOrigin == PMOrigin.Source)
				e.Cache.SetDefaultExt<PMAllocationDetail.targetBranchID>(e.Row);
		}

		protected virtual void PMAllocationDetail_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			PMAllocationDetail row = e.Row as PMAllocationDetail;
			if (row == null) return;

			PMAllocationDetail oldRow = e.OldRow as PMAllocationDetail;
			if (oldRow == null) return;

			if (row.SelectOption == PMSelectOption.Step && oldRow.SelectOption != PMSelectOption.Step)
			{
				row.AccountGroupFrom = null;
				row.AccountGroupTo = null;
			}
			else if (row.SelectOption != PMSelectOption.Step && oldRow.SelectOption == PMSelectOption.Step)
			{
				row.RangeStart = null;
				row.RangeEnd = null;
			}

			ValidateWarnings(row);
		}

		protected virtual void PMAllocationDetail_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			PMAllocationDetail row = e.Row as PMAllocationDetail;
			if (row != null && e.Operation != PXDBOperation.Delete)
			{
				Validate(row);
			}
		}

		protected virtual void PMAllocationDetail_UpdateGL_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			PMAllocationDetail row = e.Row as PMAllocationDetail;
			if (row != null)
			{
				if (row.UpdateGL == true)
				{
					sender.SetValueExt<PMAllocationDetail.accountOrigin>(row, PMOrigin.Change);
					sender.SetValueExt<PMAllocationDetail.offsetAccountOrigin>(row, PMOrigin.Change);
					row.Reverse = PMReverse.OnInvoiceRelease;
				}
				else
				{
					sender.SetValueExt<PMAllocationDetail.accountGroupOrigin>(row, PMOrigin.Source);
					sender.SetValueExt<PMAllocationDetail.offsetAccountGroupOrigin>(row, PMOrigin.Source);
					row.Reverse = PMReverse.OnInvoiceGeneration;
				}
			}
		}

		protected virtual void PMAllocationDetail_AccountOrigin_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			PMAllocationDetail row = e.Row as PMAllocationDetail;
			if (row != null)
			{
				if (row.AccountOrigin == PMOrigin.Change)
				{
					row.AccountGroupOrigin = PMOrigin.FromAccount;
				}
				else
				{
					row.AccountID = null;
				}
			}
		}

		protected virtual void PMAllocationDetail_OffsetAccountOrigin_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			PMAllocationDetail row = e.Row as PMAllocationDetail;
			if (row != null)
			{
				if (row.OffsetAccountOrigin == PMOrigin.Change)
				{
					row.OffsetAccountGroupOrigin = PMOrigin.FromAccount;
				}
				else
				{
					row.OffsetAccountID = null;
				}
			}
		}

		protected virtual void PMAllocationDetail_AccountGroupOrigin_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			PMAllocationDetail row = e.Row as PMAllocationDetail;
			if (row != null)
			{
				if (row.AccountGroupOrigin != PMOrigin.Change)
				{
					row.AccountGroupID = null;
				}

				if (row.AccountGroupOrigin == PMOrigin.None)
				{
					row.ProjectID = null;
					row.TaskID = null;
				}
			}
		}

		protected virtual void PMAllocationDetail_TaskOrigin_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			PMAllocationDetail row = e.Row as PMAllocationDetail;
			if (row != null)
			{
				if (row.TaskCD != null)
				{
					row.TaskCD = null;
				}
				if (row.TaskID != null)
				{
					row.TaskID = null;
				}
			}
		}
		protected virtual void PMAllocationDetail_OffsetTaskOrigin_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			PMAllocationDetail row = e.Row as PMAllocationDetail;
			if (row != null)
			{
				if (row.OffsetTaskCD != null)
				{
					row.OffsetTaskCD = null;
				}
				if (row.OffsetTaskID != null)
				{
					row.OffsetTaskID = null;
				}
			}
		}

		protected virtual void PMAllocationDetail_ProjectID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			PMAllocationDetail row = e.Row as PMAllocationDetail;
			if (row != null && row.TaskCD != null)
			{
				row.TaskCD = null;
			}
		}

		protected virtual void PMAllocationDetail_OffsetProjectID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			PMAllocationDetail row = e.Row as PMAllocationDetail;
			if (row != null && row.OffsetTaskCD != null)
			{
				row.OffsetTaskCD = null;
			}
		}

		protected virtual void PMAllocationDetail_ProjectOrigin_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			PMAllocationDetail row = e.Row as PMAllocationDetail;
			if (row != null && row.ProjectOrigin == PMOrigin.Source)
			{
				row.ProjectID = null;
			}
		}

		protected virtual void PMAllocationDetail_OffsetProjectOrigin_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			PMAllocationDetail row = e.Row as PMAllocationDetail;
			if (row != null && row.OffsetProjectOrigin == PMOrigin.Source)
			{
				row.OffsetProjectID = null;
			}
		}

		protected virtual void PMAllocationDetail_OffsetAccountGroupOrigin_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			PMAllocationDetail row = e.Row as PMAllocationDetail;
			if (row != null)
			{
				if (row.OffsetAccountGroupOrigin != PMOrigin.Change)
				{
					row.OffsetAccountGroupID = null;
				}
			}
		}

		protected virtual void PMAllocationDetail_Method_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			PMAllocationDetail row = e.Row as PMAllocationDetail;
			if (row != null)
			{
				if (row.Method == PMMethod.Budget)
				{
					row.RateTypeID = null;
					row.QtyFormula = null;
					row.BillableQtyFormula = null;
					row.AmountFormula = null;
					row.SelectOption = PMSelectOption.Transaction;
					row.AllocateZeroAmount = false;
					row.Post = true;
					row.DateSource = PMDateSource.Allocation;

				}
				else
				{
					if (row.UpdateGL == false && row.AccountGroupOrigin == PMOrigin.None)
					{
						sender.SetDefaultExt<PMAllocationDetail.accountGroupOrigin>(e.Row);
					}
				}
			}

		}

		protected virtual void PMAllocationDetail_SourceBranchID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			PMAllocationDetail row = e.Row as PMAllocationDetail;
			if (row != null)
			{
				e.NewValue = null;
				e.Cancel = true;
			}
		}

		protected virtual void PMAllocationDetail_TargetBranchID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			PMAllocationDetail row = e.Row as PMAllocationDetail;
			if (row != null)
			{
				e.NewValue = null;
				e.Cancel = true;
			}
		}

		protected virtual void PMAllocationDetail_AccountGroupOrigin_FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			PMAllocationDetail row = e.Row as PMAllocationDetail;
			if (row != null)
			{
				List<string> allowedValues = new List<string>();
				List<string> allowedLabels = new List<string>();

				allowedValues.Add(PMOrigin.Source);
				allowedValues.Add(PMOrigin.Change);

				allowedLabels.Add(Messages.GetLocal(Messages.Origin_Source));
				allowedLabels.Add(Messages.GetLocal(Messages.Origin_Change));

				if (row.UpdateGL == true)
				{
					allowedValues.Add(PMOrigin.FromAccount);
					allowedLabels.Add(Messages.GetLocal(Messages.Origin_FromAccount));
				}
				else if (row.Method == PMMethod.Budget)
				{
					allowedValues.Add(PMOrigin.None);
					allowedLabels.Add(Messages.GetLocal(Messages.Origin_None));
				}

				e.ReturnState = PXStringState.CreateInstance(e.ReturnState, 1, false, typeof(PMAllocationDetail.accountGroupOrigin).Name, false, 1, null,
													allowedValues.ToArray(), allowedLabels.ToArray(), true, PMOrigin.Source);

				((PXStringState)e.ReturnState).Enabled = row.UpdateGL != true;
			}
		}

		protected virtual void PMAllocationDetail_OffsetAccountGroupOrigin_FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			PMAllocationDetail row = e.Row as PMAllocationDetail;
			if (row != null)
			{
				List<string> allowedValues = new List<string>();
				List<string> allowedLabels = new List<string>();

				allowedValues.Add(PMOrigin.Source);
				allowedValues.Add(PMOrigin.Change);

				allowedLabels.Add(Messages.GetLocal(Messages.Origin_Source));
				allowedLabels.Add(Messages.GetLocal(Messages.Origin_Change));

				if (row.UpdateGL == true)
				{
					allowedValues.Add(PMOrigin.FromAccount);
					allowedLabels.Add(Messages.GetLocal(Messages.Origin_FromAccount));
				}
				else
				{
					allowedValues.Add(PMOrigin.None);
					allowedLabels.Add(Messages.GetLocal(Messages.Origin_None));
				}

				e.ReturnState = PXStringState.CreateInstance(e.ReturnState, 1, false, typeof(PMAllocationDetail.offsetAccountGroupOrigin).Name, false, 1, null,
													allowedValues.ToArray(), allowedLabels.ToArray(), true, PMOrigin.Source);

				((PXStringState)e.ReturnState).Enabled = row.UpdateGL != true;
			}
		}

		protected virtual void PMAllocationDetail_Post_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			PMAllocationDetail row = e.Row as PMAllocationDetail;
			if (row != null)
			{
				sender.SetValueExt<PMAllocationDetail.updateGL>(e.Row, row.Post);
			}
		}

		protected virtual void PMAllocationDetail_RangeStart_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			PMAllocationDetail row = e.Row as PMAllocationDetail;
			if (row != null)
			{
				if (row.RangeStart == row.StepID)
				{
					sender.RaiseExceptionHandling<PMAllocationDetail.rangeStart>(e.Row, e.NewValue, new PXSetPropertyException(Messages.RangeOverlapItself));
				}

				if (row.RangeStart > row.StepID)
				{
					sender.RaiseExceptionHandling<PMAllocationDetail.rangeStart>(e.Row, e.NewValue, new PXSetPropertyException(Messages.RangeOverlapFuture));
				}
			}
		}

		protected virtual void PMAllocationDetail_RangeEnd_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			PMAllocationDetail row = e.Row as PMAllocationDetail;
			if (row != null)
			{
				if (row.RangeEnd == row.StepID)
				{
					sender.RaiseExceptionHandling<PMAllocationDetail.rangeEnd>(e.Row, e.NewValue, new PXSetPropertyException(Messages.RangeOverlapItself));
				}

				if (row.RangeEnd > row.StepID)
				{
					sender.RaiseExceptionHandling<PMAllocationDetail.rangeEnd>(e.Row, e.NewValue, new PXSetPropertyException(Messages.RangeOverlapFuture));
				}
			}
		}

		#endregion

		protected virtual void Validate(PMAllocationDetail step)
		{
			if (ValidateErrors(step))
			{
				ValidateWarnings(step);
			}
		}

		/// <summary>
		/// Validate conditions for the given step that raise warnings.
		/// </summary>
		/// <param name="step">Allocation rule</param>
		protected virtual void ValidateWarnings(PMAllocationDetail step)
		{
			//PXUIFieldAttribute.SetError<PMAllocationStep.target>(Step.Cache, step, null);

			if (step.UpdateGL == true)
			{
				if (step.AccountID != null && step.OffsetAccountID != null && step.AccountID == step.OffsetAccountID)
				{
					Step.Cache.RaiseExceptionHandling<PMAllocationDetail.accountID>(step, null, new PXSetPropertyException(Messages.DebitAccountEqualCreditAccount, PXErrorLevel.RowWarning));
				}
			}
			else
			{
				if (step.AccountGroupID != null && step.OffsetAccountGroupID != null && step.AccountGroupID == step.OffsetAccountGroupID)
				{
					Step.Cache.RaiseExceptionHandling<PMAllocationDetail.accountID>(step, null, new PXSetPropertyException(Messages.DebitAccountGroupEqualCreditAccountGroup, PXErrorLevel.RowWarning));
				}
			}
		}

		/// <summary>
		/// Validate conditions for the given step that raise errors.
		/// </summary>
		/// <param name="step">Allocation rule</param>
		/// <returns>True if valid</returns>
		protected virtual bool ValidateErrors(PMAllocationDetail step)
		{
			bool valid = true;

			if (step.SelectOption == PMSelectOption.Step)
			{
				if (step.RangeStart == null)
				{
					valid = false;
					Step.Cache.RaiseExceptionHandling<PMAllocationDetail.rangeStart>(step, null, new PXException(ErrorMessages.FieldIsEmpty, $"[{nameof(PMAllocationDetail.rangeStart)}]"));
				}

				if (step.RangeEnd == null)
				{
					valid = false;
					Step.Cache.RaiseExceptionHandling<PMAllocationDetail.rangeEnd>(step, null, new PXException(ErrorMessages.FieldIsEmpty, $"[{nameof(PMAllocationDetail.rangeEnd)}]"));
				}

				if (step.RangeStart == step.StepID)
				{
					valid = false;
					Step.Cache.RaiseExceptionHandling<PMAllocationDetail.rangeStart>(step, step.RangeStart, new PXSetPropertyException(Messages.RangeOverlapItself));
				}

				if (step.RangeStart > step.StepID)
				{
					valid = false;
					Step.Cache.RaiseExceptionHandling<PMAllocationDetail.rangeStart>(step, step.RangeStart, new PXSetPropertyException(Messages.RangeOverlapFuture));
				}

				if (step.RangeEnd == step.StepID)
				{
					valid = false;
					Step.Cache.RaiseExceptionHandling<PMAllocationDetail.rangeEnd>(step, step.RangeEnd, new PXSetPropertyException(Messages.RangeOverlapItself));
				}

				if (step.RangeEnd > step.StepID)
				{
					valid = false;
					Step.Cache.RaiseExceptionHandling<PMAllocationDetail.rangeEnd>(step, step.RangeEnd, new PXSetPropertyException(Messages.RangeOverlapFuture));
				}

			}
			else
			{
				if (step.AccountGroupFrom == null)
				{
					valid = false;
					Step.Cache.RaiseExceptionHandling<PMAllocationDetail.accountGroupFrom>(step, null, new PXException(ErrorMessages.FieldIsEmpty, $"[{nameof(PMAllocationDetail.accountGroupFrom)}]"));
				}

			}

			if (step.AccountGroupOrigin == PMOrigin.Change && step.AccountGroupID == null)
			{
				valid = false;
				Step.Cache.RaiseExceptionHandling<PMAllocationDetail.accountGroupID>(step, step.AccountGroupID, new PXSetPropertyException(ErrorMessages.FieldIsEmpty, $"[{nameof(PMAllocationDetail.accountGroupID)}]"));
			}

			if (step.OffsetBranchOrigin == PMOrigin.Change && step.TargetBranchID == null)
			{
				valid = false;
				Step.Cache.RaiseExceptionHandling<PMAllocationDetail.targetBranchID>(step, step.TargetBranchID, new PXSetPropertyException(ErrorMessages.FieldIsEmpty, $"[{nameof(PMAllocationDetail.targetBranchID)}]"));
			}

			if (step.OffsetAccountGroupOrigin == PMOrigin.Change && step.OffsetAccountGroupID == null)
			{
				valid = false;
				Step.Cache.RaiseExceptionHandling<PMAllocationDetail.offsetAccountGroupID>(step, step.OffsetAccountGroupID, new PXSetPropertyException(ErrorMessages.FieldIsEmpty, $"[{nameof(PMAllocationDetail.offsetAccountGroupID)}]"));
			}

			if (step.UpdateGL != true)
			{
				if (step.Method == PMMethod.Transaction)
				{
					if (step.AccountGroupOrigin == PMOrigin.Change && step.AccountGroupID == null)
					{
						valid = false;
						Step.Cache.RaiseExceptionHandling<PMAllocationDetail.accountGroupID>(step, step.AccountGroupID,
																					   new PXException(
																						Messages.DebitAccountGroupIsRequired, step.StepID));
					}
				}
				else
				{
					if (step.AccountGroupOrigin == PMOrigin.None && step.OffsetAccountGroupOrigin == PMOrigin.None)
					{
						valid = false;
						Step.Cache.RaiseExceptionHandling<PMAllocationDetail.accountGroupID>(step, step.AccountGroupID,
																					   new PXException(
																						Messages.AtleastOneAccountGroupIsRequired, step.StepID));
					}
				}
			}

			return valid;
		}
	}
}
