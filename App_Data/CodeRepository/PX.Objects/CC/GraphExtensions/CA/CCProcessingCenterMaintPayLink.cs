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

using PX.Api.Webhooks.DAC;
using PX.Api.Webhooks.Graph;
using PX.Data;
using PX.Objects.AR.CCPaymentProcessing.Common;
using PX.Objects.AR.CCPaymentProcessing.Helpers;
using PX.Objects.AR.CCPaymentProcessing.Repositories;
using PX.Objects.CS;
using PX.Objects.CA;
using PX.Objects.GL;
using System.Collections;
using System.Linq;

namespace PX.Objects.CC.GraphExtensions
{
	public class CCProcessingCenterMaintPayLink : PXGraphExtension<CCProcessingCenterMaint>, PXImportAttribute.IPXPrepareItems
	{
		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<FeaturesSet.acumaticaPayments>();
		}

		[PXImport(typeof(CCProcessingCenter))]
		public PXSelect<CCProcessingCenterBranch, Where<CCProcessingCenterBranch.processingCenterID, Equal<Current<CCProcessingCenter.processingCenterID>>>,
			OrderBy<Asc<CCProcessingCenterBranch.branchID>>> ProcCenterBranch;

		public PXSelect<WebHook, Where<WebHook.webHookID, Equal<Current<CCProcessingCenter.webhookID>>>> Webhook;

		public PXAction<CCProcessingCenter> createWebhook;
		[PXUIField(DisplayName = "Create/Update Webhook", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXProcessButton]
		public virtual IEnumerable CreateWebhook(PXAdapter adapter)
		{
			var row = Base.ProcessingCenter.Current;
			if (row == null) return adapter.Get();

			var webHook = CreateLocalWebhook();

			var copy = Base.ProcessingCenter.Cache.CreateCopy(row) as CCProcessingCenter;

			PXLongOperation.StartOperation(this, delegate ()
			{
				var procCenterGraph = PXGraph.CreateInstance<CCProcessingCenterMaint>();
				procCenterGraph.ProcessingCenter.Current = procCenterGraph.ProcessingCenter
					.Search<CCProcessingCenter.processingCenterID>(copy.ProcessingCenterID);

				var ext = procCenterGraph.GetExtension<CCProcessingCenterMaintPayLink>();
				ext.CreatePayLinkWebhook(webHook);

			});
			return adapter.Get();
		}

		public void CreatePayLinkWebhook(WebHook webhook)
		{
			var procCenter = Base.ProcessingCenter.Current;

			if (procCenter.WebhookID != null && webhook.WebHookID == procCenter.WebhookID)
			{
				var payLinkProc = GetPayLinkProcessing();
				payLinkProc.CreateWebhook(procCenter, webhook);
			}
		}

		public bool PrepareImportRow(string viewName, IDictionary keys, IDictionary values)
		{
			if (viewName == nameof(ProcCenterBranch))
			{
				ClearValueFromDescription(values, nameof(CCProcessingCenterBranch.cCPaymentMethodID));
				ClearValueFromDescription(values, nameof(CCProcessingCenterBranch.eFTPaymentMethodID));
				ClearValueFromDescription(values, nameof(CCProcessingCenterBranch.cCCashAccountID));
				ClearValueFromDescription(values, nameof(CCProcessingCenterBranch.eFTCashAccountID));
			}
			return true;
		}

		public bool RowImporting(string viewName, object row)
		{
			return true;
		}

		public bool RowImported(string viewName, object row, object oldRow)
		{
			return true;
		}

		public void PrepareItems(string viewName, IEnumerable items)
		{

		}

		protected virtual void _(Events.RowSelected<CCProcessingCenter> e)
		{
			var row = e.Row;
			var cache = e.Cache;
			if (row == null) return;

			var payLinkSupported = IsFeatureSupported(row);
			PXUIFieldAttribute.SetEnabled<CCProcessingCenter.allowPayLink>(cache, row, payLinkSupported);
			PXUIFieldAttribute.SetVisible<CCProcessingCenter.allowPayLink>(cache, row, payLinkSupported);
			PXUIFieldAttribute.SetEnabled<CCProcessingCenter.webhookID>(cache, row, false);

			createWebhook.SetEnabled(row.AllowPayLink == true && payLinkSupported);
			createWebhook.SetVisible(row.AllowPayLink == true && payLinkSupported);

			ShowMappingNotDefinedWarnIfNeeded(cache, row);
		}

		protected virtual void _(Events.FieldUpdated<CCProcessingCenter, CCProcessingCenter.processingTypeName> e)
		{
			var row = e.Row;
			if (row == null) return;

			if (row.AllowPayLink == true)
			{
				e.Cache.SetDefaultExt<CCProcessingCenter.allowPayLink>(row);
			}
		}

		protected virtual void _(Events.FieldUpdated<CCProcessingCenter, CCProcessingCenter.allowPayLink> e)
		{
			var row = e.Row;
			if (row == null) return;

			var newVal = (bool?)e.NewValue;
			var oldVal = (bool?)e.OldValue;

			if (oldVal == true && newVal == false)
			{
				foreach (var item in ProcCenterBranch.Select().RowCast<CCProcessingCenterBranch>())
				{
					if (ProcCenterBranch.Cache.GetStatus(item) == PXEntryStatus.Inserted)
					{
						ProcCenterBranch.Delete(item);
					}
				}
			}
		}

		protected virtual void _(Events.FieldUpdated<CCProcessingCenterBranch, CCProcessingCenterBranch.branchID> e)
		{
			var row = e.Row;
			if (row == null || Base.IsImportFromExcel) return;

			if (e.NewValue != null)
			{
				var lastRow = ProcCenterBranch.Cache.Inserted.RowCast<CCProcessingCenterBranch>().LastOrDefault();
				if (lastRow == null)
				{
					lastRow = ProcCenterBranch.Select().RowCast<CCProcessingCenterBranch>().LastOrDefault();
				}

				if (lastRow != null)
				{
					if (row.CCPaymentMethodID == null)
					{
						e.Cache.SetValueExt<CCProcessingCenterBranch.cCPaymentMethodID>(row, lastRow.CCPaymentMethodID);
						e.Cache.SetValueExt<CCProcessingCenterBranch.cCCashAccountID>(row, lastRow.CCCashAccountID);
					}

					if (row.EFTPaymentMethodID == null)
					{
						e.Cache.SetValueExt<CCProcessingCenterBranch.eFTPaymentMethodID>(row, lastRow.EFTPaymentMethodID);
						e.Cache.SetValueExt<CCProcessingCenterBranch.eFTCashAccountID>(row, lastRow.EFTCashAccountID);
					}
				}
			}
		}

		protected virtual void _(Events.FieldUpdated<CCProcessingCenterBranch, CCProcessingCenterBranch.cCPaymentMethodID> e)
		{
			var row = e.Row;
			if (row == null) return;

			e.Cache.SetValueExt<CCProcessingCenterBranch.cCCashAccountID>(row, null);
		}

		protected virtual void _(Events.FieldUpdated<CCProcessingCenterBranch, CCProcessingCenterBranch.eFTPaymentMethodID> e)
		{
			var row = e.Row;
			if (row == null) return;

			e.Cache.SetValueExt<CCProcessingCenterBranch.eFTCashAccountID>(row, null);
		}

		protected virtual void _(Events.FieldUpdated<CCProcessingCenter, CCProcessingCenter.cashAccountID> e)
		{
			foreach (var item in ProcCenterBranch.Select())
			{
				ProcCenterBranch.Delete(item);
			}
		}

		protected virtual void _(Events.FieldVerifying<CCProcessingCenterBranch.defaultForBranch> e)
		{
			var row = e.Row as CCProcessingCenterBranch;
			var newVal = (bool?)e.NewValue;
			if (row == null || newVal == null) return;
			if (newVal == true)
			{
				var result = GetDefaultProcCenterForBranch(row);
				if (result != null)
				{
					var branch = GetBranchById(result.BranchID);
					e.Cache.RaiseExceptionHandling(nameof(CCProcessingCenterBranch.defaultForBranch), row, true,
					new PXSetPropertyException(Messages.ProcCenterWasSelectedAsDefaultForBranch, PXErrorLevel.Error, result.ProcessingCenterID, branch.BranchCD));
				}
			}
		}

		protected virtual void _(Events.FieldVerifying<CCProcessingCenterBranch.cCCashAccountID> e)
		{
			var row = e.Row as CCProcessingCenterBranch;
			var newVal = (int?)e.NewValue;
			if (row == null || newVal == null) return;

			var procCenterCashAcc = Base.CashAccount.SelectSingle();
			if (procCenterCashAcc == null) return;

			var paymentMethodCashAcc = GetCashAccountById(newVal);
			if (procCenterCashAcc.CuryID != paymentMethodCashAcc.CuryID)
			{
				e.Cache.RaiseExceptionHandling(nameof(CCProcessingCenterBranch.cCCashAccountID), row, paymentMethodCashAcc.CashAccountCD,
					new PXSetPropertyException(CA.Messages.SpecifyCorrectCurrency, PXErrorLevel.Error, procCenterCashAcc.CuryID));
			}
		}

		protected virtual void _(Events.FieldVerifying<CCProcessingCenterBranch.eFTCashAccountID> e)
		{
			var row = e.Row;
			var newVal = (int?)e.NewValue;
			if (row == null || newVal == null) return;

			var procCenterCashAcc = Base.CashAccount.SelectSingle();
			if (procCenterCashAcc == null) return;

			var paymentMethodCashAcc = GetCashAccountById(newVal);
			if (procCenterCashAcc.CuryID != paymentMethodCashAcc.CuryID)
			{
				e.Cache.RaiseExceptionHandling(nameof(CCProcessingCenterBranch.eFTCashAccountID), row, paymentMethodCashAcc.CashAccountCD,
					new PXSetPropertyException(CA.Messages.SpecifyCorrectCurrency, PXErrorLevel.Error, procCenterCashAcc.CuryID));
			}
		}

		protected virtual void _(Events.RowPersisting<CCProcessingCenter> e)
		{
			var row = e.Row;

			if (row?.WebhookID == null) return;

			if (e.Operation == PXDBOperation.Update)
			{
				Webhook.Current = Webhook.SelectSingle();
				var originalRow = (CCProcessingCenter)e.Cache.GetOriginal(e.Row);
				if (originalRow.IsActive != row.IsActive ||
					originalRow.AllowPayLink != row.AllowPayLink)
				{
					Webhook.Current.IsActive = row.IsActive == true && row.AllowPayLink == true;
					Webhook.UpdateCurrent();
				}
			}
			else if (e.Operation == PXDBOperation.Delete)
			{
				Webhook.Current = WebHook.PK.Find(Base, row.WebhookID);
				Webhook.DeleteCurrent();
			}
		}

		protected virtual void _(Events.RowPersisting<CCProcessingCenterBranch> e)
		{
			var row = e.Row;
			if (row == null || (e.Operation != PXDBOperation.Insert && e.Operation != PXDBOperation.Update)) return;

			var cardPmSelected = row.CCPaymentMethodID != null;
			var eftPmSelected = row.EFTPaymentMethodID != null;

			if (row.BranchID != null && row.CCPaymentMethodID == null && row.EFTPaymentMethodID == null)
			{
				Branch branch = GetBranchById(row.BranchID);
				throw new PXRowPersistingException(nameof(CCProcessingCenterBranch.BranchID), branch.BranchCD,
					Messages.SpecifyPaymentMethodCashAccForBranch, branch.BranchCD.Trim());
			}

			if (row.CCCashAccountID != null || row.EFTCashAccountID != null)
			{
				var procCenterCashAcc = Base.CashAccount.SelectSingle();
				if (procCenterCashAcc != null)
				{
					var cardCashAcc = GetCashAccountById(row.CCCashAccountID);
					if (cardCashAcc != null && procCenterCashAcc.CuryID != cardCashAcc.CuryID)
					{
						throw new PXRowPersistingException(nameof(CCProcessingCenterBranch.cCCashAccountID),
							cardCashAcc.CashAccountCD, CA.Messages.SpecifyCorrectCurrency, procCenterCashAcc.CuryID);
					}

					var eftCashAcc = GetCashAccountById(row.EFTCashAccountID);
					if (eftCashAcc != null && procCenterCashAcc.CuryID != eftCashAcc.CuryID)
					{
						throw new PXRowPersistingException(nameof(CCProcessingCenterBranch.eFTCashAccountID),
							eftCashAcc.CashAccountCD, CA.Messages.SpecifyCorrectCurrency, procCenterCashAcc.CuryID);
					}
				}
			}

			if (cardPmSelected && row.CCCashAccountID == null)
			{
				throw new PXRowPersistingException(nameof(CCProcessingCenterBranch.cCCashAccountID),
					row.CCCashAccountID, ErrorMessages.FieldIsEmpty,
					PXUIFieldAttribute.GetDisplayName<CCProcessingCenterBranch.cCCashAccountID>(e.Cache));
			}

			if (eftPmSelected && row.EFTCashAccountID == null)
			{
				throw new PXRowPersistingException(nameof(CCProcessingCenterBranch.eFTCashAccountID),
					row.EFTCashAccountID, ErrorMessages.FieldIsEmpty,
					PXUIFieldAttribute.GetDisplayName<CCProcessingCenterBranch.eFTCashAccountID>(e.Cache));
			}

			if (row.DefaultForBranch == true)
			{
				var result = GetDefaultProcCenterForBranch(row);
				if (result != null)
				{
					var branch = GetBranchById(result.BranchID);
					throw new PXRowPersistingException(nameof(CCProcessingCenterBranch.defaultForBranch), true,
						Messages.ProcCenterWasSelectedAsDefaultForBranch, result.ProcessingCenterID, branch.BranchCD);
				}
			}
		}

		protected virtual WebHook CreateLocalWebhook()
		{
			var webhookGraph = PXGraph.CreateInstance<WebhookMaint>();
			var row = Base.ProcessingCenter.Current;

			WebHook ret;
			if (row.WebhookID == null)
			{
				ret = CreateLocalWebhook(webhookGraph);
			}
			else
			{
				webhookGraph.Webhook.Current = webhookGraph.Webhook.Search<WebHook.webHookID>(row.WebhookID);
				ret = webhookGraph.Webhook.Current;
				if (ret == null)
				{
					ret = CreateLocalWebhook(webhookGraph);
				}
				else if (ret.IsActive != true)
				{
					ret = ActivateLocalWebhook(webhookGraph);
				}
			}

			return ret;
		}

		protected virtual PaymentProcessing.PayLinkProcessing GetPayLinkProcessing()
		{
			var repo = new CCPaymentProcessingRepository(Base);
			return new PaymentProcessing.PayLinkProcessing(repo);
		}

		protected virtual CCProcessingCenterBranch GetDefaultProcCenterForBranch(CCProcessingCenterBranch row)
		{
			var cashAccount = Base.CashAccount.SelectSingle();
			CCProcessingCenterBranch result = PXSelectJoin<CCProcessingCenterBranch, InnerJoin<CCProcessingCenter,
				On<CCProcessingCenterBranch.processingCenterID, Equal<CCProcessingCenter.processingCenterID>>,
				InnerJoin<CashAccount, On<CashAccount.cashAccountID, Equal<CCProcessingCenter.cashAccountID>>>>,
				Where<CashAccount.curyID, Equal<Required<CashAccount.curyID>>,
					And<CCProcessingCenter.allowPayLink, Equal<True>,
					And<CCProcessingCenter.processingCenterID, NotEqual<Required<CCProcessingCenter.processingCenterID>>,
					And<CCProcessingCenterBranch.branchID, Equal<Required<CCProcessingCenterBranch.branchID>>,
					And<CCProcessingCenterBranch.defaultForBranch, Equal<True>>>>>>>.Select(Base, cashAccount.CuryID,
						row.ProcessingCenterID, row.BranchID);

			return result;
		}

		protected virtual Branch GetBranchById(int? branchId)
		{
			return Branch.PK.Find(Base, branchId);
		}

		protected virtual CashAccount GetCashAccountById(int? cashAccountId)
		{
			return CashAccount.PK.Find(Base, cashAccountId);
		}

		private void ShowMappingNotDefinedWarnIfNeeded(PXCache cache, CCProcessingCenter row)
		{
			PXSetPropertyException ex = null;
			if (row.AllowPayLink == true)
			{
				var mappingRow = ProcCenterBranch.SelectSingle();
				if (mappingRow == null)
				{
					ex = new PXSetPropertyException<CCProcessingCenter.allowPayLink>(Messages.PaymentLinkMappingNotDefined,
						PXErrorLevel.Warning);
				}
			}
			cache.RaiseExceptionHandling<CCProcessingCenter.allowPayLink>(row, row.AllowPayLink, ex);
		}

		private WebHook CreateLocalWebhook(WebhookMaint webhookGraph)
		{
			var row = Base.ProcessingCenter.Current;
			WebHook webhook = webhookGraph.Webhook.Insert(new WebHook
			{
				Name = $"PaymentProcessing - {row.ProcessingCenterID}",
				Handler = "PX.DataSync.PaymentProcessing.Webhooks.WebhookHandler",
				IsActive = true,
				IsSystem = true,
				RequestRetainCount = 100,
				RequestLogLevel = LogLevel.Failed,
			});
			webhookGraph.Save.Press();

			row.WebhookID = webhook.WebHookID;
			Base.ProcessingCenter.UpdateCurrent();
			Base.Save.Press();

			return webhook;
		}

		private bool IsFeatureSupported(CCProcessingCenter procCenter)
		{
			return CCProcessingFeatureHelper.IsFeatureSupported(procCenter, CCProcessingFeature.PayLink)
				&& CCProcessingFeatureHelper.IsFeatureSupported(procCenter, CCProcessingFeature.WebhookManagement);
		}

		private WebHook ActivateLocalWebhook(WebhookMaint webhookGraph)
		{
			webhookGraph.Webhook.Current.IsActive = true;
			var ret = webhookGraph.Webhook.UpdateCurrent();
			webhookGraph.Save.Press();

			return ret;
		}

		private void ClearValueFromDescription(IDictionary values, string key)
		{
			if (values.Contains(key))
			{
				var value = (string)values[key];
				if (value != null)
				{
					var res = value.Trim().Split(new char[] { ' ' });
					if (res.Length > 0)
					{
						values[key] = res[0].Trim();
					}
				}
			}
		}
	}
}
