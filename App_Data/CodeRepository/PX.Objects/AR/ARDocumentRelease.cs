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

using PX.Common;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.AR.BQL;
using PX.Objects.AR.CCPaymentProcessing.Common;
using PX.Objects.AR.CCPaymentProcessing.Helpers;
using PX.Objects.AR.Overrides.ARDocumentRelease;
using PX.Objects.AR.Standalone;
using PX.Objects.CA;
using PX.Objects.CM.Extensions;
using PX.Objects.Common;
using PX.Objects.Common.EntityInUse;
using PX.Objects.Common.Exceptions;
using PX.Objects.Common.Extensions;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Objects.DR;
using PX.Objects.Extensions.MultiCurrency;
using PX.Objects.Extensions.MultiCurrency.AR;
using PX.Objects.GL;
using PX.Objects.GL.FinPeriods;
using PX.Objects.GL.FinPeriods.TableDefinition;
using PX.Objects.IN;
using PX.Objects.IN.Attributes;
using PX.Objects.PM;
using PX.Objects.SO;
using PX.Objects.TX;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using static PX.Objects.AP.APReleaseProcess;
using ARTranPostType = PX.Objects.AR.ARTranPost.type;
using CRLocation = PX.Objects.CR.Standalone.Location;
using INTran = PX.Objects.IN.INTran;
using ReasonCodeSubAccountMaskAttribute = PX.Objects.CS.ReasonCodeSubAccountMaskAttribute;
using SOInvoice = PX.Objects.SO.SOInvoice;
using SOOrder = PX.Objects.SO.SOOrder;
using SOOrderShipment = PX.Objects.SO.SOOrderShipment;
using PX.Objects.IN.InventoryRelease.Accumulators.Statistics.ItemCustomer;

namespace PX.Objects.AR
{
	[Serializable]
	[PXProjection(typeof(Select2<ARRegister, 
		LeftJoin<ARDocumentRelease.ARInvoice, 
			On<ARDocumentRelease.ARInvoice.docType, Equal<ARRegister.docType>,
				And<ARDocumentRelease.ARInvoice.refNbr, Equal<ARRegister.refNbr>>>,
		LeftJoin<ARDocumentRelease.ARPayment, 
			On<ARDocumentRelease.ARPayment.docType, Equal<ARRegister.docType>,
				And<ARDocumentRelease.ARPayment.refNbr, Equal<ARRegister.refNbr>>>>>>))]
	public partial class BalancedARDocument : ARRegister
	{
		#region Status
		public new abstract class status : PX.Data.BQL.BqlString.Field<status> { }
		[PXDBString(1, IsFixed = true, BqlField = typeof(ARRegister.status))]
		[PXUIField(DisplayName = "Status", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
		[ARDocStatus.List()]
		public override String Status
		{
			get
			{
				return _Status;
			}
			set
			{
				_Status = value;
			}
		}
		#endregion
		#region InvoiceNbr
		public abstract class invoiceNbr : PX.Data.BQL.BqlString.Field<invoiceNbr> { }
		[PXDBString(40, IsUnicode = true, BqlField = typeof(ARDocumentRelease.ARInvoice.invoiceNbr))]
		public virtual String InvoiceNbr { get; set; }
		#endregion
		#region ExtRefNbr
		public abstract class extRefNbr : PX.Data.BQL.BqlString.Field<extRefNbr> { }
		[PXDBString(40, IsUnicode = true, BqlField = typeof(ARDocumentRelease.ARPayment.extRefNbr))]
		public virtual String ExtRefNbr { get; set; }
		#endregion
		#region CustomerRefNbr
		public abstract class customerRefNbr : PX.Data.BQL.BqlString.Field<customerRefNbr> { }
		[PXString(40, IsUnicode = true)]
		[PXUIField(DisplayName = "Customer Order Nbr.")]
		[PXFormula(typeof(IsNull<BalancedARDocument.invoiceNbr, BalancedARDocument.extRefNbr>))]
		public string CustomerRefNbr { get; set; }
		#endregion
		#region PaymentMethodID
		public abstract class paymentMethodID : PX.Data.BQL.BqlString.Field<paymentMethodID> { }
		[PXDBString(10, IsUnicode = true, BqlField = typeof(ARDocumentRelease.ARPayment.paymentMethodID))]
		[PXUIField(DisplayName = CA.Messages.PaymentMethod, Visible = false)]
		public virtual string PaymentMethodID { get; set; }
		#endregion
		#region OrigDocAmt
		public new abstract class origDocAmt : PX.Data.BQL.BqlDecimal.Field<origDocAmt> { }
		[PXDBBaseCury(BqlField = typeof(ARRegister.origDocAmt))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Amount")]
		public override Decimal? OrigDocAmt { get; set; }
		#endregion
		#region BranchID
		public new abstract class branchID : PX.Data.BQL.BqlInt.Field<branchID> { }
		#endregion

		public new abstract class selected : PX.Data.BQL.BqlBool.Field<selected> { }
		public new abstract class docType : PX.Data.BQL.BqlString.Field<docType> { }
		public new abstract class refNbr : PX.Data.BQL.BqlString.Field<refNbr> { }
		public new abstract class origModule : PX.Data.BQL.BqlString.Field<origModule> { }
		public new abstract class openDoc : PX.Data.BQL.BqlBool.Field<openDoc> { }
		public new abstract class released : PX.Data.BQL.BqlBool.Field<released> { }
		public new abstract class hold : PX.Data.BQL.BqlBool.Field<hold> { }
		public new abstract class scheduled : PX.Data.BQL.BqlBool.Field<scheduled> { }
		public new abstract class voided : PX.Data.BQL.BqlBool.Field<voided> { }
		public new abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID> { }
		public new abstract class lastModifiedByID : PX.Data.BQL.BqlGuid.Field<lastModifiedByID> { }
		public new abstract class isTaxValid : PX.Data.BQL.BqlBool.Field<isTaxValid> { }
		public new abstract class isTaxPosted : PX.Data.BQL.BqlBool.Field<isTaxPosted> { }
		public new abstract class isTaxSaved : PX.Data.BQL.BqlBool.Field<isTaxSaved> { }
		public new abstract class customerID : PX.Data.BQL.BqlInt.Field<customerID> { }
		public new abstract class adjCntr : PX.Data.BQL.BqlInt.Field<adjCntr> { }
		public new abstract class isMigratedRecord : PX.Data.BQL.BqlBool.Field<isMigratedRecord> { }
	}

	[Obsolete(Common.InternalMessages.ClassIsObsoleteRemoveInAcumatica2019R1)]
	public class PXMassProcessException : Common.PXMassProcessException
			{
		public PXMassProcessException(int ListIndex, Exception InnerException)
			: base(ListIndex, InnerException)
		{ }

		public PXMassProcessException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{ }
	}

	[PX.Objects.GL.TableAndChartDashboardType]
	public class ARDocumentRelease : PXGraph<ARDocumentRelease>
	{
		public PXCancel<BalancedARDocument> Cancel;
		[PXFilterable]
		[PX.SM.PXViewDetailsButton(typeof(BalancedARDocument.refNbr), WindowMode = PXRedirectHelper.WindowMode.NewWindow)]
		public PXProcessingJoin<BalancedARDocument,
			LeftJoin<ARInvoice, On<ARInvoice.docType, Equal<BalancedARDocument.docType>,
					And<ARInvoice.refNbr, Equal<BalancedARDocument.refNbr>>>,
				LeftJoin<ARPayment, On<ARPayment.docType, Equal<BalancedARDocument.docType>,
					And<ARPayment.refNbr, Equal<BalancedARDocument.refNbr>>>,
				InnerJoinSingleTable<Customer, On<Customer.bAccountID, Equal<BalancedARDocument.customerID>>>>>,
			    Where<Match<Customer, Current<AccessInfo.userName>>>>
			ARDocumentList;

		public PXSetup<ARSetup> arsetup;

		public static string[] TransClassesWithoutZeroPost = {GLTran.tranClass.Discount, GLTran.tranClass.RealizedAndRoundingGOL, GLTran.tranClass.WriteOff};

		public ARDocumentRelease()
		{
			ARSetup setup = arsetup.Current;
			ARDocumentList.SetProcessDelegate(
				delegate(List<BalancedARDocument> list)
				{
					List<ARRegister> newlist = new List<ARRegister>(list.Count);
					foreach (BalancedARDocument doc in list)
					{
						newlist.Add(doc);
					}
					ReleaseDoc(newlist, true);
				}
			);
			ARDocumentList.SetProcessCaption(Messages.Release);
			ARDocumentList.SetProcessAllCaption(Messages.ReleaseAll);
		}

		public delegate void ARMassProcessDelegate(ARRegister ardoc, bool isAborted);

		public delegate void ARMassProcessReleaseTransactionScopeDelegate(ARRegister ardoc);

		public static void ReleaseDoc(List<ARRegister> list, bool isMassProcess)
		{
			ReleaseDoc(list, isMassProcess, null, null);
		}

		public static void ReleaseDoc(List<ARRegister> list, bool isMassProcess, List<Batch> externalPostList) 
		{
			ReleaseDoc(list, isMassProcess, externalPostList, null);
		}

		public static void ReleaseDoc(List<ARRegister> list, bool isMassProcess, List<Batch> externalPostList, ARMassProcessDelegate onsuccess)
		{
			ReleaseDoc(list, isMassProcess, externalPostList, onsuccess, null);
		}

		/// <summary>
		/// Static function for release of AR documents and posting of the released batch.
		/// Released batches will be posted if the corresponded flag in ARSetup is set to true.
		/// SkipPost parameter is used to override this flag. 
		/// This function can not be called from inside of the covering DB transaction scope, unless skipPost is set to true.     
		/// </summary>
		/// <param name="list">List of the documents to be released</param>
		/// <param name="isMassProcess">Flag specifing if the function is called from mass process - affects error handling</param>
		/// <param name="externalPostList"> List of batches that should not be posted inside the release procedure</param>
		/// <param name="onsuccess"> Delegate to be called if release process completed successfully</param>
		/// <param name="onreleasecomplete"> Delegate to be called inside the transaction scope of AR document release process</param>
		public static void ReleaseDoc(List<ARRegister> list, bool isMassProcess, List<Batch> externalPostList, ARMassProcessDelegate onsuccess, ARMassProcessReleaseTransactionScopeDelegate onreleasecomplete)
		{
			bool failed = false;
			ARReleaseProcess rg = PXGraph.CreateInstance<ARReleaseProcess>();
			JournalEntry je = CreateJournalEntry();

			PostGraph pg = PXGraph.CreateInstance<PostGraph>();
			Dictionary<int, int> batchbind = new Dictionary<int, int>();
			List<Batch> pmBatchList = new List<Batch>();
			bool isSkipPost = externalPostList != null;

			for (int i = 0; i < list.Count; i++)
			{
				if (list[i] == null)
					continue;

				ARRegister doc = list[i];
				try
				{
					bool onefailed = false;
					rg.Clear();
					rg.VerifyInterBranchTransactions(doc);
				    doc = rg.OnBeforeRelease(doc);

					try
					{
						doc.ReleasedToVerify = (doc.Status == ARDocStatus.Open && doc.OpenDoc == true && doc.Released == true) ? (bool?)null : false;
						List<ARRegister> childs = rg.ReleaseDocProc(je, doc, pmBatchList, onreleasecomplete);
						
						object cached;
						if ((cached = rg.ARDocument.Cache.Locate(doc)) != null)
						{
							PXCache<ARRegister>.RestoreCopy(doc, (ARRegister)cached);
							doc.Selected = true;
						}

						int k;
						if ((k = je.created.IndexOf(je.BatchModule.Current)) >= 0 && batchbind.ContainsKey(k) == false)
						{
							batchbind.Add(k, i);
						}

						if (childs != null)
						{
							foreach (ARRegister child in childs)
							{
								var isSelfAppliedCrm = child.DocType == ARDocType.CreditMemo && child.DocType == doc.DocType && child.RefNbr == doc.RefNbr;
								var isOpenDoc = child.Status == ARDocStatus.Open && child.OpenDoc == true && child.Released == true;
								child.ReleasedToVerify = (isSelfAppliedCrm || isOpenDoc) ? (bool?)null : false;

								rg.Clear();
								rg.ReleaseDocProc(je, child, pmBatchList, null);

								if ((cached = rg.ARDocument.Cache.Locate(doc)) != null)
								{
									PXCache<ARRegister>.RestoreCopy(doc, (ARRegister)cached);
									doc.Selected = true;
								}
							}
						}
					}
					catch
					{
						je.Clear();
						je.CleanupCreated(batchbind.Keys);

						onefailed = true;
						throw;
					}
					finally
					{
						onsuccess?.Invoke(doc, onefailed);
					}

					if (isMassProcess)
					{
						if (string.IsNullOrEmpty(doc.WarningMessage))
							PXProcessing<ARRegister>.SetInfo(i, ActionsMessages.RecordProcessed);
						else
							PXProcessing<ARRegister>.SetWarning(i, doc.WarningMessage);
					}
				}
				catch (Exception e)
				{
					if (isMassProcess)
					{
						PXProcessing<ARRegister>.SetError(i, e);
						failed = true;
					}
					else
					{
						throw new PXMassProcessException(i, e);
					}
				}
			}

			if (isSkipPost) 
			{
				if (rg.AutoPost)
					externalPostList.AddRange(je.created);
			}
			else
			{
				for (int i = 0; i < je.created.Count; i++)
				{
					Batch batch = je.created[i];
					try
					{
						if (rg.AutoPost)
						{
							pg.Clear();
							pg.PostBatchProc(batch);
						}
					}
					catch (Exception e)
					{
						if (isMassProcess)
						{
							failed = true;
							PXProcessing<ARRegister>.SetError(batchbind[i], e);
						}
						else
						{
							throw new PXMassProcessException(batchbind[i], e);
						}
					}
				}
			}
			if (failed)
			{
				throw new PXOperationCompletedWithErrorException(GL.Messages.DocumentsNotReleased);
			}

			List<PM.ProcessInfo<Batch>> infoList = new List<ProcessInfo<Batch>>();
			ProcessInfo<Batch> processInfo = new ProcessInfo<Batch>(0);
			processInfo.Batches.AddRange(pmBatchList);
			infoList.Add(processInfo);
			PM.RegisterRelease.Post(infoList, isMassProcess);
		}

		public static JournalEntry CreateJournalEntry()
		{
			JournalEntry je = PXGraph.CreateInstance<JournalEntry>();
			SetContextForExtention(je);
			je.PrepareForDocumentRelease();
			je.RowInserting.AddHandler<GLTran>((sender, e) => { je.SetZeroPostIfUndefined((GLTran)e.Row, TransClassesWithoutZeroPost); });
			return je;
		}

		private static void SetContextForExtention(JournalEntry je)
		{
			var jeContextExt = je.GetExtension<JournalEntry.JournalEntryContextExt>();
			jeContextExt.GraphContext = JournalEntry.JournalEntryContextExt.Context.Release;
		}

		protected virtual IEnumerable ardocumentlist()
		{
			PXSelectBase<BalancedARDocument> selectDocumentsCommand = new PXSelectJoinGroupBy<BalancedARDocument,
				InnerJoinSingleTable<Customer, On<Customer.bAccountID, Equal<BalancedARDocument.customerID>>,
				LeftJoin<ARAdjust, On<ARAdjust.adjgDocType, Equal<BalancedARDocument.docType>,
					And<ARAdjust.adjgRefNbr, Equal<BalancedARDocument.refNbr>,
					And<ARAdjust.released, NotEqual<True>,
					And<ARAdjust.hold, Equal<False>>>>>,
				LeftJoin<ARInvoice, On<ARInvoice.docType, Equal<BalancedARDocument.docType>,
					And<ARInvoice.refNbr, Equal<BalancedARDocument.refNbr>>>,
				LeftJoin<ARPayment, On<ARPayment.docType, Equal<BalancedARDocument.docType>,
					And<ARPayment.refNbr, Equal<BalancedARDocument.refNbr>>>,
				LeftJoin<PaymentMethod, On<ARPayment.paymentMethodID, Equal<PaymentMethod.paymentMethodID>>>>>>>,
				Where2<
					Match<Customer, Current<AccessInfo.userName>>, 
					And2<Where<BalancedARDocument.status, Equal<ARDocStatus.balanced>, 
						Or<BalancedARDocument.status, Equal<ARDocStatus.open>,
						Or<BalancedARDocument.status, Equal<ARDocStatus.cCHold>>>>,
					And2<Where<
						ARInvoice.refNbr, IsNotNull, 
						Or<ARPayment.refNbr, IsNotNull>>,
					And2<Where<
						BalancedARDocument.released, Equal<False>, 
						And<BalancedARDocument.origModule, In3<GL.BatchModule.moduleAR, GL.BatchModule.moduleEP>, 
						Or<
							BalancedARDocument.released, Equal<True>,
							And<BalancedARDocument.openDoc, Equal<True>, 
							And<ARAdjust.adjdRefNbr, IsNotNull,
							And<ARAdjust.isInitialApplication, NotEqual<True>>>>>>>,
					And<BalancedARDocument.isMigratedRecord, Equal<Current<ARSetup.migrationMode>>,
					And2<Where<Not<PaymentMethod.paymentType, Equal<PaymentMethodType.creditCard>,
						And<PaymentMethod.paymentMethodID, IsNotNull,
						And<Current<ARSetup.integratedCCProcessing>, Equal<True>,
						And<PaymentMethod.aRIsProcessingRequired, Equal<True>,
						And<BalancedARDocument.status, Equal<ARDocStatus.cCHold>
						>>>>>>,
					And<Where<BalancedARDocument.released, Equal<True>,
						Or<ARInvoice.refNbr, IsNull, 
						Or<Where2<
							Where<Current<ARSetup.printBeforeRelease>, NotEqual<True>, 
								Or<ARInvoice.printed, Equal<True>, 
								Or<ARInvoice.dontPrint, Equal<True>>>>,
							And<Where<Current<ARSetup.emailBeforeRelease>, NotEqual<True>,
								Or<ARInvoice.emailed, Equal<True>,
								Or<ARInvoice.dontEmail, Equal<True>>>>>>>>>>>>>>>>,
				Aggregate<
				GroupBy<BalancedARDocument.docType, 
				GroupBy<BalancedARDocument.refNbr, 
				GroupBy<BalancedARDocument.released, 
				GroupBy<BalancedARDocument.openDoc, 
				GroupBy<BalancedARDocument.hold, 
				GroupBy<BalancedARDocument.scheduled, 
				GroupBy<BalancedARDocument.voided, 
				GroupBy<BalancedARDocument.createdByID, 
				GroupBy<BalancedARDocument.lastModifiedByID,
				GroupBy<BalancedARDocument.isTaxValid,
				GroupBy<BalancedARDocument.isTaxSaved,
				GroupBy<BalancedARDocument.isTaxPosted,
				GroupBy<ARInvoice.dontPrint,
				GroupBy<ARInvoice.printed,
				GroupBy<ARInvoice.dontEmail,
				GroupBy<ARInvoice.emailed>>>>>>>>>>>>>>>>>,
				OrderBy<Asc<BalancedARDocument.docType,
						Asc<BalancedARDocument.refNbr>>>>
					(this);

			int startRow = PXView.StartRow;
			int totalRows = 0;

			List<PXView.PXSearchColumn> searchColumns = ARDocumentList.View.GetContextualExternalSearchColumns();
			var select = selectDocumentsCommand.View.Select(
				null,
				null,
				searchColumns.GetSearches(),
				searchColumns.GetSortColumns(),
				searchColumns.GetDescendings(),
				ARDocumentList.View.GetExternalFilters(),
				ref startRow,
				PXView.MaximumRows,
				ref totalRows);
			foreach (PXResult<BalancedARDocument, Customer, ARAdjust, ARInvoice, ARPayment> res in select)
			{
				BalancedARDocument ardoc = (BalancedARDocument)res;
				ardoc = ARDocumentList.Locate(ardoc) ?? ardoc;
				ARInvoice invoice = (ARInvoice)res;
				ARPayment payment = (ARPayment)res;               

				if (invoice != null && string.IsNullOrEmpty(invoice.RefNbr) == false)
				{
					//FIXME: The grid sorting/filtering will be incorrect for the PendingPrint and PendingEmail statuses (like AC-89616)
					//if PrintBeforeRelease is off and EmailBeforeRelease is on document will be simply skipped
					if (ardoc.Released == false && ardoc.Status == ARDocStatus.PendingPrint && arsetup.Current.PrintBeforeRelease != true)
					{
						ardoc.Status = ARDocStatus.Balanced;
					}
					if (ardoc.Released == false && ardoc.Status == ARDocStatus.PendingEmail && arsetup.Current.EmailBeforeRelease != true)
					{
						ardoc.Status = ARDocStatus.Balanced;
					}
				}

				ARAdjust adj = res;
					if (adj.AdjdRefNbr != null)
					{
						ardoc.DocDate = adj.AdjgDocDate;
					    FinPeriodIDAttribute.SetPeriodsByMaster<ARRegister.finPeriodID>(ARDocumentList.Cache, ardoc, adj.AdjgTranPeriodID);
					}

					yield return new PXResult<BalancedARDocument, ARInvoice, ARPayment, Customer, ARAdjust>(ardoc, res, res, res, res);
				}				

			PXView.StartRow = 0;
		}
		
	   // [PXCurrency(typeof(ARRegister.curyInfoID), typeof(ARRegister.origDocAmtWithRetainageTotal), BaseCalc = false)]
	    //[PXUIField(DisplayName = "Total Amount", FieldClass = nameof(FeaturesSet.Retainage))]
	    //[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
	   // [PXFormula(typeof(Add<ARRegister.curyOrigDocAmt, ARRegister.curyRetainageTotal>))]
        [PXRemoveBaseAttribute(typeof(PXFormulaAttribute))]
        [PXRemoveBaseAttribute(typeof(PXCurrencyAttribute))]
        protected virtual void BalancedARDocument_CuryOrigDocAmtWithRetainageTotal_CacheAttached(PXCache sender)
	    {

	    }

		[PXCustomizeBaseAttribute(typeof(PXUIFieldAttribute), nameof(PXUIFieldAttribute.DisplayName), "Currency Amount")]
		protected virtual void BalancedARDocument_CuryOrigDocAmt_CacheAttached(PXCache sender) { }

		#region Type Override events

		#region BranchID

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXUIField(DisplayName = "Branch", Visible = false)]
		[PXUIVisible(typeof(FeatureInstalled<FeaturesSet.branch>.Or<FeatureInstalled<FeaturesSet.multiCompany>>))]
		protected virtual void _(Events.CacheAttached<BalancedARDocument.branchID> e) { }
		#endregion

		#endregion

		[PXHidden()]
		[Serializable()]
		public partial class ARInvoice : IBqlTable
		{
			#region DocType
			public abstract class docType : PX.Data.BQL.BqlString.Field<docType> { }
			[PXDBString(3, IsKey = true, IsFixed = true)]
			public virtual String DocType
			{
				get;
				set;
			}
			#endregion
			#region RefNbr
			public abstract class refNbr : PX.Data.BQL.BqlString.Field<refNbr> { }
			[PXDBString(15, IsKey = true, IsUnicode = true)]
			public virtual String RefNbr
			{
				get;
				set;
			}
			#endregion
			#region InvoiceNbr
			public abstract class invoiceNbr : PX.Data.BQL.BqlString.Field<invoiceNbr> { }
			[PXDBString(40, IsKey = true, IsUnicode = true)]
			public virtual String InvoiceNbr
			{
				get;
				set;
			}
			#endregion
			#region DontPrint
			public abstract class dontPrint : PX.Data.BQL.BqlBool.Field<dontPrint> { }
			protected Boolean? _DontPrint;
			[PXDBBool()]
			public virtual Boolean? DontPrint
			{
				get;
				set;
			}
			#endregion
			#region Printed
			public abstract class printed : PX.Data.BQL.BqlBool.Field<printed> { }
			protected Boolean? _Printed;
			[PXDBBool()]
			public virtual Boolean? Printed
			{
				get;
				set;
			}
			#endregion
			#region DontEmail
			public abstract class dontEmail : PX.Data.BQL.BqlBool.Field<dontEmail> { }
			protected Boolean? _DontEmail;
			[PXDBBool()]
			public virtual Boolean? DontEmail
			{
				get;
				set;
			}
			#endregion
			#region Emailed
			public abstract class emailed : PX.Data.BQL.BqlBool.Field<emailed> { }
			protected Boolean? _Emailed;
			[PXDBBool()]
			public virtual Boolean? Emailed
			{
				get;
				set;
			}
			#endregion
		}

		[PXHidden()]
		[Serializable()]
		public partial class ARPayment : IBqlTable
		{
			#region DocType
			public abstract class docType : PX.Data.BQL.BqlString.Field<docType> { }
			[PXDBString(3, IsKey = true, IsFixed = true)]
			public virtual String DocType
			{
				get;
				set;
			}
			#endregion
			#region RefNbr
			public abstract class refNbr : PX.Data.BQL.BqlString.Field<refNbr> { }
			[PXDBString(15, IsKey = true, IsUnicode = true)]
			public virtual String RefNbr
			{
				get;
				set;
			}
			#endregion
			#region ExtRefNbr
			public abstract class extRefNbr : PX.Data.BQL.BqlString.Field<extRefNbr> { }
			[PXDBString(40, IsUnicode = true)]
			public virtual String ExtRefNbr
			{
				get;
				set;
			}
			#endregion
			#region PMInstanceID
			public abstract class pMInstanceID : PX.Data.BQL.BqlInt.Field<pMInstanceID> { }
			[PXDBInt()]
			public virtual int? PMInstanceID
			{
				get;
				set;
			}
			#endregion
			#region PaymentMethodID
			public abstract class paymentMethodID : PX.Data.BQL.BqlString.Field<paymentMethodID> { }
			[PXDBString(10, IsUnicode = true)]
			public virtual string PaymentMethodID { get; set; }
			#endregion
			#region IsCCCaptured
			public abstract class isCCCaptured : PX.Data.BQL.BqlBool.Field<isCCCaptured> { }
			[PXDBBool]
			public virtual bool? IsCCCaptured
			{
				get;
				set;
			}
			#endregion
			#region IsCCCaptured
			public abstract class isCCRefunded : PX.Data.BQL.BqlBool.Field<isCCRefunded> { }
			[PXDBBool]
			public virtual bool? IsCCRefunded
			{
				get;
				set;
			}
			#endregion
		}
	}

	public class ARPayment_CurrencyInfo_Currency_Customer : PXSelectJoin<ARPayment, 
		InnerJoin<CurrencyInfo, On<CurrencyInfo.curyInfoID, Equal<ARPayment.curyInfoID>>, 
		InnerJoin<Currency, On<Currency.curyID, Equal<CurrencyInfo.curyID>>, 
		LeftJoin<Customer, On<Customer.bAccountID, Equal<ARPayment.customerID>>, 
		LeftJoin<CashAccount, On<CashAccount.cashAccountID, Equal<ARPayment.cashAccountID>>>>>>, 
		Where<ARPayment.docType, Equal<Required<ARPayment.docType>>, And<ARPayment.refNbr, Equal<Required<ARPayment.refNbr>>>>>
	{
		public ARPayment_CurrencyInfo_Currency_Customer(PXGraph graph)
			: base(graph)
		{
		}
	}

	public class ARInvoice_CurrencyInfo_Terms_Customer : PXSelectJoin<ARInvoice, 
		InnerJoin<CurrencyInfo, On<CurrencyInfo.curyInfoID, Equal<ARInvoice.curyInfoID>>, 
		LeftJoin<Terms, On<Terms.termsID, Equal<ARInvoice.termsID>>, 
		LeftJoin<Customer, On<Customer.bAccountID, Equal<ARInvoice.customerID>>, 
		LeftJoin<Account, On<ARInvoice.aRAccountID, Equal<Account.accountID>>>>>>, 
		Where<ARInvoice.docType, Equal<Required<ARInvoice.docType>>, And<ARInvoice.refNbr, Equal<Required<ARInvoice.refNbr>>>>>
	{
		public ARInvoice_CurrencyInfo_Terms_Customer(PXGraph graph)
			: base(graph)
		{
		}
	}

	public class ARReleaseProcess : PXGraph<ARReleaseProcess>
	{
		public class MultiCurrency : ARMultiCurrencyGraph<ARReleaseProcess, ARRegister>
		{
			protected override string DocumentStatus => Base.ARDocument.Current?.Status;

			protected override CurySource CurrentSourceSelect() => null;

			protected override DocumentMapping GetDocumentMapping()
			{
				return new DocumentMapping(typeof(ARRegister))
				{
					DocumentDate = typeof(ARRegister.docDate),
					BAccountID = typeof(ARRegister.customerID)
				};
			}

			protected override PXSelectBase[] GetChildren()
			{
				return new PXSelectBase[]
				{
					Base.ARTaxTran_TranType_RefNbr,
					Base.ARInvoice_DocType_RefNbr,
					Base.ARTran_TranType_RefNbr,
					Base.ARDoc_SalesPerTrans,
					Base.ARDocument
				};
			}
			protected override IEnumerable<Type> FieldWhichShouldBeRecalculatedAnyway
			{
				get
				{
					yield return typeof(ARInvoice.curyDiscBal);
				}
			}

			protected override void CuryRowInserting(PXCache sender, PXRowInsertingEventArgs e, List<CuryField> fields, Dictionary<Type, string> topCuryInfoIDs)
			{

			}

			public void UpdateCurrencyInfoForPrepayment(ARPayment prepayment, CurrencyInfo origCuryInfoToUse)
			{
				TrackedItems[Base.ARPayment_DocType_RefNbr.Cache.GetItemType()]
					.Single(f => f.CuryName.Equals(nameof(ARPayment.curyDocBal), StringComparison.OrdinalIgnoreCase))
					.BaseCalc = true;

				CurrencyInfo curyInfoToUse = PXCache<CurrencyInfo>.CreateCopy(origCuryInfoToUse);
				curyInfoToUse.CuryInfoID = prepayment.CuryInfoID;
				curyInfoToUse.IsReadOnly = false;
				currencyinfo.Cache.Update(curyInfoToUse);
			}
		}

		public PXSelect<ARRegister> ARDocument;

		public PXSelectJoin<
			ARTran, 
				LeftJoin<ARTax, 
					On<ARTax.tranType, Equal<ARTran.tranType>, 
					And<ARTax.refNbr, Equal<ARTran.refNbr>, 
					And<ARTax.lineNbr, Equal<ARTran.lineNbr>>>>, 
				LeftJoin<Tax, 
					On<Tax.taxID, Equal<ARTax.taxID>>, 
				LeftJoin<DRDeferredCode, 
					On<DRDeferredCode.deferredCodeID, Equal<ARTran.deferredCode>>, 
				LeftJoin<SO.SOOrderType, 
					On<SO.SOOrderType.orderType, Equal<ARTran.sOOrderType>>,
				LeftJoin<ARTaxTran,
					On<ARTaxTran.module, Equal<BatchModule.moduleAR>,
					And<ARTaxTran.tranType, Equal<ARTax.tranType>,
					And<ARTaxTran.refNbr, Equal<ARTax.refNbr>,
					And<ARTaxTran.taxID, Equal<ARTax.taxID>>>>>>>>>>, 
			Where<
				ARTran.tranType, Equal<Required<ARTran.tranType>>, 
				And<ARTran.refNbr, Equal<Required<ARTran.refNbr>>>>, 
			OrderBy<
				Asc<ARTran.lineNbr, 
				Asc<Tax.taxCalcLevel>>>> 
			ARTran_TranType_RefNbr;


		public PXSelectJoin<ARTaxTran,
			InnerJoin<Tax, On<Tax.taxID, Equal<ARTaxTran.taxID>>,
			LeftJoin<Account, On<Account.accountID, Equal<ARTaxTran.accountID>>>>, 
			Where<ARTaxTran.module, Equal<BatchModule.moduleAR>, 
				And<ARTaxTran.tranType, Equal<Required<ARTaxTran.tranType>>, 
				And<ARTaxTran.refNbr, Equal<Required<ARTaxTran.refNbr>>>>>, 
			OrderBy<
				Asc<Tax.taxCalcLevel>>> 
			ARTaxTran_TranType_RefNbr;

		public PXSelect<SVATConversionHist> SVATConversionHistory;

		public PXSelect<Batch> Batch;

		public ARInvoice_CurrencyInfo_Terms_Customer ARInvoice_DocType_RefNbr;
		public ARPayment_CurrencyInfo_Currency_Customer ARPayment_DocType_RefNbr;

		public PXSelect<CurrencyInfo, Where<CurrencyInfo.curyInfoID, Equal<Required<CurrencyInfo.curyInfoID>>>> CurrencyInfo_CuryInfoID;

		public PXSelectJoin<
			ARAdjust, 
				InnerJoin<CurrencyInfo, 
					On<CurrencyInfo.curyInfoID, Equal<ARAdjust.adjdCuryInfoID>>, 
				InnerJoin<Currency, 
					On<Currency.curyID, Equal<CurrencyInfo.curyID>>, 
				InnerJoin<ARRegister,
					On<ARRegister.docType, Equal<ARAdjust.adjdDocType>, 
					And<ARRegister.refNbr, Equal<ARAdjust.adjdRefNbr>>>,
				LeftJoinSingleTable<ARInvoice, 
					On<ARInvoice.docType, Equal<ARAdjust.adjdDocType>, 
					And<ARInvoice.refNbr, Equal<ARAdjust.adjdRefNbr>>>, 
				LeftJoinSingleTable<ARPayment, 
					On<ARPayment.docType, Equal<ARAdjust.adjdDocType>, 
					And<ARPayment.refNbr, Equal<ARAdjust.adjdRefNbr>>>,
				LeftJoin<ARTran, On<ARRegister.paymentsByLinesAllowed, Equal<True>,
					And<ARTran.tranType, Equal<ARAdjust.adjdDocType>,
					And<ARTran.refNbr, Equal<ARAdjust.adjdRefNbr>,
					And<ARTran.lineNbr, Equal<ARAdjust.adjdLineNbr>>>>>,
				LeftJoin<CM.CurrencyInfo2, 
					On<CM.CurrencyInfo2.curyInfoID, Equal<ARRegister.curyInfoID>>>>>>>>>, 
			Where<
				ARAdjust.adjgDocType, Equal<Required<ARAdjust.adjgDocType>>, 
				And<ARAdjust.adjgRefNbr, Equal<Required<ARAdjust.adjgRefNbr>>, 
				And<Where<
					Switch<
						Case<Where<Required<ARAdjust.released>, Equal<True>>,
							IIf<Where<ARAdjust.adjNbr, Equal<Required<ARAdjust.adjNbr>>>, True, False>>,
						IIf<Where<ARAdjust.released, NotEqual<True>>, True, False>>, Equal<True>
				>>>>,
			OrderBy<
				Asc<ARAdjust.adjdDocType, 
				Asc<ARAdjust.adjdRefNbr, 
				Asc<ARAdjust.adjdLineNbr>>>>>
			ARAdjust_AdjgDocType_RefNbr_CustomerID;

		public PXSelectJoin<
			ARAdjust, 
				InnerJoin<CurrencyInfo, 
					On<CurrencyInfo.curyInfoID, Equal<ARAdjust.adjdCuryInfoID>>, 
				InnerJoin<Currency, 
					On<Currency.curyID, Equal<CurrencyInfo.curyID>>, 
				LeftJoin<ARPayment, 
					On<ARPayment.docType, Equal<ARAdjust.adjgDocType>, 
					And<ARPayment.refNbr, Equal<ARAdjust.adjgRefNbr>>>>>>, 
			Where<
				ARAdjust.adjdDocType, Equal<Required<ARAdjust.adjdDocType>>, 
				And<ARAdjust.adjdRefNbr, Equal<Required<ARAdjust.adjdRefNbr>>, 
				And<Where<
					ARAdjust.released, NotEqual<True>,
					Or<Required<ARAdjust.released>, Equal<True>,
					And<ARAdjust.adjNbr, Equal<Required<ARAdjust.adjNbr>>>>>>>>>
			ARAdjust_AdjdDocType_RefNbr_CustomerID;
							
		public PXSelect<ARPaymentChargeTran, Where<ARPaymentChargeTran.docType, Equal<Required<ARPaymentChargeTran.docType>>, And<ARPaymentChargeTran.refNbr, Equal<Required<ARPaymentChargeTran.refNbr>>>>> ARPaymentChargeTran_DocType_RefNbr;

		public PXSelect<ARSalesPerTran, Where<ARSalesPerTran.docType, Equal<Required<ARSalesPerTran.docType>>,
											And<ARSalesPerTran.refNbr, Equal<Required<ARSalesPerTran.refNbr>>>>> ARDoc_SalesPerTrans;

		public PXSelect<ARTranPost,
			Where<ARTranPost.docType, Equal<Required<ARRegister.docType>>,
				And<ARTranPost.refNbr, Equal<Required<ARRegister.refNbr>>>>> TranPost;

		public PXSelect<CATran> CashTran;
		public PXSetup<GLSetup> glsetup;
		public PXSetup<DRSetup> drSetup;

		public PXSelect<Tax> taxes;
		
		public PXSelect<SOOrder> soOrder;
		public PXSelect<SOAdjust> soAdjust;


		private ARSetup _arsetup;
		public ARSetup arsetup
		{
			get
			{
				_arsetup = (_arsetup ?? PXSetup<ARSetup>.Select(this));
				return _arsetup;
			}
		}

		public PXSelect<CADailySummary> caDailySummary;

		public bool AutoPost => arsetup.AutoPost == true;

		public bool SummPost => arsetup.TransactionPosting == AccountPostOption.Summary;

		public string InvoiceRounding => arsetup.InvoiceRounding;

		public decimal? InvoicePrecision => arsetup.InvoicePrecision;

		public bool? IsMigrationMode => arsetup.MigrationMode;

		public struct ARTranPostKey
		{
			public ARTranPostKey(string docType, string refNbr, int lineNbr)
			{
				DocType = docType;
				RefNbr = refNbr;
				LineNbr = lineNbr;
			}

			public string DocType;
			public string RefNbr;
			public int LineNbr;

			public override int GetHashCode()
			{
				return Tuple.Create(DocType, RefNbr, LineNbr).GetHashCode();
			}

			public override bool Equals(object obj)
			{
				ARTranPostKey value = (ARTranPostKey)obj;

				return DocType == value.DocType && RefNbr == value.RefNbr && LineNbr == value.LineNbr;
			}
		}

		public Dictionary<ARTranPostKey, ARTranPost> tranPostRetainagePayments;

		[InjectDependency]
		public IFinPeriodRepository FinPeriodRepository { get; set; }

		[InjectDependency]
		public IFinPeriodUtils FinPeriodUtils { get; set; }

		public bool IsMigratedDocumentForProcessing(ARRegister doc)
		{
			// CashSale and CashReturn documents
			// will be processed the same way as for normal mode,
			// but GL transactions will not be created.
			// 
			bool isCashSaleOrCashReturnDocument = doc.DocType == ARDocType.CashSale ||
				doc.DocType == ARDocType.CashReturn;

			return
				doc.IsMigratedRecord == true &&
				doc.Released != true &&
				doc.CuryInitDocBal != doc.CuryOrigDocAmt &&
				!isCashSaleOrCashReturnDocument;
		}

		protected ARInvoiceEntry _ie = null;
		public ARInvoiceEntry InvoiceEntryGraph
		{
			get
			{
				_ie = (_ie ?? PXGraph.CreateInstance<ARInvoiceEntry>());
				return _ie;
			}
		}

		protected ARPaymentEntry _pe = null;
		public ARPaymentEntry pe
		{
			get
			{
				_pe = (_pe ?? PXGraph.CreateInstance<ARPaymentEntry>());
				return _pe;
			}
		}

		protected DRProcess _dr = null;
		public DRProcess dr
		{
			get
			{
				_dr = (_dr ?? CreateDRProcess());
				return _dr;
			}
		}

		protected virtual DRProcess CreateDRProcess()
		{
			return PXGraph.CreateInstance<DRProcess>();
		}

		protected virtual void ReplaceCADailySummaryCache(JournalEntry je)
		{
			if (je.Caches[typeof(CADailySummary)].Current is CADailySummary daily)
			{
				this.caDailySummary.Insert(daily);
				je.Views.Caches.Remove(typeof(CADailySummary));
			}
		}

		#region Cache Attached

		[PXDBString(1, IsFixed = true)]
		public virtual void Tax_TaxType_CacheAttached(PXCache sender) { }

		[PXDBString(1, IsFixed = true)]
		public virtual void Tax_TaxCalcLevel_CacheAttached(PXCache sender) { }

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXRemoveBaseAttribute(typeof(PXFormulaAttribute))]
		protected virtual void ARInvoice_CuryApplicationBalance_CacheAttached(PXCache sender) { }

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXRemoveBaseAttribute(typeof(PXFormulaAttribute))]
		protected virtual void ARInvoice_ApplicationBalance_CacheAttached(PXCache sender) { }
		[PXMergeAttributes(Method = MergeMethod.Replace)]
		[PXDBInt]
		protected virtual void ARTranPost_AccountID_CacheAttached(PXCache sender) { }
		[PXMergeAttributes(Method = MergeMethod.Replace)]
		[PXDBInt]
		protected virtual void ARTranPost_SubID_CacheAttached(PXCache sender) { }
		[PXMergeAttributes(Method = MergeMethod.Replace)]
		[PXDBInt]
		protected virtual void ARTranPost_CustomerID_CacheAttached(PXCache sender) { }
		[PXMergeAttributes(Method = MergeMethod.Replace)]
		[PXDBInt]
		protected virtual void ARTranPost_BranchID_CacheAttached(PXCache sender) { }

		#endregion

		public ARReleaseProcess()
		{
			this.Defaults.Remove(typeof(ARSetup));

			OpenPeriodAttribute.SetValidatePeriod<ARRegister.finPeriodID>(ARDocument.Cache, null, PeriodValidation.Nothing);
			OpenPeriodAttribute.SetValidatePeriod<ARPayment.adjFinPeriodID>(ARPayment_DocType_RefNbr.Cache, null, PeriodValidation.Nothing);

			PXCache cacheARAdjust = Caches[typeof(ARAdjust)];

		    cacheARAdjust.Adjust<FinPeriodIDAttribute>()
		        .For<ARAdjust.adjgFinPeriodID>(attr =>
		        {
		            attr.AutoCalculateMasterPeriod = false;
		            attr.CalculatePeriodByHeader = false;
		            attr.HeaderFindingMode = FinPeriodIDAttribute.HeaderFindingModes.Parent;
		        })
		        .SameFor<ARAdjust.adjdFinPeriodID>();

			PXDBDefaultAttribute.SetDefaultForUpdate<ARAdjust.customerID>(cacheARAdjust, null, false);
			PXDBDefaultAttribute.SetDefaultForUpdate<ARAdjust.adjgDocType>(cacheARAdjust, null, false);
			PXDBDefaultAttribute.SetDefaultForUpdate<ARAdjust.adjgRefNbr>(cacheARAdjust, null, false);
			PXDBDefaultAttribute.SetDefaultForUpdate<ARAdjust.adjgCuryInfoID>(cacheARAdjust, null, false);
			PXDBDefaultAttribute.SetDefaultForUpdate<ARAdjust.adjgDocDate>(cacheARAdjust, null, false);

			PXCache cacheARTran = Caches[typeof(ARTran)];

		    cacheARTran.Adjust<FinPeriodIDAttribute>()
		        .For<ARTran.finPeriodID>(attr =>
		        {
		            attr.HeaderFindingMode = FinPeriodIDAttribute.HeaderFindingModes.Parent;
		        });
			PXDBDefaultAttribute.SetDefaultForInsert<ARTran.tranType>(cacheARTran, null, false);
			PXDBDefaultAttribute.SetDefaultForInsert<ARTran.refNbr>(cacheARTran, null, false);
			PXDBDefaultAttribute.SetDefaultForUpdate<ARTran.tranType>(cacheARTran, null, false);
			PXDBDefaultAttribute.SetDefaultForUpdate<ARTran.refNbr>(cacheARTran, null, false);
			PXDBDefaultAttribute.SetDefaultForUpdate<ARTran.curyInfoID>(cacheARTran, null, false);
			PXDBDefaultAttribute.SetDefaultForUpdate<ARTran.tranDate>(cacheARTran, null, false);
			PXDBDefaultAttribute.SetDefaultForUpdate<ARTran.customerID>(cacheARTran, null, false);

			PXCache cacheARTaxTran = Caches[typeof(ARTaxTran)];

			PXDBDefaultAttribute.SetDefaultForUpdate<ARTaxTran.tranType>(cacheARTaxTran, null, false);
			PXDBDefaultAttribute.SetDefaultForUpdate<ARTaxTran.refNbr>(cacheARTaxTran, null, false);
			PXDBDefaultAttribute.SetDefaultForUpdate<ARTaxTran.curyInfoID>(cacheARTaxTran, null, false);
			PXDBDefaultAttribute.SetDefaultForUpdate<ARTaxTran.tranDate>(cacheARTaxTran, null, false);
			PXDBDefaultAttribute.SetDefaultForUpdate<ARTaxTran.taxZoneID>(cacheARTaxTran, null, false);

			PXFormulaAttribute.SetAggregate<ARAdjust.curyAdjgAmt>(cacheARAdjust, null);
			PXFormulaAttribute.SetAggregate<ARAdjust.curyAdjdAmt>(cacheARAdjust, null);
			PXFormulaAttribute.SetAggregate<ARAdjust.adjAmt>(cacheARAdjust, null);

			if (IsMigrationMode == true)
			{
				PXDBDefaultAttribute.SetDefaultForInsert<ARAdjust.adjgDocDate>(cacheARAdjust, null, false);
			}
		}

		protected virtual void ARPayment_CashAccountID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			e.Cancel = true;
		}

		protected virtual void ARPayment_PMInstanceID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			e.Cancel = true;
		}

		protected virtual void ARPayment_PaymentMethodID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			e.Cancel = true;
		}

		protected virtual void ARPayment_ExtRefNbr_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			e.Cancel = true;
		}

		protected virtual void ARRegister_FinPeriodID_CommandPreparing(PXCache sender, PXCommandPreparingEventArgs e)
		{
			if ((e.Operation & PXDBOperation.Command) == PXDBOperation.Update)
			{
			    e.ExcludeFromInsertUpdate();
			}
		}

		protected virtual void ARRegister_TranPeriodID_CommandPreparing(PXCache sender, PXCommandPreparingEventArgs e)
		{
			if ((e.Operation & PXDBOperation.Command) == PXDBOperation.Update)
			{
			    e.ExcludeFromInsertUpdate();
			}
		}

		protected virtual void ARRegister_DocDate_CommandPreparing(PXCache sender, PXCommandPreparingEventArgs e)
		{
			if ((e.Operation & PXDBOperation.Command) == PXDBOperation.Update)
			{
                e.ExcludeFromInsertUpdate();
			}
		}

		protected virtual void ARAdjust_AdjdRefNbr_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			e.Cancel = true;
		}

		protected virtual void ARTran_RowUpdating(PXCache sender, PXRowUpdatingEventArgs e)
		{
		}

		protected virtual void ARTran_TaxCategoryID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			e.NewValue = null;
			e.Cancel = true;
		}

		protected virtual void ARTran_SalesPersonID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			e.NewValue = null;
			e.Cancel = true;
		}
		protected void _(Events.RowPersisting<ARTranPost> e)
		{
			if (e.Operation == PXDBOperation.Insert &&
			    (e.Row.Type == ARTranPost.type.RGOL || e.Row.Type == ARTranPost.type.Rounding) &&
			    e.Row.CuryAmt == 0 && e.Row.Amt == 0 && 
			    e.Row.RGOLAmt == 0 && e.Row.WOAmt == 0 && e.Row.DiscAmt == 0)
			{
				//Suppress inserting zero RGOL & Rounding transactions
				e.Cancel = true;
			}
		}

		private ARHist CreateHistory(int? BranchID, int? AccountID, int? SubID, int? CustomerID, string PeriodID)
		{
			ARHist accthist = new ARHist();
			accthist.BranchID = BranchID;
			accthist.AccountID = AccountID;
			accthist.SubID = SubID;
			accthist.CustomerID = CustomerID;
			accthist.FinPeriodID = PeriodID;
			return (ARHist)Caches[typeof(ARHist)].Insert(accthist);
		}

		private CuryARHist CreateHistory(int? BranchID, int? AccountID, int? SubID, int? CustomerID, string CuryID, string PeriodID)
		{
			CuryARHist accthist = new CuryARHist();
			accthist.BranchID = BranchID;
			accthist.AccountID = AccountID;
			accthist.SubID = SubID;
			accthist.CustomerID = CustomerID;
			accthist.CuryID = CuryID;
			accthist.FinPeriodID = PeriodID;
			return (CuryARHist)Caches[typeof(CuryARHist)].Insert(accthist);
		}

		private class ARHistItemDiscountsBucket : ARHistBucket
		{
			public ARHistItemDiscountsBucket(ARTran tran)
				: base()
			{
				switch (tran.TranType)
				{
					case ARDocType.Invoice:
					case ARDocType.DebitMemo:
					case ARDocType.CashSale:
						SignPtdItemDiscounts = 1m;
						break;
					case ARDocType.CreditMemo:
					case ARDocType.CashReturn:
						SignPtdItemDiscounts = -1m;
						break;
				}
			}
		}

		private class ARHistBucket
		{
			public int? arAccountID = null;
			public int? arSubID = null;
			public decimal SignPayments = 0m;
			public decimal SignDeposits = 0m;
			public decimal SignSales = 0m;
			public decimal SignFinCharges = 0m;
			public decimal SignCrMemos = 0m;
			public decimal SignDrMemos = 0m;
			public decimal SignDiscTaken = 0m;
			public decimal SignRGOL = 0m;
			public decimal SignPtd = 0m;
			public decimal SignPtdItemDiscounts = 0m;
			public decimal SignRetainageWithheld = 0m;
			public decimal SignRetainageReleased = 0m;

			public ARHistBucket(GLTran tran, string TranType)
			{
				arAccountID = tran.AccountID;
				arSubID = tran.SubID;

				switch (TranType + tran.TranClass)
				{
					case "CSLN":
						SignSales = -1m;
						SignPayments = -1m;
						SignPtd = 0m;
						break;
					case "RCSN":
						SignSales = -1m;
						SignPayments = -1m;
						SignPtd = 0m;
						break;
					case "INVN":
						SignSales = 1m;
						SignPtd = 1m;
						break;
					case "INVE":
						arAccountID = tran.OrigAccountID;
						arSubID = tran.OrigSubID;
						SignRetainageWithheld = 1m;
						break;
					case "INVF":
						SignSales = 1m;
						SignRetainageReleased = 1m;
						SignPtd = 1m;
						break;
					case "DRMN":
						SignDrMemos = 1m;
						SignPtd = 1m;
						break;
					case "DRME":
						arAccountID = tran.OrigAccountID;
						arSubID = tran.OrigSubID;
						SignRetainageWithheld = 1m;
						break;
					case "DRMF":
						SignDrMemos = 1m;
						SignRetainageReleased = 1m;
						SignPtd = 1m;
						break;
					case "FCHN":
						SignFinCharges = 1m;
						SignPtd = 1m;
						break;
					case "CRMP":
					case "CRMN":
						SignCrMemos = -1m;
						SignPtd = 1m;
						break;
					case "CRMR":
						arAccountID = tran.OrigAccountID;
						arSubID = tran.OrigSubID;
						SignCrMemos = -1m;
						SignRGOL = 1m;
						break;
					case "CRMD":
						arAccountID = tran.OrigAccountID;
						arSubID = tran.OrigSubID;
						SignCrMemos = -1m;
						SignDiscTaken = 1m;
						break;
					case "CRMB":
						arAccountID = tran.OrigAccountID;
						arSubID = tran.OrigSubID;
						SignCrMemos = 0m;
						break;
					case "CRME":
						arAccountID = tran.OrigAccountID;
						arSubID = tran.OrigSubID;
						SignRetainageWithheld = 1m;
						break;
					case "CRMF":
						SignCrMemos = -1m;
						SignRetainageReleased = 1m;
						SignPtd = 1m;
						break;
					case "PPMP":
						SignDeposits = -1m;
						break;
					case "PPMU":
						arAccountID = tran.OrigAccountID;
						arSubID = tran.OrigSubID;
						SignDeposits = -1m;
						SignDrMemos = -1m;
						SignPtd = -1m;
						break;
					case "PMTU":
						arAccountID = tran.OrigAccountID;
						arSubID = tran.OrigSubID;
						SignPayments = -1m;
						SignDrMemos = -1m;
						break;
					case "RPMP":						
					case "RPMN":						
					case "PMTP":						
					case "PMTN":
					case "PPMN":
					case "REFP":
					case "VRFP":
					case "REFN":
					case "VRFN":
						SignPayments = -1m;
						SignPtd = 1m;
						break;
					case "REFU":
					case "VRFU":
						SignDeposits = -1m;
						break;
					case "RPMR":
					case "PPMR":
					case "PMTR":
					case "REFR":
					case "VRFR":
					case "CSLR":
					case "RCSR":
					case "PPIR":
						arAccountID = tran.OrigAccountID;
						arSubID = tran.OrigSubID;
						SignPayments = -1m;
						SignRGOL = 1m;
						break;
					case "RPMD":
					case "PPMD":
					case "PMTD":
					case "REFD": //not really happens
					case "VRFD":
					case "CSLD":
					case "RCSD":
						arAccountID = tran.OrigAccountID;
						arSubID = tran.OrigSubID;
						SignPayments = -1m;
						SignDiscTaken = 1m;
						break;
					case "SMCP":
						//Zero Update
						//will insert SCWO Account in ARHistory for trial balance report
						//arAccountID = tran.OrigAccountID;
						//arSubID = tran.OrigSubID;
						break;
					case "SMCN":
						SignDrMemos = 1m;
						SignPtd = 1m;
						break;
					case "SMBP":
						//Zero Update
						//will insert SBWO Account in ARHistory for trial balance report
						break;
					case "SMBD": //not really happens
						//Zero Update
						arAccountID = tran.OrigAccountID;
						arSubID = tran.OrigSubID;
						break;
					case "SMBN":
						SignCrMemos = -1m;
						SignPtd = 1m;
						break;
					case "SMBR":
						arAccountID = tran.OrigAccountID;
						arSubID = tran.OrigSubID;
						SignCrMemos = -1m;
						SignRGOL = 1m;
						break;
					case "RPMB":
					case "PPMB":
					case "REFB": //not really happens
					case "VRFB":
					case "CSLB":
					case "RCSB":
					case "PMTB":
					case "SMBB":
						arAccountID = tran.OrigAccountID;
						arSubID = tran.OrigSubID;
						SignPayments = -1m;
						SignCrMemos = 1m;
						break;
					case "PPIN":
						SignPayments = -1m;
						SignPtd = 1m;
						break;
					case "PPIP":
					case "PPIY":
					case "PPMY":
						SignDeposits = -1m;
						break;
					case "PMTY":
						SignDeposits = 1m;
						SignPayments = -1m;
						SignPtd = 1m;
						break;
					case "CRMY":
						SignDeposits = -1m;
						break;
					case "PMTZ":
					case "PPMZ":
						//Zero Update
						break;
				}
			}

			public ARHistBucket()
			{
			}
		}

		private void UpdateHist<History>(History accthist, ARHistBucket bucket, bool FinFlag, GLTran tran)
			where History : class, IBaseARHist
		{
			if (_IsIntegrityCheck == false || accthist.DetDeleted == false)
			{
				decimal? amount = tran.DebitAmt - tran.CreditAmt;

				accthist.FinFlag = FinFlag;
				accthist.PtdPayments += bucket.SignPayments * amount;
				accthist.PtdSales += bucket.SignSales * amount;
				accthist.PtdDrAdjustments += bucket.SignDrMemos * amount;
				accthist.PtdCrAdjustments += bucket.SignCrMemos * amount;
				accthist.PtdFinCharges += bucket.SignFinCharges * amount;
				accthist.PtdDiscounts += bucket.SignDiscTaken * amount;
				accthist.PtdRGOL += bucket.SignRGOL * amount;
				accthist.YtdBalance += bucket.SignPtd * amount;
				accthist.PtdDeposits += bucket.SignDeposits * amount;
				accthist.YtdDeposits += bucket.SignDeposits * amount;
				accthist.PtdItemDiscounts += bucket.SignPtdItemDiscounts * amount;
				accthist.YtdRetainageReleased += bucket.SignRetainageReleased * amount;
				accthist.PtdRetainageReleased += bucket.SignRetainageReleased * amount;
				accthist.YtdRetainageWithheld += bucket.SignRetainageWithheld * amount;
				accthist.PtdRetainageWithheld += bucket.SignRetainageWithheld * amount;
			}
		}

		private void UpdateFinHist<History>(History accthist, ARHistBucket bucket, GLTran tran)
			where History : class, IBaseARHist
		{
			UpdateHist<History>(accthist, bucket, true, tran);
		}

		private void UpdateTranHist<History>(History accthist, ARHistBucket bucket, GLTran tran)
			where History : class, IBaseARHist
		{
			UpdateHist<History>(accthist, bucket, false, tran);
		}

		private void CuryUpdateHist<History>(History accthist, ARHistBucket bucket, bool FinFlag, GLTran tran)
			where History : class, ICuryARHist, IBaseARHist
		{
			if (_IsIntegrityCheck == false || accthist.DetDeleted == false)
			{
				UpdateHist<History>(accthist, bucket, FinFlag, tran);

				decimal? amount = tran.CuryDebitAmt - tran.CuryCreditAmt;

				accthist.FinFlag = FinFlag;
				accthist.CuryPtdPayments += bucket.SignPayments * amount;
				accthist.CuryPtdSales += bucket.SignSales * amount;
				accthist.CuryPtdDrAdjustments += bucket.SignDrMemos * amount;
				accthist.CuryPtdCrAdjustments += bucket.SignCrMemos * amount;
				accthist.CuryPtdFinCharges += bucket.SignFinCharges * amount;
				accthist.CuryPtdDiscounts += bucket.SignDiscTaken * amount;
				accthist.CuryYtdBalance += bucket.SignPtd * amount;
				accthist.CuryPtdDeposits += bucket.SignDeposits * amount;
				accthist.CuryYtdDeposits += bucket.SignDeposits * amount;
				accthist.CuryYtdRetainageReleased += bucket.SignRetainageReleased * amount;
				accthist.CuryPtdRetainageReleased += bucket.SignRetainageReleased * amount;
				accthist.CuryYtdRetainageWithheld += bucket.SignRetainageWithheld * amount;
				accthist.CuryPtdRetainageWithheld += bucket.SignRetainageWithheld * amount;
			}
		}

		private void CuryUpdateFinHist<History>(History accthist, ARHistBucket bucket, GLTran tran)
			where History : class, ICuryARHist, IBaseARHist
		{
			CuryUpdateHist<History>(accthist, bucket, true, tran);
		}

		private void CuryUpdateTranHist<History>(History accthist, ARHistBucket bucket, GLTran tran)
			where History : class, ICuryARHist, IBaseARHist
		{
			CuryUpdateHist<History>(accthist, bucket, false, tran);
		}

		private bool IsNeedUpdateHistoryForTransaction(string TranPeriodID)
		{
			if (!_IsIntegrityCheck) return true;

			return string.Compare(TranPeriodID, _IntegrityCheckStartingPeriod) >= 0;
		}

		protected void UpdateItemDiscountsHistory(ARTran tran, ARRegister ardoc)
		{
			if (!IsNeedUpdateHistoryForTransaction(tran.TranPeriodID))
			{
				return;
			}

			ARHistBucket bucket = new ARHistItemDiscountsBucket(tran);

			{
				ARHist accthist = CreateHistory(ardoc.BranchID, ardoc.ARAccountID, ardoc.ARSubID, ardoc.CustomerID, ardoc.FinPeriodID);
				if (accthist != null)
				{
					UpdateFinHist<ARHist>(accthist, bucket, new GLTran { DebitAmt = tran.DiscAmt, CreditAmt = 0m });
				}
			}

			{
				ARHist accthist = CreateHistory(ardoc.BranchID, ardoc.ARAccountID, ardoc.ARSubID, ardoc.CustomerID, ardoc.TranPeriodID);
				if (accthist != null)
				{
					UpdateTranHist<ARHist>(accthist, bucket, new GLTran { DebitAmt = tran.DiscAmt, CreditAmt = 0m });
				}
			}
		}

		private void UpdateHistory(GLTran tran, Customer cust)
		{
			ARHistBucket bucket = new ARHistBucket(tran, GetHistTranType(tran.TranType, tran.RefNbr));
			UpdateHistory(tran, cust, bucket);
		}

		private void UpdateHistory(GLTran tran, Customer cust, ARHistBucket bucket)
		{
			if (!IsNeedUpdateHistoryForTransaction(tran.TranPeriodID))
			{
				return;
			}

			{
				ARHist accthist = CreateHistory(tran.BranchID, bucket.arAccountID, bucket.arSubID, cust.BAccountID, tran.FinPeriodID);
				if (accthist != null)
				{
					UpdateFinHist<ARHist>(accthist, bucket, tran);
				}
			}

			{
				ARHist accthist = CreateHistory(tran.BranchID, bucket.arAccountID, bucket.arSubID, cust.BAccountID, tran.TranPeriodID);
				if (accthist != null)
				{
					UpdateTranHist<ARHist>(accthist, bucket, tran);
				}
			}
		}

		private void UpdateHistory(GLTran tran, Customer cust, CurrencyInfo info)
		{
			ARHistBucket bucket = new ARHistBucket(tran, GetHistTranType(tran.TranType, tran.RefNbr));
			UpdateHistory(tran, cust, info, bucket);
		}

		private void UpdateHistory(GLTran tran, Customer cust, CurrencyInfo info, ARHistBucket bucket)
		{
			if (!IsNeedUpdateHistoryForTransaction(tran.TranPeriodID))
			{
				return;
			}

			{
				CuryARHist accthist = CreateHistory(tran.BranchID, bucket.arAccountID, bucket.arSubID, cust.BAccountID, info.CuryID, tran.FinPeriodID);
				if (accthist != null)
				{
					CuryUpdateFinHist<CuryARHist>(accthist, bucket, tran);
				}
			}

			{
				CuryARHist accthist = CreateHistory(tran.BranchID, bucket.arAccountID, bucket.arSubID, cust.BAccountID, info.CuryID, tran.TranPeriodID);
				if (accthist != null)
				{
					CuryUpdateTranHist<CuryARHist>(accthist, bucket, tran);
				}
			}
		}

		private string GetHistTranType(string tranType, string refNbr)
		{
			string HistTranType = tranType;
			if (tranType == ARDocType.VoidPayment)
			{
				ARRegister doc = PXSelect<ARRegister, 
					Where<ARRegister.refNbr, Equal<Required<ARRegister.refNbr>>, 
						And<Where<ARRegister.docType, Equal<ARDocType.payment>, 
							Or<ARRegister.docType, Equal<ARDocType.prepayment>>>>>, 
					OrderBy<Asc<Switch<Case<Where<ARRegister.docType, Equal<ARDocType.payment>>, int0>, int1>, 
						Asc<ARRegister.docType, 
						Asc<ARRegister.refNbr>>>>>.Select(this, refNbr);
				if (doc != null)
				{
					HistTranType = doc.DocType;
				}
            }
            if (tranType == ARDocType.VoidRefund)
            {
                HistTranType = ARDocType.Refund;
            }

            return HistTranType;
		}

		private List<ARRegister> CreateInstallments(PXResult<ARInvoice, CurrencyInfo, Terms, Customer> res)
		{
			ARInvoice ardoc = (ARInvoice)res;
			CurrencyInfo info = (CurrencyInfo)res;
			Terms terms = (Terms)res;
			Customer customer = (Customer)res;
			List<ARRegister> ret = new List<ARRegister>();

			decimal CuryTotalInstallments = 0m;
			decimal BaseTotalInstallments = 0m;

			ARInvoiceEntry docgraph = PXGraph.CreateInstance<ARInvoiceEntry>();

			PXResultset<TermsInstallments> installments = TermsAttribute.SelectInstallments(this, terms, (DateTime)ardoc.DueDate);
			foreach (TermsInstallments inst in installments)
			{
				docgraph.customer.Current = customer;
				PXCache sender = ARInvoice_DocType_RefNbr.Cache;
				//force precision population
				object CuryOrigDocAmt = sender.GetValueExt(ardoc, "CuryOrigDocAmt");

				CurrencyInfo new_info = docgraph.GetExtension<ARInvoiceEntry.MultiCurrency>().CloneCurrencyInfo(info);

				ARInvoice new_ardoc = PXCache<ARInvoice>.CreateCopy(ardoc);
				new_ardoc.CuryInfoID = new_info.CuryInfoID;
				new_ardoc.DueDate = ((DateTime)new_ardoc.DueDate).AddDays((double)inst.InstDays);
				new_ardoc.DiscDate = new_ardoc.DueDate;
				new_ardoc.InstallmentNbr = inst.InstallmentNbr;
				new_ardoc.MasterRefNbr = new_ardoc.RefNbr;
				new_ardoc.RefNbr = null;
				new_ardoc.NoteID = null;
				new_ardoc.ProjectID = ProjectDefaultAttribute.NonProject();

				new_ardoc.CuryDetailExtPriceTotal = 0m;
				new_ardoc.DetailExtPriceTotal = 0m;
				new_ardoc.CuryLineDiscTotal = 0m;
				new_ardoc.LineDiscTotal = 0m;
				new_ardoc.CuryMiscExtPriceTotal = 0m;
				new_ardoc.MiscExtPriceTotal = 0m;
				new_ardoc.CuryGoodsExtPriceTotal = 0m;
				new_ardoc.GoodsExtPriceTotal = 0m;

				TaxAttribute.SetTaxCalc<ARTran.taxCategoryID>(docgraph.Transactions.Cache, null, TaxCalc.NoCalc);

				decimal? preCalculatedBaseAmt = null;

				if (inst.InstallmentNbr == installments.Count)
				{
					new_ardoc.CuryOrigDocAmt = new_ardoc.CuryOrigDocAmt - CuryTotalInstallments;
					preCalculatedBaseAmt = new_ardoc.OrigDocAmt - BaseTotalInstallments;
				}
				else
				{
					if (terms.InstallmentMthd == TermsInstallmentMethod.AllTaxInFirst)
					{
						new_ardoc.CuryOrigDocAmt = new_info
							.RoundCury((decimal)((ardoc.CuryOrigDocAmt - ardoc.CuryTaxTotal) * inst.InstPercent / 100m));
						if (inst.InstallmentNbr == 1)
						{
							new_ardoc.CuryOrigDocAmt += (decimal)ardoc.CuryTaxTotal;
						}
					}
					else
					{
						new_ardoc.CuryOrigDocAmt = new_info
							.RoundCury((decimal)(ardoc.CuryOrigDocAmt * inst.InstPercent / 100m));
					}
				}
				new_ardoc.CuryDocBal = new_ardoc.CuryOrigDocAmt;
				new_ardoc.CuryLineTotal = new_ardoc.CuryOrigDocAmt;
				new_ardoc.CuryTaxTotal = 0m;
				new_ardoc.CuryOrigDiscAmt = 0m;
				new_ardoc.CuryVatTaxableTotal = 0m;
				new_ardoc.CuryDiscTot = 0m;
				new_ardoc.OrigModule = BatchModule.AR;

				new_ardoc.Hold = true;
				new_ardoc = docgraph.Document.Insert(new_ardoc);

				docgraph.Approval.SuppressApproval = true;
				new_ardoc.Hold = false;

				if (preCalculatedBaseAmt.HasValue)
				{
					new_ardoc.OrigDocAmt = preCalculatedBaseAmt;
					new_ardoc.DocBal = preCalculatedBaseAmt;
					new_ardoc.LineTotal = preCalculatedBaseAmt;
				}

				docgraph.Document.Update(new_ardoc);


				CuryTotalInstallments += (decimal)new_ardoc.CuryOrigDocAmt;
				BaseTotalInstallments += new_ardoc.OrigDocAmt.Value;

				//Insert of ARInvoice causes the TaxZone to change thus setting the TaxCalc back to TaxCalc.Calc for the External (Avalara)
				//Set it back to NoCalc to avoid Document Totals recalculation on adding new transactions: 
				TaxAttribute.SetTaxCalc<ARTran.taxCategoryID>(docgraph.Transactions.Cache, null, TaxCalc.NoCalc);

				ARTran new_artran = docgraph.Transactions.Insert(new ARTran
				{
					AccountID = new_ardoc.ARAccountID,
					SubID = new_ardoc.ARSubID,
					CuryTranAmt = new_ardoc.CuryOrigDocAmt,
					TranDesc = LocalizeForCustomer(customer, Messages.MultiplyInstallmentsTranDesc)
				});

				foreach (ARSalesPerTran sptran in docgraph.salesPerTrans.Select())
				{
					docgraph.salesPerTrans.Delete(sptran);
				}

				foreach (ARSalesPerTran sptran in PXSelect<ARSalesPerTran, Where<ARSalesPerTran.docType, Equal<Required<ARSalesPerTran.docType>>, And<ARSalesPerTran.refNbr, Equal<Required<ARSalesPerTran.refNbr>>>>>.Select(this, ardoc.DocType, ardoc.RefNbr))
				{
					ARSalesPerTran new_sptran = PXCache<ARSalesPerTran>.CreateCopy(sptran);
					new_sptran.RefNbr = null;
					new_sptran.CuryInfoID = new_info.CuryInfoID;                    

					new_sptran.RefCntr = 999;
					new_sptran.CuryCommnblAmt = new_info
							.RoundCury((decimal)(sptran.CuryCommnblAmt * inst.InstPercent / 100m));
					new_sptran.CuryCommnAmt = new_info
							.RoundCury((decimal)(sptran.CuryCommnAmt * inst.InstPercent / 100m));
					new_sptran = docgraph.salesPerTrans.Insert(new_sptran);
				}

				if (preCalculatedBaseAmt.HasValue)
				{
					new_artran.TranAmt = preCalculatedBaseAmt;
					docgraph.Transactions.Update(new_artran);
					if (new_info.BaseCuryID != new_info.CuryID)
						new_info.BaseCalc = false;
				}

				docgraph.Save.Press();

				ret.Add(docgraph.Document.Current);

				docgraph.Clear();
			}

			if (installments.Count > 0)
			{
				PXDatabase.Update<ARInvoice>(
					new PXDataFieldAssign<ARInvoice.installmentCntr>(PXDbType.SmallInt, Convert.ToInt16(installments.Count)),
					new PXDataFieldRestrict<ARInvoice.docType>(PXDbType.VarChar, ardoc.DocType),
					new PXDataFieldRestrict<ARInvoice.refNbr>(PXDbType.NVarChar, ardoc.RefNbr));
			}

			return ret;
		}

		private void SetClosedPeriodsFromLatestApplication(ARRegister doc)
		{
			ARTranPost lastPeriod = PXSelect<ARTranPost,
					Where<ARTranPost.docType, Equal<Required<ARTranPost.docType>>,
						And<ARTranPost.refNbr, Equal<Required<ARTranPost.refNbr>>>>,
					OrderBy<Desc<ARTranPost.tranPeriodID,
						Desc<ARTranPost.iD>>>>
				.SelectSingleBound(this, new object[]{}, doc.DocType, doc.RefNbr);

			ARTranPost lastDate = PXSelect<ARTranPost,
					Where<ARTranPost.docType, Equal<Required<ARTranPost.docType>>,
						And<ARTranPost.refNbr, Equal<Required<ARTranPost.refNbr>>>>,
					OrderBy<Desc<ARTranPost.docDate,
						Desc<ARTranPost.iD>>>>
				.SelectSingleBound(this, new object[]{}, doc.DocType, doc.RefNbr);
			doc.ClosedTranPeriodID = GL.FinPeriods.FinPeriodUtils.Max(lastPeriod?.TranPeriodID , doc.TranPeriodID);
			FinPeriodIDAttribute.SetPeriodsByMaster<ARRegister.closedFinPeriodID>(
				ARDocument.Cache,
				doc,
				doc.ClosedTranPeriodID);
			
			doc.ClosedDate = GL.FinPeriods.FinPeriodUtils.Max(lastDate?.DocDate,doc.DocDate);
		}

		private void SetAdjgPeriodsFromLatestApplication(ARRegister doc, ARAdjust adj)
		{
			if (adj.VoidAppl == true)
			{
				// We should collect original applications to find max periods and dates,
				// because in some cases their values can be greater than values from voiding application
				//
				foreach (string adjgDocType in doc.PossibleOriginalDocumentTypes())
				{
					ARAdjust orig = PXSelect<ARAdjust,
						Where<ARAdjust.adjdDocType, Equal<Required<ARAdjust.adjdDocType>>,
							And<ARAdjust.adjdRefNbr, Equal<Required<ARAdjust.adjdRefNbr>>,
							And<ARAdjust.adjgDocType, Equal<Required<ARAdjust.adjgDocType>>,
							And<ARAdjust.adjgRefNbr, Equal<Required<ARAdjust.adjgRefNbr>>,
							And<ARAdjust.adjNbr, Equal<Required<ARAdjust.voidAdjNbr>>,
							And<ARAdjust.released, Equal<True>>>>>>>>
						.SelectSingleBound(this, null, adj.AdjdDocType, adj.AdjdRefNbr, adjgDocType, adj.AdjgRefNbr, adj.VoidAdjNbr);
					if (orig != null)
					{
					    FinPeriodIDAttribute.SetPeriodsByMaster<ARAdjust.adjgFinPeriodID>(
							ARAdjust_AdjgDocType_RefNbr_CustomerID.Cache,
							adj,
							GL.FinPeriods.FinPeriodUtils.Max(orig.AdjgTranPeriodID, adj.AdjgTranPeriodID));

						adj.AdjgDocDate = GL.FinPeriods.FinPeriodUtils.Max((DateTime)orig.AdjgDocDate, (DateTime)adj.AdjgDocDate);

						break;
					}
				}
			}

		    FinPeriodIDAttribute.SetPeriodsByMaster<ARAdjust.adjgFinPeriodID>(
							ARAdjust_AdjgDocType_RefNbr_CustomerID.Cache,
							adj,
							GL.FinPeriods.FinPeriodUtils.Max(adj.AdjdTranPeriodID, adj.AdjgTranPeriodID));

			adj.AdjgDocDate = GL.FinPeriods.FinPeriodUtils.Max((DateTime)adj.AdjdDocDate, (DateTime)adj.AdjgDocDate);
		}

		private void CreatePayment(PXResult<ARInvoice, CurrencyInfo, Terms, Customer> res, ref List<ARRegister> ret)
		{
			ret = ret ?? new List<ARRegister>();

			Lazy<ARPaymentEntry> lazyPaymentEntry = new Lazy<ARPaymentEntry>(() =>
			{
				ARPaymentEntry result = CreateInstance<ARPaymentEntry>();

				result.AutoPaymentApp = true;
				result.arsetup.Current.HoldEntry = false;
				result.arsetup.Current.RequireControlTotal = false;

				return result;
			});

			ARInvoice invoice = PXCache<ARInvoice>.CreateCopy(res);

				//Skip this alltogether?
				PXResultset<SOInvoice> invoicesCSL = PXSelectJoin<SOInvoice,
				InnerJoin<CustomerPaymentMethod, On<CustomerPaymentMethod.pMInstanceID, Equal<SOInvoice.pMInstanceID>>,
					InnerJoin<PaymentMethod, On<PaymentMethod.paymentMethodID, Equal<CustomerPaymentMethod.paymentMethodID>>,
						LeftJoin<ARPayment, On<ARPayment.docType, Equal<SOInvoice.docType>,
							And<ARPayment.refNbr, Equal<SOInvoice.refNbr>>>>>>,
				Where<SOInvoice.docType, Equal<Required<SOInvoice.docType>>,
					And<SOInvoice.refNbr, Equal<Required<SOInvoice.refNbr>>,
						And<PaymentMethod.paymentType, Equal<PaymentMethodType.creditCard>,
							And<ARPayment.refNbr, IsNotNull>>>>>.Select(this, invoice.DocType, invoice.RefNbr);

				foreach (PXResult<SOInvoice, CustomerPaymentMethod, PaymentMethod, ARPayment> csls in invoicesCSL)
				{
					SOInvoice currInvoice = csls;
					ARPayment currPayment = csls;

					using (PXTransactionScope ts = new PXTransactionScope())
					{
						SetPaymentReferenceOnCCTran(this, currInvoice, currPayment);

						ts.Complete();
					}
				}
		}

		public static void SetPaymentReferenceOnCCTran(PXGraph graph, SOInvoice soinvoice, ARPayment payment)
		{
			PXDatabase.Update<ExternalTransaction>(
				new PXDataFieldAssign("DocType", payment.DocType),
				new PXDataFieldAssign("RefNbr", payment.RefNbr),
				new PXDataFieldRestrict("DocType", PXDbType.Char, 3, soinvoice.DocType, PXComp.EQ),
				new PXDataFieldRestrict("RefNbr", PXDbType.NVarChar, 15, soinvoice.RefNbr, PXComp.EQ)
			);
			PXDatabase.Update<CCProcTran>(
				new PXDataFieldAssign("DocType", payment.DocType),
				new PXDataFieldAssign("RefNbr", payment.RefNbr),
				new PXDataFieldRestrict("DocType", PXDbType.Char, 3, soinvoice.DocType, PXComp.EQ),
				new PXDataFieldRestrict("RefNbr", PXDbType.NVarChar, 15, soinvoice.RefNbr, PXComp.EQ)
				);

			bool ccproctranupdated = false;
			string firstOrderType = null;
			string firstOrderNbr = null;

			foreach (SOOrderShipment order in PXSelect<SOOrderShipment,
				Where<SOOrderShipment.invoiceType, Equal<Required<SOOrderShipment.invoiceType>>,
				And<SOOrderShipment.invoiceNbr, Equal<Required<SOOrderShipment.invoiceNbr>>>>>.Select(graph, soinvoice.DocType, soinvoice.RefNbr))
			{
				if (firstOrderNbr == null)
				{
					firstOrderType = order.OrderType;
					firstOrderNbr = order.OrderNbr;
				}

				PXDatabase.Update<ExternalTransaction>(
					new PXDataFieldAssign("DocType", payment.DocType),
					new PXDataFieldAssign("RefNbr", payment.RefNbr),
					new PXDataFieldRestrict("OrigDocType", PXDbType.Char, 3, order.OrderType, PXComp.EQ),
					new PXDataFieldRestrict("OrigRefNbr", PXDbType.NVarChar, 15, order.OrderNbr, PXComp.EQ),
					new PXDataFieldRestrict("RefNbr", PXDbType.NVarChar, 15, null, PXComp.ISNULL)
				);
				ccproctranupdated |= PXDatabase.Update<CCProcTran>(
					new PXDataFieldAssign("DocType", payment.DocType),
					new PXDataFieldAssign("RefNbr", payment.RefNbr),
					new PXDataFieldRestrict("OrigDocType", PXDbType.Char, 3, order.OrderType, PXComp.EQ),
					new PXDataFieldRestrict("OrigRefNbr", PXDbType.NVarChar, 15, order.OrderNbr, PXComp.EQ),
					new PXDataFieldRestrict("RefNbr", PXDbType.NVarChar, 15, null, PXComp.ISNULL)
					);

				if (ccproctranupdated && (firstOrderType != order.OrderType || firstOrderNbr != order.OrderNbr))
				{
					throw new PXException(AR.Messages.ERR_CCMultiplyPreauthCombined);
				}
			}
		}

		public static void AdjustTaxCalculationLevelForNetGrossEntryMode(ARRegister document, ARTran documentLine, ref Tax taxCorrespondingToLine)
		{
			if (PXAccess.FeatureInstalled<FeaturesSet.netGrossEntryMode>())
			{
				string documentTaxCalculationMode = document.TaxCalcMode;

				switch (documentTaxCalculationMode)
				{
					case TaxCalculationMode.Gross:
						taxCorrespondingToLine.TaxCalcLevel = CSTaxCalcLevel.Inclusive;
						break;
					case TaxCalculationMode.Net:
						taxCorrespondingToLine.TaxCalcLevel = CSTaxCalcLevel.CalcOnItemAmt;
						break;
					case TaxCalculationMode.TaxSetting:
					default:
						break;
				}
			}
		}

		private void UpdateARBalancesDates(ARRegister ardoc)
		{
			ARBalances arbal = (ARBalances)Caches[typeof(ARBalances)].Insert(new ARBalances
			{
				BranchID = ardoc.BranchID,
				CustomerID = ardoc.CustomerID,
				CustomerLocationID = ardoc.CustomerLocationID
			});

			_oldInvoiceRefresher.RecordDocument(ardoc.BranchID, ardoc.CustomerID, ardoc.CustomerLocationID);
		}

		public static decimal? RoundAmount(decimal? amount, string RoundType, decimal? precision)
		{
			decimal? toround = amount / precision;

			switch (RoundType)
			{
				case RoundingType.Floor:
					return Math.Floor((decimal)toround) * precision;
				case RoundingType.Ceil:
					return Math.Ceiling((decimal)toround) * precision;
				case RoundingType.Mathematical:
					return Math.Round((decimal)toround, 0, MidpointRounding.AwayFromZero) * precision;
				default:
					return amount;
			}
		}

		protected virtual decimal? RoundAmount(decimal? amount)
		{
			return RoundAmount(amount, this.InvoiceRounding, this.InvoicePrecision);
		}

		/// <summary>
		/// The method to create a self document application (the same adjusted and adjusting documents)
		/// with amount equal to <see cref="ARRegister.CuryOrigDocAmt"/> value.
		/// </summary>
		public virtual ARAdjust CreateSelfApplicationForDocument(ARRegister doc)
		{
			ARAdjust adj = new ARAdjust();

			adj.AdjgDocType = doc.DocType;
			adj.AdjgRefNbr = doc.RefNbr;
			adj.AdjdDocType = doc.DocType;
			adj.AdjdRefNbr = doc.RefNbr;
			adj.AdjNbr = doc.AdjCntr;

			adj.AdjgBranchID = doc.BranchID;
			adj.AdjdBranchID = doc.BranchID;
			adj.CustomerID = doc.CustomerID;
			adj.AdjdCustomerID = doc.CustomerID;
			adj.AdjdARAcct = doc.ARAccountID;
			adj.AdjdARSub = doc.ARSubID;
			adj.AdjgCuryInfoID = doc.CuryInfoID;
			adj.AdjdCuryInfoID = doc.CuryInfoID;
			adj.AdjdOrigCuryInfoID = doc.CuryInfoID;

			adj.AdjgDocDate = doc.DocDate;
			adj.AdjdDocDate = doc.DocDate;
			adj.AdjgFinPeriodID = doc.FinPeriodID;
			adj.AdjdFinPeriodID = doc.FinPeriodID;
			adj.AdjgTranPeriodID = doc.TranPeriodID;
			adj.AdjdTranPeriodID = doc.TranPeriodID;

			adj.CuryAdjgAmt = doc.CuryOrigDocAmt;
			adj.CuryAdjdAmt = doc.CuryOrigDocAmt;
			adj.AdjAmt = doc.OrigDocAmt;

			adj.RGOLAmt = 0m;
			adj.CuryAdjgDiscAmt = doc.CuryOrigDiscAmt;
			adj.CuryAdjdDiscAmt = doc.CuryOrigDiscAmt;
			adj.AdjDiscAmt = doc.OrigDiscAmt;

			adj.Released = false;

			adj.InvoiceID = doc.NoteID;
			adj.PaymentID = doc.DocType != ARDocType.CreditMemo ? doc.NoteID : null;
			adj.MemoID = doc.DocType == ARDocType.CreditMemo ? doc.NoteID : null;

			adj = (ARAdjust)ARAdjust_AdjgDocType_RefNbr_CustomerID.Cache.Insert(adj);

			return adj;
		}

		/// <summary>
		/// The method to process migrated document. A special self application with amount equal to 
		/// difference between <see cref="ARRegister.CuryOrigDocAmt"/> and <see cref="ARRegister.CuryInitDocBal"/> 
		/// will be created for the document. Note, that all logic around <see cref="ARBalances"/>, <see cref="ARHistory"/> and 
		/// document balances is implemented inside this method, so we don't need to update any balances somewhere else.
		/// This is the reason why all special applications should be excluded from the adjustments processing.
		/// </summary>
		protected virtual void ProcessMigratedDocument(
			JournalEntry je, 
			GLTran tran, 
			ARRegister doc, 
			bool isDebit, 
			Customer customer, 
			CurrencyInfo currencyinfo)
		{
			// Create special application to update balances with proper amounts
			//
			ARAdjust initAdj = CreateSelfApplicationForDocument(doc);

			initAdj.RGOLAmt = 0m;
			initAdj.CuryAdjgDiscAmt = 0m;
			initAdj.CuryAdjdDiscAmt = 0m;
			initAdj.AdjDiscAmt = 0m;

			initAdj.CuryAdjgAmt -= doc.CuryInitDocBal;
			initAdj.CuryAdjdAmt -= doc.CuryInitDocBal;
			initAdj.AdjAmt -= doc.InitDocBal;

			initAdj.Released = true;
			initAdj.IsInitialApplication = true;
			initAdj = (ARAdjust)ARAdjust_AdjgDocType_RefNbr_CustomerID.Cache.Update(initAdj);

			UpdateARBalances(this, doc, -1 * initAdj.AdjAmt * doc.SignBalance, 0m);

			// We don't need to update balances for VoidPayment document,
			// because it will be closed anyway further in the code
			//
			if (initAdj.VoidAppl != true)
			{
				UpdateBalances(initAdj, doc);
				VerifyAdjustedDocumentAndClose(doc);
			}

			// Create special GL transaction to update history with proper bucket
			// 
			GLTran initTran = (GLTran)je.GLTranModuleBatNbr.Cache.CreateCopy(tran);

			initTran.TranClass = GLTran.tranClass.Normal;
			initTran.TranType = ARDocType.CreditMemo;
			initTran.DebitAmt = isDebit ? initAdj.AdjAmt : 0m;
			initTran.CuryDebitAmt = isDebit ? initAdj.CuryAdjgAmt : 0m;
			initTran.CreditAmt = isDebit ? 0m : initAdj.AdjAmt;
			initTran.CuryCreditAmt = isDebit ? 0m : initAdj.CuryAdjgAmt;

			UpdateHistory(initTran, customer);
			UpdateHistory(initTran, customer, currencyinfo);
			ProcessAdjustmentTranPost(initAdj, doc, doc, true);

			// All deposits should be moved to the Payments bucket,
			// to prevent amounts stack on the Deposits bucket.
			// 
			ARHistBucket origBucket = new ARHistBucket(tran, GetHistTranType(tran.TranType, tran.RefNbr));

			if (origBucket.SignDeposits != 0m)
			{
				ARHistBucket initBucket = new ARHistBucket();
				decimal sign = origBucket.SignDeposits;

				initBucket.arAccountID = tran.AccountID;
				initBucket.arSubID = tran.SubID;
				initBucket.SignDeposits = sign;
				initBucket.SignPayments = -sign;
				initBucket.SignPtd = sign;

				UpdateHistory(initTran, customer, initBucket);
				UpdateHistory(initTran, customer, currencyinfo, initBucket);
			}
		}

		/// <summary>
		/// The method to release invoices.
		/// The maintenance screen is "Invoice and Memos" (AR301000).
		/// </summary>
		public virtual List<ARRegister> ReleaseInvoice(
			JournalEntry je, 
			ARRegister doc, 
			PXResult<ARInvoice, CurrencyInfo, Terms, Customer, Account> res, 
			List<PMRegister> pmDocs)
		{
			ARInvoice ardoc = res;
			CurrencyInfo info = res;
			Terms terms = res;
			Customer customer = res;
			Account arAccount = res;
			bool IsMasterInstallment = ardoc.InstallmentCntr != null;

			ARInvoice_DocType_RefNbr.Current = ardoc;
			ARDocument.Cache.Current = doc;

			if (info.BaseCuryID != info.CuryID && ardoc.InstallmentNbr != null)
				info.BaseCalc = false;		

			GetExtension<MultiCurrency>().StoreResult(info);

			List<ARRegister> ret = new List<ARRegister>();

			if (doc.Released != true)
			{
				if (_IsIntegrityCheck == false && doc.DocType != ARDocType.SmallCreditWO)
				{
					if ((bool)arsetup.PrintBeforeRelease && ardoc.Printed != true && ardoc.DontPrint != true)
						throw new PXException(Messages.Invoice_NotPrinted_CannotRelease);				
					if ((bool)arsetup.EmailBeforeRelease && ardoc.Emailed != true && ardoc.DontEmail != true)
						throw new PXException(Messages.Invoice_NotEmailed_CannotRelease);
				}

				if (ardoc.PaymentsByLinesAllowed == true && ardoc.TaxZoneID != null)
				{
					TaxZone docTaxZone = SelectFrom<TaxZone>
						.Where<TaxZone.taxZoneID.IsEqual<@P.AsString>>
						.View.Select(this, ardoc.TaxZoneID);
					if (docTaxZone.IsExternal == true)
					{
						throw new PXException(AP.Messages.PaymentApplicationByLineNotCompatibleWithExternalTaxProvider);
					}
				}

				if (ardoc.CreditHold == true)
				{
					throw new PXException(
						Messages.InvoiceCreditHoldCannotRelease,
						GetLabel.For<ARDocType>(ardoc.DocType),
						ardoc.RefNbr);
				}

				string _InstallmentType = terms.InstallmentType;

				if (_IsIntegrityCheck && ardoc.InstallmentNbr == null)
				{
					_InstallmentType = ardoc.InstallmentCntr != null
						? _InstallmentType = TermsInstallmentType.Multiple
						: _InstallmentType = TermsInstallmentType.Single;
				}

				if (_InstallmentType == TermsInstallmentType.Multiple && (ardoc.DocType == ARDocType.CashSale || ardoc.DocType == ARDocType.CashReturn))
				{
					throw new PXException(Messages.Cash_Sale_Cannot_Have_Multiply_Installments);
				}

				if (_InstallmentType == TermsInstallmentType.Multiple && ardoc.InstallmentNbr == null)
				{
					if (_IsIntegrityCheck == false)
					{
						ret = CreateInstallments(res);
					}
					IsMasterInstallment = true;
					doc.CuryDocBal = 0m;
					doc.DocBal = 0m;
					doc.CuryDiscBal = 0m;
					doc.DiscBal = 0m;
					doc.CuryDiscTaken = 0m;
					doc.DiscTaken = 0m;

					doc.OpenDoc = false;
					doc.ClosedDate = doc.DocDate;
					doc.ClosedFinPeriodID = doc.FinPeriodID;
					doc.ClosedTranPeriodID = doc.TranPeriodID;
					RaiseInvoiceEvent(doc, ARInvoice.Events.Select(ev => ev.CloseDocument));
					UpdateARBalances(this, doc, -1m * doc.OrigDocAmt, 0m);
				}
				else
				{
					doc.CuryDocBal = doc.CuryOrigDocAmt;
					doc.DocBal = doc.OrigDocAmt;
					doc.CuryRetainageUnreleasedAmt = doc.CuryRetainageTotal;
					doc.RetainageUnreleasedAmt = doc.RetainageTotal;
					doc.CuryDiscBal = doc.CuryOrigDiscAmt;
					doc.DiscBal = doc.OrigDiscAmt;
					doc.CuryDiscTaken = 0m;
					doc.DiscTaken = 0m;
					doc.RGOLAmt = 0m;

					doc.OpenDoc = true;
					doc.ClosedDate = null;
					doc.ClosedFinPeriodID = null;
					doc.ClosedTranPeriodID = null;
					RaiseInvoiceEvent(doc, ARInvoice.Events.Select(ev => ev.OpenDocument));
					UpdateARBalancesDates(ardoc);
				}

				//should always restore ARRegister to ARInvoice after above assignments
				PXCache<ARRegister>.RestoreCopy(ardoc, doc);

				CurrencyInfo new_info = CurrencyInfo.GetEX(GetCurrencyInfoCopyForGL(je, info, false));

				bool isDebit = (ardoc.DrCr == DrCr.Debit);
				bool isCashSaleOrCashreturnDocument =
					ardoc.DocType == ARDocType.CashSale ||
					ardoc.DocType == ARDocType.CashReturn;

				if (isCashSaleOrCashreturnDocument)
				{
					CheckOpenForReviewTrans(doc);
					GLTran tran = new GLTran();
					tran.SummPost = true;
					tran.ZeroPost = false;
					tran.BranchID = ardoc.BranchID;
					tran.AccountID = ardoc.ARAccountID;
					tran.ReclassificationProhibited = true;
					tran.SubID = ardoc.ARSubID;
					tran.CuryDebitAmt = isDebit ? 0m : ardoc.CuryOrigDocAmt + ardoc.CuryOrigDiscAmt;
					tran.DebitAmt = isDebit ? 0m : ardoc.OrigDocAmt + ardoc.OrigDiscAmt;
					tran.CuryCreditAmt = isDebit ? ardoc.CuryOrigDocAmt + ardoc.CuryOrigDiscAmt : 0m;
					tran.CreditAmt = isDebit ? ardoc.OrigDocAmt + ardoc.OrigDiscAmt : 0m;
					tran.TranType = ardoc.DocType;
					tran.TranClass = GLTran.tranClass.Normal;
					tran.RefNbr = ardoc.RefNbr;
					tran.TranDesc = ardoc.DocDesc;
					FinPeriodIDAttribute.SetPeriodsByMaster<GLTran.finPeriodID>(je.GLTranModuleBatNbr.Cache, tran, ardoc.TranPeriodID);
					tran.TranDate = ardoc.DocDate;
					tran.CuryInfoID = new_info.CuryInfoID;
					tran.Released = true;
					tran.ReferenceID = ardoc.CustomerID;

					SetProjectAndTaxID(tran, arAccount, ardoc);

					//no history update should take place
					InsertInvoiceTransaction(je, tran, 
						new GLTranInsertionContext { ARRegisterRecord = doc });
				}
				else if (ardoc.DocType != ARDocType.SmallCreditWO)
				{
					string tranClass = string.Empty;

					if (ardoc.IsRetainageDocument == true)
						tranClass = GLTran.tranClass.RetainageReleased;
					else if (ardoc.IsPrepaymentInvoiceDocument() || ardoc.IsPrepaymentInvoiceDocumentReverse())
						tranClass = GLTran.tranClass.PrepaymentInvoice;
					else
						tranClass = GLTran.tranClass.Normal;

					GLTran tran = new GLTran();
					tran.SummPost = true;
					tran.BranchID = ardoc.BranchID;
					tran.AccountID = ardoc.ARAccountID;
					tran.ReclassificationProhibited = true;
					tran.SubID = ardoc.ARSubID;
					tran.CuryDebitAmt = isDebit ? 0m : ardoc.CuryOrigDocAmt;
					tran.DebitAmt = isDebit ? 0m : ardoc.OrigDocAmt - ardoc.RGOLAmt;
					tran.CuryCreditAmt = isDebit ? ardoc.CuryOrigDocAmt : 0m;
					tran.CreditAmt = isDebit ? ardoc.OrigDocAmt - ardoc.RGOLAmt : 0m;
					tran.TranType = ardoc.DocType;
					tran.TranClass = tranClass;
					tran.RefNbr = ardoc.RefNbr;
					tran.TranDesc = ardoc.DocDesc;
				    FinPeriodIDAttribute.SetPeriodsByMaster<GLTran.finPeriodID>(je.GLTranModuleBatNbr.Cache, tran, ardoc.TranPeriodID);
					tran.TranDate = ardoc.DocDate;
					tran.CuryInfoID = new_info.CuryInfoID;
					tran.Released = true;
					tran.ReferenceID = ardoc.CustomerID;

					SetProjectAndTaxID(tran, arAccount, ardoc);
					
					if (doc.OpenDoc == true)
					{
						UpdateHistory(tran, customer);
						UpdateHistory(tran, customer, info);
					}

					InsertInvoiceTransaction(je, tran, 
						new GLTranInsertionContext { ARRegisterRecord = doc });

					if (IsMigratedDocumentForProcessing(doc))
					{
						ProcessMigratedDocument(je, tran, doc, isDebit, customer, info);
						doc = (ARRegister)ARDocument.Cache.Locate(doc) ?? doc;
						PXCache<ARRegister>.RestoreCopy(ardoc, doc);
					}

					#region Retainage part

					if (ardoc.IsOriginalRetainageDocument())
					{
						GLTran retainageTran = (GLTran)je.GLTranModuleBatNbr.Cache.CreateCopy(tran);

						retainageTran.ReclassificationProhibited = true;
						retainageTran.AccountID = ardoc.RetainageAcctID;
						retainageTran.SubID = ardoc.RetainageSubID;

						retainageTran.CuryDebitAmt = isDebit ? 0m : ardoc.CuryRetainageTotal;
						retainageTran.DebitAmt = isDebit ? 0m : ardoc.RetainageTotal;
						retainageTran.CuryCreditAmt = isDebit ? ardoc.CuryRetainageTotal : 0m;
						retainageTran.CreditAmt = isDebit ? ardoc.RetainageTotal : 0m;

						retainageTran.OrigAccountID = tran.AccountID;
						retainageTran.OrigSubID = tran.SubID;

						retainageTran.TranClass = GLTran.tranClass.RetainageWithheld;

						UpdateHistory(retainageTran, customer);
						UpdateHistory(retainageTran, customer, info);

						je.GLTranModuleBatNbr.Insert(retainageTran);

						if (ardoc.IsRetainageReversing == true)
						{
							// We should clear unreleased retainage amount
							// for the original retainage invoice
							ClearRetainageAmount(doc);
						}
					}

					if (ardoc.IsRetainageDocument == true)
					{
						ReleaseRetainageAmount(doc);
					}
					#endregion
				}

				IEqualityComparer<ARTaxTran> arTaxTranComparer =
					new FieldSubsetEqualityComparer<ARTaxTran>(
					Caches[typeof(ARTaxTran)],
					typeof(ARTaxTran.recordID));

				bool isPayByLineRetainageReversing =
					doc.PaymentsByLinesAllowed != true &&
					(doc.IsRetainageDocument == true || doc.RetainageApply == true) &&
					doc.IsRetainageReversing == true &&
					GetOriginalRetainageDocument(doc)?.PaymentsByLinesAllowed == true;
				bool calculateLineBalances = doc.PaymentsByLinesAllowed == true || isPayByLineRetainageReversing;

				var processedDiscountsLineNbrs = new List<int?>();

				Amount itemDiscount = new Amount();
				foreach (var group in ARTran_TranType_RefNbr
					.Select(ardoc.DocType, ardoc.RefNbr)
					.AsEnumerable()
					.Cast<PXResult<ARTran, ARTax, Tax, DRDeferredCode, SOOrderType, ARTaxTran>>()
					.GroupBy(row => (ARTaxTran)row, arTaxTranComparer))
				{
					ARTaxTran arTaxTran = group.Key;
					List<ARTax> arTaxes = new List<ARTax>();
					Tax prev_tax = null;

					foreach (PXResult<ARTran, ARTax, Tax, DRDeferredCode, SO.SOOrderType, ARTaxTran> row in group)
				{
						ARTran tran = row;
						ARTax artax = row;
						prev_tax = row;
						SOOrderType sotype = row;

						tran.TranDate = ardoc.DocDate;
						PXParentAttribute.SetParent(ARTran_TranType_RefNbr.Cache,tran, typeof(ARRegister), ardoc);
						ARTran_TranType_RefNbr.Cache.SetDefaultExt<ARTran.finPeriodID>(tran);
						FinPeriodIDAttribute.SetMasterPeriodID<ARTran.finPeriodID>(ARTran_TranType_RefNbr.Cache, tran);

						if (_IsIntegrityCheck == false && !processedDiscountsLineNbrs.Contains(tran.LineNbr))
																													{ 
							ProcessInvoiceDetailDiscount(doc, customer, tran, sotype, ardoc);
							processedDiscountsLineNbrs.Add(tran.LineNbr);
;						}

						if (artax.TranType != null && artax.RefNbr != null && artax.LineNbr != null)
						{
							arTaxes.Add(artax);
						}
				}

					if (arTaxes.Count() > 0 && calculateLineBalances)
					{
						ARTaxAttribute arTaxAttr = new ARTaxAttribute(typeof(ARRegister), typeof(ARTax), typeof(ARTaxTran));
						arTaxAttr.DistributeTaxDiscrepancy<ARTax, ARTax.curyTaxAmt, ARTax.taxAmt>(this, arTaxes, arTaxTran.CuryTaxAmt.Value, true);

						if (arTaxTran.CuryRetainedTaxAmt != 0m)
						{
							ARRetainedTaxAttribute apRetainedTaxAttr = new ARRetainedTaxAttribute(typeof(ARRegister), typeof(ARTax), typeof(ARTaxTran));
							apRetainedTaxAttr.DistributeTaxDiscrepancy<ARTax, ARTax.curyRetainedTaxAmt, ARTax.retainedTaxAmt>(this, arTaxes, arTaxTran.CuryRetainedTaxAmt.Value, true);
						}
					}
				}

				List<PXResult<ARTran>> docLinesWithData = ARTran_TranType_RefNbr.Select(ardoc.DocType, ardoc.RefNbr).ToList();
				FinPeriodUtils.AllowPostToUnlockedPeriodAnyway = _IsIntegrityCheck;
				if(_IsIntegrityCheck != true)
					FinPeriodUtils.ValidateFinPeriod(docLinesWithData.Select(line => (ARTran)line), typeof(OrganizationFinPeriod.aRClosed));
				FinPeriodUtils.AllowPostToUnlockedPeriodAnyway = false;

				if (!_IsIntegrityCheck)
				{
					foreach (ARTran arTran in docLinesWithData)
					{
						if (arTran.Released != true)
							ConvertedInventoryItemAttribute.ValidateRow(ARTran_TranType_RefNbr.Cache, arTran);
					}
				}

				bool singleScheduleMode = PXAccess.FeatureInstalled<FeaturesSet.aSC606>();
				Amount postDeferredTotalAmt = new Amount(0m, 0m);
				decimal deferredNetDiscountRate = 0m;
				bool isSuspense = false;

				if (singleScheduleMode)
				{
					int? defScheduleID;
					postDeferredTotalAmt = ASC606Helper.CalculateNetAmount(this, ardoc, out deferredNetDiscountRate, out defScheduleID);

					if (_IsIntegrityCheck == false && postDeferredTotalAmt.Cury != 0m)
					{
						dr.Clear();

						try
						{
						(dr as DRSingleProcess).CreateSingleSchedule(ardoc, postDeferredTotalAmt, defScheduleID, isDraft: false);
						dr.Actions.PressSave();
					}
						catch(ScheduleCuryTotalAmtLessOrEqualZeroException)
						{
							isSuspense = true;
							doc.DRSchedCntr = 0;

							if (dr.Setup.Current.SuspenseAccountID == null)
								throw new PXException(DR.Messages.DRSetupSuspenseAccountIsEmpty);
							if (dr.Setup.Current.SuspenseSubID == null)
								throw new PXException(DR.Messages.DRSetupSuspenseSubAccountIsEmpty);
						}
					}
				}

				LineBalances validateBalances = new LineBalances(0m);
				ARTran maxRetainageTran = null;
				ARTran maxBalanceTran = null;

				IEqualityComparer<ARTran> arTranComparer =
					new FieldSubsetEqualityComparer<ARTran>(
					ARTran_TranType_RefNbr.Cache,
					typeof(ARTran.tranType),
					typeof(ARTran.refNbr),
					typeof(ARTran.lineNbr));

				foreach (var group in docLinesWithData.AsEnumerable().GroupBy(row => (ARTran)row, arTranComparer))
				{
					ARTran n = group.Key;
					PXCache<ARTran>.StoreOriginal(this, n);
					if (!_IsIntegrityCheck)
					{
						n.ClearInvoiceDetailsBalance();
					}

					if (_IsIntegrityCheck == false && n.Released == true)
					{
						throw new PXException(Messages.Document_Status_Invalid);
					}

					if (singleScheduleMode && n.InventoryID == null && n.DeferredCode != null)
					{
						throw new PXException(Messages.InventoryIDCouldNotBeEmpty);
					}

					if (singleScheduleMode && n.LineType == SOLineType.Discount && deferredNetDiscountRate == 1m)
					{
						continue;
					}

					bool isFirstARTaxRow = true;
					bool isMultipleInstallmentInvoice = _InstallmentType == TermsInstallmentType.Multiple && ardoc.InstallmentNbr == null;

					foreach (PXResult<ARTran, ARTax, Tax, DRDeferredCode, SO.SOOrderType, ARTaxTran> r in group)
					{
						ARTax x = r;
						Tax salestax = r;
						DRDeferredCode defcode = r;

						if (x != null && x.TaxID != null)
						{
							AdjustTaxCalculationLevelForNetGrossEntryMode(ardoc, n, ref salestax);
						}

						if (isFirstARTaxRow)
					{
							var resetProject = ardoc.IsRetainageDocument == true;

							int? accountID = null;
							int? subID = null;
							string tranClass = string.Empty;
							if (ardoc.IsPrepaymentInvoiceDocument() || ardoc.IsPrepaymentInvoiceDocumentReverse())
							{
								accountID = ardoc.PrepaymentAccountID;
								subID = ardoc.PrepaymentSubID;
								tranClass = GLTran.tranClass.PrepaymentInvoice;
							}
							else
							{
								accountID = isSuspense == true ? dr.Setup.Current.SuspenseAccountID : n.AccountID;
								subID = isSuspense == true ? dr.Setup.Current.SuspenseSubID : GetValueInt<ARTran.subID>(je, n);
								tranClass = ardoc.DocClass;
							}

							GLTran tran = new GLTran
							{
								ReclassificationProhibited = ardoc.IsRetainageDocument == true,
								SummPost = this.SummPost && n.TaskID == null && n.PMDeltaOption != ARTran.pMDeltaOption.BillLater,
								BranchID = n.BranchID,
								CuryInfoID = new_info.CuryInfoID,
								TranType = n.TranType,
								TranClass = tranClass,
								RefNbr = n.RefNbr,
								InventoryID = n.InventoryID,
								UOM = n.UOM,
								Qty = (n.DrCr == DrCr.Credit) ? n.Qty : -1 * n.Qty,
								TranDate = n.TranDate,
								ProjectID = resetProject ? ProjectDefaultAttribute.NonProject() : n.ProjectID,
								TaskID = resetProject ? null : n.TaskID,
								CostCodeID = resetProject ? null : n.CostCodeID,
								AccountID = accountID,
								SubID = subID,
								TranDesc = n.TranDesc,
								Released = true,
								ReferenceID = ardoc.CustomerID
							};
						tran.TranLineNbr = (tran.SummPost == true) ? null : n.LineNbr;

						Amount postedAmount = GetSalesPostingAmount(this, doc, n, x, salestax,
							amount => CM.PXDBCurrencyAttribute.Round(je.GLTranModuleBatNbr.Cache, tran, amount, CM.CMPrecision.TRANCURY),
							amount => CM.PXDBCurrencyAttribute.Round(je.GLTranModuleBatNbr.Cache, tran, amount, CM.CMPrecision.BASECURY));
						if (singleScheduleMode && n.LineType == SOLineType.Discount)
						{
							var discRemainderRate = 1m - deferredNetDiscountRate;
								postedAmount = new Amount(
									CM.PXDBCurrencyAttribute.Round(je.GLTranModuleBatNbr.Cache, tran, (postedAmount.Cury * discRemainderRate).Value, CM.CMPrecision.TRANCURY),
									CM.PXDBCurrencyAttribute.Round(je.GLTranModuleBatNbr.Cache, tran, (postedAmount.Base * discRemainderRate).Value, CM.CMPrecision.TRANCURY)
									);
						}

						tran.CuryDebitAmt = (n.DrCr == DrCr.Debit) ? postedAmount.Cury : 0m;
						tran.DebitAmt = (n.DrCr == DrCr.Debit) ? postedAmount.Base : 0m;
						tran.CuryCreditAmt = (n.DrCr == DrCr.Debit) ? 0m : postedAmount.Cury;
						tran.CreditAmt = (n.DrCr == DrCr.Debit) ? 0m : postedAmount.Base;

						bool? isStockItem = n.IsStockItem;

						if (n.OrigLineNbr != null)
						{
							InventoryItem item = InventoryItem.PK.Find(this, n.InventoryID);
							if (item?.IsConverted == true)
							{
								var origLine = ARTran.PK.Find(this, doc.OrigDocType, doc.OrigRefNbr, n.OrigLineNbr);
								isStockItem = origLine?.IsStockItem ?? isStockItem;
								n.IsStockItem = isStockItem;
							}
						}

						if (isStockItem != true && n.AccrueCost == true)
							CreateExpenseAccrualTransactions(je, doc, n, tran);

						ReleaseInvoiceTransactionPostProcessing(je, ardoc, r, tran);

						if (_IsIntegrityCheck == false)
						{
							IEnumerable<GLTran> transactions = new List<GLTran>();

							if (defcode?.DeferredCodeID != null && isSuspense == false)
							{
								if (singleScheduleMode == false)
								{
									dr.CreateSchedule(n, defcode, ardoc, postedAmount.Base.Value, isDraft: false);
									dr.Actions.PressSave();

									transactions = je.CreateTransBySchedule(dr, tran);

									je.CorrectCuryAmountsDueToRounding(transactions, tran, postedAmount.Cury.Value);
								}
								else if (dr.Schedule.Current != null)
								{
									transactions = je.CreateTransBySchedule(dr, n, tran);
								}

								foreach (var generated in transactions)
								{
									InsertInvoiceDetailsScheduleTransaction(je, generated, 
										new GLTranInsertionContext { ARRegisterRecord = doc, ARTranRecord = n });
								}
							}

							if (transactions?.Any() != true)
							{
								tran = InsertInvoiceDetailsTransaction(je, tran, 
									new GLTranInsertionContext { ARRegisterRecord = doc, ARTranRecord = n });
								if (ardoc.IsPrepaymentInvoiceDocument())
								{
									UpdateHistory(tran, customer);
									UpdateHistory(tran, customer, new_info);
								}
							}
						}

						UpdateItemDiscountsHistory(n, ardoc);
						itemDiscount += new Amount(n.CuryDiscAmt ?? 0m, n.DiscAmt ?? 0m);

						ReleaseInvoiceTransactionPostProcessed(je, ardoc, n);

						n.Released = true;
							isFirstARTaxRow = false;
						}

						if (!_IsIntegrityCheck &&
							calculateLineBalances &&
							n.LineType != SO.SOLineType.Discount &&
							!isMultipleInstallmentInvoice)
						{
							var tmp = AdjustInvoiceDetailsBalanceByTax(doc, n, x, salestax);
							validateBalances += tmp;
						}
					}

					if (calculateLineBalances &&
						n.LineType != SO.SOLineType.Discount &&
						!isMultipleInstallmentInvoice)
					{
						if (!_IsIntegrityCheck)
						{
							var tmp = AdjustInvoiceDetailsBalanceByLine(doc, n);
							validateBalances += tmp;
						}

						n.RecoverInvoiceDetailsBalance();

						validateBalances += CalculateInvoiceDetailsCashDiscBalance(n, doc);						

						maxRetainageTran = maxRetainageTran == null || maxRetainageTran.CuryOrigRetainageAmt < n.CuryOrigRetainageAmt ? n : maxRetainageTran;
						maxBalanceTran = maxBalanceTran == null || maxBalanceTran.CuryOrigTranAmt < n.CuryOrigTranAmt ? n : maxBalanceTran;

						if (ardoc.IsOriginalRetainageDocument() &&
							ardoc.IsRetainageReversing == true)
						{
							ARTran origRetainageTran = GetOriginalRetainageLine(ardoc, n);

							if (origRetainageTran != null)
							{
								n.CuryRetainageBal = 0m;
								n.RetainageBal = 0m;
								origRetainageTran.CuryRetainageBal = 0m;
								origRetainageTran.RetainageBal = 0m;
								ARTran_TranType_RefNbr.Update(origRetainageTran);
							}
						}

						if (ardoc.IsRetainageDocument == true)
						{
							AdjustOriginalRetainageLineBalance(ardoc, n, n.CuryOrigTranAmt, n.OrigTranAmt);
						}

						if (isPayByLineRetainageReversing)
						{
							n.CuryTranBal = 0m;
							n.TranBal = 0m;
							n.CuryRetainageBal = 0m;
							n.RetainageBal = 0m;
						}
					}

						ARTran_TranType_RefNbr.Cache.MarkUpdated(n);
					}

				if (calculateLineBalances &&
					// Calculate line balances only once during the
					// prebook or release process.
					// 
					doc.Released != true)
				{
					if ((validateBalances.CashDiscountBalance.Cury != doc.CuryOrigDiscAmt ||
						validateBalances.CashDiscountBalance.Base != doc.OrigDiscAmt) &&
						maxBalanceTran != null)
					{
						maxBalanceTran.CuryCashDiscBal -= validateBalances.CashDiscountBalance.Cury - doc.CuryOrigDiscAmt;
						maxBalanceTran.CashDiscBal -= validateBalances.CashDiscountBalance.Base - doc.OrigDiscAmt;
						ARTran_TranType_RefNbr.Update(maxBalanceTran);
					}

					if (!_IsIntegrityCheck)
					{
						if (validateBalances.RetainageBalance.Cury != doc.CuryRetainageTotal)
						{
							throw new PXException(AP.Messages.SumLineRetainageBalancesNotEqualRetainageTotal);
						}
						else if (validateBalances.RetainageBalance.Base != doc.RetainageTotal &&
							maxRetainageTran != null)
						{
							decimal? retainageDelta = validateBalances.RetainageBalance.Base - doc.RetainageTotal;
							maxBalanceTran.OrigRetainageAmt -= retainageDelta;
							if (maxBalanceTran.RetainageBal != 0m)
							{
								maxBalanceTran.RetainageBal -= retainageDelta;
							}
							ARTran_TranType_RefNbr.Update(maxBalanceTran);
						}

						if (validateBalances.TranBalance.Cury != doc.CuryDocBal)
						{
							throw new PXException(AP.Messages.SumLineBalancesNotEqualDocBalance);
						}
						else if (validateBalances.TranBalance.Base != doc.DocBal &&
							maxBalanceTran != null)
						{
							decimal? balanceDelta = validateBalances.TranBalance.Base - doc.DocBal;
							maxBalanceTran.OrigTranAmt -= balanceDelta;
							if (maxBalanceTran.TranBal != 0m)
							{
								maxBalanceTran.TranBal -= balanceDelta;
							}
							ARTran_TranType_RefNbr.Update(maxBalanceTran);

							//if (ardoc.IsChildRetainageDocument())
							if (ardoc.IsRetainageDocument == true)
							{
								AdjustOriginalRetainageLineBalance(ardoc, maxBalanceTran, 0m, balanceDelta);
							}
						}
					}
				}

				var docInclTaxDiscrepancy = 0.0m;

				foreach (PXResult<ARTaxTran, Tax, Account> rs in ARTaxTran_TranType_RefNbr.Select(ardoc.DocType, ardoc.RefNbr))
				{
					ARTaxTran x = (ARTaxTran)rs;
					Tax salestax = (Tax)rs;
					Account taxAccount = (Account)rs;
					
					if (ardoc.TaxCalcMode == TaxCalculationMode.Gross || salestax.TaxCalcLevel == CSTaxCalcLevel.Inclusive && ardoc.TaxCalcMode != TaxCalculationMode.Net)
						docInclTaxDiscrepancy += ((x.CuryTaxAmtSumm ?? 0.0m) - (x.CuryTaxAmt ?? 0.0m) + (x.CuryRetainedTaxAmtSumm ?? 0.0m) - (x.CuryRetainedTaxAmt ?? 0.0m)) * (salestax.ReverseTax == true ? -1.0M : 1.0M);

					if (salestax.TaxType == CSTaxType.PerUnit)
					{
						PostPerUnitTaxAmounts(je, ardoc, new_info, perUnitAggregatedTax: x, perUnitTax: salestax, isDebitTaxTran: isDebit);
					}
					else
					{
						PostGeneralTax(je, ardoc, doc, salestax, x, taxAccount, new_info, isDebit, customer);
					}

					if (ardoc.DocType == ARDocType.CashSale || ardoc.DocType == ARDocType.CashReturn)
					{
						PostReduceOnEarlyPaymentTran(je, ardoc, x.CuryTaxDiscountAmt, x.TaxDiscountAmt, customer, new_info, isDebit);
					}

					x.Released = true;
					ARTaxTran_TranType_RefNbr.Cache.SetStatus(x, PXEntryStatus.Updated);

					if (PXAccess.FeatureInstalled<FeaturesSet.vATReporting>() && _IsIntegrityCheck == false &&
						(x.TaxType == TX.TaxType.PendingPurchase || x.TaxType == TX.TaxType.PendingSales))
					{
						AP.Vendor vendor = PXSelect<AP.Vendor, Where<AP.Vendor.bAccountID, 
							Equal<Required<AP.Vendor.bAccountID>>>>.SelectSingleBound(this, null, x.VendorID);

						decimal mult = ReportTaxProcess.GetMultByTranType(BatchModule.AR, x.TranType);

						string reversalMethod = String.Empty;
						if (x.TranType == ARDocType.CashSale || x.TranType == ARDocType.CashReturn)
							reversalMethod = SVATTaxReversalMethods.OnDocuments;
						else if(x.TranType == ARDocType.PrepaymentInvoice)
							reversalMethod = SVATTaxReversalMethods.OnPrepayment;
						else
							reversalMethod = vendor?.SVATReversalMethod;

						var pendingVatDocs = new List<ARRegister>() { doc };
						if (_InstallmentType == TermsInstallmentType.Multiple)
						{
							pendingVatDocs = ret;
						}

						decimal taxableInstallmentsTotal = 0;
						decimal taxInstallmentsTotal = 0;
						decimal curyTaxableInstallmentsTotal = 0;
						decimal curyTaxInstallmentsTotal = 0;

						SVATConversionHist biggestSVAT = null;
						for (int i = 0; i < pendingVatDocs.Count; i++)
						{
							var document = pendingVatDocs[i];
						SVATConversionHist histSVAT = new SVATConversionHist
						{
							Module = BatchModule.AR,
							AdjdBranchID = x.BranchID,
							AdjdDocType = x.TranType,
								AdjdRefNbr = document.RefNbr,
							AdjgDocType = x.TranType,
								AdjgRefNbr = document.RefNbr,
								AdjdDocDate = document.DocDate,

							TaxID = x.TaxID,
							TaxType = x.TaxType,
							TaxRate = x.TaxRate,
							VendorID = x.VendorID,
							ReversalMethod = reversalMethod,

							CuryInfoID = x.CuryInfoID,
						};

							decimal installmentPct = (doc.CuryOrigDocAmt != 0m ? document.CuryOrigDocAmt / doc.CuryOrigDocAmt : 0) ?? 0m;
							histSVAT.FillAmounts(GetExtension<MultiCurrency>().GetCurrencyInfo(x.CuryInfoID), x.CuryTaxableAmt, x.CuryTaxAmt, installmentPct * mult);

							FinPeriodIDAttribute.SetPeriodsByMaster<SVATConversionHist.adjdFinPeriodID>(SVATConversionHistory.Cache, histSVAT, doc.TranPeriodID);

							taxableInstallmentsTotal += histSVAT.TaxableAmt.Value;
							taxInstallmentsTotal += histSVAT.TaxAmt.Value;
							curyTaxableInstallmentsTotal += histSVAT.CuryTaxableAmt.Value;
							curyTaxInstallmentsTotal += histSVAT.CuryTaxAmt.Value;

							histSVAT = SVATConversionHistory.Insert(histSVAT);
							biggestSVAT = biggestSVAT == null || (histSVAT.CuryTaxAmt > biggestSVAT.CuryTaxAmt) ? histSVAT : biggestSVAT;
						}

						var taxableAmtDiff = (x.TaxableAmt * mult) - taxableInstallmentsTotal;
						var taxAmtDiff = (x.TaxAmt * mult) - taxInstallmentsTotal;
						//Set base currency leftovers
						if (taxableAmtDiff != 0 || taxAmtDiff != 0)
						{
							biggestSVAT.TaxableAmt += taxableAmtDiff;
							biggestSVAT.TaxAmt += taxAmtDiff;
							biggestSVAT.UnrecognizedTaxAmt = biggestSVAT.TaxAmt;
							biggestSVAT = SVATConversionHistory.Update(biggestSVAT);
						}

						var curyTaxableAmtDiff = (x.CuryTaxableAmt * mult) - curyTaxableInstallmentsTotal;
						var curyTaxAmtDiff = (x.CuryTaxAmt * mult) - curyTaxInstallmentsTotal;
						//Set currency leftovers
						if (curyTaxableAmtDiff != 0 || curyTaxAmtDiff != 0)
						{
							biggestSVAT.CuryTaxableAmt += curyTaxableAmtDiff;
							biggestSVAT.CuryTaxAmt += curyTaxAmtDiff;
							biggestSVAT.CuryUnrecognizedTaxAmt = biggestSVAT.CuryTaxAmt;
							biggestSVAT = SVATConversionHistory.Update(biggestSVAT);
						}
					}

					if (_IsIntegrityCheck == false && ardoc.IsPrepaymentInvoiceDocument())
					{
						string reversalMethod = SVATTaxReversalMethods.OnPrepayment;
						var pendingVatDocs = new List<ARRegister>() { doc };

						for (int i = 0; i < pendingVatDocs.Count; i++)
						{
							var document = pendingVatDocs[i];
							SVATConversionHist histSVAT = new SVATConversionHist
							{
								Module = BatchModule.AR,
								AdjdBranchID = x.BranchID,
								AdjdDocType = x.TranType,
								AdjdRefNbr = document.RefNbr,
								AdjgDocType = x.TranType,
								AdjgRefNbr = document.RefNbr,
								AdjdDocDate = document.DocDate,
								TaxID = x.TaxID,
								TaxType = x.TaxType,
								TaxRate = x.TaxRate,
								VendorID = x.VendorID,
								ReversalMethod = reversalMethod,

								CuryInfoID = x.CuryInfoID,
							};

							histSVAT.FillAmounts(GetExtension<MultiCurrency>().GetCurrencyInfo(x.CuryInfoID), x.CuryTaxableAmt, x.CuryTaxAmt, 1);

							FinPeriodIDAttribute.SetPeriodsByMaster<SVATConversionHist.adjdFinPeriodID>(SVATConversionHistory.Cache, histSVAT, doc.TranPeriodID);

							histSVAT = SVATConversionHistory.Insert(histSVAT);
						}
					}
				}

				foreach (ARSalesPerTran n in ARDoc_SalesPerTrans.Select(doc.DocType, doc.RefNbr))
				{
					PXCache<ARSalesPerTran>.StoreOriginal(this, n);
					//multiply installments master and deferred revenue should not have commission
					n.Released = doc.OpenDoc;
					ARDoc_SalesPerTrans.Cache.Update(n);
				}

				//Process ARTranPost
				ProcessOriginTranPost(ardoc, itemDiscount, IsMasterInstallment);
				if (ardoc.IsPrepaymentInvoiceDocument())
					ProcessPrepaymentTranPost(ardoc);
				//Proces ARTranPost for Retainage
				if (ardoc.IsRetainageDocument == true)
					ProcessRetainageTranPost(ardoc);
				
				if (ardoc.DocType == ARDocType.SmallCreditWO && ardoc.Voided == true)
				{
					ARAdjust maxAdjust =
						PXSelect<ARAdjust,
						Where<ARAdjust.adjdDocType, Equal<Current<ARRegister.docType>>,
							And<ARAdjust.adjdRefNbr, Equal<Current<ARRegister.refNbr>>>>,
						OrderBy<Desc<ARAdjust.adjNbr>>>
						.SelectSingleBound(this, new object[] {ardoc});
					ProcessVoidWOTranPost(ardoc, maxAdjust);
				}

				if (_IsIntegrityCheck == false)
				{
					foreach (PXResult<ARAdjust, ARPayment> appres in PXSelectJoin<ARAdjust, 
						InnerJoin<ARPayment, On<ARPayment.docType, Equal<ARAdjust.adjgDocType>, 
							And<ARPayment.refNbr, Equal<ARAdjust.adjgRefNbr>>>>, 
						Where<ARAdjust.adjdDocType, Equal<Required<ARAdjust.adjdDocType>>, 
							And<ARAdjust.adjdRefNbr, Equal<Required<ARAdjust.adjdRefNbr>>, 
							And<ARAdjust.released, Equal<False>, 
							And<ARAdjust.voided, Equal<False>>>>>>.Select(this, doc.DocType, doc.RefNbr))
					{
						ARAdjust adj = (ARAdjust)appres;
						ARPayment payment = (ARPayment)appres;

						if (adj.CuryAdjdAmt > 0m || adj.Hold == true)
						{
							if (_InstallmentType != null && _InstallmentType != TermsInstallmentType.Single)
							{
								throw new PXException(Messages.PrepaymentAppliedToMultiplyInstallments);
							}

							//sync fields with the max value:
							if (adj.AdjdDocType != ARInvoiceType.SmallCreditWO && string.Compare(payment.AdjTranPeriodID, adj.AdjgTranPeriodID) >= 0)
							{
								adj.AdjgDocDate = payment.AdjDate;

                                FinPeriodIDAttribute.SetPeriodsByMaster<ARAdjust.adjgFinPeriodID>(
									ARAdjust_AdjgDocType_RefNbr_CustomerID.Cache, adj, payment.AdjTranPeriodID);
							}

							if (payment.Released == true)
							{
								//sync fields with the max value:
								if (DateTime.Compare((DateTime)payment.AdjDate, (DateTime)adj.AdjdDocDate) < 0)
								{
									payment.AdjDate = adj.AdjdDocDate;

									FinPeriodIDAttribute.SetPeriodsByMaster<ARPayment.adjFinPeriodID>(
										ARPayment_DocType_RefNbr.Cache, payment, adj.AdjdTranPeriodID);
								}
								else if (String.CompareOrdinal(payment.AdjTranPeriodID, adj.AdjdTranPeriodID) < 0)
								{
									FinPeriodIDAttribute.SetPeriodsByMaster<ARPayment.adjFinPeriodID>(
										ARPayment_DocType_RefNbr.Cache, payment, adj.AdjdTranPeriodID);
								}
								ret.Add(payment);

								// To prevent umerged cache update from the joined ARPayment part
								ARRegister cachedDoc = (ARRegister)ARDocument.Cache.Locate(payment);
								if (cachedDoc != null)
								{
									PXCache<ARRegister>.RestoreCopy(payment, cachedDoc);
									ARDocument.Cache.SetStatus(cachedDoc, PXEntryStatus.Notchanged);
								}

								using (new DisableFormulaCalculationScope(ARPayment_DocType_RefNbr.Cache, typeof(ARPayment.curyRetainageReleased)))
								{
								ARPayment_DocType_RefNbr.Cache.Update(payment);
								}
							}

							decimal? adj_rgol_amt = adj.RGOLAmt;

							if (adj.CuryAdjdWOAmt != 0m)
							{
								ARInvoiceEntry.MultiCurrency multiCurrencyExt = InvoiceEntryGraph.GetExtension<ARInvoiceEntry.MultiCurrency>();

								CurrencyInfo invoice_info = multiCurrencyExt.GetCurrencyInfo(adj.AdjdCuryInfoID);
								CurrencyInfo payment_info = multiCurrencyExt.GetCurrencyInfo(adj.AdjgCuryInfoID);
								CurrencyInfo payment_originfo = multiCurrencyExt.GetCurrencyInfo(payment.CuryInfoID);

								decimal? whtax_rgol_amt = new RGOLCalculator(
									invoice_info,
									payment_info,
									payment_originfo
									).CalcRGOL(
									adj.CuryAdjdWhTaxAmt,
									adj.AdjWhTaxAmt)
									.RgolAmt;

								adj.AdjWOAmt += whtax_rgol_amt;
								adj_rgol_amt -= whtax_rgol_amt;
							}

							if(ARDocType.Payable(doc.DocType) == true)
							{
							adj.AdjAmt += adj_rgol_amt;
							adj.RGOLAmt = -adj.RGOLAmt;
							}
							else
							{
								adj.AdjAmt -= adj_rgol_amt;
								adj.RGOLAmt = -adj.RGOLAmt;
							}
							adj.Hold = false;

							ARAdjust_AdjgDocType_RefNbr_CustomerID.Cache.SetStatus(adj, PXEntryStatus.Updated);
						}
					}

					// We should add a CreditMemo document to the list
					// to release further its payment part and increment
					// an adjustments counter.
					// 
					if (doc.DocType == ARDocType.CreditMemo)
					{
						doc.AdjCntr = 0;

						ret.Insert(0, doc);
					}

					CreatePayment(res, ref ret);
				}

				Batch arbatch = je.BatchModule.Current;

				ReleaseInvoiceBatchPostProcessing(je, ardoc, arbatch);

				decimal curyCreditDiff = Math.Round((decimal)(arbatch.CuryDebitTotal - arbatch.CuryCreditTotal), 4);
				decimal creditDiff = Math.Round((decimal)(arbatch.DebitTotal - arbatch.CreditTotal), 4);

				if (docInclTaxDiscrepancy != 0)
				{
					ProcessTaxDiscrepancy(je, arbatch, ardoc, new_info, docInclTaxDiscrepancy);
					curyCreditDiff = Math.Round((decimal)(arbatch.CuryDebitTotal - arbatch.CuryCreditTotal), 4);
					creditDiff = Math.Round((decimal)(arbatch.DebitTotal - arbatch.CreditTotal), 4);
				}

				if (Math.Abs(curyCreditDiff) >= 0.00005m)
				{
					VerifyRoundingAllowed(ardoc, arbatch, je.currencyinfo.Current.BaseCuryID);
				}

				if (Math.Abs(curyCreditDiff) >= 0.00005m || Math.Abs(creditDiff) >= 0.00005m)
				{
					ProcessInvoiceRounding(je, arbatch, ardoc, curyCreditDiff, creditDiff);
				}

				if (doc.HasZeroBalance<ARRegister.curyDocBal, ARTran.curyTranBal>(this) && 
					(!doc.IsOriginalRetainageDocument() || doc.HasZeroBalance<ARRegister.curyRetainageUnreleasedAmt, ARTran.curyRetainageBal>(this)) &&
					(!doc.IsOriginalRetainageDocument() || doc.DocType != ARDocType.CreditMemo))
				{
					doc.DocBal = 0m;
					doc.CuryDiscBal = 0m;
					doc.DiscBal = 0m;

					doc.OpenDoc = false;
					doc.ClosedDate = doc.DocDate;
					doc.ClosedFinPeriodID = doc.FinPeriodID;
					doc.ClosedTranPeriodID = doc.TranPeriodID;
					RaiseInvoiceEvent(doc, ARInvoice.Events.Select(ev => ev.CloseDocument));
				}
			}

			return ret;
		}

		
		protected virtual void AdjustOriginalRetainageLineBalance(ARRegister document, ARTran tran, decimal? curyAmount, decimal? baseAmount)
		{
			ARTran origRetainageTran = GetOriginalRetainageLine(tran);

			if (origRetainageTran != null)
			{
				// We should consider amount sign both for Original and Child Retainage document
				// to cover all possible combinations between INV, CRM and DRM documents.
				decimal sign = ARDocType.SignAmount(origRetainageTran.TranType) * document.SignAmount ?? 0m;

				origRetainageTran.CuryRetainageBal -= (curyAmount ?? 0m) * sign;
				origRetainageTran.RetainageBal -= (baseAmount ?? 0m) * sign;
				origRetainageTran = ARTran_TranType_RefNbr.Update(origRetainageTran);

				Sign balanceSign = origRetainageTran.CuryOrigRetainageAmt < 0m ? Sign.Minus : Sign.Plus;

				if (!_IsIntegrityCheck &&
					(origRetainageTran.CuryRetainageBal * balanceSign < 0m || 
						origRetainageTran.CuryRetainageBal * balanceSign > origRetainageTran.CuryOrigRetainageAmt * balanceSign))
				{
					throw new PXException(AP.Messages.RetainageUnreleasedBalanceNegative);
				}
			}
		}

		protected virtual LineBalances AdjustInvoiceDetailsBalanceByLine(ARRegister doc, ARTran tran)
		{
			// Retainage balance
			// 
			tran.CuryOrigRetainageAmt += tran.CuryRetainageAmt;
			tran.OrigRetainageAmt += tran.RetainageAmt;

			// Transaction balance
			// 
			tran.CuryOrigTranAmt += tran.CuryTranAmt;
			tran.OrigTranAmt += tran.TranAmt;

			return new LineBalances(
				new Amount(),
				new Amount(tran.CuryRetainageAmt ?? 0m, tran.RetainageAmt ?? 0m),
				new Amount(tran.CuryTranAmt ?? 0m, tran.TranAmt ?? 0m));
		}

		protected virtual LineBalances CalculateInvoiceDetailsCashDiscBalance(ARTran tran, ARRegister doc)
		{
			decimal discountPercent = (doc.CuryOrigDocAmt ?? 0m) != 0m
				? (tran.CuryOrigTranAmt ?? 0m) / (doc.CuryOrigDocAmt ?? 0m)
				: 0m;

			CurrencyInfo currencyInfo = GetExtension<MultiCurrency>().GetCurrencyInfo(tran.CuryInfoID);
			tran.CuryCashDiscBal = currencyInfo.RoundCury((doc.CuryOrigDiscAmt ?? 0m) * discountPercent);
			tran.CashDiscBal = currencyInfo.RoundCury((doc.OrigDiscAmt ?? 0m) * discountPercent);

			return new LineBalances(
				new Amount(tran.CuryCashDiscBal ?? 0m, tran.CashDiscBal ?? 0m),
				new Amount(),
				new Amount());
		}

		public static bool IncludeTaxInLineBalance(Tax tax)
		{
			return
				tax != null &&
				tax.TaxType != CSTaxType.Use &&
				tax.TaxType != CSTaxType.Withholding &&
				tax.TaxCalcLevel != CSTaxCalcLevel.Inclusive;
		}

		protected virtual LineBalances AdjustInvoiceDetailsBalanceByTax(
			ARRegister doc,
			ARTran tran,
			ARTax artax,
			Tax tax)
		{
			bool includeBalance = 
				artax?.TaxID != null && 
				IncludeTaxInLineBalance(tax);

			bool includeTax =
				artax?.TaxID != null &&
				tax != null &&
				tax.TaxType != CSTaxType.Use;

			decimal sign = tax.ReverseTax == true ? -1m : 1m;

			decimal curyTaxAmt = (artax.CuryTaxAmt ?? 0m) + (artax.CuryExpenseAmt ?? 0m);
			decimal baseTaxAmt = (artax.TaxAmt ?? 0m) + (artax.ExpenseAmt ?? 0m);
			curyTaxAmt *= sign;
			baseTaxAmt *= sign;

			decimal curyRetainedTaxAmt = artax.CuryRetainedTaxAmt ?? 0m;
			decimal baseRetainedTaxAmt = artax.RetainedTaxAmt ?? 0m;
			curyRetainedTaxAmt *= sign;
			baseRetainedTaxAmt *= sign;

			LineBalances balances = includeBalance
				? new LineBalances(
					new Amount(0m, 0m),
					new Amount(curyRetainedTaxAmt, baseRetainedTaxAmt),
					new Amount(curyTaxAmt, baseTaxAmt))
				: new LineBalances(0m);

			// Retainage balance
			// 
			tran.CuryRetainedTaxableAmt += includeTax ? artax.CuryRetainedTaxableAmt ?? 0m : 0m;
			tran.RetainedTaxableAmt += includeTax ? artax.RetainedTaxableAmt ?? 0m : 0m;
			tran.CuryRetainedTaxAmt += includeTax ? curyRetainedTaxAmt : 0m;
			tran.RetainedTaxAmt += includeTax ? baseRetainedTaxAmt : 0m;

			tran.CuryOrigRetainageAmt += balances.RetainageBalance.Cury;
			tran.OrigRetainageAmt += balances.RetainageBalance.Base;

			// Transaction balance
			// 
			tran.CuryOrigTaxableAmt += includeTax ? artax.CuryTaxableAmt ?? 0m : 0m;
			tran.OrigTaxableAmt += includeTax ? artax.TaxableAmt ?? 0m : 0m;
			tran.CuryOrigTaxAmt += includeTax ? curyTaxAmt : 0m;
			tran.OrigTaxAmt += includeTax ? baseTaxAmt : 0m;

			tran.CuryOrigTranAmt += balances.TranBalance.Cury;
			tran.OrigTranAmt += balances.TranBalance.Base;

			return balances;
		}

		[Obsolete(Common.InternalMessages.MethodIsObsoleteAndWillBeRemoved2024R2)]
		protected virtual void PostGeneralTax(JournalEntry je, ARInvoice ardoc, ARRegister doc, Tax salestax, ARTaxTran x, Account taxAccount,
											  CurrencyInfo new_info, bool isDebit)
		{
			PostGeneralTax(je, ardoc, doc, salestax, x, taxAccount, new_info, isDebit, null);
		}

		protected virtual void PostGeneralTax(JournalEntry je, ARInvoice ardoc, ARRegister doc, Tax salestax, ARTaxTran x, Account taxAccount,
											  CurrencyInfo new_info, bool isDebit, Customer customer)
		{
			GLTran tran = new GLTran();
			tran.SummPost = this.SummPost;
			tran.BranchID = x.BranchID;
			tran.CuryInfoID = new_info.CuryInfoID;
			tran.TranType = x.TranType;
			tran.TranClass = doc.IsPrepaymentInvoiceDocument() ? GLTran.tranClass.PrepaymentInvoice : GLTran.tranClass.Tax;
			tran.RefNbr = x.RefNbr;
			tran.TranDate = x.TranDate;
			tran.AccountID = doc.IsPrepaymentInvoiceDocument() ? doc.PrepaymentAccountID : x.AccountID;
			tran.SubID = doc.IsPrepaymentInvoiceDocument() ? doc.PrepaymentSubID : x.SubID;
			tran.TranDesc = x.TaxID;
			FinPeriodIDAttribute.SetPeriodsByMaster<GLTran.finPeriodID>(je.GLTranModuleBatNbr.Cache, tran, doc.TranPeriodID);
			tran.CuryDebitAmt = isDebit ? x.CuryTaxAmt : 0m;
			tran.DebitAmt = isDebit ? x.TaxAmt : 0m;
			tran.CuryCreditAmt = isDebit ? 0m : x.CuryTaxAmt;
			tran.CreditAmt = isDebit ? 0m : x.TaxAmt;
			tran.Released = true;
			tran.ReferenceID = ardoc.CustomerID;
			
			SetProjectAndTaxID(tran, taxAccount, ardoc);

			InsertInvoiceTaxTransaction(je, tran,
				new GLTranInsertionContext { ARRegisterRecord = doc, ARTaxTranRecord = x });

			if (ardoc.IsPrepaymentInvoiceDocument())
			{
				UpdateHistory(tran, customer);
				UpdateHistory(tran, customer, new_info);
			}

			PostRetainedTax(je, ardoc, tran, x, salestax);
		}

		private void SetProjectAndTaxID(GLTran tran, Account account, ARInvoice ardoc)
		{
			if (account?.AccountGroupID == null || ardoc.ProjectID == null || ProjectDefaultAttribute.IsNonProject(ardoc.ProjectID))
			{
				tran.ProjectID = ProjectDefaultAttribute.NonProject();
				tran.TaskID = null;
			}
			else
			{
				PMAccountTask mapping = PXSelect<PMAccountTask,
					Where<PMAccountTask.projectID, Equal<Required<PMAccountTask.projectID>>,
					And<PMAccountTask.accountID, Equal<Required<PMAccountTask.accountID>>>>>.Select(this, ardoc.ProjectID, account.AccountID);

				if (mapping == null)
				{
					throw new PXException(Messages.TaxAccountTaskMappingNotFound, account.AccountCD);
				}

				tran.ProjectID = ardoc.ProjectID;
				tran.TaskID = mapping.TaskID;
			}
		}

		private void ProcessInvoiceRounding(
			JournalEntry je,
			Batch arbatch,
			ARInvoice ardoc,
			decimal curyCreditDiff,
			decimal creditDiff)
		{
			Currency currency = PXSelect<Currency, Where<Currency.curyID, Equal<Required<CurrencyInfo.curyID>>>>.Select(this, ardoc.CuryID);

			if (currency.RoundingGainAcctID == null || currency.RoundingGainSubID == null)
			{
				throw new PXException(AP.Messages.NoRoundingGainLossAccSub, currency.CuryID);
			}

			if (curyCreditDiff != 0m)
			{
				GLTran tran = new GLTran();
				tran.SummPost = true;
				tran.BranchID = ardoc.BranchID;

				if (Math.Sign(curyCreditDiff) == 1)
				{
					tran.AccountID = currency.RoundingGainAcctID;
					tran.SubID = CM.GainLossSubAccountMaskAttribute.GetSubID<Currency.roundingGainSubID>(je, tran.BranchID, currency);
					tran.CuryCreditAmt = Math.Abs(curyCreditDiff);
					tran.CuryDebitAmt = 0m;
				}
				else
				{
					tran.AccountID = currency.RoundingLossAcctID;
					tran.SubID = CM.GainLossSubAccountMaskAttribute.GetSubID<Currency.roundingLossSubID>(je, tran.BranchID, currency);
					tran.CuryCreditAmt = 0m;
					tran.CuryDebitAmt = Math.Abs(curyCreditDiff);
				}

				tran.CreditAmt = 0m;
				tran.DebitAmt = 0m;
				tran.TranType = ardoc.DocType;
				tran.RefNbr = ardoc.RefNbr;
				tran.TranClass = GLTran.tranClass.Normal;
				tran.TranDesc = GL.Messages.RoundingDiff;
				tran.LedgerID = arbatch.LedgerID;
				FinPeriodIDAttribute.SetPeriodsByMaster<GLTran.finPeriodID>(je.GLTranModuleBatNbr.Cache, tran, arbatch.TranPeriodID);
				tran.TranDate = ardoc.DocDate;
				tran.ReferenceID = ardoc.CustomerID;
				tran.Released = true;

				tran.CuryInfoID = je.currencyinfo.Insert(new CM.CurrencyInfo())?.CuryInfoID;
					InsertInvoiceRoundingTransaction(je, tran, 
						new GLTranInsertionContext { ARRegisterRecord = ardoc });
			}

			if (creditDiff != 0m)
			{
				GLTran tran = new GLTran
				{
					SummPost = true,
					BranchID = ardoc.BranchID
				};

				if (Math.Sign(creditDiff) == 1)
				{
					tran.AccountID = currency.RoundingGainAcctID;
					tran.SubID = CM.GainLossSubAccountMaskAttribute.GetSubID<Currency.roundingGainSubID>(je, tran.BranchID, currency);
					tran.CreditAmt = Math.Abs(creditDiff);
					tran.DebitAmt = 0m;
				}
				else
				{
					tran.AccountID = currency.RoundingLossAcctID;
					tran.SubID = CM.GainLossSubAccountMaskAttribute.GetSubID<Currency.roundingLossSubID>(je, tran.BranchID, currency);
					tran.CreditAmt = 0m;
					tran.DebitAmt = Math.Abs(creditDiff);
				}

				tran.CuryCreditAmt = 0m;
				tran.CuryDebitAmt = 0m;
				tran.TranType = ardoc.DocType;
				tran.RefNbr = ardoc.RefNbr;
				tran.TranClass = GLTran.tranClass.Normal;
				tran.TranDesc = GL.Messages.RoundingDiff;
				tran.LedgerID = arbatch.LedgerID;
			    FinPeriodIDAttribute.SetPeriodsByMaster<GLTran.finPeriodID>(je.GLTranModuleBatNbr.Cache, tran, arbatch.TranPeriodID);
				tran.TranDate = ardoc.DocDate;
				tran.ReferenceID = ardoc.CustomerID;
				tran.Released = true;

				tran.CuryInfoID = je.currencyinfo.Insert(new CM.CurrencyInfo())?.CuryInfoID;
				InsertInvoiceRoundingTransaction(je, tran, new GLTranInsertionContext { ARRegisterRecord = ardoc });
			}
		}

		protected virtual void ProcessTaxDiscrepancy(
			JournalEntry je,
			Batch arbatch,
			ARInvoice ardoc,
			CurrencyInfo currencyInfo,
			decimal docInclTaxDiscrepancy)
		{
			if (docInclTaxDiscrepancy == 0) return;

				if (Math.Abs(docInclTaxDiscrepancy) > CM.CurrencyCollection.GetCurrency(currencyInfo.BaseCuryID).RoundingLimit 
			&& (PXAccess.FeatureInstalled<FeaturesSet.netGrossEntryMode>() || PXAccess.FeatureInstalled<FeaturesSet.invoiceRounding>()))
			{
					throw new PXException(AP.Messages.RoundingAmountTooBig,
						je.currencyinfo.Current.BaseCuryID,
						Math.Abs(Math.Round(docInclTaxDiscrepancy, currencyInfo.CuryPrecision ?? 4)),
						PXDBQuantityAttribute.Round(CM.CurrencyCollection.GetCurrency(currencyInfo.BaseCuryID).RoundingLimit));
				}

				TXSetup txsetup = PXSetup<TXSetup>.Select(this);
				if (txsetup?.TaxRoundingGainAcctID == null || txsetup?.TaxRoundingLossAcctID == null)
				{
					throw new PXException(TX.Messages.TaxRoundingGainLossAccountsRequired);
				}

				var roundAcctID = docInclTaxDiscrepancy > 0 ? txsetup.TaxRoundingGainAcctID : txsetup.TaxRoundingLossAcctID;
				var roundSubID = docInclTaxDiscrepancy > 0 ? txsetup.TaxRoundingGainSubID : txsetup.TaxRoundingLossSubID;
				var isDebit = (ardoc.DrCr == DrCr.Debit);
				
			GLTran diffTran = new GLTran
			{
				SummPost = this.SummPost,
				BranchID = ardoc.BranchID,
				CuryInfoID = currencyInfo.CuryInfoID,
				TranType = ardoc.DocType,
				TranClass = GLTran.tranClass.RealizedAndRoundingGOL,
				RefNbr = ardoc.RefNbr,
				TranDate = ardoc.DocDate,
				AccountID = roundAcctID,
				SubID = roundSubID,
				TranDesc = TX.Messages.DocumentInclusiveTaxDiscrepancy,
				CuryDebitAmt = isDebit ? docInclTaxDiscrepancy : 0m,
				DebitAmt = isDebit ? currencyInfo.CuryConvBase(docInclTaxDiscrepancy) : 0m,
				CuryCreditAmt = isDebit ? 0m : docInclTaxDiscrepancy,
				CreditAmt = isDebit ? 0m : currencyInfo.CuryConvBase(docInclTaxDiscrepancy),
				Released = true,
				ReferenceID = ardoc.CustomerID
			};

				InsertInvoiceTransaction(je, diffTran, new GLTranInsertionContext { ARRegisterRecord = ardoc });
			}

		public virtual void CreateExpenseAccrualTransactions(JournalEntry je, ARRegister doc, ARTran n, GLTran origTran)
		{
			GLTran expenseAccrualTran = (GLTran)je.GLTranModuleBatNbr.Cache.CreateCopy(origTran);
			expenseAccrualTran.Qty = (n.DrCr == DrCr.Debit) ? -1 * n.Qty : n.Qty;
			expenseAccrualTran.AccountID = n.ExpenseAccrualAccountID;
			expenseAccrualTran.SubID = GetValueInt<ARTran.expenseAccrualSubID>(je, n);
			expenseAccrualTran.CuryDebitAmt = (n.DrCr == DrCr.Debit) ? n.CuryAccruedCost : 0m;
			expenseAccrualTran.DebitAmt = (n.DrCr == DrCr.Debit) ? n.AccruedCost : 0m;
			expenseAccrualTran.CuryCreditAmt = (n.DrCr == DrCr.Debit) ? 0m : n.CuryAccruedCost;
			expenseAccrualTran.CreditAmt = (n.DrCr == DrCr.Debit) ? 0m : n.AccruedCost;
			expenseAccrualTran.ProjectID = ProjectDefaultAttribute.NonProject();
			expenseAccrualTran.TaskID = null;
			expenseAccrualTran.CostCodeID = null;

			InsertInvoiceDetailsTransaction(je, expenseAccrualTran,
									new GLTranInsertionContext { ARRegisterRecord = doc, ARTranRecord = n });
			

			GLTran expenseTran = (GLTran)je.GLTranModuleBatNbr.Cache.CreateCopy(origTran);
			expenseTran.Qty = (n.DrCr == DrCr.Debit) ? n.Qty : -1 * n.Qty;
			expenseTran.AccountID = n.ExpenseAccountID;
			expenseTran.SubID = GetValueInt<ARTran.expenseSubID>(je, n);
			expenseTran.CuryDebitAmt = (n.DrCr == DrCr.Debit) ? 0m : n.CuryAccruedCost;
			expenseTran.DebitAmt = (n.DrCr == DrCr.Debit) ? 0m : n.AccruedCost;
			expenseTran.CuryCreditAmt = (n.DrCr == DrCr.Debit) ? n.CuryAccruedCost : 0m;
			expenseTran.CreditAmt = (n.DrCr == DrCr.Debit) ? n.AccruedCost : 0m;

			InsertInvoiceDetailsTransaction(je, expenseTran,
									new GLTranInsertionContext { ARRegisterRecord = doc, ARTranRecord = n });

			je.GLTranModuleBatNbr.Current = origTran;
		}

		public virtual void ProcessOriginTranPost(ARInvoice doc, Amount itemDiscount, bool masterInstallment)
		{
			ARTranPost post = CreateTranPost(doc);
			post.Type = ARTranPost.type.Origin;
			post.CuryAmt = doc.CuryOrigDocAmt;
			post.Amt = doc.OrigDocAmt;
			post.CuryRetainageAmt = doc.CuryRetainageTotal;
			post.RetainageAmt = doc.RetainageTotal;
			if(IsNeedUpdateHistoryForTransaction(post.FinPeriodID))
				post = TranPost.Insert(post);			
			post.CuryItemDiscAmt = itemDiscount.Cury;
			post.ItemDiscAmt = itemDiscount.Base;
			post.TranRefNbr = doc.MasterRefNbr ?? post.TranRefNbr;
			if (masterInstallment)
			{
				post.AccountID = null;
				post.SubID = null;
				ProcessInstallmentTranPost(doc);
			}

			if (doc.IsRetainageReversing == true)
			{
				var postR = CreateTranPost(doc);
				postR.SourceDocType = doc.OrigDocType;
				postR.SourceRefNbr = doc.OrigRefNbr;
				postR.Type = ARTranPost.type.RetainageReverse;
				postR.CuryRetainageAmt = doc.CuryRetainageTotal;
				postR.RetainageAmt = doc.RetainageTotal;
				TranPost.Insert(postR);	
				postR.DocType = doc.OrigDocType;
				postR.RefNbr = doc.OrigRefNbr;
				postR.SourceDocType = doc.DocType;
				postR.SourceRefNbr = doc.RefNbr;
				TranPost.Insert(postR);
			}
		}

		public virtual void ProcessOriginTranPost(ARPayment doc)
		{
			if (doc.DocType == ARDocType.CashSale || doc.DocType == ARDocType.CashReturn) return;
			ARTranPost post = CreateTranPost(doc);
			post.Type = ARTranPost.type.Origin;
			post.CuryAmt = doc.CuryOrigDocAmt;
			post.Amt = doc.OrigDocAmt;
			if(IsNeedUpdateHistoryForTransaction(post.FinPeriodID))
				TranPost.Insert(post);
		}
		
		public virtual void ProcessVoidWOTranPost(ARRegister doc, ARAdjust lastAdjustment)
		{
			ARTranPost post = CreateTranPost(doc);
			post.FinPeriodID = lastAdjustment.AdjgFinPeriodID;
			post.TranPeriodID = lastAdjustment.AdjgTranPeriodID;
			post.BatchNbr = lastAdjustment.AdjBatchNbr;
			post.DocDate = lastAdjustment.AdjgDocDate;
			post.Type = ARTranPost.type.Voided;
			post.CuryAmt = -doc.CuryOrigDocAmt;
			post.Amt = -doc.OrigDocAmt;
			if(IsNeedUpdateHistoryForTransaction(post.FinPeriodID))
				TranPost.Insert(post);
		}

		public virtual void ProcessVoidPaymentTranPost(ARRegister doc, Amount docBal)
		{
			ARTranPost docPost = CreateTranPost(doc);
			ARTranPost revPost = CreateTranPost(doc);
			docPost.DocType = doc.OrigDocType ?? GetHistTranType(doc.DocType, doc.RefNbr);
			docPost.RefNbr = doc.OrigRefNbr ?? doc.RefNbr;
			revPost.SourceDocType = docPost.DocType;
			revPost.SourceRefNbr = docPost.RefNbr;
			docPost.Type = revPost.Type = ARTranPost.type.Voided;
			docPost.CuryAmt = -docBal.Cury;
			docPost.Amt = -docBal.Base;
			revPost.CuryAmt = docBal.Cury;
			revPost.Amt = docBal.Base;
			if(docPost.DocType != null &&
				IsNeedUpdateHistoryForTransaction(docPost.FinPeriodID))
				TranPost.Insert(docPost);
			if(revPost.DocType != null &&
			   IsNeedUpdateHistoryForTransaction(docPost.FinPeriodID))
				TranPost.Insert(revPost);
		}

		public virtual void ProcessPrepaymentTranPost(ARInvoice doc)
		{
			ARTranPost post = CreateTranPost(doc);
			post.Type = ARTranPost.type.PrepaymentInvoice;
			post.AccountID = doc.PrepaymentAccountID;
			post.SubID = doc.PrepaymentSubID;
			post.CuryAmt = doc.CuryOrigDocAmt;
			post.Amt = doc.OrigDocAmt;
			if (IsNeedUpdateHistoryForTransaction(post.FinPeriodID))
				TranPost.Insert(post);
		}
		public virtual void ProcessRetainageTranPost(ARInvoice doc)
		{
			if (doc.PaymentsByLinesAllowed == true)
			{
				Dictionary<ARTranPostKey, ARTranPost> tranPosts = new Dictionary<ARTranPostKey, ARTranPost>();
				IEqualityComparer<ARTran> arTranComparer =
					new FieldSubsetEqualityComparer<ARTran>(
					ARTran_TranType_RefNbr.Cache,
					typeof(ARTran.tranType),
					typeof(ARTran.refNbr),
					typeof(ARTran.lineNbr));

				foreach (var group in ARTran_TranType_RefNbr.Select(doc.DocType, doc.RefNbr).AsEnumerable().GroupBy(row => (ARTran)row, arTranComparer))
				{
					ARTran line = group.Key;
					ARTranPostKey tranPostKey = new ARTranPostKey(line.OrigDocType, line.OrigRefNbr, 0);
					tranPosts.TryGetValue(tranPostKey, out ARTranPost existedTran);
					if (existedTran != null)
					{
						existedTran.CuryRetainageAmt -= line.CuryOrigTranAmt;
						existedTran.RetainageAmt -= line.OrigTranAmt;
					}
					else
					{
						ARTranPost newTran = CreateTranPost(doc);
						newTran.DocType = tranPostKey.DocType;
						newTran.RefNbr = tranPostKey.RefNbr;
						newTran.Type = ARTranPost.type.Retainage;
						newTran.CuryRetainageAmt = -line.CuryOrigTranAmt;
						newTran.RetainageAmt = -line.OrigTranAmt;
						tranPosts.Add(tranPostKey, newTran);
					}
				}

				foreach (KeyValuePair<ARTranPostKey, ARTranPost> kvp in tranPosts)
				{
					if (IsNeedUpdateHistoryForTransaction(kvp.Value.FinPeriodID))
						TranPost.Insert(kvp.Value);
				}
			}
			else
			{
				//we need to get any ARTran to use OrigRefNbr that's refer to original retainage document
				ARTran line = ARTran_TranType_RefNbr.Select(doc.DocType, doc.RefNbr).FirstOrDefault();
			ARTranPost post = CreateTranPost(doc);
				post.DocType = line.OrigDocType;
				post.RefNbr = line.OrigRefNbr;
			post.Type = ARTranPost.type.Retainage;
			post.CuryRetainageAmt = -doc.CuryOrigDocAmt;
			post.RetainageAmt = -doc.OrigDocAmt;
			if(IsNeedUpdateHistoryForTransaction(post.FinPeriodID))
				TranPost.Insert(post);
		}
		}

		public virtual void ProcessInstallmentTranPost(ARInvoice doc)
		{
			ARTranPost post = CreateTranPost(doc);
			post.Type = ARTranPost.type.Installment;
			post.CuryAmt = -doc.CuryOrigDocAmt;
			post.Amt = -doc.OrigDocAmt;
			if(IsNeedUpdateHistoryForTransaction(post.FinPeriodID))
				TranPost.Insert(post);
		}

		public virtual void ProcessAdjustmentTranPost(ARAdjust adj, ARRegister doc, ARRegister pmt, bool adjustedOnly)
		{
			ProcessAdjustmentTranPost(null, adj, doc, pmt, adjustedOnly);
		}

		public virtual void ProcessAdjustmentTranPost(ARTran tran, ARAdjust adj, ARRegister doc, ARRegister pmt, bool adjustedOnly = false)
		{
			ARTranPost adjd = new ARTranPost();
			ARTranPost adjg = new ARTranPost();
			adjd.Type = ARTranPost.type.Application;
			adjg.Type = ARTranPost.type.Adjustment;
			adjd.AdjNbr = adjg.AdjNbr = adj.AdjNbr; 
			adjd.RefNoteID = adjg.RefNoteID = adj.NoteID;
			adjd.DocType = adjg.SourceDocType = adj.AdjdDocType;
			adjd.RefNbr = adjg.SourceRefNbr = adj.AdjdRefNbr;
			adjd.ReferenceID = adjg.ReferenceID = adj.CustomerID ?? adj.AdjdCustomerID;
			adjd.IsMigratedRecord = adjg.IsMigratedRecord = adj.IsMigratedRecord;
			adjd.SourceDocType = adjg.DocType = adj.AdjgDocType;
			adjd.SourceRefNbr = adjg.RefNbr = adj.AdjgRefNbr;
			adjd.BatchNbr = adjg.BatchNbr = adj.AdjBatchNbr;
			adjd.LineNbr = adjg.LineNbr = adj.AdjdLineNbr;
			adjd.VoidAdjNbr = adjg.VoidAdjNbr = adj.VoidAdjNbr;

			//AdjD
			adjd.TranType = adjd.DocType == ARDocType.SmallCreditWO ? adjd.DocType : adjd.SourceDocType;
			adjd.TranRefNbr = adjd.DocType == ARDocType.SmallCreditWO ? adjd.RefNbr : adjd.SourceRefNbr;
			adjd.AccountID = adj.AdjdARAcct;
			adjd.SubID =  adj.AdjdARSub;
			adjd.BranchID = adj.AdjdBranchID;
			adjd.CustomerID = doc.CustomerID;
			FinPeriodIDAttribute.SetPeriodsByMaster<ARTranPost.finPeriodID>(TranPost.Cache, adjd, adj.AdjgTranPeriodID);

			adjd.DocDate = adj.AdjgDocDate;
			adjd.CuryInfoID = adj.AdjdCuryInfoID;
			adjd.CuryAmt = adj.CuryAdjdAmt;
			adjd.CuryPPDAmt = adj.AdjdDocType.IsNotIn(ARDocType.CashSale, ARDocType.CashReturn) ? adj.CuryAdjdPPDAmt : 0m;
			adjd.CuryDiscAmt = adj.AdjdDocType.IsNotIn(ARDocType.CashSale, ARDocType.CashReturn) ? adj.CuryAdjdDiscAmt : 0m;
			adjd.CuryWOAmt = adj.CuryAdjdWOAmt;
			adjd.Amt = adj.AdjAmt;
			adjd.PPDAmt = adj.AdjdDocType.IsNotIn(ARDocType.CashSale, ARDocType.CashReturn) ? adj.AdjPPDAmt : 0m;
			adjd.DiscAmt = adj.AdjdDocType.IsNotIn(ARDocType.CashSale, ARDocType.CashReturn) ? adj.AdjDiscAmt : 0m ;
			adjd.WOAmt = adj.AdjWOAmt;
			adjd.RGOLAmt = adj.RGOLAmt;
			//Adjg
			adjg.TranType = adj.IsOrigSmallCreditWOApp() ? adj.AdjdDocType : adj.AdjgDocType;
			adjg.TranRefNbr = adj.IsOrigSmallCreditWOApp() ? adj.AdjdRefNbr : adj.AdjgRefNbr;
			adjg.AccountID = pmt.IsPrepaymentInvoiceDocument() ? pmt.PrepaymentAccountID: pmt.ARAccountID;
			adjg.SubID = pmt.IsPrepaymentInvoiceDocument() ? pmt.PrepaymentSubID : pmt.ARSubID;
			adjg.BranchID = adj.AdjgBranchID;
			adjg.CustomerID = pmt.CustomerID;
			FinPeriodIDAttribute.SetPeriodsByMaster<ARTranPost.finPeriodID>(TranPost.Cache, adjg, adj.AdjgTranPeriodID);
			adjg.DocDate = adj.AdjgDocDate;
			adjg.CuryInfoID = adj.AdjgCuryInfoID;
			adjg.CuryAmt = adj.CuryAdjgAmt;
			adjg.CuryPPDAmt = adj.CuryAdjgPPDAmt;
			adjg.CuryDiscAmt = adj.CuryAdjgDiscAmt;
			adjg.CuryWOAmt = adj.CuryAdjgWOAmt;
			adjg.Amt = adj.AdjAmt;
			adjg.PPDAmt = adj.AdjPPDAmt;
			adjg.DiscAmt = adj.AdjDiscAmt;
			adjg.WOAmt = adj.AdjWOAmt;
			adjg.RGOLAmt = adj.RGOLAmt;

			if (doc.IsMigratedRecord == true &&
			    pmt.IsMigratedRecord == true)
			{
				adjd.TranType = ARDocType.CreditMemo;
				adjd.IsMigratedRecord = true;
			}
			adjd.IsVoidPrepayment = adjg.IsVoidPrepayment =
				GetHistTranType(adjg.TranType, adjg.TranRefNbr) == ARDocType.Prepayment;

			var customerID = CustomerIntegrityCheck?.BAccountID;
			if ((customerID == null || adjd.CustomerID == customerID) &&
			    IsNeedUpdateHistoryForTransaction(adjd.FinPeriodID))
				TranPost.Insert(adjd);

			if (!adjustedOnly &&
				(customerID == null || adjg.CustomerID == customerID) &&
			    IsNeedUpdateHistoryForTransaction(adjg.FinPeriodID))
			{
				if(!adjd.DocType.IsIn(ARDocType.CashSale,ARDocType.CashReturn))
				   TranPost.Insert(adjg);
				   
				ARTranPost rgol = (ARTranPost)TranPost.Cache.CreateCopy(adjg);
				rgol.BranchID = adj.IsOrigSmallCreditWOApp() ? pmt.BranchID : doc.BranchID;
				FinPeriodIDAttribute.SetPeriodsByMaster<ARTranPost.finPeriodID>(TranPost.Cache, rgol, rgol.TranPeriodID);

				rgol.Type = ARTranPost.type.RGOL;
				rgol.CuryInfoID = adj.AdjdCuryInfoID;
				rgol.CuryAmt = 0;
				rgol.Amt = 0;
				rgol.CuryPPDAmt = adj.CuryAdjdPPDAmt;
				rgol.CuryDiscAmt = adj.CuryAdjdDiscAmt;
				rgol.PPDAmt = adj.AdjPPDAmt;
				rgol.DiscAmt = adj.AdjDiscAmt;
				rgol.CuryWOAmt = adj.CuryAdjdWOAmt;
				rgol.WOAmt = adj.AdjWOAmt;
				rgol.TranType = adj.AdjgDocType;
				rgol.TranRefNbr = adj.AdjgRefNbr;
				rgol.AccountID = doc.ARAccountID;
				rgol.SubID = doc.ARSubID;
				TranPost.Insert(rgol);
				if (adj.AdjdDocType == ARDocType.SmallCreditWO)
				{
					ARTranPost srgol = (ARTranPost)TranPost.Cache.CreateCopy(rgol);
					srgol.AccountID = pmt.ARAccountID;
					srgol.SubID = pmt.ARSubID;
					srgol.Type = ARTranPost.type.Rounding;
					TranPost.Insert(srgol);
				}
			}

			if (doc.IsRetainageDocument == true)
			{
				ARRegister origRetainageDoc = null;
				if (tran?.OrigRefNbr != null && doc.PaymentsByLinesAllowed == true)
				{
					origRetainageDoc = GetOriginalRetainageDocument(tran);
				}
				else
				{
					origRetainageDoc = GetOriginalRetainageDocument(doc);
				}
				if (origRetainageDoc != null)
				{
					decimal? sign = origRetainageDoc.SignAmount * doc.SignAmount;
					decimal curyRetainagePaidAmt = adj.CuryAdjdAmt * sign ?? 0m;
					decimal retainagePaidAmt = adj.AdjAmt * sign ?? 0m;
					decimal curyRetainagePaidDiscount = adj.CuryAdjdDiscAmt * sign ?? 0m;
					decimal retainagePaidDiscount = adj.AdjDiscAmt * sign ?? 0m;
					decimal curyRetainagePaidWriteOff = adj.CuryAdjdWOAmt * sign ?? 0m;
					decimal retainagePaidWriteOff = adj.AdjWOAmt * sign ?? 0m;

					if (tran?.OrigRefNbr != null && doc.PaymentsByLinesAllowed == true
							|| pmt.IsRetainageReversing != true && doc.IsRetainageReversing != true)
					{
						origRetainageDoc.CuryRetainageUnpaidTotal -= curyRetainagePaidAmt + curyRetainagePaidDiscount + curyRetainagePaidWriteOff;
						origRetainageDoc.RetainageUnpaidTotal -= retainagePaidAmt + retainagePaidDiscount + retainagePaidWriteOff;
						ARDocument.Cache.Update(origRetainageDoc);

						if (doc.IsMigratedRecord == true)
							return;

						ARTranPostKey tranPostKey = doc.PaymentsByLinesAllowed == true ?
							new ARTranPostKey(tran.OrigDocType, tran.OrigRefNbr, 0) :
							new ARTranPostKey(doc.OrigDocType, doc.OrigRefNbr, 0);
						tranPostRetainagePayments.TryGetValue(tranPostKey, out ARTranPost existedTran);
						if (existedTran != null)
						{
							existedTran.CuryAmt += curyRetainagePaidAmt;
							existedTran.Amt += retainagePaidAmt;

							existedTran.CuryDiscAmt += curyRetainagePaidDiscount;
							existedTran.DiscAmt += retainagePaidDiscount;

							existedTran.CuryWOAmt += curyRetainagePaidWriteOff;
							existedTran.WOAmt += retainagePaidWriteOff;
						}
						else
						{
							ARTranPost newTran = CreateTranPost(doc);
							newTran.SourceDocType = pmt.DocType;
							newTran.SourceRefNbr = pmt.RefNbr;
							newTran.DocType = tranPostKey.DocType;
							newTran.RefNbr = tranPostKey.RefNbr;
							newTran.Type = ARTranPost.type.RetainagePayment;

							newTran.CuryAmt = curyRetainagePaidAmt;
							newTran.Amt = retainagePaidAmt;

							newTran.CuryDiscAmt = curyRetainagePaidDiscount;
							newTran.DiscAmt = retainagePaidDiscount;

							newTran.CuryWOAmt = curyRetainagePaidWriteOff;
							newTran.WOAmt = retainagePaidWriteOff;

							tranPostRetainagePayments.Add(tranPostKey, newTran);
						}
					}
				}
			}
		}

		protected virtual void ProcessRGOLTranPost(ARTranPost adjg)
		{
			
		}

		protected virtual ARTranPost CreateTranPost(ARRegister doc)
		{
			ARTranPost post = new ARTranPost();
			post.RefNoteID = doc.NoteID;
			post.TranType = post.DocType = post.SourceDocType = doc.DocType;
			post.TranRefNbr = post.RefNbr = post.SourceRefNbr = doc.RefNbr;
			post.ReferenceID = post.CustomerID = doc.CustomerID;
			post.FinPeriodID = doc.FinPeriodID;
			post.TranPeriodID = doc.TranPeriodID;
			post.AccountID = doc.ARAccountID;
			post.SubID = doc.ARSubID;
			post.BranchID = doc.BranchID;
			post.BatchNbr = doc.BatchNbr;
			post.DocDate = doc.DocDate;
			post.CuryInfoID = doc.CuryInfoID;
			post.IsMigratedRecord = doc.IsMigratedRecord;
			post.IsVoidPrepayment = GetHistTranType(post.TranType, post.TranRefNbr) == ARDocType.Prepayment;
			return post;
		}

		private bool IsVoidPrepayment(ARRegister pmt)
		{
			if (pmt.DocType != ARDocType.VoidPayment) return false;
			switch (pmt.OrigDocType)
			{
				case ARDocType.Prepayment:
					return true;
				case ARDocType.Payment:
					return false;
				default:
					return
						PXSelect<ARRegister,
								Where<ARRegister.refNbr, Equal<Required<ARRegister.refNbr>>,
									And<ARRegister.docType, Equal<Required<ARRegister.docType>>>>>
							.Select(this, pmt.RefNbr, ARDocType.Prepayment)
							.Any();
			}
		}

		/// <summary>
		/// Returns account id for trade discount transaction
		/// </summary>
		/// <param name="sotype">Order type of linked SO Order</param>
		/// <param name="origTran">Original <see cref="ARTran">ARTran</see> record, that has line level discount applied to it. Required for customization</param>
		/// <param name="newTran">New <see cref="ARTran">ARTran</see> record, that is being created for trade discount</param>
		/// <param name="ardoc"> <see cref="ARTran">ARInvoice</see> record that is being released</param>
		/// <returns>Account ID</returns>
		public virtual int? GetDefaultTradeDiscountAccountID(SOOrderType sotype, ARTran origTran, ARTran newTran, ARInvoice ardoc)
		{
			switch (sotype.DiscAcctDefault)
			{
				case SO.SODiscAcctSubDefault.OrderType:
					return sotype.DiscountAcctID;
				case SO.SODiscAcctSubDefault.MaskLocation:
				{
					Location customerloc = PXSelect<Location,
							Where<Location.bAccountID, Equal<Required<ARInvoice.customerID>>,
								And<Location.locationID, Equal<Required<ARInvoice.customerLocationID>>>>>
						.Select(this, ardoc.CustomerID, ardoc.CustomerLocationID);
					if (customerloc != null)
					{
						if (customerloc.CDiscountAcctID != null)
							return customerloc.CDiscountAcctID;
						else
						{
							if (PXAccess.FeatureInstalled<FeaturesSet.accountLocations>())
								throw new PXException(IN.Messages.DiscountAccountIsNotSetupLocation, customerloc.LocationCD, Caches[typeof(ARInvoice)].GetValueExt<ARInvoice.customerID>(ardoc).ToString());
							else
								throw new PXException(IN.Messages.DiscountAccountIsNotSetupCustomer, Caches[typeof(ARInvoice)].GetValueExt<ARInvoice.customerID>(ardoc).ToString());
						}
					}
					else
						return null;
				}
				default:
					return null;
			}
		}

		/// <summary>
		/// Extension point for AR Release Invoice process. This method is called after GL Batch was created and all main GL Transactions have been
		/// inserted, but before Invoice rounding transaction, or RGOL transaction has been inserted. 
		/// </summary>
		/// <param name="je">Journal Entry graph used for posting</param>
		/// <param name="ardoc">Original AR Invoice</param>
		/// <param name="arbatch">GL Batch that was created for Invoice</param>
		public virtual void ReleaseInvoiceBatchPostProcessing(JournalEntry je, ARInvoice ardoc, Batch arbatch)
		{

		}

		[Obsolete(Common.Messages.MethodIsObsoleteRemoveInLaterAcumaticaVersions)]
		public virtual void ReleaseInvoiceTransactionPostProcessing(JournalEntry je, ARInvoice ardoc, PXResult<ARTran, ARTax, Tax, DRDeferredCode, SOOrderType> r, GLTran tran)
		{
		}

		/// <summary>
		/// Extension point for AR Release Invoice process. This method is called after transaction amounts have been calculated, but before it was inserted.
		/// </summary>
		/// <param name="je">Journal Entry graph used for posting</param>
		/// <param name="ardoc">Original AR Invoice</param>
		/// <param name="r">Document line with joined supporting entities</param>
		/// <param name="tran">Transaction that was created for ARTran. This transaction has not been inserted yet.</param>
		public virtual void ReleaseInvoiceTransactionPostProcessing(JournalEntry je, ARInvoice ardoc, PXResult<ARTran, ARTax, Tax, DRDeferredCode, SOOrderType, ARTaxTran> r, GLTran tran)
		{
			var oldResult = new PXResult<ARTran, ARTax, Tax, DRDeferredCode, SOOrderType>(r, r, r, r, r);
			ReleaseInvoiceTransactionPostProcessing(je, ardoc, oldResult, tran);
		}

		/// <summary>
		/// Extension point for AR Release Invoice process. This method is called after transaction was inserted.
		/// </summary>
		/// <param name="je">Journal Entry graph used for posting</param>
		/// <param name="ardoc">Original AR Invoice</param>
		/// <param name="n">Document line</param>
		public virtual void ReleaseInvoiceTransactionPostProcessed(JournalEntry je, ARInvoice ardoc, ARTran n)
		{
		}

		private string LocalizeForCustomer(Customer customer, string message)
		{
			using (new PXLocaleScope(customer.LocaleName))
				return PXMessages.LocalizeNoPrefix(message);
		}

		private ARTran CreateDiscountTran(ARTran originalTran, decimal curyAmount, decimal baseAmount, string tranDescr, ARRegister parentDocument)
		{
			ARTran new_tran = PXCache<ARTran>.CreateCopy(originalTran);
			new_tran.InventoryID = null;
			new_tran.SubItemID = null;
			new_tran.SOOrderSortOrder = null;
			new_tran.TaxCategoryID = null;
			new_tran.SalesPersonID = null;
			new_tran.UOM = null;
			new_tran.LineType = SO.SOLineType.Discount;
			new_tran.TranDesc = tranDescr;
			new_tran.LineNbr = (int?)PXLineNbrAttribute.NewLineNbr<ARTran.lineNbr>(ARTran_TranType_RefNbr.Cache, parentDocument);
			new_tran.CuryTranAmt = new_tran.CuryExtPrice = curyAmount;
			new_tran.TranAmt = new_tran.ExtPrice = baseAmount;
			new_tran.CuryDiscAmt = 0m;
			new_tran.Qty = 0m;
			new_tran.DiscPct = 0m;
			new_tran.CuryUnitPrice = 0m;
			new_tran.DiscountID = null;
			new_tran.DiscountSequenceID = null;
			new_tran.NoteID = null;
			new_tran.CuryAccruedCost = 0m;
			new_tran.AccruedCost = 0m;
			new_tran.AccrueCost = false;
			return new_tran;
		}

		/// <summary>
		/// The method to process discount for each invoice detail inside the
		/// <see cref="ReleaseDocProc(JournalEntry, ref ARRegister, PXResult{ARInvoice, CurrencyInfo, Terms, Customer, Account}, out PMRegister)"/> method,
		/// depending on <see cref="SOOrderType.postLineDiscSeparately"/> flag
		/// and <see cref="ARTran.curyDiscAmt"/> amount.
		/// </summary>
		public virtual void ProcessInvoiceDetailDiscount(
			ARRegister doc, 
			Customer customer, 
			ARTran tran, 
			SOOrderType sotype,
			ARInvoice ardoc)
		{
			bool processDiscount =
				sotype.PostLineDiscSeparately == true &&
				sotype.DiscountAcctID != null &&
				tran.CuryDiscAmt > 0.00005m;

			if (!processDiscount) return;

			CurrencyInfo TranCurrencyInfo = InvoiceEntryGraph.GetExtension<ARInvoiceEntry.MultiCurrency>().GetCurrencyInfo(tran.CuryInfoID);

			decimal taxRate = tran.TaxableAmt != 0m ? (decimal)tran.CuryTaxAmt / (decimal)tran.CuryTaxableAmt : 0m;
			decimal curyAmount = TranCurrencyInfo.RoundCury((decimal)tran.CuryDiscAmt / (1 + taxRate));
			decimal baseAmount = TranCurrencyInfo.CuryConvBase(curyAmount);

			string tranDescription = LocalizeForCustomer(customer, SO.Messages.LineDiscDescr);
			ARTran_TranType_RefNbr.Cache.Insert(
				CreateDiscountTran(tran, curyAmount, baseAmount, tranDescription, doc));


			ARTran new_tran = CreateDiscountTran(tran, -curyAmount, -baseAmount, tranDescription, doc);
			new_tran.AccountID = GetDefaultTradeDiscountAccountID(sotype, tran, new_tran, ardoc);

			if (sotype.UseDiscountSubFromSalesSub == false)
			{
				Location customerloc = PXSelect<Location,
					Where<Location.bAccountID, Equal<Required<ARInvoice.customerID>>,
						And<Location.locationID, Equal<Required<ARInvoice.customerLocationID>>>>>
					.Select(this, doc.CustomerID, doc.CustomerLocationID);

				CRLocation companyloc = PXSelectJoin<CRLocation,
					InnerJoin<BAccountR, On<CRLocation.bAccountID, Equal<BAccountR.bAccountID>,
						And<CRLocation.locationID, Equal<BAccountR.defLocationID>>>,
					InnerJoin<Branch, On<Branch.bAccountID, Equal<BAccountR.bAccountID>>>>,
					Where<Branch.branchID, Equal<Required<ARRegister.branchID>>>>
					.Select(this, doc.BranchID);

				object ordertype_SubID = GetValue<SO.SOOrderType.discountSubID>(sotype);
				object customer_Location = GetValue<Location.cDiscountSubID>(customerloc);
				object company_Location = GetValue<CRLocation.cMPDiscountSubID>(companyloc);

				//if (customer_Location != null && company_Location != null)
				{
					object value = SO.SODiscSubAccountMaskAttribute.MakeSub<SO.SOOrderType.discSubMask>(this,
						sotype.DiscSubMask,
						new object[]
						{
								ordertype_SubID,
								customer_Location,
								company_Location
						},
						new Type[]
						{
								typeof(SO.SOOrderType.discountSubID),
								typeof(Location.cDiscountSubID),
								typeof(Location.cMPDiscountSubID)
						});

					ARTran_TranType_RefNbr.Cache.RaiseFieldUpdating<ARTran.subID>(new_tran, ref value);
					new_tran.SubID = (int?)value;
				}
			}

			ARTran_TranType_RefNbr.Cache.Insert(new_tran);
		}

		public class Amount : Tuple<decimal?, decimal?>
		{
			public decimal? Cury => Item1;
			public decimal? Base => Item2;

			public Amount() : base(0m, 0m)
			{
			}

			public Amount(decimal? cury, decimal? baaase)
				: base(cury, baaase)
			{

			}

			public static Amount operator +(Amount a, Amount b)
			{
				return new Amount(a.Cury + b.Cury, a.Base + b.Base);
			}

			public static Amount operator -(Amount a, Amount b)
			{
				return new Amount(a.Cury - b.Cury, a.Base - b.Base);
			}

			public static Amount operator *(Amount a, decimal? mult)
			{
				return new Amount(a.Cury * mult, a.Base * mult);
			}

			public static Amount operator /(Amount a, decimal? mult)
			{
				return new Amount(a.Cury / mult, a.Base / mult);
			}

			public static Amount operator *(Amount a, decimal mult)
				=> new Amount(a.Cury * mult, a.Base * mult);

			public static Amount operator /(Amount a, decimal mult)
				=> new Amount(a.Cury / mult, a.Base / mult);

			public static bool operator <(Amount a, Amount b)
				=> a.Cury < b.Cury;

			public static bool operator >(Amount a, Amount b)
				=> a.Cury > b.Cury;

			public override bool Equals(object obj)
				=> Equals(obj as Amount);

			public bool Equals(Amount a)
				=> a?.Cury == this.Cury && a.Base == this.Base;

			public override int GetHashCode()
				=> Cury?.GetHashCode() ?? 0;
		}

		protected virtual void PostRetainedTax(
			JournalEntry je,
			ARInvoice ardoc,
			GLTran origTran,
			ARTaxTran x,
			Tax salestax)
		{
			bool isReversedTax = salestax.ReverseTax == true;
			bool isUseTax = salestax.TaxType == CSTaxType.Use;

			bool isOriginalRetainageDocumentWithTax = ardoc.IsOriginalRetainageDocument() && x.CuryRetainedTaxAmt != 0m && !isUseTax;
			bool isChildRetainageDocumentWithTax = ardoc.IsRetainageDocument == true && x.CuryTaxAmt != 0m && !isUseTax;

			if (isOriginalRetainageDocumentWithTax || isChildRetainageDocumentWithTax)
			{
				RetainageTaxCheck(salestax);
			}

			if (isOriginalRetainageDocumentWithTax)
			{
				bool isDebit = (ardoc.DrCr == DrCr.Debit);

				GLTran retainedTaxTran = PXCache<GLTran>.CreateCopy(origTran);
				retainedTaxTran.ReclassificationProhibited = true;
				retainedTaxTran.AccountID = salestax.RetainageTaxPayableAcctID;
				retainedTaxTran.SubID = salestax.RetainageTaxPayableSubID;
				retainedTaxTran.CuryDebitAmt = isDebit ? x.CuryRetainedTaxAmt : 0m;
				retainedTaxTran.DebitAmt = isDebit ? x.RetainedTaxAmt : 0m;
				retainedTaxTran.CuryCreditAmt = isDebit ? 0m : x.CuryRetainedTaxAmt;
				retainedTaxTran.CreditAmt = isDebit ? 0m : x.RetainedTaxAmt;
				je.GLTranModuleBatNbr.Insert(retainedTaxTran);
			}
			else if (isChildRetainageDocumentWithTax)
			{
				GLTran retainedTaxTran = PXCache<GLTran>.CreateCopy(origTran);
				retainedTaxTran.ReclassificationProhibited = true;
				retainedTaxTran.AccountID = salestax.RetainageTaxPayableAcctID;
				retainedTaxTran.SubID = salestax.RetainageTaxPayableSubID;
				retainedTaxTran.CuryDebitAmt = origTran.CuryCreditAmt;
				retainedTaxTran.DebitAmt = origTran.CreditAmt;
				retainedTaxTran.CuryCreditAmt = origTran.CuryDebitAmt;
				retainedTaxTran.CreditAmt = origTran.DebitAmt;
				je.GLTranModuleBatNbr.Insert(retainedTaxTran);

				GLTran retainageTran = PXCache<GLTran>.CreateCopy(origTran);
				retainedTaxTran.ReclassificationProhibited = true;
				retainageTran.SummPost = true;
				retainageTran.AccountID = ardoc.RetainageAcctID;
				retainageTran.SubID = ardoc.RetainageSubID;
				retainageTran.CuryDebitAmt = origTran.CuryDebitAmt;
				retainageTran.DebitAmt = origTran.DebitAmt;
				retainageTran.CuryCreditAmt = origTran.CuryCreditAmt;
				retainageTran.CreditAmt = origTran.CreditAmt;
				je.GLTranModuleBatNbr.Insert(retainageTran);
			}
		}

		protected virtual void RetainageTaxCheck(Tax tax)
		{
			TaxAccountCheck<Tax.retainageTaxPayableAcctID>(tax);
			TaxAccountCheck<Tax.retainageTaxPayableSubID>(tax);
		}

		private void TaxAccountCheck<Field>(Tax tax)
			where Field : IBqlField
		{
			Type table = BqlCommand.GetItemType(typeof(Field));
			var account = Caches[table].GetValue(tax, typeof(Field).Name);
			if (account == null)
			{
				throw new ReleaseException(
					AR.Messages.TaxAccountNotFound,
					PXUIFieldAttribute.GetDisplayName<Field>(Caches[typeof(Tax)]), tax.TaxID);
			}
		}

		public static Amount GetSalesPostingAmount(PXGraph graph, ARTran documentLine)
		{
			var documentLineWithTaxes = new PXSelectJoin<
				ARTran,
					LeftJoin<ARTax,
						On<ARTax.tranType, Equal<ARTran.tranType>,
						And<ARTax.refNbr, Equal<ARTran.refNbr>,
						And<ARTax.lineNbr, Equal<ARTran.lineNbr>>>>,
					LeftJoin<Tax,
						On<Tax.taxID, Equal<ARTax.taxID>>,
					LeftJoin<ARRegister,
						On<ARRegister.docType, Equal<ARTran.tranType>,
						And<ARRegister.refNbr, Equal<ARTran.refNbr>>>>>>,
				Where<
					ARTran.tranType, Equal<Required<ARTran.tranType>>,
					And<ARTran.refNbr, Equal<Required<ARTran.refNbr>>,
					And<ARTran.lineNbr, Equal<Required<ARTran.lineNbr>>>>>>
				(graph);

			PXResult<ARTran, ARTax, Tax, ARRegister> queryResult =
				documentLineWithTaxes
					.Select(documentLine.TranType, documentLine.RefNbr, documentLine.LineNbr).AsEnumerable()
					.Cast<PXResult<ARTran, ARTax, Tax, ARRegister>>()
					.First();

			Func<decimal, decimal> roundingFunction = amount => 
				CM.PXDBCurrencyAttribute.Round(
					graph.Caches[typeof(ARTran)], 
					documentLine, 
					amount, 
					CM.CMPrecision.TRANCURY);

			return GetSalesPostingAmount(graph, queryResult, documentLine, queryResult, queryResult, roundingFunction);
		}

		/// <summary>
		/// Gets the amount to be posted to the sales acount 
		/// for the given document line.
		/// </summary>
		[Obsolete(Common.Messages.MethodIsObsoleteRemoveInLaterAcumaticaVersions)]
		public static Amount GetSalesPostingAmount(
			PXGraph graph,
			ARRegister document, 
			ARTran documentLine, 
			ARTax lineTax, 
			Tax salesTax, 
			Func<decimal, decimal> round)
		{
			return GetSalesPostingAmount(graph, document, documentLine, lineTax, salesTax, round, round);
		}

		/// <summary>
		/// Gets the amount to be posted to the sales acount 
		/// for the given document line.
		/// </summary>
		public static Amount GetSalesPostingAmount(
			PXGraph graph,
			ARRegister document,
			ARTran documentLine,
			ARTax lineTax,
			Tax salesTax,
			Func<decimal, decimal> roundCury,
			Func<decimal, decimal> roundBase)
		{
			bool postedPPD = document.PendingPPD == true && document.DocType == ARDocType.CreditMemo;
			if (!postedPPD &&
				lineTax != null &&
				lineTax.TaxID == null &&
				document.DocType == ARDocType.DebitMemo &&
				document.OrigDocType == ARDocType.CreditMemo &&
				document.OrigRefNbr != null)
			{
				// Same logic for the DebitMemo, reversed from the PPD CreditMemo
				// -
				postedPPD = PXSelect<
					ARRegister,
					Where<
						ARRegister.refNbr, Equal<Required<ARRegister.refNbr>>,
						And<ARRegister.docType, Equal<Required<ARRegister.docType>>,
						And<ARRegister.pendingPPD, Equal<True>>>>>
					.SelectSingleBound(graph, null, document.OrigRefNbr, document.OrigDocType).Count > 0;
			}

			bool applyNetRevenue = PXAccess.FeatureInstalled<FeaturesSet.aSC606>() && documentLine.InventoryID != null && documentLine.DeferredCode != null;
			bool isInclusiveTax = salesTax?.TaxCalcLevel == CSTaxCalcLevel.Inclusive;

			if (applyNetRevenue)
			{
				if (!postedPPD && 
					lineTax?.TaxID != null && 
					(isInclusiveTax || document.TaxCalcMode == TX.TaxCalculationMode.Gross))
				{
					return new Amount(lineTax.CuryTaxableAmt, lineTax.TaxableAmt);
				}

				decimal origCuryDiscountAmount = roundCury((decimal)(documentLine.CuryTranAmt * documentLine.OrigGroupDiscountRate * documentLine.OrigDocumentDiscountRate));
				decimal origBaseDiscountAmount = roundBase((decimal)(documentLine.TranAmt * documentLine.OrigGroupDiscountRate * documentLine.OrigDocumentDiscountRate));

				var currentCuryDiscountAmount = roundCury((decimal)(documentLine.CuryTranAmt * documentLine.DocumentDiscountRate * documentLine.GroupDiscountRate));
				var currentBaseDiscountAmount = roundBase((decimal)(documentLine.TranAmt * documentLine.DocumentDiscountRate * documentLine.GroupDiscountRate));

				decimal? curyAmt = (documentLine.CuryTaxableAmt == 0m) ? (currentCuryDiscountAmount + origCuryDiscountAmount - documentLine.CuryTranAmt) : documentLine.CuryTaxableAmt;
				decimal? baseAmt = (documentLine.TaxableAmt == 0m) ? (currentBaseDiscountAmount + origBaseDiscountAmount - documentLine.TranAmt) : documentLine.TaxableAmt;

				return new Amount(curyAmt, baseAmt);
			}

			if (!postedPPD &&
				lineTax?.TaxID != null &&
				(isInclusiveTax || document.TaxCalcMode == TX.TaxCalculationMode.Gross))
			{
				if (salesTax.TaxType == CSTaxType.PerUnit)
				{
					return new Amount(documentLine.CuryTaxableAmt, documentLine.TaxableAmt);
				}
				else
				{
					decimal? curyAddUp =
					(documentLine.CuryTranAmt
					- roundCury((decimal)(documentLine.CuryTranAmt * documentLine.OrigGroupDiscountRate * documentLine.OrigDocumentDiscountRate)))
					+ (documentLine.CuryTranAmt
					- roundCury((decimal)(documentLine.CuryTranAmt * documentLine.GroupDiscountRate * documentLine.DocumentDiscountRate)));

					decimal? addUp =
						(documentLine.TranAmt
						- roundBase((decimal)(documentLine.TranAmt * documentLine.OrigGroupDiscountRate * documentLine.OrigDocumentDiscountRate)))
						+ (documentLine.TranAmt
						- roundBase((decimal)(documentLine.TranAmt * documentLine.GroupDiscountRate * documentLine.DocumentDiscountRate)));

					return new Amount(
						lineTax.CuryTaxableAmt + lineTax.CuryExemptedAmt + (lineTax.CuryRetainedTaxableAmt ?? 0m) + curyAddUp + lineTax.CuryTaxableDiscountAmt,
						lineTax.TaxableAmt + lineTax.ExemptedAmt + (lineTax.RetainedTaxableAmt ?? 0m) + addUp + lineTax.TaxableDiscountAmt);			
				}
			}

			return postedPPD ? 
				new Amount(documentLine.CuryTaxableAmt, documentLine.TaxableAmt) : 
				new Amount(documentLine.CuryTranAmt + (documentLine.CuryRetainageAmt ?? 0m),
					documentLine.TranAmt + (documentLine.RetainageAmt ?? 0m));
		}

		protected virtual void VerifyRoundingAllowed(ARInvoice document, Batch batch, string baseCuryID)
		{
			VerifyRoundingAllowed(document, batch, baseCuryID, 0);
		}

		[Obsolete(Common.Messages.MethodIsObsoleteAndWillBeRemoved2020R2)]
		protected virtual void VerifyRoundingAllowed(ARInvoice document, Batch batch, string baseCuryID, decimal taxesDiff)
		{
			bool useCurrencyPrecision = false;
			CM.Currency currency = PXSelect<CM.Currency, Where<CM.Currency.curyID, Equal<Required<ARInvoice.curyID>>>>.Select(this, document.CuryID);

			decimal roundDiff = Math.Abs(Math.Round((decimal)(batch.DebitTotal - batch.CreditTotal), 4));

			if (currency.UseARPreferencesSettings == true)
			{
				useCurrencyPrecision = this.InvoiceRounding == RoundingType.Currency;
			}
			else
			{
				useCurrencyPrecision = currency.ARInvoiceRounding == RoundingType.Currency;
			}

			if (useCurrencyPrecision)
			{
				throw new PXException(Messages.DocumentOutOfBalance);
			}

			if (roundDiff > CM.CurrencyCollection.GetCurrency(baseCuryID).RoundingLimit)
			{
				throw new PXException(AP.Messages.RoundingAmountTooBig, baseCuryID, roundDiff,
					IN.PXDBQuantityAttribute.Round(CM.CurrencyCollection.GetCurrency(baseCuryID).RoundingLimit));
			}
		}

		protected object GetValue<Field>(object data)
			where Field : IBqlField
		{
			return this.Caches[typeof(Field).DeclaringType].GetValue(data, typeof(Field).Name);
		}

		public int? GetValueInt<SourceField>(PXGraph target, object item)
			where SourceField : IBqlField
		{
			PXCache source = this.Caches[typeof(SourceField).DeclaringType];
			PXCache dest = target.Caches[typeof(SourceField).DeclaringType];

			object value = source.GetValueExt<SourceField>(item);
			if (value is PXFieldState)
			{
				value = ((PXFieldState)value).Value;
			}

			if (value != null)
			{
				dest.RaiseFieldUpdating<SourceField>(item, ref value);
			}

			return (int?)value;
		}

		public static void UpdateARBalances(PXGraph graph, ARRegister ardoc, decimal? BalanceAmt)
		{
			decimal? signedBalanceAmt = ardoc.SignBalance == 1m ? BalanceAmt : -BalanceAmt;

			// Voided payment is both released and voided.
			// Voided invoice (previously scheduled) is not released and voided.
			//
			if (ardoc.Released == true && 
				ardoc.Voided != true &&
				ardoc.SignBalance != 0m)
			{
				UpdateARBalances(graph, ardoc, signedBalanceAmt, 0m);
			}
			else if (ardoc.Hold != true && 
				ardoc.Scheduled != true && 
				ardoc.Voided != true && 
				ardoc.SignBalance != 0m)
			{
				UpdateARBalances(graph, ardoc, 0m, signedBalanceAmt);
			}
		}

		public static void UpdateARBalancesLastDocDate(ARBalances arbal, DateTime? date)
		{
			arbal.StatementRequired = true;
			if (arbal.LastDocDate == null || arbal.LastDocDate < date)
				{
					arbal.LastDocDate = date;
				}
			}

		public static void UpdateARBalances(PXGraph graph, ARRegister ardoc, decimal? CurrentBal, decimal? UnreleasedBal)
		{
			if (ardoc.CustomerID == null && ardoc.CustomerLocationID == null) return;

			ARBalances arbal = (ARBalances)graph.Caches[typeof(ARBalances)].Insert(new ARBalances
			{
				BranchID = ardoc.BranchID,
				CustomerID = ardoc.CustomerID,
				CustomerLocationID = ardoc.CustomerLocationID
			});

				arbal.CurrentBal += CurrentBal;
				arbal.UnreleasedBal += UnreleasedBal;
				if (ardoc.Released == true)
					UpdateARBalancesLastDocDate(arbal, ardoc.DocDate);
			}

		public static void UpdateARBalances(PXGraph graph, ARInvoice ardoc, decimal? BalanceAmt)
		{
			decimal? signedBalanceAmt = ardoc.SignBalance == 1m ? BalanceAmt : -BalanceAmt;

			// Voided payment is both released and voided
			// Voided invoice(previously scheduled) is not released and voided
			//
			if (ardoc.Released == true && 
				ardoc.Voided != true && 
				ardoc.SignBalance != 0m)
			{
				UpdateARBalances(graph, ardoc, signedBalanceAmt, 0m);
			}
			else if (ardoc.Hold != true && 
				ardoc.PendingProcessing != true &&
				ardoc.CreditHold != true && 
				ardoc.Scheduled != true && 
				ardoc.Voided != true && 
				ardoc.SignBalance != 0m)
			{
				UpdateARBalances(graph, ardoc, 0m, signedBalanceAmt);
			}
		}

		public static void UpdateARBalances(PXGraph graph, SOOrder order, decimal? UnbilledAmount, decimal? UnshippedAmount)
		{
			if (order.CustomerID != null && order.CustomerLocationID != null)
			{
				ARBalances arbal = new ARBalances();
				arbal.BranchID = order.BranchID;
				arbal.CustomerID = order.CustomerID;
				arbal.CustomerLocationID = order.CustomerLocationID;
				arbal = (ARBalances)graph.Caches[typeof(ARBalances)].Insert(arbal);

				if (ARDocType.SignBalance(order.ARDocType) != 0m && ARDocType.SignBalance(order.ARDocType) != null)
				{
					decimal? BalanceAmt;
					if (order.ShipmentCntr == 0)
					{
						BalanceAmt = UnbilledAmount;
					}
					else
					{
						BalanceAmt = UnshippedAmount;
						arbal.TotalOpenOrders += ARDocType.SignBalance(order.ARDocType) == 1m ? (UnbilledAmount - UnshippedAmount) : -(UnbilledAmount - UnshippedAmount);
					}

					//don't check field 'Completed' here it will cause incorrect calculation of 'Open Order Balance'
					if (order.InclCustOpenOrders == true && order.Cancelled == false && order.Hold == false && order.CreditHold == false)
					{
						arbal.TotalOpenOrders += ARDocType.SignBalance(order.ARDocType) == 1m ? BalanceAmt : -BalanceAmt;
					}
				}
			}
		}

		private void UpdateARBalances(ARAdjust adj, ARRegister ardoc)
		{
			// skip balance update for SmallCreditWO voiding application
			if (ardoc.DocType == ARDocType.SmallCreditWO && adj.Voided == true && adj.VoidAdjNbr != null)
			{
				return;
			}

			if (string.Equals(ardoc.DocType, adj.AdjdDocType) && 
				string.Equals(ardoc.RefNbr, adj.AdjdRefNbr, StringComparison.OrdinalIgnoreCase))
			{
				if (ardoc.CustomerID != null && ardoc.CustomerLocationID != null)
				{
					ARBalances arbal = new ARBalances();
					arbal.BranchID = ardoc.BranchID;
					arbal.CustomerID = ardoc.CustomerID;
					arbal.CustomerLocationID = ardoc.CustomerLocationID;
					arbal = (ARBalances)Caches[typeof(ARBalances)].Insert(arbal);

					arbal.CurrentBal += adj.AdjdTBSign * (adj.AdjAmt + adj.AdjDiscAmt + adj.AdjWOAmt) - adj.RGOLAmt;
					if (ardoc.Released == true)
						UpdateARBalancesLastDocDate(arbal, adj.AdjgDocDate);
				}
			}
			else if (string.Equals(ardoc.DocType, adj.AdjgDocType) && 
				string.Equals(ardoc.RefNbr, adj.AdjgRefNbr, StringComparison.OrdinalIgnoreCase))
			{
				if (ardoc.CustomerID != null && ardoc.CustomerLocationID != null)
				{
					ARBalances arbal = new ARBalances();
					arbal.BranchID = ardoc.BranchID;
					arbal.CustomerID = ardoc.CustomerID;
					arbal.CustomerLocationID = ardoc.CustomerLocationID;
					arbal = (ARBalances)Caches[typeof(ARBalances)].Insert(arbal);

					arbal.CurrentBal += adj.AdjgTBSign * adj.AdjAmt;
					if (ardoc.Released == true)
						UpdateARBalancesLastDocDate(arbal, adj.AdjgDocDate);
				}
			}
			else
			{
				throw new PXException(Messages.AdjustRefersNonExistentDocument, adj.AdjgDocType, adj.AdjgRefNbr, adj.AdjdDocType, adj.AdjdRefNbr);
			}
		}

		private void UpdateARBalances(ARRegister ardoc)
		{
			UpdateARBalances(this, ardoc, ardoc.OrigDocAmt);
		}

		private void UpdateARBalances(ARRegister ardoc, decimal? BalanceAmt)
		{
			UpdateARBalances(this, ardoc, BalanceAmt);
		}

		public virtual void VoidOrigAdjustment(ARAdjust adj)
		{
			string[] docTypes = ARPaymentType.GetVoidedARDocType(adj.AdjgDocType);
			if (docTypes.Length == 0)
			{
				docTypes = new string[] { adj.AdjgDocType };
			}
			ARAdjust voidadj = PXSelect<ARAdjust, Where<ARAdjust.adjgDocType, In<Required<ARAdjust.adjgDocType>>,
				And<ARAdjust.adjgRefNbr, Equal<Required<ARAdjust.adjgRefNbr>>,
				And<ARAdjust.adjdDocType, Equal<Required<ARAdjust.adjdDocType>>,
				And<ARAdjust.adjdRefNbr, Equal<Required<ARAdjust.adjdRefNbr>>,
				And<ARAdjust.adjNbr, Equal<Required<ARAdjust.adjNbr>>,
				And<ARAdjust.adjdLineNbr, Equal<Required<ARAdjust.adjdLineNbr>>>>>>>>>.Select
			(
				this,
				docTypes,
				adj.AdjgRefNbr,
				adj.AdjdDocType,
				adj.AdjdRefNbr,
				adj.VoidAdjNbr,
				adj.AdjdLineNbr
			);

			if (voidadj != null)
			{
				if ((bool)voidadj.Voided)
				{
					throw new PXException(Messages.DocumentApplicationAlreadyVoided);
				}
				PXCache<ARAdjust>.StoreOriginal(this, voidadj);

				voidadj.Voided = true;
				Caches[typeof(ARAdjust)].Update(voidadj);

				adj.AdjAmt = -voidadj.AdjAmt;
				adj.AdjDiscAmt = -voidadj.AdjDiscAmt;
				adj.AdjWOAmt = -voidadj.AdjWOAmt;
				adj.RGOLAmt = -voidadj.RGOLAmt;
				adj.CuryAdjdAmt = -voidadj.CuryAdjdAmt;
				adj.CuryAdjdDiscAmt = -voidadj.CuryAdjdDiscAmt;
				adj.CuryAdjdWOAmt = -voidadj.CuryAdjdWOAmt;
				adj.CuryAdjgAmt = -voidadj.CuryAdjgAmt;
				adj.CuryAdjgDiscAmt = -voidadj.CuryAdjgDiscAmt;
				adj.CuryAdjgWOAmt = -voidadj.CuryAdjgWOAmt;

				Caches[typeof(ARAdjust)].Update(adj);

				if (voidadj.AdjgDocType == ARDocType.CreditMemo && voidadj.AdjdHasPPDTaxes == true)
				{
					ARRegister crmemo = PXSelect<ARRegister, Where<ARRegister.docType, Equal<ARDocType.creditMemo>,
						And<ARRegister.refNbr, Equal<Required<ARRegister.refNbr>>>>>.SelectSingleBound(this, null, voidadj.AdjgRefNbr);
					if (crmemo != null && crmemo.PendingPPD == true)
					{
						PXUpdate<Set<ARAdjust.pPDCrMemoRefNbr, Null>, ARAdjust,
						Where<ARAdjust.pendingPPD, Equal<True>,
							And<ARAdjust.adjdDocType, Equal<Required<ARAdjust.adjdDocType>>,
 							And<ARAdjust.adjdRefNbr, Equal<Required<ARAdjust.adjdRefNbr>>,
							And<ARAdjust.pPDCrMemoRefNbr, Equal<Required<ARAdjust.pPDCrMemoRefNbr>>>>>>>
						.Update(this, voidadj.AdjdDocType, voidadj.AdjdRefNbr, voidadj.AdjgRefNbr);
					}
				}
			}
		}

		public virtual void UpdateBalances(ARAdjust adj, ARRegister adjddoc)
		{
			UpdateBalances(adj, adjddoc, null);
		}

		public virtual void UpdateBalances(ARAdjust adj, ARRegister adjddoc, ARTran adjdtran)
		{
			ARRegister ardoc = (ARRegister)adjddoc;
			ARRegister cachedDoc = (ARRegister)ARDocument.Cache.Locate(ardoc);
			if (cachedDoc != null)
			{
				PXCache<ARRegister>.RestoreCopy(ardoc, cachedDoc);
			}
			else if (_IsIntegrityCheck == true)
			{
				return;
			}

			if (_IsIntegrityCheck == false && adj.VoidAdjNbr != null)
			{
				VoidOrigAdjustment(adj);
			}

			// skip balance update for SmallCreditWO voiding application
			if (adjddoc.DocType == ARDocType.SmallCreditWO && adj.Voided == true && adj.VoidAdjNbr != null)
			{
				if (ardoc.Voided != true)
				{
					ardoc.Voided = true;
					RaiseInvoiceEvent(ardoc, ARInvoice.Events.Select(ev => ev.VoidDocument));
					ProcessVoidWOTranPost(ardoc, Caches[typeof(ARAdjust)].Current as ARAdjust);
				}

				return;
			}

			decimal? curyAdjdAmt = 0m;
			decimal? adjAmt = 0m;

			if (string.Equals(adj.AdjdDocType, ardoc.DocType) && string.Equals(adj.AdjdRefNbr, ardoc.RefNbr, StringComparison.OrdinalIgnoreCase))
			{
				curyAdjdAmt = (adj.CuryAdjdAmt + adj.CuryAdjdDiscAmt + adj.CuryAdjdWOAmt);
				adjAmt = (adj.AdjAmt + adj.AdjDiscAmt + adj.AdjWOAmt + (adj.ReverseGainLoss == false ? adj.RGOLAmt : -adj.RGOLAmt));
				
				if (_IsIntegrityCheck == false &&
					adj.IsInitialApplication != true &&
				    adj.AdjgDocType != ARDocType.Refund &&
					adj.AdjgDocType != ARDocType.VoidRefund &&
					!adj.IsSelfAdjustment())
				{
					decimal? ajustedBal = new PaymentBalanceAjuster(InvoiceEntryGraph.GetExtension<ARInvoiceEntry.MultiCurrency>())
						.AdjustWhenTheSameCuryAndRate(adj, ardoc.CuryDocBal, ardoc.DocBal);
					adj.RGOLAmt += ardoc.DocBal - ajustedBal;
					ardoc.DocBal = ajustedBal;
				}

				ardoc.CuryDiscBal -= adj.CuryAdjdDiscAmt;
				ardoc.DiscBal -= adj.AdjDiscAmt;
				ardoc.CuryDiscTaken += adj.CuryAdjdDiscAmt;
				ardoc.DiscTaken += adj.AdjDiscAmt;
				ardoc.RGOLAmt += adj.RGOLAmt;
			} 
			else if (string.Equals(adj.AdjgDocType, ardoc.DocType) && string.Equals(adj.AdjgRefNbr, ardoc.RefNbr, StringComparison.OrdinalIgnoreCase))
			{
				curyAdjdAmt = adj.CuryAdjgAmt;
				adjAmt = adj.AdjAmt;
			}

			//fully payed Prepayment Invoice
			if (ardoc.IsPrepaymentInvoiceDocument() && ardoc.Status == ARDocStatus.Open)
			{
				ardoc.CuryDocBal = 0m;
				ardoc.DocBal = 0m;
			}
			ardoc.CuryDocBal -= curyAdjdAmt;
			ardoc.DocBal -= adjAmt;

			if (ardoc.CuryDocBal == 0m && ardoc.DocBal != 0)
			{
				adj.RGOLAmt += ardoc.DocBal;
				ardoc.RGOLAmt += ardoc.DocBal;
			}

			if (_IsIntegrityCheck == false && adj.AdjgDocDate < adjddoc.DocDate)
			{
				throw new PXException(Messages.ApplDate_Less_DocDate, PXUIFieldAttribute.GetDisplayName<ARPayment.adjDate>(ARPayment_DocType_RefNbr.Cache));
			}

			if (_IsIntegrityCheck == false && string.Compare(adj.AdjgTranPeriodID, adjddoc.TranPeriodID) < 0)
			{
				throw new PXException(Messages.ApplPeriod_Less_DocPeriod, PXUIFieldAttribute.GetDisplayName<ARPayment.adjFinPeriodID>(ARPayment_DocType_RefNbr.Cache));
			}

			if (adjdtran != null && adjdtran.AreAllKeysFilled(ARTran_TranType_RefNbr.Cache))
			{
				ARTran tran = adjdtran;
				ARTran cachedTran = (ARTran)ARTran_TranType_RefNbr.Cache.Locate(tran);

				if (cachedTran != null)
				{
					tran = cachedTran;
		}
				else if (_IsIntegrityCheck) return;

				tran.CuryTranBal -= curyAdjdAmt;
				tran.TranBal -= adjAmt;
				tran.CuryCashDiscBal -= adj.CuryAdjdDiscAmt;
				tran.CashDiscBal -= adj.AdjDiscAmt;

				if (tran.CuryCashDiscBal == 0m)
				{
					tran.CashDiscBal = 0m;
				}

				if (tran.CuryTranBal == 0m)
				{
					tran.TranBal = 0m;
				}

				Sign balanceSign = tran.CuryOrigTranAmt < 0m ? Sign.Minus : Sign.Plus;

				if (!_IsIntegrityCheck &&
					(tran.CuryTranBal * balanceSign < 0m || tran.CuryCashDiscBal * balanceSign < 0m))
				{
					throw new PXException(balanceSign == Sign.Plus 
						? Messages.LineBalanceNegative 
						: Messages.LineBalancePositive);
				}

				ARTran_TranType_RefNbr.Update(tran);
			}

			Caches[typeof(ARAdjust)].Update(adj);
			PXSelectorAttribute.StoreResult<ARRegister.curyID>(ARDocument.Cache, ardoc,
				CM.CurrencyCollection.GetCurrency(ardoc.CuryID));
			ardoc = (ARRegister)ARDocument.Cache.Update(ardoc);
				}

		public virtual void CloseInvoiceAndClearBalances(ARRegister ardoc)
		{
			ardoc.CuryDiscBal = 0m;
			ardoc.DiscBal = 0m;
			ardoc.DocBal = 0m;
			ardoc.OpenDoc = false;

			SetClosedPeriodsFromLatestApplication(ardoc);
			ARDocument.Cache.Update(ardoc);
			RaiseInvoiceEvent(ardoc, ARInvoice.Events.Select(ev => ev.CloseDocument));
			RaisePaymentEvent(ardoc, ARPayment.Events.Select(ev => ev.CloseDocument));
		}

		public virtual void OpenInvoiceAndRecoverBalances(ARRegister ardoc)
		{
			if (ardoc.CuryDocBal == ardoc.CuryOrigDocAmt)
			{
				ardoc.CuryDiscBal = ardoc.CuryOrigDiscAmt;
				ardoc.DiscBal = ardoc.OrigDiscAmt;
				ardoc.CuryDiscTaken = 0m;
				ardoc.DiscTaken = 0m;
			}

			ardoc.OpenDoc = true;
			ardoc.ClosedDate = null;
			ardoc.ClosedTranPeriodID = null;
			ardoc.ClosedFinPeriodID = null;
			ARDocument.Cache.Update(ardoc);
			RaiseInvoiceEvent(ardoc, ARInvoice.Events.Select(ev => ev.OpenDocument));
			RaisePaymentEvent(ardoc, ARPayment.Events.Select(ev => ev.OpenDocument));
		}

		public virtual void CloseInvoiceAndRecoverBalances(ARRegister ardoc)
		{
			ARAdjust2 crmAdjustBalance = PXSelectGroupBy<ARAdjust2,
					Where<ARAdjust2.adjdDocType, Equal<Required<ARAdjust2.adjdDocType>>,
						And<ARAdjust2.adjdRefNbr, Equal<Required<ARAdjust2.adjdRefNbr>>,
						And<Where<ARAdjust2.adjgDocType, Equal<ARDocType.creditMemo>>>>>,
					Aggregate<GroupBy<ARAdjust2.adjdDocType, GroupBy<ARAdjust2.adjdRefNbr,
						Sum<ARAdjust2.curyAdjdAmt, Sum<ARAdjust2.adjAmt>>>>>>
					.Select(this, ardoc.DocType, ardoc.RefNbr);
			decimal? AdjustedBalance = crmAdjustBalance?.AdjAmt ?? 0m;
			decimal? CuryAdjustedBalance = crmAdjustBalance?.CuryAdjdAmt ?? 0m;
			ardoc.CuryDocBal = ardoc.CuryOrigDocAmt - AdjustedBalance;
			ardoc.DocBal = ardoc.OrigDocAmt - CuryAdjustedBalance;
			ardoc.CuryDiscBal = ardoc.CuryOrigDiscAmt;
			ardoc.DiscBal = ardoc.OrigDiscAmt;
			ardoc.CuryDiscTaken = 0m;
			ardoc.DiscTaken = 0m;

			ardoc.OpenDoc = true;
			SetClosedPeriodsFromLatestApplication(ardoc);
			if (ardoc.CuryDocBal == 0 && ardoc.Voided != true)
			{
				ardoc.Voided = true;
			}
			ARDocument.Cache.Update(ardoc);
			if (ardoc.Voided == true)
			{
				// For the voided document, we must remove all unreleased applications.
				// -
				foreach (ARAdjust application in PXSelect<ARAdjust,
					Where<
						ARAdjust.adjgDocType, Equal<Required<ARAdjust.adjgDocType>>,
						And<ARAdjust.adjgRefNbr, Equal<Required<ARAdjust.adjgRefNbr>>,
						And<ARAdjust.released, NotEqual<True>>>>>
					.Select(this, ardoc.DocType, ardoc.RefNbr))
				{
					Caches[typeof(ARAdjust)].Delete(application);
				}
				RaiseInvoiceEvent(ardoc, ARInvoice.Events.Select(ev => ev.VoidDocument));
			}
			else
			{
				RaiseInvoiceEvent(ardoc, ARInvoice.Events.Select(ev => ev.CloseDocument));
				RaisePaymentEvent(ardoc, ARPayment.Events.Select(ev => ev.CloseDocument));
			}
		}

		private void UpdateVoidedCheck(ARRegister voidcheck)
		{
			foreach (string origDocType in voidcheck.PossibleOriginalDocumentTypes())
			{
				foreach (PXResult<ARPayment, CurrencyInfo, Currency, Customer> res in ARPayment_DocType_RefNbr
					.Select(origDocType, voidcheck.RefNbr, voidcheck.CustomerID))
				{
					ARRegister ardoc = res;
					bool? voidedOldValue = ardoc.Voided;
					ARRegister cached = (ARRegister)ARDocument.Cache.Locate(ardoc);

					if (cached != null)
					{
						PXCache<ARRegister>.RestoreCopy(ardoc, cached);
					}
					
					ardoc.Voided = true;
					ardoc.OpenDoc = false;
					ardoc.Hold = false;
					ardoc.CuryDocBal = 0m;
					ardoc.DocBal = 0m;
					
					SetClosedPeriodsFromLatestApplication(ardoc);
					ARDocument.Cache.Update(ardoc);
					RaisePaymentEvent(ardoc, ARPayment.Events.Select(ev => ev.VoidDocument));

					if (voidedOldValue == false)
					{
						((ARPayment)res).PostponeVoidedFlag = true;
					}

					if (voidcheck.NoteID == ardoc.NoteID)
						ARDocument.Cache.RestoreCopy(voidcheck, ardoc);

				if (!_IsIntegrityCheck)
				{
					// For the voided document, we must remove all unreleased applications.
					// -
					foreach (ARAdjust application in PXSelect<ARAdjust,
						Where<
							ARAdjust.adjgDocType, Equal<Required<ARAdjust.adjgDocType>>,
							And<ARAdjust.adjgRefNbr, Equal<Required<ARAdjust.adjgRefNbr>>,
							And<ARAdjust.released, NotEqual<True>>>>>
						.Select(this, ardoc.DocType, ardoc.RefNbr))
					{
							Caches[typeof(ARAdjust)].Delete(application);
					}
				}
				}
			}
		}

		private void VerifyVoidCheckNumberMatchesOriginalPayment(ARPayment voidcheck)
		{
			foreach (string origDocType in voidcheck.PossibleOriginalDocumentTypes())
			{
				foreach (PXResult<ARPayment, CurrencyInfo, Currency, Customer> res in ARPayment_DocType_RefNbr
					.Select(origDocType, voidcheck.RefNbr, voidcheck.CustomerID))
				{
					ARPayment payment = res;
					if (_IsIntegrityCheck == false &&
						!string.Equals(voidcheck.ExtRefNbr, payment.ExtRefNbr, StringComparison.OrdinalIgnoreCase))
					{
						PaymentMethod pm = PXSelect<PaymentMethod, Where<PaymentMethod.paymentMethodID, Equal<Required<PaymentMethod.paymentMethodID>>>>.Select(this, voidcheck.PaymentMethodID);

						if (pm.PaymentType.IsNotIn(PaymentMethodType.CreditCard, PaymentMethodType.EFT))
						{
							throw new PXException(Messages.VoidAppl_CheckNbr_NotMatchOrigPayment);
						}
					}
				}
			}
		}

		/// <summary>
		/// Ensures that no unreleased voiding document exists for the specified payment.
		/// (If the applications of the voided and the voiding document are not
		/// synchronized, it can lead to a balance discrepancy, see AC-78131).
		/// </summary>
		public static void EnsureNoUnreleasedVoidPaymentExists(PXGraph selectGraph, ARRegister payment, string actionDescription)
		{
			ARRegister unreleasedVoidPayment = 
				HasUnreleasedVoidPayment<ARRegister.docType, ARRegister.refNbr>.Select(selectGraph, payment);

			if (unreleasedVoidPayment != null)
		{
				throw new PXException(
					Common.Messages.CannotPerformActionOnDocumentUnreleasedVoidPaymentExists,
					PXLocalizer.Localize(GetLabel.For<ARDocType>(payment.DocType)),
					payment.RefNbr,
					PXLocalizer.Localize(actionDescription),
					PXLocalizer.Localize(GetLabel.For<ARDocType>(unreleasedVoidPayment.DocType)),
					PXLocalizer.Localize(GetLabel.For<ARDocType>(payment.DocType)),
					PXLocalizer.Localize(GetLabel.For<ARDocType>(unreleasedVoidPayment.DocType)),
					unreleasedVoidPayment.RefNbr);
			}
		}
			
		/// <summary>
		/// The method to release payment part.
		/// The maintenance screen is "Payments and Applications" (AR302000).
		/// </summary>
		public virtual void ProcessPayment(
			JournalEntry je, 
			ARRegister doc,
			PXResult<ARPayment, CurrencyInfo, Currency, Customer, CashAccount> res)
		{
			ARPayment ardoc = res;
			CurrencyInfo new_info = res;
			Currency paycury = res;
			Customer vend = res;
			CashAccount cashacct = res;
			

			EnsureNoUnreleasedVoidPaymentExists(this, ardoc, Common.Messages.ActionReleased);

			CustomerClass custclass = PXSelectJoin<CustomerClass, InnerJoin<ARSetup, On<ARSetup.dfltCustomerClassID, Equal<CustomerClass.customerClassID>>>>.Select(this, null);

			bool isCashSaleOrCashReturnDocument =
				doc.DocType == ARDocType.CashSale ||
				doc.DocType == ARDocType.CashReturn;

			if (doc.Released != true)
			{
				// Should always restore ARRegister to ARPayment after invoice part release of cash sale
				PXCache<ARRegister>.RestoreCopy(ardoc, doc);

				doc.CuryDocBal = doc.CuryOrigDocAmt;
				doc.DocBal = doc.OrigDocAmt;
				doc.RGOLAmt = 0;

				if (doc.DocType != ARDocType.SmallBalanceWO)
				{
					bool isDebit = (ardoc.DrCr == DrCr.Debit);

					GLTran tran = new GLTran();
					tran.SummPost = true;
					tran.BranchID = cashacct.BranchID;
					tran.AccountID = cashacct.AccountID;
					tran.SubID = cashacct.SubID;
					tran.CuryDebitAmt = isDebit ? ardoc.CuryOrigDocAmt : 0m;
					tran.DebitAmt = isDebit ? ardoc.OrigDocAmt : 0m;
					tran.CuryCreditAmt = isDebit ? 0m : ardoc.CuryOrigDocAmt;
					tran.CreditAmt = isDebit ? 0m : ardoc.OrigDocAmt;
					tran.TranType = ardoc.DocType;
					tran.TranClass = ardoc.DocClass;
					tran.RefNbr = ardoc.RefNbr;
					tran.TranDesc = ardoc.DocDesc;
					tran.TranDate = ardoc.DocDate;
					FinPeriodIDAttribute.SetPeriodsByMaster<GLTran.finPeriodID>(
						je.GLTranModuleBatNbr.Cache, tran, ardoc.TranPeriodID);
					tran.CuryInfoID = new_info.CuryInfoID;
					tran.Released = true;
					tran.CATranID = ardoc.CATranID;
					tran.ReferenceID = ardoc.CustomerID;
					tran.ProjectID = ProjectDefaultAttribute.NonProject();
					
					InsertPaymentTransaction(je, tran, 
						new GLTranInsertionContext { ARRegisterRecord = doc });

					//Debit Payment AR Account
					tran = new GLTran();
					tran.SummPost = true;
					if (!ARPaymentType.CanHaveBalance(ardoc.DocType))
					{
						tran.ZeroPost = false;
					}
					tran.BranchID = ardoc.BranchID;
					tran.AccountID = ardoc.ARAccountID;
					tran.ReclassificationProhibited = true;
					tran.SubID = ardoc.ARSubID;
					tran.CuryDebitAmt = (ardoc.DrCr == DrCr.Debit) ? 0m : ardoc.CuryOrigDocAmt;
					tran.DebitAmt = (ardoc.DrCr == DrCr.Debit) ? 0m : ardoc.OrigDocAmt;
					tran.CuryCreditAmt = (ardoc.DrCr == DrCr.Debit) ? ardoc.CuryOrigDocAmt : 0m;
					tran.CreditAmt = (ardoc.DrCr == DrCr.Debit) ? ardoc.OrigDocAmt : 0m;
					tran.TranType = ardoc.DocType;
					tran.TranClass = GLTran.tranClass.Payment;
					tran.RefNbr = ardoc.RefNbr;
					tran.TranDesc = ardoc.DocDesc;
					tran.TranDate = ardoc.DocDate;
					FinPeriodIDAttribute.SetPeriodsByMaster<GLTran.finPeriodID>(
						je.GLTranModuleBatNbr.Cache, tran, ardoc.TranPeriodID);
					tran.CuryInfoID = new_info.CuryInfoID;
					tran.Released = true;
					tran.ReferenceID = ardoc.CustomerID;
					tran.ProjectID = ProjectDefaultAttribute.NonProject();

					UpdateHistory(tran, vend);
					UpdateHistory(tran, vend, new_info);

					InsertPaymentTransaction(je, tran, 
						new GLTranInsertionContext { ARRegisterRecord = doc });

					if (IsMigratedDocumentForProcessing(doc))
					{
						ProcessMigratedDocument(je, tran, doc, isDebit, vend, new_info);
						doc = (ARRegister)ARDocument.Cache.Locate(doc) ?? doc;
						PXCache<ARRegister>.RestoreCopy(ardoc, doc);
					}
				}

				foreach (ARPaymentChargeTran charge in ARPaymentChargeTran_DocType_RefNbr.Select(doc.DocType, doc.RefNbr))
				{
					bool isCADebit = charge.GetCASign() == 1;
					
					GLTran tran = new GLTran();
					tran.SummPost = true;
					tran.BranchID = cashacct.BranchID;
					tran.AccountID = cashacct.AccountID;
					tran.SubID = cashacct.SubID;
					tran.CuryDebitAmt = isCADebit ? charge.CuryTranAmt : 0m;
					tran.DebitAmt = isCADebit ? charge.TranAmt : 0m;
					tran.CuryCreditAmt = isCADebit ? 0m : charge.CuryTranAmt;
					tran.CreditAmt = isCADebit ? 0m : charge.TranAmt;
					tran.TranType = charge.DocType;
					tran.TranClass = ardoc.DocClass;
					tran.RefNbr = charge.RefNbr;
					tran.TranDesc = charge.Consolidate == true ? ardoc.DocDesc : charge.TranDesc;
					tran.TranDate = charge.TranDate;
					FinPeriodIDAttribute.SetPeriodsByMaster<GLTran.finPeriodID>(
						je.GLTranModuleBatNbr.Cache, tran, tran.TranPeriodID);
					tran.CuryInfoID = new_info.CuryInfoID;
					tran.Released = true;
					tran.CATranID = charge.CashTranID ?? ardoc.CATranID;
					tran.ReferenceID = ardoc.CustomerID;

					InsertPaymentChargeTransaction(je, tran, 
						new GLTranInsertionContext { ARRegisterRecord = doc, ARPaymentChargeTranRecord = charge });

					tran = new GLTran();
					tran.SummPost = true;
					tran.ZeroPost = false;
					tran.BranchID = ardoc.BranchID;
					tran.AccountID = charge.AccountID;
					tran.SubID = charge.SubID;
					tran.ProjectID = charge.ProjectID;
					tran.TaskID = charge.TaskID;
					tran.CostCodeID = charge.CostCodeID;
					tran.CuryDebitAmt = isCADebit ? 0m : charge.CuryTranAmt;
					tran.DebitAmt = isCADebit ? 0m : charge.TranAmt;
					tran.CuryCreditAmt = isCADebit ? charge.CuryTranAmt : 0m;
					tran.CreditAmt = isCADebit ? charge.TranAmt : 0m;
					tran.TranType = charge.DocType;
					tran.TranClass = GLTran.tranClass.Charge;
					tran.RefNbr = charge.RefNbr;
					tran.TranDesc = charge.TranDesc;
					tran.TranDate = charge.TranDate;
					FinPeriodIDAttribute.SetPeriodsByMaster<GLTran.finPeriodID>(
						je.GLTranModuleBatNbr.Cache, tran, tran.TranPeriodID);
					tran.CuryInfoID = new_info.CuryInfoID;
					tran.Released = true;
					tran.ReferenceID = ardoc.CustomerID;

					InsertPaymentChargeTransaction(je, tran, 
						new GLTranInsertionContext { ARRegisterRecord = doc, ARPaymentChargeTranRecord = charge });

					charge.Released = true;
					ARPaymentChargeTran_DocType_RefNbr.Update(charge);
				}

				ProcessOriginTranPost(ardoc);
				doc.Voided = false;
				doc.OpenDoc = true;
				doc.ClosedDate = null;
				doc.ClosedFinPeriodID = null;
				doc.ClosedTranPeriodID = null;
				RaisePaymentEvent(doc, ARPayment.Events.Select(ev => ev.OpenDocument));
				if (ardoc.VoidAppl == true)
				{
					doc.OpenDoc = false;
					doc.ClosedDate = doc.DocDate;
					doc.ClosedFinPeriodID = doc.FinPeriodID;
					doc.ClosedTranPeriodID = doc.TranPeriodID;
					SetClosedPeriodsFromLatestApplication(doc);
					RaiseInvoiceEvent(doc, ARInvoice.Events.Select(ev => ev.CloseDocument));
					VerifyVoidCheckNumberMatchesOriginalPayment(ardoc);
				}
			}

			if (isCashSaleOrCashReturnDocument)
			{
				if (_IsIntegrityCheck == false)
				{
					CreateSelfApplicationForDocument(doc);
				}

				doc.CuryDocBal += doc.CuryOrigDiscAmt;
				doc.DocBal += doc.OrigDiscAmt;
				doc.ClosedDate = doc.DocDate;
				doc.ClosedFinPeriodID = doc.FinPeriodID;
				doc.ClosedTranPeriodID = doc.TranPeriodID;
			}
			doc.PendingProcessing = false;
			doc.Released = true;

			if (ardoc.Released == false)
			{
				ardoc.PostponeReleasedFlag = true;
		}
		}

		/// <summary>
		/// The method to verify invoice balances and close it if needed.
		/// This verification should be called after
		/// release process of invoice applications.
		/// </summary>
		public virtual void VerifyAdjustedDocumentAndClose(ARRegister adjddoc)
		{
			ARRegister ardoc = adjddoc;
			ARRegister cachedDoc = (ARRegister)ARDocument.Cache.Locate(ardoc);
			if (cachedDoc != null)
			{
				PXCache<ARRegister>.RestoreCopy(ardoc, cachedDoc);
			}
			else if (_IsIntegrityCheck) return;

			if (ardoc.CuryDiscBal == 0m)
			{
				ardoc.DiscBal = 0m;
			}

			if (_IsIntegrityCheck == false && 
				(ardoc.CuryDocBal < 0m || ardoc.CuryDocBal > ardoc.CuryOrigDocAmt))
			{
				string docType = PXStringListAttribute.GetLocalizedLabel<ARRegister.docType>(ARDocument.Cache, ardoc);
				throw new PXException(ardoc.CuryDocBal < 0m ? 
					Messages.DocumentOutOfBalanceNegative :
					Messages.DocumentOutOfBalanceHigher, docType, ardoc.RefNbr);
			}

			if (ardoc.IsOriginalRetainageDocument())
			{
				if (IsFullyProcessedOriginalRetainageDocument(ardoc))
				{
					CloseInvoiceAndClearBalances(ardoc);
				}
				else
				{
					OpenInvoiceAndRecoverBalances(ardoc);
				}
			}
			else if (ardoc.IsPrepaymentInvoiceDocument()
				&& ardoc.PendingPayment == true
					&& ardoc.HasZeroBalance<ARRegister.curyDocBal, ARTran.curyTranBal>(this))
				{
					CloseInvoiceAndRecoverBalances(ardoc);
				}
			else if (ardoc.HasZeroBalance<ARRegister.curyDocBal, ARTran.curyTranBal>(this))
			{
				CloseInvoiceAndClearBalances(ardoc);
			}
			else
			{
				OpenInvoiceAndRecoverBalances(ardoc);
			}

			#region Close Retainage Document

			ARRegister origRetainageDoc = null;

			// We should close original retainage Document
			// only when all its retainage balances will be
			// equal to 0.
			//
			if (ardoc.IsRetainageDocument == true &&
				(origRetainageDoc = GetOriginalRetainageDocument(ardoc)) != null)
			{
				if (IsFullyProcessedOriginalRetainageDocument(origRetainageDoc))
				{
					CloseInvoiceAndClearBalances(origRetainageDoc);
				}
				else
				{
					OpenInvoiceAndRecoverBalances(origRetainageDoc);
				}

				ARDocument.Cache.Update(origRetainageDoc);
			}

			#endregion
		}

		/// <summary>
		/// The method to verify payment balances and close it if needed.
		/// This verification should be called after
		/// release process of payment and applications.
		/// </summary>
		public virtual void VerifyPaymentRoundAndClose(
			JournalEntry je,
			ARRegister paymentRegister,
			ARPayment payment,
			Customer paymentCustomer,
			CurrencyInfo new_info,
			Currency paycury,
			Tuple<ARAdjust, CurrencyInfo> lastAdjustment)
		{
			ARAdjust prev_adj = lastAdjustment.Item1;
			CurrencyInfo prev_info = lastAdjustment.Item2;

			if (_IsIntegrityCheck == false && payment.VoidAppl != true && 
				(paymentRegister.CuryDocBal < 0m || paymentRegister.CuryDocBal > paymentRegister.CuryOrigDocAmt))
			{
				string docType = PXStringListAttribute.GetLocalizedLabel<ARRegister.docType>(ARDocument.Cache, paymentRegister);
				throw new PXException(paymentRegister.CuryDocBal < 0m ?
					Messages.DocumentOutOfBalanceNegative :
					Messages.DocumentOutOfBalanceHigher, docType, paymentRegister.RefNbr);
			}

			// The case, when payment is open and sum of base amounts for applications 
			// exceeds payment base amount, due to small rounding for each application.
			// 
			bool isOpenPaymentWithNegativeBalance = paymentRegister.CuryDocBal > 0m && paymentRegister.DocBal < 0;

			if (//!_IsIntegrityCheck &&
				prev_adj.AdjdRefNbr != null &&
				(paymentRegister.CuryDocBal == 0m && paymentRegister.DocBal != 0m || isOpenPaymentWithNegativeBalance))
			{
				ProcessAdjustmentsRounding(je, paymentRegister, prev_adj, payment, paymentCustomer, 
					paycury, prev_info, new_info, paymentRegister.DocBal, prev_adj.ReverseGainLoss);
			}

			bool hasZeroBalance = paymentRegister.IsOriginalRetainageDocument()
				? IsFullyProcessedOriginalRetainageDocument(paymentRegister)
				: paymentRegister.HasZeroBalance<ARRegister.curyDocBal, ARTran.curyTranBal>(this);
			bool isVoidingDoc = payment.VoidAppl == true ||
				(payment.SelfVoidingDoc == true && prev_adj.Voided == true);
			bool hasAnyApplications = prev_adj.AdjdRefNbr != null;

			if ((!paymentRegister.IsOriginalRetainageDocument() || paymentRegister.DocType != ARDocType.CreditMemo || hasAnyApplications)
				&& (hasZeroBalance || isVoidingDoc))
			{
				paymentRegister.CuryDocBal = 0m;
				paymentRegister.DocBal = 0m;
				paymentRegister.OpenDoc = false;

				SetClosedPeriodsFromLatestApplication(paymentRegister);
				if (isVoidingDoc && paymentRegister.DocType != ARDocType.CashReturn)
				{
					UpdateVoidedCheck(paymentRegister);
				}

				if (payment.VoidAppl != true)
				{
					DeactivateOneTimeCustomerIfAllDocsIsClosed(paymentCustomer);
				}
				RaiseInvoiceEvent(paymentRegister, ARInvoice.Events.Select(e=>e.CloseDocument));
				RaisePaymentEvent(paymentRegister, ARPayment.Events.Select(e=>e.CloseDocument));
			}
			else
			{
				if (isOpenPaymentWithNegativeBalance)
				{
					paymentRegister.DocBal = 0m;
				}

				paymentRegister.OpenDoc = true;
				paymentRegister.ClosedDate = null;
				paymentRegister.ClosedTranPeriodID = null;
				paymentRegister.ClosedFinPeriodID = null;
				RaiseInvoiceEvent(paymentRegister, ARInvoice.Events.Select(e => e.OpenDocument));
				RaisePaymentEvent(paymentRegister, ARPayment.Events.Select(e => e.OpenDocument));
			}

			#region Close Retainage Document

			ARRegister origRetainageDoc = null;

			if (paymentRegister.IsRetainageDocument == true &&
				(origRetainageDoc = GetOriginalRetainageDocument(paymentRegister)) != null)
			{
				if (IsFullyProcessedOriginalRetainageDocument(origRetainageDoc))
				{
					CloseInvoiceAndClearBalances(origRetainageDoc);
		}
				else
				{
					OpenInvoiceAndRecoverBalances(origRetainageDoc);
		}

				ARDocument.Cache.Update(origRetainageDoc);
			}

			#endregion
		}

		protected void DeactivateOneTimeCustomerIfAllDocsIsClosed(Customer customer)
		{
			if (customer.Status != CustomerStatus.OneTime)
				return;

			ARRegister arRegister = PXSelect<ARRegister,
												Where<ARRegister.customerID, Equal<Required<ARRegister.customerID>>,
														And<ARRegister.released, Equal<boolTrue>,
														And<ARRegister.openDoc, Equal<boolTrue>>>>>
												.SelectWindowed(this, 0, 1, customer.BAccountID);

			if (arRegister != null)
				return;

			customer.Status = CustomerStatus.Inactive;
			Caches[typeof(Customer)].Update(customer);
			Caches[typeof(Customer)].Persist(PXDBOperation.Update);
			Caches[typeof(Customer)].Persisted(false);
		}

		protected virtual CM.CurrencyInfo GetCurrencyInfoCopyForGL(JournalEntry je, CurrencyInfo info, bool? baseCalc = null)
		{
			CM.CurrencyInfo new_info = info.GetCM();
			new_info.CuryInfoID = null;
			new_info.ModuleCode = BatchModule.GL;
			new_info.BaseCalc = (baseCalc ?? new_info.BaseCalc);
			new_info = je.currencyinfo.Insert(new_info) ?? new_info;

			return new_info;
		}

		public PXSelect<ARStatementDetail, Where<ARStatementDetail.docType, Equal<Required<ARStatementDetail.docType>>, And<ARStatementDetail.refNbr, Equal<Required<ARStatementDetail.refNbr>>>>> StatementDetailsView;

		private ARStatementDetail GetRelatedStatementDetail(ARTranPost tranPost)
		{
			if (tranPost.Type.IsIn(ARTranPostType.Origin, ARTranPostType.Application, ARTranPostType.Adjustment))
			{
				//I hope platform will automatically cache ARStatementDetails found by RefNbr
				return StatementDetailsView.Select(tranPost.DocType, tranPost.RefNbr).RowCast<ARStatementDetail>().Where(_ => _.RefNoteID == tranPost.RefNoteID).SingleOrDefault();
			}
			else return null;
		}

		protected virtual void _(Events.RowInserting<ARTranPost> e)
		{
			if (IsIntegrityCheck)
				e.Row.StatementDate = GetRelatedStatementDetail(e.Row)?.StatementDate;
		}

		protected virtual void _(Events.RowPersisted<ARTranPost> e)
		{
			if (IsIntegrityCheck)
			{
				ARStatementDetail statementDetail = GetRelatedStatementDetail(e.Row);
				if (statementDetail == null) return;
				else
				{
					statementDetail.TranPostID = e.Row.ID;
					StatementDetailsView.Update(statementDetail);
				}
			}
		}

		/// <summary>
		/// The method to release applications only 
		/// without payment part.
		/// </summary>
		protected virtual Tuple<ARAdjust, CurrencyInfo> ProcessAdjustments(
			JournalEntry je,
			PXResultset<ARAdjust> adjustments,
			ARRegister paymentRegister,
			ARPayment payment,
			Customer paymentCustomer,
			CM.CurrencyInfo new_info,
			Currency paycury)
		{
			ARAdjust prev_adj = new ARAdjust();
			CurrencyInfo prev_info = new CurrencyInfo();
			tranPostRetainagePayments = new Dictionary<ARTranPostKey, ARTranPost>();

			// All special applications, which have been created in migration mode
			// for migrated document - should be excluded from the processing
			//
			foreach (var group in adjustments
				.AsEnumerable()
				.Where(res => ((ARAdjust)res).IsInitialApplication != true)
				.Cast<PXResult<ARAdjust, CurrencyInfo, Currency, ARRegister, ARInvoice, ARPayment, ARTran>>()
				.GroupBy(res => ((ARRegister)res).AreAllKeysFilled(ARDocument.Cache) ? (ARRegister)res : (ARPayment)res, ARDocument.Cache.GetComparer()))
			{
				ARRegister key = (ARRegister)group.Key;
				ARRegister adjustedDocument = null;
				bool verifyAdjustedDocument = false;
				foreach (PXResult<ARAdjust, CurrencyInfo, Currency, ARRegister, ARInvoice, ARPayment, ARTran> adjres in group)
				{
					ARAdjust adj = adjres;
					ARRegister reg = adjres;
					CurrencyInfo vouch_info = adjres;
					Currency cury = adjres;
					ARInvoice adjustedInvoice = adjres;
					ARPayment adjgdoc = adjres;
					CurrencyInfo info2 =
						Common.Utilities.Clone<CM.CurrencyInfo2, CurrencyInfo>
							(this, PXResult.Unwrap<CM.CurrencyInfo2>(adjres));

					ARTran line = adjres;

					if (reg.Released != true && !adj.IsSelfAdjustment()) continue;

					// Restore full invoice / payment from the "single table" stripped version.
					// 
					if (adjustedInvoice?.RefNbr != null)
					{
						PXCache<ARRegister>.RestoreCopy(adjustedInvoice, reg);
						adjustedDocument = adjustedInvoice;
					}
					else if (adjgdoc?.RefNbr != null)
					{
						PXCache<ARRegister>.RestoreCopy(adjgdoc, reg);
						adjustedDocument = adjgdoc;
					}
					PXCache<ARRegister>.StoreOriginal(this, adjustedInvoice);
					PXCache<ARAdjust>.StoreOriginal(this, adj);
					PXCache<ARTran>.StoreOriginal(this, line);
					GetExtension<MultiCurrency>().StoreResult(vouch_info);
					GetExtension<MultiCurrency>().StoreResult(info2);
					_ie?.GetExtension<ARInvoiceEntry.MultiCurrency>().StoreResult(vouch_info);
					_ie?.GetExtension<ARInvoiceEntry.MultiCurrency>().StoreResult(info2);
					PXParentAttribute.SetParent(this.Caches<ARAdjust>(),adj, typeof(ARRegister), adjgdoc);

				if (adj.AdjdDocType == ARDocType.SmallCreditWO
						&& string.CompareOrdinal(payment.AdjFinPeriodID, adjustedInvoice.FinPeriodID) > 0
						&& adj.Voided != true && adj.VoidAppl != true)
				{
					// correct SCM batch period and date if it was changed by payment (application date > write off date)
					SegregateBatch(je, adjustedInvoice.BranchID, adjustedInvoice.CuryID, adjustedInvoice.DocDate, adjustedInvoice.FinPeriodID, adjustedInvoice.DocDesc, vouch_info);
				}

					EnsureNoUnreleasedVoidPaymentExists(
						this,
						adjgdoc,
						payment.DocType == ARDocType.Refund
							? Common.Messages.ActionRefunded
							: Common.Messages.ActionAdjusted);

				if (adj.CuryAdjgAmt == 0m && adj.CuryAdjgDiscAmt == 0m
						&& (!(adjustedInvoice.IsRetainageDocument == true) || adjustedInvoice.RetainageUnreleasedAmt != 0 || adjustedInvoice.RetainageReleased != 0))
					{
						ARAdjust_AdjgDocType_RefNbr_CustomerID.Cache.Delete(adj);
						continue;
					}

					if (adj.Hold == true)
					{
						throw new PXException(Messages.Document_OnHold_CannotRelease);
					}

					if (adjustedInvoice?.PaymentsByLinesAllowed == true && adj.AdjdLineNbr == 0)
					{
						continue;
					}

					if (_IsIntegrityCheck == false && adj.PendingPPD == true)
					{
						adjustedInvoice.PendingPPD = !adj.Voided;
						ARDocument.Cache.Update(adjustedInvoice);
					}

					if (_IsIntegrityCheck == true && adj.AdjdDocType == ARDocType.SmallCreditWO && adj.RGOLAmt != 0)
					{
						adj.AdjAmt += adj.SignedRGOLAmt;
						adj.RGOLAmt = 0;
						ARAdjust_AdjdDocType_RefNbr_CustomerID.Update(adj);
					}

					bool notValidatingCrossCustomerFromAdjusting = (_IsIntegrityCheck && adj.CustomerID != adj.AdjdCustomerID) == false;

					ProcessAdjustmentSalesPersonCommission(adj, payment, adjustedInvoice, cury, line);

					if (adjustedInvoice.RefNbr != null)
					{
						if (notValidatingCrossCustomerFromAdjusting)
						{
							verifyAdjustedDocument = true;
							if (_IsIntegrityCheck == false && payment.OrigReleased == true && payment.AdjDate < adjustedInvoice.DocDate)
							{
								throw new PXException(Messages.ApplDate_Less_DocDate,
									PXUIFieldAttribute.GetDisplayName<ARPayment.adjDate>(ARPayment_DocType_RefNbr.Cache));
							}
							UpdateBalances(adj, adjustedInvoice, line);
							UpdateARBalances(adj, adjustedInvoice);
						}

						UpdateARBalances(adj, paymentRegister);

						if (notValidatingCrossCustomerFromAdjusting)
						{
							_oldInvoiceRefresher.RecordDocument(adjustedInvoice.BranchID, adjustedInvoice.CustomerID, adjustedInvoice.CustomerLocationID);
						}
					}
					else
					{
						verifyAdjustedDocument = true;
						UpdateBalances(adj, adjgdoc);
						UpdateARBalances(adj, paymentRegister);
						UpdateARBalances(adj, adjgdoc);
					}
					Batch oldBatch = (Batch)je.BatchModule.Cache.CreateCopy(je.BatchModule.Current);
					ProcessAdjustmentAdjusting(je, adj, payment, adjustedInvoice, paymentCustomer, CurrencyInfo.GetEX(new_info));

					if (notValidatingCrossCustomerFromAdjusting)
					{
						ProcessAdjustmentAdjusted(je, adj, adjustedInvoice, payment, vouch_info, CurrencyInfo.GetEX(new_info));
					}

					ProcessAdjustmentCashDiscount(je, adj, payment, paymentCustomer, CurrencyInfo.GetEX(new_info), vouch_info);
					ProcessAdjustmentWriteOff(je, adj, payment, paymentCustomer, CurrencyInfo.GetEX(new_info), vouch_info);
					ProcessAdjustmentGOL(je, adj, payment, paymentCustomer, adjustedInvoice, paycury, cury, CurrencyInfo.GetEX(new_info), vouch_info);

					//true for Quick Check and Void Quick Check
					if (adj.AdjgDocType != adj.AdjdDocType || adj.AdjgRefNbr != adj.AdjdRefNbr)
					{
						var curyAdjBal = adj.CuryAdjgAmt;
						var adjBal = adj.AdjAmt;
						paymentRegister.CuryDocBal -= adj.AdjgBalSign * curyAdjBal;
						paymentRegister.DocBal -= adj.AdjgBalSign * adjBal;

						if (paymentRegister.PaymentsByLinesAllowed == true)
						{
							// It is not possible to create Pay by Line CreditMemo
							// any more after the AC-141326 issue.
							ProcessPayByLineCreditMemoAdjustment(paymentRegister, adj);
						}
					}

					ProcessSVATClosingAdjustments(je, adj, adjustedInvoice, paymentRegister, CurrencyInfo.GetEX(new_info));

					if (_IsIntegrityCheck == false)
					{
						PXSelectorAttribute.StoreCached<ARAdjust.adjdRefNbr>(Caches[typeof(ARAdjust)],
							adj, adjustedInvoice);

						bool saveBatch = (payment.Released == false) ||
								(!je.BatchModule.Cache.ObjectsEqual<
									GL.Batch.module, GL.Batch.batchNbr,
									GL.Batch.lineCntr,
									GL.Batch.creditTotal, GL.Batch.debitTotal,
									GL.Batch.curyCreditTotal, GL.Batch.curyDebitTotal>
									(oldBatch, je.BatchModule.Current));


						SaveBatchForAdjustment(je, adj, adjustedDocument, saveBatch);
						adj.Released = true;
						adj = (ARAdjust)Caches[typeof(ARAdjust)].Update(adj);
					}

					prev_adj = adj;
					prev_info = adjres;

					ProcessSVATAdjustments(je, prev_adj, adjustedInvoice, paymentRegister, CurrencyInfo.GetEX(new_info));
					ProcessAdjustmentTranPost(line, adj, adjustedDocument, paymentRegister);
				}

				if (verifyAdjustedDocument && adjustedDocument != null)
				{
					VerifyAdjustedDocumentAndClose(adjustedDocument);
				}
			}
			foreach (KeyValuePair<ARTranPostKey, ARTranPost> kvp in tranPostRetainagePayments)
			{
				if (IsNeedUpdateHistoryForTransaction(kvp.Value.FinPeriodID))
					TranPost.Insert(kvp.Value);
			}

			return new Tuple<ARAdjust, CurrencyInfo>(prev_adj, prev_info);
		}

		[Obsolete(Common.Messages.MethodIsObsoleteRemoveInLaterAcumaticaVersions)]
		private void ProcessPayByLineCreditMemoAdjustment(ARRegister paymentRegister, ARAdjust adj)
		{
			var curyAdjBal = adj.CuryAdjgAmt;
			var adjBal = adj.AdjAmt;

			IEnumerable<PXResult<ARTran>> transactions = PXSelect<ARTran,
				Where<ARTran.tranType, Equal<Required<ARRegister.docType>>,
					And<ARTran.refNbr, Equal<Required<ARRegister.refNbr>>>>,
				OrderBy<Asc<ARTran.lineNbr>>>
				.Select(this, paymentRegister.DocType, paymentRegister.RefNbr);
			if (curyAdjBal < 0)
			{
				transactions = transactions.OrderByDescending(_ => PXResult.Unwrap<ARTran>(_).LineNbr);
			}

			foreach (ARTran tran in transactions)
			{
				ARTran tranCopy = PXCache<ARTran>.CreateCopy(tran);

				decimal? curyDelta = adj.Voided != true && tranCopy.CuryTranBal <= curyAdjBal
					? tranCopy.CuryTranBal
					: adj.Voided == true && tranCopy.CuryTranBal - tranCopy.CuryOrigTranAmt > curyAdjBal
						? tranCopy.CuryTranBal - tranCopy.CuryOrigTranAmt
						: curyAdjBal;

				decimal? delta = adj.Voided != true && tranCopy.TranBal <= adjBal
					? tranCopy.TranBal
					: adj.Voided == true && tranCopy.TranBal - tranCopy.OrigTranAmt > adjBal
						? tranCopy.TranBal - tranCopy.OrigTranAmt
						: adjBal;

				curyAdjBal -= curyDelta;
				adjBal -= delta;
				tranCopy.CuryTranBal -= curyDelta;
				tranCopy.TranBal -= delta;
				ARTran_TranType_RefNbr.Update(tranCopy);

				if (curyAdjBal == 0m) break;
			}
		}

		protected virtual void ProcessSVATClosingAdjustments(JournalEntry je, ARAdjust adj, ARInvoice adjddoc, ARRegister adjgdoc, CurrencyInfo new_info)
		{
			bool isAppliedPrepaymentInvoice = adjgdoc.IsPrepaymentInvoiceDocument();

			if (isAppliedPrepaymentInvoice && _IsIntegrityCheck == false)
			{
				decimal? ratio = 0m;
				if ((adjgdoc.CuryDocBal + adj.CuryAdjgAmt) == 0m)
					ratio = 1;
				else
					ratio = adj.CuryAdjdAmt / (adjgdoc.CuryDocBal + adj.CuryAdjgAmt);

				foreach (SVATConversionHist histSVAT1 in PXSelectGroupBy<SVATConversionHist,
					Where<SVATConversionHist.adjdDocType, Equal<Current<ARAdjust.adjgDocType>>,
						And<SVATConversionHist.adjdRefNbr, Equal<Current<ARAdjust.adjgRefNbr>>,
						And<SVATConversionHist.adjdDocType, NotEqual<SVATConversionHist.adjgDocType>,
						And<SVATConversionHist.adjdRefNbr, NotEqual<SVATConversionHist.adjgRefNbr>>>>>,
					Aggregate<GroupBy<SVATConversionHist.taxID,
						Sum<SVATConversionHist.taxAmtBalance,
						Sum<SVATConversionHist.curyTaxAmtBalance,
						Sum<SVATConversionHist.taxableAmtBalance,
						Sum<SVATConversionHist.curyTaxableAmtBalance>>>>>>>
					.SelectMultiBound(this, new object[] { adj }))
				{
					SVATConversionHistExt histSVAT = Common.Utilities.Clone<SVATConversionHist, SVATConversionHistExt>(this, histSVAT1);
					IEnumerable <PXResult<SVATTaxTran>> taxTranSelection = ProcessOutputSVAT.GetSVATTaxTrans(je, histSVAT, adj.AdjdRefNbr);

					foreach (PXResult<SVATTaxTran, CM.CurrencyInfo, Tax> res in taxTranSelection)
					{
						SVATTaxTran taxtran = res;
						CM.CurrencyInfo info = res;
						Tax tax = res;

						CM.CurrencyInfo new_info_tax = PXCache<CM.CurrencyInfo>.CreateCopy(info);
						new_info_tax.CuryInfoID = null;
						new_info_tax.ModuleCode = BatchModule.AR;
						new_info_tax.BaseCalc = false;
						new_info_tax = je.currencyinfo.Insert(new_info_tax) ?? new_info_tax;

						bool drCr = (ReportTaxProcess.GetMult(taxtran) == 1m);

						decimal? curyTaxableAmt = ratio * histSVAT.CuryTaxableAmtBalance;
						decimal? taxableAmt = ratio * histSVAT.TaxableAmtBalance;
						decimal? curyTaxAmt = ratio * histSVAT.CuryTaxAmtBalance;
						decimal? taxAmt = ratio * histSVAT.TaxAmtBalance;

						#region reverse original transaction
						{
							GLTran tran = new GLTran();
							tran.Module = BatchModule.AR;
							tran.BranchID = taxtran.BranchID;
							tran.AccountID = taxtran.AccountID;
							tran.SubID = taxtran.SubID;
							tran.CuryDebitAmt = drCr ? curyTaxAmt : 0m;
							tran.DebitAmt = drCr ? taxAmt : 0m;
							tran.CuryCreditAmt = drCr ? 0m : curyTaxAmt;
							tran.CreditAmt = drCr ? 0m : taxAmt;
							tran.TranType = taxtran.TranType;
							tran.TranClass = GLTran.tranClass.Normal;
							tran.RefNbr = taxtran.RefNbr;
							tran.TranDesc = taxtran.TaxInvoiceNbr;
							tran.TranPeriodID = adj.AdjgFinPeriodID;
							tran.FinPeriodID = adj.AdjgFinPeriodID;
							tran.TranDate = taxtran.TaxInvoiceDate;
							tran.CuryInfoID = new_info_tax.CuryInfoID;
							tran.ReferenceID = adj.CustomerID;
							tran.Released = true;

							je.GLTranModuleBatNbr.Insert(tran);
						}
						#endregion

						#region reclassify to VAT account
						{
							GLTran tran = new GLTran();
							tran.Module = BatchModule.AR;
							tran.BranchID = taxtran.BranchID;
							tran.AccountID = tax.PendingSalesTaxAcctID;
							tran.SubID = tax.PendingSalesTaxSubID;
							tran.CuryDebitAmt = drCr ? 0m : curyTaxAmt;
							tran.DebitAmt = drCr ? 0m : taxAmt;
							tran.CuryCreditAmt = drCr ? curyTaxAmt : 0m;
							tran.CreditAmt = drCr ? taxAmt : 0m;
							tran.TranType = taxtran.TranType;
							tran.TranClass = GLTran.tranClass.Normal;
							tran.RefNbr = taxtran.RefNbr;
							tran.TranDesc = taxtran.TaxInvoiceNbr;
							tran.TranPeriodID = adj.AdjgFinPeriodID;
							tran.FinPeriodID = adj.AdjgFinPeriodID;
							tran.TranDate = taxtran.TaxInvoiceDate;
							tran.CuryInfoID = new_info_tax.CuryInfoID;
							tran.ReferenceID = adj.CustomerID;
							tran.Released = true;

							je.GLTranModuleBatNbr.Insert(tran);

							PXCache dummycache = je.Caches[typeof(TaxTran)];
							dummycache = je.Caches[typeof(SVATTaxTran)];
							je.Views.Caches.Add(typeof(SVATTaxTran));

							SVATTaxTran newtaxtran = PXCache<SVATTaxTran>.CreateCopy(taxtran);
							newtaxtran.RecordID = null;
							newtaxtran.Module = BatchModule.AR;
							newtaxtran.TaxType = TaxType.Recognition;
							newtaxtran.AccountID = tax.SalesTaxAcctID;
							newtaxtran.SubID = tax.SalesTaxSubID;
							newtaxtran.FinDate = null;

							decimal tranSign = (-1m) * ReportTaxProcess.GetMult(taxtran) * ReportTaxProcess.GetMult(newtaxtran);
							newtaxtran.CuryTaxableAmt = tranSign * curyTaxableAmt;
							newtaxtran.TaxableAmt = tranSign * taxableAmt;
							newtaxtran.CuryTaxAmt = tranSign * curyTaxAmt;
							newtaxtran.TaxAmt = tranSign * taxAmt;

							PXCache taxtranCache = je.Caches[typeof(SVATTaxTran)];
							taxtranCache.Insert(newtaxtran);
						}
						#endregion
					}
				}
			}
		}

		protected virtual void ProcessSVATAdjustments(JournalEntry je, ARAdjust adj, ARInvoice adjddoc, ARRegister adjgdoc, CurrencyInfo new_info)
		{
			if (adjddoc.IsPrepaymentInvoiceDocumentReverse())
				return;

			if (_IsIntegrityCheck == false
				&& (PXAccess.FeatureInstalled<FeaturesSet.vATReporting>()
					|| adjddoc.IsPrepaymentInvoiceDocument()))
			{
				foreach (SVATConversionHist docSVAT in PXSelect<SVATConversionHist, Where<
					SVATConversionHist.module, Equal<BatchModule.moduleAR>,
					And2<Where<SVATConversionHist.adjdDocType, Equal<Current<ARAdjust.adjdDocType>>,
						And<SVATConversionHist.adjdRefNbr, Equal<Current<ARAdjust.adjdRefNbr>>,
						Or<SVATConversionHist.adjdDocType, Equal<Current<ARAdjust.adjgDocType>>,
						And<SVATConversionHist.adjdRefNbr, Equal<Current<ARAdjust.adjgRefNbr>>>>>>,
					And<SVATConversionHist.reversalMethod, In3<SVATTaxReversalMethods.onPayments, SVATTaxReversalMethods.onPrepayment>,
					And<Where<SVATConversionHist.adjdDocType, Equal<SVATConversionHist.adjgDocType>,
						And<SVATConversionHist.adjdRefNbr, Equal<SVATConversionHist.adjgRefNbr>>>>>>>>
					.SelectMultiBound(this, new object[] { adj }))
				{
					bool isPayment = adj.AdjgDocType == docSVAT.AdjdDocType && adj.AdjgRefNbr == docSVAT.AdjdRefNbr;
					decimal percent = isPayment
						? ((adj.CuryAdjgAmt ?? 0m) + (adj.CuryAdjgDiscAmt ?? 0m) + (adj.CuryAdjgWOAmt ?? 0m)) / (adjgdoc.CuryOrigDocAmt ?? 0m)
						: ((adj.CuryAdjdAmt ?? 0m) + (adj.CuryAdjdDiscAmt ?? 0m) + (adj.CuryAdjdWOAmt ?? 0m)) / (adjddoc.CuryOrigDocAmt ?? 0m);

					SVATConversionHist adjSVAT = new SVATConversionHist
					{
						Module = BatchModule.AR,
						AdjdBranchID = adj.AdjdBranchID,
						AdjdDocType = isPayment ? adj.AdjgDocType : adj.AdjdDocType,
						AdjdRefNbr = isPayment ? adj.AdjgRefNbr : adj.AdjdRefNbr,
						AdjdLineNbr = adj.AdjdLineNbr,
						AdjgDocType = isPayment ? adj.AdjdDocType : adj.AdjgDocType,
						AdjgRefNbr = isPayment ? adj.AdjdRefNbr : adj.AdjgRefNbr,
						AdjNbr = adj.AdjNbr,
						AdjdDocDate = adj.AdjgDocDate,

						TaxID = docSVAT.TaxID,
						TaxType = docSVAT.TaxType,
						TaxRate = docSVAT.TaxRate,
						VendorID = docSVAT.VendorID,
						ReversalMethod = (adjddoc.IsPrepaymentInvoiceDocument() || adjgdoc.IsPrepaymentInvoiceDocument())
										? SVATTaxReversalMethods.OnPrepayment
										: SVATTaxReversalMethods.OnPayments,

						CuryInfoID = docSVAT.CuryInfoID,
					};

					adjSVAT.FillAmounts(GetExtension<MultiCurrency>().GetCurrencyInfo(docSVAT.CuryInfoID), docSVAT.CuryTaxableAmt, docSVAT.CuryTaxAmt, percent);

					FinPeriodIDAttribute.SetPeriodsByMaster<SVATConversionHist.adjdFinPeriodID>(SVATConversionHistory.Cache, adjSVAT, adj.AdjdTranPeriodID);


					ARRegister adjdoc = isPayment ? adjgdoc : adjddoc;
					ARRegister cachedDoc = (ARRegister)ARDocument.Cache.Locate(adjdoc);
					if (cachedDoc != null)
					{
						PXCache<ARRegister>.RestoreCopy(adjdoc, cachedDoc);
					}

					if (adjdoc.CuryDocBal == 0m && adjddoc.IsMigratedRecord != true)
					{
						bool isPartialApplication = percent != 1m;

						adjSVAT.CuryTaxableAmt = docSVAT.CuryTaxableAmt;
						adjSVAT.TaxableAmt = docSVAT.TaxableAmt;
						adjSVAT.CuryTaxAmt = docSVAT.CuryTaxAmt;
						adjSVAT.TaxAmt = docSVAT.TaxAmt;

						if (isPartialApplication)
					{
						var rows = PXSelect<SVATConversionHist, Where<
							SVATConversionHist.module, Equal<BatchModule.moduleAR>,
							And<SVATConversionHist.adjdDocType, Equal<Current<SVATConversionHist.adjdDocType>>,
							And<SVATConversionHist.adjdRefNbr, Equal<Current<SVATConversionHist.adjdRefNbr>>,
							And<SVATConversionHist.taxID, Equal<Current<SVATConversionHist.taxID>>,
							And<Where<SVATConversionHist.adjdDocType, NotEqual<SVATConversionHist.adjgDocType>,
								Or<SVATConversionHist.adjdRefNbr, NotEqual<SVATConversionHist.adjgRefNbr>>>>>>>>>
							.SelectMultiBound(this, new object[] { docSVAT }).AsEnumerable();
						if (rows.Any())
					{
							foreach (SVATConversionHist row in rows)
					{
								adjSVAT.CuryTaxableAmt -= (row.CuryTaxableAmt ?? 0m);
								adjSVAT.TaxableAmt -= (row.TaxableAmt ?? 0m);
								adjSVAT.CuryTaxAmt -= (row.CuryTaxAmt ?? 0m);
								adjSVAT.TaxAmt -= (row.TaxAmt ?? 0m);
					}
							}
						}

							adjSVAT.CuryUnrecognizedTaxAmt = adjSVAT.CuryTaxAmt;
							adjSVAT.UnrecognizedTaxAmt = adjSVAT.TaxAmt;
				}

					adjSVAT = (SVATConversionHist)SVATConversionHistory.Cache.Insert(adjSVAT);

					docSVAT.Processed = false;
					docSVAT.AdjgFinPeriodID = null;

					PXTimeStampScope.PutPersisted(SVATConversionHistory.Cache, docSVAT, PXDatabase.SelectTimeStamp());
					SVATConversionHistory.Cache.Update(docSVAT);

					if (_IsIntegrityCheck == false && adjddoc.IsPrepaymentInvoiceDocument())
					{
						bool isDebit = (adjddoc.DrCr == DrCr.Debit);
						Tax tax = Tax.PK.Find(this, adjSVAT.TaxID);

						if (tax.PendingSalesTaxAcctID == null)
						{
							throw new ReleaseException(AR.Messages.CannotReleasePrepaymentInvoiceWithEmptyPendingTaxPayableAccount, tax.TaxID);
						}

						if (PXAccess.FeatureInstalled<FeaturesSet.subAccount>() && tax.PendingSalesTaxSubID == null)
						{
							throw new ReleaseException(AR.Messages.CannotReleasePrepaymentInvoiceWithEmptyPendingTaxPayableSubaccount, tax.TaxID);
						}

						GLTran taxTran = new GLTran();
						taxTran.SummPost = this.SummPost;
						taxTran.BranchID = adjSVAT.AdjdBranchID;
						taxTran.CuryInfoID = adjSVAT.CuryInfoID;
						taxTran.TranType = adjSVAT.AdjdDocType;
						taxTran.TranClass = GLTran.tranClass.Tax;
						taxTran.RefNbr = adjSVAT.AdjdRefNbr;
						taxTran.TranDate = adjSVAT.AdjdDocDate;
						taxTran.AccountID = isDebit ? tax.PendingSalesTaxAcctID : tax.SalesTaxAcctID;
						taxTran.SubID = isDebit ? tax.PendingSalesTaxSubID : tax.SalesTaxSubID;
						taxTran.TranDesc = tax.TaxID;
						taxTran.CuryDebitAmt = isDebit ? adjSVAT.CuryTaxAmt : 0m;
						taxTran.DebitAmt = isDebit ? adjSVAT.TaxAmt : 0m;
						taxTran.CuryCreditAmt = isDebit ? 0m : adjSVAT.CuryTaxAmt;
						taxTran.CreditAmt = isDebit ? 0m : adjSVAT.TaxAmt;
						taxTran.Released = true;
						taxTran.ReferenceID = adjddoc.CustomerID;

						Account taxAccount = Account.PK.Find(this, tax.PendingSalesTaxAcctID);
						SetProjectAndTaxID(taxTran, taxAccount, adjddoc);

						InsertInvoiceTaxTransaction(je, taxTran,
							new GLTranInsertionContext { ARRegisterRecord = adjddoc});

						GLTran pendingTaxTran = (GLTran)je.GLTranModuleBatNbr.Cache.CreateCopy(taxTran);
						pendingTaxTran.AccountID = isDebit ? tax.SalesTaxAcctID : tax.PendingSalesTaxAcctID;
						pendingTaxTran.SubID = isDebit ? tax.SalesTaxSubID : tax.PendingSalesTaxSubID;
						pendingTaxTran.CuryDebitAmt = isDebit ? 0m : adjSVAT.CuryTaxAmt;
						pendingTaxTran.DebitAmt = isDebit ? 0m : adjSVAT.TaxAmt;
						pendingTaxTran.CuryCreditAmt = isDebit ? adjSVAT.CuryTaxAmt : 0m;
						pendingTaxTran.CreditAmt = isDebit ? adjSVAT.TaxAmt : 0m;

						InsertInvoiceTaxTransaction(je, pendingTaxTran,
							new GLTranInsertionContext { ARRegisterRecord = adjddoc });

						PXCache dummycache = je.Caches[typeof(TaxTran)];
						dummycache = je.Caches[typeof(SVATTaxTran)];
						je.Views.Caches.Add(typeof(SVATTaxTran));

						SVATConversionHistExt histSVAT = Common.Utilities.Clone<SVATConversionHist, SVATConversionHistExt>(this, docSVAT);
						IEnumerable<PXResult<SVATTaxTran>> taxTranSelection = ProcessOutputSVAT.GetSVATTaxTrans(je, histSVAT, adj.AdjdRefNbr);
						SVATTaxTran svatTaxTran = taxTranSelection.First();

						SVATTaxTran newtaxtran = PXCache<SVATTaxTran>.CreateCopy(svatTaxTran);
						newtaxtran.RecordID = null;
						newtaxtran.Module = BatchModule.AR;
						newtaxtran.TaxType = TaxType.Recognition;
						newtaxtran.AccountID = tax.SalesTaxAcctID;
						newtaxtran.SubID = tax.SalesTaxSubID;
						newtaxtran.FinDate = null;

						decimal tranSign = ReportTaxProcess.GetMult(svatTaxTran) * ReportTaxProcess.GetMult(newtaxtran);
						newtaxtran.CuryTaxableAmt = tranSign * adjSVAT.CuryTaxableAmt;
						newtaxtran.TaxableAmt = tranSign * adjSVAT.TaxableAmt;
						newtaxtran.CuryTaxAmt = tranSign * adjSVAT.CuryTaxAmt;
						newtaxtran.TaxAmt = tranSign * adjSVAT.TaxAmt;

						PXCache taxtranCache = je.Caches[typeof(SVATTaxTran)];
						taxtranCache.Insert(newtaxtran);

						if (adjgdoc.IsPrepaymentInvoiceDocumentReverse())
						{
							svatTaxTran.CuryAdjustedTaxableAmt += newtaxtran.CuryTaxableAmt;
							svatTaxTran.AdjustedTaxableAmt += newtaxtran.TaxableAmt;
							svatTaxTran.CuryAdjustedTaxAmt += newtaxtran.CuryTaxAmt;
							svatTaxTran.AdjustedTaxAmt += newtaxtran.TaxAmt;
							taxtranCache.Update(svatTaxTran);
						}
					}
				}
			}
		}

		protected virtual void ProcessAdjustmentsOnlyAdjusted(JournalEntry je, PXResultset<ARAdjust> adjustments)
		{
			foreach (var group in adjustments
				.AsEnumerable()
				.Where(res => ((ARAdjust)res).IsInitialApplication != true)
				.Cast<PXResult<ARAdjust, CurrencyInfo, Currency, ARInvoice, ARPayment, ARRegister>>()
				.GroupBy(res => (ARRegister)res, ARDocument.Cache.GetComparer()))
			{
				ARRegister adjddoc = (ARRegister)group.Key;
				ARRegister adjustedDocument = null;
				bool verifyAdjustedDocument = false;

				// All special applications, which have been created in migration mode
				// for migrated document - should be excluded from the processing
				//
				foreach (PXResult<ARAdjust, CurrencyInfo, Currency, ARInvoice, ARPayment, ARRegister> adjres in group)
				{
					ARAdjust adj = adjres;
					CurrencyInfo vouch_info = adjres;
					Currency cury = adjres;
					ARInvoice adjustedInvoice = adjres;
					ARPayment adjgdoc = adjres;

					// Restore full invoice from the "single table" stripped version.
					// 
					if (adjustedInvoice?.RefNbr != null)
					{
						PXCache<ARRegister>.RestoreCopy(adjustedInvoice, (ARRegister)adjres);
						adjustedDocument = adjustedInvoice;
					}
					else if (adjgdoc?.RefNbr != null)
					{
						adjustedDocument = adjgdoc;
					}

					if (adj.CuryAdjgAmt == 0m && adj.CuryAdjgDiscAmt == 0m)
					{
						ARAdjust_AdjgDocType_RefNbr_CustomerID.Cache.Delete(adj);
						continue;
					}

					if (adj.Hold == true)
					{
						throw new PXException(Messages.Document_OnHold_CannotRelease);
					}

					if (adjddoc.RefNbr != null) // like in ProcessAdjustments
					{
						verifyAdjustedDocument = true;

						if (_IsIntegrityCheck == false && adjgdoc.Released == true && adjgdoc.AdjDate < adjddoc.DocDate)
						{
							throw new PXException(Messages.ApplDate_Less_DocDate,
								PXUIFieldAttribute.GetDisplayName<ARPayment.adjDate>(ARPayment_DocType_RefNbr.Cache));
						}

						UpdateBalances(adj, adjddoc);
						UpdateARBalances(adj, adjddoc);

						_oldInvoiceRefresher.RecordDocument(adjddoc.BranchID, adjddoc.CustomerID, adjddoc.CustomerLocationID);
					}

					CurrencyInfo payment_info = GetExtension<MultiCurrency>().GetCurrencyInfo(adjgdoc.CuryInfoID);

					ProcessAdjustmentAdjusted(je, adj, adjddoc, adjgdoc, vouch_info, payment_info);
					ProcessAdjustmentTranPost(adj, adjddoc, adjgdoc, true);
				}

				if (verifyAdjustedDocument)
				{
					VerifyAdjustedDocumentAndClose(adjustedDocument);
				}
			}
		}
		
		private void ProcessAdjustmentSalesPersonCommission(ARAdjust adj, ARPayment payment, ARRegister adjustedInvoice, Currency cury, ARTran tran)
		{
			if (payment.DocType != ARDocType.CreditMemo) //Credit memos are treates as negative invoice
			{
				if (adjustedInvoice.PaymentsByLinesAllowed == true)
				{
					PXResult<ARSalesPerTran,CurrencyInfo> resInvoice = 	
						(PXResult<ARSalesPerTran,CurrencyInfo>)PXSelectJoin<ARSalesPerTran,
							LeftJoin<CurrencyInfo, On<CurrencyInfo.curyInfoID, Equal<ARSalesPerTran.curyInfoID>>>,
						Where<ARSalesPerTran.docType, Equal<Required<ARSalesPerTran.docType>>,
							And<ARSalesPerTran.refNbr, Equal<Required<ARSalesPerTran.refNbr>>,
							And<ARSalesPerTran.salespersonID, Equal<Required<ARSalesPerTran.salespersonID>>>>>>
					.SelectSingleBound(this, null, adjustedInvoice.DocType, adjustedInvoice.RefNbr, tran.SalesPersonID);
					ARSalesPerTran invoiceSPT = resInvoice;

					if (invoiceSPT == null)
						return;

					PXResult<ARSalesPerTran,CurrencyInfo> resPayment = 
						(PXResult<ARSalesPerTran,CurrencyInfo>)PXSelectJoin<ARSalesPerTran,
							LeftJoin<CurrencyInfo, On<CurrencyInfo.curyInfoID, Equal<ARSalesPerTran.curyInfoID>>>,
						Where<ARSalesPerTran.docType, Equal<Required<ARSalesPerTran.docType>>,
							And<ARSalesPerTran.refNbr, Equal<Required<ARSalesPerTran.refNbr>>,
							And<ARSalesPerTran.salespersonID, Equal<Required<ARSalesPerTran.salespersonID>>,
							And<ARSalesPerTran.adjNbr, Equal<Required<ARSalesPerTran.adjNbr>>,
							And<ARSalesPerTran.adjdDocType, Equal<Required<ARSalesPerTran.adjdDocType>>,
							And<ARSalesPerTran.adjdRefNbr, Equal<Required<ARSalesPerTran.adjdRefNbr>>>>>>>>>
							.Select(this, payment.DocType, payment.RefNbr, tran.SalesPersonID, adj.AdjNbr, adj.AdjdDocType, adj.AdjdRefNbr);
					ARSalesPerTran paymentSPT = resPayment;
					if (paymentSPT == null)
					{
						paymentSPT = CreatePaymentSPT(payment, adj, invoiceSPT);
						GetExtension<MultiCurrency>().StoreResult(resInvoice);
						paymentSPT.CuryCommnblAmt = adj.CuryAdjdAmt + adj.CuryAdjdDiscAmt;
						paymentSPT.CuryCommnAmt = paymentSPT.CuryCommnblAmt * invoiceSPT.CommnPct / 100;
						paymentSPT = this.ARDoc_SalesPerTrans.Insert(paymentSPT);
					}
					else
					{
						PXCache<ARSalesPerTran>.StoreOriginal(this, paymentSPT);
						GetExtension<MultiCurrency>().StoreResult(resPayment);
					paymentSPT = PXCache<ARSalesPerTran>.CreateCopy(paymentSPT);
					paymentSPT.CuryCommnblAmt += adj.CuryAdjdAmt + adj.CuryAdjdDiscAmt;
					paymentSPT.CuryCommnAmt = paymentSPT.CuryCommnblAmt * invoiceSPT.CommnPct / 100;
					this.ARDoc_SalesPerTrans.Update(paymentSPT);
					}
				}
				else
				{
					foreach (ARSalesPerTran iSPT in this.ARDoc_SalesPerTrans.Select(adjustedInvoice.DocType, adjustedInvoice.RefNbr))
					{
						ARSalesPerTran paySPT = CreatePaymentSPT(payment, adj, iSPT);

					decimal applRatio = ((adj.CuryAdjdAmt + adj.CuryAdjdDiscAmt) / (tran?.CuryOrigTranAmt ?? adjustedInvoice.CuryOrigDocAmt)).Value;
					if (payment.DocType == ARDocType.CashSale || payment.DocType == ARDocType.CashReturn)
					{
						applRatio = 1m;
					}
					CopyShare(paySPT, iSPT, applRatio, (cury.DecimalPlaces ?? 2));
						paySPT = this.ARDoc_SalesPerTrans.Insert(paySPT);
					}
				}
			}
				}

		private void ProcessAdjustmentAdjusting(JournalEntry je, ARAdjust adj, ARPayment payment, ARRegister adjustedDocument, Customer paymentCustomer, CurrencyInfo new_info)
		{
			bool isSmallCreditWO = adj.AdjdDocType == ARDocType.SmallCreditWO;
			bool isPrepaymentInvoice = adj.AdjgDocType == ARDocType.PrepaymentInvoice;
			bool isAppliedToPrepaymentInvoice = adj.AdjdDocType == ARDocType.PrepaymentInvoice;
			bool isTheSameAcc = adjustedDocument.ARAccountID == payment.ARAccountID && adj.AdjgDocType == ARDocType.Prepayment;
			bool isPrepaymentInvoiceReverse = adj.AdjdDocType == ARDocType.PrepaymentInvoice && payment.OrigDocType == ARDocType.PrepaymentInvoice; // PPI reversed by CRM;

			string tranClass;
			if (adj.IsOrigSmallCreditWOApp())
			{
				tranClass = GLTran.tranClass.Normal;
			}
			else if (isAppliedToPrepaymentInvoice)
			{
				tranClass = (isTheSameAcc && !isPrepaymentInvoiceReverse) ? GLTran.tranClass.ZeroRecord : GLTran.tranClass.PrepaymentInvoice;
			}
			else
			{
				tranClass = GLTran.tranClass.Payment;
			}

			GLTran tran = new GLTran
			{
				SummPost = true,
				ZeroPost = false,
				BranchID = adj.AdjgBranchID,
				AccountID = (isPrepaymentInvoice || isPrepaymentInvoiceReverse) ? payment.PrepaymentAccountID : payment.ARAccountID,
				ReclassificationProhibited = true,
				SubID = (isPrepaymentInvoice || isPrepaymentInvoiceReverse) ? payment.PrepaymentSubID : payment.ARSubID,
				DebitAmt = (adj.AdjgGLSign == 1m) ? adj.AdjAmt : 0m,
				CuryDebitAmt = (adj.AdjgGLSign == 1m) ? adj.CuryAdjgAmt : 0m,
				CreditAmt = (adj.AdjgGLSign == 1m) ? 0m : adj.AdjAmt,
				CuryCreditAmt = (adj.AdjgGLSign == 1m) ? 0m : adj.CuryAdjgAmt,
				TranType = adj.IsOrigSmallCreditWOApp() ? adj.AdjdDocType : adj.AdjgDocType,
				TranClass = tranClass,
				RefNbr = adj.IsOrigSmallCreditWOApp() ? adj.AdjdRefNbr : adj.AdjgRefNbr,
				TranDesc = adj.IsOrigSmallCreditWOApp() ? adjustedDocument.DocDesc : payment.DocDesc,
				TranDate = adj.AdjgDocDate
			};
			FinPeriodIDAttribute.SetPeriodsByMaster<GLTran.finPeriodID>(je.GLTranModuleBatNbr.Cache, tran, adj.AdjgTranPeriodID);
				tran.CuryInfoID = new_info.CuryInfoID;
				tran.Released = true;
			tran.ReferenceID = payment.CustomerID;
			tran.OrigAccountID = adj.IsOrigSmallCreditWOApp() ? null : adj.AdjdARAcct;
			tran.OrigSubID = adj.IsOrigSmallCreditWOApp() ? null : adj.AdjdARSub;
			tran.ProjectID = ProjectDefaultAttribute.NonProject();

			UpdateHistory(tran, paymentCustomer);
			UpdateHistory(tran, paymentCustomer, new_info);
			
			if (isSmallCreditWO)
				{
					bool isPrepayment = (adj.AdjgDocType == ARDocType.Prepayment);
					if (adj.AdjgDocType == ARDocType.VoidPayment)
					{
						ARRegister orig = PXSelect<ARRegister, Where<ARRegister.refNbr, Equal<Required<ARRegister.refNbr>>,
							And<Where<ARRegister.docType, Equal<ARDocType.payment>,
							Or<ARRegister.docType, Equal<ARDocType.prepayment>>>>>,
							OrderBy<Asc<Switch<Case<Where<ARRegister.docType, Equal<ARDocType.payment>>, int0>, int1>, Asc<ARRegister.docType, Asc<ARRegister.refNbr>>>>>.Select(this, tran.RefNbr);
						isPrepayment = (orig != null && orig.DocType == ARDocType.Prepayment);
					}

					if (isPrepayment)
					{
						ARHistBucket bucket = new ARHistBucket();
						bucket.arAccountID = tran.AccountID;
						bucket.arSubID = tran.SubID;
						bucket.SignPayments = 1m;
						bucket.SignDeposits = -1m;
						bucket.SignPtd = -1m;

					UpdateHistory(tran, paymentCustomer, bucket);
					UpdateHistory(tran, paymentCustomer, new_info, bucket);
					}
				}

			InsertAdjustmentsAdjustingTransaction(je, tran, 
				new GLTranInsertionContext { ARRegisterRecord = payment, ARAdjustRecord = adj });
		}

		private void ProcessAdjustmentAdjusted(JournalEntry je, ARAdjust adj, ARRegister adjustedDocument, ARPayment payment, CurrencyInfo vouch_info, CurrencyInfo new_info)
		{
			bool isSmallCreditWO = adj.AdjdDocType == ARDocType.SmallCreditWO;
			bool isAppliedToPrepaymentInvoice = adj.AdjdDocType == ARDocType.PrepaymentInvoice;
			bool isTheSameAcc = adjustedDocument.ARAccountID == payment.ARAccountID;

			Customer voucherCustomer = PXSelect<Customer, Where<Customer.bAccountID, Equal<Required<Customer.bAccountID>>>>.Select(this, adj.AdjdCustomerID);

			string tranClass;
			if (adj.IsOrigSmallCreditWOApp())
			{
				tranClass = GLTran.tranClass.Payment;
			}
			else if (isAppliedToPrepaymentInvoice)
			{
				tranClass = isTheSameAcc? GLTran.tranClass.ZeroRecord : GLTran.tranClass.PrepaymentInvoice;
			}
			else
			{
				tranClass = ARDocType.DocClass(adj.AdjdDocType);
			}

			//Credit Voucher AR Account/minus RGOL for refund
			var tran = new GLTran();
			tran.SummPost = true;
			tran.ZeroPost = false;
			tran.BranchID = adj.AdjdBranchID;
			//Small-Credit has Payment AR Account in AdjdARAcct  and WO Account in ARAccountID
			tran.AccountID = isSmallCreditWO ? adjustedDocument.ARAccountID : adj.AdjdARAcct;
			tran.ReclassificationProhibited = true;
			tran.SubID = isSmallCreditWO ? adjustedDocument.ARSubID : adj.AdjdARSub;
			//Small-Credit reversal should update history for payment AR Account(AdjdARAcct)
			tran.OrigAccountID = adj.AdjdARAcct;
			tran.OrigSubID = adj.AdjdARSub;
			tran.CreditAmt = (adj.AdjgGLSign == 1m) ? adj.AdjAmt + adj.AdjDiscAmt + adj.AdjWOAmt + adj.RGOLAmt : 0m;
			tran.CuryCreditAmt = (adj.AdjgGLSign == 1m) ? (object.Equals(new_info.CuryID, new_info.BaseCuryID) ? tran.CreditAmt : adj.CuryAdjgAmt + adj.CuryAdjgDiscAmt + adj.CuryAdjgWOAmt) : 0m;
			tran.DebitAmt = (adj.AdjgGLSign == 1m) ? 0m : adj.AdjAmt + adj.AdjDiscAmt + adj.AdjWOAmt - adj.RGOLAmt;
			tran.CuryDebitAmt = (adj.AdjgGLSign == 1m) ? 0m : (object.Equals(new_info.CuryID, new_info.BaseCuryID) ? tran.DebitAmt : adj.CuryAdjgAmt + adj.CuryAdjgDiscAmt + adj.CuryAdjgWOAmt);
			//tran.TranType = tranType;
			tran.TranType = adj.IsOrigSmallCreditWOApp() ? adj.AdjdDocType : adj.AdjgDocType;
			//always N for AdjdDocs except Prepayment
			tran.TranClass = tranClass;
			tran.RefNbr = adj.IsOrigSmallCreditWOApp() ? adj.AdjdRefNbr : adj.AdjgRefNbr;
			tran.TranDesc = adj.IsOrigSmallCreditWOApp() ? adjustedDocument.DocDesc : payment.DocDesc;
			tran.TranDate = adj.AdjgDocDate;
			FinPeriodIDAttribute.SetPeriodsByMaster<GLTran.finPeriodID>(
				je.GLTranModuleBatNbr.Cache, tran, adj.AdjgTranPeriodID);
			tran.CuryInfoID = new_info.CuryInfoID;
			tran.Released = true;
			tran.ReferenceID = payment.CustomerID;
			tran.ProjectID = PM.ProjectDefaultAttribute.NonProject();

			UpdateHistory(tran, voucherCustomer);

			InsertAdjustmentsAdjustedTransaction(je, tran, 
				new GLTranInsertionContext { ARRegisterRecord = payment, ARAdjustRecord = adj });

			//Update CuryHistory in Voucher currency
			tran.CuryCreditAmt = (adj.AdjgGLSign == 1m) ? (object.Equals(vouch_info.CuryID, vouch_info.BaseCuryID) ? tran.CreditAmt : adj.CuryAdjdAmt + adj.CuryAdjdDiscAmt + adj.CuryAdjdWOAmt) : 0m;
			tran.CuryDebitAmt = (adj.AdjgGLSign == 1m) ? 0m : (object.Equals(vouch_info.CuryID, vouch_info.BaseCuryID) ? tran.DebitAmt : adj.CuryAdjdAmt + adj.CuryAdjdDiscAmt + adj.CuryAdjdWOAmt);
			UpdateHistory(tran, voucherCustomer, vouch_info);
		}

		private void PostReduceOnEarlyPaymentTran(JournalEntry je, ARInvoice ardoc, decimal? curyAmount, decimal? amount, Customer customer, CurrencyInfo currencyInfo, bool isDebit)
		{
			GLTran tran = new GLTran();
			tran.SummPost = this.SummPost;
			tran.BranchID = ardoc.BranchID;
			tran.AccountID = customer.DiscTakenAcctID;
			tran.SubID = customer.DiscTakenSubID;
			tran.OrigAccountID = ardoc.ARAccountID;
			tran.OrigSubID = ardoc.ARSubID;

			tran.DebitAmt = isDebit ? amount : 0m;
			tran.CuryDebitAmt = isDebit ? curyAmount : 0m;
			tran.CreditAmt = isDebit ? 0m : amount;
			tran.CuryCreditAmt = isDebit ? 0m : curyAmount;
			tran.TranType = ardoc.DocType;
			tran.TranClass = GLTran.tranClass.Discount;
			tran.RefNbr = ardoc.RefNbr;
			tran.TranDesc = ardoc.DocDesc;
			tran.TranDate = ardoc.DocDate;
			FinPeriodIDAttribute.SetPeriodsByMaster<GLTran.finPeriodID>(
				je.GLTranModuleBatNbr.Cache, tran, ardoc.TranPeriodID);
			tran.CuryInfoID = currencyInfo.CuryInfoID;
			tran.Released = true;
			tran.ReferenceID = ardoc.CustomerID;
			tran.ProjectID = PM.ProjectDefaultAttribute.NonProject();				

			UpdateHistory(tran, customer);

			InsertAdjustmentsCashDiscountTransaction(je, tran, new GLTranInsertionContext { ARRegisterRecord = ardoc });

			tran.CuryDebitAmt = isDebit ? curyAmount : 0m;
			tran.CuryCreditAmt = isDebit ? 0m : curyAmount;
			UpdateHistory(tran, customer, currencyInfo);

		}

		private void ProcessAdjustmentCashDiscount(JournalEntry je, ARAdjust adj, ARPayment payment, Customer paymentCustomer, CurrencyInfo new_info, CurrencyInfo vouch_info)
		{
			//Credit Discount Taken/does not apply to refund, since no disc in AD
			var tran = new GLTran();
			tran.SummPost = this.SummPost;
			tran.BranchID = adj.AdjdBranchID;
			tran.AccountID = paymentCustomer.DiscTakenAcctID;
			tran.SubID = paymentCustomer.DiscTakenSubID;
			tran.OrigAccountID = adj.AdjdARAcct;
			tran.OrigSubID = adj.AdjdARSub;
			tran.DebitAmt = (adj.AdjgGLSign == 1m) ? adj.AdjDiscAmt : 0m;
			tran.CuryDebitAmt = (adj.AdjgGLSign == 1m) ? adj.CuryAdjgDiscAmt : 0m;
			tran.CreditAmt = (adj.AdjgGLSign == 1m) ? 0m : adj.AdjDiscAmt;
			tran.CuryCreditAmt = (adj.AdjgGLSign == 1m) ? 0m : adj.CuryAdjgDiscAmt;
			tran.TranType = adj.AdjgDocType;
			tran.TranClass = GLTran.tranClass.Discount;
			tran.RefNbr = adj.AdjgRefNbr;
			tran.TranDesc = payment.DocDesc;
			tran.TranDate = adj.AdjgDocDate;
			FinPeriodIDAttribute.SetPeriodsByMaster<GLTran.finPeriodID>(
				je.GLTranModuleBatNbr.Cache, tran, adj.AdjgTranPeriodID);
			tran.CuryInfoID = new_info.CuryInfoID;
			tran.Released = true;
			tran.ReferenceID = payment.CustomerID;
			tran.ProjectID = PM.ProjectDefaultAttribute.NonProject();

			UpdateHistory(tran, paymentCustomer);

			InsertAdjustmentsCashDiscountTransaction(je, tran, 
				new GLTranInsertionContext { ARRegisterRecord = payment, ARAdjustRecord = adj });

			//Update CuryHistory in Voucher currency
			tran.CuryDebitAmt = (adj.AdjgGLSign == 1m) ? adj.CuryAdjdDiscAmt : 0m;
			tran.CuryCreditAmt = (adj.AdjgGLSign == 1m) ? 0m : adj.CuryAdjdDiscAmt;
			UpdateHistory(tran, paymentCustomer, vouch_info);
		}

		private void ProcessAdjustmentWriteOff(JournalEntry je, ARAdjust adj, ARPayment payment, Customer paymentCustomer, CurrencyInfo new_info, CurrencyInfo vouch_info)
		{
			//Credit WO Account
			if (adj.AdjWOAmt != 0 || adj.CuryAdjgWOAmt != 0)
			{
				ARInvoice adjusted = PXSelect<ARInvoice, Where<ARInvoice.docType, Equal<Required<ARInvoice.docType>>, And<ARInvoice.refNbr, Equal<Required<ARInvoice.refNbr>>>>>.Select(pe, adj.AdjdDocType, adj.AdjdRefNbr);

				ReasonCode reasonCode = PXSelect<ReasonCode, Where<ReasonCode.reasonCodeID, Equal<Required<ReasonCode.reasonCodeID>>>>.Select(this, adj.WriteOffReasonCode);

				if (reasonCode == null)
					throw new PXException(PXMessages.LocalizeFormatNoPrefixNLA(Messages.ReasonCodeNotFound, adj.WriteOffReasonCode));

				Location customerLocation = PXSelect<Location, Where<Location.bAccountID, Equal<Required<Location.bAccountID>>,
					And<Location.locationID, Equal<Required<Location.locationID>>>>>.Select((PXGraph)pe, adjusted.CustomerID, adjusted.CustomerLocationID);

					CRLocation companyLocation = PXSelectJoin<CRLocation,
						InnerJoin<BAccountR, On<CRLocation.bAccountID, Equal<BAccountR.bAccountID>, And<CRLocation.locationID, Equal<BAccountR.defLocationID>>>,
					InnerJoin<GL.Branch, On<BAccountR.bAccountID, Equal<GL.Branch.bAccountID>>>>, Where<Branch.branchID, Equal<Required<Branch.branchID>>>>.Select((PXGraph)pe, adjusted.BranchID);

				object value = null;
				if (reasonCode.Usage == ReasonCodeUsages.BalanceWriteOff || reasonCode.Usage == ReasonCodeUsages.CreditWriteOff)
				{
					value = ReasonCodeSubAccountMaskAttribute.MakeSub<ReasonCode.subMask>((PXGraph)pe, reasonCode.SubMask,
						new object[] { reasonCode.SubID, customerLocation.CSalesSubID, companyLocation.CMPSalesSubID },
						new Type[] { typeof(ReasonCode.subID), typeof(Location.cSalesSubID), typeof(Location.cMPSalesSubID) });
				}
				else
				{
					throw new PXException(Messages.InvalidReasonCode);
				}

				var tran = new GLTran();
				tran.SummPost = this.SummPost;
				tran.BranchID = adj.AdjdBranchID;
				tran.AccountID = reasonCode.AccountID;
				tran.SubID = reasonCode.SubID;
				tran.OrigAccountID = adj.AdjdARAcct;
				tran.OrigSubID = adj.AdjdARSub;
				tran.DebitAmt = (adj.AdjgGLSign == 1m) ? adj.AdjWOAmt : 0m;
				tran.CuryDebitAmt = (adj.AdjgGLSign == 1m) ? adj.CuryAdjgWOAmt : 0m;
				tran.CreditAmt = (adj.AdjgGLSign == 1m) ? 0m : adj.AdjWOAmt;
				tran.CuryCreditAmt = (adj.AdjgGLSign == 1m) ? 0m : adj.CuryAdjgWOAmt;
				tran.TranType = adj.AdjgDocType;
				tran.TranClass = GLTran.tranClass.WriteOff;
				tran.RefNbr = adj.AdjgRefNbr;
				tran.TranDesc = payment.DocDesc;
				tran.TranDate = adj.AdjgDocDate;
				FinPeriodIDAttribute.SetPeriodsByMaster<GLTran.finPeriodID>(
					je.GLTranModuleBatNbr.Cache, tran, adj.AdjgTranPeriodID);
				tran.CuryInfoID = new_info.CuryInfoID;
				tran.Released = true;
				tran.ReferenceID = payment.CustomerID;
				tran.ProjectID = PM.ProjectDefaultAttribute.NonProject();

				UpdateHistory(tran, paymentCustomer);

				je.GLTranModuleBatNbr.SetValueExt<GLTran.subID>(tran, value);
				InsertAdjustmentsWriteOffTransaction(je, tran, 
					new GLTranInsertionContext { ARRegisterRecord = payment, ARAdjustRecord = adj });

				//Update CuryHistory in Voucher currency
				tran.CuryDebitAmt = (adj.AdjgGLSign == 1m) ? adj.CuryAdjdWOAmt : 0m;
				tran.CuryCreditAmt = (adj.AdjgGLSign == 1m) ? 0m : adj.CuryAdjdWOAmt;
				UpdateHistory(tran, paymentCustomer, vouch_info);
			}
		}

		private void ProcessAdjustmentGOL(JournalEntry je, ARAdjust adj, ARPayment payment, Customer paymentCustomer, ARRegister adjustedInvoice, Currency paycury, Currency cury, CurrencyInfo new_info, CurrencyInfo vouch_info)
		{
			CurrencyInfo invoice_info = CurrencyInfo_CuryInfoID.Select(adj.AdjdOrigCuryInfoID);
			bool isSameCuryAndRate =
				invoice_info.CuryID == new_info.CuryID &&
				invoice_info.CuryRate == new_info.CuryRate &&
				invoice_info.RecipRate == new_info.RecipRate;
			bool isAppliedToPrepaymentInvoice = adj.AdjdDocType == ARDocType.PrepaymentInvoice;

			//Debit/Credit RGOL Account
			if (cury.RealGainAcctID != null && cury.RealLossAcctID != null && !isSameCuryAndRate)
			{
				var tran = new GLTran();
				tran.SummPost = this.SummPost;
				tran.BranchID = (adj.AdjdDocType == ARDocType.SmallCreditWO) ? adjustedInvoice.BranchID : adj.AdjdBranchID;
				tran.AccountID = (adj.RGOLAmt > 0m && !(bool)adj.VoidAppl || adj.RGOLAmt < 0m && (bool)adj.VoidAppl)
					? cury.RealLossAcctID
					: cury.RealGainAcctID;
				tran.SubID = (adj.RGOLAmt > 0m && !(bool)adj.VoidAppl || adj.RGOLAmt < 0m && (bool)adj.VoidAppl)
					? CM.GainLossSubAccountMaskAttribute.GetSubID<Currency.realLossSubID>(je, tran.BranchID, cury)
					: CM.GainLossSubAccountMaskAttribute.GetSubID<Currency.realGainSubID>(je, tran.BranchID, cury);
				//SC has Payment AR Account in AdjdARAcct  and WO Account in ARAccountID
				tran.OrigAccountID = (adj.AdjdDocType == ARDocType.SmallCreditWO) ? adjustedInvoice.ARAccountID : adj.AdjdARAcct;
				tran.OrigSubID = (adj.AdjdDocType == ARDocType.SmallCreditWO) ? adjustedInvoice.ARSubID : adj.AdjdARSub;
				tran.CreditAmt = (adj.RGOLAmt < 0m) ? -1m * adj.RGOLAmt : 0m;
				//!object.Equals(vouch_info.CuryID, vouch_info.BaseCuryID) === precision alteration before payment application
				tran.CuryCreditAmt = object.Equals(new_info.CuryID, new_info.BaseCuryID) && !object.Equals(vouch_info.CuryID, vouch_info.BaseCuryID) ? tran.CreditAmt : 0m;
				tran.DebitAmt = (adj.RGOLAmt > 0m) ? adj.RGOLAmt : 0m;
				tran.CuryDebitAmt = object.Equals(new_info.CuryID, new_info.BaseCuryID) && !object.Equals(vouch_info.CuryID, vouch_info.BaseCuryID) ? tran.DebitAmt : 0m;
				tran.TranType = isAppliedToPrepaymentInvoice ? adj.AdjdDocType : adj.AdjgDocType;
				tran.TranClass = GLTran.tranClass.RealizedAndRoundingGOL;
				tran.RefNbr = adj.AdjgRefNbr;
				tran.TranDesc = payment.DocDesc;
				tran.TranDate = adj.AdjgDocDate;
				FinPeriodIDAttribute.SetPeriodsByMaster<GLTran.finPeriodID>(
					je.GLTranModuleBatNbr.Cache, tran, adj.AdjgTranPeriodID);
				tran.CuryInfoID = new_info.CuryInfoID;
				tran.Released = true;
				tran.ReferenceID = payment.CustomerID;
				tran.ProjectID = PM.ProjectDefaultAttribute.NonProject();

				UpdateHistory(tran, paymentCustomer);

				InsertAdjustmentsGOLTransaction(je, tran, 
					new GLTranInsertionContext { ARRegisterRecord = payment, ARAdjustRecord = adj });

				//Update CuryHistory in Voucher currency
				tran.CuryDebitAmt = 0m;
				tran.CuryCreditAmt = 0m;
				UpdateHistory(tran, paymentCustomer, vouch_info);
			}
			//Debit/Credit Rounding Gain-Loss Account
			else if (paycury.RoundingGainAcctID != null && paycury.RoundingLossAcctID != null)
			{
				var tran = new GLTran();
				tran.SummPost = this.SummPost;
				tran.BranchID = (adj.AdjdDocType == ARDocType.SmallCreditWO) ? adjustedInvoice.BranchID : adj.AdjdBranchID;
				tran.AccountID = (adj.RGOLAmt > 0m && !(bool)adj.VoidAppl || adj.RGOLAmt < 0m && (bool)adj.VoidAppl)
					? paycury.RoundingLossAcctID
					: paycury.RoundingGainAcctID;
				tran.SubID = (adj.RGOLAmt > 0m && !(bool)adj.VoidAppl || adj.RGOLAmt < 0m && (bool)adj.VoidAppl)
					? CM.GainLossSubAccountMaskAttribute.GetSubID<Currency.roundingLossSubID>(je, tran.BranchID, paycury)
					: CM.GainLossSubAccountMaskAttribute.GetSubID<Currency.roundingGainSubID>(je, tran.BranchID, paycury);

				//SC has Payment AR Account in AdjdARAcct  and WO Account in ARAccountID
				tran.OrigAccountID = (adj.AdjdDocType == ARDocType.SmallCreditWO) ? adjustedInvoice.ARAccountID : adj.AdjdARAcct;
				tran.OrigSubID = (adj.AdjdDocType == ARDocType.SmallCreditWO) ? adjustedInvoice.ARSubID : adj.AdjdARSub;
				tran.CreditAmt = (adj.RGOLAmt < 0m) ? -1m * adj.RGOLAmt : 0m;
				//!object.Equals(vouch_info.CuryID, vouch_info.BaseCuryID) === precision alteration before payment application
				tran.CuryCreditAmt = object.Equals(new_info.CuryID, new_info.BaseCuryID) && !object.Equals(vouch_info.CuryID, vouch_info.BaseCuryID) ? tran.CreditAmt : 0m;
				tran.DebitAmt = (adj.RGOLAmt > 0m) ? adj.RGOLAmt : 0m;
				tran.CuryDebitAmt = object.Equals(new_info.CuryID, new_info.BaseCuryID) && !object.Equals(vouch_info.CuryID, vouch_info.BaseCuryID) ? tran.DebitAmt : 0m;
				tran.TranType = adj.AdjgDocType;
				tran.TranClass = GLTran.tranClass.RealizedAndRoundingGOL;
				tran.RefNbr = adj.AdjgRefNbr;
				tran.TranDesc = payment.DocDesc;
				tran.TranDate = adj.AdjgDocDate;
			    FinPeriodIDAttribute.SetPeriodsByMaster<GLTran.finPeriodID>(
					je.GLTranModuleBatNbr.Cache, tran, adj.AdjgTranPeriodID);
				tran.CuryInfoID = new_info.CuryInfoID;
				tran.Released = true;
				tran.ReferenceID = payment.CustomerID;
				tran.ProjectID = PM.ProjectDefaultAttribute.NonProject();

				UpdateHistory(tran, paymentCustomer);

				InsertAdjustmentsGOLTransaction(je, tran, 
					new GLTranInsertionContext { ARRegisterRecord = payment, ARAdjustRecord = adj });

				//Update CuryHistory in Voucher currency
				tran.CuryDebitAmt = 0m;
				tran.CuryCreditAmt = 0m;
				UpdateHistory(tran, paymentCustomer, vouch_info);
			}
		}

		private void ProcessAdjustmentsRounding(
			JournalEntry je,
			ARRegister doc,
			ARAdjust prev_adj,
			ARPayment ardoc,
			Customer vend,
			Currency paycury,
			CurrencyInfo prev_info,
			CurrencyInfo new_info,
			decimal? amount,
			bool? isReversed = false)
		{
			if (prev_adj.VoidAppl == true || Equals(new_info.CuryID, new_info.BaseCuryID))
			{
				throw new PXException(Messages.UnexpectedRoundingForApplication);
			}

			UpdateARBalances(this, doc, amount, 0m);

			//BaseCalc should be false
			prev_adj.AdjAmt += amount;
			decimal? roundingLoss = isReversed != true ? amount : -amount;
			prev_adj.RGOLAmt -= roundingLoss;
			prev_adj = (ARAdjust)Caches[typeof(ARAdjust)].Update(prev_adj);
			
			foreach (ARTranPost post in 
				this.Caches<ARTranPost>()
					.Inserted
					.Cast<ARTranPost>()
					.Where(d =>d.RefNoteID == prev_adj.NoteID))
			{
				post.Amt = post.Type == ARTranPost.type.RGOL ||
				           post.Type == ARTranPost.type.Rounding
					? 0
					: prev_adj.AdjAmt;
				post.RGOLAmt = post.Type == ARTranPost.type.Rounding ? -prev_adj.RGOLAmt : prev_adj.RGOLAmt;
			}
			ARRegister adjdDoc = (ARRegister)ARDocument.Cache.Locate(
				new ARRegister { DocType = prev_adj.AdjdDocType, RefNbr = prev_adj.AdjdRefNbr });
			if (adjdDoc != null)
			{
				adjdDoc.RGOLAmt -= roundingLoss;
				ARDocument.Cache.Update(adjdDoc);
			}

			//signs are reversed to RGOL
			GLTran tran = new GLTran();
			tran.SummPost = SummPost;
			tran.BranchID = ardoc.BranchID;
			tran.AccountID = (roundingLoss < 0m)
				? paycury.RoundingLossAcctID
				: paycury.RoundingGainAcctID;
			tran.SubID = (roundingLoss < 0m)
				? CM.GainLossSubAccountMaskAttribute.GetSubID<Currency.roundingLossSubID>(je, tran.BranchID, paycury)
				: CM.GainLossSubAccountMaskAttribute.GetSubID<Currency.roundingGainSubID>(je, tran.BranchID, paycury);
			tran.OrigAccountID = prev_adj.AdjdARAcct;
			tran.OrigSubID = prev_adj.AdjdARSub;
			tran.CreditAmt = (roundingLoss > 0m) ? roundingLoss : 0m;
			tran.CuryCreditAmt = 0m;
			tran.DebitAmt = (roundingLoss < 0m) ? -roundingLoss : 0m;
			tran.CuryDebitAmt = 0m;
			tran.TranType = prev_adj.AdjgDocType;
			tran.TranClass = "R";
			tran.RefNbr = prev_adj.AdjgRefNbr;
			tran.TranDesc = ardoc.DocDesc;
			tran.TranDate = prev_adj.AdjgDocDate;
		    FinPeriodIDAttribute.SetPeriodsByMaster<GLTran.finPeriodID>(
				je.GLTranModuleBatNbr.Cache, tran, prev_adj.AdjgTranPeriodID);
			tran.CuryInfoID = new_info.CuryInfoID;
			tran.Released = true;
			tran.ReferenceID = ardoc.CustomerID;
			tran.ProjectID = ProjectDefaultAttribute.NonProject();

			UpdateHistory(tran, vend);
			//Update CuryHistory in Voucher currency
			UpdateHistory(tran, vend, prev_info);

			InsertAdjustmentsRoundingTransaction(je, tran, 
				new GLTranInsertionContext { ARRegisterRecord = doc, ARAdjustRecord = prev_adj });

			//Credit Payment AR Account
			tran = new GLTran();
			tran.SummPost = true;
			tran.ZeroPost = false;
			tran.BranchID = ardoc.BranchID;
			tran.AccountID = ardoc.ARAccountID;
			tran.SubID = ardoc.ARSubID;
			tran.ReclassificationProhibited = true;
			tran.DebitAmt = (roundingLoss > 0m) ? roundingLoss : 0m;
			tran.CuryDebitAmt = 0m;
			tran.CreditAmt = (roundingLoss < 0m) ? -roundingLoss : 0m;
			tran.CuryCreditAmt = 0m;
			tran.TranType = prev_adj.AdjgDocType;
			tran.TranClass = "P";
			tran.RefNbr = prev_adj.AdjgRefNbr;
			tran.TranDesc = ardoc.DocDesc;
			tran.TranDate = prev_adj.AdjgDocDate;
			FinPeriodIDAttribute.SetPeriodsByMaster<GLTran.finPeriodID>(
				je.GLTranModuleBatNbr.Cache, tran, prev_adj.AdjgTranPeriodID);
			tran.CuryInfoID = new_info.CuryInfoID;
			tran.Released = true;
			tran.ReferenceID = ardoc.CustomerID;
			tran.OrigAccountID = prev_adj.AdjdARAcct;
			tran.OrigSubID = prev_adj.AdjdARSub;
			tran.ProjectID = ProjectDefaultAttribute.NonProject();

			UpdateHistory(tran, vend);
			//Update CuryHistory in Payment currency
			UpdateHistory(tran, vend, new_info);

			InsertAdjustmentsRoundingTransaction(je, tran, 
				new GLTranInsertionContext { ARRegisterRecord = doc, ARAdjustRecord = prev_adj });
		}

		private void SegregateBatch(JournalEntry je, int? branchID, string curyID, DateTime? docDate, string finPeriodID, string description, CurrencyInfo curyInfo)
		{
			JournalEntry.SegregateBatch(je, BatchModule.AR, branchID, curyID, docDate, finPeriodID, description, curyInfo.GetCM(), null);
		}

		public virtual List<ARRegister> ReleaseDocProc(JournalEntry je, ARRegister ardoc, List<Batch> pmBatchList)
		{
			return ReleaseDocProc(je, ardoc, pmBatchList, null);
		}

		/// <summary>
		/// Performs basic checks that the document is releasable.
		/// Otherwise, throws error-specific exceptions.
		/// </summary>
		protected virtual void PerformBasicReleaseChecks(PXGraph selectGraph, ARRegister document)
		{
			if (document == null) throw new ArgumentNullException(nameof(document));

			if (document.Hold == true)
			{
				throw new ReleaseException(Messages.Document_OnHold_CannotRelease);
			}

			if (document.Status == ARDocStatus.PendingApproval || document.Status == ARDocStatus.Rejected)
			{
				throw new ReleaseException(Messages.DocumentNotApproved);
			}

			if (document.IsMigratedRecord == true && 
				document.Released != true && 
				IsMigrationMode != true)
			{
				throw new ReleaseException(Messages.CannotReleaseMigratedDocumentInNormalMode);
			}

			if (document.IsMigratedRecord != true && 
				IsMigrationMode == true)
			{
				throw new ReleaseException(Messages.CannotReleaseNormalDocumentInMigrationMode);
			}

			if (document.RetainageApply == true && !PXAccess.FeatureInstalled<FeaturesSet.retainage>())
			{
				throw new ReleaseException(GL.Messages.CannotReleaseRetainageDocumentIfFeatureOff);
			}

			Account acc = AccountAttribute.GetAccount(this,document.ARAccountID);
			if(acc.IsCashAccount.GetValueOrDefault() == true)
			{
				throw new ReleaseException(GL.Messages.NotValidAccount,GL.Messages.ModuleAR);
			}

			ARPayment documentAsPayment = PXSelect<
				ARPayment, 
				Where<
					ARPayment.docType, Equal<Required<ARPayment.docType>>,
					And<ARPayment.refNbr, Equal<Required<ARPayment.refNbr>>>>>
				.Select(selectGraph, document.DocType, document.RefNbr);

			if (documentAsPayment != null
				&& documentAsPayment.DocType != ARDocType.CreditMemo
				&& documentAsPayment.DocType != ARDocType.SmallBalanceWO
				&& documentAsPayment.DocType != ARDocType.PrepaymentInvoice
				&& arsetup.RequireExtRef == true
				&& string.IsNullOrEmpty(documentAsPayment.ExtRefNbr))
			{
				throw new ReleaseException(
					ErrorMessages.FieldIsEmpty,
					PXUIFieldAttribute.GetDisplayName<ARPayment.extRefNbr>(selectGraph.Caches[typeof(ARPayment)]));
			}
			else if (documentAsPayment?.CashAccountID != null)
			{
				var cashAccount = CashAccount.PK.Find(this, documentAsPayment.CashAccountID);
				Account caAcc = AccountAttribute.GetAccount(this, cashAccount.AccountID);
				if (caAcc.IsCashAccount.GetValueOrDefault() != true)
				{
					throw new PXException(CA.Messages.GLAccountIsNotCashAccount, caAcc.AccountCD);
				}
			}

			if (CCProcessingHelper.IntegratedProcessingActivated(arsetup) && documentAsPayment?.SyncLock == true)
			{
				throw new ReleaseException(Messages.ERR_CCProcessingARPaymentSyncLock);
			}

			if (document.OrigModule == BatchModule.SO)
			{
				ARInvoice arInvoice = PXSelect<ARInvoice,
					Where<ARInvoice.docType, Equal<Required<ARInvoice.docType>>,
						And<ARInvoice.refNbr, Equal<Required<ARInvoice.refNbr>>>>>
					.Select(this, document.DocType, document.RefNbr);

				ExternalTransactionState state = ExternalTranHelper.GetActiveTransactionState(selectGraph, ExternalTranHelper.GetSOInvoiceExternalTrans(selectGraph, arInvoice));
				if(state.IsPreAuthorized)	
				{
					throw new ReleaseException(Messages.InvoiceWasNotReleasedBecauseCreditCardPaymentIsNotCaptured);
				}
			}

			List<Branch> branchesInactive = SelectFrom<Branch>
				.InnerJoin<ARAdjust>
					.On<Branch.branchID.IsEqual<ARAdjust.adjdBranchID>
						.Or<Branch.branchID.IsEqual<ARAdjust.adjgBranchID>>>
				.Where<ARAdjust.adjgDocType.IsEqual<@P.AsString>
					.And<ARAdjust.adjgRefNbr.IsEqual<@P.AsString>>
					.And<Branch.active.IsNotEqual<True>>>
				.View
				.Select(this, document.DocType, document.RefNbr)
				.RowCast<Branch>()
				.ToList();

			if (branchesInactive.Any())
			{
				string branchCDs = String.Empty;
				branchCDs = string.Join(", ", branchesInactive.Select(x => x.BranchCD.Trim()).Distinct());
				throw new ReleaseException(Messages.ApplicationCannotBeReleasedBecauseBranchIsInactive, branchCDs);
			}
		}

		private void UpdateCCSpecificFields(ARRegister doc)
		{
			bool updateStatus = false;
			if (doc.DocType == ARDocType.Payment || doc.DocType == ARDocType.Prepayment || doc.DocType == ARDocType.CashSale) 
			{
				ARPayment ccPayment = ARPayment_DocType_RefNbr.Select(doc.DocType, doc.RefNbr);
				if (ccPayment?.CCReauthDate != null)
				{
					ccPayment = (ARPayment)ARPayment_DocType_RefNbr.Cache.Extend<ARRegister>(doc);
					ccPayment.CCReauthDate = null;
					ccPayment.CCReauthTriesLeft = 0;
					updateStatus = true;
				}
				if (ccPayment?.IsCCUserAttention == true)
				{
					if (!updateStatus)
					{
						ccPayment = (ARPayment)ARPayment_DocType_RefNbr.Cache.Extend<ARRegister>(doc);
					}

					ccPayment.IsCCUserAttention = false;
					updateStatus = true;
				}

				if (updateStatus)
				{
					ARPayment_DocType_RefNbr.Cache.Update(ccPayment);
					ARDocument.Cache.SetStatus(doc, PXEntryStatus.Notchanged);
				}
			}
		}

		private void CheckOpenForReviewTrans(ARRegister doc)
		{
			if (arsetup.IntegratedCCProcessing == false) return;
			if (DocWithOpenForReviewTrans(doc))
			{
				throw new PXException(Messages.CCProcessingARPaymentTranHeldWarning);
			}
		}

		private bool DocWithOpenForReviewTrans(ARRegister doc)
		{
			var trans = new PXSelect<ExternalTransaction, Where<ExternalTransaction.refNbr, Equal<Required<ARRegister.refNbr>>,
				And<ExternalTransaction.docType, Equal<Required<ARRegister.docType>>>>,
				OrderBy<Desc<ExternalTransaction.transactionID>>>(this).Select(doc.RefNbr, doc.DocType).RowCast<ExternalTransaction>();
			var state = ExternalTranHelper.GetActiveTransactionState(this, trans);
			return state.IsOpenForReview;
		}

		public virtual ARRegister OnBeforeRelease(ARRegister ardoc)
		{
	        	return ardoc;
		}

		private OldInvoiceDateRefresher _oldInvoiceRefresher = new OldInvoiceDateRefresher();

		/// <summary>
		/// Common entry point.
		/// The method to release both types of documents - invoices and payments.
		/// </summary>
		public virtual List<ARRegister> ReleaseDocProc(JournalEntry je, ARRegister ardoc, List<Batch> pmBatchList, ARDocumentRelease.ARMassProcessReleaseTransactionScopeDelegate onreleasecomplete)
		{
			List<ARRegister> ret = null;

			PerformBasicReleaseChecks(je, ardoc);

			if (ardoc.DocType.IsIn(ARDocType.Invoice, ARDocType.DebitMemo, ARDocType.CreditMemo) && ardoc.OrigDocAmt < 0)
			{
				throw new PXException(AP.Messages.DocAmtMustBeGreaterZero);
			}

			// The GL module shouldn't be affected if 
			// an AR migration mode is activated
			//
			if (IsMigrationMode == true)
			{
				je.SetOffline();
			}

			_oldInvoiceRefresher = new OldInvoiceDateRefresher();

			bool isUnreleasedCreditMemo = ardoc.DocType == ARDocType.CreditMemo && ardoc.Released != true;
			ARRegister doc = PXCache<ARRegister>.CreateCopy(ardoc);
			using (PXTransactionScope ts = new PXTransactionScope())
			{
				PXCache<ARRegister>.StoreOriginal(this, doc);
				// Mark as updated so that doc will not expire from cache and update with Released = 1 
				// will not override balances/amount in document
				//
				ARDocument.Cache.SetStatus(doc, PXEntryStatus.Updated);

				UpdateARBalances(doc, -doc.OrigDocAmt);

				List<PM.PMRegister> pmDocList = new List<PM.PMRegister>();

				foreach (PXResult<ARInvoice, CurrencyInfo, Terms, Customer, Account> res in ARInvoice_DocType_RefNbr.Select(doc.DocType, doc.RefNbr))
				{
					Customer customer = res;
					CurrencyInfo currencyinfo = res;

					switch (customer.Status)
					{
						case CustomerStatus.Inactive:
						case CustomerStatus.Hold:
						case CustomerStatus.CreditHold:
							throw new PXSetPropertyException(Messages.CustomerIsInStatus, new CustomerStatus.ListAttribute().ValueLabelDic[customer.Status]);
					}

					ARInvoice invoice = res;

					// Must check for CM application in different period
					if (doc.Released == false)
					{
						SegregateBatch(je, doc.BranchID, doc.CuryID, doc.DocDate, doc.FinPeriodID, doc.DocDesc, currencyinfo);
					}

					if (_IsIntegrityCheck == false &&
						invoice.DocType != ARDocType.CashSale &&
						invoice.DocType != ARDocType.CashReturn &&
						invoice.DocType != ARDocType.SmallCreditWO &&
						customer.AutoApplyPayments == true &&
						invoice.Released == false &&
						invoice.IsMigratedRecord != true)
					{
						InvoiceEntryGraph.Clear();
						InvoiceEntryGraph.Document.Current = invoice;

						if (InvoiceEntryGraph.Adjustments_Inv.View.SelectSingle() == null)
						{
							InvoiceEntryGraph.LoadInvoicesProc();
						}
						InvoiceEntryGraph.Save.Press();
						doc = InvoiceEntryGraph.Document.Current;
						doc.ReleasedToVerify = false;
						ret = ReleaseInvoice(je, doc,
					new PXResult<ARInvoice, CurrencyInfo, Terms, Customer, Account>(InvoiceEntryGraph.Document.Current, currencyinfo, (Terms)res, customer, (Account)res), pmDocList);
					}
					else
					{
						ret = ReleaseInvoice(je, doc, res, pmDocList);
					}

					// Ensure correct PXDBDefault behaviour on ARTran persisting
					ARInvoice_DocType_RefNbr.Current = (ARInvoice)res;
				}

				Amount docBal = new Amount();
				foreach (PXResult<ARPayment, CurrencyInfo, Currency, Customer, CashAccount> res in ARPayment_DocType_RefNbr.Select(doc.DocType, doc.RefNbr, doc.CustomerID))
				{
					ARPayment payment = res;
					CurrencyInfo info = res;
					Currency paycury = res;
					Customer customer = res;
					CashAccount cashacct = res;
					GetExtension<MultiCurrency>().StoreResult(res);

					if (ARDocType.SignBalance(doc.DocType) != 0 &&
						!(doc is ARPayment || doc is ARInvoice))
					{
						ARDocument.Cache.Remove(doc);
						ARPayment pmt = PXCache<ARPayment>.CreateCopy(payment);
						PXCache<ARRegister>.RestoreCopy(pmt, doc);
						ARDocument.Cache.Remove(doc);
						doc = pmt;
						ARDocument.Cache.SetStatus(doc, PXEntryStatus.Updated);
					}
					payment.OrigReleased = payment.Released;
					ARPayment_DocType_RefNbr.Current = payment;

					Tuple<ARAdjust, CurrencyInfo> lastAdjustment = new Tuple<ARAdjust, CurrencyInfo>(new ARAdjust(), new CurrencyInfo());

					if (customer.Status == CustomerStatus.Inactive)
					{
						throw new PXSetPropertyException(Messages.CustomerIsInStatus,
							new CustomerStatus.ListAttribute().ValueLabelDic[customer.Status]);
					}

					CM.CurrencyInfo last_info = null;
					if (doc.Released != true &&
						doc.DocType.IsIn(ARDocType.Payment , ARDocType.Prepayment, ARDocType.VoidPayment , ARDocType.Refund , ARDocType.VoidRefund, ARDocType.PrepaymentInvoice))
					{
						SegregateBatch(je, doc.BranchID, doc.CuryID, payment.DocDate, payment.FinPeriodID, doc.DocDesc, info);

						// We should use the same CurrencyInfo for Payment
						// and its applications to save proper consolidation
						// for generated GL transactions
						// 
						last_info = GetCurrencyInfoCopyForGL(je, info);
						var new_res = new PXResult<ARPayment, CurrencyInfo, Currency, Customer, CashAccount>(payment, CurrencyInfo.GetEX(last_info), paycury, customer, cashacct);

						if (_IsIntegrityCheck == false &&
							customer.AutoApplyPayments == true &&
							payment.DocType == ARDocType.Payment &&
							payment.Released != true &&
							payment.IsMigratedRecord != true)
						{
							pe.Clear();
							pe.SelectTimeStamp();
							bool anyAdj = false;
							if (PXTransactionScope.IsScoped)
							{
								//It's required to select curyInfo as it may be not committed to the database yet.
								//So it cannot be selected through RowSelecting for the balance calculation if it is not in the database.
								// (as RowSelecting have it's own connection scope).
								// 
								CurrencyInfo _curyInfo = pe.CurrencyInfo_CuryInfoID.Select(payment.CuryInfoID);
								GetExtension<MultiCurrency>().StoreResult(_curyInfo);

								foreach (ARAdjust adj in pe.Adjustments_Raw.Select(payment.DocType, payment.RefNbr))
								{
									if (!anyAdj)
									{
										anyAdj = true;
										pe.CurrencyInfo_CuryInfoID.View.Clear();
									}

									CurrencyInfo adjdCuryInfoTemp = pe.CurrencyInfo_CuryInfoID.Select(adj.AdjdCuryInfoID);
								}
								_curyInfo = pe.CurrencyInfo_CuryInfoID.Select(payment.CuryInfoID);
							}
							else
							{
								anyAdj = (pe.Adjustments_Raw.View.SelectSingle() != null);
							}

							pe.Document.Current = payment;
							if (!anyAdj)
							{
								pe.LoadInvoicesProc(false);
							}

							var updInvoices = pe.ARInvoice_DocType_RefNbr.Cache.Updated.Cast<ARInvoice>().ToList();
							pe.Save.Press();
							doc = pe.Document.Current;
							doc.ReleasedToVerify = false; // the flag is reseted after persist, we need to keep it
							// We need this to put updated payment
							// document for release process furhter.
							// 
							new_res = new PXResult<ARPayment, CurrencyInfo, Currency, Customer, CashAccount>(pe.Document.Current, CurrencyInfo.GetEX(last_info), paycury, customer, cashacct);
						}

						ProcessPayment(je, doc, new_res);

						SaveBatchForDocument(je, doc);
						this.ReplaceCADailySummaryCache(je);
						var appsByPeriod = new SortedDictionary<string, List<PXResult<ARAdjust>>>();
						var datesByPeriod = new SortedDictionary<string, DateTime?>();

						InsertCurrencyInfoIntoCache(doc, info);

						foreach(PXResult<ARAdjust, CurrencyInfo, Currency, ARRegister, ARInvoice, ARPayment, ARTran, CM.CurrencyInfo2> adjres
							in ARAdjust_AdjgDocType_RefNbr_CustomerID.Select(doc.DocType, doc.RefNbr, _IsIntegrityCheck, doc.AdjCntr))
						{
							ARAdjust adj = (ARAdjust)adjres;
							ARRegister adj_d = (ARRegister)adjres;
							SetAdjgPeriodsFromLatestApplication(doc, adj);

							if (adj_d.Released != true && !adj.IsSelfAdjustment()) continue;

							if (!appsByPeriod.TryGetValue(adj.AdjgTranPeriodID, out List<PXResult<ARAdjust>> apps))
							{
								appsByPeriod[adj.AdjgTranPeriodID] = apps = new List<PXResult<ARAdjust>>();
							}
							apps.Add(adjres);

							DateTime? maxdate;
							{
								datesByPeriod[adj.AdjgTranPeriodID] = maxdate = adj.AdjgDocDate;
							}

							if (DateTime.Compare((DateTime)adj.AdjgDocDate, (DateTime)maxdate) > 0)
							{
								datesByPeriod[adj.AdjgTranPeriodID] = adj.AdjgDocDate;
							}

							if (doc.OpenDoc == false &&
								doc.DocType == ARDocType.VoidPayment)
							{
								doc.OpenDoc = true;
								doc.CuryDocBal = doc.CuryOrigDocAmt;
								doc.DocBal = doc.OrigDocAmt;
								RaisePaymentEvent(doc, ARPayment.Events.Select(e => e.OpenDocument));
							}
						}

						CheckVoidedDoucmentAmountDiscrepancies(doc);

						Batch paymentBatch = je.BatchModule.Current;

						foreach (KeyValuePair<string, List<PXResult<ARAdjust>>> pair in appsByPeriod)
						{
							FinPeriod postPeriod = FinPeriodRepository
								.GetFinPeriodByMasterPeriodID(PXAccess.GetParentOrganizationID(doc.BranchID), pair.Key)
								.GetValueOrRaiseError();

							JournalEntry.SegregateBatch(je, BatchModule.AR, doc.BranchID, doc.CuryID,
								datesByPeriod[pair.Key], postPeriod.FinPeriodID,
								doc.DocDesc, info.GetCM(), paymentBatch);

							var adjustments = new PXResultset<ARAdjust>();
							adjustments.AddRange(pair.Value);

							last_info = GetCurrencyInfoCopyForGL(je, info);
							lastAdjustment = ProcessAdjustments(je, adjustments, doc, payment, customer, last_info, paycury);
						}
					}
					else
					{
						if (doc.DocType != ARDocType.CashSale && doc.DocType != ARDocType.CashReturn)
						{
							SegregateBatch(je, doc.BranchID, doc.CuryID, payment.AdjDate, payment.AdjFinPeriodID, payment.DocDesc, info);
						}

						// We should use the same CurrencyInfo for Payment
						// and its applications to save proper consolidation
						// for generated GL transactions
						// 
						last_info = GetCurrencyInfoCopyForGL(je, info);
						var new_res = new PXResult<ARPayment, CurrencyInfo, Currency, Customer, CashAccount>(payment, CurrencyInfo.GetEX(last_info), paycury, customer, cashacct);

						ProcessPayment(je, doc, new_res);

						foreach (PXResult<ARAdjust> adjres in ARAdjust_AdjgDocType_RefNbr_CustomerID.Select(doc.DocType, doc.RefNbr, _IsIntegrityCheck, doc.AdjCntr))
						{
							ARAdjust adj = (ARAdjust)adjres;
							SetAdjgPeriodsFromLatestApplication(doc, adj);
						}

						PXResultset<ARAdjust> adjustments = ARAdjust_AdjgDocType_RefNbr_CustomerID.Select(doc.DocType, doc.RefNbr, _IsIntegrityCheck, doc.AdjCntr);

						CheckVoidedDoucmentAmountDiscrepancies(doc);

						lastAdjustment = ProcessAdjustments(je, adjustments, doc, payment, customer, last_info, paycury);
					}
					docBal = doc.IsMigratedRecord != true
						? new Amount(doc.CuryDocBal, doc.DocBal)
						: new Amount(doc.CuryInitDocBal, doc.InitDocBal);

					if (doc.DocType == ARDocType.SmallBalanceWO && lastAdjustment.Item1.Voided == true)
					{
						ProcessVoidWOTranPost(doc, lastAdjustment.Item1);
					}
					if ((doc.DocType == ARDocType.VoidRefund ||
					     doc.DocType == ARDocType.VoidPayment) &&
					    docBal.Base != 0)
						ProcessVoidPaymentTranPost(doc, docBal);
					
					if (doc.IsRetainageDocument == true && doc.IsRetainageReversing != true)
					{
						ARRegister origRetainageDoc = GetOriginalRetainageDocument(doc);
						ARAdjust adj = lastAdjustment.Item1;
						if (origRetainageDoc != null && adj.AdjdDocType == ARDocType.Invoice)
						{
							decimal? sign = origRetainageDoc.SignAmount * adj.AdjdTBSign;
							decimal curyRetainagePaid = adj.CuryAdjdAmt * sign ?? 0m;
							decimal retainagePaid = adj.AdjAmt * sign ?? 0m;
							origRetainageDoc.CuryRetainageUnpaidTotal -= curyRetainagePaid;
							origRetainageDoc.RetainageUnpaidTotal -= retainagePaid;
							ARDocument.Cache.Update(origRetainageDoc);

							ARTranPost newTran = CreateTranPost(doc);
							newTran.SourceDocType = payment.DocType;
							newTran.SourceRefNbr = payment.RefNbr;
							newTran.DocType = origRetainageDoc.DocType;
							newTran.RefNbr = origRetainageDoc.RefNbr;
							newTran.Type = ARTranPost.type.RetainagePayment;
							newTran.CuryAmt = curyRetainagePaid;
							newTran.Amt = retainagePaid;
							if (IsNeedUpdateHistoryForTransaction(newTran.FinPeriodID))
								TranPost.Insert(newTran);
						}
					}
					VerifyPaymentRoundAndClose(je, doc, payment, customer, CurrencyInfo.GetEX(last_info), paycury, lastAdjustment);
					doc.AdjCntr++;

					// Ensure correct PXDBDefault behaviour on ARAdjust persisting.
					// 
					ARPayment_DocType_RefNbr.Current = payment;
				}
				
				if (doc.DocType == ARDocType.VoidPayment)
				{
					// Create deposit rgol reverse batch
					ARPayment voidPayment = PXSelect<ARPayment, Where<ARPayment.docType, Equal<ARDocType.voidPayment>, And<ARPayment.refNbr, Equal<Required<ARRegister.refNbr>>>>>.Select(je, doc.RefNbr);
					ARPayment origPayment = PXSelect<ARPayment, Where<ARPayment.docType, Equal<ARDocType.payment>, And<ARPayment.refNbr, Equal<Required<ARRegister.refNbr>>>>>.Select(je, doc.RefNbr);
					if (origPayment != null && origPayment.Deposited == true)
					{
						CADeposit deposit = PXSelect<CADeposit, Where<CADeposit.refNbr, Equal<Required<ARPayment.depositNbr>>>>.Select(je, origPayment.DepositNbr);
						if (deposit != null)
						{
							CADepositDetail detail = PXSelect<CADepositDetail, Where<CADepositDetail.refNbr, Equal<Required<CADeposit.refNbr>>,
							And<CADepositDetail.origRefNbr, Equal<Required<ARPayment.refNbr>>,
							And<CADepositDetail.origDocType, Equal<ARDocType.payment>>>>>.Select(je, deposit.RefNbr, origPayment.RefNbr);
							if (detail != null)
							{
								decimal rgol = Math.Round((detail.OrigAmtSigned.Value - detail.TranAmt.Value), 3);
								if (rgol != Decimal.Zero)
								{
									if (deposit.CashAccountID == voidPayment.CashAccountID)
									{
										CashAccount depositCashacct = PXSelect<CashAccount, Where<CashAccount.cashAccountID, Equal<Required<CashAccount.cashAccountID>>>>.Select(je, deposit.CashAccountID);
										GLTran rgol_tran = new GLTran();
										rgol_tran.DebitAmt = Decimal.Zero;
										rgol_tran.CreditAmt = Decimal.Zero;
										rgol_tran.AccountID = depositCashacct.AccountID;
										rgol_tran.SubID = depositCashacct.SubID;
										rgol_tran.BranchID = depositCashacct.BranchID;
										rgol_tran.TranDate = doc.DocDate;
										FinPeriodIDAttribute.SetPeriodsByMaster<GLTran.finPeriodID>(
											je.GLTranModuleBatNbr.Cache, rgol_tran, doc.TranPeriodID);
										rgol_tran.TranType = CATranType.CATransferRGOL;
										rgol_tran.RefNbr = doc.RefNbr;
										rgol_tran.TranDesc = Messages.ReversingRGOLTanDescription;
										rgol_tran.Released = true;
										rgol_tran.CuryInfoID = doc.CuryInfoID;

										rgol_tran.DebitAmt += ((origPayment.DrCr == CADrCr.CACredit) == rgol > 0 ? Decimal.Zero : Math.Abs(rgol));
										rgol_tran.CreditAmt += ((origPayment.DrCr == CADrCr.CACredit) == rgol > 0 ? Math.Abs(rgol) : Decimal.Zero);

										Currency rgol_cury = PXSelect<Currency, Where<Currency.curyID, Equal<Required<Currency.curyID>>>>.Select(je, deposit.CuryID);

										decimal rgolAmt = (decimal)(rgol_tran.DebitAmt - rgol_tran.CreditAmt);
										int sign = Math.Sign(rgolAmt);
										rgolAmt = Math.Abs(rgolAmt);

										if ((rgolAmt) != Decimal.Zero)
										{
											GLTran tran = (GLTran)je.Caches[typeof(GLTran)].CreateCopy(rgol_tran);
											tran.CuryDebitAmt = Decimal.Zero;
											tran.CuryCreditAmt = Decimal.Zero;
											if (doc.DocType == CATranType.CADeposit)
											{
												tran.AccountID = (sign < 0) ? rgol_cury.RealLossAcctID : rgol_cury.RealGainAcctID;
												tran.SubID = (sign < 0)
												? CM.GainLossSubAccountMaskAttribute.GetSubID<Currency.realLossSubID>(je, rgol_tran.BranchID, rgol_cury)
												: CM.GainLossSubAccountMaskAttribute.GetSubID<Currency.realGainSubID>(je, rgol_tran.BranchID, rgol_cury);
											}
											else
											{
												tran.AccountID = (sign < 0) ? rgol_cury.RealGainAcctID : rgol_cury.RealLossAcctID;
												tran.SubID = (sign < 0)
												? CM.GainLossSubAccountMaskAttribute.GetSubID<Currency.realGainSubID>(je, rgol_tran.BranchID, rgol_cury)
												: CM.GainLossSubAccountMaskAttribute.GetSubID<Currency.realLossSubID>(je, rgol_tran.BranchID, rgol_cury);
											}

											tran.DebitAmt = sign < 0 ? rgolAmt : Decimal.Zero;
											tran.CreditAmt = sign < 0 ? Decimal.Zero : rgolAmt;
											tran.TranType = CATranType.CATransferRGOL;
											tran.RefNbr = doc.RefNbr;
											tran.TranDesc = Messages.ReversingRGOLTanDescription;
											tran.TranDate = rgol_tran.TranDate;
											FinPeriodIDAttribute.SetPeriodsByMaster<GLTran.finPeriodID>(
												je.GLTranModuleBatNbr.Cache, tran, rgol_tran.TranPeriodID);
											tran.Released = true;
											tran.CuryInfoID = origPayment.CuryInfoID;
											tran = InsertPaymentCADepositTransaction(je, tran,
												new GLTranInsertionContext { ARRegisterRecord = doc, CADepositRecord = deposit, CADepositDetailRecord = detail });

											rgol_tran.CuryDebitAmt = Decimal.Zero;
											rgol_tran.DebitAmt = (sign > 0) ? rgolAmt : Decimal.Zero;
											rgol_tran.CreditAmt = (sign > 0) ? Decimal.Zero : rgolAmt;
											InsertPaymentCADepositTransaction(je, rgol_tran,
												new GLTranInsertionContext { ARRegisterRecord = doc, CADepositRecord = deposit, CADepositDetailRecord = detail });
										}
									}
								}
							}
						}
					}
				}

				// When doc is loaded in ARInvoiceEntry, ARPaymentEntry it will set Selected = 0  
				// and document will dissappear from list of processing items.
				// 
				doc.Selected = true;
				doc.Released = true;

				UpdateARBalances(doc);

				if (_IsIntegrityCheck == false)
				{
					SaveBatchForDocument(je, doc);
					this.ReplaceCADailySummaryCache(je);
				}

				if (doc.Released == true)
				{
					RaiseReleaseEvent(doc);
				}
				PXCache<ARRegister>.RestoreCopy(ardoc, doc);

				if (doc.DocType == ARDocType.CreditMemo)
				{
					if (!isUnreleasedCreditMemo)
					{
						ARPayment_DocType_RefNbr.Cache.SetStatus(ARPayment_DocType_RefNbr.Current, PXEntryStatus.Notchanged);
					}
					else
					{
						PXSelectorAttribute.StoreResult<ARRegister.curyID>(ARPayment_DocType_RefNbr.Cache, doc,
							CM.CurrencyCollection.GetCurrency(doc.CuryID));
						
						ARPayment crmemo = (ARPayment)ARPayment_DocType_RefNbr.Cache.Extend<ARRegister>(doc);
						crmemo.AdjTranPeriodID = null;
						crmemo.AdjFinPeriodID = null;
						crmemo.CuryInfoID = doc.CuryInfoID;
						ARPayment_DocType_RefNbr.Cache.Update(crmemo);

						crmemo.CreatedByID = doc.CreatedByID;
						crmemo.CreatedByScreenID = doc.CreatedByScreenID;
						crmemo.CreatedDateTime = doc.CreatedDateTime;
						crmemo.CashAccountID = null;
						crmemo.AdjDate = crmemo.DocDate;
						crmemo.AdjTranPeriodID = crmemo.TranPeriodID;
						crmemo.AdjFinPeriodID = crmemo.FinPeriodID;
						OpenPeriodAttribute.SetValidatePeriod<ARPayment.adjFinPeriodID>(ARPayment_DocType_RefNbr.Cache, crmemo, PeriodValidation.DefaultSelectUpdate);
						ARDocument.Cache.SetStatus(doc, PXEntryStatus.Notchanged);
						doc = ARPayment_DocType_RefNbr.Update(crmemo);
					}
				}
				else
				{
					if (ARDocument.Cache.ObjectsEqual(doc, ARPayment_DocType_RefNbr.Current))
					{
						ARPayment_DocType_RefNbr.Cache.SetStatus(ARPayment_DocType_RefNbr.Current, PXEntryStatus.Notchanged);
					}
				}

				if (ardoc.IsPrepaymentInvoiceDocument() && ardoc is ARInvoice)
				{
					ARInvoice inv = (ARInvoice)ardoc;
					ARPayment prepayment = (ARPayment)ARPayment_DocType_RefNbr.Cache.Extend<ARRegister>(ardoc);
					prepayment.CreatedByID = ardoc.CreatedByID;
					prepayment.CreatedByScreenID = ardoc.CreatedByScreenID;
					prepayment.CreatedDateTime = ardoc.CreatedDateTime;
					prepayment.CashAccountID = inv.CashAccountID;
					prepayment.PaymentMethodID = inv.PaymentMethodID;

					//prepayment.DocDate = prepaymentAdj.AdjgDocDate;
					//FinPeriodIDAttribute.SetPeriodsByMaster<ARPayment.finPeriodID>(ARPayment_DocType_RefNbr.Cache, prepayment, prepaymentAdj.AdjgTranPeriodID);

					prepayment.AdjDate = prepayment.DocDate;
					prepayment.AdjFinPeriodID = prepayment.FinPeriodID;
					prepayment.AdjTranPeriodID = prepayment.TranPeriodID;

					//APAddressAttribute.DefaultRecord<APPayment.remitAddressID>(APPayment_DocType_RefNbr.Cache, prepayment);
					//APContactAttribute.DefaultRecord<APPayment.remitContactID>(APPayment_DocType_RefNbr.Cache, prepayment);
					prepayment.CuryDiscBal = 0m;
					prepayment.DiscBal = 0m;
					prepayment.DocBal = ardoc.OrigDocAmt;
					prepayment.CuryDocBal = ardoc.CuryOrigDocAmt;
					prepayment.OpenDoc = true;
					ARPayment_DocType_RefNbr.Cache.Update(prepayment);

					TaxAttribute.SetTaxCalc<ARTran.taxCategoryID>(ARTran_TranType_RefNbr.Cache, null, TaxCalc.NoCalc);

					//GetExtension<MultiCurrency>().UpdateCurrencyInfoForPrepayment(prepayment, curyInfoToUse);
					ARDocument.Cache.SetStatus(ardoc, PXEntryStatus.Notchanged);
					//Prepayment with prepayment should not generetate any RGOL.
					//if (prepaymentAdj.AdjgDocType == ARDocType.Prepayment)
					//	prepaymentAdj.RGOLAmt = 0;
				}

				UpdateCCSpecificFields(doc);
				ProcessPostponedFlags();

				List<ProcessInfo<Batch>> batchList;
				PM.RegisterRelease.ReleaseWithoutPost(pmDocList, false, out batchList);
				foreach (ProcessInfo<Batch> processInfo in batchList)
				{
					pmBatchList.AddRange(processInfo.Batches);
				}
				bool docUpdated = ARDocument.Cache.GetStatus(doc) == PXEntryStatus.Updated;
				Actions.PressSave();

				if (docUpdated)
				{
					doc = (ARRegister)ARDocument.Cache.Locate(doc);
				}

				if (_IsIntegrityCheck != true)
				{
					EntityInUseHelper.MarkEntityAsInUse<CurrencyInUse>(doc.CuryID);
				}

				onreleasecomplete?.Invoke(ardoc);
				#region Auto Commit/Post document to avalara.
				bool arInvoiceUpdated = false;
				if (docUpdated)
				{
					ARInvoice arInvoice = PXSelect<ARInvoice, Where<ARInvoice.docType, Equal<Required<ARInvoice.docType>>, And<ARInvoice.refNbr, Equal<Required<ARInvoice.refNbr>>>>>.Select(this, doc.DocType, doc.RefNbr);
					if (arInvoice != null)
					{
						arInvoice = CommitExternalTax(arInvoice);
						if (doc.IsTaxPosted != arInvoice.IsTaxPosted || arInvoice.WarningMessage != null)
						{
							ardoc.IsTaxPosted = doc.IsTaxPosted = arInvoice.IsTaxPosted == true;
							ardoc.WarningMessage = arInvoice.WarningMessage;
							doc = (ARRegister)ARDocument.Cache.Update(doc);
							ARDocument.Cache.Persist(doc, PXDBOperation.Update);
							arInvoiceUpdated = true;
						}
					}
				}
				#endregion
				ts.Complete(this);
				if (arInvoiceUpdated)
				{
					ARDocument.Cache.Persisted(false);
				}
			}

			_oldInvoiceRefresher.CommitRefresh(this);

			PXCache<ARRegister>.RestoreCopy(ardoc, doc);
			if (isUnreleasedCreditMemo)
			{
				ARRegister located = ARDocument.Locate(doc);
				if (located != null)
					PXCache<ARRegister>.RestoreCopy(located, doc);
				if (ret?.Count > 0 && ret[0].DocType == doc.DocType && ret[0].RefNbr == doc.RefNbr)
					PXCache<ARRegister>.RestoreCopy(ret[0], doc);
			}
			return ret;
		}

		protected void CheckVoidedDoucmentAmountDiscrepancies(ARRegister document)
		{
			if (document.DocType == ARDocType.VoidPayment || document.DocType == ARDocType.VoidRefund)
			{
				ARRegister origDoc = ARRegister.PK.Find(this, document.OrigDocType, document.OrigRefNbr);

				if (document.OrigDocAmt != -origDoc.OrigDocAmt)
				{
					throw new ReleaseException(Common.Messages.AmountDifferFromDocumentBeingVoided, document.RefNbr, ARDocType.GetDisplayName(document.DocType));
				}
			}
		}

		protected virtual void ProcessPostponedFlags()
		{
			// temporary solution which allows not to update records (SOAdjust, ARAdjust) during the process of release
			// when ARAdjust records are in the updated status, select of ARAdjust_AdjgDocType_RefNbr_CustomerID does not work correctly
			foreach (ARPayment item in ARPayment_DocType_RefNbr.Cache.Cached.Cast<ARPayment>().Where(p
				=> p.PostponeReleasedFlag == true
				|| p.PostponeVoidedFlag == true))
			{
				// we need the child caches to be initialized
				PXCache socache = soAdjust.Cache; 
				PXCache arcache = ARAdjust_AdjgDocType_RefNbr_CustomerID.Cache;
				if (item.PostponeReleasedFlag == true)
				{
					ARPayment_DocType_RefNbr.Cache.RaiseEventsOnFieldChanging<ARPayment.released>(item, true);
					item.PostponeReleasedFlag = false;
				}
				if (item.PostponeVoidedFlag == true)
				{
					item.Voided = false;
					ARPayment_DocType_RefNbr.Cache.RaiseEventsOnFieldChanging<ARPayment.voided>(item, true);
					item.PostponeVoidedFlag = false;
				}
			}
		}

		public virtual ARInvoice CommitExternalTax(ARInvoice doc)
		{
			return doc;
		}

		/// <summary>
		/// Workaround for AC-167924. To prevent selection of outdated currencyinfo record from DB
		/// 1. When we generate ar doc through the voucher from, we create new currencyinfo in the voucher graph.
		/// 2. We are persisting changes but they are not committed in the db
		/// 3. When we are in the ar release graph, we select the currencyinfo from db and get outdated commited one.
		/// 4. This workaround is that to put the currencyinfo to the cache to avoid quieting the db
		/// </summary>
		/// <param name="doc"></param>
		/// <param name="info"></param>
		protected virtual void InsertCurrencyInfoIntoCache(ARRegister doc, CurrencyInfo info)
		{
			if (doc.OrigModule == BatchModule.GL)
			{
				this.CurrencyInfo_CuryInfoID.Insert(info);
			}
		}

		public virtual void SaveBatchForDocument(JournalEntry je, ARRegister doc)
		{
			if (je.GLTranModuleBatNbr.Cache.IsInsertedUpdatedDeleted)
			{
				je.Save.Press();
			}

			if (!je.BatchModule.Cache.IsDirty && string.IsNullOrEmpty(doc.BatchNbr))
			{
				string currentBatchNumber = je.BatchModule.Current.BatchNbr;
				doc.BatchNbr = doc.IsMigratedRecord != true ? currentBatchNumber : null;

				foreach (ARTranPost post in 
						TranPost.Cache.Inserted.Cast<ARTranPost>()
							.Where(d =>d.TranType == doc.DocType && d.TranRefNbr == doc.RefNbr && d.BatchNbr == null))
					post.BatchNbr = currentBatchNumber;
			}
		}

		public virtual void SaveBatchForAdjustment(JournalEntry je, ARAdjust adj, ARRegister adjustedDocument)
		{
			SaveBatchForAdjustment(je, adj, adjustedDocument, true);
		}

		public virtual void SaveBatchForAdjustment(JournalEntry je, ARAdjust adj, ARRegister adjustedDocument, bool saveBatch)
		{
			if (je.GLTranModuleBatNbr.Cache.IsInsertedUpdatedDeleted)
			{
				je.Save.Press();
			}

			if (!je.BatchModule.Cache.IsDirty && saveBatch)
			{
				adj.AdjBatchNbr = je.BatchModule.Current.BatchNbr;

				if (adj.AdjdDocType == ARDocType.SmallCreditWO && adj.Voided == true)
				{
					foreach (ARTranPost post in
						TranPost.Cache.Inserted.Cast<ARTranPost>().Where(d =>
							d.DocType == adj.AdjdDocType &&
							d.RefNbr == adj.AdjdRefNbr &&
							d.BatchNbr == null))
						post.BatchNbr = je.BatchModule.Current.BatchNbr;
				}

				if (adj.IsOrigSmallCreditWOApp())
				{
					adjustedDocument = (ARRegister)ARDocument.Cache.Locate(adjustedDocument) ?? adjustedDocument;

					je.BatchModule.Current.Description = adjustedDocument.DocDesc;
					je.BatchModule.Update(je.BatchModule.Current);

					adjustedDocument.BatchNbr = adj.AdjBatchNbr;
					ARDocument.Update(adjustedDocument);
				}
			}
		}

		public virtual void ExtensionsPersist()
		{
			// Extension point used in customizations.
		}

		public virtual void ExtensionsPersisted()
		{
			// Extension point used in customizations.
		}

		public override void Persist()
		{
			RaiseBeforePersist();

			WithRetry(() =>
			{
			using (PXTransactionScope ts = new PXTransactionScope())
			{
				ARPayment_DocType_RefNbr.Cache.Persist(PXDBOperation.Insert);
				ARPayment_DocType_RefNbr.Cache.Persist(PXDBOperation.Update);

				ARDocument.Cache.Persist(PXDBOperation.Update);
				ARTran_TranType_RefNbr.Cache.Persist(PXDBOperation.Insert);
				ARTran_TranType_RefNbr.Cache.Persist(PXDBOperation.Update);
				ARTaxTran_TranType_RefNbr.Cache.Persist(PXDBOperation.Update);
				SVATConversionHistory.Cache.Persist(PXDBOperation.Insert);
				SVATConversionHistory.Cache.Persist(PXDBOperation.Update);
				this.Caches[typeof(INTran)].Persist(PXDBOperation.Update);
				ARPaymentChargeTran_DocType_RefNbr.Cache.Persist(PXDBOperation.Update);

				ARAdjust_AdjgDocType_RefNbr_CustomerID.Cache.Persist(PXDBOperation.Insert);
				ARAdjust_AdjgDocType_RefNbr_CustomerID.Cache.Persist(PXDBOperation.Update);
				ARAdjust_AdjgDocType_RefNbr_CustomerID.Cache.Persist(PXDBOperation.Delete);

				ARDoc_SalesPerTrans.Cache.Persist(PXDBOperation.Insert);
				ARDoc_SalesPerTrans.Cache.Persist(PXDBOperation.Update);

				Caches[typeof(ARHist)].Persist(PXDBOperation.Insert);

				Caches[typeof(CuryARHist)].Persist(PXDBOperation.Insert);

				Caches[typeof(ARBalances)].Persist(PXDBOperation.Insert);
				Caches[typeof(ARTranPost)].Persist(PXDBOperation.Insert);


				this.Caches[typeof(PMCommitment)].Persist(PXDBOperation.Insert);
				this.Caches[typeof(PMCommitment)].Persist(PXDBOperation.Update);
				this.Caches[typeof(PMCommitment)].Persist(PXDBOperation.Delete);
				this.Caches[typeof(PMHistoryAccum)].Persist(PXDBOperation.Insert);
				this.Caches[typeof(PMBudgetAccum)].Persist(PXDBOperation.Insert);
				this.Caches[typeof(PMForecastHistoryAccum)].Persist(PXDBOperation.Insert);

				Caches[typeof(ARTax)].Persist(PXDBOperation.Update);

				this.Caches<ItemCustSalesStats>().Persist(PXDBOperation.Insert);//see ProcessInventory extension

				soOrder.Cache.Persist(PXDBOperation.Update);
				soAdjust.Cache.Persist(PXDBOperation.Update);
				Caches[typeof(CADailySummary)].Persist(PXDBOperation.Insert);

				ExtensionsPersist();
				RaiseBeforeCommit();

				ts.Complete(this);
			}

			ARPayment_DocType_RefNbr.Cache.Persisted(false);
			ARDocument.Cache.Persisted(false);
			ARTran_TranType_RefNbr.Cache.Persisted(false);
			ARTaxTran_TranType_RefNbr.Cache.Persisted(false);
			this.Caches[typeof(INTran)].Persisted(false);
			ARAdjust_AdjgDocType_RefNbr_CustomerID.Cache.Persisted(false);

			Caches[typeof(ARHist)].Persisted(false);

			Caches[typeof(CuryARHist)].Persisted(false);

			Caches[typeof(ARBalances)].Persisted(false);
			Caches[typeof(ARTranPost)].Persisted(false);

			ARDoc_SalesPerTrans.Cache.Persisted(false);

			Caches[typeof(CADailySummary)].Persisted(false);

			Caches[typeof(ARTax)].Persisted(false);

			this.Caches<ItemCustSalesStats>().Persisted(false);//see ProcessInventory extension
			soOrder.Cache.Persisted(false);
			soAdjust.Cache.Persisted(false);
			});

			ExtensionsPersisted();
			RaiseAfterPersist();
		}

		protected bool _IsIntegrityCheck = false;
		/// <summary>
		/// Returns True if this is a Validate Balances context.
		/// </summary>
		public bool IsIntegrityCheck
		{
			get { return _IsIntegrityCheck; }
		}

		protected Customer CustomerIntegrityCheck =>
			IsIntegrityCheck ? this.Caches[typeof(Customer)].Current as Customer : null;

		protected string _IntegrityCheckStartingPeriod = null;

		public virtual void IntegrityCheckProc(Customer cust, string startPeriod)
		{
			_IsIntegrityCheck = true;
			_IntegrityCheckStartingPeriod = startPeriod;
			JournalEntry je = PXGraph.CreateInstance<JournalEntry>();
			je.SetOffline();

			Caches[typeof(Customer)].Current = cust;

			using (new PXConnectionScope())
			{
				_oldInvoiceRefresher = new OldInvoiceDateRefresher();
				bool hasPaymentsByLinesDoc = false;

				using (PXTransactionScope ts = new PXTransactionScope())
				{
					string minPeriod = "190001";

					ARHistoryDetDeleted maxHist = (ARHistoryDetDeleted)
						PXSelectGroupBy<ARHistoryDetDeleted,
							Where<ARHistoryDetDeleted.customerID, Equal<Current<Customer.bAccountID>>>,
						Aggregate<Max<ARHistoryDetDeleted.finPeriodID>>>
						.Select(this);

					if (maxHist != null && maxHist.FinPeriodID != null)
					{
						minPeriod = FinPeriodRepository.GetOffsetPeriodId(maxHist.FinPeriodID, 1, FinPeriod.organizationID.MasterValue);
					}

					if (string.IsNullOrEmpty(startPeriod) == false && string.Compare(startPeriod, minPeriod) > 0)
					{
						minPeriod = startPeriod;
					}
					FinPeriod prevPeriod = FinPeriodRepository.FindPrevPeriod(FinPeriod.organizationID.MasterValue, minPeriod);
					
					PXUpdateJoin<
							Set<ARHistory.finBegBalance, IsNull<ARHistory2.finYtdBalance, Zero>,
							Set<ARHistory.finPtdSales, Zero,
							Set<ARHistory.finPtdPayments, Zero,
							Set<ARHistory.finPtdCrAdjustments, Zero,
							Set<ARHistory.finPtdDrAdjustments, Zero,
							Set<ARHistory.finPtdDiscounts, Zero,
							Set<ARHistory.finPtdCOGS, Zero,
							Set<ARHistory.finPtdRGOL, Zero,
							Set<ARHistory.finPtdFinCharges, Zero,
							Set<ARHistory.finYtdBalance, IsNull<ARHistory2.finYtdBalance, Zero>,
							Set<ARHistory.finPtdDeposits, Zero,
							Set<ARHistory.finYtdDeposits, IsNull<ARHistory2.finYtdDeposits, Zero>,
							Set<ARHistory.finPtdItemDiscounts, Zero,
							Set<ARHistory.finYtdRetainageReleased, IsNull<ARHistory2.finYtdRetainageReleased, Zero>,
							Set<ARHistory.finPtdRetainageReleased, Zero,
							Set<ARHistory.finYtdRetainageWithheld, IsNull<ARHistory2.finYtdRetainageWithheld, Zero>,
							Set<ARHistory.finPtdRetainageWithheld, Zero,
							Set<ARHistory.finPtdRevalued, ARHistory.finPtdRevalued,
							Set<ARHistory.numberInvoicePaid, Zero,
							Set<ARHistory.paidInvoiceDays, Zero>>>>>>>>>>>>>>>>>>>>,
						ARHistory,
						LeftJoin<Branch,
							On<ARHistory.branchID, Equal<Branch.branchID>>,
						LeftJoin<FinPeriod,
							On<ARHistory.finPeriodID, Equal<FinPeriod.finPeriodID>,
							And<Branch.organizationID, Equal<FinPeriod.organizationID>>>,
						LeftJoin<OrganizationFinPeriodExt,
							  On<OrganizationFinPeriodExt.masterFinPeriodID, Equal<Required<OrganizationFinPeriodExt.masterFinPeriodID>>,
							  And<Branch.organizationID, Equal<OrganizationFinPeriodExt.organizationID>>>,
						LeftJoin<ARHistory2ByPeriod,
							On<ARHistory2ByPeriod.branchID, Equal<ARHistory.branchID>,
							And<ARHistory2ByPeriod.accountID, Equal<ARHistory.accountID>,
							And<ARHistory2ByPeriod.subID, Equal<ARHistory.subID>,
							And<ARHistory2ByPeriod.customerID, Equal<ARHistory.customerID>,
							And<ARHistory2ByPeriod.finPeriodID, Equal<OrganizationFinPeriodExt.prevFinPeriodID>>>>>>,
						LeftJoin<ARHistory2,
							  On<ARHistory2.branchID, Equal<ARHistory.branchID>,
							 And<ARHistory2.accountID, Equal<ARHistory.accountID>,
							 And<ARHistory2.subID, Equal<ARHistory.subID>,
							 And<ARHistory2.customerID, Equal<ARHistory.customerID>,
							 And<ARHistory2.finPeriodID, Equal<ARHistory2ByPeriod.lastActivityPeriod>>>>>>>>>>>,
						Where<ARHistory.customerID, Equal<Required<ARHist.customerID>>,
							And<FinPeriod.masterFinPeriodID, GreaterEqual<Required<FinPeriod.masterFinPeriodID>>>>>
						.Update(this, minPeriod, cust.BAccountID, minPeriod);

					PXUpdateJoin<
							Set<ARHistory.tranBegBalance, IsNull<ARHistory2.tranYtdBalance, Zero>,
							Set<ARHistory.tranPtdSales, Zero,
							Set<ARHistory.tranPtdPayments, Zero,
							Set<ARHistory.tranPtdCrAdjustments, Zero,
							Set<ARHistory.tranPtdDrAdjustments, Zero,
							Set<ARHistory.tranPtdDiscounts, Zero,
							Set<ARHistory.tranPtdRGOL, Zero,
							Set<ARHistory.tranPtdCOGS, Zero,
							Set<ARHistory.tranPtdFinCharges, Zero,
							Set<ARHistory.tranYtdBalance, IsNull<ARHistory2.tranYtdBalance, Zero>,
							Set<ARHistory.tranPtdDeposits, Zero,
							Set<ARHistory.tranYtdDeposits, IsNull<ARHistory2.tranYtdDeposits, Zero>,
							Set<ARHistory.tranPtdItemDiscounts, Zero,
							Set<ARHistory.tranYtdRetainageReleased, IsNull<ARHistory2.tranYtdRetainageReleased, Zero>,
							Set<ARHistory.tranPtdRetainageReleased, Zero,
							Set<ARHistory.tranYtdRetainageWithheld, IsNull<ARHistory2.tranYtdRetainageWithheld, Zero>,
							Set<ARHistory.tranPtdRetainageWithheld, Zero>>>>>>>>>>>>>>>>>,
						ARHistory,
						LeftJoin <ARHistory2ByPeriod,
							On<ARHistory2ByPeriod.branchID, Equal<ARHistory.branchID>,
							And<ARHistory2ByPeriod.accountID, Equal<ARHistory.accountID>,
							And<ARHistory2ByPeriod.subID, Equal<ARHistory.subID>,
							And<ARHistory2ByPeriod.customerID, Equal<ARHistory.customerID>,
							And<ARHistory2ByPeriod.finPeriodID, Equal<Required<FinPeriod.masterFinPeriodID>>>>>>>,
						LeftJoin < ARHistory2,
							On<ARHistory2.branchID, Equal<ARHistory.branchID>,
								And<ARHistory2.accountID, Equal<ARHistory.accountID>,
								And<ARHistory2.subID, Equal<ARHistory.subID>,
								And<ARHistory2.customerID, Equal<ARHistory.customerID>,
								And<ARHistory2.finPeriodID, Equal<ARHistory2ByPeriod.lastActivityPeriod>>>>>>>>,
						Where <ARHistory.customerID, Equal<Required<ARHist.customerID>>,
							And<ARHistory.finPeriodID, GreaterEqual<Required<ARHistory.finPeriodID>>>>>
						.Update(this, prevPeriod?.FinPeriodID, cust.BAccountID, minPeriod);

					PXUpdateJoin<
							Set<CuryARHistory.finBegBalance, IsNull<CuryARHistory2.finYtdBalance, Zero>,
							Set<CuryARHistory.finPtdSales, Zero,
							Set<CuryARHistory.finPtdPayments, Zero,
							Set<CuryARHistory.finPtdDrAdjustments, Zero,
							Set<CuryARHistory.finPtdCrAdjustments, Zero,
							Set<CuryARHistory.finPtdDiscounts, Zero,
							Set<CuryARHistory.finPtdRGOL, Zero,
							Set<CuryARHistory.finPtdCOGS, Zero,
							Set<CuryARHistory.finPtdFinCharges, Zero,
							Set<CuryARHistory.finYtdBalance, IsNull<CuryARHistory2.finYtdBalance, Zero>,
							Set<CuryARHistory.finPtdDeposits, Zero,
							Set<CuryARHistory.finYtdDeposits, IsNull<CuryARHistory2.finYtdDeposits, Zero>,
							Set<CuryARHistory.curyFinBegBalance, IsNull<CuryARHistory2.curyFinYtdBalance, Zero>,
							Set<CuryARHistory.curyFinPtdSales, Zero,
							Set<CuryARHistory.curyFinPtdPayments, Zero,
							Set<CuryARHistory.curyFinPtdDrAdjustments, Zero,
							Set<CuryARHistory.curyFinPtdCrAdjustments, Zero,
							Set<CuryARHistory.curyFinPtdDiscounts, Zero,
							Set<CuryARHistory.curyFinPtdFinCharges, Zero,
							Set<CuryARHistory.curyFinYtdBalance, IsNull<CuryARHistory2.curyFinYtdBalance, Zero>,
							Set<CuryARHistory.curyFinPtdDeposits, Zero,
							Set<CuryARHistory.curyFinYtdDeposits, IsNull<CuryARHistory2.curyFinYtdDeposits, Zero>,
							Set<CuryARHistory.curyFinPtdRetainageWithheld, Zero,
							Set<CuryARHistory.finPtdRetainageWithheld, Zero,
							Set<CuryARHistory.curyFinYtdRetainageWithheld, IsNull<CuryARHistory2.curyFinYtdRetainageWithheld, Zero>,
							Set<CuryARHistory.finYtdRetainageWithheld, IsNull<CuryARHistory2.finYtdRetainageWithheld, Zero>,
							Set<CuryARHistory.curyFinPtdRetainageReleased, Zero,
							Set<CuryARHistory.finPtdRetainageReleased, Zero,
							Set<CuryARHistory.curyFinYtdRetainageReleased, IsNull<CuryARHistory2.curyFinYtdRetainageReleased, Zero>,
							Set<CuryARHistory.finYtdRetainageReleased, IsNull<CuryARHistory2.finYtdRetainageReleased, Zero>,
							Set<CuryARHistory.finPtdRevalued, CuryARHistory.finPtdRevalued
							>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>,
						CuryARHistory,
						LeftJoin<Branch,
							On<CuryARHistory.branchID, Equal<Branch.branchID>>,
						LeftJoin<FinPeriod,
							On<CuryARHistory.finPeriodID, Equal<FinPeriod.finPeriodID>,
							And<Branch.organizationID, Equal<FinPeriod.organizationID>>>,
						LeftJoin<OrganizationFinPeriodExt,
							  On<OrganizationFinPeriodExt.masterFinPeriodID, Equal<Required<OrganizationFinPeriodExt.masterFinPeriodID>>,
							  And<Branch.organizationID, Equal<OrganizationFinPeriodExt.organizationID>>>,
						LeftJoin<ARHistoryByPeriod,
							On<ARHistoryByPeriod.branchID, Equal<CuryARHistory.branchID>,
							And<ARHistoryByPeriod.accountID, Equal<CuryARHistory.accountID>,
							And<ARHistoryByPeriod.subID, Equal<CuryARHistory.subID>,
							And<ARHistoryByPeriod.customerID, Equal<CuryARHistory.customerID>,
							And<ARHistoryByPeriod.curyID, Equal<CuryARHistory.curyID>,
							And<ARHistoryByPeriod.finPeriodID, Equal<OrganizationFinPeriodExt.prevFinPeriodID>>>>>>>,
						LeftJoin<CuryARHistory2,
							On<CuryARHistory2.branchID, Equal<CuryARHistory.branchID>,
							And<CuryARHistory2.accountID, Equal<CuryARHistory.accountID>,
							And<CuryARHistory2.subID, Equal<CuryARHistory.subID>,
							And<CuryARHistory2.customerID, Equal<CuryARHistory.customerID>,
							And<CuryARHistory2.curyID, Equal<CuryARHistory.curyID>,
							And<CuryARHistory2.finPeriodID, Equal<ARHistoryByPeriod.lastActivityPeriod>>>>>>>>>>>>,
						Where<CuryARHistory.customerID, Equal<Required<CuryARHist.customerID>>,
							And<FinPeriod.masterFinPeriodID, GreaterEqual<Required<FinPeriod.finPeriodID>>>>>
						.Update(this, minPeriod, cust.BAccountID, minPeriod);

					PXUpdateJoin<
							Set<CuryARHistory.tranBegBalance, IsNull<CuryARHistory2.tranYtdBalance, Zero>,
							Set<CuryARHistory.tranPtdSales, Zero,
							Set<CuryARHistory.tranPtdPayments, Zero,
							Set<CuryARHistory.tranPtdDrAdjustments, Zero,
							Set<CuryARHistory.tranPtdCrAdjustments, Zero,
							Set<CuryARHistory.tranPtdDiscounts, Zero,
							Set<CuryARHistory.tranPtdRGOL, Zero,
							Set<CuryARHistory.tranPtdCOGS, Zero,
							Set<CuryARHistory.tranPtdFinCharges, Zero,
							Set<CuryARHistory.tranYtdBalance, IsNull<CuryARHistory2.tranYtdBalance, Zero>,
							Set<CuryARHistory.tranPtdDeposits, Zero,
							Set<CuryARHistory.tranYtdDeposits, IsNull<CuryARHistory2.tranYtdDeposits, Zero>,
							Set<CuryARHistory.curyTranBegBalance, IsNull<CuryARHistory2.curyTranYtdBalance, Zero>,
							Set<CuryARHistory.curyTranPtdSales, Zero,
							Set<CuryARHistory.curyTranPtdPayments, Zero,
							Set<CuryARHistory.curyTranPtdDrAdjustments, Zero,
							Set<CuryARHistory.curyTranPtdCrAdjustments, Zero,
							Set<CuryARHistory.curyTranPtdDiscounts, Zero,
							Set<CuryARHistory.curyTranPtdFinCharges, Zero,
							Set<CuryARHistory.curyTranYtdBalance, IsNull<CuryARHistory2.curyTranYtdBalance, Zero>,
							Set<CuryARHistory.curyTranPtdDeposits, Zero,
							Set<CuryARHistory.curyTranYtdDeposits, IsNull<CuryARHistory2.curyTranYtdDeposits, Zero>,
							Set<CuryARHistory.curyTranPtdRetainageWithheld, Zero,
							Set<CuryARHistory.tranPtdRetainageWithheld, Zero,
							Set<CuryARHistory.curyTranYtdRetainageWithheld, IsNull<CuryARHistory2.curyTranYtdRetainageWithheld, Zero>,
							Set<CuryARHistory.tranYtdRetainageWithheld, IsNull<CuryARHistory2.tranYtdRetainageWithheld, Zero>,
							Set<CuryARHistory.curyTranPtdRetainageReleased, Zero,
							Set<CuryARHistory.tranPtdRetainageReleased, Zero,
							Set<CuryARHistory.curyTranYtdRetainageReleased, IsNull<CuryARHistory2.curyTranYtdRetainageReleased, Zero>,
							Set<CuryARHistory.tranYtdRetainageReleased, IsNull<CuryARHistory2.tranYtdRetainageReleased, Zero>
								>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>,
						CuryARHistory,
						LeftJoin<ARHistoryByPeriod,
							On<ARHistoryByPeriod.branchID, Equal<CuryARHistory.branchID>,
							And<ARHistoryByPeriod.accountID, Equal<CuryARHistory.accountID>,
							And<ARHistoryByPeriod.subID, Equal<CuryARHistory.subID>,
							And<ARHistoryByPeriod.customerID, Equal<CuryARHistory.customerID>,
							And<ARHistoryByPeriod.curyID, Equal<CuryARHistory.curyID>,
							And<ARHistoryByPeriod.finPeriodID, Equal<Required<CuryARHistory.finPeriodID>>>>>>>>,
						LeftJoin<CuryARHistory2,
							On<CuryARHistory2.branchID, Equal<CuryARHistory.branchID>,
								And<CuryARHistory2.accountID, Equal<CuryARHistory.accountID>,
								And<CuryARHistory2.subID, Equal<CuryARHistory.subID>,
								And<CuryARHistory2.customerID, Equal<CuryARHistory.customerID>,
								And<CuryARHistory2.curyID, Equal<CuryARHistory.curyID>,
								And<CuryARHistory2.finPeriodID, Equal<ARHistoryByPeriod.lastActivityPeriod>>>>>>>>>,
						Where <CuryARHistory.customerID, Equal<Required<CuryARHist.customerID>>,
							And<CuryARHistory.finPeriodID, GreaterEqual<Required<CuryARHistory.finPeriodID>>>>>
						.Update(this, prevPeriod?.FinPeriodID, cust.BAccountID, minPeriod);

					PXDatabase.Update<ARBalances>(
						new PXDataFieldAssign<ARBalances.totalOpenOrders>(0m),
						new PXDataFieldAssign<ARBalances.unreleasedBal>(0m),
						new PXDataFieldAssign<ARBalances.currentBal>(0m),
						new PXDataFieldAssign<ARBalances.oldInvoiceDate>(null),
						new PXDataFieldAssign<ARBalances.lastDocDate>(null),
						new PXDataFieldAssign<ARBalances.statementRequired>(true),
						new PXDataFieldRestrict<ARBalances.customerID>(PXDbType.Int, 4, cust.BAccountID, PXComp.EQ)
					);
					PXDatabase.Delete<ARTranPost>(
						new PXDataFieldRestrict<ARTranPost.customerID>(PXDbType.Int, 4, cust.BAccountID, PXComp.EQ),
							new PXDataFieldRestrict<ARTranPost.finPeriodID>(PXDbType.VarChar, minPeriod.Length, minPeriod, PXComp.GE)
					);

					HashedList<ARRegister> custdocs = GetDocumentsForIntegrityCheckProc(minPeriod);
					List<ARRegister> invoices = new List<ARRegister>();
					List<ARRegister> payments = new List<ARRegister>();
					ARDocument.Cache.Clear();
					foreach (ARRegister custdoc in custdocs)
					{
						if (custdoc.Payable == true || ARDocType.HasBothInvoiceAndPaymentParts(custdoc.DocType))
						{
							invoices.Add(custdoc);
						}

						if (custdoc.Paying == true || ARDocType.HasBothInvoiceAndPaymentParts(custdoc.DocType))
						{
							payments.Add(custdoc);
						}
					}

					invoices.Sort((ARRegister docA, ARRegister docB) =>
					{
						Func<ARRegister, short?> retainageSortOrder = new Func<ARRegister, short?>((doc) =>
						{
							// Sort order for the Retainage documents validation:
							// 0. Original retainage Document (Invoice or CreditMemo),
							// 1. Child retainage Document (Invoice or CreditMemo),
							// 2. Child retainage reversing Document (CreditMemo or DebitMemo),
							// 3. Original retainage reversing Document (CreditMemo or DebitMemo).
							// 
							return doc.RetainageApply == true || doc.IsRetainageDocument == true
								? doc.IsRetainageReversing != true
									? doc.RetainageApply == true ? (short)0 : (short)1
									: doc.IsRetainageDocument == true ? (short)2 : (short)3
								: doc.SortOrder;
						});

						return ((IComparable)retainageSortOrder(docA)).CompareTo(retainageSortOrder(docB));
					});

					foreach (ARRegister custdoc in invoices)
					{
						ARRegister doc = custdoc;

						je.Clear();

						//mark as updated so that doc will not expire from cache and update with Released = 1 will not override balances/amount in document
						ARDocument.Cache.SetStatus(doc, PXEntryStatus.Updated);

						doc.Released = false;

						foreach (PXResult<ARInvoice, CurrencyInfo, Terms, Customer, Account> res in ARInvoice_DocType_RefNbr.Select(doc.DocType, doc.RefNbr))
						{
							//must check for CM application in different period
							if (doc.Released == false)
							{
								SegregateBatch(je, doc.BranchID, doc.CuryID, doc.DocDate, doc.FinPeriodID, doc.DocDesc, (CurrencyInfo)res);
							}

								List<PMRegister> pmDocs = new List<PMRegister>();
								ReleaseInvoice(je, doc, res, pmDocs);
							doc.Released = true;
							}

						ARDocument.Cache.Update(doc);
						}

					payments.Sort((ARRegister docA, ARRegister docB) =>((IComparable)docA.SortOrder).CompareTo(docB.SortOrder));

					foreach (ARRegister custdoc in payments)
					{
						ARRegister doc = custdoc;

						je.Clear();

						//mark as updated so that doc will not expire from cache and update with Released = 1 will not override balances/amount in document
						ARDocument.Cache.SetStatus(doc, PXEntryStatus.Updated);

						doc.Released = ARDocType.HasBothInvoiceAndPaymentParts(custdoc.DocType);
						bool requireOpenDoc = false;
						foreach (PXResult<ARPayment, CurrencyInfo, Currency, Customer, CashAccount> res in ARPayment_DocType_RefNbr.Select(doc.DocType, doc.RefNbr, doc.CustomerID))
						{
							ARPayment payment = res;
							CurrencyInfo info = res;
							Currency paycury = res;
							Customer customer = res;
							CashAccount cashacct = res;

							SegregateBatch(je, doc.BranchID, doc.CuryID, payment.AdjDate, payment.AdjFinPeriodID, payment.DocDesc, info);

							int OrigAdjCntr = (int)doc.AdjCntr;
							doc.AdjCntr = -1;
							Amount docBal = new Amount();
							Tuple<ARAdjust, CurrencyInfo> lastAdjustment = null;

							while (doc.AdjCntr < OrigAdjCntr)
							{
								// We should use the same CurrencyInfo for Payment
								// and its applications to save proper consolidation
								// for generated GL transactions
								// 
								CM.CurrencyInfo new_info = GetCurrencyInfoCopyForGL(je, info);

								if (doc.AdjCntr == -1 || (doc.DocType != ARDocType.CashSale && doc.DocType != ARDocType.CashReturn))
								{
									ProcessPayment(je, doc, new PXResult<ARPayment, CurrencyInfo, Currency, Customer, CashAccount>(payment, CurrencyInfo.GetEX(new_info), paycury, customer, cashacct));
								}

								PXResultset<ARAdjust> adjustments = ARAdjust_AdjgDocType_RefNbr_CustomerID.Select(doc.DocType, doc.RefNbr, _IsIntegrityCheck, doc.AdjCntr);
								var last = ProcessAdjustments(je, adjustments, doc, payment, customer, new_info, paycury);
								if (!_IsIntegrityCheck ||
								    lastAdjustment == null ||
								    lastAdjustment.Item1.AdjdDocType != ARDocType.SmallCreditWO)
									lastAdjustment = last;
								docBal += new Amount(doc.CuryDocBal, doc.DocBal);
								VerifyPaymentRoundAndClose(je, doc, payment, customer, CurrencyInfo.GetEX(new_info), paycury, lastAdjustment);
								doc.AdjCntr++;


								doc.Released = true;
							}

							if (docBal.Base != 0 && 
							    (doc.DocType == ARDocType.VoidPayment ||
							     doc.DocType == ARDocType.VoidRefund))
							{
								ProcessVoidPaymentTranPost(doc, docBal);
							}
							
							if (doc.Voided == true && doc.DocType == ARDocType.SmallBalanceWO)
							{
								ProcessVoidWOTranPost(doc, lastAdjustment.Item1);
							}

							ARAdjust reversal = ARAdjust_AdjgDocType_RefNbr_CustomerID.Select(doc.DocType, doc.RefNbr, _IsIntegrityCheck, OrigAdjCntr);
							if (reversal != null && reversal.IsInitialApplication != true)
							{
								requireOpenDoc = true;
							}
						}

						if (requireOpenDoc && doc.OpenDoc != true)
						{
								doc.OpenDoc = true;
							RaisePaymentEvent(doc, ARPayment.Events.Select(e=>e.OpenDocument));
						}

						ARDocument.Cache.Update(doc);
					}

					#region Validate Customer Balances processes cross customer applications to customer's

					foreach (ARRegister custdoc in invoices.Union(payments))
					{
						if(custdoc.PaymentsByLinesAllowed == true)
							hasPaymentsByLinesDoc =true;

						je.Clear();

						ARRegister doc = custdoc;
						ARDocument.Cache.SetStatus(doc, PXEntryStatus.Updated);

							var adjustments = PXSelectJoin<ARAdjust,
								InnerJoin<CurrencyInfo, On<CurrencyInfo.curyInfoID, Equal<ARAdjust.adjdCuryInfoID>>,
								InnerJoin<Currency, On<Currency.curyID, Equal<CurrencyInfo.curyID>>,
								LeftJoinSingleTable<ARInvoice, On<ARInvoice.docType, Equal<ARAdjust.adjdDocType>, And<ARInvoice.refNbr, Equal<ARAdjust.adjdRefNbr>>>,
								LeftJoin<ARPayment, On<ARPayment.docType, Equal<ARAdjust.adjgDocType>, And<ARPayment.refNbr, Equal<ARAdjust.adjgRefNbr>>>,
								LeftJoin<ARRegister, On<ARRegister.docType, Equal<ARAdjust.adjdDocType>, And<ARRegister.refNbr, Equal<ARAdjust.adjdRefNbr>>>>>>>>,
								Where<ARAdjust.adjdDocType, Equal<Required<ARAdjust.adjdDocType>>,
									And<ARAdjust.adjdRefNbr, Equal<Required<ARAdjust.adjdRefNbr>>,
									And<ARAdjust.adjdCustomerID, Equal<Required<ARAdjust.adjdCustomerID>>,
									And<ARAdjust.adjdCustomerID, NotEqual<ARAdjust.customerID>,
								And<ARAdjust.released, Equal<True>>>>>>>.Select(this, doc.DocType, doc.RefNbr, doc.CustomerID);

						if(adjustments.Count>0)
							ProcessAdjustmentsOnlyAdjusted(je, adjustments);

						ARDocument.Cache.Update(doc);
					}
					#endregion

					// Save Cache values of ARBalances.LastDocDate and ARBalances.StatementRequired (LeftJoin Branch is a hack)
					List<PXResult<ARBalances>> oldARBalances = PXSelectJoin<
						ARBalances, 
						LeftJoin<Branch, 
							On<Branch.branchID, Equal<ARBalances.branchID>>>, 
						Where<ARBalances.customerID, Equal<Current<Customer.bAccountID>>>>
						.Select(this).ToList();

					Caches[typeof(ARBalances)].Clear();

					#region Restore to Cache values of ARBalances.LastDocDate and ARBalances.StatementRequired
					foreach (ARBalances bal in oldARBalances)
					{
						Caches[typeof(ARBalances)].Insert(new ARBalances()
						{
							BranchID = bal.BranchID,
							CustomerID = bal.CustomerID,
							CustomerLocationID = bal.CustomerLocationID,
							LastDocDate = bal.LastDocDate,
							StatementRequired = true
						});
					}
					#endregion

					foreach (ARRegister ardoc in ARDocument.Cache.Updated)
					{
						ARDocument.Cache.PersistUpdated(ardoc);
					}

					#region UpdateARBalances SOOrder
					var _SOOrder = new PXSelectReadonly<SOOrder, Where<SOOrder.customerID, Equal<Required<SOOrder.customerID>>, And<SOOrder.inclCustOpenOrders, Equal<True>, And<SOOrder.cancelled, Equal<False>, And<SOOrder.hold, Equal<False>, And<SOOrder.creditHold, Equal<False>>>>>>>(this);

					// fields are used in UpdateARBalances
					using (new PXFieldScope(_SOOrder.View,
						typeof(SOOrder.orderType)
						, typeof(SOOrder.orderNbr)
						, typeof(SOOrder.orderDate)
						, typeof(SOOrder.customerID)
						, typeof(SOOrder.customerLocationID)
						, typeof(SOOrder.branchID)
						, typeof(SOOrder.aRDocType)
						, typeof(SOOrder.shipmentCntr)
						, typeof(SOOrder.inclCustOpenOrders)
						, typeof(SOOrder.cancelled)
						, typeof(SOOrder.hold)
						, typeof(SOOrder.creditHold)
						, typeof(SOOrder.noteID)
						, typeof(SOOrder.unbilledOrderTotal)
						, typeof(SOOrder.openOrderTotal)
					))
					foreach (SOOrder order in _SOOrder.Select(cust.BAccountID))
					{
						ARReleaseProcess.UpdateARBalances(this, order, order.UnbilledOrderTotal, order.OpenOrderTotal); 
					}
					#endregion

					#region UpdateARBalances ARRegister Open
					var _ARRegister1 = new PXSelectReadonly<ARRegister, Where<ARRegister.customerID, Equal<Required<ARRegister.customerID>>, And<ARRegister.released, Equal<True>, And<ARRegister.openDoc, Equal<True>>>>>(this);

					// fields are used in UpdateARBalances
					using (new PXFieldScope(_ARRegister1.View,
						typeof(ARRegister.docType)
						, typeof(ARRegister.refNbr)
						, typeof(ARRegister.docDate)
						, typeof(ARRegister.customerID)
						, typeof(ARRegister.customerLocationID)
						, typeof(ARRegister.branchID)
						, typeof(ARRegister.released)
						, typeof(ARRegister.voided)
						, typeof(ARRegister.hold)
						, typeof(ARRegister.scheduled)
						, typeof(ARRegister.noteID)
						, typeof(ARRegister.docBal)
						, typeof(ARRegister.origDocAmt)
					))
					foreach (ARRegister ardoc in _ARRegister1.Select(cust.BAccountID))
					{
						ARReleaseProcess.UpdateARBalances(this, ardoc, ardoc.DocBal);
						UpdateARBalancesDates(ardoc);
					}
					#endregion

					#region UpdateARBalances ARRegister Balanced
					var _ARRegister2 = new PXSelectReadonly<ARRegister, Where<ARRegister.customerID, Equal<Required<ARRegister.customerID>>, And<ARRegister.released, Equal<False>, And<ARRegister.hold, Equal<False>, And<ARRegister.voided, Equal<False>, And<ARRegister.scheduled, Equal<False>>>>>>>(this);

					// fields are used in UpdateARBalances
					using (new PXFieldScope(_ARRegister2.View,
						typeof(ARRegister.docType)
						, typeof(ARRegister.refNbr)
						, typeof(ARRegister.docDate)
						, typeof(ARRegister.customerID)
						, typeof(ARRegister.customerLocationID)
						, typeof(ARRegister.branchID)
						, typeof(ARRegister.released)
						, typeof(ARRegister.voided)
						, typeof(ARRegister.hold)
						, typeof(ARRegister.scheduled)
						, typeof(ARRegister.noteID)
						, typeof(ARRegister.docBal)
						, typeof(ARRegister.origDocAmt)
					))
					foreach (ARRegister ardoc in _ARRegister2.Select(cust.BAccountID))
					{
						ARReleaseProcess.UpdateARBalances(this, ardoc, ardoc.OrigDocAmt);
					}
					#endregion

					#region UpdateARBalances ARInvoice
					var _ARInvoice = new PXSelectReadonly<ARInvoice, Where<ARInvoice.customerID, Equal<Required<ARInvoice.customerID>>, And<ARInvoice.creditHold, Equal<True>, And<ARInvoice.released, Equal<False>, And<ARInvoice.hold, Equal<False>, And<ARInvoice.voided, Equal<False>, And<ARInvoice.scheduled, Equal<False>>>>>>>>(this);

					// fields are used in UpdateARBalances
					using (new PXFieldScope(_ARInvoice.View,
						typeof(ARInvoice.docType)
						, typeof(ARInvoice.refNbr)
						, typeof(ARInvoice.docDate)
						, typeof(ARInvoice.customerID)
						, typeof(ARInvoice.customerLocationID)
						, typeof(ARInvoice.branchID)
						, typeof(ARInvoice.released)
						, typeof(ARInvoice.voided)
						, typeof(ARInvoice.hold)
						, typeof(ARInvoice.creditHold)
						, typeof(ARInvoice.scheduled)
						, typeof(ARInvoice.noteID)
						, typeof(ARInvoice.docBal)
						, typeof(ARInvoice.origDocAmt)
					))
					foreach (ARInvoice ardoc in _ARInvoice.Select(cust.BAccountID))
					{
						ardoc.CreditHold = false;
						ARReleaseProcess.UpdateARBalances(this, ardoc, -ardoc.OrigDocAmt);
					}
					#endregion

					TranPost.Cache.Persist(PXDBOperation.Insert);
					StatementDetailsView.Cache.Persist(PXDBOperation.Update);

					if (hasPaymentsByLinesDoc)
					ARTran_TranType_RefNbr.Cache.Persist(PXDBOperation.Update);

					Caches[typeof(ARAdjust)].Persist(PXDBOperation.Update);
					
					Caches[typeof(ARHist)].Persist(PXDBOperation.Insert);

					Caches[typeof(CuryARHist)].Persist(PXDBOperation.Insert);

					Caches[typeof(ARBalances)].Persist(PXDBOperation.Insert);

					ts.Complete(this);
				}

				_oldInvoiceRefresher.CommitRefresh(this);

				ARDocument.Cache.Persisted(false);

				Caches[typeof(ARHist)].Persisted(false);

				Caches[typeof(CuryARHist)].Persisted(false);

				Caches[typeof(ARBalances)].Persisted(false);
					
				TranPost.Cache.Persisted(false);

				if (hasPaymentsByLinesDoc)
				ARTran_TranType_RefNbr.Cache.Persisted(false);
				
				Caches[typeof(ARAdjust)].Persisted(false);

			}
		}

		protected virtual HashedList<ARRegister> GetDocumentsForIntegrityCheckProc(string minPeriod)
		{
			// Customer released documents, that are created or closed after MinPeriod
			HashedList<ARRegister> custdocs = new HashedList<ARRegister>(ARDocument.Cache.GetComparer());
			foreach (PXResult<ARRegister, ARInvoice, ARPayment> rec in
				PXSelectJoin<ARRegister,
					LeftJoinSingleTable<ARInvoice,
						On<ARInvoice.docType, Equal<ARRegister.docType>,
						And<ARInvoice.refNbr, Equal<ARRegister.refNbr>>>,
					LeftJoinSingleTable<ARPayment,
						On<ARPayment.docType, Equal<ARRegister.docType>,
						And<ARPayment.refNbr, Equal<ARRegister.refNbr>>>>>,
				Where<
					ARRegister.customerID, Equal<Current<Customer.bAccountID>>,
					And<ARRegister.released, Equal<True>,
					And<
						Where<ARRegister.tranPeriodID, GreaterEqual<Required<ARRegister.tranPeriodID>>,
							Or<ARRegister.closedTranPeriodID,
							GreaterEqual<Required<ARRegister.closedTranPeriodID>>>>>>>>
				.Select(this, minPeriod, minPeriod))
			{
				ARRegister doc = GetFullDocument(rec);
				if(doc != null)
					custdocs.Add(doc);
			}

			// Original retainage documents
			if (PXAccess.FeatureInstalled<FeaturesSet.retainage>())
			{
				PXResultset<ARRegister> retainage =
					PXSelectJoin<ARRegister,
					LeftJoinSingleTable<ARInvoice,
						On<ARInvoice.docType, Equal<ARRegister.docType>,
						And<ARInvoice.refNbr, Equal<ARRegister.refNbr>>>,
					InnerJoin<ARRegisterAlias,
						On<ARRegister.docType, Equal<ARRegisterAlias.origDocType>,
							And<ARRegister.refNbr, Equal<ARRegisterAlias.origRefNbr>>>>>,
					Where<
						ARRegisterAlias.customerID, Equal<Current<Customer.bAccountID>>,
						And<ARRegisterAlias.isRetainageDocument, Equal<True>,
						And<ARRegisterAlias.released, Equal<True>,
						And<
							Where<ARRegisterAlias.tranPeriodID, GreaterEqual<Required<ARRegisterAlias.tranPeriodID>>,
							Or<ARRegisterAlias.closedTranPeriodID, GreaterEqual<Required<ARRegisterAlias.closedTranPeriodID>>>>>>>>>
						.Select(this, minPeriod, minPeriod);

				foreach (var retdoc in retainage)
				{
					if (!custdocs.Contains((ARRegister)retdoc))
					{
						ARInvoice doc = PXResult.Unwrap<ARInvoice>(retdoc);
						PXCache<ARRegister>.RestoreCopy(doc, PXResult.Unwrap<ARRegister>(retdoc));
						custdocs.Add(new PXResult<ARRegister>(doc));

						foreach (PXResult<ARAdjust, ARRegister> docadj in SelectFrom<ARAdjust>
								.InnerJoin<ARRegister>.On<ARAdjust.adjgDocType.IsEqual<ARRegister.docType>
									.And<ARAdjust.adjgRefNbr.IsEqual<ARRegister.refNbr>>>
								.Where<ARAdjust.adjdDocType.IsEqual<@P.AsString>
									.And<ARAdjust.adjdRefNbr.IsEqual<@P.AsString>>
									.And<ARRegister.released.IsEqual<True>>>
									.View.SelectMultiBound(this, null, new object[] { doc.DocType, doc.RefNbr }))
						{
							if (!custdocs.Contains((ARRegister)docadj))
							{
								custdocs.Add(new PXResult<ARRegister>(docadj));
							}
						}
					}
				}
			}

			// Direct adjustments for customer documents
			PXResultset<ARRegister> adjustments = GetDirectAdjustmentsForIntegrityCheckProc(minPeriod);
			custdocs.AddRange(adjustments.RowCast<ARRegister>());

			// Infinite-level adjustments
			GetAllReleasedAdjustments(custdocs, adjustments, minPeriod);
			return custdocs;
		}

		private ARRegister GetFullDocument(PXResult<ARRegister, ARInvoice, ARPayment> rec)
		{
			ARInvoice invoice = rec;
			ARPayment payment = rec;
			ARRegister result = null;

			// Restore full invoice / payment from the "single table" stripped version.
			// 
			if (invoice?.RefNbr != null)
			{
				PXCache<ARRegister>.RestoreCopy(invoice, (ARRegister)rec);
				result = invoice;
			}
			else if (payment?.RefNbr != null)
			{
				PXCache<ARRegister>.RestoreCopy(payment, (ARRegister)rec);
				result = payment;
			}
			return result;
		}

		private IEnumerable<PXResult<ARRegister>> GetFullDocuments(IEnumerable<PXResult<ARRegister>> list)
		{
			foreach (PXResult<ARRegister, ARInvoice, ARPayment> rec in list)
			{
				ARRegister doc = GetFullDocument(rec);
				if (doc != null)
					yield return new PXResult<ARRegister>(doc);
			}
		}
		protected virtual PXResultset<ARRegister> GetDirectAdjustmentsForIntegrityCheckProc(string minPeriod)
		{
			var adjgs =
				PXSelectJoin<ARRegister,
					LeftJoinSingleTable<ARInvoice,
						On<ARInvoice.docType, Equal<ARRegister.docType>,
						And<ARInvoice.refNbr, Equal<ARRegister.refNbr>>>,
					LeftJoinSingleTable<ARPayment,
						On<ARPayment.docType, Equal<ARRegister.docType>,
						And<ARPayment.refNbr, Equal<ARRegister.refNbr>>>>>,
				Where<ARRegister.customerID, Equal<Current<Customer.bAccountID>>,
					And<ARRegister.tranPeriodID, Less<Required<ARRegister.tranPeriodID>>,
					And<ARRegister.released, Equal<True>,
					And2<Where<ARRegister.closedTranPeriodID, Less<Required<ARRegister.closedTranPeriodID>>, 
							Or<ARRegister.closedTranPeriodID, IsNull>>,
					And<Exists<Select2<ARAdjust,
						InnerJoin<Standalone.ARRegister2, On<
							Standalone.ARRegister2.docType, Equal<ARAdjust.adjdDocType>,
							And<Standalone.ARRegister2.refNbr, Equal<ARAdjust.adjdRefNbr>>>>,
						Where2<
					Where<ARAdjust.adjgDocType, Equal<ARRegister.docType>,
						Or<ARAdjust.adjgDocType, Equal<ARDocType.payment>, And<ARRegister.docType, Equal<ARDocType.voidPayment>,
						Or<ARAdjust.adjgDocType, Equal<ARDocType.prepayment>, And<ARRegister.docType, Equal<ARDocType.voidPayment>>>>>>,
							And<ARAdjust.adjgRefNbr, Equal<ARRegister.refNbr>,
					And2<Where<Standalone.ARRegister2.closedTranPeriodID, GreaterEqual<Required<Standalone.ARRegister2.closedTranPeriodID>>,
						Or<ARAdjust.adjgTranPeriodID, GreaterEqual<Required<ARAdjust.adjdTranPeriodID>>>>,
							And<ARAdjust.released, Equal<True>>>>>>>>>>>>>
					.Select(this, minPeriod, minPeriod, minPeriod, minPeriod);

			var adjds =
				PXSelectJoin<ARRegister,
					LeftJoinSingleTable<ARInvoice,
						On<ARInvoice.docType, Equal<ARRegister.docType>,
						And<ARInvoice.refNbr, Equal<ARRegister.refNbr>>>,
					LeftJoinSingleTable<ARPayment,
						On<ARPayment.docType, Equal<ARRegister.docType>,
						And<ARPayment.refNbr, Equal<ARRegister.refNbr>>>>>,
				Where<ARRegister.customerID, Equal<Current<Customer.bAccountID>>,
					And<ARRegister.tranPeriodID, Less<Required<ARRegister.tranPeriodID>>,
					And<ARRegister.released, Equal<True>,
					And2<Where<ARRegister.closedTranPeriodID, Less<Required<ARRegister.closedTranPeriodID>>, Or<ARRegister.closedTranPeriodID, IsNull>>,
					And<Exists<Select2<ARAdjust,
						InnerJoin<Standalone.ARRegister2, On<
								Standalone.ARRegister2.docType, Equal<ARAdjust.adjgDocType>,
								And<Standalone.ARRegister2.refNbr, Equal<ARAdjust.adjgRefNbr>>>>,
							Where2<Where<ARAdjust.adjdDocType, Equal<ARRegister.docType>,
						Or<ARAdjust.adjdDocType, Equal<ARDocType.payment>, And<ARRegister.docType, Equal<ARDocType.voidPayment>,
						Or<ARAdjust.adjdDocType, Equal<ARDocType.prepayment>, And<ARRegister.docType, Equal<ARDocType.voidPayment>>>>>>,
							And<ARAdjust.adjdRefNbr,Equal<ARRegister.refNbr>,
					And2<Where<Standalone.ARRegister2.closedTranPeriodID, GreaterEqual<Required<Standalone.ARRegister2.closedTranPeriodID>>,
						Or<ARAdjust.adjgTranPeriodID, GreaterEqual<Required<ARAdjust.adjgTranPeriodID>>>>,
							And<ARAdjust.released, Equal<True>>>>>>>
					>>>>>>
					.Select(this, minPeriod, minPeriod, minPeriod, minPeriod);

			var result = new PXResultset<ARRegister>();
			result.AddRange(GetFullDocuments(adjgs));
			result.AddRange(GetFullDocuments(adjds));
			return result;
		}

		protected virtual void GetAllReleasedAdjustments(HashedList<ARRegister> documents, PXResultset<ARRegister> startFrom, string minPeriod)
		{
			var newlyFoundDocsKeys = startFrom.RowCast<ARRegister>().Select(_ => new { DocType = _.DocType, RefNbr = _.RefNbr }).ToHashSet();
			while (newlyFoundDocsKeys.Any())
			{
				var currentList = newlyFoundDocsKeys.ToList();
				newlyFoundDocsKeys.Clear();
				foreach (var source in currentList)
				{
					var adjustments =
						PXSelectJoin<ARRegister,
							LeftJoinSingleTable<ARInvoice,
								On<ARInvoice.docType, Equal<ARRegister.docType>,
								And<ARInvoice.refNbr, Equal<ARRegister.refNbr>>>,
							LeftJoinSingleTable<ARPayment,
								On<ARPayment.docType, Equal<ARRegister.docType>,
								And<ARPayment.refNbr, Equal<ARRegister.refNbr>>>>>,
							Where<ARRegister.customerID, Equal<Current<Customer.bAccountID>>,
							And2<Exists<Select<ARAdjust, 
								Where2<Where<ARAdjust.adjgDocType, Equal<ARRegister.docType>,
								Or<ARAdjust.adjgDocType, Equal<ARDocType.payment>, And<ARRegister.docType, Equal<ARDocType.voidPayment>,
								Or<ARAdjust.adjgDocType, Equal<ARDocType.prepayment>, And<ARRegister.docType, Equal<ARDocType.voidPayment>>>>>>,
								And<ARAdjust.adjgRefNbr, Equal<ARRegister.refNbr>,
							And<ARAdjust.adjdDocType, Equal<Required<ARAdjust.adjdDocType>>,
							And<ARAdjust.adjdRefNbr, Equal<Required<ARAdjust.adjdRefNbr>>,
								And<ARAdjust.released, Equal<True>>>>>>>>,
							And<ARRegister.tranPeriodID, Less<Required<ARRegister.tranPeriodID>>,
							And<ARRegister.released, Equal<True>,
							And<Where<ARRegister.closedTranPeriodID, Less<Required<ARRegister.closedTranPeriodID>>, Or<ARRegister.closedTranPeriodID, IsNull>>>>>>>>
							.Select(this, source.DocType, source.RefNbr, minPeriod, minPeriod);

					// do not add already found document
					foreach(PXResult<ARRegister> rec in GetFullDocuments(adjustments))
					{
						ARRegister doc = rec;
						if (!documents.Contains(doc))
						{
							newlyFoundDocsKeys.Add(new { doc.DocType, doc.RefNbr });
							documents.Add(rec);
						}
					}

					var adjds =
						PXSelectJoin<ARRegister,
							LeftJoinSingleTable<ARInvoice,
								On<ARInvoice.docType, Equal<ARRegister.docType>,
								And<ARInvoice.refNbr, Equal<ARRegister.refNbr>>>,
							LeftJoinSingleTable<ARPayment,
								On<ARPayment.docType, Equal<ARRegister.docType>,
								And<ARPayment.refNbr, Equal<ARRegister.refNbr>>>>>,
							Where<ARRegister.customerID, Equal<Current<Customer.bAccountID>>,
							And2<Exists<Select<ARAdjust, 
									Where2<Where<ARAdjust.adjdDocType, Equal<ARRegister.docType>,
								Or<ARAdjust.adjdDocType, Equal<ARDocType.payment>, And<ARRegister.docType, Equal<ARDocType.voidPayment>,
								Or<ARAdjust.adjdDocType, Equal<ARDocType.prepayment>, And<ARRegister.docType, Equal<ARDocType.voidPayment>>>>>>,
									And<ARAdjust.adjdRefNbr, Equal<ARRegister.refNbr>,
							And<ARAdjust.adjgDocType, Equal<Required<ARAdjust.adjgDocType>>,
							And<ARAdjust.adjgRefNbr, Equal<Required<ARAdjust.adjgRefNbr>>,
									And<ARAdjust.released, Equal<True>
									>>>>>>>,
							And<ARRegister.tranPeriodID, Less<Required<ARRegister.tranPeriodID>>,
							And<ARRegister.released, Equal<True>,
							And<Where<ARRegister.closedTranPeriodID, Less<Required<ARRegister.closedTranPeriodID>>, 
									Or<ARRegister.closedTranPeriodID, IsNull>>>>>>>>
							.Select(this, source.DocType, source.RefNbr, minPeriod, minPeriod);

					// do not add already found document
					foreach (PXResult<ARRegister> rec in GetFullDocuments(adjds))
					{
						ARRegister doc = rec;
						if (!documents.Contains(doc))
						{
							newlyFoundDocsKeys.Add(new { doc.DocType, doc.RefNbr });
							documents.Add(rec);
						}
					}
				}
			}
		}

		protected static void Copy(ARSalesPerTran aDest, ARAdjust aAdj) 
		{
			aDest.AdjdDocType = aAdj.AdjdDocType;
			aDest.AdjdRefNbr = aAdj.AdjdRefNbr;
			aDest.AdjNbr = aAdj.AdjNbr;
			aDest.BranchID = aAdj.AdjdBranchID;
			aDest.Released = true;
		}

		protected static void Copy(ARSalesPerTran aDest, ARRegister aReg)
		{
			aDest.DocType = aReg.DocType;
			aDest.RefNbr = aReg.RefNbr;
		}

		protected static void CopyShare(ARSalesPerTran aDest, ARSalesPerTran aSrc, decimal aRatio, short aPrecision) 
		{
			aDest.CuryCommnblAmt = Math.Round((decimal)(aRatio * aSrc.CuryCommnblAmt), aPrecision);
			aDest.CuryCommnAmt = Math.Round((decimal)(aRatio * aSrc.CuryCommnAmt), aPrecision);
		}

		protected ARSalesPerTran CreatePaymentSPT(ARRegister payment, ARAdjust adj, ARSalesPerTran iSPT)
		{
			ARSalesPerTran paySPT = new ARSalesPerTran();

			Copy(paySPT, payment);
			Copy(paySPT, adj);

			paySPT.SalespersonID = iSPT.SalespersonID;
			paySPT.CuryInfoID = iSPT.CuryInfoID; //We will use currency Info of the orifginal invoice for the commission calculations
			paySPT.BaseCuryID = iSPT.BaseCuryID;
			paySPT.CommnPct = iSPT.CommnPct;

			return paySPT;
		}

		public virtual void ClearRetainageAmount(ARRegister childRetainageDoc)
		{
			ARRegister origRetainageDoc = GetOriginalRetainageDocument(childRetainageDoc);

			if (origRetainageDoc != null)
			{
				if (origRetainageDoc.RetainageUnreleasedAmt != origRetainageDoc.CuryRetainageTotal)
					throw new PXException(AP.Messages.ReleasedRetainageDocumentExists, childRetainageDoc.RefNbr);

				childRetainageDoc.CuryRetainageUnreleasedAmt = 0m;
				childRetainageDoc.CuryRetainageReleased = 0m;
				childRetainageDoc.CuryRetainageUnpaidTotal = 0m;
				origRetainageDoc.CuryRetainageUnreleasedAmt = 0m;
				origRetainageDoc.CuryRetainageReleased = 0m;
				origRetainageDoc.CuryRetainageUnpaidTotal = -origRetainageDoc.CuryRetainagePaidTotal;

				using (new DisableFormulaCalculationScope(ARDocument.Cache, typeof(ARRegister.curyRetainageReleased)))
				{
					ARDocument.Update(origRetainageDoc);
				}
			}
		}

		public virtual void  ReleaseRetainageAmount(ARRegister childRetainageDoc)
		{
			IEqualityComparer<ARTran> arTranComparer =
					new FieldSubsetEqualityComparer<ARTran>(
					ARTran_TranType_RefNbr.Cache,
					typeof(ARTran.tranType),
					typeof(ARTran.refNbr),
					typeof(ARTran.lineNbr));

			foreach (var group in ARTran_TranType_RefNbr.Select(childRetainageDoc.DocType, childRetainageDoc.RefNbr).AsEnumerable().GroupBy(row => (ARTran)row, arTranComparer))
			{
				ARTran line = group.Key;
				ARRegister origRetainageDoc = GetOriginalRetainageDocument(line);
				if (origRetainageDoc != null)
				{
					decimal curyTaxAmount = 0m;
					foreach (PXResult<ARTran, ARTax, Tax, DRDeferredCode, SO.SOOrderType, ARTaxTran> r in group)
					{
						ARTax tax = r;
						Tax t = r;
						ARTaxTran taxTran = r;
						if (origRetainageDoc.TaxCalcMode == TaxCalculationMode.Gross || t.TaxCalcLevel == CSTaxCalcLevel.Inclusive && origRetainageDoc.TaxCalcMode != TaxCalculationMode.Net)
							continue;
						curyTaxAmount += (childRetainageDoc.PaymentsByLinesAllowed == true ? tax.CuryTaxAmt : taxTran.CuryTaxAmt) ?? 0m;
					}
					decimal sign = origRetainageDoc.SignAmount * childRetainageDoc.SignAmount ?? 0m;
					decimal releasedRetainage = (line.CuryTranAmt + curyTaxAmount) * sign ?? 0m;

					origRetainageDoc.CuryRetainageUnreleasedAmt -= releasedRetainage;
					origRetainageDoc.CuryRetainageReleased += releasedRetainage;
					if (origRetainageDoc.Released != true)
					{
						origRetainageDoc.CuryRetainageUnpaidTotal = origRetainageDoc.CuryRetainageUnreleasedAmt + origRetainageDoc.CuryRetainageReleased;
					}
					ARDocument.Update(origRetainageDoc);

					if (!_IsIntegrityCheck &&
					origRetainageDoc.PaymentsByLinesAllowed != true &&
					(origRetainageDoc.CuryRetainageUnreleasedAmt < 0m ||
						origRetainageDoc.CuryRetainageUnreleasedAmt > origRetainageDoc.CuryRetainageTotal))
					{
						throw new PXException(AP.Messages.RetainageUnreleasedBalanceNegative);
					}
				}
			}
		}

		public virtual ARRegister GetOriginalRetainageDocument(ARRegister childRetainageDoc)
		{
			return GetOriginalRetainageDocument(childRetainageDoc.OrigDocType, childRetainageDoc.OrigRefNbr);
		}

		public virtual ARRegister GetOriginalRetainageDocument(ARTran childRetainageLine)
		{
			return GetOriginalRetainageDocument(childRetainageLine.OrigDocType, childRetainageLine.OrigRefNbr);
		}

		public virtual ARRegister GetOriginalRetainageDocument(string origDocType, string origRefNbr)
		{
			ARInvoice origRetainageDoc = PXSelect<ARInvoice,
				Where<ARInvoice.docType, Equal<Required<ARInvoice.docType>>,
					And<ARInvoice.refNbr, Equal<Required<ARInvoice.refNbr>>,
					And<ARInvoice.retainageApply, Equal<True>>>>>
				.SelectSingleBound(this, null, origDocType, origRefNbr);
			ARRegister cached = ARDocument.Cache.Locate(origRetainageDoc) as ARRegister;
			if(cached != null && origRetainageDoc != null)
				ARDocument.Cache.RestoreCopy(origRetainageDoc, cached);
			return origRetainageDoc;
		}

		public virtual ARTran GetOriginalRetainageLine(ARRegister childRetainageDoc, ARTran childRetainageTran)
		{
			ARTran origRetainageLine = PXSelect<ARTran,
				Where<ARTran.tranType, Equal<Required<ARTran.tranType>>,
					And<ARTran.refNbr, Equal<Required<ARTran.refNbr>>,
					And<ARTran.lineNbr, Equal<Required<ARTran.lineNbr>>,
					And<ARTran.curyRetainageAmt, NotEqual<decimal0>>>>>>
				.SelectSingleBound(this, null,
					childRetainageDoc.OrigDocType,
					childRetainageDoc.OrigRefNbr,
					childRetainageTran.OrigLineNbr);

			return origRetainageLine;
		}

		//we don't need arregister anymore cause of new fields in ARTran
		public virtual ARTran GetOriginalRetainageLine(ARTran childRetainageTran)
		{
			ARTran origRetainageLine = PXSelect<ARTran,
				Where<ARTran.tranType, Equal<Required<ARTran.tranType>>,
					And<ARTran.refNbr, Equal<Required<ARTran.refNbr>>,
					And<ARTran.lineNbr, Equal<Required<ARTran.lineNbr>>,
					And<ARTran.curyRetainageAmt, NotEqual<decimal0>>>>>>
				.SelectSingleBound(this, null,
					childRetainageTran.OrigDocType,
					childRetainageTran.OrigRefNbr,
					childRetainageTran.OrigLineNbr);

			return origRetainageLine;
		}

		public virtual bool IsFullyProcessedOriginalRetainageDocument(ARRegister origRetainageInvoice)
		{
			bool hasZeroRetainageUnpaidTotal = true;

			// ARRegister class should be used here,
			// otherwise you will get not updated records
			// with incorrect balances.
			//
			foreach (ARRegister childRetainageBill in PXSelect<ARRegister,
				Where<ARRegister.isRetainageDocument, Equal<True>,
					And<ARRegister.origDocType, Equal<Required<ARRegister.docType>>,
					And<ARRegister.origRefNbr, Equal<Required<ARRegister.refNbr>>,
					And<ARRegister.released, Equal<True>>>>>>
				.Select(this, origRetainageInvoice.DocType, origRetainageInvoice.RefNbr))
			{
				if (!childRetainageBill.HasZeroBalance<ARRegister.curyDocBal, ARTran.curyTranBal>(this))
				{
					hasZeroRetainageUnpaidTotal = false;
					break;
				}
			}

			return
				origRetainageInvoice.HasZeroBalance<ARRegister.curyDocBal, ARTran.curyTranBal>(this) &&
				origRetainageInvoice.HasZeroBalance<ARRegister.curyRetainageUnreleasedAmt, ARTran.curyRetainageBal>(this) &&
				hasZeroRetainageUnpaidTotal;
		}

		#region Customizable virtual methods for GL transactions insertion

		public class GLTranInsertionContext
		{
			public virtual ARRegister ARRegisterRecord { get; set; }
			public virtual ARTran ARTranRecord { get; set; }
			public virtual ARTaxTran ARTaxTranRecord { get; set; }

			public virtual ARPaymentChargeTran ARPaymentChargeTranRecord { get; set; }
			public virtual ARAdjust ARAdjustRecord { get; set; }

			public virtual CADeposit CADepositRecord { get; set; }
			public virtual CADepositDetail CADepositDetailRecord { get; set; }

			public virtual INTran INTranRecord { get; set; }
			public virtual IN.INTranCost INTranCostRecord { get; set; }

			public virtual PMTran PMTranRecord { get; set; }
		}

		#region Invoice
		/// <summary>
		/// Posts per-unit tax amounts to document lines' accounts. 
		/// This is an extension point, actual posting is done by graph extension <see cref="ARReleaseProcessPerUnitTaxPoster"/> 
		/// which overrides this method if the festure "Per-unit Tax Support" is turned on.
		/// </summary>
		protected virtual void PostPerUnitTaxAmounts(JournalEntry journalEntry, ARInvoice invoice, CurrencyInfo newCurrencyInfo,
													 ARTaxTran perUnitAggregatedTax, Tax perUnitTax, bool isDebitTaxTran)
		{
		}

		/// <summary>
		/// The method to insert invoice GL transactions 
		/// for the <see cref="ARInvoice"/> entity inside the
		/// <see cref="ReleaseInvoice(JournalEntry, ref ARRegister, PXResult{ARInvoice, CurrencyInfo, Terms, Customer, Account}, out PMRegister)"/> method.
		/// <see cref="GLTranInsertionContext"/> class content:
		/// <see cref="GLTranInsertionContext.ARRegisterRecord"/>.
		/// </summary>
		public virtual GLTran InsertInvoiceTransaction(
			JournalEntry je,
			GLTran tran,
			GLTranInsertionContext context)
		{
			return je.GLTranModuleBatNbr.Insert(tran);
		}

		/// <summary>
		/// The method to insert invoice tax GL transactions 
		/// for the <see cref="ARTaxTran"/> entity inside the
		/// <see cref="ReleaseInvoice(JournalEntry, ref ARRegister, PXResult{ARInvoice, CurrencyInfo, Terms, Customer, Account}, out PMRegister)"/> method.
		/// <see cref="GLTranInsertionContext"/> class content:
		/// <see cref="GLTranInsertionContext.ARRegisterRecord"/>,
		/// <see cref="GLTranInsertionContext.ARTaxTranRecord"/>.
		/// </summary>
		public virtual GLTran InsertInvoiceTaxTransaction(
			JournalEntry je,
			GLTran tran,
			GLTranInsertionContext context)
		{
			return je.GLTranModuleBatNbr.Insert(tran);
		}

		/// <summary>
		/// The method to insert tax expense GL transactions for the <see cref="ARTaxTran"/> entity inside the
		/// <see cref="PerUnitTaxesPostOnRelease.PostPerUnitTaxAmountsToItemAccounts(APInvoice, CurrencyInfo, ARTaxTran, Tax, bool, bool)"/> helper method.
		/// <see cref="GLTranInsertionContext"/> class content:
		/// <see cref="GLTranInsertionContext.ARRegisterRecord"/>,
		/// <see cref="GLTranInsertionContext.ARTranRecord"/>,
		/// <see cref="GLTranInsertionContext.ARTaxTranRecord"/>.
		/// </summary>
		public virtual GLTran InsertInvoicePerUnitTaxAmountsToItemAccountsTransaction(JournalEntry journalEntryGraph, GLTran tran,
																					  GLTranInsertionContext context)
		{
			journalEntryGraph.ThrowOnNull(nameof(journalEntryGraph));
			return journalEntryGraph.GLTranModuleBatNbr.Insert(tran);
		}

		/// <summary>
		/// The method to insert invoice rounding GL transactions 
		/// for the <see cref="ARInvoice"/> entity inside the
		/// <see cref="ReleaseInvoice(JournalEntry, ref ARRegister, PXResult{ARInvoice, CurrencyInfo, Terms, Customer, Account}, out PMRegister)"/> method.
		/// <see cref="GLTranInsertionContext"/> class content:
		/// <see cref="GLTranInsertionContext.ARRegisterRecord"/>.
		/// </summary>
		public virtual GLTran InsertInvoiceRoundingTransaction(
			JournalEntry je,
			GLTran tran,
			GLTranInsertionContext context)
		{
			return je.GLTranModuleBatNbr.Insert(tran);
		}

		/// <summary>
		/// The method to insert invoice details GL transactions 
		/// for the <see cref="ARTran"/> entity inside the
		/// <see cref="ReleaseInvoice(JournalEntry, ref ARRegister, PXResult{ARInvoice, CurrencyInfo, Terms, Customer, Account}, out PMRegister)"/> method.
		/// <see cref="GLTranInsertionContext"/> class content:
		/// <see cref="GLTranInsertionContext.ARRegisterRecord"/>,
		/// <see cref="GLTranInsertionContext.ARTranRecord"/>.
		/// </summary>
		public virtual GLTran InsertInvoiceDetailsTransaction(
			JournalEntry je,
			GLTran tran,
			GLTranInsertionContext context)
		{
			return je.GLTranModuleBatNbr.Insert(tran);
		}

		/// <summary>
		/// The method to insert invoice details schedule GL transactions 
		/// for the <see cref="ARTran"/> entity inside the
		/// <see cref="ReleaseInvoice(JournalEntry, ref ARRegister, PXResult{ARInvoice, CurrencyInfo, Terms, Customer, Account}, out PMRegister)"/> method.
		/// <see cref="GLTranInsertionContext"/> class content:
		/// <see cref="GLTranInsertionContext.ARRegisterRecord"/>,
		/// <see cref="GLTranInsertionContext.ARTranRecord"/>.
		/// </summary>
		public virtual GLTran InsertInvoiceDetailsScheduleTransaction(
			JournalEntry je,
			GLTran tran,
			GLTranInsertionContext context)
		{
			return je.GLTranModuleBatNbr.Insert(tran);
		}

		/// <summary>
		/// The method to insert invoice details INCost GL transactions 
		/// for the <see cref="IN.INTranCost"/> entity inside the
		/// <see cref="ReleaseInvoice(JournalEntry, ref ARRegister, PXResult{ARInvoice, CurrencyInfo, Terms, Customer, Account}, out PMRegister)"/> method.
		/// <see cref="GLTranInsertionContext"/> class content:
		/// <see cref="GLTranInsertionContext.ARRegisterRecord"/>,
		/// <see cref="GLTranInsertionContext.ARTranRecord"/>,
		/// <see cref="GLTranInsertionContext.INTranRecord"/>,
		/// <see cref="GLTranInsertionContext.INTranCostRecord"/>.
		/// </summary>
		public virtual GLTran InsertInvoiceDetailsINTranCostTransaction(
			JournalEntry je,
			GLTran tran,
			GLTranInsertionContext context)
		{
			return je.GLTranModuleBatNbr.Insert(tran);
		}

		#endregion

		#region Payment

		/// <summary>
		/// The method to insert payment CADeposit GL transactions 
		/// for the <see cref="CADeposit"/> entity inside the
		/// <see cref="ReleaseDocProc(JournalEntry, ARRegister, List{Batch}, ARDocumentRelease.ARMassProcessReleaseTransactionScopeDelegate)"/> method.
		/// <see cref="GLTranInsertionContext"/> class content:
		/// <see cref="GLTranInsertionContext.ARRegisterRecord"/>,
		/// <see cref="GLTranInsertionContext.CADepositRecord"/>,
		/// <see cref="GLTranInsertionContext.CADepositDetailRecord"/>.
		/// </summary>
		public virtual GLTran InsertPaymentCADepositTransaction(
			JournalEntry je,
			GLTran tran,
			GLTranInsertionContext context)
		{
			return je.GLTranModuleBatNbr.Insert(tran);
		}

		/// <summary>
		/// The method to insert payment GL transactions 
		/// for the <see cref="ARPayment"/> entity inside the
		/// <see cref="ProcessPayment(JournalEntry, ARRegister, PXResult{ARPayment, CurrencyInfo, CM.Currency, Customer, CashAccount})"/> method.
		/// <see cref="GLTranInsertionContext"/> class content:
		/// <see cref="GLTranInsertionContext.ARRegisterRecord"/>.
		/// </summary>
		public virtual GLTran InsertPaymentTransaction(
			JournalEntry je,
			GLTran tran,
			GLTranInsertionContext context)
		{
			return je.GLTranModuleBatNbr.Insert(tran);
		}

		/// <summary>
		/// The method to insert payment charge GL transactions 
		/// for the <see cref="ARPaymentChargeTran"/> entity inside the
		/// <see cref="ProcessPayment(JournalEntry, ARRegister, PXResult{ARPayment, CurrencyInfo, CM.Currency, Customer, CashAccount})"/> method.
		/// <see cref="GLTranInsertionContext"/> class content:
		/// <see cref="GLTranInsertionContext.ARRegisterRecord"/>,
		/// <see cref="GLTranInsertionContext.ARPaymentChargeTranRecord"/>.
		/// </summary>
		public virtual GLTran InsertPaymentChargeTransaction(
			JournalEntry je,
			GLTran tran,
			GLTranInsertionContext context)
		{
			return je.GLTranModuleBatNbr.Insert(tran);
		}

		/// <summary>
		/// The method to insert adjustments adjusting GL transactions 
		/// for the <see cref="ARAdjust"/> entity inside the
		/// <see cref="ProcessAdjustmentAdjusting(JournalEntry, ARAdjust, ARPayment, ARRegister, Customer, CurrencyInfo)"/> method.
		/// <see cref="GLTranInsertionContext"/> class content:
		/// <see cref="GLTranInsertionContext.ARRegisterRecord"/>,
		/// <see cref="GLTranInsertionContext.ARAdjustRecord"/>.
		/// </summary>
		public virtual GLTran InsertAdjustmentsAdjustingTransaction(
			JournalEntry je,
			GLTran tran,
			GLTranInsertionContext context)
		{
			return je.GLTranModuleBatNbr.Insert(tran);
		}

		/// <summary>
		/// The method to insert adjustments adjusted GL transactions 
		/// for the <see cref="ARAdjust"/> entity inside the
		/// <see cref="ProcessAdjustmentAdjusted(JournalEntry, ARAdjust, ARRegister, ARPayment, CurrencyInfo, CurrencyInfo)"/> method.
		/// <see cref="GLTranInsertionContext"/> class content:
		/// <see cref="GLTranInsertionContext.ARRegisterRecord"/>,
		/// <see cref="GLTranInsertionContext.ARAdjustRecord"/>.
		/// </summary>
		public virtual GLTran InsertAdjustmentsAdjustedTransaction(
			JournalEntry je,
			GLTran tran,
			GLTranInsertionContext context)
		{
			return je.GLTranModuleBatNbr.Insert(tran);
		}

		/// <summary>
		/// The method to insert adjustments GOL GL transactions 
		/// for the <see cref="ARAdjust"/> entity inside the
		/// <see cref="ProcessAdjustmentGOL(JournalEntry, ARAdjust, ARPayment, Customer, ARRegister, CM.Currency, CM.Currency, CurrencyInfo, CurrencyInfo)"/> method.
		/// <see cref="GLTranInsertionContext"/> class content:
		/// <see cref="GLTranInsertionContext.ARRegisterRecord"/>,
		/// <see cref="GLTranInsertionContext.ARAdjustRecord"/>.
		/// </summary>
		public virtual GLTran InsertAdjustmentsGOLTransaction(
			JournalEntry je,
			GLTran tran,
			GLTranInsertionContext context)
		{
			return je.GLTranModuleBatNbr.Insert(tran);
		}

		/// <summary>
		/// The method to insert adjustments cash discount GL transactions 
		/// for the <see cref="ARAdjust"/> entity inside the
		/// <see cref="ProcessAdjustmentCashDiscount(JournalEntry, ARAdjust, ARPayment, Customer, CurrencyInfo, CurrencyInfo)"/> method.
		/// <see cref="GLTranInsertionContext"/> class content:
		/// <see cref="GLTranInsertionContext.ARRegisterRecord"/>,
		/// <see cref="GLTranInsertionContext.ARAdjustRecord"/>.
		/// </summary>
		public virtual GLTran InsertAdjustmentsCashDiscountTransaction(
			JournalEntry je,
			GLTran tran,
			GLTranInsertionContext context)
		{
			return je.GLTranModuleBatNbr.Insert(tran);
		}

		/// <summary>
		/// The method to insert adjustments write off GL transactions 
		/// for the <see cref="ARAdjust"/> entity inside the
		/// <see cref="ProcessAdjustmentWriteOff(JournalEntry, ARAdjust, ARPayment, Customer, CurrencyInfo, CurrencyInfo)"/> method.
		/// <see cref="GLTranInsertionContext"/> class content:
		/// <see cref="GLTranInsertionContext.ARRegisterRecord"/>,
		/// <see cref="GLTranInsertionContext.ARAdjustRecord"/>.
		/// </summary>
		public virtual GLTran InsertAdjustmentsWriteOffTransaction(
			JournalEntry je,
			GLTran tran,
			GLTranInsertionContext context)
		{
			return je.GLTranModuleBatNbr.Insert(tran);
		}

		/// <summary>
		/// The method to insert adjustments rounding GL transactions 
		/// for the <see cref="ARAdjust"/> entity inside the
		/// <see cref="ProcessAdjustmentsRounding(JournalEntry, ARRegister, ARAdjust, ARPayment, Customer, CM.Currency, CurrencyInfo, CurrencyInfo, decimal?, bool?)"/> method.
		/// <see cref="GLTranInsertionContext"/> class content:
		/// <see cref="GLTranInsertionContext.ARRegisterRecord"/>,
		/// <see cref="GLTranInsertionContext.ARAdjustRecord"/>.
		/// </summary>
		public virtual GLTran InsertAdjustmentsRoundingTransaction(
			JournalEntry je,
			GLTran tran,
			GLTranInsertionContext context)
		{
			return je.GLTranModuleBatNbr.Insert(tran);
		}

		#endregion

		#endregion

		public virtual void VerifyInterBranchTransactions(ARRegister doc)
		{
			if (IsMigrationMode == true || PXAccess.FeatureInstalled<FeaturesSet.interBranch>()) return;

			var branch = (Branch)PXSelect<Branch, Where<Branch.branchID, Equal<Required<Branch.branchID>>>>.SelectSingleBound(this, null, doc.BranchID);

			var adjdsToDifferentOrganization = PXSelectJoin<ARAdjust,
				InnerJoin<Branch, On<Branch.branchID, Equal<ARAdjust.adjdBranchID>>>,
				Where<ARAdjust.adjgDocType, Equal<Required<ARAdjust.adjgDocType>>,
					And<ARAdjust.adjgRefNbr, Equal<Required<ARAdjust.adjgRefNbr>>,
					And<ARAdjust.adjgBranchID, NotEqual<ARAdjust.adjdBranchID>,
					And<Branch.organizationID, NotEqual<Required<Branch.organizationID>>>>>>>
				.SelectSingleBound(this, null, doc.DocType, doc.RefNbr, branch.OrganizationID);

			if (adjdsToDifferentOrganization.Any())
			{
				throw new PXException(GL.Messages.InterBranchTransAreNotAllowed);
			}

			var adjgsToDifferentOrganization = PXSelectJoin<ARAdjust,
				InnerJoin<Branch, On<Branch.branchID, Equal<ARAdjust.adjgBranchID>>>,
				Where<ARAdjust.adjdDocType, Equal<Required<ARAdjust.adjdDocType>>,
					And<ARAdjust.adjdRefNbr, Equal<Required<ARAdjust.adjdRefNbr>>,
					And<ARAdjust.adjgBranchID, NotEqual<ARAdjust.adjdBranchID>,
					And<Branch.organizationID, NotEqual<Required<Branch.organizationID>>>>>>>
				.SelectSingleBound(this, null, doc.DocType, doc.RefNbr, branch.OrganizationID);

			if (adjgsToDifferentOrganization.Any())
			{
				throw new PXException(GL.Messages.InterBranchTransAreNotAllowed);
			}
		}
		#region Raise Automation Events
		protected virtual void RaiseInvoiceEvent(ARRegister doc, PX.Data.WorkflowAPI.SelectedEntityEvent<ARInvoice> invEvent)
		{
			if (doc is ARInvoice)
			{
				ARDocument.Cache.Remove(doc);
				invEvent.FireOn(this, (ARInvoice)doc);
				ARDocument.Cache.Update(doc);
				ARDocument.Cache.RestoreCopy(doc, ARDocument.Cache.Locate(doc));
			}
		}
		protected virtual void RaisePaymentEvent(ARRegister doc, PX.Data.WorkflowAPI.SelectedEntityEvent<ARPayment> pntEvent)
		{
			if (doc is ARPayment)
			{
				ARDocument.Cache.Remove(doc);
				pntEvent.FireOn(this, (ARPayment)doc);
				ARDocument.Cache.Update(doc);
				ARDocument.Cache.RestoreCopy(doc, ARDocument.Cache.Locate(doc));
			}
		}

		protected virtual void RaiseReleaseEvent(ARRegister doc)
		{
			if (ARDocument.Cache.ObjectsEqual(doc, ARInvoice_DocType_RefNbr.Current))
			{
				StoreOriginalAsNotReleased(ARInvoice_DocType_RefNbr.Current);
				ARInvoice invoice = PXCache<ARInvoice>.CreateCopy(ARInvoice_DocType_RefNbr.Current);
				ARDocument.Cache.RestoreCopy(invoice, doc);
				ARDocument.Cache.Remove(doc);
				ARInvoice.Events
					.Select(e => e.ReleaseDocument)
					.FireOn(this, invoice);
				if (ARDocument.Cache.GetStatus(invoice) != PXEntryStatus.Updated)
					ARDocument.Cache.SetStatus(invoice, PXEntryStatus.Updated);
				ARDocument.Cache.RestoreCopy(doc, ARDocument.Cache.Locate(doc));
			}
			else if (ARDocument.Cache.ObjectsEqual(doc, ARPayment_DocType_RefNbr.Current))
			{
				StoreOriginalAsNotReleased(ARPayment_DocType_RefNbr.Current);
				ARPayment payment = PXCache<ARPayment>.CreateCopy(ARPayment_DocType_RefNbr.Current);
				ARDocument.Cache.RestoreCopy(payment, doc);
				ARDocument.Cache.Remove(doc);
				ARPayment.Events
					.Select(e => e.ReleaseDocument)
					.FireOn(this, payment);
				if (ARDocument.Cache.GetStatus(payment) != PXEntryStatus.Updated)
					ARDocument.Cache.SetStatus(payment, PXEntryStatus.Updated);

				ARDocument.Cache.RestoreCopy(doc, ARDocument.Cache.Locate(doc));
			}
		}

		private void StoreOriginalAsNotReleased(ARRegister doc)
		{
			ARRegister toStore = PXCache<ARRegister>.CreateCopy(doc);
			toStore.Released = false;
			PXCache<ARRegister>.StoreOriginal(this, toStore);
		}
		#endregion
	}
}

namespace PX.Objects.AR.Overrides.ARDocumentRelease
{
	public interface IBaseARHist
	{
		Boolean? DetDeleted
		{
			get;
			set;
		}

		Boolean? FinFlag
		{
			get;
			set;
		}
		Decimal? PtdCrAdjustments
		{
			get;
			set;
		}
		Decimal? PtdDrAdjustments
		{
			get;
			set;
		}
		Decimal? PtdSales
		{
			get;
			set;
		}
		Decimal? PtdPayments
		{
			get;
			set;
		}
		Decimal? PtdDiscounts
		{
			get;
			set;
		}
		Decimal? YtdBalance
		{
			get;
			set;
		}
		Decimal? BegBalance
		{
			get;
			set;
		}
		Decimal? PtdCOGS
		{
			get;
			set;
		}
		Decimal? PtdRGOL
		{
			get;
			set;
		}
		Decimal? PtdFinCharges
		{
			get;
			set;
		}
		Decimal? PtdDeposits
		{
			get;
			set;
		}
		Decimal? YtdDeposits
		{
			get;
			set;
		}
		Decimal? PtdItemDiscounts
		{
			get;
			set;
		}
		Decimal? YtdRetainageReleased
		{
			get;
			set;
		}
		Decimal? PtdRetainageReleased
		{
			get;
			set;
		}
		Decimal? YtdRetainageWithheld
		{
			get;
			set;
		}
		Decimal? PtdRetainageWithheld
		{
			get;
			set;
		}
	}

	public interface ICuryARHist
	{
		Decimal? CuryPtdCrAdjustments
		{
			get;
			set;
		}
		Decimal? CuryPtdDrAdjustments
		{
			get;
			set;
		}
		Decimal? CuryPtdSales
		{
			get;
			set;
		}
		Decimal? CuryPtdPayments
		{
			get;
			set;
		}
		Decimal? CuryPtdDiscounts
		{
			get;
			set;
		}
		Decimal? CuryPtdFinCharges
		{
			get;
			set;
		}
		Decimal? CuryYtdBalance
		{
			get;
			set;
		}
		Decimal? CuryBegBalance
		{
			get;
			set;
		}
		Decimal? CuryPtdDeposits
		{
			get;
			set;
		}
		Decimal? CuryYtdDeposits
		{
			get;
			set;
		}
		Decimal? CuryYtdRetainageReleased
		{
			get;
			set;
		}
		Decimal? CuryPtdRetainageReleased
		{
			get;
			set;
		}
		Decimal? CuryYtdRetainageWithheld
		{
			get;
			set;
		}
		Decimal? CuryPtdRetainageWithheld
		{
			get;
			set;
		}
	}

	[PXAccumulator(new Type[] {
				typeof(CuryARHistory.finYtdBalance),
				typeof(CuryARHistory.tranYtdBalance),
				typeof(CuryARHistory.curyFinYtdBalance),
				typeof(CuryARHistory.curyTranYtdBalance),
				typeof(CuryARHistory.finYtdBalance),
				typeof(CuryARHistory.tranYtdBalance),
				typeof(CuryARHistory.curyFinYtdBalance),
				typeof(CuryARHistory.curyTranYtdBalance),
				typeof(CuryARHistory.finYtdDeposits),
				typeof(CuryARHistory.tranYtdDeposits),
				typeof(CuryARHistory.curyFinYtdDeposits),
				typeof(CuryARHistory.curyTranYtdDeposits),
				typeof(CuryARHistory.finYtdRetainageReleased),
				typeof(CuryARHistory.tranYtdRetainageReleased),
				typeof(CuryARHistory.finYtdRetainageWithheld),
				typeof(CuryARHistory.tranYtdRetainageWithheld),
				typeof(CuryARHistory.curyFinYtdRetainageReleased),
				typeof(CuryARHistory.curyTranYtdRetainageReleased),
				typeof(CuryARHistory.curyFinYtdRetainageWithheld),
				typeof(CuryARHistory.curyTranYtdRetainageWithheld)
				},
					new Type[] {
				typeof(CuryARHistory.finBegBalance),
				typeof(CuryARHistory.tranBegBalance),
				typeof(CuryARHistory.curyFinBegBalance),
				typeof(CuryARHistory.curyTranBegBalance),
				typeof(CuryARHistory.finYtdBalance),
				typeof(CuryARHistory.tranYtdBalance),
				typeof(CuryARHistory.curyFinYtdBalance),
				typeof(CuryARHistory.curyTranYtdBalance),
				typeof(CuryARHistory.finYtdDeposits),
				typeof(CuryARHistory.tranYtdDeposits),
				typeof(CuryARHistory.curyFinYtdDeposits),
				typeof(CuryARHistory.curyTranYtdDeposits),
				typeof(CuryARHistory.finYtdRetainageReleased),
				typeof(CuryARHistory.tranYtdRetainageReleased),
				typeof(CuryARHistory.finYtdRetainageWithheld),
				typeof(CuryARHistory.tranYtdRetainageWithheld),
				typeof(CuryARHistory.curyFinYtdRetainageReleased),
				typeof(CuryARHistory.curyTranYtdRetainageReleased),
				typeof(CuryARHistory.curyFinYtdRetainageWithheld),
				typeof(CuryARHistory.curyTranYtdRetainageWithheld)
				}
			)]
	[Serializable]
	[PXHidden]
	public partial class CuryARHist : CuryARHistory, ICuryARHist, IBaseARHist
	{
		#region BranchID
		public new abstract class branchID : PX.Data.BQL.BqlInt.Field<branchID> { }
		[PXDBInt(IsKey = true)]
		public override Int32? BranchID
		{
			get
			{
				return this._BranchID;
			}
			set
			{
				this._BranchID = value;
			}
		}
		#endregion
		#region AccountID
		public new abstract class accountID : PX.Data.BQL.BqlInt.Field<accountID> { }
		[PXDBInt(IsKey = true)]
		[PXDefault()]
		public override Int32? AccountID
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
		public new abstract class subID : PX.Data.BQL.BqlInt.Field<subID> { }
		[PXDBInt(IsKey = true)]
		[PXDefault()]
		public override Int32? SubID
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
		#region CustomerID
		public new abstract class customerID : PX.Data.BQL.BqlInt.Field<customerID> { }
		[PXDBInt(IsKey = true)]
		[PXDefault()]
		public override Int32? CustomerID
		{
			get
			{
				return this._CustomerID;
			}
			set
			{
				this._CustomerID = value;
			}
		}
		#endregion
		#region CuryID
		public new abstract class curyID : PX.Data.BQL.BqlString.Field<curyID> { }
		[PXDBString(5, IsUnicode = true, IsKey = true, InputMask = ">LLLLL")]
		[PXDefault()]
		public override String CuryID
		{
			get
			{
				return this._CuryID;
			}
			set
			{
				this._CuryID = value;
			}
		}
		#endregion
		#region FinPeriodID
		public new abstract class finPeriodID : PX.Data.BQL.BqlString.Field<finPeriodID> { }
		[PXDBString(6, IsKey = true, IsFixed = true)]
		[PXDefault()]
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
	}

	[PXAccumulator(new Type[] {
				typeof(ARHistory.finYtdBalance),
				typeof(ARHistory.tranYtdBalance),
				typeof(ARHistory.finYtdBalance),
				typeof(ARHistory.tranYtdBalance),
				typeof(ARHistory.finYtdDeposits),
				typeof(ARHistory.tranYtdDeposits),
				typeof(ARHistory.finYtdRetainageReleased),
				typeof(ARHistory.tranYtdRetainageReleased),
				typeof(ARHistory.finYtdRetainageWithheld),
				typeof(ARHistory.tranYtdRetainageWithheld)
				},
					new Type[] {
				typeof(ARHistory.finBegBalance),
				typeof(ARHistory.tranBegBalance),
				typeof(ARHistory.finYtdBalance),
				typeof(ARHistory.tranYtdBalance),
				typeof(ARHistory.finYtdDeposits),
				typeof(ARHistory.tranYtdDeposits),
				typeof(ARHistory.finYtdRetainageReleased),
				typeof(ARHistory.tranYtdRetainageReleased),
				typeof(ARHistory.finYtdRetainageWithheld),
				typeof(ARHistory.tranYtdRetainageWithheld)
				}
			)]
	[Serializable]
	[PXHidden]
	public partial class ARHist : ARHistory, IBaseARHist
	{
		#region BranchID
		public new abstract class branchID : PX.Data.BQL.BqlInt.Field<branchID> { }
		[PXDBInt(IsKey = true)]
		public override int? BranchID
		{
			get;
			set;
		}
		#endregion
		#region AccountID
		public new abstract class accountID : PX.Data.BQL.BqlInt.Field<accountID> { }
		[PXDBInt(IsKey = true)]
		[PXDefault]
		public override int? AccountID
		{
			get;
			set;
		}
		#endregion
		#region SubID
		public new abstract class subID : PX.Data.BQL.BqlInt.Field<subID> { }
		[PXDBInt(IsKey = true)]
		[PXDefault]
		public override int? SubID
		{
			get;
			set;
		}
		#endregion
		#region CustomerID
		public new abstract class customerID : PX.Data.BQL.BqlInt.Field<customerID> { }
		[PXDBInt(IsKey = true)]
		[PXDefault]
		public override int? CustomerID
		{
			get;
			set;
		}
		#endregion
		#region FinPeriodID
		public new abstract class finPeriodID : PX.Data.BQL.BqlString.Field<finPeriodID> { }
		[PXDBString(6, IsKey = true, IsFixed = true)]
		[PXDefault]
		public override string FinPeriodID
		{
			get;
			set;
		}
		#endregion
	}
	public class ARHistory2 : ARHistory
	{
		#region BranchID
		public new abstract class branchID : PX.Data.BQL.BqlInt.Field<branchID> { }
		#endregion
		#region AccountID
		public new abstract class accountID : PX.Data.BQL.BqlInt.Field<accountID> { }
		#endregion
		#region SubID
		public new abstract class subID : PX.Data.BQL.BqlInt.Field<subID> { }
		#endregion
		#region CustomerID
		public new abstract class customerID : PX.Data.BQL.BqlInt.Field<customerID> { }
		#endregion
		#region FinBegBalance
		public new abstract class finBegBalance : PX.Data.BQL.BqlDecimal.Field<finBegBalance> { }
		#endregion
		#region FinPtdSales
		public new abstract class finPtdSales : PX.Data.BQL.BqlDecimal.Field<finPtdSales> { }
		#endregion
		#region FinPtdPayments
		public new abstract class finPtdPayments : PX.Data.BQL.BqlDecimal.Field<finPtdPayments> { }
		#endregion
		#region FinPtdDrAdjustments
		public new abstract class finPtdDrAdjustments : PX.Data.BQL.BqlDecimal.Field<finPtdDrAdjustments> { }
		#endregion
		#region FinPtdCrAdjustments
		public new abstract class finPtdCrAdjustments : PX.Data.BQL.BqlDecimal.Field<finPtdCrAdjustments> { }
		#endregion
		#region FinPtdDiscounts
		public new abstract class finPtdDiscounts : PX.Data.BQL.BqlDecimal.Field<finPtdDiscounts> { }
		#endregion
		#region FinPtdCOGS
		public new abstract class finPtdCOGS : PX.Data.BQL.BqlDecimal.Field<finPtdCOGS> { }
		#endregion
		#region FinPtdRGOL
		public new abstract class finPtdRGOL : PX.Data.BQL.BqlDecimal.Field<finPtdRGOL> { }
		#endregion
		#region FinPtdFinCharges
		public new abstract class finPtdFinCharges : PX.Data.BQL.BqlDecimal.Field<finPtdFinCharges> { }
		#endregion
		#region FinYtdBalance
		public new abstract class finYtdBalance : PX.Data.BQL.BqlDecimal.Field<finYtdBalance> { }
		#endregion
		#region FinPtdDeposits
		public new abstract class finPtdDeposits : PX.Data.BQL.BqlDecimal.Field<finPtdDeposits> { }
		#endregion
		#region FinYtdDeposits
		public new abstract class finYtdDeposits : PX.Data.BQL.BqlDecimal.Field<finYtdDeposits> { }
		#endregion
		#region FinPtdItemDiscounts
		public new abstract class finPtdItemDiscounts : PX.Data.BQL.BqlDecimal.Field<finPtdItemDiscounts> { }
		#endregion
		#region TranBegBalance
		public new abstract class tranBegBalance : PX.Data.BQL.BqlDecimal.Field<tranBegBalance> { }
		#endregion
		#region TranPtdSales
		public new abstract class tranPtdSales : PX.Data.BQL.BqlDecimal.Field<tranPtdSales> { }
		#endregion
		#region TranPtdPayments
		public new abstract class tranPtdPayments : PX.Data.BQL.BqlDecimal.Field<tranPtdPayments> { }
		#endregion
		#region TranPtdDrAdjustments
		public new abstract class tranPtdDrAdjustments : PX.Data.BQL.BqlDecimal.Field<tranPtdDrAdjustments> { }
		#endregion
		#region TranPtdCrAdjustments
		public new abstract class tranPtdCrAdjustments : PX.Data.BQL.BqlDecimal.Field<tranPtdCrAdjustments> { }
		#endregion
		#region TranPtdDiscounts
		public new abstract class tranPtdDiscounts : PX.Data.BQL.BqlDecimal.Field<tranPtdDiscounts> { }
		#endregion
		#region TranPtdRGOL
		public new abstract class tranPtdRGOL : PX.Data.BQL.BqlDecimal.Field<tranPtdRGOL> { }
		#endregion
		#region TranPtdCOGS
		public new abstract class tranPtdCOGS : PX.Data.BQL.BqlDecimal.Field<tranPtdCOGS> { }
		#endregion
		#region TranPtdFinCharges
		public new abstract class tranPtdFinCharges : PX.Data.BQL.BqlDecimal.Field<tranPtdFinCharges> { }
		#endregion
		#region TranYtdBalance
		public new abstract class tranYtdBalance : PX.Data.BQL.BqlDecimal.Field<tranYtdBalance> { }
		#endregion
		#region TranPtdDeposits
		public new abstract class tranPtdDeposits : PX.Data.BQL.BqlDecimal.Field<tranPtdDeposits> { }
		#endregion
		#region TranYtdDeposits
		public new abstract class tranYtdDeposits : PX.Data.BQL.BqlDecimal.Field<tranYtdDeposits> { }
		#endregion
		#region TranPtdItemDiscounts
		public new abstract class tranPtdItemDiscounts : PX.Data.BQL.BqlDecimal.Field<tranPtdItemDiscounts> { }
		#endregion
		#region FinPtdRetainageWithheld
		public new abstract class finPtdRetainageWithheld : PX.Data.BQL.BqlDecimal.Field<finPtdRetainageWithheld> { }
		#endregion
		#region TranPtdRetainageWithheld
		public new abstract class tranPtdRetainageWithheld : PX.Data.BQL.BqlDecimal.Field<tranPtdRetainageWithheld> { }
		#endregion
		#region FinYtdRetainageWithheld
		public new abstract class finYtdRetainageWithheld : PX.Data.BQL.BqlDecimal.Field<finYtdRetainageWithheld> { }
		#endregion
		#region TranYtdRetainageWithheld
		public new abstract class tranYtdRetainageWithheld : PX.Data.BQL.BqlDecimal.Field<tranYtdRetainageWithheld> { }
		#endregion
		#region FinPtdRetainageReleased
		public new abstract class finPtdRetainageReleased : PX.Data.BQL.BqlDecimal.Field<finPtdRetainageReleased> { }
		#endregion
		#region TranPtdRetainageReleased
		public new abstract class tranPtdRetainageReleased : PX.Data.BQL.BqlDecimal.Field<tranPtdRetainageReleased> { }
		#endregion
		#region FinYtdRetainageReleased
		public new abstract class finYtdRetainageReleased : PX.Data.BQL.BqlDecimal.Field<finYtdRetainageReleased> { }
		#endregion
		#region TranYtdRetainageReleased
		public new abstract class tranYtdRetainageReleased : PX.Data.BQL.BqlDecimal.Field<tranYtdRetainageReleased> { }
		#endregion
		#region NumberInvoicePaid
		public new abstract class numberInvoicePaid : PX.Data.BQL.BqlInt.Field<numberInvoicePaid> { }
		#endregion
		#region PaidInvoiceDays
		public new abstract class paidInvoiceDays : IBqlField { }
		#endregion
		#region DetDeleted
		public new abstract class detDeleted : PX.Data.BQL.BqlBool.Field<detDeleted> { }
		#endregion
		#region FinPeriodID
		public new abstract class finPeriodID : PX.Data.BQL.BqlString.Field<finPeriodID> { }
		#endregion
		#region FinPtdRevalued
		public new abstract class finPtdRevalued : PX.Data.BQL.BqlDecimal.Field<finPtdRevalued> { }
		#endregion
	}

	public class CuryARHistory2 : CuryARHistory
	{
		#region BranchID
		public new abstract class branchID : PX.Data.BQL.BqlInt.Field<branchID> { }
		#endregion
		#region AccountID
		public new abstract class accountID : PX.Data.BQL.BqlInt.Field<accountID> { }
		#endregion
		#region SubID
		public new abstract class subID : PX.Data.BQL.BqlInt.Field<subID> { }
		#endregion
		#region CuryID
		public new abstract class curyID : IBqlField { }
		#endregion
		#region CustomerID
		public new abstract class customerID : PX.Data.BQL.BqlInt.Field<customerID> { }
		#endregion
		#region FinBegBalance
		public new abstract class finBegBalance : PX.Data.BQL.BqlDecimal.Field<finBegBalance> { }
		#endregion
		#region FinPtdSales
		public new abstract class finPtdSales : PX.Data.BQL.BqlDecimal.Field<finPtdSales> { }
		#endregion
		#region FinPtdPayments
		public new abstract class finPtdPayments : PX.Data.BQL.BqlDecimal.Field<finPtdPayments> { }
		#endregion
		#region FinPtdDrAdjustments
		public new abstract class finPtdDrAdjustments : PX.Data.BQL.BqlDecimal.Field<finPtdDrAdjustments> { }
		#endregion
		#region FinPtdCrAdjustments
		public new abstract class finPtdCrAdjustments : PX.Data.BQL.BqlDecimal.Field<finPtdCrAdjustments> { }
		#endregion
		#region FinPtdDiscounts
		public new abstract class finPtdDiscounts : PX.Data.BQL.BqlDecimal.Field<finPtdDiscounts> { }
		#endregion
		#region FinPtdCOGS
		public new abstract class finPtdCOGS : PX.Data.BQL.BqlDecimal.Field<finPtdCOGS> { }
		#endregion
		#region FinPtdRGOL
		public new abstract class finPtdRGOL : PX.Data.BQL.BqlDecimal.Field<finPtdRGOL> { }
		#endregion
		#region FinPtdFinCharges
		public new abstract class finPtdFinCharges : PX.Data.BQL.BqlDecimal.Field<finPtdFinCharges> { }
		#endregion
		#region FinYtdBalance
		public new abstract class finYtdBalance : PX.Data.BQL.BqlDecimal.Field<finYtdBalance> { }
		#endregion
		#region FinPtdDeposits
		public new abstract class finPtdDeposits : PX.Data.BQL.BqlDecimal.Field<finPtdDeposits> { }
		#endregion
		#region FinYtdDeposits
		public new abstract class finYtdDeposits : PX.Data.BQL.BqlDecimal.Field<finYtdDeposits> { }
		#endregion
		#region TranBegBalance
		public new abstract class tranBegBalance : PX.Data.BQL.BqlDecimal.Field<tranBegBalance> { }
		#endregion
		#region TranPtdSales
		public new abstract class tranPtdSales : PX.Data.BQL.BqlDecimal.Field<tranPtdSales> { }
		#endregion
		#region TranPtdPayments
		public new abstract class tranPtdPayments : PX.Data.BQL.BqlDecimal.Field<tranPtdPayments> { }
		#endregion
		#region TranPtdDrAdjustments
		public new abstract class tranPtdDrAdjustments : PX.Data.BQL.BqlDecimal.Field<tranPtdDrAdjustments> { }
		#endregion
		#region TranPtdCrAdjustments
		public new abstract class tranPtdCrAdjustments : PX.Data.BQL.BqlDecimal.Field<tranPtdCrAdjustments> { }
		#endregion
		#region TranPtdDiscounts
		public new abstract class tranPtdDiscounts : PX.Data.BQL.BqlDecimal.Field<tranPtdDiscounts> { }
		#endregion
		#region TranPtdRGOL
		public new abstract class tranPtdRGOL : PX.Data.BQL.BqlDecimal.Field<tranPtdRGOL> { }
		#endregion
		#region TranPtdCOGS
		public new abstract class tranPtdCOGS : PX.Data.BQL.BqlDecimal.Field<tranPtdCOGS> { }
		#endregion
		#region TranPtdFinCharges
		public new abstract class tranPtdFinCharges : PX.Data.BQL.BqlDecimal.Field<tranPtdFinCharges> { }
		#endregion
		#region TranYtdBalance
		public new abstract class tranYtdBalance : PX.Data.BQL.BqlDecimal.Field<tranYtdBalance> { }
		#endregion
		#region TranPtdDeposits
		public new abstract class tranPtdDeposits : PX.Data.BQL.BqlDecimal.Field<tranPtdDeposits> { }
		#endregion
		#region TranYtdDeposits
		public new abstract class tranYtdDeposits : PX.Data.BQL.BqlDecimal.Field<tranYtdDeposits> { }
		#endregion
		#region CuryFinBegBalance
		public new abstract class curyFinBegBalance : PX.Data.BQL.BqlDecimal.Field<curyFinBegBalance> { }
		#endregion
		#region CuryFinPtdSales
		public new abstract class curyFinPtdSales : PX.Data.BQL.BqlDecimal.Field<curyFinPtdSales> { }
		#endregion
		#region CuryFinPtdPayments
		public new abstract class curyFinPtdPayments : PX.Data.BQL.BqlDecimal.Field<curyFinPtdPayments> { }
		#endregion
		#region CuryFinPtdDrAdjustments
		public new abstract class curyFinPtdDrAdjustments : PX.Data.BQL.BqlDecimal.Field<curyFinPtdDrAdjustments> { }
		#endregion
		#region CuryFinPtdCrAdjustments
		public new abstract class curyFinPtdCrAdjustments : PX.Data.BQL.BqlDecimal.Field<curyFinPtdCrAdjustments> { }
		#endregion
		#region CuryFinPtdDiscounts
		public new abstract class curyFinPtdDiscounts : PX.Data.BQL.BqlDecimal.Field<curyFinPtdDiscounts> { }
		#endregion
		#region CuryFinPtdFinCharges
		public new abstract class curyFinPtdFinCharges : PX.Data.BQL.BqlDecimal.Field<curyFinPtdFinCharges> { }
		#endregion
		#region CuryFinYtdBalance
		public new abstract class curyFinYtdBalance : PX.Data.BQL.BqlDecimal.Field<curyFinYtdBalance> { }
		#endregion
		#region CuryFinPtdDeposits
		public new abstract class curyFinPtdDeposits : PX.Data.BQL.BqlDecimal.Field<curyFinPtdDeposits> { }
		#endregion
		#region CuryFinYtdDeposits
		public new abstract class curyFinYtdDeposits : PX.Data.BQL.BqlDecimal.Field<curyFinYtdDeposits> { }
		#endregion
		#region CuryTranBegBalance
		public new abstract class curyTranBegBalance : PX.Data.BQL.BqlDecimal.Field<curyTranBegBalance> { }
		#endregion
		#region CuryTranPtdSales
		public new abstract class curyTranPtdSales : PX.Data.BQL.BqlDecimal.Field<curyTranPtdSales> { }
		#endregion
		#region CuryTranPtdPayments
		public new abstract class curyTranPtdPayments : PX.Data.BQL.BqlDecimal.Field<curyTranPtdPayments> { }
		#endregion
		#region CuryTranPtdDrAdjustments
		public new abstract class curyTranPtdDrAdjustments : PX.Data.BQL.BqlDecimal.Field<curyTranPtdDrAdjustments> { }
		#endregion
		#region CuryTranPtdCrAdjustments
		public new abstract class curyTranPtdCrAdjustments : PX.Data.BQL.BqlDecimal.Field<curyTranPtdCrAdjustments> { }
		#endregion
		#region CuryTranPtdDiscounts
		public new abstract class curyTranPtdDiscounts : PX.Data.BQL.BqlDecimal.Field<curyTranPtdDiscounts> { }
		#endregion
		#region CuryTranPtdFinCharges
		public new abstract class curyTranPtdFinCharges : PX.Data.BQL.BqlDecimal.Field<curyTranPtdFinCharges> { }
		#endregion
		#region CuryTranYtdBalance
		public new abstract class curyTranYtdBalance : PX.Data.BQL.BqlDecimal.Field<curyTranYtdBalance> { }
		#endregion
		#region CuryTranPtdDeposits
		public new abstract class curyTranPtdDeposits : PX.Data.BQL.BqlDecimal.Field<curyTranPtdDeposits> { }
		#endregion
		#region CuryTranYtdDeposits
		public new abstract class curyTranYtdDeposits : PX.Data.BQL.BqlDecimal.Field<curyTranYtdDeposits> { }
		#endregion
		#region tstamp
		public new abstract class tstamp : IBqlField { }
		#endregion
		#region DetDeleted
		public new abstract class detDeleted : PX.Data.BQL.BqlBool.Field<detDeleted> { }
		#endregion
		#region FinPeriodID
		public new abstract class finPeriodID : PX.Data.BQL.BqlString.Field<finPeriodID> { }
		#endregion
		#region FinPtdRevalued
		public new abstract class finPtdRevalued : PX.Data.BQL.BqlDecimal.Field<finPtdRevalued> { }
		#endregion
		#region CuryFinPtdRetainageWithheld
		public new abstract class curyFinPtdRetainageWithheld : PX.Data.BQL.BqlDecimal.Field<curyFinPtdRetainageWithheld> { }
		#endregion
		#region FinPtdRetainageWithheld
		public new abstract class finPtdRetainageWithheld : PX.Data.BQL.BqlDecimal.Field<finPtdRetainageWithheld> { }
		#endregion
		#region CuryTranPtdRetainageWithheld
		public new abstract class curyTranPtdRetainageWithheld : PX.Data.BQL.BqlDecimal.Field<curyTranPtdRetainageWithheld> { }
		#endregion
		#region TranPtdRetainageWithheld
		public new abstract class tranPtdRetainageWithheld : PX.Data.BQL.BqlDecimal.Field<tranPtdRetainageWithheld> { }
		#endregion
		#region CuryFinYtdRetainageWithheld
		public new abstract class curyFinYtdRetainageWithheld : PX.Data.BQL.BqlDecimal.Field<curyFinYtdRetainageWithheld> { }
		#endregion
		#region FinYtdRetainageWithheld
		public new abstract class finYtdRetainageWithheld : PX.Data.BQL.BqlDecimal.Field<finYtdRetainageWithheld> { }
		#endregion
		#region CuryTranYtdRetainageWithheld
		public new abstract class curyTranYtdRetainageWithheld : PX.Data.BQL.BqlDecimal.Field<curyTranYtdRetainageWithheld> { }
		#endregion
		#region TranYtdRetainageWithheld
		public new abstract class tranYtdRetainageWithheld : PX.Data.BQL.BqlDecimal.Field<tranYtdRetainageWithheld> { }
		#endregion
		#region CuryFinPtdRetainageReleased
		public new abstract class curyFinPtdRetainageReleased : PX.Data.BQL.BqlDecimal.Field<curyFinPtdRetainageReleased> { }
		#endregion
		#region FinPtdRetainageReleased
		public new abstract class finPtdRetainageReleased : PX.Data.BQL.BqlDecimal.Field<finPtdRetainageReleased> { }
		#endregion
		#region CuryTranPtdRetainageReleased
		public new abstract class curyTranPtdRetainageReleased : PX.Data.BQL.BqlDecimal.Field<curyTranPtdRetainageReleased> { }
		#endregion
		#region TranPtdRetainageReleased
		public new abstract class tranPtdRetainageReleased : PX.Data.BQL.BqlDecimal.Field<tranPtdRetainageReleased> { }
		#endregion
		#region CuryFinYtdRetainageReleased
		public new abstract class curyFinYtdRetainageReleased : PX.Data.BQL.BqlDecimal.Field<curyFinYtdRetainageReleased> { }
		#endregion
		#region FinYtdRetainageReleased
		public new abstract class finYtdRetainageReleased : PX.Data.BQL.BqlDecimal.Field<finYtdRetainageReleased> { }
		#endregion
		#region CuryTranYtdRetainageReleased
		public new abstract class curyTranYtdRetainageReleased : PX.Data.BQL.BqlDecimal.Field<curyTranYtdRetainageReleased> { }
		#endregion
		#region TranYtdRetainageReleased
		public new abstract class tranYtdRetainageReleased : PX.Data.BQL.BqlDecimal.Field<tranYtdRetainageReleased> { }
		#endregion
	}

	public class ARBalAccumAttribute : PXAccumulatorAttribute
	{
		public ARBalAccumAttribute()
		{
			base._SingleRecord = true;
			PersistOrder = PersistOrder.Regular;
		}
		protected override bool PrepareInsert(PXCache sender, object row, PXAccumulatorCollection columns)
		{
			if (!base.PrepareInsert(sender, row, columns))
			{
				return false;
			}

			ARBalances bal = (ARBalances)row;

			columns.Update<ARBalances.lastInvoiceDate>(bal.LastInvoiceDate, PXDataFieldAssign.AssignBehavior.Maximize);
			columns.Update<ARBalances.numberInvoicePaid>(bal.NumberInvoicePaid, PXDataFieldAssign.AssignBehavior.Summarize);
			columns.Update<ARBalances.paidInvoiceDays>(bal.PaidInvoiceDays, PXDataFieldAssign.AssignBehavior.Summarize);
			columns.Update<ARBalances.lastModifiedByID>(bal.LastModifiedByID, PXDataFieldAssign.AssignBehavior.Replace);
			columns.Update<ARBalances.lastModifiedByScreenID>(bal.LastModifiedByScreenID, PXDataFieldAssign.AssignBehavior.Replace);
			columns.Update<ARBalances.lastModifiedDateTime>(bal.LastModifiedDateTime, PXDataFieldAssign.AssignBehavior.Replace);

			// Update LastDocDate to Max(LastDocDate) only when ARRegister.Released == true
			if (bal.LastDocDate != null)
			{
				columns.Update<ARBalances.lastDocDate>(bal.LastDocDate, PXDataFieldAssign.AssignBehavior.Maximize);
			}
			// Update StatementRequired to True only when ARRegister.Released == true
			if (bal.StatementRequired == true)
			{
				columns.Update<ARBalances.statementRequired>(bal.StatementRequired, PXDataFieldAssign.AssignBehavior.Replace);
			}

			return bal.LastInvoiceDate != null
				|| bal.NumberInvoicePaid != null
				|| bal.PaidInvoiceDays != null
				|| bal.CuryID != null
				|| bal.CurrentBal != 0m
				|| bal.UnreleasedBal != 0m
				|| bal.TotalOpenOrders != 0
				|| bal.TotalPrepayments != 0
				|| bal.TotalQuotations != 0
				|| bal.TotalShipped != 0
				|| bal.LastDocDate != null
				|| bal.StatementRequired == true;
		}
	}

	[PXHidden]
	[PXProjection(typeof(Select<ARHistory, Where<ARHistory.detDeleted, Equal<True>>>))]
	public class ARHistoryDetDeleted : IBqlTable
	{
		public abstract class finPeriodID : PX.Data.BQL.BqlString.Field<finPeriodID> { }
		[FinPeriodID(IsKey = false, BqlField = typeof(ARHistory.finPeriodID))]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual string FinPeriodID { get; set; }
		public abstract class customerID : PX.Data.BQL.BqlInt.Field<customerID> { }
		[Customer(IsKey = false, BqlField = typeof(ARHistory.customerID))]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual int? CustomerID { get; set; }
	}
}
