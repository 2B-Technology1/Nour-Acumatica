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

using PX.Common;
using PX.Data;
using PX.Data.DependencyInjection;
using PX.LicensePolicy;
using PX.Objects.CR;
using PX.Objects.GL;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PX.Concurrency;

namespace PX.Objects.FS
{
    public class ServiceContractScheduleEntry : ServiceContractScheduleEntryBase<ServiceContractScheduleEntry, 
                                                FSContractSchedule, FSContractSchedule.scheduleID, 
                                                FSContractSchedule.entityID,
                                                FSContractSchedule.customerID>, IGraphWithInitialization
    {
        [InjectDependency]
        protected ILicenseLimitsService _licenseLimits { get; set; }

        void IGraphWithInitialization.Initialize()
        {
            if (_licenseLimits != null)
            {
                OnBeforeCommit += _licenseLimits.GetCheckerDelegate<FSServiceContract>(
                    new TableQuery(TransactionTypes.LinesPerMasterRecord, typeof(FSSchedule), (graph) =>
                    {
                        return new PXDataFieldValue[]
                        {
                            new PXDataFieldValue<FSSchedule.customerID>(((ServiceContractScheduleEntry)graph).ContractScheduleRecords.Current?.CustomerID),
                            new PXDataFieldValue<FSSchedule.entityID>(((ServiceContractScheduleEntry)graph).ContractScheduleRecords.Current?.EntityID)
                        };
                    }));

                OnBeforeCommit += _licenseLimits.GetCheckerDelegate<FSSchedule>(
                    new TableQuery(TransactionTypes.LinesPerMasterRecord, typeof(FSScheduleDet), (graph) =>
                    {
                        return new PXDataFieldValue[]
                        {
                            new PXDataFieldValue<FSScheduleDet.scheduleID>(((ServiceContractScheduleEntry)graph).ContractScheduleRecords.Current?.ScheduleID)
                        };
                    }));
            }
        }

        #region Delegates
        public virtual IEnumerable scheduleProjectionRecords()
        {
            return Delegate_ScheduleProjectionRecords(ContractScheduleRecords.Cache, ContractScheduleRecords.Current, FromToFilter.Current, ID.RecordType_ServiceContract.SERVICE_CONTRACT);
        }
        #endregion

        #region CacheAttached
        #region FSContractSchedule_BranchID
        [PXDBInt]
        [PXUIField(DisplayName = "Branch ID", Enabled = false)]
        [PXSelector(typeof(Branch.branchID), SubstituteKey = typeof(Branch.branchCD), DescriptionField = typeof(Branch.acctName))]
        protected virtual void FSContractSchedule_BranchID_CacheAttached(PXCache sender)
        {
        }
        #endregion
        #region FSContractSchedule_BranchLocationID
        [PXDBInt]
        [PXUIField(DisplayName = "Branch Location ID", Enabled = false)]
        [FSSelectorBranchLocationByFSSchedule]
        [PXFormula(typeof(Default<FSSchedule.branchID>))]
        protected virtual void FSContractSchedule_BranchLocationID_CacheAttached(PXCache sender)
        {
        }
        #endregion
        #region FSContractSchedule_CreatedByScreenID
        [PXUIField(Visible = false)]
        [PXDBCreatedByScreenID]
        protected virtual void FSContractSchedule_CreatedByScreenID_CacheAttached(PXCache sender)
        {
        }
        #endregion
        #endregion

        #region Methods
        public virtual bool IsTheScheduleExpired(FSContractSchedule fsContractScheduleRow)
        {
            if (fsContractScheduleRow == null
                    || fsContractScheduleRow.EndDate == null)
            {
                return false;
            }

            return fsContractScheduleRow.EndDate < Accessinfo.BusinessDate;
        }
        #endregion 

        #region Report
        public PXAction<FSContractSchedule> report;
        [PXButton(SpecialType = PXSpecialButtonType.ReportsFolder, MenuAutoOpen = true)]
        [PXUIField(DisplayName = "Reports")]
        public virtual IEnumerable Report(PXAdapter adapter,
            [PXString(8, InputMask = "CC.CC.CC.CC")]
            string reportID
            )
        {
            List<FSContractSchedule> list = adapter.Get<FSContractSchedule>().ToList();
            if (!string.IsNullOrEmpty(reportID))
            {
                Save.Press();
                Dictionary<string, string> parameters = new Dictionary<string, string>();
                string actualReportID = null;

                PXReportRequiredException ex = null;
                Dictionary<PX.SM.PrintSettings, PXReportRequiredException> reportsToPrint = new Dictionary<PX.SM.PrintSettings, PXReportRequiredException>();

                foreach (FSContractSchedule schedule in list)
                {
                    parameters = new Dictionary<string, string>();
                    parameters["FSServiceContract.RefNbr"] = CurrentServiceContract.Current?.RefNbr;
                    parameters["FSSchedule.RefNbr"] = schedule.RefNbr;

					actualReportID = new NotificationUtility(this).SearchCustomerReport(reportID, schedule.CustomerID, schedule.BranchID);
                    ex = PXReportRequiredException.CombineReport(ex, actualReportID, parameters);

                    reportsToPrint = PX.SM.SMPrintJobMaint.AssignPrintJobToPrinter(reportsToPrint, parameters, adapter, new NotificationUtility(this).SearchPrinter, SO.SONotificationSource.Customer, reportID, actualReportID, schedule.BranchID);
                }

                if (ex != null)
                {
                    LongOperationManager.StartAsyncOperation(async ct=>
                    {
	                    await PX.SM.SMPrintJobMaint.CreatePrintJobGroups(reportsToPrint, ct);
	                    throw ex;
                    });

                    
                }
            }
            return adapter.Get();
        }
        #endregion

        #region Actions

        #region OpenServiceContractInq
        public PXAction<FSContractSchedule> openServiceContractInq;
        [PXUIField(DisplayName = "Generate from Service Contracts", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXButton(OnClosingPopup = PXSpecialButtonType.Cancel)]
        public virtual void OpenServiceContractInq()
        {
            ServiceContractInq serviceContractInqGraph = PXGraph.CreateInstance<ServiceContractInq>();

            ServiceContractFilter filter = new ServiceContractFilter();
            filter.ScheduleID = ContractScheduleRecords.Current.ScheduleID;
            filter.ToDate = ContractScheduleRecords.Current.EndDate ?? ContractScheduleRecords.Current.StartDate;
            serviceContractInqGraph.Filter.Insert(filter);

            throw new PXRedirectRequiredException(serviceContractInqGraph, null) { Mode = PXBaseRedirectException.WindowMode.NewWindow };
        }
        #endregion

        #endregion

        #region Event Handlers

        #region FSContractSchedule

        #region FieldSelecting
        #endregion
        #region FieldDefaulting

        protected virtual void _(Events.FieldDefaulting<FSContractSchedule, FSContractSchedule.scheduleStartTime> e)
        {
            e.NewValue = PXDBDateAndTimeAttribute.CombineDateTime(PXTimeZoneInfo.Now, PXTimeZoneInfo.Now);
        }

        #endregion
        #region FieldUpdating
        #endregion
        #region FieldVerifying
        #endregion
        #region FieldUpdated

        protected virtual void _(Events.FieldUpdated<FSContractSchedule, FSContractSchedule.entityID> e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSContractSchedule fsScheduleRow = (FSContractSchedule)e.Row;

            if (fsScheduleRow.EntityID != null)
            {
                FSServiceContract fsServiceContract = FSServiceContract.PK.Find(this, fsScheduleRow.EntityID);

                if (fsServiceContract != null)
                {
                    fsScheduleRow.CustomerID = fsServiceContract.CustomerID;
                    fsScheduleRow.CustomerLocationID = fsServiceContract.CustomerLocationID;
                    fsScheduleRow.BranchID = fsServiceContract.BranchID;
                    fsScheduleRow.BranchLocationID = fsServiceContract.BranchLocationID;
                    fsScheduleRow.StartDate = fsServiceContract.StartDate;
                    fsScheduleRow.EndDate = fsServiceContract.EndDate;
                }
            }
        }

        protected virtual void _(Events.FieldUpdated<FSContractSchedule, FSContractSchedule.branchID> e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSContractSchedule fsScheduleRow = (FSContractSchedule)e.Row;
            fsScheduleRow.BranchLocationID = null;
        }

        #endregion

        protected virtual void _(Events.RowSelecting<FSContractSchedule> e)
        {
        }

        protected virtual void _(Events.RowSelected<FSContractSchedule> e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSContractSchedule fsContractScheduleRow = (FSContractSchedule)e.Row;
            PXCache cache = e.Cache;

            ContractSchedule_RowSelected_PartialHandler(cache, fsContractScheduleRow);

            bool existAnyGenerationProcess = SharedFunctions.ShowWarningScheduleNotProcessed(cache, fsContractScheduleRow);
            openServiceContractInq.SetEnabled(existAnyGenerationProcess == false && !IsTheScheduleExpired(fsContractScheduleRow));

            bool enableScheduleStartTime = fsContractScheduleRow.ScheduleGenType == ID.ScheduleGenType_ServiceContract.APPOINTMENT;

            PXUIFieldAttribute.SetEnabled<FSContractSchedule.scheduleStartTime>(cache, fsContractScheduleRow, enableScheduleStartTime);
            PXDefaultAttribute.SetPersistingCheck<FSContractSchedule.scheduleStartTime>(cache, fsContractScheduleRow, enableScheduleStartTime ? PXPersistingCheck.NullOrBlank : PXPersistingCheck.Nothing);
        }

        protected virtual void _(Events.RowInserting<FSContractSchedule> e)
        {
        }

        protected virtual void _(Events.RowInserted<FSContractSchedule> e)
        {
        }

        protected virtual void _(Events.RowUpdating<FSContractSchedule> e)
        {
        }

        protected virtual void _(Events.RowUpdated<FSContractSchedule> e)
        {
        }

        protected virtual void _(Events.RowDeleting<FSContractSchedule> e)
        {
        }

        protected virtual void _(Events.RowDeleted<FSContractSchedule> e)
        {
            FSSchedule_Row_Deleted_PartialHandler(e.Cache, e.Args);
        }

        protected virtual void _(Events.RowPersisting<FSContractSchedule> e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSContractSchedule fsScheduleRow = (FSContractSchedule)e.Row;
            FSServiceContract fsServiceContractRow = (FSServiceContract)ContractSelected.Current;

            ContractSchedule_RowPersisting_PartialHandler(e.Cache, fsServiceContractRow, fsScheduleRow, e.Operation, TX.ModuleName.EQUIPMENT_MODULE);
        }

        protected virtual void _(Events.RowPersisted<FSContractSchedule> e)
        {
        }

        #endregion

        #endregion
    }
}
