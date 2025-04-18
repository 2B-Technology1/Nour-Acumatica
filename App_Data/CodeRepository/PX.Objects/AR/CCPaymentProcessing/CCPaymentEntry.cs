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
using System;
using System.Linq;
using System.Collections.Generic;
using PX.Objects.AR.CCPaymentProcessing.Common;
using PX.Objects.AR.CCPaymentProcessing.Helpers;
using PX.Objects.AR.CCPaymentProcessing.Interfaces;
using PX.Objects.AR.CCPaymentProcessing.Wrappers;
using PX.Objects.AR.CCPaymentProcessing.Repositories;
using PX.Objects.Extensions.PaymentTransaction;

namespace PX.Objects.AR.CCPaymentProcessing
{
	public delegate void AfterTranProcDelegate1(PXGraph graph, CCTranType tranType, bool success);
	public delegate void AfterTranProcDelegate(PXGraph graph, IBqlTable aTable, CCTranType tranType, bool success);
	public class CCPaymentEntry
	{
		PXGraph graph;

		ICCTransactionsProcessor transactionProcessor;

		public AfterProcessingManager AfterProcessingManager { get; set; }

		ICCTransactionsProcessor TransactionProcessor {
			get
			{
				if (transactionProcessor == null)
				{
					transactionProcessor = CCTransactionsProcessor.GetCCTransactionsProcessor();
				}
				return transactionProcessor;
			}
			set
			{
				transactionProcessor = value;
			}
		}

		public bool NeedPersistAfterRecord { get; set; } = true;

		public CCPaymentEntry(PXGraph graph)
		{
			this.graph = graph;
		}

		public void AuthorizeCCpayment(ICCPayment doc, IExternalTransactionAdapter paymentTransaction)
		{
			if (doc == null || doc.CuryDocBal == null)
				return;

			var trans = paymentTransaction.Select();
			if (ExternalTranHelper.HasOpenCCProcTran(graph, trans.FirstOrDefault()))
			{
				throw new PXException(Messages.ERR_CCTransactionCurrentlyInProgress);
			}

			ExternalTransactionState state = ExternalTranHelper.GetActiveTransactionState(graph, trans);
			if (state.IsCaptured || state.IsPreAuthorized)
			{
				throw new PXException(Messages.ERR_CCPaymentAlreadyAuthorized);
			}
			if (doc.Released == false)
			{
				graph.Actions.PressSave();
			}

			ICCPayment toProc = graph.Caches[doc.GetType()].CreateCopy(doc) as ICCPayment;
			PXLongOperation.StartOperation(graph, delegate ()
			{
				bool success = true;
				try
				{
					TransactionProcessor.ProcessAuthorize(toProc, null);
				}
				catch
				{
					success = false;
					throw;
				}
				finally
				{
					if (AfterProcessingManager != null)
					{
						AfterProcessingManager.RunAuthorizeActions((IBqlTable)doc, success);
						AfterProcessingManager.PersistData();
					}
				}
			});
		}

		public void CaptureCCpayment(ICCPayment doc, IExternalTransactionAdapter paymentTransaction)
		{
			if (doc == null || doc.CuryDocBal == null)
				return;

			var trans = paymentTransaction.Select();
			if (ExternalTranHelper.HasOpenCCProcTran(graph, trans.FirstOrDefault()))
			{
				throw new PXException(Messages.ERR_CCTransactionCurrentlyInProgress);
			}

			ExternalTransactionState state = ExternalTranHelper.GetActiveTransactionState(graph, trans);

			if (state.IsCaptured)
			{
				throw new PXException(Messages.ERR_CCAuthorizedPaymentAlreadyCaptured);
			}

			var needSyncTran = ExternalTranHelper.GetImportedNeedSyncTran(graph, trans);
			if (needSyncTran != null)
			{
				throw new PXException(Messages.ERR_TransactionIsNotValidated, needSyncTran.TranNumber);
			}

			if (doc.Released == false)
			{
				graph.Actions.PressSave();
			}
	
			ICCPayment toProc = graph.Caches[doc.GetType()].CreateCopy(doc) as ICCPayment;
			IExternalTransaction tranCopy = null;
			if (state.IsPreAuthorized && !ExternalTranHelper.IsExpired(state.ExternalTransaction))
			{
				tranCopy = graph.Caches[state.ExternalTransaction.GetType()].CreateCopy(state.ExternalTransaction) as IExternalTransaction;
			}
			CCTranType operation = tranCopy != null ? CCTranType.PriorAuthorizedCapture : CCTranType.AuthorizeAndCapture;
			PXLongOperation.StartOperation(graph, delegate ()
			{
				bool success = true;
				try
				{
					if (operation == CCTranType.PriorAuthorizedCapture)
					{
						TransactionProcessor.ProcessPriorAuthorizedCapture(toProc, tranCopy);
					}
					else
					{
						TransactionProcessor.ProcessAuthorizeCapture(toProc, tranCopy);
					}
				}
				catch
				{
					success = false;
					throw;
				}
				finally
				{
					if (AfterProcessingManager != null)
					{
						if (operation == CCTranType.PriorAuthorizedCapture)
						{
							AfterProcessingManager.RunPriorAuthorizedCaptureActions((IBqlTable)doc, success);
						}
						else
						{
							AfterProcessingManager.RunCaptureActions((IBqlTable)doc, success);
						}
						AfterProcessingManager.PersistData();
					}
				}
			});
		}

		public void CaptureOnlyCCPayment(InputPaymentInfo paymentInfo, ICCPayment doc, IExternalTransactionAdapter paymentTransaction)
		{
			if (doc == null || doc.CuryDocBal == null)
			{
				return;
			}

			var trans = paymentTransaction.Select();
			if (ExternalTranHelper.HasOpenCCProcTran(graph, trans.FirstOrDefault()))
			{
				throw new PXException(Messages.ERR_CCTransactionCurrentlyInProgress);
			}

			ExternalTransactionState state = ExternalTranHelper.GetActiveTransactionState(graph, trans);
			if (string.IsNullOrEmpty(paymentInfo.AuthNumber))
			{
				throw new PXException(Messages.ERR_CCExternalAuthorizationNumberIsRequiredForCaptureOnlyTrans);
			}
			if (doc.Released == false)
			{
				graph.Actions.PressSave();
			}
			ICCPayment toProc = graph.Caches[doc.GetType()].CreateCopy(doc) as ICCPayment;
			PXLongOperation.StartOperation(graph, delegate ()
			{
				bool success = true;
				try
				{
					IExternalTransaction tran = new ExternalTransaction();
					tran.AuthNumber = paymentInfo.AuthNumber;
					TransactionProcessor.ProcessCaptureOnly(toProc, tran);
				}
				catch
				{
					success = false;
					throw;
				}
				finally
				{
					if (AfterProcessingManager != null)
					{
						AfterProcessingManager.RunCaptureOnlyActions((IBqlTable)doc, success);
						AfterProcessingManager.PersistData();
					}
				}
			});
		}

		public void VoidCCPayment(ICCPayment doc, IExternalTransactionAdapter paymentTransaction)
		{
			if (doc == null || doc.CuryDocBal == null)
			{
				return;
			}
			var trans = paymentTransaction.Select();
			if (ExternalTranHelper.HasOpenCCProcTran(graph, trans.FirstOrDefault()))
			{
				throw new PXException(Messages.ERR_CCTransactionCurrentlyInProgress);
			}

			ExternalTransactionState state = ExternalTranHelper.GetActiveTransactionState(graph, trans);
	
			if (!state.IsActive)
			{
				throw new PXException(Messages.ERR_CCNoTransactionToVoid);
			}

			if (state.IsRefunded)
			{
				throw new PXException(Messages.ERR_CCTransactionOfThisTypeInvalidToVoid);
			}

			if (ExternalTranHelper.IsExpired(state.ExternalTransaction))
			{
				throw new PXException(Messages.TransactionHasExpired);
			}

			var needSyncTran = ExternalTranHelper.GetImportedNeedSyncTran(graph, trans);
			if (needSyncTran != null)
			{
				throw new PXException(Messages.ERR_TransactionIsNotValidated, needSyncTran.TranNumber);
			}


			if (doc.Released == false)
			{
				IExternalTransaction activeTran = ExternalTranHelper.GetActiveTransaction(trans);
				if (activeTran?.ProcStatus == ExtTransactionProcStatusCode.CaptureSuccess && activeTran?.Amount != Math.Abs(doc.CuryDocBal ?? 0m))
				{
					string docLabel = TranValidationHelper.GetDocumentName(doc.DocType);
					throw new PXException(Messages.ERR_CCTheAmountDoesntMatchOriginalTransaction, doc.RefNbr, docLabel, activeTran.TranNumber);
				}
			}

			if (doc.Released == false)
			{
				graph.Actions.PressSave();
			}
			ICCPayment toProc = graph.Caches[doc.GetType()].CreateCopy(doc) as ICCPayment;
			PXLongOperation.StartOperation(graph, delegate ()
			{
				bool success = true;
				try
				{
					TransactionProcessor.ProcessVoidOrCredit(toProc, state.ExternalTransaction);
				}
				catch
				{
					success = false;
					throw;
				}
				finally
				{
					if (AfterProcessingManager != null)
					{
						AfterProcessingManager.RunVoidActions((IBqlTable)doc, success);
						AfterProcessingManager.PersistData();
					}
				}
			});
		}

		public void CreditCCPayment(ICCPayment doc, IExternalTransactionAdapter paymentTransaction, string processingCenter)
		{
			if (doc == null || doc.CuryDocBal == null)
			{
				return;
			}
			var trans = paymentTransaction.Select();
			if (ExternalTranHelper.HasOpenCCProcTran(graph, trans.FirstOrDefault()))
			{
				throw new PXException(Messages.ERR_CCTransactionCurrentlyInProgress);
			}

			var needSyncTran = ExternalTranHelper.GetImportedNeedSyncTran(graph, trans);
			if (needSyncTran != null)
			{
				throw new PXException(Messages.ERR_TransactionIsNotValidated, needSyncTran.TranNumber);
			}

			if (doc.Released == false)
			{
				graph.Actions.PressSave();
			}
			ICCPayment toProc = graph.Caches[doc.GetType()].CreateCopy(doc) as ICCPayment;
			PXLongOperation.StartOperation(graph, delegate ()
			{
				bool success = true;
				try
				{
					IExternalTransaction tran = new ExternalTransaction();
					tran.TranNumber = doc.RefTranExtNbr;
					tran.ProcessingCenterID = processingCenter;
					TransactionProcessor.ProcessCredit(toProc, tran);
				}
				catch
				{
					success = false;
					throw;
				}
				finally
				{
					if (AfterProcessingManager != null)
					{
						AfterProcessingManager.RunCreditActions((IBqlTable)doc, success);
						AfterProcessingManager.PersistData();
					}
				}
			});
		}

		public void RecordVoid(ICCPayment doc, TranRecordData tranRecord)
		{
			bool success = true;
			try
			{
				var procGraph = PXGraph.CreateInstance<CCPaymentProcessingGraph>();
				var repo = new CCPaymentProcessingRepository(graph);
				repo.NeedPersist = this.NeedPersistAfterRecord;
				procGraph.Repository = repo;

				SetResponseTextIfNeeded(tranRecord);
				procGraph.RecordVoid(doc, tranRecord);
			}
			catch
			{
				success = false;
				throw;
			}
			finally
			{
				if (AfterProcessingManager != null)
				{
					AfterProcessingManager.RunVoidActions((IBqlTable)doc, success);
					if (NeedPersistAfterRecord)
					{
						AfterProcessingManager.PersistData();
					}
				}
			}
		}

		public void RecordVoid(ICCPayment doc, TranRecordData tranRecord, IExternalTransactionAdapter paymentTransaction)
		{
			if (doc == null || doc.CuryDocBal == null)
			{
				return;
			}

			CommonRecordChecks(paymentTransaction, tranRecord);
			var trans = paymentTransaction.Select();
			ExternalTransactionState state = ExternalTranHelper.GetActiveTransactionState(graph, trans);
			if (!state.IsActive)
			{
				throw new PXException(Messages.ERR_CCNoTransactionToVoid);
			}

			if (state.IsRefunded)
			{
				throw new PXException(Messages.ERR_CCTransactionOfThisTypeInvalidToVoid);
			}

			if (ExternalTranHelper.IsExpired(state.ExternalTransaction))
			{
				throw new PXException(Messages.TransactionHasExpired);
			}

			var needSyncTran = ExternalTranHelper.GetImportedNeedSyncTran(graph, trans);
			if (needSyncTran != null)
			{
				throw new PXException(Messages.ERR_TransactionIsNotValidated, needSyncTran.TranNumber);
			}

			RecordVoid(doc, tranRecord);
		}

		public void RecordUnknown(ICCPayment doc, TranRecordData recordData, IExternalTransactionAdapter paymentTransaction)
		{
			if (doc == null || doc.CuryDocBal == null)
			{
				return;
			}

			var trans = paymentTransaction.Select();
			if (ExternalTranHelper.HasOpenCCProcTran(graph, trans.FirstOrDefault()))
			{
				throw new PXException(Messages.ERR_CCTransactionCurrentlyInProgress);
			}

			var needSyncTran = ExternalTranHelper.GetImportedNeedSyncTran(graph, trans);
			if (needSyncTran != null)
			{
				throw new PXException(Messages.ERR_TransactionIsNotValidated, needSyncTran.TranNumber);
			}

			RecordUnknown(doc, recordData);
		}

		public void RecordUnknown(ICCPayment doc, TranRecordData tranRecord)
		{
			bool success = true; 
			try
			{
				var procGraph = PXGraph.CreateInstance<CCPaymentProcessingGraph>();
				var repo = new CCPaymentProcessingRepository(graph);
				repo.NeedPersist = this.NeedPersistAfterRecord;
				repo.KeepNewTranDeactivated = tranRecord.KeepNewTranDeactivated;
				procGraph.Repository = repo;

				SetResponseTextIfNeeded(tranRecord);
				procGraph.RecordUnknown(doc, tranRecord);
			}
			catch
			{
				success = false;
				throw;
			}
			finally
			{
				if (AfterProcessingManager != null)
				{
					AfterProcessingManager.RunUnknownActions((IBqlTable)doc, success);
					if (NeedPersistAfterRecord)
					{
						AfterProcessingManager.PersistData();
					}
				}
			}
		}

		public void RecordPriorAuthCapture(ICCPayment doc, TranRecordData tranRecord)
		{
			bool success = true;
			try
			{
				var procGraph = PXGraph.CreateInstance<CCPaymentProcessingGraph>();
				var repo = new CCPaymentProcessingRepository(graph);
				repo.NeedPersist = this.NeedPersistAfterRecord;
				repo.KeepNewTranDeactivated = tranRecord.KeepNewTranDeactivated;
				procGraph.Repository = repo;

				SetResponseTextIfNeeded(tranRecord);
				procGraph.RecordPriorAuthorizedCapture(doc, tranRecord);
			}
			catch
			{
				success = false;
				throw;
			}
			finally
			{
				if (AfterProcessingManager != null)
				{
					AfterProcessingManager.RunPriorAuthorizedCaptureActions((IBqlTable)doc, success);
					if (NeedPersistAfterRecord)
					{
						AfterProcessingManager.PersistData();
					}
				}
			}
		}

		public void RecordPriorAuthCapture(ICCPayment doc, TranRecordData tranRecord, IExternalTransactionAdapter paymentTransaction)
		{
			if (doc == null || doc.CuryDocBal == null)
			{
				return;
			}

			CommonRecordChecks(paymentTransaction, tranRecord);
			var trans = paymentTransaction.Select();
			ExternalTransactionState state = ExternalTranHelper.GetActiveTransactionState(graph, trans);

			if (state.IsCaptured)
			{
				throw new PXException(Messages.ERR_CCAuthorizedPaymentAlreadyCaptured);
			}

			var needSyncTran = ExternalTranHelper.GetImportedNeedSyncTran(graph, trans);
			if (needSyncTran != null)
			{
				throw new PXException(Messages.ERR_TransactionIsNotValidated, needSyncTran.TranNumber);
			}

			RecordPriorAuthCapture(doc, tranRecord);
		}

		public void RecordAuthCapture(ICCPayment doc, TranRecordData tranRecord)
		{
			bool success = true;
			try
			{
				var procGraph = PXGraph.CreateInstance<CCPaymentProcessingGraph>();
				var repo = new CCPaymentProcessingRepository(graph);
				repo.NeedPersist = this.NeedPersistAfterRecord;
				procGraph.Repository = repo;

				SetResponseTextIfNeeded(tranRecord);
				procGraph.RecordCapture(doc, tranRecord);
			}
			catch
			{
				success = false;
				throw;
			}
			finally
			{
				if (AfterProcessingManager != null)
				{
					AfterProcessingManager.RunCaptureActions((IBqlTable)doc, success);
					if (NeedPersistAfterRecord)
					{
						AfterProcessingManager.PersistData();
					}
				}
			}
		}

		public void RecordAuthCapture(ICCPayment doc, TranRecordData tranRecord, IExternalTransactionAdapter paymentTransaction)
		{
			if (doc == null || doc.CuryDocBal == null)
			{
				return;
			}

			CommonRecordChecks(paymentTransaction, tranRecord);
			ExternalTransactionState state = ExternalTranHelper.GetActiveTransactionState(graph, paymentTransaction.Select());
			if (state.IsCaptured)
			{
				throw new PXException(Messages.ERR_CCAuthorizedPaymentAlreadyCaptured);
			}

			RecordAuthCapture(doc, tranRecord);
		}

		public void RecordCaptureOnly(ICCPayment doc, TranRecordData tranRecord, IExternalTransactionAdapter paymentTransaction)
		{
			if (doc == null || doc.CuryDocBal == null)
			{
				return;
			}

			CommonRecordChecks(paymentTransaction, tranRecord);
			ExternalTransactionState state = ExternalTranHelper.GetActiveTransactionState(graph, paymentTransaction.Select());
			if (state.IsCaptured)
			{
				throw new PXException(Messages.ERR_CCAuthorizedPaymentAlreadyCaptured);
			}
			if (doc.Released == false)
			{
				graph.Actions.PressSave();
			}

			RecordCaptureOnly(doc, tranRecord);
		}

		public void RecordCaptureOnly(ICCPayment doc, TranRecordData tranRecord)
		{
			bool success = true;
			try
			{
				var procGraph = PXGraph.CreateInstance<CCPaymentProcessingGraph>();
				var repo = new CCPaymentProcessingRepository(graph);
				repo.NeedPersist = this.NeedPersistAfterRecord;
				procGraph.Repository = repo;

				SetResponseTextIfNeeded(tranRecord);
				procGraph.RecordCaptureOnly(doc, tranRecord);
			}
			catch
			{
				success = false;
				throw;
			}
			finally
			{
				if (AfterProcessingManager != null)
				{
					AfterProcessingManager.RunCaptureOnlyActions((IBqlTable)doc, success);
					if (NeedPersistAfterRecord)
					{
						AfterProcessingManager.PersistData();
					}
				}
			}
		}

		public void RecordAuthorization(ICCPayment doc, TranRecordData tranRecord)
		{
			bool success = true;
			try
			{
				var procGraph = PXGraph.CreateInstance<CCPaymentProcessingGraph>();
				var repo = new CCPaymentProcessingRepository(graph);
				repo.NeedPersist = this.NeedPersistAfterRecord;
				procGraph.Repository = repo;
				SetResponseTextIfNeeded(tranRecord);
				procGraph.RecordAuthorization(doc, tranRecord);
			}
			catch
			{
				success = false;
				throw;
			}
			finally
			{

				if (AfterProcessingManager != null)
				{
					AfterProcessingManager.RunAuthorizeActions((IBqlTable)doc, success);
					if (NeedPersistAfterRecord)
					{
						AfterProcessingManager.PersistData();
					}
				}
			}
		}

		public void RecordAuthorization(ICCPayment doc, TranRecordData tranRecord, IExternalTransactionAdapter paymentTransaction)
		{
			if (doc == null || doc.CuryDocBal == null)
			{
				return;
			}

			CommonRecordChecks(paymentTransaction, tranRecord);
			ExternalTransactionState state = ExternalTranHelper.GetActiveTransactionState(graph, paymentTransaction.Select());
			if (state.IsCaptured)
			{
				throw new PXException(Messages.ERR_CCAuthorizedPaymentAlreadyCaptured);
			}
			if (state.IsPreAuthorized)
			{
				throw new PXException(Messages.ERR_CCPaymentAlreadyAuthorized);
			}

			RecordAuthorization(doc, tranRecord);
		}

		public void RecordCredit(ICCPayment doc, TranRecordData tranRecord)
		{
			bool success = true;
			try
			{
				var procGraph = PXGraph.CreateInstance<CCPaymentProcessingGraph>();
				var repo = new CCPaymentProcessingRepository(graph);
				repo.NeedPersist = this.NeedPersistAfterRecord;
				repo.KeepNewTranDeactivated = tranRecord.KeepNewTranDeactivated;
				procGraph.Repository = repo;
				SetResponseTextIfNeeded(tranRecord);
				procGraph.RecordCredit(doc, tranRecord);
			}
			catch
			{
				success = false;
				throw;
			}
			finally
			{
				if (AfterProcessingManager != null)
				{
					AfterProcessingManager.RunCreditActions((IBqlTable)doc, success);
					if (NeedPersistAfterRecord)
					{
						AfterProcessingManager.PersistData();
					}
				}
			}
		}

		public void RecordCCCredit(ICCPayment doc, TranRecordData tranRecord, IExternalTransactionAdapter paymentTransaction)
		{
			if (doc == null || doc.CuryDocBal == null)
			{
				return;
			}

			CommonRecordChecks(paymentTransaction, tranRecord);
			var trans = paymentTransaction.Select();
			ExternalTransactionState state = ExternalTranHelper.GetActiveTransactionState(graph, trans);
			if (state.IsRefunded)
			{
				throw new PXException(Messages.ERR_CCPaymentIsAlreadyRefunded);
			}
			
			var needSyncTran = ExternalTranHelper.GetImportedNeedSyncTran(graph, trans);
			if (needSyncTran != null)
			{
				throw new PXException(Messages.ERR_TransactionIsNotValidated, needSyncTran.TranNumber);
			}
			RecordCredit(doc, tranRecord);
		}

		private void CommonRecordChecks(IExternalTransactionAdapter adapter, TranRecordData info)
		{
			var tran = adapter.Select().FirstOrDefault();
			if (ExternalTranHelper.HasOpenCCProcTran(graph, tran))
			{
				throw new PXException(Messages.ERR_CCTransactionCurrentlyInProgress);
			}
			if (string.IsNullOrEmpty(info.ExternalTranId))
			{
				throw new PXException(Messages.ERR_PCTransactionNumberOfTheOriginalPaymentIsRequired);
			}
		}

		private void SetResponseTextIfNeeded(TranRecordData recordData)
		{
			if (recordData.ResponseText == null)
			{
				recordData.ResponseText = Messages.ImportedExternalCCTransaction;
			}
		}
	}
}
