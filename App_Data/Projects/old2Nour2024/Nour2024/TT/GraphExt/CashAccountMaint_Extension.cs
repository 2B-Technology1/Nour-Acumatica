using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PX.Data;
using PX.Objects.Common;
using PX.Objects.GL;
using PX.Objects.CS;
using PX.Objects.AP;
using PX.Objects;
using PX.Objects.CA;

namespace Maintenance
{

    public class CashAccountMaint_Extension : PXGraphExtension<CashAccountMaint>
    {

        #region Event Handlers
        public PXSelect<BankCUC, Where<BankCUC.cashAccountID, Equal<Current<CashAccount.cashAccountID>>>> cucAccounts;
        public PXSelect<BankCheck, Where<BankCheck.cashAccountID, Equal<Current<CashAccount.cashAccountID>>>> checkAccounts;

        #endregion

    }


}