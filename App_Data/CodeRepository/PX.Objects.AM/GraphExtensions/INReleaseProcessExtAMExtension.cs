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
using PX.Objects.PM.GraphExtensions;
using PX.Objects.GL;
using static PX.Objects.IN.InventoryRelease.INReleaseProcess;
using PX.Objects.PM;
using PX.Objects.AM.CacheExtensions;
using PX.Objects.IN.InventoryRelease;

namespace PX.Objects.AM.GraphExtensions
{
    public class INReleaseProcessExtAMExtension : PXGraphExtension<INReleaseProcessExt, INReleaseProcess>
    {
        public static bool IsActive()
        {
            return PXAccess.FeatureInstalled<CS.FeaturesSet.manufacturing>();
        }

        [PXOverride]
        public virtual GLTran InsertGLCostsDebit(JournalEntry je, GLTran tran, GLTranInsertionContext context, Func<JournalEntry, GLTran, GLTranInsertionContext, GLTran> baseMethod)
        {
			if (!IsProjectAccount(tran.AccountID))
			{
				tran.ProjectID = ProjectDefaultAttribute.NonProject();
				tran.TaskID = null;
				tran.CostCodeID = null;

				return je.GLTranModuleBatNbr.Insert(tran);
			}

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
                var inTranExt = context.INTran.GetExtension<INTranExt>();
                if(context.TranCost.TranType == INTranType.Adjustment && inTranExt?.AMProdOrdID != null)
                {
                    AMProdItem amproditem = PXSelect<AMProdItem, Where<AMProdItem.orderType, Equal<Required<AMProdItem.prodOrdID>>,
                        And<AMProdItem.prodOrdID, Equal<Required<AMProdItem.prodOrdID>>>>>.Select(Base, inTranExt.AMOrderType, inTranExt.AMProdOrdID);
                    if(amproditem?.UpdateProject == true)
                    {
                        tran.ProjectID = context.INTran.ProjectID ?? locProjectID;
                        tran.TaskID = context.INTran.TaskID ?? locTaskID;
                    }
                }
            }
            else
            {
                tran.ProjectID = context.INTran.ProjectID ?? locProjectID;
                tran.TaskID = context.INTran.TaskID ?? locTaskID;
            }
            tran.CostCodeID = context.INTran.CostCodeID;
            return je.GLTranModuleBatNbr.Insert(tran);
        }

		private bool IsProjectAccount(int? accountId)
		{
			return ProjectHelper.IsProjectAcctWithProjectEnabled(Base, accountId);
		}
	}
}
