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
using PX.Objects.IN;
using PX.Objects.AM.GraphExtensions;
using PX.Objects.CS;
using PX.Objects.EP;
using PX.Objects.AM.Attributes;
using PX.Objects.AM.CacheExtensions;

namespace PX.Objects.AM
{
    public class LaborEntry : MoveEntryBase<Where<AMBatch.docType, Equal<AMDocType.labor>>>
    {
        #region LineSplitting
        // Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
        public class LineSplittingExtension : AMMoveLineSplittingExtension<LaborEntry> { }
        public LineSplittingExtension LineSplittingExt => FindImplementation<LineSplittingExtension>();
        public override PXSelectBase<AMMTran> LSSelectDataMember => LineSplittingExt.lsselect;

        // Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
        public class ItemAvailabilityExtension : AMBatchItemAvailabilityExtension<LaborEntry> { }
        #endregion

        [PXHidden]
        public PXSelect<AMClockTran> clockTrans;

        public LaborEntry()
        {
            GL.OpenPeriodAttribute.SetValidatePeriod<AMBatch.finPeriodID>(batch.Cache, null, GL.PeriodValidation.DefaultSelectUpdate);
            PXVerifySelectorAttribute.SetVerifyField<AMMTran.receiptNbr>(transactions.Cache, null, true);
            PXUIFieldAttribute.SetVisible<AMMTran.expireDate>(transactions.Cache, null, false);
            PXUIFieldAttribute.SetVisible<AMMTran.lotSerialNbr>(transactions.Cache, null, false);
        }

        protected override void AMMTran_RowDeleting(PXCache sender, PXRowDeletingEventArgs e)
        {
            var row = (AMMTran)e.Row;
            if (row == null || sender.GetStatus(row) == PXEntryStatus.InsertedDeleted)
            {
                return;
            }

            //Only prompt when a non referenced batch
            if (batch.Current != null 
                && string.IsNullOrWhiteSpace(batch.Current.OrigBatNbr)
                && row.DocType == batch.Current.DocType && row.BatNbr == batch.Current.BatNbr
                && !_skipReleasedReferenceDocsCheck 
                && ReferenceDeleteGraph.HasReleasedReferenceDocs(this, row, true))
            {
                throw new PXException(Messages.ReleasedTransactionExist);
            }
        }

        protected virtual void AMMTran_RowDeleted(PXCache sender, PXRowDeletedEventArgs e)
        {
            var row = (AMMTran)e.Row;
            if (row == null)
                return;

            //If labor came from Clock Entry, reopen the tran
            if(row.OrigDocType == AMDocType.Clock)
            {
                AMClockTran origTran = PXSelect<AMClockTran,
					Where<AMClockTran.employeeID, Equal<Required<AMClockTran.employeeID>>,
						And<AMClockTran.lineNbr, Equal<Required<AMClockTran.lineNbr>>
							>>>
					.Select(this, row.OrigBatNbr, row.OrigLineNbr);
                if(origTran != null)
                {
                    origTran = (AMClockTran)clockTrans.Cache.CreateCopy(origTran);
                    origTran.Closeflg = false;
                    clockTrans.Cache.Update(origTran);
                }
            }
        }

        protected override void AMBatch_DocType_FieldDefaulting(PXCache cache, PXFieldDefaultingEventArgs e)
        {
            e.NewValue = AMDocType.Labor;
        }

        protected override void AMMTran_DocType_FieldDefaulting(PXCache cache, PXFieldDefaultingEventArgs e)
        {
            e.NewValue = AMDocType.Labor;
        }

        protected override void AMMTranSplit_DocType_FieldDefaulting(PXCache cache, PXFieldDefaultingEventArgs e)
        {
            e.NewValue = AMDocType.Labor;
        }


        protected override void AMMTran_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
        {
            base.AMMTran_RowSelected(sender, e);
            LaborAMMTranRowSelected(sender, e);
        }

        protected override void AMMTran_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
        {
            LaborAMMTranRowPersisting(sender, e);
            base.AMMTran_RowPersisting(sender, e);
        }

        protected override void AMMTran_ProdOrdID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
        {
            var row = (AMMTran)e.Row;
            if (row == null)
            {
                return;
            }

            base.AMMTran_ProdOrdID_FieldUpdated(sender, e);
            SetDirectLaborCode((AMMTran)e.Row);

            AMProdItem amProdItem = PXSelect<AMProdItem, Where<AMProdItem.orderType, Equal<Required<AMProdItem.orderType>>,
                And<AMProdItem.prodOrdID, Equal<Required<AMProdItem.prodOrdID>>>>>.Select(this, row.OrderType, row.ProdOrdID);
            if (amProdItem != null && amProdItem.Function == OrderTypeFunction.Disassemble)
            {
                sender.SetValue<AMMTran.qty>(row, 0m);
                sender.SetValue<AMMTran.qtyScrapped>(row, 0m);
            }
        }

        protected override void AMMTranAttribute_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
        {
            var row = (AMMTranAttribute)e.Row;
            if (row == null)
            {
                return;
            }
            
            var ammTran = GetParent(sender, row) ?? transactions.Current;
            if (ammTran == null || ammTran.Qty.GetValueOrDefault() == 0 && ammTran.QtyScrapped.GetValueOrDefault() == 0)
            {
                return;
            }

            base.AMMTranAttribute_RowPersisting(sender, e);
        }

        #region Cache Attached
      
        [PXMergeAttributes(Method = MergeMethod.Append)]
        [PXDefault(TimeCardStatus.Unprocessed, PersistingCheck = PXPersistingCheck.Nothing)]
        protected virtual void _(Events.CacheAttached<AMMTran.timeCardStatus> e) { }

        [PXMergeAttributes(Method = MergeMethod.Append)]
        [PXDefault(0)]
        protected virtual void _(Events.CacheAttached<AMMTran.laborTime> e) { }

		[PXMergeAttributes(Method = MergeMethod.Replace)]
		[RawTimeField(typeof(AMMTran.laborTime), RawMinValue = -1440, RawMaxValue = 1440)]
		[PXDependsOnFields(typeof(AMMTran.laborTime))]
		protected virtual void _(Events.CacheAttached<AMMTran.laborTimeRaw> e) { }

		[PXDBInt]
        [ProductionEmployeeSelector]
        [PXDefault(typeof(Search<EPEmployee.bAccountID,
                    Where<EPEmployee.userID, Equal<Current<AccessInfo.userID>>,
                    And<EPEmployeeExt.amProductionEmployee, Equal<True>,
                    And<Current<AMPSetup.defaultEmployee>, Equal<True>>>>>), PersistingCheck = PXPersistingCheck.Null)]
        [PXUIField(DisplayName = "Employee ID", Visibility = PXUIVisibility.SelectorVisible, Required = true)]
        protected virtual void _(Events.CacheAttached<AMMTran.employeeID> e) { }

        [PXDefault(PersistingCheck = PXPersistingCheck.NullOrBlank)]
        [PXMergeAttributes(Method = MergeMethod.Append)]
        protected virtual void _(Events.CacheAttached<AMMTran.shiftCD> e) { }

        [PXDefault(typeof(Search<AMProdOper.operationID,
            Where<AMProdOper.orderType, Equal<Current<AMMTran.orderType>>, 
                And<AMProdOper.prodOrdID, Equal<Current<AMMTran.prodOrdID>>>>,
            OrderBy<Asc<AMProdOper.operationCD>>>), PersistingCheck = PXPersistingCheck.Nothing)]
        [PXMergeAttributes(Method = MergeMethod.Merge)]
        protected virtual void _(Events.CacheAttached<AMMTran.operationID> e) { }

        [PXDefault(typeof(AMPSetup.defaultOrderType), PersistingCheck = PXPersistingCheck.Nothing)]
        [PXMergeAttributes(Method = MergeMethod.Merge)]
        protected virtual void _(Events.CacheAttached<AMMTran.orderType> e) { }

        [PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
        [PXMergeAttributes(Method = MergeMethod.Merge)]
        protected virtual void _(Events.CacheAttached<AMMTran.prodOrdID> e) { }

        [PXDefault(typeof(Search<AMProdItem.inventoryID,
            Where<AMProdItem.orderType, Equal<Current<AMMTran.orderType>>,
                And<AMProdItem.prodOrdID, Equal<Current<AMMTran.prodOrdID>>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
        [Inventory(Enabled = false)]
        protected virtual void _(Events.CacheAttached<AMMTran.inventoryID> e) { }

        [PXDefault(typeof(Search<AMProdItem.siteID,
            Where<AMProdItem.orderType, Equal<Current<AMMTran.orderType>>,
                And<AMProdItem.prodOrdID, Equal<Current<AMMTran.prodOrdID>>>>>),
            PersistingCheck = PXPersistingCheck.Nothing)]
        [AMSiteAvail(typeof(AMMTran.inventoryID), typeof(AMMTran.subItemID))]
        protected virtual void _(Events.CacheAttached<AMMTran.siteID> e) { }

        [PXDefault(typeof(Search<AMProdItem.uOM, Where<AMProdItem.prodOrdID, Equal<Current<AMMTran.prodOrdID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
        [INUnit(typeof(AMMTran.inventoryID))]
        protected virtual void _(Events.CacheAttached<AMMTran.uOM> e) { }

        [PXDBInt]
        [PXUIField(DisplayName = "WIP Account")]
        protected virtual void _(Events.CacheAttached<AMMTran.wIPAcctID> e) { }

        [PXDBInt]
        [PXUIField(DisplayName = "WIP Subaccount")]
        protected virtual void _(Events.CacheAttached<AMMTran.wIPSubID> e) { }

		[PXMergeAttributes(Method = MergeMethod.Merge)]
        [IndirectLabor]
        protected virtual void _(Events.CacheAttached<AMMTran.laborCodeID> e) { }

        [PXDBString(1, IsFixed = true)]
        [AMLaborType.List]
        [PXDefault(AMLaborType.Direct)]
        [PXUIField(DisplayName = "Labor Type")]
        protected virtual void _(Events.CacheAttached<AMMTran.laborType> e) { }

        #endregion

        protected virtual void AMMTran_LaborCodeID_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
        {
            AMMTran ammTran = (AMMTran)e.Row;
            if (ammTran == null || string.IsNullOrWhiteSpace(ammTran.LaborType))
            {
                return;
            }

            //labor code select is based on indirect but direct entries auto fill in the labor code from the shift - no need to validate those
            if (ammTran.LaborType == AMLaborType.Direct)
            {
                e.Cancel = true;
            }
        }

        protected virtual void LaborAMMTranRowPersisting(PXCache sender, PXRowPersistingEventArgs e)
        {
            AMMTran ammTran = (AMMTran)e.Row;

            if (ammTran == null || (this.IsContractBasedAPI && ammTran.LaborType == AMLaborType.Direct  && ammTran.ProdOrdID == null))
            {
				e.Cancel = true;
                return;
            }

            //Need the validation here for field required due to labor entry allowing both direct and indirect labor.

            if (ammTran.LaborTime.GetValueOrDefault() == 0)
            {
                sender.RaiseExceptionHandling<AMMTran.laborTimeRaw>(
                    ammTran,
                    ammTran.LaborTime,
                    new PXSetPropertyException(Messages.GetLocal(Messages.FieldCannotBeZero, PXUIFieldAttribute.GetDisplayName<AMMTran.laborTime>(sender)),
                        PXErrorLevel.Error));
            }

            if (string.IsNullOrWhiteSpace(ammTran.LaborCodeID))
            {
                sender.RaiseExceptionHandling<AMMTran.laborCodeID>(
                    ammTran,
                    ammTran.LaborCodeID,
                    new PXSetPropertyException(ErrorMessages.FieldIsEmpty, PXUIFieldAttribute.GetDisplayName<AMMTran.laborCodeID>(sender),
                        PXErrorLevel.Error));
            }

            if (ammTran.LaborType == AMLaborType.Indirect)
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(ammTran.OrderType))
            {
                sender.RaiseExceptionHandling<AMMTran.orderType>(
                    ammTran,
                    ammTran.OrderType,
                    new PXSetPropertyException(ErrorMessages.FieldIsEmpty, PXUIFieldAttribute.GetDisplayName<AMMTran.orderType>(sender),
                        PXErrorLevel.Error));
            }

            if (string.IsNullOrWhiteSpace(ammTran.ProdOrdID))
            {
                sender.RaiseExceptionHandling<AMMTran.prodOrdID>(
                    ammTran,
                    ammTran.ProdOrdID,
                    new PXSetPropertyException(ErrorMessages.FieldIsEmpty, PXUIFieldAttribute.GetDisplayName<AMMTran.prodOrdID>(sender),
                        PXErrorLevel.Error));
            }

            if (ammTran.OperationID == null)
            {
                sender.RaiseExceptionHandling<AMMTran.operationID>(
                    ammTran,
                    ammTran.OperationID,
                    new PXSetPropertyException(ErrorMessages.FieldIsEmpty, PXUIFieldAttribute.GetDisplayName<AMMTran.operationID>(sender),
                        PXErrorLevel.Error));
            }

            if (string.IsNullOrWhiteSpace(ammTran.ShiftCD))
            {
                sender.RaiseExceptionHandling<AMMTran.shiftCD>(
                    ammTran,
                    ammTran.ShiftCD,
                    new PXSetPropertyException(ErrorMessages.FieldIsEmpty, PXUIFieldAttribute.GetDisplayName<AMMTran.shiftCD>(sender),
                        PXErrorLevel.Error));
            }

            if (ammTran.SiteID == null)
            {
                sender.RaiseExceptionHandling<AMMTran.siteID>(
                    ammTran,
                    ammTran.SiteID,
                    new PXSetPropertyException(ErrorMessages.FieldIsEmpty, PXUIFieldAttribute.GetDisplayName<AMMTran.siteID>(sender),
                        PXErrorLevel.Error));
            }

            if (ammTran.InventoryID == null)
            {
                sender.RaiseExceptionHandling<AMMTran.inventoryID>(
                    ammTran,
                    ammTran.InventoryID,
                    new PXSetPropertyException(ErrorMessages.FieldIsEmpty, PXUIFieldAttribute.GetDisplayName<AMMTran.inventoryID>(sender),
                        PXErrorLevel.Error));
            }

            if (string.IsNullOrWhiteSpace(ammTran.UOM))
            {
                sender.RaiseExceptionHandling<AMMTran.uOM>(
                    ammTran,
                    ammTran.UOM,
                    new PXSetPropertyException(ErrorMessages.FieldIsEmpty, PXUIFieldAttribute.GetDisplayName<AMMTran.uOM>(sender),
                        PXErrorLevel.Error));
            }

            if (ammTran.WIPAcctID == null)
            {
                sender.RaiseExceptionHandling<AMMTran.wIPAcctID>(
                    ammTran,
                    ammTran.WIPAcctID,
                    new PXSetPropertyException(ErrorMessages.FieldIsEmpty, PXUIFieldAttribute.GetDisplayName<AMMTran.wIPAcctID>(sender),
                        PXErrorLevel.Error));
            }

            if (ammTran.WIPSubID == null)
            {
                sender.RaiseExceptionHandling<AMMTran.wIPSubID>(
                    ammTran,
                    ammTran.WIPSubID,
                    new PXSetPropertyException(ErrorMessages.FieldIsEmpty, PXUIFieldAttribute.GetDisplayName<AMMTran.wIPSubID>(sender),
                        PXErrorLevel.Error));
            }
        }

        protected virtual void LaborAMMTranRowSelected(PXCache sender, PXRowSelectedEventArgs e)
        {
            AMMTran ammTran = (AMMTran)e.Row;
            if (ammTran == null)
            {
                return;
            }

            if (ammTran.HasReference.GetValueOrDefault())
            {
                return;
            }

            PXUIFieldAttribute.SetEnabled<AMMTran.laborTime>(sender, e.Row, ammTran.StartTime == null && ammTran.EndTime == null);
        }

        #region Labor start/end time


        protected virtual void AMMTran_StartTime_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
        {
            CalcLaborTime(cache, (AMMTran)e.Row);
        }

        /// <summary>
        /// Calculate the time between the user entered start/end times
        /// </summary>
        /// <param name="cache">cache object</param>
        /// <param name="ammTran">ammtran row</param>
        /// <returns>decimal of hours</returns>
        protected virtual int GetStartEndLaborTime(PXCache cache, AMMTran ammTran)
        {
            if (ammTran == null || ammTran.EndTime == null || ammTran.StartTime == null)
            {
                return 0;
            }
            TimeSpan? timeSpan;
            if (Common.Dates.StartBeforeEnd(ammTran.StartTime, ammTran.EndTime))
                timeSpan = ammTran.EndTime - ammTran.StartTime;
            else
                timeSpan = ammTran.EndTime.Value.AddDays(1) - ammTran.StartTime;

            return timeSpan == null ? 0 : Convert.ToInt32(timeSpan.Value.TotalMinutes);
        }

        /// <summary>
        /// Sets the Labor Hours field with the calculated start/end labor hours value
        /// </summary>
        /// <param name="cache"></param>
        /// <param name="ammTran"></param>
        protected virtual void CalcLaborTime(PXCache cache, AMMTran ammTran)
        {
            if (ammTran == null)
            {
                return;
            }

            var newLaborHours = GetStartEndLaborTime(cache, ammTran);
            cache.SetValueExt<AMMTran.laborTime>(ammTran, newLaborHours);
        }

        protected virtual void AMMTran_EndTime_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
        {
            CalcLaborTime(cache, (AMMTran)e.Row);
        }

        #endregion

        protected override void AMMTran_OperationID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
        {
            base.AMMTran_OperationID_FieldUpdated(sender, e);
            SetDefaultShift(sender, (AMMTran) e.Row);
            SetLaborAmount(sender, (AMMTran)e.Row);
        }

        protected virtual void SetDefaultShift(PXCache sender, AMMTran row)
        {
            if (row?.OperationID == null || row.ProdOrdID == null || IsCopyPasteContext || IsImport || IsContractBasedAPI)
            {
                return;
            }

            PXResultset<AMShift> result = PXSelectJoin<AMShift,
                    InnerJoin<AMProdOper, On<AMShift.wcID, Equal<AMProdOper.wcID>>>,
                    Where<AMProdOper.orderType, Equal<Required<AMProdOper.orderType>>,
                        And<AMProdOper.prodOrdID, Equal<Required<AMProdOper.prodOrdID>>,
                            And<AMProdOper.operationID, Equal<Required<AMProdOper.operationID>>>>>>
                .Select(this, row.OrderType, row.ProdOrdID, row.OperationID);

            if (result == null || result.Count != 1)
            {
                return;
            }

            sender.SetValueExt<AMMTran.shiftCD>(row, ((AMShift)result[0])?.ShiftCD);
        }

        protected virtual void AMMTran_EmployeeID_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
        {
            SetLaborAmount(cache, (AMMTran)e.Row);
        }

        protected virtual void _(Events.FieldUpdated<AMMTran, AMMTran.shiftCD> e)
        {
            SetDirectLaborCode(e.Row);
            SetLaborAmount(e.Cache, e.Row);
        }
        
        public virtual void SetDirectLaborCode(AMMTran ammTran)
        {
            if (string.IsNullOrWhiteSpace(ammTran?.ProdOrdID)
                || ammTran.OperationID == null
                || string.IsNullOrWhiteSpace(ammTran.ShiftCD)
                || ammTran.LaborType != AMLaborType.Direct)
            {
                return;
            }

            AMProdOper amProdOper = PXSelect<
                    AMProdOper,
                    Where<AMProdOper.orderType, Equal<Required<AMProdOper.orderType>>,
                        And<AMProdOper.prodOrdID, Equal<Required<AMProdOper.prodOrdID>>,
                            And<AMProdOper.operationID, Equal<Required<AMProdOper.operationID>>>>>>
                .Select(this, ammTran.OrderType, ammTran.ProdOrdID, ammTran.OperationID);

            AMShift amShift = PXSelect<
                AMShift, 
                Where<AMShift.wcID, Equal<Required<AMShift.wcID>>,
                    And<AMShift.shiftCD, Equal<Required<AMShift.shiftCD>>
                    >>>
                .Select(this, amProdOper.WcID, ammTran.ShiftCD);

            if (amShift != null)
            {
                ammTran.LaborCodeID = amShift.LaborCodeID;
            }

            if (string.IsNullOrWhiteSpace(ammTran.LaborCodeID))
            {
                throw new PXException(Messages.NoLaborCodeForShift);
            }

        }

        protected virtual void AMMTran_OrderType_FieldDefaulting(PXCache cache, PXFieldDefaultingEventArgs e)
        {
            AMMTran tran = (AMMTran)e.Row;
            if (tran == null)
            {
                return;
            }

            if (tran.LaborType == AMLaborType.Indirect)
            {
                e.Cancel = true;
            }
        }

        protected virtual void AMMTran_LaborType_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
        {
            var tran = (AMMTran)e.Row;
            if (tran == null)
            {
                return;
            }

            if (tran.LaborType == AMLaborType.Indirect)
            {               
                cache.SetValueExt<AMMTran.orderType>(e.Row, null);
                cache.SetValueExt<AMMTran.operationID>(e.Row, null);
                cache.SetValueExt<AMMTran.prodOrdID>(e.Row, null);
                cache.SetValueExt<AMMTran.subItemID>(e.Row, null);
                tran.Qty = (decimal)0.0;
                tran.QtyScrapped = (decimal)0.0;
                cache.SetValueExt<AMMTran.uOM>(e.Row, null);
                cache.SetValueExt<AMMTran.siteID>(e.Row, null);
                cache.SetValueExt<AMMTran.locationID>(e.Row, null);
                cache.SetValueExt<AMMTran.laborCodeID>(e.Row, null);
                cache.SetValueExt<AMMTran.inventoryID>(e.Row, null);
                return;
            }

            cache.SetDefaultExt<AMMTran.orderType>(e.Row);
            cache.SetValueExt<AMMTran.laborCodeID>(e.Row, null);
        }

       protected virtual void AMMTran_LaborTime_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
        {
            SetLaborAmount(cache, (AMMTran)e.Row);
        }

		protected virtual void _(Events.RowPersisting<AMBatch> e, PXRowPersisting baseMethod)
		{
			if (baseMethod != null)
			{
			baseMethod.Invoke(e.Cache, e.Args);
			}
			CheckForUnreleasing(e);
		}

		protected virtual void CheckForUnreleasing(Events.RowPersisting<AMBatch> e)
		{
			ProductionTransactionHelper.CheckForUnreleasedBatches(e);
		}

        /// <summary>
        /// Sets the transaction related labor amount fields
        /// </summary>
        protected virtual void SetLaborAmount(PXCache sender, AMMTran tran)
        {
            if (tran?.DocType == null)
            {
                return;
            }

            var laborRate = GetLaborRate(tran);
            if (laborRate == null)
            {
                return;
            }

#if DEBUG
            AMDebug.TraceWriteMethodName($"[{tran.BatNbr}:{tran.LineNbr}] Current LaborRate = {tran.LaborRate}; Calculated LaborRate = {laborRate}");
#endif
            if (tran.LaborRate != laborRate)
            {
                sender.SetValueExt<AMMTran.laborRate>(tran, laborRate);
            }

            var shiftCode = (EPShiftCode)PXSelectorAttribute.Select<AMMTran.shiftCD>(sender, tran);
            (decimal? shftDiff, string diffType) = ShiftMaint.GetShiftDiffAndType(sender.Graph, shiftCode);
            var shiftAddn = diffType != null ? ShiftDiffType.GetShiftDifferentialCost(tran.LaborRate, shftDiff, diffType) : tran.LaborRate;
            var extCost = UomHelper.PriceCostRound(tran.LaborTime.GetValueOrDefault() * shiftAddn.GetValueOrDefault() / 60.0m);
#if DEBUG
            AMDebug.TraceWriteMethodName($"[{tran.BatNbr}:{tran.LineNbr}] Current ExtCost = {tran.ExtCost}; Calculated ExtCost = {extCost}");
#endif
            if (tran.ExtCost != extCost)
            {
                sender.SetValueExt<AMMTran.extCost>(tran, extCost);
            }
        }

        /// <summary>
        /// Determine the correct labor rate for the transactions
        /// </summary>
        protected virtual decimal? GetLaborRate(AMMTran tran)
        {
            if (tran?.EmployeeID == null || tran.TranDate == null || tran.ShiftCD == null)
            {
                return 0m;
            }

            if (ampsetup.Current.DfltLbrRate == LaborRateType.Standard && tran.LaborType == AMLaborType.Direct)
            {
                var amwc = (AMWCCury)PXSelectJoin<AMWCCury,
                    InnerJoin<AMProdOper, On<AMWCCury.wcID, Equal<AMProdOper.wcID>>>,
                    Where<AMProdOper.orderType, Equal<Required<AMProdOper.orderType>>,
                        And<AMProdOper.prodOrdID, Equal<Required<AMProdOper.prodOrdID>>,
                            And<AMProdOper.operationID, Equal<Required<AMProdOper.operationID>>,
							And<AMWCCury.curyID, Equal<Current<AccessInfo.baseCuryID>>>>>>>.SelectWindowed(this, 0, 1, tran.OrderType, tran.ProdOrdID, tran.OperationID);
                return amwc?.StdCost ?? 0m;
            }

            try
            {
                var empCostEngine = new AMEmployeeCostEngine(this);
                return empCostEngine.GetEmployeeHourlyRate(tran.ProjectID, tran.TaskID, tran.EmployeeID, tran.TranDate).GetValueOrDefault();
            }
            catch (Exception exception)
            {
                PXTraceHelper.PxTraceException(exception);
            }

            return null;
        }
    }
}
