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

namespace PX.Objects.IN.InventoryRelease.Accumulators.QtyAllocated
{
	using Abstraction;
	using static PXDataFieldAssign.AssignBehavior;

	[PXHidden]
	[Accumulator]
	public class TransitLocationStatusByCostCenter : INLocationStatusByCostCenter
	{
		#region Keys
		public new class PK : PrimaryKeyOf<TransitLocationStatusByCostCenter>.By<inventoryID, subItemID, siteID, locationID, costCenterID>
		{
			public static TransitLocationStatusByCostCenter Find(PXGraph graph, int? inventoryID, int? subItemID, int? siteID, int? locationID, int? costCenterID, PKFindOptions options = PKFindOptions.None)
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
		[SubItem(IsKey = true)]
		[PXForeignSelector(typeof(INTran.subItemID))]
		[PXDefault]
		public override int? SubItemID
		{
			get;
			set;
		}
		public new abstract class subItemID : BqlInt.Field<subItemID> { }
		#endregion
		#region SiteID
		[Site(true, IsKey = true)]
		[PXForeignSelector(typeof(INTran.siteID))]
		[PXDefault]
		public override int? SiteID
		{
			get => base.SiteID;
			set => base.SiteID = value;
		}
		public new abstract class siteID : BqlInt.Field<siteID> { }
		#endregion
		#region LocationID
		[PXDBInt(IsKey = true)]
		[PXForeignSelector(typeof(INTran.locationID))]
		[PXDBDefault(typeof(INTransitLine.costSiteID))]
		public override int? LocationID
		{
			get => base.LocationID;
			set => base.LocationID = value;
		}
		public new abstract class locationID : BqlInt.Field<locationID> { }
		#endregion
		#region CostCenterID
		public new abstract class costCenterID : BqlInt.Field<costCenterID> { }
		#endregion

		#region QtyOnHand
		public new abstract class qtyOnHand : BqlDecimal.Field<qtyOnHand> { }
		#endregion
		#region QtyAvail
		public new abstract class qtyAvail : BqlDecimal.Field<qtyAvail> { }
		#endregion
		#region QtyHardAvail
		public new abstract class qtyHardAvail : BqlDecimal.Field<qtyHardAvail> { }
		#endregion
		#region QtyActual
		public new abstract class qtyActual : BqlDecimal.Field<qtyActual> { }
		#endregion

		public class AccumulatorAttribute : StatusAccumulatorAttribute
		{
			public AccumulatorAttribute()
			{
				SingleRecord = true;
			}

			protected override bool PrepareInsert(PXCache cache, object row, PXAccumulatorCollection columns)
			{
				if (!base.PrepareInsert(cache, row, columns))
					return false;

				columns.Update<qtyOnHand>(Summarize);
				columns.Update<qtyAvail>(Summarize);
				columns.Update<qtyHardAvail>(Summarize);
				columns.Update<qtyActual>(Summarize);

				var diff = (TransitLocationStatusByCostCenter)row;

				//only in release process updates onhand.
				if (diff.QtyOnHand < 0m)
					columns.Restrict<qtyOnHand>(PXComp.GE, -diff.QtyOnHand);

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
					TransitLocationStatusByCostCenter diff = (TransitLocationStatusByCostCenter)row;
					TransitLocationStatusByCostCenter orig = PK.Find(cache.Graph, diff);
					TransitLocationStatusByCostCenter aggr = Aggregate(cache, orig, diff);

					string message = null;
					//only in release process updates onhand.
					if (diff.QtyOnHand < 0m && aggr.QtyOnHand < 0m)
						message = Messages.StatusCheck_QtyTransitOnHandNegative;

					if (message != null)
						throw new PXException(message,
							PXForeignSelectorAttribute.GetValueExt<inventoryID>(cache, row),
							PXForeignSelectorAttribute.GetValueExt<subItemID>(cache, row));

					throw;
				}
			}

			public override void RowPersisted(PXCache cache, PXRowPersistedEventArgs e)
			{
				if (e.Operation.Command() == PXDBOperation.Insert && e.TranStatus == PXTranStatus.Open)
				{
					var balance = (TransitLocationStatusByCostCenter)e.Row;

					string message = null;
					//only in release process updates onhand.
					if (balance.QtyOnHand < 0m)
						message = Messages.StatusCheck_QtyTransitOnHandNegative;

					if (message != null)
					{
						// Acuminator disable once PX1073 ExceptionsInRowPersisted The transaction status is open.
						throw new PXException(message,
							PXForeignSelectorAttribute.GetValueExt<inventoryID>(cache, e.Row),
							PXForeignSelectorAttribute.GetValueExt<subItemID>(cache, e.Row));
					}
				}

				base.RowPersisted(cache, e);
			}
		}
	}
}
