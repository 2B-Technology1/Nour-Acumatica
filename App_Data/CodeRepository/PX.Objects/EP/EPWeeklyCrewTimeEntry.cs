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

using PX.Api;
using PX.Common;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Data.EP;
using PX.Objects.AP;
using PX.Objects.CN.Common.Extensions;
using PX.Objects.Common;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Objects.GL;
using PX.Objects.PM;
using PX.TM;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace PX.Objects.EP
{
	public class EPWeeklyCrewTimeEntry : PXGraph<EPWeeklyCrewTimeEntry, EPWeeklyCrewTimeActivity>, PXImportAttribute.IPXPrepareItems
	{
		#region Data Views and delegates

		public SelectFrom<EPWeeklyCrewTimeActivity>.View Document;
		//At least 1 dataview with PMTimeActivity is absolutely needed so PXFomulas/PXParent creation from PMTimeActivity DAC gets triggered
		public SelectFrom<PMTimeActivity> PMTimeActivityDummyView;
		public SelectFrom<EPCompanyTreeMember>
			.Where<EPCompanyTreeMember.workGroupID.IsEqual<EPWeeklyCrewTimeActivity.workgroupID.FromCurrent>
				.And<EPCompanyTreeMember.active.IsEqual<True>>>.View OriginalGroupMembers;

		[PXImport]
		public SelectFrom<EPActivityApprove>
			.InnerJoin<EPEmployee>.On<EPEmployee.defContactID.IsEqual<EPActivityApprove.ownerID>>
			.InnerJoin<EPEarningType>.On<EPEarningType.typeCD.IsEqual<EPActivityApprove.earningTypeID>>
			.Where<EPActivityApprove.workgroupID.IsEqual<EPWeeklyCrewTimeActivity.workgroupID.FromCurrent>
				.And<Brackets<EPActivityApprove.weekID.IsEqual<EPWeeklyCrewTimeActivity.week.FromCurrent>
					.Or<EPActivityApprove.weekID.IsNull>>>>.View TimeActivities;
		public virtual IEnumerable timeActivities()
		{
			IEnumerable<object> returnedList = new List<object>();
			if (Document.Current.WorkgroupID == null || Document.Current.Week == null)
			{
				return returnedList;
			}

			BqlCommand cmd = TimeActivities.View.BqlSelect;
			if (Filter.Current.ProjectID != null)
			{
				cmd = cmd.WhereAnd<Where<EPActivityApprove.projectID, Equal<Current<EPWeeklyCrewTimeActivityFilter.projectID>>>>();
			}
			if (Filter.Current.ProjectTaskID != null)
			{
				cmd = cmd.WhereAnd<Where<EPActivityApprove.projectTaskID, Equal<Current<EPWeeklyCrewTimeActivityFilter.projectTaskID>>>>();
			}

			//Prevents miscalculation of Filter totals if no column filters are applied
			if (TimeActivities.View.GetExternalFilters() != null)
			{
				int startRow = 0;
				int totalRows = 0;
				returnedList = new PXView(this, false, cmd).Select(PXView.Currents, PXView.Parameters, PXView.Searches, PXView.SortColumns, PXView.Descendings,
						TimeActivities.View.GetExternalFilters(), ref startRow, 0, ref totalRows);
			}
			else
			{
				returnedList = new PXView(this, false, cmd).SelectMulti();
			}

			if (Filter.Current.Day != null)
			{
				returnedList = returnedList.Cast<PXResult<EPActivityApprove, EPEmployee, EPEarningType>>().Where(x => ((EPActivityApprove)x).DayOfWeek == Filter.Current.Day);
			}

			Filter.Current.RegularTime = 0;
			Filter.Current.Overtime = 0;
			Filter.Current.BillableTime = 0;
			Filter.Current.BillableOvertime = 0;
			foreach (PXResult<EPActivityApprove, EPEmployee, EPEarningType> record in returnedList)
			{
				var activity = (EPActivityApprove)record;
				var earningType = (EPEarningType)record;

				Filter.Current.RegularTime += earningType.IsOvertime == true ? 0 : activity.TimeSpent.GetValueOrDefault();
				Filter.Current.BillableTime += earningType.IsOvertime == true ? 0 : activity.TimeBillable.GetValueOrDefault();
				Filter.Current.Overtime += activity.OvertimeSpent.GetValueOrDefault();
				Filter.Current.BillableOvertime += activity.OvertimeBillable.GetValueOrDefault();
			}

			return returnedList;
		}

		public SelectFrom<EPTimeActivitiesSummary>
			.Where<EPTimeActivitiesSummary.workgroupID.IsEqual<EPWeeklyCrewTimeActivity.workgroupID.FromCurrent>
				.And<EPTimeActivitiesSummary.week.IsEqual<EPWeeklyCrewTimeActivity.week.FromCurrent>>>.View WorkgroupTimeSummary;
		public virtual IEnumerable workgroupTimeSummary()
		{
			IEnumerable<EPTimeActivitiesSummary> returnedList = new List<EPTimeActivitiesSummary>();
			if (Document.Current.WorkgroupID == null || Document.Current.Week == null)
			{
				return returnedList;
			}

			// Gets activities marked with the selected workgroup
			BqlCommand cmd = WorkgroupTimeSummary.View.BqlSelect;
			returnedList = new PXView(this, false, cmd).SelectMulti().Select(x => (EPTimeActivitiesSummary)x);

			// Inserts group members without activities
			IEnumerable<EPCompanyTreeMember> groupMembers = OriginalGroupMembers.Select().FirstTableItems;
			int totalWithoutActivities = returnedList.Count(x => x.IsWithoutActivities == true);
			bool wasDirty = WorkgroupTimeSummary.Cache.IsDirty;
			foreach (EPCompanyTreeMember member in groupMembers.Where(x => !returnedList.Any(y => x.ContactID == y.ContactID)))
			{
				var summary = new EPTimeActivitiesSummary();
				summary.ContactID = member.ContactID;
				summary = WorkgroupTimeSummary.Insert(summary);
				returnedList = returnedList.Append(summary);
				totalWithoutActivities++;
			}
			WorkgroupTimeSummary.Cache.IsDirty = wasDirty;

			Filter.Current.TotalWorkgroupMembers = returnedList.Count();
			Filter.Current.TotalWorkgroupMembersWithActivities = Filter.Current.TotalWorkgroupMembers - totalWithoutActivities;
			return returnedList;
		}

		public SelectFrom<EPTimeActivitiesSummary>
			.LeftJoin<EPCompanyTreeMember>.On<EPTimeActivitiesSummary.contactID.IsEqual<EPCompanyTreeMember.contactID>>
			.Where<EPTimeActivitiesSummary.workgroupID.IsEqual<EPWeeklyCrewTimeActivity.workgroupID.FromCurrent>
				.And<EPTimeActivitiesSummary.week.IsEqual<EPWeeklyCrewTimeActivity.week.FromCurrent>>>.View CompanyTreeMembers;
		public virtual IEnumerable companyTreeMembers()
		{
			BqlCommand cmd = CompanyTreeMembers.View.BqlSelect;
			if (Filter.Current.ShowAllMembers == false)
			{
				cmd = cmd.WhereAnd<Where<EPTimeActivitiesSummary.status, Equal<WorkgroupMemberStatusAttribute.permanentActive>,
					Or<EPTimeActivitiesSummary.status, Equal<WorkgroupMemberStatusAttribute.temporaryActive>>>>();
			}

			List<object> results = new PXView(this, false, cmd).SelectMulti();
			if (!results.Any())
			{
				return WorkgroupTimeSummary.Select();
			}
			return results.Cast<PXResult<EPTimeActivitiesSummary, EPCompanyTreeMember>>().Distinct(x => ((EPTimeActivitiesSummary)x).ContactID);
		}

		public PXFilter<EPWeeklyCrewTimeActivityFilter> Filter;
		/// Using EPActivityApprove2 to have a different cache than <see cref="TimeActivities" /> data view.
		public SelectFrom<EPActivityApprove2>.View BulkEntryTimeActivities;
		public IEnumerable bulkEntryTimeActivities()
		{
			return Caches[typeof(EPActivityApprove2)].Inserted;
		}

		#endregion Data Views and delegates

		#region Actions

		public PXAction<EPWeeklyCrewTimeActivity> EnterBulkTime;
		[PXButton]
		[PXUIField(DisplayName = "Mass Enter Time", Enabled = true)]
		public virtual void enterBulkTime()
		{
			var dialogResult = CompanyTreeMembers.AskExt();
			if (dialogResult == WebDialogResult.OK)
			{
				insertForBulkTimeEntry();
			}
		}

		public PXAction<EPWeeklyCrewTimeActivity> InsertForBulkTimeEntry;
		[PXButton]
		[PXUIField(DisplayName = "Add")]
		public virtual void insertForBulkTimeEntry()
		{
			IEnumerable<EPTimeActivitiesSummary> selectedMembers = CompanyTreeMembers.Select().FirstTableItems.Where(x => x.Selected == true);
			foreach (EPActivityApprove activity in BulkEntryTimeActivities.Select())
			{
				foreach (EPTimeActivitiesSummary contact in selectedMembers)
				{
					EPActivityApprove copy = TimeActivities.Insert();
					Guid? noteID = copy.NoteID;
					TimeActivities.Cache.RestoreCopy(copy, activity);
					TimeActivities.Cache.SetValueExt<EPActivityApprove.ownerID>(copy, contact.ContactID);
					copy.NoteID = noteID;
					TimeActivities.Update(copy);
				}
			}

			Caches[typeof(EPActivityApprove2)].Clear();
		}

		public PXAction<EPWeeklyCrewTimeActivity> CopySelectedActivity;
		[PXButton]
		[PXUIField(DisplayName = "Copy Selected Line")]
		public virtual void copySelectedActivity()
		{
			EPActivityApprove original = TimeActivities.Current;
			EPActivityApprove copy = TimeActivities.Insert();
			Guid? noteID = copy.NoteID;
			TimeActivities.Cache.RestoreCopy(copy, original);
			copy.NoteID = noteID;
			TimeActivities.Update(copy);
		}

		public PXAction<EPWeeklyCrewTimeActivity> LoadLastWeekActivities;
		[PXButton]
		[PXUIField(DisplayName = "Load Activities from Previous Week")]
		public virtual void loadLastWeekActivities()
		{
			if (Document.Current.WorkgroupID != null && Document.Current.Week != null)
			{
				foreach (EPActivityApprove activity in SelectFrom<EPActivityApprove>
						.Where<EPActivityApprove.workgroupID.IsEqual<EPWeeklyCrewTimeActivity.workgroupID.FromCurrent>
							.And<EPActivityApprove.weekID.IsEqual<P.AsInt>>>.View.Select(this, GetLastWeekID()))
				{
					EPActivityApprove copy = TimeActivities.Insert();
					Guid? noteID = copy.NoteID;
					TimeActivities.Cache.RestoreCopy(copy, activity);
					copy.NoteID = noteID;
					copy.Date = activity.Date.Value.AddDays(7);
					copy.WeekID = Document.Current.Week.Value;
					copy.Hold = true;
					copy.TimeCardCD = null;
					TimeActivities.Update(copy);
				}
			}
		}

		public PXAction<EPWeeklyCrewTimeActivity> LoadLastWeekMembers;
		[PXButton]
		[PXUIField(DisplayName = "Load Workgroup from Previous Week")]
		public virtual void loadLastWeekMembers()
		{
			if (Document.Current.WorkgroupID != null && Document.Current.Week != null)
			{
				HashSet<int?> currentMembers = WorkgroupTimeSummary.Select().FirstTableItems.Select(x => x.ContactID).ToHashSet();
				foreach (EPTimeActivitiesSummary summary in SelectFrom<EPTimeActivitiesSummary>
						.Where<EPTimeActivitiesSummary.workgroupID.IsEqual<EPWeeklyCrewTimeActivity.workgroupID.FromCurrent>
							.And<EPTimeActivitiesSummary.week.IsEqual<P.AsInt>>>.View.Select(this, GetLastWeekID()))
				{
					if (!currentMembers.Contains(summary.ContactID))
					{
						var newMember = new EPTimeActivitiesSummary();
						newMember.ContactID = summary.ContactID;
						newMember.Week = Document.Current.Week.Value;
						newMember.WorkgroupID = Document.Current.WorkgroupID.Value;
						WorkgroupTimeSummary.Insert(newMember);
					}
				}
			}
		}

		public PXAction<EPWeeklyCrewTimeActivity> DeleteMember;
		[PXButton]
		[PXUIField]
		public virtual void deleteMember()
		{
			if (WorkgroupTimeSummary.Current.IsWithoutActivities == true)
			{
			WorkgroupTimeSummary.Delete(WorkgroupTimeSummary.Current);
			}
			else
			{
				throw new PXException(Messages.CantDeleteWithActivities);
			}
		}

		public PXAction<EPWeeklyCrewTimeActivity> CompleteAllActivities;
		[PXButton]
		[PXUIField(DisplayName = "Complete Activities")]
		public virtual void completeAllActivities()
		{
			foreach (EPActivityApprove activity in TimeActivities.Select().FirstTableItems.Where(x => x.ApprovalStatus == ActivityStatusAttribute.Open))
			{
				activity.Hold = false;
				activity.ApprovalStatus = ActivityStatusAttribute.Completed;
				TimeActivities.Update(activity);
			}
		}

		#endregion Actions

		#region CacheAttached

		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXDefault]
		[PXUIField(DisplayName = "Employee")]
		public virtual void _(Events.CacheAttached<EPActivityApprove.ownerID> e) { }

		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXDefault(typeof(EPWeeklyCrewTimeActivity.workgroupID))]
		public virtual void _(Events.CacheAttached<EPActivityApprove.workgroupID> e) { }

		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXDefault(typeof(EPWeeklyCrewTimeActivity.workgroupID))]
		public virtual void _(Events.CacheAttached<EPActivityApprove2.workgroupID> e) { }

		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXDefault(typeof(Search<PMProject.contractID, Where<PMProject.nonProject, Equal<True>>>))]
		public virtual void _(Events.CacheAttached<EPActivityApprove.projectID> e) { }

		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXRemoveBaseAttribute(typeof(EPActivityDefaultWeekAttribute))]
		[PXDefault(typeof(EPWeeklyCrewTimeActivity.week))]
		public virtual void _(Events.CacheAttached<EPActivityApprove.weekID> e) { }

		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXDefault(PersistingCheck = PXPersistingCheck.Null)]
		[PRWeekDaySelector(typeof(EPWeeklyCrewTimeActivity.week))]
		public virtual void _(Events.CacheAttached<EPActivityApprove.date> e) { }

		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXRemoveBaseAttribute(typeof(PXSelectorAttribute))]
		public virtual void _(Events.CacheAttached<EPActivityApprove.summary> e) { }

		#endregion CacheAttached

		#region Events

		public virtual void _(Events.FieldVerifying<EPActivityApprove.date> e)
		{
			EPActivityApprove activity = e.Row as EPActivityApprove;
			PXWeekSelector2Attribute.WeekInfo weekInfo = PXWeekSelector2Attribute.GetWeekInfo(this, Document.Current.Week.Value);
			if (!weekInfo.IsValid((DateTime)e.NewValue))
			{
				e.NewValue = null;
				e.Cache.RaiseExceptionHandling<EPActivityApprove.date>(activity, activity.Date,
					new PXSetPropertyException(EP.Messages.WeekDateRangeException, PXErrorLevel.Error));
			}
		}

		public virtual void _(Events.RowSelected<EPActivityApprove> e)
		{
			if (e.Row == null)
			{
				return;
			}

			PXUIFieldAttribute.SetError<EPActivityApprove.date>(e.Cache, e.Row, e.Row.Date == null ? string.Format(ErrorMessages.FieldIsEmpty, Messages.DateTimeField) : null);
			PXUIFieldAttribute.SetEnabled(e.Cache, e.Row, e.Row.ApprovalStatus == ActivityStatusListAttribute.Open);
			e.Cache.Adjust<PXUIFieldAttribute>(e.Row).ForAllFields(field => field.Enabled = e.Row.ApprovalStatus == ActivityStatusListAttribute.Open)
				.For<EPActivityApprove.approvalStatus>(field => field.Enabled = false)
				.For<EPActivityApprove.hold>(field => field.Enabled = e.Row.ApprovalStatus == ActivityStatusListAttribute.Open
					|| e.Row.ApprovalStatus == ActivityStatusListAttribute.Completed);
		}

		public virtual void _(Events.FieldDefaulting<EPActivityApprove.projectID> e)
		{
			if (Filter.Current.ProjectID != null)
			{
				e.NewValue = Filter.Current.ProjectID;
			}
		}

		public virtual void _(Events.FieldDefaulting<EPActivityApprove.projectTaskID> e)
		{
			if (Filter.Current.ProjectTaskID != null)
			{
				e.NewValue = Filter.Current.ProjectTaskID;
			}
		}

		public virtual void _(Events.FieldDefaulting<EPActivityApprove2.projectID> e)
		{
			if (Filter.Current.ProjectID != null)
			{
				e.NewValue = Filter.Current.ProjectID;
			}
		}

		public virtual void _(Events.FieldDefaulting<EPActivityApprove2.projectTaskID> e)
		{
			if (Filter.Current.ProjectTaskID != null)
			{
				e.NewValue = Filter.Current.ProjectTaskID;
			}
		}

		public virtual void _(Events.FieldDefaulting<EPActivityApprove2.date> e)
		{
			e.NewValue = GetWeekDateFromDay(Filter.Current.Day);
		}

		public virtual void _(Events.FieldVerifying<EPActivityApprove2, EPActivityApprove2.date> e)
		{
			EPActivityApprove2 massEntryTime = e.Row;
			PXWeekSelector2Attribute.WeekInfo weekInfo = PXWeekSelector2Attribute.GetWeekInfo(this, Document.Current.Week.Value);
			if (!weekInfo.IsValid((DateTime)e.NewValue))
			{
				e.NewValue = null;
				e.Cache.RaiseExceptionHandling<EPActivityApprove2.date>(massEntryTime, massEntryTime.Date,
					new PXSetPropertyException(EP.Messages.WeekDateRangeException, PXErrorLevel.Error));
			}
		}

		public virtual void _(Events.FieldSelecting<EPActivityApprove.hold> e)
		{
			EPActivityApprove row = (EPActivityApprove)e.Row;
			if (row != null)
			{
				e.ReturnValue = row.ApprovalStatus == ActivityStatusListAttribute.Open;
			}
		}

		public virtual void _(Events.FieldDefaulting<EPWeeklyCrewTimeActivity.week> e)
		{
			var row = e.Row as EPWeeklyCrewTimeActivity;

			if (row?.WorkgroupID == null)
			{
				return;
			}

			EPWeeklyCrewTimeActivity lastDocument = SelectFrom<EPWeeklyCrewTimeActivity>.Where<EPWeeklyCrewTimeActivity.workgroupID.IsEqual<P.AsInt>>
				.AggregateTo<Max<EPWeeklyCrewTimeActivity.week>>.View.Select(this, row.WorkgroupID).TopFirst;
			if (lastDocument?.Week != null)
			{
				e.NewValue = PXWeekSelector2Attribute.GetNextWeekID(this, lastDocument.Week.Value);
			}
			else 
			{
				e.NewValue = PXWeekSelector2Attribute.GetWeekID(this, Accessinfo.BusinessDate.Value);
			}
		}

		protected virtual void _(Events.FieldUpdated<EPTimeActivitiesSummary, EPTimeActivitiesSummary.contactID> e)
		{
			EPTimeActivitiesSummary row = e.Row;

			if (row != null && row.ContactID != null)
			{
				BAccount account = PXSelect<BAccount>.Search<BAccount.defContactID>(e.Cache.Graph, e.NewValue);
				e.Cache.SetValue<EPTimeActivitiesSummary.employeeStatus>(row, account.VStatus);
				PXUIFieldAttribute.SetWarning<EPTimeActivitiesSummary.contactID>(e.Cache, row, account.VStatus == VendorStatus.Inactive ? Messages.InactiveMemberTimeEntry : null);
			}
		}

		public virtual void _(Events.RowSelected<EPTimeActivitiesSummary> e)
		{
			if (e.Row != null)
			{
				PXUIFieldAttribute.SetWarning<EPTimeActivitiesSummary.contactID>(e.Cache, e.Row, e.Row.EmployeeStatus == VendorStatus.Inactive ? Messages.InactiveMemberTimeEntry : null);
				PXUIFieldAttribute.SetWarning<EPTimeActivitiesSummary.totalRegularTime>(e.Cache, e.Row, e.Row.IsWithoutActivities == true ? Messages.WithoutActivities : null);
			}
		}

		public virtual void _(Events.RowSelected<EPWeeklyCrewTimeActivity> e)
		{
			if (e.Row != null)
			{
				bool isHeaderFilled = e.Row.WorkgroupID != null && e.Row.Week != null;
				CompleteAllActivities.SetEnabled(isHeaderFilled);

				TimeActivities.AllowInsert = isHeaderFilled;
				TimeActivities.AllowUpdate = isHeaderFilled;
				EnterBulkTime.SetEnabled(isHeaderFilled);
				LoadLastWeekActivities.SetEnabled(isHeaderFilled);

				WorkgroupTimeSummary.AllowInsert = isHeaderFilled;
				WorkgroupTimeSummary.AllowUpdate = isHeaderFilled;
				LoadLastWeekMembers.SetEnabled(isHeaderFilled);
			}
		}

		public virtual void _(Events.FieldDefaulting<EPActivityApprove, EPActivityApprove.date> e)
		{
			e.NewValue = GetWeekDateFromDay(Filter.Current.Day);
		}

		public virtual void _(Events.FieldUpdated<EPActivityApprove.earningTypeID> e)
		{
			e.Cache.SetDefaultExt<EPActivityApprove.labourItemID>(e.Row);
		}

		public virtual void _(Events.FieldUpdated<EPActivityApprove.projectID> e)
		{
			if (e.Cache.GetStatus(e.Row) == PXEntryStatus.Inserted || e.Cache.GetStatus(e.Row) == PXEntryStatus.Updated)
			{
			e.Cache.SetDefaultExt<EPActivityApprove.certifiedJob>(e.Row);
			}
			e.Cache.SetDefaultExt<EPActivityApprove.labourItemID>(e.Row);
		}

		public virtual void _(Events.FieldUpdated<EPActivityApprove2.projectID> e)
		{
			if (e.Cache.GetStatus(e.Row) == PXEntryStatus.Inserted || e.Cache.GetStatus(e.Row) == PXEntryStatus.Updated)
			{
			e.Cache.SetDefaultExt<EPActivityApprove2.certifiedJob>(e.Row);
		}
		}

		public virtual void _(Events.FieldUpdated<EPActivityApprove.ownerID> e)
		{
			e.Cache.Current = e.Row;
			e.Cache.SetDefaultExt<EPActivityApprove.labourItemID>(e.Row);
		}

		public void _(Events.RowPersisting<EPActivityApprove2> e)
		{
			e.Cancel = true;
		}

		#endregion Events

		#region Helpers

		private EPActivityApprove CopyActivity(EPActivityApprove activity)
		{
			var copy = TimeActivities.Cache.CreateCopy(activity) as EPActivityApprove;
			copy.NoteID = null;
			return copy;
		}

		private int GetLastWeekID()
		{
			DateTime startDate = PXWeekSelector2Attribute.GetWeekStartDate(this, Document.Current.Week.Value);
			return PXWeekSelector2Attribute.GetWeekID(this, startDate.AddDays(-1));
		}

		public virtual DateTime? GetWeekDateFromDay(int? dayOfWeek)
		{
			if (dayOfWeek == null)
			{
				return null;
			}

			PRWeekDaySelector.WeekDate weekDate = PRWeekDaySelector.GetWeekDates(this, Document.Current.Week.Value).SingleOrDefault(x => (int)x.Date.Value.DayOfWeek == dayOfWeek);
			return weekDate.Date;
		}

		#endregion Helpers

		#region PXImportAttribute.IPXPrepareItems Implementation

		public virtual bool PrepareImportRow(string viewName, IDictionary keys, IDictionary values)
		{
			if (viewName == nameof(TimeActivities) && values[nameof(PMTimeActivity.OwnerID)] != null)
			{
				BAccount baccount = SelectFrom<BAccount>.Where<BAccount.acctCD.IsEqual<P.AsString>>.View.Select(this, values[nameof(PMTimeActivity.OwnerID)].ToString()).TopFirst;
				values[nameof(PMTimeActivity.OwnerID)] = baccount.DefContactID;
			}

			return true;
		}

		public virtual bool RowImporting(string viewName, object row)
		{
			return true;
		}

		public virtual bool RowImported(string viewName, object row, object oldRow)
		{
			return true;
		}

		public virtual void PrepareItems(string viewName, IEnumerable items)
		{
			return;
		}

		#endregion PXImportAttribute.IPXPrepareItems Implementation

		#region Avoid breaking changes for 2020R2
		[Obsolete(Common.Messages.ItemIsObsoleteAndWillBeRemoved2022R2)]
		public virtual void _(Events.RowInserted<EPWeeklyCrewTimeActivity> e) { }
		#endregion Avoid breaking changes for 2020R2

		#region Avoid breaking changes for 2021R2
		[Obsolete("This item has been deprecated and will be removed in Acumatica ERP 2023 R1.")]
		public virtual void _(Events.RowInserting<EPActivityApprove> e) { }
		#endregion Avoid breaking changes for 2021R2

	}

	/// <summary>
	/// Required to duplicate EPActivityApprove cache for Bulk Time Entry popup.
	/// </summary>
	[PXHidden]
	public class EPActivityApprove2 : EPActivityApprove
	{
		public new abstract class workgroupID : PX.Data.BQL.BqlInt.Field<workgroupID> { }

		#region NoteID
		public new abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }
		[PXDBGuid(true, IsKey = true)]
		public override Guid? NoteID { get; set; }
		#endregion

		#region ProjectID
		public new abstract class projectID : PX.Data.BQL.BqlInt.Field<projectID> { }
		[EPActivityProjectDefault(typeof(isBillable))]
		[EPTimeCardProject]
		[PXFormula(typeof(
			Switch<
				Case<Where<Not<FeatureInstalled<FeaturesSet.projectModule>>>, DefaultValue<projectID>,
				Case<Where<isBillable, Equal<True>, And<Current2<projectID>, Equal<NonProject>>>, Null,
				Case<Where<isBillable, Equal<False>, And<Current2<projectID>, IsNull>>, DefaultValue<projectID>>>>,
			projectID>))]
		[PXDefault(typeof(Search<PMProject.contractID, Where<PMProject.nonProject, Equal<True>>>))]
		public override Int32? ProjectID { get; set; }
		#endregion

		#region ProjectTaskID
		public new abstract class projectTaskID : PX.Data.BQL.BqlInt.Field<projectTaskID> { }
		[ProjectTask(typeof(projectID), BatchModule.TA, DisplayName = "Project Task")]
		public override int? ProjectTaskID { get; set; }
		#endregion

		#region Summary
		public abstract class summary : PX.Data.BQL.BqlString.Field<summary> { }

		[PXDBString(Common.Constants.TranDescLength, InputMask = "", IsUnicode = true)]
		[PXDefault]
		[PXUIField(DisplayName = "Summary", Visibility = PXUIVisibility.SelectorVisible)]
		[PXFieldDescription]
		public override string Summary { get; set; }
		#endregion

		#region Date
		public new abstract class date : PX.Data.BQL.BqlDateTime.Field<date> { }
		[PXDBDateAndTime(BqlField = typeof(PMTimeActivity.date), DisplayNameDate = "Date", DisplayNameTime = "Time")]
		[PXUIField(DisplayName = "Date")]
		public override DateTime? Date { get; set; }
		#endregion

		#region WeekID
		public new abstract class weekID : PX.Data.BQL.BqlInt.Field<weekID> { }

		[PXDBInt(BqlField = typeof(PMTimeActivity.weekID))]
		[PXUIField(DisplayName = "Time Card Week", Enabled = false)]
		[PXWeekSelector2()]
		[PXFormula(typeof(Default<date, trackTime>))]
		[PXDefault(typeof(EPWeeklyCrewTimeActivity.week))]
		public override int? WeekID { get; set; }
		#endregion
	}

	public class PRWeekDaySelector : PXCustomSelectorAttribute
	{
		protected Type WeekField;
		public PRWeekDaySelector(Type weekField) : base(typeof(WeekDate.date))
		{
			DescriptionField = typeof(WeekDate.day);
			WeekField = weekField;
			ValidateValue = false;
		}

		public IEnumerable GetRecords()
		{
			int currentWeek = (int)GetValue(_Graph, _Graph.Caches[BqlCommand.GetItemType(WeekField)].Current, WeekField);
			return GetWeekDates(_Graph, currentWeek);
		}

		public static IEnumerable<WeekDate> GetWeekDates(PXGraph graph, int weekID)
		{
			DateTime startDate = PXWeekSelector2Attribute.IsCustomWeek(graph) ? PXWeekSelector2Attribute.GetWeekStartDate(graph, weekID) :
				PXWeekSelectorAttribute.GetWeekStartDate(weekID);

			for (DateTime date = startDate; date < startDate.AddDays(7); date = date.AddDays(1))
			{
				yield return new WeekDate() { Date = date, Day = PXLocalizer.Localize(date.DayOfWeek.ToString()) };
			}
		}

		private static object GetValue(PXGraph graph, object row, Type field)
		{
			return graph.Caches[BqlCommand.GetItemType(field)].GetValue(row, field.Name);
		}

		#region Avoid breaking changes for 2022R2
		[Obsolete(Common.Messages.ItemIsObsoleteAndWillBeRemoved2022R2)]
		public static IEnumerable<WeekDate> GetWeekDates(int weekID)
		{
			DateTime startDate = PXWeekSelectorAttribute.GetWeekStartDate(weekID);
			for (DateTime date = startDate; date < startDate.AddDays(7); date = date.AddDays(1))
			{
				yield return new WeekDate() { Date = date, Day = PXLocalizer.Localize(date.DayOfWeek.ToString()) };
			}
		}
		#endregion Avoid breaking changes

		[PXHidden]
		public class WeekDate : IBqlTable
		{
			#region Date
			public abstract class date : PX.Data.BQL.BqlDateTime.Field<date> { }
			[PXDate]
			public virtual DateTime? Date { get; set; }
			#endregion

			#region Day
			public abstract class day : PX.Data.BQL.BqlString.Field<day> { }
			[PXString]
			public virtual string Day { get; set; }
			#endregion
		}
	}
}
