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
using PX.Objects.CR;
using PX.TM;
using System;
using System.Collections;
using System.Threading;
using static PX.Objects.EP.EPApprovalHelper;

namespace PX.Objects.EP
{
	public class ReassignDelegatedActivitiesProcess : PXGraph<ReassignDelegatedActivitiesProcess>
	{
		#region Filter
		/// <exclude/>
		[Serializable]
		[PXBreakInheritance]
		[PXProjection(typeof(SelectFrom<EPWingmanForApprovals>
								.InnerJoin<BAccount>
									.On<
											EPWingmanForApprovals.employeeID.IsEqual<BAccount.bAccountID>
										.And<BAccount.type.IsEqual<BAccountType.employeeType>>>
									.Where<
											EPWingmanForApprovals.startsOn.IsLessEqual<EPApprovalHelper.PXTimeZoneInfoToday.dayEnd>
										.And<Where<
												EPWingmanForApprovals.expiresOn.IsNull
												.Or<EPWingmanForApprovals.expiresOn.IsGreaterEqual<EPApprovalHelper.PXTimeZoneInfoToday.dayBegin>>>>
										.Or<EPWingmanForApprovals.startsOn.IsGreater<EPApprovalHelper.PXTimeZoneInfoToday.dayEnd>>>
		))]
		[PXHidden]
		public partial class TodayDelegates : EPWingmanForApprovals
		{
			#region ContactID
			public abstract class contactID : PX.Data.BQL.BqlInt.Field<contactID> { }
			[PXDBInt(BqlTable = typeof(BAccount), BqlField = typeof(BAccount.defContactID))]
			public Int32? ContactID { get; set; }
			#endregion

			public new abstract class recordID : PX.Data.BQL.BqlInt.Field<recordID> { }
		}

		/// <exclude/>
		[Serializable]
		[PXHidden]
		[PXProjection(typeof(SelectFrom<EPApproval>
								.LeftJoin<Note>
									.On<Note.noteID.IsEqual<EPApproval.refNoteID>>
								.InnerJoin<EPRule>
									.On<EPApproval.FK.Rule>
								.LeftJoin<TodayDelegates>
									.On<
										TodayDelegates.contactID.IsEqual<EPApproval.ownerID>
										.Or<TodayDelegates.recordID.IsEqual<EPApproval.delegationRecordID>>>
								.Where<
										EPApproval.status.IsEqual<EPApprovalStatus.pending>
									.And<EPRule.ruleID.IsNotNull>
									.And<Where2<
												TodayDelegates.contactID.IsNotNull
											.And<EPApproval.ignoreDelegations.IsEqual<False>>
											.And<
												Where<
														EPApproval.delegationRecordID.IsNull
													.Or<EPApproval.delegationRecordID.IsNotEqual<TodayDelegates.recordID>>>>,
										Or<
												EPApproval.delegationRecordID.IsNotNull
											.And<TodayDelegates.recordID.IsNull>>>>>
		))]
		public partial class EPApprovalWingmanFilter : EPApproval
		{
			#region Non DB - calculated fields

			#region Selected
			public abstract class selected : PX.Data.BQL.BqlBool.Field<selected> { }
			[PXBool]
			[PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
			[PXUIField(DisplayName = "Selected")]
			public virtual bool? Selected { get; set; }
			#endregion
			#region DelegationOf
			public abstract class delegationOf : PX.Data.BQL.BqlString.Field<delegationOf> { }
			[PXString(1, IsFixed = true)]
			[EPDelegationOf.ListApprovalsOnly()]
			[PXUIField(DisplayName = "Delegation Of")]
			public virtual string DelegationOf { get => EPDelegationOf.Approvals; }
			#endregion

			#endregion

			#region Note

			#region EntityType
			public new abstract class entityType : PX.Data.BQL.BqlString.Field<entityType> { }
			[PXDBString(BqlField = typeof(Note.entityType))]
			public string EntityType { get; set; }
			#endregion

			#endregion

			#region EPApproval

			#region OrigOwnerID
			public new abstract class origOwnerID : PX.Data.BQL.BqlInt.Field<origOwnerID> { }
			[Owner(DisplayName = "Original Assignee", Visibility = PXUIVisibility.SelectorVisible)]
			public override int? OrigOwnerID { get; set; }
			#endregion
			#region OwnerID
			public new abstract class ownerID : PX.Data.BQL.BqlInt.Field<ownerID> { }
			[Owner(DisplayName = "Current Assignee", Visibility = PXUIVisibility.SelectorVisible)]
			public override int? OwnerID { get; set; }
			#endregion
			#region CreatedDateTime
			public new abstract class createdDateTime : PX.Data.BQL.BqlDateTime.Field<createdDateTime> { }
			[PXDBDate(PreserveTime = true, DisplayMask = "g")]
			[PXUIField(DisplayName = "Requested Time")]
			public override DateTime? CreatedDateTime { get; set; }
			#endregion

			#region DocType
			public new abstract class docType : PX.Data.BQL.BqlString.Field<docType> { }
			[PXString()]
			[PXFormula(typeof(ApprovalDocType<entityType, EPApproval.sourceItemType>))]
			[PXUIField(DisplayName = "Document Type")]
			public override string DocType { get; set; }
			#endregion

			#endregion

			#region TodayDelegates

			#region DelegatedToContactID
			public abstract class delegatedToContactID : PX.Data.BQL.BqlInt.Field<delegatedToContactID> { }
			[PXDBInt(BqlField = typeof(TodayDelegates.wingmanID))]
			[PXEPEmployeeSelector]
			[PXUIField(DisplayName = "Delegated To", Visibility = PXUIVisibility.SelectorVisible)]
			public Int32? DelegatedToContactID { get; set; }
			#endregion
			#region Starts On
			public abstract class startsOn : PX.Data.BQL.BqlDateTime.Field<startsOn> { }
			[PXDBDate(BqlField = typeof(TodayDelegates.startsOn))]
			[PXUIField(DisplayName = "Starts On", Visibility = PXUIVisibility.SelectorVisible)]
			[PXUIEnabled(typeof(Where<delegationOf.IsEqual<EPDelegationOf.approvals>>))]
			public virtual DateTime? StartsOn { get; set; }
			#endregion
			#region Expires On
			public abstract class expiresOn : PX.Data.BQL.BqlDateTime.Field<expiresOn> { }
			[PXDBDate(BqlField = typeof(TodayDelegates.expiresOn))]
			[PXUIField(DisplayName = "Expires On", Visibility = PXUIVisibility.SelectorVisible)]
			[PXUIEnabled(typeof(Where<delegationOf.IsEqual<EPDelegationOf.approvals>>))]
			public virtual DateTime? ExpiresOn { get; set; }
			#endregion
			#region IsActive
			public abstract class isActive : PX.Data.BQL.BqlBool.Field<isActive> { }
			[PXDBBool(BqlField = typeof(TodayDelegates.isActive))]
			[PXDefault(true)]
			[PXUIField(DisplayName = "Active")]
			public virtual bool? IsActive { get; set; }
			#endregion

			#endregion

		}
		#endregion

		#region Selects
		[PXHidden]
		public PXSelect<EPApproval> dummyEPApproval;

		[PXFilterable]
		public PXProcessing<EPApprovalWingmanFilter> Records;

		#endregion

		#region Ctors

		public ReassignDelegatedActivitiesProcess()
		{
			Records.SetSelected<EPApprovalWingmanFilter.selected>();
			Records.SetProcessDelegate<ReassignDelegatedActivitiesProcess>(StartProcessing);
		}

		#endregion

		#region Event Handlers

		#region CacheAttached

		#endregion

		#endregion

		#region Public Methods

		public static void StartProcessing(ReassignDelegatedActivitiesProcess graph, EPApprovalWingmanFilter item, CancellationToken cancellationToken)
		{
			graph.ProcessApprovalDelegate(item, cancellationToken);
		}

		public virtual void ProcessApprovalDelegate(EPApprovalWingmanFilter item, CancellationToken cancellationToken)
		{
			if (cancellationToken.IsCancellationRequested || item == null) return;

			PXProcessing<EPApprovalWingmanFilter>.SetCurrentItem(item);

			try
			{
				if (item?.DelegatedToContactID == null)
				{
					item.DelegationRecordID = null;
					item.IgnoreDelegations = false;
					EPApprovalHelper.ReassignToDelegate(this, item, item.OrigOwnerID);
				}
				else
				{
					EPApprovalHelper.ReassignToDelegate(this, item, item.OwnerID);
				}
			}
			catch(PXReassignmentApproverNotAvailableException ex)
			{
				throw new PXSetPropertyException(ex, PXErrorLevel.RowError, MessagesNoPrefix.ReassignmentDelegateNotAvailable);
			}

			this.Caches[typeof(EPApproval)].Update(item);

			this.Persist();

			PXProcessing<EPApprovalWingmanFilter>.SetInfo(ActionsMessages.RecordProcessed);
		}

		#endregion

	}
}
