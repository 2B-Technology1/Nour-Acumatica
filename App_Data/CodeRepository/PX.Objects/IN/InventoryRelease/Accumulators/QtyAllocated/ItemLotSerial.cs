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
using System.Linq;
using PX.Common;
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
	public class ItemLotSerial : INItemLotSerial, IQtyAllocatedSeparateReceipts
	{
		#region Keys
		public new class PK : PrimaryKeyOf<ItemLotSerial>.By<inventoryID, lotSerialNbr>
		{
			public static ItemLotSerial Find(PXGraph graph, int? inventoryID, string lotSerialNbr, PKFindOptions options = PKFindOptions.None) => FindBy(graph, inventoryID, lotSerialNbr, options);
		}
		public static new class FK
		{
			public class InventoryItem : IN.InventoryItem.PK.ForeignKeyOf<ItemLotSerial>.By<inventoryID> { }
		}
		#endregion

		#region InventoryID
		[PXDBInt(IsKey = true)]
		[PXForeignSelector(typeof(INTran.inventoryID))]
		[PXSelectorMarker(typeof(SearchFor<InventoryItem.inventoryID>.Where<InventoryItem.inventoryID.IsEqual<inventoryID.FromCurrent>>), CacheGlobal = true)]
		[PXDefault]
		public override int? InventoryID
		{
			get => _InventoryID;
			set => _InventoryID = value;
		}
		public new abstract class inventoryID : BqlInt.Field<inventoryID> { }
		#endregion
		#region LotSerialNbr
		[PXDBString(100, IsUnicode = true, IsKey = true)]
		[PXDefault]
		public override string LotSerialNbr
		{
			get => _LotSerialNbr;
			set => _LotSerialNbr = value;
		}
		public new abstract class lotSerialNbr : BqlString.Field<lotSerialNbr> { }
		#endregion
		#region RefNotID
		[PXGuid]
		public virtual Guid? RefNoteID
		{
			get;
			set;
		}
		public abstract class refNoteID : BqlGuid.Field<refNoteID> { }
		#endregion
		#region IsDuplicated
		[PXBool]
		public virtual bool? IsDuplicated
		{
			get;
			set;
		}
		public abstract class isDuplicated : BqlBool.Field<isDuplicated> { }
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
		#region LotSerTrack
		[PXDBString(1, IsFixed = true)]
		[PXDefault(typeof(SelectFrom<INLotSerClass>.Where<INLotSerClass.lotSerClassID.IsEqual<lotSerClassID.FromCurrent>>),
			SourceField = typeof(INLotSerClass.lotSerTrack), CacheGlobal = true, PersistingCheck = PXPersistingCheck.Nothing)]
		public override string LotSerTrack
		{
			get => _LotSerTrack;
			set => _LotSerTrack = value;
		}
		public new abstract class lotSerTrack : BqlString.Field<lotSerTrack> { }
		#endregion
		#region LotSerAssign
		[PXDBString(1, IsFixed = true)]
		[PXDefault(typeof(SelectFrom<INLotSerClass>.Where<INLotSerClass.lotSerClassID.IsEqual<lotSerClassID.FromCurrent>>),
			SourceField = typeof(INLotSerClass.lotSerAssign), CacheGlobal = true, PersistingCheck = PXPersistingCheck.Nothing)]
		public override string LotSerAssign
		{
			get => _LotSerAssign;
			set => _LotSerAssign = value;
		}
		public new abstract class lotSerAssign : BqlString.Field<lotSerAssign> { }
		#endregion
		#region ExpireDate
		[PXDBDate]
		public override DateTime? ExpireDate
		{
			get => _ExpireDate;
			set => _ExpireDate = value;
		}
		public new abstract class expireDate : BqlDateTime.Field<expireDate> { }
		#endregion

		#region QtyNotAvail
		[PXDecimal(6)]
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual decimal? QtyNotAvail
		{
			get;
			set;
		}
		public abstract class qtyNotAvail : BqlDecimal.Field<qtyNotAvail> { }
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
		[AvailabilityFlag(true)]
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
		[AvailabilityFlag(true)]
		public virtual bool? InclQtyINIssues
		{
			get;
			set;
		}
		public abstract class inclQtyINIssues : BqlBool.Field<inclQtyINIssues> { }
		#endregion
		#region InclQtyINReceipts
		[AvailabilityFlag(true)]
		public virtual bool? InclQtyINReceipts
		{
			get;
			set;
		}
		public abstract class inclQtyINReceipts : BqlBool.Field<inclQtyINReceipts> { }
		#endregion
		#region InclQtyINAssemblyDemand
		[AvailabilityFlag(true)]
		public virtual bool? InclQtyINAssemblyDemand
		{
			get;
			set;
		}
		public abstract class inclQtyINAssemblyDemand : BqlBool.Field<inclQtyINAssemblyDemand> { }
		#endregion
		#region InclQtyINAssemblySupply
		[AvailabilityFlag(true)]
		public virtual bool? InclQtyINAssemblySupply
		{
			get;
			set;
		}
		public abstract class inclQtyINAssemblySupply : BqlBool.Field<inclQtyINAssemblySupply> { }
		#endregion
		#region InclQtyInTransit
		[AvailabilityFlag(true)]
		public virtual bool? InclQtyInTransit
		{
			get;
			set;
		}
		public abstract class inclQtyInTransit : BqlBool.Field<inclQtyInTransit> { }
		#endregion

		#region InclQtySOReverse
		[AvailabilityFlag(true)]
		public virtual bool? InclQtySOReverse
		{
			get;
			set;
		}
		public abstract class inclQtySOReverse : BqlBool.Field<inclQtySOReverse> { }
		#endregion
		#region InclQtySOBackOrdered
		[AvailabilityFlag(true)]
		public virtual bool? InclQtySOBackOrdered
		{
			get;
			set;
		}
		public abstract class inclQtySOBackOrdered : BqlBool.Field<inclQtySOBackOrdered> { }
		#endregion
		#region InclQtySOPrepared
		[AvailabilityFlag(true)]
		public virtual bool? InclQtySOPrepared
		{
			get;
			set;
		}
		public abstract class inclQtySOPrepared : BqlBool.Field<inclQtySOPrepared> { }
		#endregion
		#region InclQtySOBooked
		[AvailabilityFlag(true)]
		public virtual bool? InclQtySOBooked
		{
			get;
			set;
		}
		public abstract class inclQtySOBooked : BqlBool.Field<inclQtySOBooked> { }
		#endregion
		#region InclQtySOShipped
		[AvailabilityFlag(true)]
		public virtual bool? InclQtySOShipped
		{
			get;
			set;
		}
		public abstract class inclQtySOShipped : BqlBool.Field<inclQtySOShipped> { }
		#endregion
		#region InclQtySOShipping
		[AvailabilityFlag(true)]
		public virtual bool? InclQtySOShipping
		{
			get;
			set;
		}
		public abstract class inclQtySOShipping : BqlBool.Field<inclQtySOShipping> { }
		#endregion

		#region InclQtyPOReceipts
		[AvailabilityFlag(true)]
		public virtual bool? InclQtyPOReceipts
		{
			get;
			set;
		}
		public abstract class inclQtyPOReceipts : BqlBool.Field<inclQtyPOReceipts> { }
		#endregion
		#region InclQtyPOPrepared
		[AvailabilityFlag(true)]
		public virtual bool? InclQtyPOPrepared
		{
			get;
			set;
		}
		public abstract class inclQtyPOPrepared : BqlBool.Field<inclQtyPOPrepared> { }
		#endregion
		#region InclQtyPOOrders
		[AvailabilityFlag(true)]
		public virtual bool? InclQtyPOOrders
		{
			get;
			set;
		}
		public abstract class inclQtyPOOrders : BqlBool.Field<inclQtyPOOrders> { }
		#endregion
		#region InclQtyFixedSOPO
		[AvailabilityFlag(false)]
		public virtual bool? InclQtyFixedSOPO
		{
			get;
			set;
		}
		public abstract class inclQtyFixedSOPO : BqlBool.Field<inclQtyFixedSOPO> { }
		#endregion
		#region InclQtyPOFixedReceipt
		[AvailabilityFlag(true)]
		public virtual bool? InclQtyPOFixedReceipt
		{
			get;
			set;
		}
		public abstract class inclQtyPOFixedReceipt : BqlBool.Field<inclQtyPOFixedReceipt> { }
		#endregion

		#region InclQtyProductionDemandPrepared
		[AvailabilityFlag(true)]
		public virtual bool? InclQtyProductionDemandPrepared
		{
			get;
			set;
		}
		public abstract class inclQtyProductionDemandPrepared : BqlBool.Field<inclQtyProductionDemandPrepared> { }
		#endregion
		#region InclQtyProductionDemand
		[AvailabilityFlag(true)]
		public virtual bool? InclQtyProductionDemand
		{
			get;
			set;
		}
		public abstract class inclQtyProductionDemand : BqlBool.Field<inclQtyProductionDemand> { }
		#endregion
		#region InclQtyProductionAllocated
		[AvailabilityFlag(true)]
		public virtual bool? InclQtyProductionAllocated
		{
			get;
			set;
		}
		public abstract class inclQtyProductionAllocated : BqlBool.Field<inclQtyProductionAllocated> { }
		#endregion
		#region InclQtyProductionSupplyPrepared
		[AvailabilityFlag(true)]
		public virtual bool? InclQtyProductionSupplyPrepared
		{
			get;
			set;
		}
		public abstract class inclQtyProductionSupplyPrepared : BqlBool.Field<inclQtyProductionSupplyPrepared> { }
		#endregion
		#region InclQtyProductionSupply
		[AvailabilityFlag(true)]
		public virtual bool? InclQtyProductionSupply
		{
			get;
			set;
		}
		public abstract class inclQtyProductionSupply : BqlBool.Field<inclQtyProductionSupply> { }
		#endregion

		#region InclQtyFSSrvOrdPrepared
		[AvailabilityFlag(true)]
		public virtual bool? InclQtyFSSrvOrdPrepared
		{
			get;
			set;
		}
		public abstract class inclQtyFSSrvOrdPrepared : BqlBool.Field<inclQtyFSSrvOrdPrepared> { }
		#endregion
		#region InclQtyFSSrvOrdBooked
		[AvailabilityFlag(true)]
		public virtual bool? InclQtyFSSrvOrdBooked
		{
			get;
			set;
		}
		public abstract class inclQtyFSSrvOrdBooked : BqlBool.Field<inclQtyFSSrvOrdBooked> { }
		#endregion
		#region InclQtyFSSrvOrdAllocated
		[AvailabilityFlag(true)]
		public virtual bool? InclQtyFSSrvOrdAllocated
		{
			get;
			set;
		}
		public abstract class inclQtyFSSrvOrdAllocated : BqlBool.Field<inclQtyFSSrvOrdAllocated> { }
		#endregion

		#region IsIntercompany
		[PXBool]
		public virtual bool? IsIntercompany
		{
			get;
			set;
		}
		public abstract class isIntercompany : BqlBool.Field<isIntercompany> { }
		#endregion

		public class AccumulatorAttribute : StatusAccumulatorAttribute
		{
			protected class AutoNumberedEntityHelper : EntityHelper
			{
				public AutoNumberedEntityHelper(PXGraph graph)
					: base(graph)
				{ }

				public override string GetFieldString(object row, Type entityType, string fieldName, bool preferDescription)
				{
					PXCache cache = graph.Caches[entityType];

					if (cache.GetStatus(row) == PXEntryStatus.Inserted)
					{
						object cached = cache.Locate(row);
						string val = AutoNumberAttribute.GetKeyToAbort(cache, cached, fieldName);

						if (val != null)
							return val;
					}

					return base.GetFieldString(row, entityType, fieldName, preferDescription);
				}
			}

			protected bool forceValidateAvailQty;

			public virtual bool ValidateAvailQty(PXGraph graph) => forceValidateAvailQty || !graph.UnattendedMode;

			public static void ForceAvailQtyValidation(PXGraph graph, bool val)
			{
				graph.Caches.SubscribeCacheCreated<ItemLotSerial>(() =>
				{
					if (graph.Caches<ItemLotSerial>().Interceptor is AccumulatorAttribute attr)
						attr.forceValidateAvailQty = val;
				});
			}

			public AccumulatorAttribute()
			{
				SingleRecord = true;
			}

			protected virtual void PrepareSingleField<TQtyField>(PXCache cache, ItemLotSerial diff, PXAccumulatorCollection columns)
				where TQtyField : IBqlField, IImplement<IBqlDecimal>
			{
				decimal? qty = (decimal?)cache.GetValue<TQtyField>(diff);

				if (diff.LotSerTrack == INLotSerTrack.SerialNumbered && qty.IsNotIn(null, 0m, -1m, 1m))
					throw new PXException(Messages.SerialNumberDuplicated,
						PXForeignSelectorAttribute.GetValueExt<inventoryID>(cache, diff),
						PXForeignSelectorAttribute.GetValueExt<lotSerialNbr>(cache, diff));

				if (diff.LotSerTrack == INLotSerTrack.SerialNumbered && diff.LotSerAssign == INLotSerAssign.WhenReceived)
				{
					columns.Restrict<TQtyField>(PXComp.LE, 1m - qty);
					columns.Restrict<TQtyField>(PXComp.GE, 0m - qty);

					if (diff.QtyOnHand == 1m)
						columns.AppendException(string.Empty, new PXAccumulatorRestriction<qtyInTransit>(PXComp.NE, 1m));
				}

				if (diff.LotSerTrack == INLotSerTrack.SerialNumbered && diff.LotSerAssign == INLotSerAssign.WhenUsed)
				{
					columns.AppendException(string.Empty,
						new PXAccumulatorRestriction<TQtyField>(PXComp.LE, 1m));
					columns.AppendException(string.Empty,
						new PXAccumulatorRestriction<TQtyField>(PXComp.GE, -1m));

					columns.AppendException(string.Empty,
						new PXAccumulatorRestriction<qtyOrig>(PXComp.ISNULL, null),
						new PXAccumulatorRestriction<qtyOrig>(PXComp.NE, -1m),
						new PXAccumulatorRestriction<TQtyField>(PXComp.LT, 1m));
					columns.AppendException(string.Empty,
						new PXAccumulatorRestriction<qtyOrig>(PXComp.ISNULL, null),
						new PXAccumulatorRestriction<qtyOrig>(PXComp.NE, 1m),
						new PXAccumulatorRestriction<TQtyField>(PXComp.GT, -1m));
				}
			}

			protected virtual void ValidateSingleField<TQtyField>(PXCache cache, ItemLotSerial aggr, Guid? refNoteID, ref string message)
				where TQtyField : IBqlField, IImplement<IBqlDecimal>
			{
				decimal? qty = (decimal?)cache.GetValue<TQtyField>(aggr);

				if (aggr.LotSerTrack == INLotSerTrack.SerialNumbered && aggr.LotSerAssign == INLotSerAssign.WhenReceived)
				{
					//consider reusing PXAccumulator rules
					if (qty < 0m)
						message = refNoteID == null ? Messages.SerialNumberAlreadyIssued : Messages.SerialNumberAlreadyIssuedIn;
					else if (qty > 1m)
						message = refNoteID == null ? Messages.SerialNumberAlreadyReceived : Messages.SerialNumberAlreadyReceivedIn;
				}
			}

			protected override bool PrepareInsert(PXCache cache, object row, PXAccumulatorCollection columns)
			{
				if (!base.PrepareInsert(cache, row, columns))
					return false;

				var diff = (ItemLotSerial)row;

				columns.Update<lotSerTrack>(Initialize);
				columns.Update<lotSerAssign>(Initialize);
				columns.Update<expireDate>(diff.UpdateExpireDate == true ? Replace : Initialize);

				columns.Update<qtyOnHand>(Summarize);
				columns.Update<qtyAvail>(Summarize);
				columns.Update<qtyHardAvail>(Summarize);
				columns.Update<qtyActual>(Summarize);

				columns.Update<qtyInTransit>(Summarize);
				columns.InitializeWith<qtyOrig>(diff.QtyOrig);
				columns.InitializeWith<noteID>(diff.NoteID);

				bool whenUsedSerialNumber = diff.LotSerTrack == INLotSerTrack.SerialNumbered && diff.LotSerAssign == INLotSerAssign.WhenUsed;
				bool whenRcvdSerialNumber = diff.LotSerTrack == INLotSerTrack.SerialNumbered && diff.LotSerAssign == INLotSerAssign.WhenReceived;
				//only in release process updates onhand.
				if (diff.QtyOnHand != 0m)
				{
					if (whenUsedSerialNumber && diff.QtyOnHand != null)
						columns.Update<qtyOrig>(diff.QtyOnHand, Initialize);

					PrepareSingleField<qtyOnHand>(cache, diff, columns);
				}
				else if (ValidateAvailQty(cache.Graph))
				{
					if (diff.QtyHardAvail < 0m)
						PrepareSingleField<qtyHardAvail>(cache, diff, columns);

					if (diff.QtyAvail != 0m)
						PrepareSingleField<qtyAvail>(cache, diff, columns);

					if (diff.QtyOnReceipt != 0m && whenRcvdSerialNumber)
					{
						PrepareSingleField<qtyOnReceipt>(cache, diff, columns);
						if (diff.QtyOnReceipt > 0m)
							columns.Restrict<qtyOnHand>(PXComp.LE, 1m - diff.QtyOnReceipt);
					}
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
				ItemLotSerial diff = (ItemLotSerial)row;

				try
				{
					return base.PersistInserted(cache, row);
				}
				catch (PXLockViolationException)
				{
					ItemLotSerial orig = PK.Find(cache.Graph, diff);
					ItemLotSerial aggr = Aggregate(cache, orig, diff);
					aggr.LotSerTrack = diff.LotSerTrack;
					aggr.LotSerAssign = diff.LotSerAssign;

					bool whenRcvdSerialNumber = diff.LotSerTrack == INLotSerTrack.SerialNumbered && diff.LotSerAssign == INLotSerAssign.WhenReceived;
					if (diff.IsIntercompany == true && whenRcvdSerialNumber && diff.QtyOnHand > 0m && aggr.QtyOnHand > 1m)
					{
						PXResultset<INItemPlan> plans = GetItemPlans(cache, diff.InventoryID, diff.LotSerialNbr);
						if (plans.RowCast<INItemPlan>().Any(p => p.PlanType == INPlanConstants.Plan62))
							throw new PXIntercompanyReceivedNotIssuedException(cache, aggr);
					}

					Guid? refNoteID = LookupDocumentsByLotSerialNumber(cache, diff.InventoryID, diff.LotSerialNbr, out var isDuplicated, out var refRowID);

					string message = null;
					if (diff.LotSerTrack == INLotSerTrack.SerialNumbered && isDuplicated)
						message = Messages.SerialNumberDuplicated;

					//only in release process updates onhand.
					if (diff.QtyOnHand != 0m)
					{
						ValidateSingleField<qtyOnHand>(cache, aggr, null, ref message);
					}
					else if (ValidateAvailQty(cache.Graph))
					{
						ValidateSingleField<qtyAvail>(cache, aggr, refNoteID, ref message);
						ValidateSingleField<qtyHardAvail>(cache, aggr, refNoteID, ref message);
						if (whenRcvdSerialNumber)
						{
							ValidateSingleField<qtyOnReceipt>(cache, aggr, refNoteID, ref message);
							if (message == null && aggr.QtyOnHand + diff.QtyOnReceipt > 1m)
								message = Messages.SerialNumberAlreadyReceived;
						}
					}

					if (message != null)
						throw new PXException(message,
							PXForeignSelectorAttribute.GetValueExt<inventoryID>(cache, row),
							PXForeignSelectorAttribute.GetValueExt<lotSerialNbr>(cache, row),
							refRowID);

					throw;
				}
				catch (PXRestrictionViolationException exc)
				{
					Guid? refNoteID = LookupDocumentsByLotSerialNumber(cache, diff.InventoryID, diff.LotSerialNbr, out var isDuplicated, out var refRowID);

					// even numbers are about already received, odd - issued
					bool alreadyReceived = exc.Index % 2 == 0;
					string message = isDuplicated
						? Messages.SerialNumberDuplicated
						: alreadyReceived
							? refNoteID != null
								? Messages.SerialNumberAlreadyReceivedIn
								: Messages.SerialNumberAlreadyReceived
							: refNoteID != null
								? Messages.SerialNumberAlreadyIssuedIn
								: Messages.SerialNumberAlreadyIssued;

					throw new PXException(message,
						PXForeignSelectorAttribute.GetValueExt<inventoryID>(cache, row),
						PXForeignSelectorAttribute.GetValueExt<lotSerialNbr>(cache, row),
						refRowID);
				}
			}

			protected virtual Guid? LookupDocumentsByLotSerialNumber(PXCache cache, int? inventoryID, string lotSerialNbr, out bool isDuplicated, out string refRowID)
			{
				isDuplicated = false;
				refRowID = null;

				PXResultset<INItemPlan> plans = GetItemPlans(cache, inventoryID, lotSerialNbr);

				if (plans.Count <= 1)
				{
					return null;
				}
				else
				{
					var refs = new List<Guid?>();
					var counts = new Dictionary<Guid?, int>();
					var planCache = cache.Graph.Caches<INItemPlan>();

					for (int i = 0; i < plans.Count; i++)
					{
						var plan = plans[i].GetItem<INItemPlan>();
						Guid? refNoteID = plan.RefNoteID;

						if (planCache.GetStatus(plan) == PXEntryStatus.Notchanged)
							refs.Insert(0, refNoteID);
						else
							refs.Add(refNoteID);

						if (counts.ContainsKey(refNoteID))
						{
							counts[refNoteID]++;
							isDuplicated = true;
						}
						else
						{
							counts[refNoteID] = 1;
						}
					}

					refRowID = new AutoNumberedEntityHelper(cache.Graph).GetEntityRowID(refs[0]);

					return refs[0];
				}
			}

			protected virtual PXResultset<INItemPlan> GetItemPlans(PXCache cache, int? inventoryID, string lotSerialNbr)
			{
				return
					PXSelect<INItemPlan,
					Where<
						INItemPlan.inventoryID, Equal<Required<INItemPlan.inventoryID>>,
						And<INItemPlan.lotSerialNbr, Equal<Required<INItemPlan.lotSerialNbr>>>>>
					.SelectWindowed(cache.Graph, 0, 10, inventoryID, lotSerialNbr);
			}

			public virtual bool IsZero(ItemLotSerial a) => a.IsZero();
		}
	}
}
