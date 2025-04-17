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
using System.Collections.Generic;
using System.Collections;
using System.Linq;

using PX.Data;
using PX.Data.BQL.Fluent;
using PX.Objects.GL;
using PX.Objects.GL.Attributes;
using PX.Objects.CS;
using PX.Objects.CM;
using PX.Objects.AR.Repositories;
using PX.Objects.AR.CCPaymentProcessing;
using PX.Objects.AR.CCPaymentProcessing.Common;
using PX.Objects.AR.CCPaymentProcessing.Helpers;
using PX.Objects.Extensions.PaymentTransaction;
using PX.Objects.AR.MigrationMode;
using PX.Objects.Common.Attributes;
using PX.Common;
using PX.Objects.AR;

namespace PX.Objects.CA
{
	public class CAPendingReviewEnq : PXGraph<CAPendingReviewEnq>
	{
		#region Internal Types

		[PXHidden]
		[Serializable]
		public partial class ARPaymentInfo : ARPayment
		{
			#region PMInstanceDescr
			public abstract class pMInstanceDescr : PX.Data.BQL.BqlString.Field<pMInstanceDescr> { }
			[PXString(255)]
			[PXUIField(DisplayName = "Card/Account Nbr.", Enabled = false)]
			public virtual String PMInstanceDescr
			{
				get;
				set;
			}
			#endregion
			#region ValidationStatus
			public abstract class validationStatus : PX.Data.BQL.BqlString.Field<validationStatus> { }
			[PXString(255)]
			[PXUIField(DisplayName = "Validation Status")]
			public virtual String ValidationStatus
			{
				get;
				set;
			}
			#endregion
			#region CustomerCD
			public abstract class customerCD : PX.Data.BQL.BqlString.Field<customerCD> { }
			[PXString(255)]
			public virtual string CustomerCD
			{
				get;
				set;
			}
			#endregion
			#region IsOpenedForReview
			public abstract class isOpenedForReview : PX.Data.BQL.BqlBool.Field<isOpenedForReview> { }
			[PXBool]
			public virtual Boolean? IsOpenedForReview
			{
				get;
				set;
			}
			#endregion
			#region BranchID
			public new abstract class branchID : PX.Data.BQL.BqlInt.Field<branchID> { }
			#endregion
		}

		#endregion

		#region Type Override events

		#region BranchID

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXUIField(DisplayName = "Branch", Visible = false, Required = false)]
		[PXUIVisible(typeof(FeatureInstalled<FeaturesSet.branch>.Or<FeatureInstalled<FeaturesSet.multiCompany>>))]
		protected virtual void _(Events.CacheAttached<ARPaymentInfo.branchID> e) { }
		#endregion

		#endregion

		#region Views/Selects
		[PXFilterable]
		public PXSelectJoin<ARPaymentInfo, LeftJoin<ExternalTransaction,
				On<ARPaymentInfo.cCActualExternalTransactionID, Equal<ExternalTransaction.transactionID>>>> Documents;
		protected virtual IEnumerable documents()
		{
			var query = new PXSelectJoin<ARPaymentInfo,
						InnerJoin<Customer, On<Customer.bAccountID, Equal<ARPaymentInfo.customerID>>,
						LeftJoin<CustomerPaymentMethod, On<CustomerPaymentMethod.pMInstanceID, Equal<ARPaymentInfo.pMInstanceID>>,
						LeftJoin<ExternalTransaction, On<ExternalTransaction.transactionID, Equal<ARPaymentInfo.cCActualExternalTransactionID>>>>>,
						Where<ARPaymentInfo.isCCUserAttention, Equal<True>,
							And<ARPaymentInfo.hold, Equal<False>,
							And<ARPaymentInfo.status, NotIn3<ARDocStatus.voided, ARDocStatus.closed>,
							And<ARPaymentInfo.isMigratedRecord, NotEqual<True>,
							And<Match<Customer, Current<AccessInfo.userName>>>>>>>,
						OrderBy<Asc<ARPaymentInfo.refNbr>>>(this);

			foreach (PXResult<ARPaymentInfo, Customer, CustomerPaymentMethod, ExternalTransaction> result in query.SelectWithViewContext())
			{
				ARPaymentInfo doc = (ARPaymentInfo)result;
				Customer customer = (Customer)result;
				CustomerPaymentMethod cpm = (CustomerPaymentMethod)result;
				ExternalTransaction extTran = (ExternalTransaction)result;

				if (cpm?.PMInstanceID != null)
				{
					doc.PMInstanceDescr = cpm.Descr;
				}
				doc.CustomerCD = customer?.AcctCD;
				
				ExternalTransactionState paymentState = new ExternalTransactionState();
				if (extTran?.TransactionID != null)
				{
					paymentState = ExternalTranHelper.GetTransactionState(this, extTran);
				}
				doc.CCPaymentStateDescr = paymentState.Description;
				doc.IsOpenedForReview = paymentState.IsOpenForReview;

				if(!string.IsNullOrEmpty(extTran?.SyncStatus))
				{
					doc.ValidationStatus = extTran.SyncMessage;
				}

				yield return doc;
			}
			PXView.StartRow = 0;
		}

		#endregion

		#region Buttons
		public PXAction<ARPaymentInfo> RedirectToDoc;
		[PXUIField(DisplayName = "", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select, Visible = false)]
		[PXEditDetailButton]
		public virtual IEnumerable redirectToDoc(PXAdapter adapter)
		{
			var doc = this.Documents.Current;

			if(doc.DocType == ARPaymentType.CashSale || doc.DocType == ARPaymentType.CashReturn)
			{
				ARCashSaleEntry pe = PXGraph.CreateInstance<ARCashSaleEntry>();
				pe.Document.Current = pe.Document.Search<ARPaymentInfo.refNbr>(doc.RefNbr, doc.DocType);
				throw new PXRedirectRequiredException(pe, true, AR.Messages.ARPayment) { Mode = PXRedirectRequiredException.WindowMode.NewWindow };
			}
			else
			{
				ARPaymentEntry pe = PXGraph.CreateInstance<ARPaymentEntry>();
				pe.Document.Current = pe.Document.Search<ARPaymentInfo.refNbr>(doc.RefNbr, doc.DocType);
				throw new PXRedirectRequiredException(pe, true, AR.Messages.ARPayment) { Mode = PXRedirectRequiredException.WindowMode.NewWindow };
			}
		}


		public PXAction<ARPaymentInfo> RedirectToProcCenter;
		[PXUIField(DisplayName = "", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select, Visible = false)]
		[PXEditDetailButton]
		public virtual IEnumerable redirectToProcCenter(PXAdapter adapter)
		{
			ARPayment doc = this.Documents.Current;

			CCProcessingCenterMaint graph = PXGraph.CreateInstance<CCProcessingCenterMaint>();
			graph.ProcessingCenter.Current = graph.ProcessingCenter.Search<CCProcessingCenter.processingCenterID>(doc.ProcessingCenterID);
			throw new PXRedirectRequiredException(graph, true, AR.Messages.ViewProcessingCenter) { Mode = PXRedirectRequiredException.WindowMode.NewWindow };
		}

		public PXAction<ARPaymentInfo> RedirectToPaymentMethod;
		[PXUIField(DisplayName = "", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select, Visible = false)]
		[PXEditDetailButton]
		public virtual IEnumerable redirectToPaymentMethod(PXAdapter adapter)
		{
			ARPaymentInfo doc = this.Documents.Current;

			if (string.IsNullOrEmpty(doc.PMInstanceDescr))
			{
				PaymentMethodMaint graphPM = PXGraph.CreateInstance<PaymentMethodMaint>();
				graphPM.PaymentMethod.Current = graphPM.PaymentMethod.Search<PaymentMethod.paymentMethodID>(doc.PaymentMethodID);
				throw new PXRedirectRequiredException(graphPM, true, CA.Messages.PaymentMethod) { Mode = PXRedirectRequiredException.WindowMode.NewWindow };
			}
			else
			{
				CustomerPaymentMethodMaint graphCPM = PXGraph.CreateInstance<CustomerPaymentMethodMaint>();
				graphCPM.CustomerPaymentMethod.Current = graphCPM.CustomerPaymentMethod.Search<CustomerPaymentMethod.pMInstanceID>(doc.PMInstanceID, doc.CustomerCD);
				throw new PXRedirectRequiredException(graphCPM, true, AR.Messages.CustomerPaymentMethodInfo) { Mode = PXRedirectRequiredException.WindowMode.NewWindow };
			}
		}

		public PXAction RedirectToCustomer;

		[PXSuppressActionValidation]
		[PXUIField(DisplayName = "", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select, Visible = false)]
		[PXEditDetailButton]
		public virtual IEnumerable redirectToCustomer(PXAdapter adapter)
		{
			ARPaymentInfo doc = this.Documents.Current;

			CustomerMaint graph = PXGraph.CreateInstance<CustomerMaint>();
			graph.BAccount.Current = graph.BAccount.Search<Customer.bAccountID>(doc.CustomerID);
			throw new PXRedirectRequiredException(graph, true, CR.Messages.BAccount) { Mode = PXRedirectRequiredException.WindowMode.NewWindow };
		}

		#endregion
		#region Handlers

		protected virtual void ARPaymentInfo_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			Documents.AllowUpdate = false;
			Documents.AllowInsert = false;
			Documents.AllowDelete = false;

			ARPaymentInfo doc = e.Row as ARPaymentInfo;
			if(doc?.IsOpenedForReview == true)
			{
				sender.RaiseExceptionHandling<ARPaymentInfo.cCPaymentStateDescr>(doc, doc.CCPaymentStateDescr,
						new PXSetPropertyException(AR.Messages.CCProcessingTranHeldWarning, PXErrorLevel.RowWarning));
			}
		}

		#endregion
	}
}
