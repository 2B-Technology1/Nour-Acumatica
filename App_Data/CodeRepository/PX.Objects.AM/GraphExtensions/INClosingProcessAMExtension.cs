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
using PX.Data;
using PX.Objects.Common;
using PX.Objects.GL;
using PX.Objects.IN;

namespace PX.Objects.AM.GraphExtensions
{
    /// <summary>
    /// Manufacturing extension for Inventory Close Financial Periods (IN509000)
    /// </summary>
    public class INClosingProcessAMExtension : PXGraphExtension<INClosingProcess>
    {
        public static bool IsActive()
        {
            return PXAccess.FeatureInstalled<CS.FeaturesSet.manufacturing>();
        }

        [PXOverride]
        public virtual ProcessingResult CheckOpenDocuments(IFinPeriod finPeriod, Func<IFinPeriod, ProcessingResult> del)
        {
            var results = del?.Invoke(finPeriod) ?? new ProcessingResult();

            if (HasUnreleasedAMDocuments(finPeriod))
            {
                results.AddErrorMessage(PX.Objects.AM.Messages.GetLocal(PX.Objects.AM.Messages.PeriodHasUnreleasedProductionDocs, FinPeriodIDAttribute.FormatForError(finPeriod.FinPeriodID)));
            }

            return results;
        }

        protected virtual bool HasUnreleasedAMDocuments(IFinPeriod finPeriod)
        {
            AMBatch result = PXSelect<
                AMBatch,
                Where<AMBatch.released, NotEqual<True>,
                    And<AMBatch.finPeriodID, Equal<Required<AMBatch.finPeriodID>>>>>
                .SelectWindowed(Base, 0, 1, finPeriod.FinPeriodID);
            return result?.BatNbr != null;
        }
    }
}
