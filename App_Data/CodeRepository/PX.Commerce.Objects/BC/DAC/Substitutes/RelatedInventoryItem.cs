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

using PX.Commerce.Core;
using PX.Data;
using PX.Objects.IN;
using PX.Objects.IN.RelatedItems;
using System;

namespace PX.Commerce.Objects.Substitutes
{
	[Serializable]
	[PXHidden]
	public class BCChildrenInventoryItem : InventoryItem
	{
		#region InventoryID
		[PXDBInt]
		[PXUIField(DisplayName = "Inventory ID")]
		public virtual int? InventoryID { get; set; }
		public abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }
		#endregion
		#region InventoryCD
		[PXDBString]
		[PXUIField(DisplayName = "Inventory CD")]
		public virtual string InventoryCD { get; set; }
		public abstract class inventoryCD : PX.Data.BQL.BqlString.Field<inventoryCD> { }
		#endregion
		#region NoteID
		public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }
		[PXDBGuid()]
		[PXUIField(DisplayName = "NoteID")]
		public virtual Guid? NoteID
		{
			get; set;
		}
		#endregion
	}
	[Serializable]
	[PXHidden]
	public class BCChildrenRelatedInventory : INRelatedInventory
	{
		#region InventoryID
		[PXDBInt(IsKey = true)]
		[PXUIField(DisplayName = "Inventory ID")]
		public virtual int? InventoryID { get; set; }
		public abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }
		#endregion
		#region InventoryCD
		[PXDBInt]
		[PXUIField(DisplayName = "Related Inventory ID")]
		public virtual int? RelatedInventoryID { get; set; }
		public abstract class relatedInventoryID : PX.Data.BQL.BqlInt.Field<relatedInventoryID> { }
		#endregion
		#region IsActive
		[PXDBBool]
		[PXUIField(DisplayName = "Is Active")]
		public virtual bool? IsActive { get; set; }
		public abstract class isActive : PX.Data.BQL.BqlBool.Field<isActive> { }
		#endregion
	}
	[Serializable]
	[PXHidden]
	public class BCParentInventoryItem : InventoryItem
	{
		#region InventoryID
		[PXDBInt]
		[PXUIField(DisplayName = "Inventory ID")]
		public virtual int? InventoryID { get; set; }
		public abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }
		#endregion
		#region InventoryCD
		[PXDBString]
		[PXUIField(DisplayName = "Inventory CD")]
		public virtual string InventoryCD { get; set; }
		public abstract class inventoryCD : PX.Data.BQL.BqlString.Field<inventoryCD> { }
		#endregion
		#region TemplateItemID
		public abstract class templateItemID : PX.Data.BQL.BqlInt.Field<templateItemID> { }
		public virtual int? TemplateItemID { set; get; }
		#endregion
		#region NoteID
		public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }
		[PXDBGuid()]
		[PXUIField(DisplayName = "NoteID")]
		public virtual Guid? NoteID
		{
			get; set;
		}
		#endregion
	}
	[Serializable]
	[PXHidden]
	public class ChildSyncStatus : BCSyncStatus
	{
		#region SyncID
		public abstract class syncID : PX.Data.BQL.BqlInt.Field<BCSyncStatus.syncID> { }
		[PXDBIdentity(IsKey = true)]
		public virtual int? SyncID { get; set; }
		#endregion
		#region ConnectorType
		[PXDBString()]
		public virtual string ConnectorType { get; set; }
		public abstract class connectorType : PX.Data.BQL.BqlString.Field<connectorType> { }
		#endregion
		#region BindingID
		[PXDBInt()]
		public virtual int? BindingID { get; set; }
		public abstract class bindingID : PX.Data.BQL.BqlInt.Field<bindingID> { }
		#endregion+
		#region EntityType
		public abstract class entityType : PX.Data.BQL.BqlString.Field<entityType> { }
		[PXDBString()]
		public virtual string EntityType { get; set; }
		#endregion

		#region LocalID
		public abstract class localID : PX.Data.BQL.BqlGuid.Field<localID> { }
		[PXDBGuid()]
		public virtual Guid? LocalID { get; set; }
		#endregion

		#region ExternID
		public abstract class externID : PX.Data.BQL.BqlString.Field<externID> { }
		[PXDBString(64, IsUnicode = true)]
		public virtual string ExternID { get; set; }
		#endregion
	}
}
