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
using PX.Common;
using PX.Data;
using PX.Objects.GL;
using PX.Objects.CM;
using PX.Objects.CA;
using PX.Objects.CS;

namespace PX.Objects.AP
{
	[TableAndChartDashboardType]
	public class APReleaseChecks : PXGraph<APReleaseChecks>
	{
		public PXFilter<ReleaseChecksFilter> Filter;
		public PXCancel<ReleaseChecksFilter> Cancel;
		public PXAction<ReleaseChecksFilter> ViewDocument;
		[PXUIField(DisplayName = "", MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update)]
		[PXButton]
		public virtual IEnumerable viewDocument(PXAdapter adapter)
		{
			if (APPaymentList.Current != null)
			{
				PXRedirectHelper.TryRedirect(APPaymentList.Cache, APPaymentList.Current, "Document", PXRedirectHelper.WindowMode.NewWindow);
			}
			return adapter.Get();
		}

		public ToggleCurrency<ReleaseChecksFilter> CurrencyView;
		[PXFilterable]
		public PXFilteredProcessing<APPayment, ReleaseChecksFilter, Where<boolTrue, Equal<boolTrue>>, OrderBy<Desc<APPayment.selected>>> APPaymentList;

		[Obsolete("This item is not used anymore")]
		public PXSelect<CurrencyInfo> currencyinfo;
		[Obsolete("This item is not used anymore")]
		public PXSelect<CurrencyInfo, Where<CurrencyInfo.curyInfoID, Equal<Required<CurrencyInfo.curyInfoID>>>> CurrencyInfo_CuryInfoID;

		#region Actions for assign access rights
		public PXAction<ReleaseChecksFilter> Release;
		[PXProcessButton]
		[PXUIField(DisplayName = Messages.ReleaseChecks, MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update)]
		public IEnumerable release(PXAdapter a)
		{
			return a.Get();
		}
		public PXAction<ReleaseChecksFilter> Reprint;
		[PXProcessButton]
		[PXUIField(DisplayName = Messages.ReprintChecks, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		public IEnumerable reprint(PXAdapter a)
		{
			return a.Get();
		}
		public PXAction<ReleaseChecksFilter> VoidReprint;
		[PXProcessButton]
		[PXUIField(DisplayName = Messages.ReprintChecksWithNewNumber, MapEnableRights = PXCacheRights.Delete, MapViewRights = PXCacheRights.Delete)]
		public IEnumerable voidReprint(PXAdapter a)
		{
			return a.Get();
		}
		#endregion

		#region Setups
		public PXSetup<APSetup> APSetup;
		public PXSetup<Company> Company;

		public PXSetup<PaymentMethodAccount, Where<PaymentMethodAccount.cashAccountID, Equal<Optional<ReleaseChecksFilter.payAccountID>>, And<PaymentMethodAccount.paymentMethodID, Equal<Optional<ReleaseChecksFilter.payTypeID>>>>> cashaccountdetail;
		#endregion

		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[APDocType.ReleaseChecksList]
		protected virtual void APPayment_DocType_CacheAttached(PXCache sender) { }

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXUIField(DisplayName = "Branch", Visible = false)]
		[PXUIVisible(typeof(FeatureInstalled<FeaturesSet.branch>.Or<FeatureInstalled<FeaturesSet.multiCompany>>))]
		protected virtual void _(Events.CacheAttached<APPayment.branchID> e) { }

		[Obsolete("The method is obsolete after AC-80359 fix")]
		public virtual void SetPreloaded(APPrintChecks graph)
		{
			ReleaseChecksFilter filter_copy = PXCache<ReleaseChecksFilter>.CreateCopy(Filter.Current);
			filter_copy.PayAccountID = graph.Filter.Current.PayAccountID;
			filter_copy.PayTypeID = graph.Filter.Current.PayTypeID;
			filter_copy.CuryID = graph.Filter.Current.CuryID;
			Filter.Cache.Update(filter_copy);

			foreach (APPayment seldoc in graph.APPaymentList.Cache.Updated)
			{
				seldoc.Passed = true;
				APPaymentList.Cache.Update(seldoc);
				APPaymentList.Cache.SetStatus(seldoc, PXEntryStatus.Updated);
			}
			APPaymentList.Cache.IsDirty = false;

			TimeStamp = graph.TimeStamp;
		}

		public APReleaseChecks()
		{
			APSetup setup = APSetup.Current;
			PXUIFieldAttribute.SetEnabled(APPaymentList.Cache, null, false);
			PXUIFieldAttribute.SetEnabled<APPayment.selected>(APPaymentList.Cache, null, true);
			PXUIFieldAttribute.SetEnabled<APPayment.extRefNbr>(APPaymentList.Cache, null, true);

			PXUIFieldAttribute.SetVisible<APPayment.printCheck>(APPaymentList.Cache, null, false);
			PXUIFieldAttribute.SetVisibility<APPayment.printCheck>(APPaymentList.Cache, null, PXUIVisibility.Invisible);
		}

		private bool cleared;
		public override void Clear()
		{
			cleared = true;
			base.Clear();
		}

		protected virtual IEnumerable appaymentlist()
		{
			if (cleared)
			{
				foreach (APPayment doc in APPaymentList.Cache.Updated)
				{
					doc.Passed = false;
				}
			}

			IEnumerable<APPayment> checks = PXSelectJoin < APPayment,
				InnerJoinSingleTable<Vendor, On<Vendor.bAccountID, Equal<APPayment.vendorID>>>,
				Where2<Where< APPayment.released, NotEqual<True>,
					And<APPayment.hold, NotEqual<True>,
					And<APPayment.printed, Equal<True>,
					And<APPayment.docType, NotEqual<APDocType.refund>,
					And<APPayment.docType, NotEqual<APDocType.voidRefund>,
					And<APPayment.cashAccountID, Equal<Current<ReleaseChecksFilter.payAccountID>>,
					And<APPayment.paymentMethodID, Equal<Current<ReleaseChecksFilter.payTypeID>>,
					And<Match<Vendor, Current<AccessInfo.userName>>>>>>>>>>,
					And<Where<APPayment.dontApprove, Equal<True>,
						Or<APPayment.approved, Equal<True>>>>>>
				.Select(this).RowCast<APPayment>();

			return checks;
		}

		public static void ReleasePayments(List<APPayment> list, string Action)
		{
			APReleaseChecks releaseChecksGraph = CreateInstance<APReleaseChecks>();
			APPaymentEntry pe = CreateInstance<APPaymentEntry>();
			bool failed = false;
			bool successed = false;

			List<APRegister> docs = new List<APRegister>(list.Count);

			foreach (APPayment payment in list)
			{
				if (payment.Passed == true)
				{
					releaseChecksGraph.TimeStamp = pe.TimeStamp = payment.tstamp;
				}

				PXProcessing<APPayment>.SetCurrentItem(payment);

				if (Action == ReleaseChecksFilter.action.ReleaseChecks)
				{
					try
					{
						pe.Document.Current = pe.Document.Search<APPayment.refNbr>(payment.RefNbr, payment.DocType);
						if (pe.Document.Current?.ExtRefNbr != payment.ExtRefNbr)
						{
							APPayment payment_copy = PXCache<APPayment>.CreateCopy(pe.Document.Current);
							payment_copy.ExtRefNbr = payment.ExtRefNbr;
							pe.Document.Update(payment_copy);
						}

						if (PaymentRefAttribute.PaymentRefMustBeUnique(pe.paymenttype.Current) && string.IsNullOrEmpty(payment.ExtRefNbr))
						{
							throw new PXException(ErrorMessages.FieldIsEmpty, typeof(APPayment.extRefNbr).Name);
						}

						payment.IsReleaseCheckProcess = true;

						if (payment.Prebooked != true)
						{
							APPrintChecks.AssignNumbersWithNoAdditionalProcessing(pe, pe.Document.Current);
						}
						pe.Save.Press();

						if (payment.Passed == true)
						{
							pe.Document.Current.Passed = true;
						}
						docs.Add(pe.Document.Current);
						successed = true;
					}
					catch (PXException e)
					{
						PXProcessing<APPayment>.SetError(e);
						docs.Add(null);
						failed = true;
					}
				}

				if (Action == ReleaseChecksFilter.action.ReprintChecks || Action == ReleaseChecksFilter.action.VoidAndReprintChecks)				
				{
					try
					{
						payment.IsPrintingProcess = true;
						using (PXTransactionScope transactionScope = new PXTransactionScope())
						{

							#region Update CABatch if ReprintChecks
							CABatch caBatch = PXSelectJoin<CABatch,
							InnerJoin<CABatchDetail, On<CABatch.batchNbr, Equal<CABatchDetail.batchNbr>>>,
							Where<CABatchDetail.origModule, Equal<Required<APPayment.origModule>>,
								And<CABatchDetail.origDocType, Equal<Required<APPayment.docType>>,
								 And<CABatchDetail.origRefNbr, Equal<Required<APPayment.refNbr>>>>>>.
							 Select(releaseChecksGraph, payment.OrigModule, payment.DocType, payment.RefNbr);
							if (caBatch != null)
							{
								CABatchEntry be = CreateInstance<CABatchEntry>();
								be.Document.Current = caBatch;
								int DetailCount = be.Details.Select().Count; // load
								CABatchDetail detail = be.Details.Locate(new CABatchDetail()
								{
									BatchNbr = be.Document.Current.BatchNbr,
									OrigModule = payment.OrigModule,
									OrigDocType = payment.DocType,
									OrigRefNbr = payment.RefNbr,
									OrigLineNbr = CABatchDetail.origLineNbr.DefaultValue,
								});
								if (detail != null)
								{
									// payment.Status is recalculated in CABatchEntry.Delete
									if (DetailCount == 1)
									{
										be.Document.Delete(be.Document.Current); // Details will delete by PXParent
									}
									else
									{
										be.Details.Delete(detail);  // recalculated batch totals
									}
								}
								be.Save.Press();
							}
							else
							{
								PXCache cacheAPPayment = releaseChecksGraph.APPaymentList.Cache;
								// APPayment.Status is recalculated by SetStatusCheckAttribute
								APPayment.Events.Select(e=>e.CancelPrintCheck)
									.FireOn(releaseChecksGraph, payment );
								
								cacheAPPayment.PersistUpdated(payment);
								cacheAPPayment.Persisted(false);
							}
							#endregion
						// TODO: Need to rework. Use legal CRUD methods of caches!
						releaseChecksGraph.TimeStamp = PXDatabase.SelectTimeStamp();

						// delete check numbers only if Reprint (not Void and Reprint)
						PaymentMethod pm = pe.paymenttype.Select(payment.PaymentMethodID);
						if (pm.APPrintChecks == true && Action == ReleaseChecksFilter.action.ReprintChecks)
						{
							APPayment doc = payment;
							new HashSet<string>(pe.Adjustments_Raw.Select(doc.DocType, doc.RefNbr)
								.RowCast<APAdjust>()
								.Select(adj => adj.StubNbr))
									.ForEach(nbr => PaymentRefAttribute.DeleteCheck((int)doc.CashAccountID, doc.PaymentMethodID, nbr));

							// sync PaymentMethodAccount.APLastRefNbr with actual last CashAccountCheck number
							PaymentMethodAccount det = releaseChecksGraph.cashaccountdetail.SelectSingle(payment.CashAccountID, payment.PaymentMethodID);
							PaymentRefAttribute.LastCashAccountCheckSelect.Clear(releaseChecksGraph);
							CashAccountCheck cacheck = PaymentRefAttribute.LastCashAccountCheckSelect.SelectSingleBound(releaseChecksGraph, new object[] { det });
							det.APLastRefNbr = cacheck?.CheckNbr;
							releaseChecksGraph.cashaccountdetail.Cache.PersistUpdated(det);
							releaseChecksGraph.cashaccountdetail.Cache.Persisted(false);
						}
						// END TODO
						if (string.IsNullOrEmpty(payment.ExtRefNbr))
						{
							//try to get next number
							releaseChecksGraph.APPaymentList.Cache.SetDefaultExt<APPayment.extRefNbr>(payment);
						}
							transactionScope.Complete();
							PXProcessing<APPayment>.SetProcessed();
						}
					}
					catch (PXException e)
					{
						PXProcessing<APPayment>.SetError(e);
					}
					docs.Add(null);
				}
			}
			if (successed)
			{
				APDocumentRelease.ReleaseDoc(docs, true);
			}

			if (failed)
			{
				throw new PXOperationCompletedWithErrorException(GL.Messages.DocumentsNotReleased);
			}
		}

		protected virtual void ReleaseChecksFilter_PayAccountID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			Filter.Cache.SetDefaultExt<ReleaseChecksFilter.curyID>(e.Row);
			APPaymentList.Cache.Clear();
		}

		protected virtual void ReleaseChecksFilter_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			//redefault to release action when saved values are populated in filter
			ReleaseChecksFilter oldRow = (ReleaseChecksFilter)e.OldRow;
			if (oldRow.PayAccountID == null || oldRow.PayTypeID == null)
			{
				((ReleaseChecksFilter)e.Row).Action = ReleaseChecksFilter.action.ReleaseChecks;
				if (!UnattendedMode)
				{
					sender.SetDefaultExt<ReleaseChecksFilter.branchID>(e.Row);
				}
			}
		}

		protected virtual void ReleaseChecksFilter_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			PXUIFieldAttribute.SetVisible<ReleaseChecksFilter.curyID>(sender, null, PXAccess.FeatureInstalled<FeaturesSet.multicurrency>());

			ReleaseChecksFilter filter = e.Row as ReleaseChecksFilter;
			if (filter == null) return;

			PaymentMethod paymentMethod = PXSelect<PaymentMethod,
				Where<PaymentMethod.paymentMethodID, Equal<Required<ReleaseChecksFilter.payTypeID>>>>.Select(this, filter.PayTypeID);
			Reprint.SetEnabled(paymentMethod != null && paymentMethod.PrintOrExport == true);
			VoidReprint.SetEnabled(paymentMethod != null && paymentMethod.PrintOrExport == true);

			List<Tuple<string, string>> availableActions = new List<Tuple<string, PXAction>>
			{
					new Tuple<string, PXAction>(ReleaseChecksFilter.action.ReleaseChecks, Release),
					new Tuple<string, PXAction>(ReleaseChecksFilter.action.ReprintChecks, Reprint),
					new Tuple<string, PXAction>(ReleaseChecksFilter.action.VoidAndReprintChecks, VoidReprint)
			}
				.Select(t => new {ShortCut = t.Item1, State = t.Item2.GetState(filter) as PXButtonState})
				.Where(t => t.State?.Enabled == true)
				.Select(t => new Tuple<string, string>(t.ShortCut, t.State.DisplayName)).ToList();

			string[] actions = availableActions.Select(t => t.Item1).ToArray();
			PXStringListAttribute.SetLocalizable<ReleaseChecksFilter.action>(Filter.Cache, null, false);
			PXStringListAttribute.SetList<ReleaseChecksFilter.action>(Filter.Cache, null, actions, availableActions.Select(t => t.Item2).ToArray());
			PXUIFieldAttribute.SetEnabled<ReleaseChecksFilter.action>(Filter.Cache, filter, availableActions.Count > 1);

			if (availableActions.Count > 0)
			{
				if (filter.Action == null || !actions.Contains(filter.Action))
				{
					filter.Action = actions[0];
				}
			}
			else
			{
				filter.Action = null;
			}

			string action = filter.Action;
			APPaymentList.SetProcessEnabled(action != null);
			APPaymentList.SetProcessAllEnabled(action != null);
			APPaymentList.SetProcessDelegate(list => ReleasePayments(list, action));
		}

		protected virtual void ReleaseChecksFilter_PayTypeID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			sender.SetDefaultExt<ReleaseChecksFilter.payAccountID>(e.Row);
		}

		protected virtual void APPayment_RowUpdating(PXCache sender, PXRowUpdatingEventArgs e)
		{
			APPayment row = e.NewRow as APPayment;
			if (row != null && row.Selected == true)
			{
				PXFieldState state = (PXFieldState)sender.GetStateExt<APPayment.extRefNbr>(row);
				if (state != null && !string.IsNullOrEmpty(state.Error))
					row.Selected = false;
			}
		}

		protected virtual void APPayment_PrintCheck_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			if (((APPayment)e.Row).Printed != true)
			{
				((APPayment)e.Row).Selected = true;
			}
		}
	}

	[Serializable]
	public partial class ReleaseChecksFilter : IBqlTable
	{
		#region BranchID
		public abstract class branchID : PX.Data.BQL.BqlInt.Field<branchID> { }

		[PXDefault(typeof(AccessInfo.branchID))]
		[Branch(Visible = true, Enabled = true)]
		public virtual Int32? BranchID { get; set; }

		#endregion

		#region PayTypeID
		public abstract class payTypeID : PX.Data.BQL.BqlString.Field<payTypeID> { }
		protected String _PayTypeID;
		[PXDefault()]
		[PXDBString(10, IsUnicode = true)]
		[PXUIField(DisplayName = "Payment Method", Visibility = PXUIVisibility.SelectorVisible)]
		[PXSelector(typeof(Search<PaymentMethod.paymentMethodID,
						  Where<PaymentMethod.useForAP, Equal<True>>>))]		
		public virtual String PayTypeID
		{
			get
			{
				return this._PayTypeID;
			}
			set
			{
				this._PayTypeID = value;
			}
		}
		#endregion
		#region PayAccountID
		public abstract class payAccountID : PX.Data.BQL.BqlInt.Field<payAccountID> { }
		protected Int32? _PayAccountID;
		[CashAccount(typeof(branchID), typeof(Search2<CashAccount.cashAccountID,
						   InnerJoin<PaymentMethodAccount,
							   On<PaymentMethodAccount.cashAccountID, Equal<CashAccount.cashAccountID>>>,
						   Where2<Match<Current<AccessInfo.userName>>,
						   And<CashAccount.clearingAccount, Equal<False>,
						   And<PaymentMethodAccount.paymentMethodID, Equal<Current<ReleaseChecksFilter.payTypeID>>,
						   And<PaymentMethodAccount.useForAP, Equal<True>>>>>>), Visibility = PXUIVisibility.Visible)]
		[PXDefault(typeof(Search2<PaymentMethodAccount.cashAccountID,
							InnerJoin<CashAccount, On<CashAccount.cashAccountID, Equal<PaymentMethodAccount.cashAccountID>>>,
										Where<PaymentMethodAccount.paymentMethodID, Equal<Current<ReleaseChecksFilter.payTypeID>>,
											And<PaymentMethodAccount.useForAP, Equal<True>,
											And<PaymentMethodAccount.aPIsDefault, Equal<True>,
											And<CashAccount.branchID, Equal<Current<AccessInfo.branchID>>>>>>>))]
		public virtual Int32? PayAccountID
		{
			get
			{
				return this._PayAccountID;
			}
			set
			{
				this._PayAccountID = value;
			}
		}
		#endregion
		
		#region Action
		public abstract class action : PX.Data.BQL.BqlString.Field<action>
		{
			public class ListAttribute : PXStringListAttribute
			{
				public ListAttribute(): base(
					new string[] { ReleaseChecks},
					new string[] { Messages.ReleaseChecks }) {}
		}

			public const string ReleaseChecks = "R";
			public const string ReprintChecks = "D";
			public const string VoidAndReprintChecks = "V";

			public class releaseChecks : PX.Data.BQL.BqlString.Constant<releaseChecks>
			{
				public releaseChecks() : base(ReleaseChecks) {}
			}

			public class reprintChecks : PX.Data.BQL.BqlString.Constant<reprintChecks>
			{
				public reprintChecks() : base(ReprintChecks) {}
			}

			public class voidAndReprintChecks : PX.Data.BQL.BqlString.Constant<voidAndReprintChecks>
			{
				public voidAndReprintChecks() : base(VoidAndReprintChecks) {}
			}
		}
		[PXDBString(10)]
		[PXUIField(DisplayName = "Action")]
		[action.List]
		public virtual string Action { get; set; }
		#endregion
		#region CuryID
		public abstract class curyID : PX.Data.BQL.BqlString.Field<curyID> { }
		protected String _CuryID;
		[PXDBString(5, IsUnicode = true, InputMask = ">LLLLL")]
		[PXUIField(DisplayName = "Currency", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
		[PXDefault(typeof(Search<CashAccount.curyID, Where<CashAccount.cashAccountID, Equal<Current<ReleaseChecksFilter.payAccountID>>>>))]
		[PXSelector(typeof(Currency.curyID))]
		public virtual String CuryID
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
		#region CuryInfoID
		[Obsolete("This field is not used anymore")]
		public abstract class curyInfoID : PX.Data.BQL.BqlLong.Field<curyInfoID> { }
		protected Int64? _CuryInfoID;
		[PXDBLong()]
		public virtual Int64? CuryInfoID
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
		#region CashBalance
		public abstract class cashBalance : PX.Data.BQL.BqlDecimal.Field<cashBalance> { }
		protected Decimal? _CashBalance;
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXDBCury(typeof(ReleaseChecksFilter.curyID))]
		[PXUIField(DisplayName = "Available Balance", Enabled = false)]
		[CashBalance(typeof(ReleaseChecksFilter.payAccountID))]
		public virtual Decimal? CashBalance
		{
			get
			{
				return this._CashBalance;
			}
			set
			{
				this._CashBalance = value;
			}
		}
		#endregion
		#region PayFinPeriodID
		public abstract class payFinPeriodID : PX.Data.BQL.BqlString.Field<payFinPeriodID> { }
		protected string _PayFinPeriodID;
		[FinPeriodID(typeof(AccessInfo.businessDate))]
		[PXUIField(DisplayName = "Post Period", Visibility = PXUIVisibility.Visible)]
		public virtual String PayFinPeriodID
		{
			get
			{
				return this._PayFinPeriodID;
			}
			set
			{
				this._PayFinPeriodID = value;
			}
		}
		#endregion
		#region GLBalance
		public abstract class gLBalance : PX.Data.BQL.BqlDecimal.Field<gLBalance> { }
		protected Decimal? _GLBalance;

		[PXDefault(TypeCode.Decimal, "0.0")]
		//[PXDBDecimal()]
		[PXDBCury(typeof(ReleaseChecksFilter.curyID))]
		[PXUIField(DisplayName = "GL Balance", Enabled = false)]
		[GLBalance(typeof(ReleaseChecksFilter.payAccountID), typeof(ReleaseChecksFilter.payFinPeriodID))]
		public virtual Decimal? GLBalance
		{
			get
			{
				return this._GLBalance;
			}
			set
			{
				this._GLBalance = value;
			}
		}
		#endregion
	}
}
