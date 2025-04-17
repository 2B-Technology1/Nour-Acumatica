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

using System.Collections;
using PX.Data;

namespace PX.Objects.FA
{
     [PX.Objects.GL.TableAndChartDashboardType]
     public class FASplitsInq : PXGraph<FASplitsInq>
    {
        public PXCancel<AccountFilter> Cancel;
        public PXFilter<AccountFilter> Filter;
        public PXSelectJoin<Transact, LeftJoin<FixedAsset, On<Transact.assetID, Equal<FixedAsset.assetID>>>, Where<Transact.origin, Equal<FARegister.origin.split>, And<Transact.released, Equal<True>, And<Where<FixedAsset.assetID, Equal<Current<AccountFilter.assetID>>, And<Transact.tranType, Equal<FATran.tranType.purchasingMinus>, Or<FixedAsset.splittedFrom, Equal<Current<AccountFilter.assetID>>, And<Transact.tranType, Equal<FATran.tranType.purchasingPlus>>>>>>>>> Transactions; 

        public FASplitsInq()
        {
            Transactions.Cache.AllowInsert = false;
            Transactions.Cache.AllowUpdate = false;
            Transactions.Cache.AllowDelete = false;
        }

        public virtual IEnumerable transactions(PXAdapter adapter)
        {
            AccountFilter filter = Filter.Current;
            if (filter == null) yield break;

            PXSelectBase<Transact> cmd = new PXSelectJoin<Transact, LeftJoin<FixedAsset, On<Transact.assetID, Equal<FixedAsset.assetID>>>, Where<Transact.origin, Equal<FARegister.origin.split>, And<Transact.released, Equal<True>, And<Where<FixedAsset.assetID, Equal<Current<AccountFilter.assetID>>, And<Transact.tranType, Equal<FATran.tranType.purchasingMinus>, Or<FixedAsset.splittedFrom, Equal<Current<AccountFilter.assetID>>, And<Transact.tranType, Equal<FATran.tranType.purchasingPlus>>>>>>>>>(this);
            if (filter.BookID != null)
            {
                cmd.WhereAnd<Where<Transact.bookID, Equal<Current<AccountFilter.bookID>>>>();
            }

            foreach (Transact tran in cmd.Select())
            {
                if (tran.TranType == FATran.tranType.PurchasingPlus)
                {
                    tran.DebitAmt = tran.TranAmt;
                    tran.CreditAmt = 0m;
                    tran.AccountID = tran.DebitAccountID;
                    tran.SubID = tran.DebitSubID;
                }
                else if (tran.TranType == FATran.tranType.PurchasingMinus)
                {
                    tran.CreditAmt = tran.TranAmt;
                    tran.DebitAmt = 0m;
                    tran.AccountID = tran.CreditAccountID;
                    tran.SubID = tran.CreditSubID;
                }
                else continue;
                yield return tran;
            }
        }

        public virtual void AccountFilter_AssetID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
        {
            sender.SetDefaultExt<AccountFilter.bookID>(e.Row);
        }
    }
}