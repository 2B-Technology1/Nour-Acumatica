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

namespace PX.Objects.FS
{
    public class ServiceContractAutoNumberAttribute : AlternateAutoNumberAttribute
    {
        protected override string GetInitialRefNbr(string baseRefNbr)
        {
            return "000001";
        }

        /// <summary>
        /// Allows to calculate the <c>RefNbr</c> sequence when trying to insert a new register
        /// It's called from the Persisting event of FSServiceContract.
        /// </summary>
        protected override bool SetRefNbr(PXCache cache, object row)
        {
            FSServiceContract fsServiceContractRow = (FSServiceContract)row;

            FSServiceContract fsServiceContractRowTmp = PXSelectGroupBy<FSServiceContract,
                                                        Where<
                                                            FSServiceContract.customerID, Equal<Current<FSServiceContract.customerID>>>,
                                                        Aggregate<
                                                            Max<FSServiceContract.customerContractNbr,
                                                            GroupBy<FSServiceContract.customerID>>>>
                                                        .Select(cache.Graph);

            string lastRefNbr = fsServiceContractRowTmp?.CustomerContractNbr;

            fsServiceContractRow.CustomerContractNbr = GetNextRefNbr(null, lastRefNbr);

            return true;
        }
    }
}