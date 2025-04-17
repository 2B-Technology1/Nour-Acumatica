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
	public class TransitLotSerialStatusByCostCenter : INLotSerialStatusByCostCenter
	{
		#region Keys
		public new class PK : PrimaryKeyOf<TransitLotSerialStatusByCostCenter>.By<inventoryID, subItemID, siteID, locationID, lotSerialNbr, costCenterID>
		{
			public static TransitLotSerialStatusByCostCenter Find(PXGraph graph, int? inventoryID, int? subItemID, int? siteID, int? locationID, string lotSerialNbr, int? costCenterID, PKFindOptions options = PKFindOptions.None)
				=> FindBy(graph, inventoryID, subItemID, siteID, locationID, lotSerialNbr, costCenterID, options);
		}
		public static new class FK
		{
			public class Location : INLocation.PK.ForeignKeyOf<TransitLotSerialStatusByCostCenter>.By<locationID> { }
			public class LocationStatus : INLocationStatus.PK.ForeignKeyOf<TransitLotSerialStatusByCostCenter>.By<inventoryID, subItemID, siteID, locationID> { }
			public class SubItem : INSubItem.PK.ForeignKeyOf<TransitLotSerialStatusByCostCenter>.By<subItemID> { }
			public class InventoryItem : IN.InventoryItem.PK.ForeignKeyOf<TransitLotSerialStatusByCostCenter>.By<inventoryID> { }
			public class ItemLotSerial : INItemLotSerial.PK.ForeignKeyOf<TransitLotSerialStatusByCostCenter>.By<inventoryID, lotSerialNbr> { }
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
		[PXDefault]
		public override int? SiteID
		{
			get;
			set;
		}
		public new abstract class siteID : BqlInt.Field<siteID> { }
		#endregion
		#region LocationID
		[PXDBInt(IsKey = true)]
		[PXDBDefault(typeof(INTransitLine.costSiteID))]
		public override int? LocationID
		{
			get;
			set;
		}
		public new abstract class locationID : BqlInt.Field<locationID> { }
		#endregion
		#region LotSerialNbr
		[PXDBString(100, IsUnicode = true, IsKey = true)]
		[PXDefault]
		public override string LotSerialNbr
		{
			get;
			set;
		}
		public new abstract class lotSerialNbr : BqlString.Field<lotSerialNbr> { }
		#endregion
		#region CostCenterID
		public new abstract class costCenterID : BqlInt.Field<costCenterID> { }
		#endregion
		#region LotSerClassID
		[PXString(10, IsUnicode = true)]
		[PXDefault(typeof(SelectFrom<InventoryItem>.Where<InventoryItem.inventoryID.IsEqual<inventoryID.FromCurrent>>),
			SourceField = typeof(InventoryItem.lotSerClassID), CacheGlobal = true, PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual string LotSerClassID
		{
			get;
			set;
		}
		public abstract class lotSerClassID : BqlString.Field<lotSerClassID> { }
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

		#region ExpireDate
		[PXDate]
		public override DateTime? ExpireDate
		{
			get;
			set;
		}
		public new abstract class expireDate : BqlDateTime.Field<expireDate> { }
		#endregion
		#region ReceiptDate
		[PXDBDate]
		public override DateTime? ReceiptDate
		{
			get;
			set;
		}
		public new abstract class receiptDate : BqlDateTime.Field<receiptDate> { }
		#endregion
		#region LotSerTrack
		[PXDBString(1, IsFixed = true)]
		[PXDefault(typeof(SelectFrom<INLotSerClass>.Where<INLotSerClass.lotSerClassID.IsEqual<lotSerClassID.FromCurrent>>),
			SourceField = typeof(INLotSerClass.lotSerTrack), CacheGlobal = true, PersistingCheck = PXPersistingCheck.Nothing)]
		public override string LotSerTrack
		{
			get;
			set;
		}
		public new abstract class lotSerTrack : BqlString.Field<lotSerTrack> { }
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

				columns.Update<receiptDate>(Initialize);
				columns.Update<lotSerTrack>(Initialize);

				columns.Update<qtyOnHand>(Summarize);
				columns.Update<qtyAvail>(Summarize);
				columns.Update<qtyHardAvail>(Summarize);
				columns.Update<qtyActual>(Summarize);

				var diff = (TransitLotSerialStatusByCostCenter)row;

				//only in release process updates onhand.
				if (diff.QtyOnHand < 0m)
					columns.Restrict<qtyOnHand>(PXComp.GE, -diff.QtyOnHand);

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
					TransitLotSerialStatusByCostCenter diff = (TransitLotSerialStatusByCostCenter)row;
					TransitLotSerialStatusByCostCenter orig = PK.Find(cache.Graph, diff);
					TransitLotSerialStatusByCostCenter item = Aggregate(cache, orig, diff);

					string message = null;
					//only in release process updates onhand.
					if (diff.QtyOnHand < 0m && item.QtyOnHand < 0m)
						message = Messages.StatusCheck_QtyTransitLotSerialOnHandNegative;

					if (message != null)
						throw new PXException(message,
							PXForeignSelectorAttribute.GetValueExt<inventoryID>(cache, row),
							PXForeignSelectorAttribute.GetValueExt<subItemID>(cache, row),
							PXForeignSelectorAttribute.GetValueExt<lotSerialNbr>(cache, row));

					throw;
				}
			}

			public override void RowPersisted(PXCache cache, PXRowPersistedEventArgs e)
			{
				if (e.Operation.Command() == PXDBOperation.Insert && e.TranStatus == PXTranStatus.Open)
				{
					var balance = (TransitLotSerialStatusByCostCenter)e.Row;

					string message = null;
					//only in release process updates onhand.
					if (balance.QtyOnHand < 0m)
						message = Messages.StatusCheck_QtyTransitLotSerialOnHandNegative;

					if (message != null)
					{
						// Acuminator disable once PX1073 ExceptionsInRowPersisted The transaction status is open.
						throw new PXException(message,
							PXForeignSelectorAttribute.GetValueExt<inventoryID>(cache, e.Row),
							PXForeignSelectorAttribute.GetValueExt<subItemID>(cache, e.Row),
							PXForeignSelectorAttribute.GetValueExt<lotSerialNbr>(cache, e.Row));
					}
				}

				base.RowPersisted(cache, e);
			}
		}
	}
}
