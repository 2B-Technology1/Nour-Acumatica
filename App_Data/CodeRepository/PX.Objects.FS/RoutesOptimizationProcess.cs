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

using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.AP;
using PX.Objects.AR;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Objects.EP;
using PX.Objects.FS.RouteOtimizer;
using PX.Objects.GL;
using PX.Objects.PM;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace PX.Objects.FS
{
    #region Filter
    [Serializable]
    [PXHidden]
    public class FSAppointmentFilter : IBqlTable
    {
        #region Type
        public abstract class type : ListField_ROType { }

        [PXString(2, IsFixed = true)]
        [PXUIField(DisplayName = "Type")]
        [PXUnboundDefault(ID.Type_ROOptimization.ASSIGNED_APP)]
        [type.ListAtrribute]
        public virtual string Type { get; set; }
		#endregion

		#region ConsiderSkills
		public abstract class considerSkills : PX.Data.BQL.BqlBool.Field<considerSkills> { }

		[PXBool]
		[PXUIField(DisplayName = "Consider Skills")]
		[PXUIVisible(typeof(Where<FSAppointmentFilter.type, Equal<ListField_ROType.UnassignedApp>>))]
		public virtual bool? ConsiderSkills { get; set; }
		#endregion

		#region BranchID
		public abstract class branchID : PX.Data.BQL.BqlInt.Field<branchID> { }

        [PXInt]
        [PXDefault(typeof(AccessInfo.branchID), PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Branch")]
        [PXSelector(typeof(Search<Branch.branchID>), SubstituteKey = typeof(Branch.branchCD), DescriptionField = typeof(Branch.acctName))]
        public virtual int? BranchID { get; set; }
        #endregion
        #region BranchLocationID
        public abstract class branchLocationID : PX.Data.BQL.BqlInt.Field<branchLocationID> { }

        [PXInt]
        [PXDefault(typeof(
            Search<FSxUserPreferences.dfltBranchLocationID,
            Where<
                PX.SM.UserPreferences.userID, Equal<CurrentValue<AccessInfo.userID>>,
                And<PX.SM.UserPreferences.defBranchID, Equal<Current<FSAppointmentFilter.branchID>>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Branch Location")]
        [PXSelector(typeof(
            Search<FSBranchLocation.branchLocationID,
            Where<
                FSBranchLocation.branchID, Equal<Current<FSAppointmentFilter.branchID>>>>),
            SubstituteKey = typeof(FSBranchLocation.branchLocationCD),
            DescriptionField = typeof(FSBranchLocation.descr))]
        [PXFormula(typeof(Default<FSAppointmentFilter.branchID>))]
        public virtual int? BranchLocationID { get; set; }
        #endregion

        #region StartDate
        protected DateTime? _StartDate;
        [PXDBDate(UseTimeZone = true)]
        [PXDefault(typeof(AccessInfo.businessDate))]
        [PXUIField(DisplayName = "Date")]
        public virtual DateTime? StartDate
        {
            get
            {
                return _StartDate;
            }
            set
            {
                _StartDate = value;
                if (_StartDate == null)
                {
                    _StartDateWithTime = null;
                    _EndDateWithTime = null;
                }
                else
                {
                    _StartDateWithTime = new DateTime(_StartDate.Value.Year, _StartDate.Value.Month, _StartDate.Value.Day, 0, 0, 0);
                    _EndDateWithTime = new DateTime(_StartDate.Value.Year, _StartDate.Value.Month, _StartDate.Value.Day, 23, 59, 59);
                }
            }
        }
        public abstract class startDate : PX.Data.BQL.BqlDateTime.Field<startDate> { }
        #endregion
        #region StartDateWithTime
        protected DateTime? _StartDateWithTime;
        [PXDBDateAndTime(UseTimeZone = true)]
        public virtual DateTime? StartDateWithTime
        {
            get
            {
                return _StartDateWithTime;
            }
        }
        public abstract class startDateWithTime : PX.Data.BQL.BqlDateTime.Field<startDateWithTime> { }
        #endregion
        #region EndDateWithTime
        protected DateTime? _EndDateWithTime;
        [PXDBDateAndTime(UseTimeZone = true)]
        public virtual DateTime? EndDateWithTime
        {
            get
            {
                return _EndDateWithTime;
            }
        }
        public abstract class endDateWithTime : PX.Data.BQL.BqlDateTime.Field<endDateWithTime> { }
        #endregion
    }
    #endregion

    public class RoutesOptimizationProcess : PXGraph<RoutesOptimizationProcess>
    {
        public RoutesOptimizationProcess()
        {
            RoutesOptimizationProcess graphRoutesOptimizationProcess = null;

            AppointmentList.SetProcessDelegate(
                delegate (List<FSAppointmentFSServiceOrder> list)
                {
                    graphRoutesOptimizationProcess = PXGraph.CreateInstance<RoutesOptimizationProcess>();

                    PXResultset<FSAppointmentStaffMember, CSCalendar> staffSelected = new PXResultset<FSAppointmentStaffMember, CSCalendar>();
                
                    foreach (PXResult<FSAppointmentStaffMember, CSCalendar> row in StaffMemberFilter.Select())
                    {
                        if (((FSAppointmentStaffMember)row).Selected == true ) 
                        { 
                            staffSelected.Add(row);
                        }
                    }

                    OptimizeRoutes(graphRoutesOptimizationProcess, Filter.Current, list, staffSelected);
                }
            );
        }

        #region Views
        [PXHidden]
        public PXSetup<FSSetup> SetupRecord;

        [PXHidden]
        public PXSelect<BAccount> BAccounts;

        [PXHidden]
        public PXSelect<BAccountSelectorBase> BAccountSelectorBaseView;

        [PXHidden]
        public PXSelect<Vendor> Vendors;

        public PXCancel<FSAppointmentFilter> Cancel;

        public PXFilter<FSAppointmentFilter> Filter;
        public PXSelectJoinOrderBy<FSAppointmentStaffMember,
                    LeftJoin<CSCalendar,
                        On<CSCalendar.calendarID, Equal<FSAppointmentStaffMember.calendarID>>>,
                    OrderBy<Asc<FSAppointmentStaffMember.acctCD>>> StaffMemberFilter;

        [PXHidden]
        public PXSelect<FSAppointment> Appointments;

        [PXFilterable]
        public PXFilteredProcessingJoin<FSAppointmentFSServiceOrder, FSAppointmentFilter,
                        LeftJoin<Customer, On<Customer.bAccountID, Equal<FSAppointmentFSServiceOrder.customerID>>>,
                    Where2<
                        Where<FSAppointmentFSServiceOrder.canceled, Equal<False>,
                            And<FSAppointmentFSServiceOrder.hold, Equal<False>,
                            And<FSAppointmentFSServiceOrder.branchID, Equal<Current<FSAppointmentFilter.branchID>>,
                            And<FSAppointmentFSServiceOrder.branchLocationID, Equal<Current<FSAppointmentFilter.branchLocationID>>>>>>,
                        And2<
                            Where2<
                                Where<Current<FSAppointmentFilter.type>, Equal<FSAppointmentFilter.type.UnassignedApp>,
                                          And<FSAppointmentFSServiceOrder.primaryDriver, IsNull>>,
                                Or<Where<Current<FSAppointmentFilter.type>, Equal<FSAppointmentFilter.type.AssignedApp>,
                                          And<FSAppointmentFSServiceOrder.primaryDriver, IsNotNull,
                                          And<FSAppointmentFSServiceOrder.primaryDriver, In<Required<FSAppointmentFSServiceOrder.primaryDriver>>>>>>>,
                        And2<Where<Current<FSAppointmentFilter.startDateWithTime>, IsNull,
                            Or<Current<FSAppointmentFilter.startDateWithTime>, LessEqual<FSAppointmentFSServiceOrder.scheduledDateTimeBegin>>>,
                        And<Where<Current<FSAppointmentFilter.endDateWithTime>, IsNull,
                            Or<Current<FSAppointmentFilter.endDateWithTime>, GreaterEqual<FSAppointmentFSServiceOrder.scheduledDateTimeEnd>>>>>>>,
                    OrderBy<
                        Asc<FSAppointmentFSServiceOrder.scheduledDateTimeBegin>>> AppointmentList;

        public virtual IEnumerable appointmentList()
        {
            PXView select = new PXView(this, true, AppointmentList.View.BqlSelect);

            int[] staffResult = StaffMemberFilter.Select()
                                                .RowCast<FSAppointmentStaffMember>()
                                                .Where(_ => _.Selected == true)
                                                .Select(_=> _.BAccountID)
                                                .Cast<int>()
                                                .ToArray();

            Int32 totalrow = 0;
            Int32 startrow = PXView.StartRow;
            List<object> result = select.Select(PXView.Currents, new object[] { staffResult }, null,
                        null, null, null, ref startrow, PXView.MaximumRows, ref totalrow);

            PXView.StartRow = 0;

            return result;
        }
        #endregion

        #region CacheAttached
        [PXMergeAttributes(Method = MergeMethod.Append)]
        [PXUIField(DisplayName = "Project", Visibility = PXUIVisibility.Visible, Visible = false, FieldClass = ProjectAttribute.DimensionName)]
        protected void FSAppointmentFSServiceOrder_ProjectID_CacheAttached(PXCache sender) { }

        [PXMergeAttributes(Method = MergeMethod.Append)]
        [PXUIField(DisplayName = "Address Line 1", Visible = false)]
        protected void FSAppointmentFSServiceOrder_AddressLine1_CacheAttached(PXCache sender) { }

        [PXMergeAttributes(Method = MergeMethod.Merge)]
        [PXUIVisible(typeof(Where<Current<FSAppointmentFilter.type>, Equal<FSAppointmentFilter.type.AssignedApp>>))]
        [PXUIField(DisplayName = "Staff Member", FieldClass = "ROUTEOPTIMIZER")]
        protected virtual void FSAppointmentFSServiceOrder_PrimaryDriver_CacheAttached(PXCache sender) { }

        [PXMergeAttributes(Method = MergeMethod.Merge)]
        [PXUIField(DisplayName = "Type", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
        protected virtual void FSAppointmentStaffMember_Type_CacheAttached(PXCache sender) { }

        [PXMergeAttributes(Method = MergeMethod.Merge)]
        [PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Staff Member ID", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
        protected virtual void FSAppointmentStaffMember_AcctCD_CacheAttached(PXCache sender) { }

        [PXMergeAttributes(Method = MergeMethod.Merge)]
        [PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Staff Member Name", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
        protected virtual void FSAppointmentStaffMember_AcctName_CacheAttached(PXCache sender) { }

        #endregion

        #region ShowOnMap
        public PXAction<FSAppointmentFilter> ShowOnMap;
        [PXUIField(DisplayName = "SHOW ON MAP", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXButton]
        public virtual IEnumerable showOnMap(PXAdapter adapter)
        {
            if (Filter.Current != null && Filter.Current.StartDate != null)
            {
                KeyValuePair<string, string>[] parameters = new KeyValuePair<string, string>[]
                {
                    new KeyValuePair<string, string>("Date", Filter.Current.StartDate.Value.ToString())
                };

                throw new PXRedirectToBoardRequiredException(Paths.ScreenPaths.STAFF_APPOINTMENTS_ON_MAP, parameters);
            }

            return adapter.Get();
        }
        #endregion

        #region Events
        protected virtual void _(Events.FieldUpdating<FSAppointmentFilter.startDate> e)
        {
            if (e.Row == null)
            {
                return;
            }

            var filter = (FSAppointmentFilter)e.Row;
            DateTime? newDate = SharedFunctions.TryParseHandlingDateTime(e.Cache, e.NewValue);

            if (newDate != null)
            {
                filter.StartDate = newDate.Value.Date;
            }
            else
            {
                filter.StartDate = null;
            }
        }

        protected virtual void _(Events.RowSelected<FSAppointmentFilter> e)
        {
            if (e.Row == null)
            {
                return;
            }

            var filter = (FSAppointmentFilter)e.Row;

            AppointmentList.SetProcessAllEnabled(filter.StartDate != null);
            ShowOnMap.SetEnabled(Filter.Current.StartDate != null);
        }
        #endregion

        #region Functions
        public virtual int convertTimeToSec(DateTime? date)
        {
            DateHandler requestDate = new DateHandler(date);

            DateTime timeBegin = requestDate.StartOfDay();

            return (int)(((DateTime)date) - timeBegin).TotalSeconds;
        }

        public virtual DateTime convertSecToTime(int sec, DateTime? date)
        {
            DateHandler requestDate = new DateHandler(date);

            DateTime timeBegin = requestDate.StartOfDay();

            return timeBegin.AddSeconds(sec);
        }

        public virtual void OptimizeRoutes(RoutesOptimizationProcess graph, FSAppointmentFilter filter, List<FSAppointmentFSServiceOrder> list, PXResultset<FSAppointmentStaffMember, CSCalendar> staffSelected)
        {
            RouteOptimizerClient client = new RouteOptimizerClient();

            SingleDayOptimizationInput requestBody = new SingleDayOptimizationInput();

            List<FSAppointment> processList = new List<FSAppointment>();

            FSSetup fsSetupRow = graph.SetupRecord.Current;

            requestBody.balanced = true;

            requestBody.vehicles = new List<Vehicle>();
            requestBody.waypoints = new List<Waypoint>();

            string address = string.Empty;

            if (staffSelected != null && staffSelected.Count == 0)
            {
                throw new PXException(PXMessages.LocalizeFormatNoPrefix(TX.Error.SELECT_AT_LEAST_ONE_STAFF_MEMBER));
            }

            //Origin end Route location
            FSAddress fsAddressRow = PXSelectJoin<FSAddress,
                                     InnerJoin<FSBranchLocation, 
                                            On<FSBranchLocation.branchLocationAddressID, Equal<FSAddress.addressID>>>,
                                     Where<
                                         FSBranchLocation.branchLocationID, Equal<Required<FSBranchLocation.branchLocationID>>>>
                                     .Select(graph, list[0].BranchLocationID);

            address = SharedFunctions.GetAddressForGeolocation(fsAddressRow.PostalCode,
                                                            fsAddressRow.AddressLine1,
                                                            fsAddressRow.AddressLine2,
                                                            fsAddressRow.City,
                                                            fsAddressRow.State,
                                                            fsAddressRow.CountryID);

            GLocation[] results = Geocoder.Geocode(address, fsSetupRow.MapApiKey);

            if (results.Length == 0) 
            {
                throw new PXException(PXMessages.LocalizeFormatNoPrefix(TX.Error.MAPS_FAILED_REVERSE_ADRESS, TX.TableName.BRANCH_LOCATION));
            }

            CSCalendar csVendorCalendarRow = PXSelect<CSCalendar,
                                                        Where<CSCalendar.calendarID, Equal<Required<CSCalendar.calendarID>>>>
                                            .Select(graph, fsSetupRow.CalendarID);

			bool considerSkills = filter.ConsiderSkills == true;

			var staffSkills = considerSkills ? GetStaffSkills(staffSelected.Select(s => ((FSAppointmentStaffMember)s).BAccountID ?? 0).ToArray())
											 : null;

            //Driver Logic
            foreach (PXResult<FSAppointmentStaffMember, CSCalendar> result in staffSelected)
            {
                FSAppointmentStaffMember staffRow = (FSAppointmentStaffMember)result;
                CSCalendar csCalendarRow = (CSCalendar)result;

				List<string> tags = new List<string>() { staffRow.BAccountID.ToString() };

				if (considerSkills && staffSkills.TryGetValue(staffRow.BAccountID ?? 0, out var skills))
					tags.AddRange(skills);

				Vehicle vehicleRow = new Vehicle()
                {
                    name = staffRow.BAccountID.ToString(),
                    origin = new RouteLocation() { latitude = results[0].LatLng.Latitude, longitude = results[0].LatLng.Longitude },
                    destination = new RouteLocation() { latitude = results[0].LatLng.Latitude, longitude = results[0].LatLng.Longitude },
                    tags = tags
                };

                TimeWindow working = graph.GetWorkingTimeWindow(staffRow.EmployeeSDEnabled == true ? csCalendarRow : csVendorCalendarRow, filter.StartDate);
                Break lunchBreak = graph.GetBreakWindow(fsSetupRow);

                if (lunchBreak != null)
                {
                    vehicleRow.breaks = new List<Break>() { lunchBreak };
                }

                if (working != null)
                {
                    vehicleRow.timeWindow = working;
                    requestBody.vehicles.Add(vehicleRow);
                }
            }

            if (requestBody.vehicles.Count == 0)
            {
                for (int i = 0; i < list.Count; i++)
                {
                    FSAppointment fsAppointmentRow = list[i];
                    fsAppointmentRow.ROOptimizationStatus = ID.Status_ROOptimization.NOT_ABLE;
                    graph.Appointments.Update(fsAppointmentRow);
                    PXProcessing<FSAppointmentFSServiceOrder>.SetError(i, PXMessages.LocalizeFormatNoPrefix(TX.Error.APPOINTMENT_COULD_NOT_BE_REACH_SERVICED_NO_DRIVER_AVAILABLE));
                }

                if (graph.Appointments.Cache.IsDirty == true)
                {
                    graph.Appointments.Cache.Persist(PXDBOperation.Update);
                }

                return;
            }

            // Existing Appointment Logic
            if (filter.Type == ID.Type_ROOptimization.UNASSIGNED_APP && staffSelected.Count() > 0)
			{
				// Collect the already assigned appointments to take them into account in further assignments
				List<object> fsAppointmentSet = GetAssignedAppointments(graph, filter);

				foreach (PXResult<FSAppointment, FSServiceOrder, FSAddress> row in fsAppointmentSet)
				{
					FSAppointment fsAppointmentRow = (FSAppointment)row;
					fsAddressRow = (FSAddress)row;

					address = SharedFunctions.GetAddressForGeolocation(
														fsAddressRow.PostalCode,
														fsAddressRow.AddressLine1,
														fsAddressRow.AddressLine2,
														fsAddressRow.City,
														fsAddressRow.State,
														fsAddressRow.CountryID);

					Waypoint wp = GetWaypointFromAppointment(fsSetupRow, fsAppointmentRow, address, null);

					if (wp != null)
					{
						requestBody.waypoints.Add(wp);

						processList.Add(fsAppointmentRow);
					}
					else
					{
						fsAppointmentRow.ROOptimizationStatus = ID.Status_ROOptimization.ADDRESS_ERROR;
						graph.Appointments.Update(fsAppointmentRow);
					}
				}
			}

			// Appointment Logic
			for (int i = list.Count - 1; i >= 0; i--)
            {
                bool addressError = false,
					 skillsError = false;

                try
                {

                    address = SharedFunctions.GetAddressForGeolocation(
                                                        list[i].PostalCode,
                                                        list[i].AddressLine1,
                                                        list[i].AddressLine2,
                                                        list[i].City,
                                                        list[i].State,
                                                        list[i].CountryID);

					List<string> skills = considerSkills ? GetAppointmentSkills(list[i].AppointmentID.Value)
														 : null;

					if (considerSkills && skills.Count > 0 && !staffSkills.Values.Any(ss => skills.All(s => ss.Contains(s))))
						skillsError = true;
					else
					{
						Waypoint wp = GetWaypointFromAppointment(fsSetupRow, list[i], address, skills);

						if (wp != null)
						{
							requestBody.waypoints.Add(wp);
							processList.Add(list[i]);
						}
						else
						{
							addressError = true;
						}
					}
                }
                catch (Exception ex)
                {
                    addressError = true;
                }

                if (addressError || skillsError)
                {
                    FSAppointment fsAppointmentRow = list[i];

					if (addressError)
						fsAppointmentRow.ROOptimizationStatus = ID.Status_ROOptimization.ADDRESS_ERROR;
					else if (skillsError)
						fsAppointmentRow.ROOptimizationStatus = ID.Status_ROOptimization.NOT_ABLE;

					graph.Appointments.Update(fsAppointmentRow);

					if (addressError)
						PXProcessing<FSAppointmentFSServiceOrder>.SetError(i, PXMessages.LocalizeFormatNoPrefix(TX.Error.MAPS_FAILED_REVERSE_ADRESS, TX.TableName.APPOINTMENT));
					else if (skillsError)
						PXProcessing<FSAppointmentFSServiceOrder>.SetError(i, TX.Error.ASSIGNMENT_INSUFFICIENT_SKILLS);

					addressError = false;
					skillsError = false;
				}  
            }

			if (graph.Appointments.Cache.IsDirty == true)
            {
                graph.Appointments.Cache.Persist(PXDBOperation.Update);
            }

            try
            {
				AppointmentEntry graphAppointmentEntry = PXGraph.CreateInstance<AppointmentEntry>();

				if (requestBody.waypoints.Any()) // Should not run the process if no correct lines left
				{
					SingleDayOptimizationOutput responseObject = client.getSingleDayOptimization(fsSetupRow.ROWWApiEndPoint, fsSetupRow.ROWWLicensekey, requestBody);

					for (int i = 0; i < responseObject.routes.Count; i++)
					{
						int assignedstaffID;
						int.TryParse(responseObject.routes[i].vehicle.name, out assignedstaffID);
						bool changeFlag = false;

						for (int j = 1; j < responseObject.routes[i].steps.Count - 1; j++)
						{
							RouteOtimizer.RouteStep currentAppointment = responseObject.routes[i].steps[j];
							int appointmentID;
							int.TryParse(currentAppointment.waypoint.name, out appointmentID);

							FSAppointment fsAppointmentListRow = processList.Find(x => x.AppointmentID == appointmentID);

							var newBegin = convertSecToTime(currentAppointment.serviceStartTimeSec, fsAppointmentListRow.ScheduledDateTimeBegin);
							var newEnd = newBegin.AddSeconds(currentAppointment.departureTimeSec - currentAppointment.arrivalTimeSec);

							if (fsAppointmentListRow.PrimaryDriver != null)
							{
								FSAppointment copy = (FSAppointment)graph.Appointments.Cache.CreateCopy(fsAppointmentListRow);
								UpdateAppointmentHeader(copy, j, null, newBegin, newEnd);
								copy = graph.Appointments.Update(copy);
								copy.ROOptimizationStatus = ID.Status_ROOptimization.OPTIMIZED;
								graph.Appointments.Update(copy);
								changeFlag = true;
							}
							else
							{
								FSAppointment fsAppointmentRow = graphAppointmentEntry.AppointmentRecords.Current = graphAppointmentEntry.AppointmentRecords.Search<FSAppointment.refNbr>(
										fsAppointmentListRow.RefNbr, fsAppointmentListRow.SrvOrdType);

								fsAppointmentRow.ROOptimizationStatus = ID.Status_ROOptimization.OPTIMIZED;

								UpdateAppointmentHeader(fsAppointmentRow, j, assignedstaffID, newBegin, newEnd);

								fsAppointmentRow = graphAppointmentEntry.AppointmentRecords.Update(fsAppointmentRow);

								fsAppointmentRow.ROOptimizationStatus = ID.Status_ROOptimization.OPTIMIZED;

								graphAppointmentEntry.AppointmentRecords.Update(fsAppointmentRow);

								FSAppointmentEmployee fsAppointmentEmployeeRow_New = new FSAppointmentEmployee()
								{
									AppointmentID = fsAppointmentRow.AppointmentID,
									EmployeeID = assignedstaffID
								};

								fsAppointmentEmployeeRow_New = graphAppointmentEntry.AppointmentServiceEmployees.Insert(fsAppointmentEmployeeRow_New);

								graphAppointmentEntry.Save.Press();
							}
						}

						if (changeFlag == true)
						{
							graph.Appointments.Cache.Persist(PXDBOperation.Update);
						}
					}

					foreach (OutputWaypoint wp in responseObject.unreachableWaypoints.Concat(responseObject.unreachedWaypoints).GroupBy(p => p.name).Select(g => g.First()).ToList())
					{
						int appointmentID;
						int.TryParse(wp.name, out appointmentID);

						FSAppointment fsAppointmentRow = list.Find(x => x.AppointmentID == appointmentID);
						if (fsAppointmentRow != null)
						{
							for (int i = 0; i < list.Count; i++)
							{
								if (fsAppointmentRow.AppointmentID == list[i].AppointmentID)
								{
									fsAppointmentRow.ROOptimizationStatus = ID.Status_ROOptimization.NOT_ABLE;
									graph.Appointments.Update(fsAppointmentRow);

									PXProcessing<FSAppointmentFSServiceOrder>.SetError(i, PXMessages.LocalizeFormatNoPrefix(TX.Error.APPOINTMENT_COULD_NOT_BE_REACH_SERVICED));
								}
							}
						}
					}
				}

				#region Mark the skipped appointments as Could Not Be Optimized
				for (int i = 0; i < list.Count; i++)
				{
					if (list[i].ROOptimizationStatus == ID.Status_ROOptimization.ADDRESS_ERROR)
					{
						FSAppointment fsAppointmentRow = graphAppointmentEntry.AppointmentRecords.Current = graphAppointmentEntry.AppointmentRecords.Search<FSAppointment.refNbr>(
									list[i].RefNbr, list[i].SrvOrdType);

						fsAppointmentRow.ROOptimizationStatus = ID.Status_ROOptimization.NOT_ABLE;
						graphAppointmentEntry.AppointmentRecords.Update(fsAppointmentRow);
					}
				}

				graphAppointmentEntry.Save.Press();
				#endregion

				graph.Appointments.Cache.Persist(PXDBOperation.Update);
            }
            catch (PXException e)
            {
                for (int i = 0; i < list.Count; i++)
                {
                    FSAppointment fsAppointmentRow = list[i];
                    fsAppointmentRow.ROOptimizationStatus = ID.Status_ROOptimization.NOT_ABLE;
                    graph.Appointments.Update(fsAppointmentRow);

                    PXProcessing<FSAppointmentFSServiceOrder>.SetError(i, PXMessages.LocalizeFormatNoPrefix(e.Message));
                }

                graph.Appointments.Cache.Persist(PXDBOperation.Update);
            }
        }

		/// <summary>Retrieves the already assigned appointmens by the filter</summary>
		private List<object> GetAssignedAppointments(RoutesOptimizationProcess graph, FSAppointmentFilter filter)
		{
			List<object> args = new List<object>();

			BqlCommand fsAppointmentList = new Select2<FSAppointment,
													InnerJoin<FSServiceOrder,
														On<FSServiceOrder.sOID, Equal<FSAppointment.sOID>>,
													InnerJoin<FSAddress,
														On<FSAddress.addressID, Equal<FSServiceOrder.serviceOrderAddressID>>>>>();

			if (filter.BranchID != null)
			{
				fsAppointmentList = fsAppointmentList.WhereAnd(typeof(Where<FSServiceOrder.branchID, Equal<Required<FSServiceOrder.branchID>>>));
				args.Add(filter.BranchID);
			}

			if (filter.BranchLocationID != null)
			{
				fsAppointmentList = fsAppointmentList.WhereAnd(typeof(Where<FSServiceOrder.branchLocationID, Equal<Required<FSServiceOrder.branchLocationID>>>));
				args.Add(filter.BranchLocationID);
			}

			if (filter.StartDate != null)
			{
				fsAppointmentList = fsAppointmentList.WhereAnd(typeof(Where<FSAppointment.scheduledDateTimeBegin, GreaterEqual<Required<FSAppointment.scheduledDateTimeBegin>>>));
				args.Add(filter.StartDateWithTime);
			}

			if (filter.EndDateWithTime != null)
			{
				fsAppointmentList = fsAppointmentList.WhereAnd(typeof(Where<FSAppointment.scheduledDateTimeEnd, LessEqual<Required<FSAppointment.scheduledDateTimeEnd>>>));
				args.Add(filter.EndDateWithTime);
			}

			fsAppointmentList = fsAppointmentList.WhereAnd(typeof(Where<FSAppointment.primaryDriver, In<Required<FSAppointment.primaryDriver>>>));

			int[] staffResult = StaffMemberFilter.Select()
										.RowCast<FSAppointmentStaffMember>()
										.Where(_ => _.Selected == true)
										.Select(_ => _.BAccountID)
										.Cast<int>()
										.ToArray();

			args.Add(staffResult);

			PXView appointmentView = new PXView(graph, true, fsAppointmentList);

			var fsAppointmentSet = appointmentView.SelectMulti(args.ToArray());
			return fsAppointmentSet;
		}

		private List<string> GetAppointmentSkills(int appointmentId)
		{
			var skills = SelectFrom<FSSkill>
							.InnerJoin<FSServiceSkill>
								.On<FSSkill.skillID.IsEqual<FSServiceSkill.skillID>>
							.InnerJoin<FSAppointmentDet>.On<FSServiceSkill.serviceID.IsEqual<FSAppointmentDet.inventoryID>>
							.Where<FSAppointmentDet.appointmentID.IsEqual<@P.AsInt>>
							.AggregateTo<GroupBy<FSSkill.skillCD>>
							.View
							.Select(this, appointmentId)
							.FirstTableItems;

			return skills.Select(s => s.SkillCD.Trim()).ToList();
		}

		private Dictionary<int, List<string>> GetStaffSkills(int[] staffIds)
		{
			var skills = SelectFrom<FSSkill>
							.InnerJoin<FSEmployeeSkill>
								.On<FSSkill.skillID.IsEqual<FSEmployeeSkill.skillID>>
							.Where<FSEmployeeSkill.employeeID.IsIn<@P.AsInt>>
							.View
							.Select<PXResultset<FSSkill, FSEmployeeSkill>>(this, staffIds);

			var dict = skills.Select(s => new { s.GetItem<FSEmployeeSkill>().EmployeeID, s.GetItem<FSSkill>().SkillCD })
							 .GroupBy(s => s.EmployeeID.Value)
							 .ToDictionary(g => g.Key, g => g.Select(s => s.SkillCD.Trim()).ToList());

			return dict;
		}

		public virtual TimeWindow GetWorkingTimeWindow(CSCalendar csCalendarRow, DateTime? date) 
        {
            if (csCalendarRow.MonStartTime == null && csCalendarRow.MonStartTime.HasValue == false)
                return null;

            TimeWindow workingTime = new TimeWindow();

            workingTime.startTimeSec = 0;
            workingTime.stopTimeSec = 0;

            switch (date.Value.DayOfWeek)
            {
                case DayOfWeek.Sunday:
                    if (csCalendarRow.SunStartTime != null && csCalendarRow.SunStartTime.HasValue == true)
                        workingTime.startTimeSec = (int)csCalendarRow.SunStartTime.Value.TimeOfDay.TotalSeconds;
                    if (csCalendarRow.SunEndTime != null && csCalendarRow.SunEndTime.HasValue == true)
                        workingTime.stopTimeSec = (int)csCalendarRow.SunEndTime.Value.TimeOfDay.TotalSeconds;
                    break;
                case DayOfWeek.Monday:
                    if (csCalendarRow.MonStartTime != null && csCalendarRow.MonStartTime.HasValue == true) 
                        workingTime.startTimeSec = (int)csCalendarRow.MonStartTime.Value.TimeOfDay.TotalSeconds;
                    if (csCalendarRow.MonEndTime != null && csCalendarRow.MonEndTime.HasValue == true)
                        workingTime.stopTimeSec = (int)csCalendarRow.MonEndTime.Value.TimeOfDay.TotalSeconds;
                    break;
                case DayOfWeek.Tuesday:
                    if (csCalendarRow.TueStartTime != null && csCalendarRow.TueStartTime.HasValue == true)
                        workingTime.startTimeSec = (int)csCalendarRow.TueStartTime.Value.TimeOfDay.TotalSeconds;
                    if (csCalendarRow.TueEndTime != null && csCalendarRow.TueEndTime.HasValue == true)
                        workingTime.stopTimeSec = (int)csCalendarRow.TueEndTime.Value.TimeOfDay.TotalSeconds;
                    break;
                case DayOfWeek.Wednesday:
                    if (csCalendarRow.WedStartTime != null && csCalendarRow.WedStartTime.HasValue == true)
                        workingTime.startTimeSec = (int)csCalendarRow.WedStartTime.Value.TimeOfDay.TotalSeconds;
                    if (csCalendarRow.WedEndTime != null && csCalendarRow.WedEndTime.HasValue == true)
                        workingTime.stopTimeSec = (int)csCalendarRow.MonEndTime.Value.TimeOfDay.TotalSeconds;
                    break;
                case DayOfWeek.Thursday:
                    if (csCalendarRow.ThuStartTime != null && csCalendarRow.ThuStartTime.HasValue == true)
                        workingTime.startTimeSec = (int)csCalendarRow.ThuStartTime.Value.TimeOfDay.TotalSeconds;
                    if (csCalendarRow.ThuEndTime != null && csCalendarRow.ThuEndTime.HasValue == true)
                        workingTime.stopTimeSec = (int)csCalendarRow.ThuEndTime.Value.TimeOfDay.TotalSeconds;
                    break;
                case DayOfWeek.Friday:
                    if (csCalendarRow.FriStartTime != null && csCalendarRow.FriStartTime.HasValue == true)
                        workingTime.startTimeSec = (int)csCalendarRow.FriStartTime.Value.TimeOfDay.TotalSeconds;
                    if (csCalendarRow.FriEndTime != null && csCalendarRow.FriEndTime.HasValue == true)
                        workingTime.stopTimeSec = (int)csCalendarRow.FriEndTime.Value.TimeOfDay.TotalSeconds;
                    break;
                case DayOfWeek.Saturday:
                    if (csCalendarRow.SatStartTime != null && csCalendarRow.SatStartTime.HasValue == true)
                        workingTime.startTimeSec = (int)csCalendarRow.SatStartTime.Value.TimeOfDay.TotalSeconds;
                    if (csCalendarRow.SatEndTime != null && csCalendarRow.SatEndTime.HasValue == true)
                        workingTime.stopTimeSec = (int)csCalendarRow.SatEndTime.Value.TimeOfDay.TotalSeconds;
                    break;
                default:
                    break;
            }

            return workingTime.startTimeSec == workingTime.stopTimeSec ? null : workingTime;
        }

        public virtual Break GetBreakWindow(FSSetup fsSetupRow)
        {
            Break lunchBreak = null;

            if (fsSetupRow.ROLunchBreakDuration > 0)
            {
                lunchBreak = new Break();

                lunchBreak.durationSec = fsSetupRow.ROLunchBreakDuration.Value * 60;
                lunchBreak.startTimeSec = (int)fsSetupRow.ROLunchBreakStartTimeFrame.Value.TimeOfDay.TotalSeconds;
                lunchBreak.stopTimeSec = (int)fsSetupRow.ROLunchBreakEndTimeFrame.Value.TimeOfDay.TotalSeconds - lunchBreak.durationSec;
            }

            return lunchBreak;
        }

        public virtual Waypoint GetWaypointFromAppointment(FSSetup fsSetupRow, FSAppointment fsAppointmentRow, string address, List<string> skills) 
        {
            Waypoint wp = new Waypoint();

            GLocation[] results;

            if (fsAppointmentRow.MapLatitude == null || fsAppointmentRow.MapLongitude == null)
            {
                results = Geocoder.Geocode(address, fsSetupRow.MapApiKey);

                if (results.Length > 0)
                {
                    fsAppointmentRow.MapLatitude = (decimal)results[0].LatLng.Latitude;
                    fsAppointmentRow.MapLongitude = (decimal)results[0].LatLng.Longitude;
                }
            }

            if (fsAppointmentRow.MapLatitude != null && fsAppointmentRow.MapLongitude != null)
            {
                if (fsAppointmentRow.Confirmed == true
                        && fsAppointmentRow.ScheduledDateTimeBegin.HasValue)
                {
                    TimeWindow tm = new TimeWindow();
                    wp.timeWindows = new List<TimeWindow>();

                    tm.startTimeSec = (int)fsAppointmentRow.ScheduledDateTimeBegin.Value.TimeOfDay.TotalSeconds;
                    tm.stopTimeSec = (int)fsAppointmentRow.ScheduledDateTimeBegin.Value.TimeOfDay.TotalSeconds;

                    wp.timeWindows.Add(tm);
                }

                wp.name = fsAppointmentRow.AppointmentID.ToString();
                wp.serviceTimeSec = (int)((fsAppointmentRow.ScheduledDateTimeEnd - fsAppointmentRow.ScheduledDateTimeBegin).Value.TotalSeconds);
                wp.location = new RouteLocation() { latitude = (double)fsAppointmentRow.MapLatitude, longitude = (double)fsAppointmentRow.MapLongitude };

				if (fsAppointmentRow.PrimaryDriver != null)
				{
					wp.tagsIncludeAnd = new List<string>() { fsAppointmentRow.PrimaryDriver.ToString() };
					wp.priority = 99;
					// If the staff is already set, there is no sense to specify the skills
				}
				else if (skills != null)
					wp.tagsIncludeAnd = skills.ToList();
			}
            else 
            {
                return null;
            }

            return wp;
        }

        public virtual void UpdateAppointmentHeader(FSAppointment fsAppointmentRow, int? sortOrder, int? assignedstaffID, DateTime? newBegin, DateTime? newEnd)
        {
            fsAppointmentRow.ROOriginalSortOrder = fsAppointmentRow.ROSortOrder;
            fsAppointmentRow.ROSortOrder = sortOrder;

            if(assignedstaffID != null)
                fsAppointmentRow.PrimaryDriver = assignedstaffID;

            if(newBegin != null)
                fsAppointmentRow.ScheduledDateTimeBegin = newBegin;

            if(newEnd != null)
                fsAppointmentRow.ScheduledDateTimeEnd = newEnd;
        }
        #endregion
    }
}
