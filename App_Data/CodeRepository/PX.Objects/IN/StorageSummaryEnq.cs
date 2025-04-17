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
using PX.Data.BQL.Fluent;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.CS;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace PX.Objects.IN
{
	#region DACs
	[PXCacheName(Messages.INStoragePlace, PXDacType.Catalogue)]
	[PXProjection(typeof(
		SelectFrom<INCostSite>.
		LeftJoin<INLocation>.On<INLocation.locationID.IsEqual<INCostSite.costSiteID>>.
		LeftJoin<INCart>.On<INCart.cartID.IsEqual<INCostSite.costSiteID>>.
		LeftJoin<INSite>.On<INLocation.siteID.IfNullThen<INCart.siteID>.IsEqual<INSite.siteID>>.
		Where<INCostSite.costSiteType.IsIn<NameOf<INLocation>, NameOf<INCart>>>
		), Persistent = false)]
	[PXPrimaryGraph(
		new[] { typeof(INSiteMaint) },
		new[] { typeof(SelectFrom<INSite>.Where<INSite.siteID.IsEqual<siteID.FromCurrent>>) })]
	public class StoragePlace : IBqlTable
	{
		#region Keys
		public class PK : PrimaryKeyOf<StoragePlace>.By<siteID, storageID>
		{
			public static StoragePlace Find(PXGraph graph, int? siteID, int? storageID, PKFindOptions options = PKFindOptions.None) => FindBy(graph, siteID, storageID, options);
		}
		public static class FK
		{
			public class Site : INSite.PK.ForeignKeyOf<StoragePlace>.By<siteID> { }
			public class Cart : INCart.PK.ForeignKeyOf<StoragePlace>.By<siteID, cartID> { }
			public class Location : INLocation.PK.ForeignKeyOf<StoragePlace>.By<locationID> { }
		}
		#endregion
		#region SiteID
		[PXDBInt(IsKey = true, BqlTable = typeof(INSite))]
		public int? SiteID { get; set; }
		public abstract class siteID : PX.Data.BQL.BqlInt.Field<siteID> { }
		#endregion
		#region SiteCD
		[PXDBString(BqlTable = typeof(INSite))]
		[PXUIField(DisplayName = "Warehouse ID", Visibility = PXUIVisibility.SelectorVisible)]
		public string SiteCD { get; set; }
		public abstract class siteCD : PX.Data.BQL.BqlString.Field<siteCD> { }
		#endregion
		#region LocationID
		[Location(typeof(siteID), BqlField = typeof(INLocation.locationID), Enabled = false)]
		public virtual Int32? LocationID { get; set; }
		public abstract class locationID : PX.Data.BQL.BqlInt.Field<locationID> { }
		#endregion
		#region CartID
		[PXDBInt(BqlField = typeof(INCart.cartID))]
		[PXUIField(DisplayName = "Cart ID", IsReadOnly = true)]
		[PXSelector(typeof(SearchFor<INCart.cartID>.In<SelectFrom<INCart>.Where<INCart.active.IsEqual<True>>>), SubstituteKey = typeof(INCart.cartCD), DescriptionField = typeof(INCart.descr))]
		public int? CartID { get; set; }
		public abstract class cartID : PX.Data.BQL.BqlInt.Field<cartID> { }
		#endregion
		#region StorageID
		[PXInt(IsKey = true)]
		[PXDBCalced(typeof(INLocation.locationID.IfNullThen<INCart.cartID>), typeof(int))]
		public virtual Int32? StorageID { get; set; }
		public abstract class storageID : PX.Data.BQL.BqlInt.Field<storageID> { }
		#endregion
		#region StorageCD
		[PXString]
		[PXUIField(DisplayName = "Storage ID", IsReadOnly = true)]
		[PXDBCalced(typeof(INLocation.locationCD.IfNullThen<INCart.cartCD>), typeof(string))]
		public virtual string StorageCD { get; set; }
		public abstract class storageCD : PX.Data.BQL.BqlString.Field<storageCD> { }
		#endregion
		#region Descr
		[PXString]
		[PXUIField(DisplayName = "Description", IsReadOnly = true)]
		[PXDBCalced(typeof(INLocation.descr.IfNullThen<INCart.descr>), typeof(string))]
		public virtual String Descr { get; set; }
		public abstract class descr : PX.Data.BQL.BqlString.Field<descr> { }
		#endregion
		#region IsCart
		[PXBool]
		[PXUIField(DisplayName = "Cart", IsReadOnly = true, FieldClass = "Carts")]
		[PXDBCalced(typeof(True.When<INCart.cartID.IsNotNull>.NoDefault.Else<False>), typeof(bool))]
		public virtual Boolean? IsCart { get; set; }
		public abstract class isCart : PX.Data.BQL.BqlBool.Field<isCart> { }
		#endregion
		#region Active
		[PXBool]
		[PXUIField(DisplayName = "Active", IsReadOnly = true)]
		[PXDBCalced(typeof(INLocation.active.IfNullThen<INCart.active>), typeof(bool))]
		public virtual Boolean? Active { get; set; }
		public abstract class active : PX.Data.BQL.BqlBool.Field<active> { }
		#endregion
	}

	[PXHidden]
	public class StoragePlaceStatus : IBqlTable
	{
		#region SplittedIcon
		[Obsolete(Common.Messages.ItemIsObsoleteAndWillBeRemoved2023R1)]
		[PXImage]
		public virtual string SplittedIcon { get; set; }
		public abstract class splittedIcon : PX.Data.BQL.BqlString.Field<splittedIcon> { }
		#endregion
		#region SiteID
		[Site(IsKey = true, Enabled = false)]
		public int? SiteID { get; set; }
		public abstract class siteID : PX.Data.BQL.BqlInt.Field<siteID> { }
		#endregion
		#region SiteCD
		[Obsolete]
		[PXDBString]
		[PXUIField(DisplayName = "Warehouse", FieldClass = SiteAttribute.DimensionName, IsReadOnly = false)]
		public string SiteCD { get; set; }
		public abstract class siteCD : PX.Data.BQL.BqlString.Field<siteCD> { }
		#endregion
		#region LocationID
		[Obsolete(Common.Messages.ItemIsObsoleteAndWillBeRemoved2023R1)]
		[Location(typeof(siteID), Enabled = false)]
		public virtual Int32? LocationID { get; set; }
		public abstract class locationID : PX.Data.BQL.BqlInt.Field<locationID> { }
		#endregion
		#region CartID
		[Obsolete(Common.Messages.ItemIsObsoleteAndWillBeRemoved2023R1)]
		[PXDBInt]
		[PXSelector(typeof(SearchFor<INCart.cartID>.Where<INCart.active.IsEqual<True>>), SubstituteKey = typeof(INCart.cartCD), DescriptionField = typeof(INCart.descr))]
		public int? CartID { get; set; }
		public abstract class cartID : PX.Data.BQL.BqlInt.Field<cartID> { }
		#endregion
		#region StorageID
		[PXDBInt(IsKey = true)]
		public virtual Int32? StorageID { get; set; }
		public abstract class storageID : PX.Data.BQL.BqlInt.Field<storageID> { }
		#endregion
		#region StorageCD
		[PXDBString]
		[PXUIField(DisplayName = "Storage ID", Enabled = false)]
		public string StorageCD { get; set; }
		public abstract class storageCD : PX.Data.BQL.BqlString.Field<storageCD> { }
		#endregion
		#region Descr
		[PXDBString]
		[PXUIField(DisplayName = "Storage Description", Enabled = false)]
		public virtual String Descr { get; set; }
		public abstract class descr : PX.Data.BQL.BqlString.Field<descr> { }
		#endregion
		#region IsCart
		[Obsolete(Common.Messages.ItemIsObsoleteAndWillBeRemoved2023R1)]
		[PXDBBool]
		public virtual Boolean? IsCart { get; set; }
		public abstract class isCart : PX.Data.BQL.BqlBool.Field<isCart> { }
		#endregion
		#region Active
		[Obsolete(Common.Messages.ItemIsObsoleteAndWillBeRemoved2023R1)]
		[PXDBBool]
		[PXUIField(DisplayName = "Active", Enabled = false)]
		public virtual Boolean? Active { get; set; }
		public abstract class active : PX.Data.BQL.BqlBool.Field<active> { }
		#endregion

		#region InventoryID
		[Inventory(IsKey = true, Enabled = false)]
		public virtual Int32? InventoryID { get; set; }
		public abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }
		#endregion
		#region InventoryCD
		[Obsolete]
		[PXDBString]
		[PXUIField(DisplayName = "Inventory ID", Enabled = false)]
		public virtual string InventoryCD { get; set; }
		public abstract class inventoryCD : PX.Data.BQL.BqlString.Field<inventoryCD> { }
		#endregion
		#region InventoryDescr
		[PXDBLocalizableString(Common.Constants.TranDescLength, IsUnicode = true)]
		[PXUIField(DisplayName = "Description", Enabled = false)]
		public virtual String InventoryDescr { get; set; }
		public abstract class inventoryDescr : PX.Data.BQL.BqlString.Field<inventoryDescr> { }
		#endregion
		#region SubItemID
		[PXDBInt(IsKey = true)]
		[PXUIField(DisplayName = "Subitem", FieldClass = SubItemAttribute.DimensionName, Enabled = false)]
		public virtual Int32? SubItemID { get; set; }
		public abstract class subItemID : PX.Data.BQL.BqlInt.Field<subItemID> { }
		#endregion
		#region LotSerialNbr
		[PXString]
		[PXUIField(DisplayName = "Lot/Serial Nbr.", FieldClass = "LotSerial", Enabled = false)]
		public virtual String LotSerialNbr { get; set; }
		public abstract class lotSerialNbr : PX.Data.BQL.BqlString.Field<lotSerialNbr> { }
		#endregion
		#region ExpireDate
		[PXDBDate]
		[PXUIField(DisplayName = "Expiration Date", FieldClass = "LotSerial", Enabled = false)]
		public virtual DateTime? ExpireDate { get; set; }
		public abstract class expireDate : PX.Data.BQL.BqlDateTime.Field<expireDate> { }
		#endregion
		#region Qty
		[PXDBDecimal]
		[PXUIField(DisplayName = "Qty. On Hand", Enabled = false)]
		public virtual Decimal? Qty { get; set; }
		public abstract class qty : PX.Data.BQL.BqlDecimal.Field<qty> { }
		#endregion
		#region QtyPickedToCart
		[PXDBDecimal]
		[PXUIField(DisplayName = "Qty. Picked to Carts", Enabled = false, FieldClass = "Carts")]
		public virtual Decimal? QtyPickedToCart { get; set; }
		public abstract class qtyPickedToCart : PX.Data.BQL.BqlDecimal.Field<qtyPickedToCart> { }
		#endregion
		#region BaseUnit
		[INUnit(DisplayName = "Base Unit", Enabled = false)]
		public virtual String BaseUnit { get; set; }
		public abstract class baseUnit : PX.Data.BQL.BqlString.Field<baseUnit> { }
		#endregion
		#region NoteID
		[Common.Attributes.BorrowedNote(typeof(InventoryItem), typeof(InventoryItemMaint))]
		public virtual Guid? NoteID { get; set; }
		public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }
		#endregion
	}

	[Obsolete(Common.Messages.ItemIsObsoleteAndWillBeRemoved2023R1)]
	[PXHidden]
	public class StoragePlaceStatusExpanded : IBqlTable
	{
		#region SplittedIcon
		[PXImage]
		[PXFormula(typeof(GL.SplitIcon.split))]
		public virtual string SplittedIcon { get; set; }
		public abstract class splittedIcon : PX.Data.BQL.BqlString.Field<splittedIcon> { }
		#endregion
		#region SiteID
		[PXDBInt(IsKey = true, BqlTable = typeof(INLocation))]
		public int? SiteID { get; set; }
		public abstract class siteID : PX.Data.BQL.BqlInt.Field<siteID> { }
		#endregion
		#region SiteCD
		[PXDBString(BqlTable = typeof(INSite))]
		public string SiteCD { get; set; }
		public abstract class siteCD : PX.Data.BQL.BqlString.Field<siteCD> { }
		#endregion
		#region LocationID
		[Location(typeof(siteID), BqlTable = typeof(INLocation), Enabled = false)]
		public virtual Int32? LocationID { get; set; }
		public abstract class locationID : PX.Data.BQL.BqlInt.Field<locationID> { }
		#endregion
		#region StorageID
		[PXDBInt(IsKey = true, BqlField = typeof(INLocation.locationID))]
		public virtual Int32? StorageID { get; set; }
		public abstract class storageID : PX.Data.BQL.BqlInt.Field<storageID> { }
		#endregion
		#region StorageCD
		[PXDBString(BqlField = typeof(INLocation.locationCD))]
		public string StorageCD { get; set; }
		public abstract class storageCD : PX.Data.BQL.BqlString.Field<storageCD> { }
		#endregion
		#region Descr
		[PXDBString(BqlTable = typeof(INLocation))]
		public virtual String Descr { get; set; }
		public abstract class descr : PX.Data.BQL.BqlString.Field<descr> { }
		#endregion
		#region Active
		[PXDBBool(BqlTable = typeof(INLocation))]
		public virtual Boolean? Active { get; set; }
		public abstract class active : PX.Data.BQL.BqlBool.Field<active> { }
		#endregion

		#region InventoryID
		[PXDBInt(BqlTable = typeof(InventoryItem))]
		public virtual Int32? InventoryID { get; set; }
		public abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }
		#endregion
		#region InventoryCD
		[PXDBString(BqlTable = typeof(InventoryItem))]
		public virtual string InventoryCD { get; set; }
		public abstract class inventoryCD : PX.Data.BQL.BqlString.Field<inventoryCD> { }
		#endregion
		#region SubItemID
		[PXDBInt(BqlTable = typeof(INLotSerialStatus))]
		public virtual Int32? SubItemID { get; set; }
		public abstract class subItemID : PX.Data.BQL.BqlInt.Field<subItemID> { }
		#endregion
		#region LotSerialNbr
		[PXDBString(BqlTable = typeof(INLotSerialStatus))]
		public virtual String LotSerialNbr { get; set; }
		public abstract class lotSerialNbr : PX.Data.BQL.BqlString.Field<lotSerialNbr> { }
		#endregion
		#region ExpireDate
		[PXDBDate(BqlTable = typeof(INLotSerialStatus))]
		public virtual DateTime? ExpireDate { get; set; }
		public abstract class expireDate : PX.Data.BQL.BqlDateTime.Field<expireDate> { }
		#endregion
		#region Qty
		[PXDBDecimal(BqlField = typeof(INLotSerialStatus.qtyOnHand))]
		public virtual Decimal? Qty { get; set; }
		public abstract class qty : PX.Data.BQL.BqlDecimal.Field<qty> { }
		#endregion
		#region BaseUnit
		[INUnit(DisplayName = "Base Unit", BqlTable = typeof(InventoryItem), Enabled = false)]
		public virtual String BaseUnit { get; set; }
		public abstract class baseUnit : PX.Data.BQL.BqlString.Field<baseUnit> { }
		#endregion
	}
	#endregion

	public class StoragePlaceEnq : PXGraph<StoragePlaceEnq>
	{
		public PXFilter<StoragePlaceFilter> Filter;

		[PXFilterable]
		public
			SelectFrom<StoragePlaceStatus>.
			View storages;
		public virtual IEnumerable Storages()
		{
			IEnumerable Execute<TResult>(PXSelectBase select, Func<TResult, StoragePlaceStatus> convertor)
				where TResult : PXResult
			{
				var alteredFilters = PXView.Filters
					.Cast<PXFilterRow>()
					.Select(RemapFilterField)
					.Where(fr => fr != null)
					.ToArray();

				bool hasFiltersOverVirtualFields = alteredFilters.Length < PXView.Filters.Length;

				var result = new PXDelegateResult
				{
					IsResultSorted = true,
					IsResultTruncated = !hasFiltersOverVirtualFields
				};

				int startRow = result.IsResultTruncated ? PXView.StartRow : 0;
				int maxRows = result.IsResultTruncated ? PXView.MaximumRows : 0;
				int totalRows = 0;

				foreach (TResult element in select.View.Select(
					PXView.Currents,
					PXView.Parameters,
					new object[0],
					new string[0],
					new bool[0],
					alteredFilters,
					ref startRow, maxRows,
					ref totalRows))
				{
					var storageStatus = convertor(element);
					result.Add(storageStatus);
					storages.Cache.Hold(storageStatus);
				}

				if (result.IsResultTruncated)
				{
					PXView.StartRow = 0;
				}
				else if (PXView.ReverseOrder)
				{
					result.Reverse();
				}

				return result;
			}

			PXFilterRow RemapFilterField(PXFilterRow fr)
			{
				if (string.Equals(fr.DataField, nameof(StoragePlaceStatus.SiteID), StringComparison.OrdinalIgnoreCase))
					return new PXFilterRow(fr) { DataField = nameof(INSite) + "__" + nameof(INSite.SiteCD) };

				if (string.Equals(fr.DataField, nameof(StoragePlaceStatus.InventoryID), StringComparison.OrdinalIgnoreCase))
					return new PXFilterRow(fr) { DataField = nameof(InventoryItem) + "__" + nameof(InventoryItem.InventoryCD) };

				if (string.Equals(fr.DataField, nameof(StoragePlaceStatus.InventoryDescr), StringComparison.OrdinalIgnoreCase))
					return new PXFilterRow(fr) { DataField = nameof(InventoryItem) + "__" + nameof(InventoryItem.Descr) };

				if (string.Equals(fr.DataField, nameof(StoragePlaceStatus.BaseUnit), StringComparison.OrdinalIgnoreCase))
					return new PXFilterRow(fr) { DataField = nameof(InventoryItem) + "__" + nameof(InventoryItem.BaseUnit) };

				if (string.Equals(fr.DataField, nameof(StoragePlaceStatus.SubItemID), StringComparison.OrdinalIgnoreCase))
					return Filter.Current.StorageType == StoragePlaceFilter.storageType.Locations
						? new PXFilterRow(fr) { DataField = nameof(INLotSerialStatus) + "__" + nameof(INLotSerialStatus.SubItemID) }
						: new PXFilterRow(fr) { DataField = nameof(INCartSplit) + "__" + nameof(INCartSplit.SubItemID) };

				if (string.Equals(fr.DataField, nameof(StoragePlaceStatus.LotSerialNbr), StringComparison.OrdinalIgnoreCase))
					return Filter.Current.StorageType == StoragePlaceFilter.storageType.Locations
						? new PXFilterRow(fr) { DataField = nameof(INLotSerialStatus) + "__" + nameof(INLotSerialStatus.LotSerialNbr) }
						: new PXFilterRow(fr) { DataField = nameof(INCartSplit) + "__" + nameof(INCartSplit.LotSerialNbr) };

				if (string.Equals(fr.DataField, nameof(StoragePlaceStatus.StorageCD), StringComparison.OrdinalIgnoreCase))
					return Filter.Current.StorageType == StoragePlaceFilter.storageType.Locations
						? new PXFilterRow(fr) { DataField = nameof(INLocation.LocationCD) }
						: new PXFilterRow(fr) { DataField = nameof(INCart.CartCD) };

				if (GridVirtualFields.Any(vf => string.Equals(fr.DataField, vf.Name, StringComparison.OrdinalIgnoreCase)))
					return null;

				return fr;
			}

			if (Filter.Current.StorageType == StoragePlaceFilter.storageType.Locations)
			{
				if (Filter.Current.ExpandByLotSerialNbr == true)
				{
					var expandedLocationsSelect = new
						SelectFrom<INLocation>.
						InnerJoin<INLocationStatus>.On<INLocationStatus.FK.Location>.
						LeftJoin<INLotSerialStatus>.On<
							INLotSerialStatus.FK.LocationStatus.
							And<INLotSerialStatus.qtyOnHand.IsGreater<Zero>>>.
						InnerJoin<INSite>.On<INLocation.FK.Site>.
						InnerJoin<InventoryItem>.On<INLocationStatus.FK.InventoryItem>.
						Where<
							StoragePlaceFilter.siteID.FromCurrent.NoDefault.IsEqual<INSite.siteID>.
							And<INLocationStatus.qtyOnHand.IsGreater<Zero>>.
							And<INLocation.active.IsEqual<True>>>.
						OrderBy<
							INSite.siteCD.Asc,
							INLocation.locationCD.Asc,
							InventoryItem.inventoryCD.Asc,
							INLocationStatus.subItemID.Asc,
							INLotSerialStatus.lotSerialNbr.Asc,
							INLocationStatus.qtyOnHand.Desc,
							INLotSerialStatus.qtyOnHand.Desc>.
						View(this);

					if (Filter.Current.LocationID != null)
						expandedLocationsSelect.WhereAnd<Where<StoragePlaceFilter.locationID.FromCurrent.IsEqual<INLocation.locationID>>>();
					if (Filter.Current.InventoryID != null)
						expandedLocationsSelect.WhereAnd<Where<StoragePlaceFilter.inventoryID.FromCurrent.IsEqual<InventoryItem.inventoryID>>>();
					if (PXAccess.FeatureInstalled<FeaturesSet.subItem>() && Filter.Current.SubItemID != null)
						expandedLocationsSelect.WhereAnd<Where<StoragePlaceFilter.subItemID.FromCurrent.IsEqual<INLocationStatus.subItemID>>>();
					if (string.IsNullOrEmpty(Filter.Current.LotSerialNbr) == false)
						expandedLocationsSelect.WhereAnd<Where<StoragePlaceFilter.lotSerialNbr.FromCurrent.IsEqual<INLotSerialStatus.lotSerialNbr>>>();

					bool cartsEnabled = AreCartsInUse();
					if (cartsEnabled)
					{
						expandedLocationsSelect.Join<LeftJoin<INCartContentByLocation, On<INCartContentByLocation.FK.LocationStatus>>>();
						expandedLocationsSelect.Join<LeftJoin<INCartContentByLotSerial, On<INCartContentByLotSerial.FK.LotSerialStatus>>>();
					}

					return Execute<PXResult<INLocation, INLocationStatus, INLotSerialStatus, INSite, InventoryItem>>(expandedLocationsSelect, record =>
					{
						(INLocation location, INLocationStatus locStatus, INLotSerialStatus lsStatus, INSite site, InventoryItem item) = record;
						var cartLocSub = cartsEnabled ? record.GetItem<INCartContentByLocation>() : null;
						var cartLsSub = cartsEnabled ? record.GetItem<INCartContentByLotSerial>() : null;

						return new StoragePlaceStatus
						{
							SiteID = site.SiteID,
							StorageID = location.LocationID,
							StorageCD = location.LocationCD,
							Descr = location.Descr,
							InventoryID = item.InventoryID,
							InventoryDescr = item.Descr,
							SubItemID = locStatus.SubItemID,
							LotSerialNbr = lsStatus.LotSerialNbr,
							ExpireDate = lsStatus.ExpireDate,
							Qty = lsStatus.LotSerialNbr != null
								? (lsStatus.QtyOnHand - (cartLsSub?.BaseQty ?? 0))
								: (locStatus.QtyOnHand - (cartLocSub?.BaseQty ?? 0)),
							QtyPickedToCart = lsStatus.LotSerialNbr != null
								? (cartLsSub?.BaseQty ?? 0)
								: (cartLocSub?.BaseQty ?? 0),
							BaseUnit = item.BaseUnit,
							NoteID = item.NoteID
						};
					});
				}
				else
				{
					var groupedLocationsSelect = new
						SelectFrom<INLocation>.
						InnerJoin<INLocationStatus>.On<INLocationStatus.FK.Location>.
						InnerJoin<INSite>.On<INLocation.FK.Site>.
						InnerJoin<InventoryItem>.On<INLocationStatus.FK.InventoryItem>.
						Where<
							StoragePlaceFilter.siteID.FromCurrent.NoDefault.IsEqual<INSite.siteID>.
							And<INLocationStatus.qtyOnHand.IsGreater<Zero>>.
							And<INLocation.active.IsEqual<True>>>.
						OrderBy<
							INSite.siteCD.Asc,
							INLocation.locationCD.Asc,
							InventoryItem.inventoryCD.Asc,
							INLocationStatus.subItemID.Asc,
							INLocationStatus.qtyOnHand.Desc>.
						View(this);

					if (Filter.Current.LocationID != null)
						groupedLocationsSelect.WhereAnd<Where<StoragePlaceFilter.locationID.FromCurrent.IsEqual<INLocation.locationID>>>();
					if (Filter.Current.InventoryID != null)
						groupedLocationsSelect.WhereAnd<Where<StoragePlaceFilter.inventoryID.FromCurrent.IsEqual<InventoryItem.inventoryID>>>();
					if (PXAccess.FeatureInstalled<FeaturesSet.subItem>() && Filter.Current.SubItemID != null)
						groupedLocationsSelect.WhereAnd<Where<StoragePlaceFilter.subItemID.FromCurrent.IsEqual<INLocationStatus.subItemID>>>();

					bool areCartsInUse = AreCartsInUse();
					if (areCartsInUse)
					{
						groupedLocationsSelect.Join<LeftJoin<INCartContentByLocation, On<INCartContentByLocation.FK.LocationStatus>>>();
					}

					return Execute<PXResult<INLocation, INLocationStatus, INSite, InventoryItem>>(groupedLocationsSelect, record =>
					{
						(INLocation location, INLocationStatus locStatus, INSite site, InventoryItem item) = record;
						var cartLocSub = areCartsInUse ? record.GetItem<INCartContentByLocation>() : null;

						return new StoragePlaceStatus
						{
							SiteID = site.SiteID,
							StorageID = location.LocationID,
							StorageCD = location.LocationCD,
							Descr = location.Descr,
							InventoryID = item.InventoryID,
							InventoryDescr = item.Descr,
							SubItemID = locStatus.SubItemID,
							Qty = locStatus.QtyOnHand - (cartLocSub?.BaseQty ?? 0),
							QtyPickedToCart = (cartLocSub?.BaseQty ?? 0),
							BaseUnit = item.BaseUnit,
							NoteID = item.NoteID
						};
					});
				}
			}
			else if (Filter.Current.StorageType == StoragePlaceFilter.storageType.Carts)
			{
				if (Filter.Current.ExpandByLotSerialNbr == true)
				{
					var expandedCartsSelect = new
						SelectFrom<INCart>.
						InnerJoin<INCartSplit>.On<INCartSplit.FK.Cart>.
						InnerJoin<INSite>.On<INCart.FK.Site>.
						InnerJoin<InventoryItem>.On<INCartSplit.FK.InventoryItem>.
						Where<
							StoragePlaceFilter.siteID.FromCurrent.NoDefault.IsEqual<INSite.siteID>.
							And<INCartSplit.baseQty.IsGreater<Zero>>.
							And<INCart.active.IsEqual<True>>>.
						OrderBy<
							INSite.siteCD.Asc,
							INCart.cartCD.Asc,
							InventoryItem.inventoryCD.Asc,
							INCartSplit.subItemID.Asc,
							INCartSplit.lotSerialNbr.Asc,
							INCartSplit.baseQty.Desc>.
						View(this);

					if (Filter.Current.CartID != null)
						expandedCartsSelect.WhereAnd<Where<StoragePlaceFilter.cartID.FromCurrent.IsEqual<INCart.cartID>>>();
					if (Filter.Current.InventoryID != null)
						expandedCartsSelect.WhereAnd<Where<StoragePlaceFilter.inventoryID.FromCurrent.IsEqual<InventoryItem.inventoryID>>>();
					if (PXAccess.FeatureInstalled<FeaturesSet.subItem>() && Filter.Current.SubItemID != null)
						expandedCartsSelect.WhereAnd<Where<StoragePlaceFilter.subItemID.FromCurrent.IsEqual<INCartSplit.subItemID>>>();
					if (string.IsNullOrEmpty(Filter.Current.LotSerialNbr) == false)
						expandedCartsSelect.WhereAnd<Where<StoragePlaceFilter.lotSerialNbr.FromCurrent.IsEqual<INCartSplit.lotSerialNbr>>>();

					return Execute<PXResult<INCart, INCartSplit, INSite, InventoryItem>>(expandedCartsSelect, record =>
					{
						(INCart cart, INCartSplit split, INSite site, InventoryItem item) = record;
						return new StoragePlaceStatus
						{
							SiteID = site.SiteID,
							StorageID = cart.CartID,
							StorageCD = cart.CartCD,
							Descr = cart.Descr,
							InventoryID = item.InventoryID,
							InventoryDescr = item.Descr,
							SubItemID = split.SubItemID,
							LotSerialNbr = split.LotSerialNbr,
							ExpireDate = split.ExpireDate,
							Qty = split.BaseQty,
							BaseUnit = item.BaseUnit,
							NoteID = item.NoteID
						};
					});
				}
				else
				{
					var groupedCartsSelect = new
						SelectFrom<INCart>.
						InnerJoin<INCartSplit>.On<INCartSplit.FK.Cart>.
						InnerJoin<INSite>.On<INCart.FK.Site>.
						InnerJoin<InventoryItem>.On<INCartSplit.FK.InventoryItem>.
						Where<
							StoragePlaceFilter.siteID.FromCurrent.NoDefault.IsEqual<INSite.siteID>.
							And<INCartSplit.baseQty.IsGreater<Zero>>.
							And<INCart.active.IsEqual<True>>>.
						AggregateTo<
							GroupBy<INSite.siteCD>,
							GroupBy<INCart.cartCD>,
							GroupBy<INCartSplit.inventoryID>,
							GroupBy<INCartSplit.subItemID>,
							Sum<INCartSplit.baseQty>>.
						Having<INCartSplit.qty.Summarized.IsGreater<Zero>>.
						OrderBy<
							INSite.siteCD.Asc,
							INCart.cartCD.Asc,
							InventoryItem.inventoryCD.Asc,
							INCartSplit.subItemID.Asc,
							INCartSplit.baseQty.Desc>.
						View(this);

					if (Filter.Current.CartID != null)
						groupedCartsSelect.WhereAnd<Where<StoragePlaceFilter.cartID.FromCurrent.IsEqual<INCart.cartID>>>();
					if (Filter.Current.InventoryID != null)
						groupedCartsSelect.WhereAnd<Where<StoragePlaceFilter.inventoryID.FromCurrent.IsEqual<InventoryItem.inventoryID>>>();
					if (PXAccess.FeatureInstalled<FeaturesSet.subItem>() && Filter.Current.SubItemID != null)
						groupedCartsSelect.WhereAnd<Where<StoragePlaceFilter.subItemID.FromCurrent.IsEqual<INCartSplit.subItemID>>>();

					return Execute<PXResult<INCart, INCartSplit, INSite, InventoryItem>>(groupedCartsSelect, record =>
					{
						(INCart cart, INCartSplit split, INSite site, InventoryItem item) = record;
						return new StoragePlaceStatus
						{
							SiteID = site.SiteID,
							StorageID = cart.CartID,
							StorageCD = cart.CartCD,
							Descr = cart.Descr,
							InventoryID = item.InventoryID,
							InventoryDescr = item.Descr,
							SubItemID = split.SubItemID,
							Qty = split.BaseQty,
							BaseUnit = item.BaseUnit,
							NoteID = item.NoteID
						};
					});
				}
			}
			else
				throw new NotImplementedException();
		}

		protected virtual IEnumerable<Type> GridVirtualFields => new Type[]
		{
			typeof(StoragePlaceStatus.qty),
			typeof(StoragePlaceStatus.qtyPickedToCart),
			typeof(StoragePlaceStatus.expireDate)
		};

		protected virtual bool AreCartsInUse() => PXAccess.FeatureInstalled<FeaturesSet.wMSCartTracking>() && SelectFrom<INCartSplit>.View.Select(this).Any();

		protected virtual void _(Events.RowUpdated<StoragePlaceFilter> e) => storages.Cache.Clear();
		protected virtual void _(Events.RowInserted<StoragePlaceFilter> e) => storages.Cache.Clear();

		protected virtual void _(Events.RowSelected<StoragePlaceFilter> e)
			=> storages.Cache.AdjustUI()
				.For<StoragePlaceStatus.qtyPickedToCart>(a => a.Visible = e.Row?.StorageType == StoragePlaceFilter.storageType.Locations)
				.For<StoragePlaceStatus.lotSerialNbr>(a => a.Visible = e.Row?.ExpandByLotSerialNbr ?? false)
				.SameFor<StoragePlaceStatus.expireDate>();

		public override bool IsDirty => false;

		[PXHidden]
		public class StoragePlaceFilter : IBqlTable
		{
			#region SiteID
			[Site(Required = true)]
			public int? SiteID { get; set; }
			public abstract class siteID : PX.Data.BQL.BqlInt.Field<siteID> { }
			#endregion
			#region ExpandByLotSerialNbr
			[PXBool]
			[PXUnboundDefault(false)]
			[PXUIField(DisplayName = "Expand by Lot/Serial Number", Visibility = PXUIVisibility.Visible)]
			public virtual bool? ExpandByLotSerialNbr { get; set; }
			public abstract class expandByLotSerialNbr : PX.Data.BQL.BqlBool.Field<expandByLotSerialNbr> { }
			#endregion
			#region StorageID
			[Location(typeof(siteID))]
			[PXUIVisible(typeof(Where<Not<FeatureInstalled<FeaturesSet.wMSCartTracking>>>))]
			[PXFormula(typeof(Default<siteID>))]
			public int? StorageID
			{
				get => LocationID;
				set => LocationID = value;
			}
			public abstract class storageID : PX.Data.BQL.BqlInt.Field<storageID> { }
			#endregion
			#region LocationID
			[Location(typeof(siteID))]
			[PXUIEnabled(typeof(storageType.IsEqual<storageType.locations>))]
			[PXUIVisible(typeof(Where<FeatureInstalled<FeaturesSet.wMSCartTracking>>))]
			[PXFormula(typeof(Default<siteID, storageType>))]
			public virtual Int32? LocationID { get; set; }
			public abstract class locationID : PX.Data.BQL.BqlInt.Field<locationID> { }
			#endregion
			#region CartID
			[PXInt]
			[PXUIField(DisplayName = "Cart ID")]
			[PXUIEnabled(typeof(storageType.IsEqual<storageType.carts>))]
			[PXUIVisible(typeof(Where<FeatureInstalled<FeaturesSet.wMSCartTracking>>))]
			[PXSelector(typeof(SearchFor<INCart.cartID>.Where<INCart.active.IsEqual<True>>), SubstituteKey = typeof(INCart.cartCD), DescriptionField = typeof(INCart.descr))]
			[PXFormula(typeof(Default<siteID, storageType>))]
			public int? CartID { get; set; }
			public abstract class cartID : PX.Data.BQL.BqlInt.Field<cartID> { }
			#endregion
			#region InventoryID
			[StockItem]
			public virtual Int32? InventoryID { get; set; }
			public abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }
			#endregion
			#region SubItemID
			[SubItem(typeof(inventoryID))]
			[PXFormula(typeof(Default<inventoryID>))]
			public virtual Int32? SubItemID { get; set; }
			public abstract class subItemID : PX.Data.BQL.BqlInt.Field<subItemID> { }
			#endregion
			#region LotSerialNbr
			[LotSerialNbr]
			[PXUIEnabled(typeof(expandByLotSerialNbr))]
			[PXFormula(typeof(Default<inventoryID>))]
			public virtual String LotSerialNbr { get; set; }
			public abstract class lotSerialNbr : PX.Data.BQL.BqlString.Field<lotSerialNbr> { }
			#endregion
			#region StorageType
			[PXString]
			[storageType.List]
			[PXUnboundDefault(storageType.Locations)]
			[PXUIField(DisplayName = "Show Storages", FieldClass = "Carts")]
			[PXUIVisible(typeof(Where<FeatureInstalled<FeaturesSet.wMSCartTracking>>))]
			public virtual string StorageType { get; set; }
			public abstract class storageType : PX.Data.BQL.BqlString.Field<storageType>
			{
				public const string Carts = "C";
				public const string Locations = "L";

				[PX.Common.PXLocalizable]
				public static class DisplayNames
				{
					public const string Carts = "Cart";
					public const string Locations = "Location";
				}

				public class carts : PX.Data.BQL.BqlString.Constant<carts> { public carts() : base(Carts) { } }
				public class locations : PX.Data.BQL.BqlString.Constant<locations> { public locations() : base(Locations) { } }

				public class List : PXStringListAttribute
				{
					public List() : base(
						Pair(Locations, DisplayNames.Locations),
						Pair(Carts, DisplayNames.Carts))
					{ }
				}
			}
			#endregion
			#region ShowLocations
			[Obsolete(Common.Messages.ItemIsObsoleteAndWillBeRemoved2023R1)]
			[PXBool]
			[PXUnboundDefault(true)]
			[PXUIField(DisplayName = "Show Locations", FieldClass = "Carts")]
			public virtual Boolean? ShowLocations { get; set; }
			public abstract class showLocations : PX.Data.BQL.BqlBool.Field<showLocations> { }
			#endregion
			#region ShowCarts
			[Obsolete(Common.Messages.ItemIsObsoleteAndWillBeRemoved2023R1)]
			[PXBool]
			[PXUnboundDefault(typeof(FeatureInstalled<FeaturesSet.wMSCartTracking>))]
			[PXUIField(DisplayName = "Show Carts", FieldClass = "Carts")]
			public virtual Boolean? ShowCarts { get; set; }
			public abstract class showCarts : PX.Data.BQL.BqlBool.Field<showCarts> { }
			#endregion
		}
	}
}
