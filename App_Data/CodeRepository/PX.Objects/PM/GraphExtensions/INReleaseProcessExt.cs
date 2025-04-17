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
using PX.Objects.IN.InventoryRelease;

namespace PX.Objects.PM.GraphExtensions
{
    public class INReleaseProcessExt : PXGraphExtension<INReleaseProcess>
    {     
        public static bool IsActive()
        {
            return PXAccess.FeatureInstalled<CS.FeaturesSet.projectAccounting>();
        }

        [PXOverride]
        public virtual GLTran InsertGLSalesDebit(JournalEntry je, GLTran tran, GLTranInsertionContext context, Func<JournalEntry, GLTran, GLTranInsertionContext, GLTran> baseMethod)
        {
            if (IsProjectAccount(tran.AccountID))
            {
                tran.ProjectID = context.INTran.ProjectID;
                tran.TaskID = context.INTran.TaskID;
                if (context.INTran.CostCodeID != null)
                    tran.CostCodeID = context.INTran.CostCodeID;
            }

            return baseMethod(je, tran, context);
        }

        [PXOverride]
        public virtual GLTran InsertGLSalesCredit(JournalEntry je, GLTran tran, GLTranInsertionContext context, Func<JournalEntry, GLTran, GLTranInsertionContext, GLTran> baseMethod)
        {
            if (IsProjectAccount(tran.AccountID))
            {
                tran.ProjectID = context.INTran.ProjectID;
                tran.TaskID = context.INTran.TaskID;
                if (context.INTran.CostCodeID != null)
                    tran.CostCodeID = context.INTran.CostCodeID;
            }

            return baseMethod(je, tran, context);
        }

        [PXOverride]
        public virtual GLTran InsertGLNonStockCostDebit(JournalEntry je, GLTran tran, GLTranInsertionContext context, Func<JournalEntry, GLTran, GLTranInsertionContext, GLTran> baseMethod)
        {
            if (context.INTran.AccrueCost != true && IsProjectAccount(tran.AccountID))
            {
                tran.ProjectID = context.INTran.ProjectID;
                tran.TaskID = context.INTran.TaskID;
                if (context.INTran.CostCodeID != null)
                    tran.CostCodeID = context.INTran.CostCodeID;
            }

            return baseMethod(je, tran, context);
        }

        [PXOverride]
        public virtual GLTran InsertGLNonStockCostCredit(JournalEntry je, GLTran tran, GLTranInsertionContext context, Func<JournalEntry, GLTran, GLTranInsertionContext, GLTran> baseMethod)
        {
            if (IsProjectAccount(tran.AccountID))
            {
                tran.ProjectID = context.INTran.ProjectID;
                tran.TaskID = context.INTran.TaskID;
                if (context.INTran.CostCodeID != null)
                    tran.CostCodeID = context.INTran.CostCodeID;
            }

            return baseMethod(je, tran, context);
        }

        [PXOverride]
        public virtual GLTran InsertGLCostsDebit(JournalEntry je, GLTran tran, GLTranInsertionContext context, Func<JournalEntry, GLTran, GLTranInsertionContext, GLTran> baseMethod)
        {
            if (IsProjectAccount(tran.AccountID))
            {
                if (Base.IsOneStepTransfer())
                {
                    InsertGLCostsDebitForOneStepTransfer(je, tran, context);
                }
                else
                {
                    tran.ProjectID = context.INTran.ProjectID;
                    tran.TaskID = context.INTran.TaskID;
                    if (context.INTran.CostCodeID != null)
                        tran.CostCodeID = context.INTran.CostCodeID;
                }
            }

            return baseMethod(je, tran, context);
        }

        private void InsertGLCostsDebitForOneStepTransfer(JournalEntry je, GLTran tran, GLTranInsertionContext context)
        {
            if (context.TranCost.InvtMult == -1)
            {
                tran.ProjectID = context.INTran.ProjectID;
                tran.TaskID = context.INTran.TaskID;
                if (context.INTran.CostCodeID != null)
                    tran.CostCodeID = context.INTran.CostCodeID;
            }
            else
            {
                tran.ProjectID = context.INTran.ToProjectID ?? context.INTran.ProjectID;
				tran.TaskID = context.INTran.ToTaskID ?? context.INTran.TaskID;
				if (context.INTran.CostCodeID != null)
					tran.CostCodeID = context.INTran.CostCodeID;
			}
        }

        [PXOverride]
        public virtual GLTran InsertGLCostsCredit(JournalEntry je, GLTran tran, GLTranInsertionContext context, Func<JournalEntry, GLTran, GLTranInsertionContext, GLTran> baseMethod)
        {
            if (IsProjectAccount(tran.AccountID))
            {
                tran.ProjectID = context.INTran.ProjectID;
                tran.TaskID = context.INTran.TaskID;
                if (context.INTran.CostCodeID != null)
                    tran.CostCodeID = context.INTran.CostCodeID;
            }

            return baseMethod(je, tran, context);
        }
        
        [PXOverride]
        public virtual GLTran InsertGLCostsOversold(JournalEntry je, GLTran tran, GLTranInsertionContext context, Func<JournalEntry, GLTran, GLTranInsertionContext, GLTran> baseMethod)
        {
            if (context.TranCost.TranType == INTranType.Transfer)
            {
                //GIT always to Non-Project.
                tran.ProjectID = context.TranCost.COGSAcctID == null ? PM.ProjectDefaultAttribute.NonProject() : context.INTran.ProjectID;
                tran.TaskID = context.TranCost.COGSAcctID == null ? null : context.INTran.TaskID;
                if (context.INTran.CostCodeID != null)
                    tran.CostCodeID = context.INTran.CostCodeID;
            }
            else
            {
                if (IsProjectAccount(tran.AccountID))
                {
                    tran.ProjectID = context.INTran.ProjectID;
                    tran.TaskID = context.INTran.TaskID;
                    if (context.INTran.CostCodeID != null)
                        tran.CostCodeID = context.INTran.CostCodeID;
                }
            }
            return baseMethod(je, tran, context);
        }

        [PXOverride]
        public virtual GLTran InsertGLCostsVarianceCredit(JournalEntry je, GLTran tran, GLTranInsertionContext context, Func<JournalEntry, GLTran, GLTranInsertionContext, GLTran> baseMethod)
        {
            if (IsProjectAccount(tran.AccountID))
            {
                tran.ProjectID = context.INTran.ProjectID;
                tran.TaskID = context.INTran.TaskID;
                if (context.INTran.CostCodeID != null)
                    tran.CostCodeID = context.INTran.CostCodeID;
            }
            return baseMethod(je, tran, context);
        }

        [PXOverride]
        public virtual GLTran InsertGLCostsVarianceDebit(JournalEntry je, GLTran tran, GLTranInsertionContext context, Func<JournalEntry, GLTran, GLTranInsertionContext, GLTran> baseMethod)
        {
            if (IsProjectAccount(tran.AccountID))
            {
                tran.ProjectID = context.INTran.ProjectID;
                tran.TaskID = context.INTran.TaskID;
                if (context.INTran.CostCodeID != null)
                    tran.CostCodeID = context.INTran.CostCodeID;
            }
            return baseMethod(je, tran, context);
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
                
        
        public bool IsProjectAccount(int? accountID)
        {
            Account account = Account.PK.Find(Base, accountID);
            return account?.AccountGroupID != null;
        }
    }

}
