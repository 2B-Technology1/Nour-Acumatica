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
using PX.Objects.AP;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Objects.PO;
using PX.Objects.IN;

namespace PX.Objects.PM
{
	/// <summary>
	/// Extends AP Invoice Entry with Project related functionality. Requires Project Accounting feature and Distribution feature.
	/// </summary>
	public class APInvoiceEntryDistributionExt : PXGraphExtension<APInvoiceEntry>
	{
		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<CS.FeaturesSet.projectAccounting>() && PXAccess.FeatureInstalled<CS.FeaturesSet.distributionModule>();
		}

		public virtual string GetErrorMessage(string poOrderType, string poMesage, string scMessage)
		{
			if (string.IsNullOrWhiteSpace(poOrderType))
			{
				throw new PXArgumentException(nameof(poOrderType));
			}

			if (poOrderType == POOrderType.RegularSubcontract)
			{
				return scMessage;
			}
			else
			{
				return poMesage;
			}
		}

		public virtual void POOrderRS_Selected_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			if ((bool)e.NewValue == true)
			{
				POOrder row = (POOrder)e.Row;
				List<POLineS> orderLines = Base.GetPOOrderLines(row, Base.Document.Current, true);

				HashSet<string> inactiveProjects = new HashSet<string>();
				HashSet<string> inactiveProjectTasks = new HashSet<string>();
				foreach (POLineS line in orderLines)
				{
					if (line.TaskID != null)
					{
						PMProject project = PXSelect<PMProject, Where<PMProject.contractID, Equal<Required<PMProject.contractID>>>>.Select(Base, line.ProjectID);
						if (project.Status != ProjectStatus.Active)
						{
							inactiveProjects.Add(string.Format(Messages.ProjectTraceItem, project.ContractCD));
						}
						else
						{
							PMTask task = PXSelect<PMTask, Where<PMTask.projectID, Equal<Required<PMTask.projectID>>,
								And<PMTask.taskID, Equal<Required<PMTask.taskID>>>>>.Select(Base, line.ProjectID, line.TaskID);
							if (task.Status != ProjectTaskStatus.Active)
							{
								inactiveProjectTasks.Add(string.Format(Messages.ProjectTaskTraceItem, project.ContractCD, task.TaskCD));
							}
						}
					}
				}

				if (inactiveProjects.Count > 0)
				{
					string errorMessage = GetErrorMessage(row.OrderType, Messages.POCommitmentProjectIsNotActive, Messages.SCCommitmentProjectIsNotActive);
					string traceMessage =
						errorMessage + Environment.NewLine +
						string.Join(Environment.NewLine, inactiveProjects.OrderBy(p => p));
					PXTrace.WriteError(traceMessage);
					throw new PXSetPropertyException(errorMessage, PXErrorLevel.RowError);
				}
				else if (inactiveProjectTasks.Count > 0)
				{
					string errorMessage = GetErrorMessage(row.OrderType, Messages.POCommitmentProjectTaskIsNotActive, Messages.SCCommitmentProjectTaskIsNotActive);
					string traceMessage =
						errorMessage + Environment.NewLine +
						string.Join(Environment.NewLine, inactiveProjectTasks.OrderBy(p => p));
					PXTrace.WriteError(traceMessage);
					throw new PXSetPropertyException(errorMessage, PXErrorLevel.RowError);
				}
			}
		}

		public virtual void POLineRS_Selected_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			POLineRS line = (POLineRS)e.Row;

			if ((bool)e.NewValue == true && line.TaskID != null)
			{
				PMProject project = PXSelect<PMProject, Where<PMProject.contractID, Equal<Required<PMProject.contractID>>>>.Select(Base, line.ProjectID);
				if (project.Status != ProjectStatus.Active)
				{
					throw new PXSetPropertyException(GetErrorMessage(line.OrderType, Messages.POCommitmentLineProjectIsNotActive, Messages.SCCommitmentLineProjectIsNotActive),
						PXErrorLevel.RowError,
						string.Empty,
						project.ContractCD);
				}
				else
				{
					PMTask task = PXSelect<PMTask, Where<PMTask.projectID, Equal<Required<PMTask.projectID>>,
								And<PMTask.taskID, Equal<Required<PMTask.taskID>>>>>.Select(Base, line.ProjectID, line.TaskID);
					if (task.Status != ProjectTaskStatus.Active)
					{
						throw new PXSetPropertyException(GetErrorMessage(line.OrderType, Messages.POCommitmentLineProjectTaskIsNotActive, Messages.SCCommitmentLineProjectTaskIsNotActive),
							PXErrorLevel.RowError,
							string.Empty,
							task.TaskCD);
					}
				}
			}
		}

		public virtual void POReceipt_Selected_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			if ((bool)e.NewValue == true)
			{
				POReceipt row = (POReceipt)e.Row;
				List<POReceiptLineS> receiptLines = Base.GetInvoicePOReceiptLines(row, Base.Document.Current);

				HashSet<string> inactiveProjects = new HashSet<string>();
				HashSet<string> inactiveProjectTasks = new HashSet<string>();
				foreach (POReceiptLineS line in receiptLines)
				{
					if (line.TaskID != null)
					{
						PMProject project = PXSelect<PMProject, Where<PMProject.contractID, Equal<Required<PMProject.contractID>>>>.Select(Base, line.ProjectID);
						if (project.IsActive != true)
						{
							inactiveProjects.Add(string.Format(Messages.ProjectTraceItem, project.ContractCD));
						}
						else
						{
							PMTask task = PXSelect<PMTask, Where<PMTask.projectID, Equal<Required<PMTask.projectID>>,
								And<PMTask.taskID, Equal<Required<PMTask.taskID>>>>>.Select(Base, line.ProjectID, line.TaskID);
							if (task.IsActive != true)
							{
								inactiveProjectTasks.Add(string.Format(Messages.ProjectTaskTraceItem, project.ContractCD, task.TaskCD));
							}
						}
					}
				}

				if (inactiveProjects.Count > 0)
				{
					string traceMessage =
						Messages.POReceiptWithProjectIsNotActiveTraceCaption + Environment.NewLine +
						string.Join(Environment.NewLine, inactiveProjects.OrderBy(p => p));
					PXTrace.WriteError(traceMessage);
					throw new PXSetPropertyException(Messages.POReceiptWithProjectIsNotActive, PXErrorLevel.RowError);
				}
				else if (inactiveProjectTasks.Count > 0)
				{
					string traceMessage =
						Messages.POReceiptWithProjectTaskIsNotActiveTraceCaption + Environment.NewLine +
						string.Join(Environment.NewLine, inactiveProjectTasks.OrderBy(p => p));
					PXTrace.WriteError(traceMessage);
					throw new PXSetPropertyException(Messages.POReceiptWithProjectTaskIsNotActive, PXErrorLevel.RowError);
				}
			}
		}

		public virtual void POReceiptLineAdd_Selected_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			POReceiptLineS line = (POReceiptLineS)e.Row;

			string errorMessage = null;
			if ((bool)e.NewValue == true && line.TaskID != null)
			{
				PMProject project = PXSelect<PMProject, Where<PMProject.contractID, Equal<Required<PMProject.contractID>>>>.Select(Base, line.ProjectID);
				if (project.IsActive != true)
				{
					errorMessage = PO.Messages.ProjectIsNotActive;
				}
				else
				{
					PMTask task = PXSelect<PMTask, Where<PMTask.projectID, Equal<Required<PMTask.projectID>>,
								And<PMTask.taskID, Equal<Required<PMTask.taskID>>>>>.Select(Base, line.ProjectID, line.TaskID);
					if (task.IsActive != true)
					{
						errorMessage = PO.Messages.ProjectTaskIsNotActive;
					}
				}
			}

			if (errorMessage != null)
			{
				throw new PXSetPropertyException(errorMessage, PXErrorLevel.RowError);
			}
		}
	}
}
