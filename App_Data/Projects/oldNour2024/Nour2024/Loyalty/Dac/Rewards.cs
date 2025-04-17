using System;
using PX.Data;
using PX.Objects.IN;

namespace Nour20240918
{
  [Serializable]
  [PXCacheName("Rewards")]
  public class Rewards : IBqlTable
  {
    #region Id
    [PXDBIdentity(IsKey = true)]
    public virtual int? Id { get; set; }
    public abstract class id : PX.Data.BQL.BqlInt.Field<id> { }
    #endregion

    #region Name
    [PXDBString(100, IsUnicode = true, InputMask = "")]
    [PXUIField(DisplayName = "Name")]
    public virtual string Name { get; set; }
    public abstract class name : PX.Data.BQL.BqlString.Field<name> { }
    #endregion

    #region Item
    [PXDBString(100, IsUnicode = true, InputMask = "")]
    [PXUIField(DisplayName = "Item")]
    [PXSelector(
    typeof(Search<InventoryItem.inventoryCD>),
    typeof(InventoryItem.inventoryCD),
    typeof(InventoryItem.descr),
    typeof(InventoryItem.itemClassID),
    typeof(InventoryItem.baseUnit),
    SubstituteKey = typeof(InventoryItem.inventoryCD),
    DescriptionField = typeof(InventoryItem.descr))]
    public virtual string Item { get; set; }
    public abstract class item : PX.Data.BQL.BqlString.Field<item> { }
    #endregion

    #region Points
    [PXDBInt()]
    [PXUIField(DisplayName = "Points")]
        [PXDefault(0)]
        public virtual int? Points { get; set; }
    public abstract class points : PX.Data.BQL.BqlInt.Field<points> { }
    #endregion

    #region CreatedByID
    [PXDBCreatedByID()]
    public virtual Guid? CreatedByID { get; set; }
    public abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID> { }
    #endregion

    #region CreatedByScreen
    [PXDBString(8, IsFixed = true, InputMask = "")]
    [PXUIField(DisplayName = "Created By Screen")]
    public virtual string CreatedByScreen { get; set; }
    public abstract class createdByScreen : PX.Data.BQL.BqlString.Field<createdByScreen> { }
    #endregion

    #region CreatedDateTime
    [PXDBCreatedDateTime()]
    public virtual DateTime? CreatedDateTime { get; set; }
    public abstract class createdDateTime : PX.Data.BQL.BqlDateTime.Field<createdDateTime> { }
    #endregion

    #region LastModifiedByID
    [PXDBLastModifiedByID()]
    public virtual Guid? LastModifiedByID { get; set; }
    public abstract class lastModifiedByID : PX.Data.BQL.BqlGuid.Field<lastModifiedByID> { }
    #endregion

    #region LastModifiedByScreen
    [PXDBString(8, IsFixed = true, InputMask = "")]
    [PXUIField(DisplayName = "Last Modified By Screen")]
    public virtual string LastModifiedByScreen { get; set; }
    public abstract class lastModifiedByScreen : PX.Data.BQL.BqlString.Field<lastModifiedByScreen> { }
    #endregion

    #region LastModiyedDateTime
    [PXDBDate()]
    [PXUIField(DisplayName = "Last Modiyed Date Time")]
    public virtual DateTime? LastModiyedDateTime { get; set; }
    public abstract class lastModiyedDateTime : PX.Data.BQL.BqlDateTime.Field<lastModiyedDateTime> { }
    #endregion
  }
}