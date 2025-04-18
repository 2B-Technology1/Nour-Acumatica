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

namespace PX.Objects.FS
{
    public class ContractInvoiceLine
    {
        #region Constructors
        public ContractInvoiceLine(IDocLine row)
        {
            InventoryID = row.InventoryID;
            UOM = row.UOM;
            SMEquipmentID = row.SMEquipmentID;
            CuryUnitPrice = row.CuryUnitPrice;
            ManualPrice = row.ManualPrice;
            CuryBillableExtPrice = row.CuryBillableExtPrice;
            DiscPct = row.DiscPct;
            SubItemID = row.SubItemID;
            SiteID = row.SiteID;
            SiteLocationID = row.SiteLocationID;
            IsFree = row.IsFree;
            AcctID = row.AcctID;
            SubID = row.SubID;
            EquipmentAction = row.EquipmentAction;
            EquipmentLineRef = row.EquipmentLineRef;
            NewTargetEquipmentLineNbr = row.NewTargetEquipmentLineNbr;
            ComponentID = row.ComponentID;
            LineRef = row.LineRef;
            LineNbr = row.LineNbr;
            TranDescPrefix = string.Empty;
            ProjectTaskID = row.ProjectTaskID;
            CostCodeID = row.CostCodeID;
            Processed = false;
        }

        public ContractInvoiceLine(PXResult<FSContractPeriodDet, FSContractPeriod, FSServiceContract, FSBranchLocation> row)
        {
            FSServiceContract fsServiceContractRow = (FSServiceContract)row;
            FSContractPeriodDet fsContractPeriodDetRow = (FSContractPeriodDet)row;
            FSBranchLocation fsBranchLocationRow = (FSBranchLocation)row;

            ServiceContractID = fsServiceContractRow.ServiceContractID;
            ContractType = fsServiceContractRow.RecordType;
            ContractPeriodID = fsContractPeriodDetRow.ContractPeriodDetID;
            ContractPeriodDetID = fsContractPeriodDetRow.ContractPeriodID;

            BillingRule = fsContractPeriodDetRow.BillingRule;
            InventoryID = fsContractPeriodDetRow.InventoryID;
            UOM = fsContractPeriodDetRow.UOM;
            SMEquipmentID = fsContractPeriodDetRow.SMEquipmentID;
            CuryUnitPrice = fsContractPeriodDetRow.RecurringUnitPrice;
            ManualPrice = true;

            ContractRelated = true;
            SubItemID = fsBranchLocationRow?.DfltSubItemID;
            SiteID = fsBranchLocationRow?.DfltSiteID;
            SiteLocationID = null;
            IsFree = false;

            if (BillingRule == ID.BillingRule.TIME)
            {
                Qty = decimal.Divide((decimal)(fsContractPeriodDetRow.Time ?? 0), 60);
            }
            else
            {
                Qty = fsContractPeriodDetRow.Qty;
            }
            
            OverageItemPrice = fsContractPeriodDetRow.OverageItemPrice;
            AcctID = null;
            SubID = null;
            EquipmentAction = ID.Equipment_Action.NONE;
            EquipmentLineRef = null;
            NewTargetEquipmentLineNbr = null;
            ComponentID = null;
            LineRef = string.Empty;
            SalesPersonID = fsServiceContractRow.SalesPersonID;
            Commissionable = fsServiceContractRow.Commissionable;

            TranDescPrefix = string.Empty;

            ProjectTaskID = fsContractPeriodDetRow.ProjectTaskID;
            CostCodeID = fsContractPeriodDetRow.CostCodeID;
            DeferredCode = fsContractPeriodDetRow.DeferredCode;
            BillingType = fsServiceContractRow.BillingType;

            Processed = false;
        }

        public ContractInvoiceLine(PXResult<FSAppointmentDet, FSSODet, FSAppointment> row) : this((IDocLine)(FSAppointmentDet)row)
        { 
            FSAppointmentDet fsAppointmentDetRow = (FSAppointmentDet)row;
            FSAppointment fsAppointmentRow = (FSAppointment)row;
            FSSODet fsSODetRow = (FSSODet)row;

            fsSODet = fsSODetRow;
            fsAppointmentDet = fsAppointmentDetRow;

            SOID = fsSODetRow.SOID;
            SODetID = fsSODetRow.SODetID;
            AppointmentID = fsAppointmentDetRow.AppointmentID;
            AppDetID = fsAppointmentDetRow.AppDetID;
            SrvOrdType = fsAppointmentDetRow.SrvOrdType;
            RefNbr = fsAppointmentDetRow.RefNbr;

            BillingRule = fsSODetRow.BillingRule;
            ContractRelated = fsAppointmentDetRow.ContractRelated;
            Qty = fsAppointmentDetRow.ContractRelated == true ? fsAppointmentDetRow.ActualQty : fsAppointmentDetRow.BillableQty;
            OverageItemPrice = fsAppointmentDetRow.OverageItemPrice;
            ManualPrice = fsAppointmentDetRow.ContractRelated == true ? true : fsAppointmentDetRow.ManualPrice;
            SalesPersonID = fsAppointmentRow.SalesPersonID;
            Commissionable = fsAppointmentRow.Commissionable;
        }

        public ContractInvoiceLine(PXResult<FSSODet, FSServiceOrder> row) : this((IDocLine)(FSSODet)row)
        {
            FSSODet fsSODetRow = (FSSODet)row;
            FSServiceOrder fsServiceOrderRow = (FSServiceOrder)row;

            fsSODet = fsSODetRow;

            SOID = fsSODetRow.SOID;
            SODetID = fsSODetRow.SODetID;
            SrvOrdType = fsSODetRow.SrvOrdType;
            RefNbr = fsSODetRow.RefNbr;

            BillingRule = fsSODetRow.BillingRule;
            ContractRelated = fsSODetRow.ContractRelated;
            Qty = fsSODetRow.ContractRelated == true ? fsSODetRow.EstimatedQty : fsSODetRow.BillableQty;
            OverageItemPrice = fsSODetRow.CuryExtraUsageUnitPrice;
            ManualPrice = fsSODetRow.ContractRelated == true ? true : fsSODetRow.ManualPrice;
            SalesPersonID = fsServiceOrderRow.SalesPersonID;
            Commissionable = fsServiceOrderRow.Commissionable;
        }

        public ContractInvoiceLine(ContractInvoiceLine row, decimal? qty) : this(row)
        {
            Qty = qty;
        }

        public ContractInvoiceLine(ContractInvoiceLine row)
        {
            ServiceContractID = row.ServiceContractID;
            ContractType = row.ContractType;
            ContractPeriodID = row.ContractPeriodDetID;
            ContractPeriodDetID = row.ContractPeriodID;

            SOID = row.SOID;
            SODetID = row.SODetID;
            AppointmentID = row.AppointmentID;
            AppDetID = row.AppDetID;
            SrvOrdType = row.SrvOrdType;
            RefNbr = row.RefNbr;

            BillingRule = row.BillingRule;
            InventoryID = row.InventoryID;
            UOM = row.UOM;
            SMEquipmentID = row.SMEquipmentID;
            CuryUnitPrice = row.CuryUnitPrice;
            ManualPrice = row.ManualPrice;
            DiscPct = row.DiscPct;
            ContractRelated = row.ContractRelated;
            SubItemID = row.SubItemID;
            SiteID = row.SiteID;
            SiteLocationID = row.SiteLocationID;
            IsFree = row.IsFree;

            Qty = row.Qty;

            CuryBillableExtPrice = row.CuryBillableExtPrice;
            OverageItemPrice = row.OverageItemPrice;
            AcctID = row.AcctID;
            SubID = row.SubID;
            EquipmentAction = row.EquipmentAction;
            EquipmentLineRef = row.EquipmentLineRef;
            NewTargetEquipmentLineNbr = row.NewTargetEquipmentLineNbr;
            ComponentID = row.ComponentID;
            LineRef = row.LineRef;
            LineNbr = row.LineNbr;
            SalesPersonID = row.SalesPersonID;
            Commissionable = row.Commissionable;

            TranDescPrefix = string.Empty;

            ProjectTaskID = row.ProjectTaskID;
            CostCodeID = row.CostCodeID;
            DeferredCode = row.DeferredCode;
            BillingType = row.BillingType;

            Processed = false;

            fsSODet = row.fsSODet;
            fsAppointmentDet = row.fsAppointmentDet;
        }
        #endregion

        #region Contract Fields
        public int? ServiceContractID { get; set; }

        public int? ContractPeriodID { get; set; }

        public int? ContractPeriodDetID { get; set; }

        public string ContractType { get; set; }

        public string BillingType { get; set; }
        #endregion

        #region Appointment Fields
        public int? AppointmentID { get; set; }

        public int? AppDetID { get; set; }
        #endregion

        #region Service Order Fields
        public int? SOID { get; set; }

        public int? SODetID { get; set; }
        #endregion

        #region Transaction Fields
        public string SrvOrdType { get; set; }

        public string RefNbr { get; set; }

        public string BillingRule { get; set; }

        public int? InventoryID { get; set; }

        public string UOM { get; set; }

        public int? SMEquipmentID { get; set; }

        public decimal? CuryUnitPrice { get; set; }

        public bool? ManualPrice { get; set; }

        public decimal? CuryBillableExtPrice { get; set; }

        public decimal? DiscPct { get; set; }

        public bool? ContractRelated { get; set; }

        public int? SubItemID { get; set; }

        public int? SiteID { get; set; }

        public int? SiteLocationID { get; set; }

        public bool? IsFree { get; set; }
        #endregion

        #region BoundFields
        public decimal? OverageItemPrice { get; set; }

        public decimal? Qty { get; set; }

        public int? AcctID { get; set; }

        public int? SubID { get; set; }

        public string EquipmentAction { get; set; }

        public int? EquipmentLineRef { get; set; }

        public string NewTargetEquipmentLineNbr { get; set; }

        public int? ComponentID { get; set; }

        public string LineRef { get; set; }

        public int? LineNbr { get; set; }

        public int? SalesPersonID { get; set; }

        public bool? Commissionable { get; set; }

        public int? ProjectTaskID { get; set; }

        public int? CostCodeID { get; set; }

        public string DeferredCode { get; set; }
        #endregion

        #region UnboundFields
        public string TranDescPrefix { get; set; }

        public bool? Processed { get; set; }

        public FSSODet fsSODet { get; set; }

        public FSAppointmentDet fsAppointmentDet { get; set; }
        #endregion
    }
}