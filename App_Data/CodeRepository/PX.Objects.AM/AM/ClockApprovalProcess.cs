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
using System.Collections;
using PX.Data;
using System.Collections.Generic;
using PX.Common;
using PX.Objects.AM.Attributes;
using System.Linq;
using PX.Objects.GL;
using PX.Data.BQL.Fluent;
using PX.Data.BQL;

namespace PX.Objects.AM
{
    /// <summary>
    /// Approve Clock Entires (AM516000)
    /// </summary>
    public class ClockApprovalProcess : PXGraph<ClockApprovalProcess>
    {
        public PXCancel<AMClockTran> Cancel;
        public PXSave<AMClockTran> Save;
        [PXFilterable]
        public PXProcessingJoin<AMClockTran,
			InnerJoin<Branch, On<AMClockTran.branchID, Equal<Branch.branchID>>>,
            Where2<Where<AMClockTran.employeeID, Equal<Current<ClockTranFilter.employeeID>>, Or<Current<ClockTranFilter.employeeID>, IsNull>>,
                And2<Where<AMClockTran.orderType, Equal<Current2<ClockTranFilter.orderType>>, Or<Current2<ClockTranFilter.orderType>, IsNull>>,
                    And2<Where<AMClockTran.prodOrdID, Equal<Current<ClockTranFilter.prodOrdID>>, Or<Current<ClockTranFilter.prodOrdID>, IsNull>>,
						And2<Where<AMClockTran.status, Equal<ClockTranStatus.clockedOut>, And<AMClockTran.closeflg, Equal<False>>>,
							And<Branch.baseCuryID, Equal<Current<AccessInfo.baseCuryID>>>>>>>> UnapprovedTrans;
		public PXSelect<AMClockTranSplit, Where<AMClockTranSplit.employeeID, Equal<Current<AMClockTran.employeeID>>,
            And<AMClockTranSplit.lineNbr, Equal<Current<AMClockTran.lineNbr>>>>> splits;
        public PXFilter<ClockTranFilter> Filter;
        public PXSetup<AMPSetup> ampsetup;

        public AMClockTranLineSplittingExtension LineSplittingExt => FindImplementation<AMClockTranLineSplittingExtension>();

        public ClockApprovalProcess()
        {
            ClockTranFilter filter = Filter.Current;
            UnapprovedTrans.SetProcessDelegate(delegate (List<AMClockTran> list)
            {
                // Acuminator disable once PX1088 InvalidViewUsageInProcessingDelegate [Using new instance of process]
                CreateLaborBatch(list, true, filter);
            });

			PXUIFieldAttribute.SetVisible<AMClockTran.employeeID>(UnapprovedTrans.Cache, null, true);

			PXUIFieldAttribute.SetEnabled<AMClockTran.orderType>(UnapprovedTrans.Cache, null, true);
            PXUIFieldAttribute.SetEnabled<AMClockTran.prodOrdID>(UnapprovedTrans.Cache, null, true);
            PXUIFieldAttribute.SetEnabled<AMClockTran.operationID>(UnapprovedTrans.Cache, null, true);
            PXUIFieldAttribute.SetEnabled<AMClockTran.shiftCD>(UnapprovedTrans.Cache, null, true);
            PXUIFieldAttribute.SetEnabled<AMClockTran.qty>(UnapprovedTrans.Cache, null, true);
            PXUIFieldAttribute.SetEnabled<AMClockTran.tranDesc>(UnapprovedTrans.Cache, null, true);
            PXUIFieldAttribute.SetEnabled<AMClockTran.startTime>(UnapprovedTrans.Cache, null, true);
            PXUIFieldAttribute.SetEnabled<AMClockTran.endTime>(UnapprovedTrans.Cache, null, true);
			PXUIFieldAttribute.SetEnabled<AMClockTran.laborTime>(UnapprovedTrans.Cache, null, true);
			PXUIFieldAttribute.SetEnabled<AMClockTran.qtyScrapped>(UnapprovedTrans.Cache, null, true);
			PXUIFieldAttribute.SetEnabled<AMClockTran.scrapAction>(UnapprovedTrans.Cache, null, true);
			PXUIFieldAttribute.SetEnabled<AMClockTran.reasonCodeID>(UnapprovedTrans.Cache, null, true);
			PXUIFieldAttribute.SetEnabled<AMClockTran.tranDesc>(UnapprovedTrans.Cache, null, true);
		}

        public PXAction<AMClockTran> delete;
        [PXUIField(DisplayName = "Delete")]
        [PXButton]
        public virtual IEnumerable Delete(PXAdapter adapter)
        {
            if (UnapprovedTrans.Current == null)
                return adapter.Get();

            UnapprovedTrans.Cache.Delete(UnapprovedTrans.Current);
            return adapter.Get();
        }

		public static void CreateLaborBatch(List<AMClockTran> list, bool isMassProcess, ClockTranFilter filter)
		{
			LaborEntry graph = PXGraph.CreateInstance<LaborEntry>();
			graph.ampsetup.Current.RequireControlTotal = false;
			var batch = graph.batch.Insert();
			if (batch == null)
			{
				throw new PXException(Messages.UnableToCreateRelatedTransaction);
			}

			batch.TranDesc = Messages.GetLocal(Messages.ClockLine);
			batch.OrigDocType = AMDocType.Clock;
			batch.Hold = false;
			var included = new List<int>();

			for (var i = 0; i < list.Count; i++)
			{
				var clock = list[i];
				if (!ClockApprovalProcess.IsClockTranValid(clock))
				{
					PXProcessing<AMClockTran>.SetError(i, Messages.LaborTimeGreaterZeroLessEqual24);
					continue;
				}

				try
				{
					//check to see if an ammtran record has already been created before inserting a new one
					AMMTran existingTran = SelectFrom<AMMTran>.Where<AMMTran.origDocType.IsEqual<AMDocType.clock>
						.And<AMMTran.origBatNbr.IsEqual<@P.AsString>>.And<AMMTran.origLineNbr.IsEqual<@P.AsInt>>>.View.Select(graph, clock.EmployeeID, clock.LineNbr);
					if (existingTran != null)
					{
						PXProcessing<AMClockTran>.SetWarning(i, string.Format(Messages.ClockEntryAlreadyExists, existingTran.BatNbr));
						continue;
					}
						

					var newTran = graph.transactions.Insert();
					if (newTran == null)
					{
						PXProcessing<AMClockTran>.SetError(i, Messages.UnableToCreateRelatedTransaction);
						continue;
					}
					newTran.EmployeeID = clock.EmployeeID;
					newTran.OrderType = clock.OrderType;
					newTran.ProdOrdID = clock.ProdOrdID;
					newTran.OperationID = clock.OperationID;
					newTran.ShiftCD = clock.ShiftCD;
					newTran.Qty = clock.LastOper == true ? 0 : clock.Qty;
					newTran.UOM = clock.UOM;
					newTran.StartTime = PXDBDateAndTimeAttribute.CombineDateTime(Common.Dates.BeginOfTimeDate, clock.StartTime);
					newTran.EndTime = PXDBDateAndTimeAttribute.CombineDateTime(Common.Dates.BeginOfTimeDate, clock.EndTime);
					newTran.OrigDocType = AMDocType.Clock;
					newTran.OrigBatNbr = clock.EmployeeID.ToString();
					newTran.OrigLineNbr = clock.LineNbr;
					newTran.TranDate = (clock.TranDate == null ? System.DateTime.Now : clock.TranDate);
					newTran.QtyScrapped = clock.QtyScrapped;
					newTran.ScrapAction = clock.ScrapAction;
					newTran.ReasonCodeID = clock.ReasonCodeID;
					newTran.TranDesc = clock.TranDesc;
					newTran = graph.transactions.Update(newTran);
					// Possible LaborTime does not align with StartTime and EndTime as time can be split accross multiple orders
					newTran.LaborTime = clock.LaborTime;
					graph.transactions.Update(newTran);
					if (clock.LastOper == true)
					{
						var splitList = PXSelect<AMClockTranSplit, Where<AMClockTranSplit.employeeID, Equal<Required<AMClockTranSplit.employeeID>>,
							And<AMClockTranSplit.lineNbr, Equal<Required<AMClockTranSplit.lineNbr>>>>>.Select(graph, clock.EmployeeID, clock.LineNbr);
						for (var j = 0; j < splitList.Count; j++)
						{
							var split = (AMClockTranSplit)splitList[j];
							var newSplit = graph.splits.Insert();
							newSplit.LocationID = split.LocationID;
							newSplit.LotSerialNbr = split.LotSerialNbr;
							newSplit.ExpireDate = split.ExpireDate;
							newSplit.Qty = split.Qty;
							graph.splits.Update(newSplit);
						}
					}

					PXProcessing<AMClockTran>.SetInfo(i, ActionsMessages.RecordProcessed);
					included.Add(i);
				}
				catch (Exception ex)
				{
					PXTrace.WriteError($"[Employee ID = {clock.EmployeeID}; Line Nbr = {clock.LineNbr}] {ex.Message}");
					PXProcessing<AMClockTran>.SetError(i, ex);
					graph.transactions.Delete(graph.transactions.Current);
				}
			}

			var graphSaved = false;
			try
			{
				if (graph.transactions.Cache.Inserted.Count() > 0)
				{
					graph.Persist();
					graphSaved = true;
					AMDocumentRelease.ReleaseDoc(new List<AMBatch> { graph.batch.Current }, false);
				}
			}
			catch (Exception e)
			{
				foreach (var i in included)
				{
					PXProcessing<AMClockTran>.SetError(i, e);
				}

				if (e is PXOuterException)
				{
					PXTraceHelper.PxTraceOuterException(e, PXTraceHelper.ErrorLevel.Error);
				}

				if (graphSaved)
				{
					graph.Delete.Press();
				}
			}
		}

		/// <summary>
		/// Calculate the time between the user entered start/end times
		/// </summary>
		protected virtual int GetStartEndLaborTime(PXCache cache, AMClockTran tran)
        {
			return AMDateInfo.GetDateMinutes(tran.StartTime.Value, tran.EndTime.Value);
        }

        /// <summary>
        /// Sets the Labor Time field with the calculated start/end labor hours value
        /// </summary>
        /// <param name="cache"></param>
        /// <param name="tran"></param>
        protected virtual void CalcLaborTime(PXCache cache, AMClockTran tran)
        {
            if (tran == null)
            {
                return;
            }

            var newLaborTime = Math.Max(GetStartEndLaborTime(cache, tran), 1);
            cache.SetValueExt<AMClockTran.laborTime>(tran, newLaborTime);
        }

        protected virtual void _(Events.FieldUpdated<ClockTranFilter.orderType> e)
        {
            e.Cache.SetValueExt<ClockTranFilter.prodOrdID>(e.Row, null);
        }

        protected virtual void _(Events.RowUpdated<AMClockTran> e)
        {
			if((int)(e.Row.EndTime.GetValueOrDefault() - e.OldRow.EndTime.GetValueOrDefault()).TotalMinutes == 0
				&& (int)(e.Row.StartTime.GetValueOrDefault() - e.OldRow.StartTime.GetValueOrDefault()).TotalMinutes == 0)
			{
				return;
			}

            CalcLaborTime(e.Cache, e.Row);

            if(e.Row.StartTime == null)
            {
                return;
            }

            // Keep start and tran date in sync
            var startDate = e.Row.StartTime.GetValueOrDefault().Date;
            if (startDate.Equals(e.Row.TranDate.GetValueOrDefault()))
            {
                return;
            }

            e.Cache.SetValueExt<AMClockTran.tranDate>(e.Row, startDate);
        }

        protected virtual void _(Events.RowPersisting<AMClockTran> e)
        {
            if (string.IsNullOrWhiteSpace(e.Row?.ProdOrdID) || e.Operation == PXDBOperation.Delete)
            {
                return;
            }

            if (e.Row.LaborTime.GetValueOrDefault() == 0 && e.Row.Qty.GetValueOrDefault() == 0 )
            {
                e.Cache.RaiseExceptionHandling<AMClockTran.qty>(
                    e.Row,
                    e.Row.Qty,
                    new PXSetPropertyException(Messages.GetLocal(Messages.FieldCannotBeZero, PXUIFieldAttribute.GetDisplayName<AMClockTran.qty>(e.Cache)),
                        PXErrorLevel.Error));
                e.Cache.RaiseExceptionHandling<AMClockTran.laborTime>(
                    e.Row,
                    e.Row.LaborTime,
                    new PXSetPropertyException(Messages.GetLocal(Messages.FieldCannotBeZero, PXUIFieldAttribute.GetDisplayName<AMClockTran.laborTime>(e.Cache)),
                        PXErrorLevel.Error));
            }

						if (e.Row.QtyScrapped != 0 && e.Row.ScrapAction == ScrapAction.WriteOff && e.Row.ReasonCodeID == null)
            {
                new PXRowPersistingException(typeof(AMClockTran.reasonCodeID).Name, null, ErrorMessages.FieldIsEmpty,
                    typeof(AMClockTran.reasonCodeID).Name);
            }
        }

		protected virtual void _(Events.RowSelected<AMClockTran> e)
		{
			var row = e.Row;
			if (row == null) return;

			AMProdItem aMProdItem = AMProdItem.PK.Find(this, row.OrderType, row.ProdOrdID);

			PXUIFieldAttribute.SetEnabled<AMClockTran.qtyScrapped>(e.Cache, row, row.ScrapAction != ScrapAction.Quarantine && row.IsLotSerialPreassigned == false && aMProdItem.Function != OrderTypeFunction.Disassemble);
			PXUIFieldAttribute.SetEnabled<AMClockTran.reasonCodeID>(e.Cache, row, row.QtyScrapped != 0 && (row.ScrapAction == ScrapAction.WriteOff || row.ScrapAction == ScrapAction.NoAction) && row.IsLotSerialPreassigned == false);
			PXUIFieldAttribute.SetEnabled<AMClockTran.scrapAction>(e.Cache, row, row.IsLotSerialPreassigned == false);

		}

		public static bool IsClockTranValid(AMClockTran clockTran)
        {
            return clockTran?.LaborTime != null && clockTran.LaborTime.GetValueOrDefault().BetweenInclusive(1, 1440);
        }
    }

	/// <summary>
	/// An optional filter for information on the Approve Clock Entries (AM516000) form (corresponding to the <see cref="ClockApprovalProcess"/> graph).
	/// </summary>
	[Serializable]
    [PXCacheName("Clock Filter")]
    public class ClockTranFilter : IBqlTable
    {
        #region EmployeeID
        public abstract class employeeID : PX.Data.BQL.BqlInt.Field<employeeID> { }

        protected Int32? _EmployeeID;
        [PXInt]
        [ProductionEmployeeSelector]
        [PXUIField(DisplayName = "Employee ID")]
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
        #region OrderType
        public abstract class orderType : PX.Data.BQL.BqlString.Field<orderType> { }

        protected String _OrderType;
        [PXDefault(typeof(AMPSetup.defaultOrderType))]
        [AMOrderTypeField(Required = false)]
        [PXRestrictor(typeof(Where<AMOrderType.function, NotEqual<OrderTypeFunction.planning>>), Messages.IncorrectOrderTypeFunction)]
        [PXRestrictor(typeof(Where<AMOrderType.active, Equal<True>>), PX.Objects.SO.Messages.OrderTypeInactive)]
        [AMOrderTypeSelector]
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
        #region ProdOrdID
        public abstract class prodOrdID : PX.Data.BQL.BqlString.Field<prodOrdID> { }

        protected String _ProdOrdID;
        [ProductionNbr]
        [ProductionOrderSelector(typeof(orderType), true)]
        [PXRestrictor(typeof(Where<AMProdItem.isOpen, Equal<True>>),
            Messages.ProdStatusInvalidForProcess, typeof(AMProdItem.orderType), typeof(AMProdItem.prodOrdID), typeof(AMProdItem.statusID))]
        public virtual String ProdOrdID
        {
            get
            {
                return this._ProdOrdID;
            }
            set
            {
                this._ProdOrdID = value;
            }
        }
        #endregion
    }
}
