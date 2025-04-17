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
	public class SiteLotSerial : INSiteLotSerial, IQtyAllocatedBase
	{
		#region Keys
		public new class PK : PrimaryKeyOf<SiteLotSerial>.By<inventoryID, siteID, lotSerialNbr>
		{
			public static SiteLotSerial Find(PXGraph graph, int? inventoryID, int? siteID, string lotSerialNbr, PKFindOptions options = PKFindOptions.None)
				=> FindBy(graph, inventoryID, siteID, lotSerialNbr, options);
		}
		public static new class FK
		{
			public class Site : INSite.PK.ForeignKeyOf<SiteLotSerial>.By<siteID> { }
			public class InventoryItem : IN.InventoryItem.PK.ForeignKeyOf<SiteLotSerial>.By<inventoryID> { }
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
		#region SiteID
		[Site(IsKey = true)]
		[PXRestrictor(typeof(Where<True.IsEqual<True>>), "", ReplaceInherited = true)]
		[PXDefault]
		public override int? SiteID
		{
			get => _SiteID;
			set => _SiteID = value;
		}
		public new abstract class siteID : BqlInt.Field<siteID> { }
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
		public new abstract class qtyNotAvail : BqlDecimal.Field<qtyNotAvail> { }
		[PXDBQuantity()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public override Decimal? QtyNotAvail
		{
			get => base.QtyNotAvail;
			set => base.QtyNotAvail = value;
		}
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
		#region NegActualQty
		[PXBool]
		[PXUnboundDefault(false)]
		public virtual bool? NegActualQty
		{
			get;
			set;
		}
		public abstract class negActualQty : BqlBool.Field<negActualQty> { }
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

		#region ValidateHardAvailQtyForAdjustments
		/// <exclude/>
		[PXBool, PXUnboundDefault(false)]
		public virtual Boolean? ValidateHardAvailQtyForAdjustments { get; set; }
		public abstract class validateHardAvailQtyForAdjustments : PX.Data.BQL.BqlBool.Field<validateHardAvailQtyForAdjustments> { }
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
				graph.Caches.SubscribeCacheCreated<SiteLotSerial>(() =>
				{
					if (graph.Caches<SiteLotSerial>().Interceptor is AccumulatorAttribute attr)
						attr.forceValidateAvailQty = val;
				});
			}

			public AccumulatorAttribute()
			{
				SingleRecord = true;
			}

			protected virtual void PrepareSingleField<TQtyField>(PXCache cache, SiteLotSerial diff, PXAccumulatorCollection columns)
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
				}
			}

			protected virtual void ValidateSingleField<TQtyField>(PXCache cache, SiteLotSerial aggr, Guid? refNoteID, ref string message)
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

				var diff = (SiteLotSerial)row;

				if (diff.LotSerTrack == INLotSerTrack.SerialNumbered && diff.LotSerAssign == INLotSerAssign.WhenUsed)
					return false;

				columns.Update<lotSerTrack>(Initialize);
				columns.Update<lotSerAssign>(Initialize);
				columns.Update<expireDate>(diff.UpdateExpireDate == true ? Replace : Initialize);

				columns.Update<qtyOnHand>(Summarize);
				columns.Update<qtyAvail>(Summarize);
				columns.Update<qtyNotAvail>(Summarize);
				columns.Update<qtyAvailOnSite>(Summarize);
				columns.Update<qtyHardAvail>(Summarize);
				columns.Update<qtyActual>(Summarize);

				columns.Update<qtyInTransit>(Summarize);

				//only in release process updates onhand.
				if (diff.QtyOnHand != 0m)
				{
					PrepareSingleField<qtyOnHand>(cache, diff, columns);
				}
				else if (ValidateAvailQty(cache.Graph))
				{
					if (diff.QtyHardAvail < 0m)
						PrepareSingleField<qtyHardAvail>(cache, diff, columns);

					if (diff.QtyAvail != 0m)
						PrepareSingleField<qtyAvail>(cache, diff, columns);

					if (diff.QtyHardAvail < 0m && diff.LotSerTrack == INLotSerTrack.LotNumbered && diff.LotSerAssign == INLotSerAssign.WhenReceived)
						columns.Restrict<qtyHardAvail>(PXComp.GE, -diff.QtyHardAvail);
				}

				if (diff.NegActualQty != true && diff.QtyActual < 0m && diff.LotSerAssign == INLotSerAssign.WhenReceived)
					columns.Restrict<qtyActual>(PXComp.GE, -diff.QtyActual);

				if (diff.ValidateHardAvailQtyForAdjustments == true && diff.QtyHardAvail < 0m)
				{
					columns.AppendException(string.Empty, new PXAccumulatorRestriction<SiteLotSerial.qtyHardAvail>(PXComp.GE, 0m));
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
					SiteLotSerial diff = (SiteLotSerial)row;
					SiteLotSerial orig = PK.Find(cache.Graph, diff);
					SiteLotSerial aggr = Aggregate(cache, orig, diff);
					aggr.LotSerTrack = diff.LotSerTrack;
					aggr.LotSerAssign = diff.LotSerAssign;

					Guid? refNoteID = LookupDocumentsByLotSerialNumber(cache, diff, out bool isDuplicated, out string refRowID);

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

						if (diff.QtyHardAvail < 0m && aggr.QtyHardAvail < 0 && diff.LotSerTrack == INLotSerTrack.LotNumbered && diff.LotSerAssign == INLotSerAssign.WhenReceived)
							throw new PXException(
								cache.Graph is INRegisterEntryBase g && g.insetup.Current.AllocateDocumentsOnHold == true
									? Messages.StatusCheck_QtyLotNbrHardAvailNegativeINScreens
									: Messages.StatusCheck_QtyLotNbrHardAvailNegative,
								PXForeignSelectorAttribute.GetValueExt<inventoryID>(cache, row),
								PXForeignSelectorAttribute.GetValueExt<siteID>(cache, row),
								PXForeignSelectorAttribute.GetValueExt<lotSerialNbr>(cache, row));
					}

					if (diff.NegActualQty != true && diff.QtyActual < 0m && aggr.QtyActual < 0 && diff.LotSerAssign == INLotSerAssign.WhenReceived)
						throw new PXException(Messages.StatusCheck_QtyLotSerialActualNegative,
							PXForeignSelectorAttribute.GetValueExt<inventoryID>(cache, row),
							PXForeignSelectorAttribute.GetValueExt<siteID>(cache, row),
							PXForeignSelectorAttribute.GetValueExt<lotSerialNbr>(cache, row));

					if (message != null)
						throw new PXException(message,
							PXForeignSelectorAttribute.GetValueExt<inventoryID>(cache, row),
							PXForeignSelectorAttribute.GetValueExt<lotSerialNbr>(cache, row),
							refRowID);

					throw;
				}
			}

			private Guid? LookupDocumentsByLotSerialNumber(PXCache cache, SiteLotSerial row, out bool isDuplicated, out string refRowID)
			{
				isDuplicated = false;
				refRowID = null;

				PXResultset<INItemPlan> plans = GetItemPlans(cache, row.InventoryID, row.SiteID, row.LotSerialNbr);

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

			protected virtual PXResultset<INItemPlan> GetItemPlans(PXCache cache, int? inventoryID, int? siteID, string lotSerialNbr)
			{
				return
					PXSelect<INItemPlan,
					Where<
						INItemPlan.inventoryID, Equal<Required<INItemPlan.inventoryID>>,
						And<INItemPlan.siteID, Equal<Required<INItemPlan.siteID>>,
						And<INItemPlan.lotSerialNbr, Equal<Required<INItemPlan.lotSerialNbr>>>>>>
					.SelectWindowed(cache.Graph, 0, 10, inventoryID, siteID, lotSerialNbr);
			}

			public virtual bool IsZero(SiteLotSerial a) => a.IsZero();
		}
	}
}
