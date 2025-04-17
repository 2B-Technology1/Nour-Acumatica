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
using PX.Objects.CS;
using PX.Objects.IN;

namespace PX.Objects.FS
{
    public class FSxEquipmentModel : PXCacheExtension<InventoryItem>
    {
        public static bool IsActive()
        {
            return PXAccess.FeatureInstalled<FeaturesSet.equipmentManagementModule>();
        }

        #region EQEnabled
        public abstract class eQEnabled : PX.Data.BQL.BqlBool.Field<eQEnabled> { }
        [PXDBBool]
        [PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Model Equipment")]
        public virtual bool? EQEnabled { get; set; }
        #endregion
        #region ManufacturerID
        public abstract class manufacturerID : PX.Data.BQL.BqlInt.Field<manufacturerID> { }
        [PXDBInt]
        [PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Manufacturer")]
        [PXSelector(typeof(FSManufacturer.manufacturerID),
                        SubstituteKey = typeof(FSManufacturer.manufacturerCD),
                        DescriptionField = typeof(FSManufacturer.descr))]
        public virtual int? ManufacturerID { get; set; }
        #endregion
        #region ManufacturerModelID
        public abstract class manufacturerModelID : PX.Data.BQL.BqlInt.Field<manufacturerModelID> { }
        [PXDBInt]
        [PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Manufacturer Model", Visibility = PXUIVisibility.SelectorVisible)]
        [PXSelector(typeof(
                            Search<FSManufacturerModel.manufacturerModelID,
                            Where<
                                FSManufacturerModel.manufacturerID, Equal<Current<manufacturerID>>>>),
                           SubstituteKey = typeof(FSManufacturerModel.manufacturerModelCD),
                           DescriptionField = typeof(FSManufacturerModel.descr))]
        [PXFormula(typeof(Default<manufacturerID>))]
        public virtual int? ManufacturerModelID { get; set; }
		#endregion
		#region ModelType
		public abstract class modelType : ListField_ModelType { }
        [PXDBString(2, IsFixed = true)]
        [ListField_ModelType.ListAtrribute]
        [PXDefault(ID.ModelType.EQUIPMENT, PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Model Type")]
        public virtual string ModelType { get; set; }
        #endregion
        #region EquipmentItemClass
        public abstract class equipmentItemClass : PX.Data.BQL.BqlString.Field<equipmentItemClass> { }
        protected string _EquipmentItemClass;
        [PXDBString(2, IsFixed = true)]
        [PXDefault(ID.Equipment_Item_Class.PART_OTHER_INVENTORY, PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Equipment Class")]
        public virtual string EquipmentItemClass
        {
            get
            {
                return this._EquipmentItemClass;
            }

            set
            {
                this._EquipmentItemClass = value;
                if (this._EquipmentItemClass == ID.Equipment_Item_Class.MODEL_EQUIPMENT
                    || this._EquipmentItemClass == ID.Equipment_Item_Class.COMPONENT)
                {
                    EQEnabled = true;
                }
                else
                {
                    EQEnabled = false;
                }
            }
        }
        #endregion
        #region EquipmentTypeID
        public abstract class equipmentTypeID : PX.Data.BQL.BqlInt.Field<equipmentTypeID> { }
        [PXDBInt]
        [PXUIField(DisplayName = "Equipment Type")]
        [FSSelectorEquipmentType]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual int? EquipmentTypeID { get; set; }
        #endregion
        #region CompanyWarrantyValue
        public abstract class companyWarrantyValue : PX.Data.BQL.BqlInt.Field<companyWarrantyValue> { }
        [PXDBInt(MinValue = 0)]
        [PXDefault(0, PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Company Warranty")]
        public virtual int? CompanyWarrantyValue { get; set; }
		#endregion
		#region CompanyWarrantyType
		public abstract class companyWarrantyType : ListField_WarrantyDurationType { }
        [PXDBString(1, IsFixed = true)]
        [companyWarrantyType.ListAtrribute]
        [PXDefault(ID.WarrantyDurationType.MONTH, PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Company Warranty Type")]
        public virtual string CompanyWarrantyType { get; set; }
        #endregion
        #region VendorWarrantyValue
        public abstract class vendorWarrantyValue : PX.Data.BQL.BqlInt.Field<vendorWarrantyValue> { }
        [PXDBInt(MinValue = 0)]
        [PXDefault(0, PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Vendor Warranty")]
        public virtual int? VendorWarrantyValue { get; set; }
		#endregion
		#region VendorWarrantyType
		public abstract class vendorWarrantyType : ListField_WarrantyDurationType { }
        [PXDBString(1, IsFixed = true)]
        [vendorWarrantyType.ListAtrribute]
        [PXDefault(ID.WarrantyDurationType.MONTH, PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Vendor Warranty Type")]
        public virtual string VendorWarrantyType { get; set; }
        #endregion
        #region ChkEquipmentManagement
        public abstract class ChkEquipmentManagement : PX.Data.BQL.BqlBool.Field<ChkEquipmentManagement> { }
        [PXBool]
        [PXUIField(Visible = false)]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual bool? chkEquipmentManagement
        {
            get
            {
                return true;
            }
        }
        #endregion

        #region Mem_ShowComponent
        // This memory field exists to show the Component tab according to the values of the screen
        public abstract class mem_ShowComponent : PX.Data.BQL.BqlBool.Field<mem_ShowComponent> { }
        [PXBool]
        [PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(Visible = false, Enabled = false)]
        public virtual bool Mem_ShowComponent { get; set; }
        #endregion
    }
}
