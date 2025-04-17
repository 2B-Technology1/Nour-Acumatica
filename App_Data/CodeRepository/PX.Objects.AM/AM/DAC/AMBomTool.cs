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
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.AM.Attributes;
using PX.Data.BQL.Fluent;
using PX.Objects.CM;

namespace PX.Objects.AM
{
	/// <summary>
	/// BOM Tool
	/// </summary>
	[Serializable]
	[PXCacheName(Messages.BOMTool)]
	public class AMBomTool : IBqlTable, IBomOper, INotable, IBomDetail
	{
		#region Keys

		public class PK : PrimaryKeyOf<AMBomTool>.By<bOMID, revisionID, operationID, lineID>
		{
			public static AMBomTool Find(PXGraph graph, string bOMID, string revisionID, int? operationID, int? lineID, PKFindOptions options = PKFindOptions.None)
				=> FindBy(graph, bOMID, revisionID, operationID, lineID, options);
			public static AMBomTool FindDirty(PXGraph graph, string bOMID, string revisionID, int? operationID, int? lineID)
				=> PXSelect<AMBomTool,
					Where<bOMID, Equal<Required<bOMID>>,
						And<revisionID, Equal<Required<revisionID>>,
						And<operationID, Equal<Required<operationID>>,
						And<lineID, Equal<Required<lineID>>>>>>>
					.SelectWindowed(graph, 0, 1, bOMID, revisionID, operationID, lineID);
		}

		public static class FK
		{
			public class BOM : AMBomItem.PK.ForeignKeyOf<AMBomTool>.By<bOMID, revisionID> { }
			public class Operation : AMBomOper.PK.ForeignKeyOf<AMBomTool>.By<bOMID, revisionID, operationID> { }
			public class Tool : AMToolMst.PK.ForeignKeyOf<AMBomTool>.By<toolID> { }
		}

		#endregion

		#region Selected

		public abstract class selected : PX.Data.BQL.BqlBool.Field<selected> { }

		[PXBool]
		[PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Selected")]
		public virtual bool? Selected { get; set; }

		#endregion
		#region BOMID
		public abstract class bOMID : PX.Data.BQL.BqlString.Field<bOMID> { }

		protected string _BOMID;
		[BomID(IsKey = true, Visible = false, Enabled = false)]
		[PXDBDefault(typeof(AMBomOper.bOMID))]
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
		[RevisionIDField(IsKey = true, Visibility = PXUIVisibility.SelectorVisible, Visible = false, Enabled = false)]
		[PXDBDefault(typeof(AMBomOper.revisionID))]
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
		[OperationIDField(IsKey = true, Visible = false, Enabled = false)]
		[PXDefault(typeof(AMBomOper.operationID))]
		[PXParent(typeof(Select<AMBomOper,
			Where<AMBomOper.bOMID, Equal<Current<AMBomTool.bOMID>>,
				And<AMBomOper.revisionID, Equal<Current<AMBomTool.revisionID>>,
					And<AMBomOper.operationID, Equal<Current<AMBomTool.operationID>>>>>>))]

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
		#region ToolID
		public abstract class toolID : PX.Data.BQL.BqlString.Field<toolID> { }

		protected String _ToolID;
		[PXDBString(30, IsUnicode = true)]
		[PXUIField(DisplayName = "Tool ID")]
		[PXDefault(PersistingCheck = PXPersistingCheck.NullOrBlank)]
		[PXSelector(typeof(Search<AMToolMst.toolID, Where<AMToolMst.active, Equal<True>>>))]
		public virtual String ToolID
		{
			get
			{
				return this._ToolID;
			}
			set
			{
				this._ToolID = value;
			}
		}
		#endregion
		#region Descr
		public abstract class descr : PX.Data.BQL.BqlString.Field<descr> { }

		protected String _Descr;
		[PXDBString(256, IsUnicode = true)]
		[PXDefault(typeof(Search<AMToolMst.descr, Where<AMToolMst.toolID, Equal<Current<AMBomTool.toolID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Description")]
		[PXFormula(typeof(Default<AMBomTool.toolID>))]
		public virtual String Descr
		{
			get
			{
				return this._Descr;
			}
			set
			{
				this._Descr = value;
			}
		}
		#endregion
		#region LineID
		public abstract class lineID : PX.Data.BQL.BqlInt.Field<lineID> { }

		protected Int32? _LineID;
		[PXDBInt(IsKey = true)]
		[PXUIField(DisplayName = "Line ID", Visible = false, Enabled = false)]
		[PXLineNbr(typeof(AMBomOper.lineCntrTool), DecrementOnDelete = false, ReuseGaps = false)]
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
		#region NoteID
		public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }
		protected Guid? _NoteID;
		[PXNote]
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
		#region QtyReq
		public abstract class qtyReq : PX.Data.BQL.BqlDecimal.Field<qtyReq> { }

		protected decimal? _QtyReq;
		[PXDBQuantity]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Qty Required")]
		public virtual decimal? QtyReq
		{
			get
			{
				return this._QtyReq;
			}
			set
			{
				this._QtyReq = value;
			}
		}
		#endregion
		#region UnitCost
		public abstract class unitCost : PX.Data.BQL.BqlDecimal.Field<unitCost> { }

		protected decimal? _UnitCost;
		[PXDBPriceCost]
		[PXDefault(TypeCode.Decimal, "0.0", typeof(Search<AMToolMstCurySettings.unitCost, Where<AMToolMstCurySettings.toolID, Equal<Current<AMBomTool.toolID>>,
			And<AMToolMstCurySettings.curyID, Equal<Current<AccessInfo.baseCuryID>>>>>))]
		[PXUIField(DisplayName = "Unit Cost")]
		[PXFormula(typeof(Default<AMBomTool.toolID>))]
		public virtual decimal? UnitCost
		{
			get
			{
				return this._UnitCost;
			}
			set
			{
				this._UnitCost = value;
			}
		}
		#endregion
		#region tstamp
		public abstract class Tstamp : PX.Data.BQL.BqlByteArray.Field<Tstamp> { }

		protected Byte[] _tstamp;
		[PXDBTimestamp]
		public virtual Byte[] tstamp
		{
			get
			{
				return this._tstamp;
			}
			set
			{
				this._tstamp = value;
			}
		}
		#endregion
		#region CreatedByID

		public abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID> { }

		protected Guid? _CreatedByID;
		[PXDBCreatedByID]
		public virtual Guid? CreatedByID
		{
			get
			{
				return this._CreatedByID;
			}
			set
			{
				this._CreatedByID = value;
			}
		}
		#endregion
		#region CreatedByScreenID

		public abstract class createdByScreenID : PX.Data.BQL.BqlString.Field<createdByScreenID> { }

		protected String _CreatedByScreenID;
		[PXDBCreatedByScreenID]
		public virtual String CreatedByScreenID
		{
			get
			{
				return this._CreatedByScreenID;
			}
			set
			{
				this._CreatedByScreenID = value;
			}
		}
		#endregion
		#region CreatedDateTime

		public abstract class createdDateTime : PX.Data.BQL.BqlDateTime.Field<createdDateTime> { }

		protected DateTime? _CreatedDateTime;
		[PXDBCreatedDateTime]
		public virtual DateTime? CreatedDateTime
		{
			get
			{
				return this._CreatedDateTime;
			}
			set
			{
				this._CreatedDateTime = value;
			}
		}
		#endregion
		#region LastModifiedByID

		public abstract class lastModifiedByID : PX.Data.BQL.BqlGuid.Field<lastModifiedByID> { }

		protected Guid? _LastModifiedByID;
		[PXDBLastModifiedByID]
		public virtual Guid? LastModifiedByID
		{
			get
			{
				return this._LastModifiedByID;
			}
			set
			{
				this._LastModifiedByID = value;
			}
		}
		#endregion
		#region LastModifiedByScreenID

		public abstract class lastModifiedByScreenID : PX.Data.BQL.BqlString.Field<lastModifiedByScreenID> { }

		protected String _LastModifiedByScreenID;
		[PXDBLastModifiedByScreenID]
		public virtual String LastModifiedByScreenID
		{
			get
			{
				return this._LastModifiedByScreenID;
			}
			set
			{
				this._LastModifiedByScreenID = value;
			}
		}
		#endregion
		#region LastModifiedDateTime

		public abstract class lastModifiedDateTime : PX.Data.BQL.BqlDateTime.Field<lastModifiedDateTime> { }

		protected DateTime? _LastModifiedDateTime;
		[PXDBLastModifiedDateTime]
		public virtual DateTime? LastModifiedDateTime
		{
			get
			{
				return this._LastModifiedDateTime;
			}
			set
			{
				this._LastModifiedDateTime = value;
			}
		}
		#endregion
		#region RowStatus
		public abstract class rowStatus : PX.Data.BQL.BqlInt.Field<rowStatus> { }
		protected int? _RowStatus;
		[PXDBInt]
		[PXUIField(DisplayName = "Change Status", Enabled = false)]
		[AMRowStatus.List]
		public virtual int? RowStatus
		{
			get
			{
				return this._RowStatus;
			}
			set
			{
				this._RowStatus = value;
			}
		}
		#endregion
	}

	/// <summary>
	/// Projection of the <see cref="AMBOMCurySettings"/> class for the currency cost data of only the <see cref="AMBomTool"/> (Tool) class.
	/// </summary>
	[Serializable]
	[PXCacheName("AMBomToolCurrency")]
	[PXProjection(typeof(SelectFrom<AMBOMCurySettings>
	.Where<AMBOMCurySettings.lineType.IsEqual<BOMCurySettingsLineType.tool>>), Persistent = true)]
	public class AMBomToolCury : IBqlTable
	{
		#region Keys
		public class PK : PrimaryKeyOf<AMBomToolCury>.By<bOMID, revisionID, operationID, lineID, curyID>
		{
			public static AMBomToolCury Find(PXGraph graph, string bOMID, string revisionID, int? operationID, int? lineID, string curyID, PKFindOptions options = PKFindOptions.None) =>
				FindBy(graph, bOMID, revisionID, operationID, lineID, curyID, options);
		}
		public static class FK
		{
			public class BomTool : AMBomTool.PK.ForeignKeyOf<AMBomToolCury>.By<bOMID, revisionID, operationID, lineID> { }
		}
		#endregion

		#region BOMID
		public abstract class bOMID : PX.Data.BQL.BqlString.Field<bOMID> { }
		protected String _BOMID;
		[BomID(IsKey = true, Enabled = false, BqlField = typeof(AMBOMCurySettings.bOMID))]
		[PXDBDefault(typeof(AMBomOper.bOMID))]
		[PXParent(typeof(FK.BomTool))]
		public virtual String BOMID
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
		protected String _RevisionID;
		[PXDBString(10, IsKey = true, IsUnicode = true, InputMask = ">CCCCCCCCCC", BqlField = typeof(AMBOMCurySettings.revisionID))]
		[PXUIField(DisplayName = "Revision", Required = true, Visibility = PXUIVisibility.SelectorVisible)]
		[PXDefault(typeof(AMBomTool.revisionID))]
		public virtual String RevisionID
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
		[OperationIDField(IsKey = true, Visible = false, Enabled = false, BqlField = typeof(AMBOMCurySettings.operationID))]
		[PXDBDefault(typeof(AMBomTool.operationID))]
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
		[PXDBInt(IsKey = true, BqlField = typeof(AMBOMCurySettings.lineID))]
		[PXUIField(DisplayName = "LineID", Visible = false, Enabled = false)]
		[PXDBDefault(typeof(AMBomTool.lineID))]
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
		#region CuryID
		public abstract class curyID : PX.Data.BQL.BqlString.Field<curyID> { }

		protected String _CuryID;
		[PXDBString(IsUnicode = true, IsKey = true, BqlField = typeof(AMBOMCurySettings.curyID))]
		[PXUIField(DisplayName = "Currency", Enabled = false)]
		[PXSelector(typeof(Search<CurrencyList.curyID>))]
		public virtual String CuryID
		{
			get
			{
				return this._CuryID;
			}
			set
			{
				this._CuryID = value;
			}
		}
		#endregion
		#region LineType
		public abstract class lineType : PX.Data.BQL.BqlString.Field<lineType> { }
		protected String _LineType;
		[PXDBString(1, IsFixed = true, IsKey = true, BqlField = typeof(AMBOMCurySettings.lineType))]
		[PXDefault(BOMCurySettingsLineType.Tool)]
		[BOMCurySettingsLineType.List()]
		[PXUIField(DisplayName = "Line Type", Enabled = false)]

		public virtual String LineType
		{
			get
			{
				return this._LineType;
			}
			set
			{
				this._LineType = value;
			}
		}
		#endregion
		#region SiteID
		public abstract class siteID : PX.Data.BQL.BqlInt.Field<siteID> { }
		protected Int32? _SiteID;
		[PXUIField(DisplayName = "Site ID", Visible = false, Enabled = false)]
		[Site(BqlField = typeof(AMBOMCurySettings.siteID))]
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
		[PXDBInt(BqlField = typeof(AMBOMCurySettings.locationID))]
		[PXUIField(DisplayName = "Location ID", Visible = false, Enabled = false)]
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
		#region VendorID

		public abstract class vendorID : PX.Data.BQL.BqlInt.Field<vendorID> { }
		protected Int32? _VendorID;
		[PXDBInt(BqlField = typeof(AMBOMCurySettings.vendorID))]
		[PXUIField(DisplayName = "Vendor ID", Visible = false, Enabled = false)]
		public virtual Int32? VendorID
		{
			get
			{
				return this._VendorID;
			}
			set
			{
				this._VendorID = value;
			}
		}
		#endregion
		#region VendorLocationID

		public abstract class vendorLocationID : PX.Data.BQL.BqlInt.Field<vendorLocationID> { }
		protected Int32? _VendorLocationID;
		[PXDBInt(BqlField = typeof(AMBOMCurySettings.vendorLocationID))]
		[PXUIField(DisplayName = "Vendor Location ID", Visible = false, Enabled = false)]
		public virtual Int32? VendorLocationID
		{
			get
			{
				return this._VendorLocationID;
			}
			set
			{
				this._VendorLocationID = value;
			}
		}
		#endregion
		#region UnitCost
		public abstract class unitCost : PX.Data.BQL.BqlDecimal.Field<unitCost> { }

		protected Decimal? _UnitCost;
		[PXDBDecimal(BqlField = typeof(AMBOMCurySettings.unitCost))]
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Unit Cost")]
		public virtual Decimal? UnitCost
		{
			get
			{
				return this._UnitCost;
			}
			set
			{
				this._UnitCost = value;
			}
		}
		#endregion
		#region tstamp
		public abstract class Tstamp : PX.Data.BQL.BqlByteArray.Field<Tstamp> { }

		protected Byte[] _tstamp;
		[PXDBTimestamp(BqlField = typeof(AMBOMCurySettings.Tstamp))]
		public virtual Byte[] tstamp
		{
			get
			{
				return this._tstamp;
			}
			set
			{
				this._tstamp = value;
			}
		}
		#endregion
		#region CreatedByID

		public abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID> { }

		protected Guid? _CreatedByID;
		[PXDBCreatedByID(BqlField = typeof(AMBOMCurySettings.createdByID))]
		public virtual Guid? CreatedByID
		{
			get
			{
				return this._CreatedByID;
			}
			set
			{
				this._CreatedByID = value;
			}
		}
		#endregion
		#region CreatedByScreenID

		public abstract class createdByScreenID : PX.Data.BQL.BqlString.Field<createdByScreenID> { }

		protected String _CreatedByScreenID;
		[PXDBCreatedByScreenID(BqlField = typeof(AMBOMCurySettings.createdByScreenID))]
		public virtual String CreatedByScreenID
		{
			get
			{
				return this._CreatedByScreenID;
			}
			set
			{
				this._CreatedByScreenID = value;
			}
		}
		#endregion
		#region CreatedDateTime

		public abstract class createdDateTime : PX.Data.BQL.BqlDateTime.Field<createdDateTime> { }

		protected DateTime? _CreatedDateTime;
		[PXDBCreatedDateTime(BqlField = typeof(AMBOMCurySettings.createdDateTime))]
		public virtual DateTime? CreatedDateTime
		{
			get
			{
				return this._CreatedDateTime;
			}
			set
			{
				this._CreatedDateTime = value;
			}
		}
		#endregion
		#region LastModifiedByID

		public abstract class lastModifiedByID : PX.Data.BQL.BqlGuid.Field<lastModifiedByID> { }

		protected Guid? _LastModifiedByID;
		[PXDBLastModifiedByID(BqlField = typeof(AMBOMCurySettings.lastModifiedByID))]
		public virtual Guid? LastModifiedByID
		{
			get
			{
				return this._LastModifiedByID;
			}
			set
			{
				this._LastModifiedByID = value;
			}
		}
		#endregion
		#region LastModifiedByScreenID

		public abstract class lastModifiedByScreenID : PX.Data.BQL.BqlString.Field<lastModifiedByScreenID> { }

		protected String _LastModifiedByScreenID;
		[PXDBLastModifiedByScreenID(BqlField = typeof(AMBOMCurySettings.lastModifiedByScreenID))]
		public virtual String LastModifiedByScreenID
		{
			get
			{
				return this._LastModifiedByScreenID;
			}
			set
			{
				this._LastModifiedByScreenID = value;
			}
		}
		#endregion
		#region LastModifiedDateTime

		public abstract class lastModifiedDateTime : PX.Data.BQL.BqlDateTime.Field<lastModifiedDateTime> { }

		protected DateTime? _LastModifiedDateTime;
		[PXDBLastModifiedDateTime(BqlField = typeof(AMBOMCurySettings.lastModifiedDateTime))]
		public virtual DateTime? LastModifiedDateTime
		{
			get
			{
				return this._LastModifiedDateTime;
			}
			set
			{
				this._LastModifiedDateTime = value;
			}
		}
		#endregion
	}

}
