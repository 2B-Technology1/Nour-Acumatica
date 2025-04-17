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
﻿using PX.Common;
using PX.Objects.AP;
using PX.Objects.CN.Compliance.CL.DAC;
using PX.Objects.CN.Compliance.CL.Descriptor.Attributes.LienWaiver;
using PX.Objects.CN.Compliance.PM.DAC;
using PX.Objects.CN.JointChecks;
using PX.Objects.Common.Extensions;
using PX.Objects.CS;
using PX.Objects.PM;
using PX.Objects.PO;
using PX.Objects.SO;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace PX.Objects.CN.Compliance
{
	public class APPaymentEntryLienWaiver : PXGraphExtension<APPaymentEntry>
	{
		private LienWaiverConst lienWaiverTypes;
		private LienWaiverConst LienWaiverTypes
		{
			get
			{
				if (lienWaiverTypes == null)
				{
					int? lienWaverDocTypeID = GetLienWaiverDocumentType();
					int? conditionalPartial = GetLienWaiverType(lienWaverDocTypeID, PX.Objects.CN.Compliance.CL.Descriptor.Constants.LienWaiverDocumentTypeValues.ConditionalPartial);
					int? conditionalFinal = GetLienWaiverType(lienWaverDocTypeID, PX.Objects.CN.Compliance.CL.Descriptor.Constants.LienWaiverDocumentTypeValues.ConditionalFinal);
					int? unconditionalPartial = GetLienWaiverType(lienWaverDocTypeID, PX.Objects.CN.Compliance.CL.Descriptor.Constants.LienWaiverDocumentTypeValues.UnconditionalPartial);
					int? unconditionalFinal = GetLienWaiverType(lienWaverDocTypeID, PX.Objects.CN.Compliance.CL.Descriptor.Constants.LienWaiverDocumentTypeValues.UnconditionalFinal);

					lienWaiverTypes = new LienWaiverConst(lienWaverDocTypeID, conditionalPartial, conditionalFinal, unconditionalPartial, unconditionalFinal);
				}

				return lienWaiverTypes;
			}
		}

		public bool IsPreparePaymentsMassProcessing { get; set; }

		#region Views/Selects

		[PXCopyPasteHiddenView]
		public PXSetup<LienWaiverSetup> lienWaiverSetup;

		[PXCopyPasteHiddenView]
		public PXSelect<ComplianceDocument> LienWaivers;

		[PXCopyPasteHiddenView]
		public PXSelect<ComplianceDocumentPaymentReference> LinkToPayments;

		[PXCopyPasteHiddenView]
		public PXSelect<ComplianceDocumentReference> LienWaiversRefs;

		[PXCopyPasteHiddenView]
		public PXSelect<ComplianceDocumentBill> LinkToBills;

		public PXSelectJoin<APAdjust,
			InnerJoin<APInvoice, On<APAdjust.adjdDocType, Equal<APInvoice.docType>,
			And<APAdjust.adjdRefNbr, Equal<APInvoice.refNbr>>>,
			InnerJoin<APTran, On<APAdjust.adjdDocType, Equal<APTran.tranType>,
				And<APAdjust.adjdRefNbr, Equal<APTran.refNbr>,
				And2<Where<APAdjust.adjdLineNbr, Equal<Zero>, Or<APAdjust.adjdLineNbr, Equal<APTran.lineNbr>>>,
				And<APTran.taskID, IsNotNull>>>>,
			LeftJoin<POOrder, On<POOrder.orderType, Equal<APTran.pOOrderType>,
				And<POOrder.orderNbr, Equal<APTran.pONbr>>>>>>,
			Where<APAdjust.adjgDocType, Equal<Current<APPayment.docType>>,
			And<APAdjust.adjgRefNbr, Equal<Current<APPayment.refNbr>>>>> Transactions;

		[PXCopyPasteHiddenView]
		public PXSelectJoin<JointPayeePayment,
			InnerJoin<JointPayee, On<JointPayee.jointPayeeId, Equal<JointPayeePayment.jointPayeeId>>>,
			Where<JointPayeePayment.paymentDocType, Equal<Current<APPayment.docType>>,
			And<JointPayeePayment.paymentRefNbr, Equal<Current<APPayment.refNbr>>,
			And<JointPayee.isMainPayee, Equal<False>>>>> JointPayments;

		#endregion

		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<FeaturesSet.construction>();
		}

		[PXOverride]
		public virtual IEnumerable PutOnHold(PXAdapter adapter)
		{
			RemoveAutoLienWaivers();
			return  adapter.Get();
		}

		[PXOverride]
		public virtual IEnumerable ReleaseFromHold(PXAdapter adapter)
		{
			if (lienWaiverSetup.Current.ShouldStopPayments == true &&
				ContainsOutstandingLienWavers(Base.Document.Current))
			{
				string exceptionMessage = string.Concat(PX.Objects.CN.Compliance.Descriptor.ComplianceMessages.LienWaiver.BillHasOutstandingLienWaiverStopPayment,
					Environment.NewLine, PX.Objects.CN.Compliance.Descriptor.ComplianceMessages.LienWaiver.CheckWillBeAssignedOnHoldStatus);

				throw new PXException(exceptionMessage);
			}

			GenerateLienWaivers();
			return adapter.Get();
		}
				
		[PXOverride]
		public void VoidCheckProc(APPayment payment, Action<APPayment> baseHandler)
		{			
			baseHandler(payment);
			VoidAutomaticallyCreatedLienWaivers(payment);
		}

		public PXAction<APPayment> setAsFinal;
		[PXUIField(DisplayName = "Set As Final")]
		[PXButton(DisplayOnMainToolbar = false)]
		public virtual IEnumerable SetAsFinal(PXAdapter adapter)
		{
			if (LienWaivers.Current != null)
			{
				var lw = LienWaivers.Current;

				if (lw.DocumentTypeValue == LienWaiverTypes.ConditionalPartial ||
					lw.DocumentTypeValue == LienWaiverTypes.UnconditionalPartial)
				{
					if (!AllLinkedBillsPaid(lw) && LienWaivers.Ask("Set Lien Waiver as Final", GetDialogMessage(lw),
							MessageButtons.OKCancel, MessageIcon.Warning, true) != WebDialogResult.OK)
					{
						return adapter.Get();
					}

					if (lw.DocumentTypeValue == LienWaiverTypes.ConditionalPartial)
						lw.DocumentTypeValue = LienWaiverTypes.ConditionalFinal;
					if (lw.DocumentTypeValue == LienWaiverTypes.UnconditionalPartial)
						lw.DocumentTypeValue = LienWaiverTypes.UnconditionalFinal;

					if (lw.JointVendorInternalId == null && lw.JointVendorExternalName == null)
					{
						RecalculateLWAmount(lw);
					}

					LienWaivers.Update(lw);
				}
			}

			return adapter.Get();
		}

		protected virtual void _(Events.RowSelected<APPayment> e)
		{
			APPayment payment = e.Row;

			if (payment != null)
			{
				var lw = new PXSelectJoin<ComplianceDocument, LeftJoin<ComplianceDocumentReference,
								On<ComplianceDocumentReference.complianceDocumentReferenceId, Equal<ComplianceDocument.apCheckId>>>,
							Where<ComplianceDocumentReference.type, Equal<Required<APPayment.docType>>,
								And<ComplianceDocumentReference.referenceNumber, Equal<Required<APPayment.refNbr>>,
								And<ComplianceDocument.documentType, Equal<Required<ComplianceDocument.documentType>>>>>>(Base)
						.Select(payment.DocType, payment.RefNbr, LienWaiverTypes.DocType).ToList();

				setAsFinal.SetEnabled(payment.DocType == APPaymentType.Check && payment.Hold != true && lw.Count() > 0);
			}
        }

		protected virtual void _(Events.RowSelected<ComplianceDocument> e)
		{
			if (e.Row != null)
			{
				ComplianceDocument lw = e.Row;

				if (lw.DocumentType == LienWaiverTypes.DocType && lw.ThroughDate != null
					&& lw.ThroughDate < Base.Accessinfo.BusinessDate && lw.Received != true)
				{
					Vendor vendor = PXSelect<Vendor, Where<Vendor.bAccountID, Equal<Required<Vendor.bAccountID>>>>.Select(Base, lw.VendorID);

					e.Cache.RaiseExceptionHandling<ComplianceDocument.throughDate>(lw, lw.ThroughDate,
							new PXSetPropertyException<ComplianceDocument.throughDate>(Descriptor.ComplianceMessages.LienWaiver.OutstandingLienWaiver, PXErrorLevel.Warning, vendor.AcctName));
				}
			}
		}

		[PXOverride]
		public virtual void CreatePayment(APInvoice apdoc, string paymentType, Action<APInvoice, string> baseMethod)
		{
			baseMethod(apdoc, paymentType);
			if (Base.Document.Current.Hold != true )
			{
				GenerateLienWaivers();
			}
		}

		[PXOverride]
		public void Persist(Action baseMethod)
		{
			if (ShouldRegenerateLienWaiversOnSave(Base.Document.Current))
			{
				RemoveAutoLienWaivers();
				GenerateLienWaivers();
			}
			baseMethod();
		}				

		private bool ShouldRegenerateLienWaiversOnSave(APPayment row)
		{
			if (row == null)
				return false;
			
			if (IsPreparePaymentsMassProcessing)
				return false;
			if (row.Released == true)
				return false;
			if (row.Voided == true)
				return false;
			if (row.Hold == true)
				return false;

			bool? oldHold = (bool?) Base.Document.Cache.GetValueOriginal<APPayment.hold>(row);

			if (row.Hold != true && oldHold == true)
			{
				return false;
			}

			if (Base.Adjustments.Cache.Inserted.Any_() || Base.Adjustments.Cache.Deleted.Any_())
			{
				return true;
			}

			foreach (APAdjust adj in Base.Adjustments.Cache.Updated)
			{
				decimal oldAmountPaid = (decimal)Base.Adjustments.Cache.GetValueOriginal<APAdjust.curyAdjdAmt>(adj);

				if (oldAmountPaid != adj.CuryAdjdAmt)
				{
					return true;
				}
			}

			if(!Base.Adjustments.Cache.Inserted.Any_() &&
				!Base.Adjustments.Cache.Deleted.Any_() &&
				Base.Document.Cache.GetStatus(row) == PXEntryStatus.Inserted &&
				Base.Caches[typeof(ComplianceDocument)].Inserted.Any_())
			{
				return true;
			}

			return false;
		}

		protected virtual void _(Events.RowDeleted<APPayment> e)
		{
			if (e.Row == null) return;

			RemoveAutoLienWaivers();

			var manualLw = PXSelectJoin<ComplianceDocument,
				InnerJoin<ComplianceDocumentReference,
					On<ComplianceDocument.apCheckId, Equal<ComplianceDocumentReference.complianceDocumentReferenceId>>>,
				Where<ComplianceDocumentReference.refNoteId, Equal<Current<APPayment.noteID>>,
					And<ComplianceDocument.documentType, Equal<Required<ComplianceDocument.documentType>>,
					And<ComplianceDocument.isCreatedAutomatically, Equal<False>>>>>.Select(Base, LienWaiverTypes.DocType);

			foreach (PXResult<ComplianceDocument> res in manualLw)
			{
				ComplianceDocument lw = (ComplianceDocument)res;

				if (lw.BillID != null)
				{
					lw.ApCheckID = null;
					LienWaivers.Update(lw);
				}
				else
				{
					LienWaivers.Delete(lw);
				}
			}
		}

		private bool AllLinkedBillsPaid(ComplianceDocument lw)
		{
			var linkedLines = new PXSelect<ComplianceDocumentBill,
						Where<ComplianceDocumentBill.complianceDocumentID, Equal<Required<ComplianceDocument.complianceDocumentID>>>>(Base)
						.Select(lw.ComplianceDocumentID);

			foreach (PXResult<ComplianceDocumentBill> res in linkedLines)
			{
				ComplianceDocumentBill line = (ComplianceDocumentBill)res;

				var adjust = new PXSelect<APAdjust,
							Where<APAdjust.adjdDocType, Equal<Required<ComplianceDocumentBill.docType>>,
								And<APAdjust.adjdRefNbr, Equal<Required<ComplianceDocumentBill.refNbr>>,
								And<APAdjust.adjdLineNbr, Equal<Required<ComplianceDocumentBill.lineNbr>>,
								And<APAdjust.adjgDocType, Equal<Current<APPayment.docType>>,
								And<APAdjust.adjgRefNbr, Equal<Current<APPayment.refNbr>>>>>>>>(Base)
								.SelectSingle(line.DocType, line.RefNbr, line.LineNbr);

				if (adjust != null && adjust.CuryDocBal != 0)
				{
					return false;
				}
			}

			return true;
		}

		private void RecalculateLWAmount(ComplianceDocument lw)
		{
			var linkedLines = new PXSelect<ComplianceDocumentBill,
						Where<ComplianceDocumentBill.complianceDocumentID, Equal<Required<ComplianceDocument.complianceDocumentID>>>>(Base)
						.Select(lw.ComplianceDocumentID);

			decimal totalJointPayeesOwed = 0;
			decimal? initialLienWaiverAmount = 0;

			foreach (PXResult<ComplianceDocumentBill> res in linkedLines)
			{
				ComplianceDocumentBill line = (ComplianceDocumentBill)res;

				initialLienWaiverAmount += line.AmountPaid;

				APInvoice bill = new PXSelect<APInvoice,
							Where<APInvoice.docType, Equal<Required<ComplianceDocumentBill.docType>>,
								And<APInvoice.refNbr, Equal<Required<ComplianceDocumentBill.refNbr>>>>>(Base)
							.SelectSingle(line.DocType, line.RefNbr);

				if (bill.IsJointPayees == true)
				{
					if (bill.PaymentsByLinesAllowed == true)
					{
						var jointPayees = new PXSelect<JointPayee,
							Where<JointPayee.aPDocType, Equal<Required<ComplianceDocumentBill.docType>>,
								And<JointPayee.aPRefNbr, Equal<Required<ComplianceDocumentBill.refNbr>>,
								And<JointPayee.aPLineNbr, Equal<Required<ComplianceDocumentBill.lineNbr>>,
								And<JointPayee.isMainPayee, Equal<False>>>>>>(Base)
							.Select(line.DocType, line.RefNbr, line.LineNbr);

						foreach (PXResult<JointPayee> jpByLineRes in jointPayees)
						{
							JointPayee jp = (JointPayee)jpByLineRes;

							totalJointPayeesOwed += jp.JointAmountOwed.GetValueOrDefault();
						}
					}
					else
					{
						var jointPayees = new PXSelect<JointPayee,
							Where<JointPayee.aPDocType, Equal<Required<ComplianceDocumentBill.docType>>,
								And<JointPayee.aPRefNbr, Equal<Required<ComplianceDocumentBill.refNbr>>,
								And<JointPayee.aPLineNbr, IsNull,
								And<JointPayee.isMainPayee, Equal<False>>>>>>(Base)
							.Select(line.DocType, line.RefNbr);

						foreach (PXResult<JointPayee> jpRes in jointPayees)
						{
							JointPayee jp = (JointPayee)jpRes;

							totalJointPayeesOwed += jp.JointAmountOwed.GetValueOrDefault();
						}
					}
				}
			}

			if (totalJointPayeesOwed > 0)
			{
				lw.JointLienWaiverAmount = totalJointPayeesOwed;
				lw.LienWaiverAmount = initialLienWaiverAmount + totalJointPayeesOwed;
			}
		}

		private string GetDialogMessage(ComplianceDocument lw)
		{
			string message;

			string groupBySetting = lw.DocumentTypeValue == LienWaiverTypes.ConditionalPartial ||
				lw.DocumentTypeValue == LienWaiverTypes.ConditionalFinal ?
				lienWaiverSetup.Current.GroupByConditional : lienWaiverSetup.Current.GroupByUnconditional;

			PMProject project = PMProject.PK.Find(Base, lw.ProjectID);

			PMTask task = new PXSelect<PMTask,
				Where<PMTask.taskID, Equal<Required<ComplianceDocument.costTaskID>>>>(Base).SelectSingle(lw.CostTaskID);

			ComplianceDocumentReference poRef = new PXSelect<ComplianceDocumentReference,
					Where<ComplianceDocumentReference.complianceDocumentReferenceId,
					Equal<Required<ComplianceDocumentReference.complianceDocumentReferenceId>>>>(Base).SelectSingle(lw.PurchaseOrder);

			string commitment;
			string emptyParameter = "N/A";

			if (poRef != null)
				commitment = poRef.ReferenceNumber;
			else if (lw.Subcontract != null)
				commitment = lw.Subcontract;
			else
				commitment = emptyParameter;

			switch (groupBySetting)
			{
				case LienWaiverGroupBySource.CommitmentProjectTask:
					message = string.Format(Descriptor.ComplianceMessages.LienWaiver.OpenBalanceForCommitmentProjectTask, Base.Document.Current.RefNbr, commitment,
						project != null ? project.ContractCD : emptyParameter, task != null ? task.TaskCD : emptyParameter);
					break;
				case LienWaiverGroupBySource.CommitmentProject:
					message = string.Format(Descriptor.ComplianceMessages.LienWaiver.OpenBalanceForCommitmentProject, Base.Document.Current.RefNbr, commitment,
						project != null ? project.ContractCD : emptyParameter);
					break;
				case LienWaiverGroupBySource.ProjectTask:
					message = string.Format(Descriptor.ComplianceMessages.LienWaiver.OpenBalanceForProjectTask, Base.Document.Current.RefNbr,
						project != null ? project.ContractCD : emptyParameter, task != null ? task.TaskCD : emptyParameter);
					break;
				default:
					message = string.Format(Descriptor.ComplianceMessages.LienWaiver.OpenBalanceForProject, Base.Document.Current.RefNbr,
						project != null ? project.ContractCD : emptyParameter);
					break;
			}

			return message;
		}

		protected virtual void RemoveAutoLienWaivers()
		{
			var select = new PXSelectJoin<ComplianceDocument,
				InnerJoin<ComplianceDocumentReference, On<ComplianceDocument.apCheckId, Equal<ComplianceDocumentReference.complianceDocumentReferenceId>>>,
				Where<ComplianceDocumentReference.refNoteId, Equal<Current<APPayment.noteID>>,
				And<ComplianceDocument.documentType, Equal<Required<ComplianceDocument.documentType>>,
				And<ComplianceDocument.isCreatedAutomatically, Equal<True>>>>>(Base);

			foreach( PXResult<ComplianceDocument, ComplianceDocumentReference> res in select.Select(LienWaiverTypes.DocType))
			{
				LienWaivers.Delete(res);
				LienWaiversRefs.Delete(res);
			}

			foreach(ComplianceDocument record in Base.Caches[typeof(ComplianceDocument)].Inserted)
			{
				LienWaivers.Delete(record);
			}

			foreach (ComplianceDocumentReference record in Base.Caches[typeof(ComplianceDocumentReference)].Inserted)
			{
				LienWaiversRefs.Delete(record);
			}
		}

		public virtual void GenerateLienWaivers()
		{
			var leinWaiverCollectionConditional = GetLeinWaiverData(lienWaiverSetup.Current.GenerateWithoutCommitmentConditional,
				lienWaiverSetup.Current.GroupByConditional, true);

			foreach(ComplianceDocument manualLW in leinWaiverCollectionConditional.ManualLienWaivers)
			{
				if (manualLW.ApCheckID == null)
				{
					ComplianceDocumentPaymentReference reference = new ComplianceDocumentPaymentReference();
					reference.ComplianceDocumentReferenceId = Guid.NewGuid();
					LinkToPayments.Insert(reference);
					manualLW.ApCheckID = reference.ComplianceDocumentReferenceId;
					LienWaivers.Update(manualLW);
				}
			}

			if (lienWaiverSetup.Current.ShouldGenerateConditional == true)
			{
				foreach (LienWaiverData data in leinWaiverCollectionConditional.AutoLienWaivers)
				{
					GenerateLienWaiver(LienWaiverTypes.ConditionalPartial, data);
				}
			}

			if (lienWaiverSetup.Current.ShouldGenerateUnconditional == true)
			{
				var leinWaiverCollectionUnconditional = GetLeinWaiverData(lienWaiverSetup.Current.GenerateWithoutCommitmentUnconditional,
					lienWaiverSetup.Current.GroupByUnconditional, false);

				foreach (LienWaiverData data in leinWaiverCollectionUnconditional.AutoLienWaivers)
				{
					GenerateLienWaiver(LienWaiverTypes.UnconditionalPartial, data);
				}
			}
		}

		public virtual APPayment PutOnHoldIfOutstandingLienWavers(APPayment payment)
		{
			if (lienWaiverSetup.Current.ShouldStopPayments == true &&
				ContainsOutstandingLienWavers(payment))
			{
				payment.Hold = true;
				Base.Document.Update(payment);
			}
			
			return payment;
		}

		public virtual bool ContainsOutstandingLienWavers(APPayment payment)
		{
			var select = new PXSelectReadonly2<APAdjust,
				InnerJoin<APTran, On<APAdjust.adjdDocType, Equal<APTran.tranType>,
					And<APAdjust.adjdRefNbr, Equal<APTran.refNbr>,
					And2<Where<APAdjust.adjdLineNbr, Equal<Zero>, Or<APAdjust.adjdLineNbr, Equal<APTran.lineNbr>>>,
					And<APTran.taskID, IsNotNull>>>>,
				LeftJoin<ComplianceDocument, On<ComplianceDocument.projectID, Equal<APTran.projectID>,
					And<ComplianceDocument.vendorID, Equal<Required<ComplianceDocument.vendorID>>,
					And<ComplianceDocument.documentType, Equal<Required<ComplianceDocument.documentType>>,
					And<ComplianceDocument.throughDate, Less<Required<ComplianceDocument.throughDate>>,
					And<ComplianceDocument.received, NotEqual<True>>>>>>>>,
				Where<APAdjust.adjgDocType, Equal<Current<APPayment.docType>>,
				And<APAdjust.adjgRefNbr, Equal<Current<APPayment.refNbr>>,
				And<ComplianceDocument.complianceDocumentID, IsNotNull>>>>(Base);

			//For main vendor:
			APAdjust adjust = select.SelectWindowed(0, 1, payment.VendorID, LienWaiverTypes.DocType, Base.Accessinfo.BusinessDate);

			if (adjust != null)
			{
				return true;
			}
			else
			{
				var selectJointPayee = new PXSelectReadonly2<APAdjust,
				InnerJoin<APTran, On<APAdjust.adjdDocType, Equal<APTran.tranType>,
					And<APAdjust.adjdRefNbr, Equal<APTran.refNbr>,
					And2<Where<APAdjust.adjdLineNbr, Equal<Zero>, Or<APAdjust.adjdLineNbr, Equal<APTran.lineNbr>>>,
					And<APTran.taskID, IsNotNull>>>>,
				LeftJoin<ComplianceDocument, On<ComplianceDocument.projectID, Equal<APTran.projectID>,
					And<ComplianceDocument.vendorID, Equal<Required<ComplianceDocument.vendorID>>,
					And<ComplianceDocument.jointVendorInternalId, Equal<Required<ComplianceDocument.jointVendorInternalId>>,
					And<ComplianceDocument.documentType, Equal<Required<ComplianceDocument.documentType>>,
					And<ComplianceDocument.throughDate, Less<Required<ComplianceDocument.throughDate>>,
					And<ComplianceDocument.received, NotEqual<True>>>>>>>>>,
				Where<APAdjust.adjgDocType, Equal<Current<APPayment.docType>>,
				And<APAdjust.adjgRefNbr, Equal<Current<APPayment.refNbr>>,
				And<ComplianceDocument.complianceDocumentID, IsNotNull>>>>(Base);

				//Check Joint payees:
				foreach (PXResult<JointPayeePayment, JointPayee> res in JointPayments.Select())
				{
					JointPayee jp = (JointPayee)res;
					if (jp.JointPayeeInternalId != null)
					{
						adjust = selectJointPayee.SelectWindowed(0, 1, payment.VendorID, jp.JointPayeeInternalId, LienWaiverTypes.DocType, Base.Accessinfo.BusinessDate);

						if (adjust != null)
						{
							return true;
						}
					}
				}
			}

			return false;
		}

		private LienWaiverInfo GetLeinWaiverData(bool? generateWithoutCommitment, string groupBy, bool isConditionalCategory)
		{
			bool groupByTaskID = false;
			bool groupByOrderNbr = false;

			switch (groupBy)
			{
				case LienWaiverGroupBySource.ProjectTask:
					groupByTaskID = true;
					break;
				case LienWaiverGroupBySource.CommitmentProject:
					groupByOrderNbr = true;
					break;
				case LienWaiverGroupBySource.CommitmentProjectTask:
					groupByTaskID = true;
					groupByOrderNbr = true;
					break;
			}

			Dictionary<LienWaiverKey, LienWaiverData> data = new Dictionary<LienWaiverKey, LienWaiverData>();

			HashSet<string> processedBills = new HashSet<string>();
			HashSet<string> skipBills = new HashSet<string>();
			List<ComplianceDocument> manualLienWaivers = new List<ComplianceDocument>();

			foreach (PXResult<APAdjust, APInvoice, APTran, POOrder> res in Transactions.Select())
			{
				APAdjust adjust = (APAdjust)res;
				APTran tran = (APTran)res;
				APInvoice bill = (APInvoice)res;
				POOrder order = (POOrder)res;

				string billKey = string.Format("{0}.{1}", bill.DocType, bill.RefNbr);
				if (!processedBills.Contains(billKey))
				{
					processedBills.Add(billKey);
					var manualLWs = GetManualLienWaiverLinkedToPayment(bill);
					manualLienWaivers.AddRange(manualLWs);

					decimal totalManual = 0;
					foreach (ComplianceDocument manualLW in manualLWs)
					{
						if (isConditionalCategory)
						{
							if(manualLW.DocumentTypeValue == LienWaiverTypes.ConditionalFinal ||
								manualLW.DocumentTypeValue == LienWaiverTypes.ConditionalPartial)
							{
								totalManual += manualLW.LienWaiverAmount.GetValueOrDefault();
							}
						}
						else
						{
							if (manualLW.DocumentTypeValue == LienWaiverTypes.UnconditionalFinal ||
								manualLW.DocumentTypeValue == LienWaiverTypes.UnconditionalPartial)
							{
								totalManual += manualLW.LienWaiverAmount.GetValueOrDefault();
							}
						}						
					}

					if (totalManual >= bill.CuryOrigDocAmt)
					{
						skipBills.Add(billKey);
					}
				}

				if (skipBills.Contains(billKey))
					continue;

				LienWaiverKey key = new LienWaiverKey(
					tran.ProjectID.GetValueOrDefault(),
					groupByTaskID ? tran.TaskID : null,
					groupByOrderNbr ? tran.PONbr : null);

				if(tran.PONbr == null && generateWithoutCommitment != true)
					continue;

				if (!IsSupported(bill, tran, groupByTaskID, groupByOrderNbr))
					continue;

				LienWaiverData lv = null;
				if (data.TryGetValue(key, out lv))
				{
					lv.Records.Add(new PXResult<APAdjust, APInvoice, APTran, POOrder>(adjust, bill, tran, order));
				}
				else
				{
					if (CheckLimitsForLienWaiver(tran, order))
					{
						lv = new LienWaiverData(Base.Document.Current, PMProject.PK.Find(Base, tran.ProjectID),
							GetExistingLienWaiverStats(LienWaiverTypes.DocType, Base.Document.Current.VendorID, tran.ProjectID, order));
						lv.Records.Add(new PXResult<APAdjust, APInvoice, APTran, POOrder>(adjust, bill, tran, order));
						data.Add(key, lv);
					}
				}

				if (lv != null)
				{
					if (lv.Records.Select(r => r.GetItem<APAdjust>()).Where(r => r == adjust).Count() < 2)
					{
						decimal sign = adjust.AdjgBalSign ?? 1m;
						lv.TotalPayed += sign * adjust.AdjAmt.GetValueOrDefault();
					}
					lv.BillBalance += tran.TranBal.GetValueOrDefault();
				}
			}

			return new LienWaiverInfo(data.Values, manualLienWaivers);
		}

		private bool IsSupported(APInvoice bill, APTran tran, bool groupByTaskID, bool groupByOrderNbr)
		{
			if (bill.PaymentsByLinesAllowed == true)
				return true;

			APTran outOfGroup;
			if (groupByOrderNbr)
			{
				if (groupByTaskID)
				{
					var select = new PXSelect<APTran,
					Where<APTran.tranType, Equal<Required<APTran.tranType>>,
					And<APTran.refNbr, Equal<Required<APTran.refNbr>>,
					And<APTran.lineType, NotEqual<SOLineType.discount>,
					And<Where<APTran.taskID, NotEqual<Required<APTran.taskID>>, Or<APTran.pONbr, NotEqual<Required<APTran.pONbr>>>>>>>>>(Base);
					outOfGroup = select.SelectWindowed(0, 1, tran.TranType, tran.RefNbr, tran.TaskID, tran.PONbr);
				}
				else
				{
					var select = new PXSelect<APTran,
					Where<APTran.tranType, Equal<Required<APTran.tranType>>,
					And<APTran.refNbr, Equal<Required<APTran.refNbr>>,
					And<APTran.lineType, NotEqual<SOLineType.discount>,
					And<Where<APTran.projectID, NotEqual<Required<APTran.projectID>>, Or<APTran.pONbr, NotEqual<Required<APTran.pONbr>>>>>>>>>(Base);
					outOfGroup = select.SelectWindowed(0, 1, tran.TranType, tran.RefNbr, tran.ProjectID, tran.PONbr);
				}
			}
			else
			{
				if (groupByTaskID)
				{
					var select = new PXSelect<APTran,
					Where<APTran.tranType, Equal<Required<APTran.tranType>>,
					And<APTran.refNbr, Equal<Required<APTran.refNbr>>,
					And<APTran.lineType, NotEqual<SOLineType.discount>,
					And<APTran.taskID, NotEqual<Required<APTran.taskID>>>>>>>(Base);
					outOfGroup = select.SelectWindowed(0, 1, tran.TranType, tran.RefNbr, tran.TaskID);
				}
				else
				{
					var select = new PXSelect<APTran,
					Where<APTran.tranType, Equal<Required<APTran.tranType>>,
					And<APTran.refNbr, Equal<Required<APTran.refNbr>>,
					And<APTran.lineType, NotEqual<SOLineType.discount>,
					And<APTran.projectID, NotEqual<Required<APTran.projectID>>>>>>>(Base);
					outOfGroup = select.SelectWindowed(0, 1, tran.TranType, tran.RefNbr, tran.ProjectID);
				}
			}

			return outOfGroup == null;
		}

		private List<ComplianceDocument> GetManualLienWaiverLinkedToPayment(APInvoice bill)
		{
			var select = new PXSelectJoin<ComplianceDocument, InnerJoin<ComplianceDocumentReference,
				On<ComplianceDocumentReference.complianceDocumentReferenceId, Equal<ComplianceDocument.billID>>>,
			Where<ComplianceDocumentReference.type, Equal<Required<APInvoice.docType>>,
				And<ComplianceDocumentReference.referenceNumber, Equal<Required<APInvoice.refNbr>>,
				And<ComplianceDocument.linkToPayment, Equal<True>,
				And<ComplianceDocument.documentType, Equal<Required<ComplianceDocument.documentType>>,
				And<ComplianceDocument.isCreatedAutomatically, Equal<False>>>>>>>(Base);
						
			return select.Select(bill.DocType, bill.RefNbr, LienWaiverTypes.DocType).FirstTableItems.ToList();
		}

		private bool CheckLimitsForLienWaiver(APTran tran, POOrder order)
		{
			var selectLimits = new PXSelect<LienWaiverRecipient,
						Where<LienWaiverRecipient.projectId, Equal<Required<LienWaiverRecipient.projectId>>,
						And<LienWaiverRecipient.vendorClassId, Equal<Required<LienWaiverRecipient.vendorClassId>>>>>(Base);

			LienWaiverRecipient recipient;

			if (order.OrderNbr == null)
			{
				return true;
			}

			if (Base.Document.Current.JointPayeeID == null)
			{
				recipient = selectLimits.Select(tran.ProjectID, Base.vendor.Current.VendorClassID);
			}
			else
			{
				JointPayee jp = JointPayee.PK.Find(Base, Base.Document.Current.JointPayeeID);
				if (jp.JointPayeeInternalId == null)
				{
					return true;
				}
				else
				{
					Vendor vendor = Vendor.PK.Find(Base, jp.JointPayeeInternalId);
					recipient = selectLimits.Select(tran.ProjectID, vendor.VendorClassID);
				}
			}

			if (recipient == null)
				return false;

			return recipient.MinimumCommitmentAmount <= order.CuryOrderTotal;
		}
		
		private ExistingLienWaiverStats GetExistingLienWaiverStats(int? lienWaverDocTypeID, int? vendorID, int? projectID, POOrder order)
		{
			List<object> args = new List<object>();
			
			var select = new PXSelect<ComplianceDocument,
				Where<ComplianceDocument.documentType, Equal<Required<ComplianceDocument.documentType>>,
				And<ComplianceDocument.projectID, Equal<Required<ComplianceDocument.projectID>>,
				And<ComplianceDocument.vendorID, Equal<Required<ComplianceDocument.vendorID>>,
				And<ComplianceDocument.lienNoticeAmount, IsNotNull,
				And<ComplianceDocument.isVoided, NotEqual<True>>>>>>>(Base);

			args.Add(lienWaverDocTypeID);
			args.Add(projectID);
			args.Add(vendorID);

			if (order.OrderType != POOrderType.RegularSubcontract)
			{
				select.WhereAnd<Where<ComplianceDocument.subcontract, Equal<Required<ComplianceDocument.subcontract>>>>();
				args.Add(order.OrderNbr);
			}
			else
			{
				select.Join<InnerJoin<ComplianceDocumentReference,
					On<ComplianceDocumentReference.complianceDocumentReferenceId, Equal<ComplianceDocument.purchaseOrder>>>>();

				select.WhereAnd<Where<ComplianceDocumentReference.refNoteId,
					Equal<Required<ComplianceDocumentReference.refNoteId>>>>();

				args.Add(order.NoteID);
			}

			var lienWaivers = select.Select<ComplianceDocument>(args).ToList();
			var lienNoticeAmounts = lienWaivers.Select(lw => lw.LienNoticeAmount)
				.Where(lna => lna != null).Distinct().ToList();

			ExistingLienWaiverStats result = new ExistingLienWaiverStats();
			if (lienNoticeAmounts.IsSingleElement())
			{
				result.NoticeAmount = lienNoticeAmounts.First();
			}
			result.TotalAmount = lienWaivers.Sum(l => l.LienWaiverAmount);

			return result;
		}
	
		private ComplianceDocument GenerateLienWaiver(int? type, LienWaiverData data)
		{
			ComplianceDocument lienWaiver = new ComplianceDocument();
			lienWaiver.DocumentType = LienWaiverTypes.DocType;
			lienWaiver.DocumentTypeValue = type;
			lienWaiver.ProjectID = data.Project.ContractID;
			lienWaiver.VendorID = data.Payment.VendorID;
			lienWaiver.Required = true;
			lienWaiver.IsCreatedAutomatically = true;
			lienWaiver.CostTaskID = data.GetOnlyTaskID();
			lienWaiver.CostCodeID = data.GetOnlyCostCodeID();
			lienWaiver.AccountID = data.GetOnlyAccountID();
			lienWaiver.CustomerID = data.Project.CustomerID;
			lienWaiver.SkipInit = true; //Turn off any auto initialization. //To review and refactor.
			lienWaiver.ApPaymentMethodID = data.Payment.PaymentMethodID;

			var order = data.GetOnlyOrder();
			if (order != null)
			{
				if (order.OrderType != POOrderType.RegularSubcontract)
				{
					lienWaiver.PurchaseOrder = NewDocRef(order.OrderType, order.OrderNbr, order.NoteID);
					lienWaiver.PurchaseOrderLineItem = data.GetOnlyPOLineNbr();
				}
				else
				{
					lienWaiver.Subcontract = order.OrderNbr;
					lienWaiver.SubcontractLineItem = data.GetOnlyPOLineNbr();
				}
			}

			ComplianceDocumentPaymentReference reference = new ComplianceDocumentPaymentReference();
			reference.ComplianceDocumentReferenceId = Guid.NewGuid();
			LinkToPayments.Insert(reference);
			lienWaiver.ApCheckID = reference.ComplianceDocumentReferenceId;


			var bills = data.GetInvoices();
			var includedBillLines = data.GetLWLines();
			if (bills.Count == 1)
			{
				lienWaiver.BillID = NewDocRef(bills[0].DocType, bills[0].RefNbr, bills[0].NoteID);
				lienWaiver.BillAmount = bills[0].CuryOrigDocAmt.GetValueOrDefault();
			}
			
			lienWaiver.PaymentDate = data.Payment.AdjDate;
			lienWaiver.SourceType = Compliance.CL.Descriptor.Attributes.ComplianceDocumentSourceTypeAttribute.ApBill;

			lienWaiver.LienWaiverAmount = Math.Max(0, data.TotalPayed);
			
			if (Base.Document.Current != null &&
				Base.Document.Current.JointPayeeID != null)
			{
				JointPayee jp = JointPayee.PK.Find(Base, Base.Document.Current.JointPayeeID);
				if (jp != null)
				{
					if (jp.JointPayeeInternalId != null)
					{
						lienWaiver.JointVendorInternalId = jp.JointPayeeInternalId;
					}
					else if (jp.JointPayeeExternalName != null)
					{
						lienWaiver.JointVendorExternalName = jp.JointPayeeExternalName;
					}
				}
			}

			if (JointPayments.Select().Count > 0)
			{
				lienWaiver.JointAmount = lienWaiver.LienWaiverAmount;
				lienWaiver.JointLienWaiverAmount = lienWaiver.LienWaiverAmount;
			}

			string source = type == LienWaiverTypes.ConditionalFinal ||
				type == LienWaiverTypes.ConditionalPartial ? lienWaiverSetup.Current.ThroughDateSourceConditional :
				lienWaiverSetup.Current.ThroughDateSourceUnconditional;

			switch (source)
			{
				case PX.Objects.CN.Compliance.CL.Descriptor.Attributes.LienWaiver.LienWaiverThroughDateSource.BillDate:
					lienWaiver.ThroughDate = data.GetMaxInvoiceDate();
					break;
				case PX.Objects.CN.Compliance.CL.Descriptor.Attributes.LienWaiver.LienWaiverThroughDateSource.PaymentDate:
					lienWaiver.ThroughDate = data.Payment.AdjDate;
					break;
				default:
					lienWaiver.ThroughDate = PX.Objects.CN.Common.Services.DataProviders.FinancialPeriodDataProvider.GetFinancialPeriod(Base, data.Payment.AdjFinPeriodID).EndDate
						.GetValueOrDefault().AddDays(-1);
					break;
			}

			skipSelectorVerificationForPOLine = true;
			try
			{
				lienWaiver = LienWaivers.Insert(lienWaiver);

				foreach (APAdjust line in includedBillLines)
				{
					ComplianceDocumentBill link = new ComplianceDocumentBill();
					link.ComplianceDocumentID = lienWaiver.ComplianceDocumentID;
					link.DocType = line.AdjdDocType;
					link.RefNbr = line.AdjdRefNbr;
					link.LineNbr = line.AdjdLineNbr;
					link.AmountPaid = line.AdjAmt;

					LinkToBills.Insert(link);
				}

				return lienWaiver;
			}
			finally
			{
				skipSelectorVerificationForPOLine = false;
			}
		}

		private bool skipSelectorVerificationForPOLine;
		protected virtual void _(Events.FieldVerifying<ComplianceDocument, ComplianceDocument.purchaseOrderLineItem> e)
		{
			if (skipSelectorVerificationForPOLine)
				e.Cancel = true;
		}

		private Guid? NewDocRef(string type, string refNbr, Guid? noteID)
		{
			ComplianceDocumentReference reference = new ComplianceDocumentReference();
			reference.ComplianceDocumentReferenceId = Guid.NewGuid();
			reference.Type = type;
			reference.ReferenceNumber = refNbr;
			reference.RefNoteId = noteID;

			LienWaiversRefs.Insert(reference);

			return reference.ComplianceDocumentReferenceId;
		}
		
		private int? GetLienWaiverDocumentType()
		{
			var select = new PXSelect<ComplianceAttributeType,
				Where<ComplianceAttributeType.type, Equal<Required<ComplianceAttributeType.type>>>>(Base);

			ComplianceAttributeType type = select.SelectSingle(ComplianceDocumentType.LienWaiver);

			return type?.ComplianceAttributeTypeID;
		}

		private int? GetLienWaiverType(int? lienWaiverDocumentType, string lienWaiverType)
		{
			var select = new PXSelect<ComplianceAttribute,
				Where<ComplianceAttribute.type, Equal<Required<ComplianceAttribute.type>>,
				And<ComplianceAttribute.value, Equal<Required<ComplianceAttribute.value>>>>>(Base);

			ComplianceAttribute type = select.SelectSingle(lienWaiverDocumentType, lienWaiverType);

			return type?.AttributeId;
		}
			
		private void VoidAutomaticallyCreatedLienWaivers(APPayment payment)
		{
			var select = new PXSelectJoin<ComplianceDocument,
				InnerJoin<ComplianceDocumentReference, On<ComplianceDocument.apCheckId, Equal<ComplianceDocumentReference.complianceDocumentReferenceId>>>,
				Where<ComplianceDocument.documentType, Equal<Required<ComplianceDocument.documentType>>,
				And<ComplianceDocument.isCreatedAutomatically, Equal<True>,
				And<ComplianceDocumentReference.refNoteId, Equal<Required<ComplianceDocumentReference.refNoteId>>>>>>(Base);

			foreach (ComplianceDocument item in select.Select(LienWaiverTypes.DocType, payment.NoteID))
			{
				item.IsVoided = true;
				LienWaivers.Update(item);
			}
		}

		public class LienWaiverInfo
		{
			public ICollection<LienWaiverData> AutoLienWaivers { get; private set; }
			public IList<ComplianceDocument> ManualLienWaivers { get; private set; }

			public LienWaiverInfo(ICollection<LienWaiverData> autoLienWaivers, IList<ComplianceDocument> manualLienWaivers)
			{
				this.AutoLienWaivers = autoLienWaivers;
				this.ManualLienWaivers = manualLienWaivers;
			}
		}
		public class LienWaiverData
		{
			public decimal BillBalance { get; set; }
			public decimal TotalPayed { get; set; }

			public ExistingLienWaiverStats Stats { get; private set; }

			public PMProject Project { get; private set; }

			public APPayment Payment { get; private set; }
						
			public List<PXResult<APAdjust, APInvoice, APTran, POOrder>> Records { get; private set; }
						
			public List<APInvoice> GetInvoices()
			{
				List<APInvoice> listOfBills = new List<APInvoice>();
				HashSet<string> processed = new HashSet<string>();
				foreach (PXResult<APAdjust, APInvoice, APTran, POOrder> res in Records)
				{
					APInvoice apbill = (APInvoice)res;
					APAdjust adjust = (APAdjust)res;
					string key = string.Format("{0}.{1}", apbill.DocType, apbill.RefNbr);
					if (!processed.Contains(key))
					{
						processed.Add(key);
						listOfBills.Add(apbill);
					}
				}

				return listOfBills;
			}

			public List<APAdjust> GetLWLines()
			{
				List<APAdjust> listOfAdjLines = new List<APAdjust>();
				foreach (PXResult<APAdjust, APInvoice, APTran, POOrder> res in Records)
				{
					APAdjust adjust = (APAdjust)res;
					listOfAdjLines.Add(adjust);
				}

				return listOfAdjLines;
			}

			public decimal GetAmountPaid(APInvoice bill)
			{
				decimal amount = 0;
				foreach (PXResult<APAdjust, APInvoice, APTran, POOrder> res in Records)
				{
					APInvoice apbill = (APInvoice)res;
					APAdjust adjust = (APAdjust)res;

					if (apbill.DocType == bill.DocType && apbill.RefNbr == bill.RefNbr)
					{
						amount += adjust.AdjAmt.GetValueOrDefault();
					}
				}

				return amount;
			}

			public POOrder GetOnlyOrder()
			{
				Guid?[] distinct = Records.Select(r => r.GetItem<POOrder>().NoteID).Distinct().ToArray();
				if (distinct.Length == 1 && distinct[0] != null)
					return Records[0].GetItem<POOrder>();
				else
					return null;
			}

			public int? GetOnlyTaskID()
			{
				int?[] distinct = Records.Select(r => r.GetItem<APTran>().TaskID).Distinct().ToArray();

				if (distinct.Length != 1)
					return null;
				else
					return distinct[0];
			}

			public int? GetOnlyCostCodeID()
			{
				int?[] distinct = Records.Select(r => r.GetItem<APTran>().CostCodeID).Distinct().ToArray();

				if (distinct.Length != 1)
					return null;
				else
					return distinct[0];
			}

			public int? GetOnlyAccountID()
			{
				int?[] distinct = Records.Select(r => r.GetItem<APTran>().AccountID).Distinct().ToArray();

				if (distinct.Length != 1)
					return null;
				else
					return distinct[0];
			}

			public int? GetOnlyPOLineNbr()
			{
				int?[] distinct = Records.Select(r => r.GetItem<APTran>().POLineNbr).Distinct().ToArray();

				if (distinct.Length != 1)
					return null;
				else
					return distinct[0];
			}

			public DateTime? GetMaxInvoiceDate()
			{
				return Records.Select(r => r.GetItem<APInvoice>().DocDate).Max();
			}

			public LienWaiverData(APPayment payment, PMProject project, ExistingLienWaiverStats stats)
			{
				this.Payment = payment;
				this.Project = project;
				this.Stats = stats;

				Records = new List<PXResult<APAdjust, APInvoice, APTran, POOrder>>();
			}

		}

		public class ExistingLienWaiverStats
		{
			public decimal? TotalAmount;
			public decimal? NoticeAmount;
		}

		private class LienWaiverConst
		{
			public int? DocType { get; private set; }
			public int? ConditionalPartial { get; private set; }

			public int? ConditionalFinal { get; private set; }

			public int? UnconditionalPartial { get; private set; }

			public int? UnconditionalFinal { get; private set; }

			public LienWaiverConst(int? docType, int? conditionalPartial, int? conditionalFinal,
				int? unconditionalPartial, int? unconditionalFinal)
			{
				this.DocType = docType;
				this.ConditionalFinal = conditionalFinal;
				this.ConditionalPartial = conditionalPartial;
				this.UnconditionalFinal = unconditionalFinal;
				this.UnconditionalPartial = unconditionalPartial;
			}
		}

		[System.Diagnostics.DebuggerDisplay("{BillNbr}.{ProjectID}.{TaskID}.{OrderNbr}")]
		public struct LienWaiverKey
		{
			public readonly int ProjectID;
			public readonly int? TaskID;
			public readonly string OrderNbr;
			
			public LienWaiverKey(int projectID, int? taskID, string orderNbr)
			{
				ProjectID = projectID;
				TaskID = taskID;
				OrderNbr = orderNbr;
			}

			public override int GetHashCode()
			{
				unchecked // Overflow is fine, just wrap
				{
					int hash = 17;
					hash = hash * 23 + ProjectID.GetHashCode();
					hash = hash * 23 + (TaskID != null ? TaskID.GetHashCode() : 0);
					hash = hash * 23 + (OrderNbr != null ? OrderNbr.GetHashCode() : 0);
					return hash;
				}
			}
		}
	}
}
