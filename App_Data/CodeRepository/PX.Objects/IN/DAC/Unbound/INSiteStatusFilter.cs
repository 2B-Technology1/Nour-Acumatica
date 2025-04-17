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
using PX.Objects.GL;
using PX.Objects.Common;
using PX.Objects.Common.Bql;

namespace PX.Objects.IN
{
	[System.SerializableAttribute()]
	public partial class INSiteStatusFilter : IBqlTable
	{
		#region SiteID
		public abstract class siteID : PX.Data.BQL.BqlInt.Field<siteID> { }
		protected Int32? _SiteID;	
		[PXUIField(DisplayName = "Warehouse")]
		[SiteAttribute]
		[InterBranchRestrictor(typeof(Where<SameOrganizationBranch<INSite.branchID, Current<INRegister.branchID>>>))]
		[PXDefault(typeof(INRegister.siteID), PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual Int32? SiteID
		{
			get
			{
				return this._SiteID;
			}
			set
			{
				this._SiteID = value;
			}
		}
		#endregion

		#region LocationID
		public abstract class locationID : PX.Data.BQL.BqlInt.Field<locationID> { }
		protected Int32? _LocationID;
		[Location(typeof(INSiteStatusFilter.siteID), KeepEntry = false, DescriptionField = typeof(INLocation.descr), DisplayName = "Location")]
		public virtual Int32? LocationID
		{
			get
			{
				return this._LocationID;
			}
			set
			{
				this._LocationID = value;
			}
		}
		#endregion

		#region InventoryID
		public abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }
		protected Int32? _InventoryID;
		[Inventory()]
		public virtual Int32? InventoryID
		{
			get
			{
				return this._InventoryID;
			}
			set
			{
				this._InventoryID = value;
			}
		}
		#endregion

		#region SubItem
		public abstract class subItem : PX.Data.BQL.BqlString.Field<subItem> { }
		protected String _SubItem;
		[SubItemRawExt(typeof(INSiteStatusFilter.inventoryID), DisplayName = "Subitem")]
		public virtual String SubItem
		{
			get
			{
				return this._SubItem;
			}
			set
			{
				this._SubItem = value;
			}
		}
		#endregion

		#region SubItemCD Wildcard
		public abstract class subItemCDWildcard : PX.Data.BQL.BqlString.Field<subItemCDWildcard> { };
		[PXDBString(30, IsUnicode = true)]
		public virtual String SubItemCDWildcard
		{
			get
			{
				//return SubItemCDUtils.CreateSubItemCDWildcard(this._SubItemCD);
				return this._SubItem == null ? null : SubCDUtils.CreateSubCDWildcard(this._SubItem, SubItemAttribute.DimensionName);
			}
		}
		#endregion		

		#region BarCode
		public abstract class barCode : PX.Data.BQL.BqlString.Field<barCode> { }
		protected String _BarCode;
		[PXDBString(IsUnicode = true)]
		[PXUIField(DisplayName="Barcode")]
		public virtual String BarCode
		{
			get
			{
				return this._BarCode;
			}
			set
			{
				this._BarCode = value;
			}
		}
		#endregion

		#region BarCode Wildcard
		public abstract class barCodeWildcard : PX.Data.BQL.BqlString.Field<barCodeWildcard> { };
		[PXDBString(30, IsUnicode = true)]
		public virtual String BarCodeWildcard
		{
			get
			{				
				return this._BarCode == null ? null : "%" + this._BarCode + "%";
			}
		}
		#endregion		

		#region Inventory
		public abstract class inventory : PX.Data.BQL.BqlString.Field<inventory> { }
		protected String _Inventory;
		[PXDBString(IsUnicode=true)]
		[PXUIField(DisplayName = "Inventory")]
		public virtual String Inventory
		{
			get
			{
				return this._Inventory;
			}
			set
			{
				this._Inventory = value;
			}
		}
		#endregion		

		#region Inventory Wildcard
		public abstract class inventory_Wildcard : PX.Data.BQL.BqlString.Field<inventory_Wildcard> { };
		[PXDBString(30, IsUnicode = true)]
		public virtual String Inventory_Wildcard
		{
			get
			{
				String wildcard = PXDatabase.Provider.SqlDialect.WildcardAnything;
				return this._Inventory == null ? null : wildcard + this._Inventory + wildcard;
			}
		}
		#endregion		

		#region OnlyAvailable
		public abstract class onlyAvailable : PX.Data.BQL.BqlBool.Field<onlyAvailable> { }
		protected bool? _OnlyAvailable;
		[PXBool]
		[PXDefault(true)]
		[PXUIField(DisplayName = "Show Available Items Only")]
		public virtual bool? OnlyAvailable
		{
			get
			{
				return _OnlyAvailable;
			}
			set
			{
				_OnlyAvailable = value;
			}
		}
		#endregion

		#region ItemClass
		public abstract class itemClass : PX.Data.BQL.BqlString.Field<itemClass> { }
		protected string _ItemClass;

		[PXDBString(30, IsUnicode = true)]
		[PXUIField(DisplayName = "Item Class ID", Visibility = PXUIVisibility.SelectorVisible)]
		[PXDimensionSelector(INItemClass.Dimension, typeof(INItemClass.itemClassCD), DescriptionField = typeof(INItemClass.descr), ValidComboRequired = true)]
		public virtual string ItemClass
		{
			get { return this._ItemClass; }
			set { this._ItemClass = value; }
		}
		#endregion
		#region ItemClassCDWildcard
		public abstract class itemClassCDWildcard : PX.Data.BQL.BqlString.Field<itemClassCDWildcard> { }
		[PXString(IsUnicode = true)]
		[PXUIField(Visible = false, Visibility = PXUIVisibility.Invisible)]
		[PXDimension(INItemClass.Dimension, ParentSelect = typeof(Select<INItemClass>), ParentValueField = typeof(INItemClass.itemClassCD), AutoNumbering = false)]
		public virtual string ItemClassCDWildcard
		{
			get { return ItemClassTree.MakeWildcard(ItemClass); }
			set { }
		}
		#endregion
	}
}
