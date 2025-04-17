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
using PX.Objects.GL;
using PX.Objects.IN;
using static PX.Objects.IN.InventoryRelease.INReleaseProcess;
using System;
using PX.Objects.PM.GraphExtensions;
using PX.Common;
using PX.Objects.IN.InventoryRelease;

namespace PX.Objects.PM.MaterialManagement
{
    public class INReleaseProcessMaterialExt : PXGraphExtension<INReleaseProcessExt, INReleaseProcess>
    {
		public static bool IsActive()
        {
            return PXAccess.FeatureInstalled<CS.FeaturesSet.materialManagement>();
        }

		[PXOverride]
		public virtual void UpdateCrossReference(INTran tran, INTranSplit split, InventoryItem item, INLocation whseloc,
			Action<INTran, INTranSplit, InventoryItem, INLocation> baseMethod)
		{
			baseMethod(tran, split, item, whseloc);

			if (tran.IsSpecialOrder != true && GetAccountingMode(tran.ProjectID) == ProjectAccountingModes.Valuated)
			{
				split.CostSiteID = (item.ValMethod != INValMethod.Standard && item.ValMethod != INValMethod.Specific && whseloc.IsCosted == true ? whseloc.LocationID : split.SiteID);
			}
		}


		[PXOverride]
        public virtual void UpdateSplitDestinationLocation(INTran tran, INTranSplit split, int? value, Action<INTran, INTranSplit, int?> baseMethod)
        {
            baseMethod(tran, split, value);

            if (split.SkipCostUpdate == true && CostLayerType.Project.IsIn(tran.CostLayerType, tran.ToCostLayerType))
            {
                if (tran.CostCenterID != CostCenter.FreeStock || tran.ToCostCenterID != CostCenter.FreeStock)
                {
                    split.SkipCostUpdate = false;
                    Base.intransplit.Cache.MarkUpdated(split, assertError: true);
                }
            }
        }

        [PXOverride]
        public virtual GLTran InsertGLCostsDebit(JournalEntry je, GLTran tran, GLTranInsertionContext context, Func<JournalEntry, GLTran, GLTranInsertionContext, GLTran> baseMethod)
        {
            if (!ProjectDefaultAttribute.IsNonProject(context.INTran.ProjectID) && IsProjectAccount(tran.AccountID))
            {
                string accountingMode = GetAccountingMode(context.INTran.ProjectID);
                if (accountingMode == ProjectAccountingModes.Linked)
                {
                    WriteLinkedCostsDebitTarget(tran, context);
                }
                else
                {
                    WriteValuatedCostsDebitTarget(tran, context);
                }
            }
            if (context.INTran.CostCodeID != null)
                tran.CostCodeID = context.INTran.CostCodeID;

            return baseMethod(je, tran, context);
        }

		[PXOverride]
		public virtual INCostStatus AccumulatedCostStatus(INTran tran, INTranSplit split, InventoryItem item, Func<INTran, INTranSplit, InventoryItem, INCostStatus> baseMethod)
		{
			INCostStatus result = baseMethod(tran, split, item);

			if (item.ValMethod == INValMethod.Standard &&
				tran.TranType != INTranType.NegativeCostAdjustment &&
				tran.CostCenterID == result.CostSiteID &&
				tran.CostLayerType == CostLayerType.Project)
			{
				INItemSite itemsite = INReleaseProcess.SelectItemSite(Base, tran.InventoryID, tran.SiteID);

				if (itemsite != null)
				{
					result.UnitCost = itemsite.StdCost;
				}
			}

			return result;
		}

		private void WriteLinkedCostsDebitTarget(GLTran tran, GLTranInsertionContext context)
        {
            int? locProjectID;
            int? locTaskID = null;
            if (context.Location != null && context.Location.ProjectID != null)//can be null if Adjustment
            {
                locProjectID = context.Location.ProjectID;
                locTaskID = context.Location.TaskID;

                if (locTaskID == null)//Location with ProjectTask WildCard
                {
                    if (context.Location.ProjectID == context.INTran.ProjectID)
                    {
                        locTaskID = context.INTran.TaskID;
                    }
                    else
                    {
                        //substitute with any task from the project.
                        PMTask task = PXSelect<PMTask, Where<PMTask.projectID, Equal<Required<PMTask.projectID>>,
                            And<PMTask.visibleInIN, Equal<True>, And<PMTask.isActive, Equal<True>>>>>.Select(Base, context.Location.ProjectID);
                        if (task != null)
                        {
                            locTaskID = task.TaskID;
                        }
                    }
                }

            }
            else
            {
                locProjectID = PM.ProjectDefaultAttribute.NonProject();
            }

            if (context.TranCost.TranType == INTranType.Adjustment || context.TranCost.TranType == INTranType.Transfer)
            {
                tran.ProjectID = locProjectID;
                tran.TaskID = locTaskID;
            }
            else
            {
                tran.ProjectID = context.INTran.ProjectID ?? locProjectID;
                tran.TaskID = context.INTran.TaskID ?? locTaskID;
            }
            if (context.INTran.CostCodeID != null)
                tran.CostCodeID = context.INTran.CostCodeID;
        }

        private void WriteValuatedCostsDebitTarget(GLTran tran, GLTranInsertionContext context)
        {
            tran.ProjectID = context.INTran.ProjectID;
            tran.TaskID = context.INTran.TaskID;
            if (context.INTran.CostCodeID != null)
                tran.CostCodeID = context.INTran.CostCodeID;
        }

        private bool IsProjectAccount(int? accountID)
        {
            Account account = Account.PK.Find(Base, accountID);
            return account?.AccountGroupID != null;
        }
		        
        private string GetAccountingMode(int? projectID)
        {
            if (projectID != null)
            {
                PMProject project = PMProject.PK.Find(Base, projectID);
                if (project != null && project.NonProject != true)
                {
                    return project.AccountingMode;
                }
            }

            return ProjectAccountingModes.Valuated;
        }
               
        [System.Diagnostics.DebuggerDisplay("{SiteID}.{ProjectID}.{ProjectTaskID}.{CostCodeID}")]
        public struct ProjectCostSiteKey
        {
            public readonly int SiteID;
            public readonly int LocationID;
            public readonly int ProjectID;
            public readonly int ProjectTaskID;
            public readonly int CostCodeID;

            public ProjectCostSiteKey(int siteID, int locationID, int projectID, int projectTaskID, int costCodeID)
            {
                LocationID = locationID;
                SiteID = siteID;
                ProjectID = projectID;
                ProjectTaskID = projectTaskID;
                CostCodeID = costCodeID;
            }

            public override int GetHashCode()
            {
                unchecked // Overflow is fine, just wrap
                {
                    int hash = 17;
                    hash = hash * 23 + SiteID.GetHashCode();
                    hash = hash * 23 + LocationID.GetHashCode();
                    hash = hash * 23 + ProjectID.GetHashCode();
                    hash = hash * 23 + ProjectTaskID.GetHashCode();
                    hash = hash * 23 + CostCodeID.GetHashCode();
                    return hash;
                }
            }
        }

        [System.Diagnostics.DebuggerDisplay("{SiteID}.{LocationID}.{ProjectID}.{ProjectTaskID}")]
        public struct ProjectLocationKey
        {
            public readonly int SiteID;
            public readonly int LocationID;
            public readonly int ProjectID;
            public readonly int ProjectTaskID;
            
            public ProjectLocationKey(int siteID, int locationID, int projectID, int projectTaskID)
            {
                SiteID = siteID;
                LocationID = locationID;
                ProjectID = projectID;
                ProjectTaskID = projectTaskID;
            }

            public override int GetHashCode()
            {
                unchecked // Overflow is fine, just wrap
                {
                    int hash = 17;
                    hash = hash * 23 + SiteID.GetHashCode();
                    hash = hash * 23 + LocationID.GetHashCode();
                    hash = hash * 23 + ProjectID.GetHashCode();
                    hash = hash * 23 + ProjectTaskID.GetHashCode();
                    
                    return hash;
                }
            }
        }
    }

}
