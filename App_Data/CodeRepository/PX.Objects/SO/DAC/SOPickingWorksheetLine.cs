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
using PX.Data.ReferentialIntegrity.Attributes;
using System;
using PX.Objects.IN;

namespace PX.Objects.SO
{
	[PXCacheName(Messages.SOPickingWorksheetLine, PXDacType.Details)]
	public class SOPickingWorksheetLine : IBqlTable
	{
		#region Keys
		public class PK : PrimaryKeyOf<SOPickingWorksheetLine>.By<worksheetNbr, lineNbr>
		{
			public static SOPickingWorksheetLine Find(PXGraph graph, string worksheetNbr, int? lineNbr, PKFindOptions options = PKFindOptions.None)
				=> FindBy(graph, worksheetNbr, lineNbr, options);
		}

		public static class FK
		{
			public class Worksheet : SOPickingWorksheet.PK.ForeignKeyOf<SOPickingWorksheetLine>.By<worksheetNbr> { }
			public class Site : INSite.PK.ForeignKeyOf<SOPickingWorksheetLine>.By<siteID> { }
			public class SiteStatus : IN.INSiteStatus.PK.ForeignKeyOf<SOPickingWorksheetLine>.By<inventoryID, subItemID, siteID> { }
			public class Location : INLocation.PK.ForeignKeyOf<SOPickingWorksheetLine>.By<locationID> { }
			public class LocationStatus : IN.INLocationStatus.PK.ForeignKeyOf<SOPickingWorksheetLine>.By<inventoryID, subItemID, siteID, locationID> { }
			public class InventoryItem : IN.InventoryItem.PK.ForeignKeyOf<SOPickingWorksheetLine>.By<inventoryID> { }
			public class SubItem : INSubItem.PK.ForeignKeyOf<SOPickingWorksheetLine>.By<subItemID> { }
			//todo public class UnitOfMeasure : INUnit.PK.ForeignKeyOf<SOPickingWorksheetLine>.By<inventoryID, uOM> { }
		}
		#endregion

		#region WorksheetNbr
		[PXDBString(15, IsUnicode = true, IsKey = true, InputMask = "")]
		[PXDBDefault(typeof(SOPickingWorksheet.worksheetNbr))]
		[PXUIField(DisplayName = "Worksheet Nbr.", Visible = false, Enabled = false)]
		[PXParent(typeof(FK.Worksheet))]
		public virtual String WorksheetNbr { get; set; }
		public abstract class worksheetNbr : PX.Data.BQL.BqlString.Field<worksheetNbr> { }
		#endregion
		#region LineNbr
		[PXDBInt(IsKey = true)]
		[PXLineNbr(typeof(SOPickingWorksheet))]
		[PXUIField(DisplayName = "Line Nbr.", Visible = false, Enabled = false)]
		public virtual Int32? LineNbr { get; set; }
		public abstract class lineNbr : PX.Data.BQL.BqlInt.Field<lineNbr> { }
		#endregion

		#region SiteID
		[Site(Enabled = false)]
		[PXForeignReference(typeof(FK.Site))]
		[PXDefault]
		public virtual Int32? SiteID { get; set; }
		public abstract class siteID : PX.Data.BQL.BqlInt.Field<siteID> { }
		#endregion
		#region LocationID
		[Location(typeof(siteID), Enabled = false)]
		[PXForeignReference(typeof(FK.Location))]
		public virtual Int32? LocationID { get; set; }
		public abstract class locationID : PX.Data.BQL.BqlInt.Field<locationID> { }
		#endregion
		#region InventoryID
		[Inventory(Enabled = false)]
		[PXForeignReference(typeof(FK.InventoryItem))]
		public virtual Int32? InventoryID { get; set; }
		public abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }
		#endregion
		#region SubItemID
		[SubItem(typeof(inventoryID), Enabled = false)]
		public virtual Int32? SubItemID { get; set; }
		public abstract class subItemID : PX.Data.BQL.BqlInt.Field<subItemID> { }
		#endregion
		#region LotSerialNbr
		[PXDBString(INLotSerialStatusByCostCenter.lotSerialNbr.Length, IsUnicode = true, InputMask = "")]
		[PXUIField(DisplayName = "Lot/Serial Nbr.", FieldClass = "LotSerial", Enabled = false)]
		public virtual String LotSerialNbr { get; set; }
		public abstract class lotSerialNbr : PX.Data.BQL.BqlString.Field<lotSerialNbr> { }
		#endregion
		#region ExpireDate
		[PXDBDate(InputMask = "d", DisplayMask = "d")]
		[PXUIField(DisplayName = "Expiration Date", FieldClass = "LotSerial", Enabled = false)]
		public virtual DateTime? ExpireDate { get; set; }
		public abstract class expireDate : PX.Data.BQL.BqlDateTime.Field<expireDate> { }
		#endregion
		#region UOM
		[INUnit(typeof(inventoryID), DisplayName = "UOM", Enabled = false)]
		public virtual String UOM { get; set; }
		public abstract class uOM : PX.Data.BQL.BqlString.Field<uOM> { }
		#endregion
		#region Qty
		[PXDBQuantity(typeof(uOM), typeof(baseQty))]
		[PXUIField(DisplayName = "Shipped Qty.", Enabled = false)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? Qty { get; set; }
		public abstract class qty : PX.Data.BQL.BqlDecimal.Field<qty> { }
		#endregion
		#region BaseQty
		[PXDBDecimal(6)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Base Shipped Qty.", Visible = false, Enabled = false)]
		public virtual Decimal? BaseQty { get; set; }
		public abstract class baseQty : PX.Data.BQL.BqlDecimal.Field<baseQty> { }
		#endregion
		#region OrigOrderQty
		[PXDBQuantity]
		[PXUIField(DisplayName = "Ordered Qty.", Enabled = false)]
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual Decimal? OrigOrderQty { get; set; }
		public abstract class origOrderQty : PX.Data.BQL.BqlDecimal.Field<origOrderQty> { }
		#endregion
		#region BaseOrigOrderQty
		[PXDBBaseQuantity(typeof(uOM), typeof(origOrderQty))]
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual Decimal? BaseOrigOrderQty { get; set; }
		public abstract class baseOrigOrderQty : PX.Data.BQL.BqlDecimal.Field<baseOrigOrderQty> { }
		#endregion
		#region OpenOrderQty
		[PXDBQuantity]
		[PXUIField(DisplayName = "Open Qty.", Enabled = false)]
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual Decimal? OpenOrderQty { get; set; }
		public abstract class openOrderQty : PX.Data.BQL.BqlDecimal.Field<openOrderQty> { }
		#endregion
		#region UnassignedQty
		[PXDBQuantity]
		[PXUIField(DisplayName = "Unassigned Qty.", Enabled = false)]
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual Decimal? UnassignedQty { get; set; }
		public abstract class unassignedQty : PX.Data.BQL.BqlDecimal.Field<unassignedQty> { }
		#endregion
		#region PickedQty
		[PXDBQuantity(typeof(uOM), typeof(basePickedQty))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Picked Qty.", Enabled = false)]
		public virtual Decimal? PickedQty { get; set; }
		public abstract class pickedQty : PX.Data.BQL.BqlDecimal.Field<pickedQty> { }
		#endregion
		#region BasePickedQty
		[PXDBDecimal(6)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? BasePickedQty { get; set; }
		public abstract class basePickedQty : PX.Data.BQL.BqlDecimal.Field<basePickedQty> { }
		#endregion
		#region TranDesc
		[PXDBString(256, IsUnicode = true)]
		[PXUIField(DisplayName = "Description")]
		public virtual String TranDesc { get; set; }
		public abstract class tranDesc : PX.Data.BQL.BqlString.Field<tranDesc> { }
		#endregion

		#region Audit Fields
		#region tstamp
		[PXDBTimestamp(VerifyTimestamp = VerifyTimestampOptions.BothFromGraphAndRecord)]
		public virtual Byte[] tstamp { get; set; }
		public abstract class Tstamp : PX.Data.BQL.BqlByteArray.Field<Tstamp> { }
		#endregion
		#region CreatedByID
		[PXDBCreatedByID]
		public virtual Guid? CreatedByID { get; set; }
		public abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID> { }
		#endregion
		#region CreatedByScreenID
		[PXDBCreatedByScreenID]
		[PXUIField(DisplayName = "Created At", Enabled = false, IsReadOnly = true)]
		public virtual String CreatedByScreenID { get; set; }
		public abstract class createdByScreenID : PX.Data.BQL.BqlString.Field<createdByScreenID> { }
		#endregion
		#region CreatedDateTime
		[PXDBCreatedDateTime]
		[PXUIField(DisplayName = PXDBLastModifiedByIDAttribute.DisplayFieldNames.CreatedDateTime, Enabled = false, IsReadOnly = true)]
		public virtual DateTime? CreatedDateTime { get; set; }
		public abstract class createdDateTime : PX.Data.BQL.BqlDateTime.Field<createdDateTime> { }
		#endregion
		#region LastModifiedByID
		[PXDBLastModifiedByID]
		[PXUIField(DisplayName = PXDBLastModifiedByIDAttribute.DisplayFieldNames.LastModifiedByID, Enabled = false, IsReadOnly = true)]
		public virtual Guid? LastModifiedByID { get; set; }
		public abstract class lastModifiedByID : PX.Data.BQL.BqlGuid.Field<lastModifiedByID> { }
		#endregion
		#region LastModifiedByScreenID
		[PXDBLastModifiedByScreenID]
		[PXUIField(DisplayName = "Last Modified At", Enabled = false, IsReadOnly = true)]
		public virtual String LastModifiedByScreenID { get; set; }
		public abstract class lastModifiedByScreenID : PX.Data.BQL.BqlString.Field<lastModifiedByScreenID> { }
		#endregion
		#region LastModifiedDateTime
		[PXDBLastModifiedDateTime]
		[PXUIField(DisplayName = PXDBLastModifiedByIDAttribute.DisplayFieldNames.LastModifiedDateTime, Enabled = false, IsReadOnly = true)]
		public virtual DateTime? LastModifiedDateTime { get; set; }
		public abstract class lastModifiedDateTime : PX.Data.BQL.BqlDateTime.Field<lastModifiedDateTime> { }
		#endregion
		#endregion
	}
}
