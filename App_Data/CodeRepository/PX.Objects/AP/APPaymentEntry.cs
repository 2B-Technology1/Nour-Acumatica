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
using PX.Data.Description;
using PX.Data.WorkflowAPI;
using PX.Objects.AR;
using PX.Objects.CA;
using PX.Objects.CM.Extensions;
using PX.Objects.Common;
using PX.Objects.Common.Bql;
using PX.Objects.Common.Extensions;
using PX.Objects.Common.Utility;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Objects.EP;
using PX.Objects.Extensions.MultiCurrency;
using PX.Objects.Extensions.MultiCurrency.AP;
using PX.Objects.GL;
using PX.Objects.GL.FinPeriods;
using PX.Objects.GL.FinPeriods.TableDefinition;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace PX.Objects.AP
{
	[Serializable]
	public class AdjustedNotFoundException : PXException
	{
		public AdjustedNotFoundException(): base(ErrorMessages.ElementDoesntExist, Messages.APInvoice) {}
		public AdjustedNotFoundException(SerializationInfo info, StreamingContext context): base(info, context)
		{
			PXReflectionSerializer.RestoreObjectProps(this, info);
		}

		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			PXReflectionSerializer.GetObjectData(this, info);
			base.GetObjectData(info, context);
		}

	}

	public partial class APPaymentEntry : APDataEntryGraph<APPaymentEntry, APPayment>
	{
		public PXWorkflowEventHandler<APPayment> OnPrintCheck;
		public PXWorkflowEventHandler<APPayment> OnCancelPrintCheck;
		public PXWorkflowEventHandler<APPayment> OnReleaseDocument;
		public PXWorkflowEventHandler<APPayment> OnVoidDocument;
		public PXWorkflowEventHandler<APPayment> OnCloseDocument;
		public PXWorkflowEventHandler<APPayment> OnOpenDocument;


		public class MultiCurrency : APMultiCurrencyGraph<APPaymentEntry, APPayment>
		{
			protected override string DocumentStatus => Base.Document.Current?.Status;

			protected override CurySourceMapping GetCurySourceMapping()
			{
				return new CurySourceMapping(typeof(CashAccount))
	    {
					CuryID = typeof(CashAccount.curyID),
					CuryRateTypeID = typeof(CashAccount.curyRateTypeID)
				};
			}

			protected override CurySource CurrentSourceSelect()
	        {
				CurySource CurySource = base.CurrentSourceSelect();
				if (CurySource != null) CurySource.AllowOverrideRate = Base.vendor?.Current?.AllowOverrideRate;
				return CurySource;
			}

			protected override DocumentMapping GetDocumentMapping()
			{
				return new DocumentMapping(typeof(APPayment))
				{
					DocumentDate = typeof(APPayment.adjDate),
					BAccountID = typeof(APPayment.vendorID)
				};
	        }

			protected override PXSelectBase[] GetChildren()
			{
				return new PXSelectBase[]
	        {
					Base.Document,
					Base.Adjustments,
					Base.Adjustments_Balance,
					Base.Adjustments_Invoices,
					Base.Adjustments_Payments,
					Base.PaymentCharges,
					Base.dummy_CATran,
					Base.APPost
				};
	        }


			public virtual bool EnableCrossRate(APAdjust adj)
	        {
				if (adj.Released == false)
				{
					CurrencyInfo pay_info = GetCurrencyInfo(adj.AdjgCuryInfoID);
					CurrencyInfo vouch_info = GetCurrencyInfo(adj.AdjdCuryInfoID);

					return vouch_info != null
						&& string.Equals(pay_info.CuryID, vouch_info.CuryID) == false
						&& string.Equals(vouch_info.CuryID, vouch_info.BaseCuryID) == false;
				}
				else return false;
			}

			protected override bool ShouldBeDisabledDueToDocStatus()
			{
				switch (Base.Document.Current?.DocType)
				{
					case APDocType.VoidCheck:
					case APDocType.VoidRefund:
						return true;
					default:
						return base.ShouldBeDisabledDueToDocStatus();
				}
			}

			protected virtual void _(Events.FieldUpdated<APPayment, APPayment.cashAccountID> e)
			{
				if (Base._IsVoidCheckInProgress || !PXAccess.FeatureInstalled<FeaturesSet.multicurrency>()) return;
				else
				{
					RestoreDifferentCurrencyInfoIDs(Base.Adjustments);
					SourceFieldUpdated<APPayment.curyInfoID, APPayment.curyID, APPayment.adjDate>(e.Cache, e.Row);
					SetAdjgCuryInfoID(Base.Adjustments, e.Row.CuryInfoID);
				}
			}

			protected override void _(Events.FieldUpdated<Document, Document.documentDate> e)
			{
				base._(e);
				if (ShouldBeDisabledDueToDocStatus()) return;
				else Base.LoadInvoicesProc(true);

				APPayment payment = (APPayment)e.Row.Base;

				foreach (APAdjust adj in Base.Adjustments.View.SelectExternal().RowCast<APAdjust>())
				{
					APInvoice invoice = APInvoice.PK.Find(Base, adj.AdjdDocType, adj.AdjdRefNbr);

					if (payment.AdjDate <= invoice?.DiscDate && invoice.OrigModule == BatchModule.AP)
					{
						bool checkBalance = !(payment.Status == APDocStatus.Hold || payment.Status == APDocStatus.Balanced);   // Payment Amount is editable
						Base.applyAPAdjust(adj, payment.CuryUnappliedBal.Value, checkBalance, true);
					}
				}
			}

			protected virtual void _(Events.FieldSelecting<APAdjust, APAdjust.adjdCuryID> e)
			{
				e.ReturnValue = CuryIDFieldSelecting<APAdjust.adjdCuryInfoID>(e.Cache, e.Row);
	        }
	    }

		#region Cache Attached Events
		#region APPayment
		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXCustomizeBaseAttribute(typeof(PXUIFieldAttribute), nameof(PXUIFieldAttribute.DisplayName), "Original Document")]
		protected virtual void APPayment_OrigRefNbr_CacheAttached(PXCache sender) { }

		[PXMergeAttributes(Method = MergeMethod.Replace)]
		[PXSelector(typeof(Search<CABatch.batchNbr>))]
		[PXDBScalar(typeof(Search<CABatchDetail.batchNbr, Where<CABatchDetail.origModule, Equal<BatchModule.moduleAP>,
													And<CABatchDetail.origDocType, Equal<APPayment.docType>,
													And<CABatchDetail.origRefNbr, Equal<APPayment.refNbr>>>>>))]
		[PXUIField(DisplayName = "Batch Payment Nbr.", Enabled = false, Visibility = PXUIVisibility.SelectorVisible, Visible = true)]
		protected virtual void _(Events.CacheAttached<APPayment.batchPaymentRefNbr> e) { }
		#endregion

		#region APAdjust
		[PXMergeAttributes(Method = MergeMethod.Append)]
		[BatchNbrExt(typeof(Search<Batch.batchNbr, Where<Batch.module, Equal<BatchModule.moduleAP>>>),
			IsMigratedRecordField = typeof(APAdjust.isMigratedRecord))]
		protected virtual void APAdjust_AdjBatchNbr_CacheAttached(PXCache sender) { }

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXFormula(typeof(Switch<
			Case<Where<
				APAdjust.adjgDocType, Equal<Current<APPayment.docType>>,
				And<APAdjust.adjgRefNbr, Equal<Current<APPayment.refNbr>>>>,
				ARAdjust.adjType.adjusted>,
			ARAdjust.adjType.adjusting>))]
		protected virtual void APAdjust_AdjType_CacheAttached(PXCache sender) { }

		[PXInt()]
		protected virtual void APAdjust_AdjdWhTaxAcctID_CacheAttached(PXCache sender)
		{
		}

		[PXInt()]
		protected virtual void APAdjust_AdjdWhTaxSubID_CacheAttached(PXCache sender)
		{
		}
		

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXRemoveBaseAttribute(typeof(PXSelectorAttribute))] 
		protected virtual void APAdjust_DisplayRefNbr_CacheAttached(PXCache sender) { }
		#endregion
		#region EP Approval
		[PXDBDate]
		[PXDefault(typeof(APPayment.docDate), PersistingCheck = PXPersistingCheck.Nothing)]
		protected virtual void EPApproval_DocDate_CacheAttached(PXCache sender)
		{
		}

		[PXDBInt]
		[PXDefault(typeof(APPayment.vendorID), PersistingCheck = PXPersistingCheck.Nothing)]
		protected virtual void EPApproval_BAccountID_CacheAttached(PXCache sender)
		{
		}

		[PXDBString(60, IsUnicode = true)]
		[PXDefault(typeof(APPayment.docDesc), PersistingCheck = PXPersistingCheck.Nothing)]
		protected virtual void EPApproval_Descr_CacheAttached(PXCache sender)
		{
		}

		[PXDBLong]
		[CurrencyInfo(typeof(APPayment.curyInfoID))]
		protected virtual void EPApproval_CuryInfoID_CacheAttached(PXCache sender)
		{
		}

		[PXDBDecimal]
		[PXDefault(typeof(APPayment.curyOrigDocAmt), PersistingCheck = PXPersistingCheck.Nothing)]
		protected virtual void EPApproval_CuryTotalAmount_CacheAttached(PXCache sender)
		{
		}

		[PXDBDecimal]
		[PXDefault(typeof(APPayment.origDocAmt), PersistingCheck = PXPersistingCheck.Nothing)]
		protected virtual void EPApproval_TotalAmount_CacheAttached(PXCache sender)
		{
		}

		protected virtual void EPApproval_SourceItemType_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (Document.Current != null)
			{
				e.NewValue = new APDocType.ListAttribute()
					.ValueLabelDic[Document.Current.DocType];

				e.Cancel = true;
			}
		}
		protected virtual void EPApproval_Details_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (Document.Current != null)
			{
				e.NewValue = EPApprovalHelper.BuildEPApprovalDetailsString(sender, Document.Current);
			}
		}

		#endregion
		#region APTranPost
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXUIFieldAttribute(DisplayName = "Doc. Type")]
		protected virtual void APTranPostBal_SourceDocType_CacheAttached(PXCache sender) { }
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXUIFieldAttribute(DisplayName = "Reference Nbr.")]
		protected virtual void APTranPostBal_SourceRefNbr_CacheAttached(PXCache sender) { }
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXUIFieldAttribute(DisplayName = "Amount Paid")]
		protected virtual void APTranPostBal_CuryAmt_CacheAttached(PXCache sender) { }
		#endregion
		#endregion

		/// <summary>
		/// Necessary for proper cache resolution inside selector
		/// on <see cref="APAdjust.DisplayRefNbr"/>.
		/// </summary>
		public PXSelect<Standalone.APRegister> dummy_register;

		[PXViewName(Messages.APPayment)]
		[PXCopyPasteHiddenFields(typeof(APPayment.extRefNbr), typeof(APPayment.clearDate), typeof(APPayment.cleared))]
		public PXSelectJoin<APPayment,
			LeftJoinSingleTable<Vendor, On<Vendor.bAccountID, Equal<APPayment.vendorID>>>,
			Where<APPayment.docType, Equal<Optional<APPayment.docType>>,
			And<Where<Vendor.bAccountID, IsNull,
			Or<Match<Vendor, Current<AccessInfo.userName>>>>>>> Document;
		[PXCopyPasteHiddenFields(typeof(APPayment.clearDate), typeof(APPayment.cleared))]
		public PXSelect<APPayment, Where<APPayment.docType, Equal<Current<APPayment.docType>>, And<APPayment.refNbr, Equal<Current<APPayment.refNbr>>>>> CurrentDocument;

		[PXViewName(Messages.APAdjust)]
		[PXCopyPasteHiddenView]
		public PXSelectJoin<APAdjust, 
			LeftJoin<APInvoice, On<APInvoice.docType, Equal<APAdjust.adjdDocType>, 
				And<APInvoice.refNbr, Equal<APAdjust.adjdRefNbr>>>,
			LeftJoin<APTran, On<APInvoice.paymentsByLinesAllowed, Equal<True>,
				And<APTran.tranType, Equal<APAdjust.adjdDocType>,
				And<APTran.refNbr, Equal<APAdjust.adjdRefNbr>,
				And<APTran.lineNbr, Equal<APAdjust.adjdLineNbr>>>>>>>,
			Where<APAdjust.adjgDocType, Equal<Current<APPayment.docType>>, 
				And<APAdjust.adjgRefNbr, Equal<Current<APPayment.refNbr>>, 
				And<APAdjust.released, NotEqual<True>>>>> Adjustments;

		public PXSelect<APAdjust, 
			Where<APAdjust.adjgDocType, Equal<Optional<APPayment.docType>>, 
				And<APAdjust.adjgRefNbr, Equal<Optional<APPayment.refNbr>>, 
				And<APAdjust.released, NotEqual<True>>>>> Adjustments_Raw;

		public PXSelectJoin<
				Standalone.APAdjust,
			LeftJoinSingleTable<APInvoice,
				On<APInvoice.docType, Equal<Standalone.APAdjust.adjdDocType>,
					And<APInvoice.refNbr, Equal<Standalone.APAdjust.adjdRefNbr>>>,
			LeftJoin<Standalone.APRegisterAlias,
					On<Standalone.APRegisterAlias.docType, Equal<Standalone.APAdjust.adjdDocType>, And<Standalone.APRegisterAlias.refNbr, Equal<Standalone.APAdjust.adjdRefNbr>>>,
					LeftJoin<APTran,
						On<APTran.tranType, Equal<Standalone.APAdjust.adjdDocType>,
							And<APTran.refNbr, Equal<Standalone.APAdjust.adjdRefNbr>, 
								And<APTran.lineNbr, Equal<Standalone.APAdjust.adjdLineNbr>>>>,
						LeftJoin<CurrencyInfo,
							On<CurrencyInfo.curyInfoID, Equal<Standalone.APRegisterAlias.curyInfoID>>,
				LeftJoin<CM.CurrencyInfo2,
					On<CM.CurrencyInfo2.curyInfoID, Equal<Standalone.APAdjust.adjdCuryInfoID>>>>>>>,
			Where<Standalone.APAdjust.adjgDocType, Equal<Current<APPayment.docType>>,
				And<Standalone.APAdjust.adjgRefNbr, Equal<Current<APPayment.refNbr>>, 
				And<Standalone.APAdjust.released, Equal<Required<Standalone.APAdjust.released>>>>>> 
			Adjustments_Balance;

		public PXSelectJoin<APInvoice,
			InnerJoin<CurrencyInfo, On<CurrencyInfo.curyInfoID, Equal<APInvoice.curyInfoID>>,
				LeftJoin<APTran, On<APInvoice.paymentsByLinesAllowed, Equal<True>,
					And<APTran.tranType, Equal<APInvoice.docType>,
					And<APTran.refNbr, Equal<APInvoice.refNbr>,
					And<APTran.lineNbr, Equal<Required<APAdjust.adjdLineNbr>>>>>>>>,
			Where<APInvoice.vendorID, Equal<Current<APPayment.vendorID>>,
				And<APInvoice.docType, Equal<Required<APInvoice.docType>>,
				And<APInvoice.refNbr, Equal<Required<APInvoice.refNbr>>>>>> 
			APInvoice_DocType_RefNbr;

		public PXSelect<APInvoice,
			Where<APInvoice.vendorID, Equal<Current<APPayment.vendorID>>,
			And<APInvoice.docType, Equal<Required<APInvoice.docType>>,
			And<APInvoice.refNbr, Equal<Required<APInvoice.refNbr>>>>>>
			APInvoice_DocType_RefNbr_Single;

		public PXSelectReadonly2<APPayment,
				InnerJoin<CurrencyInfo, On<CurrencyInfo.curyInfoID, Equal<APPayment.curyInfoID>>>,
				Where<APPayment.vendorID, Equal<Current<APPayment.vendorID>>,
					And<APPayment.docType, Equal<Required<APPayment.docType>>,
					And<APPayment.refNbr, Equal<Required<APPayment.refNbr>>>>>>
		ARPayment_DocType_RefNbr;

		public PXSelectJoin<
			APAdjust,
				InnerJoinSingleTable<APInvoice,
					On<APInvoice.docType, Equal<APAdjust.adjdDocType>,
					And<APInvoice.refNbr, Equal<APAdjust.adjdRefNbr>>>,
				InnerJoin<Standalone.APRegisterAlias,
					On<Standalone.APRegisterAlias.docType, Equal<APAdjust.adjdDocType>,
					And<Standalone.APRegisterAlias.refNbr, Equal<APAdjust.adjdRefNbr>>>,
				LeftJoin<APTran, 
					On<APTran.tranType, Equal<APAdjust.adjdDocType>,
					And<APTran.refNbr, Equal<APAdjust.adjdRefNbr>,
					And<APTran.lineNbr, Equal<APAdjust.adjdLineNbr>>>>>>>,
			Where<
				APAdjust.adjgDocType, Equal<Current<APPayment.docType>>,
				And<APAdjust.adjgRefNbr, Equal<Current<APPayment.refNbr>>,
				And<APAdjust.released, NotEqual<True>,
				And<APAdjust.isInitialApplication, NotEqual<True>>>>>> 
			Adjustments_Invoices;

		private bool AppendAdjustmentsInvoicesTail(APAdjust adj, PXResult<APInvoice, CurrencyInfo, APTran> invoicesResult)
		{
			PXResult<APInvoice, Standalone.APRegisterAlias, APTran> result = new PXResult<APInvoice, Standalone.APRegisterAlias, APTran>(
				invoicesResult,
				Common.Utilities.Clone<APInvoice, Standalone.APRegisterAlias>(this, invoicesResult),
				invoicesResult
				);

			Adjustments_Invoices.StoreTailResult(
				result,
				new[] { adj },
				Document.Current.DocType, Document.Current.RefNbr
				);
			return true;
		}

		private void Adjustments_Invoices_BeforeSelect()
		{
			if (Adjustments_Raw.Cache.Cached.Empty_()) return;

			Adjustments_Raw.Cache.Cached.OfType<APAdjust>().Join(
				GetVendDocs(Document.Current, APSetup.Current).AsEnumerable().Cast<PXResult<APInvoice, CurrencyInfo, APTran>>(),
				_ => _.AdjdDocType + _.AdjdRefNbr + _.AdjdLineNbr,
				 _ => _.GetItem<APInvoice>().DocType + _.GetItem<APInvoice>().RefNbr + (_.GetItem<APTran>().LineNbr ?? 0),
				AppendAdjustmentsInvoicesTail
			).ToArray();
		}

		public PXSelectJoin<
			APAdjust,
				InnerJoinSingleTable<APPayment,
					On<APPayment.docType, Equal<APAdjust.adjdDocType>,
					And<APPayment.refNbr, Equal<APAdjust.adjdRefNbr>>>,
				LeftJoinSingleTable<APInvoice,
					On<APInvoice.docType, Equal<APAdjust.adjdDocType>,
					And<APInvoice.refNbr, Equal<APAdjust.adjdRefNbr>>>,
				InnerJoin<Standalone.APRegisterAlias,
					On<Standalone.APRegisterAlias.docType, Equal<APAdjust.adjdDocType>,
					And<Standalone.APRegisterAlias.refNbr, Equal<APAdjust.adjdRefNbr>>>>>>,
			Where<
				APAdjust.adjgDocType, Equal<Current<APPayment.docType>>,
				And<APAdjust.adjgRefNbr, Equal<Current<APPayment.refNbr>>,
				And<APAdjust.released, NotEqual<True>>>>>
			Adjustments_Payments;

		[PXViewName(Messages.APPrintCheckDetail)]
		public PXSelect<APPrintCheckDetail,
			Where<APPrintCheckDetail.adjgDocType, Equal<Current<APPayment.docType>>,
				And<APPrintCheckDetail.adjgRefNbr, Equal<Current<APPayment.refNbr>>>>> CheckDetails;

		[PXViewName(Messages.ApplicationHistory)]
		[PXCopyPasteHiddenView]
		public PXSelectJoin<APTranPostBal,
				LeftJoin<Standalone.APRegister,
					On<Standalone.APRegister.docType, Equal<APTranPostBal.sourceDocType>,
					And<Standalone.APRegister.refNbr, Equal<APTranPostBal.sourceRefNbr>>>,
				LeftJoinSingleTable<APInvoice,
					On<APInvoice.docType, Equal<APTranPostBal.sourceDocType>,
					And<APInvoice.refNbr, Equal<APTranPostBal.sourceRefNbr>>>,
				LeftJoin<APTran,
					On<APTran.tranType, Equal<APTranPostBal.sourceDocType>,
					And<APTran.refNbr, Equal<APTranPostBal.sourceRefNbr>,
					And<APTran.lineNbr, Equal<APTranPostBal.lineNbr>>>>,
				LeftJoin<CM.CurrencyInfo,
					On<CM.CurrencyInfo.curyInfoID, Equal<Standalone.APRegister.curyInfoID>>,
				LeftJoin<CM.CurrencyInfo2,
						On<CM.CurrencyInfo2.curyInfoID, Equal<Current<APPayment.curyInfoID>>>,
				LeftJoin<APAdjust2, On<APAdjust2.noteID, Equal<APTranPostBal.refNoteID>>>>>>>>,
				Where<APTranPostBal.docType, Equal<Current<APPayment.docType>>,
					And<APTranPostBal.refNbr, Equal<Current<APPayment.refNbr>>,
					And<APTranPostBal.type, NotEqual<APTranPost.type.origin>,
					And<APTranPostBal.type, NotEqual<APTranPost.type.rgol>,
					And<APTranPostBal.type, NotEqual<APTranPost.type.retainage>,
				   And<APTranPostBal.type, NotEqual<ARTranPost.type.retainageReverse>>>>>>>,
				OrderBy<Asc<APTranPostBal.iD>>>
				APPost;


		[PXViewName(Messages.APAdjustHistory)]
		public PXSelect<APAdjust> Adjustments_print;

		public PXSelect<CurrencyInfo, Where<CurrencyInfo.curyInfoID, Equal<Current<APPayment.curyInfoID>>>> currencyinfo;
		[PXReadOnlyView]
		public PXSelect<APInvoice> dummy_APInvoice;
		[PXReadOnlyView]
		public PXSelect<CATran, Where<CATran.tranID, Equal<Current<APPayment.cATranID>>>> dummy_CATran;

		[PXViewName(Messages.APAddress)]
		public PXSelect<APAddress, Where<APAddress.addressID, Equal<Current<APPayment.remitAddressID>>>> Remittance_Address;
		[PXViewName(Messages.APContact)]
		public PXSelect<APContact, Where<APContact.contactID, Equal<Current<APPayment.remitContactID>>>> Remittance_Contact;

		public PXSelect<CurrencyInfo, Where<CurrencyInfo.curyInfoID, Equal<Required<CurrencyInfo.curyInfoID>>>> CurrencyInfo_CuryInfoID;

		public PXSelectReadonly2<APInvoice, 
			LeftJoin<APTran, On<APInvoice.paymentsByLinesAllowed, Equal<True>,
				And<APTran.tranType, Equal<APInvoice.docType>,
				And<APTran.refNbr, Equal<APInvoice.refNbr>,
				And<APTran.lineNbr, Equal<Required<APAdjust.adjdLineNbr>>>>>>>,
			Where<APInvoice.vendorID, Equal<Required<APInvoice.vendorID>>, 
				And<APInvoice.docType, Equal<Required<APInvoice.docType>>, 
				And<APInvoice.refNbr, Equal<Required<APInvoice.refNbr>>>>>> 
			APInvoice_VendorID_DocType_RefNbr;

		public PXSelect<APPayment, Where<APPayment.vendorID, Equal<Required<APPayment.vendorID>>, And<APPayment.docType, Equal<Required<APPayment.docType>>, And<APPayment.refNbr, Equal<Required<APPayment.refNbr>>>>>> APPayment_VendorID_DocType_RefNbr;

		public APPaymentChargeSelect<APPayment, APPayment.paymentMethodID, APPayment.cashAccountID, APPayment.docDate, APPayment.tranPeriodID,
			Where<APPaymentChargeTran.docType, Equal<Current<APPayment.docType>>,
				And<APPaymentChargeTran.refNbr, Equal<Current<APPayment.refNbr>>>>> PaymentCharges;

		[PXViewName(Messages.Vendor)]
		public PXSetup<Vendor, Where<Vendor.bAccountID, Equal<Optional<APPayment.vendorID>>>> vendor;
		public PXSetup<Location, Where<Location.bAccountID, Equal<Current<APPayment.vendorID>>, And<Location.locationID, Equal<Optional<APPayment.vendorLocationID>>>>> location;
		public PXSetup<VendorClass, Where<VendorClass.vendorClassID, Equal<Current<Vendor.vendorClassID>>>> vendorclass;
		public PXSelect<PaymentMethodAccount, Where<PaymentMethodAccount.cashAccountID, Equal<Current<APPayment.cashAccountID>>>, OrderBy<Asc<PaymentMethodAccount.aPIsDefault>>> CashAcctDetail_AccountID;
		public PXSetup<PaymentMethod, Where<PaymentMethod.paymentMethodID, Equal<Optional<APPayment.paymentMethodID>>>> paymenttype;
		public PXSetup<CashAccount, Where<CashAccount.cashAccountID, Equal<Optional<APPayment.cashAccountID>>>> cashaccount;
		public PXSetup<PaymentMethodAccount, Where<PaymentMethodAccount.cashAccountID, Equal<Optional<APPayment.cashAccountID>>, And<PaymentMethodAccount.paymentMethodID, Equal<Current<APPayment.paymentMethodID>>>>> cashaccountdetail;
		public PXSelectReadonly<
			OrganizationFinPeriod,
			Where<OrganizationFinPeriod.finPeriodID, Equal<Optional<APPayment.adjFinPeriodID>>,
				And<EqualToOrganizationOfBranch<OrganizationFinPeriod.organizationID, Current<APPayment.branchID>>>>>
			finperiod;

		public PXSetup<GLSetup> glsetup;
		public PXSelect<CashAccountCheck> CACheck;

		public PXSelect<GLVoucher, Where<True, Equal<False>>> Voucher;

		[PXViewName(EP.Messages.Employee)]
		public PXSetup<EPEmployee, Where<EPEmployee.defContactID, Equal<Current<APPayment.employeeID>>>> employee;
		public PXSelect<APSetupApproval,
			Where<APSetupApproval.docType, Equal<Current<APPayment.docType>>,
				And<Where<Current<APPayment.docType>, Equal<APDocType.check>,
					Or<Current<APPayment.docType>, Equal<APDocType.prepayment>>>>>> SetupApproval;
		[PXViewName(EP.Messages.Approval)]
		public EPApprovalAutomationWithReservedDoc<APPayment, APPayment.approved, APPayment.rejected, APPayment.hold, APSetupApproval> Approval;

		public bool AutoPaymentApp
		{
			get;
			set;
		}

		public bool IsReverseProc
		{
			get;
			set;
		}

		public OrganizationFinPeriod FINPERIOD => finperiod.Select();

		[InjectDependency]
		public IFinPeriodRepository FinPeriodRepository { get; set; }

		[InjectDependency]
		public IFinPeriodUtils FinPeriodUtils { get; set; }

		public static string[] AdjgDocTypesToValidateFinPeriod = new string[]
		{
			APDocType.Check,
			APDocType.DebitAdj,
			APDocType.Prepayment,
			APDocType.Refund
		};

		#region Setups
		public PXSetup<APSetup> APSetup;
		#endregion

		#region Buttons
		public PXAction<APPayment> cancel;
		[PXCancelButton]
		[PXUIField(MapEnableRights = PXCacheRights.Select)]
		protected new virtual IEnumerable Cancel(PXAdapter a)
		{
			string lastDocType = null;
			string lastRefNbr = null;
			if (this.Document.Current != null)
			{
				lastDocType = this.Document.Current.DocType;
				lastRefNbr = this.Document.Current.RefNbr;
			}
			PXResult<APPayment, Vendor> r = null;
			foreach (PXResult<APPayment, Vendor> e in (new PXCancel<APPayment>(this, "Cancel")).Press(a))
			{
				r = e;
			}
			if (Document.Cache.GetStatus((APPayment)r) == PXEntryStatus.Inserted)
			{
				if (lastRefNbr != ((APPayment)r).RefNbr)
				{
					if (((APPayment)r).DocType == APPaymentType.Check || ((APPayment)r).DocType == APPaymentType.Prepayment)
					{
						string docType = ((APPayment)r).DocType;
						string refNbr = ((APPayment)r).RefNbr;
						string searchDocType = docType == APPaymentType.Check ? APPaymentType.Prepayment : APPaymentType.Check;
						APPayment duplicatePayment = PXSelect<APPayment,
							Where<APPayment.docType, Equal<Required<APPayment.docType>>, And<APPayment.refNbr, Equal<Required<APPayment.refNbr>>>>>
							.Select(this, searchDocType, refNbr);
						APInvoice inv = null;
						if (searchDocType == APPaymentType.Prepayment)
						{
							inv = PXSelect<APInvoice, Where<APInvoice.docType, Equal<APInvoiceType.prepayment>, And<APInvoice.refNbr, Equal<Required<APPayment.refNbr>>>>>.Select(this, refNbr);
						}
						if (duplicatePayment != null || inv != null)
						{
							Document.Cache.RaiseExceptionHandling<APPayment.refNbr>((APPayment)r, refNbr,
								new PXSetPropertyException<APPayment.refNbr>(Messages.SameRefNbr, searchDocType == APPaymentType.Check ? Messages.Check : Messages.Prepayment, refNbr));
						}
					}

				}
			}

			yield return r;
		}
		
		public PXAction<APPayment> printAPEdit;
		[PXButton(CommitChanges = true), PXUIField(DisplayName = "AP Edit Detailed", MapEnableRights = PXCacheRights.Select)]
		public virtual IEnumerable PrintAPEdit(PXAdapter adapter, string reportID = null) => Report(adapter, reportID ?? "AP610500");

		public PXAction<APPayment> printAPRegister;
		[PXButton(CommitChanges = true), PXUIField(DisplayName = "AP Register Detailed", MapEnableRights = PXCacheRights.Select)]
		public virtual IEnumerable PrintAPRegister(PXAdapter adapter, string reportID = null) => Report(adapter, reportID ??"AP622000");

		public PXAction<APPayment> printAPPayment;
		[PXButton(CommitChanges = true), PXUIField(DisplayName = "AP Payment Register", MapEnableRights = PXCacheRights.Select)]
		public virtual IEnumerable PrintAPPayment(PXAdapter adapter, string reportID = null) => Report(adapter, reportID ??"AP622500");

		public PXAction<APPayment> printCheck;
		[PXUIField(DisplayName = "Print/Process", MapEnableRights = PXCacheRights.Select)]
		[PXButton]
		[APMigrationModeDependentActionRestriction(
			restrictInMigrationMode: true,
			restrictForRegularDocumentInMigrationMode: true,
			restrictForUnreleasedMigratedDocumentInNormalMode: true)]
		public virtual IEnumerable PrintCheck(PXAdapter adapter)
		{
			if (this.IsDirty)
			{
				this.Save.Press();
			}
			APPayment doc = Document.Current;
			APPrintChecks pp = PXGraph.CreateInstance<APPrintChecks>();
			PrintChecksFilter filter_copy = PXCache<PrintChecksFilter>.CreateCopy(pp.Filter.Current);
			filter_copy.BranchID = doc.BranchID;
			filter_copy.PayAccountID = doc.CashAccountID;
			filter_copy.PayTypeID = doc.PaymentMethodID;
			pp.Filter.Cache.Update(filter_copy);
			doc.Selected = true;
			doc.Passed = true;
			pp.APPaymentList.Cache.Update(doc);
			pp.APPaymentList.Cache.SetStatus(doc, PXEntryStatus.Updated);
			pp.APPaymentList.Cache.IsDirty = false;
			throw new PXRedirectRequiredException(pp, "Preview");
		}

		public PXAction<APPayment> newVendor;
		[PXUIField(DisplayName = "New Vendor", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXLookupButton]
		public virtual IEnumerable NewVendor(PXAdapter adapter)
		{
			VendorMaint graph = PXGraph.CreateInstance<VendorMaint>();
			throw new PXRedirectRequiredException(graph, "New Vendor");
		}

		public PXAction<APPayment> editVendor;
		[PXUIField(DisplayName = "Edit Vendor", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXLookupButton]
		public virtual IEnumerable EditVendor(PXAdapter adapter)
		{
			if (vendor.Current != null)
			{
				VendorMaint graph = PXGraph.CreateInstance<VendorMaint>();
				graph.BAccount.Current = (VendorR)vendor.Current;
				throw new PXRedirectRequiredException(graph, "Edit Vendor");
			}
			return adapter.Get();
		}

		public PXAction<APPayment> vendorDocuments;
		[PXUIField(DisplayName = "Vendor Details", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton]
		public virtual IEnumerable VendorDocuments(PXAdapter adapter)
		{
			if (vendor.Current != null)
			{
				APDocumentEnq graph = PXGraph.CreateInstance<APDocumentEnq>();
				graph.Filter.Current.VendorID = vendor.Current.BAccountID;
				graph.Filter.Select();
				throw new PXRedirectRequiredException(graph, "Vendor Details");
			}
			return adapter.Get();
		}

		[PXUIField(DisplayName = "Release", MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update)]
		[PXProcessButton]
		[APMigrationModeDependentActionRestriction(
			restrictInMigrationMode: false,
			restrictForRegularDocumentInMigrationMode: true,
			restrictForUnreleasedMigratedDocumentInNormalMode: true)]
		public override IEnumerable Release(PXAdapter adapter)
		{
			PXCache cache = Document.Cache;
			List<APRegister> list = new List<APRegister>();
			foreach (APPayment apdoc in adapter.Get<APPayment>())
			{
				if (apdoc.Status != APDocStatus.Balanced && apdoc.Status != APDocStatus.Printed && apdoc.Status != APDocStatus.Open)
				{
					throw new PXException(Messages.Document_Status_Invalid);
				}
				if ((apdoc.DocType == APDocType.Check
						|| apdoc.DocType == APDocType.Prepayment && !IsRequestPrepayment(apdoc)
						|| apdoc.DocType == APDocType.Refund
						|| apdoc.DocType == APDocType.VoidCheck)
					&& this.PaymentRefMustBeUnique && string.IsNullOrEmpty(apdoc.ExtRefNbr))
				{
					bool extRefNbrCanNotBeEmpty = true;
					if (apdoc.DocType == APDocType.VoidCheck || apdoc.DocType == APDocType.Refund)
					{
						APPayment originDoc = PXSelectReadonly<APPayment, Where<APPayment.docType, Equal<Required<APPayment.docType>>,
							And<APPayment.refNbr, Equal<Required<APPayment.refNbr>>>>>.SelectSingleBound(this, null, apdoc.OrigDocType, apdoc.OrigRefNbr);

						if (originDoc != null && String.IsNullOrEmpty(originDoc.ExtRefNbr))
						{
							extRefNbrCanNotBeEmpty = false;
						}
					}

					if (extRefNbrCanNotBeEmpty && apdoc.Released != true)
					{
						cache.RaiseExceptionHandling<APPayment.extRefNbr>(apdoc, apdoc.ExtRefNbr,
							new PXRowPersistingException(typeof(APPayment.extRefNbr).Name, null, ErrorMessages.FieldIsEmpty, PXUIFieldAttribute.GetDisplayName<APPayment.extRefNbr>(cache)));
					}
				}
				list.Add((APRegister)cache.Update(apdoc) ?? apdoc);
			}

			Save.Press();
			PXLongOperation.StartOperation(this, delegate () { APDocumentRelease.ReleaseDoc(list, false); });
			return list;
		}

		[PXUIField(DisplayName = "Void", MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update)]
		[PXProcessButton]
		[APMigrationModeDependentActionRestriction(
			restrictInMigrationMode: false,
			restrictForRegularDocumentInMigrationMode: true,
			restrictForUnreleasedMigratedDocumentInNormalMode: true)]
		public override IEnumerable VoidCheck(PXAdapter adapter)
		{
			if (Document.Current != null && 
				Document.Current.Released == true && 
				Document.Current.Voided == false &&
				APPaymentType.VoidEnabled(Document.Current.DocType))
			{
				APAdjust checkApplication = PXSelect<APAdjust, 
					Where<APAdjust.adjdDocType, Equal<Required<APAdjust.adjdDocType>>, 
						And<APAdjust.adjdRefNbr, Equal<Required<APAdjust.adjdRefNbr>>, 
						And<APAdjust.adjgDocType, In3<APDocType.check, APDocType.prepayment>>>>>
					.SelectWindowed(this, 0, 1, Document.Current.DocType, Document.Current.RefNbr);

				if (checkApplication != null && checkApplication.IsSelfAdjustment() != true)
				{
					throw new PXException(Messages.PaymentIsPayedByCheck, checkApplication.AdjgRefNbr);
				}

				APAdjust refundApplication = PXSelect<
					APAdjust, 
					Where<
						APAdjust.adjdDocType, Equal<Required<APAdjust.adjdDocType>>, 
						And<APAdjust.adjdRefNbr, Equal<Required<APAdjust.adjdRefNbr>>, 
						And<APAdjust.adjgDocType, Equal<APDocType.refund>,
						And<APAdjust.voided, NotEqual<True>>>>>>
					.SelectWindowed(this, 0, 1, Document.Current.DocType, Document.Current.RefNbr);

				if (refundApplication != null && refundApplication.IsSelfAdjustment() != true)
				{
					throw new PXException(
						Common.Messages.DocumentHasBeenRefunded,
						GetLabel.For<APDocType>(Document.Current.DocType),
						Document.Current.RefNbr,
						GetLabel.For<APDocType>(refundApplication.AdjgDocType),
						refundApplication.AdjgRefNbr);
				}

				if (APSetup.Current.MigrationMode != true &&
					Document.Current.IsMigratedRecord == true &&
					Document.Current.CuryInitDocBal != Document.Current.CuryOrigDocAmt)
				{
					throw new PXException(Messages.MigrationModeIsDeactivatedForMigratedDocument);
				}

				if (APSetup.Current.MigrationMode == true && Document.Current.IsMigratedRecord == true)
				{
					if (PXSelect<APAdjust,
						Where<APAdjust.adjgDocType, Equal<Current<APPayment.docType>>,
						And<APAdjust.adjgRefNbr, Equal<Current<APPayment.refNbr>>,
						And<APAdjust.released, Equal<True>,
						And<APAdjust.voided, NotEqual<True>,
						And<APAdjust.isMigratedRecord, NotEqual<True>,
						And<APAdjust.isInitialApplication, NotEqual<True>>>>>>>>.Select(this)
						.RowCast<APAdjust>()
						.Any(_ => _.VoidAppl != true))
					throw new PXException(Common.Messages.CannotVoidPaymentRegularUnreversedApplications);
				}
				
				APPayment voidcheck = Document.Search<APPayment.refNbr>(Document.Current.RefNbr, APPaymentType.GetVoidingAPDocType(Document.Current.DocType));

				if (voidcheck != null)
				{
					if (IsContractBasedAPI) return new[] { voidcheck };
					else
					{
						Document.Current = voidcheck;
						throw new PXRedirectRequiredException(this, Messages.Void);
					}
				}

				DeleteUnreleasedApplications();
				this.Save.Press();

				APPayment doc = PXCache<APPayment>.CreateCopy(Document.Current);

				FinPeriodUtils.VerifyAndSetFirstOpenedFinPeriod<APPayment.finPeriodID, APPayment.branchID>(
					Document.Cache,
					doc,
					finperiod,
					typeof(OrganizationFinPeriod.aPClosed));

				TryToVoidCheck(doc);

				Document.Cache.RaiseExceptionHandling<APPayment.finPeriodID>(Document.Current, Document.Current.FinPeriodID, null);
				if (IsContractBasedAPI || IsImport)
				{
					return new[] { this.Document.Current };
				}
				else
				{
					// Redirect to itself for UI
					throw new PXRedirectRequiredException(this, Messages.Voided);
				}
			}
			return Document.Select();
		}

		public PXAction<APPayment> viewPPDVATAdj;

		[PXUIField(MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXLookupButton]
		public virtual IEnumerable ViewPPDVATAdj(PXAdapter adapter)
		{
			APTranPostBal post = this.APPost.Current;
			APAdjust adj = PXSelect<APAdjust, Where<APAdjust.noteID, Equal<Required<APTranPostBal.refNoteID>>>>.Select(this, post.RefNoteID);
			if (adj != null)
			{
				APInvoiceEntry docgraph = CreateInstance<APInvoiceEntry>();
				docgraph.Document.Current =	docgraph.Document.Search<APInvoice.refNbr>(adj.PPDVATAdjRefNbr, adj.PPDVATAdjDocType);
				throw new PXRedirectRequiredException(docgraph, true, Messages.ViewDocument) { Mode = PXBaseRedirectException.WindowMode.NewWindow };
			}
			return adapter.Get();
		}

		public override void Clear()
		{
			this.balanceCache = null;
			base.Clear();
		}

		private class PXLoadInvoiceException : Exception
		{
			public PXLoadInvoiceException() { }

			public PXLoadInvoiceException(SerializationInfo info, StreamingContext context)
				: base(info, context) { }

		}

		private APAdjust AddAdjustment(APAdjust adj)
		{
			if (Document.Current.CuryUnappliedBal == 0m && Document.Current.CuryOrigDocAmt > 0m)
			{
				throw new PXLoadInvoiceException();
			}

			return this.Adjustments.Insert(adj);
		}

		private void LoadInvoicesProc(bool LoadExistingOnly)
		{
			Dictionary<string, APAdjust> existing = new Dictionary<string, APAdjust>();
			APPayment currentDoc = Document.Current;
			try
			{
				if (currentDoc == null || currentDoc.VendorID == null || currentDoc.OpenDoc == false || currentDoc.DocType != APDocType.Check && currentDoc.DocType != APDocType.Prepayment && currentDoc.DocType != APDocType.Refund)
				{
					throw new PXLoadInvoiceException();
				}

				foreach (PXResult<APAdjust> res in Adjustments_Raw.Select())
				{
					APAdjust old_adj = (APAdjust)res;

					if (LoadExistingOnly == false)
					{
						old_adj = PXCache<APAdjust>.CreateCopy(old_adj);
						old_adj.CuryAdjgAmt = null;
						old_adj.CuryAdjgDiscAmt = null;
						old_adj.CuryAdjgWhTaxAmt = null;
						old_adj.CuryAdjgPPDAmt = null;
					}

					string s = string.Format("{0}_{1}_{2}", old_adj.AdjdDocType, old_adj.AdjdRefNbr, old_adj.AdjdLineNbr);
					existing.Add(s, old_adj);
					Adjustments.Cache.Delete((APAdjust)res);
				}

				currentDoc.AdjCntr++;
				Document.Cache.MarkUpdated(currentDoc);
				Document.Cache.IsDirty = true;

				foreach (APAdjust res in existing.Values.OrderBy(o => o.AdjgBalSign))
				{
					APAdjust adj = new APAdjust
					{
						AdjdDocType = res.AdjdDocType,
						AdjdRefNbr = res.AdjdRefNbr,
						AdjdLineNbr = res.AdjdLineNbr
					};

					try
					{
						adj = PXCache<APAdjust>.CreateCopy(AddAdjustment(adj));
						if (res.CuryAdjgWhTaxAmt != null && res.CuryAdjgWhTaxAmt < adj.CuryAdjgWhTaxAmt)
						{
							adj.CuryAdjgWhTaxAmt = res.CuryAdjgWhTaxAmt;
							adj = PXCache<APAdjust>.CreateCopy((APAdjust)this.Adjustments.Cache.Update(adj));
						}
							
						if (res.CuryAdjgDiscAmt != null && res.CuryAdjgDiscAmt < 0m
							? res.CuryAdjgDiscAmt > adj.CuryAdjgDiscAmt
							: res.CuryAdjgDiscAmt < adj.CuryAdjgDiscAmt)
						{
							adj.CuryAdjgDiscAmt = res.CuryAdjgDiscAmt;
							adj.CuryAdjgPPDAmt = res.CuryAdjgDiscAmt;
							adj = PXCache<APAdjust>.CreateCopy((APAdjust)this.Adjustments.Cache.Update(adj));
						}

						if (res.CuryAdjgAmt != null && res.CuryAdjgAmt < 0m
							? res.CuryAdjgAmt > adj.CuryAdjgAmt
							: res.CuryAdjgAmt < adj.CuryAdjgAmt)
						{
							adj.CuryAdjgAmt = res.CuryAdjgAmt;
							this.Adjustments.Cache.Update(adj);
						}
					}
					catch (PXSetPropertyException) { }
				}

				if (LoadExistingOnly)
				{
					return;
				}
				this.FieldVerifying.AddHandler<APAdjust.adjdAPSub>(suppresSubVerify);
				AutoPaymentApp = true;
				foreach (PXResult<APInvoice, CurrencyInfo, APTran> rec in GetVendDocs(currentDoc, APSetup.Current))
				{
					APInvoice invoice = rec;
					APTran tran = rec;

					string s = string.Format("{0}_{1}_{2}", invoice.DocType, invoice.RefNbr, tran?.LineNbr ?? 0);
					if (existing.ContainsKey(s) == false)
					{
						APAdjust adj = new APAdjust
						{
							AdjdDocType = invoice.DocType,
							AdjdRefNbr = invoice.RefNbr,
							AdjdLineNbr = tran?.LineNbr ?? 0,
							AdjgDocType = currentDoc.DocType,
							AdjgRefNbr = currentDoc.RefNbr,
							AdjNbr = currentDoc.AdjCntr
						};

						AddBalanceCache(adj, rec);
						PXSelectorAttribute.StoreResult<APAdjust.adjdRefNbr>(Adjustments.Cache, adj, new APAdjust.APInvoice
						{
							DocType = adj.AdjdDocType,
							RefNbr = adj.AdjdRefNbr,
							PaymentsByLinesAllowed = invoice.PaymentsByLinesAllowed
						});
						adj.VendorID = invoice.VendorID;
						StoreApplicationDataResult(adj, rec);
						AddAdjustment(adj);
					}
				}

				this.FieldVerifying.RemoveHandler<APAdjust.adjdAPSub>(suppresSubVerify);
				AutoPaymentApp = false;
				if (currentDoc.CuryApplAmt < 0m)
				{
					List<APAdjust> debits = new List<APAdjust>();

					foreach (APAdjust adj in Adjustments_Raw.Select())
					{
						if (adj.AdjdDocType == APDocType.DebitAdj && adj.AdjAmt >= 0m)
						{
							debits.Add(adj);
						}
					}

					debits.Sort((a, b) =>
						{
							return ((IComparable)a.CuryAdjgAmt).CompareTo(b.CuryAdjgAmt);
						});

					foreach (APAdjust adj in debits)
					{
						if (adj.CuryAdjgAmt <= -currentDoc.CuryApplAmt)
						{
							Adjustments.Delete(adj);
						}
						else
						{
							APAdjust copy = PXCache<APAdjust>.CreateCopy(adj);
							copy.CuryAdjgAmt += currentDoc.CuryApplAmt;
							Adjustments.Update(copy);
						}
					}
				}
			}
			catch (PXLoadInvoiceException)
			{
			}
		}

		private void suppresSubVerify(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			e.Cancel = true;
		}

		protected virtual PXResultset<APInvoice> GetVendDocs(APPayment currentAPPayment, APSetup currentAPSetup)
		{
			return GetVendDocs(currentAPPayment, this, currentAPSetup);
		}

		public static PXResultset<APInvoice> GetVendDocs(APPayment currentAPPayment, PXGraph graph, APSetup currentAPSetup)
		{
			var cmd = new PXSelectReadonly2<APInvoice,
							InnerJoin<CurrencyInfo, On<CurrencyInfo.curyInfoID, Equal<APInvoice.curyInfoID>>,
							LeftJoin<APTran, On<APInvoice.paymentsByLinesAllowed, Equal<True>,
								And<APTran.tranType, Equal<APInvoice.docType>,
								And<APTran.refNbr, Equal<APInvoice.refNbr>,
								And<APTran.curyTranBal, NotEqual<decimal0>>>>>,
							LeftJoin<APAdjust, On<APAdjust.adjdDocType, Equal<APInvoice.docType>, And<APAdjust.adjdRefNbr, Equal<APInvoice.refNbr>, And<APAdjust.released, Equal<False>>>>,
							LeftJoin<APAdjust2, On<APAdjust2.adjgDocType, Equal<APInvoice.docType>, And<APAdjust2.adjgRefNbr, Equal<APInvoice.refNbr>, And<APAdjust2.released, Equal<False>, And<APAdjust2.voided, Equal<False>>>>>,
							LeftJoin<APPayment, On<APPayment.docType, Equal<APInvoice.docType>, And<APPayment.refNbr, Equal<APInvoice.refNbr>, And<APPayment.docType, Equal<APDocType.prepayment>>>>>>>>>,
								Where<APInvoice.vendorID, Equal<Required<APPayment.vendorID>>,
											And2<Where<APInvoice.released, Equal<True>, Or<APInvoice.prebooked, Equal<True>>>,
										And<APInvoice.openDoc, Equal<True>,
										And<APInvoice.hold, Equal<False>,
										And<APAdjust.adjgRefNbr, IsNull,
										And<APAdjust2.adjdRefNbr, IsNull,
										And<APInvoice.pendingPPD, NotEqual<True>,
										And<APPayment.refNbr, IsNull,
										And<Where<APInvoice.docDate, LessEqual<Required<APPayment.adjDate>>,
											And<APInvoice.tranPeriodID, LessEqual<Required<APPayment.tranPeriodID>>,
											Or<Required<APPayment.docType>, Equal<APDocType.check>, And<Required<APSetup.earlyChecks>, Equal<True>,
											Or<Required<APPayment.docType>, Equal<APDocType.voidCheck>, And<Required<APSetup.earlyChecks>, Equal<True>,
											Or<Required<APPayment.docType>, Equal<APDocType.prepayment>, And<Required<APSetup.earlyChecks>, Equal<True>>>>>>>>>>>>>>>>>>,
							OrderBy<Asc<APInvoice.dueDate, Asc<APInvoice.refNbr, Asc<APTran.refNbr>>>>>(graph);

			switch (currentAPPayment.DocType)
			{
				case APDocType.Refund:
					cmd.WhereAnd<Where<APInvoice.docType, Equal<APDocType.debitAdj>>>();
					break;
				case APDocType.Prepayment:
					cmd.WhereAnd<Where<APInvoice.docType, Equal<APDocType.invoice>, Or<APInvoice.docType, Equal<APDocType.creditAdj>>>>();
					break;
				case APDocType.Check:
					cmd.WhereAnd<Where<APInvoice.docType, Equal<APDocType.invoice>, Or<APInvoice.docType, Equal<APDocType.debitAdj>, Or<APInvoice.docType, Equal<APDocType.creditAdj>, Or<APInvoice.docType, Equal<APDocType.prepayment>, And<APInvoice.curyID, Equal<Required<APPayment.curyID>>>>>>>>();
					break;
				default:
					cmd.WhereAnd<Where<True, Equal<False>>>();
					break;
			}

			PXResultset<APInvoice, CurrencyInfo, APTran> venddocs = new PXResultset<APInvoice, CurrencyInfo, APTran>();
			foreach (PXResult<APInvoice, CurrencyInfo, APTran> rec in
				cmd.Select(currentAPPayment.VendorID,
					currentAPPayment.AdjDate,
					currentAPPayment.AdjTranPeriodID,
					currentAPPayment.DocType,
					currentAPSetup.EarlyChecks,
					currentAPPayment.DocType,
					currentAPSetup.EarlyChecks,
					currentAPPayment.DocType,
					currentAPSetup.EarlyChecks
					))
			{
				venddocs.Add(rec);
			}

			venddocs.Sort((a, b) =>
			{
				int aSortOrder = 0;
				int bSortOrder = 0;

				if (currentAPPayment.CuryOrigDocAmt > 0m)
				{
					aSortOrder += (((APInvoice)a).DocType == APDocType.DebitAdj ? 0 : 1000);
					bSortOrder += (((APInvoice)b).DocType == APDocType.DebitAdj ? 0 : 1000);
				}
				else
				{
					aSortOrder += (((APInvoice)a).DocType == APDocType.DebitAdj ? 1000 : 0);
					bSortOrder += (((APInvoice)b).DocType == APDocType.DebitAdj ? 1000 : 0);
				}

				DateTime aDueDate = ((APInvoice)a).DueDate ?? DateTime.MinValue;
				DateTime bDueDate = ((APInvoice)b).DueDate ?? DateTime.MinValue;

				object aObj;
				object bObj;

				aSortOrder += (1 + aDueDate.CompareTo(bDueDate)) / 2 * 100;
				bSortOrder += (1 - aDueDate.CompareTo(bDueDate)) / 2 * 100;

				aObj = ((APInvoice)a).RefNbr;
				bObj = ((APInvoice)b).RefNbr;
				aSortOrder += (1 + ((IComparable)aObj).CompareTo(bObj)) / 2 * 10;
				bSortOrder += (1 - ((IComparable)aObj).CompareTo(bObj)) / 2 * 10;

				aObj = PXResult.Unwrap<APTran>(a).LineNbr ?? 0;
				bObj = PXResult.Unwrap<APTran>(b).LineNbr ?? 0;
				aSortOrder += (1 + ((IComparable)aObj).CompareTo(bObj)) / 2;
				bSortOrder += (1 - ((IComparable)aObj).CompareTo(bObj)) / 2;
				return aSortOrder.CompareTo(bSortOrder);
			});
			return venddocs;
		}

		public PXAction<APPayment> loadInvoices;
		[PXUIField(DisplayName = "Load Documents", MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update)]
		[PXButton(ImageKey = PX.Web.UI.Sprite.Main.Refresh)]
		[APMigrationModeDependentActionRestriction(
			restrictInMigrationMode: true,
			restrictForRegularDocumentInMigrationMode: true,
			restrictForUnreleasedMigratedDocumentInNormalMode: true)]
		public virtual IEnumerable LoadInvoices(PXAdapter adapter)
		{
			if (IsContractBasedAPI || IsImport) LoadInvoicesProc(false);
			else
			{
				APPaymentEntry clone = this.Clone();
				PXLongOperation.StartOperation(this, () =>
				{
					clone.LoadInvoicesProc(false);
					PXLongOperation.SetCustomInfo(clone);
				});
			}
			return adapter.Get();
		}

		public PXAction<APPayment> reverseApplication;

		[PXUIField(DisplayName = "Reverse Application", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton(ImageKey = PX.Web.UI.Sprite.Main.Refresh)]
		[APMigrationModeDependentActionRestriction(
			restrictInMigrationMode: false,
			restrictForRegularDocumentInMigrationMode: true,
			restrictForUnreleasedMigratedDocumentInNormalMode: true)]
		public virtual IEnumerable ReverseApplication(PXAdapter adapter)
		{
			APPayment payment = Document.Current;
			APAdjust application =  PXSelect<APAdjust,
					Where<APAdjust.noteID, Equal<Current<APTranPostBal.refNoteID>>>>
				.SelectSingleBound(this, new object[]{});;

			if (application == null) return adapter.Get();

			if (application.AdjType == ARAdjust.adjType.Adjusting)
			{
				throw new PXException(
					Common.Messages.IncomingApplicationCannotBeReversed,
					GetLabel.For<APDocType>(payment.DocType),
					GetLabel.For<APDocType>(application.AdjgDocType),
					application.AdjgRefNbr);
			}

			if (application.IsInitialApplication == true)
			{
				throw new PXException(Common.Messages.InitialApplicationCannotBeReversed);
			}
			else if (application.IsMigratedRecord != true &&
				APSetup.Current.MigrationMode == true)
				{
				throw new PXException(Messages.CannotReverseRegularApplicationInMigrationMode);
				}

			if (application.Voided != true 
				&& (
					APPaymentType.CanHaveBalance(application.AdjgDocType) 
					|| application.AdjgDocType == APDocType.Refund 
					|| application.AdjgDocType == APDocType.Check))
			{

				if (payment != null
					&& (payment.DocType != APDocType.DebitAdj || payment.PendingPPD != true)
					&& application.AdjdHasPPDTaxes == true
					&& application.PendingPPD != true)
				{
					APAdjust adjPPD = GetPPDApplication(this, application.AdjdDocType, application.AdjdRefNbr);

					if (adjPPD != null)
					{
						throw new PXSetPropertyException(Messages.PPDApplicationExists, adjPPD.AdjgRefNbr);
					}
				}
				if (payment.OpenDoc != true)
				{
					payment.OpenDoc = true;
					Document.Cache.RaiseRowSelected(payment);
				}

				APAdjust adj = PXCache<APAdjust>.CreateCopy(application);

				adj.Voided = true;
				adj.VoidAdjNbr = adj.AdjNbr;
				adj.Released = false;
				Adjustments.Cache.SetDefaultExt<APAdjust.isMigratedRecord>(adj);
				adj.AdjNbr = payment.AdjCntr;
				adj.AdjBatchNbr = null;
				adj.AdjgDocDate = payment.AdjDate;

				APAdjust adjnew = new APAdjust
				{
					AdjgDocType = adj.AdjgDocType,
					AdjgRefNbr = adj.AdjgRefNbr,
					AdjgBranchID = adj.AdjgBranchID,
					AdjdDocType = adj.AdjdDocType,
					AdjdRefNbr = adj.AdjdRefNbr,
					AdjdLineNbr = adj.AdjdLineNbr,
					AdjdBranchID = adj.AdjdBranchID,
					VendorID = adj.VendorID,
					AdjNbr = adj.AdjNbr,
					AdjdCuryInfoID = adj.AdjdCuryInfoID,
					AdjdHasPPDTaxes = adj.AdjdHasPPDTaxes,
					JointPayeeID = adj.JointPayeeID
				};

				try
				{
				AutoPaymentApp = true;
				IsReverseProc = true;
				adjnew = Adjustments.Insert(adjnew);

					if (adjnew != null)
				{
				adj.CuryAdjgAmt = -1 * adj.CuryAdjgAmt;
				adj.CuryAdjgDiscAmt = -1 * adj.CuryAdjgDiscAmt;
				adj.CuryAdjgPPDAmt = -1 * adj.CuryAdjgPPDAmt;
				adj.CuryAdjgWhTaxAmt = -1 * adj.CuryAdjgWhTaxAmt;
				adj.AdjAmt = -1 * adj.AdjAmt;
				adj.AdjDiscAmt = -1 * adj.AdjDiscAmt;
				adj.AdjPPDAmt = -1 * adj.AdjPPDAmt;
				adj.AdjWhTaxAmt = -1 * adj.AdjWhTaxAmt;
				adj.CuryAdjdAmt = -1 * adj.CuryAdjdAmt;
				adj.CuryAdjdDiscAmt = -1 * adj.CuryAdjdDiscAmt;
				adj.CuryAdjdPPDAmt = -1 * adj.CuryAdjdPPDAmt;
				adj.CuryAdjdWhTaxAmt = -1 * adj.CuryAdjdWhTaxAmt;
				adj.RGOLAmt = -1 * adj.RGOLAmt;
				adj.AdjgCuryInfoID = payment.CuryInfoID;

						Adjustments.Cache.SetDefaultExt<APAdjust.noteID>(adj);
				Adjustments.Update(adj);
				FinPeriodIDAttribute.SetPeriodsByMaster<APAdjust.adjgFinPeriodID>(this.Adjustments.Cache, adjnew, this.Document.Current.AdjTranPeriodID);
					}
				}
				finally
				{
				AutoPaymentApp = false;
				IsReverseProc = false;
			}
			}

			return adapter.Get();
		}
		public static APAdjust GetPPDApplication(PXGraph graph, string DocType, string RefNbr)
		{
			return PXSelect<APAdjust, Where<APAdjust.adjdDocType, Equal<Required<APAdjust.adjdDocType>>,
				And<APAdjust.adjdRefNbr, Equal<Required<APAdjust.adjdRefNbr>>,
				And<APAdjust.released, Equal<True>,
				And<APAdjust.voided, NotEqual<True>,
				And<APAdjust.pendingPPD, Equal<True>>>>>>>.SelectSingleBound(graph, null, DocType, RefNbr);
		}

		public PXAction<APPayment> viewDocumentToApply;

		[PXUIField(
			DisplayName = Messages.ViewDocument,
			MapEnableRights = PXCacheRights.Select,
			MapViewRights = PXCacheRights.Select,
			Visible = false)]
		[PXLookupButton]
		public virtual IEnumerable ViewDocumentToApply(PXAdapter adapter)
		{
			APAdjust row = Adjustments.Current;
			if (!string.IsNullOrEmpty(row?.AdjdDocType) && !string.IsNullOrEmpty(row?.AdjdRefNbr))
			{
				if (row.AdjdDocType == APDocType.Check || (row.AdjgDocType == APDocType.Refund && row.AdjdDocType == APDocType.Prepayment))
				{
					APPaymentEntry iegraph = PXGraph.CreateInstance<APPaymentEntry>();
					iegraph.Document.Current = iegraph.Document.Search<APPayment.refNbr>(row.AdjdRefNbr, row.AdjdDocType);
					throw new PXRedirectRequiredException(iegraph, true, "View Document") { Mode = PXBaseRedirectException.WindowMode.NewWindow };
				}
				else
				{
					APInvoiceEntry iegraph = PXGraph.CreateInstance<APInvoiceEntry>();
					iegraph.Document.Current = iegraph.Document.Search<APInvoice.refNbr>(row.AdjdRefNbr, row.AdjdDocType);
					throw new PXRedirectRequiredException(iegraph, true, "View Document") { Mode = PXBaseRedirectException.WindowMode.NewWindow };
				}
			}

			return adapter.Get();
		}

		public PXAction<APPayment> viewApplicationDocument;
		[PXUIField(
			DisplayName = Messages.ViewAppDoc,
			MapEnableRights = PXCacheRights.Select,
			MapViewRights = PXCacheRights.Select,
			Visible = false)]
		[PXLookupButton(Tooltip = Messages.ViewAppDoc)]
		public virtual IEnumerable ViewApplicationDocument(PXAdapter adapter)
		{
			APTranPostBal post = this.APPost.Current;

			if (post == null ||
			    string.IsNullOrEmpty(post.SourceDocType) ||
			    string.IsNullOrEmpty(post.SourceRefNbr))
			{
				return adapter.Get();
			}

					PXGraph redirect;

			switch (post.SourceDocType)
				{
					case APDocType.Check:
					case APDocType.Prepayment:
					case APDocType.Refund:
                    case APDocType.VoidRefund:
                    case APDocType.VoidCheck:
					{
							APPaymentEntry docgraph = CreateInstance<APPaymentEntry>();
					docgraph.Document.Current =
						docgraph.Document.Search<APPayment.refNbr>(post.SourceRefNbr, post.SourceDocType);
							redirect = docgraph;
							break;
						}
						default:
						{
							APInvoiceEntry docgraph = CreateInstance<APInvoiceEntry>();
					docgraph.Document.Current =
						docgraph.Document.Search<APInvoice.refNbr>(post.SourceRefNbr, post.SourceDocType);
							redirect = docgraph;
							break;
						}
					}

			throw new PXRedirectRequiredException(redirect, true, Messages.ViewAppDoc)
				{Mode = PXBaseRedirectException.WindowMode.NewWindow};

		}

		public PXAction<APPayment> validateAddresses;
		[PXUIField(DisplayName = CS.Messages.ValidateAddress, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select, FieldClass = CS.Messages.ValidateAddress)]
		[PXButton]
		[APMigrationModeDependentActionRestriction(
			restrictInMigrationMode: false,
			restrictForRegularDocumentInMigrationMode: true,
			restrictForUnreleasedMigratedDocumentInNormalMode: true)]
		public virtual IEnumerable ValidateAddresses(PXAdapter adapter)
		{
			foreach (APPayment current in adapter.Get<APPayment>())
			{
				if (current != null)
				{
					FindAllImplementations<IAddressValidationHelper>().ValidateAddresses();
				}
				yield return current;
			}
		}

		public PXAction<APPayment> ViewOriginalDocument;

		[PXUIField(Visible = false, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXLookupButton]
		protected virtual IEnumerable viewOriginalDocument(PXAdapter adapter)
		{
			RedirectionToOrigDoc.TryRedirect(Document.Current?.OrigDocType, Document.Current?.OrigRefNbr, Document.Current?.OrigModule);
			return adapter.Get();
		}
		#endregion

		protected virtual void CATran_CashAccountID_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			e.Cancel = true;
		}

		protected virtual void CATran_FinPeriodID_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			e.Cancel = true;
		}

		protected virtual void CATran_TranPeriodID_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			e.Cancel = true;
		}

		protected virtual void CATran_ReferenceID_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			e.Cancel = true;
		}

		protected virtual void CATran_CuryID_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			e.Cancel = true;
		}

		protected virtual void APPayment_CuryID_FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
		}

		protected virtual void APPayment_DocType_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			e.NewValue = APDocType.Check;
		}

		protected virtual Dictionary<Type, Type> CreateApplicationMap(bool invoiceSide)
		{
			if (invoiceSide)
				return new Dictionary<Type, Type>
				{
					{ typeof(APAdjust.displayDocType), typeof(APAdjust.adjdDocType) },
					{ typeof(APAdjust.displayRefNbr), typeof(APAdjust.adjdRefNbr) },
					{ typeof(APAdjust.displayDocDate), typeof(APInvoice.docDate) },
					{ typeof(APAdjust.displayDocDesc), typeof(Standalone.APRegisterAlias.docDesc) },
					{ typeof(APAdjust.displayCuryID), typeof(APInvoice.curyID) },
					{ typeof(APAdjust.displayFinPeriodID), typeof(APInvoice.finPeriodID) },
					{ typeof(APAdjust.displayStatus), typeof(APInvoice.status) },
					{ typeof(APAdjust.displayCuryInfoID), typeof(APInvoice.curyInfoID) },
					{ typeof(APAdjust.displayCuryAmt), typeof(APAdjust.curyAdjgAmt) },
					{ typeof(APAdjust.displayCuryDiscAmt), typeof(APAdjust.curyAdjgDiscAmt) },
					{ typeof(APAdjust.displayCuryWhTaxAmt), typeof(APAdjust.curyAdjgWhTaxAmt) },
					{ typeof(APAdjust.displayCuryPPDAmt), typeof(APAdjust.curyAdjgPPDAmt)},
					{ typeof(APInvoice.docDesc), typeof(Standalone.APRegisterAlias.docDesc) },
				};
			else
			{
				return new Dictionary<Type, Type>
				{
					{ typeof(APAdjust.displayDocType), typeof(APAdjust.adjgDocType)},
					{ typeof(APAdjust.displayRefNbr), typeof(APAdjust.adjgRefNbr)},
					{ typeof(APAdjust.displayDocDate), typeof(APPayment.docDate) },
					{ typeof(APAdjust.displayDocDesc), typeof(Standalone.APRegisterAlias.docDesc) },
					{ typeof(APAdjust.displayCuryID), typeof(APPayment.curyID) },
					{ typeof(APAdjust.displayFinPeriodID), typeof(APPayment.finPeriodID) },
					{ typeof(APAdjust.displayStatus), typeof(APPayment.status) },
					{ typeof(APAdjust.displayCuryInfoID), typeof(APPayment.curyInfoID) },
					{ typeof(APAdjust.displayCuryAmt), typeof(APAdjust.curyAdjdAmt) },
					{ typeof(APAdjust.displayCuryDiscAmt), typeof(APAdjust.curyAdjdDiscAmt) },
					{ typeof(APAdjust.displayCuryWhTaxAmt), typeof(APAdjust.curyAdjdWhTaxAmt) },
					{ typeof(APAdjust.displayCuryPPDAmt), typeof(APAdjust.curyAdjdPPDAmt) },
					{ typeof(APInvoice.docDesc), typeof(Standalone.APRegisterAlias.docDesc) },
				};
			}
		}

		protected virtual IEnumerable adjustments()
		{
			if (balanceCache == null)
				FillBalanceCache(this.Document.Current);
			int startRow = PXView.StartRow;
			int totalRows = 0;
			if (Document.Current == null || (Document.Current.DocType != APDocType.Refund && Document.Current.DocType != APDocType.VoidRefund))
			{
				//Suppress ballance calculation on row_selected event
				PXResultMapper mapper = new PXResultMapper(this,
					CreateApplicationMap(true),
					typeof(APAdjust), typeof(APInvoice), typeof(APTran));
				var ret = mapper.CreateDelegateResult();

				Adjustments_Invoices_BeforeSelect();
				foreach (PXResult<APAdjust, APInvoice, Standalone.APRegisterAlias, APTran> res
					in Adjustments_Invoices.View.Select(
						PXView.Currents,
						null,
						mapper.Searches,
						mapper.SortColumns,
						mapper.Descendings,
						mapper.Filters,
						ref startRow,
						PXView.MaximumRows,
						ref totalRows))
				{
					APInvoice fullInvoice = res;
					APTran tran = res;
					PXCache<APRegister>.RestoreCopy(fullInvoice, (Standalone.APRegisterAlias)res);

					if (Adjustments.Cache.GetStatus((APAdjust)res) == PXEntryStatus.Notchanged)
					{
						GetExtension<APPaymentEntryDocumentExtension>().CalcBalances(res, fullInvoice, true, !TakeDiscAlways, tran);
					}

					ret.Add(mapper.CreateResult(new PXResult<APAdjust, APInvoice, Standalone.APRegisterAlias, APTran>(res, fullInvoice, (Standalone.APRegisterAlias)res, tran)));
				}
				PXView.StartRow = 0;
				return ret;
			}
			else
			{
				PXResultMapper mapper = new PXResultMapper(this,
						CreateApplicationMap(false),
						typeof(APAdjust), typeof(APInvoice));
				var ret = mapper.CreateDelegateResult();
				foreach (PXResult<APAdjust, APPayment, APInvoice, Standalone.APRegisterAlias> res
					in Adjustments_Payments.View.Select(
						PXView.Currents,
						null,
						mapper.Searches,
						mapper.SortColumns,
						mapper.Descendings,
						mapper.Filters,
						ref startRow,
						PXView.MaximumRows,
						ref totalRows))
				{
					APPayment fullPayment = res;
					PXCache<APRegister>.RestoreCopy(fullPayment, (Standalone.APRegisterAlias)res);

					if (Adjustments.Cache.GetStatus((APAdjust)res) == PXEntryStatus.Notchanged)
					{
						GetExtension<APPaymentEntryDocumentExtension>().CalcBalances(res, fullPayment, true, !TakeDiscAlways, null);
					}

					ret.Add(mapper.CreateResult(
						new PXResult<APAdjust, APPayment, Standalone.APRegisterAlias>(res, fullPayment, (Standalone.APRegisterAlias)res)));
				}
				PXView.StartRow = 0;
				return ret;
			}	
		}
		public virtual IEnumerable appost()
		{
			using (var scope = new PXReadBranchRestrictedScope())
			{
				return this.APPost.View.QuickSelect();
			}
		}


		protected virtual IEnumerable adjustments_print()
		{
			if (Document.Current.DocType == APDocType.QuickCheck)
			{
				foreach (PXResult<APAdjust> res in Adjustments_Raw.Select())
				{
					Adjustments.Cache.Delete((APAdjust)res);
				}

				APPayment doc = Document.Current;
				doc.AdjCntr++;

				APAdjust adj = new APAdjust();
				adj.AdjdDocType = doc.DocType;
				adj.AdjdRefNbr = doc.RefNbr;
				adj.AdjdLineNbr = 0;
				adj.AdjdBranchID = doc.BranchID;
				adj.AdjdOrigCuryInfoID = doc.CuryInfoID;
				adj.AdjgCuryInfoID = doc.CuryInfoID;
				adj.AdjdCuryInfoID = doc.CuryInfoID;

				adj = Adjustments.Insert(adj);

				adj.CuryDocBal = doc.CuryOrigDocAmt + doc.CuryOrigDiscAmt + doc.CuryOrigWhTaxAmt;
				adj.CuryDiscBal = doc.CuryOrigDiscAmt;
				adj.CuryWhTaxBal = doc.CuryOrigWhTaxAmt;

				adj = PXCache<APAdjust>.CreateCopy(adj);

				adj.AdjgDocType = doc.DocType;
				adj.AdjgRefNbr = doc.RefNbr;
				adj.AdjdAPAcct = doc.APAccountID;
				adj.AdjdAPSub = doc.APSubID;
				adj.AdjdCuryInfoID = doc.CuryInfoID;
				adj.AdjdDocDate = doc.DocDate;
			    FinPeriodIDAttribute.SetPeriodsByMaster<APAdjust.adjdFinPeriodID>(Adjustments.Cache, adj, doc.TranPeriodID);
				adj.AdjdOrigCuryInfoID = doc.CuryInfoID;
				adj.AdjgCuryInfoID = doc.CuryInfoID;
				adj.AdjgDocDate = doc.DocDate;
			    FinPeriodIDAttribute.SetPeriodsByMaster<APAdjust.adjgFinPeriodID>(Adjustments.Cache, adj, doc.TranPeriodID);
				adj.AdjNbr = doc.AdjCntr;
				adj.AdjAmt = doc.OrigDocAmt;
				adj.AdjDiscAmt = doc.OrigDiscAmt;
				adj.AdjWhTaxAmt = doc.OrigWhTaxAmt;
				adj.RGOLAmt = 0m;
				adj.CuryAdjdAmt = doc.CuryOrigDocAmt;
				adj.CuryAdjdDiscAmt = doc.CuryOrigDiscAmt;
				adj.CuryAdjdWhTaxAmt = doc.CuryOrigWhTaxAmt;
				adj.CuryAdjgAmt = doc.CuryOrigDocAmt;
				adj.CuryAdjgDiscAmt = doc.CuryOrigDiscAmt;
				adj.CuryAdjgWhTaxAmt = doc.CuryOrigWhTaxAmt;
				adj.CuryDocBal = doc.CuryOrigDocAmt + doc.CuryOrigDiscAmt + doc.CuryOrigWhTaxAmt;
				adj.CuryDiscBal = doc.CuryOrigDiscAmt;
				adj.CuryWhTaxBal = doc.CuryOrigWhTaxAmt;
				adj.Released = false;
				adj.VendorID = doc.VendorID;
				Adjustments.Cache.Update(adj);
			}

			return new SelectFrom<APAdjust>
				.Where<APAdjust.adjgDocType.IsEqual<APPayment.docType.AsOptional>
				.And<APAdjust.adjgRefNbr.IsEqual<APPayment.refNbr.AsOptional>>>
				.View(this)
				.SelectMain();
		}

		public override int ExecuteUpdate(string viewName, IDictionary keys, IDictionary values, params object[] parameters)
		{
			if (viewName.ToLower() == "document" && values != null)
			{
				values["CuryApplAmt"] = PXCache.NotSetValue;
				values["CuryUnappliedBal"] = PXCache.NotSetValue;
			}
			return base.ExecuteUpdate(viewName, keys, values, parameters);
		}

		protected bool InternalCall => UnattendedMode;

		public APPaymentEntry()
		{
			APSetup setup = APSetup.Current;
			OpenPeriodAttribute.SetValidatePeriod<APPayment.adjFinPeriodID>(Document.Cache, null, PeriodValidation.DefaultSelectUpdate);

			created = new DocumentList<APPayment>(this);
			OnBeforeCommit += OnBeforeCommitPayment;
		}

		public DocumentList<APPayment> created { get; }
		public Dictionary<long, CurrencyInfo> createdInfo { get; } = new Dictionary<long, CurrencyInfo>();

		public virtual void Segregate(APAdjust adj, CurrencyInfo info, bool? onHold)
		{
			if (IsDirty)
			{
				Save.Press();
			}

			APInvoice apdoc = APInvoice_VendorID_DocType_RefNbr.Select(adj.AdjdLineNbr, adj.VendorID, adj.AdjdDocType, adj.AdjdRefNbr);
			if (apdoc == null)
			{
				throw new AdjustedNotFoundException();
			}

			APPayment payment = FindOrCreatePayment(apdoc, adj);

			if ((adj.SeparateCheck != true || adj.AdjdDocType == APDocType.DebitAdj) && payment.RefNbr != null)
			{
				Document.Current = Document.Search<APPayment.refNbr>(payment.RefNbr, payment.DocType);
				Document.Current.HiddenKey = payment.HiddenKey;
				if (createdInfo.TryGetValue(Document.Current.CuryInfoID.Value, out var paymentInfo))
				{
					FindImplementation<MultiCurrency>()?.StoreResult(paymentInfo);
				}
			}
			else if (adj.AdjdDocType == APDocType.DebitAdj || adj.AdjAmt <0)
			{
				throw new PXSetPropertyException(Messages.DebitAdj_CannotBeApplied, PXErrorLevel.Warning);
			}
			else
			{
				Document.View.Answer = WebDialogResult.No;

				info.CuryInfoID = null;

				info = PXCache<CurrencyInfo>.CreateCopy(this.currencyinfo.Insert(info));

				payment = new APPayment
				{
					CuryInfoID = info.CuryInfoID,
					DocType = APDocType.Check,
					HiddenKey = adj.SeparateCheck == true ? $"{apdoc.DocType}_{apdoc.RefNbr}" : null
				};

				if (onHold.HasValue)
				{
					payment.Hold = onHold;
				}

				payment = PXCache<APPayment>.CreateCopy(Document.Insert(payment));
				FillOutAPPAyment(payment, apdoc, adj);

				payment = PXCache<APPayment>.CreateCopy(Document.Update(payment));

				if (payment.ExtRefNbr == null)
				{
					payment = Document.Update(payment);
				}

				CurrencyInfo b_info = PXSelect<CurrencyInfo, Where<CurrencyInfo.curyInfoID, Equal<Current<APPayment.curyInfoID>>>>.Select(this, null);
				b_info.CuryID = info.CuryID;
				b_info.CuryEffDate = info.CuryEffDate;
				b_info.CuryRateTypeID = info.CuryRateTypeID;
				b_info.CuryRate = info.CuryRate;
				b_info.RecipRate = info.RecipRate;
				b_info.CuryMultDiv = info.CuryMultDiv;
				currencyinfo.Update(b_info);
			}
		}

		protected virtual APPayment FindOrCreatePayment(APInvoice apdoc, APAdjust adj)
		{
			APPayment payment =
				created.Find<APPayment.vendorID, APPayment.vendorLocationID, APPayment.hiddenKey>(apdoc.VendorID, apdoc.PayLocationID, $"{apdoc.DocType}_{apdoc.RefNbr}") ??
				created.Find<APPayment.vendorID, APPayment.vendorLocationID, APPayment.hiddenKey>(apdoc.VendorID, apdoc.PayLocationID, null) ??
				new APPayment();

			return payment;
		}

		protected virtual void FillOutAPPAyment(APPayment payment, APInvoice apdoc, APAdjust adj)
		{
			payment.BranchID = adj.AdjgBranchID;
			payment.VendorID = apdoc.VendorID;
			payment.VendorLocationID = apdoc.PayLocationID;
			payment.AdjDate = adj.AdjgDocDate;
			payment.AdjFinPeriodID = adj.AdjgFinPeriodID;
			payment.AdjTranPeriodID = adj.AdjgTranPeriodID;
			payment.CashAccountID = apdoc.PayAccountID;
			payment.PaymentMethodID = apdoc.PayTypeID;
			payment.DocDesc = apdoc.DocDesc;
		}


		public virtual void Segregate(APAdjust adj, CurrencyInfo info)
		{
			Segregate(adj, info, null);
		}

		public void OnBeforeCommitPayment(PXGraph graph)
		{
			if (!(Document?.Current?.VoidAppl == true || Document?.Current?.DocType == APDocType.QuickCheck))
			{

				APAdjust exceedingAdjustment = new SelectFrom<APAdjust>
				.InnerJoin<Standalone.APRegisterAlias2>
				.On<Standalone.APRegisterAlias2.refNbr.IsEqual<APAdjust.adjdRefNbr>
					.And<Standalone.APRegisterAlias2.docType.IsEqual<APAdjust.adjdDocType>>>
				.Where<
					Exists<Select<APAdjust2,
						Where<APAdjust2.adjdRefNbr.IsEqual<Standalone.APRegisterAlias2.refNbr>
						.And<APAdjust2.adjdDocType.IsEqual<Standalone.APRegisterAlias2.docType>>
						.And<APAdjust2.adjgRefNbr.IsEqual<@P.AsString>>
						.And<APAdjust2.adjgDocType.IsEqual<@P.AsString>>>>>
					.And<APAdjust.released.IsEqual<False>>
					.And<APAdjust.voided.IsEqual<False>>>
				.AggregateTo<
					GroupBy<Standalone.APRegisterAlias2.refNbr>,
					GroupBy<Standalone.APRegisterAlias2.docType>,
					GroupBy<Standalone.APRegisterAlias2.curyDocBal>,
					Sum<APAdjust.curyAdjdAmt>>
				.Having<APAdjust.curyAdjdAmt.Summarized.IsGreater<Standalone.APRegisterAlias2.curyDocBal.Maximized>>
				.View(this)
				.SelectSingle(Document?.Current?.RefNbr, Document?.Current?.DocType);

				if (exceedingAdjustment != null)
				{
					throw new PXException(Messages.UnreleasdApplicationsExceedsDocumentBalance,
						exceedingAdjustment.AdjdRefNbr, GetLabel.For<APDocType>(exceedingAdjustment.AdjdDocType));
				}
			}
		}

		public override void Persist()
		{
			foreach (APPayment doc in Document.Cache.Updated)
			{
				if (doc.OpenDoc == true && 
					((bool?)Document.Cache.GetValueOriginal<APPayment.openDoc>(doc) == false || doc.DocBal == 0 && doc.UnappliedBal == 0 && doc.Released == true && doc.Hold == false) &&
					Adjustments_Raw.SelectSingle(doc.DocType, doc.RefNbr) == null)
				{
					doc.OpenDoc = false;
					Document.Cache.RaiseRowSelected(doc);
				}
			}

			base.Persist();

			if (Document.Current != null)
			{
				APPayment existed = created.Find(Document.Current);

				if (existed == null)
				{
					created.Add(Document.Current);
				}
				else
				{
					Document.Cache.RestoreCopy(existed, Document.Current);
				}
				var currencyInfo = 
					FindImplementation<MultiCurrency>()
					?.GetCurrencyInfo(Document.Current.CuryInfoID);
				if (currencyInfo != null && currencyInfo.CuryInfoID != null)
					createdInfo[currencyInfo.CuryInfoID.Value] = currencyInfo;
			}
		}

		protected virtual void CurrencyInfo_CuryID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (PXAccess.FeatureInstalled<FeaturesSet.multicurrency>() && !string.IsNullOrEmpty(cashaccount.Current?.CuryID))
				{
					e.NewValue = cashaccount.Current.CuryID;
					e.Cancel = true;
				}
		}

		protected virtual void CurrencyInfo_CuryRateTypeID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (PXAccess.FeatureInstalled<FeaturesSet.multicurrency>() && !string.IsNullOrEmpty(cashaccount.Current?.CuryRateTypeID))
				{
					e.NewValue = cashaccount.Current.CuryRateTypeID;
					e.Cancel = true;
				}
		}

		protected virtual void CurrencyInfo_CuryEffDate_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (Document.Cache.Current != null)
			{
				e.NewValue = ((APPayment)Document.Cache.Current).DocDate;
				e.Cancel = true;
			}
		}

		protected virtual void APPayment_VendorID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			vendor.RaiseFieldUpdated(sender, e.Row);

			sender.SetDefaultExt<APInvoice.vendorLocationID>(e.Row);
		}

		protected virtual void APPayment_VendorLocationID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			location.RaiseFieldUpdated(sender, e.Row);

			sender.SetDefaultExt<APPayment.paymentMethodID>(e.Row);
			sender.SetDefaultExt<APPayment.aPAccountID>(e.Row);
			sender.SetDefaultExt<APPayment.aPSubID>(e.Row);

			try
			{
				SharedRecordAttribute.DefaultRecord<APPayment.remitAddressID>(sender, e.Row);
				SharedRecordAttribute.DefaultRecord<APPayment.remitContactID>(sender, e.Row);
			}
			catch (PXFieldValueProcessingException ex)
			{
				ex.ErrorValue = location.Current.LocationCD;
				throw;
			}
		}

		protected virtual void APPayment_ExtRefNbr_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			if (e.Row != null && ( ((APPayment)e.Row).DocType == APDocType.VoidCheck || ((APPayment)e.Row).DocType == APDocType.VoidRefund))
			{
				///avoid webdialog in <see cref=PaymentRefAttribute/> attribute
				e.Cancel = true;
			}
		}

		private static object GetAcctSub<Field>(PXCache cache, object data)
			where Field : IBqlField
		{
			object NewValue = cache.GetValueExt<Field>(data);
			PXFieldState state = NewValue as PXFieldState;
			return state != null ? state.Value : NewValue;
		}

		protected virtual void APPayment_APAccountID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (location.Current != null && e.Row != null)
			{
				e.NewValue = null;
				if (((APPayment)e.Row).DocType == APDocType.Prepayment)
				{
					e.NewValue = GetAcctSub<Vendor.prepaymentAcctID>(vendor.Cache, vendor.Current);
				}
				if (string.IsNullOrEmpty((string)e.NewValue))
				{
					e.NewValue = GetAcctSub<Location.aPAccountID>(location.Cache, location.Current);
				}
			}
		}

		protected virtual void APPayment_APSubID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (location.Current != null && e.Row != null)
			{
				e.NewValue = null;
				if (((APPayment)e.Row).DocType == APDocType.Prepayment)
				{
					e.NewValue = GetAcctSub<Vendor.prepaymentSubID>(vendor.Cache, vendor.Current);
				}
				if (string.IsNullOrEmpty((string)e.NewValue))
				{
					e.NewValue = GetAcctSub<Location.aPSubID>(location.Cache, location.Current);
				}
			}
		}

		protected virtual void APPayment_CashAccountID_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			APPayment payment = (APPayment)e.Row;

			if (payment == null || e.NewValue == null)
				return;

			CashAccount cashAccount = PXSelect<CashAccount, Where<CashAccount.cashAccountID, Equal<Required<CashAccount.cashAccountID>>>>.SelectSingleBound(this, null, e.NewValue);

			if (cashAccount != null)
			{
				foreach (PXResult<APAdjust, APInvoice, Standalone.APRegisterAlias> res in Adjustments_Invoices.Select())
				{
					APAdjust adj = res;
					APInvoice invoice = res;

					PXCache<APRegister>.RestoreCopy(invoice, (Standalone.APRegisterAlias)res);
				}
			}
		}

		protected virtual void APPayment_CashAccountID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			APPayment payment = (APPayment)e.Row;
			cashaccount.RaiseFieldUpdated(sender, e.Row);

			payment.Cleared = false;
			payment.ClearDate = null;

			if ((cashaccount.Current != null) && (cashaccount.Current.Reconcile == false))
			{
				payment.Cleared = true;
				payment.ClearDate = payment.DocDate;
			}

			sender.SetDefaultExt<APPayment.depositAsBatch>(e.Row);
			sender.SetDefaultExt<APPayment.depositAfter>(e.Row);
		}

		protected virtual void APPayment_PaymentMethodID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			paymenttype.RaiseFieldUpdated(sender, e.Row);

			sender.SetDefaultExt<APPayment.cashAccountID>(e.Row);
			sender.SetDefaultExt<APPayment.printCheck>(e.Row);
		}


		protected virtual void APPayment_PrintCheck_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (!IsPrintableDocType(((APPayment)e.Row).DocType))
			{
					e.NewValue = false;
					e.Cancel = true;
			}
		}

		/// <summary>
		/// </summary>
		/// <param name="payment"></param>
		/// <returns></returns>
		protected virtual bool MustPrintCheck(APPayment payment)
		{
			return MustPrintCheck(payment, paymenttype.Current);
		}

		public static bool MustPrintCheck(IPrintCheckControlable payment, PaymentMethod paymentMethod)
			{
			return payment != null
				&& paymentMethod != null
				&& IsPrintableDocType(payment.DocType)
				&& !APDocType.VoidQuickCheck.Equals(payment.DocType)
				&& payment.Printed != true
				&& payment.PrintCheck == true
				&& paymentMethod.PrintOrExport == true;
			}

		public static bool IsCheckReallyPrinted(IPrintCheckControlable payment)
		{
			return payment.Printed == true && payment.PrintCheck == true;
		}

		protected virtual void APPayment_PrintCheck_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			if (MustPrintCheck(e.Row as APPayment))
			{
				sender.SetValueExt<APPayment.extRefNbr>(e.Row, null);
			}
		}

		protected virtual void APPayment_AdjDate_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			if (((APPayment)e.Row).Released != true && ((APPayment)e.Row).VoidAppl != true)
			{
				sender.SetDefaultExt<APPayment.depositAfter>(e.Row);
			}
		}

		protected override List<KeyValuePair<string, List<FieldInfo>>> AdjustApiScript(List<KeyValuePair<string, List<FieldInfo>>> fieldsByView)
		{
			List<KeyValuePair<string, List<FieldInfo>>> script = base.AdjustApiScript(fieldsByView);
			List<FieldInfo> documentViewScript = script.FirstOrDefault(x => x.Key == nameof(Document)).Value;

			if (documentViewScript != null)
			{
				int paymentMethodIDIndex = documentViewScript.FindIndex(x => x.FieldName == nameof(APPayment.PaymentMethodID));
				int extRefNbrIndex = documentViewScript.FindIndex(x => x.FieldName == nameof(APPayment.ExtRefNbr));

				if (paymentMethodIDIndex != -1 && extRefNbrIndex != -1 && paymentMethodIDIndex > extRefNbrIndex)
				{
					documentViewScript.Insert(paymentMethodIDIndex + 1, documentViewScript[extRefNbrIndex]);
					documentViewScript.RemoveAt(extRefNbrIndex);
				}
			}

			return script;
		}

		protected virtual void APPayment_AdjDate_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			APPayment payment = (APPayment) e.Row;

			if (payment.VoidAppl != true)
			{
				if (vendor.Current != null && vendor.Current.Vendor1099 == true)
				{
					string year1099 = ((DateTime)e.NewValue).Year.ToString();
					AP1099Year year = PXSelect<AP1099Year, 
												Where<AP1099Year.finYear, Equal<Required<AP1099Year.finYear>>,
														And<AP1099Year.organizationID, Equal<Required<AP1099Year.organizationID>>>>>
												.Select(this, year1099, PXAccess.GetParentOrganizationID(payment.BranchID));

					if (year != null && year.Status != "N")
					{
						throw new PXSetPropertyException(Messages.AP1099_PaymentDate_NotIn_OpenYear, PXUIFieldAttribute.GetDisplayName<APPayment.adjDate>(sender));
					}
				}
			}

			if (payment.DocType == APDocType.VoidRefund)
			{
				APPayment origRefund = PXSelect<APPayment, Where<
						APPayment.docType, Equal<APDocType.refund>,
						And<APPayment.refNbr, Equal<Current<APPayment.refNbr>>>>>.SelectSingleBound(this, new object[] { e.Row });
				if (origRefund != null && origRefund.DocDate > (DateTime)e.NewValue)
				{
					throw new PXSetPropertyException(CS.Messages.Entry_GE, origRefund.DocDate.Value.ToString("d"));
				}
			}
		}

		protected virtual void APPayment_CuryOrigDocAmt_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			if (!(bool)((APPayment)e.Row).Released)
			{
				sender.SetValueExt<APPayment.curyDocBal>(e.Row, ((APPayment)e.Row).CuryOrigDocAmt);
			}
		}

		protected virtual void APAdjust_RowInserting(PXCache sender, PXRowInsertingEventArgs e)
		{
			APAdjust adjust = (APAdjust)e.Row;

			string adjdRefNbrErrMsg = PXUIFieldAttribute.GetError<APAdjust.adjdRefNbr>(sender, e.Row);
			string lineNbrErrMsg = PXUIFieldAttribute.GetError<APAdjust.adjdLineNbr>(sender, e.Row);

			e.Cancel =
				adjust.AdjdRefNbr == null ||
				adjust.AdjdLineNbr == null ||
				!string.IsNullOrEmpty(adjdRefNbrErrMsg) ||
				!string.IsNullOrEmpty(lineNbrErrMsg) ||
				Document.Current?.PaymentsByLinesAllowed == true;
		}

		protected virtual void APAdjust_RowDeleted(PXCache sender, PXRowDeletedEventArgs e)
		{
			//Prepayment requests have Check CuryInfoID in AdjdCuryInfoID should not be deleted
			if (((APAdjust)e.Row).AdjdCuryInfoID != ((APAdjust)e.Row).AdjgCuryInfoID && ((APAdjust)e.Row).AdjdCuryInfoID != ((APAdjust)e.Row).AdjdOrigCuryInfoID && ((APAdjust)e.Row).VoidAdjNbr == null)
			{
				foreach (CurrencyInfo info in CurrencyInfo_CuryInfoID.Select(((APAdjust)e.Row).AdjdCuryInfoID))
				{
					currencyinfo.Delete(info);
				}
			}
		}

		public virtual void APAdjust_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			APAdjust doc = (APAdjust)e.Row;

			if ((e.Operation & PXDBOperation.Command) == PXDBOperation.Delete)
			{
				return;
			}

			if (doc.Released != true && doc.AdjNbr < Document.Current?.AdjCntr)
			{
				sender.RaiseExceptionHandling<APAdjust.adjdRefNbr>(doc, doc.AdjdRefNbr, new PXSetPropertyException(AR.Messages.ApplicationStateInvalid));
			}

			if (((DateTime)doc.AdjdDocDate).CompareTo((DateTime)Document.Current.AdjDate) > 0 &&
				(doc.AdjgDocType != APDocType.Check && doc.AdjgDocType != APDocType.VoidCheck && doc.AdjgDocType != APDocType.Prepayment || APSetup.Current.EarlyChecks == false))
			{
				if (sender.RaiseExceptionHandling<APAdjust.adjdRefNbr>(e.Row, doc.AdjdRefNbr, new PXSetPropertyException(Messages.ApplDate_Less_DocDate, PXErrorLevel.RowError, PXUIFieldAttribute.GetDisplayName<APPayment.adjDate>(Document.Cache))))
				{
					throw new PXRowPersistingException(PXDataUtils.FieldName<APAdjust.adjdDocDate>(), doc.AdjdDocDate, Messages.ApplDate_Less_DocDate, PXUIFieldAttribute.GetDisplayName<APPayment.adjDate>(Document.Cache));
				}
			}

			if (((string)doc.AdjdTranPeriodID).CompareTo((string)Document.Current.AdjTranPeriodID) > 0 &&
				(doc.AdjgDocType != APDocType.Check && doc.AdjgDocType != APDocType.VoidCheck && doc.AdjgDocType != APDocType.Prepayment || APSetup.Current.EarlyChecks == false))
			{
				if (sender.RaiseExceptionHandling<APAdjust.adjdRefNbr>(e.Row, doc.AdjdRefNbr, new PXSetPropertyException(Messages.ApplPeriod_Less_DocPeriod, PXErrorLevel.RowError, PXUIFieldAttribute.GetDisplayName<APPayment.adjFinPeriodID>(Document.Cache))))
				{
					throw new PXRowPersistingException(PXDataUtils.FieldName<APAdjust.adjdFinPeriodID>(), doc.AdjdFinPeriodID, Messages.ApplPeriod_Less_DocPeriod, PXUIFieldAttribute.GetDisplayName<APPayment.adjFinPeriodID>(Document.Cache));
				}
			}

			if ((e.Operation & PXDBOperation.Command) == PXDBOperation.Insert
				&& !string.IsNullOrEmpty(doc.AdjgFinPeriodID)
			    && AdjgDocTypesToValidateFinPeriod.Contains(doc.AdjgDocType))
			{
				IFinPeriod organizationFinPeriod = FinPeriodRepository.GetByID(doc.AdjgFinPeriodID, PXAccess.GetParentOrganizationID(doc.AdjgBranchID));

				ProcessingResult result = FinPeriodUtils.CanPostToPeriod(organizationFinPeriod, typeof(FinPeriod.aPClosed));

				if (!result.IsSuccess)
				{
					throw new PXRowPersistingException(
						PXDataUtils.FieldName<APAdjust.adjgFinPeriodID>(),
						doc.AdjgFinPeriodID,
						result.GetGeneralMessage(),
						PXUIFieldAttribute.GetDisplayName<APAdjust.adjgFinPeriodID>(sender));
				}
			}

			Sign balanceSign = doc.CuryOrigDocAmt < 0m ? Sign.Minus : Sign.Plus;

			if (doc.CuryDocBal * balanceSign < 0m)
			{
				sender.RaiseExceptionHandling<APAdjust.curyAdjgAmt>(e.Row, doc.CuryAdjgAmt, new PXSetPropertyException(Messages.DocumentBalanceNegative));
			}

			if (doc.AdjgDocType != APDocType.QuickCheck && doc.CuryDiscBal * balanceSign < 0m)
			{
				sender.RaiseExceptionHandling<APAdjust.curyAdjgPPDAmt>(e.Row, doc.CuryAdjgPPDAmt, new PXSetPropertyException(Messages.DocumentBalanceNegative));
			}

			if (doc.AdjgDocType != APDocType.QuickCheck && doc.CuryWhTaxBal < 0m)
			{
				sender.RaiseExceptionHandling<APAdjust.curyAdjgWhTaxAmt>(e.Row, doc.CuryAdjgWhTaxAmt, new PXSetPropertyException(Messages.DocumentBalanceNegative));
			}

			doc.PendingPPD = doc.CuryAdjgPPDAmt != 0m && doc.AdjdHasPPDTaxes == true;
			if (doc.PendingPPD == true && doc.CuryDocBal != 0m && doc.Voided != true)
			{
				sender.RaiseExceptionHandling<APAdjust.curyAdjgPPDAmt>(e.Row, doc.CuryAdjgPPDAmt, new PXSetPropertyException(Messages.PartialPPD));
			}

			if (doc.VoidAdjNbr == null && doc.CuryAdjgAmt * balanceSign < 0m)
			{
				throw new PXSetPropertyException(balanceSign == Sign.Plus
					? CS.Messages.Entry_GE
					: CS.Messages.Entry_LE, 0.ToString());
			}

			if (doc.VoidAdjNbr != null && doc.CuryAdjgAmt * balanceSign > 0m)
			{
				throw new PXSetPropertyException(balanceSign == Sign.Plus
					? CS.Messages.Entry_LE
					: CS.Messages.Entry_GE, 0.ToString());
			}

			var payment = Document.Current;
			if (!InternalCall &&
				payment != null &&
				payment.DocType == APPaymentType.Check &&
				payment.Hold != true &&
				payment.Released != true &&
				payment.Status.IsIn(APDocStatus.PendingPrint, APDocStatus.PendingApproval, APDocStatus.Balanced) &&
				doc.AdjdDocType == APDocType.Prepayment &&
				doc.CuryDocBal != 0m
				)
			{
				Adjustments.Cache.RaiseExceptionHandling<APAdjust.curyDocBal>(doc, doc.CuryDocBal,
					new PXSetPropertyException(Messages.PrepaymentNotPayedFull, doc.AdjdRefNbr));

				throw new PXRowPersistingException(typeof(APPayment.hold).Name, payment.Hold, Messages.PrepaymentNotPayedFull, doc.AdjdRefNbr);
			}

			ValidatePrepaymentAmount(sender, doc);
		}

		protected virtual void _(Events.RowPersisted<APAdjust> e)
		{
			}

		protected virtual void ValidatePrepaymentAmount(PXCache sender, APAdjust doc)
		{
			if (doc?.AdjgDocType.IsIn(APDocType.Check, APDocType.Prepayment) == true &&
				doc?.AdjdDocType == APDocType.Prepayment)
			{
				var prepaymentRequest = new PXSelect<APInvoice,
					Where<APInvoice.docType, Equal<Required<APAdjust.adjdDocType>>,
						And<APInvoice.docType, Equal<APDocType.prepayment>,
						And<APInvoice.refNbr, Equal<Required<APAdjust.adjdRefNbr>>>>>>(this)
						.SelectSingle(doc.AdjdDocType, doc.AdjdRefNbr);

				if (prepaymentRequest != null)
				{
					if (prepaymentRequest.CuryOrigDocAmt != doc.CuryAdjdAmt &&
						prepaymentRequest.CuryOrigDocAmt != -doc.CuryAdjdAmt /* Reverse application */)
						sender.RaiseExceptionHandling<APAdjust.curyAdjgAmt>(doc, doc.CuryAdjdAmt,
							new PXSetPropertyException<APAdjust.curyAdjgAmt>(Messages.CanNotChangeAmountOnPrepaymentRequest));
				}
			}
		}

		/// <summary>
		/// Calc CuryAdjgPPDAmt and CuryAdjgAmt
		/// </summary>
		/// <param name="adj"></param>
		/// <param name="curyUnappliedBal"></param>
		/// <param name="checkBalance">Check that amount of UnappliedBal is greater than 0</param>
		/// <param name="trySelect">Try to alllay this APAdjust</param>
		/// <returns>Changing amount of payment.CuryUnappliedBal</returns>
		public virtual decimal applyAPAdjust(APAdjust adj, decimal curyUnappliedBal, bool checkBalance, bool trySelect)
		{
			decimal oldCuryUnappliedBal = curyUnappliedBal;
			decimal curyDiscBal = GetDiscAmountForAdjustment(adj);
			decimal oldCuryAdjgPPDAmt = adj.CuryAdjgPPDAmt.Value;
			decimal newCuryAdjgPPDAmt = oldCuryAdjgPPDAmt;

			decimal curyDocBal = adj.CuryDocBal.Value;
			decimal oldCuryAdjgAmt = adj.CuryAdjgAmt.Value;
			decimal newCuryAdjgAmt = oldCuryAdjgAmt;
			decimal balSign = adj.AdjgBalSign.Value;
			bool calcBalances = false;

			#region Calculation adjgAmt
			if (trySelect)
			{
				if (curyDocBal != 0m)
				{
					decimal maxAvailableAmt = (Math.Abs(curyDiscBal) <= Math.Abs(curyDocBal)) ? curyDiscBal : curyDocBal;

					if (!checkBalance)
					{
						newCuryAdjgPPDAmt += maxAvailableAmt;
						newCuryAdjgAmt += curyDocBal - maxAvailableAmt;
					}
					else
					{
						if ((curyDocBal * balSign - maxAvailableAmt) <= curyUnappliedBal)
						{
							newCuryAdjgPPDAmt += maxAvailableAmt;
							newCuryAdjgAmt += curyDocBal - maxAvailableAmt;
						}
						else
						{
							newCuryAdjgAmt += curyUnappliedBal > 0m ? curyUnappliedBal : 0m;
						}
					}
				}
			}
			else
			{
				newCuryAdjgAmt = 0m;
				newCuryAdjgPPDAmt = 0m;
			}
			curyUnappliedBal -= (newCuryAdjgAmt - oldCuryAdjgAmt) * balSign;
			#endregion

			#region Set adjgAmt and AdjgPPDAmt
			if (oldCuryAdjgPPDAmt != newCuryAdjgPPDAmt)
			{
				adj.CuryAdjgPPDAmt = newCuryAdjgPPDAmt;
				adj.FillDiscAmts();
				calcBalances = true;
			}
			if (oldCuryAdjgAmt != newCuryAdjgAmt)
			{
				adj.CuryAdjgAmt = newCuryAdjgAmt;
				calcBalances = true;
			}
			if (calcBalances)
			{
				GetExtension<APPaymentEntryDocumentExtension>().CalcBalancesFromAdjustedDocument(adj, true, true);
			}
			#endregion
			adj.Selected = newCuryAdjgAmt != 0m || newCuryAdjgPPDAmt != 0m;


			return (curyUnappliedBal - oldCuryUnappliedBal);
		}

		protected virtual decimal GetDiscAmountForAdjustment(APAdjust adj)
		{
			return (adj.AdjgDocType == APDocType.DebitAdj) ? 0m : adj.CuryDiscBal.Value;
		}

		private bool IsAdjdRefNbrFieldVerifyingDisabled(IAdjustment adj)
		{
			return 
				Document?.Current?.VoidAppl == true ||
				Document?.Current?.DocType == APDocType.QuickCheck ||
				adj?.Voided == true || 
				adj?.Released == true ||
				AutoPaymentApp;
		}

		protected virtual void APAdjust_AdjdRefNbr_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			if (e.Row == null) return;

			APAdjust adj = e.Row as APAdjust;
			e.Cancel = IsAdjdRefNbrFieldVerifyingDisabled(adj);

			if (APSetup.Current.EarlyChecks != true
				&& Document.Current.DocType.IsIn(APDocType.Check, APDocType.VoidCheck, APDocType.Prepayment))
			{
				APInvoice invoice = APInvoice_DocType_RefNbr_Single.SelectSingle(adj.AdjdDocType, e.NewValue);
				if (invoice != null
					&& !(invoice?.DocDate <= Document.Current.AdjDate
					&& (Document.Current.AdjTranPeriodID == null || string.Compare(invoice?.TranPeriodID, Document.Current.AdjTranPeriodID) <= 0)))
				{
					throw new PXSetPropertyException(Messages.PostInFuturePeriod, invoice.RefNbr, invoice.DocDate, FinPeriodIDFormattingAttribute.FormatForError(invoice.FinPeriodID));
				}
		}
		}

		protected virtual void APAdjust_AdjdRefNbr_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			APAdjust adj = e.Row as APAdjust;
			InitApplicationData(adj);
		}

		protected virtual void APAdjust_AdjdLineNbr_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			APAdjust adj = e.Row as APAdjust;

			if (!(e.Cancel = IsAdjdRefNbrFieldVerifyingDisabled(adj)) && 
				e.NewValue == null &&
				!PXAccess.FeatureInstalled<FeaturesSet.paymentsByLines>())
			{
				APRegister invoice = PXSelectorAttribute.Select<APAdjust.adjdRefNbr>(sender, adj) as APRegister;
				if (invoice?.PaymentsByLinesAllowed == true)
				{
					throw new PXSetPropertyException(Messages.PaymentsByLinesCanBePaidOnlyByLines);
				}
			}
		}

		protected virtual void APAdjust_AdjdLineNbr_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			APAdjust adj = e.Row as APAdjust;

			if (adj.AdjdLineNbr != 0 && adj.AdjdLineNbr != null)
			{
				InitApplicationData(adj);
			}
		}

		protected virtual void InitApplicationData(APAdjust adj)
		{
			try
				{
				foreach (PXResult<APInvoice, CurrencyInfo, APTran> res in APInvoice_DocType_RefNbr
					.Select(adj.AdjdLineNbr ?? 0, adj.AdjdDocType, adj.AdjdRefNbr))
					{
					APInvoice invoice = res;
					CurrencyInfo info = res;
					APTran tran = res;
					APAdjust_KeyUpdated(adj, invoice, info, tran);
						return;
					}

				foreach (PXResult<APPayment, CurrencyInfo> res in ARPayment_DocType_RefNbr.Select( adj.AdjdDocType, adj.AdjdRefNbr))
					{
					APPayment payment = res;
					CurrencyInfo info = res;

					APAdjust_KeyUpdated(adj, payment, info);
					}

			}
			catch (PXException ex)
			{
				throw new PXSetPropertyException(ex.Message);
			}
		}

		private void StoreApplicationDataResult(APAdjust adj, PXResult<APInvoice, CurrencyInfo, APTran> res)
		{
			foreach (string f in this.Adjustments.Cache.Keys)
			{
				if (this.Adjustments.Cache.GetValue(adj, f) == null)
				{
					this.Adjustments.Cache.SetDefaultExt(adj, f);
				}
			}
			Adjustments.StoreTailResult(
				new List<object> { new PXResult<APInvoice, APTran>(res, res) },
				new object[] { (APInvoice)res, (APTran)res, adj },
				adj.AdjgDocType, adj.AdjgRefNbr
			);

			APInvoice_VendorID_DocType_RefNbr.StoreResult(
				new List<object> { new PXResult<APInvoice, APTran>(res, res) },
				PXQueryParameters.ExplicitParameters(adj.AdjdLineNbr, adj.VendorID, adj.AdjdDocType, adj.AdjdRefNbr));

			APInvoice_DocType_RefNbr.StoreResult(
				new List<object> { res },
				PXQueryParameters.ExplicitParameters(adj.AdjdLineNbr, adj.VendorID, adj.AdjdDocType, adj.AdjdRefNbr));
		}

		private void APAdjust_KeyUpdated<T>(APAdjust adj, T invoice, CurrencyInfo info, APTran tran = null)
			where T : APRegister, CM.IInvoice, new()
		{
			GetExtension<MultiCurrency>().StoreResult(info);
			CurrencyInfo info_copy = GetExtension<MultiCurrency>().CloneCurrencyInfo(info, Document.Current.DocDate);

			adj.VendorID = invoice.VendorID;
			adj.AdjgDocDate = Document.Current.AdjDate;
			adj.AdjgCuryInfoID = Document.Current.CuryInfoID;
			adj.AdjdCuryInfoID = info_copy.CuryInfoID;
			adj.AdjdOrigCuryInfoID = info.CuryInfoID;
			adj.AdjdBranchID = invoice.BranchID;
			adj.AdjdAPAcct = invoice.APAccountID;
			adj.AdjdAPSub = invoice.APSubID;
			adj.AdjdDocDate = invoice.DocDate;
            FinPeriodIDAttribute.SetPeriodsByMaster<APAdjust.adjdFinPeriodID>(Adjustments.Cache, adj, invoice.TranPeriodID);
			adj.AdjdHasPPDTaxes = invoice.HasPPDTaxes;
			adj.Released = false;
			adj.PendingPPD = false;

			adj.CuryAdjgAmt = 0m;
			adj.CuryAdjgDiscAmt = 0m;
			adj.CuryAdjgPPDAmt = 0m;
			adj.CuryAdjgWhTaxAmt = 0m;
			adj.AdjdCuryID = info.CuryID;

			InitAdjustmentData(adj, invoice, tran);

			GetExtension<APPaymentEntryDocumentExtension>().CalcBalances(adj, invoice, false, true, tran);

			Sign balanceSign = adj.CuryOrigDocAmt < 0m ? Sign.Minus : Sign.Plus;

			if (adj.CuryWhTaxBal >= 0m && adj.CuryDiscBal * balanceSign >= 0m && (adj.CuryDocBal - adj.CuryWhTaxBal - adj.CuryDiscBal) * balanceSign <= 0m)
			{
				//no amount suggestion is possible
				return;
			}

			decimal? CuryApplDiscAmt = (adj.AdjgDocType == APDocType.DebitAdj) ? 0m : adj.CuryDiscBal;
			decimal? CuryApplWhTaxAmt = (adj.AdjgDocType == APDocType.DebitAdj) ? 0m : adj.CuryWhTaxBal;
			decimal? CuryUnappliedBal = Document.Current.CuryUnappliedBal;

			if (Document.Current != null && CuryUnappliedBal > 0m && adj.AdjgBalSign > 0m && CuryUnappliedBal < CuryApplDiscAmt)
			{
				CuryApplDiscAmt = 0m;
			}
			
			adj.CuryAdjgAmt = CalculateApplicationAmount(adj);
			adj.CuryAdjgDiscAmt = CuryApplDiscAmt;
			adj.CuryAdjgPPDAmt = CuryApplDiscAmt;
			adj.CuryAdjgWhTaxAmt = CuryApplWhTaxAmt;

			GetExtension<APPaymentEntryDocumentExtension>().CalcBalances(adj, invoice, true, true, tran);
			PXCache<APAdjust>.SyncModel(adj);
		}

		protected virtual void InitAdjustmentData(APAdjust adj, APRegister invoice, APTran tran)
		{
			//extension point to initialize APAdjustment in extensions.
		}

		protected virtual decimal CalculateApplicationAmount(APAdjust adj)
		{
			decimal? CuryApplDiscAmt = (adj.AdjgDocType == APDocType.DebitAdj) ? 0m : adj.CuryDiscBal;
			decimal? CuryApplWhTaxAmt = (adj.AdjgDocType == APDocType.DebitAdj) ? 0m : adj.CuryWhTaxBal;
			decimal? CuryApplAmt = adj.CuryDocBal - CuryApplWhTaxAmt - CuryApplDiscAmt;
			decimal? CuryUnappliedBal = Document.Current.CuryUnappliedBal;

			Sign balanceSign = adj.CuryOrigDocAmt < 0m ? Sign.Minus : Sign.Plus;
			if (Document.Current != null && (adj.AdjgBalSign < 0m || balanceSign == Sign.Minus))
			{
				if (CuryUnappliedBal < 0m)
				{
					CuryApplAmt = Math.Min((decimal)CuryApplAmt, Math.Abs((decimal)CuryUnappliedBal));
				}
			}
			else if (Document.Current != null && CuryUnappliedBal > 0m && adj.AdjgBalSign > 0m && CuryUnappliedBal < CuryApplDiscAmt)
			{
				CuryApplAmt = CuryUnappliedBal;
				CuryApplDiscAmt = 0m;
			}
			else if (Document.Current != null && CuryUnappliedBal > 0m && adj.AdjgBalSign > 0m)
			{
				CuryApplAmt = Math.Min((decimal)CuryApplAmt, (decimal)CuryUnappliedBal);
			}
			else if (Document.Current != null && CuryUnappliedBal <= 0m && Document.Current.CuryOrigDocAmt > 0)
			{
				CuryApplAmt = 0m;
			}

			return CuryApplAmt.GetValueOrDefault();
		}

		protected virtual void APAdjust_AdjdCuryRate_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			if ((decimal)e.NewValue <= 0m)
			{
				throw new PXSetPropertyException(CS.Messages.Entry_GT, ((int)0).ToString());
			}
		}

		protected void FillPPDAmts(APAdjust adj)
		{
			adj.CuryAdjgPPDAmt = adj.CuryAdjgDiscAmt;
			adj.CuryAdjdPPDAmt = adj.CuryAdjdDiscAmt;
			adj.AdjPPDAmt = adj.AdjDiscAmt;
		}

		protected virtual void APAdjust_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
		{
			APAdjust adj = (APAdjust)e.Row;
			if (adj == null || InternalCall) return;

			bool adjNotReleased = adj.Released == false;
			PXUIFieldAttribute.SetEnabled<APAdjust.adjdDocType>(cache, adj, adj.AdjdRefNbr == null);
			PXUIFieldAttribute.SetEnabled<APAdjust.adjdRefNbr>(cache, adj, adj.AdjdRefNbr == null);
			PXUIFieldAttribute.SetEnabled<APAdjust.adjdLineNbr>(cache, adj, adj.AdjdLineNbr == null);
			PXUIFieldAttribute.SetEnabled<APAdjust.curyAdjgAmt>(cache, adj, adjNotReleased && (adj.Voided != true));
			PXUIFieldAttribute.SetEnabled<APAdjust.curyAdjgPPDAmt>(cache, adj, adjNotReleased && (adj.Voided != true));
			PXUIFieldAttribute.SetEnabled<APAdjust.curyAdjgWhTaxAmt>(cache, adj, adjNotReleased && (adj.Voided != true));
			PXUIFieldAttribute.SetEnabled<APAdjust.adjBatchNbr>(cache, adj, false);
			//
			PXUIFieldAttribute.SetVisible<APAdjust.adjBatchNbr>(cache, adj, !adjNotReleased);

			PXUIFieldAttribute.SetEnabled<APAdjust.adjdCuryRate>(cache, adj, GetExtension<MultiCurrency>().EnableCrossRate(adj));
			if (adj.AdjdLineNbr == 0)
			{
				APRegister inv = GetAdjdInvoiceToVerifyArePaymentsByLineAllowed(adj);

				if (inv != null && inv.Released == true && inv.PaymentsByLinesAllowed == true)
				{
					Adjustments.Cache.RaiseExceptionHandling<APAdjust.adjgRefNbr>(adj, adj.AdjgRefNbr,
						new PXSetPropertyException(Messages.NotDistributedApplicationCannotBeReleasedNoScreenLink, PXErrorLevel.RowWarning));
				}
			}
		}

		private APRegister GetAdjdInvoiceToVerifyArePaymentsByLineAllowed( APAdjust adj)
		{
			if (balanceCache != null && balanceCache.TryGetValue(adj, out var source))
				return PXResult.Unwrap<APInvoice>(source[0]);
			else return
					Caches[typeof(APInvoice)].Cached.Cast<APInvoice>().FirstOrDefault(_ => _.DocType == adj.AdjdDocType && _.RefNbr == adj.AdjdRefNbr)
				  ?? (APRegister)PXSelectorAttribute.Select<APAdjust.adjdRefNbr>(Adjustments.Cache, adj);
		}
		protected virtual void _(Events.RowSelecting<APAdjust2> e)
		{
			if (e.Row == null)
				return;
			APAdjust adjust = e.Row;
			if(adjust.PPDVATAdjRefNbr != null)
			{
				adjust.PPDVATAdjDescription = APDocType.GetDisplayName(adjust.PPDVATAdjDocType) + ", " + adjust.PPDVATAdjRefNbr;
			}
		}

		public virtual void APAdjust_RowUpdating(PXCache sender, PXRowUpdatingEventArgs e)
		{
			APAdjust adj = (APAdjust)e.Row;
			if (adj.Voided == true && !_IsVoidCheckInProgress && !IsReverseProc)
			{
				throw new PXSetPropertyException(ErrorMessages.CantUpdateRecord);
			}
		}
		protected virtual void APAdjust_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			APAdjust adj = (APAdjust)e.Row;
			foreach (APInvoice voucher in APInvoice_VendorID_DocType_RefNbr.Select(adj.AdjdLineNbr, adj.VendorID, adj.AdjdDocType, adj.AdjdRefNbr))
			{
				CM.PaymentEntry.WarnPPDiscount(this, adj.AdjgDocDate, voucher, adj, adj.CuryAdjgPPDAmt);
		}
		}

		public bool TakeDiscAlways = false;

		protected virtual void CurrencyInfo_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			if (!sender.ObjectsEqual<CurrencyInfo.curyID, CurrencyInfo.curyRate, CurrencyInfo.curyMultDiv>(e.Row, e.OldRow))
			{
				CurrencyInfo info = e.Row as CurrencyInfo;
				if (info?.CuryRate == null || info.CuryRate == 0.0m)
				{
					return;
				}
				foreach (APAdjust adj in PXSelect<APAdjust, Where<APAdjust.adjgCuryInfoID, Equal<Required<APAdjust.adjgCuryInfoID>>>>.Select(sender.Graph, ((CurrencyInfo)e.Row).CuryInfoID))
				{
					Adjustments.Cache.MarkUpdated(adj);

					GetExtension<APPaymentEntryDocumentExtension>().CalcBalancesFromAdjustedDocument(adj, true, !TakeDiscAlways);

					if (adj.CuryDocBal < 0m)
					{
						Adjustments.Cache.RaiseExceptionHandling<APAdjust.curyAdjgAmt>(adj, adj.CuryAdjgAmt, new PXSetPropertyException(Messages.DocumentBalanceNegative));
					}

					if (adj.CuryDiscBal < 0m)
					{
						Adjustments.Cache.RaiseExceptionHandling<APAdjust.curyAdjgPPDAmt>(adj, adj.CuryAdjgPPDAmt, new PXSetPropertyException(Messages.DocumentBalanceNegative));
					}

					if (adj.CuryWhTaxBal < 0m)
					{
						Adjustments.Cache.RaiseExceptionHandling<APAdjust.curyAdjgWhTaxAmt>(adj, adj.CuryAdjgWhTaxAmt, new PXSetPropertyException(Messages.DocumentBalanceNegative));
					}
				}
			}
		}

		protected virtual void APPayment_RowDeleting(PXCache sender, PXRowDeletingEventArgs e)
		{
			foreach (APAdjust adj in Adjustments.Cache.Inserted)
			{
				Adjustments.Delete(adj);
			}
		}

		protected virtual void APPayment_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			APPayment doc = (APPayment)e.Row;

			//true for DebitAdj and Prepayment Requests
			if (doc.Released != true && doc.CashAccountID == null
				&& sender.RaiseExceptionHandling<APPayment.cashAccountID>(e.Row, null, new PXSetPropertyException(ErrorMessages.FieldIsEmpty, $"[{nameof(APPayment.cashAccountID)}]")))
				{
					throw new PXRowPersistingException(typeof(APPayment.cashAccountID).Name, null, ErrorMessages.FieldIsEmpty, nameof(APPayment.cashAccountID));
				}

			//true for DebitAdj and Prepayment Requests
			if (doc.Released != true && string.IsNullOrEmpty(doc.PaymentMethodID)
				&& sender.RaiseExceptionHandling<APPayment.paymentMethodID>(e.Row, null, new PXSetPropertyException(ErrorMessages.FieldIsEmpty, $"[{nameof(APPayment.paymentMethodID)}]")))
				{
					throw new PXRowPersistingException(typeof(APPayment.paymentMethodID).Name, null, ErrorMessages.FieldIsEmpty, nameof(APPayment.paymentMethodID));
				}

			if ((doc.DocType == APDocType.Check || doc.DocType == APDocType.Refund || doc.DocType == APDocType.Prepayment) &&
			    doc.CuryOrigDocAmt < 0)
			{
				throw new PXRowPersistingException(typeof(APPayment.curyOrigDocAmt).Name, doc.CuryOrigDocAmt, Messages.DocumentAmountsNegative, GetLabel.For<APDocType>(doc.DocType));
			}
			else if (doc.OpenDoc == true && doc.Hold != true && IsPaymentUnbalanced(doc) && (!APSetup.Current.SuggestPaymentAmount.GetValueOrDefault(false) || doc.CuryOrigDocAmt != 0m))
			{
				throw new PXRowPersistingException(typeof(APPayment.curyOrigDocAmt).Name, doc.CuryOrigDocAmt, Messages.DocumentOutOfBalance);
			}

			PaymentRefAttribute.SetUpdateCashManager<APPayment.extRefNbr>(sender, e.Row, doc.DocType != APDocType.VoidCheck && doc.DocType != APDocType.Refund && doc.DocType != APDocType.VoidRefund);

			string errMsg;
			// VerifyAdjFinPeriodID() compares Payment "Application Period" only with applications, that have been released. Sometimes, this may cause an erorr 
			// during the action, while document is saved and closed (because of Persist() for each action) - this why doc.OpenDoc flag has been used as a criteria.
			if (doc.OpenDoc == true && !VerifyAdjFinPeriodID(doc, doc.AdjFinPeriodID, out errMsg))
			{
				if (sender.RaiseExceptionHandling<APPayment.adjFinPeriodID>(e.Row,
					FinPeriodIDAttribute.FormatForDisplay(doc.AdjFinPeriodID), new PXSetPropertyException(errMsg)))
				{
					throw new PXRowPersistingException(typeof(APPayment.adjFinPeriodID).Name, FinPeriodIDAttribute.FormatForError(doc.AdjFinPeriodID), errMsg);
				}
			}

		    if (APSetup.Current.SuggestPaymentAmount.GetValueOrDefault(false)
				&& doc.Released != true
				&& doc.CuryUnappliedBal.HasValue
				&& doc.CuryUnappliedBal.Value < 0
				&& doc.CuryOrigDocAmt == 0)
		    {
		        doc.CuryOrigDocAmt = doc.CuryApplAmt;
		        doc.CuryUnappliedBal = 0;
		        Document.Cache.RaiseFieldUpdated<APPayment.curyOrigDocAmt>(doc, null);
				object newValue = doc.CuryOrigDocAmt;
				Document.Cache.RaiseFieldVerifying<APPayment.curyOrigDocAmt>(doc, ref newValue);
		    }

		}

		protected virtual void APPayment_RowPersisted(PXCache sender, PXRowPersistedEventArgs e)
		{
			APPayment row = (APPayment)e.Row;
			if (e.Operation == PXDBOperation.Insert && e.TranStatus == PXTranStatus.Open)
			{
				if (row.DocType == APDocType.Check || row.DocType == APDocType.Prepayment)
				{
					string searchDocType = row.DocType == APDocType.Check ? APDocType.Prepayment : APDocType.Check;
					APPayment duplicatePayment = PXSelect<APPayment,
						Where<APPayment.docType, Equal<Required<APPayment.docType>>, And<APPayment.refNbr, Equal<Required<APPayment.refNbr>>>>>
						.Select(this, searchDocType, row.RefNbr);
					APInvoice inv = null;
					if (searchDocType == APDocType.Prepayment)
					{
						inv = PXSelect<APInvoice, Where<APInvoice.docType, Equal<APDocType.prepayment>, And<APInvoice.refNbr, Equal<Required<APPayment.refNbr>>>>>.Select(this, row.RefNbr);
					}
					if (duplicatePayment != null || inv != null)
					{
						// use the standard AutoNumberingAttribute functionality in case of autonumbering
						// throw primary key violation to make AutoNumbering re-create the next number
						// in case of manual numbering throws UI exception 
						var numbering = (Numbering)PXSelectorAttribute.Select<APSetup.checkNumberingID>(this.Caches[typeof(APSetup)], this.Caches[typeof(APSetup)].Current);
						if (numbering?.UserNumbering == true)
						throw new PXRowPersistedException(typeof(APPayment.refNbr).Name, row.RefNbr, Messages.SameRefNbr, searchDocType == APDocType.Check ? Messages.Check : Messages.Prepayment, row.RefNbr);
						else
							throw new PXLockViolationException(typeof(APPayment), PXDBOperation.Insert, new object[] { row.DocType, row.RefNbr });
					}
				}
			}
		}

		protected virtual bool PaymentRefMustBeUnique => PaymentRefAttribute.PaymentRefMustBeUnique(paymenttype.Current);
		/// <summary>
		/// Determines whether the approval is required for the document.
		/// </summary>
		/// <param name="doc">The document for which the check should be performed.</param>
		/// <param name="cache">The cache.</param>
		/// <returns>Returns <c>true</c> if approval is required; otherwise, returns <c>false</c>.</returns>
		public bool IsApprovalRequired(APPayment doc)
		{
			return EPApprovalSettings<APSetupApproval>.ApprovedDocTypes.Contains(doc.DocType);
			
		}
		protected virtual void APPayment_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
		{
			APPayment doc = e.Row as APPayment;

			if (doc == null) return;
			this.release.SetEnabled(true);

			this.Caches[typeof(CurrencyInfo)].AllowUpdate = true;
			this.Caches[typeof(APTranPostBal)].AllowInsert =
			this.Caches[typeof(APTranPostBal)].AllowUpdate =
			this.Caches[typeof(APTranPostBal)].AllowDelete = false;
			this.Caches[typeof(APAdjust2)].AllowInsert =
			this.Caches[typeof(APAdjust2)].AllowUpdate =
			this.Caches[typeof(APAdjust2)].AllowDelete = false;

			bool dontApprove = !IsApprovalRequired(doc);
			if (doc.DontApprove != dontApprove)
			{
				cache.SetValueExt<APPayment.dontApprove>(doc, dontApprove);
			}

			if (InternalCall) return;

			// We need this for correct tabs repainting
			// in migration mode.
			// 
			Adjustments.Cache.AllowSelect =
			APPost.Cache.AllowSelect =
			PaymentCharges.AllowSelect = true;

			if (vendor.Current != null && doc.VendorID != vendor.Current.BAccountID)
			{
				vendor.Current = null;
			}

			if (finperiod.Current != null && !Equals(finperiod.Current.MasterFinPeriodID, doc.AdjTranPeriodID))
			{
				finperiod.Current = null;
			}
			bool docTypeNotDebitAdj = (doc.DocType != APDocType.DebitAdj);
			PXUIFieldAttribute.SetVisible<APPayment.curyID>(cache, doc, PXAccess.FeatureInstalled<FeaturesSet.multicurrency>());
			PXUIFieldAttribute.SetVisible<APPayment.cashAccountID>(cache, doc, docTypeNotDebitAdj);
			PXUIFieldAttribute.SetVisible<APPayment.cleared>(cache, doc, docTypeNotDebitAdj);
			PXUIFieldAttribute.SetVisible<APPayment.clearDate>(cache, doc, docTypeNotDebitAdj);
			PXUIFieldAttribute.SetVisible<APPayment.paymentMethodID>(cache, doc, docTypeNotDebitAdj);
			PXUIFieldAttribute.SetVisible<APPayment.extRefNbr>(cache, doc, docTypeNotDebitAdj);

			bool requireSingleProject = APSetup.Current.RequireSingleProjectPerDocument.GetValueOrDefault();
			PXUIFieldAttribute.SetVisible<APInvoice.projectID>(Caches[typeof(APInvoice)], null, requireSingleProject);
			PXUIFieldAttribute.SetVisible<Standalone.APRegister.projectID>(Caches[typeof(Standalone.APRegister)], null, requireSingleProject);
			PXUIFieldAttribute.SetVisible<APTran.projectID>(Caches[typeof(APTran)], null, !requireSingleProject);
		
			//true for DebitAdj and Prepayment Requests
			bool docReleased = doc.Released == true;
			bool docOnHold = doc.Hold == true;
			bool docOpen = doc.OpenDoc == true;
			bool docVoidAppl = doc.VoidAppl == true;
			bool docReallyPrinted = IsDocReallyPrinted(doc);

			const bool isCuryEnabled = false;
			bool clearEnabled = docOnHold && (cashaccount.Current != null) && (cashaccount.Current.Reconcile == true);
			bool holdAdj = false;

			PXUIFieldAttribute.SetRequired<APPayment.cashAccountID>(cache, !docReleased);
			PXUIFieldAttribute.SetRequired<APPayment.paymentMethodID>(cache, !docReleased);

			PXUIFieldAttribute.SetRequired<APPayment.extRefNbr>(cache, !docReleased && PaymentRefMustBeUnique);

			PaymentRefAttribute.SetUpdateCashManager<APPayment.extRefNbr>(cache, e.Row, doc.DocType != APDocType.VoidCheck && doc.DocType != APDocType.Refund && doc.DocType != APDocType.VoidRefund);


			bool allowDeposit = doc.DocType == APDocType.Refund;
			PXUIFieldAttribute.SetVisible<APPayment.depositAfter>(cache, doc, allowDeposit && (doc.DepositAsBatch == true));
			PXUIFieldAttribute.SetEnabled<APPayment.depositAfter>(cache, doc, false);
			PXUIFieldAttribute.SetRequired<APPayment.depositAfter>(cache, allowDeposit && (doc.DepositAsBatch == true));
			PXDefaultAttribute.SetPersistingCheck<APPayment.depositAfter>(cache, doc, allowDeposit && (doc.DepositAsBatch == true) ? PXPersistingCheck.NullOrBlank : PXPersistingCheck.Nothing);

			validateAddresses.SetEnabled(false);
			if (cache.GetStatus(doc) == PXEntryStatus.Notchanged
				&& doc.Status == APDocStatus.Open
				&& !docVoidAppl
				&& doc.AdjDate != null
				&& ((DateTime)doc.AdjDate).CompareTo((DateTime)Accessinfo.BusinessDate) < 0)
			{
				if (Adjustments_Raw.View.SelectMultiBound(new object[] { e.Row }).Any() == false)
				{
					doc.AdjDate = Accessinfo.BusinessDate;

					FinPeriod adjFinPeriod = FinPeriodRepository.FindFinPeriodByDate(doc.AdjDate, PXAccess.GetParentOrganizationID(doc.BranchID));

				    doc.AdjFinPeriodID = adjFinPeriod?.FinPeriodID;
                    doc.AdjTranPeriodID = adjFinPeriod?.MasterFinPeriodID;
					
					cache.SetStatus(doc, PXEntryStatus.Held);
				}
			}

			bool isReclassified = false;
			bool isViewOnlyRecord = AutoNumberAttribute.IsViewOnlyRecord<APPayment.refNbr>(cache, doc);

			if (isViewOnlyRecord)
			{
				PXUIFieldAttribute.SetEnabled(cache, doc, false);
				cache.AllowUpdate = false;
				cache.AllowDelete = false;
				Adjustments.Cache.SetAllEditPermissions(allowEdit: false);
			}
			else if (docVoidAppl && !docReleased)
			{
				PXUIFieldAttribute.SetEnabled(cache, doc, false);
				PXUIFieldAttribute.SetEnabled<APPayment.adjDate>(cache, doc, true);
				PXUIFieldAttribute.SetEnabled<APPayment.adjFinPeriodID>(cache, doc, true);
				PXUIFieldAttribute.SetEnabled<APPayment.docDesc>(cache, doc, true);
				PXUIFieldAttribute.SetEnabled<APPayment.hold>(cache, doc, true);
				cache.AllowUpdate = true;
				cache.AllowDelete = true;
				Adjustments.Cache.SetAllEditPermissions(allowEdit: false);
			}
			else if (docReleased && docOpen)
			{
				PXResultset<APAdjust> adjustments = Adjustments_Raw.Select();
				int adjCount = adjustments.Count;

				//these to cases do not intersect, no need to evaluate complete
				holdAdj = adjustments.RowCast<APAdjust>()
					.TakeWhile(adj => adj.Voided != true)
					.Any(adj => adj.Hold == true);

				PXUIFieldAttribute.SetEnabled(cache, doc, false);
				PXUIFieldAttribute.SetEnabled<APPayment.adjDate>(cache, doc, !holdAdj);
				PXUIFieldAttribute.SetEnabled<APPayment.adjFinPeriodID>(cache, doc, !holdAdj);
				PXUIFieldAttribute.SetEnabled<APPayment.hold>(cache, doc, !holdAdj);

				cache.AllowDelete = false;
				cache.AllowUpdate = !holdAdj;

				Adjustments.Cache.SetAllEditPermissions(allowEdit: !holdAdj && doc.PaymentsByLinesAllowed != true);

				release.SetEnabled(doc.Hold == false && !holdAdj && adjCount != 0);
			}
			else if (docReleased && !docOpen || docReallyPrinted)
			{
				PXUIFieldAttribute.SetEnabled(cache, doc, false);
				PXUIFieldAttribute.SetEnabled<APPayment.docDesc>(cache, doc, !docReleased);
				PXUIFieldAttribute.SetEnabled<APPayment.extRefNbr>(cache, doc, !docReleased && doc.DocType != APDocType.VoidRefund);
				cache.AllowUpdate = !docReleased;
				cache.AllowDelete = !docReleased && !docReallyPrinted;

				Adjustments.Cache.SetAllEditPermissions(allowEdit: false);

			}
			else if (docVoidAppl)
			{
				PXUIFieldAttribute.SetEnabled(cache, doc, false);
				cache.AllowDelete = false;
				cache.AllowUpdate = false;
				Adjustments.Cache.SetAllEditPermissions(allowEdit: false);
			}
			else
			{
				CATran tran = PXSelect<CATran, Where<CATran.tranID, Equal<Required<CATran.tranID>>>>.Select(this, doc.CATranID);
				isReclassified = tran?.RefTranID != null;
				PXUIFieldAttribute.SetEnabled(cache, doc, true);
				PXUIFieldAttribute.SetEnabled<APPayment.status>(cache, doc, false);
				PXUIFieldAttribute.SetEnabled<APPayment.curyID>(cache, doc, isCuryEnabled);
				PXUIFieldAttribute.SetEnabled<APPayment.printCheck>(
					cache,
					doc,
					!docReallyPrinted && IsPrintableDocType(doc.DocType));


				bool mustPrintCheck = MustPrintCheck(doc);

				PXUIFieldAttribute.SetEnabled<APPayment.extRefNbr>(cache, doc, !mustPrintCheck && !isReclassified && doc.DocType != APDocType.VoidRefund);


				cache.AllowDelete = true;
				cache.AllowUpdate = true;

				Adjustments.Cache.SetAllEditPermissions(allowEdit: true);
				PXUIFieldAttribute.SetEnabled<APPayment.curyOrigDocAmt>(cache, doc, !isReclassified);
				PXUIFieldAttribute.SetEnabled<APPayment.vendorID>(cache, doc, !isReclassified);
			}

			validateAddresses.SetEnabled(!docReleased && FindAllImplementations<IAddressValidationHelper>().RequiresValidation());
			PXUIFieldAttribute.SetEnabled<APPayment.cashAccountID>(cache, doc, !docReleased && !docReallyPrinted && !docVoidAppl && !isReclassified);
			PXUIFieldAttribute.SetEnabled<APPayment.paymentMethodID>(cache, doc, !docReleased && !docReallyPrinted && !docVoidAppl && !isReclassified);
			PXUIFieldAttribute.SetEnabled<APPayment.cleared>(cache, doc, clearEnabled);
			PXUIFieldAttribute.SetEnabled<APPayment.clearDate>(cache, doc, clearEnabled && doc.Cleared == true);
			PXUIFieldAttribute.SetEnabled<APPayment.docType>(cache, doc);
			PXUIFieldAttribute.SetEnabled<APPayment.refNbr>(cache, doc);
			PXUIFieldAttribute.SetEnabled<APPayment.curyUnappliedBal>(cache, doc, false);
			PXUIFieldAttribute.SetEnabled<APPayment.curyApplAmt>(cache, doc, false);
			PXUIFieldAttribute.SetEnabled<APPayment.batchNbr>(cache, doc, false);

			voidCheck.SetEnabled(docReleased && doc.Voided != true && APPaymentType.VoidEnabled(doc.DocType) && !holdAdj);
			loadInvoices.SetEnabled(doc.VendorID != null && docOpen && !holdAdj
				&& (doc.DocType == APDocType.Check || doc.DocType == APDocType.Prepayment || doc.DocType == APDocType.Refund)
				&& !docReallyPrinted);

			SetDocTypeList(e.Row);

			editVendor.SetEnabled(vendor?.Current != null);
			if (doc.VendorID != null)
			{
				if (Adjustments_Raw.Select().Any())
				{
					PXUIFieldAttribute.SetEnabled<APPayment.vendorID>(cache, doc, false);
				}
			}

			if (e.Row != null && ((APPayment)e.Row).CuryApplAmt == null)
			{
				bool IsReadOnly = (cache.GetStatus(e.Row) == PXEntryStatus.Notchanged);
				PXFormulaAttribute.CalcAggregate<APAdjust.curyAdjgAmt>(Adjustments.Cache, e.Row, IsReadOnly);
				cache.RaiseFieldUpdated<APPayment.curyApplAmt>(e.Row, null);
			}

			PXUIFieldAttribute.SetEnabled<APPayment.depositDate>(cache, null, false);
			PXUIFieldAttribute.SetEnabled<APPayment.depositAsBatch>(cache, null, false);
			PXUIFieldAttribute.SetEnabled<APPayment.deposited>(cache, null, false);
			PXUIFieldAttribute.SetEnabled<APPayment.depositNbr>(cache, null, false);

			bool isDeposited = (!string.IsNullOrEmpty(doc.DepositNbr) && !string.IsNullOrEmpty(doc.DepositType));
			CashAccount cashAccount = cashaccount.Current;
			bool isClearingAccount = cashAccount != null && cashAccount.CashAccountID == doc.CashAccountID && cashAccount.ClearingAccount == true;
			bool enableDepositEdit = !isDeposited && cashAccount != null && (isClearingAccount || doc.DepositAsBatch != isClearingAccount);
			if (enableDepositEdit)
			{
				cache.RaiseExceptionHandling<APPayment.depositAsBatch>(doc, doc.DepositAsBatch,
					doc.DepositAsBatch != isClearingAccount
					? new PXSetPropertyException(AR.Messages.DocsDepositAsBatchSettingDoesNotMatchClearingAccountFlag, PXErrorLevel.Warning)
					: null);
			}
			PXUIFieldAttribute.SetEnabled<APPayment.depositAsBatch>(cache, doc, enableDepositEdit);
			PXUIFieldAttribute.SetEnabled<APPayment.depositAfter>(cache, doc, !isDeposited && isClearingAccount && doc.DepositAsBatch == true);
			PaymentCharges.Cache.SetAllEditPermissions(allowEdit: !docReleased && (doc.DocType == APDocType.Check || doc.DocType == APDocType.VoidCheck));

			bool paymentAllowsShowingPrintCheck = IsPrintableDocType(doc.DocType) || paymenttype.Current == null;

			PXUIFieldAttribute.SetVisible<APPayment.printCheck>(cache, doc, paymenttype.Current?.PrintOrExport == true && paymentAllowsShowingPrintCheck);
			reverseApplication.SetEnabled(
				doc.DocType != APDocType.VoidCheck
                 && doc.DocType != APDocType.VoidRefund
                 && doc.Voided != true);

			#region Migration Mode Settings

			bool isMigratedDocument = doc.IsMigratedRecord == true;
			bool isUnreleasedMigratedDocument = isMigratedDocument && !docReleased;
			bool isReleasedMigratedDocument = isMigratedDocument && doc.Released == true;
			bool isCuryInitDocBalEnabled = isUnreleasedMigratedDocument &&
				doc.DocType == APDocType.Prepayment;

			PXUIFieldAttribute.SetVisible<APPayment.curyUnappliedBal>(cache, doc, !isUnreleasedMigratedDocument);
			PXUIFieldAttribute.SetVisible<APPayment.curyInitDocBal>(cache, doc, isUnreleasedMigratedDocument);
			PXUIFieldAttribute.SetVisible<APPayment.displayCuryInitDocBal>(cache, doc, isReleasedMigratedDocument);
			PXUIFieldAttribute.SetEnabled<APPayment.curyInitDocBal>(cache, doc, isCuryInitDocBalEnabled);

			if (isMigratedDocument)
			{
				PXUIFieldAttribute.SetEnabled<APPayment.printCheck>(cache, doc, false);
			}

			if (isUnreleasedMigratedDocument)
			{
				Adjustments.Cache.AllowSelect =
				APPost.Cache.AllowSelect =
				PaymentCharges.AllowSelect = false;
			}

			bool disableCaches = APSetup.Current?.MigrationMode == true
				? !isMigratedDocument
				: isUnreleasedMigratedDocument;
			if (disableCaches)
			{
				bool primaryCacheAllowInsert = Document.Cache.AllowInsert;
				bool primaryCacheAllowDelete = Document.Cache.AllowDelete;
				this.DisableCaches();
				Document.Cache.AllowInsert = primaryCacheAllowInsert;
				Document.Cache.AllowDelete = primaryCacheAllowDelete;
			}

			// We should notify the user that initial balance can be entered,
			// if there are now any errors on this box.
			// 
			if (isCuryInitDocBalEnabled)
			{
				if (string.IsNullOrEmpty(PXUIFieldAttribute.GetError<APPayment.curyInitDocBal>(cache, doc)))
				{
					cache.RaiseExceptionHandling<APPayment.curyInitDocBal>(doc, doc.CuryInitDocBal,
						new PXSetPropertyException(Messages.EnterInitialBalanceForUnreleasedMigratedDocument, PXErrorLevel.Warning));
				}
			}
			else
			{
				cache.RaiseExceptionHandling<APPayment.curyInitDocBal>(doc, doc.CuryInitDocBal, null);
			}
			if (isMigratedDocument)
			{
				cache.SetValue<APPayment.printCheck>(doc, false);
				PXUIFieldAttribute.SetEnabled<APPayment.printCheck>(cache, doc, false);
			}
			#endregion

			CheckForUnreleasedIncomingApplications(cache, doc);

			if (IsApprovalRequired(doc))
			{
				if (doc.DocType == APDocType.Check || doc.DocType == APDocType.Prepayment)
				{
					//AP Check in Pending Approval, Rejected, Printed, Closed and Voided statuses should be disabled for editing.
					//AP Prepayments in Balanced, Pending Approval, Rejected, Closed, Voided  statuses should be disabled for editing.
					if (doc.Status == APDocStatus.PendingApproval
						|| doc.Status == APDocStatus.Rejected
						|| doc.Status == APDocStatus.Closed
						|| doc.Status == APDocStatus.Printed
						|| doc.Status == APDocStatus.Voided
						|| doc.Status == APDocStatus.PendingPrint && doc.DontApprove != true && doc.Approved == true
						|| doc.Status == APDocStatus.Balanced && doc.DontApprove != true && doc.Approved == true)
					{
						PXUIFieldAttribute.SetEnabled(cache, doc, false);

						Adjustments.Cache.AllowInsert = false;
						Approval.Cache.AllowInsert = false;
						PaymentCharges.Cache.AllowInsert = false;

						Adjustments.Cache.AllowUpdate = false;
						Approval.Cache.AllowUpdate = false;
						PaymentCharges.Cache.AllowUpdate = false;
					}
					//In the documents with statuses Balanced, Rejected and Pending Approval, only possibility to change the Hold check box should be available.
					if (doc.Status == APDocStatus.PendingApproval
						|| doc.Status == APDocStatus.Rejected
						|| doc.Status == APDocStatus.Balanced
						|| doc.Status == APDocStatus.PendingPrint
						|| doc.Status == APDocStatus.Hold)
					{
						PXUIFieldAttribute.SetEnabled<APPayment.hold>(cache, doc, true);
					}

					if (doc.Approved == true && doc.Released != true)
					{
						PXUIFieldAttribute.SetEnabled<APPayment.extRefNbr>(cache, doc, true);
					}
				}

				PXUIFieldAttribute.SetEnabled<APPayment.docType>(cache, doc, true);
				PXUIFieldAttribute.SetEnabled<APPayment.refNbr>(cache, doc, true);
			}

			if (doc.DocType == APDocType.DebitAdj && doc.PaymentsByLinesAllowed == true)
			{
				cache.RaiseExceptionHandling<APPayment.refNbr>(doc, doc.RefNbr,
					new PXSetPropertyException(Messages.PayByLineDebitAdjustmentCannotBeUsedAsPayment, PXErrorLevel.Warning, doc.RefNbr));
			}

			if ((doc.Status == APDocStatus.Printed && doc.DocType == APDocType.Check) && doc.DocType != APDocType.VoidRefund)
			{
				PXUIFieldAttribute.SetEnabled<APPayment.extRefNbr>(cache, doc, true);
				PXUIFieldAttribute.SetEnabled<APPayment.docDesc>(cache, doc, true);
			}

			if (doc.DocType == APDocType.VoidCheck)
			{
				PXUIFieldAttribute.SetEnabled<APPayment.extRefNbr>(cache, doc, false);
			}

			if (docReleased && (docOnHold || (bool?)cache.GetValueOriginal<APPayment.hold>(doc) == true))
			{
				PXUIFieldAttribute.SetEnabled<APPayment.hold>(cache, doc, true);
				cache.AllowUpdate = true;
			}

			bool isPrintCheck = doc.PrintCheck == true;
			PXUIFieldAttribute.SetVisible<APAdjust.stubNbr>(Adjustments.Cache, null, isPrintCheck && doc.Status == APDocStatus.Printed);
			PXUIFieldAttribute.SetVisible<APAdjust2.stubNbr>(this.Caches[typeof(APAdjust2)], null, isPrintCheck && docReleased);
		}

		private static bool IsPrintableDocType(string docType)
		{
			return docType != APDocType.VoidCheck
				&& docType != APDocType.Refund
                && docType != APDocType.VoidRefund
				&& docType != APDocType.DebitAdj;
		}

		protected virtual void CheckForUnreleasedIncomingApplications(PXCache sender, APPayment document)
		{
			if (document.Released != true || document.OpenDoc != true)
			{
				return;
			}

			APAdjust unreleasedIncomingApplication = PXSelect<
				APAdjust,
				Where<
					APAdjust.adjdDocType, Equal<Required<APAdjust.adjdDocType>>,
					And<APAdjust.adjdRefNbr, Equal<Required<APAdjust.adjdRefNbr>>,
					And<APAdjust.released, NotEqual<True>>>>>
				.Select(this, document.DocType, document.RefNbr);

			sender.ClearFieldErrors<APPayment.refNbr>(document);

			if (unreleasedIncomingApplication != null)
			{
				sender.DisplayFieldWarning<APPayment.refNbr>(
					document,
					null,
					PXLocalizer.LocalizeFormat(
						Common.Messages.CannotApplyDocumentUnreleasedIncomingApplicationsExist,
						GetLabel.For<APDocType>(unreleasedIncomingApplication.AdjgDocType),
						unreleasedIncomingApplication.AdjgRefNbr));

				Adjustments.Cache.AllowInsert =
				Adjustments.Cache.AllowUpdate =
				Adjustments.Cache.AllowDelete = false;
			}
		}

		public virtual bool IsDocReallyPrinted(APPayment doc)
		{
			return IsCheckReallyPrinted(doc);
		}

		protected Dictionary<APAdjust, PXResultset<APInvoice>> balanceCache;
		protected virtual void FillBalanceCache(APPayment row, bool released = false)
		{
			if (row?.DocType == null || row.RefNbr == null) return;
			
			CurrencyInfo info = GetExtension<MultiCurrency>().GetCurrencyInfo(row.CuryInfoID);
			GetExtension<MultiCurrency>().StoreResult(info);

			if (balanceCache == null)
			{
				balanceCache = new Dictionary<APAdjust, PXResultset<APInvoice>>(
					new RecordKeyComparer<APAdjust>(Adjustments.Cache));
			}

			if (balanceCache.Keys.Any(_ => _.Released == released)) return;

			foreach (PXResult<Standalone.APAdjust, APInvoice, Standalone.APRegisterAlias, APTran, CurrencyInfo, CM.CurrencyInfo2> res
				in Adjustments_Balance.View.SelectMultiBound(new object[] { row }, released))
			{
				Standalone.APAdjust key = res;
				APAdjust adj = new APAdjust
				{
					AdjdDocType = key.AdjdDocType,
					AdjdRefNbr = key.AdjdRefNbr,
					AdjdLineNbr = key.AdjdLineNbr,
					AdjgDocType = key.AdjgDocType,
					AdjgRefNbr = key.AdjgRefNbr,
					AdjNbr = key.AdjNbr
				};
				AddBalanceCache(adj, res);
			}
		}
		protected virtual void AddBalanceCache(APAdjust adj, PXResult res)
		{
			if (balanceCache == null)
			{
				balanceCache = new Dictionary<APAdjust, PXResultset<APInvoice>>(
					new RecordKeyComparer<APAdjust>(Adjustments.Cache));
			}
			var fullInvoice = PXResult.Unwrap<APInvoice>(res);
			var tran = PXResult.Unwrap<APTran>(res);
			var info_copy = PXResult.Unwrap<CurrencyInfo>(res);
			var adjg_info = Common.Utilities.Clone<CM.CurrencyInfo2, CurrencyInfo>
				(this, PXResult.Unwrap<CM.CurrencyInfo2>(res));
			var adjd_info = Common.Utilities.Clone<CM.CurrencyInfo3, CurrencyInfo>
				(this, PXResult.Unwrap<CM.CurrencyInfo3>(res));
			var register = PXResult.Unwrap<Standalone.APRegisterAlias>(res);
			GetExtension<MultiCurrency>().StoreResult(info_copy);
			GetExtension<MultiCurrency>().StoreResult(adjd_info);
			GetExtension<MultiCurrency>().StoreResult(adjg_info);

			if (register != null)
				PXCache<APRegister>.RestoreCopy(fullInvoice, register);

			PXSelectorAttribute.StoreResult<APAdjust.displayRefNbr>(this.Adjustments.Cache, adj, fullInvoice);
			balanceCache[adj] = new PXResultset<APInvoice, APTran>
			{
				new PXResult<APInvoice, APTran>(fullInvoice, tran)
			};
		}
		public virtual void APPayment_RowSelecting(PXCache sender, PXRowSelectingEventArgs e)
		{
			APPayment row = (APPayment) e.Row;
			if (row != null && row.CuryApplAmt == null)
			{
				using (new PXConnectionScope())
				{
					FillBalanceCache(row);
					RecalcApplAmounts(sender, row, true);

				}
			}
		}

		public virtual void RecalcApplAmounts(PXCache sender, APPayment row, bool aReadOnly)
		{
				PXFormulaAttribute.CalcAggregate<APAdjust.curyAdjgAmt>(Adjustments.Cache, row, aReadOnly);
			sender.RaiseFieldUpdated<APPayment.curyApplAmt>(row, null);
		}

		public static void SetDocTypeList(PXCache cache, string docType)
		{
			if (docType == APDocType.Refund || docType == APDocType.VoidRefund)
			{
				PXDefaultAttribute.SetDefault<APAdjust.adjdDocType>(cache, APDocType.DebitAdj);
				PXStringListAttribute.SetList<APAdjust.adjdDocType>(cache, null, new string[] { APDocType.DebitAdj, APDocType.Prepayment }, new string[] { Messages.DebitAdj, Messages.Prepayment });
			}
			else if (docType == APDocType.Prepayment || docType == APDocType.DebitAdj)
			{
				PXDefaultAttribute.SetDefault<APAdjust.adjdDocType>(cache, APDocType.Invoice);
				PXStringListAttribute.SetList<APAdjust.adjdDocType>(cache, null, new string[] { APDocType.Invoice, APDocType.CreditAdj }, new string[] { Messages.Invoice, Messages.CreditAdj });
			}
			else if (docType == APDocType.Check || docType == APDocType.VoidCheck)
			{
				PXDefaultAttribute.SetDefault<APAdjust.adjdDocType>(cache, APDocType.Invoice);
				PXStringListAttribute.SetList<APAdjust.adjdDocType>(cache, null, new string[] { APDocType.Invoice, APDocType.DebitAdj, APDocType.CreditAdj, APDocType.Prepayment }, new string[] { Messages.Invoice, Messages.DebitAdj, Messages.CreditAdj, Messages.Prepayment });
			}
			else
			{
				PXDefaultAttribute.SetDefault<APAdjust.adjdDocType>(cache, APDocType.Invoice);
				PXStringListAttribute.SetList<APAdjust.adjdDocType>(cache, null, new string[] { APDocType.Invoice, APDocType.CreditAdj, APDocType.Prepayment }, new string[] { Messages.Invoice, Messages.CreditAdj, Messages.Prepayment });
			}
		}

		protected virtual void SetDocTypeList(object Row)
		{
			APPayment row = Row as APPayment;
			if (row != null)
			{
				SetDocTypeList(Adjustments.Cache, row.DocType);
			}
		}
		protected virtual void APPayment_Cleared_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			APPayment payment = (APPayment)e.Row;
			if (payment.Cleared == true)
			{
				if (payment.ClearDate == null)
				{
					payment.ClearDate = payment.DocDate;
				}
			}
			else
			{
				payment.ClearDate = null;
			}
		}

		protected virtual void APPayment_DocDate_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (((APPayment)e.Row).Released == false && ((APPayment)e.Row).VoidAppl == false)
			{
				e.NewValue = ((APPayment)e.Row).AdjDate;
				e.Cancel = true;
			}
		}

		protected virtual void APPayment_FinPeriodID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if ((bool)((APPayment)e.Row).Released == false)
			{
				e.NewValue = ((APPayment)e.Row).AdjFinPeriodID;
				e.Cancel = true;
			}
		}

		protected virtual void APPayment_TranPeriodID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if ((bool)((APPayment)e.Row).Released == false)
			{
				e.NewValue = ((APPayment)e.Row).AdjTranPeriodID;
				e.Cancel = true;
			}
		}

		protected virtual void APPayment_DepositAfter_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			APPayment row = (APPayment)e.Row;
			if ((row.DocType == APDocType.Refund)
				&& row.DepositAsBatch == true)
			{
				e.NewValue = row.AdjDate;
				e.Cancel = true;
			}
		}

		protected virtual void APPayment_DepositAsBatch_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			APPayment row = (APPayment)e.Row;
			if ((row.DocType == APDocType.Refund))
			{
				sender.SetDefaultExt<APPayment.depositAfter>(e.Row);
			}
		}


		protected virtual void APPayment_RowInserted(PXCache sender, PXRowInsertedEventArgs e)
		{
			APPayment payment = e.Row as APPayment;
			if (payment != null && payment.Released == false)
			{
				payment.DocDate = payment.AdjDate;
				payment.FinPeriodID = payment.AdjFinPeriodID;
				payment.TranPeriodID = payment.AdjTranPeriodID;

				sender.RaiseExceptionHandling<APPayment.finPeriodID>(e.Row, payment.FinPeriodID, null);
			}
		}
		protected virtual void APPayment_Hold_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			APPayment doc = e.Row as APPayment;
			if (IsApprovalRequired(doc))
			{
				// Acuminator disable once PX1047 RowChangesInEventHandlersForbiddenForArgs [Legacy - requested for approval process working]
					sender.SetValue<APPayment.hold>(doc, true);
				}
			}
		protected virtual void APPayment_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			APPayment row = (APPayment)e.Row;

			if (row.Released != true)
			{
				row.DocDate = row.AdjDate;

				row.FinPeriodID = row.AdjFinPeriodID;
				row.TranPeriodID = row.AdjTranPeriodID;

				sender.RaiseExceptionHandling<APPayment.finPeriodID>(row, row.FinPeriodID, null);

				PaymentCharges.UpdateChangesFromPayment(sender, e);
			}

			if (row.OpenDoc == true && row.Hold != true)
			{
				if ((row.DocType == APDocType.Check || row.DocType == APDocType.Refund || row.DocType == APDocType.Prepayment) &&
					row.CuryOrigDocAmt < 0)
				{
					sender.RaiseExceptionHandling<APPayment.curyOrigDocAmt>(
						row,
						row.CuryOrigDocAmt,
						new PXSetPropertyException(Messages.DocumentAmountsNegative, PXErrorLevel.Error, GetLabel.For<APDocType>(row.DocType)));
				}
				else if (IsPaymentUnbalanced(row))
				{
                    sender.RaiseExceptionHandling<APPayment.curyOrigDocAmt>(row, row.CuryOrigDocAmt, new PXSetPropertyException(Messages.DocumentOutOfBalance, 
                        APSetup.Current.SuggestPaymentAmount.GetValueOrDefault(false) && row.CuryOrigDocAmt == 0m ? PXErrorLevel.Warning : PXErrorLevel.Error));
				}
				else
				{
					sender.RaiseExceptionHandling<APPayment.curyOrigDocAmt>(row, row.CuryOrigDocAmt, null);
				}
			}
			if (this.IsCopyPasteContext)
			{
				sender.SetValue<APPayment.printed>(row, false);
				sender.SetDefaultExt<APPayment.printCheck>(row);
			}
		}

		public virtual bool IsPaymentUnbalanced(APPayment payment)
		{
			// It's should be allowed to enter a Check or 
			// VendorRefund document without any required 
			// applications, when migration mode is activated.
			//
			bool canHaveBalance = payment.CanHaveBalance == true ||
				payment.IsMigratedRecord == true && payment.CuryInitDocBal == 0m;

			return
				canHaveBalance && payment.VoidAppl != true &&
				(payment.CuryUnappliedBal < 0m || payment.CuryApplAmt < 0m && payment.CuryUnappliedBal > payment.CuryOrigDocAmt || payment.CuryOrigDocAmt < 0m) ||
				!canHaveBalance && (payment.CuryUnappliedBal != 0m && payment.CuryUnappliedBal != null);
		}

		public bool IsRequestPrepayment(APPayment apdoc)
		{
			if (apdoc.IsRequestPrepayment == null)
		{
				var select = new PXSelectReadonly<APInvoice,
						Where<APInvoice.docType, Equal<Required<APPayment.docType>>,
							And<APInvoice.refNbr, Equal<Required<APPayment.refNbr>>,
							And<APInvoice.docType, Equal<APDocType.prepayment>>>>>(this);

				using (new PXFieldScope(select.View, typeof(APInvoice.docType), typeof(APInvoice.refNbr)))
					apdoc.IsRequestPrepayment = (select.SelectSingle(apdoc.DocType, apdoc.RefNbr) != null);
			}

			return apdoc.IsRequestPrepayment == true;
		}

		#region BusinessProcs

		public virtual void CreatePayment(APAdjust apdoc, CurrencyInfo info, bool setOffHold)
		{
			Segregate(apdoc, info, setOffHold ? true : (bool?)null);

			APAdjust adj = new APAdjust();
			adj.AdjdDocType = apdoc.AdjdDocType;
			adj.AdjdRefNbr = apdoc.AdjdRefNbr;
			adj.AdjdLineNbr = apdoc.AdjdLineNbr;

			//set origamt to zero to apply "full" amounts to invoices.
			this.Document.Cache.SetValueExt<APPayment.curyOrigDocAmt>(this.Document.Current, 0m);
			
			this.Adjustments.Cache.DisableReadItem = true;
			APInvoice inv = APInvoice_VendorID_DocType_RefNbr
				.Select(apdoc.AdjdLineNbr, apdoc.VendorID, apdoc.AdjdDocType, apdoc.AdjdRefNbr);
			PXSelectorAttribute.StoreCached<APAdjust.adjdRefNbr>(Adjustments.Cache, adj, inv, true);
			
			adj = PXCache<APAdjust>.CreateCopy(this.Adjustments.Insert(adj));

			if (TakeDiscAlways == true)
			{
				adj.CuryAdjgAmt = 0m;
				adj.CuryAdjgDiscAmt = 0m;
				adj.CuryAdjgWhTaxAmt = 0m;

				GetExtension<APPaymentEntryDocumentExtension>().CalcBalancesFromAdjustedDocument(adj, true, !TakeDiscAlways);
				adj = PXCache<APAdjust>.CreateCopy((APAdjust)this.Adjustments.Cache.Update(adj));
			}

			if (apdoc.CuryAdjgAmt != null)
			{
				adj.CuryAdjgAmt = apdoc.CuryAdjgAmt;
				adj = PXCache<APAdjust>.CreateCopy((APAdjust)this.Adjustments.Cache.Update(adj));
			}

			if (apdoc.CuryAdjgDiscAmt != null)
			{
				adj.CuryAdjgDiscAmt = apdoc.CuryAdjgDiscAmt;
				adj = PXCache<APAdjust>.CreateCopy((APAdjust)this.Adjustments.Cache.Update(adj));
			}
			
			if (Document.Current.CuryApplAmt < 0m)
			{
				decimal? amt = adj.CuryAdjgAmt + Math.Sign(adj.CuryAdjgAmt ?? 0) * Document.Current.CuryApplAmt;
				if (amt == null || amt <= 0m)
				{
					Adjustments.Delete(adj);
				}
				else
				{
					adj.CuryAdjgAmt = amt;
					Adjustments.Update(adj);
				}
			}

			decimal? CuryApplAmt = Document.Current.CuryApplAmt;

			APPayment copy = PXCache<APPayment>.CreateCopy(Document.Current);
			

			copy.CuryOrigDocAmt = CuryApplAmt;
			copy.DocDesc = GetPaymentDescription(
				copy.DocDesc, 
				vendor.Current.AcctCD, 
				copy.DocDesc != inv.DocDesc);

			if (setOffHold && copy.Hold == true)
			{
				copy.Hold = false;
			}

			this.Document.Cache.Update(copy);
			this.Save.Press();
			this.Adjustments.Cache.DisableReadItem = false;

			apdoc.AdjgDocType = this.Document.Current.DocType;
			apdoc.AdjgRefNbr = this.Document.Current.RefNbr;
		}
		public virtual void CreatePayment(APAdjust apdoc, CurrencyInfo info)
		{
			CreatePayment(apdoc, info, false);
		}
		protected virtual string GetPaymentDescription(string descr, string vendor, bool multipleAdjust)
		{
			return multipleAdjust
				? String.Format(PXMessages.LocalizeNoPrefix(Messages.PaymentDescr), vendor)
				: descr;
		}

		public virtual void CreatePayment(APInvoice apdoc)
		{
			string paymentType = apdoc.DocType == APDocType.DebitAdj
				? APDocType.Refund
				: APDocType.Check;

			CreatePayment(apdoc, paymentType);
		}

		public virtual void CreatePayment(APInvoice apdoc, string paymentType)
		{
			APPayment payment = Document.Current;

			if
			(payment == null ||
				!object.Equals(payment.VendorID, apdoc.VendorID) ||
				!object.Equals(payment.VendorLocationID, apdoc.PayLocationID) ||
				apdoc.SeparateCheck == true
			)
			{
				this.Clear();

				Location vend = PXSelect<Location, Where<Location.bAccountID, Equal<Required<Location.bAccountID>>,
					And<Location.locationID, Equal<Required<Location.locationID>>>>>.Select(this, apdoc.VendorID, apdoc.PayLocationID);
				if (vend == null)
				{
					throw new PXException(Messages.InternalError, 502);
				}

				if (apdoc.PayTypeID == null)
				{
					apdoc.PayTypeID = vend.PaymentMethodID;
				}

				int? payAccount = apdoc.PayAccountID ?? vend.CashAccountID;
				CashAccount ca = PXSelect<CashAccount, Where<CashAccount.cashAccountID, Equal<Required<CashAccount.cashAccountID>>>>.SelectSingleBound(this, null, payAccount);
				if (ca == null)
				{
					throw new PXException(Messages.VendorMissingCashAccount);
				}

				if (ca.CuryID == apdoc.CuryID)
				{
					apdoc.PayAccountID = payAccount;
				}

				payment = new APPayment();
				payment.DocType = paymentType;

				payment = PXCache<APPayment>.CreateCopy(this.Document.Insert(payment));

				payment.VendorID = apdoc.VendorID;
				payment.VendorLocationID = apdoc.PayLocationID;
				payment.AdjDate = (DateTime.Compare((DateTime)Accessinfo.BusinessDate, (DateTime)apdoc.DocDate) < 0 ? apdoc.DocDate : Accessinfo.BusinessDate);
				payment.CashAccountID = apdoc.PayAccountID;
				payment.CuryID = apdoc.CuryID;
				payment.PaymentMethodID = apdoc.PayTypeID;
				payment.DocDesc = apdoc.DocDesc;

				this.FieldDefaulting.AddHandler<APPayment.cashAccountID>((sender, e) =>
				{
					if (apdoc.DocType == APDocType.Prepayment)
					{
						e.NewValue = null;
						e.Cancel = true;
					}
				});
				this.FieldDefaulting.AddHandler<CurrencyInfo.curyID>((sender, e) =>
				{
					if (e.Row != null)
					{
						e.NewValue = ((CurrencyInfo)e.Row).CuryID;
						e.Cancel = true;
					}
				});

				payment = Document.Update(payment);
			}

			if (APSetup.Current.EarlyChecks != true
				&& payment.DocType.IsIn(APDocType.Check, APDocType.VoidCheck, APDocType.Prepayment)
				&& !(apdoc.DocDate <= payment.AdjDate
				&& (payment.AdjTranPeriodID == null || string.Compare(apdoc.TranPeriodID, payment.AdjTranPeriodID) <= 0)))
			{
				throw new PXException(Messages.PostInFuturePeriod, apdoc.RefNbr, apdoc.DocDate, FinPeriodIDFormattingAttribute.FormatForError(apdoc.FinPeriodID));
			}

			APAdjust adj = new APAdjust();
			adj.AdjdDocType = apdoc.DocType;
			adj.AdjdRefNbr = apdoc.RefNbr;


			//set origamt to zero to apply "full" amounts to invoices.
			Document.SetValueExt<APPayment.curyOrigDocAmt>(payment, 0m);

			try
			{
				if (apdoc.PaymentsByLinesAllowed == true)
				{
					foreach (APTran tran in
						PXSelect<APTran,
							Where<APTran.tranType, Equal<Current<APInvoice.docType>>,
								And<APTran.refNbr, Equal<Current<APInvoice.refNbr>>,
								And<APTran.curyTranBal, NotEqual<Zero>>>>,
							OrderBy<Desc<APTran.curyTranBal>>>
							.SelectMultiBound(this, new object[] {apdoc}))
					{
						APAdjust lineAdj = PXCache<APAdjust>.CreateCopy(adj);
						lineAdj.AdjdLineNbr = tran.LineNbr;
						Adjustments.Insert(lineAdj);
					}
				}
				else
				{
					adj.AdjdLineNbr = 0;
				Adjustments.Insert(adj);
			}
			}
			catch (PXSetPropertyException)
			{
				throw new AdjustedNotFoundException();
			}

			decimal? CuryApplAmt = payment.CuryApplAmt;
			if (CuryApplAmt > 0m)
			{
				this.Document.SetValueExt<APPayment.curyOrigDocAmt>(payment, CuryApplAmt);
				this.Document.Current = this.Document.Update(payment);
			}
		}

		private bool _IsVoidCheckInProgress = false;

		protected virtual void APPayment_RefNbr_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			if (_IsVoidCheckInProgress)
			{
				e.Cancel = true;
			}
		}

		protected virtual void APPayment_FinPeriodID_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			if (_IsVoidCheckInProgress)
			{
				e.Cancel = true;
			}
		}

		protected virtual void APPayment_AdjFinPeriodID_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			if (_IsVoidCheckInProgress)
			{
				e.Cancel = true;
			}

			APPayment doc = (APPayment)e.Row;

			string errMsg;
			if (!VerifyAdjFinPeriodID(doc, (string)e.NewValue, out errMsg))
			{
				e.NewValue = FinPeriodIDAttribute.FormatForDisplay((string)e.NewValue);
				throw new PXSetPropertyException(errMsg);
			}
		}

		protected virtual bool VerifyAdjFinPeriodID(APPayment doc, string newValue, out string errMsg)
		{
			errMsg = null;

			if (doc.Released == true && doc.FinPeriodID.CompareTo(newValue) > 0)
			{
				errMsg = string.Format(CS.Messages.Entry_GE, FinPeriodIDAttribute.FormatForError(doc.FinPeriodID));
				return false;
			}

			if (doc.DocType == APDocType.VoidCheck)
			{
				APPayment orig_payment = PXSelect<APPayment,
					Where2<Where<APPayment.docType, Equal<APDocType.check>,
							Or<APPayment.docType, Equal<APDocType.prepayment>>>,
						And<APPayment.refNbr, Equal<Required<APPayment.refNbr>>>>>
					.SelectSingleBound(this, null, doc.RefNbr);

				if (orig_payment != null && orig_payment.FinPeriodID.CompareTo(newValue) > 0)
				{
					errMsg = string.Format(CS.Messages.Entry_GE, FinPeriodIDAttribute.FormatForError(orig_payment.FinPeriodID));
					return false;
				}
			}
			else
			{
					/// We should find maximal adjusting period of adjusted applications
					/// (excluding applications, that have been reversed in the same period)
					/// for the document, because applications in earlier period are not allowed.
					///
					APAdjust adjdmax = PXSelectJoin<APAdjust,
						LeftJoin<APAdjust2, On<
							APAdjust2.adjdDocType, Equal<APAdjust.adjdDocType>,
							And<APAdjust2.adjdRefNbr, Equal<APAdjust.adjdRefNbr>,
							And<APAdjust2.adjdLineNbr, Equal<APAdjust.adjdLineNbr>,
							And<APAdjust2.adjgDocType, Equal<APAdjust.adjgDocType>,
							And<APAdjust2.adjgRefNbr, Equal<APAdjust.adjgRefNbr>,
							And<APAdjust2.adjNbr, NotEqual<APAdjust.adjNbr>,
							And<Switch<Case<Where<APAdjust.voidAdjNbr, IsNotNull>, APAdjust.voidAdjNbr>, APAdjust.adjNbr>,
								Equal<Switch<Case<Where<APAdjust.voidAdjNbr, IsNotNull>, APAdjust2.adjNbr>, APAdjust2.voidAdjNbr>>,
							And<APAdjust2.adjgTranPeriodID, Equal<APAdjust.adjgTranPeriodID>,
							And<APAdjust2.released, Equal<True>,
							And<APAdjust2.voided, Equal<True>,
							And<APAdjust.voided, Equal<True>>>>>>>>>>>>>,
						Where<APAdjust.adjgDocType, Equal<Required<APAdjust.adjgDocType>>,
							And<APAdjust.adjgRefNbr, Equal<Required<APAdjust.adjgRefNbr>>,
							And<APAdjust.released, Equal<True>,
							And<APAdjust2.adjdRefNbr, IsNull>>>>,
						OrderBy<Desc<APAdjust.adjgTranPeriodID>>>
						.SelectSingleBound(this, null, doc.DocType, doc.RefNbr);

					if (adjdmax?.AdjgFinPeriodID.CompareTo(newValue) > 0)
				{
						errMsg = string.Format(CS.Messages.Entry_GE, FinPeriodIDAttribute.FormatForError(adjdmax.AdjgFinPeriodID));
						return false;
				}
			}

			return true;
		}

		protected virtual void APPayment_EmployeeID_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			if (_IsVoidCheckInProgress)
			{
				e.Cancel = true;
			}
		}

		public virtual void TryToVoidCheck(APPayment doc)
		{
			try
			{
				_IsVoidCheckInProgress = true;
				this.VoidCheckProc(doc);
			}
			catch (PXSetPropertyException)
			{
				this.Clear();
				Document.Current = doc;
				throw;
			}
			finally
			{
				_IsVoidCheckInProgress = false;
			}
		}

		public virtual void VoidCheckProc(APPayment doc)
		{
			this.Clear(PXClearOption.PreserveTimeStamp);
			this.Document.View.Answer = WebDialogResult.No;

			foreach (PXResult<APPayment, CurrencyInfo, Currency, Vendor> res in APPayment_CurrencyInfo_Currency_Vendor.Select(this, doc.DocType, doc.RefNbr, doc.VendorID))
			{
				doc = res;

				CurrencyInfo info = PXCache<CurrencyInfo>.CreateCopy(res);
				info.CuryInfoID = null;
				info.IsReadOnly = false;
				info = PXCache<CurrencyInfo>.CreateCopy(currencyinfo.Insert(info));

				APPayment payment = new APPayment
				{
					DocType = APPaymentType.GetVoidingAPDocType(doc.DocType),
					RefNbr = doc.RefNbr,
					CuryInfoID = info.CuryInfoID,
					VoidAppl = true
				};

				payment = Document.Insert(payment);

				string createdByScreenID = payment.CreatedByScreenID;
				Guid? createdByID = payment.CreatedByID;
				payment = PXCache<APPayment>.CreateCopy(res);
				payment.CreatedByScreenID = createdByScreenID;
				payment.CreatedByID = createdByID;

				payment.CuryInfoID = info.CuryInfoID;
				payment.VoidAppl = true;
				//must set for _RowSelected
				payment.CATranID = null;
				payment.Printed = true;
				payment.OpenDoc = true;
				payment.Released = false;

				//Set original document reference
				payment.OrigDocType = doc.DocType;
				payment.OrigRefNbr = doc.RefNbr;
				payment.OrigModule = BatchModule.AP;

				if (doc.DocType == APDocType.Prepayment || doc.DocType == APDocType.Check)
				{
					payment.ExtRefNbr = doc.ExtRefNbr;
				}

				Document.Cache.SetDefaultExt<APPayment.hold>(payment);
				Document.Cache.SetDefaultExt<APPayment.isMigratedRecord>(payment);
				Document.Cache.SetDefaultExt<APPayment.status>(payment);
				payment.LineCntr = 0;
				payment.AdjCntr = 0;
				payment.BatchNbr = null;
				payment.CuryOrigDocAmt = -1 * payment.CuryOrigDocAmt;
				payment.OrigDocAmt = -1 * payment.OrigDocAmt;
				payment.CuryInitDocBal = -1 * payment.CuryInitDocBal;
				payment.InitDocBal = -1 * payment.InitDocBal;
				payment.CuryChargeAmt = 0;
				payment.CuryApplAmt = null;
				payment.CuryUnappliedBal = null;
				payment.AdjDate = doc.DocDate;
                FinPeriodIDAttribute.SetPeriodsByMaster<APPayment.adjFinPeriodID>(Document.Cache, payment, doc.TranPeriodID);
				
				Document.Cache.SetDefaultExt<APPayment.employeeID>(payment);
				Document.Cache.SetDefaultExt<APPayment.employeeWorkgroupID>(payment);

				if (payment.Cleared == true)
				{
					payment.ClearDate = payment.DocDate;
				}
				else
				{
					payment.ClearDate = null;
				}
				Document.Cache.SetDefaultExt<APPayment.noteID>(payment);
				payment = Document.Update(payment);
				Document.Cache.SetDefaultExt<APPayment.printCheck>(payment);
				Document.Cache.SetValueExt<APPayment.adjFinPeriodID>(payment, FinPeriodIDAttribute.FormatForDisplay(doc.FinPeriodID));

				using (new PX.SM.SuppressWorkflowAutoPersistScope(this))
				{
					initializeState.Press();
				}

				payment = Document.Current;

				if (info != null)
				{
					CurrencyInfo b_info = PXSelect<CurrencyInfo, Where<CurrencyInfo.curyInfoID, Equal<Current<APPayment.curyInfoID>>>>.Select(this, null);
					b_info.CuryID = info.CuryID;
					b_info.CuryEffDate = info.CuryEffDate;
					b_info.CuryRateTypeID = info.CuryRateTypeID;
					b_info.CuryRate = info.CuryRate;
					b_info.RecipRate = info.RecipRate;
					b_info.CuryMultDiv = info.CuryMultDiv;
					currencyinfo.Update(b_info);
				}
			}

			foreach (PXResult<APAdjust, CurrencyInfo> adjres in PXSelectJoin<APAdjust, 
				InnerJoin<CurrencyInfo, On<CurrencyInfo.curyInfoID, Equal<APAdjust.adjdCuryInfoID>>>, 
				Where<APAdjust.adjgDocType, Equal<Required<APAdjust.adjgDocType>>, 
					And<APAdjust.adjgRefNbr, Equal<Required<APAdjust.adjgRefNbr>>, 
					And<APAdjust.voided, NotEqual<True>,
					And<APAdjust.isInitialApplication, NotEqual<True>>>>>>.Select(this, doc.DocType, doc.RefNbr))
			{
				APAdjust adj = PXCache<APAdjust>.CreateCopy((APAdjust)adjres);

				if ((doc.DocType != APDocType.DebitAdj || doc.PendingPPD != true) &&
				adj.AdjdHasPPDTaxes == true &&
				adj.PendingPPD != true)
				{
					APAdjust adjPPD = GetPPDApplication(this, adj.AdjdDocType, adj.AdjdRefNbr);
					if (adjPPD != null && (adjPPD.AdjgDocType != adj.AdjgDocType || adjPPD.AdjgRefNbr != adj.AdjgRefNbr))
					{
						adj = adjres;
						this.Clear();
						adj = (APAdjust)Adjustments.Cache.Update(adj);
						Document.Current = Document.Search<APPayment.refNbr>(adj.AdjgRefNbr, adj.AdjgDocType);
						Adjustments.Cache.RaiseExceptionHandling<APAdjust.adjdRefNbr>(adj, adj.AdjdRefNbr,
							new PXSetPropertyException(Messages.PPDApplicationExists, PXErrorLevel.RowError, adjPPD.AdjgRefNbr));

						throw new PXSetPropertyException(Messages.PPDApplicationExists, adjPPD.AdjgRefNbr);
					}
				}

				adj.VoidAppl = true;
				adj.Released = false;
				Adjustments.Cache.SetDefaultExt<APAdjust.isMigratedRecord>(adj);
				adj.VoidAdjNbr = adj.AdjNbr;
				adj.AdjNbr = 0;
				adj.AdjBatchNbr = null;

				APAdjust adjnew = new APAdjust
				{
					AdjgDocType = adj.AdjgDocType,
					AdjgRefNbr = adj.AdjgRefNbr,
					AdjgBranchID = adj.AdjgBranchID,
					AdjdDocType = adj.AdjdDocType,
					AdjdRefNbr = adj.AdjdRefNbr,
					AdjdLineNbr = adj.AdjdLineNbr,
					AdjdBranchID = adj.AdjdBranchID,
					VendorID = adj.VendorID,
					AdjdCuryInfoID = adj.AdjdCuryInfoID,
					AdjdHasPPDTaxes = adj.AdjdHasPPDTaxes,
				};

				var insertedAdj = this.Adjustments.Insert(adjnew);

				if (insertedAdj == null)
				{
					adj = (APAdjust)adjres;
					this.Clear();
					adj = (APAdjust)Adjustments.Cache.Update(adj);
					Document.Current = Document.Search<APPayment.refNbr>(adj.AdjgRefNbr, adj.AdjgDocType);
					Adjustments.Cache.RaiseExceptionHandling<APAdjust.adjdRefNbr>(adj, adj.AdjdRefNbr, 
						new PXSetPropertyException(Messages.MultipleApplicationError, PXErrorLevel.RowError));

					throw new PXException(Messages.MultipleApplicationError);
				}

				adj.CuryAdjgAmt = -1 * adj.CuryAdjgAmt;
				adj.CuryAdjgDiscAmt = -1 * adj.CuryAdjgDiscAmt;
				adj.CuryAdjgWhTaxAmt = -1 * adj.CuryAdjgWhTaxAmt;
				adj.AdjAmt = -1 * adj.AdjAmt;
				adj.AdjDiscAmt = -1 * adj.AdjDiscAmt;
				adj.AdjWhTaxAmt = -1 * adj.AdjWhTaxAmt;
				adj.CuryAdjdAmt = -1 * adj.CuryAdjdAmt;
				adj.CuryAdjdDiscAmt = -1 * adj.CuryAdjdDiscAmt;
				adj.CuryAdjdWhTaxAmt = -1 * adj.CuryAdjdWhTaxAmt;
				adj.RGOLAmt = -1 * adj.RGOLAmt;
				adj.AdjgCuryInfoID = Document.Current.CuryInfoID;
				adj.CuryAdjgPPDAmt = -1 * adj.CuryAdjgPPDAmt;
				adj.AdjPPDAmt = -1 * adj.AdjPPDAmt;
				adj.CuryAdjdPPDAmt = -1 * adj.CuryAdjdPPDAmt;
				adj.AdjdCuryRate = insertedAdj.AdjdCuryRate;
				Adjustments.Cache.SetDefaultExt<APAdjust.noteID>(adj);

				Adjustments.Update(adj);
			}

			PaymentCharges.ReverseCharges(doc, Document.Current);
		}

		public virtual IEnumerable<AdjustmentStubGroup> GetAdjustmentsPrintList()
		{
			List<AdjustmentStubGroup> result = Adjustments_print.SelectMain()
					.GroupBy(adj => new AdjustmentGroupKey
					{
						Source = AdjustmentGroupKey.AdjustmentType.APAdjustment,
						AdjdDocType = adj.AdjdDocType,
						AdjdRefNbr = adj.AdjdRefNbr,
						AdjdCuryInfoID = adj.AdjdCuryInfoID
					},
						adj => (IAdjustmentStub)adj)
							.Where(g => g.Sum(a => a.CuryAdjgAmt) != 0m)
							.Select(g => new AdjustmentStubGroup() { GroupedStubs = g })
					.ToList();

			if (this.Document.Current.DocType == APDocType.Prepayment && result.Count() == 0)
			{
				var outstandingBalanceRow = GetOutstandingBalanceRow();
				if (outstandingBalanceRow != null)
					result.Add(outstandingBalanceRow);
			}

			return result;
		}

		protected virtual AdjustmentStubGroup GetOutstandingBalanceRow()
		{
			decimal? outstandingBalance = this.Document.Current.CuryUnappliedBal;
							
			if (outstandingBalance != null && outstandingBalance != 0)
			{
				var outstandingBalanceRow = new AP.OutstandingBalanceRow();
				outstandingBalanceRow.CuryOutstandingBalance = outstandingBalance;
				outstandingBalanceRow.OutstandingBalanceDate = this.Accessinfo.BusinessDate;

				var result = new IAdjustmentStub[] { outstandingBalanceRow }.GroupBy(adj =>
					new AdjustmentGroupKey
					{
						Source = AdjustmentGroupKey.AdjustmentType.OutstandingBalance,
						AdjdDocType = APAdjust.EmptyApDocType,
						AdjdRefNbr = string.Empty,
						AdjdCuryInfoID = this.Document.Current.CuryInfoID
					}, adj => adj);

				return new AdjustmentStubGroup() { GroupedStubs = result.Single() };
			}

			return null;
		}

		public virtual void AddCheckDetail(AdjustmentStubGroup adj, string stubNbr)
		{
			var newPrintCheckDetail = GetCheckDetail(adj, stubNbr);
			CheckDetails.Insert(newPrintCheckDetail);
		}

		protected virtual APPrintCheckDetail GetCheckDetail(AdjustmentStubGroup adj, string stubNbr)
		{
			var newPrintCheckDetail = new APPrintCheckDetail()
			{
				AdjgDocType = Document.Current.DocType,
				AdjgRefNbr = Document.Current.RefNbr,
				Source = adj.GroupedStubs.Key.Source,
				AdjdDocType = adj.GroupedStubs.Key.AdjdDocType,
				AdjdRefNbr = adj.GroupedStubs.Key.AdjdRefNbr,
				AdjdCuryInfoID = adj.GroupedStubs.Key.AdjdCuryInfoID,
				AdjgCuryInfoID = Document.Current.CuryInfoID,
				StubNbr = stubNbr,
				CashAccountID = Document.Current.CashAccountID,
				PaymentMethodID = Document.Current.PaymentMethodID,
			};

			var total = adj.GroupedStubs.GroupBy(a => 1).Select(a => new
			{
				TotalCuryAdjgAmt = a.Sum(r => r.CuryAdjgAmt),
				TotalCuryAdjgDiscAmt = a.Sum(r => r.CuryAdjgDiscAmt),
				CuryOutstandingBalance = a.Sum(r => r.CuryOutstandingBalance),
				OutstandingBalanceDate = a.Max(r => r.OutstandingBalanceDate),
				CuryExtraDocBal = a.Sum(r => r.IsRequest == true ? r.CuryAdjgAmt : 0m)
			}).Single();

			newPrintCheckDetail.CuryAdjgAmt = total.TotalCuryAdjgAmt;
			newPrintCheckDetail.CuryAdjgDiscAmt = total.TotalCuryAdjgDiscAmt;
			newPrintCheckDetail.CuryOutstandingBalance = total.CuryOutstandingBalance;
			newPrintCheckDetail.CuryExtraDocBal = total.CuryExtraDocBal;
			newPrintCheckDetail.OutstandingBalanceDate = total.OutstandingBalanceDate;

			return newPrintCheckDetail;
		}

		public virtual void DeletePrintList()
		{
			var checkDetails = new PXSelect<APPrintCheckDetail,
				Where<APPrintCheckDetail.adjgDocType, Equal<Required<APPrintCheckDetail.adjgDocType>>,
					And<APPrintCheckDetail.adjgRefNbr, Equal<Required<APPrintCheckDetail.adjgRefNbr>>>>>(this)
					.Select(Document.Current.DocType, Document.Current.RefNbr);

			checkDetails.ForEach(c => CheckDetails.Delete(c));
		}

		public virtual void RefillAPPrintCheckDetail(APPayment payment)
		{
			Document.Current = payment;

			DeletePrintList();
			var adjustments = GetAdjustmentsPrintList();

			PaymentMethod pt = paymenttype.Current;

			if (pt.APCreateBatchPayment != true)
			{
				string checkNbr = payment.ExtRefNbr;
				short ordinalInStub = 0;

				foreach (var group in adjustments)
				{
					if (ordinalInStub > pt.APStubLines - 1)
					{
						//AssignCheckNumber only for first StubLines in check, other/all lines will be printed on remittance report
						if (pt.APPrintRemittance == true)
						{
							AddCheckDetail(group, null);
							continue;
						}

						ordinalInStub = 0;
						checkNbr = AutoNumberAttribute.NextNumber(checkNbr);
					}

					AddCheckDetail(group, checkNbr);
					ordinalInStub++;
				}
			}
			else
			{
				foreach (var group in adjustments)
					AddCheckDetail(group, null);
			}
		}

		protected virtual void DeleteUnreleasedApplications()
		{
			foreach (APAdjust adj in Adjustments_Raw.Select())
				Adjustments.Cache.Delete(adj);
		}
		#endregion

		#region Address Lookup Extension
		/// <exclude/>
		public class APPaymentEntryAddressLookupExtension : CR.Extensions.AddressLookupExtension<APPaymentEntry, APPayment, APAddress>
		{
			protected override string AddressView => nameof(Base.Remittance_Address);
		}

		public class APPaymentEntryAddressCachingHelper : AddressValidationExtension<APPaymentEntry, APAddress>
		{
			protected override IEnumerable<PXSelectBase<APAddress>> AddressSelects()
			{
				yield return Base.Remittance_Address;
			}
		}
		#endregion
	}
}
