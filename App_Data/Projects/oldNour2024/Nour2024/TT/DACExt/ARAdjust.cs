using PX.Data;
using PX.Objects.AR;
using PX.Objects.CM;
using PX.Objects.CS;
using PX.Objects.GL;
using PX.Objects.SO;
using PX.Objects;
using PX.Web.UI;
using System.Collections.Generic;
using System;

using Maintenance;

namespace PX.Objects.AR
{
      [PXNonInstantiatedExtension]
  public class AR_ARAdjust_ExistingColumn : PXCacheExtension<ARAdjust>
  {
      #region AdjdRefNbr
        [PXMergeAttributes(Method = MergeMethod.Append)]

[PXCustomizeSelectorColumns(
typeof(ARAdjust.ARInvoice.refNbr),
typeof(ARAdjust.ARInvoice.docDate),
typeof(ARAdjust.ARInvoice.finPeriodID),
typeof(ARAdjust.ARInvoice.customerID),
typeof(ARRegister.customerLocationID),
typeof(ARRegister.curyID),//typeof(PX.Objects.AR.Standalone.ARRegister.curyID),
typeof(ARRegister.curyOrigDocAmt),
typeof(ARRegister.curyDocBal),
typeof(ARRegister.status),//typeof(PX.Objects.AR.Standalone.ARRegister.status),
typeof(ARAdjust.ARInvoice.dueDate),
typeof(ARAdjust.ARInvoice.invoiceNbr),
typeof(ARRegister.docDesc),//typeof(PX.Objects.AR.Standalone.ARRegister.docDesc),
typeof(ARRegisterExt.usrBankRefNbr),
typeof(ARRegisterExt.usrDueDate))]
      public string AdjdRefNbr { get; set; }
      #endregion
  }
}