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
using PX.Objects.AR;
using PX.Objects.CR;

namespace PX.Objects.FS
{
    public class MasterContractMaint : PXGraph<MasterContractMaint, FSMasterContract>
    {
        public MasterContractMaint()
        {
            PXUIFieldAttribute.SetDisplayName<BAccount.acctName>(BAccountRecords.Cache, TX.CustomTextFields.CUSTOMER_NAME);
            PXUIFieldAttribute.SetRequired<BAccount.acctName>(BAccountRecords.Cache, false);
        }

        #region Selects
        public PXSelect<BAccount> BAccountRecords;

        public PXSelectJoin<FSMasterContract,
               LeftJoin<BAccount,
                    On<BAccount.bAccountID, Equal<FSMasterContract.customerID>>,
               LeftJoin<Customer,
                    On<Customer.bAccountID, Equal<FSMasterContract.customerID>>>>,
                Where<Customer.bAccountID, IsNull,
                    Or<Match<Customer, Current<AccessInfo.userName>>>>>
               MasterContracts;
        #endregion
    }
}
