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
using System.Text;
using PX.Common;
using PX.Objects.Common.Extensions;
using PX.Data;
using PX.Objects.CS;
using PX.Objects.GL;
using PX.Objects.CM;
using PX.Objects.IN.GraphExtensions.INReleaseProcessExt;
using PX.Objects.IN.PhysicalInventory;
using PX.Objects.IN.Services;
using PX.Objects.SO;
using PX.Objects.AP;
using PX.Objects.GL.FinPeriods;
using PX.Objects.PM;
using PX.Objects.PO;
using PX.Objects.PO.LandedCosts;
using PX.Data.BQL.Fluent;
using PX.Data.BQL;
using PX.Objects.IN.Attributes;

namespace PX.Objects.IN.InventoryRelease
{
	using Accumulators;
	using Accumulators.CostStatuses;
	using Accumulators.Documents;
	using Accumulators.ItemHistory;
	using Accumulators.QtyAllocated;
	using Accumulators.Statistics.Item;
	using Accumulators.Statistics.ItemCustomer;
	using DAC;
	using Exceptions;
	using Utility;

	public delegate List<PXResult<INItemPlan, INPlanType>> OnBeforeSalesOrderProcessPOReceipt(PXGraph graph, IEnumerable<PXResult<INItemPlan, INPlanType>> list, string POReceiptType, string POReceiptNbr);

    public class INReleaseProcess : PXGraph<INReleaseProcess>
    {
		public INTranSplitPlan TranSplitPlanExt => FindImplementation<INTranSplitPlan>();

        public OnBeforeSalesOrderProcessPOReceipt onBeforeSalesOrderProcessPOReceipt;

        public PXSelect<INCostSubItemXRef> costsubitemxref;
        public PXSelect<INItemSite> initemsite;

        public PXSelect<OversoldCostStatus> oversoldcoststatus;
        public PXSelect<UnmanagedCostStatus> unmanagedcoststatus;
        public PXSelect<FIFOCostStatus> fifocoststatus;
        public PXSelect<AverageCostStatus> averagecoststatus;
        public PXSelect<StandardCostStatus> standardcoststatus;
        public PXSelect<SpecificCostStatus> specificcoststatus;
        public PXSelect<SpecificTransitCostStatus> specifictransitcoststatus;
        public PXSelect<ReceiptStatus> receiptstatus;
		public PXSelect<LotSerialStatusByCostCenter> lotnumberedstatusbycostcenter;
		public PXSelect<ItemLotSerial> itemlotserial;
        public PXSelect<SiteLotSerial> sitelotserial;
		public PXSelect<LocationStatusByCostCenter> locationstatusbycostcenter;
		public PXSelect<SiteStatusByCostCenter> sitestatusbycostcenter;

		#region InTransit Statuses
		public PXSelect<INTransitLine> intransitline;
		public PXSelect<TransitSiteStatusByCostCenter> transitsitestatusbycostcenter;
		public PXSelect<TransitLocationStatusByCostCenter> transitlocationstatusbycostcenter;
		public PXSelect<TransitLotSerialStatusByCostCenter> transitlotnumberedstatusbycostcenter;
		#endregion

		public PXSelect<ItemStats> itemstats;
        public PXSelect<ItemCost> itemcost;

        public PXSelect<ItemSiteHist> itemsitehist;
		public PXSelect<ItemSiteHistByCostCenterD> itemsitehistbycostcenterd;
        public PXSelect<ItemSiteHistDay> itemsitehistday;
		public PXSelect<ItemCostHist> itemcosthist;
        public PXSelect<ItemSalesHist> itemsaleshist;
        public PXSelect<ItemCustSalesHist> itemcustsaleshist;
        public PXSelect<ItemCustSalesStats> itemcustsalesstats;
        public PXSelect<ItemCustDropShipStats> itemcustdropshipstats;
        public PXSelect<ItemSalesHistD> itemsaleshistd;

        public PXSelect<INRegister> inregister;
        public PXSelect<INTran> intranselect;
        public PXSelect<INTranSplit> intransplit;
        public PXAccumSelect<INTranCost> intrancost;

        public PXSelect<SOShipLineUpdate> soshiplineupdate;
        public PXSelect<ARTranUpdate> artranupdate;
        public PXSelect<POReceiptLineUpdate> poreceiptlineupdate;
        public PXSelect<INTranUpdate> intranupdate;
        public PXSelect<INTranCostUpdate> intrancostupdate;
        public PXSelect<INTranSplitAdjustmentUpdate> intransplitadjustmentupdate;
        public PXSelect<INTranSplitUpdate> intransplitupdate;
        public PXSelect<SOLineSplit> solinesplit;
        public PXSelect<SOOrder> soorder;
        public PXSelect<INItemLotSerial> initemlotserialreadonly;

        public PXSetup<INSetup> insetup;
        public PXSetup<Company> companysetup;

		[InjectDependency]
		public IFinPeriodUtils FinPeriodUtils { get; set; }

        [InjectDependency]
        public IInventoryAccountService InventoryAccountService { get; set; }

        public bool AutoPost
        {
            get
            {
                return (bool)insetup.Current.AutoPost;
            }
        }

        public bool UpdateGL
        {
            get
            {
                return (bool)insetup.Current.UpdateGL;
            }
        }

        public bool SummPost
        {
            get
            {
                return (bool)insetup.Current.SummPost;
            }
        }

        protected ReasonCode _ReceiptReasonCode;
        public ReasonCode ReceiptReasonCode
        {
            get
            {
                if (this._ReceiptReasonCode == null)
                {
                    _ReceiptReasonCode = ReasonCode.PK.Find(this, insetup.Current.ReceiptReasonCode);
                }
                return _ReceiptReasonCode;
            }
        }

        protected ReasonCode _IssuesReasonCode;
        public ReasonCode IssuesReasonCode
        {
            get
            {
                if (this._IssuesReasonCode == null)
                {
                    _IssuesReasonCode = ReasonCode.PK.Find(this, insetup.Current.IssuesReasonCode);
                }
                return _IssuesReasonCode;
            }
        }

        protected ReasonCode _AdjustmentReasonCode;
        public ReasonCode AdjustmentReasonCode
        {
            get
            {
                if (this._AdjustmentReasonCode == null)
                {
                    _AdjustmentReasonCode = ReasonCode.PK.Find(this, insetup.Current.AdjustmentReasonCode);
                }
                return _AdjustmentReasonCode;
            }
        }

		public Int32? ARClearingAcctID
        {
            get
            {
                return insetup.Current.ARClearingAcctID;
            }
        }

        public Int32? ARClearingSubID
        {
            get
            {
                return insetup.Current.ARClearingSubID;
            }
        }

        public Int32? INTransitSiteID
        {
            get
            {
                return insetup.Current.TransitSiteID;
            }
        }

        public Int32? INTransitAcctID
        {
            get
            {
                return insetup.Current.INTransitAcctID;
            }
        }

        public Int32? INTransitSubID
        {
            get
            {
                return insetup.Current.INTransitSubID;
            }
        }

        public Int32? INProgressAcctID
        {
            get
            {
                return insetup.Current.INProgressAcctID;
            }
        }

        public Int32? INProgressSubID
        {
            get
            {
                return insetup.Current.INProgressSubID;
            }
        }

        protected PXCache<INTranCost> transfercosts;

        public override void Clear()
        {
            base.Clear();
			Clear(PXClearOption.ClearQueriesOnly);

            if (transfercosts != null)
            {
                transfercosts.Clear();
            }
            WIPCalculated = false;
            WIPVariance = 0m;
        }

        public INReleaseProcess()
        {
            INSetup setup = insetup.Current;

            transfercosts = new PXNoEventsCache<INTranCost>(this);

            PXDBDefaultAttribute.SetDefaultForInsert<INTran.docType>(intranselect.Cache, null, false);
            PXDBDefaultAttribute.SetDefaultForInsert<INTran.refNbr>(intranselect.Cache, null, false);
            PXDBDefaultAttribute.SetDefaultForInsert<INTran.tranDate>(intranselect.Cache, null, false);
            PXDBDefaultAttribute.SetDefaultForInsert<INTran.origModule>(intranselect.Cache, null, false);

			PXDBDefaultAttribute.SetDefaultForInsert<INTranSplit.refNbr>(intransplit.Cache, null, false);
            PXDBDefaultAttribute.SetDefaultForInsert<INTranSplit.tranDate>(intransplit.Cache, null, false);
            PXDBDefaultAttribute.SetDefaultForInsert<INTranSplit.origModule>(intransplit.Cache, null, false);

			PXDBDefaultAttribute.SetDefaultForUpdate<INTran.docType>(intranselect.Cache, null, false);
            PXDBDefaultAttribute.SetDefaultForUpdate<INTran.refNbr>(intranselect.Cache, null, false);
            PXDBDefaultAttribute.SetDefaultForUpdate<INTran.tranDate>(intranselect.Cache, null, false);
			PXDBDefaultAttribute.SetDefaultForUpdate<INTran.origModule>(intranselect.Cache, null, false);

			intranselect.Cache.Adjust<FinPeriodIDAttribute>()
                .For<INTran.finPeriodID>(attr =>
                {
                    attr.HeaderFindingMode = FinPeriodIDAttribute.HeaderFindingModes.Parent;
                });

            PXDBDefaultAttribute.SetDefaultForUpdate<INTranSplit.refNbr>(intransplit.Cache, null, false);
            PXDBDefaultAttribute.SetDefaultForUpdate<INTranSplit.tranDate>(intransplit.Cache, null, false);
            PXDBDefaultAttribute.SetDefaultForUpdate<INTranSplit.origModule>(intransplit.Cache, null, false);

			OpenPeriodAttribute.SetValidatePeriod<INRegister.finPeriodID>(inregister.Cache, null, PeriodValidation.Nothing);

			// Acuminator disable once PX1085 DatabaseQueriesInPXGraphInitialization [Legacy code]
			ParseSubItemSegKeys();

            PXDimensionSelectorAttribute.SetSuppressViewCreation(intranselect.Cache);
            PXDimensionSelectorAttribute.SetSuppressViewCreation(intrancost.Cache);

            PXFormulaAttribute.SetAggregate<INTran.qty>(intranselect.Cache, null);
            PXFormulaAttribute.SetAggregate<INTran.tranCost>(intranselect.Cache, null);
        }

        public override void InitCacheMapping(Dictionary<Type, Type> map)
        {
            base.InitCacheMapping(map);

            Caches.AddCacheMapping(typeof(INCostStatus), typeof(INCostStatus));
        }

        public virtual JournalEntry CreateJournalEntry()
		{
			var je = PXGraph.CreateInstance<JournalEntry>();

			//Field Verification can fail if GL module is not "Visible";therfore suppress it:
			je.FieldVerifying.AddHandler<GLTran.projectID>((PXCache sender, PXFieldVerifyingEventArgs e) => { e.Cancel = true; });
			je.FieldVerifying.AddHandler<GLTran.taskID>((PXCache sender, PXFieldVerifyingEventArgs e) => { e.Cancel = true; });
			//Uncomment if posting of empty transactions for NS items is really needed.
			//je.RowInserting.AddHandler<GLTran>((sender, e) => { ((GLTran)e.Row).ZeroPost = ((GLTran)e.Row).ZeroPost ?? ("NP".IndexOf(((GLTran)e.Row).TranClass) >= 0 && ((GLTran)e.Row).AccountID != null && ((GLTran)e.Row).SubID != null); });

			if (UpdateGL)
			{
				PXCache subCache = Caches[typeof(Sub)];
				Caches[typeof(Sub)] = je.Caches[typeof(Sub)];

				je.RowPersisting.AddHandler<Sub>((cache, e) => 
					subCache.RaiseRowPersisting(e.Row, e.Operation));

				je.RowPersisted.AddHandler<Sub>((cache, e) =>
					subCache.RaiseRowPersisted(e.Row, e.Operation, e.TranStatus, e.Exception));
			}

			return je;
		}

		public virtual PostGraph CreatePostGraph()
		{
			return PXGraph.CreateInstance<PostGraph>();
		}

		protected virtual PILocksInspector CreateLocksInspector(int siteID)
		{
			return new PILocksInspector(siteID);
		}

        protected virtual void StandardCostStatus_UnitCost_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
        {
            StandardCostStatus tran = e.Row as StandardCostStatus;

            if (tran != null)
            {
                INItemSite itemsite = SelectItemSite(sender.Graph, tran.InventoryID, tran.CostSiteID);

                if (itemsite != null)
                {
                    e.NewValue = itemsite.StdCost;
                    e.Cancel = true;
                }
            }
        }

        //all descendants of INCostStatus should have this handler
        long _CostStatus_Identity = long.MinValue;
        protected virtual void StandardCostStatus_CostID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
        {
            e.NewValue = _CostStatus_Identity++;
            e.Cancel = true;
        }

        protected virtual void AverageCostStatus_CostID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
        {
            e.NewValue = _CostStatus_Identity++;
            e.Cancel = true;
        }

        protected virtual void FIFOCostStatus_CostID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
        {
            e.NewValue = _CostStatus_Identity++;
            e.Cancel = true;
        }

        protected virtual void SpecificCostStatus_CostID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
        {
            e.NewValue = _CostStatus_Identity++;
            e.Cancel = true;
        }

        protected virtual void OversoldCostStatus_CostID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
        {
            e.NewValue = _CostStatus_Identity++;
            e.Cancel = true;
        }

        [IN.LocationAvail(typeof(INTranSplit.inventoryID), typeof(INTranSplit.subItemID), typeof(INTran.costCenterID), typeof(INTranSplit.siteID), typeof(INTranSplit.tranType), typeof(INTranSplit.invtMult))]
        public virtual void INTranSplit_LocationID_CacheAttached(PXCache sender)
        {
        }

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXRemoveBaseAttribute(typeof(PXDBDefaultAttribute))]
		[PXDefault()]
        protected virtual void SOLineSplit_OrderNbr_CacheAttached(PXCache sender)
        {
        }

        [PXDBDate()]
        [PXDefault()]
        protected virtual void SOLineSplit_OrderDate_CacheAttached(PXCache sender)
        {
        }

        [PXDBInt()]
        protected virtual void SOLineSplit_SiteID_CacheAttached(PXCache sender)
        {
        }

        [PXDBInt()]
        protected virtual void SOLineSplit_LocationID_CacheAttached(PXCache sender)
        {
        }

		[PXMergeAttributes]
		[PXCustomizeBaseAttribute(typeof(PXParentAttribute), nameof(PXParentAttribute.UseCurrent), true)]
		protected virtual void _(Events.CacheAttached<INTran.refNbr> e) { }

		[PXMergeAttributes]
		[PXCustomizeBaseAttribute(typeof(PXParentAttribute), nameof(PXParentAttribute.UseCurrent), true)]
		protected virtual void _(Events.CacheAttached<INTranSplit.docType> e) { }

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXUnboundFormula(typeof(
			IIf<Where<INTranCost.costType, In3<INTranCost.costType.normal, INTranCost.costType.dropShip>>, INTranCost.qty, decimal0>),
			typeof(SumCalc<INTran.costedQty>))]
		protected virtual void _(Events.CacheAttached<INTranCost.qty> e)
		{
		}

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXUnboundFormula(typeof(
			IIf<Where<INTranCost.costType, In3<INTranCost.costType.normal, INTranCost.costType.dropShip>>, INTranCost.tranCost, decimal0>),
			typeof(SumCalc<INTran.tranCost>))]
		protected virtual void _(Events.CacheAttached<INTranCost.tranCost> e)
		{
		}

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXUnboundFormula(typeof(
			IIf<Where<INTranCost.costType, In3<INTranCost.costType.normal, INTranCost.costType.dropShip>>, INTranCost.tranAmt, decimal0>),
			typeof(SumCalc<INTran.tranAmt>))]
		protected virtual void _(Events.CacheAttached<INTranCost.tranAmt> e)
		{
		}

        protected virtual void INTran_UnitCost_CommandPreparing(PXCache sender, PXCommandPreparingEventArgs e)
        {
            if ((e.Operation & PXDBOperation.Command) == PXDBOperation.Update && ((INTran)e.Row).OverrideUnitCost != true)
            {
                e.ExcludeFromInsertUpdate();
            }
        }

        protected virtual void INTran_UnitPrice_CommandPreparing(PXCache sender, PXCommandPreparingEventArgs e)
        {
            if ((e.Operation & PXDBOperation.Command) == PXDBOperation.Update)
            {
                e.ExcludeFromInsertUpdate();
            }
        }

        protected virtual void INRegister_TotalQty_CommandPreparing(PXCache sender, PXCommandPreparingEventArgs e)
        {
            if ((e.Operation & PXDBOperation.Command) == PXDBOperation.Update)
            {
                e.ExcludeFromInsertUpdate();
            }
        }

        protected virtual void INRegister_TotalAmount_CommandPreparing(PXCache sender, PXCommandPreparingEventArgs e)
        {
            if ((e.Operation & PXDBOperation.Command) == PXDBOperation.Update)
            {
                e.ExcludeFromInsertUpdate();
            }
        }

        protected virtual void INRegister_TotalCost_CommandPreparing(PXCache sender, PXCommandPreparingEventArgs e)
        {
			if ((e.Operation & PXDBOperation.Command) == PXDBOperation.Update && ((INRegister)e.Row).DocType.IsNotIn(INDocType.Issue, INDocType.Adjustment))
            {
                e.ExcludeFromInsertUpdate();
            }
        }

        public virtual void INItemSite_InvtAcctID_CommandPreparing(PXCache sender, PXCommandPreparingEventArgs e)
        {
            if ((e.Operation & PXDBOperation.Command) == PXDBOperation.Insert || (e.Operation & PXDBOperation.Command) == PXDBOperation.Update)
            {
                if (((INItemSite)e.Row).OverrideInvtAcctSub != true)
                {
                    e.ExcludeFromInsertUpdate();
                }
            }
        }

        public virtual void INItemSite_InvtSubID_CommandPreparing(PXCache sender, PXCommandPreparingEventArgs e)
        {
            if ((e.Operation & PXDBOperation.Command) == PXDBOperation.Insert || (e.Operation & PXDBOperation.Command) == PXDBOperation.Update)
            {
                if (((INItemSite)e.Row).OverrideInvtAcctSub != true)
                {
                    e.ExcludeFromInsertUpdate();
                }
            }
        }

        public virtual void UpdateSiteStatus(INTran tran, INTranSplit split, INLocation whseloc)
        {
			var itemByCostCenter = new SiteStatusByCostCenter
			{
				InventoryID = split.InventoryID,
				SubItemID = split.SubItemID,
				SiteID = split.SiteID,
				CostCenterID = tran.CostCenterID,
			};

			itemByCostCenter = (SiteStatusByCostCenter)this.Caches<SiteStatusByCostCenter>().Insert(itemByCostCenter);

			itemByCostCenter.QtyOnHand += (decimal)split.InvtMult * (decimal)split.BaseQty;
			itemByCostCenter.QtyAvail += whseloc.InclQtyAvail == true ? (decimal)split.InvtMult * (decimal)split.BaseQty : 0m;
			itemByCostCenter.QtyHardAvail += whseloc.InclQtyAvail == true ? (decimal)split.InvtMult * (decimal)split.BaseQty : 0m;
			itemByCostCenter.QtyActual += whseloc.InclQtyAvail == true ? (decimal)split.InvtMult * (decimal)split.BaseQty : 0m;
			itemByCostCenter.QtyNotAvail += whseloc.InclQtyAvail == true ? 0m : (decimal)split.InvtMult * (decimal)split.BaseQty;
			itemByCostCenter.SkipQtyValidation = split.SkipQtyValidation;
			itemByCostCenter.ValidateHardAvailQtyForAdjustments = inregister.Current.IgnoreAllocationErrors != true
				&& (itemByCostCenter.ValidateHardAvailQtyForAdjustments.Value || split.DocType == INDocType.Adjustment && split.BaseQty > 0m && split.InvtMult == -1);

			if (tran.TranType == INTranType.Transfer && !IsOneStepTransfer())
            {
				TransitSiteStatusByCostCenter tranitembycostcenter = new TransitSiteStatusByCostCenter();

				tranitembycostcenter.InventoryID = split.InventoryID;
				tranitembycostcenter.SubItemID = split.SubItemID;
				tranitembycostcenter.SiteID = this.INTransitSiteID;
				tranitembycostcenter.CostCenterID = !IsIngoingTransfer(tran) ? tran.ToCostCenterID : tran.CostCenterID;

				tranitembycostcenter = transitsitestatusbycostcenter.Insert(tranitembycostcenter);

				tranitembycostcenter.QtyOnHand -= (decimal)split.InvtMult * (decimal)split.BaseQty;
				tranitembycostcenter.QtyAvail -= (decimal)split.InvtMult * (decimal)split.BaseQty;
				tranitembycostcenter.QtyHardAvail -= (decimal)split.InvtMult * (decimal)split.BaseQty;
				tranitembycostcenter.QtyActual -= (decimal)split.InvtMult * (decimal)split.BaseQty;
			}
        }

        public virtual void UpdateLocationStatus(INTran tran, INTranSplit split)
        {
			if (split.InvtMult == 0)
				return;
			if (this.CreateLocksInspector(split.SiteID.Value)
				.IsInventoryLocationLocked(split.InventoryID, split.LocationID, inregister.Current.PIID, out string PIID))
			{
                PXCache cache = this.Caches[typeof(INTranSplit)];

				throw new PXException(Messages.InventoryItemIsLockedInPIDocument,
									  PXForeignSelectorAttribute.GetValueExt<INTranSplit.inventoryID>(cache, split),
									  PXForeignSelectorAttribute.GetValueExt<INTranSplit.locationID>(cache, split),
									  PXForeignSelectorAttribute.GetValueExt<INTranSplit.siteID>(cache, split),
									  PIID);
			}

			var itemByCostCenter = new LocationStatusByCostCenter
			{
				InventoryID = split.InventoryID,
				SubItemID = split.SubItemID,
				SiteID = split.SiteID,
				LocationID = split.LocationID,
				CostCenterID = tran.CostCenterID,
				RelatedPIID = inregister.Current.PIID,
			};

			itemByCostCenter = (LocationStatusByCostCenter)this.Caches<LocationStatusByCostCenter>().Insert(itemByCostCenter);

			itemByCostCenter.NegQty = (split.TranType == INTranType.Adjustment) ? false : itemByCostCenter.NegQty;
			itemByCostCenter.QtyOnHand += (decimal)split.InvtMult * (decimal)split.BaseQty;
			itemByCostCenter.QtyAvail += (decimal)split.InvtMult * (decimal)split.BaseQty;
			itemByCostCenter.QtyHardAvail += (decimal)split.InvtMult * (decimal)split.BaseQty;
			itemByCostCenter.QtyActual += (decimal)split.InvtMult * (decimal)split.BaseQty;
			itemByCostCenter.SkipQtyValidation = split.SkipQtyValidation;

            if (tran.TranType == INTranType.Transfer && !IsOneStepTransfer())
            {
                TwoStepTransferNonLot(tran, split);
            }
        }

        protected virtual TransitLocationStatusByCostCenter TwoStepTransferNonLot(INTran tran, INTranSplit split)
        {
			INTransitLine transitLine;

			if (IsIngoingTransfer(tran))
            {
                transitLine = new INTransitLine();
				transitLine.TransferNbr = tran.OrigRefNbr;
				transitLine.TransferLineNbr = tran.OrigLineNbr;
				transitLine = intransitline.Locate(transitLine);
            }
            else
            {
				transitLine = GetTransitLine(tran);
            }

			TransitLocationStatusByCostCenter tranitembycostcenter = new TransitLocationStatusByCostCenter();
			tranitembycostcenter.InventoryID = split.InventoryID;
			tranitembycostcenter.SubItemID = split.SubItemID;
			tranitembycostcenter.SiteID = this.INTransitSiteID;
			tranitembycostcenter.LocationID = transitLine.CostSiteID;
			tranitembycostcenter.CostCenterID = !IsIngoingTransfer(tran) ? tran.ToCostCenterID : tran.CostCenterID;

			tranitembycostcenter = transitlocationstatusbycostcenter.Insert(tranitembycostcenter);

			tranitembycostcenter.QtyOnHand -= (decimal)split.InvtMult * (decimal)split.BaseQty;
			tranitembycostcenter.QtyAvail -= (decimal)split.InvtMult * (decimal)split.BaseQty;
			tranitembycostcenter.QtyHardAvail -= (decimal)split.InvtMult * (decimal)split.BaseQty;
			tranitembycostcenter.QtyActual -= (decimal)split.InvtMult * (decimal)split.BaseQty;

			return tranitembycostcenter;
        }

        public virtual INTransitLine GetTransitLine(INTran tran)
        {
            var ntl = new INTransitLine();
            ntl.TransferNbr = tran.RefNbr;
            ntl.TransferLineNbr = tran.LineNbr;
            var ntlloc = intransitline.Locate(ntl);
            if (ntlloc == null)
            {
                ntl.SOOrderType = tran.SOOrderType;
                ntl.SOOrderNbr = tran.SOOrderNbr;
                ntl.SOOrderLineNbr = tran.SOOrderLineNbr;
                ntl.SOShipmentType = tran.SOShipmentType;
                ntl.SOShipmentNbr = tran.SOShipmentNbr;
                ntl.SOShipmentLineNbr = tran.SOShipmentLineNbr;
                ntl.OrigModule = tran.SOOrderNbr == null ? GL.BatchModule.IN : GL.BatchModule.SO;
                ntl.SiteID = tran.SiteID;
                ntl.ToSiteID = tran.ToSiteID;
                ntl.RefNoteID = this.inregister.Current.NoteID;

                INTranSplit split = PXSelectReadonly<INTranSplit,
		            Where<INTranSplit.refNbr, Equal<Current<INTran.refNbr>>,
			        And<INTranSplit.lineNbr, Equal<Current<INTran.lineNbr>>,
				    And<INTranSplit.docType, Equal<Current<INTran.docType>>>>>>.SelectSingleBound(this, new object[] { tran });

				INSite tosite = INSite.PK.Find(this, tran.ToSiteID);
				INItemSite itemsite = INItemSite.PK.Find(this, tran.InventoryID, tosite?.SiteID);

				ntl.IsLotSerial = !String.IsNullOrEmpty(split?.LotSerialNbr);
                ntl.ToLocationID = tran.ToLocationID ?? itemsite?.DfltReceiptLocationID ?? tosite.ReceiptLocationID;

                //generate noteid for plan linking
                ntl.NoteID = PXNoteAttribute.GetNoteID<INTransitLine.noteID>(intransitline.Cache, ntl).Value;
                foreach (Note note in this.Caches[typeof(Note)].Inserted)
                {
                    if (note.NoteID == ntl.NoteID)
                    {
                        note.GraphType = typeof(INTransferEntry).FullName;
                        break;
                    }
                }
                ntl = intransitline.Insert(ntl);
            }
            else
                ntl = ntlloc;
            return ntl;
        }

		public virtual LotSerialStatusByCostCenter AccumulatedLotSerialStatusByCostCenter(INTran tran, INTranSplit split, INLotSerClass lsclass)
		{
			LotSerialStatusByCostCenter ret = new LotSerialStatusByCostCenter();
			ret.InventoryID = split.InventoryID;
			ret.SubItemID = split.SubItemID;
			ret.SiteID = split.SiteID;
			ret.LocationID = split.LocationID;
			ret.LotSerialNbr = split.LotSerialNbr;
			ret.CostCenterID = !IsIngoingTransfer(tran) ? tran.ToCostCenterID : tran.CostCenterID;
			ret = lotnumberedstatusbycostcenter.Insert(ret);
			if (ret.ExpireDate == null)
			{
				ret.ExpireDate = split.ExpireDate;
			}
			if (ret.ReceiptDate == null)
			{
				ret.ReceiptDate = split.TranDate;
			}
			ret.LotSerTrack = lsclass.LotSerTrack;

			return ret;
		}

		public virtual TransitLotSerialStatusByCostCenter AccumulatedTransitLotSerialStatusByCostCenter(
			INTran tran, INTranSplit split, INLotSerClass lsclass, INTransitLine tl)
		{
			TransitLotSerialStatusByCostCenter ret = new TransitLotSerialStatusByCostCenter();

			ret.InventoryID = split.InventoryID;
			ret.SubItemID = split.SubItemID;
			ret.SiteID = this.INTransitSiteID;
			ret.LocationID = tl.CostSiteID;
			ret.LotSerialNbr = split.LotSerialNbr;
			ret.CostCenterID = !IsIngoingTransfer(tran) ? tran.ToCostCenterID : tran.CostCenterID;
			ret = transitlotnumberedstatusbycostcenter.Insert(ret);
			if (ret.ExpireDate == null)
			{
				ret.ExpireDate = split.ExpireDate;
			}
			if (ret.ReceiptDate == null)
			{
				ret.ReceiptDate = split.TranDate;
			}
			ret.LotSerTrack = lsclass.LotSerTrack;

			return ret;
		}

		public virtual void ReceiveLot(INTran tran, INTranSplit split, InventoryItem item, INLotSerClass lsclass)
        {
            if (split.InvtMult == (short)1 && !IsOneStepTransfer())
            {
                if (lsclass.LotSerTrack != INLotSerTrack.NotNumbered &&
					(lsclass.LotSerAssign == INLotSerAssign.WhenReceived)
                )
                {
					UpdateLotSerialStatusByCostCenter(tran, split, lsclass);
                }
            }
        }

        public virtual void IssueLot(INTran tran, INTranSplit split, InventoryItem item, INLotSerClass lsclass)
        {
            if (split.InvtMult == -1)
            {
                //for when used serial numbers numbers will mark processed numbers with trandate
				if (INLotSerialNbrAttribute.IsTrackSerial(lsclass, tran.TranType, tran.InvtMult) || 
                    lsclass.LotSerTrack != INLotSerTrack.NotNumbered && lsclass.LotSerAssign == INLotSerAssign.WhenReceived)
                {
                    if (lsclass.LotSerAssign == INLotSerAssign.WhenReceived)
                    {
						UpdateLotSerialStatusByCostCenter(tran, split, lsclass);
                    }
                }
            }
        }

        public virtual void TransferLot(INTran tran, INTranSplit split, InventoryItem item, INLotSerClass lsclass)
        {
            if (IsOneStepTransfer() && split.InvtMult == (short)1 && tran.OrigLineNbr != null)
            {
				if (lsclass.LotSerTrack != INLotSerTrack.NotNumbered && lsclass.LotSerAssign == INLotSerAssign.WhenReceived)
                {
                    OneStepTransferLot(tran, split, item, lsclass);
                }
            }

            if (tran.TranType == INTranType.Transfer && !IsOneStepTransfer())
            {
                if (lsclass.LotSerTrack != INLotSerTrack.NotNumbered && lsclass.LotSerAssign == INLotSerAssign.WhenReceived)
                {
                    TwoStepTransferLot(tran, split, item, lsclass);
                }
            }
        }

        protected virtual LotSerialStatusByCostCenter OneStepTransferLot(INTran tran, INTranSplit split, InventoryItem item, INLotSerClass lsclass)
        {
            PXResult res = PXSelectJoin<INTranSplit,
				InnerJoin<INTran, On<INTranSplit.FK.Tran>,
                InnerJoin<INLotSerialStatusByCostCenter,
                    On<INLotSerialStatusByCostCenter.inventoryID, Equal<INTranSplit.inventoryID>,
					And<INLotSerialStatusByCostCenter.subItemID, Equal<INTranSplit.subItemID>,
                    And<INLotSerialStatusByCostCenter.siteID, Equal<INTranSplit.siteID>,
                    And<INLotSerialStatusByCostCenter.locationID, Equal<INTranSplit.locationID>,
                    And<INLotSerialStatusByCostCenter.lotSerialNbr, Equal<INTranSplit.lotSerialNbr>,
					And<INLotSerialStatusByCostCenter.costCenterID, Equal<INTran.costCenterID>>>>>>>>>,
                Where<INTranSplit.docType, Equal<Current<INTran.origDocType>>,
					And<INTranSplit.refNbr, Equal<Current<INTran.origRefNbr>>,
					And<INTranSplit.lineNbr, Equal<Current<INTran.origLineNbr>>,
					And<INTranSplit.lotSerialNbr, Equal<Current<INTranSplit.lotSerialNbr>>>>>>>
				.SelectSingleBound(this, new object[] { tran, split });

			DateTime? overrideReceiptDate = res?.GetItem<INLotSerialStatusByCostCenter>()?.ReceiptDate;
			var lsitem = UpdateLotSerialStatusByCostCenter(tran, split, lsclass, overrideReceiptDate);

            return lsitem;
        }

        protected virtual TransitLotSerialStatusByCostCenter TwoStepTransferLot(INTran tran, INTranSplit split, InventoryItem item, INLotSerClass lsclass)
        {
			TransitLotSerialStatusByCostCenter lstritembycostcenter;
			DateTime? overrideReceiptDate;
            if (IsIngoingTransfer(tran))
            {
                var trl = new INTransitLine();
                trl.TransferNbr = tran.OrigRefNbr;
                trl.TransferLineNbr = tran.OrigLineNbr;
                trl = intransitline.Locate(trl);

				lstritembycostcenter = AccumulatedTransitLotSerialStatusByCostCenter(tran, split, lsclass, trl);

				var res = INLotSerialStatusByCostCenter.PK.Find(this, split.InventoryID, split.SubItemID, insetup.Current.TransitSiteID, trl.CostSiteID, split.LotSerialNbr, tran.CostCenterID);
				overrideReceiptDate = res?.ReceiptDate;
				if (overrideReceiptDate != null)
                {
					TwoStepTransferLotComplement(tran, split, item, lsclass, overrideReceiptDate.Value);
                }
            }
            else
            {
                INTransitLine ntl = GetTransitLine(tran);

				lstritembycostcenter = AccumulatedTransitLotSerialStatusByCostCenter(tran, split, lsclass, ntl);

				var res = INLotSerialStatusByCostCenter.PK.Find(this, split.InventoryID, split.SubItemID, split.SiteID, split.LocationID, split.LotSerialNbr, tran.CostCenterID);
				overrideReceiptDate = res?.ReceiptDate;
				if (overrideReceiptDate != null)
				{
					lstritembycostcenter.ReceiptDate = overrideReceiptDate;
				}
            }

			lstritembycostcenter.QtyOnHand -= (decimal)split.InvtMult * (decimal)split.BaseQty;
			lstritembycostcenter.QtyAvail -= (decimal)split.InvtMult * (decimal)split.BaseQty;
			lstritembycostcenter.QtyHardAvail -= (decimal)split.InvtMult * (decimal)split.BaseQty;
			lstritembycostcenter.QtyActual -= (decimal)split.InvtMult * (decimal)split.BaseQty;

			return lstritembycostcenter;
        }

        protected virtual LotSerialStatusByCostCenter TwoStepTransferLotComplement(INTran tran, INTranSplit split, InventoryItem item, INLotSerClass lsclass, DateTime receiptDate)
        {
			LotSerialStatusByCostCenter complementlsitembycostcenter = AccumulatedLotSerialStatusByCostCenter(tran, split, lsclass);
			complementlsitembycostcenter.ReceiptDate = receiptDate;

			return complementlsitembycostcenter;
        }

        public virtual INItemLotSerial UpdateItemLotSerial(INTran tran, INTranSplit split, InventoryItem item, INLotSerClass lsclass)
        {
			if (IsOneStepTransferWithinSite()) return null;

            if (split.InvtMult == 1 &&
				!string.IsNullOrEmpty(split.LotSerialNbr) &&
                    lsclass.LotSerTrack != INLotSerTrack.NotNumbered &&
				(lsclass.LotSerAssign == INLotSerAssign.WhenReceived
				|| lsclass.LotSerAssign == INLotSerAssign.WhenUsed && split.TranType.IsIn(INTranType.CreditMemo, INTranType.Return)))
            {
                ItemLotSerial lsitem = AccumulatedItemLotSerial(split, lsclass);

                if (tran.OrigLineNbr == null && lsitem.ExpireDate != null && lsitem.UpdateExpireDate == null)
                    lsitem.UpdateExpireDate = true;

                if (split.TranType == INTranType.Adjustment && split.InvtMult == 1 && split.ExpireDate != null)
                {
                    lsitem.UpdateExpireDate = true;
                    lsitem.ExpireDate = split.ExpireDate;
                }

                lsitem.QtyOnHand += (decimal)split.InvtMult * (decimal)split.BaseQty;
                lsitem.QtyAvail += (decimal)split.InvtMult * (decimal)split.BaseQty;
                lsitem.QtyHardAvail += (decimal)split.InvtMult * (decimal)split.BaseQty;
				lsitem.QtyActual += (decimal)split.InvtMult * (decimal)split.BaseQty;

				if (split.IsIntercompany == true)
					lsitem.IsIntercompany = true;

                return lsitem;
            }

            if (split.InvtMult == -1 &&
                    split.BaseQty != 0m &&
                    !string.IsNullOrEmpty(split.LotSerialNbr) &&
                    lsclass.LotSerTrack != INLotSerTrack.NotNumbered)
            {
                ItemLotSerial lsitem = AccumulatedItemLotSerial(split, lsclass);

                if (lsitem.ExpireDate != null && lsitem.UpdateExpireDate == null)
                    lsitem.UpdateExpireDate = lsclass.LotSerAssign == INLotSerAssign.WhenUsed;

                if (lsclass.LotSerTrack == INLotSerTrack.SerialNumbered ||
                        lsclass.LotSerAssign == INLotSerAssign.WhenReceived)
                {
                    lsitem.QtyOnHand += (decimal)split.InvtMult * (decimal)split.BaseQty;
                    lsitem.QtyAvail += (decimal)split.InvtMult * (decimal)split.BaseQty;
                    lsitem.QtyHardAvail += (decimal)split.InvtMult * (decimal)split.BaseQty;
					lsitem.QtyActual += (decimal)split.InvtMult * (decimal)split.BaseQty;
                }

				if (split.IsIntercompany == true)
					lsitem.IsIntercompany = true;

                return lsitem;
            }

            return null;
        }

        public virtual INSiteLotSerial UpdateSiteLotSerial(INTran tran, INTranSplit split, InventoryItem item, INLotSerClass lsclass)
        {
			if (IsOneStepTransferWithinSite()) return null;

            if (split.InvtMult == 1 &&
					!string.IsNullOrEmpty(split.LotSerialNbr) &&
                    lsclass.LotSerTrack != INLotSerTrack.NotNumbered &&
					(lsclass.LotSerAssign == INLotSerAssign.WhenReceived)
                )
            {
                SiteLotSerial lsitem = AccumulatedSiteLotSerial(split, lsclass);

                if (tran.OrigLineNbr == null && lsitem.ExpireDate != null && lsitem.UpdateExpireDate == null)
                    lsitem.UpdateExpireDate = true;

                if (split.TranType == INTranType.Adjustment && split.InvtMult == 1 && split.ExpireDate != null)
                {
                    lsitem.UpdateExpireDate = true;
                    lsitem.ExpireDate = split.ExpireDate;
                }

                lsitem.QtyOnHand += (decimal)split.InvtMult * (decimal)split.BaseQty;
                lsitem.QtyAvail += (decimal)split.InvtMult * (decimal)split.BaseQty;
                lsitem.QtyHardAvail += (decimal)split.InvtMult * (decimal)split.BaseQty;
				lsitem.QtyActual += (decimal)split.InvtMult * (decimal)split.BaseQty;

				INLocation location = INLocation.PK.Find(this, split.LocationID ?? tran.LocationID);
				if (location?.InclQtyAvail != false)
				{
					lsitem.QtyAvailOnSite += (decimal)split.InvtMult * (decimal)split.BaseQty;
				}
				else
				{
					lsitem.QtyNotAvail += (decimal)split.InvtMult * (decimal)split.BaseQty;
				}

				return lsitem;
            }

            if (split.InvtMult == -1 &&
                    split.BaseQty != 0m &&
                    !string.IsNullOrEmpty(split.LotSerialNbr) &&
                    lsclass.LotSerTrack != INLotSerTrack.NotNumbered)
            {
                SiteLotSerial lsitem = AccumulatedSiteLotSerial(split, lsclass);

				lsitem.ValidateHardAvailQtyForAdjustments = inregister.Current.IgnoreAllocationErrors != true
					&& (lsitem.ValidateHardAvailQtyForAdjustments.Value || split.DocType == INDocType.Adjustment && split.BaseQty > 0m && split.InvtMult == -1);

                if (lsitem.ExpireDate != null && lsitem.UpdateExpireDate == null)
                    lsitem.UpdateExpireDate = false;

                if (lsclass.LotSerTrack == INLotSerTrack.SerialNumbered ||
                        lsclass.LotSerAssign == INLotSerAssign.WhenReceived)
                {
                    lsitem.QtyOnHand += (decimal)split.InvtMult * (decimal)split.BaseQty;
                    lsitem.QtyAvail += (decimal)split.InvtMult * (decimal)split.BaseQty;
                    lsitem.QtyHardAvail += (decimal)split.InvtMult * (decimal)split.BaseQty;
					lsitem.QtyActual += (decimal)split.InvtMult * (decimal)split.BaseQty;

					INLocation location = INLocation.PK.Find(this, split.LocationID ?? tran.LocationID);
					if (location?.InclQtyAvail != false)
					{
						lsitem.QtyAvailOnSite += (decimal)split.InvtMult * (decimal)split.BaseQty;
					}
					else
					{
						lsitem.QtyNotAvail += (decimal)split.InvtMult * (decimal)split.BaseQty;
					}
				}
                return lsitem;
            }

            return null;
        }

        public virtual void UpdateLotSerialStatus(INTran tran, INTranSplit split, InventoryItem item, INLotSerClass lsclass)
        {
            ReceiveLot(tran, split, item, lsclass);
            IssueLot(tran, split, item, lsclass);
            TransferLot(tran, split, item, lsclass);
        }

		protected virtual LotSerialStatusByCostCenter UpdateLotSerialStatusByCostCenter(INTran tran, INTranSplit split, INLotSerClass lsclass, DateTime? overrideReceiptDate = null)
		{
			var lsitemByCostCenter = new LotSerialStatusByCostCenter
			{
				InventoryID = split.InventoryID,
				SubItemID = split.SubItemID,
				SiteID = split.SiteID,
				LocationID = split.LocationID,
				LotSerialNbr = split.LotSerialNbr,
				CostCenterID = tran.CostCenterID,
			};
			lsitemByCostCenter = (LotSerialStatusByCostCenter)this.Caches<LotSerialStatusByCostCenter>().Insert(lsitemByCostCenter);

			lsitemByCostCenter.ExpireDate ??= split.ExpireDate;
			lsitemByCostCenter.ReceiptDate ??= overrideReceiptDate ?? split.TranDate;
			lsitemByCostCenter.LotSerTrack = lsclass.LotSerTrack;

			lsitemByCostCenter.QtyOnHand += (decimal)split.InvtMult * (decimal)split.BaseQty;
			lsitemByCostCenter.QtyAvail += (decimal)split.InvtMult * (decimal)split.BaseQty;
			lsitemByCostCenter.QtyHardAvail += (decimal)split.InvtMult * (decimal)split.BaseQty;
			lsitemByCostCenter.QtyActual += (decimal)split.InvtMult * (decimal)split.BaseQty;

			return lsitemByCostCenter;
		}

        public virtual ItemLotSerial AccumulatedItemLotSerial(INTranSplit split, INLotSerClass lsclass)
        {
            ItemLotSerial lsitem = new ItemLotSerial();
            lsitem.InventoryID = split.InventoryID;
            lsitem.LotSerialNbr = split.LotSerialNbr;
            lsitem = (ItemLotSerial)itemlotserial.Insert(lsitem);
            if (lsitem.ExpireDate == null)
            {
                lsitem.ExpireDate = split.ExpireDate;
            }
            lsitem.LotSerTrack = lsclass.LotSerTrack;

            return lsitem;
        }

        public virtual SiteLotSerial AccumulatedSiteLotSerial(INTranSplit split, INLotSerClass lsclass)
        {
            SiteLotSerial lsitem = new SiteLotSerial();
            lsitem.InventoryID = split.InventoryID;
            lsitem.LotSerialNbr = split.LotSerialNbr;
            lsitem.SiteID = split.SiteID;
            lsitem = (SiteLotSerial)sitelotserial.Insert(lsitem);
            if (lsitem.ExpireDate == null)
            {
                lsitem.ExpireDate = split.ExpireDate;
            }
            lsitem.LotSerTrack = lsclass.LotSerTrack;

            return lsitem;
        }

        public virtual INCostStatus AccumulatedCostStatus(INTran tran, INTranSplit split, InventoryItem item)
        {
            INCostStatus layer = null;

            bool isTransit = split.CostSiteID == INTransitSiteID;

			switch (item.ValMethod)
            {
                case INValMethod.Standard:

                    if (tran.TranType == INTranType.NegativeCostAdjustment)
                    {
                        return AccumOversoldCostStatus(tran, split, item);
                    }
                    else
                    {
                        layer = new StandardCostStatus();
                        layer.AccountID = tran.InvtAcctID;
                        layer.SubID = tran.InvtSubID;
                        layer.InventoryID = tran.InventoryID;
                        layer.CostSiteID = split.CostSiteID;
                        layer.SiteID = split.SiteID;
                        layer.CostSubItemID = split.CostSubItemID;
                        layer.ReceiptNbr = INLayerRef.ZZZ;
                        layer.LayerType = INLayerType.Normal;

                        return (StandardCostStatus)standardcoststatus.Cache.Insert(layer);
                    }
                case INValMethod.Average:
                    layer = new AverageCostStatus();
                    layer.AccountID = tran.InvtAcctID;
                    layer.SubID = tran.InvtSubID;
                    layer.InventoryID = tran.InventoryID;
                    layer.CostSiteID = split.CostSiteID;
                    layer.SiteID = split.SiteID;
                    layer.CostSubItemID = split.CostSubItemID;
                    layer.ReceiptNbr = INLayerRef.ZZZ;
                    layer.LayerType = INLayerType.Normal;

                    return (AverageCostStatus)averagecoststatus.Cache.Insert(layer);
                case INValMethod.FIFO:
                    layer = new FIFOCostStatus();
                    layer.AccountID = tran.InvtAcctID;
                    layer.SubID = tran.InvtSubID;
                    layer.InventoryID = tran.InventoryID;
                    layer.CostSiteID = split.CostSiteID;
                    layer.SiteID = split.SiteID;
                    layer.CostSubItemID = split.CostSubItemID;
                    layer.ReceiptDate = tran.TranDate;
                    layer.ReceiptNbr = tran.OrigRefNbr ?? tran.RefNbr;
                    layer.LayerType = INLayerType.Normal;

                    return (FIFOCostStatus)fifocoststatus.Cache.Insert(layer);
                case INValMethod.Specific:
                    if (!isTransit)
                        layer = new SpecificCostStatus();
                    else
                        layer = new SpecificTransitCostStatus();

                    layer.AccountID = tran.InvtAcctID;
                    layer.SubID = tran.InvtSubID;
                    layer.InventoryID = tran.InventoryID;
                    layer.CostSiteID = split.CostSiteID;
                    layer.SiteID = split.SiteID;
                    layer.CostSubItemID = split.CostSubItemID;
                    layer.LotSerialNbr = split.LotSerialNbr;
                    layer.ReceiptDate = tran.TranDate;
                    layer.ReceiptNbr = tran.RefNbr;
                    layer.LayerType = INLayerType.Normal;

                    if (tran.InvtMult == 0 && (tran.TranType == INTranType.Invoice || tran.TranType == INTranType.DebitMemo || tran.TranType == INTranType.CreditMemo))
                    {
                        layer.LotSerialNbr = string.Empty;
                    }

                    if (!isTransit)
                        return (SpecificCostStatus)specificcoststatus.Cache.Insert(layer);
                    else
                        return (SpecificTransitCostStatus)specifictransitcoststatus.Cache.Insert(layer);

                default:
                    throw new PXException();
            }
        }

		public virtual int? GetTransitCostSiteID(INTran tran)
			=> INTransitSiteID;


		public virtual INCostStatus AccumOversoldCostStatus(INTran tran, INTranSplit split, InventoryItem item)
        {
            INCostStatus layer = null;

            if (item.NegQty == false && tran.TranType != INTranType.NegativeCostAdjustment)
            {
				INSite warehouse = INSite.PK.Find(this, tran.SiteID);
				INLocation location = INLocation.PK.Find(this, tran.LocationID);

				string siteCD = "";
                string locationCD = "";

                if (warehouse != null)
                    siteCD = warehouse.SiteCD;

                if (location != null)
                    locationCD = location.LocationCD;

                throw new PXException(Messages.Inventory_Negative, item.InventoryCD, siteCD, locationCD);
            }

            switch (item.ValMethod)
            {
                case INValMethod.Standard:
                case INValMethod.Average:
                case INValMethod.FIFO:
                    layer = new OversoldCostStatus();
                    layer.AccountID = tran.InvtAcctID;
                    layer.SubID = tran.InvtSubID;
                    layer.InventoryID = tran.InventoryID;
                    layer.CostSiteID = split.CostSiteID;
                    layer.SiteID = split.SiteID;
                    layer.CostSubItemID = split.CostSubItemID;
                    layer.ReceiptDate = new DateTime(1900, 1, 1);
                    layer.ReceiptNbr = "OVERSOLD";
                    layer.LayerType = INLayerType.Oversold;
                    layer.ValMethod = item.ValMethod;

                    return (OversoldCostStatus)oversoldcoststatus.Cache.Insert(layer);
                case INValMethod.Specific:
                    throw new PXException(Messages.Inventory_Negative);
                default:
                    throw new PXException();
            }
        }

        public virtual INCostStatus AccumUnmanagedCostStatus(INTran tran, INTranSplit split, InventoryItem item)
        {
            INCostStatus layer = null;

            layer = new UnmanagedCostStatus();
            layer.AccountID = tran.InvtAcctID;
            layer.SubID = tran.InvtSubID;
            layer.InventoryID = tran.InventoryID;
            layer.CostSiteID = split.CostSiteID;
	        layer.SiteID = split.SiteID;
            layer.CostSubItemID = split.CostSubItemID;
            layer.LayerType = INLayerType.Oversold;
            layer.ValMethod = item.ValMethod;

            return (UnmanagedCostStatus)unmanagedcoststatus.Cache.Insert(layer);
        }

        public virtual INCostStatus AccumOversoldCostStatus(INCostStatus layer)
        {
            INCostStatus ret = new OversoldCostStatus();

            PXCache<INCostStatus>.RestoreCopy(ret, layer);
            ret.QtyOnHand = 0m;
            ret.TotalCost = 0m;

            return (OversoldCostStatus)oversoldcoststatus.Cache.Insert(ret);
        }

        public virtual PXView GetReceiptStatusView(InventoryItem item)
        {
            List<Type> bql = new List<Type>()
                {
                    typeof(Select2<,,>),
                    typeof(ReadOnlyCostStatus),
                    typeof(LeftJoin<ReceiptStatus, On<ReceiptStatus.inventoryID, Equal<ReadOnlyCostStatus.inventoryID>,
                                And<ReceiptStatus.costSubItemID, Equal<ReadOnlyCostStatus.costSubItemID>,
                                And<ReceiptStatus.costSiteID, Equal<ReadOnlyCostStatus.costSiteID>,
                                And<ReceiptStatus.accountID, Equal<ReadOnlyCostStatus.accountID>,
                                And<ReceiptStatus.subID, Equal<ReadOnlyCostStatus.subID>>>>>>>),
                    typeof(Where2<,>),
                    typeof(Where<ReadOnlyCostStatus.inventoryID, Equal<Current<INTranSplit.inventoryID>>,
                        And<ReadOnlyCostStatus.costSubItemID, Equal<Current<INTranSplit.costSubItemID>>,
                        And<ReadOnlyCostStatus.costSiteID, Equal<Current<INTranSplit.costSiteID>>,
                        And<ReadOnlyCostStatus.layerType, Equal<INLayerType.normal>,
                        And<ReadOnlyCostStatus.accountID, Equal<Required<INTran.invtAcctID>>,
                        And<ReadOnlyCostStatus.subID, Equal<Required<INTran.invtSubID>>>>>>>>)
                };

            switch (item.ValMethod)
            {
                case INValMethod.Standard:
                    bql.Add(typeof(And<Where<True, Equal<False>>>));
                    break;
                case INValMethod.FIFO:
                    bql.Add(typeof(And<Where<ReadOnlyCostStatus.receiptNbr, Equal<Current<INTran.origRefNbr>>>>));
                    break;
                case INValMethod.Specific:
                    bql.Add(typeof(And<Where<ReadOnlyCostStatus.lotSerialNbr, Equal<ReceiptStatus.lotSerialNbr>,
                        And<ReceiptStatus.docType, Equal<INDocType.receipt>,
                        And<ReceiptStatus.receiptNbr, Equal<Current<INTran.origRefNbr>>,
                        And<ReadOnlyCostStatus.lotSerialNbr, Equal<Current<INTran.lotSerialNbr>>>>>>>));
                    break;
                case INValMethod.Average:
                    bql.Add(typeof(And<Where<ReadOnlyCostStatus.receiptNbr, Equal<INLayerRef.zzz>,
                        And<ReceiptStatus.docType, Equal<INDocType.receipt>,
                        And<ReceiptStatus.receiptNbr, Equal<Current<INTran.origRefNbr>>>>>>));
                    break;
            }
            return this.TypedViews.GetView(BqlCommand.CreateInstance(bql.ToArray()), false);
        }

        public virtual PXView GetReceiptStatusByKeysView(INCostStatus layer)
        {
            List<Type> bql = new List<Type>()
            {
                    typeof(Select<,>),
                    typeof(ReadOnlyReceiptStatus),
                    typeof(Where<,,>),
                    typeof(ReadOnlyReceiptStatus.inventoryID),
                    typeof(Equal<Required<INCostStatus.inventoryID>>),
                    typeof(And<,,>),
                    typeof(ReadOnlyReceiptStatus.costSiteID),
                    typeof(Equal<Required<INCostStatus.costSiteID>>),
                    typeof(And<,,>),
                    typeof(ReadOnlyReceiptStatus.costSubItemID),
                    typeof(Equal<Required<INCostStatus.costSubItemID>>),
                    typeof(And<,,>),
                    typeof(ReadOnlyReceiptStatus.accountID),
                    typeof(Equal<Required<INCostStatus.accountID>>)
            };


            if (layer.ValMethod == INValMethod.Specific)
            {
                bql.Add(typeof(And<,,>));
                bql.Add(typeof(ReadOnlyReceiptStatus.subID));
                bql.Add(typeof(Equal<Required<INCostStatus.subID>>));
                bql.Add(typeof(And<,>));
                bql.Add(typeof(ReadOnlyReceiptStatus.lotSerialNbr));
                bql.Add(typeof(Equal<Required<INCostStatus.lotSerialNbr>>));
            }
            else
            {
                bql.Add(typeof(And<,>));
                bql.Add(typeof(ReadOnlyReceiptStatus.subID));
                bql.Add(typeof(Equal<Required<INCostStatus.subID>>));
            }

            return this.TypedViews.GetView(BqlCommand.CreateInstance(bql.ToArray()), false);
        }

        public virtual PXView GetCostStatusCommand(INTran tran, INTranSplit split, InventoryItem item, out object[] parameters, bool correctImbalance, string fifoLayerNbr)
		{
			BqlCommand cmd = null;

			int? costsiteid;

			if (IsIngoingTransfer(tran))
			{
				costsiteid = GetTransitCostSiteID(tran);
			}
			else
			{
				costsiteid = split.CostSiteID;
			}


			if (correctImbalance || IsIngoingTransfer(tran))
			{
				fifoLayerNbr = tran.OrigRefNbr;
			}

			switch (item.ValMethod)
			{
				case INValMethod.Average:
				case INValMethod.Standard:
				case INValMethod.FIFO:

					cmd = new Select<ReadOnlyCostStatus,
						Where<ReadOnlyCostStatus.inventoryID, Equal<Required<ReadOnlyCostStatus.inventoryID>>,
						And<ReadOnlyCostStatus.costSiteID, Equal<Required<ReadOnlyCostStatus.costSiteID>>,
						And<ReadOnlyCostStatus.costSubItemID, Equal<Required<ReadOnlyCostStatus.costSubItemID>>,
						And<INCostStatus.layerType, Equal<INLayerType.normal>>>>>,
						OrderBy<Asc<ReadOnlyCostStatus.receiptDate, Asc<ReadOnlyCostStatus.receiptNbr>>>>();
					if (item.ValMethod == INValMethod.FIFO && fifoLayerNbr != null || IsIngoingTransfer(tran))
					{
						cmd = cmd.WhereAnd<Where<ReadOnlyCostStatus.receiptNbr, Equal<Required<ReadOnlyCostStatus.receiptNbr>>>>();
					}
					else if (fifoLayerNbr == null)
					{
						cmd = cmd.WhereAnd<Where<ReadOnlyCostStatus.qtyOnHand, Greater<decimal0>>>();
					}
					parameters = new object[] { split.InventoryID, costsiteid, split.CostSubItemID, fifoLayerNbr };
					break;
				case INValMethod.Specific:
					cmd = new Select<ReadOnlyCostStatus,
						Where<ReadOnlyCostStatus.inventoryID, Equal<Required<ReadOnlyCostStatus.inventoryID>>,
						And<ReadOnlyCostStatus.costSiteID, Equal<Required<ReadOnlyCostStatus.costSiteID>>,
						And<ReadOnlyCostStatus.costSubItemID, Equal<Required<ReadOnlyCostStatus.costSubItemID>>,
						And<ReadOnlyCostStatus.lotSerialNbr, Equal<Required<ReadOnlyCostStatus.lotSerialNbr>>,
						And<INCostStatus.layerType, Equal<INLayerType.normal>>>>>>>();
					if (IsIngoingTransfer(tran))
					{
						cmd = cmd.WhereAnd<Where<ReadOnlyCostStatus.receiptNbr, Equal<Required<ReadOnlyCostStatus.receiptNbr>>>>();
					}
					parameters = new object[] { split.InventoryID, costsiteid, split.CostSubItemID, split.LotSerialNbr, tran.OrigRefNbr ?? string.Empty };
					break;
				default:
					throw new PXException();
			}
			return TypedViews.GetView(cmd, false);
		}

		/// <summary>
		/// Corrects the account in the cost layer (switches to another cost layer) if the target cost layer does not exist or empty
		/// </summary>
		public virtual void CorrectLayerAccountSub(INTranSplit split, InventoryItem item, INCostStatus layer, List<object> costStatuses, INTranCost costtran, bool updateAccountSub)
		{
			bool negQtyAdj = (split.InvtMult == (short)-1);
			bool costAdj = (split.TranType == INTranType.Adjustment && split.BaseQty == 0m);
			if (!negQtyAdj && !costAdj || !item.ValMethod.IsIn(INValMethod.Average, INValMethod.FIFO, INValMethod.Specific))
			{
				return;
			}

			var notEmptyCostStatuses = costStatuses.Cast<INCostStatus>()
				.Where(s => s.LayerType == INLayerType.Normal && s.QtyOnHand > 0m);

			var exactCostStatus = notEmptyCostStatuses
				.FirstOrDefault(s => s.AccountID == layer.AccountID && s.SubID == layer.SubID);
			if (exactCostStatus != null)
			{
				return;
			}

			var properCostStatus = notEmptyCostStatuses
				.OrderByDescending(s => negQtyAdj ? s.QtyOnHand : s.TotalCost)
				.FirstOrDefault();

			if (properCostStatus != null)
			{
				layer.AccountID = properCostStatus.AccountID;
				layer.SubID = properCostStatus.SubID;
				costtran.InvtAcctID = properCostStatus.AccountID;
				costtran.InvtSubID = properCostStatus.SubID;
			}
		}

        public virtual bool IsUnmanagedTran(INTran tran)
        {
            return tran.InvtMult == (short)0
                && (tran.IsCostUnmanaged ?? false);
        }


        public virtual void ReceiveCost(INTran tran, INTranSplit split, InventoryItem item, bool correctImbalance)
        {
            if (tran.InvtMult == (short)1 && tran.TranType != INTranType.Transfer
					&& (split.InvtMult == (short)1 || item.ValMethod != INValMethod.Standard && !correctImbalance || item.ValMethod == INValMethod.Standard && correctImbalance)
				|| (tran.InvtMult == (short)0 && !IsUnmanagedTran(tran))
				|| (tran.ExactCost == true && !correctImbalance))
            {
                //!!!SPECIFIC, add Account Sub population from existing layer.
                INCostStatus layer = AccumulatedCostStatus(tran, split, item);

                INTranCost costtran = new INTranCost();
                costtran.InvtAcctID = layer.AccountID;
                costtran.InvtSubID = layer.SubID;
                costtran.COGSAcctID = tran.COGSAcctID;
                costtran.COGSSubID = tran.COGSSubID;
                costtran.CostID = layer.CostID;
                costtran.InventoryID = layer.InventoryID;
                costtran.CostSiteID = layer.CostSiteID;
                costtran.CostSubItemID = layer.CostSubItemID;
                costtran.DocType = tran.DocType;
                costtran.TranType = tran.TranType;
                costtran.RefNbr = tran.RefNbr;
                costtran.LineNbr = tran.LineNbr;
                costtran.CostDocType = tran.DocType;
                costtran.CostRefNbr = tran.RefNbr;

                //for negative adjustments line InvtMult == 1 split/cost InvtMult == -1
                costtran.InvtMult = split.InvtMult;

                costtran.FinPeriodID = tran.FinPeriodID;
                costtran.TranPeriodID = tran.TranPeriodID;
                costtran.TranDate = tran.TranDate;
                costtran.TranAmt = 0m;

				INTranCost backupCostTran = intrancost.Locate(costtran);
				backupCostTran = (backupCostTran != null) ? PXCache<INTranCost>.CreateCopy(backupCostTran) : null;

				PXParentAttribute.SetParent(intrancost.Cache, costtran, typeof(INTran), tran);
				INTranCost prev_tran = intrancost.Insert(costtran);
                costtran = PXCache<INTranCost>.CreateCopy(prev_tran);

				costtran.Qty += split.BaseQty;

				//cost only adjustment
				if (split.BaseQty == 0m && tran.BaseQty == 0m)
                {
                    costtran.TranCost += tran.TranCost;
                }
                else if (UseStandardCost(item.ValMethod, tran))
                {
                    //do not add cost, recalculate
                    costtran.TranCost = PXCurrencyAttribute.BaseRound(this, (decimal)layer.UnitCost * (decimal)costtran.Qty);
                }
                else
                {
                    costtran.TranCost += PXCurrencyAttribute.BaseRound(this, (decimal)tran.UnitCost * (decimal)split.BaseQty);
                }

                costtran.TranAmt += PXCurrencyAttribute.BaseRound(this, (decimal)split.BaseQty * (decimal)tran.UnitPrice);

                object[] parameters;
                List<object> costStatuses;
                PXView cmd = null;

                if (tran.TranType == INTranType.Receipt)
                {
                    costStatuses = new List<object>();
                }
                else
                {
                    cmd = GetCostStatusCommand(tran, split, item, out parameters, false, layer.ReceiptNbr);
                    costStatuses = cmd.SelectMulti(parameters);
                }

				//such empty layer could cause not intended switch of strategy (to issue), must be refactored
				//layer is the record in cache and AccountID, SubID are the keys, it must not be modified
				var layerCopy = PXCache<INCostStatus>.CreateCopy(layer);
				CorrectLayerAccountSub(split, item, layerCopy, costStatuses, costtran, false);
				if (layerCopy.AccountID != layer.AccountID || layerCopy.SubID != layer.SubID)
				{
					layer = AccumulatedCostStatus(layerCopy, item, tran);
				}

				var unmodifiedLayer = PXCache<INCostStatus>.CreateCopy(layer);

				layer.QtyOnHand += (decimal)costtran.InvtMult * (costtran.Qty - prev_tran.Qty);
				layer.TotalCost += (decimal)costtran.InvtMult * (costtran.TranCost - prev_tran.TranCost);

				//verify imbalance and accumulate changes for possible strategy change (receipt->issue)
                INCostStatus exactCostStatus = costStatuses.Cast<INCostStatus>().FirstOrDefault(s => s.AccountID == layer.AccountID && s.SubID == layer.SubID);
                Action<INCostStatus> restoreData =
                    backup =>
				{
					PXCache<INCostStatus>.RestoreCopy(layer, unmodifiedLayer);
					if (exactCostStatus != null && backup != null)
						PXCache<INCostStatus>.RestoreCopy(exactCostStatus, backup);
					if (backupCostTran == null)
					{
						intrancost.Delete(costtran);
					}
					else
					{
						INTranCost locatedCostTran = intrancost.Locate(backupCostTran);
						PXCache<INTranCost>.RestoreCopy(locatedCostTran, backupCostTran);
					}
				};

                if (exactCostStatus != null)
                {
	                var backup = PXCache<INCostStatus>.CreateCopy(exactCostStatus);

                    exactCostStatus.QtyOnHand += (decimal)costtran.InvtMult * (costtran.Qty - prev_tran.Qty);
                    exactCostStatus.TotalCost += (decimal)costtran.InvtMult * (costtran.TranCost - prev_tran.TranCost);
                    cmd.Cache.MarkUpdated(exactCostStatus);
                        //throw exception if not cost only adjustment and quantity to cost imbalance detected
                    if (split.BaseQty != 0m && tran.BaseQty != 0m && exactCostStatus.QtyOnHand < 0m)
                        {
                        restoreData(backup);
                            throw new PXNegativeQtyImbalanceException();
                        }
                    if (split.BaseQty != 0m && tran.BaseQty != 0m && exactCostStatus.QtyOnHand == 0m && exactCostStatus.TotalCost != 0m)
                            {
                        restoreData(backup);
                            throw new PXQtyCostImbalanceException();
                        }
                }
				else if (costtran.InvtMult == -1)
                {
                    restoreData(null);
                    throw new PXNegativeQtyImbalanceException();
                }
				else if (costtran.InvtMult == 1 && tran.TranType != INTranType.Receipt)
				{
					exactCostStatus = InsertArtificialCostLayer(cmd.Cache, layer);
				}

				decimal? pretotal = costtran.TranCost;

                if (split.BaseQty == 0m && tran.BaseQty == 0m)
                {
                    //avoid PXFormula
                    PXCache<INTranCost>.RestoreCopy(prev_tran, costtran);
                }
                else
                {
                    decimal diff;
                    if (item.ValMethod != INValMethod.Standard && (diff = PXCurrencyAttribute.BaseRound(this, (tran.CostedQty + costtran.Qty - prev_tran.Qty) * tran.UnitCost) - (tran.TranCost ?? 0m) - (costtran.TranCost ?? 0m) + (prev_tran.TranCost ?? 0m)) != 0m)
                    {
                        costtran.TranCost += diff;
                        layer.TotalCost += (decimal)costtran.InvtMult * diff;
					}

                    //update after, otherwise objects.Equal(costtran, prev_tran)
                    costtran = intrancost.Update(costtran);
                }

                //write-off cost remainder
                if (tran.BaseQty != 0m && tran.BaseQty == tran.CostedQty)
                {
                    if (UseStandardCost(item.ValMethod, tran))
                    {
                        costtran.VarCost += (tran.OrigTranCost - tran.TranCost);
                        tran.TranCost = tran.OrigTranCost;
                    }
                    else
                    {
                        costtran.TranCost += (tran.OrigTranCost - tran.TranCost);
                        layer.TotalCost += costtran.InvtMult * (tran.OrigTranCost - tran.TranCost);
                        tran.TranCost = tran.OrigTranCost;
                    }
                }
                //negative cost adjustment 1:1 INTran:INTranSplit
                else if (tran.BaseQty != 0m && tran.BaseQty == -tran.CostedQty)
                {
                    if (UseStandardCost(item.ValMethod, tran))
                    {
                        costtran.VarCost += (-1m * tran.OrigTranCost - tran.TranCost);
                        tran.TranCost = tran.OrigTranCost;
                    }
                    else
                    {
                        costtran.TranCost += (-1m * tran.OrigTranCost - tran.TranCost);
						layer.TotalCost += costtran.InvtMult * (-1m * tran.OrigTranCost - tran.TranCost);
                        tran.TranCost = tran.OrigTranCost;
                    }
                }

                //write-off price remainder
                if (tran.BaseQty != 0m && (tran.BaseQty == tran.CostedQty || tran.BaseQty == -tran.CostedQty))
                {
                    costtran.TranAmt += (tran.OrigTranAmt - tran.TranAmt);
                    tran.TranAmt = tran.OrigTranAmt;
                }

				//accumulate total differences
				if (exactCostStatus != null)
				{
					if (item.ValMethod == INValMethod.Specific ||
						item.ValMethod == INValMethod.Average || item.ValMethod == INValMethod.FIFO)
					{
						exactCostStatus.TotalCost += (decimal)costtran.InvtMult * (costtran.TranCost - pretotal);
					}
				}
			}
        }

		protected virtual INCostStatus InsertArtificialCostLayer(PXCache cache, INCostStatus layer)
		{
			var newLayer = (INCostStatus)cache.CreateInstance();
			newLayer.AccountID = layer.AccountID;
			newLayer.SubID = layer.SubID;
			newLayer.InventoryID = layer.InventoryID;
			newLayer.CostSiteID = layer.CostSiteID;
			newLayer.SiteID = layer.SiteID;
			newLayer.CostSubItemID = layer.CostSubItemID;
			newLayer.LotSerialNbr = layer.LotSerialNbr;
			newLayer.ReceiptDate = layer.ReceiptDate;
			newLayer.ReceiptNbr = layer.ReceiptNbr;
			newLayer.LayerType = layer.LayerType;
			newLayer.ValMethod = layer.ValMethod;
			newLayer.QtyOnHand = layer.QtyOnHand;
			newLayer.UnitCost = layer.UnitCost;
			newLayer.TotalCost = layer.TotalCost;
			newLayer = (INCostStatus)cache.Insert(newLayer);
			return newLayer;
		}

        public virtual void DropshipCost(INTran tran, INTranSplit split, InventoryItem item)
        {
            if (!IsUnmanagedTran(tran))
                return;
            
            INCostStatus layer = AccumUnmanagedCostStatus(tran, split, item);

            INTranCost costtran = new INTranCost();
            costtran.InvtAcctID = layer.AccountID;
            costtran.InvtSubID = layer.SubID;
            costtran.COGSAcctID = tran.COGSAcctID;
            costtran.COGSSubID = tran.COGSSubID;
            costtran.CostID = layer.CostID;
            costtran.InventoryID = layer.InventoryID;
            costtran.CostSiteID = layer.CostSiteID;
            costtran.CostSubItemID = layer.CostSubItemID;
            costtran.DocType = tran.DocType;
            costtran.TranType = tran.TranType;
            costtran.RefNbr = tran.RefNbr;
            costtran.LineNbr = tran.LineNbr;
            costtran.CostDocType = tran.DocType;
            costtran.CostRefNbr = tran.RefNbr;
            costtran.InvtMult = split.InvtMult;
            costtran.FinPeriodID = tran.FinPeriodID;
            costtran.TranPeriodID = tran.TranPeriodID;
            costtran.TranDate = tran.TranDate;
            costtran.TranAmt = 0m;
            costtran.IsVirtual = true;

            INTranCost prev_tran = intrancost.Insert(costtran);
            costtran = PXCache<INTranCost>.CreateCopy(prev_tran);

			PXParentAttribute.SetParent(intrancost.Cache, costtran, typeof(INTran), tran);

			costtran.Qty += split.BaseQty;

			if (split.Qty.GetValueOrDefault() != 0m)
            {
				costtran.CostType = INTranCost.costType.DropShip;
                costtran.TranCost += PXCurrencyAttribute.BaseRound(this, (decimal)(tran.OrigTranCost / tran.BaseQty) * (decimal)split.BaseQty);
                costtran.TranAmt += PXCurrencyAttribute.BaseRound(this, (decimal)split.BaseQty * (decimal)(tran.OrigTranAmt / tran.BaseQty));

				costtran = intrancost.Update(costtran);
			}
            else if (tran.TranType == INTranType.Adjustment)
            {
				// Landed cost and PPV for non-stock items (Drop Ships)
				costtran.CostType = INTranCost.costType.DropShipPPVorLC;
				costtran.TranCost = tran.TranCost;
				costtran.TranAmt = tran.TranAmt;

				costtran = intrancost.Update(costtran);
			}

        }

        public class PXSelectOversold<InventoryID, CostSubItemID, CostSiteID> : 
            PXSelectReadonly2<INTranCost,
                InnerJoin<INTran, 
                    On2<INTranCost.FK.Tran,
                    And<INTran.docType, Equal<INTranCost.costDocType>, 
                    And<INTran.refNbr, Equal<INTranCost.costRefNbr>>>>,
                InnerJoin<ReadOnlyCostStatus, On<ReadOnlyCostStatus.costID, Equal<INTranCost.costID>, 
                    And<ReadOnlyCostStatus.layerType, Equal<INLayerType.oversold>>>>>,
                Where<INTranCost.inventoryID, Equal<Current<InventoryID>>, 
                    And<INTranCost.costSubItemID, Equal<Current<CostSubItemID>>, 
                    And<INTranCost.costSiteID, Equal<Current<CostSiteID>>, 
                    And<INTranCost.isOversold, Equal<True>,
                    And<INTranCost.oversoldQty, Greater<decimal0>>>>>>>
            where InventoryID : IBqlField
            where CostSubItemID : IBqlField
            where CostSiteID : IBqlField
        {
            public PXSelectOversold(PXGraph graph)
                : base(graph)
            {
            }
        }

        public void ReceiveOverSold<TLayer, InventoryID, CostSubItemID, CostSiteID>(INRegister doc)
            where TLayer : INCostStatus
            where InventoryID : IBqlField
            where CostSubItemID : IBqlField
            where CostSiteID : IBqlField
        {
            foreach (TLayer accumlayer in this.Caches[typeof(TLayer)].Inserted)
            {
                if (accumlayer.QtyOnHand > 0m)
                {
                    foreach (PXResult<INTranCost, INTran, ReadOnlyCostStatus> res in PXSelectOversold<InventoryID, CostSubItemID, CostSiteID>.SelectMultiBound(this, new object[] { accumlayer }))
                    {
                        INTranCost costtran = res;
                        INTran intran = res;

                        costtran = PXCache<INTranCost>.CreateCopy(costtran);
                        costtran.CostDocType = doc.DocType;
                        costtran.CostRefNbr = doc.RefNbr;

                        INTranCostUpdate oversoldtrancostupdate = new INTranCostUpdate
                        {
							DocType = costtran.DocType,
                            RefNbr = costtran.RefNbr,
                            LineNbr = costtran.LineNbr,
                            CostID = costtran.CostID,
                            CostDocType = intran.DocType,
                            CostRefNbr = intran.RefNbr,
                            ValMethod = ((ReadOnlyCostStatus)res).ValMethod
                        };

                        oversoldtrancostupdate = intrancostupdate.Insert(oversoldtrancostupdate);

                        costtran.OversoldQty += oversoldtrancostupdate.OversoldQty;
                        costtran.OversoldTranCost += oversoldtrancostupdate.OversoldTranCost;

                        if (costtran.OversoldQty <= 0m)
                            continue;

                        INCostStatus oversoldlayer = AccumOversoldCostStatus((ReadOnlyCostStatus)res);

                        if (accumlayer.QtyOnHand != 0m)
                        {
                            accumlayer.AvgCost = accumlayer.TotalCost / accumlayer.QtyOnHand;
                        }
                        if ((((ReadOnlyCostStatus)res).QtyOnHand + oversoldlayer.QtyOnHand) != 0m)
                        {
                            oversoldlayer.AvgCost = (((ReadOnlyCostStatus)res).TotalCost + oversoldlayer.TotalCost) / (((ReadOnlyCostStatus)res).QtyOnHand + oversoldlayer.QtyOnHand);
                        }

                        if (costtran.OversoldQty <= accumlayer.QtyOnHand)
                        {
                            {
                                //reverse original cost
                                INTranCost newtran = PXCache<INTranCost>.CreateCopy(costtran);

                                newtran.TranDate = doc.TranDate;
                                newtran.TranPeriodID = doc.TranPeriodID;
                                newtran.FinPeriodID = doc.FinPeriodID;
                                newtran.CostDocType = doc.DocType;
                                newtran.CostRefNbr = doc.RefNbr;
                                newtran.TranAmt = 0m;
                                newtran.Qty = 0m;
                                newtran.OversoldQty = 0m;
                                newtran.OversoldTranCost = 0m;
                                newtran.TranCost = 0m;
                                newtran.VarCost = 0m;

								PXParentAttribute.SetParent(intrancost.Cache, newtran, typeof(INTran), intran);
								//count for multiply layers adjusting single oversold transactions
								INTranCost prev_tran = intrancost.Insert(newtran);
                                newtran = PXCache<INTranCost>.CreateCopy(prev_tran);

                                newtran.IsOversold = false;
                                newtran.Qty -= costtran.OversoldQty;
                                if (oversoldlayer.ValMethod == INValMethod.Standard)
                                {
                                    newtran.TranCost = PXCurrencyAttribute.BaseRound(this, newtran.Qty * oversoldlayer.AvgCost);
                                    newtran.VarCost = -PXCurrencyAttribute.BaseRound(this, newtran.Qty * oversoldlayer.AvgCost) + PXCurrencyAttribute.BaseRound(this, newtran.Qty * oversoldlayer.UnitCost);
                                }
                                else
                                {
                                    newtran.TranCost -= costtran.OversoldTranCost;
                                }

                                decimal? oversoldcostmovement = -newtran.TranCost + prev_tran.TranCost;
                                decimal? oversoldqtymovement = -newtran.Qty + prev_tran.Qty;
                                oversoldlayer.TotalCost += oversoldcostmovement;
                                oversoldlayer.QtyOnHand += oversoldqtymovement;

                                oversoldtrancostupdate.OversoldQty -= oversoldqtymovement;
                                oversoldtrancostupdate.OversoldTranCost -= oversoldcostmovement;
                                oversoldtrancostupdate.ResetOversoldFlag = true;

                                intrancost.Update(newtran);


                            }
                            {
                                INTranCost newtran = PXCache<INTranCost>.CreateCopy(costtran);
                                newtran.IsOversold = false;
                                newtran.CostID = accumlayer.CostID;
                                newtran.InvtAcctID = accumlayer.AccountID;
                                newtran.InvtSubID = accumlayer.SubID;
                                newtran.TranDate = doc.TranDate;
                                newtran.TranPeriodID = doc.TranPeriodID;
                                newtran.FinPeriodID = doc.FinPeriodID;
                                newtran.Qty = costtran.OversoldQty;
                                newtran.OversoldQty = 0m;
                                newtran.OversoldTranCost = 0m;
                                newtran.TranCost = PXCurrencyAttribute.BaseRound(this, newtran.Qty * accumlayer.AvgCost);
                                if (accumlayer.ValMethod == INValMethod.Standard)
                                {
                                    newtran.VarCost = -PXCurrencyAttribute.BaseRound(this, newtran.Qty * accumlayer.AvgCost) + PXCurrencyAttribute.BaseRound(this, newtran.Qty * accumlayer.UnitCost);
                                }
                                newtran.TranAmt = 0m;
                                newtran.CostDocType = doc.DocType;
                                newtran.CostRefNbr = doc.RefNbr;

								PXParentAttribute.SetParent(intrancost.Cache, newtran, typeof(INTran), intran);
								intrancost.Cache.Insert(newtran);

                                accumlayer.TotalCost -= PXCurrencyAttribute.BaseRound(this, costtran.OversoldQty * accumlayer.AvgCost);
                                accumlayer.QtyOnHand -= costtran.OversoldQty;

                            }
                        }
                        else if (accumlayer.QtyOnHand > 0m)
                        {
                            {
                                //reverse original cost
                                INTranCost newtran = PXCache<INTranCost>.CreateCopy(costtran);
                                newtran.IsOversold = true;
                                newtran.TranDate = doc.TranDate;
                                newtran.TranPeriodID = doc.TranPeriodID;
                                newtran.FinPeriodID = doc.FinPeriodID;
                                newtran.CostDocType = doc.DocType;
                                newtran.CostRefNbr = doc.RefNbr;
                                newtran.TranAmt = 0m;
                                newtran.Qty = 0m;
                                newtran.OversoldQty = 0m;
                                newtran.OversoldTranCost = 0m;
                                newtran.TranCost = 0m;
                                newtran.VarCost = 0m;

								PXParentAttribute.SetParent(intrancost.Cache, newtran, typeof(INTran), intran);
								//count for multiply layers adjusting single oversold transactions
								INTranCost prev_tran = intrancost.Insert(newtran);
                                newtran = PXCache<INTranCost>.CreateCopy(prev_tran);

                                newtran.Qty -= accumlayer.QtyOnHand;
                                if (oversoldlayer.ValMethod == INValMethod.Standard)
                                {
                                    newtran.TranCost = PXCurrencyAttribute.BaseRound(this, newtran.Qty * oversoldlayer.AvgCost);
                                    newtran.VarCost = -PXCurrencyAttribute.BaseRound(this, newtran.Qty * oversoldlayer.UnitCost) + PXCurrencyAttribute.BaseRound(this, newtran.Qty * oversoldlayer.AvgCost);
                                }
                                else
                                {
                                    newtran.TranCost = PXCurrencyAttribute.BaseRound(this, newtran.Qty * costtran.OversoldTranCost / costtran.OversoldQty);
                                }

                                decimal? oversoldcostmovement = -newtran.TranCost + prev_tran.TranCost;
                                decimal? oversoldqtymovement = -newtran.Qty + prev_tran.Qty;
                                oversoldlayer.TotalCost += oversoldcostmovement;
                                oversoldlayer.QtyOnHand += oversoldqtymovement;

                                oversoldtrancostupdate.OversoldTranCost -= oversoldcostmovement;
                                oversoldtrancostupdate.OversoldQty -= oversoldqtymovement;
                                intrancost.Update(newtran);
                            }
                            {
                                INTranCost newtran = PXCache<INTranCost>.CreateCopy(costtran);
                                newtran.IsOversold = false;
                                newtran.CostID = accumlayer.CostID;
                                newtran.InvtAcctID = accumlayer.AccountID;
                                newtran.InvtSubID = accumlayer.SubID;
                                newtran.TranDate = doc.TranDate;
                                newtran.TranPeriodID = doc.TranPeriodID;
                                newtran.FinPeriodID = doc.FinPeriodID;
                                newtran.Qty = accumlayer.QtyOnHand;
                                newtran.OversoldQty = 0m;
                                newtran.OversoldTranCost = 0m;
                                newtran.TranCost = accumlayer.TotalCost;
                                if (accumlayer.ValMethod == INValMethod.Standard)
                                {
                                    newtran.VarCost = -accumlayer.TotalCost + PXCurrencyAttribute.BaseRound(this, newtran.Qty * accumlayer.UnitCost);
                                }
                                newtran.TranAmt = 0m;
                                newtran.CostDocType = doc.DocType;
                                newtran.CostRefNbr = doc.RefNbr;

								PXParentAttribute.SetParent(intrancost.Cache, newtran, typeof(INTran), intran);
								intrancost.Cache.Insert(newtran);

                                accumlayer.TotalCost = 0m;
                                accumlayer.QtyOnHand = 0m;
                            }
                        }
                    }
                }
            }
        }

        public virtual void ReceiveOversold(INRegister doc)
        {
            ReceiveOverSold<FIFOCostStatus, FIFOCostStatus.inventoryID, FIFOCostStatus.costSubItemID, FIFOCostStatus.costSiteID>(doc);
            ReceiveOverSold<AverageCostStatus, AverageCostStatus.inventoryID, AverageCostStatus.costSubItemID, AverageCostStatus.costSiteID>(doc);
            ReceiveOverSold<StandardCostStatus, StandardCostStatus.inventoryID, StandardCostStatus.costSubItemID, StandardCostStatus.costSiteID>(doc);
        }
         
        public virtual INCostStatus AccumulatedCostStatus(INCostStatus layer, InventoryItem item, INTran tran)
        {
            INCostStatus ret = null;

            if (layer.LayerType == INLayerType.Oversold)
            {
                ret = new OversoldCostStatus();
                ret.AccountID = layer.AccountID;
                ret.SubID = layer.SubID;
                ret.InventoryID = layer.InventoryID;
                ret.CostSiteID = layer.CostSiteID;
	            ret.SiteID = layer.SiteID;
                ret.CostSubItemID = layer.CostSubItemID;
                ret.ReceiptDate = layer.ReceiptDate;
                ret.ReceiptNbr = layer.ReceiptNbr;
                ret.LayerType = layer.LayerType;

                return (OversoldCostStatus)oversoldcoststatus.Cache.Insert(ret);
            }

            switch (item.ValMethod)
            {
                case INValMethod.Average:
                    ret = new AverageCostStatus();
                    ret.AccountID = layer.AccountID;
                    ret.SubID = layer.SubID;
                    ret.InventoryID = layer.InventoryID;
                    ret.CostSiteID = layer.CostSiteID;
	                ret.SiteID = layer.SiteID;
                    ret.CostSubItemID = layer.CostSubItemID;
                    ret.LayerType = layer.LayerType;
                    ret.ReceiptNbr = layer.ReceiptNbr;

                    return (AverageCostStatus)averagecoststatus.Cache.Insert(ret);
                case INValMethod.Standard:
                    ret = new StandardCostStatus();
                    ret.AccountID = layer.AccountID;
                    ret.SubID = layer.SubID;
                    ret.InventoryID = layer.InventoryID;
                    ret.CostSiteID = layer.CostSiteID;
	                ret.SiteID = layer.SiteID;
                    ret.CostSubItemID = layer.CostSubItemID;
                    ret.LayerType = layer.LayerType;
                    ret.ReceiptNbr = layer.ReceiptNbr;

                    return (StandardCostStatus)standardcoststatus.Cache.Insert(ret);
                case INValMethod.FIFO:
                    ret = new FIFOCostStatus();
                    ret.AccountID = layer.AccountID;
                    ret.SubID = layer.SubID;
                    ret.InventoryID = layer.InventoryID;
                    ret.CostSiteID = layer.CostSiteID;
	                ret.SiteID = layer.SiteID;
                    ret.CostSubItemID = layer.CostSubItemID;
                    ret.ReceiptDate = layer.ReceiptDate;
                    ret.ReceiptNbr = layer.ReceiptNbr;
                    ret.LayerType = layer.LayerType;

                    return (FIFOCostStatus)fifocoststatus.Cache.Insert(ret);
                case INValMethod.Specific:
                    if (layer.CostSiteID == GetTransitCostSiteID(tran))
                        ret = new SpecificTransitCostStatus();
                    else
                        ret = new SpecificCostStatus();
                    ret.AccountID = layer.AccountID;
                    ret.SubID = layer.SubID;
                    ret.InventoryID = layer.InventoryID;
                    ret.CostSiteID = layer.CostSiteID;
	                ret.SiteID = layer.SiteID;
                    ret.CostSubItemID = layer.CostSubItemID;
                    ret.LotSerialNbr = layer.LotSerialNbr;
                    ret.ReceiptDate = layer.ReceiptDate;
                    ret.ReceiptNbr = layer.ReceiptNbr;
                    ret.LayerType = layer.LayerType;

                    if (layer.CostSiteID == GetTransitCostSiteID(tran))
                        return (SpecificTransitCostStatus)specifictransitcoststatus.Cache.Insert(ret);
                    else
                        return (SpecificCostStatus)specificcoststatus.Cache.Insert(ret);
                default:
                    throw new PXException();
            }
        }

        public virtual INCostStatus AccumulatedTransferCostStatus(INCostStatus layer, INTran tran, INTranSplit split, InventoryItem item)
        {
            INCostStatus ret = null;
            bool transferModeEnabled = !IsIngoingTransfer(tran) && !IsOneStepTransfer();
            INCostStatus result;
            switch (item.ValMethod)
            {
                case INValMethod.Average:
                    ret = new AverageCostStatus();
                    ret.AccountID = transferModeEnabled ? this.INTransitAcctID : tran.InvtAcctID;
                    ret.SubID = transferModeEnabled ? this.INTransitSubID : tran.InvtSubID;
                    ret.InventoryID = layer.InventoryID;
                    ret.CostSiteID = transferModeEnabled ? GetTransitCostSiteID(tran) : split.CostSiteID;
                    ret.SiteID = transferModeEnabled ? INTransitSiteID : split.SiteID;
                    ret.CostSubItemID = layer.CostSubItemID;
                    ret.LayerType = INLayerType.Normal;
                    if (!transferModeEnabled)
                    {
                        ret.ReceiptNbr = INLayerRef.ZZZ;
                    }
                    else
                    {
                        ret.ReceiptNbr = tran.RefNbr;
                    }

                    result = (INCostStatus)averagecoststatus.Cache.Insert(ret);
                    return result;
                case INValMethod.Standard:
                    ret = new StandardCostStatus();
                    ret.AccountID = transferModeEnabled ? this.INTransitAcctID : tran.InvtAcctID;
                    ret.SubID = transferModeEnabled ? this.INTransitSubID : tran.InvtSubID;
                    ret.InventoryID = layer.InventoryID;
                    ret.CostSiteID = transferModeEnabled ? GetTransitCostSiteID(tran) : split.CostSiteID;
                    ret.SiteID = transferModeEnabled ? INTransitSiteID : split.SiteID;
                    ret.CostSubItemID = layer.CostSubItemID;
                    ret.LayerType = INLayerType.Normal;
                    if (!transferModeEnabled)
                    {
                        ret.ReceiptNbr = INLayerRef.ZZZ;
                    }
                    else
                    {
                        ret.ReceiptNbr = tran.RefNbr;
                    }

                    result = (INCostStatus)standardcoststatus.Cache.Insert(ret);
                    return result;
                case INValMethod.FIFO:
                    ret = new FIFOCostStatus();
                    ret.AccountID = transferModeEnabled ? this.INTransitAcctID : tran.InvtAcctID;
                    ret.SubID = transferModeEnabled ? this.INTransitSubID : tran.InvtSubID;
                    ret.InventoryID = layer.InventoryID;
                    ret.CostSiteID = transferModeEnabled ? GetTransitCostSiteID(tran) : split.CostSiteID;
                    ret.SiteID = transferModeEnabled ? INTransitSiteID : split.SiteID;
                    ret.CostSubItemID = layer.CostSubItemID;

                    DateTime? fifoDate;
                    String fifoNbr;

                    if (SameWarehouseTransfer(tran, split))
                    {
                        fifoDate = layer.ReceiptDate;
                        fifoNbr = layer.ReceiptNbr;
                    }
                    else
                    {
                        fifoDate = tran.TranDate;
                        fifoNbr = tran.RefNbr;
                    }

                    ret.ReceiptDate = fifoDate;
                    ret.ReceiptNbr = fifoNbr;

                    ret.LayerType = INLayerType.Normal;

                    result = (INCostStatus)fifocoststatus.Cache.Insert(ret);
                    return result;
                case INValMethod.Specific:
                    if (transferModeEnabled)
                        ret = new SpecificTransitCostStatus();
                    else
                        ret = new SpecificCostStatus();

                    ret.AccountID = transferModeEnabled ? this.INTransitAcctID : tran.InvtAcctID;
                    ret.SubID = transferModeEnabled ? this.INTransitSubID : tran.InvtSubID;
                    ret.InventoryID = layer.InventoryID;
                    ret.CostSiteID = transferModeEnabled ? GetTransitCostSiteID(tran) : split.CostSiteID;
                    ret.SiteID = transferModeEnabled ? INTransitSiteID : split.SiteID;
                    ret.CostSubItemID = layer.CostSubItemID;
                    ret.LotSerialNbr = layer.LotSerialNbr;

                    if (SameWarehouseTransfer(tran, split))
                    {
                        ret.ReceiptNbr = layer.ReceiptNbr;
                        ret.ReceiptDate = layer.ReceiptDate;
                    }
                    else
                    {
                        ret.ReceiptNbr = tran.RefNbr;
                        ret.ReceiptDate = tran.TranDate;
                    }

                    ret.LayerType = INLayerType.Normal;

                    if (transferModeEnabled)
                    {
                        result = (INCostStatus)specifictransitcoststatus.Cache.Insert(ret);
                    }
                    else
                    {
                        result = (INCostStatus)specificcoststatus.Cache.Insert(ret);
                    }

                    return result;
                default:
                    throw new PXException();
            }
        }

        protected virtual bool SameWarehouseTransfer(INTran tran, INTranSplit split)
        {
            if (!IsOneStepTransfer())
                return false;

            if (split.FromSiteID == null && tran.OrigDocType != null && tran.OrigRefNbr != null && tran.OrigLineNbr != null)
            {
                INTran ortran = INTran.PK.Find(this, tran.OrigDocType, tran.OrigRefNbr, tran.OrigLineNbr);
                split.FromSiteID = ortran.SiteID;
            }
            return
                split.FromSiteID == tran.SiteID;
        }

        public virtual void IssueCost(INCostStatus layer, INTran tran, INTranSplit split, InventoryItem item, ref decimal QtyUnCosted)
        {
			INCostStatus accumlayer = AccumulatedCostStatus(layer, item, tran);

            INTranCost costtran = new INTranCost();
            costtran.InvtAcctID = accumlayer.AccountID;
            costtran.InvtSubID = accumlayer.SubID;
            costtran.COGSAcctID = tran.COGSAcctID;
            costtran.COGSSubID = tran.COGSSubID;
            costtran.CostID = accumlayer.CostID;
            costtran.InventoryID = accumlayer.InventoryID;
            costtran.CostSiteID = accumlayer.CostSiteID;
            costtran.CostSubItemID = accumlayer.CostSubItemID;
            costtran.IsOversold = (accumlayer.LayerType == INLayerType.Oversold);
            costtran.DocType = tran.DocType;
            costtran.TranType = tran.TranType;
            costtran.RefNbr = tran.RefNbr;
            costtran.LineNbr = tran.LineNbr;
            costtran.CostDocType = tran.DocType;
            costtran.CostRefNbr = tran.RefNbr;
            costtran.IsVirtual = IsIngoingTransfer(tran);
			costtran.CostType = IsTransitTransfer(tran, layer) ? INTranCost.costType.TransitTransfer : INTranCost.costType.Normal;
			//for negative adjustments line InvtMult == 1 split/cost InvtMult == -1
			costtran.InvtMult = IsIngoingTransfer(tran) ? (short)-split.InvtMult : split.InvtMult;
            costtran.FinPeriodID = tran.FinPeriodID;
            costtran.TranPeriodID = tran.TranPeriodID;
            costtran.TranDate = tran.TranDate;
            costtran.TranAmt = 0m;

			if (tran.DocType == INDocType.Receipt && IsIngoingTransfer(tran))
			{// We should take InTransit account from existing intransit layer as it may be changed in setup.
				tran.AcctID = accumlayer.AccountID;
				tran.SubID = accumlayer.SubID;
			}

			PXParentAttribute.SetParent(intrancost.Cache, costtran, typeof(INTran), tran);
			costtran = PXCache<INTranCost>.CreateCopy(intrancost.Insert(costtran));

            //assigning currently accumulated qties
            decimal? issuedqty = costtran.Qty;
            decimal? issuedcost = costtran.TranCost;

            if (layer.QtyOnHand <= QtyUnCosted)
            {
                QtyUnCosted -= (decimal)layer.QtyOnHand;
                costtran.TranAmt += PXCurrencyAttribute.BaseRound(this, (decimal)layer.QtyOnHand * (decimal)tran.UnitPrice);
                costtran.Qty += layer.QtyOnHand;
                costtran.TranCost += layer.TotalCost;
                if (UseStandardCost(accumlayer.ValMethod, tran) && (!IsIngoingTransfer(tran) && !IsOneStepTransfer()))
                {
                    costtran.VarCost += PXDBCurrencyAttribute.BaseRound(this, (decimal)layer.QtyOnHand * (decimal)layer.UnitCost) - layer.TotalCost;
                }
                layer.QtyOnHand = 0m;
                layer.TotalCost = 0m;
            }
            else
            {
                costtran.TranAmt += PXCurrencyAttribute.BaseRound(this, (decimal)QtyUnCosted * (decimal)tran.UnitPrice);
                if (PXCurrencyAttribute.IsNullOrEmpty(layer.UnitCost))
                {
                    layer.UnitCost = (decimal)layer.TotalCost / (decimal)layer.QtyOnHand;
                }

                layer.QtyOnHand -= QtyUnCosted;
                layer.TotalCost += costtran.TranCost;
                layer.TotalCost -= PXCurrencyAttribute.BaseRound(this, (costtran.Qty + QtyUnCosted) * (decimal)layer.UnitCost);

                costtran.Qty += QtyUnCosted;
                costtran.TranCost = PXCurrencyAttribute.BaseRound(this, costtran.Qty * (decimal)layer.UnitCost);

                QtyUnCosted = 0m;
            }

            issuedqty -= costtran.Qty;
            issuedcost -= costtran.TranCost;

            accumlayer.QtyOnHand += issuedqty;
            accumlayer.TotalCost += issuedcost;

            TransferCost(tran, split, item, accumlayer, costtran, issuedqty.Value, issuedcost.Value);

            //Accumulate cost issued via PXFormula for Issues only
            costtran = intrancost.Update(costtran);

            //negative cost adjustment 1:1 INTran:INTranSplit
            if (tran.InvtMult == 1m && tran.BaseQty == -tran.CostedQty)
            {
                if (item.ValMethod != INValMethod.Specific)
                {
                    //reset variance to difference
                    costtran.VarCost = (-1m * tran.OrigTranCost - tran.TranCost);
                }
				if (item.ValMethod != INValMethod.Standard)
				{
					tran.TranCost = -tran.TranCost; // we should preserve the value that was actualy issued from cost layers.
				}
				else
				{
					tran.TranCost = tran.OrigTranCost; // we should preserve original value to be clear where does the std cost variance come from.
                }
            }

            //write-off price remainder
            if (tran.BaseQty != 0m && (tran.BaseQty == tran.CostedQty || tran.BaseQty == -tran.CostedQty))
            {
                costtran.TranAmt += (tran.OrigTranAmt - tran.TranAmt);
                tran.TranAmt = tran.OrigTranAmt;
            }

            //init OversoldQty
            if (costtran.IsOversold ?? false)
            {
                costtran.OversoldQty = costtran.Qty;
                costtran.OversoldTranCost = costtran.TranCost;
            }
        }

        public virtual void AssignQty(ReadOnlyReceiptStatus layer, ref decimal QtyUnAssigned)
        {
            ReceiptStatus accumreceiptstatus = new ReceiptStatus();
            accumreceiptstatus.ReceiptID = layer.ReceiptID;
            accumreceiptstatus.DocType = layer.DocType;
            accumreceiptstatus.ReceiptNbr = layer.ReceiptNbr;
            accumreceiptstatus.SubID = layer.SubID;
            accumreceiptstatus.AccountID = layer.AccountID;
            accumreceiptstatus.CostSiteID = layer.CostSiteID;
            accumreceiptstatus.OrigQty = layer.OrigQty;
            accumreceiptstatus.ReceiptDate = layer.ReceiptDate;
            accumreceiptstatus.LotSerialNbr = layer.LotSerialNbr == null || layer.ValMethod != INValMethod.Specific ? String.Empty : layer.LotSerialNbr;
            accumreceiptstatus.LayerType = layer.LayerType;
            accumreceiptstatus.InventoryID = layer.InventoryID;
            accumreceiptstatus.CostSubItemID = layer.CostSubItemID;

            if (QtyUnAssigned < 0)
            {
                if (layer.QtyOnHand <= -QtyUnAssigned)
                {
                    QtyUnAssigned += (decimal)layer.QtyOnHand;
                    accumreceiptstatus.QtyOnHand = -layer.QtyOnHand;
                    layer.QtyOnHand = 0m;
                }
                else
                {
                    layer.QtyOnHand += QtyUnAssigned;
                    accumreceiptstatus.QtyOnHand = QtyUnAssigned;
                    QtyUnAssigned = 0m;
                }
            }
            else
            {
                accumreceiptstatus.QtyOnHand = QtyUnAssigned;
                layer.QtyOnHand += QtyUnAssigned;
            }
            receiptstatus.Insert(accumreceiptstatus);
        }

        public virtual void IssueQty(INCostStatus layer)
        {
            decimal QtyUnAssigned = layer.QtyOnHand ?? 0m;
            if (QtyUnAssigned >= 0m)
                return;
            PXView readonlyreceiptstatus = GetReceiptStatusByKeysView(layer);
            readonlyreceiptstatus.OrderByNew<OrderBy<Asc<ReadOnlyReceiptStatus.receiptDate, Asc<ReadOnlyReceiptStatus.receiptID>>>>();

            foreach (ReadOnlyReceiptStatus rsLayer in readonlyreceiptstatus.SelectMulti(layer.InventoryID, layer.CostSiteID, layer.CostSubItemID, layer.AccountID, layer.SubID, layer.LotSerialNbr))
            {
                if (rsLayer.QtyOnHand > 0m)
                {
                    AssignQty(rsLayer, ref QtyUnAssigned);

                    readonlyreceiptstatus.Cache.SetStatus(rsLayer, PXEntryStatus.Held);

                    if (QtyUnAssigned == 0m)
                    {
                        break;
                    }
                }
            }
            /* Commented out for legacy db support
            if (QtyUnAssigned != 0m)
            {
                INLocation location = 
                    PXSelectReadonly<INLocation, Where<INLocation.costSiteID, Equal<Required<INCostStatus.costSiteID>>>>
                    .SelectWindowed(this,0, 1, new object[] { layer.CostSiteID });
                int? warehouseid = location == null ? location.SiteID : layer.CostSiteID;
                throw new PXException(Messages.StatusCheck_QtyOnHandNegative,
                    PXForeignSelectorAttribute.GetValueExt<INCostStatus.inventoryID>(this.Caches[layer.GetType()], layer),
                    PXForeignSelectorAttribute.GetValueExt<INCostStatus.costSubItemID>(this.Caches[layer.GetType()], layer),
                    PXForeignSelectorAttribute.GetValueExt<INCostStatus.costSiteID>(this.Caches[layer.GetType()], layer));
            }*/
        }

        public virtual void IssueCost(INTran tran, INTranSplit split, InventoryItem item, bool correctImbalance)
        {
            //costing is done in parallel, i.e. if 2 splits are all the same as transaction, one accumulated cost tran(cost&qty summarized) will be added, if varied in cost key fields, then variance
            //will be via LayerID which will be different
            if ((tran.InvtMult == (short)-1 && (tran.ExactCost != true || correctImbalance))
				|| tran.InvtMult == (short)1 && split.InvtMult == (short)-1
					&& (item.ValMethod == INValMethod.Standard && !correctImbalance || item.ValMethod != INValMethod.Standard && correctImbalance)
				|| (tran.TranType == INTranType.Transfer && !(IsOneStepTransfer() && split.InvtMult != (short)-1)))
            {
                object[] parameters;
                PXView cmd = GetCostStatusCommand(tran, split, item, out parameters, correctImbalance, null);

                if (cmd != null)
                {
					INCostStatus lastLayer = null;
					decimal QtyUnCosted = (decimal)split.BaseQty;
					foreach (INCostStatus layer in cmd.SelectMulti(parameters))
                    {
						lastLayer = layer;
                        if (layer.QtyOnHand > 0m)
                        {
                            IssueCost(layer, tran, split, item, ref QtyUnCosted);

                            cmd.Cache.MarkUpdated(layer);

                            if (QtyUnCosted == 0m)
                            {
                                break;
                            }
                        }
                    }

                    //negative cost adjustment
                    if (tran.InvtMult == (short)1 && QtyUnCosted > 0m)
                    {
                        if (item.ValMethod == INValMethod.Standard && !correctImbalance)
                        {
                            throw new PXQtyCostImbalanceException();
                        }

                        ThrowNegativeQtyException(tran, split, lastLayer);
                    }

					if (tran.POReceiptType == PO.POReceiptType.POReturn && tran.ExactCost == true && QtyUnCosted > 0m)
					{
						ThrowNegativeQtyException(tran, split, lastLayer);
					}

                    if (QtyUnCosted > 0m && (!IsIngoingTransfer(tran) || IsOneStepTransfer()))
                    {
						if (tran.DocType == INDocType.Production && tran.InvtMult == -1 && tran.ReasonCode == null)
							throw new PXException(Messages.KitComponentEmptyReasonCode, InventoryItem.PK.Find(this, tran.InventoryID)?.InventoryCD);

						INCostStatus oversold = PXCache<INCostStatus>.CreateCopy(AccumOversoldCostStatus(tran, split, item));
                        //qty and cost in this dummy layer must be set explicitly, not added.
                        oversold.QtyOnHand = QtyUnCosted;
                        oversold.TotalCost = PXCurrencyAttribute.BaseRound(this, QtyUnCosted * (decimal)oversold.UnitCost);

                        IssueCost(oversold, tran, split, item, ref QtyUnCosted);
                    }

                    if (QtyUnCosted > 0m)
                    {
                        throw new PXException(Messages.InternalError, 500);
                    }
                }
            }
        }

		protected virtual void ThrowNegativeQtyException(INTran tran, INTranSplit split, INCostStatus lastLayer)
		{
			object costSubItemID = split.CostSubItemID;
			intranselect.Cache.RaiseFieldSelecting<INTran.subItemID>(tran, ref costSubItemID, true);
			object inventoryCD = intranselect.Cache.GetValueExt<INTran.inventoryID>(tran);

			if (IsIngoingTransfer(tran))
			{
				if (split.ValMethod == INValMethod.Specific)
					throw new PXException(Messages.StatusCheck_QtyTransitLotSerialOnHandNegative, inventoryCD, costSubItemID, split.LotSerialNbr);
				throw new PXException(Messages.StatusCheck_QtyTransitOnHandNegative, inventoryCD, costSubItemID);
			}

			if (split.ValMethod == INValMethod.Specific && !string.IsNullOrEmpty(split.LotSerialNbr))
				throw new PXException(Messages.StatusCheck_QtyNegativeSPC, inventoryCD, costSubItemID, split.LotSerialNbr);

			if (split.ValMethod == INValMethod.FIFO && !string.IsNullOrEmpty(lastLayer?.ReceiptNbr))
			{
				if (tran.POReceiptType == PO.POReceiptType.POReturn && tran.ExactCost == true)
					throw new PXException(Messages.StatusCheck_QtyNegativeFifoExactCost, inventoryCD, costSubItemID, lastLayer.ReceiptNbr);

				throw new PXException(Messages.StatusCheck_QtyNegativeFifo, inventoryCD, costSubItemID, lastLayer.ReceiptNbr);
			}

			INSite site = INSite.PK.Find(this, split.CostSiteID);
			if (site != null)
				throw new PXException(Messages.StatusCheck_QtyNegative, inventoryCD, costSubItemID, site.SiteCD);

			var siteAndLocation = (PXResult<INSite, INLocation>)
				PXSelectReadonly2<INSite,
				InnerJoin<INLocation, On<INLocation.FK.Site>>,
				Where<INLocation.locationID, Equal<Required<INLocation.locationID>>>>
				.SelectWindowed(this, 0, 1, split.CostSiteID);
			if (siteAndLocation != null)
				throw new PXException(Messages.StatusCheck_QtyNegative1, inventoryCD, costSubItemID, siteAndLocation.GetItem<INLocation>().LocationCD, siteAndLocation.GetItem<INSite>().SiteCD);

			throw new PXException(Messages.StatusCheck_QtyNegative, inventoryCD, costSubItemID, split.CostSiteID);
		}

        public virtual void TransferCost(INTran tran, INTranSplit split, InventoryItem item, INCostStatus issueCost, INTranCost issueTranCost, decimal issuedQty, decimal issuedCost)
        {
            if (tran.TranType != INTranType.Transfer)
                return;

            if (IsOneStepTransfer())
            {
                foreach (INTran positivetran in intranselect.Cache.Cached)
                {
                    if (positivetran.OrigLineNbr == tran.LineNbr && positivetran.OrigRefNbr == tran.RefNbr && positivetran.OrigDocType == tran.DocType)
                    {
                        foreach (INTranSplit positivesplit in PXParentAttribute.SelectChildren(intransplit.Cache, positivetran, typeof(INTran)))
                        {
                            if (split.ToLocationID == positivesplit.LocationID || (split.ToLocationID == null && positivetran.LocationID == split.ToLocationID))
                            {
                                split = (INTranSplit)intransplit.Cache.CreateCopy(split);

                                split.FromSiteID = split.SiteID;
                                split.FromLocationID = split.LocationID;
                                split.SiteID = positivesplit.ToSiteID ?? tran.ToSiteID;
                                split.ToSiteID = split.SiteID;
                                split.LocationID = positivesplit.ToLocationID ?? tran.ToLocationID;
                                split.ToLocationID = split.LocationID;
                                split.CostSiteID = positivesplit.CostSiteID;
                                break;
                            }
                        }

                        tran = positivetran;
                        break;
                    }
                }
            }

            INCostStatus accumlayer = AccumulatedTransferCostStatus(issueCost, tran, split, item);

            accumlayer.QtyOnHand -= issuedQty;
            decimal costadded;
            if (UseStandardCost(accumlayer.ValMethod, tran) && accumlayer.CostSiteID != GetTransitCostSiteID(tran))
            {
                //do not add cost, recalculate
                costadded = -PXCurrencyAttribute.BaseRound(this, (decimal)accumlayer.UnitCost * (decimal)issuedQty);
            }
            else
            {
                costadded = -issuedCost;
            }

            accumlayer.TotalCost += costadded;

            var orig_trancost = issueTranCost;

            INTranCost costtran = new INTranCost();
            costtran.IsVirtual = !IsOneStepTransfer() && !IsIngoingTransfer(tran);
			costtran.CostType = IsTransitTransfer(tran, accumlayer) ? INTranCost.costType.TransitTransfer : INTranCost.costType.Normal;
			costtran.InvtAcctID = accumlayer.AccountID;
            costtran.InvtSubID = accumlayer.SubID;
            costtran.InventoryID = accumlayer.InventoryID;
            costtran.CostSiteID = accumlayer.CostSiteID;
            costtran.CostSubItemID = accumlayer.CostSubItemID;
            costtran.COGSAcctID = tran.COGSAcctID;
            costtran.COGSSubID = tran.COGSSubID;
            costtran.FinPeriodID = tran.FinPeriodID;
            costtran.TranPeriodID = tran.TranPeriodID;
            costtran.TranDate = tran.TranDate;

            //init keys
            costtran.CostID = accumlayer.CostID;
            costtran.DocType = tran.DocType;
            costtran.TranType = tran.TranType;
            costtran.RefNbr = tran.RefNbr;
            costtran.LineNbr = tran.LineNbr;
            costtran.CostDocType = tran.DocType;
            costtran.CostRefNbr = tran.RefNbr;

            costtran.InvtMult = (short?)-orig_trancost.InvtMult;
			costtran = intrancost.Insert(costtran);

			PXParentAttribute.SetParent(intrancost.Cache, costtran, typeof(INTran), tran);

			costtran.QtyOnHand += -issuedQty;

            if (UseStandardCost(accumlayer.ValMethod, tran))
            {
                costtran.TranCost += costadded;
                costtran.VarCost += -issuedCost - costadded;
            }
            else
            {
                costtran.TranCost += -issuedCost;
            }

            intrancost.Update(costtran);

            if (tran.BaseQty != 0m && tran.BaseQty == tran.CostedQty)
            {
                if (accumlayer.ValMethod == INValMethod.Standard)
                {
                    tran.TranCost = tran.OrigTranCost;
                }
            }
        }

        bool WIPCalculated = false;
        decimal? WIPVariance = 0m;

        public virtual void AssembleCost(INTran tran, INTranSplit split, InventoryItem item)
        {
            if ((tran.TranType == INTranType.Assembly || tran.TranType == INTranType.Disassembly) && tran.AssyType == INAssyType.KitTran && tran.InvtMult == (short)1)
            {
                tran.TranCost = 0m;

                //rollup stock components
                foreach (INTranCost costtran in intrancost.Cache.Inserted)
                {
                    if (string.Equals(costtran.CostDocType, tran.DocType) && string.Equals(costtran.CostRefNbr, tran.RefNbr) && costtran.InvtMult == (short)-1)
                    {
                        tran.TranCost += costtran.TranCost;
                    }
                }

                //rollup non-stock components
                foreach (INTran costtran in intranselect.Cache.Updated)
                {
                    if (string.Equals(costtran.DocType, tran.DocType) && string.Equals(costtran.RefNbr, tran.RefNbr) && costtran.AssyType == INAssyType.OverheadTran && costtran.InvtMult == (short)-1)
                    {
                        tran.TranCost += costtran.TranCost;
                    }
                }
            }

            if ((tran.TranType == INTranType.Assembly || tran.TranType == INTranType.Disassembly) && (tran.AssyType == INAssyType.CompTran || tran.AssyType == INAssyType.OverheadTran) && tran.InvtMult == (short)1)
            {
                if (WIPCalculated == false)
                {
                    //rollup kit disassembled
                    foreach (INTranCost costtran in intrancost.Cache.Inserted)
                    {
                        if (string.Equals(costtran.CostDocType, tran.DocType) && string.Equals(costtran.CostRefNbr, tran.RefNbr) && costtran.InvtMult == (short)-1)
                        {
                            WIPVariance += costtran.TranCost;
                        }
                    }
                    WIPCalculated = true;
                }
                WIPVariance -= tran.TranCost;
            }
        }

        public virtual bool IsIngoingTransfer(INTran tran)
        {
            return
                tran.TranType == INTranType.Transfer && tran.InvtMult == 1m;
        }

		protected virtual bool IsTransitTransfer(INTran tran, INCostStatus layer)
		{
			return
				tran.TranType == INTranType.Transfer && layer.CostSiteID == GetTransitCostSiteID(tran);
		}

		public virtual void UpdateCostStatus(INTran prev_tran, INTran tran, INTranSplit split, InventoryItem item)
        {
            if (object.Equals(prev_tran, tran) == false)
            {
                AssembleCost(tran, split, item);

                if (tran.BaseQty != 0m)
                {
                    tran.CostedQty = 0m;
                    tran.OrigTranCost = tran.TranCost;
                    tran.OrigTranAmt = tran.TranAmt;
                    tran.TranCost = 0m;
                    tran.TranAmt = 0m;

                    //CommandPreparing will prevent actual update
					//UnitCost and UnitPrice might be unrounded during release process
                    if (Math.Abs((decimal)tran.OrigTranCost - PXCurrencyAttribute.BaseRound(this, (decimal)tran.BaseQty * (decimal)tran.UnitCost)) > 0.00005m)
                        tran.UnitCost = (decimal)tran.OrigTranCost / (decimal)tran.BaseQty;
                    if (Math.Abs((decimal)tran.OrigTranAmt - PXCurrencyAttribute.BaseRound(this, (decimal)tran.BaseQty * (decimal)tran.UnitPrice)) > 0.00005m)
                        tran.UnitPrice = (decimal)tran.OrigTranAmt / (decimal)tran.BaseQty;
                }
                else
                {
                    //prevent SelectSiblings on null value.
                    tran.CostedQty = 0m;
                    tran.UnitCost = 0m;
                    tran.UnitPrice = 0m;
                }
            }

            DropshipCost(tran, split, item);

            try
            {
                ReceiveCost(tran, split, item, false);
                IssueCost(tran, split, item, false);
            }
            catch(PXNegativeQtyImbalanceException)
            {
                IssueCost(tran, split, item, true);
            }
            catch (PXQtyCostImbalanceException)
            {
				try
				{
                ReceiveCost(tran, split, item, true);
                IssueCost(tran, split, item, true);
            }
				catch (PXNegativeQtyImbalanceException)
				{
					ThrowNegativeQtyException(tran, split, null);
				}
            }
        }

        private void ProceedReceiveQtyForLayer(INCostStatus layer)
        {
            if (layer.LayerType != INLayerType.Normal || layer.ValMethod == INValMethod.FIFO)
                return;

            if (layer.QtyOnHand < 0m)
            {
                IssueQty(layer);
                return;
            }

            INRegister doc = inregister.Current;

            if (layer.QtyOnHand > 0m || (doc.DocType == INDocType.Receipt && layer.QtyOnHand == 0m))
            {
                ReceiptStatus receipt = new ReceiptStatus();
                receipt.InventoryID = layer.InventoryID;
                receipt.CostSiteID = layer.CostSiteID;
                receipt.CostSubItemID = layer.CostSubItemID;
                receipt.DocType = doc.DocType;
                receipt.ReceiptNbr = doc.RefNbr;
                receipt.ReceiptDate = doc.TranDate;
                receipt.OrigQty = layer.OrigQtyOnHand;
                receipt.ValMethod = layer.ValMethod;
                receipt.AccountID = layer.AccountID;
                receipt.SubID = layer.SubID;
                receipt.LotSerialNbr = layer.LotSerialNbr == null || layer.ValMethod != INValMethod.Specific ? String.Empty : layer.LotSerialNbr;
                receipt.QtyOnHand = layer.QtyOnHand;
                var prev_recstat = receiptstatus.Insert(receipt);
            }
        }

        private void ReceiveQty()
        {
            foreach (AverageCostStatus layer in averagecoststatus.Cache.Inserted)
                ProceedReceiveQtyForLayer(layer);

            foreach (StandardCostStatus layer in standardcoststatus.Cache.Inserted)
                ProceedReceiveQtyForLayer(layer);

            foreach (SpecificCostStatus layer in specificcoststatus.Cache.Inserted)
                ProceedReceiveQtyForLayer(layer);
        }

        public class INHistBucket
        {
            public decimal SignReceived = 0m;
            public decimal SignIssued = 0m;
            public decimal SignSales = 0m;
            public decimal SignCreditMemos = 0m;
            public decimal SignDropShip = 0m;
            public decimal SignTransferIn = 0m;
            public decimal SignTransferOut = 0m;
            public decimal SignAdjusted = 0m;
            public decimal SignAssemblyIn = 0m;
            public decimal SignAssemblyOut = 0m;
			public decimal SignAMAssemblyIn = 0m;
			public decimal SignAMAssemblyOut = 0m;
			public decimal SignYtd = 0m;

            public INHistBucket(INTran tran)
                : this(tran.TranType, tran.InvtMult, (short)Math.Sign(tran.BaseQty ?? 0), tran.OrigModule)
            {
            }

            public INHistBucket(INTranCost costtran, INTran intran)
                : this(costtran.TranType, costtran.InvtMult, (short)Math.Sign(costtran.Qty ?? 0), intran.OrigModule)
            {
                if ((costtran.TranType == INTranType.Transfer || costtran.TranType == INTranType.Assembly || costtran.TranType == INTranType.Disassembly) && (costtran.CostDocType != intran.DocType || costtran.CostRefNbr != intran.RefNbr))
                {
                    this.SignTransferOut = 0m;
                    this.SignSales = 1m;
                }
            }

            public INHistBucket(INTranSplit tran)
                : this(tran.TranType, tran.InvtMult, (short)Math.Sign(tran.BaseQty ?? 0), tran.OrigModule)
            {
            }

            public INHistBucket(string TranType, short? InvtMult, short? qtySign, string origModule)
            {
                SignYtd = (decimal)InvtMult;

                switch (TranType)
                {
                    case INTranType.Receipt:
						if (origModule == BatchModule.AM)
						{
							SignAssemblyIn = 1m;
							SignAMAssemblyIn = 1m;
						}
						else
						{
							SignReceived = 1m;
						}
                        break;
                    case INTranType.Issue:
						if (origModule == BatchModule.SO)
						{
							SignSales = 1m;
						}
						else if (origModule == BatchModule.AM)
						{
							SignAssemblyOut = 1m;
							SignAMAssemblyOut = 1m;
						}
						else if (origModule == BatchModule.PO)
						{
							SignReceived = -1m;
						}
						else
						{
							SignIssued = 1m;
						}
                        break;
                    case INTranType.Return:
						if (origModule == BatchModule.SO)
						{
							SignCreditMemos = 1m;
						}
						else if (origModule == BatchModule.AM)
						{
							SignAssemblyOut = -1m;
							SignAMAssemblyOut = -1m;
						}
						else if (origModule == BatchModule.IN)
						{
							SignReceived = 1m;
						}
						else
						{
							SignIssued = -1m;
						}
                        break;
                    case INTranType.Invoice:
                    case INTranType.DebitMemo:
                        if (SignYtd == 0m)
                        {
                            SignDropShip = 1m;
                        }
                        else
                        {
                            SignSales = 1m;
                        }
                        break;
                    case INTranType.CreditMemo:
                        if (SignYtd == 0m)
                        {
                            SignDropShip = -1m;
                        }
                        else
                        {
                            SignCreditMemos = 1m;
                        }
                        break;
                    case INTranType.Transfer:
                        if (InvtMult == 1m)
                        {
                            SignTransferIn = 1m;
                        }
                        else
                        {
                            SignTransferOut = 1m;
                        }
                        break;
                    case INTranType.Adjustment:
                        if (InvtMult == 0m)
                        {
                            SignAdjusted = 1m;
                            SignSales = 1m;
                        }
                        else if (qtySign == 0m)
						{
							SignAdjusted = 1m;
						}
						else if (origModule == BatchModule.AM)
						{
							if (InvtMult == 1m)
							{
								SignAssemblyIn = 1m;
								SignAMAssemblyIn = 1m;
							}
							else
							{
								SignAssemblyOut = 1m;
								SignAMAssemblyOut = 1m;
							}
						}
						else if (InvtMult == 1m)
						{
							SignReceived = 1m;
						}
						else
                        {
                            SignIssued = 1m;
                        }
                        break;
                    case INTranType.StandardCostAdjustment:
					case INTranType.NegativeCostAdjustment:
                        SignAdjusted = 1m;
                        break;
                    case INTranType.Assembly:
                    case INTranType.Disassembly:
                        if (InvtMult == 1m)
                        {
                            SignAssemblyIn = 1m;
                        }
                        else
                        {
                            SignAssemblyOut = 1m;
                        }
                        break;
                    default:
                        throw new PXException();
                }
            }
        }

        protected static void UpdateHistoryField<FinHistoryField, TranHistoryField>(PXGraph graph, object data, decimal? value, bool IsFinField)
            where FinHistoryField : IBqlField
            where TranHistoryField : IBqlField
        {
            PXCache cache = graph.Caches[BqlCommand.GetItemType(typeof(FinHistoryField))];

            if (IsFinField)
            {
                decimal? oldvalue = (decimal?)cache.GetValue<FinHistoryField>(data);

                cache.SetValue<FinHistoryField>(data, (oldvalue ?? 0m) + (value ?? 0m));
            }
            else
            {
                decimal? oldvalue = (decimal?)cache.GetValue<TranHistoryField>(data);

                cache.SetValue<TranHistoryField>(data, (oldvalue ?? 0m) + (value ?? 0m));
            }
        }

        public static void UpdateCostHist(PXGraph graph, INHistBucket bucket, INTranCost tran, Int32? siteID, string PeriodID, bool FinFlag)
        {
            ItemCostHist hist = new ItemCostHist();
            hist.InventoryID = tran.InventoryID;
            hist.CostSiteID = tran.CostSiteID;
			hist.SiteID = siteID;
            hist.AccountID = tran.InvtAcctID;
            hist.SubID = tran.InvtSubID;
            hist.CostSubItemID = tran.CostSubItemID;
            hist.FinPeriodID = PeriodID;

            hist = (ItemCostHist)graph.Caches[typeof(ItemCostHist)].Insert(hist);

            UpdateHistoryField<ItemCostHist.finPtdCostReceived, ItemCostHist.tranPtdCostReceived>(graph, hist, tran.TranCost * bucket.SignReceived, FinFlag);
            UpdateHistoryField<ItemCostHist.finPtdCostIssued, ItemCostHist.tranPtdCostIssued>(graph, hist, tran.TranCost * bucket.SignIssued, FinFlag);
            UpdateHistoryField<ItemCostHist.finPtdCOGS, ItemCostHist.tranPtdCOGS>(graph, hist, tran.TranCost * bucket.SignSales, FinFlag);
            UpdateHistoryField<ItemCostHist.finPtdCOGSCredits, ItemCostHist.tranPtdCOGSCredits>(graph, hist, tran.TranCost * bucket.SignCreditMemos, FinFlag);
            UpdateHistoryField<ItemCostHist.finPtdCOGSDropShips, ItemCostHist.tranPtdCOGSDropShips>(graph, hist, tran.TranCost * bucket.SignDropShip, FinFlag);
            UpdateHistoryField<ItemCostHist.finPtdCostTransferIn, ItemCostHist.tranPtdCostTransferIn>(graph, hist, tran.TranCost * bucket.SignTransferIn, FinFlag);
            UpdateHistoryField<ItemCostHist.finPtdCostTransferOut, ItemCostHist.tranPtdCostTransferOut>(graph, hist, tran.TranCost * bucket.SignTransferOut, FinFlag);
            UpdateHistoryField<ItemCostHist.finPtdCostAdjusted, ItemCostHist.tranPtdCostAdjusted>(graph, hist, tran.TranCost * bucket.SignAdjusted, FinFlag);
            UpdateHistoryField<ItemCostHist.finPtdCostAssemblyIn, ItemCostHist.tranPtdCostAssemblyIn>(graph, hist, tran.TranCost * bucket.SignAssemblyIn, FinFlag);
            UpdateHistoryField<ItemCostHist.finPtdCostAssemblyOut, ItemCostHist.tranPtdCostAssemblyOut>(graph, hist, tran.TranCost * bucket.SignAssemblyOut, FinFlag);
			UpdateHistoryField<ItemCostHist.finPtdCostAMAssemblyIn, ItemCostHist.tranPtdCostAMAssemblyIn>(graph, hist, tran.TranCost * bucket.SignAMAssemblyIn, FinFlag);
			UpdateHistoryField<ItemCostHist.finPtdCostAMAssemblyOut, ItemCostHist.tranPtdCostAMAssemblyOut>(graph, hist, tran.TranCost * bucket.SignAMAssemblyOut, FinFlag);

			UpdateHistoryField<ItemCostHist.finPtdQtyReceived, ItemCostHist.tranPtdQtyReceived>(graph, hist, tran.Qty * bucket.SignReceived, FinFlag);
            UpdateHistoryField<ItemCostHist.finPtdQtyIssued, ItemCostHist.tranPtdQtyIssued>(graph, hist, tran.Qty * bucket.SignIssued, FinFlag);
            UpdateHistoryField<ItemCostHist.finPtdQtySales, ItemCostHist.tranPtdQtySales>(graph, hist, tran.Qty * bucket.SignSales, FinFlag);
            UpdateHistoryField<ItemCostHist.finPtdQtyCreditMemos, ItemCostHist.tranPtdQtyCreditMemos>(graph, hist, tran.Qty * bucket.SignCreditMemos, FinFlag);
            UpdateHistoryField<ItemCostHist.finPtdQtyDropShipSales, ItemCostHist.tranPtdQtyDropShipSales>(graph, hist, tran.Qty * bucket.SignDropShip, FinFlag);
            UpdateHistoryField<ItemCostHist.finPtdQtyTransferIn, ItemCostHist.tranPtdQtyTransferIn>(graph, hist, tran.Qty * bucket.SignTransferIn, FinFlag);
            UpdateHistoryField<ItemCostHist.finPtdQtyTransferOut, ItemCostHist.tranPtdQtyTransferOut>(graph, hist, tran.Qty * bucket.SignTransferOut, FinFlag);
            UpdateHistoryField<ItemCostHist.finPtdQtyAdjusted, ItemCostHist.tranPtdQtyAdjusted>(graph, hist, tran.Qty * bucket.SignAdjusted, FinFlag);
            UpdateHistoryField<ItemCostHist.finPtdQtyAssemblyIn, ItemCostHist.tranPtdQtyAssemblyIn>(graph, hist, tran.Qty * bucket.SignAssemblyIn, FinFlag);
            UpdateHistoryField<ItemCostHist.finPtdQtyAssemblyOut, ItemCostHist.tranPtdQtyAssemblyOut>(graph, hist, tran.Qty * bucket.SignAssemblyOut, FinFlag);
			UpdateHistoryField<ItemCostHist.finPtdQtyAMAssemblyIn, ItemCostHist.tranPtdQtyAMAssemblyIn>(graph, hist, tran.Qty * bucket.SignAMAssemblyIn, FinFlag);
			UpdateHistoryField<ItemCostHist.finPtdQtyAMAssemblyOut, ItemCostHist.tranPtdQtyAMAssemblyOut>(graph, hist, tran.Qty * bucket.SignAMAssemblyOut, FinFlag);

			UpdateHistoryField<ItemCostHist.finPtdSales, ItemCostHist.tranPtdSales>(graph, hist, tran.TranAmt * bucket.SignSales, FinFlag);
            UpdateHistoryField<ItemCostHist.finPtdCreditMemos, ItemCostHist.tranPtdCreditMemos>(graph, hist, tran.TranAmt * bucket.SignCreditMemos, FinFlag);
            UpdateHistoryField<ItemCostHist.finPtdDropShipSales, ItemCostHist.tranPtdDropShipSales>(graph, hist, tran.TranAmt * bucket.SignDropShip, FinFlag);

            UpdateHistoryField<ItemCostHist.finYtdQty, ItemCostHist.tranYtdQty>(graph, hist, tran.Qty * bucket.SignYtd, FinFlag);
            UpdateHistoryField<ItemCostHist.finYtdCost, ItemCostHist.tranYtdCost>(graph, hist, tran.TranCost * bucket.SignYtd, FinFlag);
        }

        public static void UpdateCostHist(PXGraph graph, INTranCost costtran, INTran intran)
        {
            INHistBucket bucket = new INHistBucket(costtran, intran);

			UpdateCostHist(graph, bucket, costtran, intran.SiteID, costtran.FinPeriodID, true);
            UpdateCostHist(graph, bucket, costtran, intran.SiteID, costtran.TranPeriodID, false);
        }

        protected virtual void UpdateCostHist(INTranCost costtran, INTran intran)
        {
            UpdateCostHist(this, costtran, intran);
        }

        protected virtual void UpdateSalesHist(INHistBucket bucket, INTranCost tran, string PeriodID, bool FinFlag)
        {
            ItemSalesHist hist = new ItemSalesHist();
            hist.InventoryID = tran.InventoryID;
            hist.CostSiteID = tran.CostSiteID;
            hist.CostSubItemID = tran.CostSubItemID;
            hist.FinPeriodID = PeriodID;

            hist = itemsaleshist.Insert(hist);

            UpdateHistoryField<ItemSalesHist.finPtdCOGS, ItemSalesHist.tranPtdCOGS>(this, hist, tran.TranCost * bucket.SignSales, FinFlag);
            UpdateHistoryField<ItemSalesHist.finPtdCOGSCredits, ItemSalesHist.tranPtdCOGSCredits>(this, hist, tran.TranCost * bucket.SignCreditMemos, FinFlag);
            UpdateHistoryField<ItemSalesHist.finPtdCOGSDropShips, ItemSalesHist.tranPtdCOGSDropShips>(this, hist, tran.TranCost * bucket.SignDropShip, FinFlag);

            UpdateHistoryField<ItemSalesHist.finPtdQtySales, ItemSalesHist.tranPtdQtySales>(this, hist, tran.Qty * bucket.SignSales, FinFlag);
            UpdateHistoryField<ItemSalesHist.finPtdQtyCreditMemos, ItemSalesHist.tranPtdQtyCreditMemos>(this, hist, tran.Qty * bucket.SignCreditMemos, FinFlag);
            UpdateHistoryField<ItemSalesHist.finPtdQtyDropShipSales, ItemSalesHist.tranPtdQtyDropShipSales>(this, hist, tran.Qty * bucket.SignDropShip, FinFlag);

            UpdateHistoryField<ItemSalesHist.finPtdSales, ItemSalesHist.tranPtdSales>(this, hist, tran.TranAmt * bucket.SignSales, FinFlag);
            UpdateHistoryField<ItemSalesHist.finPtdCreditMemos, ItemSalesHist.tranPtdCreditMemos>(this, hist, tran.TranAmt * bucket.SignCreditMemos, FinFlag);
            UpdateHistoryField<ItemSalesHist.finPtdDropShipSales, ItemSalesHist.tranPtdDropShipSales>(this, hist, tran.TranAmt * bucket.SignDropShip, FinFlag);

            UpdateHistoryField<ItemSalesHist.finYtdCOGS, ItemSalesHist.tranYtdCOGS>(this, hist, tran.TranCost * bucket.SignSales, FinFlag);
            UpdateHistoryField<ItemSalesHist.finYtdCOGSCredits, ItemSalesHist.tranYtdCOGSCredits>(this, hist, tran.TranCost * bucket.SignCreditMemos, FinFlag);
            UpdateHistoryField<ItemSalesHist.finYtdCOGSDropShips, ItemSalesHist.tranYtdCOGSDropShips>(this, hist, tran.TranCost * bucket.SignDropShip, FinFlag);

            UpdateHistoryField<ItemSalesHist.finYtdQtySales, ItemSalesHist.tranYtdQtySales>(this, hist, tran.Qty * bucket.SignSales, FinFlag);
            UpdateHistoryField<ItemSalesHist.finYtdQtyCreditMemos, ItemSalesHist.tranYtdQtyCreditMemos>(this, hist, tran.Qty * bucket.SignCreditMemos, FinFlag);
            UpdateHistoryField<ItemSalesHist.finYtdQtyDropShipSales, ItemSalesHist.tranYtdQtyDropShipSales>(this, hist, tran.Qty * bucket.SignDropShip, FinFlag);

            UpdateHistoryField<ItemSalesHist.finYtdSales, ItemSalesHist.tranYtdSales>(this, hist, tran.TranAmt * bucket.SignSales, FinFlag);
            UpdateHistoryField<ItemSalesHist.finYtdCreditMemos, ItemSalesHist.tranYtdCreditMemos>(this, hist, tran.TranAmt * bucket.SignCreditMemos, FinFlag);
            UpdateHistoryField<ItemSalesHist.finYtdDropShipSales, ItemSalesHist.tranYtdDropShipSales>(this, hist, tran.TranAmt * bucket.SignDropShip, FinFlag);
        }

        protected virtual void UpdateSalesHist(INTranCost costtran, INTran intran)
        {
            INHistBucket bucket = new INHistBucket(costtran, intran);

            UpdateSalesHist(bucket, costtran, costtran.FinPeriodID, true);
            UpdateSalesHist(bucket, costtran, costtran.TranPeriodID, false);
        }

        protected virtual void UpdateSalesHistD(INTran intran)
        {
            UpdateSalesHistD(this, intran);
        }
        public static void UpdateSalesHistD(PXGraph graph, INTran intran)
        {
            INHistBucket bucket = new INHistBucket(intran);

            if (intran.TranDate == null || intran.BaseQty * bucket.SignSales <= 0 || intran.SubItemID == null) return;

            ItemSalesHistD hist = new ItemSalesHistD();
            hist.InventoryID = intran.InventoryID;
            hist.SiteID = intran.SiteID;
            hist.SubItemID = intran.SubItemID;
            hist.SDate = intran.TranDate;
            hist = (ItemSalesHistD)graph.Caches[typeof(ItemSalesHistD)].Insert(hist);

            DateTime date = (DateTime)intran.TranDate;
            hist.SYear = date.Year;
            hist.SMonth = date.Month;
            hist.SDay = date.Day;
            hist.SQuater = (date.Month + 2) / 3;
            hist.SDayOfWeek = (int)date.DayOfWeek;
            hist.QtyIssues += intran.BaseQty * bucket.SignSales;

            INItemSite itemsite = SelectItemSite(graph, intran.InventoryID, intran.SiteID);

            if (itemsite == null || itemsite.ReplenishmentPolicyID == null) return;

            INReplenishmentPolicy seasonality = INReplenishmentPolicy.PK.Find(graph, itemsite.ReplenishmentPolicyID);

            if (seasonality == null || seasonality.CalendarID == null) return;

            PXResult<CSCalendar, CSCalendarExceptions> result =
                (PXResult<CSCalendar, CSCalendarExceptions>)
                PXSelectJoin<CSCalendar,
                    LeftJoin<CSCalendarExceptions,
                    On<CSCalendarExceptions.calendarID, Equal<CSCalendar.calendarID>,
                    And<CSCalendarExceptions.date, Equal<Required<CSCalendarExceptions.date>>>>>,
                    Where<CSCalendar.calendarID, Equal<Required<CSCalendar.calendarID>>>>
                    .SelectWindowed(graph, 0, 1, date, seasonality.CalendarID);

            if (result != null)
            {
                CSCalendar calendar = result;
                CSCalendarExceptions exc = result;
                if (exc.Date != null)
                {
                    hist.DemandType1 = exc.WorkDay == true ? 1 : 0;
                    hist.DemandType2 = exc.WorkDay != true ? 1 : 0;
                }
                else
                {
                    hist.DemandType1 = calendar.IsWorkDay(date) ? 1 : 0;
                    hist.DemandType2 = calendar.IsWorkDay(date) ? 0 : 1;
                }
            }
        }

		protected virtual void UpdateCustSalesStats(INTran intran)
		{
			UpdateCustSalesStats(this, intran);
		}

		public static void UpdateCustSalesStats(PXGraph graph, INTran intran)
		{
			INHistBucket bucket = new INHistBucket(intran);

			if (intran.TranDate == null || intran.BaseQty == 0 || intran.BAccountID == null || intran.SubItemID == null || intran.ARRefNbr == null)
				return;

			if (bucket.SignSales != 0)
				UpdateCustStats<ItemCustSalesStats,
					ItemCustSalesStats.lastDate, 
					ItemCustSalesStats.lastQty, 
					ItemCustSalesStats.lastUnitPrice>(graph, intran);
			else if(bucket.SignDropShip != 0)
				UpdateCustStats<ItemCustDropShipStats, 
					ItemCustDropShipStats.dropShipLastDate, 
					ItemCustDropShipStats.dropShipLastQty, 
					ItemCustDropShipStats.dropShipLastUnitPrice>(graph, intran);
		}

		private static void UpdateCustStats<TStatus, TLastDate, TLastQty, TLastUnitPrice>(PXGraph graph, INTran intran)
			where TStatus: INItemCustSalesStats, new()
			where TLastDate : IBqlField
			where TLastQty : IBqlField
			where TLastUnitPrice : IBqlField
		{
			TStatus stats = new TStatus();
			stats.InventoryID = intran.InventoryID;
			stats.SubItemID = intran.SubItemID;
			stats.SiteID = intran.SiteID;
			stats.BAccountID = intran.BAccountID;
			var cache = graph.Caches[typeof(TStatus)];
			stats = (TStatus)cache.Insert(stats);

			var lastDate = (DateTime?)cache.GetValue<TLastDate>(stats);
			if (lastDate == null || lastDate < intran.TranDate)
			{
				cache.SetValue<TLastDate>(stats, intran.TranDate);
				cache.SetValue<TLastQty>(stats, intran.BaseQty);
				//during release process intran.UnitPrice is recalculated for base uom and discarded after
				decimal? unitPrice = Math.Abs((decimal)intran.TranAmt - PXCurrencyAttribute.BaseRound(cache.Graph, (decimal)intran.BaseQty * (decimal)intran.UnitPrice)) < 0.00005m
					? intran.UnitPrice
					: (intran.TranAmt / intran.BaseQty);
				cache.SetValue<TLastUnitPrice>(stats, unitPrice);
			}
		}

		protected virtual void UpdateCustSalesHist(INHistBucket bucket, INTranCost tran, string PeriodID, bool FinFlag, INTran intran)
        {
            if (intran.BAccountID == null) return;

            ItemCustSalesHist hist = new ItemCustSalesHist();
            hist.InventoryID = tran.InventoryID;
            hist.CostSiteID = tran.CostSiteID;
            hist.CostSubItemID = tran.CostSubItemID;
            hist.FinPeriodID = PeriodID;
            hist.BAccountID = intran.BAccountID;

            hist = itemcustsaleshist.Insert(hist);

            UpdateHistoryField<ItemCustSalesHist.finPtdCOGS, ItemCustSalesHist.tranPtdCOGS>(this, hist, tran.TranCost * bucket.SignSales, FinFlag);
            UpdateHistoryField<ItemCustSalesHist.finPtdCOGSCredits, ItemCustSalesHist.tranPtdCOGSCredits>(this, hist, tran.TranCost * bucket.SignCreditMemos, FinFlag);
            UpdateHistoryField<ItemCustSalesHist.finPtdCOGSDropShips, ItemCustSalesHist.tranPtdCOGSDropShips>(this, hist, tran.TranCost * bucket.SignDropShip, FinFlag);

            UpdateHistoryField<ItemCustSalesHist.finPtdQtySales, ItemCustSalesHist.tranPtdQtySales>(this, hist, tran.Qty * bucket.SignSales, FinFlag);
            UpdateHistoryField<ItemCustSalesHist.finPtdQtyCreditMemos, ItemCustSalesHist.tranPtdQtyCreditMemos>(this, hist, tran.Qty * bucket.SignCreditMemos, FinFlag);
            UpdateHistoryField<ItemCustSalesHist.finPtdQtyDropShipSales, ItemCustSalesHist.tranPtdQtyDropShipSales>(this, hist, tran.Qty * bucket.SignDropShip, FinFlag);

            UpdateHistoryField<ItemCustSalesHist.finPtdSales, ItemCustSalesHist.tranPtdSales>(this, hist, tran.TranAmt * bucket.SignSales, FinFlag);
            UpdateHistoryField<ItemCustSalesHist.finPtdCreditMemos, ItemCustSalesHist.tranPtdCreditMemos>(this, hist, tran.TranAmt * bucket.SignCreditMemos, FinFlag);
            UpdateHistoryField<ItemCustSalesHist.finPtdDropShipSales, ItemCustSalesHist.tranPtdDropShipSales>(this, hist, tran.TranAmt * bucket.SignDropShip, FinFlag);

            UpdateHistoryField<ItemCustSalesHist.finYtdCOGS, ItemCustSalesHist.tranYtdCOGS>(this, hist, tran.TranCost * bucket.SignSales, FinFlag);
            UpdateHistoryField<ItemCustSalesHist.finYtdCOGSCredits, ItemCustSalesHist.tranYtdCOGSCredits>(this, hist, tran.TranCost * bucket.SignCreditMemos, FinFlag);
            UpdateHistoryField<ItemCustSalesHist.finYtdCOGSDropShips, ItemCustSalesHist.tranYtdCOGSDropShips>(this, hist, tran.TranCost * bucket.SignDropShip, FinFlag);

            UpdateHistoryField<ItemCustSalesHist.finYtdQtySales, ItemCustSalesHist.tranYtdQtySales>(this, hist, tran.Qty * bucket.SignSales, FinFlag);
            UpdateHistoryField<ItemCustSalesHist.finYtdQtyCreditMemos, ItemCustSalesHist.tranYtdQtyCreditMemos>(this, hist, tran.Qty * bucket.SignCreditMemos, FinFlag);
            UpdateHistoryField<ItemCustSalesHist.finYtdQtyDropShipSales, ItemCustSalesHist.tranYtdQtyDropShipSales>(this, hist, tran.Qty * bucket.SignDropShip, FinFlag);

            UpdateHistoryField<ItemCustSalesHist.finYtdSales, ItemCustSalesHist.tranYtdSales>(this, hist, tran.TranAmt * bucket.SignSales, FinFlag);
            UpdateHistoryField<ItemCustSalesHist.finYtdCreditMemos, ItemCustSalesHist.tranYtdCreditMemos>(this, hist, tran.TranAmt * bucket.SignCreditMemos, FinFlag);
            UpdateHistoryField<ItemCustSalesHist.finYtdDropShipSales, ItemCustSalesHist.tranYtdDropShipSales>(this, hist, tran.TranAmt * bucket.SignDropShip, FinFlag);
        }

        protected virtual void UpdateCustSalesHist(INTranCost costtran, INTran intran)
        {
            INHistBucket bucket = new INHistBucket(costtran, intran);
            UpdateCustSalesHist(bucket, costtran, costtran.FinPeriodID, true, intran);
            UpdateCustSalesHist(bucket, costtran, costtran.TranPeriodID, false, intran);
        }

        protected static void UpdateSiteHist(PXGraph graph, INHistBucket bucket, INTranSplit tran, string PeriodID, bool FinFlag)
        {
            ItemSiteHist hist = new ItemSiteHist();
            hist.InventoryID = tran.InventoryID;
            hist.SiteID = tran.SiteID;
            hist.SubItemID = tran.SubItemID;
            hist.LocationID = tran.LocationID;
            hist.FinPeriodID = PeriodID;

            hist = (ItemSiteHist)graph.Caches[typeof(ItemSiteHist)].Insert(hist);

            UpdateHistoryField<ItemSiteHist.finPtdQtyReceived, ItemSiteHist.tranPtdQtyReceived>(graph, hist, tran.BaseQty * bucket.SignReceived, FinFlag);
            UpdateHistoryField<ItemSiteHist.finPtdQtyIssued, ItemSiteHist.tranPtdQtyIssued>(graph, hist, tran.BaseQty * bucket.SignIssued, FinFlag);
            UpdateHistoryField<ItemSiteHist.finPtdQtySales, ItemSiteHist.tranPtdQtySales>(graph, hist, tran.BaseQty * bucket.SignSales, FinFlag);
            UpdateHistoryField<ItemSiteHist.finPtdQtyCreditMemos, ItemSiteHist.tranPtdQtyCreditMemos>(graph, hist, tran.BaseQty * bucket.SignCreditMemos, FinFlag);
            UpdateHistoryField<ItemSiteHist.finPtdQtyDropShipSales, ItemSiteHist.tranPtdQtyDropShipSales>(graph, hist, tran.BaseQty * bucket.SignDropShip, FinFlag);
            UpdateHistoryField<ItemSiteHist.finPtdQtyTransferIn, ItemSiteHist.tranPtdQtyTransferIn>(graph, hist, tran.BaseQty * bucket.SignTransferIn, FinFlag);
            UpdateHistoryField<ItemSiteHist.finPtdQtyTransferOut, ItemSiteHist.tranPtdQtyTransferOut>(graph, hist, tran.BaseQty * bucket.SignTransferOut, FinFlag);
            UpdateHistoryField<ItemSiteHist.finPtdQtyAdjusted, ItemSiteHist.tranPtdQtyAdjusted>(graph, hist, tran.BaseQty * bucket.SignAdjusted, FinFlag);
            UpdateHistoryField<ItemSiteHist.finPtdQtyAssemblyIn, ItemSiteHist.tranPtdQtyAssemblyIn>(graph, hist, tran.BaseQty * bucket.SignAssemblyIn, FinFlag);
            UpdateHistoryField<ItemSiteHist.finPtdQtyAssemblyOut, ItemSiteHist.tranPtdQtyAssemblyOut>(graph, hist, tran.BaseQty * bucket.SignAssemblyOut, FinFlag);
            UpdateHistoryField<ItemSiteHist.finYtdQty, ItemSiteHist.tranYtdQty>(graph, hist, tran.BaseQty * bucket.SignYtd, FinFlag);
        }

	    protected virtual void UpdateSiteHistDay(INTran tran, INTranSplit split)
	    {
		    UpdateSiteHistDay(this, tran, split);

	    }

	    public static void UpdateSiteHistDay(PXGraph graph, INTran tran, INTranSplit split)
		{
			//for negative adjustments line InvtMult == 1 split/cost InvtMult == -1
			INHistBucket bucket = new INHistBucket(split);

			ItemSiteHistDay hist = new ItemSiteHistDay();
			hist.InventoryID = split.InventoryID;
			hist.SiteID = split.SiteID;
			hist.SubItemID = split.SubItemID;
			hist.LocationID = split.LocationID;
			DateTime date = (DateTime)split.TranDate;
			hist.SDate = date;

			hist = (ItemSiteHistDay)graph.Caches[typeof(ItemSiteHistDay)].Insert(hist);

			UpdateHistoryField<ItemSiteHistDay.qtyReceived, ItemSiteHistDay.qtyReceived>(graph, hist, split.BaseQty * bucket.SignReceived, true);
			UpdateHistoryField<ItemSiteHistDay.qtyIssued, ItemSiteHistDay.qtyIssued>(graph, hist, split.BaseQty * bucket.SignIssued, true);
			UpdateHistoryField<ItemSiteHistDay.qtySales, ItemSiteHistDay.qtySales>(graph, hist, split.BaseQty * bucket.SignSales, true);
			UpdateHistoryField<ItemSiteHistDay.qtyCreditMemos, ItemSiteHistDay.qtyCreditMemos>(graph, hist, split.BaseQty * bucket.SignCreditMemos, true);
			UpdateHistoryField<ItemSiteHistDay.qtyDropShipSales, ItemSiteHistDay.qtyDropShipSales>(graph, hist, split.BaseQty * bucket.SignDropShip, true);
			UpdateHistoryField<ItemSiteHistDay.qtyTransferIn, ItemSiteHistDay.qtyTransferIn>(graph, hist, split.BaseQty * bucket.SignTransferIn, true);
			UpdateHistoryField<ItemSiteHistDay.qtyTransferOut, ItemSiteHistDay.qtyTransferOut>(graph, hist, split.BaseQty * bucket.SignTransferOut, true);
			UpdateHistoryField<ItemSiteHistDay.qtyAdjusted, ItemSiteHistDay.qtyAdjusted>(graph, hist, split.BaseQty * bucket.SignAdjusted, true);
			UpdateHistoryField<ItemSiteHistDay.qtyAssemblyIn, ItemSiteHistDay.qtyAssemblyIn>(graph, hist, split.BaseQty * bucket.SignAssemblyIn, true);
			UpdateHistoryField<ItemSiteHistDay.qtyAssemblyOut, ItemSiteHistDay.qtyAssemblyOut>(graph, hist, split.BaseQty * bucket.SignAssemblyOut, true);
			UpdateHistoryField<ItemSiteHistDay.qtyDebit, ItemSiteHistDay.qtyDebit>(graph, hist, bucket.SignYtd > 0 ? split.BaseQty : 0m, true);
			UpdateHistoryField<ItemSiteHistDay.qtyCredit, ItemSiteHistDay.qtyCredit>(graph, hist, bucket.SignYtd < 0 ? split.BaseQty : 0m, true);

			UpdateHistoryField<ItemSiteHistDay.endQty, ItemSiteHistDay.endQty>(graph, hist, split.BaseQty * bucket.SignYtd, true);
		}

		public static void UpdateSiteHist(PXGraph graph, INTran tran, INTranSplit split)
        {
            //for negative adjustments line InvtMult == 1 split/cost InvtMult == -1
            INHistBucket bucket = new INHistBucket(split);

            UpdateSiteHist(graph, bucket, split, tran.FinPeriodID, true);
            UpdateSiteHist(graph, bucket, split, tran.TranPeriodID, false);
        }

        protected virtual void UpdateSiteHist(INTran tran, INTranSplit split)
        {
            UpdateSiteHist(this, tran, split);
        }

		public static void UpdateSiteHistByCostCenterD(PXGraph graph, INTran tran, INTranSplit split)
		{
			//for negative adjustments line InvtMult == 1 split/cost InvtMult == -1
			INHistBucket bucket = new INHistBucket(split);

			var hist = new ItemSiteHistByCostCenterD
			{
				InventoryID = split.InventoryID,
				SiteID = split.SiteID,
				SubItemID = split.SubItemID,
				CostCenterID = tran.CostCenterID,
				SDate = split.TranDate,
			};

			hist = (ItemSiteHistByCostCenterD)graph.Caches<ItemSiteHistByCostCenterD>().Insert(hist);

			FillDateFields(hist, (DateTime)split.TranDate);

			if (split.TranType == INTranType.Transfer && split.InvtMult == -1 && split.SiteID == split.ToSiteID)
			{
				bucket.SignTransferIn = -1;
				bucket.SignTransferOut = 0;
			}

			UpdateHistoryField<ItemSiteHistByCostCenterD.qtyReceived, ItemSiteHistByCostCenterD.qtyReceived>(graph, hist, split.BaseQty * bucket.SignReceived, true);
			UpdateHistoryField<ItemSiteHistByCostCenterD.qtyIssued, ItemSiteHistByCostCenterD.qtyIssued>(graph, hist, split.BaseQty * bucket.SignIssued, true);
			UpdateHistoryField<ItemSiteHistByCostCenterD.qtySales, ItemSiteHistByCostCenterD.qtySales>(graph, hist, split.BaseQty * bucket.SignSales, true);
			UpdateHistoryField<ItemSiteHistByCostCenterD.qtyCreditMemos, ItemSiteHistByCostCenterD.qtyCreditMemos>(graph, hist, split.BaseQty * bucket.SignCreditMemos, true);
			UpdateHistoryField<ItemSiteHistByCostCenterD.qtyDropShipSales, ItemSiteHistByCostCenterD.qtyDropShipSales>(graph, hist, split.BaseQty * bucket.SignDropShip, true);
			UpdateHistoryField<ItemSiteHistByCostCenterD.qtyTransferIn, ItemSiteHistByCostCenterD.qtyTransferIn>(graph, hist, split.BaseQty * bucket.SignTransferIn, true);
			UpdateHistoryField<ItemSiteHistByCostCenterD.qtyTransferOut, ItemSiteHistByCostCenterD.qtyTransferOut>(graph, hist, split.BaseQty * bucket.SignTransferOut, true);
			UpdateHistoryField<ItemSiteHistByCostCenterD.qtyAdjusted, ItemSiteHistByCostCenterD.qtyAdjusted>(graph, hist, split.BaseQty * bucket.SignAdjusted, true);
			UpdateHistoryField<ItemSiteHistByCostCenterD.qtyAssemblyIn, ItemSiteHistByCostCenterD.qtyAssemblyIn>(graph, hist, split.BaseQty * bucket.SignAssemblyIn, true);
			UpdateHistoryField<ItemSiteHistByCostCenterD.qtyAssemblyOut, ItemSiteHistByCostCenterD.qtyAssemblyOut>(graph, hist, split.BaseQty * bucket.SignAssemblyOut, true);

			if (split.SkipCostUpdate != true)
			{
				UpdateHistoryField<ItemSiteHistByCostCenterD.qtyDebit, ItemSiteHistByCostCenterD.qtyDebit>(graph, hist, bucket.SignYtd > 0 ? split.BaseQty : 0m, true);
				UpdateHistoryField<ItemSiteHistByCostCenterD.qtyCredit, ItemSiteHistByCostCenterD.qtyCredit>(graph, hist, bucket.SignYtd < 0 ? split.BaseQty : 0m, true);

				UpdateHistoryField<ItemSiteHistByCostCenterD.endQty, ItemSiteHistByCostCenterD.endQty>(graph, hist, split.BaseQty * bucket.SignYtd, true);
			}
		}

		protected static void FillDateFields(ItemSiteHistByCostCenterD hist, DateTime date)
		{
			hist.SYear = date.Year;
			hist.SMonth = date.Month;
			hist.SDay = date.Day;
			hist.SQuater = (date.Month + 2) / 3;
			hist.SDayOfWeek = (int)date.DayOfWeek;
		}

		protected virtual void UpdateSiteHistByCostCenterD(INTran tran, INTranSplit split)
		{
			UpdateSiteHistByCostCenterD(this, tran, split);
		}

		public static void UpdateSiteHistByCostCenterDCost(PXGraph graph, INTranCost costtran, INTran tran)
		{
			INHistBucket bucket = new INHistBucket(costtran, tran);

			var hist = new ItemSiteHistByCostCenterD
			{
				InventoryID = tran.InventoryID,
				SiteID = tran.SiteID,
				CostCenterID = tran.CostCenterID,
				SubItemID = costtran.CostSubItemID,
				SDate = tran.TranDate,
			};

			hist = (ItemSiteHistByCostCenterD)graph.Caches<ItemSiteHistByCostCenterD>().Insert(hist);

			FillDateFields(hist, (DateTime)tran.TranDate);

			UpdateHistoryField<ItemSiteHistByCostCenterD.costDebit, ItemSiteHistByCostCenterD.costDebit>(graph, hist, bucket.SignYtd > 0 ? costtran.TranCost : 0m, true);
			UpdateHistoryField<ItemSiteHistByCostCenterD.costCredit, ItemSiteHistByCostCenterD.costCredit>(graph, hist, bucket.SignYtd < 0 ? costtran.TranCost : 0m, true);
			UpdateHistoryField<ItemSiteHistByCostCenterD.endCost, ItemSiteHistByCostCenterD.endCost>(graph, hist, bucket.SignYtd * costtran.TranCost, true);
		}

		protected virtual void UpdateSiteHistByCostCenterDCost(INTranCost costtran, INTran tran)
		{
			UpdateSiteHistByCostCenterDCost(this, costtran, tran);
		}

        public int? GetAcctID<Field>(string AcctDefault, InventoryItem item, INSite site, INPostClass postclass)
            where Field : IBqlField
        {
            return GetAcctID<Field>(this, AcctDefault, item, site, postclass);
        }

        public static int? GetAcctID<Field>(PXGraph graph, string AcctDefault, InventoryItem item, INSite site, INPostClass postclass)
            where Field : IBqlField
        {
            return graph.GetService<IInventoryAccountService>().GetAcctID<Field>(graph, AcctDefault, item, site, postclass);
        }

        public int? GetSubID<Field>(string AcctDefault, string SubMask, InventoryItem item, INSite site, INPostClass postclass)
            where Field : IBqlField
        {
            return GetSubID<Field>(this, AcctDefault, SubMask, item, site, postclass);
        }

        public static int? GetSubID<Field>(PXGraph graph, string AcctDefault, string SubMask, InventoryItem item, INSite site, INPostClass postclass)
            where Field : IBqlField
        {
            return GetSubID<Field>(graph, AcctDefault, SubMask, item, site, postclass, null);
        }

        public int? GetSubID<Field>(string AcctDefault, string SubMask, InventoryItem item, INSite site, INPostClass postclass, INTran tran)
            where Field : IBqlField
        {
            return GetSubID<Field>(this, AcctDefault, SubMask, item, site, postclass);
        }

        public static int? GetSubID<Field>(PXGraph graph, string AcctDefault, string SubMask, InventoryItem item, INSite site, INPostClass postclass, INTran tran)
            where Field : IBqlField
        {
            return graph.GetService<IInventoryAccountService>().GetSubID<Field>(graph, AcctDefault, SubMask, item, site, postclass, tran);
        }

        public static int? GetPOAccrualAcctID<Field>(PXGraph graph, string AcctDefault, InventoryItem item, INSite site, INPostClass postclass, Vendor vendor)
            where Field : IBqlField
        {
            return graph.GetService<IInventoryAccountService>().GetPOAccrualAcctID<Field>(graph, AcctDefault, item, site, postclass, vendor);
        }

        public static int? GetPOAccrualSubID<Field>(PXGraph graph, string AcctDefault, string SubMask, InventoryItem item, INSite site, INPostClass postclass, Vendor vendor)
            where Field : IBqlField
        {
            return graph.GetService<IInventoryAccountService>().GetPOAccrualSubID<Field>(graph, AcctDefault, SubMask, item, site, postclass, vendor);
        }

		public virtual int? GetReasonCodeSubID(ReasonCode tranreasoncode, ReasonCode defreasoncode, InventoryItem item, INSite site, INPostClass postclass, INTran tran)
		{
			ReasonCode reasoncode = (tranreasoncode.AccountID == null) ? defreasoncode : tranreasoncode;
			return GetReasonCodeSubID(this, reasoncode, item, site, postclass, typeof(INPostClass.reasonCodeSubID));
		}

		public virtual int? GetReasonCodeSubID(ReasonCode reasoncode, InventoryItem item, INSite site, INPostClass postclass, INTran tran)
		{
			return GetReasonCodeSubID(this, reasoncode, item, site, postclass);
        }

		public static int? GetReasonCodeSubID(PXGraph graph, ReasonCode reasoncode, InventoryItem item, INSite site, INPostClass postclass)
		{
			return (reasoncode.AccountID == null) ? null
				: GetReasonCodeSubID(graph, reasoncode, item, site, postclass, typeof(INPostClass.reasonCodeSubID));
		}

		private static int? GetReasonCodeSubID(PXGraph graph, ReasonCode reasoncode, InventoryItem item, INSite site, INPostClass postclass, Type fieldType)
            {
                int? reasoncode_SubID = (int?)graph.Caches[typeof(ReasonCode)].GetValue<ReasonCode.subID>(reasoncode);
			int? item_SubID = (int?)graph.Caches[typeof(InventoryItem)].GetValue(item, fieldType.Name);
			int? site_SubID = (int?)graph.Caches[typeof(INSite)].GetValue(site, fieldType.Name);
			int? class_SubID = (int?)graph.Caches[typeof(INPostClass)].GetValue(postclass, fieldType.Name);

                object value = ReasonCodeSubAccountMaskAttribute.MakeSub<ReasonCode.subMask>(graph, reasoncode.SubMask,
                    new object[] { reasoncode_SubID, item_SubID, site_SubID, class_SubID },
                    new Type[] { typeof(ReasonCode.subID), typeof(InventoryItem.reasonCodeSubID), typeof(INSite.reasonCodeSubID), typeof(INPostClass.reasonCodeSubID) });

            PX.Objects.IN.Services.InventoryAccountService.RaiseFieldUpdating<ReasonCode.subID>(graph.Caches[typeof(ReasonCode)], reasoncode, ref value);
                return (int?)value;
            }

		public virtual int? GetCogsAcctID(InventoryItem item, INSite site, INPostClass postclass, INTran tran, bool useTran)
		{
			return GetAccountDefaults<INPostClass.cOGSAcctID>(this, item, site, postclass, useTran ? tran : null);
		}

		public virtual int? GetCogsSubID(InventoryItem item, INSite site, INPostClass postclass, INTran tran, bool useTran)
		{
			return GetAccountDefaults<INPostClass.cOGSSubID>(this, item, site, postclass, useTran ? tran : null);
		}

		public virtual int? GetInvtAcctID(InventoryItem item, INSite site, INPostClass postclass, INTran tran, bool useTran)
		{
			return GetAccountDefaults<INPostClass.invtAcctID>(this, item, site, postclass, useTran ? tran : null);
		}

		public virtual int? GetInvtSubID(InventoryItem item, INSite site, INPostClass postclass, INTran tran, bool useTran)
		{
			return GetAccountDefaults<INPostClass.invtSubID>(this, item, site, postclass, useTran ? tran : null);
		}

		public virtual int? GetSalesAcctID(InventoryItem item, INSite site, INPostClass postclass, INTran tran, bool useTran)
		{
			return GetAccountDefaults<INPostClass.salesAcctID>(this, item, site, postclass, useTran ? tran : null);
		}

		public virtual int? GetSalesSubID(InventoryItem item, INSite site, INPostClass postclass, INTran tran, bool useTran)
		{
			return GetAccountDefaults<INPostClass.salesSubID>(this, item, site, postclass, useTran ? tran : null);
		}

		public virtual int? GetStdCostVarAcctID(InventoryItem item, INSite site, INPostClass postclass, INTran tran, bool useTran)
		{
			return GetAccountDefaults<INPostClass.stdCstVarAcctID>(this, item, site, postclass, useTran ? tran : null);
		}

		public virtual int? GetStdCostVarSubID(InventoryItem item, INSite site, INPostClass postclass, INTran tran, bool useTran)
		{
			return GetAccountDefaults<INPostClass.stdCstVarSubID>(this, item, site, postclass, useTran ? tran : null);
        }

        public static int? GetAccountDefaults<Field>(PXGraph graph, InventoryItem item, INSite site, INPostClass postclass)
            where Field : IBqlField
        {
            return GetAccountDefaults<Field>(graph, item, site, postclass, null);
        }

		public static int? GetAccountDefaults<Field>(PXGraph graph, InventoryItem item, INSite site, INPostClass postclass, INTran tran)
			where Field : IBqlField
		{
            if (typeof(Field) == typeof(INPostClass.invtAcctID))
                return GetAcctID<Field>(graph, item.StkItem != true && postclass.InvtAcctDefault == INAcctSubDefault.MaskSite ? INAcctSubDefault.MaskItem : postclass.InvtAcctDefault, item, site, postclass);
            if (typeof(Field) == typeof(INPostClass.invtSubID))
                return GetSubID<Field>(graph, postclass.InvtAcctDefault, postclass.InvtSubMask, item, site, postclass);
            if (typeof(Field) == typeof(INPostClass.cOGSAcctID))
            {
                return GetAcctID<Field>(graph,
                    item.StkItem != true && postclass.COGSAcctDefault == INAcctSubDefault.MaskSite
                        ? INAcctSubDefault.MaskItem
                        : postclass.COGSAcctDefault, item, site, postclass);
            }
            if (typeof(Field) == typeof(INPostClass.cOGSSubID))
            {
                return GetSubID<Field>(graph, postclass.COGSAcctDefault, postclass.COGSSubMask, item, site, postclass,
                    tran);
            }
            if (typeof(Field) == typeof(INPostClass.salesAcctID))
                return GetAcctID<Field>(graph, postclass.SalesAcctDefault, item, site, postclass);
            if (typeof(Field) == typeof(INPostClass.salesSubID))
                return GetSubID<Field>(graph, postclass.SalesAcctDefault, postclass.SalesSubMask, item, site, postclass);
            if (typeof(Field) == typeof(INPostClass.stdCstVarAcctID))
                return GetAcctID<Field>(graph, postclass.StdCstVarAcctDefault, item, site, postclass);
            if (typeof(Field) == typeof(INPostClass.stdCstVarSubID))
                return GetSubID<Field>(graph, postclass.StdCstVarAcctDefault, postclass.StdCstVarSubMask, item, site, postclass);
            if (typeof(Field) == typeof(INPostClass.stdCstRevAcctID))
                return GetAcctID<Field>(graph, postclass.StdCstRevAcctDefault, item, site, postclass);
            if (typeof(Field) == typeof(INPostClass.stdCstRevSubID))
                return GetSubID<Field>(graph, postclass.StdCstRevAcctDefault, postclass.StdCstRevSubMask, item, site, postclass);

            throw new PXException();
        }

        public static INItemSite SelectItemSite(PXGraph graph, int? InventoryID, int? SiteID)
        {
            INItemSite itemsite = new INItemSite();
            itemsite.InventoryID = InventoryID;
            itemsite.SiteID = SiteID;
            itemsite = (INItemSite)graph.Caches<INItemSite>().Locate(itemsite);

            if (itemsite == null)
            {
                itemsite = INItemSite.PK.Find(graph, InventoryID, SiteID);
            }

            return itemsite;
        }

        public virtual void UpdateSOTransferPlans(long? oldPlanID, long? newPlandID)
        {
            foreach (INItemPlan itemPlan in PXSelect<INItemPlan,
				Where<INItemPlan.supplyPlanID, Equal<Required<INItemPlan.supplyPlanID>>>>
				.Select(this, oldPlanID))
            {
				var planCache = this.Caches<INItemPlan>();
                INItemPlan demand_plan = PXCache<INItemPlan>.CreateCopy(itemPlan);
				INPlanType demand_plantype = INPlanType.PK.Find(this, demand_plan.PlanType);

				//avoid ReadItem()
				planCache.SetStatus(demand_plan, PXEntryStatus.Notchanged);

                demand_plan.SupplyPlanID = newPlandID;
                if (demand_plantype.ReplanOnEvent == INPlanConstants.Plan95)
                {
                    demand_plan.PlanType = demand_plantype.ReplanOnEvent;
				}

				if (demand_plan.PlanType == INPlanConstants.Plan95 && newPlandID == null)
				{
					planCache.Delete(demand_plan);
                }
                else
                {
					planCache.Update(demand_plan);
                }
            }
        }

        public virtual INTransitLine GetCachedTransitLine(int? costsiteid)
        {
            foreach (INTransitLine itertl in intransitline.Cache.Cached)
            {
                if (itertl.CostSiteID == costsiteid)
                {
                    return itertl;
                }
            }
            return null;
        }

        public virtual void UpdateTransitPlans()
        {
            //only for transfer receipts
            if (inregister.Current.DocType != INDocType.Receipt) 
                return;

            List<INTransitLine> processed = new List<INTransitLine>();
            List<INItemPlan> newplans = new List<INItemPlan>();

            foreach (TransitLotSerialStatusByCostCenter status in transitlotnumberedstatusbycostcenter.Cache.Inserted)
            {
                if (status.QtyOnHand == 0m)
                    continue;
                INTransitLine tl = processed.Find(x => x.CostSiteID == status.LocationID);

                if (tl == null)
                {
                    tl = GetCachedTransitLine(status.LocationID);
                    processed.Add(tl);
                }

				var planCache = this.Caches<INItemPlan>();
				INItemPlan newplan = null;
				List<INItemPlan> oldPlans = PXSelect<INItemPlan,
                    Where<INItemPlan.refNoteID, Equal<Current<INTransitLine.noteID>>,
                        And<INItemPlan.inventoryID, Equal<Current<TransitLotSerialStatusByCostCenter.inventoryID>>,
                        And<INItemPlan.subItemID, Equal<Current<TransitLotSerialStatusByCostCenter.subItemID>>,
						And<INItemPlan.lotSerialNbr, Equal<Current<TransitLotSerialStatusByCostCenter.lotSerialNbr>>,
						And<INItemPlan.costCenterID, Equal<Current<TransitLotSerialStatusByCostCenter.costCenterID>>>>>>>>
					.SelectMultiBound(this, new object[] { tl, status })
					.RowCast<INItemPlan>()
					.ToList();
				foreach (INItemPlan oldplan in oldPlans)
                {
					//avoid ReadItem()
					planCache.SetStatus(oldplan, PXEntryStatus.Notchanged);

                    if (newplan == null)
                    {
                        newplan = PXCache<INItemPlan>.CreateCopy(oldplan);
						planCache.Delete(oldplan);

                        if (newplan.PlanType.IsIn(INPlanConstants.Plan42, INPlanConstants.Plan44))
                            newplan.DemandPlanID = null;

                        newplan.PlanQty += status.QtyOnHand;
                        newplan.PlanID = null;
                        newplan = (INItemPlan)planCache.Insert(newplan);
                    }
                    else
                    {
                        //case when we are merging plans
                        newplan.PlanQty += oldplan.PlanQty;

						planCache.Delete(oldplan);

						newplan = (INItemPlan)planCache.Update(newplan);
                    }
                }

				if (newplan?.PlanQty <= 0m)
				{
					planCache.Delete(newplan);
					newplan = null;
				}

				foreach (INItemPlan oldPlan in oldPlans)
				{
					UpdateSOTransferPlans(oldPlan.PlanID, newplan?.PlanID);
                }
            }

            foreach (TransitLocationStatusByCostCenter status in transitlocationstatusbycostcenter.Cache.Inserted)
            {
                INTransitLine tl = processed.Find(x => x.CostSiteID == status.LocationID);
                if (tl!=null || status.QtyOnHand == 0m)
                    continue;

                tl = GetCachedTransitLine(status.LocationID);

				var planCache = this.Caches<INItemPlan>();
				INItemPlan newplan = null;
				List<INItemPlan> oldPlans = PXSelect<INItemPlan,
                    Where<INItemPlan.refNoteID, Equal<Current<INTransitLine.noteID>>,
                        And<INItemPlan.inventoryID, Equal<Current<TransitLocationStatusByCostCenter.inventoryID>>,
						And<INItemPlan.subItemID, Equal<Current<TransitLocationStatusByCostCenter.subItemID>>,
						And<INItemPlan.costCenterID, Equal<Current<TransitLocationStatusByCostCenter.costCenterID>>>>>>>
						.SelectMultiBound(this, new object[] { tl, status })
						.RowCast<INItemPlan>()
						.ToList();
				foreach (INItemPlan oldplan in oldPlans)
                {
                    if (newplan == null)
                    {
                        newplan = PXCache<INItemPlan>.CreateCopy(oldplan);
						planCache.Delete(oldplan);

						if (newplan.PlanType.IsIn(INPlanConstants.Plan42, INPlanConstants.Plan44))
							newplan.DemandPlanID = null;

						newplan.PlanQty += status.QtyOnHand;
                        newplan.PlanID = null;
                        newplan = (INItemPlan)planCache.Insert(newplan);
                    }
                    else
                    {
                        //case when we are merging plans
                        newplan.PlanQty += oldplan.PlanQty;

						planCache.Delete(oldplan);

						newplan = (INItemPlan)planCache.Update(newplan);
                    }
                }

				if (newplan?.PlanQty <= 0m)
				{
					planCache.Delete(newplan);
					newplan = null;
				}

				foreach (INItemPlan oldPlan in oldPlans)
				{
					UpdateSOTransferPlans(oldPlan.PlanID, newplan?.PlanID);
                }
            }
        }

        public virtual void UpdateItemSite(INTran tran, InventoryItem item, INSite site, ReasonCode reasoncode, INPostClass postclass)
        {
            if (item.StkItem == true)
            {
                INItemSite itemsite = SelectItemSite(this, tran.InventoryID, tran.SiteID);

                if (itemsite == null)
                {
                    itemsite = new INItemSite();
                    itemsite.InventoryID = tran.InventoryID;
                    itemsite.SiteID = tran.SiteID;
                    var itemCurySettings = InventoryItemCurySettings.PK.Find(this, item.InventoryID, site.BaseCuryID);
                    INItemSiteMaint.DefaultItemSiteByItem(this, itemsite, item, site, postclass, itemCurySettings);
                    itemsite = initemsite.Insert(itemsite);
                }

                if (itemsite.InvtAcctID == null)
                {
                    INItemSiteMaint.DefaultInvtAcctSub(this, itemsite, item, site, postclass);
                }

                if (tran.InvtAcctID == null)
                {
                    tran.InvtAcctID = itemsite.InvtAcctID;
                    tran.InvtSubID = itemsite.InvtSubID;
                }
            }
            else
            {
                switch (tran.TranType)
                {
                    case INTranType.Receipt:
                    case INTranType.Issue:
                        if (tran.InvtAcctID == null)
                        {
                            tran.InvtAcctID = GetCogsAcctID(item, null, postclass, tran, false);
                            tran.InvtSubID = GetCogsSubID(item, null, postclass, tran, false);
                        }
                        break;
                    case INTranType.Invoice:
                    case INTranType.DebitMemo:
                    case INTranType.CreditMemo:
                    case INTranType.Assembly:
                    case INTranType.Disassembly:
                        if (tran.InvtAcctID == null && !IsUnmanagedTran(tran))
                        {
                            tran.InvtAcctID = GetInvtAcctID(item, null, postclass, tran, false);
                            tran.InvtSubID = GetInvtSubID(item, null, postclass, tran, false);
                        }
                        break;
					case INTranType.Adjustment:
						if (tran.InvtAcctID == null && !IsUnmanagedTran(tran))
						{
							tran.InvtAcctID = GetInvtAcctID(item, null, postclass, tran, false);
							tran.InvtSubID = GetInvtSubID(item, null, postclass, tran, false);
						}
						break;
                    case INTranType.Return:
                        if (tran.SOShipmentType == SOShipmentType.DropShip)
                        {
                            // TODO: DropshipReturn
                            // unify conditions
                            goto case INTranType.Invoice;
                        }
                        else
                        {
                            throw new PXException(Messages.TranType_Invalid);
                        }
                    default:
                        throw new PXException(Messages.TranType_Invalid);
                }
            }

            switch (tran.TranType)
            {
                case INTranType.Receipt:
                    if (tran.AcctID == null)
                    {
                        tran.AcctID = reasoncode.AccountID ?? ReceiptReasonCode.AccountID;
                        tran.SubID = GetReasonCodeSubID(reasoncode, ReceiptReasonCode, item, site, postclass, tran);
                        tran.ReasonCode = tran.ReasonCode ?? ReceiptReasonCode.ReasonCodeID;
                    }
                    if (tran.COGSAcctID != null)
                    {
                        tran.COGSAcctID = null;
                        tran.COGSSubID = null;
                    }
                    break;
                case INTranType.Issue:
                case INTranType.Return:
					if (tran.SOShipmentType == SOShipmentType.DropShip)
					{
						// TODO: DropshipReturn
						// unify conditions
						goto case INTranType.Invoice;
					}
					if (tran.POReceiptType == PO.POReceiptType.POReturn
						&& reasoncode.Usage.IsIn(ReasonCodeUsages.VendorReturn, ReasonCodeUsages.Issue)
						&& (tran.AcctID != null || tran.COGSAcctID != null))
					{
						break;	// preserve PO Accrual Account for PO Returns
					}
                    if (tran.AcctID != null)
                    {
                        tran.AcctID = null;
                        tran.SubID = null;
                    }
                    if (tran.COGSAcctID == null)
                    {
                        //some crazy guys manage to setup ordertype so that it will create return in inventory and will specify non-inventory reason code
						if ((reasoncode.Usage == ReasonCodeUsages.Issue || (string.IsNullOrEmpty(reasoncode.Usage) && inregister.Current.OrigModule != GL.BatchModule.SO)))
                        {
                            tran.COGSAcctID = reasoncode.AccountID ?? IssuesReasonCode.AccountID;
                            tran.COGSSubID = GetReasonCodeSubID(reasoncode, IssuesReasonCode, item, site, postclass, tran);
                            tran.ReasonCode = tran.ReasonCode ?? IssuesReasonCode.ReasonCodeID;
                        }
                        else
                        {
                            tran.COGSAcctID = GetCogsAcctID(item, site, postclass, tran, false);
                            tran.COGSSubID = GetCogsSubID(item, site, postclass, tran, false);
                        }
                    }
                    break;
                case INTranType.Invoice:
                case INTranType.DebitMemo:
                case INTranType.CreditMemo:
                    if (tran.AcctID == null)
                    {
                        tran.AcctID = GetSalesAcctID(item, site, postclass, tran, false);
                        tran.SubID = GetSalesSubID(item, site, postclass, tran, false);
                    }
                    if (tran.COGSAcctID == null)
                    {
                        if (reasoncode.Usage == ReasonCodeUsages.Issue)
                        {
                            tran.COGSAcctID = reasoncode.AccountID;
                            tran.COGSSubID = GetReasonCodeSubID(reasoncode, item, site, postclass, tran);
                        }
                        else
                        {
                            tran.COGSAcctID = GetCogsAcctID(item, site, postclass, tran, true);
                            tran.COGSSubID = GetCogsSubID(item, site, postclass, tran, (tran.InvtMult != 0));
                        }
                    }
                    break;
                case INTranType.Adjustment:
                case INTranType.StandardCostAdjustment:
                case INTranType.NegativeCostAdjustment:
                    if (tran.AcctID == null)
                    {
                        tran.AcctID = reasoncode.AccountID ?? AdjustmentReasonCode.AccountID;
                        tran.SubID = GetReasonCodeSubID(reasoncode, AdjustmentReasonCode, item, site, postclass, tran);
                        tran.ReasonCode = tran.ReasonCode ?? AdjustmentReasonCode.ReasonCodeID;
                    }
                    if (tran.COGSAcctID == null && tran.InvtMult == (short)0)
                    {
                        if (item.ValMethod == INValMethod.Standard)
                        {
                            tran.COGSAcctID = GetStdCostVarAcctID(item, site, postclass, tran, false);
                            tran.COGSSubID = GetStdCostVarSubID(item, site, postclass, tran, false);
                        }
                        else
                        {
                            tran.COGSAcctID = GetCogsAcctID(item, site, postclass, tran, false);
                            tran.COGSSubID = GetCogsSubID(item, site, postclass, tran, true);
                        }
                    }
                    if (tran.COGSAcctID != null && tran.InvtMult == (short)1)
                    {
                        tran.COGSAcctID = null;
                        tran.COGSSubID = null;
                    }
                    break;
                case INTranType.Transfer:
                    if (tran.AcctID == null)
                    {
                        tran.AcctID = INTransitAcctID;
                        tran.SubID = INTransitSubID;
                        tran.ReclassificationProhibited = true;
                    }
                    if (tran.COGSAcctID != null)
                    {
                        tran.COGSAcctID = null;
                        tran.COGSSubID = null;
                    }
                    break;
                case INTranType.Assembly:
                case INTranType.Disassembly:
                    if (tran.AcctID == null)
                    {
                        tran.AcctID = INProgressAcctID;
                        tran.SubID = INProgressSubID;
                        tran.ReclassificationProhibited = true;
                    }
                    if (tran.COGSAcctID != null)
                    {
                        tran.COGSAcctID = null;
                        tran.COGSSubID = null;
                    }
                    break;
                default:
                    throw new PXException(Messages.TranType_Invalid);
            }
        }

        private void SegregateBatch(JournalEntry je, int? branchID, string curyID, DateTime? docDate, string finPeriodID, string description)
        {
            je.created.Consolidate = je.glsetup.Current.ConsolidatedPosting ?? false;
            je.Segregate(BatchModule.IN, branchID, curyID, docDate, finPeriodID, description, null, null, null);
        }

        public virtual void WriteGLSales(JournalEntry je, INTran intran)
        {
            if (UpdateGL && intran.SalesMult != null && string.IsNullOrEmpty(intran.SOOrderNbr) && string.IsNullOrEmpty(intran.ARRefNbr))
            {
                {
                    GLTran tran = new GLTran();
                    tran.SummPost = this.SummPost;
                    tran.BranchID = intran.BranchID;
                    tran.AccountID = ARClearingAcctID;
                    tran.SubID = ARClearingSubID;

                    tran.CuryDebitAmt = (intran.SalesMult == (short)1) ? intran.TranAmt : 0m;
                    tran.DebitAmt = (intran.SalesMult == (short)1) ? intran.TranAmt : 0m;
                    tran.CuryCreditAmt = (intran.SalesMult == (short)1) ? 0m : intran.TranAmt;
                    tran.CreditAmt = (intran.SalesMult == (short)1) ? 0m : intran.TranAmt;

                    tran.TranType = intran.TranType;
                    tran.TranClass = GLTran.tranClass.Normal;
                    tran.RefNbr = intran.RefNbr;
                    tran.InventoryID = intran.InventoryID;
                    tran.Qty = (intran.SalesMult == (short)1) ? intran.Qty : -intran.Qty;
                    tran.UOM = intran.UOM;
                    tran.TranDesc = intran.TranDesc;
                    tran.TranDate = intran.TranDate;
                    tran.TranPeriodID = intran.TranPeriodID;
                    tran.FinPeriodID = intran.FinPeriodID;
                    tran.ProjectID = ProjectDefaultAttribute.NonProject();
                    tran.CostCodeID = CostCodeAttribute.DefaultCostCode;
                    tran.Released = true;
					tran.TranLineNbr = (tran.SummPost == true) ? null : intran.LineNbr;

                    InsertGLSalesDebit(je, tran, new GLTranInsertionContext() { INTran = intran });
                }

                {
                    GLTran tran = new GLTran();
                    tran.SummPost = this.SummPost;
                    tran.BranchID = intran.BranchID;
                    tran.AccountID = intran.AcctID;
                    tran.SubID = GetValueInt<INTran.subID>(je, intran);

                    tran.CuryDebitAmt = (intran.SalesMult == (short)1) ? 0m : intran.TranAmt;
                    tran.DebitAmt = (intran.SalesMult == (short)1) ? 0m : intran.TranAmt;
                    tran.CuryCreditAmt = (intran.SalesMult == (short)1) ? intran.TranAmt : 0m;
                    tran.CreditAmt = (intran.SalesMult == (short)1) ? intran.TranAmt : 0m;

                    tran.TranType = intran.TranType;
                    tran.TranClass = GLTran.tranClass.Normal;
                    tran.RefNbr = intran.RefNbr;
                    tran.InventoryID = intran.InventoryID;
                    tran.Qty = (intran.SalesMult == (short)1) ? -intran.Qty : intran.Qty;
                    tran.UOM = intran.UOM;
                    tran.TranDesc = intran.TranDesc;
                    tran.TranDate = intran.TranDate;
                    tran.TranPeriodID = intran.TranPeriodID;
                    tran.FinPeriodID = intran.FinPeriodID;
                    tran.ProjectID = ProjectDefaultAttribute.NonProject();
                    tran.CostCodeID = CostCodeAttribute.DefaultCostCode;
                    tran.Released = true;
					tran.TranLineNbr = (tran.SummPost == true) ? null : intran.LineNbr;

                    InsertGLSalesCredit(je, tran, new GLTranInsertionContext() { INTran = intran });
                }
            }
        }

        public virtual GLTran InsertGLSalesDebit(JournalEntry je, GLTran tran, GLTranInsertionContext context)
        {
            return je.GLTranModuleBatNbr.Insert(tran);
        }

        public virtual GLTran InsertGLSalesCredit(JournalEntry je, GLTran tran, GLTranInsertionContext context)
        {
            return je.GLTranModuleBatNbr.Insert(tran);
        }

        public int? GetValueInt<SourceField>(PXGraph target, object item)
            where SourceField : IBqlField
        {
            PXCache source = this.Caches[BqlCommand.GetItemType(typeof(SourceField))];
            PXCache dest = target.Caches[BqlCommand.GetItemType(typeof(SourceField))];

            object value = source.GetValueExt<SourceField>(item);
            if (value is PXFieldState)
            {
                value = ((PXFieldState)value).Value;
            }

            if (value != null)
            {
                dest.RaiseFieldUpdating<SourceField>(item, ref value);
            }

            return (int?)value;
        }

        public virtual void UpdateARTranCost(INTran tran)
        {
            UpdateARTranCost(tran, tran.TranCost);
        }

        public virtual void UpdateARTranCost(INTran tran, decimal? TranCost)
        {
            if (tran.ARRefNbr != null)
            {
                ARTranUpdate artran = new ARTranUpdate();
                artran.TranType = tran.ARDocType;
                artran.RefNbr = tran.ARRefNbr;
                artran.LineNbr = tran.ARLineNbr;

                artran = this.artranupdate.Insert(artran);

				if (tran.TranType == INTranType.TranTypeFromInvoiceType(tran.ARDocType, -1m))
				{
					// it means that ARTran has negative qty
					TranCost = -TranCost;
				}
				SOSetup sosetup = PXSelect<SOSetup>.Select(this);

				if (!(sosetup != null && sosetup.SalesProfitabilityForNSKits == SalesProfitabilityNSKitMethod.NSKitStandardCostOnly && tran.IsComponentItem == true))
				{
					artran.TranCost += TranCost;
				}

                artran.IsTranCostFinal = true;
            }
        }

        public virtual void OnTranReleased(INTran tran)
        {
            UpdatePOReceiptLineReleased(tran);
        }

		public virtual POReceiptLineUpdate UpdatePOReceiptLineReleased(INTran tran)
		{
			if (string.IsNullOrEmpty(tran.POReceiptType) || string.IsNullOrEmpty(tran.POReceiptNbr) || tran.POReceiptLineNbr == null)
				return null;

            var register = inregister.Current;
            if (register.IsTaxAdjustmentTran == true || register.IsPPVTran == true)
                return null;

			var row = new POReceiptLineUpdate
			{
				ReceiptType = tran.POReceiptType,
				ReceiptNbr = tran.POReceiptNbr,
				LineNbr = tran.POReceiptLineNbr
			};
			row = this.poreceiptlineupdate.Insert(row);

			row.INReleased = tran.Released;

			return this.poreceiptlineupdate.Update(row);
		}

		private bool IsUpdatablePOReturnTranCostFinal(INTran tran, InventoryItem item)
			=> (tran.TranType == INTranType.Issue && (tran.ExactCost != true || item.ValMethod != INValMethod.Standard))
				&& tran.POReceiptType == PO.POReceiptType.POReturn && !string.IsNullOrEmpty(tran.POReceiptNbr) && tran.POReceiptLineNbr != null
				&& tran.Qty != 0m;

        public virtual POReceiptLineUpdate UpdatePOReceiptLineCost(INTran tran, INTranCost tranCost, InventoryItem item)
		{
			bool updatePOTranCostFinal =
				IsUpdatablePOReturnTranCostFinal(tran, item)
				&& tran.DocType == tranCost.CostDocType && tran.RefNbr == tranCost.CostRefNbr;
			if (!updatePOTranCostFinal)
				return null;

			var row = new POReceiptLineUpdate
			{
				ReceiptType = tran.POReceiptType,
				ReceiptNbr = tran.POReceiptNbr,
				LineNbr = tran.POReceiptLineNbr
			};
			row = this.poreceiptlineupdate.Insert(row);

			row.UpdateTranCostFinal = true;
			row.TranCostFinal -= tranCost.InvtMult * tranCost.TranCost;

			return this.poreceiptlineupdate.Update(row);
		}

		public virtual ItemStats UpdateItemStatsLastPurchaseDate(INTran tran)
		{
			if (tran.OrigModule != BatchModule.PO || tran.TranType != INTranType.Receipt || tran.InvtMult != 1) return null;

			var stats = new ItemStats
			{
				InventoryID = tran.InventoryID,
				SiteID = tran.SiteID
			};

			stats = itemstats.Insert(stats);

			stats.LastPurchaseDate = tran.TranDate;

			return itemstats.Update(stats);
		}

		public virtual void WriteGLNonStockCosts(JournalEntry je, INTran intran, InventoryItem item, INSite site)
        {
            bool isProjectDropShip = IsProjectDropShip(intran);
            bool isDropshipReturn = (intran?.SOShipmentType == SOShipmentType.DropShip || isProjectDropShip) && intran.POReceiptType == PO.POReceiptType.POReturn;

            if ((item.StkItem == false || isProjectDropShip) && (intran.COGSAcctID != null || intran.AcctID != null))
            {
                GLTran tran = new GLTran();
                tran.SummPost = this.SummPost;
                tran.BranchID = intran.BranchID;
                tran.AccountID = (intran.InvtMult == (short)0) ? intran.AcctID : intran.InvtAcctID;
                tran.SubID = (intran.InvtMult == (short)0) ? intran.SubID : GetValueInt<INTran.invtSubID>(je, intran);

                tran.CuryDebitAmt = (intran.InvtMult == (short)1 || isDropshipReturn) ? intran.TranCost : 0m;
                tran.DebitAmt = (intran.InvtMult == (short)1 || isDropshipReturn) ? intran.TranCost : 0m;
                tran.CuryCreditAmt = (intran.InvtMult == (short)1 || isDropshipReturn) ? 0m : intran.TranCost;
                tran.CreditAmt = (intran.InvtMult == (short)1 || isDropshipReturn) ? 0m : intran.TranCost;

                tran.TranType = intran.TranType;
                tran.TranClass = GLTran.tranClass.Normal;
                tran.RefNbr = intran.RefNbr;
                tran.InventoryID = intran.InventoryID;
                tran.Qty = (intran.InvtMult == (short)1 || isDropshipReturn) ? intran.Qty : -intran.Qty;
                tran.UOM = intran.UOM;
                tran.TranDesc = intran.TranDesc;
                tran.TranDate = intran.TranDate;
                tran.TranPeriodID = intran.TranPeriodID;
                tran.FinPeriodID = intran.FinPeriodID;
                tran.ProjectID = ProjectDefaultAttribute.NonProject();
                tran.CostCodeID = CostCodeAttribute.DefaultCostCode;
				
				tran.Released = true;
				tran.TranLineNbr = (tran.SummPost == true) ? null : intran.LineNbr;

				InsertGLNonStockCostDebit(je, tran, new GLTranInsertionContext() { INTran = intran, Item = item, Site = site });
            }

            if ((item.StkItem == false || isProjectDropShip) && (intran.COGSAcctID != null || intran.AcctID != null))
            {
                GLTran tran = new GLTran();
                tran.SummPost = this.SummPost;
                tran.BranchID = intran.BranchID;
                tran.AccountID = (intran.COGSAcctID ?? intran.AcctID);
                tran.SubID = (GetValueInt<INTran.cOGSSubID>(je, intran) ?? GetValueInt<INTran.subID>(je, intran));

                tran.CuryDebitAmt = (intran.InvtMult == (short)1 || isDropshipReturn) ? 0m : intran.TranCost;
                tran.DebitAmt = (intran.InvtMult == (short)1 || isDropshipReturn) ? 0m : intran.TranCost;
                tran.CuryCreditAmt = (intran.InvtMult == (short)1 || isDropshipReturn) ? intran.TranCost : 0m;
                tran.CreditAmt = (intran.InvtMult == (short)1 || isDropshipReturn) ? intran.TranCost : 0m;

                tran.TranType = intran.TranType;
                tran.TranClass = GLTran.tranClass.Normal;
                tran.RefNbr = intran.RefNbr;
                tran.InventoryID = intran.InventoryID;
                tran.Qty = (intran.InvtMult == (short)1 || isDropshipReturn) ? -intran.Qty : intran.Qty;
                tran.UOM = intran.UOM;
                tran.TranDesc = intran.TranDesc;
                tran.TranDate = intran.TranDate;
                tran.TranPeriodID = intran.TranPeriodID;
                tran.FinPeriodID = intran.FinPeriodID;
                tran.ProjectID = ProjectDefaultAttribute.NonProject();
                tran.CostCodeID = CostCodeAttribute.DefaultCostCode;
				tran.ReclassificationProhibited = isProjectDropShip;
				tran.Released = true;
				tran.TranLineNbr = (tran.SummPost == true) ? null : intran.LineNbr;

				InsertGLNonStockCostCredit(je, tran, new GLTranInsertionContext() { INTran = intran, Item = item, Site = site });
            }
        }
               
        public virtual GLTran InsertGLNonStockCostDebit(JournalEntry je, GLTran tran, GLTranInsertionContext context)
        {
            return je.GLTranModuleBatNbr.Insert(tran);
        }

        public virtual GLTran InsertGLNonStockCostCredit(JournalEntry je, GLTran tran, GLTranInsertionContext context)
        {
            return je.GLTranModuleBatNbr.Insert(tran);
        }

        public virtual void WriteGLCosts(JournalEntry je, INTranCost trancost, INTran intran, InventoryItem item, INSite site, INPostClass postclass, ReasonCode reasoncode, INLocation location)
        {
            bool isStdDropShip = intran != null && intran.SOShipmentType == SOShipmentType.DropShip && intran.POReceiptNbr != null && trancost.InvtMult == 0 && item.ValMethod == INValMethod.Standard;
            bool isDropshipReturn = intran?.SOShipmentType == SOShipmentType.DropShip && intran.POReceiptType == PO.POReceiptType.POReturn;

            if (trancost.COGSAcctID != null || intran.AcctID != null)
            {
                GLTran tran = new GLTran();
                tran.SummPost = trancost.TranType == INTranType.Transfer && intran.DocType == trancost.CostDocType ? true : this.SummPost;
				tran.TranDate = trancost.TranDate;
				tran.TranPeriodID = trancost.TranPeriodID;
				tran.FinPeriodID = trancost.FinPeriodID;

				if (trancost.InvtMult == (short)0)
                {
                    tran.BranchID = intran.BranchID;
                    tran.AccountID = intran.AcctID;
                    tran.SubID = intran.SubID;
                    tran.ReclassificationProhibited = intran.ReclassificationProhibited;
                }
                else
                {
					tran.BranchID = site.BranchID;
                    tran.AccountID = trancost.InvtAcctID;
                    tran.SubID = GetValueInt<INTranCost.invtSubID>(je, trancost);
                    tran.ReclassificationProhibited = true;
					if (intran.BranchID != site.BranchID)
						FinPeriodIDAttribute.SetPeriodsByMaster<GLTran.finPeriodID>(je.GLTranModuleBatNbr.Cache, tran, tran.TranPeriodID);
				}

                if (isStdDropShip)
                {
                    tran.CuryDebitAmt = isDropshipReturn ? (trancost.TranCost + trancost.VarCost) : 0m;
                    tran.DebitAmt = isDropshipReturn ? (trancost.TranCost + trancost.VarCost) : 0m;
                    tran.CuryCreditAmt = isDropshipReturn ? 0m : (trancost.TranCost + trancost.VarCost);
                    tran.CreditAmt = isDropshipReturn ? 0m : (trancost.TranCost + trancost.VarCost);
                }
                else
                {
                    tran.CuryDebitAmt = (trancost.InvtMult == (short)1 || isDropshipReturn) ? trancost.TranCost : 0m;
                    tran.DebitAmt = (trancost.InvtMult == (short)1 || isDropshipReturn) ? trancost.TranCost : 0m;
                    tran.CuryCreditAmt = (trancost.InvtMult == (short)1 || isDropshipReturn) ? 0m : trancost.TranCost;
                    tran.CreditAmt = (trancost.InvtMult == (short)1 || isDropshipReturn) ? 0m : trancost.TranCost;
                }

                tran.TranType = trancost.TranType;
                tran.TranClass = GLTran.tranClass.Normal;
                tran.ZeroPost = trancost.CostDocType == intran.DocType && trancost.CostRefNbr == intran.RefNbr && tran.AccountID != null && tran.SubID != null;
                tran.RefNbr = trancost.RefNbr;
                tran.InventoryID = trancost.InventoryID;
                tran.Qty = (trancost.InvtMult == (short)1 || isDropshipReturn) ? trancost.Qty : -trancost.Qty;
                tran.UOM = item.BaseUnit;
                tran.TranDesc = intran.TranDesc;
                tran.ProjectID = ProjectDefaultAttribute.NonProject();
                tran.CostCodeID = CostCodeAttribute.DefaultCostCode;
                tran.Released = true;
				tran.TranLineNbr = (tran.SummPost == true) ? null : intran.LineNbr;

                InsertGLCostsDebit(je, tran, new GLTranInsertionContext() { TranCost = trancost, INTran = intran, Item = item, Site = site, PostClass = postclass, ReasonCode = reasoncode, Location = location });
            }

            if (item.ValMethod == INValMethod.Standard && (trancost.COGSAcctID != null || intran.AcctID != null) && Math.Abs(trancost.VarCost ?? 0m) > 0.00005m)
            {
                GLTran tran = new GLTran();
                tran.SummPost = this.SummPost;
                tran.BranchID = intran.BranchID;
                tran.AccountID = GetStdCostVarAcctID(item, site, postclass, intran, false);
                tran.SubID = GetStdCostVarSubID(item, site, postclass, intran, false);

                if (isStdDropShip)
                {
                    tran.CuryDebitAmt = isDropshipReturn ? 0m : trancost.VarCost;
                    tran.DebitAmt = isDropshipReturn ? 0m : trancost.VarCost;
                    tran.CuryCreditAmt = isDropshipReturn ? trancost.VarCost : 0m;
                    tran.CreditAmt = isDropshipReturn ? trancost.VarCost : 0m;
                }
                else
                {
                    tran.CuryDebitAmt = (trancost.InvtMult == (short)1 || isDropshipReturn) ? trancost.VarCost : 0m;
                    tran.DebitAmt = (trancost.InvtMult == (short)1 || isDropshipReturn) ? trancost.VarCost : 0m;
                    tran.CuryCreditAmt = (trancost.InvtMult == (short)1 || isDropshipReturn) ? 0m : trancost.VarCost;
                    tran.CreditAmt = (trancost.InvtMult == (short)1 || isDropshipReturn) ? 0m : trancost.VarCost;
                }

                tran.TranType = trancost.TranType;
                tran.TranClass = GLTran.tranClass.Normal;
                tran.ZeroPost = trancost.CostDocType == intran.DocType && trancost.CostRefNbr == intran.RefNbr && tran.AccountID != null && tran.SubID != null;
                tran.RefNbr = trancost.RefNbr;
                tran.InventoryID = trancost.InventoryID;
                tran.Qty = (trancost.InvtMult == (short)1 || isDropshipReturn) ? trancost.Qty : -trancost.Qty;
                tran.UOM = item.BaseUnit;
                tran.TranDesc = intran.TranDesc;
				tran.TranDate = trancost.TranDate;
				tran.TranPeriodID = trancost.TranPeriodID;
				tran.FinPeriodID = trancost.FinPeriodID;
                tran.ProjectID = ProjectDefaultAttribute.NonProject();
                tran.CostCodeID = CostCodeAttribute.DefaultCostCode;
                tran.Released = true;
				tran.TranLineNbr = (tran.SummPost == true) ? null : intran.LineNbr;

                InsertGLCostsCredit(je, tran, new GLTranInsertionContext() { TranCost = trancost, INTran = intran, Item = item, Site = site, PostClass = postclass, ReasonCode = reasoncode, Location = location });
            }

            if (trancost.COGSAcctID != null || intran.AcctID != null)
            {
                //oversold transfers go to COGS instead of GIT
                if (trancost.TranType == INTranType.Transfer && (trancost.CostDocType != intran.DocType || trancost.CostRefNbr != intran.RefNbr))
                {
                    trancost.COGSAcctID = GetCogsAcctID(item, site, postclass, intran, false);
                    trancost.COGSSubID = GetCogsSubID(item, site, postclass, intran, false);
                }
                //oversold Assemblies go to Variance instead of WIP
                if ((trancost.TranType == INTranType.Assembly || trancost.TranType == INTranType.Disassembly) && reasoncode != null && reasoncode.AccountID != null && (trancost.CostDocType != intran.DocType || trancost.CostRefNbr != intran.RefNbr))
                {
                    trancost.COGSAcctID = reasoncode.AccountID;
                    trancost.COGSSubID = GetReasonCodeSubID(reasoncode, item, site, postclass, intran);
                }
                //oversold for issues with UpdateShippedNotInvoiced = true go to COGS
                if (intran != null && intran.UpdateShippedNotInvoiced == true && trancost.TranType == INTranType.Receipt && (trancost.CostDocType != intran.DocType || trancost.CostRefNbr != intran.RefNbr))
                {
                    trancost.COGSAcctID = GetCogsAcctID(item, site, postclass, intran, false);
                    trancost.COGSSubID = GetCogsSubID(item, site, postclass, intran, false);
                }
				//oversold PO Returns go to PPV instead of Purchase Accrual
				if (IsUpdatablePOReturnTranCostFinal(intran, item) && (trancost.CostDocType != intran.DocType || trancost.CostRefNbr != intran.RefNbr))
				{
					trancost.COGSAcctID = GetAcctID<INPostClass.pPVAcctID>(this, postclass.PPVAcctDefault, item, site, postclass);
					try
					{
						trancost.COGSSubID = GetSubID<INPostClass.pPVSubID>(this, postclass.PPVAcctDefault, postclass.PPVSubMask, item, site, postclass);
					}
					catch (PXException)
					{
						throw new PXException(AP.Messages.PPVSubAccountMaskCanNotBeAssembled);
					}
				}

                GLTran tran = new GLTran();
                tran.SummPost = (trancost.TranType == INTranType.Transfer || trancost.TranType == INTranType.Assembly || trancost.TranType == INTranType.Disassembly) && intran.DocType == trancost.CostDocType ? true : this.SummPost;
				tran.BranchID = trancost.COGSAcctID == null ? intran.DestBranchID ?? intran.BranchID : intran.BranchID;
                tran.AccountID = (trancost.COGSAcctID ?? intran.AcctID);
                tran.SubID = (GetValueInt<INTranCost.cOGSSubID>(je, trancost) ?? GetValueInt<INTran.subID>(je, intran));

                if (isStdDropShip)
                {
                    tran.CuryDebitAmt = isDropshipReturn ? 0m : trancost.TranCost;
                    tran.DebitAmt = isDropshipReturn ? 0m : trancost.TranCost;
                    tran.CuryCreditAmt = isDropshipReturn ? trancost.TranCost : 0m;
                    tran.CreditAmt = isDropshipReturn ? trancost.TranCost : 0m;
                }
                else
                {
                    tran.CuryDebitAmt = (trancost.InvtMult == (short)1 || isDropshipReturn) ? 0m : trancost.TranCost + (item.ValMethod == INValMethod.Standard ? trancost.VarCost : 0m);
                    tran.DebitAmt = (trancost.InvtMult == (short)1 || isDropshipReturn) ? 0m : trancost.TranCost + (item.ValMethod == INValMethod.Standard ? trancost.VarCost : 0m);
                    tran.CuryCreditAmt = (trancost.InvtMult == (short)1 || isDropshipReturn) ? trancost.TranCost + (item.ValMethod == INValMethod.Standard ? trancost.VarCost : 0m) : 0m;
                    tran.CreditAmt = (trancost.InvtMult == (short)1 || isDropshipReturn) ? trancost.TranCost + (item.ValMethod == INValMethod.Standard ? trancost.VarCost : 0m) : 0m;
                }

                tran.TranType = trancost.TranType;
                tran.TranClass = GLTran.tranClass.Normal;
                tran.ZeroPost = trancost.CostDocType == intran.DocType && trancost.CostRefNbr == intran.RefNbr && tran.AccountID != null && tran.SubID != null;
                tran.RefNbr = trancost.RefNbr;
                tran.InventoryID = trancost.InventoryID;
                tran.Qty = (trancost.InvtMult == (short)1 || isDropshipReturn) ? -trancost.Qty : trancost.Qty;
                tran.UOM = item.BaseUnit;
                tran.TranDesc = intran.TranDesc;
				tran.TranDate = trancost.TranDate;
				tran.TranPeriodID = trancost.TranPeriodID;
				tran.FinPeriodID = trancost.FinPeriodID;
                tran.ProjectID = ProjectDefaultAttribute.NonProject();
                tran.CostCodeID = CostCodeAttribute.DefaultCostCode;
                tran.Released = true;
                tran.TranLineNbr = (tran.SummPost == true) ? null : intran.LineNbr;

                InsertGLCostsOversold(je, tran, new GLTranInsertionContext() { TranCost = trancost, INTran = intran, Item = item, Site = site, PostClass = postclass, ReasonCode = reasoncode, Location = location });
            }

            //Write off production variance from WIP
            if (WIPCalculated && intran.AssyType == INAssyType.KitTran && string.Equals(trancost.CostDocType, intran.DocType) && string.Equals(trancost.CostRefNbr, intran.RefNbr))
            {
                GLTran tran = new GLTran();
                tran.SummPost = true;
                tran.ZeroPost = false;
                tran.BranchID = intran.BranchID;
                tran.AccountID = INProgressAcctID;
                tran.SubID = INProgressSubID;
                tran.ReclassificationProhibited = true;

                tran.CuryDebitAmt = 0m;
                tran.DebitAmt = 0m;
                tran.CuryCreditAmt = WIPVariance;
                tran.CreditAmt = WIPVariance;

                tran.TranType = intran.TranType;
                tran.TranClass = GLTran.tranClass.Normal;
                tran.RefNbr = intran.RefNbr;
                tran.InventoryID = intran.InventoryID;
                tran.TranDesc = PXMessages.LocalizeNoPrefix(Messages.ProductionVarianceTranDesc);
                tran.TranDate = intran.TranDate;
                tran.TranPeriodID = intran.TranPeriodID;
                tran.FinPeriodID = intran.FinPeriodID;
                tran.ProjectID = ProjectDefaultAttribute.NonProject();
                tran.CostCodeID = CostCodeAttribute.DefaultCostCode;
                tran.Released = true;
                tran.TranLineNbr = null;

                InsertGLCostsVarianceCredit(je, tran, new GLTranInsertionContext() { TranCost = trancost, INTran = intran, Item = item, Site = site, PostClass = postclass, ReasonCode = reasoncode, Location = location });
            }

            if (WIPCalculated && intran.AssyType == INAssyType.KitTran && string.Equals(trancost.CostDocType, intran.DocType) && string.Equals(trancost.CostRefNbr, intran.RefNbr))
            {
                GLTran tran = new GLTran();
                tran.SummPost = this.SummPost;
                tran.BranchID = intran.BranchID;
                tran.AccountID = reasoncode.AccountID;
                tran.SubID = GetReasonCodeSubID(reasoncode, item, site, postclass, intran);

                tran.CuryDebitAmt = WIPVariance;
                tran.DebitAmt = WIPVariance;
                tran.CuryCreditAmt = 0m;
                tran.CreditAmt = 0m;

                tran.TranType = intran.TranType;
                tran.TranClass = GLTran.tranClass.Normal;
                tran.RefNbr = intran.RefNbr;
                tran.InventoryID = intran.InventoryID;
                tran.TranDesc = PXMessages.LocalizeNoPrefix(Messages.ProductionVarianceTranDesc);
                tran.TranDate = intran.TranDate;
                tran.TranPeriodID = intran.TranPeriodID;
                tran.FinPeriodID = intran.FinPeriodID;
                tran.ProjectID = ProjectDefaultAttribute.NonProject();
                tran.CostCodeID = CostCodeAttribute.DefaultCostCode;
                tran.Released = true;
                tran.TranLineNbr = null;

                InsertGLCostsVarianceDebit(je, tran, new GLTranInsertionContext() { TranCost = trancost, INTran = intran, Item = item, Site = site, PostClass = postclass, ReasonCode = reasoncode, Location = location });

                WIPCalculated = false;
                WIPVariance = 0m;
            }
        }
               
        public virtual GLTran InsertGLCostsDebit(JournalEntry je, GLTran tran, GLTranInsertionContext context)
        {
            return je.GLTranModuleBatNbr.Insert(tran);
        }

        public virtual GLTran InsertGLCostsCredit(JournalEntry je, GLTran tran, GLTranInsertionContext context)
        {
            return je.GLTranModuleBatNbr.Insert(tran);
        }

        public virtual GLTran InsertGLCostsOversold(JournalEntry je, GLTran tran, GLTranInsertionContext context)
        {
            return je.GLTranModuleBatNbr.Insert(tran);
        }
        
        public virtual GLTran InsertGLCostsVarianceCredit(JournalEntry je, GLTran tran, GLTranInsertionContext context)
        {
            return je.GLTranModuleBatNbr.Insert(tran);
        }

        public virtual GLTran InsertGLCostsVarianceDebit(JournalEntry je, GLTran tran, GLTranInsertionContext context)
        {
            return je.GLTranModuleBatNbr.Insert(tran);
        }

        public object GetValueExt<Field>(PXCache cache, object data)
            where Field : class, IBqlField
        {
            object val = cache.GetValueExt<Field>(data);

            if (val is PXFieldState)
            {
                return ((PXFieldState)val).Value;
            }
            else
            {
                return val;
            }
        }

        List<Segment> _SubItemSeg = null;
        Dictionary<short?, string> _SubItemSegVal = null;

        public virtual void ParseSubItemSegKeys()
        {
            if (_SubItemSeg == null)
            {
                _SubItemSeg = new List<Segment>();

                foreach (Segment seg in PXSelect<Segment, Where<Segment.dimensionID, Equal<IN.SubItemAttribute.dimensionName>>>.Select(this))
                {
                    _SubItemSeg.Add(seg);
                }

                _SubItemSegVal = new Dictionary<short?, string>();

                foreach (SegmentValue val in PXSelectJoin<SegmentValue, InnerJoin<Segment, On<Segment.dimensionID, Equal<SegmentValue.dimensionID>, And<Segment.segmentID, Equal<SegmentValue.segmentID>>>>, Where<SegmentValue.dimensionID, Equal<IN.SubItemAttribute.dimensionName>, And<Segment.isCosted, Equal<boolFalse>, And<SegmentValue.isConsolidatedValue, Equal<boolTrue>>>>>.Select(this))
                {
                    try
                    {
                        _SubItemSegVal.Add((short)val.SegmentID, val.Value);
                    }
                    catch (Exception excep)
                    {
                        throw new PXException(excep, Messages.MultipleAggregateChecksEncountred, val.SegmentID, val.DimensionID);
                    }
                }
            }
        }

        public virtual string MakeCostSubItemCD(string SubItemCD)
        {
            StringBuilder sb = new StringBuilder();

            int offset = 0;

            foreach (Segment seg in _SubItemSeg)
            {
                string segval = SubItemCD.Substring(offset, (int)seg.Length);
                if (seg.IsCosted == true || segval.TrimEnd() == string.Empty)
                {
                    sb.Append(segval);
                }
                else
                {
                    if (_SubItemSegVal.TryGetValue(seg.SegmentID, out segval))
                    {
						segval = segval.PadRight((int)seg.Length);
						sb.Append(segval);
                    }
                    else
                    {
                        throw new PXException(Messages.SubItemSeg_Missing_ConsolidatedVal);
                    }
                }
                offset += (int)seg.Length;
            }

            return sb.ToString();
        }

        public virtual void UpdateCrossReference(INTran tran, INTranSplit split, InventoryItem item, INLocation whseloc)
        {
            if (item.ValMethod != INValMethod.Standard && item.ValMethod != INValMethod.Specific && whseloc == null)
            {
                throw new PXException(ErrorMessages.FieldIsEmpty, PXUIFieldAttribute.GetDisplayName<INTran.locationID>(intranselect.Cache));
            }

            if (split.SubItemID == null)
            {
                throw new PXException(ErrorMessages.FieldIsEmpty, PXUIFieldAttribute.GetDisplayName<INTran.subItemID>(intranselect.Cache));
            }

            INCostSubItemXRef xref = new INCostSubItemXRef();

            xref.SubItemID = split.SubItemID;
            xref.CostSubItemID = split.SubItemID;

            string SubItemCD = (string)this.GetValueExt<INCostSubItemXRef.costSubItemID>(costsubitemxref.Cache, xref);

            xref.CostSubItemID = null;

            string CostSubItemCD = PXAccess.FeatureInstalled<FeaturesSet.subItem>() ? MakeCostSubItemCD(SubItemCD) : SubItemCD;

            costsubitemxref.Cache.SetValueExt<INCostSubItemXRef.costSubItemID>(xref, CostSubItemCD);
            xref = costsubitemxref.Update(xref);

            if (costsubitemxref.Cache.GetStatus(xref) == PXEntryStatus.Updated)
            {
                costsubitemxref.Cache.SetStatus(xref, PXEntryStatus.Notchanged);
            }

            split.CostSubItemID = xref.CostSubItemID;
            //Standard & Specific Cost items will ignore per location costing, Standard can have null location
			split.CostSiteID = (tran.CostCenterID == CostCenter.FreeStock)
				? (item.ValMethod != INValMethod.Standard && item.ValMethod != INValMethod.Specific && whseloc.IsCosted == true ? whseloc.LocationID : split.SiteID)
				: tran.CostCenterID;
        }

        public virtual void ReleaseDocProcR(JournalEntry je, INRegister doc, bool releaseFromHold = false)
        {
            int retryCnt = 5;
            while (true)
            {
                try
                {
                    ReleaseDocProc(je, doc, releaseFromHold);
                    return;
                }
                catch (PXRestartOperationException e)
                {
                    if (retryCnt-- < 0)
                    {
                        if (e.InnerException != null)
                            throw e.InnerException;
                        else
                            throw;
                    }
                    else
                    {
                        this.Clear();
                    }
                }
            }
        }

        public virtual void UpdateSplitDestinationLocation(INTran tran, INTranSplit split, int? value)
        {
            split.ToLocationID = value;

            if (IsOneStepTransferWithinSite())
            {
                INLocation originLocation = INTranSplit.FK.Location.FindParent(this, split);
                INLocation targetLocation = INTranSplit.FK.ToLocation.FindParent(this, split);

                split.SkipCostUpdate = (originLocation?.IsCosted) != true && (targetLocation?.IsCosted) != true;
                split.SkipQtyValidation = targetLocation?.IsSorting == true;
            }
            intransplit.Cache.MarkUpdated(split, assertError: true);
        }

        public virtual bool IsOneStepTransfer()
        {
            INRegister doc = inregister.Current;
            return doc.DocType == INDocType.Transfer && doc.TransferType == INTransferType.OneStep;
        }

		public virtual bool IsOneStepTransferWithinSite()
		{
			INRegister doc = inregister.Current;
			return doc.DocType == INDocType.Transfer && doc.TransferType == INTransferType.OneStep && doc.SiteID == doc.ToSiteID;
		}

		public virtual void ValidateTransferDocIntegrity(INRegister doc)
        {
			foreach (PXResult<INTranSplit,INTran, INItemPlan> res in PXSelectReadonly2<INTranSplit,
				InnerJoin<INTran,
					On<INTranSplit.FK.Tran>,
				LeftJoin<INItemPlan,
					On<INTranSplit.FK.ItemPlan>>>,
				Where<INTranSplit.docType, Equal<Required<INTranSplit.docType>>,
					And<INTranSplit.refNbr, Equal<Required<INTranSplit.refNbr>>>>>.Select(this, doc.DocType, doc.RefNbr))
            {
				INTranSplit split = res;
				INTran tran = res;
				INItemPlan plan = res;

				if (split.TransferType != doc.TransferType
					|| split.TransferType == INTransferType.TwoStep
					&& TranSplitPlanExt.IsTwoStepTransferPlanValid(split, plan) == false)
				{
					throw new PXException(Messages.TransferIsCorrupted, doc.RefNbr);
				}
				else if (split.TransferType == INTransferType.OneStep && split.InvtMult == (short)-1 && tran.ToLocationID == null)
				{
					throw new PXException(Messages.ToLocationIsEmptyForAtLeastOneTransferLine);
				}
			}
		}

		public virtual void ValidateKitAssembly(INRegister doc)
		{
			if (doc.DocType != INDocType.Production)
			{
				return;
			}

			PXResult kitDoc = SelectFrom<INKitRegister>.InnerJoin<INLocation>.On<INLocation.locationID.IsEqual<INKitRegister.locationID>>.
				Where<INKitRegister.docType.IsEqual<@P.AsString.ASCII>.And<INKitRegister.refNbr.IsEqual<@P.AsString>>>.View.Select(this, doc.DocType, doc.RefNbr);
			if (kitDoc != null)
			{
				INLocation location = kitDoc[typeof(INLocation)] as INLocation;
				if (location != null && location.AssemblyValid != true)
				{
					throw new PXException(Messages.LocationAssemblyInvalid, location.LocationCD);
				}
			}
		}

		public virtual void ValidateOrigLCReceiptIsReleased(INRegister doc)
		{
			if (doc.DocType != INDocType.Adjustment)
				return;

			var lcTransWithOrig = PXSelectJoin<INTran,
				LeftJoin<INTran2,
					On<INTran2.docType, In3<INDocType.receipt, INDocType.issue>,
						And<INTran2.pOReceiptType, Equal<INTran.pOReceiptType>,
						And<INTran2.pOReceiptNbr, Equal<INTran.pOReceiptNbr>,
						And<INTran2.pOReceiptLineNbr, Equal<INTran.pOReceiptLineNbr>>>>>>,
				Where<INTran.docType, Equal<Current<INRegister.docType>>,
					And<INTran.refNbr, Equal<Current<INRegister.refNbr>>,
					And<INTran.tranType, In3<INTranType.adjustment, INTranType.receiptCostAdjustment>,
					And<INTran.pOReceiptType, IsNotNull,
					And<INTran.pOReceiptNbr, IsNotNull,
					And<INTran.pOReceiptLineNbr, IsNotNull>>>>>>>
				.SelectMultiBound(this, new[] { doc });

			foreach (PXResult<INTran, INTran2> res in lcTransWithOrig)
			{
				INTran lcTran = res;
				INTran2 origTran = res;
				if (lcTran.TranType == INTranType.ReceiptCostAdjustment && origTran?.Released != true)
				{
					string errorMessage;
					if (doc.IsPPVTran == true)
						errorMessage = AP.Messages.INReceiptMustBeReleasedBeforePPV;
					else if (doc.IsTaxAdjustmentTran == true)
						errorMessage = AP.Messages.INReceiptMustBeReleasedBeforeTaxAdjustment;
					else
						errorMessage = PO.Messages.INReceiptMustBeReleasedBeforeLCProcessing;

					throw new PXException(errorMessage, origTran?.RefNbr ?? string.Empty, lcTran.POReceiptNbr);
				}
				else if (lcTran.TranType == INTranType.Adjustment && string.IsNullOrEmpty(lcTran.ARRefNbr))
				{
					var poRctLine = POReceiptLine.PK.Find(this, lcTran.POReceiptType, lcTran.POReceiptNbr, lcTran.POReceiptLineNbr);
					if (POLineType.IsDropShip(poRctLine?.LineType))
					{
						if (string.IsNullOrEmpty(origTran?.RefNbr))
						{
							SOLineSplit linkedSOLine = SelectFrom<SOLineSplit>
								.Where<SOLineSplit.pOType.IsEqual<POReceiptLine.pOType.FromCurrent>
									.And<SOLineSplit.pONbr.IsEqual<POReceiptLine.pONbr.FromCurrent>>
									.And<SOLineSplit.pOLineNbr.IsEqual<POReceiptLine.pOLineNbr.FromCurrent>>>
								.View.ReadOnly.SelectSingleBound(this, new object[] { poRctLine });
							if (linkedSOLine != null)
							{
								throw new PXException(PO.Messages.CannotFindINIssueForDropship, poRctLine.ReceiptNbr,
									linkedSOLine.OrderNbr, linkedSOLine.OrderType);
							}
						}
						else
						{
							lcTran.ARDocType = origTran.ARDocType;
							lcTran.ARRefNbr = origTran.ARRefNbr;
							lcTran.ARLineNbr = origTran.ARLineNbr;
							intranselect.Cache.MarkUpdated(lcTran, assertError: true);
						}
					}
				}
			}
		}

		public virtual void ReleaseDocProc(JournalEntry je, INRegister doc, bool releaseFromHold = false)
        {
			doc = ActualizeAndValidateINRegister(doc, releaseFromHold);

            if (doc.DocType == INDocType.Transfer)
            {
                ValidateTransferDocIntegrity(doc);
            }

			ValidateKitAssembly(doc);

			ValidateOrigLCReceiptIsReleased(doc);

			using (TranSplitPlanExt.ReleaseModeScope())
            using (PXTransactionScope ts = new PXTransactionScope())
            {
		        SegregateBatch(je, doc.BranchID, doc.BranchBaseCuryID, doc.TranDate, doc.FinPeriodID, doc.TranDesc);

                INTran prev_tran = null;
                int? prev_linenbr = null;

                if (IsOneStepTransfer())
                {
                    foreach (PXResult<INTran, INTranSplit> res in PXSelectJoin<INTran,
                            InnerJoin<INTranSplit, On<INTranSplit.FK.Tran>>,
                        Where<INTran.docType, Equal<Required<INTran.docType>>, And<INTran.refNbr, Equal<Required<INTran.refNbr>>,
                            And<INTran.docType, Equal<INDocType.transfer>, And<INTran.invtMult, Equal<shortMinus1>>>>>,
                        OrderBy<Asc<INTran.tranType, Asc<INTran.refNbr, Asc<INTran.lineNbr>>>>>
                    .Select(this, doc.DocType, doc.RefNbr))
                    {
                        INTran tran = res;
						INTranSplit split = intransplit.Locate(res) ?? res;

                        UpdateSplitDestinationLocation(tran, split, tran.ToLocationID);

                        if (object.Equals(prev_tran, tran) == false)
                        {
							INTran newtran = CreatePositiveOneStepTransferINTran(doc, tran, split);
                            newtran = intranselect.Insert(newtran);
                            //persist now for join right part 
                            //intranselect.Cache.PersistInserted(newtran);

                            prev_tran = tran;
                            prev_linenbr = newtran.LineNbr;
                        }

                        INTranSplit newsplit = PXCache<INTranSplit>.CreateCopy(split);
                        newsplit.LineNbr = prev_linenbr;
                        newsplit.SplitLineNbr =
                            (int)PXLineNbrAttribute.NewLineNbr<INTranSplit.splitLineNbr>(intransplit.Cache, doc);
                        newsplit.InvtMult = (short)1;
                        newsplit.SiteID = tran.ToSiteID;
                        newsplit.LocationID = tran.ToLocationID;
                        newsplit.FromSiteID = split.SiteID;
                        newsplit.FromLocationID = split.LocationID;
                        newsplit.SkipCostUpdate = split.SkipCostUpdate;
                        newsplit.PlanID = null;

                        newsplit = intransplit.Insert(newsplit);
                        //persist now for join right part 
                        //intransplit.Cache.PersistInserted(newsplit);
                    }
                }

                var originalintranlist = new PXResultset<INTran, INTranSplit, InventoryItem>();
                foreach (PXResult<INTran, InventoryItem, INLocation, INLotSerClass> res in PXSelectJoin<INTran,
                    InnerJoin<InventoryItem, On<INTran.FK.InventoryItem>,
                    LeftJoin<INLocation, On<INTran.FK.Location>,
                    InnerJoin<INLotSerClass, On<InventoryItem.FK.LotSerialClass>>>>,
                        Where<INTran.docType, Equal<Required<INTran.docType>>, And<INTran.refNbr, Equal<Required<INTran.refNbr>>,
                            And<INTran.tranType, Equal<INTranType.receiptCostAdjustment>>>>,
                        OrderBy<Asc<INTran.tranType, Asc<INTran.refNbr, Asc<INTran.lineNbr>>>>>
                    .Select(this, doc.DocType, doc.RefNbr))
                {
                    InventoryItem item = (InventoryItem)res;
                    INLocation whseloc = (INLocation)res;
                    INTran tran = (INTran)res;
                    INTranSplit split = (INTranSplit)tran;

                    UpdateCrossReference(tran, split, item, whseloc);

                    originalintranlist.Add(new PXResult<INTran, INTranSplit, InventoryItem>(tran, split, item));
                }

                RegenerateInTranList(originalintranlist);

                if (intranselect.Cache.IsDirty)
                {
                    this.Persist(typeof(INTran), PXDBOperation.Insert);
                    this.Persist(typeof(INTranSplit), PXDBOperation.Insert);
                    this.Persist(typeof(INItemPlan), PXDBOperation.Insert);
                    byte[] timestamp = this.TimeStamp;
                    try
                    {
                        this.TimeStamp = PXDatabase.SelectTimeStamp();
                        intranselect.Cache.Persisted(false);
                        intransplit.Cache.Persisted(false);
                        this.Caches<INItemPlan>().Persisted(false);
                    }
                    finally
                    {
                        this.TimeStamp = timestamp;
                    }
                }

                //caching transit lines
                foreach (INTransitLine res in
                    PXSelectJoin<INTransitLine,
                        InnerJoin<INTran, On<INTran.origRefNbr, Equal<INTransitLine.transferNbr>,
                            And<INTran.origLineNbr, Equal<INTransitLine.transferLineNbr>>>>,
                        Where<INTran.docType, Equal<Current<INRegister.docType>>,
                            And<INTran.refNbr, Equal<Current<INRegister.refNbr>>>>>.SelectMultiBound(this, new object[] { doc }))
                {
                }

                foreach (PXResult<INTranSplit, INTran, INItemPlan, INItemSite, INSite> res in PXSelectJoin<INTranSplit,
                    InnerJoin<INTran,
                        On<INTranSplit.FK.Tran>,
                    InnerJoin<INItemPlan,
                        On<INTranSplit.FK.ItemPlan>,
                    LeftJoinSingleTable<INItemSite,
                        On<INItemSite.inventoryID, Equal<INTran.inventoryID>,
                        And<INItemSite.siteID, Equal<INTran.toSiteID>>>,
                    LeftJoin<INSite,
                        On<INTran.FK.ToSite>>>>>,
                    Where<INTranSplit.docType, Equal<Required<INTranSplit.docType>>,
                        And<INTranSplit.refNbr, Equal<Required<INTranSplit.refNbr>>>>>.Select(this, doc.DocType, doc.RefNbr))
                {
                    INTranSplit split = res;
                    INTran tran = res;
                    INItemPlan plan = res;
                    INPlanType plantype = INPlanType.PK.Find(this, plan.PlanType);
                    INItemSite itemsite = res ?? new INItemSite();
                    INSite site = res ?? new INSite();

					var planCache = this.Caches<INItemPlan>();
					//avoid ReadItem()
					planCache.SetStatus(plan, PXEntryStatus.Notchanged);

                    bool replanToTransit = (!IsOneStepTransfer() && plantype.DeleteOnEvent != true && plantype.ReplanOnEvent == INPlanConstants.Plan42)
                                           || (plantype.PlanType == INPlanConstants.Plan62 && doc.OrigModule == BatchModule.SO &&
                                               doc.DocType == INDocType.Transfer);
                    if (replanToTransit)
                    {
                        ReattachPlanToTransit(plan, tran, split, itemsite, site);
                    }
                    else
                    {
                        if (plantype.DeleteOnEvent == true)
                        {
							planCache.Delete(plan);
                            intransplit.Cache.MarkUpdated(split, assertError: true);
                            split = (INTranSplit)intransplit.Cache.Locate(split);
                            if (split != null) split.PlanID = null;
                        }
                        else if (string.IsNullOrEmpty(plantype.ReplanOnEvent) == false)
                        {
                            plan = PXCache<INItemPlan>.CreateCopy(plan);
                            plan.PlanType = plantype.ReplanOnEvent;
							planCache.Update(plan);
                        }
                    }
                }

                ProcessLinkedAllocation(doc);

                PXFormulaAttribute.SetAggregate<INTranCost.qty>(intrancost.Cache, typeof(SumCalc<INTran.costedQty>));
                PXFormulaAttribute.SetAggregate<INTranCost.tranCost>(intrancost.Cache, typeof(SumCalc<INTran.tranCost>));
                PXFormulaAttribute.SetAggregate<INTranCost.tranAmt>(intrancost.Cache, typeof(SumCalc<INTran.tranAmt>));

                prev_tran = null;

                var mainselect = this.GetMainSelect(doc);

                var tranDataRows = mainselect.Select(doc.DocType, doc.RefNbr).AsEnumerable();

                foreach (PXResult<INTran, InventoryItem, INSite, INPostClass, INLotSerClass, INTranSplit, ReasonCode,
                    INLocation> res in tranDataRows)
                {
                    INTran tran = (INTran)res;
                    INSite site = (INSite)res;
                    INTranSplit resSplit = (INTranSplit)res;
                    InventoryItem item = (InventoryItem)res;
                    ValidateTran(doc, tran, resSplit, item, site);

                    tran = PXCache<INTran>.CreateCopy(tran);
                    tran.TranDate = doc.TranDate;
                    tran.TranPeriodID = doc.TranPeriodID;
                    intranselect.Cache.SetDefaultExt<INTran.finPeriodID>(tran);
                    tran = intranselect.Update(tran);
                }

                ValidateFinPeriod(doc, tranDataRows.Cast<PXResult<INTran, InventoryItem, INSite>>());

                foreach (PXResult<INTran, InventoryItem, INSite, INPostClass, INLotSerClass, INTranSplit, ReasonCode, INLocation> res in tranDataRows)
                {
                    INTran tran = (INTran)res;
                    INTranSplit resSplit = (INTranSplit)res;
                    INTranSplit split = (resSplit.RefNbr != null) ? resSplit : (INTranSplit)tran;
                    InventoryItem item = (InventoryItem)res;
                    INSite site = (INSite)res;
                    ReasonCode reasoncode = (ReasonCode)res;
                    INLocation whseloc = (((INLocation)res).LocationID != null) ? (INLocation)res : INLocation.PK.Find(this, tran.LocationID);
                    INPostClass postclass = (INPostClass)res;
                    INLotSerClass lotserclass = (INLotSerClass)res;

                    PXParentAttribute.SetParent(intranselect.Cache, tran, typeof(INRegister), inregister.Current);

                    InventoryItem.PK.StoreResult(this, item);
                    INSite.PK.StoreResult(this, site);
                    ReasonCode.PK.StoreResult(this, reasoncode);
                    INLocation.PK.StoreResult(this, whseloc);
                    INPostClass.PK.StoreResult(this, postclass);
                    INLotSerClass.PK.StoreResult(this, lotserclass);

                    tran = PXCache<INTran>.CreateCopy(tran);
                    tran.Released = true;
                    tran = intranselect.Update(tran);

                    //zero quantity auto added splits will have it null
                    if (split.CreatedDateTime != null)
                    {
                        //locate split record not to erase PlanID
                        split = intransplit.Locate(split) ?? split;
                        split.TranDate = doc.TranDate;
                        split.Released = true;
                        split = intransplit.Update(split);
                    }

                    //ignore split processing for zero qty transactions
                    if (((tran.TranType == INTranType.Adjustment || tran.TranType == INTranType.NegativeCostAdjustment || tran.TranType == INTranType.StandardCostAdjustment || tran.TranType == INTranType.ReceiptCostAdjustment) || tran.Qty != 0m))
                    {
                        if (item.StkItem == true && !IsProjectDropShip(tran))
                        {
                            UpdateCrossReference(tran, split, item, whseloc);
                            UpdateItemSite(tran, item, site, reasoncode, postclass);

                            if (split.BaseQty != 0m)
                            {
                                UpdateSiteStatus(tran, split, whseloc);
                                UpdateLocationStatus(tran, split);
                                UpdateLotSerialStatus(tran, split, item, lotserclass);
                                UpdateSiteHist(tran, split);
								UpdateSiteHistByCostCenterD(tran, split);
                                UpdateSiteHistDay(tran, split);
                            }
                            UpdateItemLotSerial(tran, split, item, lotserclass);
                            UpdateSiteLotSerial(tran, split, item, lotserclass);

                            if (!(split.SkipCostUpdate ?? false))
                                UpdateCostStatus(prev_tran, tran, split, item);

                            prev_tran = tran;
                        }
                        else
                        {
                            if (tran.AssyType == INAssyType.KitTran || tran.AssyType == INAssyType.CompTran)
                            {
                                throw new PXException(Messages.NonStockKitAssemblyNotAllowed);
                            }

                            if (IsProjectDropShip(tran))
                            {
                                WriteGLNonStockCosts(je, tran, item, site);
                            }
                            else
                            {
                                UpdateItemSite(tran, item, site, reasoncode, postclass);
                                AssembleCost(tran, split, item);
                                WriteGLNonStockCosts(je, tran, item, site);
                                UpdateARTranCost(tran);
                            }
                        }
                    }
                    OnTranReleased(tran);
                }

                UpdateTransitPlans();

                if (this.insetup.Current.ReplanBackOrders == true)
                {
                    ReplanBackOrders();
                }

                PXFormulaAttribute.SetAggregate<INTranCost.qty>(intrancost.Cache, null);
                PXFormulaAttribute.SetAggregate<INTranCost.tranCost>(intrancost.Cache, null);
                PXFormulaAttribute.SetAggregate<INTranCost.tranAmt>(intrancost.Cache, null);

                if (doc.DocType.IsIn(INDocType.Issue, INDocType.Adjustment))
                {
                    PXFormulaAttribute.SetAggregate<INTran.tranCost>(intranselect.Cache, typeof(SumCalc<INRegister.totalCost>));
                    try
                    {
                        PXFormulaAttribute.CalcAggregate<INTran.tranCost>(intranselect.Cache, doc);
                    }
                    finally
                    {
                        PXFormulaAttribute.SetAggregate<INTran.tranCost>(intranselect.Cache, null);
                    }
                }

                SetOriginalQty();
                ReceiveOversold(doc);
                ReceiveQty();

                var cosplits = new Dictionary<INTranSplit, List<INTranSplit>>(new INTranSplitCostComparer());
                foreach (INTranSplit split in intransplit.Cache.Updated)
                {
                    List<INTranSplit> list;
                    if (!cosplits.TryGetValue(split, out list))
                    {
                        cosplits[split] = list = new List<INTranSplit>();
                    }
                    list.Add(split);
                }

                foreach (INTranCost costtran in intrancost.Cache.Inserted)
                {
                    if (!string.Equals(costtran.CostType, INTranCost.costType.Normal, StringComparison.OrdinalIgnoreCase))
                        continue;

                    INTran tran = (INTran)PXParentAttribute.SelectParent(intrancost.Cache, costtran, typeof(INTran));
					if (tran == null)
						throw new Common.Exceptions.RowNotFoundException(intranselect.Cache, costtran.DocType, costtran.RefNbr, costtran.LineNbr);
                    
                    var ortran = LandedCostAllocationService.Instance.GetOriginalInTran(this, tran.POReceiptType, tran.POReceiptNbr, tran.POReceiptLineNbr);

                    UpdateAdditionalCost(ortran, costtran);

                    //specific items are handled only here since they do not have oversolds
                    if (costtran.CostDocType == tran.DocType && costtran.CostRefNbr == tran.RefNbr)
                    {
                        INTranSplit upd = new INTranSplit
                        {
                            DocType = costtran.DocType,
                            RefNbr = costtran.RefNbr,
                            LineNbr = costtran.LineNbr,
                            CostSiteID = costtran.CostSiteID,
                            CostSubItemID = costtran.CostSubItemID,
                            ValMethod = costtran.LotSerialNbr != null ? INValMethod.Specific : INValMethod.Average,
                            LotSerialNbr = costtran.LotSerialNbr
                        };

                        List<INTranSplit> list;
                        if (cosplits.TryGetValue(upd, out list))
                        {
                            foreach (INTranSplit split in list)
                            {
                                split.TotalQty += costtran.Qty;
                                split.TotalCost += costtran.TranCost;
                            }
                        }
                    }
                    else
                    {
                        INTranSplitUpdate upd = new INTranSplitUpdate
                        {
                            DocType = costtran.DocType,
                            RefNbr = costtran.RefNbr,
                            LineNbr = costtran.LineNbr,
                            CostSiteID = costtran.CostSiteID,
                            CostSubItemID = costtran.CostSubItemID,
                        };

                        upd = intransplitupdate.Insert(upd);
                        upd.TotalQty += costtran.Qty;
                        upd.TotalCost += costtran.TotalCost;
                    }
                }

                foreach (INTranCost costtran in intrancost.Cache.Inserted)
                {
					if (!string.Equals(costtran.CostType, INTranCost.costType.Normal, StringComparison.OrdinalIgnoreCase))
						continue;

					INTran tran = (INTran)PXParentAttribute.SelectParent(intrancost.Cache, costtran, typeof(INTran));
					if (tran == null)
						throw new Common.Exceptions.RowNotFoundException(intranselect.Cache, costtran.DocType, costtran.RefNbr, costtran.LineNbr);

					if (!(costtran.CostDocType == tran.DocType && costtran.CostRefNbr == tran.RefNbr))
                    {
                        INTranUpdate upd = new INTranUpdate
                        {
                            DocType = tran.DocType,
                            RefNbr = costtran.RefNbr,
                            LineNbr = costtran.LineNbr
                        };

                        upd = intranupdate.Insert(upd);
                        upd.TranCost += costtran.TotalCost;
                    }
                }

                prev_tran = null;

                var piController = new Lazy<INPIController>(() => PXGraph.CreateInstance<INPIController>());
                foreach (INTranCost costtran in intrancost.Cache.Inserted)
                {
					if (string.Equals(costtran.CostType, INTranCost.costType.TransitTransfer, StringComparison.OrdinalIgnoreCase))
						continue;

					INTran tran = (INTran)PXParentAttribute.SelectParent(intrancost.Cache, costtran, typeof(INTran));
					if (tran == null)
						throw new Common.Exceptions.RowNotFoundException(intranselect.Cache, costtran.DocType, costtran.RefNbr, costtran.LineNbr);

					InventoryItem item = InventoryItem.PK.Find(this, costtran.InventoryID);
                    INPostClass postClass = INPostClass.PK.Find(this, item?.PostClassID) ?? new INPostClass();
                    INSite site = INSite.PK.Find(this, tran.SiteID);
                    ReasonCode reasoncode = ReasonCode.PK.Find(this, tran.ReasonCode);
                    INLocation location = INLocation.PK.Find(this, tran.LocationID);

                    UpdateCostHist(costtran, tran);
                    UpdateSalesHist(costtran, tran);
                    UpdateCustSalesHist(costtran, tran);
					UpdateSiteHistByCostCenterDCost(costtran, tran);

                    if (object.Equals(prev_tran, tran) == false)
                    {
                        if (tran.DocType == costtran.CostDocType && tran.RefNbr == costtran.CostRefNbr)
                        {
                            UpdateSalesHistD(tran);
                            UpdateCustSalesStats(tran);
                            WriteGLSales(je, tran);
                        }

                        if ((tran.InvtMult == (short)-1 || tran.InvtMult == (short)1 && tran.OrigLineNbr != null) && tran.Qty != 0m &&
                            Math.Abs((decimal)tran.TranCost - PXCurrencyAttribute.BaseRound(this, (decimal)tran.Qty * (decimal)tran.UnitCost)) > 0.00005m)
                        {
                            tran.UnitCost = PXDBPriceCostAttribute.Round((decimal)tran.TranCost / (decimal)tran.Qty);
                            tran.OverrideUnitCost = true;
                        }
                    }

                    UpdateARTranCost(tran, costtran.TranCost);

                    UpdatePOReceiptLineCost(tran, costtran, item);

					UpdateItemStatsLastPurchaseDate(tran);

					if (doc.DocType == INDocType.Adjustment && !string.IsNullOrEmpty(doc.PIID) && tran.PILineNbr != null)
                    {
                        piController.Value.AccumulateFinalCost(doc.PIID, (int)tran.PILineNbr, costtran.InvtMult * costtran.TranCost ?? 0m);
                    }

                    if (tran.SOShipmentNbr != null && tran.InvtMult != (short)0)
                    {
                        if (tran.DocType == costtran.CostDocType && tran.RefNbr == costtran.CostRefNbr)
                        {
                            SOShipLineUpdate shipline = new SOShipLineUpdate();
                            shipline.ShipmentType = tran.SOShipmentType;
                            shipline.ShipmentNbr = tran.SOShipmentNbr;
                            shipline.LineNbr = tran.SOShipmentLineNbr;

                            shipline = this.soshiplineupdate.Insert(shipline);

                            shipline.ExtCost += costtran.TranCost;
                            shipline.UnitCost = PXDBPriceCostAttribute.Round((decimal)(shipline.ExtCost / (tran.Qty != 0m ? tran.Qty : null) ?? 0m));
                        }
                    }

                    prev_tran = tran;

                    WriteGLCosts(je, costtran, tran, item, site, postClass, reasoncode, location);
                }

                if (doc.DocType == INDocType.Adjustment && !string.IsNullOrEmpty(doc.PIID))
                {
                    piController.Value.ReleasePI(doc.PIID);
                }

                foreach (ItemCostHist hist in itemcosthist.Cache.Inserted)
                {
                    INSite insite = INSite.PK.Find(this, hist.CostSiteID);

                    if (insite != null)
                    {
                        ItemStats stats = new ItemStats();
                        stats.InventoryID = hist.InventoryID;
                        stats.SiteID = hist.CostSiteID;

                        stats = itemstats.Insert(stats);

                        stats.QtyOnHand += hist.FinYtdQty;
                        stats.TotalCost += hist.FinYtdCost;
                        stats.QtyReceived += (hist.FinPtdQtyReceived > 0 ? hist.FinPtdQtyReceived : 0) + hist.FinPtdQtyTransferIn + hist.FinPtdQtyAssemblyIn;
                        stats.CostReceived += (hist.FinPtdCostReceived > 0 ? hist.FinPtdCostReceived : 0) + hist.FinPtdCostTransferIn + hist.FinPtdCostAssemblyIn;
                    }
                }

                    var branch = Branch.PK.Find(this, doc.BranchID);
                    if (branch == null)
                        throw new Common.Exceptions.RowNotFoundException(Caches[typeof(Branch)], doc.BranchID);

                foreach (ItemStats stats in itemstats.Cache.Cached)
                {
                    if (itemstats.Cache.GetStatus(stats) != PXEntryStatus.Notchanged)
                    {
                        if ((stats.QtyReceived ?? 0) != 0m && (stats.CostReceived ?? 0) != 0m)
                        {
                            stats.LastCost = PXDBPriceCostAttribute.Round((decimal)(stats.CostReceived / stats.QtyReceived));
                            stats.LastCostDate = GetLastCostTime(itemstats.Cache);
                        }
                        else
                            stats.LastCost = 0m;

                        stats.MaxCost = stats.LastCost;
                        stats.MinCost = stats.LastCost;

                            UpdateItemCost(stats, branch.BaseCuryID);
                    }
                }

                if (UpdateGL && (je.GLTranModuleBatNbr.Cache.IsInsertedUpdatedDeleted || doc.SiteID != doc.ToSiteID || doc.DocType != INDocType.Transfer))
                {
                    je.Save.Press();
                    doc.BatchNbr = je.BatchModule.Current.BatchNbr;
                }

                doc = MarkDocumentReleased(doc);

                this.Actions.PressSave();
                ts.Complete();
            }
        }

		protected virtual INRegister ActualizeAndValidateINRegister(INRegister doc, bool releaseFromHold)
		{
			//planning requires document context
			inregister.Current = inregister.Search<INRegister.docType, INRegister.refNbr>(doc.DocType, doc.RefNbr);
			if (releaseFromHold)
			{
				inregister.Current.Hold = false;
			}
			else
			{
				if (inregister.Current.Hold == true)
				{
					throw new PXException(Messages.Document_OnHold_CannotRelease);
				}
			}
			if (inregister.Current.Released == true)
			{
				throw new PXException(Messages.Document_Status_Invalid);
			}
			
			inregister.Current.SrcDocType = doc.SrcDocType;
			inregister.Current.SrcRefNbr = doc.SrcRefNbr;

			//mark as updated so that doc will not expire from cache, and totalcost will not be overwritten with old value
			inregister.Cache.MarkUpdated(inregister.Current, true);
			return inregister.Current;
		}

		protected virtual INTran CreatePositiveOneStepTransferINTran(INRegister doc, INTran tran, INTranSplit split)
		{
			INTran newtran = PXCache<INTran>.CreateCopy(tran);

			newtran.OrigDocType = newtran.DocType;
			newtran.OrigTranType = newtran.TranType;
			newtran.OrigRefNbr = newtran.RefNbr;
			newtran.OrigLineNbr = newtran.LineNbr;
			if (tran.TranType == INTranType.Transfer)
			{
				newtran.OrigNoteID = doc.NoteID;
				newtran.OrigToLocationID = tran.ToLocationID;
				newtran.OrigIsLotSerial = !(string.IsNullOrEmpty(split.LotSerialNbr));
			}
			INSite toSite = INSite.PK.Find(this, tran.ToSiteID);
			newtran.BranchID = toSite.BranchID;
			newtran.LineNbr = (int)PXLineNbrAttribute.NewLineNbr<INTran.lineNbr>(intranselect.Cache, doc);
			newtran.InvtMult = (short)1;
			newtran.SiteID = newtran.ToSiteID;
			newtran.LocationID = newtran.ToLocationID;
			newtran.ProjectID = newtran.ToProjectID;
			newtran.TaskID = newtran.ToTaskID;
			newtran.CostCodeID = newtran.ToCostCodeID;
			newtran.CostLayerType = newtran.ToCostLayerType;
			newtran.CostCenterID = newtran.ToCostCenterID;
			newtran.DestBranchID = null;
			newtran.ToSiteID = null;
			newtran.ToLocationID = null;
			newtran.ToProjectID = null;
			newtran.ToTaskID = null;
			newtran.ToCostCodeID = null;
			newtran.ToCostLayerType = null;
			newtran.ToCostCenterID = null;
			newtran.InvtAcctID = null;
			newtran.InvtSubID = null;
			newtran.ARDocType = null;
			newtran.ARRefNbr = null;
			newtran.ARLineNbr = null;
			newtran.NoteID = null;
			return newtran;
		}

		public static DateTime GetLastCostTime(PXCache itemstatsCache)
        {
            // The PXDBLastChangeDateTimeAttribute sets new value only if lastCost doesn't equal previous value,
            // otherwise value from the property will be persisted. We should use GetDate of PXDBLastChangeDateTimeAttribute
            // to get correct value because PXDBLastChangeDateTimeAttribute uses time zones.
            return itemstatsCache.GetAttributesReadonly<INItemStats.lastCostDate>()
                    .OfType<PXDBLastChangeDateTimeAttribute>().FirstOrDefault()?.GetDate() ?? DateTime.Now;
        }

        protected virtual void UpdateItemCost(INItemStats stats, string baseCuryID)
		{
            if (stats == null)
                throw new PXArgumentException(nameof(stats), ErrorMessages.FieldIsEmpty, nameof(stats));

            if (baseCuryID == null)
                throw new PXArgumentException(nameof(baseCuryID), ErrorMessages.FieldIsEmpty, nameof(baseCuryID));

            if (stats.LastCost == null)
                throw new Common.Exceptions.FieldIsEmptyException(itemstats.Cache, stats, typeof(ItemStats.lastCost));

            if (stats.SiteID == null)
                throw new Common.Exceptions.FieldIsEmptyException(itemstats.Cache, stats, typeof(ItemStats.siteID));


            var cost = new ItemCost() { InventoryID = stats.InventoryID, CuryID = baseCuryID };
            cost = itemcost.Insert(cost);
            cost.QtyOnHand += stats.QtyOnHand;
            cost.TotalCost += stats.TotalCost;
            
            cost.MaxCost = Math.Max((decimal)stats.LastCost, cost.MaxCost ?? 0m);

            if (cost.MinCost.IsIn(null, 0m) || (stats.LastCost != 0m && stats.LastCost < cost.MinCost))
            {
                cost.MinCost = stats.LastCost;
            }

            if (cost.LastCostDate.IsIn(null, INItemStats.MinDate.get()) ||
                (stats.LastCostDate != INItemStats.MinDate.get() && cost.LastCostSiteID < stats.SiteID))
            {
                cost.LastCostSiteID = stats.SiteID;
                cost.LastCost = stats.LastCost;
                cost.LastCostDate = stats.LastCostDate;
            }
        }

        protected virtual INRegister MarkDocumentReleased(INRegister doc)
		{
			var original = PXCache<INRegister>.CreateCopy(doc);
			INRegister.Events.Select(e => e.DocumentReleased).FireOn(this, doc);
			if (inregister.Cache.GetDifference(original, doc).Any() == false)
			{
				// no event handler took place - change status manually
				doc.Status = INDocStatus.Released;
				doc.Released = true;
				doc.ReleasedToVerify = false;
				doc = inregister.Update(doc);
			}
			return doc;
		}

		public virtual PXSelectBase<INTran> GetMainSelect(INRegister doc)
		{
			var mainSelect = new PXSelectJoin<INTran,
				InnerJoin<InventoryItem, On<INTran.FK.InventoryItem>,
				InnerJoin<INSite, On<INTran.FK.Site>,
				LeftJoin<INPostClass, On<InventoryItem.FK.PostClass>,
				LeftJoin<INLotSerClass, On<InventoryItem.FK.LotSerialClass>,
				LeftJoin<INTranSplit, On<INTranSplit.FK.Tran>,
				LeftJoin<ReasonCode, On<INTran.FK.ReasonCode>,
				LeftJoin<INLocation, On<INTranSplit.FK.Location>>>>>>>>,
				Where<INTran.docType, Equal<Required<INTran.docType>>, And<INTran.refNbr, Equal<Required<INTran.refNbr>>>>>(this);

			this.OverrideMainSelectOrderBy(doc, mainSelect);

			return mainSelect;
		}
               
        public virtual void OverrideMainSelectOrderBy(INRegister doc, PXSelectBase<INTran> mainSelect)
		{
			switch (doc.DocType)
			{
				case INDocType.Issue:
				//For transfer its required to set up all the acceptor costsites and related info first
				case INDocType.Transfer:
					mainSelect.OrderByNew<OrderBy<Asc<INTran.docType, Asc<INTran.refNbr, Desc<INTran.invtMult, Asc<INTran.lineNbr>>>>>>();
					break;
				//for adjustments it's required to update all the acceptor cost layers first
				case INDocType.Adjustment:
					mainSelect.OrderByNew<OrderBy<Asc<INTran.docType, Asc<INTran.refNbr, Desc<INTranSplit.invtMult, Desc<INTran.tranCost, Asc<INTran.lineNbr>>>>>>>();
					break;
				//meanwhile ascending order is required for assembly
				default:
					mainSelect.OrderByNew<OrderBy<Asc<INTran.docType, Asc<INTran.refNbr, Asc<INTran.invtMult, Asc<INTran.lineNbr>>>>>>();
					break;
			}
		}

        protected virtual void ProcessLinkedAllocation(INRegister doc)
        {
			if (doc.OrigModule.IsIn<string>(BatchModule.IN, INRegister.origModule.PI))
			{
				return;
			}

            //replan fixed SO Demand
            //Multiple PO Receipts are never consolidated into single IN Receipt
            string POReceiptType = null;
            string POReceiptNbr = null;
            var planlist = new List<PXResult<INItemPlan, INTranSplit, INTran, INPlanType, INItemPlanDemand>>();

            foreach (PXResult<INItemPlan, INTranSplit, INTran, INItemPlanDemand> res in
                PXSelectJoin<INItemPlan,
                InnerJoin<INTranSplit, On<INTranSplit.planID, Equal<INItemPlan.supplyPlanID>>,
                InnerJoin<INTran, On<INTranSplit.FK.Tran>,
                LeftJoin<INItemPlanDemand, On<INItemPlan.FK.DemandItemPlan>>>>,
            Where<INTranSplit.docType, Equal<Required<INRegister.docType>>,
              And<INTranSplit.refNbr, Equal<Required<INRegister.refNbr>>>>>
                .Select(this, doc.DocType, doc.RefNbr))
            {
                INTran tran = res;
                INTranSplit split = res;
				INItemPlan plan = res;
                var plantype = INPlanType.PK.Find(this, plan.PlanType);

	            INLocation location = INLocation.PK.Find(this, split.LocationID);

                if (location != null && location.InclQtyAvail != true && plantype.ReplanOnEvent == INPlanConstants.Plan61)
                {
                    plantype = PXCache<INPlanType>.CreateCopy(plantype);
                    plantype.ReplanOnEvent = INPlanConstants.Plan60;
                }

                planlist.Add(new PXResult<INItemPlan, INTranSplit, INTran, INPlanType, INItemPlanDemand>(plan, res, res, plantype, res));

                if (string.IsNullOrEmpty(POReceiptNbr))
                {
                    POReceiptType = tran.POReceiptType;
                    POReceiptNbr = tran.POReceiptNbr;
                }
            }

            ProcessLinkedAllocation(planlist, POReceiptType, POReceiptNbr);
        }

        protected virtual void ProcessLinkedAllocation(List<PXResult<INItemPlan, INTranSplit, INTran, INPlanType, INItemPlanDemand>> list, string poReceiptType, string poReceiptNbr)
        {
			var planlist = list?.ConvertAll(x => new PXResult<INItemPlan, INPlanType>(x, x));

			if (onBeforeSalesOrderProcessPOReceipt != null)
			{
				planlist = onBeforeSalesOrderProcessPOReceipt(this, planlist, poReceiptType, poReceiptNbr);
			}

			SOOrderEntry.ProcessPOReceipt(this, planlist, poReceiptType, poReceiptNbr);
        }

		private void ReattachPlanToTransit(INItemPlan plan, INTran tran, INTranSplit split, INItemSite itemsite, INSite site)
		{
			var planCache = this.Caches<INItemPlan>();
			planCache.Delete(plan);
			INTransitLine transitline = GetTransitLine(tran);
			var transitPlan = PXCache<INItemPlan>.CreateCopy(plan);
			transitPlan.PlanType = INPlanConstants.Plan42;
			transitPlan.PlanID = null;
			transitPlan.SiteID = tran.ToSiteID;
			transitPlan.RefNoteID = transitline.NoteID;
			transitPlan.RefEntityType = typeof(INTransitLine).FullName;
			transitPlan.LocationID = tran.ToLocationID ?? itemsite.DfltReceiptLocationID ?? site.ReceiptLocationID;
			transitPlan.CostCenterID = tran.ToCostCenterID;
			transitPlan.DemandPlanID = null;
			transitPlan = (INItemPlan)planCache.Insert(transitPlan);

			bool isFixedInTransit = transitline.IsFixedInTransit == true;
			foreach (INItemPlan demandPlan in PXSelect<INItemPlan,
				Where<INItemPlan.supplyPlanID, Equal<Required<INItemPlan.supplyPlanID>>>>
				.Select(this, plan.PlanID))
			{
				bool alreadyUpdated = planCache.GetStatus(demandPlan) == PXEntryStatus.Updated;
				string origPlanType = alreadyUpdated ? INItemPlan.PK.Find(this, demandPlan).PlanType : demandPlan.PlanType;
				INPlanType demandPlantype = INPlanType.PK.Find(this, origPlanType);

				isFixedInTransit |= demandPlantype.ReplanOnEvent == INPlanConstants.Plan95 && demandPlantype.PlanType == INPlanConstants.Plan93;

				if (!alreadyUpdated)
				{
					demandPlan.SupplyPlanID = transitPlan.PlanID;
					if (demandPlantype.ReplanOnEvent == INPlanConstants.Plan95)
					{
						demandPlan.PlanType = demandPlantype.ReplanOnEvent;
						planCache.Update(demandPlan);
					}
					else
					{
						planCache.MarkUpdated(demandPlan, assertError: true);
					}
				}
			}

			if (isFixedInTransit)
			{
				//Fixed Transfer for SO
				transitPlan.PlanType = INPlanConstants.Plan44;
				planCache.Update(transitPlan);

				//Should be refactored and removed - isfixedintransit should be moved to intransitline
				split.IsFixedInTransit = true;
				transitline.IsFixedInTransit = true;
				intransitline.Update(transitline);
			}

			split.PlanID = null;
			split.ToSiteID = tran.ToSiteID;
			UpdateSplitDestinationLocation(tran, split, tran.ToLocationID ?? itemsite.DfltReceiptLocationID ?? site.ReceiptLocationID);

			intransplit.Cache.MarkUpdated(split, assertError: true);
		}

		private void UpdateAdditionalCost(INTran ortran, INTranCost trancost)
        {
            if (ortran == null || (trancost.TranCost ?? 0m) == 0m)
                return;

            if (!(trancost.CostDocType == INDocType.Adjustment && trancost.Qty == 0m && trancost.InvtMult == 1))
                return;
            INTranSplitAdjustmentUpdate upd = new INTranSplitAdjustmentUpdate
            {
                DocType = ortran.DocType,
                RefNbr = ortran.RefNbr,
                LineNbr = ortran.LineNbr,
                CostSiteID = trancost.CostSiteID,
                LotSerialNbr = trancost.LotSerialNbr,
                CostSubItemID = trancost.CostSubItemID
            };

            upd = intransplitadjustmentupdate.Insert(upd);
            upd.AdditionalCost += trancost.TranCost;
        }

        public virtual INTran Copy(INTran tran, ReadOnlyCostStatus layer, InventoryItem item)
        {
            INTran newtran = new INTran();
            
            newtran.BranchID = tran.BranchID;
            newtran.DocType = tran.DocType;
            newtran.RefNbr = tran.RefNbr;
            newtran.TranType = INTranType.Adjustment;
			newtran.IsStockItem = tran.IsStockItem;
            newtran.InventoryID = tran.InventoryID;
            newtran.SubItemID = tran.SubItemID;
            newtran.SiteID = tran.SiteID;
            newtran.LocationID = tran.LocationID;
            newtran.UOM = tran.UOM;
            newtran.Qty = 0m;
            newtran.AcctID = tran.AcctID;
            newtran.SubID = tran.SubID;
            newtran.COGSAcctID = tran.COGSAcctID;
            newtran.COGSSubID = tran.COGSSubID;
            if (layer != null)
            {
                newtran.InvtAcctID = layer.AccountID;
                newtran.InvtSubID = layer.SubID;
                newtran.OrigRefNbr = (item.ValMethod == INValMethod.FIFO || item.ValMethod == INValMethod.Specific) ? layer.ReceiptNbr : null;
                newtran.LotSerialNbr = (item.ValMethod == INValMethod.Specific) ? layer.LotSerialNbr : string.Empty;
            }
            else
            {
                newtran.InvtAcctID = null;
                newtran.InvtSubID = null;
                newtran.OrigRefNbr = null;
                newtran.LotSerialNbr = String.Empty;
            }
			newtran.POReceiptType = tran.POReceiptType;
            newtran.POReceiptNbr = tran.POReceiptNbr;
            newtran.POReceiptLineNbr = tran.POReceiptLineNbr;
			newtran.POLineType = tran.POLineType;
            newtran.ProjectID = tran.ProjectID;
            newtran.TaskID = tran.TaskID;
            newtran.CostCodeID = tran.CostCodeID;
			newtran.CostCenterID = tran.CostCenterID;
            newtran.ReasonCode = tran.ReasonCode;
			newtran.UnitCost = tran.UnitCost;
			newtran.CostLayerType = tran.CostLayerType;
			newtran.ToCostLayerType = tran.ToCostLayerType;
			return newtran;
        }

        public virtual void RegenerateInTranList(PXResultset<INTran, INTranSplit, InventoryItem> originalintranlist)
        {
            foreach (PXResult<INTran, INTranSplit, InventoryItem> res in originalintranlist)
            {
                INTran tran = res;
                INTranSplit split = res;
                InventoryItem item = res;

                decimal? accu_TranCost = 0m;
                decimal? accu_Qty = 0m;
                decimal? entiretrancost = tran.TranCost;

                ReadOnlyCostStatus prev_layer = null;

                INTran ortran = LandedCostAllocationService.Instance.GetOriginalInTran(this, tran.POReceiptType, tran.POReceiptNbr, tran.POReceiptLineNbr);

                if (ortran == null) continue;

                PXView costreceiptstatusview = GetReceiptStatusView(item);
				bool fifobreak = false; // used in case there is some old pre-valuation method change receipt statuses left for fifo item

                //there is no need of foreach anymore. It's one-to-one relation, left here for legacy.
                foreach (PXResult<ReadOnlyCostStatus, ReceiptStatus> layerres in
                            costreceiptstatusview.SelectMultiBound(new object[] { tran, split }, new object[] { ortran.InvtAcctID, ortran.InvtSubID }))
                {
                    ReadOnlyCostStatus layer = (ReadOnlyCostStatus)layerres;
                    ReceiptStatus receipt = (ReceiptStatus)layerres;

					if (fifobreak)
						break;

					if (item.ValMethod == INValMethod.FIFO && receipt != null && receipt.ReceiptNbr != null)
						fifobreak = true;

                    decimal? origqty = null;
                    decimal? qtyonhand = null;

                    switch (item.ValMethod)
                    {
                        case INValMethod.Average:
							if (layer.QtyOnHand == 0m && receipt.QtyOnHand != 0m)
							{
								origqty = layer.OrigQty;
								qtyonhand = layer.QtyOnHand;
							}
							else
							{
								origqty = receipt.OrigQty;
								qtyonhand = receipt.QtyOnHand;
							}
							break;
                        case INValMethod.Specific:
                            origqty = receipt.OrigQty;
                            qtyonhand = receipt.QtyOnHand;
                            break;
                        case INValMethod.FIFO:
                            origqty = layer.OrigQty;
                            qtyonhand = layer.QtyOnHand;
                            break;
                    }

                    if (qtyonhand > 0m)
                    {
                        prev_layer = layer;
                    }

					//PPV adjustment (Tax Adjustment) goes to expense in case resulting cost is negative
					bool isPPVTranOrTaxAdjustmentTran = inregister.Current != null && (inregister.Current.IsPPVTran == true || inregister.Current.IsTaxAdjustmentTran == true);
					if (isPPVTranOrTaxAdjustmentTran && entiretrancost < 0m &&
                        (layer.TotalCost + (qtyonhand == 0 ? 0 : (qtyonhand * entiretrancost / origqty))) < 0m)
                        break;

                    //inventory adjustment
                    if (qtyonhand != 0m)
                    {
                        INTran newtran = Copy(tran, layer, item);

                        newtran.InvtMult = (short)1;

                        decimal? newtranqty;
                        if (origqty != 0m)
                            newtranqty = qtyonhand < origqty ? qtyonhand : origqty;
                        else
                            newtranqty = qtyonhand;

                        newtran.TranCost = PXDBCurrencyAttribute.BaseRound(this, (decimal)(newtranqty * entiretrancost / origqty));

                        if (item.ValMethod == INValMethod.Specific)
                        {
                            newtran.TranCost += PXCurrencyAttribute.BaseRound(this, (accu_Qty + newtranqty) * entiretrancost / origqty - accu_TranCost - newtran.TranCost);
                        }
                        accu_TranCost += newtran.TranCost;
                        accu_Qty += newtranqty;

                        intranselect.Insert(newtran);
                    }

                    //cogs adjustment
                    if (qtyonhand < origqty && origqty != 0m)
                    {
                        INTran newtran = Copy(tran, layer, item);

                        newtran.InvtMult = (short)0;
                        decimal? newtranqty = origqty - qtyonhand;
                        newtran.TranCost = PXDBCurrencyAttribute.BaseRound(this, (decimal)(newtranqty * entiretrancost / origqty));

                        if (item.ValMethod == INValMethod.Specific)
                        {
                            newtran.TranCost += PXCurrencyAttribute.BaseRound(this, (accu_Qty + newtranqty) * entiretrancost / origqty - accu_TranCost - newtran.TranCost);
                        }
                        accu_TranCost += newtran.TranCost;
                        accu_Qty += newtranqty;

                        intranselect.Insert(newtran);
                    }
                }

                //Standard, Specific rounding and corrections for average-costed items with non-zero INReceiptStatus and zero INCostStatus
                if (entiretrancost - accu_TranCost != 0m)
                {
                    INTran newtran = Copy(tran, prev_layer, item);

                    newtran.InvtMult = (short)0;
                    newtran.TranCost = PXDBCurrencyAttribute.BaseRound(this, (decimal)(entiretrancost - accu_TranCost));

                    intranselect.Insert(newtran);
                }
                intranselect.Cache.SetStatus(tran, PXEntryStatus.Deleted);
            }
        }



        private void SetOriginalQty(INCostStatus layer)
        {
            if (layer.LayerType == INLayerType.Normal && layer.QtyOnHand > 0m)
            {
				layer.OrigQtyOnHand = layer.QtyOnHand;
				if (layer.ValMethod != INValMethod.Average)
				{
                layer.OrigQty = layer.QtyOnHand;
            }
        }
        }

        private void SetOriginalQty()
        {
            foreach (AverageCostStatus layer in averagecoststatus.Cache.Inserted)
                SetOriginalQty(layer);
            foreach (SpecificCostStatus layer in specificcoststatus.Cache.Inserted)
                SetOriginalQty(layer);
            foreach (SpecificTransitCostStatus layer in specifictransitcoststatus.Cache.Inserted)
                SetOriginalQty(layer);
            foreach (FIFOCostStatus layer in fifocoststatus.Cache.Inserted)
                SetOriginalQty(layer);
        }

        public virtual void ReplanBackOrders()
        {
            ReplanBackOrders(this, false);
        }

        public static void ReplanBackOrders(PXGraph graph)
        {
            ReplanBackOrders(graph, true);
        }

        public static void ReplanBackOrders(PXGraph graph, bool ForceReplan)
        {
            List<INItemPlan> replan = new List<INItemPlan>();
            foreach (SiteStatusByCostCenter layer in graph.Caches[typeof(SiteStatusByCostCenter)].Inserted)
            {
                //for inventory release the difference between QtyOnHand and QtyAvail is contributed by the locations excluded from Available Qty.
                //thus need to check both values
                if (layer.QtyOnHand <= 0m && layer.QtyAvail <= 0m && !ForceReplan)
                    continue;
				SiteStatusByCostCenter dbLayer = SiteStatusByCostCenter.PK.Find(graph, layer.InventoryID, layer.SubItemID, layer.SiteID, layer.CostCenterID);

                decimal qtyAvail = layer.QtyHardAvail.Value + ((dbLayer != null) ? dbLayer.QtyHardAvail.Value : 0m);

                if (qtyAvail > 0m)
                {
					foreach (PXResult<INItemPlan, SOOrder, SOOrderType, SOLineSplit> res in PXSelectJoin<INItemPlan,
                        InnerJoin<SOOrder, On<SOOrder.noteID, Equal<INItemPlan.refNoteID>>,
						InnerJoin<SOOrderType, On<SOOrder.FK.OrderType>,
						InnerJoin<SOLineSplit, On<SOLineSplit.planID, Equal<INItemPlan.planID>>>>>,
                        Where<INItemPlan.inventoryID, Equal<Required<INItemPlan.inventoryID>>,
                            And<INItemPlan.subItemID, Equal<Required<INItemPlan.subItemID>>,
                            And<INItemPlan.siteID, Equal<Required<INItemPlan.siteID>>,
							And<INItemPlan.costCenterID, Equal<Required<INItemPlan.costCenterID>>,
                            And<SOOrderType.requireAllocation, Equal<False>,
							And<INItemPlan.planType, In3<INPlanConstants.plan60, INPlanConstants.plan68>>>>>>>,
                        OrderBy<Asc<INItemPlan.planDate, Asc<INItemPlan.planType, Desc<INItemPlan.planQty>>>>>
						.Select(graph, layer.InventoryID, layer.SubItemID, layer.SiteID, layer.CostCenterID))
                    {
						INItemPlan plan = res;
						SOLineSplit split = res;
						decimal plannedQty = split.BaseQty.Value - split.BaseShippedQty.Value;

                        if (plannedQty <= qtyAvail)
                        {
                            qtyAvail -= plannedQty;
                            if (plan.PlanType == INPlanConstants.Plan68)
                                replan.Add(plan);
                        }
                        else if (qtyAvail > 0)
                        {
                            if (plan.PlanType == INPlanConstants.Plan68)
                            {
                                SOLine soLine = PXSelectJoin<SOLine,
                                InnerJoin<SOLineSplit, On<SOLine.orderType, Equal<SOLineSplit.orderType>, And<SOLine.orderNbr, Equal<SOLineSplit.orderNbr>, And<SOLine.lineNbr, Equal<SOLineSplit.lineNbr>>>>>,
                                Where<SOLineSplit.planID, Equal<Required<SOLineSplit.planID>>>>.Select(graph, plan.PlanID);

                                if (soLine != null && soLine.ShipComplete != SOShipComplete.ShipComplete)
                                {
                                    replan.Add(plan);
                                    qtyAvail = 0m;
                                }
                            }
                            else
                            {
                                qtyAvail = 0m;
                            }
                        }

                        if (qtyAvail <= 0m)
                            break;
                    }
                }
            }
            PXCache plancache = graph.Caches[typeof(INItemPlan)];
            foreach (INItemPlan plan in replan)
            {
                INItemPlan upd = PXCache<INItemPlan>.CreateCopy(plan);
                upd.PlanType = INPlanConstants.Plan60;
                plancache.Update(upd);
            }
        }

		public virtual void ValidateTran(INRegister doc, INTran tran, INTranSplit split, InventoryItem item, INSite site)
		{
			if (site.Active != true)
				throw new PXException(Messages.InactiveWarehouse, site.SiteCD);

			var splitItem = InventoryItem.PK.Find(this, split.InventoryID);

			if(splitItem?.StkItem == true && item.StkItem == true && item.InventoryID != split.InventoryID)
			{
				throw new PXException(Messages.LineStockItemIsDifferFromSplitStockItem, item.InventoryCD);
			}

			ValidateTran(tran);

			if (doc.DocType == INDocType.Transfer && tran.InvtMult == -1
				&& (doc.SiteID != tran.SiteID || doc.ToSiteID != tran.ToSiteID
					|| split.RefNbr != null && (doc.SiteID != split.SiteID || doc.ToSiteID != split.ToSiteID)))
			{
				// AC-139602. May occur during excel import.
				throw new PXException(Messages.TransferDocumentIsCorrupted, tran.LineNbr);
			}

			if (split.RefNbr == null && tran.DocType != INDocType.Adjustment && tran.InvtMult != 0 && tran.Qty != 0m && item.StkItem == true)
			{
				throw new PXException(Messages.CannotReleaseAllocationsMissing,
					intranselect.Cache.GetValueExt<INTran.inventoryID>(tran),
					intranselect.Cache.GetValueExt<INTran.siteID>(tran),
					intranselect.Cache.GetValueExt<INTran.lineNbr>(tran));
			}

			ValidateBaseQty(doc, tran, split);
		}

        public virtual void ValidateTran(INTran tran)
        {
            if (tran.UnassignedQty != 0)
            {
                RaiseUnassignedQtyNotZeroException(tran);
            }
			ValidateMultiplier(tran);

			ConvertedInventoryItemAttribute.ValidateRow(intranselect.Cache, tran);
		}

        public virtual void RaiseUnassignedQtyNotZeroException(INTran tran)
        {
            InventoryItem item = InventoryItem.PK.Find(this, tran.InventoryID);

            throw new PXException(Messages.BinLotSerialNotAssignedWithItemCode, item?.InventoryCD);
        }

		protected virtual void ValidateMultiplier(INTran tran)
		{
			switch(tran.TranType)
			{
				case INTranType.Assembly:
				case INTranType.Disassembly:
				case INTranType.Transfer:
					if (tran.InvtMult.IsNotIn<short?>(-1, 1))
						throw new PXException(Messages.MultiplierEqualsTo2, -1, 1);
					break;
				case INTranType.Adjustment:
				case INTranType.CreditMemo:
				case INTranType.Return:
				case INTranType.Receipt:
					if (tran.InvtMult.IsNotIn<short?>(0, 1))
						throw new PXException(Messages.MultiplierEqualsTo2, 0, 1);
					break;				
				case INTranType.StandardCostAdjustment:
				case INTranType.NegativeCostAdjustment:
					if (tran.InvtMult != 1)
						throw new PXException(Messages.MultiplierEqualsTo, 1);
					break;
				case INTranType.Issue:
				case INTranType.Invoice:
				case INTranType.DebitMemo:
					if (tran.InvtMult.IsNotIn<short?>(-1, 0))
						throw new PXException(Messages.MultiplierEqualsTo2, -1, 0);
					break;
				case INTranType.ReceiptCostAdjustment:
				case INTranType.NoUpdate:
					if (tran.InvtMult != 0)
						throw new PXException(Messages.MultiplierEqualsTo, 0);
					break;
			}
		}

		protected virtual void ValidateBaseQty(INRegister doc, INTran tran, INTranSplit split)
		{
			if (split?.Qty == null)
				return;

			decimal baseQty = INUnitAttribute.ConvertToBase<INTran.inventoryID>(intranselect.Cache, tran, split.UOM, (decimal)split.Qty, INPrecision.QUANTITY);
			if (split.BaseQty != baseQty)
			{
				string docType = PXStringListAttribute.GetLocalizedLabel<INRegister.docType>(inregister.Cache, doc);
				throw new PXException(Messages.BaseQtyIncorrect, docType, tran.RefNbr, tran.LineNbr);
			}
		}

		protected virtual void ValidateFinPeriod(INRegister doc, IEnumerable<PXResult<INTran, InventoryItem, INSite>> records)
	    {
		    Func<PXResult<INTran, InventoryItem, INSite>, int?[]> getBranchIDs = null;

			if (doc.DocType == INDocType.Transfer && doc.TransferType == INTransferType.OneStep)
			{
				INSite siteTo = INSite.PK.Find(this, doc.ToSiteID);

				getBranchIDs = row => new int?[]
				{
					((INTran) row).BranchID,
					((INSite) row).BranchID,
					siteTo.BranchID
				};
			}
			else
			{
				getBranchIDs = row => new int?[]
				{
					((INTran) row).BranchID,
					((INSite) row).BranchID,
				};
			}

			FinPeriodUtils.ValidateMasterFinPeriod<PXResult<INTran, InventoryItem, INSite>>(
			    records,
			    row => ((INTran)row).TranPeriodID,
			    getBranchIDs,
			    typeof(OrganizationFinPeriod.iNClosed));
		}

        public virtual bool IsProjectDropShip(INTran tran)
        {
            return PO.POLineType.IsProjectDropShip(tran?.POLineType);
        }

		protected virtual bool UseStandardCost(string valMethod, INTran tran) => valMethod == INValMethod.Standard;

        public class GLTranInsertionContext
        {
            public virtual INTranCost TranCost { get; set; }
            public virtual INTran INTran { get; set; }
            public virtual INPostClass PostClass { get; set; }
            public virtual InventoryItem Item { get; set; }
            public virtual INSite Site { get; set; }
            public virtual ReasonCode ReasonCode { get; set; }
            public virtual INLocation Location { get; set; }
        }
	}
}
