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
using PX.Objects.CM.Extensions;
using PX.Objects.IN;
using PX.Objects.PO;
using System;
using System.Collections;
using PX.Objects.Common;
using PX.Data.BQL.Fluent;
using PX.Data.BQL;
using PX.Objects.AP;
using PX.TM;
using System.Collections.Generic;

namespace PX.Objects.PM
{
	[GL.TableDashboardType]
	[Serializable]
	public class CommitmentInquiry : PXGraph<CommitmentInquiry>
	{
		[Obsolete]
		protected virtual void _(Events.CacheAttached<POLine.orderType> e) { }

		[PXBool]
		[PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
		protected virtual void _(Events.CacheAttached<PMCostCode.isProjectOverride> e)
		{
		}

		public PXFilter<ProjectBalanceFilter> Filter;
		public PXCancel<ProjectBalanceFilter> Cancel;

		[PXFilterable]
		[PXViewName(Messages.Commitments)]
		public
			SelectFrom<PMCommitment>.
			LeftJoin<POLineCommitment>.On<PMCommitment.commitmentID.IsEqual<POLineCommitment.commitmentID>>.
			LeftJoin<PMProject>.On<PMCommitment.projectID.IsEqual<PMProject.contractID>>.
			LeftJoin<POOrder>.On<POLineCommitment.orderNbr.IsEqual<POOrder.orderNbr>
				.And<POLineCommitment.orderType.IsEqual<POOrder.orderType>>>.
			Where<
				Brackets<PMCommitment.projectID.IsEqual<ProjectBalanceFilter.projectID.FromCurrent>.Or<ProjectBalanceFilter.projectID.FromCurrent.IsNull>>.
				And<PMCommitment.accountGroupID.IsEqual<ProjectBalanceFilter.accountGroupID.FromCurrent>.Or<ProjectBalanceFilter.accountGroupID.FromCurrent.IsNull>>.
				And<PMCommitment.projectTaskID.IsEqual<ProjectBalanceFilter.projectTaskID.FromCurrent>.Or<ProjectBalanceFilter.projectTaskID.FromCurrent.IsNull>>.
				And<PMCommitment.costCodeID.IsEqual<ProjectBalanceFilter.costCode.FromCurrent>.Or<ProjectBalanceFilter.costCode.FromCurrent.IsNull>>.
				And<PMCommitment.inventoryID.IsEqual<ProjectBalanceFilter.inventoryID.FromCurrent>.Or<ProjectBalanceFilter.inventoryID.FromCurrent.IsNull>>.
				And<PMProject.ownerID.IsEqual<ProjectBalanceFilter.ownerID.FromCurrent>.Or<ProjectBalanceFilter.ownerID.FromCurrent.IsNull>>.
				And<POLineCommitment.vendorID.IsEqual<ProjectBalanceFilter.vendorID.FromCurrent>.Or<ProjectBalanceFilter.vendorID.FromCurrent.IsNull>>.
				And<POLineCommitment.relatedDocumentType.IsEqual<ProjectBalanceFilter.relatedDocumentType.FromCurrent>.Or<ProjectBalanceFilter.relatedDocumentType.FromCurrent.IsEqual<PX.Objects.CN.Subcontracts.PM.Descriptor.RelatedDocumentType.all>>>
			>.
			View Items;

		public SelectFrom<PMCommitmentAlias>.
			LeftJoin<POLineCommitment>.On<PMCommitmentAlias.commitmentID.IsEqual<POLineCommitment.commitmentID>>.
			LeftJoin<PMProject>.On<PMCommitmentAlias.projectID.IsEqual<PMProject.contractID>>.
			LeftJoin<POOrder>.On<POLineCommitment.orderNbr.IsEqual<POOrder.orderNbr>
				.And<POLineCommitment.orderType.IsEqual<POOrder.orderType>>>.
			Where<
				Brackets<PMCommitmentAlias.projectID.IsEqual<ProjectBalanceFilter.projectID.FromCurrent>.Or<ProjectBalanceFilter.projectID.FromCurrent.IsNull>>.
				And<PMCommitmentAlias.accountGroupID.IsEqual<ProjectBalanceFilter.accountGroupID.FromCurrent>.Or<ProjectBalanceFilter.accountGroupID.FromCurrent.IsNull>>.
				And<PMCommitmentAlias.projectTaskID.IsEqual<ProjectBalanceFilter.projectTaskID.FromCurrent>.Or<ProjectBalanceFilter.projectTaskID.FromCurrent.IsNull>>.
				And<PMCommitmentAlias.costCodeID.IsEqual<ProjectBalanceFilter.costCode.FromCurrent>.Or<ProjectBalanceFilter.costCode.FromCurrent.IsNull>>.
				And<PMCommitmentAlias.inventoryID.IsEqual<ProjectBalanceFilter.inventoryID.FromCurrent>.Or<ProjectBalanceFilter.inventoryID.FromCurrent.IsNull>>.
				And<PMProject.ownerID.IsEqual<ProjectBalanceFilter.ownerID.FromCurrent>.Or<ProjectBalanceFilter.ownerID.FromCurrent.IsNull>>.
				And<POLineCommitment.vendorID.IsEqual<ProjectBalanceFilter.vendorID.FromCurrent>.Or<ProjectBalanceFilter.vendorID.FromCurrent.IsNull>>.
				And<POLineCommitment.relatedDocumentType.IsEqual<ProjectBalanceFilter.relatedDocumentType.FromCurrent>.Or<ProjectBalanceFilter.relatedDocumentType.FromCurrent.IsEqual<PX.Objects.CN.Subcontracts.PM.Descriptor.RelatedDocumentType.all>>>
			>.
			AggregateTo<
				Sum<PMCommitmentAlias.qty>,
				Sum<PMCommitmentAlias.amount>,
				Sum<PMCommitmentAlias.openQty>,
				Sum<PMCommitmentAlias.openAmount>,
				Sum<PMCommitmentAlias.receivedQty>,
				Sum<PMCommitmentAlias.invoicedQty>,
				Sum<PMCommitmentAlias.invoicedAmount>>
			.View Totals;

		[PXCopyPasteHiddenView]
		[PXHidden]
		public PXSelect<PMCostCode> dummyCostCode;

		public PXAction<ProjectBalanceFilter> createCommitment;
		[PXUIField(DisplayName = Messages.CreateCommitment)]
		[PXButton(Tooltip = Messages.CreateCommitment)]
		public virtual IEnumerable CreateCommitment(PXAdapter adapter)
		{
			ExternalCommitmentEntry graph = PXGraph.CreateInstance<ExternalCommitmentEntry>();
			throw new PXPopupRedirectException(graph, Messages.CommitmentEntry + " - " + Messages.CreateCommitment, true);
		}

		public PXAction<ProjectBalanceFilter> viewProject;
		[PXUIField(DisplayName = Messages.ViewProject, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton]
		public virtual IEnumerable ViewProject(PXAdapter adapter)
		{
			if (Items.Current != null)
			{
				var service = PXGraph.CreateInstance<PM.ProjectAccountingService>();
				service.NavigateToProjectScreen(Items.Current.ProjectID, PXRedirectHelper.WindowMode.NewWindow);
			}
			return adapter.Get();
		}

		public PXAction<ProjectBalanceFilter> viewExternalCommitment;
		[PXUIField(DisplayName = Messages.ViewExternalCommitment, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton]
		public virtual IEnumerable ViewExternalCommitment(PXAdapter adapter)
		{
			if (Items.Current != null)
			{
				var graph = CreateInstance<ExternalCommitmentEntry>();
				graph.Commitments.Current = Items.Current;
				throw new PXRedirectRequiredException(graph, true, Messages.ViewExternalCommitment) { Mode = PXBaseRedirectException.WindowMode.NewWindow };
			}
			return adapter.Get();
		}

		public PXAction<ProjectBalanceFilter> viewVendor;
		[PXUIField(DisplayName = AR.Messages.ViewVendor, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton]
		public virtual IEnumerable ViewVendor(PXAdapter adapter)
		{
			if (Items.Current != null)
			{
				VendorMaint graph = CreateInstance<VendorMaint>();

				var vendor = SelectFrom<VendorR>
					.InnerJoin<POLine>.On<POLine.vendorID.IsEqual<VendorR.bAccountID>>
					.Where<POLine.commitmentID.IsEqual<P.AsGuid>>
					.View
					.SelectSingleBound(this, null, Items.Current.CommitmentID)
					.TopFirst;

				graph.BAccount.Current = vendor;
				throw new PXRedirectRequiredException(graph, true, AR.Messages.ViewVendor)
				{
					Mode = PXBaseRedirectException.WindowMode.NewWindow
				};
			}
			return adapter.Get();
		}

		protected virtual void _(Events.RowSelected<PMCommitment> e)
		{
			PXUIFieldAttribute.SetVisible<PMProject.ownerID>(this.Caches<PMProject>(), null, false);
			PXUIFieldAttribute.SetVisible<POOrder.ownerID>(this.Caches<POOrder>(), null, false);

			PXUIFieldAttribute.SetDisplayName<PMProject.description>(this.Caches<PMProject>(), PM.Messages.PrjDescription);
		}

		#region Local Types
		[PXHidden]
		[Serializable]
		[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
		public class PMCommitmentAlias : PMCommitment
		{
			#region Qty
			public new abstract class qty : PX.Data.BQL.BqlDecimal.Field<qty> { }
			[PXDBQuantity(BqlField = typeof(PMCommitment.qty))]
			[PXUIField(DisplayName = "Revised Qty.")]
			public override decimal? Qty { get; set; }
			#endregion

			#region Amount
			public new abstract class amount : PX.Data.BQL.BqlDecimal.Field<amount> { }
			[PXDBBaseCury(BqlField = typeof(PMCommitment.amount))]
			[PXUIField(DisplayName = "Revised Amt.")]
			public override decimal? Amount { get; set; }
			#endregion

			#region OpenQty
			public new abstract class openQty : PX.Data.BQL.BqlDecimal.Field<openQty> { }
			[PXDBQuantity(BqlField = typeof(PMCommitment.openQty))]
			[PXUIField(DisplayName = "Open Qty.")]
			public override decimal? OpenQty { get; set; }
			#endregion

			#region OpenAmount
			public new abstract class openAmount : PX.Data.BQL.BqlDecimal.Field<openAmount> { }
			[PXDBBaseCury(BqlField = typeof(PMCommitment.openAmount))]
			[PXUIField(DisplayName = "Open Amt.")]
			public override decimal? OpenAmount { get; set; }
			#endregion

			#region ReceivedQty
			public new abstract class receivedQty : PX.Data.BQL.BqlDecimal.Field<receivedQty> { }
			[PXDBQuantity(BqlField = typeof(PMCommitment.receivedQty))]
			[PXUIField(DisplayName = "Received Qty.")]
			public override decimal? ReceivedQty { get; set; }
			#endregion

			#region InvoicedQty
			public new abstract class invoicedQty : PX.Data.BQL.BqlDecimal.Field<invoicedQty> { }
			[PXDBQuantity(BqlField = typeof(PMCommitment.invoicedQty))]
			[PXUIField(DisplayName = "Invoiced Qty.")]
			public override decimal? InvoicedQty { get; set; }
			#endregion

			#region InvoicedAmount
			public new abstract class invoicedAmount : PX.Data.BQL.BqlDecimal.Field<invoicedAmount> { }
			[PXDBBaseCury(BqlField = typeof(PMCommitment.invoicedAmount))]
			[PXUIField(DisplayName = "Invoiced Amt.")]
			public override decimal? InvoicedAmount { get; set; }
			#endregion

			public new abstract class projectID : PX.Data.BQL.BqlInt.Field<projectID> { }
			public new abstract class accountGroupID : PX.Data.BQL.BqlInt.Field<accountGroupID> { }
			public new abstract class projectTaskID : PX.Data.BQL.BqlInt.Field<projectTaskID> { }
			public new abstract class costCodeID : PX.Data.BQL.BqlInt.Field<costCodeID> { }
			public new abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }
			public new abstract class commitmentID : PX.Data.BQL.BqlGuid.Field<commitmentID> { }
			public new abstract class refNoteID : PX.Data.BQL.BqlGuid.Field<refNoteID> { }
		}

		[PXCacheName(Messages.Commitments)]
		[Serializable]
		[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
		public partial class ProjectBalanceFilter : IBqlTable
		{
			#region ProjectID
			public abstract class projectID : PX.Data.BQL.BqlInt.Field<projectID> { }
			protected Int32? _ProjectID;
			[Project(typeof(Where<PMProject.nonProject, Equal<False>, And<PMProject.baseType, Equal<CT.CTPRType.project>>>))]
			public virtual Int32? ProjectID
			{
				get
				{
					return this._ProjectID;
				}
				set
				{
					this._ProjectID = value;
				}
			}
			#endregion

			#region AccountGroupID
			public abstract class accountGroupID : PX.Data.BQL.BqlInt.Field<accountGroupID> { }
			protected Int32? _AccountGroupID;
			[AccountGroupAttribute()]
			public virtual Int32? AccountGroupID
			{
				get
				{
					return this._AccountGroupID;
				}
				set
				{
					this._AccountGroupID = value;
				}
			}
			#endregion
			#region ProjectTaskID
			public abstract class projectTaskID : PX.Data.BQL.BqlInt.Field<projectTaskID> { }
			protected Int32? _ProjectTaskID;
			[ProjectTask(typeof(ProjectBalanceFilter.projectID))]
			public virtual Int32? ProjectTaskID
			{
				get
				{
					return this._ProjectTaskID;
				}
				set
				{
					this._ProjectTaskID = value;
				}
			}
			#endregion
			#region InventoryID
			public abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }
			protected Int32? _InventoryID;
			[PXDBInt]
			[PXUIField(DisplayName = "Inventory ID", Visibility = PXUIVisibility.Visible)]
			[PMInventorySelector]
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
			#region CostCode
			public abstract class costCode : PX.Data.BQL.BqlInt.Field<costCode> { }
			[CostCode(Filterable = false, SkipVerification = true)]
			public virtual Int32? CostCode
			{
				get;
				set;
			}
			#endregion
			#region RelatedDocumentType
			public abstract class relatedDocumentType : PX.Data.BQL.BqlString.Field<relatedDocumentType> { }
			[PXString]
			[PXUIField(DisplayName = "Related Document Type")]
			[PXUnboundDefault(PX.Objects.CN.Subcontracts.PM.Descriptor.RelatedDocumentType.AllCommitmentsType)]
			[PX.Objects.CN.Subcontracts.PM.Descriptor.RelatedDocumentType.List]
			public String RelatedDocumentType
			{
				get;
				set;
			}
			#endregion
			#region Qty
			public abstract class qty : PX.Data.BQL.BqlDecimal.Field<qty> { }
			protected Decimal? _Qty;
			[PXDecimal]
			[PXUnboundDefault(TypeCode.Decimal, "0.0")]
			[PXUIField(DisplayName = "Revised Quantity")]
			public virtual Decimal? Qty
			{
				get
				{
					return this._Qty;
				}
				set
				{
					this._Qty = value;
				}
			}
			#endregion
			#region Amount
			public abstract class amount : PX.Data.BQL.BqlDecimal.Field<amount> { }
			protected Decimal? _Amount;
			[PXBaseCury]
			[PXUnboundDefault(TypeCode.Decimal, "0.0")]
			[PXUIField(DisplayName = "Revised Amount")]
			public virtual Decimal? Amount
			{
				get
				{
					return this._Amount;
				}
				set
				{
					this._Amount = value;
				}
			}
			#endregion
			#region OpenQty
			public abstract class openQty : PX.Data.BQL.BqlDecimal.Field<openQty> { }
			protected Decimal? _OpenQty;
			[PXDecimal]
			[PXUnboundDefault(TypeCode.Decimal, "0.0")]
			[PXUIField(DisplayName = "Open Quantity")]
			public virtual Decimal? OpenQty
			{
				get
				{
					return this._OpenQty;
				}
				set
				{
					this._OpenQty = value;
				}
			}
			#endregion
			#region OpenAmount
			public abstract class openAmount : PX.Data.BQL.BqlDecimal.Field<openAmount> { }
			protected Decimal? _OpenAmount;
			[PXBaseCury]
			[PXUnboundDefault(TypeCode.Decimal, "0.0")]
			[PXUIField(DisplayName = "Open Amount")]
			public virtual Decimal? OpenAmount
			{
				get
				{
					return this._OpenAmount;
				}
				set
				{
					this._OpenAmount = value;
				}
			}
			#endregion
			#region ReceivedQty
			public abstract class receivedQty : PX.Data.BQL.BqlDecimal.Field<receivedQty> { }
			protected Decimal? _ReceivedQty;
			[PXDBQuantity]
			[PXDefault(TypeCode.Decimal, "0.0")]
			[PXUIField(DisplayName = "Received Quantity")]
			public virtual Decimal? ReceivedQty
			{
				get
				{
					return this._ReceivedQty;
				}
				set
				{
					this._ReceivedQty = value;
				}
			}
			#endregion
			#region InvoicedQty
			public abstract class invoicedQty : PX.Data.BQL.BqlDecimal.Field<invoicedQty> { }
			protected Decimal? _InvoicedQty;
			[PXDBQuantity]
			[PXDefault(TypeCode.Decimal, "0.0")]
			[PXUIField(DisplayName = "Invoiced Quantity")]
			public virtual Decimal? InvoicedQty
			{
				get
				{
					return this._InvoicedQty;
				}
				set
				{
					this._InvoicedQty = value;
				}
			}
			#endregion
			#region InvoicedAmount
			public abstract class invoicedAmount : PX.Data.BQL.BqlDecimal.Field<invoicedAmount> { }
			protected Decimal? _InvoicedAmount;
			[PXDBBaseCury]
			[PXDefault(TypeCode.Decimal, "0.0")]
			[PXUIField(DisplayName = "Invoiced Amount")]
			public virtual Decimal? InvoicedAmount
			{
				get
				{
					return this._InvoicedAmount;
				}
				set
				{
					this._InvoicedAmount = value;
				}
			}
			#endregion
			#region VendorID
			public abstract class vendorID : BqlInt.Field<vendorID> { }
			[POVendor(Visibility = PXUIVisibility.SelectorVisible, DescriptionField = typeof(Vendor.acctName))]
			public virtual int? VendorID { get; set; }
			#endregion
			#region VendorID
			public abstract class ownerID : BqlInt.Field<ownerID> { }
			[Owner(DisplayName = "Project Manager")]
			public virtual int? OwnerID { get; set; }
			#endregion
		}

		[PXHidden]
		[Serializable]
		[PXProjection(typeof(
			SelectFrom<POLine>
				.InnerJoin<Vendor>
					.On<Vendor.bAccountID.IsEqual<POLine.vendorID>>),
			Persistent = false)]
		public partial class POLineCommitment : PX.Data.IBqlTable
		{
			#region OrderType
			public abstract class orderType : PX.Data.BQL.BqlString.Field<orderType> { }
			[PXDBString(2, IsKey = true, IsFixed = true, BqlField = typeof(POLine.orderType))]
			[PXDBDefault(typeof(POOrder.orderType))]
			[PXUIField(DisplayName = "Order Type", Visibility = PXUIVisibility.Visible, Visible = false)]
			public virtual String OrderType { get; set; }
			#endregion

			#region OrderNbr
			public abstract class orderNbr : PX.Data.BQL.BqlString.Field<orderNbr> { }
			protected String _OrderNbr;

			[PXDBString(15, IsUnicode = true, IsKey = true, InputMask = "", BqlField = typeof(POLine.orderType))]
			[PXUIField(DisplayName = "Order Nbr.", Visibility = PXUIVisibility.Invisible, Visible = false)]
			public virtual String OrderNbr { get; set; }
			#endregion

			#region LineNbr
			public abstract class lineNbr : PX.Data.BQL.BqlInt.Field<lineNbr> { }
			[PXDBInt(IsKey = true, BqlField = typeof(POLine.lineNbr))]
			[PXUIField(DisplayName = "Line Nbr.", Visibility = PXUIVisibility.Visible, Visible = false)]
			public virtual Int32? LineNbr { get; set; }
			#endregion

			#region CommitmentID
			public abstract class commitmentID : PX.Data.BQL.BqlGuid.Field<commitmentID> { }
			[PXDBGuid(BqlField = typeof(POLine.commitmentID))]
			public virtual Guid? CommitmentID { get; set; }
			#endregion

			#region RelatedDocumentType
			public abstract class relatedDocumentType : PX.Data.BQL.BqlString.Field<relatedDocumentType> { }
			[PXString]
			[PX.Objects.CN.Subcontracts.PM.Descriptor.RelatedDocumentType.List]
			[PXDBCalced(typeof(
				Switch<
					Case<Where<POLine.orderType, In3<POOrderType.regularOrder, POOrderType.projectDropShip>>, PX.Objects.CN.Subcontracts.PM.Descriptor.RelatedDocumentType.purchaseOrder>,
					Case<Where<POLine.orderType, Equal<POOrderType.regularSubcontract>>, PX.Objects.CN.Subcontracts.PM.Descriptor.RelatedDocumentType.subcontract>>),
				typeof(string))]
			public virtual string RelatedDocumentType { get; set; }
			#endregion

			#region RelatedDocumentTypeExt
			public abstract class relatedDocumentTypeExt : PX.Data.BQL.BqlString.Field<relatedDocumentTypeExt> { }
			[PXString]
			[PXUIField(DisplayName = "Related Document Type", Visibility = PXUIVisibility.Visible, Visible = true)]
			[PXDBCalced(typeof(POLine.orderType), typeof(string))]
			[CN.Subcontracts.PM.Descriptor.RelatedDocumentType.DetailList]
			public virtual string RelatedDocumentTypeExt { get; set; }
			#endregion

			#region VendorID
			public abstract class vendorID : PX.Data.BQL.BqlInt.Field<vendorID> { }

			[POVendor(BqlField = typeof(POLine.vendorID))]
			public virtual int? VendorID { get; set; }
			#endregion

			#region VendorName
			public abstract class vendorName : PX.Data.BQL.BqlString.Field<vendorName> { }

			[PXDBString(BqlField = typeof(Vendor.acctName))]
			[PXUIField(DisplayName = PO.Messages.VendorAcctName, Visibility = PXUIVisibility.Visible, Visible = true)]
			public virtual string VendorName { get; set; }
			#endregion
		}

		[Obsolete("Use PX.Objects.CN.Subcontracts.PM.Descriptor.RelatedDocumentType")]
		public class RelatedDocumentType
		{
			public const string AllCommitmentsType = "ALL";
			public const string PurchaseOrderType = "RO";
			public const string SubcontractType = "RS";
			public const string ProjectDropShipType = "PD";

			public const string AllCommitmentsLabel = "All Commitments";
			public const string PurchaseOrderLabel = "Purchase Order";
			public const string SubcontractLabel = "Subcontract";
			public const string ProjectDropShipLabel = "Project Drop-Ship";

			private static readonly string[] RelatedDocumentTypes =
			{
				AllCommitmentsType,
				PurchaseOrderType,
			SubcontractType,
			ProjectDropShipType
			};

			private static readonly string[] RelatedDocumentLabels =
			{
				AllCommitmentsLabel,
				PurchaseOrderLabel,
			SubcontractLabel,
			ProjectDropShipLabel
			};

			public class ListAttribute : PXStringListAttribute
			{
				public ListAttribute()
					: base(RelatedDocumentTypes, RelatedDocumentLabels)
				{
				}
			}

			public class all : PX.Data.BQL.BqlString.Constant<all>
			{
				public all() : base(AllCommitmentsType) {; }
			}
		}
		#endregion
	}
}
