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
using PX.Objects.AR;
using PX.Objects.IN;
using PX.Objects.SO;
using System;
using PX.Data.ReferentialIntegrity.Attributes;

namespace PX.Objects.FS
{
    [Serializable]
    [PXCacheName(TX.TableName.FSEquipmentComponent)]
    public class FSEquipmentComponent : PX.Data.IBqlTable
    {
        #region Keys
        public class PK : PrimaryKeyOf<FSEquipmentComponent>.By<SMequipmentID, lineNbr>
        {
            public static FSEquipmentComponent Find(PXGraph graph, int? SMequipmentID, int? lineNbr, PKFindOptions options = PKFindOptions.None) => FindBy(graph, SMequipmentID, lineNbr, options);
        }

        public static class FK
        {
            public class Equipment : FSEquipment.PK.ForeignKeyOf<FSEquipmentComponent>.By<SMequipmentID> { }
            public class Component : FSModelTemplateComponent.PK.ForeignKeyOf<FSEquipmentComponent>.By<componentID> { }
            public class Vendor : AP.Vendor.PK.ForeignKeyOf<FSEquipmentComponent>.By<vendorID> { }
            public class ItemClass : INItemClass.PK.ForeignKeyOf<FSEquipmentComponent>.By<itemClassID> { }
            public class InventoryItem : IN.InventoryItem.PK.ForeignKeyOf<FSEquipmentComponent>.By<inventoryID> { }
            public class InstallationServiceOrder : FSServiceOrder.PK.ForeignKeyOf<FSEquipmentComponent>.By<instSrvOrdType, instServiceOrderRefNbr> { }
            public class InstallationAppointment : FSAppointment.PK.ForeignKeyOf<FSEquipmentComponent>.By<instSrvOrdType, instAppointmentRefNbr> { }

            public class ReplacedComponent : FSEquipmentComponent.PK.ForeignKeyOf<FSEquipmentComponent>.By<SMequipmentID, componentReplaced> { }
        }

        #endregion

        #region SMEquipmentID
        public abstract class SMequipmentID : PX.Data.BQL.BqlInt.Field<SMequipmentID> { }

        [PXDBInt(IsKey = true)]
        [PXParent(typeof(Select<FSEquipment, Where<FSEquipment.SMequipmentID, Equal<Current<FSEquipmentComponent.SMequipmentID>>>>))]
        [PXDBDefault(typeof(FSEquipment.SMequipmentID))]
        [PXUIField(DisplayName = "Equipment ID")]
        public virtual int? SMEquipmentID { get; set; }
        #endregion
        #region LineNbr
        public abstract class lineNbr : PX.Data.BQL.BqlInt.Field<lineNbr> { }

        [PXDBInt(IsKey = true)]
        [PXLineNbr(typeof(FSEquipment))]
        public virtual int? LineNbr { get; set; }
        #endregion
        #region LineRef
        public abstract class lineRef : PX.Data.BQL.BqlString.Field<lineRef> { }

        [PXDBString(5, IsFixed = true)]
        [PXUIField(DisplayName = "Ref. Nbr.", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
        public virtual string LineRef { get; set; }
        #endregion
        #region ComponentID
        public abstract class componentID : PX.Data.BQL.BqlInt.Field<componentID> { }

        [PXDBInt]
        [PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Component ID")]
        [FSSelectorComponentIDEquipment]
        public virtual int? ComponentID { get; set; }
        #endregion
        #region CpnyWarrantyDuration
        public abstract class cpnyWarrantyDuration : PX.Data.BQL.BqlInt.Field<cpnyWarrantyDuration> { }

        [PXDBInt(MinValue = 0)]
        [PXUIField(DisplayName = "Company Warranty")]
        public virtual int? CpnyWarrantyDuration { get; set; }
        #endregion
        #region CpnyWarrantyEndDate
        public abstract class cpnyWarrantyEndDate : PX.Data.BQL.BqlDateTime.Field<cpnyWarrantyEndDate> { }

        [PXDBDate]
        [PXUIField(DisplayName = "Company Warranty End Date", Enabled = false)]
        public virtual DateTime? CpnyWarrantyEndDate { get; set; }
        #endregion
        #region InstallationDate
        public abstract class installationDate : PX.Data.BQL.BqlDateTime.Field<installationDate> { }

        [PXDBDate]
        [PXDefault(typeof(Search<FSEquipment.dateInstalled,
                          Where<
                                FSEquipment.SMequipmentID,
                                    Equal<Current<FSEquipmentComponent.SMequipmentID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Installation Date", Visible = false)]
        public virtual DateTime? InstallationDate { get; set; }
        #endregion
        #region LastReplacementDate
        public abstract class lastReplacementDate : PX.Data.BQL.BqlDateTime.Field<lastReplacementDate> { }

        [PXDBDate]
        [PXUIField(DisplayName = "Last Replacement Date", Enabled = false)]
        public virtual DateTime? LastReplacementDate { get; set; }
        #endregion
        #region LongDescr
        public abstract class longDescr : PX.Data.BQL.BqlString.Field<longDescr> { }

        [PXDBLocalizableString(256, IsUnicode = true)]
        [PXUIField(DisplayName = "Description")]
        public virtual string LongDescr { get; set; }
        #endregion
        #region VendorID
        public abstract class vendorID : PX.Data.BQL.BqlInt.Field<vendorID> { }

        [PXDBInt]
        [PXUIField(DisplayName = "Vendor ID")]
        [FSSelectorBusinessAccount_VE]
        public virtual int? VendorID { get; set; }
        #endregion
        #region VendorWarrantyDuration
        public abstract class vendorWarrantyDuration : PX.Data.BQL.BqlInt.Field<vendorWarrantyDuration> { }

        [PXDBInt(MinValue = 0)]
        [PXUIField(DisplayName = "Vendor Warranty")]
        public virtual int? VendorWarrantyDuration { get; set; }
        #endregion
        #region VendorWarrantyEndDate
        public abstract class vendorWarrantyEndDate : PX.Data.BQL.BqlDateTime.Field<vendorWarrantyEndDate> { }

        [PXDBDate]
        [PXUIField(DisplayName = "Vendor Warranty End Date", Enabled = false)]
        public virtual DateTime? VendorWarrantyEndDate { get; set; }
        #endregion

        // TODO: Rename this field to LotSerialNbr.
        // Change the label to "Lot/Serial Nbr.".
        #region SerialNumber
        public abstract class serialNumber : PX.Data.BQL.BqlString.Field<serialNumber> { }

        [PXDBString(100, IsUnicode = true)]
        [PXUIField(DisplayName = "Serial Nbr.")]
        [PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual string SerialNumber { get; set; }
        #endregion

        #region RequireSerial
        public abstract class requireSerial : PX.Data.BQL.BqlBool.Field<requireSerial> { }

        [PXDBBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Requires Serial Nbr.")]
        public virtual bool? RequireSerial { get; set; }
        #endregion
        #region ItemClassID
        public abstract class itemClassID : PX.Data.BQL.BqlInt.Field<itemClassID> { }

        [PXDBInt]
        [PXUIField(DisplayName = "Item Class ID")]
        [PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
        [PXSelector(typeof(
            Search2<INItemClass.itemClassID,
            InnerJoin<FSModelComponent, On<FSModelComponent.classID, Equal<INItemClass.itemClassID>>>,
            Where<
                FSModelComponent.componentID, Equal<Current<FSEquipmentComponent.componentID>>,
                And<FSModelComponent.modelID, Equal<Current<FSEquipment.inventoryID>>,
                And<FSxEquipmentModelTemplate.equipmentItemClass, Equal<ListField_EquipmentItemClass.Component>>>>>),
            SubstituteKey = typeof(INItemClass.itemClassCD))]
        public virtual int? ItemClassID { get; set; }
        #endregion
        #region InventoryID
        public abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }

        [PXDBInt]
        [PXUIField(DisplayName = "Inventory ID")]
        [PXDimensionSelector(InventoryAttribute.DimensionName, typeof(
            Search<InventoryItem.inventoryID,
            Where2<
                Where<
                    Current<FSEquipmentComponent.itemClassID>, IsNotNull,
                    And<InventoryItem.itemClassID, Equal<Current<FSEquipmentComponent.itemClassID>>,
                    Or<
                Where<Current<FSEquipmentComponent.itemClassID>, IsNull>>>>,
                And<FSxEquipmentModel.equipmentItemClass, Equal<ListField_EquipmentItemClass.Component>>>>), 
            typeof(InventoryItem.inventoryCD))]
        public virtual int? InventoryID { get; set; }
        #endregion
        #region CpnyWarrantyType
        public abstract class cpnyWarrantyType : ListField_WarrantyDurationType
        {
        }

        [PXDBString(1, IsFixed = true)]
        [cpnyWarrantyType.ListAtrribute]
        [PXDefault(ID.WarrantyDurationType.MONTH)]
        [PXUIField(DisplayName = "Company Warranty Type")]
        public virtual string CpnyWarrantyType { get; set; }
        #endregion
        #region VendorWarrantyType
        public abstract class vendorWarrantyType : ListField_WarrantyDurationType
        {
        }

        [PXDBString(1, IsFixed = true)]
        [vendorWarrantyType.ListAtrribute]
        [PXDefault(ID.WarrantyDurationType.MONTH)]
        [PXUIField(DisplayName = "Vendor Warranty Type")]
        public virtual string VendorWarrantyType { get; set; }
        #endregion
        #region SalesDate
        public abstract class salesDate : PX.Data.BQL.BqlDateTime.Field<salesDate> { }

        [PXDBDate]
        [PXDefault(typeof(Search<FSEquipment.salesDate,
                          Where<
                                FSEquipment.SMequipmentID,
                                    Equal<Current<FSEquipmentComponent.SMequipmentID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Sales Date", Visible = false)]
        public virtual DateTime? SalesDate { get; set; }
        #endregion
        #region InstSrvOrdType
        public abstract class instSrvOrdType : PX.Data.BQL.BqlString.Field<instSrvOrdType> { }

        [PXDBString(4, IsFixed = true)]
        [PXUIField(DisplayName = "Service Order Type")]
        public virtual string InstSrvOrdType { get; set; }
        #endregion
        #region InstServiceOrderNbr
        public abstract class instServiceOrderRefNbr : PX.Data.BQL.BqlInt.Field<instServiceOrderRefNbr> { }

        [PXDBString(15, IsUnicode = true)]
        [PXSelector(typeof(Search<FSServiceOrder.refNbr,
                            Where<FSServiceOrder.srvOrdType, Equal<Current<instSrvOrdType>>>>),
            SubstituteKey = typeof(FSServiceOrder.refNbr),
            DescriptionField = typeof(FSServiceOrder.docDesc))]
        [PXUIField(DisplayName = "Installation Service Order Nbr.", Enabled = false)]
        public virtual string InstServiceOrderRefNbr { get; set; }
        #endregion
        #region InstAppointmentRefNbr
        public abstract class instAppointmentRefNbr : PX.Data.BQL.BqlInt.Field<instAppointmentRefNbr> { }

        [PXDBString(20, IsUnicode = true)]
        [PXSelector(typeof(Search<FSAppointment.refNbr,
                            Where<FSAppointment.srvOrdType, Equal<Current<instSrvOrdType>>>>),
            SubstituteKey = typeof(FSAppointment.refNbr),
            DescriptionField = typeof(FSAppointment.docDesc))]
        [PXUIField(DisplayName = "Installation Appointment Nbr.", Enabled = false)]
        public virtual string InstAppointmentRefNbr { get; set; }
        #endregion
        #region InvoiceRefNbr
        public abstract class invoiceRefNbr : PX.Data.BQL.BqlString.Field<invoiceRefNbr> { }

        [PXDBString(15, IsUnicode = false)]
        [PXUIField(DisplayName = "Invoice Reference Nbr.", Enabled = false, Visible = false)]
        [PXSelector(typeof(
            Search<ARInvoice.refNbr, 
            Where
                <ARInvoice.docType, Equal<ARInvoiceType.invoice>,
                And<ARInvoice.refNbr, Equal<Current<FSEquipmentComponent.invoiceRefNbr>>>>>),
            SubstituteKey = typeof(ARInvoice.refNbr), ValidateValue = false)]
        public virtual string InvoiceRefNbr { get; set; }
        #endregion
        #region SalesOrderType
        public abstract class salesOrderType : PX.Data.BQL.BqlString.Field<salesOrderType> { }

        [PXDBString(3, IsFixed = true)]
        [PXUIField(DisplayName = "Sales Order Type")]
        public virtual string SalesOrderType { get; set; }
        #endregion
        #region SalesOrderNbr
        public abstract class salesOrderNbr : PX.Data.BQL.BqlString.Field<salesOrderNbr> { }

        [PXDBString(15, IsUnicode = false)]
        [PXUIField(DisplayName = "Sales Order Nbr.", Enabled = false, Visible = false)]
        [PXSelector(typeof(
            Search<SOOrder.orderNbr,
            Where<
                SOOrder.orderType, Equal<Current<FSEquipmentComponent.salesOrderType>>>>))]
        public virtual string SalesOrderNbr { get; set; }
        #endregion
        #region Status
        public abstract class status : ListField_Equipment_Status
        {
        }

        [PXDBString(1, IsFixed = true)]
        [status.ListAtrribute]
        [PXDefault(ID.Equipment_Status.ACTIVE)]
        [PXUIField(DisplayName = "Status")]
        public virtual string Status { get; set; }
        #endregion
        #region Comment
        public abstract class comment : PX.Data.BQL.BqlString.Field<comment> { }

        [PXDBString(int.MaxValue, IsUnicode = false)]
        [PXUIField(DisplayName = "Equipment Action Comment")]
        public virtual string Comment { get; set; }
        #endregion
        #region ComponentReplaced
        public abstract class componentReplaced : PX.Data.BQL.BqlInt.Field<componentReplaced> { }

        [PXDBInt]
        [PXUIField(DisplayName = "Component Replaced", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
        [PXSelector(typeof(
            Search<FSEquipmentComponent.lineNbr>), 
            SubstituteKey = typeof(FSEquipmentComponent.lineRef), ValidateValue = false)]
        public virtual int? ComponentReplaced { get; set; }
        #endregion

        #region NoteID
        public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }

        [PXUIField(DisplayName = "NoteID")]
        [PXNote]
        public virtual Guid? NoteID { get; set; }
        #endregion
        #region CreatedByID
        public abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID> { }

		[PXDBCreatedByID]
        public virtual Guid? CreatedByID { get; set; }
		#endregion
		#region CreatedByScreenID
		public abstract class createdByScreenID : PX.Data.BQL.BqlString.Field<createdByScreenID> { }

		[PXDBCreatedByScreenID]
        public virtual string CreatedByScreenID { get; set; }
		#endregion
		#region CreatedDateTime
		public abstract class createdDateTime : PX.Data.BQL.BqlDateTime.Field<createdDateTime> { }

		[PXDBCreatedDateTime]
        public virtual DateTime? CreatedDateTime { get; set; }
		#endregion
		#region LastModifiedByID
		public abstract class lastModifiedByID : PX.Data.BQL.BqlGuid.Field<lastModifiedByID> { }

		[PXDBLastModifiedByID]
        public virtual Guid? LastModifiedByID { get; set; }
		#endregion
		#region LastModifiedByScreenID
		public abstract class lastModifiedByScreenID : PX.Data.BQL.BqlString.Field<lastModifiedByScreenID> { }

		[PXDBLastModifiedByScreenID]
        public virtual string LastModifiedByScreenID { get; set; }
		#endregion
		#region LastModifiedDateTime
		public abstract class lastModifiedDateTime : PX.Data.BQL.BqlDateTime.Field<lastModifiedDateTime> { }

		[PXDBLastModifiedDateTime]
        public virtual DateTime? LastModifiedDateTime { get; set; }
		#endregion
		#region tstamp
		public abstract class Tstamp : PX.Data.BQL.BqlByteArray.Field<Tstamp> { }

		[PXDBTimestamp(VerifyTimestamp = VerifyTimestampOptions.BothFromGraphAndRecord)]
        public virtual byte[] tstamp { get; set; }
        #endregion
        #region MemoryHelper
        #region Mem_Description
        public abstract class mem_Description : PX.Data.BQL.BqlString.Field<mem_Description> { }

        [PXString(int.MaxValue, IsUnicode = true)]
        [PXUIField(DisplayName = "Component Description")]
        public virtual string Mem_Description { get; set; }
        #endregion
        #endregion
    }
}
