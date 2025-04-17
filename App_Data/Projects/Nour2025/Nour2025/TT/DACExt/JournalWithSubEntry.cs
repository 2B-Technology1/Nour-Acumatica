using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using PX.Data;
using PX.Objects.AP;
using PX.Objects.AR;
using PX.Objects.CA;
using PX.Objects.CM;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Objects.PM;
using PX.Objects.TX;
using APQuickCheck = PX.Objects.AP.Standalone.APQuickCheck;
using ARCashSale = PX.Objects.AR.Standalone.ARCashSale;
using PX.Objects;
using PX.Objects.GL;

namespace PX.Objects.GL
{

    public class JournalWithSubEntry_Extension : PXGraphExtension<JournalWithSubEntry>
    {

        #region
        public PXAction<GLDocBatch> FinishReleased;
        [PXUIField(DisplayName = "Commit Released", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXButton]
        public virtual IEnumerable finishReleased(PXAdapter adapter)
        {
            //write your code in here
            List<GLTranDoc> list = new List<GLTranDoc>();
            foreach (GLTranDoc batch in this.Base.GLTranModuleBatNbr.Select())
            {
                if (this.Base.BatchModule.Current.Status == GLDocBatchStatus.Released)
                {
                    list.Add(batch);
                }
            }
            if (list.Count == 0)
            {
                throw new PXException(Messages.BatchStatusInvalid);
            }

            if (list.Count > 0)
            {
                PXLongOperation.StartOperation(this.Base, delegate() { UpdateReleasedBatch(list); });
            }

            return adapter.Get();
        }


        public static void UpdateReleasedBatch(List<GLTranDoc> list)
        {


            APPaymentEntry graph = PXGraph.CreateInstance<APPaymentEntry>();
            ARPaymentEntry graph2 = PXGraph.CreateInstance<ARPaymentEntry>();

            for (int i = 0; i < list.Count; i++)
            {

                GLTranDoc glTranDoc = list[i];

                if (glTranDoc == null)
                    return;

                GLTranDocExt glTranDocExt = PXCache<GLTranDoc>.GetExtension<GLTranDocExt>(glTranDoc);
                if (glTranDocExt == null)
                    return;

                GLTranCode code = PXSelect<GLTranCode, Where<GLTranCode.tranCode, Equal<Required<GLTranCode.tranCode>>>>.Select(graph, glTranDoc.TranCode);

                if (code.Module.Equals("AP"))
                {
                    graph.Document.Current = PXSelect<APPayment, Where<APPayment.refNbr, Equal<Required<APPayment.refNbr>>>>.Select(graph, glTranDoc.RefNbr);
                    APRegister row = graph.Document.Current;
                    if (row == null)
                        return;
                    APRegisterExt rowExt = PXCache<APRegister>.GetExtension<APRegisterExt>(row);
                    if (rowExt == null)
                        return;
                    rowExt.UsrDueDate = glTranDocExt.UsrDueDate;
                    //sender.SetValue<APRegisterExt.usrDueDate>(rowExt, glTranDocExt.UsrDueDate);
                    rowExt.UsrPONbr = glTranDocExt.UsrPONbr;
                    //sender.SetValue<APRegisterExt.usrPONbr>(rowExt, glTranDocExt.UsrPONbr);
                    rowExt.UsrCheckNbr = glTranDocExt.UsrCheckNbr;
                    //sender.SetValue<APRegisterExt.usrCheckNbr>(rowExt, glTranDocExt.UsrCheckNbr);
                    rowExt.UsrCO = true;
                    //sender.SetValue<APRegisterExt.usrCO>(rowExt, true);
                    graph.Document.Update(graph.Document.Current);
                    graph.Persist(typeof(APRegister), PXDBOperation.Update);
                }

                if (code.Module.Equals("AR"))
                {
                    graph2.Document.Current = PXSelect<ARPayment, Where<ARPayment.refNbr, Equal<Required<ARPayment.refNbr>>>>.Select(graph2, glTranDoc.RefNbr);
                    ARRegister row = graph2.Document.Current;
                    if (row == null)
                        return;
                    ARRegisterExt rowExt = PXCache<ARRegister>.GetExtension<ARRegisterExt>(row);
                    if (rowExt == null)
                        return;
                    rowExt.UsrDueDate = glTranDocExt.UsrDueDate;
                    rowExt.UsrSONbr = glTranDocExt.UsrSONbr;
                    rowExt.UsrCheckNbr = glTranDocExt.UsrCheckNbr;
                    graph2.Document.Update(graph2.Document.Current);
                    graph2.Persist(typeof(ARRegister), PXDBOperation.Update);
                }

            }
        }

        #endregion

    }


}