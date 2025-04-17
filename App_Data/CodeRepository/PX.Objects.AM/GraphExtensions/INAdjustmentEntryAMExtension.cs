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

namespace PX.Objects.AM.GraphExtensions
{
    public class INAdjustmentEntryAMExtension : PXGraphExtension<INAdjustmentEntry>
    {
        public static bool IsActive()
        {
            return PXAccess.FeatureInstalled<CS.FeaturesSet.manufacturing>();
        }

        protected virtual void INRegister_RowSelected(PXCache sender, PXRowSelectedEventArgs e, PXRowSelected del)
        {
            if (del != null)
            {
                del(sender, e);
            }

            if (e.Row == null || sender == null || sender.Graph == null)
            {
                return;
            }

            if (((INRegister)e.Row).Released == false
                //&& ((INRegister)e.Row).Hold == true
                && ((INRegister)e.Row).OrigModule == Common.ModuleAM)
            {
                PXUIFieldAttribute.SetEnabled(sender, e.Row, false);
                sender.AllowDelete = false;
                Base.transactions.Cache.AllowDelete = false;
                Base.splits.Cache.AllowDelete = false;
            }
        }

        protected virtual void INTran_RowSelected(PXCache sender, PXRowSelectedEventArgs e, PXRowSelected del)
        {
            if (del != null)
            {
                del(sender, e);
            }

            INRegister inRegister = Base.adjustment.Current;
            if (e.Row == null 
                || sender == null 
                || sender.Graph == null
                || inRegister == null)
            {
                return;
            }

            if (((INTran)e.Row).Released == false
                && inRegister.OrigModule == Common.ModuleAM)
            {
                PXUIFieldAttribute.SetEnabled(sender, e.Row, false);
            }
        }
    }
}