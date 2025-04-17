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

namespace PX.Objects.IN.GraphExtensions.INRegisterEntryBaseExt
{
	public class INTranSplitPlan : INTranSplitPlan<INRegisterEntryBase>
	{
	}

	public abstract class INTranSplitPlan<TGraph> : INTranSplitPlanBase<TGraph, INRegister, INTranSplit>
		where TGraph : PXGraph
	{
		protected override void PrefetchDocumentPlansToCache()
		{
			PXSelect<INItemPlan, Where<INItemPlan.refNoteID, Equal<Current<INRegister.noteID>>>>.Select(Base).Consume();
		}

		protected override IEnumerable<INTranSplit> GetDocumentSplits()
		{
			return PXSelect<INTranSplit,
					Where<INTranSplit.docType, Equal<Current<INRegister.docType>>,
						And<INTranSplit.refNbr, Equal<Current<INRegister.refNbr>>>>>
					.Select(Base)
					.RowCast<INTranSplit>();
		}

		public override INItemPlan DefaultValues(INItemPlan planRow, INTranSplit origRow)
		{
			planRow = base.DefaultValues(planRow, origRow);
			if (planRow == null)
				return null;

			INTran parent = PXParentAttribute.SelectParent<INTran>(ItemPlanSourceCache, origRow);
			planRow.ProjectID = parent.ProjectID;
			planRow.TaskID = parent.TaskID;
			planRow.CostCenterID = parent.CostCenterID;
			planRow.UOM = parent.UOM;
			planRow.BAccountID = parent.BAccountID;

			if (parent != null && parent.OrigTranType == INTranType.Transfer && planRow.OrigNoteID == null)
			{
				planRow.OrigNoteID = parent.OrigNoteID;
				planRow.OrigPlanLevel =
					(parent.OrigToLocationID != null ? INPlanLevel.Location : INPlanLevel.Site)
					| (parent.OrigIsLotSerial == true ? INPlanLevel.LotSerial : INPlanLevel.Site);
			}

			return planRow;
		}
	}

	public abstract class INTranSplitPlanBase<TGraph, TRefEntity, TItemPlanSource> : ItemPlan<TGraph, TRefEntity, TItemPlanSource>
		where TGraph : PXGraph
		where TRefEntity : class, IItemPlanRegister, IBqlTable, new()
		where TItemPlanSource : class, IItemPlanINSource, IBqlTable, new()
	{
		public override void _(Events.RowUpdated<TRefEntity> e)
		{
			base._(e);

			if (e.Row.Hold != e.OldRow.Hold || e.Row.TransferType != e.OldRow.TransferType)
			{
				bool transferTypeUpdated = !Equals(e.Row.TransferType, e.OldRow.TransferType);

				PrefetchDocumentPlansToCache();

				foreach (TItemPlanSource split in GetDocumentSplits())
				{
					foreach (INItemPlan plan in PlanCache.Cached)
					{
						if (plan.PlanID == split.PlanID && PlanCache.GetStatus(plan).IsNotIn(PXEntryStatus.Deleted, PXEntryStatus.InsertedDeleted))
						{
							if (transferTypeUpdated)
							{
								split.TransferType = e.Row.TransferType;
								ItemPlanSourceCache.MarkUpdated(split, assertError: true);

								INItemPlan copy = PXCache<INItemPlan>.CreateCopy(plan);
								copy = DefaultValues(copy, split);
								PlanCache.Update(copy);
							}
							else
							{
								plan.Hold = e.Row.Hold;
								if (!GetAllocateDocumentsOnHold())
								{
									PlanCache.Update(plan);
								}
								else
								{
									PlanCache.MarkUpdated(plan, assertError: true);
								}
							}
						}
					}
				}
			}
		}

		protected abstract void PrefetchDocumentPlansToCache();

		protected abstract IEnumerable<TItemPlanSource> GetDocumentSplits();

		public override INItemPlan DefaultValues(INItemPlan planRow, TItemPlanSource origRow)
		{
			PXCache parentCache = Base.Caches<TRefEntity>();
			TRefEntity header = (TRefEntity)parentCache.Current;

			planRow.OrigPlanType = origRow.OrigPlanType;
			planRow.InventoryID = origRow.InventoryID;
			planRow.SubItemID = origRow.SubItemID;
			planRow.SiteID = origRow.SiteID;
			planRow.LocationID = origRow.LocationID;
			planRow.LotSerialNbr = origRow.LotSerialNbr;
			planRow.IsTempLotSerial = string.IsNullOrEmpty(origRow.AssignedNbr) == false &&
				INLotSerialNbrAttribute.StringsEqual(origRow.AssignedNbr, origRow.LotSerialNbr);


			if (planRow.IsTempLotSerial == true)
			{
				planRow.LotSerialNbr = null;
			}
			planRow.PlanQty = origRow.BaseQty;
			// if Plan Type is SO Shipped then we keep original Plan and Shipment Date
			planRow.PlanDate = !string.IsNullOrEmpty(origRow.SOLineType) ? origRow.TranDate : new DateTime(1900, 1, 1);
			if (header.DocType == INDocType.Transfer && header.TransferType == INTransferType.OneStep && header.SiteID == header.ToSiteID)
			{
				planRow.ExcludePlanLevel = !string.IsNullOrEmpty(planRow.LotSerialNbr) ? INPlanLevel.ExcludeSiteLotSerial : INPlanLevel.ExcludeSite;
			}
			else
			{
				planRow.ExcludePlanLevel = null;
			}

			planRow.RefNoteID = header.NoteID;
			planRow.Hold = header.Hold;

			switch (origRow.TranType)
			{
				case INTranType.Receipt:
				case INTranType.Return:
				case INTranType.CreditMemo:
					if (origRow.Released == true)
					{
						return null;
					}

					planRow.PlanType =
						(origRow.SOLineType != null) ? INPlanConstants.Plan62 :
						(origRow.POLineType == PO.POLineType.GoodsForSalesOrder) ? INPlanConstants.Plan77 :
						(origRow.POLineType == PO.POLineType.GoodsForServiceOrder) ? INPlanConstants.PlanF9 :
						(origRow.POLineType == PO.POLineType.GoodsForDropShip) ? INPlanConstants.Plan75 :
						INPlanConstants.Plan10;
					break;
				case INTranType.Issue:
				case INTranType.Invoice:
				case INTranType.DebitMemo:
					if (origRow.Released == true)
					{
						return null;
					}

					planRow.PlanType = (origRow.SOLineType == null) ? INPlanConstants.Plan20 : INPlanConstants.Plan62;
					break;
				case INTranType.Transfer:
					if (origRow.InvtMult == -1)
					{
						if (origRow.TransferType == INTransferType.OneStep)
						{
							if (origRow.Released == true)
							{
								return null;
							}

							planRow.PlanType = INPlanConstants.Plan40;
						}
						else if (origRow.Released == true)
						{
							planRow.PlanType = origRow.IsFixedInTransit == true ? INPlanConstants.Plan44 : INPlanConstants.Plan42;
							planRow.SiteID = origRow.ToSiteID;
							planRow.LocationID = origRow.ToLocationID;
						}
						else
						{
							planRow.PlanType = (origRow.SOLineType == null) ? INPlanConstants.Plan41 : INPlanConstants.Plan62;
						}
					}
					else
					{
						if (origRow.Released == true)
						{
							return null;
						}

						planRow.PlanType = INPlanConstants.Plan43;
						if (string.IsNullOrEmpty(planRow.OrigPlanType))
						{
							planRow.OrigPlanType = INPlanConstants.Plan42;
						}
					}
					break;
				case INTranType.Assembly:
				case INTranType.Disassembly:
					if (origRow.Released == true)
					{
						return null;
					}

					if (origRow.InvtMult == (short)-1)
					{
						planRow.PlanType = INPlanConstants.Plan50;
					}
					else
					{
						planRow.PlanType = INPlanConstants.Plan51;
					}
					break;
				case INTranType.Adjustment:
				case INTranType.StandardCostAdjustment:
				case INTranType.NegativeCostAdjustment:
				default:
					return null;
			}

			return planRow;
		}

		public virtual bool? IsTwoStepTransferPlanValid(TItemPlanSource split, INItemPlan plan)
		{
			if (split.DocType != INDocType.Transfer || split.TranType != INTranType.Transfer
				|| split.TransferType != INTransferType.TwoStep || split.InvtMult != -1m || split.Released == true)
				return null;

			if (split.SOLineType == null)
				return plan?.PlanType == INPlanConstants.Plan41;

			return plan?.PlanType == INPlanConstants.Plan62;
		}
	}
}
