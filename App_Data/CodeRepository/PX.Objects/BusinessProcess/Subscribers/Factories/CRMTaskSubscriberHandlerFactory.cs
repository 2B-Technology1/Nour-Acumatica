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
using System.Linq;
using PX.BusinessProcess.DAC;
using PX.BusinessProcess.Event;
using PX.BusinessProcess.Subscribers;
using PX.BusinessProcess.Subscribers.ActionHandlers;
using PX.BusinessProcess.Subscribers.Factories;
using PX.BusinessProcess.UI;
using PX.Data;
using PX.Data.PushNotifications;
using PX.Objects.BusinessProcess.Subscribers.ActionHandlers;
using PX.SM;

namespace PX.Objects.BusinessProcess.Subscribers.Factories
{
    class CRMTaskSubscriberHandlerFactory : CreateSubscriberBase, IBPSubscriberActionHandlerFactoryWithCreateAction
    {
        private readonly IPushNotificationDefinitionProvider _pushDefinitionsProvider;
        private readonly IPXPageIndexingService _pageIndexingService;

        public CRMTaskSubscriberHandlerFactory(IPushNotificationDefinitionProvider pushDefinitionsProvider, IPXPageIndexingService pageIndexingService)
        {
            _pushDefinitionsProvider = pushDefinitionsProvider;
            _pageIndexingService = pageIndexingService;
        }

        public IEventAction CreateActionHandler(Guid handlerId, bool stopOnError, IEventDefinitionsProvider eventDefinitionsProvider)
        {
            return new CRMTaskAction(handlerId, _pushDefinitionsProvider, eventDefinitionsProvider, _pageIndexingService);
        }

        public Tuple<PXButtonDelegate, PXEventSubscriberAttribute[]> getCreateActionDelegate(BusinessProcessEventMaint maintGraph)
        {
            PXButtonDelegate handler = (PXAdapter adapter) => CreateSubscriber<TaskTemplateMaint, TaskTemplate>(maintGraph, adapter, Type);
            return Tuple.Create(handler, new PXEventSubscriberAttribute[] {new PXButtonAttribute {OnClosingPopup = PXSpecialButtonType.Refresh}});
        }

        public IEnumerable<BPHandler> GetHandlers(PXGraph graph)
        {
            return PXSelect<TaskTemplate, Where<TaskTemplate.screenID, Equal<Current<BPEvent.screenID>>, Or<Current<BPEvent.screenID>, IsNull>>>
                .Select(graph).FirstTableItems.Where(t => t != null).Select(t => new BPHandler { Id = t.NoteID, Name = t.Name, Type = TypeName });
        }

        public void RedirectToHandler(Guid? handlerId)
        {
            PXRedirectHelper.TryRedirect(CRMTaskAction.CreateGraphWithTaskTemplate(handlerId), PXRedirectHelper.WindowMode.New);
        }

        public string Type
        {
            get { return BPEventSubscriber.type.TASKType; }
        }
        public string TypeName
        {
            get { return PX.BusinessProcess.Messages.CRMTaskCreation; }
        }

        public string CreateActionName
        {
            get { return "NewCRMTask"; }
        }
        public string CreateActionLabel
        {
            get { return PX.BusinessProcess.Messages.CRMTaskCreation; }
        }
    }
}
