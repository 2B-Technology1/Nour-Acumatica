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
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.CS;
using PX.Objects.SO;
using PX.Objects.IN.PhysicalInventory;

namespace PX.Objects.IN.InventoryRelease.Accumulators.QtyAllocated
{
	using Abstraction;
	using PX.Data.SQLTree;
	using static PXDataFieldAssign.AssignBehavior;

	[PXHidden]
	[Accumulator(BqlTable = typeof(INLocationStatusByCostCenter))]
	public class LocationStatusByCostCenter : INLocationStatusByCostCenter, IQtyAllocated
	{
		#region Keys
		public new class PK : PrimaryKeyOf<LocationStatusByCostCenter>.By<inventoryID, subItemID, siteID, locationID, costCenterID>
		{
			public static LocationStatusByCostCenter Find(PXGraph graph, int? inventoryID, int? subItemID, int? siteID, int? locationID, int? costCenterID, PKFindOptions options = PKFindOptions.None)
				=> FindBy(graph, inventoryID, subItemID, siteID, locationID, costCenterID, options);
		}
		#endregion
		#region InventoryID
		[PXDBInt(IsKey = true)]
		[PXForeignSelector(typeof(INTran.inventoryID))]
		[PXSelectorMarker(typeof(SearchFor<InventoryItem.inventoryID>.Where<InventoryItem.inventoryID.IsEqual<inventoryID.FromCurrent>>), CacheGlobal = true)]
		[PXDefault]
		public override int? InventoryID
		{
			get;
			set;
		}
		public new abstract class inventoryID : BqlInt.Field<inventoryID> { }
		#endregion
		#region SubItemID
		public new abstract class subItemID : BqlInt.Field<subItemID> { }
		#endregion
		#region SiteID
		public new abstract class siteID : BqlInt.Field<siteID> { }
		[Site(IsKey = true)]
		[PXRestrictor(typeof(Where<True, Equal<True>>), "", ReplaceInherited = true)]
		[PXDefault]
		public override int? SiteID
		{
			get => base.SiteID;
			set => base.SiteID = value;
		}
		#endregion
		#region LocationID
		public new abstract class locationID : BqlInt.Field<locationID> { }
		[Location(typeof(siteID), IsKey = true)]
		[PXDefault]
		[PXForeignReference(typeof(FK.Location))]
		public override int? LocationID
		{
			get => base.LocationID;
			set => base.LocationID = value;
		}
		#endregion
		#region CostCenterID
		public new abstract class costCenterID : BqlInt.Field<costCenterID> { }
		[PXDBInt(IsKey = true)]
		[PXDefault]
		public override int? CostCenterID
		{
			get => base.CostCenterID;
			set => base.CostCenterID = value;
		}
		#endregion

		#region ItemClassID
		[PXInt]
		[PXFormula(typeof(InventoryItem.itemClassID.FromSelectorOf<inventoryID>))]
		[PXSelectorMarker(typeof(SearchFor<INItemClass.itemClassID>.Where<INItemClass.itemClassID.IsEqual<itemClassID.FromCurrent>>), CacheGlobal = true)]
		public virtual int? ItemClassID
		{
			get;
			set;
		}
		public abstract class itemClassID : BqlInt.Field<itemClassID> { }
		#endregion
		#region QtyOnHand
		public new abstract class qtyOnHand : BqlDecimal.Field<qtyOnHand> { }
		#endregion
		#region NegQty
		[PXBool]
		[PXUnboundDefault(typeof(Selector<itemClassID, INItemClass.negQty>))]
		public virtual bool? NegQty
		{
			get;
			set;
		}
		public abstract class negQty : BqlBool.Field<negQty> { }
		#endregion
		#region InclQtyAvail
		[PXBool]
		[PXDefault(typeof(SelectFrom<INLocation>.Where<INLocation.locationID.IsEqual<locationID.FromCurrent>>),
			SourceField = typeof(INLocation.inclQtyAvail), CacheGlobal = true, PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual bool? InclQtyAvail
		{
			get;
			set;
		}
		public abstract class inclQtyAvail : BqlBool.Field<inclQtyAvail> { }
		#endregion

		#region AvailabilitySchemeID
		[PXString(10, IsUnicode = true)]
		[PXDefault(typeof(SelectFrom<INItemClass>.Where<INItemClass.itemClassID.IsEqual<itemClassID.FromCurrent>>),
			SourceField = typeof(INItemClass.availabilitySchemeID), CacheGlobal = true, PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual string AvailabilitySchemeID
		{
			get;
			set;
		}
		public abstract class availabilitySchemeID : BqlString.Field<availabilitySchemeID> { }
		#endregion

		#region InclQtyINIssues
		[AvailabilityFlag(typeof(availabilitySchemeID), typeof(INAvailabilityScheme.inclQtyINIssues))]
		public virtual bool? InclQtyINIssues
		{
			get;
			set;
		}
		public abstract class inclQtyINIssues : BqlBool.Field<inclQtyINIssues> { }
		#endregion
		#region InclQtyINReceipts
		[AvailabilityFlag(typeof(availabilitySchemeID), typeof(INAvailabilityScheme.inclQtyINReceipts))]
		public virtual bool? InclQtyINReceipts
		{
			get;
			set;
		}
		public abstract class inclQtyINReceipts : BqlBool.Field<inclQtyINReceipts> { }
		#endregion
		#region InclQtyINAssemblyDemand
		[AvailabilityFlag(typeof(availabilitySchemeID), typeof(INAvailabilityScheme.inclQtyINAssemblyDemand))]
		public virtual bool? InclQtyINAssemblyDemand
		{
			get;
			set;
		}
		public abstract class inclQtyINAssemblyDemand : BqlBool.Field<inclQtyINAssemblyDemand> { }
		#endregion
		#region InclQtyINAssemblySupply
		[AvailabilityFlag(typeof(availabilitySchemeID), typeof(INAvailabilityScheme.inclQtyINAssemblySupply))]
		public virtual bool? InclQtyINAssemblySupply
		{
			get;
			set;
		}
		public abstract class inclQtyINAssemblySupply : BqlBool.Field<inclQtyINAssemblySupply> { }
		#endregion
		#region InclQtyInTransit
		[AvailabilityFlag(typeof(availabilitySchemeID), typeof(INAvailabilityScheme.inclQtyInTransit))]
		public virtual bool? InclQtyInTransit
		{
			get;
			set;
		}
		public abstract class inclQtyInTransit : BqlBool.Field<inclQtyInTransit> { }
		#endregion

		#region InclQtySOReverse
		[AvailabilityFlag(typeof(availabilitySchemeID), typeof(INAvailabilityScheme.inclQtySOReverse))]
		public virtual bool? InclQtySOReverse
		{
			get;
			set;
		}
		public abstract class inclQtySOReverse : BqlBool.Field<inclQtySOReverse> { }
		#endregion
		#region InclQtySOBackOrdered
		[AvailabilityFlag(typeof(availabilitySchemeID), typeof(INAvailabilityScheme.inclQtySOBackOrdered))]
		public virtual bool? InclQtySOBackOrdered
		{
			get;
			set;
		}
		public abstract class inclQtySOBackOrdered : BqlBool.Field<inclQtySOBackOrdered> { }
		#endregion
		#region InclQtySOPrepared
		[AvailabilityFlag(typeof(availabilitySchemeID), typeof(INAvailabilityScheme.inclQtySOPrepared))]
		public virtual bool? InclQtySOPrepared
		{
			get;
			set;
		}
		public abstract class inclQtySOPrepared : BqlBool.Field<inclQtySOPrepared> { }
		#endregion
		#region InclQtySOBooked
		[AvailabilityFlag(typeof(availabilitySchemeID), typeof(INAvailabilityScheme.inclQtySOBooked))]
		public virtual bool? InclQtySOBooked
		{
			get;
			set;
		}
		public abstract class inclQtySOBooked : BqlBool.Field<inclQtySOBooked> { }
		#endregion
		#region InclQtySOShipped
		[AvailabilityFlag(typeof(availabilitySchemeID), typeof(INAvailabilityScheme.inclQtySOShipped))]
		public virtual bool? InclQtySOShipped
		{
			get;
			set;
		}
		public abstract class inclQtySOShipped : BqlBool.Field<inclQtySOShipped> { }
		#endregion
		#region InclQtySOShipping
		[AvailabilityFlag(typeof(availabilitySchemeID), typeof(INAvailabilityScheme.inclQtySOShipping))]
		public virtual bool? InclQtySOShipping
		{
			get;
			set;
		}
		public abstract class inclQtySOShipping : BqlBool.Field<inclQtySOShipping> { }
		#endregion

		#region InclQtyPOReceipts
		[AvailabilityFlag(typeof(availabilitySchemeID), typeof(INAvailabilityScheme.inclQtyPOReceipts))]
		public virtual bool? InclQtyPOReceipts
		{
			get;
			set;
		}
		public abstract class inclQtyPOReceipts : BqlBool.Field<inclQtyPOReceipts> { }
		#endregion
		#region InclQtyPOPrepared
		[AvailabilityFlag(typeof(availabilitySchemeID), typeof(INAvailabilityScheme.inclQtyPOPrepared))]
		public virtual bool? InclQtyPOPrepared
		{
			get;
			set;
		}
		public abstract class inclQtyPOPrepared : BqlBool.Field<inclQtyPOPrepared> { }
		#endregion
		#region InclQtyPOOrders
		[AvailabilityFlag(typeof(availabilitySchemeID), typeof(INAvailabilityScheme.inclQtyPOOrders))]
		public virtual bool? InclQtyPOOrders
		{
			get;
			set;
		}
		public abstract class inclQtyPOOrders : BqlBool.Field<inclQtyPOOrders> { }
		#endregion
		#region InclQtyFixedSOPO
		[AvailabilityFlag(typeof(availabilitySchemeID), typeof(INAvailabilityScheme.inclQtyFixedSOPO))]
		public virtual bool? InclQtyFixedSOPO
		{
			get;
			set;
		}
		public abstract class inclQtyFixedSOPO : BqlBool.Field<inclQtyFixedSOPO> { }
		#endregion
		#region InclQtyPOFixedReceipt
		[AvailabilityFlag(false)]
		public virtual bool? InclQtyPOFixedReceipt
		{
			get;
			set;
		}
		public abstract class inclQtyPOFixedReceipt : BqlBool.Field<inclQtyPOFixedReceipt> { }
		#endregion

		#region InclQtyProductionDemandPrepared
		[AvailabilityFlag(typeof(availabilitySchemeID), typeof(INAvailabilityScheme.inclQtyProductionDemandPrepared))]
		public virtual bool? InclQtyProductionDemandPrepared
		{
			get;
			set;
		}
		public abstract class inclQtyProductionDemandPrepared : BqlBool.Field<inclQtyProductionDemandPrepared> { }
		#endregion
		#region InclQtyProductionDemand
		[AvailabilityFlag(typeof(availabilitySchemeID), typeof(INAvailabilityScheme.inclQtyProductionDemand))]
		public virtual bool? InclQtyProductionDemand
		{
			get;
			set;
		}
		public abstract class inclQtyProductionDemand : BqlBool.Field<inclQtyProductionDemand> { }
		#endregion
		#region InclQtyProductionAllocated
		[AvailabilityFlag(typeof(availabilitySchemeID), typeof(INAvailabilityScheme.inclQtyProductionAllocated))]
		public virtual bool? InclQtyProductionAllocated
		{
			get;
			set;
		}
		public abstract class inclQtyProductionAllocated : BqlBool.Field<inclQtyProductionAllocated> { }
		#endregion
		#region InclQtyProductionSupplyPrepared
		[AvailabilityFlag(typeof(availabilitySchemeID), typeof(INAvailabilityScheme.inclQtyProductionSupplyPrepared))]
		public virtual bool? InclQtyProductionSupplyPrepared
		{
			get;
			set;
		}
		public abstract class inclQtyProductionSupplyPrepared : BqlBool.Field<inclQtyProductionSupplyPrepared> { }
		#endregion
		#region InclQtyProductionSupply
		[AvailabilityFlag(typeof(availabilitySchemeID), typeof(INAvailabilityScheme.inclQtyProductionSupply))]
		public virtual bool? InclQtyProductionSupply
		{
			get;
			set;
		}
		public abstract class inclQtyProductionSupply : BqlBool.Field<inclQtyProductionSupply> { }
		#endregion

		#region InclQtyFSSrvOrdPrepared
		[AvailabilityFlag(typeof(availabilitySchemeID), typeof(INAvailabilityScheme.inclQtyFSSrvOrdPrepared))]
		public virtual bool? InclQtyFSSrvOrdPrepared
		{
			get;
			set;
		}
		public abstract class inclQtyFSSrvOrdPrepared : BqlBool.Field<inclQtyFSSrvOrdPrepared> { }
		#endregion
		#region InclQtyFSSrvOrdBooked
		[AvailabilityFlag(typeof(availabilitySchemeID), typeof(INAvailabilityScheme.inclQtyFSSrvOrdBooked))]
		public virtual bool? InclQtyFSSrvOrdBooked
		{
			get;
			set;
		}
		public abstract class inclQtyFSSrvOrdBooked : BqlBool.Field<inclQtyFSSrvOrdBooked> { }
		#endregion
		#region InclQtyFSSrvOrdAllocated
		[AvailabilityFlag(typeof(availabilitySchemeID), typeof(INAvailabilityScheme.inclQtyFSSrvOrdAllocated))]
		public virtual bool? InclQtyFSSrvOrdAllocated
		{
			get;
			set;
		}
		public abstract class inclQtyFSSrvOrdAllocated : BqlBool.Field<inclQtyFSSrvOrdAllocated> { }
		#endregion

		#region RelatedPIID
		[PXString(IsUnicode = true)]
		public virtual string RelatedPIID
		{
			get;
			set;
		}
		public abstract class relatedPIID : BqlString.Field<relatedPIID> { }
		#endregion
		#region SkipQtyValidation
		[PXBool, PXUnboundDefault(false)]
		public virtual bool? SkipQtyValidation
		{
			get;
			set;
		}
		public abstract class skipQtyValidation : BqlBool.Field<skipQtyValidation> { }
		#endregion

		public class AccumulatorAttribute : StatusAccumulatorAttribute
		{
			private const string SiteIDToVerifyField = "SiteIDToVerify";
			public AccumulatorAttribute()
			{
				SingleRecord = true;
			}

			public override void CacheAttached(PXCache cache)
			{
				base.CacheAttached(cache);

				if (!cache.Fields.Contains(SiteIDToVerifyField))
				{
					cache.Fields.Add(SiteIDToVerifyField);
					cache.Graph.CommandPreparing.AddHandler(typeof(LocationStatusByCostCenter), SiteIDToVerifyField, SiteIDToVerifyCommandPreparing);
				}
			}

			private void SiteIDToVerifyCommandPreparing(PXCache cache, PXCommandPreparingEventArgs e)
			{
				if (e.Operation.Command() != PXDBOperation.Update)
					return;

				e.DataType = PXDbType.DirectExpression;
				e.BqlTable = _BqlTable;
				var table = e.Table == null ? _BqlTable : e.Table;
				e.Expr = new Column(nameof(SiteID), new SimpleTable(table), PXDbType.Int);
				e.IsRestriction = true;

				const string compId = "CompanyID";
				var query = new Query()
					.Select<INLocation.siteID>()
					.From<INLocation>()
					.Where(
						new Column<INLocation.locationID>().EQ(new SQLConst(e.Value))
						.And(new Column(compId, typeof(INLocation), PXDbType.Int).EQ(new SQLConst(Data.Update.PXInstanceHelper.CurrentCompany))));

				var sql = query.SQLQuery(cache.Graph.SqlDialect.GetConnection()).ToString();
				e.Value = e.DataValue = $"({sql})";
			}

			protected override bool PrepareInsert(PXCache cache, object row, PXAccumulatorCollection columns)
			{
				if (!base.PrepareInsert(cache, row, columns))
					return false;

				columns.Update<qtyOnHand>(Summarize);
				columns.Update<qtyAvail>(Summarize);
				columns.Update<qtyHardAvail>(Summarize);
				columns.Update<qtyActual>(Summarize);

				columns.Update<qtyINIssues>(Summarize);
				columns.Update<qtyINReceipts>(Summarize);
				columns.Update<qtyINAssemblyDemand>(Summarize);
				columns.Update<qtyINAssemblySupply>(Summarize);
				columns.Update<qtyInTransit>(Summarize);

				columns.Update<qtySOPrepared>(Summarize);
				columns.Update<qtySOBooked>(Summarize);
				columns.Update<qtySOShipped>(Summarize);
				columns.Update<qtySOShipping>(Summarize);

				columns.Update<qtyPOReceipts>(Summarize);
				columns.Update<qtyPOPrepared>(Summarize);
				columns.Update<qtyPOOrders>(Summarize);

				columns.Update<qtyInTransitToProduction>(Summarize);
				columns.Update<qtyProductionSupplyPrepared>(Summarize);
				columns.Update<qtyProductionSupply>(Summarize);
				columns.Update<qtyPOFixedProductionPrepared>(Summarize);
				columns.Update<qtyPOFixedProductionOrders>(Summarize);
				columns.Update<qtyProductionDemandPrepared>(Summarize);
				columns.Update<qtyProductionDemand>(Summarize);
				columns.Update<qtyProductionAllocated>(Summarize);
				columns.Update<qtySOFixedProduction>(Summarize);
				columns.Update<qtyProdFixedPurchase>(Summarize);
				columns.Update<qtyProdFixedProduction>(Summarize);
				columns.Update<qtyProdFixedProdOrdersPrepared>(Summarize);
				columns.Update<qtyProdFixedProdOrders>(Summarize);
				columns.Update<qtyProdFixedSalesOrdersPrepared>(Summarize);
				columns.Update<qtyProdFixedSalesOrders>(Summarize);

				columns.Update<qtyFSSrvOrdPrepared>(Summarize);
				columns.Update<qtyFSSrvOrdBooked>(Summarize);
				columns.Update<qtyFSSrvOrdAllocated>(Summarize);

				var diff = (LocationStatusByCostCenter)row;

				if (diff.QtyOnHand >= 0m && diff.QtyActual >= 0m)
					diff.NegQty = true;

				//only in release process updates onhand.
				if (diff.NegQty == false && diff.SkipQtyValidation != true)
				{
					if (diff.QtyOnHand < 0m)
						columns.Restrict<qtyOnHand>(PXComp.GE, -diff.QtyOnHand);

					if (diff.QtyActual < 0m)
						columns.Restrict<qtyActual>(PXComp.GE, -diff.QtyActual);
				}

				if (!_InternalCall &&
					(diff.QtyOnHand < 0m || diff.QtySOShipped < 0m ||
					diff.QtyOnHand > 0m || diff.QtySOShipped > 0m))
				{
					if (CreateLocksInspector(diff.SiteID.Value)
						.IsInventoryLocationLocked(diff.InventoryID, diff.LocationID, diff.RelatedPIID, out string PIID))
					{
						throw new PXException(Messages.InventoryItemIsLockedInPIDocument,
							PXForeignSelectorAttribute.GetValueExt<inventoryID>(cache, diff),
							PXForeignSelectorAttribute.GetValueExt<locationID>(cache, diff),
							PXForeignSelectorAttribute.GetValueExt<siteID>(cache, diff),
							PIID);
					}
				}

				if (!_InternalCall && diff.SiteID != new SiteAttribute.transitSiteID().Value)
				{
					columns.AppendException(Messages.InvalidLocationSiteID,
						new PXAccumulatorRestriction(SiteIDToVerifyField, PXComp.EQ, diff.LocationID));// The value will be replaced with INLocation.SiteID in the CommandPreparing.
				}

				if (cache.GetStatus(row) == PXEntryStatus.Inserted && IsZero(diff))
				{
					cache.SetStatus(row, PXEntryStatus.InsertedDeleted);
					return false;
				}

				return true;
			}

			public override bool PersistInserted(PXCache cache, object row)
			{
				try
				{
					return base.PersistInserted(cache, row);
				}
				catch (PXLockViolationException)
				{
					LocationStatusByCostCenter diff = (LocationStatusByCostCenter)row;
					LocationStatusByCostCenter orig = PK.Find(cache.Graph, diff);
					LocationStatusByCostCenter aggr = Aggregate(cache, orig, diff);

					string message = GetErrorMessage(cache, diff, aggr, insert: false);
					if (message != null)
						throw new PXException(message,
							PXForeignSelectorAttribute.GetValueExt<inventoryID>(cache, row),
							PXForeignSelectorAttribute.GetValueExt<subItemID>(cache, row),
							PXForeignSelectorAttribute.GetValueExt<siteID>(cache, row),
							PXForeignSelectorAttribute.GetValueExt<locationID>(cache, row));

					throw;
				}
			}

			public override void RowPersisted(PXCache cache, PXRowPersistedEventArgs e)
			{
				if (e.Operation.Command() == PXDBOperation.Insert && e.TranStatus == PXTranStatus.Open)
				{
					var balance = (LocationStatusByCostCenter)e.Row;

					string message = GetErrorMessage(cache, balance, null, insert: true);
					if (message != null)
					{
						// Acuminator disable once PX1073 ExceptionsInRowPersisted The transaction status is open.
						throw new PXException(message,
							PXForeignSelectorAttribute.GetValueExt<inventoryID>(cache, e.Row),
							PXForeignSelectorAttribute.GetValueExt<subItemID>(cache, e.Row),
							PXForeignSelectorAttribute.GetValueExt<siteID>(cache, e.Row),
							PXForeignSelectorAttribute.GetValueExt<locationID>(cache, e.Row));
					}
				}

				base.RowPersisted(cache, e);
			}

			protected virtual string GetErrorMessage(PXCache cache, LocationStatusByCostCenter bal, LocationStatusByCostCenter sumStatus, bool insert)
			{
				string GetCostLayerType(int? costCenterID)
				{
					return costCenterID == CostCenter.FreeStock
						? CostLayerType.Normal
						: INCostCenter.PK.Find(cache.Graph, costCenterID)?.CostLayerType;
				}

				string message = null;
				//only in release process updates onhand.
				if (bal.NegQty == false && bal.QtyOnHand < 0m)
				{
					if (insert || sumStatus.QtyOnHand < 0m)
					{
						message = GetCostLayerType(bal.CostCenterID) == CostLayerType.Special
							? Messages.StatusCheck_QtyLocationOnHandNegative_Special
							: Messages.StatusCheck_QtyLocationOnHandNegative;
					}
				}

				if (message == null && bal.NegQty == false && bal.QtyActual < 0m)
				{
					if (insert || sumStatus.QtyActual < 0)
					{
						message = GetCostLayerType(bal.CostCenterID) == CostLayerType.Special
							? Messages.StatusCheck_QtyLocationActualNegative_Special
							: Messages.StatusCheck_QtyLocationActualNegative;
					}
				}

				return message;
			}

			protected virtual PILocksInspector CreateLocksInspector(int siteID) => new PILocksInspector(siteID);
		}
	}
}
