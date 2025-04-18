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

using PX.CS;
using PX.Data;
using PX.Objects.AP.MigrationMode;
using PX.Objects.CM.Extensions;
using PX.Objects.Common;
using PX.Objects.Common.Discount;
using PX.Objects.CS;
using PX.Objects.GL;
using PX.Objects.GL.FinPeriods;
using PX.Objects.GL.FinPeriods.TableDefinition;
using PX.Objects.SO;
using System;
using System.Collections;
using System.Collections.Generic;

namespace PX.Objects.AP
{
	[TableAndChartDashboardType]
	public class APScheduleRun : ScheduleRunBase<APScheduleRun, APScheduleMaint, APScheduleProcess>
	{
		public APSetupNoMigrationMode APSetup;

		public PXAction<ScheduleRun.Parameters> ViewSchedule;
		[PXUIField(DisplayName = "", Visible = false, MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update)]
		[PXEditDetailButton]
		public virtual IEnumerable viewSchedule(PXAdapter adapter) => ViewScheduleAction(adapter);

		public PXAction<ScheduleRun.Parameters> NewSchedule;
		[PXUIField(DisplayName = Common.Messages.NewSchedule, MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update)]
		[PXButton]
		public virtual IEnumerable newSchedule(PXAdapter adapter)
		{
			APScheduleMaint graph = CreateInstance<APScheduleMaint>();

			graph.Schedule_Header.Insert(new Schedule());
			graph.Schedule_Header.Cache.IsDirty = false;

			throw new PXRedirectRequiredException(graph, true, Common.Messages.NewSchedule)
			{
				Mode = PXBaseRedirectException.WindowMode.NewWindow
			};
		}

		protected override bool checkAnyScheduleDetails => false;

		public APScheduleRun()
		{
			APSetup setup = APSetup.Current;

			Schedule_List.Join<
				LeftJoin<APRegisterAccess,
					On<APRegisterAccess.scheduleID, Equal<Schedule.scheduleID>,
					And<APRegisterAccess.scheduled, Equal<boolTrue>,
					And<Not<Match<APRegisterAccess, Current<AccessInfo.userName>>>>>>>>();

			Schedule_List.WhereAnd<Where<
				Schedule.module, Equal<BatchModule.moduleAP>,
				And<APRegisterAccess.docType, IsNull>>>();

			Schedule_List.WhereAnd<Where<Exists<
				Select<APRegister,
				Where<APRegister.scheduleID, Equal<Schedule.scheduleID>,
					And<APRegister.scheduled, Equal<True>>>>
			>>>();
		}
	}

	public class APScheduleProcess : PXGraph<APScheduleProcess>, IScheduleProcessing
	{
		public PXSelect<Schedule> Running_Schedule;
		public PXSelect<APTran> Tran_Created;
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

        public virtual void GenerateProc(Schedule schedule)
        {
            GenerateProc(schedule, 1, Accessinfo.BusinessDate.Value);
        }

        public virtual void GenerateProc(Schedule schedule, short times, DateTime runDate)
		{
			IEnumerable<ScheduleDet> occurrences = new Scheduler(this).MakeSchedule(schedule, times, runDate);

			APInvoiceEntry invoiceEntry = CreateGraph();

			using (PXTransactionScope transactionScope = new PXTransactionScope())
			{
				foreach (ScheduleDet occurrence in occurrences)
				{
					foreach (PXResult<APInvoice, Vendor, CurrencyInfo> scheduledInvoiceResult in PXSelectJoin<
						APInvoice, 
							InnerJoin<Vendor, On<Vendor.bAccountID,Equal<APInvoice.vendorID>>, 
							InnerJoin<CurrencyInfo, On<CurrencyInfo.curyInfoID, Equal<APInvoice.curyInfoID>>>>, 
						Where<
							APInvoice.scheduleID, Equal<Required<APInvoice.scheduleID>>, 
							And<APInvoice.scheduled, Equal<boolTrue>>>>
						.Select(this, schedule.ScheduleID))
					{
						invoiceEntry.Clear();

						invoiceEntry.vendor.Current = (Vendor)scheduledInvoiceResult;
						APInvoice scheduledInvoice = (APInvoice)scheduledInvoiceResult;
						CurrencyInfo scheduledInvoiceCurrencyInfo = (CurrencyInfo)scheduledInvoiceResult;

						if (scheduledInvoice.Released == true)
						{
							throw new PXException(AR.Messages.ScheduledDocumentAlreadyReleased);
						}

						// Cloning currency info is required because we want to preserve 
						// (and not default) the currency rate type of the template document.
						// -
						CurrencyInfo newCurrencyInfo = invoiceEntry.GetExtension<APInvoiceEntry.MultiCurrency>()
							.CloneCurrencyInfo(scheduledInvoiceCurrencyInfo, occurrence.ScheduledDate);
						APInvoice newInvoice = PXCache<APInvoice>.CreateCopy(scheduledInvoice);

						newInvoice.CuryInfoID = newCurrencyInfo.CuryInfoID;
						newInvoice.DocDate = occurrence.ScheduledDate;

						FinPeriod finPeriod =
							FinPeriodRepository.GetFinPeriodByMasterPeriodID(PXAccess.GetParentOrganizationID(newInvoice.BranchID), occurrence.ScheduledPeriod)
							.GetValueOrRaiseError();
						newInvoice.FinPeriodID = finPeriod.FinPeriodID;

						newInvoice.TranPeriodID = null;
						newInvoice.DueDate = null;
						newInvoice.DiscDate = null;
						newInvoice.PayDate = null;
						newInvoice.CuryOrigDiscAmt = null;
						newInvoice.OrigDiscAmt = null;
						newInvoice.RefNbr = null;
						newInvoice.Scheduled = false;
						newInvoice.CuryLineTotal = 0m;
						newInvoice.CuryVatTaxableTotal = 0m;
						newInvoice.CuryVatExemptTotal = 0m;
						newInvoice.NoteID = null;
						newInvoice.PaySel = false;
						newInvoice.IsTaxValid = false;
						newInvoice.IsTaxPosted = false;
						newInvoice.IsTaxSaved = false;
						newInvoice.OrigDocType = scheduledInvoice.DocType;
						newInvoice.OrigRefNbr = scheduledInvoice.RefNbr;
						newInvoice.CuryDetailExtPriceTotal = 0m;
						newInvoice.DetailExtPriceTotal = 0m;
						newInvoice.CuryLineDiscTotal = 0m;
						newInvoice.LineDiscTotal = 0m;

						KeyValueHelper.CopyAttributes(typeof(APInvoice), invoiceEntry.Document.Cache, scheduledInvoice, newInvoice);

						newInvoice = invoiceEntry.Document.Insert(newInvoice); //we insert an item here because we need DontApprove field to be set in RowSelected
						if (newInvoice.DontApprove != true)
						{
							// We always generate documents on hold
							// if approval process is enabled in AP.
							// -
							newInvoice.Hold = true;
						}

						newInvoice = invoiceEntry.Document.Update(newInvoice);

						PXNoteAttribute.CopyNoteAndFiles(Caches[typeof(APInvoice)], scheduledInvoice, invoiceEntry.Document.Cache, newInvoice);

						foreach (APTran originalLine in PXSelect<
							APTran, 
							Where<
								APTran.tranType, Equal<Required<APTran.tranType>>, 
								And<APTran.refNbr, Equal<Required<APTran.refNbr>>,
								And<Where<
									APTran.lineType, IsNull,
									Or<APTran.lineType, NotEqual<SOLineType.discount>>>>>>>
							.Select(invoiceEntry, scheduledInvoice.DocType, scheduledInvoice.RefNbr)) 
						{
							APTran newLine = PXCache<APTran>.CreateCopy(originalLine);

							newLine.FinPeriodID = null;
							newLine.TranPeriodID = null;
							newLine.RefNbr = null;
							newLine.CuryInfoID = null;
							newLine.ManualPrice = true;
							newLine.ManualDisc = true;
							newLine.NoteID = null;
							newLine = invoiceEntry.Transactions.Insert(newLine);
							newLine.Box1099 = originalLine.Box1099;

							PXNoteAttribute.CopyNoteAndFiles(Caches[typeof(APTran)], originalLine, invoiceEntry.Transactions.Cache, newLine);
						}

						foreach (APInvoiceDiscountDetail originalDiscountDetail in PXSelect<
							APInvoiceDiscountDetail,
							Where<
								APInvoiceDiscountDetail.docType, Equal<Required<APInvoiceDiscountDetail.docType>>,
								And<APInvoiceDiscountDetail.refNbr, Equal<Required<APInvoiceDiscountDetail.refNbr>>>>>
							.Select(invoiceEntry, scheduledInvoice.DocType, scheduledInvoice.RefNbr))
						{
							APInvoiceDiscountDetail newDiscountDetail = 
								PXCache<APInvoiceDiscountDetail>.CreateCopy(originalDiscountDetail);

							newDiscountDetail.RefNbr = null;
							newDiscountDetail.CuryInfoID = null;
							newDiscountDetail.IsManual = true;

							DiscountEngineProvider.GetEngineFor<APTran, APInvoiceDiscountDetail>().InsertDiscountDetail(invoiceEntry.DiscountDetails.Cache, invoiceEntry.DiscountDetails, newDiscountDetail);
						}

						BalanceCalculation.ForceDocumentControlTotals(invoiceEntry,	newInvoice);
						
						try
						{
							invoiceEntry.Save.Press();
						}
						catch
						{
							if (invoiceEntry.Document.Cache.IsInsertedUpdatedDeleted)
							{
								throw;
							}
						}
					}

					schedule.LastRunDate = occurrence.ScheduledDate;
					Running_Schedule.Cache.Update(schedule);
				}

				transactionScope.Complete(this);
			}

			using (PXTransactionScope ts = new PXTransactionScope())
			{
				Running_Schedule.Cache.Persist(PXDBOperation.Update);
				ts.Complete(this);
			}

			Running_Schedule.Cache.Persisted(false);
		}

		public virtual APInvoiceEntry CreateGraph()
		{
			return PXGraph.CreateInstance<APInvoiceEntry>();
		}
	}
}
