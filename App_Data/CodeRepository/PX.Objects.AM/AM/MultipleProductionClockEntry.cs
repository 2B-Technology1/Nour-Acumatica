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

using System.Collections;
using System.Collections.Generic;
using System;
using PX.Data;
using PX.Objects.AM.Attributes;
using PX.Objects.EP;
using PX.Objects.IN;
using System.Linq;
using PX.Data.BQL.Fluent;
using PX.Objects.GL;

namespace PX.Objects.AM
{
    public class MultipleProductionClockEntry : PXGraph<MultipleProductionClockEntry, AMClockItem>
    {
        #region Dataviews
        public PXSelect<AMClockItem, Where<AMClockItem.employeeID, Equal<Optional<AMClockItem.employeeID>>>> Document;
		public SelectFrom<AMClockTran>
			.InnerJoin<AMProdItem>.On<AMClockTran.orderType.IsEqual<AMProdItem.orderType>.And<AMClockTran.prodOrdID.IsEqual<AMProdItem.prodOrdID>>>
			.InnerJoin<Branch>.On<AMProdItem.branchID.IsEqual<Branch.branchID>>
			.Where<AMClockTran.employeeID.IsEqual<AMClockItem.employeeID.FromCurrent>.And<Branch.baseCuryID.IsEqual<AccessInfo.baseCuryID.FromCurrent>>>
			.OrderBy<Desc<AMClockTran.tranDate>, Desc<AMClockTran.startTime>>.View Transactions;

		public PXSelect<AMClockTranSplit, Where<AMClockTranSplit.employeeID, Equal<Current<AMClockTran.employeeID>>,
            And<AMClockTranSplit.lineNbr, Equal<Current<AMClockTran.lineNbr>>>>> Splits;

        public PXAction<AMClockItem> clockInOut;
        public PXSetup<AMPSetup> prodsetup;

        [PXHidden]
        public PXSelect<AMWC> WorkCenters;

        [PXHidden]
        public PXSelect<AMProdItem> prod;

        [PXFilterable]
        public PXSelectJoin<AMProdOper,
            InnerJoin<AMProdItem, On<AMProdItem.orderType, Equal<AMProdOper.orderType>,
                 And<AMProdItem.prodOrdID, Equal<AMProdOper.prodOrdID>>>,
                    InnerJoin<AMWC, On<AMWC.wcID, Equal<AMProdOper.wcID>>,
                    LeftJoin<ClockedInByOperation,
                        On<ClockedInByOperation.orderType, Equal<AMProdOper.orderType>,
                        And<ClockedInByOperation.orderType, Equal<AMProdOper.orderType>,
                        And<ClockedInByOperation.prodOrdID, Equal<AMProdOper.prodOrdID>,
                        And<ClockedInByOperation.operationID, Equal<AMProdOper.operationID>,
                        And<ClockedInByOperation.employeeID, Equal<Current<AMClockItem.employeeID>>>>>>>,
				InnerJoin<Branch, On<AMProdItem.branchID, Equal<Branch.branchID>>>>>>,
            Where<Branch.baseCuryID, Equal<Current<AccessInfo.baseCuryID>>,
            And<Where<AMProdItem.isOpen, Equal<True>>>>,
                OrderBy<Asc<AMProdOper.wcID, Asc<AMProdOper.startDate,
                    Asc<AMProdItem.schPriority>>>>> Operations;
        
        public AMClockTranLineSplittingMultipleProductionExtension LineSplittingExt => FindImplementation<AMClockTranLineSplittingMultipleProductionExtension>();

		[InjectDependency]
		public ICommon Common
		{
			get;
			set;
		}
		#endregion

		#region Constructor 
		public MultipleProductionClockEntry()
        {
            Operations.AllowInsert = true;
            Operations.AllowUpdate = true;
            Operations.AllowDelete = false;

            PXUIFieldAttribute.SetVisible<AMProdItem.ordLineRef>(prod.Cache, null, false);
            PXUIFieldAttribute.SetVisible<AMProdItem.branchID>(prod.Cache, null, true);

            PXUIFieldAttribute.SetEnabled(Operations.Cache, null, false);
            PXUIFieldAttribute.SetEnabled<AMProdOper.selected>(Operations.Cache, null, true);
			PXUIFieldAttribute.SetEnabled<AMClockItem.employeeID>(Document.Cache, null, prodsetup.Current.RestrictClockCurrentUser == false || IsMobile);
		}
        #endregion
        #region Button

        public PXAction<AMClockItem> clockEntriesClockIn;

        [PXUIField(DisplayName = "Clock In", Enabled = true, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Insert)]
        [PXButton(CommitChanges = true)]
        public virtual IEnumerable ClockEntriesClockIn(PXAdapter adapter)
        {

			List<AMClockTran> clockTrans = Transactions.Select<AMClockTran>()
				.Where(t => t.Selected == true).ToList();

			if (clockTrans.Count > 0)
            {
                ClockIn(clockTrans);
            }

            Save.Press();
            return adapter.Get();
        }

        public PXAction<AMClockItem> operationsClockIn;

        [PXUIField(DisplayName = "Clock In", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Insert)]
        [PXButton(CommitChanges = true)]
        public virtual IEnumerable OperationsClockIn(PXAdapter adapter)
        {
            List<AMProdOper> prodOpers = new List<AMProdOper>();
            foreach (AMProdOper prodOper in this.Operations.Select().RowCast<AMProdOper>().AsEnumerable()
                .Where(t => t.Selected == true))
            {
                    prodOpers.Add(prodOper);
            }

            if (prodOpers.Count > 0)
            {
                ClockIn(prodOpers);
            }

            Save.Press();

            return adapter.Get();
        }

        public PXAction<AMClockItem> clockEntriesClockOut;

        [PXUIField(DisplayName = "Clock Out", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Update)]
        [PXButton(CommitChanges = true)]
        public virtual IEnumerable ClockEntriesClockOut(PXAdapter adapter)
        {
            List<AMClockTran> clockTrans = new List<AMClockTran>();
            foreach (AMClockTran clockTran in this.Transactions.Select().RowCast<AMClockTran>().AsEnumerable()
                .Where(t => t.Selected == true))
            {
                    clockTrans.Add(clockTran);
            }

            if (clockTrans.Count > 0)
            {
                ClockOut(clockTrans);
            }

            Save.Press();
            return adapter.Get();
        }

		public PXAction<AMClockItem> fillCurrentUser;
		[PXUIField(DisplayName = "Current User")]
		[PXButton]
		public virtual IEnumerable FillCurrentUser(PXAdapter adapter)
		{
			var emp = (EPEmployee)PXSelect<EPEmployee,
					Where<EPEmployee.userID, Equal<Current<AccessInfo.userID>>>>.Select(this);
			if (emp == null)
				return adapter.Get();

			Document.Cache.Clear();

			return Document.Select(emp.BAccountID);
		}
		#endregion

		#region cache attached
		[PXMergeAttributes(Method = MergeMethod.Replace)]
        [AMOrderTypeField(PersistingCheck = PXPersistingCheck.Nothing, Required = false)]
        protected virtual void _(Events.CacheAttached<AMClockItem.orderType> e) { }

        [PXMergeAttributes(Method = MergeMethod.Replace)]
        [ProductionNbr(PersistingCheck = PXPersistingCheck.Nothing, Required = false)]
        protected virtual void _(Events.CacheAttached<AMClockItem.prodOrdID> e) { }

        [PXMergeAttributes(Method = MergeMethod.Replace)]
        [OperationIDField(PersistingCheck = PXPersistingCheck.Nothing, Required = false)]
        protected virtual void _(Events.CacheAttached<AMClockItem.operationID> e) { }

        [PXMergeAttributes(Method = MergeMethod.Replace)]
        [PXDBInt]
        [PXUIField(DisplayName = "Inventory ID")]
        protected virtual void _(Events.CacheAttached<AMClockItem.inventoryID> e) { }

        [PXMergeAttributes(Method = MergeMethod.Replace)]
        [PXDBInt]
        [PXUIField(DisplayName = "Subitem")]
        protected virtual void _(Events.CacheAttached<AMClockItem.subItemID> e) { }

        [PXMergeAttributes(Method = MergeMethod.Replace)]
        [PXDBInt]
        [PXUIField(DisplayName = "Warehouse")]
        protected virtual void _(Events.CacheAttached<AMClockItem.siteID> e) { }

        [PXMergeAttributes(Method = MergeMethod.Replace)]
        [PXDBInt]
        [PXUIField(DisplayName = "Location")]
        protected virtual void _(Events.CacheAttached<AMClockItem.locationID> e) { }

        [PXMergeAttributes(Method = MergeMethod.Replace)]
        [PXDBString(6, IsUnicode = true, InputMask = ">aaaaaa")]
        [PXUIField(DisplayName = "UOM")]
        protected virtual void _(Events.CacheAttached<AMClockItem.uOM> e) { }

        [PXMergeAttributes(Method = MergeMethod.Merge)]
        protected virtual void _(Events.CacheAttached<AMClockItem.laborTime> e) { }

        [PXMergeAttributes(Method = MergeMethod.Replace)]
        [PXDBString(INLotSerialStatusByCostCenter.lotSerialNbr.Length, IsUnicode = true, InputMask = "")]
        [PXUIField(FieldClass = "LotSerial")]
        protected virtual void _(Events.CacheAttached<AMClockItem.lotSerialNbr> e) { }

        [PXMergeAttributes(Method = MergeMethod.Merge)]
        [PXUIField(DisplayName = "Multiple Entries Allowed")]
        protected virtual void _(Events.CacheAttached<AMWC.allowMultiClockEntry> e) { }

		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXUIField(DisplayName = "Order Status")]
		protected virtual void _(Events.CacheAttached<AMProdItem.statusID> e) { }

		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXUIField(DisplayName = "Operation Status")]
		protected virtual void _(Events.CacheAttached<AMProdOper.statusID> e) { }

		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[ProductionNbr(IsKey = true, Visible = true, Enabled = false)]
		protected virtual void _(Events.CacheAttached<AMProdOper.prodOrdID> e) { }

		#endregion

		#region events
		public bool internalDelete;
        protected virtual AMClockTran CreateNewCopyFrom(AMClockTran sourceClockTran)
        {
            var clockTran = new AMClockTran
            {
                OrderType = sourceClockTran.OrderType,
                ProdOrdID = sourceClockTran.ProdOrdID,
                OperationID = sourceClockTran.OperationID
            };

			clockTran.WcID = sourceClockTran.WcID;
			clockTran.Status = ClockTranStatus.NewStatus;

            return clockTran;
        }

        protected virtual void ClockIn(List<AMClockTran> clockTrans)
        {
            int? employeeID = Document.Current.EmployeeID;
            PXResultset<AMClockTran> activeResults = new PXResultset<AMClockTran>();
            List<AMClockTran> activeClockTrans = new List<AMClockTran>();
            List<AMClockTran> newClockTrans = new List<AMClockTran>();
            activeResults = GetActiveClockIns(employeeID);
            DateTime startDateTime = Common.Now();

			if (!IsValidForMultipleClockEntries(Transactions.Select<AMClockTran>().ToList(),
				Transactions.Select<AMClockTran>().Where(w => w.Status == ClockTranStatus.ClockedIn).ToList(), out AMClockTran tran))
			{
				Transactions.Cache.RaiseExceptionHandling<AMClockTran.wcID>(tran, tran.WcID,
					new PXSetPropertyException(Messages.GetLocal(Messages.WorkCenterDoesNotSupportMultipleProductionOrders),
					tran.WcID, tran.OrderType, tran.ProdOrdID, PXErrorLevel.Error));
			}
			else
			{
				foreach (AMClockTran clockTran in activeResults)
				{
					activeClockTrans.Add(clockTran);
				}

				foreach (AMClockTran clockTran in clockTrans)
				{
					bool valid = IsValidClockIn(clockTran, activeClockTrans, Transactions.Cache);
					if (!valid)
					{
						return;
					}
					if (clockTran.Status == ClockTranStatus.ClockedOut)
					{
						newClockTrans.Add(CreateNewCopyFrom(clockTran));
					}
					clockTran.Selected = false;

					if (clockTran.Status == ClockTranStatus.NewStatus)
					{
						clockTran.Status = ClockTranStatus.ClockedIn;
						clockTran.StartTime = startDateTime;
						clockTran.TranDate = Common.Today();
					}
					Transactions.Update(clockTran);
				}

				foreach (AMClockTran clockTran in newClockTrans)
				{
					ClockIn(clockTran, startDateTime);
				}
			}
        }

        protected void ClockIn(AMClockTran clockTran, DateTime startDateTime)
        {
            clockTran.Selected = false;
            clockTran.StartTime = startDateTime;
            clockTran.TranDate = Common.Today();
			clockTran.Status = ClockTranStatus.ClockedIn;
            
            Transactions.Cache.Insert(clockTran);
        }

        protected virtual void ClockOut(List<AMClockTran> clockTrans)
        {
            bool isValid = true;
            foreach (AMClockTran clockTran in clockTrans)
            {
                bool valid = IsValidClockOut(clockTran);
                if(!valid)
                {
                    isValid = false;
                }
                
            }
            if (!isValid)
            {
                return;
            }
            PXResultset<AMClockTran> overlappedClockTrans = new PXResultset<AMClockTran>();
            List<AMClockTran> clockTransAll = new List<AMClockTran>();
            List<AMClockTran> deletedClockTran = new List<AMClockTran>();
            DateTime endDateTime = Common.Now();
            DateTime startDateTime = new DateTime();
            DateTime emptyDate = new DateTime();
            int? EmployeeID = 0;
            var isSingle = false;
            if (clockTrans.Count == 1)
            {
                var singleClockTran = clockTrans[0];
                if (singleClockTran.AllowMultiClockEntry == false)
                {
                    isSingle = true;
                    singleClockTran.EndTime = endDateTime;
					singleClockTran.LaborTime = AMDateInfo.GetDateMinutes(AMDateInfo.RemoveSeconds(singleClockTran.StartTime.GetValueOrDefault()), AMDateInfo.RemoveSeconds(singleClockTran.EndTime.GetValueOrDefault()));
					singleClockTran.LaborTimeSeconds = AMDateInfo.GetDateSeconds(singleClockTran.StartTime.GetValueOrDefault(),singleClockTran.EndTime.GetValueOrDefault());

                    if (singleClockTran.LaborTime < 1)
                    {
                        AMProdOper prodOper = PXSelect<AMProdOper,
							Where<AMProdOper.orderType, Equal<Required<AMProdOper.orderType>>,
								And<AMProdOper.prodOrdID, Equal<Required<AMProdOper.prodOrdID>>,
								And<AMProdOper.operationID, Equal<Required<AMProdOper.operationID>>>>>>
								.Select(this, singleClockTran.OrderType, singleClockTran.ProdOrdID, singleClockTran.OperationID);

                        DateTime? endTime = singleClockTran.EndTime;
                        int? laborSeconds = singleClockTran.LaborTimeSeconds;

                        singleClockTran.EndTime = null;
                        singleClockTran.LaborTime = 0;
						singleClockTran.LaborTimeSeconds = 0;

                        WebDialogResult result = Document.View.Ask(
							Messages.GetLocal(Messages.CalculatedLaborTimeLessThan1Minute, prodOper.OperationCD, singleClockTran.OrderType, singleClockTran.ProdOrdID), MessageButtons.YesNo);
                        if (result == WebDialogResult.Yes)
                        {
                            singleClockTran.LaborTime = 1;
							singleClockTran.LaborTimeSeconds = laborSeconds;
                            singleClockTran.EndTime = endTime;
                        }
                        else
                        {
                            internalDelete = true;
                            Transactions.Delete(singleClockTran);
                            internalDelete = false;
                            return;
                        }
                    }

                    ClockOut(singleClockTran);
                }
            }

            if (isSingle != true)
            {
                foreach (AMClockTran clocktran in clockTrans)
                {
                    EmployeeID = clocktran.EmployeeID;
                    if (startDateTime == emptyDate || clocktran.StartTime < startDateTime)
                    {
                        startDateTime = (DateTime)clocktran.StartTime;
                    }
                    if (clocktran.EndTime == null)
                    {
                        clocktran.EndTime = endDateTime;
                    }
                }

                overlappedClockTrans = GetOverlappingClockedOut(startDateTime, endDateTime, EmployeeID);
                foreach (AMClockTran clocktran in overlappedClockTrans)
                {
                    clockTransAll.Add(PXCache<AMClockTran>.CreateCopy(clocktran));
                }

                var calculatedClockTrans = CalculateOverlapClockOutLaborTime(clockTransAll, endDateTime);
                
                foreach(AMClockTran clockTran in clockTrans)
                {
                    clockTran.EndTime = null;
                }

                foreach (AMClockTran clockTran in clockTrans)
                {
                    foreach (AMClockTran calculatedClockTran in calculatedClockTrans)
                    {
                        if(clockTran.EmployeeID == calculatedClockTran.EmployeeID && clockTran.LineNbr == calculatedClockTran.LineNbr)
                        {
                   
                            clockTran.LaborTime = calculatedClockTran.LaborTime;
							clockTran.LaborTimeSeconds = calculatedClockTran.LaborTimeSeconds;
                        }
                    }
                    
                    if (clockTran.Status == ClockTranStatus.ClockedOut)
                    {
                        //clockTran.Selected = false;
                        //Transactions.Update(clockTran);
                    }
                    else
                    {
                        if (clockTran.LaborTime < 1)
                        {
                            AMProdOper prodOper = PXSelect<AMProdOper,
                Where<AMProdOper.orderType, Equal<Required<AMProdOper.orderType>>,
                    And<AMProdOper.prodOrdID, Equal<Required<AMProdOper.prodOrdID>>,
                    And <AMProdOper.operationID, Equal <Required<AMProdOper.operationID>>>>>>.Select(this, clockTran.OrderType, clockTran.ProdOrdID, clockTran.OperationID);

                            int? laborTime = clockTran.LaborTime;
                            int? laborSeconds = clockTran.LaborTimeSeconds;

                            clockTran.EndTime = null;
                            clockTran.LaborTime = 0;
							clockTran.LaborTimeSeconds = 0;

                            WebDialogResult result = Document.View.Ask(
                        Messages.GetLocal(Messages.CalculatedLaborTimeLessThan1Minute, prodOper.OperationCD, clockTran.OrderType, clockTran.ProdOrdID), MessageButtons.YesNo);
                            if (result == WebDialogResult.Yes)
                            {
                                clockTran.LaborTime = 1;
								clockTran.LaborTimeSeconds = laborSeconds;
                            }
                            else 
                                {
                                internalDelete = true;
                                Transactions.Delete(clockTran);
                                internalDelete = false;
                                continue;
                            }


                        }
                        
                        clockTran.EndTime = endDateTime;
                        ClockOut(clockTran);
                    }
                }
            }
        }

        protected virtual AMClockTran ClockOut(AMClockTran clockTran)
        {
            var proditem = AMProdItem.PK.Find(this, clockTran.OrderType, clockTran.ProdOrdID);
            if (proditem != null)
            {
                clockTran.SiteID = proditem.SiteID;
                clockTran.SiteID = proditem.SiteID;
                clockTran.LocationID = proditem.LocationID;
                clockTran.Selected = false;
            }
			clockTran.Status = ClockTranStatus.ClockedOut;
            return Transactions.Update(clockTran);
        }
        protected virtual PXResultset<AMClockTran> GetOverlappingClockedOut(DateTime startTime, DateTime endTime, int? employeeID)
        {
                return PXSelect <AMClockTran, Where<AMClockTran.employeeID, Equal<Required<AMClockTran.employeeID>>,
                And<Where2<Where<AMClockTran.startTime, Between<Required<AMClockTran.startTime>, Required<AMClockTran.endTime>>>,
                Or<Where<AMClockTran.endTime, Between<Required<AMClockTran.startTime>, Required<AMClockTran.endTime>>,
                Or<Where<AMClockTran.startTime, LessEqual<Required<AMClockTran.startTime>>,
                And<Where2<Where<AMClockTran.endTime, GreaterEqual<Required<AMClockTran.endTime>>>, 
                Or<AMClockTran.endTime, IsNull>>>>>>>>>>>.Select(this,employeeID, startTime, endTime, startTime, endTime, startTime, endTime);
        }

        protected virtual IEnumerable<AMClockTran> CalculateOverlapClockOutLaborTime(List<AMClockTran> clockTrans, DateTime clockOutDateTime)
        {
            double laborSeconds = 0;
            DateTime startDateTime = new DateTime();
            DateTime previousEndTime = new DateTime();

            int OverlapCount = 0;

			List<DateOverlap> dateOverLaps = GetDateOverlaps(clockTrans, clockOutDateTime);           

            foreach (AMClockTran clockTran in clockTrans.OrderBy(i => i.LineNbr))
            {
				clockTran.LaborTimeSeconds = 0;
                OverlapCount = 0;
                bool ValidForLine = false;
                bool First = true;
				if (clockTran.LaborTime != 0 && clockTran.LaborTime != null)
				{
					continue;
				}

				foreach (DateOverlap dateOverlap in dateOverLaps.OrderBy(i => i.StartTime))
                {
                    if (First == true)
                    {
                        startDateTime = (DateTime)dateOverlap.StartTime;
                        First = false;
                    }

                    if (dateOverlap.StartTime == startDateTime)
                    {
                        OverlapCount++;
                        previousEndTime = (DateTime)dateOverlap.EndTime;
                        if(clockTran.LineNbr == dateOverlap.LineNbr)
                        {
                            ValidForLine = true;
                        }
                    }
                    else
                    {
                        if (ValidForLine)
                        {
                            if (OverlapCount != 0)
                            {
                                laborSeconds = (previousEndTime - startDateTime).TotalSeconds / OverlapCount;
								clockTran.LaborTimeSeconds += (int?)laborSeconds;
                            }                            
                            ValidForLine = false;
                            if (clockTran.LineNbr == dateOverlap.LineNbr)
                            {
                                ValidForLine = true;
                            }
                        }
						OverlapCount = 1;
                        startDateTime = (DateTime)dateOverlap.StartTime;
                        previousEndTime = (DateTime)dateOverlap.EndTime;
                    }
                }
                if (ValidForLine)
                {
                    if (OverlapCount != 0)
                    {
                        laborSeconds = (previousEndTime - startDateTime).TotalSeconds / OverlapCount;
						clockTran.LaborTimeSeconds += (int?)laborSeconds;
                    }
                    OverlapCount = 1;
                    ValidForLine = false;
                }

				clockTran.LaborTime = AMDateInfo.SecondsToMinutes(clockTran.LaborTimeSeconds.GetValueOrDefault());
            }

            return clockTrans;
        }

		protected virtual List<DateOverlap> GetDateOverlaps(List<AMClockTran> clockTrans, DateTime clockOutDateTime)
		{
			DateTime startDateTime = new DateTime();
			DateTime? previousEndTime = new DateTime();
			DateTime overlapppedEndTime = new DateTime();
			List<DateOverlap> dateOverLaps = new List<DateOverlap>();
			foreach (AMClockTran clockTran in clockTrans.OrderBy(i => i.StartTime))
			{
				if (clockTran.EndTime == null)
				{
					overlapppedEndTime = clockOutDateTime;
				}
				else
				{
					overlapppedEndTime = (DateTime)clockTran.EndTime;					
				}
				previousEndTime = overlapppedEndTime;
				startDateTime = (DateTime)clockTran.StartTime;

				//first, for a given clocktran we compare to the start times of the other clocktrans
				foreach (AMClockTran clockTranOverlapped in clockTrans.OrderBy(i => i.StartTime))
				{
					//if the given clocktran doesn't overlap the current clocktran, skip it
					if (clockTranOverlapped.StartTime >= clockTran.EndTime || clockTran.StartTime >= clockTranOverlapped.EndTime)
					{
						continue;
					}
					//if the start times are the same (usually they're the same record) skip it but move the previous end time up
					if (clockTran.StartTime == clockTranOverlapped.StartTime)
					{
						previousEndTime = clockTranOverlapped.EndTime;
						continue;
					}
					//two possible inserts here, first if the previous end time is before the current record start time, insert an overlap for the previous start/end segement
					//then move the previous start up to the previous end
					if (previousEndTime < clockTranOverlapped.StartTime)
					{
						var dateOverlap = new DateOverlap();
						dateOverlap.LineNbr = clockTran.LineNbr;
						dateOverlap.StartTime = startDateTime;
						dateOverlap.EndTime = startDateTime = (DateTime)previousEndTime;
						dateOverLaps.Add(dateOverlap);
					}
					//if the previous start is before the current record start time, insert an overlap for the segment between previous start and current record start 
					if (startDateTime < clockTranOverlapped.StartTime)
					{
						var dateOverlap = new DateOverlap();
						dateOverlap.LineNbr = clockTran.LineNbr;
						dateOverlap.StartTime = startDateTime;
						dateOverlap.EndTime = startDateTime = (DateTime)clockTranOverlapped.StartTime;
						dateOverLaps.Add(dateOverlap);
					}

					previousEndTime = clockTranOverlapped.EndTime;


				}
				//second, for a given clock tran we compare to the end times of the other clocktrans, the endtimes are sorted chronologically with null values at the end
				foreach (AMClockTran clockTranOverlapped in clockTrans.OrderByDescending(i => i.EndTime.HasValue).ThenBy(i => i.EndTime))
				{
					//for a given clocktran, only consider clocktrans that ended after the previous start time, and the given clocktran itself because endtime is null
					if (clockTranOverlapped.EndTime > startDateTime || clockTranOverlapped.EndTime == null)
					{
						//two possible inserts here, if the clock tran ended before the record you're clocking out of, insert an overlap for a segment for that ended clocktran
						//move the previous start time up to that segment's end time
						if (clockTranOverlapped.EndTime < overlapppedEndTime)
						{
							var dateOverlap = new DateOverlap();
							dateOverlap = new DateOverlap();
							dateOverlap.LineNbr = clockTran.LineNbr;
							dateOverlap.StartTime = startDateTime;
							dateOverlap.EndTime = startDateTime = (DateTime)clockTranOverlapped.EndTime;
							dateOverLaps.Add(dateOverlap);
						}
						//this is the insert for the given clocktran from the main loop, insert an overlap for the segment from the previous start time to the clock out time
						if (clockTranOverlapped.LineNbr == clockTran.LineNbr && overlapppedEndTime > startDateTime)
						{
							var dateOverlap = new DateOverlap();
							dateOverlap.LineNbr = clockTran.LineNbr;
							dateOverlap.StartTime = startDateTime;
							dateOverlap.EndTime = startDateTime = overlapppedEndTime;
							dateOverLaps.Add(dateOverlap);
						}
					}
				}

			}

			return dateOverLaps;
		}

        protected virtual bool IsValidClockOut(AMClockTran clockTran)
        {
            bool isValid = true;
    
            if (clockTran.Status != ClockTranStatus.ClockedIn)
            {
                Transactions.Cache.RaiseExceptionHandling<AMClockTran.prodOrdID>(clockTran, clockTran.ProdOrdID, new PXSetPropertyException(Messages.GetLocal(Messages.EmployeeNotClockedInOnOrder), clockTran.OrderType, clockTran.ProdOrdID, PXErrorLevel.Error));
                isValid = false;
            }

            if (clockTran.ShiftCD == null)
            {
                Transactions.Cache.RaiseExceptionHandling<AMClockTran.shiftCD>(clockTran, clockTran.ShiftCD, new PXSetPropertyException(PX.Data.ErrorMessages.FieldIsEmpty, PXErrorLevel.Error));
                isValid = false;
            }

            if(clockTran.ScrapAction == ScrapAction.WriteOff && clockTran.QtyScrapped > 0 && clockTran.ReasonCodeID == null)
            {
                Transactions.Cache.RaiseExceptionHandling<AMClockTran.reasonCodeID>(clockTran, clockTran.ReasonCodeID, new PXSetPropertyException(PX.Data.ErrorMessages.FieldIsEmpty, PXErrorLevel.Error));
                isValid = false;
            }
            return isValid;
        }

        protected virtual void ClockIn(List<AMProdOper> prodOpers)
        {
            int? employeeID = Document.Current.EmployeeID;
            PXResultset<AMClockTran> activeResults = new PXResultset<AMClockTran>();
            List<AMClockTran> activeClockTrans = new List<AMClockTran>();
            activeResults = GetActiveClockIns(employeeID);
            AMClockTran clockTran = new AMClockTran();
            List<AMClockTran> newClockTrans = new List<AMClockTran>();

			if (!IsValidForMultipleClockEntries(Operations.Select<AMProdOper>().Select(s => ConvertToClockTran(s)).ToList(),
				Transactions.Select<AMClockTran>().Where(s => s.Status == ClockTranStatus.ClockedIn).ToList(), out AMClockTran tran))
			{
				var opr = Operations.Select<AMProdOper>().First(f => f.OrderType == tran.OrderType
				&& f.ProdOrdID == tran.ProdOrdID && f.OperationID == tran.OperationID);
				Operations.Cache.RaiseExceptionHandling<AMProdOper.wcID>(opr, opr.WcID,
					new PXSetPropertyException(Messages.GetLocal(Messages.WorkCenterDoesNotSupportMultipleProductionOrders),
					opr.WcID, opr.OrderType, opr.ProdOrdID, PXErrorLevel.Error));
			}
			else
			{
				foreach (AMClockTran activeClockTran in activeResults)
				{
					activeClockTrans.Add(activeClockTran);
				}

				foreach (AMProdOper prodOper in prodOpers)
				{
					clockTran = ConvertToClockTran(prodOper);
					bool valid = IsValidClockIn(clockTran, activeClockTrans, Operations.Cache, prodOper);
					if (!valid)
					{
						return;
					}
					newClockTrans.Add(clockTran);
				}

				foreach (AMProdOper prodOper in prodOpers)
				{
					prodOper.Selected = false;
					Operations.Cache.Update(prodOper);
				}

				DateTime startDateTime = Common.Now();

				foreach (AMClockTran newClockTran in newClockTrans)
				{
					ClockIn(newClockTran, startDateTime);
				}
			}
        }

        protected virtual PXResultset<AMClockTran> GetActiveClockIns(int? EmployeeID)
        {
            return PXSelect<AMClockTran, Where<AMClockTran.employeeID, Equal<Required<AMClockTran.employeeID>>,
                And <AMClockTran.status, Equal<Required<AMClockTran.status>>>>>.Select(this, EmployeeID,ClockTranStatus.ClockedIn);
        }

        protected virtual AMClockTran ConvertToClockTran(AMProdOper prodOper)
        {
            var clockTran = new AMClockTran
            {
                OrderType = prodOper.OrderType,
                ProdOrdID = prodOper.ProdOrdID,
                OperationID = prodOper.OperationID,
				Selected = prodOper.Selected,
				AllowMultiClockEntry = AMWC.PK.Find(this, prodOper.WcID)?.AllowMultiClockEntry,
			};

			clockTran.WcID = prodOper.WcID;
			clockTran.Status = ClockTranStatus.NewStatus;

			clockTran.BaseQtyScrapped = 0;

            return clockTran;
        }

		protected virtual bool IsValidClockIn(AMClockTran clockTran, List<AMClockTran> activeClockTrans, PXCache cache, AMProdOper row)
		{
			if (clockTran.Status == ClockTranStatus.ClockedIn)
			{
				cache.RaiseExceptionHandling<AMProdOper.prodOrdID>(row, clockTran.ProdOrdID, new PXSetPropertyException(Messages.GetLocal(Messages.DuplicateClockInProductionOrder), clockTran.OrderType, clockTran.ProdOrdID, PXErrorLevel.Error));
				return false;
			}

			var proditem = AMProdItem.PK.Find(this, clockTran.OrderType, clockTran.ProdOrdID);
			if ((proditem.IsOpen == false) || proditem.Hold == true)
			{
				cache.RaiseExceptionHandling<AMProdOper.prodOrdID>(row, clockTran.ProdOrdID, new PXSetPropertyException(Messages.GetLocal(Messages.ProdStatusInvalidForProcess), clockTran.OrderType, clockTran.ProdOrdID, ProductionOrderStatus.GetStatusDescription(proditem.StatusID.Trim()), PXErrorLevel.Error));
				return false;
			}

			foreach (AMProdOper selectedClockTran in Operations.Cache.Cached.RowCast<AMProdOper>().AsEnumerable()
				.Where(t => t.Selected == true))
			{
				if (selectedClockTran.ProdOrdID == clockTran.ProdOrdID && selectedClockTran.OrderType == clockTran.OrderType && selectedClockTran.OperationID != clockTran.OperationID)
				{
					cache.RaiseExceptionHandling<AMProdOper.prodOrdID>(row, clockTran.ProdOrdID, new PXSetPropertyException(Messages.GetLocal(Messages.DuplicateClockInProductionOrder), clockTran.OrderType, clockTran.ProdOrdID, PXErrorLevel.Error));
					return false;
				}
			}

			foreach (AMClockTran activeClockTran in activeClockTrans)
			{
				if (activeClockTran.ProdOrdID == clockTran.ProdOrdID && activeClockTran.OrderType == clockTran.OrderType)
				{
					cache.RaiseExceptionHandling<AMProdOper.prodOrdID>(row, clockTran.ProdOrdID, new PXSetPropertyException(Messages.GetLocal(Messages.DuplicateClockInProductionOrder), clockTran.OrderType, clockTran.ProdOrdID, PXErrorLevel.Error));
					return false;
				}
			}

			return true;
		}

		protected virtual bool IsValidClockIn(AMClockTran clockTran, List<AMClockTran> activeClockTrans, PXCache cache)
		{
			if (clockTran.Status == ClockTranStatus.ClockedIn)
			{
				cache.RaiseExceptionHandling<AMClockTran.prodOrdID>(clockTran, clockTran.ProdOrdID, new PXSetPropertyException(Messages.GetLocal(Messages.DuplicateClockInProductionOrder), clockTran.OrderType, clockTran.ProdOrdID, PXErrorLevel.Error));
				return false;
			}

			var proditem = AMProdItem.PK.Find(this, clockTran.OrderType, clockTran.ProdOrdID);
            if (proditem.IsOpen == false || proditem.Hold == true)
            {
                cache.RaiseExceptionHandling<AMClockTran.prodOrdID>(clockTran, clockTran.ProdOrdID, new PXSetPropertyException(Messages.GetLocal(Messages.ProdStatusInvalidForProcess), clockTran.OrderType, clockTran.ProdOrdID, ProductionOrderStatus.GetStatusDescription(proditem.StatusID.Trim()), PXErrorLevel.Error));
                return false;
            }

			foreach (AMClockTran selectedClockTran in Transactions.Cache.Cached.RowCast<AMClockTran>().AsEnumerable()
				.Where(t => t.Selected == true))
			{
				if (selectedClockTran.ProdOrdID == clockTran.ProdOrdID && selectedClockTran.OrderType == clockTran.OrderType && selectedClockTran.LineNbr != clockTran.LineNbr)
				{
					cache.RaiseExceptionHandling<AMClockTran.prodOrdID>(clockTran, clockTran.ProdOrdID, new PXSetPropertyException(Messages.GetLocal(Messages.DuplicateClockInProductionOrder), clockTran.OrderType, clockTran.ProdOrdID, PXErrorLevel.Error));
					return false;
				}
			}

			foreach (AMClockTran activeClockTran in activeClockTrans)
			{
				if (activeClockTran.ProdOrdID == clockTran.ProdOrdID && activeClockTran.OrderType == clockTran.OrderType)
				{
					cache.RaiseExceptionHandling<AMClockTran.prodOrdID>(clockTran, clockTran.ProdOrdID, new PXSetPropertyException(Messages.GetLocal(Messages.DuplicateClockInProductionOrder), clockTran.OrderType, clockTran.ProdOrdID, PXErrorLevel.Error));
					return false;
				}
			}

			return true;
		}

		[Obsolete]
		protected virtual bool IsValidClockIn(AMClockTran clockTran, List<AMClockTran> activeClockTrans, PXCache cache, object originalRow)
        {
			if (originalRow is AMProdOper)
				return IsValidClockIn(clockTran, activeClockTrans, cache, originalRow);
			else
				return IsValidClockIn(clockTran, activeClockTrans, cache);
        }

		public virtual bool IsValidForMultipleClockEntries(List<AMClockTran> records, List<AMClockTran> activeRecords, out AMClockTran clockTran)
		{
			clockTran = null;
			IEnumerable<AMClockTran> allowedRecords = records
				.Where(c => (c.Status == ClockTranStatus.ClockedIn || c.Selected.GetValueOrDefault())
					&& c.AllowMultiClockEntry.GetValueOrDefault());

			IEnumerable<AMClockTran> selectedAllowedRecords = records.Where(c => c.Selected == true && c.AllowMultiClockEntry == true && c.Status != ClockTranStatus.ClockedIn);
			IEnumerable<AMClockTran> activeAllowedRecords = activeRecords.Where(c => c.AllowMultiClockEntry == true);
			IEnumerable<AMClockTran> selectedRestrictedRecords = records.Where(c => c.Selected == true && c.AllowMultiClockEntry == false && c.Status != ClockTranStatus.ClockedIn);
			IEnumerable<AMClockTran> activeRestrictedRecords = activeRecords.Where(c => c.AllowMultiClockEntry == false);
			if (selectedRestrictedRecords.Count() > 1)
			{
				clockTran = selectedRestrictedRecords.First();
				return false;
			}
			if (activeAllowedRecords.Any() && selectedRestrictedRecords.Any())
			{
				clockTran = selectedRestrictedRecords.First();
				return false;
			}
			if (activeRestrictedRecords.Any() && selectedAllowedRecords.Any())
			{
				clockTran = selectedAllowedRecords.First();
				return false;
			}
			if (selectedRestrictedRecords.Any() && allowedRecords.Any())
			{
				clockTran = selectedRestrictedRecords.First();
				return false;
			}
			if (selectedRestrictedRecords.Any() && activeRestrictedRecords.Any())
			{
				clockTran = selectedRestrictedRecords.First();
				return false;
			}
			return true;
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

        protected virtual void EnableFields(AMClockTran clockTran)
        {			
			PXUIFieldAttribute.SetEnabled<AMClockTran.employeeID>(Transactions.Cache, clockTran, false);
            PXUIFieldAttribute.SetEnabled<AMClockTran.status>(Transactions.Cache, clockTran, false);
            PXUIFieldAttribute.SetEnabled<AMClockTran.startTime>(Transactions.Cache, clockTran, false);
            PXUIFieldAttribute.SetEnabled<AMClockTran.endTime>(Transactions.Cache, clockTran, false);
            PXUIFieldAttribute.SetEnabled<AMClockTran.duration>(Transactions.Cache, clockTran, false);
            PXUIFieldAttribute.SetEnabled<AMClockTran.wcID>(Transactions.Cache, clockTran, false);
            PXUIFieldAttribute.SetEnabled<AMClockTran.reasonCodeID>(Transactions.Cache, clockTran, clockTran.Status != ClockTranStatus.ClockedOut);
            PXUIFieldAttribute.SetEnabled<AMClockTran.orderType>(Transactions.Cache, clockTran, clockTran.Status == ClockTranStatus.NewStatus);
            PXUIFieldAttribute.SetEnabled<AMClockTran.prodOrdID>(Transactions.Cache, clockTran, clockTran.Status == ClockTranStatus.NewStatus);
            PXUIFieldAttribute.SetEnabled<AMClockTran.operationID>(Transactions.Cache, clockTran, clockTran.Status == ClockTranStatus.NewStatus);
            PXUIFieldAttribute.SetEnabled<AMClockTran.shiftCD>(Transactions.Cache, clockTran, clockTran.Status == ClockTranStatus.NewStatus || clockTran.Status == ClockTranStatus.ClockedIn);
            PXUIFieldAttribute.SetEnabled<AMClockTran.qty>(Transactions.Cache, clockTran, clockTran.Status == ClockTranStatus.ClockedIn);

			var prodItem = AMProdItem.PK.Find(this, clockTran.OrderType, clockTran.ProdOrdID);
			if (prodItem != null)
            {
                PXUIFieldAttribute.SetEnabled<AMClockTran.qtyScrapped>(Transactions.Cache, clockTran, clockTran.Status == ClockTranStatus.ClockedIn && clockTran.ScrapAction != ScrapAction.Quarantine && prodItem.Function != OrderTypeFunction.Disassemble && clockTran.IsLotSerialPreassigned == false);
            }
            PXUIFieldAttribute.SetEnabled<AMClockTran.reasonCodeID>(Transactions.Cache, clockTran, clockTran.Status == ClockTranStatus.ClockedIn && clockTran.QtyScrapped != 0 && (clockTran.ScrapAction == ScrapAction.WriteOff || clockTran.ScrapAction == ScrapAction.NoAction));
        }

        protected virtual void SetDefaultShift(PXCache sender, AMClockTran row)
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

            sender.SetValueExt<AMClockTran.shiftCD>(row, ((AMShift)result[0])?.ShiftCD);
        }
        #endregion
        #region Events

        protected virtual void _(Events.RowSelected<AMClockTran> e)
        {
            if (e.Row == null)
            {
                return;
            }
			Transactions.AllowInsert = e.Row.EmployeeID != null;
			EnableFields(e.Row);
        }
        
        protected virtual void _(Events.RowDeleting<AMClockTran> e)
        {
			var row = (AMClockTran)e.Row;
            if (row.Status != ClockTranStatus.NewStatus && internalDelete == false)
            {
                throw new PXException(Messages.GetLocal(Messages.DeleteInvalidForClockEntryStatus));
            }
        }

        protected virtual void _(Events.RowPersisting<AMClockTran> e)
        {
			var row = (AMClockTran)e.Row;
			if (row.QtyScrapped != 0 && row.ScrapAction == ScrapAction.WriteOff && row.ReasonCodeID == null)
            {
                new PXRowPersistingException(typeof(AMClockTran.reasonCodeID).Name, null, ErrorMessages.FieldIsEmpty, typeof(AMClockTran.reasonCodeID).Name);
            }
        }

        protected virtual void _(Events.FieldUpdated<AMClockTran, AMClockTran.operationID> e)
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

            e.Cache.SetValueExt<AMClockTran.lastOper>(e.Row, prodItem?.LastOperationID == e.Row.OperationID);
            e.Cache.SetValueExt<AMClockTran.siteID>(e.Row, prodItem?.SiteID);
            e.Cache.SetValueExt<AMClockTran.locationID>(e.Row, prodItem?.LocationID);
        }

        protected virtual void _(Events.FieldUpdated<AMClockTran, AMClockTran.prodOrdID> e)
        {
            //if the user changes the prodorder they need to reselect the operation
            e.Cache.SetValueExt<AMClockTran.operationID>(e.Row, null);
            e.Cache.SetValueExt<AMClockTran.wcID>(e.Row, null);
        }

        protected virtual void _(Events.RowInserted<AMClockTran> e)
        {
			var row = (AMClockTran)e.Row;
            if (row.Status == ClockTranStatus.NewStatus)
            {
                e.Cache.SetValueExt<AMClockTran.selected>(e.Row, true);
            }
        }

        protected virtual void _(Events.FieldUpdated<AMClockItem, AMClockItem.employeeID> e)
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

        public class DateOverlap
        {
            public int? LineNbr;
            public DateTime StartTime;
            public DateTime EndTime;
        }

        #endregion
    }
}
