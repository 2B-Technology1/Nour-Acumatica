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

using System;
using PX.Data;
using PX.Objects.IN;
using PX.Objects.EP;
using PX.Objects.CM;
using PX.Objects.PM;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.DR;
using PX.Objects.CS;

namespace PX.Objects.FS
{
    [System.SerializableAttribute]
    [PXCacheName(TX.TableName.FSContractPeriodDet)]
    public class FSContractPeriodDet : PX.Data.IBqlTable
    {
        #region Keys
        public class PK : PrimaryKeyOf<FSContractPeriodDet>.By<contractPeriodID, contractPeriodDetID>
        {
            public static FSContractPeriodDet Find(PXGraph graph, int? contractPeriodID, int? contractPeriodDetID, PKFindOptions options = PKFindOptions.None) => FindBy(graph, contractPeriodID, contractPeriodDetID, options);
        }

        public static class FK
        {
            public class ServiceContract : FSServiceContract.PK.ForeignKeyOf<FSContractPeriodDet>.By<serviceContractID> { }
            public class InventoryItem : IN.InventoryItem.PK.ForeignKeyOf<FSContractPeriodDet>.By<inventoryID> { }
            public class ContractPeriod : FSContractPeriod.PK.ForeignKeyOf<FSContractPeriodDet>.By<serviceContractID, contractPeriodID> { }
            public class Equipment : FSEquipment.PK.ForeignKeyOf<FSContractPeriodDet>.By<SMequipmentID> { }
            public class Project : PMProject.PK.ForeignKeyOf<FSContractPeriodDet>.By<projectID> { }
            public class Task : PMTask.PK.ForeignKeyOf<FSContractPeriodDet>.By<projectID, projectTaskID> { }
            public class CostCode : PMCostCode.PK.ForeignKeyOf<FSContractPeriodDet>.By<costCodeID> { }
        }
        #endregion

        #region ServiceContractID
        public abstract class serviceContractID : PX.Data.BQL.BqlInt.Field<serviceContractID> { }

        [PXDBInt]
        [PXDBDefault(typeof(FSServiceContract.serviceContractID))]
        [PXUIField(DisplayName = "Service Contract ID")]
        public virtual int? ServiceContractID { get; set; }
        #endregion
        #region ContractPeriodID
        public abstract class contractPeriodID : PX.Data.BQL.BqlInt.Field<contractPeriodID> { }

        [PXDBInt(IsKey = true)]
        [PXParent(typeof(Select<FSContractPeriod, Where<FSContractPeriod.contractPeriodID, Equal<Current<FSContractPeriodDet.contractPeriodID>>>>))]
        [PXDBDefault(typeof(FSContractPeriod.contractPeriodID))]

        public virtual int? ContractPeriodID { get; set; }
        #endregion
        #region ContractPeriodDetID
        public abstract class contractPeriodDetID : PX.Data.BQL.BqlInt.Field<contractPeriodDetID> { }

        [PXDBIdentity(IsKey = true)]
        public virtual int? ContractPeriodDetID { get; set; }
        #endregion
        #region LineType
        public abstract class lineType : ListField_LineType_ContractPeriod
        {
        }

        [PXDBString(5, IsFixed = true)]
        [PXUIField(DisplayName = "Line Type")]
        [lineType.ListAtrribute]
        [PXDefault(ID.LineType_ContractPeriod.SERVICE)]
        public virtual string LineType { get; set; }
        #endregion
        #region InventoryID
        public abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }

        [PXDefault]
        [PXUIField(DisplayName = "Inventory ID")]
        [InventoryIDByLineType(typeof(FSContractPeriodDet.lineType), Filterable = true)]
        [PXRestrictor(typeof(
                        Where<
                            InventoryItem.itemType, NotEqual<INItemTypes.serviceItem>,
                            Or<FSxServiceClass.requireRoute, Equal<True>,
                            Or<Current<FSServiceContract.recordType>, Equal<FSServiceContract.recordType.ServiceContract>>>>),
                TX.Error.NONROUTE_SERVICE_CANNOT_BE_HANDLED_WITH_ROUTE_SRVORDTYPE)]
        [PXRestrictor(typeof(
                        Where<
                            InventoryItem.itemType, NotEqual<INItemTypes.serviceItem>,
                            Or<FSxServiceClass.requireRoute, Equal<False>,
                            Or<Current<FSServiceContract.recordType>, Equal<FSServiceContract.recordType.RouteServiceContract>>>>),
                TX.Error.ROUTE_SERVICE_CANNOT_BE_HANDLED_WITH_NONROUTE_SRVORDTYPE)]
        [PXCheckUnique(typeof(SMequipmentID),
                       Where = typeof(Where<FSContractPeriodDet.serviceContractID, Equal<Current<FSContractPeriodDet.serviceContractID>>,
                                            And<FSContractPeriodDet.contractPeriodID, Equal<Current<FSContractPeriodDet.contractPeriodID>>,
                                                  And<
                                                      Where<Current<FSContractPeriodDet.SMequipmentID>, IsNull,
                                                            Or<FSContractPeriodDet.SMequipmentID, Equal<Current<FSContractPeriodDet.SMequipmentID>>>>>>>))]
        public virtual int? InventoryID { get; set; }
        #endregion
        #region UOM
        public abstract class uOM : PX.Data.BQL.BqlString.Field<uOM> { }

        [INUnit(typeof(inventoryID), DisplayName = "UOM", Enabled = false)]
        [PXDefault(typeof(Search<InventoryItem.salesUnit, Where<InventoryItem.inventoryID, Equal<Current<FSContractPeriodDet.inventoryID>>>>))]
        public virtual string UOM { get; set; }
        #endregion
        #region SMEquipmentID
        public abstract class SMequipmentID : PX.Data.BQL.BqlInt.Field<SMequipmentID> { }

        [PXDBInt]
        [PXUIField(DisplayName = "Target Equipment ID", FieldClass = FSSetup.EquipmentManagementFieldClass)]
        [FSSelectorContractPeriodEquipment]
        [PXRestrictor(typeof(Where<FSEquipment.status, Equal<EPEquipmentStatus.EquipmentStatusActive>>),
                        TX.Messages.EQUIPMENT_IS_INSTATUS, typeof(FSEquipment.status))]
		[PXForeignReference(typeof(Field<FSContractPeriodDet.SMequipmentID>.IsRelatedTo<FSEquipment.SMequipmentID>))]
        public virtual int? SMEquipmentID { get; set; }
        #endregion
        #region BillingRule
        public abstract class billingRule : ListField_BillingRule_ContractPeriod
        {
        }

        [PXDBString(4, IsFixed = true)]
        [billingRule.ListAtrribute]
        [PXUIField(DisplayName = "Billing Rule")]
        public virtual string BillingRule { get; set; }
        #endregion
        #region Time
        public abstract class time : PX.Data.BQL.BqlInt.Field<time> { }

        [PXDBTimeSpanLong(Format = TimeSpanFormatType.LongHoursMinutes)]
        [PXFormula(typeof(Default<FSContractPeriodDet.inventoryID>))]
        [PXUIField(DisplayName = "Time", Enabled = false, Visible = false)]
        public virtual int? Time { get; set; }
        #endregion
        #region Qty
        public abstract class qty : PX.Data.BQL.BqlDecimal.Field<qty> { }

        [PXDBQuantity]
        [PXFormula(typeof(Default<inventoryID, time>))]
        [PXDefault(TypeCode.Decimal, "1.0", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Quantity", Visible = false)]
        public virtual decimal? Qty { get; set; }
        #endregion
        #region UsedQty  
        public abstract class usedQty : PX.Data.BQL.BqlDecimal.Field<usedQty> { }

        [PXDBDecimal]
        [PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Used Period Quantity", Enabled = false, Visible = false)]
        public virtual decimal? UsedQty { get; set; }
        #endregion
        #region UsedTime
        public abstract class usedTime : PX.Data.BQL.BqlInt.Field<usedTime> { }

        [PXDefault(0, PersistingCheck = PXPersistingCheck.Nothing)]
        [PXDBTimeSpanLong(Format = TimeSpanFormatType.LongHoursMinutes)]
        [PXUIField(DisplayName = "Used Period Time", Enabled = false, Visible = false)]
        public virtual int? UsedTime { get; set; }
        #endregion
        #region RecurringUnitPrice
        public abstract class recurringUnitPrice : PX.Data.BQL.BqlDecimal.Field<recurringUnitPrice> { }

        [PXUIField(DisplayName = "Recurring Item Price")]
		[PXDBDecimal(typeof(Search<CommonSetup.decPlPrcCst>))]
        public virtual decimal? RecurringUnitPrice { get; set; }
        #endregion
        #region RecurringTotalPrice
        public abstract class recurringTotalPrice : PX.Data.BQL.BqlDecimal.Field<recurringTotalPrice> { }

        [PXDBBaseCury]
        [PXFormula(typeof(Default<qty, recurringUnitPrice>))]
        [PXUIField(DisplayName = "Total Recurring Price", Enabled = false)]
        [PXFormula(typeof(Mult<qty, recurringUnitPrice>), typeof(SumCalc<FSContractPeriod.periodTotal>))]
        public virtual decimal? RecurringTotalPrice { get; set; }
        #endregion
        #region OverageItemPrice
        public abstract class overageItemPrice : PX.Data.BQL.BqlDecimal.Field<overageItemPrice> { }

        [PXUIField(DisplayName = "Overage Item Price")]
		[PXDBDecimal(typeof(Search<CommonSetup.decPlPrcCst>))]
        public virtual decimal? OverageItemPrice { get; set; }
        #endregion
        #region Rollover
        public abstract class rollover : PX.Data.BQL.BqlBool.Field<rollover> { }

        [PXDBBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Rollover", IsReadOnly = true)]
        public virtual bool? Rollover { get; set; }
        #endregion
        #region RemainingQty 
        public abstract class remainingQty : PX.Data.BQL.BqlDecimal.Field<remainingQty> { }

        [PXDBDecimal]
        [PXDefault(typeof(Switch<
                            Case<Where<usedQty, LessEqual<qty>>,
                                Sub<qty, usedQty>>,
                            SharedClasses.decimal_0>), PersistingCheck = PXPersistingCheck.Nothing)]
        [PXFormula(typeof(Default<FSContractPeriodDet.qty, FSContractPeriodDet.usedQty>))]
        [PXUIField(DisplayName = "Remaining Period Quantity", Enabled = false, Visible = false)]
        public virtual decimal? RemainingQty { get; set; }
        #endregion
        #region RemainingTime
        public abstract class remainingTime : PX.Data.BQL.BqlInt.Field<remainingTime> { }

        [PXDBTimeSpanLong(Format = TimeSpanFormatType.LongHoursMinutes)]
        [PXDefault(typeof(Switch<
                            Case<Where<usedTime, LessEqual<time>>,
                                Sub<time, usedTime>>,
                            SharedClasses.int_0>), PersistingCheck = PXPersistingCheck.Nothing)]
        [PXFormula(typeof(Default<FSContractPeriodDet.time, FSContractPeriodDet.usedTime>))]
        [PXUIField(DisplayName = "Remaining Period Time", Enabled = false, Visible = false)]
        public virtual int? RemainingTime { get; set; }
        #endregion
        #region ScheduledQty  
        public abstract class scheduledQty : PX.Data.BQL.BqlDecimal.Field<scheduledQty> { }

        [PXDecimal]
        [PXUIField(DisplayName = "Scheduled Period Quantity", Enabled = false, Visible = false)]
        public virtual decimal? ScheduledQty { get; set; }
        #endregion
        #region ScheduledTime
        public abstract class scheduledTime : PX.Data.BQL.BqlInt.Field<scheduledTime> { }

        [PXDefault(0, PersistingCheck =PXPersistingCheck.Nothing)]
        [PXTimeSpanLong(Format = TimeSpanFormatType.LongHoursMinutes)]
        [PXUIField(DisplayName = "Scheduled Period Time", Enabled = false, Visible = false)]
        public virtual int? ScheduledTime { get; set; }
        #endregion
        #region DeferredCode
        public abstract class deferredCode : PX.Data.BQL.BqlString.Field<deferredCode> { }

        [PXDBString(10, IsUnicode = true, InputMask = ">aaaaaaaaaa")]
        [PXUIField(DisplayName = "Deferral Code")]
        [PXSelector(typeof(Search<DRDeferredCode.deferredCodeID, Where<DRDeferredCode.accountType, Equal<DeferredAccountType.income>>>))]
        [PXRestrictor(typeof(Where<DRDeferredCode.active, Equal<True>>), DR.Messages.InactiveDeferralCode, typeof(DRDeferredCode.deferredCodeID))]
        [PXFormula(typeof(Default<inventoryID>))]
        public virtual String DeferredCode { get; set; }
        #endregion

        #region ProjectID
        public abstract class projectID : PX.Data.BQL.BqlInt.Field<projectID> { }

        [PXDBInt]
        [PXDBDefault(typeof(FSServiceContract.projectID))]
        [PXForeignReference(typeof(FK.Project))]
        public virtual int? ProjectID { get; set; }
        #endregion
        #region ProjectTaskID
        public abstract class projectTaskID : PX.Data.BQL.BqlInt.Field<projectTaskID> { }

        [PXDefault(typeof(FSServiceContract.dfltProjectTaskID), PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIEnabled(typeof(Where<lineType, NotEqual<ListField_LineType_ALL.Comment>,
                            And<lineType, NotEqual<ListField_LineType_ALL.Instruction>>>))]
		[ActiveOrInPlanningProjectTask(typeof(FSContractPeriodDet.projectID), DisplayName = "Project Task", DescriptionField = typeof(PMTask.description))]
		[PXForeignReference(typeof(FK.Task))]
        public virtual int? ProjectTaskID { get; set; }
		#endregion
		#region CostCodeID
		public abstract class costCodeID : PX.Data.BQL.BqlInt.Field<costCodeID> { }

		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
        [SMCostCode(typeof(skipCostCodeValidation), null, typeof(projectTaskID), DisplayName = "Cost Code", Filterable = false, Enabled = false)]
        [PXForeignReference(typeof(FK.CostCode))]
        public virtual int? CostCodeID { get; set; }
        #endregion
        #region SkipCostCodeValidation
        public abstract class skipCostCodeValidation : PX.Data.BQL.BqlBool.Field<skipCostCodeValidation> { }

        [PXBool]
        [PXFormula(typeof(IIf<Where<lineType, Equal<ListField_LineType_ALL.Service>,
                                   Or<lineType, Equal<ListField_LineType_ALL.NonStockItem>>>, False, True>))]
        public virtual bool? SkipCostCodeValidation { get; set; }
        #endregion

        #region CreatedByScreenID
        public abstract class createdByScreenID : PX.Data.BQL.BqlString.Field<createdByScreenID> { }

        [PXDBCreatedByScreenID]
        [PXUIField(DisplayName = "Created By Screen ID")]
        public virtual string CreatedByScreenID { get; set; }
        #endregion
        #region CreatedDateTime
        public abstract class createdDateTime : PX.Data.BQL.BqlDateTime.Field<createdDateTime> { }

        [PXDBCreatedDateTime]
        [PXUIField(DisplayName = "Created DateTime")]
        public virtual DateTime? CreatedDateTime { get; set; }
        #endregion
        #region LastModifiedByID
        public abstract class lastModifiedByID : PX.Data.BQL.BqlGuid.Field<lastModifiedByID> { }

        [PXDBLastModifiedByID]
        [PXUIField(DisplayName = "Last Modified By ID")]
        public virtual Guid? LastModifiedByID { get; set; }
        #endregion
        #region LastModifiedByScreenID
        public abstract class lastModifiedByScreenID : PX.Data.BQL.BqlString.Field<lastModifiedByScreenID> { }

        [PXDBLastModifiedByScreenID]
        [PXUIField(DisplayName = "Last Modified By Screen ID")]
        public virtual string LastModifiedByScreenID { get; set; }
        #endregion
        #region LastModifiedDateTime
        public abstract class lastModifiedDateTime : PX.Data.BQL.BqlDateTime.Field<lastModifiedDateTime> { }

        [PXDBLastModifiedDateTime]
        [PXUIField(DisplayName = "Last Modified Date Time")]
        public virtual DateTime? LastModifiedDateTime { get; set; }
		#endregion
		#region tstamp
		public abstract class Tstamp : PX.Data.BQL.BqlByteArray.Field<Tstamp> { }
		[PXDBTimestamp(VerifyTimestamp = VerifyTimestampOptions.BothFromGraphAndRecord)]
		public virtual byte[] tstamp { get; set; }
		#endregion

        #region Memory Fields
        #region RegularPrice
        public abstract class regularPrice : PX.Data.BQL.BqlDecimal.Field<regularPrice> { }

        [PXUIField(DisplayName = "Regular Price", IsReadOnly = true, Visible = false)]
		[PXDBDecimal(typeof(Search<CommonSetup.decPlPrcCst>))]
        public virtual decimal? RegularPrice { get; set; }
        #endregion

        #region Amount
        public abstract class amount : PX.Data.BQL.BqlString.Field<amount> { }
        [PXString(255, IsUnicode = true)]
        [PXUIField(DisplayName = "Value")]
        public virtual String Amount { get; set; }
        #endregion
        #region RemainingAmount
        public abstract class remainingAmount : PX.Data.BQL.BqlString.Field<remainingAmount> { }
        [PXString(255, IsUnicode = true)]
        [PXUIField(DisplayName = " Remaining Period Value", Enabled = false)]
        public virtual String RemainingAmount { get; set; }
        #endregion
        #region UsedAmount
        public abstract class usedAmount : PX.Data.BQL.BqlString.Field<usedAmount> { }
        [PXString(255, IsUnicode = true)]
        [PXUIField(DisplayName = "Used Period Value", Enabled = false)]
        public virtual String UsedAmount { get; set; }
        #endregion 
        #region ScheduledAmount
        public abstract class scheduledAmount : PX.Data.BQL.BqlString.Field<scheduledAmount> { }
        [PXString(255, IsUnicode = true)]
        [PXUIField(DisplayName = "Scheduled Period Value", Enabled = false)]
        public virtual String ScheduledAmount { get; set; }
        #endregion
        #endregion
    }
}
