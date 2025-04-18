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
using System.Linq;
using PX.Data;
using PX.Data.ProjectDefinition.Workflow;
using PX.Objects.BQLConstants;
using PX.Objects.Common;
using PX.Common;
using PX.Objects.CA;
using PX.Objects.CS;
using PX.Objects.GL.Overrides.ScheduleMaint;

namespace PX.Objects.GL
{
	public class ScheduleMaint : ScheduleMaintBase<ScheduleMaint, ScheduleProcess>, IWorkflowExpressionParserParametersProvider
	{
		public Dictionary<string, ExpressionParameterInfo> GetAvailableExpressionParameters()
		{
			return new Dictionary<string, ExpressionParameterInfo>()
			{
				{
					"ScheduleID", new ExpressionParameterInfo()
					{
						Value = Schedule_Header.Current?.ScheduleID
					}
				}
			};
		}
		#region Views
		public PXSelect<
			Batch,
			Where<
				Batch.scheduleID, Equal<Current<Schedule.scheduleID>>,
				And<Batch.scheduled, Equal<False>>>>
			Batch_History;

		public PXSelect<
			BatchSelection,
			Where<
				BatchSelection.scheduleID, Equal<Current<Schedule.scheduleID>>,
				And<BatchSelection.scheduled, Equal<True>>>>
			Batch_Detail;

		public PXSelect<
			Batch,
			Where<Batch.module, Equal<Required<Batch.module>>,
				And<Batch.batchNbr, Equal<Required<Batch.batchNbr>>>>>
			_dummyBatch;

		public PXSelect<GLTran> GLTransactions;
		[Obsolete("Will be removed in Acumatica 2019R1")]
		public PXSelect<CATran> CATransactions;

		public PXSetup<GLSetup> GLSetup;

		#endregion

		public ScheduleMaint()
		{
			GLSetup gls = GLSetup.Current;

			Batch_History.Cache.AllowDelete = false;
			Batch_History.Cache.AllowInsert = false;
			Batch_History.Cache.AllowUpdate = false;

			Schedule_Header.WhereAnd<Where<Schedule.module, Equal<BatchModule.moduleGL>>>();

			//this.Views.Caches.Remove(typeof(BatchSelection));
			PXNoteAttribute.ForceRetain<BatchSelection.noteID>(Batch_Detail.Cache);
			this.EnsureCachePersistence<Batch>();
		}

		internal override bool AnyScheduleDetails() => Batch_Detail.Any();

		public PXAction<Schedule> viewBatchD;
		[PXUIField(DisplayName = Messages.ViewBatch, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton]
		public virtual IEnumerable ViewBatchD(PXAdapter adapter)
		{
			Batch row = Batch_Detail.Current;
			if (row != null)
			{
				JournalEntry graph = CreateInstance<JournalEntry>();
				graph.BatchModule.Current = (BatchSelection)Batch_Detail.Cache.CreateCopy((BatchSelection)row);
				throw new PXRedirectRequiredException(graph, true, "View Batch") { Mode = PXBaseRedirectException.WindowMode.NewWindow };
			}
			return adapter.Get();
		}

		public PXAction<Schedule> viewBatch;
		[PXUIField(DisplayName = Messages.ViewBatch, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton]
		public virtual IEnumerable ViewBatch(PXAdapter adapter)
		{
			Batch row = Batch_History.Current;
			if (row != null)
			{
				JournalEntry graph = CreateInstance<JournalEntry>();
				graph.BatchModule.Current = row;
				throw new PXRedirectRequiredException(graph, true, "View Batch") { Mode = PXBaseRedirectException.WindowMode.NewWindow };
			}
			return adapter.Get();
		}

		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXUIField(DisplayName = "Module", Enabled = false, IsReadOnly = true, Visible = false)]
		protected virtual void Batch_Module_CacheAttached(PXCache sender) { }

		[PXDBLong]
		[Overrides.ScheduleMaint.GLCashTranID]
		protected virtual void GLTran_CATranID_CacheAttached(PXCache sender) { }

		protected virtual void BatchSelection_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			if (e.Row == null || string.IsNullOrWhiteSpace(((BatchSelection)e.Row).BatchNbr)) return;

			PXUIFieldAttribute.SetEnabled<BatchSelection.module>(sender, e.Row, !(bool)((BatchSelection)e.Row).Scheduled);
			PXUIFieldAttribute.SetEnabled<BatchSelection.batchNbr>(sender, e.Row, !(bool)((BatchSelection)e.Row).Scheduled);
		}

		protected override void SetControlsState(PXCache cache, Schedule s)
		{
			base.SetControlsState(cache, s);

			PXUIFieldAttribute.SetEnabled<BatchSelection.module>(Batch_Detail.Cache, null, true);
			PXUIFieldAttribute.SetEnabled<BatchSelection.batchNbr>(Batch_Detail.Cache, null, true);
			PXUIFieldAttribute.SetEnabled<BatchSelection.ledgerID>(Batch_Detail.Cache, null, false);
			PXUIFieldAttribute.SetEnabled<BatchSelection.dateEntered>(Batch_Detail.Cache, null, false);
			PXUIFieldAttribute.SetEnabled<BatchSelection.finPeriodID>(Batch_Detail.Cache, null, false);
			PXUIFieldAttribute.SetEnabled<BatchSelection.curyControlTotal>(Batch_Detail.Cache, null, false);
			PXUIFieldAttribute.SetEnabled<BatchSelection.curyID>(Batch_Detail.Cache, null, false);
		}

		protected virtual void BatchSelection_RowUpdated(PXCache cache, PXRowUpdatedEventArgs e)
		{
			Batch b = e.Row as Batch;
			if (b != null && b.Voided == false)
			{
				b.Scheduled = true;
				b.ScheduleID = Schedule_Header.Current.ScheduleID;
			}

			BatchSelection batch = e.Row as BatchSelection;
			if (batch != null && !string.IsNullOrWhiteSpace(batch.Module) && !string.IsNullOrWhiteSpace(batch.BatchNbr) &&
				PXSelectorAttribute.Select<BatchSelection.batchNbr>(cache, batch) == null)
			{
				cache.RaiseExceptionHandling<BatchSelection.batchNbr>(batch, batch.BatchNbr, new PXSetPropertyException(Messages.BatchNbrNotValid));
				Batch_Detail.Cache.Remove(batch);
			}
		}

		protected virtual void BatchSelection_RowInserted(PXCache sender, PXRowInsertedEventArgs e)
		{
			BatchSelection batch = e.Row as BatchSelection;

			if (batch != null && 
				!string.IsNullOrWhiteSpace(batch.Module) && 
				!string.IsNullOrWhiteSpace(batch.BatchNbr))
			{
                batch = PXSelectReadonly<
					BatchSelection,
                    Where<
						BatchSelection.module, Equal<Required<BatchSelection.module>>,
						And<BatchSelection.batchNbr, Equal<Required<BatchSelection.batchNbr>>>>>
					.Select(this, batch.Module, batch.BatchNbr);

                PXSelectorAttribute selectorAttr = (PXSelectorAttribute)sender.GetAttributesReadonly<BatchSelection.batchNbr>(batch).FirstOrDefault(
					(PXEventSubscriberAttribute attr) => { return attr is PXSelectorAttribute; });

				BqlCommand selectorSearch = selectorAttr.GetSelect();

				if (batch != null && selectorSearch.Meet(sender, batch))
				{
					Batch_Detail.Delete(batch);
					Batch_Detail.Update(batch);
				}
				else
				{
					batch = (BatchSelection)e.Row;
					sender.RaiseExceptionHandling<BatchSelection.batchNbr>(batch, batch.BatchNbr, new PXSetPropertyException(Messages.BatchNbrNotValid));
					Batch_Detail.Delete(batch);
				}
			}
		}
		protected virtual void BatchSelection_RowPersisting(PXCache cache, PXRowPersistingEventArgs e)
		{
			e.Cancel = true;

			if (e.Operation != PXDBOperation.Delete)
			{
				BatchSelection document = e.Row as BatchSelection;
				if (document != null)
				{
					Batch updatedBatch = _dummyBatch.Cache.Updated.RowCast<Batch>()
													.FirstOrDefault(p => p.Module == document.Module && p.BatchNbr == document.BatchNbr);
					if (updatedBatch == null)
					{
						updatedBatch = _dummyBatch.Select(document.Module, document.BatchNbr);
						_dummyBatch.Cache.SetStatus(updatedBatch, PXEntryStatus.Updated);

						PXDBTimestampAttribute batchTimestampAttribute = _dummyBatch.Cache
							.GetAttributesOfType<PXDBTimestampAttribute>(null, nameof(Batch.Tstamp))
							.First();
						var verifyTimestamp = batchTimestampAttribute.VerifyTimestamp;
						batchTimestampAttribute.VerifyTimestamp = VerifyTimestampOptions.FromRecord;
						try
						{
							_dummyBatch.Cache.PersistUpdated(updatedBatch);
						}
						finally
						{
							batchTimestampAttribute.VerifyTimestamp = verifyTimestamp;
						}
					}
				}
			}
		}
		
		protected virtual void Schedule_RowDeleting(PXCache sender, PXRowDeletingEventArgs e)
		{
			foreach (BatchSelection document in PXSelect<
					BatchSelection, 
					Where<
						BatchSelection.scheduleID, Equal<Required<Schedule.scheduleID>>>>
				.Select(this, ((Schedule)e.Row).ScheduleID))
			{
				Batch_Detail.Delete(document);
			}
		}

		protected virtual void Batch_RowPersisted(PXCache sender, PXRowPersistedEventArgs e)
		{
			if (e.TranStatus == PXTranStatus.Open && e.Operation == PXDBOperation.Update)
			{
				// Acuminator disable once PX1043 SavingChangesInEventHandlers [Should be done by automation]
				// Acuminator disable once PX1045 PXGraphCreateInstanceInEventHandlers [Should be done by automation]
				// Acuminator disable once PX1071 PXActionExecutionInEventHandlers [Should be done by automation]
				CleanScheduleBatch(e.Row as Batch);
			}
		}
		private void CleanScheduleBatch(Batch batch)
		{
			JournalEntry je = PX.Common.PXContext.GetSlot<JournalEntry>();
			if (je == null)
			{
				je = PXGraph.CreateInstance<JournalEntry>();
				PX.Common.PXContext.SetSlot<JournalEntry>(je);
			}
			je.Clear();
			je.SelectTimeStamp();

			Batch scheduledBatch = PXSelect<
				Batch,
				Where<
					Batch.batchType, Equal<Required<Batch.batchType>>,
					And<Batch.batchNbr, Equal<Required<Batch.batchNbr>>>>>
				.Select(je, batch.BatchType, batch.BatchNbr);

			je.BatchModule.Current = scheduledBatch;

			je.Approval.Select()
				.RowCast<EP.EPApproval>()
				.ForEach(a => je.Approval.Delete(a));

			je.Save.Press();
		}
		protected virtual void Schedule_RowPersisted(PXCache sender, PXRowPersistedEventArgs e)
		{
			if (e.TranStatus == PXTranStatus.Open)
			{
				foreach (BatchSelection document in Batch_Detail.Cache.Cached)
				{
					var state = Batch_Detail.Cache.GetStatus(document);
					if (state == PXEntryStatus.Inserted || state == PXEntryStatus.Updated)
					{
						if (document.Voided != true)
						{
							Batch.Events
								.Select(ev => ev.ConfirmSchedule)
								.FireOn(this, document, Schedule_Header.Current);
							UpdateTransactionsLedgerBalanceType(document);
						}
					}

					if (state == PXEntryStatus.Deleted)
					{
						Batch.Events
							.Select(ev => ev.VoidSchedule)
							.FireOn(this, document, Schedule_Header.Current);
						//I have no idea, why event placed the original item into the cache instead of updated.
						Batch batch = this.Batch_History.Locate(document);
						if(batch != null)
							this.Batch_History.Cache.RestoreCopy(batch, document);
					}
				}
			}
			if(e.TranStatus == PXTranStatus.Completed)
			{
				Batch_Detail.Cache.Clear();
				Batch_Detail.View.Clear();
			}
		}

		private void UpdateTransactionsLedgerBalanceType(Batch updatedBatch)
		{
			foreach (GLTran glTransaction in PXSelect<GLTran,
									Where<GLTran.module, Equal<Current<Batch.module>>,
										And<GLTran.batchNbr, Equal<Current<Batch.batchNbr>>>>>
									.SelectMultiBound(this, new object[] { updatedBatch }))
			{
				glTransaction.LedgerBalanceType = "N";

				GLTransactions.Update(glTransaction);
			}
		}
	}
}

namespace PX.Objects.GL.Overrides.ScheduleMaint
{
	public class GLCashTranIDAttribute : GL.GLCashTranIDAttribute
	{
		public override CATran DefaultValues(PXCache sender, CATran catran_Row, object orig_Row)
		{
			return DefaultValues<BatchSelection, BatchSelection.module, BatchSelection.batchNbr>(sender, catran_Row, orig_Row);
		}
	}

	[PXPrimaryGraph(null)]
    [PXCacheName(Messages.BatchSelection)]
    [Serializable]
	public partial class BatchSelection : Batch
	{
		#region Module
		public new abstract class module : PX.Data.BQL.BqlString.Field<module> { }
		[PXDBString(2, IsKey = true, IsFixed = true)]
		[PXDefault(BatchModule.GL)]
		[PXUIField(DisplayName = "Module", Visibility = PXUIVisibility.SelectorVisible, Enabled = false, Visible = false, IsReadOnly = true)]
		public override String Module
		{
			get
			{
				return this._Module;
			}
			set
			{
				this._Module = value;
			}
		}
		#endregion		
		#region BatchNbr
		public new abstract class batchNbr : PX.Data.BQL.BqlString.Field<batchNbr> { }
		[PXDBString(15, IsUnicode = true, IsKey = true, InputMask = ">CCCCCCCCCCCCCCC")]
		[PXDefault]
		[PXSelector(typeof(Search2<
			Batch.batchNbr, 
				LeftJoin<GLVoucher, 
					On<GLVoucher.refNoteID, Equal<Batch.noteID>, 
					And<FeatureInstalled<FeaturesSet.gLWorkBooks>>>>, 
			Where<
				Batch.released, Equal<False>, 
				And<Batch.hold, Equal<False>, 
				And<Batch.voided, Equal<False>,
				And<Batch.rejected, NotEqual<True>,
				And<Batch.module, Equal<BatchModule.moduleGL>, 
				And<Batch.batchType, NotEqual<BatchTypeCode.allocation>,
				And<Batch.batchType, NotEqual<BatchTypeCode.reclassification>,
				And<Batch.batchType, NotEqual<BatchTypeCode.trialBalance>,
				And<GLVoucher.refNbr, IsNull>>>>>>>>>>))]
		[PXUIField(DisplayName = "Batch Number", Visibility = PXUIVisibility.SelectorVisible)]
		public override string BatchNbr
		{
			get
			{
				return this._BatchNbr;
			}
			set
			{
				this._BatchNbr = value;
			}
		}
		#endregion
		#region ScheduleID
		public new abstract class scheduleID : PX.Data.BQL.BqlString.Field<scheduleID> { }
		[PXDBString(15, IsUnicode = true)]
		[PXDBDefault(typeof(Schedule.scheduleID))]
		[PXParent(typeof(Select<Schedule, Where<Schedule.scheduleID, Equal<Current<Batch.scheduleID>>>>), LeaveChildren=true)]
		public override string ScheduleID
		{
			get
			{
				return this._ScheduleID;
			}
			set
			{
				this._ScheduleID = value;
			}
		}
		#endregion
		#region CuryInfoID
		public new abstract class curyInfoID : PX.Data.BQL.BqlLong.Field<curyInfoID> { }
		[PXDBLong()]
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
		#region FinPeriodID
		public new abstract class finPeriodID : PX.Data.BQL.BqlString.Field<finPeriodID> { }
		[FinPeriodID(
			typeof(Batch.dateEntered),
			typeof(Batch.branchID),
			masterFinPeriodIDType: typeof(Batch.tranPeriodID),
			IsHeader = true)]
		[PXDefault()]
		[PXUIField(DisplayName = "Post Period", Visibility = PXUIVisibility.Visible)]
		public override String FinPeriodID
		{
			get
			{
				return this._FinPeriodID;
			}
			set
			{
				this._FinPeriodID = value;
			}
		}
		#endregion
		#region TranPeriodID
		[PeriodID]		
		public new abstract class tranPeriodID : PX.Data.BQL.BqlString.Field<tranPeriodID> { }
		#endregion
		#region Released
		public new abstract class released : PX.Data.BQL.BqlBool.Field<released> { }
		#endregion
		#region Hold
		public new abstract class hold : PX.Data.BQL.BqlBool.Field<hold> { }
		#endregion
		#region Scheduled
		public new abstract class scheduled : PX.Data.BQL.BqlBool.Field<scheduled> { }
		#endregion
		#region Voided
		public new abstract class voided : PX.Data.BQL.BqlBool.Field<voided> { }
		#endregion
		#region CreatedByID
		public new abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID> { }
		#endregion
		#region LastModifiedByID
		public new abstract class lastModifiedByID : PX.Data.BQL.BqlGuid.Field<lastModifiedByID> { }
		#endregion
	}
}
