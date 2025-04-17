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
using PX.Objects.AM.Attributes;
using PX.Objects.IN;

namespace PX.Objects.AM
{
	/// <summary>
	/// Projection of <see cref="AMBomMatl"/> inner joined with <see cref="AMBomOper"/> including required fields for drill down process and sort order
	/// </summary>
	[PXProjection(typeof(Select2<AMBomMatl,
	InnerJoin<AMBomOper, On<AMBomOper.bOMID, Equal<AMBomMatl.bOMID>,
		And<AMBomOper.revisionID, Equal<AMBomMatl.revisionID>,
		And<AMBomOper.operationID, Equal<AMBomMatl.operationID>>>>>>), Persistent = false)]
	[Serializable]
	[PXHidden]
	public class AMBomMatlDrillDown : IBqlTable
	{
		#region BOMID
		public abstract class bOMID : PX.Data.BQL.BqlString.Field<bOMID> { }

		protected string _BOMID;
		[BomID(IsKey = true, BqlField = typeof(AMBomMatl.bOMID))]
		public virtual string BOMID
		{
			get
			{
				return this._BOMID;
			}
			set
			{
				this._BOMID = value;
			}
		}
		#endregion
		#region RevisionID
		public abstract class revisionID : PX.Data.BQL.BqlString.Field<revisionID> { }

		protected string _RevisionID;
		[RevisionIDField(IsKey = true, BqlField = typeof(AMBomMatl.revisionID))]
		public virtual string RevisionID
		{
			get
			{
				return this._RevisionID;
			}
			set
			{
				this._RevisionID = value;
			}
		}
		#endregion
		#region OperationID
		public abstract class operationID : PX.Data.BQL.BqlInt.Field<operationID> { }

		protected int? _OperationID;
		[OperationIDField(IsKey = true, BqlField = typeof(AMBomMatl.operationID))]
		public virtual int? OperationID
		{
			get
			{
				return this._OperationID;
			}
			set
			{
				this._OperationID = value;
			}
		}
		#endregion
		#region LineID
		public abstract class lineID : PX.Data.BQL.BqlInt.Field<lineID> { }

		protected Int32? _LineID;
		[PXDBInt(IsKey = true, BqlField = typeof(AMBomMatl.lineID))]
		[PXUIField(DisplayName = "Line Nbr.")]
		public virtual Int32? LineID
		{
			get
			{
				return this._LineID;
			}
			set
			{
				this._LineID = value;
			}
		}
		#endregion
		#region InventoryID
		public abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }

		protected Int32? _InventoryID;
		[PXDBInt(BqlField = typeof(AMBomMatl.inventoryID))]
		[PXUIField(DisplayName = "Inventory ID")]
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
		[Obsolete("Use AMBOMCurySettings.siteID")]
		public abstract class siteID : PX.Data.BQL.BqlInt.Field<siteID> { }

		protected Int32? _SiteID;
		[Obsolete("Use AMBOMCurySettings.SiteID")]
		[PXDBInt(BqlField = typeof(AMBomMatl.siteID))]
		[PXUIField(DisplayName = "Warehouse", FieldClass = SiteAttribute.DimensionName)]
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
		#region SubItemID
		public abstract class subItemID : PX.Data.BQL.BqlInt.Field<subItemID> { }

		protected Int32? _SubItemID;
		[PXDBInt(BqlField = typeof(AMBomMatl.subItemID))]
		[PXUIField(DisplayName = "Subitem", FieldClass = "INSUBITEM", Visibility = PXUIVisibility.Visible)]
		public virtual Int32? SubItemID
		{
			get
			{
				return this._SubItemID;
			}
			set
			{
				this._SubItemID = value;
			}
		}
		#endregion
		#region SortOrder
		public abstract class sortOrder : PX.Data.BQL.BqlInt.Field<sortOrder> { }

		protected Int32? _SortOrder;
		[PXUIField(DisplayName = PX.Objects.AP.APTran.sortOrder.DispalyName)]
		[PXDBInt(BqlField = typeof(AMBomMatl.sortOrder))]
		public virtual Int32? SortOrder
		{
			get
			{
				return this._SortOrder;
			}
			set
			{
				this._SortOrder = value;
			}
		}
		#endregion
		#region OperationCD
		public abstract class operationCD : PX.Data.BQL.BqlString.Field<operationCD> { }

		protected string _OperationCD;
		[OperationCDField(BqlField = typeof(AMBomOper.operationCD))]
		public virtual string OperationCD
		{
			get { return this._OperationCD; }
			set { this._OperationCD = value; }
		}

		#endregion
		#region EffDate
		public abstract class effDate : PX.Data.BQL.BqlDateTime.Field<effDate> { }

		protected DateTime? _EffDate;
		[PXDBDate(BqlField = typeof(AMBomMatl.effDate))]
		[PXUIField(DisplayName = "Effective Date")]
		public virtual DateTime? EffDate
		{
			get
			{
				return this._EffDate;
			}
			set
			{
				this._EffDate = value;
			}
		}
		#endregion
		#region ExpDate
		public abstract class expDate : PX.Data.BQL.BqlDateTime.Field<expDate> { }

		protected DateTime? _ExpDate;
		[PXDBDate(BqlField = typeof(AMBomMatl.expDate))]
		[PXUIField(DisplayName = "Expiration Date")]
		public virtual DateTime? ExpDate
		{
			get
			{
				return this._ExpDate;
			}
			set
			{
				this._ExpDate = value;
			}
		}
		#endregion
	}
}
