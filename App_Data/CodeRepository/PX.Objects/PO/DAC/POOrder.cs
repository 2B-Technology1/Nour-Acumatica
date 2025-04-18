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
using System.Collections.Generic;

using PX.Data;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Data.BQL.Fluent;

using PX.SM;
using PX.TM;

using PX.Objects.AP;
using PX.Objects.CS;
using PX.Objects.CR;
using PX.Objects.CM.Extensions;
using PX.Objects.IN;
using PX.Objects.EP;
using PX.Objects.PM;

using CRLocation = PX.Objects.CR.Standalone.Location;
using PX.Objects.Common.Bql;
using PX.Objects.Common;
using PX.Common;
using System.Web.Configuration;
using PX.Data.WorkflowAPI;
using PX.Objects.CN.Subcontracts.SC.Graphs;

namespace PX.Objects.PO
{

	[Serializable]
	[PXPrimaryGraph(
		new Type[] {
			typeof(SubcontractEntry),
			typeof(POOrderEntry)
		},
		new Type[] {
			typeof(Where<POOrder.orderType.IsEqual<POOrderType.regularSubcontract>>),
			typeof(Where<True.IsEqual<True>>)
		})]
	[PXCacheName(Messages.POOrder)]
	[PXGroupMask(typeof(InnerJoinSingleTable<Vendor, On<Vendor.bAccountID, Equal<POOrder.vendorID>, And<Match<Vendor, Current<AccessInfo.userName>>>>>))]
	public partial class POOrder : PX.Data.IBqlTable, PX.Data.EP.IAssign, INotable
	{
		#region Keys
		public class PK : PrimaryKeyOf<POOrder>.By<orderType, orderNbr>
		{
			public static POOrder Find(PXGraph graph, string orderType, string orderNbr, PKFindOptions options = PKFindOptions.None) => FindBy(graph, orderType, orderNbr, options);
		}
		public static class FK
		{
			public class Branch : GL.Branch.PK.ForeignKeyOf<POOrder>.By<branchID> { }
			public class RemittanceAddress : PORemitAddress.PK.ForeignKeyOf<POOrder>.By<remitAddressID> { }
			public class RemittanceContact : PORemitContact.PK.ForeignKeyOf<POOrder>.By<remitContactID> { }
			public class SOOrderType : SO.SOOrderType.PK.ForeignKeyOf<POOrder>.By<sOOrderType> { }
			public class SOOrder : SO.SOOrder.PK.ForeignKeyOf<POOrder>.By<sOOrderType, sOOrderNbr> { }
			public class DemandSOOrder : PX.Objects.PO.DemandSOOrder.PK.ForeignKeyOf<POOrder>.By<sOOrderType, sOOrderNbr> { }
			public class Requisition : RQ.RQRequisition.PK.ForeignKeyOf<POOrder>.By<rQReqNbr> { }
			public class Site : INSite.PK.ForeignKeyOf<POOrder>.By<siteID> { }
			public class ShipAddress : POShipAddress.PK.ForeignKeyOf<POOrder>.By<shipAddressID> { }
			public class ShipContact : POShipContact.PK.ForeignKeyOf<POOrder>.By<shipContactID> { }
			public class FOBPoint : CS.FOBPoint.PK.ForeignKeyOf<POOrder>.By<fOBPoint> { }
			public class Vendor : BAccount.PK.ForeignKeyOf<POOrder>.By<vendorID> { }
			public class VendorLocation : Location.PK.ForeignKeyOf<POOrder>.By<vendorID, vendorLocationID> { }
			public class BlanketOrder : POOrder.PK.ForeignKeyOf<POOrder>.By<bLType, bLOrderNbr> { }
			public class Currency : CM.Currency.PK.ForeignKeyOf<POOrder>.By<curyID> { }
			public class CurrencyInfo : CM.CurrencyInfo.PK.ForeignKeyOf<POOrder>.By<curyInfoID> { }
			public class TaxZone : TX.TaxZone.PK.ForeignKeyOf<POOrder>.By<taxZoneID> { }
			public class Terms : CS.Terms.PK.ForeignKeyOf<POOrder>.By<termsID> { }
			public class Owner : Contact.PK.ForeignKeyOf<POOrder>.By<ownerID> { }
			public class ShipToAccount : BAccount.PK.ForeignKeyOf<POOrder>.By<shipToBAccountID> { }
			public class ShipToLocation : Location.PK.ForeignKeyOf<POOrder>.By<shipToBAccountID, shipToLocationID> { }
			public class Workgroup : EPCompanyTree.PK.ForeignKeyOf<POOrder>.By<ownerWorkgroupID> { }
			public class Carrier : CS.Carrier.PK.ForeignKeyOf<POOrder>.By<shipVia> { }
			public class PayToVendor : AP.Vendor.PK.ForeignKeyOf<POOrder>.By<payToVendorID> { }
			public class Project : PMProject.PK.ForeignKeyOf<POOrder>.By<projectID> { }
		}
		#endregion
		#region Events
		public class Events : PXEntityEvent<POOrder>.Container<Events>
		{
			public PXEntityEvent<POOrder> LinesCompleted;
			public PXEntityEvent<POOrder> LinesClosed;
			public PXEntityEvent<POOrder> LinesReopened;

			public PXEntityEvent<POOrder> LinesLinked;
			public PXEntityEvent<POOrder> LinesUnlinked;

			public PXEntityEvent<POOrder> Printed;

			public PXEntityEvent<POOrder> DoNotPrintChecked;
			public PXEntityEvent<POOrder> DoNotEmailChecked;

			public PXEntityEvent<POOrder> ReleaseChangeOrder;
		}
		#endregion

		#region Selected
		public abstract class selected : PX.Data.BQL.BqlBool.Field<selected> { }
		protected bool? _Selected = false;
		[PXBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Selected")]
		public virtual bool? Selected
		{
			get
			{
				return _Selected;
			}
			set
			{
				_Selected = value;
			}
		}
		#endregion
		#region BranchID
		public abstract class branchID : PX.Data.BQL.BqlInt.Field<branchID> { }
		protected Int32? _BranchID;
		[GL.Branch(typeof(AccessInfo.branchID), IsDetail = false, TabOrder = 0)]
		[PXFormula(typeof(Switch<Case<Where<POOrder.vendorLocationID, IsNotNull,
					And<Selector<POOrder.vendorLocationID, Location.vBranchID>, IsNotNull>>,
				Selector<POOrder.vendorLocationID, Location.vBranchID>,
				Case<Where<Current2<POOrder.branchID>, IsNotNull>,
					Current2<POOrder.branchID>>>,
			Current<AccessInfo.branchID>>))]
		public virtual Int32? BranchID
		{
			get
			{
				return this._BranchID;
			}
			set
			{
				this._BranchID = value;
			}
		}
		#endregion
		#region OrderType
		public abstract class orderType : PX.Data.BQL.BqlString.Field<orderType> { }
		protected String _OrderType;
		[PXDBString(2, IsKey = true, IsFixed = true)]
		[PXDefault(POOrderType.RegularOrder)]
		[POOrderType.List()]
		[PXUIField(DisplayName = "Type", Visibility = PXUIVisibility.SelectorVisible, Enabled = true)]
		[PX.Data.EP.PXFieldDescription]
		public virtual String OrderType
		{
			get
			{
				return this._OrderType;
			}
			set
			{
				this._OrderType = value;
			}
		}
		#endregion
		#region OrderNbr
		public abstract class orderNbr : PX.Data.BQL.BqlString.Field<orderNbr> { }
		protected String _OrderNbr;
		[PXDBString(15, IsKey = true, IsUnicode = true, InputMask = ">CCCCCCCCCCCCCCC")]
		[PXDefault()]
		[PXUIField(DisplayName = "Order Nbr.", Visibility = PXUIVisibility.SelectorVisible)]
		[PO.Numbering()]
		[PO.RefNbr(typeof(Search2<POOrder.orderNbr,
			LeftJoinSingleTable<Vendor, On<POOrder.vendorID, Equal<Vendor.bAccountID>,
			And<Match<Vendor, Current<AccessInfo.userName>>>>>,
			Where<POOrder.orderType, Equal<Optional<POOrder.orderType>>,
			And<Vendor.bAccountID, IsNotNull>>,
			OrderBy<Desc<POOrder.orderNbr>>>), Filterable = true)]
		[PX.Data.EP.PXFieldDescription]
		public virtual String OrderNbr
		{
			get
			{
				return this._OrderNbr;
			}
			set
			{
				this._OrderNbr = value;
			}
		}
		#endregion
		#region Behavior
		public abstract class behavior : PX.Data.BQL.BqlString.Field<behavior> { }
		[PXDBString(1, IsFixed = true, InputMask = ">a")]
		[PXUIField(DisplayName = "Workflow", FieldClass = PM.PMChangeOrder.FieldClass, Visible = false)]
		[PXDefault(POBehavior.Standard)]
		[POBehavior.List]
		public virtual String Behavior
		{
			get;
			set;
		}
		#endregion
		#region OverrideCurrency
		public abstract class overrideCurrency : PX.Data.BQL.BqlBool.Field<overrideCurrency> { }

		[PXBool]
		[PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual bool? OverrideCurrency
		{
			get;
			set;
		}
		#endregion
		#region VendorID
		public abstract class vendorID : PX.Data.BQL.BqlInt.Field<vendorID> { }
		protected Int32? _VendorID;
		[POVendor(Visibility = PXUIVisibility.SelectorVisible, DescriptionField = typeof(Vendor.acctName), CacheGlobal = true, Filterable = true)]
		[PXDefault]
		[PXForeignReference(typeof(FK.Vendor))]
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
		[LocationActive(typeof(Where<Location.bAccountID, Equal<Current<POOrder.vendorID>>,
			And<MatchWithBranch<Location.vBranchID>>>), DescriptionField = typeof(Location.descr), Visibility = PXUIVisibility.SelectorVisible)]
		[PXDefault(typeof(Coalesce<Search2<BAccountR.defLocationID, 
			InnerJoin<CRLocation, On<CRLocation.bAccountID, Equal<BAccountR.bAccountID>, And<CRLocation.locationID, Equal<BAccountR.defLocationID>>>>, 
			Where<BAccountR.bAccountID, Equal<Current<POOrder.vendorID>>, 
				And<CRLocation.isActive, Equal<True>,
				And<MatchWithBranch<CRLocation.vBranchID>>>>>,
			Search<CRLocation.locationID,
			Where<CRLocation.bAccountID, Equal<Current<POOrder.vendorID>>,
			And<CRLocation.isActive, Equal<True>, And<MatchWithBranch<CRLocation.vBranchID>>>>>>))]
		[PXForeignReference(typeof(Field<vendorLocationID>.IsRelatedTo<Location.locationID>))]
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
		#region OrderDate
		public abstract class orderDate : PX.Data.BQL.BqlDateTime.Field<orderDate> { }
		protected DateTime? _OrderDate;

		[PXDBDate()]
		[PXDefault(typeof(AccessInfo.businessDate))]
		[PXUIField(DisplayName = "Date", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual DateTime? OrderDate
		{
			get
			{
				return this._OrderDate;
			}
			set
			{
				this._OrderDate = value;
			}
		}
		#endregion
		#region ExpectedDate
		public abstract class expectedDate : PX.Data.BQL.BqlDateTime.Field<expectedDate> { }
		protected DateTime? _ExpectedDate;

		[PXDBDate()]
		[PXDefault(typeof(POOrder.orderDate), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Promised On")]
		public virtual DateTime? ExpectedDate
		{
			get
			{
				return this._ExpectedDate;
			}
			set
			{
				this._ExpectedDate = value;
			}
		}
		#endregion
		#region ExpirationDate
		public abstract class expirationDate : PX.Data.BQL.BqlDateTime.Field<expirationDate> { }
		protected DateTime? _ExpirationDate;

		[PXDBDate()]
		[PXDefault(typeof(POOrder.orderDate), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Expires On")]
		public virtual DateTime? ExpirationDate
		{
			get
			{
				return this._ExpirationDate;
			}
			set
			{
				this._ExpirationDate = value;
			}
		}
		#endregion
		#region Status
		public abstract class status : PX.Data.BQL.BqlString.Field<status> { }
		protected String _Status;
		[PXDBString(1, IsFixed = true)]
		[PXDefault(POOrderStatus.Hold)]
		[PXUIField(DisplayName = "Status", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
		[POOrderStatus.List()]
		public virtual String Status
		{
			get
			{
				return this._Status;
			}
			set
			{
				this._Status = value;
			}
		}
		#endregion
		#region Hold
		public abstract class hold : PX.Data.BQL.BqlBool.Field<hold> { }
		protected Boolean? _Hold;
		[PXDBBool()]
		[PXUIField(DisplayName = "Hold", Enabled = false)]
		[PXDefault(true)]
		[PXNoUpdate]
		public virtual Boolean? Hold
		{
			get
			{
				return this._Hold;
			}
			set
			{
				this._Hold = value;				
			}
		}
		#endregion
		#region Approved
		public abstract class approved : PX.Data.BQL.BqlBool.Field<approved> { }
		protected Boolean? _Approved;
		[PXDBBool()]
		[PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual Boolean? Approved
		{
			get
			{
				return this._Approved;
			}
			set
			{
				this._Approved = value;				
			}
		}
		#endregion
		#region Rejected
		public abstract class rejected : PX.Data.BQL.BqlBool.Field<rejected> { }
		protected bool? _Rejected = false;
		[PXBool]
		[PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Reject", Visibility = PXUIVisibility.Visible, Enabled = false)]
		public bool? Rejected
		{
			get
			{
				return _Rejected;
			}
			set
			{
				_Rejected = value;											
			}
		}
		#endregion
		#region RequestApproval
		public abstract class requestApproval : PX.Data.BQL.BqlBool.Field<requestApproval> { }
		protected Boolean? _RequestApproval;
		[PXBool()]
		[PXUIField(DisplayName = "Request Approval", Visible = false)]
		public virtual Boolean? RequestApproval
		{
			get
			{
				return this._RequestApproval;
			}
			set
			{
				this._RequestApproval = value;
			}
		}
		#endregion
		#region Cancelled
		public abstract class cancelled : PX.Data.BQL.BqlBool.Field<cancelled> { }
		protected Boolean? _Cancelled;
		[PXDBBool()]
		[PXUIField(DisplayName = "Cancel", Visibility = PXUIVisibility.Visible)]
		[PXDefault(false)]
		public virtual Boolean? Cancelled
		{
			get
			{
				return this._Cancelled;
			}
			set
			{
				this._Cancelled = value;				
			}
		}
		#endregion
		#region IsTaxValid
		public abstract class isTaxValid : PX.Data.BQL.BqlBool.Field<isTaxValid> { }
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Tax Is Up to Date", Enabled = false)]
		public virtual Boolean? IsTaxValid
		{
			get;
			set;
		}
		#endregion
		#region IsUnbilledTaxValid
		public abstract class isUnbilledTaxValid : PX.Data.BQL.BqlBool.Field<isUnbilledTaxValid> { }
		[PXDBBool()]
		[PXDefault(false)]
		public virtual Boolean? IsUnbilledTaxValid
		{
			get;
			set;
		}
		#endregion
		#region ExternalTaxesImportInProgress
		/// <summary>
		/// Indicates that taxes were calculated by an external system
		/// </summary>
		public abstract class externalTaxesImportInProgress : PX.Data.BQL.BqlBool.Field<externalTaxesImportInProgress> { }
		[PXBool()]
		[PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual Boolean? ExternalTaxesImportInProgress
		{
			get;
			set;
		}
		#endregion
		#region NoteID
		public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }
		protected Guid? _NoteID;
		[PXSearchable(SM.SearchCategory.PO, "{0} - {2}", new Type[] { typeof(POOrder.orderNbr), typeof(POOrder.vendorID), typeof(Vendor.acctName) },
		   new Type[] { typeof(POOrder.vendorRefNbr), typeof(POOrder.orderDesc) },
		   NumberFields = new Type[] { typeof(POOrder.orderNbr) },
		   Line1Format = "{0}{1:d}{2}{3}{4}", Line1Fields = new Type[] { typeof(POOrder.orderType), typeof(POOrder.orderDate), typeof(POOrder.status), typeof(POOrder.vendorRefNbr), typeof(POOrder.expectedDate) },
		   Line2Format = "{0}", Line2Fields = new Type[] { typeof(POOrder.orderDesc) },
		   MatchWithJoin = typeof(InnerJoin<Vendor, On<Vendor.bAccountID, Equal<POOrder.vendorID>>>),
		   SelectForFastIndexing = typeof(Select2<POOrder, InnerJoin<Vendor, On<POOrder.vendorID, Equal<Vendor.bAccountID>>>>)
		)]
		[PXNote(ShowInReferenceSelector = true, Selector = typeof(
			Search2<
				POOrder.orderNbr,
			LeftJoinSingleTable<Vendor, 
				On<POOrder.vendorID, Equal<Vendor.bAccountID>,
				And<Match<Vendor, Current<AccessInfo.userName>>>>>,
			Where<
				Vendor.bAccountID, IsNotNull,
				And<POOrder.orderType, NotEqual<POOrderType.regularSubcontract>>>,
			OrderBy<
				Desc<POOrder.orderNbr>>>))]
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
		#region CuryID
		public abstract class curyID : PX.Data.BQL.BqlString.Field<curyID> { }
		protected String _CuryID;
		[PXDBString(5, IsUnicode = true, InputMask = ">LLLLL")]
		[PXUIField(DisplayName = "Currency", Visibility = PXUIVisibility.SelectorVisible)]
		[PXDefault(typeof(Current<AccessInfo.baseCuryID>))]
		[PXSelector(typeof(Currency.curyID))]
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
		#region CuryInfoID
		public abstract class curyInfoID : PX.Data.BQL.BqlLong.Field<curyInfoID> { }
		protected Int64? _CuryInfoID;
		[PXDBLong]
		[CurrencyInfo]
		public virtual Int64? CuryInfoID
		{
			get
			{
				return this._CuryInfoID;
			}
			set
			{
				this._CuryInfoID = value;
			}
		}
		#endregion
		#region LineCntr
		public abstract class lineCntr : PX.Data.BQL.BqlInt.Field<lineCntr> { }
		protected Int32? _LineCntr;
		[PXDBInt()]
		[PXDefault(0)]
		public virtual Int32? LineCntr
		{
			get
			{
				return this._LineCntr;
			}
			set
			{
				this._LineCntr = value;
			}
		}
		#endregion

		#region LinesToCloseCntr
		public abstract class linesToCloseCntr : PX.Data.BQL.BqlInt.Field<linesToCloseCntr> { }
		[PXDBInt]
		[PXDefault(0)]
		public virtual int? LinesToCloseCntr
		{
			get;
			set;
		}
		#endregion
		#region LinesToCompleteCntr
		public abstract class linesToCompleteCntr : PX.Data.BQL.BqlInt.Field<linesToCompleteCntr> { }
		[PXDBInt]
		[PXDefault(0)]
		public virtual int? LinesToCompleteCntr
		{
			get;
			set;
		}
		#endregion

		#region VendorRefNbr
		public abstract class vendorRefNbr : PX.Data.BQL.BqlString.Field<vendorRefNbr> { }
		protected String _VendorRefNbr;
		[PXDBString(40, IsUnicode = true)]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Vendor Ref.", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual String VendorRefNbr
		{
			get
			{
				return this._VendorRefNbr;
			}
			set
			{
				this._VendorRefNbr = value;
			}
		}
		#endregion
		#region CuryOrderTotal
		public abstract class curyOrderTotal : PX.Data.BQL.BqlDecimal.Field<curyOrderTotal> { }
		protected Decimal? _CuryOrderTotal;

		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXDBCurrency(typeof(POOrder.curyInfoID), typeof(POOrder.orderTotal))]
		[PXUIField(DisplayName = "Order Total", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
		public virtual Decimal? CuryOrderTotal
		{
			get
			{
				return this._CuryOrderTotal;
			}
			set
			{
				this._CuryOrderTotal = value;
			}
		}
		#endregion
		#region OrderTotal
		public abstract class orderTotal : PX.Data.BQL.BqlDecimal.Field<orderTotal> { }
		protected Decimal? _OrderTotal;
		[PXDBBaseCury()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Order Total")]
		public virtual Decimal? OrderTotal
		{
			get
			{
				return this._OrderTotal;
			}
			set
			{
				this._OrderTotal = value;
			}
		}
		#endregion
		#region CuryControlTotal
		public abstract class curyControlTotal : PX.Data.BQL.BqlDecimal.Field<curyControlTotal> { }
		protected Decimal? _CuryControlTotal;
		[PXDBCurrency(typeof(POOrder.curyInfoID), typeof(POOrder.controlTotal))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Control Total")]
		public virtual Decimal? CuryControlTotal
		{
			get
			{
				return this._CuryControlTotal;
			}
			set
			{
				this._CuryControlTotal = value;
			}
		}
		#endregion
		#region ControlTotal
		public abstract class controlTotal : PX.Data.BQL.BqlDecimal.Field<controlTotal> { }
		protected Decimal? _ControlTotal;
		[PXDBBaseCury()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? ControlTotal
		{
			get
			{
				return this._ControlTotal;
			}
			set
			{
				this._ControlTotal = value;
			}
		}
		#endregion
		#region OrderQty
		public abstract class orderQty : PX.Data.BQL.BqlDecimal.Field<orderQty> { }
		protected Decimal? _OrderQty;
		[PXDBQuantity()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Order Qty")]
		public virtual Decimal? OrderQty
		{
			get
			{
				return this._OrderQty;
			}
			set
			{
				this._OrderQty = value;
			}
		}
		#endregion

		#region CuryLineDiscTotal
		/// <inheritdoc cref="CuryLineDiscTotal"/>
		public abstract class curyLineDiscTotal : PX.Data.BQL.BqlDecimal.Field<curyLineDiscTotal> { }
		/// <summary>
		/// The total <see cref="lineDiscTotal">discount of the document</see> (in the currency of the document),
		/// which is calculated as the sum of line discounts of the order.
		/// </summary>
		[PXDBCurrency(typeof(POOrder.curyInfoID), typeof(POOrder.lineDiscTotal))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Line Discounts", Enabled = false)]
		public virtual Decimal? CuryLineDiscTotal
		{
			get;
			set;
		}
		#endregion
		#region LineDiscTotal
		/// <inheritdoc cref="LineDiscTotal"/>
		public abstract class lineDiscTotal : PX.Data.BQL.BqlDecimal.Field<lineDiscTotal> { }
		/// <summary>
		/// The total line discount of the document, which is calculated as the sum of line discounts of the order.
		/// </summary>
		[PXDBBaseCury()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Line Discounts", Enabled = false)]
		public virtual Decimal? LineDiscTotal
		{
			get;
			set;
		}
		#endregion
		#region CuryGroupDiscTotal
		/// <inheritdoc cref="CuryGroupDiscTotal"/>
		public abstract class curyGroupDiscTotal : PX.Data.BQL.BqlDecimal.Field<curyGroupDiscTotal> { }
		/// <summary>
		/// The total <see cref="groupDiscTotal">discount of the document</see> (in the currency of the document),
		/// which is calculated as the sum of group discounts of the order.
		/// </summary>
		[PXDBCurrency(typeof(POOrder.curyInfoID), typeof(POOrder.groupDiscTotal))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Group Discounts", Enabled = false)]
		public virtual Decimal? CuryGroupDiscTotal
		{
			get;
			set;
		}
		#endregion
		#region GroupDiscTot
		/// <inheritdoc cref="GroupDiscTotal"/>
		public abstract class groupDiscTotal : PX.Data.BQL.BqlDecimal.Field<groupDiscTotal> { }
		/// <summary>
		/// The total group discount of the document, which is calculated as the sum of group discounts of the order.
		/// </summary>
		[PXDBBaseCury()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Group Discounts", Enabled = false)]
		public virtual Decimal? GroupDiscTotal
		{
			get;
			set;
		}
		#endregion
		#region CuryDocumentDiscTotal
		/// <inheritdoc cref="CuryDocumentDiscTotal"/>
		public abstract class curyDocumentDiscTotal : PX.Data.BQL.BqlDecimal.Field<curyDocumentDiscTotal> { }
		/// <summary>
		/// The total <see cref="documentDiscTotal">discount of the document</see> (in the currency of the document),
		/// which is calculated as the sum of document discounts of the order.
		/// </summary>
		[PXDBCurrency(typeof(POOrder.curyInfoID), typeof(POOrder.documentDiscTotal))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Document Discount", Enabled = false)]
		public virtual Decimal? CuryDocumentDiscTotal
		{
			get;
			set;
		}
		#endregion
		#region DocumentDiscTotal
		/// <inheritdoc cref="DocumentDiscTotal"/>
		public abstract class documentDiscTotal : PX.Data.BQL.BqlDecimal.Field<documentDiscTotal> { }
		/// <summary>
		/// The total document discount of the document, which is calculated as the sum of document discounts of the order.
		/// </summary>
		/// <remarks>
		/// <para>If the <see cref="FeaturesSet.vendorDiscounts">Vendor Discounts</see> feature is not enabled on
		/// the Enable/Disable Features (CS100000) form,
		/// a user can enter a document-level discount manually. This manual discount has no discount code or
		/// sequence and is not recalculated by the system. If the manual discount needs to be changed, a user has to
		/// correct it manually.</para>
		/// </remarks>
		[PXDBBaseCury()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Document Discount", Enabled = false)]
		public virtual Decimal? DocumentDiscTotal
		{
			get;
			set;
		}
		#endregion
		#region DiscTot
		public abstract class discTot : PX.Data.BQL.BqlDecimal.Field<discTot> { }
		[PXDBBaseCury()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? DiscTot
		{
			get;
			set;
		}
		#endregion
		#region CuryDiscTot
		public abstract class curyDiscTot : PX.Data.BQL.BqlDecimal.Field<curyDiscTot> { }
		[PXDBCurrency(typeof(POOrder.curyInfoID), typeof(POOrder.discTot))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Document Discounts", Enabled = true)]
		public virtual Decimal? CuryDiscTot
		{
			get;
			set;
		}
		#endregion
		#region CuryOrderDiscTotal
		/// <inheritdoc cref="CuryOrderDiscTotal"/>
		public abstract class curyOrderDiscTotal : PX.Data.BQL.BqlDecimal.Field<curyOrderDiscTotal> { }
		/// <summary>
		/// The total <see cref="orderDiscTotal">discount of the document</see> (in the currency of the document),
		/// which is calculated as the sum of all group, document and line discounts of the order.
		/// </summary>
		[PXCurrency(typeof(POOrder.curyInfoID), typeof(POOrder.discTot))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXDBCalced(typeof(Add<POOrder.curyDiscTot, POOrder.curyLineDiscTotal>), typeof(Decimal))]
		[PXFormula(typeof(Add<POOrder.curyDiscTot, POOrder.curyLineDiscTotal>))]
		[PXUIField(DisplayName = "Discount Total")]
		public virtual Decimal? CuryOrderDiscTotal
		{
			get;
			set;
		}
		#endregion
		#region OrderDiscTotal
		/// <inheritdoc cref="OrderDiscTotal"/>
		public abstract class orderDiscTotal : PX.Data.BQL.BqlDecimal.Field<orderDiscTotal> { }
		/// <summary>
		/// The total discount of the document, which is calculated as the sum of group, document and line discounts of the order.
		/// </summary>
		[PXBaseCury()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXDBCalced(typeof(Add<POOrder.discTot, POOrder.lineDiscTotal>), typeof(Decimal))]
		[PXUIField(DisplayName = "Discount Total")]
		public virtual Decimal? OrderDiscTotal
		{
			get;
			set;
		}
		#endregion

		#region CuryGoodsExtPriceTotal
		/// <inheritdoc cref="CuryGoodsExtCostTotal"/>
		public abstract class curyGoodsExtCostTotal : PX.Data.BQL.BqlDecimal.Field<curyGoodsExtCostTotal> { }
		/// <summary>
		/// The total amount on all lines of the document, except for Freight, Description and Service lines, before Line-level discounts
		/// are applied (in the currency of the document).
		/// </summary>
		[PXDBCurrency(typeof(POOrder.curyInfoID), typeof(POOrder.goodsExtCostTotal))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Goods")]
		public virtual Decimal? CuryGoodsExtCostTotal
		{
			get;
			set;
		}
		#endregion
		#region GoodsTotal
		/// <inheritdoc cref="GoodsExtCostTotal"/>
		public abstract class goodsExtCostTotal : PX.Data.BQL.BqlDecimal.Field<goodsExtCostTotal> { }
		/// <summary>
		/// The total amount on all lines of the document, except for Freight, Description and Service lines, before Line-level discounts
		/// are applied.
		/// </summary>
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? GoodsExtCostTotal
		{
			get;
			set;
		}
		#endregion
		#region CuryServiceExtCostTotal
		/// <inheritdoc cref="CuryServiceExtCostTotal"/>
		public abstract class curyServiceExtCostTotal : PX.Data.BQL.BqlDecimal.Field<curyServiceExtCostTotal> { }
		/// <summary>
		/// The total amount on all Service lines of the document, before Line-level discounts
		/// are applied (in the currency of the document).
		/// </summary>
		[PXDBCurrency(typeof(POOrder.curyInfoID), typeof(POOrder.serviceExtCostTotal))]
		[PXUIField(DisplayName = "Services", Enabled = false)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? CuryServiceExtCostTotal
		{
			get;
			set;
		}
		#endregion
		#region ServiceExtCostTotal
		/// <inheritdoc cref="ServiceExtCostTotal"/>
		public abstract class serviceExtCostTotal : PX.Data.BQL.BqlDecimal.Field<serviceExtCostTotal> { }
		/// <summary>
		/// The total amount on all Service lines of the document, before Line-level discounts are applied.
		/// </summary>
		[PXDBDecimal(4)]
		public virtual Decimal? ServiceExtCostTotal
		{
			get;
			set;
		}
		#endregion
		#region CuryFreightTot
		/// <inheritdoc cref="CuryFreightTot"/>
		public abstract class curyFreightTot : PX.Data.BQL.BqlDecimal.Field<curyFreightTot> { }
		/// <summary>
		/// The total amount on all Freight lines of the document, before Line-level discounts
		/// are applied (in the currency of the document).
		/// </summary>
		[PXDBCurrency(typeof(POOrder.curyInfoID), typeof(POOrder.freightTot))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Freight Total")]
		public virtual Decimal? CuryFreightTot
		{
			get;
			set;
		}
		#endregion
		#region FreightTot
		/// <inheritdoc cref="FreightTot"/>
		public abstract class freightTot : PX.Data.BQL.BqlDecimal.Field<freightTot> { }
		/// <summary>
		/// The total amount on all Freight lines of the document, before Line-level discounts are applied.
		/// </summary>
		[PXDBDecimal(4)]
		public virtual Decimal? FreightTot
		{
			get;
			set;
		}
		#endregion
		#region CuryDetailExtCostTotal
		/// <inheritdoc cref="CuryDetailExtCostTotal"/>
		public abstract class curyDetailExtCostTotal : PX.Data.BQL.BqlDecimal.Field<curyDetailExtCostTotal> { }
		/// <summary>
		/// The <see cref="detailExtCostTotal">sum</see> of the
		/// <see cref="curyGoodsExtCostTotal">goods</see>,
		/// <see cref="curyServiceExtCostTotal">services</see> and
		/// the <see cref="curyFreightTot">freight amount</see> values.
		/// </summary>
		[PXCurrency(typeof(POOrder.curyInfoID), typeof(POOrder.detailExtCostTotal))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXDBCalced(typeof(Add<Add<POOrder.curyGoodsExtCostTotal, POOrder.curyServiceExtCostTotal>, POOrder.curyFreightTot>), typeof(Decimal))]
		[PXFormula(typeof(Add<Add<POOrder.curyGoodsExtCostTotal, POOrder.curyServiceExtCostTotal>, POOrder.curyFreightTot>))]
		[PXUIField(DisplayName = "Detail Total")]
		public virtual Decimal? CuryDetailExtCostTotal
		{
			get;
			set;
		}
		#endregion
		#region DetailExtCostTotal
		/// <inheritdoc cref="DetailExtCostTotal"/>
		public abstract class detailExtCostTotal : PX.Data.BQL.BqlDecimal.Field<detailExtCostTotal> { }
		/// <summary>
		/// The sum of the
		/// <see cref="goodsExtCostTotal">goods</see>,
		/// <see cref="serviceExtCostTotal">services</see> and
		/// the <see cref="freightTot">freight amount</see> values.
		/// </summary>
		[PXDecimal(4)]
		[PXDBCalced(typeof(Add<Add<POOrder.goodsExtCostTotal, POOrder.serviceExtCostTotal>, POOrder.freightTot>), typeof(Decimal))]
		public virtual Decimal? DetailExtCostTotal
		{
			get;
			set;
		}
		#endregion
		#region CuryLineTotal
		public abstract class curyLineTotal : PX.Data.BQL.BqlDecimal.Field<curyLineTotal> { }
		protected Decimal? _CuryLineTotal;
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXDBCurrency(typeof(POOrder.curyInfoID), typeof(POOrder.lineTotal))]
		[PXUIField(DisplayName = "Line Total", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual Decimal? CuryLineTotal
		{
			get
			{
				return this._CuryLineTotal;
			}
			set
			{
				this._CuryLineTotal = value;
			}
		}
		#endregion
		#region LineTotal
		public abstract class lineTotal : PX.Data.BQL.BqlDecimal.Field<lineTotal> { }
		protected Decimal? _LineTotal;
		[PXDBBaseCury()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Line Total")]
		public virtual Decimal? LineTotal
		{
			get
			{
				return this._LineTotal;
			}
			set
			{
				this._LineTotal = value;
			}
		}
		#endregion

		#region CuryTaxTotal
		public abstract class curyTaxTotal : PX.Data.BQL.BqlDecimal.Field<curyTaxTotal> { }
		protected Decimal? _CuryTaxTotal;

		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXDBCurrency(typeof(POOrder.curyInfoID), typeof(POOrder.taxTotal))]
		[PXUIField(DisplayName = "Tax Total")]
		public virtual Decimal? CuryTaxTotal
		{
			get
			{
				return this._CuryTaxTotal;
			}
			set
			{
				this._CuryTaxTotal = value;
			}
		}
		#endregion
		#region TaxTotal
		public abstract class taxTotal : PX.Data.BQL.BqlDecimal.Field<taxTotal> { }
		protected Decimal? _TaxTotal;
		[PXDBBaseCury()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? TaxTotal
		{
			get
			{
				return this._TaxTotal;
			}
			set
			{
				this._TaxTotal = value;
			}
		}
		#endregion

        #region CuryVatExemptTotal
        public abstract class curyVatExemptTotal : PX.Data.BQL.BqlDecimal.Field<curyVatExemptTotal> { }
        protected Decimal? _CuryVatExemptTotal;
        [PXDBCurrency(typeof(POOrder.curyInfoID), typeof(POOrder.vatExemptTotal))]
        [PXUIField(DisplayName = "VAT Exempt", Visibility = PXUIVisibility.Visible, Enabled = false)]
        [PXDefault(TypeCode.Decimal, "0.0")]
        public virtual Decimal? CuryVatExemptTotal
        {
            get
            {
                return this._CuryVatExemptTotal;
            }
            set
            {
                this._CuryVatExemptTotal = value;
            }
        }
        #endregion
        
        #region VatExemptTaxTotal
        public abstract class vatExemptTotal : PX.Data.BQL.BqlDecimal.Field<vatExemptTotal> { }
        protected Decimal? _VatExemptTotal;
        [PXDBDecimal(4)]
        [PXDefault(TypeCode.Decimal, "0.0")]
        public virtual Decimal? VatExemptTotal
        {
            get
            {
                return this._VatExemptTotal;
            }
            set
            {
                this._VatExemptTotal = value;
            }
        }
        #endregion
                        
        #region CuryVatTaxableTotal
        public abstract class curyVatTaxableTotal : PX.Data.BQL.BqlDecimal.Field<curyVatTaxableTotal> { }
        protected Decimal? _CuryVatTaxableTotal;
        [PXDBCurrency(typeof(POOrder.curyInfoID), typeof(POOrder.vatTaxableTotal))]
        [PXUIField(DisplayName = "VAT Taxable", Visibility = PXUIVisibility.Visible, Enabled = false)]
        [PXDefault(TypeCode.Decimal, "0.0")]
        public virtual Decimal? CuryVatTaxableTotal
        {
            get
            {
                return this._CuryVatTaxableTotal;
            }
            set
            {
                this._CuryVatTaxableTotal = value;
            }
        }
        #endregion

        #region VatTaxableTotal
        public abstract class vatTaxableTotal : PX.Data.BQL.BqlDecimal.Field<vatTaxableTotal> { }
        protected Decimal? _VatTaxableTotal;
        [PXDBDecimal(4)]
        [PXDefault(TypeCode.Decimal, "0.0")]
        public virtual Decimal? VatTaxableTotal
        {
            get
            {
                return this._VatTaxableTotal;
            }
            set
            {
                this._VatTaxableTotal = value;
            }
        }
        #endregion

        #region TaxZoneID
		public abstract class taxZoneID : PX.Data.BQL.BqlString.Field<taxZoneID> { }
		protected String _TaxZoneID;
		[PXDBString(10, IsUnicode = true)]
		[PXUIField(DisplayName = "Vendor Tax Zone", Visibility = PXUIVisibility.Visible)]
		[PXSelector(typeof(TX.TaxZone.taxZoneID), DescriptionField = typeof(TX.TaxZone.descr), Filterable = true)]
		[PXRestrictor(typeof(Where<TX.TaxZone.isManualVATZone, Equal<False>>), TX.Messages.CantUseManualVAT)]
		[PXFormula(typeof(Default<POOrder.vendorLocationID>))]
		public virtual String TaxZoneID
		{
			get
			{
				return this._TaxZoneID;
			}
			set
			{
				this._TaxZoneID = value;
			}
		}
		#endregion

		#region TaxCalcMode
		public abstract class taxCalcMode : PX.Data.BQL.BqlString.Field<taxCalcMode> { }
		protected string _TaxCalcMode;
		[PXDBString(1, IsFixed = true)]
		[PXDefault(TX.TaxCalculationMode.TaxSetting, typeof(Search<Location.vTaxCalcMode, Where<Location.bAccountID, Equal<Current<vendorID>>,
			And<Location.locationID, Equal<Current<vendorLocationID>>>>>))]
		[TX.TaxCalculationMode.List]
		[PXUIField(DisplayName = "Tax Calculation Mode")]
		public virtual string TaxCalcMode
		{
			get { return this._TaxCalcMode; }
			set { this._TaxCalcMode = value; }
		}
		#endregion

		#region TermsID
		public abstract class termsID : PX.Data.BQL.BqlString.Field<termsID> { }
		[PXDBString(10, IsUnicode = true)]
		[PXDefault(
			typeof(Search<Vendor.termsID, 
				Where2<FeatureInstalled<FeaturesSet.vendorRelations>, 
						And<Vendor.bAccountID, Equal<Current<POOrder.payToVendorID>>,
					Or2<Not<FeatureInstalled<FeaturesSet.vendorRelations>>,
						And<Vendor.bAccountID, Equal<Current<POOrder.vendorID>>>>>>>), 
			PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Terms", Visibility = PXUIVisibility.Visible)]
		[PXSelector(typeof(Search<Terms.termsID, Where<Terms.visibleTo, Equal<TermsVisibleTo.all>, Or<Terms.visibleTo, Equal<TermsVisibleTo.vendor>>>>), DescriptionField = typeof(Terms.descr), Filterable = true)]
		public virtual string TermsID { get; set; }
		#endregion

		#region RemitAddressID
		public abstract class remitAddressID : PX.Data.BQL.BqlInt.Field<remitAddressID> { }
		protected Int32? _RemitAddressID;
		[PXDBInt()]
		[PORemitAddress(typeof(Select2<Location,
			InnerJoin<Address, On<Address.bAccountID, Equal<Location.bAccountID>, And<Address.addressID, Equal<Location.defAddressID>>>,
			LeftJoin<PORemitAddress, On<PORemitAddress.bAccountID, Equal<Address.bAccountID>, 
						And<PORemitAddress.bAccountAddressID, Equal<Address.addressID>,
				And<PORemitAddress.revisionID, Equal<Address.revisionID>, And<PORemitAddress.isDefaultAddress, Equal<boolTrue>>>>>>>,
			Where<Location.bAccountID, Equal<Current<POOrder.vendorID>>, And<Location.locationID, Equal<Current<POOrder.vendorLocationID>>>>>))]
		[PXUIField()]
		public virtual Int32? RemitAddressID
		{
			get
			{
				return this._RemitAddressID;
			}
			set
			{
				this._RemitAddressID = value;
			}
		}
		#endregion
		#region RemitContactID
		public abstract class remitContactID : PX.Data.BQL.BqlInt.Field<remitContactID> { }
		protected Int32? _RemitContactID;
		[PXDBInt()]
		[PORemitContact(typeof(Select2<Location,
		    InnerJoin<Contact, On<Contact.bAccountID, Equal<Location.bAccountID>, And<Contact.contactID, Equal<Location.defContactID>>>,
		    LeftJoin<PORemitContact, On<PORemitContact.bAccountID, Equal<Contact.bAccountID>, 
				And<PORemitContact.bAccountContactID, Equal<Contact.contactID>,
		        And<PORemitContact.revisionID, Equal<Contact.revisionID>, 
				And<PORemitContact.isDefaultContact, Equal<boolTrue>>>>>>>,
		    Where<Location.bAccountID, Equal<Current<POOrder.vendorID>>, And<Location.locationID, Equal<Current<POOrder.vendorLocationID>>>>>))]
		public virtual Int32? RemitContactID
		{
			get
			{
				return this._RemitContactID;
			}
			set
			{
				this._RemitContactID = value;
			}
		}
		#endregion

		#region SOOrderType
		public abstract class sOOrderType : PX.Data.BQL.BqlString.Field<sOOrderType> { }
		protected String _SOOrderType;
		[PXDBString(2, IsFixed = true, InputMask = ">aa")]
		[PXSelector(typeof(Search<SO.SOOrderType.orderType, Where<SO.SOOrderType.active, Equal<boolTrue>>>))]
		[PXUIField(DisplayName = "Sales Order Type", Visibility = PXUIVisibility.SelectorVisible)]		
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
		public abstract class sOOrderNbr : PX.Data.BQL.BqlString.Field<sOOrderNbr> { }
		protected String _SOOrderNbr;
		[PXDBString(15, IsUnicode = true, InputMask = ">CCCCCCCCCCCCCCC")]		
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
        [PXSelector(typeof(Search<SO.SOOrder.orderNbr, Where<SO.SOOrder.orderType, Equal<Current<POOrder.sOOrderType>>>>))]
		[PXFormula(typeof(Default<POOrder.sOOrderType>))]
		[PXUIField(DisplayName = "Sales Order Nbr.", Visibility = PXUIVisibility.SelectorVisible)]					
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
		
		#region BLType
		public abstract class bLType : PX.Data.BQL.BqlString.Field<bLType> { }
		protected String _BLType;
		[PXDBString(2, IsFixed = true)]
		public virtual String BLType
		{
			get
			{
				return this._BLType;
			}
			set
			{
				this._BLType = value;
			}
		}
		#endregion
		#region BLOrderNbr
		public abstract class bLOrderNbr : PX.Data.BQL.BqlString.Field<bLOrderNbr> { }
		protected String _BLOrderNbr;
		[PXDBString(15, IsUnicode = true)]
		public virtual String BLOrderNbr
		{
			get
			{
				return this._BLOrderNbr;
			}
			set
			{
				this._BLOrderNbr = value;
			}
		}
		#endregion
		#region RQReqNbr
		public abstract class rQReqNbr : PX.Data.BQL.BqlString.Field<rQReqNbr> { }
		protected String _RQReqNbr;
		[PXDBString(15, IsUnicode = true)]
		[PXUIField(DisplayName="Requisition Ref. Nbr.", Enabled = false)]
		[PXSelector(typeof(PX.Objects.RQ.RQRequisition.reqNbr))]
		public virtual String RQReqNbr
		{
			get
			{
				return this._RQReqNbr;
			}
			set
			{
				this._RQReqNbr = value;
			}
		}
		#endregion
		#region OrderDesc
		public abstract class orderDesc : PX.Data.BQL.BqlString.Field<orderDesc> { }
		protected String _OrderDesc;
		[PXDBString(60, IsUnicode = true)]
		[PXUIField(DisplayName = "Description", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual String OrderDesc
		{
			get
			{
				return this._OrderDesc;
			}
			set
			{
				this._OrderDesc = value;
			}
		}
		#endregion

		#region OriginalPOType
		public abstract class originalPOType : PX.Data.BQL.BqlString.Field<originalPOType> { }

		[PXDBString(2, IsFixed = true)]
		[POOrderType.List]
		[PXUIField(DisplayName = "Originating PO Type", Enabled = false, FieldClass = nameof(FeaturesSet.DropShipments))]
		public virtual String OriginalPOType
		{
			get;
			set;
		}
		#endregion
		#region OriginalPONbr
		public abstract class originalPONbr : PX.Data.BQL.BqlString.Field<originalPONbr> { }

		[PXDBString(15, IsUnicode = true)]
		[PXUIField(DisplayName = "Originating PO Nbr.", Enabled = false, FieldClass = nameof(FeaturesSet.DropShipments))]
		[PXSelector(typeof(Search<POOrder.orderNbr, Where<POOrder.orderType, Equal<Current<POOrder.originalPOType>>>>), ValidateValue = false)]
		public virtual String OriginalPONbr
		{
			get;
			set;
		}
		#endregion
		#region SuccessorPONbr
		public abstract class successorPONbr : PX.Data.BQL.BqlString.Field<successorPONbr> { }

		[PXString(15, IsUnicode = true)]
		[PXUIField(DisplayName = "Normal PO Nbr.", Enabled = false, FieldClass = nameof(FeaturesSet.DropShipments))]
		[PXSelector(typeof(Search<POOrder.orderNbr, Where<POOrder.orderType, Equal<POOrderType.regularOrder>>>), ValidateValue = false)]
		public virtual String SuccessorPONbr
		{
			get;
			set;
		}
		#endregion

		#region DropShipLinesCount
		[Obsolete(Common.Messages.ItemIsObsoleteAndWillBeRemoved2023R1)]
		public abstract class dropShipLinesCount : PX.Data.BQL.BqlInt.Field<dropShipLinesCount> { }

		[PXDefault(0)]
		[PXDBInt]
		[Obsolete(Common.Messages.ItemIsObsoleteAndWillBeRemoved2023R1)]
		public virtual int? DropShipLinesCount
		{
			get;
			set;
		}
		#endregion
		#region DropShipLinkedLinesCount
		public abstract class dropShipLinkedLinesCount : PX.Data.BQL.BqlInt.Field<dropShipLinkedLinesCount> { }

		[PXDefault(0)]
		[PXDBInt]
		public virtual int? DropShipLinkedLinesCount
		{
			get;
			set;
		}
		#endregion
		#region DropShipActiveLinksCount
		[Obsolete(Common.Messages.ItemIsObsoleteAndWillBeRemoved2023R1)]
		public abstract class dropShipActiveLinksCount : PX.Data.BQL.BqlInt.Field<dropShipActiveLinksCount> { }

		[PXDefault(0)]
		[PXDBInt]
		[Obsolete(Common.Messages.ItemIsObsoleteAndWillBeRemoved2023R1)]
		public virtual int? DropShipActiveLinksCount
		{
			get;
			set;
		}
		#endregion
		#region DropShipOpenLinesCntr
		public abstract class dropShipOpenLinesCntr : PX.Data.BQL.BqlInt.Field<dropShipOpenLinesCntr> { }

		[PXDefault(0)]
		[PXDBInt]
		public virtual int? DropShipOpenLinesCntr
		{
			get;
			set;
		}
		#endregion
		#region DropShipNotLinkedLinesCntr
		public abstract class dropShipNotLinkedLinesCntr : PX.Data.BQL.BqlInt.Field<dropShipNotLinkedLinesCntr> { }

		[PXDefault(0)]
		[PXDBInt]
		public virtual int? DropShipNotLinkedLinesCntr
		{
			get;
			set;
		}
		#endregion

		#region IsLegacyDropShip
		public abstract class isLegacyDropShip : PX.Data.BQL.BqlBool.Field<isLegacyDropShip> { }

		[PXDefault(false)]
		[PXDBBool]
		public virtual bool? IsLegacyDropShip
		{
			get;
			set;
		}
		#endregion

		#region tstamp
		public abstract class Tstamp : PX.Data.BQL.BqlByteArray.Field<Tstamp> { }
		protected Byte[] _tstamp;
		[PXDBTimestamp(VerifyTimestamp = VerifyTimestampOptions.BothFromGraphAndRecord)]
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
		[PXDBCreatedByID()]
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
		[PXDBCreatedByScreenID()]
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
		[PXDBCreatedDateTime()]
		[PXUIField(DisplayName = PXDBLastModifiedByIDAttribute.DisplayFieldNames.CreatedDateTime, Enabled = false, IsReadOnly = true)]
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
		[PXDBLastModifiedByID()]
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
		[PXDBLastModifiedByScreenID()]
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
		[PXDBLastModifiedDateTime()]
		[PXUIField(DisplayName = PXDBLastModifiedByIDAttribute.DisplayFieldNames.LastModifiedDateTime, Enabled = false, IsReadOnly = true)]
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
		#region ShipDestType
		public abstract class shipDestType : PX.Data.BQL.BqlString.Field<shipDestType> { }
		protected String _ShipDestType;
		[PXDBString(1, IsFixed = true)]
		[POShippingDestination.List()]
		[PXFormula(typeof(Switch<
			Case<Where<Current<POOrder.orderType>, Equal<POOrderType.dropShip>>, POShippingDestination.customer,
			Case<Where<Current<POOrder.orderType>, Equal<POOrderType.projectDropShip>>, POShippingDestination.projectSite,
			Case<Where<Selector<POOrder.vendorLocationID, Location.vSiteID>, IsNotNull,
				Or<Where<Current<POOrder.vendorLocationID>, IsNotNull, And<Current<POSetup.shipDestType>, Equal<POShipDestType.site>>>>>, POShippingDestination.site>>>, 
			POShippingDestination.company>))]
		[PXUIField(DisplayName = "Shipping Destination Type")]
		public virtual String ShipDestType
		{
			get
			{
				return this._ShipDestType;
			}
			set
			{
				this._ShipDestType = value;
			}
		}
		#endregion
		#region SiteID
		public abstract class siteID : PX.Data.BQL.BqlInt.Field<siteID> { }
		protected Int32? _SiteID;

		[Site(DescriptionField = typeof(INSite.descr), ErrorHandling = PXErrorHandling.Always)]
		[PXDefault((object)null, typeof(Coalesce<
			Search2<LocationBranchSettings.vSiteID,
				InnerJoin<INSite, On<INSite.siteID, Equal<LocationBranchSettings.vSiteID>>>,
				Where<Current<POOrder.shipDestType>, Equal<POShippingDestination.site>,
				And<LocationBranchSettings.bAccountID, Equal<Current<POOrder.vendorID>>,
				And<LocationBranchSettings.locationID, Equal<Current<POOrder.vendorLocationID>>,
				And<LocationBranchSettings.branchID, Equal<Current<POOrder.branchID>>,
				And<Match<INSite, Current<AccessInfo.userName>>>>>>>>,
			Search2<Location.vSiteID,
				InnerJoin<INSite, On<INSite.siteID, Equal<Location.vSiteID>>>,
				Where<Current<POOrder.shipDestType>, Equal<POShippingDestination.site>,
				And<Location.bAccountID, Equal<Current<POOrder.vendorID>>,
				And<Location.locationID, Equal<Current<POOrder.vendorLocationID>>,
				And<Match<INSite, Current<AccessInfo.userName>>>>>>>,
			Search<INSite.siteID, Where<Current<POOrder.shipDestType>, Equal<POShippingDestination.site>,
													And<INSite.active, Equal<True>,
													And<INSite.siteID, NotEqual<SiteAttribute.transitSiteID>,
													And<Match<INSite, Current<AccessInfo.userName>>>>>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXForeignReference(typeof(FK.Site))]
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
        #region SiteIdErrorMessage
        public abstract class siteIdErrorMessage : PX.Data.BQL.BqlString.Field<siteIdErrorMessage> { }
        [PXString(150, IsUnicode = true)]
        public virtual string SiteIdErrorMessage { get; set; }
        #endregion
		#region ShipToBAccountID
		public abstract class shipToBAccountID : PX.Data.BQL.BqlInt.Field<shipToBAccountID> { }
		protected Int32? _ShipToBAccountID;
		[PXDBInt()]
		[PXSelector(typeof(
			Search2<BAccount2.bAccountID,
			LeftJoin<Vendor, On<
				Vendor.bAccountID, Equal<BAccount2.bAccountID>,
				And<Match<Vendor, Current<AccessInfo.userName>>>>,
			LeftJoin<AR.Customer, On<
				AR.Customer.bAccountID, Equal<BAccount2.bAccountID>,
				And<Match<AR.Customer, Current<AccessInfo.userName>>>>,
			LeftJoin<GL.Branch, On<
				GL.Branch.bAccountID, Equal<BAccount2.bAccountID>,
				And<Match<GL.Branch, Current<AccessInfo.userName>>>>>>>,
			Where<
				Vendor.bAccountID, IsNotNull, And<Optional<POOrder.shipDestType>, Equal<POShippingDestination.vendor>,
					And2<Where<BAccount2.type, In3<BAccountType.vendorType, BAccountType.combinedType>, Or<Optional<POOrder.orderType>, NotEqual<POOrderType.dropShip>>>,
			Or<Where<GL.Branch.bAccountID, IsNotNull, And<Optional<POOrder.shipDestType>, Equal<POShippingDestination.company>,
				Or<Where<AR.Customer.bAccountID, IsNotNull, And<Optional<POOrder.shipDestType>, Equal<POShippingDestination.customer>>>>>>>>>>>),
				typeof(BAccount.acctCD), typeof(BAccount.acctName), typeof(BAccount.type), typeof(BAccount.acctReferenceNbr), typeof(BAccount.parentBAccountID),
			SubstituteKey = typeof(BAccount.acctCD), DescriptionField = typeof(BAccount.acctName), CacheGlobal = true)]
		[PXUIField(DisplayName = "Ship To")]
		[PXDefault((object)null, typeof(Search<GL.Branch.bAccountID, Where<GL.Branch.branchID, Equal<Current<POOrder.branchID>>, And<Optional<POOrder.shipDestType>, Equal<POShippingDestination.company>>>>),PersistingCheck=PXPersistingCheck.Nothing)]
		[PXForeignReference(typeof(FK.ShipToAccount))]
		public virtual Int32? ShipToBAccountID
		{
			get
			{
				return this._ShipToBAccountID;
			}
			set
			{
				this._ShipToBAccountID = value;
			}
		}
		#endregion
		#region ShipToLocationID
		public abstract class shipToLocationID : PX.Data.BQL.BqlInt.Field<shipToLocationID> { }
		protected Int32? _ShipToLocationID;

		[LocationActive(typeof(Where<Location.bAccountID, Equal<Current<POOrder.shipToBAccountID>>>), DescriptionField = typeof(Location.descr))]
		[PXDefault((object)null, 
			typeof(Search<BAccount2.defLocationID,
					Where<BAccount2.bAccountID, Equal<Optional<POOrder.shipToBAccountID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Shipping Location")]
		[PXForeignReference(typeof(Field<shipToLocationID>.IsRelatedTo<Location.locationID>))]
		public virtual Int32? ShipToLocationID
		{
			get
			{
				return this._ShipToLocationID;
			}
			set
			{
				this._ShipToLocationID = value;
			}
		}
		#endregion

		#region ShipAddressID
		public abstract class shipAddressID : PX.Data.BQL.BqlInt.Field<shipAddressID> { }
		protected Int32? _ShipAddressID;
		[PXDBInt()]
		[POShipAddress(typeof(Select2<Address,
					InnerJoin<CRLocation, On<Address.bAccountID, Equal<CRLocation.bAccountID>,
						And<Address.addressID, Equal<CRLocation.defAddressID>,
						And<Current<POOrder.shipDestType>, NotEqual<POShippingDestination.site>,
						And<CRLocation.bAccountID, Equal<Current<POOrder.shipToBAccountID>>,
						And<CRLocation.locationID, Equal<Current<POOrder.shipToLocationID>>>>>>>,
					LeftJoin<POShipAddress, On<POShipAddress.bAccountID, Equal<Address.bAccountID>,
						And<POShipAddress.bAccountAddressID, Equal<Address.addressID>,
						And<POShipAddress.revisionID, Equal<Address.revisionID>,
						And<POShipAddress.isDefaultAddress, Equal<boolTrue>>>>>>>,
					Where<True, Equal<True>>>))]
		[PXUIField()]
		public virtual Int32? ShipAddressID
		{
			get
			{
				return this._ShipAddressID;
			}
			set
			{
				this._ShipAddressID = value;
			}
		}
		#endregion
		#region ShipContactID
		public abstract class shipContactID : PX.Data.BQL.BqlInt.Field<shipContactID> { }
		protected Int32? _ShipContactID;
		[PXDBInt()]
		[POShipContact(typeof(Select2<Contact,
					InnerJoin<CRLocation, On<Contact.bAccountID, Equal<CRLocation.bAccountID>,
						And<Contact.contactID, Equal<CRLocation.defContactID>,
						And<Current<POOrder.shipDestType>, NotEqual<POShippingDestination.site>,
						And<CRLocation.bAccountID, Equal<Current<POOrder.shipToBAccountID>>,
						And<CRLocation.locationID, Equal<Current<POOrder.shipToLocationID>>>>>>>,
					LeftJoin<POShipContact, On<POShipContact.bAccountID, Equal<Contact.bAccountID>,
						And<POShipContact.bAccountContactID, Equal<Contact.contactID>,
						And<POShipContact.revisionID, Equal<Contact.revisionID>,
						And<POShipContact.isDefaultContact, Equal<boolTrue>>>>>>>,
					Where<True, Equal<True>>>))]
		[PXUIField()]
		public virtual Int32? ShipContactID
		{
			get
			{
				return this._ShipContactID;
			}
			set
			{
				this._ShipContactID = value;
			}
		}
		#endregion
		#region VendorID_Vendor_acctName
		public abstract class vendorID_Vendor_acctName : PX.Data.BQL.BqlString.Field<vendorID_Vendor_acctName> { }
		#endregion

		#region OpenOrderQty
		public abstract class openOrderQty : PX.Data.BQL.BqlDecimal.Field<openOrderQty> { }
		[PXDBQuantity]
		[PXUIField(DisplayName = "Open Quantity", Enabled = false)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? OpenOrderQty
		{
			get;
			set;
		}
		#endregion
		#region CuryUnbilledOrderTotal
		public abstract class curyUnbilledOrderTotal : PX.Data.BQL.BqlDecimal.Field<curyUnbilledOrderTotal> { }
		[PXDBCurrency(typeof(POOrder.curyInfoID), typeof(POOrder.unbilledOrderTotal))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Unbilled Amount", Enabled = false)]
		public virtual Decimal? CuryUnbilledOrderTotal
		{
			get;
			set;
		}
		#endregion
		#region UnbilledOrderTotal
		public abstract class unbilledOrderTotal : PX.Data.BQL.BqlDecimal.Field<unbilledOrderTotal> { }
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? UnbilledOrderTotal
		{
			get;
			set;
		}
		#endregion
		#region CuryUnbilledLineTotal
		public abstract class curyUnbilledLineTotal : PX.Data.BQL.BqlDecimal.Field<curyUnbilledLineTotal> { }
		[PXDBCurrency(typeof(POOrder.curyInfoID), typeof(POOrder.unbilledLineTotal))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Unbilled Line Total", Enabled = false)]
		public virtual Decimal? CuryUnbilledLineTotal
		{
			get;
			set;
		}
		#endregion
		#region UnbilledLineTotal
		public abstract class unbilledLineTotal : PX.Data.BQL.BqlDecimal.Field<unbilledLineTotal> { }
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? UnbilledLineTotal
		{
			get;
			set;
		}
		#endregion
		#region CuryUnbilledTaxTotal
		public abstract class curyUnbilledTaxTotal : PX.Data.BQL.BqlDecimal.Field<curyUnbilledTaxTotal> { }
		[PXDBCurrency(typeof(POOrder.curyInfoID), typeof(POOrder.unbilledTaxTotal))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Unbilled Tax Total", Enabled = false)]
		public virtual Decimal? CuryUnbilledTaxTotal
		{
			get;
			set;
		}
		#endregion
		#region UnbilledTaxTotal
		public abstract class unbilledTaxTotal : PX.Data.BQL.BqlDecimal.Field<unbilledTaxTotal> { }
		protected Decimal? _OpenTaxTotal;
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? UnbilledTaxTotal
		{
			get;
			set;
		}
		#endregion
		#region UnbilledOrderQty
		public abstract class unbilledOrderQty : PX.Data.BQL.BqlDecimal.Field<unbilledOrderQty> { }
		[PXDBQuantity()]
		[PXUIField(DisplayName = "Unbilled Quantity")]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? UnbilledOrderQty
		{
			get;
			set;
		}
		#endregion

		#region EmployeeID
		[Obsolete]
		public abstract class employeeID : PX.Data.BQL.BqlInt.Field<employeeID> { }
		protected Int32? _EmployeeID;
		[PXInt()]
		[PXDefault(typeof(Search<EPEmployee.bAccountID, Where<EPEmployee.userID, Equal<Current<AccessInfo.userID>>>>), PersistingCheck=PXPersistingCheck.Nothing)]
		[PXFormula(typeof(Selector<ownerID, PX.TM.OwnerAttribute.Owner.employeeBAccountID>))]
		[PXSubordinateSelector]
		[PXUIField(DisplayName = "Owner", Visibility = PXUIVisibility.SelectorVisible, Visible = false)]
		[Obsolete]
		public virtual Int32? EmployeeID
		{
			get
			{
				return this._EmployeeID;
			}
			set
			{
				this._EmployeeID = value;
			}
		}
		#endregion
		#region OwnerWorkgroupID
		public abstract class ownerWorkgroupID : PX.Data.BQL.BqlInt.Field<ownerWorkgroupID> { }
		protected int? _OwnerWorkgroupID;
		[PXDBInt]
		[PXSelector(typeof(Search5<EPCompanyTree.workGroupID,
			InnerJoin<EPCompanyTreeMember, On<EPCompanyTreeMember.workGroupID, Equal<EPCompanyTree.workGroupID>>,
			InnerJoin<EPEmployee, On<EPCompanyTreeMember.contactID, Equal<EPEmployee.defContactID>>>>,
			Where<EPEmployee.defContactID, Equal<Current<POOrder.ownerID>>>,
			Aggregate<GroupBy<EPCompanyTree.workGroupID, GroupBy<EPCompanyTree.description>>>>), 
			SubstituteKey = typeof(EPCompanyTree.description))]
		[PXUIField(DisplayName = "Workgroup ID", Enabled = false)]
		public virtual int? OwnerWorkgroupID
		{
			get
			{
				return this._OwnerWorkgroupID;
			}
			set
			{
				this._OwnerWorkgroupID = value;
			}
		}
		#endregion
		#region WorkgroupID
		public abstract class workgroupID : PX.Data.BQL.BqlInt.Field<workgroupID> { }
		protected int? _WorkgroupID;
		[PXInt]		
		[PXSelector(typeof(Search<EPCompanyTree.workGroupID>), SubstituteKey = typeof(EPCompanyTree.description))]
		[PXUIField(DisplayName = "Approval Workgroup ID", Enabled=false)]
		public virtual int? WorkgroupID
		{
			get
			{
				return this._WorkgroupID;
			}
			set
			{
				this._WorkgroupID = value;
			}
		}
		#endregion
		#region OwnerID
		public abstract class ownerID : PX.Data.BQL.BqlInt.Field<ownerID> { }
		protected int? _OwnerID;
		[PXDefault(typeof(AccessInfo.contactID), PersistingCheck = PXPersistingCheck.Nothing)]
		[Owner(typeof(ownerWorkgroupID), DisplayName = "Owner", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual int? OwnerID
		{
			get
			{
				return this._OwnerID;
			}
			set
			{
				this._OwnerID = value;
			}
		}
		#endregion
		#region DontPrint
		public abstract class dontPrint : PX.Data.BQL.BqlBool.Field<dontPrint> { }
		protected Boolean? _DontPrint;
		[PXDBBool()]
		[PXDefault(true)]
		[PXUIField(DisplayName = "Do Not Print")]
		public virtual Boolean? DontPrint
		{
			get
			{
				return this._DontPrint;
			}
			set
			{
				this._DontPrint = value;
			}
		}
		#endregion
		#region Printed
		public abstract class printed : PX.Data.BQL.BqlBool.Field<printed> { }
		protected Boolean? _Printed;
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Printed")]
		public virtual Boolean? Printed
		{
			get
			{
				return this._Printed;
			}
			set
			{
				this._Printed = value;
			}
		}
		#endregion
		#region DontEmail
		public abstract class dontEmail : PX.Data.BQL.BqlBool.Field<dontEmail> { }
		protected Boolean? _DontEmail;
		[PXDBBool()]
		[PXDefault(true)]
		[PXUIField(DisplayName = "Do Not Email")]
		public virtual Boolean? DontEmail
		{
			get
			{
				return this._DontEmail;
			}
			set
			{
				this._DontEmail = value;
			}
		}
		#endregion
		#region Emailed
		public abstract class emailed : PX.Data.BQL.BqlBool.Field<emailed> { }
		protected Boolean? _Emailed;
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Emailed")]
		public virtual Boolean? Emailed
		{
			get
			{
				return this._Emailed;
			}
			set
			{
				this._Emailed = value;
			}
		}
		#endregion				
		#region PrintedExt
		public abstract class printedExt : PX.Data.BQL.BqlBool.Field<printedExt> { }
		[PXBool()]
		public virtual Boolean? PrintedExt
		{
			[PXDependsOnFields(typeof(dontPrint),typeof(printed))]
			get
			{
				return this._DontPrint == true || this._Printed == true;
			}
		}
		#endregion
		#region EmailedExt
		public abstract class emailedExt : PX.Data.BQL.BqlBool.Field<emailedExt> { }
		[PXBool()]
		public virtual Boolean? EmailedExt
		{
			[PXDependsOnFields(typeof(dontEmail),typeof(emailed))]
			get
			{
				return this._DontEmail == true || this._Emailed == true;
			}
		}
		#endregion
		#region FOBPoint
		public abstract class fOBPoint : PX.Data.BQL.BqlString.Field<fOBPoint> { }
		protected String _FOBPoint;
		[PXDBString(15, IsUnicode = true)]
		[PXUIField(DisplayName = "FOB Point")]
		[PXSelector(typeof(Search<FOBPoint.fOBPointID>), DescriptionField = typeof(FOBPoint.description), CacheGlobal = true)]
		[PXDefault(typeof(Search<Location.vFOBPointID, 
		             Where<Location.bAccountID, Equal<Current<POOrder.vendorID>>,
							And<Location.locationID, Equal<Current<POOrder.vendorLocationID>>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual String FOBPoint
		{
			get
			{
				return this._FOBPoint;
			}
			set
			{
				this._FOBPoint = value;
			}
		}
			#endregion
		#region ShipVia
		public abstract class shipVia : PX.Data.BQL.BqlString.Field<shipVia> { }
		protected String _ShipVia;
		[PXDBString(15, IsUnicode = true)]
		[PXUIField(DisplayName = "Ship Via")]
		[PXSelector(typeof(Search<Carrier.carrierID>), CacheGlobal = true)]
		[PXDefault(typeof(Search<Location.vCarrierID, 
							Where<Location.bAccountID, Equal<Current<POOrder.vendorID>>, 
				            And<Location.locationID, Equal<Current<POOrder.vendorLocationID>>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual String ShipVia
		{
			get
			{
				return this._ShipVia;
			}
			set
			{
				this._ShipVia = value;
			}
		}
		#endregion

		#region PayToVendorID
		public abstract class payToVendorID : PX.Data.BQL.BqlInt.Field<payToVendorID> { }
		/// <summary>
		/// A reference to the <see cref="Vendor"/>.
		/// </summary>
		/// <value>
		/// An integer identifier of the vendor, whom the AP bill will belong to. 
		/// </value>
		[PXFormula(typeof(Validate<POOrder.curyID>))]
		[POOrderPayToVendor(CacheGlobal = true, Filterable = true)]
		[PXDefault]
		[PXForeignReference(typeof(FK.PayToVendor))]
		public virtual int? PayToVendorID { get; set; }
		#endregion
		#region PrepaymentPct
		public abstract class prepaymentPct : Data.BQL.BqlDecimal.Field<prepaymentPct>
		{
		}
		[PXDBDecimal(6, MinValue = 0, MaxValue = 100)]
		[PXUIField(DisplayName = "Prepayment Percent")]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual decimal? PrepaymentPct
		{
			get;
			set;
		}
		#endregion

		#region OrderWeight
		public abstract class orderWeight : PX.Data.BQL.BqlDecimal.Field<orderWeight> { }
		protected Decimal? _OrderWeight;
		[PXDBDecimal(6)]
		[PXUIField(DisplayName = "Weight", Visible = false)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? OrderWeight
		{
			get
			{
				return this._OrderWeight;
			}
			set
			{
				this._OrderWeight = value;
			}
		}
		#endregion
		#region OrderVolume
		public abstract class orderVolume : PX.Data.BQL.BqlDecimal.Field<orderVolume> { }
		protected Decimal? _OrderVolume;
		[PXDBDecimal(6)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Volume", Visible=false)]
		public virtual Decimal? OrderVolume
		{
			get
			{
				return this._OrderVolume;
			}
			set
			{
				this._OrderVolume = value;
			}
		}
		#endregion

		#region LockCommitment
		public abstract class lockCommitment : PX.Data.BQL.BqlBool.Field<lockCommitment> { }
		[PXDBBool()]
		[PXDefault(false)]
		public virtual Boolean? LockCommitment
		{
			get;
			set;
		}
		#endregion

		#region RetainageApply
		public abstract class retainageApply : PX.Data.BQL.BqlBool.Field<retainageApply> { }
		[PXDBBool]
		[PXUIField(DisplayName = "Apply Retainage", FieldClass = nameof(FeaturesSet.Retainage))]
		[PXFormula(typeof(Switch<
			Case<Where<Current<POOrder.orderType>, Equal<POOrderType.dropShip>>, boolFalse>,
			Selector<POOrder.vendorID, Vendor.retainageApply>>))]
		public virtual bool? RetainageApply
		{
			get;
			set;
		}
		#endregion
		#region DefRetainagePct
		public abstract class defRetainagePct : PX.Data.BQL.BqlDecimal.Field<defRetainagePct> { }
		[PXDBDecimal(6, MinValue = 0, MaxValue = 100)]
		[PXUIField(DisplayName = "Retainage Percent", FieldClass = nameof(FeaturesSet.Retainage))]
		[PXFormula(typeof(Selector<POOrder.vendorID, Vendor.retainagePct>))]
		public virtual decimal? DefRetainagePct
		{
			get;
			set;
		}
		#endregion
		#region CuryLineRetainageTotal
		public abstract class curyLineRetainageTotal : PX.Data.BQL.BqlDecimal.Field<curyLineRetainageTotal> { }
		[PXDBCurrency(typeof(POOrder.curyInfoID), typeof(POOrder.lineRetainageTotal))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? CuryLineRetainageTotal
		{
			get;
			set;
		}
		#endregion
		#region LineRetainageTotal
		public abstract class lineRetainageTotal : PX.Data.BQL.BqlDecimal.Field<lineRetainageTotal> { }
		[PXDBBaseCury]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? LineRetainageTotal
		{
			get;
			set;
		}
		#endregion
		#region CuryRetainedTaxTotal
		public abstract class curyRetainedTaxTotal : PX.Data.BQL.BqlDecimal.Field<curyRetainedTaxTotal> { }
		[PXDBCurrency(typeof(POOrder.curyInfoID), typeof(POOrder.retainedTaxTotal))]
		[PXUIField(DisplayName = "Tax on Retainage", FieldClass = nameof(FeaturesSet.Retainage))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? CuryRetainedTaxTotal
		{
			get;
			set;
		}
		#endregion
		#region RetainedTaxTotal
		public abstract class retainedTaxTotal : PX.Data.BQL.BqlDecimal.Field<retainedTaxTotal> { }
		[PXDBBaseCury]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? RetainedTaxTotal
		{
			get;
			set;
		}
		#endregion
		#region CuryRetainedDiscTotal
		public abstract class curyRetainedDiscTotal : PX.Data.BQL.BqlDecimal.Field<curyRetainedDiscTotal> { }
		[PXDBCurrency(typeof(POOrder.curyInfoID), typeof(POOrder.retainedDiscTotal))]
		[PXUIField(DisplayName = "Discount on Retainage", FieldClass = nameof(FeaturesSet.Retainage))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? CuryRetainedDiscTotal
		{
			get;
			set;
		}
		#endregion
		#region RetainedDiscTotal
		public abstract class retainedDiscTotal : PX.Data.BQL.BqlDecimal.Field<retainedDiscTotal> { }
		[PXDBBaseCury]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? RetainedDiscTotal
		{
			get;
			set;
		}
		#endregion

		#region CuryRetainageTotal
		public abstract class curyRetainageTotal : PX.Data.BQL.BqlDecimal.Field<curyRetainageTotal> { }
		[PXDBCurrency(typeof(POOrder.curyInfoID), typeof(POOrder.retainageTotal))]
		[PXUIField(DisplayName = "Retainage Total", FieldClass = nameof(FeaturesSet.Retainage))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? CuryRetainageTotal
		{
			get;
			set;
		}
		#endregion
		#region RetainageTotal
		public abstract class retainageTotal : PX.Data.BQL.BqlDecimal.Field<retainageTotal> { }
		[PXDBBaseCury]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? RetainageTotal
		{
			get;
			set;
		}
		#endregion

		#region CuryPrepaidTotal
		public abstract class curyPrepaidTotal : Data.BQL.BqlDecimal.Field<curyPrepaidTotal>
		{
		}
		[PXDBCurrency(typeof(curyInfoID), typeof(prepaidTotal))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Unbilled Prepayment Total", Enabled = false)]
		public virtual decimal? CuryPrepaidTotal
		{
			get;
			set;
		}
		#endregion
		#region PrepaidTotal
		public abstract class prepaidTotal : Data.BQL.BqlDecimal.Field<prepaidTotal>
		{
		}
		[PXDBBaseCury]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? PrepaidTotal
		{
			get;
			set;
		}
		#endregion
		#region CuryUnprepaidTotal
		public abstract class curyUnprepaidTotal : Data.BQL.BqlDecimal.Field<curyUnprepaidTotal>
		{
		}
		[PXDBCurrency(typeof(curyInfoID), typeof(unprepaidTotal))]
		[PXFormula(typeof(Maximum<decimal0, Sub<curyUnbilledOrderTotal, curyPrepaidTotal>>))]
		[PXUIField(DisplayName = "Unpaid Amount", Enabled = false)]
		public virtual decimal? CuryUnprepaidTotal
		{
			get;
			set;
		}
		#endregion
		#region UnprepaidTotal
		public abstract class unprepaidTotal : Data.BQL.BqlDecimal.Field<unprepaidTotal>
		{
		}
		[PXDBBaseCury]
		public virtual decimal? UnprepaidTotal
		{
			get;
			set;
		}
		#endregion

		#region ProjectID
		public abstract class projectID : PX.Data.BQL.BqlInt.Field<projectID> { }
		[ProjectDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		[PXRestrictor(typeof(Where<PMProject.isCancelled, Equal<False>>), PM.Messages.CancelledContract, typeof(PMProject.contractCD))]
		[PXRestrictor(typeof(Where<PMProject.visibleInPO, Equal<True>, Or<PMProject.nonProject, Equal<True>>>), PM.Messages.ProjectInvisibleInModule, typeof(PMProject.contractCD))]
		[PXRestrictor(typeof(Where<Current<PMSetup.visibleInPO>, Equal<True>, Or<PMProject.nonProject, Equal<True>>>), PM.Messages.ProjectInvisibleInModule, typeof(PMProject.contractCD))]
		[PXRestrictor(typeof(Where<Current<POOrder.orderType>, Equal<POOrderType.projectDropShip>, And<PMProject.nonProject, NotEqual<True>, Or<Current<POOrder.orderType>, NotEqual<POOrderType.projectDropShip>>>>), PM.Messages.NonProjectCodeIsInvalid)]
		[ProjectBase]
		public virtual int? ProjectID
		{
			get;
			set;
		}
		#endregion

		#region DropshipReceiptProcessing
		public abstract class dropshipReceiptProcessing : PX.Data.BQL.BqlString.Field<dropshipReceiptProcessing> { }
		[PXDBString(1)]
		[PXFormula(typeof(Switch<Case<Where<POOrder.orderType, NotEqual<POOrderType.projectDropShip>>, Null>,
			Selector<POOrder.projectID, PMProject.dropshipReceiptProcessing>>))]
		[PXUIField(DisplayName = "Drop-Ship Receipt Processing", Enabled = false)]
		public virtual String DropshipReceiptProcessing
		{
			get;
			set;
		}
		#endregion
		#region DropshipExpenseRecording
		public abstract class dropshipExpenseRecording : PX.Data.BQL.BqlString.Field<dropshipExpenseRecording> { }
		[PXDBString(1)]
		[PXFormula(typeof(Switch<Case<Where<POOrder.orderType, NotEqual<POOrderType.projectDropShip>>, Null>,
			Selector<POOrder.projectID, PMProject.dropshipExpenseRecording>>))]
		[PXUIField(DisplayName = "Record Drop-Ship Expenses", Enabled = false)]
		public virtual String DropshipExpenseRecording
		{
			get;
			set;
		}
		#endregion

		#region UpdateVendorCost
		public abstract class updateVendorCost : PX.Data.BQL.BqlBool.Field<updateVendorCost> { }
		[PXBool]
		[PXFormula(typeof(boolTrue))]
		public virtual Boolean? UpdateVendorCost
		{
			get;
			set;
		}
		#endregion
		
		#region DisableAutomaticDiscountCalculation
		public abstract class disableAutomaticDiscountCalculation : PX.Data.BQL.BqlBool.Field<disableAutomaticDiscountCalculation> { }
		protected Boolean? _DisableAutomaticDiscountCalculation;
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Disable Automatic Discount Update")]
		public virtual Boolean? DisableAutomaticDiscountCalculation
		{
			get { return this._DisableAutomaticDiscountCalculation; }
			set { this._DisableAutomaticDiscountCalculation = value; }
		}
		#endregion

		#region OrderBasedAPBill
		public abstract class orderBasedAPBill : PX.Data.BQL.BqlBool.Field<orderBasedAPBill> { }
		[PXDBBool]
		[PXFormula(typeof(Selector<POOrder.vendorLocationID, Location.vAllowAPBillBeforeReceipt>))]
		[PXUIField(DisplayName = "Allow AP Bill Before Receipt", Enabled = false)]
		public virtual bool? OrderBasedAPBill
		{
			get;
			set;
		}
		#endregion
		#region POAccrualType
		public abstract class pOAccrualType : PX.Data.BQL.BqlString.Field<pOAccrualType> { }
		[PXString(1, IsFixed = true)]
		[PXFormula(typeof(Switch<Case<Where<POOrder.orderBasedAPBill, Equal<True>, 
			Or<Where<POOrder.orderType, Equal<POOrderType.projectDropShip>, 
				And<POOrder.dropshipReceiptProcessing, Equal<DropshipReceiptProcessingOption.skipReceipt>>>>>, POAccrualType.order>, 
			POAccrualType.receipt>))]
		[POAccrualType.List]
		[PXUIField(DisplayName = "Billing Based On", Enabled = false)]
		public virtual string POAccrualType
		{
			get;
			set;
		}
		#endregion
		#region LinesStatusUpdated
		public abstract class linesStatusUpdated : PX.Data.BQL.BqlBool.Field<linesStatusUpdated> { }
		[PXBool()]
		public virtual bool? LinesStatusUpdated
		{
			get;
			set;
		}
		#endregion
		#region HasUsedLine
		public abstract class hasUsedLine : PX.Data.BQL.BqlBool.Field<hasUsedLine> { }
		[PXBool()]
		public virtual bool? HasUsedLine
		{
			get;
			set;
		}
		#endregion

		#region IsIntercompany
		public abstract class isIntercompany : Data.BQL.BqlBool.Field<isIntercompany> { }
		[PXDBBool]
		[PXFormula(typeof(Where<orderType, Equal<POOrderType.regularOrder>,
			And<Selector<vendorID, Vendor.isBranch>, Equal<True>>>))]
		[PXDefault]
		public virtual bool? IsIntercompany
		{
			get;
			set;
		}
		#endregion
		#region IsIntercompanySOCreated
		public abstract class isIntercompanySOCreated : Data.BQL.BqlBool.Field<isIntercompanySOCreated> { }
		[PXDBBool]
		[PXDefault(false)]
		public virtual bool? IsIntercompanySOCreated
		{
			get;
			set;
		}
		#endregion
		#region IntercompanySOType
		public abstract class intercompanySOType : Data.BQL.BqlString.Field<intercompanySOType>
		{
		}
		[PXString(2, IsFixed = true)]
		[PXUIField(DisplayName = "Related Order Type", Enabled = false, FieldClass = nameof(FeaturesSet.InterBranch))]
		[PXSelector(typeof(Search<SO.SOOrderType.orderType>), DescriptionField = typeof(SO.SOOrderType.descr))]
		public virtual string IntercompanySOType
		{
			get;
			set;
		}
		#endregion
		#region IntercompanySONbr
		public abstract class intercompanySONbr : Data.BQL.BqlString.Field<intercompanySONbr>
		{
		}
		[PXString(15, IsUnicode = true)]
		[PXUIField(DisplayName = "Related Order Nbr.", Enabled = false, FieldClass = nameof(FeaturesSet.InterBranch))]
		[PXSelector(typeof(Search<SO.SOOrder.orderNbr, Where<SO.SOOrder.orderType, Equal<Current<intercompanySOType>>>>))]
		public virtual string IntercompanySONbr
		{
			get;
			set;
		}
		#endregion
		#region IntercompanySOCancelled
		public abstract class intercompanySOCancelled : PX.Data.BQL.BqlBool.Field<intercompanySOCancelled> { }
		[PXBool()]
		public virtual Boolean? IntercompanySOCancelled
		{
			get;
			set;
		}
		#endregion
		#region IntercompanySOWithEmptyInventory
		public abstract class intercompanySOWithEmptyInventory : Data.BQL.BqlBool.Field<intercompanySOWithEmptyInventory> { }
		[PXBool]
		public virtual bool? IntercompanySOWithEmptyInventory
		{
			get;
			set;
		}
		#endregion
		#region ExcludeFromIntercompanyProc
		public abstract class excludeFromIntercompanyProc : Data.BQL.BqlBool.Field<excludeFromIntercompanyProc> { }
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Exclude from Intercompany Processing", FieldClass = nameof(FeaturesSet.InterBranch))]
		public virtual bool? ExcludeFromIntercompanyProc
		{
			get;
			set;
		}
		#endregion
		#region SpecialLineCntr
		public abstract class specialLineCntr : Data.BQL.BqlDecimal.Field<specialLineCntr> { }
		[PXDBInt]
		[PXDefault(0)]
		public virtual int? SpecialLineCntr
		{
			get;
			set;
		}
		#endregion

		#region IAssign Members

		int? PX.Data.EP.IAssign.WorkgroupID
		{
			get { return WorkgroupID; }
			set { WorkgroupID = value; }
		}



		#endregion		
	}

	public class POOrderType
	{
		public class ListAttribute : PXStringListAttribute
		{
			public ListAttribute() : base(
				new[]
				{
					Pair(RegularOrder, Messages.RegularOrder),
					Pair(DropShip, Messages.DropShip),
					Pair(ProjectDropShip, Messages.ProjectDropShip),
					Pair(Blanket, Messages.Blanket),
					Pair(StandardBlanket, Messages.StandardBlanket),
				}) {}

			internal bool TryGetValue(string label, out string value)
			{
				var index = Array.IndexOf(_AllowedLabels, label);
				if (index >= 0)
				{
					value = _AllowedValues[index];
					return true;
				}
				value = null;
				return false;
			}
		}

		public class PrintListAttribute : PXStringListAttribute
		{
			public PrintListAttribute() : base(
				new[]
				{
					Pair(Blanket, Messages.PrintBlanket),
					Pair(StandardBlanket, Messages.PrintStandardBlanket),
					Pair(RegularOrder, Messages.PrintRegularOrder),
					Pair(DropShip, Messages.PrintDropShip),
				}) {}
		}

		public class StatdardBlanketList : PXStringListAttribute
		{
			public StatdardBlanketList() : base(
				new[]
				{
					Pair(RegularOrder, Messages.RegularOrder),
					Pair(Blanket, Messages.Blanket),
					Pair(StandardBlanket, Messages.StandardBlanket),
				}) {}
		}

		public class StatdardAndRegularList : PXStringListAttribute
		{
			public StatdardAndRegularList() : base(
				new[]
				{
					Pair(StandardBlanket, Messages.StandardBlanket),
					Pair(RegularOrder, Messages.RegularOrder),
				}) {}
		}

		public class BlanketList : PXStringListAttribute
		{
			public BlanketList() : base(
				new[]
				{
					Pair(Blanket, Messages.Blanket),
					Pair(StandardBlanket, Messages.StandardBlanket),
				}) {}
		}

		/// <summary>
		/// Selector. Provides list of "Regular" Purchase Orders types.
		/// Include RegularOrder, DropShip.
		/// </summary>
		public class RegularDropShipListAttribute : PXStringListAttribute
		{
			public RegularDropShipListAttribute() : base(
				new[]
				{
					Pair(RegularOrder, Messages.RegularOrder),
					Pair(DropShip, Messages.DropShip),
					Pair(ProjectDropShip, Messages.ProjectDropShip)
				}) {}
		}

		/// <summary>
		/// Selector. Defines a list of Purchase Order types, which are allowed <br/>
		/// for use in the SO module: RegularOrder, Blanket, DropShip, Transfer.<br/>
		/// </summary>
		public class RBDListAttribute : PXStringListAttribute
		{
			public RBDListAttribute() : base(
				new[]
				{
					Pair(RegularOrder, Messages.RegularOrder),
					Pair(Blanket, Messages.Blanket),
					Pair(DropShip, Messages.DropShip),
				}) {}
		}

		public class RBDSListAttribute : PXStringListAttribute
		{
			public RBDSListAttribute() : base(
				new[]
				{
					Pair(RegularOrder, Messages.RegularOrder),
					Pair(Blanket, Messages.Blanket),
					Pair(DropShip, Messages.DropShip),
					Pair(RegularSubcontract, CN.Subcontracts.PM.Descriptor.Messages.Subcontract)
				})
			{ }
		}


		/// <summary>
		/// Selector. Defines a list of Purchase Order types, which are allowed <br/>
		/// for use in the RQ module: RegularOrder, DropShip, Blanket, Standard.<br/>
		/// </summary>
		public class RequisitionListAttribute : PXStringListAttribute
		{
			public RequisitionListAttribute() : base(
				new[]
				{
					Pair(RegularOrder, Messages.RegularOrder),
					Pair(DropShip, Messages.DropShip),
					Pair(Blanket, Messages.Blanket),
					Pair(StandardBlanket, Messages.StandardBlanket),
				})
			{ }
		}

		public class RPSListAttribute : PXStringListAttribute
		{
			public RPSListAttribute() : base(
				new[]
				{
					Pair(RegularOrder, Messages.RegularOrder_NPO),
					Pair(ProjectDropShip, Messages.ProjectDropShip),
					Pair(RegularSubcontract, CN.Subcontracts.PM.Descriptor.Messages.Subcontract)
				})
			{ }
		}

		public class RPListAttribute : PXStringListAttribute
		{
			public RPListAttribute() : base(
				new[]
				{
					Pair(RegularOrder, Messages.RegularOrder_NPO),
					Pair(ProjectDropShip, Messages.ProjectDropShip)
				})
			{ }
		}

		//public const string Transfer = "TR";
		public const string Blanket = "BL";
		public const string StandardBlanket = "SB";
		public const string RegularOrder = "RO";
		public const string RegularSubcontract = "RS";//Reserved for Construction
		public const string DropShip = "DP";
		public const string ProjectDropShip = "PD";

		public class blanket : PX.Data.BQL.BqlString.Constant<blanket>
		{
			public blanket() : base(Blanket) { }
		}

		public class standardBlanket : PX.Data.BQL.BqlString.Constant<standardBlanket>
		{
			public standardBlanket() : base(StandardBlanket) { }
		}

		public class regularOrder : PX.Data.BQL.BqlString.Constant<regularOrder>
		{
			public regularOrder() : base(RegularOrder) { }
		}

		//Reserved for Construction
		public class regularSubcontract : PX.Data.BQL.BqlString.Constant<regularSubcontract>
		{
			public regularSubcontract() : base(RegularSubcontract) { }
		}

		public class dropShip : PX.Data.BQL.BqlString.Constant<dropShip>
		{
			public dropShip() : base(DropShip) { }
		}

		public class projectDropShip : PX.Data.BQL.BqlString.Constant<projectDropShip>
		{
			public projectDropShip() : base(ProjectDropShip) { }
		}

		/*
		public class transfer : PX.Data.BQL.BqlString.Constant<transfer>
		{
			public transfer() : base(Transfer) { }
		}
        */
		public static bool IsUseBlanket(string orderType)
		{
			return orderType == RegularOrder || orderType == DropShip;
		}

		public static bool IsNormalType(string orderType)
		{
			return orderType == RegularOrder || orderType == RegularSubcontract;
		}
	}

	public class POOrderStatus
	{
		public class ListAttribute : PXStringListAttribute
		{
			public ListAttribute() : base(
				new[]
				{
					Pair(Hold, Messages.Hold),
					Pair(PendingApproval, EP.Messages.PendingApproval),
					Pair(Rejected, EP.Messages.Rejected),
					Pair(Open, Messages.Open),
					Pair(AwaitingLink, Messages.AwaitingLink),
					Pair(PendingPrint, Messages.PendingPrint),
					Pair(PendingEmail, Messages.PendingEmail),
					Pair(Cancelled, Messages.Cancelled),
					Pair(Completed, Messages.Completed),
					Pair(Closed, Messages.Closed),
					Pair(Printed, Messages.Printed),
				}) {}
		}

		public const string Initial = "_";
		public const string Hold = "H";
		public const string PendingApproval = "B";
		public const string Rejected = "V";
		public const string Open = "N";
		public const string AwaitingLink = "A";
		public const string PendingPrint = "D";
		public const string PendingEmail = "E";
		public const string Completed = "M";
		public const string Closed = "C";
		public const string Printed = "P";
		public const string Cancelled = "L";

		public class hold : PX.Data.BQL.BqlString.Constant<hold>
		{
			public hold() : base(Hold) { }
		}

		public class pendingApproval : PX.Data.BQL.BqlString.Constant<pendingApproval>
		{
			public pendingApproval() : base(PendingApproval) { }
		}

		public class rejected : PX.Data.BQL.BqlString.Constant<rejected>
		{
			public rejected() : base(Rejected) { }
		}

		public class open : PX.Data.BQL.BqlString.Constant<open>
		{
			public open() : base(Open) { }
		}

		public class awaitingLink : PX.Data.BQL.BqlString.Constant<awaitingLink>
		{
			public awaitingLink() : base(AwaitingLink) { }
		}

		public class completed : Data.BQL.BqlString.Constant<completed>
		{
			public completed() : base(Completed) { }
		}

		public class closed : PX.Data.BQL.BqlString.Constant<closed>
		{
			public closed() : base(Closed) { }
		}

		public class printed : PX.Data.BQL.BqlString.Constant<printed>
		{
			public printed() : base(Printed) { }
		}
		public class pendingPrint : PX.Data.BQL.BqlString.Constant<pendingPrint>
		{
			public pendingPrint() : base(PendingPrint) { }
		}
		public class pendingEmail : PX.Data.BQL.BqlString.Constant<pendingEmail>
		{
			public pendingEmail() : base(PendingEmail) { }
		}
		public class cancelled : PX.Data.BQL.BqlString.Constant<cancelled>
		{
			public cancelled() : base(Cancelled) { }
		}
	}

	public class POShippingDestination
	{
		public const string CompanyLocation = "L";
		public const string Customer = "C";
		public const string Vendor = "V";
		public const string Site = "S";
		public const string ProjectSite = "P";

		public class ListAttribute : PXStringListAttribute
		{
			public ListAttribute() : base(
				new[]
				{
					Pair(CompanyLocation, Messages.ShipDestCompanyLocation),
					Pair(Customer, Messages.ShipDestCustomer),
					Pair(Vendor, Messages.ShipDestVendor),
					Pair(Site, Messages.ShipDestSite),
				}) {}
		}

		public class company : PX.Data.BQL.BqlString.Constant<company>
		{
			public company() : base(CompanyLocation) { }
		}

		public class customer : PX.Data.BQL.BqlString.Constant<customer>
		{
			public customer() : base(Customer) { }
		}

		public class vendor : PX.Data.BQL.BqlString.Constant<vendor>
		{
			public vendor() : base(Vendor) { }
		}

		public class site : PX.Data.BQL.BqlString.Constant<site>
		{
			public site() : base(Site) { }
		}

		public class projectSite : PX.Data.BQL.BqlString.Constant<projectSite>
		{
			public projectSite() : base(ProjectSite) { }
		}
	}

	public class POBehavior
	{
		public const string Standard = "S";
		public const string ChangeOrder = "C";

		public class ListAttribute : PXStringListAttribute
		{
			public ListAttribute() : base(
				new[]
			{
					Pair(Standard, Messages.POBehavior_Standard),
					Pair(ChangeOrder, Messages.POBehavior_ChangeOrder)
			}
				)
			{ }
		}

		public class standard : PX.Data.BQL.BqlString.Constant<standard>
		{
			public standard() : base(Standard) { }
		}

		public class changeOrder : PX.Data.BQL.BqlString.Constant<changeOrder>
		{
			public changeOrder() : base(ChangeOrder) { }
		}
	}

	public class PO
	{
        /// <summary>
        /// Specialized selector for POOrder RefNbr.<br/>
        /// By default, defines the following set of columns for the selector:<br/>
        /// POOrder.orderType, orderNbr, orderDate,<br/>
		/// status,vendorID, vendorID_Vendor_acctName,<br/>
		/// vendorLocationID, curyID, curyOrderTotal,<br/>
		/// vendorRefNbr, sOOrderType, sOOrderNbr<br/>
		/// </summary>
        /// <example>
        /// [PO.RefNbr(typeof(Search2<POOrder.orderNbr,
		///   LeftJoin<Vendor, On<POOrder.vendorID, Equal<Vendor.bAccountID>,
		///	   And<Match<Vendor, Current<AccessInfo.userName>>>>>,
		///	  Where<POOrder.orderType, Equal<Optional<POOrder.orderType>>,
		///	   And<Where<POOrder.orderType, Equal<POOrderType.transfer>,
		///	   Or<Vendor.bAccountID, IsNotNull>>>>>), Filterable = true)]
        /// </example>
		public class RefNbrAttribute : PXSelectorAttribute
		{
            /// <summary>
            /// Default Ctor
            /// </summary>
            /// <param name="SearchType"> Must be IBqlSearch type, pointing to POOrder.refNbr</param>
			public RefNbrAttribute(Type SearchType)
				: base(SearchType,
				typeof(POOrder.orderType),
				typeof(POOrder.orderNbr),
                typeof(POOrder.vendorRefNbr),
                typeof(POOrder.orderDate),
				typeof(POOrder.status),
				typeof(POOrder.vendorID),
				typeof(POOrder.vendorID_Vendor_acctName),
				typeof(POOrder.vendorLocationID),
				typeof(POOrder.curyID),
				typeof(POOrder.curyOrderTotal),
				typeof(POOrder.sOOrderType),
				typeof(POOrder.sOOrderNbr))
			{                
			}
		}

        /// <summary>
        /// Specialized version of the AutoNumber attribute for POOrders<br/>
        /// It defines how the new numbers are generated for the PO Order. <br/>
        /// References POOrder.docType and POOrder.docDate fields of the document,<br/>
        /// and also define a link between  numbering ID's defined in PO Setup and POOrder types:<br/>
        /// namely POSetup.regularPONumberingID, POSetup.regularPONumberingID for POOrderType.RegularOrder, POOrderType.DropShip, and POOrderType.StandardBlanket,<br/>
        /// and POSetup.standardPONumberingID for POOrderType.Blanket and POOrderType.Transfer<br/>
        /// </summary>		
		public class NumberingAttribute : AutoNumberAttribute
		{
			public NumberingAttribute()
				: base(typeof(POOrder.orderType), typeof(POOrder.orderDate),
					new string[] { POOrderType.RegularOrder, POOrderType.DropShip, POOrderType.ProjectDropShip, POOrderType.StandardBlanket, POOrderType.Blanket },
					new Type[] { typeof(POSetup.regularPONumberingID), typeof(POSetup.regularPONumberingID), typeof(POSetup.regularPONumberingID), typeof(POSetup.regularPONumberingID), typeof(POSetup.standardPONumberingID) }) { ; }
		}
	}
}
