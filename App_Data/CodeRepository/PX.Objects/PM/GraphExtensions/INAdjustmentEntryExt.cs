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
using PX.Objects.Common.Scopes;
using PX.Objects.CS;
using PX.Objects.IN;
using System;
using System.Collections.Generic;

namespace PX.Objects.PM
{
    public class INAdjustmentEntryExt : ProjectCostCenterSupport<INAdjustmentEntry>
	{
		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<FeaturesSet.projectModule>();
		}

		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXVerifySelector(typeof(Search2<INCostStatus.receiptNbr,
			InnerJoin<INCostSubItemXRef, On<INCostSubItemXRef.costSubItemID, Equal<INCostStatus.costSubItemID>>,
			InnerJoin<INLocation, On<INLocation.locationID, Equal<Optional<INTran.locationID>>>,
			InnerJoin<PMProject, On<PMProject.contractID, Equal<Optional<INTran.projectID>>>>>>,
			Where<INCostStatus.inventoryID, Equal<Optional<INTran.inventoryID>>,
			And<INCostSubItemXRef.subItemID, Equal<Optional<INTran.subItemID>>,
			And<
			Where2<Where<INCostStatus.costSiteID, Equal<Optional<INTran.siteID>>,
				And2<Where<Optional<INTran.costCenterID>, Equal<CostCenter.freeStock>,
					Or<PMProject.accountingMode, Equal<ProjectAccountingModes.valuated>>>,
				And<INLocation.isCosted, Equal<False>,
				Or<INCostStatus.costSiteID, Equal<Optional<INTran.locationID>>>>>>,
			Or<INCostStatus.costSiteID, Equal<Optional<INTran.costCenterID>>>>
				>>>>), VerifyField = false)]
		protected virtual void _(Events.CacheAttached<INTran.origRefNbr> e) { }

		private List<PXTaskSetPropertyException> _taskExceptions = new List<PXTaskSetPropertyException>();

		public bool IsTaskErrorsHandlingIsEnabled { get; set; }

		public List<PXTaskSetPropertyException> TaskExceptions
		{
			get { return _taskExceptions; }
		}

		/// <summary>
		/// Overrides <see cref="INRegisterEntryBase.IsProjectTaskEnabled(INTran)" />
		/// </summary>
		[PXOverride]
		public virtual (bool? Project, bool? Task) IsProjectTaskEnabled(INTran row,
			Func<INTran, (bool? Project, bool? Task)> baseMethod)
		{
			var result = baseMethod(row);

			LinkedInfo info = GetLinkedInfo(row.LocationID);
			return ((result.Project ?? true) && !info.IsLinked, (result.Task ?? true) && !info.IsTaskRestricted);
		}

		protected virtual void _(Events.FieldDefaulting<INTran, INTran.projectID> e)
		{
			if (SkipDefaultingFromLocationScope.IsActive)
				return;

			INTran row = e.Row as INTran;
			if (row == null) return;

			if (row.LocationID != null)
			{
				INLocation location = INLocation.PK.Find(Base, row.LocationID);
				if (location.ProjectID != null)
				{
					e.NewValue = location.ProjectID;
				}
			}
		}

		protected virtual void _(Events.FieldDefaulting<INTran, INTran.taskID> e)
		{
			if (SkipDefaultingFromLocationScope.IsActive)
				return;

			INTran row = e.Row as INTran;
			if (row == null) return;

			if (row.LocationID != null)
			{
				INLocation location = INLocation.PK.Find(Base, row.LocationID);
				if (location.TaskID != null)
				{
					InvokeActionWithTaskErrorsHandling(() =>
					{
						ActiveProjectTaskAttribute.VerifyTaskIsActive(Base, location.ProjectID, location.TaskID);
					});

					e.NewValue = location.TaskID;
				}
				else if (location.ProjectID != null)
                {
					PMTask firstTask = PXSelect<PMTask, Where<PMTask.projectID, Equal<Required<PMTask.projectID>>>>.SelectWindowed(Base, 0, 1, location.ProjectID);
					if (firstTask != null)
                    {
						InvokeActionWithTaskErrorsHandling(() =>
						{
							ActiveProjectTaskAttribute.VerifyTaskIsActive(Base, location.ProjectID, firstTask.TaskID);
						});

						e.NewValue = firstTask.TaskID;
					}
                }
			}
		}

		protected virtual void _(Events.FieldVerifying<INTran, INTran.projectID> e)
        {
			if (e.NewValue != null && e.NewValue is int?)
            {
				PMProject project = PMProject.PK.Find(Base, (int?) e.NewValue);
				if (project != null && project.NonProject != true && project.AccountingMode == ProjectAccountingModes.Linked)
                {
					if ( e.Row.LocationID != null)
                    {
						INLocation location = INLocation.PK.Find(Base, e.Row.LocationID);
						if (location != null)
                        {
							if (location.ProjectID != project.ContractID && 
								(location.ProjectID != null || PXAccess.FeatureInstalled<FeaturesSet.materialManagement>()))
                            {
								var ex = new PXSetPropertyException<INTran.projectID>(Messages.LinkedProjectNotValid, PXErrorLevel.Error, project.ContractCD, location.LocationCD);
								ex.ErrorValue = project.ContractCD;

								throw ex;
                            }
                        }
					}
                }
            }
        }

		protected virtual void _(Events.FieldUpdated<INTran, INTran.locationID> e)
		{
			INTran row = e.Row as INTran;
			if (row == null) return;

			e.Cache.SetDefaultExt<INTran.projectID>(e.Row); //will set pending value for TaskID to null if project is changed. This is the desired behavior for all other screens.
			if (e.Cache.GetValuePending<INTran.taskID>(e.Row) == null) //To redefault the TaskID in currecnt screen - set the Pending value from NULL to NOTSET
				e.Cache.SetValuePending<INTran.taskID>(e.Row, PXCache.NotSetValue);

			InvokeActionWithTaskErrorsHandling(() =>
			{
				e.Cache.SetDefaultExt<INTran.taskID>(e.Row);
			});
		}

		protected virtual void _(Events.FieldDefaulting<INTranSplit, INTranSplit.projectID> e)
		{
			INTran parent = PXParentAttribute.SelectParent(e.Cache, e.Row, typeof(INTran)) as INTran;
			if (parent != null)
			{
				e.NewValue = parent.ProjectID;
			}
		}

		protected virtual void _(Events.FieldDefaulting<INTranSplit, INTranSplit.taskID> e)
		{
			INTran parent = PXParentAttribute.SelectParent(e.Cache, e.Row, typeof(INTran)) as INTran;
			if (parent != null)
			{
				e.NewValue = parent.TaskID;
			}
		}

		private LinkedInfo GetLinkedInfo(int? locationID)
		{
			LinkedInfo result = new LinkedInfo();
			if (locationID != null)
			{
				INLocation location = INLocation.PK.Find(Base, locationID);
				if (location != null && location.ProjectID != null)
				{
					PMProject project = PMProject.PK.Find(Base, location.ProjectID);
					if (project != null)
					{
						result.IsLinked = project.AccountingMode == ProjectAccountingModes.Linked;
						result.IsTaskRestricted = location.TaskID != null;
					}
				}
			}

			return result;
		}

		private void InvokeActionWithTaskErrorsHandling(Action action)
		{
			try
			{
				action();
			}
			catch (PXTaskSetPropertyException ex)
			{
				if (IsTaskErrorsHandlingIsEnabled)
				{
					_taskExceptions.Add(ex);
				}
				else
				{
					throw;
				}
			}
		}

		private struct LinkedInfo
		{
			public bool IsLinked;
			public bool IsTaskRestricted;
		}
	}

	public class SkipDefaultingFromLocationScope : FlaggedModeScopeBase<SkipDefaultingFromLocationScope> { }

	[Obsolete]
	public class PPVAdjustmentScope : FlaggedModeScopeBase<PPVAdjustmentScope> { }
}
