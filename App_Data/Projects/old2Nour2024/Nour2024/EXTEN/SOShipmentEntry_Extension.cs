using System;
using System.Collections.Generic;
using PX.Data;
using PX.Objects.PO;

namespace PX.Objects.SO
{

    public class SOShipmentEntry_Extension : PXGraphExtension<SOShipmentEntry>
    {

        public string noData = "No data found To print!";

        #region Event Handlers

        protected void SOShipLineSplit_LotSerialNbr_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
        {

            try
            {
                SOShipLineSplit row = (SOShipLineSplit)e.Row;
                SOShipLineSplitExt rowExt = PXCache<SOShipLineSplit>.GetExtension<SOShipLineSplitExt>(row);

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

        public PXAction<PX.Objects.SO.SOShipment> DeliveryNote;

        [PXButton(CommitChanges = true)]
        [PXUIField(DisplayName = "Delivery Note", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        protected void deliveryNote()
        {
            SOShipment order = this.Base.Document.Current;
            if (order.ShipmentNbr != " <NEW>")
            {
                Dictionary<string, string> parameters = new Dictionary<string, string>();  //OrderType OrderNbr
                parameters["ShipmentNbr"] = order.ShipmentNbr;
                throw new PXReportRequiredException(parameters, "SO642000", PXBaseRedirectException.WindowMode.New, null);
            }
            else
            {
                throw new PXException(noData);
            }
        }

        #endregion

    }


}