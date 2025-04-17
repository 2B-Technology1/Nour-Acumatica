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
using System.Collections.Generic;
using System.Web.Compilation;
using PX.Data;
using PX.Objects.CR;
using PX.SM;
using PX.TM;
using PX.Objects.CM;
using PX.Common;
using PX.Data.BQL.Fluent;
using PX.Data.BQL;

namespace PX.Objects.EP
{

	#region ReassignApprovalFilter
	/// <exclude/>
	[Serializable]
	[PXHidden]
	public partial class ReassignApprovalFilter : IBqlTable
	{
		#region New Approver
		public abstract class newApprover : PX.Data.BQL.BqlInt.Field<newApprover> { }

		[Owner(DisplayName = "New Approver", Required = true)]
		[PXDefault(PersistingCheck = PXPersistingCheck.NullOrBlank)]
		public virtual int? NewApprover { get; set; }
		#endregion

		#region Ignore Approver's Delegations
		public abstract class ignoreApproversDelegations : PX.Data.BQL.BqlBool.Field<ignoreApproversDelegations> { }
		[PXBool()]
		[PXUIField(DisplayName = "Ignore Approver's Delegations")]
		[PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual bool? IgnoreApproversDelegations { get; set; }
		#endregion
	}
	#endregion

	[PX.Objects.GL.TableAndChartDashboardType]
	public class EPApprovalProcess : PXGraph<EPApprovalProcess>
	{
		public PXSelect<BAccount> bAccount;

		[Serializable]
        [PXProjection(typeof(Select2<EPApproval, LeftJoin<Note,
										On<Note.noteID, Equal<EPApproval.refNoteID>
									 >>,
									 Where<EPApproval.status, Equal<EPApprovalStatus.pending>,
										And<Where2<
												Where2<
													Where<EPApproval.ownerID, IsNotNull, And<EPApproval.ownerID, Equal<CurrentValue<AccessInfo.contactID>>>>,
													Or<Where<EPApproval.origOwnerID, IsNotNull, And<EPApproval.origOwnerID, Equal<CurrentValue<AccessInfo.contactID>>>>>>,
											Or<EPApproval.workgroupID, IsWorkgroupOfContact<CurrentValue<AccessInfo.contactID>>,
											Or<EPApproval.workgroupID, IsWorkgroupOrSubgroupOfContact<CurrentValue<AccessInfo.contactID>>>>
										>>>>), persistent: new Type[] { typeof(EPApproval) }, Persistent = true)]
		[PXHidden]
		public partial class EPOwned : EPApproval
		{
			#region RefNoteID
			public new abstract class refNoteID : PX.Data.BQL.BqlGuid.Field<refNoteID> { }
			[PXRefNote(BqlTable = typeof(EPApproval), LastKeyOnly=true)]
			[PXUIField(DisplayName = "Reference Nbr.")]
			[PXNoUpdate]
			public override Guid? RefNoteID
			{
				get
				{
					return this._RefNoteID;
				}
				set
				{
					this._RefNoteID = value;
				}
			}
			#endregion
			#region Selected
			public abstract class selected : PX.Data.BQL.BqlBool.Field<selected> { }
			protected bool? _Selected = false;
			[PXBool]
			[PXDefault(false)]
			[PXUIField(DisplayName = "Selected")]
			public virtual bool? Selected
			{
				get
				{
					return _Selected;
				}
				set
				{
					_Selected = value;
				}
			}
			#endregion
			#region PendingWaitTime
			public new abstract class pendingWaitTime : PX.Data.BQL.BqlInt.Field<pendingWaitTime> { }
			[PXInt]
			[PXUIField(DisplayName = "Pending Wait Time", Visibility = PXUIVisibility.Visible)]
			[PXDBCalced(typeof(IIf<Where<EPApproval.status, Equal<EPApprovalStatus.pending>>, DateDiff<EPApproval.createdDateTime, Now, DateDiff.minute>, Zero>), typeof(int))]
			public override int? PendingWaitTime
			{
				get
				{
					return this._PendingWaitTime;
				}
				set
				{
					this._PendingWaitTime = value;
				}
			}
			#endregion
			#region Escalated
			public abstract class escalated : PX.Data.BQL.BqlBool.Field<escalated> { }
			
			[PXBool]
			[PXUIField(DisplayName = "Escalated")]
			//[PXDependsOnFields(typeof(pendingWaitTime), typeof(waitTime))]
			[PXDBCalced(typeof(IIf<Where
				<IIf<Where<EPApproval.status, Equal<EPApprovalStatus.pending>>,
					DateDiff<EPApproval.createdDateTime, Now, DateDiff.minute>,
					Zero>, GreaterEqual<EPApproval.waitTime>>,
				True,
				False>), typeof(bool))]
			public virtual bool? Escalated
			{
				get;
				set;
			}
			#endregion

			#region BAccountID
			public new abstract class bAccountID : PX.Data.BQL.BqlInt.Field<bAccountID> { }
			[PXDBInt(BqlTable = typeof(EPApproval))]
			[PXUIField(DisplayName = "Business Account")]
			[PXSelector(typeof(BAccount.bAccountID), SubstituteKey = typeof(BAccount.acctCD), DescriptionField = typeof(BAccount.acctName))]
			public override Int32? BAccountID
			{
				get
				{
					return this._BAccountID;
				}
				set
				{
					this._BAccountID = value;
				}
			}
			#endregion
			#region CuryInfoID
			public new abstract class curyInfoID : PX.Data.BQL.BqlLong.Field<curyInfoID> { }
			[PXDBLong(BqlTable = typeof(EPApproval))]
			[CM.CurrencyInfo()]
			public override Int64? CuryInfoID
			{
				get
				{
					return this._CuryInfoID;
				}
				set
				{
					this._CuryInfoID = value;
				}
			}
			#endregion			
			#region CuryTotalAmount
			public new abstract class curyTotalAmount : PX.Data.BQL.BqlDecimal.Field<curyTotalAmount> { }
            [PXDBCurrency(typeof(EPOwned.curyInfoID), typeof(EPOwned.totalAmount), BqlTable = typeof(EPApproval))]
			[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
			[PXUIField(DisplayName = "Total Amount")]
			public override Decimal? CuryTotalAmount
			{
				get
				{
					return this._CuryTotalAmount;
				}
				set
				{
					this._CuryTotalAmount = value;
				}
			}
			#endregion
			#region TotalAmount
			public new abstract class totalAmount : PX.Data.BQL.BqlDecimal.Field<totalAmount> { }
			
			[PXDBDecimal(4, BqlTable = typeof(EPApproval))]
			[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
			public override Decimal? TotalAmount
			{
				get
				{
					return this._TotalAmount;
				}
				set
				{
					this._TotalAmount = value;
				}
			}
			#endregion                        
			#region CreatedDateTime
			public new abstract class createdDateTime : PX.Data.BQL.BqlDateTime.Field<createdDateTime> { }
			[PXDBDate(PreserveTime = true, DisplayMask = "g", BqlTable = typeof(EPApproval))]
			[PXUIField(DisplayName = "Requested Time")]
			public override DateTime? CreatedDateTime
			{
				get
				{
					return this._CreatedDateTime;
				}
				set
				{
					this._CreatedDateTime = value;
				}
			}
			#endregion
			#region EntityType
			public abstract class entityType : PX.Data.BQL.BqlString.Field<entityType> { }
			private string _EntityType;
			[PXDBString(BqlTable = typeof(Note))]
			public string EntityType
			{
				get
				{
					return _EntityType;
				}
				set
				{
					_EntityType = value;
				}
			}
			#endregion
			#region DocType
			public new abstract class docType : PX.Data.BQL.BqlString.Field<docType> { }
			private string _DocType;
			[PXString()]
			[PXUIField(DisplayName = "Type")]
			[PXFormula(typeof(ApprovalDocType<EPOwned.entityType, EPOwned.sourceItemType>))]
			public override string DocType
			{
				get
				{
					return _DocType;
				}
				set
				{
					_DocType = value;
				}
			}
			#endregion
			#region NoteID
			[PXNote]
			public override Guid? NoteID
			{
				get
				{
					return base.NoteID;
				}
				set
				{
					base.NoteID = value;
				}
			}
			#endregion
		}

		public PXCancel<EPOwned> Cancel;

        public PXAction<EPOwned> EditDetail;
        [PXUIField(DisplayName = "", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXEditDetailButton]
        public virtual IEnumerable editDetail(PXAdapter adapter)
        {
            if (Records.Current != null && Records.Current.RefNoteID != null)
            {
				bool navigate = true;

                EntityHelper helper = new EntityHelper(this);
				Note note = helper.SelectNote(Records.Current.RefNoteID);

				if (note != null && note.EntityType == typeof(EPExpenseClaim).FullName)
				{
					EPExpenseClaim claim = PXSelect<EPExpenseClaim, Where<EPExpenseClaim.noteID, Equal<Required<EPExpenseClaim.noteID>>>>.Select(this, note.NoteID);

					if (claim != null)
					{
						ExpenseClaimEntry graph = PXGraph.CreateInstance<ExpenseClaimEntry>();
						EPExpenseClaim target = graph.ExpenseClaim.Search<EPExpenseClaim.refNbr>(claim.RefNbr);
						if (target == null)
						{
							navigate = false;
						}

					}
					else
					{
						using (new PXReadBranchRestrictedScope())
						{
							claim = PXSelect<EPExpenseClaim, Where<EPExpenseClaim.noteID, Equal<Required<EPExpenseClaim.noteID>>>>.Select(this, note.NoteID);
							if (claim != null)
								throw new PXException(Messages.YouAreNotAlowedToViewTheDetails);
						}
					}
				}

				if (note != null && note.EntityType == typeof(EPTimeCard).FullName)
				{
					EPTimeCard timecard = PXSelect<EPTimeCard, Where<EPTimeCard.noteID, Equal<Required<EPTimeCard.noteID>>>>.Select(this, note.NoteID);

					if (timecard != null)
					{
						TimeCardMaint graph = PXGraph.CreateInstance<TimeCardMaint>();
						EPTimeCard target = graph.Document.Search<EPTimeCard.timeCardCD>(timecard.TimeCardCD);
						if (target == null)
						{
							navigate = false;
						}
						
					}
				}
						
				if (navigate)
                helper.NavigateToRow(Records.Current.RefNoteID.Value, PXRedirectHelper.WindowMode.InlineWindow);
            }
            return adapter.Get();
        }

		public class PXApprovalProcessing<Table, Where, OrderBy> : PXProcessing<Table, Where, OrderBy>
			where Table : class, IBqlTable, new()
			where Where : IBqlWhere, new()
			where OrderBy : IBqlOrderBy, new()
		{
			public PXApprovalProcessing(PXGraph graph)
				: this(graph, null)
			{
			}

			public PXApprovalProcessing(PXGraph graph, Delegate handler)
				: base(graph, handler)
			{				
			}
			public string ConfirmationTitle = null;
			public string ConfirmationMessage = null;
			[PXProcessButton]
			[PXUIField(DisplayName = ActionsMessages.ProcessAll, MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update)]
			protected override IEnumerable ProcessAll(PXAdapter adapter)
			{
				if (ConfirmationMessage != null && adapter.ExternalCall)
				{
					if (!PXLongOperation.Exists(_Graph.UID) && this.Ask(ConfirmationTitle, ConfirmationMessage, MessageButtons.YesNo) != WebDialogResult.Yes)
						return adapter.Get();
				}
				return base.ProcessAll(adapter);				
					
			}

			protected override void _PrepareGraph<Table>()
			{
				base._PrepareGraph<Table>();
				this._ScheduleButton.SetVisible(false);
			}

			public string ScheduleActionKey => _ScheduleActionKey;
			public string ProcessActionKey => _ProcessActionKey;
			public string ProcessAllActionKey => _ProcessAllActionKey;
		}

		[PXFilterable]
		public PXApprovalProcessing<EPOwned,Where<True, Equal<True>>, OrderBy<Desc<EPOwned.docDate, Asc<EPOwned.approvalID>>>> Records;

		public PXSetup<EPSetup> EPSetup;

		[PXHidden]
		public PXSelect<PM.PMProject> Projects;

		public EPApprovalProcess()
		{
			Records.SetProcessCaption(EP.Messages.Approve);
			Records.SetProcessAllCaption(EP.Messages.ApproveAll);
			Records.SetSelected<EPOwned.selected>();
			Records.SetProcessDelegate(list => Approve(list, true));
			Records.ConfirmationTitle = Messages.Approve;
			Records.ConfirmationMessage = Messages.ApproveAllConfirmation;
			this.Actions.Move(nameof(RejectAll), Records.ScheduleActionKey, true);

			this.Reject.StateSelectingEvents += new PXFieldSelecting(delegate (PXCache sender, PXFieldSelectingEventArgs e)
			{
				if (PXLongOperation.GetStatus(this.UID) != PXLongRunStatus.NotExists )
				{
					e.ReturnState = PXButtonState.CreateInstance(e.ReturnState, null, null, null, null, null, false,
						PXConfirmationType.Unspecified, null, null, null, null, null, null, null, null, null, null, null, typeof(EPApproval));
					((PXButtonState)e.ReturnState).Enabled = false;
				}
			});
			this.RejectAll.StateSelectingEvents += new PXFieldSelecting(delegate (PXCache sender, PXFieldSelectingEventArgs e)
			{
				if (PXLongOperation.GetStatus(this.UID) != PXLongRunStatus.NotExists)
				{
					e.ReturnState = PXButtonState.CreateInstance(e.ReturnState, null, null, null, null, null, false,
						PXConfirmationType.Unspecified, null, null, null, null, null, null, null, null, null, null, null, typeof(EPApproval));
					((PXButtonState)e.ReturnState).Enabled = false;
				}
			});
		}

        public override bool IsProcessing
        {
            get { return false;}
            set { }
        }

		public PXAction<EPOwned> Reject;
		[PXProcessButton(Category = "")]
		[PXUIField(DisplayName = Messages.Reject, MapViewRights = PXCacheRights.Update)]
		public IEnumerable reject(PXAdapter adapter)
		{
			return RunReject(adapter, false);
		}
		public PXAction<EPOwned> RejectAll;
		[PXProcessButton(Category = Messages.ApprovalCategory)]
		[PXUIField(DisplayName = Messages.RejectAll, MapViewRights = PXCacheRights.Update)]
		public IEnumerable rejectAll(PXAdapter adapter)
		{
			return RunReject(adapter, true);
		}

		private IEnumerable RunReject(PXAdapter adapter, bool  all)
		{
			Records.ConfirmationTitle = Messages.Reject;
			Records.ConfirmationMessage = Messages.RejectAllConfirmation;
			Records.SetProcessDelegate(list => Approve(list, false));
			foreach (var record in this.Actions[all ? Records.ProcessAllActionKey: Records.ProcessActionKey].Press(adapter))
				yield return record;
		}
	    public virtual IEnumerable records()
	    {
	        Records.Cache.AllowInsert = false;
	        Records.Cache.AllowDelete = false;

	        PXSelectBase<EPOwned> select =
				new PXSelect<EPOwned,
				    Where<True, Equal<True>>,				    					
					OrderBy<Desc<EPOwned.docDate, Asc<EPOwned.approvalID>>>> (this);

			select.View.Clear();

			int startRow = PXView.StartRow;
			int totalRows = 0;
			foreach (EPOwned doc in select.View.Select(PXView.Currents, null, PXView.Searches, PXView.SortColumns, PXView.Descendings, PXView.Filters, ref startRow, PXView.MaximumRows, ref totalRows))
	        {
	            yield return doc;
	        }
			PXView.StartRow = 0;
		    
	    }

		#region Reassign Approval

		public PXFilter<ReassignApprovalFilter> reassignApprovalFilter;

		public PXAction<EPOwned> ReassignApproval;
		[PXProcessButton(Category = Messages.ApprovalCategory)]
		[PXUIField(DisplayName = Messages.ReassignApproval, MapViewRights = PXCacheRights.Update)]
		public virtual IEnumerable reassignApproval(PXAdapter adapter)
		{
			if (reassignApprovalFilter.AskExt() == WebDialogResult.OK)
			{
				if (!reassignApprovalFilter.VerifyRequired())
					return adapter.Get();

				ReassignApprovalFilter filter = reassignApprovalFilter.Current;
				Records.ConfirmationTitle = Messages.ReassignApproval;
				Records.ConfirmationMessage = Messages.RejectAllConfirmation;
				Records.SetProcessDelegate(
					(item, cancellationToken) =>
						{
							if (cancellationToken.IsCancellationRequested) return;
							PXProcessing<EPApproval>.SetCurrentItem(item);

							EPApprovalProcess graph = PXGraph.CreateInstance<EPApprovalProcess>();

							EPApprovalHelper.ReassignToContact(graph, item, filter.NewApprover, filter.IgnoreApproversDelegations);

							item.Selected = false;

							graph.Caches[typeof(EPOwned)].MarkUpdated(item);

							graph.Persist();

							PXProcessing<EPApproval>.SetInfo(ActionsMessages.RecordProcessed);
						}
				);
				Actions[Records.ProcessActionKey].PressButton();
			}

			return adapter.Get();
		}

		#endregion

		protected virtual void EPOwned_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			EPOwned row = e.Row as EPOwned;
			if (row != null && row.Selected != true)
				sender.SetStatus(row, PXEntryStatus.Notchanged);
		}

		public override bool IsDirty
		{
			get
			{
				return false;
			}
		}

		protected static void Approve(List<EPOwned> items, bool approve)
		{
			EntityHelper helper = new EntityHelper(new PXGraph());
			var graphs = new Dictionary<Type, PXGraph>();

			bool errorOccured = false;
			foreach (EPOwned item in items)
			{
				try
				{
					PXProcessing<EPApproval>.SetCurrentItem(item);
					if (item.RefNoteID == null) throw new PXException(Messages.ApprovalRefNoteIDNull);
					object row = helper.GetEntityRow(item.RefNoteID.Value, true);

					if (row == null) throw new PXException(Messages.ApprovalRecordNotFound);

					Type cahceType = row.GetType();
					Type graphType = helper.GetPrimaryGraphType(ref cahceType, ref row, false);
					PXGraph graph;
					if(!graphs.TryGetValue(graphType, out graph))
					{
						graphs.Add(graphType, graph = PXGraph.CreateInstance(graphType));
					}

					EPApproval approval = PXSelectReadonly<EPApproval,
							Where<EPApproval.approvalID, Equal<Current<EPOwned.approvalID>>>>
						.SelectSingleBound(graph, new object[] { item });
					if (approval.Status == EPApprovalStatus.Pending)
					{

						graph.Clear();
						graph.Caches[cahceType].Current = row;
						graph.Caches[cahceType].SetStatus(row, PXEntryStatus.Notchanged);
						
						string approved = typeof(EPExpenseClaim.approved).Name;

						string actionName = approve ? nameof(Approve) : nameof(Reject);
						if (graph.Actions.Contains(actionName))
							graph.Actions[actionName].Press();
						else 
						{
							PXView view = graph.Views[graph.PrimaryView];
							BqlCommand select = view.BqlSelect;
							PXAdapter adapter = new PXAdapter(new PXView.Dummy(graph, select, new List<object> {row}));
							adapter.Menu = actionName;
							if (graph.Actions.Contains("Action"))
							{
								if (!CheckRights(graphType, cahceType))
									throw new PXException(Messages.DontHaveAppoveRights);
								foreach (var i in graph.Actions["Action"].Press(adapter)) ;
							}
							else
							{
								throw new PXException(PXMessages.LocalizeFormatNoPrefixNLA(Messages.AutomationNotConfigured, graph));
							}
							//PXAutomation.ApplyAction(graph, graph.Actions["Action"], "Approve", row, out rollback);							
						}
						// else if (graph.Caches[cahceType].Fields.Contains(approved))
						// {
						// 	object upd = graph.Caches[cahceType].CreateCopy(row);
						// 	graph.Caches[cahceType].SetValue(upd, approved, true);
						// 	graph.Caches[cahceType].Update(upd);
						// }
						graph.Persist();
					}
					PXProcessing<EPApproval>.SetInfo(ActionsMessages.RecordProcessed);
				}
				catch (Exception ex)
				{
					errorOccured = true;
					PXProcessing<EPApproval>.SetError(ex);
				}
			}
			if(errorOccured)
				throw new PXOperationCompletedWithErrorException(ErrorMessages.SeveralItemsFailed);
		}

        private static bool CheckRights(Type graphType, Type cacheType)
        {
            PXCacheRights rights;
            List<string> invisible = null;
            List<string> disabled = null;

	        var node = PXSiteMap.Provider.FindSiteMapNode(graphType);
			if (node == null) return false;

			PXAccess.Provider.GetRights(node.ScreenID, graphType.Name,
                            cacheType, out rights, out invisible, out disabled);

            var actionName = "Approve@Action";
            if (disabled != null && CompareIgnoreCase.IsInList(disabled, actionName))
                return false;

            return true;
        }
	}
}
