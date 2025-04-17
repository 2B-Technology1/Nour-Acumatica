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
using System.Collections.Generic;
using System.Linq;
using PX.Data;
using PX.Objects.IN;


namespace PX.Objects.PM
{
	public class ProjectCostCenterBase<T> : PXGraphExtension<T>
		where T : PXGraph
	{
		public string GetCostLayerType(INTran tran)
		{
			return GetCostLayerType(tran.ProjectID);
		}

		protected virtual string BuildCostCenterCD(int? siteID, int? projectID, int? taskID)
		{
			var project = PMProject.PK.Find(Base, projectID);
			var task = PMTask.PK.Find(Base, projectID, taskID);

			if (project != null && task != null)
			{
				return string.Format("{0}/{1}", project.ContractCD.Trim(), task.TaskCD.Trim());
			}

			return null;
		}

		protected virtual int? FindOrCreateCostCenter(int? siteID, int? projectID, int? taskID)
		{
			var select = new PXSelect<INCostCenter,
				Where<INCostCenter.siteID, Equal<Required<INCostCenter.siteID>>,
				And<INCostCenter.projectID, Equal<Required<INCostCenter.projectID>>,
				And<INCostCenter.taskID, Equal<Required<INCostCenter.taskID>>>>>>(Base);

			INCostCenter existing = select.Select(siteID, projectID, taskID);

			if (existing != null)
			{
				return existing.CostCenterID;
			}
			else
			{
				return InsertNewCostSite(siteID, projectID, taskID);
			}
		}

		protected virtual int? InsertNewCostSite(int? siteID, int? projectID, int? taskID)
		{

			var costSite = new INCostCenter
			{
				CostLayerType = GetCostLayerType(projectID),
				SiteID = siteID,
				ProjectID = projectID,
				TaskID = taskID,
				CostCenterCD = BuildCostCenterCD(siteID, projectID, taskID)
			};

			costSite = (INCostCenter)Base.Caches<INCostCenter>().Insert(costSite);
			if (costSite != null)
			{
				return costSite.CostCenterID;
			}

			throw new InvalidOperationException("Failed to insert new INCostCenter");
		}

		private string GetCostLayerType(int? projectID)
		{
			return GetAccountingMode(projectID) == ProjectAccountingModes.ProjectSpecific ? CostLayerType.Project : CostLayerType.Normal;
		}

		protected string GetAccountingMode(int? projectID)
		{
			var project = PMProject.PK.Find(Base, projectID);
			if (project != null && project.NonProject != true)
			{
				return project.AccountingMode;
			}

			return ProjectAccountingModes.Valuated;
		}

		protected virtual bool IsSpecificCostCenter(int? siteID, int? projectID, int? taskID)
		{
			return siteID != null && taskID != null && !ProjectDefaultAttribute.IsNonProject(projectID) && GetAccountingMode(projectID) != ProjectAccountingModes.Linked ;
		}
	}
}
