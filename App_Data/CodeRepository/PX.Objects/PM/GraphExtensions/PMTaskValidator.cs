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
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.CS;
using System;
using System.Collections;

namespace PX.Objects.PM
{
	public abstract class PMTaskValidator<TGraph> : PXGraphExtension<TGraph>
		where TGraph : PXGraph
	{
		public virtual void VerifyCostCodeActive(PMTask task)
		{
			if (task == null)
			{
				return;
			}

			PMBudget budget = SelectFrom<PMBudget>
				.InnerJoin<PMCostCode>.On<PMBudget.costCodeID.IsEqual<PMCostCode.costCodeID>>
				.Where<PMBudget.projectID.IsEqual<P.AsInt>
					.And<PMBudget.projectTaskID.IsEqual<P.AsInt>>
					.And<PMCostCode.isActive.IsNotEqual<True>>>
				.View
				.SelectSingleBound(Base, null, task.ProjectID, task.TaskID);

			if (budget != null)
			{
				var ex = GetCostCodeValidationException(task);
				if (ex != null)
				{
					throw ex;
				}
			}
		}

		public abstract Exception GetCostCodeValidationException(PMTask task);
	}

	public class PMTaskEntryValidator : PMTaskValidator<ProjectTaskEntry>
	{
		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<FeaturesSet.costCodes>();
		}

		[PXOverride]
		public virtual IEnumerable Activate(PXAdapter adapter, Func<PXAdapter, IEnumerable> baseMethod)
		{
			var task = Base.Task.Current;
			if (task != null)
			{
				VerifyCostCodeActive(task);
			}
			return baseMethod(adapter);
		}

		public override Exception GetCostCodeValidationException(PMTask task)
		{
			var project = (PMProject)Base.Project.Select();

			return new PXException(Messages.CannotActivateProjectTaskWithInactiveCostCode,
				task.TaskCD,
				project?.ContractCD ?? string.Empty);
		}
	}

	public class PMProjectEntryTaskValidator : PMTaskValidator<ProjectEntry>
	{
		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<FeaturesSet.costCodes>();
		}

		protected virtual void _(Events.FieldVerifying<PMTask, PMTask.status> e)
		{
			if (e.Row != null && (string)e.NewValue == ProjectTaskStatus.Active)
			{
				VerifyCostCodeActive(e.Row);
			}
		}

		public override Exception GetCostCodeValidationException(PMTask task)
		{
			return new PXSetPropertyException(Messages.CannotActivateTaskWithInactiveCostCode, task.TaskCD);
		}
	}
}
