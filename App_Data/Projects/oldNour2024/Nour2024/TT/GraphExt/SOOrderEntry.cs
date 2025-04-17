using System;
using PX.Data;
using PX.Objects.AR;
using PX.Objects.CR;
using PX.Objects.PO;
using Newtonsoft.Json;
using Nour2024.Helpers;
using Nour2024.Models;

namespace PX.Objects.SO
{

    public class SOOrderEntry_Extension : PXGraphExtension<SOOrderEntry>
    {

        #region Event Handlers

        protected bool IsSmsSent = false;
        protected void SOLineSplit_LotSerialNbr_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
        {
            try
            {
                SOLineSplit row = (SOLineSplit)e.Row;
                SOLineSplitExt rowExt = sender.GetExtension<SOLineSplitExt>(row);

                if (row != null) 
                {
                    if (!String.IsNullOrEmpty(row.LotSerialNbr + ""))
                    {
                        POReceiptLineSplit poReceiptLineSplit = PXSelect<POReceiptLineSplit, Where<POReceiptLineSplit.lotSerialNbr, Equal<Required<POReceiptLineSplit.lotSerialNbr>>>>.Select(this.Base, row.LotSerialNbr);
                        POReceiptLineSplitExt poReceiptLineSplitExt = PXCache<POReceiptLineSplit>.GetExtension<POReceiptLineSplitExt>(poReceiptLineSplit);
                        rowExt.UsrColor = poReceiptLineSplitExt.UsrColor;
                        rowExt.UsrModelYear = poReceiptLineSplitExt.UsrModelYear;
                        rowExt.UsrMotorNo = poReceiptLineSplitExt.UsrMotorNo;
                        rowExt.UsrfrontelectricMotorNumber = poReceiptLineSplitExt.UsrfrontelectricMotorNumber;
                        rowExt.UsrRearElectricMotorNumber = poReceiptLineSplitExt.UsrRearElectricMotorNumber;
                    }
                }
            }
            catch { }
        }

        protected void SOOrder_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
        {

            try
            {
                SOOrder order = (SOOrder)e.Row;
                SOOrderExt orderExt = sender.GetExtension<SOOrderExt>(order);
                if (order != null)
                {

                    PXResultset<SOAdjust> resSet = PXSelect<SOAdjust, Where<SOAdjust.adjdOrderType, Equal<Required<SOAdjust.adjdOrderType>>, And<SOAdjust.adjdOrderNbr, Equal<Required<SOAdjust.adjdOrderNbr>>>>>.Select(this.Base, order.OrderType, order.OrderNbr);
                    decimal sum = (decimal)0.0;
                    foreach (PXResult<SOAdjust> res in resSet)
                    {
                        SOAdjust line = (SOAdjust)res;
                        ARPayment payment = PXSelect<ARPayment, Where<ARPayment.docType, Equal<Required<ARPayment.docType>>, And<ARPayment.refNbr, Equal<Required<ARPayment.refNbr>>>>>.Select(this.Base, line.AdjgDocType, line.AdjgRefNbr);

                        order.OrderDesc = payment.RefNbr + "-" + payment.DocType + "-" + payment.Released.Value + "";
                        if ((line.AdjgDocType == ARDocType.Payment || line.AdjgDocType == ARDocType.Prepayment) && (payment.Released.Value == ((bool?)true)))
                        {
                            sum += (decimal)line.CuryAdjdAmt;
                        }
                        else
                        {
                            if ((payment.Released.Value == true))
                            {
                                sum -= (decimal)line.CuryAdjdAmt;
                            }
                        }
                    }

                    sender.SetValueExt<SOOrderExt.usrTotalPayments>(order, (decimal)sum);
                    orderExt.UsrTotalPayments = (decimal)sum;

                }

            }
            catch { }

        }





        protected virtual void _(Events.RowPersisted<SOOrder> e)
        {
            bool IsSavingForFirtTime = (e.Operation == PXDBOperation.Insert && !IsSmsSent);
            if (IsSavingForFirtTime == true)
            {
                IsSmsSent = true;
                SOOrder order = e.Row;
                SOOrderExt orderExt = PXCache<SOOrder>.GetExtension<SOOrderExt>(order);
                if (order.OrderNbr != null && order.OrderNbr != "" && order.OrderNbr != " <NEW>" && order.OrderType == "QT")
                {
                    Contact client = PXSelect<Contact, Where<Contact.bAccountID, Equal<Required<SOOrder.customerID>>>>.Select(this.Base, order.CustomerID);
                    string phone = client.Phone1;
                    if (phone != null)
                    {
                        if (phone.Length == 11 || phone.Length == 12)
                        {
                            string message = "������ ������ ������ ������� ��� �� ���� ��� ����� ������ �� ����� ����� ��� ��� ��� : " + order.OrderNbr + " ����� �� ��������� ���������� ������� ��� ��� 19943 ";
                            string response = SmsSender.SendMessage(message, phone);
                            salssmsrespons responseRoot = JsonConvert.DeserializeObject<salssmsrespons>(response);

                            if (responseRoot != null)
                            {
                                if (responseRoot.code == 0)
                                {
                                    string message0 = responseRoot.message;
                                    string smsid = responseRoot.smsid;
                                }

                            }
                        }
                    }

                }
            }
        }

        #endregion


        


    }


}