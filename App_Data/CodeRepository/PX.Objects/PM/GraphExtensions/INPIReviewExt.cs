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
using PX.Objects.IN;
using System;
using System.Collections.Generic;
using System.Linq;


namespace PX.Objects.PM
{
	public class INPIReviewExt : PXGraphExtension<INPIReview>
	{
		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<CS.FeaturesSet.projectModule>();
		}

		[PXOverride]
		public virtual INAdjustmentEntry CreateAdjustmentEntry(Func<INAdjustmentEntry> baseMethod)
		{
			INAdjustmentEntry je = baseMethod();
			INAdjustmentEntryExt ext = je.GetExtension<INAdjustmentEntryExt>();
			if (ext != null)
			{
				ext.IsTaskErrorsHandlingIsEnabled = true;
			}
			return je;
		}

		[PXOverride]
		public virtual void HandleAdjustmentExceptions(INAdjustmentEntry je, List<PXException> exceptions,
			Action<INAdjustmentEntry, List<PXException>> baseMethod)
		{
			INAdjustmentEntryExt ext = je.GetExtension<INAdjustmentEntryExt>();
			if (ext != null)
			{
				ThrowComplexTaskException(ext.TaskExceptions);
			}

			baseMethod(je, exceptions);
		}

		private void ThrowComplexTaskException(List<PXTaskSetPropertyException> taskExceptions)
		{
			var projectsDictionary = new Dictionary<int, string>();
			var inactiveTasksDictionary = new Dictionary<int, HashSet<string>>();

			foreach (var taskException in taskExceptions)
			{
				var taskId = taskException.TaskID;

				if (!taskId.HasValue)
					continue;

				var projectId = taskException.ProjectID;

				if (!projectId.HasValue)
					continue;

				var task = PMTask.PK.Find(Base, projectId, taskId);

				if (task == null)
					continue;

				if (projectsDictionary.TryGetValue(projectId.Value, out _))
				{
					if (inactiveTasksDictionary.TryGetValue(projectId.Value, out var taskList))
					{
						taskList.Add(task.TaskCD);
					}
					else
					{
						inactiveTasksDictionary.Add(task.ProjectID.Value, new HashSet<string> { task.TaskCD });
					}
				}
				else
				{
					var project = PMProject.PK.Find(Base, projectId);

					if (project == null)
						continue;

					projectsDictionary.Add(projectId.Value, project.ContractCD);

					inactiveTasksDictionary.Add(projectId.Value, new HashSet<string> { task.TaskCD });
				}
			}

			if (inactiveTasksDictionary.Count == 0)
			{
				return;
			}

			if (inactiveTasksDictionary.Count == 1)
			{
				var kv = inactiveTasksDictionary.First();
				var projectName = projectsDictionary[kv.Key];
				var taskNames = string.Join(", ", kv.Value);

				if (kv.Value.Count == 1)
				{
					throw new PXException(
						Messages.AdjustmentCannotBeReleasedDueToInactiveTaskInOneProject, projectName, taskNames);
				}

				throw new PXException(
					Messages.AdjustmentCannotBeReleasedDueToInactiveTasksInOneProject, projectName, taskNames);
			}

			throw new PXException(
				Messages.AdjustmentCannotBeReleasedDueToInactiveTasksInManyProjects);
		}
	}
}
