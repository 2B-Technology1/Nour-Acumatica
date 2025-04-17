using System;
using PX.Data;
using PX.Objects.CS;
using PX.Objects.SO;

namespace PX.Objects.CA
{
  [Serializable]
  public class BankCUC: IBqlTable
  {


      #region CashAccountID



      [PXDBInt(IsKey = true)]

      [PXDBDefault(typeof(CashAccount.cashAccountID))]

      [PXParent(typeof(Select<CashAccount,

                Where<CashAccount.cashAccountID,

                    Equal<Current<BankCUC.cashAccountID>>>>))]
      //[PXDBDefault(typeof(Search<PaymentMethod.paymentMethodID, Where<PaymentMethod.useForAR,Equal<True>>>))]

      //[PXSelector(typeof(Search<PaymentMethod.paymentMethodID, Where<PaymentMethod.useForAR,Equal<True>>>),

      //                    new Type[]

      //      {

      //        typeof(PaymentMethod.paymentMethodID),

      //        typeof(PaymentMethod.descr)

      //      },

      //                    SubstituteKey = typeof(PaymentMethod.paymentMethodID), DescriptionField = typeof(PaymentMethod.descr))]
      //[PXSelector(typeof(Search<CashAccount.cashAccountID>),

      //               new Type[]

      //      {

      //        typeof(CashAccount.cashAccountID),

      //        typeof(CashAccount.descr)

      //      },

      //               SubstituteKey = typeof(CashAccount.cashAccountID), DescriptionField = typeof(CashAccount.descr))]

      public int? CashAccountID { get; set; }



      public class cashAccountID : IBqlField { }



      #endregion

      #region Cucaccountid



      [PXDBInt(IsKey = true)]

      [PXUIField(DisplayName = "CUC Account")]
     

      [PXSelector(typeof(Search<CashAccount.cashAccountID, Where<CashAccountExt.usrCUC, Equal<True>>>),

                          new Type[]

            {

                  typeof(CashAccount.cashAccountCD),

              typeof(CashAccount.descr),

              typeof(CashAccount.curyID),

            },

                          SubstituteKey = typeof(CashAccount.cashAccountCD), DescriptionField = typeof(CashAccount.descr))]


      //[PXSelector(typeof(Terms.termsID),

      //                   new Type[]

      //      {

      //            typeof(Terms.termsID),

      //        typeof(Terms.descr)

      //      },

      //                   SubstituteKey = typeof(Terms.termsID), DescriptionField = typeof(Terms.descr))]


      public int? Cucaccountid { get; set; }



      public class cucaccountid : IBqlField { }



      #endregion

  }
}