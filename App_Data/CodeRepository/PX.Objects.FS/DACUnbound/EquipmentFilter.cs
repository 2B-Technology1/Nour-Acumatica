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
using PX.Objects.CR;
using PX.Objects.CS;

namespace PX.Objects.FS
{
    [System.Serializable]
    public class EquipmentFilter : IBqlTable
    { 
        #region CustomerID
        public abstract class customerID : PX.Data.BQL.BqlInt.Field<customerID> { }

        [PXInt]
        [PXUIField(DisplayName = "Customer ID")]
        [FSSelectorCustomer]
        public virtual int? CustomerID { get; set; }
        #endregion
        #region CustomerLocationID
        public abstract class customerLocationID : PX.Data.BQL.BqlInt.Field<customerLocationID> { }

        [PXInt]
        [LocationID(typeof(Where<Location.bAccountID, Equal<Current<EquipmentFilter.customerID>>>),
                    DescriptionField = typeof(Location.descr), DisplayName = "Customer Location ID", DirtyRead = true)]
        [PXDefault(typeof(Search<BAccount.defLocationID,
                          Where<
                            BAccount.bAccountID, Equal<Current<customerID>>>>),
                   PersistingCheck = PXPersistingCheck.Nothing)]
        [PXFormula(typeof(Default<customerID>))]
        public virtual int? CustomerLocationID { get; set; }
        #endregion
        #region InventoryID
        public abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }

        [EquipmentModelItem(Filterable = true)]
        public virtual int? InventoryID { get; set; }
        #endregion
        #region OwnerID
        public abstract class ownerID : PX.Data.BQL.BqlInt.Field<ownerID> { }

        [PXInt]
        [PXUIField(DisplayName = "Owner ID")]
        [FSSelectorCustomer]
        public virtual int? OwnerID { get; set; }
        #endregion
        #region RefNbr
        public abstract class refNbr : PX.Data.BQL.BqlString.Field<refNbr> { }

        [PXString]
        [PXUIField(DisplayName = "Equipment Nbr.")]
        [FSSelectorSMEquipmentRefNbr]
        public virtual string RefNbr { get; set; }
        #endregion
        #region RequireMaintenance
        public abstract class requireMaintenance : PX.Data.BQL.BqlBool.Field<requireMaintenance> { }

        [PXBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Show only maintenance equipment")]
        public virtual bool? RequireMaintenance { get; set; }
        #endregion
        #region ResourceEquipment
        public abstract class resourceEquipment : PX.Data.BQL.BqlBool.Field<resourceEquipment> { }

        [PXBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Show only resource equipment")]
        public virtual bool? ResourceEquipment { get; set; }
        #endregion
        #region WarrantyLess
        public abstract class warrantyLess : PX.Data.BQL.BqlBool.Field<warrantyLess> { }

        [PXBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Show only equipment without warranties and components")]
        public virtual bool? WarrantyLess { get; set; }
        #endregion
    }
}