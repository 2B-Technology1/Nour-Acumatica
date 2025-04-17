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

using V2 = PX.CCProcessingBase.Interfaces.V2;
using PX.Common;
using PX.Data;
using PX.Objects.AR.CCPaymentProcessing.Common;
using PX.Objects.AR.CCPaymentProcessing.Helpers;
using PX.Objects.AR.CCPaymentProcessing.Repositories;
using PX.Objects.AR.CCPaymentProcessing.Wrappers;
using PX.Objects.CC.PaymentProcessing.Helpers;
using PX.Objects.CA;
using PX.Objects.Extensions.PayLink;
using System;
using PX.CCProcessingBase.Interfaces.V2;
using PX.Api.Webhooks.DAC;
using PX.Objects.AR.CCPaymentProcessing;

namespace PX.Objects.CC.PaymentProcessing
{
	public class PayLinkProcessing
	{
		private ICCPaymentProcessingRepository _repository;
		private PXGraph _graph;
		public PayLinkProcessing(ICCPaymentProcessingRepository repo)
		{
			_repository = repo;
			_graph = repo.Graph;
		}

		public void SetErrorStatus(Exception ex, CCPayLink payLink)
		{
			var message = ex.Message;
			if (ex is PXOuterException outerEx)
			{
				var innerMsgs = outerEx.InnerMessages;
				if (innerMsgs.Length > 0)
				{
					message = string.Join("\r\n", outerEx.InnerMessages);
				}
			}

			payLink.ActionStatus = PayLinkActionStatus.Error;
			payLink.ErrorMessage = ShrinkMessage(message);
			SetStatusDate(payLink);
			UpdatePayLink(payLink);
			Save();
		}

		public string CreateCustomerProfileId(int? baccountID, string procCenterID)
		{
			string customerPCID = CCCustomerInformationManager.CreateCustomerProfile(_repository.Graph, baccountID, procCenterID);

			using (var scope = new PXTransactionScope())
			{
				_repository.Graph.Caches[typeof(CustomerProcessingCenterID)].Insert(
					new CustomerProcessingCenterID
					{
						BAccountID = baccountID,
						CCProcessingCenterID = procCenterID,
						CustomerCCPID = customerPCID
					});
				_repository.Graph.Persist();
				scope.Complete();
			}

			return customerPCID;
		}

		public void CreateWebhook(CCProcessingCenter procCenter, WebHook webHook)
		{
			var context = new CCProcessingContext();
			context.processingCenter = procCenter;
			context.callerGraph = _repository.Graph;
			ICardProcessingReadersProvider provider = new CardProcessingReadersProvider(context);

			var processor = CreatePluginAndGetWebhookProcessor(procCenter, provider);

			var pcWebhook = new Webhook { Url = webHook.Url };
			processor.AddWebhook(pcWebhook);
		}

		public PayLinkData GetPayments(PayLinkDocument doc, CCPayLink payLink)
		{
			var procCenter = doc.ProcessingCenterID;
			var pc = GetProcessingCenter(procCenter);
			CheckProcessingCenter(pc);

			var context = new CCProcessingContext();
			context.processingCenter = pc;
			context.callerGraph = _repository.Graph;
			ICardProcessingReadersProvider provider = new CardProcessingReadersProvider(context);

			var processor = CreatePluginAndGetPayLinkProcessor(pc, provider);

			payLink.Action = PayLinkAction.Read;
			payLink.ActionStatus = PayLinkActionStatus.Open;
			payLink.ErrorMessage = null;
			SetStatusDate(payLink);
			payLink = UpdatePayLink(payLink);
			Save();

			var payLinkParams = new PayLinkSearchParams() { ExternalId = payLink.ExternalID };
			var res = SendRequestAndHandleError(() => processor.GetLinkWithTransactions(payLinkParams),
				payLink);

			UpdatePayLinkByData(payLink, res);
			return res;
		}

		//call this method only after the successful Payments processing
		public void SetLinkStatus(CCPayLink payLink, PayLinkData res)
		{
			if (res.StatusCode == V2.PayLinkStatus.Closed)
			{
				payLink.LinkStatus = PayLinkStatus.Closed;
				payLink.NeedSync = false;
				UpdatePayLink(payLink);
				Save();
			}
		}

		public void UpdatePayLinkByData(CCPayLink payLink, PayLinkData res)
		{
			SetStatuses(payLink, res);
			if (payLink.LinkStatus == PayLinkStatus.Closed)
			{
				payLink.NeedSync = false;
			}
			UpdatePayLink(payLink);
			Save();
		}

		public void CloseLink(PayLinkDocument doc, CCPayLink payLink, PayLinkProcessingParams payLinkData)
		{
			var procCenter = doc.ProcessingCenterID;
			var pc = GetProcessingCenter(procCenter);
			CheckProcessingCenter(pc);

			var context = new CCProcessingContext();
			context.processingCenter = pc;
			context.callerGraph = _repository.Graph;

			ICardProcessingReadersProvider provider = new CardProcessingReadersProvider(context);
			var processor = CreatePluginAndGetPayLinkProcessor(pc, provider);

			payLink.Action = PayLinkAction.Close;
			payLink.ActionStatus = PayLinkActionStatus.Open;
			payLink.ErrorMessage = null;
			SetStatusDate(payLink);
			payLink = UpdatePayLink(payLink);
			Save();

			var link = SendRequestAndHandleError(() => processor.CloseLink(payLinkData), payLink);

			SetStatuses(payLink, link);
			payLink.NeedSync = false;
			UpdatePayLink(payLink);
			Save();
		}

		public void SyncLink(PayLinkDocument doc, CCPayLink payLink, PayLinkProcessingParams payLinkData)
		{
			var procCenter = doc.ProcessingCenterID;
			var pc = GetProcessingCenter(procCenter);
			CheckProcessingCenter(pc);

			var context = new CCProcessingContext();
			context.processingCenter = pc;
			context.callerGraph = _repository.Graph;

			ICardProcessingReadersProvider provider = new CardProcessingReadersProvider(context);
			var processor = CreatePluginAndGetPayLinkProcessor(pc, provider);

			if (doc.PayLinkID != null && payLink.PayLinkID == doc.PayLinkID)
			{
				payLink.Action = PayLinkAction.Update;
				payLink.ActionStatus = PayLinkActionStatus.Open;
				payLink.ErrorMessage = null;
				payLink.Amount = payLinkData.Amount;
				payLink.DeliveryMethod = doc.DeliveryMethod;
				payLink.DueDate = payLinkData.DueDate;
				SetStatusDate(payLink);

				payLink = UpdatePayLink(payLink);
				Save();

				var link = SendRequestAndHandleError(() => processor.UpdateLink(payLinkData), payLink);

				SetStatuses(payLink, link);
				payLink.NeedSync = false;
				UpdatePayLink(payLink);
				Save();
			}
		}

		public void CreateLink(PayLinkDocument doc, PayLinkProcessingParams payLinkData)
		{
			var docData = payLinkData.DocumentData;

			CCPayLink oldPayLink = null;
			if (doc.PayLinkID != null)
			{
				oldPayLink = GetPayLinkById(doc.PayLinkID);
			}

			if (oldPayLink != null)
			{
				CheckPayLinkNotProcessed(oldPayLink, payLinkData);
			}

			var context = new CCProcessingContext();
			var pc = GetProcessingCenter(doc.ProcessingCenterID);
			CheckProcessingCenter(pc);
			context.processingCenter = pc;
			context.callerGraph = _repository.Graph;
			ICardProcessingReadersProvider provider = new CardProcessingReadersProvider(context);

			var processor = CreatePluginAndGetPayLinkProcessor(pc, provider);

			CCPayLink payLink;
			if (oldPayLink != null && !PayLinkHelper.PayLinkCreated(oldPayLink))
			{
				payLink = oldPayLink;
			}
			else
			{
				payLink = new CCPayLink();
				payLink = InsertPayLink(payLink);
			}
			payLink.Action = PayLinkAction.Insert;
			payLink.Amount = payLinkData.Amount;
			payLink.DeliveryMethod = doc.DeliveryMethod;
			payLink.ProcessingCenterID = doc.ProcessingCenterID;
			payLink.CuryID = doc.CuryID;
			payLink.DueDate = payLinkData.DueDate;

			if (doc.OrderType != null)
			{
				payLink.OrderType = doc.OrderType;
				payLink.OrderNbr = doc.OrderNbr;
			}
			else
			{
				payLink.DocType = doc.DocType;
				payLink.RefNbr = doc.RefNbr;
			}

			payLink.NeedSync = false;
			payLink.ActionStatus = PayLinkActionStatus.Open;
			payLink.LinkStatus = PayLinkStatus.None;
			payLink.PaymentStatus = PayLinkPaymentStatus.None;
			payLink.ErrorMessage = null;
			SetStatusDate(payLink);
			payLink = UpdatePayLink(payLink);
			Save();
			payLinkData.LinkGuid = payLink.NoteID;

			doc.PayLinkID = payLink.PayLinkID;

			var link = SendRequestAndHandleError(() => processor.CreateLink(payLinkData), payLink);

			SetStatusDate(payLink);
			payLink.LinkStatus = PayLinkStatus.Open;
			payLink.PaymentStatus = PayLinkPaymentStatus.Unpaid;
			payLink.ActionStatus = PayLinkActionStatus.Success;
			payLink.Url = link.Url;
			payLink.ExternalID = link.Id;
			UpdatePayLink(payLink);
			Save();
		}

		protected ICCPayLinkProcessor CreatePluginAndGetPayLinkProcessor(CCProcessingCenter processingCenter, ICardProcessingReadersProvider provider)
		{
			if (processingCenter == null)
			{
				throw new PXException(AR.Messages.ERR_CCProcessingCenterNotFound);
			}

			var plugin = CreatePlugin(processingCenter);
			var processor = GetPayLinkProcessor(plugin, provider);
			return processor;
		}

		protected ICCWebhookProcessor CreatePluginAndGetWebhookProcessor(CCProcessingCenter processingCenter, ICardProcessingReadersProvider provider)
		{
			if (processingCenter == null)
			{
				throw new PXException(AR.Messages.ERR_CCProcessingCenterNotFound);
			}

			var plugin = CreatePlugin(processingCenter);
			var processor = GetWebhookProcessor(plugin, provider);
			return processor;
		}

		private object CreatePlugin(CCProcessingCenter processingCenter)
		{
			object plugin;
			try
			{
				plugin = CCPluginTypeHelper.CreatePluginInstance(processingCenter);
			}
			catch (PXException)
			{
				throw;
			}
			catch
			{
				throw new PXException(AR.Messages.ERR_ProcessingCenterTypeInstanceCreationFailed,
					processingCenter.ProcessingTypeName,
					processingCenter.ProcessingCenterID);
			}
			return plugin;
		}

		private void SetStatusDate(CCPayLink payLink)
		{
			payLink.StatusDate = PXTimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, LocaleInfo.GetTimeZone());
		}

		private void SetStatuses(CCPayLink payLink, PayLinkData payLinkData)
		{
			if (payLink.Action == PayLinkAction.Close
				&& payLinkData.StatusCode == V2.PayLinkStatus.Closed)
			{
				payLink.LinkStatus = PayLinkStatus.Closed;
			}
			if (payLinkData.PaymentStatusCode == V2.PayLinkPaymentStatus.Unpaid)
			{
				payLink.PaymentStatus = PayLinkPaymentStatus.Unpaid;
			}
			else if (payLinkData.PaymentStatusCode == V2.PayLinkPaymentStatus.Paid)
			{
				payLink.PaymentStatus = PayLinkPaymentStatus.Paid;
			}
			else if (payLinkData.PaymentStatusCode == V2.PayLinkPaymentStatus.PartiallyPaid)
			{
				payLink.PaymentStatus = PayLinkPaymentStatus.Incomplete;
			}
			payLink.ActionStatus = PayLinkActionStatus.Success;
			SetStatusDate(payLink);
		}

		private ICCPayLinkProcessor GetPayLinkProcessor(object pluginObject, ICardProcessingReadersProvider provider)
		{
			var v2Interface = CCProcessingHelper.IsV2ProcessingInterface(pluginObject);

			V2SettingsGenerator seetingsGen = new V2SettingsGenerator(provider);
			ICCPayLinkProcessor processor = v2Interface.CreateProcessor<ICCPayLinkProcessor>(seetingsGen.GetSettings());
			if (processor == null)
			{
				CreateAndThrowException(CCProcessingFeature.PayLink);
			}
			return processor;
		}

		private ICCWebhookProcessor GetWebhookProcessor(object pluginObject, ICardProcessingReadersProvider provider)
		{
			var v2Interface = CCProcessingHelper.IsV2ProcessingInterface(pluginObject);
			V2SettingsGenerator seetingsGen = new V2SettingsGenerator(provider);
			ICCWebhookProcessor processor = v2Interface.CreateProcessor<ICCWebhookProcessor>(seetingsGen.GetSettings());
			if (processor == null)
			{
				CreateAndThrowException(CCProcessingFeature.WebhookManagement);
			}
			return processor;
		}

		private void CreateAndThrowException(CCProcessingFeature feature)
		{
			string errorMessage = PXMessages.LocalizeFormatNoPrefixNLA(AR.Messages.FeatureNotSupportedByProcessing, feature);
			throw new PXException(errorMessage);
		}

		private CCProcessingCenter GetProcessingCenter(string procCenterId)
		{
			return _repository.GetProcessingCenterByID(procCenterId);
		}

		private void CheckProcessingCenter(CCProcessingCenter procCenter)
		{
			if (procCenter == null)
			{
				throw new PXArgumentException(nameof(procCenter), ErrorMessages.ArgumentNullException);
			}
			if (procCenter.IsActive == false)
			{
				throw new PXException(AR.Messages.ERR_CCProcessingIsInactive, procCenter.ProcessingCenterID);
			}
		}

		private string ShrinkMessage(string message)
		{
			const int maxMessageLength = 500;
			return message?.Length > maxMessageLength ? message.Substring(0, maxMessageLength) : message;
		}

		private void CheckPayLinkNotProcessed(CCPayLink payLink, PayLinkProcessingParams payLinkData)
		{
			if (!PayLinkHelper.PayLinkWasProcessed(payLink)
				&& PayLinkHelper.PayLinkCreated(payLink))
			{
				var docType = payLinkData.DocumentData.DocType;
				var refNbr = payLinkData.DocumentData.DocRefNbr;
				throw new PXException(Messages.DocumentHasActivePayLink, docType, refNbr);
			}
		}

		private CCPayLink InsertPayLink(CCPayLink payLink)
		{
			return _graph.Caches[typeof(CCPayLink)].Insert(payLink) as CCPayLink;
		}

		private CCPayLink UpdatePayLink(CCPayLink payLink)
		{
			return _graph.Caches[typeof(CCPayLink)].Update(payLink) as CCPayLink;
		}

		private CCPayLink GetPayLinkById(int? payLinkId)
		{
			return CCPayLink.PK.Find(_graph, payLinkId);
		}

		private void Save()
		{
			var action = _graph.Actions["Save"];
			if (action != null)
			{
				action.Press();
			}
			else
			{
				_graph.Actions.PressSave();
			}
		}

		private T SendRequestAndHandleError<T>(Func<T> func, CCPayLink payLink)
		{
			try
			{
				return func();
			}
			catch (Exception ex)
			{
				SetErrorStatus(ex, payLink);
				throw;
			}
		}
	}
}
