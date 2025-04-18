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
using PX.Objects.EP;
using PX.Objects.AM.Attributes;
using PX.Objects.CS;
using System.Collections;
using PX.Objects.AM.CacheExtensions;
using PX.Objects.GL;
using PX.Objects.GDPR;

namespace PX.Objects.AM
{
    /// <summary>
    /// Clock Entry (AM315000)
    /// </summary>
    public class ClockEntry : PXGraph<ClockEntry, AMClockItem>
    {
        public PXSelect<AMClockItem, Where<AMClockItem.employeeID, Equal<Optional<AMClockItem.employeeID>>>> header;
		public PXSelectJoin<AMClockTran,
			InnerJoin<Branch, On<Branch.branchID, Equal<AMClockTran.branchID>, And<Branch.baseCuryID, Equal<Current<AccessInfo.baseCuryID>>>>>,
			Where<AMClockTran.employeeID, Equal<Current<AMClockItem.employeeID>>>> transactions;
        public PXSelect<AMClockItemSplit,
			Where<AMClockItemSplit.employeeID, Equal<Current<AMClockItem.employeeID>>,
            And<AMClockItemSplit.lineNbr, Equal<int0>>>> splits;
        public PXSelect<AMClockTranSplit,
			Where<AMClockTran.employeeID, Equal<Current<AMClockTran.employeeID>>,
            And<AMClockTranSplit.lineNbr, Equal<Current<AMClockTran.lineNbr>>>>> transplits;

        public PXAction<AMClockItem> clockInOut;

        public PXSetup<AMPSetup> prodsetup;

        public AMClockItemLineSplittingExtension LineSplittingExt => FindImplementation<AMClockItemLineSplittingExtension>();

        public ClockEntry()
        {
			var tranCache = transactions.Cache;
			tranCache.AllowInsert =
				tranCache.AllowUpdate =
					tranCache.AllowDelete = false;

            // Remove red asterisks on read only grid
            PXUIFieldAttribute.SetRequired<AMClockTran.startTime>(tranCache, false);
            PXUIFieldAttribute.SetRequired<AMClockTran.endTime>(tranCache, false);
            PXUIFieldAttribute.SetRequired<AMClockTran.orderType>(tranCache, false);
            PXUIFieldAttribute.SetRequired<AMClockTran.prodOrdID>(tranCache, false);
            PXUIFieldAttribute.SetRequired<AMClockTran.operationID>(tranCache, false);
            PXUIFieldAttribute.SetRequired<AMClockTran.inventoryID>(tranCache, false);
            PXUIFieldAttribute.SetRequired<AMClockTran.subItemID>(tranCache, false);
            PXUIFieldAttribute.SetRequired<AMClockTran.uOM>(tranCache, false);
            PXUIFieldAttribute.SetRequired<AMClockTran.siteID>(tranCache, false);
            PXUIFieldAttribute.SetRequired<AMClockTran.tranDate>(tranCache, false);
        }

		[InjectDependency]
		public ICommon Common
		{
			get;
			set;
		}

        #region Events

        protected virtual void _(Events.RowSelected<AMClockItem> e)
        {
            if (e.Row == null)
            {
                return;
            }

            var prodEmployee = CheckProdEmployee(e.Cache, e.Row);
            EnableFields(e.Row.IsClockedIn == true, prodEmployee);
        }

		protected virtual void _(Events.RowSelecting<AMClockItem> e)
		{
			if (e.Row == null)
				return;

			if(e.Row.BranchID != Accessinfo.BranchID)
			{
				var branch = Branch.PK.Find(this, e.Row.BranchID);
				if (branch == null || branch.BaseCuryID == Accessinfo.BaseCuryID)
					return;
				if (e.Row.IsClockedIn == true)
				{
					throw new Exception(String.Format(Messages.DuplicateClockInProductionOrder, e.Row.OrderType, e.Row.ProdOrdID));
				}
				e.Row.BranchID = Accessinfo.BranchID;
				e.Row.OrderType = null;
				e.Row.ProdOrdID = null;
				e.Row.OperationID = null;
				e.Row.ShiftCD = null;
				e.Cache.SetDefaultExt<AMClockItem.orderType>(e.Row);
			}

		}

        protected virtual void _(Events.FieldDefaulting<AMClockItem, AMClockItem.orderType> e)
        {
            e.NewValue = prodsetup.Current.DefaultOrderType;
        }

		protected virtual void _(Events.FieldDefaulting<AMClockTran, AMClockTran.status> e)
		{
			e.NewValue = ClockTranStatus.ClockedOut;
		}

		protected virtual void _(Events.FieldUpdated<AMClockItem, AMClockItem.operationID> e)
        {
            SetDefaultShift(e.Cache, e.Row);
            if (string.IsNullOrWhiteSpace(e.Row?.ProdOrdID) || e.Row.OperationID == null)
            {
                return;
            }

            AMProdItem prodItem = PXSelect<AMProdItem,
                Where<AMProdItem.orderType, Equal<Required<AMProdItem.orderType>>,
                    And<AMProdItem.prodOrdID, Equal<Required<AMProdItem.prodOrdID>>
                >>>.Select(this, e.Row.OrderType, e.Row.ProdOrdID);

            e.Cache.SetValueExt<AMClockItem.lastOper>(e.Row, prodItem?.LastOperationID == e.Row.OperationID);
            e.Cache.SetValueExt<AMClockItem.siteID>(e.Row, prodItem?.SiteID);
            e.Cache.SetValueExt<AMClockItem.locationID>(e.Row, prodItem?.LocationID);
        }

		protected virtual void _(Events.FieldUpdated<AMClockItem.prodOrdID> e)
		{
			//if the user changes the prodorder they need to reselect the operation
			e.Cache.SetValueExt<AMClockItem.operationID>(e.Row, null);
		}

        protected virtual void _(Events.RowInserting<AMClockItem> e)
        {
            if (e.Row == null)
            {
                return;
            }

			if (IsMobile)
			{
            var existing = (AMClockItem)PXSelect<
                AMClockItem, 
                Where<AMClockItem.employeeID, Equal<Required<AMClockItem.employeeID>>>>
                .Select(this, e.Row.EmployeeID);
            if (existing == null)
                return;

            header.Current = existing;
            e.Cancel = true;
        }

			if (e.Row.EmployeeID == null && prodsetup.Current.RestrictClockCurrentUser == true)
				{
					EPEmployee emp = PXSelect<EPEmployee, Where<EPEmployee.userID, Equal<Required<EPEmployee.userID>>>>.Select(this, Accessinfo.UserID);
					if (emp != null)
				{
						e.Row.EmployeeID = emp.BAccountID;
					var existing = (AMClockItem)PXSelect<
						AMClockItem,
						Where<AMClockItem.employeeID, Equal<Required<AMClockItem.employeeID>>>>
						.Select(this, e.Row.EmployeeID);
					if (existing != null)
					{
						header.Current = existing;
						e.Cancel = true;
				}
			}

        }

		}


        protected virtual void _(Events.RowUpdated<AMClockItem> e)
        {
            if (prodsetup.Current.RestrictClockCurrentUser != true || e.Row == null)
            {
                return;
            }
            EPEmployee emp = PXSelect<EPEmployee, Where<EPEmployee.bAccountID, Equal<Required<EPEmployee.bAccountID>>>>.Select(this, e.Row.EmployeeID);
            if (emp == null)
                return;

            if (emp.UserID != Accessinfo.UserID)
            {
                e.Cache.RaiseExceptionHandling<AMClockItem.employeeID>(e.Row, null, new PXSetPropertyException(Messages.EmployeeNotCurrentUser, PXErrorLevel.Error));
            }
        }

		protected virtual void _(Events.FieldVerifying<AMClockItem.startTime> e)
		{
			if (e.Row == null || e.NewValue == null)
				return;

			DateTime starttime = (DateTime)e.NewValue;
			if (starttime.Year < 2000)
			{
				var row = (AMClockItem)e.Row;
				var trandate = row.TranDate.GetValueOrDefault();
				e.NewValue = new DateTime(trandate.Year, trandate.Month, trandate.Day, starttime.Hour, starttime.Minute, starttime.Second);
			}
		}

        #endregion

        #region Buttons 
        [PXUIField(DisplayName = "Clock")]
        [PXButton]
        public virtual IEnumerable ClockInOut(PXAdapter adapter)
        {
            try
            {
				var item = header.Current;

				if (item == null)
                {
                    return adapter.Get();
                }

				//make sure record doesn't have invalid times
				if (item.StartTime != null && item.EndTime != null && item.StartTime >= item.EndTime)
				{
					item.EndTime = null;
				}

                if (item.IsClockedIn == true)
                {
                    ClockOut(item);
                }
                else
                {
					var prodItem = AMProdItem.PK.Find(this, item.OrderType, item.ProdOrdID);
					if ((prodItem == null && !string.IsNullOrWhiteSpace(item.OrderType)
						&& !string.IsNullOrWhiteSpace(item.ProdOrdID)) || (
                        prodItem.Canceled == true ||
                        prodItem.Closed == true ||
                        prodItem.Hold == true ||
						prodItem.Locked == true))
					{
						throw new PXException(
							Messages.ProdStatusInvalidForProcess,
							item.OrderType,
							item.ProdOrdID,
							ProductionOrderStatus.GetStatusDescription(
								prodItem.Hold == true ? ProductionOrderStatus.Hold : prodItem.StatusID));
					}
                    ClockIn(item);
                }
                Actions.PressSave();
                return Actions["Cancel"].Press(adapter);
            }
            catch (Exception e)
            {
                PXTraceHelper.PxTraceException(e);
                throw;
            }
        }

        public PXAction<AMClockItem> fillCurrentUser;
        [PXUIField(DisplayName = "Current User")]
        [PXButton]        
        public virtual IEnumerable FillCurrentUser(PXAdapter adapter)
        {
            var emp = (EPEmployee)PXSelect <EPEmployee,
                    Where<EPEmployee.userID, Equal<Current<AccessInfo.userID>>>>.Select(this);
			if (emp == null)
			return adapter.Get();

			header.Cache.Clear();

			return header.Select(emp.BAccountID);
        }

		#endregion

		#region Methods


		protected virtual void ClockOut(AMClockItem item)
        {
            item.EndTime = Common.Now();
            //if the starttime has an invalid date but correct time, use the trandate instead
            var starttime = item.StartTime.GetValueOrDefault();
			if (starttime.Year < 2000)
			{
				var trandate = item.TranDate.GetValueOrDefault();
				item.StartTime = new DateTime(trandate.Year, trandate.Month, trandate.Day, starttime.Hour, starttime.Minute, starttime.Second);
			}

            int laborTime = AMDateInfo.GetDateMinutes(AMDateInfo.RemoveSeconds(item.StartTime.GetValueOrDefault()), AMDateInfo.RemoveSeconds(item.EndTime.GetValueOrDefault()));
			if (laborTime > 0)
            {
                var tran = (AMClockTran)transactions.Insert();
                if(tran == null)
                {
                    PXTrace.WriteInformation(Messages.GetLocal(Messages.UnableToInsertDAC, typeof(AMClockTran).Name));
                    return;
                }
				//verify the prod order locations have not changed since clock in
				//also fix the inventoryID if it is incorrect
				var proditem = AMProdItem.PK.Find(this, item.OrderType, item.ProdOrdID);
				if(proditem != null)
				{
					item.SiteID = proditem.SiteID;
					item.LocationID = proditem.LocationID;
					item.InventoryID = proditem.InventoryID;
				}
                CopyTran(item, tran);
				tran.LaborTime = laborTime;
				tran.LaborTimeSeconds = AMDateInfo.GetDateSeconds(tran.StartTime.GetValueOrDefault(), tran.EndTime.GetValueOrDefault());
				transactions.Update(tran);
                foreach (AMClockItemSplit split in PXParentAttribute.SelectChildren(splits.Cache, item, typeof(AMClockItem)))
                {
                    var newSplit = (AMClockTranSplit)transplits.Insert();
                    if (newSplit == null)
                    {
                        PXTrace.WriteInformation(Messages.GetLocal(Messages.UnableToInsertDAC, typeof(AMClockTranSplit).Name));
                        continue;
                    }
                    CopySplit(split, newSplit);
                    newSplit.Released = true;
					transplits.Update(newSplit);
                }
            }
            ClearHeader(item);
		}

		protected virtual void ClockIn(AMClockItem item)
        {
			item.StartTime = Common.Now();
			header.Cache.SetDefaultExt<AMClockItem.tranDate>(item);
            header.Cache.Update(item);


        }

        protected virtual void ClearHeader(AMClockItem item)
        {
            foreach (AMClockItemSplit split in PXParentAttribute.SelectChildren(splits.Cache, item, typeof(AMClockItem)))
            {
                var status = splits.Cache.GetStatus(split);
                if (status == PXEntryStatus.Inserted)
					splits.Cache.PersistInserted(split);                
                    splits.Cache.Delete(split);
            }

			var clockItem = header.Cache.LocateElse(item);
			clockItem.Qty = 0m;
			clockItem.InvtMult = 0;
			clockItem.UnassignedQty = 0m;
			clockItem.StartTime = null;
			clockItem.EndTime = null;
			clockItem.LotSerialNbr = null;
			header.Update(clockItem);
		}

        protected virtual void CopyTran(AMClockItem item, AMClockTran tran)
        {
            tran.EmployeeID = item?.EmployeeID;
            tran.OrderType = item?.OrderType;
            tran.ProdOrdID = item?.ProdOrdID;
            tran.OperationID = item?.OperationID;
            tran.ShiftCD = item?.ShiftCD;
            tran.Qty = item?.Qty;
            tran.BaseQty = item?.BaseQty;
            tran.UOM = item?.UOM;
            tran.InventoryID = item?.InventoryID;
            tran.SiteID = item?.SiteID;
            tran.StartTime = item?.StartTime;
            tran.EndTime = item?.EndTime;
            tran.LocationID = item?.LocationID;
            tran.InvtMult = item?.InvtMult;
            tran.LastOper = item?.LastOper;
            tran.TranDate = item?.TranDate;
            tran.ExpireDate = item?.ExpireDate;
        }

        protected virtual void CopySplit(AMClockItemSplit split, AMClockTranSplit newSplit)
        {
            newSplit.Qty = split?.Qty;
            newSplit.LotSerialNbr = split?.LotSerialNbr;
            newSplit.LocationID = split?.LocationID;
            newSplit.InventoryID = split?.InventoryID;
            newSplit.TranType = split?.TranType;
            newSplit.UOM = split?.UOM;
            newSplit.SiteID = split?.SiteID;
            newSplit.SubItemID = split?.SubItemID;
            newSplit.InvtMult = split?.InvtMult;
            newSplit.EmployeeID = split?.EmployeeID;
            newSplit.TranDate = split?.TranDate;
            newSplit.ExpireDate = split?.ExpireDate;
        }

        protected virtual void EnableFields(bool clockedIn, bool prodEmployee)
        {
            LineSplittingExt.showSplits.SetEnabled(clockedIn && header.Current.Qty > 0 && header.Current.LastOper == true && prodEmployee);
            PXUIFieldAttribute.SetEnabled<AMClockItem.employeeID>(header.Cache, null, prodsetup.Current.RestrictClockCurrentUser == false || IsMobile);
            PXUIFieldAttribute.SetEnabled<AMClockItem.orderType>(header.Cache, null, !clockedIn && prodEmployee);
            PXUIFieldAttribute.SetEnabled<AMClockItem.prodOrdID>(header.Cache, null, !clockedIn && prodEmployee);
            PXUIFieldAttribute.SetEnabled<AMClockItem.operationID>(header.Cache, null, !clockedIn && prodEmployee);
            PXUIFieldAttribute.SetEnabled<AMClockItem.shiftCD>(header.Cache, null, !clockedIn && prodEmployee);
            PXUIFieldAttribute.SetEnabled<AMClockItem.qty>(header.Cache, null, clockedIn && prodEmployee);
            this.clockInOut.SetCaption(clockedIn ? Messages.ClockOut : Messages.ClockIn);
            clockInOut.SetEnabled(prodEmployee);
            Save.SetEnabled(prodEmployee);
        }

        protected virtual void SetDefaultShift(PXCache sender, AMClockItem row)
        {
            if (row?.OperationID == null || row.ProdOrdID == null)
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

            sender.SetValueExt<AMClockItem.shiftCD>(row, ((AMShift)result[0])?.ShiftCD);
        }

        protected virtual bool CheckProdEmployee(PXCache sender, AMClockItem row)
        {
            EPEmployee emp = PXSelect<EPEmployee, Where<EPEmployee.bAccountID, Equal<Required<EPEmployee.bAccountID>>>>.Select(this, row.EmployeeID);
            if (emp == null)
            {
                return false;
            }
            var ext = emp.GetExtension<EPEmployeeExt>();
            if (ext == null)
            {
                return false;
            }

            if(ext.AMProductionEmployee == false)
            {
                sender.RaiseExceptionHandling<AMClockItem.employeeID>(row, null, new PXSetPropertyException(Messages.EmployeeNotProduction, PXErrorLevel.Error));
            }
            return ext.AMProductionEmployee == true;
        }

        #endregion
    }
}
