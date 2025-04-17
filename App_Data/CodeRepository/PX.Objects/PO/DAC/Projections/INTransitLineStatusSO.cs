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

namespace PX.Objects.PO
{
	[PXProjection(typeof(Select5<INLocationStatusInTransit,
		InnerJoin<INTransitLine, On<INTransitLine.costSiteID, Equal<INLocationStatusInTransit.locationID>>>,
			Where<INLocationStatusInTransit.siteID, Equal<SiteAttribute.transitSiteID>>,
		Aggregate<GroupBy<INTransitLine.sOShipmentNbr,
			GroupBy<INTransitLine.sOShipmentLineNbr,
			GroupBy<INTransitLine.sOOrderType,
			GroupBy<INTransitLine.sOOrderNbr,
			GroupBy<INTransitLine.siteID,
			GroupBy<INTransitLine.toSiteID,
			GroupBy<INTransitLine.origModule,
			Sum<INLocationStatusInTransit.qtyOnHand,
			Sum<INLocationStatusInTransit.qtyInTransit,
			Sum<INLocationStatusInTransit.qtyInTransitToSO>>>>>>>>>>>>), Persistent = false)]
	public partial class INTransitLineStatusSO : PX.Data.IBqlTable
	{
		#region InventoryID
		public abstract class inventoryID : PX.Data.IBqlField
		{
		}
		protected Int32? _InventoryID;
		[StockItem(BqlField = typeof(INLocationStatusInTransit.inventoryID))]
		[PXDefault()]
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
		#region SiteID
		public abstract class siteID : PX.Data.IBqlField
		{
		}
		protected Int32? _SiteID;
		[Site(BqlField = typeof(INLocationStatusInTransit.siteID))]
		[PXDefault()]
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
		#region CostSiteID
		public abstract class costSiteID : PX.Data.IBqlField
		{
		}
		protected Int32? _CostSiteID;
		[PXDBInt(BqlField = typeof(INTransitLine.costSiteID))]
		public virtual Int32? CostSiteID
		{
			get
			{
				return this._CostSiteID;
			}
			set
			{
				this._CostSiteID = value;
			}
		}
		#endregion

		#region FromSiteID
		public abstract class fromSiteID : PX.Data.IBqlField
		{
		}
		protected Int32? _FromSiteID;
		[IN.Site(DisplayName = "From Warehouse ID", DescriptionField = typeof(INSite.descr), BqlField = typeof(INTransitLine.siteID))]
		public virtual Int32? FromSiteID
		{
			get
			{
				return this._FromSiteID;
			}
			set
			{
				this._FromSiteID = value;
			}
		}
		#endregion
		#region ToSiteID
		public abstract class toSiteID : PX.Data.IBqlField
		{
		}
		protected Int32? _ToSiteID;
		[IN.ToSite(DisplayName = "To Warehouse ID", DescriptionField = typeof(INSite.descr), BqlField = typeof(INTransitLine.toSiteID))]
		public virtual Int32? ToSiteID
		{
			get
			{
				return this._ToSiteID;
			}
			set
			{
				this._ToSiteID = value;
			}
		}
		#endregion

		#region OrigModule
		public abstract class origModule : PX.Data.IBqlField
		{
			public const string PI = "PI";

			public class List : PXStringListAttribute
			{
				public List() : base(
				  new[]
				{
			Pair(GL.BatchModule.SO, GL.Messages.ModuleSO),
			Pair(GL.BatchModule.PO, GL.Messages.ModulePO),
			Pair(GL.BatchModule.IN, GL.Messages.ModuleIN),
			Pair(PI, IN.Messages.ModulePI),
			Pair(GL.BatchModule.AP, GL.Messages.ModuleAP),
				  })
				{ }
			}
		}
		protected String _OrigModule;
		[PXDBString(2, IsFixed = true, BqlField = typeof(INTransitLine.origModule))]
		[PXDefault(GL.BatchModule.IN)]
		[PXUIField(DisplayName = "Source", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
		[origModule.List]
		public virtual String OrigModule
		{
			get
			{
				return this._OrigModule;
			}
			set
			{
				this._OrigModule = value;
			}
		}
		#endregion

		#region ToLocationID
		public abstract class toLocationID : PX.Data.IBqlField
		{
		}
		protected Int32? _ToLocationID;
		[IN.Location(typeof(toSiteID), DisplayName = "To Location ID", BqlField = typeof(INTransitLine.toLocationID))]
		public virtual Int32? ToLocationID
		{
			get
			{
				return this._ToLocationID;
			}
			set
			{
				this._ToLocationID = value;
			}
		}
		#endregion

		#region NoteID
		public abstract class noteID : PX.Data.IBqlField
		{
		}
		protected Guid? _NoteID;
		[PXNote(BqlField = typeof(INTransitLine.noteID))]
		public virtual Guid? NoteID
		{
			get
			{
				return this._NoteID;
			}
			set
			{
				this._NoteID = value;
			}
		}
		#endregion

		#region RefNoteID
		public abstract class refNoteID : PX.Data.IBqlField
		{
		}
		protected Guid? _RefNoteID;
		[PXDBGuid(BqlField = typeof(INTransitLine.refNoteID))]
		public virtual Guid? RefNoteID
		{
			get
			{
				return this._RefNoteID;
			}
			set
			{
				this._RefNoteID = value;
			}
		}
		#endregion

		#region TransferNbr
		public abstract class transferNbr : PX.Data.IBqlField
		{
		}
		protected String _TransferNbr;
		[PXDBString(15, IsUnicode = true, BqlField = typeof(INTransitLine.transferNbr))]
		public virtual String TransferNbr
		{
			get
			{
				return this._TransferNbr;
			}
			set
			{
				this._TransferNbr = value;
			}
		}
		#endregion
		#region TransferLineNbr
		public abstract class transferLineNbr : PX.Data.IBqlField
		{
		}
		protected Int32? _TransferLineNbr;

		[PXDBInt(BqlField = typeof(INTransitLine.transferLineNbr))]
		public virtual Int32? TransferLineNbr
		{
			get
			{
				return this._TransferLineNbr;
			}
			set
			{
				this._TransferLineNbr = value;
			}
		}
		#endregion

		#region SOOrderType
		public abstract class sOOrderType : PX.Data.IBqlField
		{
		}
		protected String _SOOrderType;
		[PXDBString(2, IsFixed = true, BqlField = typeof(INTransitLine.sOOrderType))]
		public virtual String SOOrderType
		{
			get
			{
				return this._SOOrderType;
			}
			set
			{
				this._SOOrderType = value;
			}
		}
		#endregion
		#region SOOrderNbr
		public abstract class sOOrderNbr : PX.Data.IBqlField
		{
		}
		protected String _SOOrderNbr;
		[PXDBString(15, InputMask = "", IsUnicode = true, BqlField = typeof(INTransitLine.sOOrderNbr))]
		public virtual String SOOrderNbr
		{
			get
			{
				return this._SOOrderNbr;
			}
			set
			{
				this._SOOrderNbr = value;
			}
		}
		#endregion

		#region SOOrderLineNbr
		public abstract class sOOrderLineNbr : PX.Data.IBqlField
		{
		}
		protected Int32? _SOOrderLineNbr;

		[PXDBInt(BqlField = typeof(INTransitLine.sOOrderLineNbr))]
		public virtual Int32? SOOrderLineNbr
		{
			get
			{
				return this._SOOrderLineNbr;
			}
			set
			{
				this._SOOrderLineNbr = value;
			}
		}
		#endregion

		#region SOShipmentType
		public abstract class sOShipmentType : PX.Data.IBqlField
		{
		}
		protected String _SOShipmentType;
		[PXDBString(1, IsFixed = true, BqlField = typeof(INTransitLine.sOShipmentType))]
		public virtual String SOShipmentType
		{
			get
			{
				return this._SOShipmentType;
			}
			set
			{
				this._SOShipmentType = value;
			}
		}
		#endregion
		#region SOShipmentNbr
		public abstract class sOShipmentNbr : PX.Data.IBqlField
		{
		}
		protected String _SOShipmentNbr;
		[PXDBString(15, InputMask = "", IsUnicode = true, BqlField = typeof(INTransitLine.sOShipmentNbr), IsKey = true)]
		public virtual String SOShipmentNbr
		{
			get
			{
				return this._SOShipmentNbr;
			}
			set
			{
				this._SOShipmentNbr = value;
			}
		}
		#endregion

		#region SOShipmentLineNbr
		public abstract class sOShipmentLineNbr : PX.Data.IBqlField
		{
		}
		protected Int32? _SOShipmentLineNbr;

		[PXDBInt(BqlField = typeof(INTransitLine.sOShipmentLineNbr), IsKey = true)]
		public virtual Int32? SOShipmentLineNbr
		{
			get
			{
				return this._SOShipmentLineNbr;
			}
			set
			{
				this._SOShipmentLineNbr = value;
			}
		}
		#endregion

		#region QtyOnHand
		public abstract class qtyOnHand : PX.Data.IBqlField
		{
		}
		protected Decimal? _QtyOnHand;
		[PXDBQuantity(BqlField = typeof(INLocationStatusInTransit.qtyOnHand))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Qty. On Hand")]
		public virtual Decimal? QtyOnHand
		{
			get
			{
				return this._QtyOnHand;
			}
			set
			{
				this._QtyOnHand = value;
			}
		}
		#endregion

		#region QtyInTransit
		public abstract class qtyInTransit : PX.Data.IBqlField
		{
		}
		protected Decimal? _QtyInTransit;
		[PXDBQuantity(BqlField = typeof(INLocationStatusInTransit.qtyInTransit))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? QtyInTransit
		{
			get
			{
				return this._QtyInTransit;
			}
			set
			{
				this._QtyInTransit = value;
			}
		}
		#endregion
		#region QtyInTransitToSO
		public abstract class qtyInTransitToSO : PX.Data.IBqlField
		{
		}
		protected Decimal? _QtyInTransitToSO;
		[PXDBQuantity(BqlField = typeof(INLocationStatusInTransit.qtyInTransitToSO))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? QtyInTransitToSO
		{
			get
			{
				return this._QtyInTransitToSO;
			}
			set
			{
				this._QtyInTransitToSO = value;
			}
		}
		#endregion

	}
}
