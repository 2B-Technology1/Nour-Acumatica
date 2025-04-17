using System;
using PX.Data;
using PX.Objects.CA;

namespace PX.Objects.CA
{
    [Serializable]
    public class BankCheck : IBqlTable
    {


        #region CashAccountID

        [PXDBInt(IsKey = true)]
        [PXDBDefault(typeof(CashAccount.cashAccountID))]
        [PXParent(typeof(Select<CashAccount,
                  Where<CashAccount.cashAccountID,
                      Equal<Current<BankCheck.cashAccountID>>>>))]
        public int? CashAccountID { get; set; }

        public class cashAccountID : IBqlField { }

        #endregion

        #region CheckAccountID

        [PXDBInt(IsKey = true)]
        [PXUIField(DisplayName = "Check Account")]
        [PXSelector(typeof(Search<CashAccount.cashAccountID, Where<CashAccountExt.usrCheckDispersant, Equal<True>>>),
                            new Type[]
            {
                  typeof(CashAccount.cashAccountCD),
              typeof(CashAccount.descr),
              typeof(CashAccount.curyID),
            },
                            SubstituteKey = typeof(CashAccount.cashAccountCD), DescriptionField = typeof(CashAccount.descr))]
        public int? CheckAccountID { get; set; }

        public class checkAccountID : IBqlField { }

        #endregion

    }
}