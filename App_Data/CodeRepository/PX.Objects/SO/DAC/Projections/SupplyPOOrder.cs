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
using PX.Objects.CS;
using PX.Objects.PO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PX.Objects.SO
{
	[PXHidden]
	[PXProjection(typeof(Select<POOrder, Where<POOrder.isLegacyDropShip, Equal<boolFalse>>>), Persistent = true)]
	public class SupplyPOOrder : IBqlTable
	{
		#region Keys
		public class PK : PrimaryKeyOf<SupplyPOOrder>.By<orderType, orderNbr>
		{
			public static SupplyPOOrder Find(PXGraph graph, string orderType, string orderNbr, PKFindOptions options = PKFindOptions.None) => FindBy(graph, orderType, orderNbr, options);
			public static SupplyPOOrder FindDirty(PXGraph graph, string orderType, string orderNbr) => PXSelect<SupplyPOOrder,
						Where<orderType, Equal<Required<orderType>>,
							And<orderNbr, Equal<Required<orderNbr>>>>>
					.SelectWindowed(graph, 0, 1, orderType, orderNbr);
		}
		public static class FK
		{
			public class DemandOrder : SOOrder.PK.ForeignKeyOf<SupplyPOOrder>.By<sOOrderType, sOOrderNbr> { }
		}
		#endregion

		#region OrderType
		public abstract class orderType : PX.Data.BQL.BqlString.Field<orderType> { }

		[PXDefault]
		[POOrderType.List]
		[PXDBString(2, IsKey = true, IsFixed = true, BqlField = typeof(POOrder.orderType))]
		public virtual String OrderType
		{
			get;
			set;
		}
		#endregion
		#region OrderNbr
		public abstract class orderNbr : PX.Data.BQL.BqlString.Field<orderNbr> { }

		[PXDefault]
		[PXDBString(15, IsKey = true, IsUnicode = true, InputMask = ">CCCCCCCCCCCCCCC", BqlField = typeof(POOrder.orderNbr))]
		public virtual String OrderNbr
		{
			get;
			set;
		}
		#endregion
		#region Status
		public abstract class status : PX.Data.BQL.BqlString.Field<status> { }
		
		[PXDefault]
		[PXUIField(DisplayName = "Status", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
		[POOrderStatus.List]
		[PXDBString(1, IsFixed = true, BqlField = typeof(POOrder.status))]
		public virtual String Status
		{
			get;
			set;
		}
		#endregion
		#region Hold
		public abstract class hold : PX.Data.BQL.BqlBool.Field<hold> { }

		[PXDefault]
		[PXDBBool(BqlField = typeof(POOrder.hold))]
		public virtual Boolean? Hold
		{
			get;
			set;
		}
		#endregion
		#region Approved
		public abstract class approved : PX.Data.BQL.BqlBool.Field<approved> { }

		[PXDefault]
		[PXDBBool(BqlField = typeof(POOrder.approved))]
		public virtual Boolean? Approved
		{
			get;
			set;
		}
		#endregion
		#region Cancelled
		public abstract class cancelled : PX.Data.BQL.BqlBool.Field<cancelled> { }

		[PXDefault]
		[PXDBBool(BqlField = typeof(POOrder.cancelled))]
		public virtual Boolean? Cancelled
		{
			get;
			set;
		}
		#endregion

		#region VendorID
		public abstract class vendorID : PX.Data.BQL.BqlInt.Field<vendorID> { }

		[PXDBInt(BqlField = typeof(POOrder.vendorID))]
		public virtual Int32? VendorID
		{
			get;
			set;
		}
		#endregion
		#region VendorLocationID
		public abstract class vendorLocationID : PX.Data.BQL.BqlInt.Field<vendorLocationID> { }

		[PXDBInt(BqlField = typeof(POOrder.vendorLocationID))]
		public virtual Int32? VendorLocationID
		{
			get;
			set;
		}
		#endregion
		#region VendorRefNbr
		public abstract class vendorRefNbr : PX.Data.BQL.BqlString.Field<vendorRefNbr> { }

		[PXDBString(40, IsUnicode = true, BqlField = typeof(POOrder.vendorRefNbr))]
		public virtual String VendorRefNbr
		{
			get;
			set;
		}
		#endregion

		#region SOOrderType
		public abstract class sOOrderType : PX.Data.BQL.BqlString.Field<sOOrderType> { }

		[PXParent(typeof(FK.DemandOrder), LeaveChildren = true)]
		[PXDBString(2, IsFixed = true, InputMask = ">aa", BqlField = typeof(POOrder.sOOrderType))]
		public virtual String SOOrderType
		{
			get;
			set;
		}
		#endregion
		#region SOOrderNbr
		public abstract class sOOrderNbr : PX.Data.BQL.BqlString.Field<sOOrderNbr> { }
		
		[PXDBDefault(typeof(SOOrder.orderNbr), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXDBString(15, IsUnicode = true, InputMask = ">CCCCCCCCCCCCCCC", BqlField = typeof(POOrder.sOOrderNbr))]
		public virtual String SOOrderNbr
		{
			get;
			set;
		}
		#endregion

		#region ShipDestType
		public abstract class shipDestType : PX.Data.BQL.BqlString.Field<shipDestType> { }

		[POShippingDestination.List]
		[PXDBString(1, IsFixed = true, BqlField = typeof(POOrder.shipDestType))]
		public virtual String ShipDestType
		{
			get;
			set;
		}
		#endregion
		#region ShipToBAccountID
		public abstract class shipToBAccountID : PX.Data.BQL.BqlInt.Field<shipToBAccountID> { }

		[PXDBInt(BqlField = typeof(POOrder.shipToBAccountID))]
		public virtual Int32? ShipToBAccountID
		{
			get;
			set;
		}
		#endregion

		#region DropShipLinesCount
		[Obsolete(Common.Messages.ItemIsObsoleteAndWillBeRemoved2023R1)]
		public abstract class dropShipLinesCount : PX.Data.BQL.BqlInt.Field<dropShipLinesCount> { }

		[PXDefault]
		[Obsolete(Common.Messages.ItemIsObsoleteAndWillBeRemoved2023R1)]
		[PXDBInt(BqlField = typeof(POOrder.dropShipLinesCount))]
		public virtual int? DropShipLinesCount
		{
			get;
			set;
		}
		#endregion
		#region DropShipLinkedLinesCount
		public abstract class dropShipLinkedLinesCount : PX.Data.BQL.BqlInt.Field<dropShipLinkedLinesCount> { }

		[PXDefault]
		[PXDBInt(BqlField = typeof(POOrder.dropShipLinkedLinesCount))]
		public virtual int? DropShipLinkedLinesCount
		{
			get;
			set;
		}
		#endregion
		#region DropShipActiveLinksCount
		[Obsolete(Common.Messages.ItemIsObsoleteAndWillBeRemoved2023R1)]
		public abstract class dropShipActiveLinksCount : PX.Data.BQL.BqlInt.Field<dropShipActiveLinksCount> { }

		[PXDefault]
		[Obsolete(Common.Messages.ItemIsObsoleteAndWillBeRemoved2023R1)]
		[PXDBInt(BqlField = typeof(POOrder.dropShipActiveLinksCount))]
		public virtual int? DropShipActiveLinksCount
		{
			get;
			set;
		}
		#endregion
		#region DropShipNotLinkedLinesCntr
		public abstract class dropShipNotLinkedLinesCntr : PX.Data.BQL.BqlInt.Field<dropShipNotLinkedLinesCntr> { }

		[PXDefault]
		[PXDBInt(BqlField = typeof(POOrder.dropShipNotLinkedLinesCntr))]
		public virtual int? DropShipNotLinkedLinesCntr
		{
			get;
			set;
		}
		#endregion

		#region IsLegacyDropShip
		public abstract class isLegacyDropShip : PX.Data.BQL.BqlBool.Field<isLegacyDropShip> { }

		[PXDefault]
		[PXDBBool(BqlField = typeof(POOrder.isLegacyDropShip))]
		public virtual bool? IsLegacyDropShip
		{
			get;
			set;
		}
		#endregion
		#region SpecialLineCntr
		public abstract class specialLineCntr : Data.BQL.BqlDecimal.Field<specialLineCntr> { }
		[PXDBInt(BqlField = typeof(POOrder.specialLineCntr))]
		public virtual int? SpecialLineCntr
		{
			get;
			set;
		}
		#endregion

		#region NoteID
		public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }

		[PXNote(BqlField = typeof(POOrder.noteID))]
		public virtual Guid? NoteID
		{
			get;
			set;
		}
		#endregion
		#region tstamp
		public abstract class Tstamp : PX.Data.BQL.BqlByteArray.Field<Tstamp> { }

		[PXDBTimestamp(VerifyTimestamp = VerifyTimestampOptions.BothFromGraphAndRecord, BqlField = typeof(POOrder.Tstamp))]
		public virtual Byte[] tstamp
		{
			get;
			set;
		}
		#endregion
	}
}
