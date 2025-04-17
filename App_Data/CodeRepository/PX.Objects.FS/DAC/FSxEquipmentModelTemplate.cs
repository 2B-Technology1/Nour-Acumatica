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
    public class FSxEquipmentModelTemplate : PXCacheExtension<INItemClass>
    {
        public static bool IsActive()
        {
            return PXAccess.FeatureInstalled<FeaturesSet.equipmentManagementModule>();
        }

		#region DefaultEquipmentModelType
		public abstract class defaultEquipmentModelType : ListField_ModelType { }
        [PXDBString(2, IsFixed = true)]
        [ListField_ModelType.ListAtrribute]
        [PXDefault(ID.ModelType.EQUIPMENT, PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Model Type")]
        public virtual string DefaultEquipmentModelType { get; set; }
        #endregion
        #region EQEnabled
        public abstract class eQEnabled : PX.Data.BQL.BqlBool.Field<eQEnabled> { }
        [PXDBBool]
        [PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Model Equipment Class", Visible = false)]
        public virtual bool? EQEnabled { get; set; }
		#endregion
		#region EquipmentItemClass
		public abstract class equipmentItemClass : ListField_EquipmentItemClass { }
        protected string _EquipmentItemClass;
        [PXDBString(2, IsFixed = true)]
        [equipmentItemClass.ListAtrribute]
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
