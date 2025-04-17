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
using PX.Objects.CS;
using PX.Objects.DR;
using PX.Objects.GL;
using PX.Objects.IN;
using PX.Objects.PO;
using PX.Objects.TX;

namespace PX.Objects.PM
{
    public class APReleaseProcessExt : CommitmentTracking<APReleaseProcess>
    {
        public static new bool IsActive()
        {
            return PXAccess.FeatureInstalled<FeaturesSet.projectAccounting>();
        }

        [PXOverride]
        public virtual void ReleaseInvoiceTransactionPostProcessing(JournalEntry je, APInvoice apdoc, PXResult<APTran, APTax, Tax, DRDeferredCode, LandedCostCode, InventoryItem, APTaxTran> r, GLTran tran)
        {
            APTran n = (APTran)r;

			if (CopyProjectFromAPTran(apdoc, n))
            {
                tran.ProjectID = n.ProjectID;
                tran.TaskID = n.TaskID;
                tran.CostCodeID = n.CostCodeID;
            }            
        }

        protected virtual bool CopyProjectFromAPTran(APInvoice doc, APTran tran)
        {
            if (doc.IsChildRetainageDocument()) return false;
            Account account = Account.PK.Find(Base, tran.AccountID);
            if (account != null && account.AccountGroupID == null) return false;

            return true;
        }
    }
}
