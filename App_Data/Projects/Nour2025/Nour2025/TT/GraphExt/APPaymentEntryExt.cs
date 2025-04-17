using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using PX.Common;
using PX.Data;
using PX.Objects.AR;
using PX.Objects.CM;
using PX.Objects.CA;
using PX.Objects.Common;
using PX.Objects.Common.Extensions;
using PX.Objects.CS;
using PX.Objects.GL;
using PX.Objects.CR;
using PX.Objects;
using PX.Objects.AP;


namespace Maintenance
{

    public class APPaymentEntry_Extension : PXGraphExtension<APPaymentEntry>
    {

        #region Event Handlers
        protected static void APAdjust_RowPersisted(PXCache sender, PXRowPersistedEventArgs e)
        {
            //try
            //{
            //    var row = (APAdjust)e.Row;
            //    if (row.AdjgDocType == ARDocType.Refund)
            //    {
            //        APPaymentEntry paymentEntry = PXGraph.CreateInstance<APPaymentEntry>();
            //        APPayment adjustedPayment = PXSelect<APPayment, Where<APPayment.refNbr, Equal<Required<APPayment.refNbr>>>>.Select(paymentEntry, row.AdjdRefNbr);
            //        paymentEntry.Document.Update(adjustedPayment);
            //        paymentEntry.Document.Current.GetExtension<APRegisterExt>().UsrRefunded = true;
            //        paymentEntry.Actions.PressSave();
            //    }
            //}
            //catch { }
        }
        //[PXUIField(DisplayName = "Release", MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update)]
        //[PXProcessButton]
        //[PXOverride]
        //public IEnumerable Release(PXAdapter adapter)
        //{
        //    PXCache cache = this.Base.Document.Cache;
        //    APPaymentEntry paymentEntry = PXGraph.CreateInstance<APPaymentEntry>();
        //    List<ARRegister> list = new List<ARRegister>();
        //    foreach (ARPayment ardoc in adapter.Get<ARPayment>())
        //    {
        //        if (!(bool)ardoc.Hold)
        //        {
        //            cache.Update(ardoc);
        //            list.Add(ardoc);
        //        }
        //    }
        //    if (list.Count == 0)
        //    {
        //        throw new PXException( PX.Objects.AR.Messages.Document_Status_Invalid);
        //    }
        //    paymentEntry.Actions.PressSave();
        //    PXLongOperation.StartOperation(this, delegate() { ARDocumentRelease.ReleaseDoc(list, false); });
        //    APPayment adjustedPayment = PXSelect<APPayment, Where<APPayment.refNbr, Equal<Required<APPayment.refNbr>>>>.Select(paymentEntry, row.AdjdRefNbr);
        //   // var row = (APAdjust)e.Row;
        //    if (adjustedPayment.DocType == ARDocType.Refund)
        //    {
               
        //        paymentEntry.Document.Update(adjustedPayment);
        //        paymentEntry.Document.Current.GetExtension<APRegisterExt>().UsrRefunded = true;
        //        paymentEntry.Actions.PressSave();
        //    }
        //    return list;
        //}
        ////public delegate IEnumerable PrepareInvoiceDelegate(PXAdapter adapter);
        ////[PXUIField(DisplayName = "Prepare Invoice", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select, Visible = true)]
        ////[PXOverride]
        ////public IEnumerable PrepareInvoice(PXAdapter adapter, PrepareInvoiceDelegate baseMethod)
        ////{
        ////    if (this.Base.Document.Current.OrderNbr == " <NEW>")
        ////    {
        ////        throw new PXException("Must Save The order first");
        ////    }
        ////    PXResultset<SOLine> set = PXSelect<SOLine, Where<SOLine.orderType, Equal<Required<SOLine.orderType>>, And<SOLine.orderNbr, Equal<Required<SOLine.orderNbr>>>>>.Select(this.Base, this.Base.Document.Current.OrderType, this.Base.Document.Current.OrderNbr);
        ////    foreach (PXResult<SOLine> resLine in set)
        ////    {
        ////        SOLine linee = (SOLine)resLine;
        ////        UVSerials(linee);
        ////    }
        ////    VSerials();
        ////    return baseMethod(adapter);
        ////}                      

        #endregion

    }


}