using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using PX.Common;
using PX.Data;
using PX.Objects.Common;
using PX.Objects.Common.Extensions;
using PX.Objects.GL;
using PX.Objects.CM;
using PX.Objects.CS;
using PX.Objects.CR;
using PX.Objects.CT;
using PX.Objects.PM;
using PX.Objects.TX;
using PX.Objects.IN;
using PX.Objects.CA;
using PX.Objects.BQLConstants;
using PX.Objects.EP;
using PX.Objects.PO;
using PX.Objects.SO;
using PX.Objects.DR;
using PX.Objects.AR;
//using Avalara.AvaTax.Adapter;
//using Avalara.AvaTax.Adapter.TaxService;
using AP1099Hist = PX.Objects.AP.Overrides.APDocumentRelease.AP1099Hist;
using AP1099Yr = PX.Objects.AP.Overrides.APDocumentRelease.AP1099Yr;
using PX.Objects.GL.Reclassification.UI;
//using AvaAddress = Avalara.AvaTax.Adapter.AddressService;
//using AvaMessage = Avalara.AvaTax.Adapter.Message;
using Branch = PX.Objects.GL.Branch;
using CRLocation = PX.Objects.CR.Standalone.Location;
using PX.Objects.AP.BQL;
using PX.Objects;
using PX.Objects.AP;

namespace PX.Objects.AP
{
  
  public class APInvoiceEntry_Extension:PXGraphExtension<APInvoiceEntry>
  {

    #region Event Handlers

      protected virtual void APTran_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
     {
         //try
         //{
         //    APTran row = (APTran)e.Row;
         //    APTranExt rowExt = PXCache<APTran>.GetExtension<APTranExt>(row);
         //    if (row != null)
         //    {
         //        if (!String.IsNullOrEmpty(row.ReceiptNbr + ""))
         //        {
         //            POReceiptLineSplit poReceiptLineSplit = PXSelect<POReceiptLineSplit, Where<POReceiptLineSplit.receiptNbr, Equal<Required<POReceiptLineSplit.receiptNbr>>, And<POReceiptLineSplit.receiptType, Equal<Required<POReceiptLineSplit.receiptType>>, And<POReceiptLineSplit.lineNbr, Equal<Required<POReceiptLineSplit.lineNbr>>>>>>.Select(this.Base, row.ReceiptNbr, row.ReceiptType, row.ReceiptLineNbr);
         //            if (!String.IsNullOrEmpty(poReceiptLineSplit.LotSerialNbr + ""))
         //            {
         //                rowExt.UsrChassis = poReceiptLineSplit.LotSerialNbr;
         //                sender.SetValue<APTranExt.usrChassis>(rowExt, poReceiptLineSplit.LotSerialNbr);
         //                if (isReturned(poReceiptLineSplit.LotSerialNbr))
         //                {
         //                    //e.Cancel = true;
         //                    //throw new PXException("This Chassis returned");
         //                    PXUIFieldAttribute.SetWarning<APTranExt.usrChassis>(sender, row, "The " + poReceiptLineSplit.LotSerialNbr + " Chassis returned");
         //                }
         //            }
         //        }
         //        else
         //        {

         //        }
         //    }
         //}
         //catch { }
         
     }


      public bool isReturned(String chassis)
      {
          bool ret = false;
          POReceiptLineSplit poReceiptLineSplit = PXSelect<POReceiptLineSplit, Where<POReceiptLineSplit.lotSerialNbr, Equal<Required<POReceiptLineSplit.lotSerialNbr>>, And<POReceiptLineSplit.receiptType,Equal<POReceiptType.poreturn>>>>.Select(this.Base, chassis);
          if (poReceiptLineSplit != null)
          {
              ret = true;
          }

          return ret;
      }

    #endregion

  }


}