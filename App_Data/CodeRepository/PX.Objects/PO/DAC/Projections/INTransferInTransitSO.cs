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

namespace PX.Objects.PO
{
	[PXProjection(typeof(Select4<INTransitLineStatusSO,
	  Where<INTransitLineStatusSO.qtyOnHand, Greater<Zero>>,
	  Aggregate<GroupBy<INTransitLineStatusSO.sOShipmentNbr,
		GroupBy<INTransitLineStatusSO.sOOrderType,
		  GroupBy<INTransitLineStatusSO.sOOrderNbr,
			GroupBy<INTransitLineStatusSO.fromSiteID,
			  GroupBy<INTransitLineStatusSO.toSiteID,
				GroupBy<INTransitLineStatusSO.origModule>>>>>>>>))]
	[Serializable]
	[PXHidden]
	public partial class INTransferInTransitSO : IBqlTable
	{
		#region SOShipmentNbr
		public abstract class sOShipmentNbr : PX.Data.IBqlField
		{
		}
		protected String _SOShipmentNbr;
		[PXDBString(15, IsUnicode = true, IsKey = true, BqlField = typeof(INTransitLineStatusSO.sOShipmentNbr))]
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

		#region SOOrderType
		public abstract class sOOrderType : PX.Data.IBqlField
		{
		}
		protected String _SOOrderType;
		[PXDBString(2, IsKey = true, BqlField = typeof(INTransitLineStatusSO.sOOrderType))]
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
		[PXDBString(15, IsUnicode = true, IsKey = true, BqlField = typeof(INTransitLineStatusSO.sOOrderNbr))]
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

		#region RefNoteID
		public abstract class refNoteID : PX.Data.IBqlField
		{
		}
		protected Guid? _RefNoteID;
		[PXNote(BqlField = typeof(INTransitLineStatusSO.refNoteID))]
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

		#region FromSiteID
		public abstract class fromSiteID : PX.Data.BQL.BqlInt.Field<fromSiteID> { }
		[PXDBInt(BqlField = typeof(INTransitLineStatusSO.fromSiteID))]
		public virtual int? FromSiteID
		{
			get;
			set;
		}
		#endregion

		#region ToSiteID
		public abstract class toSiteID : PX.Data.BQL.BqlInt.Field<toSiteID> { }
		[PXDBInt(BqlField = typeof(INTransitLineStatusSO.toSiteID))]
		public virtual int? ToSiteID
		{
			get;
			set;
		}
		#endregion

		#region OrigModule
		public abstract class origModule : PX.Data.BQL.BqlString.Field<origModule> { }
		[PXDBString(2, IsFixed = true, BqlField = typeof(INTransitLineStatusSO.origModule))]
		public virtual string OrigModule
		{
			get;
			set;
		}
		#endregion
	}
}
