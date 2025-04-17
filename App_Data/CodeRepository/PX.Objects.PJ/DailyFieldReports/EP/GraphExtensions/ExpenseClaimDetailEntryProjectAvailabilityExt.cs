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
using PX.Objects.CN.Common.Extensions;
using PX.Objects.CS;
using PX.Objects.EP;

namespace PX.Objects.PJ.DailyFieldReports.EP.GraphExtensions
{
    public class ExpenseClaimDetailEntryProjectAvailabilityExt : PXGraphExtension<ExpenseClaimDetailEntryExt,
        ExpenseClaimDetailEntry.ExpenseClaimDetailEntryExt, ExpenseClaimDetailEntry>
    {
        public static bool IsActive()
        {
            return PXAccess.FeatureInstalled<FeaturesSet.constructionProjectManagement>();
        }

        public virtual void _(Events.RowSelected<EPExpenseClaimDetails> args, PXRowSelected baseHandler)
        {
            baseHandler(args.Cache, args.Args);
            if (args.Row is EPExpenseClaimDetails claimDetails &&
                args.Cache.GetEnabled<EPExpenseClaimDetails.contractID>(claimDetails))
            {
                var isEditable = Base2.IsProjectEditable();
                PXUIFieldAttribute.SetEnabled<EPExpenseClaimDetails.contractID>(args.Cache, claimDetails, isEditable);
            }
        }
    }
}