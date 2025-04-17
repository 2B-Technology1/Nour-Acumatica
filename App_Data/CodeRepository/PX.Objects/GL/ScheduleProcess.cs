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

using PX.Objects.GL.Overrides.ScheduleProcess;
using PX.Objects.CS;
using PX.Objects.CM;
using PX.Objects.Common;
using PX.Objects.GL.FinPeriods.TableDefinition;
using PX.Objects.GL.FinPeriods;

namespace PX.Objects.GL
{
	public class ScheduleDet
	{
		public DateTime? ScheduledDate;
		public string ScheduledPeriod;
	}

	[PX.Objects.GL.TableAndChartDashboardType]
	public class ScheduleRun : ScheduleRunBase<ScheduleRun, ScheduleMaint, ScheduleProcess>
	{
		[Serializable]
		public partial class Parameters : IBqlTable
		{
			#region LimitTypeSel
			public abstract class limitTypeSel : PX.Data.BQL.BqlString.Field<limitTypeSel> { }
			[PXDBString(1, IsFixed = true)]
			[PXUIField(DisplayName = Messages.Stop, Visibility = PXUIVisibility.Visible, Required = true)]
			[PXDefault(ScheduleRunLimitType.StopAfterNumberOfExecutions)]
			[LabelList(typeof(ScheduleRunLimitType))]
			public virtual string LimitTypeSel { get; set; }
			#endregion
			#region ExecutionDate
			public abstract class executionDate : PX.Data.BQL.BqlDateTime.Field<executionDate> { }
			[PXDBDate]
			[PXDefault(typeof(AccessInfo.businessDate))]
			[PXUIField(DisplayName = Messages.ExecutionDate, Visibility = PXUIVisibility.Visible)]
			public virtual DateTime? ExecutionDate { get; set; }
			#endregion
			#region RunLimit
			public abstract class runLimit : PX.Data.BQL.BqlShort.Field<runLimit> { }
			[PXDBShort(MinValue = 1)]
			[PXUIField(DisplayName = Messages.StopAfterNumberOfExecutions, Visibility = PXUIVisibility.Visible)]
			[PXDefault((short)1, PersistingCheck = PXPersistingCheck.Nothing)]
			public virtual short? RunLimit { get; set; }
			#endregion
		}
	
		public PXSetup<GLSetup> GLSetup;

		protected override bool checkAnyScheduleDetails => false;

		public ScheduleRun()
		{
			GLSetup setup = GLSetup.Current;

			Schedule_List.WhereAnd<Where<
				Schedule.module, Equal<BatchModule.moduleGL>>>();

			Schedule_List.WhereAnd<Where<Exists<
				Select<Batch,
				Where<Batch.scheduleID, Equal<Schedule.scheduleID>,
					And<Batch.scheduled, Equal<True>>>>>>>();
		}

		public PXAction<Parameters> EditDetail;
		[PXUIField(DisplayName = "", Visible = false, MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update)]
        [PXEditDetailButton]
		public virtual IEnumerable editDetail(PXAdapter adapter) => ViewScheduleAction(adapter);

		[Obsolete("This method has been moved to " + nameof(ScheduleRunBase))]
		public static void SetProcessDelegate<ProcessGraph>(PXGraph graph, ScheduleRun.Parameters filter, PXProcessing<Schedule> view)
			where ProcessGraph : PXGraph<ProcessGraph>, IScheduleProcessing, new()
			=> ScheduleRunBase.SetProcessDelegate<ProcessGraph>(graph, filter, view);
	}

	public class ScheduleProcess : PXGraph<ScheduleProcess>, IScheduleProcessing
	{
		public PXSelect<Schedule> Running_Schedule;
		public PXSelect<BatchNew> Batch_Created;
		public PXSelect<GLTranNew> Tran_Created;
		public PXSelect<CurrencyInfo> CuryInfo_Created;

		[InjectDependency]
		public IFinPeriodRepository FinPeriodRepository { get; set; }

		public GLSetup GLSetup
		{
			get
			{
				return PXSelect<GLSetup>.Select(this);
			}
		}

		protected virtual void BatchNew_BatchNbr_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			// TODO: Need to clarify for what purpose Cancel is set to true.
			// -
			e.Cancel = true;
		}

		[Obsolete("Please use " + nameof(Scheduler.MakeSchedule), false)]
        private List<ScheduleDet> MakeSchedule(Schedule schedule, short times, DateTime runDate)
			=> MakeSchedule(this, schedule, times, runDate);

		[Obsolete("Please use " + nameof(Scheduler.MakeSchedule), false)]
		public static List<ScheduleDet> MakeSchedule(PXGraph graph, Schedule schedule, short times)
			=> MakeSchedule(graph, schedule, times, graph.Accessinfo.BusinessDate.Value);

		[Obsolete("Please use " + nameof(Scheduler.MakeSchedule), false)]
		public static List<ScheduleDet> MakeSchedule(PXGraph graph, Schedule schedule, short times, DateTime runDate)
			=> new Scheduler(graph).MakeSchedule(schedule, times, runDate).ToList();

		[Obsolete("Please use " + nameof(Scheduler.GetNextRunDate), false)]
		public static DateTime? GetNextRunDate(PXGraph graph, Schedule schedule)
			=> new Scheduler(graph).GetNextRunDate(schedule);

		public virtual void GenerateProc(Schedule schedule)
		{
            GenerateProc(schedule, 1, Accessinfo.BusinessDate.Value);
        }

		public virtual void GenerateProc(Schedule schedule, short times, DateTime runDate)
		{
			IEnumerable<ScheduleDet> occurrences = new Scheduler(this).MakeSchedule(schedule, times, runDate);
			JournalEntry je = PXGraph.CreateInstance<JournalEntry>();

			using (PXTransactionScope ts = new PXTransactionScope())
			{
				foreach (ScheduleDet occurrence in occurrences)
				{
					foreach (PXResult<Batch, CurrencyInfo> scheduledBatchRes in PXSelectJoin<
						Batch,
						InnerJoin<CurrencyInfo, On<CurrencyInfo.curyInfoID, Equal<Batch.curyInfoID>>>,
						Where<
							Batch.scheduleID, Equal<Optional<Schedule.scheduleID>>, 
							And<Batch.scheduled,Equal<boolTrue>>>>
						.Select(this, schedule.ScheduleID))
					{
						je.Clear();
						Batch scheduledBatch = (Batch)scheduledBatchRes;
						CurrencyInfo scheduledBatchCurrencyInfo = (CurrencyInfo)scheduledBatchRes;

						Batch copy = PXCache<Batch>.CreateCopy(scheduledBatch);

						copy.OrigBatchNbr = copy.BatchNbr;
						copy.OrigModule = copy.Module;
						copy.NumberCode = "GLREC";
						copy.CuryDebitTotal = 0m;
						copy.CuryCreditTotal = 0m;
						copy.DebitTotal = 0m;
						copy.CreditTotal = 0m;
						copy.CuryControlTotal = 0m;
						copy.ControlTotal = 0m;
						copy.NoteID = null;
						copy.Posted = false;
						copy.Approved = false;
						copy.Released = false;
						copy.Scheduled = false;
						copy.AutoReverseCopy = false;
						copy.Hold = true;
						copy.Status = null;

						CurrencyInfo newCurrencyInfo = new CurrencyInfo
						{
							ModuleCode = scheduledBatchCurrencyInfo.ModuleCode,
							CuryRateTypeID = scheduledBatchCurrencyInfo.CuryRateTypeID,
							CuryID = scheduledBatchCurrencyInfo.CuryID,
							BaseCuryID = scheduledBatchCurrencyInfo.BaseCuryID,
							CuryEffDate = occurrence.ScheduledDate
						};
						newCurrencyInfo = (CurrencyInfo)je.currencyinfo.Insert(newCurrencyInfo);
						copy.CuryInfoID = newCurrencyInfo.CuryInfoID;

						copy.DateEntered = occurrence.ScheduledDate;

						FinPeriod finPeriod =
							FinPeriodRepository.GetFinPeriodByMasterPeriodID(PXAccess.GetParentOrganizationID(copy.BranchID), occurrence.ScheduledPeriod)
							.GetValueOrRaiseError();
						copy.FinPeriodID = finPeriod.FinPeriodID;
						copy.TranPeriodID = null;
						copy = (Batch) je.BatchModule.Insert(copy);

						PXNoteAttribute.CopyNoteAndFiles(je.BatchModule.Cache, scheduledBatch, je.BatchModule.Cache, copy);

						foreach (GLTran scheduledBatchTransaction in PXSelect<
							GLTran, 
							Where<
								GLTran.module, Equal<Required<GLTran.module>>, 
								And<GLTran.batchNbr, Equal<Required<GLTran.batchNbr>>>>>
							.Select(this, scheduledBatch.Module, scheduledBatch.BatchNbr))
						{
							GLTran transactionCopy = PXCache<GLTran>.CreateCopy(scheduledBatchTransaction);

							transactionCopy.OrigBatchNbr = transactionCopy.BatchNbr;
							transactionCopy.OrigModule = transactionCopy.Module;
							transactionCopy.BatchNbr = copy.BatchNbr;
							transactionCopy.CuryInfoID = copy.CuryInfoID;
							transactionCopy.CATranID = null;
							transactionCopy.NoteID = null;

							transactionCopy.TranDate = occurrence.ScheduledDate;
						    FinPeriodIDAttribute.SetPeriodsByMaster<GLTran.finPeriodID>(je.GLTranModuleBatNbr.Cache, transactionCopy, occurrence.ScheduledPeriod);

							transactionCopy = (GLTran) je.GLTranModuleBatNbr.Insert(transactionCopy);
							PXNoteAttribute.CopyNoteAndFiles(je.GLTranModuleBatNbr.Cache, scheduledBatchTransaction, je.GLTranModuleBatNbr.Cache, transactionCopy);
						}

						copy.CuryControlTotal = copy.CuryDebitTotal;
						copy.ControlTotal = copy.DebitTotal;
						copy = (Batch)je.BatchModule.Update(copy);
						je.releaseFromHold.Press();
						je.Persist();
					}

					schedule.LastRunDate = occurrence.ScheduledDate;
					Running_Schedule.Cache.Update(schedule);
				}

				Running_Schedule.Cache.Persist(PXDBOperation.Update);
				je.GLTranModuleBatNbr.Cache.Normalize();

				ts.Complete(this);
			}

			Running_Schedule.Cache.Persisted(false);
		}
	}
}

namespace PX.Objects.GL.Overrides.ScheduleProcess
{

	[System.SerializableAttribute()]
	[PXCacheName(Messages.BatchNew)]
	public partial class BatchNew : Batch
	{
		#region Module
		public new abstract class module : PX.Data.BQL.BqlString.Field<module> { }
		[PXDBString(2, IsKey = true, IsFixed = true)]
		[PXUIField(DisplayName = "Module")]
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
		[PXDefault()]
		[AutoNumber(typeof(GLSetup.batchNumberingID), typeof(BatchNew.dateEntered))]
		[PXUIField(DisplayName = "Batch Number")]
		public override String BatchNbr
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
		#region RefBatchNbr
		public abstract class refBatchNbr : PX.Data.BQL.BqlString.Field<refBatchNbr> { }
		protected string _RefBatchNbr;
		[PXString(15, IsUnicode = true)]
		public virtual string RefBatchNbr
		{
			get
			{
				return this._RefBatchNbr;
			}
			set
			{
				this._RefBatchNbr = value;
			}
		}
		#endregion
		#region DateEntered
		public new abstract class dateEntered : PX.Data.BQL.BqlDateTime.Field<dateEntered> { }
		[PXDBDate()]
		public override DateTime? DateEntered
		{
			get
			{
				return this._DateEntered;
			}
			set
			{
				this._DateEntered = value;
			}
		}
		#endregion
		#region OrigBatchNbr
		public new abstract class origBatchNbr : PX.Data.BQL.BqlString.Field<origBatchNbr> { }
		#endregion
		#region OrigModule
		public new abstract class origModule : PX.Data.BQL.BqlString.Field<origModule> { }
		#endregion
		#region TranPeriodID
		public new abstract class tranPeriodID : PX.Data.IBqlField { }

		[PeriodID]
		public override String TranPeriodID { get; set; }
		#endregion
	}

	[Serializable]
	public partial class GLTranNew : GLTran
	{
		#region Module
		public new abstract class module : PX.Data.BQL.BqlString.Field<module> { }
		[PXDBString(2, IsKey = true, IsFixed = true)]
		public override string Module
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
		[PXDBString(15, IsUnicode = true, IsKey = true)]
		[PXParent(typeof(
			Select<BatchNew, 
			Where<BatchNew.module, Equal<Current<GLTranNew.module>>, 
				And<BatchNew.batchNbr, Equal<Current<GLTranNew.batchNbr>>>>>))]
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
		#region RefBatchNbr
		public abstract class refBatchNbr : PX.Data.BQL.BqlString.Field<refBatchNbr> { }
		protected string _RefBatchNbr;
		[PXString(15, IsUnicode = true)]
		public virtual string RefBatchNbr
		{
			get
			{
				return this._RefBatchNbr;
			}
			set
			{
				this._RefBatchNbr = value;
			}
		}
		#endregion
		#region LedgerID
		[PXDBInt]
		public override int? LedgerID
		{
			get
			{
				return this._LedgerID;
			}
			set
			{
				this._LedgerID = value;
			}
		}
		#endregion
		#region AccountID
		[PXDBInt]
		public override int? AccountID
		{
			get
			{
				return this._AccountID;
			}
			set
			{
				this._AccountID = value;
			}
		}
		#endregion
		#region SubID
		[PXDBInt]
		public override int? SubID
		{
			get
			{
				return this._SubID;
			}
			set
			{
				this._SubID = value;
			}
		}
		#endregion
		#region CuryInfoID
		public new abstract class curyInfoID : PX.Data.BQL.BqlLong.Field<curyInfoID> { }
		[PXDBLong]
		public override long? CuryInfoID
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
		#region ReferenceNbr
		[PXDBString(15, IsUnicode = true)]
		public override string RefNbr
		{
			get
			{
				return this._RefNbr;
			}
			set
			{
				this._RefNbr = value;
			}
		}
		#endregion
		#region TranDesc
		[PXDBString(Common.Constants.TranDescLength, IsUnicode = true)]
		public override string TranDesc
		{
			get
			{
				return this._TranDesc;
			}
			set
			{
				this._TranDesc = value;
			}
		}
		#endregion
		#region TranDate
		public new abstract class tranDate : PX.Data.BQL.BqlDateTime.Field<tranDate> { }
		[PXDBDate]
		public override DateTime? TranDate
		{
			get
			{
				return this._TranDate;
			}
			set
			{
				this._TranDate = value;
			}
		}
		#endregion
		public new abstract class taxID : PX.Data.BQL.BqlString.Field<taxID> { }
		#region FinPeriodID
		public new abstract class finPeriodID : PX.Data.IBqlField { }

		[PXDefault]
		[FinPeriodID(
			branchSourceType: typeof(GLTranNew.branchID),
			masterFinPeriodIDType: typeof(GLTranNew.tranPeriodID),
			headerMasterFinPeriodIDType: typeof(BatchNew.tranPeriodID))]
		[PXUIField(DisplayName = "Period ID", Enabled = false, Visible = false)]
		public override String FinPeriodID { get; set; }
		#endregion
		#region TaxID

		[PXDBString(TX.Tax.taxID.Length, IsUnicode = true)]
		public override string TaxID
		{
			get
			{
				return base.TaxID;
			}

			set
			{
				base.TaxID = value;
			}
		}
		#endregion
	}
}

