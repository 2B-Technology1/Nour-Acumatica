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
using PX.Api;
using PX.Data;
using PX.Objects.AP;
using PX.Objects.CS;
using PX.Objects.CR;
using PX.Objects.CA;

namespace PX.Objects.Localizations.GB.BACS.CA
{
	/// <summary>
	/// The CABatchEntry extension is used to export batch payments for the BACS Lloyds payment export scenario.
	/// </summary>
	public class CABatchEntryExt : PXGraphExtension<CABatchEntry>
	{
		protected bool batchHasMultiplePaymentDates = false;

		#region IsActive

		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<FeaturesSet.uKLocalization>();
		}
		#endregion

		[PXOverride]
		public IEnumerable Export(PXAdapter adapter, Func<PXAdapter, IEnumerable> del)
		{
			#region MultiplePaymentDates

			PaymentMethod pm = GetPaymentMethodDetails(Base.Document.Current);

			if (pm.DirectDepositFileFormat == LloydsDirectDepositType.Code)
			{
				var pmnt = Base.BatchPayments.Select().FirstOrDefault().GetItem<APPayment>();

				foreach (var batchDetail in Base.BatchPayments.Select())
				{
					APPayment paymentInBatch = batchDetail.GetItem<APPayment>();

					if (paymentInBatch?.DocDate != pmnt.DocDate)
					{
						throw new PXException(Messages.BACSLloyds.DifferentBatchPaymentDates,
							PXUIFieldAttribute.GetDisplayName<APPayment.docDate>(this.Base.APPaymentList.Cache));
					}
				}
			}

			#endregion

			if (RunExport(Base.Document.Current))
				return adapter.Get();

			if (del != null)
			{
				return (del(adapter));
			}
			else
			{
				return (adapter.Get());
			}
		}

		protected bool RunExport(CABatch doc)
		{
			bool needUpdate = false;

			Base.Save.Press();

			if (CanRunExport(doc))
			{
				if (doc?.SkipExport == false)
				{
					if (doc != null && doc.Released != true && doc.Hold != true && doc.Exported != true)
					{
						doc.Exported = true;
						doc.DateSeqNbr = CABatchEntry.GetNextDateSeqNbr(Base, doc);
						needUpdate = true;
					}
				}

				PXSYParameter[] paramList = setExportParameters(doc);
				PXResult<PaymentMethod, SYMapping, SYProvider> res = GetPaymentMethodDetails(doc);
				SYMapping map = res;

				PXLongOperation.StartOperation(Base, delegate ()
				{
					PX.Api.SYExportProcess.RunScenario(map.Name, SYMapping.RepeatingOption.All,
						true,
						true,
						paramList);

					if (needUpdate)
					{
						Base.Document.Update(doc);
						Base.Save.Press();
					}
				});

				return true;
			}
			else
				return false;
		}

		protected PXSYParameter[] setExportParameters(CABatch doc)
		{
			string fileName = BuildFileName(Base, doc);

			return new PXSYParameter[] {
							new PX.Api.PXSYParameter(BACSLloydsProvider.Params.FileName, fileName),
							new PX.Api.PXSYParameter(BACSLloydsProvider.Params.BatchNbr, doc.BatchNbr)
						};
		}

		private string BuildFileName(CABatchEntry graph, CABatch doc)
		{
			string fileName = "Lloyds.csv"; // Should never appear, a released batch is always linked to a cash account

			CashAccount cashAccount = graph.cashAccount.Select(doc.CashAccountID);

			if (cashAccount != null)
			{
				int batchSeqNbr;

				if (int.TryParse(doc.BatchSeqNbr, out batchSeqNbr))
				{
					fileName = string.Format("{0}-{1}-{2:yyyyMMdd}-{3:0000}.csv", doc.PaymentMethodID, cashAccount.CashAccountCD.Trim(), doc.TranDate.Value, batchSeqNbr);
				}
			}

			return fileName;
		}

		protected bool CanRunExport(CABatch doc)
		{
			bool result = false;

			if (PXLongOperation.GetStatus(Base.UID) == PXLongRunStatus.InProcess)
			{
				throw new ApplicationException(PX.Objects.GL.Messages.PrevOperationNotCompleteYet);
			}

			if (doc != null && doc.Hold == false)
			{
				PXResult<PaymentMethod, SYMapping> res = GetPaymentMethodDetails(doc);

				PaymentMethod pm = res;
				SYMapping map = res;

				if (pm != null &&
					pm.APCreateBatchPayment == true &&
					pm.APBatchExportSYMappingID != null &&
					map != null)
				{
					if (IsProviderBACSLloyds(pm.DirectDepositFileFormat))
					{
						result = true;
					}
				}
			}

			return result;
		}

		private bool IsProviderBACSLloyds(string directDepositFileFormat)
		{
			if (directDepositFileFormat == LloydsDirectDepositType.Code)
				return true;
			else
				return false;
		}

		private PXResult<PaymentMethod, SYMapping, SYProvider> GetPaymentMethodDetails(CABatch doc)
		{
			PXResult<PaymentMethod, SYMapping, SYProvider> res =
				(PXResult<PaymentMethod, SYMapping, SYProvider>)
				PXSelectJoin<PaymentMethod,
						LeftJoin<SYMapping,
							On<PaymentMethod.aPBatchExportSYMappingID,
								Equal<SYMapping.mappingID>>,
							LeftJoin<SYProvider,
								On<SYMapping.providerID,
									Equal<SYProvider.providerID>>>>,
						Where<PaymentMethod.paymentMethodID,
							Equal<Optional<CABatch.paymentMethodID>>>>
					.Select(Base, doc.PaymentMethodID);

			return res;
		}

		/// <summary>
		/// Print the payment and add it to the batch delegate.
		/// </summary>
		public delegate void PrintPaymentAndAddToBatchDel(CABatchEntry graph, APPrintChecks printChecks,
			APPaymentEntry pe,
			CABatch batch, APPayment pmt, ref string NextCheckNbr);

		[PXOverride]
		public virtual void PrintPaymentAndAddToBatch(CABatchEntry graph, APPrintChecks printChecks, APPaymentEntry pe,
			CABatch batch, APPayment pmt, ref string NextCheckNbr, PrintPaymentAndAddToBatchDel del)
		{
			PaymentMethod paymentMethod = GetPaymentMethodDetails(graph.Document.Current);
			if (IsProviderBACSLloyds(paymentMethod.DirectDepositFileFormat))
			{
				foreach (var batchDetail in graph.BatchPayments.Select())
				{
					APPayment paymentInBatch = batchDetail.GetItem<APPayment>();
					if (paymentInBatch?.DocDate != pmt.DocDate)
					{
						string fieldName =
							PXUIFieldAttribute.GetDisplayName<APPayment.docDate>(graph.APPaymentList.Cache);
						throw new PXException(Messages.BACSLloyds.DifferentBatchPaymentDates, fieldName);
					}
				}
			}

			del(graph, printChecks, pe, batch, pmt, ref NextCheckNbr);
		}
	}
}
