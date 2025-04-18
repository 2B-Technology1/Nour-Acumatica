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
using PX.Objects.GL;
using PX.Objects.IN;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PX.Objects.PM
{
    
	public class INReceiptEntryExt : ProjectCostCenterSupport<INReceiptEntry>
	{
		public PXSetupOptional<PMSetup> Setup;
		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<CS.FeaturesSet.projectAccounting>();
		}

		protected virtual bool IsPMVisible
		{
			get
			{
				return Setup.Current?.VisibleInIN == true;
			}
		}
				

		protected virtual void _(Events.FieldDefaulting<INTran, INTran.projectID> e)
		{
			INTran row = e.Row as INTran;
			if (row == null) return;

			if (PM.ProjectAttribute.IsPMVisible(BatchModule.IN))
			{
				if (row.LocationID != null)
				{
					InventoryItem item = InventoryItem.PK.Find(Base, e.Row.InventoryID);
					if (item != null && item.StkItem != true)
						return;

					PXResultset<INLocation> result = PXSelectReadonly2<INLocation,
						LeftJoin<PMProject,
							On<PMProject.contractID, Equal<INLocation.projectID>>>,
						Where<INLocation.siteID, Equal<Required<INLocation.siteID>>,
						And<INLocation.locationID, Equal<Required<INLocation.locationID>>>>>.Select(e.Cache.Graph, row.SiteID, row.LocationID);

					foreach (PXResult<INLocation, PMProject> res in result)
					{
						PMProject project = (PMProject)res;
						if (project != null && project.ContractCD != null && project.VisibleInIN == true)
						{
							e.NewValue = project.ContractCD;
							return;
						}
					}
				}
			}
		}

		protected virtual void _(Events.FieldVerifying<INTran, INTran.projectID> e)
		{
			if (e.Row.InvtMult != 0 && e.NewValue != null && e.NewValue is int?)
			{
				PMProject project = PMProject.PK.Find(Base, (int?)e.NewValue);
				if (project != null && project.NonProject != true && project.AccountingMode == ProjectAccountingModes.Linked)
				{
					if (e.Row.LocationID != null)
					{
						INLocation location = INLocation.PK.Find(Base, e.Row.LocationID);
						if (location != null)
						{
							if (location.ProjectID != project.ContractID && 
								(location.ProjectID != null || PXAccess.FeatureInstalled<FeaturesSet.materialManagement>()))
							{
								InventoryItem item = InventoryItem.PK.Find(Base, e.Row.InventoryID);
								if (item?.StkItem == true)
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
		}

		protected virtual void _(Events.FieldDefaulting<INTran, INTran.taskID> e)
		{
			INTran row = e.Row as INTran;
			if (row == null) return;

			if (PM.ProjectAttribute.IsPMVisible(BatchModule.IN))
			{
				if (row.LocationID != null)
				{
					InventoryItem item = InventoryItem.PK.Find(Base, e.Row.InventoryID);
					if (item != null && item.StkItem != true)
						return;

					PXResultset<INLocation> result = PXSelectReadonly2<INLocation,
						LeftJoin<PMTask,
							On<PMTask.projectID, Equal<INLocation.projectID>,
							And<PMTask.taskID, Equal<INLocation.taskID>>>>,
						Where<INLocation.siteID, Equal<Required<INLocation.siteID>>,
						And<INLocation.locationID, Equal<Required<INLocation.locationID>>>>>.Select(e.Cache.Graph, row.SiteID, row.LocationID);

					foreach (PXResult<INLocation, PMTask> res in result)
					{
						PMTask task = (PMTask)res;
						if (task != null && task.TaskCD != null && task.VisibleInIN == true && task.IsActive == true)
						{
							e.NewValue = task.TaskCD;
							return;
						}
					}

				}
			}
		}

		protected virtual void _(Events.FieldUpdated<INTran, INTran.locationID> e)
		{
			INTran row = e.Row as INTran;
			if (row == null) return;

			InventoryItem item = InventoryItem.PK.Find(Base, e.Row.InventoryID);
			if (item?.StkItem == true)
			{
				e.Cache.SetDefaultExt<INTran.projectID>(e.Row); //will set pending value for TaskID to null if project is changed. This is the desired behavior for all other screens.
				if (e.Cache.GetValuePending<INTran.taskID>(e.Row) == null) //To redefault the TaskID in currecnt screen - set the Pending value from NULL to NOTSET
					e.Cache.SetValuePending<INTran.taskID>(e.Row, PXCache.NotSetValue);
				e.Cache.SetDefaultExt<INTran.taskID>(e.Row);
			}
		}

		protected virtual void _(Events.FieldVerifying<INTran, INTran.reasonCode> e)
		{
			INTran row = e.Row as INTran;
			if (row != null)
			{
				ReasonCode reasoncd = ReasonCode.PK.Find(Base, (string)e.NewValue);

				if (reasoncd != null && row.ProjectID != null && !ProjectDefaultAttribute.IsNonProject(row.ProjectID))
				{
					PX.Objects.GL.Account account = PXSelect<PX.Objects.GL.Account, Where<PX.Objects.GL.Account.accountID, Equal<Required<PX.Objects.GL.Account.accountID>>>>.Select(Base, reasoncd.AccountID);
					if (account != null && account.AccountGroupID == null)
					{
						e.Cache.RaiseExceptionHandling<INTran.reasonCode>(e.Row, account.AccountCD, new PXSetPropertyException(PM.Messages.NoAccountGroup, PXErrorLevel.Warning, account.AccountCD));
					}
				}
			}
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

		protected virtual void _(Events.RowSelected<INRegister> e)
		{
			if (e.Row == null)
			{
				return;
			}
						
			PXUIFieldAttribute.SetVisible<INTran.projectID>(Base.transactions.Cache, null, IsPMVisible);
			PXUIFieldAttribute.SetVisible<INTran.taskID>(Base.transactions.Cache, null, IsPMVisible);
		}

		protected virtual void _(Events.RowPersisting<INTran> e)
		{
			INTran row = (INTran)e.Row;
			
			if (((e.Operation & PXDBOperation.Command) == PXDBOperation.Insert || (e.Operation & PXDBOperation.Command) == PXDBOperation.Update))
			{
				PMProject project = PMProject.PK.Find(Base, e.Row.ProjectID);
				if (project.AccountingMode == ProjectAccountingModes.Linked)
				{
					CheckForSingleLocation(e.Cache, e.Row);
					CheckSplitsForSameTask(e.Cache, e.Row);
					CheckLocationTaskRule(e.Cache, e.Row);
				}
				
			}
		}

		protected virtual void CheckLocationTaskRule(PXCache sender, INTran row)
		{
			if (row.TaskID != null)
			{
				INLocation selectedLocation = INLocation.PK.Find(Base, row.LocationID);
				if (selectedLocation != null && selectedLocation.TaskID != row.TaskID && selectedLocation.TaskID != null)
				{
					sender.RaiseExceptionHandling<INTran.locationID>(row, selectedLocation.LocationCD,
						new PXSetPropertyException(IN.Messages.LocationIsMappedToAnotherTask, PXErrorLevel.Warning));
				}
			}
		}

		protected virtual void CheckForSingleLocation(PXCache sender, INTran row)
		{
			InventoryItem item = InventoryItem.PK.Find(Base, row.InventoryID);
			if (item != null && item.StkItem == true && !PO.POLineType.IsProjectDropShip(row.POLineType) && row.TaskID != null && row.LocationID == null)
			{
				sender.RaiseExceptionHandling<INTran.locationID>(row, null, new PXSetPropertyException(IN.Messages.RequireSingleLocation));
			}
		}

		[Obsolete]
		protected virtual void CheckSplitsForSameTask(PXCache sender, INTran row)
		{
		}
	}
}
